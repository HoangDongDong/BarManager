using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucMatHangControl : UserControl
    {
        private readonly LocalMatHangService _matHangService;
        private ObservableCollection<MatHangInMaVach> _matHangInMaVachList;

        public DanhMucMatHangControl()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _matHangInMaVachList = new ObservableCollection<MatHangInMaVach>();
            DgInMaVach.ItemsSource = _matHangInMaVachList;
        }

        private System.Collections.Generic.List<MatHangViewModel> _allMatHangs;
        private string _currentNhomId = null;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load danh sách nhóm mặt hàng lên TreeView
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
            
            // Load toàn bộ mặt hàng mặc định (ID = 0 hoặc null)
            LoadMatHangData(null);
        }

        private void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomMatHangViewModel selectedNhom)
            {
                bool isTrash = selectedNhom.Id == "-1";
                if (BtnThemMoi != null) BtnThemMoi.IsEnabled = !isTrash;
                if (BtnChinhSua != null) BtnChinhSua.IsEnabled = !isTrash;
                if (BtnThemExcel != null) BtnThemExcel.IsEnabled = !isTrash;
                if (BtnImportDinhLuong != null) BtnImportDinhLuong.IsEnabled = !isTrash;
                if (BtnXoa != null) BtnXoa.Content = isTrash ? "❌ Xóa vĩnh viễn" : "❌ Xóa (Del)";

                // Nếu chọn "Tất cả" (Id = string.Empty) thì truyền null để lấy hết
                string filterId = string.IsNullOrEmpty(selectedNhom.Id) ? null : selectedNhom.Id;
                LoadMatHangData(filterId);
            }
        }

        private async void LoadMatHangData(string nhomId)
        {
            _currentNhomId = nhomId;
            _allMatHangs = await _matHangService.GetMatHangListAsync(nhomId);
            ApplyFilter();
        }

        private string _currentSortColumn = "Name";
        private bool _isSortAscending = true;

        private void ApplyFilter()
        {
            if (_allMatHangs == null)
            {
                DgMatHang.ItemsSource = null;
                return;
            }

            var query = _allMatHangs.AsEnumerable();
            string filter = TxtLocMatHang?.Text?.Trim();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(m => 
                    (!string.IsNullOrEmpty(m.Name) && m.Name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(m.Code) && m.Code.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(m.NhomMatHangName) && m.NhomMatHangName.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(m.LoaiMatHangName) && m.LoaiMatHangName.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // Sắp xếp
            query = _currentSortColumn switch
            {
                "Nhom" => _isSortAscending ? query.OrderBy(m => m.NhomMatHangName) : query.OrderByDescending(m => m.NhomMatHangName),
                "Loai" => _isSortAscending ? query.OrderBy(m => m.LoaiMatHangName) : query.OrderByDescending(m => m.LoaiMatHangName),
                "Dvt" => _isSortAscending ? query.OrderBy(m => m.DonViTinhName) : query.OrderByDescending(m => m.DonViTinhName),
                "GiaBan" => _isSortAscending ? query.OrderBy(m => m.Giaban) : query.OrderByDescending(m => m.Giaban),
                "GiaNhap" => _isSortAscending ? query.OrderBy(m => m.Gianhap) : query.OrderByDescending(m => m.Gianhap),
                "DvtChan" => _isSortAscending ? query.OrderBy(m => m.DonViTinhChanName) : query.OrderByDescending(m => m.DonViTinhChanName),
                "QuyDoi" => _isSortAscending ? query.OrderBy(m => m.Quydoi) : query.OrderByDescending(m => m.Quydoi),
                "GiaBanChan" => _isSortAscending ? query.OrderBy(m => m.Giabanchan) : query.OrderByDescending(m => m.Giabanchan),
                "Code" => _isSortAscending ? query.OrderBy(m => m.Code) : query.OrderByDescending(m => m.Code),
                _ => _isSortAscending ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name)
            };

            var list = query.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Stt = i + 1;
            }

            DgMatHang.ItemsSource = list;
        }

        private void TxtLocMatHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
                row.Focus();
            }
        }

        private void DgMatHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selected)
            {
                if (TxtInfoKhoiTao != null)
                    TxtInfoKhoiTao.Text = selected.Timecreated?.ToString("dd/MM/yyyy hh:mm tt") ?? "--/--/---- --:--:--";
                if (TxtInfoNguoiTao != null)
                    TxtInfoNguoiTao.Text = !string.IsNullOrEmpty(selected.UsercreatedName) ? selected.UsercreatedName : "Administrator";
                if (TxtInfoSuaDoi != null)
                    TxtInfoSuaDoi.Text = selected.Timemodified?.ToString("dd/MM/yyyy hh:mm tt") ?? "--/--/---- --:--:--";
                if (TxtInfoNguoiSua != null)
                    TxtInfoNguoiSua.Text = !string.IsNullOrEmpty(selected.UsermodifiedName) ? selected.UsermodifiedName : "Administrator";
            }
            else
            {
                if (TxtInfoKhoiTao != null) TxtInfoKhoiTao.Text = "--/--/---- --:--:--";
                if (TxtInfoNguoiTao != null) TxtInfoNguoiTao.Text = "Administrator";
                if (TxtInfoSuaDoi != null) TxtInfoSuaDoi.Text = "--/--/---- --:--:--";
                if (TxtInfoNguoiSua != null) TxtInfoNguoiSua.Text = "Administrator";
            }
        }

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool isTrash = _currentNhomId == "-1";
            if (isTrash)
            {
                if (MenuKhoiPhuc != null) MenuKhoiPhuc.Visibility = Visibility.Visible;
                if (MenuThemMoi != null) MenuThemMoi.Visibility = Visibility.Collapsed;
                if (MenuThemNhanhExcel != null) MenuThemNhanhExcel.Visibility = Visibility.Collapsed;
                if (MenuCapNhatNhanhExcel != null) MenuCapNhatNhanhExcel.Visibility = Visibility.Collapsed;
                if (MenuChinhSua != null) MenuChinhSua.Visibility = Visibility.Collapsed;
                if (Sep1 != null) Sep1.Visibility = Visibility.Collapsed;
                if (MenuDanhLaiMaHang != null) MenuDanhLaiMaHang.Visibility = Visibility.Collapsed;
                if (MenuDatCot != null) MenuDatCot.Visibility = Visibility.Collapsed;
                if (MenuLocCot != null) MenuLocCot.Visibility = Visibility.Collapsed;
                if (Sep2 != null) Sep2.Visibility = Visibility.Collapsed;
                if (MenuInDanhSach != null) MenuInDanhSach.Visibility = Visibility.Collapsed;
                if (Sep3 != null) Sep3.Visibility = Visibility.Collapsed;
                if (MenuSaoChepVungChon != null) MenuSaoChepVungChon.Visibility = Visibility.Collapsed;
                if (Sep5 != null) Sep5.Visibility = Visibility.Collapsed;
                if (MenuTuDongGianCot != null) MenuTuDongGianCot.Visibility = Visibility.Collapsed;
                if (MenuCotHienThi != null) MenuCotHienThi.Visibility = Visibility.Collapsed;
                if (MenuXoa != null) MenuXoa.Header = "Xóa vĩnh viễn";
                if (MenuSapXep != null) MenuSapXep.Header = "Sắp xếp";
            }
            else
            {
                if (MenuKhoiPhuc != null) MenuKhoiPhuc.Visibility = Visibility.Collapsed;
                if (MenuThemMoi != null) MenuThemMoi.Visibility = Visibility.Visible;
                if (MenuThemNhanhExcel != null) MenuThemNhanhExcel.Visibility = Visibility.Visible;
                if (MenuCapNhatNhanhExcel != null) MenuCapNhatNhanhExcel.Visibility = Visibility.Visible;
                if (MenuChinhSua != null) MenuChinhSua.Visibility = Visibility.Visible;
                if (Sep1 != null) Sep1.Visibility = Visibility.Visible;
                if (MenuDanhLaiMaHang != null) MenuDanhLaiMaHang.Visibility = Visibility.Visible;
                if (MenuDatCot != null) MenuDatCot.Visibility = Visibility.Visible;
                if (MenuLocCot != null) MenuLocCot.Visibility = Visibility.Visible;
                if (Sep2 != null) Sep2.Visibility = Visibility.Visible;
                if (MenuInDanhSach != null) MenuInDanhSach.Visibility = Visibility.Visible;
                if (Sep3 != null) Sep3.Visibility = Visibility.Visible;
                if (MenuSaoChepVungChon != null) MenuSaoChepVungChon.Visibility = Visibility.Visible;
                if (Sep5 != null) Sep5.Visibility = Visibility.Visible;
                if (MenuTuDongGianCot != null) MenuTuDongGianCot.Visibility = Visibility.Visible;
                if (MenuCotHienThi != null) MenuCotHienThi.Visibility = Visibility.Visible;
                if (MenuXoa != null) MenuXoa.Header = "Xóa";
                if (MenuSapXep != null) MenuSapXep.Header = "Sắp xếp theo";
            }
        }

        private async void MenuItem_KhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selected)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn khôi phục mặt hàng '{selected.Name}' không?", "Xác nhận khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await _matHangService.RestoreMatHangAsync(selected.Id);
                    ReloadAllData();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuItem_SaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selected)
            {
                Clipboard.SetText(selected.Name ?? "");
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgMatHang.SelectedItems.Cast<MatHangViewModel>().ToList();
            if (selectedList.Count == 0 && DgMatHang.SelectedItem is MatHangViewModel single) selectedList.Add(single);
            if (selectedList.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("STT\tTên mặt hàng\tNhóm mặt hàng\tLoại mặt hàng\tĐVT\tGiá bán\tGiá nhập\tMã hàng");
                foreach (var m in selectedList)
                {
                    sb.AppendLine($"{m.Stt}\t{m.Name}\t{m.NhomMatHangName}\t{m.LoaiMatHangName}\t{m.DonViTinhName}\t{m.Giaban}\t{m.Gianhap}\t{m.Code}");
                }
                Clipboard.SetText(sb.ToString());
                MessageBox.Show($"Đã sao chép {selectedList.Count} dòng vào Clipboard.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e) { _isSortAscending = true; ApplyFilter(); }
        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e) { _isSortAscending = false; ApplyFilter(); }
        private void MenuItem_SortByName_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "Name"; ApplyFilter(); }
        private void MenuItem_SortByNhom_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "Nhom"; ApplyFilter(); }
        private void MenuItem_SortByLoai_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "Loai"; ApplyFilter(); }
        private void MenuItem_SortByDvt_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "Dvt"; ApplyFilter(); }
        private void MenuItem_SortByGiaBan_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "GiaBan"; ApplyFilter(); }
        private void MenuItem_SortByGiaNhap_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "GiaNhap"; ApplyFilter(); }
        private void MenuItem_SortByDvtChan_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "DvtChan"; ApplyFilter(); }
        private void MenuItem_SortByQuyDoi_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "QuyDoi"; ApplyFilter(); }
        private void MenuItem_SortByGiaBanChan_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "GiaBanChan"; ApplyFilter(); }
        private void MenuItem_SortByCode_Click(object sender, RoutedEventArgs e) { _currentSortColumn = "Code"; ApplyFilter(); }

        private void MenuDanhLaiMaHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đánh lại mã hàng tự động.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgMatHang.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tùy chọn ẩn/hiển thị cột.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selected)
            {
                var win = new ThuocTinhWindow(selected.Id, "DMATHANG", selected.Name, selected.Timecreated, selected.Timemodified, selected.UsercreatedName, selected.UsermodifiedName);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để xem thuộc tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var chonDuLieuWin = new ChonDuLieuWindow();
            if (chonDuLieuWin.ShowDialog() == true)
            {
                var selectedNhom = chonDuLieuWin.SelectedNhomMatHang;
                string nhomId = selectedNhom != null ? selectedNhom.Id : null;

                var list = DgMatHang.ItemsSource as System.Collections.Generic.List<MatHangViewModel>;
                var selectedMatHang = DgMatHang.SelectedItem as MatHangViewModel;
                int initialIndex = selectedMatHang != null && list != null ? list.IndexOf(selectedMatHang) : (list != null && list.Count > 0 ? 0 : -1);

                var themMoiWin = new ThemMoiMatHangWindow(nhomId, null, list, initialIndex, ReloadMatHangGrid);
                themMoiWin.ShowDialog();
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selectedMatHang)
            {
                var list = DgMatHang.ItemsSource as System.Collections.Generic.List<MatHangViewModel>;
                int initialIndex = list != null ? list.IndexOf(selectedMatHang) : -1;
                var themMoiWin = new ThemMoiMatHangWindow(selectedMatHang.DnhommathangId, selectedMatHang.Id, list, initialIndex, ReloadMatHangGrid);
                themMoiWin.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            bool isTrash = _currentNhomId == "-1";

            if (DgMatHang.SelectedItem is MatHangViewModel selectedMatHang)
            {
                string msg = isTrash 
                    ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN mặt hàng '{selectedMatHang.Name}' không?"
                    : $"Bạn có chắc muốn đưa mặt hàng '{selectedMatHang.Name}' vào Thùng rác không?";

                var result = MessageBox.Show(msg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _matHangService.DeleteMatHangAsync(selectedMatHang.Id, isPermanent: isTrash);
                    if (success)
                    {
                        ReloadMatHangGrid();
                    }
                }
            }
            else if (isTrash)
            {
                var confirmEmpty = MessageBox.Show("Bạn có chắc chắn muốn DỌN SẠCH THÙNG RÁC (Xóa vĩnh viễn toàn bộ mặt hàng trong thùng rác)?", "Dọn sạch thùng rác", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmEmpty == MessageBoxResult.Yes)
                {
                    await _matHangService.EmptyTrashAsync();
                    ReloadMatHangGrid();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ReloadMatHangGrid()
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel currentNhom)
            {
                LoadMatHangData(string.IsNullOrEmpty(currentNhom.Id) ? null : currentNhom.Id);
            }
            else
            {
                LoadMatHangData(null);
            }
        }

        private async void ReloadAllData()
        {
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
            ReloadMatHangGrid();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            var items = DgMatHang.ItemsSource as System.Collections.Generic.IEnumerable<MatHangViewModel>;
            if (items == null || !System.Linq.Enumerable.Any(items))
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachMatHang.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("MatHang");
                        
                        // Header
                        worksheet.Cell(1, 1).Value = "STT";
                        worksheet.Cell(1, 2).Value = "Tên mặt hàng";
                        worksheet.Cell(1, 3).Value = "Nhóm mặt hàng";
                        worksheet.Cell(1, 4).Value = "Loại mặt hàng";
                        worksheet.Cell(1, 5).Value = "Đơn vị tính";
                        worksheet.Cell(1, 6).Value = "Giá bán";
                        worksheet.Cell(1, 7).Value = "Giá nhập";
                        worksheet.Cell(1, 8).Value = "ĐVT chẵn";
                        worksheet.Cell(1, 9).Value = "Quy đổi";
                        worksheet.Cell(1, 10).Value = "Giá bán chẵn";
                        worksheet.Cell(1, 11).Value = "Mã hàng";
                        worksheet.Cell(1, 12).Value = "Tạm khóa";
                        worksheet.Cell(1, 13).Value = "Giá theo thời giá";
                        
                        // Format header
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int row = 2;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = item.Stt;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.NhomMatHangName;
                            worksheet.Cell(row, 4).Value = item.LoaiMatHangName;
                            worksheet.Cell(row, 5).Value = item.DonViTinhName;
                            worksheet.Cell(row, 6).Value = item.Giaban;
                            worksheet.Cell(row, 7).Value = item.Gianhap;
                            worksheet.Cell(row, 8).Value = item.DonViTinhChanName;
                            worksheet.Cell(row, 9).Value = item.Quydoi;
                            worksheet.Cell(row, 10).Value = item.Giabanchan;
                            worksheet.Cell(row, 11).Value = item.Code;
                            worksheet.Cell(row, 12).Value = item.Tamkhoa;
                            worksheet.Cell(row, 13).Value = item.Giatheothoigia;
                            row++;
                        }
                        
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    var result = MessageBox.Show("Xuất Excel thành công! Bạn có muốn mở file vừa xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhWindow(ReloadAllData);
            win.ShowDialog();
        }

        private void BtnImportDinhLuong_Click(object sender, RoutedEventArgs e)
        {
            var win = new ImportDinhLuongWindow();
            win.ShowDialog();
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgMatHang);
            win.ShowDialog();
        }

        private async void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            var selectedNhom = TvNhomMatHang.SelectedItem as NhomMatHangViewModel;
            if (selectedNhom == null)
            {
                MessageBox.Show("Vui lòng chọn một thư mục hoặc nhóm mặt hàng để thêm nhóm con!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedNhom.Id))
            {
                MessageBox.Show("Không thể thêm con vào nút gốc này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int index = 1;
            string defaultName = $"Thư mục {index}";
            while (selectedNhom.Children != null && selectedNhom.Children.Any(x => x.Name == defaultName))
            {
                index++;
                defaultName = $"Thư mục {index}";
            }

            var newNhom = new DNHOMMATHANG
            {
                Id = Guid.NewGuid().ToString(),
                Name = defaultName,
                Code = "TMP",
                ParentId = selectedNhom.Id,
                Timecreated = DateTime.Now
            };

            bool success = await _matHangService.InsertNhomMatHangAsync(newNhom);
            if (success)
            {
                await ReloadTreeViewAsync();
                var items = TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>;
                var node = FindNode(items, selectedNhom.Id);
                if (node != null)
                {
                    var newItem = node.Children.FirstOrDefault(x => x.Name == defaultName);
                    if (newItem != null)
                    {
                        if (TvNhomMatHang.ItemContainerGenerator.ContainerFromItem(node) is TreeViewItem tvi)
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

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var items = TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>;
            if (items == null) return;
            
            // Lấy danh mục gốc "Tất cả" (parent = null)
            var rootNode = items.FirstOrDefault();
            if (rootNode == null) return;

            int index = 1;
            string defaultName = $"Thư mục {index}";
            while (rootNode.Children.Any(x => x.Name == defaultName))
            {
                index++;
                defaultName = $"Thư mục {index}";
            }

            var newNhom = new DNHOMMATHANG
            {
                Id = Guid.NewGuid().ToString(),
                Name = defaultName,
                Code = "TMP",
                ParentId = null,
                Timecreated = DateTime.Now
            };

            bool success = await _matHangService.InsertNhomMatHangAsync(newNhom);
            if (success)
            {
                await ReloadTreeViewAsync();
                items = TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>;
                rootNode = items.FirstOrDefault();
                if (rootNode != null)
                {
                    var newItem = rootNode.Children.FirstOrDefault(x => x.Name == defaultName);
                    if (newItem != null)
                    {
                        if (TvNhomMatHang.ItemContainerGenerator.ContainerFromItem(rootNode) is TreeViewItem tvi)
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

        private NhomMatHangViewModel FindNode(System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel> nodes, string id)
        {
            if (nodes == null) return null;
            foreach (var node in nodes)
            {
                if (node.Id == id) return node;
                var child = FindNode(node.Children, id);
                if (child != null) return child;
            }
            return null;
        }

        private bool IsDuplicateName(System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel> tree, string name, string excludeId)
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

        private async void InlineEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt)
            {
                string id = txt.Tag as string;
                if (!string.IsNullOrEmpty(id) && txt.DataContext is NhomMatHangViewModel model && model.IsEditing)
                {
                    if (string.IsNullOrWhiteSpace(model.Name))
                    {
                        model.IsEditing = false;
                        await ReloadTreeViewAsync();
                        return;
                    }

                    if (IsDuplicateName(TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>, model.Name, id))
                    {
                        MessageBox.Show($"Tên '{model.Name}' đã tồn tại. Hệ thống sẽ khôi phục tên cũ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        model.IsEditing = false;
                        await ReloadTreeViewAsync();
                        return;
                    }

                    model.IsEditing = false;
                    await UpdateGroupName(id, model.Name);
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
                    if (!string.IsNullOrEmpty(id) && txt.DataContext is NhomMatHangViewModel model && model.IsEditing)
                    {
                        if (string.IsNullOrWhiteSpace(model.Name))
                        {
                            model.IsEditing = false;
                            await ReloadTreeViewAsync();
                            e.Handled = true;
                            return;
                        }

                        if (IsDuplicateName(TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>, model.Name, id))
                        {
                            MessageBox.Show($"Tên '{model.Name}' đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Handled = true;
                            return;
                        }

                        model.IsEditing = false;
                        await UpdateGroupName(id, model.Name);
                    }
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    if (txt.DataContext is NhomMatHangViewModel model && model.IsEditing)
                    {
                        model.IsEditing = false;
                        await ReloadTreeViewAsync();
                    }
                    e.Handled = true;
                }
            }
        }

        private async System.Threading.Tasks.Task UpdateGroupName(string id, string newName)
        {
            // Cần truy vấn group từ DB và update
            var groups = await _matHangService.GetAllNhomMatHangAsync();
            var group = groups.FirstOrDefault(g => g.Id == id);
            if (group != null)
            {
                group.Name = newName;
                await _matHangService.UpdateNhomMatHangAsync(group);
            }
        }

        private void MenuItem_ThemMoi_Click(object sender, RoutedEventArgs e)
        {
            BtnThemThuMuc_Click(null, null);
        }

        private void MenuItem_ThemMoiCon_Click(object sender, RoutedEventArgs e)
        {
            BtnThemNhom_Click(null, null);
        }

        private void MenuItem_SuaDoi_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel selected)
            {
                if (string.IsNullOrEmpty(selected.Id))
                {
                    MessageBox.Show("Không thể sửa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                selected.IsEditing = true;
            }
        }

        private async void MenuItem_Xoa_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel selected)
            {
                if (string.IsNullOrEmpty(selected.Id))
                {
                    MessageBox.Show("Không thể xóa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{selected.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await _matHangService.DeleteNhomMatHangAsync(selected.Id);
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await ReloadTreeViewAsync();
        }

        private async System.Threading.Tasks.Task ReloadTreeViewAsync()
        {
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
        }

        private void BtnInMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (PanelInMaVach.Visibility == Visibility.Visible)
            {
                PanelInMaVach.Visibility = Visibility.Collapsed;
                GsInMaVach.Visibility = Visibility.Collapsed;
                ColInMaVach.Width = new GridLength(0);
            }
            else
            {
                PanelInMaVach.Visibility = Visibility.Visible;
                GsInMaVach.Visibility = Visibility.Visible;
                ColInMaVach.Width = new GridLength(350); // Mở rộng cột bên phải
            }
        }

        private void BtnThemMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItems.Count > 0)
            {
                bool hasExisting = false;
                foreach (MatHangViewModel item in DgMatHang.SelectedItems)
                {
                    // Check if already exists by Id
                    var existing = _matHangInMaVachList.FirstOrDefault(x => x.Id == item.Id);
                    if (existing != null)
                    {
                        hasExisting = true;
                    }
                    else
                    {
                        _matHangInMaVachList.Add(new MatHangInMaVach
                        {
                            Id = item.Id,
                            Code = item.Code,
                            Name = item.Name,
                            Quantity = 1
                        });
                    }
                }
                
                if (hasExisting)
                {
                    MessageBox.Show("Các mặt hàng đang chọn đã nằm trong danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn mặt hàng để thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXoaMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (DgInMaVach.SelectedItems.Count > 0)
            {
                var itemsToRemove = DgInMaVach.SelectedItems.Cast<MatHangInMaVach>().ToList();
                foreach (var item in itemsToRemove)
                {
                    _matHangInMaVachList.Remove(item);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn mặt hàng để xóa khỏi danh sách in mã vạch!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXuatExcelMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (_matHangInMaVachList == null || _matHangInMaVachList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachInMaVach.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("InMaVach");
                        
                        // Header
                        worksheet.Cell(1, 1).Value = "Mã";
                        worksheet.Cell(1, 2).Value = "Tên";
                        worksheet.Cell(1, 3).Value = "Số lượng";
                        
                        // Format header
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int row = 2;
                        foreach (var item in _matHangInMaVachList)
                        {
                            worksheet.Cell(row, 1).Value = item.Code;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.Quantity;
                            row++;
                        }
                        
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    var result = MessageBox.Show("Xuất Excel thành công! Bạn có muốn mở file vừa xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnInBarcode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng in mã vạch đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBartender_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng in Bartender đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
