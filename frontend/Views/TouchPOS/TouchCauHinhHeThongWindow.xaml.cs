using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using QuanLyBar.Views.TouchPOS;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchCauHinhHeThongWindow : Window
    {
        private byte[] _newLogoBytes = null;
        private readonly List<FrameworkElement> _tabPanels = new List<FrameworkElement>();
        private readonly List<CategoryItem> _categories = new List<CategoryItem>();
        private CategoryItem _selectedCategory = null;

        public class CategoryItem : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string Name { get; set; }

            private Brush _btnBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006400"));
            public Brush BtnBrush
            {
                get => _btnBrush;
                set { _btnBrush = value; OnPropertyChanged(nameof(BtnBrush)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public TouchCauHinhHeThongWindow()
        {
            InitializeComponent();
            InitializePanelsList();
            InitializeCategories();
            InitializeComboBoxes();
        }

        private void InitializePanelsList()
        {
            _tabPanels.Add(PanelTab1);
            _tabPanels.Add(PanelTab2);
            _tabPanels.Add(PanelTab3);
            _tabPanels.Add(PanelTab4);
            _tabPanels.Add(PanelTab5);
            _tabPanels.Add(PanelTab6);
            _tabPanels.Add(PanelTab7);
            _tabPanels.Add(PanelTab8);
            _tabPanels.Add(PanelTab9);
            _tabPanels.Add(PanelTab10);
            _tabPanels.Add(PanelTab11);
            _tabPanels.Add(PanelTab12);
            _tabPanels.Add(PanelTab13);
            _tabPanels.Add(PanelTab14);
        }

        private void InitializeCategories()
        {
            string[] names = new string[]
            {
                "Thông tin chung",
                "In HĐ và in chế biến",
                "Bán hàng",
                "Giờ trên bill",
                "Thanh toán",
                "Tích điểm",
                "Kho hàng",
                "Mặt hàng",
                "Thiết bị khác",
                "Cảnh báo",
                "Quản trị",
                "Cảm ứng",
                "Tùy chọn khác",
                "Ứng dụng"
            };

            for (int i = 0; i < names.Length; i++)
            {
                _categories.Add(new CategoryItem { Id = i + 1, Name = names[i] });
            }

            IcCategories.ItemsSource = _categories;
            SelectCategory(_categories[0]);
        }

        private void SelectCategory(CategoryItem cat)
        {
            if (cat == null) return;
            _selectedCategory = cat;

            foreach (var item in _categories)
            {
                if (item.Id == cat.Id)
                {
                    item.BtnBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F00"));
                }
                else
                {
                    item.BtnBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006400"));
                }
            }

            int index = cat.Id - 1;
            for (int i = 0; i < _tabPanels.Count; i++)
            {
                _tabPanels[i].Visibility = (i == index) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CategoryItem cat)
            {
                SelectCategory(cat);
            }
        }

        private void InitializeComboBoxes()
        {
            // Mẫu hóa đơn
            CboMauHoaDon.ItemsSource = new string[]
            {
                "Mẫu 54 mm x 2 dòng",
                "Mẫu 80mm 2 ngôn ngữ",
                "Mẫu in 80mm (cộng gộp)",
                "Mẫu in bill 54mm",
                "Mẫu in bill 80 (Có CK)",
                "Mẫu in bill 80mm",
                "Mẫu in bill 80mm (CK tổng)",
                "Mẫu in bill A4",
                "Mẫu in bill A5"
            };
            CboMauHoaDon.SelectedIndex = 5;

            // Mẫu in chế biến
            CboMauInCheBien.ItemsSource = new string[] { "Mẫu 58mm", "Mẫu 80mm" };
            CboMauInCheBien.SelectedIndex = 1;

            // Mẫu in chuyển bàn
            CboMauInChuyenBan.ItemsSource = new string[] { "Mẫu 58mm", "Mẫu 80mm" };
            CboMauInChuyenBan.SelectedIndex = 1;

            // In hóa đơn theo khu vực
            CboInHoaDonTheoKhuVuc.ItemsSource = new string[]
            {
                "Không sử dụng",
                "Chỉ in hóa đơn theo máy in theo khu vực",
                "In một liên tại quầy và một liên theo khu vực",
                "In tạm tính theo khu vực và in hóa đơn tại quầy"
            };
            CboInHoaDonTheoKhuVuc.SelectedIndex = 0;

            // Bán hàng
            CboCachChonGioTinhGia.ItemsSource = new string[] { "Giờ gọi đồ", "Giờ thanh toán" };
            CboCachChonGioTinhGia.SelectedIndex = 0;

            CboCachChonKhachHang.ItemsSource = new string[] { "Chọn bằng chuột và bàn phím", "Tự động chọn khách lẻ", "Bắt buộc chọn khách hàng" };
            CboCachChonKhachHang.SelectedIndex = 0;

            CboCachChonNgayGiaoDich.ItemsSource = new string[] {
                "Theo ngày mở hóa đơn",
                "Theo ngày đóng hóa đơn",
                "Hiển thị để lựa chọn khi đăng nhập",
                "Đóng mở ngày chủ động",
                "Tự động lựa chọn theo giờ"
            };
            CboCachChonNgayGiaoDich.SelectedIndex = 1;

            CboKichThuocHienThiOGiaoDienDichVu.ItemsSource = new string[] { "Lớn", "Trung bình", "Vừa", "Nhỏ" };
            CboKichThuocHienThiOGiaoDienDichVu.SelectedIndex = 0;

            // Giờ trên bill
            CboNgayCuoiTuan.ItemsSource = new string[] { "Chủ nhật", "Thứ bảy & Chủ nhật", "Không" };
            CboNgayCuoiTuan.SelectedIndex = 0;

            // Tích điểm
            CboCachTinhDiem.ItemsSource = new string[] { "Điểm được tính trên từng hóa đơn", "Điểm được tính trên tổng doanh số" };
            CboCachTinhDiem.SelectedIndex = 0;

            // Mặt hàng
            CboTimTheo.ItemsSource = new string[] { "Cả tên hàng và mã hàng", "Tên hàng", "Mã hàng" };
            CboTimTheo.SelectedIndex = 0;

            CboCachTim.ItemsSource = new string[] { "Tìm có chứa", "Bắt đầu bằng" };
            CboCachTim.SelectedIndex = 0;

            CboSapXepThuTuTheo.ItemsSource = new string[] { "Mã hàng", "Tên hàng" };
            CboSapXepThuTuTheo.SelectedIndex = 0;

            CboCachLamTron.ItemsSource = new string[] { "Làm tròn xuống", "Làm tròn giữa", "Làm tròn lên" };
            CboCachLamTron.SelectedIndex = 0;

            // Thiết bị khác
            CboBaudRate.ItemsSource = new string[] { "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
            CboBaudRate.SelectedIndex = 2;

            CboParity.ItemsSource = new string[] { "None", "Odd", "Even", "Mark", "Space" };
            CboParity.SelectedIndex = 0;

            CboStopBits.ItemsSource = new string[] { "1", "1.5", "2" };
            CboStopBits.SelectedIndex = 0;

            // Quản trị
            CboSoHoaDonQuayVongTheo.ItemsSource = new string[] { "Không quay vòng", "Tháng", "Năm", "Ngày" };
            CboSoHoaDonQuayVongTheo.SelectedIndex = 0;

            // Tùy chọn khác
            CboPhimNhapLieuTrenLuoi.ItemsSource = new string[] { "Tab để sang ngang, Enter để xuống dòng", "Enter để sang ngang và xuống ở cuối dòng" };
            CboPhimNhapLieuTrenLuoi.SelectedIndex = 0;

            CboDoToChuHienThi.ItemsSource = new string[] { "Nhỏ (100%)", "Nhỡ (110%)", "Vừa (125%)", "To (150%)", "Rất to (200%)" };
            CboDoToChuHienThi.SelectedIndex = 0;

            CboGiaoDienChuongTrinh.ItemsSource = new string[] { "Office 2010 - Blue", "Office 2010 - Silver", "Office 2010 - Black", "Visual Studio 2019 Dark", "Material Design Light" };
            CboGiaoDienChuongTrinh.SelectedIndex = 0;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();

                // 1. Thông tin chung
                TxtCompanyName.Text = GetStr(configs, "CompanyName", "NÀNG HƯƠNG QUÁN");
                TxtCompanyAddress.Text = GetStr(configs, "CompanyAddress", "Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội");
                TxtCompanyPhone.Text = GetStr(configs, "CompanyPhone", "Điện thoại: 0909090880");
                TxtCompanyEmail.Text = GetStr(configs, "CompanyEmail", "0");
                TxtLoiCamOn.Text = GetStr(configs, "LoiCamOn", "Cảm ơn Quý khách. Hẹn gặp lại.!");
                TxtCompanyFax.Text = GetStr(configs, "CompanyFax", "0");

                // Load Logo
                var logoBytes = await LocalCauHinhService.LoadCompanyLogoAsync();
                if (logoBytes != null && logoBytes.Length > 0)
                {
                    SetLogoImage(logoBytes);
                }

                // 2. In HĐ và in chế biến
                ChkLuaChonMauKhiIn.IsChecked = GetBool(configs, "LuaChonMauKhiIn", false);
                ChkHienThiTruocKhiIn.IsChecked = GetBool(configs, "HienThiTruocKhiIn", false);
                SetComboValue(CboMauHoaDon, GetStr(configs, "MauHoaDon", "Mẫu in bill 80mm"));
                TxtSoLanIn.Text = GetStr(configs, "SoLanIn", "1");
                ChkSuDungChucNangInXuongBep.IsChecked = GetBool(configs, "SuDungChucNangInXuongBep", true);
                ChkInDoAn.IsChecked = GetBool(configs, "InDoAn", true);
                ChkInDoUong.IsChecked = GetBool(configs, "InDoUong", true);
                ChkInDichVu.IsChecked = GetBool(configs, "InDichVu", false);
                SetComboValue(CboMauInCheBien, GetStr(configs, "MauInCheBien", "Mẫu 80mm"));
                TxtSoLienInCheBien.Text = GetStr(configs, "SoLienInCheBien", "1");
                ChkInThem1LienTaiQuay.IsChecked = GetBool(configs, "InThem1LienTaiQuay", false);
                ChkInMoiDoRa1To.IsChecked = GetBool(configs, "InMoiDoRa1To", false);
                ChkInRiengDoAnUong.IsChecked = GetBool(configs, "InRiengDoAnUong", false);
                ChkTuDongInPhaCheSauKhiGoiMon.IsChecked = GetBool(configs, "TuDongInPhaCheSauKhiGoiMon", false);
                TxtThoiGianTuDongIn.Text = GetStr(configs, "ThoiGianTuDongIn", "10");
                ChkInPhaCheTheoKhuVuc.IsChecked = GetBool(configs, "InPhaCheTheoKhuVuc", false);
                ChkInThongBaoKhiChuyenBan.IsChecked = GetBool(configs, "InThongBaoKhiChuyenBan", false);
                SetComboValue(CboMauInChuyenBan, GetStr(configs, "MauInChuyenBan", "Mẫu 80mm"));
                SetComboValue(CboInHoaDonTheoKhuVuc, GetStr(configs, "InHoaDonTheoKhuVuc", "Không sử dụng"));
                ChkInMatKhauWifiTrenBill.IsChecked = GetBool(configs, "InMatKhauWifiTrenBill", false);

                // 3. Bán hàng
                ChkChoPhepNhapGiamGia.IsChecked = GetBool(configs, "ChoPhepNhapGiamGia", true);
                TxtMacDinhGiamGia.Text = GetStr(configs, "MacDinhGiamGia", "0");
                TxtMacDinhGiamGiaTienGio.Text = GetStr(configs, "MacDinhGiamGiaTienGio", "0");
                ChkCoThueSuat.IsChecked = GetBool(configs, "CoThueSuat", false);
                TxtMacDinhThueSuat.Text = GetStr(configs, "MacDinhThueSuat", "10");
                ChkCoPhiDichVu.IsChecked = GetBool(configs, "CoPhiDichVu", false);
                TxtMacDinhPhiDichVu.Text = GetStr(configs, "MacDinhPhiDichVu", "10");
                ChkNhapSoLuongSauKhiChon.IsChecked = GetBool(configs, "NhapSoLuongSauKhiChon", false);
                ChkChoPhepThayDoiNgayTrenHoaDon.IsChecked = GetBool(configs, "ChoPhepThayDoiNgayTrenHoaDon", false);
                ChkBatBuocNhapNhanVienBanHang.IsChecked = GetBool(configs, "BatBuocNhapNhanVienBanHang", false);
                ChkChoPhepInTamTinh.IsChecked = GetBool(configs, "ChoPhepInTamTinh", true);
                ChkKichHoatKhuyenMaiTuDong.IsChecked = GetBool(configs, "KichHoatKhuyenMaiTuDong", true);
                ChkChoPhepTrungTenKhachHang.IsChecked = GetBool(configs, "ChoPhepTrungTenKhachHang", false);
                ChkHienThi3NhomOGiaoDienBanHang.IsChecked = GetBool(configs, "HienThi3NhomOGiaoDienBanHang", false);
                ChkToiUuDungBanPhim.IsChecked = GetBool(configs, "ToiUuDungBanPhim", false);
                ChkSuDungMatHangMacDinh.IsChecked = GetBool(configs, "SuDungMatHangMacDinh", true);
                ChkHienThiGhiChuTrenGiaoDienBan.IsChecked = GetBool(configs, "HienThiGhiChuTrenGiaoDienBan", true);
                ChkMatHangGiaTheoGioTheoKhuVuc.IsChecked = GetBool(configs, "SuDungGiaTheoGio", true);
                SetComboValue(CboCachChonGioTinhGia, GetStr(configs, "CachChonGioTinhGia", "Giờ gọi đồ"));
                SetComboValue(CboCachChonKhachHang, GetStr(configs, "CachChonKhachHang", "Chọn bằng chuột và bàn phím"));
                SetComboValue(CboCachChonNgayGiaoDich, GetStr(configs, "CachChonNgayGiaoDich", "Theo ngày đóng hóa đơn"));
                TxtTruocGioTinhVaoNgayHomTruoc.Text = GetStr(configs, "TruocGioTinhVaoNgayHomTruoc", "0");
                SetComboValue(CboKichThuocHienThiOGiaoDienDichVu, GetStr(configs, "KichThuocHienThiOGiaoDienDichVu", "Lớn"));
                ChkInHoaDonCongGopMatHang.IsChecked = GetBool(configs, "InHoaDonCongGopMatHang", true);

                // 4. Giờ trên bill
                ChkChoPhepDoiGioTrenBill.IsChecked = GetBool(configs, "ChoPhepDoiGioTrenBill", false);
                ChkChoPhepDoiGioVaoVeTruoc.IsChecked = GetBool(configs, "ChoPhepDoiGioVaoVeTruoc", false);
                ChkChoPhepDoiGioRaVeSau.IsChecked = GetBool(configs, "ChoPhepDoiGioRaVeSau", false);
                TxtTuDongDoiGioVaoVeTruoc.Text = GetStr(configs, "TuDongDoiGioVaoVeTruoc", "0");
                TxtTuDongDoiGioRaVeSau.Text = GetStr(configs, "TuDongDoiGioRaVeSau", "0");
                TxtNgayLeDuongLich.Text = GetStr(configs, "NgayLeDuongLich", "");
                TxtNgayLeAmLich.Text = GetStr(configs, "NgayLeAmLich", "");
                TxtBlockTinhGio.Text = GetStr(configs, "BlockTinhGio", "0");
                ChkDungThoiGianKhiInTamTinh.IsChecked = GetBool(configs, "DungThoiGianKhiInTamTinh", true);
                TxtSoPhutToiDaChoPhepDung.Text = GetStr(configs, "SoPhutToiDaChoPhepDung", "5");
                SetComboValue(CboNgayCuoiTuan, GetStr(configs, "NgayCuoiTuan", "Chủ nhật"));
                ChkChoPhepDungNhieuBangGiaTrenBill.IsChecked = GetBool(configs, "ChoPhepDungNhieuBangGiaTrenBill", false);
                ChkGopChungGiamGiaTienGioVaTienHang.IsChecked = GetBool(configs, "GopChungGiamGiaTienGioVaTienHang", false);
                ChkInHoaDonChiTietTheoTungKhoangGio.IsChecked = GetBool(configs, "InHoaDonChiTietTheoTungKhoangGio", true);

                // 5. Thanh toán
                ChkChoPhepKhachNo.IsChecked = GetBool(configs, "ChoPhepKhachNo", true);
                ChkCoThanhToanVoucher.IsChecked = GetBool(configs, "CoThanhToanVoucher", false);
                ChkLuaChonVoucherTuDanhSach.IsChecked = GetBool(configs, "LuaChonVoucherTuDanhSach", false);
                ChkSuDungTheTraTruoc.IsChecked = GetBool(configs, "SuDungTheTraTruoc", true);
                ChkSuDungDiemTichLuyDeThanhToan.IsChecked = GetBool(configs, "SuDungDiemTichLuyDeThanhToan", false);
                TxtQuyDoi1DiemSangTien.Text = GetStr(configs, "QuyDoi1DiemSangTien", "1,000");
                ChkCoThanhToanThe.IsChecked = GetBool(configs, "CoThanhToanThe", true);
                ChkCoThanhToanChuyenKhoan.IsChecked = GetBool(configs, "CoThanhToanChuyenKhoan", true);
                TxtLamTronTien.Text = GetStr(configs, "LamTronTien", "1,000");
                ChkBatBuocInKhiThanhToan.IsChecked = GetBool(configs, "BatBuocInKhiThanhToan", true);
                ChkSuDungChucNangTamUngTrongDonHang.IsChecked = GetBool(configs, "SuDungChucNangTamUngTrongDonHang", true);

                // 6. Tích điểm
                TxtDoanhSoTuongUngVoi1Diem.Text = GetStr(configs, "DoanhSoTuongUngVoi1Diem", "20,000");
                SetComboValue(CboCachTinhDiem, GetStr(configs, "CachTinhDiem", "Điểm được tính trên từng hóa đơn"));
                ChkTuDongNangCapThanhVienKhiDatHanMuc.IsChecked = GetBool(configs, "TuDongNangCapThanhVienKhiDatHanMuc", true);
                ChkHienThiDiemCuaKhachHangTrenHoaDon.IsChecked = GetBool(configs, "HienThiDiemCuaKhachHangTrenHoaDon", true);

                // 7. Kho hàng
                ChkNhapMotMatHangNhieuLanTrongPhieu.IsChecked = GetBool(configs, "NhapMotMatHangNhieuLanTrongPhieu", false);
                ChkBatBuocChonNhanVienTrongNhapKho.IsChecked = GetBool(configs, "BatBuocChonNhanVienTrongNhapKho", false);
                ChkBatBuocChonNhaCungCapTrongNhapKho.IsChecked = GetBool(configs, "BatBuocChonNhaCungCapTrongNhapKho", false);
                ChkSuDung2DonViTinh.IsChecked = GetBool(configs, "SuDung2DonViTinh", false);
                ChkTuDongTinhGiaVon.IsChecked = GetBool(configs, "TuDongTinhGiaVon", true);
                ChkNhapKhoBangDauDocMaVach.IsChecked = GetBool(configs, "NhapKhoBangDauDocMaVach", true);
                ChkSuDungNhieuKho.IsChecked = GetBool(configs, "SuDungNhieuKho", true);
                ChkTuDongCapNhatGiaNhapCuaMatHangDinhLuong.IsChecked = GetBool(configs, "TuDongCapNhatGiaNhapCuaMatHangDinhLuong", true);
                ChkTuDongCapNhatGiaVonCuaMatHangDinhLuong.IsChecked = GetBool(configs, "TuDongCapNhatGiaVonCuaMatHangDinhLuong", true);

                // 8. Mặt hàng
                ChkSuDungMaHang.IsChecked = GetBool(configs, "SuDungMaHang", true);
                SetComboValue(CboTimTheo, GetStr(configs, "TimTheo", "Cả tên hàng và mã hàng"));
                SetComboValue(CboCachTim, GetStr(configs, "CachTim", "Tìm có chứa"));
                ChkSuDungGia2.IsChecked = GetBool(configs, "SuDungGia2", false);
                TxtDienGiaiGia2.Text = GetStr(configs, "DienGiaiGia2", "Giá 2");
                ChkSuDungGia3.IsChecked = GetBool(configs, "SuDungGia3", false);
                TxtDienGiaiGia3.Text = GetStr(configs, "DienGiaiGia3", "Giá 2");
                ChkSuDungGia4.IsChecked = GetBool(configs, "SuDungGia4", false);
                TxtDienGiaiGia4.Text = GetStr(configs, "DienGiaiGia4", "Giá 4");
                SetComboValue(CboSapXepThuTuTheo, GetStr(configs, "SapXepThuTuTheo", "Mã hàng"));
                ChkSuDungTenTiengAnh.IsChecked = GetBool(configs, "SuDungTenTiengAnh", false);
                ChkGiaBanLeUsd.IsChecked = GetBool(configs, "GiaBanLeUsd", true);
                TxtLamTronMatHangDichVuTheoGio.Text = GetStr(configs, "LamTronMatHangDichVuTheoGio", "0");
                SetComboValue(CboCachLamTron, GetStr(configs, "CachLamTron", "Làm tròn xuống"));

                // 9. Thiết bị khác
                ChkCoCayHienThiGia.IsChecked = GetBool(configs, "CoCayHienThiGia", false);
                TxtCongSuDung.Text = GetStr(configs, "CongSuDung", "0");
                ChkKichHoatSuDungCanDienTu.IsChecked = GetBool(configs, "KichHoatSuDungCanDienTu", false);
                TxtCongCom.Text = GetStr(configs, "CongCom", "COM4");
                SetComboValue(CboBaudRate, GetStr(configs, "BaudRate", "9600"));
                TxtDatabit.Text = GetStr(configs, "Databit", "8");
                SetComboValue(CboParity, GetStr(configs, "Parity", "None"));
                SetComboValue(CboStopBits, GetStr(configs, "StopBits", "1"));
                TxtTimeout.Text = GetStr(configs, "Timeout", "1,000");
                TxtViTri.Text = GetStr(configs, "ViTri", "6");
                TxtDoDai.Text = GetStr(configs, "DoDai", "9");
                ChkCoSuDungCongTacDienTu.IsChecked = GetBool(configs, "CoSuDungCongTacDienTu", false);
                TxtIpMayChamCong.Text = GetStr(configs, "IpMayChamCong", "0");

                // 10. Cảnh báo
                ChkHienThiCanhBao.IsChecked = GetBool(configs, "HienThiCanhBao", false);
                ChkThongBaoHangDuoiMuocAnToan.IsChecked = GetBool(configs, "ThongBaoHangDuoiMuocAnToan", true);
                ChkThongBaoKhachDenNgaySinhNhat.IsChecked = GetBool(configs, "ThongBaoKhachDenNgaySinhNhat", false);
                ChkCanhBaoChuongTrinhKhuyenMai.IsChecked = GetBool(configs, "CanhBaoChuongTrinhKhuyenMai", false);

                // 11. Quản trị
                ChkNhapMatKhauGiamDo.IsChecked = GetBool(configs, "NhapMatKhauGiamDo", false);
                TxtPasswordGiamDo.Password = GetStr(configs, "PasswordGiamDo", "0");
                ChkKichHoatLuuVetHoatDong.IsChecked = GetBool(configs, "KichHoatLuuVetHoatDong", true);
                TxtLuuVetTrong.Text = GetStr(configs, "LuuVetTrong", "20");
                ChkKhongDuocGiamDoSauKhiInPhaChe.IsChecked = GetBool(configs, "KhongDuocGiamDoSauKhiInPhaChe", true);
                ChkLuuVetInCheBien.IsChecked = GetBool(configs, "LuuVetInCheBien", true);
                ChkHeThongChayNhieuMayTram.IsChecked = GetBool(configs, "HeThongChayNhieuMayTram", true);
                TxtSoLanInToiDa.Text = GetStr(configs, "SoLanInToiDa", "0");
                TxtSoLanInTamTinhToiDa.Text = GetStr(configs, "SoLanInTamTinhToiDa", "0");
                TxtThoiGianChoPhepHuyBill.Text = GetStr(configs, "ThoiGianChoPhepHuyBill", "50");
                ChkBatChucNangKiemSoatOrder.IsChecked = GetBool(configs, "BatChucNangKiemSoatOrder", true);
                ChkPhanQuyenTruyCapTheoKhuVuc.IsChecked = GetBool(configs, "PhanQuyenTruyCapTheoKhuVuc", false);
                SetComboValue(CboSoHoaDonQuayVongTheo, GetStr(configs, "SoHoaDonQuayVongTheo", "Không quay vòng"));
                ChkKichHoatChucNangKiemDo.IsChecked = GetBool(configs, "KichHoatChucNangKiemDo", true);
                ChkNhapLyDoKhiHuyHoaDon.IsChecked = GetBool(configs, "NhapLyDoKhiHuyHoaDon", true);
                ChkNhapLyDoKhiXoaMon.IsChecked = GetBool(configs, "NhapLyDoKhiXoaMon", true);

                // 12. Cảm ứng
                ChkSuDungGiaoDienThietKe.IsChecked = GetBool(configs, "SuDungGiaoDienThietKe", false);
                ChkNhapSoLuongKhiChonMatHang.IsChecked = GetBool(configs, "NhapSoLuongKhiChonMatHang", true);
                ChkNhapSoKhachKhiMoHoaDon.IsChecked = GetBool(configs, "NhapSoKhachKhiMoHoaDon", false);
                ChkCoManHinhPhu.IsChecked = GetBool(configs, "CoManHinhPhu", true);

                // 13. Tùy chọn khác
                ChkPhieuGanNhatODuoi.IsChecked = GetBool(configs, "PhieuGanNhatODuoi", false);
                SetComboValue(CboPhimNhapLieuTrenLuoi, GetStr(configs, "PhimNhapLieuTrenLuoi", "Tab để sang ngang, Enter để xuống dòng"));
                ChkTuDongSaoLuuDuLieu.IsChecked = GetBool(configs, "TuDongSaoLuuDuLieu", true);
                TxtDuongDanSaoLuu.Text = GetStr(configs, "DuongDanSaoLuu", "0");
                TxtSoNgaySaoLuu.Text = GetStr(configs, "SoNgaySaoLuu", "1");
                TxtChucNangMacDinh.Text = await LocalCauHinhService.ResolveDefaultFunctionNameAsync(
                    GetStr(configs, "ChucNangMacDinh", "Sử dụng dịch vụ"));
                ChkLocDuLieuBoKhoangTrong.IsChecked = GetBool(configs, "LocDuLieuBoKhoangTrong", false);
                ChkHienThiXuongDongNeuDoRongCotNho.IsChecked = GetBool(configs, "HienThiXuongDongNeuDoRongCotNho", false);
                SetComboValue(CboDoToChuHienThi, GetStr(configs, "DoToChuHienThi", "Nhỏ (100%)"));
                SetComboValue(CboGiaoDienChuongTrinh, GetStr(configs, "GiaoDienChuongTrinh", "Office 2010 - Blue"));
                ChkTuDongTaiLai.IsChecked = GetBool(configs, "TuDongTaiLai", true);

                // 14. Ứng dụng
                TxtUngDung.Text = GetStr(configs, "UngDung", "POS");
                ChkNhieuCuaHang.IsChecked = GetBool(configs, "NhieuCuaHang", false);
                ChkGiuNguyenGiaoDienNhuThietKe.IsChecked = GetBool(configs, "GiuNguyenGiaoDienNhuThietKe", false);
            }
            catch (Exception ex)
            {
                TouchConfirmWindow.ShowAlert(this, "Lỗi tải thông tin cấu hình: " + ex.Message, "LỖI");
            }
        }

        private string GetStr(Dictionary<string, string> dict, string key, string defVal)
        {
            return dict.TryGetValue(key, out string val) && !string.IsNullOrWhiteSpace(val) ? val : defVal;
        }

        private bool GetBool(Dictionary<string, string> dict, string key, bool defVal)
        {
            if (dict.TryGetValue(key, out string val))
            {
                return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase) || val == "30";
            }
            return defVal;
        }

        private void SetComboValue(ComboBox cbo, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (var item in cbo.Items)
                {
                    string itemStr = item?.ToString() ?? "";
                    if (itemStr.Equals(value, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(itemStr) && (itemStr.Contains(value, StringComparison.OrdinalIgnoreCase) || value.Contains(itemStr, StringComparison.OrdinalIgnoreCase))))
                    {
                        cbo.SelectedItem = item;
                        return;
                    }
                }
            }
            if (cbo.SelectedIndex < 0 && cbo.Items.Count > 0)
            {
                cbo.SelectedIndex = 0;
            }
        }

        private void SetLogoImage(byte[] bytes)
        {
            try
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(bytes))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                ImgLogo.Source = image;
            }
            catch
            {
                ImgLogo.Source = null;
            }
        }

        private void BtnChonLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Chọn hình ảnh Logo công ty",
                Filter = "Hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Tất cả tập tin (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _newLogoBytes = File.ReadAllBytes(dlg.FileName);
                    SetLogoImage(_newLogoBytes);
                }
                catch (Exception ex)
                {
                    TouchConfirmWindow.ShowAlert(this, "Không thể tải file hình ảnh: " + ex.Message, "LỖI");
                }
            }
        }

        private void BtnVirtualKey_Click(object sender, RoutedEventArgs e)
        {
            // Focus target element or open touch numpad/keyboard if available
        }

        private async void BtnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnCapNhat.IsEnabled = false;

                var configs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // 1. Thông tin chung
                    ["CompanyName"] = TxtCompanyName.Text.Trim(),
                    ["CompanyAddress"] = TxtCompanyAddress.Text.Trim(),
                    ["CompanyPhone"] = TxtCompanyPhone.Text.Trim(),
                    ["CompanyEmail"] = TxtCompanyEmail.Text.Trim(),
                    ["LoiCamOn"] = TxtLoiCamOn.Text.Trim(),
                    ["CompanyFax"] = TxtCompanyFax.Text.Trim(),

                    // 2. In HĐ và in chế biến
                    ["LuaChonMauKhiIn"] = (ChkLuaChonMauKhiIn.IsChecked == true) ? "1" : "0",
                    ["HienThiTruocKhiIn"] = (ChkHienThiTruocKhiIn.IsChecked == true) ? "1" : "0",
                    ["MauHoaDon"] = CboMauHoaDon.Text.Trim(),
                    ["SoLanIn"] = TxtSoLanIn.Text.Trim(),
                    ["SuDungChucNangInXuongBep"] = (ChkSuDungChucNangInXuongBep.IsChecked == true) ? "1" : "0",
                    ["InDoAn"] = (ChkInDoAn.IsChecked == true) ? "1" : "0",
                    ["InDoUong"] = (ChkInDoUong.IsChecked == true) ? "1" : "0",
                    ["InDichVu"] = (ChkInDichVu.IsChecked == true) ? "1" : "0",
                    ["MauInCheBien"] = CboMauInCheBien.Text.Trim(),
                    ["SoLienInCheBien"] = TxtSoLienInCheBien.Text.Trim(),
                    ["InThem1LienTaiQuay"] = (ChkInThem1LienTaiQuay.IsChecked == true) ? "1" : "0",
                    ["InMoiDoRa1To"] = (ChkInMoiDoRa1To.IsChecked == true) ? "1" : "0",
                    ["InRiengDoAnUong"] = (ChkInRiengDoAnUong.IsChecked == true) ? "1" : "0",
                    ["TuDongInPhaCheSauKhiGoiMon"] = (ChkTuDongInPhaCheSauKhiGoiMon.IsChecked == true) ? "1" : "0",
                    ["ThoiGianTuDongIn"] = TxtThoiGianTuDongIn.Text.Trim(),
                    ["InPhaCheTheoKhuVuc"] = (ChkInPhaCheTheoKhuVuc.IsChecked == true) ? "1" : "0",
                    ["InThongBaoKhiChuyenBan"] = (ChkInThongBaoKhiChuyenBan.IsChecked == true) ? "1" : "0",
                    ["MauInChuyenBan"] = CboMauInChuyenBan.Text.Trim(),
                    ["InHoaDonTheoKhuVuc"] = CboInHoaDonTheoKhuVuc.Text.Trim(),
                    ["InMatKhauWifiTrenBill"] = (ChkInMatKhauWifiTrenBill.IsChecked == true) ? "1" : "0",

                    // 3. Bán hàng
                    ["ChoPhepNhapGiamGia"] = (ChkChoPhepNhapGiamGia.IsChecked == true) ? "1" : "0",
                    ["MacDinhGiamGia"] = TxtMacDinhGiamGia.Text.Trim(),
                    ["MacDinhGiamGiaTienGio"] = TxtMacDinhGiamGiaTienGio.Text.Trim(),
                    ["CoThueSuat"] = (ChkCoThueSuat.IsChecked == true) ? "1" : "0",
                    ["MacDinhThueSuat"] = TxtMacDinhThueSuat.Text.Trim(),
                    ["CoPhiDichVu"] = (ChkCoPhiDichVu.IsChecked == true) ? "1" : "0",
                    ["MacDinhPhiDichVu"] = TxtMacDinhPhiDichVu.Text.Trim(),
                    ["NhapSoLuongSauKhiChon"] = (ChkNhapSoLuongSauKhiChon.IsChecked == true) ? "1" : "0",
                    ["ChoPhepThayDoiNgayTrenHoaDon"] = (ChkChoPhepThayDoiNgayTrenHoaDon.IsChecked == true) ? "1" : "0",
                    ["BatBuocNhapNhanVienBanHang"] = (ChkBatBuocNhapNhanVienBanHang.IsChecked == true) ? "1" : "0",
                    ["ChoPhepInTamTinh"] = (ChkChoPhepInTamTinh.IsChecked == true) ? "1" : "0",
                    ["KichHoatKhuyenMaiTuDong"] = (ChkKichHoatKhuyenMaiTuDong.IsChecked == true) ? "1" : "0",
                    ["ChoPhepTrungTenKhachHang"] = (ChkChoPhepTrungTenKhachHang.IsChecked == true) ? "1" : "0",
                    ["HienThi3NhomOGiaoDienBanHang"] = (ChkHienThi3NhomOGiaoDienBanHang.IsChecked == true) ? "1" : "0",
                    ["ToiUuDungBanPhim"] = (ChkToiUuDungBanPhim.IsChecked == true) ? "1" : "0",
                    ["SuDungMatHangMacDinh"] = (ChkSuDungMatHangMacDinh.IsChecked == true) ? "1" : "0",
                    ["HienThiGhiChuTrenGiaoDienBan"] = (ChkHienThiGhiChuTrenGiaoDienBan.IsChecked == true) ? "1" : "0",
                    ["SuDungGiaTheoGio"] = (ChkMatHangGiaTheoGioTheoKhuVuc.IsChecked == true) ? "1" : "0",
                    ["CachChonGioTinhGia"] = CboCachChonGioTinhGia.Text.Trim(),
                    ["CachChonKhachHang"] = CboCachChonKhachHang.Text.Trim(),
                    ["CachChonNgayGiaoDich"] = CboCachChonNgayGiaoDich.Text.Trim(),
                    ["TruocGioTinhVaoNgayHomTruoc"] = TxtTruocGioTinhVaoNgayHomTruoc.Text.Trim(),
                    ["KichThuocHienThiOGiaoDienDichVu"] = CboKichThuocHienThiOGiaoDienDichVu.Text.Trim(),
                    ["InHoaDonCongGopMatHang"] = (ChkInHoaDonCongGopMatHang.IsChecked == true) ? "1" : "0",

                    // 4. Giờ trên bill
                    ["ChoPhepDoiGioTrenBill"] = (ChkChoPhepDoiGioTrenBill.IsChecked == true) ? "1" : "0",
                    ["ChoPhepDoiGioVaoVeTruoc"] = (ChkChoPhepDoiGioVaoVeTruoc.IsChecked == true) ? "1" : "0",
                    ["ChoPhepDoiGioRaVeSau"] = (ChkChoPhepDoiGioRaVeSau.IsChecked == true) ? "1" : "0",
                    ["TuDongDoiGioVaoVeTruoc"] = TxtTuDongDoiGioVaoVeTruoc.Text.Trim(),
                    ["TuDongDoiGioRaVeSau"] = TxtTuDongDoiGioRaVeSau.Text.Trim(),
                    ["NgayLeDuongLich"] = TxtNgayLeDuongLich.Text.Trim(),
                    ["NgayLeAmLich"] = TxtNgayLeAmLich.Text.Trim(),
                    ["BlockTinhGio"] = TxtBlockTinhGio.Text.Trim(),
                    ["DungThoiGianKhiInTamTinh"] = (ChkDungThoiGianKhiInTamTinh.IsChecked == true) ? "1" : "0",
                    ["SoPhutToiDaChoPhepDung"] = TxtSoPhutToiDaChoPhepDung.Text.Trim(),
                    ["NgayCuoiTuan"] = CboNgayCuoiTuan.Text.Trim(),
                    ["ChoPhepDungNhieuBangGiaTrenBill"] = (ChkChoPhepDungNhieuBangGiaTrenBill.IsChecked == true) ? "1" : "0",
                    ["GopChungGiamGiaTienGioVaTienHang"] = (ChkGopChungGiamGiaTienGioVaTienHang.IsChecked == true) ? "1" : "0",
                    ["InHoaDonChiTietTheoTungKhoangGio"] = (ChkInHoaDonChiTietTheoTungKhoangGio.IsChecked == true) ? "1" : "0",

                    // 5. Thanh toán
                    ["ChoPhepKhachNo"] = (ChkChoPhepKhachNo.IsChecked == true) ? "1" : "0",
                    ["CoThanhToanVoucher"] = (ChkCoThanhToanVoucher.IsChecked == true) ? "1" : "0",
                    ["LuaChonVoucherTuDanhSach"] = (ChkLuaChonVoucherTuDanhSach.IsChecked == true) ? "1" : "0",
                    ["SuDungTheTraTruoc"] = (ChkSuDungTheTraTruoc.IsChecked == true) ? "1" : "0",
                    ["SuDungDiemTichLuyDeThanhToan"] = (ChkSuDungDiemTichLuyDeThanhToan.IsChecked == true) ? "1" : "0",
                    ["QuyDoi1DiemSangTien"] = TxtQuyDoi1DiemSangTien.Text.Trim(),
                    ["CoThanhToanThe"] = (ChkCoThanhToanThe.IsChecked == true) ? "1" : "0",
                    ["CoThanhToanChuyenKhoan"] = (ChkCoThanhToanChuyenKhoan.IsChecked == true) ? "1" : "0",
                    ["LamTronTien"] = TxtLamTronTien.Text.Trim(),
                    ["BatBuocInKhiThanhToan"] = (ChkBatBuocInKhiThanhToan.IsChecked == true) ? "1" : "0",
                    ["SuDungChucNangTamUngTrongDonHang"] = (ChkSuDungChucNangTamUngTrongDonHang.IsChecked == true) ? "1" : "0",

                    // 6. Tích điểm
                    ["DoanhSoTuongUngVoi1Diem"] = TxtDoanhSoTuongUngVoi1Diem.Text.Trim(),
                    ["CachTinhDiem"] = CboCachTinhDiem.Text.Trim(),
                    ["TuDongNangCapThanhVienKhiDatHanMuc"] = (ChkTuDongNangCapThanhVienKhiDatHanMuc.IsChecked == true) ? "1" : "0",
                    ["HienThiDiemCuaKhachHangTrenHoaDon"] = (ChkHienThiDiemCuaKhachHangTrenHoaDon.IsChecked == true) ? "1" : "0",

                    // 7. Kho hàng
                    ["NhapMotMatHangNhieuLanTrongPhieu"] = (ChkNhapMotMatHangNhieuLanTrongPhieu.IsChecked == true) ? "1" : "0",
                    ["BatBuocChonNhanVienTrongNhapKho"] = (ChkBatBuocChonNhanVienTrongNhapKho.IsChecked == true) ? "1" : "0",
                    ["BatBuocChonNhaCungCapTrongNhapKho"] = (ChkBatBuocChonNhaCungCapTrongNhapKho.IsChecked == true) ? "1" : "0",
                    ["SuDung2DonViTinh"] = (ChkSuDung2DonViTinh.IsChecked == true) ? "1" : "0",
                    ["TuDongTinhGiaVon"] = (ChkTuDongTinhGiaVon.IsChecked == true) ? "1" : "0",
                    ["NhapKhoBangDauDocMaVach"] = (ChkNhapKhoBangDauDocMaVach.IsChecked == true) ? "1" : "0",
                    ["SuDungNhieuKho"] = (ChkSuDungNhieuKho.IsChecked == true) ? "1" : "0",
                    ["TuDongCapNhatGiaNhapCuaMatHangDinhLuong"] = (ChkTuDongCapNhatGiaNhapCuaMatHangDinhLuong.IsChecked == true) ? "1" : "0",
                    ["TuDongCapNhatGiaVonCuaMatHangDinhLuong"] = (ChkTuDongCapNhatGiaVonCuaMatHangDinhLuong.IsChecked == true) ? "1" : "0",

                    // 8. Mặt hàng
                    ["SuDungMaHang"] = (ChkSuDungMaHang.IsChecked == true) ? "1" : "0",
                    ["TimTheo"] = CboTimTheo.Text.Trim(),
                    ["CachTim"] = CboCachTim.Text.Trim(),
                    ["SuDungGia2"] = (ChkSuDungGia2.IsChecked == true) ? "1" : "0",
                    ["DienGiaiGia2"] = TxtDienGiaiGia2.Text.Trim(),
                    ["SuDungGia3"] = (ChkSuDungGia3.IsChecked == true) ? "1" : "0",
                    ["DienGiaiGia3"] = TxtDienGiaiGia3.Text.Trim(),
                    ["SuDungGia4"] = (ChkSuDungGia4.IsChecked == true) ? "1" : "0",
                    ["DienGiaiGia4"] = TxtDienGiaiGia4.Text.Trim(),
                    ["SapXepThuTuTheo"] = CboSapXepThuTuTheo.Text.Trim(),
                    ["SuDungTenTiengAnh"] = (ChkSuDungTenTiengAnh.IsChecked == true) ? "1" : "0",
                    ["GiaBanLeUsd"] = (ChkGiaBanLeUsd.IsChecked == true) ? "1" : "0",
                    ["LamTronMatHangDichVuTheoGio"] = TxtLamTronMatHangDichVuTheoGio.Text.Trim(),
                    ["CachLamTron"] = CboCachLamTron.Text.Trim(),

                    // 9. Thiết bị khác
                    ["CoCayHienThiGia"] = (ChkCoCayHienThiGia.IsChecked == true) ? "1" : "0",
                    ["CongSuDung"] = TxtCongSuDung.Text.Trim(),
                    ["KichHoatSuDungCanDienTu"] = (ChkKichHoatSuDungCanDienTu.IsChecked == true) ? "1" : "0",
                    ["CongCom"] = TxtCongCom.Text.Trim(),
                    ["BaudRate"] = CboBaudRate.Text.Trim(),
                    ["Databit"] = TxtDatabit.Text.Trim(),
                    ["Parity"] = CboParity.Text.Trim(),
                    ["StopBits"] = CboStopBits.Text.Trim(),
                    ["Timeout"] = TxtTimeout.Text.Trim(),
                    ["ViTri"] = TxtViTri.Text.Trim(),
                    ["DoDai"] = TxtDoDai.Text.Trim(),
                    ["CoSuDungCongTacDienTu"] = (ChkCoSuDungCongTacDienTu.IsChecked == true) ? "1" : "0",
                    ["IpMayChamCong"] = TxtIpMayChamCong.Text.Trim(),

                    // 10. Cảnh báo
                    ["HienThiCanhBao"] = (ChkHienThiCanhBao.IsChecked == true) ? "1" : "0",
                    ["ThongBaoHangDuoiMuocAnToan"] = (ChkThongBaoHangDuoiMuocAnToan.IsChecked == true) ? "1" : "0",
                    ["ThongBaoKhachDenNgaySinhNhat"] = (ChkThongBaoKhachDenNgaySinhNhat.IsChecked == true) ? "1" : "0",
                    ["CanhBaoChuongTrinhKhuyenMai"] = (ChkCanhBaoChuongTrinhKhuyenMai.IsChecked == true) ? "1" : "0",

                    // 11. Quản trị
                    ["NhapMatKhauGiamDo"] = (ChkNhapMatKhauGiamDo.IsChecked == true) ? "1" : "0",
                    ["PasswordGiamDo"] = TxtPasswordGiamDo.Password.Trim(),
                    ["KichHoatLuuVetHoatDong"] = (ChkKichHoatLuuVetHoatDong.IsChecked == true) ? "1" : "0",
                    ["LuuVetTrong"] = TxtLuuVetTrong.Text.Trim(),
                    ["KhongDuocGiamDoSauKhiInPhaChe"] = (ChkKhongDuocGiamDoSauKhiInPhaChe.IsChecked == true) ? "1" : "0",
                    ["LuuVetInCheBien"] = (ChkLuuVetInCheBien.IsChecked == true) ? "1" : "0",
                    ["HeThongChayNhieuMayTram"] = (ChkHeThongChayNhieuMayTram.IsChecked == true) ? "1" : "0",
                    ["SoLanInToiDa"] = TxtSoLanInToiDa.Text.Trim(),
                    ["SoLanInTamTinhToiDa"] = TxtSoLanInTamTinhToiDa.Text.Trim(),
                    ["ThoiGianChoPhepHuyBill"] = TxtThoiGianChoPhepHuyBill.Text.Trim(),
                    ["BatChucNangKiemSoatOrder"] = (ChkBatChucNangKiemSoatOrder.IsChecked == true) ? "1" : "0",
                    ["PhanQuyenTruyCapTheoKhuVuc"] = (ChkPhanQuyenTruyCapTheoKhuVuc.IsChecked == true) ? "1" : "0",
                    ["SoHoaDonQuayVongTheo"] = CboSoHoaDonQuayVongTheo.Text.Trim(),
                    ["KichHoatChucNangKiemDo"] = (ChkKichHoatChucNangKiemDo.IsChecked == true) ? "1" : "0",
                    ["NhapLyDoKhiHuyHoaDon"] = (ChkNhapLyDoKhiHuyHoaDon.IsChecked == true) ? "1" : "0",
                    ["NhapLyDoKhiXoaMon"] = (ChkNhapLyDoKhiXoaMon.IsChecked == true) ? "1" : "0",

                    // 12. Cảm ứng
                    ["SuDungGiaoDienThietKe"] = (ChkSuDungGiaoDienThietKe.IsChecked == true) ? "1" : "0",
                    ["NhapSoLuongKhiChonMatHang"] = (ChkNhapSoLuongKhiChonMatHang.IsChecked == true) ? "1" : "0",
                    ["NhapSoKhachKhiMoHoaDon"] = (ChkNhapSoKhachKhiMoHoaDon.IsChecked == true) ? "1" : "0",
                    ["CoManHinhPhu"] = (ChkCoManHinhPhu.IsChecked == true) ? "1" : "0",

                    // 13. Tùy chọn khác
                    ["TuDongSaoLuuDuLieu"] = (ChkTuDongSaoLuuDuLieu.IsChecked == true) ? "1" : "0",
                    ["SoNgaySaoLuu"] = TxtSoNgaySaoLuu.Text.Trim(),
                    ["DuongDanSaoLuu"] = TxtDuongDanSaoLuu.Text.Trim(),
                    ["PhimNhapLieuTrenLuoi"] = CboPhimNhapLieuTrenLuoi.Text.Trim(),
                    ["PhieuGanNhatODuoi"] = (ChkPhieuGanNhatODuoi.IsChecked == true) ? "1" : "0",
                    ["ChucNangMacDinh"] = TxtChucNangMacDinh.Text.Trim(),
                    ["LocDuLieuBoKhoangTrong"] = (ChkLocDuLieuBoKhoangTrong.IsChecked == true) ? "1" : "0",
                    ["HienThiXuongDongNeuDoRongCotNho"] = (ChkHienThiXuongDongNeuDoRongCotNho.IsChecked == true) ? "1" : "0",
                    ["DoToChuHienThi"] = CboDoToChuHienThi.Text.Trim(),
                    ["GiaoDienChuongTrinh"] = CboGiaoDienChuongTrinh.Text.Trim(),
                    ["TuDongTaiLai"] = (ChkTuDongTaiLai.IsChecked == true) ? "1" : "0",

                    // 14. Ứng dụng
                    ["UngDung"] = TxtUngDung.Text.Trim(),
                    ["NhieuCuaHang"] = (ChkNhieuCuaHang.IsChecked == true) ? "1" : "0",
                    ["GiuNguyenGiaoDienNhuThietKe"] = (ChkGiuNguyenGiaoDienNhuThietKe.IsChecked == true) ? "1" : "0"
                };

                var (ok, error) = await LocalCauHinhService.SaveAllConfigsAsync(configs, null, _newLogoBytes);
                if (ok)
                {
                    TouchConfirmWindow.ShowAlert(this, "ĐÃ LƯU THÔNG TIN CẤU HÌNH HỆ THỐNG THÀNH CÔNG!", "THÔNG BÁO");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    TouchConfirmWindow.ShowAlert(this, "CÓ LỖI XẢY RA KHI LƯU CẤU HÌNH:\n" + (error ?? "Lỗi không xác định"), "LỖI");
                }
            }
            catch (Exception ex)
            {
                TouchConfirmWindow.ShowAlert(this, "Lỗi khi ghi dữ liệu cấu hình: " + ex.Message, "LỖI");
            }
            finally
            {
                BtnCapNhat.IsEnabled = true;
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
