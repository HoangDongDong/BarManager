using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CauHinhHeThong
{
    public partial class ThietLapDinhDangThanhPhanWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly ObservableCollection<DisplayColumnItem> _unusedColumns = new();
        private readonly ObservableCollection<DisplayColumnItem> _usedColumns = new();
        private int _columnCount = 5;
        private int _rowCount = 5;
        private string _selectedColor = "#0D4B5B";
        private string _numericTarget = "";

        public string TargetType { get; }

        public string SelectedColor
        {
            get => _selectedColor;
            private set
            {
                _selectedColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedColorBrush));
                UpdateLivePreview();
            }
        }

        public Brush SelectedColorBrush
        {
            get
            {
                try
                {
                    return (Brush)new BrushConverter().ConvertFromString(SelectedColor)!;
                }
                catch
                {
                    return new SolidColorBrush(Color.FromRgb(0x00, 0x4D, 0x40));
                }
            }
        }

        public int SelectedColumnCount
        {
            get => _columnCount;
            private set
            {
                _columnCount = value;
                OnPropertyChanged();
            }
        }

        public int SelectedRowCount => _rowCount;
        public ObservableCollection<DisplayColumnItem> SelectedColumns => _usedColumns;

        public ThietLapDinhDangThanhPhanWindow(string targetType)
        {
            InitializeComponent();
            TargetType = targetType;
            LstUnused.ItemsSource = _unusedColumns;
            LstUsed.ItemsSource = _usedColumns;
            Loaded += Window_Loaded;
        }

        private bool _showTitle = true;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Window_Loaded;
            string defaultVal = TargetType switch
            {
                "MatHang" => "4|4|#0D4B5B|1",
                "Nhom" or "NhomHang" => "2|2|#0D4B5B|1",
                "GioHang" or "Cart" or "HoaDon" => "1|10|#0D4B5B|1",
                _ => "5|5|#0D4B5B|1"
            };

            string value = await LocalCauHinhService.GetConfigValueAsync(GetSettingsKey(), defaultVal);
            string[] parts = value.Split('|');
            if (parts.Length > 0 && int.TryParse(parts[0], out int columns) && columns is >= 1 and <= 99)
            {
                SelectedColumnCount = columns;
            }
            if (parts.Length > 1 && int.TryParse(parts[1], out int rows) && rows is >= 1 and <= 99)
            {
                _rowCount = rows;
            }
            if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
            {
                SelectedColor = parts[2];
            }
            if (parts.Length > 3 && int.TryParse(parts[3], out int showTitle))
            {
                _showTitle = showTitle == 1;
            }

            BtnColumnCount.Content = $"SỐ CỘT: {_columnCount}";
            BtnRowCount.Content = $"SỐ DÒNG: {_rowCount}";
            BtnShowTitle.Content = _showTitle ? "HIỆN TIÊU ĐỀ: ON" : "HIỆN TIÊU ĐỀ: OFF";
            try
            {
                BtnCellColor.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(SelectedColor)!;
            }
            catch { }
            BtnFormatToggle.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#5B1647")!;

            await LoadColumnsAsync();
        }

        private string GetSettingsKey() => $"TOUCH_LAYOUT_{TargetType}";
        private string GetColumnsKey() => $"TOUCH_COLUMNS_{TargetType}";

        private void PopulateColumns(List<string> allColumns, List<string> defaultUsedColNames, string savedColsStr)
        {
            List<string> rawUsedList = !string.IsNullOrWhiteSpace(savedColsStr)
                ? savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : defaultUsedColNames;

            foreach (var raw in rawUsedList)
            {
                var item = DisplayColumnItem.Parse(raw);
                if (allColumns.Contains(item.Name))
                {
                    _usedColumns.Add(item);
                }
            }

            foreach (var col in allColumns)
            {
                if (!_usedColumns.Any(x => x.Name == col))
                {
                    _unusedColumns.Add(new DisplayColumnItem(col));
                }
            }
        }

        private async Task LoadColumnsAsync()
        {
            _unusedColumns.Clear();
            _usedColumns.Clear();

            string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");

            if (TargetType == "TonKho")
            {
                List<string> allColumns = new()
                {
                    "GHI CHÚ", "GIÁ TRỊ BÁN", "GIÁ VỐN", "TỒN 2 ĐVT", "TỒN", "HÃNG SX",
                    "GIÁ BÁN", "TỒN TỐI THIỂU", "GIÁ TRỊ VỐN", "ĐVT", "MẶT HÀNG", "MÃ HÀNG"
                };
                List<string> defaults = new() { "GHI CHÚ", "GIÁ TRỊ BÁN", "GIÁ VỐN", "TỒN 2 ĐVT", "TỒN", "HÃNG SX" };
                PopulateColumns(allColumns, defaults, savedColsStr);
            }
            else if (TargetType == "ChiTietTonKho")
            {
                List<string> allColumns = new()
                {
                    "ĐƠN GIÁ", "THÀNH TIỀN", "ĐVT", "SỐ LƯỢNG XUẤT", "SỐ LƯỢNG NHẬP",
                    "SỐ PHIẾU", "ĐỐI TƯỢNG", "NGÀY", "GIẢM GIÁ %", "DIỄN GIẢI"
                };
                PopulateColumns(allColumns, allColumns, savedColsStr);
            }
            else if (TargetType == "GioHang" || TargetType == "Cart" || TargetType == "HoaDon")
            {
                List<string> allColumns = new()
                {
                    "SỐ LƯỢNG", "ĐẾN GIỜ", "TỪ GIỜ", "ĐƠN GIÁ", "TÊN HÀNG", "GIẢM GIÁ %", "THÀNH TIỀN", "GHI CHÚ"
                };
                List<string> defaults = new() { "ĐẾN GIỜ", "TỪ GIỜ", "ĐƠN GIÁ", "TÊN HÀNG", "GIẢM GIÁ %", "THÀNH TIỀN", "GHI CHÚ" };
                PopulateColumns(allColumns, defaults, savedColsStr);
            }
            else if (TargetType == "HoaDonChuaThanhToan")
            {
                List<string> allColumns = new()
                {
                    "BÀN", "TỔNG CỘNG", "NHÂN VIÊN", "NGÀY", "KHÁCH HÀNG", "SỐ PHIẾU"
                };
                PopulateColumns(allColumns, allColumns, savedColsStr);
            }
            else if (TargetType == "MatHang")
            {
                List<string> allColumns = new() { "MẶT HÀNG", "GIÁ BÁN" };
                PopulateColumns(allColumns, allColumns, savedColsStr);
            }
            else if (TargetType == "Nhom" || TargetType == "NhomHang")
            {
                PopulateColumns(new List<string> { "NHÓM" }, new List<string> { "NHÓM" }, savedColsStr);
            }
            else if (TargetType == "KhuVuc")
            {
                PopulateColumns(new List<string> { "KHU VỰC" }, new List<string> { "KHU VỰC" }, savedColsStr);
            }
            else if (TargetType == "Ban")
            {
                List<string> allColumns = new()
                {
                    "BÀN", "GIỜ VÀO", "SỐ TIỀN", "KHÁCH HÀNG"
                };
                PopulateColumns(allColumns, new List<string> { "BÀN" }, savedColsStr);
            }
            else if (TargetType == "QuyenSuDung" || TargetType == "FuncRoles")
            {
                List<string> allColumns = new()
                {
                    "TÊN CHỨC NĂNG", "KHÓA", "XEM", "THÊM", "SỬA", "XÓA", "TẤT CẢ"
                };
                PopulateColumns(allColumns, allColumns, savedColsStr);
            }
            else if (TargetType == "Users" || TargetType == "NguoiDung")
            {
                List<string> allColumns = new()
                {
                    "TÀI KHOẢN", "HỌ TÊN", "NHÓM NGƯỜI DÙNG"
                };
                PopulateColumns(allColumns, allColumns, savedColsStr);
            }
            else if (TargetType == "QuyenBaoCao" || TargetType == "BaoCao" || TargetType == "ReportRoles")
            {
            }
            else
            {
                string name = TargetType.ToUpperInvariant();
                PopulateColumns(new List<string> { name }, new List<string> { name }, savedColsStr);
            }

            UpdateLivePreview();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (LstUnused.SelectedItem is DisplayColumnItem column)
            {
                _unusedColumns.Remove(column);
                _usedColumns.Add(column);
                UpdateLivePreview();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem column)
            {
                _usedColumns.Remove(column);
                _unusedColumns.Add(column);
                UpdateLivePreview();
            }
        }

        private void BtnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            while (_usedColumns.Count > 0)
            {
                var column = _usedColumns[0];
                _usedColumns.RemoveAt(0);
                _unusedColumns.Add(column);
            }
            UpdateLivePreview();
        }

        private bool _isUpdatingFormatUI = false;
        private bool _isPickingTextColor = false;

        private void LstUsed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFormatUIForSelectedColumn();
            UpdateLivePreview();
        }

        private void UpdateFormatUIForSelectedColumn()
        {
            if (LstUsed.SelectedItem is not DisplayColumnItem selectedCol) return;

            _isUpdatingFormatUI = true;
            try
            {
                BtnAlignLeft.Background = selectedCol.Alignment == HorizontalAlignment.Left ? GetBrush("#C95B16") : GetBrush("#6C757D");
                BtnAlignRight.Background = selectedCol.Alignment == HorizontalAlignment.Right ? GetBrush("#C95B16") : GetBrush("#6C757D");

                BtnBold.Background = selectedCol.IsBold ? GetBrush("#C95B16") : GetBrush("#6C757D");
                BtnItalic.Background = selectedCol.IsItalic ? GetBrush("#C95B16") : GetBrush("#6C757D");

                BtnFontSize.Content = $"CỠ CHỮ: {selectedCol.FontSize}";
                BtnTextColor.Background = GetBrush(string.IsNullOrWhiteSpace(selectedCol.TextColorHex) ? "#FFFFFF" : selectedCol.TextColorHex);

                TxtShortTitle.Text = selectedCol.ShortTitle;
            }
            finally
            {
                _isUpdatingFormatUI = false;
            }
        }

        private Brush GetBrush(string colorHex)
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFromString(colorHex)!;
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }

        private void BtnAlignLeft_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
            {
                selectedCol.Alignment = HorizontalAlignment.Left;
                UpdateFormatUIForSelectedColumn();
                UpdateLivePreview();
            }
        }

        private void BtnAlignRight_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
            {
                selectedCol.Alignment = HorizontalAlignment.Right;
                UpdateFormatUIForSelectedColumn();
                UpdateLivePreview();
            }
        }

        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
            {
                selectedCol.IsBold = !selectedCol.IsBold;
                UpdateFormatUIForSelectedColumn();
                UpdateLivePreview();
            }
        }

        private void BtnItalic_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
            {
                selectedCol.IsItalic = !selectedCol.IsItalic;
                UpdateFormatUIForSelectedColumn();
                UpdateLivePreview();
            }
        }

        private void BtnFontSize_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
            {
                OpenNumericPad("Cỡ chữ", selectedCol.FontSize);
            }
        }

        private void BtnTextColor_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem)
            {
                _isPickingTextColor = true;
                ColorPaletteOverlay.Visibility = Visibility.Visible;
            }
        }

        private void TxtShortTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFormatUI) return;
            if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
            {
                selectedCol.ShortTitle = TxtShortTitle.Text;
                UpdateLivePreview();
            }
        }

        private void UpdateLivePreview()
        {
            try
            {
                if (TargetType == "HoaDonChuaThanhToan")
                {
                    if (CardPreviewBorder != null) CardPreviewBorder.Visibility = Visibility.Visible;

                    var usedNames = _usedColumns.Select(x => x.Name).ToList();

                    bool showBan = usedNames.Contains("BÀN");
                    bool showTongCong = usedNames.Contains("TỔNG CỘNG");
                    bool showNhanVien = usedNames.Contains("NHÂN VIÊN");
                    bool showNgay = usedNames.Contains("NGÀY");
                    bool showKhachHang = usedNames.Contains("KHÁCH HÀNG");
                    bool showSoPhieu = usedNames.Contains("SỐ PHIẾU");

                    if (TxtPreviewTongCong != null)
                    {
                        TxtPreviewTongCong.Visibility = showTongCong ? Visibility.Visible : Visibility.Collapsed;
                    }

                    if (StkPreviewLeft != null)
                    {
                        StkPreviewLeft.Children.Clear();
                        foreach (var col in _usedColumns)
                        {
                            switch (col.Name)
                            {
                                case "BÀN":
                                    if (TxtPreviewBan != null)
                                    {
                                        TxtPreviewBan.Visibility = Visibility.Visible;
                                        StkPreviewLeft.Children.Add(TxtPreviewBan);
                                    }
                                    break;
                                case "NGÀY":
                                    if (TxtPreviewNgay != null)
                                    {
                                        TxtPreviewNgay.Visibility = Visibility.Visible;
                                        StkPreviewLeft.Children.Add(TxtPreviewNgay);
                                    }
                                    break;
                                case "NHÂN VIÊN":
                                    if (TxtPreviewNhanVien != null)
                                    {
                                        TxtPreviewNhanVien.Visibility = Visibility.Visible;
                                        StkPreviewLeft.Children.Add(TxtPreviewNhanVien);
                                    }
                                    break;
                                case "KHÁCH HÀNG":
                                    if (TxtPreviewKhachHang != null)
                                    {
                                        TxtPreviewKhachHang.Visibility = Visibility.Visible;
                                        StkPreviewLeft.Children.Add(TxtPreviewKhachHang);
                                    }
                                    break;
                                case "SỐ PHIẾU":
                                    if (TxtPreviewSoPhieu != null)
                                    {
                                        TxtPreviewSoPhieu.Visibility = Visibility.Visible;
                                        StkPreviewLeft.Children.Add(TxtPreviewSoPhieu);
                                    }
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    if (CardPreviewBorder != null) CardPreviewBorder.Visibility = Visibility.Collapsed;
                }

                if (StkPreviewItems != null)
                {
                    StkPreviewItems.Children.Clear();
                    if (_usedColumns.Count == 0)
                    {
                        var tb = new TextBlock
                        {
                            Text = TargetType == "Ban" ? "Bàn 01" : TargetType,
                            Foreground = Brushes.White,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        };
                        StkPreviewItems.Children.Add(tb);
                    }
                    else
                    {
                        foreach (var col in _usedColumns)
                        {
                            string textToDisplay = !string.IsNullOrWhiteSpace(col.ShortTitle)
                                ? col.ShortTitle
                                : GetSampleValueForColumn(col.Name, TargetType);

                            var tb = new TextBlock
                            {
                                Text = textToDisplay,
                                HorizontalAlignment = col.Alignment,
                                TextAlignment = col.Alignment == HorizontalAlignment.Right ? TextAlignment.Right : TextAlignment.Left,
                                FontSize = col.FontSize > 0 ? col.FontSize : 14,
                                FontWeight = col.IsBold ? FontWeights.Bold : FontWeights.Normal,
                                FontStyle = col.IsItalic ? FontStyles.Italic : FontStyles.Normal,
                                Foreground = GetBrush(string.IsNullOrWhiteSpace(col.TextColorHex) ? "#FFFFFF" : col.TextColorHex),
                                Margin = new Thickness(0, 1, 0, 1)
                            };
                            StkPreviewItems.Children.Add(tb);
                        }
                    }
                }
            }
            catch { }
        }

        private string GetSampleValueForColumn(string colName, string targetType)
        {
            string upper = colName.Trim().ToUpperInvariant();
            return upper switch
            {
                "BÀN" => "Bàn 01",
                "GIỜ VÀO" => "Vào: 14:30",
                "KHÁCH HÀNG" => "Khách A",
                "SỐ TIỀN" => "150,000đ",
                "MẶT HÀNG" or "TÊN HÀNG" => "Bia Heineken",
                "GIÁ BÁN" or "ĐƠN GIÁ" => "25,000đ",
                "THÀNH TIỀN" or "GIÁ TRỊ BÁN" or "GIÁ TRỊ VỐN" or "TỔNG CỘNG" => "150,000đ",
                "SỐ LƯỢNG" or "TỒN" => "10",
                "ĐVT" => "Chai",
                "MÃ HÀNG" => "MH001",
                "NGÀY" => "14/09/2026",
                "SỐ PHIẾU" => "HD00001",
                "NHÂN VIÊN" => "NV Admin",
                "GHI CHÚ" or "DIỄN GIẢI" => "Ghi chú mẫu",
                "GIÁ VỐN" => "18,000đ",
                "GIẢM GIÁ %" => "10%",
                "TỪ GIỜ" => "14:00",
                "ĐẾN GIỜ" => "15:00",
                "HÃNG SX" => "Heineken",
                "KHU VỰC" => "Tầng 1",
                "NHÓM" => "Đồ uống",
                _ => colName
            };
        }

        private bool IsRightAlignedColumn(string colName)
        {
            if (string.IsNullOrEmpty(colName)) return false;
            string upper = colName.Trim().ToUpperInvariant();
            return upper == "TỒN";
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem column)
            {
                int index = _usedColumns.IndexOf(column);
                if (index > 0)
                {
                    _usedColumns.Move(index, index - 1);
                    UpdateLivePreview();
                }
            }
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (LstUsed.SelectedItem is DisplayColumnItem column)
            {
                int index = _usedColumns.IndexOf(column);
                if (index >= 0 && index < _usedColumns.Count - 1)
                {
                    _usedColumns.Move(index, index + 1);
                    UpdateLivePreview();
                }
            }
        }

        private void BtnFormat_Click(object sender, RoutedEventArgs e)
        {
            if (PanelFormatOptions.Visibility == Visibility.Visible)
            {
                PanelFormatOptions.Visibility = Visibility.Collapsed;
                PanelFormatSubOptions.Visibility = Visibility.Collapsed;
                BtnFormatToggle.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#5B1647")!; // Tím sẫm khi đóng
            }
            else
            {
                PanelFormatOptions.Visibility = Visibility.Visible;
                PanelFormatSubOptions.Visibility = Visibility.Visible;
                BtnFormatToggle.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#C95B16")!; // Cam rực khi mở
                UpdateFormatUIForSelectedColumn();
            }
        }

        private void BtnColumnCount_Click(object sender, RoutedEventArgs e)
        {
            OpenNumericPad("Số cột", _columnCount);
        }

        private void BtnRowCount_Click(object sender, RoutedEventArgs e)
        {
            OpenNumericPad("Số dòng", _rowCount);
        }

        private void BtnCellColor_Click(object sender, RoutedEventArgs e)
        {
            _isPickingTextColor = false;
            ColorPaletteOverlay.Visibility = Visibility.Visible;
        }

        private void ColorTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string colorHex)
            {
                if (_isPickingTextColor)
                {
                    if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
                    {
                        selectedCol.TextColorHex = colorHex;
                        BtnTextColor.Background = GetBrush(colorHex);
                    }
                    _isPickingTextColor = false;
                }
                else
                {
                    SelectedColor = colorHex;
                    try
                    {
                        BtnCellColor.Background = GetBrush(colorHex);
                    }
                    catch { }
                }
                ColorPaletteOverlay.Visibility = Visibility.Collapsed;
                UpdateLivePreview();
            }
        }

        private void BtnColorCancel_Click(object sender, RoutedEventArgs e)
        {
            _isPickingTextColor = false;
            ColorPaletteOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnShowTitle_Click(object sender, RoutedEventArgs e)
        {
            _showTitle = !_showTitle;
            BtnShowTitle.Content = _showTitle ? "HIỆN TIÊU ĐỀ: ON" : "HIỆN TIÊU ĐỀ: OFF";
            try
            {
                BtnShowTitle.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(_showTitle ? "#5C5C00" : "#505050")!;
            }
            catch { }
        }

        private void OpenNumericPad(string target, int value)
        {
            _numericTarget = target;
            TxtNumericPadTitle.Text = $"NHẬP {target.ToUpperInvariant()} HIỂN THỊ TRÊN LƯỚI";
            TxtNumericValue.Text = value.ToString();
            NumericPadOverlay.Visibility = Visibility.Visible;
            TxtNumericValue.Focus();
        }

        private void NumericKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string key = button.Tag?.ToString() ?? "";
                TxtNumericValue.Text = TxtNumericValue.Text == "0" ? key : TxtNumericValue.Text + key;
            }
        }

        private void BtnNumericBackspace_Click(object sender, RoutedEventArgs e)
        {
            TxtNumericValue.Text = TxtNumericValue.Text.Length > 1
                ? TxtNumericValue.Text[..^1]
                : "0";
        }

        private void BtnNumericConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtNumericValue.Text, out int value) || value < 1 || value > 99)
            {
                MessageBox.Show("Giá trị phải nằm trong khoảng từ 1 đến 99.", "Giá trị không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_numericTarget == "Số cột")
            {
                SelectedColumnCount = value;
                BtnColumnCount.Content = $"SỐ CỘT: {value}";
            }
            else if (_numericTarget == "Cỡ chữ")
            {
                if (LstUsed.SelectedItem is DisplayColumnItem selectedCol)
                {
                    selectedCol.FontSize = value;
                    BtnFontSize.Content = $"CỠ CHỮ: {value}";
                    UpdateLivePreview();
                }
            }
            else
            {
                _rowCount = value;
                BtnRowCount.Content = $"SỐ DÒNG: {value}";
            }

            NumericPadOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnNumericCancel_Click(object sender, RoutedEventArgs e)
        {
            NumericPadOverlay.Visibility = Visibility.Collapsed;
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            string usedColsStr = string.Join(",", _usedColumns.Select(x => x.ToStringConfig()));
            bool saved1 = await LocalCauHinhService.SaveSingleConfigAsync(
                GetSettingsKey(),
                $"{_columnCount}|{_rowCount}|{SelectedColor}|{(_showTitle ? 1 : 0)}");
            bool saved2 = await LocalCauHinhService.SaveSingleConfigAsync(
                GetColumnsKey(),
                usedColsStr);

            if (!saved1 || !saved2)
            {
                MessageBox.Show("Không thể lưu cấu hình vào cơ sở dữ liệu.", "Lỗi lưu cấu hình", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public sealed class DisplayColumnItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string? prop = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }

            private string _name = "";
            private HorizontalAlignment _alignment = HorizontalAlignment.Left;
            private bool _isBold = true;
            private bool _isItalic = false;
            private int _fontSize = 14;
            private string _textColorHex = "#FFFFFF";
            private string _shortTitle = "";

            public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            public string DisplayName => Name switch
            {
                "BÀN" => "Bàn",
                "GIỜ VÀO" => "Giờ vào",
                "SỐ TIỀN" => "Số tiền",
                "KHÁCH HÀNG" => "Khách hàng",
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
                _ => Name
            };

            public HorizontalAlignment Alignment { get => _alignment; set { _alignment = value; OnPropertyChanged(); } }
            public bool IsBold { get => _isBold; set { _isBold = value; OnPropertyChanged(); } }
            public bool IsItalic { get => _isItalic; set { _isItalic = value; OnPropertyChanged(); } }
            public int FontSize { get => _fontSize; set { _fontSize = value; OnPropertyChanged(); } }
            public string TextColorHex { get => _textColorHex; set { _textColorHex = value; OnPropertyChanged(); } }
            public string ShortTitle { get => _shortTitle; set { _shortTitle = value; OnPropertyChanged(); } }

            public DisplayColumnItem(string name)
            {
                Name = name;
            }

            public string ToStringConfig()
            {
                string alignStr = Alignment == HorizontalAlignment.Right ? "R" : "L";
                string boldStr = IsBold ? "1" : "0";
                string italicStr = IsItalic ? "1" : "0";
                return $"{Name};{alignStr};{boldStr};{italicStr};{FontSize};{TextColorHex};{ShortTitle}";
            }

            public static DisplayColumnItem Parse(string colStr)
            {
                if (string.IsNullOrWhiteSpace(colStr)) return new DisplayColumnItem("");
                string[] parts = colStr.Split(';');
                var item = new DisplayColumnItem(parts[0].Trim());
                if (parts.Length > 1) item.Alignment = parts[1].Trim().ToUpperInvariant() == "R" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                if (parts.Length > 2) item.IsBold = parts[2].Trim() == "1";
                if (parts.Length > 3) item.IsItalic = parts[3].Trim() == "1";
                if (parts.Length > 4 && int.TryParse(parts[4].Trim(), out int fs)) item.FontSize = fs;
                if (parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5])) item.TextColorHex = parts[5].Trim();
                if (parts.Length > 6) item.ShortTitle = parts[6].Trim();
                return item;
            }
        }
    }
}
