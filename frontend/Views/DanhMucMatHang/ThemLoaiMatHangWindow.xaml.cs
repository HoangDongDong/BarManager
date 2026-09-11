using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemLoaiMatHangWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private DLOAIMATHANG _editingItem;
        private string _initialName;
        private string _selectedSImageId;
        private List<SImageViewModel> _allSImages = new List<SImageViewModel>();
        private Action _onSaved;

        public DLOAIMATHANG SavedItem { get; private set; }

        public ThemLoaiMatHangWindow(DLOAIMATHANG itemToEdit = null, string initialName = "", Action onSaved = null)
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _editingItem = itemToEdit;
            _initialName = initialName;
            _onSaved = onSaved;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _allSImages = await _matHangService.GetSImageListAsync();
                BuildImagePopup();

                if (_editingItem != null)
                {
                    Title = "LOẠI MẶT HÀNG - SỬA";
                    TxtTenLoai.Text = _editingItem.Name ?? "";
                    ChkCoBanHang.IsChecked = _editingItem.IsBanHang;
                    ChkCoTonKho.IsChecked = _editingItem.IsTonKho;
                    ChkCoDinhLuong.IsChecked = _editingItem.IsDinhLuong;
                    _selectedSImageId = _editingItem.SimageId;

                    if (!string.IsNullOrEmpty(_selectedSImageId))
                    {
                        var found = _allSImages.FirstOrDefault(x => x.Id == _selectedSImageId);
                        if (found != null)
                        {
                            ImgPreview.Source = found.ImageSource;
                        }
                    }
                }
                else
                {
                    Title = "LOẠI MẶT HÀNG - THÊM MỚI";
                    if (!string.IsNullOrEmpty(_initialName))
                    {
                        TxtTenLoai.Text = _initialName;
                    }
                    ChkCoBanHang.IsChecked = true;
                    ChkCoTonKho.IsChecked = true;
                    ChkCoDinhLuong.IsChecked = false;
                }

                TxtTenLoai.Focus();
                TxtTenLoai.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin: " + ex.Message);
            }
        }

        private void BuildImagePopup()
        {
            PanelNhomAnh.Children.Clear();
            if (_allSImages == null || _allSImages.Count == 0) return;

            var grouped = _allSImages.GroupBy(x => string.IsNullOrWhiteSpace(x.Name) ? "Khác" : x.Name);
            foreach (var grp in grouped)
            {
                // Header group with line
                var headerGrid = new Grid { Margin = new Thickness(4, 6, 4, 3) };
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var lbl = new TextBlock
                {
                    Text = grp.Key,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#336699")),
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var sep = new Separator { VerticalAlignment = VerticalAlignment.Center, Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c8d6e5")) };

                headerGrid.Children.Add(lbl);
                Grid.SetColumn(sep, 1);
                headerGrid.Children.Add(sep);
                PanelNhomAnh.Children.Add(headerGrid);

                // WrapPanel for icons
                var wrap = new WrapPanel { Margin = new Thickness(4, 2, 4, 4) };
                foreach (var img in grp)
                {
                    var btn = new Button
                    {
                        Width = 26,
                        Height = 26,
                        Margin = new Thickness(2),
                        Padding = new Thickness(1),
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Transparent,
                        Cursor = Cursors.Hand,
                        ToolTip = grp.Key
                    };
                    var imgControl = new Image
                    {
                        Source = img.ImageSource,
                        Width = 20,
                        Height = 20,
                        Stretch = Stretch.Uniform
                    };
                    btn.Content = imgControl;

                    string currId = img.Id;
                    var currSrc = img.ImageSource;
                    btn.Click += (s, ev) =>
                    {
                        _selectedSImageId = currId;
                        ImgPreview.Source = currSrc;
                        PopupAnh.IsOpen = false;
                    };

                    wrap.Children.Add(btn);
                }
                PanelNhomAnh.Children.Add(wrap);
            }
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            PopupAnh.IsOpen = !PopupAnh.IsOpen;
        }

        private void BtnKhongAnh_Click(object sender, RoutedEventArgs e)
        {
            _selectedSImageId = null;
            ImgPreview.Source = null;
            PopupAnh.IsOpen = false;
        }

        private async System.Threading.Tasks.Task<bool> SaveDataInternalAsync()
        {
            string name = TxtTenLoai.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên loại mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenLoai.Focus();
                return false;
            }

            bool isEdit = _editingItem != null;
            var loai = isEdit ? _editingItem : new DLOAIMATHANG();
            loai.Name = name;
            loai.SimageId = _selectedSImageId;
            loai.Cobanhang = ChkCoBanHang.IsChecked == true ? "30" : "0";
            loai.Cotonkho = ChkCoTonKho.IsChecked == true ? "30" : "0";
            loai.Codinhluong = ChkCoDinhLuong.IsChecked == true ? "30" : "0";

            bool success = isEdit
                ? await _matHangService.UpdateLoaiMatHangAsync(loai)
                : await _matHangService.InsertLoaiMatHangAsync(loai);

            if (success)
            {
                SavedItem = loai;
                _onSaved?.Invoke();
                return true;
            }
            return false;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataInternalAsync())
            {
                MessageBox.Show("Đã lưu loại mặt hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataInternalAsync())
            {
                _editingItem = null;
                TxtTenLoai.Text = "";
                _selectedSImageId = null;
                ImgPreview.Source = null;
                ChkCoBanHang.IsChecked = true;
                ChkCoTonKho.IsChecked = true;
                ChkCoDinhLuong.IsChecked = false;
                TxtTenLoai.Focus();
            }
        }

        private async void BtnLuuThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataInternalAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuMoi_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuu_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuMoi_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
