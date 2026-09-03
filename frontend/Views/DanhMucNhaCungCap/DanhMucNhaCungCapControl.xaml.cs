using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views;

namespace QuanLyBar.Client.Views.DanhMucNhaCungCap
{
    public partial class DanhMucNhaCungCapControl : UserControl
    {
        private ObservableCollection<NhomNhaCungCapTreeItem> _treeItems = new ObservableCollection<NhomNhaCungCapTreeItem>();
        private List<NhaCungCapItem> _allNhaCungCap = new List<NhaCungCapItem>();
        private NhomNhaCungCapTreeItem _selectedTreeItem;
        private NhaCungCapItem _selectedNhaCungCap;

        public DanhMucNhaCungCapControl()
        {
            InitializeComponent();
            Loaded += DanhMucNhaCungCapControl_Loaded;
        }

        private async void DanhMucNhaCungCapControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        public async Task LoadTreeAsync()
        {
            try
            {
                _treeItems = await LocalNhaCungCapService.GetNhomNhaCungCapTreeAsync();
                TvNhomNhaCungCap.ItemsSource = _treeItems;

                if (_treeItems.Count > 0)
                {
                    _selectedTreeItem = _treeItems[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTreeAsync: " + ex.Message);
            }
        }

        public async Task LoadDataGridAsync()
        {
            try
            {
                string nhomId = _selectedTreeItem?.Id;
                string specType = _selectedTreeItem?.SpecialType ?? (string.IsNullOrEmpty(nhomId) ? "ALL" : null);
                string filter = TxtLoc?.Text.Trim() ?? "";

                _allNhaCungCap = await LocalNhaCungCapService.GetNhaCungCapListAsync(nhomId, filter, specType);
                DgNhaCungCap.ItemsSource = _allNhaCungCap;

                if (_allNhaCungCap.Count > 0)
                {
                    DgNhaCungCap.SelectedIndex = 0;
                    _selectedNhaCungCap = _allNhaCungCap[0];
                }
                else
                {
                    _selectedNhaCungCap = null;
                }

                UpdateDetailTabs();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadDataGridAsync: " + ex.Message);
            }
        }

        private void UpdateDetailTabs()
        {
            if (_selectedNhaCungCap != null)
            {
                TxtDetailTimeCreated.Text = _selectedNhaCungCap.TimeCreatedFormatted;
                TxtDetailUserCreated.Text = string.IsNullOrEmpty(_selectedNhaCungCap.UserCreatedName) ? "Administrator" : _selectedNhaCungCap.UserCreatedName;
                TxtDetailTimeModified.Text = _selectedNhaCungCap.TimeModifiedFormatted;
                TxtDetailUserModified.Text = _selectedNhaCungCap.UserModifiedName ?? "";

                _ = LoadRelatedReceiptsAsync(_selectedNhaCungCap.Id);
            }
            else
            {
                TxtDetailTimeCreated.Text = "";
                TxtDetailUserCreated.Text = "";
                TxtDetailTimeModified.Text = "";
                TxtDetailUserModified.Text = "";
                DgPhieuNhapKho.ItemsSource = null;
            }
        }

        private async Task LoadRelatedReceiptsAsync(string nccId)
        {
            try
            {
                var receipts = await LocalNhaCungCapService.GetPhieuNhapKhoByNccAsync(nccId);
                DgPhieuNhapKho.ItemsSource = receipts;
            }
            catch { }
        }

        private async void TvNhomNhaCungCap_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as NhomNhaCungCapTreeItem;
            await LoadDataGridAsync();
        }

        private void DgNhaCungCap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedNhaCungCap = DgNhaCungCap.SelectedItem as NhaCungCapItem;
            UpdateDetailTabs();
        }

        private void DgNhaCungCap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedNhaCungCap != null)
            {
                BtnChinhSua_Click(sender, e);
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadDataGridAsync();
        }

        private void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            BtnThemNhom_Click(sender, e);
        }

        private bool _isFlatMode = false;
        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _isFlatMode = !_isFlatMode;
            if (_isFlatMode)
            {
                BtnXemTheoThuMuc.Background = System.Windows.Media.Brushes.LightYellow;
                BtnXemTheoThuMuc.BorderBrush = System.Windows.Media.Brushes.Goldenrod;
            }
            else
            {
                BtnXemTheoThuMuc.Background = System.Windows.Media.Brushes.Transparent;
                BtnXemTheoThuMuc.BorderBrush = System.Windows.Media.Brushes.Transparent;
            }
        }

        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            if (BtnMenuCheDo.ContextMenu != null)
            {
                BtnMenuCheDo.ContextMenu.PlacementTarget = BtnMenuCheDo;
                BtnMenuCheDo.ContextMenu.IsOpen = true;
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            string defaultNhomId = (_selectedTreeItem != null && _selectedTreeItem.SpecialType == null) ? _selectedTreeItem.Id : null;
            var win = new ThemNhaCungCapWindow(defaultNhomId: defaultNhomId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadDataGridAsync();
            };
            win.ShowDialog();
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhaCungCap == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemNhaCungCapWindow(item: _selectedNhaCungCap);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadDataGridAsync();
            };
            win.ShowDialog();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhaCungCap == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isTrash = (_selectedTreeItem?.SpecialType == "TRASH");
            string confirmMsg = isTrash
                ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN nhà cung cấp '{_selectedNhaCungCap.Name}'?"
                : $"Bạn có chắc chắn muốn chuyển nhà cung cấp '{_selectedNhaCungCap.Name}' vào Thùng rác?";

            var result = MessageBox.Show(confirmMsg, "Xác nhận xóa", MessageBoxButton.YesNo, isTrash ? MessageBoxImage.Warning : MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool ok = await LocalNhaCungCapService.DeleteNhaCungCapAsync(_selectedNhaCungCap.Id, permanent: isTrash);
                if (ok)
                {
                    await LoadDataGridAsync();
                }
                else
                {
                    MessageBox.Show("Lỗi khi xóa dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            string parentId = (_selectedTreeItem != null && _selectedTreeItem.SpecialType == null) ? _selectedTreeItem.Id : null;
            var win = new ThemNhomNhaCungCapWindow(parentId: parentId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeAsync();
            };
            win.ShowDialog();
        }

        private void BtnSuaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.SpecialType != null)
            {
                MessageBox.Show("Vui lòng chọn một nhóm tùy chỉnh để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemNhomNhaCungCapWindow(id: _selectedTreeItem.Id, name: _selectedTreeItem.Name);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeAsync();
            };
            win.ShowDialog();
        }

        private void BtnXoaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.SpecialType != null)
            {
                MessageBox.Show("Không thể xóa nhóm mặc định này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm '{_selectedTreeItem.Name}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Đã xóa nhóm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _ = LoadTreeAsync();
            }
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private void BtnExpandCollapse_Click(object sender, RoutedEventArgs e)
        {
            // Toggle tree expansion
        }

        private void BtnCauHinhNhom_Click(object sender, RoutedEventArgs e)
        {
            BtnThemNhom_Click(sender, e);
        }

        private async void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhNhaCungCapBangExcelWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadTreeAsync();
                await LoadDataGridAsync();
            }
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allNhaCungCap == null || _allNhaCungCap.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu nhà cung cấp để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachNhaCungCap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("NhaCungCap");
                        string[] headers = new[] { "STT", "Mã nhà cung cấp", "Tên nhà cung cấp", "Địa chỉ", "Điện thoại", "Email", "Website", "Nhóm nhà cung cấp", "Ghi chú" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            ws.Cell(1, i + 1).Value = headers[i];
                            ws.Cell(1, i + 1).Style.Font.Bold = true;
                            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                        }

                        int r = 2;
                        foreach (var item in _allNhaCungCap)
                        {
                            ws.Cell(r, 1).Value = item.Stt;
                            ws.Cell(r, 2).Value = item.MaNhaCungCap;
                            ws.Cell(r, 3).Value = item.Name;
                            ws.Cell(r, 4).Value = item.DiaChi;
                            ws.Cell(r, 5).Value = item.DienThoai;
                            ws.Cell(r, 6).Value = item.Email;
                            ws.Cell(r, 7).Value = item.Website;
                            ws.Cell(r, 8).Value = item.TenNhom;
                            ws.Cell(r, 9).Value = item.Note;
                            r++;
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show($"Đã xuất thành công {_allNhaCungCap.Count} nhà cung cấp ra file Excel!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgNhaCungCap, "Nhà cung cấp");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            int count = _allNhaCungCap.Count;
            MessageBox.Show($"Tổng số nhà cung cấp: {count}", "Tổng hợp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region TreeView ContextMenu Handlers
        private void MiSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadTreeAsync();
        }

        private void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadTreeAsync();
        }

        private void MiSaoChepNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem != null)
            {
                Clipboard.SetText(_selectedTreeItem.Name);
                MessageBox.Show($"Đã sao chép tên nhóm: {_selectedTreeItem.Name}", "Sao chép", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MiMoRongNhom_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TvNhomNhaCungCap.Items)
            {
                var tvi = TvNhomNhaCungCap.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (tvi != null) tvi.IsExpanded = true;
            }
        }

        private void MiThuGonNhom_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TvNhomNhaCungCap.Items)
            {
                var tvi = TvNhomNhaCungCap.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (tvi != null) tvi.IsExpanded = false;
            }
        }

        private void MiThungRacNhom_Click(object sender, RoutedEventArgs e)
        {
            var trashNode = _treeItems.FirstOrDefault(x => x.SpecialType == "TRASH");
            if (trashNode != null)
            {
                _selectedTreeItem = trashNode;
                _ = LoadDataGridAsync();
            }
        }

        private void MiBieuTuongNhom_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đổi biểu tượng nhóm đang được phát triển!", "Biểu tượng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiThuocTinhNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem != null)
            {
                MessageBox.Show($"Tên nhóm: {_selectedTreeItem.Name}\nID: {_selectedTreeItem.Id}", "Thuộc tính nhóm", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region DataGrid ContextMenu Handlers
        // Track which column was right-clicked and its value
        private string _contextMenuColumnHeader = "";
        private string _contextMenuCellValue = "";

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            // Determine current column from CurrentColumn or the clicked position
            string colName = "Tên nhà cung cấp";
            string cellValue = "";

            if (DgNhaCungCap.CurrentColumn != null)
            {
                colName = DgNhaCungCap.CurrentColumn.Header?.ToString() ?? "Tên nhà cung cấp";
            }

            // Get the cell value for current selected row + current column
            if (_selectedNhaCungCap != null && DgNhaCungCap.CurrentColumn != null)
            {
                cellValue = GetCellValue(_selectedNhaCungCap, colName);
            }

            _contextMenuColumnHeader = colName;
            _contextMenuCellValue = cellValue;

            if (MenuDatCot != null) MenuDatCot.Header = $"Đặt {colName}";
            if (MenuLocCot != null) MenuLocCot.Header = $"Lọc {colName}";

            // Update "Sắp xếp theo" submenu to show current column first
            if (MenuSapXep != null && MenuSapXep.Items.Count > 0)
            {
                // Bold the first item to indicate "sort by current column"
                var firstItem = MenuSapXep.Items[0] as MenuItem;
                if (firstItem != null)
                    firstItem.Header = $"↑ Tăng dần theo {colName}";
                var secondItem = MenuSapXep.Items.Count > 1 ? MenuSapXep.Items[1] as MenuItem : null;
                if (secondItem != null)
                    secondItem.Header = $"↓ Giảm dần theo {colName}";
            }
        }

        private string GetCellValue(NhaCungCapItem item, string columnHeader)
        {
            return columnHeader switch
            {
                "Mã nhà cung cấp" => item.MaNhaCungCap ?? "",
                "Tên nhà cung cấp" => item.Name ?? "",
                "Địa chỉ" => item.DiaChi ?? "",
                "Điện thoại" => item.DienThoai ?? "",
                "Email" => item.Email ?? "",
                "Website" => item.Website ?? "",
                "Ghi chú" => item.Note ?? "",
                "Nhóm nhà cung cấp" => item.TenNhom ?? "",
                _ => item.Name ?? ""
            };
        }

        private string GetSortKey(NhaCungCapItem item, string columnHeader)
        {
            return columnHeader switch
            {
                "Mã nhà cung cấp" => item.MaNhaCungCap ?? "",
                "Tên nhà cung cấp" => item.Name ?? "",
                "Địa chỉ" => item.DiaChi ?? "",
                "Điện thoại" => item.DienThoai ?? "",
                "Email" => item.Email ?? "",
                "Website" => item.Website ?? "",
                "Ghi chú" => item.Note ?? "",
                "Nhóm nhà cung cấp" => item.TenNhom ?? "",
                _ => item.Name ?? ""
            };
        }

        private void MenuDatCot_Click(object sender, RoutedEventArgs e)
        {
            // "Đặt" = paste cell value into the filter box
            if (!string.IsNullOrEmpty(_contextMenuCellValue))
            {
                TxtLoc.Text = _contextMenuCellValue;
            }
            else if (_selectedNhaCungCap != null)
            {
                TxtLoc.Text = GetCellValue(_selectedNhaCungCap, _contextMenuColumnHeader);
            }
        }

        private void MenuLocCot_Click(object sender, RoutedEventArgs e)
        {
            // "Lọc" = set filter text and reload
            string val = !string.IsNullOrEmpty(_contextMenuCellValue)
                ? _contextMenuCellValue
                : (_selectedNhaCungCap != null ? GetCellValue(_selectedNhaCungCap, _contextMenuColumnHeader) : "");

            TxtLoc.Text = val;
            // LoadDataGridAsync will be triggered by TextChanged
        }

        private bool _sortAscending = true;
        private string _lastSortColumn = "";

        private void MenuItem_SortByMa_Click(object sender, RoutedEventArgs e)
        {
            // If triggered from dynamic Tăng dần menu, sort by current column ascending
            var mi = sender as MenuItem;
            if (mi?.Name == "MenuSortAsc" && !string.IsNullOrEmpty(_contextMenuColumnHeader))
            {
                _lastSortColumn = ""; // reset to force ascending
                SortByColumn(_contextMenuColumnHeader);
            }
            else
            {
                SortByColumn("Mã nhà cung cấp");
            }
        }

        private void MenuItem_SortByName_Click(object sender, RoutedEventArgs e)
        {
            // If triggered from dynamic Giảm dần menu, sort by current column descending
            var mi = sender as MenuItem;
            if (mi?.Name == "MenuSortDesc" && !string.IsNullOrEmpty(_contextMenuColumnHeader))
            {
                _lastSortColumn = _contextMenuColumnHeader; // force descending
                _sortAscending = true; // will be toggled to false
                SortByColumn(_contextMenuColumnHeader);
            }
            else
            {
                SortByColumn("Tên nhà cung cấp");
            }
        }

        private void MenuItem_SortByCustom_Click(object sender, RoutedEventArgs e)
        {
            // "Sort by current column" or "Thứ tự tùy chọn" toggles asc/desc
            if (!string.IsNullOrEmpty(_contextMenuColumnHeader))
                SortByColumn(_contextMenuColumnHeader);
            else
                SortByColumn("Tên nhà cung cấp");
        }

        private void SortByColumn(string columnHeader)
        {
            // Toggle asc/desc if same column, else reset to asc
            if (_lastSortColumn == columnHeader)
                _sortAscending = !_sortAscending;
            else
            {
                _sortAscending = true;
                _lastSortColumn = columnHeader;
            }

            if (_sortAscending)
                _allNhaCungCap = _allNhaCungCap.OrderBy(x => GetSortKey(x, columnHeader)).ToList();
            else
                _allNhaCungCap = _allNhaCungCap.OrderByDescending(x => GetSortKey(x, columnHeader)).ToList();

            // Re-number STT
            for (int i = 0; i < _allNhaCungCap.Count; i++)
                _allNhaCungCap[i].Stt = i + 1;

            DgNhaCungCap.ItemsSource = null;
            DgNhaCungCap.ItemsSource = _allNhaCungCap;

            // Show sort indicator in column header
            foreach (var col in DgNhaCungCap.Columns)
            {
                string hdr = col.Header?.ToString() ?? "";
                // Remove existing indicators
                hdr = hdr.Replace(" ▲", "").Replace(" ▼", "");
                if (hdr == columnHeader)
                    col.Header = hdr + (_sortAscending ? " ▲" : " ▼");
                else
                    col.Header = hdr;
            }
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            string val = _contextMenuCellValue;
            if (string.IsNullOrEmpty(val) && _selectedNhaCungCap != null)
                val = GetCellValue(_selectedNhaCungCap, _contextMenuColumnHeader);

            if (!string.IsNullOrEmpty(val))
            {
                Clipboard.SetText(val);
                MessageBox.Show($"Đã sao chép: {val}", "Sao chép ô", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhaCungCap != null)
            {
                string info = $"{_selectedNhaCungCap.MaNhaCungCap}\t{_selectedNhaCungCap.Name}\t{_selectedNhaCungCap.DiaChi}\t{_selectedNhaCungCap.DienThoai}\t{_selectedNhaCungCap.Email}\t{_selectedNhaCungCap.Website}";
                Clipboard.SetText(info);
                MessageBox.Show("Đã sao chép thông tin nhà cung cấp vào bộ nhớ tạm.", "Sao chép vùng chọn", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgNhaCungCap.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MenuItem_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng tùy chỉnh cột hiển thị đang được phát triển!", "Cột hiển thị", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhaCungCap != null)
            {
                string info = $"Mã NCC: {_selectedNhaCungCap.MaNhaCungCap}\n" +
                              $"Tên NCC: {_selectedNhaCungCap.Name}\n" +
                              $"Địa chỉ: {_selectedNhaCungCap.DiaChi}\n" +
                              $"Điện thoại: {_selectedNhaCungCap.DienThoai}\n" +
                              $"Email: {_selectedNhaCungCap.Email}\n" +
                              $"Website: {_selectedNhaCungCap.Website}\n" +
                              $"Khởi tạo: {_selectedNhaCungCap.TimeCreatedFormatted} bởi {_selectedNhaCungCap.UserCreatedName}";

                MessageBox.Show(info, "Thuộc tính nhà cung cấp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
