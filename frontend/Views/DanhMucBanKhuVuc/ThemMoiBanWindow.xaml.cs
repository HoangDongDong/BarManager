using System;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemMoiBanWindow : Window
    {
        private DBAN _currentBan;
        private readonly LocalBanKhuVucService _service;
        private readonly System.Collections.Generic.List<BanViewModel> _banList;
        private int _currentIndex = -1;
        private bool _isDataChanged = false;

        public ThemMoiBanWindow(DBAN ban, System.Collections.Generic.List<BanViewModel> banList = null)
        {
            InitializeComponent();
            _currentBan = ban ?? new DBAN();
            _service = new LocalBanKhuVucService();
            _banList = banList;

            if (_banList != null && !string.IsNullOrEmpty(_currentBan.Id))
            {
                _currentIndex = _banList.FindIndex(b => b.Id == _currentBan.Id.ToString());
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load lookups
            var khuVucList = await _service.GetLookupAsync("DKHUVUC");
            var nhomHienThiList = await _service.GetLookupAsync("DNHOMHIENTHI");
            var loaiPhongList = await _service.GetLookupAsync("DLOAIPHONG");

            CmbKhuVuc.ItemsSource = khuVucList;
            CmbNhomHienThi.ItemsSource = nhomHienThiList;
            CmbLoaiPhong.ItemsSource = loaiPhongList;

            // Bind data
            LoadDataToForm();
            UpdateNavigationButtons();
        }

        private void LoadDataToForm()
        {
            if (_currentBan.Id != null && !string.IsNullOrEmpty(_currentBan.Id))
            {
                this.Title = "BÀN - CHỈNH SỬA";
            }
            else
            {
                this.Title = "BÀN - THÊM MỚI";
            }

            TxtTenBan.Text = _currentBan.Name;
            TxtGhiChu.Text = _currentBan.Note;
            
            CmbKhuVuc.SelectedValue = _currentBan.DkhuvucId;
            CmbNhomHienThi.SelectedValue = _currentBan.DnhomhienthiId;
            CmbLoaiPhong.SelectedValue = _currentBan.DloaiphongId;
        }

        private void UpdateNavigationButtons()
        {
            if (_banList == null || _banList.Count == 0)
            {
                BtnTruoc.IsEnabled = false;
                BtnSau.IsEnabled = false;
                return;
            }

            BtnTruoc.IsEnabled = _currentIndex > 0;
            BtnSau.IsEnabled = _currentIndex >= 0 && _currentIndex < _banList.Count - 1;
        }

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
                
                // Cập nhật lại ID nếu là thêm mới
                if (_currentBan.Id != null) 
                {
                    this.Title = "BÀN - CHỈNH SỬA";
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
                var ban = await _service.GetBanByIdAsync(prevId);
                if (ban != null)
                {
                    _currentBan = ban;
                    LoadDataToForm();
                    UpdateNavigationButtons();
                }
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_banList != null && _currentIndex < _banList.Count - 1)
            {
                _currentIndex++;
                var nextId = _banList[_currentIndex].Id;
                var ban = await _service.GetBanByIdAsync(nextId);
                if (ban != null)
                {
                    _currentBan = ban;
                    LoadDataToForm();
                    UpdateNavigationButtons();
                }
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

            _currentIndex = -1; // Not pointing to any existing item
            LoadDataToForm();
            UpdateNavigationButtons();
            
            this.Title = "BÀN - THÊM MỚI";
            TxtTenBan.Focus();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _isDataChanged;
            Close();
        }
    }
}

