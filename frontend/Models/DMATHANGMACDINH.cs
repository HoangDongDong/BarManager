using System;

namespace QuanLyBar.Client.Models
{
    public class DMATHANGMACDINH
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? DkhuvucId { get; set; }
        public int? UsercreatedId { get; set; }
        public int? DmathangId { get; set; }
        public decimal? Soluong { get; set; }
    }
}
