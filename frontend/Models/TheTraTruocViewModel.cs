using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QuanLyBar.Client.Models
{
    public class TheTraTruocViewModel : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string MaThe { get; set; }
        public string DnhomthetratruocId { get; set; }
        public string TenNhomTheTraTruoc { get; set; }
        public bool Khoa { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public string GhiChu { get; set; }
        public int? Status { get; set; }

        public DateTime? TimeCreated { get; set; }
        public string UserCreatedId { get; set; }
        public string UserCreatedName { get; set; }

        public DateTime? TimeModified { get; set; }
        public string UserModifiedId { get; set; }
        public string UserModifiedName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class NhomTheTraTruocTreeItem : INotifyPropertyChanged
    {
        private string _name;
        private bool _isExpanded = true;
        private bool _isSelected;

        public string Id { get; set; }
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string ParentId { get; set; }
        public string Icon { get; set; } = "📁";
        public string IconColor { get; set; } = "#f0ad4e";

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public int ItemType { get; set; } = 2; // 0: All, 1: Folder, 2: Group, 3: Unset, 4: Trash
        public ObservableCollection<NhomTheTraTruocTreeItem> Children { get; set; } = new ObservableCollection<NhomTheTraTruocTreeItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocHoaDonItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string KhachHang { get; set; }
        public decimal TongCong { get; set; }
        public string NhanVienBan { get; set; }
        public string GioThanhToan { get; set; }
        public string ThuNgan { get; set; }
        public decimal Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; }
        public decimal TrichNhanVien { get; set; }
        public string CuaHang { get; set; }
        public decimal ConLai { get; set; }
        public decimal ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string MaVoucher { get; set; }
        public decimal TheTt { get; set; }
        public string TheTraTruoc { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal DiemGiam { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string Ban { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public decimal TienGio { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public int SoKhach { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal TiLePhiDichVu { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string SoOrder { get; set; }
        public string SoHoaDon { get; set; }
        public string SoThanhToan { get; set; }
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string BangGia { get; set; }
        public decimal TienGioPhongCuoi { get; set; }
        public DateTime? BatDauPhongCuoi { get; set; }
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public decimal PhutKhuyenMai { get; set; }
        public DateTime? InTamTinhLuc { get; set; }
        public decimal DatTruoc { get; set; }
        public decimal CongNo { get; set; }
        public decimal TienHangChuaGiam { get; set; }
        public decimal GiamGiaMatHang { get; set; }
        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string PassWifi { get; set; }
        public string GhiChu { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocNhapKhoItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public decimal TongCong { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TiLeGiamGia { get; set; }
        public decimal TienThue { get; set; }
        public decimal TiLeThue { get; set; }
        public decimal TienHang { get; set; }
        public string NhaCungCap { get; set; }
        public string KhoNhap { get; set; }
        public string NhanVienNhap { get; set; }
        public string DienGiai { get; set; }
        public decimal Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; }
        public decimal TrichNhanVien { get; set; }
        public string CuaHang { get; set; }
        public decimal ConLai { get; set; }
        public decimal ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string MaVoucher { get; set; }
        public decimal TheTt { get; set; }
        public string TheTraTruoc { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal DiemGiam { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string Ban { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public decimal TienGio { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public int SoKhach { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal TiLePhiDichVu { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string SoOrder { get; set; }
        public string SoHoaDon { get; set; }
        public string SoThanhToan { get; set; }
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string BangGia { get; set; }
        public decimal TienGioPhongCuoi { get; set; }
        public DateTime? BatDauPhongCuoi { get; set; }
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public decimal PhutKhuyenMai { get; set; }
        public DateTime? InTamTinhLuc { get; set; }
        public decimal DatTruoc { get; set; }
        public decimal CongNo { get; set; }
        public decimal TienHangChuaGiam { get; set; }
        public decimal GiamGiaMatHang { get; set; }
        public decimal TiLeKhuyenMaiPhutDau { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocXuatKhoItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public decimal TongCong { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TiLeGiamGia { get; set; }
        public decimal TienThue { get; set; }
        public decimal TiLeThue { get; set; }
        public decimal TienHang { get; set; }
        public string KhoXuat { get; set; }
        public string NhanVienXuat { get; set; }
        public decimal Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; }
        public decimal TrichNhanVien { get; set; }
        public string CuaHang { get; set; }
        public decimal ConLai { get; set; }
        public decimal ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string MaVoucher { get; set; }
        public decimal TheTt { get; set; }
        public string TheTraTruoc { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal DiemGiam { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string Ban { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public decimal TienGio { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public int SoKhach { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal TiLePhiDichVu { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string SoOrder { get; set; }
        public string SoHoaDon { get; set; }
        public string SoThanhToan { get; set; }
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string BangGia { get; set; }
        public decimal TienGioPhongCuoi { get; set; }
        public DateTime? BatDauPhongCuoi { get; set; }
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public decimal PhutKhuyenMai { get; set; }
        public DateTime? InTamTinhLuc { get; set; }
        public decimal DatTruoc { get; set; }
        public decimal CongNo { get; set; }
        public decimal TienHangChuaGiam { get; set; }
        public decimal GiamGiaMatHang { get; set; }
        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string PassWifi { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocChuyenKhoItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public decimal TongCong { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TiLeGiamGia { get; set; }
        public decimal TienThue { get; set; }
        public decimal TiLeThue { get; set; }
        public decimal TienHang { get; set; }
        public string KhoXuat { get; set; }
        public string KhoNhap { get; set; }
        public string NhanVienXuat { get; set; }
        public decimal Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; }
        public decimal TrichNhanVien { get; set; }
        public string CuaHang { get; set; }
        public decimal ConLai { get; set; }
        public decimal ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string MaVoucher { get; set; }
        public decimal TheTt { get; set; }
        public string TheTraTruoc { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal DiemGiam { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string Ban { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public decimal TienGio { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public int SoKhach { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal TiLePhiDichVu { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string SoOrder { get; set; }
        public string SoHoaDon { get; set; }
        public string SoThanhToan { get; set; }
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string BangGia { get; set; }
        public decimal TienGioPhongCuoi { get; set; }
        public DateTime? BatDauPhongCuoi { get; set; }
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public decimal PhutKhuyenMai { get; set; }
        public DateTime? InTamTinhLuc { get; set; }
        public decimal DatTruoc { get; set; }
        public decimal CongNo { get; set; }
        public decimal TienHangChuaGiam { get; set; }
        public decimal GiamGiaMatHang { get; set; }
        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string PassWifi { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocKiemKeItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string KhoHang { get; set; }
        public string NhanVien { get; set; }
        public string DienGiai { get; set; }
        public decimal Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; }
        public decimal TrichNhanVien { get; set; }
        public string CuaHang { get; set; }
        public decimal ConLai { get; set; }
        public decimal ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string MaVoucher { get; set; }
        public decimal TheTt { get; set; }
        public string TheTraTruoc { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal DiemGiam { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string Ban { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public decimal TienGio { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public int SoKhach { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal TiLePhiDichVu { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string SoOrder { get; set; }
        public string SoHoaDon { get; set; }
        public string SoThanhToan { get; set; }
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string BangGia { get; set; }
        public decimal TienGioPhongCuoi { get; set; }
        public DateTime? BatDauPhongCuoi { get; set; }
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public decimal PhutKhuyenMai { get; set; }
        public DateTime? InTamTinhLuc { get; set; }
        public decimal DatTruoc { get; set; }
        public decimal CongNo { get; set; }
        public decimal TienHangChuaGiam { get; set; }
        public decimal GiamGiaMatHang { get; set; }
        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string PassWifi { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocKhachHangItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string MaKhach { get; set; }
        public string TenKhachHang { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string NhomKhachHang { get; set; }
        public string MaSoThue { get; set; }
        public string NhanVien { get; set; }
        public string TinhThanh { get; set; }
        public string Facebook { get; set; }
        public string TheTraTruoc { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocThuChiItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string TenDoiTuong { get; set; }
        public string DiaChi { get; set; }
        public string LyDoThuChi { get; set; }
        public string DienGiai { get; set; }
        public string ChungTuGoc { get; set; }
        public decimal SoTien { get; set; }
        public decimal SoTienThu { get => SoTien; set => SoTien = value; }
        public decimal SoTienChi { get; set; }
        public bool ChuyenKhoan { get; set; }
        public string DatHang { get; set; }
        public string GhiChu { get; set; }
        public string CuaHang { get; set; }
        public bool LaPhieuThuCongNo { get; set; }
        public bool KhongThayDoiCongNo { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string TheTraTruoc { get; set; }
        public string DonHang { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class TheTraTruocThuCongNoItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt
        {
            get => _stt;
            set { _stt = value; OnPropertyChanged(nameof(Stt)); }
        }

        public string Id { get; set; }
        public string GhiChu { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public string TenDoiTuong { get; set; }
        public string DiaChi { get; set; }
        public string NhanVien { get; set; }
        public string KhachHang { get; set; }
        public string LoaiDoiTuong { get; set; }
        public string LyDoThuChi { get; set; }
        public string DienGiai { get; set; }
        public string ChungTuGoc { get; set; }
        public decimal SoTienThu { get; set; }
        public decimal SoTienChi { get; set; }
        public string NhaCungCap { get; set; }
        public bool ChuyenKhoan { get; set; }
        public string DatHang { get; set; }
        public string CuaHang { get; set; }
        public bool LaPhieuThuCongNo { get; set; }
        public bool KhongThayDoiCongNo { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string TheTraTruoc { get; set; }
        public string DonHang { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
