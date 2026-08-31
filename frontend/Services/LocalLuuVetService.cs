using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalLuuVetService
    {
        public static async Task GhiLuuVetAsync(
            string soDonHangId, 
            string ban, 
            string chucNang, 
            string note, 
            int phanLoai = 0, 
            decimal soLuong = 0, 
            decimal donGia = 0, 
            decimal thanhTien = 0, 
            string tenHang = "")
        {
            try
            {
                if (string.IsNullOrEmpty(soDonHangId)) return;

                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    if (string.IsNullOrEmpty(ban))
                    {
                        ban = await conn.QueryFirstOrDefaultAsync<string>(
                            "SELECT b.NAME FROM TDONHANG h LEFT JOIN DBAN b ON h.DBANID = b.ID WHERE CAST(h.ID AS VARCHAR(50)) = @Id",
                            new { Id = soDonHangId });
                    }

                    string currentUserId = SessionContext.CurrentUser?.Id?.ToString() ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";
                    string currentUserName = SessionContext.CurrentUser?.TenDangNhap ?? "Administrator";
                    string machineName = Environment.MachineName;

                    string sql = @"
                        INSERT INTO TLUUVET (
                            ID, GIO, NOTE, STATUS, TIMECREATED, NGAY, USERCREATEDID, 
                            SODONHANG, TAIKHOAN, THIETBI, PHANLOAI, BAN, CHUCNANG, 
                            SOLUONG, DONGIA, THANHTIEN, TENHANG
                        ) VALUES (
                            @Id, @Gio, @Note, @Status, @TimeCreated, @Ngay, @UserCreatedId,
                            @SoDonHang, @TaiKhoan, @ThietBi, @PhanLoai, @Ban, @ChucNang,
                            @SoLuong, @DonGia, @ThanhTien, @TenHang
                        )";

                    var parameters = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Gio = DateTime.Now,
                        Note = note ?? "",
                        Status = 30,
                        TimeCreated = DateTime.Now,
                        Ngay = DateTime.Today,
                        UserCreatedId = currentUserId,
                        SoDonHang = soDonHangId,
                        TaiKhoan = currentUserName,
                        ThietBi = machineName,
                        PhanLoai = phanLoai,
                        Ban = ban ?? "",
                        ChucNang = chucNang ?? "Sử dụng dịch vụ",
                        SoLuong = soLuong,
                        DonGia = donGia,
                        ThanhTien = thanhTien,
                        TenHang = tenHang ?? ""
                    };

                    await conn.ExecuteAsync(sql, parameters);
                }
            }
            catch { }
        }

        public async Task<List<LuuVetHoaDonItemViewModel>> GetHoaDonListForLuuVetAsync(DateTime tuNgay, DateTime denNgay, string soHd = null)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(h.ID AS VARCHAR(50)) as Id,
                        h.NAME as SoPhieu, 
                        b.NAME as Ban, 
                        h.STATUS as Status, 
                        h.DATHANHTOAN as DaThanhToan,
                        h.NOTE as Note,
                        h.NGAY as Ngay,
                        h.TIMECREATED as TimeCreated
                    FROM TDONHANG h
                    LEFT JOIN DBAN b ON h.DBANID = b.ID
                    WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                      AND CAST(h.NGAY AS DATE) <= @DenNgay";

                var parameters = new DynamicParameters();
                parameters.Add("TuNgay", tuNgay.Date);
                parameters.Add("DenNgay", denNgay.Date);

                if (!string.IsNullOrWhiteSpace(soHd))
                {
                    sql += " AND UPPER(h.NAME) LIKE @SoHdLike";
                    parameters.Add("SoHdLike", $"%{soHd.Trim().ToUpper()}%");
                }

                sql += " ORDER BY h.NAME ASC";

                var rows = (await conn.QueryAsync(sql, parameters)).ToList();
                var result = new List<LuuVetHoaDonItemViewModel>();
                int stt = 1;

                foreach (var r in rows)
                {
                    string id = r.ID?.ToString();
                    string soPhieu = r.SOPHIEU?.ToString();
                    string ban = r.BAN?.ToString();
                    int? status = r.STATUS != null ? Convert.ToInt32(r.STATUS) : (int?)null;
                    int? daThanhToan = r.DATHANHTOAN != null ? Convert.ToInt32(r.DATHANHTOAN) : (int?)null;
                    string note = r.NOTE?.ToString();

                    string trangThai = "Kết thúc";
                    string color = "#000000";

                    if (!string.IsNullOrEmpty(note) && note.Contains("Gộp"))
                    {
                        trangThai = note;
                        color = "#555555";
                    }
                    else if (!string.IsNullOrEmpty(note) && (note.Contains("Hủy") || note.Contains("hủy") || status == 0))
                    {
                        trangThai = note.Trim('[', ']', ' ');
                        if (trangThai.StartsWith("Hủy: ")) trangThai = trangThai.Substring(5);
                        color = "#d9363e";
                    }
                    else if (status == 1 || status == 2)
                    {
                        trangThai = "Đang sử dụng";
                        color = "#0066cc";
                    }
                    else if (status == 30 || daThanhToan == 30)
                    {
                        trangThai = "Kết thúc";
                        color = "#000000";
                    }
                    else if (!string.IsNullOrEmpty(note))
                    {
                        trangThai = note;
                        color = "#d9363e";
                    }

                    result.Add(new LuuVetHoaDonItemViewModel
                    {
                        Stt = (stt++).ToString("D2"),
                        Id = id,
                        SoPhieu = soPhieu,
                        Ban = ban,
                        TrangThai = trangThai,
                        TextColor = color,
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null
                    });
                }

                return result;
            }
        }

        public async Task<List<LuuVetViewModel>> GetLuuVetChiTietAsync(DateTime tuNgay, DateTime denNgay, string donHangId = null, string soDonHang = null, string taiKhoan = null, string locText = null)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(l.ID AS VARCHAR(50)) as Id,
                        l.NGAY as Ngay, 
                        l.GIO as Gio, 
                        COALESCE(h.NAME, l.SODONHANG) as Sodonhang, 
                        l.NOTE as Note, 
                        l.TAIKHOAN as Taikhoan, 
                        l.THIETBI as Thietbi, 
                        l.BAN as Ban, 
                        l.CHUCNANG as Chucnang
                    FROM TLUUVET l
                    LEFT JOIN TDONHANG h ON l.SODONHANG = h.ID";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(donHangId))
                {
                    sql += " WHERE l.SODONHANG = @DonHangId";
                    parameters.Add("DonHangId", donHangId.Trim());
                }

                sql += " ORDER BY l.GIO ASC, l.TIMECREATED ASC";

                var rows = (await conn.QueryAsync(sql, parameters)).ToList();
                var result = new List<LuuVetViewModel>();
                int stt = 1;

                foreach (var r in rows)
                {
                    string tk = r.TAIKHOAN?.ToString();
                    string note = r.NOTE?.ToString();
                    string tb = r.THIETBI?.ToString();
                    string ban = r.BAN?.ToString();
                    string cn = r.CHUCNANG?.ToString();
                    string soDh = r.SODONHANG?.ToString();

                    if (!string.IsNullOrEmpty(taiKhoan) && taiKhoan != "[Tất cả]" && !string.Equals(tk, taiKhoan, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(locText))
                    {
                        string s = $"{note} {tb} {ban} {cn} {soDh}".ToLower();
                        if (!s.Contains(locText.ToLower())) continue;
                    }

                    result.Add(new LuuVetViewModel
                    {
                        Stt = (stt++).ToString("D2"),
                        Id = r.ID?.ToString(),
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        Gio = r.GIO != null ? Convert.ToDateTime(r.GIO) : (DateTime?)null,
                        Sodonhang = soDh,
                        Note = note,
                        Taikhoan = tk,
                        Thietbi = tb,
                        Ban = ban,
                        Chucnang = cn
                    });
                }

                return result;
            }
        }

        public async Task<List<string>> GetDanhSachTaiKhoanAsync()
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                var list = (await conn.QueryAsync<string>("SELECT DISTINCT TAIKHOAN FROM TLUUVET WHERE TAIKHOAN IS NOT NULL ORDER BY TAIKHOAN ASC")).ToList();
                list.Insert(0, "[Tất cả]");
                return list;
            }
        }
    }
}
