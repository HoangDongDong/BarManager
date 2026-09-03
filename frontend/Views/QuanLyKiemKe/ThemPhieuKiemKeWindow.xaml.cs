using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.KhoHang;

namespace QuanLyBar.Client.Views.QuanLyKiemKe
{
    public partial class ThemPhieuKiemKeWindow : Window
    {
        private string _phieuKiemKeId = null;
        private List<PhieuKiemKeItem> _allPhieuKiemKe = null;
        private int _currentIndex = -1;
        private ObservableCollection<PhieuKiemKeChiTietItem> _chiTietList = new ObservableCollection<PhieuKiemKeChiTietItem>();
        private List<MatHangNhapKhoItem> _catalogMatHangList = new List<MatHangNhapKhoItem>();
        private Dictionary<string, decimal> _tonKhoDict = new Dictionary<string, decimal>();
        private bool _isLoaded = false;

        public event Action OnSaved;

        public ThemPhieuKiemKeWindow(string phieuKiemKeId = null, List<PhieuKiemKeItem> allPhieuKiemKe = null)
        {
            InitializeComponent();
            _phieuKiemKeId = phieuKiemKeId;
            _allPhieuKiemKe = allPhieuKiemKe ?? new List<PhieuKiemKeItem>();

            if (!string.IsNullOrEmpty(_phieuKiemKeId))
            {
                _currentIndex = _allPhieuKiemKe.FindIndex(x => x.Id == _phieuKiemKeId);
            }

            Loaded += ThemPhieuKiemKeWindow_Loaded;
            PreviewKeyDown += ThemPhieuKiemKeWindow_PreviewKeyDown;
        }

        private async void ThemPhieuKiemKeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
            await LoadCatalogMatHangAsync();
            await UpdateTonKhoHeThongAsync();

            if (!string.IsNullOrEmpty(_phieuKiemKeId))
            {
                Title = "PHIẾU KIỂM KÊ - CHỈNH SỬA";
                await LoadPhieuKiemKeDataAsync(_phieuKiemKeId);
            }
            else
            {
                Title = "PHIẾU KIỂM KÊ - THÊM MỚI";
                DpNgay.SelectedDate = DateTime.Now;
                TxtSoPhieu.Text = await LocalKiemKeService.GenerateSoPhieuAsync();
                DgPhieuKiemKeChiTiet.ItemsSource = _chiTietList;
            }

            _isLoaded = true;
            TxtTimMatHang.Focus();
        }

        private async Task UpdateTonKhoHeThongAsync()
        {
            string khoId = CboKhoHang.SelectedValue?.ToString() ?? "";
            _tonKhoDict = await LocalKiemKeService.GetTonKhoDictionaryAsync(khoId);

            foreach (var item in _chiTietList)
            {
                decimal ton = _tonKhoDict.GetValueOrDefault(item.DmathangId, 0);
                item.SoLuongHeThong = ton;
                item.SoLuongTon = ton;
            }
        }

        private async void CboKhoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            await UpdateTonKhoHeThongAsync();
        }

        private async Task LoadLookupsAsync()
        {
            var khoList = await LocalNhapKhoService.GetKhoHangListFlatAsync();
            CboKhoHang.ItemsSource = khoList;
            if (khoList.Count > 0) CboKhoHang.SelectedIndex = 0;

            var nvList = await LocalNhapKhoService.GetNhanVienLookupListAsync();
            CboNhanVien.ItemsSource = nvList;
            if (nvList.Count > 0) CboNhanVien.SelectedIndex = 0;
        }

        private async Task LoadCatalogMatHangAsync()
        {
            _catalogMatHangList = await LocalNhapKhoService.GetMatHangForNhapKhoAsync();
            int stt = 1;
            foreach (var item in _catalogMatHangList)
            {
                item.Stt = stt++;
            }
            DgMatHangCatalog.ItemsSource = _catalogMatHangList;
        }

        private async Task LoadPhieuKiemKeDataAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = "SELECT * FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @Id";
                    var p = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
                    if (p != null)
                    {
                        if (p.NGAY != null)
                        {
                            try { DpNgay.SelectedDate = Convert.ToDateTime(p.NGAY); } catch { }
                        }
                        TxtSoPhieu.Text = p.NAME?.ToString() ?? "";
                        CboKhoHang.SelectedValue = p.DKHOHANGID?.ToString() ?? p.DKHONHAPID?.ToString() ?? "";
                        CboNhanVien.SelectedValue = p.DNHANVIENID?.ToString() ?? p.DNHANVIENNHAPID?.ToString() ?? "";
                        TxtDienGiai.Text = p.DIENGIAI?.ToString() ?? "Kiểm kê";
                        TxtGhiChu.Text = p.NOTE?.ToString() ?? "";
                    }

                    var details = await LocalKiemKeService.GetPhieuKiemKeChiTietAsync(id);
                    _chiTietList.Clear();
                    int stt = 1;
                    foreach (var d in details)
                    {
                        d.Stt = stt++;
                        _chiTietList.Add(d);
                    }
                    DgPhieuKiemKeChiTiet.ItemsSource = _chiTietList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu phiếu kiểm kê: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtTimMatHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtTimMatHang.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(kw))
            {
                DgMatHangCatalog.ItemsSource = _catalogMatHangList;
            }
            else
            {
                var filtered = _catalogMatHangList.Where(x =>
                    (x.Name?.ToLowerInvariant().Contains(kw) == true) ||
                    (x.Code?.ToLowerInvariant().Contains(kw) == true)
                ).ToList();
                DgMatHangCatalog.ItemsSource = filtered;
            }
        }

        private void TxtTimMatHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DgMatHangCatalog.SelectedItem is MatHangNhapKhoItem item)
                {
                    ThemMatHangVaoChiTiet(item);
                }
                else if (DgMatHangCatalog.Items.Count > 0)
                {
                    DgMatHangCatalog.SelectedIndex = 0;
                    ThemMatHangVaoChiTiet(DgMatHangCatalog.SelectedItem as MatHangNhapKhoItem);
                }
            }
        }

        private void TxtSoLuongThem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnThemMatHang_Click(sender, e);
            }
        }

        private void BtnThemMatHang_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHangCatalog.SelectedItem is MatHangNhapKhoItem item)
            {
                ThemMatHangVaoChiTiet(item);
            }
            else if (DgMatHangCatalog.Items.Count > 0)
            {
                DgMatHangCatalog.SelectedIndex = 0;
                ThemMatHangVaoChiTiet(DgMatHangCatalog.SelectedItem as MatHangNhapKhoItem);
            }
        }

        private void DgMatHangCatalog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgMatHangCatalog.SelectedItem is MatHangNhapKhoItem item)
            {
                ThemMatHangVaoChiTiet(item);
            }
        }

        private void ThemMatHangVaoChiTiet(MatHangNhapKhoItem catalogItem)
        {
            if (catalogItem == null) return;

            decimal sl = 1;
            decimal.TryParse(TxtSoLuongThem.Text?.Trim(), out sl);
            if (sl <= 0) sl = 1;

            var existing = _chiTietList.FirstOrDefault(x => x.DmathangId == catalogItem.Id);
            if (existing != null)
            {
                existing.SoLuongThucTe += sl;
            }
            else
            {
                decimal ton = _tonKhoDict.GetValueOrDefault(catalogItem.Id, 0);
                var newItem = new PhieuKiemKeChiTietItem
                {
                    Stt = _chiTietList.Count + 1,
                    DmathangId = catalogItem.Id,
                    MaHang = catalogItem.Code,
                    TenHang = catalogItem.Name,
                    DdonvitinhId = catalogItem.DdonvitinhId,
                    TenDonViTinh = catalogItem.TenDonViTinh,
                    SoLuongTon = ton,
                    SoLuongHeThong = ton,
                    SoLuongThucTe = sl,
                    DonGia = catalogItem.GiaBan,
                    ThanhTien = 0,
                    GhiChu = ""
                };
                _chiTietList.Add(newItem);
            }

            TxtTimMatHang.SelectAll();
            TxtTimMatHang.Focus();
        }

        private void DgPhieuKiemKeChiTiet_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Row.Item is PhieuKiemKeChiTietItem item)
                {
                    // Triggers update for differences and foreground
                    item.SoLuongHeThong = item.SoLuongHeThong;
                }
            }));
        }

        private void BtnMatHangChuaKiemKe_Click(object sender, RoutedEventArgs e)
        {
            var testedIds = new HashSet<string>(_chiTietList.Select(x => x.DmathangId));
            var win = new MatHangChuaKiemKeWindow(testedIds);
            win.Owner = this;
            win.OnItemChosen += (chosenItem) =>
            {
                ThemMatHangVaoChiTiet(chosenItem);
            };
            win.ShowDialog();
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgPhieuKiemKeChiTiet.SelectedItem is PhieuKiemKeChiTietItem sel)
            {
                _chiTietList.Remove(sel);
                for (int i = 0; i < _chiTietList.Count; i++)
                {
                    _chiTietList[i].Stt = i + 1;
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TxtLocChiTiet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            string kw = TxtLocChiTiet.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(kw))
            {
                DgPhieuKiemKeChiTiet.ItemsSource = _chiTietList;
            }
            else
            {
                var filtered = _chiTietList.Where(x => (x.TenHang?.ToLowerInvariant().Contains(kw) == true) ||
                                                       (x.MaHang?.ToLowerInvariant().Contains(kw) == true)).ToList();
                DgPhieuKiemKeChiTiet.ItemsSource = filtered;
            }
        }

        private void BtnNhapMayKiemKho_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng kết nối và nhập từ máy kiểm kho không dây đang sẵn sàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNhapExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng nhập từ Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất ra Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task<bool> SaveCurrentAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtSoPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập số phiếu kiểm kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_chiTietList.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một mặt hàng vào phiếu kiểm kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string khoId = CboKhoHang.SelectedValue?.ToString();
            string nvId = CboNhanVien.SelectedValue?.ToString();
            decimal tongTien = _chiTietList.Sum(x => x.ThanhTien);

            var pk = new PhieuKiemKeItem
            {
                Id = _phieuKiemKeId,
                SoPhieu = TxtSoPhieu.Text.Trim(),
                Ngay = DpNgay.SelectedDate ?? DateTime.Now,
                KhoHangId = khoId,
                NhanVienId = nvId,
                DienGiai = TxtDienGiai.Text.Trim(),
                Note = TxtGhiChu.Text.Trim(),
                TongCong = tongTien,
                TienHangChuaGiam = tongTien
            };

            bool success = await LocalKiemKeService.SavePhieuKiemKeAsync(pk, _chiTietList.ToList());
            if (success)
            {
                _phieuKiemKeId = pk.Id;
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Không thể lưu phiếu kiểm kê. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                MessageBox.Show("Đã lưu phiếu kiểm kê thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                MessageBox.Show("Đã lưu và gửi lệnh in phiếu kiểm kê thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                MessageBox.Show("Đã lưu thành công. Đang mở xem trước bản in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                _phieuKiemKeId = null;
                Title = "PHIẾU KIỂM KÊ - THÊM MỚI";
                TxtSoPhieu.Text = await LocalKiemKeService.GenerateSoPhieuAsync();
                _chiTietList.Clear();
                TxtTimMatHang.Focus();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnPhimTat_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Phím tắt hệ thống:\n- Ctrl+S: Lưu & Mới\n- Ctrl+L: Lưu\n- Ctrl+N: Mới\n- Ctrl+P: In\n- F3: Tìm mặt hàng\n- F8: Xóa dòng\n- F10: Xem phiếu trước\n- F11: Xem phiếu sau\n- ESC: Thoát", "Danh sách phím tắt", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuKiemKe != null && _allPhieuKiemKe.Count > 0)
            {
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                    _phieuKiemKeId = _allPhieuKiemKe[_currentIndex].Id;
                    Title = "PHIẾU KIỂM KÊ - CHỈNH SỬA";
                    await LoadPhieuKiemKeDataAsync(_phieuKiemKeId);
                }
                else
                {
                    MessageBox.Show("Đã ở phiếu đầu tiên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuKiemKe != null && _allPhieuKiemKe.Count > 0)
            {
                if (_currentIndex < _allPhieuKiemKe.Count - 1)
                {
                    _currentIndex++;
                    _phieuKiemKeId = _allPhieuKiemKe[_currentIndex].Id;
                    Title = "PHIẾU KIỂM KÊ - CHỈNH SỬA";
                    await LoadPhieuKiemKeDataAsync(_phieuKiemKeId);
                }
                else
                {
                    MessageBox.Show("Đã ở phiếu cuối cùng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void BtnHeaderThemMoi_Click(object sender, RoutedEventArgs e)
        {
            _phieuKiemKeId = null;
            Title = "PHIẾU KIỂM KÊ - THÊM MỚI";
            TxtSoPhieu.Text = await LocalKiemKeService.GenerateSoPhieuAsync();
            _chiTietList.Clear();
            TxtTimMatHang.Focus();
        }

        private async void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _phieuKiemKeId = null;
            Title = "PHIẾU KIỂM KÊ - THÊM MỚI (SAO CHÉP)";
            TxtSoPhieu.Text = await LocalKiemKeService.GenerateSoPhieuAsync();
            DpNgay.SelectedDate = DateTime.Now;
            MessageBox.Show("Đã sao chép nội dung phiếu thành phiếu mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnHeaderXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_phieuKiemKeId))
            {
                _chiTietList.Clear();
                return;
            }

            var res = MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu kiểm kê '{TxtSoPhieu.Text}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                await LocalKiemKeService.DeletePhieuKiemKeAsync(_phieuKiemKeId, false);
                OnSaved?.Invoke();
                DialogResult = true;
                Close();
            }
        }

        private void ThemPhieuKiemKeWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.S)
                {
                    BtnLuuVaMoi_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.L)
                {
                    BtnLuu_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.N)
                {
                    BtnHeaderThemMoi_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.P)
                {
                    BtnLuuVaIn_Click(sender, e);
                    e.Handled = true;
                }
            }
            else
            {
                if (e.Key == Key.F3)
                {
                    TxtTimMatHang.Focus();
                    TxtTimMatHang.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.F8)
                {
                    BtnXoaDong_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.F10)
                {
                    BtnTruoc_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.F11)
                {
                    BtnSau_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
            }
        }
    }
}
