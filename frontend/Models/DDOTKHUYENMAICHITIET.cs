using System;

namespace QuanLyBar.Client.Models
{
    public class DDOTKHUYENMAICHITIET
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? DdotkhuyenmaiId { get; set; }
        public int? DnhommathangId { get; set; }
        public decimal? Tilegiamgia { get; set; }
        public int? DmathangId { get; set; }
        public decimal? Giaban { get; set; }
        public decimal? Giatridonhang { get; set; }
        public decimal? Soluongmathang { get; set; }
        public decimal? Soluongmua { get; set; }
        public decimal? Soluongtang { get; set; }
        public int? DmathangtangId { get; set; }
    }
}
