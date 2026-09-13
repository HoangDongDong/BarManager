using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TimKiemMatHangTouchWindow : Window
    {
        public TouchMatHangVM? SelectedItem { get; private set; }

        private List<TouchMatHangVM> _sourceList = new();
        private List<TouchMatHangVM> _filteredList = new();

        public static readonly DependencyProperty MatHangColumnsProperty =
            DependencyProperty.Register(nameof(MatHangColumns), typeof(int), typeof(TimKiemMatHangTouchWindow), new PropertyMetadata(4));

        public static readonly DependencyProperty MatHangTileHeightProperty =
            DependencyProperty.Register(nameof(MatHangTileHeight), typeof(double), typeof(TimKiemMatHangTouchWindow), new PropertyMetadata(65.0));

        public int MatHangColumns
        {
            get => (int)GetValue(MatHangColumnsProperty);
            set => SetValue(MatHangColumnsProperty, value);
        }

        public double MatHangTileHeight
        {
            get => (double)GetValue(MatHangTileHeightProperty);
            set => SetValue(MatHangTileHeightProperty, value);
        }

        private int _matHangRowsConfig = 4;

        public TimKiemMatHangTouchWindow(List<TouchMatHangVM> matHangList)
        {
            InitializeComponent();
            _sourceList = matHangList != null ? matHangList.ToList() : new List<TouchMatHangVM>();
            _filteredList = new List<TouchMatHangVM>(_sourceList);

            IcSearchMatHang.ItemsSource = _filteredList;
            _ = LoadLayoutConfigAsync();
        }

        private async System.Threading.Tasks.Task LoadLayoutConfigAsync()
        {
            try
            {
                string matHangVal = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_MatHang", "4|4|#EF4423|1");
                string[] mParts = matHangVal.Split('|');
                int mCols = 4, mRows = 4;
                if (mParts.Length > 0 && int.TryParse(mParts[0], out int mc) && mc >= 1) mCols = mc;
                if (mParts.Length > 1 && int.TryParse(mParts[1], out int mr) && mr >= 1) mRows = mr;

                MatHangColumns = mCols;
                _matHangRowsConfig = mRows;
                UpdateTileHeight();
            }
            catch { }
        }

        private void UpdateTileHeight()
        {
            try
            {
                double containerHeight = SvSearchMatHang != null && SvSearchMatHang.ActualHeight > 100 ? SvSearchMatHang.ActualHeight : 400;
                int rows = _matHangRowsConfig > 0 ? _matHangRowsConfig : 4;
                MatHangTileHeight = Math.Max(50, (containerHeight - (rows * 8)) / rows);
            }
            catch { }
        }

        private void SvSearchMatHang_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTileHeight();
        }

        private async void BtnConfigSearchMatHang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("MatHang");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadLayoutConfigAsync();
                }
            }
            catch { }
        }

        private void TxtSearchQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            string query = TxtSearchQuery.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                _filteredList = _sourceList.ToList();
            }
            else
            {
                string unsignedQuery = RemoveAccents(query).ToLower(CultureInfo.InvariantCulture);
                _filteredList = _sourceList.Where(x =>
                {
                    string unsignedName = RemoveAccents(x.TenMatHang ?? "").ToLower(CultureInfo.InvariantCulture);
                    string code = (x.MAMATHANG ?? "").ToLower(CultureInfo.InvariantCulture);
                    return unsignedName.Contains(unsignedQuery) || code.Contains(unsignedQuery);
                }).ToList();
            }

            IcSearchMatHang.ItemsSource = null;
            IcSearchMatHang.ItemsSource = _filteredList;
        }

        public static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }

        private void MatHangTile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is TouchMatHangVM item)
            {
                SelectedItem = item;
                if (Owner is TouchMainWindow mainWin)
                {
                    mainWin.AddItemToCart(item);
                }
                DialogResult = true;
                Close();
            }
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
