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
    public class CongNoKhachHangViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string Makhach { get; set; } = "";
        public string Name { get; set; } = "";
        public string Diachi { get; set; } = "";
        public string Dienthoai { get; set; } = "";
        public string Email { get; set; } = "";
        public string Masothue { get; set; } = "";
        public string DnhomkhachhangId { get; set; } = "";
        public string TenNhom { get; set; } = "";
        public decimal TongPhatSinh { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public string Note { get; set; } = "";

        public string TongPhatSinhFormatted => TongPhatSinh.ToString("N0");
        public string DaThanhToanFormatted => DaThanhToan.ToString("N0");
        public string ConNoFormatted => ConNo.ToString("N0");
    }

    public class ChiTietCongNoItemViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string LoaiPhieu { get; set; } = "";
        public decimal TongCong { get; set; }
        public decimal TienThanhToan { get; set; }
        public string DienGiai { get; set; } = "";
        public decimal LuyKe { get; set; }

        public string NgayFormatted => Ngay?.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string TongCongFormatted => TongCong != 0 ? TongCong.ToString("N0") : "";
        public string TienThanhToanFormatted => TienThanhToan != 0 ? TienThanhToan.ToString("N0") : "";
        public string LuyKeFormatted => LuyKe.ToString("N0");
    }

    public static class LocalCongNoKhachHangService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static int ParseInt(object val)
        {
            if (val == null) return 0;
            string s = val.ToString().Trim();
            if (int.TryParse(s, out int res)) return res;
            return 0;
        }

        public static async Task<ObservableCollection<NhomKhachHangTreeItem>> GetNhomKhachHangTreeAsync()
        {
            var result = new ObservableCollection<NhomKhachHangTreeItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DNHOMKHACHHANG WHERE STATUS > 0 OR STATUS IS NULL ORDER BY SORTORDER, NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var rootItem = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        ItemType = 0,
                        Icon = "📁",
                        IconColor = "#f0ad4e",
                        IsExpanded = true,
                        IsSelected = true
                    };

                    var folders = new Dictionary<string, NhomKhachHangTreeItem>();
                    var items = new List<dynamic>();

                    foreach (var r in rows)
                    {
                        int itemType = ParseInt(r.ITEMTYPE);
                        int parentDir = ParseInt(r.PARENTDIR);

                        if (itemType == 1 || parentDir == 1)
                        {
                            var fNode = new NhomKhachHangTreeItem
                            {
                                Id = r.ID?.ToString(),
                                Name = r.NAME?.ToString(),
                                ParentId = r.PARENTID?.ToString()?.Trim(),
                                ItemType = 2,
                                Icon = "📁",
                                IconColor = "#f0ad4e",
                                IsExpanded = true
                            };
                            folders[fNode.Id] = fNode;
                        }
                        else
                        {
                            items.Add(r);
                        }
                    }

                    foreach (var f in folders.Values)
                    {
                        if (!string.IsNullOrEmpty(f.ParentId) && folders.ContainsKey(f.ParentId))
                        {
                            folders[f.ParentId].Children.Add(f);
                        }
                        else
                        {
                            rootItem.Children.Add(f);
                        }
                    }

                    foreach (var it in items)
                    {
                        string parentId = it.PARENTID?.ToString()?.Trim();
                        var itNode = new NhomKhachHangTreeItem
                        {
                            Id = it.ID?.ToString(),
                            Name = it.NAME?.ToString(),
                            ParentId = parentId,
                            ItemType = 2,
                            Icon = "📁",
                            IconColor = "#f0ad4e",
                            IsExpanded = true
                        };

                        if (!string.IsNullOrEmpty(parentId) && folders.ContainsKey(parentId))
                        {
                            folders[parentId].Children.Add(itNode);
                        }
                        else
                        {
                            rootItem.Children.Add(itNode);
                        }
                    }

                    result.Add(rootItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomKhachHangTreeAsync: " + ex.Message);
            }

            return result;
        }

        public static async Task<List<CongNoKhachHangViewModel>> GetCongNoKhachHangListAsync(
            string filterNhomId = "ALL", 
            string keyword = "", 
            int debtFilterMode = 0, // 0: Tất cả, 1: Chỉ còn nợ > 0, 2: Có phát sinh trong kỳ
            DateTime? tuNgay = null, 
            DateTime? denNgay = null)
        {
            var list = new List<CongNoKhachHangViewModel>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Lấy danh sách khách hàng
                    string sqlKhach = @"
                        SELECT 
                            k.ID, k.MAKHACH, k.NAME, k.DIACHI, k.DIENTHOAI, k.EMAIL, 
                            k.MASOTHUE, k.NOTE, k.DNHOMKHACHHANGID, n.NAME AS TENNHOM
                        FROM DKHACHHANG k
                        LEFT JOIN DNHOMKHACHHANG n ON CAST(k.DNHOMKHACHHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS > 0)";

                    if (!string.IsNullOrEmpty(filterNhomId) && filterNhomId != "ALL" && filterNhomId != "UNASSIGNED")
                    {
                        sqlKhach += " AND CAST(k.DNHOMKHACHHANGID AS VARCHAR(50)) = @NhomId";
                    }
                    else if (filterNhomId == "UNASSIGNED")
                    {
                        sqlKhach += " AND (k.DNHOMKHACHHANGID IS NULL OR TRIM(CAST(k.DNHOMKHACHHANGID AS VARCHAR(50))) = '')";
                    }

                    sqlKhach += " ORDER BY k.MAKHACH, k.NAME";

                    var customers = (await conn.QueryAsync(sqlKhach, new { NhomId = filterNhomId })).ToList();
                    if (customers.Count == 0) return list;

                    // 2. Tính tổng phát sinh nợ & đã trả từ các đơn hàng bán (TDONHANG)
                    string sqlDonHang = @"
                        SELECT 
                            CAST(DKHACHHANGID AS VARCHAR(50)) AS KHACHID,
                            SUM(COALESCE(TONGCONG, 0)) AS TONGTIENHANG,
                            SUM(COALESCE(TIENTHANHTOAN, 0)) AS DATHANHTOAN_DONHANG,
                            SUM(COALESCE(CONNO, 0)) AS CONNO_DONHANG
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND DKHACHHANGID IS NOT NULL";

                    if (tuNgay.HasValue)
                    {
                        sqlDonHang += " AND NGAY >= @TuNgay";
                    }
                    if (denNgay.HasValue)
                    {
                        sqlDonHang += " AND NGAY <= @DenNgay";
                    }

                    sqlDonHang += " GROUP BY DKHACHHANGID";

                    var donHangSummaries = (await conn.QueryAsync(sqlDonHang, new { TuNgay = tuNgay, DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) }))
                        .ToDictionary(x => (string)x.KHACHID?.ToString(), x => new {
                            TongTienHang = (decimal)(x.TONGTIENHANG ?? 0),
                            DaThanhToan = (decimal)(x.DATHANHTOAN_DONHANG ?? 0),
                            ConNo = (decimal)(x.CONNO_DONHANG ?? 0)
                        });

                    // 3. Tính tổng tiền thu nợ từ phiếu thu độc lập (TTHUCHI)
                    string sqlThuChi = @"
                        SELECT 
                            CAST(DKHACHHANGID AS VARCHAR(50)) AS KHACHID,
                            SUM(COALESCE(THU, 0)) AS TONGTHU
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND DKHACHHANGID IS NOT NULL
                          AND (LAPHIEUTHUCONGNO > 0 OR LOAI = '1' OR LOAI = 'thu' OR LOAI = 'Thu')";

                    if (tuNgay.HasValue)
                    {
                        sqlThuChi += " AND NGAY >= @TuNgay";
                    }
                    if (denNgay.HasValue)
                    {
                        sqlThuChi += " AND NGAY <= @DenNgay";
                    }

                    sqlThuChi += " GROUP BY DKHACHHANGID";

                    var thuChiSummaries = (await conn.QueryAsync(sqlThuChi, new { TuNgay = tuNgay, DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) }))
                        .ToDictionary(x => (string)x.KHACHID?.ToString(), x => (decimal)(x.TONGTHU ?? 0));

                    int stt = 1;
                    foreach (var c in customers)
                    {
                        string khId = c.ID?.ToString() ?? "";
                        decimal tongPhatSinh = 0;
                        decimal daThanhToan = 0;

                        if (donHangSummaries.TryGetValue(khId, out var dhSum))
                        {
                            tongPhatSinh += dhSum.TongTienHang;
                            daThanhToan += dhSum.DaThanhToan;
                        }

                        if (thuChiSummaries.TryGetValue(khId, out var tcSum))
                        {
                            daThanhToan += tcSum;
                        }

                        decimal conNo = tongPhatSinh - daThanhToan;

                        // Lọc theo chế độ nợ
                        if (debtFilterMode == 1 && conNo <= 0) continue; // Chỉ khách còn nợ > 0
                        if (debtFilterMode == 2 && tongPhatSinh == 0 && daThanhToan == 0) continue; // Chỉ khách có phát sinh

                        // Lọc theo từ khóa tìm kiếm
                        if (!string.IsNullOrWhiteSpace(keyword))
                        {
                            string kw = keyword.Trim().ToLower();
                            string ma = (c.MAKHACH?.ToString() ?? "").ToLower();
                            string name = (c.NAME?.ToString() ?? "").ToLower();
                            string dc = (c.DIACHI?.ToString() ?? "").ToLower();
                            string sdt = (c.DIENTHOAI?.ToString() ?? "").ToLower();
                            string email = (c.EMAIL?.ToString() ?? "").ToLower();
                            string mst = (c.MASOTHUE?.ToString() ?? "").ToLower();

                            if (!ma.Contains(kw) && !name.Contains(kw) && !dc.Contains(kw) && 
                                !sdt.Contains(kw) && !email.Contains(kw) && !mst.Contains(kw))
                            {
                                continue;
                            }
                        }

                        list.Add(new CongNoKhachHangViewModel
                        {
                            Stt = stt++,
                            Id = khId,
                            Makhach = c.MAKHACH?.ToString() ?? "",
                            Name = c.NAME?.ToString() ?? "",
                            Diachi = c.DIACHI?.ToString() ?? "",
                            Dienthoai = c.DIENTHOAI?.ToString() ?? "",
                            Email = c.EMAIL?.ToString() ?? "",
                            Masothue = c.MASOTHUE?.ToString() ?? "",
                            DnhomkhachhangId = c.DNHOMKHACHHANGID?.ToString() ?? "",
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
                Console.WriteLine("Error GetCongNoKhachHangListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<ChiTietCongNoItemViewModel>> GetChiTietCongNoKhachHangAsync(
            string khachHangId, 
            DateTime? tuNgay = null, 
            DateTime? denNgay = null)
        {
            var list = new List<ChiTietCongNoItemViewModel>();
            if (string.IsNullOrEmpty(khachHangId)) return list;

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Lấy tất cả đơn hàng bán của khách hàng này
                    string sqlDonHang = @"
                        SELECT 
                            ID, NAME AS SOPHIEU, NGAY, TONGCONG, TIENTHANHTOAN, CONNO, DIENGIAI
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND CAST(DKHACHHANGID AS VARCHAR(50)) = @KhachId";

                    if (tuNgay.HasValue) sqlDonHang += " AND NGAY >= @TuNgay";
                    if (denNgay.HasValue) sqlDonHang += " AND NGAY <= @DenNgay";

                    var donHangs = (await conn.QueryAsync(sqlDonHang, new { 
                        KhachId = khachHangId, 
                        TuNgay = tuNgay, 
                        DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) 
                    })).ToList();

                    // 2. Lấy tất cả phiếu thu tiền của khách hàng này
                    string sqlThuChi = @"
                        SELECT 
                            ID, NAME AS SOPHIEU, NGAY, THU AS SOTIEN, DIENGIAI, LAPHIEUTHUCONGNO
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS > 0)
                          AND CAST(DKHACHHANGID AS VARCHAR(50)) = @KhachId
                          AND (LAPHIEUTHUCONGNO > 0 OR LOAI = '1' OR LOAI = 'thu' OR LOAI = 'Thu')";

                    if (tuNgay.HasValue) sqlThuChi += " AND NGAY >= @TuNgay";
                    if (denNgay.HasValue) sqlThuChi += " AND NGAY <= @DenNgay";

                    var thuChis = (await conn.QueryAsync(sqlThuChi, new { 
                        KhachId = khachHangId, 
                        TuNgay = tuNgay, 
                        DenNgay = denNgay?.Date.AddDays(1).AddSeconds(-1) 
                    })).ToList();

                    // 3. Hợp nhất danh sách giao dịch
                    var rawTransactions = new List<ChiTietCongNoItemViewModel>();

                    foreach (var dh in donHangs)
                    {
                        DateTime? dt = null;
                        if (dh.NGAY != null)
                        {
                            if (dh.NGAY is DateTime dVal) dt = dVal;
                            else if (DateTime.TryParse(dh.NGAY.ToString(), out DateTime pVal)) dt = pVal;
                        }

                        decimal tong = (decimal)(dh.TONGCONG ?? 0);
                        decimal tt = (decimal)(dh.TIENTHANHTOAN ?? 0);

                        rawTransactions.Add(new ChiTietCongNoItemViewModel
                        {
                            Id = dh.ID?.ToString() ?? "",
                            SoPhieu = dh.SOPHIEU?.ToString() ?? "",
                            Ngay = dt,
                            LoaiPhieu = "Hóa đơn bán hàng",
                            TongCong = tong,
                            TienThanhToan = tt,
                            DienGiai = dh.DIENGIAI?.ToString() ?? "Bán hàng / Dịch vụ"
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

                        decimal tienThu = (decimal)(tc.SOTIEN ?? 0);

                        rawTransactions.Add(new ChiTietCongNoItemViewModel
                        {
                            Id = tc.ID?.ToString() ?? "",
                            SoPhieu = tc.SOPHIEU?.ToString() ?? "",
                            Ngay = dt,
                            LoaiPhieu = "Phiếu thu công nợ",
                            TongCong = 0,
                            TienThanhToan = tienThu,
                            DienGiai = tc.DIENGIAI?.ToString() ?? "Thu tiền nợ khách hàng"
                        });
                    }

                    // Sắp xếp theo ngày tăng dần để tính lũy kế (Hóa đơn trước, Phiếu thu sau)
                    var sorted = rawTransactions
                        .OrderBy(x => (x.Ngay ?? DateTime.MinValue).Date)
                        .ThenBy(x => x.LoaiPhieu == "Hóa đơn bán hàng" ? 0 : 1)
                        .ThenBy(x => x.SoPhieu)
                        .ToList();

                    decimal luyKe = 0;
                    int stt = 1;
                    foreach (var item in sorted)
                    {
                        luyKe += (item.TongCong - item.TienThanhToan);
                        item.Stt = stt++;
                        item.LuyKe = luyKe;
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetChiTietCongNoKhachHangAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<string> GetNextSoPhieuThuAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string yearPrefix = DateTime.Now.ToString("yy");
                    string prefix = $"PT{yearPrefix}/";

                    string sql = $"SELECT NAME FROM TTHUCHI WHERE NAME LIKE '{prefix}%' ORDER BY NAME DESC";
                    var list = (await conn.QueryAsync<string>(sql)).ToList();
                    int maxNum = 0;
                    foreach (var code in list)
                    {
                        if (!string.IsNullOrEmpty(code) && code.StartsWith(prefix))
                        {
                            string numPart = code.Substring(prefix.Length).Trim();
                            if (int.TryParse(numPart, out int n))
                            {
                                if (n > maxNum) maxNum = n;
                            }
                        }
                    }
                    return $"{prefix}{(maxNum + 1):D5}";
                }
            }
            catch
            {
                return $"PT{DateTime.Now:yy}/00001";
            }
        }

        public static async Task<List<dynamic>> GetLyDoThuLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DLYDOTHUCHI WHERE STATUS > 0 OR STATUS IS NULL ORDER BY NAME";
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
                    string sql = "SELECT ID, NAME FROM DCUAHANG WHERE STATUS > 0 OR STATUS IS NULL ORDER BY NAME";
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
                    string sql = "SELECT ID, NAME FROM DNHANVIEN WHERE STATUS > 0 OR STATUS IS NULL ORDER BY NAME";
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
                    string sql = "SELECT ID, MAKHACH, NAME, DIACHI FROM DKHACHHANG WHERE STATUS > 0 OR STATUS IS NULL ORDER BY NAME";
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
                    string sql = "SELECT ID, MANHACUNGCAP, NAME, DIACHI FROM DNHACUNGCAP WHERE STATUS > 0 OR STATUS IS NULL ORDER BY NAME";
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
                    string sql = "SELECT ID, NAME FROM DTAIKHOANNGANHANG WHERE STATUS > 0 OR STATUS IS NULL ORDER BY NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<bool> SavePhieuThuFullAsync(
            DateTime ngay,
            string soPhieu,
            string phanLoai,
            string lyDoId,
            string chungTuGoc,
            string loaiDoiTuong,
            string tenDoiTuong,
            string diaChi,
            string nhanVienId,
            string khachHangId,
            string nhaCungCapId,
            decimal soTien,
            int chuyenKhoan,
            string taiKhoanNganHangId,
            string cuaHangId,
            int khongThayDoiCongNo,
            string ghiChu)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string id = Guid.NewGuid().ToString();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";
                    DateTime now = DateTime.Now;

                    int laPhieuThuCongNo = (khongThayDoiCongNo == 1) ? 0 : 1;

                    string sql = @"
                        INSERT INTO TTHUCHI (
                            ID, NAME, NGAY, TENDOITUONG, DIACHI, LOAI, LOAIDOITUONG, DIENGIAI, 
                            CHUNGTUGOC, THU, CHI, STATUS, TIMECREATED, TIMEMODIFIED, 
                            USERCREATEDID, USERMODIFIEDID, CHUYENKHOAN, LAPHIEUTHUCONGNO, 
                            KHONGTHAYDOICONGNO, DNHANVIENID, DKHACHHANGID, DNHACUNGCAPID,
                            DLYDOTHUCHIID, DTAIKHOANNGANHANGID, DCUAHANGID, NOTE
                        ) VALUES (
                            @Id, @Name, @Ngay, @TenDoiTuong, @DiaChi, 'thu', @LoaiDoiTuong, @PhanLoai, 
                            @ChungTuGoc, @SoTien, 0, 30, @Now, @Now, 
                            @UserId, @UserId, @ChuyenKhoan, @LaPhieuThuCongNo, 
                            @KhongThayDoiCongNo, @NhanVienId, @KhachHangId, @NhaCungCapId,
                            @LyDoId, @TaiKhoanNganHangId, @CuaHangId, @GhiChu
                        )";

                    int loaiDoiTuongVal = 0;
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
                        Id = id,
                        Name = soPhieu,
                        Ngay = ngay,
                        TenDoiTuong = tenDoiTuong,
                        DiaChi = diaChi,
                        LoaiDoiTuong = loaiDoiTuongVal,
                        PhanLoai = phanLoai,
                        ChungTuGoc = chungTuGoc,
                        SoTien = soTien,
                        Now = now,
                        UserId = userId,
                        ChuyenKhoan = chuyenKhoan,
                        LaPhieuThuCongNo = laPhieuThuCongNo,
                        KhongThayDoiCongNo = khongThayDoiCongNo,
                        NhanVienId = string.IsNullOrEmpty(nhanVienId) ? null : nhanVienId,
                        KhachHangId = string.IsNullOrEmpty(khachHangId) ? null : khachHangId,
                        NhaCungCapId = string.IsNullOrEmpty(nhaCungCapId) ? null : nhaCungCapId,
                        LyDoId = string.IsNullOrEmpty(lyDoId) ? null : lyDoId,
                        TaiKhoanNganHangId = string.IsNullOrEmpty(taiKhoanNganHangId) ? null : taiKhoanNganHangId,
                        CuaHangId = string.IsNullOrEmpty(cuaHangId) ? null : cuaHangId,
                        GhiChu = ghiChu
                    });

                    // Ghi nhật ký lưu vết
                    try
                    {
                        string logContent = $"Tạo phiếu thu {soPhieu} - Đối tượng: {tenDoiTuong} - Số tiền: {soTien:N0} đ";
                        await LocalLuuVetService.GhiLuuVetAsync(soPhieu, "Phiếu thu", "Thêm mới phiếu thu", logContent);
                    }
                    catch { }

                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SavePhieuThuFullAsync: " + ex.Message);
                return false;
            }
        }

        public static void ExportCongNoToExcel(List<CongNoKhachHangViewModel> list, string filePath)
        {
            if (list == null) return;

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("TongHopCongNo");

                // Tiêu đề báo cáo
                ws.Cell(1, 1).Value = "TỔNG HỢP CÔNG NỢ KHÁCH HÀNG";
                ws.Range(1, 1, 1, 8).Merge().Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(2, 1).Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range(2, 1, 2, 8).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Header cột
                string[] headers = { "STT", "Mã khách", "Tên khách hàng", "Địa chỉ", "Điện thoại", "Email", "Mã số thuế", "Còn nợ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(4, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#dfe9f5");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                int row = 5;
                decimal totalConNo = 0;

                foreach (var item in list)
                {
                    ws.Cell(row, 1).Value = item.Stt;
                    ws.Cell(row, 2).Value = item.Makhach;
                    ws.Cell(row, 3).Value = item.Name;
                    ws.Cell(row, 4).Value = item.Diachi;
                    ws.Cell(row, 5).Value = item.Dienthoai;
                    ws.Cell(row, 6).Value = item.Email;
                    ws.Cell(row, 7).Value = item.Masothue;
                    ws.Cell(row, 8).Value = item.ConNo;
                    ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";

                    totalConNo += item.ConNo;

                    for (int c = 1; c <= 8; c++)
                    {
                        ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Dòng tổng cộng
                ws.Cell(row, 1).Value = "TỔNG CỘNG";
                ws.Range(row, 1, row, 7).Merge().Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 8).Value = totalConNo;
                ws.Cell(row, 8).Style.Font.Bold = true;
                ws.Cell(row, 8).Style.Font.FontColor = XLColor.Red;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";

                for (int c = 1; c <= 8; c++)
                {
                    ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#fff2cc");
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public static void ExportChiTietCongNoToExcel(CongNoKhachHangViewModel khach, List<ChiTietCongNoItemViewModel> list, string filePath)
        {
            if (khach == null || list == null) return;

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("ChiTietCongNo");

                // Header
                ws.Cell(1, 1).Value = "SỔ CHI TIẾT CÔNG NỢ KHÁCH HÀNG";
                ws.Range(1, 1, 1, 7).Merge().Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(2, 1).Value = $"Khách hàng: {khach.Name} ({khach.Makhach}) - ĐT: {khach.Dienthoai} - ĐC: {khach.Diachi}";
                ws.Range(2, 1, 2, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(3, 1).Value = $"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range(3, 1, 3, 7).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                string[] headers = { "STT", "Số phiếu", "Ngày", "Tổng cộng", "Tiền thanh toán", "Diễn giải", "Lũy kế" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(5, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#dfe9f5");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                int row = 6;
                decimal tongCong = 0;
                decimal tongThanhToan = 0;

                foreach (var item in list)
                {
                    ws.Cell(row, 1).Value = item.Stt;
                    ws.Cell(row, 2).Value = item.SoPhieu;
                    ws.Cell(row, 3).Value = item.NgayFormatted;
                    ws.Cell(row, 4).Value = item.TongCong;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 5).Value = item.TienThanhToan;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 6).Value = item.DienGiai;
                    ws.Cell(row, 7).Value = item.LuyKe;
                    ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0";

                    tongCong += item.TongCong;
                    tongThanhToan += item.TienThanhToan;

                    for (int c = 1; c <= 7; c++)
                    {
                        ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Dòng tổng
                ws.Cell(row, 1).Value = "TỔNG CỘNG";
                ws.Range(row, 1, row, 3).Merge().Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 4).Value = tongCong;
                ws.Cell(row, 4).Style.Font.Bold = true;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 5).Value = tongThanhToan;
                ws.Cell(row, 5).Style.Font.Bold = true;
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 6).Value = "";
                ws.Cell(row, 7).Value = khach.ConNo;
                ws.Cell(row, 7).Style.Font.Bold = true;
                ws.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0";

                for (int c = 1; c <= 7; c++)
                {
                    ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#fff2cc");
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }
    }
}
