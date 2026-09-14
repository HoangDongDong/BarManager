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
using static QuanLyBar.Client.Services.LocalBaoCaoKhoHangService;

namespace QuanLyBar.Client.Views.BaoCaoKhoHang
{
    public partial class BaoCaoTongHopXntControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private bool _isLoaded = false;
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;
        private List<BaoCaoTongHopXntItem> _allData = new List<BaoCaoTongHopXntItem>();

        public BaoCaoTongHopXntControl()
        {
            InitializeComponent();
        }

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            if (p == null) return;
            _pendingParams = p;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            SelectCombo(CboKhoHang, p.KhoId);
            SelectCombo(CboNhomHang, p.NhomHangId);
            SelectCombo(CboMatHang, p.MatHangId);

            if (_isLoaded)
            {
                _ = LoadDataAsync();
            }
        }

        private void SelectCombo(ComboBox cbo, string? id)
        {
            if (string.IsNullOrEmpty(id) || cbo == null) return;
            cbo.SelectedValue = id;
            if (cbo.SelectedItem == null && cbo.ItemsSource != null)
            {
                foreach (var item in cbo.ItemsSource)
                {
                    if (item is FilterComboItem fi && fi.Id == id)
                    {
                        cbo.SelectedItem = fi;
                        break;
                    }
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = new DateTime(today.Year, today.Month, 1);
            DpDenNgay.SelectedDate = today;
            TxtSignDate.Text = $"Ngày {today:dd} tháng {today:MM} năm {today:yyyy}";

            await LoadTemplateConfigAsync();
            BuildTableHeader();
            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
            if (_pendingParams != null)
            {
                ApplyFilterParams(_pendingParams);
            }
            await LoadDataAsync();
        }

        private async Task LoadTemplateConfigAsync(QuanLyBar.Client.Services.ReportPaperLayout? immediateLayout = null)
        {
            try
            {
                if (immediateLayout == null)
                {
                    var fullConfig = await QuanLyBar.Client.Services.ReportTemplateConfigService.GetFullTemplateConfigAsync(TxtReportTitle.Text);
                    _colConfigs = fullConfig.Columns;
                    _currentLayout = fullConfig.Layout;
                }
                else
                {
                    _currentLayout = immediateLayout;
                }
                ReportPaperLayoutHelper.ApplyReportPaperLayout(
                    reportPaper: ReportPaper,
                    layout: _currentLayout,
                    colLogo: ColLogo,
                    brdLogo: BrdLogo,
                    pnlCompanyText: PnlCompanyText,
                    txtCompanyName: TxtCompanyName,
                    txtCompanyAddress: TxtCompanyAddress,
                    txtCompanyContact: TxtCompanyContact,
                    gridTitleArea: GridReportTitleArea,
                    colTitleLeft: ColTitleLeft,
                    colTitleRight: ColTitleRight,
                    txtReportTitle: TxtReportTitle,
                    pnlDateAndFilter: PnlDateAndFilter,
                    txtSubTitleDate: TxtSubTitleDate,
                    txtFilterSummary: TxtFilterSummary,
                    gridSignatures: GridSignatures,
                    sigCol0: SigCol0,
                    sigCol1: SigCol1,
                    sigCol2: SigCol2,
                    sigCol3: SigCol3,
                    sigBlock0: SigBlock0,
                    sigBlock1: SigBlock1,
                    sigBlock2: SigBlock2,
                    sigBlock3: SigBlock3
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadTemplateConfigAsync error: " + ex.Message);
            }
        }

        private (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[] GetColumnDefinitions()
        {
            return new (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[]
            {
                ("STT", "STT", 35, HorizontalAlignment.Center),
                ("MaHang", "Mã hàng", 75, HorizontalAlignment.Center),
                ("TenHang", "Tên hàng hóa", 180, HorizontalAlignment.Left),
                ("DVT", "ĐVT", 50, HorizontalAlignment.Center),
                ("TonDau", "Tồn đầu", 60, HorizontalAlignment.Right),
                ("Nhap", "Nhập", 60, HorizontalAlignment.Right),
                ("Xuat", "Xuất", 60, HorizontalAlignment.Right),
                ("TonCuoi", "Tồn cuối", 65, HorizontalAlignment.Right),
                ("GiaVon", "Giá vốn", 95, HorizontalAlignment.Right),
                ("TienTon", "Tiền tồn", 105, HorizontalAlignment.Right)
            };
        }

        private void BuildTableHeader()
        {
            double hFontSize = _currentLayout?.HeaderFontSize ?? 11.0;
            var scaledCols = _colConfigs.GetScaledColumns(_currentLayout, GetColumnDefinitions());

            Grid headerGrid = new Grid();
            for (int i = 0; i < scaledCols.Count; i++)
            {
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(scaledCols[i].ScaledWidth) });
                headerGrid.Children.Add(CreateHeaderCell(scaledCols[i].Caption, i, hFontSize, isLast: (i == scaledCols.Count - 1)));
            }

            BrdTableHeader.Child = headerGrid;
        }

        private Border CreateHeaderCell(string text, int col, double fontSize, bool isLast = false)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0),
                Padding = new Thickness(4, 6, 4, 6)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            return b;
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

        private async Task LoadFiltersAsync()
        {
            try
            {
                var khoList = await _reportService.GetKhoHangFilterAsync();
                CboKhoHang.ItemsSource = khoList;
                CboKhoHang.SelectedIndex = 0;

                var nhomList = await _reportService.GetNhomMatHangFilterAsync();
                CboNhomHang.ItemsSource = nhomList;
                CboNhomHang.SelectedIndex = 0;

                var mhList = await _reportService.GetMatHangFilterAsync();
                CboMatHang.ItemsSource = mhList;
                CboMatHang.SelectedIndex = 0;
            }
            catch { }
        }

        private Border CreateCell(string text, int col, HorizontalAlignment align = HorizontalAlignment.Left, bool isBold = false, bool isLast = false)
        {
            double cFontSize = _currentLayout?.CellFontSize ?? 10.5;
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0),
                Padding = new Thickness(4, 0, 4, 0)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                FontSize = cFontSize,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            return b;
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string khoId = (CboKhoHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string mhId = (CboMatHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            string khoName = (CboKhoHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nhomName = (CboNhomHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string mhName = (CboMatHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khoName)) parts.Add($"Kho hàng: {khoName}");
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm: {nhomName}");
            if (Utilities.IsSpecificFilter(mhName)) parts.Add($"Mặt hàng: {mhName}");
            if (parts.Count > 0)
            {
                TxtFilterSummary.Text = string.Join("\n", parts);
                TxtFilterSummary.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                TxtFilterSummary.Text = "";
                TxtFilterSummary.Visibility = System.Windows.Visibility.Collapsed;
            }

            _allData = await _reportService.GetBaoCaoTongHopXntAsync(tuNgay, denNgay, khoId, nhomId, mhId);
            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();

            var filtered = _allData;
            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = _allData.Where(x =>
                    x.MaHang.ToLower().Contains(keyword) ||
                    x.TenHang.ToLower().Contains(keyword) ||
                    x.DVT.ToLower().Contains(keyword)
                ).ToList();
            }

            var scaledCols = _colConfigs.GetScaledColumns(_currentLayout, GetColumnDefinitions());

            decimal totalTonDau = 0;
            decimal totalNhap = 0;
            decimal totalXuat = 0;
            decimal totalTonCuoi = 0;
            decimal totalTienTon = 0;
            int stt = 1;

            foreach (var item in filtered)
            {
                item.STT = stt++;
                totalTonDau += item.TonDau;
                totalNhap += item.Nhap;
                totalXuat += item.Xuat;
                totalTonCuoi += item.TonCuoi;
                totalTienTon += item.TienTon;

                var rowValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "STT", item.STT.ToString() },
                    { "MaHang", item.MaHang ?? "" },
                    { "TenHang", item.TenHang ?? "" },
                    { "DVT", item.DVT ?? "" },
                    { "TonDau", item.TonDau.ToString("#,##0.##") },
                    { "Nhap", item.Nhap.ToString("#,##0.##") },
                    { "Xuat", item.Xuat.ToString("#,##0.##") },
                    { "TonCuoi", item.TonCuoi.ToString("#,##0.##") },
                    { "GiaVon", item.GiaVon.ToString("#,##0") },
                    { "TienTon", item.TienTon.ToString("#,##0") }
                };

                Border rowBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 26
                };

                Grid rowGrid = new Grid();
                for (int i = 0; i < scaledCols.Count; i++)
                {
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(scaledCols[i].ScaledWidth) });
                    var col = scaledCols[i];
                    string val = rowValues.TryGetValue(col.Key, out var v) ? v : "";
                    rowGrid.Children.Add(CreateCell(val, i, col.Align, isLast: (i == scaledCols.Count - 1)));
                }

                rowBorder.Child = rowGrid;
                StkDataRows.Children.Add(rowBorder);
            }

            // Total
            Border sumBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 0, 1, 1),
                Height = 28
            };
            Grid sumGrid = new Grid();
            for (int i = 0; i < scaledCols.Count; i++)
            {
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(scaledCols[i].ScaledWidth) });
            }

            int firstValIndex = scaledCols.FindIndex(c => c.Key.Equals("TonDau", StringComparison.OrdinalIgnoreCase) || c.Key.Equals("Nhap", StringComparison.OrdinalIgnoreCase));
            if (firstValIndex < 0) firstValIndex = Math.Min(4, scaledCols.Count);

            if (firstValIndex > 0)
            {
                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, firstValIndex);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontWeight = FontWeights.Bold,
                    FontSize = _currentLayout?.CellFontSize ?? 10.5,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);
            }

            var sumValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TonDau", totalTonDau.ToString("#,##0.##") },
                { "Nhap", totalNhap.ToString("#,##0.##") },
                { "Xuat", totalXuat.ToString("#,##0.##") },
                { "TonCuoi", totalTonCuoi.ToString("#,##0.##") },
                { "GiaVon", "" },
                { "TienTon", totalTienTon.ToString("#,##0") }
            };

            for (int i = firstValIndex; i < scaledCols.Count; i++)
            {
                var col = scaledCols[i];
                string val = sumValues.TryGetValue(col.Key, out var v) ? v : "";
                sumGrid.Children.Add(CreateCell(val, i, col.Align, isBold: true, isLast: (i == scaledCols.Count - 1)));
            }

            sumBorder.Child = sumGrid;
            StkDataRows.Children.Add(sumBorder);
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderTable();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chế độ xem trước trang in đã sẵn sàng trên màn hình.", "Xem trước", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "Báo cáo tổng hợp xuất nhập tồn");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV UTF-8 (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"BaoCaoTongHopXNT_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();

                    sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Tồn đầu,Nhập,Xuất,Tồn cuối,Giá vốn,Tiền tồn");
                    foreach (var i in _allData)
                    {
                        sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.TonDau},{i.Nhap},{i.Xuat},{i.TonCuoi},{i.GiaVon},{i.TienTon}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.InAn.ThietKeMauInWindow(
                    TxtReportTitle.Text,
                    null,
                    async (cols) =>
                    {
                        await LoadTemplateConfigAsync();
                        BuildTableHeader();
                        await LoadDataAsync();
                    },
                    onLayoutCallback: async (layout) =>
                    {
                        await LoadTemplateConfigAsync(layout);
                        BuildTableHeader();
                        RenderTable();
                    });
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở thiết kế mẫu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThamSoTuyChinh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.InAn.TuyChonThamSoBaoCaoWindow(
                    TxtReportTitle.Text,
                    async () =>
                    {
                        await LoadFiltersAsync();
                        await LoadDataAsync();
                    });
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tùy chọn tham số: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXemDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (InlineDataBorder.Visibility == System.Windows.Visibility.Visible)
            {
                InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

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
                InlineDataGrid.ItemsSource = new[] { new { ThongBao = "Không có dữ liệu. Hãy tải dữ liệu trước (F5) rồi mở lại." } };

            TxtSoBanGhi.Text = $"Tổng số: {maxCount} bản ghi";
            InlineDataBorder.Visibility = System.Windows.Visibility.Visible;

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
