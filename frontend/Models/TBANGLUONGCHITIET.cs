using System;

namespace QuanLyBar.Client.Models
{
    public class TBANGLUONGCHITIET
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public int? TbangluongId { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? DnhanvienId { get; set; }
        public int? DcalamviecId { get; set; }
        public string Trangthai { get; set; }
        public DateTime? Ngay { get; set; }
    }
}
