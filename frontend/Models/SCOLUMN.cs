using System;

namespace QuanLyBar.Client.Models
{
    public class SCOLUMN
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? StabledescId { get; set; }
        public string Caption { get; set; }
        public string Format { get; set; }
        public int? ReftableId { get; set; }
        public string Allowempty { get; set; }
        public string Allowduplicate { get; set; }
        public bool? Issystem { get; set; }
        public int? EnableconfigId { get; set; }
        public int? TitleconfigId { get; set; }
        public string Tooltip { get; set; }
        public int? SfunctionId { get; set; }
        public string Maxvalue { get; set; }
        public string Minvalue { get; set; }
        public int? DefaultconfigId { get; set; }
        public string Tag { get; set; }
        public string Controltype { get; set; }
        public string Showonlookup { get; set; }
        public string Formula { get; set; }
        public string Autoreport { get; set; }
    }
}
