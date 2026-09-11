using System;
using System.Collections.Generic;

namespace QuanLyBar.Client.Services
{
    /// <summary>Central Vietnamese labels for report parameters and raw-data columns.</summary>
    public static class ReportTextLocalizationService
    {
        private static readonly Dictionary<string, string> ColumnLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["GHICHU"] = "Ghi chú", ["NOTE"] = "Ghi chú",
            ["SOPHIEU"] = "Số phiếu", ["NGAY"] = "Ngày", ["SOHD"] = "Số hóa đơn", ["SOHOADON"] = "Số hóa đơn",
            ["TONGCONG"] = "Tổng cộng", ["TONGCONGBAN"] = "Tổng cộng",
            ["PHIVANCHUYEN"] = "Phí vận chuyển", ["TIENGIAMGIA"] = "Tiền giảm giá", ["GIAMGIA"] = "Tiền giảm giá",
            ["TILEGIAMGIA"] = "Tỷ lệ giảm giá (%)", ["CKPHANTRAM"] = "Tỷ lệ giảm giá (%)",
            ["TIENTHUE"] = "Tiền thuế", ["TILETHUE"] = "Tỷ lệ thuế (%)", ["TIENHANG"] = "Tiền hàng",
            ["NHACUNGCAP"] = "Nhà cung cấp", ["TENNCC"] = "Nhà cung cấp", ["MANCC"] = "Mã nhà cung cấp",
            ["KHONHAP"] = "Kho nhập", ["KHOXUAT"] = "Kho xuất", ["KHONHAN"] = "Kho nhận", ["KHOHANG"] = "Kho hàng",
            ["BANKHUVUC"] = "Khu vực", ["KHUVUC"] = "Khu vực", ["BANPHONG"] = "Bàn / Phòng", ["TENBAN"] = "Bàn / Phòng", ["MABAN"] = "Mã bàn",
            ["THUNGAN"] = "Thu ngân", ["NHANVIEN"] = "Nhân viên", ["NHANVIENBAN"] = "Nhân viên bán hàng",
            ["NHANVIENNHAP"] = "Nhân viên nhập", ["NHANVIENXUAT"] = "Nhân viên xuất", ["NHANVIENPHUCVU"] = "Nhân viên phục vụ",
            ["NHANVIENKIEMKE"] = "Nhân viên kiểm kê", ["KHACHHANG"] = "Khách hàng", ["TENKHACH"] = "Khách hàng",
            ["TENKHACHHANG"] = "Khách hàng", ["MAKHACH"] = "Mã khách hàng",
            ["THANHTOAN"] = "Thanh toán", ["TIENMAT"] = "Tiền mặt", ["CHUYENKHOAN"] = "Chuyển khoản",
            ["THE"] = "Thẻ", ["THETT"] = "Thẻ thanh toán", ["DOITRA"] = "Đổi trả",
            ["LYDO"] = "Lý do", ["LYDOTHUCHI"] = "Lý do thu chi", ["LYDOTHU"] = "Lý do thu", ["LYDOCHI"] = "Lý do chi", ["LYDOHUY"] = "Lý do hủy",
            ["NGUOINOP"] = "Người nộp", ["NGUOINHAN"] = "Người nhận", ["SOTIEN"] = "Số tiền",
            ["SOTIENTHU"] = "Số tiền thu", ["THU"] = "Số tiền thu", ["SOTIENCHI"] = "Số tiền chi", ["CHI"] = "Số tiền chi",
            ["CUAHANG"] = "Cửa hàng", ["MAHANG"] = "Mã hàng", ["MATHANG"] = "Mặt hàng", ["TENHANG"] = "Mặt hàng", ["TENMATHANG"] = "Mặt hàng",
            ["NGUYENLIEU"] = "Nguyên liệu", ["DVT"] = "ĐVT", ["DONVITINH"] = "Đơn vị tính",
            ["SOLUONG"] = "Số lượng", ["SOLUONGMON"] = "Số lượng", ["SOLUONGNHAP"] = "Số lượng nhập", ["SOLUONGXUAT"] = "Số lượng xuất", ["SOLUONGDON"] = "Số đơn",
            ["DONGIA"] = "Đơn giá", ["DONGIABAN"] = "Đơn giá", ["DONGIAVON"] = "Đơn giá vốn",
            ["THANHTIEN"] = "Thành tiền", ["THANHTIENBAN"] = "Thành tiền", ["GIATRIVON"] = "Tiền vốn", ["TIENVON"] = "Tiền vốn",
            ["LAIGOP"] = "Lãi gộp", ["LOINHUAN"] = "Lợi nhuận", ["TILELAI"] = "Tỷ lệ lãi (%)",
            ["DOANHTHU"] = "Doanh thu", ["TONGDOANHSO"] = "Doanh thu", ["DOANHSO"] = "Doanh thu",
            ["TONKHO"] = "Tồn kho", ["TONDAU"] = "Số dư đầu", ["NODAUKY"] = "Số dư đầu", ["CONGNODAU"] = "Số dư đầu",
            ["TONCUOI"] = "Số dư cuối", ["NOCUOIKY"] = "Số dư cuối", ["CONGNOCUOI"] = "Số dư cuối",
            ["NHAPTRONGKY"] = "Phát sinh tăng", ["PHATSINHMUA"] = "Phát sinh tăng", ["GHINO"] = "Phát sinh tăng",
            ["XUATTRONGKY"] = "Phát sinh giảm", ["GHICO"] = "Phát sinh giảm", ["SODU"] = "Công nợ", ["CONGNO"] = "Công nợ", ["TONGNO"] = "Công nợ",
            ["TONSOSACH"] = "Tồn sổ sách", ["TONTHUCTE"] = "Tồn thực tế", ["CHENHLECH"] = "Chênh lệch", ["TIENTON"] = "Giá trị tồn",
            ["GIAMUA"] = "Giá mua", ["GIABAN"] = "Giá bán", ["NHOMHANG"] = "Nhóm hàng", ["TENNHOM"] = "Nhóm hàng", ["TENNHOMHANG"] = "Nhóm hàng",
            ["NHOMKHACH"] = "Nhóm khách hàng", ["NHOMNCC"] = "Nhóm nhà cung cấp", ["DIENTHOAI"] = "Điện thoại", ["DIACHI"] = "Địa chỉ", ["EMAIL"] = "Email",
            ["HANDUNG"] = "Hạn dùng", ["SONGAYCONLAI"] = "Số ngày còn lại", ["KHUNGGIO"] = "Khung giờ", ["GIOKHACHVAO"] = "Khung giờ",
            ["GIO"] = "Giờ", ["BATDAU"] = "Bắt đầu", ["KETTHUC"] = "Kết thúc", ["TENDOITUONG"] = "Tên đối tượng",
            ["CHUNGTUGOC"] = "Chứng từ gốc", ["DIENGIAI"] = "Diễn giải", ["TAOBOI"] = "Tạo bởi", ["THOIGIAN"] = "Thời gian",
            ["DOAN"] = "Đồ ăn", ["DOUONG"] = "Đồ uống", ["DICHVU"] = "Dịch vụ", ["DOKHAC"] = "Đồ khác",
            ["HOAHONG"] = "Hoa hồng", ["TAIKHOAN"] = "Tài khoản", ["THIETBI"] = "Thiết bị", ["THAOTAC"] = "Thao tác", ["TRANGTHAI"] = "Trạng thái",
            ["TYLE"] = "Tỷ lệ (%)", ["TILEPHANTRAM"] = "Tỷ lệ (%)", ["THANG"] = "Tháng", ["NAM"] = "Năm",
            ["MAKM"] = "Mã khuyến mại", ["TENKM"] = "Tên khuyến mại", ["GIATRIKM"] = "Giá trị khuyến mại", ["DINHLUONG"] = "Định lượng",
            ["HANGSANXUAT"] = "Hãng sản xuất", ["NGAYSINH"] = "Ngày sinh", ["NHOMNGUOIDUNG"] = "Nhóm người dùng",
            ["TENNGUOIDUNG"] = "Tên người dùng", ["TENDANGNHAP"] = "Tên đăng nhập"
        };

        public static string GetColumnLabel(string? fieldName)
        {
            string raw = fieldName?.Trim() ?? "";
            string key = Normalize(raw);
            return ColumnLabels.TryGetValue(key, out string? label) ? label : raw;
        }

        public static string GetParameterLabel(string? code)
        {
            string raw = code?.Trim() ?? "";
            string c = Normalize(raw);
            if (c.Contains("NGAY")) return "Khoảng thời gian";
            if (c.Contains("GIOTHANHTOAN")) return "Giờ thanh toán";
            if (c.Contains("GIOTINHLUONG")) return "Giờ tính lương";
            if (c.Contains("BATDAU")) return "Thời gian bắt đầu";
            if (c.Contains("CHUCNANG")) return "Chức năng";
            if (c.Contains("DIENGIAI")) return "Diễn giải";
            if (c.Contains("CHUNGTUGOC")) return "Chứng từ gốc";
            if (c.Contains("TENDOITUONG")) return "Tên đối tượng";
            if (c.Contains("DIACHI")) return "Địa chỉ";
            if (c.Contains("NOTE") || c.Contains("GHICHU")) return "Ghi chú";
            if (c.Contains("GIAOHANG")) return "Giao hàng";
            if (c.Contains("DNHANVIENGIAO")) return "Nhân viên giao hàng";
            if (c.Contains("NHOMHIENTHI")) return "Nhóm hiển thị";
            if (c.Contains("KHOXUAT")) return "Kho xuất";
            if (c.Contains("KHONHAP")) return "Kho nhập";
            if (c.Contains("KHOHANG")) return "Kho hàng";
            if (c.Contains("CUAHANG")) return "Cửa hàng";
            if (c.Contains("NHANVIENNHAP")) return "Nhân viên nhập";
            if (c.Contains("NHANVIENXUAT")) return "Nhân viên xuất";
            if (c.Contains("USERTHANHTOAN") || c.Contains("THUNGAN")) return "Thu ngân";
            if (c.Contains("NHANVIEN")) return "Nhân viên";
            if (c.Contains("NHOMKHACH")) return "Nhóm khách hàng";
            if (c.Contains("KHACHHANG")) return "Khách hàng";
            if (c.Contains("NHOMNHACUNGCAP") || c.Contains("NHOMNCC")) return "Nhóm nhà cung cấp";
            if (c.Contains("NHACUNGCAP")) return "Nhà cung cấp";
            if (c.Contains("LOAIMATHANG")) return "Loại mặt hàng";
            if (c.Contains("NHOMMATHANG")) return "Nhóm mặt hàng";
            if (c.Contains("MATHANG")) return "Mặt hàng";
            if (c.Contains("HANGSANXUAT")) return "Hãng sản xuất";
            if (c.Contains("LYDOTHUCHI")) return "Lý do thu chi";
            if (c.Contains("TAIKHOANNGANHANG")) return "Tài khoản ngân hàng";
            if (c.Contains("KHUVUC")) return "Khu vực";
            if (c.Contains("BAN")) return "Bàn / Phòng";
            if (c.Contains("GROUPUSER")) return "Nhóm người dùng";
            if (c.Contains("SUSERNAME")) return "Người dùng";
            if (c.Contains("THETRATRUOC")) return "Thẻ trả trước";
            if (c.Contains("VOUCHER")) return "Voucher";
            if (c.Contains("TDATHANGID")) return "Đơn đặt hàng";
            if (c.Contains("TDONHANGNAME")) return "Số phiếu";
            if (c.Contains("SOPHIEU")) return "Số phiếu";
            return raw;
        }

        private static string Normalize(string value) =>
            value.Replace(".", "").Replace("_", "").Replace(" ", "").ToUpperInvariant();
    }
}
