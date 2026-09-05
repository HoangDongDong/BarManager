using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NhanSu
{
    public partial class ThemSuaLyDoThuongPhatWindow : Window
    {
        private string _id;
        private string _parentId;
        private string _itemType;

        public event Action OnSaved;
        public bool IsSaved { get; private set; } = false;

        public ThemSuaLyDoThuongPhatWindow(string id = null, string name = null, string parentId = null, string itemType = "0")
        {
            InitializeComponent();
            _id = id;
            _parentId = parentId;
            _itemType = itemType;

            if (string.IsNullOrEmpty(_id))
            {
                TxtHeaderTitle.Text = "Thêm lý do thưởng phạt";
                Title = "Lý do thưởng phạt";
                if (!string.IsNullOrEmpty(name)) TxtName.Text = name;
            }
            else
            {
                TxtHeaderTitle.Text = "Chỉnh sửa lý do thưởng phạt";
                Title = "Lý do thưởng phạt";
                if (!string.IsNullOrEmpty(name)) TxtName.Text = name;
            }

            Loaded += async (s, e) =>
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    await LoadDetailAsync(_id);
                }
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
                else if (e.Key == Key.F2 || (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && !TxtGhiChu.IsFocused))
                {
                    BtnGhiDuLieu_Click(null, null);
                    e.Handled = true;
                }
            };
        }

        private async Task LoadDetailAsync(string id)
        {
            try
            {
                var list = await LocalThuongPhatService.GetLyDoThuongPhatFlatListAsync();
                var item = list.Find(x => x.Id == id);
                if (item != null)
                {
                    TxtName.Text = item.Name ?? "";
                    TxtGhiChu.Text = item.Note ?? "";
                    _parentId = item.ParentId ?? "";
                    _itemType = item.ItemType ?? "0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadDetailAsync: " + ex.Message);
            }
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên lý do thưởng phạt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            var item = new LyDoThuongPhatTreeItem
            {
                Id = _id,
                Name = name,
                Note = TxtGhiChu.Text.Trim(),
                ParentId = _parentId,
                ItemType = _itemType,
                Status = 1
            };

            var (ok, error) = await LocalThuongPhatService.SaveLyDoThuongPhatAsync(item);
            if (ok)
            {
                IsSaved = true;
                OnSaved?.Invoke();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Lỗi lưu lý do thưởng phạt: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
