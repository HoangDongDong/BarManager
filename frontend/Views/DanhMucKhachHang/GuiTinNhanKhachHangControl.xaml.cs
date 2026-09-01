using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class NguoiNhanSmsViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Makhach { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string Dienthoai { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Chờ gửi";
    }

    public partial class GuiTinNhanKhachHangControl : UserControl
    {
        private ObservableCollection<NhomKhachHangTreeItem> _nhomTree;
        private List<KhachHangViewModel> _rawList = new List<KhachHangViewModel>();
        private NhomKhachHangTreeItem _selectedNhom;
        private ObservableCollection<NguoiNhanSmsViewModel> _dsNguoiNhan = new ObservableCollection<NguoiNhanSmsViewModel>();

        public GuiTinNhanKhachHangControl()
        {
            InitializeComponent();
            DgNguoiNhan.ItemsSource = _dsNguoiNhan;
            this.IsVisibleChanged += GuiTinNhanKhachHangControl_IsVisibleChanged;
        }

        private async void GuiTinNhanKhachHangControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await LoadDataAsync();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _nhomTree = await LocalKhachHangService.GetNhomKhachHangTreeAsync();
                TvNhomKhachHang.ItemsSource = _nhomTree;

                if (_nhomTree.Count > 0)
                {
                    _selectedNhom = _nhomTree[0];
                }

                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshGridAsync()
        {
            try
            {
                string searchKeyword = TxtLoc.Text.Trim();
                string filterId = _selectedNhom?.Id ?? "ALL";
                int itemType = _selectedNhom?.ItemType ?? 0;

                var list = await LocalKhachHangService.GetKhachHangListAsync(filterId, itemType, 0, searchKeyword);
                _rawList = list;

                int stt = 1;
                foreach (var item in _rawList)
                {
                    item.Stt = stt++;
                }

                DgKhachHang.ItemsSource = null;
                DgKhachHang.ItemsSource = _rawList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RefreshGridAsync: " + ex.Message);
            }
        }

        private async void TvNhomKhachHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem selected)
            {
                _selectedNhom = selected;
                await RefreshGridAsync();
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            await RefreshGridAsync();
        }

        #region Chức năng chuyển khách hàng sang danh sách nhận SMS (Trái sang Phải)

        private static bool IsValidVietnameseMobile(string rawPhone, out string cleanPhone)
        {
            cleanPhone = string.Empty;
            if (string.IsNullOrWhiteSpace(rawPhone)) return false;

            // Loại bỏ khoảng trắng, dấu chấm, gạch ngang, ngoặc
            cleanPhone = Regex.Replace(rawPhone.Trim(), @"[^\d+]", "");

            // Chuẩn hóa đầu số quốc tế
            if (cleanPhone.StartsWith("+84"))
            {
                cleanPhone = "0" + cleanPhone.Substring(3);
            }
            else if (cleanPhone.StartsWith("84") && cleanPhone.Length >= 11)
            {
                cleanPhone = "0" + cleanPhone.Substring(2);
            }

            // Kiểm tra số điện thoại di động Việt Nam 10 số (đầu 03, 05, 07, 08, 09)
            // Ví dụ: 0358010437, 0984568485, v.v.
            return Regex.IsMatch(cleanPhone, @"^0(3[2-9]|5[25689]|7[06-9]|8[1-9]|9[0-9])\d{7}$");
        }

        private void BtnThemNguoiNhan_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgKhachHang.SelectedItems.Cast<KhachHangViewModel>().ToList();
            if (selectedList.Count == 0 && DgKhachHang.SelectedItem is KhachHangViewModel single)
            {
                selectedList.Add(single);
            }

            if (selectedList.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần thêm vào danh sách gửi tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int addedCount = 0;
            foreach (var cust in selectedList)
            {
                if (IsValidVietnameseMobile(cust.Dienthoai, out string validMobile))
                {
                    // Kiểm tra xem số điện thoại hoặc ID đã tồn tại trong danh sách nhận hay chưa
                    bool alreadyExists = _dsNguoiNhan.Any(n => n.Dienthoai == validMobile || (!string.IsNullOrEmpty(n.Id) && n.Id == cust.Id));
                    if (!alreadyExists)
                    {
                        _dsNguoiNhan.Add(new NguoiNhanSmsViewModel
                        {
                            Stt = _dsNguoiNhan.Count + 1,
                            Id = cust.Id ?? "",
                            Makhach = cust.Makhach ?? "",
                            Ten = cust.Name ?? "",
                            Dienthoai = validMobile,
                            TrangThai = ""
                        });
                        addedCount++;
                    }
                }
            }

            // Nếu không có khách hàng nào được thêm (do trùng hoặc sai SĐT)
            if (addedCount == 0)
            {
                MessageBox.Show("Các khách hàng đang chọn đã nằm trong danh sách hoặc số điện thoại không đúng", 
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ReindexNguoiNhan();
            }
        }

        private void BtnXoaNguoiNhan_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecipients = DgNguoiNhan.SelectedItems.Cast<NguoiNhanSmsViewModel>().ToList();
            if (selectedRecipients.Count == 0 && DgNguoiNhan.SelectedItem is NguoiNhanSmsViewModel single)
            {
                selectedRecipients.Add(single);
            }

            if (selectedRecipients.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn người nhận cần xóa khỏi danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var item in selectedRecipients)
            {
                _dsNguoiNhan.Remove(item);
            }

            ReindexNguoiNhan();
        }

        private void ReindexNguoiNhan()
        {
            int idx = 1;
            foreach (var item in _dsNguoiNhan)
            {
                item.Stt = idx++;
            }
            DgNguoiNhan.Items.Refresh();
        }

        private void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            if (_dsNguoiNhan.Count == 0)
            {
                MessageBox.Show("Danh sách người nhận đang trống! Vui lòng chọn và thêm khách hàng trước khi thực hiện gửi tin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var inputWin = new InputWindow("Soạn nội dung tin nhắn", $"Nội dung tin nhắn gửi tới {_dsNguoiNhan.Count} khách hàng:", "");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string msg = inputWin.InputText?.Trim() ?? "";
                if (string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show("Nội dung tin nhắn không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var item in _dsNguoiNhan)
                {
                    item.TrangThai = "Đã gửi";
                }
                DgNguoiNhan.Items.Refresh();

                MessageBox.Show($"Đã gửi tin nhắn thành công tới {_dsNguoiNhan.Count} khách hàng!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region Các nút Toolbar khác
        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemKhachHangWindow((string)null, _nhomTree?[0]?.Children);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => await RefreshGridAsync();
            win.ShowDialog();
            await RefreshGridAsync();
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                var win = new ThemKhachHangWindow(selected.Id, _nhomTree?[0]?.Children);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => await RefreshGridAsync();
                win.ShowDialog();
                await RefreshGridAsync();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangViewModel selected)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{selected.Name}' ({selected.Makhach})?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    bool ok = await LocalKhachHangService.DeleteKhachHangAsync(selected.Id);
                    if (ok) await RefreshGridAsync();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhanhKhachHangBangExcelWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true) await RefreshGridAsync();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            var items = (DgKhachHang.ItemsSource as System.Collections.IEnumerable)?.Cast<KhachHangViewModel>().ToList() ?? _rawList;
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu khách hàng để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Xuất danh sách khách hàng ra Excel",
                FileName = "DanhSachKhachHang.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("KhachHang");
                        string[] headers = new[]
                        {
                            "STT", "Mã khách", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email",
                            "Nhóm khách hàng", "Mã số thuế", "Nhân viên", "Tỉnh thành", "Facebook",
                            "Thẻ trả trước", "Ghi chú"
                        };

                        for (int col = 0; col < headers.Length; col++)
                        {
                            worksheet.Cell(1, col + 1).Value = headers[col];
                        }

                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dfe9f5");
                        headerRow.Height = 25;

                        int row = 2;
                        int stt = 1;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = stt++;
                            worksheet.Cell(row, 2).Value = item.Makhach ?? "";
                            worksheet.Cell(row, 3).Value = item.Name ?? "";
                            worksheet.Cell(row, 4).Value = item.Diachi ?? "";
                            worksheet.Cell(row, 5).Value = item.Dienthoai ?? "";
                            worksheet.Cell(row, 6).Value = item.Email ?? "";
                            worksheet.Cell(row, 7).Value = item.TenNhomKhachHang ?? "";
                            worksheet.Cell(row, 8).Value = item.Masothue ?? "";
                            worksheet.Cell(row, 9).Value = item.TenNhanVien ?? "";
                            worksheet.Cell(row, 10).Value = item.TinhThanh ?? "";
                            worksheet.Cell(row, 11).Value = item.Facebook ?? "";
                            worksheet.Cell(row, 12).Value = item.TheTraTruoc ?? "";
                            worksheet.Cell(row, 13).Value = item.Note ?? "";
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    MessageBox.Show($"Đã xuất {items.Count} khách hàng ra Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgKhachHang, "Khách hàng");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Tổng số khách hàng: {_rawList.Count}", "Thống kê", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomKhachHangWindow();
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () => { await LoadDataAsync(); };
            if (win.ShowDialog() == true) await LoadDataAsync();
        }

        private async void BtnSuaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.ItemType == 2)
            {
                var win = new ThemNhomKhachHangWindow(_selectedNhom.Id);
                win.Owner = Window.GetWindow(this);
                win.OnSaved += async () => { await LoadDataAsync(); };
                if (win.ShowDialog() == true) await LoadDataAsync();
            }
        }

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Tạo mới thư mục", "Nhập tên thư mục mới:", "Thư mục mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "";
                if (!string.IsNullOrEmpty(folderName))
                {
                    string parentFolderId = (_selectedNhom != null && _selectedNhom.Icon == "📁") ? _selectedNhom.Id : null;
                    await LocalKhachHangService.SaveNhomKhachHangFolderAsync(null, folderName, true, parentFolderId);
                    await LoadDataAsync();
                }
            }
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }
        #endregion
    }
}
