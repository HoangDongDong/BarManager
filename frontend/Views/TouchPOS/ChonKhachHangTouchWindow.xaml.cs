using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class ChonKhachHangTouchWindow : Window
    {
        public Models.DKHACHHANG? SelectedKhachHang { get; private set; }
        private List<Models.DKHACHHANG> _allKhachHang = new();
        private string _searchText = "";
        private bool _isCaps = true;

        public ChonKhachHangTouchWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadKhachHang();
        }

        private void LoadKhachHang(string? filter = null)
        {
            try
            {
                string sql = "SELECT * FROM DKHACHHANG WHERE STATUS = 1";
                if (!string.IsNullOrEmpty(filter))
                    sql += $" AND (NAME LIKE '%{filter}%' OR DIENTHOAI LIKE '%{filter}%')";
                sql += " ORDER BY NAME";

                _allKhachHang = LocalDatabaseService.GetAll<Models.DKHACHHANG>(sql).ToList();
                IcCustomers.ItemsSource = _allKhachHang;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải khách hàng: {ex.Message}");
            }
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string key = btn.Content?.ToString() ?? "";
                _searchText += _isCaps ? key.ToUpper() : key.ToLower();
                TxtSearchDisplay.Text = _searchText;
                LoadKhachHang(_searchText);
            }
        }

        private void KeyBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (_searchText.Length > 0)
            {
                _searchText = _searchText[..^1];
                TxtSearchDisplay.Text = _searchText.Length == 0 ? "Nhập tên khách hàng..." : _searchText;
                LoadKhachHang(_searchText);
            }
        }

        private void KeyCaps_Click(object sender, RoutedEventArgs e)
        {
            _isCaps = !_isCaps;
            BtnCaps.Background = _isCaps
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(25, 118, 210))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(55, 71, 79));
        }

        private void KeySpace_Click(object sender, RoutedEventArgs e)
        {
            _searchText += " ";
            TxtSearchDisplay.Text = _searchText;
            LoadKhachHang(_searchText);
        }

        private void CustomerCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.DataContext is Models.DKHACHHANG kh)
            {
                SelectedKhachHang = kh;
                DialogResult = true;
                Close();
            }
        }

        private void BtnConfigTiles_Click(object sender, RoutedEventArgs e) { }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ThemKhachHangTouchWindow();
                win.Owner = this;
                if (win.ShowDialog() == true)
                    LoadKhachHang(_searchText);
            }
            catch { }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e) { }

        private void BtnXoaTrong_Click(object sender, RoutedEventArgs e)
        {
            _searchText = "";
            TxtSearchDisplay.Text = "Nhập tên khách hàng...";
            LoadKhachHang();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
