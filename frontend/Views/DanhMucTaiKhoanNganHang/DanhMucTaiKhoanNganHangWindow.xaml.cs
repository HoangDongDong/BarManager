using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang
{
    public partial class DanhMucTaiKhoanNganHangWindow : Window
    {
        private ObservableCollection<TaiKhoanNganHangTreeItem> _treeItems = new ObservableCollection<TaiKhoanNganHangTreeItem>();
        private TaiKhoanNganHangTreeItem _selectedItem = null;
        private bool _isViewingTrash = false;
        private bool _isFlatView = false;
        private bool _sortByCustom = false;

        public DanhMucTaiKhoanNganHangWindow()
        {
            InitializeComponent();
            TvTaiKhoan.ItemsSource = _treeItems;

            TvTaiKhoan.PreviewMouseRightButtonDown += TvTaiKhoan_PreviewMouseRightButtonDown;

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

        private void TvTaiKhoan_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
                _selectedItem = treeViewItem.DataContext as TaiKhoanNganHangTreeItem;
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
                    var list = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangListAsync(_isViewingTrash);
                    _treeItems.Clear();
                    foreach (var item in list)
                    {
                        _treeItems.Add(item);
                    }
                }
                else
                {
                    var tree = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangTreeAsync(_isViewingTrash);
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
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDefaultDropdownAsync()
        {
            try
            {
                var list = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangListAsync(false);
                CboDefaultTK.ItemsSource = list;
                if (list.Count > 0)
                {
                    CboDefaultTK.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void UpdateViewModeUI()
        {
            if (_isViewingTrash)
            {
                Title = "Thùng rác";
                BannerMain.Visibility = Visibility.Collapsed;
                BannerTrash.Visibility = Visibility.Visible;
                ToolbarMain.Visibility = Visibility.Collapsed;
                ToolbarTrash.Visibility = Visibility.Visible;
                BorderSubBarDefault.Visibility = Visibility.Collapsed;
            }
            else
            {
                Title = "Tài khoản ngân hàng";
                BannerMain.Visibility = Visibility.Visible;
                BannerTrash.Visibility = Visibility.Collapsed;
                ToolbarMain.Visibility = Visibility.Visible;
                ToolbarTrash.Visibility = Visibility.Collapsed;
                BorderSubBarDefault.Visibility = Visibility.Visible;
            }
        }

        private void TvTaiKhoan_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedItem = e.NewValue as TaiKhoanNganHangTreeItem;
        }

        private void TvTaiKhoan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        private async void MenuThemTK_Click(object sender, RoutedEventArgs e)
        {
            string parentId = (_selectedItem != null && _selectedItem.ItemType == "FOLDER") ? _selectedItem.Id : (_selectedItem?.ParentId ?? "");
            var win = new ThemSuaTaiKhoanNganHangWindow(null, "", parentId);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private async void MenuThemNhanh_Click(object sender, RoutedEventArgs e)
        {
            string parentId = (_selectedItem != null && _selectedItem.ItemType == "FOLDER") ? _selectedItem.Id : (_selectedItem?.ParentId ?? "");
            var win = new ThemSuaTaiKhoanNganHangWindow(null, "", parentId);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private async void MenuThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemSuaTaiKhoanNganHangWindow(null, "FOLDER", "");
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private async void MenuThemTKCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem != null ? _selectedItem.Id : "";
            var win = new ThemSuaTaiKhoanNganHangWindow(null, "", parentId);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private async void MenuThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem != null ? _selectedItem.Id : "";
            var win = new ThemSuaTaiKhoanNganHangWindow(null, "FOLDER", parentId);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private async void MenuThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem?.ParentId ?? "";
            var (ok, _, _) = await LocalTaiKhoanNganHangService.SaveTaiKhoanNganHangAsync(
                null,
                "----------------------------------------",
                parentId,
                "",
                "SEPARATOR"
            );
            if (ok)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemSuaTaiKhoanNganHangWindow(_selectedItem.Id, _selectedItem.ItemType, _selectedItem.ParentId);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
                await LoadDefaultDropdownAsync();
            }
        }

        private async void MenuSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            _sortByCustom = false;
            MenuSortByName.IsChecked = true;
            MenuSortByCustom.IsChecked = false;
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
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để sao chép!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string copyName = $"{_selectedItem.Name} (Copy)";
            var (ok, _, _) = await LocalTaiKhoanNganHangService.SaveTaiKhoanNganHangAsync(
                null,
                copyName,
                _selectedItem.ParentId ?? "",
                _selectedItem.Note ?? "",
                _selectedItem.ItemType ?? ""
            );

            if (ok)
            {
                MessageBox.Show($"Đã sao chép thành công '{copyName}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
            void Traverse(IEnumerable<TaiKhoanNganHangTreeItem> items)
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
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ask = MessageBox.Show($"Bạn có chắc chắn muốn chuyển '{_selectedItem.Name}' vào thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                bool ok = await LocalTaiKhoanNganHangService.DeleteTaiKhoanNganHangAsync(_selectedItem.Id, permanent: false);
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
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để đổi tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BtnChinhSua_Click(null, null);
        }

        private void MenuBieuTuong_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để chọn biểu tượng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool ok = await LocalTaiKhoanNganHangService.RestoreTaiKhoanNganHangAsync(_selectedItem.Id);
            if (ok)
            {
                MessageBox.Show($"Đã khôi phục thành công '{_selectedItem.Name}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để xem chi tiết!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemSuaTaiKhoanNganHangWindow(_selectedItem.Id, _selectedItem.ItemType, _selectedItem.ParentId);
            win.Owner = this;
            win.ShowDialog();
        }

        private async void BtnXoaVinhVien_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để xóa vĩnh viễn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ask = MessageBox.Show($"Bạn có chắc chắn muốn XÓA VĨNH VIỄN '{_selectedItem.Name}' không?\nDữ liệu đã xóa sẽ không thể phục hồi!", "Cảnh báo xóa vĩnh viễn", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (ask == MessageBoxResult.Yes)
            {
                bool ok = await LocalTaiKhoanNganHangService.DeleteTaiKhoanNganHangAsync(_selectedItem.Id, permanent: true);
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
                MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để xem thuộc tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string info = $"Mã TK: {_selectedItem.Id}\n" +
                          $"Tên TK: {_selectedItem.Name}\n" +
                          $"Ghi chú: {_selectedItem.Note}\n" +
                          $"Trạng thái: " + (_isViewingTrash ? "Đã xóa vào thùng rác" : "Đang hoạt động");

            MessageBox.Show(info, $"Thuộc tính: {_selectedItem.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CboDefaultTK_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
