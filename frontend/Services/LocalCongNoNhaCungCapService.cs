using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class NhomNccDropdownItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class CongNoNhaCungCapViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string MaNhaCungCap { get; set; } = "";
        public string Name { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public string Email { get; set; } = "";
        public string Website { get; set; } = "";
        public string DnhomnhacungcapId { get; set; } = "";
        public string TenNhom { get; set; } = "";
        public decimal TongPhatSinh { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public string Note { get; set; } = "";

        public string TongPhatSinhFormatted => TongPhatSinh.ToString("N0");
        public string DaThanhToanFormatted => DaThanhToan.ToString("N0");
        public string ConNoFormatted => ConNo.ToString("N0");
    }

    public class ChiTietCongNoNccItemViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public DateTime? TimeCreated { get; set; }
        public string LoaiPhieu { get; set; } = "";
        public decimal TongCong { get; set; }
        public string DienGiai { get; set; } = "";
        public decimal TienThanhToan { get; set; }
        public decimal LuyKe { get; set; }

        public string NgayFormatted => Ngay?.ToString("dd/MM/yyyy") ?? "";
        public string TongCongFormatted => TongCong != 0 ? TongCong.ToString("N0") : "0";
        public string TienThanhToanFormatted => TienThanhToan != 0 ? TienThanhToan.ToString("N0") : "0";
        public string LuyKeFormatted => LuyKe.ToString("N0");
    }

    public static class LocalCongNoNhaCungCapService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        public static async Task<ObservableCollection<NhomKhachHangTreeItem>> GetNhomNhaCungCapTreeAsync(int sortBy = 0)
        {
            var result = new ObservableCollection<NhomKhachHangTreeItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string orderClause = sortBy == 1 ? "SORTORDER, NAME" : "NAME, SORTORDER";
                    string sql = $"SELECT ID, NAME, PARENTID FROM DNHOMNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY {orderClause}";
                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var rootItem = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        ItemType = 0,
                        Icon = "🌐",
                        IconColor = "#3498db",
                        IsExpanded = true,
                        IsSelected = true
                    };

                    var nodes = new Dictionary<string, NhomKhachHangTreeItem>();
                    foreach (var r in rows)
                    {
                        string id = r.ID?.ToString();
                        if (string.IsNullOrEmpty(id)) continue;

                        var node = new NhomKhachHangTreeItem
                        {
                            Id = id,
                            Name = r.NAME?.ToString() ?? "",
                            ParentId = r.PARENTID?.ToString()?.Trim(),
                            ItemType = 2,
                            Icon = "📁",
                            IconColor = "#f0ad4e",
                            IsExpanded = true
                        };
                        nodes[node.Id] = node;
                    }

                    foreach (var node in nodes.Values)
                    {
                        if (!string.IsNullOrEmpty(node.ParentId) && nodes.ContainsKey(node.ParentId))
                        {
                            nodes[node.ParentId].Children.Add(node);
                        }
                        else
                        {
                            rootItem.Children.Add(node);
                        }
                    }

                    result.Add(rootItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomNhaCungCapTreeAsync: " + ex.Message);
            }

            return result;
        }

        public static async Task<List<NhomNccDropdownItem>> GetNhomNhaCungCapDropdownAsync()
        {
            var list = new List<NhomNccDropdownItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DNHOMNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();
                    list.Add(new NhomNccDropdownItem { Id = "ALL", Name = "Tất cả" });
                    foreach (var r in rows)
                    {
                        list.Add(new NhomNccDropdownItem { Id = r.ID?.ToString() ?? "", Name = r.NAME?.ToString() ?? "" });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomNhaCungCapDropdownAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<CongNoNhaCungCapViewModel>> GetCongNoNhaCungCapListAsync(
            string filterNhomId = "ALL",
            string keyword = "",
            int debtFilterMode = 0, // 0: Tất cả, 1: Chỉ còn nợ != 0, 2: Có phát sinh trong kỳ
            DateTime? tuNgay = null,
            DateTime? denNgay = null)
        {
            var list = new List<CongNoNhaCungCapViewModel>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Lấy danh sách nhà cung cấp
                    string sqlNcc = @"
                        SELECT 
                            k.ID, k.MANHACUNGCAP, k.NAME, k.DIACHI, k.DIENTHOAI, k.EMAIL, 
                            k.WEBSITE, k.NOTE, k.DNHOMNHACUNGCAPID, n.NAME AS TENNHOM
                        FROM DNHACUNGCAP k
                        LEFT JOIN DNHOMNHACUNGCAP n ON CAST(k.DNHOMNHACUNGCAPID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS <> 0)";

                    if (!string.IsNullOrEmpty(filterNhomId) && filterNhomId != "ALL" && filterNhomId != "UNSET" && filterNhomId != "UNASSIGNED")
                    {
                        sqlNcc += " AND CAST(k.DNHOMNHACUNGCAPID AS VARCHAR(50)) = @NhomId";
                    }
                    else if (filterNhomId == "UNSET" || filterNhomId == "UNASSIGNED")
                    {
                        sqlNcc += " AND (k.DNHOMNHACUNGCAPID IS NULL OR TRIM(CAST(k.DNHOMNHACUNGCAPID AS VARCHAR(50))) = '')";
                    }

                    sqlNcc += " ORDER BY k.MANHACUNGCAP, k.NAME";

                    var nccs = (await conn.QueryAsync(sqlNcc, new { NhomId = filterNhomId })).ToList();
                    if (nccs.Count == 0) return list;

                    // 2. Tính tổng tiền nhập hàng và thanh toán trên đơn nhập (TDONHANG LOAI=1)
                    string sqlDonHang = @"
                        SELECT 
                            CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID,
                            SUM(COALESCE(TONGCONG, 0)) AS TONGTIENHANG,
                            SUM(COALESCE(THANHTOAN, 0)) AS DATHANHTOAN_DONHANG,
                            SUM(COALESCE(CONLAI, 0)) AS CONNO_DONHANG
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND DNHACUNGCAPID IS NOT NULL
                          AND (LOAI = 1 OR LOAI IS NULL OR NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')";

                    if (tuNgay.HasValue)
                    {
                        sqlDonHang += " AND NGAY >= @TuNgay";
                    }
                    if (denNgay.HasValue)
                    {
                        sqlDonHang += " AND NGAY <= @DenNgay";
                    }

                    sqlDonHang += " GROUP BY DNHACUNGCAPID";

                    var donHangSummaries = (await conn.QueryAsync(sqlDonHang, new { TuNgay = tuNgay, DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) }))
                        .ToDictionary(x => (string)x.NCCID?.ToString(), x => new {
                            TongTienHang = (decimal)(x.TONGTIENHANG ?? 0),
                            DaThanhToan = (decimal)(x.DATHANHTOAN_DONHANG ?? 0),
                            ConNo = (decimal)(x.CONNO_DONHANG ?? 0)
                        });

                    // 3. Tính tổng tiền chi trả từ phiếu chi độc lập (TTHUCHI)
                    string sqlThuChi = @"
                        SELECT 
                            CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID,
                            SUM(COALESCE(CHI, 0)) AS TONGCHI
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND DNHACUNGCAPID IS NOT NULL
                          AND (KHONGTHAYDOICONGNO IS NULL OR KHONGTHAYDOICONGNO = 0)
                          AND (LOAI = 2 OR CAST(LOAI AS VARCHAR(20)) = '2' OR COALESCE(CHI, 0) > 0)";

                    if (tuNgay.HasValue)
                    {
                        sqlThuChi += " AND NGAY >= @TuNgay";
                    }
                    if (denNgay.HasValue)
                    {
                        sqlThuChi += " AND NGAY <= @DenNgay";
                    }

                    sqlThuChi += " GROUP BY DNHACUNGCAPID";

                    var thuChiSummaries = (await conn.QueryAsync(sqlThuChi, new { TuNgay = tuNgay, DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) }))
                        .ToDictionary(x => (string)x.NCCID?.ToString()?.Trim(), x => (decimal)(x.TONGCHI ?? 0));

                    int stt = 1;
                    foreach (var c in nccs)
                    {
                        string nccId = (c.ID?.ToString() ?? "").Trim();
                        decimal tongPhatSinh = 0;
                        decimal daThanhToan = 0;

                        if (donHangSummaries.TryGetValue(nccId, out var dhSum))
                        {
                            tongPhatSinh = dhSum.TongTienHang;
                            daThanhToan += dhSum.DaThanhToan;
                        }

                        if (thuChiSummaries.TryGetValue(nccId, out var tcChi))
                        {
                            daThanhToan += tcChi;
                        }

                        decimal conNo = tongPhatSinh - daThanhToan;

                        // Áp dụng bộ lọc hiển thị
                        if (debtFilterMode == 1 && conNo == 0) continue; // Chỉ còn nợ
                        if (debtFilterMode == 2 && tongPhatSinh == 0 && daThanhToan == 0) continue; // Có phát sinh

                        // Bộ lọc từ khóa
                        if (!string.IsNullOrWhiteSpace(keyword))
                        {
                            string kw = keyword.ToLower().Trim();
                            string ten = (c.NAME?.ToString() ?? "").ToLower();
                            string ma = (c.MANHACUNGCAP?.ToString() ?? "").ToLower();
                            string dt = (c.DIENTHOAI?.ToString() ?? "").ToLower();
                            string dc = (c.DIACHI?.ToString() ?? "").ToLower();
                            string email = (c.EMAIL?.ToString() ?? "").ToLower();

                            if (!ten.Contains(kw) && !ma.Contains(kw) && !dt.Contains(kw) && !dc.Contains(kw) && !email.Contains(kw))
                            {
                                continue;
                            }
                        }

                        list.Add(new CongNoNhaCungCapViewModel
                        {
                            Stt = stt++,
                            Id = nccId,
                            MaNhaCungCap = c.MANHACUNGCAP?.ToString() ?? "",
                            Name = c.NAME?.ToString() ?? "",
                            DiaChi = c.DIACHI?.ToString() ?? "",
                            DienThoai = c.DIENTHOAI?.ToString() ?? "",
                            Email = c.EMAIL?.ToString() ?? "",
                            Website = c.WEBSITE?.ToString() ?? "",
                            DnhomnhacungcapId = c.DNHOMNHACUNGCAPID?.ToString() ?? "",
                            TenNhom = c.TENNHOM?.ToString() ?? "",
                            TongPhatSinh = tongPhatSinh,
                            DaThanhToan = daThanhToan,
                            ConNo = conNo,
                            Note = c.NOTE?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCongNoNhaCungCapListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<ChiTietCongNoNccItemViewModel>> GetChiTietCongNoNhaCungCapAsync(
            string nccId, 
            DateTime? tuNgay = null, 
            DateTime? denNgay = null)
        {
            var list = new List<ChiTietCongNoNccItemViewModel>();
            if (string.IsNullOrEmpty(nccId)) return list;

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Lấy tất cả phiếu nhập hàng / công nợ ban đầu (TDONHANG)
                    string sqlDonHang = @"
                        SELECT 
                            ID,
                            NAME AS SOPHIEU,
                            NGAY,
                            TIMECREATED,
                            COALESCE(TONGCONG, 0) AS TONGCONG,
                            COALESCE(THANHTOAN, 0) AS THANHTOAN,
                            COALESCE(TIENHANG, 0) AS TIENHANG,
                            NOTE AS DIENGIAI,
                            LOAI
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND (TRIM(CAST(DNHACUNGCAPID AS VARCHAR(50))) = @NccId OR UPPER(TRIM(CAST(DNHACUNGCAPID AS VARCHAR(50)))) = UPPER(@NccId))
                          AND (LOAI = 1 OR LOAI IS NULL OR NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')";

                    if (tuNgay.HasValue) sqlDonHang += " AND NGAY >= @TuNgay";
                    if (denNgay.HasValue) sqlDonHang += " AND NGAY <= @DenNgay";

                    var donHangs = (await conn.QueryAsync(sqlDonHang, new { 
                        NccId = nccId.Trim(), 
                        TuNgay = tuNgay, 
                        DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) 
                    })).ToList();

                    // 2. Lấy tất cả phiếu chi (TTHUCHI)
                    string sqlThuChi = @"
                        SELECT 
                            ID,
                            NAME AS SOPHIEU,
                            NGAY,
                            TIMECREATED,
                            COALESCE(CHI, 0) AS CHI,
                            DIENGIAI
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND (TRIM(CAST(DNHACUNGCAPID AS VARCHAR(50))) = @NccId OR UPPER(TRIM(CAST(DNHACUNGCAPID AS VARCHAR(50)))) = UPPER(@NccId))
                          AND (KHONGTHAYDOICONGNO IS NULL OR KHONGTHAYDOICONGNO = 0)
                          AND (LOAI = 2 OR CAST(LOAI AS VARCHAR(20)) = '2' OR COALESCE(CHI, 0) > 0)";

                    if (tuNgay.HasValue) sqlThuChi += " AND NGAY >= @TuNgay";
                    if (denNgay.HasValue) sqlThuChi += " AND NGAY <= @DenNgay";

                    var thuChis = (await conn.QueryAsync(sqlThuChi, new { 
                        NccId = nccId.Trim(), 
                        TuNgay = tuNgay, 
                        DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) 
                    })).ToList();

                    // 3. Gộp và sắp xếp theo ngày/thời gian tăng dần
                    var rawTransactions = new List<ChiTietCongNoNccItemViewModel>();

                    foreach (var dh in donHangs)
                    {
                        DateTime? dt = null;
                        if (dh.NGAY != null)
                        {
                            if (dh.NGAY is DateTime dVal) dt = dVal;
                            else if (DateTime.TryParse(dh.NGAY.ToString(), out DateTime pVal)) dt = pVal;
                        }

                        DateTime? timeCreated = null;
                        if (dh.TIMECREATED != null)
                        {
                            if (dh.TIMECREATED is DateTime tcVal) timeCreated = tcVal;
                            else if (DateTime.TryParse(dh.TIMECREATED.ToString(), out DateTime tcParsed)) timeCreated = tcParsed;
                        }

                        decimal tong = 0;
                        if (dh.TONGCONG != null) decimal.TryParse(dh.TONGCONG.ToString(), out tong);
                        if (tong == 0 && dh.TIENHANG != null) decimal.TryParse(dh.TIENHANG.ToString(), out tong);

                        decimal tt = 0;
                        if (dh.THANHTOAN != null) decimal.TryParse(dh.THANHTOAN.ToString(), out tt);

                        string note = dh.DIENGIAI?.ToString() ?? "";
                        string soPhieu = dh.SOPHIEU?.ToString() ?? "";
                        string loaiPhieu = (note == "Công nợ ban đầu" || soPhieu.StartsWith("CNBD_")) ? "Công nợ ban đầu" : "PN";
                        string dienGiai = !string.IsNullOrWhiteSpace(note) ? note : "Nhập mua hàng";

                        rawTransactions.Add(new ChiTietCongNoNccItemViewModel
                        {
                            Id = dh.ID?.ToString() ?? "",
                            SoPhieu = soPhieu,
                            Ngay = dt,
                            TimeCreated = timeCreated,
                            LoaiPhieu = loaiPhieu,
                            TongCong = tong,
                            TienThanhToan = tt,
                            DienGiai = dienGiai
                        });
                    }

                    foreach (var tc in thuChis)
                    {
                        DateTime? dt = null;
                        if (tc.NGAY != null)
                        {
                            if (tc.NGAY is DateTime dVal) dt = dVal;
                            else if (DateTime.TryParse(tc.NGAY.ToString(), out DateTime pVal)) dt = pVal;
                        }

                        DateTime? timeCreated = null;
                        if (tc.TIMECREATED != null)
                        {
                            if (tc.TIMECREATED is DateTime tcVal) timeCreated = tcVal;
                            else if (DateTime.TryParse(tc.TIMECREATED.ToString(), out DateTime tcParsed)) timeCreated = tcParsed;
                        }

                        decimal tienChi = 0;
                        if (tc.CHI != null) decimal.TryParse(tc.CHI.ToString(), out tienChi);

                        string note = tc.DIENGIAI?.ToString() ?? "";
                        string soPhieu = tc.SOPHIEU?.ToString() ?? "";

                        rawTransactions.Add(new ChiTietCongNoNccItemViewModel
                        {
                            Id = tc.ID?.ToString() ?? "",
                            SoPhieu = soPhieu,
                            Ngay = dt,
                            TimeCreated = timeCreated,
                            LoaiPhieu = "PC",
                            TongCong = 0,
                            TienThanhToan = tienChi,
                            DienGiai = !string.IsNullOrWhiteSpace(note) ? note : "Thanh toán tiền hàng..."
                        });
                    }

                    var sorted = rawTransactions
                        .OrderBy(x => (x.Ngay ?? DateTime.MinValue).Date)
                        .ThenBy(x => x.TimeCreated ?? x.Ngay ?? DateTime.MinValue)
                        .ThenBy(x => x.SoPhieu)
                        .ToList();

                    decimal runningBalance = 0;
                    int stt = 1;

                    foreach (var tx in sorted)
                    {
                        if (tx.LoaiPhieu != "PC")
                        {
                            runningBalance += (tx.TongCong - tx.TienThanhToan);
                        }
                        else
                        {
                            runningBalance -= tx.TienThanhToan;
                        }

                        tx.Stt = stt++;
                        tx.LuyKe = runningBalance;
                        list.Add(tx);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetChiTietCongNoNhaCungCapAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<string> GetNextSoPhieuChiAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string yearSuffix = DateTime.Now.ToString("yy");
                    string prefix = $"PC{yearSuffix}/";

                    string sql = $"SELECT NAME FROM TTHUCHI WHERE NAME STARTING WITH @Prefix ORDER BY NAME DESC";
                    var names = (await conn.QueryAsync<string>(sql, new { Prefix = prefix })).ToList();

                    int maxIndex = 0;
                    foreach (var name in names)
                    {
                        if (name != null && name.StartsWith(prefix))
                        {
                            string numPart = name.Substring(prefix.Length).Trim();
                            if (int.TryParse(numPart, out int idx))
                            {
                                if (idx > maxIndex) maxIndex = idx;
                            }
                        }
                    }

                    return $"{prefix}{(maxIndex + 1):D5}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNextSoPhieuChiAsync: " + ex.Message);
                return $"PC{DateTime.Now:yy}/00001";
            }
        }

        public static async Task<List<dynamic>> GetLyDoChiLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME, LALYDOTHU, LOAILYDO, NOTE FROM DLYDOTHUCHI WHERE (STATUS IS NULL OR STATUS <> 0) AND (LALYDOTHU IS NULL OR LALYDOTHU = 0) ORDER BY NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetCuaHangLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DCUAHANG WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY SORTORDER, NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetNhanVienLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DNHANVIEN WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY SORTORDER, NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetNhaCungCapLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, MANHACUNGCAP, NAME, DIACHI, DIENTHOAI FROM DNHACUNGCAP WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY MANHACUNGCAP, NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetKhachHangLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME, DIACHI, DIENTHOAI FROM DKHACHHANG WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY MAKHACH, NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetTaiKhoanNganHangLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DTAIKHOANNGANHANG WHERE STATUS IS NULL OR STATUS <> 0 ORDER BY SORTORDER, NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<bool> SavePhieuChiFullAsync(
            string soPhieu,
            DateTime ngay,
            decimal soTien,
            string nccId,
            string tenDoiTuong,
            string diaChi,
            string lyDoId,
            string nhanVienId,
            string khachHangId,
            string taiKhoanNganHangId,
            string cuaHangId,
            bool isChuyenKhoan,
            bool khongThayDoiCongNo,
            string dienGiai,
            string chungTuGoc,
            string loaiDoiTuong = "Nhà cung cấp")
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string userId = SessionContext.CurrentUser?.Id;
                    if (string.IsNullOrEmpty(userId))
                    {
                        try
                        {
                            var uObj = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                            if (uObj != null) userId = uObj.ToString();
                        }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(userId)) userId = "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    string newId = Guid.NewGuid().ToString();
                    string sql = @"
                        INSERT INTO TTHUCHI (
                            ID, NAME, NGAY, TIMECREATED, TIMEMODIFIED, USERCREATEDID, USERMODIFIEDID,
                            TENDOITUONG, DIACHI, LOAI, LOAIDOITUONG, DIENGIAI, CHUNGTUGOC,
                            THU, CHI, CHUYENKHOAN, LAPHIEUTHUCONGNO, KHONGTHAYDOICONGNO,
                            DNHANVIENID, DKHACHHANGID, DNHACUNGCAPID, DLYDOTHUCHIID,
                            DTAIKHOANNGANHANGID, DCUAHANGID, NOTE, STATUS
                        ) VALUES (
                            @Id, @SoPhieu, @Ngay, @Now, @Now, @UserId, @UserId,
                            @TenDoiTuong, @DiaChi, '2', @LoaiDoiTuong, @DienGiai, @ChungTuGoc,
                            0, @Chi, @ChuyenKhoan, @LaPhieuThuCongNo, @KhongThayDoiCongNo,
                            @NhanVienId, @KhachHangId, @NccId, @LyDoId,
                            @TaiKhoanNganHangId, @CuaHangId, @Note, 30
                        )";

                    int loaiDoiTuongVal = 1;
                    if (!string.IsNullOrEmpty(loaiDoiTuong))
                    {
                        string s = loaiDoiTuong.Trim().ToLowerInvariant();
                        if (s.Contains("khách") || s == "0") loaiDoiTuongVal = 0;
                        else if (s.Contains("cung cấp") || s.Contains("ncc") || s == "1") loaiDoiTuongVal = 1;
                        else if (s.Contains("nhân viên") || s.Contains("nv") || s == "2") loaiDoiTuongVal = 2;
                        else if (int.TryParse(s, out int parsed)) loaiDoiTuongVal = parsed;
                        else loaiDoiTuongVal = 3;
                    }

                    int affected = await conn.ExecuteAsync(sql, new
                    {
                        Id = newId,
                        SoPhieu = soPhieu,
                        Ngay = ngay,
                        Now = DateTime.Now,
                        UserId = userId,
                        TenDoiTuong = tenDoiTuong,
                        DiaChi = diaChi,
                        LoaiDoiTuong = loaiDoiTuongVal,
                        DienGiai = dienGiai,
                        ChungTuGoc = chungTuGoc,
                        Chi = soTien,
                        ChuyenKhoan = isChuyenKhoan ? 1 : 0,
                        LaPhieuThuCongNo = khongThayDoiCongNo ? 0 : 1,
                        KhongThayDoiCongNo = khongThayDoiCongNo ? 1 : 0,
                        NhanVienId = string.IsNullOrEmpty(nhanVienId) ? (object)DBNull.Value : nhanVienId,
                        KhachHangId = string.IsNullOrEmpty(khachHangId) ? (object)DBNull.Value : khachHangId,
                        NccId = string.IsNullOrEmpty(nccId) ? (object)DBNull.Value : nccId,
                        LyDoId = string.IsNullOrEmpty(lyDoId) ? (object)DBNull.Value : lyDoId,
                        TaiKhoanNganHangId = (isChuyenKhoan && !string.IsNullOrEmpty(taiKhoanNganHangId)) ? taiKhoanNganHangId : (object)DBNull.Value,
                        CuaHangId = string.IsNullOrEmpty(cuaHangId) ? (object)DBNull.Value : cuaHangId,
                        Note = dienGiai
                    });

                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SavePhieuChiFullAsync: " + ex.Message);
                throw;
            }
        }

        public static bool ExportCongNoToExcel(List<CongNoNhaCungCapViewModel> items, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("CongNoNhaCungCap");

                    worksheet.Cell(1, 1).Value = "BÁO CÁO CÔNG NỢ NHÀ CUNG CẤP";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Range(1, 1, 1, 7).Merge();

                    worksheet.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Cell(2, 1).Style.Font.Italic = true;
                    worksheet.Range(2, 1, 2, 7).Merge();

                    string[] headers = { "STT", "Tên nhà cung cấp", "Mã nhà cung cấp", "Địa chỉ", "Điện thoại", "Email", "Còn nợ" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cell(4, i + 1);
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#3498db");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    int row = 5;
                    foreach (var item in items)
                    {
                        worksheet.Cell(row, 1).Value = item.Stt;
                        worksheet.Cell(row, 2).Value = item.Name;
                        worksheet.Cell(row, 3).Value = item.MaNhaCungCap;
                        worksheet.Cell(row, 4).Value = item.DiaChi;
                        worksheet.Cell(row, 5).Value = item.DienThoai;
                        worksheet.Cell(row, 6).Value = item.Email;
                        worksheet.Cell(row, 7).Value = item.ConNo;
                        worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
                        row++;
                    }

                    worksheet.Cell(row, 2).Value = "TỔNG CỘNG:";
                    worksheet.Cell(row, 2).Style.Font.Bold = true;
                    worksheet.Cell(row, 7).Value = items.Sum(x => x.ConNo);
                    worksheet.Cell(row, 7).Style.Font.Bold = true;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ExportCongNoToExcel: " + ex.Message);
                return false;
            }
        }

        public static bool ExportChiTietCongNoToExcel(string nccName, List<ChiTietCongNoNccItemViewModel> items, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("SoChiTietCongNo");

                    worksheet.Cell(1, 1).Value = $"SỔ CHI TIẾT CÔNG NỢ NHÀ CUNG CẤP - {nccName.ToUpper()}";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 15;
                    worksheet.Range(1, 1, 1, 7).Merge();

                    worksheet.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Cell(2, 1).Style.Font.Italic = true;
                    worksheet.Range(2, 1, 2, 7).Merge();

                    string[] headers = { "STT", "Số phiếu", "Ngày", "Tổng cộng", "Diễn giải", "Tiền thanh toán", "Lũy kế" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cell(4, i + 1);
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2980b9");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    int row = 5;
                    foreach (var item in items)
                    {
                        worksheet.Cell(row, 1).Value = item.Stt;
                        worksheet.Cell(row, 2).Value = item.SoPhieu;
                        worksheet.Cell(row, 3).Value = item.NgayFormatted;
                        worksheet.Cell(row, 4).Value = item.TongCong;
                        worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, 5).Value = item.DienGiai;
                        worksheet.Cell(row, 6).Value = item.TienThanhToan;
                        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, 7).Value = item.LuyKe;
                        worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ExportChiTietCongNoToExcel: " + ex.Message);
                return false;
            }
        }
    }
}
