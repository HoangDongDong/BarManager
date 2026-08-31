using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhanVienWindow : Window
    {
        private string _id;
        private string _parentId;
        private bool _isNew = true;
        private List<string> _allIds = new List<string>();
        private int _currentIndex = -1;
        public event Action OnSaved;

        public ThemNhanVienWindow(string id = null, string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _parentId = parentId;
            _isNew = string.IsNullOrEmpty(id);

            Loaded += ThemNhanVienWindow_Loaded;
        }

        private List<SImageViewModel> _images = new();

        private async void ThemNhanVienWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _allIds = await LocalKhachHangService.GetAllNhanVienIdsAsync();

            try
            {
                var service = new LocalKhachDatHangService();
                _images = await service.GetSImagesAsync();
                CboAnh.ItemsSource = _images;
                if (_images.Count > 0) CboAnh.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading SImages in ThemNhanVienWindow: " + ex.Message);
            }

            if (!_isNew && !string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allIds.IndexOf(_id);
                await LoadNhanVienDataAsync(_id);
            }
            else
            {
                ClearForm();
            }
        }

        private async Task LoadNhanVienDataAsync(string id)
        {
            _id = id;
            _isNew = false;
            TxtHeaderTitle.Text = "Nhân viên";
            this.Title = "👩 NHÂN VIÊN - SỬA";

            var data = await LocalKhachHangService.GetNhanVienByIdAsync(_id);
            if (data != null)
            {
                TxtTenNhanVien.Text = data.NAME?.ToString() ?? "";
                TxtDiaChi.Text = data.DIACHI?.ToString() ?? "";
                TxtDienThoai.Text = data.DIENTHOAI?.ToString() ?? "";
                TxtGhiChu.Text = data.NOTE?.ToString() ?? "";

                string simageId = data.SIMAGEID?.ToString() ?? data.IMAGE?.ToString();
                if (!string.IsNullOrEmpty(simageId) && _images != null)
                {
                    var matched = _images.FirstOrDefault(x => x.Id == simageId);
                    if (matched != null) CboAnh.SelectedItem = matched;
                }

                int cachTinhLuong = data.CACHTINHLUONG != null ? Convert.ToInt32(data.CACHTINHLUONG) : 0;
                if (cachTinhLuong == 1)
                {
                    RbLuongThangTheoCa.IsChecked = true;
                }
                else if (cachTinhLuong == 2)
                {
                    RbLuongThangTheoNgay.IsChecked = true;
                }
                else
                {
                    RbLuongTheoCa.IsChecked = true;
                }

                TxtLuongCa.Text = (data.LUONGCA != null ? Convert.ToDecimal(data.LUONGCA) : 0).ToString("N0");
                TxtLuongThang.Text = (data.LUONGTHANG != null ? Convert.ToDecimal(data.LUONGTHANG) : 0).ToString("N0");

                ChkNghiThu7.IsChecked = (data.NGHITHU7 != null && Convert.ToInt32(data.NGHITHU7) == 1);
                ChkNghiChuNhat.IsChecked = (data.NGHICHUNHAT != null && Convert.ToInt32(data.NGHICHUNHAT) == 1);
            }
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_allIds.Count == 0) return;

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _allIds.Count - 1; // Loop back
            }

            await LoadNhanVienDataAsync(_allIds[_currentIndex]);
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_allIds.Count == 0) return;

            if (_currentIndex < _allIds.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0; // Loop front
            }

            await LoadNhanVienDataAsync(_allIds[_currentIndex]);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F10)
            {
                e.Handled = true;
                BtnTruoc_Click(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.F11)
            {
                e.Handled = true;
                BtnSau_Click(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.F12)
            {
                e.Handled = true;
                BtnTaoMoi_Click(sender, e);
            }
        }

        private void RbCachTinhLuong_Checked(object sender, RoutedEventArgs e)
        {
            // Toggle enabled/focus if needed
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtTenNhanVien.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập Tên nhân viên!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenNhanVien.Focus();
                return false;
            }

            string diachi = TxtDiaChi.Text.Trim();
            string dienthoai = TxtDienThoai.Text.Trim();
            string note = TxtGhiChu.Text.Trim();

            int cachTinhLuong = 0;
            if (RbLuongThangTheoCa.IsChecked == true) cachTinhLuong = 1;
            else if (RbLuongThangTheoNgay.IsChecked == true) cachTinhLuong = 2;

            decimal.TryParse(TxtLuongCa.Text.Trim(), out decimal luongCa);
            decimal.TryParse(TxtLuongThang.Text.Trim(), out decimal luongThang);

            int nghiThu7 = (ChkNghiThu7.IsChecked == true) ? 1 : 0;
            int nghiChuNhat = (ChkNghiChuNhat.IsChecked == true) ? 1 : 0;

            bool ok = await LocalKhachHangService.SaveNhanVienAsync(_id, name, diachi, dienthoai, cachTinhLuong, luongCa, luongThang, nghiThu7, nghiChuNhat, note, _isNew, _parentId);
            if (ok)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Không thể lưu nhân viên. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                MessageBox.Show("Lưu nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                MessageBox.Show("Lưu nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _id = Guid.NewGuid().ToString();
            _isNew = true;
            _currentIndex = -1;
            TxtHeaderTitle.Text = "Nhân viên";
            this.Title = "👩 NHÂN VIÊN - THÊM MỚI";
            TxtTenNhanVien.Text = "";
            TxtDiaChi.Text = "";
            TxtDienThoai.Text = "";
            TxtLuongCa.Text = "0";
            TxtLuongThang.Text = "0";
            RbLuongTheoCa.IsChecked = true;
            ChkNghiThu7.IsChecked = true;
            ChkNghiChuNhat.IsChecked = false;
            TxtGhiChu.Text = "";
            TxtTenNhanVien.Focus();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
