using System;
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
                this.Title = $"Phần Mềm Quản Lý Bar, Nhà Hàng - [{userInfoStr}]";
                
                AddTab("Sử dụng dịch vụ", new QuanLyBar.Client.Views.SuDungDichVuControl());
                AddTab("Danh mục khách hàng", new QuanLyBar.Client.Views.DanhMucKhachHangControl());
            }
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

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
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
                }
            }
        }

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
    }
}
