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

                    return tree;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cây danh mục: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<TreeCategoryViewModel>();
            }
        }

        public async Task<List<DatHangViewModel>> GetDatHangListAsync(string categoryId, bool isMucDichDat, DateTime? tuNgay = null, DateTime? denNgay = null)
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
                               d.DENNGAY as DenNgay
                        FROM TDATHANG d
                        LEFT JOIN DPHUONGTHUCDAT p ON d.DPHUONGTHUCDATID = p.ID
                        LEFT JOIN DMUCDICHDAT m ON d.DMUCDICHDATID = m.ID
                        WHERE 1=1 ";

                    var parameters = new DynamicParameters();

                    if (!string.IsNullOrEmpty(categoryId))
                    {
                        if (isMucDichDat)
                        {
                            sql += " AND d.DMUCDICHDATID = @CategoryId";
                        }
                        else
                        {
                            sql += " AND d.DPHUONGTHUCDATID = @CategoryId";
                        }
                        parameters.Add("CategoryId", categoryId);
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
                               m.NAME as TenMatHang,
                               dvt.NAME as DonViTinh,
                               dc.SOLUONG as SoLuong,
                               dc.DONGIA as DonGia,
                               dc.THANHTIEN as ThanhTien,
                               CAST(dc.NOTE AS VARCHAR(255)) as GhiChu
                        FROM TDATHANGCHITIET dc
                        LEFT JOIN SMATHANG m ON dc.SMATHANGID = m.ID
                        LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                        WHERE dc.TDATHANGID = @DatHangId";

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
                    
                    int maxSortOrder = await conn.QueryFirstOrDefaultAsync<int>($"SELECT COALESCE(MAX(SORTORDER), 0) FROM {tableName} WHERE PARENTID IS NOT DISTINCT FROM @ParentId", new { ParentId = parentId });
                    
                    string sql = $@"
                        INSERT INTO {tableName} (NAME, NOTE, SIMAGEID, PARENTID, SORTORDER)
                        VALUES (@Name, @Note, @SimageId, @ParentId, @SortOrder)";
                        
                    int affected = await conn.ExecuteAsync(sql, new { Name = name, Note = note, SimageId = simageId, ParentId = string.IsNullOrEmpty(parentId) ? null : parentId, SortOrder = maxSortOrder + 1 });
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
                    string sql = $@"
                        UPDATE {tableName} 
                        SET NAME = @Name, NOTE = @Note, SIMAGEID = @SimageId
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

        public async Task<bool> DeletePhuongThucDatAsync(string id, bool isMucDichDat)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string tableName = isMucDichDat ? "DMUCDICHDAT" : "DPHUONGTHUCDAT";
                    string sql = $"DELETE FROM {tableName} WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa danh mục: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
