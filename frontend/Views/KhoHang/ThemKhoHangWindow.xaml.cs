using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.KhoHang
{
    public partial class ThemKhoHangWindow : Window
    {
        private KhoHangTreeItem _currentItem;
        private readonly KhoHangTreeItem _editItem;
        private bool _isNew;
        private List<KhoHangTreeItem> _warehouseList = new List<KhoHangTreeItem>();
        private int _currentIndex = -1;

        public event Action OnSaved;

        private List<SImageViewModel> _images = new List<SImageViewModel>();
        private List<KhoHangCuaHangItem> _stores = new List<KhoHangCuaHangItem>();
        private string _selectedStoreId;

        public ThemKhoHangWindow(KhoHangTreeItem item = null)
        {
            InitializeComponent();
            _editItem = item;
            _currentItem = item;
            _isNew = (item == null);

            Loaded += ThemKhoHangWindow_Loaded;
            UpdateButtonsState();
        }

        private async void ThemKhoHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadImagesAsync();
            await LoadCuaHangListAsync();
            await LoadWarehouseListAsync();

            if (_isNew)
            {
                ResetFormForNew();
            }
            else
            {
                DisplayWarehouse(_editItem);
            }
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                _images = await LocalKhoHangService.GetSImagesAsync();
                CboAnh.ItemsSource = _images;
                if (_images.Count > 0)
                {
                    var defaultImg = _images.FirstOrDefault(x => x.Id == "a38e42b9-aeda-4a67-8761-a5f4dc3571c1") ?? _images[0];
                    CboAnh.SelectedItem = defaultImg;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadImagesAsync: " + ex.Message);
            }
        }

        private async Task LoadCuaHangListAsync()
        {
            try
            {
                _stores = await LocalKhoHangService.GetCuaHangListAsync();
                LstCuaHang.ItemsSource = _stores;

                if (_stores.Count > 0)
                {
                    if (string.IsNullOrEmpty(_selectedStoreId) || !_stores.Any(x => x.Id == _selectedStoreId))
                    {
                        SelectStore(_stores[0]);
                    }
                    else
                    {
                        var cur = _stores.FirstOrDefault(x => x.Id == _selectedStoreId);
                        if (cur != null) SelectStore(cur);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCuaHangListAsync: " + ex.Message);
            }
        }

        private void SelectStore(KhoHangCuaHangItem store)
        {
            if (store == null) return;
            _selectedStoreId = store.Id;
            TxtSelectedCuaHang.Text = store.Name;
            LstCuaHang.SelectedItem = store;
        }

        private void TxtSelectedCuaHang_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PopupCuaHang.IsOpen = !PopupCuaHang.IsOpen;
        }

        private void LstCuaHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCuaHang.SelectedItem is KhoHangCuaHangItem store)
            {
                SelectStore(store);
                PopupCuaHang.IsOpen = false;
                BtnToggleCuaHang.IsChecked = false;
            }
        }

        private void BtnThemCuaHang_Click(object sender, RoutedEventArgs e)
        {
            PopupCuaHang.IsOpen = false;
            BtnToggleCuaHang.IsChecked = false;

            var win = new ThemCuaHangWindow();
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadCuaHangListAsync();
            };
            win.ShowDialog();
        }

        private async void BtnTaiCuaHang_Click(object sender, RoutedEventArgs e)
        {
            await LoadCuaHangListAsync();
        }

        private void BtnDanhMucCuaHang_Click(object sender, RoutedEventArgs e)
        {
            PopupCuaHang.IsOpen = false;
            BtnToggleCuaHang.IsChecked = false;
            // Mở thêm cửa hàng hoặc reload
            BtnThemCuaHang_Click(sender, e);
        }

        private void UpdateButtonsState()
        {
            bool hasValidName = !string.IsNullOrWhiteSpace(TxtTenKho?.Text);
            if (BtnLuu != null) BtnLuu.IsEnabled = hasValidName;
            if (BtnLuuVaMoi != null) BtnLuuVaMoi.IsEnabled = hasValidName;
            if (BtnLuuVaThoat != null) BtnLuuVaThoat.IsEnabled = hasValidName;
        }

        private void TxtTenKho_TextChanged(object sender, TextChangedEventArgs e) => UpdateButtonsState();

        private async Task LoadWarehouseListAsync()
        {
            try
            {
                _warehouseList = await LocalKhoHangService.GetAllWarehousesFlatAsync();

                if (_currentItem != null)
                {
                    _currentIndex = _warehouseList.FindIndex(x => x.Id == _currentItem.Id);
                }
                else
                {
                    _currentIndex = _warehouseList.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadWarehouseListAsync: " + ex.Message);
            }
        }

        private void DisplayWarehouse(KhoHangTreeItem item)
        {
            if (item == null) return;

            _isNew = false;
            _currentItem = item;
            Title = "KHO HÀNG - SỬA";

            BtnTruoc.IsEnabled = true;
            BtnSau.IsEnabled = true;

            TxtTenKho.Text = item.Name;
            ChkChoPhepAmKho.IsChecked = item.Chophepamkho;
            TxtGhiChu.Text = item.Note ?? "";

            if (!string.IsNullOrEmpty(item.SimageId) && _images.Count > 0)
            {
                CboAnh.SelectedValue = item.SimageId;
            }

            if (!string.IsNullOrEmpty(item.DcuahangId))
            {
                var st = _stores.FirstOrDefault(x => x.Id == item.DcuahangId);
                if (st != null) SelectStore(st);
            }
            else if (_stores.Count > 0)
            {
                SelectStore(_stores[0]);
            }

            TxtTenKho.Focus();
            TxtTenKho.SelectAll();
        }

        private void ResetFormForNew()
        {
            _isNew = true;
            _currentItem = null;
            if (_warehouseList != null)
            {
                _currentIndex = _warehouseList.Count;
            }
            Title = "KHO HÀNG - THÊM MỚI";

            BtnTruoc.IsEnabled = false;
            BtnSau.IsEnabled = false;

            TxtTenKho.Text = "";
            ChkChoPhepAmKho.IsChecked = false;
            TxtGhiChu.Text = "";

            if (_images.Count > 0)
            {
                var defaultImg = _images.FirstOrDefault(x => x.Id == "a38e42b9-aeda-4a67-8761-a5f4dc3571c1") ?? _images[0];
                CboAnh.SelectedItem = defaultImg;
            }

            if (_stores.Count > 0)
            {
                SelectStore(_stores[0]);
            }

            TxtTenKho.Focus();
        }

        private async Task<bool> SaveDataAsync()
        {
            string tenKho = TxtTenKho.Text.Trim();
            if (string.IsNullOrEmpty(tenKho))
            {
                MessageBox.Show("Vui lòng nhập Tên kho hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenKho.Focus();
                return false;
            }

            string selectedImageId = (CboAnh.SelectedItem as SImageViewModel)?.Id ?? "a38e42b9-aeda-4a67-8761-a5f4dc3571c1";

            var item = new KhoHangTreeItem
            {
                Id = _isNew ? "" : (_currentItem?.Id ?? _editItem?.Id ?? ""),
                Name = tenKho,
                Chophepamkho = ChkChoPhepAmKho.IsChecked == true,
                DcuahangId = _selectedStoreId,
                SimageId = selectedImageId,
                Note = TxtGhiChu.Text?.Trim() ?? "",
                ItemType = "0"
            };

            var (success, errorMsg, newId) = await LocalKhoHangService.SaveKhoHangAsync(item, _isNew);
            if (success)
            {
                OnSaved?.Invoke();
                await LoadWarehouseListAsync();
                _currentIndex = _warehouseList.FindIndex(x => x.Id == (_isNew ? newId : item.Id));
                if (_currentIndex >= 0)
                {
                    _currentItem = _warehouseList[_currentIndex];
                }
                _isNew = false;
                Title = "KHO HÀNG - SỬA";
                return true;
            }
            else
            {
                MessageBox.Show($"Có lỗi xảy ra khi lưu kho hàng:\n{errorMsg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu thông tin kho hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void BtnAnh_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng chọn ảnh kho hàng đang được phát triển!", "Ảnh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_warehouseList == null || _warehouseList.Count == 0)
            {
                await LoadWarehouseListAsync();
                if (_warehouseList.Count == 0)
                {
                    MessageBox.Show("Chưa có kho hàng nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _warehouseList.Count - 1;
            }

            DisplayWarehouse(_warehouseList[_currentIndex]);
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_warehouseList == null || _warehouseList.Count == 0)
            {
                await LoadWarehouseListAsync();
                if (_warehouseList.Count == 0)
                {
                    MessageBox.Show("Chưa có kho hàng nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            if (_currentIndex < _warehouseList.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            DisplayWarehouse(_warehouseList[_currentIndex]);
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
