using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyBar.Client.Views
{
    public class ColumnDisplayItem
    {
        public DataGridColumn Column { get; set; }
        public string Header => Column?.Header?.ToString() ?? "";
        public bool DefaultVisible { get; set; }
        public int DefaultDisplayIndex { get; set; }
    }

    public partial class ChonCotHienThiWindow : Window
    {
        private DataGrid _grid;
        private List<ColumnDisplayItem> _allItems = new List<ColumnDisplayItem>();
        private ObservableCollection<ColumnDisplayItem> _hiddenColumns = new ObservableCollection<ColumnDisplayItem>();
        private ObservableCollection<ColumnDisplayItem> _visibleColumns = new ObservableCollection<ColumnDisplayItem>();

        public ChonCotHienThiWindow(DataGrid grid, List<string> defaultVisibleHeaders = null)
        {
            InitializeComponent();
            _grid = grid;

            int index = 0;
            foreach (var col in _grid.Columns)
            {
                string header = col.Header?.ToString();
                if (string.IsNullOrEmpty(header)) continue;

                // STT có thể giữ nguyên cố định nếu muốn, hoặc cho chọn
                if (header == "STT") continue;

                bool isDefaultVisible = defaultVisibleHeaders != null 
                    ? defaultVisibleHeaders.Contains(header) 
                    : (col.Visibility == Visibility.Visible);

                var item = new ColumnDisplayItem
                {
                    Column = col,
                    DefaultVisible = isDefaultVisible,
                    DefaultDisplayIndex = index++
                };
                _allItems.Add(item);

                if (col.Visibility == Visibility.Visible)
                {
                    _visibleColumns.Add(item);
                }
                else
                {
                    _hiddenColumns.Add(item);
                }
            }

            LstCotAn.ItemsSource = _hiddenColumns;
            LstCotHienThi.ItemsSource = _visibleColumns;
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = LstCotAn.SelectedItems.Cast<ColumnDisplayItem>().ToList();
            foreach (var item in selectedList)
            {
                _hiddenColumns.Remove(item);
                _visibleColumns.Add(item);
            }
        }

        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = LstCotHienThi.SelectedItems.Cast<ColumnDisplayItem>().ToList();
            foreach (var item in selectedList)
            {
                _visibleColumns.Remove(item);
                _hiddenColumns.Add(item);
            }
        }

        private void LstCotAn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstCotAn.SelectedItem is ColumnDisplayItem item)
            {
                _hiddenColumns.Remove(item);
                _visibleColumns.Add(item);
            }
        }

        private void LstCotHienThi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstCotHienThi.SelectedItem is ColumnDisplayItem item)
            {
                _visibleColumns.Remove(item);
                _hiddenColumns.Add(item);
            }
        }

        private void BtnChuyenLen_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = LstCotHienThi.SelectedIndex;
            if (selectedIndex > 0)
            {
                var item = _visibleColumns[selectedIndex];
                _visibleColumns.RemoveAt(selectedIndex);
                _visibleColumns.Insert(selectedIndex - 1, item);
                LstCotHienThi.SelectedIndex = selectedIndex - 1;
            }
        }

        private void BtnChuyenXuong_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = LstCotHienThi.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _visibleColumns.Count - 1)
            {
                var item = _visibleColumns[selectedIndex];
                _visibleColumns.RemoveAt(selectedIndex);
                _visibleColumns.Insert(selectedIndex + 1, item);
                LstCotHienThi.SelectedIndex = selectedIndex + 1;
            }
        }

        private void BtnKhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            _hiddenColumns.Clear();
            _visibleColumns.Clear();

            var sortedDefaults = _allItems.OrderBy(i => i.DefaultDisplayIndex).ToList();
            foreach (var item in sortedDefaults)
            {
                if (item.DefaultVisible)
                {
                    _visibleColumns.Add(item);
                }
                else
                {
                    _hiddenColumns.Add(item);
                }
            }
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            // Ẩn tất cả cột ẩn
            foreach (var item in _hiddenColumns)
            {
                if (item.Column != null)
                {
                    item.Column.Visibility = Visibility.Collapsed;
                }
            }

            // Hiện các cột được chọn và cập nhật thứ tự hiển thị DisplayIndex
            int displayIndex = 1; // 0 dành cho STT nếu có
            foreach (var item in _visibleColumns)
            {
                if (item.Column != null)
                {
                    item.Column.Visibility = Visibility.Visible;
                    if (item.Column.DisplayIndex != displayIndex && displayIndex < _grid.Columns.Count)
                    {
                        item.Column.DisplayIndex = displayIndex;
                    }
                    displayIndex++;
                }
            }

            DialogResult = true;
            Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
