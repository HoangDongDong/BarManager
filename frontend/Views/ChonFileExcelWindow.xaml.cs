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
            // Placeholder: User said they will guide excel later
            MessageBox.Show("Sẽ xuất file: cac cot hien thi trong danh sach.xls\n(Chức năng Excel sẽ được hoàn thiện sau theo hướng dẫn)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatTatCa_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder
            MessageBox.Show("Sẽ xuất file: all cot.xls\n(Chức năng Excel sẽ được hoàn thiện sau theo hướng dẫn)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
