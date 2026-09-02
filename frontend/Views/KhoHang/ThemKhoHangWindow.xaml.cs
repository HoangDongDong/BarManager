using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

        public ThemKhoHangWindow(KhoHangTreeItem item = null)
        {
            InitializeComponent();
            _editItem = item;
            _currentItem = item;
            _isNew = (item == null);

            Loaded += ThemKhoHangWindow_Loaded;
        }

        private async void ThemKhoHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

        private async Task LoadCuaHangListAsync()
        {
            try
            {
                var stores = await LocalKhoHangService.GetCuaHangListAsync();
                CboCuaHang.ItemsSource = stores;
                CboCuaHang.SelectedValuePath = "Id";

                if (stores.Count > 0)
                {
                    CboCuaHang.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCuaHangListAsync: " + ex.Message);
            }
        }

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

            if (item.DcuahangId.HasValue)
            {
                CboCuaHang.SelectedValue = item.DcuahangId.Value;
            }
            else if (CboCuaHang.Items.Count > 0)
            {
                CboCuaHang.SelectedIndex = 0;
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

            if (CboCuaHang.Items.Count > 0)
            {
                CboCuaHang.SelectedIndex = 0;
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

            int? cuahangId = null;
            if (CboCuaHang.SelectedValue != null && int.TryParse(CboCuaHang.SelectedValue.ToString(), out int chId))
            {
                cuahangId = chId;
            }

            var item = new KhoHangTreeItem
            {
                Id = _isNew ? 0 : (_currentItem?.Id ?? _editItem?.Id ?? 0),
                Name = tenKho,
                Chophepamkho = ChkChoPhepAmKho.IsChecked == true,
                DcuahangId = cuahangId,
                Note = TxtGhiChu.Text?.Trim() ?? "",
                ItemType = "ITEM"
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
