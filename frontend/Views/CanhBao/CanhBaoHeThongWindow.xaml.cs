using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CanhBao
{
    public partial class CanhBaoHeThongWindow : Window
    {
        private CanhBaoSummary _summary;

        public CanhBaoHeThongWindow(CanhBaoSummary summary = null)
        {
            InitializeComponent();
            _summary = summary;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_summary != null)
            {
                BindData(_summary);
            }
            else
            {
                await ReloadDataAsync();
            }
        }

        private void BindData(CanhBaoSummary summary)
        {
            _summary = summary ?? new CanhBaoSummary();

            DgHangTon.ItemsSource = _summary.HangDuoiMucAnToan;
            TabHangTon.Header = $"📦 Hàng dưới mức an toàn ({_summary.HangDuoiMucAnToan.Count})";

            DgSinhNhat.ItemsSource = _summary.KhachHangSinhNhat;
            TabSinhNhat.Header = $"🎂 Khách sinh nhật hôm nay ({_summary.KhachHangSinhNhat.Count})";

            DgKhuyenMai.ItemsSource = _summary.ChuongTrinhKhuyenMai;
            TabKhuyenMai.Header = $"🎁 Khuyến mại đang áp dụng ({_summary.ChuongTrinhKhuyenMai.Count})";

            // Chọn tab có cảnh báo đầu tiên
            if (_summary.HangDuoiMucAnToan.Count > 0)
            {
                TabCanhBao.SelectedItem = TabHangTon;
            }
            else if (_summary.KhachHangSinhNhat.Count > 0)
            {
                TabCanhBao.SelectedItem = TabSinhNhat;
            }
            else if (_summary.ChuongTrinhKhuyenMai.Count > 0)
            {
                TabCanhBao.SelectedItem = TabKhuyenMai;
            }

            int total = _summary.HangDuoiMucAnToan.Count + _summary.KhachHangSinhNhat.Count + _summary.ChuongTrinhKhuyenMai.Count;
            TxtStatus.Text = $"Tổng cộng {total} cảnh báo cần lưu ý";
        }

        private async Task ReloadDataAsync()
        {
            try
            {
                TxtStatus.Text = "Đang tải dữ liệu cảnh báo...";
                _summary = await LocalCanhBaoService.GetCanhBaoSummaryAsync();
                BindData(_summary);
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Lỗi tải dữ liệu: " + ex.Message;
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await ReloadDataAsync();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                _ = ReloadDataAsync();
                e.Handled = true;
            }
        }
    }
}
