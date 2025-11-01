# Database Logic (15 tables, single-role per account)

## A. Auth & Tổ chức

### 1) roles
- `id` PK
- `code` UNIQUE (CUSTOMER, DEALER_STAFF, DEALER_MANAGER, EVM_STAFF, EVM_ADMIN)
- `name` (Tên hiển thị)
- `is_operational` BOOL (Có phải role hoạt động không)

### 2) users
- `id` PK
- `email` UNIQUE
- `password_hash`
- `full_name`
- `phone`
- `role_id` FK → roles **DEFAULT=CUSTOMER**
- `dealer_id` FK → dealers NULLABLE
- `created_at`

**Ràng buộc:**
- DEALER_* roles bắt buộc `dealer_id` NOT NULL
- EVM_* roles `dealer_id` = NULL
- CUSTOMER `dealer_id` = NULL

### 3) dealers
- `id` PK
- `name`
- `code` UNIQUE (Mã đại lý)
- `address`
- `status` (ACTIVE, INACTIVE, SUSPENDED)

---

## B. Sản phẩm, giá & phân phối

### 4) vehicles
- `id` PK
- `model_name` (Tên mẫu xe, ví dụ: "Model S", "Model 3")
- `variant_name` (Phiên bản, ví dụ: "Standard", "Premium", "Performance")
- `spec_json` (JSON chứa thông số kỹ thuật: động cơ, pin, tốc độ, v.v.)
- `image_url` NOT NULL (URL ảnh xe)
- `status` (AVAILABLE, DISCONTINUED, COMING_SOON)

**Ràng buộc:**
- UNIQUE(`model_name`, `variant_name`)

### 5) price_policies
- `id` PK
- `vehicle_id` FK → vehicles
- `dealer_id` FK → dealers NULLABLE (NULL = giá chung, có giá trị = giá riêng cho đại lý)
- `msrp` (Manufacturer's Suggested Retail Price - Giá niêm yết)
- `wholesale_price` NULLABLE (Giá sỉ cho đại lý)
- `discount_rule_json` (JSON chứa quy tắc giảm giá)
- `valid_from` (Ngày bắt đầu hiệu lực)
- `valid_to` NULLABLE (Ngày kết thúc hiệu lực)

**Ràng buộc:**
- **No overlap** theo date-range: không được có 2 policy cùng `vehicle_id`, `dealer_id` trùng thời gian
- Cần CHECK constraint hoặc validation trong service layer

### 6) stocks
- `id` PK
- `owner_type` ENUM(EVM, DEALER) (Chủ sở hữu: Hãng xe hoặc Đại lý)
- `owner_id` (ID của EVM hoặc Dealer)
- `vehicle_id` FK → vehicles
- `color_code` (Mã màu, ví dụ: "RED", "BLUE", "BLACK")
- `qty` ≥ 0 (Số lượng tồn kho)

**Ràng buộc:**
- UNIQUE(`owner_type`, `owner_id`, `vehicle_id`, `color_code`)
- `qty` phải ≥ 0

### 7) dealer_orders
- `id` PK
- `dealer_id` FK → dealers
- `status` ENUM(DRAFT, SUBMITTED, APPROVED, REJECTED, FULFILLING, CLOSED)
- `items_json[]` (JSON array chứa các item: vehicle_id, color_code, qty)
- `created_by` FK → users
- `approved_by` FK → users NULLABLE (EVM_STAFF hoặc EVM_ADMIN)
- `created_at`
- `updated_at`
- `approved_at` NULLABLE
- `fulfilled_at` NULLABLE

**State Machine:**
- DRAFT → SUBMITTED → APPROVED/REJECTED → FULFILLING → CLOSED
- Hoặc: DRAFT → SUBMITTED → REJECTED (end)

---

## C. Bán hàng tại đại lý

### 8) sales_documents
- `id` PK
- `type` ENUM(QUOTE, ORDER, CONTRACT)
- `dealer_id` FK → dealers
- `customer_id` FK → customer_profiles
- `status` (Xem state machine bên dưới)
- `promotion_id` FK → promotions NULLABLE
- `signed_at` NULLABLE (Ngày ký hợp đồng)
- `created_at`
- `updated_at`
- `created_by` FK → users (Dealer Staff tạo)

**State Machine:**

**QUOTE:**
- DRAFT → SENT → ACCEPTED/EXPIRED/REJECTED

**ORDER:**
- OPEN → PARTIALLY_PAID → PAID → READY_TO_DELIVER → DELIVERED → CLOSED
- Hoặc: OPEN → CANCELLED

**CONTRACT:**
- DRAFT → SIGNED → ACTIVE
- Gắn với ORDER đã được chấp thuận

### 9) sales_document_lines
- `id` PK
- `sales_document_id` FK → sales_documents
- `vehicle_id` FK → vehicles
- `color_code`
- `qty` > 0
- `unit_price` ≥ 0 (Giá đơn vị tại thời điểm bán)
- `discount_value` ≥ 0 (Tổng giá trị giảm giá cho dòng này)

**Ràng buộc:**
- CHECK `discount_value ≤ unit_price * qty`

### 10) payments
- `id` PK
- `sales_document_id` FK → sales_documents
- `method` ENUM(CASH, FINANCE) (Tiền mặt hoặc Trả góp)
- `amount` > 0
- `meta_json` (JSON chứa thông tin bổ sung: số thẻ, ngân hàng, v.v.)
- `paid_at` (Thời điểm thanh toán)

**Logic:**
- Auto cập nhật tổng đã thanh toán của sales_document
- Khi tổng thanh toán ≥ tổng giá trị → chuyển ORDER sang PAID/READY_TO_DELIVER

### 11) deliveries
- `id` PK
- `sales_document_id` FK → sales_documents
- `scheduled_date` (Ngày hẹn giao xe)
- `delivered_date` NULLABLE (Ngày thực tế giao xe)
- `status` (SCHEDULED, IN_TRANSIT, DELIVERED, CANCELLED)
- `handover_note` NULLABLE (Ghi chú giao xe)

**Logic:**
- Khi `delivered_date` được set → update ORDER status = DELIVERED
- Trừ số lượng tồn kho (stocks) của dealer

### 12) promotions
- `id` PK
- `name`
- `scope` ENUM(GLOBAL, DEALER, VEHICLE)
  - GLOBAL: Áp dụng cho tất cả
  - DEALER: Áp dụng cho đại lý cụ thể
  - VEHICLE: Áp dụng cho mẫu xe cụ thể
- `dealer_id` FK → dealers NULLABLE (Required nếu scope = DEALER)
- `vehicle_id` FK → vehicles NULLABLE (Required nếu scope = VEHICLE)
- `rule_json` (JSON chứa quy tắc: giảm %, giảm số tiền, điều kiện, v.v.)
- `valid_from`
- `valid_to` (Có thể NULL nếu không có hạn)

---

## D. Khách hàng & tương tác

### 13) customer_profiles
- `id` PK
- `user_id` FK → users NULLABLE UNIQUE (Link với account nếu khách đăng ký)
- `full_name`
- `phone` UNIQUE
- `email` UNIQUE
- `address`
- `identity_no` NULLABLE (Số CMND/CCCD)

**Lưu ý:**
- Khách hàng có thể không có account (user_id = NULL)
- Nhưng phone và email phải UNIQUE để tránh trùng lặp

### 14) test_drives
- `id` PK
- `customer_id` FK → customer_profiles
- `dealer_id` FK → dealers
- `vehicle_id` FK → vehicles
- `schedule_time` ≥ now (Thời gian hẹn lái thử)
- `status` ENUM(REQUESTED, CONFIRMED, DONE, CANCELLED)
- `note` NULLABLE
- `created_at`
- `updated_at`

**Ràng buộc:**
- `schedule_time` phải ≥ hiện tại (validation khi tạo)

**State Machine:**
- REQUESTED → CONFIRMED → DONE/CANCELLED
- Hoặc: REQUESTED → CANCELLED

### 15) feedbacks
- `id` PK
- `customer_id` FK → customer_profiles
- `dealer_id` FK → dealers
- `type` ENUM(FEEDBACK, COMPLAINT)
- `status` ENUM(NEW, IN_PROGRESS, RESOLVED, REJECTED)
- `content` (Nội dung phản hồi/khiếu nại)
- `created_at`
- `updated_at`
- `resolved_at` NULLABLE

**State Machine:**
- NEW → IN_PROGRESS → RESOLVED/REJECTED
- Hoặc: NEW → REJECTED

---

## Ràng buộc & State Machine tổng hợp

### Ràng buộc dữ liệu
- `users`: DEALER_* bắt buộc `dealer_id` NOT NULL; EVM_* `dealer_id` NULL
- `price_policies`: Chặn overlap theo `(vehicle_id, dealer_id, date-range)`
- `stocks`: `qty` không âm; unique composite key
- `sales_document_lines`: CHECK `discount_value ≤ unit_price * qty`
- `payments`: `amount > 0`
- `test_drives`: `schedule_time ≥ now()`

### State Machines

**Sales Flow:**
1. QUOTE: DRAFT → SENT → ACCEPTED/EXPIRED/REJECTED
2. ORDER: OPEN → PARTIALLY_PAID → PAID → READY_TO_DELIVER → DELIVERED → CLOSED
3. CONTRACT: DRAFT → SIGNED → ACTIVE
4. Dealer Order: DRAFT → SUBMITTED → APPROVED/REJECTED → FULFILLING → CLOSED

**Payment Logic:**
- Auto cập nhật tổng đã thanh toán
- Đủ thì chuyển ORDER sang PAID/READY_TO_DELIVER

**Delivery Logic:**
- Khi `delivered_date` set → update ORDER = DELIVERED & trừ kho

---

## Index gợi ý

### Auth & Tổ chức
- `users(email)` UNIQUE
- `users(role_id)`
- `users(dealer_id)`
- `dealers(code)` UNIQUE

### Sản phẩm
- `vehicles(model_name, variant_name)` UNIQUE
- `vehicles(status)`
- `price_policies(vehicle_id, dealer_id, valid_from, valid_to)` + EXCLUDE overlap (PostgreSQL)
- `stocks(owner_type, owner_id, vehicle_id, color_code)` UNIQUE
- `dealer_orders(dealer_id, status, created_at)`

### Bán hàng
- `sales_documents(type, dealer_id, status, created_at)`
- `sales_documents(customer_id, created_at)`
- `sales_document_lines(sales_document_id, vehicle_id)`
- `payments(sales_document_id, paid_at)`
- `deliveries(sales_document_id, scheduled_date)`
- `promotions(scope, dealer_id, vehicle_id, valid_from, valid_to)`

### Khách hàng
- `customer_profiles(phone)` UNIQUE
- `customer_profiles(email)` UNIQUE
- `customer_profiles(user_id)` UNIQUE
- `test_drives(dealer_id, schedule_time)`
- `test_drives(customer_id, status)`
- `feedbacks(dealer_id, status)`
- `feedbacks(customer_id, created_at)`

---

## Quan hệ giữa các bảng (Relationships)

```
roles (1) ──< (N) users
dealers (1) ──< (N) users
dealers (1) ──< (N) dealer_orders
dealers (1) ──< (N) sales_documents
dealers (1) ──< (N) stocks (owner_type=DEALER)
dealers (1) ──< (N) test_drives
dealers (1) ──< (N) feedbacks

vehicles (1) ──< (N) price_policies
vehicles (1) ──< (N) stocks
vehicles (1) ──< (N) sales_document_lines
vehicles (1) ──< (N) test_drives

customer_profiles (1) ──< (N) sales_documents
customer_profiles (1) ──< (N) test_drives
customer_profiles (1) ──< (N) feedbacks

sales_documents (1) ──< (N) sales_document_lines
sales_documents (1) ──< (N) payments
sales_documents (1) ──< (1) deliveries

promotions (1) ──< (N) sales_documents
```

---

## Lưu ý implementation

1. **Price Policy Overlap Prevention:**
   - Dùng database CHECK constraint (nếu database hỗ trợ)
   - Hoặc validation trong Service layer
   - Query existing policies trước khi tạo mới

2. **Stock Deduction:**
   - Dùng transaction khi deduct stock
   - Đảm bảo atomic operation

3. **Payment Calculation:**
   - Trigger hoặc service method để auto-update tổng thanh toán
   - Check payment status khi add payment

4. **Multi-tenancy:**
   - DEALER_STAFF chỉ thấy data của dealer mình
   - Filter theo `dealer_id` trong repositories/services

5. **Date Range Validation:**
   - `price_policies`: `valid_from ≤ valid_to` (nếu valid_to không NULL)
   - `promotions`: `valid_from ≤ valid_to` (nếu valid_to không NULL)
   - `test_drives`: `schedule_time ≥ now()`

---

Xem thêm:
- **[requirements.md](requirements.md)** - Yêu cầu chức năng
- **[Roadmap.md](Roadmap.md)** - Lộ trình triển khai

