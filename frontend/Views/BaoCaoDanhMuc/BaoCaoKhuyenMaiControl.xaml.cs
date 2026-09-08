using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoDanhMuc
{
    public partial class BaoCaoKhuyenMaiControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<DotKhuyenMaiViewModel> _rawItems = new List<DotKhuyenMaiViewModel>();

        public BaoCaoKhuyenMaiControl(string reportType = "DANH SÁCH ĐỢT KHUYẾN MẠI")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "DANH SÁCH ĐỢT KHUYẾN MẠI" : reportType.Trim().ToUpper();

            TxtReportTitle.Text = _reportType;

            // Keyboard Shortcuts: F5 (Refresh), F3 (Search), Ctrl+P (Print), F12 (Excel)
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F5)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
                else if (e.Key == Key.F3)
                {
                    TxtSearch.Focus();
                    TxtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.F12)
                {
                    ExportToCsv();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
                {
                    PrintReport();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
            };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

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

        private async Task LoadDataAsync()
        {
            if (!_isLoaded) return;

            try
            {
                _rawItems = await LocalKhuyenMaiService.GetDotKhuyenMaiListAsync();
                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.Name?.ToLower().Contains(search) == true) ||
                (x.TenLoaiHinhKhuyenMai?.ToLower().Contains(search) == true)
            ).OrderBy(x => x.Stt).ThenBy(x => x.Name).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns: STT(40), Đợt khuyến mại(Star), Từ ngày(110), Đến ngày(110), Loại hình(220)
            stackTable.Children.Add(CreateRowDotKhuyenMai("STT", "Đợt khuyến mại", "Từ ngày", "Đến ngày", "Loại hình", isHeader: true, isSummary: false));

            int stt = 1;
            foreach (var item in filtered)
            {
                string tuNgayStr = item.Tungay?.ToString("dd/MM/yyyy") ?? "";
                string denNgayStr = item.Denngay?.ToString("dd/MM/yyyy") ?? "";

                stackTable.Children.Add(CreateRowDotKhuyenMai(
                    stt: (stt++).ToString(),
                    name: item.Name ?? "",
                    tuNgay: tuNgayStr,
                    denNgay: denNgayStr,
                    loaiHinh: item.TenLoaiHinhKhuyenMai ?? "",
                    isHeader: false,
                    isSummary: false
                ));
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateRowDotKhuyenMai(string stt, string name, string tuNgay, string denNgay, string loaiHinh, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : 22 };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Đợt khuyến mại
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Từ ngày
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Đến ngày
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) }); // Loại hình

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(name, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tuNgay, 2, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(denNgay, 3, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(loaiHinh, 4, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight, bool isHeader, bool isSummary)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 3, 6, 3),
                Background = isSummary 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bbfe6")) 
                    : (isHeader ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9")) : Brushes.White)
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text ?? "",
                FontSize = isHeader ? 10.5 : 10,
                FontWeight = weight,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            return border;
        }

        #region Event Handlers

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderReportTable();
        }

        private void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }

        private void PrintReport()
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(A4PageBorder, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"{TxtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("STT,Đợt khuyến mại,Từ ngày,Đến ngày,Loại hình");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        string tuNgayStr = item.Tungay?.ToString("dd/MM/yyyy") ?? "";
                        string denNgayStr = item.Denngay?.ToString("dd/MM/yyyy") ?? "";
                        sb.AppendLine($"\"{stt++}\",\"{item.Name?.Replace("\"", "\"\"")}\",\"{tuNgayStr}\",\"{denNgayStr}\",\"{item.TenLoaiHinhKhuyenMai?.Replace("\"", "\"\"")}\"");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
