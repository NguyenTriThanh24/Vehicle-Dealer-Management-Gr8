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
        private readonly IFeedbackService _feedbackService;

        public FeedbackModel(
            ApplicationDbContext context, 
            ICustomerService customerService,
            IFeedbackService feedbackService)
        {
            _context = context;
            _customerService = customerService;
            _feedbackService = feedbackService;
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

            // Get feedbacks using service
            IEnumerable<Vehicle_Dealer_Management.DAL.Models.Feedback> feedbacks;

            if (TypeFilter != "all")
            {
                feedbacks = await _feedbackService.GetFeedbacksByTypeAsync(TypeFilter, dealerIdInt);
            }
            else
            {
                feedbacks = await _feedbackService.GetFeedbacksByDealerIdAsync(dealerIdInt);
            }

            var feedbacksList = feedbacks.ToList();

            TotalFeedback = feedbacksList.Count;
            NewCount = feedbacksList.Count(f => f.Status == "NEW");
            InProgressCount = feedbacksList.Count(f => f.Status == "IN_PROGRESS");
            ResolvedCount = feedbacksList.Count(f => f.Status == "RESOLVED");

            Feedbacks = (await Task.WhenAll(feedbacksList.Select(async f =>
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
            await _feedbackService.UpdateFeedbackStatusAsync(id, "IN_PROGRESS");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResolveAsync(int id)
        {
            await _feedbackService.UpdateFeedbackStatusAsync(id, "RESOLVED");
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

