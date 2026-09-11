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
    public partial class BaoCaoTongHopXntChiTietControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private bool _isLoaded = false;
        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;
        private List<BaoCaoTongHopXntChiTietItem> _allData = new List<BaoCaoTongHopXntChiTietItem>();

        public BaoCaoTongHopXntChiTietControl()
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

        private void BuildTableHeader()
        {
            double scale = _currentLayout != null ? _currentLayout.GetScaleFactor() : 1.0;
            double hFontSize = _currentLayout?.HeaderFontSize ?? 11.0;

            Grid headerGrid = new Grid();
            headerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
            headerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });

            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(30 * scale)) });  // 0: STT
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(60 * scale)) });  // 1: Mã
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(150 * scale)) }); // 2: Tên
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(40 * scale)) });  // 3: ĐVT
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });  // 4: Tồn đầu SL
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });  // 5: Tồn đầu TT
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });  // 6: Nhập SL
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });  // 7: Nhập TT
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });  // 8: Xuất SL
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });  // 9: Xuất TT
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });  // 10: Tồn cuối SL
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });  // 11: Tồn cuối TT

            // Top row spanning 2 rows
            headerGrid.Children.Add(CreateSpannedHeaderCell("STT", 0, 0, 2, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("Mã hàng", 0, 1, 2, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("Tên mặt hàng", 0, 2, 2, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("ĐVT", 0, 3, 2, 1, hFontSize, hasRightBorder: true));

            // Top row group headers
            headerGrid.Children.Add(CreateSpannedHeaderCell("TỒN ĐẦU", 0, 4, 1, 2, hFontSize, hasRightBorder: true, hasBottomBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("NHẬP KHO", 0, 6, 1, 2, hFontSize, hasRightBorder: true, hasBottomBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("XUẤT KHO", 0, 8, 1, 2, hFontSize, hasRightBorder: true, hasBottomBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("TỒN CUỐI", 0, 10, 1, 2, hFontSize, hasRightBorder: false, hasBottomBorder: true));

            // Bottom row sub headers
            headerGrid.Children.Add(CreateSpannedHeaderCell("SL", 1, 4, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("Tiền", 1, 5, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("SL", 1, 6, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("Tiền", 1, 7, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("SL", 1, 8, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("Tiền", 1, 9, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("SL", 1, 10, 1, 1, hFontSize, hasRightBorder: true));
            headerGrid.Children.Add(CreateSpannedHeaderCell("Tiền", 1, 11, 1, 1, hFontSize, hasRightBorder: false));

            BrdTableHeader.Child = headerGrid;
        }

        private Border CreateSpannedHeaderCell(string text, int row, int col, int rowSpan, int colSpan, double fontSize, bool hasRightBorder, bool hasBottomBorder = false)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, hasRightBorder ? 1 : 0, hasBottomBorder ? 1 : 0),
                Padding = new Thickness(2, 2, 2, 2)
            };
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            if (rowSpan > 1) Grid.SetRowSpan(b, rowSpan);
            if (colSpan > 1) Grid.SetColumnSpan(b, colSpan);

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

            _allData = await _reportService.GetBaoCaoTongHopXntChiTietAsync(tuNgay, denNgay, khoId, nhomId, mhId);
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

            double scale = _currentLayout != null ? _currentLayout.GetScaleFactor() : 1.0;
            decimal totalTonDauSL = 0;
            decimal totalTonDauTT = 0;
            decimal totalNhapSL = 0;
            decimal totalNhapTT = 0;
            decimal totalXuatSL = 0;
            decimal totalXuatTT = 0;
            decimal totalTonCuoiSL = 0;
            decimal totalTonCuoiTT = 0;
            int stt = 1;

            foreach (var item in filtered)
            {
                item.STT = stt++;
                totalTonDauSL += item.TonDauSL;
                totalTonDauTT += item.TonDauTT;
                totalNhapSL += item.NhapSL;
                totalNhapTT += item.NhapTT;
                totalXuatSL += item.XuatSL;
                totalXuatTT += item.XuatTT;
                totalTonCuoiSL += item.TonCuoiSL;
                totalTonCuoiTT += item.TonCuoiTT;

                Border rowBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 26
                };

                Grid rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(30 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(60 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(150 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(40 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });

                rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.MaHang, 1, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TenHang, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TonDauSL.ToString("#,##0.##"), 4, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TonDauTT.ToString("#,##0"), 5, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.NhapSL.ToString("#,##0.##"), 6, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.NhapTT.ToString("#,##0"), 7, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.XuatSL.ToString("#,##0.##"), 8, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.XuatTT.ToString("#,##0"), 9, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TonCuoiSL.ToString("#,##0.##"), 10, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TonCuoiTT.ToString("#,##0"), 11, HorizontalAlignment.Right, isLast: true));

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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(30 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(60 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(150 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(40 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(45 * scale)) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Round(65 * scale)) });

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
                FontSize = _currentLayout?.CellFontSize ?? 10.5,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            sumGrid.Children.Add(span);

            sumGrid.Children.Add(CreateCell(totalTonDauSL.ToString("#,##0.##"), 4, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalTonDauTT.ToString("#,##0"), 5, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalNhapSL.ToString("#,##0.##"), 6, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalNhapTT.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalXuatSL.ToString("#,##0.##"), 8, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalXuatTT.ToString("#,##0"), 9, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalTonCuoiSL.ToString("#,##0.##"), 10, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalTonCuoiTT.ToString("#,##0"), 11, HorizontalAlignment.Right, isBold: true, isLast: true));

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
                    printDlg.PrintVisual(ReportPaper, "Báo cáo tổng hợp xuất nhập tồn chi tiết");
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
                    FileName = $"BaoCaoTongHopXNTChiTiet_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN CHI TIẾT");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();

                    sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Tồn đầu SL,Tồn đầu TT,Nhập SL,Nhập TT,Xuất SL,Xuất TT,Tồn cuối SL,Tồn cuối TT");
                    foreach (var i in _allData)
                    {
                        sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.TonDauSL},{i.TonDauTT},{i.NhapSL},{i.NhapTT},{i.XuatSL},{i.XuatTT},{i.TonCuoiSL},{i.TonCuoiTT}");
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
                    onLayoutCallback: layout => _ = LoadTemplateConfigAsync(layout));
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
