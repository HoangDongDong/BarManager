using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class NhomNhaCungCapTreeItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; } = "📁";
        public string ParentId { get; set; }
        public string SpecialType { get; set; } // "ALL", "UNSET", "TRASH", or null
        public byte[] ImageBytes { get; set; }
        public ImageSource ImageSource { get; set; }
        public ObservableCollection<NhomNhaCungCapTreeItem> Children { get; set; } = new ObservableCollection<NhomNhaCungCapTreeItem>();
    }

    public class NhaCungCapItem
    {
        public int Stt { get; set; }
        public string Id { get; set; }
        public string MaNhaCungCap { get; set; }
        public string Name { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Note { get; set; }
        public string DnhomnhacungcapId { get; set; }
        public string TenNhom { get; set; }
        public int Status { get; set; } = 30;
        public DateTime? TimeCreated { get; set; }
        public string UserCreatedId { get; set; }
        public string UserCreatedName { get; set; }
        public DateTime? TimeModified { get; set; }
        public string UserModifiedId { get; set; }
        public string UserModifiedName { get; set; }

        public string TimeCreatedFormatted => TimeCreated?.ToString("dd/MM/yyyy hh:mm tt") ?? "";
        public string TimeModifiedFormatted => TimeModified?.ToString("dd/MM/yyyy hh:mm tt") ?? "";
    }

    public class NccPhieuNhapItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public decimal TongCong { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TiLeGiamGia { get; set; }
        public decimal TienThue { get; set; }
        public decimal TiLeThue { get; set; }
        public decimal TienHang { get; set; }
        public string NhaCungCap { get; set; } = "";
        public string KhoNhap { get; set; } = "";
        public string NhanVienNhap { get; set; } = "";
        public string DienGiai { get; set; } = "";
        public decimal Voucher { get; set; }
        public string NhanVienGiaoHang { get; set; } = "";
        public decimal TrichNhanVien { get; set; }
        public string CuaHang { get; set; } = "";
        public decimal ConLai { get; set; }
        public decimal ThanhToan { get; set; }
        public string TaiKhoanNganHang { get; set; } = "";
        public string MaVoucher { get; set; } = "";
        public string TheTt { get; set; } = "";
        public decimal TheTraTruoc { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal DiemGiam { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public string Ban { get; set; } = "";
        public string BatDau { get; set; } = "";
        public string KetThuc { get; set; } = "";
        public decimal TienGio { get; set; }
        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public int SoKhach { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal TiLePhiDichVu { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string SoOrder { get; set; } = "";
        public string SoHoaDon { get; set; } = "";
        public string SoThanhToan { get; set; } = "";
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string BangGia { get; set; } = "";
        public decimal TienGioPhongCuoi { get; set; }
        public string BatDauPhongCuoi { get; set; } = "";
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public int PhutKhuyenMai { get; set; }
        public string InTamTinhLuc { get; set; } = "";
        public decimal DatTruoc { get; set; }
        public decimal CongNo { get; set; }
        public decimal TienHangChuaGiam { get; set; }
        public decimal GiamGiaMatHang { get; set; }
        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string PassWifi { get; set; } = "";
        public string TenDoiTuong { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string LyDoThuChi { get; set; } = "";
        public string ChungTuGoc { get; set; } = "";
        public decimal SoTien { get; set; }
        public string DatHang { get; set; } = "";
        public string LaPhieuThuCongNo { get; set; } = "";
        public string KhongThayDoiCongNo { get; set; } = "";
        public string DonHang { get; set; } = "";
        public string NhanVien { get => NhanVienNhap; set => NhanVienNhap = value; }
        public string KhachHang { get; set; } = "";
        public string LoaiDoiTuong { get; set; } = "";
        public decimal SoTienThu { get; set; }
        public decimal SoTienChi { get; set; }

        public string NgayFormatted => Ngay?.ToString("dd/MM/yyyy") ?? "";
        public string TongCongFormatted => TongCong.ToString("N0");
        public string TienHangFormatted => TienHang.ToString("N0");
        public string TienGiamGiaFormatted => TienGiamGia.ToString("N0");
        public string PhiVanChuyenFormatted => PhiVanChuyen.ToString("N0");
        public string TienThueFormatted => TienThue.ToString("N0");
        public string ConLaiFormatted => ConLai.ToString("N0");
        public string ThanhToanFormatted => ThanhToan.ToString("N0");
        public string TienMatFormatted => TienMat.ToString("N0");
        public string ChuyenKhoanFormatted => ChuyenKhoan.ToString("N0");
        public string TheFormatted => The.ToString("N0");
        public string SoTienFormatted => (SoTien != 0 ? SoTien : TongCong).ToString("N0");
        public string SoTienThuFormatted => (SoTienThu != 0 ? SoTienThu : TongCong).ToString("N0");
        public string SoTienChiFormatted => (SoTienChi != 0 ? SoTienChi : 0).ToString("N0");
        public string DonGiaFormatted => DonGia.ToString("N0");
        public string TienGioPhongCuoiFormatted => TienGioPhongCuoi.ToString("N0");
        public string TienMoBanFormatted => TienMoBan.ToString("N0");
        public string DatTruocFormatted => DatTruoc.ToString("N0");
        public string CongNoFormatted => CongNo.ToString("N0");
        public string TienHangChuaGiamFormatted => TienHangChuaGiam.ToString("N0");
        public string GiamGiaMatHangFormatted => GiamGiaMatHang.ToString("N0");
    }

    public class ChungTuItem : NccPhieuNhapItem
    {
        public string SoChungTu { get => SoPhieu; set => SoPhieu = value; }
        public DateTime? NgayLap { get => Ngay; set => Ngay = value; }
        public string KhoHang { get => KhoNhap; set => KhoNhap = value; }
        public decimal TongTien { get => TongCong; set => TongCong = value; }
        public decimal DaThanhToan { get => ThanhToan; set => ThanhToan = value; }
        public string NgayLapFormatted => NgayFormatted;
        public string TongTienFormatted => TongCongFormatted;
        public string DaThanhToanFormatted => ThanhToanFormatted;
        public string ConNoFormatted => ConLaiFormatted;
    }

    public class HoaDonNhaHangChiTietNccItem : NccPhieuNhapItem
    {
    }

    public static class LocalNhaCungCapService
    {
        public static ImageSource BytesToBitmapImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;
            try
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<ObservableCollection<NhomNhaCungCapTreeItem>> GetNhomNhaCungCapTreeAsync()
        {
            var tree = new ObservableCollection<NhomNhaCungCapTreeItem>();

            // 1. Node Tất cả
            tree.Add(new NhomNhaCungCapTreeItem
            {
                Id = "ALL",
                Name = "Tất cả",
                Icon = "🌐",
                SpecialType = "ALL"
            });

            // 2. Node Chưa thiết lập
            tree.Add(new NhomNhaCungCapTreeItem
            {
                Id = "UNSET",
                Name = "Chưa thiết lập",
                Icon = "✳️",
                SpecialType = "UNSET"
            });

            // 3. Node Thùng rác
            tree.Add(new NhomNhaCungCapTreeItem
            {
                Id = "TRASH",
                Name = "Thùng rác",
                Icon = "🗑️",
                SpecialType = "TRASH"
            });

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT n.ID, n.NAME, n.NOTE, n.PARENTID, n.SIMAGEID, sim.IMAGE as ImageBytes
                        FROM DNHOMNHACUNGCAP n
                        LEFT JOIN SIMAGE sim ON CAST(n.SIMAGEID AS VARCHAR(50)) = CAST(sim.ID AS VARCHAR(50))
                        WHERE (n.STATUS IS NULL OR n.STATUS <> 0)
                        ORDER BY n.SORTORDER, n.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();
                    var flatList = new List<NhomNhaCungCapTreeItem>();

                    foreach (var r in rows)
                    {
                        byte[] b = r.IMAGEBYTES as byte[];
                        flatList.Add(new NhomNhaCungCapTreeItem
                        {
                            Id = r.ID?.ToString(),
                            Name = r.NAME?.ToString(),
                            ParentId = r.PARENTID?.ToString(),
                            Icon = "📁",
                            ImageBytes = b,
                            ImageSource = BytesToBitmapImage(b)
                        });
                    }

                    var lookup = new Dictionary<string, NhomNhaCungCapTreeItem>();
                    foreach (var item in flatList)
                    {
                        if (!string.IsNullOrEmpty(item.Id)) lookup[item.Id] = item;
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomNhaCungCapTreeAsync: " + ex.Message);
            }

            return tree;
        }

        public static async Task<List<NhaCungCapItem>> GetNhaCungCapListAsync(string nhomId, string filterText = "", string specialType = "ALL")
        {
            var list = new List<NhaCungCapItem>();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            n.ID,
                            n.MANHACUNGCAP,
                            n.NAME,
                            n.DIACHI,
                            n.DIENTHOAI,
                            n.EMAIL,
                            n.WEBSITE,
                            n.NOTE,
                            n.DNHOMNHACUNGCAPID,
                            nh.NAME as TenNhom,
                            n.STATUS,
                            n.TIMECREATED,
                            n.USERCREATEDID,
                            u1.NAME as UserCreatedName,
                            n.TIMEMODIFIED,
                            n.USERMODIFIEDID,
                            u2.NAME as UserModifiedName
                        FROM DNHACUNGCAP n
                        LEFT JOIN DNHOMNHACUNGCAP nh ON CAST(n.DNHOMNHACUNGCAPID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u1 ON CAST(n.USERCREATEDID AS VARCHAR(50)) = CAST(u1.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u2 ON CAST(n.USERMODIFIEDID AS VARCHAR(50)) = CAST(u2.ID AS VARCHAR(50))
                        WHERE 1=1";

                    if (specialType == "TRASH")
                    {
                        sql += " AND (n.STATUS = 0)";
                    }
                    else
                    {
                        sql += " AND (n.STATUS IS NULL OR n.STATUS <> 0)";

                        if (specialType == "UNSET")
                        {
                            sql += " AND (n.DNHOMNHACUNGCAPID IS NULL OR CAST(n.DNHOMNHACUNGCAPID AS VARCHAR(50)) = '')";
                        }
                        else if (!string.IsNullOrEmpty(nhomId) && specialType != "ALL")
                        {
                            sql += " AND (CAST(n.DNHOMNHACUNGCAPID AS VARCHAR(50)) = @NhomId)";
                        }
                    }

                    sql += " ORDER BY n.MANHACUNGCAP, n.NAME";

                    var rows = (await conn.QueryAsync(sql, new { NhomId = nhomId })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        string ma = r.MANHACUNGCAP?.ToString() ?? "";
                        string name = r.NAME?.ToString() ?? "";
                        string diachi = r.DIACHI?.ToString() ?? "";
                        string dt = r.DIENTHOAI?.ToString() ?? "";
                        string email = r.EMAIL?.ToString() ?? "";
                        string website = r.WEBSITE?.ToString() ?? "";
                        string note = r.NOTE?.ToString() ?? "";
                        string tenNhom = r.TENNHOM?.ToString() ?? "";

                        if (!string.IsNullOrWhiteSpace(filterText))
                        {
                            string ft = filterText.Trim().ToLower();
                            bool match = ma.ToLower().Contains(ft) ||
                                         name.ToLower().Contains(ft) ||
                                         diachi.ToLower().Contains(ft) ||
                                         dt.ToLower().Contains(ft) ||
                                         email.ToLower().Contains(ft) ||
                                         tenNhom.ToLower().Contains(ft);
                            if (!match) continue;
                        }

                        DateTime? tc = null;
                        if (r.TIMECREATED != null)
                        {
                            if (DateTime.TryParse(r.TIMECREATED.ToString(), out DateTime parsedTc)) tc = parsedTc;
                        }

                        DateTime? tm = null;
                        if (r.TIMEMODIFIED != null)
                        {
                            if (DateTime.TryParse(r.TIMEMODIFIED.ToString(), out DateTime parsedTm)) tm = parsedTm;
                        }

                        string u1Name = r.USERCREATEDNAME?.ToString();
                        if (string.IsNullOrEmpty(u1Name) && !string.IsNullOrEmpty(r.USERCREATEDID?.ToString()))
                        {
                            u1Name = "Administrator";
                        }

                        list.Add(new NhaCungCapItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            MaNhaCungCap = ma,
                            Name = name,
                            DiaChi = diachi,
                            DienThoai = dt,
                            Email = email,
                            Website = website,
                            Note = note,
                            DnhomnhacungcapId = r.DNHOMNHACUNGCAPID?.ToString(),
                            TenNhom = tenNhom,
                            Status = Convert.ToInt32(r.STATUS ?? 30),
                            TimeCreated = tc,
                            UserCreatedId = r.USERCREATEDID?.ToString(),
                            UserCreatedName = u1Name ?? "Administrator",
                            TimeModified = tm,
                            UserModifiedId = r.USERMODIFIEDID?.ToString(),
                            UserModifiedName = r.USERMODIFIEDNAME?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhaCungCapListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<string> GetNextMaNhaCungCapAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    var codes = (await conn.QueryAsync<string>("SELECT MANHACUNGCAP FROM DNHACUNGCAP WHERE MANHACUNGCAP LIKE 'NCC%'")).ToList();
                    int maxNumber = 0;
                    foreach (var c in codes)
                    {
                        if (string.IsNullOrEmpty(c)) continue;
                        string numPart = c.Substring(3).Trim();
                        if (int.TryParse(numPart, out int n))
                        {
                            if (n > maxNumber) maxNumber = n;
                        }
                    }

                    return $"NCC{(maxNumber + 1).ToString("D4")}";
                }
            }
            catch
            {
                return "NCC0001";
            }
        }

        public static async Task<string> GetCurrentUserIdAsync(IDbConnection conn)
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

        public static async Task<(bool Success, string Message, string Id)> SaveNhaCungCapAsync(NhaCungCapItem item, bool isNew)
        {
            if (item == null) return (false, "Dữ liệu rỗng", "");
            if (string.IsNullOrWhiteSpace(item.Name)) return (false, "Vui lòng nhập Tên nhà cung cấp", "");

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string userId = await GetCurrentUserIdAsync(conn);

                    if (isNew || string.IsNullOrEmpty(item.Id))
                    {
                        string newId = Guid.NewGuid().ToString();
                        string code = item.MaNhaCungCap;
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            code = await GetNextMaNhaCungCapAsync();
                        }

                        string insertSql = @"
                            INSERT INTO DNHACUNGCAP (
                                ID, MANHACUNGCAP, NAME, DIACHI, DIENTHOAI, EMAIL, WEBSITE, NOTE,
                                DNHOMNHACUNGCAPID, STATUS, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @MaNhaCungCap, @Name, @DiaChi, @DienThoai, @Email, @Website, @Note,
                                @DnhomnhacungcapId, 30, @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = newId,
                            MaNhaCungCap = code,
                            Name = item.Name.Trim(),
                            DiaChi = item.DiaChi?.Trim() ?? "",
                            DienThoai = item.DienThoai?.Trim() ?? "",
                            Email = item.Email?.Trim() ?? "",
                            Website = item.Website?.Trim() ?? "",
                            Note = item.Note?.Trim() ?? "",
                            DnhomnhacungcapId = string.IsNullOrEmpty(item.DnhomnhacungcapId) ? null : item.DnhomnhacungcapId,
                            UserCreatedId = userId
                        });

                        return (true, "", newId);
                    }
                    else
                    {
                        string updateSql = @"
                            UPDATE DNHACUNGCAP SET
                                MANHACUNGCAP = @MaNhaCungCap,
                                NAME = @Name,
                                DIACHI = @DiaChi,
                                DIENTHOAI = @DienThoai,
                                EMAIL = @Email,
                                WEBSITE = @Website,
                                NOTE = @Note,
                                DNHOMNHACUNGCAPID = @DnhomnhacungcapId,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(updateSql, new
                        {
                            Id = item.Id,
                            MaNhaCungCap = item.MaNhaCungCap?.Trim() ?? "",
                            Name = item.Name.Trim(),
                            DiaChi = item.DiaChi?.Trim() ?? "",
                            DienThoai = item.DienThoai?.Trim() ?? "",
                            Email = item.Email?.Trim() ?? "",
                            Website = item.Website?.Trim() ?? "",
                            Note = item.Note?.Trim() ?? "",
                            DnhomnhacungcapId = string.IsNullOrEmpty(item.DnhomnhacungcapId) ? null : item.DnhomnhacungcapId,
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

        public static async Task<(bool Success, string Message, string Id)> SaveNhomNhaCungCapAsync(string id, string name, string note, string parentId, bool isNew)
        {
            if (string.IsNullOrWhiteSpace(name)) return (false, "Vui lòng nhập tên nhóm", "");

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string userId = await GetCurrentUserIdAsync(conn);

                    if (isNew || string.IsNullOrEmpty(id))
                    {
                        string newId = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
                        string sql = @"
                            INSERT INTO DNHOMNHACUNGCAP (
                                ID, NAME, NOTE, STATUS, PARENTID, USERCREATEDID, TIMECREATED, SORTORDER
                            ) VALUES (
                                @Id, @Name, @Note, 30, @ParentId, @UserCreatedId, CURRENT_TIMESTAMP, 'ZZZZ'
                            )";
                        await conn.ExecuteAsync(sql, new
                        {
                            Id = newId,
                            Name = name.Trim(),
                            Note = note?.Trim() ?? "",
                            ParentId = string.IsNullOrEmpty(parentId) ? null : parentId,
                            UserCreatedId = userId
                        });

                        return (true, "", newId);
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DNHOMNHACUNGCAP SET
                                NAME = @Name,
                                NOTE = @Note,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        await conn.ExecuteAsync(sql, new
                        {
                            Id = id,
                            Name = name.Trim(),
                            Note = note?.Trim() ?? "",
                            UserModifiedId = userId
                        });

                        return (true, "", id);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, "");
            }
        }

        public static async Task<bool> DeleteNhaCungCapAsync(string id, bool permanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    if (permanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM DNHACUNGCAP WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE DNHACUNGCAP SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteNhaCungCapAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreNhaCungCapAsync(string id)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    await conn.ExecuteAsync("UPDATE DNHACUNGCAP SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RestoreNhaCungCapAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<List<NhomNhaCungCapTreeItem>> GetAllNhomListFlatAsync()
        {
            var list = new List<NhomNhaCungCapTreeItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT n.ID, n.NAME, n.NOTE, n.PARENTID
                        FROM DNHOMNHACUNGCAP n
                        WHERE (n.STATUS IS NULL OR n.STATUS <> 0)
                        ORDER BY n.SORTORDER, n.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();
                    foreach (var r in rows)
                    {
                        list.Add(new NhomNhaCungCapTreeItem
                        {
                            Id = r.ID?.ToString(),
                            Name = r.NAME?.ToString(),
                            ParentId = r.PARENTID?.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAllNhomListFlatAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<ChungTuItem>> GetPhieuNhapKhoByNccAsync(string nccId)
        {
            var list = new List<ChungTuItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(d.ID AS VARCHAR(50)) as Id,
                            d.NAME as SoPhieu,
                            d.NGAY as Ngay,
                            COALESCE(d.TONGCONG, 0) as TongCong,
                            COALESCE(d.TIENHANG, 0) as TienHang,
                            COALESCE(d.TIENGIAMGIA, 0) as TienGiamGia,
                            COALESCE(d.TILEGIAMGIA, 0) as TiLeGiamGia,
                            COALESCE(d.TIENTHUE, 0) as TienThue,
                            COALESCE(d.TILETHUE, 0) as TiLeThue,
                            COALESCE(d.PHIVANCHUYEN, 0) as PhiVanChuyen,
                            CAST(d.DNHACUNGCAPID AS VARCHAR(50)) as DnhacungcapId,
                            ncc.NAME as TenNhaCungCap,
                            CAST(d.DKHONHAPID AS VARCHAR(50)) as DkhoNhapId,
                            k.NAME as TenKhoNhap,
                            CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) as DnhanVienNhapId,
                            nv.NAME as TenNhanVienNhap,
                            d.NOTE as Note,
                            d.DIENGIAI as DienGiai,
                            CAST(d.DCUAHANGID AS VARCHAR(50)) as DcuaHangId,
                            ch.NAME as TenCuaHang
                        FROM TDONHANG d
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(d.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        WHERE d.LOAI = 1 AND (d.STATUS IS NULL OR d.STATUS <> 0) AND CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId
                        ORDER BY d.NGAY DESC, d.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new { NccId = nccId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dt = null;
                        if (r.NGAY != null)
                        {
                            if (DateTime.TryParse(r.NGAY.ToString(), out DateTime parsedDt))
                            {
                                dt = parsedDt;
                            }
                        }

                        decimal tong = Convert.ToDecimal(r.TONGCONG ?? 0);
                        decimal tienHang = Convert.ToDecimal(r.TIENHANG ?? 0);
                        decimal tienGiam = Convert.ToDecimal(r.TIENGIAMGIA ?? 0);
                        decimal tiLeGiam = Convert.ToDecimal(r.TILEGIAMGIA ?? 0);
                        decimal tienThue = Convert.ToDecimal(r.TIENTHUE ?? 0);
                        decimal tiLeThue = Convert.ToDecimal(r.TILETHUE ?? 0);
                        decimal phiVC = Convert.ToDecimal(r.PHIVANCHUYEN ?? 0);

                        list.Add(new ChungTuItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            GhiChu = r.NOTE?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dt,
                            TongCong = tong,
                            PhiVanChuyen = phiVC,
                            TienGiamGia = tienGiam,
                            TiLeGiamGia = tiLeGiam,
                            TienThue = tienThue,
                            TiLeThue = tiLeThue,
                            TienHang = tienHang,
                            NhaCungCap = r.TENNHACUNGCAP?.ToString() ?? "",
                            KhoNhap = r.TENKHONHAP?.ToString() ?? "",
                            NhanVienNhap = r.TENNHANVIENNHAP?.ToString() ?? "",
                            DienGiai = r.DIENGIAI?.ToString() ?? "Nhập mua hàng",
                            CuaHang = r.TENCUAHANG?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPhieuNhapKhoByNccAsync error: " + ex.Message);
            }
            return list;
        }
    }
}
