using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public static class LocalKhachHangThanThietService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static decimal ParseDecimal(object val)
        {
            if (val == null) return 0;
            string s = val.ToString().Trim().Replace(",", ".");
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal res))
                return res;
            if (decimal.TryParse(val.ToString().Trim(), out decimal res2))
                return res2;
            return 0;
        }

        private static int ParseInt(object val)
        {
            if (val == null) return 0;
            if (int.TryParse(val.ToString().Trim(), out int res)) return res;
            return 0;
        }

        public static async Task EnsureTableCreatedAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    bool isSqlServer = conn.GetType().Name.Contains("SqlConnection", StringComparison.OrdinalIgnoreCase);

                    if (isSqlServer)
                    {
                        string checkSql = @"
                            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TTANGGIAMDIEM')
                            BEGIN
                                CREATE TABLE TTANGGIAMDIEM (
                                    [ID] INT PRIMARY KEY IDENTITY(1,1),
                                    [NAME] NVARCHAR(MAX),
                                    [NOTE] NVARCHAR(MAX),
                                    [STATUS] BIT DEFAULT 1,
                                    [USERMODIFIEDID] INT,
                                    [TIMEMODIFIED] DATETIME,
                                    [TIMECREATED] DATETIME DEFAULT GETDATE(),
                                    [NGAY] DATETIME,
                                    [USERCREATEDID] INT,
                                    [DKHACHHANGID] INT,
                                    [DIEMTANG] DECIMAL(18,2),
                                    [DIEMGIAM] DECIMAL(18,2),
                                    [LYDO] NVARCHAR(MAX)
                                );
                            END";
                        await conn.ExecuteAsync(checkSql);
                    }
                    else
                    {
                        // Firebird table existence check
                        string checkFb = "SELECT COUNT(*) FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'TTANGGIAMDIEM'";
                        int count = await conn.ExecuteScalarAsync<int>(checkFb);
                        if (count == 0)
                        {
                            string createFb = @"
                                CREATE TABLE TTANGGIAMDIEM (
                                    ID INTEGER NOT NULL PRIMARY KEY,
                                    NAME VARCHAR(255),
                                    NOTE VARCHAR(500),
                                    STATUS SMALLINT DEFAULT 1,
                                    USERMODIFIEDID INTEGER,
                                    TIMEMODIFIED TIMESTAMP,
                                    TIMECREATED TIMESTAMP,
                                    NGAY TIMESTAMP,
                                    USERCREATEDID INTEGER,
                                    DKHACHHANGID INTEGER,
                                    DIEMTANG NUMERIC(18,2),
                                    DIEMGIAM NUMERIC(18,2),
                                    LYDO VARCHAR(500)
                                )";
                            await conn.ExecuteAsync(createFb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnsureTableCreatedAsync error: " + ex.Message);
            }
        }

        public static async Task<List<KhachHangThanThietViewModel>> GetKhachHangThanThietListAsync(string nhomId = "ALL", string keyword = "")
        {
            var result = new List<KhachHangThanThietViewModel>();

            try
            {
                await EnsureTableCreatedAsync();

                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Query danh sách khách hàng
                    string sqlKhach = @"
                        SELECT 
                            K.ID, K.MAKHACH, K.NAME, K.DIACHI, K.DIENTHOAI, K.EMAIL, 
                            K.DNHOMKHACHHANGID, N.NAME AS TENNHOMKHACHHANG,
                            K.STATUS, K.DIEMTICHLUYBANDAU, K.NOTE
                        FROM DKHACHHANG K
                        LEFT JOIN DNHOMKHACHHANG N ON K.DNHOMKHACHHANGID = N.ID
                        WHERE (K.STATUS IS NULL OR K.STATUS > 0)";

                    var p = new DynamicParameters();

                    if (!string.IsNullOrEmpty(nhomId) && nhomId != "ALL")
                    {
                        if (nhomId == "UNASSIGNED")
                        {
                            sqlKhach += " AND (K.DNHOMKHACHHANGID IS NULL OR TRIM(K.DNHOMKHACHHANGID) = '')";
                        }
                        else
                        {
                            sqlKhach += " AND K.DNHOMKHACHHANGID = @NhomId";
                            p.Add("@NhomId", nhomId);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string kw = $"%{keyword.Trim().ToLower()}%";
                        sqlKhach += " AND (LOWER(K.MAKHACH) LIKE @kw OR LOWER(K.NAME) LIKE @kw OR LOWER(K.DIENTHOAI) LIKE @kw OR LOWER(K.EMAIL) LIKE @kw OR LOWER(K.DIACHI) LIKE @kw)";
                        p.Add("@kw", kw);
                    }

                    sqlKhach += " ORDER BY K.NAME";

                    var rowsKhach = (await conn.QueryAsync(sqlKhach, p)).ToList();

                    // 2. Query thống kê từ TDONHANG (Doanh số, số HĐ, điểm cộng/trừ từ hóa đơn: 20.000đ = 1 điểm)
                    var ordersMap = new Dictionary<string, (decimal DoanhSo, int SoHd, decimal DiemCong, decimal DiemTru)>();
                    try
                    {
                        string sqlOrders = @"
                            SELECT 
                                CAST(DKHACHHANGID AS VARCHAR(50)) AS KHACHID,
                                TONGCONG,
                                DIEM,
                                DIEMGIAM
                            FROM TDONHANG
                            WHERE (STATUS IS NULL OR STATUS > 0) AND DKHACHHANGID IS NOT NULL";

                        var orderRows = (await conn.QueryAsync(sqlOrders)).ToList();
                        foreach (var r in orderRows)
                        {
                            string kId = r.KHACHID?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(kId))
                            {
                                decimal tc = ParseDecimal(r.TONGCONG);
                                decimal d = ParseDecimal(r.DIEM);
                                // Quy tắc: 20.000đ sẽ được 1 điểm
                                if (d <= 0 && tc >= 20000)
                                {
                                    d = Math.Floor(tc / 20000m);
                                }
                                decimal dtru = ParseDecimal(r.DIEMGIAM);

                                if (!ordersMap.ContainsKey(kId))
                                {
                                    ordersMap[kId] = (tc, 1, d, dtru);
                                }
                                else
                                {
                                    var prev = ordersMap[kId];
                                    ordersMap[kId] = (prev.DoanhSo + tc, prev.SoHd + 1, prev.DiemCong + d, prev.DiemTru + dtru);
                                }
                            }
                        }
                    }
                    catch (Exception exOrders)
                    {
                        Console.WriteLine("Error querying TDONHANG summary: " + exOrders.Message);
                    }

                    // 3. Query thống kê từ TTANGGIAMDIEM (Điểm tặng, điểm giảm quà)
                    var pointAdjMap = new Dictionary<string, (decimal DiemTang, decimal DiemGiam)>();
                    try
                    {
                        string sqlPointAdj = @"
                            SELECT 
                                CAST(DKHACHHANGID AS VARCHAR(50)) AS KHACHID,
                                SUM(COALESCE(DIEMTANG, 0)) AS DIEMTANG,
                                SUM(COALESCE(DIEMGIAM, 0)) AS DIEMGIAM
                            FROM TTANGGIAMDIEM
                            WHERE (STATUS IS NULL OR STATUS > 0) AND DKHACHHANGID IS NOT NULL
                            GROUP BY DKHACHHANGID";

                        var adjRows = (await conn.QueryAsync(sqlPointAdj)).ToList();
                        foreach (var r in adjRows)
                        {
                            string kId = r.KHACHID?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(kId))
                            {
                                pointAdjMap[kId] = (
                                    ParseDecimal(r.DIEMTANG),
                                    ParseDecimal(r.DIEMGIAM)
                                );
                            }
                        }
                    }
                    catch (Exception exAdj)
                    {
                        Console.WriteLine("Error querying TTANGGIAMDIEM summary: " + exAdj.Message);
                    }

                    // 4. Ghép dữ liệu tạo danh sách ViewModel
                    int stt = 1;
                    foreach (var k in rowsKhach)
                    {
                        string kId = k.ID?.ToString()?.Trim();
                        decimal banDau = ParseDecimal(k.DIEMTICHLUYBANDAU);

                        decimal doanhSo = 0;
                        int soHd = 0;
                        decimal diemTuHd = 0;
                        decimal diemTruHd = 0;

                        if (ordersMap.TryGetValue(kId, out var ord))
                        {
                            doanhSo = ord.DoanhSo;
                            soHd = ord.SoHd;
                            diemTuHd = ord.DiemCong;
                            diemTruHd = ord.DiemTru;
                        }

                        decimal diemTang = 0;
                        decimal diemGiam = 0;
                        if (pointAdjMap.TryGetValue(kId, out var adj))
                        {
                            diemTang = adj.DiemTang;
                            diemGiam = adj.DiemGiam;
                        }

                        decimal diemTichLuy = banDau + (diemTuHd - diemTruHd) + (diemTang - diemGiam);

                        result.Add(new KhachHangThanThietViewModel
                        {
                            Stt = stt++,
                            Id = kId,
                            Makhach = k.MAKHACH?.ToString() ?? "",
                            Name = k.NAME?.ToString() ?? "",
                            Diachi = k.DIACHI?.ToString() ?? "",
                            Dienthoai = k.DIENTHOAI?.ToString() ?? "",
                            Email = k.EMAIL?.ToString() ?? "",
                            DoanhSo = doanhSo,
                            SoHoaDon = soHd,
                            DiemTichLuy = diemTichLuy,
                            DiemTichLuyBanDau = banDau,
                            Note = k.NOTE?.ToString() ?? "",
                            DnhomkhachhangId = k.DNHOMKHACHHANGID?.ToString(),
                            TenNhomKhachHang = k.TENNHOMKHACHHANG?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetKhachHangThanThietListAsync error: " + ex.Message);
            }

            return result;
        }

        public static async Task<List<TangGiamDiemItem>> GetLichSuTangGiamDiemAsync(string khachHangId)
        {
            var list = new List<TangGiamDiemItem>();
            if (string.IsNullOrEmpty(khachHangId)) return list;

            try
            {
                await EnsureTableCreatedAsync();

                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT ID, NAME, NOTE, STATUS, NGAY, DKHACHHANGID, DIEMTANG, DIEMGIAM, LYDO
                        FROM TTANGGIAMDIEM
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND CAST(DKHACHHANGID AS VARCHAR(50)) = @KhachHangId
                        ORDER BY NGAY DESC, ID DESC";

                    var rows = (await conn.QueryAsync(sql, new { KhachHangId = khachHangId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dt = null;
                        if (r.NGAY != null)
                        {
                            try { dt = Convert.ToDateTime(r.NGAY); } catch { }
                        }

                        list.Add(new TangGiamDiemItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            Ngay = dt,
                            SoPhieu = r.NAME?.ToString() ?? "",
                            GhiChu = r.NOTE?.ToString() ?? "",
                            DiemTang = r.DIEMTANG != null ? (decimal?)ParseDecimal(r.DIEMTANG) : null,
                            DiemGiam = r.DIEMGIAM != null ? (decimal?)ParseDecimal(r.DIEMGIAM) : null,
                            LyDo = r.LYDO?.ToString() ?? "",
                            DkhachhangId = r.DKHACHHANGID?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuTangGiamDiemAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TangGiamDiemItem>> GetAllTangGiamDiemAsync(string khachHangId = null)
        {
            var list = new List<TangGiamDiemItem>();
            try
            {
                await EnsureTableCreatedAsync();

                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT ID, NAME, NOTE, STATUS, NGAY, DKHACHHANGID, DIEMTANG, DIEMGIAM, LYDO
                        FROM TTANGGIAMDIEM
                        WHERE (STATUS IS NULL OR STATUS > 0)";

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                    }

                    sql += " ORDER BY NGAY ASC, ID ASC";

                    var rows = (await conn.QueryAsync(sql, new { KhachHangId = khachHangId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dt = null;
                        if (r.NGAY != null)
                        {
                            try { dt = Convert.ToDateTime(r.NGAY); } catch { }
                        }

                        list.Add(new TangGiamDiemItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            Ngay = dt,
                            SoPhieu = r.NAME?.ToString() ?? "",
                            GhiChu = r.NOTE?.ToString() ?? "",
                            DiemTang = r.DIEMTANG != null ? (decimal?)ParseDecimal(r.DIEMTANG) : null,
                            DiemGiam = r.DIEMGIAM != null ? (decimal?)ParseDecimal(r.DIEMGIAM) : null,
                            LyDo = r.LYDO?.ToString() ?? "",
                            DkhachhangId = r.DKHACHHANGID?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllTangGiamDiemAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<DiemTheoHoaDonItem>> GetLichSuDiemTheoHoaDonAsync(string khachHangId)
        {
            var list = new List<DiemTheoHoaDonItem>();
            if (string.IsNullOrEmpty(khachHangId)) return list;

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            h.ID, 
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as NAME, 
                            h.NGAY, 
                            b.NAME as BAN, 
                            CAST(COALESCE(h.TONGCONG, '0') AS DECIMAL(18,2)) as TONGCONG, 
                            COALESCE(h.DIEM, 0) as DIEM, 
                            COALESCE(h.DIEMGIAM, 0) as DIEMGIAM, 
                            h.NOTE
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON h.DBANID = b.ID
                        WHERE (h.STATUS IS NULL OR h.STATUS > 0)
                          AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId
                        ORDER BY h.NGAY DESC, h.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new { KhachHangId = khachHangId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dt = null;
                        if (r.NGAY != null)
                        {
                            try { dt = Convert.ToDateTime(r.NGAY); } catch { }
                        }

                        decimal tc = ParseDecimal(r.TONGCONG);
                        decimal d = ParseDecimal(r.DIEM);
                        // Quy tắc: 20 000đ sẽ được 1 điểm
                        if (d <= 0 && tc >= 20000)
                        {
                            d = Math.Floor(tc / 20000m);
                        }

                        list.Add(new DiemTheoHoaDonItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString(),
                            Ngay = dt,
                            SoPhieu = r.NAME?.ToString() ?? "",
                            Ban = r.BAN?.ToString() ?? "",
                            TongCong = tc,
                            Diem = d,
                            DiemSuDung = ParseDecimal(r.DIEMGIAM),
                            GhiChu = r.NOTE?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuDiemTheoHoaDonAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<string> GenerateSoPhieuTangDiemAsync()
        {
            try
            {
                await EnsureTableCreatedAsync();

                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string prefix = $"TG{DateTime.Now:yy}/";
                    string sql = $"SELECT NAME FROM TTANGGIAMDIEM WHERE NAME LIKE '{prefix}%' ORDER BY NAME DESC";
                    var rows = (await conn.QueryAsync<string>(sql)).ToList();

                    int maxNum = 0;
                    foreach (var name in rows)
                    {
                        if (name.StartsWith(prefix) && name.Length > prefix.Length)
                        {
                            string sub = name.Substring(prefix.Length);
                            if (int.TryParse(sub, out int n) && n > maxNum)
                            {
                                maxNum = n;
                            }
                        }
                    }

                    return $"{prefix}{(maxNum + 1):D5}";
                }
            }
            catch
            {
                return $"TG{DateTime.Now:yy}/00001";
            }
        }

        public static async Task<(bool Success, string Message)> SaveTangGiamDiemAsync(TangGiamDiemItem item, bool isNew)
        {
            try
            {
                await EnsureTableCreatedAsync();

                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    bool isSqlServer = conn.GetType().Name.Contains("SqlConnection", StringComparison.OrdinalIgnoreCase);
                    DateTime now = DateTime.Now;
                    DateTime ngay = item.Ngay ?? now;

                    int userId = 1;
                    if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUid) && parsedUid > 0)
                    {
                        userId = parsedUid;
                    }

                    string recId = !string.IsNullOrEmpty(item.Id) ? item.Id : Guid.NewGuid().ToString();

                    if (isNew)
                    {
                        if (isSqlServer)
                        {
                            string sql = @"
                                INSERT INTO TTANGGIAMDIEM (
                                    [ID], [NAME], [NOTE], [STATUS], [TIMECREATED], [TIMEMODIFIED], 
                                    [NGAY], [DKHACHHANGID], [DIEMTANG], [DIEMGIAM], [LYDO],
                                    [USERCREATEDID], [USERMODIFIEDID]
                                ) VALUES (
                                    @Id, @Name, @Note, 1, @Now, @Now,
                                    @Ngay, @DkhachhangId, @Diemtang, @Diemgiam, @Lydo,
                                    @UserId, @UserId
                                )";

                            await conn.ExecuteAsync(sql, new
                            {
                                Id = recId,
                                Name = item.SoPhieu,
                                Note = item.GhiChu,
                                Now = now,
                                Ngay = ngay,
                                DkhachhangId = item.DkhachhangId,
                                Diemtang = item.DiemTang,
                                Diemgiam = item.DiemGiam,
                                Lydo = item.LyDo,
                                UserId = userId
                            });
                        }
                        else
                        {
                            string sql = @"
                                INSERT INTO TTANGGIAMDIEM (
                                    ID, NAME, NOTE, STATUS, TIMECREATED, TIMEMODIFIED, 
                                    NGAY, DKHACHHANGID, DIEMTANG, DIEMGIAM, LYDO,
                                    USERCREATEDID, USERMODIFIEDID
                                ) VALUES (
                                    @Id, @Name, @Note, 1, @Now, @Now,
                                    @Ngay, @DkhachhangId, @Diemtang, @Diemgiam, @Lydo,
                                    @UserId, @UserId
                                )";

                            await conn.ExecuteAsync(sql, new
                            {
                                Id = recId,
                                Name = item.SoPhieu,
                                Note = item.GhiChu,
                                Now = now,
                                Ngay = ngay,
                                DkhachhangId = item.DkhachhangId,
                                Diemtang = item.DiemTang,
                                Diemgiam = item.DiemGiam,
                                Lydo = item.LyDo,
                                UserId = userId
                            });
                        }
                    }
                    else
                    {
                        string sql = @"
                            UPDATE TTANGGIAMDIEM SET 
                                NAME = @Name,
                                NOTE = @Note,
                                NGAY = @Ngay,
                                DKHACHHANGID = @DkhachhangId,
                                DIEMTANG = @Diemtang,
                                DIEMGIAM = @Diemgiam,
                                LYDO = @Lydo,
                                USERMODIFIEDID = @UserId,
                                TIMEMODIFIED = @Now
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(sql, new
                        {
                            Id = item.Id,
                            Name = item.SoPhieu,
                            Note = item.GhiChu,
                            Ngay = ngay,
                            DkhachhangId = item.DkhachhangId,
                            Diemtang = item.DiemTang,
                            Diemgiam = item.DiemGiam,
                            Lydo = item.LyDo,
                            UserId = userId,
                            Now = now
                        });
                    }

                    return (true, "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveTangGiamDiemAsync error: " + ex.Message);
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteTangGiamDiemAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "UPDATE TTANGGIAMDIEM SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteTangGiamDiemAsync error: " + ex.Message);
                return false;
            }
        }
    }
}
