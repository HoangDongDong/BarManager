using System;

namespace QuanLyBar.Client.Models
{
    public class SQUICKNOTE
    {
        public int? Id { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? Sortorder { get; set; }
        public int? UsercreatedId { get; set; }
        public string Notedata { get; set; }
    }
}
