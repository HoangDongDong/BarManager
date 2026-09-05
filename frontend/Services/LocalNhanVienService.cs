using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class NhanVienTreeItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Note { get; set; } = "";
        public string Dienthoai { get; set; } = "";
        public string Diachi { get; set; } = "";
        public int? Status { get; set; } = 30;
        public int? Sortorder { get; set; }
        public string ParentId { get; set; } = "";
        public string ItemType { get; set; } = ""; // FOLDER, ITEM, SEPARATOR
        public int CachTinhLuong { get; set; } = 0;
        public decimal LuongThang { get; set; } = 0;
        public decimal LuongCa { get; set; } = 0;
        public int NghiThu7 { get; set; } = 1;
        public int NghiChuNhat { get; set; } = 0;
        public string SimageId { get; set; } = "";
        public int? DcalamviecId { get; set; }

        public ObservableCollection<NhanVienTreeItem> Children { get; set; } = new ObservableCollection<NhanVienTreeItem>();

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
                    if (lower.Contains("hằng") || lower.Contains("sương") || lower.Contains("tuyết") || lower.Contains("nguyệt") || lower.Contains("hựu") || lower.Contains("đào") || lower.Contains("thị") || lower.Contains("hoa") || lower.Contains("mai") || lower.Contains("lan") || lower.Contains("linh") || lower.Contains("nga") || lower.Contains("minh"))
                        return "👩";
                }
                return "👤";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalNhanVienService
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

        private static NhanVienTreeItem MapFromRow(object r)
        {
            var dict = r as IDictionary<string, object>;
            string id = GetValue(dict, "ID")?.ToString() ?? "";
            string name = GetValue(dict, "NAME")?.ToString() ?? "";
            string code = GetValue(dict, "CODE")?.ToString() ?? "";
            string note = GetValue(dict, "NOTE")?.ToString() ?? "";
            string dienthoai = GetValue(dict, "DIENTHOAI")?.ToString() ?? "";
            string diachi = GetValue(dict, "DIACHI")?.ToString() ?? "";
            string parentId = GetValue(dict, "PARENTID")?.ToString() ?? (GetValue(dict, "PARENT_ID")?.ToString() ?? "");
            string itemType = GetValue(dict, "ITEMTYPE")?.ToString() ?? (GetValue(dict, "ITEM_TYPE")?.ToString() ?? "");
            string simageId = GetValue(dict, "SIMAGEID")?.ToString() ?? (GetValue(dict, "IMAGE")?.ToString() ?? "");

            int cachTinhLuong = 0;
            var rawCach = GetValue(dict, "CACHTINHLUONG");
            if (rawCach != null && int.TryParse(rawCach.ToString(), out int ctlVal)) cachTinhLuong = ctlVal;

            decimal luongCa = 0;
            var rawLc = GetValue(dict, "LUONGCA");
            if (rawLc != null && decimal.TryParse(rawLc.ToString(), out decimal lcVal)) luongCa = lcVal;

            decimal luongThang = 0;
            var rawLt = GetValue(dict, "LUONGTHANG");
            if (rawLt != null && decimal.TryParse(rawLt.ToString(), out decimal ltVal)) luongThang = ltVal;

            int nghiThu7 = 1;
            var rawT7 = GetValue(dict, "NGHITHU7");
            if (rawT7 != null && int.TryParse(rawT7.ToString(), out int t7Val)) nghiThu7 = t7Val;

            int nghiChuNhat = 0;
            var rawCn = GetValue(dict, "NGHICHUNHAT");
            if (rawCn != null && int.TryParse(rawCn.ToString(), out int cnVal)) nghiChuNhat = cnVal;

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

            return new NhanVienTreeItem
            {
                Id = id,
                Name = name,
                Code = code,
                Note = note,
                Dienthoai = dienthoai,
                Diachi = diachi,
                ParentId = parentId,
                ItemType = itemType,
                SimageId = simageId,
                CachTinhLuong = cachTinhLuong,
                LuongCa = luongCa,
                LuongThang = luongThang,
                NghiThu7 = nghiThu7,
                NghiChuNhat = nghiChuNhat,
                Status = status,
                Sortorder = sortOrder
            };
        }

        public static async Task<ObservableCollection<NhanVienTreeItem>> GetNhanVienTreeAsync(bool isTrash = false)
        {
            var result = new ObservableCollection<NhanVienTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var rows = (await conn.QueryAsync("SELECT * FROM DNHANVIEN")).ToList();
                    var lookup = new Dictionary<string, NhanVienTreeItem>(StringComparer.OrdinalIgnoreCase);
                    var allItems = new List<NhanVienTreeItem>();

                    foreach (object r in rows)
                    {
                        NhanVienTreeItem item = MapFromRow(r);

                        // Lọc theo thùng rác
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

                    foreach (NhanVienTreeItem item in allItems)
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
                Console.WriteLine("Error GetNhanVienTreeAsync: " + ex.Message);
            }
            return result;
        }

        public static async Task<List<NhanVienTreeItem>> GetNhanVienFlatListAsync(bool isTrash = false)
        {
            var list = new List<NhanVienTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var rows = (await conn.QueryAsync("SELECT * FROM DNHANVIEN")).ToList();
                    foreach (object r in rows)
                    {
                        NhanVienTreeItem item = MapFromRow(r);
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
                Console.WriteLine("Error GetNhanVienFlatListAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<NhanVienTreeItem> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var row = await conn.QueryFirstOrDefaultAsync("SELECT * FROM DNHANVIEN WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    if (row != null) return MapFromRow(row);
                }
            }
            catch { }
            return null;
        }

        public static async Task<bool> SaveNhanVienAsync(NhanVienTreeItem item)
        {
            if (item == null) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

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
                            INSERT INTO DNHANVIEN (
                                ID, NAME, CODE, NOTE, DIENTHOAI, DIACHI, PARENTID, ITEMTYPE, 
                                CACHTINHLUONG, LUONGCA, LUONGTHANG, NGHITHU7, NGHICHUNHAT, SIMAGEID,
                                STATUS, SORTORDER, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @Name, @Code, @Note, @Dienthoai, @Diachi, @ParentId, @ItemType, 
                                @CachTinhLuong, @LuongCa, @LuongThang, @NghiThu7, @NghiChuNhat, @SimageId,
                                30, @Sortorder, @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        try
                        {
                            await conn.ExecuteAsync(sqlInsert, new
                            {
                                Id = nextId,
                                Name = item.Name?.Trim() ?? "",
                                Code = item.Code?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                Dienthoai = item.Dienthoai?.Trim() ?? "",
                                Diachi = item.Diachi?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                CachTinhLuong = item.CachTinhLuong,
                                LuongCa = item.LuongCa,
                                LuongThang = item.LuongThang,
                                NghiThu7 = item.NghiThu7,
                                NghiChuNhat = item.NghiChuNhat,
                                SimageId = simageIdParam,
                                Sortorder = item.Sortorder ?? 0,
                                UserCreatedId = userId
                            });
                        }
                        catch (Exception exInsert)
                        {
                            Console.WriteLine("Insert DNHANVIEN try 1 failed: " + exInsert.Message);
                            // Fallback without simageId if type mismatch
                            string sqlInsertFallback = @"
                                INSERT INTO DNHANVIEN (
                                    ID, NAME, CODE, NOTE, DIENTHOAI, DIACHI, PARENTID, ITEMTYPE, 
                                    CACHTINHLUONG, LUONGCA, LUONGTHANG, NGHITHU7, NGHICHUNHAT,
                                    STATUS, SORTORDER, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @Name, @Code, @Note, @Dienthoai, @Diachi, @ParentId, @ItemType, 
                                    @CachTinhLuong, @LuongCa, @LuongThang, @NghiThu7, @NghiChuNhat,
                                    30, @Sortorder, @UserCreatedId, CURRENT_TIMESTAMP
                                )";

                            await conn.ExecuteAsync(sqlInsertFallback, new
                            {
                                Id = nextId,
                                Name = item.Name?.Trim() ?? "",
                                Code = item.Code?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                Dienthoai = item.Dienthoai?.Trim() ?? "",
                                Diachi = item.Diachi?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                CachTinhLuong = item.CachTinhLuong,
                                LuongCa = item.LuongCa,
                                LuongThang = item.LuongThang,
                                NghiThu7 = item.NghiThu7,
                                NghiChuNhat = item.NghiChuNhat,
                                Sortorder = item.Sortorder ?? 0,
                                UserCreatedId = userId
                            });
                        }
                    }
                    else
                    {
                        string sqlUpdate = @"
                            UPDATE DNHANVIEN SET
                                NAME = @Name,
                                CODE = @Code,
                                NOTE = @Note,
                                DIENTHOAI = @Dienthoai,
                                DIACHI = @Diachi,
                                PARENTID = @ParentId,
                                ITEMTYPE = @ItemType,
                                CACHTINHLUONG = @CachTinhLuong,
                                LUONGCA = @LuongCa,
                                LUONGTHANG = @LuongThang,
                                NGHITHU7 = @NghiThu7,
                                NGHICHUNHAT = @NghiChuNhat,
                                SIMAGEID = @SimageId,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        try
                        {
                            await conn.ExecuteAsync(sqlUpdate, new
                            {
                                Id = item.Id,
                                Name = item.Name?.Trim() ?? "",
                                Code = item.Code?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                Dienthoai = item.Dienthoai?.Trim() ?? "",
                                Diachi = item.Diachi?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                CachTinhLuong = item.CachTinhLuong,
                                LuongCa = item.LuongCa,
                                LuongThang = item.LuongThang,
                                NghiThu7 = item.NghiThu7,
                                NghiChuNhat = item.NghiChuNhat,
                                SimageId = simageIdParam,
                                UserModifiedId = userId
                            });
                        }
                        catch (Exception exUpdate)
                        {
                            Console.WriteLine("Update DNHANVIEN try 1 failed: " + exUpdate.Message);
                            string sqlUpdateFallback = @"
                                UPDATE DNHANVIEN SET
                                    NAME = @Name,
                                    CODE = @Code,
                                    NOTE = @Note,
                                    DIENTHOAI = @Dienthoai,
                                    DIACHI = @Diachi,
                                    PARENTID = @ParentId,
                                    ITEMTYPE = @ItemType,
                                    CACHTINHLUONG = @CachTinhLuong,
                                    LUONGCA = @LuongCa,
                                    LUONGTHANG = @LuongThang,
                                    NGHITHU7 = @NghiThu7,
                                    NGHICHUNHAT = @NghiChuNhat,
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE CAST(ID AS VARCHAR(50)) = @Id";

                            await conn.ExecuteAsync(sqlUpdateFallback, new
                            {
                                Id = item.Id,
                                Name = item.Name?.Trim() ?? "",
                                Code = item.Code?.Trim() ?? "",
                                Note = item.Note?.Trim() ?? "",
                                Dienthoai = item.Dienthoai?.Trim() ?? "",
                                Diachi = item.Diachi?.Trim() ?? "",
                                ParentId = parentIdParam,
                                ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                                CachTinhLuong = item.CachTinhLuong,
                                LuongCa = item.LuongCa,
                                LuongThang = item.LuongThang,
                                NghiThu7 = item.NghiThu7,
                                NghiChuNhat = item.NghiChuNhat
                            });
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveNhanVienAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteNhanVienAsync(string id, bool permanent = false)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    if (permanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM DNHANVIEN WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DNHANVIEN SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteNhanVienAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreNhanVienAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    await conn.ExecuteAsync("UPDATE DNHANVIEN SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id.Trim() });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreNhanVienAsync: " + ex.Message);
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
                    var rows = (await conn.QueryAsync("SELECT ID, NAME FROM DNHANVIEN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    int order = 1;
                    foreach (var r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(id))
                        {
                            await conn.ExecuteAsync("UPDATE DNHANVIEN SET SORTORDER = @Order WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Order = order++, Id = id });
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
