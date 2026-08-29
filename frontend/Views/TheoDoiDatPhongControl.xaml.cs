using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class TheoDoiDatPhongControl : UserControl
    {
        private LocalTheoDoiDatPhongService _service;
        private int _currentDayCount = 14;
        private string _selectedKhuVucId = "";
        private string _selectedKhuVucName = "Tất cả";

        public TheoDoiDatPhongControl()
        {
            InitializeComponent();
            _service = new LocalTheoDoiDatPhongService();
            
            DpStartDate.SelectedDate = DateTime.Now;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var khuVucs = await _service.GetKhuVucLookupAsync();
            CmbKhuVuc.ItemsSource = khuVucs;
            if (khuVucs.Count > 1)
            {
                // Chọn khu vực đầu tiên (ví dụ phong mot nếu có) hoặc Tất cả
                CmbKhuVuc.SelectedIndex = 1;
            }
            else if (khuVucs.Count > 0)
            {
                CmbKhuVuc.SelectedIndex = 0;
            }

            await LoadData();
        }

        private async void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadData();
        }

        private async void CmbKhuVuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbKhuVuc.SelectedItem is LookupItem item)
            {
                _selectedKhuVucId = item.Id;
                _selectedKhuVucName = item.Name;
            }
            else
            {
                _selectedKhuVucId = "";
                _selectedKhuVucName = "Tất cả";
            }
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            if (!DpStartDate.SelectedDate.HasValue) return;
            DateTime startDate = DpStartDate.SelectedDate.Value.Date;

            // Xây dựng các cột lưới và header tháng năm
            BuildGridColumnsAndMonthHeaders(startDate, _currentDayCount);

            // Tải dữ liệu từ service
            var data = await _service.GetTheoDoiDataAsync(startDate, _currentDayCount, _selectedKhuVucId);
            DgTheoDoi.ItemsSource = data;
        }

        private void BuildGridColumnsAndMonthHeaders(DateTime startDate, int dayCount)
        {
            DgTheoDoi.Columns.Clear();
            GridMonthHeaders.Children.Clear();
            GridMonthHeaders.ColumnDefinitions.Clear();

            double colKhuVucWidth = 100;
            double colPhongWidth = 100;
            double colTongWidth = 60;

            // --- 1. Tạo ColumnDefinitions cho GridMonthHeaders ---
            // Cột 0: Khu vực
            GridMonthHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(colKhuVucWidth) });
            // Cột 1: Phòng
            GridMonthHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(colPhongWidth) });
            // Cột 2 đến 2 + dayCount - 1: Các ngày
            for (int i = 0; i < dayCount; i++)
            {
                GridMonthHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 48 });
            }
            // Cột cuối: Tổng
            GridMonthHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(colTongWidth) });

            // --- 2. Thêm các khối tháng vào GridMonthHeaders ---
            // Cột trống cho khu vực và phòng
            var blankBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#485f75")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d566e")),
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(blankBorder, 0);
            Grid.SetColumnSpan(blankBorder, 2);
            GridMonthHeaders.Children.Add(blankBorder);

            // Nhóm ngày theo tháng
            int curColIndex = 2;
            int iDay = 0;
            while (iDay < dayCount)
            {
                DateTime blockStartDate = startDate.AddDays(iDay);
                int month = blockStartDate.Month;
                int year = blockStartDate.Year;
                int countInMonth = 0;

                while (iDay < dayCount && startDate.AddDays(iDay).Month == month && startDate.AddDays(iDay).Year == year)
                {
                    countInMonth++;
                    iDay++;
                }

                var monthBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#485f75")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d566e")),
                    BorderThickness = new Thickness(0, 0, 1, 0)
                };

                var monthGrid = new Grid();
                monthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
                monthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                monthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });

                var txtPrev = new TextBlock
                {
                    Text = "◀",
                    Foreground = Brushes.White,
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                int prevMonthOffset = -1;
                txtPrev.MouseLeftButtonDown += (s, e) =>
                {
                    DpStartDate.SelectedDate = startDate.AddMonths(prevMonthOffset);
                };
                Grid.SetColumn(txtPrev, 0);
                monthGrid.Children.Add(txtPrev);

                var txtMonth = new TextBlock
                {
                    Text = $"Tháng {month} - {year}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetColumn(txtMonth, 1);
                monthGrid.Children.Add(txtMonth);

                var txtNext = new TextBlock
                {
                    Text = "▶",
                    Foreground = Brushes.White,
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                int nextMonthOffset = 1;
                txtNext.MouseLeftButtonDown += (s, e) =>
                {
                    DpStartDate.SelectedDate = startDate.AddMonths(nextMonthOffset);
                };
                Grid.SetColumn(txtNext, 2);
                monthGrid.Children.Add(txtNext);

                monthBorder.Child = monthGrid;
                Grid.SetColumn(monthBorder, curColIndex);
                Grid.SetColumnSpan(monthBorder, countInMonth);
                GridMonthHeaders.Children.Add(monthBorder);

                curColIndex += countInMonth;
            }

            // Nút / Header Tổng bên phải
            var tongBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d566e")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a3c4d")),
                BorderThickness = new Thickness(1, 0, 0, 0)
            };
            var txtTong = new TextBlock
            {
                Text = "Tổng",
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            tongBorder.Child = txtTong;
            Grid.SetColumn(tongBorder, 2 + dayCount);
            GridMonthHeaders.Children.Add(tongBorder);

            // --- 3. Tạo các cột cho DataGrid ---
            // Cột 0: Khu vực (ví dụ phong mot)
            string displayAreaHeader = !string.IsNullOrEmpty(_selectedKhuVucName) && _selectedKhuVucName != "Tất cả" ? _selectedKhuVucName : "Khu vực";
            var colKhuVuc = new DataGridTextColumn
            {
                Header = displayAreaHeader,
                Binding = new Binding("KhuVucName"),
                Width = new DataGridLength(colKhuVucWidth)
            };
            colKhuVuc.HeaderStyle = CreateHeaderStyle(false);
            DgTheoDoi.Columns.Add(colKhuVuc);

            // Cột 1: Phòng
            var colPhong = new DataGridTextColumn
            {
                Header = "Phòng",
                Binding = new Binding("PhongName"),
                Width = new DataGridLength(colPhongWidth)
            };
            colPhong.HeaderStyle = CreateHeaderStyle(false);
            var phongElementStyle = new Style(typeof(TextBlock));
            phongElementStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, new Binding("RowFontWeight")));
            phongElementStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("RowForeground")));
            phongElementStyle.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(4, 0, 0, 0)));
            phongElementStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            colPhong.ElementStyle = phongElementStyle;
            DgTheoDoi.Columns.Add(colPhong);

            // Các cột ngày
            for (int i = 0; i < dayCount; i++)
            {
                int colIdx = i;
                DateTime curDate = startDate.AddDays(i);
                string dowStr = GetDayOfWeekVN(curDate.DayOfWeek);
                string headerText = $"{curDate.Day:D2}.{dowStr}";

                var colDay = new DataGridTemplateColumn
                {
                    Header = headerText,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    MinWidth = 48
                };

                bool isSunday = curDate.DayOfWeek == DayOfWeek.Sunday;
                colDay.HeaderStyle = CreateHeaderStyle(isSunday);

                // Template cho Cell
                var borderFactory = new FrameworkElementFactory(typeof(Border));
                borderFactory.SetBinding(Border.BackgroundProperty, new Binding($"Cells[{colIdx}].CellBackground"));
                borderFactory.SetValue(Border.PaddingProperty, new Thickness(2, 1, 2, 1));
                borderFactory.SetValue(Border.SnapsToDevicePixelsProperty, true);

                var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
                tbFactory.SetBinding(TextBlock.TextProperty, new Binding($"Cells[{colIdx}].Text"));
                tbFactory.SetBinding(TextBlock.ForegroundProperty, new Binding($"Cells[{colIdx}].CellForeground"));
                tbFactory.SetBinding(TextBlock.FontWeightProperty, new Binding($"Cells[{colIdx}].FontWeight"));
                tbFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                tbFactory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);

                // Tooltip nếu có thông tin đặt
                var ttBinding = new Binding($"Cells[{colIdx}]");
                var tooltipFactory = new FrameworkElementFactory(typeof(ToolTip));
                var tooltipTb = new FrameworkElementFactory(typeof(TextBlock));
                tooltipTb.SetBinding(TextBlock.TextProperty, new Binding($"Cells[{colIdx}].CustomerName"));
                tooltipFactory.AppendChild(tooltipTb);

                borderFactory.AppendChild(tbFactory);
                colDay.CellTemplate = new DataTemplate { VisualTree = borderFactory };

                DgTheoDoi.Columns.Add(colDay);
            }

            // Cột Tổng
            var colTong = new DataGridTextColumn
            {
                Header = "Tổng",
                Binding = new Binding("Tong"),
                Width = new DataGridLength(colTongWidth)
            };
            colTong.HeaderStyle = CreateTotalHeaderStyle();
            var tongElementStyle = new Style(typeof(TextBlock));
            tongElementStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            tongElementStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            tongElementStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, new Binding("RowFontWeight")));
            tongElementStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("RowForeground")));
            colTong.ElementStyle = tongElementStyle;
            DgTheoDoi.Columns.Add(colTong);
        }

        private Style CreateHeaderStyle(bool isSunday)
        {
            var style = new Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#485f75"))));
            style.Setters.Add(new Setter(Control.ForegroundProperty, isSunday ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffff00")) : Brushes.White));
            style.Setters.Add(new Setter(Control.FontWeightProperty, isSunday ? FontWeights.Bold : FontWeights.Normal));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4, 3, 4, 3)));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d566e"))));
            return style;
        }

        private Style CreateTotalHeaderStyle()
        {
            var style = new Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d566e"))));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4, 3, 4, 3)));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1, 0, 0, 1)));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a3c4d"))));
            return style;
        }

        private string GetDayOfWeekVN(DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Monday: return "T2";
                case DayOfWeek.Tuesday: return "T3";
                case DayOfWeek.Wednesday: return "T4";
                case DayOfWeek.Thursday: return "T5";
                case DayOfWeek.Friday: return "T6";
                case DayOfWeek.Saturday: return "T7";
                case DayOfWeek.Sunday: return "CN";
                default: return "";
            }
        }

        private async void BtnPrevPeriod_Click(object sender, RoutedEventArgs e)
        {
            if (DpStartDate.SelectedDate.HasValue)
            {
                DpStartDate.SelectedDate = DpStartDate.SelectedDate.Value.AddDays(-_currentDayCount);
            }
        }

        private async void BtnNextPeriod_Click(object sender, RoutedEventArgs e)
        {
            if (DpStartDate.SelectedDate.HasValue)
            {
                DpStartDate.SelectedDate = DpStartDate.SelectedDate.Value.AddDays(_currentDayCount);
            }
        }

        private async void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            DpStartDate.SelectedDate = DateTime.Now;
        }

        private async void BtnZoom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int days))
            {
                _currentDayCount = days;

                // Reset styles
                BtnZoomDiv4.Style = (Style)FindResource("ZoomButtonStyle");
                BtnZoomDiv2.Style = (Style)FindResource("ZoomButtonStyle");
                BtnZoom2x.Style = (Style)FindResource("ZoomButtonStyle");
                BtnZoom4x.Style = (Style)FindResource("ZoomButtonStyle");

                btn.Style = (Style)FindResource("ActiveZoomButtonStyle");

                await LoadData();
            }
        }

        private void DgTheoDoi_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgTheoDoi.SelectedItem is TheoDoiDatPhongRowViewModel selectedRow)
            {
                if (selectedRow.IsSummary) return;

                // Lấy cell được double click
                var hit = VisualTreeHelper.HitTest(DgTheoDoi, e.GetPosition(DgTheoDoi));
                if (hit != null)
                {
                    DependencyObject dep = hit.VisualHit;
                    while (dep != null && !(dep is DataGridCell))
                    {
                        dep = VisualTreeHelper.GetParent(dep);
                    }

                    if (dep is DataGridCell cell && cell.Column is DataGridTemplateColumn)
                    {
                        int colIndex = DgTheoDoi.Columns.IndexOf(cell.Column) - 2; // Bỏ qua 2 cột đầu
                        if (colIndex >= 0 && colIndex < selectedRow.Cells.Count)
                        {
                            var cellData = selectedRow.Cells[colIndex];
                            if (cellData.IsBooked && !string.IsNullOrEmpty(cellData.BookingId))
                            {
                                // Mở xem / sửa đơn đặt phòng đã có
                                var win = new ThemMoiDatHangWindow(cellData.BookingId);
                                win.Owner = Window.GetWindow(this);
                                win.OrderSaved += async () => await LoadData();
                                win.ShowDialog();
                                _ = LoadData();
                                return;
                            }
                            else
                            {
                                // Tạo mới đơn đặt hàng cho ngày và phòng này
                                var win = new ThemMoiDatHangWindow();
                                win.Owner = Window.GetWindow(this);
                                win.OrderSaved += async () => await LoadData();
                                win.ShowDialog();
                                _ = LoadData();
                                return;
                            }
                        }
                    }
                }

                // Fallback nếu không bấm đúng cell ngày
                var defaultWin = new ThemMoiDatHangWindow();
                defaultWin.Owner = Window.GetWindow(this);
                defaultWin.OrderSaved += async () => await LoadData();
                defaultWin.ShowDialog();
                _ = LoadData();
            }
        }

        private void BtnThemDatPhong_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemMoiDatHangWindow();
            win.Owner = Window.GetWindow(this);
            win.OrderSaved += async () => await LoadData();
            win.ShowDialog();
            _ = LoadData();
        }

        private void MenuItem_SuaDatPhong_Click(object sender, RoutedEventArgs e)
        {
            if (DgTheoDoi.SelectedItem is TheoDoiDatPhongRowViewModel selectedRow && !selectedRow.IsSummary)
            {
                var firstBooked = selectedRow.Cells.FirstOrDefault(c => c.IsBooked);
                if (firstBooked != null && !string.IsNullOrEmpty(firstBooked.BookingId))
                {
                    var win = new ThemMoiDatHangWindow(firstBooked.BookingId);
                    win.Owner = Window.GetWindow(this);
                    win.OrderSaved += async () => await LoadData();
                    win.ShowDialog();
                    _ = LoadData();
                }
                else
                {
                    MessageBox.Show("Phòng này chưa có đơn đặt trong kỳ được chọn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgTheoDoi, "Theo dõi đặt phòng");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
