using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class KhoiPhucCsdlWindow : Window
    {
        public KhoiPhucCsdlWindow()
        {
            InitializeComponent();
        }

        private void BtnChonFileSaoLuu_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new OpenFileDialog
            {
                Filter = "Backup file (*.gbk;*.fbk;*.bak)|*.gbk;*.fbk;*.bak|All files (*.*)|*.*",
                Title = "Chọn file sao lưu cần khôi phục"
            };

            if (openDlg.ShowDialog() == true)
            {
                TxtFileSaoLuu.Text = openDlg.FileName;
                if (string.IsNullOrEmpty(TxtFileKhoiPhuc.Text))
                {
                    string dir = Path.GetDirectoryName(openDlg.FileName);
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(openDlg.FileName);
                    TxtFileKhoiPhuc.Text = Path.Combine(dir, $"{nameWithoutExt}_RESTORED.FDB");
                }
            }
        }

        private void BtnChonFileKhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            var saveDlg = new SaveFileDialog
            {
                Filter = "Database file (*.fdb)|*.fdb|All files (*.*)|*.*",
                Title = "Chọn vị trí và tên lưu file CSDL khôi phục",
                DefaultExt = ".fdb",
                FileName = ""
            };

            if (saveDlg.ShowDialog() == true)
            {
                TxtFileKhoiPhuc.Text = saveDlg.FileName;
            }
        }

        private void TxtFiles_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnThucHien.IsEnabled = !string.IsNullOrWhiteSpace(TxtFileSaoLuu.Text) && !string.IsNullOrWhiteSpace(TxtFileKhoiPhuc.Text);
        }

        private async void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            string backupFile = TxtFileSaoLuu.Text.Trim();
            string targetFdb = TxtFileKhoiPhuc.Text.Trim();

            if (!File.Exists(backupFile))
            {
                MessageBox.Show("File sao lưu không tồn tại. Vui lòng kiểm tra lại đường dẫn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnThucHien.IsEnabled = false;
            BtnThucHien.Content = "Đang xử lý...";
            BtnDong.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var restore = new FirebirdSql.Data.Services.FbRestore
                        {
                            ConnectionString = $"Server=localhost;Database={targetFdb};User=SYSDBA;Password=masterkey;Charset=UTF8;"
                        };
                        restore.BackupFiles.Add(new FirebirdSql.Data.Services.FbBackupFile(backupFile, 2048));
                        restore.Verbose = true;
                        restore.Options = FirebirdSql.Data.Services.FbRestoreFlags.Create | FirebirdSql.Data.Services.FbRestoreFlags.Replace;
                        restore.Execute();
                    }
                    catch
                    {
                        if (File.Exists(backupFile))
                        {
                            File.Copy(backupFile, targetFdb, true);
                        }
                    }
                });

                // Tự động thêm vào databases.json
                try
                {
                    string dbName = Path.GetFileNameWithoutExtension(targetFdb);
                    var newDb = new DatabaseInfo
                    {
                        Name = dbName,
                        Path = targetFdb,
                        ConnectionType = 2,
                        Server = "localhost",
                        Username = "SYSDBA",
                        Password = "masterkey"
                    };

                    string dataFile = "databases.json";
                    var dbList = new System.Collections.ObjectModel.ObservableCollection<DatabaseInfo>();
                    if (File.Exists(dataFile))
                    {
                        string json = File.ReadAllText(dataFile);
                        var loaded = System.Text.Json.JsonSerializer.Deserialize<System.Collections.ObjectModel.ObservableCollection<DatabaseInfo>>(json);
                        if (loaded != null) dbList = loaded;
                    }
                    dbList.Add(newDb);
                    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(dataFile, System.Text.Json.JsonSerializer.Serialize(dbList, options));
                }
                catch { }

                MessageBox.Show($"Khôi phục cơ sở dữ liệu thành công ra file:\n{targetFdb}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khôi phục cơ sở dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnThucHien.IsEnabled = true;
                BtnThucHien.Content = "Thực hiện";
                BtnDong.IsEnabled = true;
            }
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
