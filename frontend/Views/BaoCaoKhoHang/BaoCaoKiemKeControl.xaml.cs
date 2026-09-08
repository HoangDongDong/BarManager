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
    public partial class BaoCaoKiemKeControl : UserControl
    {
        public enum ReportMode
        {
            DS_Ngay,
            TH_Ngay,
            TH_NV,
            DS_NV
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;

        private List<BaoCaoKiemKeDanhSachItem> _dataDs = new List<BaoCaoKiemKeDanhSachItem>();
        private List<BaoCaoKiemKeTongHopItem> _dataTh = new List<BaoCaoKiemKeTongHopItem>();

        public BaoCaoKiemKeControl(ReportMode mode = ReportMode.DS_Ngay)
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
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU KIỂM KÊ THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.TH_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG KIỂM KÊ THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_NV:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG KIỂM KÊ THEO NHÂN VIÊN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.DS_NV:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU KIỂM KÊ THEO NHÂN VIÊN";
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

            if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NV)
            {
                // STT(35), Số phiếu(85), Ngày(85), Kho hàng(135), Nhân viên(130), Chênh lệch SL(100), Giá trị CL(150) = 720
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Số phiếu", 1));
                grid.Children.Add(CreateHeaderCell("Ngày", 2));
                grid.Children.Add(CreateHeaderCell("Kho hàng", 3));
                grid.Children.Add(CreateHeaderCell("Nhân viên", 4));
                grid.Children.Add(CreateHeaderCell("Chênh lệch SL", 5));
                grid.Children.Add(CreateHeaderCell("Giá trị chênh lệch", 6, isLast: true));
            }
            else
            {
                // STT(35), Mã hàng(85), Tên hàng(220), ĐVT(60), SL Sổ sách(80), SL Thực tế(80), Chênh lệch(70), Giá trị CL(90) = 720
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

                grid.Children.Add(CreateHeaderCell("STT", 0));
                grid.Children.Add(CreateHeaderCell("Mã hàng", 1));
                grid.Children.Add(CreateHeaderCell("Tên mặt hàng", 2));
                grid.Children.Add(CreateHeaderCell("ĐVT", 3));
                grid.Children.Add(CreateHeaderCell("SL Sổ sách", 4));
                grid.Children.Add(CreateHeaderCell("SL Thực tế", 5));
                grid.Children.Add(CreateHeaderCell("Chênh lệch", 6));
                grid.Children.Add(CreateHeaderCell("Giá trị CL", 7, isLast: true));
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
            TxtFilterSummary.Text = $"Kho hàng: {khoName} | Nhóm: {nhomName} | Nhân viên: {nvName}";

            switch (_mode)
            {
                case ReportMode.DS_Ngay:
                    _dataDs = await _reportService.GetKiemKeDanhSachTheoNgayAsync(tuNgay, denNgay, khoId, nvId);
                    break;
                case ReportMode.TH_Ngay:
                    _dataTh = await _reportService.GetKiemKeTongHopTheoNgayAsync(tuNgay, denNgay, khoId, nhomId, nvId);
                    break;
                case ReportMode.TH_NV:
                    _dataTh = await _reportService.GetKiemKeTongHopTheoNhanVienAsync(tuNgay, denNgay, khoId, nhomId, nvId);
                    break;
                case ReportMode.DS_NV:
                    _dataDs = await _reportService.GetKiemKeDanhSachTheoNhanVienAsync(tuNgay, denNgay, khoId, nvId);
                    break;
            }

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();

            if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NV)
            {
                var filtered = _dataDs;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDs.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.KhoHang.ToLower().Contains(keyword) ||
                        x.NhanVien.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalChenhLech = 0;
                decimal totalGiaTri = 0;

                string groupProp = _mode == ReportMode.DS_Ngay ? "Ngay" : "NV";
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
                        totalChenhLech += item.ChenhLechSL;
                        totalGiaTri += item.GiaTriChenhLech;

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
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

                        rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.SoPhieu, 1, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.NgayDisplay, 2, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.KhoHang, 3, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.NhanVien, 4, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.ChenhLechSL.ToString("#,##0.##"), 5, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.GiaTriChenhLech.ToString("#,##0"), 6, HorizontalAlignment.Right, isLast: true));

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
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

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

                sumGrid.Children.Add(CreateCell(totalChenhLech.ToString("#,##0.##"), 5, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalGiaTri.ToString("#,##0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true));

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

                decimal totalSoSach = 0;
                decimal totalThucTe = 0;
                decimal totalChenhLech = 0;
                decimal totalGiaTri = 0;

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
                        totalSoSach += item.SoLuongSoSach;
                        totalThucTe += item.SoLuongThucTe;
                        totalChenhLech += item.ChenhLech;
                        totalGiaTri += item.GiaTriChenhLech;

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
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

                        rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.MaHang, 1, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.TenHang, 2, HorizontalAlignment.Left));
                        rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                        rowGrid.Children.Add(CreateCell(item.SoLuongSoSach.ToString("#,##0.##"), 4, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.SoLuongThucTe.ToString("#,##0.##"), 5, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.ChenhLech.ToString("#,##0.##"), 6, HorizontalAlignment.Right));
                        rowGrid.Children.Add(CreateCell(item.GiaTriChenhLech.ToString("#,##0"), 7, HorizontalAlignment.Right, isLast: true));

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
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

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

                sumGrid.Children.Add(CreateCell(totalSoSach.ToString("#,##0.##"), 4, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalThucTe.ToString("#,##0.##"), 5, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalChenhLech.ToString("#,##0.##"), 6, HorizontalAlignment.Right, isBold: true));
                sumGrid.Children.Add(CreateCell(totalGiaTri.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true, isLast: true));

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

                    if (_mode == ReportMode.DS_Ngay || _mode == ReportMode.DS_NV)
                    {
                        sb.AppendLine("STT,Số phiếu,Ngày,Kho hàng,Nhân viên,Chênh lệch SL,Giá trị chênh lệch");
                        foreach (var i in _dataDs)
                        {
                            sb.AppendLine($"{i.STT},\"{i.SoPhieu}\",\"{i.NgayDisplay}\",\"{i.KhoHang}\",\"{i.NhanVien}\",{i.ChenhLechSL},{i.GiaTriChenhLech}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,SL Sổ sách,SL Thực tế,Chênh lệch,Giá trị CL");
                        foreach (var i in _dataTh)
                        {
                            sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.SoLuongSoSach},{i.SoLuongThucTe},{i.ChenhLech},{i.GiaTriChenhLech}");
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
