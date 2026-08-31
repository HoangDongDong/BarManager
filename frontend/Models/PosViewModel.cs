using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace QuanLyBar.Client.Models
{
    public class PosKhuVucViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<PosBanViewModel> BanList { get; set; } = new ObservableCollection<PosBanViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class PosBanViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _khuVucId;
        private string _khuVucName;
        private bool _isOccupied;
        private bool _isSelected;
        private string _timerText;
        private string _activeOrderId;
        private DateTime? _startTime;
        private int _soKhach = 0;
        private string _soPhieu;
        private string _khachHangName;
        private string _ghiChu;
        private decimal _tienHang = 0;
        private decimal _giamGiaPhanTram = 0;
        private decimal _giamGia = 0;
        private decimal _tongCong = 0;

        public string Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string KhuVucId { get => _khuVucId; set { _khuVucId = value; OnPropertyChanged(); } }
        public string KhuVucName { get => _khuVucName; set { _khuVucName = value; OnPropertyChanged(); } }
        public string ActiveOrderId { get => _activeOrderId; set { _activeOrderId = value; OnPropertyChanged(); } }
        public DateTime? StartTime { get => _startTime; set { _startTime = value; OnPropertyChanged(); UpdateTimerText(); } }
        
        public bool IsOccupied 
        { 
            get => _isOccupied; 
            set 
            { 
                _isOccupied = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IconBackgroundColor));
                OnPropertyChanged(nameof(TimerVisibility));
            } 
        }

        public bool IsSelected 
        { 
            get => _isSelected; 
            set 
            { 
                _isSelected = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(SelectionBorderBrush));
                OnPropertyChanged(nameof(SelectionBackground));
            } 
        }

        public string TimerText 
        { 
            get => _timerText; 
            set { _timerText = value; OnPropertyChanged(); } 
        }

        public int SoKhach { get => _soKhach; set { _soKhach = value; OnPropertyChanged(); } }
        public string SoPhieu { get => _soPhieu; set { _soPhieu = value; OnPropertyChanged(); } }
        public string KhachHangName { get => _khachHangName; set { _khachHangName = value; OnPropertyChanged(); } }
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        public decimal TienHang { get => _tienHang; set { _tienHang = value; OnPropertyChanged(); } }
        public decimal GiamGiaPhanTram { get => _giamGiaPhanTram; set { _giamGiaPhanTram = value; OnPropertyChanged(); } }
        public decimal GiamGia { get => _giamGia; set { _giamGia = value; OnPropertyChanged(); } }
        public decimal TongCong { get => _tongCong; set { _tongCong = value; OnPropertyChanged(); } }

        public ObservableCollection<PosDonHangChiTietViewModel> OrderItems { get; set; } = new ObservableCollection<PosDonHangChiTietViewModel>();

        public Brush IconBackgroundColor => IsOccupied 
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c0392b")) 
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4a4a"));

        public Visibility TimerVisibility => IsOccupied && !string.IsNullOrEmpty(TimerText) 
            ? Visibility.Visible 
            : Visibility.Collapsed;

        public Brush SelectionBorderBrush => IsSelected 
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078d7")) 
            : Brushes.Transparent;

        public Brush SelectionBackground => IsSelected 
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cbe8f6")) 
            : Brushes.Transparent;

        public void UpdateTimerText()
        {
            if (IsOccupied && StartTime.HasValue)
            {
                var elapsed = DateTime.Now - StartTime.Value;
                int hours = (int)elapsed.TotalHours;
                int minutes = elapsed.Minutes;
                TimerText = $"{hours}h {minutes}'";
            }
            else
            {
                TimerText = "";
            }
            OnPropertyChanged(nameof(TimerVisibility));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class PosNhomMatHangViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded = true;
        private bool _isSelected;

        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string Icon { get; set; } = "📁";
        public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(); } }
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
        public ObservableCollection<PosNhomMatHangViewModel> Children { get; set; } = new ObservableCollection<PosNhomMatHangViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class PosMatHangViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DonViTinh { get; set; }
        public decimal? GiaBan { get; set; }
        public int LoaiDoId { get; set; } = 1;
        public string LoaiDoName { get; set; } = "Đồ ăn";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class PosDonHangChiTietViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _matHangId;
        private string _matHangName;
        private string _donViTinh;
        private decimal _soLuong = 1;
        private decimal _donGia = 0;
        private decimal _chietKhauPhanTram = 0;
        private decimal _thanhTien = 0;
        private string _ghiChu;
        private int _loaiDoId = 1;
        private string _loaiDoName = "Đồ ăn";
        private bool _daInCheBien = false;

        public string Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string MatHangId { get => _matHangId; set { _matHangId = value; OnPropertyChanged(); } }
        public string MatHangName { get => _matHangName; set { _matHangName = value; OnPropertyChanged(); } }
        public string DonViTinh { get => _donViTinh; set { _donViTinh = value; OnPropertyChanged(); } }
        public int LoaiDoId { get => _loaiDoId; set { _loaiDoId = value; OnPropertyChanged(); } }
        public string LoaiDoName { get => _loaiDoName; set { _loaiDoName = value; OnPropertyChanged(); } }
        public bool DaInCheBien { get => _daInCheBien; set { _daInCheBien = value; OnPropertyChanged(); } }
        
        public string ItemCategory
        {
            get
            {
                string name = (LoaiDoName ?? "").ToLower();
                string itemName = (MatHangName ?? "").ToLower();

                if (LoaiDoId == 2 || name.Contains("uống") || name.Contains("nước") || name.Contains("bia") || name.Contains("rượu") || name.Contains("trà") || name.Contains("cà phê")
                    || itemName.Contains("bia") || itemName.Contains("rượu") || itemName.Contains("aquafina") || itemName.Contains("nước") || itemName.Contains("trà ") || itemName.Contains("c2") || itemName.Contains("sting"))
                {
                    return "DoUong";
                }
                if (LoaiDoId == 3 || name.Contains("dịch vụ") || name.Contains("hát") || name.Contains("phòng") || name.Contains("karaoke")
                    || itemName.Contains("khăn lạnh") || itemName.Contains("karaoke") || itemName.Contains("tiền giờ"))
                {
                    return "DichVu";
                }
                if (LoaiDoId == 4 || name.Contains("khác") || itemName.Contains("thuốc lá") || itemName.Contains("ba số") || itemName.Contains("thăng long") || itemName.Contains("vinataba") || itemName.Contains("ngựa") || itemName.Contains("marlboro"))
                {
                    return "DoKhac";
                }
                return "DoAn";
            }
        }

        public decimal SoLuong 
        { 
            get => _soLuong; 
            set 
            { 
                _soLuong = value; 
                Recalculate(); 
                OnPropertyChanged(); 
            } 
        }

        public decimal DonGia 
        { 
            get => _donGia; 
            set 
            { 
                _donGia = value; 
                Recalculate(); 
                OnPropertyChanged(); 
            } 
        }

        public decimal ChietKhauPhanTram 
        { 
            get => _chietKhauPhanTram; 
            set 
            { 
                _chietKhauPhanTram = value; 
                Recalculate(); 
                OnPropertyChanged(); 
            } 
        }

        public decimal ThanhTien 
        { 
            get => _thanhTien; 
            set { _thanhTien = value; OnPropertyChanged(); } 
        }

        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        public void Recalculate()
        {
            decimal raw = SoLuong * DonGia;
            if (ChietKhauPhanTram > 0)
            {
                raw -= raw * (ChietKhauPhanTram / 100m);
            }
            ThanhTien = Math.Max(0, raw);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
