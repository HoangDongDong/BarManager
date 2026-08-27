using System;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemPhuongThucDatWindow : Window
    {
        private LocalKhachDatHangService _service;
        private TreeCategoryViewModel _editingItem;
        public Action OnSaveSuccess;
        public string ParentId { get; set; }
        public bool IsMucDichDat { get; set; } = false;

        public ThemPhuongThucDatWindow(TreeCategoryViewModel itemToEdit = null)
        {
            InitializeComponent();
            _service = new LocalKhachDatHangService();
            _editingItem = itemToEdit;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string typeName = IsMucDichDat ? "mục đích đặt" : "phương thức đặt";
            
            if (_editingItem != null)
            {
                TxtTitle.Text = $"Chỉnh sửa {typeName}";
                TxtName.Text = _editingItem.Name;
                TxtNote.Text = _editingItem.Note;
            }
            else
            {
                TxtTitle.Text = $"Thêm mới {typeName}";
            }

            var images = await _service.GetSImagesAsync();
            CmbImage.ItemsSource = images;

            if (_editingItem != null && _editingItem.SimageId.HasValue)
            {
                CmbImage.SelectedItem = images.FirstOrDefault(x => x.Id == _editingItem.SimageId.Value);
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string typeName = IsMucDichDat ? "mục đích đặt" : "phương thức đặt";
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show($"Vui lòng nhập tên {typeName}.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            int? selectedImageId = (CmbImage.SelectedItem as SImageViewModel)?.Id;

            bool success = false;
            if (_editingItem == null)
            {
                success = await _service.InsertPhuongThucDatAsync(TxtName.Text.Trim(), TxtNote.Text.Trim(), selectedImageId, ParentId, IsMucDichDat);
            }
            else
            {
                success = await _service.UpdatePhuongThucDatAsync(_editingItem.Id, TxtName.Text.Trim(), TxtNote.Text.Trim(), selectedImageId, IsMucDichDat);
            }

            if (success)
            {
                OnSaveSuccess?.Invoke();
                this.Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
