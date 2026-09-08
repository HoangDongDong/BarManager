using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    #region Models
    public class CongNoFilterComboItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public class DoiChieuItemChiTiet
    {
        public int Stt { get; set; }
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal CkPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class DoiChieuPhieuMua
    {
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public List<DoiChieuItemChiTiet> ChiTiet { get; set; } = new List<DoiChieuItemChiTiet>();
    }

    public class DoiChieuPhieuThanhToan
    {
        public int Stt { get; set; }
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string DienGiai { get; set; } = "";
        public decimal SoTien { get; set; }
    }

    public class DoiChieuCongNoResult
    {
        public string DoiTuongId { get; set; } = "";
        public string MaDoiTuong { get; set; } = "";
        public string TenDoiTuong { get; set; } = "";
        public decimal NoDauKy { get; set; }
        public decimal PhatSinhMua { get; set; }
        public decimal ThanhToan { get; set; }
        public decimal NoCuoiKy { get; set; }
        public List<DoiChieuPhieuMua> DanhSachMua { get; set; } = new List<DoiChieuPhieuMua>();
        public List<DoiChieuPhieuThanhToan> DanhSachThanhToan { get; set; } = new List<DoiChieuPhieuThanhToan>();
    }

    public class BaoCaoCongNoRowItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string Ma { get; set; } = "";
        public string Ten { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string Email { get; set; } = "";
        public string TenNhom { get; set; } = "";
        public decimal NoDau { get; set; }
        public decimal Mua { get; set; }
        public decimal ThanhToan { get; set; }
        public decimal NoCuoi { get; set; }
        public decimal TongNo { get; set; }
    }
    #endregion

    public static class LocalBaoCaoCongNoService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        #region Dropdowns
        public static async Task<List<CongNoFilterComboItem>> GetNccDropdownAsync()
        {
            var list = new List<CongNoFilterComboItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, MANHACUNGCAP AS CODE, NAME FROM DNHACUNGCAP WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY MANHACUNGCAP, NAME";
                    var rows = await conn.QueryAsync(sql);
                    foreach (var r in rows)
                    {
                        list.Add(new CongNoFilterComboItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Code = r.CODE?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Icon = "🏢"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNccDropdownAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<CongNoFilterComboItem>> GetNhomNccDropdownAsync()
        {
            var list = new List<CongNoFilterComboItem>
            {
                new CongNoFilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "📁" }
            };
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DNHOMNHACUNGCAP WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY SORTORDER, NAME";
                    var rows = await conn.QueryAsync(sql);
                    foreach (var r in rows)
                    {
                        list.Add(new CongNoFilterComboItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Icon = "📁"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomNccDropdownAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<CongNoFilterComboItem>> GetKhachHangDropdownAsync()
        {
            var list = new List<CongNoFilterComboItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, MAKHACH AS CODE, NAME FROM DKHACHHANG WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY MAKHACH, NAME";
                    var rows = await conn.QueryAsync(sql);
                    foreach (var r in rows)
                    {
                        list.Add(new CongNoFilterComboItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Code = r.CODE?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Icon = "👤"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetKhachHangDropdownAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<CongNoFilterComboItem>> GetNhomKhachHangDropdownAsync()
        {
            var list = new List<CongNoFilterComboItem>
            {
                new CongNoFilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "📁" }
            };
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DNHOMKHACHHANG WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY SORTORDER, NAME";
                    var rows = await conn.QueryAsync(sql);
                    foreach (var r in rows)
                    {
                        list.Add(new CongNoFilterComboItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Icon = "📁"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomKhachHangDropdownAsync: " + ex.Message);
            }
            return list;
        }
        #endregion

        #region 1. Đối Chiếu Công Nợ Nhà Cung Cấp
        public static async Task<DoiChieuCongNoResult> GetDoiChieuCongNoNccAsync(DateTime tuNgay, DateTime denNgay, string nccId)
        {
            var result = new DoiChieuCongNoResult { DoiTuongId = nccId };
            if (string.IsNullOrEmpty(nccId)) return result;

            DateTime start = tuNgay.Date;
            DateTime end = denNgay.Date.AddDays(1).AddSeconds(-1);

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Lấy thông tin NCC
                    var ncc = await conn.QueryFirstOrDefaultAsync("SELECT ID, MANHACUNGCAP, NAME FROM DNHACUNGCAP WHERE CAST(ID AS VARCHAR(50)) = @NccId", new { NccId = nccId });
                    if (ncc != null)
                    {
                        result.MaDoiTuong = ncc.MANHACUNGCAP?.ToString() ?? "";
                        result.TenDoiTuong = ncc.NAME?.ToString() ?? "";
                    }

                    // 1. Nợ đầu kỳ (< tuNgay)
                    // Mua trước kỳ
                    string sqlMuaTruoc = @"
                        SELECT SUM(COALESCE(TONGCONG, 0)) AS MUA, SUM(COALESCE(THANHTOAN, 0)) AS TRA
                        FROM TDONHANG
                        WHERE CAST(DNHACUNGCAPID AS VARCHAR(50)) = @NccId
                          AND (STATUS IS NULL OR STATUS <> 0)
                          AND (LOAI = 1 OR LOAI IS NULL OR NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')
                          AND NGAY < @TuNgay";
                    var muaTruocRow = await conn.QueryFirstOrDefaultAsync(sqlMuaTruoc, new { NccId = nccId, TuNgay = start });
                    decimal muaTruoc = muaTruocRow != null ? (decimal)(muaTruocRow.MUA ?? 0) : 0;
                    decimal traTrenDonTruoc = muaTruocRow != null ? (decimal)(muaTruocRow.TRA ?? 0) : 0;

                    // Chi trả trước kỳ (TTHUCHI)
                    string sqlChiTruoc = @"
                        SELECT SUM(COALESCE(CHI, 0))
                        FROM TTHUCHI
                        WHERE CAST(DNHACUNGCAPID AS VARCHAR(50)) = @NccId
                          AND (STATUS IS NULL OR STATUS <> 0)
                          AND (KHONGTHAYDOICONGNO IS NULL OR KHONGTHAYDOICONGNO = 0)
                          AND (LOAI = 2 OR CAST(LOAI AS VARCHAR(20)) = '2' OR COALESCE(CHI, 0) > 0)
                          AND NGAY < @TuNgay";
                    decimal chiTruoc = await conn.ExecuteScalarAsync<decimal?>(sqlChiTruoc, new { NccId = nccId, TuNgay = start }) ?? 0;

                    result.NoDauKy = muaTruoc - (traTrenDonTruoc + chiTruoc);

                    // 2. Phát sinh mua trong kỳ (TDONHANG)
                    string sqlDonHang = @"
                        SELECT ID, SOPHIEU, NGAY, COALESCE(TONGCONG, 0) AS TONGCONG, COALESCE(THANHTOAN, 0) AS THANHTOAN
                        FROM TDONHANG
                        WHERE CAST(DNHACUNGCAPID AS VARCHAR(50)) = @NccId
                          AND (STATUS IS NULL OR STATUS <> 0)
                          AND (LOAI = 1 OR LOAI IS NULL OR NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        ORDER BY NGAY, SOPHIEU";
                    var donHangs = (await conn.QueryAsync(sqlDonHang, new { NccId = nccId, TuNgay = start, DenNgay = end })).ToList();

                    decimal tongPhatSinhMua = 0;
                    decimal tongTraTrenDon = 0;

                    foreach (var dh in donHangs)
                    {
                        string dhId = dh.ID?.ToString() ?? "";
                        var phieu = new DoiChieuPhieuMua
                        {
                            SoPhieu = dh.SOPHIEU?.ToString() ?? "",
                            Ngay = dh.NGAY != null ? (DateTime?)dh.NGAY : null,
                            TongTien = (decimal)(dh.TONGCONG ?? 0),
                            DaThanhToan = (decimal)(dh.THANHTOAN ?? 0)
                        };

                        tongPhatSinhMua += phieu.TongTien;
                        tongTraTrenDon += phieu.DaThanhToan;

                        // Chi tiết mặt hàng
                        string sqlChiTiet = @"
                            SELECT 
                                c.ID, c.SLNHAP, c.DONGIA, c.TILEGIAMGIA, c.THANHTIEN,
                                m.MAMATHANG, m.NAME AS TENHANG, d.NAME AS TENDVT
                            FROM TDONHANGCHITIET c
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH d ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                            WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @DhId
                            ORDER BY c.ID";
                        var ctRows = (await conn.QueryAsync(sqlChiTiet, new { DhId = dhId })).ToList();
                        int stt = 1;
                        foreach (var ct in ctRows)
                        {
                            phieu.ChiTiet.Add(new DoiChieuItemChiTiet
                            {
                                Stt = stt++,
                                MaHang = ct.MAMATHANG?.ToString() ?? "",
                                TenHang = ct.TENHANG?.ToString() ?? "",
                                Dvt = ct.TENDVT?.ToString() ?? "",
                                SoLuong = (decimal)(ct.SLNHAP ?? 0),
                                DonGia = (decimal)(ct.DONGIA ?? 0),
                                CkPhanTram = (decimal)(ct.TILEGIAMGIA ?? 0),
                                ThanhTien = (decimal)(ct.THANHTIEN ?? 0)
                            });
                        }

                        result.DanhSachMua.Add(phieu);
                    }

                    // 3. Thanh toán trong kỳ (TTHUCHI)
                    string sqlChiTrongKy = @"
                        SELECT SOPHIEU, NGAY, DIENGIAI, COALESCE(CHI, 0) AS SOTIEN
                        FROM TTHUCHI
                        WHERE CAST(DNHACUNGCAPID AS VARCHAR(50)) = @NccId
                          AND (STATUS IS NULL OR STATUS <> 0)
                          AND (KHONGTHAYDOICONGNO IS NULL OR KHONGTHAYDOICONGNO = 0)
                          AND (LOAI = 2 OR CAST(LOAI AS VARCHAR(20)) = '2' OR COALESCE(CHI, 0) > 0)
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        ORDER BY NGAY, SOPHIEU";
                    var chiTrongKyRows = (await conn.QueryAsync(sqlChiTrongKy, new { NccId = nccId, TuNgay = start, DenNgay = end })).ToList();
                    decimal tongChiTrongKy = 0;
                    int sttChi = 1;
                    foreach (var c in chiTrongKyRows)
                    {
                        decimal soTien = (decimal)(c.SOTIEN ?? 0);
                        tongChiTrongKy += soTien;
                        result.DanhSachThanhToan.Add(new DoiChieuPhieuThanhToan
                        {
                            Stt = sttChi++,
                            SoPhieu = c.SOPHIEU?.ToString() ?? "",
                            Ngay = c.NGAY != null ? (DateTime?)c.NGAY : null,
                            DienGiai = c.DIENGIAI?.ToString() ?? "",
                            SoTien = soTien
                        });
                    }

                    result.PhatSinhMua = tongPhatSinhMua;
                    result.ThanhToan = tongTraTrenDon + tongChiTrongKy;
                    result.NoCuoiKy = result.NoDauKy + result.PhatSinhMua - result.ThanhToan;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetDoiChieuCongNoNccAsync: " + ex.Message);
            }

            return result;
        }
        #endregion

        #region 2. Báo Cáo Công Nợ Nhà Cung Cấp & 5. Tổng Hợp Công Nợ NCC
        public static async Task<List<BaoCaoCongNoRowItem>> GetBaoCaoCongNoNccAsync(DateTime tuNgay, DateTime denNgay, string nhomNccId = "")
        {
            var list = new List<BaoCaoCongNoRowItem>();
            DateTime start = tuNgay.Date;
            DateTime end = denNgay.Date.AddDays(1).AddSeconds(-1);

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sqlNcc = @"
                        SELECT 
                            k.ID, k.MANHACUNGCAP AS MA, k.NAME AS TEN, k.DIENTHOAI, k.DIACHI, k.EMAIL,
                            COALESCE(n.NAME, 'Chưa phân nhóm') AS TENNHOM
                        FROM DNHACUNGCAP k
                        LEFT JOIN DNHOMNHACUNGCAP n ON CAST(k.DNHOMNHACUNGCAPID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS <> 0)";

                    if (!string.IsNullOrEmpty(nhomNccId))
                    {
                        sqlNcc += " AND CAST(k.DNHOMNHACUNGCAPID AS VARCHAR(50)) = @NhomId";
                    }
                    sqlNcc += " ORDER BY n.SORTORDER, n.NAME, k.MANHACUNGCAP, k.NAME";

                    var nccs = (await conn.QueryAsync(sqlNcc, new { NhomId = nhomNccId })).ToList();

                    // Đơn hàng trước kỳ
                    string sqlDhTruoc = @"
                        SELECT 
                            CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID,
                            SUM(COALESCE(TONGCONG, 0)) AS MUA,
                            SUM(COALESCE(THANHTOAN, 0)) AS TRA
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND DNHACUNGCAPID IS NOT NULL
                          AND (LOAI = 1 OR LOAI IS NULL OR NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')
                          AND NGAY < @TuNgay
                        GROUP BY DNHACUNGCAPID";
                    var dhTruocDict = (await conn.QueryAsync(sqlDhTruoc, new { TuNgay = start }))
                        .ToDictionary(x => (string)x.NCCID?.ToString()?.Trim(), x => new { Mua = (decimal)(x.MUA ?? 0), Tra = (decimal)(x.TRA ?? 0) });

                    // Thu chi trước kỳ
                    string sqlChiTruoc = @"
                        SELECT 
                            CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID,
                            SUM(COALESCE(CHI, 0)) AS CHI
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND DNHACUNGCAPID IS NOT NULL
                          AND (KHONGTHAYDOICONGNO IS NULL OR KHONGTHAYDOICONGNO = 0)
                          AND (LOAI = 2 OR CAST(LOAI AS VARCHAR(20)) = '2' OR COALESCE(CHI, 0) > 0)
                          AND NGAY < @TuNgay
                        GROUP BY DNHACUNGCAPID";
                    var chiTruocDict = (await conn.QueryAsync(sqlChiTruoc, new { TuNgay = start }))
                        .ToDictionary(x => (string)x.NCCID?.ToString()?.Trim(), x => (decimal)(x.CHI ?? 0));

                    // Đơn hàng trong kỳ
                    string sqlDhTrongKy = @"
                        SELECT 
                            CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID,
                            SUM(COALESCE(TONGCONG, 0)) AS MUA,
                            SUM(COALESCE(THANHTOAN, 0)) AS TRA
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND DNHACUNGCAPID IS NOT NULL
                          AND (LOAI = 1 OR LOAI IS NULL OR NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        GROUP BY DNHACUNGCAPID";
                    var dhTrongKyDict = (await conn.QueryAsync(sqlDhTrongKy, new { TuNgay = start, DenNgay = end }))
                        .ToDictionary(x => (string)x.NCCID?.ToString()?.Trim(), x => new { Mua = (decimal)(x.MUA ?? 0), Tra = (decimal)(x.TRA ?? 0) });

                    // Thu chi trong kỳ
                    string sqlChiTrongKy = @"
                        SELECT 
                            CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID,
                            SUM(COALESCE(CHI, 0)) AS CHI
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND DNHACUNGCAPID IS NOT NULL
                          AND (KHONGTHAYDOICONGNO IS NULL OR KHONGTHAYDOICONGNO = 0)
                          AND (LOAI = 2 OR CAST(LOAI AS VARCHAR(20)) = '2' OR COALESCE(CHI, 0) > 0)
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        GROUP BY DNHACUNGCAPID";
                    var chiTrongKyDict = (await conn.QueryAsync(sqlChiTrongKy, new { TuNgay = start, DenNgay = end }))
                        .ToDictionary(x => (string)x.NCCID?.ToString()?.Trim(), x => (decimal)(x.CHI ?? 0));

                    int stt = 1;
                    foreach (var ncc in nccs)
                    {
                        string id = (ncc.ID?.ToString() ?? "").Trim();
                        decimal noDau = 0;
                        if (dhTruocDict.TryGetValue(id, out var dhT)) noDau += dhT.Mua - dhT.Tra;
                        if (chiTruocDict.TryGetValue(id, out var cT)) noDau -= cT;

                        decimal mua = 0;
                        decimal thanhToan = 0;
                        if (dhTrongKyDict.TryGetValue(id, out var dhK))
                        {
                            mua = dhK.Mua;
                            thanhToan += dhK.Tra;
                        }
                        if (chiTrongKyDict.TryGetValue(id, out var cK))
                        {
                            thanhToan += cK;
                        }

                        decimal noCuoi = noDau + mua - thanhToan;

                        list.Add(new BaoCaoCongNoRowItem
                        {
                            Stt = stt++,
                            Id = id,
                            Ma = ncc.MA?.ToString() ?? "",
                            Ten = ncc.TEN?.ToString() ?? "",
                            DienThoai = ncc.DIENTHOAI?.ToString() ?? "",
                            DiaChi = ncc.DIACHI?.ToString() ?? "",
                            Email = ncc.EMAIL?.ToString() ?? "",
                            TenNhom = ncc.TENNHOM?.ToString() ?? "Chưa phân nhóm",
                            NoDau = noDau,
                            Mua = mua,
                            ThanhToan = thanhToan,
                            NoCuoi = noCuoi,
                            TongNo = noCuoi
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBaoCaoCongNoNccAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<BaoCaoCongNoRowItem>> GetTongHopCongNoNccAsync(string nhomNccId = "")
        {
            // Tổng nợ hiện tại đến thời điểm hiện tại
            return await GetBaoCaoCongNoNccAsync(new DateTime(2000, 1, 1), DateTime.Today, nhomNccId);
        }
        #endregion

        #region 3. Đối Chiếu Công Nợ Khách Hàng
        public static async Task<DoiChieuCongNoResult> GetDoiChieuCongNoKhachHangAsync(DateTime tuNgay, DateTime denNgay, string khachId)
        {
            var result = new DoiChieuCongNoResult { DoiTuongId = khachId };
            if (string.IsNullOrEmpty(khachId)) return result;

            DateTime start = tuNgay.Date;
            DateTime end = denNgay.Date.AddDays(1).AddSeconds(-1);

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Lấy thông tin khách
                    var kh = await conn.QueryFirstOrDefaultAsync("SELECT ID, MAKHACH, NAME FROM DKHACHHANG WHERE CAST(ID AS VARCHAR(50)) = @KhId", new { KhId = khachId });
                    if (kh != null)
                    {
                        result.MaDoiTuong = kh.MAKHACH?.ToString() ?? "";
                        result.TenDoiTuong = kh.NAME?.ToString() ?? "";
                    }

                    // 1. Nợ đầu kỳ (< tuNgay)
                    string sqlMuaTruoc = @"
                        SELECT SUM(COALESCE(TONGCONG, 0)) AS MUA, SUM(COALESCE(TIENTHANHTOAN, 0)) AS TRA
                        FROM TDONHANG
                        WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId
                          AND (STATUS IS NULL OR STATUS > 0)
                          AND NGAY < @TuNgay";
                    var muaTruocRow = await conn.QueryFirstOrDefaultAsync(sqlMuaTruoc, new { KhId = khachId, TuNgay = start });
                    decimal muaTruoc = muaTruocRow != null ? (decimal)(muaTruocRow.MUA ?? 0) : 0;
                    decimal traTrenDonTruoc = muaTruocRow != null ? (decimal)(muaTruocRow.TRA ?? 0) : 0;

                    string sqlThuTruoc = @"
                        SELECT SUM(COALESCE(THU, 0))
                        FROM TTHUCHI
                        WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId
                          AND (STATUS IS NULL OR STATUS > 0)
                          AND (COALESCE(LAPHIEUTHUCONGNO, 0) > 0 OR LOAI = 1 OR CAST(LOAI AS VARCHAR(20)) = '1' OR COALESCE(THU, 0) > 0)
                          AND NGAY < @TuNgay";
                    decimal thuTruoc = await conn.ExecuteScalarAsync<decimal?>(sqlThuTruoc, new { KhId = khachId, TuNgay = start }) ?? 0;

                    result.NoDauKy = muaTruoc - (traTrenDonTruoc + thuTruoc);

                    // 2. Phát sinh mua trong kỳ (TDONHANG)
                    string sqlDonHang = @"
                        SELECT ID, SOPHIEU, NGAY, COALESCE(TONGCONG, 0) AS TONGCONG, COALESCE(TIENTHANHTOAN, 0) AS THANHTOAN
                        FROM TDONHANG
                        WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId
                          AND (STATUS IS NULL OR STATUS > 0)
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        ORDER BY NGAY, SOPHIEU";
                    var donHangs = (await conn.QueryAsync(sqlDonHang, new { KhId = khachId, TuNgay = start, DenNgay = end })).ToList();

                    decimal tongPhatSinhMua = 0;
                    decimal tongTraTrenDon = 0;

                    foreach (var dh in donHangs)
                    {
                        string dhId = dh.ID?.ToString() ?? "";
                        var phieu = new DoiChieuPhieuMua
                        {
                            SoPhieu = dh.SOPHIEU?.ToString() ?? "",
                            Ngay = dh.NGAY != null ? (DateTime?)dh.NGAY : null,
                            TongTien = (decimal)(dh.TONGCONG ?? 0),
                            DaThanhToan = (decimal)(dh.THANHTOAN ?? 0)
                        };

                        tongPhatSinhMua += phieu.TongTien;
                        tongTraTrenDon += phieu.DaThanhToan;

                        string sqlChiTiet = @"
                            SELECT 
                                c.ID, c.SLBAN, c.DONGIA, c.TILEGIAMGIA, c.THANHTIEN,
                                m.MAMATHANG, m.NAME AS TENHANG, d.NAME AS TENDVT
                            FROM TDONHANGCHITIET c
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH d ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                            WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @DhId
                            ORDER BY c.ID";
                        var ctRows = (await conn.QueryAsync(sqlChiTiet, new { DhId = dhId })).ToList();
                        int stt = 1;
                        foreach (var ct in ctRows)
                        {
                            phieu.ChiTiet.Add(new DoiChieuItemChiTiet
                            {
                                Stt = stt++,
                                MaHang = ct.MAMATHANG?.ToString() ?? "",
                                TenHang = ct.TENHANG?.ToString() ?? "",
                                Dvt = ct.TENDVT?.ToString() ?? "",
                                SoLuong = (decimal)(ct.SLBAN ?? 0),
                                DonGia = (decimal)(ct.DONGIA ?? 0),
                                CkPhanTram = (decimal)(ct.TILEGIAMGIA ?? 0),
                                ThanhTien = (decimal)(ct.THANHTIEN ?? 0)
                            });
                        }

                        result.DanhSachMua.Add(phieu);
                    }

                    // 3. Thanh toán trong kỳ (TTHUCHI)
                    string sqlThuTrongKy = @"
                        SELECT SOPHIEU, NGAY, DIENGIAI, COALESCE(THU, 0) AS SOTIEN
                        FROM TTHUCHI
                        WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId
                          AND (STATUS IS NULL OR STATUS > 0)
                          AND (COALESCE(LAPHIEUTHUCONGNO, 0) > 0 OR LOAI = 1 OR CAST(LOAI AS VARCHAR(20)) = '1' OR COALESCE(THU, 0) > 0)
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        ORDER BY NGAY, SOPHIEU";
                    var thuTrongKyRows = (await conn.QueryAsync(sqlThuTrongKy, new { KhId = khachId, TuNgay = start, DenNgay = end })).ToList();
                    decimal tongThuTrongKy = 0;
                    int sttThu = 1;
                    foreach (var t in thuTrongKyRows)
                    {
                        decimal soTien = (decimal)(t.SOTIEN ?? 0);
                        tongThuTrongKy += soTien;
                        result.DanhSachThanhToan.Add(new DoiChieuPhieuThanhToan
                        {
                            Stt = sttThu++,
                            SoPhieu = t.SOPHIEU?.ToString() ?? "",
                            Ngay = t.NGAY != null ? (DateTime?)t.NGAY : null,
                            DienGiai = t.DIENGIAI?.ToString() ?? "",
                            SoTien = soTien
                        });
                    }

                    result.PhatSinhMua = tongPhatSinhMua;
                    result.ThanhToan = tongTraTrenDon + tongThuTrongKy;
                    result.NoCuoiKy = result.NoDauKy + result.PhatSinhMua - result.ThanhToan;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetDoiChieuCongNoKhachHangAsync: " + ex.Message);
            }

            return result;
        }
        #endregion

        #region 4. Tổng Hợp & 6. Báo Cáo Công Nợ Khách Hàng
        public static async Task<List<BaoCaoCongNoRowItem>> GetBaoCaoCongNoKhachHangAsync(DateTime tuNgay, DateTime denNgay, string nhomKhachId = "")
        {
            var list = new List<BaoCaoCongNoRowItem>();
            DateTime start = tuNgay.Date;
            DateTime end = denNgay.Date.AddDays(1).AddSeconds(-1);

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sqlKh = @"
                        SELECT 
                            k.ID, k.MAKHACH AS MA, k.NAME AS TEN, k.DIENTHOAI, k.DIACHI, k.EMAIL,
                            COALESCE(n.NAME, 'Chưa phân nhóm') AS TENNHOM
                        FROM DKHACHHANG k
                        LEFT JOIN DNHOMKHACHHANG n ON CAST(k.DNHOMKHACHHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS > 0)";

                    if (!string.IsNullOrEmpty(nhomKhachId))
                    {
                        sqlKh += " AND CAST(k.DNHOMKHACHHANGID AS VARCHAR(50)) = @NhomId";
                    }
                    sqlKh += " ORDER BY n.SORTORDER, n.NAME, k.MAKHACH, k.NAME";

                    var khachs = (await conn.QueryAsync(sqlKh, new { NhomId = nhomKhachId })).ToList();

                    // Đơn hàng trước kỳ
                    string sqlDhTruoc = @"
                        SELECT 
                            CAST(DKHACHHANGID AS VARCHAR(50)) AS KHID,
                            SUM(COALESCE(TONGCONG, 0)) AS MUA,
                            SUM(COALESCE(TIENTHANHTOAN, 0)) AS TRA
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND DKHACHHANGID IS NOT NULL
                          AND NGAY < @TuNgay
                        GROUP BY DKHACHHANGID";
                    var dhTruocDict = (await conn.QueryAsync(sqlDhTruoc, new { TuNgay = start }))
                        .ToDictionary(x => (string)x.KHID?.ToString()?.Trim(), x => new { Mua = (decimal)(x.MUA ?? 0), Tra = (decimal)(x.TRA ?? 0) });

                    // Thu chi trước kỳ
                    string sqlThuTruoc = @"
                        SELECT 
                            CAST(DKHACHHANGID AS VARCHAR(50)) AS KHID,
                            SUM(COALESCE(THU, 0)) AS THU
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND DKHACHHANGID IS NOT NULL
                          AND (COALESCE(LAPHIEUTHUCONGNO, 0) > 0 OR LOAI = 1 OR CAST(LOAI AS VARCHAR(20)) = '1' OR COALESCE(THU, 0) > 0)
                          AND NGAY < @TuNgay
                        GROUP BY DKHACHHANGID";
                    var thuTruocDict = (await conn.QueryAsync(sqlThuTruoc, new { TuNgay = start }))
                        .ToDictionary(x => (string)x.KHID?.ToString()?.Trim(), x => (decimal)(x.THU ?? 0));

                    // Đơn hàng trong kỳ
                    string sqlDhTrongKy = @"
                        SELECT 
                            CAST(DKHACHHANGID AS VARCHAR(50)) AS KHID,
                            SUM(COALESCE(TONGCONG, 0)) AS MUA,
                            SUM(COALESCE(TIENTHANHTOAN, 0)) AS TRA
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND DKHACHHANGID IS NOT NULL
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        GROUP BY DKHACHHANGID";
                    var dhTrongKyDict = (await conn.QueryAsync(sqlDhTrongKy, new { TuNgay = start, DenNgay = end }))
                        .ToDictionary(x => (string)x.KHID?.ToString()?.Trim(), x => new { Mua = (decimal)(x.MUA ?? 0), Tra = (decimal)(x.TRA ?? 0) });

                    // Thu chi trong kỳ
                    string sqlThuTrongKy = @"
                        SELECT 
                            CAST(DKHACHHANGID AS VARCHAR(50)) AS KHID,
                            SUM(COALESCE(THU, 0)) AS THU
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND DKHACHHANGID IS NOT NULL
                          AND (COALESCE(LAPHIEUTHUCONGNO, 0) > 0 OR LOAI = 1 OR CAST(LOAI AS VARCHAR(20)) = '1' OR COALESCE(THU, 0) > 0)
                          AND NGAY >= @TuNgay AND NGAY <= @DenNgay
                        GROUP BY DKHACHHANGID";
                    var thuTrongKyDict = (await conn.QueryAsync(sqlThuTrongKy, new { TuNgay = start, DenNgay = end }))
                        .ToDictionary(x => (string)x.KHID?.ToString()?.Trim(), x => (decimal)(x.THU ?? 0));

                    int stt = 1;
                    foreach (var kh in khachs)
                    {
                        string id = (kh.ID?.ToString() ?? "").Trim();
                        decimal noDau = 0;
                        if (dhTruocDict.TryGetValue(id, out var dhT)) noDau += dhT.Mua - dhT.Tra;
                        if (thuTruocDict.TryGetValue(id, out var tT)) noDau -= tT;

                        decimal mua = 0;
                        decimal thanhToan = 0;
                        if (dhTrongKyDict.TryGetValue(id, out var dhK))
                        {
                            mua = dhK.Mua;
                            thanhToan += dhK.Tra;
                        }
                        if (thuTrongKyDict.TryGetValue(id, out var tK))
                        {
                            thanhToan += tK;
                        }

                        decimal noCuoi = noDau + mua - thanhToan;

                        list.Add(new BaoCaoCongNoRowItem
                        {
                            Stt = stt++,
                            Id = id,
                            Ma = kh.MA?.ToString() ?? "",
                            Ten = kh.TEN?.ToString() ?? "",
                            DienThoai = kh.DIENTHOAI?.ToString() ?? "",
                            DiaChi = kh.DIACHI?.ToString() ?? "",
                            Email = kh.EMAIL?.ToString() ?? "",
                            TenNhom = kh.TENNHOM?.ToString() ?? "Chưa phân nhóm",
                            NoDau = noDau,
                            Mua = mua,
                            ThanhToan = thanhToan,
                            NoCuoi = noCuoi,
                            TongNo = noCuoi
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBaoCaoCongNoKhachHangAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<BaoCaoCongNoRowItem>> GetTongHopCongNoKhachHangAsync(string nhomKhachId = "")
        {
            return await GetBaoCaoCongNoKhachHangAsync(new DateTime(2000, 1, 1), DateTime.Today, nhomKhachId);
        }
        #endregion
    }
}
