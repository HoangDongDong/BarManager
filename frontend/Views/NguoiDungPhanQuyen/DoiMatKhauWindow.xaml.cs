using System;
using System.Threading.Tasks;
using System.Windows;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Data.SqlClient;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NguoiDungPhanQuyen
{
    public partial class DoiMatKhauWindow : Window
    {
        private readonly UserProfile _currentUser;

        public DoiMatKhauWindow()
        {
            InitializeComponent();

            _currentUser = SessionContext.CurrentUser ?? new UserProfile
            {
                Id = "1",
                TenDangNhap = "Admin",
                TenHienThi = "Admin"
            };

            TxtHeaderAccount.Text = $"Đổi mật khẩu cho tài khoản: {_currentUser.TenDangNhap}";
            Loaded += (s, e) => TxtOldPassword.Focus();
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = TxtOldPassword.Password ?? "";
            string newPassword = TxtNewPassword.Password ?? "";
            string confirmPassword = TxtConfirmPassword.Password ?? "";

            // 1. Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu mới và Nhập lại không khớp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtConfirmPassword.Focus();
                TxtConfirmPassword.SelectAll();
                return;
            }

            // 2. Nếu mật khẩu mới để trống -> hiển thị hộp thoại cảnh báo như mẫu
            if (string.IsNullOrEmpty(newPassword))
            {
                var confirmBlank = MessageBox.Show(
                    "Mật khẩu mới của bạn để trống, chúng tôi khuyên bạn không nên dùng mật khẩu này\nBạn có muốn tiếp tục không?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmBlank != MessageBoxResult.Yes)
                {
                    TxtNewPassword.Focus();
                    return;
                }
            }

            BtnGhiDuLieu.IsEnabled = false;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Tìm thông tin tài khoản trong SUSER
                    string sqlFind = conn is FbConnection
                        ? "SELECT FIRST 1 ID, USERNAME, PASSWORD FROM SUSER WHERE UPPER(TRIM(USERNAME)) = UPPER(TRIM(@Username)) OR CAST(ID AS VARCHAR(50)) = @UserId"
                        : "SELECT TOP 1 ID, USERNAME, PASSWORD FROM SUSER WHERE UPPER(TRIM(USERNAME)) = UPPER(TRIM(@Username)) OR CAST(ID AS VARCHAR(50)) = @UserId";

                    var userRow = await conn.QueryFirstOrDefaultAsync(sqlFind, new 
                    { 
                        Username = _currentUser.TenDangNhap?.Trim() ?? "admin", 
                        UserId = _currentUser.Id ?? "1" 
                    });

                    if (userRow == null)
                    {
                        MessageBox.Show("Không tìm thấy thông tin tài khoản người dùng trong cơ sở dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string dbPassword = userRow.PASSWORD?.ToString() ?? "";
                    object actualUserId = userRow.ID;

                    // 3. Kiểm tra mật khẩu cũ (cho phép để trống nếu mật khẩu hiện tại trong DB cũng trống)
                    bool isOldPasswordCorrect = false;
                    if (string.IsNullOrWhiteSpace(dbPassword))
                    {
                        // Mật khẩu hiện tại trong CSDL đang trống -> chấp nhận để trống hoặc nhập admin123
                        isOldPasswordCorrect = string.IsNullOrEmpty(oldPassword) || oldPassword == "admin123";
                    }
                    else if (dbPassword == "admin123")
                    {
                        isOldPasswordCorrect = (oldPassword == "admin123" || string.IsNullOrEmpty(oldPassword) || oldPassword == dbPassword);
                    }
                    else
                    {
                        try
                        {
                            isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(oldPassword, dbPassword);
                        }
                        catch
                        {
                            isOldPasswordCorrect = (oldPassword == dbPassword);
                        }
                    }

                    if (!isOldPasswordCorrect)
                    {
                        MessageBox.Show("Mật khẩu cũ không chính xác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtOldPassword.Focus();
                        TxtOldPassword.SelectAll();
                        return;
                    }

                    // 4. Cập nhật mật khẩu mới vào SUSER
                    string sqlUpdate = conn is FbConnection
                        ? "UPDATE SUSER SET PASSWORD = @Password, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id"
                        : "UPDATE SUSER SET PASSWORD = @Password, TIMEMODIFIED = GETDATE() WHERE ID = @Id";

                    await conn.ExecuteAsync(sqlUpdate, new 
                    { 
                        Password = newPassword, 
                        Id = actualUserId 
                    });

                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đổi mật khẩu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGhiDuLieu.IsEnabled = true;
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
