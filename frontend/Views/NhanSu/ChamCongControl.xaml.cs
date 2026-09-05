using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views;

namespace QuanLyBar.Client.Views.NhanSu
{
    public partial class ChamCongControl : UserControl
    {
        private List<BangLuongItemViewModel> _bangLuongList = new List<BangLuongItemViewModel>();
        private BangLuongItemViewModel _selectedBangLuong = null;
        private List<ChamCongNhanVienCaRow> _matrixRows = new List<ChamCongNhanVienCaRow>();
        private bool _isLoaded = false;

        public ChamCongControl()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    await LoadBangLuongListAsync();
                }
            };
        }

        public async Task LoadBangLuongListAsync(string selectId = null)
        {
            try
            {
                _bangLuongList = await LocalChamCongService.GetBangLuongListAsync();
                DgBangLuong.ItemsSource = null;
                DgBangLuong.ItemsSource = _bangLuongList;

                if (_bangLuongList.Count > 0)
                {
                    int idx = 0;
                    if (!string.IsNullOrEmpty(selectId))
                    {
                        int found = _bangLuongList.FindIndex(x => x.Id == selectId);
                        if (found >= 0) idx = found;
                    }

                    DgBangLuong.SelectedIndex = idx;
                    _selectedBangLuong = _bangLuongList[idx];
                    TxtSelectedBangLuongName.Text = _selectedBangLuong.Name;
                    await LoadChamCongMatrixAsync(_selectedBangLuong);
                }
                else
                {
                    _selectedBangLuong = null;
                    TxtSelectedBangLuongName.Text = "-";
                    _matrixRows.Clear();
                    DgChamCong.ItemsSource = null;
                    DgChamCong.Columns.Clear();
                    UpdateTotalsSummary();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách bảng lương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadChamCongMatrixAsync(BangLuongItemViewModel bangLuong)
        {
            if (bangLuong == null) return;

            int month = int.TryParse(bangLuong.Thang, out int m) ? m : DateTime.Now.Month;
            int year = int.TryParse(bangLuong.Nam, out int y) ? y : DateTime.Now.Year;

            try
            {
                BuildGridColumns(month, year);
                _matrixRows = await LocalChamCongService.GetChamCongMatrixAsync(bangLuong.Id, month, year);
                DgChamCong.ItemsSource = null;
                DgChamCong.ItemsSource = _matrixRows;
                UpdateTotalsSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bảng chấm công: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildGridColumns(int month, int year)
        {
            DgChamCong.Columns.Clear();

            // 1. Cột Nhân viên (Màu xanh lam nhạt #d4e8fc)
            var colName = new DataGridTextColumn
            {
                Header = "Nhân viên",
                Binding = new Binding("TenNhanVienDisplay"),
                Width = 140,
                IsReadOnly = true
            };
            var nameStyle = new Style(typeof(TextBlock));
            nameStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            nameStyle.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(6, 0, 4, 0)));
            nameStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            nameStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Black));
            colName.ElementStyle = nameStyle;

            var nameCellStyle = new Style(typeof(DataGridCell));
            nameCellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Color.FromRgb(212, 232, 252)))); // #d4e8fc
            nameCellStyle.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(182, 202, 223))));
            nameCellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            colName.CellStyle = nameCellStyle;

            DgChamCong.Columns.Add(colName);

            // 2. Cột Ca
            var colCa = new DataGridTextColumn
            {
                Header = "Ca",
                Binding = new Binding("TenCaLamViec"),
                Width = 75,
                IsReadOnly = true
            };
            var caStyle = new Style(typeof(TextBlock));
            caStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            caStyle.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(6, 0, 4, 0)));
            caStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, 11.5));
            colCa.ElementStyle = caStyle;

            var caCellStyle = new Style(typeof(DataGridCell));
            caCellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.White));
            caCellStyle.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(200, 215, 230))));
            caCellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            colCa.CellStyle = caCellStyle;

            DgChamCong.Columns.Add(colCa);

            // 3. Các cột ngày trong tháng (1..DaysInMonth)
            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int d = 1; d <= daysInMonth; d++)
            {
                DateTime date = new DateTime(year, month, d);
                string dayOfWeekStr = date.ToString("ddd", new CultureInfo("vi-VN")); // T2, T3, T4, T5, T6, T7, CN
                if (dayOfWeekStr.Length > 2) dayOfWeekStr = dayOfWeekStr.Substring(0, 2);
                bool isSunday = date.DayOfWeek == DayOfWeek.Sunday;

                // Header view
                var headerSp = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 2) };
                var txtDay = new TextBlock { Text = d.ToString(), FontWeight = FontWeights.SemiBold, FontSize = 10.5, HorizontalAlignment = HorizontalAlignment.Center };
                var txtDow = new TextBlock
                {
                    Text = dayOfWeekStr.ToUpper(),
                    FontSize = 9.5,
                    Foreground = isSunday ? Brushes.Red : new SolidColorBrush(Color.FromRgb(30, 63, 102)),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                headerSp.Children.Add(txtDay);
                headerSp.Children.Add(txtDow);

                // Column
                int dayIndex = d;
                var template = new DataTemplate();
                var borderFactory = new FrameworkElementFactory(typeof(Border));
                borderFactory.SetValue(Border.MarginProperty, new Thickness(1));
                borderFactory.SetBinding(Border.BackgroundProperty, new Binding($"DaysMap[{dayIndex}].BackgroundColor") { Mode = BindingMode.OneWay });
                borderFactory.SetValue(Border.CursorProperty, Cursors.Hand);

                template.VisualTree = borderFactory;

                var colDay = new DataGridTemplateColumn
                {
                    Header = headerSp,
                    CellTemplate = template,
                    Width = 28,
                    IsReadOnly = true,
                    SortMemberPath = dayIndex.ToString()
                };

                var dayCellStyle = new Style(typeof(DataGridCell));
                dayCellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(0)));
                dayCellStyle.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(200, 215, 230))));
                dayCellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
                dayCellStyle.Setters.Add(new Setter(DataGridCell.CursorProperty, Cursors.Hand));

                // Hover trigger
                var hoverTrigger = new Trigger { Property = DataGridCell.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(51, 153, 255))));
                hoverTrigger.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(1.5)));
                dayCellStyle.Triggers.Add(hoverTrigger);

                // Selected trigger - viền xanh đậm nổi bật khi chọn ô
                var selTrigger = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };
                selTrigger.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0, 102, 204))));
                selTrigger.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(2.5)));
                selTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 0, 102, 204))));
                dayCellStyle.Triggers.Add(selTrigger);

                DgChamCong.Columns.Add(colDay);
            }
        }

        private void DgChamCong_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest(DgChamCong, e.GetPosition(DgChamCong));
            if (hit != null)
            {
                var cell = FindVisualParent<DataGridCell>(hit.VisualHit);
                if (cell != null && cell.DataContext is ChamCongNhanVienCaRow && cell.Column != null)
                {
                    // If the cell is not currently selected, clear other selections and select this cell
                    var currentSelected = DgChamCong.SelectedCells;
                    bool isAlreadySelected = currentSelected.Any(c => c.Item == cell.DataContext && c.Column == cell.Column);
                    if (!isAlreadySelected)
                    {
                        DgChamCong.SelectedCells.Clear();
                        var cellInfo = new DataGridCellInfo(cell.DataContext, cell.Column);
                        DgChamCong.SelectedCells.Add(cellInfo);
                    }
                }
            }
        }

        private void ApplyStatusToSelectedCells(string status)
        {
            var selectedCells = DgChamCong.SelectedCells;
            if (selectedCells == null || selectedCells.Count == 0) return;

            foreach (var cellInfo in selectedCells)
            {
                if (cellInfo.Item is ChamCongNhanVienCaRow row && cellInfo.Column != null)
                {
                    int day = 0;
                    if (int.TryParse(cellInfo.Column.SortMemberPath, out int parsedDay) && parsedDay > 0)
                    {
                        day = parsedDay;
                    }
                    else
                    {
                        int colIndex = cellInfo.Column.DisplayIndex;
                        if (colIndex >= 2 && colIndex < 2 + row.DaysMap.Count)
                        {
                            day = colIndex - 1;
                        }
                    }

                    if (day > 0 && row.DaysMap.TryGetValue(day, out var cellItem))
                    {
                        cellItem.Status = status;
                    }
                }
            }

            if (_selectedBangLuong != null)
            {
                int month = int.TryParse(_selectedBangLuong.Thang, out int m) ? m : DateTime.Now.Month;
                int year = int.TryParse(_selectedBangLuong.Nam, out int y) ? y : DateTime.Now.Year;
                LocalChamCongService.RecalculateAllSalaries(_matrixRows, month, year);
            }

            UpdateTotalsSummary();
        }

        private void BtnKhongCoLich_Click(object sender, RoutedEventArgs e)
        {
            ApplyStatusToSelectedCells("0");
        }

        private void BtnDiLam_Click(object sender, RoutedEventArgs e)
        {
            ApplyStatusToSelectedCells("1");
        }

        private void BtnNghiCoPhep_Click(object sender, RoutedEventArgs e)
        {
            ApplyStatusToSelectedCells("2");
        }

        private void BtnNghiKhongPhep_Click(object sender, RoutedEventArgs e)
        {
            ApplyStatusToSelectedCells("3");
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent) return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void UpdateTotalsSummary()
        {
            if (_matrixRows == null || _matrixRows.Count == 0)
            {
                TxtTotalNhanVien.Text = "0";
                return;
            }

            var firstRows = _matrixRows.Where(x => x.IsFirstShiftOfEmployee).ToList();
            var distinctEmployees = firstRows.Count;
            TxtTotalNhanVien.Text = distinctEmployees.ToString("N0");
        }

        private void DgBangLuong_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void DgBangLuong_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest(DgBangLuong, e.GetPosition(DgBangLuong));
            if (hit != null)
            {
                var row = FindVisualParent<DataGridRow>(hit.VisualHit);
                if (row != null && row.Item is BangLuongItemViewModel item)
                {
                    DgBangLuong.SelectedItem = item;
                }
            }
        }

        private async void BtnNapLaiBangLuong_Click(object sender, RoutedEventArgs e)
        {
            await LoadBangLuongListAsync(_selectedBangLuong?.Id);
        }

        private async void DgBangLuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgBangLuong.SelectedItem is BangLuongItemViewModel selected)
            {
                _selectedBangLuong = selected;
                TxtSelectedBangLuongName.Text = _selectedBangLuong.Name;
                await LoadChamCongMatrixAsync(_selectedBangLuong);
            }
        }

        private void DgBangLuong_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnSuaBangLuong_Click(null, null);
        }

        private async void BtnThemBangLuong_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemSuaBangLuongWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true || win.IsSaved)
            {
                await LoadBangLuongListAsync(win.SavedId);
            }
        }

        private async void BtnSuaBangLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBangLuong == null)
            {
                MessageBox.Show("Vui lòng chọn bảng lương để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemSuaBangLuongWindow(_selectedBangLuong.Id);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true || win.IsSaved)
            {
                await LoadBangLuongListAsync(_selectedBangLuong.Id);
            }
        }

        private async void BtnXoaBangLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBangLuong == null)
            {
                MessageBox.Show("Vui lòng chọn bảng lương để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa bảng lương '{_selectedBangLuong.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalChamCongService.DeleteBangLuongAsync(_selectedBangLuong.Id);
                if (ok)
                {
                    MessageBox.Show("Đã xóa bảng lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadBangLuongListAsync();
                }
                else
                {
                    MessageBox.Show("Xóa không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuSortAsc_Click(object sender, RoutedEventArgs e)
        {
            if (_bangLuongList != null && _bangLuongList.Count > 0)
            {
                _bangLuongList = _bangLuongList.OrderBy(x => x.Name).ToList();
                DgBangLuong.ItemsSource = null;
                DgBangLuong.ItemsSource = _bangLuongList;
            }
        }

        private void MenuSortDesc_Click(object sender, RoutedEventArgs e)
        {
            if (_bangLuongList != null && _bangLuongList.Count > 0)
            {
                _bangLuongList = _bangLuongList.OrderByDescending(x => x.Name).ToList();
                DgBangLuong.ItemsSource = null;
                DgBangLuong.ItemsSource = _bangLuongList;
            }
        }

        private void MenuSortByName_Click(object sender, RoutedEventArgs e)
        {
            if (_bangLuongList != null && _bangLuongList.Count > 0)
            {
                _bangLuongList = _bangLuongList.OrderBy(x => x.Name).ToList();
                DgBangLuong.ItemsSource = null;
                DgBangLuong.ItemsSource = _bangLuongList;
            }
        }

        private void BtnInDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgBangLuong, "Bảng lương");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuSaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBangLuong != null)
            {
                Clipboard.SetText(_selectedBangLuong.Name ?? "");
            }
        }

        private void MenuSaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBangLuong != null)
            {
                Clipboard.SetText($"{_selectedBangLuong.Name}\t{_selectedBangLuong.Thang}/{_selectedBangLuong.Nam}");
            }
        }

        private async void BtnCapNhat_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBangLuong == null)
            {
                MessageBox.Show("Vui lòng chọn một bảng lương trước khi cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int month = int.TryParse(_selectedBangLuong.Thang, out int m) ? m : DateTime.Now.Month;
            int year = int.TryParse(_selectedBangLuong.Nam, out int y) ? y : DateTime.Now.Year;

            var (ok, error) = await LocalChamCongService.SaveChamCongMatrixAsync(_selectedBangLuong.Id, month, year, _matrixRows);
            if (ok)
            {
                MessageBox.Show($"Đã cập nhật bảng chấm công cho '{_selectedBangLuong.Name}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                // Reload matrix to verify persisted data
                await LoadChamCongMatrixAsync(_selectedBangLuong);
            }
            else
            {
                MessageBox.Show($"Cập nhật dữ liệu không thành công!\nChi tiết: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
