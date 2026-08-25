using System;

namespace QuanLyBar.Client.Models
{
    public class TNGUOIDUNGTHEOCUAHANG
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? SuserId { get; set; }
        public int? DcuahangId { get; set; }
    }
}
