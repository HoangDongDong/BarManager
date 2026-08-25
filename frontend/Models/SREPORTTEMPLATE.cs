using System;

namespace QuanLyBar.Client.Models
{
    public class SREPORTTEMPLATE
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Template { get; set; }
        public int? StemplateId { get; set; }
        public int? SreportId { get; set; }
        public string Config { get; set; }
        public int? Sortorder { get; set; }
        public int? ParentId { get; set; }
        public string Parentdir { get; set; }
        public string Itemtype { get; set; }
        public int? AutoId { get; set; }
        public int? SimageId { get; set; }
        public string Autogenreport { get; set; }
        public string Colautowidth { get; set; }
        public int? StemplatelandscapeId { get; set; }
    }
}
