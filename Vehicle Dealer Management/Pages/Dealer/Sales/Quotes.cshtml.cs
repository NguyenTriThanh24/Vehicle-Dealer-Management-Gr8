using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class QuotesModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;

        public QuotesModel(ISalesDocumentService salesDocumentService)
        {
            _salesDocumentService = salesDocumentService;
        }

        public List<QuoteViewModel> Quotes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);

            var quotes = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(
                dealerIdInt, 
                type: "QUOTE", 
                status: null);

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

