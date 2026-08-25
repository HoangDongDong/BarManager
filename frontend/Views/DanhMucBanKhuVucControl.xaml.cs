using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucBanKhuVucControl : UserControl
    {
        private LocalBanKhuVucService _service;

        public DanhMucBanKhuVucControl()
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load tree khu vực
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;

            // Load tất cả bàn mặc định
            await LoadBanData(null);
        }

        private async void TvKhuVuc_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is KhuVucViewModel selectedKhuVuc)
            {
                await LoadBanData(selectedKhuVuc.Id);
            }
        }

        private async System.Threading.Tasks.Task LoadBanData(string khuVucId)
        {
            var data = await _service.GetBanListAsync(khuVucId);
            DgBan.ItemsSource = data;
        }
    }
}
