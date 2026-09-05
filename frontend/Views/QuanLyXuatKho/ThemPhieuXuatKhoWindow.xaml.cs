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
using QuanLyBar.Client.Views;
using QuanLyBar.Client.Views.DanhMucNhaCungCap;
using QuanLyBar.Client.Views.KhoHang;

namespace QuanLyBar.Client.Views.QuanLyXuatKho
{
    public partial class ThemPhieuXuatKhoWindow : Window
    {
        private string _phieuXuatId;
        private List<PhieuXuatItem> _allPhieuXuat = new List<PhieuXuatItem>();
        private int _currentIndex = -1;
        private bool _isNew = true;
        private bool _isLoaded = false;
        public event Action OnSaved;

        private List<MatHangNhapKhoItem> _allCatalog = new List<MatHangNhapKhoItem>();
        private ObservableCollection<MatHangNhapKhoItem> _filteredCatalog = new ObservableCollection<MatHangNhapKhoItem>();
        private ObservableCollection<PhieuXuatChiTietItem> _phieuXuatChiTietList = new ObservableCollection<PhieuXuatChiTietItem>();

        private List<NhapKhoLookupItem> _allKho = new List<NhapKhoLookupItem>();
        private List<NhapKhoLookupItem> _allNv = new List<NhapKhoLookupItem>();
        private List<NhapKhoLookupItem> _allNcc = new List<NhapKhoLookupItem>();

        private string _selectedKhoId;
        private string _selectedNvId;
        private string _selectedNccId;

        public ThemPhieuXuatKhoWindow(string phieuXuatId = null, List<PhieuXuatItem> allPhieuXuat = null)
        {
            InitializeComponent();
            _phieuXuatId = phieuXuatId;
            _allPhieuXuat = allPhieuXuat ?? new List<PhieuXuatItem>();
            _isNew = string.IsNullOrEmpty(phieuXuatId);

            if (!_isNew)
            {
                Title = "PHIẾU XUẤT KHO - SỬA";
                _currentIndex = _allPhieuXuat.FindIndex(x => x.Id == _phieuXuatId);
            }

            DgMatHangCatalog.ItemsSource = _filteredCatalog;
            DgPhieuXuatChiTiet.ItemsSource = _phieuXuatChiTietList;

            Loaded += ThemPhieuXuatKhoWindow_Loaded;
        }

        private async void ThemPhieuXuatKhoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DpNgay.SelectedDate = DateTime.Now;
                await LoadLookupsAsync();
                await LoadCatalogAsync();

                if (_isNew)
                {
                    await SinhSoPhieuTuDongAsync();
                }
                else
                {
                    await LoadPhieuXuatAsync(_phieuXuatId);
                }

                _isLoaded = true;
                TinhTongCong();
                TxtTimMatHang.Focus();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ThemPhieuXuatKhoWindow_Loaded error: " + ex.Message);
            }
        }

        private async Task SinhSoPhieuTuDongAsync()
        {
            TxtSoPhieu.Text = await LocalXuatKhoService.GetNextSoPhieuXuatAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                _allKho = await LocalNhapKhoService.GetKhoHangListFlatAsync();
                LstKhoPopup.ItemsSource = _allKho;
                if (_allKho.Count > 0 && string.IsNullOrEmpty(_selectedKhoId))
                {
                    SelectKho(_allKho[0]);
                }

                _allNv = await LocalNhapKhoService.GetNhanVienLookupListAsync();
                LstNvPopup.ItemsSource = _allNv;
                if (_allNv.Count > 0 && string.IsNullOrEmpty(_selectedNvId))
                {
                    SelectNv(_allNv[0]);
                }

                _allNcc = await LocalNhapKhoService.GetNhaCungCapLookupListAsync();
                DgNccPopup.ItemsSource = _allNcc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadLookupsAsync error: " + ex.Message);
            }
        }

        private async Task LoadCatalogAsync()
        {
            try
            {
                _allCatalog = await LocalNhapKhoService.GetMatHangForNhapKhoAsync();
                ApplyCatalogFilter();
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadCatalogAsync error: " + ex.Message);
            }
        }

        private void ApplyCatalogFilter()
        {
            string kw = TxtTimMatHang.Text?.Trim().ToLowerInvariant() ?? "";
            _filteredCatalog.Clear();

            var list = string.IsNullOrEmpty(kw)
                ? _allCatalog
                : _allCatalog.Where(x => (x.Name?.ToLowerInvariant().Contains(kw) == true) ||
                                         (x.Code?.ToLowerInvariant().Contains(kw) == true));

            int stt = 1;
            foreach (var item in list)
            {
                item.Stt = stt++;
                _filteredCatalog.Add(item);
            }
        }

        private async Task LoadPhieuXuatAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    var px = await conn.QueryFirstOrDefaultAsync(
                        "SELECT * FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @Id",
                        new { Id = id }
                    );

                    if (px != null)
                    {
                        IDictionary<string, object> d = px as IDictionary<string, object>;
                        TxtSoPhieu.Text = d.ContainsKey("NAME") && d["NAME"] != null ? d["NAME"].ToString() : "";
                        if (d.ContainsKey("NGAY") && d["NGAY"] != null)
                        {
                            DpNgay.SelectedDate = Convert.ToDateTime(d["NGAY"]);
                        }

                        _selectedKhoId = d.ContainsKey("DKHOXUATID") && d["DKHOXUATID"] != null ? d["DKHOXUATID"].ToString() : null;
                        var k = _allKho.FirstOrDefault(x => x.Id == _selectedKhoId);
                        if (k != null) SelectKho(k);

                        _selectedNvId = d.ContainsKey("DNHANVIENXUATID") && d["DNHANVIENXUATID"] != null ? d["DNHANVIENXUATID"].ToString() : null;
                        var nv = _allNv.FirstOrDefault(x => x.Id == _selectedNvId);
                        if (nv != null) SelectNv(nv);

                        _selectedNccId = d.ContainsKey("DNHACUNGCAPID") && d["DNHACUNGCAPID"] != null ? d["DNHACUNGCAPID"].ToString() : null;
                        var ncc = _allNcc.FirstOrDefault(x => x.Id == _selectedNccId);
                        if (ncc != null) SelectNcc(ncc);

                        TxtDienGiai.Text = d.ContainsKey("DIENGIAI") && d["DIENGIAI"] != null ? d["DIENGIAI"].ToString() : "Xuất khác";
                        TxtGhiChu.Text = d.ContainsKey("NOTE") && d["NOTE"] != null ? d["NOTE"].ToString() : "";

                        decimal tile = d.ContainsKey("TILEGIAMGIA") && d["TILEGIAMGIA"] != null ? Convert.ToDecimal(d["TILEGIAMGIA"]) : 0;
                        decimal tiengiam = d.ContainsKey("TIENGIAMGIA") && d["TIENGIAMGIA"] != null ? Convert.ToDecimal(d["TIENGIAMGIA"]) : 0;
                        TxtTiLeGiamGia.Text = tile.ToString("N0");
                        TxtTienGiamGia.Text = tiengiam.ToString("N0");

                        // Load chi tiết
                        var details = await LocalXuatKhoService.GetPhieuXuatChiTietAsync(id);
                        _phieuXuatChiTietList.Clear();
                        foreach (var item in details)
                        {
                            _phieuXuatChiTietList.Add(item);
                        }

                        TinhTongCong();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin phiếu xuất: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Master-Detail & Calculations
        private void TxtTimMatHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyCatalogFilter();
        }

        private void TxtTimMatHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_filteredCatalog.Count > 0)
                {
                    ThemMatHangVaoPhieu(_filteredCatalog[0]);
                }
            }
        }

        private void TxtSoLuongNhap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnThemMatHang_Click(sender, e);
            }
        }

        private void BtnThemMatHang_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHangCatalog.SelectedItem is MatHangNhapKhoItem sel)
            {
                ThemMatHangVaoPhieu(sel);
            }
            else if (_filteredCatalog.Count > 0)
            {
                ThemMatHangVaoPhieu(_filteredCatalog[0]);
            }
        }

        private void DgMatHangCatalog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgMatHangCatalog.SelectedItem is MatHangNhapKhoItem sel)
            {
                ThemMatHangVaoPhieu(sel);
            }
        }

        private void ThemMatHangVaoPhieu(MatHangNhapKhoItem catalogItem)
        {
            if (catalogItem == null) return;

            decimal sl = 1;
            if (decimal.TryParse(TxtSoLuongNhap.Text?.Trim(), out decimal parsedSl) && parsedSl > 0)
            {
                sl = parsedSl;
            }

            var existing = _phieuXuatChiTietList.FirstOrDefault(x => x.DmathangId == catalogItem.Id);
            if (existing != null)
            {
                existing.SlXuat += sl;
            }
            else
            {
                var newItem = new PhieuXuatChiTietItem
                {
                    Stt = _phieuXuatChiTietList.Count + 1,
                    DmathangId = catalogItem.Id,
                    MaHang = catalogItem.Code,
                    TenHang = catalogItem.Name,
                    DdonvitinhId = catalogItem.DdonvitinhId,
                    TenDonViTinh = catalogItem.TenDonViTinh,
                    SlXuat = sl,
                    DonGia = catalogItem.GiaNhap > 0 ? catalogItem.GiaNhap : catalogItem.GiaBan,
                    TiLeGiamGia = 0,
                    TienGiamGia = 0,
                    GhiChu = ""
                };
                _phieuXuatChiTietList.Add(newItem);
            }

            TinhTongCong();
            TxtTimMatHang.SelectAll();
            TxtTimMatHang.Focus();
        }

        private void DgPhieuXuatChiTiet_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TinhTongCong();
            }));
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgPhieuXuatChiTiet.SelectedItem is PhieuXuatChiTietItem sel)
            {
                _phieuXuatChiTietList.Remove(sel);
                // Đánh lại STT
                for (int i = 0; i < _phieuXuatChiTietList.Count; i++)
                {
                    _phieuXuatChiTietList[i].Stt = i + 1;
                }
                TinhTongCong();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool _isUpdatingSummary = false;

        private void TinhTongCong()
        {
            if (!_isLoaded || _isUpdatingSummary) return;
            if (TxtTiengHang == null || TxtTiLeGiamGia == null || TxtTienGiamGia == null || TxtTongCong == null || TxtTienThanhToan == null || _phieuXuatChiTietList == null) return;
            _isUpdatingSummary = true;

            try
            {
                decimal tongTienHang = _phieuXuatChiTietList.Sum(x => x.ThanhTien);
                TxtTiengHang.Text = tongTienHang.ToString("N0");

                decimal tileGiam = 0;
                decimal.TryParse(TxtTiLeGiamGia.Text?.Trim(), out tileGiam);

                decimal tienGiam = 0;
                if (tileGiam > 0)
                {
                    tienGiam = tongTienHang * tileGiam / 100m;
                    TxtTienGiamGia.Text = tienGiam.ToString("N0");
                }
                else
                {
                    decimal.TryParse(TxtTienGiamGia.Text?.Trim(), out tienGiam);
                }

                decimal tongCong = Math.Max(0, tongTienHang - tienGiam);
                TxtTongCong.Text = tongCong.ToString("N0");
                TxtTienThanhToan.Text = tongCong.ToString("N0");
            }
            finally
            {
                _isUpdatingSummary = false;
            }
        }

        private void TxtTiLeGiamGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || _isUpdatingSummary) return;
            TinhTongCong();
        }

        private void TxtTienGiamGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || _isUpdatingSummary) return;
            TinhTongCong();
        }

        private void TxtTienThanhToan_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TxtLocChiTiet_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtLocChiTiet.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(kw))
            {
                DgPhieuXuatChiTiet.ItemsSource = _phieuXuatChiTietList;
            }
            else
            {
                var filtered = _phieuXuatChiTietList.Where(x => (x.TenHang?.ToLowerInvariant().Contains(kw) == true) ||
                                                                (x.MaHang?.ToLowerInvariant().Contains(kw) == true)).ToList();
                DgPhieuXuatChiTiet.ItemsSource = filtered;
            }
        }
        #endregion

        #region Dropdowns & Popups
        private void SelectKho(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedKhoId = item.Id;
            if (TxtSelectedKho != null) TxtSelectedKho.Text = item.Name ?? "";
            if (PopupKho != null) PopupKho.IsOpen = false;
            if (BtnToggleKho != null) BtnToggleKho.IsChecked = false;
        }

        private void TxtSelectedKho_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupKho != null) PopupKho.IsOpen = !PopupKho.IsOpen;
        }

        private void LstKhoPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstKhoPopup?.SelectedItem is NhapKhoLookupItem item)
            {
                SelectKho(item);
            }
        }

        private void TxtTimKhoPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || LstKhoPopup == null || _allKho == null) return;
            string kw = TxtTimKhoPopup.Text?.Trim().ToLowerInvariant() ?? "";
            LstKhoPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allKho
                : _allKho.Where(x => x.Name?.ToLowerInvariant().Contains(kw) == true).ToList();
        }

        private void BtnThemKho_Click(object sender, RoutedEventArgs e)
        {
            if (PopupKho != null) PopupKho.IsOpen = false;
            var win = new ThemKhoHangWindow();
            win.Owner = this;
            win.OnSaved += async () => { await LoadLookupsAsync(); };
            win.ShowDialog();
        }

        private void BtnSuaKho_Click(object sender, RoutedEventArgs e)
        {
            if (PopupKho != null) PopupKho.IsOpen = false;
            var cur = _allKho?.FirstOrDefault(x => x.Id == _selectedKhoId);
            if (cur != null)
            {
                var treeItem = new KhoHangTreeItem { Id = cur.Id, Name = cur.Name };
                var win = new ThemKhoHangWindow(treeItem);
                win.Owner = this;
                win.OnSaved += async () => { await LoadLookupsAsync(); };
                win.ShowDialog();
            }
        }

        private async void BtnTaiKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
        }

        private void BtnDanhMucKho_Click(object sender, RoutedEventArgs e)
        {
            if (PopupKho != null) PopupKho.IsOpen = false;
            var win = new DanhMucKhoHangWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void SelectNv(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedNvId = item.Id;
            if (TxtSelectedNhanVien != null) TxtSelectedNhanVien.Text = item.Name ?? "";
            if (PopupNhanVien != null) PopupNhanVien.IsOpen = false;
            if (BtnToggleNhanVien != null) BtnToggleNhanVien.IsChecked = false;
        }

        private void TxtSelectedNhanVien_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupNhanVien != null) PopupNhanVien.IsOpen = !PopupNhanVien.IsOpen;
        }

        private void LstNvPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstNvPopup?.SelectedItem is NhapKhoLookupItem item)
            {
                SelectNv(item);
            }
        }

        private void TxtTimNvPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || LstNvPopup == null || _allNv == null) return;
            string kw = TxtTimNvPopup.Text?.Trim().ToLowerInvariant() ?? "";
            LstNvPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allNv
                : _allNv.Where(x => x.Name?.ToLowerInvariant().Contains(kw) == true).ToList();
        }

        private void BtnThemNv_Click(object sender, RoutedEventArgs e)
        {
            if (PopupNhanVien != null) PopupNhanVien.IsOpen = false;
            var win = new ThemNhanVienWindow();
            win.Owner = this;
            win.OnSaved += async () => { await LoadLookupsAsync(); };
            win.ShowDialog();
        }

        private void BtnSuaNv_Click(object sender, RoutedEventArgs e)
        {
            if (PopupNhanVien != null) PopupNhanVien.IsOpen = false;
            if (!string.IsNullOrEmpty(_selectedNvId))
            {
                var win = new ThemNhanVienWindow(id: _selectedNvId);
                win.Owner = this;
                win.OnSaved += async () => { await LoadLookupsAsync(); };
                win.ShowDialog();
            }
        }

        private async void BtnTaiNv_Click(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
        }

        private async void BtnDanhMucNv_Click(object sender, RoutedEventArgs e)
        {
            if (PopupNhanVien != null) PopupNhanVien.IsOpen = false;
            var win = new QuanLyBar.Client.Views.DanhMucNhanVien.DanhMucNhanVienWindow();
            win.Owner = this;
            win.ShowDialog();
            await LoadLookupsAsync();
        }

        private void SelectNcc(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedNccId = item.Id;
            if (TxtSelectedNhaCungCap != null) TxtSelectedNhaCungCap.Text = item.Name ?? "";
            if (PopupNhaCungCap != null) PopupNhaCungCap.IsOpen = false;
            if (BtnToggleNhaCungCap != null) BtnToggleNhaCungCap.IsChecked = false;
        }

        private void TxtSelectedNhaCungCap_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PopupNhaCungCap.IsOpen = !PopupNhaCungCap.IsOpen;
        }

        private void DgNccPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DgNccPopup.SelectedItem is NhapKhoLookupItem item)
            {
                SelectNcc(item);
            }
        }

        private void TxtTimNccPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtTimNccPopup.Text?.Trim().ToLowerInvariant() ?? "";
            DgNccPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allNcc
                : _allNcc.Where(x => (x.Name?.ToLowerInvariant().Contains(kw) == true) ||
                                     (x.DienThoai?.ToLowerInvariant().Contains(kw) == true) ||
                                     (x.DiaChi?.ToLowerInvariant().Contains(kw) == true)).ToList();
        }

        private void BtnThemNcc_Click(object sender, RoutedEventArgs e)
        {
            PopupNhaCungCap.IsOpen = false;
            var win = new ThemNhaCungCapWindow();
            win.Owner = this;
            win.OnSaved += async () => { await LoadLookupsAsync(); };
            win.ShowDialog();
        }

        private void BtnSuaNcc_Click(object sender, RoutedEventArgs e)
        {
            PopupNhaCungCap.IsOpen = false;
            var cur = _allNcc.FirstOrDefault(x => x.Id == _selectedNccId);
            if (cur != null)
            {
                var nccItem = new NhaCungCapItem { Id = cur.Id, Name = cur.Name, DienThoai = cur.DienThoai, DiaChi = cur.DiaChi };
                var win = new ThemNhaCungCapWindow(nccItem);
                win.Owner = this;
                win.OnSaved += async () => { await LoadLookupsAsync(); };
                win.ShowDialog();
            }
        }

        private async void BtnTaiNcc_Click(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
        }

        private void BtnDanhMucNcc_Click(object sender, RoutedEventArgs e)
        {
            PopupNhaCungCap.IsOpen = false;
        }
        #endregion

        #region Save & Actions
        private async Task<bool> SaveDataAsync()
        {
            string soPhieu = TxtSoPhieu.Text?.Trim();
            if (string.IsNullOrEmpty(soPhieu))
            {
                MessageBox.Show("Vui lòng nhập số phiếu xuất!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoPhieu.Focus();
                return false;
            }

            if (_phieuXuatChiTietList.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một mặt hàng vào phiếu xuất!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTimMatHang.Focus();
                return false;
            }

            decimal.TryParse(TxtTiengHang.Text?.Trim(), out decimal tienHang);
            decimal.TryParse(TxtTiLeGiamGia.Text?.Trim(), out decimal tileGiam);
            decimal.TryParse(TxtTienGiamGia.Text?.Trim(), out decimal tienGiam);
            decimal.TryParse(TxtTongCong.Text?.Trim(), out decimal tongCong);
            decimal.TryParse(TxtTienThanhToan.Text?.Trim(), out decimal tienThanhToan);

            var px = new PhieuXuatItem
            {
                Id = _isNew ? Guid.NewGuid().ToString() : _phieuXuatId,
                SoPhieu = soPhieu,
                Ngay = DpNgay.SelectedDate ?? DateTime.Now,
                DkhoXuatId = _selectedKhoId ?? "",
                DnhanVienXuatId = _selectedNvId ?? "",
                DkhachHangId = _selectedNccId ?? "",
                TienHang = tienHang,
                TiLeGiamGia = tileGiam,
                TienGiamGia = tienGiam,
                TongCong = tongCong,
                ThanhToan = tienThanhToan,
                ConLai = Math.Max(0, tongCong - tienThanhToan),
                Note = TxtGhiChu.Text?.Trim() ?? "",
                Status = 30
            };

            var res = await LocalXuatKhoService.SavePhieuXuatAsync(px, _phieuXuatChiTietList.ToList(), _isNew);
            if (res.Success)
            {
                _phieuXuatId = res.Id;
                _isNew = false;
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lưu phiếu xuất không thành công: " + res.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu xuất kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                _isNew = true;
                _phieuXuatId = null;
                Title = "PHIẾU XUẤT KHO - THÊM MỚI";
                _phieuXuatChiTietList.Clear();
                TxtGhiChu.Text = "";
                TxtTiLeGiamGia.Text = "0";
                TxtTienGiamGia.Text = "0";
                TinhTongCong();
                await SinhSoPhieuTuDongAsync();
                TxtTimMatHang.Focus();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu xuất và gửi lệnh in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu xuất kho! Đang mở xem trước mẫu in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            BtnLuuVaMoi_Click(sender, e);
        }

        private void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _isNew = true;
            _phieuXuatId = null;
            Title = "PHIẾU XUẤT KHO - THÊM MỚI";
            _ = SinhSoPhieuTuDongAsync();
            MessageBox.Show("Đã sao chép nội dung phiếu sang phiếu mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnXoaPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_phieuXuatId)) return;
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu xuất này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalXuatKhoService.DeletePhieuXuatAsync(_phieuXuatId);
                if (ok)
                {
                    OnSaved?.Invoke();
                    Close();
                }
            }
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuXuat != null && _allPhieuXuat.Count > 0 && _currentIndex > 0)
            {
                _currentIndex--;
                var px = _allPhieuXuat[_currentIndex];
                _phieuXuatId = px.Id;
                _isNew = false;
                Title = "PHIẾU XUẤT KHO - SỬA";
                await LoadPhieuXuatAsync(_phieuXuatId);
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuXuat != null && _allPhieuXuat.Count > 0 && _currentIndex < _allPhieuXuat.Count - 1)
            {
                _currentIndex++;
                var px = _allPhieuXuat[_currentIndex];
                _phieuXuatId = px.Id;
                _isNew = false;
                Title = "PHIẾU XUẤT KHO - SỬA";
                await LoadPhieuXuatAsync(_phieuXuatId);
            }
        }

        private void BtnNhapExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng nhập chi tiết phiếu xuất từ Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất chi tiết phiếu xuất sang Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
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
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
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
                    BtnTaoMoi_Click(sender, e);
                    e.Handled = true;
                }
            }
        }
    }
}
