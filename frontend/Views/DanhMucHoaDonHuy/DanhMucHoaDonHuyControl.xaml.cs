using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;
using System.Threading.Tasks;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucHoaDonHuyControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;

        public DanhMucHoaDonHuyControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = DateTime.Now;
            dpDenNgay.SelectedDate = DateTime.Now;
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Now;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Now;
                
                var list = await _hoaDonService.GetHoaDonHuyListAsync(tuNgay, denNgay);
                DgHoaDonHuy.ItemsSource = list;
                
                // Clear chi tiết
                DgChiTiet.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục hóa đơn hủy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgHoaDonHuy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgHoaDonHuy.SelectedItem is HoaDonHuyViewModel selectedHoaDonHuy)
            {
                try
                {
                    var chiTietList = await _hoaDonService.GetChiTietHoaDonHuyAsync(selectedHoaDonHuy.Id);
                    DgChiTiet.ItemsSource = chiTietList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết hóa đơn hủy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
