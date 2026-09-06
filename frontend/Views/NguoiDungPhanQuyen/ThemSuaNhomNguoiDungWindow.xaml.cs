using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NguoiDungPhanQuyen
{
    public partial class ThemSuaNhomNguoiDungWindow : Window
    {
        public GroupUserItem GroupItem { get; private set; }
        private bool _isEdit = false;

        public ThemSuaNhomNguoiDungWindow(GroupUserItem item = null)
        {
            InitializeComponent();
            GroupItem = item;

            if (GroupItem != null && !string.IsNullOrEmpty(GroupItem.Id))
            {
                _isEdit = true;
                TxtHeaderTitle.Text = "Chỉnh sửa nhóm người dùng";
                TxtGroupName.Text = GroupItem.Name;
                TxtNote.Text = GroupItem.Note;
                PickerAnh.SelectedSimageId = GroupItem.SimageId;
            }
            else
            {
                _isEdit = false;
                TxtHeaderTitle.Text = "Thêm mới nhóm người dùng";
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtGroupName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm người dùng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtGroupName.Focus();
                return;
            }

            try
            {
                if (GroupItem == null)
                {
                    GroupItem = new GroupUserItem();
                }

                GroupItem.Name = TxtGroupName.Text.Trim();
                GroupItem.Note = TxtNote.Text?.Trim() ?? "";
                GroupItem.SimageId = PickerAnh.SelectedSimageId;

                bool success = await LocalPhanQuyenService.SaveGroupUserAsync(GroupItem);
                if (success)
                {
                    MessageBox.Show(_isEdit ? "Cập nhật nhóm thành công!" : "Thêm nhóm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Lưu nhóm không thành công. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu nhóm người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
