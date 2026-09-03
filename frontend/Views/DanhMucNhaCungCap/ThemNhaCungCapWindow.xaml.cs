using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucNhaCungCap
{
    public partial class ThemNhaCungCapWindow : Window
    {
        public event Action OnSaved;

        private string _id;
        private bool _isNew = true;
        private string _defaultNhomId;

        private List<NhaCungCapItem> _allSuppliers = new List<NhaCungCapItem>();
        private int _currentIndex = -1;

        public ThemNhaCungCapWindow(NhaCungCapItem item = null, string defaultNhomId = null)
        {
            InitializeComponent();
            _defaultNhomId = defaultNhomId;

            if (item != null)
            {
                _id = item.Id;
                _isNew = false;
            }

            Loaded += ThemNhaCungCapWindow_Loaded;
            UpdateButtonsState();
        }

        private async void ThemNhaCungCapWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNhomListAsync();
            await LoadAllSuppliersListAsync();

            if (!_isNew && !string.IsNullOrEmpty(_id))
            {
                _currentIndex = _allSuppliers.FindIndex(x => x.Id == _id);
                if (_currentIndex >= 0)
                {
                    LoadSupplierDataIntoForm(_allSuppliers[_currentIndex]);
                }
                else
                {
                    await LoadSupplierByIdAsync(_id);
                }
            }
            else
            {
                await ClearFormAsync();
            }

            TxtTenNcc.Focus();
        }

        private async Task LoadAllSuppliersListAsync()
        {
            try
            {
                _allSuppliers = await LocalNhaCungCapService.GetNhaCungCapListAsync(null, "", "ALL");
                UpdateNavButtons();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadAllSuppliersListAsync: " + ex.Message);
            }
        }

        private async Task LoadNhomListAsync()
        {
            try
            {
                var list = await LocalNhaCungCapService.GetAllNhomListFlatAsync();
                list.Insert(0, new NhomNhaCungCapTreeItem { Id = null, Name = "-- Chưa thiết lập nhóm --" });
                CboNhomNcc.ItemsSource = list;
                CboNhomNcc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadNhomListAsync: " + ex.Message);
            }
        }

        private void LoadSupplierDataIntoForm(NhaCungCapItem item)
        {
            if (item == null) return;

            _id = item.Id;
            _isNew = false;
            this.Title = "NHÀ CUNG CẤP - SỬA";

            TxtMaNcc.Text = item.MaNhaCungCap ?? "";
            TxtTenNcc.Text = item.Name ?? "";
            TxtDiaChi.Text = item.DiaChi ?? "";
            TxtDienThoai.Text = item.DienThoai ?? "";
            TxtEmail.Text = item.Email ?? "";
            TxtWebsite.Text = item.Website ?? "";
            TxtGhiChu.Text = item.Note ?? "";

            if (!string.IsNullOrEmpty(item.DnhomnhacungcapId))
            {
                CboNhomNcc.SelectedValue = item.DnhomnhacungcapId;
            }
            else
            {
                CboNhomNcc.SelectedIndex = 0;
            }

            UpdateNavButtons();
            UpdateButtonsState();
        }

        private async Task LoadSupplierByIdAsync(string id)
        {
            var list = await LocalNhaCungCapService.GetNhaCungCapListAsync(null, "", "ALL");
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                LoadSupplierDataIntoForm(item);
            }
        }

        public async Task ClearFormAsync()
        {
            _id = null;
            _isNew = true;
            this.Title = "NHÀ CUNG CẤP - THÊM MỚI";

            TxtMaNcc.Text = await LocalNhaCungCapService.GetNextMaNhaCungCapAsync();
            TxtTenNcc.Text = "";
            TxtDiaChi.Text = "";
            TxtDienThoai.Text = "";
            TxtEmail.Text = "";
            TxtWebsite.Text = "";
            TxtGhiChu.Text = "";

            if (!string.IsNullOrEmpty(_defaultNhomId))
            {
                CboNhomNcc.SelectedValue = _defaultNhomId;
            }
            else
            {
                CboNhomNcc.SelectedIndex = 0;
            }

            UpdateNavButtons();
            UpdateButtonsState();
            TxtTenNcc.Focus();
        }

        private void UpdateNavButtons()
        {
            bool hasList = _allSuppliers.Count > 0;
            if (BtnTruoc != null) BtnTruoc.IsEnabled = hasList;
            if (BtnSau != null) BtnSau.IsEnabled = hasList;
        }

        private void UpdateButtonsState()
        {
            bool hasValidName = !string.IsNullOrWhiteSpace(TxtTenNcc?.Text);
            if (BtnLuu != null) BtnLuu.IsEnabled = hasValidName;
            if (BtnLuuVaMoi != null) BtnLuuVaMoi.IsEnabled = hasValidName;
            if (BtnLuuVaThoat != null) BtnLuuVaThoat.IsEnabled = hasValidName;
        }

        private void TxtMaNcc_TextChanged(object sender, TextChangedEventArgs e) => UpdateButtonsState();
        private void TxtTenNcc_TextChanged(object sender, TextChangedEventArgs e) => UpdateButtonsState();

        #region Navigation Buttons
        private void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_allSuppliers.Count == 0) return;

            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = _allSuppliers.Count - 1; // Vòng lại cuối
            }

            LoadSupplierDataIntoForm(_allSuppliers[_currentIndex]);
        }

        private void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_allSuppliers.Count == 0) return;

            if (_currentIndex < _allSuppliers.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0; // Vòng về đầu
            }

            LoadSupplierDataIntoForm(_allSuppliers[_currentIndex]);
        }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            await ClearFormAsync();
        }

        private async void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            _isNew = true;
            this.Title = "NHÀ CUNG CẤP - THÊM MỚI";
            TxtMaNcc.Text = await LocalNhaCungCapService.GetNextMaNhaCungCapAsync();
            TxtTenNcc.Focus();
            UpdateButtonsState();
        }
        #endregion

        #region Save & Actions
        private async Task<bool> SaveDataAsync()
        {
            string name = TxtTenNcc.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenNcc.Focus();
                return false;
            }

            string code = TxtMaNcc.Text.Trim();
            string diachi = TxtDiaChi.Text.Trim();
            string dt = TxtDienThoai.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string website = TxtWebsite.Text.Trim();
            string note = TxtGhiChu.Text.Trim();
            string nhomId = CboNhomNcc.SelectedValue?.ToString();

            var item = new NhaCungCapItem
            {
                Id = _id,
                MaNhaCungCap = code,
                Name = name,
                DiaChi = diachi,
                DienThoai = dt,
                Email = email,
                Website = website,
                Note = note,
                DnhomnhacungcapId = nhomId
            };

            var (success, msg, savedId) = await LocalNhaCungCapService.SaveNhaCungCapAsync(item, _isNew);
            if (success)
            {
                _id = savedId;
                _isNew = false;
                this.Title = "NHÀ CUNG CẤP - SỬA";

                await LoadAllSuppliersListAsync();
                _currentIndex = _allSuppliers.FindIndex(x => x.Id == _id);

                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show(msg, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Lưu thông tin nhà cung cấp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                await ClearFormAsync();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region ContextMenu & Phím tắt
        private void BtnPhimTat_Click(object sender, RoutedEventArgs e)
        {
            if (BtnPhimTat.ContextMenu != null)
            {
                BtnPhimTat.ContextMenu.PlacementTarget = BtnPhimTat;
                BtnPhimTat.ContextMenu.IsOpen = true;
            }
        }

        private async void MiTaoMoi_Click(object sender, RoutedEventArgs e) => await ClearFormAsync();
        private void MiSaoChep_Click(object sender, RoutedEventArgs e) => BtnSaoChep_Click(sender, e);
        private async void MiLuu_Click(object sender, RoutedEventArgs e) => await SaveDataAsync();
        private async void MiLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                await ClearFormAsync();
            }
        }
        private async void MiLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                this.DialogResult = true;
                this.Close();
            }
        }
        private void MiThoat_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F10)
            {
                e.Handled = true;
                BtnTruoc_Click(sender, e);
            }
            else if (e.Key == Key.F11)
            {
                e.Handled = true;
                BtnSau_Click(sender, e);
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                BtnTaoMoi_Click(sender, e);
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;
                BtnSaoChep_Click(sender, e);
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                BtnLuu_Click(sender, e);
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                BtnLuuVaMoi_Click(sender, e);
            }
            else if (e.Key == Key.Enter && !TxtGhiChu.IsFocused)
            {
                e.Handled = true;
                BtnLuuVaThoat_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
        #endregion
    }
}
