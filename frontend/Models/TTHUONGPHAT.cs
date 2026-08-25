using System;

namespace QuanLyBar.Client.Models
{
    public class TTHUONGPHAT
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public DateTime? Ngay { get; set; }
        public int? UsercreatedId { get; set; }
        public int? DnhanvienId { get; set; }
        public decimal? Thuong { get; set; }
        public string Phat { get; set; }
        public int? DlydothuongphatId { get; set; }
    }
}
