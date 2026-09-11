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

        public ThietKeGiaoDienCamUngControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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
                if (parts.Length > 0 && int.TryParse(parts[0], out int columns) && columns is >= 1 and <= 99)
                {
                    int rows = parts.Length > 1 && int.TryParse(parts[1], out int parsedRows) && parsedRows is >= 1 and <= 99
                        ? parsedRows
                        : 5;
                    ApplyGridSettings(targetType, columns, rows);
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Responsive layout adjustments
        }

        private void LoadData()
        {
            try
            {
                // Load Khu Vực
                var khuVucList = LocalDatabaseService.GetAll<TouchLayoutItem>("SELECT T.ID, T.NAME, T.STATUS, T.ANH, T.MAUSAC FROM DKHUVUC T WHERE (T.STATUS IS NULL OR T.STATUS <> 0) ORDER BY T.SORTORDER, T.NAME");
                foreach (var item in khuVucList) item.MauNen = GetColor(item.Mausac, "#3D8EAA");
                IcKhuVuc.ItemsSource = khuVucList;

                // Load Nhóm Hàng  
                var nhomList = LocalDatabaseService.GetAll<TouchLayoutItem>("SELECT T.ID, T.NAME, T.STATUS, T.ANH, T.MAUSAC FROM DNHOMMATHANG T WHERE (T.STATUS IS NULL OR T.STATUS <> 0) ORDER BY T.SORTORDER, T.NAME");
                foreach (var item in nhomList) item.MauNen = GetColor(item.Mausac, "#4E78A6");
                IcNhomHang.ItemsSource = nhomList;

                // Load Mặt Hàng
                _allMatHang = LocalDatabaseService.GetAll<TouchLayoutItem>("SELECT T.ID, T.NAME, T.STATUS, T.ANH, T.MAUSAC, T.DNHOMMATHANGID FROM DMATHANG T WHERE (T.STATUS IS NULL OR T.STATUS <> 0) ORDER BY T.NAME").ToList();
                foreach (var item in _allMatHang) item.MauNen = GetColor(item.Mausac, "#5B7F95");
                IcMatHang.ItemsSource = _allMatHang;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetColor(string? color, string fallback)
        {
            return string.IsNullOrWhiteSpace(color) ? fallback : color;
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
                        .Where(item => string.Equals(item.DnhommathangId, group.Id, StringComparison.OrdinalIgnoreCase))
                        .ToList();
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
            ItemsControl target = targetType switch
            {
                "KhuVuc" => IcKhuVuc,
                "NhomHang" => IcNhomHang,
                _ => IcMatHang
            };

            var panelFactory = new FrameworkElementFactory(typeof(UniformGrid));
            panelFactory.SetValue(UniformGrid.ColumnsProperty, Math.Max(1, columns));
            target.ItemsPanel = new ItemsPanelTemplate(panelFactory);
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
                    await ApplyQuickColorAsync(win.SelectedColor, win.SelectedTarget);
                    LoadData();
                }
            }
            catch { }
        }

        private void ApplyColor(string targetType, string color)
        {
            try
            {
                string table = GetTableName(targetType);
                LocalDatabaseService.Execute($"UPDATE {table} SET MAUSAC = @Color WHERE ID = @Id", new { Color = color, Id = GetSelectedId() });
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu màu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    $"UPDATE {table} SET ANH = @Image WHERE ID = @Id",
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
            public string MauNen { get; set; } = "#5B7F95";
            public Brush MauNenBrush
            {
                get
                {
                    try
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(MauNen));
                    }
                    catch
                    {
                        return new SolidColorBrush(Color.FromRgb(91, 127, 149));
                    }
                }
            }
            public byte[]? Anh { get; set; }
            public string? DnhommathangId { get; set; }
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

        private async System.Threading.Tasks.Task ApplyQuickColorAsync(string color, string target)
        {
            if (target == "MatHangTrongNhom" && _selectedItem is TouchLayoutItem selectedGroup && !string.IsNullOrWhiteSpace(selectedGroup.Id))
            {
                LocalDatabaseService.Execute(
                    "UPDATE DMATHANG SET MAUSAC = @Color WHERE DNHOMMATHANGID = @GroupId AND (STATUS IS NULL OR STATUS <> 0)",
                    new { Color = color, GroupId = selectedGroup.Id });
                await LocalCauHinhService.SaveSingleConfigAsync("TOUCH_COLOR_MatHangTrongNhom", color);
                return;
            }

            string table = target switch
            {
                "KhuVuc" => "DKHUVUC",
                "NhomHang" => "DNHOMMATHANG",
                _ => "DMATHANG"
            };
            LocalDatabaseService.Execute($"UPDATE {table} SET MAUSAC = @Color WHERE (STATUS IS NULL OR STATUS <> 0)", new { Color = color });
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
