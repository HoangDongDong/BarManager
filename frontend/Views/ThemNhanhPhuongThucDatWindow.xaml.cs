using System;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhanhPhuongThucDatWindow : Window
    {
        private readonly LocalKhachDatHangService _service;
        private readonly string _parentId;
        private readonly bool _isMucDichDat;

        public ThemNhanhPhuongThucDatWindow(string parentId, bool isMucDichDat)
        {
            InitializeComponent();
            _service = new LocalKhachDatHangService();
            _parentId = parentId;
            _isMucDichDat = isMucDichDat;

            string titlePrefix = _isMucDichDat ? "Mục đích đặt" : "Phương thức đặt";
            this.Title = "Thêm nhanh " + titlePrefix;
            TxtHeader.Text = $"Mời bạn điền danh sách {titlePrefix} vào ô phía dưới, mỗi dòng một {titlePrefix}";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var images = await _service.GetSImagesAsync();
            CmbImage.ItemsSource = images;
            
            // Set default image to the first one if available
            if (images != null && images.Count > 0)
            {
                CmbImage.SelectedIndex = 0;
            }
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDanhSach.Text))
            {
                MessageBox.Show("Vui lòng nhập danh sách trước khi lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var lines = TxtDanhSach.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .ToList();

            if (lines.Count == 0) return;

            string selectedImageId = null;
            if (CmbImage.SelectedItem is Models.SImageViewModel selectedImg)
            {
                selectedImageId = selectedImg.Id;
            }

            int successCount = 0;
            foreach (var line in lines)
            {
                bool success = await _service.InsertPhuongThucDatAsync(line, "", selectedImageId, _parentId, _isMucDichDat);
                if (success) successCount++;
            }

            MessageBox.Show($"Đã thêm thành công {successCount}/{lines.Count} danh mục.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
