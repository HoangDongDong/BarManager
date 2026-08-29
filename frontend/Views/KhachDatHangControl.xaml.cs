using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class KhachDatHangControl : UserControl
    {
        private LocalKhachDatHangService _service;
        private string _currentCategoryId = null;
        private bool _isMucDichDatMode = false;
        private System.Collections.Generic.List<DatHangViewModel> _allDatHangList;

        public KhachDatHangControl()
        {
            InitializeComponent();
            _service = new LocalKhachDatHangService();
            
            // Default filter past month
            DpTuNgay.SelectedDate = DateTime.Now.AddMonths(-1);
            DpDenNgay.SelectedDate = DateTime.Now;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadTreeAsync();
            var khachHangs = await _service.GetKhachHangLookupAsync();
            CmbKhachHang.ItemsSource = khachHangs;
            await LoadData();
        }
        
        private async System.Threading.Tasks.Task ReloadTreeAsync()
        {
            var treeData = await _service.GetTreeAsync(_isMucDichDatMode);
            TvCategoryTree.ItemsSource = treeData;
        }

        private void BtnModeSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (BtnModeSwitch.ContextMenu != null)
            {
                BtnModeSwitch.ContextMenu.PlacementTarget = BtnModeSwitch;
                BtnModeSwitch.ContextMenu.Placement = PlacementMode.Bottom;
                BtnModeSwitch.ContextMenu.IsOpen = true;
                BtnModeSwitch.IsChecked = false;
            }
        }

        private async void MenuItemModePhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            _isMucDichDatMode = false;
            TxtTreeHeader.Text = "Phương thức đặt";
            _currentCategoryId = null;
            await ReloadTreeAsync();
            await LoadData();
        }

        private async void MenuItemModeMucDich_Click(object sender, RoutedEventArgs e)
        {
            _isMucDichDatMode = true;
            TxtTreeHeader.Text = "Mục đích đặt";
            _currentCategoryId = null;
            await ReloadTreeAsync();
            await LoadData();
        }

        private async void TvPhuongThucDat_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeCategoryViewModel selectedNode)
            {
                _currentCategoryId = selectedNode.Id;
                UpdateTrashUIState();
                await LoadData();
            }
        }

        private void UpdateTrashUIState()
        {
            bool isTrash = _currentCategoryId == "-1";
            if (BtnToolbarThemMoi != null) BtnToolbarThemMoi.IsEnabled = !isTrash;
            if (BtnToolbarChinhSua != null) BtnToolbarChinhSua.IsEnabled = !isTrash;
            if (BtnToolbarXoa != null) BtnToolbarXoa.Content = isTrash ? "❌ Xóa vĩnh viễn" : "❌ Xóa (Del)";
        }

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool isTrash = _currentCategoryId == "-1";
            if (MenuKhoiPhuc != null) MenuKhoiPhuc.Visibility = isTrash ? Visibility.Visible : Visibility.Collapsed;
            if (MenuThemMoi != null) MenuThemMoi.Visibility = !isTrash ? Visibility.Visible : Visibility.Collapsed;
            if (MenuChinhSua != null) MenuChinhSua.Visibility = !isTrash ? Visibility.Visible : Visibility.Collapsed;
            if (MenuXoa != null) MenuXoa.Header = isTrash ? "Xóa vĩnh viễn" : "Xóa (Del)";
        }

        private void MenuItem_ThungRac_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.ItemsSource is System.Collections.ObjectModel.ObservableCollection<TreeCategoryViewModel> tree && tree.Count > 0)
            {
                var rootNode = tree[0];
                var trashNode = rootNode.Children.FirstOrDefault(x => x.Id == "-1");
                if (trashNode != null)
                {
                    _currentCategoryId = "-1";
                    UpdateTrashUIState();
                    _ = LoadData();
                }
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var currentList = (DgDatHang.ItemsSource as List<DatHangViewModel>) ?? _allDatHangList;
            var win = new ThemMoiDatHangWindow(null, currentList);
            win.Owner = Window.GetWindow(this);
            win.OrderSaved += async () =>
            {
                await LoadData();
            };
            win.ShowDialog();
            _ = LoadData();
        }

        private void BtnToolbarChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                OpenEditOrderWindow(selectedOrder.Id);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đơn đặt hàng cần chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgDatHang_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentCategoryId == "-1") return;
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                OpenEditOrderWindow(selectedOrder.Id);
            }
        }

        private void OpenEditOrderWindow(string orderId)
        {
            var currentList = (DgDatHang.ItemsSource as List<DatHangViewModel>) ?? _allDatHangList;
            var win = new ThemMoiDatHangWindow(orderId, currentList);
            win.Owner = Window.GetWindow(this);
            win.OrderSaved += async () =>
            {
                await LoadData();
            };
            win.ShowDialog();
            _ = LoadData();
        }

        private async void BtnToolbarXoa_Click(object sender, RoutedEventArgs e)
        {
            bool isTrash = _currentCategoryId == "-1";
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                string msg = isTrash 
                    ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN đơn đặt hàng '{selectedOrder.SoPhieu}' không?" 
                    : $"Bạn có chắc muốn đưa đơn đặt hàng '{selectedOrder.SoPhieu}' vào Thùng rác không?";

                if (MessageBox.Show(msg, "Xác nhận xóa", MessageBoxButton.YesNo, isTrash ? MessageBoxImage.Warning : MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (await _service.DeleteDatHangAsync(selectedOrder.Id, isPermanent: isTrash))
                    {
                        await LoadData();
                    }
                }
            }
            else if (isTrash)
            {
                var confirmTrash = MessageBox.Show("Bạn có chắc chắn muốn DỌN SẠCH THÙNG RÁC (Xóa vĩnh viễn toàn bộ dữ liệu trong thùng rác)?", "Dọn sạch thùng rác", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmTrash == MessageBoxResult.Yes)
                {
                    if (await _service.EmptyTrashAsync())
                    {
                        await ReloadTreeAsync();
                        await LoadData();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đơn đặt hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnToolbarThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Không thể thực hiện import/cập nhật dữ liệu từ excel với dữ liệu này", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnToolbarXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = (DgDatHang.ItemsSource as System.Collections.Generic.IEnumerable<DatHangViewModel>)?.ToList() ?? _allDatHangList;
                if (list == null || list.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu đơn đặt hàng để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachKhachDatHang_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                    Title = "Xuất danh sách khách đặt hàng ra Excel"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Khách Đặt Hàng");

                        // Tiêu đề
                        ws.Cell(1, 1).Value = "BAR & LOUNGE RESTAURANT";
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 13;

                        ws.Cell(2, 1).Value = "Địa chỉ: 12 Xuân Thủy, Cầu Giấy, Hà Nội - Hotline: (024) 3756 8888";
                        ws.Cell(2, 1).Style.Font.Italic = true;

                        ws.Cell(4, 1).Value = "DANH SÁCH KHÁCH ĐẶT HÀNG";
                        ws.Cell(4, 1).Style.Font.Bold = true;
                        ws.Cell(4, 1).Style.Font.FontSize = 16;
                        ws.Cell(4, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        ws.Range(4, 1, 4, 17).Merge();

                        string timeFilter = $"Từ ngày: {(DpTuNgay.SelectedDate.HasValue ? DpTuNgay.SelectedDate.Value.ToString("dd/MM/yyyy") : "---")}   Đến ngày: {(DpDenNgay.SelectedDate.HasValue ? DpDenNgay.SelectedDate.Value.ToString("dd/MM/yyyy") : "---")}";
                        ws.Cell(5, 1).Value = timeFilter;
                        ws.Cell(5, 1).Style.Font.Italic = true;
                        ws.Cell(5, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        ws.Range(5, 1, 5, 17).Merge();

                        // Bảng dữ liệu
                        int startRow = 7;
                        string[] headers = new string[]
                        {
                            "STT", "Ngày", "Số phiếu", "Tên khách", "Địa chỉ", "Điện thoại",
                            "Email", "Tổng cộng", "Phương thức đặt", "Mục đích đặt",
                            "Từ giờ", "Đến giờ", "Từ ngày", "Đến ngày"
                        };

                        for (int c = 0; c < headers.Length; c++)
                        {
                            ws.Cell(startRow, c + 1).Value = headers[c];
                        }

                        var headerRange = ws.Range(startRow, 1, startRow, headers.Length);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#2e75b6");
                        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                        int currentRow = startRow + 1;
                        int stt = 1;
                        foreach (var item in list)
                        {
                            ws.Cell(currentRow, 1).Value = stt++;
                            ws.Cell(currentRow, 2).Value = item.Ngay.HasValue ? item.Ngay.Value.ToString("dd/MM/yyyy") : "";
                            ws.Cell(currentRow, 3).Value = item.SoPhieu ?? "";
                            ws.Cell(currentRow, 4).Value = item.TenKhach ?? "";
                            ws.Cell(currentRow, 5).Value = item.DiaChi ?? "";
                            ws.Cell(currentRow, 6).Value = item.DienThoai ?? "";
                            ws.Cell(currentRow, 7).Value = item.Email ?? "";
                            
                            decimal tc = 0;
                            if (decimal.TryParse(item.TongCong?.Replace(",", "")?.Replace(".", ""), out decimal t)) tc = t;
                            ws.Cell(currentRow, 8).Value = tc;

                            ws.Cell(currentRow, 9).Value = item.PhuongThucDatName ?? "";
                            ws.Cell(currentRow, 10).Value = item.MucDichDatName ?? "";
                            ws.Cell(currentRow, 11).Value = item.TuGio.HasValue ? item.TuGio.Value.ToString("HH:mm") : "";
                            ws.Cell(currentRow, 12).Value = item.DenGio.HasValue ? item.DenGio.Value.ToString("HH:mm") : "";
                            ws.Cell(currentRow, 13).Value = item.TuNgay.HasValue ? item.TuNgay.Value.ToString("dd/MM/yyyy") : "";
                            ws.Cell(currentRow, 14).Value = item.DenNgay.HasValue ? item.DenNgay.Value.ToString("dd/MM/yyyy") : "";

                            // Định dạng
                            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 11).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 12).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 13).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 14).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                            currentRow++;
                        }

                        var dataRange = ws.Range(startRow, 1, currentRow - 1, headers.Length);
                        dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                        // Dòng tổng cộng
                        ws.Cell(currentRow, 7).Value = "TỔNG CỘNG:";
                        ws.Cell(currentRow, 7).Style.Font.Bold = true;

                        ws.Cell(currentRow, 8).FormulaA1 = $"SUM(H{startRow + 1}:H{currentRow - 1})";
                        ws.Cell(currentRow, 8).Style.Font.Bold = true;
                        ws.Cell(currentRow, 8).Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                        ws.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0";

                        ws.Columns().AdjustToContents();

                        workbook.SaveAs(saveDlg.FileName);
                    }

                    var result = MessageBox.Show("Xuất danh sách khách đặt hàng ra Excel thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnToolbarIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgDatHang, "Đặt hàng");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnToolbarTong_Click(object sender, RoutedEventArgs e)
        {
            if (BtnToolbarTong.IsChecked == true)
            {
                BorderTongFooter.Visibility = Visibility.Visible;
                UpdateSummaryFooter();
            }
            else
            {
                BorderTongFooter.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateSummaryFooter()
        {
            if (BorderTongFooter == null || BorderTongFooter.Visibility != Visibility.Visible) return;
            decimal total = 0;
            var list = (DgDatHang?.ItemsSource as System.Collections.Generic.IEnumerable<DatHangViewModel>) ?? _allDatHangList;
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (decimal.TryParse(item.TongCong?.Replace(",", "")?.Replace(".", ""), out decimal tc))
                    {
                        total += tc;
                    }
                }
            }
            if (TxtTongCongFooter != null)
            {
                TxtTongCongFooter.Text = total.ToString("N0");
            }
        }

        private void BtnToolbarPhanTich_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = (DgDatHang?.ItemsSource as System.Collections.Generic.IEnumerable<DatHangViewModel>)?.ToList() ?? _allDatHangList ?? new System.Collections.Generic.List<DatHangViewModel>();
                var win = new ThongKeNhanhWindow(list);
                try
                {
                    var owner = Window.GetWindow(this);
                    if (owner != null && owner.IsVisible)
                    {
                        win.Owner = owner;
                    }
                }
                catch { }
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở phân tích: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MenuItem_KhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                if (MessageBox.Show($"Bạn có chắc muốn khôi phục đơn đặt hàng '{selectedOrder.SoPhieu}' không?", "Khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (await _service.RestoreDatHangAsync(selectedOrder.Id))
                    {
                        await LoadData();
                    }
                }
            }
        }
        
        private async void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async void CmbKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            DateTime? tuNgay = DpTuNgay.SelectedDate;
            DateTime? denNgay = DpDenNgay.SelectedDate;
            string soPhieu = TxtSoPhieuLoc?.Text?.Trim();
            string khachHangId = CmbKhachHang?.SelectedValue as string;
            
            _allDatHangList = await _service.GetDatHangListAsync(_currentCategoryId, _isMucDichDatMode, tuNgay, denNgay, soPhieu, khachHangId);
            ApplyQuickFilter();
        }

        private void ApplyQuickFilter()
        {
            if (_allDatHangList == null)
            {
                DgDatHang.ItemsSource = null;
                UpdateSummaryFooter();
                return;
            }

            string filter = TxtLocNhanh?.Text?.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                for (int i = 0; i < _allDatHangList.Count; i++) _allDatHangList[i].Stt = i + 1;
                DgDatHang.ItemsSource = _allDatHangList;
                if (_allDatHangList.Count > 0 && DgDatHang.SelectedIndex < 0)
                {
                    DgDatHang.SelectedIndex = 0;
                }
                UpdateSummaryFooter();
                return;
            }

            var filtered = _allDatHangList.Where(d =>
                (!string.IsNullOrEmpty(d.SoPhieu) && d.SoPhieu.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(d.TenKhach) && d.TenKhach.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(d.DiaChi) && d.DiaChi.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(d.DienThoai) && d.DienThoai.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(d.Email) && d.Email.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(d.PhuongThucDatName) && d.PhuongThucDatName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(d.MucDichDatName) && d.MucDichDatName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            for (int i = 0; i < filtered.Count; i++) filtered[i].Stt = i + 1;
            DgDatHang.ItemsSource = filtered;
            if (filtered.Count > 0 && DgDatHang.SelectedIndex < 0)
            {
                DgDatHang.SelectedIndex = 0;
            }
            UpdateSummaryFooter();
        }

        private void TxtLocNhanh_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyQuickFilter();
        }

        private async void TxtSoPhieuLoc_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await LoadData();
            }
        }

        private async void TxtSoPhieuLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadData();
        }

        private async void DgDatHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                if (TxtInfoKhoiTao != null)
                    TxtInfoKhoiTao.Text = selectedOrder.Timecreated.HasValue ? $"Khởi tạo: {selectedOrder.Timecreated:dd/MM/yyyy HH:mm}" : "Khởi tạo:";
                if (TxtInfoNguoiTao != null)
                    TxtInfoNguoiTao.Text = $"Khởi tạo bởi: {selectedOrder.UsercreatedName ?? "Administrator"}";
                if (TxtInfoSuaDoi != null)
                    TxtInfoSuaDoi.Text = selectedOrder.Timemodified.HasValue ? $"Sửa đổi gần nhất: {selectedOrder.Timemodified:dd/MM/yyyy HH:mm}" : "Sửa đổi gần nhất:";
                if (TxtInfoNguoiSua != null)
                    TxtInfoNguoiSua.Text = $"Sửa đổi bởi: {selectedOrder.UsermodifiedName ?? "Administrator"}";

                var chiTiet = await _service.GetDatHangChiTietListAsync(selectedOrder.Id);
                DgDatHangChiTiet.ItemsSource = chiTiet;
            }
            else
            {
                DgDatHangChiTiet.ItemsSource = null;
                if (TxtInfoKhoiTao != null) TxtInfoKhoiTao.Text = "Khởi tạo:";
                if (TxtInfoNguoiTao != null) TxtInfoNguoiTao.Text = "Khởi tạo bởi:";
                if (TxtInfoSuaDoi != null) TxtInfoSuaDoi.Text = "Sửa đổi gần nhất:";
                if (TxtInfoNguoiSua != null) TxtInfoNguoiSua.Text = "Sửa đổi bởi:";
            }
        }

        private async void BtnRefreshPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            await ReloadTreeAsync();
        }

        private void BtnThemPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                parentId = selected.Id;
            }

            var win = new ThemPhuongThucDatWindow();
            win.ParentId = parentId;
            win.IsMucDichDat = _isMucDichDatMode;
            win.OnSaveSuccess = () => BtnRefreshPhuongThuc_Click(null, null);
            win.ShowDialog();
        }

        private async void BtnThemThuMucPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            // Determine max Thư mục number
            int nextIndex = 1;
            if (TvCategoryTree.ItemsSource is System.Collections.ObjectModel.ObservableCollection<TreeCategoryViewModel> items && items.Count > 0)
            {
                var rootNode = items[0]; // "Tất cả"
                var folderNodes = rootNode.Children.Where(x => x.Name != null && x.Name.StartsWith("Thư mục ")).ToList();
                foreach (var node in folderNodes)
                {
                    if (int.TryParse(node.Name.Replace("Thư mục ", ""), out int num))
                    {
                        if (num >= nextIndex) nextIndex = num + 1;
                    }
                }
            }

            string defaultName = $"Thư mục {nextIndex}";
            
            // Generate a random GUID for temporary tracking so we can find it
            // Wait, we need to find it after reloading. We can just find the one that has Name == defaultName, since it's freshly created.
            if (await _service.InsertPhuongThucDatAsync(defaultName, "", null, null, _isMucDichDatMode))
            {
                await ReloadTreeAsync();
                
                // Find the newly added item and set IsEditing = true
                if (TvCategoryTree.ItemsSource is System.Collections.ObjectModel.ObservableCollection<TreeCategoryViewModel> currentItems && currentItems.Count > 0)
                {
                    var rootNode = currentItems[0];
                    var newItem = rootNode.Children.FirstOrDefault(x => x.Name == defaultName);
                    if (newItem != null)
                    {
                        // Expand the root node so the new item is visible
                        if (TvCategoryTree.ItemContainerGenerator.ContainerFromItem(rootNode) is TreeViewItem tvi)
                        {
                            tvi.IsExpanded = true;
                            tvi.UpdateLayout();
                        }
                        
                        await Application.Current.Dispatcher.InvokeAsync(() => 
                        {
                            newItem.IsEditing = true;
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
        }

        private void InlineEditTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt && txt.Visibility == Visibility.Visible)
            {
                txt.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txt.Focus();
                    System.Windows.Input.Keyboard.Focus(txt);
                    txt.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void InlineEditTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt && (bool)e.NewValue == true)
            {
                txt.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txt.Focus();
                    System.Windows.Input.Keyboard.Focus(txt);
                    txt.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private bool IsDuplicateName(System.Collections.ObjectModel.ObservableCollection<TreeCategoryViewModel> tree, string name, string excludeId)
        {
            if (tree == null) return false;
            foreach (var node in tree)
            {
                if (node.Name != null && node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && node.Id != excludeId)
                    return true;
                if (node.Children != null && IsDuplicateName(node.Children, name, excludeId))
                    return true;
            }
            return false;
        }

        private async void InlineEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt)
            {
                string id = txt.Tag as string;
                if (!string.IsNullOrEmpty(id) && txt.DataContext is TreeCategoryViewModel model && model.IsEditing)
                {
                    if (string.IsNullOrWhiteSpace(model.Name))
                    {
                        model.IsEditing = false;
                        await ReloadTreeAsync();
                        return;
                    }

                    if (IsDuplicateName(TvCategoryTree.ItemsSource as System.Collections.ObjectModel.ObservableCollection<TreeCategoryViewModel>, model.Name, id))
                    {
                        MessageBox.Show($"Tên '{model.Name}' đã tồn tại. Hệ thống sẽ khôi phục tên cũ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        model.IsEditing = false;
                        await ReloadTreeAsync();
                        return;
                    }

                    model.IsEditing = false;
                    await _service.UpdatePhuongThucDatAsync(id, model.Name, model.Note, model.SimageId, _isMucDichDatMode);
                }
            }
        }

        private async void InlineEditTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    string id = txt.Tag as string;
                    if (!string.IsNullOrEmpty(id) && txt.DataContext is TreeCategoryViewModel model && model.IsEditing)
                    {
                        if (string.IsNullOrWhiteSpace(model.Name))
                        {
                            model.IsEditing = false;
                            await ReloadTreeAsync();
                            e.Handled = true;
                            return;
                        }

                        if (IsDuplicateName(TvCategoryTree.ItemsSource as System.Collections.ObjectModel.ObservableCollection<TreeCategoryViewModel>, model.Name, id))
                        {
                            MessageBox.Show($"Tên '{model.Name}' đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Handled = true;
                            return;
                        }

                        model.IsEditing = false;
                        await _service.UpdatePhuongThucDatAsync(id, model.Name, model.Note, model.SimageId, _isMucDichDatMode);
                    }
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    if (txt.DataContext is TreeCategoryViewModel model && model.IsEditing)
                    {
                        model.IsEditing = false;
                        // Reload tree to revert changes
                        await ReloadTreeAsync();
                    }
                    e.Handled = true;
                }
            }
        }

        private void BtnSuaPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể sửa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selected.Id == "-1")
                {
                    MessageBox.Show("Không thể sửa Thùng rác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var win = new ThemPhuongThucDatWindow(selected);
                win.IsMucDichDat = _isMucDichDatMode;
                win.OnSaveSuccess = () => BtnRefreshPhuongThuc_Click(null, null);
                win.ShowDialog();
            }
        }

        private async void BtnXoaPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể xóa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selected.Id == "-1")
                {
                    var confirmTrash = MessageBox.Show("Bạn có chắc chắn muốn DỌN SẠCH THÙNG RÁC (Xóa vĩnh viễn toàn bộ dữ liệu trong thùng rác)?", "Dọn sạch thùng rác", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (confirmTrash == MessageBoxResult.Yes)
                    {
                        if (await _service.EmptyTrashAsync())
                        {
                            await ReloadTreeAsync();
                            await LoadData();
                        }
                    }
                    return;
                }

                string typeName = _isMucDichDatMode ? "mục đích đặt" : "phương thức đặt";
                if (MessageBox.Show($"Bạn có chắc muốn đưa {typeName} '{selected.Name}' vào Thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (await _service.DeletePhuongThucDatAsync(selected.Id, _isMucDichDatMode, isPermanent: false))
                    {
                        await ReloadTreeAsync();
                        await LoadData();
                    }
                }
            }
        }

        private async void BtnDoiTenPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể đổi tên thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selected.Id == "-1")
                {
                    MessageBox.Show("Không thể đổi tên Thùng rác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var inputWin = new InputWindow("Đổi tên", "Nhập tên mới:", selected.Name);
                if (inputWin.ShowDialog() == true)
                {
                    string newName = inputWin.InputText;
                    if (!string.IsNullOrWhiteSpace(newName) && newName != selected.Name)
                    {
                        if (await _service.UpdatePhuongThucDatAsync(selected.Id, newName, selected.Note, selected.SimageId, _isMucDichDatMode))
                        {
                            BtnRefreshPhuongThuc_Click(null, null);
                        }
                    }
                }
            }
        }

        private void MenuItem_ThemPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                parentId = selected.ParentId; // Add sibling
            }
            var win = new ThemPhuongThucDatWindow();
            win.ParentId = parentId;
            win.IsMucDichDat = _isMucDichDatMode;
            win.OnSaveSuccess = () => BtnRefreshPhuongThuc_Click(null, null);
            win.ShowDialog();
        }

        private void MenuItem_ThemNhanh_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                parentId = selected.ParentId; // Add sibling
            }
            var win = new ThemNhanhPhuongThucDatWindow(parentId, _isMucDichDatMode);
            if (win.ShowDialog() == true)
            {
                BtnRefreshPhuongThuc_Click(null, null);
            }
        }

        private async void MenuItem_ThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                parentId = selected.ParentId; // Add sibling
            }
            if (await _service.InsertPhuongThucDatAsync("----------------", "Phân cách", null, parentId, _isMucDichDatMode))
            {
                BtnRefreshPhuongThuc_Click(null, null);
            }
        }

        private void MenuItem_ThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            BtnThemThuMucPhuongThuc_Click(sender, e);
        }

        private void MenuItem_ThemCon_Click(object sender, RoutedEventArgs e)
        {
            BtnThemPhuongThuc_Click(sender, e); // Already adds child based on selection
        }

        private void MenuItem_ThemPhuongThucCon_Click(object sender, RoutedEventArgs e)
        {
            BtnThemPhuongThuc_Click(sender, e);
        }

        private void MenuItem_ThemNhanhCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected && selected.Id != null)
            {
                parentId = selected.Id;
            }
            
            var win = new ThemNhanhPhuongThucDatWindow(parentId, _isMucDichDatMode);
            if (win.ShowDialog() == true)
            {
                BtnRefreshPhuongThuc_Click(null, null);
            }
        }

        private async void MenuItem_ThemPhanCachCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected && selected.Id != null)
            {
                parentId = selected.Id;
            }

            if (await _service.InsertPhuongThucDatAsync("----------------", "Phân cách", null, parentId, _isMucDichDatMode))
            {
                BtnRefreshPhuongThuc_Click(null, null);
            }
        }

        private void MenuItem_ThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            BtnThemPhuongThuc_Click(sender, e);
        }
    }
}
