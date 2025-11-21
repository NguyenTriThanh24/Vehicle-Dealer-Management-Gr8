using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using System.Web;

namespace Vehicle_Dealer_Management.Pages.Customer.Payment
{
    public class CallbackModel : PageModel
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentService _paymentService;
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly ApplicationDbContext _context;

        public CallbackModel(
            IPaymentGatewayService paymentGatewayService,
            IPaymentService paymentService,
            ISalesDocumentService salesDocumentService,
            ApplicationDbContext context)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentService = paymentService;
            _salesDocumentService = salesDocumentService;
            _context = context;
        }

        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public int? OrderId { get; set; }

        public async Task<IActionResult> OnGetAsync(string? provider, int? orderId)
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "CUSTOMER";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Customer";

            if (string.IsNullOrEmpty(provider) || !orderId.HasValue)
            {
                Status = "error";
                Message = "Thông tin không hợp lệ.";
                return Page();
            }

            OrderId = orderId.Value;
            var callbackData = new Dictionary<string, string>();

            try
            {
                if (provider.Equals("MOMO", StringComparison.OrdinalIgnoreCase) ||
                    provider.Equals("MOMO_ATM", StringComparison.OrdinalIgnoreCase))
                {
                    // MoMo trả về trong query string khi redirect
                    foreach (var query in Request.Query)
                    {
                        callbackData[query.Key] = query.Value.ToString();
                    }

                    var result = await _paymentGatewayService.ProcessMoMoCallbackAsync(callbackData);
                    // Nếu không có orderId từ callback, dùng orderId từ query
                    var finalOrderId = result.OrderId ?? orderId.Value.ToString();
                    var methodCode = provider.Equals("MOMO_ATM", StringComparison.OrdinalIgnoreCase) ? "MOMO_ATM" : "MOMO";
                    await ProcessPaymentResultAsync(finalOrderId, result.Amount ?? 0, methodCode, result.TransactionId, result.IsSuccess, result.Message ?? "Payment processed");
                    
                    Status = result.IsSuccess ? "success" : "failed";
                    Message = result.Message ?? (result.IsSuccess ? "Thanh toán thành công" : "Thanh toán thất bại");
                }
                else if (provider.ToUpper() == "VNPAY")
                {
                    // VNPay trả về trong query string
                    foreach (var query in Request.Query)
                    {
                        callbackData[query.Key] = query.Value.ToString();
                    }

                    var result = await _paymentGatewayService.ProcessVNPayCallbackAsync(callbackData);
                    // Nếu không có orderId từ callback, dùng orderId từ query
                    var finalOrderId = result.OrderId ?? orderId.Value.ToString();
                    await ProcessPaymentResultAsync(finalOrderId, result.Amount ?? 0, "VNPAY", result.TransactionId, result.IsSuccess, result.Message ?? "Payment processed");
                    
                    Status = result.IsSuccess ? "success" : "failed";
                    Message = result.Message ?? (result.IsSuccess ? "Thanh toán thành công" : "Thanh toán thất bại");
                }
                else
                {
                    Status = "error";
                    Message = "Phương thức thanh toán không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                Status = "error";
                Message = $"Lỗi xử lý thanh toán: {ex.Message}";
            }

            return Page();
        }

        private async Task ProcessPaymentResultAsync(string? orderIdStr, decimal amount, string method, string? transactionId, bool isSuccess, string message)
        {
            if (string.IsNullOrEmpty(orderIdStr) || !int.TryParse(orderIdStr, out int orderId))
            {
                TempData["Error"] = "Thông tin đơn hàng không hợp lệ.";
                return;
            }

            try
            {
                var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
                if (order == null || order.Type != "ORDER")
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return;
                }

                if (isSuccess)
                {
                    // Tạo payment record
                    // Với môi trường test MoMo, số tiền gửi đi có thể là số tiền đã "scale down" (ví dụ: 25,000 thay vì 2,500,000,000).
                    // Theo yêu cầu: nếu MoMo báo thành công thì xem như hoàn tất thanh toán đơn hàng.
                    // Vì vậy, ghi nhận khoản thanh toán bằng đúng số tiền còn lại của đơn hàng (không quan tâm số tiền từ callback).
                    var orderForCalc = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
                    if (orderForCalc == null || orderForCalc.Lines == null || orderForCalc.Type != "ORDER")
                    {
                        TempData["Error"] = "Không thể tính toán số tiền thanh toán.";
                        return;
                    }

                    var totalAmount = orderForCalc.Lines.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue);
                    var totalPaidSoFar = await _paymentService.GetTotalPaidAmountAsync(orderId);
                    var remainingAmount = totalAmount - totalPaidSoFar;
                    if (remainingAmount < 0) remainingAmount = 0;

                    // Luôn ghi nhận số tiền còn lại để đảm bảo "CÒN LẠI" = 0 sau khi thanh toán thành công
                    // Không quan tâm số tiền từ callback (có thể đã scale down để test)
                    var amountToRecord = remainingAmount;
                    
                    // Nếu đã thanh toán đủ rồi (remainingAmount = 0), không cần tạo payment mới
                    if (amountToRecord <= 0)
                    {
                        TempData["Info"] = "Đơn hàng đã được thanh toán đủ. Không cần thanh toán thêm.";
                        return;
                    }

                    var metaJson = $"{{\"TransactionId\":\"{transactionId ?? "N/A"}\",\"Provider\":\"{method}\",\"Note\":\"Payment from {method} gateway (test amount: {amount:N0} VND, actual recorded: {amountToRecord:N0} VND)\",\"CallbackAmount\":\"{amount}\",\"ActualAmount\":\"{amountToRecord}\"}}";
                    
                    try
                    {
                        await _paymentService.CreatePaymentAsync(orderId, method, amountToRecord, metaJson);
                        TempData["Success"] = $"Thanh toán thành công! Đơn hàng đã được thanh toán đủ.";
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Lỗi do hợp đồng chưa ký hoặc validation khác
                        TempData["Error"] = $"Không thể tạo thanh toán: {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Lỗi khi tạo thanh toán: {ex.Message}";
                    }
                }
                else
                {
                    TempData["Error"] = $"Thanh toán thất bại: {message}";
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                TempData["Error"] = $"Lỗi khi xử lý kết quả thanh toán: {ex.Message}";
            }
        }
    }
}

