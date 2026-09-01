using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemKhachHangWindow : Window
    {
        private string _id;
        private bool _isNew = true;
        private List<string> _allIds = new List<string>();
        private int _currentIndex = -1;
        public event Action OnSaved;

        public ThemKhachHangWindow(string id = null, IEnumerable<NhomKhachHangTreeItem> nhomList = null)
        {
            InitializeComponent();
            _id = id;
            _isNew = string.IsNullOrEmpty(id);

            Loaded += ThemKhachHangWindow_Loaded;
        }

        // Backward compatibility constructor if model was passed
        public ThemKhachHangWindow(KhachHangViewModel model, IEnumerable<NhomKhachHangTreeItem> nhomList = null)
            : this(model?.Id, nhomList)
        {
        }

        private async void ThemKhachHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
            _allIds = await LocalKhachHangService.GetAllKhachHangIdsAsync();

            if (!_isNew && !string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allIds.IndexOf(_id);
                await LoadKhachHangDataAsync(_id);
            }
            else
            {
                await ClearFormAsync();
            }
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                var nhomList = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                CboNhomKhach.ItemsSource = nhomList;

                var nhanVienList = await LocalKhachHangService.GetNhanVienLookupAsync();
                CboNhanVien.ItemsSource = nhanVienList;

                var tinhThanhList = await LocalKhachHangService.GetTinhThanhLookupAsync();
                CboTinhThanh.ItemsSource = tinhThanhList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadLookupsAsync: " + ex.Message);
            }
        }

        private async Task LoadKhachHangDataAsync(string id)
        {
            _id = id;
            _isNew = false;
            TxtHeaderTitle.Text = "Khách hàng";
            this.Title = "😀 KHÁCH HÀNG - SỬA";

            var data = await LocalKhachHangService.GetKhachHangByIdAsync(_id);
            if (data != null)
            {
                TxtMaKhach.Text = data.MAKHACH?.ToString() ?? "";
                TxtTenKhach.Text = data.NAME?.ToString() ?? "";
                TxtDiaChi.Text = data.DIACHI?.ToString() ?? "";
                TxtDienThoai.Text = data.DIENTHOAI?.ToString() ?? "";
                TxtEmail.Text = data.EMAIL?.ToString() ?? "";

                if (data.NGAYSINH != null)
                {
                    try
                    {
                        DpNgaySinh.SelectedDate = Convert.ToDateTime(data.NGAYSINH);
                    }
                    catch
                    {
                        DpNgaySinh.SelectedDate = null;
                    }
                }
                else
                {
                    DpNgaySinh.SelectedDate = null;
                }

                TxtDiemTichLuy.Text = (data.DIEMTICHLUYBANDAU != null ? Convert.ToDecimal(data.DIEMTICHLUYBANDAU) : 0).ToString("N0");
                TxtMaSoThue.Text = data.MASOTHUE?.ToString() ?? "";
                TxtFacebook.Text = data.FACEBOOK?.ToString() ?? "";
                TxtGhiChu.Text = data.NOTE?.ToString() ?? "";

                if (data.DNHOMKHACHHANGID != null)
                    CboNhomKhach.SelectedValue = data.DNHOMKHACHHANGID.ToString();
                else
                    CboNhomKhach.SelectedIndex = -1;

                if (data.DNHANVIENID != null)
                    CboNhanVien.SelectedValue = data.DNHANVIENID.ToString();
                else
                    CboNhanVien.SelectedIndex = -1;

                if (data.DTINHTHANHID != null)
                    CboTinhThanh.SelectedValue = data.DTINHTHANHID.ToString();
                else
                    CboTinhThanh.SelectedIndex = -1;
            }
        }

        private async Task ClearFormAsync()
        {
            _id = null;
            _isNew = true;
            TxtHeaderTitle.Text = "Khách hàng";
            this.Title = "😀 KHÁCH HÀNG - THÊM MỚI";

            TxtMaKhach.Text = await LocalKhachHangService.GetNextMaKhachAsync();
            TxtTenKhach.Text = "";
            TxtDiaChi.Text = "";
            TxtDienThoai.Text = "";
            TxtEmail.Text = "";
            DpNgaySinh.SelectedDate = null;
            TxtDiemTichLuy.Text = "0";
            TxtMaSoThue.Text = "";
            TxtFacebook.Text = "";
            TxtGhiChu.Text = "";

            CboNhomKhach.SelectedIndex = -1;
            CboNhanVien.SelectedIndex = -1;
            CboTinhThanh.SelectedIndex = -1;

            TxtTenKhach.Focus();
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

            await LoadKhachHangDataAsync(_allIds[_currentIndex]);
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

            await LoadKhachHangDataAsync(_allIds[_currentIndex]);
        }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            await ClearFormAsync();
        }

        private async void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            _isNew = true;
            TxtMaKhach.Text = await LocalKhachHangService.GetNextMaKhachAsync();
            this.Title = "😀 KHÁCH HÀNG - THÊM MỚI";
            TxtTenKhach.Focus();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!_isNew && !string.IsNullOrEmpty(_id))
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{TxtTenKhach.Text}'?", 
                                          "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = await LocalKhachHangService.DeleteKhachHangAsync(_id);
                    if (ok)
                    {
                        _allIds = await LocalKhachHangService.GetAllKhachHangIdsAsync();
                        if (_allIds.Count > 0)
                        {
                            if (_currentIndex >= _allIds.Count) _currentIndex = _allIds.Count - 1;
                            await LoadKhachHangDataAsync(_allIds[_currentIndex]);
                        }
                        else
                        {
                            await ClearFormAsync();
                        }
                        OnSaved?.Invoke();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa khách hàng này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Chỉ có thể xóa khi đang chỉnh sửa khách hàng có sẵn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void MiTaoMoi_Click(object sender, RoutedEventArgs e) => await ClearFormAsync();
        private void MiSaoChep_Click(object sender, RoutedEventArgs e) => BtnSaoChep_Click(sender, e);
        private async void MiLuu_Click(object sender, RoutedEventArgs e) => await SaveDataAsync();
        private async void MiLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                await ClearFormAsync();
            }
        }
        private async void MiLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                this.DialogResult = true;
                this.Close();
            }
        }
        private void MiThoat_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F10)
            {
                e.Handled = true;
                BtnTruoc_Click(sender, e);
            }
            else if (e.Key == Key.F11)
            {
                e.Handled = true;
                BtnSau_Click(sender, e);
            }
            else if (e.Key == Key.F12)
            {
                e.Handled = true;
                BtnTaoMoi_Click(sender, e);
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                MiTaoMoi_Click(sender, e);
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;
                MiSaoChep_Click(sender, e);
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                MiLuu_Click(sender, e);
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                MiLuuVaMoi_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtTenKhach.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập Tên khách hàng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenKhach.Focus();
                return false;
            }

            string makhach = TxtMaKhach.Text.Trim();
            if (string.IsNullOrEmpty(makhach))
            {
                makhach = await LocalKhachHangService.GetNextMaKhachAsync();
            }

            decimal diemTichLuy = 0;
            decimal.TryParse(TxtDiemTichLuy.Text.Replace(",", "").Replace(".", ""), out diemTichLuy);

            DateTime? ngaySinh = DpNgaySinh.SelectedDate;

            var model = new KhachHangViewModel
            {
                Id = _isNew ? Guid.NewGuid().ToString() : _id,
                Makhach = makhach,
                Name = name,
                Diachi = TxtDiaChi.Text.Trim(),
                Dienthoai = TxtDienThoai.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                Masothue = TxtMaSoThue.Text.Trim(),
                Facebook = TxtFacebook.Text.Trim(),
                Note = TxtGhiChu.Text.Trim(),
                Ngaysinh = ngaySinh,
                Diemtichluy = diemTichLuy,
                DnhomkhachhangId = CboNhomKhach.SelectedValue?.ToString(),
                TenNhanVien = CboNhanVien.SelectedValue?.ToString(),
                TinhThanh = CboTinhThanh.SelectedValue?.ToString()
            };

            bool ok = await LocalKhachHangService.SaveKhachHangAsync(model, _isNew);
            if (ok)
            {
                _id = model.Id;
                _isNew = false;
                this.Title = "😀 KHÁCH HÀNG - SỬA";

                _allIds = await LocalKhachHangService.GetAllKhachHangIdsAsync();
                _currentIndex = _allIds.IndexOf(_id);

                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu khách hàng. Vui lòng kiểm tra lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Lưu thông tin khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                await ClearFormAsync();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
