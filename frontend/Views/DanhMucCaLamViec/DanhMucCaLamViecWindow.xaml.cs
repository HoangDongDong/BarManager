using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucCaLamViec
{
    public partial class DanhMucCaLamViecWindow : Window
    {
        private ObservableCollection<CaLamViecTreeItem> _treeItems = new ObservableCollection<CaLamViecTreeItem>();
        private CaLamViecTreeItem _selectedItem = null;
        private bool _isViewingTrash = false;
        private bool _isFlatView = false;
        private bool _sortByCustom = false;

        public DanhMucCaLamViecWindow()
        {
            InitializeComponent();
            TvCaLamViec.ItemsSource = _treeItems;

            TvCaLamViec.PreviewMouseRightButtonDown += TvCaLamViec_PreviewMouseRightButtonDown;

            Loaded += async (s, e) =>
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    BtnThoat_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F5)
                {
                    MenuRefresh_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F2)
                {
                    MenuDoiTen_Click(null, null);
                    e.Handled = true;
                }
            };
        }

        private void TvCaLamViec_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
                _selectedItem = treeViewItem.DataContext as CaLamViecTreeItem;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as TreeViewItem;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (_isFlatView)
                {
                    var list = await LocalCaLamViecService.GetCaLamViecFlatListAsync(_isViewingTrash);
                    _treeItems.Clear();
                    foreach (var item in list)
                    {
                        _treeItems.Add(item);
                    }
                }
                else
                {
                    var tree = await LocalCaLamViecService.GetCaLamViecTreeAsync(_isViewingTrash);
                    _treeItems.Clear();
                    foreach (var item in tree)
                    {
                        _treeItems.Add(item);
                    }
                }

                if (_treeItems.Count > 0)
                {
                    _treeItems[0].IsSelected = true;
                    _selectedItem = _treeItems[0];
                }
                else
                {
                    _selectedItem = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục ca làm việc: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDefaultDropdownAsync()
        {
            try
            {
                var list = await LocalCaLamViecService.GetCaLamViecFlatListAsync(false);
                CboDefaultCa.ItemsSource = list;
                if (list.Count > 0)
                {
                    CboDefaultCa.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void UpdateViewModeUI()
        {
            if (_isViewingTrash)
            {
                Title = "Thùng rác - Ca làm việc";
                BannerMain.Visibility = Visibility.Collapsed;
                BannerTrash.Visibility = Visibility.Visible;
                ToolbarMain.Visibility = Visibility.Collapsed;
                ToolbarTrash.Visibility = Visibility.Visible;
                BorderSubBarDefault.Visibility = Visibility.Collapsed;
            }
            else
            {
                Title = "Ca làm việc";
                BannerMain.Visibility = Visibility.Visible;
                BannerTrash.Visibility = Visibility.Collapsed;
                ToolbarMain.Visibility = Visibility.Visible;
                ToolbarTrash.Visibility = Visibility.Collapsed;
                BorderSubBarDefault.Visibility = Visibility.Visible;
            }
        }

        private void TvCaLamViec_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedItem = e.NewValue as CaLamViecTreeItem;
        }

        private void TvCaLamViec_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedItem != null)
            {
                if (_isViewingTrash)
                {
                    BtnXemChiTiet_Click(null, null);
                }
                else
                {
                    BtnChinhSua_Click(null, null);
                }
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            if (BtnThemMoi.ContextMenu != null)
            {
                BtnThemMoi.ContextMenu.PlacementTarget = BtnThemMoi;
                BtnThemMoi.ContextMenu.IsOpen = true;
            }
        }

        private void OpenThemSuaWindow(string id, string itemType, string parentId)
        {
            var win = new ThemSuaCaLamViecWindow(id, itemType, parentId);
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            };
            win.ShowDialog();
            _ = LoadDataAsync();
            _ = LoadDefaultDropdownAsync();
        }

        private void MenuThemCa_Click(object sender, RoutedEventArgs e)
        {
            string parentId = (_selectedItem != null && _selectedItem.ItemType == "FOLDER") ? _selectedItem.Id : (_selectedItem?.ParentId ?? "");
            OpenThemSuaWindow(null, "0", parentId);
        }

        private void MenuThemNhanh_Click(object sender, RoutedEventArgs e)
        {
            string parentId = (_selectedItem != null && _selectedItem.ItemType == "FOLDER") ? _selectedItem.Id : (_selectedItem?.ParentId ?? "");
            OpenThemSuaWindow(null, "0", parentId);
        }

        private void MenuThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            OpenThemSuaWindow(null, "FOLDER", "");
        }

        private void MenuThemCaCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem != null ? _selectedItem.Id : "";
            OpenThemSuaWindow(null, "0", parentId);
        }

        private void MenuThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem != null ? _selectedItem.Id : "";
            OpenThemSuaWindow(null, "FOLDER", parentId);
        }

        private async void MenuThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem?.ParentId ?? "";
            var item = new CaLamViecTreeItem
            {
                Name = "----------------------------------------",
                ParentId = parentId,
                ItemType = "SEPARATOR"
            };
            var (ok, _) = await LocalCaLamViecService.SaveCaLamViecAsync(item);
            if (ok)
            {
                await LoadDataAsync();
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenThemSuaWindow(_selectedItem.Id, _selectedItem.ItemType, _selectedItem.ParentId);
        }

        private async void MenuSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            _sortByCustom = false;
            MenuSortByName.IsChecked = true;
            MenuSortByCustom.IsChecked = false;
            await LocalCaLamViecService.AutoSortAsync();
            await LoadDataAsync();
        }

        private async void MenuSapXepThuTu_Click(object sender, RoutedEventArgs e)
        {
            _sortByCustom = true;
            MenuSortByName.IsChecked = false;
            MenuSortByCustom.IsChecked = true;
            await LoadDataAsync();
        }

        private async void MenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            await LoadDefaultDropdownAsync();
        }

        private async void MenuSaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để sao chép!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string copyName = $"{_selectedItem.Name} (Copy)";
            var item = new CaLamViecTreeItem
            {
                Name = copyName,
                Note = _selectedItem.Note ?? "",
                ParentId = _selectedItem.ParentId ?? "",
                ItemType = _selectedItem.ItemType ?? "0",
                TiLeLuong = _selectedItem.TiLeLuong,
                TuGio = _selectedItem.TuGio ?? "",
                DenGio = _selectedItem.DenGio ?? "",
                SimageId = _selectedItem.SimageId ?? ""
            };

            var (ok, _) = await LocalCaLamViecService.SaveCaLamViecAsync(item);
            if (ok)
            {
                MessageBox.Show($"Đã sao chép thành công ca làm việc '{copyName}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private void MenuMoRong_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(true);
        }

        private void MenuThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(false);
        }

        private void SetExpandAll(bool expand)
        {
            void Traverse(IEnumerable<CaLamViecTreeItem> items)
            {
                if (items == null) return;
                foreach (var item in items)
                {
                    item.IsExpanded = expand;
                    Traverse(item.Children);
                }
            }
            Traverse(_treeItems);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ask = MessageBox.Show($"Bạn có chắc chắn muốn chuyển ca làm việc '{_selectedItem.Name}' vào thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                bool ok = await LocalCaLamViecService.DeleteCaLamViecAsync(_selectedItem.Id, permanent: false);
                if (ok)
                {
                    await LoadDataAsync();
                    await LoadDefaultDropdownAsync();
                }
                else
                {
                    MessageBox.Show("Xóa không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuDoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để đổi tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BtnChinhSua_Click(null, null);
        }

        private void MenuBieuTuong_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để chọn biểu tượng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BtnChinhSua_Click(null, null);
        }

        private async void BtnToggleView_Click(object sender, RoutedEventArgs e)
        {
            _isFlatView = !_isFlatView;
            await LoadDataAsync();
        }

        private async void BtnThungRac_Click(object sender, RoutedEventArgs e)
        {
            _isViewingTrash = true;
            UpdateViewModeUI();
            await LoadDataAsync();
        }

        // ================= TRASH ACTIONS =================

        private async void BtnKhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool ok = await LocalCaLamViecService.RestoreCaLamViecAsync(_selectedItem.Id);
            if (ok)
            {
                MessageBox.Show($"Đã khôi phục thành công ca làm việc '{_selectedItem.Name}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
            else
            {
                MessageBox.Show("Khôi phục không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để xem chi tiết!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemSuaCaLamViecWindow(_selectedItem.Id, _selectedItem.ItemType, _selectedItem.ParentId);
            win.Owner = this;
            win.ShowDialog();
        }

        private async void BtnXoaVinhVien_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để xóa vĩnh viễn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ask = MessageBox.Show($"Bạn có chắc chắn muốn XÓA VĨNH VIỄN ca làm việc '{_selectedItem.Name}' không?\nDữ liệu đã xóa sẽ không thể phục hồi!", "Cảnh báo xóa vĩnh viễn", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (ask == MessageBoxResult.Yes)
            {
                bool ok = await LocalCaLamViecService.DeleteCaLamViecAsync(_selectedItem.Id, permanent: true);
                if (ok)
                {
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Xóa vĩnh viễn không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một ca làm việc để xem thuộc tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string info = $"Mã ca: {_selectedItem.Id}\n" +
                          $"Tên ca: {_selectedItem.Name}\n" +
                          $"Tỉ lệ lương: {_selectedItem.TiLeLuong}%\n" +
                          $"Thời gian: {_selectedItem.TuGio} - {_selectedItem.DenGio}\n" +
                          $"Ghi chú: {_selectedItem.Note}\n" +
                          $"Trạng thái: " + (_isViewingTrash ? "Đã xóa vào thùng rác" : "Đang hoạt động");

            MessageBox.Show(info, $"Thuộc tính: {_selectedItem.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CboDefaultCa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Persist default shift
        }

        private async void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            if (_isViewingTrash)
            {
                _isViewingTrash = false;
                UpdateViewModeUI();
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
            else
            {
                Close();
            }
        }
    }
}
