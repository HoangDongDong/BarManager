using System;

namespace QuanLyBar.Client.Models
{
    public class DDINHLUONG
    {
        public string Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string DmathangId { get; set; }
        public decimal? Soluong { get; set; }
        public string DvattuId { get; set; }
    }
}
