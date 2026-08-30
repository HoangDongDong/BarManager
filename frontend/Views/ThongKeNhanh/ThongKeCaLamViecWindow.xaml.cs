using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class HoaDonThongKeViewModel
    {
        public string Id { get; set; }
        public string SoPhieu { get; set; }
        public DateTime? Ngay { get; set; }
        public DateTime? GioThanhToan { get; set; }
        public DateTime? BatDau { get; set; }
        public string KhachHang { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public decimal TongCong { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TienHang { get; set; }
        public string GhiChu { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string LoaiThanhToan { get; set; }
    }

    public class MatHangDaBanViewModel
    {
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string DonViTinh { get; set; }
        public decimal TiLeGiamGia { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class ThuChiViewModel
    {
        public string SoPhieu { get; set; }
        public string TenDoiTuong { get; set; }
        public string DienGiai { get; set; }
        public decimal Thu { get; set; }
        public decimal Chi { get; set; }
    }

    public class BanLookupItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public partial class ThongKeCaLamViecWindow : Window
    {
        private string _initialBanId;
        private bool _isDataLoading = false;

        public ThongKeCaLamViecWindow(string selectedBanId = null, string selectedBanName = null)
        {
            InitializeComponent();
            _initialBanId = selectedBanId;
            DpNgayXem.SelectedDate = DateTime.Today;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isDataLoading = true;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    var rawBans = (await conn.QueryAsync("SELECT ID, NAME FROM DBAN WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME")).ToList();
                    var bans = new List<BanLookupItem>
                    {
                        new BanLookupItem { Id = "", Name = "-- Tất cả bàn --" }
                    };

                    foreach (var b in rawBans)
                    {
                        var dict = (IDictionary<string, object>)b;
                        string id = dict["ID"]?.ToString() ?? "";
                        string name = dict["NAME"]?.ToString() ?? "";
                        bans.Add(new BanLookupItem { Id = id, Name = name });
                    }

                    CboBan.ItemsSource = bans;

                    if (!string.IsNullOrEmpty(_initialBanId))
                    {
                        var found = bans.FirstOrDefault(x => x.Id == _initialBanId);
                        if (found != null)
                        {
                            CboBan.SelectedValue = found.Id;
                        }
                        else
                        {
                            CboBan.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        CboBan.SelectedIndex = 0;
                    }
                }
            }
            catch { }
            finally
            {
                _isDataLoading = false;
            }

            await LoadThongKeDataAsync();
        }

        private async void CboBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && !_isDataLoading)
            {
                await LoadThongKeDataAsync();
            }
        }

        private async void DpNgayXem_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && !_isDataLoading)
            {
                await LoadThongKeDataAsync();
            }
        }

        private async Task LoadThongKeDataAsync()
        {
            try
            {
                DateTime selectedDate = DpNgayXem.SelectedDate ?? DateTime.Today;
                TxtCurrentDateHeader.Text = selectedDate.ToString("dd/MM/yyyy");
                string selectedBanId = CboBan?.SelectedValue?.ToString() ?? "";

                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // 1. Tải danh sách hóa đơn trong ngày
                    string sqlHoaDon = @"
                        SELECT 
                            h.ID, 
                            h.SOHD, 
                            h.SOORDER, 
                            h.NGAY, 
                            h.TIMECREATED,
                            h.GIOTHANHTOAN, 
                            h.BATDAU, 
                            h.DBANID,
                            k.NAME as KhachHangName, 
                            k.DIACHI as KhachHangDiaChi, 
                            k.DIENTHOAI as KhachHangDienThoai, 
                            h.TONGCONG, 
                            h.TIENGIAMGIA, 
                            h.TIENHANG, 
                            h.NOTE,
                            h.TIENMAT,
                            h.THE,
                            h.LOAITHANHTOAN,
                            h.STATUS
                        FROM TDONHANG h
                        LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                        WHERE (CAST(h.NGAY AS DATE) = @SelectedDate OR (h.NGAY IS NULL AND CAST(h.TIMECREATED AS DATE) = @SelectedDate))
                        ORDER BY h.ID DESC
                    ";

                    var rawList = (await conn.QueryAsync(sqlHoaDon, new { SelectedDate = selectedDate.Date })).ToList();

                    // Lọc theo bàn nếu có
                    if (!string.IsNullOrEmpty(selectedBanId))
                    {
                        rawList = rawList.Where(row => {
                            var d = (IDictionary<string, object>)row;
                            return d.ContainsKey("DBANID") && d["DBANID"]?.ToString() == selectedBanId;
                        }).ToList();
                    }

                    var hoaDonList = new List<HoaDonThongKeViewModel>();
                    foreach (var row in rawList)
                    {
                        var dict = (IDictionary<string, object>)row;
                        string id = dict.ContainsKey("ID") ? dict["ID"]?.ToString() ?? "" : "";
                        string soPhieu = dict.ContainsKey("SOHD") && dict["SOHD"] != null ? dict["SOHD"].ToString() :
                                        (dict.ContainsKey("SOORDER") && dict["SOORDER"] != null ? dict["SOORDER"].ToString() : id);
                        
                        DateTime? ngay = dict.ContainsKey("NGAY") && dict["NGAY"] is DateTime d ? d : (DateTime?)null;
                        DateTime? gioThanhToan = dict.ContainsKey("GIOTHANHTOAN") && dict["GIOTHANHTOAN"] is DateTime gtt ? gtt : (DateTime?)null;
                        DateTime? batDau = dict.ContainsKey("BATDAU") && dict["BATDAU"] is DateTime bd ? bd : (DateTime?)null;

                        string khachHang = dict.ContainsKey("KHACHHANGNAME") ? dict["KHACHHANGNAME"]?.ToString() : "";
                        string diaChi = dict.ContainsKey("KHACHHANGDIACHI") ? dict["KHACHHANGDIACHI"]?.ToString() : "";
                        string dienThoai = dict.ContainsKey("KHACHHANGDIENTHOAI") ? dict["KHACHHANGDIENTHOAI"]?.ToString() : "";
                        string ghiChu = dict.ContainsKey("NOTE") ? dict["NOTE"]?.ToString() : "";
                        string rawLoaiTT = dict.ContainsKey("LOAITHANHTOAN") ? dict["LOAITHANHTOAN"]?.ToString() : "0";
                        string loaiTT = "TienMat";
                        if (rawLoaiTT == "1" || rawLoaiTT == "ChuyenKhoan") loaiTT = "ChuyenKhoan";
                        else if (rawLoaiTT == "2" || rawLoaiTT == "TheATM" || rawLoaiTT == "The") loaiTT = "TheATM";
                        else if (rawLoaiTT == "3" || rawLoaiTT == "TheTraTruoc") loaiTT = "TheTraTruoc";
                        else if (rawLoaiTT == "4" || rawLoaiTT == "CongNo" || rawLoaiTT == "KhachNo") loaiTT = "CongNo";
                        else if (rawLoaiTT == "5" || rawLoaiTT == "Voucher") loaiTT = "Voucher";

                        decimal.TryParse(dict.ContainsKey("TONGCONG") ? dict["TONGCONG"]?.ToString() : "0", out decimal tongCong);
                        decimal.TryParse(dict.ContainsKey("TIENGIAMGIA") ? dict["TIENGIAMGIA"]?.ToString() : "0", out decimal tienGiamGia);
                        decimal.TryParse(dict.ContainsKey("TIENHANG") ? dict["TIENHANG"]?.ToString() : "0", out decimal tienHang);
                        decimal.TryParse(dict.ContainsKey("TIENMAT") ? dict["TIENMAT"]?.ToString() : "0", out decimal tienMatVal);
                        decimal.TryParse(dict.ContainsKey("THE") ? dict["THE"]?.ToString() : "0", out decimal theVal);

                        hoaDonList.Add(new HoaDonThongKeViewModel
                        {
                            Id = id,
                            SoPhieu = soPhieu,
                            Ngay = ngay,
                            GioThanhToan = gioThanhToan,
                            BatDau = batDau,
                            KhachHang = khachHang,
                            DiaChi = diaChi,
                            DienThoai = dienThoai,
                            TongCong = tongCong,
                            TienGiamGia = tienGiamGia,
                            TienHang = tienHang,
                            GhiChu = ghiChu,
                            TienMat = tienMatVal,
                            The = theVal,
                            LoaiThanhToan = loaiTT
                        });
                    }

                    DgHoaDon.ItemsSource = hoaDonList;

                    // 2. Tính tổng doanh thu theo hình thức
                    decimal tongTien = hoaDonList.Sum(x => x.TongCong);
                    decimal tienMat = hoaDonList.Where(x => x.LoaiThanhToan == "TienMat" || string.IsNullOrEmpty(x.LoaiThanhToan)).Sum(x => x.TongCong);
                    decimal chuyenKhoan = hoaDonList.Where(x => x.LoaiThanhToan == "ChuyenKhoan").Sum(x => x.TongCong);
                    decimal theATM = hoaDonList.Where(x => x.LoaiThanhToan == "TheATM" || x.LoaiThanhToan == "The").Sum(x => x.TongCong);
                    decimal theTraTruoc = hoaDonList.Where(x => x.LoaiThanhToan == "TheTraTruoc").Sum(x => x.TongCong);
                    decimal voucher = hoaDonList.Where(x => x.LoaiThanhToan == "Voucher").Sum(x => x.TongCong);
                    decimal congNo = hoaDonList.Where(x => x.LoaiThanhToan == "CongNo").Sum(x => x.TongCong);

                    TxtTienMat.Text = tienMat.ToString("N0");
                    TxtChuyenKhoan.Text = chuyenKhoan.ToString("N0");
                    TxtTheATM.Text = theATM.ToString("N0");
                    TxtVoucher.Text = voucher.ToString("N0");
                    TxtCongNo.Text = congNo.ToString("N0");
                    TxtTongDoanhThu.Text = tongTien.ToString("N0");

                    // 3. Tải danh sách mặt hàng đã bán trong ngày
                    string sqlMatHang = @"
                        SELECT 
                            c.TDONHANGID,
                            c.DMATHANGID,
                            c.TENHANG,
                            c.DONGIA,
                            c.SLXUAT,
                            c.THANHTIEN,
                            c.TILEGIAMGIA,
                            m.CODE as MatHangCode,
                            dvt.NAME as DvtName,
                            h.DBANID
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG h ON c.TDONHANGID = h.ID
                        LEFT JOIN DMATHANG m ON c.DMATHANGID = m.ID
                        LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                        WHERE (CAST(h.NGAY AS DATE) = @SelectedDate OR (h.NGAY IS NULL AND CAST(h.TIMECREATED AS DATE) = @SelectedDate))
                          AND (c.STATUS <> 0 OR c.STATUS IS NULL)
                    ";

                    var rawMatHangList = (await conn.QueryAsync(sqlMatHang, new { SelectedDate = selectedDate.Date })).ToList();
                    
                    if (!string.IsNullOrEmpty(selectedBanId))
                    {
                        rawMatHangList = rawMatHangList.Where(row => {
                            var d = (IDictionary<string, object>)row;
                            return d.ContainsKey("DBANID") && d["DBANID"]?.ToString() == selectedBanId;
                        }).ToList();
                    }

                    var matHangList = rawMatHangList
                        .Select(row => {
                            var d = (IDictionary<string, object>)row;
                            decimal.TryParse(d.ContainsKey("SLXUAT") ? d["SLXUAT"]?.ToString() : "0", out decimal sl);
                            decimal.TryParse(d.ContainsKey("DONGIA") ? d["DONGIA"]?.ToString() : "0", out decimal gia);
                            decimal.TryParse(d.ContainsKey("THANHTIEN") ? d["THANHTIEN"]?.ToString() : "0", out decimal tt);
                            decimal.TryParse(d.ContainsKey("TILEGIAMGIA") ? d["TILEGIAMGIA"]?.ToString() : "0", out decimal gg);
                            return new {
                                MaHang = d.ContainsKey("MATHANGCODE") ? d["MATHANGCODE"]?.ToString() ?? "" : "",
                                TenHang = d.ContainsKey("TENHANG") ? d["TENHANG"]?.ToString() ?? "" : "",
                                DonViTinh = d.ContainsKey("DVTNAME") ? d["DVTNAME"]?.ToString() ?? "" : "",
                                TiLeGiamGia = gg,
                                SoLuong = sl,
                                DonGia = gia,
                                ThanhTien = tt
                            };
                        })
                        .GroupBy(x => new { x.MaHang, x.TenHang, x.DonViTinh, x.TiLeGiamGia })
                        .Select(g => new MatHangDaBanViewModel {
                            MaHang = g.Key.MaHang,
                            TenHang = g.Key.TenHang,
                            DonViTinh = g.Key.DonViTinh,
                            TiLeGiamGia = g.Key.TiLeGiamGia,
                            SoLuong = g.Sum(x => x.SoLuong),
                            DonGia = g.Any() ? g.Average(x => x.DonGia) : 0,
                            ThanhTien = g.Sum(x => x.ThanhTien)
                        })
                        .OrderByDescending(x => x.ThanhTien)
                        .ToList();

                    DgMatHangDaBan.ItemsSource = matHangList;

                    // 4. Tải danh sách thu chi
                    var thuChiList = new List<ThuChiViewModel>();
                    try
                    {
                        string sqlThuChi = @"
                            SELECT 
                                SOPHIEU as SoPhieu, 
                                TENDOITUONG as TenDoiTuong, 
                                DIENGIAI as DienGiai, 
                                COALESCE(SOTIENTHU, 0) as Thu, 
                                COALESCE(SOTIENCHI, 0) as Chi 
                            FROM TTHUCHI 
                            WHERE CAST(NGAY AS DATE) = @SelectedDate
                        ";
                        thuChiList = (await conn.QueryAsync<ThuChiViewModel>(sqlThuChi, new { SelectedDate = selectedDate.Date })).ToList();
                    }
                    catch { }

                    DgThuChi.ItemsSource = thuChiList;
                    decimal tongThu = thuChiList.Sum(x => x.Thu);
                    decimal tongChi = thuChiList.Sum(x => x.Chi);
                    TxtThuChi.Text = (tongThu - tongChi).ToString("N0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXemDonHang_Click(object sender, RoutedEventArgs e)
        {
            if (DgHoaDon.SelectedItem is HoaDonThongKeViewModel selected)
            {
                MessageBox.Show($"Chi tiết hóa đơn số: {selected.SoPhieu}\nKhách hàng: {selected.KhachHang}\nTổng thanh toán: {selected.TongCong:N0} đ", "Chi tiết đơn hàng", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn từ danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnInBaoCao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new InLuoiWindow(DgHoaDon, $"BÁO CÁO THỐNG KÊ DOANH THU CA - NGÀY {TxtCurrentDateHeader.Text}");
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in báo cáo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
