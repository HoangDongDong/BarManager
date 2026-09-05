using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class CaLamViecTreeItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; } = 30;
        public int? Sortorder { get; set; }
        public string ParentId { get; set; } = "";
        public string ItemType { get; set; } = "0"; // FOLDER, 0, SEPARATOR
        public string SimageId { get; set; } = "";
        public decimal TiLeLuong { get; set; } = 100;
        public string TuGio { get; set; } = "";
        public string DenGio { get; set; } = "";
        public ImageSource ImageSource { get; set; }

        public ObservableCollection<CaLamViecTreeItem> Children { get; set; } = new ObservableCollection<CaLamViecTreeItem>();

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        public string IconText
        {
            get
            {
                if (ItemType == "FOLDER" || (Children != null && Children.Count > 0))
                    return "📁";
                if (ItemType == "SEPARATOR")
                    return "➖";
                if (!string.IsNullOrEmpty(Name))
                {
                    string lower = Name.ToLower().Trim();
                    if (lower.Contains("sáng")) return "🌅";
                    if (lower.Contains("chiều")) return "🌇";
                    if (lower.Contains("tối") || lower.Contains("đêm")) return "🌙";
                    if (lower.Contains("gãy")) return "⚡";
                }
                return "🕒";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalCaLamViecService
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

        private static string FormatTimeString(object obj)
        {
            if (obj == null) return "";
            if (obj is DateTime dt)
            {
                return dt.ToString("HH:mm");
            }
            string str = obj.ToString().Trim();
            if (DateTime.TryParse(str, out var parsedDt))
            {
                return parsedDt.ToString("HH:mm");
            }
            return str;
        }

        private static CaLamViecTreeItem MapFromRow(object r)
        {
            var dict = r as IDictionary<string, object>;
            string id = GetValue(dict, "ID")?.ToString() ?? "";
            string name = GetValue(dict, "NAME")?.ToString() ?? "";
            string note = GetValue(dict, "NOTE")?.ToString() ?? "";
            string parentId = GetValue(dict, "PARENTID")?.ToString() ?? "";
            string itemType = GetValue(dict, "ITEMTYPE")?.ToString() ?? "0";
            string simageId = GetValue(dict, "SIMAGEID")?.ToString() ?? "";

            decimal tileluong = 100;
            var rawTl = GetValue(dict, "TILELUONG");
            if (rawTl != null && decimal.TryParse(rawTl.ToString(), out decimal tlVal)) tileluong = tlVal;

            string tuGio = FormatTimeString(GetValue(dict, "TUGIO"));
            string denGio = FormatTimeString(GetValue(dict, "DENGIO"));

            int? status = null;
            var rawStatus = GetValue(dict, "STATUS");
            if (rawStatus != null && int.TryParse(rawStatus.ToString(), out int statVal))
            {
                status = statVal;
            }

            int? sortOrder = null;
            var rawSort = GetValue(dict, "SORTORDER");
            if (rawSort != null && int.TryParse(rawSort.ToString(), out int sortVal))
            {
                sortOrder = sortVal;
            }

            return new CaLamViecTreeItem
            {
                Id = id,
                Name = name,
                Note = note,
                ParentId = parentId,
                ItemType = itemType,
                SimageId = simageId,
                TiLeLuong = tileluong,
                TuGio = tuGio,
                DenGio = denGio,
                Status = status,
                Sortorder = sortOrder
            };
        }

        public static async Task<ObservableCollection<CaLamViecTreeItem>> GetCaLamViecTreeAsync(bool isTrash = false)
        {
            var result = new ObservableCollection<CaLamViecTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var rows = (await conn.QueryAsync("SELECT * FROM DCALAMVIEC")).ToList();
                    var lookup = new Dictionary<string, CaLamViecTreeItem>(StringComparer.OrdinalIgnoreCase);
                    var allItems = new List<CaLamViecTreeItem>();

                    foreach (object r in rows)
                    {
                        CaLamViecTreeItem item = MapFromRow(r);

                        if (isTrash)
                        {
                            if ((item.Status ?? 1) != 0) continue;
                        }
                        else
                        {
                            if ((item.Status ?? 1) == 0) continue;
                        }

                        if (!string.IsNullOrEmpty(item.Id))
                        {
                            lookup[item.Id] = item;
                        }
                        allItems.Add(item);
                    }

                    allItems = allItems
                        .OrderBy(x => x.Sortorder ?? 9999)
                        .ThenBy(x => x.Name)
                        .ToList();

                    foreach (CaLamViecTreeItem item in allItems)
                    {
                        if (!string.IsNullOrEmpty(item.ParentId) && 
                            lookup.TryGetValue(item.ParentId, out var parent) && 
                            parent != item)
                        {
                            parent.Children.Add(item);
                        }
                        else
                        {
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCaLamViecTreeAsync: " + ex.Message);
            }
            return result;
        }

        public static async Task<List<CaLamViecTreeItem>> GetCaLamViecFlatListAsync(bool isTrash = false)
        {
            var list = new List<CaLamViecTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var rows = (await conn.QueryAsync("SELECT * FROM DCALAMVIEC")).ToList();
                    foreach (object r in rows)
                    {
                        CaLamViecTreeItem item = MapFromRow(r);
                        if (isTrash)
                        {
                            if ((item.Status ?? 1) != 0) continue;
                        }
                        else
                        {
                            if ((item.Status ?? 1) == 0) continue;
                        }
                        list.Add(item);
                    }

                    list = list.OrderBy(x => x.Sortorder ?? 9999).ThenBy(x => x.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCaLamViecFlatListAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<CaLamViecTreeItem> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var row = await conn.QueryFirstOrDefaultAsync("SELECT * FROM DCALAMVIEC WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    if (row != null) return MapFromRow(row);
                }
            }
            catch { }
            return null;
        }

        public static async Task<(bool Ok, string Error)> SaveCaLamViecAsync(CaLamViecTreeItem item)
        {
            if (item == null) return (false, "Dữ liệu rỗng");
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    DateTime? tuGioDt = null;
                    if (!string.IsNullOrWhiteSpace(item.TuGio))
                    {
                        if (DateTime.TryParseExact(item.TuGio.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt1))
                            tuGioDt = new DateTime(2000, 1, 1, dt1.Hour, dt1.Minute, 0);
                        else if (DateTime.TryParse(item.TuGio.Trim(), out var dtParsed1))
                            tuGioDt = new DateTime(2000, 1, 1, dtParsed1.Hour, dtParsed1.Minute, 0);
                    }

                    DateTime? denGioDt = null;
                    if (!string.IsNullOrWhiteSpace(item.DenGio))
                    {
                        if (DateTime.TryParseExact(item.DenGio.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
                            denGioDt = new DateTime(2000, 1, 1, dt2.Hour, dt2.Minute, 0);
                        else if (DateTime.TryParse(item.DenGio.Trim(), out var dtParsed2))
                            denGioDt = new DateTime(2000, 1, 1, dtParsed2.Hour, dtParsed2.Minute, 0);
                    }

                    object userId = 1;
                    try
                    {
                        var u = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                        if (u != null && u != DBNull.Value) userId = u;
                    }
                    catch { }

                    object parentIdParam = string.IsNullOrWhiteSpace(item.ParentId) ? null : item.ParentId.Trim();
                    object simageIdParam = string.IsNullOrWhiteSpace(item.SimageId) ? null : item.SimageId.Trim();

                    if (string.IsNullOrEmpty(item.Id))
                    {
                        string nextId = Guid.NewGuid().ToString();
                        item.Id = nextId;

                        string sqlInsert = @"
                            INSERT INTO DCALAMVIEC (
                                ID, NAME, NOTE, PARENTID, ITEMTYPE, SIMAGEID,
                                TILELUONG, TUGIO, DENGIO,
                                STATUS, SORTORDER, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @Name, @Note, @ParentId, @ItemType, @SimageId,
                                @TiLeLuong, @TuGio, @DenGio,
                                30, @Sortorder, @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        try
                        {
                            await conn.ExecuteAsync(sqlInsert, new
                            {
                                Id = nextId,
                                Name = item.Name?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                SimageId = simageIdParam,
                                TiLeLuong = item.TiLeLuong,
                                TuGio = tuGioDt,
                                DenGio = denGioDt,
                                Sortorder = item.Sortorder ?? 0,
                                UserCreatedId = userId
                            });
                        }
                        catch (Exception exInsert1)
                        {
                            Console.WriteLine("Insert try 1 failed: " + exInsert1.Message);
                            string sqlInsertFallback = @"
                                INSERT INTO DCALAMVIEC (
                                    ID, NAME, NOTE, PARENTID, ITEMTYPE,
                                    TILELUONG, STATUS, SORTORDER, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @Name, @Note, @ParentId, @ItemType,
                                    @TiLeLuong, 30, @Sortorder, @UserCreatedId, CURRENT_TIMESTAMP
                                )";

                            await conn.ExecuteAsync(sqlInsertFallback, new
                            {
                                Id = nextId,
                                Name = item.Name?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                TiLeLuong = item.TiLeLuong,
                                Sortorder = item.Sortorder ?? 0,
                                UserCreatedId = userId
                            });
                        }
                    }
                    else
                    {
                        string sqlUpdate = @"
                            UPDATE DCALAMVIEC SET
                                NAME = @Name,
                                NOTE = @Note,
                                PARENTID = @ParentId,
                                ITEMTYPE = @ItemType,
                                SIMAGEID = @SimageId,
                                TILELUONG = @TiLeLuong,
                                TUGIO = @TuGio,
                                DENGIO = @DenGio,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        try
                        {
                            await conn.ExecuteAsync(sqlUpdate, new
                            {
                                Id = item.Id,
                                Name = item.Name?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                SimageId = simageIdParam,
                                TiLeLuong = item.TiLeLuong,
                                TuGio = tuGioDt,
                                DenGio = denGioDt,
                                UserModifiedId = userId
                            });
                        }
                        catch (Exception exUpdate1)
                        {
                            Console.WriteLine("Update try 1 failed: " + exUpdate1.Message);
                            string sqlUpdateFallback = @"
                                UPDATE DCALAMVIEC SET
                                    NAME = @Name,
                                    NOTE = @Note,
                                    PARENTID = @ParentId,
                                    ITEMTYPE = @ItemType,
                                    TILELUONG = @TiLeLuong,
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE CAST(ID AS VARCHAR(50)) = @Id";

                            await conn.ExecuteAsync(sqlUpdateFallback, new
                            {
                                Id = item.Id,
                                Name = item.Name?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                TiLeLuong = item.TiLeLuong
                            });
                        }
                    }
                    return (true, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveCaLamViecAsync: " + ex.Message);
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteCaLamViecAsync(string id, bool permanent = false)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    if (permanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM DCALAMVIEC WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DCALAMVIEC SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteCaLamViecAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreCaLamViecAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    await conn.ExecuteAsync("UPDATE DCALAMVIEC SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreCaLamViecAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> AutoSortAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var rows = (await conn.QueryAsync("SELECT ID, NAME FROM DCALAMVIEC WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    int order = 1;
                    foreach (var r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(id))
                        {
                            await conn.ExecuteAsync("UPDATE DCALAMVIEC SET SORTORDER = @Order WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Order = order++, Id = id });
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error AutoSortAsync: " + ex.Message);
                return false;
            }
        }
    }
}
