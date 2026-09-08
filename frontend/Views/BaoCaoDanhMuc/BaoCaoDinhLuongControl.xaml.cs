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
    public partial class BaoCaoDinhLuongControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private readonly LocalMatHangService _matHangService = new LocalMatHangService();
        private List<DinhLuongReportItem> _rawItems = new List<DinhLuongReportItem>();

        public BaoCaoDinhLuongControl(string reportType = "CÔNG THỨC ĐỊNH LƯỢNG")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "CÔNG THỨC ĐỊNH LƯỢNG" : reportType.Trim();

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
                var listNhom = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "🌐" }
                };

                var treeNhom = await _matHangService.GetNhomMatHangTreeAsync();
                FlattenTreeNodes(treeNhom, listNhom);

                CboNhomHang.ItemsSource = listNhom;
                CboNhomHang.SelectedIndex = 0;
            }
            catch { }
        }

        private void FlattenTreeNodes(IEnumerable<NhomMatHangViewModel> nodes, List<ComboLookupItem> result)
        {
            if (nodes == null) return;
            foreach (var node in nodes)
            {
                if (node.Id != "-1" && !string.IsNullOrEmpty(node.Name) && node.Name != "Tất cả")
                {
                    result.Add(new ComboLookupItem
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Icon = "📁"
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
                _rawItems = await _matHangService.GetBaoCaoDinhLuongAsync();
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
            var selectedNhom = CboNhomHang.SelectedItem as ComboLookupItem;
            string selNhomId = selectedNhom?.Id ?? "";
            string selNhomName = selectedNhom != null ? selectedNhom.Name : "Tất cả";

            TxtSubtitle.Text = $"Nhóm hàng: {selNhomName}";

            var filtered = _rawItems.Where(x =>
                (string.IsNullOrEmpty(selNhomId) || x.NhomMatHangId == selNhomId) &&
                (string.IsNullOrEmpty(search) ||
                 (x.MatHangName?.ToLower().Contains(search) == true) ||
                 (x.NhomMatHangName?.ToLower().Contains(search) == true) ||
                 (x.VatTuName?.ToLower().Contains(search) == true) ||
                 (x.DonViTinh?.ToLower().Contains(search) == true))
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Header row: STT (40), Vật tư nguyên liệu (Star), Số lượng (100), Đơn vị tính (90)
            stackTable.Children.Add(CreateRowDinhLuong("STT", "Vật tư nguyên liệu", "Số lượng", "Đơn vị tính", isHeader: true));

            var groupNhom = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.NhomMatHangName) ? "Chưa xếp nhóm" : x.NhomMatHangName)
                                  .OrderBy(g => g.Key)
                                  .ToList();

            if (groupNhom.Count == 0)
            {
                stackTable.Children.Add(CreateLevel1GroupHeaderRow("Nhóm hàng:"));
            }
            else
            {
                foreach (var gNhom in groupNhom)
                {
                    // Level 1 Header (Yellow Background)
                    stackTable.Children.Add(CreateLevel1GroupHeaderRow(gNhom.Key));

                    var groupMatHang = gNhom.GroupBy(x => string.IsNullOrWhiteSpace(x.MatHangName) ? "Chưa đặt tên" : x.MatHangName)
                                            .OrderBy(g => g.Key)
                                            .ToList();

                    foreach (var gMatHang in groupMatHang)
                    {
                        // Level 2 Header (White Background, Bold Text)
                        stackTable.Children.Add(CreateLevel2GroupHeaderRow($"Mặt hàng: {gMatHang.Key}"));

                        int stt = 1;
                        foreach (var item in gMatHang.OrderBy(x => x.VatTuName))
                        {
                            string soLuongStr = item.SoLuong.ToString("#,##0.##");

                            stackTable.Children.Add(CreateRowDinhLuong(
                                stt: (stt++).ToString(),
                                vatTu: item.VatTuName ?? "",
                                soLuong: soLuongStr,
                                dvt: item.DonViTinh ?? "",
                                isHeader: false
                            ));
                        }
                    }
                }
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateLevel1GroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 23 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff59d")) // Yellow group row
            };
            Grid.SetColumnSpan(border, 4);

            var tb = new TextBlock
            {
                Text = title,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            grid.Children.Add(border);
            return grid;
        }

        private UIElement CreateLevel2GroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = Brushes.White
            };
            Grid.SetColumnSpan(border, 4);

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

        private UIElement CreateRowDinhLuong(string stt, string vatTu, string soLuong, string dvt, bool isHeader)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : 21 };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });                  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Vật tư nguyên liệu
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });                 // Số lượng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });                  // Đơn vị tính

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader));
            grid.Children.Add(CreateTableCell(vatTu, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader));
            grid.Children.Add(CreateTableCell(soLuong, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader));
            grid.Children.Add(CreateTableCell(dvt, 3, HorizontalAlignment.Center, weight, isHeader));

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
                    sb.AppendLine($"\"{TxtSubtitle.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("Nhóm hàng,Mặt hàng,STT,Vật tư nguyên liệu,Số lượng,Đơn vị tính");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        sb.AppendLine($"\"{item.NhomMatHangName?.Replace("\"", "\"\"")}\",\"{item.MatHangName?.Replace("\"", "\"\"")}\",\"{stt++}\",\"{item.VatTuName?.Replace("\"", "\"\"")}\",\"{item.SoLuong.ToString("#,##0.##")}\",\"{item.DonViTinh?.Replace("\"", "\"\"")}\"");
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
