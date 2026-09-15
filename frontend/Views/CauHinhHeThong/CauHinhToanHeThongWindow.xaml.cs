using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dapper;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CauHinhHeThong
{
    public partial class CauHinhToanHeThongWindow : Window
    {
        private byte[] _newLogoBytes = null;
        private readonly List<FrameworkElement> _tabPanels = new List<FrameworkElement>();

        public CauHinhToanHeThongWindow()
        {
            InitializeComponent();
            InitializePanelsList();
            InitializeComboBoxes();
            Loaded += async (s, e) => await LoadDataAsync();
        }

        private void InitializePanelsList()
        {
            _tabPanels.Clear();
            _tabPanels.Add(TabThongTinChung);
            _tabPanels.Add(TabInHoaDon);
            _tabPanels.Add(TabMauHoaDon);
            _tabPanels.Add(TabSoPhieu);
            _tabPanels.Add(TabBanHang);
            _tabPanels.Add(TabThanhToan);
            _tabPanels.Add(TabTichDiem);
            _tabPanels.Add(TabKhoHang);
            _tabPanels.Add(TabMatHang);
            _tabPanels.Add(TabThietBiKhac);
            _tabPanels.Add(TabCanhBao);
            _tabPanels.Add(TabQuanTri);
            _tabPanels.Add(TabCamUng);
            _tabPanels.Add(TabTuyChonKhac);
        }

        private void InitializeComboBoxes()
        {
            // Mẫu hóa đơn
            string[] mauHoaDonList = new string[]
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
            CboMauHoaDon.ItemsSource = mauHoaDonList;
            CboMauHoaDon.SelectedIndex = 5; // "Mẫu in bill 80mm"

            // Mẫu in chế biến
            CboMauInCheBien.ItemsSource = new string[] { "Mẫu 58mm", "Mẫu 80mm" };
            CboMauInCheBien.SelectedIndex = 1; // "Mẫu 80mm"

            // Mẫu in chuyển bàn
            CboMauInChuyenBan.ItemsSource = new string[] { "Mẫu 58mm", "Mẫu 80mm" };
            CboMauInChuyenBan.SelectedIndex = 1; // "Mẫu 80mm"

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
            CboCachChonNgayGiaoDich.SelectedIndex = 1; // Default: Theo ngày đóng hóa đơn

            // Tích điểm
            CboCachTinhDiem.ItemsSource = new string[] { "Điểm được tính trên từng hóa đơn", "Điểm được tính trên tổng doanh số" };
            CboCachTinhDiem.SelectedIndex = 0;

            // Mặt hàng
            CboTimTheo.ItemsSource = new string[] { "Cả tên hàng và mã hàng", "Tên hàng", "Mã hàng" };
            CboTimTheo.SelectedIndex = 0;

            CboCachTim.ItemsSource = new string[] { "Chứa cụm từ tìm kiếm", "Bắt đầu bằng cụm từ" };
            CboCachTim.SelectedIndex = 0;

            CboSapXepThuTuTheo.ItemsSource = new string[] { "Mã hàng", "Tên hàng" };
            CboSapXepThuTuTheo.SelectedIndex = 0;

            CboCachLamTron.ItemsSource = new string[] { "Làm tròn xuống", "Làm tròn giữa", "Làm tròn lên" };
            CboCachLamTron.SelectedIndex = 0;

            // Thiết bị khác
            CboBaudRate.ItemsSource = new string[] { "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
            CboBaudRate.SelectedIndex = 2; // 9600

            CboParity.ItemsSource = new string[] { "None", "Odd", "Even", "Mark", "Space" };
            CboParity.SelectedIndex = 0;

            CboStopBits.ItemsSource = new string[] { "1", "1.5", "2" };
            CboStopBits.SelectedIndex = 0;

            // Quản trị
            CboSoHoaDonQuayVongTheo.ItemsSource = new string[] { "Tháng", "Năm", "Ngày", "Không quay vòng" };
            CboSoHoaDonQuayVongTheo.SelectedIndex = 0;

            // Tùy chọn khác
            CboPhimNhapLieuTrenLuoi.ItemsSource = new string[] { "Tab để sang ngang, Enter để xuống dòng", "Enter để sang ngang và xuống ở cuối dòng" };
            CboPhimNhapLieuTrenLuoi.SelectedIndex = 0;

            CboDoToChuHienThi.ItemsSource = new string[] { "Nhỏ (100%)", "Nhỡ (110%)", "Vừa (125%)", "To (150%)", "Rất to (200%)" };
            CboDoToChuHienThi.SelectedIndex = 0;

            CboGiaoDienChuongTrinh.ItemsSource = new string[] { "Office 2010 - Blue", "Office 2010 - Silver", "Office 2010 - Black", "Visual Studio 2019 Dark", "Material Design Light" };
            CboGiaoDienChuongTrinh.SelectedIndex = 0;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                var tableFormats = await LocalCauHinhService.LoadTableFormatsAsync();

                // 1. Thông tin chung
                TxtCompanyName.Text = GetStr(configs, "CompanyName", "NÀNG HƯƠNG QUÁN");
                TxtCompanyAddress.Text = GetStr(configs, "CompanyAddress", "Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội");
                TxtCompanyPhone.Text = GetStr(configs, "CompanyPhone", "Điện thoại: 0909090880");
                TxtCompanyEmail.Text = GetStr(configs, "CompanyEmail", "");
                TxtLoiCamOn.Text = GetStr(configs, "LoiCamOn", "Cảm ơn Quý khách. Hẹn gặp lại.!");
                TxtCompanyFax.Text = GetStr(configs, "CompanyFax", "");

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
                ChkInDoKhac.IsChecked = GetBool(configs, "InDoKhac", true);
                SetComboValue(CboMauInCheBien, GetStr(configs, "MauInCheBien", "Mẫu 80mm"));
                TxtSoLienInCheBien.Text = GetStr(configs, "SoLienInCheBien", "1");
                ChkInThem1LienTaiQuay.IsChecked = GetBool(configs, "InThem1LienTaiQuay", true);
                ChkInMoiDoRa1To.IsChecked = GetBool(configs, "InMoiDoRa1To", false);
                ChkInRiengDoAnUong.IsChecked = GetBool(configs, "InRiengDoAnUong", false);
                ChkTuDongInPhaCheSauKhiGoiMon.IsChecked = GetBool(configs, "TuDongInPhaCheSauKhiGoiMon", false);
                TxtThoiGianTuDongIn.Text = GetStr(configs, "ThoiGianTuDongIn", "10");
                ChkInPhaCheTheoKhuVuc.IsChecked = GetBool(configs, "InPhaCheTheoKhuVuc", false);
                ChkInThongBaoKhiChuyenBan.IsChecked = GetBool(configs, "InThongBaoKhiChuyenBan", false);
                SetComboValue(CboMauInChuyenBan, GetStr(configs, "MauInChuyenBan", "Mẫu 80mm"));
                SetComboValue(CboInHoaDonTheoKhuVuc, GetStr(configs, "InHoaDonTheoKhuVuc", "Không sử dụng"));
                ChkInMatKhauWifiTrenBill.IsChecked = GetBool(configs, "InMatKhauWifiTrenBill", false);
                string wifiName = GetStr(configs, "WifiName", "");
                string wifiPass = GetStr(configs, "WifiPass", "");
                TxtWifiName.Text = wifiName;
                TxtWifiPass.Text = wifiPass;
                TxtWifiNameChung.Text = wifiName;
                TxtWifiPassChung.Text = wifiPass;

                // 3. Số phiếu
                TxtFormatBaoGia.Text = GetFormatStr(tableFormats, "TBAOGIA", "BG(yy)/(*****)");
                TxtFormatHoaDon.Text = GetFormatStr(tableFormats, "TSOHOADON", "(yy)(******)");
                TxtFormatNhapKho.Text = GetFormatStr(tableFormats, "TPHIEUNHAPKHO", "PN(yy)/(*****)");
                TxtFormatXuatKho.Text = GetFormatStr(tableFormats, "TPHIEUXUATKHO", "PX(yy)/(*****)");
                TxtFormatChuyenKho.Text = GetFormatStr(tableFormats, "TPHIEUCHUYENKHO", "PCK(yy)/(*****)");
                TxtFormatKiemKe.Text = GetFormatStr(tableFormats, "TPHIEUKIEMKE", "PKK(yy)/(*****)");
                TxtFormatDatHang.Text = GetFormatStr(tableFormats, "TDATHANG", "DH(yy)/(*****)");
                TxtFormatPhieuThu.Text = GetFormatStr(tableFormats, "TPHIEUTHU", "PT(yy)/(*****)");
                TxtFormatPhieuChi.Text = GetFormatStr(tableFormats, "TPHIEUCHI", "PC(yy)/(*****)");
                TxtFormatThuCongNo.Text = GetFormatStr(tableFormats, "TPHIEUTHUCONGNO", "PTCN(yy)/(*****)");
                TxtFormatBangLuong.Text = GetFormatStr(tableFormats, "TBANGLUONG", "");

                // 4. Bán hàng
                ChkChoPhepNhapGiamGia.IsChecked = GetBool(configs, "ChoPhepNhapGiamGia", true);
                TxtMacDinhGiamGia.Text = GetStr(configs, "MacDinhGiamGia", "0");
                TxtMacDinhGiamGiaTienGio.Text = GetStr(configs, "MacDinhGiamGiaTienGio", "0");
                ChkCoThueSuat.IsChecked = GetBool(configs, "CoThueSuat", false);
                TxtMacDinhThueSuat.Text = GetStr(configs, "MacDinhThueSuat", "0");
                ChkCoPhiDichVu.IsChecked = GetBool(configs, "CoPhiDichVu", false);
                TxtMacDinhPhiDichVu.Text = GetStr(configs, "MacDinhPhiDichVu", "0");
                ChkNhapSoLuongSauKhiChon.IsChecked = GetBool(configs, "NhapSoLuongSauKhiChon", false) || GetBool(configs, "HienThiCuaSoNhapSoLuongKhiQuetMaVach", false);
                ChkChoPhepThayDoiNgayTrenHoaDon.IsChecked = GetBool(configs, "ChoPhepThayDoiNgayTrenHoaDon", false);
                ChkBatBuocNhapNhanVienBanHang.IsChecked = GetBool(configs, "BatBuocNhapNhanVienBanHang", false);
                ChkChoPhepInTamTinh.IsChecked = GetBool(configs, "ChoPhepInTamTinh", true);
                ChkKichHoatKhuyenMaiTuDong.IsChecked = GetBool(configs, "KichHoatKhuyenMaiTuDong", true);
                ChkChoPhepTrungTenKhachHang.IsChecked = GetBool(configs, "ChoPhepTrungTenKhachHang", false);
                ChkSuDungMatHangMacDinh.IsChecked = GetBool(configs, "SuDungMatHangMacDinh", true);
                ChkHienThiGhiChuTrenGiaoDienBan.IsChecked = GetBool(configs, "HienThiGhiChuTrenGiaoDienBan", false);
                SetComboValue(CboCachChonGioTinhGia, GetStr(configs, "CachChonGioTinhGia", "Giờ gọi đồ"));
                SetComboValue(CboCachChonKhachHang, GetStr(configs, "CachChonKhachHang", "Chọn bằng chuột và bàn phím"));
                SetComboValue(CboCachChonNgayGiaoDich, GetStr(configs, "CachChonNgayGiaoDich", "Theo ngày đóng hóa đơn"));
                TxtTruocGioTinhVaoNgayHomTruoc.Text = GetStr(configs, "TruocGioTinhVaoNgayHomTruoc", "0");

                // 5. Thanh toán
                ChkChoPhepKhachNo.IsChecked = GetBool(configs, "ChoPhepKhachNo", false);
                ChkCoThanhToanVoucher.IsChecked = GetBool(configs, "CoThanhToanVoucher", false);
                ChkLuaChonVoucherTuDanhSach.IsChecked = GetBool(configs, "LuaChonVoucherTuDanhSach", false);
                ChkSuDungTheTraTruoc.IsChecked = GetBool(configs, "SuDungTheTraTruoc", true);
                ChkSuDungDiemTichLuyDeThanhToan.IsChecked = GetBool(configs, "SuDungDiemTichLuyDeThanhToan", false);
                TxtQuyDoi1DiemSangTien.Text = GetStr(configs, "QuyDoi1DiemSangTien", "1,000");
                ChkCoThanhToanThe.IsChecked = GetBool(configs, "CoThanhToanThe", true);
                ChkCoThanhToanChuyenKhoan.IsChecked = GetBool(configs, "CoThanhToanChuyenKhoan", false);
                TxtLamTronTien.Text = GetStr(configs, "LamTronTien", "1,000");
                ChkBatBuocInKhiThanhToan.IsChecked = GetBool(configs, "BatBuocInKhiThanhToan", false);
                ChkSuDungChucNangTamUngTrongDonHang.IsChecked = GetBool(configs, "SuDungChucNangTamUngTrongDonHang", false);

                // 6. Tích điểm
                TxtDoanhSoTuongUngVoi1Diem.Text = GetStr(configs, "DoanhSoTuongUngVoi1Diem", "20,000");
                SetComboValue(CboCachTinhDiem, GetStr(configs, "CachTinhDiem", "Điểm được tính trên từng hóa đơn"));
                ChkTuDongNangCapThanhVienKhiDatHanMuc.IsChecked = GetBool(configs, "TuDongNangCapThanhVienKhiDatHanMuc", true);
                ChkHienThiDiemCuaKhachHangTrenHoaDon.IsChecked = GetBool(configs, "HienThiDiemCuaKhachHangTrenHoaDon", false);

                // 7. Kho hàng
                ChkNhapMotMatHangNhieuLanTrongPhieu.IsChecked = GetBool(configs, "NhapMotMatHangNhieuLanTrongPhieu", false);
                ChkBatBuocChonNhanVienTrongNhapKho.IsChecked = GetBool(configs, "BatBuocChonNhanVienTrongNhapKho", false);
                ChkBatBuocChonNhaCungCapTrongNhapKho.IsChecked = GetBool(configs, "BatBuocChonNhaCungCapTrongNhapKho", false);
                ChkSuDung2DonViTinh.IsChecked = GetBool(configs, "SuDung2DonViTinh", true);
                ChkTuDongTinhGiaVon.IsChecked = GetBool(configs, "TuDongTinhGiaVon", true);
                ChkNhapKhoBangDauDocMaVach.IsChecked = GetBool(configs, "NhapKhoBangDauDocMaVach", false);
                ChkSuDungNhieuKho.IsChecked = GetBool(configs, "SuDungNhieuKho", false);
                ChkTuDongCapNhatGiaNhapCuaMatHangDinhLuong.IsChecked = GetBool(configs, "TuDongCapNhatGiaNhapCuaMatHangDinhLuong", true);
                ChkTuDongCapNhatGiaVonCuaMatHangDinhLuong.IsChecked = GetBool(configs, "TuDongCapNhatGiaVonCuaMatHangDinhLuong", true);

                // 8. Mặt hàng
                ChkSuDungMaHang.IsChecked = GetBool(configs, "SuDungMaHang", true);
                SetComboValue(CboTimTheo, GetStr(configs, "TimTheo", "Cả tên hàng và mã hàng"));
                SetComboValue(CboCachTim, GetStr(configs, "CachTim", ""));
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
                TxtCongSuDung.Text = GetStr(configs, "CongSuDung", "");
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
                TxtIpMayChamCong.Text = GetStr(configs, "IpMayChamCong", "");

                // 10. Cảnh báo
                ChkHienThiCanhBao.IsChecked = GetBool(configs, "HienThiCanhBao", false);
                ChkThongBaoHangDuoiMuocAnToan.IsChecked = GetBool(configs, "ThongBaoHangDuoiMuocAnToan", false);
                ChkThongBaoKhachDenNgaySinhNhat.IsChecked = GetBool(configs, "ThongBaoKhachDenNgaySinhNhat", false);
                ChkCanhBaoChuongTrinhKhuyenMai.IsChecked = GetBool(configs, "CanhBaoChuongTrinhKhuyenMai", false);

                // 11. Quản trị
                ChkNhapMatKhauGiamDo.IsChecked = GetBool(configs, "NhapMatKhauGiamDo", false);
                TxtPasswordGiamDo.Password = GetStr(configs, "PasswordGiamDo", "");
                ChkKichHoatLuuVetHoatDong.IsChecked = GetBool(configs, "KichHoatLuuVetHoatDong", true);
                TxtLuuVetTrong.Text = GetStr(configs, "LuuVetTrong", "20");
                ChkKhongDuocGiamDoSauKhiInPhaChe.IsChecked = GetBool(configs, "KhongDuocGiamDoSauKhiInPhaChe", true);
                ChkLuuVetInCheBien.IsChecked = GetBool(configs, "LuuVetInCheBien", true);
                ChkHeThongChayNhieuMayTram.IsChecked = GetBool(configs, "HeThongChayNhieuMayTram", true);
                TxtSoLanInToiDa.Text = GetStr(configs, "SoLanInToiDa", "0");
                TxtSoLanInTamTinhToiDa.Text = GetStr(configs, "SoLanInTamTinhToiDa", "0");
                TxtThoiGianChoPhepHuyBill.Text = GetStr(configs, "ThoiGianChoPhepHuyBill", "50");
                ChkBatChucNangKiemSoatOrder.IsChecked = GetBool(configs, "BatChucNangKiemSoatOrder", false);
                ChkPhanQuyenTruyCapTheoKhuVuc.IsChecked = GetBool(configs, "PhanQuyenTruyCapTheoKhuVuc", false);
                SetComboValue(CboSoHoaDonQuayVongTheo, GetStr(configs, "SoHoaDonQuayVongTheo", "Tháng"));
                ChkKichHoatChucNangKiemDo.IsChecked = GetBool(configs, "KichHoatChucNangKiemDo", false);
                ChkNhapLyDoKhiHuyHoaDon.IsChecked = GetBool(configs, "NhapLyDoKhiHuyHoaDon", true);
                ChkNhapLyDoKhiXoaMon.IsChecked = GetBool(configs, "NhapLyDoKhiXoaMon", false);

                // 12. Cảm ứng
                ChkSuDungGiaoDienThietKe.IsChecked = GetBool(configs, "SuDungGiaoDienThietKe", false);
                ChkNhapSoLuongKhiChonMatHang.IsChecked = GetBool(configs, "NhapSoLuongKhiChonMatHang", false);
                ChkNhapSoKhachKhiMoHoaDon.IsChecked = GetBool(configs, "NhapSoKhachKhiMoHoaDon", false);
                ChkCoManHinhPhu.IsChecked = GetBool(configs, "CoManHinhPhu", true);

                // 13. Tùy chọn khác
                ChkPhieuGanNhatODuoi.IsChecked = GetBool(configs, "PhieuGanNhatODuoi", false);
                SetComboValue(CboPhimNhapLieuTrenLuoi, GetStr(configs, "PhimNhapLieuTrenLuoi", "Tab để sang ngang, Enter để xuống dòng"));
                ChkTuDongSaoLuuDuLieu.IsChecked = GetBool(configs, "TuDongSaoLuuDuLieu", true);
                TxtDuongDanSaoLuu.Text = GetStr(configs, "DuongDanSaoLuu", "");
                TxtSoNgaySaoLuu.Text = GetStr(configs, "SoNgaySaoLuu", "1");
                TxtChucNangMacDinh.Text = await LocalCauHinhService.ResolveDefaultFunctionNameAsync(
                    GetStr(configs, "ChucNangMacDinh", "Sử dụng dịch vụ"));
                ChkLocDuLieuBoKhoangTrong.IsChecked = GetBool(configs, "LocDuLieuBoKhoangTrong", false);
                ChkHienThiXuongDongNeuDoRongCotNho.IsChecked = GetBool(configs, "HienThiXuongDongNeuDoRongCotNho", false);
                SetComboValue(CboDoToChuHienThi, GetStr(configs, "DoToChuHienThi", "Nhỏ (100%)"));
                SetComboValue(CboGiaoDienChuongTrinh, GetStr(configs, "GiaoDienChuongTrinh", "Office 2010 - Blue"));
                ChkTuDongTaiLai.IsChecked = GetBool(configs, "TuDongTaiLai", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin cấu hình: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private string GetFormatStr(Dictionary<string, string> dict, string key, string defVal)
        {
            return dict.TryGetValue(key, out string val) && !string.IsNullOrWhiteSpace(val) ? val : defVal;
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

        private void SetLogoManHinhPhuImage(byte[] bytes)
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
                ImgLogoManHinhPhu.Source = image;
            }
            catch
            {
                ImgLogoManHinhPhu.Source = null;
            }
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is ListBoxItem selectedItem && int.TryParse(selectedItem.Tag?.ToString(), out int index))
            {
                for (int i = 0; i < _tabPanels.Count; i++)
                {
                    _tabPanels[i].Visibility = (i == index) ? Visibility.Visible : Visibility.Collapsed;
                }
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
                    MessageBox.Show("Không thể tải file hình ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnChonLogoManHinhPhu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Chọn hình ảnh Logo màn hình phụ",
                Filter = "Hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Tất cả tập tin (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bytes = File.ReadAllBytes(dlg.FileName);
                    SetLogoManHinhPhuImage(bytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể tải file hình ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void TxtSearchConfig_TextChanged(object sender, TextChangedEventArgs e)
        {
            string q = TxtSearchConfig.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(q)) return;

            // Tìm tab phù hợp
            for (int i = 0; i < CategoryListBox.Items.Count; i++)
            {
                if (CategoryListBox.Items[i] is ListBoxItem item)
                {
                    string header = item.Content is StackPanel sp && sp.Children.Count > 1 && sp.Children[1] is TextBlock tb ? tb.Text : "";
                    if (header.ToLower().Contains(q))
                    {
                        CategoryListBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TxtSearchConfig.Focus();
                TxtSearchConfig.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnGhiDuLieu.IsEnabled = false;

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
                    ["InDoKhac"] = (ChkInDoKhac.IsChecked == true) ? "1" : "0",
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
                    ["WifiName"] = TxtWifiName.Text.Trim(),
                    ["WifiPass"] = TxtWifiPass.Text.Trim(),

                    // 4. Bán hàng
                    ["ChoPhepNhapGiamGia"] = (ChkChoPhepNhapGiamGia.IsChecked == true) ? "1" : "0",
                    ["MacDinhGiamGia"] = TxtMacDinhGiamGia.Text.Trim(),
                    ["MacDinhGiamGiaTienGio"] = TxtMacDinhGiamGiaTienGio.Text.Trim(),
                    ["CoThueSuat"] = (ChkCoThueSuat.IsChecked == true) ? "1" : "0",
                    ["MacDinhThueSuat"] = TxtMacDinhThueSuat.Text.Trim(),
                    ["CoPhiDichVu"] = (ChkCoPhiDichVu.IsChecked == true) ? "1" : "0",
                    ["MacDinhPhiDichVu"] = TxtMacDinhPhiDichVu.Text.Trim(),
                    ["HienThiCuaSoNhapSoLuongKhiQuetMaVach"] = (ChkNhapSoLuongSauKhiChon.IsChecked == true) ? "1" : "0",
                    ["NhapSoLuongSauKhiChon"] = (ChkNhapSoLuongSauKhiChon.IsChecked == true) ? "1" : "0",
                    ["ChoPhepThayDoiNgayTrenHoaDon"] = (ChkChoPhepThayDoiNgayTrenHoaDon.IsChecked == true) ? "1" : "0",
                    ["BatBuocNhapNhanVienBanHang"] = (ChkBatBuocNhapNhanVienBanHang.IsChecked == true) ? "1" : "0",
                    ["ChoPhepInTamTinh"] = (ChkChoPhepInTamTinh.IsChecked == true) ? "1" : "0",
                    ["KichHoatKhuyenMaiTuDong"] = (ChkKichHoatKhuyenMaiTuDong.IsChecked == true) ? "1" : "0",
                    ["ChoPhepTrungTenKhachHang"] = (ChkChoPhepTrungTenKhachHang.IsChecked == true) ? "1" : "0",
                    ["SuDungMatHangMacDinh"] = (ChkSuDungMatHangMacDinh.IsChecked == true) ? "1" : "0",
                    ["HienThiGhiChuTrenGiaoDienBan"] = (ChkHienThiGhiChuTrenGiaoDienBan.IsChecked == true) ? "1" : "0",
                    ["CachChonGioTinhGia"] = CboCachChonGioTinhGia.Text.Trim(),
                    ["CachChonKhachHang"] = CboCachChonKhachHang.Text.Trim(),
                    ["CachChonNgayGiaoDich"] = CboCachChonNgayGiaoDich.Text.Trim(),
                    ["TruocGioTinhVaoNgayHomTruoc"] = TxtTruocGioTinhVaoNgayHomTruoc.Text.Trim(),

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
                    ["TuDongTaiLai"] = (ChkTuDongTaiLai.IsChecked == true) ? "1" : "0"
                };

                // STABLEDESC format mapping
                var tableFormats = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["TBAOGIA"] = TxtFormatBaoGia.Text.Trim(),
                    ["TSOHOADON"] = TxtFormatHoaDon.Text.Trim(),
                    ["TPHIEUNHAPKHO"] = TxtFormatNhapKho.Text.Trim(),
                    ["TPHIEUXUATKHO"] = TxtFormatXuatKho.Text.Trim(),
                    ["TPHIEUCHUYENKHO"] = TxtFormatChuyenKho.Text.Trim(),
                    ["TPHIEUKIEMKE"] = TxtFormatKiemKe.Text.Trim(),
                    ["TDATHANG"] = TxtFormatDatHang.Text.Trim(),
                    ["TPHIEUTHU"] = TxtFormatPhieuThu.Text.Trim(),
                    ["TPHIEUCHI"] = TxtFormatPhieuChi.Text.Trim(),
                    ["TPHIEUTHUCONGNO"] = TxtFormatThuCongNo.Text.Trim(),
                    ["TBANGLUONG"] = TxtFormatBangLuong.Text.Trim()
                };

                var (ok, error) = await LocalCauHinhService.SaveAllConfigsAsync(configs, tableFormats, _newLogoBytes);
                if (ok)
                {
                    MessageBox.Show("Đã lưu thông tin cấu hình hệ thống thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi lưu cấu hình vào cơ sở dữ liệu:\n" + (error ?? "Lỗi không xác định"), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ghi dữ liệu cấu hình: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGhiDuLieu.IsEnabled = true;
            }
        }

        private bool _isSyncingWifi = false;

        private void TxtWifiName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingWifi) return;
            _isSyncingWifi = true;
            if (TxtWifiNameChung != null) TxtWifiNameChung.Text = TxtWifiName.Text;
            _isSyncingWifi = false;
        }

        private void TxtWifiNameChung_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingWifi) return;
            _isSyncingWifi = true;
            if (TxtWifiName != null) TxtWifiName.Text = TxtWifiNameChung.Text;
            _isSyncingWifi = false;
        }

        private void TxtWifiPass_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingWifi) return;
            _isSyncingWifi = true;
            if (TxtWifiPassChung != null) TxtWifiPassChung.Text = TxtWifiPass.Text;
            _isSyncingWifi = false;
        }

        private void TxtWifiPassChung_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingWifi) return;
            _isSyncingWifi = true;
            if (TxtWifiPass != null) TxtWifiPass.Text = TxtWifiPassChung.Text;
            _isSyncingWifi = false;
        }

        private void BtnConfigSoPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string loaiPhieu)
            {
                TextBox targetTextBox = loaiPhieu switch
                {
                    "Báo giá" => TxtFormatBaoGia,
                    "Hóa đơn nhà hàng" => TxtFormatHoaDon,
                    "Phiếu nhập kho" => TxtFormatNhapKho,
                    "Phiếu xuất kho" => TxtFormatXuatKho,
                    "Phiếu chuyển kho" => TxtFormatChuyenKho,
                    "Phiếu kiểm kê" => TxtFormatKiemKe,
                    "Đặt hàng" => TxtFormatDatHang,
                    "Phiếu thu" => TxtFormatPhieuThu,
                    "Phiếu chi" => TxtFormatPhieuChi,
                    "Phiếu thu công nợ" => TxtFormatThuCongNo,
                    "Bảng lương" => TxtFormatBangLuong,
                    _ => null
                };

                if (targetTextBox != null)
                {
                    var dialog = new CauHinhSoPhieuWindow(loaiPhieu, targetTextBox.Text);
                    dialog.Owner = this;
                    if (dialog.ShowDialog() == true)
                    {
                        targetTextBox.Text = dialog.ResultPattern;
                    }
                }
            }
        }

        private void ChucNangMacDinh_Click(object sender, MouseButtonEventArgs e)
        {
            ShowChucNangMacDinhMenu();
        }

        private void BtnChucNangMacDinhDropdown_Click(object sender, RoutedEventArgs e)
        {
            ShowChucNangMacDinhMenu();
        }

        private void ShowChucNangMacDinhMenu()
        {
            var cm = new ContextMenu
            {
                PlacementTarget = TxtChucNangMacDinh,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
            };

            void AddMenuItem(ItemCollection parentItems, string header)
            {
                var mi = new MenuItem { Header = header };
                mi.Click += (s, e) =>
                {
                    TxtChucNangMacDinh.Text = header;
                };
                parentItems.Add(mi);
            }

            MenuItem CreateGroup(string groupHeader)
            {
                var mi = new MenuItem { Header = groupHeader };
                cm.Items.Add(mi);
                return mi;
            }

            // 1. HOẠT ĐỘNG
            var mHoatDong = CreateGroup("HOẠT ĐỘNG");
            AddMenuItem(mHoatDong.Items, "Dịch vụ MTB");
            AddMenuItem(mHoatDong.Items, "Danh mục bảng giá");
            AddMenuItem(mHoatDong.Items, "Danh mục cửa hàng");
            AddMenuItem(mHoatDong.Items, "Danh mục mặt hàng");
            AddMenuItem(mHoatDong.Items, "Danh mục loại phòng");
            AddMenuItem(mHoatDong.Items, "Danh mục bàn khu vực");
            AddMenuItem(mHoatDong.Items, "Danh mục mật khẩu wifi");
            mHoatDong.Items.Add(new Separator());
            AddMenuItem(mHoatDong.Items, "Thông báo sửa chữa");
            AddMenuItem(mHoatDong.Items, "Khách đặt hàng");
            AddMenuItem(mHoatDong.Items, "Theo dõi đặt phòng");
            mHoatDong.Items.Add(new Separator());
            AddMenuItem(mHoatDong.Items, "Sử dụng dịch vụ");
            AddMenuItem(mHoatDong.Items, "Điều chỉnh hóa đơn");
            AddMenuItem(mHoatDong.Items, "Quản lý bán hàng");
            mHoatDong.Items.Add(new Separator());
            AddMenuItem(mHoatDong.Items, "Lưu vết hoạt động");
            AddMenuItem(mHoatDong.Items, "Kiểm soát order");
            mHoatDong.Items.Add(new Separator());
            AddMenuItem(mHoatDong.Items, "Thống kê doanh thu");
            AddMenuItem(mHoatDong.Items, "Thống kê mặt hàng bán");
            AddMenuItem(mHoatDong.Items, "Tổng hợp kết quả kinh doanh");
            AddMenuItem(mHoatDong.Items, "Chi tiết hoạt động ngày");
            AddMenuItem(mHoatDong.Items, "Danh mục hóa đơn hủy");

            // 2. KHÁCH HÀNG
            var mKhachHang = CreateGroup("KHÁCH HÀNG");
            AddMenuItem(mKhachHang.Items, "Danh mục khách hàng");
            AddMenuItem(mKhachHang.Items, "Gửi tin nhắn tới khách hàng");
            AddMenuItem(mKhachHang.Items, "Danh mục đợt khuyến mại");
            AddMenuItem(mKhachHang.Items, "Khách hàng thân thiết");
            AddMenuItem(mKhachHang.Items, "Danh mục voucher");
            AddMenuItem(mKhachHang.Items, "Danh mục thẻ trả trước");

            // 3. KHO HÀNG
            var mKhoHang = CreateGroup("KHO HÀNG");
            AddMenuItem(mKhoHang.Items, "Danh mục kho hàng");
            AddMenuItem(mKhoHang.Items, "Danh mục nhà cung cấp");
            mKhoHang.Items.Add(new Separator());
            AddMenuItem(mKhoHang.Items, "Quản lý nhập kho");
            AddMenuItem(mKhoHang.Items, "Quản lý xuất kho");
            AddMenuItem(mKhoHang.Items, "Quản lý chuyển kho");
            AddMenuItem(mKhoHang.Items, "Quản lý kiểm kê");
            mKhoHang.Items.Add(new Separator());
            AddMenuItem(mKhoHang.Items, "Tính lại giá vốn");
            AddMenuItem(mKhoHang.Items, "Xuất lại định lượng");
            mKhoHang.Items.Add(new Separator());
            AddMenuItem(mKhoHang.Items, "Tồn kho");
            AddMenuItem(mKhoHang.Items, "Tồn nhiều kho");

            // 4. CÔNG NỢ
            var mCongNo = CreateGroup("CÔNG NỢ");
            AddMenuItem(mCongNo.Items, "Công nợ khách hàng");
            AddMenuItem(mCongNo.Items, "Công nợ nhà cung cấp");
            mCongNo.Items.Add(new Separator());
            AddMenuItem(mCongNo.Items, "Công nợ ban đầu khách hàng");
            AddMenuItem(mCongNo.Items, "Công nợ ban đầu nhà cung cấp");

            // 5. QUỸ
            var mQuy = CreateGroup("QUỸ");
            AddMenuItem(mQuy.Items, "Tạo phiếu thu");
            AddMenuItem(mQuy.Items, "Tạo phiếu chi");
            mQuy.Items.Add(new Separator());
            AddMenuItem(mQuy.Items, "Danh mục phiếu thu");
            AddMenuItem(mQuy.Items, "Danh mục phiếu chi");
            mQuy.Items.Add(new Separator());
            AddMenuItem(mQuy.Items, "Danh mục lý do thu chi");
            AddMenuItem(mQuy.Items, "Danh mục tài khoản ngân hàng");
            mQuy.Items.Add(new Separator());
            AddMenuItem(mQuy.Items, "Sổ quỹ tiền mặt");
            AddMenuItem(mQuy.Items, "Tồn quỹ");

            // 6. NHÂN SỰ
            var mNhanSu = CreateGroup("NHÂN SỰ");
            AddMenuItem(mNhanSu.Items, "Danh mục nhân viên");
            AddMenuItem(mNhanSu.Items, "Danh mục ca làm việc");
            mNhanSu.Items.Add(new Separator());
            AddMenuItem(mNhanSu.Items, "Chấm công");
            AddMenuItem(mNhanSu.Items, "Tạm ứng lương");
            AddMenuItem(mNhanSu.Items, "Thưởng phạt");
            AddMenuItem(mNhanSu.Items, "Bảng lương");

            // 7. QUẢN TRỊ
            var mQuanTri = CreateGroup("QUẢN TRỊ");
            AddMenuItem(mQuanTri.Items, "Cấu hình toàn hệ thống");
            AddMenuItem(mQuanTri.Items, "Thiết kế giao diện màn cảm ứng");
            AddMenuItem(mQuanTri.Items, "Người dùng và phân quyền");
            AddMenuItem(mQuanTri.Items, "Nhật ký truy cập");
            AddMenuItem(mQuanTri.Items, "Sao lưu dữ liệu");
            AddMenuItem(mQuanTri.Items, "Khôi phục dữ liệu");

            cm.IsOpen = true;
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Developer Password & Report Template Tree (SREPORT, SREPORTROLE, SREPORTTEMPLATE)
        private ObservableCollection<DevReportCategoryNode> _devCategoryNodes = new ObservableCollection<DevReportCategoryNode>();
        private ObservableCollection<DevReportTemplateNode> _devTemplateNodes = new ObservableCollection<DevReportTemplateNode>();
        private DevReportCategoryNode? _selectedDevCategory;

        private async void BtnUnlockDev_Click(object sender, RoutedEventArgs e)
        {
            await UnlockDevModeAsync();
        }

        private async void TxtDevPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await UnlockDevModeAsync();
            }
        }

        private async System.Threading.Tasks.Task UnlockDevModeAsync()
        {
            string pass = TxtDevPassword.Password.Trim();
            if (string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu nhà phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDevPassword.Focus();
                return;
            }

            PnlMauHoaDonDevContainer.Visibility = Visibility.Visible;
            if (BtnSuaMauFastReport != null)
            {
                BtnSuaMauFastReport.Visibility = Visibility.Visible;
            }

            await LoadDevReportTreeAsync();
        }

        private static readonly List<string> InvoiceCategoryOrder = new List<string>
        {
            "Mẫu cơ bản",
            "Chuyển bàn",
            "Phiếu xuất kho",
            "Thống kê",
            "Phiếu nhập kho",
            "Hóa đơn bán hàng",
            "Thống kê mặt hàng bán",
            "Phiếu thu",
            "Phiếu kiểm kê",
            "Phiếu chi",
            "In chế biến",
            "Phiếu chuyển kho",
            "Thống kê doanh thu",
            "Đặt hàng",
            "Mặt hàng",
            "Báo giá",
            "Bảng lương"
        };

        private async System.Threading.Tasks.Task LoadDevReportTreeAsync()
        {
            try
            {
                _devCategoryNodes.Clear();

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // Nạp tất cả danh mục từ bảng CSDL SREPORT
                var dbReports = (await conn.QueryAsync<dynamic>(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME, PARENTID, COALESCE(ITEMTYPE, 0) AS ITEMTYPE FROM SREPORT WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME"
                )).ToList();

                // Map tên SREPORT trong CSDL để lấy ID thực tế
                var dbReportMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var rep in dbReports)
                {
                    string rName = ((string)rep.NAME)?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(rName) && !dbReportMap.ContainsKey(rName))
                    {
                        dbReportMap[rName] = (string)rep.ID;
                    }
                }

                var addedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 1. Luôn hiển thị 17 danh mục Mẫu Hóa Đơn theo đúng thứ tự chuẩn trong hình
                foreach (var catName in InvoiceCategoryOrder)
                {
                    string id = catName.Equals("Mẫu cơ bản", StringComparison.OrdinalIgnoreCase) ? "STEMPLATE_BASE" : "";
                    if (string.IsNullOrEmpty(id) && dbReportMap.TryGetValue(catName, out string? foundId))
                    {
                        id = foundId;
                    }
                    if (string.IsNullOrEmpty(id))
                    {
                        // Fuzzy search SREPORT by name
                        var matchRep = dbReports.FirstOrDefault(r => 
                            r.NAME != null && 
                            (((string)r.NAME).Trim().Equals(catName, StringComparison.OrdinalIgnoreCase) ||
                             ((string)r.NAME).Trim().ToUpper().Contains(catName.ToUpper()) ||
                             catName.ToUpper().Contains(((string)r.NAME).Trim().ToUpper())));

                        if (matchRep != null)
                        {
                            id = (string)matchRep.ID;
                        }
                        else
                        {
                            id = "SREPORT_" + catName;
                        }
                    }

                    _devCategoryNodes.Add(new DevReportCategoryNode
                    {
                        Id = id,
                        Name = catName,
                        Icon = "📁"
                    });
                    addedNames.Add(catName);
                }

                // 2. Thêm các mẫu hóa đơn/phiếu in khác từ CSDL (loại trừ các báo cáo tổng hợp hệ thống bắt đầu bằng "BÁO CÁO", "TỔNG HỢP", "ĐỐI CHIẾU")
                foreach (var rep in dbReports)
                {
                    string rName = ((string)rep.NAME)?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(rName) || addedNames.Contains(rName)) continue;

                    string upperName = rName.ToUpper();
                    if (upperName.StartsWith("BÁO CÁO") || upperName.StartsWith("TỔNG HỢP") || upperName.StartsWith("ĐỐI CHIẾU"))
                    {
                        continue; // Bỏ qua tất cả các báo cáo hệ thống trong phần Mẫu hóa đơn
                    }

                    _devCategoryNodes.Add(new DevReportCategoryNode
                    {
                        Id = (string)rep.ID,
                        Name = rName,
                        Icon = "📁"
                    });
                    addedNames.Add(rName);
                }

                TvReportCategories.ItemsSource = _devCategoryNodes;

                if (_devCategoryNodes.Count > 0)
                {
                    _selectedDevCategory = _devCategoryNodes[0];
                    await LoadDevTemplatesForCategoryAsync(_devCategoryNodes[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadDevReportTreeAsync error: " + ex.Message);
            }
        }

        private async void TvReportCategories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is DevReportCategoryNode cat)
            {
                _selectedDevCategory = cat;
                await LoadDevTemplatesForCategoryAsync(cat);
            }
        }

        private static readonly Dictionary<string, string> CategoryTableCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bảng lương"] = "TBANGLUONG",
            ["Báo giá"] = "TBAOGIA",
            ["Đặt hàng"] = "TDATHANG",
            ["Mặt hàng"] = "DMATHANG",
            ["Hóa đơn bán hàng"] = "TSOHOADON",
            ["Phiếu nhập kho"] = "TPHIEUNHAPKHO",
            ["Phiếu xuất kho"] = "TPHIEUXUATKHO",
            ["Phiếu chuyển kho"] = "TPHIEUCHUYENKHO",
            ["Phiếu kiểm kê"] = "TPHIEUKIEMKE",
            ["Phiếu thu"] = "TPHIEUTHU",
            ["Phiếu chi"] = "TPHIEUCHI",
            ["In chế biến"] = "TSOINCHEBIEN"
        };

        private static string DetermineTemplateIcon(string templateName, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(templateName)) return "📄";
            string catLower = (categoryName ?? "").Trim().ToLower();
            string nLower = templateName.Trim().ToLower();

            if (catLower == "bảng lương" || nLower.StartsWith("bảng lương")) return "🔮";

            if (nLower.Contains("cộng gộp")) return "📷";
            if (nLower.Contains("2 ngôn ngữ") || nLower.Contains("ck tổng") || nLower.EndsWith("a5") || nLower.Contains(" bill a5")) return "🟢";
            if (nLower.Equals("mẫu in bill 80mm") || nLower.Equals("bill 80mm") || nLower.Equals("54mm") || (nLower.Equals("a4") && !nLower.Contains("mẫu in bill"))) return "🌸";
            if (nLower.Contains("80mm") && !nLower.Contains("in bill") && !nLower.Contains("2 ngôn ngữ") && !nLower.Contains("cộng gộp")) return "⭐️";
            if (nLower.Contains("54mm x 2") || nLower.Contains("54 mm x 2") || nLower.Contains("80 (có ck)") || nLower.Contains("bill a4") || nLower.Contains("bill 54mm")) return "📄";

            if (nLower.Contains("54") || nLower.Contains("58")) return "📊";
            if (nLower.Contains("80")) return "⭐️";

            return "📄";
        }

        private async System.Threading.Tasks.Task LoadDevTemplatesForCategoryAsync(DevReportCategoryNode cat)
        {
            try
            {
                _devTemplateNodes.Clear();

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                List<DevReportTemplateNode> list = new List<DevReportTemplateNode>();
                string catName = cat.Name.Trim();
                string catClean = catName.ToUpperInvariant();

                // Query all STEMPLATE records with SFORM and STABLEDESC joins
                var allStemplates = (await conn.QueryAsync<dynamic>(
                    @"SELECT CAST(st.ID AS VARCHAR(50)) AS ID, 
                             st.NAME, 
                             st.STABLEDESCID, 
                             sd.NAME AS TABLENAME, 
                             st.SFORMID,
                             sf.NAME AS FORMNAME,
                             st.REPORTBASE
                      FROM STEMPLATE st
                      LEFT JOIN STABLEDESC sd ON st.STABLEDESCID = sd.ID
                      LEFT JOIN SFORM sf ON st.SFORMID = sf.ID
                      WHERE (st.STATUS IS NULL OR st.STATUS <> 0)
                      ORDER BY st.SORTORDER, st.NAME"
                )).ToList();

                // Query all SREPORTTEMPLATE records
                var allRepTemplates = (await conn.QueryAsync<dynamic>(
                    @"SELECT CAST(t.ID AS VARCHAR(50)) AS ID, 
                             t.NAME, 
                             r.NAME AS REPORTNAME, 
                             CAST(t.SREPORTID AS VARCHAR(50)) AS SREPORTID, 
                             CAST(t.STEMPLATEID AS VARCHAR(50)) AS STEMPLATEID
                      FROM SREPORTTEMPLATE t
                      LEFT JOIN SREPORT r ON t.SREPORTID = r.ID
                      WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                      ORDER BY t.SORTORDER, t.NAME"
                )).ToList();

                // 1. Direct match STEMPLATE by SFORMID (Form Name) OR STABLEDESCID (Table Name)
                var matchedStemplates = allStemplates.Where(st => {
                    string fName = ((string)(st.FORMNAME ?? "")).Trim().ToUpperInvariant();
                    string tName = ((string)(st.TABLENAME ?? "")).Trim().ToUpperInvariant();

                    if (catClean == "MẪU CƠ BẢN")
                    {
                        return (st.STABLEDESCID == null || string.IsNullOrWhiteSpace((string)(st.STABLEDESCID ?? ""))) &&
                               (st.SFORMID == null || string.IsNullOrWhiteSpace((string)(st.SFORMID ?? "")));
                    }

                    if (fName.Length > 0 && fName == catClean) return true;

                    string targetTableCode = catClean switch
                    {
                        "BẢNG LƯƠNG" => "TBANGLUONG",
                        "BÁO GIÁ" => "TBAOGIA",
                        "ĐẶT HÀNG" => "TDATHANG",
                        "MẶT HÀNG" => "DMATHANG",
                        "HÓA ĐƠN BÁN HÀNG" => "TSOHOADON",
                        "PHIẾU NHẬP KHO" => "TPHIEUNHAPKHO",
                        "PHIẾU XUẤT KHO" => "TPHIEUXUATKHO",
                        "PHIẾU CHUYỂN KHO" => "TPHIEUCHUYENKHO",
                        "PHIẾU THU" => "TPHIEUTHU",
                        "PHIẾU CHI" => "TPHIEUCHI",
                        "PHIẾU KIỂM KÊ" => "TPHIEUKIEMKE",
                        "IN CHẾ BIẾN" => "TSOINCHEBIEN",
                        _ => ""
                    };

                    return targetTableCode.Length > 0 && tName == targetTableCode;
                }).ToList();

                foreach (var st in matchedStemplates)
                {
                    string nameStr = ((string)st.NAME)?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(nameStr) && !list.Any(x => x.Name.Equals(nameStr, StringComparison.OrdinalIgnoreCase)))
                    {
                        string icon = DetermineTemplateIcon(nameStr, catName);
                        list.Add(new DevReportTemplateNode
                        {
                            Id = (string)st.ID,
                            Name = nameStr,
                            Icon = icon,
                            StemplateId = (string)st.ID,
                            SreportId = cat.Id
                        });
                    }
                }

                // 2. ONLY Query SREPORTTEMPLATE if no STEMPLATE match was found (for custom SREPORT nodes)
                if (list.Count == 0)
                {
                    var repStems = allRepTemplates.Where(rt => {
                        string sRepId = (rt.SREPORTID ?? "").ToString().Trim();
                        string rName = ((string)(rt.REPORTNAME ?? "")).Trim().ToUpperInvariant();
                        return (sRepId.Length > 0 && sRepId == cat.Id) || rName == catClean;
                    }).ToList();

                    foreach (var rt in repStems)
                    {
                        string nameStr = ((string)rt.NAME)?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(nameStr) && !list.Any(x => x.Name.Equals(nameStr, StringComparison.OrdinalIgnoreCase)))
                        {
                            string icon = DetermineTemplateIcon(nameStr, catName);
                            list.Add(new DevReportTemplateNode
                            {
                                Id = (string)rt.ID,
                                Name = nameStr,
                                Icon = icon,
                                StemplateId = rt.STEMPLATEID?.ToString() ?? "",
                                SreportId = cat.Id
                            });
                        }
                    }
                }

                // 3. Fallbacks for standard categories if still no templates found
                if (list.Count == 0)
                {
                    if (catClean == "CHUYỂN BÀN" || catClean == "IN CHẾ BIẾN")
                    {
                        list.Add(new DevReportTemplateNode { Id = "DEF_58", Name = "Mẫu 58mm", Icon = "📊", SreportId = cat.Id });
                        list.Add(new DevReportTemplateNode { Id = "DEF_80", Name = "Mẫu 80mm", Icon = "⭐️", SreportId = cat.Id });
                    }
                    else if (catClean == "THỐNG KÊ")
                    {
                        list.Add(new DevReportTemplateNode { Id = "TK_58", Name = "Báo cáo kết ca 58mm", Icon = "📄", SreportId = cat.Id });
                        list.Add(new DevReportTemplateNode { Id = "TK_80", Name = "Báo cáo kết ca 80mm", Icon = "⭐️", SreportId = cat.Id });
                        list.Add(new DevReportTemplateNode { Id = "TK_A4", Name = "Báo cáo kết ca A4", Icon = "📊", SreportId = cat.Id });
                        list.Add(new DevReportTemplateNode { Id = "TK_A5", Name = "Báo cáo kết ca A5", Icon = "📊", SreportId = cat.Id });
                    }
                    else
                    {
                        list.Add(new DevReportTemplateNode { Id = cat.Id + "_54MM", Name = "54mm", Icon = "🌸", SreportId = cat.Id });
                        list.Add(new DevReportTemplateNode { Id = cat.Id + "_80MM", Name = "80mm", Icon = "⭐️", SreportId = cat.Id });
                        list.Add(new DevReportTemplateNode { Id = cat.Id + "_A4", Name = "A4", Icon = "🌸", SreportId = cat.Id });
                    }
                }

                var distinctList = list.GroupBy(x => x.Name.Trim().ToUpperInvariant()).Select(g => g.First()).ToList();

                foreach (var item in distinctList)
                {
                    _devTemplateNodes.Add(item);
                }

                TvReportTemplates.ItemsSource = _devTemplateNodes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadDevTemplatesForCategoryAsync error: " + ex.Message);
            }
        }

        private void BtnSuaMauFastReport_Click(object sender, RoutedEventArgs e)
        {
            OpenFastReportDesigner();
        }

        private void TvReportTemplates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFastReportDesigner();
        }

        private void OpenFastReportDesigner()
        {
            string reportName = "";
            string stemplateId = "";

            if (TvReportTemplates.SelectedItem is DevReportTemplateNode templateNode && !string.IsNullOrWhiteSpace(templateNode.Name))
            {
                reportName = templateNode.Name;
                stemplateId = templateNode.StemplateId;
            }
            else if (_selectedDevCategory != null && !string.IsNullOrWhiteSpace(_selectedDevCategory.Name))
            {
                reportName = _selectedDevCategory.Name;
            }

            if (string.IsNullOrWhiteSpace(reportName))
            {
                reportName = "Mẫu A4 nằm ngang";
            }

            try
            {
                var designerCtrl = new QuanLyBar.Client.Views.InAn.FastReportDesignerControl(reportName, stemplateId);

                if (Application.Current.MainWindow is MainAppWindow mainWin)
                {
                    mainWin.AddTab(reportName, designerCtrl);
                    this.Close();
                }
                else
                {
                    var win = new Window
                    {
                        Title = $"FastReport Designer - {reportName}",
                        Content = designerCtrl,
                        Width = 1100,
                        Height = 720,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    win.Owner = this;
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở thiết kế mẫu FastReport: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }

    public class InvoiceTemplateDisplayItem
    {
        public string Name { get; set; } = "";
        public string PaperSize { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "Đang sử dụng";
    }

    public class DevReportCategoryNode : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "📁";
        public ObservableCollection<DevReportCategoryNode> Children { get; set; } = new ObservableCollection<DevReportCategoryNode>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class DevReportTemplateNode : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "📊";
        public string StemplateId { get; set; } = "";
        public string SreportId { get; set; } = "";
        public ObservableCollection<DevReportTemplateNode> Children { get; set; } = new ObservableCollection<DevReportTemplateNode>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
