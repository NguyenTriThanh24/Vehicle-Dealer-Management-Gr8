using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class OrdersModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentService _paymentService;

        public OrdersModel(
            ISalesDocumentService salesDocumentService,
            IPaymentService paymentService)
        {
            _salesDocumentService = salesDocumentService;
            _paymentService = paymentService;
        }

        public string StatusFilter { get; set; } = "all";
        public int TotalOrders { get; set; }
        public int OpenOrders { get; set; }
        public int PaidOrders { get; set; }
        public int DeliveredOrders { get; set; }

        public List<OrderViewModel> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? status)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Set UserRole from Session for proper navigation
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            StatusFilter = status ?? "all";
            var dealerIdInt = int.Parse(dealerId);

            // Get orders using Service
            var orders = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(
                dealerIdInt,
                type: "ORDER",
                status: StatusFilter != "all" ? StatusFilter : null);

            // Calculate counts from the orders list
            var allOrders = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(
                dealerIdInt,
                type: "ORDER",
                status: null);

            TotalOrders = allOrders.Count();
            OpenOrders = allOrders.Count(o => o.Status == "OPEN");
            PaidOrders = allOrders.Count(o => o.Status == "PAID");
            DeliveredOrders = allOrders.Count(o => o.Status == "DELIVERED");

            Orders = orders.Select(o =>
            {
                var totalAmount = o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
                var paidAmount = o.Payments?.Sum(p => p.Amount) ?? 0;
                return new OrderViewModel
                {
                    Id = o.Id,
                    CustomerName = o.Customer?.FullName ?? "N/A",
                    CustomerPhone = o.Customer?.Phone ?? "N/A",
                    CreatedAt = o.CreatedAt,
                    VehicleCount = (int)(o.Lines?.Sum(l => (decimal?)l.Qty) ?? 0),
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    RemainingAmount = totalAmount - paidAmount,
                    Status = o.Status
                };
            }).ToList();

            return Page();
        }

        public class OrderViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public int VehicleCount { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal RemainingAmount { get; set; }
            public string Status { get; set; } = "";
        }
    }
}

