using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class KhachDatHangControl : UserControl
    {
        private LocalKhachDatHangService _service;
        private string _currentPhuongThucId = null;

        public KhachDatHangControl()
        {
            InitializeComponent();
            _service = new LocalKhachDatHangService();
            
            // Default filter past month
            DpTuNgay.SelectedDate = DateTime.Now.AddMonths(-1);
            DpDenNgay.SelectedDate = DateTime.Now;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var treeData = await _service.GetPhuongThucDatTreeAsync();
            TvPhuongThucDat.ItemsSource = treeData;

            await LoadData();
        }

        private async void TvPhuongThucDat_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is PhuongThucDatViewModel selectedNode)
            {
                _currentPhuongThucId = selectedNode.Id;
                await LoadData();
            }
        }
        
        private async void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            DateTime? tuNgay = DpTuNgay.SelectedDate;
            DateTime? denNgay = DpDenNgay.SelectedDate;
            
            var data = await _service.GetDatHangListAsync(_currentPhuongThucId, tuNgay, denNgay);
            DgDatHang.ItemsSource = data;
        }

        private async void DgDatHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                var chiTiet = await _service.GetDatHangChiTietListAsync(selectedOrder.Id);
                DgDatHangChiTiet.ItemsSource = chiTiet;
            }
            else
            {
                DgDatHangChiTiet.ItemsSource = null;
            }
        }
    }
}
