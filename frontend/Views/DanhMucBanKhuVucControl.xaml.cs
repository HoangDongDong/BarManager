using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucBanKhuVucControl : UserControl
    {
        private LocalBanKhuVucService _service;

        public DanhMucBanKhuVucControl()
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load tree khu vực
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;

            // Load tất cả bàn mặc định
            await LoadBanData(null);
        }

        private async void TvKhuVuc_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is KhuVucViewModel selectedKhuVuc)
            {
                await LoadBanData(selectedKhuVuc.Id);
            }
        }

        private async System.Threading.Tasks.Task LoadBanData(string khuVucId)
        {
            var data = await _service.GetBanListAsync(khuVucId);
            DgBan.ItemsSource = data;
        }

        private async void BtnThemKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var selectedKhuVuc = TvKhuVuc.SelectedItem as KhuVucViewModel;
            // Nếu không chọn gì, mặc định sẽ lưu vào gốc (Tất cả)

            var win = new ThemKhuVucWindow("THÊM MỚI KHU VỰC");
            if (win.ShowDialog() == true)
            {
                int? parentId = null;
                if (selectedKhuVuc != null && !string.IsNullOrEmpty(selectedKhuVuc.Id) && int.TryParse(selectedKhuVuc.Id, out int parsedId))
                {
                    parentId = parsedId;
                }

                var newKhuVuc = new DKHUVUC
                {
                    Name = win.TenKhuVuc,
                    ParentId = parentId
                };

                bool success = await _service.InsertKhuVucAsync(newKhuVuc);
                if (success)
                {
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async void BtnSuaKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var selectedKhuVuc = TvKhuVuc.SelectedItem as KhuVucViewModel;
            if (selectedKhuVuc != null && !string.IsNullOrEmpty(selectedKhuVuc.Id))
            {
                var win = new ThemKhuVucWindow("SỬA KHU VỰC");
                if (win.ShowDialog() == true)
                {
                    if (int.TryParse(selectedKhuVuc.Id, out int parsedId))
                    {
                        var updatedKhuVuc = new DKHUVUC
                        {
                            Id = parsedId,
                            Name = win.TenKhuVuc
                        };

                        bool success = await _service.UpdateKhuVucAsync(updatedKhuVuc);
                        if (success)
                        {
                            await ReloadTreeViewAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khu vực cụ thể cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnThemThuMucKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemKhuVucWindow("TẠO MỚI THƯ MỤC KHU VỰC");
            if (win.ShowDialog() == true)
            {
                var newKhuVuc = new DKHUVUC
                {
                    Name = win.TenKhuVuc,
                    ParentId = null // Thư mục nằm ở gốc
                };

                bool success = await _service.InsertKhuVucAsync(newKhuVuc);
                if (success)
                {
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async System.Threading.Tasks.Task ReloadTreeViewAsync()
        {
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;
        }

        private async void BtnHienThiTatCa_Click(object sender, RoutedEventArgs e)
        {
            // Bỏ chọn TreeView để hiển thị tất cả
            if (TvKhuVuc.SelectedItem != null)
            {
                // Note: WPF TreeView doesn't have an easy way to clear selection, so we just load all data directly
                await LoadBanData(null);
            }
            else
            {
                await LoadBanData(null);
            }
        }

        private async void BtnTaiLaiKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;
            await LoadBanData(null);
        }

        private async void BtnThemNhanhBan_Click(object sender, RoutedEventArgs e)
        {
            var window = new ThemNhanhBanKhuVucWindow();
            if (window.ShowDialog() == true)
            {
                BtnTaiLaiKhuVuc_Click(null, null);
            }
        }

        private void BtnThemBanExcel_Click(object sender, RoutedEventArgs e)
        {
            var window = new ThemBanExcelWindow(() =>
            {
                BtnTaiLaiKhuVuc_Click(null, null);
            });
            window.ShowDialog();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = DgBan.ItemsSource as System.Collections.Generic.IEnumerable<BanViewModel>;
                if (items == null || !System.Linq.Enumerable.Any(items))
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = "DanhSachBan.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("DanhSachBan");
                        
                        worksheet.Cell(1, 1).Value = "STT";
                        worksheet.Cell(1, 2).Value = "Tên bàn";
                        worksheet.Cell(1, 3).Value = "Khu vực";
                        worksheet.Cell(1, 4).Value = "Nhóm hiển thị";
                        worksheet.Cell(1, 5).Value = "Loại phòng";
                        worksheet.Cell(1, 6).Value = "Ghi chú";
                        
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int row = 2;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = item.Stt;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.KhuVucName;
                            worksheet.Cell(row, 4).Value = item.NhomHienThiName;
                            worksheet.Cell(row, 5).Value = item.LoaiPhongName;
                            worksheet.Cell(row, 6).Value = item.Note;
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
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgBan);
            win.TxtTieuDe.Text = "Bàn";
            win.ShowDialog();
        }

        private async void BtnThemMoiBan_Click(object sender, RoutedEventArgs e)
        {
            int? selectedKhuVucId = null;

            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                if (int.TryParse(selected.Id, out int parsedId))
                {
                    selectedKhuVucId = parsedId;
                }
            }
            
            // Bật popup chọn khu vực nếu chưa có
            if (selectedKhuVucId == null)
            {
                var chonKhuVucWin = new ChonKhuVucWindow();
                if (chonKhuVucWin.ShowDialog() == true)
                {
                    if (chonKhuVucWin.SelectedKhuVuc != null && int.TryParse(chonKhuVucWin.SelectedKhuVuc.Id, out int parsedId))
                    {
                        selectedKhuVucId = parsedId;
                    }
                }
                else
                {
                    return; // Người dùng hủy
                }
            }

            var newBan = new DBAN { DkhuvucId = selectedKhuVucId };
            var banList = DgBan.ItemsSource as System.Collections.Generic.List<BanViewModel>;
            var win = new ThemMoiBanWindow(newBan, banList);
            if (win.ShowDialog() == true)
            {
                // Tải lại danh sách
                await LoadBanData(selectedKhuVucId?.ToString());
            }
        }

        private async void BtnSuaBan_Click(object sender, RoutedEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selectedRow)
            {
                if (int.TryParse(selectedRow.Id, out int banId))
                {
                    var editBan = await _service.GetBanByIdAsync(banId);
                    if (editBan != null)
                    {
                        var banList = DgBan.ItemsSource as System.Collections.Generic.List<BanViewModel>;
                        var win = new ThemMoiBanWindow(editBan, banList);
                        if (win.ShowDialog() == true)
                        {
                            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                            await LoadBanData(khuVucId);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông tin bàn trong CSDL!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một Bàn để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnXoaBan_Click(object sender, RoutedEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selectedRow)
            {
                if (int.TryParse(selectedRow.Id, out int banId))
                {
                    var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                    bool isTrash = khuVucId == "-1";

                    string message = isTrash 
                        ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN bàn '{selectedRow.Name}' không?" 
                        : $"Bạn có chắc muốn đưa bàn '{selectedRow.Name}' vào Thùng rác không?";
                    
                    var result = MessageBox.Show(message, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = await _service.DeleteBanAsync(banId, isTrash);
                        if (success)
                        {
                            await LoadBanData(khuVucId);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một Bàn để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuItem_Placeholder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang được cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void MenuItem_BieuTuongKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                if (int.TryParse(selected.Id, out int khuVucId))
                {
                    var win = new ThuVienAnhWindow();
                    if (win.ShowDialog() == true && win.SelectedIcon != null)
                    {
                        if (await _service.UpdateKhuVucIconAsync(khuVucId, win.SelectedIcon.Id, win.SelectedIcon.Anh))
                        {
                            BtnTaiLaiKhuVuc_Click(null, null);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Không thể đổi biểu tượng của node này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void MenuItem_DoiTenKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể đổi tên thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(selected.Id, out int khuVucId))
                {
                    var inputWin = new InputWindow("Đổi tên", "Nhập tên mới:", selected.Name);
                    if (inputWin.ShowDialog() == true)
                    {
                        string newName = inputWin.InputText;
                        if (!string.IsNullOrWhiteSpace(newName) && newName != selected.Name)
                        {
                            if (await _service.RenameKhuVucAsync(khuVucId, newName))
                            {
                                BtnTaiLaiKhuVuc_Click(null, null);
                            }
                        }
                    }
                }
            }
        }
    }
}
