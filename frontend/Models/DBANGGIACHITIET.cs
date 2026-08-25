using System;

namespace QuanLyBar.Client.Models
{
    public class DBANGGIACHITIET
    {
        public int? Id { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? DbanggiaId { get; set; }
        public int? UsercreatedId { get; set; }
        public DateTime? Tugio { get; set; }
        public DateTime? Dengio { get; set; }
        public decimal? Sotien { get; set; }
        public DateTime? Ngayle { get; set; }
    }
}
