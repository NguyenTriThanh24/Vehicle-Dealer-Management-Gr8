using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class CreateQuoteModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly ICustomerService _customerService;
        private readonly ApplicationDbContext _context; // Cần cho CustomerProfiles và Promotions

        public CreateQuoteModel(
            ISalesDocumentService salesDocumentService,
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            ICustomerService customerService,
            ApplicationDbContext context)
        {
            _salesDocumentService = salesDocumentService;
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _customerService = customerService;
            _context = context;
        }

        public List<CustomerViewModel> Customers { get; set; } = new();
        public List<VehicleViewModel> Vehicles { get; set; } = new();
        public List<PromotionViewModel> Promotions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Load customers (all for now, can be filtered later)
            Customers = await _context.CustomerProfiles
                .Select(c => new CustomerViewModel
                {
                    Id = c.Id,
                    Name = c.FullName,
                    Phone = c.Phone
                })
                .ToListAsync();

            // Load available vehicles
            var vehicles = await _vehicleService.GetAvailableVehiclesAsync();

            var dealerIdInt = int.Parse(dealerId);

            foreach (var vehicle in vehicles)
            {
                // Get price policy
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, dealerIdInt);

                Vehicles.Add(new VehicleViewModel
                {
                    Id = vehicle.Id,
                    Name = vehicle.ModelName,
                    Variant = vehicle.VariantName,
                    Msrp = pricePolicy?.Msrp ?? 0
                });
            }

            // Load active promotions
            Promotions = await _context.Promotions
                .Where(p => p.ValidFrom <= DateTime.UtcNow &&
                            (p.ValidTo == null || p.ValidTo >= DateTime.UtcNow))
                .Select(p => new PromotionViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int customerId, int vehicleId, string color, int quantity, string action, int? promotionId, decimal additionalDiscount, string? note)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            var userId = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(dealerId) || string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);
            var userIdInt = int.Parse(userId);

            // Create sales document (QUOTE) using Service
            var salesDocument = await _salesDocumentService.CreateQuoteAsync(
                dealerIdInt, 
                customerId, 
                userIdInt, 
                promotionId);

            // Update status if sending
            if (action == "send")
            {
                await _salesDocumentService.UpdateSalesDocumentStatusAsync(salesDocument.Id, "SENT");
            }

            // Get price policy
            var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicleId, dealerIdInt);

            var unitPrice = pricePolicy?.Msrp ?? 0;

            // Create line item
            var lineItem = new SalesDocumentLine
            {
                SalesDocumentId = salesDocument.Id,
                VehicleId = vehicleId,
                ColorCode = color,
                Qty = quantity,
                UnitPrice = unitPrice,
                DiscountValue = additionalDiscount
            };

            _context.SalesDocumentLines.Add(lineItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dealer/Sales/Quotes");
        }

        public class CustomerViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Phone { get; set; } = "";
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Variant { get; set; } = "";
            public decimal Msrp { get; set; }
        }

        public class PromotionViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}

