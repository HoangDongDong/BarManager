using System;
using System.Windows;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhomWindow : Window
    {
        public string TenNhom => TxtTenNhom.Text.Trim();
        public string MaSanPham => TxtMaSanPham.Text.Trim();
        public int? LoaiDoId => CboLoaiDo.SelectedValue as int?;

        private bool _isThuMuc;

        public ThemNhomWindow(bool isThuMuc = false)
        {
            InitializeComponent();
            _isThuMuc = isThuMuc;
            
            if (_isThuMuc)
            {
                this.Title = "THƯ MỤC - THÊM MỚI";
            }
            else
            {
                this.Title = "NHÓM MẶT HÀNG - THÊM MỚI";
            }
            TxtTenNhom.Focus();
            
            // Dummy data for CboLoaiDo for now
            CboLoaiDo.ItemsSource = new[] 
            { 
                new { Id = 1, Name = "Dịch vụ" },
                new { Id = 2, Name = "Đồ ăn" },
                new { Id = 3, Name = "Đồ khác" },
                new { Id = 4, Name = "Đồ uống" },
                new { Id = 5, Name = "Nguyên liệu" }
            };
            CboLoaiDo.DisplayMemberPath = "Name";
            CboLoaiDo.SelectedValuePath = "Id";
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TenNhom))
            {
                MessageBox.Show("Vui lòng nhập tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
