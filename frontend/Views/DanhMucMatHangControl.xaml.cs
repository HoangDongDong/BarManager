using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucMatHangControl : UserControl
    {
        private readonly LocalMatHangService _matHangService;

        public DanhMucMatHangControl()
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
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
                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private async void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            var selectedNhom = TvNhomMatHang.SelectedItem as NhomMatHangViewModel;
            if (selectedNhom == null)
            {
                MessageBox.Show("Vui lòng chọn một thư mục hoặc nhóm mặt hàng để thêm nhóm con!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new ThemNhomWindow(isThuMuc: false);
            if (win.ShowDialog() == true)
            {
                var newGroup = new DNHOMMATHANG
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = win.TenNhom,
                    Code = win.MaSanPham,
                    DloaidoId = win.LoaiDoId,
                    ParentId = string.IsNullOrEmpty(selectedNhom.Id) ? null : selectedNhom.Id
                };

                bool success = await _matHangService.InsertNhomMatHangAsync(newGroup);
                if (success)
                {
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomWindow(isThuMuc: true);
            if (win.ShowDialog() == true)
            {
                var newGroup = new DNHOMMATHANG
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = win.TenNhom,
                    Code = win.MaSanPham,
                    DloaidoId = win.LoaiDoId,
                    ParentId = null // Thư mục nằm ở gốc
                };

                bool success = await _matHangService.InsertNhomMatHangAsync(newGroup);
                if (success)
                {
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
    }
}
