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
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using static QuanLyBar.Client.Services.LocalBaoCaoQuanTriService;

namespace QuanLyBar.Client.Views.BaoCaoQuanTri
{
    public partial class BaoCaoBanHangTheoGioControl : UserControl
    {
        private readonly LocalBaoCaoQuanTriService _service = new LocalBaoCaoQuanTriService();
        private bool _isLoaded = false;
        private List<BanHangTheoGioItem> _allData = new List<BanHangTheoGioItem>();

        public BaoCaoBanHangTheoGioControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            DpTuNgay.SelectedDate = startOfMonth;
            DpDenNgay.SelectedDate = today;
            SideTuNgay.SelectedDate = startOfMonth;
            SideDenNgay.SelectedDate = today;
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

        private void SideDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            if (sender == SideTuNgay) DpTuNgay.SelectedDate = SideTuNgay.SelectedDate;
            if (sender == SideDenNgay) DpDenNgay.SelectedDate = SideDenNgay.SelectedDate;
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

                TxtFilterSummary.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";

                _allData = await _service.GetBaoCaoBanHangTheoGioAsync(tuNgay, denNgay);

                RenderTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderTable();
        }

        private void RenderTable()
        {
            PnlReportContent.Children.Clear();

            string filter = TxtSearch.Text?.Trim().ToLower() ?? "";

            var filteredItems = string.IsNullOrEmpty(filter)
                ? _allData
                : _allData.Where(x => (x.GioKhachVao?.ToLower().Contains(filter) ?? false)).ToList();

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Giờ khách vào
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) }); // Số hóa đơn
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) }); // Tổng doanh số

            // Header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderCell(grid, "STT", 0, 0);
            AddHeaderCell(grid, "Giờ khách vào", 0, 1);
            AddHeaderCell(grid, "Số hóa đơn", 0, 2);
            AddHeaderCell(grid, "Tổng doanh số", 0, 3);

            int rowIdx = 1;
            int stt = 1;
            foreach (var it in filteredItems)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                AddDataCell(grid, stt.ToString(), rowIdx, 0, TextAlignment.Center);
                AddDataCell(grid, it.GioKhachVao, rowIdx, 1, TextAlignment.Center);
                AddDataCell(grid, it.SoHoaDon.ToString("#,##0"), rowIdx, 2, TextAlignment.Right);
                AddDataCell(grid, it.TongDoanhSo > 0 ? it.TongDoanhSo.ToString("#,##0") : "0", rowIdx, 3, TextAlignment.Right);

                stt++;
                rowIdx++;
            }

            // Total row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddGrandTotalCell(grid, "TỔNG CỘNG", rowIdx, 0, 1, 2, TextAlignment.Center);
            AddGrandTotalCell(grid, filteredItems.Sum(x => x.SoHoaDon).ToString("#,##0"), rowIdx, 2, 1, 1, TextAlignment.Right);
            AddGrandTotalCell(grid, filteredItems.Sum(x => x.TongDoanhSo).ToString("#,##0"), rowIdx, 3, 1, 1, TextAlignment.Right);

            PnlReportContent.Children.Add(grid);
        }

        private void AddHeaderCell(Grid g, string text, int row, int col)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 1, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(4, 5, 4, 5)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            g.Children.Add(b);
        }

        private void AddDataCell(Grid g, string text, int row, int col, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = Brushes.White,
                Padding = new Thickness(5, 4, 5, 4)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 11,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            g.Children.Add(b);
        }

        private void AddGrandTotalCell(Grid g, string text, int row, int col, int rowSpan, int colSpan, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(235, 240, 248)),
                Padding = new Thickness(4, 5, 4, 5)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            if (colSpan > 1) Grid.SetColumnSpan(b, colSpan);
            g.Children.Add(b);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "BÁO CÁO BÁN HÀNG THEO GIỜ VÀO");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"BaoCao_BanHangTheoGio_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO BÁN HÀNG THEO GIỜ VÀO");
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("STT,Giờ khách vào,Số hóa đơn,Tổng doanh số");

                    int idx = 1;
                    foreach (var it in _allData)
                    {
                        sb.AppendLine($"{idx++},\"{it.GioKhachVao}\",{it.SoHoaDon},{it.TongDoanhSo}");
                    }
                    sb.AppendLine($"TỔNG CỘNG,,{_allData.Sum(x => x.SoHoaDon)},{_allData.Sum(x => x.TongDoanhSo)}");

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
