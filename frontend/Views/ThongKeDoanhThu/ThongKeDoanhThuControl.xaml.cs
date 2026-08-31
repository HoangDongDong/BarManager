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
    public partial class ThongKeDoanhThuControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private bool _isLoaded = false;

        public ThongKeDoanhThuControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            dpTuNgay.SelectedDate = DateTime.Today;
            dpDenNgay.SelectedDate = DateTime.Today;

            await LoadCuaHangListAsync();
            await LoadDataAsync();
        }

        private async Task LoadCuaHangListAsync()
        {
            try
            {
                var stores = await _hoaDonService.GetCuaHangListAsync();
                LstCuaHang.ItemsSource = stores;
                if (stores.Count > 0 && string.IsNullOrEmpty(TxtSelectedCuaHang.Text))
                {
                    TxtSelectedCuaHang.Text = stores[0].Name;
                }
            }
            catch { }
        }

        private void TxtSelectedCuaHang_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleCuaHang.IsChecked = !BtnToggleCuaHang.IsChecked;
        }

        private void LstCuaHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCuaHang.SelectedItem is CuaHangViewModel sel)
            {
                TxtSelectedCuaHang.Text = sel.Name;
                BtnToggleCuaHang.IsChecked = false;
            }
        }

        private async void BtnThemCuaHang_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new Window
            {
                Title = "Thêm Trụ sở / Cửa hàng mới",
                Width = 360,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 232, 245))
            };

            var grid = new Grid { Margin = new Thickness(12) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lbl = new TextBlock { Text = "Tên trụ sở / cửa hàng:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
            var txt = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10), VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetRow(lbl, 0);
            Grid.SetRow(txt, 1);

            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnSave = new Button { Content = "Lưu", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true, Background = System.Windows.Media.Brushes.White };
            var btnCancel = new Button { Content = "Đóng", Width = 75, Height = 26, IsCancel = true, Background = System.Windows.Media.Brushes.White };
            sp.Children.Add(btnSave);
            sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2);

            grid.Children.Add(lbl);
            grid.Children.Add(txt);
            grid.Children.Add(sp);
            inputWin.Content = grid;

            btnSave.Click += async (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên cửa hàng/trụ sở!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await _hoaDonService.InsertCuaHangAsync(txt.Text.Trim());
                inputWin.DialogResult = true;
                inputWin.Close();
            };

            if (inputWin.ShowDialog() == true)
            {
                await LoadCuaHangListAsync();
            }
        }

        private async void BtnTaiCuaHang_Click(object sender, RoutedEventArgs e)
        {
            await LoadCuaHangListAsync();
        }

        private void BtnDanhMucCuaHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Danh mục cửa hàng/trụ sở đã được liệt kê trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnInBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            string storeName = string.IsNullOrEmpty(TxtSelectedCuaHang.Text) ? "NÀNG HƯƠNG QUÁN" : TxtSelectedCuaHang.Text;
            var win = new ChonMauInWindow(DgHoaDon, "BÁO CÁO THỐNG KÊ DOANH THU", storeName, tuNgay, denNgay);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
                
                var list = await _hoaDonService.GetHoaDonListAsync(tuNgay, denNgay);
                DgHoaDon.ItemsSource = list;

                // Tính toán tổng hợp
                decimal tongTienHang = 0;
                decimal tongGiamGiaTienHang = 0;
                decimal tongTienMat = 0;
                decimal tongTienThe = 0;
                decimal tongTongDoanhThu = 0;

                foreach (var hd in list)
                {
                    tongTienHang += hd.TienHang;
                    tongGiamGiaTienHang += hd.TienGiamGia;
                    tongTienMat += hd.TienMat;
                    tongTienThe += hd.TheThanhToan;
                    tongTongDoanhThu += hd.TongCong;
                }

                TxtTienHang.Text = tongTienHang.ToString("N0");
                TxtTienGio.Text = "0";
                TxtGiamGiaTienHang.Text = tongGiamGiaTienHang > 0 ? ("-" + tongGiamGiaTienHang.ToString("N0")) : "-0";
                TxtGiamGiaTienGio.Text = "-0";
                TxtTongGiamGia.Text = tongGiamGiaTienHang > 0 ? ("-" + tongGiamGiaTienHang.ToString("N0")) : "-0";
                TxtPhiDichVu.Text = "0";
                TxtThue.Text = "0";
                TxtTongDoanhThu.Text = tongTongDoanhThu.ToString("N0");
                
                TxtTienMat.Text = tongTienMat.ToString("N0");
                TxtTienThe.Text = tongTienThe.ToString("N0");
                TxtChuyenKhoan.Text = "0";
                TxtTruTichLuy.Text = "0";
                TxtTheTraTruoc.Text = "0";
                TxtVoucher.Text = "0";
                TxtCongNo.Text = "0";
                TxtThuKhac.Text = "0";
                TxtChiKhac.Text = "0";
                
                decimal tongThucThu = tongTienMat + tongTienThe;
                TxtTongThucThu.Text = tongThucThu.ToString("N0");

                if (list.Count > 0)
                {
                    DgHoaDon.SelectedIndex = 0;
                }
                else
                {
                    DgChiTiet.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgHoaDon.SelectedItem is HoaDonViewModel selectedHoaDon)
            {
                try
                {
                    if (!string.IsNullOrEmpty(selectedHoaDon.Id))
                    {
                        var chiTietList = await _hoaDonService.GetChiTietHoaDonAsync(selectedHoaDon.Id);
                        DgChiTiet.ItemsSource = chiTietList;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region MENU CHUỘT PHẢI BẢNG HÓA ĐƠN (DgHoaDon)

        private void MnuHoaDon_SaoChepO_Click(object sender, RoutedEventArgs e)
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

        private void MnuHoaDon_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgHoaDon.SelectedItem is HoaDonViewModel item)
                {
                    string rowText = $"{item.SoPhieu}\t{item.Ngay:dd/MM/yyyy}\t{item.Ban}\t{item.BatDau:HH:mm}\t{item.KetThuc:HH:mm}\t{item.GioThanhToan:HH:mm}\t{item.KhachHang}\t{item.TienGiamGia:N0}\t{item.TongCong:N0}\t{item.ThanhToanBoi}\t{item.TienHang:N0}\t{item.GhiChu}\t{item.DiaChi}\t{item.MaKhach}\t{item.TiLeGiamGia}\t{item.DienGiai}\t{item.KhachDua:N0}\t{item.TraLai:N0}";
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
            var defaults = new List<string>
            {
                "Số phiếu", "Ngày", "Bàn", "Giờ vào", "Giờ ra", "Giờ thanh toán",
                "Khách hàng", "Tiền giảm giá", "Tổng cộng", "Thanh toán bởi", "Tiền hàng",
                "Ghi chú", "Địa chỉ", "Mã khách", "Tỉ lệ giảm giá", "Diễn giải", "Khách đưa", "Trả lại"
            };
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
                    FileName = $"ThongKeDoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgHoaDon?.ItemsSource as IEnumerable<HoaDonViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("Số phiếu,Ngày,Bàn,Giờ vào,Giờ ra,Giờ thanh toán,Khách hàng,Tiền giảm giá,Tổng cộng,Thanh toán bởi,Tiền hàng,Ghi chú,Địa chỉ,Mã khách,Tỉ lệ giảm giá,Diễn giải,Khách đưa,Trả lại");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.SoPhieu}\",\"{item.Ngay:dd/MM/yyyy}\",\"{item.Ban}\",\"{item.BatDau:HH:mm}\",\"{item.KetThuc:HH:mm}\",\"{item.GioThanhToan:HH:mm}\",\"{item.KhachHang}\",\"{item.TienGiamGia:N0}\",\"{item.TongCong:N0}\",\"{item.ThanhToanBoi}\",\"{item.TienHang:N0}\",\"{item.GhiChu}\",\"{item.DiaChi}\",\"{item.MaKhach}\",\"{item.TiLeGiamGia}\",\"{item.DienGiai}\",\"{item.KhachDua:N0}\",\"{item.TraLai:N0}\"");
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
            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            var win = new InLuoiWindow(DgHoaDon, $"DANH SÁCH HÓA ĐƠN ({tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy})");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion

        #region MENU CHUỘT PHẢI BẢNG CHI TIẾT MÓN (DgChiTiet)

        private void MnuChiTiet_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgChiTiet.CurrentCell != null && DgChiTiet.CurrentCell.Item is ChiTietHoaDonViewModel row)
                {
                    var col = DgChiTiet.CurrentCell.Column as DataGridBoundColumn;
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
                if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel sel)
                {
                    Clipboard.SetText(sel.TenMon ?? "");
                }
            }
            catch { }
        }

        private void MnuChiTiet_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
                {
                    string rowText = $"{item.Stt}\t{item.MaHang}\t{item.TenMon}\t{item.Dvt}\t{item.DonGia:N0}\t{item.PhanTramGiamGia:N0}\t{item.GhiChu}\t{item.SoLuong:0.##}\t{item.ThanhTien:N0}";
                    Clipboard.SetText(rowText);
                }
            }
            catch { }
        }

        private void MnuChiTiet_TuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgChiTiet.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MnuChiTiet_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "STT", "Mã hàng", "Mặt hàng", "ĐVT", "Đơn giá", "CK%", "Ghi chú", "S.lượng", "Thành tiền" };
            var win = new ChonCotHienThiWindow(DgChiTiet, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MnuChiTiet_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"ChiTietHoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgChiTiet?.ItemsSource as IEnumerable<ChiTietHoaDonViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,Mã hàng,Mặt hàng,ĐVT,Đơn giá,CK%,Ghi chú,S.lượng,Thành tiền");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.Stt}\",\"{item.MaHang}\",\"{item.TenMon}\",\"{item.Dvt}\",\"{item.DonGia:N0}\",\"{item.PhanTramGiamGia:N0}\",\"{item.GhiChu}\",\"{item.SoLuong:0.##}\",\"{item.ThanhTien:N0}\"");
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

        private void MnuChiTiet_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgChiTiet, "Chi tiết mặt hàng hóa đơn");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion
    }
}
