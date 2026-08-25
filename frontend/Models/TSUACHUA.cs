using System;

namespace QuanLyBar.Client.Models
{
    public class TSUACHUA
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
        public int? DbanId { get; set; }
        public string Dasuaxong { get; set; }
        public string Noidung { get; set; }
        public int? DloaiphongId { get; set; }
        public int? DnhanvienId { get; set; }
        public string Consudungduoc { get; set; }
    }
}
