using System;

namespace QuanLyBar.Client.Models
{
    public class SCONFIG
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
        public string Textvalue { get; set; }
        public DateTime? Datetimevalue { get; set; }
        public string Intvalue { get; set; }
        public string Decimalvalue { get; set; }
        public string Datatype { get; set; }
        public byte[] Blobvalue { get; set; }
        public string Caption { get; set; }
        public string Itemtype { get; set; }
        public int? SimageId { get; set; }
        public string Moredetail { get; set; }
        public string Controltype { get; set; }
        public int? ReftableId { get; set; }
        public string Otherconfig { get; set; }
        public int? SconfiggroupId { get; set; }
        public string Tab { get; set; }
        public int? Socot { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string Showonreport { get; set; }
        public int? ParentId { get; set; }
    }
}
