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
    public partial class BaoCaoChuyenKhoControl : UserControl
    {
        public enum ReportMode
        {
            TH_NV_Nhan,
            TH_NV_Chuyen,
            TH_Ngay,
            DS_NV_Xuat,
            DS_NV_Nhap,
            DS_Ngay
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;

        private List<BaoCaoChuyenKhoTongHopMatHangItem> _dataTh = new List<BaoCaoChuyenKhoTongHopMatHangItem>();
        private List<BaoCaoChuyenKhoDanhSachItem> _dataDs = new List<BaoCaoChuyenKhoDanhSachItem>();

        public BaoCaoChuyenKhoControl(ReportMode mode = ReportMode.DS_Ngay)
        {
            InitializeComponent();
            _mode = mode;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            switch (_mode)
            {
                case ReportMode.TH_NV_Nhan:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NHÂN VIÊN NHẬN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_NV_Chuyen:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NHÂN VIÊN CHUYỂN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.DS_NV_Xuat:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHUYỂN KHO THEO NHÂN VIÊN XUẤT";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_NV_Nhap:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHUYỂN KHO THEO NHÂN VIÊN NHẬP";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_Ngay:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHUYỂN KHO THEO NGÀY";
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
                var khoList = await _reportService.GetKhoHangFilterAsync();
                CboKhoXuat.ItemsSource = khoList;
                CboKhoXuat.SelectedIndex = 0;

                var khoNhapList = await _reportService.GetKhoHangFilterAsync();
                CboKhoNhap.ItemsSource = khoNhapList;
                CboKhoNhap.SelectedIndex = 0;

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

            if (_mode == ReportMode.DS_NV_Xuat || _mode == ReportMode.DS_NV_Nhap || _mode == ReportMode.DS_Ngay)
            {
                // STT(35), Số phiếu(85), Ngày(85), Kho xuất(120), Kho nhập(120), NV xuất(100), NV nhập(95), Tổng tiền(80) = 720
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Số phiếu", 1));
                grid.Children.Add(CreateHeaderCell("Ngày", 2));
                grid.Children.Add(CreateHeaderCell("Kho xuất", 3));
                grid.Children.Add(CreateHeaderCell("Kho nhập", 4));
                grid.Children.Add(CreateHeaderCell("NV xuất", 5));
                grid.Children.Add(CreateHeaderCell("NV nhập", 6));
                grid.Children.Add(CreateHeaderCell("Tổng cộng", 7, isLast: true));
            }
            else
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

            string khoXuatId = (CboKhoXuat.SelectedItem as FilterComboItem)?.Id ?? "";
            string khoNhapId = (CboKhoNhap.SelectedItem as FilterComboItem)?.Id ?? "";
            string nvId = (CboNhanVien.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            string khoXuatName = (CboKhoXuat.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string khoNhapName = (CboKhoNhap.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nhomName = (CboNhomHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nvName = (CboNhanVien.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khoXuatName)) parts.Add($"Kho xuất: {khoXuatName}");
            if (Utilities.IsSpecificFilter(khoNhapName)) parts.Add($"Kho nhập: {khoNhapName}");
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
                case ReportMode.TH_NV_Nhan:
                    _dataTh = await _reportService.GetChuyenKhoTongHopMatHangTheoNhanVienNhanAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
                    break;
                case ReportMode.TH_NV_Chuyen:
                    _dataTh = await _reportService.GetChuyenKhoTongHopMatHangTheoNhanVienChuyenAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
                    break;
                case ReportMode.TH_Ngay:
                    _dataTh = await _reportService.GetChuyenKhoTongHopMatHangTheoNgayAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
                    break;
                case ReportMode.DS_NV_Xuat:
                    _dataDs = await _reportService.GetChuyenKhoDanhSachTheoNhanVienXuatAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
                    break;
                case ReportMode.DS_NV_Nhap:
                    _dataDs = await _reportService.GetChuyenKhoDanhSachTheoNhanVienNhapAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
                    break;
                case ReportMode.DS_Ngay:
                    _dataDs = await _reportService.GetChuyenKhoDanhSachTheoNgayAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
                    break;
            }

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();

            if (_mode == ReportMode.DS_NV_Xuat || _mode == ReportMode.DS_NV_Nhap || _mode == ReportMode.DS_Ngay)
            {
                var filtered = _dataDs;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDs.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.KhoXuat.ToLower().Contains(keyword) ||
                        x.KhoNhap.ToLower().Contains(keyword) ||
                        x.NhanVienXuat.ToLower().Contains(keyword) ||
                        x.NhanVienNhap.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTongCong = 0;

                var grouped = _mode == ReportMode.DS_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    (_mode == ReportMode.DS_NV_Xuat ?
                    filtered.GroupBy(x => x.NhanVienXuat).ToList() :
                    filtered.GroupBy(x => x.NhanVienNhap).ToList());

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    string grpLabel = _mode == ReportMode.DS_Ngay ? $"Ngày: {g.Key}" : (_mode == ReportMode.DS_NV_Xuat ? $"NV xuất: {g.Key}" : $"NV nhập: {g.Key}");
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
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                        rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.SoPhieu, 1, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.NgayDisplay, 2, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.KhoXuat, 3, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.KhoNhap, 4, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.NhanVienXuat, 5, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.NhanVienNhap, 6, HorizontalAlignment.Left));
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
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                Border span = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Padding = new Thickness(4, 0, 10, 0)
                };
                Grid.SetColumn(span, 0);
                Grid.SetColumnSpan(span, 7);
                span.Child = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sumGrid.Children.Add(span);

                sumGrid.Children.Add(CreateCell(totalTongCong.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true, isLast: true));

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else
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
                    (_mode == ReportMode.TH_NV_Nhan ?
                    filtered.GroupBy(x => x.NhanVienNhan).ToList() :
                    filtered.GroupBy(x => x.NhanVienChuyen).ToList());

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    string grpLabel = _mode == ReportMode.TH_Ngay ? $"Ngày: {g.Key}" : (_mode == ReportMode.TH_NV_Nhan ? $"NV nhận: {g.Key}" : $"NV chuyển: {g.Key}");
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

                    if (_mode == ReportMode.DS_NV_Xuat || _mode == ReportMode.DS_NV_Nhap || _mode == ReportMode.DS_Ngay)
                    {
                        sb.AppendLine("STT,Số phiếu,Ngày,Kho xuất,Kho nhập,NV xuất,NV nhập,Tổng cộng");
                        foreach (var i in _dataDs)
                        {
                            sb.AppendLine($"{i.STT},\"{i.SoPhieu}\",\"{i.NgayDisplay}\",\"{i.KhoXuat}\",\"{i.KhoNhap}\",\"{i.NhanVienXuat}\",\"{i.NhanVienNhap}\",{i.TongCong}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Số lượng,Đơn giá,Thành tiền");
                        foreach (var i in _dataTh)
                        {
                            sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.SoLuong},{i.DonGia},{i.ThanhTien}");
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
