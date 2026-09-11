using System;
using System.Windows;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class DanhSachHoaDonChuaThanhToanWindow : Window
    {
        public DanhSachHoaDonChuaThanhToanWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var data = LocalDatabaseService.GetAll<Models.TDONHANG>(
                    "SELECT FIRST 100 * FROM TDONHANG WHERE STATUS = 1 ORDER BY NGAY DESC");
                DgHoaDon.ItemsSource = data;
            }
            catch { }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e) { LoadData(); }

        private void BtnClose_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}
