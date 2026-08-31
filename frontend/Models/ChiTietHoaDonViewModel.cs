using System;

namespace QuanLyBar.Client.Models
{
    public class ChiTietHoaDonViewModel
    {
        public string Id { get; set; }
        public string MatHangId { get; set; }
        public string MaHang { get; set; }
        public int Stt { get; set; }
        public string TenMon { get; set; }
        public string Dvt { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal PhanTramGiamGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string GhiChu { get; set; }
    }
}
