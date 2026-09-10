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
    public partial class BaoCaoTongHopBanTheoKhachHangControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<TongHopBanTheoKhachHangItem> _allData = new List<TongHopBanTheoKhachHangItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopBanTheoKhachHangControl(string tabName = "TỔNG HỢP BÁN THEO KHÁCH HÀNG")
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
                // 1. Khách hàng
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

                // 2. Kho xuất
                var khoList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "🏬" } };
                try
                {
                    var dbKho = await LocalKhoHangService.GetAllWarehousesFlatAsync();
                    if (dbKho != null)
                    {
                        khoList.AddRange(dbKho.Select(k => new FilterComboItem { Id = k.Id ?? "", Name = k.Name ?? "", Icon = "🏬" }));
                    }
                }
                catch { }
                CboKhoXuat.ItemsSource = khoList;
                CboKhoXuat.SelectedIndex = 0;

                // 3. Nhân viên xuất
                var nvList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👤" } };
                try
                {
                    var dbNv = await LocalNhanVienService.GetNhanVienFlatListAsync();
                    if (dbNv != null)
                    {
                        nvList.AddRange(dbNv.Select(n => new FilterComboItem { Id = n.Id ?? "", Name = n.Name ?? "", Icon = "👤" }));
                    }
                }
                catch { }
                CboNhanVienXuat.ItemsSource = nvList;
                CboNhanVienXuat.SelectedIndex = 0;
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

            string khachHangId = (CboKhachHang.SelectedItem as FilterComboItem)?.Id ?? "";
            string khoId = (CboKhoXuat.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhanVienId = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string khText = (CboKhachHang.SelectedItem as FilterComboItem)?.Name;
            
            string khoText = (CboKhoXuat.SelectedItem as FilterComboItem)?.Name;
            
            string nvText = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Name;
            
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khText)) parts.Add($"Khách hàng: {khText}");
            if (Utilities.IsSpecificFilter(khoText)) parts.Add($"Kho xuất: {khoText}");
            if (Utilities.IsSpecificFilter(nvText)) parts.Add($"NV xuất: {nvText}");
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

            _allData = await _hoaDonService.GetTongHopBanTheoKhachHangAsync(tuNgay, denNgay, khachHangId, khoId, nhanVienId);

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
                    x.MaKhach.ToLower().Contains(keyword) ||
                    x.TenKhach.ToLower().Contains(keyword) ||
                    x.DiaChi.ToLower().Contains(keyword)
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
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                rowGrid.Children.Add(CreateCell(item.MaKhach, 1, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.TenKhach, 2, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.DiaChi, 3, HorizontalAlignment.Left));
                rowGrid.Children.Add(CreateCell(item.TienHang > 0 ? item.TienHang.ToString("#,##0") : "0", 4, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.GiamGia > 0 ? item.GiamGia.ToString("#,##0") : "0", 5, HorizontalAlignment.Right));
                rowGrid.Children.Add(CreateCell(item.TongCong > 0 ? item.TongCong.ToString("#,##0") : "0", 6, HorizontalAlignment.Right));

                rowBorder.Child = rowGrid;
                StkDataRows.Children.Add(rowBorder);
            }

            TxtTotalTienHang.Text = totalTienHang > 0 ? totalTienHang.ToString("#,##0") : "0";
            TxtTotalGiamGia.Text = totalGiamGia > 0 ? totalGiamGia.ToString("#,##0") : "0";
            TxtTotalTongCong.Text = totalTongCong > 0 ? totalTongCong.ToString("#,##0") : "0";
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
                    printDlg.PrintVisual(ReportPaper, "Tổng hợp bán theo khách hàng");
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
                    FileName = $"TongHopBanTheoKhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("\"STT\",\"Mã khách\",\"Tên khách hàng\",\"Địa chỉ\",\"Tiền hàng\",\"Giảm giá\",\"Tổng cộng\"");

                    int stt = 1;
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{stt++}\",\"{item.MaKhach}\",\"{item.TenKhach}\",\"{item.DiaChi}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
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
