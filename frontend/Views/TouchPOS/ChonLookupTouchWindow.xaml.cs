using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public class LookupItemVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string SubTitle { get; set; } = "";
    }

    public partial class ChonLookupTouchWindow : Window
    {
        public LookupItemVM? SelectedItem { get; private set; }
        private List<LookupItemVM> _allItems = new();
        private Func<Task<List<LookupItemVM>>>? _refreshFunc;

        public ChonLookupTouchWindow(string title, IEnumerable<LookupItemVM> items, Func<Task<List<LookupItemVM>>>? refreshFunc = null)
        {
            InitializeComponent();
            TxtTitle.Text = title.ToUpper();
            _allItems = items?.ToList() ?? new List<LookupItemVM>();
            _refreshFunc = refreshFunc;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FilterItems("");
        }

        private void FilterItems(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                IcItems.ItemsSource = _allItems;
                return;
            }

            string f = RemoveAccents(filter.Trim().ToLower());
            var filtered = _allItems.Where(x =>
                (x.Name != null && RemoveAccents(x.Name.ToLower()).Contains(f)) ||
                (x.SubTitle != null && RemoveAccents(x.SubTitle.ToLower()).Contains(f))
            ).ToList();

            IcItems.ItemsSource = filtered;
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
            FilterItems(TxtSearchQuery.Text);
        }

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

        private void ItemTile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is LookupItemVM item)
            {
                e.Handled = true;
                SelectedItem = item;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { DialogResult = true; } catch { }
                }));
            }
        }

        private async void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTitle.Text;
            if (title.Contains("NHÂN VIÊN"))
            {
                var dlg = new ThemSuaNhanVienTouchWindow();
                dlg.Owner = this;
                dlg.OnSaved += async () => await ReloadDataAsync();
                dlg.ShowDialog();
            }
            else if (title.Contains("NHÓM KHÁCH"))
            {
                var dlg = new ThemNhomKhachHangWindow();
                dlg.Owner = this;
                dlg.OnSaved += async () => await ReloadDataAsync();
                dlg.ShowDialog();
            }
            else if (title.Contains("TỈNH"))
            {
                var dlg = new ThemTinhThanhWindow();
                dlg.Owner = this;
                dlg.OnSaved += async () => await ReloadDataAsync();
                dlg.ShowDialog();
            }
        }

        private async void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null || string.IsNullOrEmpty(SelectedItem.Id))
            {
                MessageBox.Show("Vui lòng chọn một mục để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string title = TxtTitle.Text;
            if (title.Contains("NHÂN VIÊN"))
            {
                var dlg = new ThemSuaNhanVienTouchWindow(SelectedItem.Id);
                dlg.Owner = this;
                dlg.OnSaved += async () => await ReloadDataAsync();
                dlg.ShowDialog();
            }
            else if (title.Contains("NHÓM KHÁCH"))
            {
                var dlg = new ThemNhomKhachHangWindow(SelectedItem.Id);
                dlg.Owner = this;
                dlg.OnSaved += async () => await ReloadDataAsync();
                dlg.ShowDialog();
            }
            else if (title.Contains("TỈNH"))
            {
                var dlg = new ThemTinhThanhWindow(SelectedItem.Id);
                dlg.Owner = this;
                dlg.OnSaved += async () => await ReloadDataAsync();
                dlg.ShowDialog();
            }
        }

        private async Task ReloadDataAsync()
        {
            if (_refreshFunc != null)
            {
                _allItems = await _refreshFunc();
                FilterItems(TxtSearchQuery.Text);
            }
        }

        private void BtnXoaTrong_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = new LookupItemVM { Id = "", Name = "" };
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { DialogResult = true; } catch { }
            }));
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
