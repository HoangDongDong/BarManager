using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ChonKhuVucWindow : Window
    {
        private readonly LocalBanKhuVucService _khuVucService;
        public KhuVucViewModel SelectedKhuVuc { get; private set; }

        public ChonKhuVucWindow()
        {
            InitializeComponent();
            _khuVucService = new LocalBanKhuVucService();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var treeData = await _khuVucService.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                SelectedKhuVuc = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một khu vực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
