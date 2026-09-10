using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using static QuanLyBar.Client.Services.LocalBaoCaoDatHangService;

namespace QuanLyBar.Client.Views.BaoCaoDatHang
{
    public partial class BaoCaoTongHopDatHangTheoKhachHangControl : UserControl
    {
        private readonly LocalBaoCaoDatHangService _reportService = new LocalBaoCaoDatHangService();
        private bool _isLoaded = false;
        private List<BaoCaoTongHopDatHangTheoKhachHangItem> _allData = new List<BaoCaoTongHopDatHangTheoKhachHangItem>();

        public BaoCaoTongHopDatHangTheoKhachHangControl(string tabName = "TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG")
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = today;
            DpDenNgay.SelectedDate = today;

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
                var khList = await _reportService.GetKhachHangFilterAsync();
                CboKhachHang.ItemsSource = khList;
                CboKhachHang.SelectedIndex = 0;
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string khId = (CboKhachHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string khName = (CboKhachHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khName)) parts.Add($"Khách hàng: {khName}");
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

            _allData = await _reportService.GetTongHopDatHangTheoKhachHangAsync(tuNgay, denNgay, khId);
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
                    x.TenKhachHang.ToLower().Contains(keyword) ||
                    x.MaKhach.ToLower().Contains(keyword) ||
                    x.DiaChi.ToLower().Contains(keyword) ||
                    x.DienThoai.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totalTienHang = 0;
            decimal totalGiamGia = 0;
            decimal totalTongCong = 0;

            int stt = 1;
            foreach (var item in filtered)
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
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

                rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.MaKhach, 1, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TenKhachHang, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DiaChi, 3, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DienThoai, 4, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.TienHang > 0 ? item.TienHang.ToString("#,##0") : "0", 5, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.GiamGia > 0 ? item.GiamGia.ToString("#,##0") : "0", 6, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TongCong > 0 ? item.TongCong.ToString("#,##0") : "0", 7, HorizontalAlignment.Right));

                rowBorder.Child = rowGrid;
                StkDataRows.Children.Add(rowBorder);
            }

            // Dòng TỔNG CỘNG
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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            // Cột gộp TỔNG CỘNG
            Border spanBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(4, 0, 10, 0)
            };
            Grid.SetColumn(spanBorder, 0);
            Grid.SetColumnSpan(spanBorder, 5);
            spanBorder.Child = new TextBlock
            {
                Text = "TỔNG CỘNG",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            sumGrid.Children.Add(spanBorder);

            sumGrid.Children.Add(CreateCell(totalTienHang > 0 ? totalTienHang.ToString("#,##0") : "0", 5, HorizontalAlignment.Right, true));
            sumGrid.Children.Add(CreateCell(totalGiamGia > 0 ? totalGiamGia.ToString("#,##0") : "0", 6, HorizontalAlignment.Right, true));
            sumGrid.Children.Add(CreateCell(totalTongCong > 0 ? totalTongCong.ToString("#,##0") : "0", 7, HorizontalAlignment.Right, true));

            sumBorder.Child = sumGrid;
            StkDataRows.Children.Add(sumBorder);
        }

        private UIElement CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false)
        {
            Border b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, col == 7 ? 0 : 1, 0),
                Padding = new Thickness(4, 0, 4, 0)
            };
            Grid.SetColumn(b, col);

            TextBlock tb = new TextBlock
            {
                Text = text,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                FontSize = 11.5
            };
            b.Child = tb;
            return b;
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
            RenderTable();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(ReportPaper, "Tổng hợp đặt hàng theo khách hàng");
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(ReportPaper, "In tổng hợp đặt hàng theo khách hàng");
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"TongHopDatHangTheoKhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("STT,MaKhach,TenKhachHang,DiaChi,DienThoai,TienHang,GiamGia,TongCong");
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{item.STT}\",\"{item.MaKhach}\",\"{item.TenKhachHang}\",\"{item.DiaChi}\",\"{item.DienThoai}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
