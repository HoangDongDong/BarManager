using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class DieuChinhHoaDonControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly LocalSuDungDichVuService _dichVuService;

        public DieuChinhHoaDonControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _dichVuService = new LocalSuDungDichVuService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = DateTime.Now;
            dpDenNgay.SelectedDate = DateTime.Now;
            
            await LoadDataAsync();
            await LoadMenuTreeAsync();
        }

        private async void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Now;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Now;
                
                var list = await _hoaDonService.GetHoaDonListAsync(tuNgay, denNgay);
                DgHoaDon.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMenuTreeAsync()
        {
            try
            {
                var menuTree = await _dichVuService.GetNhomMatHangTreeAsync();
                TvMenu.ItemsSource = menuTree;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is PosNhomMatHangViewModel selectedItem)
            {
                try
                {
                    // If Id is empty, it means "Tất cả"
                    string searchNhomId = string.IsNullOrEmpty(selectedItem.Id) ? null : selectedItem.Id;
                    var matHangList = await _dichVuService.GetMatHangListAsync(searchNhomId);
                    DgMatHang.ItemsSource = matHangList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải danh sách món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void DgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgHoaDon.SelectedItem is HoaDonViewModel selectedHoaDon)
            {
                try
                {
                    if (int.TryParse(selectedHoaDon.Id, out int donHangId))
                    {
                        var chiTietList = await _hoaDonService.GetChiTietHoaDonAsync(donHangId);
                        DgChiTiet.ItemsSource = chiTietList;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
