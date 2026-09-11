using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                var configs = LocalCauHinhService.LoadAllConfigsAsync().GetAwaiter().GetResult();
                TxtMenuCompanyName.Text = configs.TryGetValue("TEN_CUAHANG", out var companyName)
                    ? companyName
                    : "QUẢN LÝ BAR";
                TxtMenuStatusLeft.Text = $"Nhân viên: {SessionContext.CurrentUser?.TenHienThi}";
                TxtMenuStatusRight.Text = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
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

        private void LoadTables()
        {
            try
            {
                var bans = LocalDatabaseService.GetAll<DBAN>("SELECT T.ID, T.NAME, T.STATUS, T.ID AS MABAN, T.NAME AS TENBAN FROM DBAN T WHERE T.STATUS = 1 ORDER BY T.NAME");
                IcTableTiles.ItemsSource = bans;

                var khuVucList = LocalDatabaseService.GetAll<DKHUVUC>("SELECT T.ID, T.NAME, T.STATUS, T.NAME AS TenKhuVuc FROM DKHUVUC T WHERE T.STATUS = 1 ORDER BY T.SORTORDER");
                IcAreaButtons.ItemsSource = khuVucList;

                TxtCurrentPOSUser.Text = $"👤 {SessionContext.CurrentUser?.TenHienThi}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải bàn: {ex.Message}");
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

        private void DoLogin(string userName, string password)
        {
            try
            {
                var user = LocalAuthService.LoginAsync(userName, password).GetAwaiter().GetResult();
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
            if (MessageBox.Show("Tắt ứng dụng?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void BtnTouchKbUser_Click(object sender, RoutedEventArgs e)
        {
            // Show touch keyboard for username
        }

        private void BtnTouchKbPass_Click(object sender, RoutedEventArgs e)
        {
            // Show touch keyboard for password
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
        private void BtnMenuCategory_Click(object sender, RoutedEventArgs e) { }

        private void BtnMenuSuDungDichVu_Click(object sender, RoutedEventArgs e)
        {
            ShowTableScreen();
        }

        private void BtnMenuTonKho_Click(object sender, RoutedEventArgs e) { }
        private void BtnMenuGhiChu_Click(object sender, RoutedEventArgs e) { }

        private void BtnMenuDangXuat_Click(object sender, RoutedEventArgs e)
        {
            SessionContext.CurrentUser = null;
            ShowLoginScreen();
        }

        // ===== TABLE EVENTS =====
        private void SvTableTiles_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            SbTableTiles.Maximum = SvTableTiles.ScrollableHeight;
            SbTableTiles.Value = SvTableTiles.VerticalOffset;
        }

        private void SvTableTiles_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SbTableTiles.ViewportSize = SvTableTiles.ViewportHeight;
        }

        private void SbTableTiles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SvTableTiles.ScrollToVerticalOffset(e.NewValue);
        }

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
            SessionContext.CurrentUser = null;
            ShowLoginScreen();
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

    // Simple model classes for TouchPOS context
    public class DBAN { public string MABAN { get; set; } = ""; public string TENBAN { get; set; } = ""; public string MauNen { get; set; } = "#16213E"; public string TrangThai { get; set; } = "Trống"; public DateTime? ThoiGianMo { get; set; } }
    public class DKHUVUC { public string MAKHUVUC { get; set; } = ""; public string TenKhuVuc { get; set; } = ""; public int THUTU { get; set; } }
    public class DNHOMMATHANG { public string MANHOММATHANG { get; set; } = ""; public string TenNhom { get; set; } = ""; public int THUTU { get; set; } }
    public class DMATHANG { public string MAMATHANG { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal GiaBan { get; set; } public string MauNen { get; set; } = "#1976D2"; public string? ANH { get; set; } }
    public class TDONHANGCHITIET { public string MAMATHANG { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal SoLuong { get; set; } public decimal DonGia { get; set; } public decimal THANHTIEN { get; set; } }
}
