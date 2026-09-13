using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public class KhachHangItemVM : System.ComponentModel.INotifyPropertyChanged
    {
        public Models.DKHACHHANG Data { get; set; } = new();
        public string Id => Data.Id ?? "";
        public string Makhach => Data.Makhach ?? "";
        public string Name => Data.Name ?? "";
        public string Dienthoai => Data.Dienthoai ?? "";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(BorderColor));
                OnPropertyChanged(nameof(CardBackground));
            }
        }

        public string BorderColor => IsSelected ? "#FFEB3B" : "#1976D2";
        public string CardBackground => IsSelected ? "#004D40" : "#0F3460";

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
    }

    public partial class ChonKhachHangTouchWindow : Window
    {
        public Models.DKHACHHANG? SelectedKhachHang { get; private set; }
        private List<KhachHangItemVM> _allKhachHangVM = new();

        public ChonKhachHangTouchWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadKhachHang();
        }

        private async void LoadKhachHang(string? filter = null)
        {
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                string sql = "SELECT ID, MAKHACH, NAME, DIACHI, DIENTHOAI, EMAIL FROM DKHACHHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                var rows = (await conn.QueryAsync(sql)).ToList();

                _allKhachHangVM = new List<KhachHangItemVM>();
                foreach (var r in rows)
                {
                    var model = new Models.DKHACHHANG
                    {
                        Id = r.ID?.ToString(),
                        Makhach = r.MAKHACH?.ToString() ?? "",
                        Name = r.NAME?.ToString() ?? "",
                        Diachi = r.DIACHI?.ToString() ?? "",
                        Dienthoai = r.DIENTHOAI?.ToString() ?? "",
                        Email = r.EMAIL?.ToString() ?? ""
                    };
                    _allKhachHangVM.Add(new KhachHangItemVM { Data = model });
                }

                FilterCustomers(filter ?? TxtSearchQuery.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải khách hàng: {ex.Message}");
            }
        }

        private void FilterCustomers(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                IcCustomers.ItemsSource = _allKhachHangVM;
                return;
            }

            string f = RemoveAccents(filter.Trim().ToLower());
            var filtered = _allKhachHangVM.Where(k =>
                (k.Name != null && RemoveAccents(k.Name.ToLower()).Contains(f)) ||
                (k.Dienthoai != null && k.Dienthoai.ToLower().Contains(f)) ||
                (k.Makhach != null && k.Makhach.ToLower().Contains(f))
            ).ToList();

            IcCustomers.ItemsSource = filtered;
        }

        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text ?? "";
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }

        private void TxtSearchQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCustomers(TxtSearchQuery.Text);
        }

        // ===== TOUCH KEYBOARD HANDLERS =====
        private void Key_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string keyText)
            {
                TxtSearchQuery.Text += keyText;
            }
        }

        private void BtnBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtSearchQuery.Text))
            {
                TxtSearchQuery.Text = TxtSearchQuery.Text.Substring(0, TxtSearchQuery.Text.Length - 1);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchQuery.Text = "";
        }

        private void BtnSpace_Click(object sender, RoutedEventArgs e)
        {
            TxtSearchQuery.Text += " ";
        }

        private void SelectCustomerItem(KhachHangItemVM item)
        {
            foreach (var vm in _allKhachHangVM)
            {
                vm.IsSelected = false;
            }
            item.IsSelected = true;
            SelectedKhachHang = item.Data;
        }

        private void CustomerCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is KhachHangItemVM item)
            {
                SelectCustomerItem(item);

                if (e.ClickCount >= 2)
                {
                    e.Handled = true;
                    ConfirmSelection();
                }
            }
        }

        private void BtnChon_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (SelectedKhachHang == null)
            {
                MessageBox.Show("Vui lòng chọn một khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { DialogResult = true; } catch { }
            }));
        }

        private void BtnConfigSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("KhachHang");
                win.Owner = this;
                win.ShowDialog();
            }
            catch { }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ThemKhachHangTouchWindow();
                win.Owner = this;
                if (win.ShowDialog() == true)
                    LoadKhachHang();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở cửa sổ thêm khách hàng: {ex.Message}");
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedKhachHang == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var win = new ThemKhachHangTouchWindow(SelectedKhachHang);
                win.Owner = this;
                if (win.ShowDialog() == true)
                    LoadKhachHang();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chỉnh sửa khách hàng: {ex.Message}");
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { DialogResult = false; } catch { }
            }));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { DialogResult = false; } catch { }
                }));
            }
        }
    }
}
