using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;
using System.Windows;

namespace QuanLyBar.Client.Services
{
    public class LocalBanKhuVucService
    {
        public async Task<ObservableCollection<KhuVucViewModel>> GetKhuVucTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // Firebird schema typically uses uppercase column names without underscores
                    string sql = @"
                        SELECT ID as Id, 
                               NAME as Name, 
                               PARENTID as ParentId, 
                               PARENTDIR as ParentDir, 
                               SORTORDER as SortOrder, 
                               STATUS as Status,
                               ANH as Anh
                        FROM DKHUVUC
                        ORDER BY SORTORDER, NAME";

                    var allItemsQuery = await conn.QueryAsync<dynamic>(sql);
                    var allItems = new List<KhuVucViewModel>();
                    foreach (var d in allItemsQuery)
                    {
                        var vm = new KhuVucViewModel
                        {
                            Id = d.ID?.ToString(),
                            Name = d.NAME,
                            ParentId = d.PARENTID?.ToString(),
                            ParentDir = d.PARENTDIR,
                            SortOrder = d.SORTORDER?.ToString(),
                            Status = (d.STATUS != null) ? Convert.ToBoolean(d.STATUS) : (bool?)null
                        };
                        
                        byte[] anh = d.ANH as byte[];
                        if (anh != null && anh.Length > 0)
                        {
                            try
                            {
                                using (var ms = new System.IO.MemoryStream(anh))
                                {
                                    var image = new System.Windows.Media.Imaging.BitmapImage();
                                    image.BeginInit();
                                    image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                    image.StreamSource = ms;
                                    image.EndInit();
                                    image.Freeze();
                                    vm.ImageSource = image;
                                }
                            }
                            catch { }
                        }
                        
                        allItems.Add(vm);
                    }

                    // Xây dựng cây
                    var tree = new ObservableCollection<KhuVucViewModel>();
                    var lookup = new Dictionary<string, KhuVucViewModel>();

                    // Fake root note "Tất cả"
                    var rootNode = new KhuVucViewModel { Id = null, Name = "Tất cả", IsExpanded = true, IsSelected = true };
                    tree.Add(rootNode);

                    foreach (var item in allItems)
                    {
                        lookup[item.Id] = item;
                    }

                    foreach (var item in allItems)
                    {
                        if (string.IsNullOrEmpty(item.ParentId))
                        {
                            rootNode.Children.Add(item);
                        }
                        else
                        {
                            if (lookup.TryGetValue(item.ParentId, out var parent))
                            {
                                parent.Children.Add(item);
                            }
                            else
                            {
                                rootNode.Children.Add(item);
                            }
                        }
                    }

                    // Thêm "Thùng rác"
                    var trashNode = new KhuVucViewModel { Id = "-1", Name = "Thùng rác" };
                    rootNode.Children.Add(trashNode);

                    return tree;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cây khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<KhuVucViewModel>();
            }
        }

        public async Task<List<BanViewModel>> GetBanListAsync(string khuvucId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT CAST(b.ID AS VARCHAR(50)) as Id, 
                               b.NAME as Name, 
                               b.NOTE as Note,
                               k.NAME as KhuVucName,
                               h.NAME as NhomHienThiName,
                               p.NAME as LoaiPhongName
                        FROM DBAN b
                        LEFT JOIN DKHUVUC k ON b.DKHUVUCID = k.ID
                        LEFT JOIN DNHOMHIENTHI h ON b.DNHOMHIENTHIID = h.ID
                        LEFT JOIN DLOAIPHONG p ON b.DLOAIPHONGID = p.ID
                        WHERE 1=1 ";

                    if (khuvucId == "-1")
                    {
                        sql += " AND (b.STATUS = 0) ";
                    }
                    else
                    {
                        sql += " AND (b.STATUS <> 0 OR b.STATUS IS NULL) ";
                        if (!string.IsNullOrEmpty(khuvucId))
                        {
                            sql += " AND (b.DKHUVUCID = @KhuVucId OR k.PARENTID = @KhuVucId OR k.PARENTDIR LIKE '%' || @KhuVucId || ',%')";
                        }
                    }

                    sql += " ORDER BY b.NAME";

                    var result = await conn.QueryAsync<BanViewModel>(sql, new { KhuVucId = khuvucId });
                    var list = result.ToList();
                    
                    // Gán số thứ tự cho giao diện hiển thị
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Stt = i + 1;
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BanViewModel>();
            }
        }
        public async Task<bool> InsertKhuVucAsync(string name, string parentId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // Generate an integer ID (stored as string in DB but parses as int)
                    var allIds = await conn.QueryAsync<string>("SELECT ID FROM DKHUVUC");
                    int maxId = 0;
                    foreach (var idStr in allIds)
                    {
                        if (int.TryParse(idStr, out int idInt))
                        {
                            if (idInt > maxId) maxId = idInt;
                        }
                    }
                    var newId = (maxId + 1).ToString();

                    string sql = @"
                        INSERT INTO DKHUVUC (ID, NAME, PARENTID, STATUS, USERCREATEDID, TIMECREATED) 
                        VALUES (@Id, @Name, @ParentId, 1, 1, CURRENT_TIMESTAMP)";
                    
                    var rows = await conn.ExecuteAsync(sql, new { Id = newId, Name = name, ParentId = parentId });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateKhuVucAsync(string id, string name)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        UPDATE DKHUVUC 
                        SET NAME = @Name 
                        WHERE ID = @Id";
                    
                    var rows = await conn.ExecuteAsync(sql, new { Name = name, Id = id });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteKhuVucAsync(string id, bool isPermanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = isPermanent
                        ? "DELETE FROM DKHUVUC WHERE ID = @Id"
                        : "UPDATE DKHUVUC SET STATUS = 0 WHERE ID = @Id";
                    var rows = await conn.ExecuteAsync(sql, new { Id = id });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<List<LookupItem>> GetLookupAsync(string tableName)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = $"SELECT CAST(ID AS VARCHAR(50)) as Id, CAST(NAME AS VARCHAR(255)) as Name FROM {tableName} WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME";
                    var result = new List<LookupItem>();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var idStr = reader[0]?.ToString()?.Trim() ?? "";
                                var nameStr = reader[1]?.ToString()?.Trim() ?? "";
                                if (!string.IsNullOrEmpty(idStr) && !string.IsNullOrEmpty(nameStr))
                                {
                                    result.Add(new LookupItem { Id = idStr, Name = nameStr });
                                }
                            }
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải {tableName}: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<LookupItem>();
            }
        }

        public async Task<bool> InsertBanAsync(DBAN ban)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    if (string.IsNullOrEmpty(ban.Id))
                    {
                        var maxId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT MAX(CAST(ID AS INTEGER)) FROM DBAN WHERE ID SIMILAR TO '[0-9]+'");
                        ban.Id = ((maxId ?? 0) + 1).ToString();
                    }

                    string sql = @"
                        INSERT INTO DBAN (ID, NAME, NOTE, DKHUVUCID, DNHOMHIENTHIID, DLOAIPHONGID, STATUS, USERCREATEDID, TIMECREATED) 
                        VALUES (@Id, @Name, @Note, @DkhuvucId, @DnhomhienthiId, @DloaiphongId, 1, 1, CURRENT_TIMESTAMP)";
                    
                    var rows = await conn.ExecuteAsync(sql, ban);
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateBanAsync(DBAN ban)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        UPDATE DBAN 
                        SET NAME = @Name,
                            NOTE = @Note,
                            DKHUVUCID = @DkhuvucId,
                            DNHOMHIENTHIID = @DnhomhienthiId,
                            DLOAIPHONGID = @DloaiphongId,
                            USERMODIFIEDID = 1,
                            TIMEMODIFIED = CURRENT_TIMESTAMP
                        WHERE ID = @Id";
                    
                    var rows = await conn.ExecuteAsync(sql, ban);
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateBansColumnAsync(List<string> ids, string columnName, object value)
        {
            if (ids == null || ids.Count == 0) return true;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        string sql = $@"UPDATE DBAN SET {columnName} = @Value, USERMODIFIEDID = 1, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id";
                        foreach (var id in ids)
                        {
                            await conn.ExecuteAsync(sql, new { Value = value, Id = id }, transaction: trans);
                        }
                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật thuộc tính bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteBanAsync(string id, bool isPermanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql;
                    if (isPermanent)
                    {
                        sql = "DELETE FROM DBAN WHERE ID = @Id";
                    }
                    else
                    {
                        sql = "UPDATE DBAN SET STATUS = 0 WHERE ID = @Id";
                    }
                    var rows = await conn.ExecuteAsync(sql, new { Id = id });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteBansAsync(List<string> ids, bool isPermanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        string sql = isPermanent
                            ? "DELETE FROM DBAN WHERE ID = @Id"
                            : "UPDATE DBAN SET STATUS = 0 WHERE ID = @Id";
                        
                        foreach (var id in ids)
                        {
                            await conn.ExecuteAsync(sql, new { Id = id }, transaction: trans);
                        }
                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa danh sách bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RestoreBanAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE DBAN SET STATUS = 1 WHERE ID = @Id";
                    var rows = await conn.ExecuteAsync(sql, new { Id = id });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khôi phục bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<DBAN> GetBanByIdAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT ID as Id, 
                               NAME as Name, 
                               NOTE as Note, 
                               DKHUVUCID as DkhuvucId,
                               DNHOMHIENTHIID as DnhomhienthiId,
                               DLOAIPHONGID as DloaiphongId
                        FROM DBAN 
                        WHERE ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync<DBAN>(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi GetBanByIdAsync: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        public async Task<ObservableCollection<BieuTuongViewModel>> GetBieuTuongTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        SELECT ID as Id, 
                               NAME as Name, 
                               PARENTID as ParentId,
                               ANH as Anh
                        FROM DBIEUTUONG
                        ORDER BY ID";

                    var allItemsQuery = await conn.QueryAsync<dynamic>(sql);
                    var allItems = new List<BieuTuongViewModel>();
                    foreach (var d in allItemsQuery)
                    {
                        var vm = new BieuTuongViewModel
                        {
                            Id = d.ID != null ? Convert.ToInt32(d.ID) : 0,
                            Name = d.NAME,
                            ParentId = d.PARENTID != null ? Convert.ToInt32(d.PARENTID) : (int?)null,
                            Anh = d.ANH as byte[]
                        };
                        
                        if (vm.Anh != null && vm.Anh.Length > 0)
                        {
                            try
                            {
                                using (var ms = new System.IO.MemoryStream(vm.Anh))
                                {
                                    var image = new System.Windows.Media.Imaging.BitmapImage();
                                    image.BeginInit();
                                    image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                    image.StreamSource = ms;
                                    image.EndInit();
                                    image.Freeze();
                                    vm.ImageSource = image;
                                }
                            }
                            catch { }
                        }
                        
                        allItems.Add(vm);
                    }

                    var tree = new ObservableCollection<BieuTuongViewModel>();
                    var lookup = new Dictionary<int, BieuTuongViewModel>();

                    foreach (var item in allItems)
                    {
                        lookup[item.Id] = item;
                    }

                    foreach (var item in allItems)
                    {
                        if (item.ParentId == null)
                        {
                            tree.Add(item);
                        }
                        else
                        {
                            if (lookup.TryGetValue(item.ParentId.Value, out var parent))
                            {
                                parent.Children.Add(item);
                            }
                            else
                            {
                                tree.Add(item);
                            }
                        }
                    }

                    return tree;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách biểu tượng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<BieuTuongViewModel>();
            }
        }

        public async Task<bool> InsertBieuTuongAsync(DBIEUTUONG item)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        INSERT INTO DBIEUTUONG (NAME, PARENTID, ANH)
                        VALUES (@Name, @ParentId, @Anh)
                        RETURNING ID";

                    var result = await conn.QueryFirstOrDefaultAsync<int>(sql, new { item.Name, item.ParentId, item.Anh });
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm biểu tượng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteBieuTuongAsync(int id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = "DELETE FROM DBIEUTUONG WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa biểu tượng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateKhuVucIconAsync(int khuVucId, int? bieuTuongId, byte[] anh)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        UPDATE DKHUVUC 
                        SET DBIEUTUONG_ID = @BieuTuongId,
                            ANH = @Anh
                        WHERE ID = @Id";

                    int affected = await conn.ExecuteAsync(sql, new { 
                        Id = khuVucId,
                        BieuTuongId = bieuTuongId,
                        Anh = anh
                    });

                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật biểu tượng khu vực: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RenameKhuVucAsync(int id, string newName)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        UPDATE DKHUVUC 
                        SET NAME = @NewName
                        WHERE ID = @Id";

                    int affected = await conn.ExecuteAsync(sql, new { Id = id, NewName = newName });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đổi tên khu vực: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}


