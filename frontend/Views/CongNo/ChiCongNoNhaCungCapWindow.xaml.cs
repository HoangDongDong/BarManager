using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CongNo
{
    public partial class ChiCongNoNhaCungCapWindow : Window
    {
        private CongNoNhaCungCapViewModel _ncc;
        private List<dynamic> _khachHangList = new List<dynamic>();
        private List<dynamic> _nhaCungCapList = new List<dynamic>();
        private List<dynamic> _nhanVienList = new List<dynamic>();
        private List<dynamic> _lyDoList = new List<dynamic>();
        private List<dynamic> _cuaHangList = new List<dynamic>();
        private List<dynamic> _taiKhoanList = new List<dynamic>();

        public bool IsSaved { get; private set; } = false;

        public ChiCongNoNhaCungCapWindow(CongNoNhaCungCapViewModel ncc = null)
        {
            InitializeComponent();
            _ncc = ncc;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DpNgay.SelectedDate = DateTime.Today;

                // 1. Sinh số phiếu chi tự động theo mẫu: PC26/00001
                TxtSoPhieu.Text = await LocalCongNoNhaCungCapService.GetNextSoPhieuChiAsync();

                // 2. Nạp các danh mục
                _nhaCungCapList = await LocalCongNoNhaCungCapService.GetNhaCungCapLookupAsync();
                CboNhaCungCap.ItemsSource = _nhaCungCapList;

                _khachHangList = await LocalCongNoNhaCungCapService.GetKhachHangLookupAsync();
                CboKhachHang.ItemsSource = _khachHangList;

                _nhanVienList = await LocalCongNoNhaCungCapService.GetNhanVienLookupAsync();
                CboNhanVien.ItemsSource = _nhanVienList;

                _lyDoList = await LocalCongNoNhaCungCapService.GetLyDoChiLookupAsync();
                CboLyDoChi.ItemsSource = _lyDoList;
                if (_lyDoList.Count > 0)
                {
                    int selectedIdx = 0;
                    for (int i = 0; i < _lyDoList.Count; i++)
                    {
                        string name = _lyDoList[i].NAME?.ToString() ?? "";
                        if (name.Contains("công nợ", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("nhà cung cấp", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("mua hàng", StringComparison.OrdinalIgnoreCase))
                        {
                            selectedIdx = i;
                            break;
                        }
                    }
                    CboLyDoChi.SelectedIndex = selectedIdx;
                }

                _cuaHangList = await LocalCongNoNhaCungCapService.GetCuaHangLookupAsync();
                CboCuaHang.ItemsSource = _cuaHangList;
                if (_cuaHangList.Count > 0)
                {
                    CboCuaHang.SelectedIndex = 0;
                }

                _taiKhoanList = await LocalCongNoNhaCungCapService.GetTaiKhoanNganHangLookupAsync();
                CboTaiKhoanNganHang.ItemsSource = _taiKhoanList;
                if (_taiKhoanList.Count > 0)
                {
                    CboTaiKhoanNganHang.SelectedIndex = 0;
                }

                // Phân loại
                CboPhanLoai.Items.Add("Chi trả nợ nhà cung cấp");
                CboPhanLoai.Items.Add("Chi tiền mua hàng");
                CboPhanLoai.Items.Add("Chi tiền dịch vụ");
                CboPhanLoai.Items.Add("Chi khác");
                CboPhanLoai.SelectedIndex = 0;

                // Điền thông tin nhà cung cấp nếu đã chọn trước
                if (_ncc != null)
                {
                    TxtTenDoiTuong.Text = _ncc.Name;
                    TxtDiaChi.Text = _ncc.DiaChi;
                    TxtSoTien.Text = _ncc.ConNo > 0 ? _ncc.ConNo.ToString("N0", new CultureInfo("en-US")) : "0";

                    // Chọn trong ComboBox Nhà cung cấp
                    for (int i = 0; i < _nhaCungCapList.Count; i++)
                    {
                        if (_nhaCungCapList[i].ID?.ToString() == _ncc.Id)
                        {
                            CboNhaCungCap.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    TxtSoTien.Text = "0";
                }

                TxtSoTien.Focus();
                TxtSoTien.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo phiếu chi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CboLoaiDoiTuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboKhachHang == null || CboNhaCungCap == null) return;

            string loai = (CboLoaiDoiTuong.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Nhà cung cấp";
            if (loai == "Nhà cung cấp")
            {
                CboNhaCungCap.IsEnabled = true;
                CboKhachHang.IsEnabled = false;
            }
            else if (loai == "Khách hàng")
            {
                CboNhaCungCap.IsEnabled = false;
                CboKhachHang.IsEnabled = true;
            }
            else
            {
                CboKhachHang.IsEnabled = false;
                CboNhaCungCap.IsEnabled = false;
            }
        }

        private void CboNhaCungCap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboNhaCungCap.SelectedItem != null)
            {
                dynamic selected = CboNhaCungCap.SelectedItem;
                TxtTenDoiTuong.Text = selected.NAME?.ToString() ?? "";
                TxtDiaChi.Text = selected.DIACHI?.ToString() ?? "";
            }
        }

        private void ChkChuyenKhoan_Checked(object sender, RoutedEventArgs e)
        {
            if (CboTaiKhoanNganHang != null) CboTaiKhoanNganHang.IsEnabled = true;
        }

        private void ChkChuyenKhoan_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CboTaiKhoanNganHang != null) CboTaiKhoanNganHang.IsEnabled = false;
        }

        private void TxtSoTien_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSoTien.Text)) return;

            string text = TxtSoTien.Text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(text, out decimal val))
            {
                int caret = TxtSoTien.CaretIndex;
                int oldLength = TxtSoTien.Text.Length;
                string formatted = val.ToString("N0", new CultureInfo("en-US"));
                if (TxtSoTien.Text != formatted)
                {
                    TxtSoTien.Text = formatted;
                    int newLength = TxtSoTien.Text.Length;
                    TxtSoTien.CaretIndex = Math.Max(0, Math.Min(newLength, caret + (newLength - oldLength)));
                }
            }
        }

        private decimal GetSoTienValue()
        {
            string raw = TxtSoTien.Text.Replace(".", "").Replace(",", "").Trim();
            if (decimal.TryParse(raw, out decimal val))
            {
                return val;
            }
            return 0;
        }

        private async Task<bool> SaveDataAsync()
        {
            string soPhieu = TxtSoPhieu.Text.Trim();
            if (string.IsNullOrEmpty(soPhieu))
            {
                MessageBox.Show("Vui lòng nhập hoặc kiểm tra số phiếu chi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoPhieu.Focus();
                return false;
            }

            decimal soTien = GetSoTienValue();
            if (soTien <= 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền chi lớn hơn 0!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoTien.Focus();
                return false;
            }

            DateTime ngay = DpNgay.SelectedDate ?? DateTime.Today;
            string tenDoiTuong = TxtTenDoiTuong.Text.Trim();
            string diaChi = TxtDiaChi.Text.Trim();
            string chungTuGoc = TxtChungTuGoc.Text.Trim();
            string ghiChu = TxtGhiChu.Text.Trim();

            string loaiDoiTuong = (CboLoaiDoiTuong.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Nhà cung cấp";
            string nccId = CboNhaCungCap.SelectedValue?.ToString() ?? (_ncc?.Id ?? "");
            string khachId = CboKhachHang.SelectedValue?.ToString() ?? "";
            string nhanVienId = CboNhanVien.SelectedValue?.ToString() ?? "";
            string lyDoId = CboLyDoChi.SelectedValue?.ToString() ?? "";
            string cuaHangId = CboCuaHang.SelectedValue?.ToString() ?? "";
            string taiKhoanId = CboTaiKhoanNganHang.SelectedValue?.ToString() ?? "";
            bool isChuyenKhoan = ChkChuyenKhoan.IsChecked == true;
            bool khongThayDoiCongNo = ChkKhongThayDoiCongNo.IsChecked == true;

            string dienGiai = !string.IsNullOrEmpty(ghiChu) ? ghiChu : (CboPhanLoai.Text ?? "Chi tiền nhà cung cấp");

            try
            {
                bool ok = await LocalCongNoNhaCungCapService.SavePhieuChiFullAsync(
                    soPhieu,
                    ngay,
                    soTien,
                    nccId,
                    tenDoiTuong,
                    diaChi,
                    lyDoId,
                    nhanVienId,
                    khachId,
                    taiKhoanId,
                    cuaHangId,
                    isChuyenKhoan,
                    khongThayDoiCongNo,
                    dienGiai,
                    chungTuGoc,
                    loaiDoiTuong
                );

                if (ok)
                {
                    IsSaved = true;
                    return true;
                }
                else
                {
                    MessageBox.Show("Lưu phiếu chi thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu phiếu chi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu phiếu chi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                // Sinh số phiếu mới sẵn sàng nhập tiếp
                TxtSoPhieu.Text = await LocalCongNoNhaCungCapService.GetNextSoPhieuChiAsync();
                TxtSoTien.Text = "0";
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show($"Đã lưu và gửi lệnh in phiếu chi: {TxtSoPhieu.Text}", "In phiếu chi", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show($"Xem trước bản in phiếu chi: {TxtSoPhieu.Text}\nSố tiền: {TxtSoTien.Text} VNĐ\nĐối tượng: {TxtTenDoiTuong.Text}", "Xem trước bản in", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
