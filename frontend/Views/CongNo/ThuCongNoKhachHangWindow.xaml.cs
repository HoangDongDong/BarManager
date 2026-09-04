using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CongNo
{
    public partial class ThuCongNoKhachHangWindow : Window
    {
        private CongNoKhachHangViewModel _khach;
        private List<dynamic> _khachHangList = new List<dynamic>();
        private List<dynamic> _nhaCungCapList = new List<dynamic>();
        private List<dynamic> _nhanVienList = new List<dynamic>();
        private List<dynamic> _lyDoList = new List<dynamic>();
        private List<dynamic> _cuaHangList = new List<dynamic>();
        private List<dynamic> _taiKhoanList = new List<dynamic>();

        public bool IsSaved { get; private set; } = false;

        public ThuCongNoKhachHangWindow(CongNoKhachHangViewModel khach = null)
        {
            InitializeComponent();
            _khach = khach;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DpNgay.SelectedDate = DateTime.Today;

                // 1. Sinh số phiếu thu tự động theo mẫu: PT26/00001
                TxtSoPhieu.Text = await LocalCongNoKhachHangService.GetNextSoPhieuThuAsync();

                // 2. Nạp các danh mục
                _khachHangList = await LocalCongNoKhachHangService.GetKhachHangLookupAsync();
                CboKhachHang.ItemsSource = _khachHangList;

                _nhaCungCapList = await LocalCongNoKhachHangService.GetNhaCungCapLookupAsync();
                CboNhaCungCap.ItemsSource = _nhaCungCapList;

                _nhanVienList = await LocalCongNoKhachHangService.GetNhanVienLookupAsync();
                CboNhanVien.ItemsSource = _nhanVienList;

                _lyDoList = await LocalCongNoKhachHangService.GetLyDoThuLookupAsync();
                CboLyDoThu.ItemsSource = _lyDoList;

                _cuaHangList = await LocalCongNoKhachHangService.GetCuaHangLookupAsync();
                CboCuaHang.ItemsSource = _cuaHangList;
                if (_cuaHangList.Count > 0)
                {
                    CboCuaHang.SelectedIndex = 0;
                }

                _taiKhoanList = await LocalCongNoKhachHangService.GetTaiKhoanNganHangLookupAsync();
                CboTaiKhoanNganHang.ItemsSource = _taiKhoanList;
                if (_taiKhoanList.Count > 0)
                {
                    CboTaiKhoanNganHang.SelectedIndex = 0;
                }

                // Phân loại
                CboPhanLoai.Items.Add("Thu tiền công nợ");
                CboPhanLoai.Items.Add("Thu tiền bán hàng");
                CboPhanLoai.Items.Add("Thu đặt cọc");
                CboPhanLoai.Items.Add("Thu khác");
                CboPhanLoai.SelectedIndex = 0;

                // Điền thông tin khách hàng nếu đã chọn trước
                if (_khach != null)
                {
                    TxtTenDoiTuong.Text = _khach.Name;
                    TxtDiaChi.Text = _khach.Diachi;
                    TxtSoTien.Text = _khach.ConNo > 0 ? _khach.ConNo.ToString("N0") : "0";

                    // Chọn trong ComboBox Khách hàng
                    for (int i = 0; i < _khachHangList.Count; i++)
                    {
                        if (_khachHangList[i].ID?.ToString() == _khach.Id)
                        {
                            CboKhachHang.SelectedIndex = i;
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
                MessageBox.Show("Lỗi khởi tạo phiếu thu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CboLoaiDoiTuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboKhachHang == null || CboNhaCungCap == null) return;

            string loai = (CboLoaiDoiTuong.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Khách hàng";
            if (loai == "Khách hàng")
            {
                CboKhachHang.IsEnabled = true;
                CboNhaCungCap.IsEnabled = false;
            }
            else if (loai == "Nhà cung cấp")
            {
                CboKhachHang.IsEnabled = false;
                CboNhaCungCap.IsEnabled = true;
            }
            else
            {
                CboKhachHang.IsEnabled = false;
                CboNhaCungCap.IsEnabled = false;
            }
        }

        private void CboKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboKhachHang.SelectedItem != null)
            {
                dynamic selected = CboKhachHang.SelectedItem;
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
                int oldLen = TxtSoTien.Text.Length;

                TxtSoTien.TextChanged -= TxtSoTien_TextChanged;
                TxtSoTien.Text = val.ToString("N0");
                TxtSoTien.TextChanged += TxtSoTien_TextChanged;

                int newLen = TxtSoTien.Text.Length;
                TxtSoTien.CaretIndex = Math.Max(0, caret + (newLen - oldLen));
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string amountText = TxtSoTien.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(amountText, out decimal soTien) || soTien <= 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền thu hợp lệ lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoTien.Focus();
                return false;
            }

            DateTime ngay = DpNgay.SelectedDate ?? DateTime.Today;
            string soPhieu = TxtSoPhieu.Text.Trim();
            if (string.IsNullOrEmpty(soPhieu))
            {
                soPhieu = await LocalCongNoKhachHangService.GetNextSoPhieuThuAsync();
            }

            string phanLoai = CboPhanLoai.Text?.Trim() ?? "Thu tiền công nợ";
            string lyDoId = CboLyDoThu.SelectedValue?.ToString();
            string chungTuGoc = TxtChungTuGoc.Text.Trim();
            string loaiDoiTuong = (CboLoaiDoiTuong.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Khách hàng";
            string tenDoiTuong = TxtTenDoiTuong.Text.Trim();
            string diaChi = TxtDiaChi.Text.Trim();
            string nhanVienId = CboNhanVien.SelectedValue?.ToString();
            string khachHangId = (loaiDoiTuong == "Khách hàng") ? CboKhachHang.SelectedValue?.ToString() : null;
            string nhaCungCapId = (loaiDoiTuong == "Nhà cung cấp") ? CboNhaCungCap.SelectedValue?.ToString() : null;
            int chuyenKhoan = (ChkChuyenKhoan.IsChecked == true) ? 1 : 0;
            string taiKhoanId = chuyenKhoan == 1 ? CboTaiKhoanNganHang.SelectedValue?.ToString() : null;
            string cuaHangId = CboCuaHang.SelectedValue?.ToString();
            int khongThayDoiCongNo = (ChkKhongThayDoiCongNo.IsChecked == true) ? 1 : 0;
            string ghiChu = TxtGhiChu.Text.Trim();

            bool success = await LocalCongNoKhachHangService.SavePhieuThuFullAsync(
                ngay,
                soPhieu,
                phanLoai,
                lyDoId,
                chungTuGoc,
                loaiDoiTuong,
                tenDoiTuong,
                diaChi,
                nhanVienId,
                khachHangId,
                nhaCungCapId,
                soTien,
                chuyenKhoan,
                taiKhoanId,
                cuaHangId,
                khongThayDoiCongNo,
                ghiChu
            );

            if (success)
            {
                IsSaved = true;
                return true;
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi lưu phiếu thu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu thu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                TxtSoPhieu.Text = await LocalCongNoKhachHangService.GetNextSoPhieuThuAsync();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu thu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void BtnLuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu thu thành công! Đang chuyển lệnh in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void BtnLuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu phiếu thu thành công! Đang mở xem trước bản in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
