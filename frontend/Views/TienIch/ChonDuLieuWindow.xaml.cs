using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ChonDuLieuWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        public NhomMatHangViewModel SelectedNhomMatHang { get; private set; }

        public ChonDuLieuWindow()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel selected)
            {
                SelectedNhomMatHang = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một nhóm mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
