using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemTheTraTruocWindow : Window
    {
        private TheTraTruocViewModel _currentItem;
        private readonly TheTraTruocViewModel _editItem;
        private bool _isNew;
        private string _selectedNhomId;
        private List<TheTraTruocViewModel> _cardList = new List<TheTraTruocViewModel>();
        private int _currentIndex = -1;

        private ObservableCollection<TheTraTruocThuChiItem> _napTienList = new ObservableCollection<TheTraTruocThuChiItem>();
        private List<TheTraTruocThuChiItem> _allNapTienList = new List<TheTraTruocThuChiItem>();

        private ObservableCollection<TheTraTruocHoaDonItem> _suDungList = new ObservableCollection<TheTraTruocHoaDonItem>();
        private List<TheTraTruocHoaDonItem> _allSuDungList = new List<TheTraTruocHoaDonItem>();

        public event Action OnSaved;

        public ThemTheTraTruocWindow(TheTraTruocViewModel item = null, string defaultNhomId = null)
        {
            InitializeComponent();
            _editItem = item;
            _currentItem = item;
            _isNew = (item == null);
            _selectedNhomId = defaultNhomId;

            DgLichSuNapTien.ItemsSource = _napTienList;
            DgLichSuSuDung.ItemsSource = _suDungList;

            Loaded += ThemTheTraTruocWindow_Loaded;
        }

        private async void ThemTheTraTruocWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNhomTheListAsync();
            await LoadCardListAsync();

            if (_isNew)
            {
                ResetFormForNew();
            }
            else
            {
                await DisplayCardAsync(_editItem);
            }
        }

        private async Task LoadNhomTheListAsync()
        {
            try
            {
                var tree = await LocalTheTraTruocService.GetNhomTheTraTruocTreeAsync();
                var flatList = new List<NhomTheTraTruocTreeItem>();

                void Flatten(IEnumerable<NhomTheTraTruocTreeItem> items)
                {
                    foreach (var it in items)
                    {
                        if (it.Id != "TRASH" && it.Id != "UNSET")
                        {
                            flatList.Add(it);
                        }
                        if (it.Children != null && it.Children.Count > 0)
                        {
                            Flatten(it.Children);
                        }
                    }
                }

                Flatten(tree);

                CboNhomThe.ItemsSource = flatList;
                CboNhomThe.DisplayMemberPath = "Name";
                CboNhomThe.SelectedValuePath = "Id";

                if (!string.IsNullOrEmpty(_selectedNhomId) && flatList.Any(x => x.Id == _selectedNhomId))
                {
                    CboNhomThe.SelectedValue = _selectedNhomId;
                }
                else if (flatList.Count > 0)
                {
                    CboNhomThe.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadNhomTheListAsync: " + ex.Message);
            }
        }

        private async Task LoadCardListAsync()
        {
            try
            {
                _cardList = await LocalTheTraTruocService.GetTheTraTruocListAsync();

                if (_currentItem != null)
                {
                    _currentIndex = _cardList.FindIndex(x => x.Id == _currentItem.Id || x.MaThe == _currentItem.MaThe);
                }
                else if (!_isNew && _editItem != null)
                {
                    _currentIndex = _cardList.FindIndex(x => x.Id == _editItem.Id || x.MaThe == _editItem.MaThe);
                }
                else
                {
                    _currentIndex = _cardList.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCardListAsync: " + ex.Message);
            }
        }

        private async Task DisplayCardAsync(TheTraTruocViewModel item)
        {
            if (item == null) return;

            _isNew = false;
            _currentItem = item;
            Title = "THẺ TRẢ TRƯỚC - SỬA";

            BtnTruoc.IsEnabled = true;
            BtnSau.IsEnabled = true;
            BtnXoa.IsEnabled = true;

            TxtMaThe.Text = item.MaThe;
            ChkKhoa.IsChecked = item.Khoa;
            DpNgayHetHan.SelectedDate = item.NgayHetHan;
            TxtGhiChu.Text = item.GhiChu ?? "";

            if (!string.IsNullOrEmpty(item.DnhomthetratruocId))
            {
                CboNhomThe.SelectedValue = item.DnhomthetratruocId;
            }

            // Tải Lịch sử nạp tiền và Lịch sử sử dụng
            await LoadCardHistoryAsync(item.Id, item.MaThe);
        }

        private async Task LoadCardHistoryAsync(string theId, string maThe)
        {
            _napTienList.Clear();
            _allNapTienList.Clear();
            _suDungList.Clear();
            _allSuDungList.Clear();

            decimal tongNap = 0;
            decimal tongSuDung = 0;

            try
            {
                // Nạp tiền
                var napData = await LocalTheTraTruocService.GetLichSuThuChiTheTraTruocAsync(theId, maThe, 0);
                _allNapTienList = napData ?? new List<TheTraTruocThuChiItem>();
                foreach (var n in _allNapTienList)
                {
                    _napTienList.Add(n);
                    tongNap += n.SoTienThu;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error load nap tien: " + ex.Message);
            }

            try
            {
                // Sử dụng
                var suDungData = await LocalTheTraTruocService.GetLichSuHoaDonTheTraTruocAsync(theId, maThe);
                _allSuDungList = suDungData ?? new List<TheTraTruocHoaDonItem>();
                foreach (var s in _allSuDungList)
                {
                    _suDungList.Add(s);
                    tongSuDung += s.TongCong;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error load su dung: " + ex.Message);
            }

            decimal conLai = tongNap - tongSuDung;

            TxtTongNap.Text = tongNap.ToString("#,##0");
            TxtTongSuDung.Text = tongSuDung.ToString("#,##0");
            TxtConLai.Text = conLai.ToString("#,##0");
        }

        private void ResetFormForNew()
        {
            _isNew = true;
            _currentItem = null;
            if (_cardList != null)
            {
                _currentIndex = _cardList.Count;
            }
            Title = "THẺ TRẢ TRƯỚC - THÊM MỚI";

            BtnTruoc.IsEnabled = false;
            BtnSau.IsEnabled = false;
            BtnXoa.IsEnabled = false;

            TxtMaThe.Text = "";
            ChkKhoa.IsChecked = false;
            DpNgayHetHan.SelectedDate = null;
            TxtGhiChu.Text = "";

            _napTienList.Clear();
            _allNapTienList.Clear();
            _suDungList.Clear();
            _allSuDungList.Clear();

            TxtTongNap.Text = "0";
            TxtTongSuDung.Text = "0";
            TxtConLai.Text = "0";

            TxtMaThe.Focus();
        }

        private async Task<bool> SaveDataAsync()
        {
            string maThe = TxtMaThe.Text.Trim();
            if (string.IsNullOrEmpty(maThe))
            {
                MessageBox.Show("Vui lòng nhập Mã thẻ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMaThe.Focus();
                return false;
            }

            string nhomId = CboNhomThe.SelectedValue?.ToString();

            var item = new TheTraTruocViewModel
            {
                Id = _isNew ? null : (_currentItem?.Id ?? _editItem?.Id),
                MaThe = maThe,
                DnhomthetratruocId = nhomId,
                Khoa = ChkKhoa.IsChecked == true,
                NgayHetHan = DpNgayHetHan.SelectedDate,
                GhiChu = TxtGhiChu.Text?.Trim() ?? ""
            };

            var (success, errorMsg) = await LocalTheTraTruocService.SaveTheTraTruocAsync(item, _isNew);
            if (success)
            {
                OnSaved?.Invoke();
                await LoadCardListAsync();
                _currentIndex = _cardList.FindIndex(x => x.MaThe.Equals(maThe, StringComparison.OrdinalIgnoreCase));
                if (_currentIndex >= 0)
                {
                    _currentItem = _cardList[_currentIndex];
                }
                _isNew = false;
                Title = "THẺ TRẢ TRƯỚC - CHỈNH SỬA";
                return true;
            }
            else
            {
                MessageBox.Show($"Có lỗi xảy ra khi lưu thẻ trả trước:\n{errorMsg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu thông tin thẻ trả trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                ResetFormForNew();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e) => ResetFormForNew();

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_cardList == null || _cardList.Count == 0)
            {
                await LoadCardListAsync();
                if (_cardList.Count == 0)
                {
                    MessageBox.Show("Chưa có thẻ trả trước nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _cardList.Count - 1;
            }

            await DisplayCardAsync(_cardList[_currentIndex]);
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_cardList == null || _cardList.Count == 0)
            {
                await LoadCardListAsync();
                if (_cardList.Count == 0)
                {
                    MessageBox.Show("Chưa có thẻ trả trước nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_currentIndex < _cardList.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            await DisplayCardAsync(_cardList[_currentIndex]);
        }

        private void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _isNew = true;
            _currentItem = null;
            Title = "THẺ TRẢ TRƯỚC - THÊM MỚI";

            BtnTruoc.IsEnabled = false;
            BtnSau.IsEnabled = false;
            BtnXoa.IsEnabled = false;

            TxtMaThe.Text = TxtMaThe.Text + "_COPY";
            TxtMaThe.Focus();
            TxtMaThe.SelectAll();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null) return;

            var dr = MessageBox.Show($"Bạn có chắc chắn muốn xóa thẻ '{_currentItem.MaThe}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dr == MessageBoxResult.Yes)
            {
                bool ok = await LocalTheTraTruocService.DeleteTheTraTruocAsync(_currentItem.Id, false);
                if (ok)
                {
                    OnSaved?.Invoke();
                    await LoadCardListAsync();
                    if (_cardList.Count > 0)
                    {
                        _currentIndex = Math.Min(_currentIndex, _cardList.Count - 1);
                        await DisplayCardAsync(_cardList[_currentIndex]);
                    }
                    else
                    {
                        ResetFormForNew();
                    }
                }
            }
        }

        private void BtnNapTien_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Mở chức năng nạp tiền cho thẻ '{TxtMaThe.Text}'.", "Nạp tiền", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnChiTraTien_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Mở chức năng chi trả tiền cho thẻ '{TxtMaThe.Text}'.", "Chi trả tiền", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcelNap_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Xuất danh sách lịch sử nạp tiền ra Excel thành công!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcelDung_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Xuất danh sách lịch sử sử dụng ra Excel thành công!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TxtLocNap_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtLocNap.Text?.Trim().ToLower() ?? "";
            _napTienList.Clear();
            foreach (var item in _allNapTienList)
            {
                if (string.IsNullOrEmpty(kw) || (item.SoPhieu != null && item.SoPhieu.ToLower().Contains(kw)))
                {
                    _napTienList.Add(item);
                }
            }
        }

        private void TxtLocDung_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtLocDung.Text?.Trim().ToLower() ?? "";
            _suDungList.Clear();
            foreach (var item in _allSuDungList)
            {
                if (string.IsNullOrEmpty(kw) || 
                    (item.SoHoaDon != null && item.SoHoaDon.ToLower().Contains(kw)) ||
                    (item.Ban != null && item.Ban.ToLower().Contains(kw)))
                {
                    _suDungList.Add(item);
                }
            }
        }

        private void BtnPhimTat_Click(object sender, RoutedEventArgs e)
        {
            if (BtnPhimTat.ContextMenu != null)
            {
                BtnPhimTat.ContextMenu.PlacementTarget = BtnPhimTat;
                BtnPhimTat.ContextMenu.IsOpen = true;
            }
        }

        private void MiTaoMoi_Click(object sender, RoutedEventArgs e) => ResetFormForNew();
        private void MiLuu_Click(object sender, RoutedEventArgs e) => BtnLuu_Click(sender, e);
        private void MiLuuVaMoi_Click(object sender, RoutedEventArgs e) => BtnLuuVaMoi_Click(sender, e);
        private void MiLuuVaThoat_Click(object sender, RoutedEventArgs e) => BtnLuuVaThoat_Click(sender, e);
        private void MiThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuVaMoi_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuu_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ResetFormForNew();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuVaThoat_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.F10)
            {
                BtnTruoc_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnSau_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}
