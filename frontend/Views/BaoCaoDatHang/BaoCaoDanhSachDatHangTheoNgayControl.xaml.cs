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
using static QuanLyBar.Client.Services.LocalBaoCaoDatHangService;

namespace QuanLyBar.Client.Views.BaoCaoDatHang
{
    public partial class BaoCaoDanhSachDatHangTheoNgayControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        private readonly LocalBaoCaoDatHangService _reportService = new LocalBaoCaoDatHangService();
        private bool _isLoaded = false;
        private List<BaoCaoDanhSachDatHangTheoNgayItem> _allData = new List<BaoCaoDanhSachDatHangTheoNgayItem>();
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            _pendingParams = p;
            if (!_isLoaded) return;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            SelectCombo(CboKhachHang, p.KhachHangId);
            SelectCombo(CboNhanVien, p.NhanVienId);

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
                    if (item is FilterComboItem fi && fi.Id == id)
                    {
                        cbo.SelectedItem = fi;
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

        public BaoCaoDanhSachDatHangTheoNgayControl(string tabName = "DANH SÁCH ĐẶT HÀNG THEO NGÀY")
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = today;
            DpDenNgay.SelectedDate = today;

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
                var khList = await _reportService.GetKhachHangFilterAsync();
                CboKhachHang.ItemsSource = khList;
                CboKhachHang.SelectedIndex = 0;

                var nhomKhList = await _reportService.GetNhomKhachHangFilterAsync();
                CboNhomKhachHang.ItemsSource = nhomKhList;
                CboNhomKhachHang.SelectedIndex = 0;

                var nvList = await _reportService.GetNhanVienFilterAsync();
                CboNhanVien.ItemsSource = nvList;
                CboNhanVien.SelectedIndex = 0;
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string khId = (CboKhachHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomKhId = (CboNhomKhachHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string nvId = (CboNhanVien.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string nhomName = (CboNhomKhachHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nvName = (CboNhanVien.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm KH: {nhomName}");
            if (Utilities.IsSpecificFilter(nvName)) parts.Add($"Nhân viên: {nvName}");
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

            _allData = await _reportService.GetDanhSachDatHangTheoNgayAsync(tuNgay, denNgay, khId, nhomKhId, nvId);
            RenderTable();
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
            RenderTable();
        }

        private (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[] GetColumnDefinitions()
        {
            return new (string Key, string DefaultCaption, double BaseWidth, HorizontalAlignment DefaultAlign)[]
            {
                ("STT", "STT", 40, HorizontalAlignment.Center),
                ("Ngay", "Ngày", 95, HorizontalAlignment.Center),
                ("SoPhieu", "Số phiếu", 110, HorizontalAlignment.Center),
                ("KhachHang", "Khách hàng", 140, HorizontalAlignment.Left),
                ("DiaChi", "Địa chỉ", 120, HorizontalAlignment.Left),
                ("DienThoai", "Điện thoại", 95, HorizontalAlignment.Left),
                ("TienHang", "Tiền hàng", 100, HorizontalAlignment.Right),
                ("GiamGia", "Giảm giá", 90, HorizontalAlignment.Right),
                ("TongCong", "Tổng cộng", 110, HorizontalAlignment.Right)
            };
        }

        private void RenderTable()
        {
            TableContainer.Children.Clear();

            string keyword = TxtFilter.Text.Trim().ToLower();
            var filtered = _allData;
            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = _allData.Where(x =>
                    x.SoPhieu.ToLower().Contains(keyword) ||
                    x.KhachHang.ToLower().Contains(keyword) ||
                    x.DiaChi.ToLower().Contains(keyword) ||
                    x.DienThoai.ToLower().Contains(keyword)
                ).ToList();
            }

            var scaledCols = _colConfigs.GetScaledColumns(_currentLayout, GetColumnDefinitions());

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Header row
            stackTable.Children.Add(CreateDataRow(scaledCols, null, isHeader: true, isSummary: false));

            decimal totalTienHang = 0;
            decimal totalGiamGia = 0;
            decimal totalTongCong = 0;

            int stt = 1;
            foreach (var item in filtered)
            {
                item.STT = stt++;
                totalTienHang += item.TienHang;
                totalGiamGia += item.GiamGia;
                totalTongCong += item.TongCong;

                var rowValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "STT", item.STT.ToString() },
                    { "Ngay", item.NgayDisplay },
                    { "SoPhieu", item.SoPhieu },
                    { "KhachHang", item.KhachHang },
                    { "DiaChi", item.DiaChi },
                    { "DienThoai", item.DienThoai },
                    { "TienHang", item.TienHang > 0 ? item.TienHang.ToString("#,##0") : "0" },
                    { "GiamGia", item.GiamGia > 0 ? item.GiamGia.ToString("#,##0") : "0" },
                    { "TongCong", item.TongCong > 0 ? item.TongCong.ToString("#,##0") : "0" }
                };

                stackTable.Children.Add(CreateDataRow(scaledCols, rowValues, isHeader: false, isSummary: false));
            }

            // Summary row: TỔNG CỘNG
            var summaryValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "STT", "" },
                { "Ngay", "" },
                { "SoPhieu", "" },
                { "KhachHang", "" },
                { "DiaChi", "" },
                { "DienThoai", "" },
                { "TienHang", totalTienHang > 0 ? totalTienHang.ToString("#,##0") : "0" },
                { "GiamGia", totalGiamGia > 0 ? totalGiamGia.ToString("#,##0") : "0" },
                { "TongCong", totalTongCong > 0 ? totalTongCong.ToString("#,##0") : "0" }
            };
            stackTable.Children.Add(CreateDataRow(scaledCols, summaryValues, isHeader: false, isSummary: true));

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateDataRow(List<QuanLyBar.Client.Services.ReportColumnInfo> cols, Dictionary<string, string>? values, bool isHeader = false, bool isSummary = false)
        {
            var grid = new Grid { MinHeight = isHeader ? 26 : 24 };
            foreach (var col in cols)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(col.ScaledWidth) });
            }

            if (isSummary)
            {
                int firstValueColIndex = cols.FindIndex(c => c.Key.Equals("TienHang", StringComparison.OrdinalIgnoreCase) || c.Key.Equals("GiamGia", StringComparison.OrdinalIgnoreCase) || c.Key.Equals("TongCong", StringComparison.OrdinalIgnoreCase));
                if (firstValueColIndex < 0) firstValueColIndex = Math.Min(6, cols.Count);

                if (firstValueColIndex > 0)
                {
                    var summaryLabelBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Padding = new Thickness(5, 4, 8, 4),
                        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
                    };
                    Grid.SetColumn(summaryLabelBorder, 0);
                    Grid.SetColumnSpan(summaryLabelBorder, firstValueColIndex);
                    var txtSummary = new TextBlock
                    {
                        Text = "TỔNG CỘNG",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.Black
                    };
                    summaryLabelBorder.Child = txtSummary;
                    grid.Children.Add(summaryLabelBorder);
                }

                for (int i = firstValueColIndex; i < cols.Count; i++)
                {
                    var col = cols[i];
                    string val = values != null && values.TryGetValue(col.Key, out var v) ? v : "";
                    var cellBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Padding = new Thickness(4, 3, 4, 3),
                        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
                    };
                    Grid.SetColumn(cellBorder, i);

                    var txt = new TextBlock
                    {
                        Text = val,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = col.Align,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.Black
                    };
                    cellBorder.Child = txt;
                    grid.Children.Add(cellBorder);
                }
                return grid;
            }

            for (int i = 0; i < cols.Count; i++)
            {
                var col = cols[i];
                var cellBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(4, 3, 4, 3),
                    Background = isHeader ? new SolidColorBrush(Color.FromRgb(240, 240, 240)) : Brushes.White
                };
                Grid.SetColumn(cellBorder, i);

                string val = isHeader ? col.Caption : (values != null && values.TryGetValue(col.Key, out var v) ? v : "");
                var txt = new TextBlock
                {
                    Text = val,
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                    HorizontalAlignment = isHeader ? HorizontalAlignment.Center : col.Align,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                };
                cellBorder.Child = txt;
                grid.Children.Add(cellBorder);
            }

            return grid;
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(ReportPaper, "Danh sách đặt hàng theo ngày");
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(ReportPaper, "In danh sách đặt hàng theo ngày");
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"DanhSachDatHangTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("DANH SÁCH ĐẶT HÀNG THEO NGÀY");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("STT,Ngay,SoPhieu,KhachHang,DiaChi,DienThoai,TienHang,GiamGia,TongCong");
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{item.STT}\",\"{item.NgayDisplay}\",\"{item.SoPhieu}\",\"{item.KhachHang}\",\"{item.DiaChi}\",\"{item.DienThoai}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    RenderTable();
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
