using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Views.TouchPOS;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchNhapSoLuongWindow : Window
    {
        public decimal NewQuantity { get; private set; } = 1;
        private bool _isFirstInput = true;

        public TouchNhapSoLuongWindow(string itemName, decimal currentQty)
        {
            InitializeComponent();
            TxtItemName.Text = itemName;
            NewQuantity = currentQty > 0 ? currentQty : 1;
            TxtSoLuong.Text = NewQuantity.ToString("0.#");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtSoLuong.Focus();
            TxtSoLuong.SelectAll();
        }

        private void BtnNumpadDigit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string digit)
            {
                if (_isFirstInput || TxtSoLuong.Text == "0")
                {
                    TxtSoLuong.Text = digit;
                    _isFirstInput = false;
                }
                else
                {
                    TxtSoLuong.Text += digit;
                }
            }
        }

        private void BtnNumpadClear_Click(object sender, RoutedEventArgs e)
        {
            TxtSoLuong.Text = "0";
            _isFirstInput = true;
        }

        private void BtnNumpadBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (TxtSoLuong.Text.Length > 1)
            {
                TxtSoLuong.Text = TxtSoLuong.Text.Substring(0, TxtSoLuong.Text.Length - 1);
            }
            else
            {
                TxtSoLuong.Text = "0";
                _isFirstInput = true;
            }
        }

        private void BtnDongY_Click(object sender, RoutedEventArgs e)
        {
            ConfirmQuantity();
        }

        private void ConfirmQuantity()
        {
            if (decimal.TryParse(TxtSoLuong.Text.Trim(), out decimal qty) && qty > 0)
            {
                NewQuantity = qty;
                DialogResult = true;
                Close();
            }
            else
            {
                TouchConfirmWindow.ShowAlert(this, "SỐ LƯỢNG BẠN NHẬP PHẢI LỚN HƠN 0!", "CẢNH BÁO");
                TxtSoLuong.Focus();
                TxtSoLuong.SelectAll();
            }
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmQuantity();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
        }
    }
}
