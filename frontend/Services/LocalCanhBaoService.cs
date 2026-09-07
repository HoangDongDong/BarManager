using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class HangDuoiMucAnToanItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string DonViTinh { get; set; } = "";
        public decimal TonHienTai { get; set; }
        public decimal TonToiThieu { get; set; }
        public decimal TonToiDa { get; set; }
        public decimal SoLuongCanNhap => TonToiThieu > TonHienTai ? (TonToiThieu - TonHienTai) : 0;
        public string TonHienTaiFormatted => TonHienTai.ToString("N0");
        public string TonToiThieuFormatted => TonToiThieu.ToString("N0");
        public string SoLuongCanNhapFormatted => SoLuongCanNhap.ToString("N0");
    }

    public class KhachHangSinhNhatItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string MaKhach { get; set; } = "";
        public string TenKhach { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public DateTime? NgaySinh { get; set; }
        public string NgaySinhFormatted => NgaySinh?.ToString("dd/MM/yyyy") ?? "";
        public string DiaChi { get; set; } = "";
        public string TenNhomKhach { get; set; } = "";
    }

    public class DotKhuyenMaiCanhBaoItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string TenKm { get; set; } = "";
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string TuNgayFormatted => TuNgay?.ToString("dd/MM/yyyy") ?? "";
        public string DenNgayFormatted => DenNgay?.ToString("dd/MM/yyyy") ?? "";
        public decimal GiamGia { get; set; }
        public string GiamGiaFormatted => GiamGia > 0 ? $"{GiamGia:N0}%" : "Theo hóa đơn";
        public string GhiChu { get; set; } = "";
    }

    public class CanhBaoSummary
    {
        public List<HangDuoiMucAnToanItem> HangDuoiMucAnToan { get; set; } = new List<HangDuoiMucAnToanItem>();
        public List<KhachHangSinhNhatItem> KhachHangSinhNhat { get; set; } = new List<KhachHangSinhNhatItem>();
        public List<DotKhuyenMaiCanhBaoItem> ChuongTrinhKhuyenMai { get; set; } = new List<DotKhuyenMaiCanhBaoItem>();

        public bool HasAlerts => HangDuoiMucAnToan.Count > 0 || KhachHangSinhNhat.Count > 0 || ChuongTrinhKhuyenMai.Count > 0;
    }

    public static class LocalCanhBaoService
    {
        public static async Task<List<HangDuoiMucAnToanItem>> GetHangDuoiMucAnToanListAsync()
        {
            var list = new List<HangDuoiMucAnToanItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    // 1. Lấy tất cả mặt hàng có quy định Tồn tối thiểu > 0
                    string sqlMatHang = @"
                        SELECT 
                            CAST(m.ID AS VARCHAR(50)) as Id,
                            m.CODE as MaHang,
                            m.NAME as TenHang,
                            dvt.NAME as DonViTinh,
                            COALESCE(m.TONTOITHIEU, 0) as TonToiThieu,
                            COALESCE(m.TONTOIDA, 0) as TonToiDa
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                          AND m.TONTOITHIEU IS NOT NULL 
                          AND m.TONTOITHIEU > 0
                        ORDER BY m.NAME";

                    var matHangs = (await conn.QueryAsync(sqlMatHang)).ToList();
                    if (matHangs.Count == 0) return list;

                    // 2. Tính tồn kho thực tế hiện tại
                    string sqlTon = @"
                        SELECT 
                            CAST(c.DMATHANGID AS VARCHAR(50)) as DmathangId,
                            SUM(COALESCE(c.SLNHAP, 0) - COALESCE(c.SLXUAT, 0)) as Ton
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        WHERE (c.STATUS IS NULL OR c.STATUS <> 0)
                          AND (h.STATUS IS NULL OR h.STATUS <> 0)
                        GROUP BY c.DMATHANGID";

                    var tonRows = (await conn.QueryAsync(sqlTon)).ToList();
                    var tonDict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    foreach (var tr in tonRows)
                    {
                        string mhId = tr.DMATHANGID?.ToString() ?? "";
                        decimal ton = tr.TON != null ? Convert.ToDecimal(tr.TON) : 0m;
                        if (!string.IsNullOrEmpty(mhId)) tonDict[mhId] = ton;
                    }

                    int stt = 1;
                    foreach (var mh in matHangs)
                    {
                        string id = mh.ID?.ToString() ?? "";
                        decimal tonHienTai = tonDict.TryGetValue(id, out var t) ? t : 0m;
                        decimal toiThieu = mh.TONTOITHIEU != null ? Convert.ToDecimal(mh.TONTOITHIEU) : 0m;
                        decimal toiDa = mh.TONTOIDA != null ? Convert.ToDecimal(mh.TONTOIDA) : 0m;

                        if (tonHienTai < toiThieu)
                        {
                            list.Add(new HangDuoiMucAnToanItem
                            {
                                Stt = stt++,
                                Id = id,
                                MaHang = mh.MAHANG?.ToString() ?? "",
                                TenHang = mh.TENHANG?.ToString() ?? "",
                                DonViTinh = mh.DONVITINH?.ToString() ?? "",
                                TonHienTai = tonHienTai,
                                TonToiThieu = toiThieu,
                                TonToiDa = toiDa
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetHangDuoiMucAnToanListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<KhachHangSinhNhatItem>> GetKhachHangSinhNhatListAsync()
        {
            var list = new List<KhachHangSinhNhatItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    DateTime today = DateTime.Today;
                    int curDay = today.Day;
                    int curMonth = today.Month;

                    string sql = @"
                        SELECT 
                            CAST(k.ID AS VARCHAR(50)) as Id,
                            k.MAKHACH as MaKhach,
                            k.NAME as TenKhach,
                            k.DIENTHOAI as DienThoai,
                            k.NGAYSINH as NgaySinh,
                            k.DIACHI as DiaChi,
                            nk.NAME as TenNhomKhach
                        FROM DKHACHHANG k
                        LEFT JOIN DNHOMKHACHHANG nk ON CAST(k.DNHOMKHACHHANGID AS VARCHAR(50)) = CAST(nk.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS <> 0)
                          AND k.NGAYSINH IS NOT NULL
                          AND EXTRACT(MONTH FROM k.NGAYSINH) = @CurMonth
                          AND EXTRACT(DAY FROM k.NGAYSINH) = @CurDay
                        ORDER BY k.NAME";

                    var rows = await conn.QueryAsync(sql, new { CurMonth = curMonth, CurDay = curDay });
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new KhachHangSinhNhatItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            MaKhach = r.MAKHACH?.ToString() ?? "",
                            TenKhach = r.TENKHACH?.ToString() ?? "",
                            DienThoai = r.DIENTHOAI?.ToString() ?? "",
                            NgaySinh = r.NGAYSINH != null ? Convert.ToDateTime(r.NGAYSINH) : null,
                            DiaChi = r.DIACHI?.ToString() ?? "",
                            TenNhomKhach = r.TENNHOMKHACH?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetKhachHangSinhNhatListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<DotKhuyenMaiCanhBaoItem>> GetChuongTrinhKhuyenMaiListAsync()
        {
            var list = new List<DotKhuyenMaiCanhBaoItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    DateTime today = DateTime.Today;

                    string sql = @"
                        SELECT 
                            CAST(ID AS VARCHAR(50)) as Id,
                            NAME as TenKm,
                            TUNGAY as TuNgay,
                            DENNGAY as DenNgay,
                            COALESCE(TILEGIAMGIA, 0) as GiamGia,
                            NOTE as GhiChu
                        FROM DDOTKHUYENMAI
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND (NGUNGAPDUNG IS NULL OR NGUNGAPDUNG = '0')
                          AND (TUNGAY IS NULL OR CAST(TUNGAY AS DATE) <= @Today)
                          AND (DENNGAY IS NULL OR CAST(DENNGAY AS DATE) >= @Today)
                        ORDER BY NAME";

                    var rows = await conn.QueryAsync(sql, new { Today = today });
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new DotKhuyenMaiCanhBaoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            TenKm = r.TENKM?.ToString() ?? "",
                            TuNgay = r.TUNGAY != null ? Convert.ToDateTime(r.TUNGAY) : null,
                            DenNgay = r.DENNGAY != null ? Convert.ToDateTime(r.DENNGAY) : null,
                            GiamGia = r.GIAMGIA != null ? Convert.ToDecimal(r.GIAMGIA) : 0,
                            GhiChu = r.GHICHU?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetChuongTrinhKhuyenMaiListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<CanhBaoSummary> GetCanhBaoSummaryAsync()
        {
            var summary = new CanhBaoSummary();
            var configs = await LocalCauHinhService.LoadAllConfigsAsync();

            bool chkHangTon = configs.TryGetValue("ThongBaoHangDuoiMuocAnToan", out var v1) && (v1 == "1" || v1.Equals("true", StringComparison.OrdinalIgnoreCase));
            bool chkSinhNhat = configs.TryGetValue("ThongBaoKhachDenNgaySinhNhat", out var v2) && (v2 == "1" || v2.Equals("true", StringComparison.OrdinalIgnoreCase));
            bool chkKhuyenMai = configs.TryGetValue("CanhBaoChuongTrinhKhuyenMai", out var v3) && (v3 == "1" || v3.Equals("true", StringComparison.OrdinalIgnoreCase));

            if (chkHangTon)
            {
                summary.HangDuoiMucAnToan = await GetHangDuoiMucAnToanListAsync();
            }

            if (chkSinhNhat)
            {
                summary.KhachHangSinhNhat = await GetKhachHangSinhNhatListAsync();
            }

            if (chkKhuyenMai)
            {
                summary.ChuongTrinhKhuyenMai = await GetChuongTrinhKhuyenMaiListAsync();
            }

            return summary;
        }

        public static async Task CheckAndShowAlertsAsync(System.Windows.Window owner = null, bool forceShow = false)
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                bool hienThiCanhBao = configs.TryGetValue("HienThiCanhBao", out var vMaster) && (vMaster == "1" || vMaster.Equals("true", StringComparison.OrdinalIgnoreCase));

                if (!hienThiCanhBao && !forceShow)
                {
                    return;
                }

                var summary = await GetCanhBaoSummaryAsync();

                if (summary.HasAlerts || forceShow)
                {
                    var win = new QuanLyBar.Client.Views.CanhBao.CanhBaoHeThongWindow(summary);
                    if (owner != null) win.Owner = owner;
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CheckAndShowAlertsAsync error: " + ex.Message);
            }
        }
    }
}
