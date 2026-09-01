using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public static class LocalKhachHangService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static int ParseInt(object val)
        {
            if (val == null) return 0;
            string s = val.ToString().Trim();
            if (int.TryParse(s, out int res)) return res;
            return 0;
        }

        public static async Task<ObservableCollection<NhomKhachHangTreeItem>> GetNhomKhachHangTreeAsync()
        {
            var result = new ObservableCollection<NhomKhachHangTreeItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DNHOMKHACHHANG WHERE STATUS > 0 ORDER BY SORTORDER, NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var rootItem = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        ItemType = 0,
                        Icon = "🌐",
                        IconColor = "#0078d7"
                    };

                    rootItem.Children.Add(new NhomKhachHangTreeItem
                    {
                        Id = "UNASSIGNED",
                        Name = "Chưa thiết lập",
                        ItemType = 1,
                        Icon = "☀️",
                        IconColor = "#f0ad4e"
                    });

                    var folders = new Dictionary<string, NhomKhachHangTreeItem>();
                    var items = new List<dynamic>();

                    foreach (var r in rows)
                    {
                        int itemType = ParseInt(r.ITEMTYPE);
                        int parentDir = ParseInt(r.PARENTDIR);

                        if (itemType == 1 || parentDir == 1)
                        {
                            var fNode = new NhomKhachHangTreeItem
                            {
                                Id = r.ID?.ToString(),
                                Name = r.NAME?.ToString(),
                                ParentId = r.PARENTID?.ToString()?.Trim(),
                                ItemType = 2,
                                Icon = "📁",
                                IconColor = "#f0ad4e"
                            };
                            folders[fNode.Id] = fNode;
                        }
                        else
                        {
                            items.Add(r);
                        }
                    }

                    foreach (var f in folders.Values)
                    {
                        if (!string.IsNullOrEmpty(f.ParentId) && folders.ContainsKey(f.ParentId))
                        {
                            folders[f.ParentId].Children.Add(f);
                        }
                        else
                        {
                            rootItem.Children.Add(f);
                        }
                    }

                    foreach (var it in items)
                    {
                        string parentId = it.PARENTID?.ToString()?.Trim();
                        var itNode = new NhomKhachHangTreeItem
                        {
                            Id = it.ID?.ToString(),
                            Name = it.NAME?.ToString(),
                            ParentId = parentId,
                            ItemType = 2,
                            Icon = "📁",
                            IconColor = "#f0ad4e"
                        };

                        if (!string.IsNullOrEmpty(parentId) && folders.ContainsKey(parentId))
                        {
                            folders[parentId].Children.Add(itNode);
                        }
                        else
                        {
                            rootItem.Children.Add(itNode);
                        }
                    }

                    var trashNode = new NhomKhachHangTreeItem
                    {
                        Id = "TRASH",
                        Name = "Thùng rác",
                        ItemType = 3,
                        Icon = "🗑️",
                        IconColor = "#888888"
                    };

                    string sqlTrash = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DNHOMKHACHHANG WHERE STATUS <= 0 OR STATUS IS NULL ORDER BY NAME";
                    var trashRows = (await conn.QueryAsync(sqlTrash)).ToList();
                    foreach (var r in trashRows)
                    {
                        trashNode.Children.Add(new NhomKhachHangTreeItem
                        {
                            Id = r.ID?.ToString(),
                            Name = r.NAME?.ToString() ?? "",
                            ParentId = "TRASH",
                            ItemType = 2,
                            Icon = "📁",
                            IconColor = "#888888"
                        });
                    }

                    rootItem.Children.Add(trashNode);
                    result.Add(rootItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomKhachHangTreeAsync: " + ex.Message);
            }

            return result;
        }

        public static async Task<ObservableCollection<NhomKhachHangTreeItem>> GetNhanVienTreeAsync()
        {
            var result = new ObservableCollection<NhomKhachHangTreeItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DNHANVIEN WHERE STATUS > 0 ORDER BY NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var rootItem = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        ItemType = 0,
                        Icon = "🌐",
                        IconColor = "#0078d7"
                    };

                    rootItem.Children.Add(new NhomKhachHangTreeItem
                    {
                        Id = "UNASSIGNED",
                        Name = "Chưa thiết lập",
                        ItemType = 1,
                        Icon = "☀️",
                        IconColor = "#f0ad4e"
                    });

                    var folders = new Dictionary<string, NhomKhachHangTreeItem>();
                    var employees = new List<dynamic>();

                    foreach (var r in rows)
                    {
                        int itemType = ParseInt(r.ITEMTYPE);
                        int parentDir = ParseInt(r.PARENTDIR);

                        if (itemType == 1 || parentDir == 1)
                        {
                            var fNode = new NhomKhachHangTreeItem
                            {
                                Id = r.ID?.ToString(),
                                Name = r.NAME?.ToString(),
                                ParentId = r.PARENTID?.ToString()?.Trim(),
                                ItemType = 2,
                                Icon = "📁",
                                IconColor = "#f0ad4e"
                            };
                            folders[fNode.Id] = fNode;
                        }
                        else
                        {
                            employees.Add(r);
                        }
                    }

                    foreach (var f in folders.Values)
                    {
                        if (!string.IsNullOrEmpty(f.ParentId) && folders.ContainsKey(f.ParentId))
                        {
                            folders[f.ParentId].Children.Add(f);
                        }
                        else
                        {
                            rootItem.Children.Add(f);
                        }
                    }

                    foreach (var nv in employees)
                    {
                        string name = nv.NAME?.ToString() ?? "";
                        string icon = name.Contains("Hằng") || name.Contains("Nữ") || name.Contains("Tuyết") ? "👩" : "👤";
                        string parentId = nv.PARENTID?.ToString()?.Trim();

                        var nvNode = new NhomKhachHangTreeItem
                        {
                            Id = nv.ID?.ToString(),
                            Name = name,
                            ParentId = parentId,
                            ItemType = 2,
                            Icon = icon,
                            IconColor = "#28a745"
                        };

                        if (!string.IsNullOrEmpty(parentId) && folders.ContainsKey(parentId))
                        {
                            folders[parentId].Children.Add(nvNode);
                        }
                        else
                        {
                            rootItem.Children.Add(nvNode);
                        }
                    }

                    var trashNode = new NhomKhachHangTreeItem
                    {
                        Id = "TRASH",
                        Name = "Thùng rác",
                        ItemType = 3,
                        Icon = "🗑️",
                        IconColor = "#888888"
                    };

                    string sqlTrash = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DNHANVIEN WHERE STATUS <= 0 OR STATUS IS NULL ORDER BY NAME";
                    var trashRows = (await conn.QueryAsync(sqlTrash)).ToList();
                    foreach (var r in trashRows)
                    {
                        trashNode.Children.Add(new NhomKhachHangTreeItem
                        {
                            Id = r.ID?.ToString(),
                            Name = r.NAME?.ToString() ?? "",
                            ParentId = "TRASH",
                            ItemType = 2,
                            Icon = "👤",
                            IconColor = "#888888"
                        });
                    }

                    rootItem.Children.Add(trashNode);
                    result.Add(rootItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhanVienTreeAsync: " + ex.Message);
            }

            return result;
        }

        public static async Task<ObservableCollection<NhomKhachHangTreeItem>> GetTinhThanhTreeAsync()
        {
            var result = new ObservableCollection<NhomKhachHangTreeItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DTINHTHANH WHERE STATUS > 0 ORDER BY NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var rootItem = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        ItemType = 0,
                        Icon = "🌐",
                        IconColor = "#0078d7"
                    };

                    rootItem.Children.Add(new NhomKhachHangTreeItem
                    {
                        Id = "UNASSIGNED",
                        Name = "Chưa thiết lập",
                        ItemType = 1,
                        Icon = "☀️",
                        IconColor = "#f0ad4e"
                    });

                    var folders = new Dictionary<string, NhomKhachHangTreeItem>();
                    var items = new List<dynamic>();

                    foreach (var r in rows)
                    {
                        int itemType = ParseInt(r.ITEMTYPE);
                        int parentDir = ParseInt(r.PARENTDIR);

                        if (itemType == 1 || parentDir == 1)
                        {
                            var fNode = new NhomKhachHangTreeItem
                            {
                                Id = r.ID?.ToString(),
                                Name = r.NAME?.ToString(),
                                ParentId = r.PARENTID?.ToString()?.Trim(),
                                ItemType = 2,
                                Icon = "📁",
                                IconColor = "#f0ad4e"
                            };
                            folders[fNode.Id] = fNode;
                        }
                        else
                        {
                            items.Add(r);
                        }
                    }

                    foreach (var f in folders.Values)
                    {
                        if (!string.IsNullOrEmpty(f.ParentId) && folders.ContainsKey(f.ParentId))
                        {
                            folders[f.ParentId].Children.Add(f);
                        }
                        else
                        {
                            rootItem.Children.Add(f);
                        }
                    }

                    foreach (var tt in items)
                    {
                        string parentId = tt.PARENTID?.ToString()?.Trim();
                        var ttNode = new NhomKhachHangTreeItem
                        {
                            Id = tt.ID?.ToString(),
                            Name = tt.NAME?.ToString(),
                            ParentId = parentId,
                            ItemType = 2,
                            Icon = "🏢",
                            IconColor = "#17a2b8"
                        };

                        if (!string.IsNullOrEmpty(parentId) && folders.ContainsKey(parentId))
                        {
                            folders[parentId].Children.Add(ttNode);
                        }
                        else
                        {
                            rootItem.Children.Add(ttNode);
                        }
                    }

                    var trashNode = new NhomKhachHangTreeItem
                    {
                        Id = "TRASH",
                        Name = "Thùng rác",
                        ItemType = 3,
                        Icon = "🗑️",
                        IconColor = "#888888"
                    };

                    string sqlTrash = "SELECT ID, NAME, PARENTID, ITEMTYPE, PARENTDIR FROM DTINHTHANH WHERE STATUS <= 0 OR STATUS IS NULL ORDER BY NAME";
                    var trashRows = (await conn.QueryAsync(sqlTrash)).ToList();
                    foreach (var r in trashRows)
                    {
                        trashNode.Children.Add(new NhomKhachHangTreeItem
                        {
                            Id = r.ID?.ToString(),
                            Name = r.NAME?.ToString() ?? "",
                            ParentId = "TRASH",
                            ItemType = 2,
                            Icon = "🏢",
                            IconColor = "#888888"
                        });
                    }

                    rootItem.Children.Add(trashNode);
                    result.Add(rootItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetTinhThanhTreeAsync: " + ex.Message);
            }

            return result;
        }

        public static async Task<List<KhachHangViewModel>> GetKhachHangListAsync(string filterId = "ALL", int itemType = 0, int groupMode = 0, string keyword = "")
        {
            var list = new List<KhachHangViewModel>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            K.ID, K.MAKHACH, K.NAME, K.DIACHI, K.DIENTHOAI, K.EMAIL, 
                            K.DNHOMKHACHHANGID, N.NAME AS TENNHOMKHACHHANG, K.MASOTHUE, 
                            K.STATUS, K.NGAYSINH, K.DIEMTICHLUYBANDAU AS DIEMTICHLUY, K.NOTE,
                            K.FACEBOOK, K.DTINHTHANHID, K.DTHETRATRUOCID, K.DNHANVIENID,
                            NV.NAME AS TENNHANVIEN, TT.NAME AS TENTINHTHANH
                        FROM DKHACHHANG K
                        LEFT JOIN DNHOMKHACHHANG N ON K.DNHOMKHACHHANGID = N.ID
                        LEFT JOIN DNHANVIEN NV ON K.DNHANVIENID = NV.ID
                        LEFT JOIN DTINHTHANH TT ON K.DTINHTHANHID = TT.ID
                        WHERE 1=1 ";

                    var p = new DynamicParameters();

                    if (itemType == 3) // Thùng rác
                    {
                        sql += " AND K.STATUS = 0 ";
                    }
                    else
                    {
                        sql += " AND (K.STATUS IS NULL OR K.STATUS > 0) ";

                        if (groupMode == 0) // Nhóm khách hàng
                        {
                            if (itemType == 1)
                            {
                                sql += " AND (K.DNHOMKHACHHANGID IS NULL OR TRIM(K.DNHOMKHACHHANGID) = '') ";
                            }
                            else if (itemType == 2 && !string.IsNullOrEmpty(filterId))
                            {
                                sql += " AND K.DNHOMKHACHHANGID = @FilterId ";
                                p.Add("@FilterId", filterId);
                            }
                        }
                        else if (groupMode == 1) // Nhân viên
                        {
                            if (itemType == 1)
                            {
                                sql += " AND (K.DNHANVIENID IS NULL OR TRIM(K.DNHANVIENID) = '') ";
                            }
                            else if (itemType == 2 && !string.IsNullOrEmpty(filterId))
                            {
                                sql += " AND K.DNHANVIENID = @FilterId ";
                                p.Add("@FilterId", filterId);
                            }
                        }
                        else if (groupMode == 2) // Tỉnh thành
                        {
                            if (itemType == 1)
                            {
                                sql += " AND (K.DTINHTHANHID IS NULL OR TRIM(K.DTINHTHANHID) = '') ";
                            }
                            else if (itemType == 2 && !string.IsNullOrEmpty(filterId))
                            {
                                sql += " AND K.DTINHTHANHID = @FilterId ";
                                p.Add("@FilterId", filterId);
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string kw = $"%{keyword.Trim().ToLower()}%";
                        sql += " AND (LOWER(K.MAKHACH) LIKE @kw OR LOWER(K.NAME) LIKE @kw OR LOWER(K.DIENTHOAI) LIKE @kw OR LOWER(K.EMAIL) LIKE @kw OR LOWER(K.DIACHI) LIKE @kw) ";
                        p.Add("@kw", kw);
                    }

                    sql += " ORDER BY K.MAKHACH, K.NAME";

                    var rows = (await conn.QueryAsync(sql, p)).ToList();
                    int stt = 1;

                    foreach (var r in rows)
                    {
                        list.Add(new KhachHangViewModel
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString(),
                            Makhach = r.MAKHACH?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Diachi = r.DIACHI?.ToString() ?? "",
                            Dienthoai = r.DIENTHOAI?.ToString() ?? "",
                            Email = r.EMAIL?.ToString() ?? "",
                            DnhomkhachhangId = r.DNHOMKHACHHANGID?.ToString(),
                            TenNhomKhachHang = r.TENNHOMKHACHHANG?.ToString() ?? "",
                            Masothue = r.MASOTHUE?.ToString() ?? "",
                            TenNhanVien = r.TENNHANVIEN?.ToString() ?? "",
                            TinhThanh = r.TENTINHTHANH?.ToString() ?? "",
                            Facebook = r.FACEBOOK?.ToString() ?? "",
                            TheTraTruoc = "",
                            Status = r.STATUS != null ? Convert.ToInt32(r.STATUS) : 30,
                            Ngaysinh = r.NGAYSINH != null ? Convert.ToDateTime(r.NGAYSINH) : null,
                            Diemtichluy = r.DIEMTICHLUY != null ? Convert.ToDecimal(r.DIEMTICHLUY) : 0,
                            Note = r.NOTE?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetKhachHangListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<bool> DeleteKhachHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DKHACHHANG SET STATUS = 0, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreKhachHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DKHACHHANG SET STATUS = 30, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePermanentKhachHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM DKHACHHANG WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePermanentKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<List<string>> GetAllKhachHangIdsAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID FROM DKHACHHANG WHERE STATUS > 0 ORDER BY MAKHACH, NAME";
                    var rows = await conn.QueryAsync<string>(sql);
                    return rows.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAllKhachHangIdsAsync: " + ex.Message);
                return new List<string>();
            }
        }

        public static async Task<dynamic> GetKhachHangByIdAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = @"
                        SELECT 
                            K.*,
                            N.NAME AS TENNHOMKHACHHANG,
                            NV.NAME AS TENNHANVIEN,
                            TT.NAME AS TENTINHTHANH
                        FROM DKHACHHANG K
                        LEFT JOIN DNHOMKHACHHANG N ON K.DNHOMKHACHHANGID = N.ID
                        LEFT JOIN DNHANVIEN NV ON K.DNHANVIENID = NV.ID
                        LEFT JOIN DTINHTHANH TT ON K.DTINHTHANHID = TT.ID
                        WHERE K.ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetKhachHangByIdAsync: " + ex.Message);
                return null;
            }
        }

        public static async Task<string> GetNextMaKhachAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT MAKHACH FROM DKHACHHANG WHERE MAKHACH LIKE 'KH%' ORDER BY MAKHACH DESC";
                    var list = (await conn.QueryAsync<string>(sql)).ToList();
                    int maxNum = 0;
                    foreach (var code in list)
                    {
                        if (!string.IsNullOrEmpty(code) && code.StartsWith("KH") && int.TryParse(code.Substring(2), out int n))
                        {
                            if (n > maxNum) maxNum = n;
                        }
                    }
                    return $"KH{(maxNum + 1):D5}";
                }
            }
            catch
            {
                return "KH00001";
            }
        }

        public static async Task<List<dynamic>> GetNhanVienLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DNHANVIEN WHERE STATUS > 0 ORDER BY NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetNhomKhachHangLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DNHOMKHACHHANG WHERE STATUS > 0 ORDER BY NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<List<dynamic>> GetTinhThanhLookupAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DTINHTHANH WHERE STATUS > 0 ORDER BY NAME";
                    return (await conn.QueryAsync(sql)).ToList();
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<bool> SaveKhachHangAsync(KhachHangViewModel model, bool isNew)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
                        string sql = @"
                            INSERT INTO DKHACHHANG (
                                ID, MAKHACH, NAME, DIACHI, DIENTHOAI, EMAIL, 
                                DNHOMKHACHHANGID, DNHANVIENID, DTINHTHANHID, MASOTHUE, STATUS, TIMECREATED, 
                                TIMEMODIFIED, NGAYSINH, DIEMTICHLUYBANDAU, NOTE, FACEBOOK,
                                USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Makhach, @Name, @Diachi, @Dienthoai, @Email, 
                                @DnhomkhachhangId, @DnhanvienId, @DtinhthanhId, @Masothue, 30, @Now, 
                                @Now, @Ngaysinh, @Diemtichluy, @Note, @Facebook,
                                @UserId, @UserId
                            )";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = id,
                            Makhach = model.Makhach,
                            Name = model.Name,
                            Diachi = model.Diachi,
                            Dienthoai = model.Dienthoai,
                            Email = model.Email,
                            DnhomkhachhangId = string.IsNullOrEmpty(model.DnhomkhachhangId) ? null : model.DnhomkhachhangId,
                            DnhanvienId = string.IsNullOrEmpty(model.TenNhanVien) ? null : model.TenNhanVien,
                            DtinhthanhId = string.IsNullOrEmpty(model.TinhThanh) ? null : model.TinhThanh,
                            Masothue = model.Masothue,
                            Now = DateTime.Now,
                            Ngaysinh = model.Ngaysinh,
                            Diemtichluy = model.Diemtichluy,
                            Note = model.Note,
                            Facebook = model.Facebook,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DKHACHHANG SET 
                                MAKHACH = @Makhach,
                                NAME = @Name,
                                DIACHI = @Diachi,
                                DIENTHOAI = @Dienthoai,
                                EMAIL = @Email,
                                DNHOMKHACHHANGID = @DnhomkhachhangId,
                                DNHANVIENID = @DnhanvienId,
                                DTINHTHANHID = @DtinhthanhId,
                                MASOTHUE = @Masothue,
                                TIMEMODIFIED = @Now,
                                NGAYSINH = @Ngaysinh,
                                DIEMTICHLUYBANDAU = @Diemtichluy,
                                NOTE = @Note,
                                FACEBOOK = @Facebook,
                                USERMODIFIEDID = @UserId
                            WHERE ID = @Id";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = model.Id,
                            Makhach = model.Makhach,
                            Name = model.Name,
                            Diachi = model.Diachi,
                            Dienthoai = model.Dienthoai,
                            Email = model.Email,
                            DnhomkhachhangId = string.IsNullOrEmpty(model.DnhomkhachhangId) ? null : model.DnhomkhachhangId,
                            DnhanvienId = string.IsNullOrEmpty(model.TenNhanVien) ? null : model.TenNhanVien,
                            DtinhthanhId = string.IsNullOrEmpty(model.TinhThanh) ? null : model.TinhThanh,
                            Masothue = model.Masothue,
                            Now = DateTime.Now,
                            Ngaysinh = model.Ngaysinh,
                            Diemtichluy = model.Diemtichluy,
                            Note = model.Note,
                            Facebook = model.Facebook,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<dynamic> GetNhomKhachHangByIdAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT * FROM DNHOMKHACHHANG WHERE ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomKhachHangByIdAsync: " + ex.Message);
                return null;
            }
        }

        public static async Task<bool> SaveNhomKhachHangAsync(string id, string name, decimal diemTichLuy, decimal giamTienHang, decimal giamDoAn, decimal giamDoUong, decimal giamDichVu, decimal giamDoKhac, string note, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string newId = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
                        string sql = @"
                            INSERT INTO DNHOMKHACHHANG (
                                ID, NAME, DIEMTICHLUY, TILEGIAMGIATIENHANG, TILEGIAMDOAN, 
                                TILEGIAMDOUONG, TILEGIAMDICHVU, TILEGIAMDOKHAC, NOTE, 
                                STATUS, TIMECREATED, TIMEMODIFIED, SORTORDER, ITEMTYPE, PARENTID,
                                USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, @DiemTichLuy, @GiamTienHang, @GiamDoAn, 
                                @GiamDoUong, @GiamDichVu, @GiamDoKhac, @Note, 
                                30, @Now, @Now, 'ZZZZ', 0, @ParentId,
                                @UserId, @UserId
                            )";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = newId,
                            Name = name,
                            DiemTichLuy = diemTichLuy,
                            GiamTienHang = giamTienHang,
                            GiamDoAn = giamDoAn,
                            GiamDoUong = giamDoUong,
                            GiamDichVu = giamDichVu,
                            GiamDoKhac = giamDoKhac,
                            Note = note,
                            ParentId = string.IsNullOrEmpty(parentId) ? "" : parentId,
                            Now = DateTime.Now,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DNHOMKHACHHANG SET 
                                NAME = @Name,
                                DIEMTICHLUY = @DiemTichLuy,
                                TILEGIAMGIATIENHANG = @GiamTienHang,
                                TILEGIAMDOAN = @GiamDoAn,
                                TILEGIAMDOUONG = @GiamDoUong,
                                TILEGIAMDICHVU = @GiamDichVu,
                                TILEGIAMDOKHAC = @GiamDoKhac,
                                NOTE = @Note,
                                TIMEMODIFIED = @Now,
                                USERMODIFIEDID = @UserId
                            WHERE ID = @Id";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = id,
                            Name = name,
                            DiemTichLuy = diemTichLuy,
                            GiamTienHang = giamTienHang,
                            GiamDoAn = giamDoAn,
                            GiamDoUong = giamDoUong,
                            GiamDichVu = giamDichVu,
                            GiamDoKhac = giamDoKhac,
                            Note = note,
                            Now = DateTime.Now,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveNhomKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteNhomKhachHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DNHOMKHACHHANG SET STATUS = 0, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteNhomKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SaveNhomKhachHangFolderAsync(string id, string name, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string sql = @"
                            INSERT INTO DNHOMKHACHHANG (
                                ID, NAME, STATUS, ITEMTYPE, PARENTDIR, PARENTID, TIMECREATED, TIMEMODIFIED,
                                SORTORDER, USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, 30, 1, 1, @ParentId, @Now, @Now,
                                'ZZZZ', @UserId, @UserId
                            )";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, ParentId = string.IsNullOrEmpty(parentId) ? "" : parentId, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = "UPDATE DNHOMKHACHHANG SET NAME = @Name, TIMEMODIFIED = @Now, USERMODIFIEDID = @UserId WHERE ID = @Id";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveNhomKhachHangFolderAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SaveNhanVienFolderAsync(string id, string name, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string sql = @"
                            INSERT INTO DNHANVIEN (
                                ID, NAME, STATUS, ITEMTYPE, PARENTDIR, PARENTID, TIMECREATED, TIMEMODIFIED,
                                SORTORDER, USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, 30, 1, 1, @ParentId, @Now, @Now,
                                'ZZZZ', @UserId, @UserId
                            )";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, ParentId = string.IsNullOrEmpty(parentId) ? "" : parentId, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = "UPDATE DNHANVIEN SET NAME = @Name, TIMEMODIFIED = @Now, USERMODIFIEDID = @UserId WHERE ID = @Id";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveNhanVienFolderAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SaveTinhThanhFolderAsync(string id, string name, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string sql = @"
                            INSERT INTO DTINHTHANH (
                                ID, NAME, STATUS, ITEMTYPE, PARENTDIR, PARENTID, TIMECREATED, TIMEMODIFIED,
                                SORTORDER, USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, 30, 1, 1, @ParentId, @Now, @Now,
                                'ZZZZ', @UserId, @UserId
                            )";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, ParentId = string.IsNullOrEmpty(parentId) ? "" : parentId, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = "UPDATE DTINHTHANH SET NAME = @Name, TIMEMODIFIED = @Now, USERMODIFIEDID = @UserId WHERE ID = @Id";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveTinhThanhFolderAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<List<string>> GetAllNhanVienIdsAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID FROM DNHANVIEN WHERE STATUS > 0 ORDER BY NAME";
                    var rows = await conn.QueryAsync<string>(sql);
                    return rows.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAllNhanVienIdsAsync: " + ex.Message);
                return new List<string>();
            }
        }

        public static async Task<dynamic> GetNhanVienByIdAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT * FROM DNHANVIEN WHERE ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhanVienByIdAsync: " + ex.Message);
                return null;
            }
        }

        public static async Task<bool> SaveNhanVienAsync(string id, string name, string diachi, string dienthoai, int cachTinhLuong, decimal luongCa, decimal luongThang, int nghiThu7, int nghiChuNhat, string note, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string newId = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
                        string sql = @"
                            INSERT INTO DNHANVIEN (
                                ID, NAME, DIACHI, DIENTHOAI, CACHTINHLUONG, LUONGCA, 
                                LUONGTHANG, NGHITHU7, NGHICHUNHAT, NOTE, STATUS, 
                                TIMECREATED, TIMEMODIFIED, SORTORDER, ITEMTYPE, PARENTID,
                                USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, @Diachi, @Dienthoai, @CachTinhLuong, @LuongCa, 
                                @LuongThang, @NghiThu7, @NghiChuNhat, @Note, 30, 
                                @Now, @Now, 'ZZZZ', 0, @ParentId,
                                @UserId, @UserId
                            )";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = newId,
                            Name = name,
                            Diachi = diachi,
                            Dienthoai = dienthoai,
                            CachTinhLuong = cachTinhLuong,
                            LuongCa = luongCa,
                            LuongThang = luongThang,
                            NghiThu7 = nghiThu7,
                            NghiChuNhat = nghiChuNhat,
                            Note = note,
                            ParentId = string.IsNullOrEmpty(parentId) ? "" : parentId,
                            Now = DateTime.Now,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DNHANVIEN SET 
                                NAME = @Name,
                                DIACHI = @Diachi,
                                DIENTHOAI = @Dienthoai,
                                CACHTINHLUONG = @CachTinhLuong,
                                LUONGCA = @LuongCa,
                                LUONGTHANG = @LuongThang,
                                NGHITHU7 = @NghiThu7,
                                NGHICHUNHAT = @NghiChuNhat,
                                NOTE = @Note,
                                TIMEMODIFIED = @Now,
                                USERMODIFIEDID = @UserId
                            WHERE ID = @Id";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = id,
                            Name = name,
                            Diachi = diachi,
                            Dienthoai = dienthoai,
                            CachTinhLuong = cachTinhLuong,
                            LuongCa = luongCa,
                            LuongThang = luongThang,
                            NghiThu7 = nghiThu7,
                            NghiChuNhat = nghiChuNhat,
                            Note = note,
                            Now = DateTime.Now,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveNhanVienAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> UpdateEmployeeParentAsync(string employeeId, string parentFolderId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DNHANVIEN SET PARENTID = @ParentId, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { ParentId = string.IsNullOrEmpty(parentFolderId) ? "" : parentFolderId, Now = DateTime.Now, Id = employeeId });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error UpdateEmployeeParentAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteNhanVienAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DNHANVIEN SET STATUS = 0, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteNhanVienAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<dynamic> GetTinhThanhByIdAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT * FROM DTINHTHANH WHERE ID = @Id";
                    return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetTinhThanhByIdAsync: " + ex.Message);
                return null;
            }
        }

        public static async Task<bool> SaveTinhThanhAsync(string id, string name, string note, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (isNew)
                    {
                        string newId = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
                        string sql = @"
                            INSERT INTO DTINHTHANH (
                                ID, NAME, NOTE, STATUS, TIMECREATED, TIMEMODIFIED, 
                                SORTORDER, ITEMTYPE, PARENTID, USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, @Note, 30, @Now, @Now, 
                                'ZZZZ', 0, @ParentId, @UserId, @UserId
                            )";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = newId,
                            Name = name,
                            Note = note,
                            ParentId = string.IsNullOrEmpty(parentId) ? "" : parentId,
                            Now = DateTime.Now,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DTINHTHANH SET 
                                NAME = @Name,
                                NOTE = @Note,
                                TIMEMODIFIED = @Now,
                                USERMODIFIEDID = @UserId
                            WHERE ID = @Id";

                        int affected = await conn.ExecuteAsync(sql, new
                        {
                            Id = id,
                            Name = name,
                            Note = note,
                            Now = DateTime.Now,
                            UserId = userId
                        });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveTinhThanhAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteTinhThanhAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DTINHTHANH SET STATUS = 0, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteTinhThanhAsync: " + ex.Message);
                return false;
            }
        }

        #region Trash Operations
        public static async Task<bool> RestoreNhanVienAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DNHANVIEN SET STATUS = 1, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreNhanVienAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreNhomKhachHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DNHOMKHACHHANG SET STATUS = 30, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreNhomKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreTinhThanhAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "UPDATE DTINHTHANH SET STATUS = 30, TIMEMODIFIED = @Now WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Now = DateTime.Now, Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreTinhThanhAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePermanentNhanVienAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM DNHANVIEN WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePermanentNhanVienAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePermanentNhomKhachHangAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM DNHOMKHACHHANG WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePermanentNhomKhachHangAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePermanentTinhThanhAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM DTINHTHANH WHERE ID = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeletePermanentTinhThanhAsync: " + ex.Message);
                return false;
            }
        }
        public static async Task<bool> UpdateCustomersColumnAsync(List<string> ids, string columnName, object value)
        {
            if (ids == null || ids.Count == 0) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string inClause = string.Join(",", ids.Select(id => $"'{id.Replace("'", "''")}'"));
                    string valSql = value == null ? "NULL" : $"'{value.ToString().Replace("'", "''")}'";
                    string sql = $"UPDATE DKHACHHANG SET {columnName} = {valSql}, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID IN ({inClause})";
                    await conn.ExecuteAsync(sql);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error UpdateCustomersColumnAsync: " + ex.Message);
                return false;
            }
        }
        #endregion
    }
}
