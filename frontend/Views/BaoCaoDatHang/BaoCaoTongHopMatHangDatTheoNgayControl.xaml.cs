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
    public partial class BaoCaoTongHopMatHangDatTheoNgayControl : UserControl
    {
        private readonly LocalBaoCaoDatHangService _reportService = new LocalBaoCaoDatHangService();
        private bool _isLoaded = false;
        private List<BaoCaoTongHopMatHangDatTheoNgayItem> _allData = new List<BaoCaoTongHopMatHangDatTheoNgayItem>();

        public BaoCaoTongHopMatHangDatTheoNgayControl(string tabName = "TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY")
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
                var mhList = await _reportService.GetMatHangFilterAsync();
                CboMatHang.ItemsSource = mhList;
                CboMatHang.SelectedIndex = 0;

                var nhomMhList = await _reportService.GetNhomMatHangFilterAsync();
                CboNhomMatHang.ItemsSource = nhomMhList;
                CboNhomMatHang.SelectedIndex = 0;

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

            string mhId = (CboMatHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomMhId = (CboNhomMatHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string khId = (CboKhachHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string nhomName = (CboNhomMatHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string mhName = (CboMatHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string khName = (CboKhachHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm hàng: {nhomName}");
            if (Utilities.IsSpecificFilter(mhName)) parts.Add($"Mặt hàng: {mhName}");
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

            _allData = await _reportService.GetTongHopMatHangDatTheoNgayAsync(tuNgay, denNgay, mhId, nhomMhId, khId);
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
                    x.TenMatHang.ToLower().Contains(keyword) ||
                    x.KhachHang.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totalSoLuong = 0;
            decimal totalThanhTien = 0;

            var grouped = filtered.GroupBy(x => x.NgayDisplay).ToList();

            foreach (var group in grouped)
            {
                // Group header row: Ngày: dd/MM/yyyy
                Border groupHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };

                TextBlock txtGroup = new TextBlock
                {
                    Text = $"Ngày: {group.Key}",
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    FontSize = 12
                };
                groupHeader.Child = txtGroup;
                StkDataRows.Children.Add(groupHeader);

                int stt = 1;
                foreach (var item in group)
                {
                    item.STT = stt++;
                    totalSoLuong += item.SoLuong;
                    totalThanhTien += item.ThanhTien;

                    Border rowBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Height = 26
                    };

                    Grid rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                    rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.TenMatHang, 1, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.DVT, 2, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.SoLuong.ToString("#,##0.##"), 3, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.DonGia > 0 ? item.DonGia.ToString("#,##0") : "0", 4, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhTien > 0 ? item.ThanhTien.ToString("#,##0") : "0", 5, HorizontalAlignment.Right));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                }
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
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

            // Cột gộp TỔNG CỘNG
            Border spanBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(4, 0, 10, 0)
            };
            Grid.SetColumn(spanBorder, 0);
            Grid.SetColumnSpan(spanBorder, 3);
            spanBorder.Child = new TextBlock
            {
                Text = "TỔNG CỘNG",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            sumGrid.Children.Add(spanBorder);

            sumGrid.Children.Add(CreateCell(totalSoLuong.ToString("#,##0.##"), 3, HorizontalAlignment.Right, true));
            sumGrid.Children.Add(CreateCell("", 4, HorizontalAlignment.Right, true));
            sumGrid.Children.Add(CreateCell(totalThanhTien > 0 ? totalThanhTien.ToString("#,##0") : "0", 5, HorizontalAlignment.Right, true));

            sumBorder.Child = sumGrid;
            StkDataRows.Children.Add(sumBorder);
        }

        private UIElement CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false)
        {
            Border b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, col == 5 ? 0 : 1, 0),
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
                pd.PrintVisual(ReportPaper, "Tổng hợp mặt hàng đặt theo ngày");
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(ReportPaper, "In tổng hợp mặt hàng đặt theo ngày");
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"TongHopMatHangDatTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY");
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("STT,Ngay,TenMatHang,DVT,SoLuong,DonGia,ThanhTien");
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{item.STT}\",\"{item.NgayDisplay}\",\"{item.TenMatHang}\",\"{item.DVT}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.ThanhTien}\"");
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
