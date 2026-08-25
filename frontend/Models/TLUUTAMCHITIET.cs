using System;

namespace QuanLyBar.Client.Models
{
    public class TLUUTAMCHITIET
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public int? TluutamId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public decimal? Soluong { get; set; }
        public decimal? Dongia { get; set; }
        public decimal? Thanhtien { get; set; }
        public decimal? Tilegiamgia { get; set; }
        public string Baohanh { get; set; }
        public int? DmathangId { get; set; }
        public decimal? Soluongchuaquydoi { get; set; }
        public int? DdonvitinhId { get; set; }
    }
}
