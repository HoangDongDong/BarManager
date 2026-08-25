using System;

namespace QuanLyBar.Client.Models
{
    public class DNHACUNGCAP
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? DnhomnhacungcapId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Manhacungcap { get; set; }
        public decimal? Diachi { get; set; }
        public string Dienthoai { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
    }
}
