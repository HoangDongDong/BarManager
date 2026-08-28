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
                        var statusVal = d.STATUS?.ToString();
                        bool isDeleted = statusVal == "0" || statusVal == "False";
                        if (isDeleted) continue; // Khu vực đã xóa không đưa vào thùng rác

                        var vm = new KhuVucViewModel
                        {
                            Id = d.ID?.ToString(),
                            Name = d.NAME,
                            ParentId = d.PARENTID?.ToString(),
                            ParentDir = d.PARENTDIR,
                            SortOrder = d.SORTORDER?.ToString(),
                            Status = true
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

                    // Fake root node "Tất cả"
                    var rootNode = new KhuVucViewModel { Id = null, Name = "Tất cả", IsExpanded = true, IsSelected = true };
                    tree.Add(rootNode);

                    foreach (var item in allItems)
                    {
                        if (!string.IsNullOrEmpty(item.Id))
                        {
                            lookup[item.Id] = item;
                        }
                    }

                    foreach (var item in allItems)
                    {
                        if (string.IsNullOrEmpty(item.ParentId) || !lookup.ContainsKey(item.ParentId))
                        {
                            rootNode.Children.Add(item);
                        }
                        else
                        {
                            lookup[item.ParentId].Children.Add(item);
                        }
                    }

                    // Thêm "Thùng rác" (Nơi chứa các Bàn bị xóa)
                    var trashNode = new KhuVucViewModel { Id = "-1", Name = "Thùng rác", IsExpanded = false };
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
            if (string.IsNullOrWhiteSpace(name)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Kiểm tra trùng tên khu vực
                    var exists = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM DKHUVUC WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name)) AND (STATUS <> 0 OR STATUS IS NULL)",
                        new { Name = name.Trim() });

                    if (exists > 0)
                    {
                        MessageBox.Show($"Khu vực \"{name.Trim()}\" đã tồn tại trong hệ thống! Vui lòng chọn tên khác.", "Thông báo trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
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
                    
                    var rows = await conn.ExecuteAsync(sql, new { Id = newId, Name = name.Trim(), ParentId = parentId });
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
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(id)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Kiểm tra trùng tên khu vực với các khu vực khác
                    var exists = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM DKHUVUC WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name)) AND CAST(ID AS VARCHAR(50)) <> CAST(@Id AS VARCHAR(50)) AND (STATUS <> 0 OR STATUS IS NULL)",
                        new { Name = name.Trim(), Id = id });

                    if (exists > 0)
                    {
                        MessageBox.Show($"Tên khu vực \"{name.Trim()}\" đã tồn tại trong hệ thống! Vui lòng chọn tên khác.", "Thông báo trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    string sql = @"
                        UPDATE DKHUVUC 
                        SET NAME = @Name 
                        WHERE ID = @Id";
                    
                    var rows = await conn.ExecuteAsync(sql, new { Name = name.Trim(), Id = id });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteKhuVucAsync(string id, bool isPermanent = true)
        {
            if (string.IsNullOrEmpty(id)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    // Đưa các bàn trong khu vực này về trạng thái Chưa thiết lập (DKHUVUCID = NULL)
                    await conn.ExecuteAsync("UPDATE DBAN SET DKHUVUCID = NULL WHERE CAST(DKHUVUCID AS VARCHAR(50)) = @Id", new { Id = id });
                    // Xóa khu vực con nếu có
                    await conn.ExecuteAsync("DELETE FROM DKHUVUC WHERE CAST(PARENTID AS VARCHAR(50)) = @Id", new { Id = id });
                    // Xóa chính khu vực
                    var rows = await conn.ExecuteAsync("DELETE FROM DKHUVUC WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> EmptyTrashAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    await conn.ExecuteAsync("DELETE FROM DBAN WHERE STATUS = 0");
                    await conn.ExecuteAsync("DELETE FROM DKHUVUC WHERE STATUS = 0");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dọn sạch thùng rác: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (ids == null || ids.Count == 0) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        string sql = isPermanent
                            ? "DELETE FROM DBAN WHERE CAST(ID AS VARCHAR(50)) = @Id"
                            : "UPDATE DBAN SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        
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

        public async Task<List<BangGiaTabViewModel>> GetBangGiaTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<BangGiaTabViewModel>();
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
                    var res = await conn.QueryAsync<BangGiaTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<BangGiaTabViewModel>();
            }
        }

        public async Task<List<DatHangTabViewModel>> GetDatHangTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<DatHangTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NGAY as Ngay,
                               COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               COALESCE(d.TENKHACH, 'Khách lẻ') as TenKhach, 
                               CAST(d.DIACHI AS VARCHAR(255)) as DiaChi,
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
                    var res = await conn.QueryAsync<DatHangTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<DatHangTabViewModel>();
            }
        }

        public async Task<List<HoaDonTabViewModel>> GetHoaDonTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<HoaDonTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NOTE as GhiChu,
                               COALESCE(d.SOHD, d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(u.NAME, u.USERNAME, 'Thu ngân') as NhanVien, 
                               COALESCE(kh.NAME, 'Khách lẻ') as KhachHang, 
                               CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,2)) as TongTien, 
                               CAST(COALESCE(d.TIENGIAMGIA, 0) AS DECIMAL(18,2)) as GiamGia, 
                               CAST(COALESCE(d.TIENTHANHTOAN, 0) AS DECIMAL(18,2)) as ThanhToan, 
                               COALESCE(d.LOAITHANHTOAN, 'Tiền mặt') as HinhThuc
                        FROM TDONHANG d
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    var res = await conn.QueryAsync<HoaDonTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<HoaDonTabViewModel>();
            }
        }

        public async Task<List<KhoTabViewModel>> GetNhapKhoTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<KhoTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NOTE as GhiChu,
                               COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(k.NAME, 'Kho chính') as KhoHang,
                               COALESCE(ncc.NAME, 'Nhà cung cấp') as NhaCungCap,
                               COALESCE(u.NAME, u.USERNAME, 'Nhân viên') as NhanVien,
                               CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,2)) as TongTien, 
                               CAST(COALESCE(d.DATHANHTOAN, d.TIENTHANHTOAN, 0) AS DECIMAL(18,2)) as DaThanhToan, 
                               CAST(COALESCE(d.CONNO, 0) AS DECIMAL(18,2)) as ConNo
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    var res = await conn.QueryAsync<KhoTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<KhoTabViewModel>();
            }
        }

        public async Task<List<KhoTabViewModel>> GetXuatKhoTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<KhoTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NOTE as GhiChu,
                               COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(k.NAME, 'Kho chính') as KhoHang, 
                               COALESCE(kh.NAME, 'Khách lẻ') as KhachHang, 
                               COALESCE(u.NAME, u.USERNAME, 'Nhân viên') as NhanVien, 
                               CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,2)) as TongTien, 
                               CAST(COALESCE(d.DATHANHTOAN, d.TIENTHANHTOAN, 0) AS DECIMAL(18,2)) as DaThanhToan, 
                               CAST(COALESCE(d.CONNO, 0) AS DECIMAL(18,2)) as ConNo
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    var res = await conn.QueryAsync<KhoTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<KhoTabViewModel>();
            }
        }

        public async Task<List<KhoTabViewModel>> GetChuyenKhoTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<KhoTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NOTE as GhiChu,
                               COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(kx.NAME, 'Kho xuất') as TuKho, 
                               COALESCE(kn.NAME, 'Kho nhập') as DenKho, 
                               COALESCE(u.NAME, u.USERNAME, 'Nhân viên') as NhanVien, 
                               COALESCE(d.NOTE, '') as DienGiai
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG kx ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(kx.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG kn ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(kn.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    var res = await conn.QueryAsync<KhoTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<KhoTabViewModel>();
            }
        }

        public async Task<List<KiemKeTabViewModel>> GetKiemKeTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<KiemKeTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT d.NOTE as GhiChu,
                               COALESCE(d.NAME, CAST(d.ID AS VARCHAR(50))) as SoPhieu, 
                               d.NGAY as Ngay, 
                               COALESCE(k.NAME, 'Kho chính') as KhoHang, 
                               COALESCE(u.NAME, u.USERNAME, 'Nhân viên') as NhanVien, 
                               COALESCE(d.NOTE, '') as DienGiai,
                               COALESCE(d.VOUCHER, '') as Voucher,
                               COALESCE(nvg.NAME, '') as NhanVienGiaoHang,
                               CAST(COALESCE(d.TRICHNHANVIEN, '0') AS DECIMAL(18,2)) as TrichNhanVien,
                               COALESCE(ch.NAME, '') as CuaHang,
                               CAST(COALESCE(d.CONLAI, '0') AS DECIMAL(18,2)) as ConLai,
                               CAST(COALESCE(d.THANHTOAN, d.TIENTHANHTOAN, 0) AS DECIMAL(18,2)) as ThanhToan,
                               COALESCE(tk.NAME, '') as TaiKhoanNganHang,
                               COALESCE(d.VOUCHER, '') as MaVoucher,
                               COALESCE(d.THE, '') as TheTt
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nvg ON CAST(d.DNHANVIENGIAOID AS VARCHAR(50)) = CAST(nvg.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(d.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                        WHERE CAST(d.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY d.NGAY DESC";
                    var res = await conn.QueryAsync<KiemKeTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<KiemKeTabViewModel>();
            }
        }

        public async Task<List<SuaChuaTabViewModel>> GetSuaChuaTheoBanAsync(string banId)
        {
            if (string.IsNullOrEmpty(banId)) return new List<SuaChuaTabViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT sc.NOTE as GhiChu,
                               COALESCE(sc.NAME, CAST(sc.ID AS VARCHAR(50))) as SoPhieu, 
                               sc.NGAY as Ngay, 
                               COALESCE(b.NAME, 'Bàn') as Phong,
                               CASE WHEN sc.DASUAXONG = '1' OR sc.DASUAXONG = 'true' THEN 1 ELSE 0 END as DaSuaXong,
                               COALESCE(sc.NOIDUNG, '') as NoiDung,
                               COALESCE(lp.NAME, '') as LoaiPhong,
                               COALESCE(nv.NAME, 'Nhân viên') as NhanVien,
                               CASE WHEN sc.CONSUDUNGDUOC = '1' OR sc.CONSUDUNGDUOC = 'true' THEN 1 ELSE 0 END as ConSuDungDuoc
                        FROM TSUACHUA sc
                        LEFT JOIN DBAN b ON CAST(sc.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DLOAIPHONG lp ON CAST(sc.DLOAIPHONGID AS VARCHAR(50)) = CAST(lp.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(sc.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE CAST(sc.DBANID AS VARCHAR(50)) = @BanId
                        ORDER BY sc.NGAY DESC";
                    var res = await conn.QueryAsync<SuaChuaTabViewModel>(sql, new { BanId = banId });
                    return res.ToList();
                }
            }
            catch
            {
                return new List<SuaChuaTabViewModel>();
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
            if (string.IsNullOrWhiteSpace(newName)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Kiểm tra trùng tên khu vực
                    var exists = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM DKHUVUC WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@NewName)) AND ID <> @Id AND (STATUS <> 0 OR STATUS IS NULL)",
                        new { NewName = newName.Trim(), Id = id });

                    if (exists > 0)
                    {
                        MessageBox.Show($"Tên khu vực \"{newName.Trim()}\" đã tồn tại trong hệ thống! Vui lòng chọn tên khác.", "Thông báo trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    string sql = @"
                        UPDATE DKHUVUC 
                        SET NAME = @NewName
                        WHERE ID = @Id";

                    int affected = await conn.ExecuteAsync(sql, new { Id = id, NewName = newName.Trim() });
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


