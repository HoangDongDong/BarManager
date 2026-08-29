using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Dapper;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalKhachDatHangService
    {
        public async Task<ObservableCollection<TreeCategoryViewModel>> GetTreeAsync(bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    
                    string sql = $@"
                        SELECT a.ID as Id, 
                               a.NAME as Name, 
                               a.PARENTID as ParentId, 
                               a.PARENTDIR as ParentDir, 
                               a.SORTORDER as SortOrder,
                               a.NOTE as Note,
                               a.SIMAGEID as SimageId,
                               b.IMAGE as AnhBytes
                        FROM {tableName} a
                        LEFT JOIN SIMAGE b ON a.SIMAGEID = b.ID
                        WHERE (a.STATUS <> 0 OR a.STATUS IS NULL)
                        ORDER BY a.SORTORDER, a.NAME";

                    var rawItems = await conn.QueryAsync<dynamic>(sql);
                    var allItems = new List<TreeCategoryViewModel>();

                    foreach (var raw in rawItems)
                    {
                        var item = new TreeCategoryViewModel
                        {
                            Id = raw.ID?.ToString(),
                            Name = raw.NAME,
                            ParentId = raw.PARENTID?.ToString(),
                            ParentDir = raw.PARENTDIR,
                            SortOrder = raw.SORTORDER?.ToString(),
                            Note = raw.NOTE,
                            SimageId = raw.SIMAGEID?.ToString()
                        };

                        if (raw.ANHBYTES != null)
                        {
                            try
                            {
                                byte[] bytes = (byte[])raw.ANHBYTES;
                                using (var ms = new MemoryStream(bytes))
                                {
                                    var img = new BitmapImage();
                                    img.BeginInit();
                                    img.CacheOption = BitmapCacheOption.OnLoad;
                                    img.StreamSource = ms;
                                    img.EndInit();
                                    img.Freeze();
                                    item.ImageSource = img;
                                }
                            }
                            catch { }
                        }
                        allItems.Add(item);
                    }

                    var tree = new ObservableCollection<TreeCategoryViewModel>();
                    var lookup = new Dictionary<string, TreeCategoryViewModel>();

                    var rootNode = new TreeCategoryViewModel { Id = null, Name = "Tất cả" };
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

                    // Thêm nút Thùng rác
                    var trashNode = new TreeCategoryViewModel
                    {
                        Id = "-1",
                        Name = "Thùng rác"
                    };
                    rootNode.Children.Add(trashNode);

                    return tree;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cây danh mục: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<TreeCategoryViewModel>();
            }
        }

        public async Task<List<LookupItem>> GetLookupListAsync(bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    string sql = $"SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM {tableName} WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME";
                    var result = await conn.QueryAsync<LookupItem>(sql);
                    return result.ToList();
                }
            }
            catch
            {
                return new List<LookupItem>();
            }
        }

        public async Task<List<KhachHangLookupViewModel>> GetKhachHangLookupAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT CAST(ID AS VARCHAR(50)) as Id,
                               NAME as Name,
                               MAKHACH as Makhach,
                               CAST(DIACHI AS VARCHAR(255)) as Diachi,
                               DIENTHOAI as Dienthoai
                        FROM DKHACHHANG
                        WHERE STATUS <> 0 OR STATUS IS NULL
                        ORDER BY NAME";
                    var items = (await conn.QueryAsync<KhachHangLookupViewModel>(sql)).ToList();
                    items.Insert(0, new KhachHangLookupViewModel { Id = null, Name = "", Makhach = "", Diachi = "", Dienthoai = "" });
                    return items;
                }
            }
            catch
            {
                return new List<KhachHangLookupViewModel> { new KhachHangLookupViewModel { Id = null, Name = "" } };
            }
        }

        public async Task<string> SaveDatHangAsync(DatHangSaveParam datHang, List<DatHangChiTietViewModel> chiTiets)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string orderId;
                            if (!string.IsNullOrEmpty(datHang.Id))
                            {
                                orderId = datHang.Id;
                                string updateSql = @"
                                    UPDATE TDATHANG SET 
                                        NAME = @Name,
                                        NGAY = @Ngay,
                                        TENKHACH = @Tenkhach,
                                        DIACHI = @Diachi,
                                        DIENTHOAI = @Dienthoai,
                                        EMAIL = @Email,
                                        TIENHANG = @Tienhang,
                                        TILEGIAMGIA = @Tilegiamgia,
                                        TIENGIAMGIA = @Tiengiamgia,
                                        TILETHUE = @Tilethue,
                                        TIENTHUE = @Tienthue,
                                        PHIVANCHUYEN = @Phivanchuyen,
                                        TONGCONG = @Tongcong,
                                        DKHACHHANGID = @DkhachhangId,
                                        DPHUONGTHUCDATID = @DphuongthucdatId,
                                        DMUCDICHDATID = @DmucdichdatId,
                                        TUGIO = @Tugio,
                                        DENGIO = @Dengio,
                                        TUNGAY = @Tungay,
                                        DENNGAY = @Denngay,
                                        GIODAT = @Giodat,
                                        NOTE = @Note,
                                        DBANID = @DbanId,
                                        STATUS = 1,
                                        USERMODIFIEDID = 1,
                                        TIMEMODIFIED = CURRENT_TIMESTAMP
                                    WHERE CAST(ID AS VARCHAR(50)) = @Id";

                                await conn.ExecuteAsync(updateSql, datHang, transaction: trans);
                                await conn.ExecuteAsync("DELETE FROM TDATHANGCHITIET WHERE CAST(TDATHANGID AS VARCHAR(50)) = @DatHangId", new { DatHangId = orderId }, transaction: trans);
                            }
                            else
                            {
                                orderId = Guid.NewGuid().ToString();
                                datHang.Id = orderId;

                                string insertSql = @"
                                    INSERT INTO TDATHANG (
                                        ID, NAME, NGAY, TENKHACH, DIACHI, DIENTHOAI, EMAIL,
                                        TIENHANG, TILEGIAMGIA, TIENGIAMGIA, TILETHUE, TIENTHUE,
                                        PHIVANCHUYEN, TONGCONG, DKHACHHANGID, DPHUONGTHUCDATID, DMUCDICHDATID,
                                        TUGIO, DENGIO, TUNGAY, DENNGAY, GIODAT, NOTE, DBANID, STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @Id, @Name, @Ngay, @Tenkhach, @Diachi, @Dienthoai, @Email,
                                        @Tienhang, @Tilegiamgia, @Tiengiamgia, @Tilethue, @Tienthue,
                                        @Phivanchuyen, @Tongcong, @DkhachhangId, @DphuongthucdatId, @DmucdichdatId,
                                        @Tugio, @Dengio, @Tungay, @Denngay, @Giodat, @Note, @DbanId, 1, 1, CURRENT_TIMESTAMP
                                    )";

                                await conn.ExecuteAsync(insertSql, datHang, transaction: trans);
                            }

                            if (chiTiets != null && chiTiets.Count > 0)
                            {
                                string insertDetailSql = @"
                                    INSERT INTO TDATHANGCHITIET (
                                        ID, TDATHANGID, DMATHANGID, SOLUONG, DONGIA, TILEGIAMGIA, THANHTIEN, NOTE, STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @Id, @TdathangId, @DmathangId, @Soluong, @Dongia, @Tilegiamgia, @Thanhtien, @Note, 1, 1, CURRENT_TIMESTAMP
                                    )";

                                foreach (var ct in chiTiets)
                                {
                                    string detailId = Guid.NewGuid().ToString();

                                    await conn.ExecuteAsync(insertDetailSql, new
                                    {
                                        Id = detailId,
                                        TdathangId = orderId,
                                        DmathangId = ct.MatHangId,
                                        Soluong = ct.SoLuong ?? 1,
                                        Dongia = ct.DonGia ?? 0,
                                        Tilegiamgia = ct.GiamGiaPhanTram ?? 0,
                                        Thanhtien = ct.ThanhTien ?? 0,
                                        Note = ct.GhiChu
                                    }, transaction: trans);
                                }
                            }

                            trans.Commit();
                            return orderId;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu đơn đặt hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public class DatHangSaveParam
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime? Ngay { get; set; }
            public string Tenkhach { get; set; }
            public string Diachi { get; set; }
            public string Dienthoai { get; set; }
            public string Email { get; set; }
            public decimal? Tienhang { get; set; }
            public decimal? Tilegiamgia { get; set; }
            public decimal? Tiengiamgia { get; set; }
            public decimal? Tilethue { get; set; }
            public decimal? Tienthue { get; set; }
            public string Phivanchuyen { get; set; }
            public string Tongcong { get; set; }
            public string DkhachhangId { get; set; }
            public string DphuongthucdatId { get; set; }
            public string DmucdichdatId { get; set; }
            public DateTime? Tugio { get; set; }
            public DateTime? Dengio { get; set; }
            public DateTime? Tungay { get; set; }
            public DateTime? Denngay { get; set; }
            public DateTime? Giodat { get; set; }
            public string Note { get; set; }
            public string DbanId { get; set; }
        }

        public async Task<List<DatHangViewModel>> GetDatHangListAsync(string categoryId, bool isMucDichDat, DateTime? tuNgay = null, DateTime? denNgay = null, string soPhieu = null, string khachHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT d.ID as Id, 
                               d.NGAY as Ngay,
                               d.NAME as SoPhieu,
                               d.TENKHACH as TenKhach,
                               CAST(d.DIACHI AS VARCHAR(255)) as DiaChi,
                               d.DIENTHOAI as DienThoai,
                               d.EMAIL as Email,
                               d.TONGCONG as TongCong,
                               p.NAME as PhuongThucDatName,
                               m.NAME as MucDichDatName,
                               d.TUGIO as TuGio,
                               d.DENGIO as DenGio,
                               d.TUNGAY as TuNgay,
                               d.DENNGAY as DenNgay,
                               d.TIMECREATED as Timecreated,
                               d.TIMEMODIFIED as Timemodified,
                               COALESCE(uc.NAME, 'Administrator') as UsercreatedName,
                               COALESCE(um.NAME, 'Administrator') as UsermodifiedName
                        FROM TDATHANG d
                        LEFT JOIN DPHUONGTHUCDAT p ON CAST(d.DPHUONGTHUCDATID AS VARCHAR(50)) = CAST(p.ID AS VARCHAR(50))
                        LEFT JOIN DMUCDICHDAT m ON CAST(d.DMUCDICHDATID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN SUSER uc ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(uc.ID AS VARCHAR(50))
                        LEFT JOIN SUSER um ON CAST(d.USERMODIFIEDID AS VARCHAR(50)) = CAST(um.ID AS VARCHAR(50))
                        WHERE 1=1 ";

                    var parameters = new DynamicParameters();

                    if (categoryId == "-1")
                    {
                        sql += " AND (d.STATUS = 0) ";
                    }
                    else
                    {
                        sql += " AND (d.STATUS <> 0 OR d.STATUS IS NULL) ";
                        if (!string.IsNullOrEmpty(categoryId))
                        {
                            if (isMucDichDat)
                            {
                                sql += " AND CAST(d.DMUCDICHDATID AS VARCHAR(50)) = @CategoryId";
                            }
                            else
                            {
                                sql += " AND CAST(d.DPHUONGTHUCDATID AS VARCHAR(50)) = @CategoryId";
                            }
                            parameters.Add("CategoryId", categoryId);
                        }
                    }
                    if (tuNgay.HasValue)
                    {
                        sql += " AND d.NGAY >= @TuNgay";
                        parameters.Add("TuNgay", tuNgay.Value.Date);
                    }
                    if (denNgay.HasValue)
                    {
                        sql += " AND d.NGAY <= @DenNgay";
                        parameters.Add("DenNgay", denNgay.Value.Date.AddDays(1).AddSeconds(-1));
                    }
                    if (!string.IsNullOrWhiteSpace(soPhieu))
                    {
                        string cleanCode = soPhieu.Trim();
                        string digitsOnly = new string(cleanCode.Where(char.IsDigit).ToArray());

                        sql += " AND (UPPER(d.NAME) LIKE '%' || UPPER(@SoPhieu) || '%'";
                        parameters.Add("SoPhieu", cleanCode);

                        if (!string.IsNullOrEmpty(digitsOnly))
                        {
                            sql += " OR UPPER(d.NAME) LIKE '%' || @Digits || '%'";
                            parameters.Add("Digits", digitsOnly);

                            if (int.TryParse(digitsOnly, out int num))
                            {
                                sql += " OR UPPER(d.NAME) LIKE '%' || @Format5 || '%' OR UPPER(d.NAME) LIKE '%' || @Format4 || '%'";
                                parameters.Add("Format5", num.ToString("D5"));
                                parameters.Add("Format4", num.ToString("D4"));
                            }
                        }
                        sql += ")";
                    }
                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        parameters.Add("KhachHangId", khachHangId);
                    }

                    sql += " ORDER BY d.NGAY DESC";

                    var result = await conn.QueryAsync<DatHangViewModel>(sql, parameters);
                    var list = result.ToList();
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Stt = i + 1;
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách đặt hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DatHangViewModel>();
            }
        }

        public async Task<List<DatHangChiTietViewModel>> GetDatHangChiTietListAsync(string datHangId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT dc.ID as Id, 
                               m.NAME as MatHangName,
                               COALESCE(dvt.NAME, dvtm.NAME) as DonViTinhName,
                               dc.SOLUONG as SoLuong,
                               dc.DONGIA as DonGia,
                               dc.TILEGIAMGIA as GiamGiaPhanTram,
                               dc.THANHTIEN as ThanhTien,
                               CAST(dc.NOTE AS VARCHAR(255)) as GhiChu,
                               m.CODE as MaHang
                        FROM TDATHANGCHITIET dc
                        LEFT JOIN DMATHANG m ON dc.DMATHANGID = m.ID
                        LEFT JOIN DDONVITINH dvt ON dc.DDONVITINHID = dvt.ID
                        LEFT JOIN DDONVITINH dvtm ON m.DDONVITINHID = dvtm.ID
                        WHERE CAST(dc.TDATHANGID AS VARCHAR(50)) = @DatHangId";

                    var result = await conn.QueryAsync<DatHangChiTietViewModel>(sql, new { DatHangId = datHangId });
                    
                    var chiTiet = result.ToList();
                    
                    for (int i = 0; i < chiTiet.Count; i++)
                    {
                        chiTiet[i].Stt = i + 1;
                    }

                    return chiTiet;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết đơn hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DatHangChiTietViewModel>();
            }
        }

        public async Task<DatHangSaveParam> GetDatHangByIdAsync(string orderId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT CAST(ID AS VARCHAR(50)) as Id, 
                               NAME as Name, 
                               NGAY as Ngay, 
                               TENKHACH as Tenkhach,
                               CAST(DIACHI AS VARCHAR(255)) as Diachi, 
                               DIENTHOAI as Dienthoai, 
                               EMAIL as Email,
                               TIENHANG as Tienhang, 
                               TILEGIAMGIA as Tilegiamgia, 
                               TIENGIAMGIA as Tiengiamgia,
                               TILETHUE as Tilethue, 
                               TIENTHUE as Tienthue, 
                               PHIVANCHUYEN as Phivanchuyen,
                               TONGCONG as Tongcong, 
                               CAST(DKHACHHANGID AS VARCHAR(50)) as DkhachhangId,
                               CAST(DPHUONGTHUCDATID AS VARCHAR(50)) as DphuongthucdatId,
                               CAST(DMUCDICHDATID AS VARCHAR(50)) as DmucdichdatId,
                               TUGIO as Tugio, 
                               DENGIO as Dengio, 
                               TUNGAY as Tungay, 
                               DENNGAY as Denngay,
                               GIODAT as Giodat, 
                               CAST(NOTE AS VARCHAR(255)) as Note, 
                               CAST(DBANID AS VARCHAR(50)) as DbanId
                        FROM TDATHANG
                        WHERE CAST(ID AS VARCHAR(50)) = @Id";
                    return await conn.QueryFirstOrDefaultAsync<DatHangSaveParam>(sql, new { Id = orderId });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin đơn đặt hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<List<SImageViewModel>> GetSImagesAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID as Id, IMAGE as ImageBytes FROM SIMAGE";
                    var rawItems = await conn.QueryAsync<dynamic>(sql);
                    var list = new List<SImageViewModel>();

                    foreach (var raw in rawItems)
                    {
                        var item = new SImageViewModel { Id = raw.ID?.ToString(), ImageBytes = raw.IMAGEBYTES };
                        if (item.ImageBytes != null)
                        {
                            try
                            {
                                using (var ms = new MemoryStream(item.ImageBytes))
                                {
                                    var img = new BitmapImage();
                                    img.BeginInit();
                                    img.CacheOption = BitmapCacheOption.OnLoad;
                                    img.StreamSource = ms;
                                    img.EndInit();
                                    img.Freeze();
                                    item.ImageSource = img;
                                }
                            }
                            catch { }
                        }
                        list.Add(item);
                    }
                    return list;
                }
            }
            catch
            {
                return new List<SImageViewModel>();
            }
        }

        public async Task<bool> InsertPhuongThucDatAsync(string name, string note, string simageId, string parentId, bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    string typeName = isMucDichDat ? "Mục đích đặt" : "Phương thức đặt";

                    // Kiểm tra trùng tên
                    if (!string.IsNullOrWhiteSpace(name) && name.Trim() != "----------------")
                    {
                        string checkSql = $"SELECT COUNT(1) FROM {tableName} WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name)) AND (STATUS <> 0 OR STATUS IS NULL)";
                        int count = await conn.ExecuteScalarAsync<int>(checkSql, new { Name = name.Trim() });
                        if (count > 0)
                        {
                            MessageBox.Show($"{typeName} '{name.Trim()}' đã tồn tại trong hệ thống! Vui lòng nhập tên khác.", "Cảnh báo trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                    
                    int maxSortOrder = await conn.QueryFirstOrDefaultAsync<int>($"SELECT COALESCE(MAX(SORTORDER), 0) FROM {tableName} WHERE PARENTID IS NOT DISTINCT FROM @ParentId", new { ParentId = parentId });
                    
                    string sql = $@"
                        INSERT INTO {tableName} (ID, NAME, NOTE, SIMAGEID, PARENTID, SORTORDER, STATUS, USERCREATEDID, TIMECREATED)
                        VALUES (@Id, @Name, @Note, @SimageId, @ParentId, @SortOrder, 1, 1, CURRENT_TIMESTAMP)";
                        
                    string newId = Guid.NewGuid().ToString();
                    int affected = await conn.ExecuteAsync(sql, new { Id = newId, Name = name, Note = note, SimageId = simageId, ParentId = string.IsNullOrEmpty(parentId) ? null : parentId, SortOrder = maxSortOrder + 1 });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdatePhuongThucDatAsync(string id, string name, string note, string simageId, bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    string typeName = isMucDichDat ? "Mục đích đặt" : "Phương thức đặt";

                    // Kiểm tra trùng tên khi sửa
                    if (!string.IsNullOrWhiteSpace(name) && name.Trim() != "----------------")
                    {
                        string checkSql = $"SELECT COUNT(1) FROM {tableName} WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name)) AND CAST(ID AS VARCHAR(50)) <> @Id AND (STATUS <> 0 OR STATUS IS NULL)";
                        int count = await conn.ExecuteScalarAsync<int>(checkSql, new { Name = name.Trim(), Id = id });
                        if (count > 0)
                        {
                            MessageBox.Show($"{typeName} '{name.Trim()}' đã tồn tại trong hệ thống! Vui lòng nhập tên khác.", "Cảnh báo trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }

                    string sql = $@"
                        UPDATE {tableName} 
                        SET NAME = @Name, NOTE = @Note, SIMAGEID = @SimageId,
                            TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = 1
                        WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, Note = note, SimageId = simageId });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeletePhuongThucDatAsync(string id, bool isMucDichDat, bool isPermanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    if (isPermanent)
                    {
                        string sql = $"DELETE FROM {tableName} WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = $"UPDATE {tableName} SET STATUS = 0, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = 1 WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RestorePhuongThucDatAsync(string id, bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    string sql = $"UPDATE {tableName} SET STATUS = 1, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = 1 WHERE CAST(ID AS VARCHAR(50)) = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khôi phục danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteDatHangAsync(string orderId, bool isPermanent)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    if (isPermanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM TDATHANGCHITIET WHERE CAST(TDATHANGID AS VARCHAR(50)) = @Id", new { Id = orderId });
                        await conn.ExecuteAsync("DELETE FROM TDATHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = orderId });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE TDATHANG SET STATUS = 0, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = 1 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = orderId });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa đơn đặt hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RestoreDatHangAsync(string orderId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    await conn.ExecuteAsync("UPDATE TDATHANG SET STATUS = 1, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = 1 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = orderId });
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khôi phục đơn đặt hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    await conn.ExecuteAsync("DELETE FROM TDATHANGCHITIET WHERE CAST(TDATHANGID AS VARCHAR(50)) IN (SELECT CAST(ID AS VARCHAR(50)) FROM TDATHANG WHERE STATUS = 0)");
                    await conn.ExecuteAsync("DELETE FROM TDATHANG WHERE STATUS = 0");
                    await conn.ExecuteAsync("DELETE FROM DPHUONGTHUCDAT WHERE STATUS = 0");
                    await conn.ExecuteAsync("DELETE FROM DMUCDICHDAT WHERE STATUS = 0");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dọn sạch thùng rác: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdatePhuongThucDatParentAsync(string id, string newParentId, bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    
                    int maxSortOrder = await conn.QueryFirstOrDefaultAsync<int>($"SELECT COALESCE(MAX(SORTORDER), 0) FROM {tableName} WHERE PARENTID IS NOT DISTINCT FROM @ParentId", new { ParentId = newParentId });
                    
                    string sql = $@"
                        UPDATE {tableName} 
                        SET PARENTID = @ParentId, SORTORDER = @SortOrder
                        WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id, ParentId = string.IsNullOrEmpty(newParentId) ? null : newParentId, SortOrder = maxSortOrder + 1 });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi chuyển danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
