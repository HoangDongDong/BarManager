using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.KhoHang
{
    public partial class DanhMucKhoHangWindow : Window
    {
        private ObservableCollection<KhoHangTreeItem> _treeItems = new ObservableCollection<KhoHangTreeItem>();
        private KhoHangTreeItem _selectedItem;
        private bool _isViewingTrash = false;
        private bool _isFlatView = false;

        public DanhMucKhoHangWindow()
        {
            InitializeComponent();
            TvKhoHang.ItemsSource = _treeItems;

            Loaded += DanhMucKhoHangWindow_Loaded;
            PreviewKeyDown += DanhMucKhoHangWindow_PreviewKeyDown;
        }

        private async void DanhMucKhoHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhoHangTreeAsync();
            await LoadKhoMacDinhAsync();
        }

        private void DanhMucKhoHangWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private async Task LoadKhoHangTreeAsync()
        {
            try
            {
                var tree = await LocalKhoHangService.GetKhoHangTreeAsync(_isViewingTrash);
                _treeItems.Clear();

                if (_isFlatView)
                {
                    // Chế độ phẳng: hiển thị tất cả các node dạng phẳng
                    void Flatten(IEnumerable<KhoHangTreeItem> nodes)
                    {
                        foreach (var n in nodes)
                        {
                            var clone = new KhoHangTreeItem
                            {
                                Id = n.Id,
                                Name = n.Name,
                                Note = n.Note,
                                ItemType = n.ItemType,
                                Chophepamkho = n.Chophepamkho,
                                DcuahangId = n.DcuahangId,
                                TenCuaHang = n.TenCuaHang,
                                Status = n.Status
                            };
                            _treeItems.Add(clone);
                            if (n.Children != null && n.Children.Count > 0)
                            {
                                Flatten(n.Children);
                            }
                        }
                    }
                    Flatten(tree);
                }
                else
                {
                    foreach (var item in tree)
                    {
                        _treeItems.Add(item);
                    }
                }

                // Tự động chọn node đầu tiên nếu có
                if (_treeItems.Count > 0)
                {
                    _treeItems[0].IsSelected = true;
                    _selectedItem = _treeItems[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadKhoHangTreeAsync: " + ex.Message);
            }
        }

        private async Task LoadKhoMacDinhAsync()
        {
            try
            {
                var warehouses = await LocalKhoHangService.GetAllWarehousesFlatAsync();
                CboKhoMacDinh.ItemsSource = warehouses;

                if (warehouses.Count > 0)
                {
                    CboKhoMacDinh.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadKhoMacDinhAsync: " + ex.Message);
            }
        }

        private void TvKhoHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedItem = e.NewValue as KhoHangTreeItem;
        }

        private void TvKhoHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedItem != null && _selectedItem.IsWarehouse)
            {
                OpenEditWindow(_selectedItem);
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

        private void MiThemKhoHang_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemKhoHangWindow(null);
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadKhoHangTreeAsync();
                await LoadKhoMacDinhAsync();
            };
            win.ShowDialog();
        }

        private async void MiThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            string folderName = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên thư mục mới:", "Thêm thư mục kho hàng", "Thư mục mới");
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var item = new KhoHangTreeItem
                {
                    Name = folderName.Trim(),
                    ItemType = "FOLDER",
                    ParentId = null
                };
                await LocalKhoHangService.SaveKhoHangAsync(item, true);
                await LoadKhoHangTreeAsync();
            }
        }

        private async void MiThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (_selectedItem != null && _selectedItem.IsFolder)
            {
                parentId = _selectedItem.Id;
            }

            string folderName = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên thư mục con mới:", "Thêm thư mục con", "Thư mục con mới");
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var item = new KhoHangTreeItem
                {
                    Name = folderName.Trim(),
                    ItemType = "FOLDER",
                    ParentId = parentId
                };
                await LocalKhoHangService.SaveKhoHangAsync(item, true);
                await LoadKhoHangTreeAsync();
            }
        }

        private async void MiThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedItem?.ParentId;
            var item = new KhoHangTreeItem
            {
                Name = "---",
                ItemType = "SEPARATOR",
                ParentId = parentId
            };
            await LocalKhoHangService.SaveKhoHangAsync(item, true);
            await LoadKhoHangTreeAsync();
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một kho hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_selectedItem.IsWarehouse)
            {
                OpenEditWindow(_selectedItem);
            }
            else if (_selectedItem.IsFolder)
            {
                MiDoiTen_Click(sender, e);
            }
        }

        private void OpenEditWindow(KhoHangTreeItem item)
        {
            var win = new ThemKhoHangWindow(item);
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadKhoHangTreeAsync();
                await LoadKhoMacDinhAsync();
            };
            win.ShowDialog();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn mục cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string msg = _isViewingTrash
                ? $"Bạn có chắc chắn muốn xóa vĩnh viễn '{_selectedItem.Name}' khỏi hệ thống?"
                : $"Bạn có chắc chắn muốn chuyển '{_selectedItem.Name}' vào thùng rác?";

            var dr = MessageBox.Show(msg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dr == MessageBoxResult.Yes)
            {
                bool ok = await LocalKhoHangService.DeleteKhoHangAsync(_selectedItem.Id, _isViewingTrash);
                if (ok)
                {
                    await LoadKhoHangTreeAsync();
                    await LoadKhoMacDinhAsync();
                }
            }
        }

        private async void BtnThungRac_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThungRacKhoHangWindow();
            win.Owner = this;
            win.OnChanged += async () =>
            {
                await LoadKhoHangTreeAsync();
                await LoadKhoMacDinhAsync();
            };
            win.ShowDialog();
            await LoadKhoHangTreeAsync();
            await LoadKhoMacDinhAsync();
        }

        private async void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _isFlatView = !_isFlatView;
            await LoadKhoHangTreeAsync();
        }

        private async void MiDoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;

            string newName = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên mới:", "Đổi tên", _selectedItem.Name);
            if (!string.IsNullOrWhiteSpace(newName) && newName.Trim() != _selectedItem.Name)
            {
                _selectedItem.Name = newName.Trim();
                await LocalKhoHangService.SaveKhoHangAsync(_selectedItem, false);
                await LoadKhoHangTreeAsync();
                await LoadKhoMacDinhAsync();
            }
        }

        private async void MiLamMoi_Click(object sender, RoutedEventArgs e)
        {
            await LoadKhoHangTreeAsync();
            await LoadKhoMacDinhAsync();
        }

        private void CboKhoMacDinh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Cập nhật kho mặc định
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
