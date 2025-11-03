using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class QuotesModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;

        public QuotesModel(ISalesDocumentService salesDocumentService)
        {
            _salesDocumentService = salesDocumentService;
        }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public List<QuoteViewModel> Quotes { get; set; } = new();
        public int TotalCount { get; set; }
        public int SentCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }

        public async Task<IActionResult> OnGetAsync(string? status)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            StatusFilter = status ?? "all";
            var dealerIdInt = int.Parse(dealerId);

            // Get all quotes for counting
            var allQuotes = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(
                dealerIdInt, 
                type: "QUOTE", 
                status: null);

            TotalCount = allQuotes.Count();
            SentCount = allQuotes.Count(q => q.Status == "SENT");
            AcceptedCount = allQuotes.Count(q => q.Status == "ACCEPTED");
            RejectedCount = allQuotes.Count(q => q.Status == "REJECTED");

            // Filter by status if specified
            IEnumerable<SalesDocument> quotes;
            if (StatusFilter != "all" && !string.IsNullOrEmpty(StatusFilter))
            {
                quotes = allQuotes.Where(q => q.Status == StatusFilter);
            }
            else
            {
                quotes = allQuotes;
            }

            Quotes = quotes.Select(q => new QuoteViewModel
            {
                Id = q.Id,
                CustomerName = q.Customer?.FullName ?? "N/A",
                CreatedAt = q.CreatedAt,
                Status = q.Status,
                Total = q.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0
            }).ToList();

            return Page();
        }

        public class QuoteViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string Status { get; set; } = "";
            public decimal Total { get; set; }
        }
    }
}

