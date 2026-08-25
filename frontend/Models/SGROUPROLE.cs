using System;

namespace QuanLyBar.Client.Models
{
    public class SGROUPROLE
    {
        public int? Id { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public int? SgroupuserId { get; set; }
        public int? SfunctionId { get; set; }
        public string Mode { get; set; }
    }
}
