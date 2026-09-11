using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtUsername.Text = "admin";
            txtPassword.Password = "";

            var currentDb = DbConnectionManager.CurrentConfig;
            if (currentDb != null && !string.IsNullOrEmpty(currentDb.Name))
            {
                lblDbName.Text = currentDb.Name;
            }

            Loaded += MainWindow_Loaded;
            btnLogin.Focus();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCompanyLogoAsync();
        }

        private async Task LoadCompanyLogoAsync()
        {
            try
            {
                var comp = await LocalCauHinhService.GetCompanyInfoAsync();
                if (comp != null && comp.LogoBytes != null && comp.LogoBytes.Length > 0)
                {
                    var bi = LocalCauHinhService.ImageFromBytes(comp.LogoBytes);
                    if (bi != null)
                    {
                        imgLogo.Source = bi;
                        return;
                    }
                }
            }
            catch { }

            try
            {
                imgLogo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/img/logo.png", UriKind.Absolute));
            }
            catch { }
        }

        private async void BorderLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var ofd = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                    Title = "Chọn logo quán"
                };

                if (ofd.ShowDialog() == true)
                {
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    var bi = LocalCauHinhService.ImageFromBytes(bytes);
                    if (bi != null)
                    {
                        imgLogo.Source = bi;

                        // Lưu vào CSDL
                        await LocalCauHinhService.SaveAllConfigsAsync(
                            new System.Collections.Generic.Dictionary<string, string>(),
                            new System.Collections.Generic.Dictionary<string, string>(),
                            bytes);

                        MessageBox.Show("Cập nhật logo quán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chọn logo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password ?? "";

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập Tên đăng nhập!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }

            btnLogin.IsEnabled = false;
            btnLogin.Content = "ĐANG ĐĂNG NHẬP...";

            try
            {
                var user = await LocalAuthService.LoginAsync(username, password);

                if (user != null)
                {
                    // Lưu thông tin vào Session (không cần Token nữa, chỉ cần User)
                    SessionContext.CurrentToken = "local_direct_db_token"; 
                    SessionContext.CurrentUser = user;

                    // Chuyển sang màn hình chính
                    var mainApp = new MainAppWindow();
                    mainApp.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Đăng nhập";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var dataManagerWindow = new DataManagerWindow();
            dataManagerWindow.ShowDialog();
            
            if (Application.Current.Properties.Contains("SelectedDbName"))
            {
                lblDbName.Text = Application.Current.Properties["SelectedDbName"].ToString();
            }

            await LoadCompanyLogoAsync();
        }
    }
}