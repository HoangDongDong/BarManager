using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client
{
    public partial class MainAppWindow : Window
    {
        public MainAppWindow()
        {
            InitializeComponent();
            LoadUserInfo();

            PreviewKeyDown += (s, e) =>
            {
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                {
                    if (e.Key == Key.T)
                    {
                        if (!LocalPhanQuyenService.CheckPermissionAndAlert("Tạo phiếu thu", "View", this)) return;
                        var win = new QuanLyBar.Client.Views.PhieuThuChi.TaoPhieuThuWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.C)
                    {
                        if (!LocalPhanQuyenService.CheckPermissionAndAlert("Tạo phiếu chi", "View", this)) return;
                        var win = new QuanLyBar.Client.Views.PhieuThuChi.TaoPhieuChiWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        e.Handled = true;
                    }
                }
            };
        }

        private void LoadUserInfo()
        {
            if (SessionContext.CurrentUser != null)
            {
                var userInfoStr = $"Nhân viên: {SessionContext.CurrentUser.TenDangNhap} | Vai trò: {SessionContext.CurrentUser.VaiTro}";
                this.Title = $"Phần Mềm Quản Lý Bar, Nhà Hàng - [{userInfoStr}]";
                
                // 1. Tự động ẩn/hiện Menu bar và Toolbar theo quyền thực tế của tài khoản
                ApplyPermissionsToUI();

                // 2. Tự động mở các tab mà tài khoản có quyền xem
                if (LocalPhanQuyenService.HasFunctionPermission("Sử dụng dịch vụ", "View"))
                    AddTab("Sử dụng dịch vụ", new QuanLyBar.Client.Views.SuDungDichVuControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Danh mục nhà cung cấp", "View"))
                    AddTab("Danh mục nhà cung cấp", new QuanLyBar.Client.Views.DanhMucNhaCungCap.DanhMucNhaCungCapControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Nhập kho", "View"))
                    AddTab("Quản lý nhập kho", new QuanLyBar.Client.Views.QuanLyNhapKho.QuanLyNhapKhoControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Xuất kho", "View"))
                    AddTab("Quản lý xuất kho", new QuanLyBar.Client.Views.QuanLyXuatKho.QuanLyXuatKhoControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Quản lý chuyển kho", "View"))
                    AddTab("Quản lý chuyển kho", new QuanLyBar.Client.Views.QuanLyChuyenKho.QuanLyChuyenKhoControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Kiểm kê kho", "View"))
                    AddTab("Quản lý kiểm kê", new QuanLyBar.Client.Views.QuanLyKiemKe.QuanLyKiemKeControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Thưởng phạt", "View"))
                    AddTab("Thưởng phạt", new QuanLyBar.Client.Views.NhanSu.ThuongPhatControl());

                if (LocalPhanQuyenService.HasFunctionPermission("Chấm công", "View"))
                    AddTab("Chấm công", new QuanLyBar.Client.Views.NhanSu.ChamCongControl());

                if (MainTabControl.Items.Count > 0)
                {
                    MainTabControl.SelectedIndex = 0;
                }
            }
        }

        private void ApplyPermissionsToUI()
        {
            if (SessionContext.CurrentUser == null) return;
            bool isAdmin = SessionContext.CurrentUser.IsAdmin || SessionContext.CurrentUser.TenDangNhap?.ToLower() == "admin";

            // 1. Áp dụng phân quyền cho Menu bar
            if (MainMenu != null)
            {
                foreach (var item in MainMenu.Items)
                {
                    if (item is MenuItem rootMenu)
                    {
                        ApplyPermissionsToMenuItem(rootMenu, isAdmin);
                    }
                }
            }

            // 2. Áp dụng phân quyền cho Toolbar
            if (MainToolBar != null)
            {
                foreach (var child in MainToolBar.Items)
                {
                    if (child is Button btn)
                    {
                        string text = btn.Content?.ToString()?.Trim() ?? "";
                        if (text == "Thoát" || text == "Ghi chú")
                        {
                            btn.Visibility = Visibility.Visible;
                        }
                        else if (text == "Báo cáo")
                        {
                            btn.Visibility = isAdmin || LocalPhanQuyenService.MasterReports.Any(r => LocalPhanQuyenService.HasReportPermission(r.Name)) 
                                ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else
                        {
                            string funcName = MapTabNameToFunctionName(text);
                            btn.Visibility = (isAdmin || LocalPhanQuyenService.HasFunctionPermission(funcName, "View"))
                                ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }

                // Ẩn separator thừa trên Toolbar
                UIElement lastVisible = null;
                foreach (UIElement elem in MainToolBar.Items)
                {
                    if (elem is Separator sep)
                    {
                        if (lastVisible == null || lastVisible is Separator)
                        {
                            sep.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            sep.Visibility = Visibility.Visible;
                            lastVisible = sep;
                        }
                    }
                    else if (elem.Visibility == Visibility.Visible)
                    {
                        lastVisible = elem;
                    }
                }
                if (lastVisible is Separator trailingSep)
                {
                    trailingSep.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool ApplyPermissionsToMenuItem(MenuItem mi, bool isAdmin)
        {
            if (mi == null) return false;

            string header = mi.Header?.ToString()?.Trim() ?? "";
            if (header.Contains("Ctrl+"))
            {
                header = header.Substring(0, header.IndexOf("Ctrl+")).Trim();
            }

            // Các menu hệ thống cơ bản luôn cho phép hiển thị
            if (header == "TRỢ GIÚP" || header == "Đổi mật khẩu đăng nhập" || 
                header == "Đăng xuất khỏi hệ thống" || header == "Thoát khỏi hệ thống" || 
                header == "Đăng ký bản quyền..." || header == "Thông tin phần mềm")
            {
                mi.Visibility = Visibility.Visible;
                return true;
            }

            if (mi.Items.Count > 0)
            {
                bool hasVisibleChild = false;

                foreach (var child in mi.Items)
                {
                    if (child is MenuItem childMi)
                    {
                        bool childVisible = ApplyPermissionsToMenuItem(childMi, isAdmin);
                        if (childVisible)
                        {
                            hasVisibleChild = true;
                        }
                    }
                }

                // Xử lý ẩn các Separator thừa trong submenu
                UIElement prev = null;
                foreach (var child in mi.Items)
                {
                    if (child is Separator sep)
                    {
                        if (prev == null || prev is Separator)
                        {
                            sep.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            sep.Visibility = Visibility.Visible;
                            prev = sep;
                        }
                    }
                    else if (child is UIElement elem && elem.Visibility == Visibility.Visible)
                    {
                        prev = elem;
                    }
                }
                if (prev is Separator lastSep)
                {
                    lastSep.Visibility = Visibility.Collapsed;
                }

                mi.Visibility = (isAdmin || hasVisibleChild) ? Visibility.Visible : Visibility.Collapsed;
                return mi.Visibility == Visibility.Visible;
            }
            else
            {
                // Leaf MenuItem (mục menu cấp cuối)
                bool isReport = IsReportMenuItem(mi);
                
                bool canView;
                if (isAdmin)
                {
                    canView = true;
                }
                else if (isReport)
                {
                    canView = LocalPhanQuyenService.HasReportPermission(header);
                }
                else
                {
                    string funcName = MapTabNameToFunctionName(header);
                    canView = LocalPhanQuyenService.HasFunctionPermission(funcName, "View");
                }

                mi.Visibility = canView ? Visibility.Visible : Visibility.Collapsed;
                return canView;
            }
        }

        private bool IsReportMenuItem(MenuItem mi)
        {
            if (mi == null) return false;
            DependencyObject current = mi;
            while (current != null)
            {
                if (current is MenuItem p && p != mi)
                {
                    string h = p.Header?.ToString()?.Trim() ?? "";
                    if (h.IndexOf("BÁO CÁO", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                current = LogicalTreeHelper.GetParent(current) ?? VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private string MapTabNameToFunctionName(string tabName)
        {
            if (string.IsNullOrWhiteSpace(tabName)) return "";
            string t = tabName.Trim();
            if (t == "Sử dụng dịch vụ") return "Hóa đơn bán hàng";
            if (t == "Khách đặt hàng" || t == "Theo dõi đặt phòng" || t == "Đặt phòng") return "Đặt hàng";
            if (t == "Thống kê bán hàng") return "Thống kê mặt hàng bán";
            if (t == "Tổng hợp KQKD") return "Tổng hợp kết quả kinh doanh";
            if (t == "Chi tiết hoạt động") return "Chi tiết hoạt động ngày";
            if (t == "Nhập hàng vào kho" || t == "Phiếu nhập kho" || t == "Quản lý nhập kho") return "Nhập kho";
            if (t == "Xuất khác" || t == "Xuất kho" || t == "Phiếu xuất kho" || t == "Quản lý xuất kho") return "Xuất kho";
            if (t == "Chuyển kho" || t == "Phiếu chuyển kho" || t == "Quản lý chuyển kho") return "Chuyển kho";
            if (t == "Kiểm kê kho" || t == "Kiểm kê" || t == "Phiếu kiểm kê" || t == "Quản lý kiểm kê") return "Kiểm kê kho";
            if (t == "Kho hàng") return "Danh mục kho hàng";
            if (t == "Nhà cung cấp") return "Danh mục nhà cung cấp";
            if (t == "Nhân viên") return "Danh mục nhân viên";
            if (t == "Ca làm việc") return "Danh mục ca làm việc";
            if (t == "Lý do thu chi") return "Danh mục lý do thu chi";
            if (t == "Tài khoản ngân hàng") return "Danh mục tài khoản ngân hàng";
            if (t == "Thẻ trả trước") return "Danh mục thẻ trả trước";
            if (t == "Đợt khuyến mại") return "Danh mục đợt khuyến mại";
            if (t == "Quản lý người dùng" || t == "Phân quyền" || t == "Người dùng và phân quyền") return "Quản lý người dùng";
            if (t == "Báo cáo tồn quỹ" || t == "BÁO CÁO TỒN QUỸ") return "Tồn quỹ";
            if (t == "Quản lý phiếu thu" || t == "Tạo phiếu thu" || t == "Phiếu thu" || t == "Tạo phiếu chi" || t == "Phiếu chi" || t == "Quản lý phiếu chi") return "Danh mục phiếu thu chi";
            if (t == "Thưởng phạt" || t == "Quản lý thưởng phạt") return "Quản lý thưởng phạt";
            if (t == "Quản lý chấm công") return "Chấm công";
            if (t == "Bảng tính lương" || t == "Bảng lương") return "Tính lương";
            if (t == "Quản lý công nợ khách hàng") return "Công nợ khách hàng";
            if (t == "Quản lý công nợ nhà cung cấp") return "Công nợ nhà cung cấp";
            if (t == "Báo cáo tồn kho" || t == "Quản lý tồn kho" || t == "Tồn nhiều kho" || t == "Báo cáo tồn nhiều kho") return "Tồn kho";
            if (t == "Gửi tin nhắn khách hàng" || t == "Gửi tin nhắn") return "Gửi tin nhắn tới khách hàng";
            if (t == "Thư viện ảnh..." || t == "Quản lý thư viện ảnh") return "Thư viện ảnh";
            if (t == "CÔNG NỢ BAN ĐẦU" || t == "Công nợ ban đầu" || t == "Công nợ khách hàng ban đầu" || t == "Công nợ nhà cung cấp ban đầu" || t == "Tồn kho ban đầu") return "Dữ liệu ban đầu";
            return t;
        }

        private string NormalizeTabName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            if (name == "Tổng hợp KQKD" || name == "Tổng hợp kết quả kinh doanh")
                return "Tổng hợp kết quả kinh doanh";
            if (name == "Chi tiết hoạt động" || name == "Chi tiết hoạt động ngày")
                return "Chi tiết hoạt động ngày";
            if (name == "Thống kê bán hàng" || name == "Thống kê mặt hàng bán")
                return "Thống kê mặt hàng bán";
            return name;
        }

        public void AddTab(string header, UIElement content)
        {
            try
            {
                string normalizedHeader = NormalizeTabName(header);

                // Kiểm tra xem tab đã tồn tại chưa
                foreach (System.Windows.Controls.TabItem tab in MainTabControl.Items)
                {
                    if (NormalizeTabName(tab.Header?.ToString() ?? "") == normalizedHeader)
                    {
                        tab.IsSelected = true;
                        MainTabControl.SelectedItem = tab;
                        MainTabControl.UpdateLayout();
                        return;
                    }
                }

                // Tạo tab mới
                var newTab = new System.Windows.Controls.TabItem
                {
                    Header = normalizedHeader,
                    Content = new System.Windows.Controls.Border
                    {
                        Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#eef3f9")),
                        Child = content
                    }
                };

                MainTabControl.Items.Add(newTab);
                newTab.IsSelected = true;
                MainTabControl.SelectedItem = newTab;
                MainTabControl.UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị tab {header}: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tabName = string.Empty;

                if (sender is System.Windows.Controls.Button button)
                {
                    tabName = button.Content?.ToString() ?? "";
                }
                else if (sender is System.Windows.Controls.MenuItem menuItem)
                {
                    tabName = menuItem.Header?.ToString() ?? "";
                }

                if (!string.IsNullOrEmpty(tabName))
                {
                    if (tabName.Contains("Ctrl+"))
                    {
                        tabName = tabName.Substring(0, tabName.IndexOf("Ctrl+")).Trim();
                    }

                    tabName = NormalizeTabName(tabName);

                    string funcName = MapTabNameToFunctionName(tabName);
                    if (!LocalPhanQuyenService.CheckPermissionAndAlert(funcName, "View", this))
                    {
                        return;
                    }

                    System.Windows.UIElement content;

                    if (tabName == "Danh mục mặt hàng")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucMatHangControl();
                    }
                    else if (tabName == "Danh mục bàn khu vực")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucBanKhuVucControl();
                    }
                    else if (tabName == "Khách đặt hàng")
                    {
                        content = new QuanLyBar.Client.Views.KhachDatHangControl();
                    }
                    else if (tabName == "Theo dõi đặt phòng")
                    {
                        content = new QuanLyBar.Client.Views.TheoDoiDatPhongControl();
                    }
                    else if (tabName == "Sử dụng dịch vụ")
                    {
                        content = new QuanLyBar.Client.Views.SuDungDichVuControl();
                    }
                    else if (tabName == "Điều chỉnh hóa đơn")
                    {
                        content = new QuanLyBar.Client.Views.DieuChinhHoaDonControl();
                    }
                    else if (tabName == "Quản lý bán hàng")
                    {
                        content = new QuanLyBar.Client.Views.QuanLyBanHangControl();
                    }
                    else if (tabName == "Lưu vết hoạt động")
                    {
                        content = new QuanLyBar.Client.Views.LuuVetHoatDongControl();
                    }
                    else if (tabName == "Thống kê doanh thu")
                    {
                        content = new QuanLyBar.Client.Views.ThongKeDoanhThuControl();
                    }
                    else if (tabName == "Thống kê mặt hàng bán")
                    {
                        content = new QuanLyBar.Client.Views.ThongKeMatHangBanControl();
                    }
                    else if (tabName == "Tổng hợp kết quả kinh doanh")
                    {
                        content = new QuanLyBar.Client.Views.TongHopKqkdControl();
                    }
                    else if (tabName == "Chi tiết hoạt động ngày")
                    {
                        content = new QuanLyBar.Client.Views.ChiTietHoatDongControl();
                    }
                    else if (tabName == "Danh mục hóa đơn hủy")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucHoaDonHuyControl();
                    }
                    else if (tabName == "Danh mục khách hàng")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucKhachHangControl();
                    }
                    else if (tabName == "Gửi tin nhắn tới khách hàng" || tabName == "Gửi tin nhắn khách hàng" || tabName == "Gửi tin nhắn")
                    {
                        content = new QuanLyBar.Client.Views.GuiTinNhanKhachHangControl();
                    }
                    else if (tabName == "Danh mục đợt khuyến mại" || tabName == "Đợt khuyến mại")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucDotKhuyenMaiControl();
                    }
                    else if (tabName == "Khách hàng thân thiết")
                    {
                        content = new QuanLyBar.Client.Views.KhachHangThanThietControl();
                    }
                    else if (tabName == "Danh mục thẻ trả trước" || tabName == "Thẻ trả trước")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucTheTraTruocControl();
                    }
                    else if (tabName == "Danh mục kho hàng" || tabName == "Kho hàng")
                    {
                        var win = new QuanLyBar.Client.Views.KhoHang.DanhMucKhoHangWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Danh mục nhà cung cấp" || tabName == "Nhà cung cấp")
                    {
                        content = new QuanLyBar.Client.Views.DanhMucNhaCungCap.DanhMucNhaCungCapControl();
                    }
                    else if (tabName == "Quản lý nhập kho" || tabName == "Nhập hàng vào kho" || tabName == "Phiếu nhập kho")
                    {
                        content = new QuanLyBar.Client.Views.QuanLyNhapKho.QuanLyNhapKhoControl();
                    }
                    else if (tabName == "Quản lý xuất kho" || tabName == "Xuất khác" || tabName == "Xuất kho" || tabName == "Phiếu xuất kho")
                    {
                        tabName = "Quản lý xuất kho";
                        content = new QuanLyBar.Client.Views.QuanLyXuatKho.QuanLyXuatKhoControl();
                    }
                    else if (tabName == "Quản lý chuyển kho" || tabName == "Chuyển kho" || tabName == "Phiếu chuyển kho")
                    {
                        tabName = "Quản lý chuyển kho";
                        content = new QuanLyBar.Client.Views.QuanLyChuyenKho.QuanLyChuyenKhoControl();
                    }
                    else if (tabName == "Quản lý kiểm kê" || tabName == "Kiểm kê kho" || tabName == "Kiểm kê" || tabName == "Phiếu kiểm kê")
                    {
                        tabName = "Quản lý kiểm kê";
                        content = new QuanLyBar.Client.Views.QuanLyKiemKe.QuanLyKiemKeControl();
                    }
                    else if (tabName == "Tính lại giá vốn" || tabName == "Tính lại giá vốn hàng bán")
                    {
                        var win = new QuanLyBar.Client.Views.KhoHang.TinhLaiGiaVonWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Xuất lại định lượng")
                    {
                        var win = new QuanLyBar.Client.Views.KhoHang.XuatLaiDinhLuongWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Tồn kho" || tabName == "Báo cáo tồn kho" || tabName == "Quản lý tồn kho")
                    {
                        tabName = "Tồn kho";
                        content = new QuanLyBar.Client.Views.TonKho.TonKhoControl();
                    }
                    else if (tabName == "Tồn nhiều kho" || tabName == "Báo cáo tồn nhiều kho")
                    {
                        tabName = "Tồn nhiều kho";
                        content = new QuanLyBar.Client.Views.TonKho.TonNhieuKhoControl();
                    }
                    else if (tabName == "Công nợ khách hàng" || tabName == "Quản lý công nợ khách hàng")
                    {
                        tabName = "Công nợ khách hàng";
                        content = new QuanLyBar.Client.Views.CongNo.CongNoKhachHangControl();
                    }
                    else if (tabName == "Danh mục lý do thu chi" || tabName == "Lý do thu chi")
                    {
                        var win = new QuanLyBar.Client.Views.DanhMucLyDoThuChi.DanhMucLyDoThuChiWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Danh mục nhân viên" || tabName == "Nhân viên")
                    {
                        var win = new QuanLyBar.Client.Views.DanhMucNhanVien.DanhMucNhanVienWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Danh mục ca làm việc" || tabName == "Ca làm việc")
                    {
                        var win = new QuanLyBar.Client.Views.DanhMucCaLamViec.DanhMucCaLamViecWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Danh mục tài khoản ngân hàng" || tabName == "Tài khoản ngân hàng")
                    {
                        var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.DanhMucTaiKhoanNganHangWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Tạo phiếu thu" || tabName == "Phiếu thu")
                    {
                        var win = new QuanLyBar.Client.Views.PhieuThuChi.TaoPhieuThuWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Tạo phiếu chi" || tabName == "Phiếu chi")
                    {
                        var win = new QuanLyBar.Client.Views.PhieuThuChi.TaoPhieuChiWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Danh mục phiếu thu" || tabName == "Quản lý phiếu thu")
                    {
                        tabName = "Danh mục phiếu thu";
                        content = new QuanLyBar.Client.Views.PhieuThuChi.DanhMucPhieuThuControl();
                    }
                    else if (tabName == "Danh mục phiếu chi" || tabName == "Quản lý phiếu chi")
                    {
                        tabName = "Danh mục phiếu chi";
                        content = new QuanLyBar.Client.Views.PhieuThuChi.DanhMucPhieuChiControl();
                    }
                    else if (tabName == "Công nợ nhà cung cấp" || tabName == "Quản lý công nợ nhà cung cấp")
                    {
                        tabName = "Công nợ nhà cung cấp";
                        content = new QuanLyBar.Client.Views.CongNo.CongNoNhaCungCapControl();
                    }
                    else if (tabName == "Tồn quỹ" || tabName == "Báo cáo tồn quỹ" || tabName == "BÁO CÁO TỒN QUỸ")
                    {
                        tabName = "Tồn quỹ";
                        content = new QuanLyBar.Client.Views.TonQuy.TonQuyControl();
                    }
                    else if (tabName == "Tạm ứng lương" || tabName == "Tạm ứng")
                    {
                        tabName = "Tạm ứng lương";
                        content = new QuanLyBar.Client.Views.NhanSu.TamUngLuongControl();
                    }
                    else if (tabName == "Thưởng phạt" || tabName == "Quản lý thưởng phạt")
                    {
                        tabName = "Thưởng phạt";
                        content = new QuanLyBar.Client.Views.NhanSu.ThuongPhatControl();
                    }
                    else if (tabName == "Chấm công" || tabName == "Quản lý chấm công")
                    {
                        tabName = "Chấm công";
                        content = new QuanLyBar.Client.Views.NhanSu.ChamCongControl();
                    }
                    else if (tabName == "Tính lương" || tabName == "Bảng tính lương" || tabName == "Bảng lương")
                    {
                        tabName = "Tính lương";
                        content = new QuanLyBar.Client.Views.NhanSu.TinhLuongControl();
                    }
                    else if (tabName == "Thư viện ảnh..." || tabName == "Thư viện ảnh" || tabName == "Quản lý thư viện ảnh")
                    {
                        var win = new QuanLyBar.Client.Views.ThuVienAnh.ThuVienAnhWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Công nợ khách hàng ban đầu" || tabName == "CÔNG NỢ BAN ĐẦU" || tabName == "Công nợ ban đầu")
                    {
                        var win = new QuanLyBar.Client.Views.DuLieuBanDau.CongNoKhachHangBanDauWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Công nợ nhà cung cấp ban đầu")
                    {
                        var win = new QuanLyBar.Client.Views.DuLieuBanDau.CongNoNhaCungCapBanDauWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Tồn kho ban đầu")
                    {
                        var chonKhoWin = new QuanLyBar.Client.Views.DuLieuBanDau.ChonKhoTonBanDauWindow();
                        chonKhoWin.Owner = this;
                        if (chonKhoWin.ShowDialog() == true && chonKhoWin.SelectedKho != null)
                        {
                            var win = new QuanLyBar.Client.Views.DuLieuBanDau.TonKhoBanDauWindow(chonKhoWin.SelectedKho.Id);
                            win.Owner = this;
                            win.ShowDialog();
                        }
                        return;
                    }
                    else if (tabName == "Người dùng và phân quyền" || tabName == "Quản lý người dùng" || tabName == "Phân quyền")
                    {
                        var win = new QuanLyBar.Client.Views.NguoiDungPhanQuyen.NguoiDungPhanQuyenWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else if (tabName == "Cấu hình toàn hệ thống" || tabName == "Cấu hình hệ thống")
                    {
                        var win = new QuanLyBar.Client.Views.CauHinhHeThong.CauHinhToanHeThongWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        return;
                    }
                    else
                    {
                        content = new System.Windows.Controls.TextBlock
                        {
                            Text = $"Nội dung của màn hình: {tabName}\n(Đang tải từ file UserControl...)",
                            FontSize = 18,
                            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }

                    AddTab(tabName, content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở tab: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Tab Management (Pin & Close)
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePinButtonState();
        }

        private void UpdatePinButtonState()
        {
            if (BtnPinCurrentTab == null) return;

            if (MainTabControl.SelectedItem is TabItem currentTab && currentTab.Tag?.ToString() == "Pinned")
            {
                BtnPinCurrentTab.ToolTip = "Bỏ ghim tab này";
                BtnPinCurrentTab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fed7aa"));
            }
            else
            {
                BtnPinCurrentTab.ToolTip = "Ghim tab này";
                BtnPinCurrentTab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f4fa"));
            }
        }

        private void BtnPinTab_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem currentTab)
            {
                TogglePinTab(currentTab);
            }
        }

        private void TogglePinTab(TabItem tab)
        {
            if (tab == null) return;

            if (tab.Tag?.ToString() == "Pinned")
            {
                tab.Tag = null;
            }
            else
            {
                tab.Tag = "Pinned";
            }
            UpdatePinButtonState();
        }

        private void BtnCloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            CloseAllTabs(onlyUnpinned: true);
        }

        private void CloseAllTabs(bool onlyUnpinned = true)
        {
            for (int i = MainTabControl.Items.Count - 1; i >= 0; i--)
            {
                if (MainTabControl.Items[i] is TabItem tab)
                {
                    if (!onlyUnpinned || tab.Tag?.ToString() != "Pinned")
                    {
                        MainTabControl.Items.RemoveAt(i);
                    }
                }
            }

            if (MainTabControl.Items.Count > 0 && MainTabControl.SelectedItem == null)
            {
                MainTabControl.SelectedIndex = 0;
            }
            UpdatePinButtonState();
        }

        private void TabItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is TabItem tab)
            {
                tab.IsSelected = true;
                MainTabControl.SelectedItem = tab;
                if (tab.ContextMenu != null)
                {
                    foreach (var item in tab.ContextMenu.Items)
                    {
                        if (item is MenuItem mi && (mi.Name == "CtxPinItem" || mi.Header?.ToString().Contains("Ghim tab") == true || mi.Header?.ToString().Contains("Bỏ ghim tab") == true))
                        {
                            mi.Header = (tab.Tag?.ToString() == "Pinned") ? "📌 Bỏ ghim tab này" : "📌 Ghim tab này";
                            break;
                        }
                    }
                }
            }
        }

        private void MenuPinTab_Click(object sender, RoutedEventArgs e)
        {
            TabItem tab = GetContextMenuTabItem(sender);
            if (tab != null)
            {
                TogglePinTab(tab);
            }
            else if (MainTabControl.SelectedItem is TabItem currentTab)
            {
                TogglePinTab(currentTab);
            }
        }

        private void MenuCloseCurrentTab_Click(object sender, RoutedEventArgs e)
        {
            TabItem targetTab = GetContextMenuTabItem(sender) ?? (MainTabControl.SelectedItem as TabItem);
            if (targetTab != null)
            {
                MainTabControl.Items.Remove(targetTab);
                UpdatePinButtonState();
            }
        }

        private void MenuCloseOtherTabs_Click(object sender, RoutedEventArgs e)
        {
            TabItem currentTab = GetContextMenuTabItem(sender) ?? (MainTabControl.SelectedItem as TabItem);
            if (currentTab == null) return;

            for (int i = MainTabControl.Items.Count - 1; i >= 0; i--)
            {
                if (MainTabControl.Items[i] is TabItem t && t != currentTab)
                {
                    if (t.Tag?.ToString() != "Pinned")
                    {
                        MainTabControl.Items.RemoveAt(i);
                    }
                }
            }
            currentTab.IsSelected = true;
            UpdatePinButtonState();
        }

        private void MenuCloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            CloseAllTabs(onlyUnpinned: true);
        }

        private TabItem GetContextMenuTabItem(object sender)
        {
            if (sender is MenuItem mi)
            {
                var cm = FindVisualParent<ContextMenu>(mi) ?? mi.Parent as ContextMenu;
                if (cm != null && cm.PlacementTarget is TabItem t)
                {
                    return t;
                }
            }
            return MainTabControl.SelectedItem as TabItem;
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(button);
                while (parent != null && !(parent is System.Windows.Controls.TabItem))
                {
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                }

                if (parent is System.Windows.Controls.TabItem tabItem)
                {
                    MainTabControl.Items.Remove(tabItem);
                    UpdatePinButtonState();
                }
            }
        }
        #endregion

        private void MenuTaoMoiCsdl_Click(object sender, RoutedEventArgs e)
        {
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

                    // 1. Copy từ file template Firebird chuẩn
                    try
                    {
                        string templatePath = @"D:\taifirebird\new.fdb";
                        if (!System.IO.File.Exists(templatePath))
                        {
                            templatePath = @"D:\taifirebird\DEMO.FDB";
                        }

                        if (System.IO.File.Exists(templatePath))
                        {
                            System.IO.File.Copy(templatePath, filename, true);
                        }
                    }
                    catch (Exception exCopy)
                    {
                        MessageBox.Show($"Cảnh báo khi copy file mẫu: {exCopy.Message}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // 2. Thêm vào danh sách databases.json
                    var newDb = new DatabaseInfo
                    {
                        Name = dbName,
                        Path = filename,
                        ConnectionType = 2, // Firebird File
                        Server = "localhost",
                        Username = "SYSDBA",
                        Password = "masterkey"
                    };

                    try
                    {
                        string dataFile = "databases.json";
                        var dbList = new System.Collections.ObjectModel.ObservableCollection<DatabaseInfo>();
                        if (System.IO.File.Exists(dataFile))
                        {
                            string json = System.IO.File.ReadAllText(dataFile);
                            var loaded = System.Text.Json.JsonSerializer.Deserialize<System.Collections.ObjectModel.ObservableCollection<DatabaseInfo>>(json);
                            if (loaded != null) dbList = loaded;
                        }
                        dbList.Add(newDb);
                        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                        System.IO.File.WriteAllText(dataFile, System.Text.Json.JsonSerializer.Serialize(dbList, options));
                    }
                    catch { }

                    var ask = MessageBox.Show($"Đã tạo mới cơ sở dữ liệu trắng thành công tại:\n{filename}\n\nBạn có muốn chuyển sang làm việc với cơ sở dữ liệu mới này ngay bây giờ không?", 
                                              "Tạo CSDL thành công", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ask == MessageBoxResult.Yes)
                    {
                        QuanLyBar.Client.Services.DbConnectionManager.SaveConfig(newDb);
                        Application.Current.Properties["SelectedDbName"] = newDb.Name;
                        MessageBox.Show($"Đã thiết lập kết nối sang CSDL: {newDb.Name}.\nVui lòng khởi động lại ứng dụng hoặc đăng nhập lại để nạp dữ liệu trắng mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo mới cơ sở dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MenuSaoLuuCsdl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentDb = QuanLyBar.Client.Services.DbConnectionManager.CurrentConfig;
                string dbName = currentDb?.Name ?? "DATABASE";
                string currentDbPath = currentDb?.Path ?? @"D:\taifirebird\DEMO.FDB";

                var saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Firebird Backup (*.gbk)|*.gbk|All files (*.*)|*.*",
                    Title = "Sao lưu cơ sở dữ liệu",
                    DefaultExt = ".gbk",
                    FileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.gbk"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    string backupPath = saveDlg.FileName;

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            string server = string.IsNullOrEmpty(currentDb?.Server) ? "localhost" : currentDb.Server;
                            string user = string.IsNullOrEmpty(currentDb?.Username) ? "SYSDBA" : currentDb.Username;
                            string pass = string.IsNullOrEmpty(currentDb?.Password) ? "masterkey" : currentDb.Password;

                            var backup = new FirebirdSql.Data.Services.FbBackup
                            {
                                ConnectionString = $"Server={server};Database={currentDbPath};User={user};Password={pass};Charset=UTF8;"
                            };
                            backup.BackupFiles.Add(new FirebirdSql.Data.Services.FbBackupFile(backupPath, 2048));
                            backup.Verbose = true;
                            backup.Options = FirebirdSql.Data.Services.FbBackupFlags.IgnoreLimbo;
                            backup.Execute();
                        }
                        catch
                        {
                            if (System.IO.File.Exists(currentDbPath))
                            {
                                System.IO.File.Copy(currentDbPath, backupPath, true);
                            }
                        }
                    });

                    MessageBox.Show($"Sao lưu cơ sở dữ liệu thành công ra file:\n{backupPath}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sao lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuPhucHoiCsdl_Click(object sender, RoutedEventArgs e)
        {
            var win = new QuanLyBar.Client.Views.KhoiPhucCsdlWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void MenuDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đổi mật khẩu tài khoản.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SessionContext.Clear();
                var loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        #region Chrome-Style Dynamic Animated Tab Dragging & Sliding
        private TabItem _draggedTab = null;
        private Point _dragStartScreenPos;
        private double _startMouseTabControlX;
        private bool _isDraggingTab = false;
        private int _initialTabIndex = -1;
        private int _currentTargetIndex = -1;
        private List<double> _initialTabLefts = new List<double>();
        private List<double> _initialTabWidths = new List<double>();

        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (sender is TabItem tabItem && e.OriginalSource is DependencyObject dep)
            {
                // 1. Không kích hoạt kéo nếu bấm vào nút đóng tab ✕
                if (FindVisualParent<System.Windows.Controls.Button>(dep) != null)
                {
                    return;
                }

                // 2. Không kích hoạt nếu bấm vào nội dung bên trong Tab (Content / DataGrid / ScrollBar)
                if (tabItem.Content is DependencyObject contentDep && (dep == contentDep || IsDescendantOf(dep, contentDep)))
                {
                    return;
                }

                // 3. Chỉ kích hoạt khi click vào phần Header của TabItem
                var headerBorder = tabItem.Template?.FindName("Border", tabItem) as FrameworkElement;
                if (headerBorder != null)
                {
                    if (dep != headerBorder && !IsDescendantOf(dep, headerBorder))
                    {
                        return;
                    }
                }
                else
                {
                    // Fallback: nếu click không nằm trong TabItem Header
                    var parentTab = FindVisualParent<TabItem>(dep);
                    if (parentTab == null || parentTab != tabItem)
                    {
                        return;
                    }
                }

                _draggedTab = tabItem;
                _dragStartScreenPos = e.GetPosition(this);
                _startMouseTabControlX = e.GetPosition(MainTabControl).X;
                _isDraggingTab = false;
                _initialTabIndex = MainTabControl.Items.IndexOf(tabItem);
                _currentTargetIndex = _initialTabIndex;
            }
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedTab != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentScreenPos = e.GetPosition(this);
                double diffX = currentScreenPos.X - _dragStartScreenPos.X;
                double diffY = currentScreenPos.Y - _dragStartScreenPos.Y;

                if (!_isDraggingTab && (Math.Abs(diffX) > 8 || Math.Abs(diffY) > 8))
                {
                    _isDraggingTab = true;

                    _initialTabLefts.Clear();
                    _initialTabWidths.Clear();
                    foreach (TabItem t in MainTabControl.Items)
                    {
                        if (!(t.RenderTransform is TranslateTransform))
                        {
                            t.RenderTransform = new TranslateTransform();
                        }
                        var pt = t.TranslatePoint(new Point(0, 0), MainTabControl);
                        _initialTabLefts.Add(pt.X);
                        _initialTabWidths.Add(t.ActualWidth);
                    }

                    Panel.SetZIndex(_draggedTab, 999);
                    _draggedTab.Opacity = 0.88;
                    _draggedTab.CaptureMouse();
                }

                if (_isDraggingTab)
                {
                    double currentMouseX = e.GetPosition(MainTabControl).X;
                    double deltaX = currentMouseX - _startMouseTabControlX;

                    // Tab đang kéo di chuyển trơn tru theo con trỏ chuột
                    if (_draggedTab.RenderTransform is TranslateTransform draggedTransform)
                    {
                        draggedTransform.BeginAnimation(TranslateTransform.XProperty, null);
                        draggedTransform.X = deltaX;
                    }

                    // Tính toán vị trí tâm hiện tại của tab đang kéo
                    if (_initialTabIndex >= 0 && _initialTabIndex < _initialTabLefts.Count)
                    {
                        double draggedOriginalCenter = _initialTabLefts[_initialTabIndex] + _initialTabWidths[_initialTabIndex] / 2.0;
                        double draggedCurrentCenter = draggedOriginalCenter + deltaX;

                        int newTargetIndex = _initialTabIndex;
                        for (int i = 0; i < MainTabControl.Items.Count; i++)
                        {
                            if (i == _initialTabIndex) continue;

                            double slotCenter = _initialTabLefts[i] + _initialTabWidths[i] / 2.0;
                            if (i < _initialTabIndex && draggedCurrentCenter < slotCenter)
                            {
                                if (i < newTargetIndex) newTargetIndex = i;
                            }
                            else if (i > _initialTabIndex && draggedCurrentCenter > slotCenter)
                            {
                                if (i > newTargetIndex) newTargetIndex = i;
                            }
                        }

                        if (newTargetIndex != _currentTargetIndex)
                        {
                            _currentTargetIndex = newTargetIndex;
                            AnimateNeighborTabs();
                        }
                    }
                }
            }
        }

        private void AnimateNeighborTabs()
        {
            double draggedWidth = (_initialTabIndex >= 0 && _initialTabIndex < _initialTabWidths.Count)
                ? _initialTabWidths[_initialTabIndex]
                : 100;

            for (int i = 0; i < MainTabControl.Items.Count; i++)
            {
                if (i == _initialTabIndex) continue;

                if (MainTabControl.Items[i] is TabItem neighbor && neighbor.RenderTransform is TranslateTransform trans)
                {
                    double targetOffset = 0;

                    if (_currentTargetIndex > _initialTabIndex)
                    {
                        // Kéo sang phải -> các tab ở giữa trượt mượt mà sang TRÁI
                        if (i > _initialTabIndex && i <= _currentTargetIndex)
                        {
                            targetOffset = -draggedWidth;
                        }
                    }
                    else if (_currentTargetIndex < _initialTabIndex)
                    {
                        // Kéo sang trái -> các tab ở giữa trượt mượt mà sang PHẢI
                        if (i >= _currentTargetIndex && i < _initialTabIndex)
                        {
                            targetOffset = draggedWidth;
                        }
                    }

                    var anim = new DoubleAnimation
                    {
                        To = targetOffset,
                        Duration = TimeSpan.FromMilliseconds(140),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    trans.BeginAnimation(TranslateTransform.XProperty, anim);
                }
            }
        }

        private void TabItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FinishTabDrag();
        }

        private void FinishTabDrag()
        {
            if (_draggedTab == null) return;

            var tabToFinish = _draggedTab;
            int initialIdx = _initialTabIndex;
            int targetIdx = _currentTargetIndex;
            bool wasDragging = _isDraggingTab;
            if (tabToFinish.IsMouseCaptured)
            {
                tabToFinish.ReleaseMouseCapture();
            }
            _draggedTab = null;
            _isDraggingTab = false;

            if (wasDragging && initialIdx != targetIdx && initialIdx >= 0 && targetIdx >= 0 &&
                initialIdx < MainTabControl.Items.Count && targetIdx < MainTabControl.Items.Count)
            {
                double targetSnapX = (_initialTabLefts.Count > targetIdx && _initialTabLefts.Count > initialIdx)
                    ? (_initialTabLefts[targetIdx] - _initialTabLefts[initialIdx])
                    : 0;
                if (targetIdx > initialIdx && _initialTabWidths.Count > targetIdx && _initialTabWidths.Count > initialIdx)
                {
                    targetSnapX = _initialTabLefts[targetIdx] + _initialTabWidths[targetIdx] - _initialTabWidths[initialIdx] - _initialTabLefts[initialIdx];
                }

                if (tabToFinish.RenderTransform is TranslateTransform draggedTrans)
                {
                    var snapAnim = new DoubleAnimation
                    {
                        To = targetSnapX,
                        Duration = TimeSpan.FromMilliseconds(130),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    snapAnim.Completed += (s, ev) =>
                    {
                        foreach (TabItem t in MainTabControl.Items)
                        {
                            if (t.RenderTransform is TranslateTransform tr)
                            {
                                tr.BeginAnimation(TranslateTransform.XProperty, null);
                                tr.X = 0;
                            }
                        }

                        MainTabControl.Items.RemoveAt(initialIdx);
                        MainTabControl.Items.Insert(targetIdx, tabToFinish);
                        tabToFinish.IsSelected = true;
                        MainTabControl.SelectedItem = tabToFinish;

                        Panel.SetZIndex(tabToFinish, 0);
                        tabToFinish.Opacity = 1.0;
                    };

                    draggedTrans.BeginAnimation(TranslateTransform.XProperty, snapAnim);
                    return;
                }
            }

            foreach (TabItem t in MainTabControl.Items)
            {
                if (t.RenderTransform is TranslateTransform tr)
                {
                    tr.BeginAnimation(TranslateTransform.XProperty, null);
                    tr.X = 0;
                }
            }
            Panel.SetZIndex(tabToFinish, 0);
            tabToFinish.Opacity = 1.0;

            if (!wasDragging)
            {
                tabToFinish.IsSelected = true;
                MainTabControl.SelectedItem = tabToFinish;
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typed) return typed;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static bool IsDescendantOf(DependencyObject child, DependencyObject ancestor)
        {
            if (child == null || ancestor == null) return false;
            DependencyObject current = child;
            while (current != null)
            {
                if (current == ancestor) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }
        #endregion
    }
}
