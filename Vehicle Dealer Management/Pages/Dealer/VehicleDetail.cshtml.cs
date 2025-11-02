using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using System.Text.Json;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class VehicleDetailModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IStockService _stockService;

        public VehicleDetailModel(
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            IStockService stockService)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _stockService = stockService;
        }

        public VehicleDetailViewModel Vehicle { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var dealerIdInt = int.Parse(dealerId);

            // Get vehicle from Service
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            // Get price policy (dealer-specific or global)
            var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, dealerIdInt);

            // Get stock availability (EVM stock - dealer can order from EVM)
            var stocks = await _stockService.GetAvailableStocksByVehicleIdAsync(vehicle.Id, "EVM");

            // Get dealer stock (if any)
            var dealerStocks = await _stockService.GetAvailableStocksByVehicleIdAsync(vehicle.Id, "DEALER");
            dealerStocks = dealerStocks.Where(s => s.OwnerId == dealerIdInt).ToList();

            // Parse specs from JSON
            var specs = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(vehicle.SpecJson))
            {
                try
                {
                    specs = JsonSerializer.Deserialize<Dictionary<string, string>>(vehicle.SpecJson) ?? new Dictionary<string, string>();
                }
                catch
                {
                    specs = new Dictionary<string, string>();
                }
            }

            Vehicle = new VehicleDetailViewModel
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                VariantName = vehicle.VariantName,
                ImageUrl = vehicle.ImageUrl,
                Status = vehicle.Status,
                Specs = specs,
                Msrp = pricePolicy?.Msrp ?? 0,
                WholesalePrice = pricePolicy?.WholesalePrice ?? 0,
                AvailableColors = stocks.Select(s => new StockColorViewModel
                {
                    ColorCode = s.ColorCode,
                    Qty = (int)s.Qty,
                    OwnerType = "EVM"
                }).ToList(),
                DealerStocks = dealerStocks.Select(s => new StockColorViewModel
                {
                    ColorCode = s.ColorCode,
                    Qty = (int)s.Qty,
                    OwnerType = "DEALER"
                }).ToList()
            };

            return Page();
        }

        public class VehicleDetailViewModel
        {
            public int Id { get; set; }
            public string ModelName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Status { get; set; } = "";
            public Dictionary<string, string> Specs { get; set; } = new();
            public decimal Msrp { get; set; }
            public decimal WholesalePrice { get; set; }
            public List<StockColorViewModel> AvailableColors { get; set; } = new();
            public List<StockColorViewModel> DealerStocks { get; set; } = new();
        }

        public class StockColorViewModel
        {
            public string ColorCode { get; set; } = "";
            public int Qty { get; set; }
            public string OwnerType { get; set; } = "";
        }
    }
}

