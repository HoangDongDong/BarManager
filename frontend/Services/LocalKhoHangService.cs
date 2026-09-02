using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuanLyBar.Client.Services
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b) return Visibility.Visible;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b) return Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public static class KhoHangVisibilityConverters
    {
        public static IValueConverter BooleanToVisibilityConverter { get; } = new BooleanToVisibilityConverter();
        public static IValueConverter InverseBooleanToVisibilityConverter { get; } = new InverseBooleanToVisibilityConverter();
    }

    public class KhoHangTreeItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? ParentId { get; set; }
        public string ParentDir { get; set; }
        public string ItemType { get; set; } // FOLDER, ITEM, SEPARATOR
        public bool Chophepamkho { get; set; }
        public int? DcuahangId { get; set; }
        public string TenCuaHang { get; set; }
        public int? SortOrder { get; set; }
        public bool Status { get; set; } = true;

        public bool IsFolder => ItemType == "FOLDER";
        public bool IsSeparator => ItemType == "SEPARATOR";
        public bool IsWarehouse => !IsFolder && !IsSeparator;

        public string Icon
        {
            get
            {
                if (IsSeparator) return "—";
                if (IsFolder) return "📁";
                return "🔍";
            }
        }

        public bool IsExpanded { get; set; } = true;
        public bool IsSelected { get; set; }
        public ObservableCollection<KhoHangTreeItem> Children { get; set; } = new ObservableCollection<KhoHangTreeItem>();
    }

    public static class LocalKhoHangService
    {
        public static async Task<ObservableCollection<KhoHangTreeItem>> GetKhoHangTreeAsync(bool showTrash = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    string sql = @"
                        SELECT 
                            k.ID,
                            k.NAME,
                            k.NOTE,
                            k.PARENTID,
                            k.PARENTDIR,
                            k.ITEMTYPE,
                            k.CHOPHEPAMKHO,
                            k.DCUAHANGID,
                            c.NAME as TenCuaHang,
                            k.SORTORDER,
                            k.STATUS
                        FROM DKHOHANG k
                        LEFT JOIN DCUAHANG c ON k.DCUAHANGID = c.ID
                        ORDER BY k.SORTORDER, k.ID";

                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var flatList = new List<KhoHangTreeItem>();

                    foreach (var r in rows)
                    {
                        bool status = true;
                        if (r.STATUS != null)
                        {
                            if (r.STATUS is bool b) status = b;
                            else if (int.TryParse(r.STATUS.ToString(), out int sInt)) status = (sInt == 1);
                        }

                        if (!showTrash && !status) continue;
                        if (showTrash && status) continue;

                        bool allowAmKho = false;
                        if (r.CHOPHEPAMKHO != null)
                        {
                            string s = r.CHOPHEPAMKHO.ToString().Trim().ToLower();
                            allowAmKho = (s == "1" || s == "true");
                        }

                        flatList.Add(new KhoHangTreeItem
                        {
                            Id = Convert.ToInt32(r.ID),
                            Name = r.NAME?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            ParentId = r.PARENTID != null ? Convert.ToInt32(r.PARENTID) : (int?)null,
                            ParentDir = r.PARENTDIR?.ToString() ?? "",
                            ItemType = r.ITEMTYPE?.ToString() ?? "ITEM",
                            Chophepamkho = allowAmKho,
                            DcuahangId = r.DCUAHANGID != null ? Convert.ToInt32(r.DCUAHANGID) : (int?)null,
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            SortOrder = r.SORTORDER != null ? Convert.ToInt32(r.SORTORDER) : 0,
                            Status = status
                        });
                    }

                    // Nếu chưa có kho nào trong DB, tạo kho mặc định "KHO BÁN HÀNG"
                    if (flatList.Count == 0 && !showTrash)
                    {
                        await EnsureDefaultKhoHangAsync(conn);
                        return await GetKhoHangTreeAsync(showTrash);
                    }

                    return BuildTree(flatList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetKhoHangTreeAsync: " + ex.Message);
                return new ObservableCollection<KhoHangTreeItem>();
            }
        }

        private static async Task EnsureDefaultKhoHangAsync(IDbConnection conn)
        {
            try
            {
                int count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM DKHOHANG");
                if (count == 0)
                {
                    int? cuahangId = await conn.ExecuteScalarAsync<int?>("SELECT FIRST 1 ID FROM DCUAHANG");

                    string insertSql = @"
                        INSERT INTO DKHOHANG (ID, NAME, NOTE, STATUS, ITEMTYPE, CHOPHEPAMKHO, DCUAHANGID, SORTORDER)
                        VALUES (1, 'KHO BÁN HÀNG', 'Kho bán hàng mặc định', 1, 'ITEM', '0', @CuaHangId, 1)";
                    await conn.ExecuteAsync(insertSql, new { CuaHangId = cuahangId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error EnsureDefaultKhoHangAsync: " + ex.Message);
            }
        }

        private static ObservableCollection<KhoHangTreeItem> BuildTree(List<KhoHangTreeItem> flatList)
        {
            var tree = new ObservableCollection<KhoHangTreeItem>();
            var lookup = flatList.ToDictionary(x => x.Id);

            foreach (var item in flatList)
            {
                if (item.ParentId.HasValue && lookup.ContainsKey(item.ParentId.Value))
                {
                    lookup[item.ParentId.Value].Children.Add(item);
                }
                else
                {
                    tree.Add(item);
                }
            }

            return tree;
        }

        public static async Task<List<KhoHangTreeItem>> GetAllWarehousesFlatAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    string sql = @"
                        SELECT 
                            k.ID,
                            k.NAME,
                            k.NOTE,
                            k.PARENTID,
                            k.PARENTDIR,
                            k.ITEMTYPE,
                            k.CHOPHEPAMKHO,
                            k.DCUAHANGID,
                            c.NAME as TenCuaHang,
                            k.SORTORDER,
                            k.STATUS
                        FROM DKHOHANG k
                        LEFT JOIN DCUAHANG c ON k.DCUAHANGID = c.ID
                        WHERE (k.STATUS = 1 OR k.STATUS IS NULL)
                          AND (k.ITEMTYPE IS NULL OR k.ITEMTYPE = 'ITEM')
                        ORDER BY k.SORTORDER, k.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();
                    var list = new List<KhoHangTreeItem>();

                    foreach (var r in rows)
                    {
                        list.Add(new KhoHangTreeItem
                        {
                            Id = Convert.ToInt32(r.ID),
                            Name = r.NAME?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            DcuahangId = r.DCUAHANGID != null ? Convert.ToInt32(r.DCUAHANGID) : (int?)null,
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            ItemType = "ITEM"
                        });
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAllWarehousesFlatAsync: " + ex.Message);
                return new List<KhoHangTreeItem>();
            }
        }

        public static async Task<List<DCUAHANG>> GetCuaHangListAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    string sql = "SELECT ID, NAME, NOTE FROM DCUAHANG WHERE STATUS = 1 OR STATUS IS NULL ORDER BY NAME";
                    var list = (await conn.QueryAsync<DCUAHANG>(sql)).ToList();

                    if (list.Count == 0)
                    {
                        list.Add(new DCUAHANG { Id = 1, Name = "TRỤ SỞ CHÍNH" });
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCuaHangListAsync: " + ex.Message);
                return new List<DCUAHANG> { new DCUAHANG { Id = 1, Name = "TRỤ SỞ CHÍNH" } };
            }
        }

        public static async Task<(bool Success, string ErrorMsg, int NewId)> SaveKhoHangAsync(KhoHangTreeItem item, bool isNew)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    string chophepAmKho = item.Chophepamkho ? "1" : "0";

                    if (isNew)
                    {
                        int nextId = await conn.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(ID), 0) + 1 FROM DKHOHANG");

                        string insertSql = @"
                            INSERT INTO DKHOHANG (
                                ID, NAME, NOTE, STATUS, PARENTID, PARENTDIR, ITEMTYPE, 
                                CHOPHEPAMKHO, DCUAHANGID, SORTORDER, TIMECREATED
                            ) VALUES (
                                @Id, @Name, @Note, 1, @ParentId, @ParentDir, @ItemType,
                                @Chophepamkho, @DcuahangId, @SortOrder, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = nextId,
                            Name = item.Name,
                            Note = item.Note ?? "",
                            ParentId = item.ParentId,
                            ParentDir = item.ParentDir ?? "",
                            ItemType = item.ItemType ?? "ITEM",
                            Chophepamkho = chophepAmKho,
                            DcuahangId = item.DcuahangId,
                            SortOrder = item.SortOrder ?? nextId
                        });

                        return (true, "", nextId);
                    }
                    else
                    {
                        string updateSql = @"
                            UPDATE DKHOHANG SET
                                NAME = @Name,
                                NOTE = @Note,
                                PARENTID = @ParentId,
                                CHOPHEPAMKHO = @Chophepamkho,
                                DCUAHANGID = @DcuahangId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE ID = @Id";

                        await conn.ExecuteAsync(updateSql, new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Note = item.Note ?? "",
                            ParentId = item.ParentId,
                            Chophepamkho = chophepAmKho,
                            DcuahangId = item.DcuahangId
                        });

                        return (true, "", item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, 0);
            }
        }

        public static async Task<bool> DeleteKhoHangAsync(int id, bool permanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    if (permanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM DKHOHANG WHERE ID = @Id OR PARENTID = @Id", new { Id = id });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DKHOHANG SET STATUS = 0 WHERE ID = @Id OR PARENTID = @Id", new { Id = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteKhoHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreKhoHangAsync(int id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    await conn.ExecuteAsync("UPDATE DKHOHANG SET STATUS = 1 WHERE ID = @Id", new { Id = id });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreKhoHangAsync: " + ex.Message);
                return false;
            }
        }
    }
}
