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

namespace QuanLyBar.Client.Views.BaoCaoBanHang
{
    public partial class BaoCaoTongHopMatHangBanTheoKhachHangControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<TongHopMatHangBanTheoKhachHangItem> _allData = new List<TongHopMatHangBanTheoKhachHangItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopMatHangBanTheoKhachHangControl(string tabName = "TỔNG HỢP MẶT HÀNG BÁN THEO KHÁCH HÀNG")
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
                // 1. Nhóm khách hàng
                var nhomList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👥" } };
                try
                {
                    var treeNhom = await LocalKhachHangService.GetNhomKhachHangTreeAsync();
                    if (treeNhom != null)
                    {
                        foreach (var item in treeNhom)
                        {
                            if (item.Id != "ALL" && item.Id != "UNASSIGNED")
                            {
                                nhomList.Add(new FilterComboItem { Id = item.Id ?? "", Name = item.Name ?? "", Icon = "👥" });
                            }
                        }
                    }
                }
                catch { }
                CboNhomKhach.ItemsSource = nhomList;
                CboNhomKhach.SelectedIndex = 0;

                // 2. Khách hàng
                var khList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👤" } };
                try
                {
                    var dbKh = await _hoaDonService.GetKhachHangLookupAsync();
                    if (dbKh != null)
                    {
                        khList.AddRange(dbKh.Select(k => new FilterComboItem { Id = k.Id ?? "", Name = k.Name ?? "", Icon = "👤" }));
                    }
                }
                catch { }
                CboKhachHang.ItemsSource = khList;
                CboKhachHang.SelectedIndex = 0;

                // 3. Mặt hàng
                var mhList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "🍹" } };
                CboMatHang.ItemsSource = mhList;
                CboMatHang.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadFiltersAsync: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string nhomKhachId = (CboNhomKhach.SelectedItem as FilterComboItem)?.Id ?? "";
            string khachHangId = (CboKhachHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string matHangId = (CboMatHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string nhomKhachText = (CboNhomKhach.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(nhomKhachText)) nhomKhachText = "Tất cả";
            string khText = (CboKhachHang.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(khText)) khText = "Tất cả";
            string mhText = (CboMatHang.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(mhText)) mhText = "Tất cả";
            TxtFilterSummary.Text = $"Nhóm khách: {nhomKhachText} | Khách hàng: {khText} | Mặt hàng: {mhText}";

            _allData = await _hoaDonService.GetTongHopMatHangBanTheoKhachHangAsync(tuNgay, denNgay, nhomKhachId, khachHangId, matHangId);

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
                    x.KhachHang.ToLower().Contains(keyword) ||
                    x.TenHang.ToLower().Contains(keyword)
                ).ToList();
            }

            var grouped = filtered.GroupBy(x => x.KhachHang).ToList();

            foreach (var group in grouped)
            {
                // Group Header Row
                Border groupHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };

                TextBlock txtGroup = new TextBlock
                {
                    Text = $"Khách hàng: {group.Key}",
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

                    Border rowBorder = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Height = 26
                    };

                    Grid rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                    rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.TenHang, 1, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.Dvt, 2, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.SoLuong > 0 ? item.SoLuong.ToString("#,##0.##") : "0", 3, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.DonGia > 0 ? item.DonGia.ToString("#,##0") : "0", 4, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.GiamGiaPhanTram > 0 ? item.GiamGiaPhanTram.ToString("#,##0.##") : "0", 5, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhTien > 0 ? item.ThanhTien.ToString("#,##0") : "0", 6, HorizontalAlignment.Right));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                }
            }
        }

        private UIElement CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false)
        {
            Border b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, col == 6 ? 0 : 1, 0),
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

        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
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
            BtnIn_Click(sender, e);
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "Tổng hợp mặt hàng bán theo khách hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"TongHopMatHangBanTheoKhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtSubTitleDate.Text}\"");
                    sb.AppendLine($"\"{TxtFilterSummary.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("\"STT\",\"Khách hàng\",\"Tên hàng\",\"ĐVT\",\"Số lượng\",\"Đơn giá\",\"Giảm giá %\",\"Thành tiền\"");

                    int stt = 1;
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{stt++}\",\"{item.KhachHang}\",\"{item.TenHang}\",\"{item.Dvt}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.GiamGiaPhanTram}\",\"{item.ThanhTien}\"");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel (CSV) thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
