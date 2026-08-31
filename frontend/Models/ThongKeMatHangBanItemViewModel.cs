using System;

namespace QuanLyBar.Client.Models
{
    public class ThongKeMatHangBanItemViewModel
    {
        public int Stt { get; set; }
        public string SttStr => Stt.ToString("D2");
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string Dvt { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal TienGiam { get; set; }
        public decimal ThanhTienBan { get; set; }
        public decimal GiaVon { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTienNhap { get; set; }
        public decimal Lai { get; set; }
        public decimal TiLeLai { get; set; }
        public string NhomId { get; set; }
    }
}
