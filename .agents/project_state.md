# Project State & WBS (Work Breakdown Structure)
*Dự án Backend API Quản lý Nhà hàng/Bar (Node.js, Express, Sequelize)*

> **STRICT RULE FOR AGENT:** BẮT BUỘC phải đọc file này để nắm ngữ cảnh trước khi viết code bất kỳ module API nào. Sau khi hoàn thành một module, BẮT BUỘC phải tự động cập nhật trạng thái tương ứng tại file này.

## Tình trạng các Module API

### 1. Module Hệ thống & Xác thực (Auth)
- [x] Authentication API (Login, Token generation)
- [x] Phân quyền (Role-based access)
- [ ] Quản lý Cửa hàng & Thiết bị

### 2. Module Vận hành Cốt lõi (Core Operations)
- [x] Quản lý Khu vực & Sơ đồ Bàn (Tables & Areas)
- [x] Danh mục Thực đơn (Menu Items & Categories)
- [ ] Giá bán & Định lượng (Pricing & Recipes)

### 3. Module Giao dịch & Bán hàng (Transactions)
- [x] Khách đặt hàng (Pre-orders / Reservations)
- [ ] Xử lý Hóa đơn & Thanh toán (Orders & Payments)
- [x] Thêm món, Giảm giá & Gộp/Chuyển Bàn (POS Operations)
- [ ] Đặt hàng trước (Pre-orders)
- [ ] Khuyến mãi & Giảm giá (Promotions)
- [ ] Thanh toán Hóa đơn (Checkout & Payment)

### 4. Module Khách hàng & Nhân sự (Entities)
- [ ] Quản lý Khách hàng & Thẻ trả trước (Customers & Prepaid Cards)
- [ ] Quản lý Nhân sự & Chấm công (HR & Timesheets)

### 5. Module Kho hàng & Chuỗi cung ứng (Inventory)
- [ ] Quản lý Nhà cung cấp (Suppliers)
- [ ] Phiếu Nhập & Xuất Kho (In/Out Inventory)
- [ ] Kiểm kê & Tồn kho (Stock check & Balances)

### 6. Module Tài chính & Báo cáo (Finance & Reports)
- [ ] Quỹ Thu Chi (Cash Fund)
- [ ] Công nợ (Debts)
- [ ] Báo cáo Doanh thu & Tồn kho (Reports)
