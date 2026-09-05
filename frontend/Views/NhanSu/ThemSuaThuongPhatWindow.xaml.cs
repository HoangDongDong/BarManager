using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NhanSu
{
    public partial class ThemSuaThuongPhatWindow : Window
    {
        private string _id;
        private bool _isNew = true;
        private string _preselectedNhanVienId;
        private string _preselectedLyDoId;

        private List<ThuongPhatItemViewModel> _allRecords = new List<ThuongPhatItemViewModel>();
        private int _currentIndex = -1;

        public event Action OnSaved;
        public bool IsSaved { get; private set; } = false;

        public ThemSuaThuongPhatWindow(string id = null, string nhanVienId = null, string lyDoId = null)
        {
            InitializeComponent();
            _id = id;
            _preselectedNhanVienId = nhanVienId;
            _preselectedLyDoId = lyDoId;
            _isNew = string.IsNullOrEmpty(_id);

            Loaded += ThemSuaThuongPhatWindow_Loaded;
            UpdateButtonsState();
        }

        private async void ThemSuaThuongPhatWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
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
                await ClearFormAsync();
            }

            CboNhanVien.Focus();
        }

        private async Task LoadAllRecordsListAsync()
        {
            try
            {
                _allRecords = await LocalThuongPhatService.GetThuongPhatListAsync();
                UpdateNavButtons();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadAllRecordsListAsync: " + ex.Message);
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
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                // Employees
                var nvList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                CboNhanVien.ItemsSource = nvList;
                if (!string.IsNullOrEmpty(_preselectedNhanVienId))
                {
                    CboNhanVien.SelectedValue = _preselectedNhanVienId;
                }
                else if (nvList.Count > 0 && CboNhanVien.SelectedIndex < 0)
                {
                    CboNhanVien.SelectedIndex = 0;
                }

                // Reasons
                var lyDoList = await LocalThuongPhatService.GetLyDoThuongPhatFlatListAsync();
                CboLyDo.ItemsSource = lyDoList;
                if (!string.IsNullOrEmpty(_preselectedLyDoId))
                {
                    CboLyDo.SelectedValue = _preselectedLyDoId;
                }
                else if (lyDoList.Count > 0 && CboLyDo.SelectedIndex < 0)
                {
                    CboLyDo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading lookups: " + ex.Message);
            }
        }

        public async Task ClearFormAsync()
        {
            _id = null;
            _isNew = true;
            Title = "THƯỞNG PHẠT - THÊM MỚI";

            TxtSoPhieu.Text = await LocalThuongPhatService.GetNextSoPhieuAsync();
            DpNgay.SelectedDate = DateTime.Today;
            TxtThuong.Text = "0";
            TxtPhat.Text = "0";
            TxtGhiChu.Text = "";

            if (!string.IsNullOrEmpty(_preselectedNhanVienId))
            {
                CboNhanVien.SelectedValue = _preselectedNhanVienId;
            }
            else if (CboNhanVien.Items.Count > 0 && CboNhanVien.SelectedIndex < 0)
            {
                CboNhanVien.SelectedIndex = 0;
            }

            if (!string.IsNullOrEmpty(_preselectedLyDoId))
            {
                CboLyDo.SelectedValue = _preselectedLyDoId;
            }
            else if (CboLyDo.Items.Count > 0 && CboLyDo.SelectedIndex < 0)
            {
                CboLyDo.SelectedIndex = 0;
            }

            UpdateNavButtons();
            UpdateButtonsState();
        }

        private void LoadRecordDataIntoForm(ThuongPhatItemViewModel item)
        {
            if (item == null) return;

            _id = item.Id;
            _isNew = false;
            Title = "THƯỞNG PHẠT - CHỈNH SỬA";

            TxtSoPhieu.Text = item.SoPhieu ?? "";
            DpNgay.SelectedDate = item.Ngay ?? DateTime.Today;
            CboNhanVien.SelectedValue = item.DnhanvienId;
            CboLyDo.SelectedValue = item.DlydothuongphatId;

            TxtThuong.Text = (item.Thuong.HasValue && item.Thuong.Value > 0) ? item.Thuong.Value.ToString("N0", CultureInfo.InvariantCulture) : "0";
            TxtPhat.Text = (item.Phat.HasValue && item.Phat.Value > 0) ? item.Phat.Value.ToString("N0", CultureInfo.InvariantCulture) : "0";
            TxtGhiChu.Text = item.GhiChu ?? "";

            UpdateNavButtons();
            UpdateButtonsState();
        }

        private async Task LoadDetailAsync(string id)
        {
            try
            {
                var item = await LocalThuongPhatService.GetThuongPhatByIdAsync(id);
                if (item != null)
                {
                    LoadRecordDataIntoForm(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết thưởng phạt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FormatNumberTextBox(TextBox tb)
        {
            if (tb == null || string.IsNullOrWhiteSpace(tb.Text)) return;
            string raw = tb.Text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(raw, out decimal val))
            {
                int caret = tb.CaretIndex;
                int oldLen = tb.Text.Length;
                tb.TextChanged -= (tb == TxtThuong ? (TextChangedEventHandler)TxtThuong_TextChanged : TxtPhat_TextChanged);
                tb.Text = val.ToString("N0", CultureInfo.InvariantCulture);
                tb.TextChanged += (tb == TxtThuong ? (TextChangedEventHandler)TxtThuong_TextChanged : TxtPhat_TextChanged);
                int newLen = tb.Text.Length;
                tb.CaretIndex = Math.Max(0, caret + (newLen - oldLen));
            }
        }

        private void TxtThuong_TextChanged(object sender, TextChangedEventArgs e)
        {
            FormatNumberTextBox(TxtThuong);
        }

        private void TxtPhat_TextChanged(object sender, TextChangedEventArgs e)
        {
            FormatNumberTextBox(TxtPhat);
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string clean = text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(clean, out decimal val)) return val;
            return 0;
        }

        private async Task<bool> SaveDataAsync()
        {
            string soPhieu = TxtSoPhieu.Text.Trim();
            if (string.IsNullOrWhiteSpace(soPhieu))
            {
                MessageBox.Show("Vui lòng nhập số phiếu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoPhieu.Focus();
                return false;
            }

            string nhanVienId = (CboNhanVien.SelectedItem as NhanVienTreeItem)?.Id ?? CboNhanVien.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(nhanVienId))
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                CboNhanVien.Focus();
                return false;
            }

            decimal thuong = ParseDecimal(TxtThuong.Text);
            decimal phat = ParseDecimal(TxtPhat.Text);

            DateTime ngay = DpNgay.SelectedDate ?? DateTime.Today;
            string lyDoId = (CboLyDo.SelectedItem as LyDoThuongPhatTreeItem)?.Id ?? CboLyDo.SelectedValue?.ToString() ?? "";
            string ghiChu = TxtGhiChu.Text.Trim();

            var (ok, error, savedId) = await LocalThuongPhatService.SaveThuongPhatAsync(
                _id,
                soPhieu,
                ngay,
                nhanVienId,
                thuong,
                phat,
                lyDoId,
                ghiChu
            );

            if (ok)
            {
                _id = savedId;
                _isNew = false;
                IsSaved = true;
                OnSaved?.Invoke();
                await LoadAllRecordsListAsync();
                _currentIndex = _allRecords.FindIndex(x => x.Id == _id);
                UpdateNavButtons();
                return true;
            }
            else
            {
                MessageBox.Show("Lỗi lưu thông tin thưởng phạt: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin thưởng phạt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin thưởng phạt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await ClearFormAsync();
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

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            await ClearFormAsync();
        }

        private async void MiSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            _isNew = true;
            Title = "THƯỞNG PHẠT - THÊM MỚI (SAO CHÉP)";
            TxtSoPhieu.Text = await LocalThuongPhatService.GetNextSoPhieuAsync();
            UpdateNavButtons();
            UpdateButtonsState();
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
