# Requirements (Condensed)

## 1. Dealer (Dealer Staff – Dealer Manager)

### a. Truy vấn thông tin xe (Dealer Staff)
- Xem danh mục, cấu hình, giá bán.
- So sánh mẫu xe và tính năng.

### b. Quản lý bán hàng (Dealer Staff)
- Tạo báo giá, đơn hàng, hợp đồng.
- Quản lý khuyến mãi.
- Đặt xe từ hãng, theo dõi giao xe.
- Quản lý thanh toán (trả thẳng, trả góp).

### c. Quản lý khách hàng (Dealer Staff – Dealer Manager)
- Lưu trữ hồ sơ khách.
- Quản lý lịch hẹn lái thử.
- Ghi nhận phản hồi & xử lý khiếu nại.

### d. Báo cáo (Dealer Manager)
- Doanh số theo nhân viên.
- Báo cáo công nợ khách hàng & hãng.

---

## 2. Hãng xe (EVM Staff – Admin)

### a. Quản lý sản phẩm & phân phối (EVM Staff)
- Danh mục xe: mẫu, phiên bản, màu sắc.
- Tồn kho & điều phối đến đại lý.
- Giá sỉ, chiết khấu, khuyến mãi.

### b. Quản lý đại lý (EVM Staff)
- Hợp đồng, chỉ tiêu doanh số, công nợ.
- Tài khoản đại lý trên hệ thống.

### c. Báo cáo & phân tích (Admin)
- Doanh số theo khu vực, đại lý.
- Phân tích tồn kho & tốc độ tiêu thụ.

---

## 3. Customer

### a. Khám phá & mua xe
- Xem danh mục, cấu hình, giá bán, so sánh.
- Gửi yêu cầu báo giá / đặt lịch lái thử.
- Theo dõi trạng thái đơn hàng & giao xe.

### b. Tài khoản & phản hồi
- Đăng ký, đăng nhập, cập nhật thông tin.
- Gửi feedback/complaint.

---

## Tóm tắt Roles

Hệ thống có **5 roles** chính:

1. **CUSTOMER** (mặc định)
   - Khách hàng mua xe
   - Xem catalog, yêu cầu báo giá, đặt lịch lái thử
   - Theo dõi đơn hàng, gửi feedback

2. **DEALER_STAFF**
   - Nhân viên bán hàng tại đại lý
   - Truy vấn xe, tạo báo giá/đơn hàng
   - Quản lý khách hàng, lịch lái thử

3. **DEALER_MANAGER**
   - Quản lý đại lý
   - Xem báo cáo doanh số theo nhân viên
   - Báo cáo công nợ

4. **EVM_STAFF**
   - Nhân viên hãng xe
   - Quản lý sản phẩm, tồn kho, giá
   - Quản lý đại lý, xử lý đơn đặt hàng từ đại lý

5. **EVM_ADMIN**
   - Quản trị viên hệ thống
   - Phân tích doanh số, tồn kho
   - Cấu hình hệ thống

---

## Workflow chính

### Sales Flow
```
Catalog → Quote → Order/Contract → Payment → Delivery
```

### Inventory Flow
```
EVM Stock → Dealer Order → Approval → Transfer to Dealer → Sale → Deduct Stock
```

### Customer Journey
```
Browse Catalog → Request Quote → Test Drive → Order → Payment → Delivery Tracking
```

---

Xem thêm:
- **[Roadmap.md](Roadmap.md)** - Lộ trình triển khai chi tiết
- **[database.md](database.md)** - Thiết kế database (15 tables)

