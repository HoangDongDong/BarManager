using System;

namespace QuanLyBar.Client.Models
{
    public class SUSER
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public int? UsermodifiedId { get; set; }
        public DateTime? Timemodified { get; set; }
        public DateTime? Timecreated { get; set; }
        public int? UsercreatedId { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool? Isadmin { get; set; }
        public int? SgroupuserId { get; set; }
        public string Smtp { get; set; }
        public string Ssl { get; set; }
        public int? Port { get; set; }
        public string Pass { get; set; }
        public string Vietinput { get; set; }
        public string Inputmode { get; set; }
        public int? SimageId { get; set; }
        public int? AutoId { get; set; }
        public int? ParentId { get; set; }
        public string Parentdir { get; set; }
        public int? Sortorder { get; set; }
        public string Itemtype { get; set; }
        public int? DnhanvienId { get; set; }
        public int? DefaultfuncId { get; set; }
        public int? UserId { get; set; }
        public string Cardcode { get; set; }
    }
}
