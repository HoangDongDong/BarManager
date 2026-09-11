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
    public partial class BaoCaoNhapHangControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        public enum ReportMode
        {
            DS_Ngay,
            DS_NCC,
            DS_NV,
            TH_NCC,
            TH_NV,
            TH_Nhap_Ngay,
            TH_Nhap_NCC,
            TH_Nhap_NV,
            TH_MatHang_Ngay
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;

        private List<BaoCaoNhapHangDanhSachItem> _dataDs = new List<BaoCaoNhapHangDanhSachItem>();
        private List<BaoCaoNhapHangTongHopMatHangItem> _dataThMatHang = new List<BaoCaoNhapHangTongHopMatHangItem>();
        private List<BaoCaoNhapHangTongHopNhapItem> _dataThNhap = new List<BaoCaoNhapHangTongHopNhapItem>();

        private sealed class ActiveReportCol<T>
        {
            public string Caption { get; set; } = "";
            public GridLength Width { get; set; }
            public HorizontalAlignment Align { get; set; }
            public Func<T, string> ValueFunc { get; set; } = _ => "";
            public bool IsTotalQuantity { get; set; }
            public bool IsTotalCount { get; set; }
            public bool IsTotalMoney { get; set; }
            public bool IsDiscount { get; set; }
            public string TotalKind { get; set; } = "";
            public int ConfigOrder { get; set; } = int.MaxValue;
        }

        public BaoCaoNhapHangControl(ReportMode mode = ReportMode.DS_Ngay)
        {
            InitializeComponent();
            _mode = mode;
            UpdateTitle();
        }

        // Tên hiển thị đang dùng cụm "nhập hàng", trong khi khóa NAME của
        // SREPORT gốc dùng "nhập kho". Luôn dùng khóa chuẩn này khi đọc/ghi mẫu.
        private string TemplateReportName => _mode switch
        {
            ReportMode.DS_Ngay => "DANH SÁCH PHIẾU NHẬP KHO THEO NGÀY",
            ReportMode.DS_NCC => "DANH SÁCH PHIẾU NHẬP KHO THEO NHÀ CUNG CẤP",
            ReportMode.DS_NV => "DANH SÁCH PHIẾU NHẬP KHO THEO NHÂN VIÊN NHẬP",
            ReportMode.TH_NCC => "TỔNG HỢP MẶT HÀNG PHIẾU NHẬP THEO NHÀ CUNG CẤP",
            ReportMode.TH_NV => "TỔNG HỢP MẶT HÀNG PHIẾU NHẬP THEO NHÂN VIÊN NHẬP",
            ReportMode.TH_Nhap_Ngay => "TỔNG HỢP NHẬP THEO NGÀY",
            ReportMode.TH_Nhap_NCC => "TỔNG HỢP NHẬP THEO NHÀ CUNG CẤP",
            ReportMode.TH_Nhap_NV => "TỔNG HỢP NHẬP THEO NHÂN VIÊN NHẬP",
            ReportMode.TH_MatHang_Ngay => "TỔNG HỢP MẶT HÀNG NHẬP KHO THEO NGÀY",
            _ => TxtReportTitle.Text
        };

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            if (p == null) return;
            _pendingParams = p;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            SelectCombo(CboKhoHang, p.KhoNhapId ?? p.KhoId);
            SelectCombo(CboNcc, p.NhaCungCapId);
            SelectCombo(CboNhanVien, p.NhanVienId);
            SelectCombo(CboNhomHang, p.NhomHangId);

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
                    var fullConfig = await QuanLyBar.Client.Services.ReportTemplateConfigService.GetFullTemplateConfigAsync(TemplateReportName);
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
                case ReportMode.DS_Ngay:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU NHẬP HÀNG THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_NCC:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU NHẬP HÀNG THEO NHÀ CUNG CẤP";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_NV:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU NHẬP HÀNG THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_NCC:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG NHẬP THEO NHÀ CUNG CẤP";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_NV:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG NHẬP THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_Nhap_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP NHẬP HÀNG THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_Nhap_NCC:
                    TxtReportTitle.Text = "TỔNG HỢP NHẬP HÀNG THEO NHÀ CUNG CẤP";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_Nhap_NV:
                    TxtReportTitle.Text = "TỔNG HỢP NHẬP HÀNG THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_MatHang_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG NHẬP THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
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
                var nccList = await _reportService.GetNhaCungCapFilterAsync();
                CboNcc.ItemsSource = nccList;
                CboNcc.SelectedIndex = 0;

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

        private bool IsColVisible(params string[] aliases)
        {
            return _colConfigs.IsColumnVisible(aliases, true);
        }

        private ReportColumnConfigItem? FindColConfig(IEnumerable<string> aliases)
        {
            if (_colConfigs == null) return null;
            return _colConfigs.FirstOrDefault(c => aliases.Any(a =>
                c.Cot.Equals(a, StringComparison.OrdinalIgnoreCase) ||
                c.DataField.Equals(a, StringComparison.OrdinalIgnoreCase) ||
                c.Caption.Equals(a, StringComparison.OrdinalIgnoreCase)));
        }

        private List<ActiveReportCol<T>> ApplyTemplateOrder<T>(List<ActiveReportCol<T>> cols)
        {
            if (_colConfigs == null || _colConfigs.Count == 0) return cols;
            var stt = cols.Take(1);
            return stt.Concat(cols.Skip(1).OrderBy(c => c.ConfigOrder)).ToList();
        }

        private string FormatNumber(IEnumerable<string> aliases, decimal value, string defaultFormat)
        {
            string format = FindColConfig(aliases)?.Format?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(format)) format = defaultFormat;
            try { return value.ToString(format); }
            catch (FormatException) { return value.ToString(defaultFormat); }
        }

        private string EnabledTotalKind(IEnumerable<string> aliases, string totalKind)
        {
            if (string.IsNullOrEmpty(totalKind)) return "";
            var config = FindColConfig(aliases);
            return config == null || config.TongCong ? totalKind : "";
        }

        private GridLength ColWidth(double normal, double compact)
        {
            double width = _currentLayout?.Is80mm == true ? compact : normal;
            return new GridLength(Math.Round(width * _currentLayout.GetScaleFactor()));
        }

        private List<ActiveReportCol<BaoCaoNhapHangDanhSachItem>> GetActiveColsDs()
        {
            var cols = new List<ActiveReportCol<BaoCaoNhapHangDanhSachItem>>
            {
                new ActiveReportCol<BaoCaoNhapHangDanhSachItem> { Caption = "STT", Width = ColWidth(35, 25), Align = HorizontalAlignment.Center, ValueFunc = x => x.STT.ToString() }
            };

            void Add(string[] aliases, string caption, double width, double compact, HorizontalAlignment align,
                Func<BaoCaoNhapHangDanhSachItem, string> value, string totalKind = "")
            {
                if (!IsColVisible(aliases)) return;
                string enabledTotalKind = EnabledTotalKind(aliases, totalKind);
                cols.Add(new ActiveReportCol<BaoCaoNhapHangDanhSachItem>
                {
                    Caption = _colConfigs.GetColumnCaption(aliases, caption),
                    Width = ColWidth(width, compact),
                    Align = _colConfigs.GetColumnAlign(aliases, align),
                    ValueFunc = value,
                    TotalKind = enabledTotalKind,
                    ConfigOrder = FindColConfig(aliases)?.Index ?? int.MaxValue
                });
            }

            Add(new[] { "Ghi chú", "GHICHU", "NOTE" }, "Ghi chú", 130, 65, HorizontalAlignment.Left, x => x.GhiChu);
            Add(new[] { "Số phiếu", "SOPHIEU" }, "Số phiếu", 85, 54, HorizontalAlignment.Center, x => x.SoPhieu);
            Add(new[] { "Ngày", "NGAY" }, "Ngày", 85, 52, HorizontalAlignment.Center, x => x.NgayDisplay);
            Add(new[] { "Loại", "LOAI" }, "Loại", 75, 48, HorizontalAlignment.Left, x => x.Loai);
            string[] tongCongAliases = { "Tổng cộng", "TONGCONG" };
            string[] phiVcAliases = { "Phí vận chuyển", "PHIVANCHUYEN" };
            string[] giamGiaAliases = { "Chiết khấu", "Tiền giảm giá", "Giảm giá", "TIENGIAMGIA", "GIAMGIA" };
            string[] tiLeGiamAliases = { "Tỉ lệ giảm giá", "Tỷ lệ giảm giá", "TILEGIAMGIA" };
            string[] tienThueAliases = { "Tiền thuế", "TIENTHUE" };
            string[] tiLeThueAliases = { "Tỉ lệ thuế", "Tỷ lệ thuế", "TILETHUE" };
            string[] tienHangAliases = { "Tiền hàng", "TIENHANG" };
            Add(tongCongAliases, "Tổng cộng", 105, 68, HorizontalAlignment.Right, x => FormatNumber(tongCongAliases, x.TongCong, "#,##0"), "TongCong");
            Add(phiVcAliases, "Phí vận chuyển", 105, 62, HorizontalAlignment.Right, x => FormatNumber(phiVcAliases, x.PhiVanChuyen, "#,##0"), "PhiVanChuyen");
            Add(giamGiaAliases, "Tiền giảm giá", 95, 56, HorizontalAlignment.Right, x => FormatNumber(giamGiaAliases, x.GiamGia, "#,##0"), "GiamGia");
            Add(tiLeGiamAliases, "Tỉ lệ giảm giá", 85, 52, HorizontalAlignment.Right, x => FormatNumber(tiLeGiamAliases, x.TiLeGiamGia, "#,##0.##"));
            Add(tienThueAliases, "Tiền thuế", 95, 56, HorizontalAlignment.Right, x => FormatNumber(tienThueAliases, x.TienThue, "#,##0"), "TienThue");
            Add(tiLeThueAliases, "Tỉ lệ thuế", 75, 48, HorizontalAlignment.Right, x => FormatNumber(tiLeThueAliases, x.TiLeThue, "#,##0.##"));
            Add(tienHangAliases, "Tiền hàng", 110, 62, HorizontalAlignment.Right, x => FormatNumber(tienHangAliases, x.TienHang, "#,##0"), "TienHang");
            Add(new[] { "Nhà cung cấp", "NHACUNGCAP", "DNHACUNGCAP_NAME" }, "Nhà cung cấp", 145, 70, HorizontalAlignment.Left, x => x.NhaCungCap);
            Add(new[] { "Kho nhập", "Kho hàng", "KHOHANG", "DKHONHAP_NAME" }, "Kho nhập", 120, 60, HorizontalAlignment.Left, x => x.KhoHang);
            Add(new[] { "Nhân viên nhập", "Nhân viên", "NHANVIEN", "DNHANVIENNHAP_NAME" }, "Nhân viên nhập", 120, 62, HorizontalAlignment.Left, x => x.NhanVien);
            Add(new[] { "Diễn giải", "DIENGIAI" }, "Diễn giải", 150, 75, HorizontalAlignment.Left, x => x.DienGiai);
            return ApplyTemplateOrder(cols);
        }

        private List<ActiveReportCol<BaoCaoNhapHangTongHopMatHangItem>> GetActiveColsMatHang()
        {
            var cols = new List<ActiveReportCol<BaoCaoNhapHangTongHopMatHangItem>>
            {
                new ActiveReportCol<BaoCaoNhapHangTongHopMatHangItem> { Caption = "STT", Width = ColWidth(35, 25), Align = HorizontalAlignment.Center, ValueFunc = x => x.STT.ToString() }
            };

            void Add(string[] aliases, string caption, double width, double compact, HorizontalAlignment align,
                Func<BaoCaoNhapHangTongHopMatHangItem, string> value, string totalKind = "")
            {
                if (!IsColVisible(aliases)) return;
                string enabledTotalKind = EnabledTotalKind(aliases, totalKind);
                cols.Add(new ActiveReportCol<BaoCaoNhapHangTongHopMatHangItem>
                {
                    Caption = _colConfigs.GetColumnCaption(aliases, caption), Width = ColWidth(width, compact),
                    Align = _colConfigs.GetColumnAlign(aliases, align), ValueFunc = value,
                    TotalKind = enabledTotalKind,
                    ConfigOrder = FindColConfig(aliases)?.Index ?? int.MaxValue
                });
            }

            Add(new[] { "Mã hàng", "MAHANG", "DMATHANG_CODE" }, "Mã hàng", 85, 45, HorizontalAlignment.Center, x => x.MaHang);
            Add(new[] { "Tên mặt hàng", "Mặt hàng", "TENHANG", "DMATHANG_NAME" }, "Tên mặt hàng", 220, 90, HorizontalAlignment.Left, x => x.TenHang);
            Add(new[] { "ĐVT", "DONVITINH", "DDONVITINH_NAME" }, "ĐVT", 60, 35, HorizontalAlignment.Center, x => x.DVT);
            Add(new[] { "Số lượng", "SOLUONG", "SLNHAPCHUAQUYDOI" }, "Số lượng", 90, 48, HorizontalAlignment.Right, x => x.SoLuong.ToString("#,##0.##"), "SoLuong");
            Add(new[] { "Đơn giá", "DONGIA" }, "Đơn giá", 100, 58, HorizontalAlignment.Right, x => x.DonGia.ToString("#,##0"));
            Add(new[] { "Thành tiền", "THANHTIEN", "Tổng cộng", "TONGCONG" }, "Thành tiền", 130, 70, HorizontalAlignment.Right, x => x.ThanhTien.ToString("#,##0"), "ThanhTien");
            return ApplyTemplateOrder(cols);
        }

        private List<ActiveReportCol<BaoCaoNhapHangTongHopNhapItem>> GetActiveColsTongHop()
        {
            var cols = new List<ActiveReportCol<BaoCaoNhapHangTongHopNhapItem>>
            {
                new ActiveReportCol<BaoCaoNhapHangTongHopNhapItem> { Caption = "STT", Width = ColWidth(35, 25), Align = HorizontalAlignment.Center, ValueFunc = x => x.STT.ToString() }
            };
            string groupTitle = _mode == ReportMode.TH_Nhap_Ngay ? "Ngày nhập" : (_mode == ReportMode.TH_Nhap_NCC ? "Nhà cung cấp" : "Nhân viên");

            void Add(string[] aliases, string caption, double width, double compact, HorizontalAlignment align,
                Func<BaoCaoNhapHangTongHopNhapItem, string> value, string totalKind = "")
            {
                if (!IsColVisible(aliases)) return;
                string enabledTotalKind = EnabledTotalKind(aliases, totalKind);
                cols.Add(new ActiveReportCol<BaoCaoNhapHangTongHopNhapItem>
                {
                    Caption = _colConfigs.GetColumnCaption(aliases, caption), Width = ColWidth(width, compact),
                    Align = _colConfigs.GetColumnAlign(aliases, align), ValueFunc = value,
                    TotalKind = enabledTotalKind,
                    ConfigOrder = FindColConfig(aliases)?.Index ?? int.MaxValue
                });
            }

            Add(new[] { groupTitle, "Ngày", "Nhà cung cấp", "Nhân viên", "TENHIENTHI" }, groupTitle, 165, 75, HorizontalAlignment.Left, x => x.TenHienThi);
            Add(new[] { "Số phiếu", "SOPHIEU" }, "Số phiếu", 80, 48, HorizontalAlignment.Center, x => x.SoPhieu.ToString("#,##0"), "SoPhieu");
            Add(new[] { "Kho nhập", "Kho hàng", "KHOHANG", "DKHONHAP_NAME" }, "Kho nhập", 120, 60, HorizontalAlignment.Left, x => x.KhoHang);
            Add(new[] { "Tiền hàng", "TIENHANG" }, "Tiền hàng", 110, 60, HorizontalAlignment.Right, x => x.TienHang.ToString("#,##0"), "TienHang");
            Add(new[] { "Chiết khấu", "Tiền giảm giá", "GIAMGIA", "TIENGIAMGIA" }, "Chiết khấu", 80, 50, HorizontalAlignment.Right, x => x.GiamGia.ToString("#,##0"), "GiamGia");
            Add(new[] { "Phí vận chuyển", "PHIVANCHUYEN" }, "Phí vận chuyển", 105, 58, HorizontalAlignment.Right, x => x.PhiVanChuyen.ToString("#,##0"), "PhiVanChuyen");
            Add(new[] { "Tiền thuế", "TIENTHUE" }, "Tiền thuế", 95, 54, HorizontalAlignment.Right, x => x.TienThue.ToString("#,##0"), "TienThue");
            Add(new[] { "Tổng cộng", "TONGCONG" }, "Tổng cộng", 130, 68, HorizontalAlignment.Right, x => x.TongCong.ToString("#,##0"), "TongCong");
            return ApplyTemplateOrder(cols);
        }

        private void BuildTableHeader()
        {
            BrdTableHeader.Child = null;
            Grid grid = new Grid { Height = _currentLayout?.RowHeight ?? 28 };
            IEnumerable<(string Caption, GridLength Width)> columns;

            if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NCC || _mode == ReportMode.DS_NV)
                columns = GetActiveColsDs().Select(x => (x.Caption, x.Width));
            else if (_mode == ReportMode.TH_NCC || _mode == ReportMode.TH_NV || _mode == ReportMode.TH_MatHang_Ngay)
                columns = GetActiveColsMatHang().Select(x => (x.Caption, x.Width));
            else
                columns = GetActiveColsTongHop().Select(x => (x.Caption, x.Width));

            var active = columns.ToList();
            for (int i = 0; i < active.Count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = active[i].Width });
                grid.Children.Add(CreateHeaderCell(active[i].Caption, i, i == active.Count - 1));
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

            string nccId = (CboNcc.SelectedItem as FilterComboItem)?.Id ?? "";
            string khoId = (CboKhoHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string nvId = (CboNhanVien.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            string nccName = (CboNcc.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string khoName = (CboKhoHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nhomName = (CboNhomHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nvName = (CboNhanVien.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khoName)) parts.Add($"Kho hàng: {khoName}");
            if (Utilities.IsSpecificFilter(nccName)) parts.Add($"NCC: {nccName}");
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
                case ReportMode.DS_Ngay:
                    _dataDs = await _reportService.GetNhapHangDanhSachTheoNgayAsync(tuNgay, denNgay, nccId, khoId, nvId);
                    break;
                case ReportMode.DS_NCC:
                    _dataDs = await _reportService.GetNhapHangDanhSachTheoNhaCungCapAsync(tuNgay, denNgay, nccId, khoId, nvId);
                    break;
                case ReportMode.DS_NV:
                    _dataDs = await _reportService.GetNhapHangDanhSachTheoNhanVienAsync(tuNgay, denNgay, nccId, khoId, nvId);
                    break;
                case ReportMode.TH_NCC:
                    _dataThMatHang = await _reportService.GetNhapHangTongHopMatHangTheoNhaCungCapAsync(tuNgay, denNgay, nccId, khoId, nhomId, nvId);
                    break;
                case ReportMode.TH_NV:
                    _dataThMatHang = await _reportService.GetNhapHangTongHopMatHangTheoNhanVienAsync(tuNgay, denNgay, nccId, khoId, nhomId, nvId);
                    break;
                case ReportMode.TH_Nhap_Ngay:
                    _dataThNhap = await _reportService.GetNhapHangTongHopNhapTheoNgayAsync(tuNgay, denNgay, nccId, khoId, nvId);
                    break;
                case ReportMode.TH_Nhap_NCC:
                    _dataThNhap = await _reportService.GetNhapHangTongHopNhapTheoNhaCungCapAsync(tuNgay, denNgay, nccId, khoId, nvId);
                    break;
                case ReportMode.TH_Nhap_NV:
                    _dataThNhap = await _reportService.GetNhapHangTongHopNhapTheoNhanVienAsync(tuNgay, denNgay, nccId, khoId, nvId);
                    break;
                case ReportMode.TH_MatHang_Ngay:
                    _dataThMatHang = await _reportService.GetNhapHangTongHopMatHangNhapTheoNgayAsync(tuNgay, denNgay, nccId, khoId, nhomId, nvId);
                    break;
            }

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();

            if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NCC || _mode == ReportMode.DS_NV)
            {
                var cols = GetActiveColsDs();
                var filtered = _dataDs;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDs.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.NhaCungCap.ToLower().Contains(keyword) ||
                        x.NhanVien.ToLower().Contains(keyword) ||
                        x.KhoHang.ToLower().Contains(keyword) ||
                        x.GhiChu.ToLower().Contains(keyword) ||
                        x.DienGiai.ToLower().Contains(keyword) ||
                        x.Loai.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTienHang = 0;
                decimal totalGiamGia = 0;
                decimal totalPhiVanChuyen = 0;
                decimal totalTienThue = 0;
                decimal totalTongCong = 0;

                var grouped = _mode == ReportMode.DS_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    (_mode == ReportMode.DS_NCC ?
                    filtered.GroupBy(x => x.NhaCungCap).ToList() :
                    filtered.GroupBy(x => x.NhanVien).ToList());

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    string grpLabel = _mode == ReportMode.DS_Ngay ? $"Ngày: {g.Key}" : (_mode == ReportMode.DS_NCC ? $"Nhà cung cấp: {g.Key}" : $"Nhân viên: {g.Key}");
                    grpBorder.Child = new TextBlock
                    {
                        Text = grpLabel,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };
                    StkDataRows.Children.Add(grpBorder);

                    int stt = 1;
                    foreach (var item in g)
                    {
                        item.STT = stt++;
                        totalTienHang += item.TienHang;
                        totalGiamGia += item.GiamGia;
                        totalPhiVanChuyen += item.PhiVanChuyen;
                        totalTienThue += item.TienThue;
                        totalTongCong += item.TongCong;

                        Border rowBorder = new Border
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 1, 1),
                            Height = 26
                        };

                        Grid rowGrid = new Grid();
                        for (int c = 0; c < cols.Count; c++)
                        {
                            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                            rowGrid.Children.Add(CreateCell(cols[c].ValueFunc(item), c, cols[c].Align, isLast: c == cols.Count - 1));
                        }

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
                for (int c = 0; c < cols.Count; c++)
                    sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                int firstTotal = cols.FindIndex(c => !string.IsNullOrEmpty(c.TotalKind));
                if (firstTotal < 1) firstTotal = cols.Count;
                Grid.SetColumnSpan(span, firstTotal);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);
                for (int c = firstTotal; c < cols.Count; c++)
                {
                    string value = cols[c].TotalKind switch
                    {
                        "TienHang" => totalTienHang.ToString("#,##0"),
                        "GiamGia" => totalGiamGia.ToString("#,##0"),
                        "PhiVanChuyen" => totalPhiVanChuyen.ToString("#,##0"),
                        "TienThue" => totalTienThue.ToString("#,##0"),
                        "TongCong" => totalTongCong.ToString("#,##0"),
                        _ => ""
                    };
                    sumGrid.Children.Add(CreateCell(value, c, cols[c].Align, isBold: true, isLast: c == cols.Count - 1));
                }

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else if (_mode == ReportMode.TH_NCC || _mode == ReportMode.TH_NV || _mode == ReportMode.TH_MatHang_Ngay)
            {
                var cols = GetActiveColsMatHang();
                var filtered = _dataThMatHang;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataThMatHang.Where(x =>
                        x.MaHang.ToLower().Contains(keyword) ||
                        x.TenHang.ToLower().Contains(keyword) ||
                        x.DVT.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalQty = 0;
                decimal totalMoney = 0;

                var grouped = _mode == ReportMode.TH_MatHang_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    (_mode == ReportMode.TH_NCC ?
                    filtered.GroupBy(x => x.NhaCungCap).ToList() :
                    filtered.GroupBy(x => x.NhanVien).ToList());

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    string grpLabel = _mode == ReportMode.TH_MatHang_Ngay ? $"Ngày: {g.Key}" : (_mode == ReportMode.TH_NCC ? $"Nhà cung cấp: {g.Key}" : $"Nhân viên: {g.Key}");
                    grpBorder.Child = new TextBlock
                    {
                        Text = grpLabel,
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
                        for (int c = 0; c < cols.Count; c++)
                        {
                            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                            rowGrid.Children.Add(CreateCell(cols[c].ValueFunc(item), c, cols[c].Align, isLast: c == cols.Count - 1));
                        }

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
                for (int c = 0; c < cols.Count; c++)
                    sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                int firstTotal = cols.FindIndex(c => !string.IsNullOrEmpty(c.TotalKind));
                if (firstTotal < 1) firstTotal = cols.Count;
                Grid.SetColumnSpan(span, firstTotal);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);
                for (int c = firstTotal; c < cols.Count; c++)
                {
                    string value = cols[c].TotalKind switch
                    {
                        "SoLuong" => totalQty.ToString("#,##0.##"),
                        "ThanhTien" => totalMoney.ToString("#,##0"),
                        _ => ""
                    };
                    sumGrid.Children.Add(CreateCell(value, c, cols[c].Align, isBold: true, isLast: c == cols.Count - 1));
                }

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else if (_mode == ReportMode.TH_Nhap_Ngay || _mode == ReportMode.TH_Nhap_NCC || _mode == ReportMode.TH_Nhap_NV)
            {
                var cols = GetActiveColsTongHop();
                var filtered = _dataThNhap;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataThNhap.Where(x =>
                        x.TenHienThi.ToLower().Contains(keyword) ||
                        x.KhoHang.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTienHang = 0;
                decimal totalGiamGia = 0;
                decimal totalPhiVanChuyen = 0;
                decimal totalTienThue = 0;
                decimal totalTongCong = 0;
                int totalSoPhieu = 0;

                int stt = 1;
                foreach (var item in filtered)
                {
                    item.STT = stt++;
                    totalSoPhieu += item.SoPhieu;
                    totalTienHang += item.TienHang;
                    totalGiamGia += item.GiamGia;
                    totalPhiVanChuyen += item.PhiVanChuyen;
                    totalTienThue += item.TienThue;
                    totalTongCong += item.TongCong;

                    Border rowBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Height = 26
                    };

                    Grid rowGrid = new Grid();
                    for (int c = 0; c < cols.Count; c++)
                    {
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                        rowGrid.Children.Add(CreateCell(cols[c].ValueFunc(item), c, cols[c].Align, isLast: c == cols.Count - 1));
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
                for (int c = 0; c < cols.Count; c++)
                    sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                int firstTotal = cols.FindIndex(c => !string.IsNullOrEmpty(c.TotalKind));
                if (firstTotal < 1) firstTotal = cols.Count;
                Grid.SetColumnSpan(span, firstTotal);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);
                for (int c = firstTotal; c < cols.Count; c++)
                {
                    string value = cols[c].TotalKind switch
                    {
                        "SoPhieu" => totalSoPhieu.ToString("#,##0"),
                        "TienHang" => totalTienHang.ToString("#,##0"),
                        "GiamGia" => totalGiamGia.ToString("#,##0"),
                        "PhiVanChuyen" => totalPhiVanChuyen.ToString("#,##0"),
                        "TienThue" => totalTienThue.ToString("#,##0"),
                        "TongCong" => totalTongCong.ToString("#,##0"),
                        _ => ""
                    };
                    sumGrid.Children.Add(CreateCell(value, c, cols[c].Align, isBold: true, isLast: c == cols.Count - 1));
                }

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

                    if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NCC || _mode == ReportMode.DS_NV)
                    {
                        var cols = GetActiveColsDs();
                        sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.Caption))));
                        foreach (var i in _dataDs)
                        {
                            sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.ValueFunc(i)))));
                        }
                    }
                    else if (_mode == ReportMode.TH_NCC || _mode == ReportMode.TH_NV || _mode == ReportMode.TH_MatHang_Ngay)
                    {
                        var cols = GetActiveColsMatHang();
                        sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.Caption))));
                        foreach (var i in _dataThMatHang)
                        {
                            sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.ValueFunc(i)))));
                        }
                    }
                    else if (_mode == ReportMode.TH_Nhap_Ngay || _mode == ReportMode.TH_Nhap_NCC || _mode == ReportMode.TH_Nhap_NV)
                    {
                        var cols = GetActiveColsTongHop();
                        sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.Caption))));
                        foreach (var i in _dataThNhap)
                        {
                            sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.ValueFunc(i)))));
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

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.InAn.ThietKeMauInWindow(
                    TemplateReportName,
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

        private static string Csv(string? value)
        {
            string text = value ?? "";
            return $"\"{text.Replace("\"", "\"\"")}\"";
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
