using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class TamUngLuongItemViewModel
    {
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string NgayStr => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
        public string GioStr => Ngay.HasValue ? Ngay.Value.ToString("HH:mm") : "";
        public string NhanVienId { get; set; } = "";
        public string NguoiNhan { get; set; } = "";
        public decimal SoTien { get; set; } = 0;
        public string SoTienStr => SoTien.ToString("N0");
        public string DienGiai { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; } = 30;
    }

    public static class LocalTamUngLuongService
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

        public static async Task<string> GetNextSoPhieuAsync()
        {
            string prefix = "PC";
            string year2Digits = DateTime.Now.ToString("yy");
            string pattern = $"{prefix}{year2Digits}/";

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = $"SELECT NAME FROM TTHUCHI WHERE (LATAMUNG = '1' OR LATAMUNG = 1 OR CHI > 0) AND NAME LIKE '{pattern}%'";
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

        public static async Task<List<TamUngLuongItemViewModel>> GetListAsync(DateTime fromDate, DateTime toDate, string searchKeyword = null)
        {
            var result = new List<TamUngLuongItemViewModel>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT t.ID, t.NAME as SOPHIEU, t.NGAY, t.TENDOITUONG, t.CHI as SOTIEN, 
                               t.DIENGIAI, t.NOTE, t.STATUS, t.DNHANVIENID,
                               nv.NAME as NHANVIENNAME
                        FROM TTHUCHI t
                        LEFT JOIN DNHANVIEN nv ON CAST(t.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE (t.LATAMUNG = '1' OR t.LATAMUNG = 1)
                          AND (t.STATUS IS NULL OR t.STATUS <> 0)
                          AND CAST(t.NGAY AS DATE) >= @FromDate
                          AND CAST(t.NGAY AS DATE) <= @ToDate
                        ORDER BY t.NGAY DESC, t.NAME DESC";

                    var rows = (await conn.QueryAsync(sql, new
                    {
                        FromDate = fromDate.Date,
                        ToDate = toDate.Date
                    })).ToList();

                    foreach (object r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string soPhieu = GetValue(dict, "SOPHIEU")?.ToString() ?? "";
                        
                        DateTime? ngay = null;
                        var rawNgay = GetValue(dict, "NGAY");
                        if (rawNgay is DateTime dt) ngay = dt;
                        else if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dt2)) ngay = dt2;

                        string nhanVienId = GetValue(dict, "DNHANVIENID")?.ToString() ?? "";
                        string nhanVienName = GetValue(dict, "NHANVIENNAME")?.ToString();
                        string tenDoiTuong = GetValue(dict, "TENDOITUONG")?.ToString();
                        string nguoiNhan = !string.IsNullOrWhiteSpace(nhanVienName) ? nhanVienName : (tenDoiTuong ?? "");

                        decimal soTien = 0;
                        var rawTien = GetValue(dict, "SOTIEN");
                        if (rawTien != null && decimal.TryParse(rawTien.ToString(), out decimal stVal)) soTien = stVal;

                        string dienGiai = GetValue(dict, "DIENGIAI")?.ToString() ?? "";
                        string note = GetValue(dict, "NOTE")?.ToString() ?? "";

                        result.Add(new TamUngLuongItemViewModel
                        {
                            Id = id,
                            SoPhieu = soPhieu,
                            Ngay = ngay,
                            NhanVienId = nhanVienId,
                            NguoiNhan = nguoiNhan,
                            SoTien = soTien,
                            DienGiai = dienGiai,
                            Note = note
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        string kw = searchKeyword.Trim().ToLower();
                        result = result.Where(x => 
                            (x.SoPhieu != null && x.SoPhieu.ToLower().Contains(kw)) ||
                            (x.NguoiNhan != null && x.NguoiNhan.ToLower().Contains(kw)) ||
                            (x.DienGiai != null && x.DienGiai.ToLower().Contains(kw)) ||
                            (x.Note != null && x.Note.ToLower().Contains(kw))
                        ).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetTamUngLuongList: " + ex.Message);
            }
            return result;
        }

        public static async Task<TamUngLuongItemViewModel> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT t.ID, t.NAME as SOPHIEU, t.NGAY, t.TENDOITUONG, t.CHI as SOTIEN, 
                               t.DIENGIAI, t.NOTE, t.STATUS, t.DNHANVIENID,
                               nv.NAME as NHANVIENNAME
                        FROM TTHUCHI t
                        LEFT JOIN DNHANVIEN nv ON CAST(t.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE CAST(t.ID AS VARCHAR(50)) = @Id";

                    var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id.Trim() });
                    if (row != null)
                    {
                        var dict = row as IDictionary<string, object>;
                        string nhanVienId = GetValue(dict, "DNHANVIENID")?.ToString() ?? "";
                        string nhanVienName = GetValue(dict, "NHANVIENNAME")?.ToString();
                        string tenDoiTuong = GetValue(dict, "TENDOITUONG")?.ToString();

                        DateTime? ngay = null;
                        var rawNgay = GetValue(dict, "NGAY");
                        if (rawNgay is DateTime dt) ngay = dt;
                        else if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dt2)) ngay = dt2;

                        decimal soTien = 0;
                        var rawTien = GetValue(dict, "SOTIEN");
                        if (rawTien != null && decimal.TryParse(rawTien.ToString(), out decimal stVal)) soTien = stVal;

                        return new TamUngLuongItemViewModel
                        {
                            Id = GetValue(dict, "ID")?.ToString() ?? "",
                            SoPhieu = GetValue(dict, "SOPHIEU")?.ToString() ?? "",
                            Ngay = ngay,
                            NhanVienId = nhanVienId,
                            NguoiNhan = !string.IsNullOrWhiteSpace(nhanVienName) ? nhanVienName : (tenDoiTuong ?? ""),
                            SoTien = soTien,
                            DienGiai = GetValue(dict, "DIENGIAI")?.ToString() ?? "",
                            Note = GetValue(dict, "NOTE")?.ToString() ?? ""
                        };
                    }
                }
            }
            catch { }
            return null;
        }

        public static async Task<bool> SaveAsync(string id, string soPhieu, DateTime ngay, string nhanVienId, string tenNhanVien, decimal soTien, string ghiChu)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    if (string.IsNullOrEmpty(id))
                    {
                        string newId = Guid.NewGuid().ToString();
                        if (string.IsNullOrEmpty(soPhieu))
                        {
                            soPhieu = await GetNextSoPhieuAsync();
                        }

                        string sqlInsert = @"
                            INSERT INTO TTHUCHI (
                                ID, NAME, NGAY, TENDOITUONG, DNHANVIENID, LOAI, LOAIDOITUONG, 
                                CHI, THU, LATAMUNG, DIENGIAI, NOTE, STATUS, TIMECREATED
                            ) VALUES (
                                @Id, @Name, @Ngay, @TenDoiTuong, @NhanVienId, -1, 2, 
                                @Chi, 0, '1', @DienGiai, @Note, 30, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(sqlInsert, new
                        {
                            Id = newId,
                            Name = soPhieu,
                            Ngay = ngay,
                            TenDoiTuong = tenNhanVien ?? "",
                            NhanVienId = string.IsNullOrEmpty(nhanVienId) ? null : nhanVienId,
                            Chi = soTien,
                            DienGiai = ghiChu ?? "Tạm ứng lương",
                            Note = ghiChu ?? ""
                        });
                    }
                    else
                    {
                        string sqlUpdate = @"
                            UPDATE TTHUCHI SET
                                NAME = @Name,
                                NGAY = @Ngay,
                                TENDOITUONG = @TenDoiTuong,
                                DNHANVIENID = @NhanVienId,
                                CHI = @Chi,
                                LATAMUNG = '1',
                                DIENGIAI = @DienGiai,
                                NOTE = @Note,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(sqlUpdate, new
                        {
                            Id = id,
                            Name = soPhieu,
                            Ngay = ngay,
                            TenDoiTuong = tenNhanVien ?? "",
                            NhanVienId = string.IsNullOrEmpty(nhanVienId) ? null : nhanVienId,
                            Chi = soTien,
                            DienGiai = ghiChu ?? "Tạm ứng lương",
                            Note = ghiChu ?? ""
                        });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveTamUngLuong: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    await conn.ExecuteAsync("UPDATE TTHUCHI SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteTamUngLuong: " + ex.Message);
                return false;
            }
        }
    }
}
