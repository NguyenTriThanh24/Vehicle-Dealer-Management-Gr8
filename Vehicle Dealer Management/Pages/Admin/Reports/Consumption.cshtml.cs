using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Admin.Reports
{
    public class ConsumptionModel : AdminPageModel
    {
        public ConsumptionModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService)
            : base(context, authorizationService)
        {
        }

        public decimal AvgConsumptionRate { get; set; }
        public int FastMovingCount { get; set; }
        public int SlowMovingCount { get; set; }
        public int AvgDaysToEmpty { get; set; }

        public List<ConsumptionDataViewModel> ConsumptionData { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var authResult = await CheckAuthorizationAsync();
            if (authResult != null)
                return authResult;

            SetViewData();

            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var sevenDaysAgo = now.AddDays(-7);

            // Get all vehicles
            var vehicles = await _context.Vehicles.ToListAsync();

            // Get current stocks (EVM stock only)
            var evmStocksData = await _context.Stocks
                .Where(s => s.OwnerType == "EVM")
                .GroupBy(s => s.VehicleId)
                .Select(g => new { VehicleId = g.Key, Stock = g.Sum(s => (int)s.Qty) })
                .ToListAsync();

            var evmStocks = evmStocksData.ToDictionary(s => s.VehicleId, s => s.Stock);

            // Get sales in last 30 days
            var salesLast30Days = await _context.SalesDocumentLines
                .Include(sdl => sdl.SalesDocument)
                .Include(sdl => sdl.Vehicle)
                .Where(sdl => sdl.SalesDocument != null &&
                              sdl.SalesDocument.Type == "ORDER" &&
                              (sdl.SalesDocument.Status == "PAID" || sdl.SalesDocument.Status == "DELIVERED") &&
                              sdl.SalesDocument.CreatedAt >= thirtyDaysAgo &&
                              sdl.SalesDocument.CreatedAt < now)
                .GroupBy(sdl => sdl.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    SoldLast30Days = g.Sum(sdl => (int)sdl.Qty)
                })
                .ToListAsync();

            // Get sales in last 7 days for weekly rate
            var salesLast7Days = await _context.SalesDocumentLines
                .Include(sdl => sdl.SalesDocument)
                .Where(sdl => sdl.SalesDocument != null &&
                              sdl.SalesDocument.Type == "ORDER" &&
                              (sdl.SalesDocument.Status == "PAID" || sdl.SalesDocument.Status == "DELIVERED") &&
                              sdl.SalesDocument.CreatedAt >= sevenDaysAgo &&
                              sdl.SalesDocument.CreatedAt < now)
                .GroupBy(sdl => sdl.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    SoldLast7Days = g.Sum(sdl => (int)sdl.Qty)
                })
                .ToListAsync();

            var sales30DaysDict = salesLast30Days.ToDictionary(s => s.VehicleId, s => s.SoldLast30Days);
            var sales7DaysDict = salesLast7Days.ToDictionary(s => s.VehicleId, s => s.SoldLast7Days);

            // Build consumption data
            ConsumptionData = new List<ConsumptionDataViewModel>();

            foreach (var vehicle in vehicles)
            {
                var sold30Days = sales30DaysDict.ContainsKey(vehicle.Id) ? sales30DaysDict[vehicle.Id] : 0;
                var sold7Days = sales7DaysDict.ContainsKey(vehicle.Id) ? sales7DaysDict[vehicle.Id] : 0;
                var currentStock = evmStocks.ContainsKey(vehicle.Id) ? evmStocks[vehicle.Id] : 0;

                // Calculate weekly rate (from last 7 days)
                var weeklyRate = (decimal)sold7Days;

                // Calculate days to empty (if weekly rate > 0)
                int? daysToEmpty = null;
                if (weeklyRate > 0 && currentStock > 0)
                {
                    daysToEmpty = (int)Math.Ceiling((currentStock / weeklyRate) * 7);
                }

                // Determine speed category
                var speed = "SLOW";
                if (weeklyRate > 10 || (sold30Days > 40 && currentStock < 20))
                    speed = "FAST";
                else if (weeklyRate > 5 || (sold30Days > 20 && currentStock < 30))
                    speed = "MEDIUM";

                // Only include vehicles that have sales or stock
                if (sold30Days > 0 || currentStock > 0)
                {
                    ConsumptionData.Add(new ConsumptionDataViewModel
                    {
                        VehicleName = $"{vehicle.ModelName} {vehicle.VariantName}",
                        SoldLast30Days = sold30Days,
                        CurrentStock = currentStock,
                        WeeklyRate = weeklyRate,
                        DaysToEmpty = daysToEmpty,
                        Speed = speed
                    });
                }
            }

            // Order by weekly rate descending
            ConsumptionData = ConsumptionData.OrderByDescending(c => c.WeeklyRate).ToList();

            // Calculate summary
            if (ConsumptionData.Any())
            {
                AvgConsumptionRate = ConsumptionData.Average(c => c.WeeklyRate);
                FastMovingCount = ConsumptionData.Count(c => c.Speed == "FAST");
                SlowMovingCount = ConsumptionData.Count(c => c.Speed == "SLOW");
                
                var daysToEmptyValues = ConsumptionData.Where(c => c.DaysToEmpty.HasValue).Select(c => c.DaysToEmpty!.Value).ToList();
                if (daysToEmptyValues.Any())
                {
                    AvgDaysToEmpty = (int)daysToEmptyValues.Average();
                }
            }

            return Page();
        }

        public class ConsumptionDataViewModel
        {
            public string VehicleName { get; set; } = "";
            public int SoldLast30Days { get; set; }
            public int CurrentStock { get; set; }
            public decimal WeeklyRate { get; set; }
            public int? DaysToEmpty { get; set; }
            public string Speed { get; set; } = ""; // FAST, MEDIUM, SLOW
        }
    }
}

