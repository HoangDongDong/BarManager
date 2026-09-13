using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyBar.Client.Views
{
    public partial class GiamGiaTheoNhomWindow : Window
    {
        public decimal DoAnPercent { get; private set; } = 0;
        public decimal DoUongPercent { get; private set; } = 0;
        public decimal DichVuPercent { get; private set; } = 0;
        public decimal DoKhacPercent { get; private set; } = 0;

        private int _activeCategory = 0; // 0 = ĐỒ ĂN, 1 = ĐỒ UỐNG, 2 = DỊCH VỤ, 3 = ĐỒ KHÁC
        private bool _isUserTyping = false;

        public GiamGiaTheoNhomWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectCategory(0);
        }

        private void SelectCategory(int categoryIndex)
        {
            _activeCategory = categoryIndex;
            _isUserTyping = false;

            Brush highlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));
            Brush transparentBrush = Brushes.Transparent;

            BorderDoAn.BorderBrush = (_activeCategory == 0) ? highlightBrush : transparentBrush;
            BorderDoUong.BorderBrush = (_activeCategory == 1) ? highlightBrush : transparentBrush;
            BorderDichVu.BorderBrush = (_activeCategory == 2) ? highlightBrush : transparentBrush;
            BorderDoKhac.BorderBrush = (_activeCategory == 3) ? highlightBrush : transparentBrush;

            string currentValText = GetCurrentCategoryValueText();
            TxtKeypadDisplay.Text = currentValText;
            TxtKeypadDisplay.Focus();
            TxtKeypadDisplay.SelectAll();
        }

        private string GetCurrentCategoryValueText()
        {
            string raw = _activeCategory switch
            {
                0 => TxtDoAnVal.Text,
                1 => TxtDoUongVal.Text,
                2 => TxtDichVuVal.Text,
                3 => TxtDoKhacVal.Text,
                _ => "0 %"
            };
            return raw.Replace("%", "").Trim();
        }

        private void SetCurrentCategoryValueText(string valStr)
        {
            string formatted = $"{valStr} %";
            switch (_activeCategory)
            {
                case 0:
                    TxtDoAnVal.Text = formatted;
                    break;
                case 1:
                    TxtDoUongVal.Text = formatted;
                    break;
                case 2:
                    TxtDichVuVal.Text = formatted;
                    break;
                case 3:
                    TxtDoKhacVal.Text = formatted;
                    break;
            }
        }

        private void InputBox_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag != null)
            {
                if (int.TryParse(el.Tag.ToString(), out int catIndex))
                {
                    SelectCategory(catIndex);
                }
            }
        }

        private void NumpadKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string digit = btn.Tag.ToString() ?? "";
                string current = TxtKeypadDisplay.Text.Trim();

                if (!_isUserTyping || current == "0")
                {
                    if (digit == ".")
                        TxtKeypadDisplay.Text = "0.";
                    else if (digit == "000")
                        TxtKeypadDisplay.Text = "0";
                    else
                        TxtKeypadDisplay.Text = digit;
                    _isUserTyping = true;
                }
                else
                {
                    if (digit == "." && current.Contains("."))
                        return; // avoid multiple decimals

                    TxtKeypadDisplay.Text = current + digit;
                }
            }
        }

        private void BtnBackspace_Click(object sender, RoutedEventArgs e)
        {
            string current = TxtKeypadDisplay.Text.Trim();
            if (current.Length > 1)
            {
                TxtKeypadDisplay.Text = current.Substring(0, current.Length - 1);
            }
            else
            {
                TxtKeypadDisplay.Text = "0";
                _isUserTyping = false;
            }
        }

        private void BtnClearCurrent_Click(object sender, RoutedEventArgs e)
        {
            TxtKeypadDisplay.Text = "0";
            _isUserTyping = false;
            SetCurrentCategoryValueText("0");
        }

        private void BtnConfirmNumpad_Click(object sender, RoutedEventArgs e)
        {
            // Move to next category row
            int nextCat = (_activeCategory + 1) % 4;
            SelectCategory(nextCat);
        }

        private void TxtKeypadDisplay_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtKeypadDisplay == null) return;
            string txt = TxtKeypadDisplay.Text.Trim();
            if (string.IsNullOrEmpty(txt)) txt = "0";

            SetCurrentCategoryValueText(txt);
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            decimal.TryParse(TxtDoAnVal.Text.Replace("%", "").Trim(), out decimal doAn);
            decimal.TryParse(TxtDoUongVal.Text.Replace("%", "").Trim(), out decimal doUong);
            decimal.TryParse(TxtDichVuVal.Text.Replace("%", "").Trim(), out decimal dichVu);
            decimal.TryParse(TxtDoKhacVal.Text.Replace("%", "").Trim(), out decimal doKhac);

            DoAnPercent = Math.Max(0, Math.Min(100, doAn));
            DoUongPercent = Math.Max(0, Math.Min(100, doUong));
            DichVuPercent = Math.Max(0, Math.Min(100, dichVu));
            DoKhacPercent = Math.Max(0, Math.Min(100, doKhac));

            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }
    }
}
