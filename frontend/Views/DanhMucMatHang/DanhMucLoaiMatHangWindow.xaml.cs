using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucLoaiMatHangWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private List<DLOAIMATHANG> _loaiList;
        public Action OnDataChanged { get; set; }

        public DanhMucLoaiMatHangWindow()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _loaiList = await _matHangService.GetLoaiMatHangKhacListAsync();
                DgLoaiMatHang.ItemsSource = _loaiList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message);
            }
        }

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemLoaiMatHangWindow();
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                OnDataChanged?.Invoke();
            }
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgLoaiMatHang.SelectedItem is DLOAIMATHANG selected)
            {
                var win = new ThemLoaiMatHangWindow(selected);
                if (win.ShowDialog() == true)
                {
                    await LoadDataAsync();
                    OnDataChanged?.Invoke();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn loại mặt hàng cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (DgLoaiMatHang.SelectedItem is DLOAIMATHANG selected)
            {
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa loại mặt hàng '{selected.Name}' không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    bool ok = await _matHangService.DeleteLoaiMatHangAsync(selected.Id);
                    if (ok)
                    {
                        await LoadDataAsync();
                        OnDataChanged?.Invoke();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn loại mặt hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnTaiLai_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void DgLoaiMatHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
