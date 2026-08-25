using System.Windows;
using System.Windows.Media;

namespace QuanLyBar.Client.Models
{
    public class TheoDoiDatPhongViewModel
    {
        public string TieuDe { get; set; }
        
        public string Ngay1 { get; set; }
        public string Ngay2 { get; set; }
        public string Ngay3 { get; set; }
        public string Ngay4 { get; set; }
        public string Ngay5 { get; set; }
        public string Ngay6 { get; set; }
        public string Ngay7 { get; set; }
        
        public string Tong { get; set; }
        
        public Brush MauChu { get; set; }
        public FontWeight DoDam { get; set; }
    }
}
