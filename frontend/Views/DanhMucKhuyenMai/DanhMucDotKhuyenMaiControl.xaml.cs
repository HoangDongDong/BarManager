using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucDotKhuyenMaiControl : UserControl
    {
        private ObservableCollection<NhomKhachHangTreeItem> _nhomTree;
        private List<DotKhuyenMaiViewModel> _rawList = new List<DotKhuyenMaiViewModel>();
        private NhomKhachHangTreeItem _selectedNhom;
        private DataGridColumn _clickedColumn = null;
        private DataGridCell _clickedCell = null;
        private string _clickedCellValue = "";

        public DanhMucDotKhuyenMaiControl()
        {
            InitializeComponent();
            this.IsVisibleChanged += DanhMucDotKhuyenMaiControl_IsVisibleChanged;
        }

        private async void DanhMucDotKhuyenMaiControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await LoadDataAsync();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _nhomTree = await LocalKhuyenMaiService.GetLoaiHinhKhuyenMaiTreeAsync();
                TvLoaiHinhKhuyenMai.ItemsSource = _nhomTree;

                if (_nhomTree.Count > 0)
                {
                    _selectedNhom = _nhomTree[0];
                }

                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục đợt khuyến mại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshGridAsync()
        {
            try
            {
                string searchKeyword = TxtLoc.Text.Trim();
                string filterId = _selectedNhom?.Id ?? "ALL";
                int itemType = _selectedNhom?.ItemType ?? 0;

                _rawList = await LocalKhuyenMaiService.GetDotKhuyenMaiListAsync(filterId, itemType, searchKeyword);
                DgDotKhuyenMai.ItemsSource = _rawList;

                if (DgDotKhuyenMai.SelectedItem == null && _rawList.Count > 0)
                {
                    DgDotKhuyenMai.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RefreshGridAsync: " + ex.Message);
            }
        }

        private async void TvLoaiHinhKhuyenMai_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem item)
            {
                _selectedNhom = item;
                await RefreshGridAsync();
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            await RefreshGridAsync();
        }

        private void DgDotKhuyenMai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel selected)
            {
                if (TxtInfoKhoiTao != null) TxtInfoKhoiTao.Text = selected.Timecreated?.ToString("dd/MM/yyyy HH:mm:ss") ?? "--/--/---- --:--:--";
                if (TxtInfoNguoiTao != null) TxtInfoNguoiTao.Text = selected.NguoiTao ?? "Administrator";
                if (TxtInfoSuaDoi != null) TxtInfoSuaDoi.Text = selected.Timemodified?.ToString("dd/MM/yyyy HH:mm:ss") ?? "--/--/---- --:--:--";
                if (TxtInfoNguoiSua != null) TxtInfoNguoiSua.Text = selected.NguoiSua ?? "Administrator";
            }
            else
            {
                if (TxtInfoKhoiTao != null) TxtInfoKhoiTao.Text = "--/--/---- --:--:--";
                if (TxtInfoNguoiTao != null) TxtInfoNguoiTao.Text = "Administrator";
                if (TxtInfoSuaDoi != null) TxtInfoSuaDoi.Text = "--/--/---- --:--:--";
                if (TxtInfoNguoiSua != null) TxtInfoNguoiSua.Text = "Administrator";
            }
        }

        #region Toolbar Actions
        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            string defaultLoaiId = (_selectedNhom != null && _selectedNhom.Id != "ALL" && _selectedNhom.Id != "UNASSIGNED" && _selectedNhom.Id != "TRASH")
                ? _selectedNhom.Id
                : null;

            var win = new ThemDotKhuyenMaiWindow(null, defaultLoaiId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => { await RefreshGridAsync(); };
            if (win.ShowDialog() == true)
            {
                _ = RefreshGridAsync();
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel selected)
            {
                var win = new ThemDotKhuyenMaiWindow(selected.Id);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await RefreshGridAsync(); };
                if (win.ShowDialog() == true)
                {
                    _ = RefreshGridAsync();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đợt khuyến mại cần chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgDotKhuyenMai_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel selected)
            {
                bool isTrash = _selectedNhom != null && (_selectedNhom.Id == "TRASH" || _selectedNhom.ParentId == "TRASH");
                string title = isTrash ? "Xác nhận xóa vĩnh viễn" : "Xác nhận xóa";
                string msg = isTrash 
                    ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN đợt khuyến mại '{selected.Name}' không?" 
                    : $"Bạn có chắc chắn muốn xóa đợt khuyến mại '{selected.Name}'?";

                var ask = MessageBox.Show(msg, title, MessageBoxButton.YesNo, isTrash ? MessageBoxImage.Warning : MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = isTrash 
                        ? await LocalKhuyenMaiService.DeletePermanentDotKhuyenMaiAsync(selected.Id)
                        : await LocalKhuyenMaiService.DeleteDotKhuyenMaiAsync(selected.Id);

                    if (ok)
                    {
                        await RefreshGridAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa đợt khuyến mại này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đợt khuyến mại cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm đợt khuyến mại từ Excel đang được cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            var items = (DgDotKhuyenMai.ItemsSource as System.Collections.IEnumerable)?.Cast<DotKhuyenMaiViewModel>().ToList() ?? _rawList;
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Xuất danh sách đợt khuyến mại ra Excel",
                FileName = "DanhSachDotKhuyenMai.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("DotKhuyenMai");
                        string[] headers = new[]
                        {
                            "STT", "Tên đợt khuyến mại", "Loại hình khuyến mại", "Từ ngày", "Đến ngày",
                            "Ngừng áp dụng", "Ghi chú", "Tỉ lệ giảm giá", "Tỉ lệ giảm giá tiền giờ",
                            "Khuyến mại giờ hát", "Tỉ lệ giảm giá tổng", "Từ giờ", "Đến giờ", "Tỉ lệ giảm giá giờ hát"
                        };

                        for (int col = 0; col < headers.Length; col++)
                        {
                            worksheet.Cell(1, col + 1).Value = headers[col];
                        }

                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dfe9f5");
                        headerRow.Height = 25;

                        int row = 2;
                        int stt = 1;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = stt++;
                            worksheet.Cell(row, 2).Value = item.Name ?? "";
                            worksheet.Cell(row, 3).Value = item.TenLoaiHinhKhuyenMai ?? "";
                            worksheet.Cell(row, 4).Value = item.Tungay?.ToString("dd/MM/yyyy") ?? "";
                            worksheet.Cell(row, 5).Value = item.Denngay?.ToString("dd/MM/yyyy") ?? "";
                            worksheet.Cell(row, 6).Value = item.IsNgungApDung ? "Ngừng" : "Hoạt động";
                            worksheet.Cell(row, 7).Value = item.Note ?? "";
                            worksheet.Cell(row, 8).Value = item.Tilegiamgia ?? 0;
                            worksheet.Cell(row, 9).Value = item.Tilegiamgiatiengio ?? 0;
                            worksheet.Cell(row, 10).Value = item.Khuyenmaigiohat?.ToString("HH:mm") ?? "";
                            worksheet.Cell(row, 11).Value = item.Tilegiamgiatong ?? 0;
                            worksheet.Cell(row, 12).Value = item.Tugio?.ToString("HH:mm") ?? "";
                            worksheet.Cell(row, 13).Value = item.Dengio?.ToString("HH:mm") ?? "";
                            worksheet.Cell(row, 14).Value = item.Tilegiamgiagiodau ?? 0;
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    MessageBox.Show($"Đã xuất {items.Count} đợt khuyến mại ra Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgDotKhuyenMai, "Đợt khuyến mại");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Tổng số đợt khuyến mại: {_rawList.Count}", "Thống kê", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            // Dropdown menu options
        }
        #endregion

        #region Left Panel Sub-Toolbar
        private void BtnThemLoaiHinh_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemLoaiHinhKhuyenMaiWindow();
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => { await LoadDataAsync(); };
            if (win.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }

        private void BtnSuaLoaiHinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.Id != "ALL" && _selectedNhom.Id != "UNASSIGNED" && _selectedNhom.Id != "TRASH")
            {
                var win = new ThemLoaiHinhKhuyenMaiWindow(_selectedNhom.Id);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true)
                {
                    _ = LoadDataAsync();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại hình khuyến mại để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Tạo mới thư mục", "Nhập tên thư mục mới:", "Thư mục mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "";
                if (!string.IsNullOrEmpty(folderName))
                {
                    try
                    {
                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                            string newId = Guid.NewGuid().ToString();
                            string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";
                            string sql = $@"
                                INSERT INTO DLOAIHINHKHUYENMAI (
                                    ID, NAME, STATUS, ITEMTYPE, PARENTDIR, USERCREATEDID, USERMODIFIEDID, TIMECREATED, TIMEMODIFIED
                                ) VALUES (
                                    '{newId}', '{folderName.Replace("'", "''")}', 30, '1', '1', '{userId}', '{userId}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                                )";
                            await Dapper.SqlMapper.ExecuteAsync(conn, sql);
                        }
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi tạo thư mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            // Toggle view by folder
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }
        #endregion

        #region TreeView Context Menu Handlers
        private void MiThemMoiLoaiHinh_Click(object sender, RoutedEventArgs e) => BtnThemLoaiHinh_Click(sender, e);
        private void MiThemNhanhLoaiHinh_Click(object sender, RoutedEventArgs e) => BtnThemLoaiHinh_Click(sender, e);
        private void MiThemPhanCach_Click(object sender, RoutedEventArgs e) { }
        private void MiThemThuMucLoaiHinh_Click(object sender, RoutedEventArgs e) => BtnThemThuMuc_Click(sender, e);
        private void MiThemConLoaiHinh_Click(object sender, RoutedEventArgs e) => BtnThemLoaiHinh_Click(sender, e);
        private void MiThemNhanhCon_Click(object sender, RoutedEventArgs e) => BtnThemLoaiHinh_Click(sender, e);
        private void MiThemThuMucCon_Click(object sender, RoutedEventArgs e) => BtnThemThuMuc_Click(sender, e);
        private void MiChinhSuaLoaiHinh_Click(object sender, RoutedEventArgs e) => BtnSuaLoaiHinh_Click(sender, e);
        private void MiSapXepTen_Click(object sender, RoutedEventArgs e) { }
        private void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e) { }
        private void MiSaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null) Clipboard.SetText(_selectedNhom.Name ?? "");
        }
        private void MiMoRong_Click(object sender, RoutedEventArgs e)
        {
            SetExpandedRecursive(_nhomTree, true);
        }
        private void MiThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetExpandedRecursive(_nhomTree, false);
        }
        private void SetExpandedRecursive(IEnumerable<NhomKhachHangTreeItem> items, bool isExpanded)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                item.IsExpanded = isExpanded;
                SetExpandedRecursive(item.Children, isExpanded);
            }
        }
        private async void BtnXoaLoaiHinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.Id != "ALL" && _selectedNhom.Id != "UNASSIGNED" && _selectedNhom.Id != "TRASH")
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa loại hình '{_selectedNhom.Name}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                            string sql = $"UPDATE DLOAIHINHKHUYENMAI SET STATUS = 0, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = '{_selectedNhom.Id.Replace("'", "''")}'";
                            await Dapper.SqlMapper.ExecuteAsync(conn, sql);
                        }
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xóa loại hình: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private async void BtnKhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel selected)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn KHÔI PHỤC đợt khuyến mại '{selected.Name}'?", "Xác nhận khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = await LocalKhuyenMaiService.RestoreDotKhuyenMaiAsync(selected.Id);
                    if (ok)
                    {
                        MessageBox.Show("Khôi phục đợt khuyến mại thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await RefreshGridAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể khôi phục đợt khuyến mại này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (_selectedNhom != null && (_selectedNhom.ParentId == "TRASH" || _selectedNhom.Id == "TRASH"))
            {
                MiKhoiPhucLoaiHinh_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đợt khuyến mại hoặc loại hình trong thùng rác để khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MiKhoiPhucLoaiHinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.Id != "ALL" && _selectedNhom.Id != "UNASSIGNED" && _selectedNhom.Id != "TRASH")
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn KHÔI PHỤC loại hình '{_selectedNhom.Name}'?", "Xác nhận khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = await LocalKhuyenMaiService.RestoreLoaiHinhKhuyenMaiAsync(_selectedNhom.Id);
                    if (ok)
                    {
                        MessageBox.Show("Khôi phục loại hình thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể khôi phục loại hình này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một loại hình trong Thùng rác để khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MiDoiTen_Click(object sender, RoutedEventArgs e) => BtnSuaLoaiHinh_Click(sender, e);
        private void MiThungRac_Click(object sender, RoutedEventArgs e)
        {
            if (_nhomTree != null)
            {
                var trash = FindItemRecursive(_nhomTree, "TRASH");
                if (trash != null)
                {
                    _selectedNhom = trash;
                    _ = RefreshGridAsync();
                }
            }
        }
        private NhomKhachHangTreeItem FindItemRecursive(IEnumerable<NhomKhachHangTreeItem> items, string id)
        {
            if (items == null) return null;
            foreach (var item in items)
            {
                if (item.Id == id) return item;
                var found = FindItemRecursive(item.Children, id);
                if (found != null) return found;
            }
            return null;
        }
        private void MiBieuTuong_Click(object sender, RoutedEventArgs e) { }
        private void MiThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                MessageBox.Show($"Tên: {_selectedNhom.Name}\nID: {_selectedNhom.Id}", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region DataGrid ContextMenu Handlers
        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    DgDotKhuyenMai.SelectedItems.Clear();
                    row.IsSelected = true;
                }
                row.Focus();

                var hit = VisualTreeHelper.HitTest(row, e.GetPosition(row));
                if (hit != null)
                {
                    DependencyObject dep = hit.VisualHit;
                    while (dep != null && !(dep is DataGridCell))
                    {
                        dep = VisualTreeHelper.GetParent(dep);
                    }
                    if (dep is DataGridCell cell)
                    {
                        _clickedCell = cell;
                        _clickedColumn = cell.Column;
                        _clickedCellValue = (cell.Content as TextBlock)?.Text ?? "";
                    }
                }
            }
        }

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (MenuDatCot == null || MenuLocCot == null) return;

            bool isTrash = _selectedNhom != null && (_selectedNhom.Id == "TRASH" || _selectedNhom.ParentId == "TRASH");

            if (isTrash)
            {
                if (MenuKhoiPhuc != null) MenuKhoiPhuc.Visibility = Visibility.Visible;
                if (MenuThemMoi != null) MenuThemMoi.Visibility = Visibility.Collapsed;
                if (MenuThemNhanhExcel != null) MenuThemNhanhExcel.Visibility = Visibility.Collapsed;
                if (MenuCapNhatNhanhExcel != null) MenuCapNhatNhanhExcel.Visibility = Visibility.Collapsed;
                if (MenuChinhSua != null) MenuChinhSua.Visibility = Visibility.Collapsed;
                if (SepDatLoc != null) SepDatLoc.Visibility = Visibility.Collapsed;
                if (MenuDatCot != null) MenuDatCot.Visibility = Visibility.Collapsed;
                if (MenuLocCot != null) MenuLocCot.Visibility = Visibility.Collapsed;
                if (SepSapXep != null) SepSapXep.Visibility = Visibility.Collapsed;
                if (MenuSapXep != null)
                {
                    MenuSapXep.Visibility = Visibility.Visible;
                    MenuSapXep.Header = "Sắp xếp";
                }
                if (MenuRefresh != null) MenuRefresh.Visibility = Visibility.Visible;
                if (MenuInDanhSach != null) MenuInDanhSach.Visibility = Visibility.Collapsed;
                if (SepSaoChep != null) SepSaoChep.Visibility = Visibility.Collapsed;
                if (MenuSaoChepO != null)
                {
                    MenuSaoChepO.Visibility = Visibility.Visible;
                    MenuSaoChepO.Header = "Sao chép";
                }
                if (MenuSaoChepVungChon != null) MenuSaoChepVungChon.Visibility = Visibility.Collapsed;
                if (SepKhac != null) SepKhac.Visibility = Visibility.Visible;
                if (MenuXoa != null)
                {
                    MenuXoa.Visibility = Visibility.Visible;
                    MenuXoa.Header = "Xóa vĩnh viễn";
                }
                if (MenuTuDongGianCot != null) MenuTuDongGianCot.Visibility = Visibility.Collapsed;
                if (MenuCotHienThi != null) MenuCotHienThi.Visibility = Visibility.Collapsed;
                if (MenuThuocTinh != null) MenuThuocTinh.Visibility = Visibility.Visible;
                return;
            }

            if (MenuKhoiPhuc != null) MenuKhoiPhuc.Visibility = Visibility.Collapsed;
            if (MenuThemMoi != null) MenuThemMoi.Visibility = Visibility.Visible;
            if (MenuThemNhanhExcel != null) MenuThemNhanhExcel.Visibility = Visibility.Visible;
            if (MenuCapNhatNhanhExcel != null) MenuCapNhatNhanhExcel.Visibility = Visibility.Visible;
            if (MenuChinhSua != null) MenuChinhSua.Visibility = Visibility.Visible;
            if (SepDatLoc != null) SepDatLoc.Visibility = Visibility.Visible;
            if (MenuDatCot != null) MenuDatCot.Visibility = Visibility.Visible;
            if (MenuLocCot != null) MenuLocCot.Visibility = Visibility.Visible;
            if (SepSapXep != null) SepSapXep.Visibility = Visibility.Visible;
            if (MenuSapXep != null)
            {
                MenuSapXep.Visibility = Visibility.Visible;
                MenuSapXep.Header = "Sắp xếp theo";
            }
            if (MenuRefresh != null) MenuRefresh.Visibility = Visibility.Visible;
            if (MenuInDanhSach != null) MenuInDanhSach.Visibility = Visibility.Visible;
            if (SepSaoChep != null) SepSaoChep.Visibility = Visibility.Visible;
            if (MenuSaoChepO != null)
            {
                MenuSaoChepO.Visibility = Visibility.Visible;
                MenuSaoChepO.Header = "Sao chép ô";
            }
            if (MenuSaoChepVungChon != null) MenuSaoChepVungChon.Visibility = Visibility.Visible;
            if (SepKhac != null) SepKhac.Visibility = Visibility.Visible;
            if (MenuXoa != null)
            {
                MenuXoa.Visibility = Visibility.Visible;
                MenuXoa.Header = "Xóa";
            }
            if (MenuTuDongGianCot != null) MenuTuDongGianCot.Visibility = Visibility.Visible;
            if (MenuCotHienThi != null) MenuCotHienThi.Visibility = Visibility.Visible;
            if (MenuThuocTinh != null) MenuThuocTinh.Visibility = Visibility.Visible;

            string colHeader = _clickedColumn?.Header?.ToString() ?? "Tên đợt khuyến mại";
            MenuDatCot.Header = $"Đặt {colHeader}";
            MenuLocCot.Header = $"Lọc {colHeader}";
        }

        private async void MenuDatCot_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgDotKhuyenMai.SelectedItems.Cast<DotKhuyenMaiViewModel>().ToList();
            if (selectedList.Count == 0 && DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel single) selectedList.Add(single);
            if (selectedList.Count == 0) return;

            string colHeader = _clickedColumn?.Header?.ToString() ?? "Tên đợt khuyến mại";
            string currentVal = _clickedCellValue ?? "";

            var inputWin = new InputWindow($"Đặt {colHeader}", $"Nhập giá trị mới cho '{colHeader}':", currentVal);
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string newVal = inputWin.InputText?.Trim() ?? "";
                string dbCol = "NAME";

                if (colHeader == "Tên đợt khuyến mại") dbCol = "NAME";
                else if (colHeader == "Ghi chú") dbCol = "NOTE";
                else if (colHeader == "Tỉ lệ giảm giá") dbCol = "TILEGIAMGIA";

                var ids = selectedList.Select(k => k.Id).ToList();
                if (await LocalKhuyenMaiService.UpdateDotKhuyenMaiColumnAsync(ids, dbCol, newVal))
                {
                    await RefreshGridAsync();
                }
            }
        }

        private void MenuLocCot_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                TxtLoc.Text = _clickedCellValue;
            }
        }

        private async void MenuItem_KhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel selected)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn khôi phục đợt khuyến mại '{selected.Name}' không?", "Khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    if (await LocalKhuyenMaiService.RestoreDotKhuyenMaiAsync(selected.Id))
                    {
                        await RefreshGridAsync();
                    }
                }
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Name).ToList();
            RebindGrid();
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderByDescending(k => k.Name).ToList();
            RebindGrid();
        }

        private void MenuItem_SortByName_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Name).ToList();
            RebindGrid();
        }

        private void MenuItem_SortByLoaiHinh_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.TenLoaiHinhKhuyenMai).ToList();
            RebindGrid();
        }

        private void MenuItem_SortByTuNgay_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Tungay).ToList();
            RebindGrid();
        }

        private void MenuItem_SortByDenNgay_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Denngay).ToList();
            RebindGrid();
        }

        private void RebindGrid()
        {
            int stt = 1;
            foreach (var item in _rawList) item.Stt = stt++;
            DgDotKhuyenMai.ItemsSource = null;
            DgDotKhuyenMai.ItemsSource = _rawList;
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue)) Clipboard.SetText(_clickedCellValue);
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgDotKhuyenMai.SelectedItems.Cast<DotKhuyenMaiViewModel>().ToList();
            if (selectedList.Count == 0 && DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel single) selectedList.Add(single);
            if (selectedList.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            foreach (var item in selectedList)
            {
                sb.AppendLine($"{item.Name}\t{item.TenLoaiHinhKhuyenMai}\t{item.Tungay:dd/MM/yyyy}\t{item.Denngay:dd/MM/yyyy}\t{item.IsNgungApDung}\t{item.Tilegiamgia}");
            }
            Clipboard.SetText(sb.ToString());
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgDotKhuyenMai.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "Tên đợt khuyến mại", "Loại hình khuyến mại", "Từ ngày", "Đến ngày", "Ngừng áp dụng", "Ghi chú", "Tỉ lệ giảm giá" };
            var win = new ChonCotHienThiWindow(DgDotKhuyenMai, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (DgDotKhuyenMai.SelectedItem is DotKhuyenMaiViewModel selected)
            {
                MessageBox.Show($"Đợt khuyến mại: {selected.Name}\nLoại hình: {selected.TenLoaiHinhKhuyenMai}\nThời gian: {selected.Tungay:dd/MM/yyyy} - {selected.Denngay:dd/MM/yyyy}", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
