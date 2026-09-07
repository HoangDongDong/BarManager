using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

            CboKichThuocHienThiOGiaoDienDichVu.ItemsSource = new string[] { "Lớn", "Trung bình", "Vừa", "Nhỏ" };
            CboKichThuocHienThiOGiaoDienDichVu.SelectedIndex = 0;

            // Tích điểm
            CboCachTinhDiem.ItemsSource = new string[] { "Điểm được tính trên từng hóa đơn", "Theo tổng tiền thanh toán", "Theo tiền hàng sau chiết khấu" };
            CboCachTinhDiem.SelectedIndex = 0;

            // Mặt hàng
            CboTimTheo.ItemsSource = new string[] { "Cả tên hàng và mã hàng", "Tên mặt hàng", "Mã mặt hàng" };
            CboTimTheo.SelectedIndex = 0;

            CboCachTim.ItemsSource = new string[] { "Chứa cụm từ tìm kiếm", "Bắt đầu bằng cụm từ" };
            CboCachTim.SelectedIndex = 0;

            CboSapXepThuTuTheo.ItemsSource = new string[] { "Mã hàng", "Tên mặt hàng", "Nhóm mặt hàng", "Thứ tự sắp xếp" };
            CboSapXepThuTuTheo.SelectedIndex = 0;

            CboCachLamTron.ItemsSource = new string[] { "Làm tròn xuống", "Làm tròn lên", "Làm tròn chuẩn" };
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
            CboPhimNhapLieuTrenLuoi.ItemsSource = new string[] { "Tab để sang ngang, Enter để xuống dòng", "Enter để sang ô tiếp theo", "Phím mũi tên" };
            CboPhimNhapLieuTrenLuoi.SelectedIndex = 0;

            CboChucNangMacDinh.ItemsSource = new string[] { "Sử dụng dịch vụ", "Bán hàng", "Danh mục bàn", "Phiếu thu chi" };
            CboChucNangMacDinh.SelectedIndex = 0;

            CboDoToChuHienThi.ItemsSource = new string[] { "Nhỏ (100%)", "Vừa (110%)", "Lớn (125%)", "Rất lớn (150%)" };
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
                ChkToiUuDungBanPhim.IsChecked = GetBool(configs, "ToiUuDungBanPhim", false);
                ChkSuDungMatHangMacDinh.IsChecked = GetBool(configs, "SuDungMatHangMacDinh", true);
                ChkHienThiGhiChuTrenGiaoDienBan.IsChecked = GetBool(configs, "HienThiGhiChuTrenGiaoDienBan", false);
                ChkMatHangGiaTheoGioTheoKhuVuc.IsChecked = GetBool(configs, "SuDungGiaTheoGio", false);
                SetComboValue(CboCachChonGioTinhGia, GetStr(configs, "CachChonGioTinhGia", "Giờ gọi đồ"));
                SetComboValue(CboCachChonKhachHang, GetStr(configs, "CachChonKhachHang", "Chọn bằng chuột và bàn phím"));
                SetComboValue(CboCachChonNgayGiaoDich, GetStr(configs, "CachChonNgayGiaoDich", "Theo ngày đóng hóa đơn"));
                TxtTruocGioTinhVaoNgayHomTruoc.Text = GetStr(configs, "TruocGioTinhVaoNgayHomTruoc", "0");
                SetComboValue(CboKichThuocHienThiOGiaoDienDichVu, GetStr(configs, "KichThuocHienThiOGiaoDienDichVu", "Lớn"));

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
                SetComboValue(CboChucNangMacDinh, GetStr(configs, "ChucNangMacDinh", "Sử dụng dịch vụ"));
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

        private void BtnUnlockDev_Click(object sender, RoutedEventArgs e)
        {
            if (TxtDevPassword.Password == "123456" || TxtDevPassword.Password.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                PnlDevUnlocked.Visibility = Visibility.Visible;
                MessageBox.Show("Mở khóa chế độ Nhà phát triển thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Mật khẩu không đúng. Vui lòng thử lại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDevPassword.Focus();
                TxtDevPassword.SelectAll();
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
                    ["ToiUuDungBanPhim"] = (ChkToiUuDungBanPhim.IsChecked == true) ? "1" : "0",
                    ["SuDungMatHangMacDinh"] = (ChkSuDungMatHangMacDinh.IsChecked == true) ? "1" : "0",
                    ["HienThiGhiChuTrenGiaoDienBan"] = (ChkHienThiGhiChuTrenGiaoDienBan.IsChecked == true) ? "1" : "0",
                    ["SuDungGiaTheoGio"] = (ChkMatHangGiaTheoGioTheoKhuVuc.IsChecked == true) ? "1" : "0",
                    ["CachChonGioTinhGia"] = CboCachChonGioTinhGia.Text.Trim(),
                    ["CachChonKhachHang"] = CboCachChonKhachHang.Text.Trim(),
                    ["CachChonNgayGiaoDich"] = CboCachChonNgayGiaoDich.Text.Trim(),
                    ["TruocGioTinhVaoNgayHomTruoc"] = TxtTruocGioTinhVaoNgayHomTruoc.Text.Trim(),
                    ["KichThuocHienThiOGiaoDienDichVu"] = CboKichThuocHienThiOGiaoDienDichVu.Text.Trim(),

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
                    ["ChucNangMacDinh"] = CboChucNangMacDinh.Text.Trim(),
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

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
