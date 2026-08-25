using System.Windows;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client
{
    public partial class DbConnectionWindow : Window
    {
        public DbConnectionWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Firebird Database (*.fdb)|*.fdb|All files (*.*)|*.*";
            openFileDialog.Title = "Chọn file cơ sở dữ liệu";

            if (openFileDialog.ShowDialog() == true)
            {
                txtDatabase.Text = openFileDialog.FileName;
            }
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var testConfig = new DatabaseInfo
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(txtDatabase.Text),
                Path = txtDatabase.Text,
                ConnectionType = rbFile.IsChecked == true ? 2 : (rbSqlServer.IsChecked == true ? 1 : 0),
                Server = txtServer.Text,
                Username = txtUsername.Text,
                Password = txtPassword.Password
            };

            if (DbConnectionManager.TestConnection(testConfig, out string error))
            {
                MessageBox.Show("Kết nối thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Kết nối thất bại:\n" + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public DatabaseInfo ResultData { get; private set; }

        private void RbConnectionType_Checked(object sender, RoutedEventArgs e)
        {
            if (txtServer == null) return; // UI chưa khởi tạo xong

            if (rbFile.IsChecked == true)
            {
                txtServer.IsEnabled = false;
                btnSelectFile.IsEnabled = true;
            }
            else
            {
                txtServer.IsEnabled = true;
                btnSelectFile.IsEnabled = false;
                if (rbSqlServer.IsChecked == true)
                {
                    txtUsername.Text = "sa";
                }
                else if (rbFirebirdServer.IsChecked == true)
                {
                    txtUsername.Text = "SYSDBA";
                }
            }
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDatabase.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập cơ sở dữ liệu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            ResultData = new DatabaseInfo
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(txtDatabase.Text),
                Path = txtDatabase.Text,
                ConnectionType = rbFile.IsChecked == true ? 2 : (rbSqlServer.IsChecked == true ? 1 : 0),
                Server = txtServer.Text,
                Username = txtUsername.Text,
                Password = txtPassword.Password
            };

            if (string.IsNullOrEmpty(ResultData.Name))
                ResultData.Name = txtDatabase.Text;

            DbConnectionManager.SaveConfig(ResultData);

            MessageBox.Show("Đã lưu thông tin cấu hình!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
