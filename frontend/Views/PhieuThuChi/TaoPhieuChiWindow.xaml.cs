using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.PhieuThuChi
{
    public partial class TaoPhieuChiWindow : Window
    {
        private string _id = null;
        private List<TTHUCHI> _allPhieuList = new List<TTHUCHI>();
        private int _currentIndex = -1;

        private List<dynamic> _khachHangList = new List<dynamic>();
        private List<dynamic> _nhaCungCapList = new List<dynamic>();
        private List<dynamic> _nhanVienList = new List<dynamic>();
        private List<dynamic> _lyDoList = new List<dynamic>();
        private List<dynamic> _cuaHangList = new List<dynamic>();
        private List<dynamic> _taiKhoanList = new List<dynamic>();

        public string SavedId { get; private set; }

        public TaoPhieuChiWindow(string id = null)
        {
            InitializeComponent();
            _id = id;

            Loaded += async (s, e) =>
            {
                await LoadLookupsAsync();
                await LoadAllPhieuListAsync();

                if (!string.IsNullOrEmpty(_id))
                {
                    await LoadPhieuDetailAsync(_id);
                }
                else
                {
                    await ResetFormAsync();
                }
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
                else if (e.Key == Key.F10)
                {
                    BtnPrevious_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F11)
                {
                    BtnNext_Click(null, null);
                    e.Handled = true;
                }
            };
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                _lyDoList = await LocalPhieuThuChiService.GetLyDoThuLookupAsync();
                CboPhanLoai.ItemsSource = _lyDoList;
                CboLyDoChi.ItemsSource = _lyDoList;
                if (_lyDoList.Count > 0)
                {
                    CboPhanLoai.SelectedIndex = 0;
                    CboLyDoChi.SelectedIndex = 0;
                }

                _nhanVienList = await LocalPhieuThuChiService.GetNhanVienLookupAsync();
                CboNhanVien.ItemsSource = _nhanVienList;

                _nhaCungCapList = await LocalPhieuThuChiService.GetNhaCungCapLookupAsync();
                CboNhaCungCap.ItemsSource = _nhaCungCapList;

                _khachHangList = await LocalPhieuThuChiService.GetKhachHangLookupAsync();
                CboKhachHang.ItemsSource = _khachHangList;

                _cuaHangList = await LocalPhieuThuChiService.GetCuaHangLookupAsync();
                CboCuaHang.ItemsSource = _cuaHangList;
                if (_cuaHangList.Count > 0)
                {
                    CboCuaHang.SelectedIndex = 0;
                }

                _taiKhoanList = await LocalPhieuThuChiService.GetTaiKhoanNganHangLookupAsync();
                CboTaiKhoanNganHang.ItemsSource = _taiKhoanList;
                if (_taiKhoanList.Count > 0)
                {
                    CboTaiKhoanNganHang.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private async Task LoadAllPhieuListAsync()
        {
            try
            {
                _allPhieuList = await LocalPhieuThuChiService.GetAllPhieuListAsync(isThu: false);
                if (!string.IsNullOrEmpty(_id))
                {
                    _currentIndex = _allPhieuList.FindIndex(x => x.Id == _id);
                }
            }
            catch { }
        }

        private async Task ResetFormAsync()
        {
            _id = null;
            Title = "PHIẾU CHI - THÊM MỚI";
            DpNgay.SelectedDate = DateTime.Today;
            TxtSoPhieu.Text = await LocalPhieuThuChiService.GetNextSoPhieuAsync(isThu: false);
            TxtChungTuGoc.Text = "";
            CboLoaiDoiTuong.SelectedIndex = 0;
            TxtTenDoiTuong.Text = "";
            TxtDiaChi.Text = "";
            CboNhanVien.SelectedIndex = -1;
            CboKhachHang.SelectedIndex = -1;
            CboNhaCungCap.SelectedIndex = -1;
            TxtSoTien.Text = "0";
            ChkChuyenKhoan.IsChecked = false;
            CboTaiKhoanNganHang.IsEnabled = false;
            ChkKhongThayDoiCongNo.IsChecked = false;
            TxtGhiChu.Text = "";
            TxtTenDoiTuong.Focus();
        }

        private async Task LoadPhieuDetailAsync(string id)
        {
            try
            {
                var p = await LocalPhieuThuChiService.GetPhieuByIdAsync(id);
                if (p != null)
                {
                    _id = p.Id;
                    Title = "PHIẾU CHI - CHỈNH SỬA";
                    DpNgay.SelectedDate = p.Ngay ?? DateTime.Today;
                    TxtSoPhieu.Text = p.Name ?? "";
                    TxtChungTuGoc.Text = p.Chungtugoc ?? "";
                    TxtTenDoiTuong.Text = p.Tendoituong ?? "";
                    TxtDiaChi.Text = p.Diachi ?? "";
                    TxtGhiChu.Text = p.Note ?? "";
                    TxtSoTien.Text = p.Chi.HasValue ? p.Chi.Value.ToString("N0", CultureInfo.InvariantCulture) : "0";

                    ChkChuyenKhoan.IsChecked = p.Chuyenkhoan == "1" || !string.IsNullOrEmpty(p.DtaikhoannganhangId);
                    CboTaiKhoanNganHang.IsEnabled = ChkChuyenKhoan.IsChecked == true;
                    if (!string.IsNullOrEmpty(p.DtaikhoannganhangId))
                    {
                        CboTaiKhoanNganHang.SelectedValue = p.DtaikhoannganhangId;
                    }

                    ChkKhongThayDoiCongNo.IsChecked = p.Khongthaydoicongno == "1";

                    if (!string.IsNullOrEmpty(p.DlydothuchiId))
                    {
                        CboPhanLoai.SelectedValue = p.DlydothuchiId;
                        CboLyDoChi.SelectedValue = p.DlydothuchiId;
                    }

                    if (!string.IsNullOrEmpty(p.DnhanvienId)) CboNhanVien.SelectedValue = p.DnhanvienId;
                    if (!string.IsNullOrEmpty(p.DkhachhangId)) CboKhachHang.SelectedValue = p.DkhachhangId;
                    if (!string.IsNullOrEmpty(p.DnhacungcapId)) CboNhaCungCap.SelectedValue = p.DnhacungcapId;
                    if (!string.IsNullOrEmpty(p.DcuahangId)) CboCuaHang.SelectedValue = p.DcuahangId;
                }
            }
            catch { }
        }

        private void CboPhanLoai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboPhanLoai.SelectedItem != null && CboLyDoChi.SelectedItem != CboPhanLoai.SelectedItem)
            {
                CboLyDoChi.SelectedItem = CboPhanLoai.SelectedItem;
            }
        }

        private void CboLoaiDoiTuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboLoaiDoiTuong.SelectedItem is ComboBoxItem item)
            {
                string text = item.Content?.ToString() ?? "";
                if (text == "Khách hàng" && CboKhachHang != null)
                {
                    CboKhachHang.IsEnabled = true;
                    CboNhaCungCap.IsEnabled = false;
                    CboNhanVien.IsEnabled = false;
                }
                else if (text == "Nhà cung cấp" && CboNhaCungCap != null)
                {
                    CboKhachHang.IsEnabled = false;
                    CboNhaCungCap.IsEnabled = true;
                    CboNhanVien.IsEnabled = false;
                }
                else if (text == "Nhân viên" && CboNhanVien != null)
                {
                    CboKhachHang.IsEnabled = false;
                    CboNhaCungCap.IsEnabled = false;
                    CboNhanVien.IsEnabled = true;
                }
                else
                {
                    if (CboKhachHang != null) CboKhachHang.IsEnabled = true;
                    if (CboNhaCungCap != null) CboNhaCungCap.IsEnabled = true;
                    if (CboNhanVien != null) CboNhanVien.IsEnabled = true;
                }
            }
        }

        private void CboKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboKhachHang.SelectedItem != null)
            {
                dynamic kh = CboKhachHang.SelectedItem;
                TxtTenDoiTuong.Text = kh.NAME?.ToString() ?? "";
                TxtDiaChi.Text = kh.DIACHI?.ToString() ?? "";
            }
        }

        private void CboNhaCungCap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboNhaCungCap.SelectedItem != null)
            {
                dynamic ncc = CboNhaCungCap.SelectedItem;
                TxtTenDoiTuong.Text = ncc.NAME?.ToString() ?? "";
                TxtDiaChi.Text = ncc.DIACHI?.ToString() ?? "";
            }
        }

        private void CboNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboNhanVien.SelectedItem != null)
            {
                dynamic nv = CboNhanVien.SelectedItem;
                if (string.IsNullOrEmpty(TxtTenDoiTuong.Text))
                {
                    TxtTenDoiTuong.Text = nv.NAME?.ToString() ?? "";
                }
            }
        }

        private void ChkChuyenKhoan_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CboTaiKhoanNganHang != null)
            {
                CboTaiKhoanNganHang.IsEnabled = ChkChuyenKhoan.IsChecked == true;
            }
        }

        private void TxtSoTien_TextChanged(object sender, TextChangedEventArgs e)
        {
            string raw = TxtSoTien.Text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(raw, out decimal val))
            {
                int caret = TxtSoTien.CaretIndex;
                int oldLen = TxtSoTien.Text.Length;
                TxtSoTien.TextChanged -= TxtSoTien_TextChanged;
                TxtSoTien.Text = val.ToString("N0", CultureInfo.InvariantCulture);
                TxtSoTien.TextChanged += TxtSoTien_TextChanged;
                int newLen = TxtSoTien.Text.Length;
                TxtSoTien.CaretIndex = Math.Max(0, caret + (newLen - oldLen));
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string soPhieu = TxtSoPhieu.Text.Trim();
            if (string.IsNullOrEmpty(soPhieu))
            {
                MessageBox.Show("Vui lòng nhập số phiếu chi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoPhieu.Focus();
                return false;
            }

            string rawTien = TxtSoTien.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(rawTien, out decimal soTien) || soTien <= 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền chi hợp lệ (> 0)!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoTien.Focus();
                return false;
            }

            DateTime ngay = DpNgay.SelectedDate ?? DateTime.Today;
            string tenDoiTuong = TxtTenDoiTuong.Text.Trim();
            string diaChi = TxtDiaChi.Text.Trim();
            string chungTuGoc = TxtChungTuGoc.Text.Trim();
            string loaiDoiTuong = (CboLoaiDoiTuong.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string ghiChu = TxtGhiChu.Text.Trim();

            string lyDoId = CboLyDoChi.SelectedValue?.ToString();
            string nhanVienId = CboNhanVien.SelectedValue?.ToString();
            string khachHangId = CboKhachHang.SelectedValue?.ToString();
            string nhaCungCapId = CboNhaCungCap.SelectedValue?.ToString();
            string taiKhoanNganHangId = CboTaiKhoanNganHang.SelectedValue?.ToString();
            string cuaHangId = CboCuaHang.SelectedValue?.ToString();

            bool chuyenKhoan = ChkChuyenKhoan.IsChecked == true;
            bool khongThayDoiCongNo = ChkKhongThayDoiCongNo.IsChecked == true;

            var (success, error, savedId) = await LocalPhieuThuChiService.SavePhieuThuChiAsync(
                _id,
                soPhieu,
                ngay,
                isThu: false,
                soTien,
                tenDoiTuong,
                diaChi,
                loaiDoiTuong,
                chungTuGoc,
                ghiChu,
                lyDoId,
                nhanVienId,
                khachHangId,
                nhaCungCapId,
                taiKhoanNganHangId,
                cuaHangId,
                chuyenKhoan,
                khongThayDoiCongNo
            );

            if (success)
            {
                SavedId = savedId;
                _id = savedId;
                return true;
            }
            else
            {
                MessageBox.Show("Lỗi lưu phiếu chi: " + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Lưu phiếu chi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Lưu phiếu chi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllPhieuListAsync();
                await ResetFormAsync();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Lưu phiếu chi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show($"Lưu thành công!\nĐang gửi lệnh in phiếu chi '{TxtSoPhieu.Text}'...", "In phiếu chi", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show($"Xem trước bản in phiếu chi '{TxtSoPhieu.Text}'\nNgày: {DpNgay.SelectedDate:dd/MM/yyyy}\nĐối tượng: {TxtTenDoiTuong.Text}\nSố tiền: {TxtSoTien.Text} VNĐ", "Xem bản in", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            await ResetFormAsync();
        }

        private async void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            TxtSoPhieu.Text = await LocalPhieuThuChiService.GetNextSoPhieuAsync(isThu: false);
            Title = "PHIẾU CHI - THÊM MỚI (Sao chép)";
            MessageBox.Show("Đã sao chép nội dung sang phiếu mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuList != null && _allPhieuList.Count > 0)
            {
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                }
                else
                {
                    _currentIndex = _allPhieuList.Count - 1;
                }
                if (!string.IsNullOrEmpty(_allPhieuList[_currentIndex].Id))
                {
                    await LoadPhieuDetailAsync(_allPhieuList[_currentIndex].Id);
                }
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuList != null && _allPhieuList.Count > 0)
            {
                if (_currentIndex < _allPhieuList.Count - 1)
                {
                    _currentIndex++;
                }
                else
                {
                    _currentIndex = 0;
                }
                if (!string.IsNullOrEmpty(_allPhieuList[_currentIndex].Id))
                {
                    await LoadPhieuDetailAsync(_allPhieuList[_currentIndex].Id);
                }
            }
        }
    }
}
