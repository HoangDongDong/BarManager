using System;

namespace QuanLyBar.Client.Models
{
    public class DDOTKHUYENMAI
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? DloaihinhkhuyenmaiId { get; set; }
        public DateTime? Tungay { get; set; }
        public DateTime? Denngay { get; set; }
        public string Ngungapdung { get; set; }
        public decimal? Tilegiamgia { get; set; }
        public decimal? Tilegiamgiatiengio { get; set; }
        public DateTime? Khuyenmaigiohat { get; set; }
        public decimal? Tilegiamgiatong { get; set; }
        public DateTime? Tugio { get; set; }
        public DateTime? Dengio { get; set; }
        public decimal? Tilegiamgiagiodau { get; set; }
    }
}
