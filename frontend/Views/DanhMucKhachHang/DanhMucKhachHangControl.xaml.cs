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
            var win = new ThemKhachHangWindow(null, _nhomTree?[0]?.Children);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await RefreshGridAsync();
            }
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                var win = new ThemKhachHangWindow(selected, _nhomTree?[0]?.Children);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    await RefreshGridAsync();
                }
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
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{selected.Name}' ({selected.Makhach})?", 
                                          "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = await LocalKhachHangService.DeleteKhachHangAsync(selected.Id);
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

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng nhập từ Excel đang sẵn sàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Đã xuất danh sách {_rawList.Count} khách hàng ra Excel thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(DgKhachHang, "Danh Mục Khách Hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in danh sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}
