using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class OrderDetailModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentService _paymentService;
        private readonly IDeliveryService _deliveryService;

        public OrderDetailModel(
            ISalesDocumentService salesDocumentService,
            IPaymentService paymentService,
            IDeliveryService deliveryService)
        {
            _salesDocumentService = salesDocumentService;
            _paymentService = paymentService;
            _deliveryService = deliveryService;
        }

        public OrderDetailViewModel Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Set UserRole from Session for proper navigation
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var dealerIdInt = int.Parse(dealerId);

            // Get order with all related data from Service
            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (order == null || order.DealerId != dealerIdInt || order.Type != "ORDER")
            {
                return NotFound();
            }

            // Calculate totals from real data
            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(order.Id);
            var remainingAmount = totalAmount - paidAmount;

            Order = new OrderDetailViewModel
            {
                Id = order.Id,
                OrderNumber = $"ORD-{order.Id:D6}",
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                SignedAt = order.SignedAt,

                // Customer Info
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? "N/A",
                CustomerPhone = order.Customer?.Phone ?? "N/A",
                CustomerEmail = order.Customer?.Email ?? "N/A",
                CustomerAddress = order.Customer?.Address ?? "N/A",

                // Dealer Info
                DealerName = order.Dealer?.Name ?? "N/A",
                DealerAddress = order.Dealer?.Address ?? "N/A",

                // Created By
                CreatedBy = order.CreatedByUser?.FullName ?? "N/A",

                // Promotion
                PromotionId = order.PromotionId,
                PromotionName = order.Promotion?.Name,

                // Items (Lines)
                Items = order.Lines?.Select(l => new OrderItemViewModel
                {
                    Id = l.Id,
                    VehicleId = l.VehicleId,
                    VehicleModel = l.Vehicle?.ModelName ?? "N/A",
                    VehicleVariant = l.Vehicle?.VariantName ?? "N/A",
                    ColorCode = l.ColorCode,
                    Qty = l.Qty,
                    UnitPrice = l.UnitPrice,
                    DiscountValue = l.DiscountValue,
                    LineTotal = l.UnitPrice * l.Qty - l.DiscountValue,
                    VehicleImageUrl = l.Vehicle?.ImageUrl
                }).ToList() ?? new List<OrderItemViewModel>(),

                // Payments
                Payments = order.Payments?.OrderByDescending(p => p.PaidAt).Select(p => new PaymentViewModel
                {
                    Id = p.Id,
                    Method = p.Method,
                    Amount = p.Amount,
                    PaidAt = p.PaidAt,
                    MetaJson = p.MetaJson
                }).ToList() ?? new List<PaymentViewModel>(),

                // Delivery
                Delivery = order.Delivery != null ? new DeliveryViewModel
                {
                    Id = order.Delivery.Id,
                    ScheduledDate = order.Delivery.ScheduledDate,
                    DeliveredDate = order.Delivery.DeliveredDate,
                    Status = order.Delivery.Status,
                    HandoverNote = order.Delivery.HandoverNote,
                    CustomerConfirmed = order.Delivery.CustomerConfirmed,
                    CustomerConfirmedDate = order.Delivery.CustomerConfirmedDate
                } : null,

                // Totals
                SubTotal = totalAmount,
                TotalPaid = paidAmount,
                RemainingAmount = remainingAmount,
                TotalAmount = totalAmount
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAddPaymentAsync(int id, string method, decimal amount, string? metaJson)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);

            // Get order
            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (order == null || order.DealerId != dealerIdInt || order.Type != "ORDER")
            {
                return NotFound();
            }

            // Validate amount
            if (amount <= 0)
            {
                TempData["Error"] = "Số tiền phải lớn hơn 0";
                return RedirectToPage(new { id });
            }

            // Calculate remaining amount
            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(order.Id);
            var remainingAmount = totalAmount - paidAmount;

            if (amount > remainingAmount)
            {
                TempData["Error"] = $"Số tiền không được vượt quá số tiền còn lại: {remainingAmount:N0} VND";
                return RedirectToPage(new { id });
            }

            // Create payment using Service (auto-updates order status)
            await _paymentService.CreatePaymentAsync(order.Id, method ?? "CASH", amount, metaJson);

            TempData["Success"] = "Thêm thanh toán thành công!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostScheduleDeliveryAsync(int id, DateTime scheduledDate, string? scheduledTime)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);

            // Get order
            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (order == null || order.DealerId != dealerIdInt || order.Type != "ORDER")
            {
                return NotFound();
            }

            // Combine date and time
            DateTime scheduledDateTime = scheduledDate.Date;
            if (!string.IsNullOrEmpty(scheduledTime) && TimeSpan.TryParse(scheduledTime, out var time))
            {
                scheduledDateTime = scheduledDate.Date.Add(time);
            }
            else
            {
                scheduledDateTime = scheduledDate.Date.AddHours(9); // Default 9:00 AM
            }

            // Validate date
            if (scheduledDateTime < DateTime.Now)
            {
                TempData["Error"] = "Ngày giờ giao không được trong quá khứ";
                return RedirectToPage(new { id });
            }

            // Create or update delivery using Service (auto-updates order status)
            await _deliveryService.CreateOrUpdateDeliveryAsync(order.Id, scheduledDateTime);

            TempData["Success"] = "Lên lịch giao xe thành công!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostStartDeliveryAsync(int id)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);

            // Get order with delivery
            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (order == null || order.DealerId != dealerIdInt || order.Type != "ORDER")
            {
                return NotFound();
            }

            var delivery = await _deliveryService.GetDeliveryBySalesDocumentIdAsync(order.Id);
            if (delivery == null)
            {
                TempData["Error"] = "Chưa có lịch giao xe. Vui lòng lên lịch giao trước.";
                return RedirectToPage(new { id });
            }

            try
            {
                await _deliveryService.StartDeliveryAsync(delivery.Id);
                TempData["Success"] = "Đã bắt đầu giao xe!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCompleteDeliveryAsync(int id, string? handoverNote)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);

            // Get order with delivery
            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (order == null || order.DealerId != dealerIdInt || order.Type != "ORDER")
            {
                return NotFound();
            }

            var delivery = await _deliveryService.GetDeliveryBySalesDocumentIdAsync(order.Id);
            if (delivery == null)
            {
                TempData["Error"] = "Chưa có lịch giao xe. Vui lòng lên lịch giao trước.";
                return RedirectToPage(new { id });
            }

            try
            {
                await _deliveryService.CompleteDeliveryAsync(delivery.Id, DateTime.UtcNow, handoverNote);
                TempData["Success"] = "Hoàn thành giao xe thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage(new { id });
        }

        public class OrderDetailViewModel
        {
            public int Id { get; set; }
            public string OrderNumber { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? SignedAt { get; set; }

            // Customer
            public int CustomerId { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public string CustomerEmail { get; set; } = "";
            public string CustomerAddress { get; set; } = "";

            // Dealer
            public string DealerName { get; set; } = "";
            public string DealerAddress { get; set; } = "";

            // Created By
            public string CreatedBy { get; set; } = "";

            // Promotion
            public int? PromotionId { get; set; }
            public string? PromotionName { get; set; }

            // Items
            public List<OrderItemViewModel> Items { get; set; } = new();

            // Payments
            public List<PaymentViewModel> Payments { get; set; } = new();

            // Delivery
            public DeliveryViewModel? Delivery { get; set; }

            // Totals
            public decimal SubTotal { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal RemainingAmount { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class OrderItemViewModel
        {
            public int Id { get; set; }
            public int VehicleId { get; set; }
            public string VehicleModel { get; set; } = "";
            public string VehicleVariant { get; set; } = "";
            public string ColorCode { get; set; } = "";
            public decimal Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal LineTotal { get; set; }
            public string? VehicleImageUrl { get; set; }
        }

        public class PaymentViewModel
        {
            public int Id { get; set; }
            public string Method { get; set; } = "";
            public decimal Amount { get; set; }
            public DateTime PaidAt { get; set; }
            public string? MetaJson { get; set; }
        }

        public class DeliveryViewModel
        {
            public int Id { get; set; }
            public DateTime ScheduledDate { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public string Status { get; set; } = "";
            public string? HandoverNote { get; set; }
            public bool CustomerConfirmed { get; set; }
            public DateTime? CustomerConfirmedDate { get; set; }
        }
    }
}

