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
                AddTab("Sử dụng dịch vụ", new System.Windows.Controls.TextBlock 
                { 
                    Text = "(Phần nội dung chính bên dưới sẽ được thiết kế ở các bước tiếp theo)\n\n" + userInfoStr, 
                    FontSize = 16, 
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    VerticalAlignment = VerticalAlignment.Center 
                });
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
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                string tabName = button.Content.ToString();
                
                // Demo tạo nội dung ảo cho các Tab để thấy sự khác biệt
                var content = new System.Windows.Controls.TextBlock
                {
                    Text = $"Nội dung của màn hình: {tabName}\n(Đang tải từ file UserControl...)",
                    FontSize = 18,
                    Foreground = System.Windows.Media.Brushes.DarkSlateGray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

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
