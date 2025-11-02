using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.EVM
{
    public class PricePoliciesModel : PageModel
    {
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IVehicleService _vehicleService;
        private readonly IDealerService _dealerService;
        private readonly ApplicationDbContext _context; // Tạm thời cần cho query phức tạp

        public PricePoliciesModel(
            IPricePolicyService pricePolicyService,
            IVehicleService vehicleService,
            IDealerService dealerService,
            ApplicationDbContext context)
        {
            _pricePolicyService = pricePolicyService;
            _vehicleService = vehicleService;
            _dealerService = dealerService;
            _context = context;
        }

        public List<string> AllVehicles { get; set; } = new();
        public List<string> AllDealers { get; set; } = new();
        public List<VehicleSimple> Vehicles { get; set; } = new();
        public List<PricePolicyViewModel> PricePolicies { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get all vehicles for filter
            var allVehiclesList = await _vehicleService.GetAllVehiclesAsync();
            AllVehicles = allVehiclesList.Select(v => v.ModelName + " " + v.VariantName).ToList();

            // Get all dealers for filter
            var allDealersList = await _dealerService.GetAllDealersAsync();
            AllDealers = allDealersList.Select(d => d.Name).ToList();

            // Get vehicles for create form
            Vehicles = allVehiclesList.Select(v => new VehicleSimple
            {
                Id = v.Id,
                Name = v.ModelName + " " + v.VariantName
            }).ToList();

            // Get all price policies (cần include để map ViewModel)
            var policies = await _context.PricePolicies
                .Include(p => p.Vehicle)
                .Include(p => p.Dealer)
                .OrderByDescending(p => p.ValidFrom)
                .ToListAsync();

            PricePolicies = policies.Select(p => new PricePolicyViewModel
            {
                Id = p.Id,
                VehicleName = $"{p.Vehicle?.ModelName} {p.Vehicle?.VariantName}",
                DealerName = p.Dealer?.Name ?? "",
                Msrp = p.Msrp,
                WholesalePrice = p.WholesalePrice ?? 0,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int vehicleId, decimal msrp, decimal wholesalePrice)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var pricePolicy = new PricePolicy
                {
                    VehicleId = vehicleId,
                    DealerId = null, // Global
                    Msrp = msrp,
                    WholesalePrice = wholesalePrice,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = null
                };

                await _pricePolicyService.CreatePricePolicyAsync(pricePolicy);

                TempData["Success"] = "Tạo chính sách giá thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePolicyAsync(int policyId, decimal msrp, decimal wholesalePrice, string validFrom, string? validTo)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var policy = await _pricePolicyService.GetPricePolicyByIdAsync(policyId);
                if (policy == null)
                {
                    TempData["Error"] = "Không tìm thấy chính sách giá này.";
                    return RedirectToPage();
                }

                // Parse dates
                if (!DateTime.TryParse(validFrom, out var validFromDate))
                {
                    TempData["Error"] = "Ngày bắt đầu không hợp lệ.";
                    return RedirectToPage();
                }

                DateTime? validToDate = null;
                if (!string.IsNullOrWhiteSpace(validTo) && DateTime.TryParse(validTo, out var parsedDate))
                {
                    validToDate = parsedDate;
                }

                // Update policy
                policy.Msrp = msrp;
                policy.WholesalePrice = wholesalePrice;
                policy.ValidFrom = validFromDate;
                policy.ValidTo = validToDate;

                await _pricePolicyService.UpdatePricePolicyAsync(policy);

                TempData["Success"] = "Cập nhật chính sách giá thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePolicyAsync(int policyId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                await _pricePolicyService.DeletePricePolicyAsync(policyId);
                TempData["Success"] = "Xóa chính sách giá thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public class VehicleSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class PricePolicyViewModel
        {
            public int Id { get; set; }
            public string VehicleName { get; set; } = "";
            public string DealerName { get; set; } = "";
            public decimal Msrp { get; set; }
            public decimal WholesalePrice { get; set; }
            public DateTime ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
        }
    }
}

