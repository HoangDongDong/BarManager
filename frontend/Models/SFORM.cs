using System;

namespace QuanLyBar.Client.Models
{
    public class SFORM
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? Sortorder { get; set; }
        public int? ParentId { get; set; }
        public string Parentdir { get; set; }
        public string Itemtype { get; set; }
        public int? AutoId { get; set; }
        public int? SimageId { get; set; }
        public string Code { get; set; }
        public string Designcode { get; set; }
        public string Formtype { get; set; }
        public int? StabledescId { get; set; }
        public string Notemplate { get; set; }
        public string Aelayout { get; set; }
        public string Loai { get; set; }
        public byte[] Image32 { get; set; }
        public string Tabledesc { get; set; }
        public int? LasttemplateId { get; set; }
        public string Classname { get; set; }
        public string Filterconfig { get; set; }
        public string Refconfig { get; set; }
        public string Reportdesc { get; set; }
        public int? SfunctionId { get; set; }
        public string Showtotal { get; set; }
        public string Clientcode { get; set; }
        public string Servercode { get; set; }
        public string Tscode { get; set; }
        public string Tslayout { get; set; }
        public string Tsdesigncode { get; set; }
    }
}
