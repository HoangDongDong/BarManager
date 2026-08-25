using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class TheoDoiDatPhongControl : UserControl
    {
        private LocalTheoDoiDatPhongService _service;

        public TheoDoiDatPhongControl()
        {
            InitializeComponent();
            _service = new LocalTheoDoiDatPhongService();
            
            DpStartDate.SelectedDate = DateTime.Now;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DpStartDate.SelectedDate.HasValue)
            {
                await LoadData(DpStartDate.SelectedDate.Value);
            }
        }

        private async void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpStartDate.SelectedDate.HasValue)
            {
                await LoadData(DpStartDate.SelectedDate.Value);
            }
        }

        private async System.Threading.Tasks.Task LoadData(DateTime startDate)
        {
            // Cập nhật tiêu đề tháng năm
            TxtMonthYear.Text = $"Tháng {startDate.Month} - {startDate.Year}";

            // Cập nhật tiêu đề các cột (ví dụ 25.T3)
            UpdateColumnHeaders(startDate);

            // Tải dữ liệu từ Service
            var data = await _service.GetTheoDoiDatPhongAsync(startDate);
            DgTheoDoi.ItemsSource = data;
        }

        private void UpdateColumnHeaders(DateTime startDate)
        {
            var columns = new DataGridColumn[] { ColNgay1, ColNgay2, ColNgay3, ColNgay4, ColNgay5, ColNgay6, ColNgay7 };
            
            for (int i = 0; i < 7; i++)
            {
                DateTime date = startDate.AddDays(i);
                string dayOfWeekStr = GetDayOfWeekVN(date.DayOfWeek);
                
                // Format: "25.T3"
                columns[i].Header = $"{date.Day}.{dayOfWeekStr}";
                
                // Đổi màu vàng cho ngày Chủ nhật (như trong ảnh mẫu)
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    var style = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
                    style.Setters.Add(new Setter(Control.BackgroundProperty, new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#a1b3c6"))));
                    style.Setters.Add(new Setter(Control.ForegroundProperty, System.Windows.Media.Brushes.Yellow));
                    style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                    style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(5)));
                    style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
                    style.Setters.Add(new Setter(Control.BorderBrushProperty, new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#869db4"))));
                    columns[i].HeaderStyle = style;
                }
                else
                {
                    // Reset style về mặc định (chữ trắng)
                    columns[i].HeaderStyle = null; 
                }
            }
        }

        private string GetDayOfWeekVN(DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Monday: return "T2";
                case DayOfWeek.Tuesday: return "T3";
                case DayOfWeek.Wednesday: return "T4";
                case DayOfWeek.Thursday: return "T5";
                case DayOfWeek.Friday: return "T6";
                case DayOfWeek.Saturday: return "T7";
                case DayOfWeek.Sunday: return "CN";
                default: return "";
            }
        }
    }
}
