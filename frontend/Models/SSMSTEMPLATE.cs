using System;

namespace QuanLyBar.Client.Models
{
    public class SSMSTEMPLATE
    {
        public int? AutoId { get; set; }
        public bool? Status { get; set; }
        public string Itemtype { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? UsermodifiedId { get; set; }
        public int? Sortorder { get; set; }
        public int? UsercreatedId { get; set; }
        public int? ParentId { get; set; }
        public string Parentdir { get; set; }
        public int? SimageId { get; set; }
    }
}
