# Kiến trúc Hệ thống & Cơ sở dữ liệu

> **Agent phải luôn đọc file này trước khi bắt đầu code một module API mới để duy trì độ chính xác của ngữ cảnh.**

Tài liệu này tóm tắt các bảng cơ sở dữ liệu quan trọng nhất làm nền tảng cho hệ thống Quản lý Nhà hàng/Bar. Toàn bộ kiến trúc xoay quanh các module chính này.

## 1. Bảng `cua_hang` (Core System)
Bảng nền tảng, gốc rễ của mọi dữ liệu. Hệ thống hỗ trợ đa chi nhánh.
- **Vai trò:** Lưu thông tin cửa hàng/chi nhánh (Tên, địa chỉ, số điện thoại, mã số thuế).
- **Mối quan hệ:** Liên kết một-nhiều với hầu hết các bảng khác trong hệ thống (`tai_khoan`, `ban`, `hoa_don`, `kho_hang`).

## 2. Bảng `tai_khoan` (Authentication & Authorization)
Quản lý người dùng, định danh và phân quyền thiết bị/nhân viên.
- **Vai trò:** Lưu trữ thông tin đăng nhập, mật khẩu, và vai trò (Admin, Quản lý, Phục vụ, Thu ngân, Bếp).
- **Logic quan trọng:** Vai trò Admin kế thừa toàn bộ nghiệp vụ của Staff (xem `.windsurfrules`).
- **Mối quan hệ:** Thuộc về `cua_hang`. Liên kết chặt chẽ với bảng `nhan_vien`.

## 3. Bảng `ban` (Physical Layout & State)
Quản lý thực thể vật lý nơi diễn ra giao dịch.
- **Vai trò:** Lưu thông tin từng bàn, thuộc khu vực nào (Tầng 1, Tầng 2, VIP). Quản lý trạng thái trực tiếp của bàn (`trong`, `dang_su_dung`, `dat_truoc`, `don_dep`).
- **Mối quan hệ:** Một bàn có thể có nhiều `hoa_don`.

## 4. Bảng `hoa_don` (Core Transaction)
Trái tim của hệ thống POS. Mọi hoạt động kinh doanh đều quy tụ về đây.
- **Vai trò:** Lưu trữ thông tin tổng quan của một lượt khách: Tại bàn nào? Khách nào? Ai phục vụ? Tổng tiền, tiền giảm giá, thuế, tiền khách đưa, hình thức thanh toán. Trạng thái hóa đơn (`dang_phuc_vu`, `cho_thanh_toan`, `da_thanh_toan`, `huy`).
- **Mối quan hệ:** Gắn liền với `ban`, `khach_hang`, `nhan_vien`, `tai_khoan`, `thiet_bi`.

## 5. Bảng `hoa_don_chi_tiet` (Transaction Details)
Chi tiết món ăn/dịch vụ của hóa đơn.
- **Vai trò:** Lưu trữ chi tiết từng món mà khách gọi: Mã món (`mat_hang_id`), tên món tại thời điểm gọi (snapshot để tránh mất dữ liệu nếu món bị đổi tên sau này), số lượng, đơn giá, chiết khấu, thành tiền. Quản lý trạng thái món (`dang_cho`, `dang_che_bien`, `da_phuc_vu`).
- **Mối quan hệ:** Nằm trong `hoa_don`, liên kết lấy thông tin gốc từ `mat_hang`.
