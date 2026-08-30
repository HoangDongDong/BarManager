using System;
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
    }

    public partial class ChonCotHienThiWindow : Window
    {
        public static readonly List<string> DefaultColumns = new List<string>
        {
            "Tên bàn",
            "Ghi chú",
            "Khu vực",
            "Nhóm hiển thị",
            "Loại phòng",
            "Đơn giá"
        };

        private DataGrid _grid;
        private List<ColumnDisplayItem> _allItems = new List<ColumnDisplayItem>();
        private ObservableCollection<ColumnDisplayItem> _hiddenColumns = new ObservableCollection<ColumnDisplayItem>();
        private ObservableCollection<ColumnDisplayItem> _visibleColumns = new ObservableCollection<ColumnDisplayItem>();

        public ChonCotHienThiWindow(DataGrid grid, List<string> defaultVisibleHeaders = null)
        {
            InitializeComponent();
            _grid = grid;

            var defaults = defaultVisibleHeaders ?? DefaultColumns;

            foreach (var col in _grid.Columns)
            {
                string header = col.Header?.ToString();
                if (string.IsNullOrEmpty(header) || header == "STT") continue;

                var item = new ColumnDisplayItem
                {
                    Column = col,
                    DefaultVisible = defaults.Any(d => string.Equals(d, header, StringComparison.OrdinalIgnoreCase))
                };
                _allItems.Add(item);
            }

            // Danh sách hiển thị theo thứ tự DisplayIndex hiện tại trên lưới
            var currentVisible = _allItems
                .Where(i => i.Column.Visibility == Visibility.Visible)
                .OrderBy(i => i.Column.DisplayIndex)
                .ToList();

            var currentHidden = _allItems
                .Where(i => i.Column.Visibility != Visibility.Visible)
                .ToList();

            foreach (var item in currentVisible) _visibleColumns.Add(item);
            foreach (var item in currentHidden) _hiddenColumns.Add(item);

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

            // 1. Thêm các cột mặc định theo đúng thứ tự yêu cầu:
            // Tên bàn, Ghi chú, Khu vực, Nhóm hiển thị, Loại phòng, Đơn giá
            foreach (var defaultName in DefaultColumns)
            {
                var match = _allItems.FirstOrDefault(i => string.Equals(i.Header, defaultName, StringComparison.OrdinalIgnoreCase));
                if (match != null && !_visibleColumns.Contains(match))
                {
                    _visibleColumns.Add(match);
                }
            }

            // 2. Tất cả các cột còn lại đưa vào danh sách ẩn
            foreach (var item in _allItems)
            {
                if (!_visibleColumns.Contains(item))
                {
                    _hiddenColumns.Add(item);
                }
            }
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Ẩn tất cả cột trong danh sách cột ẩn
            foreach (var item in _hiddenColumns)
            {
                if (item.Column != null)
                {
                    item.Column.Visibility = Visibility.Collapsed;
                }
            }

            // 2. Chuẩn bị danh sách toàn bộ cột theo thứ tự hiển thị mong muốn
            var orderedCols = new List<DataGridColumn>();

            // Giữ STT ở đầu nếu có
            var sttCol = _grid.Columns.FirstOrDefault(c => c.Header?.ToString() == "STT");
            if (sttCol != null)
            {
                sttCol.Visibility = Visibility.Visible;
                orderedCols.Add(sttCol);
            }

            // Thêm các cột hiển thị theo đúng thứ tự người dùng đã chọn
            foreach (var item in _visibleColumns)
            {
                if (item.Column != null)
                {
                    item.Column.Visibility = Visibility.Visible;
                    if (!orderedCols.Contains(item.Column))
                    {
                        orderedCols.Add(item.Column);
                    }
                }
            }

            // Thêm các cột ẩn còn lại vào cuối
            foreach (var item in _hiddenColumns)
            {
                if (item.Column != null && !orderedCols.Contains(item.Column))
                {
                    orderedCols.Add(item.Column);
                }
            }

            // Gán DisplayIndex tuần tự để WPF cập nhật vị trí chính xác 100%
            for (int i = 0; i < orderedCols.Count; i++)
            {
                orderedCols[i].DisplayIndex = i;
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
