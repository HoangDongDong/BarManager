using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemMoiBanWindow : Window
    {
        private DBAN _currentBan;
        private readonly LocalBanKhuVucService _service;
        private readonly List<BanViewModel> _banList;
        private int _currentIndex = -1;
        private bool _isDataChanged = false;

        public ThemMoiBanWindow(DBAN ban, List<BanViewModel> banList = null, int initialIndex = -1)
        {
            InitializeComponent();
            _currentBan = ban ?? new DBAN();
            _service = new LocalBanKhuVucService();
            _banList = banList;
            _currentIndex = initialIndex;

            if (_banList != null && _currentIndex == -1 && !string.IsNullOrEmpty(_currentBan.Id))
            {
                _currentIndex = _banList.FindIndex(b => b.Id == _currentBan.Id.ToString());
            }

            LoadDataToForm();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhuVucListAsync(_currentBan.DkhuvucId);
            await LoadNhomHienThiListAsync(_currentBan.DnhomhienthiId);
            await LoadLoaiPhongListAsync(_currentBan.DloaiphongId);

            UpdateNavigationButtons();
            TxtTenBan.Focus();
            TxtTenBan.SelectAll();
        }

        public async Task LoadKhuVucListAsync(string selectId = null, string selectName = null)
        {
            try
            {
                var list = await _service.GetLookupAsync("DKHUVUC");
                CmbKhuVuc.ItemsSource = list;
                if (!string.IsNullOrEmpty(selectId))
                {
                    CmbKhuVuc.SelectedValue = selectId;
                }
                else if (!string.IsNullOrEmpty(selectName))
                {
                    var found = list.FirstOrDefault(k => string.Equals(k.Name?.Trim(), selectName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        CmbKhuVuc.SelectedValue = found.Id;
                    }
                }
                else if (_currentBan.DkhuvucId != null)
                {
                    CmbKhuVuc.SelectedValue = _currentBan.DkhuvucId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi nạp danh sách Khu vực: " + ex.Message);
            }
        }

        public async Task LoadNhomHienThiListAsync(string selectId = null)
        {
            try
            {
                var list = await _service.GetLookupAsync("DNHOMHIENTHI");
                CmbNhomHienThi.ItemsSource = list;
                if (!string.IsNullOrEmpty(selectId))
                {
                    CmbNhomHienThi.SelectedValue = selectId;
                }
                else if (_currentBan.DnhomhienthiId != null)
                {
                    CmbNhomHienThi.SelectedValue = _currentBan.DnhomhienthiId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi nạp danh sách Nhóm hiển thị: " + ex.Message);
            }
        }

        public async Task LoadLoaiPhongListAsync(string selectId = null)
        {
            try
            {
                var list = await _service.GetLookupAsync("DLOAIPHONG");
                CmbLoaiPhong.ItemsSource = list;
                if (!string.IsNullOrEmpty(selectId))
                {
                    CmbLoaiPhong.SelectedValue = selectId;
                }
                else if (_currentBan.DloaiphongId != null)
                {
                    CmbLoaiPhong.SelectedValue = _currentBan.DloaiphongId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi nạp danh sách Loại phòng: " + ex.Message);
            }
        }

        private void LoadDataToForm()
        {
            if (_currentBan.Id != null && !string.IsNullOrEmpty(_currentBan.Id))
            {
                this.Title = "BÀN - SỬA";
                TxtHeaderTitle.Text = "Bàn";
                BtnXoa.IsEnabled = true;
            }
            else
            {
                this.Title = "BÀN - THÊM MỚI";
                TxtHeaderTitle.Text = "Bàn";
                BtnXoa.IsEnabled = false;
            }

            TxtTenBan.Text = _currentBan.Name ?? "";
            TxtGhiChu.Text = _currentBan.Note ?? "";
            
            CmbKhuVuc.SelectedValue = _currentBan.DkhuvucId;
            CmbNhomHienThi.SelectedValue = _currentBan.DnhomhienthiId;
            CmbLoaiPhong.SelectedValue = _currentBan.DloaiphongId;
        }

        private void TxtTenBan_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtTenBan.SelectAll();
        }

        private void UpdateNavigationButtons()
        {
            if (_banList == null || _banList.Count <= 1 || _currentIndex < 0)
            {
                BtnTruoc.IsEnabled = false;
                BtnSau.IsEnabled = false;
                return;
            }

            BtnTruoc.IsEnabled = _currentIndex > 0;
            BtnSau.IsEnabled = _currentIndex < _banList.Count - 1;
        }

        #region ComboBox Popup Actions (Khu Vuc, Nhom Hien Thi, Loai Phong)

        private async void BtnThemKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            CmbKhuVuc.IsDropDownOpen = false;
            var win = new ThemKhuVucWindow(false);
            if (win.ShowDialog() == true)
            {
                await LoadKhuVucListAsync(selectName: win.TenKhuVuc);
            }
        }

        private async void BtnTaiKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var curVal = CmbKhuVuc.SelectedValue?.ToString();
            await LoadKhuVucListAsync(curVal);
        }

        private void BtnDanhMucKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            CmbKhuVuc.IsDropDownOpen = false;
            MessageBox.Show("Để quản lý toàn bộ danh mục Khu vực & Bàn, vui lòng mở màn hình 'Danh mục Bàn & Khu vực'.", "Danh mục Khu vực", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnThemNhomHienThi_Click(object sender, RoutedEventArgs e)
        {
            CmbNhomHienThi.IsDropDownOpen = false;
            var inputWin = new InputWindow("Thêm nhóm hiển thị", "Nhập tên nhóm hiển thị mới:", "");
            if (inputWin.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputWin.InputText))
            {
                string newId = await _service.InsertNhomHienThiAsync(inputWin.InputText.Trim());
                if (!string.IsNullOrEmpty(newId))
                {
                    await LoadNhomHienThiListAsync(newId);
                }
            }
        }

        private async void BtnTaiNhomHienThi_Click(object sender, RoutedEventArgs e)
        {
            var curVal = CmbNhomHienThi.SelectedValue?.ToString();
            await LoadNhomHienThiListAsync(curVal);
        }

        private void BtnDanhMucNhomHienThi_Click(object sender, RoutedEventArgs e)
        {
            CmbNhomHienThi.IsDropDownOpen = false;
            MessageBox.Show("Danh mục Nhóm hiển thị dùng để phân nhóm giao diện bàn phòng.", "Danh mục Nhóm hiển thị", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnThemLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            CmbLoaiPhong.IsDropDownOpen = false;
            var inputWin = new InputWindow("Thêm loại phòng", "Nhập tên loại phòng mới:", "");
            if (inputWin.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputWin.InputText))
            {
                string newId = await _service.InsertLoaiPhongAsync(inputWin.InputText.Trim());
                if (!string.IsNullOrEmpty(newId))
                {
                    await LoadLoaiPhongListAsync(newId);
                }
            }
        }

        private async void BtnTaiLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            var curVal = CmbLoaiPhong.SelectedValue?.ToString();
            await LoadLoaiPhongListAsync(curVal);
        }

        private void BtnDanhMucLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            CmbLoaiPhong.IsDropDownOpen = false;
            MessageBox.Show("Danh mục Loại phòng dùng để phân biệt loại bàn/phòng trong hệ thống (VIP, Thường, Ngoài trời...).", "Danh mục Loại phòng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Save & Navigation

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentDataAsync(false);
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveCurrentDataAsync(true);
            if (success)
            {
                BtnTaoMoi_Click(sender, e);
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveCurrentDataAsync(true);
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }

        private async Task<bool> SaveCurrentDataAsync(bool suppressSuccessMessage)
        {
            if (string.IsNullOrWhiteSpace(TxtTenBan.Text))
            {
                MessageBox.Show("Vui lòng nhập tên bàn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenBan.Focus();
                return false;
            }

            _currentBan.Name = TxtTenBan.Text.Trim();
            _currentBan.Note = TxtGhiChu.Text.Trim();
            
            _currentBan.DkhuvucId = CmbKhuVuc.SelectedValue != null ? CmbKhuVuc.SelectedValue?.ToString() : null;
            _currentBan.DnhomhienthiId = CmbNhomHienThi.SelectedValue != null ? CmbNhomHienThi.SelectedValue?.ToString() : null;
            _currentBan.DloaiphongId = CmbLoaiPhong.SelectedValue != null ? CmbLoaiPhong.SelectedValue?.ToString() : null;

            bool success = false;
            if (_currentBan.Id == null || string.IsNullOrEmpty(_currentBan.Id))
            {
                success = await _service.InsertBanAsync(_currentBan);
            }
            else
            {
                success = await _service.UpdateBanAsync(_currentBan);
            }

            if (success)
            {
                _isDataChanged = true;
                if (!suppressSuccessMessage)
                {
                    MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                if (_currentBan.Id != null) 
                {
                    this.Title = "BÀN - SỬA";
                    BtnXoa.IsEnabled = true;
                }
                return true;
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi lưu dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_banList != null && _currentIndex > 0)
            {
                _currentIndex--;
                var prevId = _banList[_currentIndex].Id;
                TxtTenBan.Text = _banList[_currentIndex].Name ?? "";
                var ban = await _service.GetBanByIdAsync(prevId);
                if (ban != null)
                {
                    _currentBan = ban;
                    LoadDataToForm();
                }
                UpdateNavigationButtons();
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_banList != null && _currentIndex < _banList.Count - 1)
            {
                _currentIndex++;
                var nextId = _banList[_currentIndex].Id;
                TxtTenBan.Text = _banList[_currentIndex].Name ?? "";
                var ban = await _service.GetBanByIdAsync(nextId);
                if (ban != null)
                {
                    _currentBan = ban;
                    LoadDataToForm();
                }
                UpdateNavigationButtons();
            }
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            var previousKhuVucId = _currentBan?.DkhuvucId ?? (CmbKhuVuc.SelectedValue != null ? CmbKhuVuc.SelectedValue?.ToString() : null);
            var previousNhomHienThiId = _currentBan?.DnhomhienthiId ?? (CmbNhomHienThi.SelectedValue != null ? CmbNhomHienThi.SelectedValue?.ToString() : null);
            var previousLoaiPhongId = _currentBan?.DloaiphongId ?? (CmbLoaiPhong.SelectedValue != null ? CmbLoaiPhong.SelectedValue?.ToString() : null);

            _currentBan = new DBAN();
            _currentBan.DkhuvucId = previousKhuVucId;
            _currentBan.DnhomhienthiId = previousNhomHienThiId;
            _currentBan.DloaiphongId = previousLoaiPhongId;

            _currentIndex = -1;
            LoadDataToForm();
            UpdateNavigationButtons();
            
            this.Title = "BÀN - THÊM MỚI";
            TxtTenBan.Focus();
        }

        private void MenuSaoChep_Click(object sender, RoutedEventArgs e)
        {
            var previousKhuVucId = CmbKhuVuc.SelectedValue?.ToString();
            var previousNhomHienThiId = CmbNhomHienThi.SelectedValue?.ToString();
            var previousLoaiPhongId = CmbLoaiPhong.SelectedValue?.ToString();
            string oldName = TxtTenBan.Text.Trim();

            _currentBan = new DBAN();
            _currentBan.DkhuvucId = previousKhuVucId;
            _currentBan.DnhomhienthiId = previousNhomHienThiId;
            _currentBan.DloaiphongId = previousLoaiPhongId;
            _currentBan.Name = $"{oldName} (Bản sao)";

            _currentIndex = -1;
            LoadDataToForm();
            UpdateNavigationButtons();

            this.Title = "BÀN - THÊM MỚI";
            TxtTenBan.Focus();
            TxtTenBan.SelectAll();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentBan?.Id))
            {
                MessageBox.Show("Bàn này chưa được lưu, không thể xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa bàn '{_currentBan.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                bool deleted = await _service.DeleteBanAsync(_currentBan.Id);
                if (deleted)
                {
                    _isDataChanged = true;
                    MessageBox.Show("Xóa bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _isDataChanged;
            Close();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BtnThoat_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F10)
            {
                BtnTruoc_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnSau_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnTaoMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                MenuSaoChep_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                await SaveCurrentDataAsync(false);
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuVaMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.FocusedElement is not TextBox)
            {
                BtnLuuVaThoat_Click(sender, e);
                e.Handled = true;
            }
        }

        #endregion
    }
}
