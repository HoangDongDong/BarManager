using System;

namespace QuanLyBar.Client.Models
{
    public class TDONHANGGIO
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? TdonhangId { get; set; }
        public int? UsercreatedId { get; set; }
        public DateTime? Tugio { get; set; }
        public DateTime? Dengio { get; set; }
        public decimal? Dongia { get; set; }
        public int? DbanggiaId { get; set; }
        public int? DbanId { get; set; }
        public decimal? Thanhtien { get; set; }
        public decimal? Cachtinhgia { get; set; }
    }
}
