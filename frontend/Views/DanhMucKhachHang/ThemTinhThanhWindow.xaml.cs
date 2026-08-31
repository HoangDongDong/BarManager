using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemTinhThanhWindow : Window
    {
        private string _id;
        private string _parentId;
        private bool _isNew = true;
        public event Action OnSaved;

        public ThemTinhThanhWindow(string id = null, string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _parentId = parentId;
            _isNew = string.IsNullOrEmpty(id);

            Loaded += ThemTinhThanhWindow_Loaded;
        }

        private List<SImageViewModel> _images = new();

        private async void ThemTinhThanhWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var service = new LocalKhachDatHangService();
                _images = await service.GetSImagesAsync();
                CboAnh.ItemsSource = _images;
                if (_images.Count > 0) CboAnh.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading SImages in ThemTinhThanhWindow: " + ex.Message);
            }

            if (!_isNew)
            {
                TxtHeaderTitle.Text = "Chỉnh sửa tỉnh thành";
                this.Title = "Tỉnh thành - Chỉnh sửa";

                var data = await LocalKhachHangService.GetTinhThanhByIdAsync(_id);
                if (data != null)
                {
                    TxtTenTinhThanh.Text = data.NAME?.ToString() ?? "";
                    string simageId = data.SIMAGEID?.ToString() ?? data.IMAGE?.ToString();
                    if (!string.IsNullOrEmpty(simageId) && _images != null)
                    {
                        var matched = _images.FirstOrDefault(x => x.Id == simageId);
                        if (matched != null) CboAnh.SelectedItem = matched;
                    }
                    TxtGhiChu.Text = data.NOTE?.ToString() ?? "";
                }
            }
            else
            {
                TxtHeaderTitle.Text = "Thêm mới tỉnh thành";
                this.Title = "Tỉnh thành - Thêm mới";
                TxtTenTinhThanh.Focus();
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtTenTinhThanh.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập Tên tỉnh thành!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenTinhThanh.Focus();
                return false;
            }

            string note = TxtGhiChu.Text.Trim();

            bool ok = await LocalKhachHangService.SaveTinhThanhAsync(_id, name, note, _isNew, _parentId);
            if (ok)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Không thể lưu tỉnh thành. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                MessageBox.Show("Lưu tỉnh thành thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
