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
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoDanhMuc
{
    public partial class BaoCaoNhaCungCapControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<NhaCungCapItem> _rawItems = new List<NhaCungCapItem>();

        public BaoCaoNhaCungCapControl(string reportType = "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM" : reportType.Trim().ToUpper();

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

            await LoadCompanyInfoAndLogoAsync();
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
            if (!_isLoaded) return;

            try
            {
                _rawItems = await LocalNhaCungCapService.GetNhaCungCapListAsync("", "", "ALL");
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
                (x.MaNhaCungCap?.ToLower().Contains(search) == true) ||
                (x.Name?.ToLower().Contains(search) == true) ||
                (x.DienThoai?.ToLower().Contains(search) == true) ||
                (x.DiaChi?.ToLower().Contains(search) == true) ||
                (x.Email?.ToLower().Contains(search) == true) ||
                (x.TenNhom?.ToLower().Contains(search) == true)
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns: STT(35), Mã nhà cung cấp(100), Tên nhà cung cấp(160), Địa chỉ(Star), Điện thoại(110), Email(130)
            stackTable.Children.Add(CreateRowNhaCungCap("STT", "Mã nhà cung cấp", "Tên nhà cung cấp", "Địa chỉ", "Điện thoại", "Email", isHeader: true, isSummary: false));

            var nhomGroups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.TenNhom) ? "" : x.TenNhom).OrderBy(g => g.Key).ToList();
            if (nhomGroups.Count == 0)
            {
                stackTable.Children.Add(CreateGroupHeaderRow("Nhóm nhà cung cấp:"));
            }
            else
            {
                int totalCount = 0;
                foreach (var group in nhomGroups)
                {
                    string groupTitle = string.IsNullOrEmpty(group.Key) ? "Nhóm nhà cung cấp:" : $"Nhóm nhà cung cấp: {group.Key}";
                    stackTable.Children.Add(CreateGroupHeaderRow(groupTitle));

                    int stt = 1;
                    foreach (var item in group.OrderBy(x => x.MaNhaCungCap).ThenBy(x => x.Name))
                    {
                        totalCount++;
                        stackTable.Children.Add(CreateRowNhaCungCap(
                            stt: (stt++).ToString(),
                            maNcc: item.MaNhaCungCap ?? "",
                            tenNcc: item.Name ?? "",
                            diaChi: item.DiaChi ?? "",
                            sdt: item.DienThoai ?? "",
                            email: item.Email ?? "",
                            isHeader: false,
                            isSummary: false
                        ));
                    }
                }
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateGroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });

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

        private UIElement CreateRowNhaCungCap(string stt, string maNcc, string tenNcc, string diaChi, string sdt, string email, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Mã nhà cung cấp
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) }); // Tên nhà cung cấp
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Địa chỉ
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Điện thoại
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) }); // Email

            if (isSummary)
            {
                var summaryLabelCell = CreateTableCell(diaChi, 0, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                Grid.SetColumnSpan(summaryLabelCell, 6);
                grid.Children.Add(summaryLabelCell);
                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(maNcc, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tenNcc, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
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
                    sb.AppendLine("");

                    sb.AppendLine("STT,Mã nhà cung cấp,Tên nhà cung cấp,Địa chỉ,Điện thoại,Email");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        sb.AppendLine($"\"{stt++}\",\"{item.MaNhaCungCap}\",\"{item.Name?.Replace("\"", "\"\"")}\",\"{item.DiaChi?.Replace("\"", "\"\"")}\",\"{item.DienThoai}\",\"{item.Email?.Replace("\"", "\"\"")}\"");
                    }

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
