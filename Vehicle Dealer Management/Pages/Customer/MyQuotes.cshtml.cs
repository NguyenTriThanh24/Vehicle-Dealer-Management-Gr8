using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class MyQuotesModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly ApplicationDbContext _context; // Cần cho CustomerProfile

        public MyQuotesModel(
            ISalesDocumentService salesDocumentService,
            ApplicationDbContext context)
        {
            _salesDocumentService = salesDocumentService;
            _context = context;
        }

        public List<QuoteViewModel> Quotes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get customer profile
            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

            if (customerProfile == null)
            {
                return Page();
            }

            // Get quotes using Service
            var quotes = await _salesDocumentService.GetSalesDocumentsByCustomerIdAsync(customerProfile.Id, "QUOTE");

            Quotes = quotes.Select(q => new QuoteViewModel
            {
                Id = q.Id,
                DealerName = q.Dealer?.Name ?? "N/A",
                CreatedAt = q.CreatedAt,
                VehicleCount = (int)(q.Lines?.Sum(l => (decimal?)l.Qty) ?? 0),
                TotalAmount = q.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0,
                Status = q.Status
            }).ToList();

            return Page();
        }

        public class QuoteViewModel
        {
            public int Id { get; set; }
            public string DealerName { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public int VehicleCount { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; } = "";
        }
    }
}

