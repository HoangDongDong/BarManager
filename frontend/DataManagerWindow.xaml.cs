using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyBar.Client
{
    public class DatabaseInfo
    {
        public string Name { get; set; }
        public string Path { get; set; } // Database path or name
        public int ConnectionType { get; set; } // 0: Firebird Server, 1: SQL Server, 2: Firebird Embedded (File)
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public partial class DataManagerWindow : Window
    {
        public ObservableCollection<DatabaseInfo> Databases { get; set; }

        private const string DATA_FILE = "databases.json";

        public DataManagerWindow()
        {
            InitializeComponent();
            LoadDatabases();
            dgDatabases.ItemsSource = Databases;
        }

        private void LoadDatabases()
        {
            try
            {
                if (System.IO.File.Exists(DATA_FILE))
                {
                    string json = System.IO.File.ReadAllText(DATA_FILE);
                    var list = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<DatabaseInfo>>(json);
                    if (list != null) Databases = list;
                }
                else
                {
                    // Fallback mặc định nếu chưa có file
                    Databases = new ObservableCollection<DatabaseInfo>();
                }
            }
            catch
            {
                Databases = new ObservableCollection<DatabaseInfo>();
            }
        }

        private void SaveDatabases()
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(Databases, options);
                System.IO.File.WriteAllText(DATA_FILE, json);
            }
            catch { }
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ cấu hình kết nối thay vì mở trực tiếp file dialog
            var dbWindow = new DbConnectionWindow();
            
            if (dbWindow.ShowDialog() == true)
            {
                var newDb = dbWindow.ResultData;
                if (newDb != null && !string.IsNullOrEmpty(newDb.Path))
                {
                    // Thêm vào danh sách hiển thị
                    Databases.Add(newDb);
                    SaveDatabases();
                    
                    // Tự động chọn dòng vừa thêm
                    dgDatabases.SelectedIndex = Databases.Count - 1;
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDatabases.SelectedItem is DatabaseInfo selectedDb)
            {
                var dbWindow = new DbConnectionWindow();
                // Set data to dbWindow if needed
                dbWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dữ liệu cần sửa.", "Thông báo");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDatabases.SelectedItem is DatabaseInfo selectedDb)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{selectedDb.Name}' khỏi danh sách?", 
                                             "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Databases.Remove(selectedDb);
                    SaveDatabases();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dữ liệu cần xóa.", "Thông báo");
            }
        }

        private void DgDatabases_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDatabases.SelectedItem is DatabaseInfo selectedDb)
            {
                try
                {
                    // Lưu cấu hình trực tiếp vào DbConnectionManager (không cần gọi Backend API nữa)
                    QuanLyBar.Client.Services.DbConnectionManager.SaveConfig(selectedDb);
                    
                    Application.Current.Properties["SelectedDbName"] = selectedDb.Name;
                    
                    MessageBox.Show($"Đã kết nối tới CSDL: {selectedDb.Name}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void RbCreateNew_Checked(object sender, RoutedEventArgs e)
        {
            // Tránh trigger khi khởi tạo UI chưa xong
            if (!IsLoaded) return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Database file (*.fdb)|*.fdb|All files (*.*)|*.*",
                Title = "Tạo mới cơ sở dữ liệu",
                DefaultExt = ".fdb"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                string dbName = System.IO.Path.GetFileNameWithoutExtension(filename);

                // TODO: Gọi API Backend để copy/tạo mới file FDB trắng tại đường dẫn này
                // Tạm thời chỉ lưu cấu hình đường dẫn mới
                var newDb = new DatabaseInfo 
                { 
                    Name = dbName, 
                    Path = filename,
                    ConnectionType = 2 // Firebird File
                };

                Databases.Add(newDb);
                SaveDatabases();
                
                dgDatabases.SelectedIndex = Databases.Count - 1;
                
                MessageBox.Show($"Đã cấu hình đường dẫn tạo mới CSDL: {filename}\n(Ghi chú: Cần hoàn thiện API Backend để gen cấu trúc bảng trắng vào file này)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Dù có chọn hay Cancel thì cũng chuyển RadioButton về lại chế độ Mở danh sách
            rbOpenExisting.IsChecked = true;
        }
    }
}
