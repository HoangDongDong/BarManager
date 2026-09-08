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
    public partial class BaoCaoBanHangTheoThuNganControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<BaoCaoBanHangTheoThuNganItem> _allData = new List<BaoCaoBanHangTheoThuNganItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoBanHangTheoThuNganControl(string tabName = "BÁO CÁO BÁN HÀNG THEO THU NGÂN")
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
                var khList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👥" } };
                try
                {
                    var dbKh = await _hoaDonService.GetKhachHangLookupAsync();
                    if (dbKh != null)
                    {
                        khList.AddRange(dbKh.Select(k => new FilterComboItem { Id = k.Id ?? "", Name = k.Name ?? "", Icon = "👥" }));
                    }
                }
                catch { }
                CboKhachHang.ItemsSource = khList;
                CboKhachHang.SelectedIndex = 0;

                // 2. Nhân viên
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

                CboThanhToanBoi.ItemsSource = nvList;
                CboThanhToanBoi.SelectedIndex = 0;

                // 3. Cửa hàng
                var chList = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "🏢" } };
                CboCuaHang.ItemsSource = chList;
                CboCuaHang.SelectedIndex = 0;
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
            string nhanVienXuatId = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Id ?? "";
            string thanhToanBoiId = (CboThanhToanBoi.SelectedItem as FilterComboItem)?.Id ?? "";
            string cuaHangId = (CboCuaHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string khText = (CboKhachHang.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(khText)) khText = "Tất cả";
            string nvText = (CboNhanVienXuat.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(nvText)) nvText = "Tất cả";
            string ttText = (CboThanhToanBoi.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(ttText)) ttText = "Tất cả";
            string chText = (CboCuaHang.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(chText)) chText = "Tất cả";
            TxtFilterSummary.Text = $"Khách hàng: {khText} | NV xuất: {nvText} | Thu ngân: {ttText} | Cửa hàng: {chText}";

            _allData = await _hoaDonService.GetBaoCaoBanHangTheoThuNganAsync(tuNgay, denNgay, khachHangId, nhanVienXuatId, thanhToanBoiId, cuaHangId);

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
                    x.SoPhieu.ToLower().Contains(keyword) ||
                    x.KhachHang.ToLower().Contains(keyword)
                ).ToList();
            }

            if (filtered.Count == 0)
            {
                var emptyRow = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(6) };
                emptyRow.Child = new TextBlock { Text = "Không có dữ liệu báo cáo", FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Gray };
                StkDataRows.Children.Add(emptyRow);
                return;
            }

            // Group by ThuNgan
            var groups = filtered.GroupBy(x => x.ThuNgan).OrderBy(g => g.Key).ToList();

            decimal grandTienHang = 0;
            decimal grandGiamGia = 0;
            decimal grandTongCong = 0;

            foreach (var group in groups)
            {
                // Group Header Row: Thu ngân: Administrator
                var groupHeaderBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 247, 250)),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 3, 6, 3)
                };
                var groupHeaderTb = new TextBlock
                {
                    Text = $"Thu ngân: {group.Key}",
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Foreground = Brushes.Black
                };
                groupHeaderBorder.Child = groupHeaderTb;
                StkDataRows.Children.Add(groupHeaderBorder);

                decimal groupTienHang = 0;
                decimal groupGiamGia = 0;
                decimal groupTongCong = 0;
                int sttInGroup = 1;

                foreach (var item in group)
                {
                    var rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                    // STT
                    rowGrid.Children.Add(CreateCell(sttInGroup.ToString(), 0, HorizontalAlignment.Center));
                    sttInGroup++;

                    // Ngày
                    rowGrid.Children.Add(CreateCell(item.NgayDisplay, 1, HorizontalAlignment.Center));

                    // Số phiếu
                    rowGrid.Children.Add(CreateCell(item.SoPhieu, 2, HorizontalAlignment.Left));

                    // Khách hàng
                    rowGrid.Children.Add(CreateCell(item.KhachHang, 3, HorizontalAlignment.Left));

                    // Tiền hàng
                    rowGrid.Children.Add(CreateCell(item.TienHang == 0 ? "0" : item.TienHang.ToString("N0"), 4, HorizontalAlignment.Right));

                    // Giảm giá
                    rowGrid.Children.Add(CreateCell(item.GiamGia == 0 ? "0" : item.GiamGia.ToString("N0"), 5, HorizontalAlignment.Right));

                    // Tổng cộng
                    rowGrid.Children.Add(CreateCell(item.TongCong == 0 ? "0" : item.TongCong.ToString("N0"), 6, HorizontalAlignment.Right, isLast: true));

                    StkDataRows.Children.Add(rowGrid);

                    groupTienHang += item.TienHang;
                    groupGiamGia += item.GiamGia;
                    groupTongCong += item.TongCong;
                }

                // Group Subtotal Row
                var subtotalGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)) };
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                subtotalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                var cellLabelBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(3) };
                Grid.SetColumnSpan(cellLabelBorder, 4);
                cellLabelBorder.Child = new TextBlock { Text = "Tổng cộng", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 11 };
                subtotalGrid.Children.Add(cellLabelBorder);

                subtotalGrid.Children.Add(CreateCell(groupTienHang == 0 ? "0" : groupTienHang.ToString("N0"), 4, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateCell(groupGiamGia == 0 ? "0" : groupGiamGia.ToString("N0"), 5, HorizontalAlignment.Right, isBold: true));
                subtotalGrid.Children.Add(CreateCell(groupTongCong == 0 ? "0" : groupTongCong.ToString("N0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true));

                StkDataRows.Children.Add(subtotalGrid);

                grandTienHang += groupTienHang;
                grandGiamGia += groupGiamGia;
                grandTongCong += groupTongCong;
            }

            // Grand Total Row (TỔNG CỘNG)
            var grandGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)) };
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            var grandLabelBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(3) };
            Grid.SetColumnSpan(grandLabelBorder, 4);
            grandLabelBorder.Child = new TextBlock { Text = "TỔNG CỘNG", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 11 };
            grandGrid.Children.Add(grandLabelBorder);

            grandGrid.Children.Add(CreateCell(grandTienHang == 0 ? "0" : grandTienHang.ToString("N0"), 4, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell(grandGiamGia == 0 ? "0" : grandGiamGia.ToString("N0"), 5, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell(grandTongCong == 0 ? "0" : grandTongCong.ToString("N0"), 6, HorizontalAlignment.Right, isBold: true, isLast: true, isGrandTotal: true));

            StkDataRows.Children.Add(grandGrid);
        }

        private Border CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false, bool isLast = false, bool isGrandTotal = false)
        {
            var border = new Border
            {
                BorderBrush = isGrandTotal ? Brushes.Black : Brushes.Gray,
                BorderThickness = new Thickness(0, 0, isLast ? 0 : 1, 1),
                Padding = new Thickness(3)
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
                    printDlg.PrintVisual(ReportPaper, "Bao Cao Ban Hang Theo Thu Ngan");
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
                    FileName = $"BaoCaoBanHangTheoThuNgan_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("STT,Thu ngân,Ngày,Số phiếu,Khách hàng,Tiền hàng,Giảm giá,Tổng cộng");

                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{item.STT}\",\"{item.ThuNgan}\",\"{item.NgayDisplay}\",\"{item.SoPhieu}\",\"{item.KhachHang}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
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
