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
    public partial class BaoCaoTheKhoControl : UserControl
    {
        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private bool _isLoaded = false;
        private List<BaoCaoTheKhoItem> _allData = new List<BaoCaoTheKhoItem>();

        public BaoCaoTheKhoControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = new DateTime(today.Year, today.Month, 1);
            DpDenNgay.SelectedDate = today;
            TxtSignDate.Text = $"Ngày {today:dd} tháng {today:MM} năm {today:yyyy}";

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

                var mhList = await _reportService.GetMatHangFilterAsync();
                CboMatHang.ItemsSource = mhList;
                if (mhList.Count > 1)
                {
                    CboMatHang.SelectedIndex = 1; // Pick the first actual item
                }
                else
                {
                    CboMatHang.SelectedIndex = 0;
                }
            }
            catch { }
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
            string khoName = (CboKhoHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string mhId = (CboMatHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string mhName = (CboMatHang.SelectedItem as FilterComboItem)?.Name ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            TxtFilterSummary.Text = $"Kho hàng: {khoName} | Mặt hàng: {(string.IsNullOrEmpty(mhName) ? "Tất cả" : mhName)}";

            TxtMatHangInfo.Text = $"Tên mặt hàng: {mhName}";
            TxtKhoHangInfo.Text = $"Kho hàng: {khoName}";

            _allData = await _reportService.GetBaoCaoTheKhoAsync(tuNgay, denNgay, khoId, mhId);

            if (_allData.Count > 0)
            {
                TxtMaHangInfo.Text = $"Mã hàng: {_allData[0].MaHang}";
                TxtDvtInfo.Text = $"Đơn vị tính: {_allData[0].DVT}";
            }
            else
            {
                TxtMaHangInfo.Text = "Mã hàng: ";
                TxtDvtInfo.Text = "Đơn vị tính: ";
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
                    x.SoPhieu.ToLower().Contains(keyword) ||
                    x.DienGiai.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totalNhap = 0;
            decimal totalXuat = 0;
            decimal lastTon = 0;
            decimal lastTienTon = 0;

            foreach (var item in filtered)
            {
                totalNhap += item.NhapSL;
                totalXuat += item.XuatSL;
                lastTon = item.TonSL;
                lastTienTon = item.ThanhTienTon;

                Border rowBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 26
                };

                Grid rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(185) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                rowGrid.Children.Add(CreateCell(item.NgayDisplay, 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.SoPhieu, 1, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.DienGiai, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DonGia > 0 ? item.DonGia.ToString("#,##0") : "", 3, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.NhapSL > 0 ? item.NhapSL.ToString("#,##0.##") : "", 4, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.XuatSL > 0 ? item.XuatSL.ToString("#,##0.##") : "", 5, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TonSL.ToString("#,##0.##"), 6, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.ThanhTienTon.ToString("#,##0"), 7, HorizontalAlignment.Right, isLast: true));

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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(185) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

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
                Text = "TỔNG CỘNG PHÁT SINH",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            sumGrid.Children.Add(span);

            sumGrid.Children.Add(CreateCell(totalNhap.ToString("#,##0.##"), 4, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalXuat.ToString("#,##0.##"), 5, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(lastTon.ToString("#,##0.##"), 6, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(lastTienTon.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true, isLast: true));

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
                    printDlg.PrintVisual(ReportPaper, "Thẻ kho");
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
                    FileName = $"TheKho_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("THẺ KHO");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine($"{TxtMatHangInfo.Text}, {TxtMaHangInfo.Text}, {TxtDvtInfo.Text}, {TxtKhoHangInfo.Text}");
                    sb.AppendLine();

                    sb.AppendLine("Ngày,Số phiếu,Diễn giải,Đơn giá,Nhập,Xuất,Tồn,Thành tiền tồn");
                    foreach (var i in _allData)
                    {
                        sb.AppendLine($"\"{i.NgayDisplay}\",\"{i.SoPhieu}\",\"{i.DienGiai}\",{i.DonGia},{i.NhapSL},{i.XuatSL},{i.TonSL},{i.ThanhTienTon}");
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
