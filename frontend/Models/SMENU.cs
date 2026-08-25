using System;

namespace QuanLyBar.Client.Models
{
    public class SMENU
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
        public string Operation { get; set; }
        public int? StabledescId { get; set; }
        public int? SformId { get; set; }
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public byte[] Image { get; set; }
        public string Toolbar { get; set; }
        public string Toolbarindex { get; set; }
        public string Shortcut { get; set; }
        public string Toolbartitlte { get; set; }
        public int? VisibleconfigId { get; set; }
        public int? SreportId { get; set; }
        public int? Viewcount { get; set; }
        public int? SfunctionId { get; set; }
        public string Loai { get; set; }
        public string Loctheoloai { get; set; }
        public string Hideinmenu { get; set; }
        public string Menucode { get; set; }
        public string Viewonts { get; set; }
        public string Viewonmobi { get; set; }
        public string Viewonweb { get; set; }
    }
}
