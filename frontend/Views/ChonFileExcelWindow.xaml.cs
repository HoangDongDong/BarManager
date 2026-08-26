using System;
using System.IO;
using System.Windows;

namespace QuanLyBar.Client.Views
{
    public partial class ChonFileExcelWindow : Window
    {
        public string[] SelectedFilePaths { get; private set; }

        public ChonFileExcelWindow()
        {
            InitializeComponent();
        }

        private void BtnXuatHienThi_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xls;*.xlsx";
            saveFileDialog.FileName = "cac cot hien thi trong danh sach.xls";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "cac cot hien thi trong danh sach.xls");
                    // Tạm thời lấy đường dẫn tuyệt đối vì app đang chạy trong frontend/bin/Debug/net9.0-windows/
                    templatePath = @"d:\QuanLyBar\cac cot hien thi trong danh sach.xls";
                    
                    if (System.IO.File.Exists(templatePath))
                    {
                        System.IO.File.Copy(templatePath, saveFileDialog.FileName, true);
                        MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file mẫu gốc tại: " + templatePath, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnXuatTatCa_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xls;*.xlsx";
            saveFileDialog.FileName = "all cot.xls";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string templatePath = @"d:\QuanLyBar\all cot.xls";
                    
                    if (System.IO.File.Exists(templatePath))
                    {
                        System.IO.File.Copy(templatePath, saveFileDialog.FileName, true);
                        MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file mẫu gốc tại: " + templatePath, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnChonFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx|All Files|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePaths = openFileDialog.FileNames;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
