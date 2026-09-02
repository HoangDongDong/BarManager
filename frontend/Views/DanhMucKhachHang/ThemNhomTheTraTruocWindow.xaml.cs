using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhomTheTraTruocWindow : Window
    {
        private string _id;
        private string _name;
        private string _parentId;
        private bool _isNew;

        public event Action OnSaved;

        public ThemNhomTheTraTruocWindow(string id = null, string name = "", string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _name = name;
            _parentId = parentId;
            _isNew = string.IsNullOrEmpty(id);

            Title = _isNew ? "NHÓM THẺ TRẢ TRƯỚC - THÊM MỚI" : "NHÓM THẺ TRẢ TRƯỚC - CHỈNH SỬA";
            Loaded += ThemNhomTheTraTruocWindow_Loaded;
        }

        private async void ThemNhomTheTraTruocWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTenNhom.Text = _name ?? "";

            try
            {
                var tree = await LocalTheTraTruocService.GetNhomTheTraTruocTreeAsync();
                var list = new List<NhomTheTraTruocTreeItem>
                {
                    new NhomTheTraTruocTreeItem { Id = "ALL", Name = "Tất cả (Gốc)" }
                };

                void Flatten(IEnumerable<NhomTheTraTruocTreeItem> items)
                {
                    foreach (var it in items)
                    {
                        if (it.Id != "TRASH" && it.Id != "UNSET" && it.Id != "ALL" && it.Id != _id)
                        {
                            list.Add(it);
                        }
                        if (it.Children != null && it.Children.Count > 0)
                        {
                            Flatten(it.Children);
                        }
                    }
                }

                Flatten(tree);

                CboNhomCha.ItemsSource = list;
                CboNhomCha.DisplayMemberPath = "Name";
                CboNhomCha.SelectedValuePath = "Id";

                if (!string.IsNullOrEmpty(_parentId) && list.Any(x => x.Id == _parentId))
                {
                    CboNhomCha.SelectedValue = _parentId;
                }
                else
                {
                    CboNhomCha.SelectedIndex = 0;
                }
            }
            catch { }

            TxtTenNhom.Focus();
            TxtTenNhom.SelectAll();
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtTenNhom.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenNhom.Focus();
                return;
            }

            string pId = CboNhomCha.SelectedValue?.ToString();

            bool ok = await LocalTheTraTruocService.SaveNhomTheTraTruocAsync(_id, name, pId);
            if (ok)
            {
                OnSaved?.Invoke();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Không thể lưu nhóm thẻ trả trước. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
