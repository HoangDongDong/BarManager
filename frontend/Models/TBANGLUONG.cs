using System;

namespace QuanLyBar.Client.Models
{
    public class TBANGLUONG
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
        public string Thang { get; set; }
        public string Nam { get; set; }
        public decimal? Chitiet { get; set; }
    }
}
