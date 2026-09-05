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
    }
}
