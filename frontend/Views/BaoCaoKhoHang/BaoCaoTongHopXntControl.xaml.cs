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
    public partial class BaoCaoTongHopXntControl : UserControl
    {
        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private bool _isLoaded = false;
        private List<BaoCaoTongHopXntItem> _allData = new List<BaoCaoTongHopXntItem>();

        public BaoCaoTongHopXntControl()
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
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string mhId = (CboMatHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            string khoName = (CboKhoHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nhomName = (CboNhomHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string mhName = (CboMatHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            TxtFilterSummary.Text = $"Kho hàng: {khoName} | Nhóm: {nhomName} | Mặt hàng: {mhName}";

            _allData = await _reportService.GetBaoCaoTongHopXntAsync(tuNgay, denNgay, khoId, nhomId, mhId);
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

            decimal totalTonDau = 0;
            decimal totalNhap = 0;
            decimal totalXuat = 0;
            decimal totalTonCuoi = 0;
            decimal totalTienTon = 0;
            int stt = 1;

            foreach (var item in filtered)
            {
                item.STT = stt++;
                totalTonDau += item.TonDau;
                totalNhap += item.Nhap;
                totalXuat += item.Xuat;
                totalTonCuoi += item.TonCuoi;
                totalTienTon += item.TienTon;

                Border rowBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 26
                };

                Grid rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.MaHang, 1, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TenHang, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TonDau.ToString("#,##0.##"), 4, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.Nhap.ToString("#,##0.##"), 5, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.Xuat.ToString("#,##0.##"), 6, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TonCuoi.ToString("#,##0.##"), 7, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.GiaVon.ToString("#,##0"), 8, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TienTon.ToString("#,##0"), 9, HorizontalAlignment.Right, isLast: true));

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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

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

            sumGrid.Children.Add(CreateCell(totalTonDau.ToString("#,##0.##"), 4, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalNhap.ToString("#,##0.##"), 5, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalXuat.ToString("#,##0.##"), 6, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell(totalTonCuoi.ToString("#,##0.##"), 7, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell("", 8, isBold: true));
            sumGrid.Children.Add(CreateCell(totalTienTon.ToString("#,##0"), 9, HorizontalAlignment.Right, isBold: true, isLast: true));

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
                    printDlg.PrintVisual(ReportPaper, "Báo cáo tổng hợp xuất nhập tồn");
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
                    FileName = $"BaoCaoTongHopXNT_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();

                    sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Tồn đầu,Nhập,Xuất,Tồn cuối,Giá vốn,Tiền tồn");
                    foreach (var i in _allData)
                    {
                        sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",{i.TonDau},{i.Nhap},{i.Xuat},{i.TonCuoi},{i.GiaVon},{i.TienTon}");
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
