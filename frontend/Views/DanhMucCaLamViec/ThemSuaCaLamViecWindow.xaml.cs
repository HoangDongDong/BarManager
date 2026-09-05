using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucCaLamViec
{
    public partial class ThemSuaCaLamViecWindow : Window
    {
        public event Action OnSaved;
        private string _id;
        private string _parentId;
        private string _itemType = "0";
        private List<CaLamViecTreeItem> _allList = new List<CaLamViecTreeItem>();
        private List<SImageViewModel> _images = new List<SImageViewModel>();
        private int _currentIndex = -1;

        public ThemSuaCaLamViecWindow(string id = null, string itemType = "0", string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _itemType = itemType ?? "0";
            _parentId = parentId ?? "";

            InitTimeDropdowns();

            Loaded += async (s, e) =>
            {
                await LoadImagesAsync();
                await LoadAllShiftsAsync();

                if (!string.IsNullOrEmpty(_id))
                {
                    Title = "CA LÀM VIỆC - SỬA";
                    TxtHeaderTitle.Text = "Ca làm việc";
                    await LoadDetailAsync(_id);
                }
                else
                {
                    Title = _itemType == "FOLDER" ? "THƯ MỤC CA LÀM VIỆC - THÊM MỚI" : "CA LÀM VIỆC - THÊM MỚI";
                    TxtHeaderTitle.Text = _itemType == "FOLDER" ? "Thư mục ca làm việc" : "Ca làm việc";
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

        private void InitTimeDropdowns()
        {
            var times = new List<string>();
            for (int h = 0; h < 24; h++)
            {
                times.Add($"{h:00}:00");
                times.Add($"{h:00}:30");
            }
            CboTuGio.ItemsSource = times;
            CboDenGio.ItemsSource = times;
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
                Console.WriteLine("Error loading images in ThemSuaCaLamViecWindow: " + ex.Message);
            }
        }

        private async Task LoadAllShiftsAsync()
        {
            _allList = await LocalCaLamViecService.GetCaLamViecFlatListAsync(false);
            if (!string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allList.FindIndex(x => x.Id == _id);
            }
            UpdateNavButtons();
        }

        private async Task LoadDetailAsync(string id)
        {
            var item = await LocalCaLamViecService.GetByIdAsync(id);
            if (item != null)
            {
                _id = item.Id;
                _itemType = item.ItemType ?? "0";
                _parentId = item.ParentId ?? "";

                TxtName.Text = item.Name ?? "";
                TxtTiLeLuong.Text = item.TiLeLuong > 0 ? item.TiLeLuong.ToString("G29") : "100";
                CboTuGio.Text = item.TuGio ?? "";
                CboDenGio.Text = item.DenGio ?? "";
                TxtNote.Text = item.Note ?? "";

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
            TxtTiLeLuong.Text = "100";
            CboTuGio.Text = "";
            CboDenGio.Text = "";
            TxtNote.Text = "";
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

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên ca làm việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            decimal.TryParse(TxtTiLeLuong.Text.Trim(), out decimal tiLeLuong);
            if (tiLeLuong <= 0) tiLeLuong = 100;

            string selectedImageId = (CboAnh.SelectedItem as SImageViewModel)?.Id ?? "";

            var item = new CaLamViecTreeItem
            {
                Id = _id,
                Name = name,
                Note = TxtNote.Text.Trim(),
                ParentId = _parentId,
                ItemType = _itemType,
                TiLeLuong = tiLeLuong,
                TuGio = CboTuGio.Text.Trim(),
                DenGio = CboDenGio.Text.Trim(),
                SimageId = selectedImageId
            };

            var (ok, error) = await LocalCaLamViecService.SaveCaLamViecAsync(item);
            if (ok)
            {
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lưu thông tin ca làm việc không thành công!\n" + (error ?? ""), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin ca làm việc thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllShiftsAsync();
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin ca làm việc thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllShiftsAsync();
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
