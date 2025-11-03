using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.EVM
{
    public class DealersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDealerService _dealerService;
        private readonly ISalesDocumentService _salesDocumentService;

        public DealersModel(
            ApplicationDbContext context, 
            IDealerService dealerService,
            ISalesDocumentService salesDocumentService)
        {
            _context = context;
            _dealerService = dealerService;
            _salesDocumentService = salesDocumentService;
        }

        public int TotalDealers { get; set; }
        public int ActiveDealers { get; set; }
        public int InactiveDealers { get; set; }
        public decimal AvgDealerSales { get; set; }

        public List<DealerViewModel> Dealers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get all dealers
            var dealers = (await _dealerService.GetAllDealersAsync()).ToList();

            TotalDealers = dealers.Count;
            ActiveDealers = dealers.Count(d => d.Status == "ACTIVE");
            InactiveDealers = dealers.Count(d => d.Status != "ACTIVE");

            // Get sales for each dealer from actual data
            foreach (var dealer in dealers)
            {
                // Get orders for this dealer
                var orders = (await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealer.Id, "ORDER", null)).ToList();
                
                // Calculate monthly sales (current month)
                var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var monthlyOrders = orders
                    .Where(o => o.CreatedAt >= currentMonthStart && 
                                o.CreatedAt < currentMonthStart.AddMonths(1))
                    .ToList();
                
                var monthlySales = monthlyOrders
                    .Sum(o => o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0);
                
                // Total orders count
                var totalOrders = orders.Count;

                Dealers.Add(new DealerViewModel
                {
                    Id = dealer.Id,
                    Name = dealer.Name,
                    Address = dealer.Address,
                    Phone = dealer.PhoneNumber,
                    Email = dealer.Email,
                    Status = dealer.Status,
                    MonthlySales = monthlySales,
                    TotalOrders = totalOrders
                });
            }

            AvgDealerSales = Dealers.Any() ? Dealers.Average(d => d.MonthlySales) : 0;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteDealerAsync(int dealerId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Chỉ EVM_STAFF và EVM_ADMIN mới có quyền xóa đại lý
            if (userRole != "EVM_STAFF" && userRole != "EVM_ADMIN")
            {
                TempData["Error"] = "Bạn không có quyền thực hiện thao tác này.";
                return RedirectToPage();
            }

            var dealer = await _context.Dealers.FindAsync(dealerId);
            if (dealer == null)
            {
                TempData["Error"] = "Không tìm thấy đại lý.";
                return RedirectToPage();
            }

            try
            {
                // Xóa các bản ghi liên quan trước khi xóa Dealer (vì có Restrict constraint)

                // 1. Xóa PricePolicies của đại lý
                var pricePolicies = await _context.PricePolicies
                    .Where(p => p.DealerId == dealerId)
                    .ToListAsync();
                _context.PricePolicies.RemoveRange(pricePolicies);

                // 2. Xóa DealerOrders
                var dealerOrders = await _context.DealerOrders
                    .Where(o => o.DealerId == dealerId)
                    .ToListAsync();
                _context.DealerOrders.RemoveRange(dealerOrders);

                // 3. Xóa SalesDocuments và SalesDocumentLines
                var salesDocuments = await _context.SalesDocuments
                    .Where(s => s.DealerId == dealerId)
                    .ToListAsync();
                foreach (var salesDoc in salesDocuments)
                {
                    var lines = await _context.SalesDocumentLines
                        .Where(l => l.SalesDocumentId == salesDoc.Id)
                        .ToListAsync();
                    _context.SalesDocumentLines.RemoveRange(lines);
                }
                _context.SalesDocuments.RemoveRange(salesDocuments);

                // 4. Xóa Payments liên quan đến SalesDocuments đã xóa
                // (Payment có thể có foreign key với SalesDocument)
                var payments = await _context.Payments
                    .Where(p => salesDocuments.Select(s => s.Id).Contains(p.SalesDocumentId))
                    .ToListAsync();
                _context.Payments.RemoveRange(payments);

                // 5. Xóa Deliveries
                var deliveries = await _context.Deliveries
                    .Where(d => salesDocuments.Select(s => s.Id).Contains(d.SalesDocumentId))
                    .ToListAsync();
                _context.Deliveries.RemoveRange(deliveries);

                // 6. Xóa TestDrives
                var testDrives = await _context.TestDrives
                    .Where(t => t.DealerId == dealerId)
                    .ToListAsync();
                _context.TestDrives.RemoveRange(testDrives);

                // 7. Xóa Feedbacks
                var feedbacks = await _context.Feedbacks
                    .Where(f => f.DealerId == dealerId)
                    .ToListAsync();
                _context.Feedbacks.RemoveRange(feedbacks);

                // 8. Xóa Stocks của đại lý (OwnerType = "DEALER", OwnerId = dealerId)
                var stocks = await _context.Stocks
                    .Where(s => s.OwnerType == "DEALER" && s.OwnerId == dealerId)
                    .ToListAsync();
                _context.Stocks.RemoveRange(stocks);

                // 9. Gỡ liên kết Users (set DealerId = null)
                var users = await _context.Users
                    .Where(u => u.DealerId == dealerId)
                    .ToListAsync();
                foreach (var user in users)
                {
                    user.DealerId = null;
                }

                // 10. Xóa Dealer
                _context.Dealers.Remove(dealer);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã xóa đại lý '{dealer.Name}' và tất cả dữ liệu liên quan thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa đại lý: {ex.Message}";
            }

            return RedirectToPage();
        }

        public class DealerViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal MonthlySales { get; set; }
            public int TotalOrders { get; set; }
        }
    }
}

