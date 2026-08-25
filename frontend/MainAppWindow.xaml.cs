using System.Windows;

namespace QuanLyBar.Client
{
    public partial class MainAppWindow : Window
    {
        public MainAppWindow()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            if (SessionContext.CurrentUser != null)
            {
                var userInfoStr = $"Nhân viên: {SessionContext.CurrentUser.TenDangNhap} | Vai trò: {SessionContext.CurrentUser.VaiTro}";
                // Create a welcome tab or set title
                this.Title = $"Phần Mềm Quản Lý Bar, Nhà Hàng - [{userInfoStr}]";
                
                // Add initial tab
                AddTab("Sử dụng dịch vụ", new QuanLyBar.Client.Views.SuDungDichVuControl());
            }
        }

        public void AddTab(string header, UIElement content)
        {
            // Kiểm tra xem tab đã tồn tại chưa
            foreach (System.Windows.Controls.TabItem tab in MainTabControl.Items)
            {
                if (tab.Header.ToString() == header)
                {
                    MainTabControl.SelectedItem = tab;
                    return; // Nếu có rồi thì focus vào nó
                }
            }

            // Tạo tab mới
            var newTab = new System.Windows.Controls.TabItem
            {
                Header = header,
                Content = new System.Windows.Controls.Border
                {
                    Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#eef3f9")),
                    Child = content
                }
            };

            MainTabControl.Items.Add(newTab);
            MainTabControl.SelectedItem = newTab;
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            string tabName = string.Empty;

            if (sender is System.Windows.Controls.Button button)
            {
                tabName = button.Content?.ToString();
            }
            else if (sender is System.Windows.Controls.MenuItem menuItem)
            {
                tabName = menuItem.Header?.ToString();
            }

            if (!string.IsNullOrEmpty(tabName))
            {
                // Loại bỏ phần phím tắt nếu bị dính (vd: "Danh mục mặt hàng Ctrl+M" -> "Danh mục mặt hàng")
                if (tabName.Contains("Ctrl+"))
                {
                    tabName = tabName.Substring(0, tabName.IndexOf("Ctrl+")).Trim();
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
                else if (tabName == "Tổng hợp KQKD" || tabName == "Tổng hợp kết quả kinh doanh")
                {
                    content = new QuanLyBar.Client.Views.TongHopKqkdControl();
                }
                else if (tabName == "Chi tiết hoạt động ngày" || tabName == "Chi tiết hoạt động")
                {
                    content = new QuanLyBar.Client.Views.ChiTietHoatDongControl();
                }
                else if (tabName == "Danh mục hóa đơn hủy")
                {
                    content = new QuanLyBar.Client.Views.DanhMucHoaDonHuyControl();
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

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                // Find the parent TabItem
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(button);
                while (parent != null && !(parent is System.Windows.Controls.TabItem))
                {
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                }

                if (parent is System.Windows.Controls.TabItem tabItem)
                {
                    MainTabControl.Items.Remove(tabItem);
                }
            }
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
    }
}
