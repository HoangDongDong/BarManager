using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.ThuVienAnh
{
    public partial class ThemAnhVaoThuVienWindow : Window
    {
        private byte[] _imageBytes = null;
        private string _fileName = "";

        public bool IsSaved { get; private set; } = false;
        public string SavedId { get; private set; } = null;

        public ThemAnhVaoThuVienWindow(string initialCategory = null, string initialFilePath = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(initialCategory))
            {
                CboNhomAnh.Text = initialCategory;
            }
            else
            {
                CboNhomAnh.SelectedIndex = 0; // Biểu tượng
            }

            if (!string.IsNullOrEmpty(initialFilePath) && File.Exists(initialFilePath))
            {
                LoadImageFromFile(initialFilePath);
            }
        }

        private void LoadImageFromFile(string filePath)
        {
            try
            {
                _imageBytes = File.ReadAllBytes(filePath);
                _fileName = Path.GetFileNameWithoutExtension(filePath);
                TxtFileName.Text = Path.GetFileName(filePath);
                ImgPreview.Source = LocalThuVienAnhService.BytesToBitmapImage(_imageBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải tệp ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Chọn tệp hình ảnh",
                Filter = "Tệp hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif|Tất cả tệp (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) == true)
            {
                LoadImageFromFile(ofd.FileName);
            }
        }

        private async void BtnDongY_Click(object sender, RoutedEventArgs e)
        {
            if (_imageBytes == null || _imageBytes.Length == 0)
            {
                MessageBox.Show("Vui lòng chọn một tệp hình ảnh!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                BtnBrowse_Click(null, null);
                return;
            }

            string category = CboNhomAnh.Text?.Trim();
            if (string.IsNullOrEmpty(category))
            {
                category = "Khác";
            }

            string imageName = !string.IsNullOrEmpty(_fileName) ? _fileName : category;

            var (ok, error, newId) = await LocalThuVienAnhService.AddImageAsync(imageName, category, _imageBytes);
            if (ok)
            {
                IsSaved = true;
                SavedId = newId;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Lỗi thêm ảnh vào thư viện: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBoQua_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
