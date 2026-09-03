using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.KhoHang
{
    public partial class ThungRacKhoHangWindow : Window
    {
        private ObservableCollection<KhoHangTreeItem> _trashTree = new ObservableCollection<KhoHangTreeItem>();
        private KhoHangTreeItem _selectedItem;

        public event Action OnChanged;

        public ThungRacKhoHangWindow()
        {
            InitializeComponent();
            Loaded += ThungRacKhoHangWindow_Loaded;
        }

        private async void ThungRacKhoHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshTrashTreeAsync();
            UpdateButtonsState();
        }

        private async Task RefreshTrashTreeAsync()
        {
            try
            {
                _trashTree = await LocalKhoHangService.GetKhoHangTreeAsync(showTrash: true);
                TvTrashKhoHang.ItemsSource = _trashTree;
                _selectedItem = null;
                UpdateButtonsState();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RefreshTrashTreeAsync: " + ex.Message);
            }
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = (_selectedItem != null);
            BtnKhoiPhuc.IsEnabled = hasSelection;
            BtnChiTiet.IsEnabled = hasSelection;
            BtnXoaVinhVien.IsEnabled = hasSelection;
            BtnThuocTinh.IsEnabled = hasSelection;
        }

        private void TvTrashKhoHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedItem = e.NewValue as KhoHangTreeItem;
            UpdateButtonsState();
        }

        private void TvTrashKhoHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedItem != null)
            {
                BtnChiTiet_Click(sender, e);
            }
        }

        private async void BtnKhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kho hàng/mục trong thùng rác cần khôi phục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn khôi phục kho hàng '{_selectedItem.Name}'?", "Khôi phục dữ liệu", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool ok = await LocalKhoHangService.RestoreKhoHangAsync(_selectedItem.Id);
                if (ok)
                {
                    OnChanged?.Invoke();
                    await RefreshTrashTreeAsync();
                }
                else
                {
                    MessageBox.Show("Lỗi khi khôi phục dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;

            var win = new ThemKhoHangWindow(_selectedItem);
            win.Owner = this;
            win.OnSaved += async () =>
            {
                OnChanged?.Invoke();
                await RefreshTrashTreeAsync();
            };
            win.ShowDialog();
        }

        private async void BtnXoaVinhVien_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn mục cần xóa vĩnh viễn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Hành động này sẽ XÓA VĨNH VIỄN kho hàng '{_selectedItem.Name}' khỏi cơ sở dữ liệu và không thể phục hồi!\n\nBạn có chắc chắn muốn xóa không?", "Cảnh báo xóa vĩnh viễn", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                bool ok = await LocalKhoHangService.DeleteKhoHangAsync(_selectedItem.Id, permanent: true);
                if (ok)
                {
                    OnChanged?.Invoke();
                    await RefreshTrashTreeAsync();
                }
                else
                {
                    MessageBox.Show("Lỗi khi xóa vĩnh viễn dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;

            string info = $"Tên: {_selectedItem.Name}\n" +
                          $"Mã ID: {_selectedItem.Id}\n" +
                          $"Loại: {(_selectedItem.IsWarehouse ? "Kho hàng" : (_selectedItem.IsFolder ? "Thư mục" : "Phân cách"))}\n" +
                          $"Cửa hàng: {_selectedItem.TenCuaHang}\n" +
                          $"Ghi chú: {_selectedItem.Note}";

            MessageBox.Show(info, "Thuộc tính kho hàng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}
