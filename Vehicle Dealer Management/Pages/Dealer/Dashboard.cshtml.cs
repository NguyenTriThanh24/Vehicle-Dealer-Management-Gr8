using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly ICustomerService _customerService;

        public DashboardModel(
            ApplicationDbContext context,
            ISalesDocumentService salesDocumentService,
            ICustomerService customerService)
        {
            _context = context;
            _salesDocumentService = salesDocumentService;
            _customerService = customerService;
        }

        public string UserName { get; set; } = "";
        public string DealerName { get; set; } = "";
        public decimal TodaySales { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int TotalCustomers { get; set; }

        public List<OrderViewModel> RecentOrders { get; set; } = new();
        public List<TestDriveViewModel> TodayTestDrives { get; set; } = new();
        public List<QuoteViewModel> PendingQuotesList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var dealerId = HttpContext.Session.GetString("DealerId");
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _context.Users
                .Include(u => u.Dealer)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            UserName = user.FullName;
            DealerName = user.Dealer?.Name ?? "Đại lý";

            var dealerIdInt = int.Parse(dealerId);

            // Get statistics using services
            var allOrders = (await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerIdInt, "ORDER", null)).ToList();

            TotalOrders = allOrders.Count;
            PendingOrders = allOrders.Count(o => o.Status == "OPEN" || o.Status == "PENDING");
            
            // Calculate today sales
            var todayOrders = allOrders.Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date).ToList();
            TodaySales = todayOrders.Sum(o => o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0);

            var allQuotes = (await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerIdInt, "QUOTE", null)).ToList();

            TotalQuotes = allQuotes.Count;
            PendingQuotes = allQuotes.Count(q => q.Status == "DRAFT");

            // Get recent orders
            var recentOrders = allOrders
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToList();

            RecentOrders = (await Task.WhenAll(recentOrders.Select(async o =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(o.CustomerId);
                return new OrderViewModel
                {
                    Id = o.Id,
                    CustomerName = customer?.FullName ?? "N/A",
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    TotalAmount = o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0
                };
            }))).ToList();

            // Get today's test drives
            var today = DateTime.Today;
            var testDrives = await _context.TestDrives
                .Where(t => t.DealerId == dealerIdInt && 
                            t.ScheduleTime.Date == today)
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .OrderBy(t => t.ScheduleTime)
                .ToListAsync();

            TodayTestDrives = testDrives.Select(t => new TestDriveViewModel
            {
                CustomerName = t.Customer?.FullName ?? "N/A",
                VehicleName = $"{t.Vehicle?.ModelName} {t.Vehicle?.VariantName}",
                Time = t.ScheduleTime.ToString("HH:mm"),
                Status = t.Status
            }).ToList();

            // Get pending quotes
            var pendingQuotes = allQuotes
                .Where(q => q.Status == "DRAFT")
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToList();

            PendingQuotesList = (await Task.WhenAll(pendingQuotes.Select(async q =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(q.CustomerId);
                return new QuoteViewModel
                {
                    Id = q.Id,
                    CustomerName = customer?.FullName ?? "N/A",
                    CreatedAt = q.CreatedAt
                };
            }))).ToList();

            // Get total unique customers
            var uniqueCustomerIds = allOrders.Select(o => o.CustomerId).Union(allQuotes.Select(q => q.CustomerId)).Distinct();
            TotalCustomers = uniqueCustomerIds.Count();

            return Page();
        }

        public class OrderViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string Status { get; set; } = "";
            public decimal TotalAmount { get; set; }
        }

        public class TestDriveViewModel
        {
            public string CustomerName { get; set; } = "";
            public string VehicleName { get; set; } = "";
            public string Time { get; set; } = "";
            public string Status { get; set; } = "";
        }

        public class QuoteViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public DateTime CreatedAt { get; set; }
        }
    }
}

