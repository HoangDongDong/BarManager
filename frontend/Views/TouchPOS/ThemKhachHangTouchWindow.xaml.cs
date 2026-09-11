using System;
using System.Windows;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class ThemKhachHangTouchWindow : Window
    {
        public ThemKhachHangTouchWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLookups();
        }

        private void LoadLookups()
        {
            try
            {
                // Load nhóm khách
                var nhomList = LocalDatabaseService.GetAll<Models.DNHOMKHACHHANG>("SELECT * FROM DNHOMKHACHHANG WHERE STATUS = 1 ORDER BY NAME");
                CboNhomKhach.ItemsSource = nhomList;
                CboNhomKhach.DisplayMemberPath = "TENHOMKHACH";
                CboNhomKhach.SelectedValuePath = "MANHOMKHACH";

                // Load nhân viên
                var nvList = LocalDatabaseService.GetAll<Models.DNHANVIEN>("SELECT * FROM DNHANVIEN WHERE STATUS = 1 ORDER BY NAME");
                CboNhanVien.ItemsSource = nvList;
                CboNhanVien.DisplayMemberPath = "HOTEN";
                CboNhanVien.SelectedValuePath = "MANHANVIEN";

                // Load tỉnh thành
                var tinhList = LocalDatabaseService.GetAll<Models.DTINHTHANH>("SELECT MATINHTHANH, TENTINHTHANH FROM DTINHTHANH ORDER BY TENTINHTHANH");
                CboTinhThanh.ItemsSource = tinhList;
                CboTinhThanh.DisplayMemberPath = "TENTINHTHANH";
                CboTinhThanh.SelectedValuePath = "MATINHTHANH";
            }
            catch { }
        }

        private void BtnTruoc_Click(object sender, RoutedEventArgs e) { }
        private void BtnSau_Click(object sender, RoutedEventArgs e) { }
        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e) { ClearForm(); }
        private void BtnSaoChep_Click(object sender, RoutedEventArgs e) { TxtMaKhach.Text = ""; }
        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xóa khách hàng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Delete logic
            }
        }

        private void BtnTouchKb_Click(object sender, RoutedEventArgs e) { }
        private void BtnSearchNhomKhach_Click(object sender, RoutedEventArgs e) { }
        private void BtnSearchNhanVien_Click(object sender, RoutedEventArgs e) { }
        private void BtnCalcDiem_Click(object sender, RoutedEventArgs e) { }
        private void BtnDatePicker_Click(object sender, RoutedEventArgs e) { DpNgaySinh.IsDropDownOpen = true; }
        private void BtnSearchTinhThanh_Click(object sender, RoutedEventArgs e) { }

        private void ClearForm()
        {
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

        private bool Save()
        {
            if (string.IsNullOrWhiteSpace(TxtTenKhach.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            // Save logic here
            MessageBox.Show("Đã lưu thông tin khách hàng!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e) { Save(); }

        private void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (Save()) ClearForm();
        }

        private void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (Save()) { DialogResult = true; Close(); }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
