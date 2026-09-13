using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class ThemKhachHangTouchWindow : Window
    {
        private Models.DKHACHHANG? _editingKhachHang;

        public ThemKhachHangTouchWindow(Models.DKHACHHANG? khachHang = null)
        {
            InitializeComponent();
            _editingKhachHang = khachHang;
        }

        private List<LookupItemVM> _allNhomKhach = new();
        private List<LookupItemVM> _allNhanVien = new();
        private List<LookupItemVM> _allTinhThanh = new();

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();

            if (_editingKhachHang != null)
            {
                PopulateForm(_editingKhachHang);
            }
            else
            {
                string nextCode = await LocalKhachHangService.GetNextMaKhachAsync();
                TxtMaKhach.Text = nextCode;
            }
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                var nhomData = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                _allNhomKhach = nhomData.Select(x => new LookupItemVM
                {
                    Id = x.ID?.ToString() ?? "",
                    Name = x.NAME?.ToString() ?? ""
                }).ToList();
                CboNhomKhach.ItemsSource = _allNhomKhach;
                CboNhomKhach.DisplayMemberPath = "Name";
                CboNhomKhach.SelectedValuePath = "Id";

                var nvData = await LocalKhachHangService.GetNhanVienLookupAsync();
                _allNhanVien = nvData.Select(x => new LookupItemVM
                {
                    Id = x.ID?.ToString() ?? "",
                    Name = x.NAME?.ToString() ?? ""
                }).ToList();
                CboNhanVien.ItemsSource = _allNhanVien;
                CboNhanVien.DisplayMemberPath = "Name";
                CboNhanVien.SelectedValuePath = "Id";

                var tinhData = await LocalKhachHangService.GetTinhThanhLookupAsync();
                _allTinhThanh = tinhData.Select(x => new LookupItemVM
                {
                    Id = x.ID?.ToString() ?? "",
                    Name = x.NAME?.ToString() ?? ""
                }).ToList();
                CboTinhThanh.ItemsSource = _allTinhThanh;
                CboTinhThanh.DisplayMemberPath = "Name";
                CboTinhThanh.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi load lookups: {ex.Message}");
            }
        }

        private void PopulateForm(Models.DKHACHHANG kh)
        {
            TxtMaKhach.Text = kh.Makhach ?? "";
            TxtTenKhach.Text = kh.Name ?? "";
            TxtDiaChi.Text = kh.Diachi ?? "";
            TxtDienThoai.Text = kh.Dienthoai ?? "";
            TxtEmail.Text = kh.Email ?? "";
            TxtMaSoThue.Text = kh.Masothue ?? "";
            TxtFacebook.Text = kh.Facebook ?? "";
            TxtGhiChu.Text = kh.Note ?? "";
            TxtDiemTichLuy.Text = kh.Diemtichluybandau?.ToString() ?? "0";
            DpNgaySinh.SelectedDate = kh.Ngaysinh;

            if (!string.IsNullOrEmpty(kh.DnhomkhachhangId))
                CboNhomKhach.SelectedValue = kh.DnhomkhachhangId;
            if (!string.IsNullOrEmpty(kh.DnhanvienId))
                CboNhanVien.SelectedValue = kh.DnhanvienId;
            if (!string.IsNullOrEmpty(kh.DtinhthanhId))
                CboTinhThanh.SelectedValue = kh.DtinhthanhId;
        }

        private void BtnTruoc_Click(object sender, RoutedEventArgs e) { }
        private void BtnSau_Click(object sender, RoutedEventArgs e) { }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            string nextCode = await LocalKhachHangService.GetNextMaKhachAsync();
            TxtMaKhach.Text = nextCode;
        }

        private void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _editingKhachHang = null;
            TxtMaKhach.Text = "";
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_editingKhachHang?.Id)) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa khách hàng này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalKhachHangService.DeleteKhachHangAsync(_editingKhachHang.Id);
                if (ok)
                {
                    MessageBox.Show("Đã xóa khách hàng!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void BtnTouchKb_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Parent is Grid grid)
            {
                var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
                if (tb != null)
                {
                    var kb = new TouchKeyboardWindow(tb.Text, false, "MỜI BẠN NHẬP DỮ LIỆU");
                    kb.Owner = this;
                    if (kb.ShowDialog() == true)
                    {
                        tb.Text = kb.ResultText;
                    }
                }
            }
        }

        private void CboNhomKhach_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            OpenSelectNhomKhach();
        }

        private void BtnSearchNhomKhach_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectNhomKhach();
        }

        private void OpenSelectNhomKhach()
        {
            var dlg = new ChonLookupTouchWindow("CHỌN NHÓM KHÁCH HÀNG", _allNhomKhach, async () =>
            {
                var data = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                _allNhomKhach = data.Select(x => new LookupItemVM { Id = x.ID?.ToString() ?? "", Name = x.NAME?.ToString() ?? "" }).ToList();
                CboNhomKhach.ItemsSource = _allNhomKhach;
                return _allNhomKhach;
            });
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                if (dlg.SelectedItem != null)
                {
                    CboNhomKhach.SelectedValue = dlg.SelectedItem.Id;
                }
            }
        }

        private void CboNhanVien_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            OpenSelectNhanVien();
        }

        private void BtnSearchNhanVien_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectNhanVien();
        }

        private void OpenSelectNhanVien()
        {
            var dlg = new ChonLookupTouchWindow("CHỌN NHÂN VIÊN", _allNhanVien, async () =>
            {
                var data = await LocalKhachHangService.GetNhanVienLookupAsync();
                _allNhanVien = data.Select(x => new LookupItemVM { Id = x.ID?.ToString() ?? "", Name = x.NAME?.ToString() ?? "" }).ToList();
                CboNhanVien.ItemsSource = _allNhanVien;
                return _allNhanVien;
            });
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                if (dlg.SelectedItem != null)
                {
                    CboNhanVien.SelectedValue = dlg.SelectedItem.Id;
                }
            }
        }

        private void CboTinhThanh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            OpenSelectTinhThanh();
        }

        private void BtnSearchTinhThanh_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectTinhThanh();
        }

        private void OpenSelectTinhThanh()
        {
            var dlg = new ChonLookupTouchWindow("CHỌN TỈNH / THÀNH PHỐ", _allTinhThanh, async () =>
            {
                var data = await LocalKhachHangService.GetTinhThanhLookupAsync();
                _allTinhThanh = data.Select(x => new LookupItemVM { Id = x.ID?.ToString() ?? "", Name = x.NAME?.ToString() ?? "" }).ToList();
                CboTinhThanh.ItemsSource = _allTinhThanh;
                return _allTinhThanh;
            });
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                if (dlg.SelectedItem != null)
                {
                    CboTinhThanh.SelectedValue = dlg.SelectedItem.Id;
                }
            }
        }

        private void BtnCalcDiem_Click(object sender, RoutedEventArgs e) { }
        private void BtnDatePicker_Click(object sender, RoutedEventArgs e) { DpNgaySinh.IsDropDownOpen = true; }

        private void ClearForm()
        {
            _editingKhachHang = null;
            TxtMaKhach.Text = "";
            TxtTenKhach.Text = "";
            TxtDiaChi.Text = "";
            TxtDienThoai.Text = "";
            TxtEmail.Text = "";
            TxtMaSoThue.Text = "";
            TxtFacebook.Text = "";
            TxtGhiChu.Text = "";
            TxtDiemTichLuy.Text = "";
            DpNgaySinh.SelectedDate = null;
            CboNhomKhach.SelectedIndex = -1;
            CboNhanVien.SelectedIndex = -1;
            CboTinhThanh.SelectedIndex = -1;
        }

        private async Task<bool> SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtTenKhach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                bool isNew = _editingKhachHang == null;
                var model = new KhachHangViewModel
                {
                    Id = _editingKhachHang?.Id?.ToString(),
                    Makhach = TxtMaKhach.Text.Trim(),
                    Name = TxtTenKhach.Text.Trim(),
                    Diachi = TxtDiaChi.Text.Trim(),
                    Dienthoai = TxtDienThoai.Text.Trim(),
                    Email = TxtEmail.Text.Trim(),
                    Masothue = TxtMaSoThue.Text.Trim(),
                    Facebook = TxtFacebook.Text.Trim(),
                    Note = TxtGhiChu.Text.Trim(),
                    DnhomkhachhangId = CboNhomKhach.SelectedValue?.ToString(),
                    TenNhanVien = CboNhanVien.SelectedValue?.ToString(),
                    TinhThanh = CboTinhThanh.SelectedValue?.ToString(),
                    Ngaysinh = DpNgaySinh.SelectedDate,
                    Diemtichluy = decimal.TryParse(TxtDiemTichLuy.Text, out decimal diem) ? diem : 0
                };

                bool ok = await LocalKhachHangService.SaveKhachHangAsync(model, isNew);
                if (ok)
                {
                    MessageBox.Show("Đã lưu thông tin khách hàng!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else
                {
                    MessageBox.Show("Lưu thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu thông tin: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e) { await SaveAsync(); }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                ClearForm();
                string nextCode = await LocalKhachHangService.GetNextMaKhachAsync();
                TxtMaKhach.Text = nextCode;
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
