using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
using QuanLyBar.Client.Models;

    public partial class TongHopKqkdControl : UserControl
    {
        private readonly QuanLyBar.Client.Services.LocalHoaDonService _hoaDonService;

        public TongHopKqkdControl()
        {
            InitializeComponent();
            _hoaDonService = new QuanLyBar.Client.Services.LocalHoaDonService();
            this.Loaded += TongHopKqkdControl_Loaded;
        }

        private async void TongHopKqkdControl_Loaded(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpDenNgay.SelectedDate = DateTime.Now;
            await LoadDataAsync();
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
                
                var list = await _hoaDonService.GetTongHopKqkdAsync(tuNgay, denNgay);
                DgKqkd.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
