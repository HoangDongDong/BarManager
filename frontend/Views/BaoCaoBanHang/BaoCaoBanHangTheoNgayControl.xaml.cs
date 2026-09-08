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
    public class ComboLookupItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public partial class BaoCaoBanHangTheoNgayControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<TongHopBanHangTheoNgayItem> _rawItems = new List<TongHopBanHangTheoNgayItem>();

        public BaoCaoBanHangTheoNgayControl(string reportType = "TỔNG HỢP BÁN HÀNG THEO NGÀY")
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "TỔNG HỢP BÁN HÀNG THEO NGÀY" : reportType.Trim().ToUpper();

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

            // Standard initial date range: 30 days ago to today
            var now = DateTime.Now;
            DpTuNgay.SelectedDate = now.AddDays(-30);
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
                // 1. Kho xuất
                var listKho = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };
                try
                {
                    var khoItems = await LocalKhoHangService.GetKhoHangTreeAsync(false);
                    foreach (var k in khoItems)
                    {
                        if (k.IsWarehouse)
                        {
                            listKho.Add(new ComboLookupItem { Id = k.Id ?? "", Name = k.Name ?? "", Icon = "📦" });
                        }
                    }
                }
                catch { }
                CboKhoXuat.ItemsSource = listKho;
                CboKhoXuat.SelectedIndex = 0;

                // 2. Nhân viên xuất
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
                CboNhanVienXuat.ItemsSource = listNv;
                CboNhanVienXuat.SelectedIndex = 0;

                // 3. Khách hàng
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

                string khoText = (CboKhoXuat.SelectedItem as ComboLookupItem)?.Name;
                if (string.IsNullOrWhiteSpace(khoText)) khoText = "Tất cả";
                string nvText = (CboNhanVienXuat.SelectedItem as ComboLookupItem)?.Name;
                if (string.IsNullOrWhiteSpace(nvText)) nvText = "Tất cả";
                string khText = (CboKhachHang.SelectedItem as ComboLookupItem)?.Name;
                if (string.IsNullOrWhiteSpace(khText)) khText = "Tất cả";
                TxtFilterSummary.Text = $"Kho xuất: {khoText} | NV xuất: {nvText} | Khách hàng: {khText}";

                string khoId = (CboKhoXuat.SelectedItem as ComboLookupItem)?.Id;
                string nhanVienId = (CboNhanVienXuat.SelectedItem as ComboLookupItem)?.Id;
                string khachHangId = (CboKhachHang.SelectedItem as ComboLookupItem)?.Id;

                _rawItems = await _hoaDonService.GetTongHopBanHangTheoNgayAsync(tuNgay, denNgay, khoId, nhanVienId, khachHangId);
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
                (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                (x.TienHang.ToString().Contains(search)) ||
                (x.TongCong.ToString().Contains(search))
            ).OrderBy(x => x.Ngay).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns: STT (50), Ngày (140), Tiền hàng (150), Giảm giá (130), Tổng cộng (150)
            stackTable.Children.Add(CreateDataRow("STT", "Ngày", "Tiền hàng", "Giảm giá", "Tổng cộng", isHeader: true, isSummary: false));

            decimal totalTienHang = 0;
            decimal totalGiamGia = 0;
            decimal totalTongCong = 0;

            int stt = 1;
            foreach (var item in filtered)
            {
                totalTienHang += item.TienHang;
                totalGiamGia += item.GiamGia;
                totalTongCong += item.TongCong;

                stackTable.Children.Add(CreateDataRow(
                    stt: (stt++).ToString(),
                    ngay: item.NgayDisplay,
                    tienHang: item.TienHang.ToString("#,##0"),
                    giamGia: item.GiamGia.ToString("#,##0"),
                    tongCong: item.TongCong.ToString("#,##0"),
                    isHeader: false,
                    isSummary: false
                ));
            }

            // Summary Row: TỔNG CỘNG
            stackTable.Children.Add(CreateDataRow(
                stt: "",
                ngay: "",
                tienHang: totalTienHang.ToString("#,##0"),
                giamGia: totalGiamGia.ToString("#,##0"),
                tongCong: totalTongCong.ToString("#,##0"),
                isHeader: false,
                isSummary: true
            ));

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateDataRow(string stt, string ngay, string tienHang, string giamGia, string tongCong, bool isHeader = false, bool isSummary = false)
        {
            var grid = new Grid { MinHeight = isHeader ? 26 : 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            if (isSummary)
            {
                // Cell 0 & 1 merged for TỔNG CỘNG text
                var summaryLabelBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(5, 4, 8, 4),
                    Background = Brushes.White
                };
                Grid.SetColumn(summaryLabelBorder, 0);
                Grid.SetColumnSpan(summaryLabelBorder, 2);
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
                AddCell(grid, 2, tienHang, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 3, giamGia, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 4, tongCong, HorizontalAlignment.Right, isBold: true);
            }
            else
            {
                Brush bg = isHeader ? (Brush)new BrushConverter().ConvertFromString("#f0f0f0") : Brushes.White;
                AddCell(grid, 0, stt, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 1, ngay, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 2, tienHang, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 3, giamGia, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 4, tongCong, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
            }

            return grid;
        }

        private void AddCell(Grid grid, int col, string text, HorizontalAlignment align, bool isBold = false, Brush bg = null)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5, 4, 5, 4),
                Background = bg ?? Brushes.White
            };
            Grid.SetColumn(border, col);

            var txt = new TextBlock
            {
                Text = text,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11.5,
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
                    printDlg.PrintVisual(A4PageBorder, "In Tổng Hợp Bán Hàng Theo Ngày");
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
                    FileName = $"TongHopBanHangTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("STT,Ngày,Tiền hàng,Giảm giá,Tổng cộng");

                    string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
                    var filtered = _rawItems.Where(x =>
                        string.IsNullOrEmpty(search) ||
                        (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                        (x.TienHang.ToString().Contains(search)) ||
                        (x.TongCong.ToString().Contains(search))
                    ).OrderBy(x => x.Ngay).ToList();

                    int stt = 1;
                    decimal sumTienHang = 0, sumGiamGia = 0, sumTongCong = 0;
                    foreach (var item in filtered)
                    {
                        sumTienHang += item.TienHang;
                        sumGiamGia += item.GiamGia;
                        sumTongCong += item.TongCong;

                        sb.AppendLine($"\"{stt++}\",\"{item.NgayDisplay}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
                    }

                    sb.AppendLine($"\"TỔNG CỘNG\",\"\",\"{sumTienHang}\",\"{sumGiamGia}\",\"{sumTongCong}\"");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất CSV: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
