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
                System.Windows.MessageBox.Show("Lỗi tải danh sách đơn vị tính: " + ex.Message);
                return new List<DDONVITINH>();
            }
        }

        public async Task<List<DLOAIMATHANG>> GetLoaiMatHangListAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID as Id, NAME as Name FROM DLOAIMATHANG ORDER BY NAME";
                    var result = await conn.QueryAsync<DLOAIMATHANG>(sql);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải danh sách loại mặt hàng: " + ex.Message);
                return new List<DLOAIMATHANG>();
            }
        }

        public async Task<List<DNHOMMATHANG>> GetNhomMatHangListAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID as Id, NAME as Name FROM DNHOMMATHANG ORDER BY NAME";
                    var result = await conn.QueryAsync<DNHOMMATHANG>(sql);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải danh sách nhóm mặt hàng: " + ex.Message);
                return new List<DNHOMMATHANG>();
            }
        }



        public async Task<bool> InsertLoaiMatHangAsync(DLOAIMATHANG model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // Since DLOAIMATHANG ID is an integer, we might need a generator or let Firebird auto-increment it if there's a trigger/generator
                    // Let's assume Firebird uses a generator and we don't supply ID, or we get the max ID + 1. 
                    // To be safe, if model.Id is null, let's query the max ID.
                    if (model.Id == null || model.Id == 0)
                    {
                        var maxId = await conn.QueryFirstOrDefaultAsync<int?>("SELECT MAX(ID) FROM DLOAIMATHANG");
                        model.Id = (maxId ?? 0) + 1;
                    }

                    string sql = @"
                        INSERT INTO DLOAIMATHANG (
                            ID, NAME, NOTE, STATUS, USERCREATEDID, TIMECREATED
                        ) VALUES (
                            @Id, @Name, @Note, 1, 1, CURRENT_TIMESTAMP
                        )";

                    var parameters = new {
                        Id = model.Id,
                        Name = model.Name,
                        Note = model.Note
                    };

                    int affectedRows = await Dapper.SqlMapper.ExecuteAsync(conn, sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi thêm loại mặt hàng: " + ex.Message);
                return false;
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
                            STATUS, USERCREATEDID, TIMECREATED,
                            DLOAIMATHANGID, DDONVITINHCHANID, GIABANCHAN,
                            TONTOITHIEU, TONTOIDA, HOAHONG, GIAVON,
                            MACDINHGIAMGIA, MACDINHGIAMTIEN, TAMKHOA, NOTE
                        ) VALUES (
                            @Id, @Code, @Name, @Gianhap, @Giaban, @Quydoi, 
                            @Giatheothoigia, @DnhommathangId, @DdonvitinhId,
                            1, 1, CURRENT_TIMESTAMP,
                            @DloaimathangId, @DdonvitinhchanId, @Giabanchan,
                            @Tontoithieu, @Tontoida, @Hoahong, @Giavon,
                            @Macdinhgiamgia, @Macdinhgiamtien, @Tamkhoa, @Note
                        )";

                    // Note: 'Doitackygui' in MatHangViewModel is string, but DdoitackyguiId is int in DB.
                    // We skip DdoitackyguiId since we don't have the ID mapping for now.
                    // 'Anh' is byte[] in DB but string in ViewModel, skip for now.
                    
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
                        DloaimathangId = model.DloaimathangId,
                        DdonvitinhchanId = model.DdonvitinhchanId,
                        Giabanchan = model.Giabanchan,
                        Tontoithieu = model.Tontoithieu,
                        Tontoida = model.Tontoida,
                        Hoahong = model.Hoahong,
                        Giavon = model.Giavon,
                        Macdinhgiamgia = model.Macdinhgiamgia,
                        Macdinhgiamtien = model.Macdinhgiamtien,
                        Tamkhoa = model.Tamkhoa,
                        Note = model.Ghichu
                    };

                    int affectedRows = await Dapper.SqlMapper.ExecuteAsync(conn, sql, parameters);
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
                        INSERT INTO DNHOMMATHANG (ID, CODE, NAME, DLOAIDO_ID, PARENT_ID)
                        VALUES (@Id, @Code, @Name, @DloaidoId, @ParentId)";

                    var parameters = new {
                        Id = model.Id,
                        Code = model.Code,
                        Name = model.Name,
                        DloaidoId = model.DloaidoId,
                        ParentId = model.ParentId
                    };

                    int affectedRows = await conn.ExecuteAsync(sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi thêm nhóm mặt hàng: " + ex.Message);
                return false;
            }
        }

        public async Task<List<DNHOMMATHANG>> GetAllNhomMatHangAsync()
        {
            return await GetNhomMatHangListAsync();
        }

        public async Task<bool> UpdateNhomMatHangAsync(DNHOMMATHANG model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        UPDATE DNHOMMATHANG 
                        SET NAME = @Name
                        WHERE ID = @Id";

                    var parameters = new {
                        Id = model.Id,
                        Name = model.Name
                    };

                    int affectedRows = await conn.ExecuteAsync(sql, parameters);
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi cập nhật nhóm mặt hàng: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteNhomMatHangAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = "DELETE FROM DNHOMMATHANG WHERE ID = @Id";
                    int affectedRows = await conn.ExecuteAsync(sql, new { Id = id });
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi xóa nhóm mặt hàng: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> InsertOrUpdateDinhLuongAsync(DDINHLUONG model)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Check if exists
                    string checkSql = "SELECT ID FROM DDINHLUONG WHERE DMATHANGID = @DmathangId AND DVATTUID = @DvattuId";
                    var existingId = await conn.QueryFirstOrDefaultAsync<string>(checkSql, new { DmathangId = model.DmathangId, DvattuId = model.DvattuId });

                    if (!string.IsNullOrEmpty(existingId))
                    {
                        string updateSql = "UPDATE DDINHLUONG SET SOLUONG = @Soluong, TIMEMODIFIED = @Timemodified WHERE ID = @Id";
                        await conn.ExecuteAsync(updateSql, new { Soluong = model.Soluong, Timemodified = DateTime.Now, Id = existingId });
                    }
                    else
                    {
                        string insertSql = "INSERT INTO DDINHLUONG (ID, DMATHANGID, DVATTUID, SOLUONG, STATUS, USERCREATEDID, TIMECREATED) VALUES (@Id, @DmathangId, @DvattuId, @Soluong, @Status, 1, @Timecreated)";
                        await conn.ExecuteAsync(insertSql, new { Id = Guid.NewGuid().ToString(), DmathangId = model.DmathangId, DvattuId = model.DvattuId, Soluong = model.Soluong, Status = model.Status, Timecreated = model.Timecreated });
                    }

                    // Update Dmathang to Pha chế
                    string getLoaiSql = "SELECT ID FROM DLOAIMATHANG WHERE NAME = 'Mặt hàng pha chế'";
                    var loaiId = await conn.QueryFirstOrDefaultAsync<string>(getLoaiSql);
                    if (string.IsNullOrEmpty(loaiId))
                    {
                        loaiId = Guid.NewGuid().ToString();
                        string insertLoaiSql = "INSERT INTO DLOAIMATHANG (ID, NAME) VALUES (@Id, @Name)";
                        await conn.ExecuteAsync(insertLoaiSql, new { Id = loaiId, Name = "Mặt hàng pha chế" });
                    }

                    string updateMatHangSql = "UPDATE DMATHANG SET DLOAIMATHANGID = @LoaiId WHERE ID = @DmathangId";
                    await conn.ExecuteAsync(updateMatHangSql, new { LoaiId = loaiId, DmathangId = model.DmathangId });

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi cập nhật định lượng: " + ex.Message);
                return false;
            }
        }

        public async Task<List<DinhLuongChiTietViewModel>> GetDinhLuongByMatHangIdAsync(string matHangId, IEnumerable<MatHangViewModel> allMaterials)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID as Id, DMATHANGID as DmathangId, DVATTUID as DvattuId, SOLUONG as Soluong FROM DDINHLUONG WHERE DMATHANGID = @Id";
                    var dls = await conn.QueryAsync<DDINHLUONG>(sql, new { Id = matHangId });
                    
                    var list = new List<DinhLuongChiTietViewModel>();
                    foreach (var dl in dls)
                    {
                        var mat = allMaterials.FirstOrDefault(m => m.Id == dl.DvattuId);
                        if (mat != null)
                        {
                            var vm = new DinhLuongChiTietViewModel
                            {
                                OriginalId = dl.Id?.ToString(),
                                SelectedMatHang = mat,
                                SoLuong = dl.Soluong ?? 0
                            };
                            list.Add(vm);
                        }
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải định lượng: " + ex.Message);
                return new List<DinhLuongChiTietViewModel>();
            }
        }

        public async Task<bool> SaveDinhLuongListAsync(string matHangId, List<DinhLuongChiTietViewModel> list)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        // 1. Delete all existing ingredients for this product
                        string deleteSql = "DELETE FROM DDINHLUONG WHERE DMATHANGID = @Id";
                        await conn.ExecuteAsync(deleteSql, new { Id = matHangId }, transaction);

                        // 2. Insert the new ones
                        foreach (var item in list)
                        {
                            if (item.SelectedMatHang != null && item.SoLuong > 0)
                            {
                                string insertSql = "INSERT INTO DDINHLUONG (ID, DMATHANGID, DVATTUID, SOLUONG, STATUS, USERCREATEDID, TIMECREATED) VALUES (@Id, @DmathangId, @DvattuId, @Soluong, @Status, 1, @Timecreated)";
                                await conn.ExecuteAsync(insertSql, new 
                                { 
                                    Id = item.OriginalId ?? Guid.NewGuid().ToString(),
                                    DmathangId = matHangId, 
                                    DvattuId = item.SelectedMatHang.Id, 
                                    Soluong = item.SoLuong, 
                                    Status = 1, 
                                    Timecreated = DateTime.Now 
                                }, transaction);
                            }
                        }

                        // 3. Update Dmathang to Pha chế if there are ingredients
                        if (list.Any(x => x.SelectedMatHang != null && x.SoLuong > 0))
                        {
                            string getLoaiSql = "SELECT ID FROM DLOAIMATHANG WHERE NAME = 'Mặt hàng pha chế'";
                            var loaiId = await conn.QueryFirstOrDefaultAsync<string>(getLoaiSql, null, transaction);
                            if (string.IsNullOrEmpty(loaiId))
                            {
                                loaiId = Guid.NewGuid().ToString();
                                string insertLoaiSql = "INSERT INTO DLOAIMATHANG (ID, NAME) VALUES (@Id, @Name)";
                                await conn.ExecuteAsync(insertLoaiSql, new { Id = loaiId, Name = "Mặt hàng pha chế" }, transaction);
                            }

                            string updateMatHangSql = "UPDATE DMATHANG SET DLOAIMATHANGID = @LoaiId WHERE ID = @DmathangId";
                            await conn.ExecuteAsync(updateMatHangSql, new { LoaiId = loaiId, DmathangId = matHangId }, transaction);
                        }

                        transaction.Commit();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi lưu định lượng: " + ex.Message);
                return false;
            }
        }
    }
}
