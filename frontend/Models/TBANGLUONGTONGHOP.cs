using System;

namespace QuanLyBar.Client.Models
{
    public class TBANGLUONGTONGHOP
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? DnhanvienId { get; set; }
        public string Luongca { get; set; }
        public string Tongluong { get; set; }
        public string Phat { get; set; }
        public string Tamung { get; set; }
        public decimal? Thucnhan { get; set; }
        public decimal? Thuong { get; set; }
        public string Luongthang { get; set; }
        public string Cachtinhluong { get; set; }
        public int? TbangluongId { get; set; }
        public DateTime? Sogiolam { get; set; }
        public DateTime? Sogiotangca { get; set; }
        public string Luongtheoca { get; set; }
    }
}
