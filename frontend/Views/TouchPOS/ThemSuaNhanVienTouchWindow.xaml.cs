using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class ThemSuaNhanVienTouchWindow : Window
    {
        public event Action? OnSaved;
        private string? _id;
        private List<NhanVienTreeItem> _allList = new();
        private int _currentIndex = -1;

        public ThemSuaNhanVienTouchWindow(string? id = null)
        {
            InitializeComponent();
            _id = id;

            Loaded += async (s, e) =>
            {
                await LoadAllNhanVienAsync();

                if (!string.IsNullOrEmpty(_id))
                {
                    TxtHeaderTitle.Text = "👩 NHÂN VIÊN - SỬA";
                    await LoadDetailAsync(_id);
                }
                else
                {
                    TxtHeaderTitle.Text = "👩 NHÂN VIÊN - THÊM MỚI";
                    ClearForm();
                }
            };
        }

        private async Task LoadAllNhanVienAsync()
        {
            _allList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
            if (!string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allList.FindIndex(x => x.Id == _id);
            }
        }

        private async Task LoadDetailAsync(string id)
        {
            var item = await LocalNhanVienService.GetByIdAsync(id);
            if (item != null)
            {
                _id = item.Id;
                TxtName.Text = item.Name ?? "";
                TxtDiaChi.Text = item.Diachi ?? "";
                TxtDienThoai.Text = item.Dienthoai ?? "";
                TxtNote.Text = item.Note ?? "";

                if (item.CachTinhLuong == 1)
                {
                    RbLuongThangTheoCa.IsChecked = true;
                }
                else if (item.CachTinhLuong == 2)
                {
                    RbLuongThangTheoNgay.IsChecked = true;
                }
                else
                {
                    RbLuongTheoCa.IsChecked = true;
                }

                TxtLuongCa.Text = item.LuongCa > 0 ? item.LuongCa.ToString("N0") : "60,000";
                TxtLuongThang.Text = item.LuongThang > 0 ? item.LuongThang.ToString("N0") : "0";

                ChkNghiThu7.IsChecked = item.NghiThu7 == 1;
                ChkNghiChuNhat.IsChecked = item.NghiChuNhat == 1;
            }
        }

        private void ClearForm()
        {
            _id = null;
            TxtName.Text = "";
            TxtDiaChi.Text = "";
            TxtDienThoai.Text = "";
            TxtNote.Text = "";
            TxtLuongCa.Text = "60,000";
            TxtLuongThang.Text = "0";
            RbLuongTheoCa.IsChecked = true;
            ChkNghiThu7.IsChecked = true;
            ChkNghiChuNhat.IsChecked = false;
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_allList.Count == 0) return;

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _allList.Count - 1;
            }

            await LoadDetailAsync(_allList[_currentIndex].Id);
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_allList.Count == 0) return;

            if (_currentIndex >= 0 && _currentIndex < _allList.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            await LoadDetailAsync(_allList[_currentIndex].Id);
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnTouchKb_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Parent is Grid grid)
            {
                var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
                if (tb != null)
                {
                    var kb = new TouchKeyboardWindow(tb.Text, false, "MỜI NHẬP THÔNG TIN");
                    kb.Owner = this;
                    if (kb.ShowDialog() == true)
                    {
                        tb.Text = kb.ResultText;
                    }
                }
            }
        }

        private void BtnTouchNum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Parent is Grid grid)
            {
                var tb = grid.Children.OfType<TextBox>().FirstOrDefault();
                if (tb != null)
                {
                    var kb = new TouchKeyboardWindow(tb.Text, false, "NHẬP SỐ LƯƠNG");
                    kb.Owner = this;
                    if (kb.ShowDialog() == true)
                    {
                        tb.Text = kb.ResultText;
                    }
                }
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string clean = text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(clean, out decimal val)) return val;
            return 0;
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            int cachTinhLuong = 0;
            if (RbLuongThangTheoCa.IsChecked == true) cachTinhLuong = 1;
            else if (RbLuongThangTheoNgay.IsChecked == true) cachTinhLuong = 2;

            decimal luongCa = ParseDecimal(TxtLuongCa.Text);
            decimal luongThang = ParseDecimal(TxtLuongThang.Text);

            int nghiThu7 = (ChkNghiThu7.IsChecked == true) ? 1 : 0;
            int nghiChuNhat = (ChkNghiChuNhat.IsChecked == true) ? 1 : 0;

            var item = new NhanVienTreeItem
            {
                Id = _id,
                Name = name,
                Diachi = TxtDiaChi.Text.Trim(),
                Dienthoai = TxtDienThoai.Text.Trim(),
                Note = TxtNote.Text.Trim(),
                ParentId = "",
                ItemType = "0",
                CachTinhLuong = cachTinhLuong,
                LuongCa = luongCa,
                LuongThang = luongThang,
                NghiThu7 = nghiThu7,
                NghiChuNhat = nghiChuNhat
            };

            bool ok = await LocalNhanVienService.SaveNhanVienAsync(item);
            if (ok)
            {
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lưu thông tin nhân viên không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllNhanVienAsync();
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllNhanVienAsync();
                ClearForm();
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
            DialogResult = false;
            Close();
        }
    }
}
