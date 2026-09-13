using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchGhiChuNhanhWindow : Window
    {
        private bool _isShiftActive = false;

        public TouchGhiChuNhanhWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string noteText = await LocalCauHinhService.GetConfigValueAsync("GHICHU_PAGE_1", "");
                TxtNoteContent.Text = noteText ?? "";
                TxtNoteContent.Focus();
                TxtNoteContent.CaretIndex = TxtNoteContent.Text.Length;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp ghi chú: {ex.Message}");
            }
        }

        private void KeyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content != null)
            {
                string charStr = btn.Content.ToString() ?? "";
                if (_isShiftActive)
                {
                    charStr = charStr.ToUpper();
                }

                InsertTextAtCaret(charStr);

                if (_isShiftActive)
                {
                    _isShiftActive = false;
                    UpdateShiftState();
                }
            }
        }

        private void InsertTextAtCaret(string text)
        {
            int selectionStart = TxtNoteContent.SelectionStart;
            int selectionLength = TxtNoteContent.SelectionLength;

            if (selectionLength > 0)
            {
                TxtNoteContent.Text = TxtNoteContent.Text.Remove(selectionStart, selectionLength);
            }

            TxtNoteContent.Text = TxtNoteContent.Text.Insert(selectionStart, text);
            TxtNoteContent.CaretIndex = selectionStart + text.Length;
            TxtNoteContent.Focus();
        }

        private void BtnBackspace_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = TxtNoteContent.SelectionStart;
            int selectionLength = TxtNoteContent.SelectionLength;

            if (selectionLength > 0)
            {
                TxtNoteContent.Text = TxtNoteContent.Text.Remove(selectionStart, selectionLength);
                TxtNoteContent.CaretIndex = selectionStart;
            }
            else if (selectionStart > 0)
            {
                TxtNoteContent.Text = TxtNoteContent.Text.Remove(selectionStart - 1, 1);
                TxtNoteContent.CaretIndex = selectionStart - 1;
            }
            TxtNoteContent.Focus();
        }

        private void BtnSpace_Click(object sender, RoutedEventArgs e)
        {
            InsertTextAtCaret(" ");
        }

        private void BtnShift_Click(object sender, RoutedEventArgs e)
        {
            _isShiftActive = !_isShiftActive;
            UpdateShiftState();
        }

        private void UpdateShiftState()
        {
            TxtShiftIcon.Foreground = _isShiftActive ? System.Windows.Media.Brushes.OrangeRed : System.Windows.Media.Brushes.Yellow;

            // Toggle all letter keys case
            ToggleGridLetterCase(this);
        }

        private void ToggleGridLetterCase(DependencyObject parent)
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is Button btn && btn.Content is string s && s.Length == 1 && char.IsLetter(s[0]))
                {
                    btn.Content = _isShiftActive ? s.ToUpper() : s.ToLower();
                }
                else
                {
                    ToggleGridLetterCase(child);
                }
            }
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LocalCauHinhService.SaveSingleConfigAsync("GHICHU_PAGE_1", TxtNoteContent.Text ?? "");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu ghi chú: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
