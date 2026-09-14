using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoBanHang
{
    public partial class BaoCaoMatHangBanTheoNgayControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<TongHopMatHangBanItem> _rawItems = new List<TongHopMatHangBanItem>();
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            _pendingParams = p;
            if (!_isLoaded) return;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            SelectCombo(CboKhoXuat, p.KhoId);
            SelectCombo(CboNhanVienXuat, p.NhanVienId);

            _ = LoadDataAsync();
        }

        private void SelectCombo(ComboBox cbo, string? id)
        {
            if (string.IsNullOrEmpty(id) || cbo == null) return;
            cbo.SelectedValue = id;
            if (cbo.SelectedItem == null && cbo.ItemsSource != null)
            {
                foreach (var item in cbo.ItemsSource)
                {
                    if (item is ComboLookupItem ci && ci.Id == id)
                    {
                        cbo.SelectedItem = ci;
                        break;
                    }
                }
            }
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

        public BaoCaoMatHangBanTheoNgayControl(string reportType = "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY")
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY" : reportType.Trim().ToUpper();

            TxtReportTitle.Text = _reportType;

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

            var now = DateTime.Now;
            DpTuNgay.SelectedDate = now;
            DpDenNgay.SelectedDate = now;

            await LoadTemplateConfigAsync();
            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
            if (_pendingParams != null)
            {
                ApplyFilterParams(_pendingParams);
            }
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

        private async Task LoadFiltersAsync()
        {
            try
            {
                var listKho = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "📦" }
                };
                try
                {
                    var khoItems = await LocalKhoHangService.GetKhoHangTreeAsync(false);
                    foreach (var k in khoItems)
                    {
                        if (k.IsWarehouse)
                        {
                            listKho.Add(new ComboLookupItem { Id = k.Id ?? "", Name = k.Name ?? "", Icon = "📦" });
                        }
                    }
                }
                catch { }
                CboKhoXuat.ItemsSource = listKho;
                CboKhoXuat.SelectedIndex = 0;

                var listNv = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "👤" }
                };
                try
                {
                    var nvItems = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                    foreach (var nv in nvItems)
                    {
                        listNv.Add(new ComboLookupItem { Id = nv.Id ?? "", Name = nv.Name ?? "", Icon = "👤" });
                    }
                }
                catch { }
                CboNhanVienXuat.ItemsSource = listNv;
                CboNhanVienXuat.SelectedIndex = 0;
            }
            catch { }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            _ = LoadDataAsync();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
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

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                DateTime tuNgay = DpTuNgay.SelectedDate ?? DateTime.Now.Date;
                DateTime denNgay = DpDenNgay.SelectedDate ?? DateTime.Now.Date;

                string? khoId = (CboKhoXuat.SelectedItem as ComboLookupItem)?.Id;
                string? nvId = (CboNhanVienXuat.SelectedItem as ComboLookupItem)?.Id;

                TxtSubTitleDate.Text = tuNgay.Date == denNgay.Date
                    ? $"Ngày: {tuNgay:dd/MM/yyyy}"
                    : $"Từ ngày: {tuNgay:dd/MM/yyyy} đến ngày {denNgay:dd/MM/yyyy}";

                string khoStr = string.IsNullOrEmpty(khoId) ? "Tất cả" : ((CboKhoXuat.SelectedItem as ComboLookupItem)?.Name ?? "Tất cả");
                string nvStr = string.IsNullOrEmpty(nvId) ? "Tất cả" : ((CboNhanVienXuat.SelectedItem as ComboLookupItem)?.Name ?? "Tất cả");
                TxtFilterSummary.Text = $"Kho xuất: {khoStr} | NV xuất: {nvStr}";

                _rawItems = await _hoaDonService.GetTongHopMatHangBanTheoNgayAsync(tuNgay, denNgay, khoId, nvId)
                            ?? new List<TongHopMatHangBanItem>();

                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[] GetColumnDefinitions()
        {
            return new (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[]
            {
                ("STT", "STT", 50, HorizontalAlignment.Center),
                ("TenHang", "Tên hàng", 220, HorizontalAlignment.Left),
                ("Dvt", "ĐVT", 80, HorizontalAlignment.Center),
                ("SoLuong", "Số lượng", 90, HorizontalAlignment.Right),
                ("DonGia", "Đơn giá", 110, HorizontalAlignment.Right),
                ("GiamGiaPhanTram", "Giảm giá %", 90, HorizontalAlignment.Right),
                ("ThanhTien", "Thành tiền", 140, HorizontalAlignment.Right)
            };
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            var baseDefs = GetColumnDefinitions();
            var scaledCols = _colConfigs.GetScaledColumns(_currentLayout, baseDefs);

            if (scaledCols.Count == 0) return;

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.TenNhom?.ToLower().Contains(search) == true) ||
                (x.TenHang?.ToLower().Contains(search) == true) ||
                (x.Dvt?.ToLower().Contains(search) == true)
            ).ToList();

            var grid = new Grid { Margin = new Thickness(0, 5, 0, 10) };
            foreach (var col in scaledCols)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(col.ScaledWidth) });
            }

            int rowIndex = 0;

            // Header Row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < scaledCols.Count; i++)
            {
                var col = scaledCols[i];
                var cell = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(i == 0 ? 1 : 0, 1, 1, 1),
                    Padding = new Thickness(4, 6, 4, 6)
                };
                var txt = new TextBlock
                {
                    Text = col.Caption,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = col.Align,
                    TextWrapping = TextWrapping.Wrap
                };
                cell.Child = txt;
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, i);
                grid.Children.Add(cell);
            }
            rowIndex++;

            var groups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.TenNhom) ? "KHÁC" : x.TenNhom)
                                 .OrderBy(g => g.Key)
                                 .ToList();

            decimal grandTotal = 0;

            foreach (var g in groups)
            {
                // Group Header Row
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var groupBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 247, 250)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };
                var groupTxt = new TextBlock
                {
                    Text = $"Nhóm hàng: {g.Key}",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 95))
                };
                groupBorder.Child = groupTxt;
                Grid.SetRow(groupBorder, rowIndex);
                Grid.SetColumn(groupBorder, 0);
                Grid.SetColumnSpan(groupBorder, scaledCols.Count);
                grid.Children.Add(groupBorder);
                rowIndex++;

                int stt = 1;
                decimal groupTotal = 0;

                foreach (var item in g.OrderBy(x => x.TenHang))
                {
                    groupTotal += item.ThanhTien;
                    grandTotal += item.ThanhTien;

                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    for (int i = 0; i < scaledCols.Count; i++)
                    {
                        var col = scaledCols[i];
                        var cell = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(i == 0 ? 1 : 0, 0, 1, 1),
                            Padding = new Thickness(4, 4, 4, 4)
                        };

                        string valStr = col.Key switch
                        {
                            "STT" => stt.ToString(),
                            "TenHang" => item.TenHang,
                            "Dvt" => item.Dvt,
                            "SoLuong" => item.SoLuong.ToString("N0"),
                            "DonGia" => item.DonGia.ToString("N0"),
                            "GiamGiaPhanTram" => item.GiamGiaPhanTram > 0 ? item.GiamGiaPhanTram.ToString("N0") : "",
                            "ThanhTien" => item.ThanhTien.ToString("N0"),
                            _ => ""
                        };

                        var txt = new TextBlock
                        {
                            Text = valStr,
                            HorizontalAlignment = col.Align,
                            TextWrapping = TextWrapping.Wrap
                        };
                        cell.Child = txt;
                        Grid.SetRow(cell, rowIndex);
                        Grid.SetColumn(cell, i);
                        grid.Children.Add(cell);
                    }
                    stt++;
                    rowIndex++;
                }

                // Group Total Row
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                for (int i = 0; i < scaledCols.Count; i++)
                {
                    var col = scaledCols[i];
                    var cell = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(i == 0 ? 1 : 0, 0, 1, 1),
                        Padding = new Thickness(4, 4, 4, 4)
                    };

                    string valStr = "";
                    if (i == 0) valStr = "Tổng cộng";
                    else if (col.Key == "ThanhTien") valStr = groupTotal.ToString("N0");

                    var txt = new TextBlock
                    {
                        Text = valStr,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = (i == 0) ? HorizontalAlignment.Left : col.Align
                    };
                    cell.Child = txt;
                    Grid.SetRow(cell, rowIndex);
                    Grid.SetColumn(cell, i);
                    grid.Children.Add(cell);
                }
                rowIndex++;
            }

            // Grand Total Row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < scaledCols.Count; i++)
            {
                var col = scaledCols[i];
                var cell = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(235, 240, 245)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(i == 0 ? 1 : 0, 0, 1, 1),
                    Padding = new Thickness(4, 6, 4, 6)
                };

                string valStr = "";
                if (i == 0) valStr = "TỔNG CỘNG";
                else if (col.Key == "ThanhTien") valStr = grandTotal.ToString("N0");

                var txt = new TextBlock
                {
                    Text = valStr,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = (i == 0) ? HorizontalAlignment.Left : col.Align
                };
                cell.Child = txt;
                Grid.SetRow(cell, rowIndex);
                Grid.SetColumn(cell, i);
                grid.Children.Add(cell);
            }

            TableContainer.Children.Add(grid);
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"TongHopMatHangBanTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtSubTitleDate.Text}\"");
                    sb.AppendLine($"\"{TxtFilterSummary.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("Nhóm hàng,STT,Tên hàng,ĐVT,Số lượng,Đơn giá,Giảm giá %,Thành tiền");

                    string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
                    var filtered = _rawItems.Where(x =>
                        string.IsNullOrEmpty(search) ||
                        (x.TenNhom?.ToLower().Contains(search) == true) ||
                        (x.TenHang?.ToLower().Contains(search) == true) ||
                        (x.Dvt?.ToLower().Contains(search) == true)
                    ).ToList();

                    var groups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.TenNhom) ? "KHÁC" : x.TenNhom).OrderBy(g => g.Key).ToList();

                    decimal grandTotal = 0;
                    foreach (var g in groups)
                    {
                        sb.AppendLine($"\"Nhóm hàng: {g.Key}\",,,,,,,");
                        int stt = 1;
                        decimal groupTotal = 0;
                        foreach (var item in g.OrderBy(x => x.TenHang))
                        {
                            groupTotal += item.ThanhTien;
                            grandTotal += item.ThanhTien;
                            sb.AppendLine($"\"\",\"{stt++}\",\"{item.TenHang}\",\"{item.Dvt}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.GiamGiaPhanTram}\",\"{item.ThanhTien}\"");
                        }
                        sb.AppendLine($"\"Tổng cộng\",,,,,,\"{groupTotal}\"");
                    }
                    sb.AppendLine($"\"TỔNG CỘNG\",,,,,,\"{grandTotal}\"");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất CSV: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReport()
        {
            try
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(ReportPaper, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var win = new QuanLyBar.Client.Views.InAn.ThietKeMauInWindow(
                TxtReportTitle.Text,
                null,
                onSaveCallback: async (cols) =>
                {
                    await LoadTemplateConfigAsync();
                    await LoadDataAsync();
                },
                onLayoutCallback: async (layout) =>
                {
                    await LoadTemplateConfigAsync(layout);
                    RenderReportTable();
                });
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnThamSoTuyChinh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var win = new QuanLyBar.Client.Views.InAn.TuyChonThamSoBaoCaoWindow(
                TxtReportTitle.Text,
                onSavedCallback: async () =>
                {
                    await LoadFiltersAsync();
                    await LoadDataAsync();
                });
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
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
                InlineDataGrid.ItemsSource = new[] { new { ThongBao = "Không có dữ liệu. Hãy tải dữ liệu trước (F5) roi mo lai." } };

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
