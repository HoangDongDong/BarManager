using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.DanhMucNhanVien;
using QuanLyBar.Client.Views;

namespace QuanLyBar.Client.Views.NhanSu
{
    public class ThuongPhatTreeDisplayItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string IconText { get; set; } = "";
        public string ItemType { get; set; } = ""; // ROOT, UNSET, TRASH, FOLDER, ITEM, SEPARATOR
        public string ParentId { get; set; } = "";
        public object OriginalTag { get; set; }

        public ObservableCollection<ThuongPhatTreeDisplayItem> Children { get; set; } = new ObservableCollection<ThuongPhatTreeDisplayItem>();

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public partial class ThuongPhatControl : UserControl
    {
        private enum TreeMode { NhanVien, LyDoThuongPhat }
        private TreeMode _currentMode = TreeMode.NhanVien;

        private ObservableCollection<ThuongPhatTreeDisplayItem> _treeItems = new ObservableCollection<ThuongPhatTreeDisplayItem>();
        private ThuongPhatTreeDisplayItem _selectedTreeItem = null;
        private readonly System.Threading.SemaphoreSlim _treeLock = new System.Threading.SemaphoreSlim(1, 1);

        private List<ThuongPhatItemViewModel> _allList = new List<ThuongPhatItemViewModel>();
        private bool _isLoaded = false;
        private bool _isFlatView = false;
        private DataGridColumn _clickedColumn = null;
        private string _clickedCellValue = null;

        public ThuongPhatControl()
        {
            InitializeComponent();
            TvDanhMuc.ItemsSource = _treeItems;

            Loaded += async (s, e) =>
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    await SwitchTreeModeAsync(TreeMode.NhanVien);
                    await LoadDataAsync();
                }
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F3)
                {
                    TxtSearch.Focus();
                    TxtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.Insert)
                {
                    BtnThemMoi_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F4)
                {
                    BtnChinhSua_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    BtnXoa_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F5)
                {
                    _ = LoadDataAsync();
                    _ = ReloadTreeAsync();
                    e.Handled = true;
                }
            };
        }

        #region TREE SWITCHER & LOADING

        private async void BtnTreeSwitcher_Click(object sender, RoutedEventArgs e)
        {
            var ctx = new ContextMenu();

            var mNv = new MenuItem { Header = "Nhân viên" };
            mNv.Icon = new TextBlock { Text = "👩", FontSize = 12 };
            mNv.Click += async (s, ev) => await SwitchTreeModeAsync(TreeMode.NhanVien);

            var mLd = new MenuItem { Header = "Lý do thưởng phạt" };
            mLd.Icon = new TextBlock { Text = "📝", FontSize = 12 };
            mLd.Click += async (s, ev) => await SwitchTreeModeAsync(TreeMode.LyDoThuongPhat);

            ctx.Items.Add(mNv);
            ctx.Items.Add(mLd);

            ctx.PlacementTarget = BtnTreeSwitcher;
            ctx.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ctx.IsOpen = true;
        }

        private async Task SwitchTreeModeAsync(TreeMode mode)
        {
            _currentMode = mode;
            if (mode == TreeMode.NhanVien)
            {
                TxtTreeHeader.Text = "Nhân viên";
            }
            else
            {
                TxtTreeHeader.Text = "Lý do thưởng phạt";
            }

            await ReloadTreeAsync();
            await LoadDataAsync();
        }

        private async Task ReloadTreeAsync()
        {
            await _treeLock.WaitAsync();
            try
            {
                _treeItems.Clear();

                // 1. Root: Tất cả
                var rootAll = new ThuongPhatTreeDisplayItem
                {
                    Id = "ALL",
                    Name = "Tất cả",
                    IconText = "🌐",
                    ItemType = "ROOT",
                    IsExpanded = true
                };

                // 2. Node: Chưa thiết lập
                var nodeUnset = new ThuongPhatTreeDisplayItem
                {
                    Id = "UNSET",
                    Name = "Chưa thiết lập",
                    IconText = "☀️",
                    ItemType = "UNSET"
                };
                rootAll.Children.Add(nodeUnset);

                if (_currentMode == TreeMode.NhanVien)
                {
                    if (_isFlatView)
                    {
                        var nvList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                        foreach (var nv in nvList)
                        {
                            rootAll.Children.Add(new ThuongPhatTreeDisplayItem
                            {
                                Id = nv.Id,
                                Name = nv.Name,
                                IconText = nv.IconText,
                                ItemType = "ITEM",
                                OriginalTag = nv
                            });
                        }
                    }
                    else
                    {
                        var tree = await LocalNhanVienService.GetNhanVienTreeAsync(false);
                        foreach (var node in tree)
                        {
                            rootAll.Children.Add(ConvertNhanVienNode(node));
                        }
                    }
                }
                else // LyDoThuongPhat
                {
                    if (_isFlatView)
                    {
                        var ldList = await LocalThuongPhatService.GetLyDoThuongPhatFlatListAsync();
                        foreach (var ld in ldList)
                        {
                            rootAll.Children.Add(new ThuongPhatTreeDisplayItem
                            {
                                Id = ld.Id,
                                Name = ld.Name,
                                IconText = string.IsNullOrEmpty(ld.IconText) ? "🔹" : ld.IconText,
                                ItemType = "ITEM",
                                OriginalTag = ld
                            });
                        }
                    }
                    else
                    {
                        var tree = await LocalThuongPhatService.GetLyDoThuongPhatTreeAsync(false);
                        foreach (var node in tree)
                        {
                            rootAll.Children.Add(ConvertLyDoNode(node));
                        }
                    }
                }

                // 3. Node: Thùng rác
                var nodeTrash = new ThuongPhatTreeDisplayItem
                {
                    Id = "TRASH",
                    Name = "Thùng rác",
                    IconText = "🗑️",
                    ItemType = "TRASH"
                };
                rootAll.Children.Add(nodeTrash);

                _treeItems.Add(rootAll);
                rootAll.IsSelected = true;
                _selectedTreeItem = rootAll;

                // Force refresh TreeView items source
                TvDanhMuc.ItemsSource = null;
                TvDanhMuc.ItemsSource = _treeItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cây danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _treeLock.Release();
            }
        }

        private ThuongPhatTreeDisplayItem ConvertNhanVienNode(NhanVienTreeItem node)
        {
            var item = new ThuongPhatTreeDisplayItem
            {
                Id = node.Id,
                Name = node.Name,
                IconText = node.IconText,
                ItemType = node.ItemType,
                ParentId = node.ParentId,
                IsExpanded = true,
                OriginalTag = node
            };
            if (node.Children != null)
            {
                foreach (var c in node.Children)
                {
                    item.Children.Add(ConvertNhanVienNode(c));
                }
            }
            return item;
        }

        private ThuongPhatTreeDisplayItem ConvertLyDoNode(LyDoThuongPhatTreeItem node)
        {
            var item = new ThuongPhatTreeDisplayItem
            {
                Id = node.Id,
                Name = node.Name,
                IconText = string.IsNullOrEmpty(node.IconText) ? "🔹" : node.IconText,
                ItemType = node.ItemType,
                ParentId = node.ParentId,
                IsExpanded = true,
                OriginalTag = node
            };
            if (node.Children != null)
            {
                foreach (var c in node.Children)
                {
                    item.Children.Add(ConvertLyDoNode(c));
                }
            }
            return item;
        }

        private async void TvDanhMuc_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = TvDanhMuc.SelectedItem as ThuongPhatTreeDisplayItem;
            await LoadDataAsync();
        }

        private void TvDanhMuc_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
                _selectedTreeItem = treeViewItem.DataContext as ThuongPhatTreeDisplayItem;
            }
        }

        private void TvDanhMuc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnSuaTree_Click(null, null);
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as TreeViewItem;
        }

        #endregion

        #region TREE ACTION TOOLBAR

        private async void BtnThemTree_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == TreeMode.NhanVien)
            {
                string parentId = (_selectedTreeItem != null && _selectedTreeItem.ItemType == "FOLDER") ? _selectedTreeItem.Id : (_selectedTreeItem?.ParentId ?? "");
                var win = new ThemSuaNhanVienWindow(null, "0", parentId);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    await ReloadTreeAsync();
                }
            }
            else
            {
                string parentId = (_selectedTreeItem != null && _selectedTreeItem.ItemType == "FOLDER") ? _selectedTreeItem.Id : (_selectedTreeItem?.ParentId ?? "");
                var win = new ThemSuaLyDoThuongPhatWindow(null, null, parentId);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true || win.IsSaved)
                {
                    await ReloadTreeAsync();
                }
            }
        }

        private async void BtnSuaTree_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.Id == "ALL" || _selectedTreeItem.Id == "UNSET" || _selectedTreeItem.Id == "TRASH")
                return;

            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new ThemSuaNhanVienWindow(_selectedTreeItem.Id, _selectedTreeItem.ItemType, _selectedTreeItem.ParentId);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    await ReloadTreeAsync();
                }
            }
            else
            {
                var win = new ThemSuaLyDoThuongPhatWindow(_selectedTreeItem.Id, _selectedTreeItem.Name, _selectedTreeItem.ParentId, _selectedTreeItem.ItemType);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true || win.IsSaved)
                {
                    await ReloadTreeAsync();
                }
            }
        }

        private async void BtnThemThuMucTree_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new ThemSuaNhanVienWindow(null, "FOLDER", _selectedTreeItem?.Id);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    await ReloadTreeAsync();
                }
            }
            else
            {
                var dlg = new InputWindow("Tạo thư mục lý do:", "Tên thư mục");
                dlg.Owner = Window.GetWindow(this);
                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.InputText))
                {
                    var item = new LyDoThuongPhatTreeItem
                    {
                        Name = dlg.InputText.Trim(),
                        ItemType = "FOLDER",
                        ParentId = _selectedTreeItem?.Id ?? "",
                        Status = 1
                    };
                    await LocalThuongPhatService.SaveLyDoThuongPhatAsync(item);
                    await ReloadTreeAsync();
                }
            }
        }

        private void CmTreeView_Opened(object sender, RoutedEventArgs e)
        {
            if (_currentMode == TreeMode.NhanVien)
            {
                if (MenuTreeThemMoiItem != null) MenuTreeThemMoiItem.Header = "➕  Thêm nhân viên";
                if (MenuTreeThemPhanCach != null) MenuTreeThemPhanCach.Visibility = Visibility.Collapsed;
                if (MenuTreeThemConItem != null) MenuTreeThemConItem.Header = "➕  Thêm nhân viên";
            }
            else
            {
                if (MenuTreeThemMoiItem != null) MenuTreeThemMoiItem.Header = "➕  Thêm lý do thưởng phạt";
                if (MenuTreeThemPhanCach != null) MenuTreeThemPhanCach.Visibility = Visibility.Visible;
                if (MenuTreeThemConItem != null) MenuTreeThemConItem.Header = "➕  Thêm lý do thưởng phạt";
            }
        }

        private void MiThemMoiItem_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new ThemSuaNhanVienWindow(null, "0", "");
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true) _ = ReloadTreeAsync();
            }
            else
            {
                var win = new ThemSuaLyDoThuongPhatWindow(null, null, "");
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true || win.IsSaved) _ = ReloadTreeAsync();
            }
        }

        private void MiThemNhanhGoc_Click(object sender, RoutedEventArgs e) => MiThemMoiItem_Click(sender, e);

        private async void MiThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == TreeMode.LyDoThuongPhat)
            {
                var item = new LyDoThuongPhatTreeItem
                {
                    Name = "----------",
                    ItemType = "SEPARATOR",
                    ParentId = "",
                    Status = 1
                };
                await LocalThuongPhatService.SaveLyDoThuongPhatAsync(item);
                await ReloadTreeAsync();
            }
        }

        private async void MiThemThuMucGoc_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new ThemSuaNhanVienWindow(null, "FOLDER", "");
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true) await ReloadTreeAsync();
            }
            else
            {
                var dlg = new InputWindow("Tạo thư mục lý do:", "Tên thư mục");
                dlg.Owner = Window.GetWindow(this);
                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.InputText))
                {
                    var item = new LyDoThuongPhatTreeItem
                    {
                        Name = dlg.InputText.Trim(),
                        ItemType = "FOLDER",
                        ParentId = "",
                        Status = 1
                    };
                    await LocalThuongPhatService.SaveLyDoThuongPhatAsync(item);
                    await ReloadTreeAsync();
                }
            }
        }

        private void MiThemConItem_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedTreeItem?.Id ?? "";
            if (parentId == "ALL" || parentId == "UNSET" || parentId == "TRASH") parentId = "";

            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new ThemSuaNhanVienWindow(null, "0", parentId);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true) _ = ReloadTreeAsync();
            }
            else
            {
                var win = new ThemSuaLyDoThuongPhatWindow(null, null, parentId);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true || win.IsSaved) _ = ReloadTreeAsync();
            }
        }

        private void MiThemNhanhCon_Click(object sender, RoutedEventArgs e) => MiThemConItem_Click(sender, e);

        private async void MiThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedTreeItem?.Id ?? "";
            if (parentId == "ALL" || parentId == "UNSET" || parentId == "TRASH") parentId = "";

            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new ThemSuaNhanVienWindow(null, "FOLDER", parentId);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true) await ReloadTreeAsync();
            }
            else
            {
                var dlg = new InputWindow("Tạo thư mục con:", "Tên thư mục con");
                dlg.Owner = Window.GetWindow(this);
                if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.InputText))
                {
                    var item = new LyDoThuongPhatTreeItem
                    {
                        Name = dlg.InputText.Trim(),
                        ItemType = "FOLDER",
                        ParentId = parentId,
                        Status = 1
                    };
                    await LocalThuongPhatService.SaveLyDoThuongPhatAsync(item);
                    await ReloadTreeAsync();
                }
            }
        }

        private void MiChinhSua_Click(object sender, RoutedEventArgs e) => BtnSuaTree_Click(sender, e);

        private void MiSapXepTen_Click(object sender, RoutedEventArgs e) => _ = ReloadTreeAsync();

        private void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e) => _ = ReloadTreeAsync();

        private void MiTreeSaoChep_Click(object sender, RoutedEventArgs e) => BtnSuaTree_Click(sender, e);

        private void MiTreeMoRong_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(_treeItems, true);
        }

        private void MiTreeThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(_treeItems, false);
        }

        private void SetExpandAll(IEnumerable<ThuongPhatTreeDisplayItem> items, bool isExpanded)
        {
            if (items == null) return;
            foreach (var it in items)
            {
                it.IsExpanded = isExpanded;
                SetExpandAll(it.Children, isExpanded);
            }
        }

        private async void MiTreeXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.Id == "ALL" || _selectedTreeItem.Id == "UNSET" || _selectedTreeItem.Id == "TRASH")
            {
                MessageBox.Show("Vui lòng chọn một mục hợp lệ để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string itemTypeStr = _currentMode == TreeMode.NhanVien ? "nhân viên" : "lý do thưởng phạt";
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa {itemTypeStr} '{_selectedTreeItem.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (_currentMode == TreeMode.NhanVien)
                {
                    await LocalNhanVienService.DeleteNhanVienAsync(_selectedTreeItem.Id);
                }
                else
                {
                    await LocalThuongPhatService.DeleteLyDoThuongPhatAsync(_selectedTreeItem.Id);
                }
                await ReloadTreeAsync();
            }
        }

        private async void MiTreeDoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.Id == "ALL" || _selectedTreeItem.Id == "UNSET" || _selectedTreeItem.Id == "TRASH")
                return;

            var dlg = new InputWindow("Đổi tên:", "Tên mới", _selectedTreeItem.Name);
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.InputText))
            {
                string newName = dlg.InputText.Trim();
                if (_currentMode == TreeMode.NhanVien)
                {
                    var nv = await LocalNhanVienService.GetByIdAsync(_selectedTreeItem.Id);
                    if (nv != null)
                    {
                        nv.Name = newName;
                        await LocalNhanVienService.SaveNhanVienAsync(nv);
                    }
                }
                else
                {
                    var item = new LyDoThuongPhatTreeItem
                    {
                        Id = _selectedTreeItem.Id,
                        Name = newName,
                        ItemType = _selectedTreeItem.ItemType,
                        ParentId = _selectedTreeItem.ParentId,
                        Status = 1
                    };
                    await LocalThuongPhatService.SaveLyDoThuongPhatAsync(item);
                }
                await ReloadTreeAsync();
            }
        }

        private void MiTreeThungRac_Click(object sender, RoutedEventArgs e)
        {
            var trashNode = _treeItems.FirstOrDefault(x => x.Id == "TRASH");
            if (trashNode != null)
            {
                trashNode.IsSelected = true;
            }
        }

        private void MiTreeBieuTuong_Click(object sender, RoutedEventArgs e)
        {
            BtnSuaTree_Click(sender, e);
        }

        private void MiTreeThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem != null)
            {
                MessageBox.Show($"Tên: {_selectedTreeItem.Name}\nID: {_selectedTreeItem.Id}\nLoại: {_selectedTreeItem.ItemType}", "Thuộc tính danh mục", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _isFlatView = !_isFlatView;
            await ReloadTreeAsync();
        }

        private async void BtnLamMoiTree_Click(object sender, RoutedEventArgs e)
        {
            await ReloadTreeAsync();
        }

        private void BtnCauHinhTree_Click(object sender, MouseButtonEventArgs e)
        {
            if (_currentMode == TreeMode.NhanVien)
            {
                var win = new DanhMucNhanVienWindow();
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
                _ = ReloadTreeAsync();
            }
        }

        #endregion

        #region DATA GRID LOADING & CRUD

        public async Task LoadDataAsync()
        {
            try
            {
                bool isTrash = _selectedTreeItem != null && _selectedTreeItem.Id == "TRASH";
                string nhanVienId = null;
                string lyDoId = null;

                if (_selectedTreeItem != null && _selectedTreeItem.Id != "ALL" && _selectedTreeItem.Id != "TRASH")
                {
                    if (_currentMode == TreeMode.NhanVien)
                    {
                        nhanVienId = _selectedTreeItem.Id;
                    }
                    else
                    {
                        lyDoId = _selectedTreeItem.Id;
                    }
                }

                string kw = TxtSearch?.Text?.Trim();
                _allList = await LocalThuongPhatService.GetThuongPhatListAsync(nhanVienId, lyDoId, kw, isTrash);
                DgThuongPhat.ItemsSource = _allList;

                UpdateSummaryFooter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách thưởng phạt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummaryFooter()
        {
            if (_allList == null) return;
            TxtTotalCount.Text = _allList.Count.ToString("N0");
            decimal totalThuong = _allList.Sum(x => x.Thuong ?? 0);
            decimal totalPhat = _allList.Sum(x => x.Phat ?? 0);
            TxtTotalThuong.Text = totalThuong.ToString("N0") + " VNĐ";
            TxtTotalPhat.Text = totalPhat.ToString("N0") + " VNĐ";
        }

        private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            string preNvId = (_currentMode == TreeMode.NhanVien && _selectedTreeItem != null && _selectedTreeItem.Id != "ALL" && _selectedTreeItem.Id != "TRASH" && _selectedTreeItem.Id != "UNSET") ? _selectedTreeItem.Id : null;
            string preLdId = (_currentMode == TreeMode.LyDoThuongPhat && _selectedTreeItem != null && _selectedTreeItem.Id != "ALL" && _selectedTreeItem.Id != "TRASH" && _selectedTreeItem.Id != "UNSET") ? _selectedTreeItem.Id : null;

            var win = new ThemSuaThuongPhatWindow(null, preNvId, preLdId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadDataAsync();
            };
            win.ShowDialog();
            if (win.IsSaved || win.DialogResult == true)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgThuongPhat.SelectedItem as ThuongPhatItemViewModel;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một bản ghi thưởng phạt để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemSuaThuongPhatWindow(selected.Id);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadDataAsync();
            };
            win.ShowDialog();
            if (win.IsSaved || win.DialogResult == true)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgThuongPhat.SelectedItem as ThuongPhatItemViewModel;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một bản ghi thưởng phạt để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isTrash = _selectedTreeItem != null && _selectedTreeItem.Id == "TRASH";
            string msg = isTrash ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN phiếu '{selected.SoPhieu}' không?" : $"Bạn có chắc chắn muốn xóa phiếu thưởng phạt '{selected.SoPhieu}' không?";
            var ask = MessageBox.Show(msg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                bool ok = await LocalThuongPhatService.DeleteThuongPhatAsync(selected.Id, permanent: isTrash);
                if (ok)
                {
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Xóa không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgThuongPhat_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgThuongPhat.SelectedItem != null)
            {
                BtnChinhSua_Click(null, null);
            }
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            UpdateSummaryFooter();
            MessageBox.Show($"Tổng số phiếu: {TxtTotalCount.Text}\nTổng tiền thưởng: {TxtTotalThuong.Text}\nTổng tiền phạt: {TxtTotalPhat.Text}", "Tổng kết thưởng phạt", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPhanTich_Click(object sender, RoutedEventArgs e)
        {
            if (_allList == null || _allList.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu để phân tích!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var topThuong = _allList.Where(x => x.Thuong.HasValue && x.Thuong > 0)
                                    .GroupBy(x => x.TenNhanVien)
                                    .Select(g => new { Ten = g.Key, Tong = g.Sum(x => x.Thuong.Value) })
                                    .OrderByDescending(x => x.Tong)
                                    .Take(5)
                                    .ToList();

            string report = "=== TOP NHÂN VIÊN THƯỞNG CAO NHẤT ===\n";
            foreach (var item in topThuong)
            {
                report += $"- {item.Ten}: {item.Tong:N0} VNĐ\n";
            }

            MessageBox.Show(report, "Phân tích thưởng phạt", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Không thể thực hiện import/cập nhật dữ liệu từ excel với dữ liệu này", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_allList == null || _allList.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"DanhSachThuongPhat_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dlg.ShowDialog() == true)
                {
                    using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
                    sw.WriteLine("Số phiếu,Ngày,Nhân viên,Thưởng,Phạt,Lý do thưởng phạt,Ghi chú");
                    foreach (var item in _allList)
                    {
                        sw.WriteLine($"\"{item.SoPhieu}\",\"{item.NgayStr}\",\"{item.TenNhanVien}\",\"{item.ThuongStr}\",\"{item.PhatStr}\",\"{item.TenLyDo}\",\"{item.GhiChu}\"");
                    }
                    MessageBox.Show("Xuất file thành công: " + dlg.FileName, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgThuongPhat, "Thưởng phạt");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion

        #region CONTEXT MENU ON RIGHT CLICK

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            var parentObj = VisualTreeHelper.GetParent(child);
            if (parentObj == null) return null;
            if (parentObj is T parent) return parent;
            return FindVisualParent<T>(parentObj);
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
                var pt = e.GetPosition(DgThuongPhat);
                var element = DgThuongPhat.InputHitTest(pt) as DependencyObject;
                var cell = FindVisualParent<DataGridCell>(element);
                if (cell != null)
                {
                    _clickedColumn = cell.Column;
                    if (cell.Content is TextBlock tb)
                    {
                        _clickedCellValue = tb.Text;
                    }
                    else if (cell.Content != null)
                    {
                        _clickedCellValue = cell.Content.ToString();
                    }
                }
                else
                {
                    _clickedColumn = DgThuongPhat.CurrentColumn ?? DgThuongPhat.Columns.FirstOrDefault();
                    _clickedCellValue = null;
                }
            }
        }

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            string colHeader = _clickedColumn?.Header?.ToString() ?? "Số phiếu";
            if (MenuLocCot != null)
            {
                MenuLocCot.Header = $"Lọc {colHeader}";
            }
        }

        private void MenuLocCot_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                TxtSearch.Text = _clickedCellValue;
                TxtSearch.Focus();
                TxtSearch.SelectAll();
            }
            else
            {
                TxtSearch.Focus();
                TxtSearch.SelectAll();
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null && _allList.Count > 0)
            {
                string colHeader = _clickedColumn?.Header?.ToString() ?? "";
                if (colHeader == "Ngày")
                    _allList = _allList.OrderBy(x => x.Ngay).ToList();
                else if (colHeader == "Nhân viên")
                    _allList = _allList.OrderBy(x => x.TenNhanVien).ToList();
                else if (colHeader == "Thưởng")
                    _allList = _allList.OrderBy(x => x.Thuong ?? 0).ToList();
                else if (colHeader == "Phạt")
                    _allList = _allList.OrderBy(x => x.Phat ?? 0).ToList();
                else if (colHeader == "Lý do thưởng phạt")
                    _allList = _allList.OrderBy(x => x.TenLyDo).ToList();
                else if (colHeader == "Ghi chú")
                    _allList = _allList.OrderBy(x => x.GhiChu).ToList();
                else
                    _allList = _allList.OrderBy(x => x.SoPhieu).ToList();

                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null && _allList.Count > 0)
            {
                string colHeader = _clickedColumn?.Header?.ToString() ?? "";
                if (colHeader == "Ngày")
                    _allList = _allList.OrderByDescending(x => x.Ngay).ToList();
                else if (colHeader == "Nhân viên")
                    _allList = _allList.OrderByDescending(x => x.TenNhanVien).ToList();
                else if (colHeader == "Thưởng")
                    _allList = _allList.OrderByDescending(x => x.Thuong ?? 0).ToList();
                else if (colHeader == "Phạt")
                    _allList = _allList.OrderByDescending(x => x.Phat ?? 0).ToList();
                else if (colHeader == "Lý do thưởng phạt")
                    _allList = _allList.OrderByDescending(x => x.TenLyDo).ToList();
                else if (colHeader == "Ghi chú")
                    _allList = _allList.OrderByDescending(x => x.GhiChu).ToList();
                else
                    _allList = _allList.OrderByDescending(x => x.SoPhieu).ToList();

                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortBySoPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderBy(x => x.SoPhieu).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortByNgay_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderByDescending(x => x.Ngay).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortByNhanVien_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderBy(x => x.TenNhanVien).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortByThuong_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderByDescending(x => x.Thuong ?? 0).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortByPhat_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderByDescending(x => x.Phat ?? 0).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortByLyDo_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderBy(x => x.TenLyDo).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private void MenuItem_SortByGhiChu_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null)
            {
                _allList = _allList.OrderBy(x => x.GhiChu).ToList();
                DgThuongPhat.ItemsSource = null;
                DgThuongPhat.ItemsSource = _allList;
                UpdateSummaryFooter();
            }
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                Clipboard.SetText(_clickedCellValue);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgThuongPhat.SelectedItem as ThuongPhatItemViewModel;
            if (selected != null)
            {
                string text = $"{selected.SoPhieu}\t{selected.NgayStr}\t{selected.TenNhanVien}\t{selected.ThuongStr}\t{selected.PhatStr}\t{selected.TenLyDo}\t{selected.GhiChu}";
                Clipboard.SetText(text);
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgThuongPhat.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonCotHienThiWindow(DgThuongPhat);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgThuongPhat.SelectedItem as ThuongPhatItemViewModel;
            if (selected != null)
            {
                MessageBox.Show($"Phiếu: {selected.SoPhieu}\nNgày: {selected.NgayStr}\nNhân viên: {selected.TenNhanVien}\nThưởng: {selected.ThuongStr} VNĐ\nPhạt: {selected.PhatStr} VNĐ\nLý do: {selected.TenLyDo}\nGhi chú: {selected.GhiChu}", "Thuộc tính phiếu thưởng phạt", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion
    }
}
