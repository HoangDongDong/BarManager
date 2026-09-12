using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchKeyboardWindow : Window
    {
        public string ResultText { get; private set; } = "";
        private bool _isPasswordMode;
        private bool _isCapsLock;

        public TouchKeyboardWindow(string initialText = "", bool isPasswordMode = false)
        {
            InitializeComponent();
            _isPasswordMode = isPasswordMode;
            ResultText = initialText ?? "";

            if (_isPasswordMode)
            {
                TxtResult.Visibility = Visibility.Collapsed;
                TxtPasswordResult.Visibility = Visibility.Visible;
                TxtPasswordResult.Password = ResultText;
            }
            else
            {
                TxtResult.Visibility = Visibility.Visible;
                TxtPasswordResult.Visibility = Visibility.Collapsed;
                TxtResult.Text = ResultText;
            }

            Loaded += (s, e) =>
            {
                if (_isPasswordMode) TxtPasswordResult.Focus();
                else
                {
                    TxtResult.Focus();
                    TxtResult.CaretIndex = TxtResult.Text.Length;
                }
            };
        }

        private string GetCurrentText()
        {
            return _isPasswordMode ? TxtPasswordResult.Password : TxtResult.Text;
        }

        private void SetCurrentText(string text)
        {
            ResultText = text;
            if (_isPasswordMode)
            {
                TxtPasswordResult.Password = text;
            }
            else
            {
                TxtResult.Text = text;
                TxtResult.CaretIndex = text.Length;
            }
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string charStr)
            {
                string text = GetCurrentText();
                text += charStr;
                SetCurrentText(text);
            }
        }

        private void BtnBackspace_Click(object sender, RoutedEventArgs e)
        {
            string text = GetCurrentText();
            if (text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
                SetCurrentText(text);
            }
        }

        private void BtnSpace_Click(object sender, RoutedEventArgs e)
        {
            string text = GetCurrentText();
            text += " ";
            SetCurrentText(text);
        }

        private void BtnShift_Click(object sender, RoutedEventArgs e)
        {
            _isCapsLock = !_isCapsLock;
            ToggleCase(_isCapsLock);
        }

        private void ToggleCase(bool upper)
        {
            Button[] letterButtons = new Button[]
            {
                KeyQ, KeyW, KeyE, KeyR, KeyT, KeyY, KeyU, KeyI, KeyO, KeyP,
                KeyA, KeyS, KeyD, KeyF, KeyG, KeyH, KeyJ, KeyK, KeyL,
                KeyZ, KeyX, KeyC, KeyV, KeyB, KeyN, KeyM
            };

            foreach (var btn in letterButtons)
            {
                if (btn != null && btn.Content is string s)
                {
                    btn.Content = upper ? s.ToUpper() : s.ToLower();
                }
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            ResultText = GetCurrentText();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
