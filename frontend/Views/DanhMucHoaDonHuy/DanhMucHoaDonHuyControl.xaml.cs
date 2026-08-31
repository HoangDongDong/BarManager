using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucHoaDonHuyControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private bool _isLoaded = false;

        public DanhMucHoaDonHuyControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            this.IsVisibleChanged += DanhMucHoaDonHuyControl_IsVisibleChanged;
        }

        private async void DanhMucHoaDonHuyControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && _isLoaded)
            {
                await LoadDataAsync();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpDenNgay.SelectedDate = DateTime.Now;
            await LoadDataAsync();
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                await LoadDataAsync();
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
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

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
                string kw = txtLoc?.Text;

                var list = await _hoaDonService.GetHoaDonHuyListAsync(tuNgay, denNgay, kw);
                DgHoaDonHuy.ItemsSource = list;

                if (list.Count > 0)
                {
                    DgHoaDonHuy.SelectedIndex = 0;
                }
                else
                {
                    DgChiTiet.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục hóa đơn hủy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgHoaDonHuy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgHoaDonHuy.SelectedItem is HoaDonHuyViewModel selectedHoaDonHuy)
            {
                try
                {
                    var chiTietList = await _hoaDonService.GetChiTietHoaDonHuyAsync(selectedHoaDonHuy.Id);
                    DgChiTiet.ItemsSource = chiTietList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết hóa đơn hủy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                DgChiTiet.ItemsSource = null;
            }
        }

        #region CHUỘT PHẢI HÀNG TRÊN (HÓA ĐƠN HỦY)

        private void MnuHoaDonHuy_SaoChep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgHoaDonHuy.SelectedItem is HoaDonHuyViewModel item)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Số phiếu: {item.SoPhieu}");
                    sb.AppendLine($"Ngày hủy: {item.NgayHuy:dd/MM/yyyy} {item.GioHuy:HH:mm}");
                    sb.AppendLine($"Thu ngân hủy: {item.ThuNganHuy}");
                    sb.AppendLine($"Lý do hủy: {item.LyDoHuy}");
                    sb.AppendLine($"Tiền hàng: {item.TienHang:N0}");
                    Clipboard.SetText(sb.ToString());
                    MessageBox.Show("Đã sao chép thông tin hóa đơn hủy vào Clipboard!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sao chép: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuHoaDonHuy_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng tùy chỉnh cột hiển thị đang được áp dụng theo chế độ mặc định.", "Cột hiển thị", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void MnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void MnuTimKiem_Click(object sender, RoutedEventArgs e)
        {
            txtLoc?.Focus();
            txtLoc?.SelectAll();
        }

        private void MnuHoaDonHuy_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(DgHoaDonHuy, "DANH MỤC HÓA ĐƠN HỦY");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in danh sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuHoaDonHuy_TuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgHoaDonHuy.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MnuHoaDonHuy_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (DgHoaDonHuy.SelectedItem is HoaDonHuyViewModel item)
            {
                MessageBox.Show($"[Thuộc tính hóa đơn hủy]\n- Số phiếu: {item.SoPhieu}\n- Ngày: {item.Ngay:dd/MM/yyyy}\n- Khách hàng: {item.KhachHang}\n- Thu ngân hủy: {item.ThuNganHuy}\n- Lý do: {item.LyDoHuy}\n- Tổng tiền: {item.TienHang:N0} VNĐ", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region CHUỘT PHẢI HÀNG DƯỚI (CHI TIẾT HÓA ĐƠN HỦY)

        private void MnuChiTiet_SaoChep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgChiTiet.SelectedItem is ChiTietHoaDonHuyViewModel item)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Mã hàng: {item.MaHang}");
                    sb.AppendLine($"Tên hàng: {item.TenHang}");
                    sb.AppendLine($"ĐVT: {item.Dvt}");
                    sb.AppendLine($"Số lượng: {item.SoLuong:0.##}");
                    sb.AppendLine($"Đơn giá: {item.DonGia:N0}");
                    sb.AppendLine($"Thành tiền: {item.ThanhTien:N0}");
                    Clipboard.SetText(sb.ToString());
                    MessageBox.Show("Đã sao chép chi tiết món hủy vào Clipboard!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sao chép: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuChiTiet_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng tùy chỉnh cột hiển thị chi tiết đang được áp dụng theo chế độ mặc định.", "Cột hiển thị", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MnuChiTiet_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(DgChiTiet, "CHI TIẾT MÓN HÓA ĐƠN HỦY");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in danh sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuChiTiet_TuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgChiTiet.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MnuChiTiet_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet.SelectedItem is ChiTietHoaDonHuyViewModel item)
            {
                MessageBox.Show($"[Chi tiết món hủy]\n- Tên hàng: {item.TenHang}\n- Đơn vị: {item.Dvt}\n- Số lượng: {item.SoLuong:0.##}\n- Đơn giá: {item.DonGia:N0} VNĐ\n- Thành tiền: {item.ThanhTien:N0} VNĐ\n- Ghi chú: {item.GhiChu}", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion
    }
}
