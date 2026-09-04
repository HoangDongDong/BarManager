using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucLyDoThuChi
{
    public partial class ThemSuaLyDoThuChiWindow : Window
    {
        private string _id;
        private string _itemType;
        private string _parentId;
        private List<LyDoThuChiTreeItem> _itemList = new List<LyDoThuChiTreeItem>();
        private int _currentIndex = -1;

        public bool IsSaved { get; private set; } = false;

        public ThemSuaLyDoThuChiWindow(string id = null, string itemType = "", string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _itemType = itemType;
            _parentId = parentId ?? "";

            Loaded += async (s, e) =>
            {
                await LoadItemListAsync();

                if (!string.IsNullOrEmpty(_id))
                {
                    Title = "LÝ DO THU CHI - CHỈNH SỬA";
                    _currentIndex = _itemList.FindIndex(x => x.Id == _id);
                    await LoadCurrentItemDataAsync();
                }
                else
                {
                    Title = "LÝ DO THU CHI - THÊM MỚI";
                    ResetFormForNew();
                }
                TxtName.Focus();
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
                else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    BtnLuuVaThoat_Click(null, null);
                    e.Handled = true;
                }
            };
        }

        private async Task LoadItemListAsync()
        {
            try
            {
                _itemList = await LocalLyDoThuChiService.GetLyDoThuChiListAsync(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadItemListAsync: " + ex.Message);
            }
        }

        private async Task LoadCurrentItemDataAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_id)) return;

                var item = _itemList.Find(x => x.Id == _id);
                if (item == null)
                {
                    var freshList = await LocalLyDoThuChiService.GetLyDoThuChiListAsync(false);
                    item = freshList.Find(x => x.Id == _id);
                }

                if (item != null)
                {
                    TxtName.Text = item.Name ?? "";
                    TxtGhiChu.Text = item.Note ?? "";
                    _parentId = item.ParentId ?? "";
                    _itemType = item.ItemType ?? "";

                    if (item.Lalydothu.HasValue && item.Lalydothu.Value > 0)
                    {
                        ChkLaLyDoThu.IsChecked = true;
                        ChkLaLyDoChi.IsChecked = false;
                    }
                    else
                    {
                        ChkLaLyDoThu.IsChecked = false;
                        ChkLaLyDoChi.IsChecked = true;
                    }

                    // Loại
                    SelectLoai(item.Loailydo);

                    // Icon
                    SelectIcon(item.IconText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin lý do: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectLoai(string loai)
        {
            if (string.IsNullOrEmpty(loai))
            {
                CboLoai.SelectedIndex = 0;
                return;
            }

            foreach (ComboBoxItem item in CboLoai.Items)
            {
                if (item.Tag?.ToString() == loai || item.Content?.ToString() == loai)
                {
                    CboLoai.SelectedItem = item;
                    return;
                }
            }
            CboLoai.SelectedIndex = 0;
        }

        private void SelectIcon(string icon)
        {
            if (string.IsNullOrEmpty(icon))
            {
                CboIcon.SelectedIndex = 0;
                return;
            }

            foreach (ComboBoxItem item in CboIcon.Items)
            {
                if (item.Content?.ToString()?.Contains(icon) == true)
                {
                    CboIcon.SelectedItem = item;
                    return;
                }
            }
            CboIcon.SelectedIndex = 0;
        }

        private void ResetFormForNew()
        {
            _id = null;
            TxtName.Text = "";
            TxtGhiChu.Text = "";
            ChkLaLyDoThu.IsChecked = false;
            ChkLaLyDoChi.IsChecked = true;
            CboLoai.SelectedIndex = 0;
            CboIcon.SelectedIndex = 0;
            Title = "LÝ DO THU CHI - THÊM MỚI";
            TxtName.Focus();
        }

        private void ChkLaLyDoThu_Checked(object sender, RoutedEventArgs e)
        {
            if (ChkLaLyDoChi != null)
            {
                ChkLaLyDoChi.IsChecked = false;
            }
        }

        private void ChkLaLyDoChi_Checked(object sender, RoutedEventArgs e)
        {
            if (ChkLaLyDoThu != null)
            {
                ChkLaLyDoThu.IsChecked = false;
            }
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_itemList == null || _itemList.Count == 0) return;

            if (_currentIndex > 0)
            {
                _currentIndex--;
                _id = _itemList[_currentIndex].Id;
                Title = "LÝ DO THU CHI - CHỈNH SỬA";
                await LoadCurrentItemDataAsync();
            }
            else
            {
                _currentIndex = 0;
                _id = _itemList[0].Id;
                Title = "LÝ DO THU CHI - CHỈNH SỬA";
                await LoadCurrentItemDataAsync();
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_itemList == null || _itemList.Count == 0) return;

            if (_currentIndex < _itemList.Count - 1)
            {
                _currentIndex++;
                _id = _itemList[_currentIndex].Id;
                Title = "LÝ DO THU CHI - CHỈNH SỬA";
                await LoadCurrentItemDataAsync();
            }
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ResetFormForNew();
        }

        private async Task<bool> SaveDataInternalAsync()
        {
            string name = TxtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên lý do thu chi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            decimal laLyDoThu = (ChkLaLyDoThu.IsChecked == true) ? 30 : 0;
            
            string loaiLyDo = "-1";
            if (CboLoai.SelectedItem is ComboBoxItem selLoai && selLoai.Tag != null)
            {
                loaiLyDo = selLoai.Tag.ToString();
            }

            string note = TxtGhiChu.Text.Trim();

            var (ok, errMsg, savedId) = await LocalLyDoThuChiService.SaveLyDoThuChiAsync(
                _id,
                name,
                laLyDoThu,
                loaiLyDo,
                _parentId,
                note,
                _itemType
            );

            if (ok)
            {
                IsSaved = true;
                if (!string.IsNullOrEmpty(savedId))
                {
                    _id = savedId;
                }
                // Cập nhật lại danh sách nội bộ
                await LoadItemListAsync();
                var found = _itemList.Find(x => x.Id == _id || x.Name == name);
                if (found != null)
                {
                    _id = found.Id;
                    _currentIndex = _itemList.IndexOf(found);
                }
                return true;
            }
            else
            {
                string msg = string.IsNullOrEmpty(errMsg) ? "Lưu dữ liệu không thành công!" : errMsg;
                MessageBox.Show(msg, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataInternalAsync();
            if (ok)
            {
                Title = "LÝ DO THU CHI - CHỈNH SỬA";
                MessageBox.Show("Đã lưu lý do thu chi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataInternalAsync();
            if (ok)
            {
                ResetFormForNew();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataInternalAsync();
            if (ok)
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
