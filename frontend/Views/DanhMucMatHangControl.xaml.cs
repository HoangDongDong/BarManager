using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucMatHangControl : UserControl
    {
        private readonly LocalMatHangService _matHangService;

        public DanhMucMatHangControl()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load danh sách nhóm mặt hàng lên TreeView
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
            
            // Load toàn bộ mặt hàng mặc định (ID = 0 hoặc null)
            LoadMatHangData(null);
        }

        private void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomMatHangViewModel selectedNhom)
            {
                // Nếu chọn "Tất cả" (Id = string.Empty) thì truyền null để lấy hết
                string filterId = string.IsNullOrEmpty(selectedNhom.Id) ? null : selectedNhom.Id;
                LoadMatHangData(filterId);
            }
        }

        private async void LoadMatHangData(string nhomId)
        {
            var data = await _matHangService.GetMatHangListAsync(nhomId);
            DgMatHang.ItemsSource = data;
        }
    }
}
