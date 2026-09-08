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
    public partial class BaoCaoMatHangControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private readonly LocalMatHangService _matHangService = new LocalMatHangService();
        private List<MatHangViewModel> _rawItems = new List<MatHangViewModel>();

        public BaoCaoMatHangControl(string reportType = "DANH SÁCH MẶT HÀNG THEO NHÓM")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "DANH SÁCH MẶT HÀNG THEO NHÓM" : reportType.Trim().ToUpper();

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
                // 1. Nhóm mặt hàng
                var listNhom = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "🌐" }
                };

                var treeNhom = await _matHangService.GetNhomMatHangTreeAsync();
                FlattenTreeNodes(treeNhom, listNhom);

                CboNhomMatHang.ItemsSource = listNhom;
                CboNhomMatHang.SelectedIndex = 0;

                // 2. Loại mặt hàng / Hãng sản xuất
                if (_reportType.Contains("HÃNG SẢN XUẤT"))
                {
                    TxtLoaiLabel.Text = "Hãng sản xuất";

                    var listHsx = new List<ComboLookupItem>
                    {
                        new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "🏭" }
                    };

                    var hsxList = await _matHangService.GetHangSanXuatListAsync();
                    if (hsxList != null)
                    {
                        foreach (var h in hsxList)
                        {
                            if (!string.IsNullOrEmpty(h.Name))
                            {
                                listHsx.Add(new ComboLookupItem
                                {
                                    Id = h.Id?.ToString() ?? "",
                                    Name = h.Name,
                                    Icon = "🏬"
                                });
                            }
                        }
                    }

                    CboLoaiMatHang.ItemsSource = listHsx;
                    CboLoaiMatHang.SelectedIndex = 0;
                }
                else
                {
                    TxtLoaiLabel.Text = "Loại mặt hàng";

                    var listLoai = new List<ComboLookupItem>
                    {
                        new ComboLookupItem { Id = "", Name = "Tất cả", Icon = "🏷️" }
                    };

                    var loaiList = await _matHangService.GetLoaiMatHangListAsync();
                    if (loaiList != null)
                    {
                        foreach (var l in loaiList)
                        {
                            if (!string.IsNullOrEmpty(l.Name))
                            {
                                listLoai.Add(new ComboLookupItem
                                {
                                    Id = l.Id?.ToString() ?? "",
                                    Name = l.Name,
                                    Icon = "📦"
                                });
                            }
                        }
                    }

                    CboLoaiMatHang.ItemsSource = listLoai;
                    CboLoaiMatHang.SelectedIndex = 0;
                }
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
                string nhomText = (CboNhomMatHang.SelectedItem as ComboLookupItem)?.Name ?? "Tất cả";
                string loaiText = (CboLoaiMatHang.SelectedItem as ComboLookupItem)?.Name ?? "Tất cả";
                TxtFilterSummary.Text = $"Nhóm mặt hàng: {nhomText} | Loại mặt hàng: {loaiText}";

                _rawItems = await _matHangService.GetMatHangListAsync(null);
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
            string selNhomId = (CboNhomMatHang.SelectedItem as ComboLookupItem)?.Id ?? "";
            string selFilter2Id = (CboLoaiMatHang.SelectedItem as ComboLookupItem)?.Id ?? "";

            bool isHsxMode = _reportType.Contains("HÃNG SẢN XUẤT");

            var filtered = _rawItems.Where(x =>
                (string.IsNullOrEmpty(selNhomId) || x.DnhommathangId == selNhomId) &&
                (string.IsNullOrEmpty(selFilter2Id) || (isHsxMode ? x.DhangsanxuatId == selFilter2Id : x.DloaimathangId == selFilter2Id)) &&
                (string.IsNullOrEmpty(search) ||
                 (x.Code?.ToLower().Contains(search) == true) ||
                 (x.Name?.ToLower().Contains(search) == true) ||
                 (x.NhomMatHangName?.ToLower().Contains(search) == true) ||
                 (x.LoaiMatHangName?.ToLower().Contains(search) == true) ||
                 (x.HangSanXuatName?.ToLower().Contains(search) == true))
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Columns: STT(35), Mã hàng(85), Tên mặt hàng(Star), Giá bán(110), Tồn tối đa(85), Tồn tối thiểu(85)
            stackTable.Children.Add(CreateRowMatHang("STT", "Mã hàng", "Tên mặt hàng", "Giá bán", "Tồn tối đa", "Tồn tối thiểu", isHeader: true, isSummary: false));

            if (isHsxMode)
            {
                var hsxGroups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.HangSanXuatName) ? "" : x.HangSanXuatName).OrderBy(g => g.Key).ToList();
                if (hsxGroups.Count == 0)
                {
                    stackTable.Children.Add(CreateGroupHeaderRow("Hãng sản xuất:"));
                }
                else
                {
                    foreach (var group in hsxGroups)
                    {
                        string groupTitle = string.IsNullOrEmpty(group.Key) ? "Hãng sản xuất:" : $"Hãng sản xuất: {group.Key}";
                        stackTable.Children.Add(CreateGroupHeaderRow(groupTitle));

                        int stt = 1;
                        foreach (var item in group.OrderBy(x => x.Code).ThenBy(x => x.Name))
                        {
                            string giaBanStr = item.Giaban.HasValue ? item.Giaban.Value.ToString("#,##0.00") : "0.00";
                            string tonToiDaStr = item.Tontoida.HasValue && item.Tontoida.Value > 0 ? item.Tontoida.Value.ToString("N0") : "";
                            string tonToiThieuStr = item.Tontoithieu.HasValue ? item.Tontoithieu.Value.ToString("N0") : "0";

                            stackTable.Children.Add(CreateRowMatHang(
                                stt: (stt++).ToString(),
                                maHang: item.Code ?? "",
                                tenMatHang: item.Name ?? "",
                                giaBan: giaBanStr,
                                tonToiDa: tonToiDaStr,
                                tonToiThieu: tonToiThieuStr,
                                isHeader: false,
                                isSummary: false
                            ));
                        }
                    }
                }
            }
            else
            {
                var nhomGroups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.NhomMatHangName) ? "" : x.NhomMatHangName).OrderBy(g => g.Key).ToList();
                if (nhomGroups.Count == 0)
                {
                    stackTable.Children.Add(CreateGroupHeaderRow("Nhóm mặt hàng:"));
                }
                else
                {
                    foreach (var group in nhomGroups)
                    {
                        string groupTitle = string.IsNullOrEmpty(group.Key) ? "Nhóm mặt hàng:" : $"Nhóm mặt hàng: {group.Key}";
                        stackTable.Children.Add(CreateGroupHeaderRow(groupTitle));

                        int stt = 1;
                        foreach (var item in group.OrderBy(x => x.Code).ThenBy(x => x.Name))
                        {
                            string giaBanStr = item.Giaban.HasValue ? item.Giaban.Value.ToString("#,##0.00") : "0.00";
                            string tonToiDaStr = item.Tontoida.HasValue && item.Tontoida.Value > 0 ? item.Tontoida.Value.ToString("N0") : "";
                            string tonToiThieuStr = item.Tontoithieu.HasValue ? item.Tontoithieu.Value.ToString("N0") : "0";

                            stackTable.Children.Add(CreateRowMatHang(
                                stt: (stt++).ToString(),
                                maHang: item.Code ?? "",
                                tenMatHang: item.Name ?? "",
                                giaBan: giaBanStr,
                                tonToiDa: tonToiDaStr,
                                tonToiThieu: tonToiThieuStr,
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

        private UIElement CreateGroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });

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

        private UIElement CreateRowMatHang(string stt, string maHang, string tenMatHang, string giaBan, string tonToiDa, string tonToiThieu, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : 21 };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Mã hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Tên mặt hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Giá bán
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Tồn tối đa
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Tồn tối thiểu

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(maHang, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tenMatHang, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(giaBan, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tonToiDa, 4, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tonToiThieu, 5, HorizontalAlignment.Center, weight, isHeader, false));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight, bool isHeader, bool isSummary)
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

                    sb.AppendLine("STT,Mã hàng,Tên mặt hàng,Giá bán,Tồn tối đa,Tồn tối thiểu");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        string giaBanStr = item.Giaban.HasValue ? item.Giaban.Value.ToString("#,##0.00") : "0.00";
                        string tonToiDaStr = item.Tontoida.HasValue && item.Tontoida.Value > 0 ? item.Tontoida.Value.ToString("N0") : "";
                        string tonToiThieuStr = item.Tontoithieu.HasValue ? item.Tontoithieu.Value.ToString("N0") : "0";

                        sb.AppendLine($"\"{stt++}\",\"{item.Code}\",\"{item.Name?.Replace("\"", "\"\"")}\",\"{giaBanStr}\",\"{tonToiDaStr}\",\"{tonToiThieuStr}\"");
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
