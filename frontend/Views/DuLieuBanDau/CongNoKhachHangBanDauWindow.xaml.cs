using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DuLieuBanDau
{
    public partial class CongNoKhachHangBanDauWindow : Window
    {
        private List<CongNoBanDauItemViewModel> _allItems = new List<CongNoBanDauItemViewModel>();
        private ObservableCollection<CongNoBanDauItemViewModel> _displayItems = new ObservableCollection<CongNoBanDauItemViewModel>();

        public CongNoKhachHangBanDauWindow()
        {
            InitializeComponent();
            DpNgayChot.SelectedDate = DateTime.Today;

            Loaded += CongNoKhachHangBanDauWindow_Loaded;
            KeyDown += CongNoKhachHangBanDauWindow_KeyDown;
        }

        private void CongNoKhachHangBanDauWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                TxtTimKiem.Focus();
                TxtTimKiem.SelectAll();
                e.Handled = true;
            }
        }

        private async void CongNoKhachHangBanDauWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _allItems = await LocalCongNoBanDauService.GetCongNoKhachHangBanDauListAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu công nợ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            string kw = TxtTimKiem.Text?.Trim().ToLower() ?? "";

            var filtered = _allItems.Where(x =>
            {
                if (string.IsNullOrEmpty(kw)) return true;
                return (x.MaKhach?.ToLower().Contains(kw) ?? false) ||
                       (x.TenKhach?.ToLower().Contains(kw) ?? false) ||
                       (x.DiaChi?.ToLower().Contains(kw) ?? false) ||
                       (x.DienThoai?.ToLower().Contains(kw) ?? false);
            }).ToList();

            _displayItems.Clear();
            int stt = 1;
            foreach (var item in filtered)
            {
                item.Stt = stt++;
                _displayItems.Add(item);
            }

            DgCongNo.ItemsSource = null;
            DgCongNo.ItemsSource = _displayItems;
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DgCongNo_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void DgCongNo_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.EditingElement is TextBox tb)
            {
                if (decimal.TryParse(tb.Text.Replace(",", "").Replace(".", "").Replace(" ", ""), out decimal val))
                {
                    if (e.Row.DataContext is CongNoBanDauItemViewModel item)
                    {
                        item.SoTien = val;
                    }
                }
            }
        }

        private void BtnExportMau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Title = "Xuất file mẫu công nợ khách hàng ban đầu",
                    Filter = "Excel Files (*.xls)|*.xls|Excel Workbook (*.xlsx)|*.xlsx|Tất cả tệp (*.*)|*.*",
                    FileName = "MauCongNo.xls"
                };

                if (sfd.ShowDialog(this) == true)
                {
                    LocalCongNoBanDauService.ExportMauCongNo(sfd.FileName, _allItems);
                    MessageBox.Show("Xuất file mẫu công nợ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file mẫu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Chọn file excel công nợ để import",
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|Tất cả tệp (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) == true)
            {
                try
                {
                    var cols = LocalCongNoBanDauService.GetExcelColumnNames(ofd.FileName);
                    if (cols == null || cols.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy dữ liệu cột trong file Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var mapWin = new MappingCongNoExcelWindow(cols)
                    {
                        Owner = this
                    };

                    if (mapWin.ShowDialog() == true)
                    {
                        var mapping = mapWin.MappingList;
                        string colMaExcel = mapping.FirstOrDefault(x => x.MappedField == "Mã đối tác")?.ExcelColumn;
                        string colTienExcel = mapping.FirstOrDefault(x => x.MappedField == "Công nợ đầu")?.ExcelColumn;

                        if (string.IsNullOrEmpty(colMaExcel))
                        {
                            MessageBox.Show("Chưa chọn cột ánh xạ cho 'Mã đối tác'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var imported = LocalCongNoBanDauService.ReadExcelWithMapping(ofd.FileName, colMaExcel, colTienExcel);
                        if (imported == null || imported.Count == 0)
                        {
                            MessageBox.Show("Không có dữ liệu trong file Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        int matchCount = 0;
                        foreach (var imp in imported)
                        {
                            string ma = imp.MaKhach?.Trim() ?? "";
                            if (string.IsNullOrEmpty(ma)) continue;

                            var matched = _allItems.FirstOrDefault(x => string.Equals(x.MaKhach?.Trim(), ma, StringComparison.OrdinalIgnoreCase));
                            if (matched == null)
                            {
                                // Show warning message matching user's screenshot
                                MessageBox.Show($"Mã đối tác '{ma}' không tồn tại", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                                continue;
                            }

                            matched.SoTien = imp.SoTien;
                            matchCount++;
                        }

                        ApplyFilter();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            DateTime ngayChot = DpNgayChot.SelectedDate ?? DateTime.Today;

            var (ok, error) = await LocalCongNoBanDauService.SaveCongNoKhachHangBanDauAsync(ngayChot, _allItems);
            if (ok)
            {
                MessageBox.Show("Đã lưu công nợ ban đầu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
