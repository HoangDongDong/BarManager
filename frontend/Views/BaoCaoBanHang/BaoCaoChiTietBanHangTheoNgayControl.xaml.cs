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
    public partial class BaoCaoChiTietBanHangTheoNgayControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<BaoCaoChiTietBanHangOrderModel> _rawOrders = new List<BaoCaoChiTietBanHangOrderModel>();
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            _pendingParams = p;
            if (!_isLoaded) return;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            _ = LoadDataAsync();
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

        public BaoCaoChiTietBanHangTheoNgayControl(string reportType = "BÁO CÁO CHI TIẾT BÁN HÀNG THEO NGÀY")
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "BÁO CÁO CHI TIẾT BÁN HÀNG THEO NGÀY" : reportType.Trim().ToUpper();

            TxtReportTitle.Text = _reportType;

            // Short-cut keys: F5 (Tải dữ liệu), F3 (Lọc/Tìm kiếm), F12 (Excel), Ctrl+P (In), Ctrl+Shift+P (Xem)
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
            DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
            DpDenNgay.SelectedDate = now;

            await LoadTemplateConfigAsync();
            await LoadCompanyInfoAndLogoAsync();
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

        private async Task LoadDataAsync()
        {
            try
            {
                DateTime tuNgay = DpTuNgay.SelectedDate ?? DateTime.Today.AddDays(-30);
                DateTime denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;

                TxtSubTitleDate.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";

                _rawOrders = await _hoaDonService.GetBaoCaoChiTietBanHangTheoNgayAsync(tuNgay, denNgay);
                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[] GetColumnDefinitions()
        {
            return new (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[]
            {
                ("SoPhieu", "Số phiếu", 75, HorizontalAlignment.Left),
                ("MatHangBan", "Mặt hàng bán", 140, HorizontalAlignment.Left),
                ("Sl", "SL", 35, HorizontalAlignment.Right),
                ("DonGia", "Đơn giá", 65, HorizontalAlignment.Right),
                ("PtCk", "% CK", 45, HorizontalAlignment.Right),
                ("TienGiamMh", "Tiền giảm MH", 75, HorizontalAlignment.Right),
                ("ThanhTien", "Thành tiền", 80, HorizontalAlignment.Right),
                ("TienHang", "Tiền hàng", 80, HorizontalAlignment.Right),
                ("TienGio", "Tiền giờ", 65, HorizontalAlignment.Right),
                ("GiamTongBill", "Giảm tổng bill", 75, HorizontalAlignment.Right),
                ("PhiDv", "Phí dv", 55, HorizontalAlignment.Right),
                ("Thue", "Thuế", 50, HorizontalAlignment.Right),
                ("TongCong", "Tổng cộng", 85, HorizontalAlignment.Right),
                ("ThanhToan", "Thanh toán", 85, HorizontalAlignment.Right),
                ("ConNo", "Còn nợ", 65, HorizontalAlignment.Right)
            };
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";

            var filtered = _rawOrders.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                (x.Items.Any(i => i.MatHangBan?.ToLower().Contains(search) == true))
            ).ToList();

            var scaledCols = _colConfigs.GetScaledColumns(_currentLayout, GetColumnDefinitions());

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Header row
            stackTable.Children.Add(CreateHeaderRow(scaledCols));

            var dateGroups = filtered.GroupBy(x => x.NgayDisplay).OrderBy(g => g.Key).ToList();

            foreach (var group in dateGroups)
            {
                // Date Header Row: Ngày dd/MM/yyyy
                stackTable.Children.Add(CreateDateHeaderRow(scaledCols, $"Ngày {group.Key}"));

                foreach (var order in group.OrderBy(x => x.SoPhieu))
                {
                    stackTable.Children.Add(CreateOrderGrid(scaledCols, order));
                }
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateHeaderRow(List<QuanLyBar.Client.Services.ReportColumnInfo> cols)
        {
            var grid = new Grid { MinHeight = 26 };
            AddColumnDefinitions(grid, cols);

            Brush bg = (Brush)new BrushConverter().ConvertFromString("#f0f0f0");

            for (int i = 0; i < cols.Count; i++)
            {
                AddCellToGrid(grid, 0, i, cols[i].Caption, HorizontalAlignment.Center, isBold: true, bg: bg);
            }

            return grid;
        }

        private UIElement CreateDateHeaderRow(List<QuanLyBar.Client.Services.ReportColumnInfo> cols, string title)
        {
            var grid = new Grid { MinHeight = 24 };
            AddColumnDefinitions(grid, cols);

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5, 4, 5, 4),
                Background = Brushes.White
            };
            Grid.SetColumn(border, 0);
            Grid.SetColumnSpan(border, cols.Count);

            var txt = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = _currentLayout?.HeaderFontSize ?? 11
            };
            border.Child = txt;
            grid.Children.Add(border);

            return grid;
        }

        private UIElement CreateOrderGrid(List<QuanLyBar.Client.Services.ReportColumnInfo> cols, BaoCaoChiTietBanHangOrderModel order)
        {
            var items = order.Items ?? new List<BaoCaoChiTietBanHangItemModel>();
            int rowCount = Math.Max(1, items.Count);

            var grid = new Grid();
            AddColumnDefinitions(grid, cols);

            for (int r = 0; r < rowCount; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Map values per column
            for (int colIndex = 0; colIndex < cols.Count; colIndex++)
            {
                var col = cols[colIndex];
                if (col.Key.Equals("SoPhieu", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.SoPhieu ?? "", col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("TienHang", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.TienHang.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("TienGio", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.TienGio.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("GiamTongBill", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.GiamTongBill.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("PhiDv", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.PhiDv.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("Thue", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.Thue.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("TongCong", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.TongCong.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("ThanhToan", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.ThanhToan.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else if (col.Key.Equals("ConNo", StringComparison.OrdinalIgnoreCase))
                {
                    AddCellToGrid(grid, 0, colIndex, order.ConNo.ToString("#,##0"), col.Align, rowSpan: rowCount);
                }
                else
                {
                    // Item row level columns
                    for (int i = 0; i < rowCount; i++)
                    {
                        string val = "";
                        if (i < items.Count)
                        {
                            var item = items[i];
                            if (col.Key.Equals("MatHangBan", StringComparison.OrdinalIgnoreCase)) val = item.MatHangBan ?? "";
                            else if (col.Key.Equals("Sl", StringComparison.OrdinalIgnoreCase)) val = item.Sl.ToString("#,##0.##");
                            else if (col.Key.Equals("DonGia", StringComparison.OrdinalIgnoreCase)) val = item.DonGia.ToString("#,##0");
                            else if (col.Key.Equals("PtCk", StringComparison.OrdinalIgnoreCase)) val = item.PtCk.ToString("#,##0");
                            else if (col.Key.Equals("TienGiamMh", StringComparison.OrdinalIgnoreCase)) val = item.TienGiamMh.ToString("#,##0");
                            else if (col.Key.Equals("ThanhTien", StringComparison.OrdinalIgnoreCase)) val = item.ThanhTien.ToString("#,##0");
                        }
                        AddCellToGrid(grid, i, colIndex, val, col.Align);
                    }
                }
            }

            return grid;
        }

        private void AddColumnDefinitions(Grid grid, List<QuanLyBar.Client.Services.ReportColumnInfo> cols)
        {
            foreach (var col in cols)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(col.ScaledWidth) });
            }
        }

        private void AddCellToGrid(Grid grid, int row, int col, string text, HorizontalAlignment align, bool isBold = false, Brush? bg = null, int rowSpan = 1)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(3, 3, 3, 3),
                Background = bg ?? Brushes.White
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            if (rowSpan > 1)
            {
                Grid.SetRowSpan(border, rowSpan);
            }

            var txt = new TextBlock
            {
                Text = text,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = isBold ? (_currentLayout?.HeaderFontSize ?? 10) : (_currentLayout?.CellFontSize ?? 10),
                TextWrapping = TextWrapping.Wrap
            };
            border.Child = txt;
            grid.Children.Add(border);
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            _ = LoadDataAsync();
        }

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

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void PrintReport()
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "In Báo Cáo Chi Tiết Bán Hàng Theo Ngày");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"BaoCaoChiTietBanHangTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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
                    sb.AppendLine("");

                    sb.AppendLine("Ngày,Số phiếu,Mặt hàng bán,SL,Đơn giá,% CK,Tiền giảm MH,Thành tiền,Tiền hàng,Tiền giờ,Giảm tổng bill,Phí dv,Thuế,Tổng cộng,Thanh toán,Còn nợ");

                    string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
                    var filtered = _rawOrders.Where(x =>
                        string.IsNullOrEmpty(search) ||
                        (x.SoPhieu?.ToLower().Contains(search) == true) ||
                        (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                        (x.Items.Any(i => i.MatHangBan?.ToLower().Contains(search) == true))
                    ).ToList();

                    var dateGroups = filtered.GroupBy(x => x.NgayDisplay).OrderBy(g => g.Key).ToList();

                    foreach (var g in dateGroups)
                    {
                        sb.AppendLine($"\"Ngày {g.Key}\",,,,,,,,,,,,,,,");
                        foreach (var order in g.OrderBy(x => x.SoPhieu))
                        {
                            var items = order.Items ?? new List<BaoCaoChiTietBanHangItemModel>();
                            if (items.Count == 0)
                            {
                                sb.AppendLine($"\"\",\"{order.SoPhieu}\",\"\",0,0,0,0,0,\"{order.TienHang}\",\"{order.TienGio}\",\"{order.GiamTongBill}\",\"{order.PhiDv}\",\"{order.Thue}\",\"{order.TongCong}\",\"{order.ThanhToan}\",\"{order.ConNo}\"");
                            }
                            else
                            {
                                for (int i = 0; i < items.Count; i++)
                                {
                                    var item = items[i];
                                    if (i == 0)
                                    {
                                        sb.AppendLine($"\"\",\"{order.SoPhieu}\",\"{item.MatHangBan}\",\"{item.Sl}\",\"{item.DonGia}\",\"{item.PtCk}\",\"{item.TienGiamMh}\",\"{item.ThanhTien}\",\"{order.TienHang}\",\"{order.TienGio}\",\"{order.GiamTongBill}\",\"{order.PhiDv}\",\"{order.Thue}\",\"{order.TongCong}\",\"{order.ThanhToan}\",\"{order.ConNo}\"");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"\"\",\"\",\"{item.MatHangBan}\",\"{item.Sl}\",\"{item.DonGia}\",\"{item.PtCk}\",\"{item.TienGiamMh}\",\"{item.ThanhTien}\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"");
                                    }
                                }
                            }
                        }
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất CSV: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    RenderReportTable();
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
