using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Admin.Reports
{
    public class SalesByVehicleModel : AdminPageModel
    {
        public SalesByVehicleModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService)
            : base(context, authorizationService)
        {
        }

        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string TopVehicle { get; set; } = "";
        public decimal AvgPrice { get; set; }

        public List<VehicleReportViewModel> VehicleReports { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var authResult = await CheckAuthorizationAsync();
            if (authResult != null)
                return authResult;

            SetViewData();

            // Get sales data from SalesDocumentLines for paid/delivered orders
            var vehicleSalesData = await _context.SalesDocumentLines
                .Include(sdl => sdl.SalesDocument)
                .Include(sdl => sdl.Vehicle)
                .Where(sdl => sdl.SalesDocument != null && 
                              sdl.SalesDocument.Type == "ORDER" &&
                              (sdl.SalesDocument.Status == "PAID" || sdl.SalesDocument.Status == "DELIVERED"))
                .GroupBy(sdl => sdl.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    QuantitySold = g.Sum(sdl => (int)sdl.Qty),
                    Revenue = g.Sum(sdl => (sdl.UnitPrice * sdl.Qty) - sdl.DiscountValue),
                    FirstLine = g.First()
                })
                .ToListAsync();

            // Get vehicle details and build report
            VehicleReports = new List<VehicleReportViewModel>();
            
            foreach (var saleData in vehicleSalesData.OrderByDescending(s => s.Revenue))
            {
                var vehicle = saleData.FirstLine.Vehicle;
                if (vehicle == null) continue;

                // Determine sales speed based on quantity sold
                var speed = "SLOW";
                if (saleData.QuantitySold > 50)
                    speed = "FAST";
                else if (saleData.QuantitySold > 20)
                    speed = "MEDIUM";

                var avgPrice = saleData.QuantitySold > 0 
                    ? saleData.Revenue / saleData.QuantitySold 
                    : 0;

                VehicleReports.Add(new VehicleReportViewModel
                {
                    ModelName = vehicle.ModelName,
                    VariantName = vehicle.VariantName,
                    ImageUrl = vehicle.ImageUrl ?? "",
                    QuantitySold = saleData.QuantitySold,
                    Revenue = saleData.Revenue,
                    AvgPrice = avgPrice,
                    Speed = speed
                });
            }

            // Calculate totals
            TotalSold = VehicleReports.Sum(v => v.QuantitySold);
            TotalRevenue = VehicleReports.Sum(v => v.Revenue);
            TopVehicle = VehicleReports.OrderByDescending(v => v.QuantitySold).FirstOrDefault()?.ModelName ?? "N/A";
            AvgPrice = TotalSold > 0 ? TotalRevenue / TotalSold : 0;

            return Page();
        }

        public class VehicleReportViewModel
        {
            public string ModelName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public int QuantitySold { get; set; }
            public decimal Revenue { get; set; }
            public decimal AvgPrice { get; set; }
            public string Speed { get; set; } = ""; // FAST, MEDIUM, SLOW
        }
    }
}

