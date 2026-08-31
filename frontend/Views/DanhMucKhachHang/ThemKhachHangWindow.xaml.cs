using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemKhachHangWindow : Window
    {
        private KhachHangViewModel _model;
        private bool _isNew = true;

        public ThemKhachHangWindow(KhachHangViewModel model = null, IEnumerable<NhomKhachHangTreeItem> nhomList = null)
        {
            InitializeComponent();

            if (nhomList != null)
            {
                var flatGroups = new List<NhomKhachHangTreeItem>();
                flatGroups.Add(new NhomKhachHangTreeItem { Id = null, Name = "-- Không chọn --" });
                flatGroups.AddRange(nhomList.Where(x => x.ItemType == 2));
                CboNhomKhach.ItemsSource = flatGroups;
            }

            if (model != null)
            {
                _model = model;
                _isNew = false;
                TxtHeaderTitle.Text = "Chỉnh sửa thông tin khách hàng";
                TxtMaKhach.Text = model.Makhach;
                TxtTenKhach.Text = model.Name;
                CboNhomKhach.SelectedValue = model.DnhomkhachhangId;
                TxtDienThoai.Text = model.Dienthoai;
                TxtEmail.Text = model.Email;
                TxtDiaChi.Text = model.Diachi;
                TxtGhiChu.Text = model.Note;
            }
            else
            {
                _model = new KhachHangViewModel();
                _isNew = true;
                TxtHeaderTitle.Text = "Thêm mới khách hàng";
                TxtMaKhach.Text = DateTime.Now.ToString("yyMMddHHmm");
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string ma = TxtMaKhach.Text.Trim();
            string ten = TxtTenKhach.Text.Trim();

            if (string.IsNullOrEmpty(ma) || string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã khách và Tên khách!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _model.Makhach = ma;
            _model.Name = ten;
            _model.DnhomkhachhangId = CboNhomKhach.SelectedValue?.ToString();
            _model.Dienthoai = TxtDienThoai.Text.Trim();
            _model.Email = TxtEmail.Text.Trim();
            _model.Diachi = TxtDiaChi.Text.Trim();
            _model.Note = TxtGhiChu.Text.Trim();

            bool ok = await LocalKhachHangService.SaveKhachHangAsync(_model, _isNew);
            if (ok)
            {
                MessageBox.Show("Lưu thông tin khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Không thể lưu thông tin khách hàng. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
