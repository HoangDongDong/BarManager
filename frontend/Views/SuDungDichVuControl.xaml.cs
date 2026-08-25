using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class SuDungDichVuControl : UserControl
    {
        private LocalSuDungDichVuService _service;

        public SuDungDichVuControl()
        {
            InitializeComponent();
            _service = new LocalSuDungDichVuService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Tải danh sách Khu vực và bàn
            var khuVucList = await _service.GetKhuVucBanListAsync();
            IcKhuVuc.ItemsSource = khuVucList;

            // Tải cây danh mục Nhóm mặt hàng
            var menuTree = await _service.GetNhomMatHangTreeAsync();
            TvMenu.ItemsSource = menuTree;

            // Mặc định tải toàn bộ danh sách mặt hàng lên lưới
            var allItems = await _service.GetMatHangListAsync(string.Empty);
            DgMatHang.ItemsSource = allItems;
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            // Xử lý sự kiện click vào 1 Bàn
            if (sender is Button btn && btn.DataContext is PosBanViewModel ban)
            {
                MessageBox.Show($"Bạn vừa chọn bàn: {ban.Name}", "Thông báo");
                // TODO (Giai đoạn 2): Tải chi tiết đơn hàng của bàn này lên DgChiTiet
            }
        }

        private async void TvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is PosNhomMatHangViewModel nhom)
            {
                var list = await _service.GetMatHangListAsync(nhom.Id);
                DgMatHang.ItemsSource = list;
            }
        }
    }
}
