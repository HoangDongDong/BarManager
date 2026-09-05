using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DuLieuBanDau
{
    public partial class ChonKhoTonBanDauWindow : Window
    {
        public KhoHangComboItem SelectedKho { get; private set; }
        private List<KhoHangComboItem> _khoList = new List<KhoHangComboItem>();

        public ChonKhoTonBanDauWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _khoList = await LocalTonKhoBanDauService.GetKhoHangListAsync();
                LbKhoHang.ItemsSource = _khoList;

                if (_khoList.Count > 0)
                {
                    LbKhoHang.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách kho hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LbKhoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedKho = LbKhoHang.SelectedItem as KhoHangComboItem;
        }

        private void LbKhoHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LbKhoHang.SelectedItem is KhoHangComboItem kho)
            {
                SelectedKho = kho;
                DialogResult = true;
                Close();
            }
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedKho == null && LbKhoHang.SelectedItem is KhoHangComboItem kho)
            {
                SelectedKho = kho;
            }

            if (SelectedKho == null)
            {
                MessageBox.Show("Vui lòng chọn một kho hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
