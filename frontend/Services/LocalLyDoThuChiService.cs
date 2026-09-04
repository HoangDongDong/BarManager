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
    public class LyDoThuChiTreeItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal? Lalydothu { get; set; } // 0: Chi, > 0: Thu
        public string Loailydo { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; }
        public int? Sortorder { get; set; }
        public string ParentId { get; set; } = "";
        public string ItemType { get; set; } = ""; // FOLDER, ITEM, SEPARATOR

        public ObservableCollection<LyDoThuChiTreeItem> Children { get; set; } = new ObservableCollection<LyDoThuChiTreeItem>();

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
                    if (lower == "chi lương nhân viên" || lower == "tạm ứng" || lower.Contains("lương nhân viên"))
                        return "👤";
                    if (lower.Contains("đặt trước tiền mua hàng") || lower.Contains("thu đặt trước"))
                        return "🏷️";
                }
                return "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalLyDoThuChiService
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

        private static LyDoThuChiTreeItem MapFromRow(object r)
        {
            var dict = r as IDictionary<string, object>;
            
            string id = GetValue(dict, "ID")?.ToString() ?? "";
            string name = GetValue(dict, "NAME")?.ToString() ?? "";
            string note = GetValue(dict, "NOTE")?.ToString() ?? "";
            string loailydo = GetValue(dict, "LOAILYDO")?.ToString() ?? "";
            string parentId = GetValue(dict, "PARENTID")?.ToString() ?? (GetValue(dict, "PARENT_ID")?.ToString() ?? "");
            string itemType = GetValue(dict, "ITEMTYPE")?.ToString() ?? (GetValue(dict, "ITEM_TYPE")?.ToString() ?? "");

            decimal? lalydothu = null;
            var rawLaly = GetValue(dict, "LALYDOTHU");
            if (rawLaly != null && decimal.TryParse(rawLaly.ToString(), out decimal decVal))
            {
                lalydothu = decVal;
            }

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

            return new LyDoThuChiTreeItem
            {
                Id = id,
                Name = name,
                Lalydothu = lalydothu,
                Loailydo = loailydo,
                Note = note,
                Status = status,
                Sortorder = sortOrder,
                ParentId = parentId,
                ItemType = itemType
            };
        }

        private static async Task EnsureDefaultDataAsync(IDbConnection conn)
        {
            try
            {
                var countObj = await conn.ExecuteScalarAsync("SELECT COUNT(*) FROM DLYDOTHUCHI");
                int count = Convert.ToInt32(countObj);
                if (count == 0)
                {
                    var defaultItems = new (string Name, decimal LaThu, string Loai)[]
                    {
                        ("Chi khác", 0, "-1"),
                        ("Chi lương nhân viên", 0, "1"),
                        ("Đặt trước", 0, "-1"),
                        ("Đồ dùng, dụng cụ", 0, "-1"),
                        ("Lương nhân viên", 0, "1"),
                        ("Lương vệ sỹ", 0, "1"),
                        ("Nạp thẻ trả trước", 30, "-1"),
                        ("Ngoại giao", 0, "2"),
                        ("Tạm ứng", 0, "-1"),
                        ("Thanh toán công nợ", 0, "-1"),
                        ("Thu công nợ", 30, "-1"),
                        ("Thu đặt trước tiền mua hàng", 30, "-1"),
                        ("Thu tạm ứng", 30, "-1"),
                        ("Thu tiền hoàn ứng", 30, "-1"),
                        ("Thưởng nhân viên", 0, "2"),
                        ("Tiền điện", 0, "1"),
                        ("Tiền điện thoại", 0, "1"),
                        ("Tiền nhà", 0, "1"),
                        ("Tiền nước", 0, "1"),
                        ("Trả trước tiền mua hàng", 0, "-1")
                    };

                    int nextId = 1;
                    foreach (var item in defaultItems)
                    {
                        try
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO DLYDOTHUCHI (ID, NAME, LALYDOTHU, LOAILYDO, STATUS, TIMECREATED)
                                VALUES (@Id, @Name, @LaThu, @Loai, 30, CURRENT_TIMESTAMP)",
                                new { Id = nextId, Name = item.Name, LaThu = item.LaThu, Loai = item.Loai });
                        }
                        catch
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO DLYDOTHUCHI (ID, NAME, LALYDOTHU, LOAILYDO)
                                VALUES (@Id, @Name, @LaThu, @Loai)",
                                new { Id = nextId, Name = item.Name, LaThu = item.LaThu, Loai = item.Loai });
                        }
                        nextId++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnsureDefaultDataAsync exception: " + ex.Message);
            }
        }

        public static async Task<ObservableCollection<LyDoThuChiTreeItem>> GetLyDoThuChiTreeAsync(bool isTrash = false)
        {
            var result = new ObservableCollection<LyDoThuChiTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    await EnsureDefaultDataAsync(conn);

                    var rows = (await conn.QueryAsync("SELECT * FROM DLYDOTHUCHI")).ToList();
                    var lookup = new Dictionary<string, LyDoThuChiTreeItem>(StringComparer.OrdinalIgnoreCase);
                    var allItems = new List<LyDoThuChiTreeItem>();

                    foreach (object r in rows)
                    {
                        LyDoThuChiTreeItem item = MapFromRow(r);

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

                    // Sắp xếp theo Sortorder, sau đó theo Tên
                    allItems = allItems
                        .OrderBy(x => x.Sortorder ?? 9999)
                        .ThenBy(x => x.Name)
                        .ToList();

                    // Xây dựng cây phân cấp
                    foreach (LyDoThuChiTreeItem item in allItems)
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
                Console.WriteLine("Error GetLyDoThuChiTreeAsync: " + ex.Message);
                throw;
            }
            return result;
        }

        public static async Task<List<LyDoThuChiTreeItem>> GetLyDoThuChiListAsync(bool isTrash = false)
        {
            var list = new List<LyDoThuChiTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    await EnsureDefaultDataAsync(conn);

                    var rows = (await conn.QueryAsync("SELECT * FROM DLYDOTHUCHI")).ToList();
                    foreach (object r in rows)
                    {
                        LyDoThuChiTreeItem item = MapFromRow(r);

                        // Lọc theo thùng rác
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

                    list = list
                        .OrderBy(x => x.Sortorder ?? 9999)
                        .ThenBy(x => x.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetLyDoThuChiListAsync: " + ex.Message);
                throw;
            }
            return list;
        }

        public static async Task<(bool Success, string ErrorMessage, string SavedId)> SaveLyDoThuChiAsync(
            string id, 
            string name, 
            decimal laLyDoThu, 
            string loaiLyDo, 
            string parentId, 
            string note, 
            string itemType = "")
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    bool isNew = string.IsNullOrEmpty(id);
                    if (isNew)
                    {
                        // Kiểm tra xem ID trong bảng là số hay chuỗi
                        int nextIntId = 1;
                        try
                        {
                            var maxId = await conn.ExecuteScalarAsync("SELECT MAX(ID) FROM DLYDOTHUCHI");
                            if (maxId != null && maxId != DBNull.Value)
                            {
                                nextIntId = Convert.ToInt32(maxId) + 1;
                            }
                        }
                        catch { }

                        string insertedId = nextIntId.ToString();

                        // Cách 1: Thử chèn với Int ID và USERCREATEDID
                        try
                        {
                            string sql1 = @"
                                INSERT INTO DLYDOTHUCHI (ID, NAME, LALYDOTHU, LOAILYDO, NOTE, STATUS, USERCREATEDID, TIMECREATED)
                                VALUES (@ID, @NAME, @LALYDOTHU, @LOAILYDO, @NOTE, 30, 1, CURRENT_TIMESTAMP)";

                            await conn.ExecuteAsync(sql1, new
                            {
                                ID = nextIntId,
                                NAME = name,
                                LALYDOTHU = laLyDoThu,
                                LOAILYDO = loaiLyDo,
                                NOTE = note ?? ""
                            });

                            return (true, "", insertedId);
                        }
                        catch (Exception ex1)
                        {
                            // Cách 2: Thử chèn với Int ID không có USERCREATEDID nếu cột không tồn tại
                            try
                            {
                                string sql2 = @"
                                    INSERT INTO DLYDOTHUCHI (ID, NAME, LALYDOTHU, LOAILYDO, NOTE, STATUS)
                                    VALUES (@ID, @NAME, @LALYDOTHU, @LOAILYDO, @NOTE, 30)";

                                await conn.ExecuteAsync(sql2, new
                                {
                                    ID = nextIntId,
                                    NAME = name,
                                    LALYDOTHU = laLyDoThu,
                                    LOAILYDO = loaiLyDo,
                                    NOTE = note ?? ""
                                });

                                return (true, "", insertedId);
                            }
                            catch (Exception ex2)
                            {
                                // Cách 3: Thử chèn với Guid ID
                                try
                                {
                                    string guidId = Guid.NewGuid().ToString();
                                    string sql3 = @"
                                        INSERT INTO DLYDOTHUCHI (ID, NAME, LALYDOTHU, LOAILYDO, NOTE, STATUS, USERCREATEDID, TIMECREATED)
                                        VALUES (@ID, @NAME, @LALYDOTHU, @LOAILYDO, @NOTE, 30, 1, CURRENT_TIMESTAMP)";

                                    await conn.ExecuteAsync(sql3, new
                                    {
                                        ID = guidId,
                                        NAME = name,
                                        LALYDOTHU = laLyDoThu,
                                        LOAILYDO = loaiLyDo,
                                        NOTE = note ?? ""
                                    });

                                    return (true, "", guidId);
                                }
                                catch (Exception ex3)
                                {
                                    return (false, $"Lỗi ghi CSDL: {ex1.Message}", "");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Cập nhật bản ghi
                        try
                        {
                            string sqlUpdate1 = @"
                                UPDATE DLYDOTHUCHI SET
                                    NAME = @NAME,
                                    LALYDOTHU = @LALYDOTHU,
                                    LOAILYDO = @LOAILYDO,
                                    NOTE = @NOTE,
                                    USERMODIFIEDID = 1,
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE ID = @ID";

                            int rowsAffected = 0;
                            if (int.TryParse(id, out int numId))
                            {
                                rowsAffected = await conn.ExecuteAsync(sqlUpdate1, new
                                {
                                    ID = numId,
                                    NAME = name,
                                    LALYDOTHU = laLyDoThu,
                                    LOAILYDO = loaiLyDo,
                                    NOTE = note ?? ""
                                });
                            }

                            if (rowsAffected == 0)
                            {
                                string sqlUpdate2 = @"
                                    UPDATE DLYDOTHUCHI SET
                                        NAME = @NAME,
                                        LALYDOTHU = @LALYDOTHU,
                                        LOAILYDO = @LOAILYDO,
                                        NOTE = @NOTE,
                                        USERMODIFIEDID = 1,
                                        TIMEMODIFIED = CURRENT_TIMESTAMP
                                    WHERE CAST(ID AS VARCHAR(50)) = @ID";

                                await conn.ExecuteAsync(sqlUpdate2, new
                                {
                                    ID = id,
                                    NAME = name,
                                    LALYDOTHU = laLyDoThu,
                                    LOAILYDO = loaiLyDo,
                                    NOTE = note ?? ""
                                });
                            }

                            return (true, "", id);
                        }
                        catch (Exception exUp)
                        {
                            return (false, $"Lỗi cập nhật CSDL: {exUp.Message}", "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, "");
            }
        }

        public static async Task<bool> DeleteLyDoThuChiAsync(string id, bool permanent = false)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    if (permanent)
                    {
                        if (int.TryParse(id, out int numId))
                        {
                            await conn.ExecuteAsync("DELETE FROM DLYDOTHUCHI WHERE ID = @ID", new { ID = numId });
                        }
                        else
                        {
                            await conn.ExecuteAsync("DELETE FROM DLYDOTHUCHI WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                        }
                    }
                    else
                    {
                        try
                        {
                            if (int.TryParse(id, out int numId))
                            {
                                await conn.ExecuteAsync("UPDATE DLYDOTHUCHI SET STATUS = 0 WHERE ID = @ID", new { ID = numId });
                            }
                            else
                            {
                                await conn.ExecuteAsync("UPDATE DLYDOTHUCHI SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                            }
                        }
                        catch
                        {
                            if (int.TryParse(id, out int numId))
                            {
                                await conn.ExecuteAsync("DELETE FROM DLYDOTHUCHI WHERE ID = @ID", new { ID = numId });
                            }
                            else
                            {
                                await conn.ExecuteAsync("DELETE FROM DLYDOTHUCHI WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteLyDoThuChiAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreLyDoThuChiAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    if (int.TryParse(id, out int numId))
                    {
                        await conn.ExecuteAsync("UPDATE DLYDOTHUCHI SET STATUS = 30 WHERE ID = @ID", new { ID = numId });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DLYDOTHUCHI SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreLyDoThuChiAsync: " + ex.Message);
                return false;
            }
        }
    }
}
