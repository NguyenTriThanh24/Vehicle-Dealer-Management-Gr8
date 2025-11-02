using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.DealerManager
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentService _paymentService;

        public DashboardModel(
            ApplicationDbContext context,
            ISalesDocumentService salesDocumentService,
            IPaymentService paymentService)
        {
            _context = context;
            _salesDocumentService = salesDocumentService;
            _paymentService = paymentService;
        }

        public string UserName { get; set; } = "";
        public string DealerName { get; set; } = "";
        public decimal MonthSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public decimal TotalDebt { get; set; }

        public List<StaffPerformanceViewModel> StaffPerformance { get; set; } = new();
        public List<CustomerDebtViewModel> CustomerDebts { get; set; } = new();

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

            // Get real data for statistics
            var allOrders = (await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerIdInt, "ORDER", null)).ToList();
            
            // Calculate month sales
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthOrders = allOrders.Where(o => o.CreatedAt >= monthStart).ToList();
            MonthSales = monthOrders.Sum(o => o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0);
            
            TotalOrders = allOrders.Count;
            
            // Get staff count
            TotalStaff = await _context.Users.CountAsync(u => u.DealerId == dealerIdInt);
            ActiveStaff = TotalStaff; // Assuming all staff are active (User model may not have IsActive property)
            
            // Calculate total debt (orders not fully paid)
            decimal totalDebt = 0;
            foreach (var order in allOrders)
            {
                var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
                var paidAmount = await _paymentService.GetTotalPaidAmountAsync(order.Id);
                var remaining = totalAmount - paidAmount;
                if (remaining > 0)
                {
                    totalDebt += remaining;
                }
            }
            TotalDebt = totalDebt;

            // Mock staff performance
            StaffPerformance = new List<StaffPerformanceViewModel>
            {
                new() { Name = "Nguyễn Văn A", OrderCount = 15, Sales = 1200000000, TargetPercent = 95, Rank = 1 },
                new() { Name = "Trần Thị B", OrderCount = 12, Sales = 980000000, TargetPercent = 82, Rank = 2 },
                new() { Name = "Lê Văn C", OrderCount = 10, Sales = 850000000, TargetPercent = 71, Rank = 3 },
                new() { Name = "Phạm Thị D", OrderCount = 5, Sales = 470000000, TargetPercent = 47, Rank = 4 }
            };

            // Mock customer debts
            CustomerDebts = new List<CustomerDebtViewModel>
            {
                new() { CustomerName = "Nguyễn Văn X", Amount = 150000000, DueDate = DateTime.Now.AddDays(5) },
                new() { CustomerName = "Trần Thị Y", Amount = 100000000, DueDate = DateTime.Now.AddDays(-2) }
            };

            return Page();
        }

        public class StaffPerformanceViewModel
        {
            public string Name { get; set; } = "";
            public int OrderCount { get; set; }
            public decimal Sales { get; set; }
            public int TargetPercent { get; set; }
            public int Rank { get; set; }
        }

        public class CustomerDebtViewModel
        {
            public string CustomerName { get; set; } = "";
            public decimal Amount { get; set; }
            public DateTime DueDate { get; set; }
        }
    }
}

