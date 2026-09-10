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
    public partial class BaoCaoTongHopMatHangTheoNhomHienThiControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();
        private readonly LocalMatHangService _matHangService = new LocalMatHangService();
        private readonly LocalTheoDoiDatPhongService _theoDoiDatPhongService = new LocalTheoDoiDatPhongService();

        private bool _isLoaded = false;
        private List<TongHopMatHangTheoNhomHienThiItem> _allData = new List<TongHopMatHangTheoNhomHienThiItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopMatHangTheoNhomHienThiControl(string tabName = "TỔNG HỢP MẶT HÀNG THEO NHÓM HIỂN THỊ")
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
                // 1. Khu vực
                var kvList = new List<FilterComboItem>();
                try
                {
                    var dbKv = await _theoDoiDatPhongService.GetKhuVucLookupAsync();
                    if (dbKv != null)
                    {
                        kvList.AddRange(dbKv.Select(k => new FilterComboItem { Id = k.Id ?? "", Name = string.IsNullOrEmpty(k.Id) ? "" : (k.Name ?? ""), Icon = "📍" }));
                    }
                }
                catch { }
                if (kvList.Count == 0)
                {
                    kvList.Add(new FilterComboItem { Id = "", Name = "", Icon = "" });
                }
                CboKhuVuc.ItemsSource = kvList;
                CboKhuVuc.SelectedIndex = 0;

                // 2. Nhóm hiển thị
                var nhomList = new List<FilterComboItem>
                {
                    new FilterComboItem { Id = "", Name = "", Icon = "" }
                };
                try
                {
                    var dbNhom = await _matHangService.GetNhomMatHangListAsync();
                    if (dbNhom != null)
                    {
                        nhomList.AddRange(dbNhom.Select(n => new FilterComboItem { Id = n.Id ?? "", Name = n.Name ?? "", Icon = "📁" }));
                    }
                }
                catch { }

                CboNhomHienThi.ItemsSource = nhomList;
                CboNhomHienThi.SelectedIndex = 0;
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

            string khuVucId = (CboKhuVuc.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomHienThiId = (CboNhomHienThi.SelectedItem as FilterComboItem)?.Id ?? "";

            string kvText = (CboKhuVuc.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(kvText) || kvText.Contains("Tất cả")) kvText = "Tất cả";
            string nhomText = (CboNhomHienThi.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(nhomText) || nhomText.Contains("Tất cả")) nhomText = "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(kvText)) parts.Add($"Khu vực: {kvText}");
            if (Utilities.IsSpecificFilter(nhomText)) parts.Add($"Nhóm hiển thị: {nhomText}");
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

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            _allData = await _hoaDonService.GetTongHopMatHangTheoNhomHienThiAsync(tuNgay, denNgay, khuVucId, nhomHienThiId);

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
                    x.NhomHienThi.ToLower().Contains(keyword) ||
                    x.TenHang.ToLower().Contains(keyword)
                ).ToList();
            }

            if (filtered.Count == 0)
            {
                var emptyRow = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(6) };
                emptyRow.Child = new TextBlock { Text = "Không có dữ liệu báo cáo", FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Gray };
                StkDataRows.Children.Add(emptyRow);
                return;
            }

            // Group by NhomHienThi
            var groups = filtered.GroupBy(x => x.NhomHienThi).OrderBy(g => g.Key).ToList();

            decimal grandSoLuong = 0;
            decimal grandThanhTien = 0;

            foreach (var group in groups)
            {
                // Group Banner Row: Nhóm hiển thị:
                var groupHeaderBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 247, 250)),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 3, 6, 3)
                };

                string groupTitle = string.IsNullOrWhiteSpace(group.Key) ? "Nhóm hiển thị:" : $"Nhóm hiển thị: {group.Key}";
                var groupHeaderText = new TextBlock
                {
                    Text = groupTitle,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 51, 102))
                };
                groupHeaderBorder.Child = groupHeaderText;
                StkDataRows.Children.Add(groupHeaderBorder);

                int stt = 1;
                decimal grpSoLuong = 0;
                decimal grpThanhTien = 0;

                foreach (var item in group)
                {
                    grpSoLuong += item.SoLuong;
                    grpThanhTien += item.ThanhTien;

                    grandSoLuong += item.SoLuong;
                    grandThanhTien += item.ThanhTien;

                    var rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    // STT
                    rowGrid.Children.Add(CreateTableCell((stt++).ToString(), 0, HorizontalAlignment.Center));
                    // Tên hàng
                    rowGrid.Children.Add(CreateTableCell(item.TenHang, 1, HorizontalAlignment.Left));
                    // ĐVT
                    rowGrid.Children.Add(CreateTableCell(item.Dvt, 2, HorizontalAlignment.Center));
                    // Số lượng
                    rowGrid.Children.Add(CreateTableCell(item.SoLuong.ToString("#,##0.##"), 3, HorizontalAlignment.Right));
                    // Đơn giá
                    rowGrid.Children.Add(CreateTableCell(item.DonGia.ToString("#,##0"), 4, HorizontalAlignment.Right));
                    // Giảm giá %
                    rowGrid.Children.Add(CreateTableCell(item.GiamGiaPhanTram > 0 ? item.GiamGiaPhanTram.ToString("#,##0.##") : "0", 5, HorizontalAlignment.Right));
                    // Thành tiền
                    rowGrid.Children.Add(CreateTableCell(item.ThanhTien.ToString("#,##0"), 6, HorizontalAlignment.Right, isLast: true));

                    StkDataRows.Children.Add(rowGrid);
                }

                // Subtotal Row for Group (Tổng cộng)
                var subtotalGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(250, 250, 252)) };
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) }); // STT + TenHang + DVT
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });  // SoLuong
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) }); // DonGia + GiamGia
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // ThanhTien

                subtotalGrid.Children.Add(CreateTableCell("Tổng cộng", 0, HorizontalAlignment.Center, isBold: true));
                subtotalGrid.Children.Add(CreateTableCell(grpSoLuong.ToString("#,##0.##"), 1, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateTableCell("0", 2, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateTableCell(grpThanhTien.ToString("#,##0"), 3, HorizontalAlignment.Right, isBold: true, isLast: true));

                StkDataRows.Children.Add(subtotalGrid);
            }

            // Grand Total Row (TỔNG CỘNG)
            var totalGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(235, 240, 248)) };
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) }); // STT + TenHang + DVT
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });  // SoLuong
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) }); // DonGia + GiamGia
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // ThanhTien

            totalGrid.Children.Add(CreateTableCell("TỔNG CỘNG", 0, HorizontalAlignment.Center, isBold: true));
            totalGrid.Children.Add(CreateTableCell(grandSoLuong.ToString("#,##0.##"), 1, HorizontalAlignment.Right, isBold: true));
            totalGrid.Children.Add(CreateTableCell("", 2, HorizontalAlignment.Center, isBold: true));
            totalGrid.Children.Add(CreateTableCell(grandThanhTien.ToString("#,##0"), 3, HorizontalAlignment.Right, isBold: true, isLast: true));

            StkDataRows.Children.Add(totalGrid);
        }

        private Border CreateTableCell(string text, int columnIndex, HorizontalAlignment align, bool isBold = false, bool isLast = false)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, isLast ? 0 : 1, 1),
                Padding = new Thickness(4, 3, 4, 3)
            };

            Grid.SetColumn(border, columnIndex);

            var tb = new TextBlock
            {
                Text = text,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
            };

            border.Child = tb;
            return border;
        }

        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private async void Filter_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
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

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            BtnPrint_Click(sender, e);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "Tổng hợp mặt hàng theo nhóm hiển thị");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"TongHopMatHangTheoNhomHienThi_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtSubTitleDate.Text}\"");
                    sb.AppendLine($"\"{TxtFilterSummary.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine("STT,Nhóm hiển thị,Tên hàng,ĐVT,Số lượng,Đơn giá,Giảm giá %,Thành tiền");

                    var groups = _allData.GroupBy(x => x.NhomHienThi).OrderBy(g => g.Key);

                    foreach (var grp in groups)
                    {
                        int stt = 1;
                        foreach (var item in grp)
                        {
                            sb.AppendLine($"{stt++},\"{item.NhomHienThi}\",\"{item.TenHang}\",\"{item.Dvt}\",{item.SoLuong},{item.DonGia},{item.GiamGiaPhanTram},{item.ThanhTien}");
                        }
                    }

                    sb.AppendLine($",,,TỔNG CỘNG,{_allData.Sum(x => x.SoLuong)},,,{_allData.Sum(x => x.ThanhTien)}");

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
