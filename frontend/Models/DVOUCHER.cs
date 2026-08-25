using System;

namespace QuanLyBar.Client.Models
{
    public class DVOUCHER
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? DnhomvoucherId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public decimal? Giatri { get; set; }
        public DateTime? Ngayphathanh { get; set; }
        public DateTime? Hansudung { get; set; }
        public int? DkhachhangId { get; set; }
    }
}
