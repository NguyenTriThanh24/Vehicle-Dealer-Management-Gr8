using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.EVM.Dealers
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDealerService _dealerService;
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentService _paymentService;
        private readonly IStockService _stockService;

        public DetailModel(
            ApplicationDbContext context,
            IDealerService dealerService,
            ISalesDocumentService salesDocumentService,
            IPaymentService paymentService,
            IStockService stockService)
        {
            _context = context;
            _dealerService = dealerService;
            _salesDocumentService = salesDocumentService;
            _paymentService = paymentService;
            _stockService = stockService;
        }

        public DealerDetailViewModel Dealer { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!id.HasValue)
            {
                return RedirectToPage("/EVM/Dealers");
            }

            var dealer = await _dealerService.GetDealerByIdAsync(id.Value);
            if (dealer == null)
            {
                TempData["Error"] = "Không tìm thấy đại lý này.";
                return RedirectToPage("/EVM/Dealers");
            }

            // Get dealer orders
            var dealerOrders = await _context.DealerOrders
                .Where(o => o.DealerId == id.Value)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Get sales documents (orders) from this dealer
            var allOrders = (await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(id.Value))
                .Where(s => s.Type == "ORDER")
                .ToList();

            var orders = allOrders
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .ToList();

            var totalSales = allOrders
                .Sum(o => o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0);

            // Calculate total paid
            decimal totalPaid = 0;
            foreach (var order in allOrders)
            {
                totalPaid += await _paymentService.GetTotalPaidAmountAsync(order.Id);
            }

            // Calculate debt (outstanding amount)
            var totalDebt = totalSales - totalPaid;

            // Get staff count
            var staffCount = await _context.Users
                .CountAsync(u => u.DealerId == id.Value);

            // Get stock summary
            var stocks = (await _stockService.GetStocksByOwnerAsync("DEALER", id.Value)).ToList();
            var stockSummary = stocks
                .GroupBy(s => s.VehicleId)
                .Select(g =>
                {
                    var firstStock = g.First();
                    var vehicle = _context.Vehicles.Find(firstStock.VehicleId);
                    return new StockSummaryViewModel
                    {
                        VehicleName = vehicle != null ? $"{vehicle.ModelName} {vehicle.VariantName}" : "N/A",
                        TotalQty = (int)g.Sum(s => s.Qty)
                    };
                })
                .ToList();

            Dealer = new DealerDetailViewModel
            {
                Id = dealer.Id,
                Name = dealer.Name,
                Address = dealer.Address,
                Phone = dealer.PhoneNumber ?? "",
                Email = dealer.Email ?? "",
                Status = dealer.Status,
                CreatedDate = dealer.CreatedDate,

                // Stats
                StaffCount = staffCount,
                TotalOrders = allOrders.Count,
                TotalSales = totalSales,
                TotalPaid = totalPaid,
                TotalDebt = totalDebt,
                DealerOrdersCount = dealerOrders.Count,

                // Recent orders
                RecentOrders = (await Task.WhenAll(orders.Select(async o =>
                {
                    var paidAmount = await _paymentService.GetTotalPaidAmountAsync(o.Id);
                    var customer = await _context.CustomerProfiles.FindAsync(o.CustomerId);
                    return new OrderSummaryViewModel
                    {
                        Id = o.Id,
                        OrderNumber = $"ORD-{o.Id:D6}",
                        CustomerName = customer?.FullName ?? "N/A",
                        TotalAmount = o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0,
                        PaidAmount = paidAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt
                    };
                }))).ToList(),

                // Stock summary
                StockSummary = stockSummary
            };

            return Page();
        }

        public class DealerDetailViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime CreatedDate { get; set; }

            public int StaffCount { get; set; }
            public int TotalOrders { get; set; }
            public decimal TotalSales { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal TotalDebt { get; set; }
            public int DealerOrdersCount { get; set; }

            public List<OrderSummaryViewModel> RecentOrders { get; set; } = new();
            public List<StockSummaryViewModel> StockSummary { get; set; } = new();
        }

        public class OrderSummaryViewModel
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public decimal TotalAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public string Status { get; set; } = "";
            public DateTime CreatedAt { get; set; }
        }

        public class StockSummaryViewModel
        {
            public string VehicleName { get; set; } = "";
            public int TotalQty { get; set; }
        }
    }
}

