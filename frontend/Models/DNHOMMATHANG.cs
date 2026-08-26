using System;

namespace QuanLyBar.Client.Models
{
    public class DNHOMMATHANG
    {
        public string Note { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? Sortorder { get; set; }
        public string ParentId { get; set; }
        public string Parentdir { get; set; }
        public string Itemtype { get; set; }
        public int? AutoId { get; set; }
        public int? SimageId { get; set; }
        public string Code { get; set; }
        public int? DloaidoId { get; set; }
        public string Mausac { get; set; }
        public byte[] Anh { get; set; }
    }
}
