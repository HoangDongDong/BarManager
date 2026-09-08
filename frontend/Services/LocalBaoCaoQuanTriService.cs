using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class LocalBaoCaoQuanTriService
    {
        #region 1 & 2. TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ NHẬP / GIÁ VỐN)

        public class LaiGopMatHangItem
        {
            public int Stt { get; set; }
            public string NhomHangId { get; set; }
            public string TenNhomHang { get; set; }
            public string MaHang { get; set; }
            public string TenHang { get; set; }
            public string Dvt { get; set; }
            public decimal SoLuong { get; set; }
            public decimal DoiTra { get; set; }
            public decimal DonGiaBan { get; set; }
            public decimal ThanhTienBan { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCongBan { get; set; }
            public decimal DonGiaVon { get; set; }
            public decimal GiaTriVon { get; set; }
            public decimal LoiNhuan { get; set; }
        }

        public class LaiGopGroupViewModel
        {
            public string GroupName { get; set; }
            public List<LaiGopMatHangItem> Items { get; set; } = new List<LaiGopMatHangItem>();
            public decimal TongSoLuong => Items.Sum(x => x.SoLuong);
            public decimal TongDoiTra => Items.Sum(x => x.DoiTra);
            public decimal TongThanhTien => Items.Sum(x => x.ThanhTienBan);
            public decimal TongThanhTienBan => Items.Sum(x => x.ThanhTienBan);
            public decimal TongGiamGia => Items.Sum(x => x.GiamGia);
            public decimal TongTongCong => Items.Sum(x => x.TongCongBan);
            public decimal TongCongBan => Items.Sum(x => x.TongCongBan);
            public decimal TongGiaTriVon => Items.Sum(x => x.GiaTriVon);
            public decimal TongLoiNhuan => Items.Sum(x => x.LoiNhuan);
        }

        public async Task<List<LaiGopGroupViewModel>> GetBaoCaoTongHopLaiGopMatHangAsync(DateTime tuNgay, DateTime denNgay, string nhomHang = null, bool useGiaNhap = false)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string costField = useGiaNhap ? "COALESCE(m.GIANHAP, 0)" : "COALESCE(m.GIAVON, m.GIANHAP, 0)";

                string sql = $@"
                    SELECT 
                        COALESCE(n.NAME, 'KHÁC') AS TenNhomHang,
                        m.CODE AS MaHang,
                        m.NAME AS TenHang,
                        COALESCE(dvt.NAME, '') AS Dvt,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) AS SoLuong,
                        SUM(CASE WHEN COALESCE(c.SLXUAT, c.SLNHAP, 0) < 0 THEN ABS(COALESCE(c.SLXUAT, c.SLNHAP, 0)) ELSE 0 END) AS DoiTra,
                        CASE WHEN SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) <> 0 
                             THEN SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0)) / SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) 
                             ELSE AVG(COALESCE(c.DONGIA, 0)) END AS DonGiaBan,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0)) AS ThanhTienBan,
                        SUM(COALESCE(c.THANHTIEN, 0) * COALESCE(c.TILEGIAMGIA, 0) / 100) AS GiamGia,
                        SUM(COALESCE(c.THANHTIEN, (COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0)))) AS TongCongBan,
                        AVG({costField}) AS DonGiaVon,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0) * {costField}) AS GiaTriVon
                    FROM TDONHANGCHITIET c
                    JOIN TDONHANG d ON c.TDONHANGID = d.ID
                    JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID
                    LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY n.NAME, m.CODE, m.NAME, dvt.NAME
                    ORDER BY n.NAME, m.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                var list = new List<LaiGopMatHangItem>();
                int stt = 1;
                foreach (var r in rows)
                {
                    string group = (string)r.TENNHOMHANG ?? "KHÁC";
                    if (!string.IsNullOrEmpty(nhomHang) && nhomHang != "[Tất cả]" && nhomHang != "Tất cả" && !group.Equals(nhomHang, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                    decimal doiTra = Convert.ToDecimal(r.DOITRA ?? 0);
                    decimal dgBan = Convert.ToDecimal(r.DONGIABAN ?? 0);
                    decimal ttBan = Convert.ToDecimal(r.THANHTIENBAN ?? 0);
                    decimal giamGia = Convert.ToDecimal(r.GIAMGIA ?? 0);
                    decimal tongCongBan = Convert.ToDecimal(r.TONGCONGBAN ?? 0);
                    decimal dgVon = Convert.ToDecimal(r.DONGIAVON ?? 0);
                    decimal gtrVon = Convert.ToDecimal(r.GIATRIVON ?? 0);
                    decimal loiNhuan = tongCongBan - gtrVon;

                    list.Add(new LaiGopMatHangItem
                    {
                        Stt = stt++,
                        TenNhomHang = group,
                        MaHang = (string)r.MAHANG ?? "",
                        TenHang = (string)r.TENHANG ?? "",
                        Dvt = (string)r.DVT ?? "",
                        SoLuong = sl,
                        DoiTra = doiTra,
                        DonGiaBan = dgBan,
                        ThanhTienBan = ttBan,
                        GiamGia = giamGia,
                        TongCongBan = tongCongBan,
                        DonGiaVon = dgVon,
                        GiaTriVon = gtrVon,
                        LoiNhuan = loiNhuan
                    });
                }

                var grouped = list.GroupBy(x => x.TenNhomHang).Select(g =>
                {
                    int groupStt = 1;
                    var items = g.ToList();
                    foreach (var it in items) it.Stt = groupStt++;
                    return new LaiGopGroupViewModel
                    {
                        GroupName = g.Key,
                        Items = items
                    };
                }).ToList();

                return grouped;
            }
        }

        #endregion

        #region 3 & 4. CHI TIẾT LÃI THEO HÓA ĐƠN (GIÁ NHẬP / GIÁ VỐN)

        public class LaiGopHoaDonItem
        {
            public int Stt { get; set; }
            public string SoHd { get; set; }
            public string MaHang { get; set; }
            public string TenHang { get; set; }
            public string Dvt { get; set; }
            public decimal SoLuong { get; set; }
            public decimal DoiTra { get; set; }
            public decimal DonGiaBan { get; set; }
            public decimal ThanhTienBan { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCongBan { get; set; }
            public decimal DonGiaVon { get; set; }
            public decimal GiaTriVon { get; set; }
            public decimal LoiNhuan { get; set; }
        }

        public class LaiGopHoaDonGroupViewModel
        {
            public string SoHd { get; set; }
            public List<LaiGopHoaDonItem> Items { get; set; } = new List<LaiGopHoaDonItem>();
            public decimal TongSoLuong => Items.Sum(x => x.SoLuong);
            public decimal TongDoiTra => Items.Sum(x => x.DoiTra);
            public decimal TongThanhTien => Items.Sum(x => x.ThanhTienBan);
            public decimal TongGiamGia => Items.Sum(x => x.GiamGia);
            public decimal TongTongCong => Items.Sum(x => x.TongCongBan);
            public decimal TongGiaTriVon => Items.Sum(x => x.GiaTriVon);
            public decimal TongLoiNhuan => Items.Sum(x => x.LoiNhuan);
        }

        public async Task<List<LaiGopHoaDonGroupViewModel>> GetBaoCaoChiTietLaiTheoHoaDonAsync(DateTime tuNgay, DateTime denNgay, bool useGiaNhap = false)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string costField = useGiaNhap ? "COALESCE(m.GIANHAP, 0)" : "COALESCE(m.GIAVON, m.GIANHAP, 0)";

                string sql = $@"
                    SELECT 
                        COALESCE(d.NAME, CAST(d.SOHD AS VARCHAR(50))) AS SoHd,
                        m.CODE AS MaHang,
                        m.NAME AS TenHang,
                        COALESCE(dvt.NAME, '') AS Dvt,
                        COALESCE(c.SLXUAT, c.SLNHAP, 0) AS SoLuong,
                        CASE WHEN COALESCE(c.SLXUAT, c.SLNHAP, 0) < 0 THEN ABS(COALESCE(c.SLXUAT, c.SLNHAP, 0)) ELSE 0 END AS DoiTra,
                        COALESCE(c.DONGIA, 0) AS DonGiaBan,
                        (COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0)) AS ThanhTienBan,
                        (COALESCE(c.THANHTIEN, 0) * COALESCE(c.TILEGIAMGIA, 0) / 100) AS GiamGia,
                        COALESCE(c.THANHTIEN, (COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0))) AS TongCongBan,
                        {costField} AS DonGiaVon,
                        (COALESCE(c.SLXUAT, c.SLNHAP, 0) * {costField}) AS GiaTriVon,
                        d.NGAY,
                        d.TIMECREATED
                    FROM TDONHANGCHITIET c
                    JOIN TDONHANG d ON c.TDONHANGID = d.ID
                    JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    ORDER BY d.NGAY ASC, d.TIMECREATED ASC, d.ID, c.ID";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                var list = new List<LaiGopHoaDonItem>();
                foreach (var r in rows)
                {
                    decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                    decimal doiTra = Convert.ToDecimal(r.DOITRA ?? 0);
                    decimal dgBan = Convert.ToDecimal(r.DONGIABAN ?? 0);
                    decimal ttBan = Convert.ToDecimal(r.THANHTIENBAN ?? 0);
                    decimal giamGia = Convert.ToDecimal(r.GIAMGIA ?? 0);
                    decimal tongCongBan = Convert.ToDecimal(r.TONGCONGBAN ?? 0);
                    decimal dgVon = Convert.ToDecimal(r.DONGIAVON ?? 0);
                    decimal gtrVon = Convert.ToDecimal(r.GIATRIVON ?? 0);
                    decimal loiNhuan = tongCongBan - gtrVon;

                    list.Add(new LaiGopHoaDonItem
                    {
                        SoHd = (string)r.SOHD ?? "Chưa rõ",
                        MaHang = (string)r.MAHANG ?? "",
                        TenHang = (string)r.TENHANG ?? "",
                        Dvt = (string)r.DVT ?? "",
                        SoLuong = sl,
                        DoiTra = doiTra,
                        DonGiaBan = dgBan,
                        ThanhTienBan = ttBan,
                        GiamGia = giamGia,
                        TongCongBan = tongCongBan,
                        DonGiaVon = dgVon,
                        GiaTriVon = gtrVon,
                        LoiNhuan = loiNhuan
                    });
                }

                var grouped = list.GroupBy(x => x.SoHd).Select(g =>
                {
                    int invStt = 1;
                    var items = g.ToList();
                    foreach (var it in items) it.Stt = invStt++;
                    return new LaiGopHoaDonGroupViewModel
                    {
                        SoHd = g.Key,
                        Items = items
                    };
                }).ToList();

                return grouped;
            }
        }

        #endregion

        #region 7. BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT

        public class MatHangBanChayItem
        {
            public int Stt { get; set; }
            public string MaHang { get; set; }
            public string MatHang { get; set; }
            public string Dvt { get; set; }
            public decimal SoLuong { get; set; }
            public decimal GiaBan { get; set; }
        }

        public async Task<List<MatHangBanChayItem>> GetBaoCao20MatHangBanChayAsync(DateTime tuNgay, DateTime denNgay, string nhomHang = null)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = @"
                    SELECT FIRST 20
                        m.CODE AS MaHang,
                        m.NAME AS MatHang,
                        COALESCE(dvt.NAME, '') AS Dvt,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) AS SoLuong,
                        SUM(COALESCE(c.THANHTIEN, COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0))) AS GiaBan,
                        n.NAME AS TenNhom
                    FROM TDONHANGCHITIET c
                    JOIN TDONHANG d ON c.TDONHANGID = d.ID
                    JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID
                    LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY m.CODE, m.NAME, dvt.NAME, n.NAME
                    ORDER BY SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) DESC";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                var list = new List<MatHangBanChayItem>();
                int stt = 1;
                foreach (var r in rows)
                {
                    string group = (string)r.TENNHOM ?? "KHÁC";
                    if (!string.IsNullOrEmpty(nhomHang) && nhomHang != "[Tất cả]" && nhomHang != "Tất cả" && !group.Equals(nhomHang, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    list.Add(new MatHangBanChayItem
                    {
                        Stt = stt++,
                        MaHang = (string)r.MAHANG ?? "",
                        MatHang = (string)r.MATHANG ?? "",
                        Dvt = (string)r.DVT ?? "",
                        SoLuong = Convert.ToDecimal(r.SOLUONG ?? 0),
                        GiaBan = Convert.ToDecimal(r.GIABAN ?? 0)
                    });
                }

                return list;
            }
        }

        #endregion

        #region 8. PHÂN TÍCH TÌNH HÌNH BÁN HÀNG

        public class PhanTichTopBanItem
        {
            public string TenHang { get; set; }
            public decimal GiaBan { get; set; }
            public decimal SoLuong { get; set; }
            public decimal ThanhTien { get; set; }
        }

        public class PhanTichAllBanItem
        {
            public string TenHang { get; set; }
            public decimal SoLuong { get; set; }
            public decimal ThanhTien { get; set; }
            public decimal TyLe { get; set; }
        }

        public class PhanTichMucGiaItem
        {
            public decimal DonGia { get; set; }
            public decimal SoLuong { get; set; }
            public decimal ThanhTien { get; set; }
            public decimal TyLe { get; set; }
        }

        public class PhanTichTinhHinhBanHangResult
        {
            public decimal TongTonKho { get; set; }
            public decimal SoLuongBanRa { get; set; }
            public int ThoiGianBan { get; set; }
            public decimal BinhQuanTrongNgay { get; set; }

            public List<PhanTichTopBanItem> TopBanItems { get; set; } = new List<PhanTichTopBanItem>();
            public List<PhanTichAllBanItem> AllBanItems { get; set; } = new List<PhanTichAllBanItem>();
            public List<PhanTichMucGiaItem> MucGiaItems { get; set; } = new List<PhanTichMucGiaItem>();

            public decimal TongTopSoLuong => TopBanItems.Sum(x => x.SoLuong);
            public decimal TongTopThanhTien => TopBanItems.Sum(x => x.ThanhTien);
            public decimal TongAllSoLuong => AllBanItems.Sum(x => x.SoLuong);
            public decimal TongAllThanhTien => AllBanItems.Sum(x => x.ThanhTien);
            public decimal TongMucGiaSoLuong => MucGiaItems.Sum(x => x.SoLuong);
            public decimal TongMucGiaThanhTien => MucGiaItems.Sum(x => x.ThanhTien);
        }

        public async Task<PhanTichTinhHinhBanHangResult> GetBaoCaoPhanTichTinhHinhBanHangAsync(DateTime tuNgay, DateTime denNgay)
        {
            var result = new PhanTichTinhHinhBanHangResult();
            int days = (int)(denNgay.Date - tuNgay.Date).TotalDays + 1;
            if (days < 1) days = 1;
            result.ThoiGianBan = days;

            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                // 1. Tổng tồn kho hiện tại
                decimal totalTonKho = 0;
                try
                {
                    totalTonKho = await conn.ExecuteScalarAsync<decimal?>("SELECT SUM(COALESCE(TONKHO, 0)) FROM DMATHANG") ?? 0;
                }
                catch { }
                result.TongTonKho = totalTonKho;

                // 2. Query items sold
                string sqlItems = @"
                    SELECT 
                        m.NAME AS TenHang,
                        AVG(COALESCE(c.DONGIA, 0)) AS DonGia,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) AS SoLuong,
                        SUM(COALESCE(c.THANHTIEN, COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0))) AS ThanhTien
                    FROM TDONHANGCHITIET c
                    JOIN TDONHANG d ON c.TDONHANGID = d.ID
                    JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY m.NAME
                    ORDER BY SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) DESC";

                var rows = (await conn.QueryAsync(sqlItems, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                decimal totalQtySold = rows.Sum(r => (decimal)Convert.ToDecimal(r.SOLUONG ?? 0));
                decimal totalRevenueSold = rows.Sum(r => (decimal)Convert.ToDecimal(r.THANHTIEN ?? 0));

                result.SoLuongBanRa = totalQtySold;
                result.BinhQuanTrongNgay = days > 0 ? Math.Round(totalQtySold / days, 1) : 0;

                // Left block: Top sold items
                foreach (var r in rows)
                {
                    decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                    if (sl <= 0) continue;

                    result.TopBanItems.Add(new PhanTichTopBanItem
                    {
                        TenHang = (string)r.TENHANG ?? "",
                        GiaBan = Convert.ToDecimal(r.DONGIA ?? 0),
                        SoLuong = sl,
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }

                // Middle block: All items with % of total quantity/revenue
                foreach (var r in rows)
                {
                    decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                    decimal tt = Convert.ToDecimal(r.THANHTIEN ?? 0);
                    decimal tyLe = totalQtySold > 0 ? Math.Round((sl / totalQtySold) * 100, 1) : 0;

                    result.AllBanItems.Add(new PhanTichAllBanItem
                    {
                        TenHang = (string)r.TENHANG ?? "",
                        SoLuong = sl,
                        ThanhTien = tt,
                        TyLe = tyLe
                    });
                }

                // Right block: Grouped by price points
                string sqlPrices = @"
                    SELECT 
                        COALESCE(c.DONGIA, 0) AS DonGia,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 0)) AS SoLuong,
                        SUM(COALESCE(c.THANHTIEN, COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0))) AS ThanhTien
                    FROM TDONHANGCHITIET c
                    JOIN TDONHANG d ON c.TDONHANGID = d.ID
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY COALESCE(c.DONGIA, 0)
                    ORDER BY COALESCE(c.DONGIA, 0) ASC";

                var priceRows = (await conn.QueryAsync(sqlPrices, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                foreach (var pr in priceRows)
                {
                    decimal dg = Convert.ToDecimal(pr.DONGIA ?? 0);
                    decimal sl = Convert.ToDecimal(pr.SOLUONG ?? 0);
                    decimal tt = Convert.ToDecimal(pr.THANHTIEN ?? 0);
                    decimal tyLe = totalRevenueSold > 0 ? Math.Round((tt / totalRevenueSold) * 100, 1) : 0;

                    result.MucGiaItems.Add(new PhanTichMucGiaItem
                    {
                        DonGia = dg,
                        SoLuong = sl,
                        ThanhTien = tt,
                        TyLe = tyLe
                    });
                }

                return result;
            }
        }

        #endregion

        #region 9. BÁO CÁO BÁN HÀNG THEO GIỜ

        public class BanHangTheoGioItem
        {
            public int Stt { get; set; }
            public string GioKhachVao { get; set; }
            public int SoHoaDon { get; set; }
            public decimal TongDoanhSo { get; set; }
        }

        public async Task<List<BanHangTheoGioItem>> GetBaoCaoBanHangTheoGioAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = @"
                    SELECT 
                        EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY)) AS Gio,
                        COUNT(d.ID) AS SoHoaDon,
                        SUM(COALESCE(d.TONGCONG, 0)) AS TongDoanhSo
                    FROM TDONHANG d
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY))
                    ORDER BY EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY)) ASC";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                var map = new Dictionary<int, (int Count, decimal Sum)>();
                foreach (var r in rows)
                {
                    if (r.GIO != null)
                    {
                        int hour = Convert.ToInt32(r.GIO);
                        int count = Convert.ToInt32(r.SOHOADON ?? 0);
                        decimal sum = Convert.ToDecimal(r.TONGDOANHSO ?? 0);
                        map[hour] = (count, sum);
                    }
                }

                var list = new List<BanHangTheoGioItem>();
                int stt = 1;

                // Typical active hours from 6h to 24h, or whatever has data
                for (int h = 0; h < 24; h++)
                {
                    if (map.ContainsKey(h))
                    {
                        list.Add(new BanHangTheoGioItem
                        {
                            Stt = stt++,
                            GioKhachVao = $"{h}h đến {h + 1}h",
                            SoHoaDon = map[h].Count,
                            TongDoanhSo = map[h].Sum
                        });
                    }
                }

                if (list.Count == 0)
                {
                    // Default template rows if no sales
                    string[] defaultSlots = { "8h đến 9h", "9h đến 10h", "10h đến 11h", "11h đến 12h", "12h đến 13h", "13h đến 14h", "15h đến 16h", "17h đến 18h" };
                    int i = 1;
                    foreach (var s in defaultSlots)
                    {
                        list.Add(new BanHangTheoGioItem
                        {
                            Stt = i++,
                            GioKhachVao = s,
                            SoHoaDon = 0,
                            TongDoanhSo = 0
                        });
                    }
                }

                return list;
            }
        }

        #endregion

        #region 10. DANH SÁCH MÓN XÓA, GIẢM, TRẢ LẠI

        public class MonXoaGiamTraLaiItem
        {
            public int Stt { get; set; }
            public DateTime Ngay { get; set; }
            public string Gio { get; set; }
            public string SoHd { get; set; }
            public string TaiKhoan { get; set; }
            public string ThietBi { get; set; }
            public string TenHang { get; set; }
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public string ThaoTac { get; set; }
        }

        public async Task<List<MonXoaGiamTraLaiItem>> GetBaoCaoMonXoaGiamTraLaiAsync(DateTime tuNgay, DateTime denNgay, string chucNang = null)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                var list = new List<MonXoaGiamTraLaiItem>();
                int stt = 1;

                // 1. Check TLUUVET for actions
                try
                {
                    string sqlLuuVet = @"
                        SELECT 
                            l.NGAY,
                            l.GIO,
                            COALESCE(h.NAME, l.SODONHANG) AS SoHd,
                            l.TAIKHOAN,
                            l.THIETBI,
                            l.NOTE,
                            l.CHUCNANG
                        FROM TLUUVET l
                        LEFT JOIN TDONHANG h ON CAST(l.SODONHANG AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        WHERE CAST(l.NGAY AS DATE) >= @TuNgay AND CAST(l.NGAY AS DATE) <= @DenNgay
                          AND (l.CHUCNANG CONTAINING 'Xóa' OR l.CHUCNANG CONTAINING 'Giảm' OR l.CHUCNANG CONTAINING 'Trả' OR l.CHUCNANG CONTAINING 'Hủy'
                               OR l.NOTE CONTAINING 'Xóa' OR l.NOTE CONTAINING 'Giảm' OR l.NOTE CONTAINING 'Trả' OR l.NOTE CONTAINING 'Hủy')
                        ORDER BY l.NGAY ASC, l.GIO ASC";

                    var lvRows = (await conn.QueryAsync(sqlLuuVet, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                    foreach (var r in lvRows)
                    {
                        string note = (string)r.NOTE ?? "";
                        string cn = (string)r.CHUCNANG ?? "Thao tác";
                        string thaoTac = !string.IsNullOrEmpty(cn) ? cn : note;

                        if (!string.IsNullOrEmpty(chucNang) && chucNang != "[Tất cả]" && chucNang != "Tất cả")
                        {
                            if (!thaoTac.Contains(chucNang, StringComparison.OrdinalIgnoreCase)) continue;
                        }

                        DateTime d = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : tuNgay;
                        string g = (string)r.GIO ?? "";

                        list.Add(new MonXoaGiamTraLaiItem
                        {
                            Stt = stt++,
                            Ngay = d,
                            Gio = g,
                            SoHd = (string)r.SOHD ?? "",
                            TaiKhoan = (string)r.TAIKHOAN ?? "admin",
                            ThietBi = (string)r.THIETBI ?? "Máy chính",
                            TenHang = note,
                            SoLuong = 1,
                            DonGia = 0,
                            ThanhTien = 0,
                            ThaoTac = thaoTac
                        });
                    }
                }
                catch { }

                // 2. Check TDONHANGCHITIET for items with discount or negative quantity (returned)
                try
                {
                    string sqlChiTiet = @"
                        SELECT 
                            d.NGAY,
                            d.TIMECREATED,
                            d.NAME AS SoHd,
                            COALESCE(u.NAME, 'Administrator') AS TaiKhoan,
                            m.NAME AS TenHang,
                            COALESCE(c.SLXUAT, c.SLNHAP, 0) AS SoLuong,
                            COALESCE(c.DONGIA, 0) AS DonGia,
                            COALESCE(c.TILEGIAMGIA, 0) AS GiamGia,
                            COALESCE(c.THANHTIEN, 0) AS ThanhTien
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON c.TDONHANGID = d.ID
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        JOIN DMATHANG m ON c.DMATHANGID = m.ID
                        WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                          AND (COALESCE(c.SLXUAT, c.SLNHAP, 0) < 0 OR COALESCE(c.TILEGIAMGIA, 0) > 0)
                        ORDER BY d.NGAY ASC, d.TIMECREATED ASC";

                    var ctRows = (await conn.QueryAsync(sqlChiTiet, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                    foreach (var r in ctRows)
                    {
                        decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                        decimal dg = Convert.ToDecimal(r.DONGIA ?? 0);
                        decimal gg = Convert.ToDecimal(r.GIAMGIA ?? 0);
                        decimal tt = Convert.ToDecimal(r.THANHTIEN ?? (sl * dg));

                        string action = sl < 0 ? "Trả lại món" : "Giảm giá món";

                        if (!string.IsNullOrEmpty(chucNang) && chucNang != "[Tất cả]" && chucNang != "Tất cả")
                        {
                            if (!action.Contains(chucNang, StringComparison.OrdinalIgnoreCase)) continue;
                        }

                        DateTime d = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : tuNgay;
                        string g = r.TIMECREATED != null ? Convert.ToDateTime(r.TIMECREATED).ToString("HH:mm") : "";

                        list.Add(new MonXoaGiamTraLaiItem
                        {
                            Stt = stt++,
                            Ngay = d,
                            Gio = g,
                            SoHd = (string)r.SOHD ?? "",
                            TaiKhoan = (string)r.TAIKHOAN ?? "admin",
                            ThietBi = "Máy chính",
                            TenHang = (string)r.TENHANG ?? "",
                            SoLuong = Math.Abs(sl),
                            DonGia = dg,
                            ThanhTien = Math.Abs(tt),
                            ThaoTac = action
                        });
                    }
                }
                catch { }

                return list;
            }
        }

        #endregion

        #region COMMON HELPERS

        public async Task<List<string>> GetNhomHangListAsync()
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                var list = (await conn.QueryAsync<string>("SELECT DISTINCT NAME FROM DNHOMMATHANG WHERE NAME IS NOT NULL ORDER BY NAME")).ToList();
                list.Insert(0, "[Tất cả]");
                return list;
            }
        }

        #endregion
    }
}
