using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.QuanLyBanHang
{
    public partial class HoaDonNhaHangWindow : Window
    {
        private readonly string _idOrSoPhieu;
        private readonly LocalHoaDonService _hoaDonService;
        private readonly LocalSuDungDichVuService _dichVuService;

        private HoaDonViewModel _hoaDon;
        private ObservableCollection<ChiTietHoaDonViewModel> _chiTietList = new ObservableCollection<ChiTietHoaDonViewModel>();
        private ObservableCollection<PosNhomMatHangViewModel> _menuTreeList;
        private List<PosMatHangViewModel> _allMatHangList = new List<PosMatHangViewModel>();
        private string _selectedNhomId;

        public bool IsDataSaved { get; private set; }

        public HoaDonNhaHangWindow(string idOrSoPhieu)
        {
            InitializeComponent();
            _idOrSoPhieu = idOrSoPhieu;
            _hoaDonService = new LocalHoaDonService();
            _dichVuService = new LocalSuDungDichVuService();

            Loaded += async (s, e) => await InitializeWindowAsync();
        }

        private async Task InitializeWindowAsync()
        {
            try
            {
                await LoadHoaDonDataAsync();
                await LoadMenuTreeAsync();
                await LoadMatHangListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo màn hình: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadHoaDonDataAsync()
        {
            if (string.IsNullOrWhiteSpace(_idOrSoPhieu))
            {
                MessageBox.Show("Không tìm thấy thông tin hóa đơn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            _hoaDon = await _hoaDonService.GetHoaDonByIdOrSoPhieuAsync(_idOrSoPhieu);
            if (_hoaDon == null)
            {
                MessageBox.Show($"Không tìm thấy dữ liệu hóa đơn '{_idOrSoPhieu}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            // Tiêu đề
            Title = $"HÓA ĐƠN NHÀ HÀNG - SỬA";

            // Thông tin đầu phiếu
            TxtGioBatDau.Text = _hoaDon.BatDau.HasValue ? _hoaDon.BatDau.Value.ToString("dd/MM/yyyy HH:mm") : (_hoaDon.Ngay.HasValue ? _hoaDon.Ngay.Value.ToString("dd/MM/yyyy HH:mm") : "");
            TxtGioKetThuc.Text = _hoaDon.KetThuc.HasValue ? _hoaDon.KetThuc.Value.ToString("dd/MM/yyyy HH:mm") : (_hoaDon.GioThanhToan.HasValue ? _hoaDon.GioThanhToan.Value.ToString("dd/MM/yyyy HH:mm") : "");
            
            DpNgayOrder.SelectedDate = _hoaDon.Ngay ?? DateTime.Today;
            TxtSoPhieu.Text = _hoaDon.SoPhieu ?? "";
            TxtSoKhach.Text = _hoaDon.SoKhach > 0 ? _hoaDon.SoKhach.ToString() : "1";

            string khach = _hoaDon.KhachHang ?? "";
            if (!string.IsNullOrWhiteSpace(_hoaDon.MaKhach)) khach = $"[{_hoaDon.MaKhach}] " + khach;
            TxtKhachHang.Text = khach;

            TxtGhiChuTab.Text = _hoaDon.GhiChu ?? _hoaDon.DienGiai ?? "";

            // Chi tiết món
            var items = await _hoaDonService.GetChiTietHoaDonAsync(_hoaDon.Id);
            _chiTietList = new ObservableCollection<ChiTietHoaDonViewModel>(items);
            DgChiTiet.ItemsSource = _chiTietList;

            // Footer tổng kết
            TxtGiamGiaPt.Text = _hoaDon.TiLeGiamGia.ToString("N0");
            TxtGiamGia.Text = (_hoaDon.TienGiamGia + _hoaDon.TienGiamGiaGio).ToString("N0");

            CalculateTotals();
        }

        private async Task LoadMenuTreeAsync()
        {
            try
            {
                _menuTreeList = await _dichVuService.GetNhomMatHangTreeAsync();
                if (_menuTreeList != null && _menuTreeList.Count > 0)
                {
                    _menuTreeList[0].Icon = "🌐";
                    SetTreeExpandState(_menuTreeList, true);
                    _menuTreeList[0].IsSelected = true;
                    _selectedNhomId = null;
                }
                TvMenu.ItemsSource = _menuTreeList;

                // Tự động cuộn lên đầu danh mục
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var sv = FindVisualChild<ScrollViewer>(TvMenu);
                    sv?.ScrollToTop();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch { }
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

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child is T typedChild) return typedChild;
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        private async Task LoadMatHangListAsync()
        {
            try
            {
                _allMatHangList = await _dichVuService.GetMatHangListAsync(_selectedNhomId);
                FilterMatHangList();
            }
            catch { }
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

        private async void TvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is PosNhomMatHangViewModel item)
            {
                _selectedNhomId = string.IsNullOrEmpty(item.Id) ? null : item.Id;
                await LoadMatHangListAsync();
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterMatHangList();
        }

        private void CalculateTotals()
        {
            if (_chiTietList == null) return;

            decimal tienHang = _chiTietList.Sum(x => x.ThanhTien);
            decimal.TryParse(TxtGiamGiaPt?.Text?.Replace(",", "").Trim(), out decimal ptGiam);
            decimal.TryParse(TxtGiamGia?.Text?.Replace(",", "").Trim(), out decimal tienGiam);

            if (ptGiam > 0 && tienGiam <= 0)
            {
                tienGiam = Math.Round(tienHang * (ptGiam / 100m));
            }

            decimal tongCong = Math.Max(0, tienHang - tienGiam);

            if (TxtTienHang != null) TxtTienHang.Text = tienHang.ToString("N0");
            if (TxtTongCong != null) TxtTongCong.Text = tongCong.ToString("N0");
        }

        private void TxtGiamGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotals();
        }

        private void AddMatHangToOrder(PosMatHangViewModel mon, decimal soLuong)
        {
            if (mon == null) return;
            if (soLuong <= 0) soLuong = 1;

            var existing = _chiTietList.FirstOrDefault(x => x.MatHangId == mon.Id || (!string.IsNullOrEmpty(mon.Code) && x.MaHang == mon.Code));
            if (existing != null)
            {
                existing.SoLuong += soLuong;
                existing.ThanhTien = Math.Round(existing.SoLuong * existing.DonGia * (1 - (existing.PhanTramGiamGia / 100m)));
                DgChiTiet.Items.Refresh();
            }
            else
            {
                decimal donGia = mon.GiaBan ?? 0;
                var newItem = new ChiTietHoaDonViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    MatHangId = mon.Id,
                    MaHang = mon.Code,
                    TenMon = mon.Name,
                    Dvt = mon.DonViTinh,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    PhanTramGiamGia = 0,
                    ThanhTien = Math.Round(soLuong * donGia),
                    Stt = _chiTietList.Count + 1
                };
                _chiTietList.Add(newItem);
            }

            CalculateTotals();
        }

        private void DgMatHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgMatHang.SelectedItem is PosMatHangViewModel mon)
            {
                decimal.TryParse(TxtSoLuongChon?.Text?.Trim(), out decimal sl);
                if (sl <= 0) sl = 1;
                AddMatHangToOrder(mon, sl);
            }
        }

        private void BtnThemSangHoaDon_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang.SelectedItem is PosMatHangViewModel mon)
            {
                decimal.TryParse(TxtSoLuongChon?.Text?.Trim(), out decimal sl);
                if (sl <= 0) sl = 1;
                AddMatHangToOrder(mon, sl);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn món trong thực đơn để thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnGiamKhoiHoaDon_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                decimal.TryParse(TxtSoLuongChon?.Text?.Trim(), out decimal sl);
                if (sl <= 0) sl = 1;

                if (item.SoLuong > sl)
                {
                    item.SoLuong -= sl;
                    item.ThanhTien = Math.Round(item.SoLuong * item.DonGia * (1 - (item.PhanTramGiamGia / 100m)));
                    DgChiTiet.Items.Refresh();
                }
                else
                {
                    _chiTietList.Remove(item);
                }
                CalculateTotals();
            }
        }

        private void BtnXoaKhoiHoaDon_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                _chiTietList.Remove(item);
                CalculateTotals();
            }
        }

        private void BtnThemMon_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                item.SoLuong += 1;
                item.ThanhTien = Math.Round(item.SoLuong * item.DonGia * (1 - (item.PhanTramGiamGia / 100m)));
                DgChiTiet.Items.Refresh();
                CalculateTotals();
            }
        }

        private void BtnGiamMon_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong -= 1;
                    item.ThanhTien = Math.Round(item.SoLuong * item.DonGia * (1 - (item.PhanTramGiamGia / 100m)));
                    DgChiTiet.Items.Refresh();
                }
                else
                {
                    _chiTietList.Remove(item);
                }
                CalculateTotals();
            }
        }

        private void BtnXoaMonGrid_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                _chiTietList.Remove(item);
                CalculateTotals();
            }
        }

        private void BtnDatSl_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                var win = new InputWindow("Đặt số lượng", $"Nhập số lượng cho '{item.TenMon}':", item.SoLuong.ToString("0.##"));
                win.Owner = this;
                if (win.ShowDialog() == true && decimal.TryParse(win.InputText, out decimal newSl) && newSl > 0)
                {
                    item.SoLuong = newSl;
                    item.ThanhTien = Math.Round(item.SoLuong * item.DonGia * (1 - (item.PhanTramGiamGia / 100m)));
                    DgChiTiet.Items.Refresh();
                    CalculateTotals();
                }
            }
        }

        private void BtnDoiGia_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                var win = new InputWindow("Đổi đơn giá", $"Nhập đơn giá mới cho '{item.TenMon}':", item.DonGia.ToString("N0"));
                win.Owner = this;
                if (win.ShowDialog() == true && decimal.TryParse(win.InputText.Replace(",", ""), out decimal newGia) && newGia >= 0)
                {
                    item.DonGia = newGia;
                    item.ThanhTien = Math.Round(item.SoLuong * item.DonGia * (1 - (item.PhanTramGiamGia / 100m)));
                    DgChiTiet.Items.Refresh();
                    CalculateTotals();
                }
            }
        }

        private void BtnGhiChu_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                var win = new InputWindow("Ghi chú món", $"Ghi chú cho '{item.TenMon}':", item.GhiChu ?? "");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    item.GhiChu = win.InputText;
                    DgChiTiet.Items.Refresh();
                }
            }
        }

        private void BtnChietKhau_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonViewModel item)
            {
                var win = new InputWindow("Chiết khấu %", $"Nhập tỉ lệ chiết khấu (%) cho '{item.TenMon}':", item.PhanTramGiamGia.ToString("0.##"));
                win.Owner = this;
                if (win.ShowDialog() == true && decimal.TryParse(win.InputText, out decimal ck) && ck >= 0 && ck <= 100)
                {
                    item.PhanTramGiamGia = ck;
                    item.ThanhTien = Math.Round(item.SoLuong * item.DonGia * (1 - (ck / 100m)));
                    DgChiTiet.Items.Refresh();
                    CalculateTotals();
                }
            }
        }

        private void BtnInCheBien_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã gửi lệnh in chế biến cho nhà bếp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnInLaiBill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hoaDon == null) return;

                var banVm = new PosBanViewModel
                {
                    Id = _hoaDon.Id,
                    Name = _hoaDon.Ban ?? "Bàn",
                    SoPhieu = _hoaDon.SoPhieu,
                    StartTime = _hoaDon.BatDau ?? _hoaDon.Ngay,
                    KhachHangName = _hoaDon.KhachHang,
                    GhiChu = TxtGhiChuTab.Text,
                    TienHang = _hoaDon.TienHang,
                    GiamGia = _hoaDon.TienGiamGia,
                    GiamGiaPhanTram = _hoaDon.TiLeGiamGia,
                    TongCong = _hoaDon.TongCong
                };

                foreach (var c in _chiTietList)
                {
                    banVm.OrderItems.Add(new PosDonHangChiTietViewModel
                    {
                        Id = c.Id,
                        MatHangId = c.MatHangId,
                        MatHangName = c.TenMon,
                        DonViTinh = c.Dvt,
                        SoLuong = c.SoLuong,
                        DonGia = c.DonGia,
                        ChietKhauPhanTram = c.PhanTramGiamGia,
                        ThanhTien = c.ThanhTien,
                        GhiChu = c.GhiChu
                    });
                }

                var printWin = new HoaDonBanHangPrintWindow(banVm, isTamTinh: false);
                printWin.Owner = this;
                printWin.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            BtnLuu_Click(sender, e);
        }

        private async Task<bool> SaveDataAsync()
        {
            try
            {
                if (_hoaDon == null) return false;

                decimal tienHang = _chiTietList.Sum(x => x.ThanhTien);
                decimal.TryParse(TxtGiamGiaPt?.Text?.Replace(",", "").Trim(), out decimal ptGiam);
                decimal.TryParse(TxtGiamGia?.Text?.Replace(",", "").Trim(), out decimal tienGiam);
                decimal tongCong = Math.Max(0, tienHang - tienGiam);
                int.TryParse(TxtSoKhach?.Text?.Trim(), out int soKhach);

                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        // 1. Cập nhật TDONHANG
                        string sqlUpdateHd = @"
                            UPDATE TDONHANG 
                            SET TIENHANG = @TienHang, 
                                TILEGIAMGIA = @TiLeGiamGia, 
                                TIENGIAMGIA = @TienGiamGia, 
                                TONGCONG = @TongCong,
                                SOKHACH = @SoKhach,
                                NOTE = @Note
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(sqlUpdateHd, new
                        {
                            TienHang = tienHang,
                            TiLeGiamGia = ptGiam,
                            TienGiamGia = tienGiam,
                            TongCong = tongCong,
                            SoKhach = soKhach,
                            Note = TxtGhiChuTab.Text?.Trim(),
                            Id = _hoaDon.Id
                        }, trans);

                        trans.Commit();
                    }
                }

                IsDataSaved = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu thông tin hóa đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                BtnInLaiBill_Click(sender, e);
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                BtnInLaiBill_Click(sender, e);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.F8)
            {
                BtnInLaiBill_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnThanhToan_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                BtnLuu_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
            {
                BtnLuuVaThoat_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                BtnInLaiBill_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
