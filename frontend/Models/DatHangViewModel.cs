using System;

namespace QuanLyBar.Client.Models
{
    public class DatHangViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; }
        public DateTime? Ngay { get; set; }
        public string SoPhieu { get; set; } // Map từ Name
        public string TenKhach { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string TongCong { get; set; }
        
        public string PhuongThucDatName { get; set; }
        public string MucDichDatName { get; set; }
        
        public DateTime? TuGio { get; set; }
        public DateTime? DenGio { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
    }
}
