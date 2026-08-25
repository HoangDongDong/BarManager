using System;

namespace QuanLyBar.Client.Models
{
    public class SREPORT
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
        public byte[] Image32 { get; set; }
        public string Filterconfig { get; set; }
        public string Verticallayout { get; set; }
        public string Runcount { get; set; }
        public string Masterview { get; set; }
        public string Detailview { get; set; }
        public bool? Hasdetail { get; set; }
        public string Filterontop { get; set; }
        public bool? Hasdetail2 { get; set; }
        public string Detail2view { get; set; }
        public string Viewinreportmg { get; set; }
        public string Sql { get; set; }
        public string Params { get; set; }
        public int? LasttemplateId { get; set; }
        public string Sqlmode { get; set; }
        public string Reminder { get; set; }
        public string Format { get; set; }
        public decimal? Thuongdung { get; set; }
        public string Code { get; set; }
        public string Colconfig { get; set; }
        public string Rawmode { get; set; }
        public string Classname { get; set; }
        public string Servercode { get; set; }
        public string Clientcode { get; set; }
    }
}
