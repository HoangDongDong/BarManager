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
    public partial class BaoCaoHangTonKhoControl : UserControl
    {
        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private bool _isLoaded = false;
        private List<BaoCaoHangTonKhoItem> _allData = new List<BaoCaoHangTonKhoItem>();

        public BaoCaoHangTonKhoControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
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
            if (DpDenNgay.SelectedDate == null) return;

            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;
            string khoId = (CboKhoHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string mhId = (CboMatHang.SelectedItem as FilterComboItem)?.Id ?? "";

            TxtSubTitleDate.Text = $"Tính đến ngày: {denNgay:dd/MM/yyyy}";

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

            _allData = await _reportService.GetBaoCaoHangTonKhoAsync(denNgay, khoId, nhomId, mhId);
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
                    x.KhoHang.ToLower().Contains(keyword) ||
                    x.DVT.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totalTon = 0;
            decimal totalMoney = 0;
            int stt = 1;

            foreach (var item in filtered)
            {
                item.STT = stt++;
                totalTon += item.SoLuongTon;
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
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.MaHang, 1, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TenHang, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DVT, 3, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.KhoHang, 4, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.SoLuongTon.ToString("#,##0.##"), 5, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.GiaVon.ToString("#,##0"), 6, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.ThanhTien.ToString("#,##0"), 7, HorizontalAlignment.Right, isLast: true));

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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
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

            sumGrid.Children.Add(CreateCell(totalTon.ToString("#,##0.##"), 5, HorizontalAlignment.Right, isBold: true));
            sumGrid.Children.Add(CreateCell("", 6, isBold: true));
            sumGrid.Children.Add(CreateCell(totalMoney.ToString("#,##0"), 7, HorizontalAlignment.Right, isBold: true, isLast: true));

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
                    printDlg.PrintVisual(ReportPaper, "Báo cáo hàng tồn kho");
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
                    FileName = $"BaoCaoHangTonKho_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO HÀNG TỒN KHO");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();

                    sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Kho hàng,SL tồn,Giá vốn,Thành tiền");
                    foreach (var i in _allData)
                    {
                        sb.AppendLine($"{i.STT},\"{i.MaHang}\",\"{i.TenHang}\",\"{i.DVT}\",\"{i.KhoHang}\",{i.SoLuongTon},{i.GiaVon},{i.ThanhTien}");
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
