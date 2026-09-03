using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views.KhoHang
{
    public partial class XuatLaiDinhLuongWindow : Window
    {
        public XuatLaiDinhLuongWindow()
        {
            InitializeComponent();
            DpTuNgay.SelectedDate = DateTime.Today;
            DpDenNgay.SelectedDate = DateTime.Today;
        }

        private void BtnQuickDate_Click(object sender, RoutedEventArgs e)
        {
            if (BtnQuickDate.ContextMenu != null)
            {
                BtnQuickDate.ContextMenu.PlacementTarget = BtnQuickDate;
                BtnQuickDate.ContextMenu.IsOpen = true;
            }
        }

        private void QuickDate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string tag)
            {
                var today = DateTime.Today;
                switch (tag)
                {
                    case "Today":
                        DpTuNgay.SelectedDate = today;
                        DpDenNgay.SelectedDate = today;
                        break;
                    case "Yesterday":
                        DpTuNgay.SelectedDate = today.AddDays(-1);
                        DpDenNgay.SelectedDate = today.AddDays(-1);
                        break;
                    case "ThisWeek":
                        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                        DpTuNgay.SelectedDate = today.AddDays(-1 * diff);
                        DpDenNgay.SelectedDate = today;
                        break;
                    case "ThisMonth":
                        DpTuNgay.SelectedDate = new DateTime(today.Year, today.Month, 1);
                        DpDenNgay.SelectedDate = today;
                        break;
                    case "LastMonth":
                        var firstDayLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                        var lastDayLastMonth = new DateTime(today.Year, today.Month, 1).AddDays(-1);
                        DpTuNgay.SelectedDate = firstDayLastMonth;
                        DpDenNgay.SelectedDate = lastDayLastMonth;
                        break;
                    case "All":
                        DpTuNgay.SelectedDate = new DateTime(2000, 1, 1);
                        DpDenNgay.SelectedDate = today;
                        break;
                }
            }
        }

        private async void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnThucHien.IsEnabled = false;
                BtnHuyBo.IsEnabled = false;

                await Task.Delay(500); // Giả lập xử lý xuất lại định lượng

                MessageBox.Show("Xuất lại định lượng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnThucHien.IsEnabled = true;
                BtnHuyBo.IsEnabled = true;
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
