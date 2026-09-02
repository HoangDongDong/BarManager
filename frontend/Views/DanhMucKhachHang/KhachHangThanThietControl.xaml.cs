using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class KhachHangThanThietControl : UserControl
    {
        private ObservableCollection<KhachHangThanThietViewModel> _customers = new ObservableCollection<KhachHangThanThietViewModel>();
        private ObservableCollection<TangGiamDiemItem> _tangGiamDiemList = new ObservableCollection<TangGiamDiemItem>();
        private ObservableCollection<DiemTheoHoaDonItem> _diemHoaDonList = new ObservableCollection<DiemTheoHoaDonItem>();

        private string _currentNhomId = "ALL";
        private NhomKhachHangTreeItem _selectedNhom = null;
        private KhachHangThanThietViewModel _selectedCustomer = null;

        private DataGridCell _clickedCell;
        private DataGridColumn _clickedColumn;
        private string _clickedCellValue;

        public KhachHangThanThietControl()
        {
            InitializeComponent();
            DgKhachHang.ItemsSource = _customers;
            DgTangGiamDiem.ItemsSource = _tangGiamDiemList;
            DgDiemHoaDon.ItemsSource = _diemHoaDonList;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomAsync();
            await LoadKhachHangListAsync();
        }

        private async Task LoadTreeNhomAsync()
        {
            try
            {
                var tree = await LocalKhachHangService.GetNhomKhachHangTreeAsync();
                TvNhomKhachHang.ItemsSource = tree;

                if (tree != null && tree.Count > 0)
                {
                    var allNode = tree[0];
                    allNode.IsExpanded = true;
                    allNode.IsSelected = true;
                    _selectedNhom = allNode;
                    _currentNhomId = allNode.Id;

                    foreach (var c in allNode.Children)
                    {
                        c.IsExpanded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTreeNhomAsync: " + ex.Message);
            }
        }

        private async Task LoadKhachHangListAsync()
        {
            try
            {
                string kw = TxtLoc.Text?.Trim() ?? "";
                var data = await LocalKhachHangThanThietService.GetKhachHangThanThietListAsync(_currentNhomId, kw);

                _customers.Clear();
                foreach (var item in data)
                {
                    _customers.Add(item);
                }

                if (_customers.Count > 0)
                {
                    if (_selectedCustomer != null)
                    {
                        var found = _customers.FirstOrDefault(x => x.Id == _selectedCustomer.Id);
                        DgKhachHang.SelectedItem = found ?? _customers[0];
                    }
                    else
                    {
                        DgKhachHang.SelectedIndex = 0;
                    }

                    _selectedCustomer = DgKhachHang.SelectedItem as KhachHangThanThietViewModel;
                    await LoadCustomerDetailsAsync();
                }
                else
                {
                    _selectedCustomer = null;
                    _tangGiamDiemList.Clear();
                    _diemHoaDonList.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TvNhomKhachHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem node)
            {
                _selectedNhom = node;
                _currentNhomId = node.Id;
                await LoadKhachHangListAsync();
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadKhachHangListAsync();
        }

        private async void TxtLoc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await LoadKhachHangListAsync();
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadKhachHangListAsync();
        }

        private async void DgKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = DgKhachHang.SelectedItem as KhachHangThanThietViewModel;
            await LoadCustomerDetailsAsync();
        }

        private async Task LoadCustomerDetailsAsync()
        {
            _tangGiamDiemList.Clear();
            _diemHoaDonList.Clear();

            if (_selectedCustomer == null || string.IsNullOrEmpty(_selectedCustomer.Id))
                return;

            try
            {
                var tangDiemData = await LocalKhachHangThanThietService.GetLichSuTangGiamDiemAsync(_selectedCustomer.Id);
                foreach (var item in tangDiemData)
                {
                    _tangGiamDiemList.Add(item);
                }

                var diemHdData = await LocalKhachHangThanThietService.GetLichSuDiemTheoHoaDonAsync(_selectedCustomer.Id);
                foreach (var item in diemHdData)
                {
                    _diemHoaDonList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCustomerDetailsAsync: " + ex.Message);
            }
        }

        private async void BtnThemTangDiem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn một khách hàng từ danh sách trước khi thêm phiếu tặng điểm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new ThemTangDiemWindow(_selectedCustomer);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await RefreshSelectedCustomerPointsAsync();
                await LoadCustomerDetailsAsync();
            };
            win.ShowDialog();
            await RefreshSelectedCustomerPointsAsync();
            await LoadCustomerDetailsAsync();
        }

        private void BtnSuaTangDiem_Click(object sender, RoutedEventArgs e)
        {
            OpenEditTangDiem();
        }

        private void DgTangGiamDiem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenEditTangDiem();
        }

        private async void OpenEditTangDiem()
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = DgTangGiamDiem.SelectedItem as TangGiamDiemItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu tặng điểm cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new ThemTangDiemWindow(_selectedCustomer, selectedItem);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await RefreshSelectedCustomerPointsAsync();
                await LoadCustomerDetailsAsync();
            };
            win.ShowDialog();
            await RefreshSelectedCustomerPointsAsync();
            await LoadCustomerDetailsAsync();
        }

        private async void BtnXoaTangDiem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DgTangGiamDiem.SelectedItem as TangGiamDiemItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dr = MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu '{selectedItem.SoPhieu}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dr == MessageBoxResult.Yes)
            {
                bool ok = await LocalKhachHangThanThietService.DeleteTangGiamDiemAsync(selectedItem.Id);
                if (ok)
                {
                    await RefreshSelectedCustomerPointsAsync();
                    await LoadCustomerDetailsAsync();
                    MessageBox.Show("Đã xóa phiếu tặng điểm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa phiếu. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task RefreshSelectedCustomerPointsAsync()
        {
            if (_selectedCustomer == null) return;

            try
            {
                var freshList = await LocalKhachHangThanThietService.GetKhachHangThanThietListAsync(_currentNhomId, TxtLoc.Text?.Trim() ?? "");
                var updated = freshList.FirstOrDefault(x => x.Id == _selectedCustomer.Id);
                if (updated != null)
                {
                    _selectedCustomer.DiemTichLuy = updated.DiemTichLuy;
                    _selectedCustomer.DoanhSo = updated.DoanhSo;
                    _selectedCustomer.SoHoaDon = updated.SoHoaDon;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RefreshSelectedCustomerPointsAsync: " + ex.Message);
            }
        }

        #region DataGrid ContextMenu Handlers
        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    DgKhachHang.SelectedItems.Clear();
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

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                Clipboard.SetText(_clickedCellValue);
            }
            else if (_selectedCustomer != null)
            {
                Clipboard.SetText(_selectedCustomer.Name ?? "");
            }
        }

        private void MenuItem_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgKhachHang.SelectedItems.Cast<KhachHangThanThietViewModel>().ToList();
            if (selectedList.Count == 0 && _selectedCustomer != null)
            {
                selectedList.Add(_selectedCustomer);
            }

            if (selectedList.Count == 0) return;

            var sb = new StringBuilder();
            foreach (var item in selectedList)
            {
                sb.AppendLine($"{item.Name}\t{item.Diachi}\t{item.Dienthoai}\t{item.Email}\t{item.DoanhSo:N0}\t{item.SoHoaDon}\t{item.DiemTichLuy:N2}\t{item.DiemTichLuyBanDau:N2}\t{item.Note}");
            }

            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgKhachHang.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email", "Doanh số", "Số hóa đơn", "Điểm tích lũy", "Điểm tích lũy ban đầu", "Ghi chú" };
            var win = new ChonCotHienThiWindow(DgKhachHang, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuItem_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_customers == null || _customers.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"KhachHangThanThiet_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Khách hàng thân thiết");

                        string[] headers = new[]
                        {
                            "STT", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email",
                            "Doanh số", "Số hóa đơn", "Điểm tích lũy", "Điểm tích lũy ban đầu", "Ghi chú"
                        };

                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = worksheet.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3f66");
                            cell.Style.Font.FontColor = XLColor.White;
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }

                        int row = 2;
                        int stt = 1;
                        foreach (var item in _customers)
                        {
                            worksheet.Cell(row, 1).Value = stt++;
                            worksheet.Cell(row, 2).Value = item.Name ?? "";
                            worksheet.Cell(row, 3).Value = item.Diachi ?? "";
                            worksheet.Cell(row, 4).Value = item.Dienthoai ?? "";
                            worksheet.Cell(row, 5).Value = item.Email ?? "";
                            worksheet.Cell(row, 6).Value = item.DoanhSo;
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 7).Value = item.SoHoaDon;
                            worksheet.Cell(row, 8).Value = item.DiemTichLuy;
                            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.##";
                            worksheet.Cell(row, 9).Value = item.DiemTichLuyBanDau;
                            worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0.##";
                            worksheet.Cell(row, 10).Value = item.Note ?? "";

                            row++;
                        }

                        var dataRange = worksheet.Range(1, 1, row - 1, headers.Length);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show($"Đã xuất thành công {_customers.Count} khách hàng ra file Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgKhachHang, "Khách hàng thân thiết");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }
        #endregion

        #region TreeView ContextMenu Handlers
        private void TvNhomKhachHang_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is TreeViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is TreeViewItem tvi)
            {
                tvi.IsSelected = true;
                tvi.Focus();
                _selectedNhom = tvi.DataContext as NhomKhachHangTreeItem;
            }
        }

        private void MiThemMoiItem_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomKhachHangWindow();
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
                await LoadKhachHangListAsync();
            };
            if (win.ShowDialog() == true)
            {
                LoadTreeNhomAsync();
            }
        }

        private void MiThemNhanhGoc_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhKhachHangWindow(0, parentId: null);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
                await LoadKhachHangListAsync();
            };
            if (win.ShowDialog() == true)
            {
                LoadTreeNhomAsync();
            }
        }

        private void MiThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã thêm đường phân cách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void MiThemThuMucGoc_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Tạo mới thư mục", "Nhập tên thư mục mới:", "Thư mục mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "Thư mục mới";
                string newId = Guid.NewGuid().ToString();
                await LocalKhachHangService.SaveNhomKhachHangFolderAsync(newId, folderName, true, null);
                await LoadTreeNhomAsync();
            }
        }

        private void MiThemConItem_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedNhom?.Id;
            var win = new ThemNhomKhachHangWindow(parentId: parentId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
                await LoadKhachHangListAsync();
            };
            if (win.ShowDialog() == true)
            {
                LoadTreeNhomAsync();
            }
        }

        private void MiThemNhanhCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedNhom?.Id;
            var win = new ThemNhanhKhachHangWindow(0, parentId: parentId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
                await LoadKhachHangListAsync();
            };
            if (win.ShowDialog() == true)
            {
                LoadTreeNhomAsync();
            }
        }

        private async void MiThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom == null) return;

            var inputWin = new InputWindow("Tạo thư mục con", "Nhập tên thư mục con:", "Thư mục con");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "Thư mục con";
                string newId = Guid.NewGuid().ToString();
                await LocalKhachHangService.SaveNhomKhachHangFolderAsync(newId, folderName, true, _selectedNhom.Id);
                await LoadTreeNhomAsync();
            }
        }

        private void MiChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                var win = new ThemNhomKhachHangWindow(_selectedNhom.Id);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () =>
                {
                    await LoadTreeNhomAsync();
                    await LoadKhachHangListAsync();
                };
                if (win.ShowDialog() == true)
                {
                    LoadTreeNhomAsync();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một nhóm khách hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MiSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            MiSapXepTen.IsChecked = true;
            MiSapXepTuyChon.IsChecked = false;
            SortTreeNodes(byName: true);
        }

        private void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e)
        {
            MiSapXepTen.IsChecked = false;
            MiSapXepTuyChon.IsChecked = true;
            SortTreeNodes(byName: false);
        }

        private void SortTreeNodes(bool byName)
        {
            if (TvNhomKhachHang.ItemsSource is ObservableCollection<NhomKhachHangTreeItem> list && list.Count > 0)
            {
                var root = list[0];
                if (byName)
                {
                    var sorted = root.Children.OrderBy(x => x.Name).ToList();
                    root.Children.Clear();
                    foreach (var s in sorted) root.Children.Add(s);
                }
                else
                {
                    var sorted = root.Children.OrderBy(x => x.Id).ToList();
                    root.Children.Clear();
                    foreach (var s in sorted) root.Children.Add(s);
                }
            }
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomAsync();
            await LoadKhachHangListAsync();
        }

        private void MiSaoChepNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                Clipboard.SetText(_selectedNhom.Name ?? "");
                MessageBox.Show($"Đã sao chép tên nhóm '{_selectedNhom.Name}' vào bộ nhớ tạm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MiMoRong_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(true);
        }

        private void MiThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(false);
        }

        private void SetExpandAll(bool expand)
        {
            if (TvNhomKhachHang.ItemsSource is ObservableCollection<NhomKhachHangTreeItem> list)
            {
                foreach (var item in list)
                {
                    SetExpandRecursive(item, expand);
                }
            }
        }

        private void SetExpandRecursive(NhomKhachHangTreeItem node, bool expand)
        {
            if (node == null) return;
            node.IsExpanded = expand;
            foreach (var ch in node.Children)
            {
                SetExpandRecursive(ch, expand);
            }
        }

        private async void BtnXoaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm '{_selectedNhom.Name}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = await LocalKhachHangService.DeleteNhomKhachHangAsync(_selectedNhom.Id);
                    if (ok)
                    {
                        await LoadTreeNhomAsync();
                        await LoadKhachHangListAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa nhóm này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một nhóm khách hàng cụ thể để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MiDoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                var inputWin = new InputWindow("Đổi tên nhóm", "Nhập tên nhóm mới:", _selectedNhom.Name);
                inputWin.Owner = Window.GetWindow(this);
                if (inputWin.ShowDialog() == true)
                {
                    string newName = inputWin.InputText?.Trim();
                    if (!string.IsNullOrEmpty(newName))
                    {
                        await LocalKhachHangService.SaveNhomKhachHangFolderAsync(_selectedNhom.Id, newName, false);
                        _selectedNhom.Name = newName;
                    }
                }
            }
            else
            {
                MessageBox.Show("Chỉ có thể đổi tên nhóm hoặc thư mục cụ thể!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MiThungRac_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomKhachHang.ItemsSource is ObservableCollection<NhomKhachHangTreeItem> list && list.Count > 0)
            {
                var trash = list[0].Children.FirstOrDefault(x => x.Id == "TRASH");
                if (trash != null)
                {
                    trash.IsExpanded = true;
                    _selectedNhom = trash;
                    _currentNhomId = trash.Id;
                    LoadKhachHangListAsync();
                }
            }
        }

        private void MiBieuTuong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đổi biểu tượng đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                MessageBox.Show($"Tên: {_selectedNhom.Name}\nID: {_selectedNhom.Id}\nLoại: Nhóm khách hàng", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #region DgTangGiamDiem ContextMenu Handlers
        private string _clickedTangDiemCellValue;

        private void DgTangGiamDiem_Row_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    DgTangGiamDiem.SelectedItems.Clear();
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
                        _clickedTangDiemCellValue = (cell.Content as TextBlock)?.Text ?? "";
                    }
                }
            }
        }

        private void MiThemNhanhExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm nhanh từ Excel cho Tăng giảm điểm đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiCapNhatNhanhExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng cập nhật nhanh từ Excel cho Tăng giảm điểm đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiSapXepTangDan_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderBy(x => x.Ngay).ThenBy(x => x.SoPhieu).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepGiamDan_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderByDescending(x => x.Ngay).ThenByDescending(x => x.SoPhieu).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepNgay_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderBy(x => x.Ngay).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepSoPhieu_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderBy(x => x.SoPhieu).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepGhiChu_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderBy(x => x.GhiChu).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepDiemTang_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderByDescending(x => x.DiemTang ?? 0).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepDiemGiam_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderByDescending(x => x.DiemGiam ?? 0).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void MiSapXepLyDo_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _tangGiamDiemList.OrderBy(x => x.LyDo).ToList();
            RebindTangGiamDiem(sorted);
        }

        private void RebindTangGiamDiem(List<TangGiamDiemItem> items)
        {
            _tangGiamDiemList.Clear();
            int stt = 1;
            foreach (var item in items)
            {
                item.Stt = stt++;
                _tangGiamDiemList.Add(item);
            }
        }

        private async void BtnRefreshTangDiem_Click(object sender, RoutedEventArgs e)
        {
            await LoadCustomerDetailsAsync();
        }

        private void MiInDanhSachTangDiem_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgTangGiamDiem, "Danh sách Tăng giảm điểm - Tặng quà");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MiSaoChepOTangDiem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedTangDiemCellValue))
            {
                Clipboard.SetText(_clickedTangDiemCellValue);
            }
            else if (DgTangGiamDiem.SelectedItem is TangGiamDiemItem item)
            {
                Clipboard.SetText(item.SoPhieu ?? "");
            }
        }

        private void MiSaoChepVungChonTangDiem_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgTangGiamDiem.SelectedItems.Cast<TangGiamDiemItem>().ToList();
            if (selectedList.Count == 0 && DgTangGiamDiem.SelectedItem is TangGiamDiemItem single)
            {
                selectedList.Add(single);
            }

            if (selectedList.Count == 0) return;

            var sb = new StringBuilder();
            foreach (var item in selectedList)
            {
                sb.AppendLine($"{item.Ngay:dd/MM/yyyy}\t{item.SoPhieu}\t{item.GhiChu}\t{item.DiemTang}\t{item.DiemGiam}\t{item.LyDo}");
            }

            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private void MiTuDongGianCotTangDiem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgTangGiamDiem.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MiCotHienThiTangDiem_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "Ngày", "Số phiếu", "Ghi chú", "Điểm tăng", "Điểm giảm", "Lý do" };
            var win = new ChonCotHienThiWindow(DgTangGiamDiem, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MiThuocTinhTangDiem_Click(object sender, RoutedEventArgs e)
        {
            if (DgTangGiamDiem.SelectedItem is TangGiamDiemItem item)
            {
                MessageBox.Show($"Số phiếu: {item.SoPhieu}\nNgày: {item.Ngay:dd/MM/yyyy}\nĐiểm tăng: {item.DiemTang}\nĐiểm giảm: {item.DiemGiam}\nLý do: {item.LyDo}\nGhi chú: {item.GhiChu}", "Thuộc tính phiếu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
        #endregion
    }
}
