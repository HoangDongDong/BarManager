using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang
{
    public partial class ThemSuaTaiKhoanNganHangWindow : Window
    {
        private string _id;
        private string _itemType;
        private string _parentId;
        private List<TaiKhoanNganHangTreeItem> _allList = new List<TaiKhoanNganHangTreeItem>();
        private int _currentIndex = -1;

        public string SavedId { get; private set; }

        public ThemSuaTaiKhoanNganHangWindow(string id = null, string itemType = "", string parentId = "")
        {
            InitializeComponent();
            _id = id;
            _itemType = itemType;
            _parentId = parentId;

            Loaded += async (s, e) =>
            {
                await LoadAllListAsync();
                LoadItemData();
                TxtName.Focus();
                TxtName.SelectAll();
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
                else if (e.Key == Key.F10)
                {
                    BtnPrevious_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F11)
                {
                    BtnNext_Click(null, null);
                    e.Handled = true;
                }
            };
        }

        private async Task LoadAllListAsync()
        {
            try
            {
                _allList = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangListAsync(false);
                if (!string.IsNullOrEmpty(_id))
                {
                    _currentIndex = _allList.FindIndex(x => x.Id == _id);
                }
            }
            catch { }
        }

        private void LoadItemData()
        {
            if (!string.IsNullOrEmpty(_id) && _currentIndex >= 0 && _currentIndex < _allList.Count)
            {
                var item = _allList[_currentIndex];
                Title = "TÀI KHOẢN NGÂN HÀNG - CHỈNH SỬA";
                TxtName.Text = item.Name ?? "";
                TxtGhiChu.Text = item.Note ?? "";
            }
            else
            {
                Title = "TÀI KHOẢN NGÂN HÀNG - THÊM MỚI";
                TxtName.Text = "";
                TxtGhiChu.Text = "";
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản ngân hàng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            string note = TxtGhiChu.Text.Trim();

            var (success, error, savedId) = await LocalTaiKhoanNganHangService.SaveTaiKhoanNganHangAsync(
                _id,
                name,
                _parentId ?? "",
                note,
                _itemType ?? ""
            );

            if (success)
            {
                SavedId = savedId;
                _id = savedId;
                return true;
            }
            else
            {
                MessageBox.Show("Lưu dữ liệu không thành công: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                await LoadAllListAsync();
                BtnTaoMoi_Click(null, null);
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            _currentIndex = -1;
            LoadItemData();
            TxtName.Focus();
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null && _allList.Count > 0)
            {
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                }
                else
                {
                    _currentIndex = _allList.Count - 1;
                }
                _id = _allList[_currentIndex].Id;
                _parentId = _allList[_currentIndex].ParentId;
                _itemType = _allList[_currentIndex].ItemType;
                LoadItemData();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_allList != null && _allList.Count > 0)
            {
                if (_currentIndex < _allList.Count - 1)
                {
                    _currentIndex++;
                }
                else
                {
                    _currentIndex = 0;
                }
                _id = _allList[_currentIndex].Id;
                _parentId = _allList[_currentIndex].ParentId;
                _itemType = _allList[_currentIndex].ItemType;
                LoadItemData();
            }
        }
    }
}
