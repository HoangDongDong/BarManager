using System;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThongKeDoanhThuControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;

        public ThongKeDoanhThuControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = DateTime.Now;
            dpDenNgay.SelectedDate = DateTime.Now;
            
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Now;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Now;
                
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
                TxtGiamGiaTienHang.Text = "-" + tongGiamGiaTienHang.ToString("N0");
                TxtTongGiamGia.Text = "-" + tongGiamGiaTienHang.ToString("N0");
                TxtTongDoanhThu.Text = tongTongDoanhThu.ToString("N0");
                
                TxtTienMat.Text = tongTienMat.ToString("N0");
                TxtTienThe.Text = tongTienThe.ToString("N0");
                
                decimal tongThucThu = tongTienMat + tongTienThe;
                TxtTongThucThu.Text = tongThucThu.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgHoaDon.SelectedItem is QuanLyBar.Client.Models.HoaDonViewModel selectedHoaDon)
            {
                try
                {
                    if (int.TryParse(selectedHoaDon.Id, out int donHangId))
                    {
                        var chiTietList = await _hoaDonService.GetChiTietHoaDonAsync(donHangId);
                        DgChiTiet.ItemsSource = chiTietList;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
