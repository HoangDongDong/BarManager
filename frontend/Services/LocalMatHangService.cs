using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalMatHangService
    {
        public async Task<ObservableCollection<NhomMatHangViewModel>> GetNhomMatHangTreeAsync()
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
                               PARENTDIR as ParentDir, 
                               SORTORDER as SortOrder, 
                               STATUS as Status
                        FROM DNHOMMATHANG
                        ORDER BY PARENTDIR, SORTORDER, NAME";

                    var allGroups = (await conn.QueryAsync<NhomMatHangViewModel>(sql)).ToList();

                    return BuildTree(allGroups);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải cây nhóm mặt hàng: " + ex.Message);
                return new ObservableCollection<NhomMatHangViewModel>();
            }
        }

        private ObservableCollection<NhomMatHangViewModel> BuildTree(List<NhomMatHangViewModel> flatList)
        {
            var tree = new ObservableCollection<NhomMatHangViewModel>();
            var lookup = flatList.ToDictionary(g => g.Id);

            foreach (var item in flatList)
            {
                if (!string.IsNullOrEmpty(item.ParentId) && lookup.ContainsKey(item.ParentId))
                {
                    lookup[item.ParentId].Children.Add(item);
                }
                else
                {
                    tree.Add(item);
                }
            }

            // Thêm nút "Tất cả" ở đầu
            var root = new NhomMatHangViewModel
            {
                Id = string.Empty,
                Name = "Tất cả",
                Children = tree
            };

            return new ObservableCollection<NhomMatHangViewModel> { root };
        }

        public async Task<List<MatHangViewModel>> GetMatHangListAsync(string nhomId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT m.ID as Id, 
                               m.CODE as Code, 
                               m.NAME as Name, 
                               m.GIANHAP as Gianhap, 
                               m.GIABAN as Giaban, 
                               m.GIABANCHAN as Giabanchan, 
                               m.QUYDOI as Quydoi, 
                               m.TAMKHOA as Tamkhoa, 
                               m.GIATHEOTHOIGIA as Giatheothoigia,
                               m.DNHOMMATHANGID as DnhommathangId, 
                               n.NAME as NhomMatHangName,
                               m.DLOAIMATHANGID as DloaimathangId,
                               l.NAME as LoaiMatHangName,
                               m.DDONVITINHID as DdonvitinhId,
                               d.NAME as DonViTinhName,
                               m.DDONVITINHCHANID as DdonvitinhchanId,
                               dc.NAME as DonViTinhChanName
                        FROM DMATHANG m
                        LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID
                        LEFT JOIN DLOAIMATHANG l ON m.DLOAIMATHANGID = l.ID
                        LEFT JOIN DDONVITINH d ON m.DDONVITINHID = d.ID
                        LEFT JOIN DDONVITINH dc ON m.DDONVITINHCHANID = dc.ID
                        WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(nhomId))
                    {
                        sql += " AND (m.DNHOMMATHANGID = @NhomId OR n.PARENTID = @NhomId OR n.PARENTDIR LIKE '%' || @NhomId || ',%')";
                    }

                    sql += " ORDER BY m.NAME";

                    var result = await conn.QueryAsync<MatHangViewModel>(sql, new { NhomId = nhomId });
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
                System.Windows.MessageBox.Show("Lỗi tải danh sách mặt hàng: " + ex.Message);
                return new List<MatHangViewModel>();
            }
        } // <-- Add this closing bracket

        public async Task<List<DDONVITINH>> GetDonViTinhListAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID as Id, NAME as Name FROM DDONVITINH ORDER BY NAME";
                    var result = await conn.QueryAsync<DDONVITINH>(sql);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải danh mục đơn vị tính: " + ex.Message);
                return new List<DDONVITINH>();
            }
        }

        public async Task<bool> InsertDonViTinhAsync(DDONVITINH model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        INSERT INTO DDONVITINH (
                            ID, NAME, STATUS, USERCREATEDID, TIMECREATED
                        ) VALUES (
                            @Id, @Name, 1, 1, CURRENT_TIMESTAMP
                        )";

                    var parameters = new {
                        Id = model.Id,
                        Name = model.Name
                    };

                    int affectedRows = await Dapper.SqlMapper.ExecuteAsync(conn, sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi thêm đơn vị tính: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteDonViTinhAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = "DELETE FROM DDONVITINH WHERE ID = @Id";
                    var parameters = new { Id = id };

                    int affectedRows = await Dapper.SqlMapper.ExecuteAsync(conn, sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi xóa đơn vị tính: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> InsertMatHangAsync(MatHangViewModel model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        INSERT INTO DMATHANG (
                            ID, CODE, NAME, GIANHAP, GIABAN, QUYDOI, 
                            GIATHEOTHOIGIA, DNHOMMATHANGID, DDONVITINHID,
                            STATUS, USERCREATEDID, TIMECREATED
                        ) VALUES (
                            @Id, @Code, @Name, @Gianhap, @Giaban, @Quydoi, 
                            @Giatheothoigia, @DnhommathangId, @DdonvitinhId,
                            1, 1, CURRENT_TIMESTAMP
                        )";

                    var parameters = new {
                        Id = model.Id,
                        Code = model.Code,
                        Name = model.Name,
                        Gianhap = model.Gianhap,
                        Giaban = model.Giaban,
                        Quydoi = model.Quydoi,
                        Giatheothoigia = model.Giatheothoigia,
                        DnhommathangId = model.DnhommathangId,
                        DdonvitinhId = model.DdonvitinhId
                    };

                    int affectedRows = await conn.ExecuteAsync(sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi thêm mặt hàng: " + ex.Message);
                return false;
            }
        }
        public async Task<MatHangViewModel> GetMatHangByIdAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT m.ID as Id, 
                               m.CODE as Code, 
                               m.NAME as Name, 
                               m.GIANHAP as Gianhap, 
                               m.GIABAN as Giaban, 
                               m.GIABANCHAN as Giabanchan, 
                               m.QUYDOI as Quydoi, 
                               m.TAMKHOA as Tamkhoa, 
                               m.GIATHEOTHOIGIA as Giatheothoigia,
                               m.DNHOMMATHANGID as DnhommathangId, 
                               m.DLOAIMATHANGID as DloaimathangId,
                               m.DDONVITINHID as DdonvitinhId,
                               m.DDONVITINHCHANID as DdonvitinhchanId
                        FROM DMATHANG m
                        WHERE m.ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync<MatHangViewModel>(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải chi tiết mặt hàng: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateMatHangAsync(MatHangViewModel model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        UPDATE DMATHANG SET 
                            CODE = @Code,
                            NAME = @Name,
                            GIANHAP = @Gianhap,
                            GIABAN = @Giaban,
                            QUYDOI = @Quydoi,
                            GIATHEOTHOIGIA = @Giatheothoigia,
                            DNHOMMATHANGID = @DnhommathangId,
                            DDONVITINHID = @DdonvitinhId,
                            TAMKHOA = @Tamkhoa
                        WHERE ID = @Id";

                    var parameters = new {
                        Id = model.Id,
                        Code = model.Code,
                        Name = model.Name,
                        Gianhap = model.Gianhap,
                        Giaban = model.Giaban,
                        Quydoi = model.Quydoi,
                        Giatheothoigia = model.Giatheothoigia,
                        DnhommathangId = model.DnhommathangId,
                        DdonvitinhId = model.DdonvitinhId,
                        Tamkhoa = model.Tamkhoa
                    };

                    int affectedRows = await conn.ExecuteAsync(sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi cập nhật mặt hàng: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteMatHangAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = "DELETE FROM DMATHANG WHERE ID = @Id";
                    var parameters = new { Id = id };

                    int affectedRows = await conn.ExecuteAsync(sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi xóa mặt hàng: " + ex.Message);
                return false;
            }
        }
        public async Task<bool> InsertNhomMatHangAsync(DNHOMMATHANG model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        INSERT INTO DNHOMMATHANG (
                            ID, NAME, PARENTID, ITEMTYPE, STATUS, USERCREATEDID, TIMECREATED, CODE, DLOAIDOID
                        ) VALUES (
                            @Id, @Name, @ParentId, @Itemtype, 1, 1, CURRENT_TIMESTAMP, @Code, @DloaidoId
                        )";

                    var parameters = new {
                        Id = model.Id,
                        Name = model.Name,
                        ParentId = model.ParentId,
                        Itemtype = model.Itemtype,
                        Code = model.Code,
                        DloaidoId = model.DloaidoId
                    };

                    int affectedRows = await Dapper.SqlMapper.ExecuteAsync(conn, sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi thêm nhóm mặt hàng: " + ex.Message);
                return false;
            }
        }
    }
}
