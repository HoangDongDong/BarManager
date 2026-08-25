using System.Collections.Generic;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
    public class BanHangRowViewModel
    {
        public string Stt { get; set; }
        public string SoHd { get; set; }
        public string GioTt { get; set; }
        public string KhachHang { get; set; }
        public string TongCong { get; set; }
        public string TienMat { get; set; }
        public string TheAtm { get; set; }
        public string DatTruoc { get; set; }
        public string ConNo { get; set; }
    }

    public class MatHangBanRowViewModel
    {
        public string Stt { get; set; }
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string Dvt { get; set; }
        public string SoLuong { get; set; }
        public string DonGia { get; set; }
        public string GGia { get; set; }
        public string ThanhTien { get; set; }
    }

    public class NhapHangRowViewModel
    {
        public string Stt { get; set; }
        public string SoPhieu { get; set; }
        public string NhaCungCap { get; set; }
        public string TongCong { get; set; }
        public string TienThanhToan { get; set; }
        public string ConNo { get; set; }
    }

    public partial class ChiTietHoatDongControl : UserControl
    {
        private readonly QuanLyBar.Client.Services.LocalHoaDonService _hoaDonService;

        public ChiTietHoatDongControl()
        {
            InitializeComponent();
            _hoaDonService = new QuanLyBar.Client.Services.LocalHoaDonService();
            this.Loaded += ChiTietHoatDongControl_Loaded;
        }

        private async void ChiTietHoatDongControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = System.DateTime.Now;
            dpDenNgay.SelectedDate = System.DateTime.Now;
            await LoadDataAsync();
        }

        private async void BtnTaiDuLieu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? System.DateTime.Now;
                var denNgay = dpDenNgay.SelectedDate ?? System.DateTime.Now;

                txtNgayBaoCao.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";
                txtNgayKy.Text = $"Ngày {System.DateTime.Now:dd} tháng {System.DateTime.Now:MM} năm {System.DateTime.Now:yyyy}";

                // 1. Bán hàng
                var hoaDons = await _hoaDonService.GetHoaDonListAsync(tuNgay, denNgay);
                var banHangs = new List<BanHangRowViewModel>();
                int stt = 1;
                decimal tongCongAll = 0;
                decimal tienMatAll = 0;
                decimal theAtmAll = 0;

                foreach (var hd in hoaDons)
                {
                    banHangs.Add(new BanHangRowViewModel 
                    { 
                        Stt = stt.ToString(), 
                        SoHd = hd.SoPhieu, 
                        GioTt = hd.GioThanhToan?.ToString("HH:mm"), 
                        KhachHang = hd.Ban, 
                        TongCong = hd.TongCong.ToString("N0"), 
                        TienMat = hd.TienMat.ToString("N0"), 
                        TheAtm = hd.TheThanhToan.ToString("N0"), 
                        DatTruoc = "0", 
                        ConNo = "0" 
                    });
                    tongCongAll += hd.TongCong;
                    tienMatAll += hd.TienMat;
                    theAtmAll += hd.TheThanhToan;
                    stt++;
                }
                banHangs.Add(new BanHangRowViewModel 
                { 
                    Stt = "", 
                    SoHd = "TỔNG CỘNG", 
                    GioTt = "", 
                    KhachHang = "", 
                    TongCong = tongCongAll.ToString("N0"), 
                    TienMat = tienMatAll.ToString("N0"), 
                    TheAtm = theAtmAll.ToString("N0"), 
                    DatTruoc = "0", 
                    ConNo = "0" 
                });
                DgBanHang.ItemsSource = banHangs;

                // Cập nhật Footer
                txtTienMat.Text = tienMatAll.ToString("N0");
                txtTienThe.Text = theAtmAll.ToString("N0");

                // 2. Mặt hàng bán (Lấy chi tiết của tất cả hóa đơn trong ngày)
                var matHangHangs = new List<MatHangBanRowViewModel>();
                int sttMh = 1;
                decimal slTong = 0;
                decimal tienHangTong = 0;

                foreach (var hd in hoaDons)
                {
                    if (int.TryParse(hd.Id, out int donHangId))
                    {
                        var chiTiets = await _hoaDonService.GetChiTietHoaDonAsync(donHangId);
                        foreach (var ct in chiTiets)
                        {
                            matHangHangs.Add(new MatHangBanRowViewModel
                            {
                                Stt = sttMh.ToString(),
                                MaHang = "",
                                TenHang = ct.TenMon,
                                Dvt = ct.Dvt,
                                SoLuong = ct.SoLuong.ToString("N2"),
                                DonGia = ct.DonGia.ToString("N0"),
                                GGia = ct.PhanTramGiamGia.ToString("N2"),
                                ThanhTien = ct.ThanhTien.ToString("N0")
                            });
                            slTong += ct.SoLuong;
                            tienHangTong += ct.ThanhTien;
                            sttMh++;
                        }
                    }
                }
                matHangHangs.Add(new MatHangBanRowViewModel { Stt = "", MaHang = "", TenHang = "TIỀN HÀNG CHƯA GIẢM GIÁ", Dvt = "", SoLuong = slTong.ToString("N2"), DonGia = "", GGia = "", ThanhTien = tienHangTong.ToString("N0") });
                matHangHangs.Add(new MatHangBanRowViewModel { Stt = "", MaHang = "", TenHang = "TỔNG CỘNG", Dvt = "", SoLuong = "", DonGia = "", GGia = "", ThanhTien = tienHangTong.ToString("N0") });
                DgMatHangBan.ItemsSource = matHangHangs;

                // 3. Nhập hàng (Mock empty for now)
                var nhapHangList = new List<NhapHangRowViewModel>();
                DgNhapHang.ItemsSource = nhapHangList;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
