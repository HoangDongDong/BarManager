using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemTangDiemWindow : Window
    {
        private KhachHangThanThietViewModel _customer;
        private readonly TangGiamDiemItem _editItem;
        private TangGiamDiemItem _currentItem;
        private bool _isNew;
        private List<KhachHangThanThietViewModel> _allCustomers = new List<KhachHangThanThietViewModel>();
        private List<TangGiamDiemItem> _ticketList = new List<TangGiamDiemItem>();
        private int _currentIndex = -1;

        public event Action OnSaved;

        public ThemTangDiemWindow(KhachHangThanThietViewModel customer, TangGiamDiemItem item = null)
        {
            InitializeComponent();
            _customer = customer;
            _editItem = item;
            _currentItem = item;
            _isNew = (item == null);

            Loaded += ThemTangDiemWindow_Loaded;
        }

        private async void ThemTangDiemWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();
            await LoadTicketListAsync();

            if (_isNew)
            {
                Title = "TĂNG GIẢM ĐIỂM - THÊM MỚI";
                DpNgay.SelectedDate = DateTime.Now;
                TxtSoPhieu.Text = await LocalKhachHangThanThietService.GenerateSoPhieuTangDiemAsync();
                TxtDiemTang.Text = "0";
                TxtDiemGiam.Text = "0";
                TxtLyDo.Text = "";
                TxtGhiChu.Text = "";

                if (_customer != null)
                {
                    CboKhachHang.SelectedValue = _customer.Id;
                }
                else if (_allCustomers.Count > 0)
                {
                    CboKhachHang.SelectedIndex = 0;
                }

                TxtDiemTang.Focus();
                TxtDiemTang.SelectAll();
            }
            else
            {
                DisplayTicket(_editItem);
            }
        }

        private async Task LoadTicketListAsync()
        {
            try
            {
                _ticketList = await LocalKhachHangThanThietService.GetAllTangGiamDiemAsync(_customer?.Id);
                if (_ticketList.Count == 0 && _customer != null)
                {
                    _ticketList = await LocalKhachHangThanThietService.GetAllTangGiamDiemAsync();
                }

                if (_currentItem != null)
                {
                    _currentIndex = _ticketList.FindIndex(x => x.Id == _currentItem.Id || x.SoPhieu == _currentItem.SoPhieu);
                }
                else if (!_isNew && _editItem != null)
                {
                    _currentIndex = _ticketList.FindIndex(x => x.Id == _editItem.Id || x.SoPhieu == _editItem.SoPhieu);
                }
                else
                {
                    _currentIndex = _ticketList.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTicketListAsync: " + ex.Message);
            }
        }

        private void DisplayTicket(TangGiamDiemItem item)
        {
            if (item == null) return;

            _isNew = false;
            _currentItem = item;
            Title = "TĂNG GIẢM ĐIỂM - CHỈNH SỬA";

            DpNgay.SelectedDate = item.Ngay ?? DateTime.Now;
            TxtSoPhieu.Text = item.SoPhieu;
            TxtDiemTang.Text = item.DiemTang?.ToString("G") ?? "0";
            TxtDiemGiam.Text = item.DiemGiam?.ToString("G") ?? "0";
            TxtLyDo.Text = item.LyDo ?? "";
            TxtGhiChu.Text = item.GhiChu ?? "";

            if (!string.IsNullOrEmpty(item.DkhachhangId))
            {
                CboKhachHang.SelectedValue = item.DkhachhangId;
            }
            else if (_customer != null)
            {
                CboKhachHang.SelectedValue = _customer.Id;
            }
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                _allCustomers = await LocalKhachHangThanThietService.GetKhachHangThanThietListAsync("ALL", "");
                CboKhachHang.ItemsSource = _allCustomers;
                CboKhachHang.DisplayMemberPath = "Name";
                CboKhachHang.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCustomersAsync: " + ex.Message);
            }
        }

        private void CboKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboKhachHang.SelectedItem is KhachHangThanThietViewModel sel)
            {
                _customer = sel;
            }
        }

        private decimal ParseInputDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string clean = text.Trim().Replace(",", ".");
            if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                return val;
            if (decimal.TryParse(text.Trim(), out decimal val2))
                return val2;
            return 0;
        }

        private async Task<bool> SaveDataAsync()
        {
            string soPhieu = TxtSoPhieu.Text.Trim();
            if (string.IsNullOrEmpty(soPhieu))
            {
                MessageBox.Show("Vui lòng nhập số phiếu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoPhieu.Focus();
                return false;
            }

            decimal diemTang = ParseInputDecimal(TxtDiemTang.Text);
            decimal diemGiam = ParseInputDecimal(TxtDiemGiam.Text);

            if (diemTang <= 0 && diemGiam <= 0)
            {
                MessageBox.Show("Vui lòng nhập Điểm tăng hoặc Điểm giảm lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDiemTang.Focus();
                return false;
            }

            string khachId = "";
            if (CboKhachHang.SelectedValue != null)
            {
                khachId = CboKhachHang.SelectedValue.ToString();
            }
            else if (_customer != null)
            {
                khachId = _customer.Id;
            }

            var item = new TangGiamDiemItem
            {
                Id = _isNew ? null : (_currentItem?.Id ?? _editItem?.Id),
                Ngay = DpNgay.SelectedDate ?? DateTime.Now,
                SoPhieu = soPhieu,
                GhiChu = TxtGhiChu.Text?.Trim() ?? "",
                DiemTang = diemTang > 0 ? diemTang : (decimal?)null,
                DiemGiam = diemGiam > 0 ? diemGiam : (decimal?)null,
                LyDo = TxtLyDo.Text?.Trim() ?? "",
                DkhachhangId = khachId
            };

            var (success, errorMsg) = await LocalKhachHangThanThietService.SaveTangGiamDiemAsync(item, _isNew);
            if (success)
            {
                OnSaved?.Invoke();
                await LoadTicketListAsync();
                _currentIndex = _ticketList.FindIndex(x => x.SoPhieu == soPhieu);
                if (_currentIndex >= 0)
                {
                    _currentItem = _ticketList[_currentIndex];
                }
                _isNew = false;
                Title = "TĂNG GIẢM ĐIỂM - CHỈNH SỬA";
                return true;
            }
            else
            {
                MessageBox.Show($"Có lỗi xảy ra khi lưu phiếu tặng điểm:\n{errorMsg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu thông tin tăng giảm điểm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _isNew = false;
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

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ResetFormForNew()
        {
            _isNew = true;
            _currentItem = null;
            if (_ticketList != null)
            {
                _currentIndex = _ticketList.Count;
            }
            Title = "TĂNG GIẢM ĐIỂM - THÊM MỚI";
            DpNgay.SelectedDate = DateTime.Now;
            TxtSoPhieu.Text = await LocalKhachHangThanThietService.GenerateSoPhieuTangDiemAsync();
            TxtDiemTang.Text = "0";
            TxtDiemGiam.Text = "0";
            TxtLyDo.Text = "";
            TxtGhiChu.Text = "";
            TxtDiemTang.Focus();
            TxtDiemTang.SelectAll();
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ResetFormForNew();
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

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_ticketList == null || _ticketList.Count == 0)
            {
                await LoadTicketListAsync();
                if (_ticketList.Count == 0)
                {
                    MessageBox.Show("Chưa có phiếu tăng giảm điểm nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _ticketList.Count - 1;
            }

            DisplayTicket(_ticketList[_currentIndex]);
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_ticketList == null || _ticketList.Count == 0)
            {
                await LoadTicketListAsync();
                if (_ticketList.Count == 0)
                {
                    MessageBox.Show("Chưa có phiếu tăng giảm điểm nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_currentIndex < _ticketList.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            DisplayTicket(_ticketList[_currentIndex]);
        }

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
