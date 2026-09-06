using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NguoiDungPhanQuyen
{
    public partial class ThemSuaNguoiDungWindow : Window
    {
        public UserAccountItem UserItem { get; private set; }
        private string _preselectedGroupId;
        private bool _isEdit = false;
        private List<UserAccountItem> _allUsers = new List<UserAccountItem>();
        private int _currentIndex = -1;
        private ObservableCollection<UserShopItem> _userShops = new ObservableCollection<UserShopItem>();
        private List<GroupUserItem> _groups = new List<GroupUserItem>();
        private List<NhanVienTreeItem> _nhanViens = new List<NhanVienTreeItem>();

        private bool _isPasswordVisible = false;
        private bool _isSyncingPassword = false;

        public ThemSuaNguoiDungWindow(UserAccountItem item = null, string defaultGroupId = null, List<UserAccountItem> allUsers = null)
        {
            InitializeComponent();
            UserItem = item;
            _preselectedGroupId = defaultGroupId;
            _allUsers = allUsers ?? new List<UserAccountItem>();

            if (UserItem != null && _allUsers.Count > 0)
            {
                _currentIndex = _allUsers.FindIndex(u => u.Id == UserItem.Id);
            }

            Loaded += ThemSuaNguoiDungWindow_Loaded;
        }

        private async void ThemSuaNguoiDungWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Load danh sách nhóm người dùng
                _groups = await LocalPhanQuyenService.GetGroupUsersAsync();
                CboGroup.ItemsSource = _groups;

                // 2. Load danh sách nhân viên
                var nvList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                _nhanViens = new List<NhanVienTreeItem>();
                _nhanViens.Add(new NhanVienTreeItem { Id = "", Name = "-- Không chọn --" });
                _nhanViens.AddRange(nvList.Where(n => !string.IsNullOrEmpty(n.Name)));
                CboNhanVien.ItemsSource = _nhanViens;

                await PopulateFormDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PopulateFormDataAsync()
        {
            _isPasswordVisible = false;
            PbPassword.Visibility = Visibility.Visible;
            TxtPassword.Visibility = Visibility.Collapsed;
            TxtEyeIcon.Text = "👁️";

            if (UserItem != null && !string.IsNullOrEmpty(UserItem.Id))
            {
                _isEdit = true;
                Title = "TÀI KHOẢN NGƯỜI DÙNG - CHỈNH SỬA";
                TxtHeaderTitle.Text = "Tài khoản người dùng";
                TxtUsername.Text = UserItem.Username;
                TxtUsername.IsEnabled = false;

                _isSyncingPassword = true;
                PbPassword.Password = UserItem.Password ?? "";
                TxtPassword.Text = UserItem.Password ?? "";
                _isSyncingPassword = false;

                TxtFullName.Text = UserItem.Name;
                TxtEmail.Text = UserItem.Email;
                TxtUserId.Text = UserItem.UserId;
                TxtCardCode.Text = UserItem.CardCode;
                PickerAnh.SelectedSimageId = UserItem.SimageId;

                if (!string.IsNullOrEmpty(UserItem.SgroupuserId))
                {
                    CboGroup.SelectedValue = UserItem.SgroupuserId;
                }
                else if (_groups.Count > 0)
                {
                    CboGroup.SelectedIndex = 0;
                }

                if (!string.IsNullOrEmpty(UserItem.DnhanvienId))
                {
                    CboNhanVien.SelectedValue = UserItem.DnhanvienId;
                }
                else
                {
                    CboNhanVien.SelectedIndex = 0;
                }

                // Load Cửa hàng truy cập
                var shops = await LocalPhanQuyenService.GetUserShopAccessListAsync(UserItem.Id);
                _userShops = new ObservableCollection<UserShopItem>(shops);
                DgUserShops.ItemsSource = _userShops;
            }
            else
            {
                _isEdit = false;
                Title = "TÀI KHOẢN NGƯỜI DÙNG - THÊM MỚI";
                TxtHeaderTitle.Text = "Tài khoản người dùng";
                TxtUsername.Text = "";
                TxtUsername.IsEnabled = true;

                _isSyncingPassword = true;
                PbPassword.Password = "";
                TxtPassword.Text = "";
                _isSyncingPassword = false;

                TxtFullName.Text = "";
                TxtEmail.Text = "";
                TxtUserId.Text = "";
                TxtCardCode.Text = "";
                PickerAnh.SelectedSimageId = null;

                if (!string.IsNullOrEmpty(_preselectedGroupId) && _preselectedGroupId != "0")
                {
                    CboGroup.SelectedValue = _preselectedGroupId;
                }
                else if (_groups.Count > 0)
                {
                    CboGroup.SelectedIndex = 0;
                }

                CboNhanVien.SelectedIndex = 0;

                // Load Cửa hàng truy cập mặc định cho user mới (tất cả true)
                var shops = await LocalPhanQuyenService.GetUserShopAccessListAsync(null);
                _userShops = new ObservableCollection<UserShopItem>(shops);
                DgUserShops.ItemsSource = _userShops;

                TxtUsername.Focus();
            }
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingPassword) return;
            _isSyncingPassword = true;
            TxtPassword.Text = PbPassword.Password;
            _isSyncingPassword = false;
        }

        private void TxtPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingPassword) return;
            _isSyncingPassword = true;
            PbPassword.Password = TxtPassword.Text;
            _isSyncingPassword = false;
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                TxtPassword.Text = PbPassword.Password;
                PbPassword.Visibility = Visibility.Collapsed;
                TxtPassword.Visibility = Visibility.Visible;
                TxtEyeIcon.Text = "🙈";
                TxtPassword.Focus();
                TxtPassword.CaretIndex = TxtPassword.Text.Length;
            }
            else
            {
                PbPassword.Password = TxtPassword.Text;
                TxtPassword.Visibility = Visibility.Collapsed;
                PbPassword.Visibility = Visibility.Visible;
                TxtEyeIcon.Text = "👁️";
                PbPassword.Focus();
            }
        }

        private void CboNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboNhanVien.SelectedItem is NhanVienTreeItem selectedNv && !string.IsNullOrEmpty(selectedNv.Id))
            {
                if (string.IsNullOrWhiteSpace(TxtFullName.Text))
                {
                    TxtFullName.Text = selectedNv.Name;
                }
            }
        }

        private async Task<bool> SaveCurrentDataAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản (Tên đăng nhập).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUsername.Focus();
                return false;
            }

            try
            {
                if (UserItem == null)
                {
                    UserItem = new UserAccountItem();
                }

                string currentPass = _isPasswordVisible ? (TxtPassword.Text?.Trim() ?? "") : (PbPassword.Password?.Trim() ?? "");

                UserItem.Username = TxtUsername.Text.Trim();
                UserItem.Password = currentPass;
                UserItem.Name = string.IsNullOrWhiteSpace(TxtFullName.Text) ? TxtUsername.Text.Trim() : TxtFullName.Text.Trim();
                UserItem.Email = TxtEmail.Text?.Trim() ?? "";
                UserItem.UserId = TxtUserId.Text?.Trim() ?? "";
                UserItem.CardCode = TxtCardCode.Text?.Trim() ?? "";
                UserItem.SgroupuserId = CboGroup.SelectedValue?.ToString() ?? "";
                UserItem.DnhanvienId = CboNhanVien.SelectedValue?.ToString() ?? "";
                UserItem.SimageId = PickerAnh.SelectedSimageId;

                bool success = await LocalPhanQuyenService.SaveUserAsync(UserItem, _userShops);
                return success;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveCurrentDataAsync();
            if (success)
            {
                MessageBox.Show("Lưu dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _isEdit = true;
                TxtUsername.IsEnabled = false;
                Title = "TÀI KHOẢN NGƯỜI DÙNG - CHỈNH SỬA";
            }
        }

        private async void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveCurrentDataAsync();
            if (success)
            {
                UserItem = null;
                await PopulateFormDataAsync();
            }
        }

        private async void BtnSaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveCurrentDataAsync();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            UserItem = null;
            await PopulateFormDataAsync();
        }

        private async void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_allUsers != null && _allUsers.Count > 0 && _currentIndex > 0)
            {
                _currentIndex--;
                UserItem = _allUsers[_currentIndex];
                await PopulateFormDataAsync();
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_allUsers != null && _allUsers.Count > 0 && _currentIndex < _allUsers.Count - 1)
            {
                _currentIndex++;
                UserItem = _allUsers[_currentIndex];
                await PopulateFormDataAsync();
            }
        }
    }
}
