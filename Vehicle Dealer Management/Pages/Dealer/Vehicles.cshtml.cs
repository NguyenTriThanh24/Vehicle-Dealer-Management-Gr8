using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class VehiclesModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IStockService _stockService;

        public VehiclesModel(
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            IStockService stockService)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _stockService = stockService;
        }

        public List<VehicleViewModel> Vehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get all vehicles with price policies and stocks
            var vehicles = await _vehicleService.GetAvailableVehiclesAsync();

            var dealerIdInt = int.Parse(dealerId);

            foreach (var vehicle in vehicles)
            {
                // Get price policy (dealer-specific or global)
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, dealerIdInt);

                // Get stock colors available at EVM (dealer can order from EVM)
                var stocks = await _stockService.GetAvailableStocksByVehicleIdAsync(vehicle.Id, "EVM");

                Vehicles.Add(new VehicleViewModel
                {
                    Id = vehicle.Id,
                    Name = vehicle.ModelName,
                    Variant = vehicle.VariantName,
                    ImageUrl = vehicle.ImageUrl,
                    Status = vehicle.Status,
                    Msrp = pricePolicy?.Msrp ?? 0,
                    WholesalePrice = pricePolicy?.WholesalePrice ?? 0,
                    AvailableColors = stocks.Select(s => new ColorStock
                    {
                        Color = s.ColorCode,
                        Qty = (int)s.Qty
                    }).ToList()
                });
            }

            return Page();
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Variant { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal Msrp { get; set; }
            public decimal WholesalePrice { get; set; }
            public List<ColorStock> AvailableColors { get; set; } = new();
        }

        public class ColorStock
        {
            public string Color { get; set; } = "";
            public int Qty { get; set; }
        }
    }
}

