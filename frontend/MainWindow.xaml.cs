using System;
using System.Windows;
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
            txtPassword.Password = "admin123";
            btnLogin.Focus();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var dataManagerWindow = new DataManagerWindow();
            dataManagerWindow.ShowDialog();
            
            if (Application.Current.Properties.Contains("SelectedDbName"))
            {
                lblDbName.Text = Application.Current.Properties["SelectedDbName"].ToString();
            }
        }
    }
}