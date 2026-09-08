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
    public partial class BaoCaoBanKhuVucControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private readonly LocalBanKhuVucService _banKhuVucService = new LocalBanKhuVucService();
        private List<BanViewModel> _rawItems = new List<BanViewModel>();

        public BaoCaoBanKhuVucControl(string reportType = "BÁO CÁO CẤU HÌNH BÀN KHU VỰC")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "BÁO CÁO CẤU HÌNH BÀN KHU VỰC" : reportType.Trim();

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
                var listKhuVuc = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "🌐" }
                };

                var treeKhuVuc = await _banKhuVucService.GetKhuVucTreeAsync();
                if (treeKhuVuc != null)
                {
                    // treeKhuVuc root is "Tất cả", its children are actual KhuVuc nodes
                    foreach (var root in treeKhuVuc)
                    {
                        FlattenTreeNodes(root.Children, listKhuVuc);
                    }
                }

                CboKhuVuc.ItemsSource = listKhuVuc;
                CboKhuVuc.SelectedIndex = 0;
            }
            catch { }
        }

        private void FlattenTreeNodes(IEnumerable<KhuVucViewModel> nodes, List<ComboLookupItem> result)
        {
            if (nodes == null) return;
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.Id) && node.Id != "-1" && !string.IsNullOrEmpty(node.Name) && node.Name != "Tất cả")
                {
                    result.Add(new ComboLookupItem
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Icon = "🏢"
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
                string kvText = (CboKhuVuc.SelectedItem as ComboLookupItem)?.Name ?? "Tất cả";
                TxtFilterSummary.Text = $"Khu vực: {kvText}";

                _rawItems = await _banKhuVucService.GetBanListAsync(null);
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
            var selectedKhuVuc = CboKhuVuc.SelectedItem as ComboLookupItem;
            string selKhuVucName = selectedKhuVuc != null && !string.IsNullOrEmpty(selectedKhuVuc.Id) ? selectedKhuVuc.Name : "";

            var filtered = _rawItems.Where(x =>
                (string.IsNullOrEmpty(selKhuVucName) || string.Equals(x.KhuVucName, selKhuVucName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(search) ||
                 (x.Name?.ToLower().Contains(search) == true) ||
                 (x.KhuVucName?.ToLower().Contains(search) == true) ||
                 (x.LoaiPhongName?.ToLower().Contains(search) == true) ||
                 (x.BanggiaName?.ToLower().Contains(search) == true) ||
                 (x.Note?.ToLower().Contains(search) == true))
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Header row: STT (40), Bàn/Phòng (200), Cách tính giá (Star)
            stackTable.Children.Add(CreateRowBanKhuVuc("STT", "Bàn/Phòng", "Cách tính giá", isHeader: true));

            var kvGroups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.KhuVucName) ? "Chưa chọn khu vực" : x.KhuVucName)
                                   .OrderBy(g => g.Key)
                                   .ToList();

            if (kvGroups.Count == 0)
            {
                stackTable.Children.Add(CreateGroupHeaderRow("Khu vực:"));
            }
            else
            {
                foreach (var group in kvGroups)
                {
                    string groupTitle = $"Khu vực: {group.Key}";
                    stackTable.Children.Add(CreateGroupHeaderRow(groupTitle));

                    int stt = 1;
                    foreach (var item in group.OrderBy(x => x.Name))
                    {
                        // Determine cách tính giá representation
                        string cachTinhGia = item.BanggiaName;
                        if (string.IsNullOrWhiteSpace(cachTinhGia)) cachTinhGia = item.LoaiPhongName;
                        if (string.IsNullOrWhiteSpace(cachTinhGia)) cachTinhGia = item.Note;
                        if (string.IsNullOrWhiteSpace(cachTinhGia) && item.Dongia.HasValue && item.Dongia.Value > 0)
                        {
                            cachTinhGia = item.Dongia.Value.ToString("#,##0.00");
                        }
                        if (string.IsNullOrWhiteSpace(cachTinhGia)) cachTinhGia = "";

                        stackTable.Children.Add(CreateRowBanKhuVuc(
                            stt: (stt++).ToString(),
                            banPhong: item.Name ?? "",
                            cachTinhGia: cachTinhGia,
                            isHeader: false
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = Brushes.White
            };
            Grid.SetColumnSpan(border, 3);

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

        private UIElement CreateRowBanKhuVuc(string stt, string banPhong, string cachTinhGia, bool isHeader)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : 21 };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });                  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });                 // Bàn/Phòng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Cách tính giá

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader));
            grid.Children.Add(CreateTableCell(banPhong, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader));
            grid.Children.Add(CreateTableCell(cachTinhGia, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight, bool isHeader)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 2, 4, 2),
                Background = isHeader ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9")) : Brushes.White
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text ?? "",
                FontSize = isHeader ? 10.5 : 10,
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
                    sb.AppendLine($"\"{TxtFilterSummary.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("STT,Bàn/Phòng,Cách tính giá");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        string cachTinhGia = item.BanggiaName;
                        if (string.IsNullOrWhiteSpace(cachTinhGia)) cachTinhGia = item.LoaiPhongName;
                        if (string.IsNullOrWhiteSpace(cachTinhGia)) cachTinhGia = item.Note;
                        if (string.IsNullOrWhiteSpace(cachTinhGia) && item.Dongia.HasValue && item.Dongia.Value > 0)
                        {
                            cachTinhGia = item.Dongia.Value.ToString("#,##0.00");
                        }
                        if (string.IsNullOrWhiteSpace(cachTinhGia)) cachTinhGia = "";

                        sb.AppendLine($"\"{stt++}\",\"{item.Name?.Replace("\"", "\"\"")}\",\"{cachTinhGia.Replace("\"", "\"\"")}\"");
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
