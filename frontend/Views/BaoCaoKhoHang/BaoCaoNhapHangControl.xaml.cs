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
    public partial class BaoCaoNhapHangControl : UserControl
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

        private List<BaoCaoNhapHangDanhSachItem> _dataDs = new List<BaoCaoNhapHangDanhSachItem>();
        private List<BaoCaoNhapHangTongHopMatHangItem> _dataThMatHang = new List<BaoCaoNhapHangTongHopMatHangItem>();
        private List<BaoCaoNhapHangTongHopNhapItem> _dataThNhap = new List<BaoCaoNhapHangTongHopNhapItem>();

        public BaoCaoNhapHangControl(ReportMode mode = ReportMode.DS_Ngay)
        {
            InitializeComponent();
            _mode = mode;
            UpdateTitle();
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

            BuildTableHeader();
            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
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

        private void BuildTableHeader()
        {
            BrdTableHeader.Child = null;
            Grid grid = new Grid { Height = 28 };

            if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NCC || _mode == ReportMode.DS_NV)
            {
                // STT(35), Số phiếu(85), Ngày(85), Nhà CC(145), Kho/NV(120), Tiền hàng(110), Giảm giá(60), Tổng cộng(80) = 720
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Số phiếu", 1));
                grid.Children.Add(CreateHeaderCell("Ngày", 2));
                grid.Children.Add(CreateHeaderCell("Nhà cung cấp", 3));
                grid.Children.Add(CreateHeaderCell(_mode == ReportMode.DS_NV ? "Kho hàng" : "Nhân viên", 4));
                grid.Children.Add(CreateHeaderCell("Tiền hàng", 5));
                grid.Children.Add(CreateHeaderCell("Chiết khấu", 6));
                grid.Children.Add(CreateHeaderCell("Tổng cộng", 7, isLast: true));
            }
            else if (_mode == ReportMode.TH_NCC || _mode == ReportMode.TH_NV || _mode == ReportMode.TH_MatHang_Ngay)
            {
                // STT(35), Mã hàng(85), Tên mặt hàng(220), ĐVT(60), Số lượng(90), Đơn giá(100), Thành tiền(130) = 720
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Mã hàng", 1));
                grid.Children.Add(CreateHeaderCell("Tên mặt hàng", 2));
                grid.Children.Add(CreateHeaderCell("ĐVT", 3));
                grid.Children.Add(CreateHeaderCell("Số lượng", 4));
                grid.Children.Add(CreateHeaderCell("Đơn giá", 5));
                grid.Children.Add(CreateHeaderCell("Thành tiền", 6, isLast: true));
            }
            else if (_mode == ReportMode.TH_Nhap_Ngay || _mode == ReportMode.TH_Nhap_NCC || _mode == ReportMode.TH_Nhap_NV)
            {
                // STT(35), Tên hiển thị(165), Số phiếu(80), Kho nhập(120), Tiền hàng(110), Chiết khấu(80), Tổng cộng(130) = 720
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

                grid.Children.Add(CreateHeaderCell("STT", 0));
                string groupTitle = _mode == ReportMode.TH_Nhap_Ngay ? "Ngày nhập" : (_mode == ReportMode.TH_Nhap_NCC ? "Nhà cung cấp" : "Nhân viên");
                grid.Children.Add(CreateHeaderCell(groupTitle, 1));
                grid.Children.Add(CreateHeaderCell("Số phiếu", 2));
                grid.Children.Add(CreateHeaderCell("Kho nhập", 3));
                grid.Children.Add(CreateHeaderCell("Tiền hàng", 4));
                grid.Children.Add(CreateHeaderCell("Chiết khấu", 5));
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
                var filtered = _dataDs;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDs.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.NhaCungCap.ToLower().Contains(keyword) ||
                        x.NhanVien.ToLower().Contains(keyword) ||
                        x.KhoHang.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTienHang = 0;
                decimal totalGiamGia = 0;
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
                        totalTongCong += item.TongCong;

                        Border rowBorder = new Border
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 1, 1),
                            Height = 26
                        };

                        Grid rowGrid = new Grid();
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                        rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.SoPhieu, 1, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.NgayDisplay, 2, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.NhaCungCap, 3, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(_mode == ReportMode.DS_NV ? item.KhoHang : item.NhanVien, 4, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.TienHang.ToString("#,##0"), 5, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.GiamGia.ToString("#,##0"), 6, HorizontalAlignment.Right));
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
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

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
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalTienHang.ToString("#,##0"), 5, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalGiamGia.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalTongCong.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true, isLast: true));

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else if (_mode == ReportMode.TH_NCC || _mode == ReportMode.TH_NV || _mode == ReportMode.TH_MatHang_Ngay)
            {
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
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

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
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

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
            else if (_mode == ReportMode.TH_Nhap_Ngay || _mode == ReportMode.TH_Nhap_NCC || _mode == ReportMode.TH_Nhap_NV)
            {
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
                decimal totalTongCong = 0;
                int totalSoPhieu = 0;

                int stt = 1;
                foreach (var item in filtered)
                {
                    item.STT = stt++;
                    totalSoPhieu += item.SoPhieu;
                    totalTienHang += item.TienHang;
                    totalGiamGia += item.GiamGia;
                    totalTongCong += item.TongCong;

                    Border rowBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Height = 26
                    };

                    Grid rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

                    rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.TenHienThi, 1, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.SoPhieu.ToString("#,##0"), 2, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.KhoHang, 3, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.TienHang.ToString("#,##0"), 4, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.GiamGia.ToString("#,##0"), 5, HorizontalAlignment.Right));
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
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

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
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalSoPhieu.ToString("#,##0"), 2, HorizontalAlignment.Center, isBold: true));
                sumGrid.Children.Add(CreateCell("", 3, isBold: true));
                sumGrid.Children.Add(CreateCell(totalTienHang.ToString("#,##0"), 4, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalGiamGia.ToString("#,##0"), 5, HorizontalAlignment.Right, isBold: true));
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

                    if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NCC || _mode == ReportMode.DS_NV)
                    {
                        sb.AppendLine("STT,Số phiếu,Ngày,Nhà cung cấp,Nhân viên/Kho,Tiền hàng,Chiết khấu,Tổng cộng");
                        foreach (var i in _dataDs)
                        {
                            sb.AppendLine($"{i.STT},\"{i.SoPhieu}\",\"{i.NgayDisplay}\",\"{i.NhaCungCap}\",\"{(_mode == ReportMode.DS_NV ? i.KhoHang : i.NhanVien)}\",{i.TienHang},{i.GiamGia},{i.TongCong}");
                        }
                    }
                    else if (_mode == ReportMode.TH_NCC || _mode == ReportMode.TH_NV || _mode == ReportMode.TH_MatHang_Ngay)
                    {
                        sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Số lượng,Đơn giá,Thành tiền");
                        foreach (var i in _dataThMatHang)
                        {
                            sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.SoLuong},{i.DonGia},{i.ThanhTien}");
                        }
                    }
                    else if (_mode == ReportMode.TH_Nhap_Ngay || _mode == ReportMode.TH_Nhap_NCC || _mode == ReportMode.TH_Nhap_NV)
                    {
                        sb.AppendLine($"STT,{(_mode == ReportMode.TH_Nhap_Ngay ? "Ngày nhập" : (_mode == ReportMode.TH_Nhap_NCC ? "Nhà cung cấp" : "Nhân viên"))},Số phiếu,Kho nhập,Tiền hàng,Chiết khấu,Tổng cộng");
                        foreach (var i in _dataThNhap)
                        {
                            sb.AppendLine($"{i.STT},\"{i.TenHienThi}\",{i.SoPhieu},\"{i.KhoHang}\",{i.TienHang},{i.GiamGia},{i.TongCong}");
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
    }
}
