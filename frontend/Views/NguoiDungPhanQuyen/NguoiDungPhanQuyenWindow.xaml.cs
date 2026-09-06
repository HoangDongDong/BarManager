using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NguoiDungPhanQuyen
{
    public partial class NguoiDungPhanQuyenWindow : Window
    {
        private ObservableCollection<GroupUserItem> _groups = new ObservableCollection<GroupUserItem>();
        private ObservableCollection<UserAccountItem> _users = new ObservableCollection<UserAccountItem>();
        private ObservableCollection<FunctionRoleItem> _allFuncRoles = new ObservableCollection<FunctionRoleItem>();
        private ObservableCollection<ReportRoleItem> _allReportRoles = new ObservableCollection<ReportRoleItem>();

        private ICollectionView _funcRolesView;
        private ICollectionView _reportRolesView;

        private GroupUserItem _selectedGroup;
        private string _selectedFuncCategory = "Tất cả";
        private string _selectedReportCategory = "Tất cả";
        private ReportCategoryNode _selectedReportCategoryNode;

        public NguoiDungPhanQuyenWindow()
        {
            InitializeComponent();
            Loaded += NguoiDungPhanQuyenWindow_Loaded;
        }

        private async void NguoiDungPhanQuyenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitCategoryListsAsync();
            await LoadGroupsAsync();
        }

        private async Task InitCategoryListsAsync()
        {
            // 1. Danh mục nhóm chức năng (ListBox bên Tab 1) - Tải động từ DB (SFUNCTION)
            try
            {
                var funcCats = await LocalPhanQuyenService.GetFunctionCategoriesAsync();
                LstFuncCategories.ItemsSource = funcCats;
                LstFuncCategories.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading func categories: " + ex.Message);
            }

            // 2. Danh mục nhóm báo cáo dạng TreeView - Tải trực tiếp từ DB (SREPORT)
            try
            {
                var rootCats = await LocalPhanQuyenService.GetReportCategoriesTreeAsync();
                TreeReportCategories.ItemsSource = rootCats;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading report tree: " + ex.Message);
            }
        }

        #region Load Data
        public async Task LoadGroupsAsync(string selectGroupId = null)
        {
            try
            {
                var list = await LocalPhanQuyenService.GetGroupUsersAsync();
                _groups.Clear();

                _groups.Add(new GroupUserItem
                {
                    Id = "0",
                    Name = "Tất cả",
                    Note = "Tất cả nhóm"
                });

                foreach (var g in list)
                {
                    _groups.Add(g);
                }

                LstGroups.ItemsSource = _groups;

                if (!string.IsNullOrEmpty(selectGroupId))
                {
                    var item = _groups.FirstOrDefault(x => x.Id == selectGroupId);
                    LstGroups.SelectedItem = item ?? _groups.FirstOrDefault();
                }
                else
                {
                    LstGroups.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách nhóm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                string groupId = _selectedGroup?.Id;
                var list = await LocalPhanQuyenService.GetUsersAsync(groupId);
                _users = new ObservableCollection<UserAccountItem>(list);
                DgUsers.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                if (_selectedGroup == null || _selectedGroup.Id == "0")
                {
                    // Khi chọn "Tất cả", không hiển thị quyền gì ở bảng bên phải
                    _allFuncRoles.Clear();
                    _funcRolesView = CollectionViewSource.GetDefaultView(_allFuncRoles);
                    DgFuncRoles.ItemsSource = _funcRolesView;

                    _allReportRoles.Clear();
                    _reportRolesView = CollectionViewSource.GetDefaultView(_allReportRoles);
                    DgReportRoles.ItemsSource = _reportRolesView;
                    return;
                }

                string groupId = _selectedGroup.Id;

                // Function roles
                var fRoles = await LocalPhanQuyenService.GetFunctionRolesAsync(groupId);
                _allFuncRoles = new ObservableCollection<FunctionRoleItem>(fRoles);
                _funcRolesView = CollectionViewSource.GetDefaultView(_allFuncRoles);
                _funcRolesView.Filter = FilterFuncRoleItem;
                DgFuncRoles.ItemsSource = _funcRolesView;

                // Report roles
                var rRoles = await LocalPhanQuyenService.GetReportRolesAsync(groupId);
                _allReportRoles = new ObservableCollection<ReportRoleItem>(rRoles);
                _reportRolesView = CollectionViewSource.GetDefaultView(_allReportRoles);
                _reportRolesView.Filter = FilterReportRoleItem;
                DgReportRoles.ItemsSource = _reportRolesView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải quyền: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Filter Logic
        private bool FilterFuncRoleItem(object item)
        {
            if (item is FunctionRoleItem f)
            {
                string search = TxtFuncFilter?.Text?.Trim().ToLower();
                if (!string.IsNullOrEmpty(search))
                {
                    bool match = (f.FunctionName != null && f.FunctionName.ToLower().Contains(search)) ||
                                 (f.GroupName != null && f.GroupName.ToLower().Contains(search));
                    if (!match) return false;
                }

                if (!string.IsNullOrEmpty(_selectedFuncCategory) && _selectedFuncCategory != "Tất cả")
                {
                    if (string.Equals(_selectedFuncCategory, "Hóa đơn bán hàng", StringComparison.OrdinalIgnoreCase))
                    {
                        return (f.FunctionName != null && f.FunctionName.ToLower().Contains("hóa đơn")) ||
                               (f.GroupName != null && f.GroupName.Equals("Bán hàng", StringComparison.OrdinalIgnoreCase));
                    }
                    return f.GroupName != null && f.GroupName.Equals(_selectedFuncCategory, StringComparison.OrdinalIgnoreCase);
                }

                return true;
            }
            return false;
        }

        private bool FilterReportRoleItem(object item)
        {
            if (item is ReportRoleItem r)
            {
                string search = TxtReportFilter?.Text?.Trim().ToLower();
                if (!string.IsNullOrEmpty(search))
                {
                    bool match = (r.ReportName != null && r.ReportName.ToLower().Contains(search)) ||
                                 (r.GroupName != null && r.GroupName.ToLower().Contains(search));
                    if (!match) return false;
                }

                if (_selectedReportCategoryNode != null && _selectedReportCategoryNode.Id != "0")
                {
                    return IsReportInNode(r, _selectedReportCategoryNode);
                }

                return true;
            }
            return false;
        }

        private bool IsReportInNode(ReportRoleItem r, ReportCategoryNode node)
        {
            if (node == null || node.Id == "0") return true;
            if (!string.IsNullOrEmpty(r.ParentId) && r.ParentId == node.Id) return true;
            if (!string.IsNullOrEmpty(r.GroupName) && r.GroupName.Equals(node.Name, StringComparison.OrdinalIgnoreCase)) return true;

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (IsReportInNode(r, child)) return true;
                }
            }
            return false;
        }

        private void LstFuncCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstFuncCategories.SelectedItem is CategoryItem cat)
            {
                _selectedFuncCategory = cat.Name;
                _funcRolesView?.Refresh();
            }
        }

        private void TreeReportCategories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ReportCategoryNode node)
            {
                _selectedReportCategoryNode = node;
                _reportRolesView?.Refresh();
            }
            else if (e.NewValue is CategoryItem cat)
            {
                _selectedReportCategory = cat.Name;
                _reportRolesView?.Refresh();
            }
        }

        private void TxtFuncFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _funcRolesView?.Refresh();
        }

        private void TxtReportFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _reportRolesView?.Refresh();
        }
        #endregion

        #region Group & User Selection
        private async void LstGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedGroup = LstGroups.SelectedItem as GroupUserItem;
            bool isRealGroup = _selectedGroup != null && _selectedGroup.Id != "0";
            
            if (BtnEditGroup != null)
            {
                BtnEditGroup.IsEnabled = isRealGroup;
                BtnEditGroup.Opacity = isRealGroup ? 1.0 : 0.45;
            }
            if (BtnDeleteGroup != null)
            {
                BtnDeleteGroup.IsEnabled = isRealGroup;
                BtnDeleteGroup.Opacity = isRealGroup ? 1.0 : 0.45;
            }

            if (MenuEditGroup != null) MenuEditGroup.IsEnabled = isRealGroup;
            if (MenuDeleteGroup != null) MenuDeleteGroup.IsEnabled = isRealGroup;
            if (MenuRenameGroup != null) MenuRenameGroup.IsEnabled = isRealGroup;
            if (MenuIconGroup != null) MenuIconGroup.IsEnabled = isRealGroup;
            if (MenuPropGroup != null) MenuPropGroup.IsEnabled = isRealGroup;

            await LoadUsersAsync();
            await LoadRolesAsync();
        }
        #endregion

        #region Group CRUD
        private async void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ThemSuaNhomNguoiDungWindow();
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                await LoadGroupsAsync(dlg.GroupItem?.Id);
            }
        }

        private async void BtnEditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedGroup.Id == "0")
            {
                MessageBox.Show("Vui lòng chọn một nhóm người dùng cụ thể để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new ThemSuaNhomNguoiDungWindow(_selectedGroup);
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                await LoadGroupsAsync(_selectedGroup.Id);
            }
        }

        private async void BtnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedGroup.Id == "0")
            {
                MessageBox.Show("Vui lòng chọn một nhóm người dùng để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm '{_selectedGroup.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    bool ok = await LocalPhanQuyenService.DeleteGroupUserAsync(_selectedGroup.Id);
                    if (ok)
                    {
                        MessageBox.Show("Xóa nhóm người dùng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadGroupsAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region User CRUD
        private async void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string defaultGroupId = (_selectedGroup != null && _selectedGroup.Id != "0") ? _selectedGroup.Id : null;
            var dlg = new ThemSuaNguoiDungWindow(null, defaultGroupId, _users.ToList());
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                await LoadUsersAsync();
            }
        }

        private async void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is UserAccountItem selectedUser)
            {
                var dlg = new ThemSuaNguoiDungWindow(selectedUser, null, _users.ToList());
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    await LoadUsersAsync();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài khoản người dùng để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DgUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnEditUser_Click(sender, e);
        }

        private async void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is UserAccountItem selectedUser)
            {
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản '{selectedUser.Username}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool ok = await LocalPhanQuyenService.DeleteUserAsync(selectedUser.Id);
                        if (ok)
                        {
                            MessageBox.Show("Xóa tài khoản người dùng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadUsersAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài khoản người dùng để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Quick Actions & Save Roles
        private void ChkLockAllFunc_Click(object sender, RoutedEventArgs e)
        {
            bool isLock = ChkLockAllFunc.IsChecked == true;
            if (isLock)
            {
                ChkAllowAllFunc.IsChecked = false;
                foreach (var item in _allFuncRoles)
                {
                    if (FilterFuncRoleItem(item))
                    {
                        item.IsLocked = true;
                    }
                }
            }
        }

        private void ChkAllowAllFunc_Click(object sender, RoutedEventArgs e)
        {
            bool isAllow = ChkAllowAllFunc.IsChecked == true;
            if (isAllow)
            {
                ChkLockAllFunc.IsChecked = false;
                foreach (var item in _allFuncRoles)
                {
                    if (FilterFuncRoleItem(item))
                    {
                        item.IsAll = true;
                    }
                }
            }
        }

        private async void BtnSaveFuncRoles_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedGroup.Id == "0")
            {
                MessageBox.Show("Vui lòng chọn một nhóm người dùng cụ thể (Thu ngân, Thủ kho, ...) để lưu phân quyền.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool ok = await LocalPhanQuyenService.SaveFunctionRolesAsync(_selectedGroup.Id, _allFuncRoles);
                if (ok)
                {
                    MessageBox.Show($"Đã lưu quyền sử dụng chức năng cho nhóm '{_selectedGroup.Name}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Lưu quyền không thành công.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu quyền: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChkLockAllReport_Click(object sender, RoutedEventArgs e)
        {
            bool isLock = ChkLockAllReport.IsChecked == true;
            if (isLock)
            {
                ChkAllowAllReport.IsChecked = false;
                foreach (var item in _allReportRoles)
                {
                    if (FilterReportRoleItem(item))
                    {
                        item.NotAllowed = true;
                    }
                }
            }
        }

        private void ChkAllowAllReport_Click(object sender, RoutedEventArgs e)
        {
            bool isAllow = ChkAllowAllReport.IsChecked == true;
            if (isAllow)
            {
                ChkLockAllReport.IsChecked = false;
                foreach (var item in _allReportRoles)
                {
                    if (FilterReportRoleItem(item))
                    {
                        item.IsAllowed = true;
                    }
                }
            }
        }

        private async void BtnSaveReportRoles_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedGroup.Id == "0")
            {
                MessageBox.Show("Vui lòng chọn một nhóm người dùng cụ thể để lưu phân quyền báo cáo.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool ok = await LocalPhanQuyenService.SaveReportRolesAsync(_selectedGroup.Id, _allReportRoles);
                if (ok)
                {
                    MessageBox.Show($"Đã lưu quyền xem báo cáo cho nhóm '{_selectedGroup.Name}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Lưu quyền xem báo cáo không thành công.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu quyền báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region ContextMenu & Mouse Handlers
        private void LstGroups_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is ListBoxItem item && item.DataContext is GroupUserItem gItem)
            {
                LstGroups.SelectedItem = gItem;
            }
        }

        private void DgUsers_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is DataGridRow row && row.Item is UserAccountItem uItem)
            {
                DgUsers.SelectedItem = uItem;
            }
        }

        private void MenuThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã thêm đường phân cách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuSortGroupByName_Click(object sender, RoutedEventArgs e)
        {
            MenuSortGroupByName.Header = "✓  Tên";
            MenuSortGroupByCustom.Header = "Thứ tự tùy chọn";
            var sorted = _groups.Where(g => g.Id == "0").Concat(_groups.Where(g => g.Id != "0").OrderBy(g => g.Name)).ToList();
            _groups.Clear();
            foreach (var item in sorted) _groups.Add(item);
        }

        private async void MenuSortGroupByCustom_Click(object sender, RoutedEventArgs e)
        {
            MenuSortGroupByName.Header = "Tên";
            MenuSortGroupByCustom.Header = "✓  Thứ tự tùy chọn";
            await LoadGroupsAsync(_selectedGroup?.Id);
        }

        private async void MenuRefreshGroup_Click(object sender, RoutedEventArgs e)
        {
            await LoadGroupsAsync(_selectedGroup?.Id);
        }

        private void MenuCopyGroupName_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup != null)
            {
                Clipboard.SetText(_selectedGroup.Name);
                MessageBox.Show($"Đã sao chép: {_selectedGroup.Name}", "Sao chép", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuExpandGroup_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuCollapseGroup_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuTrash_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Thùng rác nhóm người dùng hiện đang trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuUserExcelImport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng nhập người dùng từ Excel.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuUserExcelUpdate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng cập nhật người dùng từ Excel.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuSortUserByName_Click(object sender, RoutedEventArgs e)
        {
            MenuSortUserByName.Header = "✓  Tên";
            MenuSortUserByUsername.Header = "Tài khoản";
            MenuSortUserByCustom.Header = "Thứ tự tùy chọn";
            var sorted = _users.OrderBy(u => u.Name).ToList();
            _users.Clear();
            foreach (var u in sorted) _users.Add(u);
        }

        private void MenuSortUserByUsername_Click(object sender, RoutedEventArgs e)
        {
            MenuSortUserByName.Header = "Tên";
            MenuSortUserByUsername.Header = "✓  Tài khoản";
            MenuSortUserByCustom.Header = "Thứ tự tùy chọn";
            var sorted = _users.OrderBy(u => u.Username).ToList();
            _users.Clear();
            foreach (var u in sorted) _users.Add(u);
        }

        private async void MenuSortUserByCustom_Click(object sender, RoutedEventArgs e)
        {
            MenuSortUserByName.Header = "Tên";
            MenuSortUserByUsername.Header = "Tài khoản";
            MenuSortUserByCustom.Header = "✓  Thứ tự tùy chọn";
            await LoadUsersAsync();
        }

        private async void MenuRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private void MenuPrintUsers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuẩn bị in danh sách tài khoản người dùng...", "In ấn", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuCopyUserCell_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is UserAccountItem u)
            {
                Clipboard.SetText(u.Username);
                MessageBox.Show($"Đã sao chép: {u.Username}", "Sao chép ô", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuCopyUserSelection_Click(object sender, RoutedEventArgs e)
        {
            if (DgUsers.SelectedItem is UserAccountItem u)
            {
                string text = $"{u.Username}\t{u.Name}\t{u.GroupName}\t{u.Email}";
                Clipboard.SetText(text);
                MessageBox.Show($"Đã sao chép thông tin tài khoản: {u.Username}", "Sao chép vùng chọn", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuAutoFitUserColumns_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgUsers.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void MenuUserColumns_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cấu hình cột hiển thị danh sách người dùng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Report Categories & Roles ContextMenu
        private bool _isReportTreeSortByName = false;

        private void TreeReportCategories_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is TreeViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is TreeViewItem tvi && tvi.DataContext is ReportCategoryNode node)
            {
                tvi.IsSelected = true;
                _selectedReportCategoryNode = node;
                _reportRolesView?.Refresh();
            }
        }

        private async void MenuReportTreeSortByName_Click(object sender, RoutedEventArgs e)
        {
            _isReportTreeSortByName = true;
            if (MenuReportTreeSortByName != null) MenuReportTreeSortByName.Header = "✓  Tên";
            if (MenuReportTreeSortByCustom != null) MenuReportTreeSortByCustom.Header = "Thứ tự tùy chọn";

            var rootCats = await LocalPhanQuyenService.GetReportCategoriesTreeAsync(sortByName: true);
            TreeReportCategories.ItemsSource = rootCats;
        }

        private async void MenuReportTreeSortByCustom_Click(object sender, RoutedEventArgs e)
        {
            _isReportTreeSortByName = false;
            if (MenuReportTreeSortByName != null) MenuReportTreeSortByName.Header = "Tên";
            if (MenuReportTreeSortByCustom != null) MenuReportTreeSortByCustom.Header = "✓  Thứ tự tùy chọn";

            var rootCats = await LocalPhanQuyenService.GetReportCategoriesTreeAsync(sortByName: false);
            TreeReportCategories.ItemsSource = rootCats;
        }

        private async void MenuRefreshReportCategories_Click(object sender, RoutedEventArgs e)
        {
            var rootCats = await LocalPhanQuyenService.GetReportCategoriesTreeAsync(sortByName: _isReportTreeSortByName);
            TreeReportCategories.ItemsSource = rootCats;
            await LoadRolesAsync();
        }

        private void DgReportRoles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is DataGridRow row && row.Item is ReportRoleItem rItem)
            {
                DgReportRoles.SelectedItem = rItem;
            }
        }

        private void MenuAllowReportRole_Click(object sender, RoutedEventArgs e)
        {
            if (DgReportRoles.SelectedItem is ReportRoleItem item)
            {
                item.IsAllowed = true;
            }
        }

        private void MenuLockReportRole_Click(object sender, RoutedEventArgs e)
        {
            if (DgReportRoles.SelectedItem is ReportRoleItem item)
            {
                item.NotAllowed = true;
            }
        }
        #endregion

        #region Function Categories & Roles ContextMenu
        private void LstFuncCategories_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is ListBoxItem item && item.DataContext is CategoryItem cat)
            {
                LstFuncCategories.SelectedItem = cat;
            }
        }

        private void MenuFuncSortByName_Click(object sender, RoutedEventArgs e)
        {
            if (MenuFuncSortByName != null) MenuFuncSortByName.Header = "✓  Tên";
            if (MenuFuncSortByCustom != null) MenuFuncSortByCustom.Header = "Thứ tự tùy chọn";

            if (LstFuncCategories.ItemsSource is IEnumerable<CategoryItem> items)
            {
                var sorted = items.Where(x => x.Name == "Tất cả").Concat(items.Where(x => x.Name != "Tất cả").OrderBy(x => x.Name)).ToList();
                LstFuncCategories.ItemsSource = sorted;
            }
        }

        private async void MenuFuncSortByCustom_Click(object sender, RoutedEventArgs e)
        {
            if (MenuFuncSortByName != null) MenuFuncSortByName.Header = "Tên";
            if (MenuFuncSortByCustom != null) MenuFuncSortByCustom.Header = "✓  Thứ tự tùy chọn";

            var funcCats = await LocalPhanQuyenService.GetFunctionCategoriesAsync();
            LstFuncCategories.ItemsSource = funcCats;
        }

        private async void MenuRefreshFuncCategories_Click(object sender, RoutedEventArgs e)
        {
            var funcCats = await LocalPhanQuyenService.GetFunctionCategoriesAsync();
            LstFuncCategories.ItemsSource = funcCats;
            await LoadRolesAsync();
        }

        private void DgFuncRoles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep is DataGridRow row && row.Item is FunctionRoleItem fItem)
            {
                DgFuncRoles.SelectedItem = fItem;
            }
        }

        private void MenuLockFuncRole_Click(object sender, RoutedEventArgs e)
        {
            if (DgFuncRoles.SelectedItem is FunctionRoleItem item)
            {
                item.IsLocked = true;
            }
        }

        private void MenuAllowFuncView_Click(object sender, RoutedEventArgs e)
        {
            if (DgFuncRoles.SelectedItem is FunctionRoleItem item)
            {
                item.CanView = true;
            }
        }

        private void MenuAllowFuncAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DgFuncRoles.SelectedItem is FunctionRoleItem item && item.HasCrud)
            {
                item.CanAdd = true;
            }
        }

        private void MenuAllowFuncEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgFuncRoles.SelectedItem is FunctionRoleItem item && item.HasCrud)
            {
                item.CanEdit = true;
            }
        }

        private void MenuAllowFuncDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgFuncRoles.SelectedItem is FunctionRoleItem item && item.HasCrud)
            {
                item.CanDelete = true;
            }
        }

        private void MenuAllowFuncAll_Click(object sender, RoutedEventArgs e)
        {
            if (DgFuncRoles.SelectedItem is FunctionRoleItem item)
            {
                item.IsAll = true;
            }
        }
        #endregion
        #endregion

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
