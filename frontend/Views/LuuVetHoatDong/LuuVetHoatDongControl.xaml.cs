using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class LuuVetHoatDongControl : UserControl
    {
        private readonly LocalLuuVetService _luuVetService;
        private bool _isInitialLoaded = false;

        public LuuVetHoatDongControl()
        {
            InitializeComponent();
            _luuVetService = new LocalLuuVetService();
            this.Loaded += LuuVetHoatDongControl_Loaded;
        }

        private async void LuuVetHoatDongControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialLoaded) return;
            _isInitialLoaded = true;

            try
            {
                // Mặc định khoảng thời gian như ảnh mẫu (15/05/2010 đến 31/08/2026)
                dpTuNgay.SelectedDate = new DateTime(2010, 5, 15);
                dpDenNgay.SelectedDate = new DateTime(2026, 8, 31);

                // Nạp danh sách tài khoản
                var tkList = await _luuVetService.GetDanhSachTaiKhoanAsync();
                cbTaiKhoan.ItemsSource = tkList;
                if (tkList.Count > 0)
                {
                    cbTaiKhoan.SelectedIndex = 0;
                }

                await LoadHoaDonListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp dữ liệu lưu vết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialLoaded)
            {
                await LoadHoaDonListAsync();
            }
        }

        private async void TxtSoHd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await LoadHoaDonListAsync();
            }
        }

        private async void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadHoaDonListAsync();
        }

        private async Task LoadHoaDonListAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? new DateTime(2010, 5, 15);
                var denNgay = dpDenNgay.SelectedDate ?? new DateTime(2026, 8, 31);
                string soHd = txtSoHd?.Text?.Trim();

                var hdList = await _luuVetService.GetHoaDonListForLuuVetAsync(tuNgay, denNgay, soHd);
                DgHoaDon.ItemsSource = hdList;

                if (hdList != null && hdList.Count > 0)
                {
                    // Ưu tiên chọn phiếu '082600002' như mẫu ảnh hoặc chọn dòng đầu tiên
                    var target = hdList.FirstOrDefault(x => x.SoPhieu == "082600002") ?? hdList[0];
                    DgHoaDon.SelectedItem = target;
                    DgHoaDon.ScrollIntoView(target);
                }
                else
                {
                    DgLuuVet.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadLuuVetChiTietAsync();
        }

        private async void CbTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialLoaded)
            {
                await LoadLuuVetChiTietAsync();
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitialLoaded)
            {
                await LoadLuuVetChiTietAsync();
            }
        }

        private async Task LoadLuuVetChiTietAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? new DateTime(2010, 5, 15);
                var denNgay = dpDenNgay.SelectedDate ?? new DateTime(2026, 8, 31);
                var selectedHoaDon = DgHoaDon?.SelectedItem as LuuVetHoaDonItemViewModel;

                string donHangId = selectedHoaDon?.Id;
                string soDonHang = selectedHoaDon?.SoPhieu;
                string taiKhoan = cbTaiKhoan?.SelectedItem?.ToString();
                string locText = txtLoc?.Text?.Trim();

                var luuVetList = await _luuVetService.GetLuuVetChiTietAsync(tuNgay, denNgay, donHangId, soDonHang, taiKhoan, locText);
                DgLuuVet.ItemsSource = luuVetList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết lưu vết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region MENU CHUỘT PHẢI BẢNG HÓA ĐƠN (DgHoaDon)

        private void MnuHoaDon_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgHoaDon.CurrentCell != null && DgHoaDon.CurrentCell.Item is LuuVetHoaDonItemViewModel row)
                {
                    var col = DgHoaDon.CurrentCell.Column as DataGridBoundColumn;
                    if (col != null && col.Binding is System.Windows.Data.Binding binding)
                    {
                        var propName = binding.Path?.Path;
                        if (!string.IsNullOrEmpty(propName))
                        {
                            var val = row.GetType().GetProperty(propName)?.GetValue(row, null);
                            Clipboard.SetText(val?.ToString() ?? "");
                            return;
                        }
                    }
                }
                if (DgHoaDon.SelectedItem is LuuVetHoaDonItemViewModel sel)
                {
                    Clipboard.SetText(sel.SoPhieu ?? "");
                }
            }
            catch { }
        }

        private void MnuHoaDon_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgHoaDon.SelectedItem is LuuVetHoaDonItemViewModel item)
                {
                    string rowText = $"{item.Stt}\t{item.SoPhieu}\t{item.Ban}\t{item.TrangThai}";
                    Clipboard.SetText(rowText);
                }
            }
            catch { }
        }

        private void MnuHoaDon_TuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgHoaDon.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MnuHoaDon_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "STT", "Hóa đơn", "Bàn", "Trạng thái" };
            var win = new ChonCotHienThiWindow(DgHoaDon, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MnuHoaDon_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"LuuVet_DanhSachHoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgHoaDon?.ItemsSource as IEnumerable<LuuVetHoaDonItemViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,Hóa đơn,Bàn,Trạng thái");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.Stt}\",\"{item.SoPhieu}\",\"{item.Ban}\",\"{item.TrangThai}\"");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất dữ liệu Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuHoaDon_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgHoaDon, "Danh sách hóa đơn lưu vết");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion

        #region MENU CHUỘT PHẢI BẢNG CHI TIẾT THAO TÁC (DgLuuVet)

        private void MnuLuuVet_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgLuuVet.CurrentCell != null && DgLuuVet.CurrentCell.Item is LuuVetViewModel row)
                {
                    var col = DgLuuVet.CurrentCell.Column as DataGridBoundColumn;
                    if (col != null && col.Binding is System.Windows.Data.Binding binding)
                    {
                        var propName = binding.Path?.Path;
                        if (!string.IsNullOrEmpty(propName))
                        {
                            var val = row.GetType().GetProperty(propName)?.GetValue(row, null);
                            Clipboard.SetText(val?.ToString() ?? "");
                            return;
                        }
                    }
                    if (DgLuuVet.CurrentCell.Column is DataGridTemplateColumn tCol)
                    {
                        if (tCol.Header?.ToString() == "Ngày")
                        {
                            Clipboard.SetText(row.NgayStr ?? "");
                            return;
                        }
                        if (tCol.Header?.ToString() == "Chức năng")
                        {
                            Clipboard.SetText(row.Chucnang ?? "");
                            return;
                        }
                    }
                }
                if (DgLuuVet.SelectedItem is LuuVetViewModel sel)
                {
                    Clipboard.SetText(sel.Note ?? "");
                }
            }
            catch { }
        }

        private void MnuLuuVet_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgLuuVet.SelectedItem is LuuVetViewModel item)
                {
                    string rowText = $"{item.Stt}\t{item.NgayStr}\t{item.GioStr}\t{item.Sodonhang}\t{item.Note}\t{item.Taikhoan}\t{item.Thietbi}\t{item.Ban}\t{item.Chucnang}";
                    Clipboard.SetText(rowText);
                }
            }
            catch { }
        }

        private void MnuLuuVet_TuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgLuuVet.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MnuLuuVet_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "STT", "Ngày", "Giờ", "Số đơn hàng", "Nội dung", "Tài khoản", "Thiết bị", "Bàn", "Chức năng" };
            var win = new ChonCotHienThiWindow(DgLuuVet, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MnuLuuVet_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"LuuVet_ChiTietThaoTac_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgLuuVet?.ItemsSource as IEnumerable<LuuVetViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,Ngày,Giờ,Số đơn hàng,Nội dung,Tài khoản,Thiết bị,Bàn,Chức năng");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.Stt}\",\"{item.NgayStr}\",\"{item.GioStr}\",\"{item.Sodonhang}\",\"{item.Note?.Replace("\"", "\"\"")}\",\"{item.Taikhoan}\",\"{item.Thietbi}\",\"{item.Ban}\",\"{item.Chucnang}\"");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất dữ liệu Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuLuuVet_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgLuuVet, "Chi tiết thao tác lưu vết");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion
    }
}
