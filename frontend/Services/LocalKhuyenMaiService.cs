using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class DotKhuyenMaiViewModel
    {
        public int Stt { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LoaiHinhKhuyenMaiId { get; set; } = string.Empty;
        public string TenLoaiHinhKhuyenMai { get; set; } = string.Empty;
        public DateTime? Tungay { get; set; }
        public DateTime? Denngay { get; set; }
        public bool IsNgungApDung { get; set; }
        public string Ngungapdung { get; set; } = "0";
        public string Note { get; set; } = string.Empty;
        public decimal? Tilegiamgia { get; set; }
        public decimal? Tilegiamgiatiengio { get; set; }
        public decimal? Khuyenmaigiohat { get; set; }
        public decimal? Tilegiamgiatong { get; set; }
        public DateTime? Tugio { get; set; }
        public DateTime? Dengio { get; set; }
        public decimal? Tilegiamgiagiodau { get; set; }
        public DateTime? Timecreated { get; set; }
        public DateTime? Timemodified { get; set; }
        public string NguoiTao { get; set; } = "Administrator";
        public string NguoiSua { get; set; } = "Administrator";
    }

    public static class LocalKhuyenMaiService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static int ParseInt(object val)
        {
            if (val == null || val == DBNull.Value) return 0;
            string s = val.ToString().Trim();
            if (int.TryParse(s, out int res)) return res;
            return 0;
        }

        private static decimal? ParseDecimal(object val)
        {
            if (val == null || val == DBNull.Value) return null;
            if (val is decimal d) return d;
            if (decimal.TryParse(val.ToString().Trim(), out decimal res)) return res;
            return null;
        }

        private static DateTime? ParseDateTime(object val)
        {
            if (val == null || val == DBNull.Value) return null;
            if (val is DateTime dt) return dt;
            if (DateTime.TryParse(val.ToString().Trim(), out DateTime res)) return res;
            return null;
        }

        public static async Task<ObservableCollection<NhomKhachHangTreeItem>> GetLoaiHinhKhuyenMaiTreeAsync()
        {
            var result = new ObservableCollection<NhomKhachHangTreeItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR, STATUS FROM DLOAIHINHKHUYENMAI WHERE STATUS = 30 OR STATUS > 0 ORDER BY SORTORDER, NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var rootItem = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        ItemType = 0,
                        Icon = "🌐",
                        IconColor = "#0078d7",
                        IsExpanded = true
                    };

                    rootItem.Children.Add(new NhomKhachHangTreeItem
                    {
                        Id = "UNASSIGNED",
                        Name = "Chưa thiết lập",
                        ItemType = 1,
                        Icon = "☀️",
                        IconColor = "#f0ad4e"
                    });

                    var folders = new Dictionary<string, NhomKhachHangTreeItem>();
                    var items = new List<dynamic>();

                    foreach (var r in rows)
                    {
                        int itemType = ParseInt(r.ITEMTYPE);
                        int parentDir = ParseInt(r.PARENTDIR);
                        string idStr = r.ID?.ToString()?.Trim() ?? "";

                        if (itemType == 1 || parentDir == 1)
                        {
                            var fNode = new NhomKhachHangTreeItem
                            {
                                Id = idStr,
                                Name = r.NAME?.ToString()?.Trim() ?? "",
                                ParentId = r.PARENTID?.ToString()?.Trim(),
                                ItemType = 2,
                                Icon = "📁",
                                IconColor = "#f0ad4e"
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
                        string idStr = it.ID?.ToString()?.Trim() ?? "";
                        string parentId = it.PARENTID?.ToString()?.Trim();
                        var itNode = new NhomKhachHangTreeItem
                        {
                            Id = idStr,
                            Name = it.NAME?.ToString()?.Trim() ?? "",
                            ParentId = parentId,
                            ItemType = 2,
                            Icon = "",
                            IconColor = "#333333"
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

                    var trashNode = new NhomKhachHangTreeItem
                    {
                        Id = "TRASH",
                        Name = "Thùng rác",
                        ItemType = 3,
                        Icon = "🗑️",
                        IconColor = "#888888"
                    };

                    string sqlTrash = "SELECT ID, NAME, PARENTID FROM DLOAIHINHKHUYENMAI WHERE STATUS <= 0 OR STATUS IS NULL ORDER BY NAME";
                    try
                    {
                        var trashRows = (await conn.QueryAsync(sqlTrash)).ToList();
                        foreach (var r in trashRows)
                        {
                            trashNode.Children.Add(new NhomKhachHangTreeItem
                            {
                                Id = r.ID?.ToString()?.Trim() ?? "",
                                Name = r.NAME?.ToString()?.Trim() ?? "",
                                ParentId = "TRASH",
                                ItemType = 2,
                                Icon = "📁",
                                IconColor = "#888888"
                            });
                        }
                    }
                    catch { }

                    result.Add(rootItem);
                    result.Add(trashNode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetLoaiHinhKhuyenMaiTreeAsync: " + ex.Message);
            }

            return result;
        }

        public static async Task<List<DotKhuyenMaiViewModel>> GetDotKhuyenMaiListAsync(string filterId, int itemType, string keyword)
        {
            var list = new List<DotKhuyenMaiViewModel>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT d.ID, d.NAME, d.NOTE, d.STATUS, d.TIMEMODIFIED, d.TIMECREATED,
                               d.DLOAIHINHKHUYENMAIID, d.TUNGAY, d.DENNGAY, d.NGUNGAPDUNG,
                               d.TILEGIAMGIA, d.TILEGIAMGIATIENGIO, d.KHUYENMAIGIOHAT,
                               d.TILEGIAMGIATONG, d.TUGIO, d.DENGIO, d.TILEGIAMGIAGIODAU,
                               l.NAME AS TENLOAIHINH
                        FROM DDOTKHUYENMAI d
                        LEFT JOIN DLOAIHINHKHUYENMAI l ON d.DLOAIHINHKHUYENMAIID = l.ID
                        WHERE 1=1";

                    if (filterId == "TRASH")
                    {
                        sql += " AND (d.STATUS <= 0 OR d.STATUS IS NULL)";
                    }
                    else
                    {
                        sql += " AND (d.STATUS = 30 OR d.STATUS > 0 OR d.STATUS IS NULL)";

                        if (filterId == "UNASSIGNED")
                        {
                            sql += " AND (d.DLOAIHINHKHUYENMAIID IS NULL OR TRIM(d.DLOAIHINHKHUYENMAIID) = '')";
                        }
                        else if (filterId != "ALL" && !string.IsNullOrEmpty(filterId))
                        {
                            string safeFilterId = filterId.Replace("'", "''");
                            sql += $" AND d.DLOAIHINHKHUYENMAIID = '{safeFilterId}'";
                        }
                    }

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        string safeKw = keyword.Replace("'", "''");
                        sql += $" AND (UPPER(d.NAME) LIKE UPPER('%{safeKw}%') OR UPPER(d.NOTE) LIKE UPPER('%{safeKw}%'))";
                    }

                    sql += " ORDER BY d.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();
                    int stt = 1;

                    foreach (var r in rows)
                    {
                        string ngungApDungStr = r.NGUNGAPDUNG?.ToString()?.Trim();
                        bool isNgung = ngungApDungStr == "1" || ngungApDungStr == "True" || ngungApDungStr == "T";

                        list.Add(new DotKhuyenMaiViewModel
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString()?.Trim() ?? "",
                            Name = r.NAME?.ToString()?.Trim() ?? "",
                            Note = r.NOTE?.ToString()?.Trim() ?? "",
                            LoaiHinhKhuyenMaiId = r.DLOAIHINHKHUYENMAIID?.ToString()?.Trim() ?? "",
                            TenLoaiHinhKhuyenMai = r.TENLOAIHINH?.ToString()?.Trim() ?? "",
                            Tungay = ParseDateTime(r.TUNGAY),
                            Denngay = ParseDateTime(r.DENNGAY),
                            IsNgungApDung = isNgung,
                            Ngungapdung = ngungApDungStr ?? "0",
                            Tilegiamgia = ParseDecimal(r.TILEGIAMGIA),
                            Tilegiamgiatiengio = ParseDecimal(r.TILEGIAMGIATIENGIO),
                            Khuyenmaigiohat = ParseDecimal(r.KHUYENMAIGIOHAT),
                            Tilegiamgiatong = ParseDecimal(r.TILEGIAMGIATONG),
                            Tugio = ParseDateTime(r.TUGIO),
                            Dengio = ParseDateTime(r.DENGIO),
                            Tilegiamgiagiodau = ParseDecimal(r.TILEGIAMGIAGIODAU),
                            Timecreated = ParseDateTime(r.TIMECREATED),
                            Timemodified = ParseDateTime(r.TIMEMODIFIED)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetDotKhuyenMaiListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<bool> DeleteDotKhuyenMaiAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DDOTKHUYENMAI SET STATUS = 0, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteDotKhuyenMaiAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreDotKhuyenMaiAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DDOTKHUYENMAI SET STATUS = 30, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreDotKhuyenMaiAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePermanentDotKhuyenMaiAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM DDOTKHUYENMAI WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePermanentDotKhuyenMaiAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> UpdateDotKhuyenMaiColumnAsync(List<string> ids, string columnName, object value)
        {
            if (ids == null || ids.Count == 0) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string inClause = string.Join(",", ids.Select(id => $"'{id.Replace("'", "''")}'"));
                    string valSql = value == null ? "NULL" : $"'{value.ToString().Replace("'", "''")}'";
                    string sql = $"UPDATE DDOTKHUYENMAI SET {columnName} = {valSql}, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID IN ({inClause})";
                    await conn.ExecuteAsync(sql);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error UpdateDotKhuyenMaiColumnAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreLoaiHinhKhuyenMaiAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DLOAIHINHKHUYENMAI SET STATUS = 30, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreLoaiHinhKhuyenMaiAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePermanentLoaiHinhKhuyenMaiAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM DLOAIHINHKHUYENMAI WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePermanentLoaiHinhKhuyenMaiAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<List<ActiveDotKhuyenMaiInfo>> GetActivePromotionsAsync(DateTime checkTime)
        {
            var result = new List<ActiveDotKhuyenMaiInfo>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    DateTime checkDate = checkTime.Date;
                    TimeSpan checkTod = checkTime.TimeOfDay;

                    string sqlMaster = @"
                        SELECT d.ID, d.NAME, d.NOTE, d.DLOAIHINHKHUYENMAIID,
                               d.TUNGAY, d.DENNGAY, d.TUGIO, d.DENGIO, d.NGUNGAPDUNG,
                               d.TILEGIAMGIA, d.TILEGIAMGIATONG,
                               l.NAME AS TENLOAIHINH
                        FROM DDOTKHUYENMAI d
                        LEFT JOIN DLOAIHINHKHUYENMAI l ON d.DLOAIHINHKHUYENMAIID = l.ID
                        WHERE (d.STATUS = 30 OR d.STATUS > 0 OR d.STATUS IS NULL)
                          AND (d.NGUNGAPDUNG IS NULL OR d.NGUNGAPDUNG = '0' OR d.NGUNGAPDUNG = 0)";

                    var masterRows = (await conn.QueryAsync(sqlMaster)).ToList();

                    foreach (var r in masterRows)
                    {
                        // Kiểm tra ngày áp dụng
                        DateTime? tuNgay = ParseDateTime(r.TUNGAY);
                        DateTime? denNgay = ParseDateTime(r.DENNGAY);
                        if (tuNgay.HasValue && checkDate < tuNgay.Value.Date) continue;
                        if (denNgay.HasValue && checkDate > denNgay.Value.Date) continue;

                        // Kiểm tra khung giờ (nếu có cấu hình)
                        if (r.TUGIO != null && r.DENGIO != null && !string.IsNullOrWhiteSpace(r.TUGIO.ToString()) && !string.IsNullOrWhiteSpace(r.DENGIO.ToString()))
                        {
                            DateTime? dtTu = ParseDateTime(r.TUGIO);
                            DateTime? dtDen = ParseDateTime(r.DENGIO);
                            if (dtTu.HasValue && dtDen.HasValue)
                            {
                                TimeSpan tu = dtTu.Value.TimeOfDay;
                                TimeSpan den = dtDen.Value.TimeOfDay;

                                if (tu <= den)
                                {
                                    if (checkTod < tu || checkTod > den) continue;
                                }
                                else
                                {
                                    // Qua đêm (VD: 22:00 -> 04:00)
                                    if (checkTod < tu && checkTod > den) continue;
                                }
                            }
                        }

                        string dotId = r.ID?.ToString()?.Trim();
                        var activeDot = new ActiveDotKhuyenMaiInfo
                        {
                            Id = dotId,
                            Name = r.NAME?.ToString()?.Trim() ?? "",
                            LoaiHinhName = r.TENLOAIHINH?.ToString()?.Trim() ?? "",
                            TileGiamGia = ParseDecimal(r.TILEGIAMGIA) ?? 0,
                            TileGiamGiaTong = ParseDecimal(r.TILEGIAMGIATONG) ?? 0
                        };

                        // Tải chi tiết khuyến mại
                        string sqlDetails = $@"
                            SELECT c.DMATHANGID, c.DNHOMMATHANGID, c.GIABAN, c.TILEGIAMGIA, 
                                   c.SOLUONGMUA, c.DMATHANGTANGID, c.SOLUONGTANG,
                                   mt.NAME AS TENHANGTANG, dt.NAME AS DVTTANG
                            FROM DDOTKHUYENMAICHITIET c
                            LEFT JOIN DMATHANG mt ON c.DMATHANGTANGID = mt.ID
                            LEFT JOIN DDONVITINH dt ON mt.DDONVITINHID = dt.ID
                            WHERE c.DDOTKHUYENMAIID = '{dotId.Replace("'", "''")}'
                              AND (c.STATUS = 30 OR c.STATUS > 0 OR c.STATUS IS NULL)";

                        var detailRows = (await conn.QueryAsync(sqlDetails)).ToList();
                        foreach (var d in detailRows)
                        {
                            activeDot.Details.Add(new ActiveKhuyenMaiChiTietInfo
                            {
                                MathangId = d.DMATHANGID?.ToString()?.Trim(),
                                NhomMathangId = d.DNHOMMATHANGID?.ToString()?.Trim(),
                                DonGiaGoc = ParseDecimal(d.GIABAN) ?? 0,
                                TileGiamGia = ParseDecimal(d.TILEGIAMGIA) ?? 0,
                                SoLuongMua = ParseDecimal(d.SOLUONGMUA) ?? 0,
                                MathangTangId = d.DMATHANGTANGID?.ToString()?.Trim(),
                                TenHangTang = d.TENHANGTANG?.ToString()?.Trim(),
                                DvtTang = d.DVTTANG?.ToString()?.Trim(),
                                SoLuongTang = ParseDecimal(d.SOLUONGTANG) ?? 0
                            });
                        }

                        result.Add(activeDot);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetActivePromotionsAsync: " + ex.Message);
            }

            return result;
        }
    }

    public class ActiveDotKhuyenMaiInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LoaiHinhName { get; set; }
        public decimal TileGiamGia { get; set; }
        public decimal TileGiamGiaTong { get; set; }
        public List<ActiveKhuyenMaiChiTietInfo> Details { get; set; } = new List<ActiveKhuyenMaiChiTietInfo>();
    }

    public class ActiveKhuyenMaiChiTietInfo
    {
        public string MathangId { get; set; }
        public string NhomMathangId { get; set; }
        public decimal DonGiaGoc { get; set; }
        public decimal TileGiamGia { get; set; }
        public decimal SoLuongMua { get; set; }
        public string MathangTangId { get; set; }
        public string TenHangTang { get; set; }
        public string DvtTang { get; set; }
        public decimal SoLuongTang { get; set; }
    }
}
