using System;

namespace QuanLyBar.Client.Models
{
    public class DNHOMKHACHHANG
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
        public decimal? Tilegiamgia { get; set; }
        public decimal? Diemtichluy { get; set; }
        public decimal? Tilegiamgiatienhang { get; set; }
        public decimal? Tilegiamgiatiengio { get; set; }
        public decimal? Tilegiamdoan { get; set; }
        public decimal? Tilegiamdouong { get; set; }
        public decimal? Tilegiamdichvu { get; set; }
        public decimal? Tilegiamdokhac { get; set; }
    }
}
