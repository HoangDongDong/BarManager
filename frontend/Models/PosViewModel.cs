using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace QuanLyBar.Client.Models
{
    public class PosKhuVucViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<PosBanViewModel> BanList { get; set; } = new ObservableCollection<PosBanViewModel>();
    }

    public class PosBanViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsOccupied { get; set; }
        public string TimerText { get; set; }
        
        public Brush BackgroundColor => IsOccupied ? Brushes.Red : Brushes.White;
        public Brush ForegroundColor => IsOccupied ? Brushes.White : Brushes.Black;
        public Visibility TimerVisibility => IsOccupied ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public class PosNhomMatHangViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public ObservableCollection<PosNhomMatHangViewModel> Children { get; set; } = new ObservableCollection<PosNhomMatHangViewModel>();
    }

    public class PosMatHangViewModel
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DonViTinh { get; set; }
        public decimal? GiaBan { get; set; }
    }

    public class PosDonHangViewModel
    {
        public string Id { get; set; }
        public string SoPhieu { get; set; }
        public string KhachHangName { get; set; }
        public DateTime? Ngay { get; set; }
        public string GioBatDau { get; set; }
        
        public decimal? TienHang { get; set; }
        public decimal? GiamGia { get; set; }
        public decimal? TongCong { get; set; }
    }

    public class PosDonHangChiTietViewModel
    {
        public string Id { get; set; }
        public string MatHangName { get; set; }
        public string DonViTinh { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ChietKhauPhanTram { get; set; }
        public decimal? ThanhTien { get; set; }
        public string GhiChu { get; set; }
    }
}
