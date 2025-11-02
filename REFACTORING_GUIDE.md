# 📘 HƯỚNG DẪN REFACTOR PAGES SỬ DỤNG SERVICES

**Mục đích:** Refactor các Pages từ việc dùng `ApplicationDbContext` trực tiếp sang dùng Services/Repositories đúng pattern 3-layer architecture.

---

## ✅ **ĐÃ HOÀN THÀNH:**

### **Services đã tạo (11 Services):**
1. ✅ **VehicleService** - Quản lý xe điện
2. ✅ **CustomerService** - Quản lý khách hàng
3. ✅ **DealerService** - Quản lý đại lý
4. ✅ **PricePolicyService** - Quản lý giá bán
5. ✅ **StockService** - Quản lý tồn kho
6. ✅ **SalesDocumentService** - Quản lý Quotes/Orders (QUAN TRỌNG)
7. ✅ **PaymentService** - Quản lý thanh toán
8. ✅ **DeliveryService** - Quản lý giao xe
9. ✅ **TestDriveService** - Quản lý lịch lái thử
10. ✅ **FeedbackService** - Quản lý phản hồi/khiếu nại
11. ✅ **SaleService** - Quản lý bán hàng (legacy)

### **Pages đã refactor (36 Pages):**

**Sales Pages:**
1. ✅ `Dealer/Sales/Quotes.cshtml.cs` - Dùng `ISalesDocumentService`
2. ✅ `Dealer/Sales/CreateQuote.cshtml.cs` - Dùng `ISalesDocumentService` + `IVehicleService` + `IPricePolicyService`
3. ✅ `Dealer/Sales/QuoteDetail.cshtml.cs` - Dùng `ISalesDocumentService`
4. ✅ `Dealer/Sales/Orders.cshtml.cs` - Dùng `ISalesDocumentService` + `IPaymentService`
5. ✅ `Dealer/Sales/OrderDetail.cshtml.cs` - Dùng `ISalesDocumentService` + `IPaymentService` + `IDeliveryService`
6. ✅ `Dealer/Sales/EditQuote.cshtml.cs` - Dùng `ISalesDocumentService` + `IVehicleService` + `IPricePolicyService`

**Vehicle Pages:**
6. ✅ `Customer/Vehicles.cshtml.cs` - Dùng `VehicleService` + `PricePolicyService`
7. ✅ `Customer/VehicleDetail.cshtml.cs` - Dùng `IVehicleService` + `IPricePolicyService`
8. ✅ `Dealer/Vehicles.cshtml.cs` - Dùng `VehicleService` + `PricePolicyService` + `StockService`
9. ✅ `Dealer/VehicleDetail.cshtml.cs` - Dùng `IVehicleService` + `IPricePolicyService` + `IStockService`
10. ✅ `EVM/Vehicles/Index.cshtml.cs` - Dùng `VehicleService` + `PricePolicyService`
11. ✅ `EVM/Vehicles/Create.cshtml.cs` - Dùng `IVehicleService` + `IPricePolicyService` + `IStockService`
12. ✅ `EVM/Vehicles/Edit.cshtml.cs` - Dùng `IVehicleService`

**EVM Pages:**
13. ✅ `EVM/PricePolicies.cshtml.cs` - Dùng `IPricePolicyService` + `IVehicleService` + `IDealerService`
14. ✅ `EVM/Stocks.cshtml.cs` - Dùng `IStockService` + `IVehicleService`

**Customer Pages:**
15. ✅ `Customer/MyQuotes.cshtml.cs` - Dùng `ISalesDocumentService`
16. ✅ `Customer/MyOrders.cshtml.cs` - Dùng `ISalesDocumentService` + `IPaymentService`
17. ✅ `Customer/OrderDetail.cshtml.cs` - Dùng `ISalesDocumentService` + `IPaymentService` + `IDeliveryService`
18. ✅ `Customer/TestDrive.cshtml.cs` - Dùng `IDealerService` + `IVehicleService`

**EVM Pages (tiếp):**
19. ✅ `EVM/Dealers.cshtml.cs` - Dùng `IDealerService`
20. ✅ `EVM/Dealers/Detail.cshtml.cs` - Dùng `IDealerService` + `ISalesDocumentService` + `IPaymentService` + `IStockService`

**Dealer Pages:**
21. ✅ `Dealer/Customers.cshtml.cs` - Dùng `ISalesDocumentService` (một phần)
22. ✅ `Dealer/Dashboard.cshtml.cs` - Dùng `ISalesDocumentService` + `ICustomerService`

**Dashboard Pages:**
23. ✅ `Customer/Dashboard.cshtml.cs` - Dùng `ISalesDocumentService` + `IVehicleService`
24. ✅ `EVM/Dashboard.cshtml.cs` - Dùng `IVehicleService` + `IStockService` + `IDealerService`
25. ✅ `Admin/Dashboard.cshtml.cs` - Dùng `IDealerService` + `IStockService` + `IVehicleService`
26. ✅ `DealerManager/Dashboard.cshtml.cs` - Dùng `ISalesDocumentService` + `IPaymentService`

**Other Pages:**
27. ✅ `Dealer/TestDrives.cshtml.cs` - Dùng `ICustomerService` + `IVehicleService` (một phần)
28. ✅ `EVM/DealerOrders.cshtml.cs` - Dùng `IDealerService` (một phần)
29. ✅ `EVM/DealerOrderDetail.cshtml.cs` - Dùng `IDealerService` + `IStockService` + `IVehicleService` (một phần)
30. ✅ `Admin/Users.cshtml.cs` - Dùng `IDealerService`
31. ✅ `Dealer/Feedback.cshtml.cs` - Dùng `ICustomerService` + `IFeedbackService`
32. ✅ `Auth/Register.cshtml.cs` - Dùng `ICustomerService` (một phần)
33. ✅ `Customer/TestDrive.cshtml.cs` - Dùng `IDealerService` + `IVehicleService` + `ITestDriveService`
34. ✅ `Dealer/TestDrives.cshtml.cs` - Dùng `ICustomerService` + `IVehicleService` + `ITestDriveService`
35. ✅ `Customer/Dashboard.cshtml.cs` - Updated dùng `ITestDriveService`
36. ✅ `Dealer/Dashboard.cshtml.cs` - Updated dùng `ITestDriveService`

---

## 🔄 **PATTERN REFACTOR:**

### **Bước 1: Thay thế ApplicationDbContext bằng Services**

**TRƯỚC:**
```csharp
public class VehiclesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public VehiclesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.Status == "AVAILABLE")
            .ToListAsync();
        
        foreach (var vehicle in vehicles)
        {
            var price = await _context.PricePolicies
                .Where(p => p.VehicleId == vehicle.Id && p.DealerId == null)
                .FirstOrDefaultAsync();
        }
    }
}
```

**SAU:**
```csharp
public class VehiclesModel : PageModel
{
    private readonly IVehicleService _vehicleService;
    private readonly IPricePolicyService _pricePolicyService;

    public VehiclesModel(
        IVehicleService vehicleService,
        IPricePolicyService pricePolicyService)
    {
        _vehicleService = vehicleService;
        _pricePolicyService = pricePolicyService;
    }

    public async Task OnGetAsync()
    {
        var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
        
        foreach (var vehicle in vehicles)
        {
            var pricePolicy = await _pricePolicyService
                .GetActivePricePolicyAsync(vehicle.Id, null);
        }
    }
}
```

---

## 📋 **MAPPING SERVICES THEO CHỨC NĂNG:**

### **Vehicle Operations:**
- `IVehicleService.GetAllVehiclesAsync()` - Lấy tất cả xe
- `IVehicleService.GetAvailableVehiclesAsync()` - Lấy xe available
- `IVehicleService.GetVehicleByIdAsync(id)` - Lấy xe theo ID
- `IVehicleService.CreateVehicleAsync(vehicle)` - Tạo xe mới
- `IVehicleService.UpdateVehicleAsync(vehicle)` - Cập nhật xe
- `IVehicleService.DeleteVehicleAsync(id)` - Xóa xe

### **Price Policy Operations:**
- `IPricePolicyService.GetActivePricePolicyAsync(vehicleId, dealerId)` - Lấy giá hiện tại
- `IPricePolicyService.GetPricePoliciesByVehicleIdAsync(vehicleId)` - Lấy tất cả giá của xe
- `IPricePolicyService.CreatePricePolicyAsync(pricePolicy)` - Tạo giá mới
- `IPricePolicyService.UpdatePricePolicyAsync(pricePolicy)` - Cập nhật giá
- `IPricePolicyService.DeletePricePolicyAsync(id)` - Xóa giá

### **Stock Operations:**
- `IStockService.GetAvailableStocksByVehicleIdAsync(vehicleId, ownerType)` - Lấy tồn kho available
- `IStockService.GetStocksByOwnerAsync(ownerType, ownerId)` - Lấy tồn kho theo owner
- `IStockService.CreateOrUpdateStockAsync(...)` - Tạo/cập nhật tồn kho
- `IStockService.UpdateStockQtyAsync(stockId, newQty)` - Cập nhật số lượng

### **SalesDocument Operations (Quotes/Orders):**
- `ISalesDocumentService.GetSalesDocumentWithDetailsAsync(id)` - Lấy Quote/Order với tất cả details
- `ISalesDocumentService.GetSalesDocumentsByDealerIdAsync(dealerId, type, status)` - Lấy danh sách
- `ISalesDocumentService.GetSalesDocumentsByCustomerIdAsync(customerId, type)` - Lấy theo customer
- `ISalesDocumentService.CreateQuoteAsync(...)` - Tạo Quote mới
- `ISalesDocumentService.ConvertQuoteToOrderAsync(quoteId)` - Chuyển Quote thành Order
- `ISalesDocumentService.UpdateSalesDocumentStatusAsync(id, status)` - Cập nhật trạng thái

### **Payment Operations:**
- `IPaymentService.GetPaymentsBySalesDocumentIdAsync(salesDocumentId)` - Lấy lịch sử thanh toán
- `IPaymentService.GetTotalPaidAmountAsync(salesDocumentId)` - Lấy tổng đã thanh toán
- `IPaymentService.CreatePaymentAsync(...)` - Tạo payment mới (tự động update status)

### **Delivery Operations:**
- `IDeliveryService.GetDeliveryBySalesDocumentIdAsync(salesDocumentId)` - Lấy delivery
- `IDeliveryService.CreateOrUpdateDeliveryAsync(...)` - Tạo/cập nhật lịch giao
- `IDeliveryService.MarkDeliveryAsDeliveredAsync(...)` - Đánh dấu đã giao (tự động update status)

### **Customer Operations:**
- `ICustomerService.GetAllCustomersAsync()` - Lấy tất cả khách hàng
- `ICustomerService.GetCustomerByIdAsync(id)` - Lấy theo ID
- `ICustomerService.SearchCustomersAsync(searchTerm)` - Tìm kiếm
- `ICustomerService.CreateCustomerAsync(customer)` - Tạo mới
- `ICustomerService.UpdateCustomerAsync(customer)` - Cập nhật

### **Dealer Operations:**
- `IDealerService.GetAllDealersAsync()` - Lấy tất cả đại lý
- `IDealerService.GetDealerByIdAsync(id)` - Lấy theo ID
- `IDealerService.GetActiveDealersAsync()` - Lấy đại lý active

---

## 🔧 **VÍ DỤ REFACTOR CỤ THỂ:**

### **Example 1: Dealer/Vehicles.cshtml.cs**

**Cần inject:**
- `IVehicleService` - Lấy danh sách xe
- `IPricePolicyService` - Lấy giá theo dealer
- `IStockService` - Lấy tồn kho EVM

**Code:**
```csharp
private readonly IVehicleService _vehicleService;
private readonly IPricePolicyService _pricePolicyService;
private readonly IStockService _stockService;

// Thay thế:
var vehicles = await _context.Vehicles.Where(...).ToListAsync();
// Bằng:
var vehicles = await _vehicleService.GetAvailableVehiclesAsync();

// Thay thế:
var pricePolicy = await _context.PricePolicies.Where(...).FirstOrDefaultAsync();
// Bằng:
var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, dealerIdInt);

// Thay thế:
var stocks = await _context.Stocks.Where(...).ToListAsync();
// Bằng:
var stocks = await _stockService.GetAvailableStocksByVehicleIdAsync(vehicle.Id, "EVM");
```

### **Example 2: Dealer/Sales/CreateQuote.cshtml.cs**

**Cần inject:**
- `ISalesDocumentService` - Tạo Quote
- `IVehicleService` - Lấy danh sách xe
- `IPricePolicyService` - Lấy giá
- `ICustomerService` - Lấy danh sách khách hàng

**Code:**
```csharp
// Tạo Quote:
var quote = await _salesDocumentService.CreateQuoteAsync(dealerIdInt, customerId, userIdInt, promotionId);

// Tạo Line Item (cần access DbContext cho SalesDocumentLine):
_context.SalesDocumentLines.Add(new SalesDocumentLine { ... });
await _context.SaveChangesAsync();
```

**Lưu ý:** `SalesDocumentLine` chưa có Service riêng, tạm thời vẫn dùng `_context` trực tiếp.

### **Example 3: Dealer/Sales/Orders.cshtml.cs**

**Cần inject:**
- `ISalesDocumentService` - Lấy danh sách Orders

**Code:**
```csharp
var orders = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(
    dealerIdInt, 
    type: "ORDER", 
    status: StatusFilter != "all" ? StatusFilter : null);
```

---

## ⚠️ **LƯU Ý:**

1. **SalesDocumentLine:** Chưa có Service riêng, tạm thời vẫn dùng `_context.SalesDocumentLines` trực tiếp khi cần thao tác CRUD.

2. **Complex Queries:** Một số query phức tạp có thể cần giữ `_context` tạm thời, nhưng ưu tiên dùng Services trước.

3. **Auto Status Update:**
   - `PaymentService.CreatePaymentAsync()` tự động update status Order (PAID/PARTIAL_PAID)
   - `DeliveryService.MarkDeliveryAsDeliveredAsync()` tự động update status Order (DELIVERED)

4. **Dependency Injection:** Tất cả Services đã được register trong `Program.cs`, chỉ cần inject vào constructor.

---

## 📝 **CHECKLIST REFACTOR:**

Cho mỗi Page cần refactor:

- [ ] Xác định Services cần inject
- [ ] Thay thế `ApplicationDbContext _context` bằng Services
- [ ] Thay thế các query `_context.Entity` bằng Service methods
- [ ] Xóa `using Microsoft.EntityFrameworkCore;` nếu không cần
- [ ] Thêm `using Vehicle_Dealer_Management.BLL.IService;`
- [ ] Test lại Page hoạt động đúng

---

## 🎯 **PAGES ƯU TIÊN REFACTOR:**

### **Priority 1 (Sales Pages - Quan trọng nhất):**
1. `Dealer/Sales/CreateQuote.cshtml.cs`
2. `Dealer/Sales/Quotes.cshtml.cs`
3. `Dealer/Sales/Orders.cshtml.cs`
4. `Dealer/Sales/QuoteDetail.cshtml.cs`
5. `Dealer/Sales/OrderDetail.cshtml.cs`

### **Priority 2 (Vehicle Pages):**
6. `Dealer/Vehicles.cshtml.cs`
7. `Dealer/VehicleDetail.cshtml.cs`
8. `Customer/VehicleDetail.cshtml.cs`
9. `EVM/Vehicles/Create.cshtml.cs`
10. `EVM/Vehicles/Edit.cshtml.cs`

### **Priority 3 (Other Pages):**
11. `EVM/PricePolicies.cshtml.cs`
12. `EVM/Stocks.cshtml.cs`
13. `Dealer/Customers.cshtml.cs`
14. Các Dashboard pages
15. Các Report pages

---

**Last Updated:** 2025-01-XX  
**Version:** 1.0

