using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CauHinhHeThong
{
    public partial class ThietKeGiaoDienCamUngControl : UserControl
    {
        private object? _selectedItem;
        private string _selectedType = ""; // "KhuVuc", "NhomHang", "MatHang"
        private byte[]? _selectedImageBytes;
        private List<TouchLayoutItem> _allMatHang = new();

        private readonly Dictionary<string, (int Columns, int Rows)> _gridSettings = new()
        {
            ["KhuVuc"] = (1, 5),
            ["NhomHang"] = (1, 5),
            ["MatHang"] = (5, 5)
        };

        public ThietKeGiaoDienCamUngControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LocalCauHinhService.RefreshConfigCacheAsync();
            LoadColors();
            await LoadLayoutSettingsAsync();
            LoadData();
        }

        private async System.Threading.Tasks.Task LoadLayoutSettingsAsync()
        {
            foreach (string targetType in new[] { "KhuVuc", "NhomHang", "MatHang" })
            {
                string value = await LocalCauHinhService.GetConfigValueAsync($"TOUCH_LAYOUT_{targetType}", "");
                string[] parts = value.Split('|');
                int columns = targetType == "MatHang" ? 5 : 1;
                int rows = 5;

                if (parts.Length > 0 && int.TryParse(parts[0], out int parsedCols) && parsedCols is >= 1 and <= 99)
                {
                    columns = parsedCols;
                }
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedRows) && parsedRows is >= 1 and <= 99)
                {
                    rows = parsedRows;
                }

                ApplyGridSettings(targetType, columns, rows);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemDimensions();
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemDimensions();
        }

        private void UpdateItemDimensions()
        {
            UpdateSectionDimensions("KhuVuc", SvKhuVuc, IcKhuVuc, 6);
            UpdateSectionDimensions("NhomHang", SvNhomHang, IcNhomHang, 6);
            UpdateSectionDimensions("MatHang", SvMatHang, IcMatHang, 6);
        }

        private void UpdateSectionDimensions(string targetType, ScrollViewer sv, ItemsControl ic, double marginPerItem)
        {
            if (sv == null || ic == null || ic.ItemsSource is not IEnumerable<TouchLayoutItem> items)
                return;

            var (columns, rows) = _gridSettings.TryGetValue(targetType, out var setting) ? setting : (1, 5);
            rows = Math.Max(1, rows);

            double availableHeight = sv.ActualHeight;
            if (availableHeight <= 0)
                return;

            double targetHeight = Math.Max(32, (availableHeight / rows) - marginPerItem);
            foreach (var item in items)
            {
                item.ItemHeight = targetHeight;
            }
        }

        private async void LoadData()
        {
            try
            {
                await LocalCauHinhService.RefreshConfigCacheAsync();
                string kvDefaultColor = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_KhuVuc", "#3D8EAA");
                string nhomDefaultColor = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_NhomHang", "#4E78A6");
                string mhDefaultColor = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_MatHang", "#5B7F95");

                // Load Khu Vực
                var khuVucList = LocalDatabaseService.GetAll<TouchLayoutItem>("SELECT CAST(T.ID AS VARCHAR(50)) AS ID, T.NAME, T.STATUS, T.ANH, T.MAUSAC FROM DKHUVUC T WHERE (T.STATUS IS NULL OR T.STATUS <> 0) ORDER BY T.SORTORDER, T.NAME");
                foreach (var item in khuVucList) item.MauNen = GetColor(item.Mausac, kvDefaultColor);
                IcKhuVuc.ItemsSource = khuVucList;

                // Load Nhóm Hàng  
                var nhomList = LocalDatabaseService.GetAll<TouchLayoutItem>("SELECT CAST(T.ID AS VARCHAR(50)) AS ID, T.NAME, T.STATUS, T.ANH, T.MAUSAC FROM DNHOMMATHANG T WHERE (T.STATUS IS NULL OR T.STATUS <> 0) ORDER BY T.SORTORDER, T.NAME");
                foreach (var item in nhomList) item.MauNen = GetColor(item.Mausac, nhomDefaultColor);
                IcNhomHang.ItemsSource = nhomList;

                // Load Mặt Hàng
                _allMatHang = LocalDatabaseService.GetAll<TouchLayoutItem>("SELECT CAST(T.ID AS VARCHAR(50)) AS ID, T.NAME, T.STATUS, T.ANH, T.MAUSAC, CAST(T.DNHOMMATHANGID AS VARCHAR(50)) AS DNHOMMATHANGID FROM DMATHANG T WHERE (T.STATUS IS NULL OR T.STATUS <> 0) ORDER BY T.NAME").ToList();
                foreach (var item in _allMatHang) 
                {
                    item.MauNen = GetColor(item.Mausac, mhDefaultColor);
                }
                IcMatHang.ItemsSource = _allMatHang;

                UpdateItemDimensions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetColor(string? color, string fallback)
        {
            return ColorUtils.ColorIntToHex(color, fallback);
        }

        private void LoadColors()
        {
            var colors = new List<string>
            {
                "#F44336","#E91E63","#9C27B0","#673AB7","#3F51B5","#2196F3",
                "#03A9F4","#00BCD4","#009688","#4CAF50","#8BC34A","#CDDC39",
                "#FFEB3B","#FFC107","#FF9800","#FF5722","#795548","#9E9E9E",
                "#607D8B","#263238","#FFFFFF","#1769AA","#145DA0","#2C8A72",
                "#B6423A","#D98900","#C96B35","#F29F05","#557A45","#236B73"
            };
            IcColorMatrix.ItemsSource = colors;
        }

        private void Item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                ClearSelection();
                _selectedItem = border.DataContext;
                _selectedType = border.Tag?.ToString() ?? "MatHang";
                if (_selectedItem is TouchLayoutItem selected)
                {
                    selected.IsSelected = true;
                }
                if (_selectedType == "NhomHang" && _selectedItem is TouchLayoutItem group)
                {
                    IcMatHang.ItemsSource = _allMatHang
                        .Where(item => string.Equals(item.DnhommathangId, group.Id, StringComparison.OrdinalIgnoreCase));
                }
                else if (_selectedType == "KhuVuc")
                {
                    IcMatHang.ItemsSource = _allMatHang;
                }
                _selectedImageBytes = (_selectedItem as TouchLayoutItem)?.Anh;
                ImgItemPreview.Source = LocalThuVienAnhService.BytesToBitmapImage(_selectedImageBytes);
                BorderColorPalette.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearSelection()
        {
            foreach (var source in new[] { IcKhuVuc.ItemsSource, IcNhomHang.ItemsSource, IcMatHang.ItemsSource })
            {
                if (source is IEnumerable<TouchLayoutItem> items)
                {
                    foreach (var item in items)
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        private void ColorTile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && _selectedItem != null)
            {
                var colorStr = border.DataContext?.ToString();
                if (colorStr != null)
                {
                    ApplyColor(_selectedType, colorStr);
                }
                BorderColorPalette.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSettingKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            OpenFormattingWindow("KhuVuc");
        }

        private void BtnSettingNhomHang_Click(object sender, RoutedEventArgs e)
        {
            OpenFormattingWindow("NhomHang");
        }

        private void BtnSettingMatHang_Click(object sender, RoutedEventArgs e)
        {
            OpenFormattingWindow("MatHang");
        }

        private void OpenFormattingWindow(string targetType)
        {
            var win = new ThietLapDinhDangThanhPhanWindow(targetType)
            {
                Owner = Window.GetWindow(this)
            };

            if (win.ShowDialog() == true)
            {
                ApplyGridSettings(targetType, win.SelectedColumnCount, win.SelectedRowCount);
                ReloadDesignerView();
            }
        }

        private void ApplyGridSettings(string targetType, int columns, int rows)
        {
            _gridSettings[targetType] = (Math.Max(1, columns), Math.Max(1, rows));

            ItemsControl target = targetType switch
            {
                "KhuVuc" => IcKhuVuc,
                "NhomHang" => IcNhomHang,
                _ => IcMatHang
            };

            var panelFactory = new FrameworkElementFactory(typeof(UniformGrid));
            panelFactory.SetValue(UniformGrid.ColumnsProperty, Math.Max(1, columns));
            target.ItemsPanel = new ItemsPanelTemplate(panelFactory);

            UpdateItemDimensions();
        }

        private void ReloadDesignerView()
        {
            _selectedItem = null;
            _selectedType = "";
            _selectedImageBytes = null;
            ImgItemPreview.Source = null;
            BorderColorPalette.Visibility = Visibility.Collapsed;
            LoadData();
            SvKhuVuc.ScrollToTop();
            SvNhomHang.ScrollToTop();
            SvMatHang.ScrollToTop();
        }

        private void BtnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*",
                Title = "Chọn ảnh đại diện"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _selectedImageBytes = LocalThuVienAnhService.ImageSourceToBytes(dialog.FileName);
                    ImgItemPreview.Source = LocalThuVienAnhService.BytesToBitmapImage(_selectedImageBytes);
                    SaveSelectedImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            _selectedImageBytes = null;
            ImgItemPreview.Source = null;
            SaveSelectedImage();
        }

        private void BtnMauSac_Click(object sender, RoutedEventArgs e)
        {
            BorderColorPalette.Visibility = BorderColorPalette.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void BtnThietKeViTri_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thiết kế vị trí sẽ được mở trong cửa sổ riêng.", "Thiết kế vị trí", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnThietLapMauNhanh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new Views.CauHinhHeThong.ThietLapMauNhanhWindow();
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    await ApplyQuickColorAsync(win.SelectedColor, win.SelectedTarget, win.IsAutoColor);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi áp dụng màu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private object? GetSelectedId() => _selectedItem switch
        {
            TouchLayoutItem item => item.Id,
            _ => null
        };

        private void SaveSelectedImage()
        {
            if (_selectedItem == null || GetSelectedId() == null)
            {
                return;
            }

            try
            {
                string table = GetTableName(_selectedType);
                LocalDatabaseService.Execute(
                    $"UPDATE {table} SET ANH = @Image WHERE CAST(ID AS VARCHAR(50)) = CAST(@Id AS VARCHAR(50))",
                    new { Image = _selectedImageBytes, Id = GetSelectedId() });
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetTableName(string targetType) => targetType switch
        {
            "KhuVuc" => "DKHUVUC",
            "NhomHang" => "DNHOMMATHANG",
            "MatHang" => "DMATHANG",
            _ => throw new ArgumentException("Loại thành phần không hợp lệ.", nameof(targetType))
        };

        private sealed class TouchLayoutItem : INotifyPropertyChanged
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public int? Status { get; set; }
            public string? Mausac { get; set; }
            private string _mauNen = "#5B7F95";
            public string MauNen
            {
                get => _mauNen;
                set
                {
                    if (_mauNen != value)
                    {
                        _mauNen = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MauNen)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MauNenBrush)));
                    }
                }
            }
            public Brush MauNenBrush
            {
                get
                {
                    try
                    {
                        string hex = MauNen?.Trim() ?? "#5B7F95";
                        if (!hex.StartsWith("#")) hex = "#" + hex;
                        if (hex.Length == 7) hex = "#FF" + hex.Substring(1);
                        return (Brush)new BrushConverter().ConvertFromString(hex)!;
                    }
                    catch
                    {
                        return new SolidColorBrush(Color.FromRgb(91, 127, 149));
                    }
                }
            }
            private byte[]? _anh;
            public byte[]? Anh
            {
                get => _anh;
                set
                {
                    if (_anh != value)
                    {
                        _anh = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anh)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnhImageSource)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasImageVisibility)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NoImageVisibility)));
                    }
                }
            }

            public ImageSource? AnhImageSource => LocalThuVienAnhService.BytesToBitmapImage(Anh);
            public Visibility HasImageVisibility => (Anh != null && Anh.Length > 0) ? Visibility.Visible : Visibility.Collapsed;
            public Visibility NoImageVisibility => (Anh != null && Anh.Length > 0) ? Visibility.Collapsed : Visibility.Visible;

            public string? DnhommathangId { get; set; }

            private double _itemHeight = 62;
            public double ItemHeight
            {
                get => _itemHeight;
                set
                {
                    if (Math.Abs(_itemHeight - value) > 0.01)
                    {
                        _itemHeight = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemHeight)));
                    }
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value)
                    {
                        return;
                    }

                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }



        private void ApplyColor(string targetType, string color)
        {
            if (_selectedItem is not TouchLayoutItem selected || string.IsNullOrEmpty(selected.Id))
                return;

            try
            {
                selected.Mausac = color;
                selected.MauNen = color;

                int colorInt = ColorUtils.HexToColorInt(color);
                string table = GetTableName(targetType);
                LocalDatabaseService.Execute($"UPDATE {table} SET MAUSAC = @Color WHERE CAST(ID AS VARCHAR(50)) = CAST(@Id AS VARCHAR(50))", new { Color = colorInt, Id = selected.Id });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu màu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async System.Threading.Tasks.Task ApplyQuickColorAsync(string color, string target, bool isAutoColor)
        {
            var palette = new[]
            {
                "#F44336","#E91E63","#9C27B0","#673AB7","#3F51B5","#2196F3",
                "#03A9F4","#00BCD4","#009688","#4CAF50","#8BC34A","#CDDC39",
                "#FFEB3B","#FFC107","#FF9800","#FF5722","#795548","#607D8B",
                "#1769AA","#2C8A72","#B6423A","#D98900","#C96B35","#F29F05"
            };

            if (isAutoColor)
            {
                if (target == "MatHangTrongNhom" && _selectedItem is TouchLayoutItem selectedGroup && !string.IsNullOrWhiteSpace(selectedGroup.Id))
                {
                    var groupItems = _allMatHang.Where(m => string.Equals(m.DnhommathangId, selectedGroup.Id, StringComparison.OrdinalIgnoreCase)).ToList();
                    int idx = 0;
                    foreach (var item in groupItems)
                    {
                        string itemColor = palette[idx % palette.Length];
                        item.MauNen = itemColor;
                        item.Mausac = itemColor;
                        int cInt = ColorUtils.HexToColorInt(itemColor);
                        LocalDatabaseService.Execute("UPDATE DMATHANG SET MAUSAC = @Color WHERE CAST(ID AS VARCHAR(50)) = CAST(@Id AS VARCHAR(50))", new { Color = cInt, Id = item.Id });
                        idx++;
                    }
                    return;
                }

                if (target == "KhuVuc" && IcKhuVuc.ItemsSource is IEnumerable<TouchLayoutItem> kvItems)
                {
                    int index = 0;
                    foreach (var item in kvItems)
                    {
                        string c = palette[index % palette.Length];
                        item.MauNen = c;
                        item.Mausac = c;
                        int cInt = ColorUtils.HexToColorInt(c);
                        LocalDatabaseService.Execute("UPDATE DKHUVUC SET MAUSAC = @Color WHERE CAST(ID AS VARCHAR(50)) = CAST(@Id AS VARCHAR(50))", new { Color = cInt, Id = item.Id });
                        index++;
                    }
                }
                else if (target == "NhomHang" && IcNhomHang.ItemsSource is IEnumerable<TouchLayoutItem> nhomItems)
                {
                    int index = 0;
                    foreach (var item in nhomItems)
                    {
                        string c = palette[index % palette.Length];
                        item.MauNen = c;
                        item.Mausac = c;
                        int cInt = ColorUtils.HexToColorInt(c);
                        LocalDatabaseService.Execute("UPDATE DNHOMMATHANG SET MAUSAC = @Color WHERE CAST(ID AS VARCHAR(50)) = CAST(@Id AS VARCHAR(50))", new { Color = cInt, Id = item.Id });
                        index++;
                    }
                }
                else
                {
                    int index = 0;
                    foreach (var item in _allMatHang)
                    {
                        string c = palette[index % palette.Length];
                        item.MauNen = c;
                        item.Mausac = c;
                        int cInt = ColorUtils.HexToColorInt(c);
                        LocalDatabaseService.Execute("UPDATE DMATHANG SET MAUSAC = @Color WHERE CAST(ID AS VARCHAR(50)) = CAST(@Id AS VARCHAR(50))", new { Color = cInt, Id = item.Id });
                        index++;
                    }
                }
                return;
            }

            // CHẾ ĐỘ CHỌN MÀU THỦ CÔNG
            int colorIntVal = ColorUtils.HexToColorInt(color);
            if (target == "MatHangTrongNhom" && _selectedItem is TouchLayoutItem gItem && !string.IsNullOrWhiteSpace(gItem.Id))
            {
                var groupItems = _allMatHang.Where(m => string.Equals(m.DnhommathangId, gItem.Id, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var item in groupItems)
                {
                    item.MauNen = color;
                    item.Mausac = color;
                }
                LocalDatabaseService.Execute(
                    "UPDATE DMATHANG SET MAUSAC = @Color WHERE CAST(DNHOMMATHANGID AS VARCHAR(50)) = CAST(@GroupId AS VARCHAR(50)) AND (STATUS IS NULL OR STATUS <> 0)",
                    new { Color = colorIntVal, GroupId = gItem.Id });
                await LocalCauHinhService.SaveSingleConfigAsync("TOUCH_COLOR_MatHangTrongNhom", color);
                return;
            }

            if (target == "KhuVuc" && IcKhuVuc.ItemsSource is IEnumerable<TouchLayoutItem> listKv)
            {
                foreach (var item in listKv)
                {
                    item.MauNen = color;
                    item.Mausac = color;
                }
                LocalDatabaseService.Execute("UPDATE DKHUVUC SET MAUSAC = @Color WHERE (STATUS IS NULL OR STATUS <> 0)", new { Color = colorIntVal });
            }
            else if (target == "NhomHang" && IcNhomHang.ItemsSource is IEnumerable<TouchLayoutItem> listNhom)
            {
                foreach (var item in listNhom)
                {
                    item.MauNen = color;
                    item.Mausac = color;
                }
                LocalDatabaseService.Execute("UPDATE DNHOMMATHANG SET MAUSAC = @Color WHERE (STATUS IS NULL OR STATUS <> 0)", new { Color = colorIntVal });
            }
            else
            {
                foreach (var item in _allMatHang)
                {
                    item.MauNen = color;
                    item.Mausac = color;
                }
                LocalDatabaseService.Execute("UPDATE DMATHANG SET MAUSAC = @Color WHERE (STATUS IS NULL OR STATUS <> 0)", new { Color = colorIntVal });
            }

            await LocalCauHinhService.SaveSingleConfigAsync($"TOUCH_COLOR_{target}", color);
        }

        private void BtnSaveLayout_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem != null)
            {
                SaveSelectedImage();
            }

            MessageBox.Show("Đã lưu cấu hình giao diện cảm ứng.", "Lưu bố cục", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnRefreshLayout_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
