using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucMatHangControl : UserControl
    {
        private readonly LocalMatHangService _matHangService;
        private ObservableCollection<MatHangInMaVach> _matHangInMaVachList;

        public DanhMucMatHangControl()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _matHangInMaVachList = new ObservableCollection<MatHangInMaVach>();
            DgInMaVach.ItemsSource = _matHangInMaVachList;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load danh sách nhóm mặt hàng lên TreeView
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
            
            // Load toàn bộ mặt hàng mặc định (ID = 0 hoặc null)
            LoadMatHangData(null);
        }

        private void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomMatHangViewModel selectedNhom)
            {
                // Nếu chọn "Tất cả" (Id = string.Empty) thì truyền null để lấy hết
                string filterId = string.IsNullOrEmpty(selectedNhom.Id) ? null : selectedNhom.Id;
                LoadMatHangData(filterId);
            }
        }

        private async void LoadMatHangData(string nhomId)
        {
            var data = await _matHangService.GetMatHangListAsync(nhomId);
            DgMatHang.ItemsSource = data;
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var chonDuLieuWin = new ChonDuLieuWindow();
            if (chonDuLieuWin.ShowDialog() == true)
            {
                var selectedNhom = chonDuLieuWin.SelectedNhomMatHang;
                string nhomId = selectedNhom != null ? selectedNhom.Id : null;

                var list = DgMatHang.ItemsSource as System.Collections.Generic.List<MatHangViewModel>;
                var selectedMatHang = DgMatHang.SelectedItem as MatHangViewModel;
                int initialIndex = selectedMatHang != null && list != null ? list.IndexOf(selectedMatHang) : (list != null && list.Count > 0 ? 0 : -1);

                var themMoiWin = new ThemMoiMatHangWindow(nhomId, null, list, initialIndex, ReloadMatHangGrid);
                themMoiWin.ShowDialog();
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selectedMatHang)
            {
                var list = DgMatHang.ItemsSource as System.Collections.Generic.List<MatHangViewModel>;
                int initialIndex = list != null ? list.IndexOf(selectedMatHang) : -1;
                var themMoiWin = new ThemMoiMatHangWindow(selectedMatHang.DnhommathangId, selectedMatHang.Id, list, initialIndex, ReloadMatHangGrid);
                themMoiWin.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is MatHangViewModel selectedMatHang)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa mặt hàng '{selectedMatHang.Name}' không?", 
                                             "Xác nhận xóa", 
                                             MessageBoxButton.YesNo, 
                                             MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _matHangService.DeleteMatHangAsync(selectedMatHang.Id);
                    if (success)
                    {
                        ReloadMatHangGrid();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ReloadMatHangGrid()
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel currentNhom)
            {
                LoadMatHangData(string.IsNullOrEmpty(currentNhom.Id) ? null : currentNhom.Id);
            }
            else
            {
                LoadMatHangData(null);
            }
        }

        private async void ReloadAllData()
        {
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
            ReloadMatHangGrid();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            var items = DgMatHang.ItemsSource as System.Collections.Generic.IEnumerable<MatHangViewModel>;
            if (items == null || !System.Linq.Enumerable.Any(items))
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachMatHang.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("MatHang");
                        
                        // Header
                        worksheet.Cell(1, 1).Value = "STT";
                        worksheet.Cell(1, 2).Value = "Tên mặt hàng";
                        worksheet.Cell(1, 3).Value = "Nhóm mặt hàng";
                        worksheet.Cell(1, 4).Value = "Loại mặt hàng";
                        worksheet.Cell(1, 5).Value = "Đơn vị tính";
                        worksheet.Cell(1, 6).Value = "Giá bán";
                        worksheet.Cell(1, 7).Value = "Giá nhập";
                        worksheet.Cell(1, 8).Value = "ĐVT chẵn";
                        worksheet.Cell(1, 9).Value = "Quy đổi";
                        worksheet.Cell(1, 10).Value = "Giá bán chẵn";
                        worksheet.Cell(1, 11).Value = "Mã hàng";
                        worksheet.Cell(1, 12).Value = "Tạm khóa";
                        worksheet.Cell(1, 13).Value = "Giá theo thời giá";
                        
                        // Format header
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int row = 2;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = item.Stt;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.NhomMatHangName;
                            worksheet.Cell(row, 4).Value = item.LoaiMatHangName;
                            worksheet.Cell(row, 5).Value = item.DonViTinhName;
                            worksheet.Cell(row, 6).Value = item.Giaban;
                            worksheet.Cell(row, 7).Value = item.Gianhap;
                            worksheet.Cell(row, 8).Value = item.DonViTinhChanName;
                            worksheet.Cell(row, 9).Value = item.Quydoi;
                            worksheet.Cell(row, 10).Value = item.Giabanchan;
                            worksheet.Cell(row, 11).Value = item.Code;
                            worksheet.Cell(row, 12).Value = item.Tamkhoa;
                            worksheet.Cell(row, 13).Value = item.Giatheothoigia;
                            row++;
                        }
                        
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    var result = MessageBox.Show("Xuất Excel thành công! Bạn có muốn mở file vừa xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhWindow(ReloadAllData);
            win.ShowDialog();
        }

        private void BtnImportDinhLuong_Click(object sender, RoutedEventArgs e)
        {
            var win = new ImportDinhLuongWindow();
            win.ShowDialog();
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgMatHang);
            win.ShowDialog();
        }

        private async void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            var selectedNhom = TvNhomMatHang.SelectedItem as NhomMatHangViewModel;
            if (selectedNhom == null)
            {
                MessageBox.Show("Vui lòng chọn một thư mục hoặc nhóm mặt hàng để thêm nhóm con!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedNhom.Id))
            {
                MessageBox.Show("Không thể thêm con vào nút gốc này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int index = 1;
            string defaultName = $"Thư mục {index}";
            while (selectedNhom.Children != null && selectedNhom.Children.Any(x => x.Name == defaultName))
            {
                index++;
                defaultName = $"Thư mục {index}";
            }

            var newNhom = new DNHOMMATHANG
            {
                Id = Guid.NewGuid().ToString(),
                Name = defaultName,
                Code = "TMP",
                ParentId = selectedNhom.Id,
                Timecreated = DateTime.Now
            };

            bool success = await _matHangService.InsertNhomMatHangAsync(newNhom);
            if (success)
            {
                await ReloadTreeViewAsync();
                var items = TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>;
                var node = FindNode(items, selectedNhom.Id);
                if (node != null)
                {
                    var newItem = node.Children.FirstOrDefault(x => x.Name == defaultName);
                    if (newItem != null)
                    {
                        if (TvNhomMatHang.ItemContainerGenerator.ContainerFromItem(node) is TreeViewItem tvi)
                        {
                            tvi.IsExpanded = true;
                            tvi.UpdateLayout();
                        }
                        
                        await Application.Current.Dispatcher.InvokeAsync(() => 
                        {
                            newItem.IsEditing = true;
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
        }

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var items = TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>;
            if (items == null) return;
            
            // Lấy danh mục gốc "Tất cả" (parent = null)
            var rootNode = items.FirstOrDefault();
            if (rootNode == null) return;

            int index = 1;
            string defaultName = $"Thư mục {index}";
            while (rootNode.Children.Any(x => x.Name == defaultName))
            {
                index++;
                defaultName = $"Thư mục {index}";
            }

            var newNhom = new DNHOMMATHANG
            {
                Id = Guid.NewGuid().ToString(),
                Name = defaultName,
                Code = "TMP",
                ParentId = null,
                Timecreated = DateTime.Now
            };

            bool success = await _matHangService.InsertNhomMatHangAsync(newNhom);
            if (success)
            {
                await ReloadTreeViewAsync();
                items = TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>;
                rootNode = items.FirstOrDefault();
                if (rootNode != null)
                {
                    var newItem = rootNode.Children.FirstOrDefault(x => x.Name == defaultName);
                    if (newItem != null)
                    {
                        if (TvNhomMatHang.ItemContainerGenerator.ContainerFromItem(rootNode) is TreeViewItem tvi)
                        {
                            tvi.IsExpanded = true;
                            tvi.UpdateLayout();
                        }
                        
                        await Application.Current.Dispatcher.InvokeAsync(() => 
                        {
                            newItem.IsEditing = true;
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
        }

        private NhomMatHangViewModel FindNode(System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel> nodes, string id)
        {
            if (nodes == null) return null;
            foreach (var node in nodes)
            {
                if (node.Id == id) return node;
                var child = FindNode(node.Children, id);
                if (child != null) return child;
            }
            return null;
        }

        private bool IsDuplicateName(System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel> tree, string name, string excludeId)
        {
            if (tree == null) return false;
            foreach (var node in tree)
            {
                if (node.Name != null && node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && node.Id != excludeId)
                    return true;
                if (node.Children != null && IsDuplicateName(node.Children, name, excludeId))
                    return true;
            }
            return false;
        }

        private void InlineEditTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt && txt.Visibility == Visibility.Visible)
            {
                txt.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txt.Focus();
                    System.Windows.Input.Keyboard.Focus(txt);
                    txt.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void InlineEditTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt && (bool)e.NewValue == true)
            {
                txt.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txt.Focus();
                    System.Windows.Input.Keyboard.Focus(txt);
                    txt.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private async void InlineEditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt)
            {
                string id = txt.Tag as string;
                if (!string.IsNullOrEmpty(id) && txt.DataContext is NhomMatHangViewModel model && model.IsEditing)
                {
                    if (string.IsNullOrWhiteSpace(model.Name))
                    {
                        model.IsEditing = false;
                        await ReloadTreeViewAsync();
                        return;
                    }

                    if (IsDuplicateName(TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>, model.Name, id))
                    {
                        MessageBox.Show($"Tên '{model.Name}' đã tồn tại. Hệ thống sẽ khôi phục tên cũ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        model.IsEditing = false;
                        await ReloadTreeViewAsync();
                        return;
                    }

                    model.IsEditing = false;
                    await UpdateGroupName(id, model.Name);
                }
            }
        }

        private async void InlineEditTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox txt)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    string id = txt.Tag as string;
                    if (!string.IsNullOrEmpty(id) && txt.DataContext is NhomMatHangViewModel model && model.IsEditing)
                    {
                        if (string.IsNullOrWhiteSpace(model.Name))
                        {
                            model.IsEditing = false;
                            await ReloadTreeViewAsync();
                            e.Handled = true;
                            return;
                        }

                        if (IsDuplicateName(TvNhomMatHang.ItemsSource as System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel>, model.Name, id))
                        {
                            MessageBox.Show($"Tên '{model.Name}' đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Handled = true;
                            return;
                        }

                        model.IsEditing = false;
                        await UpdateGroupName(id, model.Name);
                    }
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    if (txt.DataContext is NhomMatHangViewModel model && model.IsEditing)
                    {
                        model.IsEditing = false;
                        await ReloadTreeViewAsync();
                    }
                    e.Handled = true;
                }
            }
        }

        private async System.Threading.Tasks.Task UpdateGroupName(string id, string newName)
        {
            // Cần truy vấn group từ DB và update
            var groups = await _matHangService.GetAllNhomMatHangAsync();
            var group = groups.FirstOrDefault(g => g.Id == id);
            if (group != null)
            {
                group.Name = newName;
                await _matHangService.UpdateNhomMatHangAsync(group);
            }
        }

        private void MenuItem_ThemMoi_Click(object sender, RoutedEventArgs e)
        {
            BtnThemThuMuc_Click(null, null);
        }

        private void MenuItem_ThemMoiCon_Click(object sender, RoutedEventArgs e)
        {
            BtnThemNhom_Click(null, null);
        }

        private void MenuItem_SuaDoi_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel selected)
            {
                if (string.IsNullOrEmpty(selected.Id))
                {
                    MessageBox.Show("Không thể sửa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                selected.IsEditing = true;
            }
        }

        private async void MenuItem_Xoa_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang.SelectedItem is NhomMatHangViewModel selected)
            {
                if (string.IsNullOrEmpty(selected.Id))
                {
                    MessageBox.Show("Không thể xóa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{selected.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await _matHangService.DeleteNhomMatHangAsync(selected.Id);
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await ReloadTreeViewAsync();
        }

        private async System.Threading.Tasks.Task ReloadTreeViewAsync()
        {
            var treeData = await _matHangService.GetNhomMatHangTreeAsync();
            TvNhomMatHang.ItemsSource = treeData;
        }

        private void BtnInMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (PanelInMaVach.Visibility == Visibility.Visible)
            {
                PanelInMaVach.Visibility = Visibility.Collapsed;
                GsInMaVach.Visibility = Visibility.Collapsed;
                ColInMaVach.Width = new GridLength(0);
            }
            else
            {
                PanelInMaVach.Visibility = Visibility.Visible;
                GsInMaVach.Visibility = Visibility.Visible;
                ColInMaVach.Width = new GridLength(350); // Mở rộng cột bên phải
            }
        }

        private void BtnThemMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItems.Count > 0)
            {
                bool hasExisting = false;
                foreach (MatHangViewModel item in DgMatHang.SelectedItems)
                {
                    // Check if already exists by Id
                    var existing = _matHangInMaVachList.FirstOrDefault(x => x.Id == item.Id);
                    if (existing != null)
                    {
                        hasExisting = true;
                    }
                    else
                    {
                        _matHangInMaVachList.Add(new MatHangInMaVach
                        {
                            Id = item.Id,
                            Code = item.Code,
                            Name = item.Name,
                            Quantity = 1
                        });
                    }
                }
                
                if (hasExisting)
                {
                    MessageBox.Show("Các mặt hàng đang chọn đã nằm trong danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn mặt hàng để thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXoaMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (DgInMaVach.SelectedItems.Count > 0)
            {
                var itemsToRemove = DgInMaVach.SelectedItems.Cast<MatHangInMaVach>().ToList();
                foreach (var item in itemsToRemove)
                {
                    _matHangInMaVachList.Remove(item);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn mặt hàng để xóa khỏi danh sách in mã vạch!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXuatExcelMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (_matHangInMaVachList == null || _matHangInMaVachList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachInMaVach.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("InMaVach");
                        
                        // Header
                        worksheet.Cell(1, 1).Value = "Mã";
                        worksheet.Cell(1, 2).Value = "Tên";
                        worksheet.Cell(1, 3).Value = "Số lượng";
                        
                        // Format header
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int row = 2;
                        foreach (var item in _matHangInMaVachList)
                        {
                            worksheet.Cell(row, 1).Value = item.Code;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.Quantity;
                            row++;
                        }
                        
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    var result = MessageBox.Show("Xuất Excel thành công! Bạn có muốn mở file vừa xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnInBarcode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng in mã vạch đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBartender_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng in Bartender đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
