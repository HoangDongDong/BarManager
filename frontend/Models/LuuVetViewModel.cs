using System;

namespace QuanLyBar.Client.Models
{
    public class LuuVetHoaDonItemViewModel
    {
        public string Stt { get; set; }
        public string Id { get; set; }
        public string SoPhieu { get; set; }
        public string Ban { get; set; }
        public string TrangThai { get; set; }
        public string TextColor { get; set; } = "#000000";
        public DateTime? Ngay { get; set; }
    }

    public class LuuVetViewModel
    {
        public string Stt { get; set; }
        public string Id { get; set; }
        public DateTime? Ngay { get; set; }
        public DateTime? Gio { get; set; }
        public string NgayStr => Ngay?.ToString("dd/MM/yyyy") ?? "";
        public string GioStr => Gio?.ToString("HH:mm:ss") ?? "";
        public string Sodonhang { get; set; }
        public string Note { get; set; }
        public string Taikhoan { get; set; }
        public string Thietbi { get; set; }
        public string Ban { get; set; }
        public string Chucnang { get; set; }

        public string ChucNangIcon
        {
            get
            {
                if (Chucnang != null && Chucnang.Contains("Sử dụng")) return "⭐ ";
                if (Chucnang != null && Chucnang.Contains("Điều chỉnh")) return "🌟 ";
                return "🔹 ";
            }
        }

        public string ChucNangColor
        {
            get
            {
                if (Chucnang != null && Chucnang.Contains("Sử dụng")) return "#d48806";
                if (Chucnang != null && Chucnang.Contains("Điều chỉnh")) return "#0066cc";
                return "#333333";
            }
        }
    }
}
