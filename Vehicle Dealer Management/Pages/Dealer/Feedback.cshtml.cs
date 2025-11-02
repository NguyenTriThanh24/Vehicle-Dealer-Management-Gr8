using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class FeedbackModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomerService _customerService;

        public FeedbackModel(ApplicationDbContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        public string TypeFilter { get; set; } = "all";
        public int TotalFeedback { get; set; }
        public int NewCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }

        public List<FeedbackViewModel> Feedbacks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? type)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            TypeFilter = type ?? "all";
            var dealerIdInt = int.Parse(dealerId);

            // Get feedbacks
            var query = _context.Feedbacks
                .Where(f => f.DealerId == dealerIdInt)
                .Include(f => f.Customer)
                .AsQueryable();

            if (TypeFilter != "all")
            {
                query = query.Where(f => f.Type == TypeFilter);
            }

            var feedbacks = await query
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            TotalFeedback = feedbacks.Count;
            NewCount = feedbacks.Count(f => f.Status == "NEW");
            InProgressCount = feedbacks.Count(f => f.Status == "IN_PROGRESS");
            ResolvedCount = feedbacks.Count(f => f.Status == "RESOLVED");

            Feedbacks = (await Task.WhenAll(feedbacks.Select(async f =>
            {
                var customer = f.CustomerId > 0 
                    ? await _customerService.GetCustomerByIdAsync(f.CustomerId) 
                    : null;
                
                return new FeedbackViewModel
                {
                    Id = f.Id,
                    CustomerName = customer?.FullName ?? "N/A",
                    Type = f.Type,
                    Status = f.Status,
                    Content = f.Content,
                    CreatedAt = f.CreatedAt
                };
            }))).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostStartProcessAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                feedback.Status = "IN_PROGRESS";
                feedback.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResolveAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                feedback.Status = "RESOLVED";
                feedback.ResolvedAt = DateTime.UtcNow;
                feedback.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public class FeedbackViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string Type { get; set; } = "";
            public string Status { get; set; } = "";
            public string Content { get; set; } = "";
            public DateTime CreatedAt { get; set; }
        }
    }
}

