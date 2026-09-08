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
    public partial class BaoCaoTongHopDoanhThuChuaThanhToanControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<TongHopDoanhThuChuaThanhToanItem> _rawItems = new List<TongHopDoanhThuChuaThanhToanItem>();

        public BaoCaoTongHopDoanhThuChuaThanhToanControl(string reportType = "TỔNG HỢP DOANH THU CHƯA THANH TOÁN")
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "TỔNG HỢP DOANH THU CHƯA THANH TOÁN" : reportType.Trim().ToUpper();

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

            await LoadCompanyInfoAndLogoAsync();

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

        private async Task LoadDataAsync()
        {
            try
            {
                _rawItems = await _hoaDonService.GetTongHopDoanhThuChuaThanhToanAsync();
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
                (x.BanPhong?.ToLower().Contains(search) == true) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.TongCong.ToString().Contains(search))
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns (9 Columns): STT(40), Ngày(100), Bàn/Phòng(100), Số phiếu(100), Bắt đầu(65), Kết thúc(65), Tiền hàng(100), Giảm giá(80), Tổng cộng(100)
            stackTable.Children.Add(CreateDataRow("STT", "Ngày", "Bàn/Phòng", "Số phiếu", "Bắt đầu", "Kết thúc", "Tiền hàng", "Giảm giá", "Tổng cộng", isHeader: true, isSummary: false));

            decimal sumTienHang = 0;
            decimal sumGiamGia = 0;
            decimal sumTongCong = 0;

            int stt = 1;
            foreach (var item in filtered)
            {
                sumTienHang += item.TienHang;
                sumGiamGia += item.GiamGia;
                sumTongCong += item.TongCong;

                stackTable.Children.Add(CreateDataRow(
                    stt: (stt++).ToString(),
                    ngay: item.NgayDisplay,
                    banPhong: item.BanPhong,
                    soPhieu: item.SoPhieu,
                    batDau: item.BatDau,
                    ketThuc: item.KetThuc,
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
                banPhong: "",
                soPhieu: "",
                batDau: "",
                ketThuc: "",
                tienHang: sumTienHang.ToString("#,##0"),
                giamGia: sumGiamGia.ToString("#,##0"),
                tongCong: sumTongCong.ToString("#,##0"),
                isHeader: false,
                isSummary: true
            ));

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateDataRow(string stt, string ngay, string banPhong, string soPhieu, string batDau, string ketThuc, string tienHang, string giamGia, string tongCong, bool isHeader = false, bool isSummary = false)
        {
            var grid = new Grid { MinHeight = isHeader ? 26 : 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            if (isSummary)
            {
                // Cell 0 to 5 merged for TỔNG CỘNG text
                var summaryLabelBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(5, 4, 8, 4),
                    Background = Brushes.White
                };
                Grid.SetColumn(summaryLabelBorder, 0);
                Grid.SetColumnSpan(summaryLabelBorder, 6);
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
                AddCell(grid, 6, tienHang, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 7, giamGia, HorizontalAlignment.Right, isBold: true);
                AddCell(grid, 8, tongCong, HorizontalAlignment.Right, isBold: true);
            }
            else
            {
                Brush bg = isHeader ? (Brush)new BrushConverter().ConvertFromString("#f0f0f0") : Brushes.White;
                AddCell(grid, 0, stt, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 1, ngay, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 2, banPhong, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, isBold: isHeader, bg: bg);
                AddCell(grid, 3, soPhieu, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 4, batDau, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 5, ketThuc, HorizontalAlignment.Center, isBold: isHeader, bg: bg);
                AddCell(grid, 6, tienHang, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 7, giamGia, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
                AddCell(grid, 8, tongCong, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, isBold: isHeader, bg: bg);
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
                    printDlg.PrintVisual(A4PageBorder, "In Tổng Hợp Doanh Thu Chưa Thanh Toán");
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
                    FileName = $"TongHopDoanhThuChuaThanhToan_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("STT,Ngày,Bàn/Phòng,Số phiếu,Bắt đầu,Kết thúc,Tiền hàng,Giảm giá,Tổng cộng");

                    string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
                    var filtered = _rawItems.Where(x =>
                        string.IsNullOrEmpty(search) ||
                        (x.NgayDisplay?.ToLower().Contains(search) == true) ||
                        (x.BanPhong?.ToLower().Contains(search) == true) ||
                        (x.SoPhieu?.ToLower().Contains(search) == true) ||
                        (x.TongCong.ToString().Contains(search))
                    ).ToList();

                    int stt = 1;
                    decimal sumTienHang = 0, sumGiamGia = 0, sumTongCong = 0;
                    foreach (var item in filtered)
                    {
                        sumTienHang += item.TienHang;
                        sumGiamGia += item.GiamGia;
                        sumTongCong += item.TongCong;

                        sb.AppendLine($"\"{stt++}\",\"{item.NgayDisplay}\",\"{item.BanPhong}\",\"{item.SoPhieu}\",\"{item.BatDau}\",\"{item.KetThuc}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
                    }

                    sb.AppendLine($"\"TỔNG CỘNG\",\"\",\"\",\"\",\"\",\"\",\"{sumTienHang}\",\"{sumGiamGia}\",\"{sumTongCong}\"");

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
