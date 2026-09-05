using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.ThuVienAnh
{
    public partial class ThuVienAnhWindow : Window
    {
        private List<ThuVienAnhGroupViewModel> _groups = new List<ThuVienAnhGroupViewModel>();
        private ThuVienAnhItemViewModel _selectedItem = null;

        public ThuVienAnhItemViewModel SelectedItem => _selectedItem;

        public ThuVienAnhWindow()
        {
            InitializeComponent();
            Loaded += ThuVienAnhWindow_Loaded;
        }

        private async void ThuVienAnhWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadImagesAsync();
        }

        private async Task LoadImagesAsync(string selectId = null)
        {
            try
            {
                _groups = await LocalThuVienAnhService.GetGroupedImagesAsync();
                IcGroups.ItemsSource = null;
                IcGroups.ItemsSource = _groups;

                _selectedItem = null;

                if (!string.IsNullOrEmpty(selectId))
                {
                    foreach (var grp in _groups)
                    {
                        var found = grp.Items.FirstOrDefault(x => x.Id == selectId);
                        if (found != null)
                        {
                            SelectItem(found);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thư viện ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectItem(ThuVienAnhItemViewModel item)
        {
            // Deselect all
            foreach (var grp in _groups)
            {
                foreach (var it in grp.Items)
                {
                    it.IsSelected = false;
                }
            }

            _selectedItem = item;
            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = true;
            }
        }

        private void ImageItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is ThuVienAnhItemViewModel item)
            {
                SelectItem(item);

                if (e.ClickCount == 2)
                {
                    // Double click to confirm selection
                    DialogResult = true;
                    Close();
                }
            }
        }

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Chọn hình ảnh để thêm vào thư viện",
                Filter = "Tệp hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif|Tất cả tệp (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) == true)
            {
                string category = _selectedItem?.Category ?? "Biểu tượng";
                var addWin = new ThemAnhVaoThuVienWindow(category, ofd.FileName)
                {
                    Owner = this
                };

                if (addWin.ShowDialog() == true || addWin.IsSaved)
                {
                    await LoadImagesAsync(addWin.SavedId);
                }
            }
        }

        private async void BtnThayThe_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn ảnh muốn thay thế!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ofd = new OpenFileDialog
            {
                Title = $"Chọn ảnh mới để thay thế cho ảnh '{_selectedItem.Name}'",
                Filter = "Tệp hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif|Tất cả tệp (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) == true)
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    var (ok, error) = await LocalThuVienAnhService.UpdateImageAsync(_selectedItem.Id, bytes);
                    if (ok)
                    {
                        MessageBox.Show("Đã thay thế ảnh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadImagesAsync(_selectedItem.Id);
                    }
                    else
                    {
                        MessageBox.Show("Lỗi thay thế ảnh: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn ảnh muốn xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa ảnh này khỏi thư viện?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var (ok, error) = await LocalThuVienAnhService.DeleteImageAsync(_selectedItem.Id);
                if (ok)
                {
                    MessageBox.Show("Đã xóa ảnh khỏi thư viện thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadImagesAsync();
                }
                else
                {
                    MessageBox.Show("Lỗi xóa ảnh: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
