using System;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemDonViTinhWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private DDONVITINH _editingItem;

        public ThemDonViTinhWindow(DDONVITINH itemToEdit = null, string initialName = "")
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _editingItem = itemToEdit;

            if (_editingItem != null)
            {
                this.Title = "Chỉnh sửa đơn vị tính";
                TxtTenDonViTinh.Text = _editingItem.Name;
            }
            else
            {
                this.Title = "Thêm đơn vị tính";
                if (!string.IsNullOrEmpty(initialName))
                {
                    TxtTenDonViTinh.Text = initialName;
                }
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTenDonViTinh.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đơn vị tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = false;
            if (_editingItem != null)
            {
                // Gọi API update (nếu có), tạm thời chưa làm
                MessageBox.Show("Chức năng cập nhật chưa được kích hoạt.", "Thông báo");
                return;
            }
            else
            {
                var newDvt = new DDONVITINH
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = TxtTenDonViTinh.Text.Trim()
                };
                success = await _matHangService.InsertDonViTinhAsync(newDvt);
            }

            if (success)
            {
                this.DialogResult = true;
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
