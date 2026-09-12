using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class TongHopKqkdControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private bool _isInitialized = false;
        private bool _isLoading = false;
        private List<KqkdRowViewModel> _cachedList = new List<KqkdRowViewModel>();

        public TongHopKqkdControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            this.Loaded += TongHopKqkdControl_Loaded;
            this.IsVisibleChanged += TongHopKqkdControl_IsVisibleChanged;
        }

        private async void TongHopKqkdControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && _isInitialized)
            {
                await LoadDataAsync();
            }
        }

        private async void TongHopKqkdControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                await LoadDataAsync();
                return;
            }

            dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpDenNgay.SelectedDate = DateTime.Today;
            _isInitialized = true;

            await LoadCuaHangListAsync();
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAsync(string branchName = null)
        {
            try
            {
                _companyInfo = await LocalCauHinhService.GetCompanyInfoAsync(branchName ?? TxtSelectedCuaHang.Text);
            }
            catch { }
        }

        private async Task LoadCuaHangListAsync()
        {
            try
            {
                var stores = await _hoaDonService.GetCuaHangListAsync();
                LstCuaHang.ItemsSource = stores;
                if (stores.Count > 0 && string.IsNullOrEmpty(TxtSelectedCuaHang.Text))
                {
                    TxtSelectedCuaHang.Text = stores[0].Name;
                }
                await LoadCompanyInfoAsync(TxtSelectedCuaHang.Text);
            }
            catch { }
        }

        private void TxtSelectedCuaHang_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleCuaHang.IsChecked = !BtnToggleCuaHang.IsChecked;
        }

        private async void LstCuaHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCuaHang.SelectedItem is CuaHangViewModel sel)
            {
                TxtSelectedCuaHang.Text = sel.Name;
                BtnToggleCuaHang.IsChecked = false;
                await LoadCompanyInfoAsync(sel.Name);
            }
        }

        private async void BtnThemCuaHang_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new Window
            {
                Title = "Thêm Trụ sở / Cửa hàng mới",
                Width = 360,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(220, 232, 245))
            };

            var grid = new Grid { Margin = new Thickness(12) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lbl = new TextBlock { Text = "Tên trụ sở / cửa hàng:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
            var txt = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10), VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetRow(lbl, 0);
            Grid.SetRow(txt, 1);

            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnSave = new Button { Content = "Lưu", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true, Background = Brushes.White };
            var btnCancel = new Button { Content = "Đóng", Width = 75, Height = 26, IsCancel = true, Background = Brushes.White };
            sp.Children.Add(btnSave);
            sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2);

            grid.Children.Add(lbl);
            grid.Children.Add(txt);
            grid.Children.Add(sp);
            inputWin.Content = grid;

            btnSave.Click += async (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên cửa hàng/trụ sở!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await _hoaDonService.InsertCuaHangAsync(txt.Text.Trim());
                inputWin.DialogResult = true;
                inputWin.Close();
            };

            if (inputWin.ShowDialog() == true)
            {
                await LoadCuaHangListAsync();
            }
        }

        private async void BtnTaiCuaHang_Click(object sender, RoutedEventArgs e)
        {
            await LoadCuaHangListAsync();
        }

        private void BtnDanhMucCuaHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Danh mục cửa hàng/trụ sở đã được liệt kê trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized && !_isLoading)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private LocalCauHinhService.CompanyInfoModel _companyInfo;
        private int _currentA4Page = 1;
        private int _totalA4Pages = 1;
        private readonly List<Border> _pageBorders = new List<Border>();

        private async Task LoadDataAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;

                _companyInfo = await LocalCauHinhService.GetCompanyInfoAsync(TxtSelectedCuaHang.Text);
                _cachedList = await Task.Run(async () => await _hoaDonService.GetTongHopKqkdAsync(tuNgay, denNgay));

                BuildA4Pages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void BuildA4Pages()
        {
            PagesStackPanel.Children.Clear();
            _pageBorders.Clear();

            if (_cachedList == null || _cachedList.Count == 0) return;

            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            string storeName = string.IsNullOrEmpty(TxtSelectedCuaHang.Text) ? "NÀNG HƯƠNG QUÁN" : TxtSelectedCuaHang.Text.ToUpper();

            // Chia danh sách hợp lý sao cho Trang 1 đầy trang A4 trước khi ngắt sang Trang 2
            // Trang 1 chứa khoảng 35 - 38 dòng để lấp đầy khổ A4
            int splitIndex = _cachedList.FindIndex(r => r.Stt == "C." || r.Stt == "III.");
            if (splitIndex < 0 || splitIndex < 32)
            {
                splitIndex = Math.Min(36, _cachedList.Count);
            }

            var page1Items = _cachedList.Take(splitIndex).ToList();
            var page2Items = _cachedList.Skip(splitIndex).ToList();

            bool hasPage2 = page2Items.Count > 0;
            _totalA4Pages = hasPage2 ? 2 : 1;

            // --- TRANG 1 A4 ---
            var page1 = CreateA4PageBase();
            var p1Stack = (StackPanel)page1.Child;
            p1Stack.Children.Add(CreateHeaderBlock(storeName, tuNgay, denNgay));
            p1Stack.Children.Add(CreateKqkdTable(page1Items));
            if (hasPage2)
            {
                p1Stack.Children.Add(CreateFooterPageNumber("1", "2"));
            }
            else
            {
                p1Stack.Children.Add(CreateSignatureBlock());
                p1Stack.Children.Add(CreateFooterPageNumber("1", "1"));
            }
            _pageBorders.Add(page1);
            PagesStackPanel.Children.Add(page1);

            // --- TRANG 2 A4 (nếu có) ---
            if (hasPage2)
            {
                var page2 = CreateA4PageBase();
                var p2Stack = (StackPanel)page2.Child;
                p2Stack.Children.Add(CreateKqkdTable(page2Items));
                p2Stack.Children.Add(CreateSignatureBlock());
                p2Stack.Children.Add(CreateFooterPageNumber("2", "2"));
                _pageBorders.Add(page2);
                PagesStackPanel.Children.Add(page2);
            }

            _currentA4Page = 1;
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
        }

        private Border CreateA4PageBase()
        {
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(25),
                Width = 760,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 0, 25),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 15,
                    ShadowDepth = 4,
                    Opacity = 0.35,
                    Color = Colors.Black
                }
            };

            var sp = new StackPanel();
            border.Child = sp;
            return border;
        }

        private UIElement CreateHeaderBlock(string storeName, DateTime tuNgay, DateTime denNgay)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var logoBorder = new Border { Width = 65, Height = 65, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (_companyInfo?.LogoBytes != null && _companyInfo.LogoBytes.Length > 0)
            {
                var bi = LocalCauHinhService.ImageFromBytes(_companyInfo.LogoBytes);
                if (bi != null)
                {
                    logoBorder.Child = new Image { Source = bi, Stretch = Stretch.Uniform };
                }
                else
                {
                    logoBorder.Child = new TextBlock { Text = "🥢", FontSize = 42, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(68, 68, 68)) };
                }
            }
            else
            {
                logoBorder.Child = new TextBlock { Text = "🥢", FontSize = 42, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(68, 68, 68)) };
            }
            Grid.SetColumn(logoBorder, 0);

            var spInfo = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            string displayName = !string.IsNullOrWhiteSpace(storeName) && storeName != "Tất cả" && storeName != "[Tất cả]" ? storeName : (_companyInfo?.Name ?? "TRỤ SỞ CHÍNH");
            spInfo.Children.Add(new TextBlock { Text = displayName, FontWeight = FontWeights.Bold, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 2) });

            string addr = _companyInfo?.FormattedAddress ?? "";
            if (!string.IsNullOrWhiteSpace(addr))
            {
                spInfo.Children.Add(new TextBlock { Text = addr, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 2) });
            }

            string contact = _companyInfo?.FormattedContact ?? "";
            if (!string.IsNullOrWhiteSpace(contact))
            {
                spInfo.Children.Add(new TextBlock { Text = contact, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center });
            }
            Grid.SetColumn(spInfo, 1);

            grid.Children.Add(logoBorder);
            grid.Children.Add(spInfo);

            var container = new StackPanel();
            container.Children.Add(grid);

            var titleGrid = new Grid { Margin = new Thickness(0, 10, 0, 12) };
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var txtPeriod = new TextBlock { Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}", FontSize = 11, FontStyle = FontStyles.Italic, VerticalAlignment = VerticalAlignment.Center };
            var txtTitle = new TextBlock { Text = "BÁO CÁO KẾT QUẢ KINH DOANH", FontWeight = FontWeights.Bold, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };

            Grid.SetColumn(txtPeriod, 0);
            Grid.SetColumn(txtTitle, 1);
            titleGrid.Children.Add(txtPeriod);
            titleGrid.Children.Add(txtTitle);

            container.Children.Add(titleGrid);
            return container;
        }

        private UIElement CreateKqkdTable(List<KqkdRowViewModel> items)
        {
            var tableBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1, 1, 0, 0), Background = Brushes.White };
            var spTable = new StackPanel();

            // Header Row
            var headerGrid = new Grid { Height = 28, Background = Brushes.White };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            string[] headers = { "", "", "% / DT", "Giá trị", "%", " %/CP", "Tăng giảm so với\ntháng trước", "KQKD tháng trước" };
            for (int col = 0; col < headers.Length; col++)
            {
                var b = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness( col >= 6 ? 2 : col == 1 ? 4 : 2, 2, 2, 2) };
                var tb = new TextBlock
                {
                    Text = headers[col],
                    FontWeight = FontWeights.Bold,
                    FontSize = col >= 6 ? 9.5 : 10.5,
                    HorizontalAlignment = (col == 2 || col >= 4) ? HorizontalAlignment.Center : (col == 3 ? HorizontalAlignment.Center : HorizontalAlignment.Left),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = col >= 6 ? TextWrapping.Wrap : TextWrapping.NoWrap,
                    TextAlignment = col >= 6 ? TextAlignment.Center : TextAlignment.Left
                };
                b.Child = tb;
                Grid.SetColumn(b, col);
                headerGrid.Children.Add(b);
            }
            spTable.Children.Add(headerGrid);

            // Data Rows
            foreach (var item in items)
            {
                var rowGrid = new Grid { Height = 21, Background = Brushes.White };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(135) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                string[] vals = { item.Stt, item.ChiTieu, item.PhanTramDt, item.GiaTri, item.PhanTram, item.PhanTramCp, item.TangGiam, item.KqThangTruoc };
                for (int col = 0; col < vals.Length; col++)
                {
                    var b = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(col == 1 ? 4 : 2, 2, col == 3 || col >= 6 ? 4 : 2, 2) };
                    var tb = new TextBlock
                    {
                        Text = vals[col],
                        FontSize = 11,
                        FontWeight = item.IsBold ? FontWeights.Bold : FontWeights.Normal,
                        HorizontalAlignment = (col == 0 || col == 2 || col == 4 || col == 5) ? HorizontalAlignment.Center : (col == 1 ? HorizontalAlignment.Left : HorizontalAlignment.Right),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    b.Child = tb;
                    Grid.SetColumn(b, col);
                    rowGrid.Children.Add(b);
                }
                spTable.Children.Add(rowGrid);
            }

            tableBorder.Child = spTable;
            return tableBorder;
        }

        private UIElement CreateSignatureBlock()
        {
            var grid = new Grid { Margin = new Thickness(0, 15, 0, 20) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });

            var spRight = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            spRight.Children.Add(new TextBlock { Text = $"Ngày {DateTime.Now.Day:D2} tháng {DateTime.Now.Month:D2} năm {DateTime.Now.Year}", FontSize = 11, FontStyle = FontStyles.Italic, Margin = new Thickness(0, 0, 0, 4), HorizontalAlignment = HorizontalAlignment.Center });
            spRight.Children.Add(new TextBlock { Text = "Người lập", FontWeight = FontWeights.Bold, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center });
            spRight.Children.Add(new TextBlock { Text = "(Ký, họ tên)", FontSize = 10, FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 50) });

            Grid.SetColumn(spRight, 1);
            grid.Children.Add(spRight);
            return grid;
        }

        private UIElement CreateFooterPageNumber(string currentPage, string totalPages)
        {
            var txt = new TextBlock
            {
                Text = $"Trang {currentPage}/{totalPages}",
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            return txt;
        }

        private void ScrollToPage(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < _pageBorders.Count)
            {
                var targetBorder = _pageBorders[pageIndex];
                GeneralTransform transform = targetBorder.TransformToVisual(PagesStackPanel);
                Point point = transform.Transform(new Point(0, 0));
                ReportScrollViewer.ScrollToVerticalOffset(point.Y);
            }
        }

        private void ScrollToCurrentPage()
        {
            if (_currentA4Page >= 1 && _currentA4Page <= _pageBorders.Count)
            {
                ScrollToPage(_currentA4Page - 1);
            }
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
        }

        private void BtnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            _currentA4Page = 1;
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
            ScrollToPage(0);
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentA4Page > 1)
            {
                _currentA4Page--;
                TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
                ScrollToPage(_currentA4Page - 1);
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentA4Page < _totalA4Pages)
            {
                _currentA4Page++;
                TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
                ScrollToPage(_currentA4Page - 1);
            }
        }

        private void BtnLastPage_Click(object sender, RoutedEventArgs e)
        {
            _currentA4Page = _totalA4Pages;
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
            ScrollToPage(_totalA4Pages - 1);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    foreach (var pBorder in _pageBorders)
                    {
                        printDlg.PrintVisual(pBorder, "BÁO CÁO KẾT QUẢ KINH DOANH");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"BaoCaoKQKD_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (_cachedList != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,Khoản mục,% / DT,Giá trị,%,%/CP,Tăng giảm so với tháng trước,KQKD tháng trước");
                        foreach (var item in _cachedList)
                        {
                            sb.AppendLine($"\"{item.Stt}\",\"{item.ChiTieu}\",\"{item.PhanTramDt}\",\"{item.GiaTri}\",\"{item.PhanTram}\",\"{item.PhanTramCp}\",\"{item.TangGiam}\",\"{item.KqThangTruoc}\"");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
