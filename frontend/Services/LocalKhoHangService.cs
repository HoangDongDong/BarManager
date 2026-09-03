using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapper;
using QuanLyBar.Client.Models;

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

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public static class KhoHangVisibilityConverters
    {
        public static IValueConverter BooleanToVisibilityConverter { get; } = new BooleanToVisibilityConverter();
        public static IValueConverter InverseBooleanToVisibilityConverter { get; } = new InverseBooleanToVisibilityConverter();
        public static IValueConverter NullToVisibilityConverter { get; } = new NullToVisibilityConverter();
        public static IValueConverter NotNullToVisibilityConverter { get; } = new NotNullToVisibilityConverter();
    }

    public class KhoHangTreeItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string ParentId { get; set; }
        public string ParentDir { get; set; }
        public string ItemType { get; set; } = "0"; // "0" or "ITEM" = Warehouse, "1" or "FOLDER" = Folder, "2" or "SEPARATOR" = Separator
        public bool Chophepamkho { get; set; }
        public string DcuahangId { get; set; }
        public string TenCuaHang { get; set; }
        public string SortOrder { get; set; }
        public bool Status { get; set; } = true;
        public string SimageId { get; set; }
        public byte[] ImageBytes { get; set; }
        public ImageSource ImageSource { get; set; }

        public bool IsFolder => ItemType == "FOLDER" || ItemType == "1";
        public bool IsSeparator => ItemType == "SEPARATOR" || ItemType == "2";
        public bool IsWarehouse => !IsFolder && !IsSeparator;

        public string CustomIcon { get; set; }

        public string Icon
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomIcon)) return CustomIcon;
                if (Id == "ALL") return "🌐";
                if (Id == "UNASSIGNED") return "✳️";
                if (Id == "TRASH") return "🗑️";
                if (IsSeparator) return "—";
                if (IsFolder) return "📁";
                return "🏢";
            }
        }

        public bool IsExpanded { get; set; } = true;
        public bool IsSelected { get; set; }
        public ObservableCollection<KhoHangTreeItem> Children { get; set; } = new ObservableCollection<KhoHangTreeItem>();
    }

    public class KhoHangCuaHangItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string SimageId { get; set; }
        public byte[] ImageBytes { get; set; }
        public ImageSource ImageSource { get; set; }
    }

    public static class LocalKhoHangService
    {
        public static BitmapImage BytesToBitmapImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = ms;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<SImageViewModel>> GetSImagesAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    string sql = "SELECT ID, NAME, IMAGE, SORTORDER FROM SIMAGE WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME";
                    var rows = await conn.QueryAsync(sql);
                    var list = new List<SImageViewModel>();
                    foreach (var r in rows)
                    {
                        byte[] bytes = r.IMAGE as byte[];
                        var img = BytesToBitmapImage(bytes);
                        list.Add(new SImageViewModel
                        {
                            Id = r.ID?.ToString(),
                            ImageBytes = bytes,
                            ImageSource = img
                        });
                    }
                    return list;
                }
            }
            catch
            {
                return new List<SImageViewModel>();
            }
        }
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
                            k.STATUS,
                            k.SIMAGEID,
                            sim.IMAGE as ImageBytes
                        FROM DKHOHANG k
                        LEFT JOIN DCUAHANG c ON CAST(k.DCUAHANGID AS VARCHAR(50)) = CAST(c.ID AS VARCHAR(50))
                        LEFT JOIN SIMAGE sim ON CAST(k.SIMAGEID AS VARCHAR(50)) = CAST(sim.ID AS VARCHAR(50))
                        ORDER BY k.SORTORDER, k.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var flatList = new List<KhoHangTreeItem>();

                    foreach (var r in rows)
                    {
                        bool isDeleted = false;
                        if (r.STATUS != null)
                        {
                            string sVal = r.STATUS.ToString().Trim();
                            if (sVal == "0" || sVal.Equals("false", StringComparison.OrdinalIgnoreCase))
                            {
                                isDeleted = true;
                            }
                        }

                        if (!showTrash && isDeleted) continue;
                        if (showTrash && !isDeleted) continue;

                        bool allowAmKho = false;
                        if (r.CHOPHEPAMKHO != null)
                        {
                            string s = r.CHOPHEPAMKHO.ToString().Trim().ToLower();
                            allowAmKho = (s == "1" || s == "true");
                        }

                        byte[] imgBytes = r.IMAGEBYTES as byte[];

                        flatList.Add(new KhoHangTreeItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            ParentId = r.PARENTID?.ToString(),
                            ParentDir = r.PARENTDIR?.ToString() ?? "",
                            ItemType = r.ITEMTYPE?.ToString() ?? "0",
                            Chophepamkho = allowAmKho,
                            DcuahangId = r.DCUAHANGID?.ToString(),
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            SortOrder = r.SORTORDER?.ToString() ?? "",
                            Status = !isDeleted,
                            SimageId = r.SIMAGEID?.ToString(),
                            ImageBytes = imgBytes,
                            ImageSource = BytesToBitmapImage(imgBytes)
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

        private static async Task<string> GetCurrentUserIdAsync(IDbConnection conn)
        {
            if (SessionContext.CurrentUser != null && !string.IsNullOrEmpty(SessionContext.CurrentUser.Id))
            {
                return SessionContext.CurrentUser.Id;
            }

            try
            {
                var userId = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                if (userId != null) return userId.ToString();
            }
            catch { }

            return "4f1466a0-0756-4ba9-afa8-053b96ca7569";
        }

        private static async Task EnsureDefaultKhoHangAsync(IDbConnection conn)
        {
            try
            {
                int count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM DKHOHANG WHERE (STATUS IS NULL OR STATUS <> 0)");
                if (count == 0)
                {
                    string cuahangId = null;
                    try
                    {
                        cuahangId = (await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM DCUAHANG WHERE STATUS IS NULL OR STATUS <> 0"))?.ToString();
                    }
                    catch
                    {
                        try
                        {
                            cuahangId = (await conn.ExecuteScalarAsync<object>("SELECT TOP 1 ID FROM DCUAHANG WHERE STATUS IS NULL OR STATUS <> 0"))?.ToString();
                        }
                        catch { }
                    }

                    string newId = Guid.NewGuid().ToString();
                    string userId = await GetCurrentUserIdAsync(conn);

                    string insertSql = @"
                        INSERT INTO DKHOHANG (ID, NAME, NOTE, STATUS, ITEMTYPE, CHOPHEPAMKHO, DCUAHANGID, SORTORDER, USERCREATEDID, TIMECREATED)
                        VALUES (@Id, 'KHO BÁN HÀNG', 'Kho bán hàng mặc định', 30, '0', '0', @CuaHangId, 'ZZZZ', @UserCreatedId, CURRENT_TIMESTAMP)";
                    await conn.ExecuteAsync(insertSql, new { Id = newId, CuaHangId = cuahangId, UserCreatedId = userId });
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
            var lookup = new Dictionary<string, KhoHangTreeItem>();

            foreach (var item in flatList)
            {
                if (!string.IsNullOrEmpty(item.Id))
                {
                    lookup[item.Id] = item;
                }
            }

            foreach (var item in flatList)
            {
                if (!string.IsNullOrEmpty(item.ParentId) && lookup.TryGetValue(item.ParentId, out var parent))
                {
                    parent.Children.Add(item);
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
                            k.STATUS,
                            k.SIMAGEID,
                            sim.IMAGE as ImageBytes
                        FROM DKHOHANG k
                        LEFT JOIN DCUAHANG c ON CAST(k.DCUAHANGID AS VARCHAR(50)) = CAST(c.ID AS VARCHAR(50))
                        LEFT JOIN SIMAGE sim ON CAST(k.SIMAGEID AS VARCHAR(50)) = CAST(sim.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS <> 0)
                          AND (k.ITEMTYPE IS NULL OR CAST(k.ITEMTYPE AS VARCHAR(10)) = '0')
                        ORDER BY k.SORTORDER, k.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();
                    var list = new List<KhoHangTreeItem>();

                    foreach (var r in rows)
                    {
                        byte[] imgBytes = r.IMAGEBYTES as byte[];
                        list.Add(new KhoHangTreeItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            DcuahangId = r.DCUAHANGID?.ToString(),
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            ItemType = "0",
                            SimageId = r.SIMAGEID?.ToString(),
                            ImageBytes = imgBytes,
                            ImageSource = BytesToBitmapImage(imgBytes)
                        });
                    }

                    if (list.Count == 0)
                    {
                        await EnsureDefaultKhoHangAsync(conn);
                        // Query lại
                        var reRows = (await conn.QueryAsync(sql)).ToList();
                        foreach (var r in reRows)
                        {
                            byte[] imgBytes = r.IMAGEBYTES as byte[];
                            list.Add(new KhoHangTreeItem
                            {
                                Id = r.ID?.ToString() ?? "",
                                Name = r.NAME?.ToString() ?? "",
                                Note = r.NOTE?.ToString() ?? "",
                                DcuahangId = r.DCUAHANGID?.ToString(),
                                TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                                ItemType = "0",
                                SimageId = r.SIMAGEID?.ToString(),
                                ImageBytes = imgBytes,
                                ImageSource = BytesToBitmapImage(imgBytes)
                            });
                        }
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

        public static async Task<List<KhoHangCuaHangItem>> GetCuaHangListAsync()
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
                            c.ID, 
                            c.NAME, 
                            c.NOTE, 
                            c.SIMAGEID, 
                            sim.IMAGE as ImageBytes 
                        FROM DCUAHANG c
                        LEFT JOIN SIMAGE sim ON CAST(c.SIMAGEID AS VARCHAR(50)) = CAST(sim.ID AS VARCHAR(50))
                        WHERE (c.STATUS IS NULL OR c.STATUS <> 0) 
                        ORDER BY c.SORTORDER, c.NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();
                    var list = new List<KhoHangCuaHangItem>();

                    foreach (var r in rows)
                    {
                        byte[] imgBytes = r.IMAGEBYTES as byte[];
                        list.Add(new KhoHangCuaHangItem
                        {
                            Id = r.ID?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            SimageId = r.SIMAGEID?.ToString(),
                            ImageBytes = imgBytes,
                            ImageSource = BytesToBitmapImage(imgBytes)
                        });
                    }

                    if (list.Count == 0)
                    {
                        list.Add(new KhoHangCuaHangItem { Id = "1", Name = "TRỤ SỞ CHÍNH" });
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCuaHangListAsync: " + ex.Message);
                return new List<KhoHangCuaHangItem> { new KhoHangCuaHangItem { Id = "1", Name = "TRỤ SỞ CHÍNH" } };
            }
        }

        public static async Task<(bool Success, string ErrorMsg, string NewId)> SaveKhoHangAsync(KhoHangTreeItem item, bool isNew)
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
                    string itemType = item.ItemType;
                    if (string.IsNullOrEmpty(itemType) || itemType == "ITEM") itemType = "0";
                    else if (itemType == "FOLDER") itemType = "1";
                    else if (itemType == "SEPARATOR") itemType = "2";

                    string userId = await GetCurrentUserIdAsync(conn);

                    // Mặc định icon kính lúp / kho nếu trống
                    string simageId = item.SimageId;
                    if (string.IsNullOrEmpty(simageId))
                    {
                        simageId = "a38e42b9-aeda-4a67-8761-a5f4dc3571c1";
                    }

                    if (isNew || string.IsNullOrEmpty(item.Id))
                    {
                        string newId = Guid.NewGuid().ToString();

                        string insertSql = @"
                            INSERT INTO DKHOHANG (
                                ID, NAME, NOTE, STATUS, PARENTID, PARENTDIR, ITEMTYPE, 
                                CHOPHEPAMKHO, DCUAHANGID, SORTORDER, SIMAGEID, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @Name, @Note, 30, @ParentId, @ParentDir, @ItemType,
                                @Chophepamkho, @DcuahangId, @SortOrder, @SimageId, @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = newId,
                            Name = item.Name,
                            Note = item.Note ?? "",
                            ParentId = string.IsNullOrEmpty(item.ParentId) ? null : item.ParentId,
                            ParentDir = item.ParentDir ?? "",
                            ItemType = itemType,
                            Chophepamkho = chophepAmKho,
                            DcuahangId = string.IsNullOrEmpty(item.DcuahangId) ? null : item.DcuahangId,
                            SortOrder = string.IsNullOrEmpty(item.SortOrder) ? "ZZZZ" : item.SortOrder,
                            SimageId = simageId,
                            UserCreatedId = userId
                        });

                        return (true, "", newId);
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
                                SIMAGEID = @SimageId,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(updateSql, new
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Note = item.Note ?? "",
                            ParentId = string.IsNullOrEmpty(item.ParentId) ? null : item.ParentId,
                            Chophepamkho = chophepAmKho,
                            DcuahangId = string.IsNullOrEmpty(item.DcuahangId) ? null : item.DcuahangId,
                            SimageId = simageId,
                            UserModifiedId = userId
                        });

                        return (true, "", item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, "");
            }
        }

        public static async Task<bool> DeleteKhoHangAsync(string id, bool permanent = false)
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
                        await conn.ExecuteAsync("DELETE FROM DKHOHANG WHERE CAST(ID AS VARCHAR(50)) = @Id OR CAST(PARENTID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DKHOHANG SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id OR CAST(PARENTID AS VARCHAR(50)) = @Id", new { Id = id });
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

        public static async Task<bool> RestoreKhoHangAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }

                    await conn.ExecuteAsync("UPDATE DKHOHANG SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
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
