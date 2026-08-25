using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
    public partial class LuuVetHoatDongControl : UserControl
    {
        private readonly QuanLyBar.Client.Services.LocalHoaDonService _hoaDonService;
        private readonly QuanLyBar.Client.Services.LocalLuuVetService _luuVetService;

        public LuuVetHoatDongControl()
        {
            InitializeComponent();
            _hoaDonService = new QuanLyBar.Client.Services.LocalHoaDonService();
            _luuVetService = new QuanLyBar.Client.Services.LocalLuuVetService();
            this.Loaded += LuuVetHoatDongControl_Loaded;
        }

        private async void LuuVetHoatDongControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = System.DateTime.Now;
            dpDenNgay.SelectedDate = System.DateTime.Now;
            await LoadDataAsync();
        }

        private async void BtnTaiDuLieu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? System.DateTime.Now;
                var denNgay = dpDenNgay.SelectedDate ?? System.DateTime.Now;
                
                var hdList = await _hoaDonService.GetHoaDonListAsync(tuNgay, denNgay);
                DgHoaDon.ItemsSource = hdList;

                var luuVetList = await _luuVetService.GetLuuVetListAsync(tuNgay, denNgay);
                DgLuuVet.ItemsSource = luuVetList;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
