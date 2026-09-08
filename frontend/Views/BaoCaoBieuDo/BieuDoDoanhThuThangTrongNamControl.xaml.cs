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
    public partial class BieuDoDoanhThuThangTrongNamControl : UserControl
    {
        private readonly LocalBaoCaoBieuDoService _service = new LocalBaoCaoBieuDoService();
        private bool _isLoaded = false;
        private List<ThangTrongNamItem> _data = new List<ThangTrongNamItem>();

        public BieuDoDoanhThuThangTrongNamControl()
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

                _data = await _service.GetDoanhThuThangTrongNamAsync(nam);

                RenderChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CvsYearChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderChart();
        }

        private void RenderChart()
        {
            CvsYearChart.Children.Clear();

            double w = CvsYearChart.ActualWidth > 0 ? CvsYearChart.ActualWidth : 800;
            double h = CvsYearChart.ActualHeight > 0 ? CvsYearChart.ActualHeight : 320;

            double padLeft = 70;
            double padRight = 40;
            double padTop = 30;
            double padBottom = 40;

            double plotW = w - padLeft - padRight;
            double plotH = h - padTop - padBottom;
            if (plotW <= 0 || plotH <= 0) return;

            decimal maxVal = 0;
            foreach (var it in _data)
            {
                if (it.DoanhThuNamNay > maxVal) maxVal = it.DoanhThuNamNay;
                if (it.DoanhThuNamNgoai > maxVal) maxVal = it.DoanhThuNamNgoai;
            }
            if (maxVal <= 0) maxVal = 6000000;
            decimal step = CalculateNiceStep(maxVal);
            decimal topVal = Math.Ceiling(maxVal / step) * step;
            if (topVal <= 0) topVal = step;

            // Y-axis grid & labels
            int numYSteps = 3;
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
                CvsYearChart.Children.Add(line);

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
                CvsYearChart.Children.Add(lbl);
            }

            // X-axis grid (Months 0..12)
            for (int m = 0; m <= 12; m++)
            {
                double x = padLeft + (m / 12.0) * plotW;

                if (m > 0)
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
                    CvsYearChart.Children.Add(line);
                }

                var lbl = new TextBlock
                {
                    Text = m.ToString(),
                    FontSize = 9.5,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    Width = 20
                };
                Canvas.SetLeft(lbl, x - 10);
                Canvas.SetTop(lbl, padTop + plotH + 4);
                CvsYearChart.Children.Add(lbl);
            }

            // Axes
            var yAxis = new Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = padTop + plotH, Stroke = Brushes.Black, StrokeThickness = 1.2 };
            var xAxis = new Line { X1 = padLeft, Y1 = padTop + plotH, X2 = padLeft + plotW, Y2 = padTop + plotH, Stroke = Brushes.Black, StrokeThickness = 1.2 };
            CvsYearChart.Children.Add(yAxis);
            CvsYearChart.Children.Add(xAxis);

            // Draw Series 2: Năm ngoái (Orange)
            var pointsNgoai = new List<Point> { new Point(padLeft, padTop + plotH) };
            foreach (var it in _data)
            {
                double px = padLeft + (it.Thang / 12.0) * plotW;
                double py = padTop + plotH - (double)(it.DoanhThuNamNgoai / topVal) * plotH;
                pointsNgoai.Add(new Point(px, py));
            }
            DrawSmoothCurve(pointsNgoai, Color.FromRgb(245, 166, 35));

            // Draw Series 1: Năm nay (Blue)
            var pointsNay = new List<Point> { new Point(padLeft, padTop + plotH) };
            foreach (var it in _data)
            {
                double px = padLeft + (it.Thang / 12.0) * plotW;
                double py = padTop + plotH - (double)(it.DoanhThuNamNay / topVal) * plotH;
                pointsNay.Add(new Point(px, py));
            }
            DrawSmoothCurve(pointsNay, Color.FromRgb(74, 144, 226));

            // Add value labels on notable peaks for Năm nay and Năm ngoái
            foreach (var it in _data)
            {
                if (it.DoanhThuNamNay > 0)
                {
                    double px = padLeft + (it.Thang / 12.0) * plotW;
                    double py = padTop + plotH - (double)(it.DoanhThuNamNay / topVal) * plotH;
                    AddPeakLabel(px, py, it.DoanhThuNamNay, Color.FromRgb(74, 144, 226), $"Tháng {it.Thang} (Năm nay)");
                }
                if (it.DoanhThuNamNgoai > 0)
                {
                    double px = padLeft + (it.Thang / 12.0) * plotW;
                    double py = padTop + plotH - (double)(it.DoanhThuNamNgoai / topVal) * plotH;
                    AddPeakLabel(px, py, it.DoanhThuNamNgoai, Color.FromRgb(245, 166, 35), $"Tháng {it.Thang} (Năm ngoái)");
                }
            }
        }

        private void DrawSmoothCurve(List<Point> points, Color color)
        {
            if (points.Count < 2) return;

            var poly = new Polyline
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2.0,
                StrokeLineJoin = PenLineJoin.Round
            };
            foreach (var p in points) poly.Points.Add(p);
            CvsYearChart.Children.Add(poly);
        }

        private void AddPeakLabel(double px, double py, decimal val, Color color, string tooltip)
        {
            var dot = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorBrush(color),
                ToolTip = $"{tooltip}: {val:#,##0} VNĐ"
            };
            Canvas.SetLeft(dot, px - 2.5);
            Canvas.SetTop(dot, py - 2.5);
            CvsYearChart.Children.Add(dot);

            var valTxt = new TextBlock
            {
                Text = val.ToString("#,##0"),
                FontSize = 8.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Width = 80
            };
            Canvas.SetLeft(valTxt, px - 40);
            Canvas.SetTop(valTxt, py - 15);
            CvsYearChart.Children.Add(valTxt);
        }

        private decimal CalculateNiceStep(decimal max)
        {
            if (max <= 1000000) return 200000;
            if (max <= 3000000) return 1000000;
            if (max <= 6000000) return 2000000;
            if (max <= 15000000) return 5000000;
            return 10000000;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "BIỂU ĐỒ DOANH THU THÁNG TRONG NĂM");
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
                    FileName = $"BieuDo_DoanhThuThangTrongNam_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BIỂU ĐỒ DOANH THU THÁNG TRONG NĂM");
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("Tháng,Doanh thu năm nay,Doanh thu năm ngoái");

                    foreach (var it in _data)
                    {
                        sb.AppendLine($"Tháng {it.Thang},{it.DoanhThuNamNay},{it.DoanhThuNamNgoai}");
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
    }
}
