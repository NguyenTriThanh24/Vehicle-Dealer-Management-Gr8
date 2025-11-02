using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class RequestQuoteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IDealerService _dealerService;
        private readonly ISalesDocumentService _salesDocumentService;

        public RequestQuoteModel(
            ApplicationDbContext context,
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            IDealerService dealerService,
            ISalesDocumentService salesDocumentService)
        {
            _context = context;
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _dealerService = dealerService;
            _salesDocumentService = salesDocumentService;
        }

        public VehicleViewModel? Vehicle { get; set; }
        public List<DealerViewModel> Dealers { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? vehicleId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!vehicleId.HasValue)
            {
                ErrorMessage = "Vui lòng chọn mẫu xe.";
                return RedirectToPage("/Customer/Vehicles");
            }

            // Get vehicle
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId.Value);
            if (vehicle == null || vehicle.Status != "AVAILABLE")
            {
                ErrorMessage = "Mẫu xe không tồn tại hoặc không có sẵn.";
                return RedirectToPage("/Customer/Vehicles");
            }

            // Get price policy
            var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, null);

            Vehicle = new VehicleViewModel
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                VariantName = vehicle.VariantName,
                ImageUrl = vehicle.ImageUrl,
                Price = pricePolicy?.Msrp ?? 0
            };

            // Get active dealers
            var dealers = await _dealerService.GetActiveDealersAsync();
            Dealers = dealers.Select(d => new DealerViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Address = d.Address,
                Phone = d.PhoneNumber ?? ""
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int vehicleId, int dealerId, string? color = null, int quantity = 1, string? note = null)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userIdInt = int.Parse(userId);

            // Get customer profile
            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customerProfile == null)
            {
                ErrorMessage = "Vui lòng cập nhật thông tin cá nhân trước khi yêu cầu báo giá.";
                return RedirectToPage("/Auth/Profile");
            }

            // Validate dealer
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            if (dealer == null || dealer.Status != "ACTIVE")
            {
                ErrorMessage = "Đại lý không tồn tại hoặc không hoạt động.";
                return await OnGetAsync(vehicleId);
            }

            // Validate vehicle
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null || vehicle.Status != "AVAILABLE")
            {
                ErrorMessage = "Mẫu xe không tồn tại hoặc không có sẵn.";
                return await OnGetAsync(vehicleId);
            }

            try
            {
                // Note: SalesDocument.CustomerId references CustomerProfile
                // But SalesDocumentService.CreateQuoteAsync uses CustomerRepository which works with Customer model
                // We need to work around this - create quote directly in database

                // Create quote directly in database (bypass service for now)
                var quote = new SalesDocument
                {
                    Type = "QUOTE",
                    DealerId = dealerId,
                    CustomerId = customerProfile.Id,
                    Status = "DRAFT",
                    PromotionId = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userIdInt
                };

                _context.SalesDocuments.Add(quote);
                await _context.SaveChangesAsync();

                // Get price policy for this dealer
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicleId, dealerId);
                var unitPrice = pricePolicy?.Msrp ?? 0;

                // Add line item
                var lineItem = new SalesDocumentLine
                {
                    SalesDocumentId = quote.Id,
                    VehicleId = vehicleId,
                    ColorCode = color ?? "STANDARD",
                    Qty = quantity,
                    UnitPrice = unitPrice,
                    DiscountValue = 0
                };

                _context.SalesDocumentLines.Add(lineItem);
                await _context.SaveChangesAsync();

                SuccessMessage = "Yêu cầu báo giá đã được gửi thành công! Đại lý sẽ liên hệ với bạn sớm nhất.";
                return RedirectToPage("/Customer/MyQuotes");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra khi tạo yêu cầu báo giá. Vui lòng thử lại.";
                return await OnGetAsync(vehicleId);
            }
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string ModelName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public decimal Price { get; set; }
        }

        public class DealerViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
        }
    }
}

