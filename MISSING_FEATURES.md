# 📋 Danh sách Tính năng Chưa Triển khai

**Ngày kiểm tra:** 2025-01-XX  
**Phương pháp:** So sánh Roadmap.md với codebase thực tế

---

## 🟢 Priority 1: Enhanced Features (Optional - Nice to Have)

### 1. **Create Order Page (Riêng biệt)**
- **Hiện trạng:** Chỉ có Convert Quote to Order
- **Cần làm:**
  - `/Dealer/Sales/CreateOrder` page
  - Tương tự CreateQuote nhưng tạo ORDER trực tiếp
  - Có payment terms
  - Có thể auto-fill từ Quote (optional)

### 2. **Vehicle Comparison Feature**
- **Hiện trạng:** ❌ Chưa có
- **Cần làm:**
  - `/Customer/Compare` hoặc `/Dealer/Compare` page
  - Side-by-side comparison table
  - So sánh: Specs, Pricing, Stock, Features
  - Select multiple vehicles để compare
  - Visual comparison với highlights

### 3. **Promotion Management UI (Đầy đủ)**
- **Hiện trạng:** Chỉ có dropdown trong CreateQuote/EditQuote
- **Cần làm:**
  - `/EVM/Promotions` page (List + CRUD)
  - Create/Edit/Delete promotions
  - Rule editor (JSON hoặc form fields)
  - Preview promotion effect
  - Apply/Remove promotion trong Quote/Order detail pages

---

## 🟡 Priority 2: Missing CRUD Operations

### 4. **Vehicle Delete Functionality**
- **Hiện trạng:** EVM Staff có Create/Edit, thiếu Delete
- **Cần làm:**
  - Delete button trong `/EVM/Vehicles/Index`
  - Confirm dialog
  - Soft delete (set Status = DISCONTINUED) hoặc hard delete
  - Validate: không xóa nếu có Orders/Quotes đang dùng

### 5. **Price Policy Edit/Delete**
- **Hiện trạng:** Chỉ có Create, button Edit có nhưng chưa implement
- **Cần làm:**
  - Edit modal/form trong `/EVM/PricePolicies`
  - Delete functionality
  - Validate date range không overlap

---

## 🔵 Priority 3: Search & Filter Functionality

### 6. **Customer Search/Filter (Functional)**
- **Hiện trạng:** Có UI search box nhưng chưa functional
- **Cần làm:**
  - POST handler hoặc JavaScript filter
  - Search by: Name, Phone, Email
  - Filter by: HasAccount, WithPurchase
  - Real-time search hoặc form submit

### 7. **Quote/Order Advanced Filtering**
- **Hiện trạng:** Có basic filter nhưng có thể enhance
- **Cần làm:**
  - Date range filter
  - Amount range filter
  - Customer search trong filter
  - Multi-select status filter

---

## 🟠 Priority 4: Missing Pages/Views

### 8. **Dealer Detail Page**
- **Hiện trạng:** EVM Staff có Dealer list, thiếu Detail page
- **Cần làm:**
  - `/EVM/Dealers/Detail?id={id}` page
  - Thông tin đại lý: Name, Address, Contact, Status
  - Orders history
  - Debt summary
  - Performance metrics

### 9. **Test Drive Calendar View (Dealer Staff)**
- **Hiện trạng:** Chỉ có list view
- **Cần làm:**
  - Calendar view với date picker
  - Visual calendar hiển thị scheduled test drives
  - Click vào date → show test drives
  - Drag & drop để reschedule (optional)

### 10. **Customer Detail/Profile Page (Dealer Staff)**
- **Hiện trạng:** Customer list có, thiếu detail page
- **Cần làm:**
  - `/Dealer/Customers/Detail?id={id}` page
  - Customer info
  - Orders history
  - Quotes history
  - Test drives history
  - Total spending

---

## 🟣 Priority 5: Enhanced UI/UX

### 11. **Feedback System (Full Implementation)**
- **Hiện trạng:** Có page `/Dealer/Feedback` nhưng cần kiểm tra functionality
- **Cần làm:**
  - Update status (NEW → IN_PROGRESS → RESOLVED)
  - Reply to feedback
  - Close/Resolve action
  - Notes/Comments on feedback

### 12. **Advanced Reports (Charts/Visualizations)**
- **Hiện trạng:** Reports chỉ có tables
- **Cần làm:**
  - Simple charts (bar, line, pie) cho reports
  - Visual data representation
  - Export to Excel/PDF (optional)

### 13. **Customer Export Functionality**
- **Hiện trạng:** ❌ Chưa có
- **Cần làm:**
  - Export customer list to Excel/CSV
  - Filter before export
  - Export selected customers

---

## ⚪ Optional Features (Low Priority)

### 14. **Quote/Order Print/PDF**
- **Hiện trạng:** Có button Print nhưng cần CSS print styles
- **Cần làm:**
  - Print-friendly CSS
  - PDF generation (optional)
  - Professional quote/order templates

### 15. **Bulk Operations**
- **Hiện trạng:** ❌ Chưa có
- **Cần làm:**
  - Bulk update stock quantities
  - Bulk delete vehicles
  - Bulk status update

### 16. **Advanced Notifications**
- **Hiện trạng:** Chỉ có Toast notifications
- **Cần làm:**
  - In-app notifications center
  - Notification history
  - Mark as read/unread

---

## 📊 Tổng kết

### Theo Roadmap Phase:

#### **Phase 3: Dealer Staff UI**
- ✅ Vehicle Catalog & Detail
- ✅ Sales Management (Quote, Order, Payment, Delivery)
- ⚠️ Customer Management (cần search functional)
- ⚠️ Test Drive (cần calendar view)

#### **Phase 4: EVM Staff UI**
- ✅ Vehicle Management (Create/Edit, thiếu Delete)
- ⚠️ Price Policy (cần Edit/Delete)
- ✅ Stock Management
- ⚠️ Dealer Management (cần Detail page)
- ✅ Dealer Order Processing

#### **Phase 4b: EVM Admin UI**
- ✅ Dashboard
- ✅ Reports (tables)
- ⚠️ Reports (có thể thêm charts)

#### **Phase 5: Customer Portal**
- ✅ Vehicles, Quotes, Orders, TestDrive
- ❌ Vehicle Comparison (missing)

---

## 🎯 Đề xuất Ưu tiên Triển khai

### **Nhóm A: Core Features còn thiếu (Nên làm)**
1. ✅ Customer Search/Filter (Functional)
2. ✅ Price Policy Edit/Delete
3. ✅ Vehicle Delete
4. ✅ Dealer Detail Page

### **Nhóm B: Enhanced Features (Nice to have)**
5. Vehicle Comparison
6. Promotion Management UI
7. Test Drive Calendar View
8. Customer Detail Page

### **Nhóm C: Optional (Có thể bỏ qua)**
9. Create Order Page (riêng biệt)
10. Advanced Charts/Reports
11. Export functionality
12. Bulk operations

---

## 📝 Ghi chú

- **Must Have features:** ✅ **100% HOÀN THÀNH**
- **Nice to Have features:** ⚠️ **Một số còn thiếu**
- Tất cả core workflows đã functional và có thể demo được
- Các features còn thiếu chủ yếu là enhancements và optional features

