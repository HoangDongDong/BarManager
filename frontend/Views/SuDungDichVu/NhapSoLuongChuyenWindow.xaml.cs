using System;
using System.Windows;
using System.Windows.Input;

namespace QuanLyBar.Client.Views
{
    public partial class NhapSoLuongChuyenWindow : Window
    {
        public decimal SoLuong { get; private set; } = 1;
        private decimal _maxSoLuong = 1;

        public NhapSoLuongChuyenWindow(string tenHang, decimal currentSoLuong)
        {
            InitializeComponent();
            _maxSoLuong = currentSoLuong > 0 ? currentSoLuong : 1;
            TxtTenMatHang.Text = $"Mặt hàng: {tenHang}";
            TxtSoLuong.Text = "1";

            Loaded += (s, e) =>
            {
                TxtSoLuong.Focus();
                TxtSoLuong.SelectAll();
            };
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                BtnGhiDuLieu_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                BtnThoat_Click(this, new RoutedEventArgs());
            }
        }

        private void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            string raw = TxtSoLuong.Text?.Trim()?.Replace(",", ".");
            if (!decimal.TryParse(raw, out decimal sl) || sl <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ lớn hơn 0!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoLuong.Focus();
                return;
            }

            if (sl > _maxSoLuong)
            {
                MessageBox.Show($"Số lượng chuyển ({sl}) không được vượt quá số lượng hiện có ({_maxSoLuong})!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoLuong.Focus();
                return;
            }

            SoLuong = sl;
            DialogResult = true;
            Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
