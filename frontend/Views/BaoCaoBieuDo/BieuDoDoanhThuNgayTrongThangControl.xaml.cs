using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using static QuanLyBar.Client.Services.LocalBaoCaoBieuDoService;

namespace QuanLyBar.Client.Views.BaoCaoBieuDo
{
    public partial class BieuDoDoanhThuNgayTrongThangControl : UserControl
    {
        private readonly LocalBaoCaoBieuDoService _service = new LocalBaoCaoBieuDoService();
        private bool _isLoaded = false;
        private List<ThangDoanhThuSeries> _seriesList = new List<ThangDoanhThuSeries>();

        private readonly Color[] _monthColors = new[]
        {
            Color.FromRgb(74, 144, 226),  // Tháng 1: Blue
            Color.FromRgb(245, 166, 35),  // Tháng 2: Orange
            Color.FromRgb(180, 115, 34),  // Tháng 3: Brown
            Color.FromRgb(126, 211, 33),  // Tháng 4: Green
            Color.FromRgb(189, 16, 224),  // Tháng 5: Purple
            Color.FromRgb(30, 40, 90),    // Tháng 6: Dark Navy
            Color.FromRgb(248, 180, 0),   // Tháng 7: Gold
            Color.FromRgb(208, 2, 27),    // Tháng 8: Red
            Color.FromRgb(80, 200, 220),  // Tháng 9: Light Blue / Cyan
            Color.FromRgb(144, 19, 254),  // Tháng 10: Violet
            Color.FromRgb(46, 125, 50),   // Tháng 11: Dark Green
            Color.FromRgb(100, 100, 100)  // Tháng 12: Gray
        };

        public BieuDoDoanhThuNgayTrongThangControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            TxtNam.Text = today.Year.ToString();
            TxtSignDate.Text = $"Ngày {today:dd} tháng {today:MM} năm {today:yyyy}";

            await LoadCompanyInfoAndLogoAsync();
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAndLogoAsync()
        {
            try
            {
                var comp = await LocalCauHinhService.GetCompanyInfoAsync();
                TxtCompanyName.Text = comp.Name;
                TxtCompanyAddress.Text = comp.FormattedAddress;
                TxtCompanyContact.Text = comp.FormattedContact;

                if (comp.LogoBytes != null && comp.LogoBytes.Length > 0)
                {
                    var bi = LocalCauHinhService.ImageFromBytes(comp.LogoBytes);
                    if (bi != null)
                    {
                        ImgLogo.Source = bi;
                        ImgLogo.Visibility = Visibility.Visible;
                        if (FindName("VbDefaultLogo") is UIElement vb) vb.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                int.TryParse(TxtNam.Text?.Trim(), out int nam);
                if (nam < 2000 || nam > 2100) nam = DateTime.Today.Year;

                TxtFilterSummary.Text = $"Năm: {nam}";

                _seriesList = await _service.GetDoanhThuNgayTrongThangAsync(nam);

                RenderChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CvsLineChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderChart();
        }

        private void RenderChart()
        {
            CvsLineChart.Children.Clear();
            PnlLegend.Children.Clear();

            double w = CvsLineChart.ActualWidth > 0 ? CvsLineChart.ActualWidth : 750;
            double h = CvsLineChart.ActualHeight > 0 ? CvsLineChart.ActualHeight : 350;

            double padLeft = 70;
            double padRight = 30;
            double padTop = 30;
            double padBottom = 40;

            double plotW = w - padLeft - padRight;
            double plotH = h - padTop - padBottom;
            if (plotW <= 0 || plotH <= 0) return;

            // Find Max Value
            decimal maxVal = 0;
            foreach (var s in _seriesList)
            {
                foreach (var pt in s.Data)
                {
                    if (pt.DoanhThu > maxVal) maxVal = pt.DoanhThu;
                }
            }
            if (maxVal <= 0) maxVal = 5000000;
            decimal step = CalculateNiceStep(maxVal);
            decimal topVal = Math.Ceiling(maxVal / step) * step;
            if (topVal <= 0) topVal = step;

            // Draw Y-axis grid & labels
            int numYSteps = 5;
            for (int i = 0; i <= numYSteps; i++)
            {
                decimal v = (topVal / numYSteps) * i;
                double y = padTop + plotH - (double)(v / topVal) * plotH;

                var line = new Line
                {
                    X1 = padLeft,
                    Y1 = y,
                    X2 = padLeft + plotW,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    StrokeThickness = 1
                };
                CvsLineChart.Children.Add(line);

                var lbl = new TextBlock
                {
                    Text = v.ToString("#,##0"),
                    FontSize = 9.5,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Right,
                    Width = padLeft - 8
                };
                Canvas.SetLeft(lbl, 0);
                Canvas.SetTop(lbl, y - 7);
                CvsLineChart.Children.Add(lbl);
            }

            // Draw X-axis grid lines (Days 0, 5, 10, 15, 20, 25, 30)
            int[] xTicks = { 0, 5, 10, 15, 20, 25, 30 };
            foreach (int xVal in xTicks)
            {
                double x = padLeft + (xVal / 31.0) * plotW;

                if (xVal > 0)
                {
                    var line = new Line
                    {
                        X1 = x,
                        Y1 = padTop,
                        X2 = x,
                        Y2 = padTop + plotH,
                        Stroke = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)),
                        StrokeThickness = 1
                    };
                    CvsLineChart.Children.Add(line);
                }

                var lbl = new TextBlock
                {
                    Text = xVal.ToString(),
                    FontSize = 9.5,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    Width = 20
                };
                Canvas.SetLeft(lbl, x - 10);
                Canvas.SetTop(lbl, padTop + plotH + 4);
                CvsLineChart.Children.Add(lbl);
            }

            // Draw Axes
            var yAxis = new Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = padTop + plotH, Stroke = Brushes.Black, StrokeThickness = 1.2 };
            var xAxis = new Line { X1 = padLeft, Y1 = padTop + plotH, X2 = padLeft + plotW, Y2 = padTop + plotH, Stroke = Brushes.Black, StrokeThickness = 1.2 };
            CvsLineChart.Children.Add(yAxis);
            CvsLineChart.Children.Add(xAxis);

            // Draw Series Lines and Peak labels
            for (int m = 0; m < _seriesList.Count; m++)
            {
                var series = _seriesList[m];
                var color = _monthColors[m % _monthColors.Length];

                // Build Path Geometry for smooth spline or polyline
                var points = new List<Point>();
                // Day 0 starts at (padLeft, padTop + plotH)
                points.Add(new Point(padLeft, padTop + plotH));

                foreach (var pt in series.Data)
                {
                    double px = padLeft + (pt.Ngay / 31.0) * plotW;
                    double py = padTop + plotH - (double)(pt.DoanhThu / topVal) * plotH;
                    points.Add(new Point(px, py));
                }

                var polyline = new Polyline
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1.6,
                    StrokeLineJoin = PenLineJoin.Round
                };
                foreach (var p in points) polyline.Points.Add(p);
                CvsLineChart.Children.Add(polyline);

                // Add value labels on notable peaks (> 0)
                foreach (var pt in series.Data)
                {
                    if (pt.DoanhThu > 0)
                    {
                        double px = padLeft + (pt.Ngay / 31.0) * plotW;
                        double py = padTop + plotH - (double)(pt.DoanhThu / topVal) * plotH;

                        // Dot marker
                        var dot = new Ellipse
                        {
                            Width = 4,
                            Height = 4,
                            Fill = new SolidColorBrush(color),
                            ToolTip = $"Tháng {series.Thang}, Ngày {pt.Ngay}: {pt.DoanhThu:#,##0} VNĐ"
                        };
                        Canvas.SetLeft(dot, px - 2);
                        Canvas.SetTop(dot, py - 2);
                        CvsLineChart.Children.Add(dot);

                        // Value text
                        var valTxt = new TextBlock
                        {
                            Text = pt.DoanhThu.ToString("#,##0"),
                            FontSize = 8.5,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            Width = 70
                        };
                        Canvas.SetLeft(valTxt, px - 35);
                        Canvas.SetTop(valTxt, py - 14);
                        CvsLineChart.Children.Add(valTxt);
                    }
                }

                // Legend item
                var legItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                var legLine = new Rectangle { Width = 14, Height = 3, Fill = new SolidColorBrush(color), Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
                var legTxt = new TextBlock { Text = series.TenThang, FontSize = 9.5, Foreground = Brushes.Black };
                legItem.Children.Add(legLine);
                legItem.Children.Add(legTxt);
                PnlLegend.Children.Add(legItem);
            }
        }

        private decimal CalculateNiceStep(decimal max)
        {
            if (max <= 500000) return 100000;
            if (max <= 2000000) return 500000;
            if (max <= 5000000) return 1000000;
            if (max <= 10000000) return 2000000;
            return 5000000;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "BIỂU ĐỒ DOANH THU NGÀY TRONG THÁNG");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"BieuDo_DoanhThuNgayTrongThang_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BIỂU ĐỒ DOANH THU NGÀY TRONG THÁNG");
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("Tháng,Ngày,Doanh thu");

                    foreach (var s in _seriesList)
                    {
                        foreach (var pt in s.Data)
                        {
                            if (pt.DoanhThu > 0)
                            {
                                sb.AppendLine($"{s.Thang},{pt.Ngay},{pt.DoanhThu}");
                            }
                        }
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file báo cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Chuc nang thiet ke mau bao cao cho phep ban tuy chinh:\n" +
                "  - Bo cuc va dinh dang bao cao\n" +
                "  - Font chu, co chu, mau sac\n" +
                "  - Logo va thong tin dau trang\n" +
                "  - Them/bo cac cot hien thi\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Thiết kế mẫu bao cao",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnThamSoTuyChinh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tham số tùy chỉnh báo cáo cho phep ban:\n" +
                "  - Chon cac cot hien thi trong bao cao\n" +
                "  - Thiet lap tieu chi nhom du lieu\n" +
                "  - Cau hinh cac dieu kien loc nang cao\n" +
                "  - Tuy chinh dinh dang so va ngay thang\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Tham số tuy chinh",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnXemDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (InlineDataBorder.Visibility == System.Windows.Visibility.Visible)
            {
                InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            // Use reflection to find the largest data list in this control
            var fields = this.GetType().GetFields(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object? dataSource = null;
            int maxCount = 0;
            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                if (val is System.Collections.IList list && list.Count > maxCount)
                {
                    maxCount = list.Count;
                    dataSource = val;
                }
            }

            if (dataSource != null)
                InlineDataGrid.ItemsSource = (System.Collections.IEnumerable)dataSource;
            else
                InlineDataGrid.ItemsSource = new[] { new { ThongBao = "Không có dữ liệu. Hãy tải dữ liệu trước (F5) roi mo lai." } };

            TxtSoBanGhi.Text = $"Tổng số: {maxCount} bản ghi";
            InlineDataBorder.Visibility = System.Windows.Visibility.Visible;

            // Scroll to bottom so user can see the panel
            var scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(this);
            scrollViewer?.ScrollToEnd();
        }

        private void BtnDongDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
