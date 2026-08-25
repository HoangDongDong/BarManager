using System;

namespace QuanLyBar.Client.Models
{
    public class SGRID
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Mobile { get; set; }
        public int? GridId { get; set; }
    }
}
