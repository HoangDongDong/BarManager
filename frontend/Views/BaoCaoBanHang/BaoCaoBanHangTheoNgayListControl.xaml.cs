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
    public partial class BaoCaoBanHangTheoNgayListControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<BaoCaoBanHangTheoNgayOrderItem> _rawItems = new List<BaoCaoBanHangTheoNgayOrderItem>();

        public BaoCaoBanHangTheoNgayListControl(string reportType = "BÁO CÁO BÁN HÀNG THEO NGÀY")
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "BÁO CÁO BÁN HÀNG THEO NGÀY" : reportType.Trim().ToUpper();

            TxtReportTitle.Text = _reportType;

            // Short-cut keys: F5 (Tải dữ liệu), F3 (Lọc/Tìm kiếm), F12 (Excel), Ctrl+P (In), Ctrl+Shift+P (Xem)
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F5)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
                else if (e.Key == Key.F3)
                {
                    TxtSearch.Focus();
                    TxtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.F12)
                {
                    ExportToCsv();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
                {
                    PrintReport();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
            };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;

            var now = DateTime.Now;
            DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
            DpDenNgay.SelectedDate = now;

            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();

            _isLoaded = true;
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
                // 1. Nhân viên
                var listNv = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };
                try
                {
                    var nvItems = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                    foreach (var nv in nvItems)
                    {
                        listNv.Add(new ComboLookupItem { Id = nv.Id ?? "", Name = nv.Name ?? "", Icon = "👤" });
                    }
                }
                catch { }
                CboNhanVien.ItemsSource = listNv;
                CboNhanVien.SelectedIndex = 0;

                // 2. Khách hàng
                var listKh = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };
                try
                {
                    var khItems = await _hoaDonService.GetKhachHangLookupAsync();
                    foreach (var kh in khItems)
                    {
                        listKh.Add(new ComboLookupItem { Id = kh.Id ?? "", Name = kh.Name ?? "", Icon = "👤" });
                    }
                }
                catch { }
                CboKhachHang.ItemsSource = listKh;
                CboKhachHang.SelectedIndex = 0;

                // 3. Cửa hàng
                var listCh = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };
                CboCuaHang.ItemsSource = listCh;
                CboCuaHang.SelectedIndex = 0;
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                DateTime tuNgay = DpTuNgay.SelectedDate ?? DateTime.Today.AddDays(-30);
                DateTime denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;

                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

                string nvText = (CboNhanVien.SelectedItem as ComboLookupItem)?.Name;
                
                string khText = (CboKhachHang.SelectedItem as ComboLookupItem)?.Name;
                
                string chText = (CboCuaHang.SelectedItem as ComboLookupItem)?.Name;
                
                var parts = new List<string>();
            if (Utilities.IsSpecificFilter(nvText)) parts.Add($"Nhân viên: {nvText}");
            if (Utilities.IsSpecificFilter(khText)) parts.Add($"Khách hàng: {khText}");
            if (Utilities.IsSpecificFilter(chText)) parts.Add($"Cửa hàng: {chText}");
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

                string nhanVienId = (CboNhanVien.SelectedItem as ComboLookupItem)?.Id;
                string khachHangId = (CboKhachHang.SelectedItem as ComboLookupItem)?.Id;
                string cuaHangId = (CboCuaHang.SelectedItem as ComboLookupItem)?.Id;

                _rawItems = await _hoaDonService.GetBaoCaoBanHangTheoNgayListAsync(tuNgay, denNgay, nhanVienId, khachHangId, cuaHangId);
                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                (x.ThuNgan?.ToLower().Contains(search) == true) ||
                (x.NhanVienBan?.ToLower().Contains(search) == true)
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns (10 Columns): STT(35), Số phiếu(85), Nhân viên bán(100), Tiền hàng(85), Giảm giá(65), Tổng cộng(85), Tiền mặt(85), Chuyển khoản(85), Thẻ(65), Thẻ tt(65)
            stackTable.Children.Add(CreateDataRow("STT", "Số phiếu", "Nhân viên bán", "Tiền hàng", "Giảm giá", "Tổng cộng", "Tiền mặt", "Chuyển khoản", "Thẻ", "Thẻ tt", isHeader: true, isSummary: false));

            var dateGroups = filtered.GroupBy(x => x.NgayDisplay).OrderBy(g => g.Key).ToList();

            decimal sumTienHang = 0;
            decimal sumGiamGia = 0;
            decimal sumTongCong = 0;
            decimal sumTienMat = 0;
            decimal sumChuyenKhoan = 0;
            decimal sumThe = 0;
            decimal sumTheTt = 0;

            foreach (var dateGroup in dateGroups)
            {
                // Level 1 Group Header: Ngày: dd/MM/yyyy
                stackTable.Children.Add(CreateGroupHeaderRow($"Ngày: {dateGroup.Key}"));

                var cashierGroups = dateGroup.GroupBy(x => string.IsNullOrWhiteSpace(x.ThuNgan) ? "Administrator" : x.ThuNgan).OrderBy(g => g.Key).ToList();

                foreach (var cashierGroup in cashierGroups)
                {
                    // Level 2 Group Header: Thu ngân: <ThuNgan>
                    stackTable.Children.Add(CreateGroupHeaderRow($"Thu ngân: {cashierGroup.Key}"));

                    int stt = 1;
                    foreach (var item in cashierGroup.OrderBy(x => x.SoPhieu))
                    {
                        sumTienHang += item.TienHang;
                        sumGiamGia += item.GiamGia;
                        sumTongCong += item.TongCong;
                        sumTienMat += item.TienMat;
                        sumChuyenKhoan += item.ChuyenKhoan;
                        sumThe += item.The;
                        sumTheTt += item.TheTt;

                        stackTable.Children.Add(CreateDataRow(
                            stt: (stt++).ToString(),
                            soPhieu: item.SoPhieu,
                            nhanVienBan: item.NhanVienBan,
                            tienHang: item.TienHang.ToString("#,##0"),
                            giamGia: item.GiamGia.ToString("#,##0"),
                            tongCong: item.TongCong.ToString("#,##0"),
                            tienMat: item.TienMat.ToString("#,##0"),
                            chuyenKhoan: item.ChuyenKhoan.ToString("#,##0"),
                            the: item.The.ToString("#,##0"),
                            theTt: item.TheTt.ToString("#,##0"),
                            isHeader: false,
                            isSummary: false
                        ));
                    }
                }
            }

            // Summary Row: TỔNG CỘNG
            if (filtered.Count > 0)
            {
                stackTable.Children.Add(CreateDataRow(
                    stt: "",
                    soPhieu: "",
                    nhanVienBan: "",
                    tienHang: sumTienHang.ToString("#,##0"),
                    giamGia: sumGiamGia.ToString("#,##0"),
                    tongCong: sumTongCong.ToString("#,##0"),
                    tienMat: sumTienMat.ToString("#,##0"),
                    chuyenKhoan: sumChuyenKhoan.ToString("#,##0"),
                    the: sumThe.ToString("#,##0"),
                    theTt: sumTheTt.ToString("#,##0"),
                    isHeader: false,
                    isSummary: true
                ));
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateGroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5, 4, 5, 4),
                Background = Brushes.White
            };
            Grid.SetColumn(border, 0);
            Grid.SetColumnSpan(border, 10);

            var txt = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11.5
            };
            border.Child = txt;
            grid.Children.Add(border);

            return grid;
        }

        private UIElement CreateDataRow(string stt, string soPhieu, string nhanVienBan, string tienHang, string giamGia, string tongCong, string tienMat, string chuyenKhoan, string the, string theTt, bool isHeader = false, bool isSummary = false)
        {
            var grid = new Grid { MinHeight = isHeader ? 26 : 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });

            if (isSummary)
            {
                // Cell 0 to 2 merged for TỔNG CỘNG text
                var summaryLabelBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(5, 4, 8, 4),
                    Background = Brushes.White
                };
                Grid.SetColumn(summaryLabelBorder, 0);
                Grid.SetColumnSpan(summaryLabelBorder, 3);
                var txtSummary = new TextBlock
                {
                    Text = "TỔNG CỘNG",
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 11.5
                };
                summaryLabelBorder.Child = txtSummary;
                grid.Children.Add(summaryLabelBorder);

                // Summary Values
                AddCell(grid, 3, tienHang, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 4, giamGia, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 5, tongCong, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 6, tienMat, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 7, chuyenKhoan, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 8, the, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 9, theTt, HorizontalAlignment.Right, isBold: true);
            }
            else
            {
                Brush bg = isHeader ? (Brush)new BrushConverter().ConvertFromString("#f0f0f0") : Brushes.White;
                AddCell(grid, 0, stt, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 1, soPhieu, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 2, nhanVienBan, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, isBold: isHeader, bg: bg);
                AddCell(grid, 3, tienHang, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 4, giamGia, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 5, tongCong, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 6, tienMat, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 7, chuyenKhoan, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 8, the, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 9, theTt, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
            }

            return grid;
        }

        private void AddCell(Grid grid, int col, string text, HorizontalAlignment align, bool isBold = false, Brush bg = null)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 4, 4, 4),
                Background = bg ?? Brushes.White
            };
            Grid.SetColumn(border, col);

            var txt = new TextBlock
            {
                Text = text,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap
            };
            border.Child = txt;
            grid.Children.Add(border);
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            _ = LoadDataAsync();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderReportTable();
        }

        private void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void PrintReport()
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(A4PageBorder, "In Báo Cáo Bán Hàng Theo Ngày");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"BaoCaoBanHangTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("STT,Số phiếu,Nhân viên bán,Tiền hàng,Giảm giá,Tổng cộng,Tiền mặt,Chuyển khoản,Thẻ,Thẻ tt");

                    string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
                    var filtered = _rawItems.Where(x =>
                        string.IsNullOrEmpty(search) ||
                        (x.SoPhieu?.ToLower().Contains(search) == true) ||
                        (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                        (x.ThuNgan?.ToLower().Contains(search) == true) ||
                        (x.NhanVienBan?.ToLower().Contains(search) == true)
                    ).ToList();

                    var dateGroups = filtered.GroupBy(x => x.NgayDisplay).OrderBy(g => g.Key).ToList();

                    decimal sumTienHang = 0, sumGiamGia = 0, sumTongCong = 0, sumTienMat = 0, sumChuyenKhoan = 0, sumThe = 0, sumTheTt = 0;
                    foreach (var dateGroup in dateGroups)
                    {
                        sb.AppendLine($"\"Ngày: {dateGroup.Key}\",,,,,,,,,");
                        var cashierGroups = dateGroup.GroupBy(x => string.IsNullOrWhiteSpace(x.ThuNgan) ? "Administrator" : x.ThuNgan).OrderBy(g => g.Key).ToList();
                        foreach (var cashierGroup in cashierGroups)
                        {
                            sb.AppendLine($"\"Thu ngân: {cashierGroup.Key}\",,,,,,,,,");
                            int stt = 1;
                            foreach (var item in cashierGroup.OrderBy(x => x.SoPhieu))
                            {
                                sumTienHang += item.TienHang;
                                sumGiamGia += item.GiamGia;
                                sumTongCong += item.TongCong;
                                sumTienMat += item.TienMat;
                                sumChuyenKhoan += item.ChuyenKhoan;
                                sumThe += item.The;
                                sumTheTt += item.TheTt;

                                sb.AppendLine($"\"{stt++}\",\"{item.SoPhieu}\",\"{item.NhanVienBan}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\",\"{item.TienMat}\",\"{item.ChuyenKhoan}\",\"{item.The}\",\"{item.TheTt}\"");
                            }
                        }
                    }

                    sb.AppendLine($"\"TỔNG CỘNG\",\"\",\"\",\"{sumTienHang}\",\"{sumGiamGia}\",\"{sumTongCong}\",\"{sumTienMat}\",\"{sumChuyenKhoan}\",\"{sumThe}\",\"{sumTheTt}\"");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất CSV: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Chuc nang thiet ke mau bao cao cho phep ban tuy chinh:\n" +
                "  - Bo cuc va dinh dang bao cao\n" +
                "  - Font chu, co chu, mau sac\n" +
                "  - Logo va thong tin dau trang\n" +
                "  - Them/bo cac cot hien thi\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Thiết kế mẫu bao cao",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnThamSoTuyChinh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tham số tùy chỉnh báo cáo cho phep ban:\n" +
                "  - Chon cac cot hien thi trong bao cao\n" +
                "  - Thiet lap tieu chi nhom du lieu\n" +
                "  - Cau hinh cac dieu kien loc nang cao\n" +
                "  - Tuy chinh dinh dang so va ngay thang\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Tham số tuy chinh",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnXemDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (InlineDataBorder.Visibility == System.Windows.Visibility.Visible)
            {
                InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            // Use reflection to find the largest data list in this control
            var fields = this.GetType().GetFields(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object? dataSource = null;
            int maxCount = 0;
            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                if (val is System.Collections.IList list && list.Count > maxCount)
                {
                    maxCount = list.Count;
                    dataSource = val;
                }
            }

            if (dataSource != null)
                InlineDataGrid.ItemsSource = (System.Collections.IEnumerable)dataSource;
            else
                InlineDataGrid.ItemsSource = new[] { new { ThongBao = "Không có dữ liệu. Hãy tải dữ liệu trước (F5) roi mo lai." } };

            TxtSoBanGhi.Text = $"Tổng số: {maxCount} bản ghi";
            InlineDataBorder.Visibility = System.Windows.Visibility.Visible;

            // Scroll to bottom so user can see the panel
            var scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(this);
            scrollViewer?.ScrollToEnd();
        }

        private void BtnDongDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
