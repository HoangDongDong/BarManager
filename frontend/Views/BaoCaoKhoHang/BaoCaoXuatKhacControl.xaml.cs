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
    public partial class BaoCaoXuatKhacControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        public enum ReportMode
        {
            TH_Ngay,
            TH_NV,
            DS_NV,
            TH_XuatKhac_Ngay,
            TH_XuatKhac_NV,
            DS_Ngay
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;

        private List<BaoCaoXuatKhacTongHopItem> _dataTh = new List<BaoCaoXuatKhacTongHopItem>();
        private List<BaoCaoXuatKhacDanhSachItem> _dataDs = new List<BaoCaoXuatKhacDanhSachItem>();
        private List<BaoCaoXuatKhacTongHopXuatItem> _dataThXuat = new List<BaoCaoXuatKhacTongHopXuatItem>();

        public BaoCaoXuatKhacControl(ReportMode mode = ReportMode.TH_Ngay)
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
            SelectCombo(CboNhanVien, p.NhanVienId);

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
                case ReportMode.TH_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG XUẤT KHÁC THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_NV:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG XUẤT KHÁC THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.DS_NV:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU XUẤT KHÁC THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_XuatKhac_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP XUẤT KHÁC THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_XuatKhac_NV:
                    TxtReportTitle.Text = "TỔNG HỢP XUẤT KHÁC THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_Ngay:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU XUẤT KHÁC THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
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

                var nvList = await _reportService.GetNhanVienFilterAsync();
                CboNhanVien.ItemsSource = nvList;
                CboNhanVien.SelectedIndex = 0;

                var nhomList = await _reportService.GetNhomMatHangFilterAsync();
                CboNhomHang.ItemsSource = nhomList;
                CboNhomHang.SelectedIndex = 0;
            }
            catch { }
        }

        private List<double> GetBaseColumnWidths()
        {
            if (_mode == ReportMode.TH_Ngay || _mode == ReportMode.TH_NV)
                return new List<double> { 35, 85, 220, 60, 90, 100, 130 };
            if (_mode == ReportMode.DS_NV || _mode == ReportMode.DS_Ngay)
                return new List<double> { 35, 85, 85, 145, 120, 110, 60, 80 };
            return new List<double> { 35, 155, 90, 120, 100, 80, 140 };
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

            if (_mode == ReportMode.TH_Ngay || _mode == ReportMode.TH_NV)
            {
                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Mã hàng", 1));
                grid.Children.Add(CreateHeaderCell("Tên mặt hàng", 2));
                grid.Children.Add(CreateHeaderCell("ĐVT", 3));
                grid.Children.Add(CreateHeaderCell("Số lượng", 4));
                grid.Children.Add(CreateHeaderCell("Đơn giá", 5));
                grid.Children.Add(CreateHeaderCell("Thành tiền", 6, isLast: true));
            }
            else if (_mode == ReportMode.DS_NV || _mode == ReportMode.DS_Ngay)
            {
                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Số phiếu", 1));
                grid.Children.Add(CreateHeaderCell("Ngày", 2));
                grid.Children.Add(CreateHeaderCell("Lý do xuất", 3));
                grid.Children.Add(CreateHeaderCell(_mode == ReportMode.DS_NV ? "Nhân viên" : "Kho xuất", 4));
                grid.Children.Add(CreateHeaderCell("Tiền hàng", 5));
                grid.Children.Add(CreateHeaderCell("CP khác", 6));
                grid.Children.Add(CreateHeaderCell("Tổng cộng", 7, isLast: true));
            }
            else if (_mode == ReportMode.TH_XuatKhac_Ngay || _mode == ReportMode.TH_XuatKhac_NV)
            {
                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell(_mode == ReportMode.TH_XuatKhac_Ngay ? "Ngày xuất" : "Nhân viên xuất", 1));
                grid.Children.Add(CreateHeaderCell("Số phiếu xuất", 2));
                grid.Children.Add(CreateHeaderCell("Kho xuất", 3));
                grid.Children.Add(CreateHeaderCell("Tiền hàng", 4));
                grid.Children.Add(CreateHeaderCell("Chi phí khác", 5));
                grid.Children.Add(CreateHeaderCell("Tổng cộng", 6, isLast: true));
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
            string nvId = (CboNhanVien.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            string khoName = (CboKhoHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nhomName = (CboNhomHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nvName = (CboNhanVien.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khoName)) parts.Add($"Kho hàng: {khoName}");
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm: {nhomName}");
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

            switch (_mode)
            {
                case ReportMode.TH_Ngay:
                    _dataTh = await _reportService.GetXuatKhacTongHopTheoNgayAsync(tuNgay, denNgay, khoId, nhomId, nvId);
                    break;
                case ReportMode.TH_NV:
                    _dataTh = await _reportService.GetXuatKhacTongHopTheoNhanVienAsync(tuNgay, denNgay, khoId, nhomId, nvId);
                    break;
                case ReportMode.DS_NV:
                    _dataDs = await _reportService.GetXuatKhacDanhSachTheoNhanVienAsync(tuNgay, denNgay, khoId, nvId);
                    break;
                case ReportMode.TH_XuatKhac_Ngay:
                    _dataThXuat = await _reportService.GetXuatKhacTongHopXuatKhacTheoNgayAsync(tuNgay, denNgay, khoId, nvId);
                    break;
                case ReportMode.TH_XuatKhac_NV:
                    _dataThXuat = await _reportService.GetXuatKhacTongHopXuatKhacTheoNhanVienAsync(tuNgay, denNgay, khoId, nvId);
                    break;
                case ReportMode.DS_Ngay:
                    _dataDs = await _reportService.GetXuatKhacDanhSachTheoNgayAsync(tuNgay, denNgay, khoId, nvId);
                    break;
            }

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();

            if (_mode == ReportMode.TH_Ngay || _mode == ReportMode.TH_NV)
            {
                var filtered = _dataTh;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataTh.Where(x =>
                        x.MaHang.ToLower().Contains(keyword) ||
                        x.TenHang.ToLower().Contains(keyword) ||
                        x.DVT.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalQty = 0;
                decimal totalMoney = 0;

                var grouped = _mode == ReportMode.TH_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    filtered.GroupBy(x => x.NhanVien).ToList();

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    grpBorder.Child = new TextBlock
                    {
                        Text = _mode == ReportMode.TH_Ngay ? $"Ngày: {g.Key}" : $"Nhân viên: {g.Key}",
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

                // Total
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
            else if (_mode == ReportMode.DS_NV || _mode == ReportMode.DS_Ngay)
            {
                var filtered = _dataDs;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDs.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.LyDoXuat.ToLower().Contains(keyword) ||
                        x.NhanVien.ToLower().Contains(keyword) ||
                        x.KhoXuat.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTienHang = 0;
                decimal totalCpKhac = 0;
                decimal totalTongCong = 0;

                var grouped = _mode == ReportMode.DS_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    filtered.GroupBy(x => x.NhanVien).ToList();

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    grpBorder.Child = new TextBlock
                    {
                        Text = _mode == ReportMode.DS_Ngay ? $"Ngày: {g.Key}" : $"Nhân viên: {g.Key}",
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };
                    StkDataRows.Children.Add(grpBorder);

                    int stt = 1;
                    foreach (var item in g)
                    {
                        item.STT = stt++;
                        totalTienHang += item.TienHang;
                        totalCpKhac += item.ChiPhiKhac;
                        totalTongCong += item.TongCong;

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
                        rowGrid.Children.Add(CreateCell(item.NgayDisplay, 2, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.LyDoXuat, 3, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(_mode == ReportMode.DS_NV ? item.NhanVien : item.KhoXuat, 4, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.TienHang.ToString("#,##0"), 5, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.ChiPhiKhac.ToString("#,##0"), 6, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.TongCong.ToString("#,##0"), 7, HorizontalAlignment.Right, isLast: true));

                        rowBorder.Child = rowGrid;
                        StkDataRows.Children.Add(rowBorder);
                    }
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
                ApplyColumnDefinitions(sumGrid);

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, 5);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontSize = _currentLayout?.CellFontSize ?? 10.5,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalTienHang.ToString("#,##0"), 5, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalCpKhac.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalTongCong.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true, isLast: true));

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else if (_mode == ReportMode.TH_XuatKhac_Ngay || _mode == ReportMode.TH_XuatKhac_NV)
            {
                var filtered = _dataThXuat;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataThXuat.Where(x =>
                        x.TenHienThi.ToLower().Contains(keyword) ||
                        x.KhoXuat.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTienHang = 0;
                decimal totalCpKhac = 0;
                decimal totalTongCong = 0;
                int totalSoPhieu = 0;

                int stt = 1;
                foreach (var item in filtered)
                {
                    item.STT = stt++;
                    totalSoPhieu += item.SoPhieu;
                    totalTienHang += item.TienHang;
                    totalCpKhac += item.ChiPhiKhac;
                    totalTongCong += item.TongCong;

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
                    rowGrid.Children.Add(CreateCell(item.TenHienThi, 1, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.SoPhieu.ToString("#,##0"), 2, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.KhoXuat, 3, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.TienHang.ToString("#,##0"), 4, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ChiPhiKhac.ToString("#,##0"), 5, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.TongCong.ToString("#,##0"), 6, HorizontalAlignment.Right, isLast: true));

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
                ApplyColumnDefinitions(sumGrid);

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, 2);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontSize = _currentLayout?.CellFontSize ?? 10.5,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalSoPhieu.ToString("#,##0"), 2, HorizontalAlignment.Center, isBold: true));
                sumGrid.Children.Add(CreateCell("", 3, isBold: true));
                sumGrid.Children.Add(CreateCell(totalTienHang.ToString("#,##0"), 4, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalCpKhac.ToString("#,##0"), 5, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalTongCong.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true));

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

                    if (_mode == ReportMode.TH_Ngay || _mode == ReportMode.TH_NV)
                    {
                        sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Số lượng,Đơn giá,Thành tiền");
                        foreach (var i in _dataTh)
                        {
                            sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.SoLuong},{i.DonGia},{i.ThanhTien}");
                        }
                    }
                    else if (_mode == ReportMode.DS_NV || _mode == ReportMode.DS_Ngay)
                    {
                        sb.AppendLine("STT,Số phiếu,Ngày,Lý do xuất,Kho/NV,Tiền hàng,Chi phí khác,Tổng cộng");
                        foreach (var i in _dataDs)
                        {
                            sb.AppendLine($"{i.STT},\"{i.SoPhieu}\",\"{i.NgayDisplay}\",\"{i.LyDoXuat}\",\"{(_mode == ReportMode.DS_NV ? i.NhanVien : i.KhoXuat)}\",{i.TienHang},{i.ChiPhiKhac},{i.TongCong}");
                        }
                    }
                    else if (_mode == ReportMode.TH_XuatKhac_Ngay || _mode == ReportMode.TH_XuatKhac_NV)
                    {
                        sb.AppendLine($"STT,{(_mode == ReportMode.TH_XuatKhac_Ngay ? "Ngày xuất" : "Nhân viên")},Số phiếu xuất,Kho xuất,Tiền hàng,Chi phí khác,Tổng cộng");
                        foreach (var i in _dataThXuat)
                        {
                            sb.AppendLine($"{i.STT},\"{i.TenHienThi}\",{i.SoPhieu},\"{i.KhoXuat}\",{i.TienHang},{i.ChiPhiKhac},{i.TongCong}");
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
