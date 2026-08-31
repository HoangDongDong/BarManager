namespace QuanLyBar.Client.Models
{
    public class KqkdRowViewModel
    {
        public string Stt { get; set; }
        public string ChiTieu { get; set; }
        public string PhanTramDt { get; set; } = "";
        public string GiaTri { get; set; }
        public string PhanTram { get; set; } = "";
        public string PhanTramCp { get; set; } = "";
        public string TangGiam { get; set; } = "0";
        public string KqThangTruoc { get; set; } = "0";
        public bool IsBold { get; set; } = false;
        public bool IsHeader { get; set; } = false;
    }
}
