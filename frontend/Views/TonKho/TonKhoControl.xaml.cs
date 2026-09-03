using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TonKho
{
    public partial class TonKhoControl : UserControl
    {
        private List<TonKhoItem> _allTonKhoList = new List<TonKhoItem>();
        private string _selectedNhomId = "all";
        private bool _isLoading = false;

        public TonKhoControl()
        {
            InitializeComponent();
            Loaded += TonKhoControl_Loaded;
        }

        private async void TonKhoControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                DpTheKhoTuNgay.SelectedDate = new DateTime(2015, 1, 1);
                DpTheKhoDenNgay.SelectedDate = DateTime.Today;

                await LoadKhoHangAsync();
                await LoadTreeNhomMatHangAsync();
                await LoadTonKhoAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("TonKhoControl_Loaded error: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadKhoHangAsync()
        {
            var khoList = await LocalTonKhoService.GetKhoHangListAsync();
            CboKhoHang.ItemsSource = khoList;
            if (khoList.Count > 0)
            {
                var defaultKho = khoList.FirstOrDefault(k => k.Name == "Kho 10" || k.Name == "10") 
                              ?? khoList.FirstOrDefault(k => k.Name.Contains("10")) 
                              ?? khoList[0];
                CboKhoHang.SelectedItem = defaultKho;
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

        private async void CboKhoHang_ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn)
            {
                string header = btn.Content?.ToString() ?? "";
                if (header.Contains("Thêm"))
                {
                    CboKhoHang.IsDropDownOpen = false;
                    var win = new QuanLyBar.Client.Views.KhoHang.ThemKhoHangWindow();
                    if (win.ShowDialog() == true)
                    {
                        await LoadKhoHangAsync();
                        await LoadTonKhoAsync();
                    }
                }
                else if (header.Contains("Tải"))
                {
                    await LoadKhoHangAsync();
                    await LoadTonKhoAsync();
                }
                else if (header.Contains("Danh mục"))
                {
                    CboKhoHang.IsDropDownOpen = false;
                    var win = new QuanLyBar.Client.Views.KhoHang.DanhMucKhoHangWindow();
                    win.ShowDialog();
                    await LoadKhoHangAsync();
                    await LoadTonKhoAsync();
                }
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

        private async Task LoadTonKhoAsync()
        {
            string khoId = "";
            if (CboKhoHang.SelectedItem is KhoHangComboItem kItem)
            {
                khoId = kItem.Id;
            }
            else if (CboKhoHang.SelectedValue != null)
            {
                khoId = CboKhoHang.SelectedValue.ToString() ?? "";
            }
            else
            {
                var khoList = CboKhoHang.ItemsSource as List<KhoHangComboItem>;
                if (khoList != null && khoList.Count > 0)
                {
                    khoId = khoList[0].Id;
                    CboKhoHang.SelectedIndex = 0;
                }
            }

            string search = TxtTimKiem.Text?.Trim();
            bool chiTon = ChkChiTonKhoKhacKhong.IsChecked == true;

            _allTonKhoList = await LocalTonKhoService.GetTonKhoListAsync(khoId, _selectedNhomId, search, chiTon);
            DgTonKho.ItemsSource = _allTonKhoList;

            UpdateTotals(_allTonKhoList);

            if (_allTonKhoList.Count > 0)
            {
                DgTonKho.SelectedIndex = 0;
                // Tự động load Thẻ kho cho mặt hàng đầu tiên
                await LoadTheKhoAsync(_allTonKhoList[0]);
            }
            else
            {
                DgTheKho.ItemsSource = null;
            }
        }

        private void UpdateTotals(List<TonKhoItem> list)
        {
            if (list == null || list.Count == 0)
            {
                TxtTongTon.Text = "0";
                TxtTongGiaBan.Text = "0";
                TxtTongGiaTriBan.Text = "0";
                TxtTongGiaVon.Text = "0";
                TxtTongGiaTriVon.Text = "0";
                TxtTongQuyDoi.Text = "0";
                return;
            }

            decimal totalTon = list.Sum(x => x.Ton);
            decimal totalGiaBan = list.Sum(x => x.GiaBan);
            decimal totalGiaTriBan = list.Sum(x => x.GiaTriBan);
            decimal totalGiaVon = list.Sum(x => x.GiaVon);
            decimal totalGiaTriVon = list.Sum(x => x.GiaTriVon);
            decimal totalQuyDoi = list.Sum(x => x.QuyDoi);

            TxtTongTon.Text = totalTon != 0 ? totalTon.ToString("N0") : "0";
            TxtTongGiaBan.Text = totalGiaBan > 0 ? totalGiaBan.ToString("N0") : "0";
            TxtTongGiaTriBan.Text = totalGiaTriBan > 0 ? totalGiaTriBan.ToString("N0") : "0";
            TxtTongGiaVon.Text = totalGiaVon > 0 ? totalGiaVon.ToString("N0") : "0";
            TxtTongGiaTriVon.Text = totalGiaTriVon > 0 ? totalGiaTriVon.ToString("N0") : "0";
            TxtTongQuyDoi.Text = totalQuyDoi > 0 ? totalQuyDoi.ToString("N0") : "0";
        }

        private async void CboKhoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            await LoadTonKhoAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadTonKhoAsync();
        }

        private async void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoading) return;
            await LoadTonKhoAsync();
        }

        private async void FilterChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            await LoadTonKhoAsync();
        }

        private async void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem tvi)
            {
                _selectedNhomId = tvi.Tag?.ToString() ?? "all";
                await LoadTonKhoAsync();
            }
        }

        private async void DgTonKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTonKho.SelectedItem is TonKhoItem item)
            {
                await LoadTheKhoAsync(item);
            }
            else
            {
                DgTheKho.ItemsSource = null;
            }
        }

        private async Task LoadTheKhoAsync(TonKhoItem item)
        {
            if (item == null) return;

            // Lấy khoId từ SelectedItem (đáng tin cậy hơn SelectedValue)
            string khoId = "";
            if (CboKhoHang.SelectedItem is KhoHangComboItem kItem)
                khoId = kItem.Id;
            else if (CboKhoHang.SelectedValue != null)
                khoId = CboKhoHang.SelectedValue.ToString() ?? "";

            DateTime? tuNgay = DpTheKhoTuNgay.SelectedDate;
            DateTime? denNgay = DpTheKhoDenNgay.SelectedDate;

            var list = await LocalTonKhoService.GetTheKhoListAsync(item.DmathangId, khoId, tuNgay, denNgay);
            DgTheKho.ItemsSource = list;
        }

        private async void DpTheKho_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTonKho.SelectedItem is TonKhoItem item)
            {
                await LoadTheKhoAsync(item);
            }
        }

        private async void BtnTheKhoRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (DgTonKho.SelectedItem is TonKhoItem item)
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
            await LoadTonKhoAsync();
        }
        #endregion

        #region DataGrid Context Menu Actions
        private void MenuSaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgTonKho.CurrentCell.Item != null && DgTonKho.CurrentCell.Column != null)
                {
                    var cellContent = DgTonKho.CurrentCell.Column.GetCellContent(DgTonKho.CurrentCell.Item);
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
                if (DgTonKho.SelectedItem is TonKhoItem item)
                {
                    string text = $"{item.Stt}\t{item.TenHang}\t{item.TonFormatted}\t{item.TenDonViTinh}\t{item.Ton2Dvt}\t{item.MaHang}\t{item.GiaBanFormatted}\t{item.GiaTriBanFormatted}\t{item.GiaVonFormatted}\t{item.GiaTriVonFormatted}\t{item.QuyDoiFormatted}\t{item.GhiChu}";
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
            foreach (var col in DgTonKho.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tất cả các cột đang được hiển thị theo thiết lập chuẩn.", "Cột hiển thị", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    FileName = $"BaoCaoTonKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("TonKho");
                        ws.Cell(1, 1).Value = "STT";
                        ws.Cell(1, 2).Value = "Mặt hàng";
                        ws.Cell(1, 3).Value = "Tồn";
                        ws.Cell(1, 4).Value = "ĐVT";
                        ws.Cell(1, 5).Value = "Tồn 2 ĐVT";
                        ws.Cell(1, 6).Value = "Mã hàng";
                        ws.Cell(1, 7).Value = "Giá bán";
                        ws.Cell(1, 8).Value = "Giá trị bán";
                        ws.Cell(1, 9).Value = "Giá vốn";
                        ws.Cell(1, 10).Value = "Giá trị vốn";
                        ws.Cell(1, 11).Value = "Quy đổi";
                        ws.Cell(1, 12).Value = "Ghi chú";

                        var headerRow = ws.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2F8");

                        int row = 2;
                        foreach (var item in _allTonKhoList)
                        {
                            ws.Cell(row, 1).Value = item.Stt;
                            ws.Cell(row, 2).Value = item.TenHang;
                            ws.Cell(row, 3).Value = item.Ton;
                            ws.Cell(row, 4).Value = item.TenDonViTinh;
                            ws.Cell(row, 5).Value = item.Ton2Dvt;
                            ws.Cell(row, 6).Value = item.MaHang;
                            ws.Cell(row, 7).Value = item.GiaBan;
                            ws.Cell(row, 8).Value = item.GiaTriBan;
                            ws.Cell(row, 9).Value = item.GiaVon;
                            ws.Cell(row, 10).Value = item.GiaTriVon;
                            ws.Cell(row, 11).Value = item.QuyDoi;
                            ws.Cell(row, 12).Value = item.GhiChu;
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
                    printDialog.PrintVisual(DgTonKho, "In danh sách tồn kho");
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
