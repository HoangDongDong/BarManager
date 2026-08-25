using System;

namespace QuanLyBar.Client.Models
{
    public class TTANGGIAMDIEM
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
        public int? DkhachhangId { get; set; }
        public decimal? Diemtang { get; set; }
        public decimal? Diemgiam { get; set; }
        public string Lydo { get; set; }
    }
}
