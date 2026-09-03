using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views;
using QuanLyBar.Client.Views.KhoHang;

namespace QuanLyBar.Client.Views.QuanLyChuyenKho
{
    public partial class ThemPhieuChuyenKhoWindow : Window
    {
        private string _phieuChuyenId;
        private List<PhieuChuyenKhoItem> _allPhieuChuyen = new List<PhieuChuyenKhoItem>();
        private int _currentIndex = -1;
        private bool _isNew = true;
        private bool _isLoaded = false;
        public event Action OnSaved;

        private List<MatHangNhapKhoItem> _allCatalog = new List<MatHangNhapKhoItem>();
        private ObservableCollection<MatHangNhapKhoItem> _filteredCatalog = new ObservableCollection<MatHangNhapKhoItem>();
        private ObservableCollection<PhieuChuyenKhoChiTietItem> _phieuChuyenChiTietList = new ObservableCollection<PhieuChuyenKhoChiTietItem>();

        private List<NhapKhoLookupItem> _allKho = new List<NhapKhoLookupItem>();
        private List<NhapKhoLookupItem> _allNv = new List<NhapKhoLookupItem>();

        private string _selectedKhoXuatId;
        private string _selectedKhoNhapId;
        private string _selectedNvXuatId;
        private string _selectedNvNhapId;

        public ThemPhieuChuyenKhoWindow(string phieuChuyenId = null, List<PhieuChuyenKhoItem> allPhieuChuyen = null)
        {
            InitializeComponent();
            _phieuChuyenId = phieuChuyenId;
            _allPhieuChuyen = allPhieuChuyen ?? new List<PhieuChuyenKhoItem>();
            _isNew = string.IsNullOrEmpty(phieuChuyenId);

            if (!_isNew)
            {
                Title = "PHIẾU CHUYỂN KHO - SỬA";
                _currentIndex = _allPhieuChuyen.FindIndex(x => x.Id == _phieuChuyenId);
            }

            DgMatHangCatalog.ItemsSource = _filteredCatalog;
            DgPhieuChuyenChiTiet.ItemsSource = _phieuChuyenChiTietList;

            Loaded += ThemPhieuChuyenKhoWindow_Loaded;
        }

        private async void ThemPhieuChuyenKhoWindow_Loaded(object sender, RoutedEventArgs e)
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
                    await LoadPhieuChuyenAsync(_phieuChuyenId);
                }

                _isLoaded = true;
                TinhTongCong();
                TxtTimMatHang.Focus();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ThemPhieuChuyenKhoWindow_Loaded error: " + ex.Message);
            }
        }

        private async Task SinhSoPhieuTuDongAsync()
        {
            TxtSoPhieu.Text = await LocalChuyenKhoService.GetNextSoPhieuChuyenKhoAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                _allKho = await LocalNhapKhoService.GetKhoHangListFlatAsync();
                LstKhoXuatPopup.ItemsSource = _allKho;
                LstKhoNhapPopup.ItemsSource = _allKho;

                if (_allKho != null && _allKho.Count > 0 && string.IsNullOrEmpty(_selectedKhoXuatId))
                {
                    SelectKhoXuat(_allKho[0]);
                }
                if (_allKho != null && _allKho.Count > 1 && string.IsNullOrEmpty(_selectedKhoNhapId))
                {
                    SelectKhoNhap(_allKho[1]);
                }

                _allNv = await LocalNhapKhoService.GetNhanVienLookupListAsync();
                LstNvXuatPopup.ItemsSource = _allNv;
                LstNvNhapPopup.ItemsSource = _allNv;

                if (_allNv != null && _allNv.Count > 0 && string.IsNullOrEmpty(_selectedNvXuatId))
                {
                    SelectNvXuat(_allNv[0]);
                }
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
            _filteredCatalog.Clear();
            string kw = TxtTimMatHang.Text?.Trim().ToLowerInvariant() ?? "";

            var filtered = string.IsNullOrEmpty(kw)
                ? _allCatalog
                : _allCatalog.Where(x => (x.Name?.ToLowerInvariant().Contains(kw) == true) ||
                                         (x.Code?.ToLowerInvariant().Contains(kw) == true)).ToList();

            int stt = 1;
            foreach (var item in filtered)
            {
                item.Stt = stt++;
                _filteredCatalog.Add(item);
            }
        }

        private async Task LoadPhieuChuyenAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            try
            {
                var list = await LocalChuyenKhoService.GetPhieuChuyenKhoListAsync(null, null, null, null, null, null, null, null);
                var px = list.FirstOrDefault(x => x.Id == id);
                if (px != null)
                {
                    TxtSoPhieu.Text = px.SoPhieu;
                    DpNgay.SelectedDate = px.Ngay ?? DateTime.Now;
                    TxtDienGiai.Text = px.DienGiai;
                    TxtGhiChu.Text = px.Note;

                    var kXuat = _allKho.FirstOrDefault(x => x.Id == px.DkhoXuatId);
                    if (kXuat != null) SelectKhoXuat(kXuat);

                    var kNhap = _allKho.FirstOrDefault(x => x.Id == px.DkhoNhapId);
                    if (kNhap != null) SelectKhoNhap(kNhap);

                    var nvXuat = _allNv.FirstOrDefault(x => x.Id == px.DnhanVienXuatId);
                    if (nvXuat != null) SelectNvXuat(nvXuat);

                    var nvNhap = _allNv.FirstOrDefault(x => x.Id == px.DnhanVienNhapId);
                    if (nvNhap != null) SelectNvNhap(nvNhap);
                }

                var details = await LocalChuyenKhoService.GetPhieuChuyenKhoChiTietAsync(id);
                _phieuChuyenChiTietList.Clear();
                foreach (var d in details)
                {
                    _phieuChuyenChiTietList.Add(d);
                }

                TinhTongCong();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin phiếu chuyển: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Thao tác thêm mặt hàng
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
                    DgMatHangCatalog.SelectedIndex = 0;
                    ThemMatHangVaoPhieuChuyen(_filteredCatalog[0]);
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
                ThemMatHangVaoPhieuChuyen(item);
            }
            else if (_filteredCatalog.Count > 0)
            {
                ThemMatHangVaoPhieuChuyen(_filteredCatalog[0]);
            }
        }

        private void DgMatHangCatalog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgMatHangCatalog.SelectedItem is MatHangNhapKhoItem item)
            {
                ThemMatHangVaoPhieuChuyen(item);
            }
        }

        private void ThemMatHangVaoPhieuChuyen(MatHangNhapKhoItem catalogItem)
        {
            if (catalogItem == null) return;

            decimal sl = 1;
            decimal.TryParse(TxtSoLuongThem.Text?.Trim(), out sl);
            if (sl <= 0) sl = 1;

            var existing = _phieuChuyenChiTietList.FirstOrDefault(x => x.DmathangId == catalogItem.Id);
            if (existing != null)
            {
                existing.SlChuyen += sl;
            }
            else
            {
                var newItem = new PhieuChuyenKhoChiTietItem
                {
                    Stt = _phieuChuyenChiTietList.Count + 1,
                    DmathangId = catalogItem.Id,
                    MaHang = catalogItem.Code,
                    TenHang = catalogItem.Name,
                    DdonvitinhId = catalogItem.DdonvitinhId,
                    TenDonViTinh = catalogItem.TenDonViTinh,
                    SlChuyen = sl,
                    DonGia = catalogItem.GiaBan,
                    ThanhTien = sl * catalogItem.GiaBan,
                    GhiChu = ""
                };
                _phieuChuyenChiTietList.Add(newItem);
            }

            TinhTongCong();
            TxtTimMatHang.SelectAll();
            TxtTimMatHang.Focus();
        }

        private void DgPhieuChuyenChiTiet_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TinhTongCong();
            }));
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgPhieuChuyenChiTiet.SelectedItem is PhieuChuyenKhoChiTietItem sel)
            {
                _phieuChuyenChiTietList.Remove(sel);
                for (int i = 0; i < _phieuChuyenChiTietList.Count; i++)
                {
                    _phieuChuyenChiTietList[i].Stt = i + 1;
                }
                TinhTongCong();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool _isUpdatingSummary = false;

        private void SummaryInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || _isUpdatingSummary) return;

            try
            {
                decimal tienHang = _phieuChuyenChiTietList.Sum(x => x.ThanhTien);

                if (sender == TxtTiLeGiamGia)
                {
                    if (decimal.TryParse(TxtTiLeGiamGia.Text?.Trim(), out decimal tlGiam))
                    {
                        decimal tienGiam = tienHang * (tlGiam / 100m);
                        _isUpdatingSummary = true;
                        TxtTienGiamGia.Text = tienGiam.ToString("N0");
                        _isUpdatingSummary = false;
                    }
                }
                else if (sender == TxtTiLeThue)
                {
                    if (decimal.TryParse(TxtTiLeThue.Text?.Trim(), out decimal tlThue))
                    {
                        decimal.TryParse(TxtTienGiamGia.Text?.Replace(",", "").Replace(".", ""), out decimal tienGiam);
                        decimal sauGiam = Math.Max(0, tienHang - tienGiam);
                        decimal tienThue = sauGiam * (tlThue / 100m);
                        _isUpdatingSummary = true;
                        TxtTienThue.Text = tienThue.ToString("N0");
                        _isUpdatingSummary = false;
                    }
                }
            }
            catch { }

            TinhTongCong();
        }

        private void TinhTongCong()
        {
            if (!_isLoaded || _isUpdatingSummary) return;
            if (TxtTienHang == null || TxtTongCong == null || _phieuChuyenChiTietList == null) return;
            _isUpdatingSummary = true;

            try
            {
                decimal tongTienHang = _phieuChuyenChiTietList.Sum(x => x.ThanhTien);
                TxtTienHang.Text = tongTienHang.ToString("N0");

                decimal tienGiam = 0;
                if (TxtTienGiamGia != null)
                {
                    decimal.TryParse(TxtTienGiamGia.Text?.Replace(",", "").Replace(".", ""), out tienGiam);
                }

                decimal tienThue = 0;
                if (TxtTienThue != null)
                {
                    decimal.TryParse(TxtTienThue.Text?.Replace(",", "").Replace(".", ""), out tienThue);
                }

                decimal phiVanChuyen = 0;
                if (TxtPhiVanChuyen != null)
                {
                    decimal.TryParse(TxtPhiVanChuyen.Text?.Replace(",", "").Replace(".", ""), out phiVanChuyen);
                }

                decimal tongCong = Math.Max(0, tongTienHang - tienGiam) + tienThue + phiVanChuyen;
                TxtTongCong.Text = tongCong.ToString("N0");
            }
            finally
            {
                _isUpdatingSummary = false;
            }
        }

        private void TxtLocChiTiet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            string kw = TxtLocChiTiet.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(kw))
            {
                DgPhieuChuyenChiTiet.ItemsSource = _phieuChuyenChiTietList;
            }
            else
            {
                var filtered = _phieuChuyenChiTietList.Where(x => (x.TenHang?.ToLowerInvariant().Contains(kw) == true) ||
                                                                  (x.MaHang?.ToLowerInvariant().Contains(kw) == true)).ToList();
                DgPhieuChuyenChiTiet.ItemsSource = filtered;
            }
        }
        #endregion

        #region Dropdowns & Popups
        private void SelectKhoXuat(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedKhoXuatId = item.Id;
            if (TxtSelectedKhoXuat != null) TxtSelectedKhoXuat.Text = item.Name ?? "";
            if (PopupKhoXuat != null) PopupKhoXuat.IsOpen = false;
            if (BtnToggleKhoXuat != null) BtnToggleKhoXuat.IsChecked = false;
        }

        private void TxtSelectedKhoXuat_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupKhoXuat != null) PopupKhoXuat.IsOpen = !PopupKhoXuat.IsOpen;
        }

        private void LstKhoXuatPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstKhoXuatPopup?.SelectedItem is NhapKhoLookupItem item)
            {
                SelectKhoXuat(item);
            }
        }

        private void TxtTimKhoXuatPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || LstKhoXuatPopup == null || _allKho == null) return;
            string kw = TxtTimKhoXuatPopup.Text?.Trim().ToLowerInvariant() ?? "";
            LstKhoXuatPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allKho
                : _allKho.Where(x => x.Name?.ToLowerInvariant().Contains(kw) == true).ToList();
        }

        private void SelectKhoNhap(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedKhoNhapId = item.Id;
            if (TxtSelectedKhoNhap != null) TxtSelectedKhoNhap.Text = item.Name ?? "";
            if (PopupKhoNhap != null) PopupKhoNhap.IsOpen = false;
            if (BtnToggleKhoNhap != null) BtnToggleKhoNhap.IsChecked = false;
        }

        private void TxtSelectedKhoNhap_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupKhoNhap != null) PopupKhoNhap.IsOpen = !PopupKhoNhap.IsOpen;
        }

        private void LstKhoNhapPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstKhoNhapPopup?.SelectedItem is NhapKhoLookupItem item)
            {
                SelectKhoNhap(item);
            }
        }

        private void TxtTimKhoNhapPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || LstKhoNhapPopup == null || _allKho == null) return;
            string kw = TxtTimKhoNhapPopup.Text?.Trim().ToLowerInvariant() ?? "";
            LstKhoNhapPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allKho
                : _allKho.Where(x => x.Name?.ToLowerInvariant().Contains(kw) == true).ToList();
        }

        private void SelectNvXuat(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedNvXuatId = item.Id;
            if (TxtSelectedNvXuat != null) TxtSelectedNvXuat.Text = item.Name ?? "";
            if (PopupNvXuat != null) PopupNvXuat.IsOpen = false;
            if (BtnToggleNvXuat != null) BtnToggleNvXuat.IsChecked = false;
        }

        private void TxtSelectedNvXuat_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupNvXuat != null) PopupNvXuat.IsOpen = !PopupNvXuat.IsOpen;
        }

        private void LstNvXuatPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstNvXuatPopup?.SelectedItem is NhapKhoLookupItem item)
            {
                SelectNvXuat(item);
            }
        }

        private void TxtTimNvXuatPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || LstNvXuatPopup == null || _allNv == null) return;
            string kw = TxtTimNvXuatPopup.Text?.Trim().ToLowerInvariant() ?? "";
            LstNvXuatPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allNv
                : _allNv.Where(x => x.Name?.ToLowerInvariant().Contains(kw) == true).ToList();
        }

        private void SelectNvNhap(NhapKhoLookupItem item)
        {
            if (item == null) return;
            _selectedNvNhapId = item.Id;
            if (TxtSelectedNvNhap != null) TxtSelectedNvNhap.Text = item.Name ?? "";
            if (PopupNvNhap != null) PopupNvNhap.IsOpen = false;
            if (BtnToggleNvNhap != null) BtnToggleNvNhap.IsChecked = false;
        }

        private void TxtSelectedNvNhap_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupNvNhap != null) PopupNvNhap.IsOpen = !PopupNvNhap.IsOpen;
        }

        private void LstNvNhapPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstNvNhapPopup?.SelectedItem is NhapKhoLookupItem item)
            {
                SelectNvNhap(item);
            }
        }

        private void TxtTimNvNhapPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded || LstNvNhapPopup == null || _allNv == null) return;
            string kw = TxtTimNvNhapPopup.Text?.Trim().ToLowerInvariant() ?? "";
            LstNvNhapPopup.ItemsSource = string.IsNullOrEmpty(kw)
                ? _allNv
                : _allNv.Where(x => x.Name?.ToLowerInvariant().Contains(kw) == true).ToList();
        }
        #endregion

        #region Lưu và Thoát
        private async Task<bool> SaveCurrentAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtSoPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập số phiếu chuyển kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_phieuChuyenChiTietList.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một mặt hàng vào phiếu chuyển kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(_selectedKhoXuatId))
            {
                MessageBox.Show("Vui lòng chọn Kho xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(_selectedKhoNhapId))
            {
                MessageBox.Show("Vui lòng chọn Kho nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(_selectedKhoXuatId) && !string.IsNullOrEmpty(_selectedKhoNhapId) && 
                (_selectedKhoXuatId.Equals(_selectedKhoNhapId, StringComparison.OrdinalIgnoreCase) || 
                 (TxtSelectedKhoXuat != null && TxtSelectedKhoNhap != null && !string.IsNullOrWhiteSpace(TxtSelectedKhoXuat.Text) && TxtSelectedKhoXuat.Text.Trim().Equals(TxtSelectedKhoNhap.Text.Trim(), StringComparison.OrdinalIgnoreCase))))
            {
                MessageBox.Show("Không được nhập trùng kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            decimal tongTien = _phieuChuyenChiTietList.Sum(x => x.ThanhTien);

            var px = new PhieuChuyenKhoItem
            {
                Id = _phieuChuyenId,
                SoPhieu = TxtSoPhieu.Text.Trim(),
                Ngay = DpNgay.SelectedDate ?? DateTime.Now,
                DienGiai = TxtDienGiai.Text.Trim(),
                DkhoXuatId = _selectedKhoXuatId,
                DkhoNhapId = _selectedKhoNhapId,
                DnhanVienXuatId = _selectedNvXuatId,
                DnhanVienNhapId = _selectedNvNhapId,
                Note = TxtGhiChu.Text.Trim(),
                TienHang = tongTien,
                TongCong = tongTien
            };

            var res = await LocalChuyenKhoService.SavePhieuChuyenKhoAsync(px, _phieuChuyenChiTietList.ToList(), _isNew);
            if (res.Success)
            {
                _phieuChuyenId = res.Id;
                _isNew = false;
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lưu phiếu chuyển kho không thành công: " + res.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                MessageBox.Show("Đã lưu phiếu chuyển kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                _isNew = true;
                _phieuChuyenId = null;
                Title = "PHIẾU CHUYỂN KHO - THÊM MỚI";
                _phieuChuyenChiTietList.Clear();
                TxtGhiChu.Text = "";
                await SinhSoPhieuTuDongAsync();
                TinhTongCong();
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

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                MessageBox.Show("Chức năng in phiếu chuyển kho đang được tích hợp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentAsync())
            {
                MessageBox.Show("Chức năng xem trước bản in phiếu chuyển kho đang được tích hợp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
            _phieuChuyenId = null;
            Title = "PHIẾU CHUYỂN KHO - THÊM MỚI";
            _ = SinhSoPhieuTuDongAsync();
            MessageBox.Show("Đã sao chép nội dung phiếu sang phiếu mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnXoaPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_phieuChuyenId)) return;
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu chuyển kho này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalChuyenKhoService.DeletePhieuChuyenKhoAsync(_phieuChuyenId);
                if (ok)
                {
                    OnSaved?.Invoke();
                    Close();
                }
            }
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuChuyen != null && _allPhieuChuyen.Count > 0 && _currentIndex > 0)
            {
                _currentIndex--;
                var px = _allPhieuChuyen[_currentIndex];
                _phieuChuyenId = px.Id;
                _isNew = false;
                Title = "PHIẾU CHUYỂN KHO - SỬA";
                await LoadPhieuChuyenAsync(_phieuChuyenId);
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuChuyen != null && _allPhieuChuyen.Count > 0 && _currentIndex < _allPhieuChuyen.Count - 1)
            {
                _currentIndex++;
                var px = _allPhieuChuyen[_currentIndex];
                _phieuChuyenId = px.Id;
                _isNew = false;
                Title = "PHIẾU CHUYỂN KHO - SỬA";
                await LoadPhieuChuyenAsync(_phieuChuyenId);
            }
        }

        private void BtnNhapExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng nhập chi tiết phiếu chuyển từ Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất chi tiết phiếu chuyển sang Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                BtnXoaDong_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F10)
            {
                BtnTruoc_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnSau_Click(null, null);
                e.Handled = true;
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.S)
                {
                    BtnLuuVaMoi_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.L)
                {
                    BtnLuu_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.N)
                {
                    BtnTaoMoi_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.P)
                {
                    BtnLuuVaIn_Click(null, null);
                    e.Handled = true;
                }
            }
        }
    }
}
