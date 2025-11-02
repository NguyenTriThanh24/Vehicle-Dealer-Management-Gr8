using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.EVM
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IVehicleService _vehicleService;
        private readonly IStockService _stockService;
        private readonly IDealerService _dealerService;

        public DashboardModel(
            ApplicationDbContext context,
            IVehicleService vehicleService,
            IStockService stockService,
            IDealerService dealerService)
        {
            _context = context;
            _vehicleService = vehicleService;
            _stockService = stockService;
            _dealerService = dealerService;
        }

        public string UserName { get; set; } = "";
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int TotalStock { get; set; }
        public int TotalDealers { get; set; }
        public int ActiveDealers { get; set; }
        public int PendingDealerOrders { get; set; }

        public List<DealerOrderViewModel> DealerOrders { get; set; } = new();
        public List<StockViewModel> StockSummary { get; set; } = new();
        public List<DealerViewModel> ActiveDealersList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            UserName = user.FullName;

            // Get vehicles
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            TotalVehicles = allVehicles.Count();
            AvailableVehicles = allVehicles.Count(v => v.Status == "AVAILABLE");

            // Get EVM stock
            var evmStocks = (await _stockService.GetStocksByOwnerAsync("EVM", 0)).ToList();

            TotalStock = (int)evmStocks.Sum(s => (long)s.Qty);

            StockSummary = (await Task.WhenAll(evmStocks
                .OrderByDescending(s => s.Qty)
                .Take(10)
                .Select(async s =>
                {
                    var vehicle = await _vehicleService.GetVehicleByIdAsync(s.VehicleId);
                    return new StockViewModel
                    {
                        VehicleName = vehicle != null ? $"{vehicle.ModelName} {vehicle.VariantName}" : "N/A",
                        Color = s.ColorCode,
                        Qty = (int)s.Qty
                    };
                }))).ToList();

            // Get dealers
            var allDealers = (await _dealerService.GetAllDealersAsync()).ToList();
            TotalDealers = allDealers.Count;
            ActiveDealers = allDealers.Count(d => d.Status == "ACTIVE");

            var activeDealers = (await _dealerService.GetActiveDealersAsync())
                .Take(5)
                .ToList();

            ActiveDealersList = activeDealers.Select(d => new DealerViewModel
            {
                Name = d.Name,
                Address = d.Address,
                Status = d.Status
            }).ToList();

            // Get pending dealer orders
            var pendingOrders = await _context.DealerOrders
                .Where(o => o.Status == "SUBMITTED")
                .Include(o => o.Dealer)
                .OrderBy(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            PendingDealerOrders = pendingOrders.Count;

            DealerOrders = pendingOrders.Select(o => new DealerOrderViewModel
            {
                Id = o.Id,
                DealerName = o.Dealer?.Name ?? "N/A",
                CreatedAt = o.CreatedAt,
                VehicleCount = 3, // Mock - parse from ItemsJson
                Status = o.Status
            }).ToList();

            return Page();
        }

        public class DealerOrderViewModel
        {
            public int Id { get; set; }
            public string DealerName { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public int VehicleCount { get; set; }
            public string Status { get; set; } = "";
        }

        public class StockViewModel
        {
            public string VehicleName { get; set; } = "";
            public string Color { get; set; } = "";
            public int Qty { get; set; }
        }

        public class DealerViewModel
        {
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Status { get; set; } = "";
        }
    }
}

