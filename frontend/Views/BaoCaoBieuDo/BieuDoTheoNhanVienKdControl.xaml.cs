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
    public partial class BieuDoTheoNhanVienKdControl : UserControl
    {
        private readonly LocalBaoCaoBieuDoService _service = new LocalBaoCaoBieuDoService();
        private bool _isLoaded = false;
        private List<DoanhSoNhanVienItem> _allData = new List<DoanhSoNhanVienItem>();

        private readonly Color[] _chartColors = new[]
        {
            Color.FromRgb(74, 144, 226),  // Blue
            Color.FromRgb(245, 166, 35),  // Orange
            Color.FromRgb(126, 211, 33),  // Green
            Color.FromRgb(189, 16, 224),  // Purple
            Color.FromRgb(80, 227, 194),  // Teal
            Color.FromRgb(208, 2, 27),    // Red
            Color.FromRgb(248, 231, 28),  // Yellow
            Color.FromRgb(144, 19, 254),  // Indigo
            Color.FromRgb(65, 117, 164),  // Steel Blue
            Color.FromRgb(243, 156, 18)   // Amber
        };

        public BieuDoTheoNhanVienKdControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = new DateTime(today.Year, today.Month, 1);
            DpDenNgay.SelectedDate = today;
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
                var tuNgay = DpTuNgay.SelectedDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;

                TxtFilterSummary.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} đến ngày {denNgay:dd/MM/yyyy}";

                _allData = await _service.GetDoanhSoTheoNhanVienKdAsync(tuNgay, denNgay);

                RenderCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderCharts();
        }

        private void CvsChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderCharts();
        }

        private void RenderCharts()
        {
            string filter = TxtSearch.Text?.Trim().ToLower() ?? "";
            var data = string.IsNullOrEmpty(filter)
                ? _allData
                : _allData.Where(x => x.TenNhanVien.ToLower().Contains(filter)).ToList();

            RenderBarChart(data);
            RenderPieChart(data);
        }

        private void RenderBarChart(List<DoanhSoNhanVienItem> data)
        {
            CvsBarChart.Children.Clear();
            double w = CvsBarChart.ActualWidth > 0 ? CvsBarChart.ActualWidth : 500;
            double h = CvsBarChart.ActualHeight > 0 ? CvsBarChart.ActualHeight : 300;

            if (data == null || data.Count == 0)
            {
                var noData = new TextBlock
                {
                    Text = "Không có dữ liệu doanh số nhân viên trong khoảng thời gian này",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(noData, w / 4);
                Canvas.SetTop(noData, h / 2);
                CvsBarChart.Children.Add(noData);
                return;
            }

            double padLeft = 70;
            double padRight = 20;
            double padTop = 30;
            double padBottom = 55;

            double plotW = w - padLeft - padRight;
            double plotH = h - padTop - padBottom;
            if (plotW <= 0 || plotH <= 0) return;

            decimal maxVal = data.Max(x => x.DoanhSo);
            if (maxVal <= 0) maxVal = 1000000;
            decimal step = CalculateNiceStep(maxVal);
            decimal topVal = Math.Ceiling(maxVal / step) * step;
            if (topVal <= 0) topVal = step;

            // Draw Y-axis grid lines and labels
            int numSteps = 5;
            for (int i = 0; i <= numSteps; i++)
            {
                decimal v = (topVal / numSteps) * i;
                double y = padTop + plotH - (double)(v / topVal) * plotH;

                var line = new Line
                {
                    X1 = padLeft,
                    Y1 = y,
                    X2 = padLeft + plotW,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                    StrokeThickness = 1
                };
                CvsBarChart.Children.Add(line);

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
                CvsBarChart.Children.Add(lbl);
            }

            // Draw Axes
            var yAxis = new Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = padTop + plotH, Stroke = Brushes.Black, StrokeThickness = 1.2 };
            var xAxis = new Line { X1 = padLeft, Y1 = padTop + plotH, X2 = padLeft + plotW, Y2 = padTop + plotH, Stroke = Brushes.Black, StrokeThickness = 1.2 };
            CvsBarChart.Children.Add(yAxis);
            CvsBarChart.Children.Add(xAxis);

            // Draw Bars
            int n = data.Count;
            double groupWidth = plotW / n;
            double barWidth = Math.Min(groupWidth * 0.65, 45);

            for (int i = 0; i < n; i++)
            {
                var it = data[i];
                double barH = (double)(it.DoanhSo / topVal) * plotH;
                double x = padLeft + i * groupWidth + (groupWidth - barWidth) / 2;
                double y = padTop + plotH - barH;

                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(barH, 0),
                    Fill = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
                    Stroke = new SolidColorBrush(Color.FromRgb(50, 110, 180)),
                    StrokeThickness = 1,
                    ToolTip = $"{it.TenNhanVien}: {it.DoanhSo:#,##0} VNĐ ({it.TyLe}%)"
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                CvsBarChart.Children.Add(rect);

                var valTxt = new TextBlock
                {
                    Text = it.DoanhSo > 0 ? it.DoanhSo.ToString("#,##0") : "0",
                    FontSize = 9,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    Width = groupWidth
                };
                Canvas.SetLeft(valTxt, padLeft + i * groupWidth);
                Canvas.SetTop(valTxt, Math.Max(y - 14, 5));
                CvsBarChart.Children.Add(valTxt);

                var catTxt = new TextBlock
                {
                    Text = it.TenNhanVien,
                    FontSize = 8.5,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Width = groupWidth
                };
                Canvas.SetLeft(catTxt, padLeft + i * groupWidth);
                Canvas.SetTop(catTxt, padTop + plotH + 4);
                CvsBarChart.Children.Add(catTxt);
            }
        }

        private void RenderPieChart(List<DoanhSoNhanVienItem> data)
        {
            CvsPieChart.Children.Clear();
            PnlPieLegend.Children.Clear();

            if (data == null || data.Count == 0) return;

            double w = CvsPieChart.ActualWidth > 0 ? CvsPieChart.ActualWidth : 200;
            double h = CvsPieChart.ActualHeight > 0 ? CvsPieChart.ActualHeight : 300;

            double cx = w / 2;
            double cy = h / 2;
            double radius = Math.Min(cx, cy) * 0.78;
            if (radius <= 0) radius = 80;

            decimal total = data.Sum(x => x.DoanhSo);
            if (total <= 0) return;

            double startAngle = -90;

            for (int i = 0; i < data.Count; i++)
            {
                var it = data[i];
                double sweepAngle = (double)(it.DoanhSo / total) * 360.0;
                var color = _chartColors[i % _chartColors.Length];

                var slice = CreatePieSlice(cx, cy, radius, startAngle, sweepAngle, color);
                slice.ToolTip = $"{it.TenNhanVien}: {it.DoanhSo:#,##0} VNĐ ({it.TyLe}%)";
                CvsPieChart.Children.Add(slice);

                if (sweepAngle > 15)
                {
                    double midAngle = (startAngle + sweepAngle / 2) * Math.PI / 180.0;
                    double lblR = radius * 0.65;
                    double lx = cx + lblR * Math.Cos(midAngle);
                    double ly = cy + lblR * Math.Sin(midAngle);

                    var lbl = new TextBlock
                    {
                        Text = $"{it.TyLe}%",
                        FontSize = 9,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };
                    Canvas.SetLeft(lbl, lx - 14);
                    Canvas.SetTop(lbl, ly - 7);
                    CvsPieChart.Children.Add(lbl);
                }

                startAngle += sweepAngle;

                var legendItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                var colBox = new Rectangle { Width = 10, Height = 10, Fill = new SolidColorBrush(color), Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
                var legendText = new TextBlock
                {
                    Text = $"{it.TenNhanVien}: {it.TyLe}%",
                    FontSize = 9.5,
                    Foreground = Brushes.Black,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 120,
                    ToolTip = $"{it.TenNhanVien}: {it.DoanhSo:#,##0} VNĐ ({it.TyLe}%)"
                };
                legendItem.Children.Add(colBox);
                legendItem.Children.Add(legendText);
                PnlPieLegend.Children.Add(legendItem);
            }
        }

        private System.Windows.Shapes.Path CreatePieSlice(double cx, double cy, double r, double startAngle, double sweepAngle, Color color)
        {
            if (sweepAngle >= 360)
            {
                var ellipse = new EllipseGeometry(new Point(cx, cy), r, r);
                return new System.Windows.Shapes.Path
                {
                    Fill = new SolidColorBrush(color),
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5,
                    Data = ellipse
                };
            }

            double startRad = startAngle * Math.PI / 180.0;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180.0;

            Point p1 = new Point(cx, cy);
            Point p2 = new Point(cx + r * Math.Cos(startRad), cy + r * Math.Sin(startRad));
            Point p3 = new Point(cx + r * Math.Cos(endRad), cy + r * Math.Sin(endRad));

            var fig = new PathFigure { StartPoint = p1, IsClosed = true, IsFilled = true };
            fig.Segments.Add(new LineSegment(p2, true));
            fig.Segments.Add(new ArcSegment(p3, new Size(r, r), 0, sweepAngle > 180, SweepDirection.Clockwise, true));

            var geo = new PathGeometry();
            geo.Figures.Add(fig);

            return new System.Windows.Shapes.Path
            {
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                Data = geo
            };
        }

        private decimal CalculateNiceStep(decimal max)
        {
            if (max <= 100000) return 20000;
            if (max <= 500000) return 100000;
            if (max <= 2000000) return 200000;
            if (max <= 5000000) return 500000;
            if (max <= 10000000) return 1000000;
            return 2000000;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "BIỂU ĐỒ DOANH SỐ THEO NHÂN VIÊN KINH DOANH");
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
                    FileName = $"BieuDo_DoanhSoNhanVien_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BIỂU ĐỒ DOANH SỐ THEO NHÂN VIÊN KINH DOANH");
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("STT,Nhân viên,Doanh số,Tỷ lệ %");

                    int idx = 1;
                    foreach (var it in _allData)
                    {
                        sb.AppendLine($"{idx++},\"{it.TenNhanVien}\",{it.DoanhSo},{it.TyLe}");
                    }
                    sb.AppendLine($"TỔNG CỘNG,,{_allData.Sum(x => x.DoanhSo)},100%");

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
