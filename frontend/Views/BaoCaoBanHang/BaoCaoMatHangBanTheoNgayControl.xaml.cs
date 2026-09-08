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
    public partial class BaoCaoMatHangBanTheoNgayControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<TongHopMatHangBanItem> _rawItems = new List<TongHopMatHangBanItem>();

        public BaoCaoMatHangBanTheoNgayControl(string reportType = "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY")
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY" : reportType.Trim().ToUpper();

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

            // Initial date: today
            var now = DateTime.Now;
            DpTuNgay.SelectedDate = now;
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
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                DateTime tuNgay = DpTuNgay.SelectedDate ?? DateTime.Today;
                DateTime denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;

                if (tuNgay.Date == denNgay.Date)
                {
                    TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
                }
                else
                {
                    TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
                }

                string khoText = (CboKhoXuat.SelectedItem as ComboLookupItem)?.Name;
                if (string.IsNullOrWhiteSpace(khoText)) khoText = "Tất cả";
                string nvText = (CboNhanVienXuat.SelectedItem as ComboLookupItem)?.Name;
                if (string.IsNullOrWhiteSpace(nvText)) nvText = "Tất cả";
                TxtFilterSummary.Text = $"Kho xuất: {khoText} | NV xuất: {nvText}";

                string khoId = (CboKhoXuat.SelectedItem as ComboLookupItem)?.Id;
                string nhanVienId = (CboNhanVienXuat.SelectedItem as ComboLookupItem)?.Id;

                _rawItems = await _hoaDonService.GetTongHopMatHangBanTheoNgayAsync(tuNgay, denNgay, khoId, nhanVienId);
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
                (x.TenNhom?.ToLower().Contains(search) == true) ||
                (x.TenHang?.ToLower().Contains(search) == true) ||
                (x.Dvt?.ToLower().Contains(search) == true)
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns: STT(35), Tên hàng(220), ĐVT(55), Số lượng(65), Đơn giá(90), Giảm giá %(75), Thành tiền(110)
            stackTable.Children.Add(CreateHeaderRow());

            var groups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.TenNhom) ? "KHÁC" : x.TenNhom).OrderBy(g => g.Key).ToList();
            
            decimal grandTotalThanhTien = 0;

            foreach (var group in groups)
            {
                // Group Header Row: Nhóm hàng: <TEN_NHOM>
                stackTable.Children.Add(CreateGroupHeaderRow($"Nhóm hàng: {group.Key.ToUpper()}"));

                int stt = 1;
                decimal groupThanhTien = 0;

                foreach (var item in group.OrderBy(x => x.TenHang))
                {
                    groupThanhTien += item.ThanhTien;
                    grandTotalThanhTien += item.ThanhTien;

                    stackTable.Children.Add(CreateDataRow(
                        stt: (stt++).ToString(),
                        tenHang: item.TenHang,
                        dvt: item.Dvt,
                        soLuong: item.SoLuong.ToString("#,##0.##"),
                        donGia: item.DonGia.ToString("#,##0"),
                        giamGiaPt: item.GiamGiaPhanTram.ToString("#,##0"),
                        thanhTien: item.ThanhTien.ToString("#,##0")
                    ));
                }

                // Group Subtotal Row: Tổng cộng
                stackTable.Children.Add(CreateGroupSubtotalRow("Tổng cộng", groupThanhTien.ToString("#,##0")));
            }

            // Grand Total Row at bottom if multiple items/groups
            if (filtered.Count > 0)
            {
                stackTable.Children.Add(CreateGrandTotalRow("TỔNG CỘNG", grandTotalThanhTien.ToString("#,##0")));
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateHeaderRow()
        {
            var grid = new Grid { MinHeight = 26 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

            Brush bg = (Brush)new BrushConverter().ConvertFromString("#f0f0f0");

            AddCellToGrid(grid, 0, "ST", HorizontalAlignment.Center, isBold: true, bg: bg);
            AddCellToGrid(grid, 1, "Tên hàng", HorizontalAlignment.Center, isBold: true, bg: bg);
            AddCellToGrid(grid, 2, "ĐVT", HorizontalAlignment.Center, isBold: true, bg: bg);
            AddCellToGrid(grid, 3, "Số lượng", HorizontalAlignment.Center, isBold: true, bg: bg);
            AddCellToGrid(grid, 4, "Đơn giá", HorizontalAlignment.Center, isBold: true, bg: bg);
            AddCellToGrid(grid, 5, "Giảm giá %", HorizontalAlignment.Center, isBold: true, bg: bg);
            AddCellToGrid(grid, 6, "Thành tiền", HorizontalAlignment.Center, isBold: true, bg: bg);

            return grid;
        }

        private UIElement CreateGroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 24 };
            for (int i = 0; i < 7; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition 
                { 
                    Width = i switch 
                    { 
                        0 => new GridLength(35), 
                        1 => new GridLength(220), 
                        2 => new GridLength(55), 
                        3 => new GridLength(65), 
                        4 => new GridLength(90), 
                        5 => new GridLength(75), 
                        6 => new GridLength(110), 
                        _ => new GridLength(100) 
                    } 
                });
            }

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5, 4, 5, 4),
                Background = Brushes.White
            };
            Grid.SetColumn(border, 0);
            Grid.SetColumnSpan(border, 7);

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

        private UIElement CreateDataRow(string stt, string tenHang, string dvt, string soLuong, string donGia, string giamGiaPt, string thanhTien)
        {
            var grid = new Grid { MinHeight = 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

            AddCellToGrid(grid, 0, stt, HorizontalAlignment.Center);
            AddCellToGrid(grid, 1, tenHang, HorizontalAlignment.Left);
            AddCellToGrid(grid, 2, dvt, HorizontalAlignment.Center);
            AddCellToGrid(grid, 3, soLuong, HorizontalAlignment.Right);
            AddCellToGrid(grid, 4, donGia, HorizontalAlignment.Right);
            AddCellToGrid(grid, 5, giamGiaPt, HorizontalAlignment.Right);
            AddCellToGrid(grid, 6, thanhTien, HorizontalAlignment.Right);

            return grid;
        }

        private UIElement CreateGroupSubtotalRow(string labelText, string groupTotal)
        {
            var grid = new Grid { MinHeight = 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

            // Label spanning columns 0 to 5
            var labelBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5, 4, 8, 4),
                Background = Brushes.White
            };
            Grid.SetColumn(labelBorder, 0);
            Grid.SetColumnSpan(labelBorder, 6);
            var txtLabel = new TextBlock
            {
                Text = labelText,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11.5
            };
            labelBorder.Child = txtLabel;
            grid.Children.Add(labelBorder);

            // Group Total Value
            AddCellToGrid(grid, 6, groupTotal, HorizontalAlignment.Right, isBold: true);

            return grid;
        }

        private UIElement CreateGrandTotalRow(string labelText, string grandTotal)
        {
            var grid = new Grid { MinHeight = 24 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

            var labelBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5, 4, 8, 4),
                Background = Brushes.White
            };
            Grid.SetColumn(labelBorder, 0);
            Grid.SetColumnSpan(labelBorder, 6);
            var txtLabel = new TextBlock
            {
                Text = labelText,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11.5
            };
            labelBorder.Child = txtLabel;
            grid.Children.Add(labelBorder);

            AddCellToGrid(grid, 6, grandTotal, HorizontalAlignment.Right, isBold: true);

            return grid;
        }

        private void AddCellToGrid(Grid grid, int col, string text, HorizontalAlignment align, bool isBold = false, Brush bg = null)
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
                    printDlg.PrintVisual(A4PageBorder, "In Tổng Hợp Mặt Hàng Bán Theo Ngày");
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
                    FileName = $"TongHopMatHangBanTheoNgay_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("Nhóm hàng,STT,Tên hàng,ĐVT,Số lượng,Đơn giá,Giảm giá %,Thành tiền");

                    string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
                    var filtered = _rawItems.Where(x =>
                        string.IsNullOrEmpty(search) ||
                        (x.TenNhom?.ToLower().Contains(search) == true) ||
                        (x.TenHang?.ToLower().Contains(search) == true) ||
                        (x.Dvt?.ToLower().Contains(search) == true)
                    ).ToList();

                    var groups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.TenNhom) ? "KHÁC" : x.TenNhom).OrderBy(g => g.Key).ToList();

                    decimal grandTotal = 0;
                    foreach (var g in groups)
                    {
                        sb.AppendLine($"\"Nhóm hàng: {g.Key}\",,,,,,,");
                        int stt = 1;
                        decimal groupTotal = 0;
                        foreach (var item in g.OrderBy(x => x.TenHang))
                        {
                            groupTotal += item.ThanhTien;
                            grandTotal += item.ThanhTien;
                            sb.AppendLine($"\"\",\"{stt++}\",\"{item.TenHang}\",\"{item.Dvt}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.GiamGiaPhanTram}\",\"{item.ThanhTien}\"");
                        }
                        sb.AppendLine($"\"Tổng cộng\",,,,,,\"{groupTotal}\"");
                    }
                    sb.AppendLine($"\"TỔNG CỘNG\",,,,,,\"{grandTotal}\"");

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
