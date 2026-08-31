using System;

namespace QuanLyBar.Client.Models
{
    public class HoaDonHuyViewModel
    {
        public string Id { get; set; }
        public DateTime? Ngay { get; set; }
        public string SoPhieu { get; set; }
        public string GhiChu { get; set; }
        public string KhachHang { get; set; }
        public string NhanVien { get; set; }
        public string ThuNganHuy { get; set; }
        public DateTime? GioHuy { get; set; }
        public DateTime? NgayHuy { get; set; }
        public decimal? Doitra { get; set; }
        public bool DaThanhToan { get; set; }
        public DateTime? GioThanhToan { get; set; }
        public decimal? TraLai { get; set; }
        public decimal? TienHang { get; set; }
        public decimal? TiLeThue { get; set; }
        public decimal? TienThue { get; set; }
        public decimal? TiLeGiamGia { get; set; }
        public decimal? TienGiamGia { get; set; }
        public string ThanhToanBoi { get; set; }
        public decimal? PhiVanChuyen { get; set; }
        public string LyDoHuy { get; set; }
        public decimal? TienGio { get; set; }
        public decimal? PhiDichVu { get; set; }
    }
}
