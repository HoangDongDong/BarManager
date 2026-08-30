using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DieuChinhHoaDonControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly LocalSuDungDichVuService _dichVuService;
        private List<PosMatHangViewModel> _allMatHangList = new List<PosMatHangViewModel>();

        public DieuChinhHoaDonControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _dichVuService = new LocalSuDungDichVuService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = DateTime.Today;
            dpDenNgay.SelectedDate = DateTime.Today;
            
            await LoadDataAsync();
            await LoadMenuTreeAsync();
        }

        private async void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
                
                var list = await _hoaDonService.GetHoaDonListAsync(tuNgay, denNgay);
                DgHoaDon.ItemsSource = list;

                if (list != null && list.Count > 0)
                {
                    DgHoaDon.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadMenuTreeAsync()
        {
            try
            {
                var menuTree = await _dichVuService.GetNhomMatHangTreeAsync();
                SetTreeExpandState(menuTree, true);
                TvMenu.ItemsSource = menuTree;
                
                if (menuTree != null && menuTree.Count > 0)
                {
                    menuTree[0].IsSelected = true;
                }

                _allMatHangList = await _dichVuService.GetMatHangListAsync(null);
                FilterMatHangList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetTreeExpandState(IEnumerable<PosNhomMatHangViewModel> items, bool isExpanded)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                item.IsExpanded = isExpanded;
                if (item.Children != null && item.Children.Count > 0)
                {
                    SetTreeExpandState(item.Children, isExpanded);
                }
            }
        }

        private async void TvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is PosNhomMatHangViewModel selectedItem)
            {
                try
                {
                    string searchNhomId = string.IsNullOrEmpty(selectedItem.Id) ? null : selectedItem.Id;
                    _allMatHangList = await _dichVuService.GetMatHangListAsync(searchNhomId);
                    FilterMatHangList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải danh sách món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterMatHangList();
        }

        private void FilterMatHangList()
        {
            if (_allMatHangList == null) return;
            string kw = TxtTimKiem?.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(kw))
            {
                DgMatHang.ItemsSource = _allMatHangList;
            }
            else
            {
                DgMatHang.ItemsSource = _allMatHangList.Where(m => 
                    (m.Name != null && m.Name.ToLower().Contains(kw)) ||
                    (m.Code != null && m.Code.ToLower().Contains(kw))
                ).ToList();
            }
        }

        private async void DgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgHoaDon.SelectedItem is HoaDonViewModel selectedHoaDon)
            {
                try
                {
                    // 1. Hiển thị thông tin chung lên phần Header
                    TxtGioBatDau.Text = selectedHoaDon.BatDau.HasValue 
                        ? selectedHoaDon.BatDau.Value.ToString("dd/MM/yyyy HH:mm") 
                        : (selectedHoaDon.Ngay.HasValue ? selectedHoaDon.Ngay.Value.ToString("dd/MM/yyyy") : "");

                    var gioKetThuc = selectedHoaDon.KetThuc ?? selectedHoaDon.GioThanhToan;
                    TxtGioKetThuc.Text = gioKetThuc.HasValue 
                        ? gioKetThuc.Value.ToString("dd/MM/yyyy HH:mm") 
                        : "";

                    DpNgayOrder.SelectedDate = selectedHoaDon.Ngay ?? DateTime.Today;
                    TxtSoPhieu.Text = selectedHoaDon.SoPhieu ?? "";
                    TxtSoKhach.Text = selectedHoaDon.SoKhach > 0 ? selectedHoaDon.SoKhach.ToString() : "1";
                    TxtTenBan.Text = selectedHoaDon.Ban ?? "";
                    TxtKhachHang.Text = selectedHoaDon.KhachHang ?? "";

                    // 2. Hiển thị thông tin Tổng kết ở Footer
                    TxtTienHang.Text = selectedHoaDon.TienHang.ToString("N0");
                    TxtGiamGiaPt.Text = selectedHoaDon.TiLeGiamGia.ToString("N0");
                    TxtGiamGia.Text = selectedHoaDon.TienGiamGia.ToString("N0");
                    TxtTongCong.Text = selectedHoaDon.TongCong.ToString("N0");
                    TxtGhiChu.Text = selectedHoaDon.GhiChu ?? "";

                    // 3. Tải danh sách món ăn chi tiết
                    if (!string.IsNullOrEmpty(selectedHoaDon.Id))
                    {
                        var chiTietList = await _hoaDonService.GetChiTietHoaDonAsync(selectedHoaDon.Id);
                        DgChiTiet.ItemsSource = chiTietList;
                    }
                    else
                    {
                        DgChiTiet.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnTraDo_Click(object sender, RoutedEventArgs e)
        {
            if (DgHoaDon.SelectedItem is HoaDonViewModel selectedHoaDon)
            {
                var chiTietList = DgChiTiet.ItemsSource as IEnumerable<ChiTietHoaDonViewModel>;
                if (chiTietList == null || !chiTietList.Any())
                {
                    chiTietList = await _hoaDonService.GetChiTietHoaDonAsync(selectedHoaDon.Id);
                }

                if (chiTietList == null || !chiTietList.Any())
                {
                    MessageBox.Show("Hóa đơn này không có món nào để trả đồ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var win = new KiemDoWindow(selectedHoaDon.Id, chiTietList);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    int selectedIndex = DgHoaDon.SelectedIndex;
                    await LoadDataAsync();
                    if (selectedIndex >= 0 && selectedIndex < DgHoaDon.Items.Count)
                    {
                        DgHoaDon.SelectedIndex = selectedIndex;
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn trong danh sách để trả đồ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hóa đơn này đã được thanh toán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region MENU CHUỘT PHẢI (CONTEXT MENU)

        private void MnuSaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgHoaDon.CurrentCell != null && DgHoaDon.CurrentCell.Item is HoaDonViewModel row)
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
                if (DgHoaDon.SelectedItem is HoaDonViewModel sel)
                {
                    Clipboard.SetText(sel.SoPhieu ?? "");
                }
            }
            catch { }
        }

        private void MnuSaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgHoaDon.SelectedItem is HoaDonViewModel item)
                {
                    string rowText = $"{item.SoPhieu}\t{item.Ngay:dd/MM/yyyy}\t{item.Ban}\t{item.BatDau:HH:mm}\t{item.KetThuc:HH:mm}\t{item.GioThanhToan:HH:mm}\t{item.TongCong:N0}\t{item.KhachHang}\t{item.TienGiamGia:N0}\t{item.TiLeGiamGia}\t{item.TienHang:N0}\t{item.KhachDua:N0}\t{item.TraLai:N0}\t{item.TheThanhToan:N0}\t{item.TienMat:N0}\t{item.SoKhach}\t{item.TiLeGiamGiaGio}\t{item.SoOrder}\t{item.TienGiamGiaGio:N0}";
                    Clipboard.SetText(rowText);
                }
            }
            catch { }
        }

        private void MnuTuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgHoaDon.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MnuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string>
            {
                "Số phiếu", "Ngày", "Bàn", "Bắt đầu", "Kết thúc", "Giờ thanh toán",
                "Tổng cộng", "Khách hàng", "Tiền giảm giá", "Tỉ lệ giảm giá",
                "Tiền hàng", "Khách đưa", "Trả lại", "Thẻ tt", "Tiền mặt", "Số khách",
                "Tỉ lệ giảm giá giờ", "Số order", "Tiền giảm giá giờ"
            };
            var win = new ChonCotHienThiWindow(DgHoaDon, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MnuXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"DanhSachHoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgHoaDon?.ItemsSource as IEnumerable<HoaDonViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("Số phiếu,Ngày,Bàn,Bắt đầu,Kết thúc,Giờ thanh toán,Tổng cộng,Khách hàng,Tiền giảm giá,Tỉ lệ giảm giá,Tiền hàng,Khách đưa,Trả lại,Thẻ tt,Tiền mặt,Số khách,Tỉ lệ giảm giá giờ,Số order,Tiền giảm giá giờ");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.SoPhieu}\",\"{item.Ngay:dd/MM/yyyy}\",\"{item.Ban}\",\"{item.BatDau:HH:mm}\",\"{item.KetThuc:HH:mm}\",\"{item.GioThanhToan:HH:mm}\",{item.TongCong},\"{item.KhachHang}\",{item.TienGiamGia},{item.TiLeGiamGia},{item.TienHang},{item.KhachDua},{item.TraLai},{item.TheThanhToan},{item.TienMat},{item.SoKhach},{item.TiLeGiamGiaGio},\"{item.SoOrder}\",{item.TienGiamGiaGio}");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất file Excel CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuInDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            var win = new InLuoiWindow(DgHoaDon, $"DANH SÁCH HÓA ĐƠN ({tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy})");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion

        #region XỬ LÝ KHÁCH HÀNG (F12)

        private List<KhachHangLookupViewModel> _allKhachHangs = new List<KhachHangLookupViewModel>();
        private ObservableCollection<KhachHangLookupViewModel> _filteredKhachHangs = new ObservableCollection<KhachHangLookupViewModel>();

        private async void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                e.Handled = true;
                if (TxtKhachHang != null)
                {
                    TxtKhachHang.Focus();
                    TxtKhachHang.SelectAll();
                    if (PopupKhachHang != null) PopupKhachHang.IsOpen = true;
                    if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = true;
                    await LoadKhachHangAsync();
                    await FilterKhachHangAsync(TxtKhachHang.Text);
                }
            }
        }

        private async void TxtKhachHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PopupKhachHang != null && !PopupKhachHang.IsOpen && TxtKhachHang.IsFocused)
            {
                PopupKhachHang.IsOpen = true;
                if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = true;
            }
            await FilterKhachHangAsync(TxtKhachHang?.Text ?? "");
        }

        private void TxtKhachHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && PopupKhachHang != null && PopupKhachHang.IsOpen)
            {
                DgKhachHang?.Focus();
                if (DgKhachHang?.Items.Count > 0)
                {
                    DgKhachHang.SelectedIndex = 0;
                }
            }
        }

        private async void BtnKhachHangToggle_Click(object sender, RoutedEventArgs e)
        {
            if (BtnKhachHangToggle?.IsChecked == true)
            {
                await LoadKhachHangAsync();
                await FilterKhachHangAsync(TxtKhachHang?.Text ?? "");
            }
        }

        private async Task LoadKhachHangAsync()
        {
            if (_allKhachHangs.Count == 0 && _hoaDonService != null)
            {
                var list = await _hoaDonService.GetKhachHangLookupAsync();
                _allKhachHangs = list.Where(x => !string.IsNullOrEmpty(x.Id) || !string.IsNullOrEmpty(x.Name)).ToList();
            }
        }

        private async Task FilterKhachHangAsync(string filter)
        {
            await LoadKhachHangAsync();
            _filteredKhachHangs.Clear();

            var query = _allKhachHangs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string lower = filter.Trim().ToLower();
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Contains(lower))
                                      || (x.Makhach != null && x.Makhach.ToLower().Contains(lower))
                                      || (x.Dienthoai != null && x.Dienthoai.ToLower().Contains(lower))
                                      || (x.Diachi != null && x.Diachi.ToLower().Contains(lower)));
            }

            foreach (var item in query)
            {
                _filteredKhachHangs.Add(item);
            }

            if (DgKhachHang != null)
            {
                DgKhachHang.ItemsSource = _filteredKhachHangs;
                if (_filteredKhachHangs.Count > 0)
                {
                    DgKhachHang.SelectedIndex = 0;
                }
            }
        }

        private async void DgKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await SelectKhachHangItemAsync();
        }

        private async void DgKhachHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SelectKhachHangItemAsync();
            }
        }

        private async Task SelectKhachHangItemAsync()
        {
            if (DgKhachHang?.SelectedItem is KhachHangLookupViewModel selected)
            {
                if (TxtKhachHang != null)
                {
                    TxtKhachHang.Text = selected.Name;
                    TxtKhachHang.Tag = selected.Id;
                }

                if (DgHoaDon?.SelectedItem is HoaDonViewModel selectedHoaDon)
                {
                    selectedHoaDon.KhachHang = selected.Name;
                    await _hoaDonService.UpdateHoaDonKhachHangAsync(selectedHoaDon.Id, selected.Id);
                }

                if (PopupKhachHang != null) PopupKhachHang.IsOpen = false;
                if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = false;
            }
        }

        private async void BtnThemKhachHang_Click(object sender, RoutedEventArgs e)
        {
            var win = new InputWindow("Thêm khách hàng", "Nhập tên khách hàng mới:", "");
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.InputText))
            {
                string newName = win.InputText.Trim();
                string maKhach = (_allKhachHangs.Count + 1).ToString("D3");
                bool ok = await _hoaDonService.InsertKhachHangAsync(newName, maKhach, "", "");
                if (ok)
                {
                    _allKhachHangs.Clear();
                    await LoadKhachHangAsync();
                    await FilterKhachHangAsync(newName);

                    if (TxtKhachHang != null) TxtKhachHang.Text = newName;
                    if (DgHoaDon?.SelectedItem is HoaDonViewModel selectedHoaDon)
                    {
                        selectedHoaDon.KhachHang = newName;
                        var found = _allKhachHangs.FirstOrDefault(x => x.Name == newName);
                        if (found != null)
                        {
                            await _hoaDonService.UpdateHoaDonKhachHangAsync(selectedHoaDon.Id, found.Id);
                        }
                    }
                    if (PopupKhachHang != null) PopupKhachHang.IsOpen = false;
                    if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = false;
                }
            }
        }

        private void BtnSuaKhachHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Vui lòng vào menu 'Khách hàng' để cập nhật chi tiết thông tin khách hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnKhachHangReload_Click(object sender, RoutedEventArgs e)
        {
            _allKhachHangs.Clear();
            await LoadKhachHangAsync();
            await FilterKhachHangAsync(TxtKhachHang?.Text ?? "");
        }

        private void BtnKhachHangDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở danh mục quản lý khách hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
