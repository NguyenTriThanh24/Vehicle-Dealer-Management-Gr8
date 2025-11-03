using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Admin
{
    public class DashboardModel : AdminPageModel
    {
        private readonly IDealerService _dealerService;
        private readonly IStockService _stockService;
        private readonly IVehicleService _vehicleService;

        public DashboardModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            IDealerService dealerService,
            IStockService stockService,
            IVehicleService vehicleService)
            : base(context, authorizationService)
        {
            _dealerService = dealerService;
            _stockService = stockService;
            _vehicleService = vehicleService;
        }

        public string UserName { get; set; } = "";
        public decimal MonthSales { get; set; }
        public decimal LastMonthSales { get; set; }
        public decimal MonthSalesChangePercent { get; set; }
        public int TotalSoldVehicles { get; set; }
        public int LastWeekSoldVehicles { get; set; }
        public int WeekSoldVehiclesChange { get; set; }
        public int ActiveDealers { get; set; }
        public int TotalDealers { get; set; }
        public int TotalInventory { get; set; }
        public int LastMonthInventory { get; set; }
        public decimal InventoryChangePercent { get; set; }
        public int ActiveUsers { get; set; }

        public List<DealerPerformanceViewModel> TopDealers { get; set; } = new();
        public List<VehicleConsumptionViewModel> ConsumptionSpeed { get; set; } = new();
        public List<StockAlertViewModel> LowStockAlerts { get; set; } = new();
        public List<VehicleSalesViewModel> TopVehicleSales { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var authResult = await CheckAuthorizationAsync();
            if (authResult != null)
                return authResult;

            SetViewData();

            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            UserName = user.FullName;

            // Calculate dates for filtering
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddDays(-1);
            var lastWeekStart = now.AddDays(-7);
            var lastMonthSameDate = now.AddMonths(-1);

            // Get dealers
            var allDealers = (await _dealerService.GetAllDealersAsync()).ToList();
            TotalDealers = allDealers.Count;
            ActiveDealers = allDealers.Count(d => d.Status == "ACTIVE");

            // Calculate MonthSales from SalesDocuments (ORDER type, PAID or DELIVERED status) in current month
            var currentMonthOrders = await _context.SalesDocuments
                .Include(sd => sd.Lines)
                .Where(sd => sd.Type == "ORDER" && 
                             (sd.Status == "PAID" || sd.Status == "DELIVERED") &&
                             sd.CreatedAt >= currentMonthStart && 
                             sd.CreatedAt < now)
                .ToListAsync();

            MonthSales = currentMonthOrders
                .SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                .Sum(line => (line.UnitPrice * line.Qty) - line.DiscountValue);

            // Calculate LastMonthSales for comparison
            var lastMonthOrders = await _context.SalesDocuments
                .Include(sd => sd.Lines)
                .Where(sd => sd.Type == "ORDER" && 
                             (sd.Status == "PAID" || sd.Status == "DELIVERED") &&
                             sd.CreatedAt >= lastMonthStart && 
                             sd.CreatedAt < currentMonthStart)
                .ToListAsync();

            LastMonthSales = lastMonthOrders
                .SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                .Sum(line => (line.UnitPrice * line.Qty) - line.DiscountValue);

            MonthSalesChangePercent = LastMonthSales > 0 
                ? ((MonthSales - LastMonthSales) / LastMonthSales) * 100 
                : (MonthSales > 0 ? 100 : 0);

            // Calculate TotalSoldVehicles from SalesDocumentLines in paid/delivered orders
            var allSoldOrders = await _context.SalesDocuments
                .Include(sd => sd.Lines)
                .Where(sd => sd.Type == "ORDER" && 
                             (sd.Status == "PAID" || sd.Status == "DELIVERED"))
                .ToListAsync();

            TotalSoldVehicles = (int)allSoldOrders
                .SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                .Sum(line => (decimal)line.Qty);

            // Calculate vehicles sold in last week
            var lastWeekOrders = allSoldOrders
                .Where(sd => sd.CreatedAt >= lastWeekStart && sd.CreatedAt < now)
                .ToList();

            var lastWeekSoldVehicles = (int)lastWeekOrders
                .SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                .Sum(line => (decimal)line.Qty);

            // Calculate vehicles sold in week before last week for comparison
            var weekBeforeLastStart = lastWeekStart.AddDays(-7);
            var weekBeforeLastOrders = allSoldOrders
                .Where(sd => sd.CreatedAt >= weekBeforeLastStart && sd.CreatedAt < lastWeekStart)
                .ToList();

            var weekBeforeLastSoldVehicles = (int)weekBeforeLastOrders
                .SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                .Sum(line => (decimal)line.Qty);

            LastWeekSoldVehicles = lastWeekSoldVehicles;
            WeekSoldVehiclesChange = lastWeekSoldVehicles - weekBeforeLastSoldVehicles;

            // Calculate ActiveUsers (users created in last 30 days or with recent activity)
            var thirtyDaysAgo = now.AddDays(-30);
            ActiveUsers = await _context.Users
                .Where(u => u.CreatedAt >= thirtyDaysAgo)
                .CountAsync();

            // Get total inventory (EVM stock)
            var evmStocks = (await _stockService.GetStocksByOwnerAsync("EVM", 0)).ToList();
            TotalInventory = (int)evmStocks.Sum(s => (long)s.Qty);

            // Calculate inventory change (compare with last month)
            // Note: This would ideally track historical inventory, but for now we'll use a simple calculation
            // In a real system, you'd want to store inventory snapshots
            LastMonthInventory = TotalInventory; // Placeholder - would need historical data
            InventoryChangePercent = LastMonthInventory > 0 
                ? ((TotalInventory - LastMonthInventory) / (decimal)LastMonthInventory) * 100 
                : 0;

            // Calculate TopDealers from real sales data
            // Load data first, then calculate in memory
            var dealerSalesDataRaw = await _context.SalesDocuments
                .Include(sd => sd.Dealer)
                .Include(sd => sd.Lines)
                .Where(sd => sd.Type == "ORDER" && 
                             (sd.Status == "PAID" || sd.Status == "DELIVERED"))
                .ToListAsync();

            var dealerSalesData = dealerSalesDataRaw
                .GroupBy(sd => new { sd.DealerId, DealerName = sd.Dealer?.Name ?? "N/A" })
                .Select(g => new
                {
                    DealerId = g.Key.DealerId,
                    DealerName = g.Key.DealerName,
                    OrderCount = g.Count(),
                    Sales = g.SelectMany(sd => sd.Lines ?? Enumerable.Empty<SalesDocumentLine>())
                        .Sum(line => (line.UnitPrice * line.Qty) - line.DiscountValue)
                })
                .OrderByDescending(d => d.Sales)
                .Take(10)
                .ToList();

            // Get dealer details for region
            var dealerDict = allDealers.ToDictionary(d => d.Id, d => d);
            
            TopDealers = dealerSalesData.Select((d, index) => 
            {
                var dealer = dealerDict.ContainsKey(d.DealerId) ? dealerDict[d.DealerId] : null;
                // Mock target percent for now (would need actual targets in database)
                var targetSales = d.Sales * 1.2m; // Assume target is 20% higher
                var targetPercent = targetSales > 0 ? (int)((d.Sales / targetSales) * 100) : 0;
                return new DealerPerformanceViewModel
                {
                    Name = d.DealerName,
                    Region = dealer?.Address?.Contains("Hà Nội") == true ? "Miền Bắc" :
                             dealer?.Address?.Contains("TP.HCM") == true ? "Miền Nam" :
                             dealer?.Address?.Contains("Đà Nẵng") == true ? "Miền Trung" :
                             "N/A",
                    OrderCount = d.OrderCount,
                    Sales = d.Sales,
                    TargetPercent = targetPercent,
                    Rank = index + 1
                };
            }).ToList();

            // Calculate ConsumptionSpeed from real data
            var vehicles = await _context.Vehicles.ToListAsync();
            var vehicleSalesData = await _context.SalesDocumentLines
                .Include(sdl => sdl.SalesDocument)
                .Where(sdl => sdl.SalesDocument != null && 
                              sdl.SalesDocument.Type == "ORDER" &&
                              (sdl.SalesDocument.Status == "PAID" || sdl.SalesDocument.Status == "DELIVERED"))
                .GroupBy(sdl => sdl.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    Sold = g.Sum(sdl => (int)sdl.Qty)
                })
                .ToListAsync();

            var vehicleSales = vehicleSalesData.ToDictionary(v => v.VehicleId, v => v.Sold);

            var allStocksData = await _context.Stocks
                .Where(s => s.OwnerType == "EVM")
                .GroupBy(s => s.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    Stock = g.Sum(s => (int)s.Qty)
                })
                .ToListAsync();

            var allStocks = allStocksData.ToDictionary(s => s.VehicleId, s => s.Stock);

            ConsumptionSpeed = vehicles
                .Where(v => vehicleSales.ContainsKey(v.Id) || allStocks.ContainsKey(v.Id))
                .Select(v =>
                {
                    var sold = vehicleSales.ContainsKey(v.Id) ? vehicleSales[v.Id] : 0;
                    var stock = allStocks.ContainsKey(v.Id) ? allStocks[v.Id] : 0;
                    var speed = "SLOW";
                    if (sold > 50 || (sold > 30 && stock < 20))
                        speed = "FAST";
                    else if (sold > 20 || (sold > 10 && stock < 30))
                        speed = "MEDIUM";

                    return new VehicleConsumptionViewModel
                    {
                        Name = $"{v.ModelName} {v.VariantName}",
                        Sold = sold,
                        Stock = stock,
                        Speed = speed
                    };
                })
                .OrderByDescending(v => v.Sold)
                .Take(5)
                .ToList();

            // Get low stock alerts (real data)
            var lowStocks = evmStocks
                .Where(s => s.Qty < 10)
                .OrderBy(s => s.Qty)
                .Take(5)
                .ToList();

            LowStockAlerts = (await Task.WhenAll(lowStocks.Select(async s =>
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(s.VehicleId);
                return new StockAlertViewModel
                {
                    VehicleName = vehicle != null ? $"{vehicle.ModelName} {vehicle.VariantName}" : "N/A",
                    Color = s.ColorCode,
                    Qty = (int)s.Qty
                };
            }))).ToList();

            // Calculate TopVehicleSales for pie chart
            // Load data first, then group in memory
            var vehicleSalesLines = await _context.SalesDocumentLines
                .Include(sdl => sdl.SalesDocument)
                .Include(sdl => sdl.Vehicle)
                .Where(sdl => sdl.SalesDocument != null && 
                              sdl.SalesDocument.Type == "ORDER" &&
                              (sdl.SalesDocument.Status == "PAID" || sdl.SalesDocument.Status == "DELIVERED"))
                .ToListAsync();

            var topVehicleSalesData = vehicleSalesLines
                .GroupBy(sdl => sdl.VehicleId)
                .Select(g =>
                {
                    var firstLine = g.First();
                    var vehicleName = firstLine.Vehicle != null 
                        ? $"{firstLine.Vehicle.ModelName} {firstLine.Vehicle.VariantName}" 
                        : "N/A";
                    return new
                    {
                        VehicleId = g.Key,
                        VehicleName = vehicleName,
                        Revenue = g.Sum(sdl => (sdl.UnitPrice * sdl.Qty) - sdl.DiscountValue),
                        Quantity = g.Sum(sdl => (int)sdl.Qty)
                    };
                })
                .OrderByDescending(v => v.Revenue)
                .Take(5)
                .ToList();

            TopVehicleSales = topVehicleSalesData.Select(v => new VehicleSalesViewModel
            {
                VehicleName = v.VehicleName,
                Revenue = v.Revenue,
                Quantity = v.Quantity
            }).ToList();

            return Page();
        }

        public class DealerPerformanceViewModel
        {
            public string Name { get; set; } = "";
            public string Region { get; set; } = "";
            public int OrderCount { get; set; }
            public decimal Sales { get; set; }
            public int TargetPercent { get; set; }
            public int Rank { get; set; }
        }

        public class VehicleConsumptionViewModel
        {
            public string Name { get; set; } = "";
            public int Sold { get; set; }
            public int Stock { get; set; }
            public string Speed { get; set; } = ""; // FAST, MEDIUM, SLOW
        }

        public class StockAlertViewModel
        {
            public string VehicleName { get; set; } = "";
            public string Color { get; set; } = "";
            public int Qty { get; set; }
        }

        public class VehicleSalesViewModel
        {
            public string VehicleName { get; set; } = "";
            public decimal Revenue { get; set; }
            public int Quantity { get; set; }
        }
    }
}

