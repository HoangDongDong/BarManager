using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CauHinhHeThong
{
    public partial class ThietLapDinhDangThanhPhanWindow : Window
    {
        private readonly ObservableCollection<DisplayColumnItem> _unusedColumns = new();
        private readonly ObservableCollection<DisplayColumnItem> _usedColumns = new();
        private int _columnCount = 5;
        private int _rowCount = 5;
        private string _numericTarget = "";

        public string TargetType { get; }
        public string SelectedColor { get; private set; } = "#0D4B5B";
        public int SelectedColumnCount => _columnCount;
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Window_Loaded;
            string value = await LocalCauHinhService.GetConfigValueAsync(GetSettingsKey(), "5|5|#0D4B5B");
            string[] parts = value.Split('|');
            if (parts.Length > 0 && int.TryParse(parts[0], out int columns) && columns is >= 1 and <= 99)
            {
                _columnCount = columns;
            }
            if (parts.Length > 1 && int.TryParse(parts[1], out int rows) && rows is >= 1 and <= 99)
            {
                _rowCount = rows;
            }
            if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
            {
                SelectedColor = parts[2];
            }

            BtnColumnCount.Content = $"SỐ CỘT: {_columnCount}";
            BtnRowCount.Content = $"SỐ DÒNG: {_rowCount}";
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
            else
            {
                string name = TargetType switch
                {
                    "KhuVuc" => "Khu vực",
                    "NhomHang" => "Nhóm món",
                    _ => "Mặt hàng"
                };

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
                var leftColumns = _usedColumns.Where(x => !IsRightAlignedColumn(x.Name)).ToList();
                var rightColumns = _usedColumns.Where(x => IsRightAlignedColumn(x.Name)).ToList();

                IcPreviewLeft.ItemsSource = leftColumns;
                IcPreviewRight.ItemsSource = rightColumns;
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
                BtnFormatToggle.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#5B1647")!; // Tím sẫm khi đóng
            }
            else
            {
                PanelFormatOptions.Visibility = Visibility.Visible;
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
            MessageBox.Show("Màu ô được thiết lập từ bảng màu của thành phần đang chọn.", "Màu ô", MessageBoxButton.OK, MessageBoxImage.Information);
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
                _columnCount = value;
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
                $"{_columnCount}|{_rowCount}|{SelectedColor}");
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
