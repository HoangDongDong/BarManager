using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NhanSu
{
    public partial class ThemSuaBangLuongWindow : Window
    {
        private string _id;
        private bool _isNew = true;
        private List<BangLuongItemViewModel> _allRecords = new List<BangLuongItemViewModel>();
        private int _currentIndex = -1;

        public event Action OnSaved;
        public bool IsSaved { get; private set; } = false;
        public string SavedId => _id;

        public ThemSuaBangLuongWindow(string id = null)
        {
            InitializeComponent();
            _id = id;
            _isNew = string.IsNullOrEmpty(_id);

            Loaded += ThemSuaBangLuongWindow_Loaded;
            UpdateButtonsState();
        }

        private async void ThemSuaBangLuongWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllRecordsListAsync();

            if (!_isNew && !string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allRecords.FindIndex(x => x.Id == _id);
                if (_currentIndex >= 0)
                {
                    LoadRecordDataIntoForm(_allRecords[_currentIndex]);
                }
                else
                {
                    await LoadDetailAsync(_id);
                }
            }
            else
            {
                ClearForm();
            }

            TxtThang.Focus();
            TxtThang.SelectAll();
        }

        private async Task LoadAllRecordsListAsync()
        {
            try
            {
                _allRecords = await LocalChamCongService.GetBangLuongListAsync();
                UpdateNavButtons();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadAllRecordsList: " + ex.Message);
            }
        }

        private void UpdateNavButtons()
        {
            if (_allRecords == null || _allRecords.Count == 0 || _currentIndex < 0)
            {
                BtnTruoc.IsEnabled = false;
                BtnSau.IsEnabled = false;
                return;
            }

            BtnTruoc.IsEnabled = _currentIndex > 0;
            BtnSau.IsEnabled = _currentIndex < _allRecords.Count - 1;
        }

        private void UpdateButtonsState()
        {
            if (BtnTruoc != null) BtnTruoc.IsEnabled = !_isNew && _currentIndex > 0;
            if (BtnSau != null) BtnSau.IsEnabled = !_isNew && _currentIndex >= 0 && _currentIndex < _allRecords.Count - 1;
            if (BtnXoa != null) BtnXoa.IsEnabled = !_isNew;
        }

        public void ClearForm()
        {
            _id = null;
            _isNew = true;
            Title = "BẢNG LƯƠNG - THÊM MỚI";

            DateTime now = DateTime.Now;
            TxtThang.Text = now.Month.ToString();
            TxtNam.Text = now.Year.ToString();
            UpdateSoPhieuText();
            TxtGhiChu.Text = "";

            UpdateNavButtons();
            UpdateButtonsState();
        }

        private void LoadRecordDataIntoForm(BangLuongItemViewModel item)
        {
            if (item == null) return;

            _id = item.Id;
            _isNew = false;
            Title = "BẢNG LƯƠNG - CHỈNH SỬA";

            TxtThang.Text = string.IsNullOrEmpty(item.Thang) ? DateTime.Now.Month.ToString() : item.Thang;
            TxtNam.Text = string.IsNullOrEmpty(item.Nam) ? DateTime.Now.Year.ToString() : item.Nam;
            TxtSoPhieu.Text = item.Name ?? "";
            TxtGhiChu.Text = item.Note ?? "";

            UpdateNavButtons();
            UpdateButtonsState();
        }

        private async Task LoadDetailAsync(string id)
        {
            try
            {
                var item = await LocalChamCongService.GetBangLuongByIdAsync(id);
                if (item != null)
                {
                    LoadRecordDataIntoForm(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết bảng lương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSoPhieuText()
        {
            if (TxtThang == null || TxtNam == null || TxtSoPhieu == null) return;
            string t = TxtThang.Text.Trim();
            string n = TxtNam.Text.Trim();
            if (string.IsNullOrEmpty(t)) t = DateTime.Now.Month.ToString();
            if (string.IsNullOrEmpty(n)) n = DateTime.Now.Year.ToString();

            TxtSoPhieu.Text = $"Tháng {t}/{n}";
        }

        private void TxtThangNam_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSoPhieuText();
        }

        private async Task<bool> SaveDataAsync()
        {
            if (!int.TryParse(TxtThang.Text.Trim(), out int thang) || thang < 1 || thang > 12)
            {
                MessageBox.Show("Tháng phải là số từ 1 đến 12!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtThang.Focus();
                TxtThang.SelectAll();
                return false;
            }

            if (!int.TryParse(TxtNam.Text.Trim(), out int nam) || nam < 2000 || nam > 2100)
            {
                MessageBox.Show("Năm không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNam.Focus();
                TxtNam.SelectAll();
                return false;
            }

            string soPhieu = TxtSoPhieu.Text.Trim();
            string ghiChu = TxtGhiChu.Text.Trim();

            var (ok, error, savedId) = await LocalChamCongService.SaveBangLuongAsync(_id, soPhieu, thang, nam, ghiChu);

            if (ok)
            {
                _id = savedId;
                _isNew = false;
                IsSaved = true;
                OnSaved?.Invoke();
                await LoadAllRecordsListAsync();
                _currentIndex = _allRecords.FindIndex(x => x.Id == _id);
                UpdateNavButtons();
                UpdateButtonsState();
                return true;
            }
            else
            {
                MessageBox.Show("Lỗi lưu thông tin bảng lương: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin bảng lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin bảng lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void MiSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            _isNew = true;
            Title = "BẢNG LƯƠNG - THÊM MỚI (SAO CHÉP)";
            UpdateNavButtons();
            UpdateButtonsState();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_id)) return;
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa bảng lương '{TxtSoPhieu.Text}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalChamCongService.DeleteBangLuongAsync(_id);
                if (ok)
                {
                    MessageBox.Show("Đã xóa bảng lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsSaved = true;
                    OnSaved?.Invoke();
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Xóa không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnPhimTat_Click(object sender, RoutedEventArgs e)
        {
            if (CmPhimTat != null)
            {
                CmPhimTat.PlacementTarget = BtnPhimTat;
                CmPhimTat.IsOpen = true;
            }
        }

        private void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_allRecords != null && _currentIndex > 0)
            {
                _currentIndex--;
                LoadRecordDataIntoForm(_allRecords[_currentIndex]);
            }
        }

        private void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_allRecords != null && _currentIndex >= 0 && _currentIndex < _allRecords.Count - 1)
            {
                _currentIndex++;
                LoadRecordDataIntoForm(_allRecords[_currentIndex]);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BtnThoat_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F10)
            {
                if (BtnTruoc.IsEnabled) BtnTruoc_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                if (BtnSau.IsEnabled) BtnSau_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F2 || (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L))
            {
                BtnLuu_Click(null, null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                BtnLuuVaMoi_Click(null, null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                BtnTaoMoi_Click(null, null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.C)
            {
                MiSaoChep_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
