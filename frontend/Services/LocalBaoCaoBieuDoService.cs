using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class LocalBaoCaoBieuDoService
    {
        #region 1. BIỂU ĐỒ DOANH SỐ THEO NHÓM HÀNG HÓA

        public class DoanhSoNhomItem
        {
            public string TenNhom { get; set; } = "";
            public decimal DoanhSo { get; set; }
            public decimal TyLe { get; set; }
        }

        public async Task<List<DoanhSoNhomItem>> GetDoanhSoTheoNhomAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = @"
                    SELECT 
                        COALESCE(n.NAME, 'KHÁC') AS TenNhom,
                        SUM(COALESCE(c.THANHTIEN, COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0))) AS DoanhSo
                    FROM TDONHANGCHITIET c
                    JOIN TDONHANG d ON c.TDONHANGID = d.ID
                    JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY n.NAME
                    ORDER BY SUM(COALESCE(c.THANHTIEN, COALESCE(c.SLXUAT, c.SLNHAP, 0) * COALESCE(c.DONGIA, 0))) DESC";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                var list = new List<DoanhSoNhomItem>();
                decimal total = 0;

                foreach (var r in rows)
                {
                    decimal ds = Convert.ToDecimal(r.DOANHSO ?? 0);
                    if (ds <= 0) continue;
                    total += ds;
                    list.Add(new DoanhSoNhomItem
                    {
                        TenNhom = (string)r.TENNHOM ?? "KHÁC",
                        DoanhSo = ds
                    });
                }

                if (total > 0)
                {
                    foreach (var item in list)
                    {
                        item.TyLe = Math.Round((item.DoanhSo / total) * 100, 2);
                    }
                }

                return list;
            }
        }

        #endregion

        #region 2. BIỂU ĐỒ DOANH THU NGÀY TRONG THÁNG (THEO NĂM)

        public class NgayDoanhThuItem
        {
            public int Ngay { get; set; }
            public decimal DoanhThu { get; set; }
        }

        public class ThangDoanhThuSeries
        {
            public int Thang { get; set; }
            public string TenThang => $"Tháng {Thang}";
            public List<NgayDoanhThuItem> Data { get; set; } = new List<NgayDoanhThuItem>();
            public decimal TongDoanhThu => Data.Sum(x => x.DoanhThu);
        }

        public async Task<List<ThangDoanhThuSeries>> GetDoanhThuNgayTrongThangAsync(int nam)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = @"
                    SELECT 
                        EXTRACT(MONTH FROM d.NGAY) AS Thang,
                        EXTRACT(DAY FROM d.NGAY) AS Ngay,
                        SUM(COALESCE(d.TONGCONG, 0)) AS DoanhThu
                    FROM TDONHANG d
                    WHERE EXTRACT(YEAR FROM d.NGAY) = @Nam
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY EXTRACT(MONTH FROM d.NGAY), EXTRACT(DAY FROM d.NGAY)
                    ORDER BY EXTRACT(MONTH FROM d.NGAY) ASC, EXTRACT(DAY FROM d.NGAY) ASC";

                var rows = (await conn.QueryAsync(sql, new { Nam = nam })).ToList();

                var map = new Dictionary<(int Thang, int Ngay), decimal>();
                foreach (var r in rows)
                {
                    if (r.THANG != null && r.NGAY != null)
                    {
                        int t = Convert.ToInt32(r.THANG);
                        int n = Convert.ToInt32(r.NGAY);
                        decimal dt = Convert.ToDecimal(r.DOANHTHU ?? 0);
                        map[(t, n)] = dt;
                    }
                }

                var result = new List<ThangDoanhThuSeries>();
                for (int m = 1; m <= 12; m++)
                {
                    var series = new ThangDoanhThuSeries { Thang = m };
                    int daysInMonth = 31; // Chart shows 1..31
                    for (int d = 1; d <= daysInMonth; d++)
                    {
                        decimal val = map.TryGetValue((m, d), out var v) ? v : 0;
                        series.Data.Add(new NgayDoanhThuItem { Ngay = d, DoanhThu = val });
                    }
                    result.Add(series);
                }

                return result;
            }
        }

        #endregion

        #region 3. BIỂU ĐỒ DOANH THU THÁNG TRONG NĂM (NĂM NAY VS NĂM NGOÁI)

        public class ThangTrongNamItem
        {
            public int Thang { get; set; }
            public decimal DoanhThuNamNay { get; set; }
            public decimal DoanhThuNamNgoai { get; set; }
        }

        public async Task<List<ThangTrongNamItem>> GetDoanhThuThangTrongNamAsync(int nam)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = @"
                    SELECT 
                        EXTRACT(YEAR FROM d.NGAY) AS Nam,
                        EXTRACT(MONTH FROM d.NGAY) AS Thang,
                        SUM(COALESCE(d.TONGCONG, 0)) AS DoanhThu
                    FROM TDONHANG d
                    WHERE (EXTRACT(YEAR FROM d.NGAY) = @Nam OR EXTRACT(YEAR FROM d.NGAY) = @NamNgoai)
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY EXTRACT(YEAR FROM d.NGAY), EXTRACT(MONTH FROM d.NGAY)
                    ORDER BY EXTRACT(YEAR FROM d.NGAY) ASC, EXTRACT(MONTH FROM d.NGAY) ASC";

                var rows = (await conn.QueryAsync(sql, new { Nam = nam, NamNgoai = nam - 1 })).ToList();

                var mapNamNay = new Dictionary<int, decimal>();
                var mapNamNgoai = new Dictionary<int, decimal>();

                foreach (var r in rows)
                {
                    if (r.NAM != null && r.THANG != null)
                    {
                        int y = Convert.ToInt32(r.NAM);
                        int m = Convert.ToInt32(r.THANG);
                        decimal dt = Convert.ToDecimal(r.DOANHTHU ?? 0);
                        if (y == nam) mapNamNay[m] = dt;
                        else if (y == nam - 1) mapNamNgoai[m] = dt;
                    }
                }

                var list = new List<ThangTrongNamItem>();
                for (int m = 1; m <= 12; m++)
                {
                    list.Add(new ThangTrongNamItem
                    {
                        Thang = m,
                        DoanhThuNamNay = mapNamNay.TryGetValue(m, out var v1) ? v1 : 0,
                        DoanhThuNamNgoai = mapNamNgoai.TryGetValue(m, out var v2) ? v2 : 0
                    });
                }

                return list;
            }
        }

        #endregion

        #region 4. BIỂU ĐỒ THEO NHÂN VIÊN KINH DOANH

        public class DoanhSoNhanVienItem
        {
            public string TenNhanVien { get; set; } = "";
            public decimal DoanhSo { get; set; }
            public decimal TyLe { get; set; }
        }

        public async Task<List<DoanhSoNhanVienItem>> GetDoanhSoTheoNhanVienKdAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = @"
                    SELECT 
                        COALESCE(nv.NAME, u.NAME, 'Chưa rõ') AS TenNhanVien,
                        SUM(COALESCE(d.TONGCONG, 0)) AS DoanhSo
                    FROM TDONHANG d
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                    WHERE CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (d.STATUS <> 0 OR d.STATUS IS NULL)
                    GROUP BY nv.NAME, u.NAME
                    ORDER BY SUM(COALESCE(d.TONGCONG, 0)) DESC";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();

                var list = new List<DoanhSoNhanVienItem>();
                decimal total = 0;

                foreach (var r in rows)
                {
                    decimal ds = Convert.ToDecimal(r.DOANHSO ?? 0);
                    if (ds <= 0) continue;
                    total += ds;
                    list.Add(new DoanhSoNhanVienItem
                    {
                        TenNhanVien = (string)r.TENNHANVIEN ?? "Chưa rõ",
                        DoanhSo = ds
                    });
                }

                if (total > 0)
                {
                    foreach (var item in list)
                    {
                        item.TyLe = Math.Round((item.DoanhSo / total) * 100, 2);
                    }
                }

                return list;
            }
        }

        #endregion
    }
}

