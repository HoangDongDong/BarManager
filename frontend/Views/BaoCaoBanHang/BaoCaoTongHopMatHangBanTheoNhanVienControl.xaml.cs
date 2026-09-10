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
    public partial class BaoCaoTongHopMatHangBanTheoNhanVienControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<TongHopMatHangBanTheoNhanVienItem> _allData = new List<TongHopMatHangBanTheoNhanVienItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopMatHangBanTheoNhanVienControl(string tabName = "TỔNG HỢP MẶT HÀNG BÁN THEO NHÂN VIÊN")
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
                // 1. Kho xuất
                var khoList = new List<FilterComboItem>
                {
                    new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "🏢" }
                };
                try
                {
                    var dbKho = await LocalKhoHangService.GetAllWarehousesFlatAsync();
                    if (dbKho != null)
                    {
                        khoList.AddRange(dbKho.Select(k => new FilterComboItem { Id = k.Id ?? "", Name = k.Name ?? "", Icon = "📦" }));
                    }
                }
                catch { }
                CboKhoXuat.ItemsSource = khoList;
                CboKhoXuat.SelectedIndex = 0;

                // 2. Nhân viên xuất
                var nvList = new List<FilterComboItem>
                {
                    new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👥" }
                };
                try
                {
                    var dbNv = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
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

            string khoId = (CboKhoXuat.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhanVienId = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Id ?? "";

            string khoText = (CboKhoXuat.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(khoText) || khoText.Contains("Tất cả")) khoText = "Tất cả";
            string nvText = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(nvText) || nvText.Contains("Tất cả")) nvText = "Tất cả";
            var parts = new List<string>();
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

            TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            _allData = await _hoaDonService.GetTongHopMatHangBanTheoNhanVienAsync(tuNgay, denNgay, khoId, nhanVienId);

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
                    x.NhanVien.ToLower().Contains(keyword) ||
                    x.TenNhom.ToLower().Contains(keyword) ||
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

            // Level 1 Group: NhanVien
            var nvGroups = filtered.GroupBy(x => x.NhanVien).OrderBy(g => g.Key).ToList();

            decimal grandSoLuong = 0;
            decimal grandThanhTien = 0;

            foreach (var nvGroup in nvGroups)
            {
                // Level 1 Banner Row: Nhân viên bán:
                var nvHeaderBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 244, 250)),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 3, 6, 3)
                };

                var nvHeaderText = new TextBlock
                {
                    Text = $"Nhân viên bán: {nvGroup.Key}",
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 51, 102))
                };
                nvHeaderBorder.Child = nvHeaderText;
                StkDataRows.Children.Add(nvHeaderBorder);

                // Level 2 Group: TenNhom
                var catGroups = nvGroup.GroupBy(x => x.TenNhom).OrderBy(g => g.Key).ToList();

                foreach (var catGroup in catGroups)
                {
                    // Level 2 Banner Row: Nhóm hàng:
                    var catHeaderBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(250, 250, 252)),
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Padding = new Thickness(12, 3, 6, 3)
                    };

                    var catHeaderText = new TextBlock
                    {
                        Text = $"Nhóm hàng: {catGroup.Key}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30))
                    };
                    catHeaderBorder.Child = catHeaderText;
                    StkDataRows.Children.Add(catHeaderBorder);

                    int stt = 1;
                    foreach (var item in catGroup)
                    {
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
                }
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
                    printDlg.PrintVisual(ReportPaper, "Tổng hợp mặt hàng bán theo nhân viên");
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
                    FileName = $"TongHopMatHangBanTheoNhanVien_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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
                    sb.AppendLine("STT,Nhân viên bán,Nhóm hàng,Tên hàng,ĐVT,Số lượng,Đơn giá,Giảm giá %,Thành tiền");

                    var nvGroups = _allData.GroupBy(x => x.NhanVien).OrderBy(g => g.Key);

                    foreach (var nvGroup in nvGroups)
                    {
                        var catGroups = nvGroup.GroupBy(x => x.TenNhom).OrderBy(g => g.Key);
                        foreach (var catGroup in catGroups)
                        {
                            int stt = 1;
                            foreach (var item in catGroup)
                            {
                                sb.AppendLine($"{stt++},\"{item.NhanVien}\",\"{item.TenNhom}\",\"{item.TenHang}\",\"{item.Dvt}\",{item.SoLuong},{item.DonGia},{item.GiamGiaPhanTram},{item.ThanhTien}");
                            }
                        }
                    }

                    sb.AppendLine($",,,,TỔNG CỘNG,{_allData.Sum(x => x.SoLuong)},,,{_allData.Sum(x => x.ThanhTien)}");

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
