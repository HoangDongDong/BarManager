using System;

namespace QuanLyBar.Client.Models
{
    public class HoaDonViewModel
    {
        public string Id { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string Ban { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public DateTime? GioThanhToan { get; set; }
        public decimal TongCong { get; set; }
        public string KhachHang { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TiLeGiamGia { get; set; }
        public decimal TienHang { get; set; }
        public decimal KhachDua { get; set; }
        public decimal TraLai { get; set; }
        public decimal TheThanhToan { get; set; }
        public decimal TienMat { get; set; }
        public int SoKhach { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public string SoOrder { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public string GhiChu { get; set; }
        public string ThanhToanBoi { get; set; }
        public string DiaChi { get; set; }
        public string MaKhach { get; set; }
        public string DienGiai { get; set; }
    }
}
