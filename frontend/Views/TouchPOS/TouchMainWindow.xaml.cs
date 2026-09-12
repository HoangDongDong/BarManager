using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchMainWindow : Window
    {
        private DBAN? _currentBan;

        public TouchMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLoginScreen();
        }

        private void ShowLoginScreen()
        {
            GridTouchLogin.Visibility = Visibility.Visible;
            GridTouchMenu.Visibility = Visibility.Collapsed;
            GridTouchTables.Visibility = Visibility.Collapsed;
            GridTouchOrder.Visibility = Visibility.Collapsed;
        }

        private void ShowMenuScreen()
        {
            GridTouchLogin.Visibility = Visibility.Collapsed;
            GridTouchMenu.Visibility = Visibility.Visible;
            GridTouchTables.Visibility = Visibility.Collapsed;
            GridTouchOrder.Visibility = Visibility.Collapsed;

            try
            {
                TxtMenuHeaderTitle.Text = "PHẦN MỀM QUẢN LÝ BAR, NHÀ HÀNG V6.0 TÂN AN PHÁT - HOTLINE: 0967041111";
                TxtFooterUser.Text = SessionContext.CurrentUser?.TenHienThi ?? "Administrator";
                TxtFooterDb.Text = "DEMO";
                SelectMenuCategory(null);
            }
            catch { }
        }

        private void ShowTableScreen()
        {
            GridTouchLogin.Visibility = Visibility.Collapsed;
            GridTouchMenu.Visibility = Visibility.Collapsed;
            GridTouchTables.Visibility = Visibility.Visible;
            GridTouchOrder.Visibility = Visibility.Collapsed;
            LoadTables();
        }

        private void ShowOrderScreen(DBAN ban)
        {
            _currentBan = ban;
            GridTouchLogin.Visibility = Visibility.Collapsed;
            GridTouchMenu.Visibility = Visibility.Collapsed;
            GridTouchTables.Visibility = Visibility.Collapsed;
            GridTouchOrder.Visibility = Visibility.Visible;

            TxtOrderTableName.Text = ban.TENBAN;
            TxtOrderDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            TxtOrderNo.Text = $"HĐ: ...";
            LoadOrderItems();
            LoadMenuItems();
        }

        private List<DBAN> _allBanList = new();
        private List<DKHUVUC> _allKhuVucList = new();
        private string? _selectedAreaId = null;
        private bool _filterOnlyOpened = false;

        private async void LoadTables()
        {
            try
            {
                TxtCurrentPOSUser.Text = SessionContext.CurrentUser?.TenHienThi ?? "Administrator";

                var service = new LocalSuDungDichVuService();
                var kvBanList = await service.GetKhuVucBanListAsync();

                _allKhuVucList = new List<DKHUVUC>();
                _allBanList = new List<DBAN>();

                string[] presetColors = new string[] { "#E65100", "#414BEA", "#00C800", "#00838F", "#C2185B", "#6A1B9A", "#D84315" };
                int colorIndex = 0;

                foreach (var kv in kvBanList)
                {
                    _allKhuVucList.Add(new DKHUVUC
                    {
                        Id = kv.Id,
                        MAKHUVUC = kv.Id,
                        Name = kv.Name,
                        TenKhuVuc = kv.Name,
                        ColorHex = presetColors[colorIndex % presetColors.Length]
                    });
                    colorIndex++;

                    foreach (var b in kv.BanList)
                    {
                        _allBanList.Add(new DBAN
                        {
                            Id = b.Id,
                            MABAN = b.Id,
                            Name = b.Name,
                            TENBAN = b.Name,
                            MAKHUVUC = kv.Id,
                            IsOpened = b.IsOccupied
                        });
                    }
                }

                IcAreaButtons.ItemsSource = _allKhuVucList;
                ApplyTableFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải bàn: {ex.Message}");
            }
        }

        private void ApplyTableFilters()
        {
            var query = _allBanList.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedAreaId))
            {
                query = query.Where(x => x.MAKHUVUC == _selectedAreaId);
            }

            if (_filterOnlyOpened)
            {
                query = query.Where(x => x.IsOpened);
            }

            IcTableTiles.ItemsSource = query.ToList();
        }

        private void BtnAreaSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DKHUVUC kv)
            {
                if (_selectedAreaId == kv.MAKHUVUC)
                    _selectedAreaId = null;
                else
                    _selectedAreaId = kv.MAKHUVUC;

                ApplyTableFilters();
            }
        }

        private void BtnFilterDangMo_Click(object sender, RoutedEventArgs e)
        {
            _filterOnlyOpened = !_filterOnlyOpened;
            ApplyTableFilters();
        }

        private void TableTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is DBAN ban)
            {
                ShowOrderScreen(ban);
            }
        }

        private void LoadOrderItems()
        {
            try
            {
                if (_currentBan == null) return;
                var items = LocalDatabaseService.GetAll<TDONHANGCHITIET>(
                    "SELECT * FROM TDONHANGCHITIET WHERE STATUS = 1",
                    new { MaBan = _currentBan.MABAN });
                DgOrderItems.ItemsSource = items;
                UpdateTotals();
            }
            catch { }
        }

        private void LoadMenuItems()
        {
            try
            {
                var nhomList = LocalDatabaseService.GetAll<DNHOMMATHANG>("SELECT T.ID, T.NAME, T.STATUS, T.NAME AS TenNhom FROM DNHOMMATHANG T WHERE T.STATUS = 1 ORDER BY T.SORTORDER");
                IcTouchNhomHang.ItemsSource = nhomList;

                var matHangList = LocalDatabaseService.GetAll<DMATHANG>("SELECT FIRST 100 T.ID, T.NAME, T.STATUS, T.ANH, T.NAME AS TenMatHang FROM DMATHANG T WHERE T.STATUS = 1 ORDER BY T.NAME");
                IcTouchMatHang.ItemsSource = matHangList;
            }
            catch { }
        }

        private void UpdateTotals()
        {
            decimal tamTinh = 0, giamGia = 0;
            if (DgOrderItems.Items != null)
            {
                foreach (TDONHANGCHITIET item in DgOrderItems.Items)
                {
                    tamTinh += item.THANHTIEN;
                }
            }
            TxtTotalSub.Text = tamTinh.ToString("N0") + "đ";
            TxtTotalDiscount.Text = giamGia.ToString("N0") + "đ";
            TxtTotalFinal.Text = (tamTinh - giamGia).ToString("N0") + "đ";
        }

        private async void DoLogin(string userName, string password)
        {
            try
            {
                var user = await LocalAuthService.LoginAsync(userName, password);
                if (user != null)
                {
                    SessionContext.CurrentUser = user;
                    ShowMenuScreen();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Đăng nhập thất bại",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== LOGIN EVENTS =====
        private void BtnPowerOff_Click(object sender, RoutedEventArgs e)
        {
            if (QuanLyBar.Views.TouchPOS.TouchConfirmWindow.Show(this, "BẠN CÓ MUỐN THOÁT KHỎI HỆ THỐNG KHÔNG?"))
                Application.Current.Shutdown();
        }

        private void BtnTouchKbUser_Click(object sender, RoutedEventArgs e)
        {
            TxtLoginUser.Focus();
            OpenOSK();
        }

        private void BtnTouchKbPass_Click(object sender, RoutedEventArgs e)
        {
            TxtLoginPass.Focus();
            OpenOSK();
        }

        private void OpenOSK()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osk.exe",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void TxtLoginInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                DoLogin(TxtLoginUser.Text.Trim(), TxtLoginPass.Password);
            }
        }

        private void BtnSubmitLogin_Click(object sender, RoutedEventArgs e)
        {
            DoLogin(TxtLoginUser.Text.Trim(), TxtLoginPass.Password);
        }

        private void BtnResetLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtLoginUser.Text = "";
            TxtLoginPass.Password = "";
        }

        // ===== MENU EVENTS =====
        private string? _currentSelectedCategory = null;

        private void BtnMenuCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string categoryTag)
            {
                if (_currentSelectedCategory == categoryTag)
                {
                    SelectMenuCategory(null);
                }
                else
                {
                    SelectMenuCategory(categoryTag);
                }
            }
        }

        private void SelectMenuCategory(string? categoryTag)
        {
            _currentSelectedCategory = categoryTag;

            // Reset styles
            var tealBrush = (Brush)new BrushConverter().ConvertFromString("#009688")!;
            var orangeBrush = (Brush)new BrushConverter().ConvertFromString("#E65100")!;

            BtnMenuHeThong.Background = categoryTag == "HeThong" ? orangeBrush : tealBrush;
            BtnMenuQuanTri.Background = categoryTag == "QuanTri" ? orangeBrush : tealBrush;
            BtnMenuBaoCao.Background = categoryTag == "BaoCao" ? orangeBrush : tealBrush;
            BtnMenuTroGiup.Background = categoryTag == "TroGiup" ? orangeBrush : tealBrush;

            // Show center panel corresponding to category
            PanelCenterHeThong.Visibility = categoryTag == "HeThong" ? Visibility.Visible : Visibility.Collapsed;
            PanelCenterCoSoDuLieu.Visibility = Visibility.Collapsed;
            PanelCenterQuanTri.Visibility = categoryTag == "QuanTri" ? Visibility.Visible : Visibility.Collapsed;
            PanelCenterBaoCao.Visibility = categoryTag == "BaoCao" ? Visibility.Visible : Visibility.Collapsed;
            HideAllSubReportPanels();
            PanelCenterTroGiup.Visibility = categoryTag == "TroGiup" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HideAllSubReportPanels()
        {
            PanelSubBaoCaoQuy.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoDanhMuc.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoBanHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoDatHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoKhoHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoCongNo.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoQuanTri.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoBieuDo.Visibility = Visibility.Collapsed;

            // Sub-level 3 panels (Kho hàng)
            PanelSubBaoCaoXuatBanHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoKiemKe.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoXuatKhac.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoNhapHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoChuyenKho.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoHSD.Visibility = Visibility.Collapsed;

            // Sub-level 3 panels (Bán hàng)
            PanelSubBaoCaoTheoNhanVienPhucVu.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoKhuVuc.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoThuNgan.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoKhachHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoBanHangKhac.Visibility = Visibility.Collapsed;
        }

        // ===== HỆ THỐNG HANDLERS =====
        private void BtnMenuCoSoDuLieuSub_Click(object sender, RoutedEventArgs e)
        {
            PanelCenterHeThong.Visibility = Visibility.Collapsed;
            PanelCenterCoSoDuLieu.Visibility = Visibility.Visible;
        }

        private void BtnMenuBackHeThong_Click(object sender, RoutedEventArgs e)
        {
            PanelCenterCoSoDuLieu.Visibility = Visibility.Collapsed;
            PanelCenterHeThong.Visibility = Visibility.Visible;
        }
        private void BtnMenuCoSoDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.DataManagerWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi quản lý CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuTaoMoiCsdl_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Database file (*.fdb)|*.fdb|All files (*.*)|*.*",
                    Title = "Tạo mới cơ sở dữ liệu trắng",
                    DefaultExt = ".fdb",
                    FileName = ""
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filename = saveFileDialog.FileName;
                    string dbName = System.IO.Path.GetFileNameWithoutExtension(filename);

                    string templatePath = @"D:\QuanLyBar\frontend\CSDL\TEMPLATE.FDB";
                    if (!System.IO.File.Exists(templatePath))
                    {
                        templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSDL", "TEMPLATE.FDB");
                    }

                    if (System.IO.File.Exists(templatePath))
                    {
                        System.IO.File.Copy(templatePath, filename, true);
                        MessageBox.Show($"Đã tạo mới cơ sở dữ liệu trắng thành công tại:\n{filename}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file CSDL mẫu TEMPLATE.FDB!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo mới CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuSaoLuuCsdl_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Backup file (*.fbk;*.gbk)|*.fbk;*.gbk|All files (*.*)|*.*",
                    Title = "Sao lưu cơ sở dữ liệu",
                    FileName = $"SAOLUU_CSDL_{DateTime.Now:yyyyMMdd_HHmmss}.fbk"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    string currentDbPath = QuanLyBar.Client.Services.DbConnectionManager.CurrentConfig?.Path ?? "";
                    if (string.IsNullOrEmpty(currentDbPath) || !System.IO.File.Exists(currentDbPath))
                    {
                        currentDbPath = @"D:\taifirebird\HIHI.FDB";
                    }

                    if (System.IO.File.Exists(currentDbPath))
                    {
                        System.IO.File.Copy(currentDbPath, saveDlg.FileName, true);
                        MessageBox.Show($"Sao lưu cơ sở dữ liệu thành công!\nFile lưu tại: {saveDlg.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file CSDL hiện tại để sao lưu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sao lưu CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuPhucHoiCsdl_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.Views.KhoiPhucCsdlWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khôi phục CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.NguoiDungPhanQuyen.DoiMatKhauWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Đổi mật khẩu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== QUẢN TRỊ HANDLERS =====
        private void BtnMenuXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Xóa dữ liệu", "View", this)) return;
            MessageBox.Show("Chức năng Xóa dữ liệu hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuCongCuDeveloper_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Công cụ nhà phát triển", "View", this)) return;
            MessageBox.Show("Công cụ nhà phát triển hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuNguoiDungPhanQuyen_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Quản lý người dùng", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.Views.NguoiDungPhanQuyen.NguoiDungPhanQuyenWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Người dùng & Phân quyền: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuCauHinhHeThong_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.Views.CauHinhHeThong.CauHinhToanHeThongWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Cấu hình hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== BÁO CÁO HANDLERS =====
        private void BtnSubReportGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string group)
            {
                PanelCenterBaoCao.Visibility = Visibility.Collapsed;
                HideAllSubReportPanels();

                switch (group)
                {
                    case "BaoCaoQuy": PanelSubBaoCaoQuy.Visibility = Visibility.Visible; break;
                    case "BaoCaoDanhMuc": PanelSubBaoCaoDanhMuc.Visibility = Visibility.Visible; break;
                    case "BaoCaoBanHang": PanelSubBaoCaoBanHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoDatHang": PanelSubBaoCaoDatHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoKhoHang": PanelSubBaoCaoKhoHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoCongNo": PanelSubBaoCaoCongNo.Visibility = Visibility.Visible; break;
                    case "BaoCaoQuanTri": PanelSubBaoCaoQuanTri.Visibility = Visibility.Visible; break;
                    case "BaoCaoBieuDo": PanelSubBaoCaoBieuDo.Visibility = Visibility.Visible; break;

                    // Level 3 (Kho hàng)
                    case "BaoCaoXuatBanHang": PanelSubBaoCaoXuatBanHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoKiemKe": PanelSubBaoCaoKiemKe.Visibility = Visibility.Visible; break;
                    case "BaoCaoXuatKhac": PanelSubBaoCaoXuatKhac.Visibility = Visibility.Visible; break;
                    case "BaoCaoNhapHang": PanelSubBaoCaoNhapHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoChuyenKho": PanelSubBaoCaoChuyenKho.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoHSD": PanelSubBaoCaoTheoHSD.Visibility = Visibility.Visible; break;

                    // Level 3 (Bán hàng)
                    case "BaoCaoTheoNhanVienPhucVu": PanelSubBaoCaoTheoNhanVienPhucVu.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoKhuVuc": PanelSubBaoCaoTheoKhuVuc.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoThuNgan": PanelSubBaoCaoTheoThuNgan.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoKhachHang": PanelSubBaoCaoTheoKhachHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoBanHangKhac": PanelSubBaoCaoBanHangKhac.Visibility = Visibility.Visible; break;
                }
            }
        }

        private void BtnBackBaoCaoMain_Click(object sender, RoutedEventArgs e)
        {
            HideAllSubReportPanels();
            PanelCenterBaoCao.Visibility = Visibility.Visible;
        }

        private void BtnBackBaoCaoBanHang_Click(object sender, RoutedEventArgs e)
        {
            HideAllSubReportPanels();
            PanelSubBaoCaoBanHang.Visibility = Visibility.Visible;
        }

        private void BtnBackBaoCaoKhoHang_Click(object sender, RoutedEventArgs e)
        {
            HideAllSubReportPanels();
            PanelSubBaoCaoKhoHang.Visibility = Visibility.Visible;
        }

        private void BtnOpenReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string reportTitle)
            {
                if (!LocalPhanQuyenService.CheckPermissionAndAlert(reportTitle, "View", this)) return;

                UIElement? content = null;
                switch (reportTitle)
                {
                    case "DANH SÁCH PHIẾU THU THEO NGÀY":
                    case "DANH SÁCH PHIẾU THU THEO LÝ DO THU CHI":
                    case "DANH SÁCH PHIẾU CHI THEO NGÀY":
                    case "DANH SÁCH PHIẾU CHI THEO LÝ DO THU CHI":
                    case "TỔNG HỢP THU CHI THEO NGÀY":
                    case "TỔNG HỢP THU CHI THEO LÝ DO":
                    case "BÁO CÁO TỒN QUỸ":
                        content = new QuanLyBar.Client.Views.BaoCaoQuy.BaoCaoPhieuThuChiControl(reportTitle);
                        break;

                    case "DANH SÁCH KHÁCH HÀNG THEO NHÓM":
                    case "DANH SÁCH KHÁCH HÀNG THEO NHÂN VIÊN":
                    case "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM":
                    case "DANH SÁCH ĐỢT KHUYẾN MẠI":
                    case "DANH SÁCH MẶT HÀNG THEO NHÓM":
                    case "DANH SÁCH MẶT HÀNG THEO HÃNG SẢN XUẤT":
                    case "KHÁCH HÀNG ĐẾN NGÀY SINH NHẬT":
                    case "BÁO CÁO CẤU HÌNH BÀN KHU VỰC":
                    case "CÔNG THỨC ĐỊNH LƯỢNG":
                    case "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG":
                        content = new QuanLyBar.Client.Views.BaoCaoDanhMuc.BaoCaoMatHangControl(reportTitle);
                        break;

                    case "TỔNG HỢP BÁN HÀNG THEO NGÀY":
                    case "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY":
                    case "BÁO CÁO CHI TIẾT BÁN HÀNG THEO NGÀY":
                    case "TỔNG HỢP DOANH THU THEO LOẠI ĐỒ":
                    case "TỔNG HỢP DOANH THU CHƯA THANH TOÁN":
                    case "BÁO CÁO BÁN HÀNG THEO NGÀY":
                    case "TỔNG HỢP BÁN THEO NHÂN VIÊN":
                    case "TỔNG HỢP MẶT HÀNG BÁN THEO NHÂN VIÊN":
                    case "TỔNG HỢP BÁN HÀNG THEO KHU VỰC":
                    case "TỔNG HỢP BÁN HÀNG THEO BÀN PHÒNG":
                        content = new QuanLyBar.Client.Views.BaoCaoBanHang.BaoCaoBanHangTheoNgayControl(reportTitle);
                        break;

                    case "DANH SÁCH ĐẶT HÀNG THEO NGÀY":
                    case "DANH SÁCH ĐẶT HÀNG THEO KHÁCH HÀNG":
                    case "TỔNG HỢP ĐẶT HÀNG THEO NGÀY":
                    case "TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG":
                    case "TỔNG HỢP MẶT HÀNG ĐẶT THEO KHÁCH HÀNG":
                    case "TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY":
                        content = new QuanLyBar.Client.Views.BaoCaoDatHang.BaoCaoDanhSachDatHangTheoNgayControl(reportTitle);
                        break;

                    case "BÁO CÁO HÀNG TỒN KHO":
                    case "BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN":
                    case "BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN CHI TIẾT":
                    case "THẺ KHO":
                    case "DANH SÁCH PHIẾU NHẬP HÀNG THEO NGÀY":
                    case "DANH SÁCH PHIẾU KIỂM KÊ THEO NGÀY":
                        content = new QuanLyBar.Client.Views.BaoCaoKhoHang.BaoCaoHangTonKhoControl();
                        break;

                    case "BÁO CÁO CÔNG NỢ KHÁCH HÀNG":
                    case "TỔNG HỢP CÔNG NỢ KHÁCH HÀNG":
                    case "ĐỐI CHIẾU CÔNG NỢ KHÁCH HÀNG":
                    case "BÁO CÁO CÔNG NỢ NHÀ CUNG CẤP":
                    case "TỔNG HỢP CÔNG NỢ NHÀ CUNG CẤP":
                    case "ĐỐI CHIẾU CÔNG NỢ NHÀ CUNG CẤP":
                        content = new QuanLyBar.Client.Views.BaoCaoCongNo.BaoCaoTongHopCongNoKhachHangControl();
                        break;

                    case "BÁO CÁO KẾT QUẢ KINH DOANH":
                    case "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ VỐN)":
                    case "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ NHẬP)":
                    case "CHI TIẾT LÃI THEO HÓA ĐƠN (GIÁ VỐN)":
                    case "BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT":
                    case "BÁO CÁO BÁN HÀNG THEO GIỜ":
                    case "DANH SÁCH MÓN XÓA, GIẢM, TRẢ LẠI":
                        content = new QuanLyBar.Client.Views.TongHopKqkdControl();
                        break;

                    default:
                        MessageBox.Show($"Báo cáo '{reportTitle}' đang được mở.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }

                if (content != null)
                {
                    OpenReportWindow(content, reportTitle);
                }
            }
        }

        private void OpenReportWindow(UIElement content, string title)
        {
            try
            {
                var win = new QuanLyBar.Views.TouchPOS.TouchReportViewerWindow(content, title);
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== TRỢ GIÚP HANDLERS =====
        private void BtnMenuHuongDanSuDung_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hướng dẫn sử dụng Phần mềm Quản Lý Bar / Nhà Hàng V6.0.", "Hướng dẫn sử dụng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuHoTroTeamViewer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://teamviewer.com", UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show("Hỗ trợ kỹ thuật từ xa qua TeamViewer / UltraViewer.\nHotline: 0967041111", "Hỗ trợ kỹ thuật", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnMenuDangKyBanQuyen_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đăng ký bản quyền Phần mềm Quản Lý Bar V6.0 Tân An Phát.\nHotline hỗ trợ: 0967041111", "Đăng ký bản quyền", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuThongTinPhanMem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PHẦN MỀM QUẢN LÝ BAR, NHÀ HÀNG V6.0 TÂN AN PHÁT\nHotline: 0967041111\nPhiên bản: 6.0.0.0", "Thông tin phần mềm", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ===== CỘT BÊN PHẢI HANDLERS =====
        private void BtnMenuSuDungDichVu_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Hóa đơn bán hàng", "View", this)) return;
            ShowTableScreen();
        }

        private void BtnMenuTonKho_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Tồn kho", "View", this)) return;
            try
            {
                var win = new TouchTonKhoWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở giao diện Tồn kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuGhiChu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.TienIch.GhiChuNhanhWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Ghi chú nhanh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuDangXuat_Click(object sender, RoutedEventArgs e)
        {
            BtnLogoutPOS_Click(sender, e);
        }

        // ===== TABLE EVENTS =====
        private void SvTableTiles_ScrollChanged(object sender, ScrollChangedEventArgs e) { }

        private void SvTableTiles_SizeChanged(object sender, SizeChangedEventArgs e) { }

        private void SbTableTiles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }

        private void BtnConfigTables_Click(object sender, RoutedEventArgs e) { }

        private void BtnConfigAreas_Click(object sender, RoutedEventArgs e) { }

        private void BtnOpenTablesList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new DanhSachHoaDonChuaThanhToanWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch { }
        }

        private void BtnShiftStats_Click(object sender, RoutedEventArgs e) { }

        private void BtnClosePOS_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Đóng ca làm việc?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                ShowMenuScreen();
        }

        private void BtnLogoutPOS_Click(object sender, RoutedEventArgs e)
        {
            if (QuanLyBar.Views.TouchPOS.TouchConfirmWindow.Show(this, "BẠN CÓ MUỐN ĐĂNG XUẤT KHỎI HỆ THỐNG KHÔNG?"))
            {
                SessionContext.CurrentUser = null;
                ShowLoginScreen();
            }
        }

        // ===== ORDER EVENTS =====
        private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            ShowTableScreen();
        }

        private void TxtOrderCustomer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnSelectCustomer_Click(sender, e);
        }

        private void BtnSelectCustomer_Click(object sender, object e)
        {
            try
            {
                var win = new ChonKhachHangTouchWindow();
                win.Owner = this;
                if (win.ShowDialog() == true && win.SelectedKhachHang != null)
                    TxtOrderCustomer.Text = win.SelectedKhachHang.Name;
            }
            catch { }
        }

        private void BtnOrderDiscount_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrderMoveTable_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrderMergeTable_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrderPrintKitchen_Click(object sender, RoutedEventArgs e) { }
        private void BtnSearchItem_Click(object sender, RoutedEventArgs e) { }

        private void BtnOrderPayment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thanh toán...", "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Simple model classes / helpers for TouchPOS context
    public class DBAN 
    { 
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string MABAN { get; set; } = ""; 
        public string TENBAN { get; set; } = ""; 
        public string MAKHUVUC { get; set; } = "";
        public string MauNen { get; set; } = "#16213E"; 
        public string TrangThai { get; set; } = "Trống"; 
        public bool IsOpened { get; set; } = false;
        public DateTime? ThoiGianMo { get; set; } 
    }
    public class DKHUVUC 
    { 
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string MAKHUVUC { get; set; } = ""; 
        public string TenKhuVuc { get; set; } = ""; 
        public string ColorHex { get; set; } = "#E65100";
        public SolidColorBrush ColorBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(ColorHex);
        public int THUTU { get; set; } 
    }
    public class DNHOMMATHANG { public string MANHOММATHANG { get; set; } = ""; public string TenNhom { get; set; } = ""; public int THUTU { get; set; } }
    public class DMATHANG { public string MAMATHANG { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal GiaBan { get; set; } public string MauNen { get; set; } = "#1976D2"; public string? ANH { get; set; } }
    public class TDONHANGCHITIET { public string MAMATHANG { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal SoLuong { get; set; } public decimal DonGia { get; set; } public decimal THANHTIEN { get; set; } }
}
