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
                               b.TIMECREATED as Timecreated,
                               CAST(b.USERCREATEDID AS VARCHAR(50)) as UsercreatedId,
                               b.TIMEMODIFIED as Timemodified,
                               CAST(b.USERMODIFIEDID AS VARCHAR(50)) as UsermodifiedId,
                               b.DONGIA as Dongia,
                               b.TIENMOBAN as Tienmoban,
                               k.NAME as KhuVucName,
                               h.NAME as NhomHienThiName,
                               p.NAME as LoaiPhongName,
                               bg.NAME as BanggiaName,
                               COALESCE(uc.NAME, uc.USERNAME, 'Administrator') as UsercreatedName,
                               COALESCE(um.NAME, um.USERNAME, 'Administrator') as UsermodifiedName
                        FROM DBAN b
                        LEFT JOIN DKHUVUC k ON b.DKHUVUCID = k.ID
                        LEFT JOIN DNHOMHIENTHI h ON b.DNHOMHIENTHIID = h.ID
                        LEFT JOIN DLOAIPHONG p ON b.DLOAIPHONGID = p.ID
                        LEFT JOIN DBANGGIA bg ON b.DBANGGIAID = bg.ID
                        LEFT JOIN SUSER uc ON CAST(b.USERCREATEDID AS VARCHAR(50)) = CAST(uc.ID AS VARCHAR(50))
                        LEFT JOIN SUSER um ON CAST(b.USERMODIFIEDID AS VARCHAR(50)) = CAST(um.ID AS VARCHAR(50))
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
                        SELECT b.ID as Id, 
                               b.NAME as Name, 
                               b.NOTE as Note, 
                               b.DKHUVUCID as DkhuvucId,
                               b.DNHOMHIENTHIID as DnhomhienthiId,
                               b.DLOAIPHONGID as DloaiphongId,
                               b.DBANGGIAID as DbanggiaId,
                               b.DONGIA as Dongia,
                               b.TIENMOBAN as Tienmoban,
                               b.TIMECREATED as Timecreated,
                               CAST(b.USERCREATEDID AS VARCHAR(50)) as UsercreatedId,
                               b.TIMEMODIFIED as Timemodified,
                               CAST(b.USERMODIFIEDID AS VARCHAR(50)) as UsermodifiedId
                        FROM DBAN b
                        WHERE b.ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync<DBAN>(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi GetBanByIdAsync: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<string> GetUserNameAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return "Administrator";
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    var name = await conn.QueryFirstOrDefaultAsync<string>(
                        "SELECT COALESCE(NAME, USERNAME) FROM SUSER WHERE CAST(ID AS VARCHAR(50)) = @UserId", 
                        new { UserId = userId });
                    return !string.IsNullOrEmpty(name) ? name : "Administrator";
                }
            }
            catch
            {
                return "Administrator";
            }
        }

        public async Task<IEnumerable<dynamic>> GetBangGiaTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<dynamic>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT COALESCE(bg.NAME, 'Bảng giá chuẩn') as BangGia,
                               b.DONGIA as DonGia,
                               '00:00' as GioBatDau,
                               '23:59' as GioKetThuc,
                               b.NOTE as GhiChu
                        FROM DBAN b
                        LEFT JOIN DBANGGIA bg ON CAST(b.DBANGGIAID AS VARCHAR(50)) = CAST(bg.ID AS VARCHAR(50))
                        WHERE CAST(b.ID AS VARCHAR(50)) = @BanId";
                    return await conn.QueryAsync(sql, new { BanId = banId });
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public async Task<IEnumerable<dynamic>> GetDatHangTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<dynamic>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NGAY as Ngay,
                               COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               COALESCE(d.TENKHACH, 'Khách lẻ') as TenKhach, 
                               d.DIACHI as DiaChi,
                               d.DIENTHOAI as DienThoai, 
                               d.EMAIL as Email,
                               CAST(COALESCE(d.TONGCONG, d.TIENHANG, 0) AS DECIMAL(18,2)) as TongCong,
                               COALESCE(pt.NAME, '') as PhuongThucDat,
                               COALESCE(md.NAME, '') as MucDichDat,
                               d.TUGIO as TuGio,
                               d.DENGIO as DenGio,
                               d.TUNGAY as TuNgay,
                               d.DENNGAY as DenNgay,
                               d.NOTE as GhiChu
                        FROM TDATHANG d
                        LEFT JOIN DPHUONGTHUCDAT pt ON CAST(d.DPHUONGTHUCDATID AS VARCHAR(50)) = CAST(pt.ID AS VARCHAR(50))
                        LEFT JOIN DMUCDICHDAT md ON CAST(d.DMUCDICHDATID AS VARCHAR(50)) = CAST(md.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    return await conn.QueryAsync(sql, new { BanId = banId });
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public async Task<IEnumerable<dynamic>> GetHoaDonTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<dynamic>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT COALESCE(d.SOHD, d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(u.NAME, u.USERNAME, 'Thu ngân') as NhanVien, 
                               COALESCE(kh.NAME, 'Khách lẻ') as KhachHang, 
                               CAST(d.TIENHANG AS DECIMAL(18,2)) as TongTien, 
                               CAST(d.TIENGIAMGIA AS DECIMAL(18,2)) as GiamGia, 
                               CAST(d.TIENTHANHTOAN AS DECIMAL(18,2)) as ThanhToan, 
                               COALESCE(d.LOAITHANHTOAN, 'Tiền mặt') as HinhThuc, 
                               d.NOTE as GhiChu
                        FROM TDONHANG d
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    return await conn.QueryAsync(sql, new { BanId = banId });
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public async Task<IEnumerable<dynamic>> GetXuatKhoTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<dynamic>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(k.NAME, 'Kho chính') as KhoHang, 
                               COALESCE(kh.NAME, 'Khách lẻ') as KhachHang, 
                               COALESCE(u.NAME, u.USERNAME, 'Nhân viên') as NhanVien, 
                               CAST(d.TIENHANG AS DECIMAL(18,2)) as TongTien, 
                               CAST(d.TIENTHANHTOAN AS DECIMAL(18,2)) as DaThanhToan, 
                               CAST(d.CONNO AS DECIMAL(18,2)) as ConNo, 
                               d.NOTE as GhiChu
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    return await conn.QueryAsync(sql, new { BanId = banId });
                }
            }
            catch
            {
                return new List<dynamic>();
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


