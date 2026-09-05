using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucNhanVien
{
    public partial class ThemSuaNhanVienWindow : Window
    {
        public event Action OnSaved;
        private string _id;
        private string _parentId;
        private string _itemType = "0"; // 0: Item, FOLDER: Folder, SEPARATOR: Separator
        private List<NhanVienTreeItem> _allList = new List<NhanVienTreeItem>();
        private List<SImageViewModel> _images = new List<SImageViewModel>();
        private int _currentIndex = -1;

        public ThemSuaNhanVienWindow(string id = null, string itemType = "0", string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _itemType = itemType ?? "0";
            _parentId = parentId ?? "";

            Loaded += async (s, e) =>
            {
                await LoadImagesAsync();
                await LoadAllNhanVienAsync();

                if (!string.IsNullOrEmpty(_id))
                {
                    Title = "👩 NHÂN VIÊN - SỬA";
                    TxtHeaderTitle.Text = "Nhân viên";
                    await LoadDetailAsync(_id);
                }
                else
                {
                    Title = _itemType == "FOLDER" ? "📁 THƯ MỤC NHÂN VIÊN - THÊM MỚI" : "👩 NHÂN VIÊN - THÊM MỚI";
                    TxtHeaderTitle.Text = _itemType == "FOLDER" ? "Thư mục nhân viên" : "Nhân viên";
                    ClearForm();
                }
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F10)
                {
                    BtnPrevious_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F11)
                {
                    BtnNext_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F12)
                {
                    BtnTaoMoi_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
            };
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                var service = new LocalKhachDatHangService();
                _images = await service.GetSImagesAsync();
                CboAnh.ItemsSource = _images;
                if (_images.Count > 0) CboAnh.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading images in ThemSuaNhanVienWindow: " + ex.Message);
            }
        }

        private async Task LoadAllNhanVienAsync()
        {
            _allList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
            if (!string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allList.FindIndex(x => x.Id == _id);
            }
            UpdateNavButtons();
        }

        private async Task LoadDetailAsync(string id)
        {
            var item = await LocalNhanVienService.GetByIdAsync(id);
            if (item != null)
            {
                _id = item.Id;
                _itemType = item.ItemType ?? "0";
                _parentId = item.ParentId ?? "";

                TxtName.Text = item.Name ?? "";
                TxtDiaChi.Text = item.Diachi ?? "";
                TxtDienThoai.Text = item.Dienthoai ?? "";
                TxtNote.Text = item.Note ?? "";

                if (item.CachTinhLuong == 1)
                {
                    RbLuongThangTheoCa.IsChecked = true;
                }
                else if (item.CachTinhLuong == 2)
                {
                    RbLuongThangTheoNgay.IsChecked = true;
                }
                else
                {
                    RbLuongTheoCa.IsChecked = true;
                }

                TxtLuongCa.Text = item.LuongCa > 0 ? item.LuongCa.ToString("N0") : "60,000";
                TxtLuongThang.Text = item.LuongThang > 0 ? item.LuongThang.ToString("N0") : "0";

                ChkNghiThu7.IsChecked = item.NghiThu7 == 1;
                ChkNghiChuNhat.IsChecked = item.NghiChuNhat == 1;

                if (!string.IsNullOrEmpty(item.SimageId) && _images != null)
                {
                    var matched = _images.FirstOrDefault(x => x.Id == item.SimageId);
                    if (matched != null) CboAnh.SelectedItem = matched;
                }
            }
            UpdateNavButtons();
        }

        private void ClearForm()
        {
            _id = null;
            TxtName.Text = "";
            TxtDiaChi.Text = "";
            TxtDienThoai.Text = "";
            TxtNote.Text = "";
            TxtLuongCa.Text = "60,000";
            TxtLuongThang.Text = "0";
            RbLuongTheoCa.IsChecked = true;
            ChkNghiThu7.IsChecked = true;
            ChkNghiChuNhat.IsChecked = false;
            if (_images != null && _images.Count > 0) CboAnh.SelectedIndex = 0;
            TxtName.Focus();
        }

        private void UpdateNavButtons()
        {
            BtnPrevious.IsEnabled = _allList.Count > 0;
            BtnNext.IsEnabled = _allList.Count > 0;
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_allList.Count == 0) return;

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _allList.Count - 1;
            }

            await LoadDetailAsync(_allList[_currentIndex].Id);
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_allList.Count == 0) return;

            if (_currentIndex >= 0 && _currentIndex < _allList.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            await LoadDetailAsync(_allList[_currentIndex].Id);
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
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
            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            int cachTinhLuong = 0;
            if (RbLuongThangTheoCa.IsChecked == true) cachTinhLuong = 1;
            else if (RbLuongThangTheoNgay.IsChecked == true) cachTinhLuong = 2;

            decimal luongCa = ParseDecimal(TxtLuongCa.Text);
            decimal luongThang = ParseDecimal(TxtLuongThang.Text);

            int nghiThu7 = (ChkNghiThu7.IsChecked == true) ? 1 : 0;
            int nghiChuNhat = (ChkNghiChuNhat.IsChecked == true) ? 1 : 0;

            string selectedImageId = (CboAnh.SelectedItem as SImageViewModel)?.Id ?? "";

            var item = new NhanVienTreeItem
            {
                Id = _id,
                Name = name,
                Diachi = TxtDiaChi.Text.Trim(),
                Dienthoai = TxtDienThoai.Text.Trim(),
                Note = TxtNote.Text.Trim(),
                ParentId = _parentId,
                ItemType = _itemType,
                CachTinhLuong = cachTinhLuong,
                LuongCa = luongCa,
                LuongThang = luongThang,
                NghiThu7 = nghiThu7,
                NghiChuNhat = nghiChuNhat,
                SimageId = selectedImageId
            };

            bool ok = await LocalNhanVienService.SaveNhanVienAsync(item);
            if (ok)
            {
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lưu thông tin nhân viên không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllNhanVienAsync();
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllNhanVienAsync();
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

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
