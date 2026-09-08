using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using static QuanLyBar.Client.Services.LocalBaoCaoQuanTriService;

namespace QuanLyBar.Client.Views.BaoCaoQuanTri
{
    public partial class BaoCaoTongHopLaiGopMatHangControl : UserControl
    {
        public enum ReportPriceMode
        {
            GiaVon,
            GiaNhap
        }

        private readonly ReportPriceMode _mode;
        private readonly LocalBaoCaoQuanTriService _service = new LocalBaoCaoQuanTriService();
        private bool _isLoaded = false;
        private List<LaiGopGroupViewModel> _allData = new List<LaiGopGroupViewModel>();

        public BaoCaoTongHopLaiGopMatHangControl(ReportPriceMode mode = ReportPriceMode.GiaVon)
        {
            InitializeComponent();
            _mode = mode;

            if (_mode == ReportPriceMode.GiaNhap)
            {
                TxtReportTitle.Text = "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ NHẬP)";
            }
            else
            {
                TxtReportTitle.Text = "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG";
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = new DateTime(today.Year, today.Month, 1);
            DpDenNgay.SelectedDate = today;
            TxtSignDate.Text = $"Ngày {today:dd} tháng {today:MM} năm {today:yyyy}";

            await LoadCompanyInfoAndLogoAsync();
            await LoadNhomHangAsync();
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

        private async Task LoadNhomHangAsync()
        {
            try
            {
                var nhomList = await _service.GetNhomHangListAsync();
                CboNhomHang.ItemsSource = nhomList;
                if (nhomList.Count > 0) CboNhomHang.SelectedIndex = 0;
            }
            catch { }
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = DpTuNgay.SelectedDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;
                string nhomHang = CboNhomHang.SelectedItem as string ?? "[Tất cả]";

                TxtFilterSummary.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";

                _allData = await _service.GetBaoCaoTongHopLaiGopMatHangAsync(tuNgay, denNgay, nhomHang, _mode == ReportPriceMode.GiaNhap);

                RenderTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderTable();
        }

        private void RenderTable()
        {
            PnlReportContent.Children.Clear();

            string filter = TxtSearch.Text?.Trim().ToLower() ?? "";
            string costHeader = _mode == ReportPriceMode.GiaNhap ? "Giá nhập" : "Giá vốn";

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });  // Mã hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) }); // Tên hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });  // ĐVT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });  // Số lượng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });  // Đổi trả
            // Giá bán
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });  // Đơn giá bán
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Thành tiền
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });  // Giảm giá
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Tổng cộng bán
            // Giá vốn / Giá nhập
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });  // Đơn giá vốn
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Giá trị vốn
            // Lợi nhuận
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Lợi nhuận

            int rowIdx = 0;

            // Row 0 & 1: Two-level Header
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Headers spanning 2 rows
            AddHeaderCell(grid, "STT", 0, 0, 2, 1);
            AddHeaderCell(grid, "Mã\nhàng", 0, 1, 2, 1);
            AddHeaderCell(grid, "Tên hàng", 0, 2, 2, 1);
            AddHeaderCell(grid, "ĐVT", 0, 3, 2, 1);
            AddHeaderCell(grid, "Số\nlượng", 0, 4, 2, 1);
            AddHeaderCell(grid, "Đổi\ntrả", 0, 5, 2, 1);

            // Group Header: Giá bán (cols 6-9)
            AddHeaderCell(grid, "Giá bán", 0, 6, 1, 4);
            // Group Header: Giá vốn / Giá nhập (cols 10-11)
            AddHeaderCell(grid, costHeader, 0, 10, 1, 2);
            // Header: Lợi nhuận (col 12)
            AddHeaderCell(grid, "Lợi nhuận", 0, 12, 2, 1);

            // Sub-headers (Row 1)
            AddHeaderCell(grid, "Đơn giá", 1, 6);
            AddHeaderCell(grid, "Thành tiền", 1, 7);
            AddHeaderCell(grid, "Giảm giá", 1, 8);
            AddHeaderCell(grid, "Tổng cộng", 1, 9);
            AddHeaderCell(grid, "Đơn giá", 1, 10);
            AddHeaderCell(grid, "Giá trị vốn", 1, 11);

            rowIdx = 2;

            decimal grandQty = 0;
            decimal grandDoiTra = 0;
            decimal grandThanhTien = 0;
            decimal grandGiamGia = 0;
            decimal grandTongCong = 0;
            decimal grandGiaTriVon = 0;
            decimal grandLoiNhuan = 0;

            foreach (var group in _allData)
            {
                var filteredItems = string.IsNullOrEmpty(filter)
                    ? group.Items
                    : group.Items.Where(x => (x.TenHang?.ToLower().Contains(filter) ?? false) ||
                                             (x.MaHang?.ToLower().Contains(filter) ?? false) ||
                                             (group.GroupName?.ToLower().Contains(filter) ?? false)).ToList();

                if (filteredItems.Count == 0) continue;

                // Group header row
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var groupBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    Padding = new Thickness(4, 3, 4, 3)
                };
                var groupText = new TextBlock
                {
                    Text = $"Nhóm hàng hóa: {group.GroupName}",
                    FontWeight = FontWeights.Bold,
                    FontSize = 11,
                    Foreground = Brushes.Black
                };
                groupBorder.Child = groupText;
                Grid.SetRow(groupBorder, rowIdx);
                Grid.SetColumn(groupBorder, 0);
                Grid.SetColumnSpan(groupBorder, 13);
                grid.Children.Add(groupBorder);
                rowIdx++;

                int itemStt = 1;
                foreach (var it in filteredItems)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    AddDataCell(grid, itemStt.ToString(), rowIdx, 0, TextAlignment.Center);
                    AddDataCell(grid, it.MaHang, rowIdx, 1, TextAlignment.Left);
                    AddDataCell(grid, it.TenHang, rowIdx, 2, TextAlignment.Left);
                    AddDataCell(grid, it.Dvt, rowIdx, 3, TextAlignment.Center);
                    AddDataCell(grid, it.SoLuong.ToString("#,##0"), rowIdx, 4, TextAlignment.Right);
                    AddDataCell(grid, it.DoiTra.ToString("#,##0"), rowIdx, 5, TextAlignment.Right);
                    AddDataCell(grid, it.DonGiaBan > 0 ? it.DonGiaBan.ToString("#,##0") : "0", rowIdx, 6, TextAlignment.Right);
                    AddDataCell(grid, it.ThanhTienBan > 0 ? it.ThanhTienBan.ToString("#,##0") : "0", rowIdx, 7, TextAlignment.Right);
                    AddDataCell(grid, it.GiamGia > 0 ? it.GiamGia.ToString("#,##0") : "0", rowIdx, 8, TextAlignment.Right);
                    AddDataCell(grid, it.TongCongBan > 0 ? it.TongCongBan.ToString("#,##0") : "0", rowIdx, 9, TextAlignment.Right);
                    AddDataCell(grid, it.DonGiaVon > 0 ? it.DonGiaVon.ToString("#,##0") : "0", rowIdx, 10, TextAlignment.Right);
                    AddDataCell(grid, it.GiaTriVon > 0 ? it.GiaTriVon.ToString("#,##0") : "0", rowIdx, 11, TextAlignment.Right);
                    AddDataCell(grid, it.LoiNhuan.ToString("#,##0"), rowIdx, 12, TextAlignment.Right);

                    itemStt++;
                    rowIdx++;
                }

                // Subtotal row for group
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                decimal grpQty = filteredItems.Sum(x => x.SoLuong);
                decimal grpDoiTra = filteredItems.Sum(x => x.DoiTra);
                decimal grpThanhTien = filteredItems.Sum(x => x.ThanhTienBan);
                decimal grpGiamGia = filteredItems.Sum(x => x.GiamGia);
                decimal grpTongCong = filteredItems.Sum(x => x.TongCongBan);
                decimal grpGiaTriVon = filteredItems.Sum(x => x.GiaTriVon);
                decimal grpLoiNhuan = filteredItems.Sum(x => x.LoiNhuan);

                grandQty += grpQty;
                grandDoiTra += grpDoiTra;
                grandThanhTien += grpThanhTien;
                grandGiamGia += grpGiamGia;
                grandTongCong += grpTongCong;
                grandGiaTriVon += grpGiaTriVon;
                grandLoiNhuan += grpLoiNhuan;

                AddSubtotalCell(grid, "Tổng", rowIdx, 0, 1, 4, TextAlignment.Right);
                AddSubtotalCell(grid, grpQty.ToString("#,##0"), rowIdx, 4, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, grpDoiTra.ToString("#,##0"), rowIdx, 5, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, "", rowIdx, 6, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, grpThanhTien.ToString("#,##0"), rowIdx, 7, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, grpGiamGia.ToString("#,##0"), rowIdx, 8, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, grpTongCong.ToString("#,##0"), rowIdx, 9, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, "", rowIdx, 10, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, grpGiaTriVon.ToString("#,##0"), rowIdx, 11, 1, 1, TextAlignment.Right);
                AddSubtotalCell(grid, grpLoiNhuan.ToString("#,##0"), rowIdx, 12, 1, 1, TextAlignment.Right);

                rowIdx++;
            }

            // Grand total row
            if (rowIdx > 2)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                AddGrandTotalCell(grid, "TỔNG CỘNG", rowIdx, 0, 1, 4, TextAlignment.Right);
                AddGrandTotalCell(grid, grandQty.ToString("#,##0"), rowIdx, 4, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, grandDoiTra.ToString("#,##0"), rowIdx, 5, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, "", rowIdx, 6, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, grandThanhTien.ToString("#,##0"), rowIdx, 7, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, grandGiamGia.ToString("#,##0"), rowIdx, 8, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, grandTongCong.ToString("#,##0"), rowIdx, 9, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, "", rowIdx, 10, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, grandGiaTriVon.ToString("#,##0"), rowIdx, 11, 1, 1, TextAlignment.Right);
                AddGrandTotalCell(grid, grandLoiNhuan.ToString("#,##0"), rowIdx, 12, 1, 1, TextAlignment.Right);
            }

            PnlReportContent.Children.Add(grid);
        }

        private void AddHeaderCell(Grid g, string text, int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, row == 0 ? 1 : 0, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(3, 4, 3, 4)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            if (rowSpan > 1) Grid.SetRowSpan(b, rowSpan);
            if (colSpan > 1) Grid.SetColumnSpan(b, colSpan);
            g.Children.Add(b);
        }

        private void AddDataCell(Grid g, string text, int row, int col, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = Brushes.White,
                Padding = new Thickness(4, 3, 4, 3)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 11,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            g.Children.Add(b);
        }

        private void AddSubtotalCell(Grid g, string text, int row, int col, int rowSpan, int colSpan, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Padding = new Thickness(4, 3, 4, 3)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            if (colSpan > 1) Grid.SetColumnSpan(b, colSpan);
            g.Children.Add(b);
        }

        private void AddGrandTotalCell(Grid g, string text, int row, int col, int rowSpan, int colSpan, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(235, 240, 248)),
                Padding = new Thickness(4, 4, 4, 4)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            if (colSpan > 1) Grid.SetColumnSpan(b, colSpan);
            g.Children.Add(b);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"{TxtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine(TxtReportTitle.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    string costCol = _mode == ReportPriceMode.GiaNhap ? "Đơn giá nhập,Giá trị nhập" : "Đơn giá vốn,Giá trị vốn";
                    sb.AppendLine($"STT,Mã hàng,Tên hàng,ĐVT,Số lượng,Đổi trả,Đơn giá bán,Thành tiền bán,Giảm giá,Tổng cộng bán,{costCol},Lợi nhuận");

                    foreach (var g in _allData)
                    {
                        sb.AppendLine($"Nhóm: {g.GroupName}");
                        foreach (var it in g.Items)
                        {
                            sb.AppendLine($"{it.Stt},\"{it.MaHang}\",\"{it.TenHang}\",\"{it.Dvt}\",{it.SoLuong},{it.DoiTra},{it.DonGiaBan},{it.ThanhTienBan},{it.GiamGia},{it.TongCongBan},{it.DonGiaVon},{it.GiaTriVon},{it.LoiNhuan}");
                        }
                        sb.AppendLine($"Tổng {g.GroupName},,,,{g.TongSoLuong},{g.TongDoiTra},,{g.TongThanhTien},{g.TongGiamGia},{g.TongTongCong},,{g.TongGiaTriVon},{g.TongLoiNhuan}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file báo cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
