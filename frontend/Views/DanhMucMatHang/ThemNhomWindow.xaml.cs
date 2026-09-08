using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhomWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private string _nhomIdToEdit;
        private bool _isThuMuc;
        private List<NhomMatHangViewModel> _nhomList;
        private int _currentIndex = -1;
        private Action _onDataSaved;

        public string TenNhom => TxtTenNhom.Text.Trim();
        public string MaSanPham => TxtMaSanPham.Text.Trim();
        public int? LoaiDoId => CboLoaiDo.SelectedValue as int?;

        public ThemNhomWindow(bool isThuMuc = false, string nhomIdToEdit = null, List<NhomMatHangViewModel> nhomList = null, int initialIndex = -1, Action onDataSaved = null, string initialName = null)
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _isThuMuc = isThuMuc;
            _nhomIdToEdit = nhomIdToEdit;
            _nhomList = nhomList;
            _currentIndex = initialIndex;
            _onDataSaved = onDataSaved;

            LoadLoaiDoData();

            if (!string.IsNullOrEmpty(_nhomIdToEdit))
            {
                this.Title = _isThuMuc ? "THƯ MỤC - SỬA" : "NHÓM MẶT HÀNG - SỬA";
                TxtHeaderTitle.Text = _isThuMuc ? "Thư mục mặt hàng" : "Nhóm mặt hàng";
                TxtHeaderIcon.Text = _isThuMuc ? "📁" : "🍺";

                if (!string.IsNullOrEmpty(initialName))
                {
                    TxtTenNhom.Text = initialName;
                }
                else if (_nhomList != null && _currentIndex >= 0 && _currentIndex < _nhomList.Count)
                {
                    TxtTenNhom.Text = _nhomList[_currentIndex].Name ?? "";
                }

                LoadGroupDataAsync(_nhomIdToEdit);
            }
            else
            {
                this.Title = _isThuMuc ? "THƯ MỤC - THÊM MỚI" : "NHÓM MẶT HÀNG - THÊM MỚI";
                TxtHeaderTitle.Text = _isThuMuc ? "Thư mục mặt hàng" : "Nhóm mặt hàng";
                TxtHeaderIcon.Text = _isThuMuc ? "📁" : "🍺";
                TxtTenNhom.Focus();
            }

            UpdateNavigationButtons();
        }

        private void LoadLoaiDoData()
        {
            CboLoaiDo.ItemsSource = new[]
            {
                new { Id = 2, Name = "🔮 Đồ ăn" },
                new { Id = 4, Name = "🔮 Đồ uống" },
                new { Id = 1, Name = "🔮 Dịch vụ" },
                new { Id = 3, Name = "🔮 Đồ khác" },
                new { Id = 5, Name = "🔮 Nguyên liệu" }
            };
            CboLoaiDo.DisplayMemberPath = "Name";
            CboLoaiDo.SelectedValuePath = "Id";
            CboLoaiDo.SelectedValue = 2; // Default Đồ ăn
        }

        private async void LoadGroupDataAsync(string nhomId)
        {
            try
            {
                var data = await _matHangService.GetNhomMatHangByIdAsync(nhomId);
                if (data != null)
                {
                    var dict = data as IDictionary<string, object>;
                    string name = null;
                    string code = null;
                    object dloaidoObj = null;
                    object simageObj = null;

                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            string key = kvp.Key?.ToUpperInvariant();
                            if (key == "NAME") name = kvp.Value?.ToString();
                            else if (key == "CODE") code = kvp.Value?.ToString();
                            else if (key == "DLOAIDOID") dloaidoObj = kvp.Value;
                            else if (key == "SIMAGEID") simageObj = kvp.Value;
                        }
                    }
                    else
                    {
                        try { name = data.NAME?.ToString(); } catch { }
                        try { code = data.CODE?.ToString(); } catch { }
                        try { dloaidoObj = data.DLOAIDOID; } catch { }
                        try { simageObj = data.SIMAGEID; } catch { }
                    }

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        TxtTenNhom.Text = name;
                    }
                    if (code != null)
                    {
                        TxtMaSanPham.Text = code;
                    }

                    if (dloaidoObj != null && int.TryParse(dloaidoObj.ToString(), out int dloaidoId) && dloaidoId > 0)
                    {
                        CboLoaiDo.SelectedValue = dloaidoId;
                    }

                    if (simageObj != null && int.TryParse(simageObj.ToString(), out int sImgId))
                    {
                        if (sImgId >= 0 && sImgId < CboAnh.Items.Count)
                            CboAnh.SelectedIndex = sImgId;
                    }
                }
                
                TxtTenNhom.Focus();
                TxtTenNhom.SelectAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadGroupDataAsync: " + ex.Message);
            }
        }

        private void TxtTenNhom_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtTenNhom.SelectAll();
        }

        private void UpdateNavigationButtons()
        {
            if (_nhomList != null && _nhomList.Count > 1 && _currentIndex >= 0)
            {
                BtnTruoc.IsEnabled = _currentIndex > 0;
                BtnSau.IsEnabled = _currentIndex < _nhomList.Count - 1;
            }
            else
            {
                BtnTruoc.IsEnabled = false;
                BtnSau.IsEnabled = false;
            }
        }

        private void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_nhomList != null && _currentIndex > 0)
            {
                _currentIndex--;
                var target = _nhomList[_currentIndex];
                _nhomIdToEdit = target.Id;
                TxtTenNhom.Text = target.Name ?? "";
                LoadGroupDataAsync(target.Id);
                UpdateNavigationButtons();
            }
        }

        private void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_nhomList != null && _currentIndex < _nhomList.Count - 1)
            {
                _currentIndex++;
                var target = _nhomList[_currentIndex];
                _nhomIdToEdit = target.Id;
                TxtTenNhom.Text = target.Name ?? "";
                LoadGroupDataAsync(target.Id);
                UpdateNavigationButtons();
            }
        }

        private void MenuTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _nhomIdToEdit = null;
            this.Title = _isThuMuc ? "THƯ MỤC - THÊM MỚI" : "NHÓM MẶT HÀNG - THÊM MỚI";
            TxtTenNhom.Text = "";
            TxtMaSanPham.Text = "";
            CboLoaiDo.SelectedValue = 2;
            TxtTenNhom.Focus();
            UpdateNavigationButtons();
        }

        private void MenuSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _nhomIdToEdit = null;
            this.Title = _isThuMuc ? "THƯ MỤC - THÊM MỚI" : "NHÓM MẶT HÀNG - THÊM MỚI";
            TxtTenNhom.Text = $"{TxtTenNhom.Text.Trim()} (Bản sao)";
            TxtTenNhom.Focus();
            TxtTenNhom.SelectAll();
        }

        private async Task<bool> SaveDataAsync()
        {
            if (string.IsNullOrWhiteSpace(TenNhom))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenNhom.Focus();
                return false;
            }

            int selectedIconIndex = CboAnh.SelectedIndex >= 0 ? CboAnh.SelectedIndex : 0;

            var model = new DNHOMMATHANG
            {
                Id = _nhomIdToEdit,
                Name = TenNhom,
                Code = MaSanPham,
                DloaidoId = LoaiDoId,
                SimageId = selectedIconIndex
            };

            bool success;
            if (!string.IsNullOrEmpty(_nhomIdToEdit))
            {
                success = await _matHangService.UpdateNhomMatHangAsync(model);
            }
            else
            {
                model.Id = Guid.NewGuid().ToString();
                success = await _matHangService.InsertNhomMatHangAsync(model);
                if (success)
                {
                    _nhomIdToEdit = model.Id;
                }
            }

            if (success)
            {
                _onDataSaved?.Invoke();
            }

            return success;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            await SaveDataAsync();
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveDataAsync();
            if (success)
            {
                MenuTaoMoi_Click(sender, e);
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveDataAsync();
            if (success)
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

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BtnThoat_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F10)
            {
                BtnTruoc_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnSau_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                MenuTaoMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                MenuSaoChep_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                await SaveDataAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuVaMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.FocusedElement is not TextBox)
            {
                BtnLuuVaThoat_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
