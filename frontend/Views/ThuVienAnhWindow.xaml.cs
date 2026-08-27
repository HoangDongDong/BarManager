using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using System.Linq;

namespace QuanLyBar.Client.Views
{
    public partial class ThuVienAnhWindow : Window
    {
        private LocalBanKhuVucService _service;
        public BieuTuongViewModel SelectedIcon { get; private set; }

        public ThuVienAnhWindow()
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
            LoadData();
        }

        private async void LoadData()
        {
            var data = await _service.GetBieuTuongTreeAsync();
            IcDanhGia.ItemsSource = data;
            
            // Clear selection
            SelectedIcon = null;
            BtnChon.IsEnabled = false;
            BtnThayThe.IsEnabled = false;
            BtnXoa.IsEnabled = false;
        }

        private void LbIcons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                SelectedIcon = listBox.SelectedItem as BieuTuongViewModel;
                BtnChon.IsEnabled = true;
                BtnThayThe.IsEnabled = true;
                BtnXoa.IsEnabled = true;

                // Deselect other listboxes
                foreach (var item in Utilities.FindVisualChildren<ListBox>(this))
                {
                    if (item != listBox)
                    {
                        item.SelectedItem = null;
                    }
                }
            }
        }

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    byte[] anh = File.ReadAllBytes(ofd.FileName);
                    
                    // Lấy nhóm đầu tiên hoặc tạo nhóm mặc định nếu chưa có
                    var tree = IcDanhGia.ItemsSource as System.Collections.ObjectModel.ObservableCollection<BieuTuongViewModel>;
                    int? parentId = null;
                    if (tree != null && tree.Count > 0)
                    {
                        parentId = tree[0].Id;
                    }
                    else
                    {
                        // Tạo thư mục gốc "Chung"
                        var newDir = new DBIEUTUONG { Name = "Chung" };
                        await _service.InsertBieuTuongAsync(newDir);
                        
                        // Lấy lại danh sách để có ID
                        tree = await _service.GetBieuTuongTreeAsync();
                        if (tree.Count > 0) parentId = tree[0].Id;
                    }

                    var newIcon = new DBIEUTUONG
                    {
                        Name = Path.GetFileNameWithoutExtension(ofd.FileName),
                        ParentId = parentId,
                        Anh = anh
                    };

                    if (await _service.InsertBieuTuongAsync(newIcon))
                    {
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThayThe_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thay thế đang được cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIcon == null) return;
            
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa biểu tượng '{SelectedIcon.Name}' không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (await _service.DeleteBieuTuongAsync(SelectedIcon.Id))
                {
                    LoadData();
                }
            }
        }

        private void BtnChon_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIcon != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
    
    // Utilities helper class to find visual children
    public static class Utilities
    {
        public static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject depObj) where T : System.Windows.DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    System.Windows.DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
