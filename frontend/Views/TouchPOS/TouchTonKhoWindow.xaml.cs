using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchTonKhoWindow : Window
    {
        private List<BaoCaoTonKhoTileItem> _allTonData = new List<BaoCaoTonKhoTileItem>();
        private string? _selectedKhoId = null;
        private string? _selectedNhomId = null;
        private bool _filterTonKhac0 = false; // Mặc định hiển thị tất cả mặt hàng (kể cả tồn = 0)

        public TouchTonKhoWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhoListAsync();
            await LoadNhomListAsync();
            await LoadTonDataAsync();
        }

        private async Task LoadKhoListAsync()
        {
            PanelKhoButtons.Children.Clear();

            // Nút TẤT CẢ kho
            AddKhoButton("TẤT CẢ", null, true);

            try
            {
                var khoList = await LocalKhoHangService.GetAllWarehousesFlatAsync();
                foreach (var k in khoList)
                {
                    AddKhoButton(k.Name, k.Id, false);
                }
            }
            catch { }
        }

        private void AddKhoButton(string name, string? id, bool isSelected)
        {
            var btn = new Button
            {
                Content = name,
                Height = 36,
                Margin = new Thickness(0, 2, 2, 2),
                Tag = id,
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };

            SetKhoButtonStyle(btn, isSelected);

            btn.Click += async (s, e) =>
            {
                _selectedKhoId = id;
                foreach (Button child in PanelKhoButtons.Children.OfType<Button>())
                {
                    SetKhoButtonStyle(child, child.Tag as string == _selectedKhoId);
                }
                await LoadTonDataAsync();
            };

            PanelKhoButtons.Children.Add(btn);
        }

        private void SetKhoButtonStyle(Button btn, bool isSelected)
        {
            var darkBlue = (Brush)new BrushConverter().ConvertFromString("#0B1B3D")!;
            var orange = (Brush)new BrushConverter().ConvertFromString("#FF6F00")!;
            var teal = (Brush)new BrushConverter().ConvertFromString("#00695C")!;

            Brush bg = isSelected ? (btn.Tag == null ? darkBlue : orange) : teal;

            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, bg);
            factory.SetValue(Border.BorderBrushProperty, (Brush)new BrushConverter().ConvertFromString("#36B5B0")!);
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(ContentPresenter.MarginProperty, new Thickness(8, 0, 0, 0));

            factory.AppendChild(presenter);
            template.VisualTree = factory;

            btn.Template = template;
        }

        private async Task LoadNhomListAsync()
        {
            PanelNhomButtons.Children.Clear();
            AddNhomButton("TẤT CẢ", null, true);

            try
            {
                var matHangService = new LocalMatHangService();
                var nhomList = await matHangService.GetNhomMatHangListAsync();
                foreach (var n in nhomList)
                {
                    AddNhomButton(n.Name, n.Id, false);
                }
            }
            catch { }
        }

        private void AddNhomButton(string name, string? id, bool isSelected)
        {
            var btn = new Button
            {
                Content = name,
                Height = 36,
                Margin = new Thickness(0, 2, 2, 2),
                Tag = id,
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };

            SetNhomButtonStyle(btn, isSelected);

            btn.Click += (s, e) =>
            {
                _selectedNhomId = id;
                foreach (Button child in PanelNhomButtons.Children.OfType<Button>())
                {
                    SetNhomButtonStyle(child, child.Tag as string == _selectedNhomId);
                }
                ApplyFilters();
            };

            PanelNhomButtons.Children.Add(btn);
        }

        private void SetNhomButtonStyle(Button btn, bool isSelected)
        {
            var orange = (Brush)new BrushConverter().ConvertFromString("#FF6F00")!;
            var green = (Brush)new BrushConverter().ConvertFromString("#00695C")!;

            Brush bg = isSelected ? orange : green;

            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, bg);
            factory.SetValue(Border.BorderBrushProperty, (Brush)new BrushConverter().ConvertFromString("#36B5B0")!);
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(ContentPresenter.MarginProperty, new Thickness(8, 0, 0, 0));

            factory.AppendChild(presenter);
            template.VisualTree = factory;

            btn.Template = template;
        }

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(TouchTonKhoWindow), new PropertyMetadata(3));

        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.Register(nameof(RowCount), typeof(int), typeof(TouchTonKhoWindow), new PropertyMetadata(5));

        public static readonly DependencyProperty TileHeightProperty =
            DependencyProperty.Register(nameof(TileHeight), typeof(double), typeof(TouchTonKhoWindow), new PropertyMetadata(85.0));

        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        public int RowCount
        {
            get => (int)GetValue(RowCountProperty);
            set => SetValue(RowCountProperty, value);
        }

        public double TileHeight
        {
            get => (double)GetValue(TileHeightProperty);
            set => SetValue(TileHeightProperty, value);
        }

        private List<string> _usedColumns = new();

        private void RecalculateTileHeight()
        {
            if (TileScrollViewer != null && TileScrollViewer.ActualHeight > 100 && RowCount > 0)
            {
                double totalHeight = TileScrollViewer.ActualHeight;
                double available = totalHeight - (RowCount * 9.0);
                TileHeight = Math.Max(50.0, Math.Floor(available / RowCount));
            }
        }

        private void TileScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalculateTileHeight();
        }

        private async Task LoadTileLayoutConfigAsync()
        {
            try
            {
                string layoutValue = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_TonKho", "3|5|#0D4B5B");
                string[] parts = layoutValue.Split('|');
                if (parts.Length > 0 && int.TryParse(parts[0], out int columns) && columns is >= 1 and <= 99)
                {
                    ColumnCount = columns;
                }
                if (parts.Length > 1 && int.TryParse(parts[1], out int rows) && rows is >= 1 and <= 99)
                {
                    RowCount = rows;
                }

                RecalculateTileHeight();

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLUMNS_TonKho", "");
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    _usedColumns = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    _usedColumns = new List<string> { "GHI CHÚ", "GIÁ TRỊ BÁN", "GIÁ VỐN", "TỒN 2 ĐVT", "TỒN", "HÃNG SX" };
                }

                UpdateAllItemDisplayColumns();
            }
            catch { }
        }

        private void UpdateAllItemDisplayColumns()
        {
            foreach (var item in _allTonData)
            {
                item.BuildColumns(_usedColumns);
            }
        }

        private async Task LoadTonDataAsync()
        {
            try
            {
                var list = await LocalTonKhoService.GetTonKhoListAsync(_selectedKhoId, null, null, false);
                _allTonData = list.Select(x => new BaoCaoTonKhoTileItem
                {
                    MaMatHang = x.MaHang,
                    TenMatHang = x.TenHang,
                    MaNhom = x.DnhommathangId,
                    MaKho = _selectedKhoId ?? "",
                    SoLuongTon = x.Ton,
                    TenDonViTinh = x.TenDonViTinh,
                    GiaBan = x.GiaBan,
                    GiaVon = x.GiaVon,
                    GhiChu = x.GhiChu,
                    Ton2Dvt = x.Ton2Dvt
                }).ToList();

                await LoadTileLayoutConfigAsync();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tồn kho: " + ex.Message);
            }
        }

        private string _searchKeyword = "";

        private static string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }

        private void ApplyFilters()
        {
            var query = _allTonData.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedNhomId) && _selectedNhomId != "all")
            {
                query = query.Where(x => x.MaNhom == _selectedNhomId);
            }

            if (_filterTonKhac0)
            {
                query = query.Where(x => x.SoLuongTon != 0);
            }

            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                string kw = RemoveVietnameseDiacritics(_searchKeyword).Trim().ToLower();
                query = query.Where(x => (x.TenMatHang != null && RemoveVietnameseDiacritics(x.TenMatHang).ToLower().Contains(kw)) ||
                                         (x.MaMatHang != null && RemoveVietnameseDiacritics(x.MaMatHang).ToLower().Contains(kw)));
            }

            ItemsTileControl.ItemsSource = query.OrderBy(x => x.TenMatHang).ToList();

            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                TxtSearchBadge.Text = _searchKeyword;
                BdrSearchBadge.Visibility = Visibility.Visible;
            }
            else
            {
                TxtSearchBadge.Text = "";
                BdrSearchBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnToggleTonKhac0_Click(object sender, RoutedEventArgs e)
        {
            _filterTonKhac0 = !_filterTonKhac0;
            ApplyFilters();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            try
            {
                if (btn != null)
                {
                    btn.IsEnabled = false;
                    btn.Content = "ĐANG TẢI...";
                }

                await LoadKhoListAsync();
                await LoadNhomListAsync();
                await LoadTonDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi làm mới dữ liệu tồn kho: " + ex.Message, "Lỗi Refresh", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (btn != null)
                {
                    btn.Content = "REFRESH";
                    btn.IsEnabled = true;
                }
            }
        }

        private BaoCaoTonKhoTileItem? _selectedTileItem = null;

        private void ItemTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is BaoCaoTonKhoTileItem item)
            {
                foreach (var x in _allTonData)
                {
                    x.IsSelected = false;
                }
                item.IsSelected = true;
                _selectedTileItem = item;

                ItemsTileControl.Items.Refresh();

                if (e.ClickCount == 2)
                {
                    OpenChiTietWindow(item);
                }
            }
        }

        private void BtnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var target = _selectedTileItem ?? _allTonData.FirstOrDefault();
            if (target == null)
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để xem chi tiết.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            OpenChiTietWindow(target);
        }

        private void OpenChiTietWindow(BaoCaoTonKhoTileItem item)
        {
            try
            {
                var win = new TouchChiTietTonKhoWindow(item.MaMatHang, item.TenMatHang);
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xem chi tiết mặt hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var displayList = (ItemsTileControl.ItemsSource as IEnumerable<BaoCaoTonKhoTileItem>)?.ToList() ?? _allTonData;
                if (displayList.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu tồn kho để xuất Excel.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"BaoCaoTonKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("TonKho");

                        // 1. Tiêu đề cột STT
                        ws.Cell(1, 1).Value = "STT";

                        // 2. Tiêu đề các cột theo thuộc tính đã cài đặt
                        int colIdx = 2;
                        foreach (var col in _usedColumns)
                        {
                            string upper = col.Trim().ToUpperInvariant();
                            string headerText = upper switch
                            {
                                "GHI CHÚ" => "Ghi chú",
                                "GIÁ TRỊ BÁN" => "Giá trị bán",
                                "GIÁ VỐN" => "Giá vốn",
                                "TỒN 2 ĐVT" => "Tồn 2 dvt",
                                "TỒN" => "Tồn",
                                "HÃNG SX" => "Hãng sx",
                                "MÃ HÀNG" => "Mã hàng",
                                "MẶT HÀNG" => "Mặt hàng",
                                "ĐVT" => "ĐVT",
                                "GIÁ BÁN" => "Giá bán",
                                "TỒN TỐI THIỂU" => "Tồn tối thiểu",
                                "GIÁ TRỊ VỐN" => "Giá trị vốn",
                                _ => col
                            };
                            ws.Cell(1, colIdx).Value = headerText;
                            colIdx++;
                        }

                        var headerRow = ws.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1B4D5C");
                        headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

                        // 3. Đổ dữ liệu các dòng tương ứng
                        int rowIndex = 2;
                        int stt = 1;
                        foreach (var item in displayList)
                        {
                            ws.Cell(rowIndex, 1).Value = stt;
                            colIdx = 2;

                            foreach (var col in _usedColumns)
                            {
                                string upper = col.Trim().ToUpperInvariant();
                                object cellValue = upper switch
                                {
                                    "MẶT HÀNG" => item.TenMatHang,
                                    "MÃ HÀNG" => item.MaMatHang,
                                    "ĐVT" => item.TenDonViTinh,
                                    "HÃNG SX" => item.HangSx,
                                    "GHI CHÚ" => item.GhiChu,
                                    "TỒN" => item.SoLuongTon,
                                    "TỒN 2 ĐVT" => item.Ton2Dvt,
                                    "GIÁ VỐN" => item.GiaVon,
                                    "GIÁ TRỊ BÁN" => item.SoLuongTon * item.GiaBan,
                                    "GIÁ TRỊ VỐN" => item.SoLuongTon * item.GiaVon,
                                    "GIÁ BÁN" => item.GiaBan,
                                    "TỒN TỐI THIỂU" => item.TonToiThieu,
                                    _ => ""
                                };

                                ws.Cell(rowIndex, colIdx).Value = ClosedXML.Excel.XLCellValue.FromObject(cellValue);
                                colIdx++;
                            }

                            stt++;
                            rowIndex++;
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    // 4. Hiển thị hộp thoại xác nhận mở file vừa xuất
                    var confirmWin = new XacNhanMoFileWindow();
                    confirmWin.Owner = this;
                    if (confirmWin.ShowDialog() == true)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName)
                        {
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file Excel: " + ex.Message, "Lỗi xuất Excel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            if (KeyboardPanel.Visibility == Visibility.Visible)
            {
                KeyboardPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                KeyboardPanel.Visibility = Visibility.Visible;
            }
        }

        private void KbKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string charStr)
            {
                _searchKeyword += charStr;
                ApplyFilters();
            }
        }

        private void KbBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (_searchKeyword.Length > 0)
            {
                _searchKeyword = _searchKeyword.Substring(0, _searchKeyword.Length - 1);
                ApplyFilters();
            }
        }

        private void KbSpace_Click(object sender, RoutedEventArgs e)
        {
            _searchKeyword += " ";
            ApplyFilters();
        }

        private void KbShift_Click(object sender, RoutedEventArgs e)
        {
            // Toggle caps / shift if needed
        }

        private void KbCancel_Click(object sender, RoutedEventArgs e)
        {
            _searchKeyword = "";
            KeyboardPanel.Visibility = Visibility.Collapsed;
            ApplyFilters();
        }

        private async void BtnGearSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("TonKho");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadTileLayoutConfigAsync();
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cài đặt cột hiển thị: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ColumnDisplayInfo
    {
        public string TextToDisplay { get; set; } = "";
    }

    public class BaoCaoTonKhoTileItem
    {
        public bool IsSelected { get; set; }
        public System.Windows.Media.Brush TileBackground => IsSelected ? (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#D35400")! : (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#004D40")!;
        public System.Windows.Media.Brush TileBorderBrush => IsSelected ? (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#F39C12")! : (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#36B5B0")!;

        public string MaMatHang { get; set; } = "";
        public string TenMatHang { get; set; } = "";
        public string MaNhom { get; set; } = "";
        public string MaKho { get; set; } = "";
        public decimal SoLuongTon { get; set; }
        public string TenDonViTinh { get; set; } = "";
        public decimal GiaBan { get; set; }
        public decimal GiaVon { get; set; }
        public string GhiChu { get; set; } = "";
        public string Ton2Dvt { get; set; } = "";
        public string HangSx { get; set; } = "";
        public decimal TonToiThieu { get; set; }

        public List<ColumnDisplayInfo> LeftColumns { get; set; } = new();
        public List<ColumnDisplayInfo> RightColumns { get; set; } = new();

        public void BuildColumns(List<string> usedColumns)
        {
            LeftColumns.Clear();
            RightColumns.Clear();

            foreach (var col in usedColumns)
            {
                string upper = col.Trim().ToUpperInvariant();
                string text = upper switch
                {
                    "MẶT HÀNG" => TenMatHang?.Trim() ?? "",
                    "MÃ HÀNG" => MaMatHang?.Trim() ?? "",
                    "ĐVT" => TenDonViTinh?.Trim() ?? "",
                    "HÃNG SX" => HangSx?.Trim() ?? "",
                    "GHI CHÚ" => GhiChu?.Trim() ?? "",
                    "TỒN" => SoLuongTon.ToString("N0"),
                    "TỒN 2 ĐVT" => Ton2Dvt?.Trim() ?? "",
                    "GIÁ VỐN" => GiaVon != 0 ? GiaVon.ToString("N0") : "",
                    "GIÁ TRỊ BÁN" => (SoLuongTon * GiaBan) != 0 ? (SoLuongTon * GiaBan).ToString("N0") : "",
                    "GIÁ TRỊ VỐN" => (SoLuongTon * GiaVon) != 0 ? (SoLuongTon * GiaVon).ToString("N0") : "",
                    "GIÁ BÁN" => GiaBan != 0 ? GiaBan.ToString("N0") : "",
                    "TỒN TỐI THIỂU" => TonToiThieu != 0 ? TonToiThieu.ToString("N0") : "",
                    _ => ""
                };

                // Nếu thuộc tính đó không có dữ liệu (rỗng/trống) thì hàng đó không cần hiện
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var item = new ColumnDisplayInfo { TextToDisplay = text };
                if (upper == "TỒN")
                {
                    RightColumns.Add(item);
                }
                else
                {
                    LeftColumns.Add(item);
                }
            }
        }
    }
}
