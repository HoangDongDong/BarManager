using System;

namespace QuanLyBar.Client.Models
{
    public class DichVuYeuCauViewModel
    {
        public string Id { get; set; }
        public string BanId { get; set; }
        public string NoiDung { get; set; }
        public string Phong { get; set; }
        public int SoLan { get; set; } = 1;
        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}
