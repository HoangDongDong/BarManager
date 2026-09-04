using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public static class LocalPhieuThuChiService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static object GetValue(IDictionary<string, object> d, string name)
        {
            if (d == null) return null;
            foreach (var kv in d)
            {
                if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
            return null;
        }

        public static async Task<string> GetNextSoPhieuAsync(bool isThu)
        {
            string prefix = isThu ? "PT" : "PC";
            string year2Digits = DateTime.Now.ToString("yy");
            string pattern = $"{prefix}{year2Digits}/";

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string colAmount = isThu ? "THU" : "CHI";
                    string sql = $"SELECT NAME FROM TTHUCHI WHERE {colAmount} > 0 AND NAME LIKE '{pattern}%'";
                    var names = (await conn.QueryAsync<string>(sql)).ToList();

                    int maxNumber = 0;
                    foreach (var name in names)
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        var parts = name.Split('/');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int num))
                        {
                            if (num > maxNumber) maxNumber = num;
                        }
                    }

                    int nextNum = maxNumber + 1;
                    return $"{pattern}{nextNum:D5}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNextSoPhieuAsync: " + ex.Message);
                return $"{pattern}00001";
            }
        }

        public static async Task<List<dynamic>> GetLyDoThuLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var list = (await conn.QueryAsync("SELECT ID, NAME, LALYDOTHU, LOAILYDO FROM DLYDOTHUCHI WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME")).ToList();
                    return list;
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
                    var list = (await conn.QueryAsync("SELECT ID, NAME FROM DCUAHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY ID")).ToList();
                    if (list.Count == 0)
                    {
                        list.Add(new { ID = 1, NAME = "💻 TRU SỞ CHÍNH" });
                    }
                    return list;
                }
            }
            catch
            {
                return new List<dynamic> { new { ID = 1, NAME = "💻 TRU SỞ CHÍNH" } };
            }
        }

        public static async Task<List<dynamic>> GetTaiKhoanNganHangLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var list = (await conn.QueryAsync("SELECT ID, NAME, NOTE FROM DTAIKHOANNGANHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME")).ToList();
                    return list;
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
                    var list = (await conn.QueryAsync("SELECT ID, NAME, DIACHI, DIENTHOAI FROM DKHACHHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    return list;
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
                    var list = (await conn.QueryAsync("SELECT ID, NAME, DIACHI, DIENTHOAI FROM DNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    return list;
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
                    var list = (await conn.QueryAsync("SELECT ID, NAME, DIENTHOAI FROM DNHANVIEN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    return list;
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<(bool Success, string ErrorMessage, string SavedId)> SavePhieuThuChiAsync(
            string id,
            string soPhieu,
            DateTime ngay,
            bool isThu,
            decimal soTien,
            string tenDoiTuong,
            string diaChi,
            string loaiDoiTuong,
            string chungTuGoc,
            string ghiChu,
            string lyDoId,
            string nhanVienId,
            string khachHangId,
            string nhaCungCapId,
            string taiKhoanNganHangId,
            string cuaHangId,
            bool chuyenKhoan,
            bool khongThayDoiCongNo)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    decimal thu = isThu ? soTien : 0;
                    decimal chi = isThu ? 0 : soTien;
                    string loai = isThu ? "1" : "-1";

                    if (string.IsNullOrEmpty(id))
                    {
                        // Thêm mới
                        string nextId = Guid.NewGuid().ToString();

                        // Thử chèn đầy đủ thông tin
                        try
                        {
                            string sql = @"
                                INSERT INTO TTHUCHI (
                                    ID, NAME, NGAY, THU, CHI, LOAI, TENDOITUONG, DIACHI, LOAIDOITUONG, 
                                    CHUNGTUGOC, NOTE, DLYDOTHUCHIID, DNHANVIENID, DKHACHHANGID, DNHACUNGCAPID, 
                                    DTAIKHOANNGANHANGID, DCUAHANGID, CHUYENKHOAN, KHONGTHAYDOICONGNO, 
                                    STATUS, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @ID, @NAME, @NGAY, @THU, @CHI, @LOAI, @TENDOITUONG, @DIACHI, @LOAIDOITUONG,
                                    @CHUNGTUGOC, @NOTE, @DLYDOTHUCHIID, @DNHANVIENID, @DKHACHHANGID, @DNHACUNGCAPID,
                                    @DTAIKHOANNGANHANGID, @DCUAHANGID, @CHUYENKHOAN, @KHONGTHAYDOICONGNO,
                                    30, 1, CURRENT_TIMESTAMP
                                )";

                            await conn.ExecuteAsync(sql, new
                            {
                                ID = nextId,
                                NAME = soPhieu,
                                NGAY = ngay,
                                THU = thu,
                                CHI = chi,
                                LOAI = loai,
                                TENDOITUONG = tenDoiTuong ?? "",
                                DIACHI = diaChi ?? "",
                                LOAIDOITUONG = loaiDoiTuong ?? "",
                                CHUNGTUGOC = chungTuGoc ?? "",
                                NOTE = ghiChu ?? "",
                                DLYDOTHUCHIID = string.IsNullOrEmpty(lyDoId) ? (object)DBNull.Value : lyDoId,
                                DNHANVIENID = string.IsNullOrEmpty(nhanVienId) ? (object)DBNull.Value : nhanVienId,
                                DKHACHHANGID = string.IsNullOrEmpty(khachHangId) ? (object)DBNull.Value : khachHangId,
                                DNHACUNGCAPID = string.IsNullOrEmpty(nhaCungCapId) ? (object)DBNull.Value : nhaCungCapId,
                                DTAIKHOANNGANHANGID = (chuyenKhoan && !string.IsNullOrEmpty(taiKhoanNganHangId)) ? (object)taiKhoanNganHangId : DBNull.Value,
                                DCUAHANGID = string.IsNullOrEmpty(cuaHangId) ? (object)DBNull.Value : cuaHangId,
                                CHUYENKHOAN = chuyenKhoan ? "1" : "0",
                                KHONGTHAYDOICONGNO = khongThayDoiCongNo ? "1" : "0"
                            });

                            return (true, "", nextId);
                        }
                        catch (Exception exIns)
                        {
                            // Fallback chèn rút gọn
                            try
                            {
                                string fallbackSql = @"
                                    INSERT INTO TTHUCHI (
                                        ID, NAME, NGAY, THU, CHI, LOAI, TENDOITUONG, NOTE, 
                                        STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @ID, @NAME, @NGAY, @THU, @CHI, @LOAI, @TENDOITUONG, @NOTE,
                                        30, 1, CURRENT_TIMESTAMP
                                    )";

                                await conn.ExecuteAsync(fallbackSql, new
                                {
                                    ID = nextId,
                                    NAME = soPhieu,
                                    NGAY = ngay,
                                    THU = thu,
                                    CHI = chi,
                                    LOAI = loai,
                                    TENDOITUONG = tenDoiTuong ?? "",
                                    NOTE = ghiChu ?? ""
                                });

                                return (true, "", nextId);
                            }
                            catch (Exception ex2)
                            {
                                return (false, $"Lỗi lưu phiếu: {exIns.Message} | {ex2.Message}", null);
                            }
                        }
                    }
                    else
                    {
                        // Cập nhật
                        try
                        {
                            string sqlUp = @"
                                UPDATE TTHUCHI SET
                                    NAME = @NAME,
                                    NGAY = @NGAY,
                                    THU = @THU,
                                    CHI = @CHI,
                                    LOAI = @LOAI,
                                    TENDOITUONG = @TENDOITUONG,
                                    DIACHI = @DIACHI,
                                    LOAIDOITUONG = @LOAIDOITUONG,
                                    CHUNGTUGOC = @CHUNGTUGOC,
                                    NOTE = @NOTE,
                                    DLYDOTHUCHIID = @DLYDOTHUCHIID,
                                    DNHANVIENID = @DNHANVIENID,
                                    DKHACHHANGID = @DKHACHHANGID,
                                    DNHACUNGCAPID = @DNHACUNGCAPID,
                                    DTAIKHOANNGANHANGID = @DTAIKHOANNGANHANGID,
                                    DCUAHANGID = @DCUAHANGID,
                                    CHUYENKHOAN = @CHUYENKHOAN,
                                    KHONGTHAYDOICONGNO = @KHONGTHAYDOICONGNO,
                                    USERMODIFIEDID = 1,
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE ID = @ID";

                            await conn.ExecuteAsync(sqlUp, new
                            {
                                ID = id,
                                NAME = soPhieu,
                                NGAY = ngay,
                                THU = thu,
                                CHI = chi,
                                LOAI = loai,
                                TENDOITUONG = tenDoiTuong ?? "",
                                DIACHI = diaChi ?? "",
                                LOAIDOITUONG = loaiDoiTuong ?? "",
                                CHUNGTUGOC = chungTuGoc ?? "",
                                NOTE = ghiChu ?? "",
                                DLYDOTHUCHIID = string.IsNullOrEmpty(lyDoId) ? (object)DBNull.Value : lyDoId,
                                DNHANVIENID = string.IsNullOrEmpty(nhanVienId) ? (object)DBNull.Value : nhanVienId,
                                DKHACHHANGID = string.IsNullOrEmpty(khachHangId) ? (object)DBNull.Value : khachHangId,
                                DNHACUNGCAPID = string.IsNullOrEmpty(nhaCungCapId) ? (object)DBNull.Value : nhaCungCapId,
                                DTAIKHOANNGANHANGID = (chuyenKhoan && !string.IsNullOrEmpty(taiKhoanNganHangId)) ? (object)taiKhoanNganHangId : DBNull.Value,
                                DCUAHANGID = string.IsNullOrEmpty(cuaHangId) ? (object)DBNull.Value : cuaHangId,
                                CHUYENKHOAN = chuyenKhoan ? "1" : "0",
                                KHONGTHAYDOICONGNO = khongThayDoiCongNo ? "1" : "0"
                            });

                            return (true, "", id);
                        }
                        catch (Exception exUp)
                        {
                            return (false, $"Lỗi cập nhật phiếu: {exUp.Message}", id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<TTHUCHI> GetPhieuByIdAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var item = await conn.QueryFirstOrDefaultAsync<TTHUCHI>("SELECT * FROM TTHUCHI WHERE ID = @ID", new { ID = id });
                    return item;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<TTHUCHI>> GetAllPhieuListAsync(bool isThu)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string col = isThu ? "THU" : "CHI";
                    var list = (await conn.QueryAsync<TTHUCHI>($"SELECT * FROM TTHUCHI WHERE {col} > 0 AND (STATUS IS NULL OR STATUS <> 0) ORDER BY ID")).ToList();
                    return list;
                }
            }
            catch
            {
                return new List<TTHUCHI>();
            }
        }

        public static async Task<List<PhieuThuChiGridItem>> GetDanhSachPhieuThuChiAsync(
            bool isThu,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string cuaHangId = null,
            string lyDoId = null,
            string taiKhoanNganHangId = null,
            string searchText = null,
            bool isTrash = false)
        {
            var result = new List<PhieuThuChiGridItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Tải trước từ điển Lý do và Cửa hàng để map an toàn
                    var dictLyDo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        var lyDoRows = (await conn.QueryAsync("SELECT ID, NAME FROM DLYDOTHUCHI")).ToList();
                        foreach (var lr in lyDoRows)
                        {
                            var d = lr as IDictionary<string, object>;
                            string k = GetValue(d, "ID")?.ToString()?.Trim();
                            string v = GetValue(d, "NAME")?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(k) && !dictLyDo.ContainsKey(k))
                            {
                                dictLyDo[k] = v ?? "";
                            }
                        }
                    }
                    catch { }

                    var dictCuaHang = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        var chRows = (await conn.QueryAsync("SELECT ID, NAME FROM DCUAHANG")).ToList();
                        foreach (var chr in chRows)
                        {
                            var d = chr as IDictionary<string, object>;
                            string k = GetValue(d, "ID")?.ToString()?.Trim();
                            string v = GetValue(d, "NAME")?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(k) && !dictCuaHang.ContainsKey(k))
                            {
                                dictCuaHang[k] = v ?? "";
                            }
                        }
                    }
                    catch { }

                    var dictUser = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        var userRows = (await conn.QueryAsync("SELECT ID, NAME FROM SUSER")).ToList();
                        foreach (var ur in userRows)
                        {
                            var d = ur as IDictionary<string, object>;
                            string k = GetValue(d, "ID")?.ToString()?.Trim();
                            string v = GetValue(d, "NAME")?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(k) && !dictUser.ContainsKey(k))
                            {
                                dictUser[k] = v ?? "";
                            }
                        }
                    }
                    catch { }

                    string colAmount = isThu ? "THU" : "CHI";
                    var conditions = new List<string>();
                    var p = new DynamicParameters();

                    conditions.Add($"{colAmount} > 0");

                    if (isTrash)
                    {
                        conditions.Add("STATUS = 0");
                    }
                    else
                    {
                        conditions.Add("(STATUS IS NULL OR STATUS <> 0)");
                    }

                    if (fromDate.HasValue)
                    {
                        conditions.Add("NGAY >= @FromDate");
                        p.Add("FromDate", fromDate.Value.Date);
                    }

                    if (toDate.HasValue)
                    {
                        conditions.Add("NGAY < @ToDate");
                        p.Add("ToDate", toDate.Value.Date.AddDays(1));
                    }

                    if (!string.IsNullOrEmpty(cuaHangId) && cuaHangId != "ALL" && cuaHangId != "0")
                    {
                        conditions.Add("DCUAHANGID = @CuaHangId");
                        p.Add("CuaHangId", cuaHangId);
                    }

                    if (!string.IsNullOrEmpty(lyDoId))
                    {
                        if (lyDoId == "NOT_SET") // Chưa thiết lập
                        {
                            conditions.Add("(DLYDOTHUCHIID IS NULL OR DLYDOTHUCHIID = '')");
                        }
                        else if (lyDoId != "ALL" && lyDoId != "TRASH")
                        {
                            conditions.Add("DLYDOTHUCHIID = @LyDoId");
                            p.Add("LyDoId", lyDoId);
                        }
                    }

                    if (!string.IsNullOrEmpty(taiKhoanNganHangId) && taiKhoanNganHangId != "ALL")
                    {
                        conditions.Add("DTAIKHOANNGANHANGID = @TaiKhoanId");
                        p.Add("TaiKhoanId", taiKhoanNganHangId);
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        conditions.Add(@"(
                            UPPER(NAME) LIKE @Search OR 
                            UPPER(TENDOITUONG) LIKE @Search OR 
                            UPPER(DIACHI) LIKE @Search OR 
                            UPPER(NOTE) LIKE @Search OR 
                            UPPER(CHUNGTUGOC) LIKE @Search
                        )");
                        p.Add("Search", $"%{searchText.Trim().ToUpper()}%");
                    }

                    string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

                    string sql = $@"
                        SELECT 
                            ID,
                            NAME AS SOPHIEU,
                            NGAY,
                            TENDOITUONG,
                            DIACHI,
                            DIENGIAI,
                            CHUNGTUGOC,
                            THU,
                            CHI,
                            NOTE AS GHICHU,
                            CHUYENKHOAN,
                            TDATHANGID,
                            LAPHIEUTHUCONGNO,
                            USERCREATEDID,
                            TIMECREATED,
                            USERMODIFIEDID,
                            TIMEMODIFIED,
                            DLYDOTHUCHIID,
                            DCUAHANGID,
                            DTAIKHOANNGANHANGID
                        FROM TTHUCHI
                        {whereClause}
                        ORDER BY NGAY DESC, NAME DESC";

                    var rows = (await conn.QueryAsync(sql, p)).ToList();
                    foreach (var r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string soPhieu = GetValue(dict, "SOPHIEU")?.ToString() ?? "";
                        DateTime? ngay = null;
                        var rawNgay = GetValue(dict, "NGAY");
                        if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dt)) ngay = dt;

                        string tenDoiTuong = GetValue(dict, "TENDOITUONG")?.ToString() ?? "";
                        string diaChi = GetValue(dict, "DIACHI")?.ToString() ?? "";
                        string dLyDoId = GetValue(dict, "DLYDOTHUCHIID")?.ToString();
                        string lyDoThuChi = "";
                        if (!string.IsNullOrEmpty(dLyDoId) && dictLyDo.TryGetValue(dLyDoId.Trim(), out var lName))
                        {
                            lyDoThuChi = lName;
                        }

                        string dienGiai = GetValue(dict, "DIENGIAI")?.ToString() ?? "";
                        string chungTuGoc = GetValue(dict, "CHUNGTUGOC")?.ToString() ?? "";

                        decimal soTien = 0;
                        var rawTien = isThu ? GetValue(dict, "THU") : GetValue(dict, "CHI");
                        if (rawTien != null && decimal.TryParse(rawTien.ToString(), out var dec)) soTien = dec;

                        string ghiChu = GetValue(dict, "GHICHU")?.ToString() ?? "";
                        string ckRaw = GetValue(dict, "CHUYENKHOAN")?.ToString() ?? "";
                        string chuyenKhoan = (ckRaw == "1" || ckRaw == "True") ? "Có" : "Không";

                        string datHang = GetValue(dict, "TDATHANGID")?.ToString() ?? "";
                        string dChId = GetValue(dict, "DCUAHANGID")?.ToString();
                        string cuaHang = "TRỤ SỞ CHÍNH";
                        if (!string.IsNullOrEmpty(dChId) && dictCuaHang.TryGetValue(dChId.Trim(), out var chName))
                        {
                            cuaHang = chName;
                        }

                        string lpRaw = GetValue(dict, "LAPHIEUTHUCONGNO")?.ToString() ?? "";
                        string laPhieuThuCongNo = (lpRaw == "1" || lpRaw == "True") ? "Có" : "Không";

                        string uCreatedId = GetValue(dict, "USERCREATEDID")?.ToString()?.Trim() ?? "";
                        string userCreated = "Administrator";
                        if (!string.IsNullOrEmpty(uCreatedId))
                        {
                            if (dictUser.TryGetValue(uCreatedId, out var ucName) && !string.IsNullOrEmpty(ucName))
                                userCreated = ucName;
                            else if (uCreatedId == "1" || uCreatedId.Length > 10)
                                userCreated = "Administrator";
                            else
                                userCreated = uCreatedId;
                        }

                        DateTime? tCreated = null;
                        var rawTCreated = GetValue(dict, "TIMECREATED");
                        if (rawTCreated != null && DateTime.TryParse(rawTCreated.ToString(), out var dtc)) tCreated = dtc;

                        string uModifiedId = GetValue(dict, "USERMODIFIEDID")?.ToString()?.Trim() ?? "";
                        string userModified = "";
                        if (!string.IsNullOrEmpty(uModifiedId))
                        {
                            if (dictUser.TryGetValue(uModifiedId, out var umName) && !string.IsNullOrEmpty(umName))
                                userModified = umName;
                            else if (uModifiedId == "1")
                                userModified = "Administrator";
                            else
                                userModified = uModifiedId;
                        }

                        DateTime? tModified = null;
                        var rawTModified = GetValue(dict, "TIMEMODIFIED");
                        if (rawTModified != null && DateTime.TryParse(rawTModified.ToString(), out var dtm)) tModified = dtm;

                        // Nếu chưa sửa đổi lần nào hoặc trùng timecreated
                        if (tModified.HasValue && tCreated.HasValue && Math.Abs((tModified.Value - tCreated.Value).TotalSeconds) < 2 && string.IsNullOrEmpty(uModifiedId))
                        {
                            tModified = null;
                            userModified = "";
                        }

                        string dTkId = GetValue(dict, "DTAIKHOANNGANHANGID")?.ToString();

                        result.Add(new PhieuThuChiGridItem
                        {
                            Id = id,
                            SoPhieu = soPhieu,
                            Ngay = ngay,
                            TenDoiTuong = tenDoiTuong,
                            DiaChi = diaChi,
                            LyDoThuChi = lyDoThuChi,
                            DienGiai = dienGiai,
                            ChungTuGoc = chungTuGoc,
                            SoTien = soTien,
                            GhiChu = ghiChu,
                            ChuyenKhoan = chuyenKhoan,
                            DatHang = datHang,
                            CuaHang = cuaHang,
                            LaPhieuThuCongNo = laPhieuThuCongNo,
                            UserCreated = userCreated,
                            TimeCreated = tCreated,
                            UserModified = userModified,
                            TimeModified = tModified,
                            DLyDoThuChiId = dLyDoId,
                            DCuaHangId = dChId,
                            DTaiKhoanNganHangId = dTkId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetDanhSachPhieuThuChiAsync: " + ex.Message);
            }
            return result;
        }

        public static async Task<bool> DeletePhieuThuChiAsync(string id, bool permanent = false)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    if (permanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM TTHUCHI WHERE ID = @ID", new { ID = id });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE TTHUCHI SET STATUS = 0, USERMODIFIEDID = 1, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @ID", new { ID = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePhieuThuChiAsync: " + ex.Message);
                return false;
            }
        }
    }

    public class PhieuThuChiGridItem
    {
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string TenDoiTuong { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string LyDoThuChi { get; set; } = "";
        public string DienGiai { get; set; } = "";
        public string ChungTuGoc { get; set; } = "";
        public decimal SoTien { get; set; }
        public string GhiChu { get; set; } = "";
        public string ChuyenKhoan { get; set; } = "";
        public string DatHang { get; set; } = "";
        public string CuaHang { get; set; } = "";
        public string LaPhieuThuCongNo { get; set; } = "";
        public string UserCreated { get; set; } = "";
        public DateTime? TimeCreated { get; set; }
        public string UserModified { get; set; } = "";
        public DateTime? TimeModified { get; set; }
        public string DLyDoThuChiId { get; set; }
        public string DCuaHangId { get; set; }
        public string DTaiKhoanNganHangId { get; set; }
    }
}
