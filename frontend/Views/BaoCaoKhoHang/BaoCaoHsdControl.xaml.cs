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
    public partial class BaoCaoHsdControl : UserControl
    {
        public enum ReportMode
        {
            TheoHsd,
            HetHan,
            TonCoHsd
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;

        private List<BaoCaoHsdItem> _allData = new List<BaoCaoHsdItem>();

        public BaoCaoHsdControl(ReportMode mode = ReportMode.TheoHsd)
        {
            InitializeComponent();
            _mode = mode;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            switch (_mode)
            {
                case ReportMode.TheoHsd:
                    TxtReportTitle.Text = "BÁO CÁO HÀNG HÓA THEO HẠN DÙNG";
                    LblTuNgay.Text = "Hạn dùng từ:";
                    break;
                case ReportMode.HetHan:
                    TxtReportTitle.Text = "BÁO CÁO HÀNG HÓA ĐÃ HẾT HẠN DÙNG";
                    LblTuNgay.Text = "Hết hạn từ:";
                    break;
                case ReportMode.TonCoHsd:
                    TxtReportTitle.Text = "BÁO CÁO HÀNG TỒN KHO CÓ HẠN DÙNG";
                    LblTuNgay.Text = "Từ:";
                    break;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = today;
            DpDenNgay.SelectedDate = today.AddMonths(1);
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

                var nhomList = await _reportService.GetNhomMatHangFilterAsync();
                CboNhomHang.ItemsSource = nhomList;
                CboNhomHang.SelectedIndex = 0;

                var mhList = await _reportService.GetMatHangFilterAsync();
                CboMatHang.ItemsSource = mhList;
                CboMatHang.SelectedIndex = 0;
            }
            catch { }
        }

        private void BuildTableHeader()
        {
            BrdTableHeader.Child = null;
            Grid grid = new Grid { Height = 28 };

            // STT(35), Mã hàng(75), Tên hàng(195), ĐVT(55), Số lô(80), Ngày SX(80), Hạn dùng(80), Tồn kho(60), Trạng thái(60) = 715 (~720)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(195) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            grid.Children.Add(CreateHeaderCell("STT", 0));
            grid.Children.Add(CreateHeaderCell("Mã hàng", 1));
            grid.Children.Add(CreateHeaderCell("Tên mặt hàng", 2));
            grid.Children.Add(CreateHeaderCell("ĐVT", 3));
            grid.Children.Add(CreateHeaderCell("Số lô", 4));
            grid.Children.Add(CreateHeaderCell("Ngày SX", 5));
            grid.Children.Add(CreateHeaderCell("Hạn dùng", 6));
            grid.Children.Add(CreateHeaderCell("Tồn kho", 7));
            grid.Children.Add(CreateHeaderCell("Trạng thái", 8, isLast: true));

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

        private Border CreateCell(string text, int col, HorizontalAlignment align = HorizontalAlignment.Left, bool isBold = false, bool isLast = false, Brush fg = null)
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
                Foreground = fg ?? Brushes.Black,
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
                TxtSubTitleDate.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

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

            switch (_mode)
            {
                case ReportMode.TheoHsd:
                    _allData = await _reportService.GetBaoCaoTheoHsdAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                    break;
                case ReportMode.HetHan:
                    _allData = await _reportService.GetBaoCaoHangHetHanAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                    break;
                case ReportMode.TonCoHsd:
                    _allData = await _reportService.GetBaoCaoTonKhoCoHsdAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                    break;
            }

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
                    x.SoLo.ToLower().Contains(keyword) ||
                    x.DVT.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totalTon = 0;
            int stt = 1;

            foreach (var item in filtered)
            {
                item.STT = stt++;
                totalTon += item.TonKho;

                Border rowBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 26
                };

                Grid rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(195) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

                Brush statusBrush = item.TrangThai == "Hết hạn" ? Brushes.Red : (item.TrangThai == "Cận hạn" ? Brushes.OrangeRed : Brushes.Green);

                rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.MaHang, 1, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TenHang, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.SoLo, 4, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.NgaySxDisplay, 5, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.HanDungDisplay, 6, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TonKho.ToString("#,##0.##"), 7, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TrangThai, 8, HorizontalAlignment.Center, isBold: true, isLast: true, fg: statusBrush));

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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(195) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

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

            sumGrid.Children.Add(CreateCell(totalTon.ToString("#,##0.##"), 7, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell("", 8, isBold: true, isLast: true));

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

                    sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Số lô,Ngày SX,Hạn dùng,Tồn kho,Trạng thái");
                    foreach (var i in _allData)
                    {
                        sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",\"{i.SoLo}\",\"{i.NgaySxDisplay}\",\"{i.HanDungDisplay}\",{i.TonKho},\"{i.TrangThai}\"");
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
