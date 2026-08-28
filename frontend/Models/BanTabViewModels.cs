using System;

namespace QuanLyBar.Client.Models
{
    public class BangGiaTabViewModel
    {
        public string BangGia { get; set; }
        public decimal? DonGia { get; set; }
        public string GioBatDau { get; set; }
        public string GioKetThuc { get; set; }
        public string GhiChu { get; set; }
    }

    public class DatHangTabViewModel
    {
        public DateTime? Ngay { get; set; }
        public string SoPhieu { get; set; }
        public string TenKhach { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public decimal? TongCong { get; set; }
        public string PhuongThucDat { get; set; }
        public string MucDichDat { get; set; }
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }
    }

    public class HoaDonTabViewModel
    {
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string NhanVien { get; set; }
        public string KhachHang { get; set; }
        public decimal? TongTien { get; set; }
        public decimal? GiamGia { get; set; }
        public decimal? ThanhToan { get; set; }
        public string HinhThuc { get; set; }
    }

    public class KhoTabViewModel
    {
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string KhoHang { get; set; }
        public string NhaCungCap { get; set; }
        public string KhachHang { get; set; }
        public string NhanVien { get; set; }
        public decimal? TongTien { get; set; }
        public decimal? DaThanhToan { get; set; }
        public decimal? ConNo { get; set; }
        public string TuKho { get; set; }
        public string DenKho { get; set; }
        public string DienGiai { get; set; }
    }

    public class KiemKeTabViewModel
    {
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string KhoHang { get; set; }
        public string NhanVien { get; set; }
        public string DienGiai { get; set; }
        public string Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; }
        public decimal? TrichNhanVien { get; set; }
        public string CuaHang { get; set; }
        public decimal? ConLai { get; set; }
        public decimal? ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string MaVoucher { get; set; }
        public string TheTt { get; set; }
    }

    public class SuaChuaTabViewModel
    {
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string Phong { get; set; }
        public bool DaSuaXong { get; set; }
        public string NoiDung { get; set; }
        public string LoaiPhong { get; set; }
        public string NhanVien { get; set; }
        public bool ConSuDungDuoc { get; set; }
    }
}
