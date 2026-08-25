using System;

namespace QuanLyBar.Client.Models
{
    public class TDONHANGHUYCHITIET
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public int? TdonhanghuyId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Mahang { get; set; }
        public string Tenhang { get; set; }
        public string Dvt { get; set; }
        public decimal? Dongia { get; set; }
        public decimal? Thanhtien { get; set; }
        public decimal? Soluong { get; set; }
    }
}
