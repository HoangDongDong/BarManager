using System;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemLoaiMatHangWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private DLOAIMATHANG _editingItem;

        public ThemLoaiMatHangWindow(DLOAIMATHANG itemToEdit = null, string initialName = "")
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _editingItem = itemToEdit;

            if (_editingItem != null)
            {
                this.Title = "Chỉnh sửa loại mặt hàng";
                TxtTenLoai.Text = _editingItem.Name;
                TxtGhiChu.Text = _editingItem.Note;
            }
            else
            {
                this.Title = "Thêm loại mặt hàng";
                if (!string.IsNullOrEmpty(initialName))
                {
                    TxtTenLoai.Text = initialName;
                }
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTenLoai.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loai = new DLOAIMATHANG
            {
                Name = TxtTenLoai.Text.Trim(),
                Note = TxtGhiChu.Text.Trim()
            };

            bool success = await _matHangService.InsertLoaiMatHangAsync(loai);
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
