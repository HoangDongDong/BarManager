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
                await LoadData();
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemMoiDatHangWindow();
            if (win.ShowDialog() == true)
            {
                LoadData();
            }
        }
        
        private async void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            DateTime? tuNgay = DpTuNgay.SelectedDate;
            DateTime? denNgay = DpDenNgay.SelectedDate;
            
            var data = await _service.GetDatHangListAsync(_currentCategoryId, _isMucDichDatMode, tuNgay, denNgay);
            DgDatHang.ItemsSource = data;
        }

        private async void DgDatHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                var chiTiet = await _service.GetDatHangChiTietListAsync(selectedOrder.Id);
                DgDatHangChiTiet.ItemsSource = chiTiet;
            }
            else
            {
                DgDatHangChiTiet.ItemsSource = null;
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

                string typeName = _isMucDichDatMode ? "mục đích đặt" : "phương thức đặt";
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa {typeName} '{selected.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (await _service.DeletePhuongThucDatAsync(selected.Id, _isMucDichDatMode))
                    {
                        BtnRefreshPhuongThuc_Click(null, null);
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
