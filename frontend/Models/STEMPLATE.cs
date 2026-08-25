using System;

namespace QuanLyBar.Client.Models
{
    public class STEMPLATE
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
        public string Template { get; set; }
        public int? StabledescId { get; set; }
        public string Reportbase { get; set; }
        public int? SformId { get; set; }
        public string Barcode { get; set; }
        public string Barcodecol { get; set; }
        public string Style { get; set; }
    }
}
