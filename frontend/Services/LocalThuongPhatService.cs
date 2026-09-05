using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;

namespace QuanLyBar.Client.Services
{
    public class LyDoThuongPhatTreeItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; } = 30;
        public int? Sortorder { get; set; }
        public string ParentId { get; set; } = "";
        public string ItemType { get; set; } = ""; // FOLDER, ITEM, SEPARATOR

        public ObservableCollection<LyDoThuongPhatTreeItem> Children { get; set; } = new ObservableCollection<LyDoThuongPhatTreeItem>();

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
                return "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class ThuongPhatItemViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string NgayStr => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
        public string DnhanvienId { get; set; } = "";
        public string TenNhanVien { get; set; } = "";
        public decimal? Thuong { get; set; }
        public string ThuongStr => (Thuong.HasValue && Thuong.Value > 0) ? Thuong.Value.ToString("N0") : "";
        public decimal? Phat { get; set; }
        public string PhatStr => (Phat.HasValue && Phat.Value > 0) ? Phat.Value.ToString("N0") : "";
        public string DlydothuongphatId { get; set; } = "";
        public string TenLyDo { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public int? Status { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalThuongPhatService
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

        private static async Task<int> GetNextDlyDoThuongPhatIdAsync(IDbConnection conn)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT ID FROM DLYDOTHUONGPHAT")).Cast<IDictionary<string, object>>().ToList();
                int maxId = 0;
                foreach (var dict in rows)
                {
                    object valObj = GetValue(dict, "ID");
                    if (valObj != null && !Convert.IsDBNull(valObj))
                    {
                        if (int.TryParse(valObj.ToString(), out int val) && val > maxId)
                        {
                            maxId = val;
                        }
                    }
                }
                return maxId + 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNextDlyDoThuongPhatId: " + ex.Message);
                return 100;
            }
        }

        private static async Task<int> GetNextTThuongPhatIdAsync(IDbConnection conn)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT ID FROM TTHUONGPHAT")).Cast<IDictionary<string, object>>().ToList();
                int maxId = 0;
                foreach (var dict in rows)
                {
                    object valObj = GetValue(dict, "ID");
                    if (valObj != null && !Convert.IsDBNull(valObj))
                    {
                        if (int.TryParse(valObj.ToString(), out int val) && val > maxId)
                        {
                            maxId = val;
                        }
                    }
                }
                return maxId + 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNextTThuongPhatId: " + ex.Message);
                return 100;
            }
        }

        public static async Task<List<LyDoThuongPhatTreeItem>> GetLyDoThuongPhatTreeAsync(bool isViewingTrash = false)
        {
            var rawList = new List<LyDoThuongPhatTreeItem>();
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                // Check and insert defaults if needed
                if (!isViewingTrash)
                {
                    try
                    {
                        await EnsureDefaultReasonsAsync();
                    }
                    catch { }
                }

                string query = "SELECT * FROM DLYDOTHUONGPHAT ORDER BY SORTORDER, NAME";
                var rows = (await conn.QueryAsync(query)).Cast<IDictionary<string, object>>().ToList();

                foreach (var r in rows)
                {
                    object rawStatus = GetValue(r, "STATUS");
                    bool isDeleted = false;
                    if (rawStatus != null && !Convert.IsDBNull(rawStatus))
                    {
                        if (rawStatus is bool b) isDeleted = !b;
                        else if (int.TryParse(rawStatus.ToString(), out int sVal)) isDeleted = (sVal == 0 || sVal < 0);
                        else if (bool.TryParse(rawStatus.ToString(), out bool b2)) isDeleted = !b2;
                    }

                    if (!isViewingTrash && isDeleted) continue;
                    if (isViewingTrash && !isDeleted) continue;

                    string id = GetValue(r, "ID")?.ToString() ?? "";
                    string name = GetValue(r, "NAME")?.ToString() ?? "";
                    string note = GetValue(r, "NOTE")?.ToString() ?? "";
                    string parentId = GetValue(r, "PARENTID")?.ToString() ?? GetValue(r, "PARENT_ID")?.ToString() ?? "";
                    string itemType = GetValue(r, "ITEMTYPE")?.ToString() ?? "";
                    int? sortOrder = Convert.IsDBNull(GetValue(r, "SORTORDER")) ? (int?)null : Convert.ToInt32(GetValue(r, "SORTORDER"));

                    rawList.Add(new LyDoThuongPhatTreeItem
                    {
                        Id = id,
                        Name = name,
                        Note = note,
                        ParentId = (parentId == "0" || parentId == "ALL" || parentId == "UNSET") ? "" : parentId,
                        ItemType = itemType,
                        Status = isDeleted ? 0 : 1,
                        Sortorder = sortOrder
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetLyDoThuongPhatTree: " + ex.Message);
            }

            var map = new Dictionary<string, LyDoThuongPhatTreeItem>();
            var rootNodes = new List<LyDoThuongPhatTreeItem>();

            foreach (var item in rawList)
            {
                if (!string.IsNullOrEmpty(item.Id))
                {
                    map[item.Id] = item;
                }
            }

            foreach (var item in rawList)
            {
                if (!string.IsNullOrEmpty(item.ParentId) && map.TryGetValue(item.ParentId, out var parentItem) && parentItem != item)
                {
                    parentItem.Children.Add(item);
                }
                else
                {
                    rootNodes.Add(item);
                }
            }

            return rootNodes;
        }

        public static async Task<List<LyDoThuongPhatTreeItem>> GetLyDoThuongPhatFlatListAsync()
        {
            var list = new List<LyDoThuongPhatTreeItem>();
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                // Check defaults
                try
                {
                    await EnsureDefaultReasonsAsync();
                }
                catch { }

                string query = "SELECT * FROM DLYDOTHUONGPHAT ORDER BY SORTORDER, NAME";
                var rows = (await conn.QueryAsync(query)).Cast<IDictionary<string, object>>().ToList();

                foreach (var r in rows)
                {
                    string itemType = GetValue(r, "ITEMTYPE")?.ToString() ?? "";
                    if (itemType == "SEPARATOR") continue;

                    object rawStatus = GetValue(r, "STATUS");
                    bool isDeleted = false;
                    if (rawStatus != null && !Convert.IsDBNull(rawStatus))
                    {
                        if (rawStatus is bool b) isDeleted = !b;
                        else if (int.TryParse(rawStatus.ToString(), out int sVal)) isDeleted = (sVal == 0 || sVal < 0);
                        else if (bool.TryParse(rawStatus.ToString(), out bool b2)) isDeleted = !b2;
                    }
                    if (isDeleted) continue;

                    string parentId = GetValue(r, "PARENTID")?.ToString() ?? GetValue(r, "PARENT_ID")?.ToString() ?? "";

                    list.Add(new LyDoThuongPhatTreeItem
                    {
                        Id = GetValue(r, "ID")?.ToString() ?? "",
                        Name = GetValue(r, "NAME")?.ToString() ?? "",
                        Note = GetValue(r, "NOTE")?.ToString() ?? "",
                        ParentId = (parentId == "0" || parentId == "ALL" || parentId == "UNSET") ? "" : parentId,
                        ItemType = itemType,
                        Status = isDeleted ? 0 : 1,
                        Sortorder = Convert.IsDBNull(GetValue(r, "SORTORDER")) ? (int?)null : Convert.ToInt32(GetValue(r, "SORTORDER"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetLyDoThuongPhatFlatList: " + ex.Message);
            }
            return list;
        }

        public static async Task EnsureDefaultReasonsAsync()
        {
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                var rows = (await conn.QueryAsync("SELECT * FROM DLYDOTHUONGPHAT")).Cast<IDictionary<string, object>>().ToList();
                var defaultReasons = new List<(string Name, string Note, int Sort)>
                {
                    ("Doanh số", "Thưởng đạt doanh số", 1),
                    ("Tính sai cho khách", "Phạt do tính sai tiền", 2),
                    ("Chuyên cần / Đi làm đúng giờ", "Thưởng chuyên cần", 3),
                    ("Đi muộn / Về sớm", "Phạt vi phạm giờ giấc", 4)
                };

                var activeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int currentMaxId = 0;

                foreach (var r in rows)
                {
                    string idStr = GetValue(r, "ID")?.ToString();
                    if (int.TryParse(idStr, out int idVal) && idVal > currentMaxId)
                    {
                        currentMaxId = idVal;
                    }

                    string name = GetValue(r, "NAME")?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    object rawStatus = GetValue(r, "STATUS");
                    bool isDeleted = false;
                    if (rawStatus != null && !Convert.IsDBNull(rawStatus))
                    {
                        if (rawStatus is bool b) isDeleted = !b;
                        else if (int.TryParse(rawStatus.ToString(), out int sVal)) isDeleted = (sVal == 0 || sVal < 0);
                        else if (bool.TryParse(rawStatus.ToString(), out bool b2)) isDeleted = !b2;
                    }

                    if (isDeleted)
                    {
                        // Check if it matches any default reason -> re-activate it
                        if (defaultReasons.Any(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase)))
                        {
                            try
                            {
                                await conn.ExecuteAsync("UPDATE DLYDOTHUONGPHAT SET STATUS = 1 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = idStr });
                                activeNames.Add(name);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        activeNames.Add(name);
                    }
                }

                foreach (var def in defaultReasons)
                {
                    if (!activeNames.Contains(def.Name))
                    {
                        currentMaxId++;
                        int newId = currentMaxId;

                        try
                        {
                            string sql = @"
                                INSERT INTO DLYDOTHUONGPHAT (ID, NAME, NOTE, STATUS, SORTORDER, ITEMTYPE, USERCREATEDID, TIMECREATED)
                                VALUES (@ID, @NAME, @NOTE, 1, @SORTORDER, '0', 1, CURRENT_TIMESTAMP)";
                            await conn.ExecuteAsync(sql, new
                            {
                                ID = newId,
                                NAME = def.Name,
                                NOTE = def.Note,
                                SORTORDER = def.Sort
                            });
                        }
                        catch
                        {
                            try
                            {
                                string sql = @"
                                    INSERT INTO DLYDOTHUONGPHAT (ID, NAME, NOTE, STATUS, SORTORDER, USERCREATEDID)
                                    VALUES (@ID, @NAME, @NOTE, 1, @SORTORDER, 1)";
                                await conn.ExecuteAsync(sql, new
                                {
                                    ID = newId,
                                    NAME = def.Name,
                                    NOTE = def.Note,
                                    SORTORDER = def.Sort
                                });
                            }
                            catch
                            {
                                string sql = @"INSERT INTO DLYDOTHUONGPHAT (ID, NAME, NOTE, STATUS, USERCREATEDID) VALUES (@ID, @NAME, @NOTE, 1, 1)";
                                await conn.ExecuteAsync(sql, new
                                {
                                    ID = newId,
                                    NAME = def.Name,
                                    NOTE = def.Note
                                });
                            }
                        }
                        activeNames.Add(def.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error EnsureDefaultReasons: " + ex.Message);
            }
        }

        public static async Task<(bool success, string error)> SaveLyDoThuongPhatAsync(LyDoThuongPhatTreeItem item)
        {
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                object parentVal = DBNull.Value;
                if (!string.IsNullOrEmpty(item.ParentId) && int.TryParse(item.ParentId, out int pInt) && pInt > 0)
                {
                    parentVal = pInt;
                }

                if (string.IsNullOrEmpty(item.Id))
                {
                    int nextId = await GetNextDlyDoThuongPhatIdAsync(conn);
                    item.Id = nextId.ToString();

                    Exception lastEx = null;
                    // Attempt 1: Full insert with TIMECREATED & USERCREATEDID
                    try
                    {
                        string q = @"INSERT INTO DLYDOTHUONGPHAT 
                                    (ID, NAME, NOTE, PARENTID, ITEMTYPE, STATUS, SORTORDER, USERCREATEDID, TIMECREATED) 
                                    VALUES (@Id, @Name, @Note, @ParentId, @ItemType, 1, @Sortorder, 1, CURRENT_TIMESTAMP)";
                        await conn.ExecuteAsync(q, new
                        {
                            Id = nextId,
                            Name = item.Name ?? "",
                            Note = item.Note ?? "",
                            ParentId = parentVal,
                            ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                            Sortorder = item.Sortorder ?? nextId
                        });
                        return (true, null);
                    }
                    catch (Exception ex1) { lastEx = ex1; }

                    // Attempt 2: Without TIMECREATED (always keep USERCREATEDID = 1)
                    try
                    {
                        string q = @"INSERT INTO DLYDOTHUONGPHAT 
                                    (ID, NAME, NOTE, PARENTID, ITEMTYPE, STATUS, SORTORDER, USERCREATEDID) 
                                    VALUES (@Id, @Name, @Note, @ParentId, @ItemType, 1, @Sortorder, 1)";
                        await conn.ExecuteAsync(q, new
                        {
                            Id = nextId,
                            Name = item.Name ?? "",
                            Note = item.Note ?? "",
                            ParentId = parentVal,
                            ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                            Sortorder = item.Sortorder ?? nextId
                        });
                        return (true, null);
                    }
                    catch (Exception ex2) { lastEx = ex2; }

                    // Attempt 3: Without PARENTID & ITEMTYPE
                    try
                    {
                        string q = @"INSERT INTO DLYDOTHUONGPHAT (ID, NAME, NOTE, STATUS, SORTORDER, USERCREATEDID) 
                                    VALUES (@Id, @Name, @Note, 1, @Sortorder, 1)";
                        await conn.ExecuteAsync(q, new
                        {
                            Id = nextId,
                            Name = item.Name ?? "",
                            Note = item.Note ?? "",
                            Sortorder = item.Sortorder ?? nextId
                        });
                        return (true, null);
                    }
                    catch (Exception ex3) { lastEx = ex3; }

                    // Attempt 4: Minimal insert (ID, NAME, NOTE, STATUS, USERCREATEDID)
                    try
                    {
                        string q = @"INSERT INTO DLYDOTHUONGPHAT (ID, NAME, NOTE, STATUS, USERCREATEDID) VALUES (@Id, @Name, @Note, 1, 1)";
                        await conn.ExecuteAsync(q, new
                        {
                            Id = nextId,
                            Name = item.Name ?? "",
                            Note = item.Note ?? ""
                        });
                        return (true, null);
                    }
                    catch (Exception ex4) { lastEx = ex4; }

                    return (false, lastEx?.Message ?? "Không thể thêm lý do thưởng phạt");
                }
                else
                {
                    Exception lastEx = null;
                    // Attempt 1: Full update with TIMEMODIFIED
                    try
                    {
                        string q = @"UPDATE DLYDOTHUONGPHAT SET 
                                    NAME = @Name, NOTE = @Note, PARENTID = @ParentId, ITEMTYPE = @ItemType, 
                                    STATUS = 1, SORTORDER = @Sortorder, USERMODIFIEDID = 1, TIMEMODIFIED = CURRENT_TIMESTAMP 
                                    WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                        await conn.ExecuteAsync(q, new
                        {
                            IdStr = item.Id,
                            Name = item.Name ?? "",
                            Note = item.Note ?? "",
                            ParentId = parentVal,
                            ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                            Sortorder = item.Sortorder ?? 0
                        });
                        return (true, null);
                    }
                    catch (Exception ex1) { lastEx = ex1; }

                    // Attempt 2: Without USERMODIFIEDID / TIMEMODIFIED
                    try
                    {
                        string q = @"UPDATE DLYDOTHUONGPHAT SET 
                                    NAME = @Name, NOTE = @Note, PARENTID = @ParentId, ITEMTYPE = @ItemType, 
                                    STATUS = 1, SORTORDER = @Sortorder 
                                    WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                        await conn.ExecuteAsync(q, new
                        {
                            IdStr = item.Id,
                            Name = item.Name ?? "",
                            Note = item.Note ?? "",
                            ParentId = parentVal,
                            ItemType = string.IsNullOrEmpty(item.ItemType) ? "0" : item.ItemType,
                            Sortorder = item.Sortorder ?? 0
                        });
                        return (true, null);
                    }
                    catch (Exception ex2) { lastEx = ex2; }

                    // Attempt 3: Minimal update (NAME, NOTE)
                    try
                    {
                        string q = @"UPDATE DLYDOTHUONGPHAT SET NAME = @Name, NOTE = @Note WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                        await conn.ExecuteAsync(q, new
                        {
                            IdStr = item.Id,
                            Name = item.Name ?? "",
                            Note = item.Note ?? ""
                        });
                        return (true, null);
                    }
                    catch (Exception ex3) { lastEx = ex3; }

                    return (false, lastEx?.Message ?? "Không thể cập nhật lý do thưởng phạt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveLyDoThuongPhat: " + ex.Message);
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteLyDoThuongPhatAsync(string id, bool permanent = false)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                if (permanent)
                {
                    await conn.ExecuteAsync("DELETE FROM DLYDOTHUONGPHAT WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                }
                else
                {
                    await conn.ExecuteAsync("UPDATE DLYDOTHUONGPHAT SET STATUS = -1 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteLyDoThuongPhat: " + ex.Message);
                return false;
            }
        }

        public static async Task<List<ThuongPhatItemViewModel>> GetThuongPhatListAsync(
            string nhanVienId = null,
            string lyDoId = null,
            string searchKeyword = null,
            bool isTrash = false)
        {
            var result = new List<ThuongPhatItemViewModel>();
            try
            {
                using var conn = GetConnection();
                conn.Open();

                // Build lookup caches for fallback in case SQL LEFT JOIN fails on mismatched types
                var nvList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                var nvMapById = nvList.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);
                var ldList = await GetLyDoThuongPhatFlatListAsync();
                var ldMapById = ldList.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);

                string query = @"
                    SELECT 
                        tp.ID,
                        tp.NAME AS SOPHIEU,
                        tp.NGAY,
                        tp.DNHANVIENID,
                        nv.NAME AS TENNHANVIEN,
                        tp.THUONG,
                        tp.PHAT,
                        tp.DLYDOTHUONGPHATID,
                        ld.NAME AS TENLYDO,
                        tp.NOTE AS GHICHU,
                        tp.STATUS
                    FROM TTHUONGPHAT tp
                    LEFT JOIN DNHANVIEN nv ON CAST(tp.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN DLYDOTHUONGPHAT ld ON CAST(tp.DLYDOTHUONGPHATID AS VARCHAR(50)) = CAST(ld.ID AS VARCHAR(50))
                    WHERE 1=1
                ";

                if (isTrash)
                {
                    query += " AND tp.STATUS < 0";
                }
                else
                {
                    query += " AND (tp.STATUS IS NULL OR tp.STATUS >= 0)";
                }

                if (!string.IsNullOrEmpty(nhanVienId))
                {
                    if (nhanVienId == "UNSET")
                    {
                        query += " AND (tp.DNHANVIENID IS NULL OR CAST(tp.DNHANVIENID AS VARCHAR(50)) = '')";
                    }
                    else if (nhanVienId != "ALL" && nhanVienId != "TRASH")
                    {
                        query += " AND CAST(tp.DNHANVIENID AS VARCHAR(50)) = @NhanVienId";
                    }
                }

                if (!string.IsNullOrEmpty(lyDoId))
                {
                    if (lyDoId == "UNSET")
                    {
                        query += " AND (tp.DLYDOTHUONGPHATID IS NULL OR CAST(tp.DLYDOTHUONGPHATID AS VARCHAR(50)) = '')";
                    }
                    else if (lyDoId != "ALL" && lyDoId != "TRASH")
                    {
                        query += " AND CAST(tp.DLYDOTHUONGPHATID AS VARCHAR(50)) = @LyDoId";
                    }
                }

                query += " ORDER BY tp.NGAY DESC, tp.NAME DESC";

                var rows = (await conn.QueryAsync(query, new { NhanVienId = nhanVienId, LyDoId = lyDoId })).Cast<IDictionary<string, object>>().ToList();

                foreach (var r in rows)
                {
                    string id = GetValue(r, "ID")?.ToString() ?? "";
                    string soPhieu = GetValue(r, "SOPHIEU")?.ToString() ?? "";
                    DateTime? ngay = Convert.IsDBNull(GetValue(r, "NGAY")) ? (DateTime?)null : Convert.ToDateTime(GetValue(r, "NGAY"));
                    string nvId = GetValue(r, "DNHANVIENID")?.ToString() ?? "";
                    string tenNv = GetValue(r, "TENNHANVIEN")?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(tenNv) && !string.IsNullOrEmpty(nvId) && nvMapById.TryGetValue(nvId, out var mappedNvName))
                    {
                        tenNv = mappedNvName;
                    }

                    decimal? thuong = null;
                    object rawThuong = GetValue(r, "THUONG");
                    if (rawThuong != null && !Convert.IsDBNull(rawThuong) && decimal.TryParse(rawThuong.ToString(), out decimal tVal))
                    {
                        thuong = tVal;
                    }

                    decimal? phat = null;
                    object rawPhat = GetValue(r, "PHAT");
                    if (rawPhat != null && !Convert.IsDBNull(rawPhat) && decimal.TryParse(rawPhat.ToString(), out decimal pVal))
                    {
                        phat = pVal;
                    }

                    string ldId = GetValue(r, "DLYDOTHUONGPHATID")?.ToString() ?? "";
                    string tenLd = GetValue(r, "TENLYDO")?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(tenLd) && !string.IsNullOrEmpty(ldId) && ldMapById.TryGetValue(ldId, out var mappedLdName))
                    {
                        tenLd = mappedLdName;
                    }

                    string ghiChu = GetValue(r, "GHICHU")?.ToString() ?? "";
                    int? status = Convert.IsDBNull(GetValue(r, "STATUS")) ? (int?)null : Convert.ToInt32(GetValue(r, "STATUS"));

                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        string kw = searchKeyword.Trim().ToLower();
                        bool match = soPhieu.ToLower().Contains(kw) ||
                                     tenNv.ToLower().Contains(kw) ||
                                     tenLd.ToLower().Contains(kw) ||
                                     ghiChu.ToLower().Contains(kw);
                        if (!match) continue;
                    }

                    result.Add(new ThuongPhatItemViewModel
                    {
                        Id = id,
                        SoPhieu = soPhieu,
                        Ngay = ngay,
                        DnhanvienId = nvId,
                        TenNhanVien = tenNv,
                        Thuong = thuong,
                        Phat = phat,
                        DlydothuongphatId = ldId,
                        TenLyDo = tenLd,
                        GhiChu = ghiChu,
                        Status = status
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetThuongPhatList: " + ex.Message);
            }
            return result;
        }

        public static async Task<ThuongPhatItemViewModel> GetThuongPhatByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            try
            {
                using var conn = GetConnection();
                conn.Open();

                var nvList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                var nvMapById = nvList.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);
                var ldList = await GetLyDoThuongPhatFlatListAsync();
                var ldMapById = ldList.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);

                string query = @"
                    SELECT 
                        tp.ID,
                        tp.NAME AS SOPHIEU,
                        tp.NGAY,
                        tp.DNHANVIENID,
                        nv.NAME AS TENNHANVIEN,
                        tp.THUONG,
                        tp.PHAT,
                        tp.DLYDOTHUONGPHATID,
                        ld.NAME AS TENLYDO,
                        tp.NOTE AS GHICHU,
                        tp.STATUS
                    FROM TTHUONGPHAT tp
                    LEFT JOIN DNHANVIEN nv ON CAST(tp.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN DLYDOTHUONGPHAT ld ON CAST(tp.DLYDOTHUONGPHATID AS VARCHAR(50)) = CAST(ld.ID AS VARCHAR(50))
                    WHERE tp.ID = @Id
                ";

                var r = (await conn.QueryFirstOrDefaultAsync(query, new { Id = id })) as IDictionary<string, object>;
                if (r != null)
                {
                    decimal? thuong = null;
                    object rawThuong = GetValue(r, "THUONG");
                    if (rawThuong != null && !Convert.IsDBNull(rawThuong) && decimal.TryParse(rawThuong.ToString(), out decimal tVal))
                        thuong = tVal;

                    decimal? phat = null;
                    object rawPhat = GetValue(r, "PHAT");
                    if (rawPhat != null && !Convert.IsDBNull(rawPhat) && decimal.TryParse(rawPhat.ToString(), out decimal pVal))
                        phat = pVal;

                    string nvId = GetValue(r, "DNHANVIENID")?.ToString() ?? "";
                    string tenNv = GetValue(r, "TENNHANVIEN")?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(tenNv) && !string.IsNullOrEmpty(nvId) && nvMapById.TryGetValue(nvId, out var mappedNvName))
                    {
                        tenNv = mappedNvName;
                    }

                    string ldId = GetValue(r, "DLYDOTHUONGPHATID")?.ToString() ?? "";
                    string tenLd = GetValue(r, "TENLYDO")?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(tenLd) && !string.IsNullOrEmpty(ldId) && ldMapById.TryGetValue(ldId, out var mappedLdName))
                    {
                        tenLd = mappedLdName;
                    }

                    return new ThuongPhatItemViewModel
                    {
                        Id = GetValue(r, "ID")?.ToString() ?? "",
                        SoPhieu = GetValue(r, "SOPHIEU")?.ToString() ?? "",
                        Ngay = Convert.IsDBNull(GetValue(r, "NGAY")) ? (DateTime?)null : Convert.ToDateTime(GetValue(r, "NGAY")),
                        DnhanvienId = nvId,
                        TenNhanVien = tenNv,
                        Thuong = thuong,
                        Phat = phat,
                        DlydothuongphatId = ldId,
                        TenLyDo = tenLd,
                        GhiChu = GetValue(r, "GHICHU")?.ToString() ?? "",
                        Status = Convert.IsDBNull(GetValue(r, "STATUS")) ? (int?)null : Convert.ToInt32(GetValue(r, "STATUS"))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetThuongPhatById: " + ex.Message);
            }
            return null;
        }

        public static async Task<string> GetNextSoPhieuAsync()
        {
            string yearSuffix = DateTime.Now.ToString("yy");
            string prefix = $"TP{yearSuffix}/";
            try
            {
                using var conn = GetConnection();
                conn.Open();

                string query = "SELECT NAME FROM TTHUONGPHAT WHERE NAME LIKE @Prefix ORDER BY NAME DESC";
                var list = (await conn.QueryAsync<string>(query, new { Prefix = prefix + "%" })).ToList();

                int maxNum = 0;
                foreach (var name in list)
                {
                    if (!string.IsNullOrEmpty(name) && name.Length > prefix.Length)
                    {
                        string part = name.Substring(prefix.Length);
                        if (int.TryParse(part, out int num) && num > maxNum)
                        {
                            maxNum = num;
                        }
                    }
                }
                return $"{prefix}{(maxNum + 1):D5}";
            }
            catch
            {
                return $"{prefix}00001";
            }
        }

        public static async Task<(bool success, string error, string savedId)> SaveThuongPhatAsync(
            string id,
            string soPhieu,
            DateTime ngay,
            string nhanVienId,
            decimal? thuong,
            decimal? phat,
            string lyDoId,
            string ghiChu)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                object nvVal = DBNull.Value;
                if (!string.IsNullOrEmpty(nhanVienId))
                {
                    if (int.TryParse(nhanVienId, out int nvInt))
                    {
                        nvVal = nvInt;
                    }
                    else
                    {
                        try
                        {
                            var foundId = await conn.ExecuteScalarAsync<object>(
                                "SELECT FIRST 1 ID FROM DNHANVIEN WHERE CAST(ID AS VARCHAR(50)) = @Val OR NAME = @Val",
                                new { Val = nhanVienId });
                            if (foundId != null && foundId != DBNull.Value)
                                nvVal = foundId;
                            else
                                nvVal = nhanVienId;
                        }
                        catch
                        {
                            nvVal = nhanVienId;
                        }
                    }
                }

                object ldVal = DBNull.Value;
                if (!string.IsNullOrEmpty(lyDoId))
                {
                    if (int.TryParse(lyDoId, out int ldInt))
                    {
                        ldVal = ldInt;
                    }
                    else
                    {
                        try
                        {
                            var foundLdId = await conn.ExecuteScalarAsync<object>(
                                "SELECT FIRST 1 ID FROM DLYDOTHUONGPHAT WHERE CAST(ID AS VARCHAR(50)) = @Val OR NAME = @Val",
                                new { Val = lyDoId });
                            if (foundLdId != null && foundLdId != DBNull.Value)
                                ldVal = foundLdId;
                            else
                                ldVal = lyDoId;
                        }
                        catch
                        {
                            ldVal = lyDoId;
                        }
                    }
                }

                if (string.IsNullOrEmpty(id))
                {
                    int nextId = await GetNextTThuongPhatIdAsync(conn);
                    string newIdStr = nextId.ToString();

                    try
                    {
                        string insertSql = @"
                            INSERT INTO TTHUONGPHAT 
                            (ID, NAME, NGAY, DNHANVIENID, THUONG, PHAT, DLYDOTHUONGPHATID, NOTE, STATUS, USERCREATEDID, TIMECREATED)
                            VALUES 
                            (@Id, @Name, @Ngay, @DnhanvienId, @Thuong, @Phat, @DlydothuongphatId, @Note, @Status, @UserCreatedId, CURRENT_TIMESTAMP)
                        ";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = nextId,
                            Name = soPhieu,
                            Ngay = ngay,
                            DnhanvienId = nvVal,
                            Thuong = thuong ?? 0,
                            Phat = (phat.HasValue && phat.Value > 0) ? phat.Value.ToString() : "0",
                            DlydothuongphatId = ldVal,
                            Note = ghiChu ?? "",
                            Status = 30,
                            UserCreatedId = 1
                        });
                    }
                    catch
                    {
                        string insertSql = @"
                            INSERT INTO TTHUONGPHAT 
                            (ID, NAME, NGAY, DNHANVIENID, THUONG, PHAT, DLYDOTHUONGPHATID, NOTE, STATUS)
                            VALUES 
                            (@Id, @Name, @Ngay, @DnhanvienId, @Thuong, @Phat, @DlydothuongphatId, @Note, @Status)
                        ";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = nextId,
                            Name = soPhieu,
                            Ngay = ngay,
                            DnhanvienId = nvVal,
                            Thuong = thuong ?? 0,
                            Phat = (phat.HasValue && phat.Value > 0) ? phat.Value.ToString() : "0",
                            DlydothuongphatId = ldVal,
                            Note = ghiChu ?? "",
                            Status = 30
                        });
                    }

                    return (true, null, newIdStr);
                }
                else
                {
                    try
                    {
                        string updateSql = @"
                            UPDATE TTHUONGPHAT SET 
                                NAME = @Name,
                                NGAY = @Ngay,
                                DNHANVIENID = @DnhanvienId,
                                THUONG = @Thuong,
                                PHAT = @Phat,
                                DLYDOTHUONGPHATID = @DlydothuongphatId,
                                NOTE = @Note,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @IdStr
                        ";

                        await conn.ExecuteAsync(updateSql, new
                        {
                            IdStr = id,
                            Name = soPhieu,
                            Ngay = ngay,
                            DnhanvienId = nvVal,
                            Thuong = thuong ?? 0,
                            Phat = (phat.HasValue && phat.Value > 0) ? phat.Value.ToString() : "0",
                            DlydothuongphatId = ldVal,
                            Note = ghiChu ?? "",
                            UserModifiedId = 1
                        });
                    }
                    catch
                    {
                        string updateSql = @"
                            UPDATE TTHUONGPHAT SET 
                                NAME = @Name,
                                NGAY = @Ngay,
                                DNHANVIENID = @DnhanvienId,
                                THUONG = @Thuong,
                                PHAT = @Phat,
                                DLYDOTHUONGPHATID = @DlydothuongphatId,
                                NOTE = @Note
                            WHERE CAST(ID AS VARCHAR(50)) = @IdStr
                        ";

                        await conn.ExecuteAsync(updateSql, new
                        {
                            IdStr = id,
                            Name = soPhieu,
                            Ngay = ngay,
                            DnhanvienId = nvVal,
                            Thuong = thuong ?? 0,
                            Phat = (phat.HasValue && phat.Value > 0) ? phat.Value.ToString() : "0",
                            DlydothuongphatId = ldVal,
                            Note = ghiChu ?? ""
                        });
                    }

                    return (true, null, id);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<bool> DeleteThuongPhatAsync(string id, bool permanent = false)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                if (permanent)
                {
                    await conn.ExecuteAsync("DELETE FROM TTHUONGPHAT WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                }
                else
                {
                    await conn.ExecuteAsync("UPDATE TTHUONGPHAT SET STATUS = -1 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteThuongPhat: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreThuongPhatAsync(string id)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                await conn.ExecuteAsync("UPDATE TTHUONGPHAT SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreThuongPhat: " + ex.Message);
                return false;
            }
        }
    }
}
