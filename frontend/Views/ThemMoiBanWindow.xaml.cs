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

            if (_banList != null && _currentBan.Id.HasValue)
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
            if (_currentBan.Id != null && _currentBan.Id > 0)
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
            if (string.IsNullOrWhiteSpace(TxtTenBan.Text))
            {
                MessageBox.Show("Vui lòng nhập tên bàn!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenBan.Focus();
                return;
            }

            _currentBan.Name = TxtTenBan.Text.Trim();
            _currentBan.Note = TxtGhiChu.Text.Trim();
            
            _currentBan.DkhuvucId = CmbKhuVuc.SelectedValue != null ? (int?)CmbKhuVuc.SelectedValue : null;
            _currentBan.DnhomhienthiId = CmbNhomHienThi.SelectedValue != null ? (int?)CmbNhomHienThi.SelectedValue : null;
            _currentBan.DloaiphongId = CmbLoaiPhong.SelectedValue != null ? (int?)CmbLoaiPhong.SelectedValue : null;

            bool success = false;
            if (_currentBan.Id == null || _currentBan.Id == 0)
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
                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                // Nếu muốn đóng form sau khi lưu thì mở dòng dưới, 
                // nhưng thường thì nút "Lưu" (không phải "Lưu & thoát") sẽ giữ form ở lại để tiếp tục sửa
                // DialogResult = true;
                // Close();
                
                // Cập nhật lại ID nếu là thêm mới
                if (_currentBan.Id != null) 
                {
                    this.Title = "BÀN - CHỈNH SỬA";
                }
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi lưu dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_banList != null && _currentIndex > 0)
            {
                _currentIndex--;
                var prevId = int.Parse(_banList[_currentIndex].Id);
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
                var nextId = int.Parse(_banList[_currentIndex].Id);
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
            _currentBan = new DBAN();
            _currentIndex = -1; // Not pointing to any existing item
            LoadDataToForm();
            UpdateNavigationButtons();
            TxtTenBan.Focus();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _isDataChanged;
            Close();
        }
    }
}
