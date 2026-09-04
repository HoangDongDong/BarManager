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
    public class TaiKhoanNganHangTreeItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string SoTaiKhoan { get; set; } = "";
        public string NganHang { get; set; } = "";
        public string ChiNhanh { get; set; } = "";
        public string ChuTaiKhoan { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; }
        public int? Sortorder { get; set; }
        public string ParentId { get; set; } = "";
        public string ItemType { get; set; } = ""; // FOLDER, ITEM, SEPARATOR

        public ObservableCollection<TaiKhoanNganHangTreeItem> Children { get; set; } = new ObservableCollection<TaiKhoanNganHangTreeItem>();

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

    public static class LocalTaiKhoanNganHangService
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

        private static TaiKhoanNganHangTreeItem MapFromRow(object r)
        {
            var dict = r as IDictionary<string, object>;

            string id = GetValue(dict, "ID")?.ToString() ?? "";
            string name = GetValue(dict, "NAME")?.ToString() ?? "";
            string note = GetValue(dict, "NOTE")?.ToString() ?? "";
            string soTaiKhoan = GetValue(dict, "SOTAIKHOAN")?.ToString() ?? (GetValue(dict, "SO_TK")?.ToString() ?? "");
            string nganHang = GetValue(dict, "NGANHANG")?.ToString() ?? (GetValue(dict, "TEN_NGAN_HANG")?.ToString() ?? "");
            string chiNhanh = GetValue(dict, "CHINHANH")?.ToString() ?? (GetValue(dict, "CHI_NHANH")?.ToString() ?? "");
            string chuTaiKhoan = GetValue(dict, "CHUTAIKHOAN")?.ToString() ?? (GetValue(dict, "CHU_TK")?.ToString() ?? "");
            string parentId = GetValue(dict, "PARENTID")?.ToString() ?? (GetValue(dict, "PARENT_ID")?.ToString() ?? "");
            string itemType = GetValue(dict, "ITEMTYPE")?.ToString() ?? (GetValue(dict, "ITEM_TYPE")?.ToString() ?? "");

            int? status = null;
            var rawStatus = GetValue(dict, "STATUS");
            if (rawStatus != null)
            {
                if (int.TryParse(rawStatus.ToString(), out int statVal))
                {
                    status = statVal;
                }
                else if (bool.TryParse(rawStatus.ToString(), out bool bVal))
                {
                    status = bVal ? 30 : 0;
                }
            }

            int? sortOrder = null;
            var rawSort = GetValue(dict, "SORTORDER");
            if (rawSort != null && int.TryParse(rawSort.ToString(), out int sortVal))
            {
                sortOrder = sortVal;
            }

            return new TaiKhoanNganHangTreeItem
            {
                Id = id,
                Name = name,
                SoTaiKhoan = soTaiKhoan,
                NganHang = nganHang,
                ChiNhanh = chiNhanh,
                ChuTaiKhoan = chuTaiKhoan,
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
                var countObj = await conn.ExecuteScalarAsync("SELECT COUNT(*) FROM DTAIKHOANNGANHANG");
                int count = Convert.ToInt32(countObj);
                if (count == 0)
                {
                    var defaultItems = new (string Name, string SoTk, string NganHang)[]
                    {
                        ("tk 1", "0011001234567", "Vietcombank"),
                        ("tk2", "1902837465901", "Techcombank"),
                        ("tk3", "0451000384729", "BIDV")
                    };

                    int nextId = 1;
                    foreach (var item in defaultItems)
                    {
                        try
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO DTAIKHOANNGANHANG (ID, NAME, NOTE, STATUS, USERCREATEDID, TIMECREATED, SORTORDER)
                                VALUES (@Id, @Name, @Note, 30, 1, CURRENT_TIMESTAMP, @Sort)",
                                new { Id = nextId, Name = item.Name, Note = $"{item.NganHang} - {item.SoTk}", Sort = nextId });
                        }
                        catch
                        {
                            try
                            {
                                await conn.ExecuteAsync(@"
                                    INSERT INTO DTAIKHOANNGANHANG (ID, NAME, STATUS, USERCREATEDID, TIMECREATED)
                                    VALUES (@Id, @Name, 30, 1, CURRENT_TIMESTAMP)",
                                    new { Id = nextId, Name = item.Name });
                            }
                            catch
                            {
                                try
                                {
                                    await conn.ExecuteAsync(@"
                                        INSERT INTO DTAIKHOANNGANHANG (ID, NAME, USERCREATEDID)
                                        VALUES (@Id, @Name, 1)",
                                        new { Id = nextId, Name = item.Name });
                                }
                                catch { }
                            }
                        }
                        nextId++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnsureDefaultDataAsync DTAIKHOANNGANHANG exception: " + ex.Message);
            }
        }

        public static async Task<ObservableCollection<TaiKhoanNganHangTreeItem>> GetTaiKhoanNganHangTreeAsync(bool isTrash = false)
        {
            var result = new ObservableCollection<TaiKhoanNganHangTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    await EnsureDefaultDataAsync(conn);

                    var rows = (await conn.QueryAsync("SELECT * FROM DTAIKHOANNGANHANG")).ToList();
                    var lookup = new Dictionary<string, TaiKhoanNganHangTreeItem>(StringComparer.OrdinalIgnoreCase);
                    var allItems = new List<TaiKhoanNganHangTreeItem>();

                    foreach (object r in rows)
                    {
                        TaiKhoanNganHangTreeItem item = MapFromRow(r);

                        // Lọc theo thùng rác (Status = 0 hoặc false là đã xóa)
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
                    foreach (TaiKhoanNganHangTreeItem item in allItems)
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
                Console.WriteLine("Error GetTaiKhoanNganHangTreeAsync: " + ex.Message);
                throw;
            }
            return result;
        }

        public static async Task<List<TaiKhoanNganHangTreeItem>> GetTaiKhoanNganHangListAsync(bool isTrash = false)
        {
            var list = new List<TaiKhoanNganHangTreeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    await EnsureDefaultDataAsync(conn);

                    var rows = (await conn.QueryAsync("SELECT * FROM DTAIKHOANNGANHANG")).ToList();
                    foreach (object r in rows)
                    {
                        TaiKhoanNganHangTreeItem item = MapFromRow(r);

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
                Console.WriteLine("Error GetTaiKhoanNganHangListAsync: " + ex.Message);
                throw;
            }
            return list;
        }

        public static async Task<(bool Success, string ErrorMessage, string SavedId)> SaveTaiKhoanNganHangAsync(
            string id,
            string name,
            string parentId = "",
            string note = "",
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
                        int nextIntId = 1;
                        try
                        {
                            var maxId = await conn.ExecuteScalarAsync("SELECT MAX(ID) FROM DTAIKHOANNGANHANG");
                            if (maxId != null && maxId != DBNull.Value)
                            {
                                nextIntId = Convert.ToInt32(maxId) + 1;
                            }
                        }
                        catch { }

                        string insertedId = nextIntId.ToString();
                        int? parentIdInt = null;
                        if (int.TryParse(parentId, out int pVal) && pVal > 0)
                        {
                            parentIdInt = pVal;
                        }

                        // Cách 1: Thử chèn đầy đủ các cột
                        try
                        {
                            string sql1 = @"
                                INSERT INTO DTAIKHOANNGANHANG (ID, NAME, NOTE, STATUS, USERCREATEDID, TIMECREATED, ITEMTYPE, PARENTID, SORTORDER)
                                VALUES (@ID, @NAME, @NOTE, 30, 1, CURRENT_TIMESTAMP, @ITEMTYPE, @PARENTID, @SORTORDER)";

                            await conn.ExecuteAsync(sql1, new
                            {
                                ID = nextIntId,
                                NAME = name,
                                NOTE = note ?? "",
                                ITEMTYPE = itemType ?? "",
                                PARENTID = (object)parentIdInt ?? DBNull.Value,
                                SORTORDER = nextIntId
                            });

                            return (true, "", insertedId);
                        }
                        catch
                        {
                            // Cách 2: Thử chèn không có ITEMTYPE nếu cột không tồn tại
                            try
                            {
                                string sql2 = @"
                                    INSERT INTO DTAIKHOANNGANHANG (ID, NAME, NOTE, STATUS, USERCREATEDID, TIMECREATED, PARENTID, SORTORDER)
                                    VALUES (@ID, @NAME, @NOTE, 30, 1, CURRENT_TIMESTAMP, @PARENTID, @SORTORDER)";

                                await conn.ExecuteAsync(sql2, new
                                {
                                    ID = nextIntId,
                                    NAME = name,
                                    NOTE = note ?? "",
                                    PARENTID = (object)parentIdInt ?? DBNull.Value,
                                    SORTORDER = nextIntId
                                });

                                return (true, "", insertedId);
                            }
                            catch
                            {
                                // Cách 3: Thử chèn cơ bản có USERCREATEDID và TIMECREATED
                                try
                                {
                                    string sql3 = @"
                                        INSERT INTO DTAIKHOANNGANHANG (ID, NAME, NOTE, STATUS, USERCREATEDID, TIMECREATED)
                                        VALUES (@ID, @NAME, @NOTE, 30, 1, CURRENT_TIMESTAMP)";

                                    await conn.ExecuteAsync(sql3, new
                                    {
                                        ID = nextIntId,
                                        NAME = name,
                                        NOTE = note ?? ""
                                    });

                                    return (true, "", insertedId);
                                }
                                catch
                                {
                                    // Cách 4: Thử chèn tối thiểu có USERCREATEDID
                                    try
                                    {
                                        string sql4 = @"
                                            INSERT INTO DTAIKHOANNGANHANG (ID, NAME, USERCREATEDID)
                                            VALUES (@ID, @NAME, 1)";

                                        await conn.ExecuteAsync(sql4, new
                                        {
                                            ID = nextIntId,
                                            NAME = name
                                        });

                                        return (true, "", insertedId);
                                    }
                                    catch (Exception ex4)
                                    {
                                        return (false, $"Lỗi ghi CSDL: {ex4.Message}", "");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Cập nhật
                        try
                        {
                            string sqlUpdate1 = @"
                                UPDATE DTAIKHOANNGANHANG SET
                                    NAME = @NAME,
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
                                    NOTE = note ?? ""
                                });
                            }

                            if (rowsAffected == 0)
                            {
                                string sqlUpdate2 = @"
                                    UPDATE DTAIKHOANNGANHANG SET
                                        NAME = @NAME,
                                        NOTE = @NOTE,
                                        USERMODIFIEDID = 1,
                                        TIMEMODIFIED = CURRENT_TIMESTAMP
                                    WHERE CAST(ID AS VARCHAR(50)) = @ID";

                                await conn.ExecuteAsync(sqlUpdate2, new
                                {
                                    ID = id,
                                    NAME = name,
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

        public static async Task<bool> DeleteTaiKhoanNganHangAsync(string id, bool permanent = false)
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
                            await conn.ExecuteAsync("DELETE FROM DTAIKHOANNGANHANG WHERE ID = @ID", new { ID = numId });
                        }
                        else
                        {
                            await conn.ExecuteAsync("DELETE FROM DTAIKHOANNGANHANG WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                        }
                    }
                    else
                    {
                        try
                        {
                            if (int.TryParse(id, out int numId))
                            {
                                await conn.ExecuteAsync("UPDATE DTAIKHOANNGANHANG SET STATUS = 0 WHERE ID = @ID", new { ID = numId });
                            }
                            else
                            {
                                await conn.ExecuteAsync("UPDATE DTAIKHOANNGANHANG SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                            }
                        }
                        catch
                        {
                            if (int.TryParse(id, out int numId))
                            {
                                await conn.ExecuteAsync("DELETE FROM DTAIKHOANNGANHANG WHERE ID = @ID", new { ID = numId });
                            }
                            else
                            {
                                await conn.ExecuteAsync("DELETE FROM DTAIKHOANNGANHANG WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteTaiKhoanNganHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreTaiKhoanNganHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    if (int.TryParse(id, out int numId))
                    {
                        await conn.ExecuteAsync("UPDATE DTAIKHOANNGANHANG SET STATUS = 30 WHERE ID = @ID", new { ID = numId });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DTAIKHOANNGANHANG SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @ID", new { ID = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreTaiKhoanNganHangAsync: " + ex.Message);
                return false;
            }
        }
    }
}
