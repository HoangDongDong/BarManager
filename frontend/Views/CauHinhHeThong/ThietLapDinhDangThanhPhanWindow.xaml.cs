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

        private async Task LoadColumnsAsync()
        {
            _unusedColumns.Clear();
            _usedColumns.Clear();

            if (TargetType == "TonKho")
            {
                List<string> allColumns = new()
                {
                    "GHI CHÚ", "GIÁ TRỊ BÁN", "GIÁ VỐN", "TỒN 2 ĐVT", "TỒN", "HÃNG SX",
                    "GIÁ BÁN", "TỒN TỐI THIỂU", "GIÁ TRỊ VỐN", "ĐVT", "MẶT HÀNG", "MÃ HÀNG"
                };

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames;
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    usedColNames = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    usedColNames = new List<string> { "GHI CHÚ", "GIÁ TRỊ BÁN", "GIÁ VỐN", "TỒN 2 ĐVT", "TỒN", "HÃNG SX" };
                }

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "ChiTietTonKho")
            {
                List<string> allColumns = new()
                {
                    "ĐƠN GIÁ", "THÀNH TIỀN", "ĐVT", "SỐ LƯỢNG XUẤT", "SỐ LƯỢNG NHẬP",
                    "SỐ PHIẾU", "ĐỐI TƯỢNG", "NGÀY", "GIẢM GIÁ %", "DIỄN GIẢI"
                };

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames;
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    usedColNames = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    usedColNames = new List<string>
                    {
                        "ĐƠN GIÁ", "THÀNH TIỀN", "ĐVT", "SỐ LƯỢNG XUẤT", "SỐ LƯỢNG NHẬP",
                        "SỐ PHIẾU", "ĐỐI TƯỢNG", "NGÀY", "GIẢM GIÁ %", "DIỄN GIẢI"
                    };
                }

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "GioHang" || TargetType == "Cart" || TargetType == "HoaDon")
            {
                List<string> allColumns = new()
                {
                    "SỐ LƯỢNG", "ĐẾN GIỜ", "TỪ GIỜ", "ĐƠN GIÁ", "TÊN HÀNG", "GIẢM GIÁ %", "THÀNH TIỀN", "GHI CHÚ"
                };

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames;
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    usedColNames = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    // Default matching reference screenshot 3
                    usedColNames = new List<string>
                    {
                        "ĐẾN GIỜ", "TỪ GIỜ", "ĐƠN GIÁ", "TÊN HÀNG", "GIẢM GIÁ %", "THÀNH TIỀN", "GHI CHÚ"
                    };
                }

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "HoaDonChuaThanhToan")
            {
                List<string> allColumns = new()
                {
                    "BÀN", "TỔNG CỘNG", "NHÂN VIÊN", "NGÀY", "KHÁCH HÀNG", "SỐ PHIẾU"
                };

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames;
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    usedColNames = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    usedColNames = new List<string>
                    {
                        "BÀN", "TỔNG CỘNG", "NHÂN VIÊN", "NGÀY", "KHÁCH HÀNG", "SỐ PHIẾU"
                    };
                }

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "MatHang")
            {
                List<string> allColumns = new() { "MẶT HÀNG", "GIÁ BÁN" };
                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames = !string.IsNullOrWhiteSpace(savedColsStr)
                    ? savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string> { "MẶT HÀNG", "GIÁ BÁN" };

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "Nhom" || TargetType == "NhomHang")
            {
                _usedColumns.Add(new DisplayColumnItem("NHÓM"));
            }
            else if (TargetType == "KhuVuc")
            {
                _usedColumns.Add(new DisplayColumnItem("KHU VỰC"));
            }
            else if (TargetType == "Ban")
            {
                _usedColumns.Add(new DisplayColumnItem("BÀN"));
            }
            else if (TargetType == "QuyenSuDung" || TargetType == "FuncRoles")
            {
                List<string> allColumns = new()
                {
                    "TÊN CHỨC NĂNG", "KHÓA", "XEM", "THÊM", "SỬA", "XÓA", "TẤT CẢ"
                };
                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames = !string.IsNullOrWhiteSpace(savedColsStr)
                    ? savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string> { "TÊN CHỨC NĂNG", "KHÓA", "XEM", "THÊM", "SỬA", "XÓA", "TẤT CẢ" };

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "Users" || TargetType == "NguoiDung")
            {
                List<string> allColumns = new()
                {
                    "TÀI KHOẢN", "HỌ TÊN", "NHÓM NGƯỜI DÙNG"
                };
                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync(GetColumnsKey(), "");
                List<string> usedColNames = !string.IsNullOrWhiteSpace(savedColsStr)
                    ? savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string> { "TÀI KHOẢN", "HỌ TÊN", "NHÓM NGƯỜI DÙNG" };

                foreach (var col in usedColNames)
                {
                    if (allColumns.Contains(col))
                    {
                        _usedColumns.Add(new DisplayColumnItem(col));
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
            else if (TargetType == "QuyenBaoCao" || TargetType == "BaoCao" || TargetType == "ReportRoles")
            {
                // Riêng phần báo cáo không hiện cột nào
            }
            else
            {
                string name = TargetType.ToUpperInvariant();
                _usedColumns.Add(new DisplayColumnItem(name));
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

        private void LstUsed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLivePreview();
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
            }
            catch { }
        }

        private bool IsRightAlignedColumn(string colName)
        {
            if (string.IsNullOrEmpty(colName)) return false;
            string upper = colName.Trim().ToUpperInvariant();
            // Tồn để bên phải, Hãng & các thuộc tính khác (Ghi chú, Giá trị bán, Giá vốn, Tồn 2 dvt...) để bên trái
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
            ColorPaletteOverlay.Visibility = Visibility.Visible;
        }

        private void ColorTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string colorHex)
            {
                SelectedColor = colorHex;
                try
                {
                    BtnCellColor.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(colorHex)!;
                }
                catch { }
                ColorPaletteOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnColorCancel_Click(object sender, RoutedEventArgs e)
        {
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
            string usedColsStr = string.Join(",", _usedColumns.Select(x => x.Name));
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

        public sealed class DisplayColumnItem
        {
            public string Name { get; }
            public string DisplayName => Name switch
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
                _ => Name
            };
            public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
            public bool IsBold { get; set; } = true;
            public bool IsItalic { get; set; } = false;

            public DisplayColumnItem(string name)
            {
                Name = name;
            }
        }
    }
}
