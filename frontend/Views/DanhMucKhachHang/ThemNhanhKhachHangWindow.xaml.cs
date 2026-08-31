using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhanhKhachHangWindow : Window
    {
        private readonly int _groupMode; // 0: Nhóm khách hàng, 1: Nhân viên, 2: Tỉnh thành
        private readonly string _parentId;
        public event Action OnSaved;

        public ThemNhanhKhachHangWindow(int groupMode, string parentId = null)
        {
            InitializeComponent();
            _groupMode = groupMode;
            _parentId = parentId;

            string typeName = "Nhân viên";
            if (_groupMode == 0) typeName = "Nhóm khách hàng";
            else if (_groupMode == 2) typeName = "Tỉnh thành";

            Title = "Thêm nhanh";
            TxtHeaderInstruction.Text = $"Mời bạn điền danh sách {typeName} vào ô phía dưới, mỗi dòng một {typeName}";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var service = new LocalKhachDatHangService();
                var images = await service.GetSImagesAsync();
                CmbImage.ItemsSource = images;
                if (images != null && images.Count > 0)
                {
                    CmbImage.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading SImages: " + ex.Message);
            }

            TxtDanhSach.Focus();
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDanhSach.Text))
            {
                MessageBox.Show("Vui lòng nhập ít nhất một dòng tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var lines = TxtDanhSach.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .ToList();

            if (lines.Count == 0) return;

            string selectedImageId = null;
            if (CmbImage.SelectedItem is SImageViewModel selectedImg)
            {
                selectedImageId = selectedImg.Id;
            }

            int count = 0;
            foreach (var line in lines)
            {
                string newId = Guid.NewGuid().ToString();
                if (_groupMode == 1) // Nhân viên
                {
                    bool ok = await LocalKhachHangService.SaveNhanVienAsync(newId, line, "", "", 0, 0, 0, 1, 0, "", true, _parentId);
                    if (ok) count++;
                }
                else if (_groupMode == 0) // Nhóm khách hàng
                {
                    bool ok = await LocalKhachHangService.SaveNhomKhachHangAsync(newId, line, 0, 0, 0, 0, 0, 0, "", true, _parentId);
                    if (ok) count++;
                }
                else if (_groupMode == 2) // Tỉnh thành
                {
                    bool ok = await LocalKhachHangService.SaveTinhThanhAsync(newId, line, "", true, _parentId);
                    if (ok) count++;
                }
            }

            OnSaved?.Invoke();
            DialogResult = true;
            Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
