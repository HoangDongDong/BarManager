using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.NguoiDungPhanQuyen;

namespace QuanLyBar.Client.Views.TouchPOS
{
    #region ViewModels / Wrappers for Touch UI
    public class TouchGroupItem : INotifyPropertyChanged
    {
        public GroupUserItem Group { get; set; }
        public string Id => Group?.Id ?? "0";
        public string Name => Group?.Name ?? "";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(GroupBtnBrush));
            }
        }

        public Brush GroupBtnBrush
        {
            get
            {
                if (IsSelected)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8000")); // Active Orange
                }
                if (Id == "0" || Name.Equals("Tất cả", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#04044A")); // Dark Navy
                }
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006400")); // Dark Green
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class TouchUserItem : INotifyPropertyChanged
    {
        public UserAccountItem User { get; set; }
        public string Id => User?.Id ?? "";
        public string TenHienThi => !string.IsNullOrWhiteSpace(User?.Name) ? User.Name : (User?.Username ?? "");

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(UserBtnBrush));
                OnPropertyChanged(nameof(BorderBrushColor));
                OnPropertyChanged(nameof(BorderThicknessVal));
            }
        }

        public Brush UserBtnBrush => IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F57C00")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B4F4A"));
        public Brush BorderBrushColor => IsSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB74D")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C5E73"));
        public Thickness BorderThicknessVal => IsSelected ? new Thickness(2) : new Thickness(1);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class TouchCategoryItem : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(CategoryBtnBrush));
            }
        }

        public Brush CategoryBtnBrush
        {
            get
            {
                if (IsSelected)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8000")); // Active Orange
                }
                if (Name.Equals("Tất cả", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#04044A")); // Dark Navy
                }
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006400")); // Dark Green
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class TouchFunctionRoleVM : INotifyPropertyChanged
    {
        public FunctionRoleItem Role { get; set; }
        public string FunctionId => Role?.FunctionId ?? "";
        public string FunctionName => Role?.FunctionName ?? "";
        public string GroupName => Role?.GroupName ?? "";
        public bool HasCrud => Role?.HasCrud ?? true;

        public Brush KhoaBtnBrush => (Role != null && Role.IsLocked) ? RedBrush : GrayBrush;
        public Brush XemBtnBrush => (Role != null && !Role.IsLocked && Role.CanView) ? RedBrush : GrayBrush;
        public Brush ThemBtnBrush => (Role != null && !Role.IsLocked && Role.CanAdd) ? RedBrush : GrayBrush;
        public Brush SuaBtnBrush => (Role != null && !Role.IsLocked && Role.CanEdit) ? RedBrush : GrayBrush;
        public Brush XoaBtnBrush => (Role != null && !Role.IsLocked && Role.CanDelete) ? RedBrush : GrayBrush;
        public Brush TatCaBtnBrush => (Role != null && !Role.IsLocked && Role.IsAll) ? RedBrush : GrayBrush;

        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
        private static readonly SolidColorBrush GrayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7E7E7E"));

        public TouchFunctionRoleVM(FunctionRoleItem role)
        {
            Role = role;
            if (Role != null)
            {
                Role.PropertyChanged += (s, e) => NotifyAllBrushes();
            }
        }

        public void NotifyAllBrushes()
        {
            OnPropertyChanged(nameof(KhoaBtnBrush));
            OnPropertyChanged(nameof(XemBtnBrush));
            OnPropertyChanged(nameof(ThemBtnBrush));
            OnPropertyChanged(nameof(SuaBtnBrush));
            OnPropertyChanged(nameof(XoaBtnBrush));
            OnPropertyChanged(nameof(TatCaBtnBrush));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class TouchReportRoleVM : INotifyPropertyChanged
    {
        public ReportRoleItem Role { get; set; }
        public string ReportId => Role?.ReportId ?? "";
        public string ReportName => Role?.ReportName ?? "";
        public string GroupName => Role?.GroupName ?? "";

        public Brush KhoaBtnBrush => (Role != null && Role.NotAllowed) ? RedBrush : GrayBrush;
        public Brush XemBtnBrush => (Role != null && Role.IsAllowed) ? RedBrush : GrayBrush;
        public Brush TatCaBtnBrush => (Role != null && Role.IsAllowed) ? RedBrush : GrayBrush;

        private static readonly SolidColorBrush RedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
        private static readonly SolidColorBrush GrayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7E7E7E"));

        public TouchReportRoleVM(ReportRoleItem role)
        {
            Role = role;
            if (Role != null)
            {
                Role.PropertyChanged += (s, e) => NotifyAllBrushes();
            }
        }

        public void NotifyAllBrushes()
        {
            OnPropertyChanged(nameof(KhoaBtnBrush));
            OnPropertyChanged(nameof(XemBtnBrush));
            OnPropertyChanged(nameof(TatCaBtnBrush));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
    #endregion

    public partial class TouchNguoiDungPhanQuyenWindow : Window
    {
        private ObservableCollection<TouchGroupItem> _groups = new ObservableCollection<TouchGroupItem>();
        private ObservableCollection<TouchUserItem> _users = new ObservableCollection<TouchUserItem>();
        private ObservableCollection<TouchCategoryItem> _funcCategories = new ObservableCollection<TouchCategoryItem>();
        private ObservableCollection<TouchCategoryItem> _reportCategories = new ObservableCollection<TouchCategoryItem>();

        private List<TouchFunctionRoleVM> _allFuncRoles = new List<TouchFunctionRoleVM>();
        private List<TouchReportRoleVM> _allReportRoles = new List<TouchReportRoleVM>();

        private TouchGroupItem _selectedGroup;
        private TouchUserItem _selectedUser;
        private TouchCategoryItem _selectedFuncCategory;
        private TouchCategoryItem _selectedReportCategory;

        public TouchNguoiDungPhanQuyenWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGroupsAsync();
            await LoadUsersAsync();
            await LoadCategoriesAsync();
            SelectGroup(_groups.FirstOrDefault());
            SetSubTabActive(true);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        #region Data Loading
        private async Task LoadGroupsAsync(string selectGroupId = null)
        {
            try
            {
                var list = await LocalPhanQuyenService.GetGroupUsersAsync();
                _groups.Clear();

                // Group "TẤT CẢ"
                var allGroup = new TouchGroupItem
                {
                    Group = new GroupUserItem { Id = "0", Name = "TẤT CẢ", Note = "Tất cả nhóm" }
                };
                _groups.Add(allGroup);

                foreach (var g in list)
                {
                    _groups.Add(new TouchGroupItem { Group = g });
                }

                IcGroups.ItemsSource = _groups;

                if (!string.IsNullOrEmpty(selectGroupId))
                {
                    var target = _groups.FirstOrDefault(x => x.Id == selectGroupId);
                    SelectGroup(target ?? _groups.FirstOrDefault());
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
                string groupId = (_selectedGroup != null && _selectedGroup.Id != "0") ? _selectedGroup.Id : null;
                var list = await LocalPhanQuyenService.GetUsersAsync(groupId);
                _users.Clear();

                foreach (var u in list)
                {
                    _users.Add(new TouchUserItem { User = u });
                }

                IcUsers.ItemsSource = _users;
                _selectedUser = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                // Function Categories
                var fCats = await LocalPhanQuyenService.GetFunctionCategoriesAsync();
                _funcCategories.Clear();
                foreach (var c in fCats)
                {
                    _funcCategories.Add(new TouchCategoryItem { Name = c.Name });
                }
                IcFuncCategories.ItemsSource = _funcCategories;
                SelectFuncCategory(_funcCategories.FirstOrDefault());

                // Report Categories
                var rTree = await LocalPhanQuyenService.GetReportCategoriesTreeAsync();
                _reportCategories.Clear();
                _reportCategories.Add(new TouchCategoryItem { Name = "TẤT CẢ" });
                foreach (var node in rTree)
                {
                    if (node.Id != "0" && !string.IsNullOrWhiteSpace(node.Name))
                    {
                        _reportCategories.Add(new TouchCategoryItem { Name = node.Name.ToUpper() });
                    }
                }
                IcReportCategories.ItemsSource = _reportCategories;
                SelectReportCategory(_reportCategories.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCategoriesAsync: " + ex.Message);
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                if (_selectedGroup == null || _selectedGroup.Id == "0")
                {
                    _allFuncRoles.Clear();
                    _allReportRoles.Clear();
                    IcFuncRoles.ItemsSource = null;
                    IcReportRoles.ItemsSource = null;
                    return;
                }

                string groupId = _selectedGroup.Id;

                // Function Roles
                var fRoles = await LocalPhanQuyenService.GetFunctionRolesAsync(groupId);
                _allFuncRoles = fRoles.Select(r => new TouchFunctionRoleVM(r)).ToList();
                ApplyFuncRolesFilter();

                // Report Roles
                var rRoles = await LocalPhanQuyenService.GetReportRolesAsync(groupId);
                _allReportRoles = rRoles.Select(r => new TouchReportRoleVM(r)).ToList();
                ApplyReportRolesFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải phân quyền: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFuncRolesFilter()
        {
            if (_allFuncRoles == null) return;
            string catName = _selectedFuncCategory?.Name;
            if (string.IsNullOrEmpty(catName) || catName.Equals("Tất cả", StringComparison.OrdinalIgnoreCase) || catName.Equals("TẤT CẢ", StringComparison.OrdinalIgnoreCase))
            {
                IcFuncRoles.ItemsSource = new ObservableCollection<TouchFunctionRoleVM>(_allFuncRoles);
            }
            else
            {
                var filtered = _allFuncRoles.Where(r => r.GroupName != null && r.GroupName.Equals(catName, StringComparison.OrdinalIgnoreCase)).ToList();
                IcFuncRoles.ItemsSource = new ObservableCollection<TouchFunctionRoleVM>(filtered);
            }
        }

        private void ApplyReportRolesFilter()
        {
            if (_allReportRoles == null) return;
            string catName = _selectedReportCategory?.Name;
            if (string.IsNullOrEmpty(catName) || catName.Equals("Tất cả", StringComparison.OrdinalIgnoreCase) || catName.Equals("TẤT CẢ", StringComparison.OrdinalIgnoreCase))
            {
                IcReportRoles.ItemsSource = new ObservableCollection<TouchReportRoleVM>(_allReportRoles);
            }
            else
            {
                var filtered = _allReportRoles.Where(r => r.GroupName != null && r.GroupName.Equals(catName, StringComparison.OrdinalIgnoreCase)).ToList();
                IcReportRoles.ItemsSource = new ObservableCollection<TouchReportRoleVM>(filtered);
            }
        }
        #endregion

        #region Selection Handlers
        private async void BtnGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TouchGroupItem groupItem)
            {
                SelectGroup(groupItem);
                await LoadUsersAsync();
                await LoadRolesAsync();
            }
        }

        private void SelectGroup(TouchGroupItem item)
        {
            _selectedGroup = item;
            foreach (var g in _groups)
            {
                g.IsSelected = (g == item);
            }

            if (item == null || item.Id == "0")
            {
                BorderPromptNoGroup.Visibility = Visibility.Visible;
                GridPermissionsContainer.Visibility = Visibility.Collapsed;
                PanelSubTabs.Visibility = Visibility.Collapsed;
            }
            else
            {
                BorderPromptNoGroup.Visibility = Visibility.Collapsed;
                GridPermissionsContainer.Visibility = Visibility.Visible;
                PanelSubTabs.Visibility = Visibility.Visible;
            }
        }

        private void UserCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is TouchUserItem userItem)
            {
                _selectedUser = userItem;
                foreach (var u in _users)
                {
                    u.IsSelected = (u == userItem);
                }
            }
        }

        private void BtnFuncCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TouchCategoryItem catItem)
            {
                SelectFuncCategory(catItem);
            }
        }

        private void SelectFuncCategory(TouchCategoryItem catItem)
        {
            _selectedFuncCategory = catItem;
            foreach (var c in _funcCategories)
            {
                c.IsSelected = (c == catItem);
            }
            ApplyFuncRolesFilter();
        }

        private void BtnReportCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TouchCategoryItem catItem)
            {
                SelectReportCategory(catItem);
            }
        }

        private void SelectReportCategory(TouchCategoryItem catItem)
        {
            _selectedReportCategory = catItem;
            foreach (var c in _reportCategories)
            {
                c.IsSelected = (c == catItem);
            }
            ApplyReportRolesFilter();
        }
        #endregion

        #region Sub-Tabs Handlers
        private void BtnTabQuyenSuDung_Click(object sender, RoutedEventArgs e)
        {
            SetSubTabActive(true);
        }

        private void BtnTabQuyenBaoCao_Click(object sender, RoutedEventArgs e)
        {
            SetSubTabActive(false);
        }

        private void SetSubTabActive(bool isQuyenSuDung)
        {
            if (isQuyenSuDung)
            {
                GridQuyenSuDung.Visibility = Visibility.Visible;
                GridQuyenBaoCao.Visibility = Visibility.Collapsed;

                BtnTabQuyenSuDung.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D84315")); // Active Orange/Red
                BtnTabQuyenSuDung.Foreground = Brushes.White;
                BtnTabQuyenSuDung.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF360C"));

                BtnTabQuyenBaoCao.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264A5A")); // Inactive Teal/Blue
                BtnTabQuyenBaoCao.Foreground = Brushes.White;
                BtnTabQuyenBaoCao.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C5E73"));
            }
            else
            {
                GridQuyenSuDung.Visibility = Visibility.Collapsed;
                GridQuyenBaoCao.Visibility = Visibility.Visible;

                BtnTabQuyenBaoCao.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D84315")); // Active Orange/Red
                BtnTabQuyenBaoCao.Foreground = Brushes.White;
                BtnTabQuyenBaoCao.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF360C"));

                BtnTabQuyenSuDung.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264A5A")); // Inactive Teal/Blue
                BtnTabQuyenSuDung.Foreground = Brushes.White;
                BtnTabQuyenSuDung.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C5E73"));
            }
        }
        #endregion

        #region Action Buttons Click & Save
        private async void BtnFuncAction_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedGroup.Id == "0") return;

            if (sender is Button btn && btn.DataContext is TouchFunctionRoleVM vm)
            {
                string tag = btn.Tag?.ToString() ?? "";
                switch (tag)
                {
                    case "Khoa":
                        vm.Role.IsLocked = true;
                        break;
                    case "Xem":
                        vm.Role.CanView = !vm.Role.CanView;
                        break;
                    case "Them":
                        vm.Role.CanAdd = !vm.Role.CanAdd;
                        break;
                    case "Sua":
                        vm.Role.CanEdit = !vm.Role.CanEdit;
                        break;
                    case "Xoa":
                        vm.Role.CanDelete = !vm.Role.CanDelete;
                        break;
                    case "TatCa":
                        vm.Role.IsAll = !vm.Role.IsAll;
                        break;
                }

                vm.NotifyAllBrushes();

                // Save to DB
                try
                {
                    var rolesToSave = _allFuncRoles.Select(x => x.Role);
                    await LocalPhanQuyenService.SaveFunctionRolesAsync(_selectedGroup.Id, rolesToSave);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi lưu quyền chức năng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnReportAction_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null || _selectedGroup.Id == "0") return;

            if (sender is Button btn && btn.DataContext is TouchReportRoleVM vm)
            {
                string tag = btn.Tag?.ToString() ?? "";
                switch (tag)
                {
                    case "Khoa":
                        vm.Role.NotAllowed = true;
                        break;
                    case "Xem":
                    case "TatCa":
                        vm.Role.IsAllowed = !vm.Role.IsAllowed;
                        break;
                }

                vm.NotifyAllBrushes();

                // Save to DB
                try
                {
                    var rolesToSave = _allReportRoles.Select(x => x.Role);
                    await LocalPhanQuyenService.SaveReportRolesAsync(_selectedGroup.Id, rolesToSave);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi lưu quyền báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Header Action Buttons (CRUD)
        private async void BtnQuanLyNhom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new ThemSuaNhomNguoiDungWindow();
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    string currentSelectedId = _selectedGroup?.Id;
                    await LoadGroupsAsync(currentSelectedId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi quản lý nhóm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new ThemSuaNguoiDungWindow();
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null || _selectedUser.User == null)
            {
                MessageBox.Show("Mời bạn chọn tài khoản người dùng cần chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var dlg = new ThemSuaNguoiDungWindow(_selectedUser.User);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sửa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null || _selectedUser.User == null)
            {
                MessageBox.Show("Mời bạn chọn tài khoản người dùng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản [{_selectedUser.TenHienThi}] không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await LocalPhanQuyenService.DeleteUserAsync(_selectedUser.Id);
                    await LoadUsersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string targetType = btn.Tag?.ToString() ?? "QuyenSuDung";
                var win = new QuanLyBar.Client.Views.CauHinhHeThong.ThietLapDinhDangThanhPhanWindow(targetType);
                win.Owner = this;
                win.ShowDialog();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
