using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views;
using QuanLyBar.Client.Views.DanhMucNhaCungCap;
using QuanLyBar.Client.Views.KhoHang;

namespace QuanLyBar.Client.Views.QuanLyNhapKho
{
    public partial class ThemPhieuNhapKhoWindow : Window
    {
        public event Action OnSaved;
        private string _phieuNhapId;
        private List<PhieuNhapItem> _allPhieuNhap;
        private int _currentIndex = -1;
        private ObservableCollection<PhieuNhapChiTietItem> _details = new();
        private List<MatHangNhapKhoItem> _allMatHang = new();
        private bool _isUpdatingTotals = false;
        private bool _isUserEditedThanhToan = false;
        private bool _isLoaded = false;

        private List<NhapKhoLookupItem> _allNccLookup = new();
        private List<NhapKhoLookupItem> _allKhoLookup = new();
        private List<NhapKhoLookupItem> _allNvLookup = new();

        private string _selectedNhaCungCapId = "";
        private string _selectedKhoNhapId = "";
        private string _selectedNhanVienNhapId = "";

        public ThemPhieuNhapKhoWindow(string phieuNhapId = null, List<PhieuNhapItem> allPhieuNhap = null)
        {
            InitializeComponent();
            _phieuNhapId = phieuNhapId;
            _allPhieuNhap = allPhieuNhap ?? new List<PhieuNhapItem>();

            _details.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (PhieuNhapChiTietItem item in e.NewItems)
                    {
                        item.PropertyChanged -= DetailItem_PropertyChanged;
                        item.PropertyChanged += DetailItem_PropertyChanged;
                    }
                }
                CalculateTotals();
            };

            DgChiTiet.ItemsSource = _details;
            Loaded += ThemPhieuNhapKhoWindow_Loaded;
        }

        private void DetailItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CalculateTotals();
        }

        private async void ThemPhieuNhapKhoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadLookupsAsync();
                await LoadMatHangListAsync();

                if (!string.IsNullOrEmpty(_phieuNhapId))
                {
                    _currentIndex = _allPhieuNhap.FindIndex(x => x.Id == _phieuNhapId);
                    await LoadPhieuNhapDetailAsync(_phieuNhapId);
                }
                else
                {
                    await CreateNewPhieuNhapAsync();
                }

                _isLoaded = true;
                UpdateNavigationButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLookupsAsync()
        {
            await LoadKhoListAsync();
            await LoadNvListAsync();
            await LoadNccListAsync();
        }

        private async Task LoadKhoListAsync(string selectId = null)
        {
            _allKhoLookup = await LocalNhapKhoService.GetKhoHangListFlatAsync();
            LstKho.ItemsSource = _allKhoLookup;
            if (!string.IsNullOrEmpty(selectId))
            {
                SetSelectedKho(selectId);
            }
            else if (string.IsNullOrEmpty(_selectedKhoNhapId) && _allKhoLookup.Count > 0)
            {
                SetSelectedKho(_allKhoLookup[0].Id);
            }
        }

        private void SetSelectedKho(string khoId)
        {
            _selectedKhoNhapId = khoId ?? "";
            var item = _allKhoLookup.FirstOrDefault(x => x.Id == _selectedKhoNhapId);
            TxtSelectedKho.Text = item?.Name ?? "";
            LstKho.SelectedItem = item;
        }

        private async Task LoadNvListAsync(string selectId = null)
        {
            _allNvLookup = await LocalNhapKhoService.GetNhanVienLookupListAsync();
            ApplyNvFilter();
            if (!string.IsNullOrEmpty(selectId))
            {
                SetSelectedNhanVien(selectId);
            }
            else if (string.IsNullOrEmpty(_selectedNhanVienNhapId) && _allNvLookup.Count > 0)
            {
                SetSelectedNhanVien(_allNvLookup[0].Id);
            }
        }

        private void SetSelectedNhanVien(string nvId)
        {
            _selectedNhanVienNhapId = nvId ?? "";
            var item = _allNvLookup.FirstOrDefault(x => x.Id == _selectedNhanVienNhapId);
            TxtSelectedNhanVien.Text = item?.Name ?? "";
            LstNhanVien.SelectedItem = item;
        }

        private void ApplyNvFilter()
        {
            string keyword = TxtTimNvPopup?.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(keyword))
            {
                LstNhanVien.ItemsSource = _allNvLookup;
            }
            else
            {
                LstNhanVien.ItemsSource = _allNvLookup.Where(x => x.Name != null && x.Name.ToLowerInvariant().Contains(keyword)).ToList();
            }
        }

        private async Task LoadNccListAsync(string selectId = null)
        {
            _allNccLookup = await LocalNhapKhoService.GetNhaCungCapLookupListAsync();
            ApplyNccFilter();
            if (!string.IsNullOrEmpty(selectId))
            {
                SetSelectedNhaCungCap(selectId);
            }
            else if (string.IsNullOrEmpty(_selectedNhaCungCapId) && _allNccLookup.Count > 0)
            {
                SetSelectedNhaCungCap(_allNccLookup[0].Id);
            }
        }

        private void SetSelectedNhaCungCap(string nccId)
        {
            _selectedNhaCungCapId = nccId ?? "";
            var item = _allNccLookup.FirstOrDefault(x => x.Id == _selectedNhaCungCapId);
            TxtSelectedNhaCungCap.Text = item?.Name ?? "";
            DgNccPopup.SelectedItem = item;
        }

        private void ApplyNccFilter()
        {
            string keyword = TxtTimNccPopup?.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(keyword))
            {
                DgNccPopup.ItemsSource = _allNccLookup;
            }
            else
            {
                DgNccPopup.ItemsSource = _allNccLookup.Where(x => 
                    (x.Name != null && x.Name.ToLowerInvariant().Contains(keyword)) ||
                    (x.DienThoai != null && x.DienThoai.ToLowerInvariant().Contains(keyword)) ||
                    (x.DiaChi != null && x.DiaChi.ToLowerInvariant().Contains(keyword))
                ).ToList();
            }
        }

        private async Task LoadMatHangListAsync()
        {
            _allMatHang = await LocalNhapKhoService.GetMatHangForNhapKhoAsync();
            ApplyMatHangFilter();
        }

        private void ApplyMatHangFilter()
        {
            if (TxtTimMatHang == null || _allMatHang == null || DgMatHangChon == null) return;
            string keyword = TxtTimMatHang.Text?.Trim().ToLowerInvariant() ?? "";
            var filtered = _allMatHang.AsEnumerable();
            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = filtered.Where(x =>
                    (x.Name != null && x.Name.ToLowerInvariant().Contains(keyword)) ||
                    (x.Code != null && x.Code.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenDonViTinh != null && x.TenDonViTinh.ToLowerInvariant().Contains(keyword))
                );
            }
            var list = filtered.ToList();
            DgMatHangChon.ItemsSource = list;
            if (list.Count > 0) DgMatHangChon.SelectedIndex = 0;
        }

        private void TxtTimMatHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyMatHangFilter();
        }

        private void TxtTimMatHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnQuickAdd_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (DgMatHangChon.SelectedIndex < DgMatHangChon.Items.Count - 1)
                {
                    DgMatHangChon.SelectedIndex++;
                    DgMatHangChon.ScrollIntoView(DgMatHangChon.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (DgMatHangChon.SelectedIndex > 0)
                {
                    DgMatHangChon.SelectedIndex--;
                    DgMatHangChon.ScrollIntoView(DgMatHangChon.SelectedItem);
                }
                e.Handled = true;
            }
        }

        private void TxtQuickInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnQuickAdd_Click(sender, e);
                e.Handled = true;
            }
        }

        private void DgMatHangChon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected != null)
            {
                TxtQuickDonGia.Text = selected.GiaNhap.ToString("N0");
                TxtQuickSoLuong.Text = "1";
            }
        }

        private void DgMatHangChon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnQuickAdd_Click(sender, e);
        }

        private void BtnQuickAdd_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected == null)
            {
                if (DgMatHangChon.Items.Count > 0)
                {
                    DgMatHangChon.SelectedIndex = 0;
                    selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
                }
            }

            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng từ danh sách bên trái!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal sl = 1;
            decimal.TryParse(TxtQuickSoLuong.Text.Replace(",", "").Replace(".", ""), out sl);
            if (sl <= 0) sl = 1;

            decimal donGia = selected.GiaNhap;
            decimal.TryParse(TxtQuickDonGia.Text.Replace(",", "").Replace(".", ""), out donGia);

            var existing = _details.FirstOrDefault(x => x.DmathangId == selected.Id);
            if (existing != null)
            {
                existing.SlNhap += sl;
                if (donGia > 0) existing.DonGia = donGia;
                existing.Recalculate();
                DgChiTiet.ScrollIntoView(existing);
            }
            else
            {
                var row = new PhieuNhapChiTietItem
                {
                    Stt = _details.Count + 1,
                    Id = Guid.NewGuid().ToString(),
                    DmathangId = selected.Id,
                    MaHang = selected.Code,
                    TenHang = selected.Name,
                    DdonvitinhId = selected.DdonvitinhId,
                    TenDonViTinh = selected.TenDonViTinh,
                    SlNhap = sl,
                    DonGia = donGia,
                    TiLeGiamGia = 0,
                    TienGiamGia = 0,
                    ThanhTien = sl * donGia,
                    GiaBan = selected.GiaBan,
                    Note = ""
                };
                row.PropertyChanged += DetailItem_PropertyChanged;
                _details.Add(row);
                DgChiTiet.ScrollIntoView(row);
            }

            ReindexDetails();
            CalculateTotals();
            TxtQuickSoLuong.Text = "1";
            TxtTimMatHang.Focus();
            TxtTimMatHang.SelectAll();
        }

        private async Task CreateNewPhieuNhapAsync()
        {
            _phieuNhapId = null;
            _isUserEditedThanhToan = false;
            DpNgay.SelectedDate = DateTime.Now;
            TxtSoPhieu.Text = await LocalNhapKhoService.GetNextSoPhieuNhapAsync();
            TxtDienGiai.Text = "Nhập mua hàng";
            TxtGhiChu.Text = "";
            _details.Clear();

            TxtTiLeGiamGia.Text = "0";
            TxtTienGiamGia.Text = "0";
            TxtTiLeThue.Text = "0";
            TxtTienThue.Text = "0";

            CalculateTotals();
            UpdateNavigationButtonsState();
        }

        private async Task LoadPhieuNhapDetailAsync(string id)
        {
            var item = _allPhieuNhap.FirstOrDefault(x => x.Id == id);
            if (item == null) return;

            _phieuNhapId = item.Id;
            DpNgay.SelectedDate = item.Ngay ?? DateTime.Now;
            TxtSoPhieu.Text = item.SoPhieu;
            SetSelectedNhaCungCap(item.DnhacungcapId);
            SetSelectedKho(item.DkhoNhapId);
            SetSelectedNhanVien(item.DnhanVienNhapId);
            TxtDienGiai.Text = "Nhập mua hàng";
            TxtGhiChu.Text = item.Note;

            _isUpdatingTotals = true;
            TxtTiLeGiamGia.Text = item.TiLeGiamGia.ToString("N1");
            TxtTienGiamGia.Text = item.TienGiamGia.ToString("N0");
            _isUpdatingTotals = false;

            var details = await LocalNhapKhoService.GetPhieuNhapChiTietAsync(id);
            _details.Clear();
            foreach (var d in details) _details.Add(d);

            _isUserEditedThanhToan = true;
            CalculateTotals();
            TxtTienThanhToan.Text = item.ThanhToan.ToString("N0");
            UpdateNavigationButtonsState();
        }

        private void RecalculateRow(PhieuNhapChiTietItem row)
        {
            decimal tienGoc = row.SlNhap * row.DonGia;
            if (row.TiLeGiamGia > 0)
            {
                row.TienGiamGia = tienGoc * (row.TiLeGiamGia / 100m);
            }
            row.ThanhTien = tienGoc - row.TienGiamGia;
        }

        private void ReindexDetails()
        {
            int stt = 1;
            foreach (var d in _details) d.Stt = stt++;
        }

        private void CalculateTotals()
        {
            if (_isUpdatingTotals) return;
            if (TxtTienHang == null || TxtTiLeGiamGia == null || TxtTienGiamGia == null || TxtTiLeThue == null || TxtTienThue == null || TxtTongCong == null || TxtTienThanhToan == null || _details == null) return;
            _isUpdatingTotals = true;

            try
            {
                decimal tongTienHang = _details.Sum(x => x.ThanhTien);
                TxtTienHang.Text = tongTienHang.ToString("N0");

                decimal.TryParse(TxtTiLeGiamGia.Text, out decimal tiLeGiam);
                decimal tienGiam = 0;
                if (tiLeGiam > 0)
                {
                    tienGiam = tongTienHang * (tiLeGiam / 100m);
                    TxtTienGiamGia.Text = tienGiam.ToString("N0");
                }
                else
                {
                    decimal.TryParse(TxtTienGiamGia.Text.Replace(",", "").Replace(".", ""), out tienGiam);
                }

                decimal.TryParse(TxtTiLeThue.Text, out decimal tiLeThue);
                decimal tienThue = 0;
                if (tiLeThue > 0)
                {
                    tienThue = (tongTienHang - tienGiam) * (tiLeThue / 100m);
                    TxtTienThue.Text = tienThue.ToString("N0");
                }
                else
                {
                    decimal.TryParse(TxtTienThue.Text.Replace(",", "").Replace(".", ""), out tienThue);
                }

                decimal tongCong = (tongTienHang - tienGiam) + tienThue;
                if (tongCong < 0) tongCong = 0;

                TxtTongCong.Text = tongCong.ToString("N0");
                if (!_isUserEditedThanhToan)
                {
                    TxtTienThanhToan.Text = tongCong.ToString("N0");
                }
            }
            finally
            {
                _isUpdatingTotals = false;
            }
        }

        private void TxtTienThanhToan_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingTotals || TxtTienThanhToan == null) return;

            _isUserEditedThanhToan = true;
            string text = TxtTienThanhToan.Text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(text, out decimal val))
            {
                _isUpdatingTotals = true;
                try
                {
                    int caret = TxtTienThanhToan.CaretIndex;
                    int oldLength = TxtTienThanhToan.Text.Length;
                    string formatted = val.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
                    if (TxtTienThanhToan.Text != formatted)
                    {
                        TxtTienThanhToan.Text = formatted;
                        int newLength = TxtTienThanhToan.Text.Length;
                        TxtTienThanhToan.CaretIndex = Math.Max(0, Math.Min(newLength, caret + (newLength - oldLength)));
                    }
                }
                finally
                {
                    _isUpdatingTotals = false;
                }
            }
        }

        private void DgChiTiet_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var row = e.Row.Item as PhieuNhapChiTietItem;
                if (row != null)
                {
                    RecalculateRow(row);
                    DgChiTiet.Items.Refresh();
                    CalculateTotals();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void TxtDiscount_TextChanged(object sender, TextChangedEventArgs e) => CalculateTotals();
        private void TxtTax_TextChanged(object sender, TextChangedEventArgs e) => CalculateTotals();

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgChiTiet.SelectedItem as PhieuNhapChiTietItem;
            if (selected != null)
            {
                _details.Remove(selected);
                ReindexDetails();
                CalculateTotals();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TxtLocChiTiet_TextChanged(object sender, TextChangedEventArgs e)
        {
            string kw = TxtLocChiTiet.Text?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrEmpty(kw))
            {
                DgChiTiet.ItemsSource = _details;
            }
            else
            {
                DgChiTiet.ItemsSource = _details.Where(x =>
                    (x.TenHang != null && x.TenHang.ToLowerInvariant().Contains(kw)) ||
                    (x.MaHang != null && x.MaHang.ToLowerInvariant().Contains(kw))
                ).ToList();
            }
        }

        private void BtnNhapExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new NhapExcelPhieuNhapWindow(_allMatHang);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                if (win.ImportedItems != null && win.ImportedItems.Count > 0)
                {
                    foreach (var item in win.ImportedItems)
                    {
                        var existing = _details.FirstOrDefault(x => (!string.IsNullOrEmpty(item.DmathangId) && x.DmathangId == item.DmathangId) ||
                                                                    (!string.IsNullOrEmpty(item.MaHang) && x.MaHang.Equals(item.MaHang, StringComparison.OrdinalIgnoreCase)) ||
                                                                    (!string.IsNullOrEmpty(item.TenHang) && x.TenHang.Equals(item.TenHang, StringComparison.OrdinalIgnoreCase)));
                        if (existing != null)
                        {
                            existing.SlNhap += item.SlNhap;
                            if (item.DonGia > 0) existing.DonGia = item.DonGia;
                            if (item.TiLeGiamGia > 0) existing.TiLeGiamGia = item.TiLeGiamGia;
                            existing.Recalculate();
                        }
                        else
                        {
                            item.Stt = _details.Count + 1;
                            item.PropertyChanged += DetailItem_PropertyChanged;
                            _details.Add(item);
                        }
                    }

                    ReindexDetails();
                    CalculateTotals();
                    DgChiTiet.Items.Refresh();
                    MessageBox.Show($"Đã nhập thành công {win.ImportedItems.Count} mặt hàng vào phiếu nhập!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnXuatExcelChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (_details == null || _details.Count == 0)
            {
                MessageBox.Show("Chưa có mặt hàng trong phiếu để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"ChiTietPhieuNhap_{TxtSoPhieu.Text.Replace("/", "_")}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("ChiTiet");
                        string[] headers = new[] { "STT", "Tên mặt hàng", "ĐVT", "Số lượng", "Đơn giá", "Giảm giá %", "Thành tiền", "Ghi chú", "Mã hàng" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                        }

                        int r = 2;
                        foreach (var item in _details)
                        {
                            ws.Cell(r, 1).Value = item.Stt;
                            ws.Cell(r, 2).Value = item.TenHang;
                            ws.Cell(r, 3).Value = item.TenDonViTinh;
                            ws.Cell(r, 4).Value = item.SlNhap;
                            ws.Cell(r, 5).Value = item.DonGia;
                            ws.Cell(r, 6).Value = item.TiLeGiamGia;
                            ws.Cell(r, 7).Value = item.ThanhTien;
                            ws.Cell(r, 8).Value = item.Note;
                            ws.Cell(r, 9).Value = item.MaHang;
                            r++;
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Đã xuất chi tiết phiếu nhập ra Excel thành công!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtSoPhieu.Text))
            {
                MessageBox.Show("Vui lòng nhập số phiếu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_details.Count == 0)
            {
                MessageBox.Show("Phiếu nhập chưa có mặt hàng nào. Vui lòng thêm mặt hàng trước khi lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            decimal.TryParse(TxtTienHang.Text.Replace(",", "").Replace(".", ""), out decimal tienHang);
            decimal.TryParse(TxtTienGiamGia.Text.Replace(",", "").Replace(".", ""), out decimal tienGiam);
            decimal.TryParse(TxtTiLeGiamGia.Text, out decimal tiLeGiam);
            decimal.TryParse(TxtTongCong.Text.Replace(",", "").Replace(".", ""), out decimal tongCong);
            decimal.TryParse(TxtTienThanhToan.Text.Replace(",", "").Replace(".", ""), out decimal thanhToan);
            decimal conLai = tongCong - thanhToan;

            bool isNew = string.IsNullOrEmpty(_phieuNhapId);
            var item = new PhieuNhapItem
            {
                Id = isNew ? Guid.NewGuid().ToString() : _phieuNhapId,
                SoPhieu = TxtSoPhieu.Text.Trim(),
                Ngay = DpNgay.SelectedDate ?? DateTime.Now,
                DnhacungcapId = _selectedNhaCungCapId,
                DkhoNhapId = _selectedKhoNhapId,
                DnhanVienNhapId = _selectedNhanVienNhapId,
                TienHang = tienHang,
                TienGiamGia = tienGiam,
                TiLeGiamGia = tiLeGiam,
                TongCong = tongCong,
                ThanhToan = thanhToan,
                ConLai = conLai,
                Note = TxtGhiChu.Text.Trim(),
                Status = 30
            };

            var res = await LocalNhapKhoService.SavePhieuNhapAsync(item, _details.ToList(), isNew);
            if (!res.Success)
            {
                MessageBox.Show(res.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            _phieuNhapId = res.Id;
            OnSaved?.Invoke();
            return true;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                MessageBox.Show("Đã lưu phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                await CreateNewPhieuNhapAsync();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                var win = new InLuoiWindow(DgChiTiet, $"Phiếu nhập kho {TxtSoPhieu.Text}");
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            BtnLuuVaIn_Click(sender, e);
        }

        private async void BtnLuuVaInMaVach_Click(object sender, RoutedEventArgs e)
        {
            if (_details.Count == 0)
            {
                MessageBox.Show("Phiếu nhập chưa có mặt hàng nào để in mã vạch!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!await SaveAsync()) return;

            // Đảm bảo lấy giá bán chính xác từ danh mục mặt hàng
            foreach (var item in _details)
            {
                var match = _allMatHang.FirstOrDefault(m => (!string.IsNullOrEmpty(item.DmathangId) && m.Id == item.DmathangId) ||
                                                            (!string.IsNullOrEmpty(item.MaHang) && m.Code.Equals(item.MaHang, StringComparison.OrdinalIgnoreCase)) ||
                                                            (!string.IsNullOrEmpty(item.TenHang) && m.Name.Equals(item.TenHang, StringComparison.OrdinalIgnoreCase)));
                if (match != null)
                {
                    item.GiaBan = match.GiaBan;
                }
            }

            var mauWin = new MauInMaVachWindow();
            mauWin.Owner = this;
            if (mauWin.ShowDialog() == true && mauWin.SelectedMau != null)
            {
                var previewWin = new XemInMaVachWindow(mauWin.SelectedMau, _details.ToList());
                previewWin.Owner = this;
                previewWin.ShowDialog();
            }
        }

        private void BtnThietKeMaVach_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThietKeMauInMaVachWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            BtnXuatExcelChiTiet_Click(sender, e);
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                await LoadPhieuNhapDetailAsync(_allPhieuNhap[_currentIndex].Id);
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _allPhieuNhap.Count - 1)
            {
                _currentIndex++;
                await LoadPhieuNhapDetailAsync(_allPhieuNhap[_currentIndex].Id);
            }
        }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            await CreateNewPhieuNhapAsync();
        }

        private async void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _phieuNhapId = null;
            TxtSoPhieu.Text = await LocalNhapKhoService.GetNextSoPhieuNhapAsync();
            MessageBox.Show("Đã sao chép nội dung phiếu nhập sang số phiếu mới. Hãy bấm Lưu để hoàn tất!", "Sao chép", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnXoaPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_phieuNhapId))
            {
                MessageBox.Show("Phiếu chưa được lưu hoặc đang tạo mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu nhập '{TxtSoPhieu.Text}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalNhapKhoService.DeletePhieuNhapAsync(_phieuNhapId, false);
                if (ok)
                {
                    OnSaved?.Invoke();
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void UpdateNavigationButtonsState()
        {
            BtnTruoc.IsEnabled = _currentIndex > 0;
            BtnSau.IsEnabled = _currentIndex >= 0 && _currentIndex < _allPhieuNhap.Count - 1;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region ContextMenu Handlers for DgMatHangChon (Lưới danh mục mặt hàng trái)
        private void MenuMatHang_ThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemMoiMatHangWindow(null, null, null, -1, async () =>
            {
                await LoadMatHangListAsync();
            });
            win.Owner = this;
            win.ShowDialog();
        }

        private void MenuMatHang_ChinhSua_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemMoiMatHangWindow(null, selected.Id, null, -1, async () =>
            {
                await LoadMatHangListAsync();
            });
            win.Owner = this;
            win.ShowDialog();
        }

        private async void MenuMatHang_ThungRac_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected == null) return;
            if (MessageBox.Show($"Bạn có chắc chắn muốn chuyển mặt hàng '{selected.Name}' vào thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var service = new LocalMatHangService();
                await service.DeleteMatHangAsync(selected.Id, false);
                await LoadMatHangListAsync();
            }
        }

        private async void MenuMatHang_XoaHeThong_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected == null) return;
            if (MessageBox.Show($"Bạn có chắc chắn muốn XÓA VĨNH VIỄN mặt hàng '{selected.Name}' khỏi hệ thống không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var service = new LocalMatHangService();
                await service.DeleteMatHangAsync(selected.Id, true);
                await LoadMatHangListAsync();
            }
        }

        private async void MenuMatHang_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadMatHangListAsync();
        }

        private void MenuMatHang_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected != null)
            {
                Clipboard.SetText(selected.Name);
            }
        }

        private void MenuMatHang_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHangChon.SelectedItem as MatHangNhapKhoItem;
            if (selected != null)
            {
                Clipboard.SetText($"{selected.SttHienThi}\t{selected.Name}\t{selected.TenDonViTinh}\t{selected.Code}\t{selected.GiaNhap:N0}");
            }
        }

        private void MenuMatHang_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgMatHangChon.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuMatHang_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgMatHangChon, "Danh mục mặt hàng");
            win.Owner = this;
            win.ShowDialog();
        }

        private void MenuMatHang_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allMatHang == null || _allMatHang.Count == 0) return;
            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"DanhMucMatHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("MatHang");
                        string[] headers = new[] { "STT", "Tên hàng", "ĐVT", "Mã hàng", "Giá nhập" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                        }

                        int r = 2;
                        foreach (var item in _allMatHang)
                        {
                            ws.Cell(r, 1).Value = item.SttHienThi;
                            ws.Cell(r, 2).Value = item.Name;
                            ws.Cell(r, 3).Value = item.TenDonViTinh;
                            ws.Cell(r, 4).Value = item.Code;
                            ws.Cell(r, 5).Value = item.GiaNhap;
                            r++;
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Đã xuất danh mục mặt hàng ra Excel thành công!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuMatHang_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgMatHangChon, "Danh mục mặt hàng");
            win.Owner = this;
            win.ShowDialog();
        }
        #endregion

        #region ContextMenu Handlers for DgChiTiet (Lưới chi tiết phiếu nhập phải)
        private void MenuChiTiet_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgChiTiet.SelectedItem as PhieuNhapChiTietItem;
            if (selected != null)
            {
                Clipboard.SetText(selected.TenHang);
            }
        }

        private void MenuChiTiet_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgChiTiet.SelectedItem as PhieuNhapChiTietItem;
            if (selected != null)
            {
                Clipboard.SetText($"{selected.Stt}\t{selected.TenHang}\t{selected.TenDonViTinh}\t{selected.SlNhap}\t{selected.DonGia:N0}\t{selected.ThanhTien:N0}");
            }
        }

        private void MenuChiTiet_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgChiTiet.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuChiTiet_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgChiTiet, $"Chi tiết phiếu nhập {TxtSoPhieu.Text}");
            win.Owner = this;
            win.ShowDialog();
        }

        private void MenuChiTiet_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgChiTiet, $"Chi tiết phiếu nhập {TxtSoPhieu.Text}");
            win.Owner = this;
            win.ShowDialog();
        }
        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                BtnLuuVaMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
            {
                BtnLuu_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                BtnTaoMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                BtnLuuVaIn_Click(sender, e);
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
                BtnThoat_Click(sender, e);
                e.Handled = true;
            }
        }

        #region Nhà cung cấp Popup Handlers
        private void TxtSelectedNhaCungCap_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleNhaCungCap.IsChecked = !BtnToggleNhaCungCap.IsChecked;
        }

        private void TxtTimNccPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyNccFilter();
        }

        private void DgNccPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DgNccPopup.SelectedItem is NhapKhoLookupItem item)
            {
                SetSelectedNhaCungCap(item.Id);
                PopupNhaCungCap.IsOpen = false;
            }
        }

        private void BtnThemNcc_Click(object sender, RoutedEventArgs e)
        {
            PopupNhaCungCap.IsOpen = false;
            var win = new ThemNhaCungCapWindow();
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadNccListAsync();
            };
            win.ShowDialog();
        }

        private void BtnSuaNcc_Click(object sender, RoutedEventArgs e)
        {
            PopupNhaCungCap.IsOpen = false;
            if (!string.IsNullOrEmpty(_selectedNhaCungCapId))
            {
                var ncc = _allNccLookup.FirstOrDefault(x => x.Id == _selectedNhaCungCapId);
                var item = new NhaCungCapItem
                {
                    Id = _selectedNhaCungCapId,
                    Name = ncc?.Name,
                    DienThoai = ncc?.DienThoai,
                    DiaChi = ncc?.DiaChi
                };
                var win = new ThemNhaCungCapWindow(item);
                win.Owner = this;
                win.OnSaved += async () =>
                {
                    await LoadNccListAsync(_selectedNhaCungCapId);
                };
                win.ShowDialog();
            }
        }

        private async void BtnTaiNcc_Click(object sender, RoutedEventArgs e)
        {
            await LoadNccListAsync(_selectedNhaCungCapId);
        }

        private async void BtnDanhMucNcc_Click(object sender, RoutedEventArgs e)
        {
            await LoadNccListAsync(_selectedNhaCungCapId);
        }
        #endregion

        #region Kho nhập Popup Handlers
        private void TxtSelectedKho_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleKho.IsChecked = !BtnToggleKho.IsChecked;
        }

        private void LstKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstKho.SelectedItem is NhapKhoLookupItem item)
            {
                SetSelectedKho(item.Id);
                PopupKho.IsOpen = false;
            }
        }

        private void BtnThemKho_Click(object sender, RoutedEventArgs e)
        {
            PopupKho.IsOpen = false;
            var win = new ThemKhoHangWindow();
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadKhoListAsync();
            };
            win.ShowDialog();
        }

        private async void BtnTaiKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadKhoListAsync(_selectedKhoNhapId);
        }

        private async void BtnDanhMucKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadKhoListAsync(_selectedKhoNhapId);
        }
        #endregion

        #region Nhân viên nhập Popup Handlers
        private void TxtSelectedNhanVien_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleNhanVien.IsChecked = !BtnToggleNhanVien.IsChecked;
        }

        private void TxtTimNvPopup_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyNvFilter();
        }

        private void LstNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstNhanVien.SelectedItem is NhapKhoLookupItem item)
            {
                SetSelectedNhanVien(item.Id);
                PopupNhanVien.IsOpen = false;
            }
        }

        private void BtnThemNv_Click(object sender, RoutedEventArgs e)
        {
            PopupNhanVien.IsOpen = false;
            var win = new ThemNhanVienWindow();
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadNvListAsync();
            };
            win.ShowDialog();
        }

        private async void BtnTaiNv_Click(object sender, RoutedEventArgs e)
        {
            await LoadNvListAsync(_selectedNhanVienNhapId);
        }

        private async void BtnDanhMucNv_Click(object sender, RoutedEventArgs e)
        {
            await LoadNvListAsync(_selectedNhanVienNhapId);
        }
        #endregion
    }
}
