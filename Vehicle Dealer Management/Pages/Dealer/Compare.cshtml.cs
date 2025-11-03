using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using System.Text.Json;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class CompareModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IStockService _stockService;

        public CompareModel(
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            IStockService stockService)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _stockService = stockService;
        }

        public List<VehicleComparisonViewModel> Vehicles { get; set; } = new();
        public List<VehicleSimpleViewModel> AllVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? ids)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var dealerIdInt = int.Parse(dealerId);

            // Get all available vehicles for selection
            var allVehicles = await _vehicleService.GetAvailableVehiclesAsync();
            AllVehicles = allVehicles.Select(v => new VehicleSimpleViewModel
            {
                Id = v.Id,
                Name = $"{v.ModelName} {v.VariantName}",
                ImageUrl = v.ImageUrl ?? ""
            }).ToList();

            // Parse vehicle IDs from query string
            var vehicleIds = new List<int>();
            if (!string.IsNullOrEmpty(ids))
            {
                var idArray = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in idArray)
                {
                    if (int.TryParse(idStr, out var id))
                    {
                        vehicleIds.Add(id);
                    }
                }
            }

            // Limit to 4 vehicles for comparison
            vehicleIds = vehicleIds.Take(4).ToList();

            // Load vehicle details
            foreach (var vehicleId in vehicleIds)
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null) continue;

                // Get price policy
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, dealerIdInt);

                // Get stock
                var stocks = await _stockService.GetAvailableStocksByVehicleIdAsync(vehicle.Id, "EVM");
                var totalStock = stocks.Sum(s => (int)s.Qty);

                // Parse specs
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

                Vehicles.Add(new VehicleComparisonViewModel
                {
                    Id = vehicle.Id,
                    ModelName = vehicle.ModelName,
                    VariantName = vehicle.VariantName,
                    ImageUrl = vehicle.ImageUrl,
                    Status = vehicle.Status,
                    Msrp = pricePolicy?.Msrp ?? 0,
                    WholesalePrice = pricePolicy?.WholesalePrice ?? 0,
                    TotalStock = totalStock,
                    Specs = specs
                });
            }

            return Page();
        }

        public class VehicleComparisonViewModel
        {
            public int Id { get; set; }
            public string ModelName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal Msrp { get; set; }
            public decimal WholesalePrice { get; set; }
            public int TotalStock { get; set; }
            public Dictionary<string, string> Specs { get; set; } = new();
        }

        public class VehicleSimpleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string ImageUrl { get; set; } = "";
        }
    }
}

