namespace QuanLyBar.Client.Models
{
    public class DinhLuongImportViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public int Stt { get; set; }
        
        public string MatHangName { get; set; }
        public string DmathangId { get; set; }
        public string MatHangDVT { get; set; }
        
        public string NguyenLieuName { get; set; }
        public string DvattuId { get; set; }
        public string NguyenLieuDVT { get; set; }
        
        public decimal SoLuong { get; set; }
        
        public string NhomMatHangId { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
