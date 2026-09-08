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
    public partial class BaoCaoTongHopMatHangBanTheoThuNganControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<TongHopMatHangBanTheoThuNganItem> _allData = new List<TongHopMatHangBanTheoThuNganItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopMatHangBanTheoThuNganControl(string tabName = "TỔNG HỢP MẶT HÀNG BÁN THEO THU NGÂN")
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

                // 2. Thanh toán bởi
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
                CboThanhToanBoi.ItemsSource = nvList;
                CboThanhToanBoi.SelectedIndex = 0;
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
            string thanhToanBoiId = (CboThanhToanBoi.SelectedItem as FilterComboItem)?.Id ?? "";

            string khoText = (CboKhoXuat.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(khoText) || khoText.Contains("Tất cả")) khoText = "Tất cả";
            string tnText = (CboThanhToanBoi.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(tnText) || tnText.Contains("Tất cả")) tnText = "Tất cả";
            TxtFilterSummary.Text = $"Kho xuất: {khoText} | Thu ngân: {tnText}";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            _allData = await _hoaDonService.GetTongHopMatHangBanTheoThuNganAsync(tuNgay, denNgay, khoId, thanhToanBoiId);

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
                    x.ThuNgan.ToLower().Contains(keyword) ||
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

            // Group level 1: ThuNgan
            var cashierGroups = filtered.GroupBy(x => x.ThuNgan).OrderBy(g => g.Key).ToList();

            decimal grandSoLuong = 0;
            decimal grandThanhTien = 0;

            foreach (var cashierGroup in cashierGroups)
            {
                // Level 1 Header: Thu ngân: Administrator
                var cashierHeaderBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 243, 248)),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };
                var cashierHeaderTb = new TextBlock
                {
                    Text = $"Thu ngân: {cashierGroup.Key}",
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Foreground = Brushes.Black
                };
                cashierHeaderBorder.Child = cashierHeaderTb;
                StkDataRows.Children.Add(cashierHeaderBorder);

                decimal cashierSoLuong = 0;
                decimal cashierThanhTien = 0;

                // Group level 2: TenNhom
                var itemGroups = cashierGroup.GroupBy(x => x.TenNhom).OrderBy(g => g.Key).ToList();

                foreach (var itemGroup in itemGroups)
                {
                    // Level 2 Header: Nhóm hàng: CÁC MÓN CÁ
                    var nhomHeaderBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(250, 252, 255)),
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Padding = new Thickness(6, 2, 6, 2)
                    };
                    var nhomHeaderTb = new TextBlock
                    {
                        Text = $"Nhóm hàng: {itemGroup.Key}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 11,
                        Foreground = Brushes.Black
                    };
                    nhomHeaderBorder.Child = nhomHeaderTb;
                    StkDataRows.Children.Add(nhomHeaderBorder);

                    int sttInGroup = 1;

                    foreach (var item in itemGroup)
                    {
                        var rowGrid = new Grid();
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });

                        // STT
                        rowGrid.Children.Add(CreateCell(sttInGroup.ToString(), 0, HorizontalAlignment.Center));
                        sttInGroup++;

                        // Tên hàng
                        rowGrid.Children.Add(CreateCell(item.TenHang, 1, HorizontalAlignment.Left));

                        // ĐVT
                        rowGrid.Children.Add(CreateCell(item.Dvt, 2, HorizontalAlignment.Center));

                        // Số lượng
                        string slStr = (item.SoLuong % 1 == 0) ? item.SoLuong.ToString("N0") : item.SoLuong.ToString("N2");
                        rowGrid.Children.Add(CreateCell(slStr, 3, HorizontalAlignment.Right));

                        // Đơn giá
                        rowGrid.Children.Add(CreateCell(item.DonGia == 0 ? "0" : item.DonGia.ToString("N0"), 4, HorizontalAlignment.Right));

                        // Giảm giá %
                        rowGrid.Children.Add(CreateCell(item.GiamGiaPhanTram == 0 ? "0" : item.GiamGiaPhanTram.ToString("N0"), 5, HorizontalAlignment.Right));

                        // Thành tiền
                        rowGrid.Children.Add(CreateCell(item.ThanhTien == 0 ? "0" : item.ThanhTien.ToString("N0"), 6, HorizontalAlignment.Right, isLast: true));

                        StkDataRows.Children.Add(rowGrid);

                        cashierSoLuong += item.SoLuong;
                        cashierThanhTien += item.ThanhTien;
                    }
                }

                // Cashier Subtotal Row
                var subtotalGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });

                var cellLabelBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4) };
                Grid.SetColumnSpan(cellLabelBorder, 3);
                cellLabelBorder.Child = new TextBlock { Text = "Tổng cộng", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 11 };
                subtotalGrid.Children.Add(cellLabelBorder);

                string cashierSlStr = (cashierSoLuong % 1 == 0) ? cashierSoLuong.ToString("N0") : cashierSoLuong.ToString("N2");
                subtotalGrid.Children.Add(CreateCell(cashierSlStr, 3, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateCell("", 4, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateCell("", 5, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateCell(cashierThanhTien == 0 ? "0" : cashierThanhTien.ToString("N0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true));

                StkDataRows.Children.Add(subtotalGrid);

                grandSoLuong += cashierSoLuong;
                grandThanhTien += cashierThanhTien;
            }

            // Grand Total Row (TỔNG CỘNG)
            var grandGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(235, 235, 235)) };
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });

            var grandLabelBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4) };
            Grid.SetColumnSpan(grandLabelBorder, 3);
            grandLabelBorder.Child = new TextBlock { Text = "TỔNG CỘNG", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 11 };
            grandGrid.Children.Add(grandLabelBorder);

            string grandSlStr = (grandSoLuong % 1 == 0) ? grandSoLuong.ToString("N0") : grandSoLuong.ToString("N2");
            grandGrid.Children.Add(CreateCell(grandSlStr, 3, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell("", 4, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell("", 5, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell(grandThanhTien == 0 ? "0" : grandThanhTien.ToString("N0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true, isGrandTotal: true));

            StkDataRows.Children.Add(grandGrid);
        }

        private Border CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false, bool isLast = false, bool isGrandTotal = false)
        {
            var border = new Border
            {
                BorderBrush = isGrandTotal ? Brushes.Black : Brushes.Gray,
                BorderThickness = new Thickness(0, 0, isLast ? 0 : 1, 1),
                Padding = new Thickness(4)
            };
            Grid.SetColumn(border, col);

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

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderTable();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang mở chế độ xem trước trang in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "Tong Hop Mat Hang Ban Theo Thu Ngan");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"TongHopMatHangBanTheoThuNgan_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
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
                    sb.AppendLine("Thu ngân,Nhóm hàng,STT,Tên hàng,ĐVT,Số lượng,Đơn giá,Giảm giá %,Thành tiền");

                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{item.ThuNgan}\",\"{item.TenNhom}\",\"{item.STT}\",\"{item.TenHang}\",\"{item.Dvt}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.GiamGiaPhanTram}\",\"{item.ThanhTien}\"");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất báo cáo ra CSV/Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
