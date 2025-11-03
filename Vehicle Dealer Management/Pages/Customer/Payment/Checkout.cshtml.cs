using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Customer.Payment
{
    public class CheckoutModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _context;

        public CheckoutModel(
            ISalesDocumentService salesDocumentService,
            IPaymentGatewayService paymentGatewayService,
            IPaymentService paymentService,
            ApplicationDbContext context)
        {
            _salesDocumentService = salesDocumentService;
            _paymentGatewayService = paymentGatewayService;
            _paymentService = paymentService;
            _context = context;
        }

        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId, string? provider)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "CUSTOMER";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Customer";

            var userIdInt = int.Parse(userId);
            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customer == null)
            {
                return RedirectToPage("/Auth/Profile");
            }

            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
            if (order == null || order.CustomerId != customer.Id || order.Type != "ORDER")
            {
                return NotFound();
            }

            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(orderId);
            var remainingAmount = totalAmount - paidAmount;

            OrderId = orderId;
            OrderNumber = $"ORD-{orderId:D6}";
            TotalAmount = totalAmount;
            RemainingAmount = remainingAmount;

            if (remainingAmount <= 0)
            {
                TempData["Error"] = "Đơn hàng này đã được thanh toán đủ.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            // Nếu có provider thì redirect đến payment gateway
            if (!string.IsNullOrEmpty(provider))
            {
                return await ProcessPaymentAsync(orderId, provider, remainingAmount);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProcessPaymentAsync(int orderId, string provider)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userIdInt = int.Parse(userId);
            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customer == null)
            {
                return RedirectToPage("/Auth/Profile");
            }

            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
            if (order == null || order.CustomerId != customer.Id || order.Type != "ORDER")
            {
                return NotFound();
            }

            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(orderId);
            var remainingAmount = totalAmount - paidAmount;

            if (remainingAmount <= 0)
            {
                TempData["Error"] = "Đơn hàng này đã được thanh toán đủ.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            return await ProcessPaymentAsync(orderId, provider, remainingAmount);
        }

        private async Task<IActionResult> ProcessPaymentAsync(int orderId, string provider, decimal amount)
        {
            try
            {
                var orderNumber = $"ORD-{orderId:D6}";
                var orderInfo = $"Thanh toan don hang {orderNumber}";
                string paymentUrl;

                if (provider.ToUpper() == "MOMO")
                {
                    paymentUrl = await _paymentGatewayService.CreateMoMoPaymentUrlAsync(
                        orderId,
                        amount,
                        $"/Customer/Payment/Callback?provider=momo&orderId={orderId}",
                        $"/Customer/Payment/IPN?provider=momo",
                        orderInfo
                    );
                }
                else if (provider.ToUpper() == "VNPAY")
                {
                    paymentUrl = await _paymentGatewayService.CreateVNPayPaymentUrlAsync(
                        orderId,
                        amount,
                        $"/Customer/Payment/Callback?provider=vnpay&orderId={orderId}",
                        $"/Customer/Payment/IPN?provider=vnpay",
                        orderInfo,
                        HttpContext
                    );
                }
                else
                {
                    TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToPage(new { orderId });
                }

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo thanh toán: {ex.Message}";
                return RedirectToPage(new { orderId });
            }
        }
    }
}

