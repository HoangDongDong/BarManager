using System;

namespace QuanLyBar.Client.Models
{
    public class SREPORTROLE
    {
        public bool? Status { get; set; }
        public string Mode { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? Id { get; set; }
        public int? UsermodifiedId { get; set; }
        public int? UsercreatedId { get; set; }
        public int? SgroupuserId { get; set; }
        public int? SreportId { get; set; }
    }
}
