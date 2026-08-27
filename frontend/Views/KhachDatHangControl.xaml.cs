using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class KhachDatHangControl : UserControl
    {
        private LocalKhachDatHangService _service;
        private string _currentCategoryId = null;
        private bool _isMucDichDatMode = false;

        public KhachDatHangControl()
        {
            InitializeComponent();
            _service = new LocalKhachDatHangService();
            
            // Default filter past month
            DpTuNgay.SelectedDate = DateTime.Now.AddMonths(-1);
            DpDenNgay.SelectedDate = DateTime.Now;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadTreeAsync();
            await LoadData();
        }
        
        private async System.Threading.Tasks.Task ReloadTreeAsync()
        {
            var treeData = await _service.GetTreeAsync(_isMucDichDatMode);
            TvCategoryTree.ItemsSource = treeData;
        }

        private void BtnModeSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (BtnModeSwitch.ContextMenu != null)
            {
                BtnModeSwitch.ContextMenu.PlacementTarget = BtnModeSwitch;
                BtnModeSwitch.ContextMenu.Placement = PlacementMode.Bottom;
                BtnModeSwitch.ContextMenu.IsOpen = true;
                BtnModeSwitch.IsChecked = false;
            }
        }

        private async void MenuItemModePhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            _isMucDichDatMode = false;
            TxtTreeHeader.Text = "Phương thức đặt";
            _currentCategoryId = null;
            await ReloadTreeAsync();
            await LoadData();
        }

        private async void MenuItemModeMucDich_Click(object sender, RoutedEventArgs e)
        {
            _isMucDichDatMode = true;
            TxtTreeHeader.Text = "Mục đích đặt";
            _currentCategoryId = null;
            await ReloadTreeAsync();
            await LoadData();
        }

        private async void TvPhuongThucDat_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeCategoryViewModel selectedNode)
            {
                _currentCategoryId = selectedNode.Id;
                await LoadData();
            }
        }
        
        private async void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            DateTime? tuNgay = DpTuNgay.SelectedDate;
            DateTime? denNgay = DpDenNgay.SelectedDate;
            
            var data = await _service.GetDatHangListAsync(_currentCategoryId, _isMucDichDatMode, tuNgay, denNgay);
            DgDatHang.ItemsSource = data;
        }

        private async void DgDatHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDatHang.SelectedItem is DatHangViewModel selectedOrder)
            {
                var chiTiet = await _service.GetDatHangChiTietListAsync(selectedOrder.Id);
                DgDatHangChiTiet.ItemsSource = chiTiet;
            }
            else
            {
                DgDatHangChiTiet.ItemsSource = null;
            }
        }

        private async void BtnRefreshPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            await ReloadTreeAsync();
        }

        private void BtnThemPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            string parentId = null;
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                parentId = selected.Id;
            }

            var win = new ThemPhuongThucDatWindow();
            win.ParentId = parentId;
            win.IsMucDichDat = _isMucDichDatMode;
            win.OnSaveSuccess = () => BtnRefreshPhuongThuc_Click(null, null);
            win.ShowDialog();
        }

        private void BtnSuaPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể sửa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var win = new ThemPhuongThucDatWindow(selected);
                win.IsMucDichDat = _isMucDichDatMode;
                win.OnSaveSuccess = () => BtnRefreshPhuongThuc_Click(null, null);
                win.ShowDialog();
            }
        }

        private async void BtnXoaPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể xóa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string typeName = _isMucDichDatMode ? "mục đích đặt" : "phương thức đặt";
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa {typeName} '{selected.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (await _service.DeletePhuongThucDatAsync(selected.Id, _isMucDichDatMode))
                    {
                        BtnRefreshPhuongThuc_Click(null, null);
                    }
                }
            }
        }

        private async void BtnDoiTenPhuongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể đổi tên thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var inputWin = new InputWindow("Đổi tên", "Nhập tên mới:", selected.Name);
                if (inputWin.ShowDialog() == true)
                {
                    string newName = inputWin.InputText;
                    if (!string.IsNullOrWhiteSpace(newName) && newName != selected.Name)
                    {
                        if (await _service.UpdatePhuongThucDatAsync(selected.Id, newName, selected.Note, selected.SimageId, _isMucDichDatMode))
                        {
                            BtnRefreshPhuongThuc_Click(null, null);
                        }
                    }
                }
            }
        }

        private async void BtnLenTren_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected && selected.Id != null)
            {
                // Logic đổi SortOrder với phần tử trên
                MessageBox.Show("Chức năng đang được cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnXuongDuoi_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected && selected.Id != null)
            {
                // Logic đổi SortOrder với phần tử dưới
                MessageBox.Show("Chức năng đang được cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLamCon_Click(object sender, RoutedEventArgs e)
        {
            if (TvCategoryTree.SelectedItem is TreeCategoryViewModel selected && selected.Id != null)
            {
                // Logic chuyển parentId sang node trên liền kề
                MessageBox.Show("Chức năng đang được cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
