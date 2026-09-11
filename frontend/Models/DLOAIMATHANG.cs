using System;

namespace QuanLyBar.Client.Models
{
    public class DLOAIMATHANG
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public string UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public string Sortorder { get; set; }
        public string UsercreatedId { get; set; }
        public string ParentId { get; set; }
        public string Parentdir { get; set; }
        public string Itemtype { get; set; }
        public int? AutoId { get; set; }
        public string SimageId { get; set; }
        public string Cobanhang { get; set; }
        public string Cotonkho { get; set; }
        public string Codinhluong { get; set; }

        // UI helper properties
        public System.Windows.Media.ImageSource IconSource { get; set; }
        public bool IsBanHang => Cobanhang == "30" || Cobanhang == "1" || Cobanhang?.ToLower() == "true";
        public bool IsTonKho => Cotonkho == "30" || Cotonkho == "1" || Cotonkho?.ToLower() == "true";
        public bool IsDinhLuong => Codinhluong == "30" || Codinhluong == "1" || Codinhluong?.ToLower() == "true";
    }
}
