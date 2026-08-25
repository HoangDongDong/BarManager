using System;

namespace QuanLyBar.Client.Models
{
    public class DNHANVIEN
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? Sortorder { get; set; }
        public int? UsercreatedId { get; set; }
        public int? ParentId { get; set; }
        public string Parentdir { get; set; }
        public string Itemtype { get; set; }
        public int? AutoId { get; set; }
        public int? SimageId { get; set; }
        public string Cachtinhluong { get; set; }
        public decimal? Nghithu7 { get; set; }
        public string Nghichunhat { get; set; }
        public string Luongca { get; set; }
        public string Luongthang { get; set; }
        public decimal? Diachi { get; set; }
        public string Dienthoai { get; set; }
        public string Luongtheoca { get; set; }
        public int? DcalamviecId { get; set; }
        public string Code { get; set; }
    }
}
