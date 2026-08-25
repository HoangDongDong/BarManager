using System;

namespace QuanLyBar.Client.Models
{
    public class DKHACHHANG
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? DnhomkhachhangId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Makhach { get; set; }
        public decimal? Diachi { get; set; }
        public string Dienthoai { get; set; }
        public string Email { get; set; }
        public decimal? Masothue { get; set; }
        public int? DnhanvienId { get; set; }
        public DateTime? Ngaysinh { get; set; }
        public decimal? Diemtichluybandau { get; set; }
        public decimal? Giaban { get; set; }
        public int? DtinhthanhId { get; set; }
        public string Facebook { get; set; }
        public int? DthetratruocId { get; set; }
    }
}
