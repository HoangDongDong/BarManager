using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
    public partial class GiamGiaTheoNhomWindow : Window
    {
        public decimal DoAnPercent { get; private set; } = 0;
        public decimal DoUongPercent { get; private set; } = 0;
        public decimal DichVuPercent { get; private set; } = 0;
        public decimal DoKhacPercent { get; private set; } = 0;

        public GiamGiaTheoNhomWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtDoAn.Focus();
            TxtDoAn.SelectAll();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            decimal.TryParse(TxtDoAn.Text?.Trim(), out decimal doAn);
            decimal.TryParse(TxtDoUong.Text?.Trim(), out decimal doUong);
            decimal.TryParse(TxtDichVu.Text?.Trim(), out decimal dichVu);
            decimal.TryParse(TxtDoKhac.Text?.Trim(), out decimal doKhac);

            DoAnPercent = Math.Max(0, Math.Min(100, doAn));
            DoUongPercent = Math.Max(0, Math.Min(100, doUong));
            DichVuPercent = Math.Max(0, Math.Min(100, dichVu));
            DoKhacPercent = Math.Max(0, Math.Min(100, doKhac));

            string msg = $"Bạn có muốn đặt giảm giá đồ ăn: {DoAnPercent:0.#}%, đồ uống: {DoUongPercent:0.#}%, dịch vụ: {DichVuPercent:0.#}%, đồ khác: {DoKhacPercent:0.#}% không?";
            var confirm = MessageBox.Show(msg, "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
