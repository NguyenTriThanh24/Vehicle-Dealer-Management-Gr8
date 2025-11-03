using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using System.Text.Json;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class CompareModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;

        public CompareModel(
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
        }

        public List<VehicleComparisonViewModel> Vehicles { get; set; } = new();
        public List<VehicleSimpleViewModel> AllVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? ids)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "CUSTOMER";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Customer";

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

                // Get price policy (customer sees MSRP, no dealerId)
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, null);

                // Parse specs
                var specs = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(vehicle.SpecJson))
                {
                    try
                    {
                        var parsedSpecs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(vehicle.SpecJson);
                        if (parsedSpecs != null)
                        {
                            foreach (var kvp in parsedSpecs)
                            {
                                specs[kvp.Key] = kvp.Value.ValueKind switch
                                {
                                    JsonValueKind.String => kvp.Value.GetString() ?? "",
                                    JsonValueKind.Number => kvp.Value.GetRawText(),
                                    JsonValueKind.True => "Có",
                                    JsonValueKind.False => "Không",
                                    _ => kvp.Value.GetRawText()
                                };
                            }
                        }
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
                    Price = pricePolicy?.Msrp ?? 0,
                    OriginalPrice = pricePolicy?.OriginalMsrp,
                    PriceNote = pricePolicy?.Note,
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
            public decimal Price { get; set; }
            public decimal? OriginalPrice { get; set; }
            public string? PriceNote { get; set; }
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

