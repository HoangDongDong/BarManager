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
    public partial class DanhMucKhachHangControl : UserControl
    {
        private ObservableCollection<NhomKhachHangTreeItem> _nhomTree;
        private List<KhachHangViewModel> _rawList = new List<KhachHangViewModel>();
        private NhomKhachHangTreeItem _selectedNhom;
        private int _groupMode = 0; // 0: Nhóm khách hàng, 1: Nhân viên, 2: Tỉnh thành
        private DataGridColumn _clickedColumn = null;
        private DataGridCell _clickedCell = null;
        private string _clickedCellValue = "";

        public DanhMucKhachHangControl()
        {
            InitializeComponent();
            this.IsVisibleChanged += DanhMucKhachHangControl_IsVisibleChanged;
        }

        private async void DanhMucKhachHangControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private bool _xemTheoThuMuc = true;

        private async Task LoadDataAsync()
        {
            try
            {
                if (_groupMode == 0)
                {
                    _nhomTree = await LocalKhachHangService.GetNhomKhachHangTreeAsync();
                    TxtLeftHeaderTitle.Text = "Nhóm khách hàng";
                    BtnThemNhom.ToolTip = "Thêm mới nhóm khách hàng";
                    BtnSuaNhom.ToolTip = "Sửa nhóm khách hàng";
                }
                else if (_groupMode == 1)
                {
                    _nhomTree = await LocalKhachHangService.GetNhanVienTreeAsync();
                    TxtLeftHeaderTitle.Text = "Nhân viên";
                    BtnThemNhom.ToolTip = "Thêm mới nhân viên";
                    BtnSuaNhom.ToolTip = "Sửa nhân viên";
                }
                else if (_groupMode == 2)
                {
                    _nhomTree = await LocalKhachHangService.GetTinhThanhTreeAsync();
                    TxtLeftHeaderTitle.Text = "Tỉnh thành";
                    BtnThemNhom.ToolTip = "Thêm mới tỉnh thành";
                    BtnSuaNhom.ToolTip = "Sửa tỉnh thành";
                }

                TvNhomKhachHang.ItemsSource = _nhomTree;

                if (_nhomTree.Count > 0)
                {
                    _selectedNhom = _nhomTree[0]; // Tất cả
                }

                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadDataAsync in DanhMucKhachHangControl: " + ex.Message);
            }
        }

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_nhomTree == null || _nhomTree.Count == 0) return;

                var rootNode = _nhomTree[0]; // Tất cả
                if (rootNode == null) return;

                int index = 1;
                string defaultName = $"Thư mục {index}";
                while (rootNode.Children.Any(x => x.Name == defaultName))
                {
                    index++;
                    defaultName = $"Thư mục {index}";
                }

                string newId = Guid.NewGuid().ToString();

                var newItem = new NhomKhachHangTreeItem
                {
                    Id = newId,
                    Name = defaultName,
                    ItemType = 2,
                    Icon = "📁",
                    IconColor = "#f0ad4e",
                    IsEditing = true
                };

                // Chèn trước Thùng rác (hoặc cuối danh sách)
                int insertPos = rootNode.Children.Count;
                for (int i = 0; i < rootNode.Children.Count; i++)
                {
                    if (rootNode.Children[i].ItemType == 3) // Thùng rác
                    {
                        insertPos = i;
                        break;
                    }
                }
                rootNode.Children.Insert(insertPos, newItem);

                // Lưu vào CSDL với tư cách là Thư mục (ITEMTYPE = 1)
                if (_groupMode == 0)
                {
                    await LocalKhachHangService.SaveNhomKhachHangFolderAsync(newId, defaultName, true);
                }
                else if (_groupMode == 1)
                {
                    await LocalKhachHangService.SaveNhanVienFolderAsync(newId, defaultName, true);
                }
                else if (_groupMode == 2)
                {
                    await LocalKhachHangService.SaveTinhThanhFolderAsync(newId, defaultName, true);
                }

                // Focus và bôi đen tên thư mục để người dùng gõ lại
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    newItem.IsEditing = true;
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BtnThemThuMuc_Click: " + ex.Message);
            }
        }

        private void InlineEditTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt)
            {
                txt.Focus();
                txt.SelectAll();
            }
        }

        private async void InlineEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt && txt.DataContext is NhomKhachHangTreeItem model && model.IsEditing)
            {
                model.IsEditing = false;
                string newName = txt.Text.Trim();
                if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(model.Id))
                {
                    model.Name = newName;
                    await UpdateItemNameAsync(model.Id, newName);
                }
            }
        }

        private async void InlineEditTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox txt && txt.DataContext is NhomKhachHangTreeItem model && model.IsEditing)
            {
                if (e.Key == Key.Enter)
                {
                    model.IsEditing = false;
                    string newName = txt.Text.Trim();
                    if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(model.Id))
                    {
                        model.Name = newName;
                        await UpdateItemNameAsync(model.Id, newName);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    model.IsEditing = false;
                    e.Handled = true;
                }
            }
        }

        private async Task UpdateItemNameAsync(string id, string newName)
        {
            if (_groupMode == 0)
            {
                await LocalKhachHangService.SaveNhomKhachHangFolderAsync(id, newName, false);
            }
            else if (_groupMode == 1)
            {
                await LocalKhachHangService.SaveNhanVienFolderAsync(id, newName, false);
            }
            else if (_groupMode == 2)
            {
                await LocalKhachHangService.SaveTinhThanhFolderAsync(id, newName, false);
            }
        }

        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _xemTheoThuMuc = !_xemTheoThuMuc;
            if (_xemTheoThuMuc)
            {
                BtnXemTheoThuMuc.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fff9e6"));
                BtnXemTheoThuMuc.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#e6a800"));
                BtnXemTheoThuMuc.BorderThickness = new Thickness(1);
            }
            else
            {
                BtnXemTheoThuMuc.Background = System.Windows.Media.Brushes.Transparent;
                BtnXemTheoThuMuc.BorderBrush = System.Windows.Media.Brushes.Transparent;
                BtnXemTheoThuMuc.BorderThickness = new Thickness(0);
            }
        }

        private async Task RefreshGridAsync()
        {
            string filterId = _selectedNhom?.Id ?? "ALL";
            int itemType = _selectedNhom?.ItemType ?? 0;
            string kw = TxtLoc.Text.Trim();

            _rawList = await LocalKhachHangService.GetKhachHangListAsync(filterId, itemType, _groupMode, kw);
            DgKhachHang.ItemsSource = _rawList;
        }

        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            CmCheDoPhanNhom.PlacementTarget = BtnMenuCheDo;
            CmCheDoPhanNhom.IsOpen = true;
        }

        private async void MenuItem_CheDoNhom_Click(object sender, RoutedEventArgs e)
        {
            _groupMode = 0;
            await LoadDataAsync();
        }

        private async void MenuItem_CheDoNhanVien_Click(object sender, RoutedEventArgs e)
        {
            _groupMode = 1;
            await LoadDataAsync();
        }

        private async void MenuItem_CheDoTinhThanh_Click(object sender, RoutedEventArgs e)
        {
            _groupMode = 2;
            await LoadDataAsync();
        }

        private async void TvNhomKhachHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemKhachHangWindow((string)null, _nhomTree?[0]?.Children);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => await RefreshGridAsync();
            win.ShowDialog();
            await RefreshGridAsync();
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                var win = new ThemKhachHangWindow(selected.Id, _nhomTree?[0]?.Children);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => await RefreshGridAsync();
                win.ShowDialog();
                await RefreshGridAsync();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DgKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                bool isTrash = _selectedNhom != null && (_selectedNhom.Id == "TRASH" || _selectedNhom.ParentId == "TRASH");
                string title = isTrash ? "Xác nhận xóa vĩnh viễn" : "Xác nhận xóa";
                string msg = isTrash 
                    ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN khách hàng '{selected.Name}' ({selected.Makhach}) không?" 
                    : $"Bạn có chắc chắn muốn xóa khách hàng '{selected.Name}' ({selected.Makhach})?";

                var ask = MessageBox.Show(msg, title, MessageBoxButton.YesNo, isTrash ? MessageBoxImage.Warning : MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = isTrash 
                        ? await LocalKhachHangService.DeletePermanentKhachHangAsync(selected.Id)
                        : await LocalKhachHangService.DeleteKhachHangAsync(selected.Id);

                    if (ok)
                    {
                        await RefreshGridAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa khách hàng này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhKhachHangBangExcelWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await RefreshGridAsync();
            }
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            var items = (DgKhachHang.ItemsSource as System.Collections.IEnumerable)?.Cast<KhachHangViewModel>().ToList() ?? _rawList;
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu khách hàng để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Xuất danh sách khách hàng ra Excel",
                FileName = "DanhSachKhachHang.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("KhachHang");

                        // Headers
                        string[] headers = new[]
                        {
                            "STT", "Mã khách", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email",
                            "Nhóm khách hàng", "Mã số thuế", "Nhân viên", "Tỉnh thành", "Facebook",
                            "Thẻ trả trước", "Ghi chú", "Điểm tích lũy ban đầu", "Ngày thành lập/sinh nhật"
                        };

                        for (int col = 0; col < headers.Length; col++)
                        {
                            worksheet.Cell(1, col + 1).Value = headers[col];
                        }

                        // Style header
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dfe9f5");
                        headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        headerRow.Height = 25;

                        int row = 2;
                        int stt = 1;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = stt++;
                            worksheet.Cell(row, 2).Value = item.Makhach ?? "";
                            worksheet.Cell(row, 3).Value = item.Name ?? "";
                            worksheet.Cell(row, 4).Value = item.Diachi ?? "";
                            worksheet.Cell(row, 5).Value = item.Dienthoai ?? "";
                            worksheet.Cell(row, 6).Value = item.Email ?? "";
                            worksheet.Cell(row, 7).Value = item.TenNhomKhachHang ?? "";
                            worksheet.Cell(row, 8).Value = item.Masothue ?? "";
                            worksheet.Cell(row, 9).Value = item.TenNhanVien ?? "";
                            worksheet.Cell(row, 10).Value = item.TinhThanh ?? "";
                            worksheet.Cell(row, 11).Value = item.Facebook ?? "";
                            worksheet.Cell(row, 12).Value = item.TheTraTruoc ?? "";
                            worksheet.Cell(row, 13).Value = item.Note ?? "";
                            worksheet.Cell(row, 14).Value = item.Diemtichluy;
                            worksheet.Cell(row, 15).Value = item.Ngaysinh?.ToString("dd/MM/yyyy") ?? "";

                            row++;
                        }

                        // Apply borders
                        var dataRange = worksheet.Range(1, 1, row - 1, headers.Length);
                        dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show($"Đã xuất danh sách {items.Count} khách hàng ra Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgKhachHang, "Khách hàng");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Tổng số khách hàng hiện tại: {_rawList.Count}", "Thống kê", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private Point _dragStartPoint;
        private NhomKhachHangTreeItem _draggedTreeItem;

        private void TvNhomKhachHang_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is TreeViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is TreeViewItem tvi)
            {
                _draggedTreeItem = tvi.DataContext as NhomKhachHangTreeItem;
            }
            else
            {
                _draggedTreeItem = null;
            }
        }

        private void TvNhomKhachHang_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedTreeItem != null && _draggedTreeItem.ItemType == 2 && _draggedTreeItem.Icon != "📁")
            {
                Point currentPos = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPos;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(TvNhomKhachHang, _draggedTreeItem, DragDropEffects.Move);
                }
            }
        }

        private void TvNhomKhachHang_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private async void TvNhomKhachHang_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(NhomKhachHangTreeItem)) is NhomKhachHangTreeItem dragged)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while (dep != null && !(dep is TreeViewItem))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                if (dep is TreeViewItem tvi && tvi.DataContext is NhomKhachHangTreeItem targetNode && targetNode.Id != dragged.Id)
                {
                    string targetFolderId = (targetNode.Icon == "📁" || targetNode.ItemType == 1) ? targetNode.Id : (targetNode.Id == "ALL" ? "" : targetNode.ParentId);

                    if (_groupMode == 1)
                    {
                        await LocalKhachHangService.UpdateEmployeeParentAsync(dragged.Id, targetFolderId);
                        await LoadDataAsync();
                    }
                }
            }
        }

        private async void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            string parentFolderId = (_selectedNhom != null && _selectedNhom.Icon == "📁") ? _selectedNhom.Id : null;

            if (_groupMode == 1)
            {
                var win = new ThemNhanVienWindow(parentId: parentFolderId);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true)
                {
                    await LoadDataAsync();
                }
            }
            else if (_groupMode == 2)
            {
                var win = new ThemTinhThanhWindow();
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true)
                {
                    await LoadDataAsync();
                }
            }
            else
            {
                var win = new ThemNhomKhachHangWindow();
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async void BtnSuaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                if (_groupMode == 1)
                {
                    var win = new ThemNhanVienWindow(_selectedNhom.Id);
                    win.Owner = Window.GetWindow(this);
                    win.OnSaved += async () => { await LoadDataAsync(); };
                    if (win.ShowDialog() == true)
                    {
                        await LoadDataAsync();
                    }
                }
                else if (_groupMode == 2)
                {
                    var win = new ThemTinhThanhWindow(_selectedNhom.Id);
                    win.Owner = Window.GetWindow(this);
                    win.OnSaved += async () => { await LoadDataAsync(); };
                    if (win.ShowDialog() == true)
                    {
                        await LoadDataAsync();
                    }
                }
                else
                {
                    var win = new ThemNhomKhachHangWindow(_selectedNhom.Id);
                    win.Owner = Window.GetWindow(this);
                    win.OnSaved += async () => { await LoadDataAsync(); };
                    if (win.ShowDialog() == true)
                    {
                        await LoadDataAsync();
                    }
                }
            }
            else
            {
                string label = (_groupMode == 1) ? "nhân viên" : (_groupMode == 2 ? "tỉnh thành" : "nhóm khách hàng");
                MessageBox.Show($"Vui lòng chọn một {label} để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnXoaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                string label = (_groupMode == 1) ? "nhân viên" : (_groupMode == 2 ? "tỉnh thành" : "nhóm khách hàng");
                bool isTrashItem = _selectedNhom.ParentId == "TRASH";

                string confirmText = isTrashItem 
                    ? $"Bạn có chắc chắn muốn xóa VĨNH VIỄN {label} '{_selectedNhom.Name}'? (Thao tác này không thể hoàn tác!)" 
                    : $"Bạn có chắc chắn muốn xóa {label} '{_selectedNhom.Name}' và đưa vào Thùng rác?";

                var ask = MessageBox.Show(confirmText, "Xác nhận xóa", MessageBoxButton.YesNo, isTrashItem ? MessageBoxImage.Warning : MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = false;
                    if (isTrashItem)
                    {
                        if (_groupMode == 1)
                            ok = await LocalKhachHangService.DeletePermanentNhanVienAsync(_selectedNhom.Id);
                        else if (_groupMode == 2)
                            ok = await LocalKhachHangService.DeletePermanentTinhThanhAsync(_selectedNhom.Id);
                        else
                            ok = await LocalKhachHangService.DeletePermanentNhomKhachHangAsync(_selectedNhom.Id);
                    }
                    else
                    {
                        if (_groupMode == 1)
                            ok = await LocalKhachHangService.DeleteNhanVienAsync(_selectedNhom.Id);
                        else if (_groupMode == 2)
                            ok = await LocalKhachHangService.DeleteTinhThanhAsync(_selectedNhom.Id);
                        else
                            ok = await LocalKhachHangService.DeleteNhomKhachHangAsync(_selectedNhom.Id);
                    }

                    if (ok)
                    {
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Không thể xóa {label} này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                string label = (_groupMode == 1) ? "nhân viên" : (_groupMode == 2 ? "tỉnh thành" : "nhóm khách hàng");
                MessageBox.Show($"Vui lòng chọn một {label} cụ thể để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MiKhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ParentId == "TRASH")
            {
                string label = (_groupMode == 1) ? "nhân viên" : (_groupMode == 2 ? "tỉnh thành" : "nhóm khách hàng");
                var ask = MessageBox.Show($"Bạn có muốn khôi phục {label} '{_selectedNhom.Name}' từ Thùng rác?", 
                                          "Khôi phục dữ liệu", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = false;
                    if (_groupMode == 1)
                        ok = await LocalKhachHangService.RestoreNhanVienAsync(_selectedNhom.Id);
                    else if (_groupMode == 2)
                        ok = await LocalKhachHangService.RestoreTinhThanhAsync(_selectedNhom.Id);
                    else
                        ok = await LocalKhachHangService.RestoreNhomKhachHangAsync(_selectedNhom.Id);

                    if (ok)
                    {
                        MessageBox.Show($"Đã khôi phục {label} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Không thể khôi phục {label} này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Chỉ có thể khôi phục các mục nằm trong Thùng rác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void MenuSaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                string info = $"Mã: {selected.Makhach}\nTên: {selected.Name}\nSĐT: {selected.Dienthoai}\nĐịa chỉ: {selected.Diachi}\nEmail: {selected.Email}";
                Clipboard.SetText(info);
                MessageBox.Show("Đã sao chép thông tin khách hàng vào bộ nhớ tạm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshGridAsync();
        }

        #region ContextMenu Handlers
        private void CmTreeView_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu cm)
            {
                string label = (_groupMode == 1) ? "nhân viên" : (_groupMode == 2 ? "tỉnh thành" : "nhóm khách hàng");
                if (cm.Items.Count > 0 && cm.Items[0] is MenuItem miThemMoi && miThemMoi.Items.Count > 0 && miThemMoi.Items[0] is MenuItem miAdd)
                {
                    miAdd.Header = $"➕  Thêm {label}";
                }
                if (cm.Items.Count > 1 && cm.Items[1] is MenuItem miThemCon && miThemCon.Items.Count > 0 && miThemCon.Items[0] is MenuItem miAddChild)
                {
                    miAddChild.Header = $"➕  Thêm {label}";
                }
            }
        }

        private void MiThemMoiItem_Click(object sender, RoutedEventArgs e)
        {
            BtnThemNhom_Click(sender, e);
        }

        private async void MiThemNhanhGoc_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhKhachHangWindow(_groupMode, parentId: null);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => { await LoadDataAsync(); };
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private void MiThemThuMucGoc_Click(object sender, RoutedEventArgs e)
        {
            BtnThemThuMuc_Click(sender, e);
        }

        private async void MiThemConItem_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedNhom?.Id;
            if (_groupMode == 1)
            {
                var win = new ThemNhanVienWindow(parentId: parentId);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true) await LoadDataAsync();
            }
            else if (_groupMode == 2)
            {
                var win = new ThemTinhThanhWindow(parentId: parentId);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true) await LoadDataAsync();
            }
            else
            {
                var win = new ThemNhomKhachHangWindow(parentId: parentId);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true) await LoadDataAsync();
            }
        }

        private async void MiThemNhanhCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedNhom?.Id;
            var win = new ThemNhanhKhachHangWindow(_groupMode, parentId: parentId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => { await LoadDataAsync(); };
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private async void MiThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                string newId = Guid.NewGuid().ToString();
                string defaultName = "Thư mục 1";
                var newItem = new NhomKhachHangTreeItem
                {
                    Id = newId,
                    Name = defaultName,
                    ItemType = 2,
                    Icon = "📁",
                    IconColor = "#f0ad4e",
                    ParentId = _selectedNhom.Id
                };
                _selectedNhom.Children.Add(newItem);

                if (_groupMode == 0)
                    await LocalKhachHangService.SaveNhomKhachHangFolderAsync(newId, defaultName, true, _selectedNhom.Id);
                else if (_groupMode == 1)
                    await LocalKhachHangService.SaveNhanVienFolderAsync(newId, defaultName, true, _selectedNhom.Id);
                else if (_groupMode == 2)
                    await LocalKhachHangService.SaveTinhThanhFolderAsync(newId, defaultName, true, _selectedNhom.Id);

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    newItem.IsEditing = true;
                }));
            }
            else
            {
                BtnThemThuMuc_Click(sender, e);
            }
        }

        private void MiChinhSua_Click(object sender, RoutedEventArgs e)
        {
            BtnSuaNhom_Click(sender, e);
        }

        private async void MiSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void MiSaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                Clipboard.SetText(_selectedNhom.Name);
                MessageBox.Show($"Đã sao chép: {_selectedNhom.Name}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        private void MiDoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                _selectedNhom.IsEditing = true;
            }
        }

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

        private void MiBieuTuong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đổi biểu tượng đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                MessageBox.Show($"Tên: {_selectedNhom.Name}\nID: {_selectedNhom.Id}\nLoại: {_selectedNhom.Icon}", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void GridContextMenu_Opened(object sender, RoutedEventArgs e)
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

            string colHeader = _clickedColumn?.Header?.ToString() ?? "Mã khách";
            var selectedList = DgKhachHang.SelectedItems.Cast<KhachHangViewModel>().ToList();
            if (selectedList.Count == 0 && DgKhachHang.SelectedItem is KhachHangViewModel single) selectedList.Add(single);

            MenuDatCot.Items.Clear();
            MenuLocCot.Items.Clear();
            MenuDatCot.Click -= MenuDatCot_Click;
            MenuLocCot.Click -= MenuLocCot_Click;

            MenuDatCot.Header = $"Đặt {colHeader}";
            MenuLocCot.Header = $"Lọc {colHeader}";

            if (colHeader == "Nhóm khách hàng")
            {
                var nhoms = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                foreach (var nh in nhoms)
                {
                    string nhId = nh.ID?.ToString();
                    string nhName = nh.NAME?.ToString();

                    var miDat = new MenuItem { Header = nhName, Tag = nhId };
                    miDat.Click += async (s, args) =>
                    {
                        var ids = selectedList.Select(k => k.Id).ToList();
                        if (await LocalKhachHangService.UpdateCustomersColumnAsync(ids, "DNHOMKHACHHANGID", nhId))
                        {
                            await RefreshGridAsync();
                        }
                    };
                    MenuDatCot.Items.Add(miDat);

                    var miLoc = new MenuItem { Header = nhName };
                    miLoc.Click += (s, args) =>
                    {
                        TxtLoc.Text = nhName;
                    };
                    MenuLocCot.Items.Add(miLoc);
                }
            }
            else if (colHeader == "Nhân viên")
            {
                var nvs = await LocalKhachHangService.GetNhanVienLookupAsync();
                foreach (var nv in nvs)
                {
                    string nvId = nv.ID?.ToString();
                    string nvName = nv.NAME?.ToString();

                    var miDat = new MenuItem { Header = nvName, Tag = nvId };
                    miDat.Click += async (s, args) =>
                    {
                        var ids = selectedList.Select(k => k.Id).ToList();
                        if (await LocalKhachHangService.UpdateCustomersColumnAsync(ids, "DNHANVIENID", nvId))
                        {
                            await RefreshGridAsync();
                        }
                    };
                    MenuDatCot.Items.Add(miDat);

                    var miLoc = new MenuItem { Header = nvName };
                    miLoc.Click += (s, args) =>
                    {
                        TxtLoc.Text = nvName;
                    };
                    MenuLocCot.Items.Add(miLoc);
                }
            }
            else if (colHeader == "Tỉnh thành")
            {
                var tts = await LocalKhachHangService.GetTinhThanhLookupAsync();
                foreach (var tt in tts)
                {
                    string ttId = tt.ID?.ToString();
                    string ttName = tt.NAME?.ToString();

                    var miDat = new MenuItem { Header = ttName, Tag = ttId };
                    miDat.Click += async (s, args) =>
                    {
                        var ids = selectedList.Select(k => k.Id).ToList();
                        if (await LocalKhachHangService.UpdateCustomersColumnAsync(ids, "DTINHTHANHID", ttId))
                        {
                            await RefreshGridAsync();
                        }
                    };
                    MenuDatCot.Items.Add(miDat);

                    var miLoc = new MenuItem { Header = ttName };
                    miLoc.Click += (s, args) =>
                    {
                        TxtLoc.Text = ttName;
                    };
                    MenuLocCot.Items.Add(miLoc);
                }
            }
            else
            {
                MenuDatCot.Click += MenuDatCot_Click;
                MenuLocCot.Click += MenuLocCot_Click;
            }
        }

        private async void MenuDatCot_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgKhachHang.SelectedItems.Cast<KhachHangViewModel>().ToList();
            if (selectedList.Count == 0 && DgKhachHang.SelectedItem is KhachHangViewModel single) selectedList.Add(single);
            if (selectedList.Count == 0) return;

            string colHeader = _clickedColumn?.Header?.ToString() ?? "Mã khách";
            string currentVal = _clickedCellValue ?? "";

            var inputWin = new InputWindow($"Đặt {colHeader}", $"Nhập giá trị mới cho '{colHeader}':", currentVal);
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string newVal = inputWin.InputText?.Trim() ?? "";
                string dbCol = "NAME";

                if (colHeader == "Mã khách") dbCol = "MAKHACH";
                else if (colHeader == "Tên khách hàng") dbCol = "NAME";
                else if (colHeader == "Địa chỉ") dbCol = "DIACHI";
                else if (colHeader == "Điện thoại") dbCol = "DIENTHOAI";
                else if (colHeader == "Email") dbCol = "EMAIL";
                else if (colHeader == "Mã số thuế") dbCol = "MASOTHUE";
                else if (colHeader == "Facebook") dbCol = "FACEBOOK";
                else if (colHeader == "Thẻ trả trước") dbCol = "THETRATRUOC";

                var ids = selectedList.Select(k => k.Id).ToList();
                if (await LocalKhachHangService.UpdateCustomersColumnAsync(ids, dbCol, newVal))
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
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn khôi phục khách hàng '{selected.Name}' ({selected.Makhach}) không?", "Xác nhận khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    if (await LocalKhachHangService.RestoreKhachHangAsync(selected.Id))
                    {
                        await RefreshGridAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể khôi phục khách hàng này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khách hàng để khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Name).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderByDescending(k => k.Name).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByMaKhach_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Makhach).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByName_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Name).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByDiaChi_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Diachi).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByDienThoai_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Dienthoai).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByEmail_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Email).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByNhom_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.TenNhomKhachHang).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByMst_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Masothue).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByNhanVien_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.TenNhanVien).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByTinhThanh_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.TinhThanh).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByFacebook_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.Facebook).ToList();
            RebindSttAndGrid();
        }

        private void MenuItem_SortByTheTraTruoc_Click(object sender, RoutedEventArgs e)
        {
            _rawList = _rawList.OrderBy(k => k.TheTraTruoc).ToList();
            RebindSttAndGrid();
        }

        private void RebindSttAndGrid()
        {
            int stt = 1;
            foreach (var item in _rawList)
            {
                item.Stt = stt++;
            }
            DgKhachHang.ItemsSource = null;
            DgKhachHang.ItemsSource = _rawList;
        }



        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                Clipboard.SetText(_clickedCellValue);
            }
            else
            {
                Clipboard.Clear();
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgKhachHang.SelectedItems.Cast<KhachHangViewModel>().ToList();
            if (selectedList.Count == 0 && DgKhachHang.SelectedItem is KhachHangViewModel single) selectedList.Add(single);
            if (selectedList.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            foreach (var item in selectedList)
            {
                sb.AppendLine($"{item.Makhach}\t{item.Name}\t{item.Diachi}\t{item.Dienthoai}\t{item.Email}\t{item.TenNhomKhachHang}\t{item.Masothue}\t{item.TenNhanVien}\t{item.TinhThanh}\t{item.Facebook}\t{item.TheTraTruoc}");
            }
            Clipboard.SetText(sb.ToString());
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
            var defaults = new List<string> { "Mã khách", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email", "Nhóm khách hàng", "Mã số thuế", "Nhân viên", "Tỉnh thành", "Facebook", "Thẻ trả trước" };
            var win = new ChonCotHienThiWindow(DgKhachHang, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                MessageBox.Show($"Khách hàng: {selected.Name} ({selected.Makhach})\nNhóm: {selected.TenNhomKhachHang}\nĐịa chỉ: {selected.Diachi}\nĐiện thoại: {selected.Dienthoai}", 
                                "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
