using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DuLieuBanDau
{
    public partial class TonKhoBanDauWindow : Window
    {
        private List<KhoHangComboItem> _khoList = new List<KhoHangComboItem>();
        private List<TonKhoBanDauItemViewModel> _allItems = new List<TonKhoBanDauItemViewModel>();
        private List<TonKhoBanDauItemViewModel> _displayItems = new List<TonKhoBanDauItemViewModel>();
        private string _selectedKhoId = "";
        private bool _isUpdatingCombo = false;

        public TonKhoBanDauWindow(string initialKhoId = "")
        {
            InitializeComponent();
            _selectedKhoId = initialKhoId;
            this.KeyDown += TonKhoBanDauWindow_KeyDown;
        }

        private void TonKhoBanDauWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                TxtTimKiem.Focus();
                TxtTimKiem.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhoListAsync();
        }

        private async Task LoadKhoListAsync()
        {
            try
            {
                _isUpdatingCombo = true;
                _khoList = await LocalTonKhoBanDauService.GetKhoHangListAsync();
                CboKhoHang.ItemsSource = _khoList;

                if (_khoList.Count > 0)
                {
                    var found = _khoList.FirstOrDefault(x => x.Id == _selectedKhoId);
                    if (found != null)
                    {
                        CboKhoHang.SelectedItem = found;
                    }
                    else
                    {
                        CboKhoHang.SelectedIndex = 0;
                        _selectedKhoId = _khoList[0].Id;
                    }
                }
                _isUpdatingCombo = false;

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadKhoListAsync: " + ex.Message);
            }
            finally
            {
                _isUpdatingCombo = false;
            }
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_selectedKhoId)) return;

            try
            {
                var result = await LocalTonKhoBanDauService.GetTonKhoBanDauListAsync(_selectedKhoId);
                DpNgayChot.SelectedDate = result.NgayChot;
                _allItems = result.Items;

                foreach (var item in _allItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải tồn kho ban đầu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TonKhoBanDauItemViewModel.Ton) ||
                e.PropertyName == nameof(TonKhoBanDauItemViewModel.GiaVon) ||
                e.PropertyName == nameof(TonKhoBanDauItemViewModel.GiaTri))
            {
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            decimal tongGiaTri = _allItems.Sum(x => x.GiaTri);
            int countTon = _allItems.Count(x => x.Ton > 0);

            TxtTongGiaTri.Text = tongGiaTri.ToString("N0");
            TxtTongSoLuong.Text = $"Tổng số lượng: {countTon:N0}";
        }

        private void ApplyFilter()
        {
            string kw = TxtTimKiem.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(kw))
            {
                _displayItems = _allItems.ToList();
            }
            else
            {
                _displayItems = _allItems.Where(x =>
                    (!string.IsNullOrEmpty(x.TenHang) && x.TenHang.ToLower().Contains(kw)) ||
                    (!string.IsNullOrEmpty(x.MaHang) && x.MaHang.ToLower().Contains(kw)) ||
                    (!string.IsNullOrEmpty(x.MaSanCo) && x.MaSanCo.ToLower().Contains(kw))
                ).ToList();
            }

            DgTonKhoBanDau.ItemsSource = _displayItems;
            CalculateTotals();
        }

        private async void CboKhoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingCombo) return;

            if (CboKhoHang.SelectedValue is string khoId && !string.IsNullOrEmpty(khoId))
            {
                _selectedKhoId = khoId;
                await LoadDataAsync();
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TxtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                DgTonKhoBanDau.Focus();
                if (DgTonKhoBanDau.Items.Count > 0 && DgTonKhoBanDau.SelectedIndex < 0)
                {
                    DgTonKhoBanDau.SelectedIndex = 0;
                }
            }
        }

        private void EditableTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Focus();
                tb.SelectAll();
            }
        }

        private void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotals();
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allItems.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu mặt hàng để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"MauTonKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                bool success = LocalTonKhoBanDauService.ExportMauTonKho(sfd.FileName, _allItems);
                if (success)
                {
                    var res = MessageBox.Show("Xuất file mẫu tồn kho thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Xuất file mẫu thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*",
                Title = "Chọn file Excel tồn kho"
            };

            if (ofd.ShowDialog() != true) return;

            try
            {
                var excelCols = LocalTonKhoBanDauService.GetExcelColumns(ofd.FileName);
                if (excelCols.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu hoặc cột trong file Excel!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var mapWin = new MappingTonKhoExcelWindow(excelCols)
                {
                    Owner = this
                };

                if (mapWin.ShowDialog() != true) return;

                var (importedData, unmatchedItems) = LocalTonKhoBanDauService.ReadExcelDataWithMapping(ofd.FileName, mapWin.FinalMappings, _allItems);

                if (unmatchedItems != null && unmatchedItems.Count > 0)
                {
                    foreach (var un in unmatchedItems)
                    {
                        MessageBox.Show($"Mã hàng hóa '{un}' không tồn tại", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                int appliedCount = 0;
                foreach (var row in importedData)
                {
                    var matched = _allItems.FirstOrDefault(x =>
                        (!string.IsNullOrEmpty(row.DmathangId) && x.Id == row.DmathangId) ||
                        (!string.IsNullOrEmpty(row.MaSanCo) && string.Equals(x.MaSanCo, row.MaSanCo, StringComparison.OrdinalIgnoreCase))
                    );

                    if (matched != null)
                    {
                        matched.Ton = row.Ton;
                        if (row.GiaVon > 0)
                        {
                            matched.GiaVon = row.GiaVon;
                        }
                        appliedCount++;
                    }
                }

                ApplyFilter();
                CalculateTotals();
                MessageBox.Show($"Nhập thành công {appliedCount} dòng dữ liệu từ Excel!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi nhập dữ liệu từ Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedKhoId))
            {
                MessageBox.Show("Vui lòng chọn kho hàng trước khi ghi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime ngayChot = DpNgayChot.SelectedDate ?? DateTime.Today;

            try
            {
                bool success = await LocalTonKhoBanDauService.SaveTonKhoBanDauAsync(_selectedKhoId, ngayChot, _allItems);
                if (success)
                {
                    MessageBox.Show("Ghi dữ liệu tồn kho ban đầu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ghi dữ liệu thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Chuc nang thiet ke mau bao cao cho phep ban tuy chinh:\n" +
                "  - Bo cuc va dinh dang bao cao\n" +
                "  - Font chu, co chu, mau sac\n" +
                "  - Logo va thong tin dau trang\n" +
                "  - Them/bo cac cot hien thi\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Thiet ke mau bao cao",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnThamSoTuyChinh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tham so tuy chinh bao cao cho phep ban:\n" +
                "  - Chon cac cot hien thi trong bao cao\n" +
                "  - Thiet lap tieu chi nhom du lieu\n" +
                "  - Cau hinh cac dieu kien loc nang cao\n" +
                "  - Tuy chinh dinh dang so va ngay thang\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Tham so tuy chinh",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnXemDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dlg = new System.Windows.Window
                {
                    Title = "Xem du lieu tho",
                    Width = 950,
                    Height = 620,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                    Owner = System.Windows.Window.GetWindow(this),
                    Background = System.Windows.Media.Brushes.White
                };

                var mainGrid = new System.Windows.Controls.Grid();
                mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
                mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

                var header = new System.Windows.Controls.Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1e3a5f")),
                    Padding = new System.Windows.Thickness(14, 8, 14, 8)
                };
                header.Child = new System.Windows.Controls.TextBlock
                {
                    Text = "Xem du lieu tho - Du lieu hien thi trong bao cao",
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14,
                    FontWeight = System.Windows.FontWeights.SemiBold,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                System.Windows.Controls.Grid.SetRow(header, 0);
                mainGrid.Children.Add(header);

                var dataGrid = new System.Windows.Controls.DataGrid
                {
                    IsReadOnly = true,
                    AutoGenerateColumns = true,
                    CanUserSortColumns = true,
                    CanUserResizeColumns = true,
                    GridLinesVisibility = System.Windows.Controls.DataGridGridLinesVisibility.All,
                    AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#f0f4f8")),
                    HeadersVisibility = System.Windows.Controls.DataGridHeadersVisibility.Column,
                    Margin = new System.Windows.Thickness(8),
                    FontSize = 12
                };

                var fields = this.GetType().GetFields(
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                object? dataSource = null;
                int maxCount = 0;
                foreach (var f in fields)
                {
                    var val = f.GetValue(this);
                    if (val is System.Collections.IList list && list.Count > maxCount)
                    {
                        maxCount = list.Count;
                        dataSource = val;
                    }
                }

                if (dataSource != null)
                    dataGrid.ItemsSource = (System.Collections.IEnumerable)dataSource;
                else
                    dataGrid.ItemsSource = new[] { new { ThongBao = "Khong co du lieu. Hay tai du lieu truoc (F5) roi mo lai." } };

                var scroll = new System.Windows.Controls.ScrollViewer
                {
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    Content = dataGrid
                };
                System.Windows.Controls.Grid.SetRow(scroll, 1);
                mainGrid.Children.Add(scroll);

                var footer = new System.Windows.Controls.Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#f5f7fa")),
                    BorderBrush = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#d0d8e4")),
                    BorderThickness = new System.Windows.Thickness(0, 1, 0, 0),
                    Padding = new System.Windows.Thickness(12, 6, 12, 6)
                };
                var footerPanel = new System.Windows.Controls.DockPanel { LastChildFill = false };
                footerPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = $"Tong so ban ghi: {maxCount}",
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                });
                var closeBtn = new System.Windows.Controls.Button
                {
                    Content = "Dong",
                    Width = 80,
                    Height = 28
                };
                System.Windows.Controls.DockPanel.SetDock(closeBtn, System.Windows.Controls.Dock.Right);
                closeBtn.Click += (s, ev) => dlg.Close();
                footerPanel.Children.Add(closeBtn);
                footer.Child = footerPanel;
                System.Windows.Controls.Grid.SetRow(footer, 2);
                mainGrid.Children.Add(footer);

                dlg.Content = mainGrid;
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi: {ex.Message}", "Loi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
