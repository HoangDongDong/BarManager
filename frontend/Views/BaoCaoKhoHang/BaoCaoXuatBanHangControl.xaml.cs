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
using static QuanLyBar.Client.Services.LocalBaoCaoKhoHangService;

namespace QuanLyBar.Client.Views.BaoCaoKhoHang
{
    public partial class BaoCaoXuatBanHangControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        public enum ReportMode
        {
            TheoNgay,
            TheoDon,
            DinhLuong
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;

        private List<BaoCaoXuatBanHangTheoNgayItem> _dataTheoNgay = new List<BaoCaoXuatBanHangTheoNgayItem>();
        private List<BaoCaoXuatBanHangTheoDonItem> _dataTheoDon = new List<BaoCaoXuatBanHangTheoDonItem>();
        private List<BaoCaoXuatBanHangDinhLuongItem> _dataDinhLuong = new List<BaoCaoXuatBanHangDinhLuongItem>();

        public BaoCaoXuatBanHangControl(ReportMode mode = ReportMode.TheoNgay)
        {
            InitializeComponent();
            _mode = mode;
            UpdateTitle();
        }

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            if (p == null) return;
            _pendingParams = p;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            SelectCombo(CboKhoHang, p.KhoXuatId ?? p.KhoId);
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

        private void UpdateTitle()
        {
            switch (_mode)
            {
                case ReportMode.TheoNgay:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG XUẤT BÁN THEO NGÀY";
                    break;
                case ReportMode.TheoDon:
                    TxtReportTitle.Text = "BÁO CÁO MẶT HÀNG BÁN THEO ĐƠN HÀNG";
                    break;
                case ReportMode.DinhLuong:
                    TxtReportTitle.Text = "BÁO CÁO XUẤT ĐỊNH LƯỢNG MẶT HÀNG THEO ĐƠN HÀNG";
                    break;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = today;
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

        private List<double> GetBaseColumnWidths()
        {
            switch (_mode)
            {
                case ReportMode.TheoNgay:
                    return new List<double> { 35, 80, 215, 60, 80, 110, 140 };
                case ReportMode.TheoDon:
                    return new List<double> { 35, 85, 100, 70, 70, 110, 80, 80, 90 };
                case ReportMode.DinhLuong:
                    return new List<double> { 35, 80, 215, 60, 100, 100, 130 };
                default:
                    return new List<double> { 35, 80, 215, 60, 80, 110, 140 };
            }
        }

        private void ApplyColumnDefinitions(Grid grid)
        {
            grid.ColumnDefinitions.Clear();
            double scale = _currentLayout != null ? _currentLayout.GetScaleFactor() : 1.0;
            var baseWidths = GetBaseColumnWidths();
            foreach (var w in baseWidths)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(w * scale)) });
            }
        }

        private void BuildTableHeader()
        {
            BrdTableHeader.Child = null;
            Grid grid = new Grid { Height = 28 };
            ApplyColumnDefinitions(grid);

            switch (_mode)
            {
                case ReportMode.TheoNgay:
                    grid.Children.Add(CreateHeaderCell("STT", 0));
                    grid.Children.Add(CreateHeaderCell("Mã hàng", 1));
                    grid.Children.Add(CreateHeaderCell("Tên mặt hàng", 2));
                    grid.Children.Add(CreateHeaderCell("ĐVT", 3));
                    grid.Children.Add(CreateHeaderCell("Số lượng", 4));
                    grid.Children.Add(CreateHeaderCell("Đơn giá", 5));
                    grid.Children.Add(CreateHeaderCell("Thành tiền", 6, isLast: true));
                    break;

                case ReportMode.TheoDon:
                    grid.Children.Add(CreateHeaderCell("STT", 0));
                    grid.Children.Add(CreateHeaderCell("Số phiếu", 1));
                    grid.Children.Add(CreateHeaderCell("Bàn / Vị trí", 2));
                    grid.Children.Add(CreateHeaderCell("Giờ vào", 3));
                    grid.Children.Add(CreateHeaderCell("Giờ ra", 4));
                    grid.Children.Add(CreateHeaderCell("Thu ngân", 5));
                    grid.Children.Add(CreateHeaderCell("Tiền hàng", 6));
                    grid.Children.Add(CreateHeaderCell("Giảm giá", 7));
                    grid.Children.Add(CreateHeaderCell("Thanh toán", 8, isLast: true));
                    break;

                case ReportMode.DinhLuong:
                    grid.Children.Add(CreateHeaderCell("STT", 0));
                    grid.Children.Add(CreateHeaderCell("Mã NVL", 1));
                    grid.Children.Add(CreateHeaderCell("Tên nguyên vật liệu", 2));
                    grid.Children.Add(CreateHeaderCell("ĐVT", 3));
                    grid.Children.Add(CreateHeaderCell("SL định lượng", 4));
                    grid.Children.Add(CreateHeaderCell("Đơn giá", 5));
                    grid.Children.Add(CreateHeaderCell("Thành tiền", 6, isLast: true));
                    break;
            }

            BrdTableHeader.Child = grid;
        }

        private Border CreateHeaderCell(string text, int col, bool isLast = false)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontSize = _currentLayout?.HeaderFontSize ?? 11.0,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            return b;
        }

        private Border CreateCell(string text, int col, HorizontalAlignment align = HorizontalAlignment.Left, bool isBold = false, bool isLast = false)
        {
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
                FontSize = _currentLayout?.CellFontSize ?? 10.5,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
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
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm hàng: {nhomName}");
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

            switch (_mode)
            {
                case ReportMode.TheoNgay:
                    _dataTheoNgay = await _reportService.GetXuatBanHangTheoNgayAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                    break;
                case ReportMode.TheoDon:
                    _dataTheoDon = await _reportService.GetXuatBanHangTheoDonAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                    break;
                case ReportMode.DinhLuong:
                    _dataDinhLuong = await _reportService.GetXuatBanHangDinhLuongTheoDonAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                    break;
            }

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();

            if (_mode == ReportMode.TheoNgay)
            {
                var filtered = _dataTheoNgay;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataTheoNgay.Where(x =>
                        x.MaHang.ToLower().Contains(keyword) ||
                        x.TenHang.ToLower().Contains(keyword) ||
                        x.DVT.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalQty = 0;
                decimal totalMoney = 0;
                var grouped = filtered.GroupBy(x => x.NgayDisplay).ToList();

                foreach (var g in grouped)
                {
                    // Group row
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    grpBorder.Child = new TextBlock
                    {
                        Text = $"Ngày: {g.Key}",
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };
                    StkDataRows.Children.Add(grpBorder);

                    int stt = 1;
                    foreach (var item in g)
                    {
                        item.STT = stt++;
                        totalQty += item.SoLuong;
                        totalMoney += item.ThanhTien;

                        Border rowBorder = new Border
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 1, 1),
                            Height = 26
                        };

                        Grid rowGrid = new Grid();
                        ApplyColumnDefinitions(rowGrid);

                        rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.MaHang, 1, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.TenHang, 2, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.SoLuong.ToString("#,##0.##"), 4, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.DonGia.ToString("#,##0"), 5, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.ThanhTien.ToString("#,##0"), 6, HorizontalAlignment.Right, isLast: true));

                        rowBorder.Child = rowGrid;
                        StkDataRows.Children.Add(rowBorder);
                    }
                }

                // Total Row
                Border sumBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 28
                };
                Grid sumGrid = new Grid();
                ApplyColumnDefinitions(sumGrid);

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, 4);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontSize = _currentLayout?.CellFontSize ?? 10.5,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalQty.ToString("#,##0.##"), 4, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell("", 5, isBold: true));
                sumGrid.Children.Add(CreateCell(totalMoney.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true));

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else if (_mode == ReportMode.TheoDon)
            {
                var filtered = _dataTheoDon;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataTheoDon.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.BanKhuVuc.ToLower().Contains(keyword) ||
                        x.ThuNgan.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTienHang = 0;
                decimal totalGiamGia = 0;
                decimal totalThanhToan = 0;
                int stt = 1;

                foreach (var item in filtered)
                {
                    item.STT = stt++;
                    totalTienHang += item.TienHang;
                    totalGiamGia += item.GiamGia;
                    totalThanhToan += item.ThanhToan;

                    Border rowBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Height = 26
                    };

                    Grid rowGrid = new Grid();
                    ApplyColumnDefinitions(rowGrid);

                    rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.SoPhieu, 1, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.BanKhuVuc, 2, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.GioVao, 3, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.GioRa, 4, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.ThuNgan, 5, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.TienHang.ToString("#,##0"), 6, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.GiamGia.ToString("#,##0"), 7, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhToan.ToString("#,##0"), 8, HorizontalAlignment.Right, isLast: true));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                }

                // Total Row
                Border sumBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 28
                };
                Grid sumGrid = new Grid();
                ApplyColumnDefinitions(sumGrid);

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, 6);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontSize = _currentLayout?.CellFontSize ?? 10.5,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalTienHang.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalGiamGia.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalThanhToan.ToString("#,##0"), 8, HorizontalAlignment.Right, isBold: true, isLast: true));

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else if (_mode == ReportMode.DinhLuong)
            {
                var filtered = _dataDinhLuong;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDinhLuong.Where(x =>
                        x.MaNvl.ToLower().Contains(keyword) ||
                        x.TenNvl.ToLower().Contains(keyword) ||
                        x.DVT.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalQty = 0;
                decimal totalMoney = 0;
                int stt = 1;

                foreach (var item in filtered)
                {
                    item.STT = stt++;
                    totalQty += item.SoLuongDinhLuong;
                    totalMoney += item.ThanhTien;

                    Border rowBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Height = 26
                    };

                    Grid rowGrid = new Grid();
                    ApplyColumnDefinitions(rowGrid);

                    rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.MaNvl, 1, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.TenNvl, 2, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.SoLuongDinhLuong.ToString("#,##0.##"), 4, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.DonGia.ToString("#,##0"), 5, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhTien.ToString("#,##0"), 6, HorizontalAlignment.Right, isLast: true));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                }

                // Total Row
                Border sumBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 28
                };
                Grid sumGrid = new Grid();
                ApplyColumnDefinitions(sumGrid);

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, 4);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontSize = _currentLayout?.CellFontSize ?? 10.5,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalQty.ToString("#,##0.##"), 4, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell("", 5, isBold: true));
                sumGrid.Children.Add(CreateCell(totalMoney.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true));

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
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
                    printDlg.PrintVisual(ReportPaper, TxtReportTitle.Text);
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
                    FileName = $"{TxtReportTitle.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine(TxtReportTitle.Text);
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();

                    if (_mode == ReportMode.TheoNgay)
                    {
                        sb.AppendLine("STT,Mã hàng,Tên mặt hàng,ĐVT,Số lượng,Đơn giá,Thành tiền,Ngày");
                        foreach (var i in _dataTheoNgay)
                        {
                            sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.SoLuong},{i.DonGia},{i.ThanhTien},\"{i.NgayDisplay}\"");
                        }
                    }
                    else if (_mode == ReportMode.TheoDon)
                    {
                        sb.AppendLine("STT,Số phiếu,Bàn / Vị trí,Giờ vào,Giờ ra,Thu ngân,Tiền hàng,Giảm giá,Thanh toán");
                        foreach (var i in _dataTheoDon)
                        {
                            sb.AppendLine($"{i.STT},\"{i.SoPhieu}\",\"{i.BanKhuVuc}\",\"{i.GioVao}\",\"{i.GioRa}\",\"{i.ThuNgan}\",{i.TienHang},{i.GiamGia},{i.ThanhToan}");
                        }
                    }
                    else if (_mode == ReportMode.DinhLuong)
                    {
                        sb.AppendLine("STT,Mã NVL,Tên NVL,ĐVT,SL định lượng,Đơn giá,Thành tiền");
                        foreach (var i in _dataDinhLuong)
                        {
                            sb.AppendLine($"{i.STT},\"{i.MaNvl}\",\"{i.TenNvl}\",\"{i.DVT}\",{i.SoLuongDinhLuong},{i.DonGia},{i.ThanhTien}");
                        }
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

        private void BtnThietKeMau_Click(object sender, RoutedEventArgs e)
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
                    onLayoutCallback: layout => _ = LoadTemplateConfigAsync(layout));
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở thiết kế mẫu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThamSoTuyChinh_Click(object sender, RoutedEventArgs e)
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

        private void BtnXemDuLieuTho_Click(object sender, RoutedEventArgs e)
        {
            if (InlineDataBorder.Visibility == Visibility.Visible)
            {
                InlineDataBorder.Visibility = Visibility.Collapsed;
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
            InlineDataBorder.Visibility = Visibility.Visible;

            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            scrollViewer?.ScrollToEnd();
        }

        private void BtnDongDuLieuTho_Click(object sender, RoutedEventArgs e)
        {
            InlineDataBorder.Visibility = Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
