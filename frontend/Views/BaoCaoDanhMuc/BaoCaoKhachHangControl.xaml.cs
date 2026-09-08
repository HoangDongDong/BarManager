using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoDanhMuc
{
    public class ComboLookupItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public partial class BaoCaoKhachHangControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<KhachHangViewModel> _rawItems = new List<KhachHangViewModel>();

        public BaoCaoKhachHangControl(string reportType = "DANH SÁCH KHÁCH HÀNG THEO NHÓM")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "DANH SÁCH KHÁCH HÀNG THEO NHÓM" : reportType.Trim().ToUpper();

            TxtReportTitle.Text = _reportType;

            // Keyboard Shortcuts: F5 (Refresh), F3 (Search), Ctrl+P (Print), F12 (Excel)
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
            _isLoaded = true;

            if (!_reportType.Contains("NHÂN VIÊN"))
            {
                TxtNhanVienLabel.Visibility = Visibility.Collapsed;
                CboNhanVien.Visibility = Visibility.Collapsed;
            }

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
                // 1. Nhóm khách hàng
                var listNhom = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "🌐" }
                };

                var treeNhom = await LocalKhachHangService.GetNhomKhachHangTreeAsync();
                FlattenTreeNodes(treeNhom, listNhom);

                CboNhomKhachHang.ItemsSource = listNhom;
                CboNhomKhachHang.SelectedIndex = 0;

                // 2. Nhân viên
                if (!_reportType.Contains("SINH NHẬT"))
                {
                    var listNv = new List<ComboLookupItem>
                    {
                        new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "👥" }
                    };

                    var treeNv = await LocalKhachHangService.GetNhanVienTreeAsync();
                    FlattenTreeNodes(treeNv, listNv);

                    CboNhanVien.ItemsSource = listNv;
                    CboNhanVien.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void FlattenTreeNodes(IEnumerable<NhomKhachHangTreeItem> nodes, List<ComboLookupItem> result)
        {
            if (nodes == null) return;
            foreach (var node in nodes)
            {
                if (node.Id != "ALL" && node.Id != "TRASH" && !string.IsNullOrEmpty(node.Name))
                {
                    result.Add(new ComboLookupItem
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Icon = string.IsNullOrEmpty(node.Icon) ? "📁" : node.Icon
                    });
                }
                if (node.Children != null && node.Children.Count > 0)
                {
                    FlattenTreeNodes(node.Children, result);
                }
            }
        }

        private async Task LoadDataAsync()
        {
            if (!_isLoaded) return;

            try
            {
                _rawItems = await LocalKhachHangService.GetKhachHangListAsync();

                UpdateReportSubHeader();
                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReportSubHeader()
        {
            if (_reportType.Contains("SINH NHẬT"))
            {
                TxtNhomDisplay.Text = "";
                TxtNhanVienDisplay.Text = "";
                return;
            }

            string nhomName = (CboNhomKhachHang.SelectedItem as ComboLookupItem)?.Name;
            if (string.IsNullOrEmpty(nhomName)) nhomName = "Tất cả";
            TxtNhomDisplay.Text = $"Nhóm khách hàng: {nhomName}";

            string nhanVienName = (CboNhanVien.SelectedItem as ComboLookupItem)?.Name;
            if (string.IsNullOrEmpty(nhanVienName)) nhanVienName = "Tất cả";
            TxtNhanVienDisplay.Text = $"Nhân viên: {nhanVienName}";
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
            string selNhomId = (CboNhomKhachHang.SelectedItem as ComboLookupItem)?.Id ?? "";
            string selNhanVienId = (CboNhanVien.SelectedItem as ComboLookupItem)?.Id ?? "";
            string selNhanVienName = (CboNhanVien.SelectedItem as ComboLookupItem)?.Name ?? "";

            var filtered = _rawItems.Where(x =>
                (string.IsNullOrEmpty(selNhomId) || x.DnhomkhachhangId == selNhomId) &&
                (string.IsNullOrEmpty(selNhanVienId) || x.TenNhanVien == selNhanVienName || x.TenNhanVien?.Contains(selNhanVienName) == true) &&
                (string.IsNullOrEmpty(search) ||
                 (x.Makhach?.ToLower().Contains(search) == true) ||
                 (x.Name?.ToLower().Contains(search) == true) ||
                 (x.Dienthoai?.ToLower().Contains(search) == true) ||
                 (x.Diachi?.ToLower().Contains(search) == true) ||
                 (x.TenNhomKhachHang?.ToLower().Contains(search) == true) ||
                 (x.TenNhanVien?.ToLower().Contains(search) == true))
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            if (_reportType.Contains("SINH NHẬT"))
            {
                // Columns: STT(35), Khách hàng(150), Mã khách(85), Địa chỉ(Star), Điện thoại(110), Email(140)
                stackTable.Children.Add(CreateRowKhachHangSinhNhat("STT", "Khách hàng", "Mã khách", "Địa chỉ", "Điện thoại", "Email", isHeader: true, isSummary: false));

                int stt = 1;
                foreach (var item in filtered.OrderBy(x => x.Name))
                {
                    stackTable.Children.Add(CreateRowKhachHangSinhNhat(
                        stt: (stt++).ToString(),
                        tenKh: item.Name ?? "",
                        maKh: item.Makhach ?? "",
                        diaChi: item.Diachi ?? "",
                        sdt: item.Dienthoai ?? "",
                        email: item.Email ?? "",
                        isHeader: false,
                        isSummary: false
                    ));
                }
            }
            else if (_reportType.Contains("NHÂN VIÊN"))
            {
                // Columns: STT(35), Mã khách(85), Tên khách hàng(150), Địa chỉ(Star), Điện thoại(110), Email(140)
                stackTable.Children.Add(CreateRowKhachHang("STT", "Mã khách", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email", isHeader: true, isSummary: false));

                var nvGroups = filtered.GroupBy(x => string.IsNullOrEmpty(x.TenNhanVien) ? "" : x.TenNhanVien).OrderBy(g => g.Key).ToList();
                if (nvGroups.Count == 0)
                {
                    stackTable.Children.Add(CreateGroupHeaderRow("Nhân viên:"));
                }
                else
                {
                    foreach (var group in nvGroups)
                    {
                        string gTitle = string.IsNullOrEmpty(group.Key) ? "Nhân viên:" : $"Nhân viên: {group.Key}";
                        stackTable.Children.Add(CreateGroupHeaderRow(gTitle));
                        int stt = 1;
                        foreach (var item in group.OrderBy(x => x.Makhach).ThenBy(x => x.Name))
                        {
                            stackTable.Children.Add(CreateRowKhachHang(
                                stt: (stt++).ToString(),
                                maKh: item.Makhach ?? "",
                                tenKh: item.Name ?? "",
                                diaChi: item.Diachi ?? "",
                                sdt: item.Dienthoai ?? "",
                                email: item.Email ?? "",
                                isHeader: false,
                                isSummary: false
                            ));
                        }
                    }
                }
            }
            else
            {
                // Columns: STT(35), Mã khách(85), Tên khách hàng(150), Địa chỉ(Star), Điện thoại(110), Email(140)
                stackTable.Children.Add(CreateRowKhachHang("STT", "Mã khách", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email", isHeader: true, isSummary: false));

                // Mặc định Báo cáo theo Nhóm khách hàng
                var nhomGroups = filtered.GroupBy(x => string.IsNullOrEmpty(x.TenNhomKhachHang) ? "" : x.TenNhomKhachHang).OrderBy(g => g.Key).ToList();
                if (nhomGroups.Count == 0)
                {
                    stackTable.Children.Add(CreateGroupHeaderRow("Nhóm khách hàng:"));
                }
                else
                {
                    foreach (var group in nhomGroups)
                    {
                        string gTitle = string.IsNullOrEmpty(group.Key) ? "Nhóm khách hàng:" : $"Nhóm khách hàng: {group.Key}";
                        stackTable.Children.Add(CreateGroupHeaderRow(gTitle));
                        int stt = 1;
                        foreach (var item in group.OrderBy(x => x.Makhach).ThenBy(x => x.Name))
                        {
                            stackTable.Children.Add(CreateRowKhachHang(
                                stt: (stt++).ToString(),
                                maKh: item.Makhach ?? "",
                                tenKh: item.Name ?? "",
                                diaChi: item.Diachi ?? "",
                                sdt: item.Dienthoai ?? "",
                                email: item.Email ?? "",
                                isHeader: false,
                                isSummary: false
                            ));
                        }
                    }
                }
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateRowKhachHangSinhNhat(string stt, string tenKh, string maKh, string diaChi, string sdt, string email, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Khách hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Mã khách
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Địa chỉ
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Điện thoại
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) }); // Email

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tenKh, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(maKh, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(diaChi, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(sdt, 4, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(email, 5, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));

            return grid;
        }

        private UIElement CreateGroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = Brushes.White
            };
            Grid.SetColumnSpan(border, 6);

            var tb = new TextBlock
            {
                Text = title,
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            grid.Children.Add(border);
            return grid;
        }

        private UIElement CreateRowKhachHang(string stt, string maKh, string tenKh, string diaChi, string sdt, string email, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Mã khách
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Tên khách hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Địa chỉ
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Điện thoại
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) }); // Email

            if (isSummary)
            {
                var summaryLabelCell = CreateTableCell(diaChi, 0, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                Grid.SetColumnSpan(summaryLabelCell, 6);
                grid.Children.Add(summaryLabelCell);
                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(maKh, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tenKh, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(diaChi, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(sdt, 4, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(email, 5, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight, bool isHeader, bool isSummary)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 2, 4, 2),
                Background = isSummary 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bbfe6")) 
                    : (isHeader ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9")) : Brushes.White)
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text ?? "",
                FontSize = isHeader ? 10.5 : (isSummary ? 10.5 : 10),
                FontWeight = weight,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            return border;
        }

        #region Event Handlers

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            UpdateReportSubHeader();
            RenderReportTable();
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

        private void PrintReport()
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(A4PageBorder, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"{TxtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtNhomDisplay.Text}\" - \"{TxtNhanVienDisplay.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("STT,Mã khách,Tên khách hàng,Địa chỉ,Điện thoại,Email");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        sb.AppendLine($"\"{stt++}\",\"{item.Makhach}\",\"{item.Name?.Replace("\"", "\"\"")}\",\"{item.Diachi?.Replace("\"", "\"\"")}\",\"{item.Dienthoai}\",\"{item.Email?.Replace("\"", "\"\"")}\"");
                    }

                    sb.AppendLine($"\"\",\"\",\"\",\"TỔNG CỘNG ({_rawItems.Count} khách hàng)\",\"\",\"\"");
                    File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
