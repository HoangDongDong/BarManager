using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyBar.Client.Models
{
    public class DinhLuongChiTietViewModel : INotifyPropertyChanged
    {
        private MatHangViewModel _selectedMatHang;
        private decimal _soLuong;
        private decimal _giaNhap;
        private decimal _giaVon;

        public string OriginalId { get; set; } // ID in DDINHLUONG if editing

        public MatHangViewModel SelectedMatHang
        {
            get => _selectedMatHang;
            set
            {
                if (_selectedMatHang != value)
                {
                    _selectedMatHang = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DonViTinh));
                    
                    if (_selectedMatHang != null)
                    {
                        GiaNhap = _selectedMatHang.Gianhap ?? 0;
                        GiaVon = _selectedMatHang.Giavon ?? 0; // Using Giavon if exists, or maybe we just map it.
                    }
                    else
                    {
                        GiaNhap = 0;
                        GiaVon = 0;
                    }
                }
            }
        }

        public string DonViTinh => SelectedMatHang?.DonViTinhName ?? "";

        public decimal SoLuong
        {
            get => _soLuong;
            set
            {
                if (_soLuong != value)
                {
                    _soLuong = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ThanhTienNhap));
                    OnPropertyChanged(nameof(ThanhTienVon));
                }
            }
        }

        public decimal GiaNhap
        {
            get => _giaNhap;
            set
            {
                if (_giaNhap != value)
                {
                    _giaNhap = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ThanhTienNhap));
                }
            }
        }

        public decimal ThanhTienNhap => SoLuong * GiaNhap;

        public decimal GiaVon
        {
            get => _giaVon;
            set
            {
                if (_giaVon != value)
                {
                    _giaVon = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ThanhTienVon));
                }
            }
        }

        public decimal ThanhTienVon => SoLuong * GiaVon;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
