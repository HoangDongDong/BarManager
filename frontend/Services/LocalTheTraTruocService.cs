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
    public static class LocalTheTraTruocService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        public static async Task<ObservableCollection<NhomTheTraTruocTreeItem>> GetNhomTheTraTruocTreeAsync()
        {
            var result = new ObservableCollection<NhomTheTraTruocTreeItem>();

            var rootAll = new NhomTheTraTruocTreeItem
            {
                Id = "ALL",
                Name = "Tất cả",
                Icon = "🌐",
                IconColor = "#2b78e4",
                IsExpanded = true,
                IsSelected = true,
                ItemType = 0
            };

            var nodeUnset = new NhomTheTraTruocTreeItem
            {
                Id = "UNSET",
                Name = "Chưa thiết lập",
                Icon = "✳️",
                IconColor = "#f0ad4e",
                ParentId = "ALL",
                ItemType = 3
            };
            rootAll.Children.Add(nodeUnset);

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT ID, NAME, NOTE, STATUS, PARENTID, SORTORDER 
                        FROM DNHOMTHETRATRUOC 
                        WHERE (STATUS IS NULL OR STATUS > 0)
                        ORDER BY SORTORDER, NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();

                    var lookup = new Dictionary<string, NhomTheTraTruocTreeItem>();

                    foreach (var r in rows)
                    {
                        string id = r.ID?.ToString() ?? "";
                        if (string.IsNullOrEmpty(id)) continue;

                        var item = new NhomTheTraTruocTreeItem
                        {
                            Id = id,
                            Name = r.NAME?.ToString() ?? "",
                            ParentId = r.PARENTID?.ToString() ?? "ALL",
                            Icon = "📁",
                            IconColor = "#f0ad4e",
                            IsExpanded = true,
                            ItemType = 2
                        };
                        lookup[id] = item;
                    }

                    // Build parent-child tree
                    foreach (var kvp in lookup)
                    {
                        var item = kvp.Value;
                        if (!string.IsNullOrEmpty(item.ParentId) && lookup.ContainsKey(item.ParentId))
                        {
                            lookup[item.ParentId].Children.Add(item);
                        }
                        else
                        {
                            rootAll.Children.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetNhomTheTraTruocTreeAsync: " + ex.Message);
            }

            var nodeTrash = new NhomTheTraTruocTreeItem
            {
                Id = "TRASH",
                Name = "Thùng rác",
                Icon = "🗑️",
                IconColor = "#718096",
                ParentId = "ALL",
                ItemType = 4
            };
            rootAll.Children.Add(nodeTrash);

            result.Add(rootAll);
            return result;
        }

        public static async Task<List<TheTraTruocViewModel>> GetTheTraTruocListAsync(string nhomId = "ALL", string keyword = "")
        {
            var result = new List<TheTraTruocViewModel>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            t.ID, 
                            t.NAME, 
                            t.NOTE, 
                            t.DNHOMTHETRATRUOCID, 
                            t.KHOA, 
                            t.NGAYHETHAN, 
                            t.STATUS, 
                            t.TIMECREATED, 
                            t.USERCREATEDID, 
                            t.TIMEMODIFIED, 
                            t.USERMODIFIEDID,
                            n.NAME AS TENNHOM,
                            u1.NAME AS TENUSERTAO,
                            u2.NAME AS TENUSERSUA
                        FROM DTHETRATRUOC t
                        LEFT JOIN DNHOMTHETRATRUOC n ON CAST(t.DNHOMTHETRATRUOCID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u1 ON CAST(t.USERCREATEDID AS VARCHAR(50)) = CAST(u1.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u2 ON CAST(t.USERMODIFIEDID AS VARCHAR(50)) = CAST(u2.ID AS VARCHAR(50))
                        WHERE 1=1";

                    if (nhomId == "TRASH")
                    {
                        sql += " AND t.STATUS = 0";
                    }
                    else
                    {
                        sql += " AND (t.STATUS IS NULL OR t.STATUS > 0)";

                        if (nhomId == "UNSET")
                        {
                            sql += " AND (t.DNHOMTHETRATRUOCID IS NULL OR t.DNHOMTHETRATRUOCID = '')";
                        }
                        else if (!string.IsNullOrEmpty(nhomId) && nhomId != "ALL")
                        {
                            sql += " AND CAST(t.DNHOMTHETRATRUOCID AS VARCHAR(50)) = @NhomId";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " AND (UPPER(t.NAME) LIKE @Keyword OR UPPER(t.NOTE) LIKE @Keyword OR UPPER(n.NAME) LIKE @Keyword)";
                    }

                    sql += " ORDER BY t.NAME ASC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        NhomId = nhomId, 
                        Keyword = "%" + (keyword?.Trim().ToUpper() ?? "") + "%" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtHetHan = null;
                        if (r.NGAYHETHAN != null)
                        {
                            try { dtHetHan = Convert.ToDateTime(r.NGAYHETHAN); } catch { }
                        }

                        DateTime? dtTao = null;
                        if (r.TIMECREATED != null)
                        {
                            try { dtTao = Convert.ToDateTime(r.TIMECREATED); } catch { }
                        }

                        DateTime? dtSua = null;
                        if (r.TIMEMODIFIED != null)
                        {
                            try { dtSua = Convert.ToDateTime(r.TIMEMODIFIED); } catch { }
                        }

                        int khoaVal = 0;
                        if (r.KHOA != null)
                        {
                            int.TryParse(r.KHOA.ToString(), out khoaVal);
                        }

                        result.Add(new TheTraTruocViewModel
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            MaThe = r.NAME?.ToString() ?? "",
                            GhiChu = r.NOTE?.ToString() ?? "",
                            DnhomthetratruocId = r.DNHOMTHETRATRUOCID?.ToString() ?? "",
                            TenNhomTheTraTruoc = r.TENNHOM?.ToString() ?? "",
                            Khoa = (khoaVal == 1),
                            NgayHetHan = dtHetHan,
                            Status = r.STATUS != null ? (int?)Convert.ToInt32(r.STATUS) : 30,
                            TimeCreated = dtTao,
                            UserCreatedId = r.USERCREATEDID?.ToString() ?? "",
                            UserCreatedName = r.TENUSERTAO?.ToString() ?? (r.USERCREATEDID != null ? "Administrator" : ""),
                            TimeModified = dtSua,
                            UserModifiedId = r.USERMODIFIEDID?.ToString() ?? "",
                            UserModifiedName = r.TENUSERSUA?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTheTraTruocListAsync error: " + ex.Message);
            }

            return result;
        }

        public static async Task<(bool Success, string Message)> SaveTheTraTruocAsync(TheTraTruocViewModel item, bool isNew)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    DateTime now = DateTime.Now;
                    string userId = SessionContext.CurrentUser?.Id ?? "1";
                    int khoaVal = item.Khoa ? 1 : 0;
                    string nhomId = !string.IsNullOrEmpty(item.DnhomthetratruocId) && item.DnhomthetratruocId != "ALL" && item.DnhomthetratruocId != "UNSET" && item.DnhomthetratruocId != "TRASH" 
                        ? item.DnhomthetratruocId 
                        : null;

                    if (isNew)
                    {
                        // Check if card number already exists
                        string checkSql = "SELECT COUNT(*) FROM DTHETRATRUOC WHERE UPPER(NAME) = @Name AND (STATUS IS NULL OR STATUS > 0)";
                        int count = await conn.ExecuteScalarAsync<int>(checkSql, new { Name = item.MaThe.Trim().ToUpper() });
                        if (count > 0)
                        {
                            return (false, $"Mã thẻ '{item.MaThe}' đã tồn tại trong hệ thống!");
                        }

                        string newId = Guid.NewGuid().ToString();

                        string sql = @"
                            INSERT INTO DTHETRATRUOC (
                                ID, NAME, NOTE, DNHOMTHETRATRUOCID, KHOA, NGAYHETHAN, 
                                STATUS, TIMECREATED, TIMEMODIFIED, USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, @Note, @NhomId, @Khoa, @NgayHetHan,
                                30, @Now, @Now, @UserId, @UserId
                            )";

                        await conn.ExecuteAsync(sql, new
                        {
                            Id = newId,
                            Name = item.MaThe.Trim(),
                            Note = item.GhiChu?.Trim() ?? "",
                            NhomId = nhomId,
                            Khoa = khoaVal,
                            NgayHetHan = item.NgayHetHan,
                            Now = now,
                            UserId = userId
                        });
                    }
                    else
                    {
                        string checkSql = "SELECT COUNT(*) FROM DTHETRATRUOC WHERE UPPER(NAME) = @Name AND CAST(ID AS VARCHAR(50)) <> @Id AND (STATUS IS NULL OR STATUS > 0)";
                        int count = await conn.ExecuteScalarAsync<int>(checkSql, new { Name = item.MaThe.Trim().ToUpper(), Id = item.Id });
                        if (count > 0)
                        {
                            return (false, $"Mã thẻ '{item.MaThe}' đã tồn tại trong hệ thống!");
                        }

                        string sql = @"
                            UPDATE DTHETRATRUOC SET 
                                NAME = @Name,
                                NOTE = @Note,
                                DNHOMTHETRATRUOCID = @NhomId,
                                KHOA = @Khoa,
                                NGAYHETHAN = @NgayHetHan,
                                TIMEMODIFIED = @Now,
                                USERMODIFIEDID = @UserId
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(sql, new
                        {
                            Id = item.Id,
                            Name = item.MaThe.Trim(),
                            Note = item.GhiChu?.Trim() ?? "",
                            NhomId = nhomId,
                            Khoa = khoaVal,
                            NgayHetHan = item.NgayHetHan,
                            Now = now,
                            UserId = userId
                        });
                    }

                    return (true, "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveTheTraTruocAsync error: " + ex.Message);
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteTheTraTruocAsync(string id, bool permanent = false)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = permanent 
                        ? "DELETE FROM DTHETRATRUOC WHERE CAST(ID AS VARCHAR(50)) = @Id"
                        : "UPDATE DTHETRATRUOC SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id";

                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteTheTraTruocAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreTheTraTruocAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "UPDATE DTHETRATRUOC SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RestoreTheTraTruocAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<List<TheTraTruocHoaDonItem>> GetLichSuHoaDonTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocHoaDonItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            h.ID,
                            h.NAME AS SOPHIEU,
                            h.NGAY,
                            k.NAME AS KHACHHANG,
                            h.TONGCONG,
                            nv.NAME AS NHANVIENBAN,
                            h.GIOTHANHTOAN,
                            tn.NAME AS THUNGAN,
                            h.VOUCHER,
                            h.TRICHNHANVIEN,
                            ch.NAME AS CUAHANG,
                            h.CONLAI,
                            h.THANHTOAN,
                            h.THETRATRUOC AS THETT,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            h.TRUTICHLUY,
                            h.DIEMGIAM,
                            h.TIENMAT,
                            h.CHUYENKHOAN,
                            h.THE,
                            b.NAME AS BAN,
                            h.BATDAU,
                            h.KETTHUC,
                            h.TIENGIO,
                            h.TILEGIAMGIAGIO,
                            h.TIENGIAMGIAGIO,
                            h.SOKHACH,
                            h.PHIDICHVU,
                            h.TILEPHIDICHVU,
                            h.TILEGIAMGIATONG,
                            h.TIENGIAMGIATONG,
                            h.SOORDER,
                            h.SOHD,
                            h.SOTT,
                            h.SOLANINTAMTINH,
                            h.DONGIA,
                            h.TIENGIOPHONGCUOI,
                            h.BATDAUPHONGCUOI,
                            h.TIENMOBAN,
                            h.LANINHOADON,
                            h.PHUTKHUYENMAI,
                            h.INTAMTINHLUC,
                            h.DATTRUOC,
                            h.CONGNO,
                            h.TIENHANGCHUAGIAM,
                            h.GIAMGIAMATHANG,
                            h.TILEKHUYENMAIPHUTDAU,
                            h.NOTE
                        FROM TDONHANG h
                        LEFT JOIN DTHETRATRUOC t ON CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG k ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN SUSER nv ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN SUSER tn ON CAST(h.USERTHANHTOANID AS VARCHAR(50)) = CAST(tn.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(h.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        WHERE (h.STATUS IS NULL OR h.STATUS > 0)
                          AND (CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY h.NGAY DESC, h.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        DateTime? dtBatDau = null;
                        if (r.BATDAU != null) { try { dtBatDau = Convert.ToDateTime(r.BATDAU); } catch { } }

                        DateTime? dtKetThuc = null;
                        if (r.KETTHUC != null) { try { dtKetThuc = Convert.ToDateTime(r.KETTHUC); } catch { } }

                        DateTime? dtInTamTinh = null;
                        if (r.INTAMTINHLUC != null) { try { dtInTamTinh = Convert.ToDateTime(r.INTAMTINHLUC); } catch { } }

                        DateTime? dtBatDauCuoi = null;
                        if (r.BATDAUPHONGCUOI != null) { try { dtBatDauCuoi = Convert.ToDateTime(r.BATDAUPHONGCUOI); } catch { } }

                        string gioTt = "";
                        if (r.GIOTHANHTOAN != null)
                        {
                            try { gioTt = Convert.ToDateTime(r.GIOTHANHTOAN).ToString("HH:mm"); } catch { gioTt = r.GIOTHANHTOAN.ToString(); }
                        }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        int ParseInt(object val)
                        {
                            if (val == null) return 0;
                            if (int.TryParse(val.ToString(), out int i)) return i;
                            return 0;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocHoaDonItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            KhachHang = r.KHACHHANG?.ToString() ?? "",
                            TongCong = ParseDec(r.TONGCONG),
                            NhanVienBan = r.NHANVIENBAN?.ToString() ?? "",
                            GioThanhToan = gioTt,
                            ThuNgan = r.THUNGAN?.ToString() ?? "",
                            Voucher = ParseDec(r.VOUCHER),
                            TrichNhanVien = ParseDec(r.TRICHNHANVIEN),
                            CuaHang = cuahangName,
                            ConLai = ParseDec(r.CONLAI),
                            ThanhToan = ParseDec(r.THANHTOAN),
                            TheTt = ParseDec(r.THETT),
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            TruTichLuy = ParseDec(r.TRUTICHLUY),
                            DiemGiam = ParseDec(r.DIEMGIAM),
                            TienMat = ParseDec(r.TIENMAT),
                            ChuyenKhoan = ParseDec(r.CHUYENKHOAN),
                            The = ParseDec(r.THE),
                            Ban = r.BAN?.ToString() ?? "",
                            BatDau = dtBatDau,
                            KetThuc = dtKetThuc,
                            TienGio = ParseDec(r.TIENGIO),
                            TiLeGiamGiaGio = ParseDec(r.TILEGIAMGIAGIO),
                            TienGiamGiaGio = ParseDec(r.TIENGIAMGIAGIO),
                            SoKhach = ParseInt(r.SOKHACH),
                            PhiDichVu = ParseDec(r.PHIDICHVU),
                            TiLePhiDichVu = ParseDec(r.TILEPHIDICHVU),
                            TiLeGiamGiaTong = ParseDec(r.TILEGIAMGIATONG),
                            TienGiamGiaTong = ParseDec(r.TIENGIAMGIATONG),
                            SoOrder = r.SOORDER?.ToString() ?? "",
                            SoHoaDon = r.SOHD?.ToString() ?? "",
                            SoThanhToan = r.SOTT?.ToString() ?? "",
                            SoLanInTamTinh = ParseInt(r.SOLANINTAMTINH),
                            DonGia = ParseDec(r.DONGIA),
                            TienGioPhongCuoi = ParseDec(r.TIENGIOPHONGCUOI),
                            BatDauPhongCuoi = dtBatDauCuoi,
                            TienMoBan = ParseDec(r.TIENMOBAN),
                            LanInHoaDon = ParseInt(r.LANINHOADON),
                            PhutKhuyenMai = ParseDec(r.PHUTKHUYENMAI),
                            InTamTinhLuc = dtInTamTinh,
                            DatTruoc = ParseDec(r.DATTRUOC),
                            CongNo = ParseDec(r.CONGNO),
                            TienHangChuaGiam = ParseDec(r.TIENHANGCHUAGIAM),
                            GiamGiaMatHang = ParseDec(r.GIAMGIAMATHANG),
                            TiLeKhuyenMaiPhutDau = ParseDec(r.TILEKHUYENMAIPHUTDAU),
                            GhiChu = r.NOTE?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuHoaDonTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocNhapKhoItem>> GetLichSuNhapKhoTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocNhapKhoItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            h.ID,
                            h.NOTE AS GHICHU,
                            h.NAME AS SOPHIEU,
                            h.NGAY,
                            h.TONGCONG,
                            h.PHIVANCHUYEN,
                            h.TIENGIAMGIA,
                            h.TILEGIAMGIA,
                            h.TIENTHUE,
                            h.TILETHUE,
                            h.TIENHANG,
                            ncc.NAME AS NHACUNGCAP,
                            kho.NAME AS KHONHAP,
                            nv.NAME AS NHANVIENNHAP,
                            h.DIENGIAI,
                            h.VOUCHER,
                            nvgh.NAME AS NHANVIENGIAOHANG,
                            h.TRICHNHANVIEN,
                            ch.NAME AS CUAHANG,
                            h.CONLAI,
                            h.THANHTOAN,
                            h.THETRATRUOC AS THETT,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            h.TRUTICHLUY,
                            h.DIEMGIAM,
                            h.TIENMAT,
                            h.CHUYENKHOAN,
                            h.THE,
                            b.NAME AS BAN,
                            h.BATDAU,
                            h.KETTHUC,
                            h.TIENGIO,
                            h.TILEGIAMGIAGIO,
                            h.TIENGIAMGIAGIO,
                            h.SOKHACH,
                            h.PHIDICHVU,
                            h.TILEPHIDICHVU,
                            h.TILEGIAMGIATONG,
                            h.TIENGIAMGIATONG,
                            h.SOORDER,
                            h.SOHD,
                            h.SOTT,
                            h.SOLANINTAMTINH,
                            h.DONGIA,
                            h.TIENGIOPHONGCUOI,
                            h.BATDAUPHONGCUOI,
                            h.TIENMOBAN,
                            h.LANINHOADON,
                            h.PHUTKHUYENMAI,
                            h.INTAMTINHLUC,
                            h.DATTRUOC,
                            h.CONGNO,
                            h.TIENHANGCHUAGIAM,
                            h.GIAMGIAMATHANG,
                            h.TILEKHUYENMAIPHUTDAU
                        FROM TDONHANG h
                        LEFT JOIN DTHETRATRUOC t ON CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(h.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG kho ON CAST(h.DKHONHAPID AS VARCHAR(50)) = CAST(kho.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN SUSER nvgh ON CAST(h.DNHANVIENGIAOHANGID AS VARCHAR(50)) = CAST(nvgh.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(h.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE (h.STATUS IS NULL OR h.STATUS > 0)
                          AND h.LOAI = 1
                          AND (CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY h.NGAY DESC, h.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        DateTime? dtBatDau = null;
                        if (r.BATDAU != null) { try { dtBatDau = Convert.ToDateTime(r.BATDAU); } catch { } }

                        DateTime? dtKetThuc = null;
                        if (r.KETTHUC != null) { try { dtKetThuc = Convert.ToDateTime(r.KETTHUC); } catch { } }

                        DateTime? dtInTamTinh = null;
                        if (r.INTAMTINHLUC != null) { try { dtInTamTinh = Convert.ToDateTime(r.INTAMTINHLUC); } catch { } }

                        DateTime? dtBatDauCuoi = null;
                        if (r.BATDAUPHONGCUOI != null) { try { dtBatDauCuoi = Convert.ToDateTime(r.BATDAUPHONGCUOI); } catch { } }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        int ParseInt(object val)
                        {
                            if (val == null) return 0;
                            if (int.TryParse(val.ToString(), out int i)) return i;
                            return 0;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocNhapKhoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            GhiChu = r.GHICHU?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            TongCong = ParseDec(r.TONGCONG),
                            PhiVanChuyen = ParseDec(r.PHIVANCHUYEN),
                            TienGiamGia = ParseDec(r.TIENGIAMGIA),
                            TiLeGiamGia = ParseDec(r.TILEGIAMGIA),
                            TienThue = ParseDec(r.TIENTHUE),
                            TiLeThue = ParseDec(r.TILETHUE),
                            TienHang = ParseDec(r.TIENHANG),
                            NhaCungCap = r.NHACUNGCAP?.ToString() ?? "",
                            KhoNhap = r.KHONHAP?.ToString() ?? "",
                            NhanVienNhap = r.NHANVIENNHAP?.ToString() ?? "",
                            DienGiai = r.DIENGIAI?.ToString() ?? "",
                            Voucher = ParseDec(r.VOUCHER),
                            TrichNhanVien = ParseDec(r.TRICHNHANVIEN),
                            CuaHang = cuahangName,
                            ConLai = ParseDec(r.CONLAI),
                            ThanhToan = ParseDec(r.THANHTOAN),
                            TheTt = ParseDec(r.THETT),
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            TruTichLuy = ParseDec(r.TRUTICHLUY),
                            DiemGiam = ParseDec(r.DIEMGIAM),
                            TienMat = ParseDec(r.TIENMAT),
                            ChuyenKhoan = ParseDec(r.CHUYENKHOAN),
                            The = ParseDec(r.THE),
                            Ban = r.BAN?.ToString() ?? "",
                            BatDau = dtBatDau,
                            KetThuc = dtKetThuc,
                            TienGio = ParseDec(r.TIENGIO),
                            TiLeGiamGiaGio = ParseDec(r.TILEGIAMGIAGIO),
                            TienGiamGiaGio = ParseDec(r.TIENGIAMGIAGIO),
                            SoKhach = ParseInt(r.SOKHACH),
                            PhiDichVu = ParseDec(r.PHIDICHVU),
                            TiLePhiDichVu = ParseDec(r.TILEPHIDICHVU),
                            TiLeGiamGiaTong = ParseDec(r.TILEGIAMGIATONG),
                            TienGiamGiaTong = ParseDec(r.TIENGIAMGIATONG),
                            SoOrder = r.SOORDER?.ToString() ?? "",
                            SoHoaDon = r.SOHD?.ToString() ?? "",
                            SoThanhToan = r.SOTT?.ToString() ?? "",
                            SoLanInTamTinh = ParseInt(r.SOLANINTAMTINH),
                            DonGia = ParseDec(r.DONGIA),
                            TienGioPhongCuoi = ParseDec(r.TIENGIOPHONGCUOI),
                            BatDauPhongCuoi = dtBatDauCuoi,
                            TienMoBan = ParseDec(r.TIENMOBAN),
                            LanInHoaDon = ParseInt(r.LANINHOADON),
                            PhutKhuyenMai = ParseDec(r.PHUTKHUYENMAI),
                            InTamTinhLuc = dtInTamTinh,
                            DatTruoc = ParseDec(r.DATTRUOC),
                            CongNo = ParseDec(r.CONGNO),
                            TienHangChuaGiam = ParseDec(r.TIENHANGCHUAGIAM),
                            GiamGiaMatHang = ParseDec(r.GIAMGIAMATHANG),
                            TiLeKhuyenMaiPhutDau = ParseDec(r.TILEKHUYENMAIPHUTDAU)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuNhapKhoTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocXuatKhoItem>> GetLichSuXuatKhoTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocXuatKhoItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            h.ID,
                            h.NOTE AS GHICHU,
                            h.NAME AS SOPHIEU,
                            h.NGAY,
                            h.TONGCONG,
                            h.PHIVANCHUYEN,
                            h.TIENGIAMGIA,
                            h.TILEGIAMGIA,
                            h.TIENTHUE,
                            h.TILETHUE,
                            h.TIENHANG,
                            kho.NAME AS KHOXUAT,
                            nv.NAME AS NHANVIENXUAT,
                            h.VOUCHER,
                            nvgh.NAME AS NHANVIENGIAOHANG,
                            h.TRICHNHANVIEN,
                            ch.NAME AS CUAHANG,
                            h.CONLAI,
                            h.THANHTOAN,
                            h.THETRATRUOC AS THETT,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            h.TRUTICHLUY,
                            h.DIEMGIAM,
                            h.TIENMAT,
                            h.CHUYENKHOAN,
                            h.THE,
                            b.NAME AS BAN,
                            h.BATDAU,
                            h.KETTHUC,
                            h.TIENGIO,
                            h.TILEGIAMGIAGIO,
                            h.TIENGIAMGIAGIO,
                            h.SOKHACH,
                            h.PHIDICHVU,
                            h.TILEPHIDICHVU,
                            h.TILEGIAMGIATONG,
                            h.TIENGIAMGIATONG,
                            h.SOORDER,
                            h.SOHD,
                            h.SOTT,
                            h.SOLANINTAMTINH,
                            h.DONGIA,
                            h.TIENGIOPHONGCUOI,
                            h.BATDAUPHONGCUOI,
                            h.TIENMOBAN,
                            h.LANINHOADON,
                            h.PHUTKHUYENMAI,
                            h.INTAMTINHLUC,
                            h.DATTRUOC,
                            h.CONGNO,
                            h.TIENHANGCHUAGIAM,
                            h.GIAMGIAMATHANG,
                            h.TILEKHUYENMAIPHUTDAU,
                            h.PASSWIFI
                        FROM TDONHANG h
                        LEFT JOIN DTHETRATRUOC t ON CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG kho ON CAST(h.DKHOXUATID AS VARCHAR(50)) = CAST(kho.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN SUSER nvgh ON CAST(h.DNHANVIENGIAOHANGID AS VARCHAR(50)) = CAST(nvgh.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(h.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE (h.STATUS IS NULL OR h.STATUS > 0)
                          AND h.LOAI = 2
                          AND (CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY h.NGAY DESC, h.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        DateTime? dtBatDau = null;
                        if (r.BATDAU != null) { try { dtBatDau = Convert.ToDateTime(r.BATDAU); } catch { } }

                        DateTime? dtKetThuc = null;
                        if (r.KETTHUC != null) { try { dtKetThuc = Convert.ToDateTime(r.KETTHUC); } catch { } }

                        DateTime? dtInTamTinh = null;
                        if (r.INTAMTINHLUC != null) { try { dtInTamTinh = Convert.ToDateTime(r.INTAMTINHLUC); } catch { } }

                        DateTime? dtBatDauCuoi = null;
                        if (r.BATDAUPHONGCUOI != null) { try { dtBatDauCuoi = Convert.ToDateTime(r.BATDAUPHONGCUOI); } catch { } }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        int ParseInt(object val)
                        {
                            if (val == null) return 0;
                            if (int.TryParse(val.ToString(), out int i)) return i;
                            return 0;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocXuatKhoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            GhiChu = r.GHICHU?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            TongCong = ParseDec(r.TONGCONG),
                            PhiVanChuyen = ParseDec(r.PHIVANCHUYEN),
                            TienGiamGia = ParseDec(r.TIENGIAMGIA),
                            TiLeGiamGia = ParseDec(r.TILEGIAMGIA),
                            TienThue = ParseDec(r.TIENTHUE),
                            TiLeThue = ParseDec(r.TILETHUE),
                            TienHang = ParseDec(r.TIENHANG),
                            KhoXuat = r.KHOXUAT?.ToString() ?? "",
                            NhanVienXuat = r.NHANVIENXUAT?.ToString() ?? "",
                            Voucher = ParseDec(r.VOUCHER),
                            TrichNhanVien = ParseDec(r.TRICHNHANVIEN),
                            CuaHang = cuahangName,
                            ConLai = ParseDec(r.CONLAI),
                            ThanhToan = ParseDec(r.THANHTOAN),
                            TheTt = ParseDec(r.THETT),
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            TruTichLuy = ParseDec(r.TRUTICHLUY),
                            DiemGiam = ParseDec(r.DIEMGIAM),
                            TienMat = ParseDec(r.TIENMAT),
                            ChuyenKhoan = ParseDec(r.CHUYENKHOAN),
                            The = ParseDec(r.THE),
                            Ban = r.BAN?.ToString() ?? "",
                            BatDau = dtBatDau,
                            KetThuc = dtKetThuc,
                            TienGio = ParseDec(r.TIENGIO),
                            TiLeGiamGiaGio = ParseDec(r.TILEGIAMGIAGIO),
                            TienGiamGiaGio = ParseDec(r.TIENGIAMGIAGIO),
                            SoKhach = ParseInt(r.SOKHACH),
                            PhiDichVu = ParseDec(r.PHIDICHVU),
                            TiLePhiDichVu = ParseDec(r.TILEPHIDICHVU),
                            TiLeGiamGiaTong = ParseDec(r.TILEGIAMGIATONG),
                            TienGiamGiaTong = ParseDec(r.TIENGIAMGIATONG),
                            SoOrder = r.SOORDER?.ToString() ?? "",
                            SoHoaDon = r.SOHD?.ToString() ?? "",
                            SoThanhToan = r.SOTT?.ToString() ?? "",
                            SoLanInTamTinh = ParseInt(r.SOLANINTAMTINH),
                            DonGia = ParseDec(r.DONGIA),
                            TienGioPhongCuoi = ParseDec(r.TIENGIOPHONGCUOI),
                            BatDauPhongCuoi = dtBatDauCuoi,
                            TienMoBan = ParseDec(r.TIENMOBAN),
                            LanInHoaDon = ParseInt(r.LANINHOADON),
                            PhutKhuyenMai = ParseDec(r.PHUTKHUYENMAI),
                            InTamTinhLuc = dtInTamTinh,
                            DatTruoc = ParseDec(r.DATTRUOC),
                            CongNo = ParseDec(r.CONGNO),
                            TienHangChuaGiam = ParseDec(r.TIENHANGCHUAGIAM),
                            GiamGiaMatHang = ParseDec(r.GIAMGIAMATHANG),
                            TiLeKhuyenMaiPhutDau = ParseDec(r.TILEKHUYENMAIPHUTDAU),
                            PassWifi = r.PASSWIFI?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuXuatKhoTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocChuyenKhoItem>> GetLichSuChuyenKhoTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocChuyenKhoItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            h.ID,
                            h.NOTE AS GHICHU,
                            h.NAME AS SOPHIEU,
                            h.NGAY,
                            h.TONGCONG,
                            h.PHIVANCHUYEN,
                            h.TIENGIAMGIA,
                            h.TILEGIAMGIA,
                            h.TIENTHUE,
                            h.TILETHUE,
                            h.TIENHANG,
                            khox.NAME AS KHOXUAT,
                            khon.NAME AS KHONHAP,
                            nv.NAME AS NHANVIENXUAT,
                            h.VOUCHER,
                            nvgh.NAME AS NHANVIENGIAOHANG,
                            h.TRICHNHANVIEN,
                            ch.NAME AS CUAHANG,
                            h.CONLAI,
                            h.THANHTOAN,
                            h.THETRATRUOC AS THETT,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            h.TRUTICHLUY,
                            h.DIEMGIAM,
                            h.TIENMAT,
                            h.CHUYENKHOAN,
                            h.THE,
                            b.NAME AS BAN,
                            h.BATDAU,
                            h.KETTHUC,
                            h.TIENGIO,
                            h.TILEGIAMGIAGIO,
                            h.TIENGIAMGIAGIO,
                            h.SOKHACH,
                            h.PHIDICHVU,
                            h.TILEPHIDICHVU,
                            h.TILEGIAMGIATONG,
                            h.TIENGIAMGIATONG,
                            h.SOORDER,
                            h.SOHD,
                            h.SOTT,
                            h.SOLANINTAMTINH,
                            h.DONGIA,
                            h.TIENGIOPHONGCUOI,
                            h.BATDAUPHONGCUOI,
                            h.TIENMOBAN,
                            h.LANINHOADON,
                            h.PHUTKHUYENMAI,
                            h.INTAMTINHLUC,
                            h.DATTRUOC,
                            h.CONGNO,
                            h.TIENHANGCHUAGIAM,
                            h.GIAMGIAMATHANG,
                            h.TILEKHUYENMAIPHUTDAU,
                            h.PASSWIFI
                        FROM TDONHANG h
                        LEFT JOIN DTHETRATRUOC t ON CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG khox ON CAST(h.DKHOXUATID AS VARCHAR(50)) = CAST(khox.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG khon ON CAST(h.DKHONHAPID AS VARCHAR(50)) = CAST(khon.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN SUSER nvgh ON CAST(h.DNHANVIENGIAOHANGID AS VARCHAR(50)) = CAST(nvgh.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(h.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE (h.STATUS IS NULL OR h.STATUS > 0)
                          AND h.LOAI = 3
                          AND (CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY h.NGAY DESC, h.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        DateTime? dtBatDau = null;
                        if (r.BATDAU != null) { try { dtBatDau = Convert.ToDateTime(r.BATDAU); } catch { } }

                        DateTime? dtKetThuc = null;
                        if (r.KETTHUC != null) { try { dtKetThuc = Convert.ToDateTime(r.KETTHUC); } catch { } }

                        DateTime? dtInTamTinh = null;
                        if (r.INTAMTINHLUC != null) { try { dtInTamTinh = Convert.ToDateTime(r.INTAMTINHLUC); } catch { } }

                        DateTime? dtBatDauCuoi = null;
                        if (r.BATDAUPHONGCUOI != null) { try { dtBatDauCuoi = Convert.ToDateTime(r.BATDAUPHONGCUOI); } catch { } }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        int ParseInt(object val)
                        {
                            if (val == null) return 0;
                            if (int.TryParse(val.ToString(), out int i)) return i;
                            return 0;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocChuyenKhoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            GhiChu = r.GHICHU?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            TongCong = ParseDec(r.TONGCONG),
                            PhiVanChuyen = ParseDec(r.PHIVANCHUYEN),
                            TienGiamGia = ParseDec(r.TIENGIAMGIA),
                            TiLeGiamGia = ParseDec(r.TILEGIAMGIA),
                            TienThue = ParseDec(r.TIENTHUE),
                            TiLeThue = ParseDec(r.TILETHUE),
                            TienHang = ParseDec(r.TIENHANG),
                            KhoXuat = r.KHOXUAT?.ToString() ?? "",
                            KhoNhap = r.KHONHAP?.ToString() ?? "",
                            NhanVienXuat = r.NHANVIENXUAT?.ToString() ?? "",
                            Voucher = ParseDec(r.VOUCHER),
                            TrichNhanVien = ParseDec(r.TRICHNHANVIEN),
                            CuaHang = cuahangName,
                            ConLai = ParseDec(r.CONLAI),
                            ThanhToan = ParseDec(r.THANHTOAN),
                            TheTt = ParseDec(r.THETT),
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            TruTichLuy = ParseDec(r.TRUTICHLUY),
                            DiemGiam = ParseDec(r.DIEMGIAM),
                            TienMat = ParseDec(r.TIENMAT),
                            ChuyenKhoan = ParseDec(r.CHUYENKHOAN),
                            The = ParseDec(r.THE),
                            Ban = r.BAN?.ToString() ?? "",
                            BatDau = dtBatDau,
                            KetThuc = dtKetThuc,
                            TienGio = ParseDec(r.TIENGIO),
                            TiLeGiamGiaGio = ParseDec(r.TILEGIAMGIAGIO),
                            TienGiamGiaGio = ParseDec(r.TIENGIAMGIAGIO),
                            SoKhach = ParseInt(r.SOKHACH),
                            PhiDichVu = ParseDec(r.PHIDICHVU),
                            TiLePhiDichVu = ParseDec(r.TILEPHIDICHVU),
                            TiLeGiamGiaTong = ParseDec(r.TILEGIAMGIATONG),
                            TienGiamGiaTong = ParseDec(r.TIENGIAMGIATONG),
                            SoOrder = r.SOORDER?.ToString() ?? "",
                            SoHoaDon = r.SOHD?.ToString() ?? "",
                            SoThanhToan = r.SOTT?.ToString() ?? "",
                            SoLanInTamTinh = ParseInt(r.SOLANINTAMTINH),
                            DonGia = ParseDec(r.DONGIA),
                            TienGioPhongCuoi = ParseDec(r.TIENGIOPHONGCUOI),
                            BatDauPhongCuoi = dtBatDauCuoi,
                            TienMoBan = ParseDec(r.TIENMOBAN),
                            LanInHoaDon = ParseInt(r.LANINHOADON),
                            PhutKhuyenMai = ParseDec(r.PHUTKHUYENMAI),
                            InTamTinhLuc = dtInTamTinh,
                            DatTruoc = ParseDec(r.DATTRUOC),
                            CongNo = ParseDec(r.CONGNO),
                            TienHangChuaGiam = ParseDec(r.TIENHANGCHUAGIAM),
                            GiamGiaMatHang = ParseDec(r.GIAMGIAMATHANG),
                            TiLeKhuyenMaiPhutDau = ParseDec(r.TILEKHUYENMAIPHUTDAU),
                            PassWifi = r.PASSWIFI?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuChuyenKhoTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocKiemKeItem>> GetLichSuKiemKeTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocKiemKeItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            h.ID,
                            h.NOTE AS GHICHU,
                            h.NAME AS SOPHIEU,
                            h.NGAY,
                            COALESCE(khox.NAME, khon.NAME) AS KHOHANG,
                            COALESCE(nv.NAME, nv2.NAME) AS NHANVIEN,
                            h.DIENGIAI,
                            h.VOUCHER,
                            nvgh.NAME AS NHANVIENGIAOHANG,
                            h.TRICHNHANVIEN,
                            ch.NAME AS CUAHANG,
                            h.CONLAI,
                            h.THANHTOAN,
                            h.THETRATRUOC AS THETT,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            h.TRUTICHLUY,
                            h.DIEMGIAM,
                            h.TIENMAT,
                            h.CHUYENKHOAN,
                            h.THE,
                            b.NAME AS BAN,
                            h.BATDAU,
                            h.KETTHUC,
                            h.TIENGIO,
                            h.TILEGIAMGIAGIO,
                            h.TIENGIAMGIAGIO,
                            h.SOKHACH,
                            h.PHIDICHVU,
                            h.TILEPHIDICHVU,
                            h.TILEGIAMGIATONG,
                            h.TIENGIAMGIATONG,
                            h.SOORDER,
                            h.SOHD,
                            h.SOTT,
                            h.SOLANINTAMTINH,
                            h.DONGIA,
                            h.TIENGIOPHONGCUOI,
                            h.BATDAUPHONGCUOI,
                            h.TIENMOBAN,
                            h.LANINHOADON,
                            h.PHUTKHUYENMAI,
                            h.INTAMTINHLUC,
                            h.DATTRUOC,
                            h.CONGNO,
                            h.TIENHANGCHUAGIAM,
                            h.GIAMGIAMATHANG,
                            h.TILEKHUYENMAIPHUTDAU,
                            h.PASSWIFI
                        FROM TDONHANG h
                        LEFT JOIN DTHETRATRUOC t ON CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG khox ON CAST(h.DKHOXUATID AS VARCHAR(50)) = CAST(khox.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG khon ON CAST(h.DKHONHAPID AS VARCHAR(50)) = CAST(khon.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv2 ON CAST(h.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv2.ID AS VARCHAR(50))
                        LEFT JOIN SUSER nvgh ON CAST(h.DNHANVIENGIAOHANGID AS VARCHAR(50)) = CAST(nvgh.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(h.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE (h.STATUS IS NULL OR h.STATUS > 0)
                          AND h.LOAI = 4
                          AND (CAST(h.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY h.NGAY DESC, h.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        DateTime? dtBatDau = null;
                        if (r.BATDAU != null) { try { dtBatDau = Convert.ToDateTime(r.BATDAU); } catch { } }

                        DateTime? dtKetThuc = null;
                        if (r.KETTHUC != null) { try { dtKetThuc = Convert.ToDateTime(r.KETTHUC); } catch { } }

                        DateTime? dtInTamTinh = null;
                        if (r.INTAMTINHLUC != null) { try { dtInTamTinh = Convert.ToDateTime(r.INTAMTINHLUC); } catch { } }

                        DateTime? dtBatDauCuoi = null;
                        if (r.BATDAUPHONGCUOI != null) { try { dtBatDauCuoi = Convert.ToDateTime(r.BATDAUPHONGCUOI); } catch { } }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        int ParseInt(object val)
                        {
                            if (val == null) return 0;
                            if (int.TryParse(val.ToString(), out int i)) return i;
                            return 0;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocKiemKeItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            GhiChu = r.GHICHU?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            KhoHang = r.KHOHANG?.ToString() ?? "",
                            NhanVien = r.NHANVIEN?.ToString() ?? "",
                            DienGiai = r.DIENGIAI?.ToString() ?? "",
                            Voucher = ParseDec(r.VOUCHER),
                            TrichNhanVien = ParseDec(r.TRICHNHANVIEN),
                            CuaHang = cuahangName,
                            ConLai = ParseDec(r.CONLAI),
                            ThanhToan = ParseDec(r.THANHTOAN),
                            TheTt = ParseDec(r.THETT),
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            TruTichLuy = ParseDec(r.TRUTICHLUY),
                            DiemGiam = ParseDec(r.DIEMGIAM),
                            TienMat = ParseDec(r.TIENMAT),
                            ChuyenKhoan = ParseDec(r.CHUYENKHOAN),
                            The = ParseDec(r.THE),
                            Ban = r.BAN?.ToString() ?? "",
                            BatDau = dtBatDau,
                            KetThuc = dtKetThuc,
                            TienGio = ParseDec(r.TIENGIO),
                            TiLeGiamGiaGio = ParseDec(r.TILEGIAMGIAGIO),
                            TienGiamGiaGio = ParseDec(r.TIENGIAMGIAGIO),
                            SoKhach = ParseInt(r.SOKHACH),
                            PhiDichVu = ParseDec(r.PHIDICHVU),
                            TiLePhiDichVu = ParseDec(r.TILEPHIDICHVU),
                            TiLeGiamGiaTong = ParseDec(r.TILEGIAMGIATONG),
                            TienGiamGiaTong = ParseDec(r.TIENGIAMGIATONG),
                            SoOrder = r.SOORDER?.ToString() ?? "",
                            SoHoaDon = r.SOHD?.ToString() ?? "",
                            SoThanhToan = r.SOTT?.ToString() ?? "",
                            SoLanInTamTinh = ParseInt(r.SOLANINTAMTINH),
                            DonGia = ParseDec(r.DONGIA),
                            TienGioPhongCuoi = ParseDec(r.TIENGIOPHONGCUOI),
                            BatDauPhongCuoi = dtBatDauCuoi,
                            TienMoBan = ParseDec(r.TIENMOBAN),
                            LanInHoaDon = ParseInt(r.LANINHOADON),
                            PhutKhuyenMai = ParseDec(r.PHUTKHUYENMAI),
                            InTamTinhLuc = dtInTamTinh,
                            DatTruoc = ParseDec(r.DATTRUOC),
                            CongNo = ParseDec(r.CONGNO),
                            TienHangChuaGiam = ParseDec(r.TIENHANGCHUAGIAM),
                            GiamGiaMatHang = ParseDec(r.GIAMGIAMATHANG),
                            TiLeKhuyenMaiPhutDau = ParseDec(r.TILEKHUYENMAIPHUTDAU),
                            PassWifi = r.PASSWIFI?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuKiemKeTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocKhachHangItem>> GetKhachHangTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocKhachHangItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            k.ID,
                            k.MAKHACH,
                            k.NAME AS TENKHACHHANG,
                            k.DIACHI,
                            k.DIENTHOAI,
                            k.EMAIL,
                            nk.NAME AS NHOMKHACHHANG,
                            k.MASOTHUE,
                            nv.NAME AS NHANVIEN,
                            tt.NAME AS TINHTHANH,
                            k.FACEBOOK,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC
                        FROM DKHACHHANG k
                        LEFT JOIN DTHETRATRUOC t ON CAST(k.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMKHACHHANG nk ON CAST(k.DNHOMKHACHHANGID AS VARCHAR(50)) = CAST(nk.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(k.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DTINHTHANH tt ON CAST(k.DTINHTHANHID AS VARCHAR(50)) = CAST(tt.ID AS VARCHAR(50))
                        WHERE (k.STATUS IS NULL OR k.STATUS > 0)
                          AND (CAST(k.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new TheTraTruocKhachHangItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            MaKhach = r.MAKHACH?.ToString() ?? "",
                            TenKhachHang = r.TENKHACHHANG?.ToString() ?? "",
                            DiaChi = r.DIACHI?.ToString() ?? "",
                            DienThoai = r.DIENTHOAI?.ToString() ?? "",
                            Email = r.EMAIL?.ToString() ?? "",
                            NhomKhachHang = r.NHOMKHACHHANG?.ToString() ?? "",
                            MaSoThue = r.MASOTHUE?.ToString() ?? "",
                            NhanVien = r.NHANVIEN?.ToString() ?? "",
                            TinhThanh = r.TINHTHANH?.ToString() ?? "",
                            Facebook = r.FACEBOOK?.ToString() ?? "",
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetKhachHangTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocThuChiItem>> GetLichSuThuChiTheTraTruocAsync(string theId, string maThe, int loaiThuChi = 0)
        {
            var list = new List<TheTraTruocThuChiItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            tc.ID,
                            tc.NAME AS SOPHIEU,
                            tc.NGAY,
                            tc.TENDOITUONG,
                            tc.DIACHI,
                            lydo.NAME AS LYDOTHUCHI,
                            tc.DIENGIAI,
                            tc.CHUNGTUGOC,
                            CASE WHEN @Loai = 0 THEN tc.THU ELSE tc.CHI END AS SOTIEN,
                            tc.CHUYENKHOAN,
                            dh.NAME AS DATHANG,
                            tc.NOTE AS GHICHU,
                            ch.NAME AS CUAHANG,
                            tc.LAPHIEUTHUCONGNO,
                            tc.KHONGTHAYDOICONGNO,
                            tk.NAME AS TAIKHOANNGANHANG,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            donhang.NAME AS DONHANG
                        FROM TTHUCHI tc
                        LEFT JOIN DLYDOTHUCHI lydo ON CAST(tc.DLYDOTHUCHIID AS VARCHAR(50)) = CAST(lydo.ID AS VARCHAR(50))
                        LEFT JOIN TDATHANG dh ON CAST(tc.TDATHANGID AS VARCHAR(50)) = CAST(dh.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(tc.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(tc.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                        LEFT JOIN DTHETRATRUOC t ON CAST(tc.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN TDONHANG donhang ON CAST(tc.TDONHANGID AS VARCHAR(50)) = CAST(donhang.ID AS VARCHAR(50))
                        WHERE (tc.STATUS IS NULL OR tc.STATUS > 0)
                          AND tc.LOAI = @Loai
                          AND (CAST(tc.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY tc.NGAY DESC, tc.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "",
                        Loai = loaiThuChi
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        bool ParseBool(object val)
                        {
                            if (val == null) return false;
                            if (val is bool b) return b;
                            if (int.TryParse(val.ToString(), out int i)) return i > 0;
                            return false;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocThuChiItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            TenDoiTuong = r.TENDOITUONG?.ToString() ?? "",
                            DiaChi = r.DIACHI?.ToString() ?? "",
                            LyDoThuChi = r.LYDOTHUCHI?.ToString() ?? "",
                            DienGiai = r.DIENGIAI?.ToString() ?? "",
                            ChungTuGoc = r.CHUNGTUGOC?.ToString() ?? "",
                            SoTien = ParseDec(r.SOTIEN),
                            ChuyenKhoan = ParseBool(r.CHUYENKHOAN),
                            DatHang = r.DATHANG?.ToString() ?? "",
                            GhiChu = r.GHICHU?.ToString() ?? "",
                            CuaHang = cuahangName,
                            LaPhieuThuCongNo = ParseBool(r.LAPHIEUTHUCONGNO),
                            KhongThayDoiCongNo = ParseBool(r.KHONGTHAYDOICONGNO),
                            TaiKhoanNganHang = r.TAIKHOANNGANHANG?.ToString() ?? "",
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            DonHang = r.DONHANG?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuThuChiTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<TheTraTruocThuCongNoItem>> GetLichSuThuCongNoTheTraTruocAsync(string theId, string maThe)
        {
            var list = new List<TheTraTruocThuCongNoItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            tc.ID,
                            tc.NOTE AS GHICHU,
                            tc.NAME AS SOPHIEU,
                            tc.NGAY,
                            tc.TENDOITUONG,
                            tc.DIACHI,
                            nv.NAME AS NHANVIEN,
                            kh.NAME AS KHACHHANG,
                            tc.LOAIDOITUONG,
                            lydo.NAME AS LYDOTHUCHI,
                            tc.DIENGIAI,
                            tc.CHUNGTUGOC,
                            tc.THU AS SOTIENTHU,
                            tc.CHI AS SOTIENCHI,
                            ncc.NAME AS NHACUNGCAP,
                            tc.CHUYENKHOAN,
                            dh.NAME AS DATHANG,
                            ch.NAME AS CUAHANG,
                            tc.LAPHIEUTHUCONGNO,
                            tc.KHONGTHAYDOICONGNO,
                            tk.NAME AS TAIKHOANNGANHANG,
                            COALESCE(t.NAME, @MaThe) AS THETRATRUOC,
                            donhang.NAME AS DONHANG
                        FROM TTHUCHI tc
                        LEFT JOIN DNHANVIEN nv ON CAST(tc.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(tc.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN DLYDOTHUCHI lydo ON CAST(tc.DLYDOTHUCHIID AS VARCHAR(50)) = CAST(lydo.ID AS VARCHAR(50))
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(tc.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN TDATHANG dh ON CAST(tc.TDATHANGID AS VARCHAR(50)) = CAST(dh.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(tc.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(tc.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                        LEFT JOIN DTHETRATRUOC t ON CAST(tc.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                        LEFT JOIN TDONHANG donhang ON CAST(tc.TDONHANGID AS VARCHAR(50)) = CAST(donhang.ID AS VARCHAR(50))
                        WHERE (tc.STATUS IS NULL OR tc.STATUS > 0)
                          AND tc.LAPHIEUTHUCONGNO > 0
                          AND (CAST(tc.DTHETRATRUOCID AS VARCHAR(50)) = @TheId OR t.NAME = @MaThe)
                        ORDER BY tc.NGAY DESC, tc.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, new 
                    { 
                        TheId = theId ?? "", 
                        MaThe = maThe?.Trim() ?? "" 
                    })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        decimal ParseDec(object val)
                        {
                            if (val == null) return 0;
                            if (decimal.TryParse(val.ToString(), out decimal d)) return d;
                            return 0;
                        }

                        bool ParseBool(object val)
                        {
                            if (val == null) return false;
                            if (val is bool b) return b;
                            if (int.TryParse(val.ToString(), out int i)) return i > 0;
                            return false;
                        }

                        string cuahangName = r.CUAHANG?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cuahangName))
                        {
                            cuahangName = "🖥️ " + cuahangName;
                        }

                        list.Add(new TheTraTruocThuCongNoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            GhiChu = r.GHICHU?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            TenDoiTuong = r.TENDOITUONG?.ToString() ?? "",
                            DiaChi = r.DIACHI?.ToString() ?? "",
                            NhanVien = r.NHANVIEN?.ToString() ?? "",
                            KhachHang = r.KHACHHANG?.ToString() ?? "",
                            LoaiDoiTuong = r.LOAIDOITUONG?.ToString() ?? "",
                            LyDoThuChi = r.LYDOTHUCHI?.ToString() ?? "",
                            DienGiai = r.DIENGIAI?.ToString() ?? "",
                            ChungTuGoc = r.CHUNGTUGOC?.ToString() ?? "",
                            SoTienThu = ParseDec(r.SOTIENTHU),
                            SoTienChi = ParseDec(r.SOTIENCHI),
                            NhaCungCap = r.NHACUNGCAP?.ToString() ?? "",
                            ChuyenKhoan = ParseBool(r.CHUYENKHOAN),
                            DatHang = r.DATHANG?.ToString() ?? "",
                            CuaHang = cuahangName,
                            LaPhieuThuCongNo = ParseBool(r.LAPHIEUTHUCONGNO),
                            KhongThayDoiCongNo = ParseBool(r.KHONGTHAYDOICONGNO),
                            TaiKhoanNganHang = r.TAIKHOANNGANHANG?.ToString() ?? "",
                            TheTraTruoc = r.THETRATRUOC?.ToString() ?? "",
                            DonHang = r.DONHANG?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLichSuThuCongNoTheTraTruocAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<bool> SaveNhomTheTraTruocAsync(string id, string name, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    DateTime now = DateTime.Now;
                    string userId = SessionContext.CurrentUser?.Id ?? "1";
                    string pId = (!string.IsNullOrEmpty(parentId) && parentId != "ALL" && parentId != "UNSET" && parentId != "TRASH") ? parentId : null;

                    if (string.IsNullOrEmpty(id))
                    {
                        string newId = Guid.NewGuid().ToString();
                        string sql = @"
                            INSERT INTO DNHOMTHETRATRUOC (
                                ID, NAME, STATUS, PARENTID, TIMECREATED, TIMEMODIFIED, USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, 1, @ParentId, @Now, @Now, @UserId, @UserId
                            )";

                        await conn.ExecuteAsync(sql, new { Id = newId, Name = name, ParentId = pId, Now = now, UserId = userId });
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DNHOMTHETRATRUOC SET 
                                NAME = @Name,
                                PARENTID = @ParentId,
                                TIMEMODIFIED = @Now,
                                USERMODIFIEDID = @UserId
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(sql, new { Id = id, Name = name, ParentId = pId, Now = now, UserId = userId });
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveNhomTheTraTruocAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteNhomTheTraTruocAsync(string id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "UPDATE DNHOMTHETRATRUOC SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id";
                    int affected = await conn.ExecuteAsync(sql, new { Id = id });
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteNhomTheTraTruocAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SaveNhomTheTraTruocFolderAsync(string id, string name, bool isNew, string parentId = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string userId = SessionContext.CurrentUser?.Id ?? "1";
                    string pId = (!string.IsNullOrEmpty(parentId) && parentId != "ALL" && parentId != "UNSET" && parentId != "TRASH") ? parentId : null;

                    if (isNew)
                    {
                        string sql = @"
                            INSERT INTO DNHOMTHETRATRUOC (
                                ID, NAME, STATUS, PARENTID, TIMECREATED, TIMEMODIFIED,
                                USERCREATEDID, USERMODIFIEDID
                            ) VALUES (
                                @Id, @Name, 1, @ParentId, @Now, @Now,
                                @UserId, @UserId
                            )";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, ParentId = pId, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                    else
                    {
                        string sql = "UPDATE DNHOMTHETRATRUOC SET NAME = @Name, TIMEMODIFIED = @Now, USERMODIFIEDID = @UserId WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        int affected = await conn.ExecuteAsync(sql, new { Id = id, Name = name, Now = DateTime.Now, UserId = userId });
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveNhomTheTraTruocFolderAsync: " + ex.Message);
                return false;
            }
        }
    }
}
