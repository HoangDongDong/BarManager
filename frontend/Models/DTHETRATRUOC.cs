using System;

namespace QuanLyBar.Client.Models
{
    public class DTHETRATRUOC
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? DnhomthetratruocId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Khoa { get; set; }
        public DateTime? Ngayhethan { get; set; }
    }
}
