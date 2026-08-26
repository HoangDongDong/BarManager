using System;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using System.Collections.Generic;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucDonViTinhWindow : Window
    {
        private readonly LocalMatHangService _matHangService;

        public DanhMucDonViTinhWindow()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            this.Loaded += DanhMucDonViTinhWindow_Loaded;
        }

        private async void DanhMucDonViTinhWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var dvtList = await _matHangService.GetDonViTinhListAsync();
                LvDonViTinh.ItemsSource = dvtList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void BtnTaiLai_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadData();
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemDonViTinhWindow();
            if (win.ShowDialog() == true)
            {
                _ = LoadData();
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (LvDonViTinh.SelectedItem is DDONVITINH selectedDvt)
            {
                var win = new ThemDonViTinhWindow(selectedDvt);
                if (win.ShowDialog() == true)
                {
                    _ = LoadData();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một đơn vị tính để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (LvDonViTinh.SelectedItem is DDONVITINH selectedDvt)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa đơn vị tính '{selectedDvt.Name}' không?", 
                                             "Xác nhận xóa", 
                                             MessageBoxButton.YesNo, 
                                             MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _matHangService.DeleteDonViTinhAsync(selectedDvt.Id);
                    if (success)
                    {
                        _ = LoadData();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một đơn vị tính để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
