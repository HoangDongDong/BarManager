using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TonKho
{
    public partial class TonNhieuKhoControl : UserControl
    {
        private List<TonNhieuKhoItem> _allTonKhoList = new List<TonNhieuKhoItem>();
        private List<KhoHangComboItem> _khoList = new List<KhoHangComboItem>();
        private string _selectedNhomId = "all";
        private bool _isLoading = false;

        public TonNhieuKhoControl()
        {
            InitializeComponent();
            Loaded += TonNhieuKhoControl_Loaded;
        }

        private async void TonNhieuKhoControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                DpTheKhoTuNgay.SelectedDate = new DateTime(2015, 1, 1);
                DpTheKhoDenNgay.SelectedDate = DateTime.Today;

                await LoadTreeNhomMatHangAsync();
                await LoadTonNhieuKhoAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("TonNhieuKhoControl_Loaded error: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadTreeNhomMatHangAsync()
        {
            TvNhomMatHang.Items.Clear();

            var rootItem = new TreeViewItem
            {
                Header = CreateTreeHeader("🌐", "Tất cả"),
                Tag = "all",
                IsSelected = true,
                IsExpanded = true
            };
            TvNhomMatHang.Items.Add(rootItem);

            var matHangService = new LocalMatHangService();
            var treeData = await matHangService.GetNhomMatHangTreeAsync();

            var rootNode = treeData.FirstOrDefault(x => x.Name == "Tất cả");
            var groupsToDisplay = (rootNode != null && rootNode.Children != null && rootNode.Children.Count > 0)
                ? rootNode.Children
                : treeData.Where(x => x.Id != "-1" && x.Name != "Tất cả");

            foreach (var group in groupsToDisplay)
            {
                if (group.Id == "-1") continue;
                rootItem.Items.Add(BuildTreeNode(group));
            }
        }

        private TreeViewItem BuildTreeNode(NhomMatHangViewModel vm)
        {
            var item = new TreeViewItem
            {
                Header = CreateTreeHeader("📁", vm.Name),
                Tag = vm.Id,
                IsExpanded = false
            };

            if (vm.Children != null)
            {
                foreach (var child in vm.Children)
                {
                    if (child.Id == "-1") continue;
                    item.Items.Add(BuildTreeNode(child));
                }
            }

            return item;
        }

        private StackPanel CreateTreeHeader(string icon, string text)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBlock { Text = icon, Margin = new Thickness(0, 0, 5, 0), FontSize = 12 });
            sp.Children.Add(new TextBlock { Text = text, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
            return sp;
        }

        private async Task LoadTonNhieuKhoAsync()
        {
            string search = TxtTimKiem.Text?.Trim();
            bool chiTon = ChkChiTonKhoKhacKhong.IsChecked == true;

            var data = await LocalTonKhoService.GetTonNhieuKhoDataAsync(_selectedNhomId, search, chiTon);
            _khoList = data.KhoList;
            _allTonKhoList = data.Items;

            RebuildDataGridColumns();

            DgTonNhieuKho.ItemsSource = _allTonKhoList;
            UpdateTotals();

            if (_allTonKhoList.Count > 0)
            {
                DgTonNhieuKho.SelectedIndex = 0;
            }
            else
            {
                DgTheKho.ItemsSource = null;
            }
        }

        private void RebuildDataGridColumns()
        {
            DgTonNhieuKho.Columns.Clear();

            // 1. STT
            var colStt = new DataGridTextColumn
            {
                Header = "STT",
                Binding = new Binding("Stt"),
                Width = 45
            };
            var styleCenter = new Style(typeof(TextBlock));
            styleCenter.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            styleCenter.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            styleCenter.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748b"))));
            colStt.ElementStyle = styleCenter;
            DgTonNhieuKho.Columns.Add(colStt);

            // 2. Mã hàng
            DgTonNhieuKho.Columns.Add(new DataGridTextColumn { Header = "Mã hàng", Binding = new Binding("MaHang"), Width = 85 });

            // 3. Mặt hàng
            DgTonNhieuKho.Columns.Add(new DataGridTextColumn { Header = "Mặt hàng", Binding = new Binding("TenHang"), Width = 160 });

            // 4. ĐVT
            DgTonNhieuKho.Columns.Add(new DataGridTextColumn { Header = "ĐVT", Binding = new Binding("TenDonViTinh"), Width = 55 });

            var styleRight = new Style(typeof(TextBlock));
            styleRight.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
            styleRight.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            styleRight.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(0, 0, 4, 0)));

            // 5. Cột Kho đầu tiên (nếu có)
            if (_khoList.Count > 0)
            {
                var kho1 = _khoList[0];
                var colKho1 = new DataGridTextColumn
                {
                    Header = kho1.Name,
                    Binding = new Binding($"KhoTonDict[{kho1.Id}]") { StringFormat = "{0:N0}" },
                    Width = 75,
                    ElementStyle = styleRight
                };
                DgTonNhieuKho.Columns.Add(colKho1);
            }

            // 6. Tổng tồn
            var styleTongTon = new Style(typeof(TextBlock));
            styleTongTon.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
            styleTongTon.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            styleTongTon.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(0, 0, 4, 0)));
            styleTongTon.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));

            var colTongTon = new DataGridTextColumn
            {
                Header = "Tổng tồn",
                Binding = new Binding("TongTonFormatted"),
                Width = 75,
                ElementStyle = styleTongTon
            };
            DgTonNhieuKho.Columns.Add(colTongTon);

            // 7. Quy đổi
            var colQuyDoi = new DataGridTextColumn
            {
                Header = "Quy đổi",
                Binding = new Binding("QuyDoiFormatted"),
                Width = 75,
                ElementStyle = styleRight
            };
            DgTonNhieuKho.Columns.Add(colQuyDoi);

            // 8. Tồn 2 đvt
            DgTonNhieuKho.Columns.Add(new DataGridTextColumn { Header = "Tồn 2 đvt", Binding = new Binding("Ton2Dvt"), Width = 75 });

            // 9. Ghi chú
            DgTonNhieuKho.Columns.Add(new DataGridTextColumn { Header = "Ghi chú", Binding = new Binding("GhiChu"), Width = 100 });

            // 10. Các kho còn lại (kho 2, 3...)
            for (int i = 1; i < _khoList.Count; i++)
            {
                var kho = _khoList[i];
                var colKho = new DataGridTextColumn
                {
                    Header = kho.Name,
                    Binding = new Binding($"KhoTonDict[{kho.Id}]") { StringFormat = "{0:N0}" },
                    Width = 85,
                    ElementStyle = styleRight
                };
                DgTonNhieuKho.Columns.Add(colKho);
            }
        }

        private void UpdateTotals()
        {
            SpFooterTotals.Children.Clear();

            // Label TỔNG (chiếm độ rộng STT + Mã hàng + Mặt hàng + ĐVT = 45 + 85 + 160 + 55 = 345)
            SpFooterTotals.Children.Add(new TextBlock
            {
                Text = "TỔNG",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0f172a")),
                Width = 345,
                VerticalAlignment = VerticalAlignment.Center
            });

            if (_allTonKhoList == null || _allTonKhoList.Count == 0)
            {
                return;
            }

            // Tổng Kho 1
            if (_khoList.Count > 0)
            {
                var kho1 = _khoList[0];
                decimal sumKho1 = _allTonKhoList.Sum(x => x.GetKhoTon(kho1.Id));
                SpFooterTotals.Children.Add(new TextBlock
                {
                    Text = sumKho1 != 0 ? sumKho1.ToString("N0") : "0",
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    Width = 75,
                    Margin = new Thickness(0, 0, 4, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            // Tổng Tồn
            decimal sumTongTon = _allTonKhoList.Sum(x => x.TongTon);
            SpFooterTotals.Children.Add(new TextBlock
            {
                Text = sumTongTon != 0 ? sumTongTon.ToString("N0") : "0",
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right,
                Width = 75,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Tổng Quy đổi
            decimal sumQuyDoi = _allTonKhoList.Sum(x => x.QuyDoi);
            SpFooterTotals.Children.Add(new TextBlock
            {
                Text = sumQuyDoi > 0 ? sumQuyDoi.ToString("N0") : "0",
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right,
                Width = 75,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Khoảng trống cho Tồn 2 đvt (75) + Ghi chú (100) = 175
            SpFooterTotals.Children.Add(new TextBlock { Width = 175 });

            // Tổng các kho còn lại
            for (int i = 1; i < _khoList.Count; i++)
            {
                var kho = _khoList[i];
                decimal sumKho = _allTonKhoList.Sum(x => x.GetKhoTon(kho.Id));
                SpFooterTotals.Children.Add(new TextBlock
                {
                    Text = sumKho != 0 ? sumKho.ToString("N0") : "0",
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    Width = 85,
                    Margin = new Thickness(0, 0, 4, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadTonNhieuKhoAsync();
        }

        private async void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoading) return;
            await LoadTonNhieuKhoAsync();
        }

        private async void FilterChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            await LoadTonNhieuKhoAsync();
        }

        private async void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem tvi)
            {
                _selectedNhomId = tvi.Tag?.ToString() ?? "all";
                await LoadTonNhieuKhoAsync();
            }
        }

        private async void DgTonNhieuKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTonNhieuKho.SelectedItem is TonNhieuKhoItem item)
            {
                await LoadTheKhoAsync(item);
            }
            else
            {
                DgTheKho.ItemsSource = null;
            }
        }

        private async Task LoadTheKhoAsync(TonNhieuKhoItem item)
        {
            if (item == null) return;
            DateTime? tuNgay = DpTheKhoTuNgay.SelectedDate;
            DateTime? denNgay = DpTheKhoDenNgay.SelectedDate;

            var list = await LocalTonKhoService.GetTheKhoListAsync(item.DmathangId, null, tuNgay, denNgay);
            DgTheKho.ItemsSource = list;
        }

        private async void DpTheKho_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTonNhieuKho.SelectedItem is TonNhieuKhoItem item)
            {
                await LoadTheKhoAsync(item);
            }
        }

        private async void BtnTheKhoRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (DgTonNhieuKho.SelectedItem is TonNhieuKhoItem item)
            {
                await LoadTheKhoAsync(item);
            }
        }

        #region TreeView Menu Actions
        private async void MenuSortByCode_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomMatHangAsync();
        }

        private async void MenuSortByName_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomMatHangAsync();
        }

        private async void MenuRefreshTree_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomMatHangAsync();
            await LoadTonNhieuKhoAsync();
        }
        #endregion

        #region DataGrid Context Menu Actions
        private void MenuSaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgTonNhieuKho.CurrentCell.Item != null && DgTonNhieuKho.CurrentCell.Column != null)
                {
                    var cellContent = DgTonNhieuKho.CurrentCell.Column.GetCellContent(DgTonNhieuKho.CurrentCell.Item);
                    if (cellContent is TextBlock tb)
                    {
                        Clipboard.SetText(tb.Text ?? "");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Copy cell error: " + ex.Message);
            }
        }

        private void MenuSaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgTonNhieuKho.SelectedItem is TonNhieuKhoItem item)
                {
                    string text = $"{item.Stt}\t{item.MaHang}\t{item.TenHang}\t{item.TenDonViTinh}\t{item.TongTonFormatted}\t{item.QuyDoiFormatted}\t{item.Ton2Dvt}\t{item.GhiChu}";
                    Clipboard.SetText(text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Copy row error: " + ex.Message);
            }
        }

        private void MenuTuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgTonNhieuKho.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tất cả các cột kho hàng đang được hiển thị đầy đủ.", "Cột hiển thị", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_allTonKhoList == null || _allTonKhoList.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu tồn kho để xuất Excel.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"BaoCaoTonNhieuKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("TonNhieuKho");
                        ws.Cell(1, 1).Value = "STT";
                        ws.Cell(1, 2).Value = "Mã hàng";
                        ws.Cell(1, 3).Value = "Mặt hàng";
                        ws.Cell(1, 4).Value = "ĐVT";
                        
                        int colIdx = 5;
                        foreach (var kho in _khoList)
                        {
                            ws.Cell(1, colIdx++).Value = kho.Name;
                        }
                        ws.Cell(1, colIdx++).Value = "Tổng tồn";
                        ws.Cell(1, colIdx++).Value = "Quy đổi";
                        ws.Cell(1, colIdx++).Value = "Tồn 2 ĐVT";
                        ws.Cell(1, colIdx++).Value = "Ghi chú";

                        var headerRow = ws.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2F8");

                        int row = 2;
                        foreach (var item in _allTonKhoList)
                        {
                            ws.Cell(row, 1).Value = item.Stt;
                            ws.Cell(row, 2).Value = item.MaHang;
                            ws.Cell(row, 3).Value = item.TenHang;
                            ws.Cell(row, 4).Value = item.TenDonViTinh;
                            
                            int c = 5;
                            foreach (var kho in _khoList)
                            {
                                ws.Cell(row, c++).Value = item.GetKhoTon(kho.Id);
                            }
                            ws.Cell(row, c++).Value = item.TongTon;
                            ws.Cell(row, c++).Value = item.QuyDoi;
                            ws.Cell(row, c++).Value = item.Ton2Dvt;
                            ws.Cell(row, c++).Value = item.GhiChu;
                            row++;
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuInDanhSach_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(DgTonNhieuKho, "In danh sách tồn nhiều kho");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
