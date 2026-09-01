using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
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

            // Mặc định chọn dòng HIHI
            var defaultDb = Databases.FirstOrDefault(d => string.Equals(d.Name, "HIHI", StringComparison.OrdinalIgnoreCase))
                            ?? Databases.FirstOrDefault(d => string.Equals(d.Name, QuanLyBar.Client.Services.DbConnectionManager.CurrentConfig?.Name, StringComparison.OrdinalIgnoreCase))
                            ?? Databases.FirstOrDefault();

            if (defaultDb != null)
            {
                dgDatabases.SelectedItem = defaultDb;
                dgDatabases.ScrollIntoView(defaultDb);
            }
        }

        private void LoadDatabases()
        {
            try
            {
                if (File.Exists(DATA_FILE))
                {
                    string json = File.ReadAllText(DATA_FILE);
                    var list = JsonSerializer.Deserialize<ObservableCollection<DatabaseInfo>>(json);
                    if (list != null) Databases = list;
                }
                else
                {
                    Databases = new ObservableCollection<DatabaseInfo>();
                }
            }
            catch
            {
                Databases = new ObservableCollection<DatabaseInfo>();
            }

            if (Databases.Count == 0)
            {
                // Mặc định nạp CSDL DEMO và HIHI nếu chưa có danh sách
                Databases.Add(new DatabaseInfo
                {
                    Name = "DEMO",
                    Path = @"D:\taifirebird\DEMO.FDB",
                    ConnectionType = 2,
                    Server = "localhost",
                    Username = "SYSDBA",
                    Password = "masterkey"
                });

                Databases.Add(new DatabaseInfo
                {
                    Name = "HIHI",
                    Path = @"D:\saoluu\HIHI.FDB",
                    ConnectionType = 2,
                    Server = "localhost",
                    Username = "SYSDBA",
                    Password = "masterkey"
                });

                SaveDatabases();
            }
            else
            {
                // Đảm bảo có HIHI trong danh sách nếu file tồn tại
                if (!Databases.Any(d => string.Equals(d.Name, "HIHI", StringComparison.OrdinalIgnoreCase)) && File.Exists(@"D:\saoluu\HIHI.FDB"))
                {
                    Databases.Add(new DatabaseInfo
                    {
                        Name = "HIHI",
                        Path = @"D:\saoluu\HIHI.FDB",
                        ConnectionType = 2,
                        Server = "localhost",
                        Username = "SYSDBA",
                        Password = "masterkey"
                    });
                    SaveDatabases();
                }
            }
        }

        private void SaveDatabases()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Databases, options);
                File.WriteAllText(DATA_FILE, json);
            }
            catch { }
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dbWindow = new DbConnectionWindow();
            
            if (dbWindow.ShowDialog() == true)
            {
                var newDb = dbWindow.ResultData;
                if (newDb != null && !string.IsNullOrEmpty(newDb.Path))
                {
                    Databases.Add(newDb);
                    SaveDatabases();
                    dgDatabases.SelectedIndex = Databases.Count - 1;
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDatabases.SelectedItem is DatabaseInfo selectedDb)
            {
                var dbWindow = new DbConnectionWindow();
                dbWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dữ liệu cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Vui lòng chọn dữ liệu cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgDatabases_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDatabases.SelectedItem is DatabaseInfo selectedDb)
            {
                try
                {
                    QuanLyBar.Client.Services.DbConnectionManager.SaveConfig(selectedDb);
                    Application.Current.Properties["SelectedDbName"] = selectedDb.Name;
                    
                    MessageBox.Show($"Đã chọn CSDL: {selectedDb.Name}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
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
            if (!IsLoaded) return;

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
                string dbName = Path.GetFileNameWithoutExtension(filename);

                try
                {
                    // Copy từ file template nếu có
                    string templatePath = @"D:\taifirebird\new.fdb";
                    if (!File.Exists(templatePath))
                    {
                        templatePath = @"D:\taifirebird\DEMO.FDB";
                    }

                    if (File.Exists(templatePath))
                    {
                        File.Copy(templatePath, filename, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể tạo file CSDL: {ex.Message}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                var newDb = new DatabaseInfo 
                { 
                    Name = dbName, 
                    Path = filename,
                    ConnectionType = 2, // Firebird File
                    Server = "localhost",
                    Username = "SYSDBA",
                    Password = "masterkey"
                };

                Databases.Add(newDb);
                SaveDatabases();
                
                dgDatabases.SelectedIndex = Databases.Count - 1;
                
                MessageBox.Show($"Đã tạo mới cơ sở dữ liệu trắng thành công tại:\n{filename}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            rbOpenExisting.IsChecked = true;
        }
    }
}
