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
    public partial class BaoCaoTongHopHoaHongTheoNvkdControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<TongHopHoaHongTheoNvkdItem> _allData = new List<TongHopHoaHongTheoNvkdItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopHoaHongTheoNvkdControl(string tabName = "TỔNG HỢP HOA HỒNG THEO NVKD")
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
                // 1. Kho xuất
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

                // 2. Nhân viên bán & Nhân viên xuất
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
                CboNhanVienBan.ItemsSource = nvList;
                CboNhanVienBan.SelectedIndex = 0;

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
            string nhanVienBanId = (CboNhanVienBan.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhanVienXuatId = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string khoText = (CboKhoXuat.SelectedItem as FilterComboItem)?.Name;
            
            string nvBanText = (CboNhanVienBan.SelectedItem as FilterComboItem)?.Name;
            
            string nvXuatText = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Name;
            
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khoText)) parts.Add($"Kho xuất: {khoText}");
            if (Utilities.IsSpecificFilter(nvBanText)) parts.Add($"NV bán: {nvBanText}");
            if (Utilities.IsSpecificFilter(nvXuatText)) parts.Add($"NV xuất: {nvXuatText}");
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

            _allData = await _hoaDonService.GetTongHopHoaHongTheoNvkdAsync(tuNgay, denNgay, khoId, nhanVienBanId, nhanVienXuatId);

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
                    x.NhanVienBan.ToLower().Contains(keyword) ||
                    x.TenHang.ToLower().Contains(keyword)
                ).ToList();
            }

            var grouped = filtered.GroupBy(x => x.NhanVienBan).ToList();
            decimal grandTotalThanhTien = 0;

            foreach (var group in grouped)
            {
                // Group Header Row (e.g. "Nhân viên bán:")
                Border groupHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };

                string groupTitle = group.Key;
                if (!groupTitle.EndsWith(":") && !groupTitle.Contains(":"))
                {
                    groupTitle = $"Nhân viên bán: {groupTitle}";
                }

                TextBlock txtGroup = new TextBlock
                {
                    Text = groupTitle,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    FontSize = 12
                };
                groupHeader.Child = txtGroup;
                StkDataRows.Children.Add(groupHeader);

                int stt = 1;
                decimal groupThanhTien = 0;

                foreach (var item in group)
                {
                    item.STT = stt++;
                    groupThanhTien += item.ThanhTien;

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
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

                    rowGrid.Children.Add(CreateCell(item.STT.ToString(), 0, HorizontalAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.TenHang, 1, HorizontalAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.SoLuong > 0 ? item.SoLuong.ToString("#,##0.##") : "0", 2, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.HoaHong > 0 ? item.HoaHong.ToString("#,##0.00") : "0.00", 3, HorizontalAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhTien > 0 ? item.ThanhTien.ToString("#,##0") : "0", 4, HorizontalAlignment.Right));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                }

                grandTotalThanhTien += groupThanhTien;

                // Group Subtotal Row ("Tổng cộng")
                Border subtotalBorder = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = 26
                };

                Grid subtotalGrid = new Grid();
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

                subtotalGrid.Children.Add(CreateCell("Tổng cộng", 3, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateCell(groupThanhTien > 0 ? groupThanhTien.ToString("#,##0") : "0", 4, HorizontalAlignment.Right, isBold: true));

                subtotalBorder.Child = subtotalGrid;
                StkDataRows.Children.Add(subtotalBorder);
            }

            TxtGrandTotalThanhTien.Text = grandTotalThanhTien > 0 ? grandTotalThanhTien.ToString("#,##0") : "0";
        }

        private UIElement CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false)
        {
            Border b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, col == 4 ? 0 : 1, 0),
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
                    printDlg.PrintVisual(ReportPaper, "Tổng hợp hoa hồng theo NVKD");
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
                    FileName = $"TongHopHoaHongTheoNVKD_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("\"STT\",\"Nhân viên bán\",\"Tên hàng\",\"Số lượng\",\"Hoa hồng\",\"Thành tiền\"");

                    int stt = 1;
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{stt++}\",\"{item.NhanVienBan}\",\"{item.TenHang}\",\"{item.SoLuong}\",\"{item.HoaHong}\",\"{item.ThanhTien}\"");
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
