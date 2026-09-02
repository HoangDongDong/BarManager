using System;
using System.ComponentModel;

namespace QuanLyBar.Client.Models
{
    public class KhachHangThanThietViewModel : INotifyPropertyChanged
    {
        private int _stt;
        private decimal _doanhSo;
        private int _soHoaDon;
        private decimal _diemTichLuy;
        private decimal _diemTichLuyBanDau;

        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string Makhach { get; set; }
        public string Name { get; set; }
        public string Diachi { get; set; }
        public string Dienthoai { get; set; }
        public string Email { get; set; }

        public decimal DoanhSo
        {
            get => _doanhSo;
            set { _doanhSo = value; OnPropertyChanged(nameof(DoanhSo)); }
        }

        public int SoHoaDon
        {
            get => _soHoaDon;
            set { _soHoaDon = value; OnPropertyChanged(nameof(SoHoaDon)); }
        }

        public decimal DiemTichLuy
        {
            get => _diemTichLuy;
            set { _diemTichLuy = value; OnPropertyChanged(nameof(DiemTichLuy)); }
        }

        public decimal DiemTichLuyBanDau
        {
            get => _diemTichLuyBanDau;
            set { _diemTichLuyBanDau = value; OnPropertyChanged(nameof(DiemTichLuyBanDau)); }
        }

        public string Note { get; set; }
        public string DnhomkhachhangId { get; set; }
        public string TenNhomKhachHang { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TangGiamDiemItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public DateTime? Ngay { get; set; }
        public string SoPhieu { get; set; }
        public string GhiChu { get; set; }
        public decimal? DiemTang { get; set; }
        public decimal? DiemGiam { get; set; }
        public string LyDo { get; set; }
        public string DkhachhangId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class DiemTheoHoaDonItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public DateTime? Ngay { get; set; }
        public string SoPhieu { get; set; }
        public string SoHoaDon { get => SoPhieu; set => SoPhieu = value; }
        public string Ban { get; set; }
        public decimal TongCong { get; set; }
        public decimal Diem { get; set; }
        public decimal DiemTichLuy { get => Diem; set => Diem = value; }
        public decimal DiemSuDung { get; set; }
        public decimal DiemGiam { get => DiemSuDung; set => DiemSuDung = value; }
        public string GhiChu { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
