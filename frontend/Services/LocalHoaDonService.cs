using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalHoaDonService
    {
        public async Task<List<HoaDonViewModel>> GetHoaDonListAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                
                string sql = @"
                    SELECT 
                        CAST(h.ID AS VARCHAR(50)) as Id, 
                        COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as SoPhieu, 
                        h.NGAY as Ngay, 
                        b.NAME as Ban, 
                        h.BATDAU as BatDau, 
                        h.KETTHUC as KetThuc,
                        h.GIOTHANHTOAN as GioThanhToan, 
                        CAST(COALESCE(h.TONGCONG, '0') AS DECIMAL(18,0)) as TongCong, 
                        k.NAME as KhachHang, 
                        h.TIENGIAMGIA as TienGiamGia, 
                        h.TILEGIAMGIA as TiLeGiamGia, 
                        h.TIENHANG as TienHang, 
                        CAST(COALESCE(h.KHACHDUA, '0') AS DECIMAL(18,0)) as KhachDua, 
                        CAST(COALESCE(h.TRALAI, '0') AS DECIMAL(18,0)) as TraLai, 
                        CAST(COALESCE(h.THE, '0') AS DECIMAL(18,0)) as TheThanhToan, 
                        h.TIENMAT as TienMat, 
                        CAST(COALESCE(h.SOKHACH, '0') AS INTEGER) as SoKhach, 
                        CAST(COALESCE(h.TILEGIAMGIAGIO, '0') AS DECIMAL(18,2)) as TiLeGiamGiaGio,
                        h.SOORDER as SoOrder, 
                        h.TIENGIAMGIAGIO as TienGiamGiaGio,
                        h.NOTE as GhiChu
                    FROM TDONHANG h
                    LEFT JOIN DBAN b ON h.DBANID = b.ID
                    LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                    WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                      AND CAST(h.NGAY AS DATE) <= @DenNgay
                    ORDER BY h.NGAY DESC, h.TIMECREATED DESC
                ";

                var parameters = new 
                { 
                    TuNgay = tuNgay.Date, 
                    DenNgay = denNgay.Date 
                };

                var list = (await conn.QueryAsync<HoaDonViewModel>(sql, parameters)).ToList();
                return list;
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
                    return items;
                }
            }
            catch
            {
                return new List<KhachHangLookupViewModel>();
            }
        }

        public async Task<bool> InsertKhachHangAsync(string name, string maKhach, string diaChi, string dienThoai)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        INSERT INTO DKHACHHANG (NAME, MAKHACH, DIACHI, DIENTHOAI, STATUS, TIMECREATED)
                        VALUES (@Name, @Makhach, @Diachi, @Dienthoai, 1, CURRENT_TIMESTAMP)";
                    await conn.ExecuteAsync(sql, new
                    {
                        Name = name,
                        Makhach = maKhach,
                        Diachi = diaChi,
                        Dienthoai = dienThoai
                    });
                    return true;
                }
            }
            catch { return false; }
        }

        public async Task<bool> UpdateHoaDonKhachHangAsync(string donHangId, string khachHangId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE TDONHANG SET DKHACHHANGID = @KhachHangId WHERE CAST(ID AS VARCHAR(50)) = @DonHangId";
                    await conn.ExecuteAsync(sql, new { KhachHangId = khachHangId, DonHangId = donHangId });
                    return true;
                }
            }
            catch { return false; }
        }

        public Task<List<ChiTietHoaDonViewModel>> GetChiTietHoaDonAsync(int donHangId) => GetChiTietHoaDonAsync(donHangId.ToString());

        public async Task<List<ChiTietHoaDonViewModel>> GetChiTietHoaDonAsync(string donHangId)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT 
                        CAST(c.ID AS VARCHAR(50)) as Id,
                        CAST(c.DMATHANGID AS VARCHAR(50)) as MatHangId,
                        COALESCE(c.TENHANG, m.NAME) as TenMon,
                        COALESCE(dvt.NAME, 'đĩa') as Dvt,
                        CAST(COALESCE(c.SLXUAT, c.SLNHAP, 1) AS DECIMAL(18,2)) as SoLuong,
                        CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                        CAST(COALESCE(c.TILEGIAMGIA, 0) AS DECIMAL(18,2)) as PhanTramGiamGia,
                        CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) as ThanhTien,
                        c.NOTE as GhiChu
                    FROM TDONHANGCHITIET c
                    LEFT JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                    WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @DonHangId
                    ORDER BY c.ID
                ";
                
                var parameters = new { DonHangId = donHangId };
                var list = (await conn.QueryAsync<ChiTietHoaDonViewModel>(sql, parameters)).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Stt = i + 1;
                }
                return list;
            }
        }

        public async Task<bool> TraDoHoaDonAsync(string donHangId, List<KiemDoItemViewModel> items)
        {
            int userCreatedId = 1;
            if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUserId))
            {
                userCreatedId = parsedUserId;
            }

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in items)
                        {
                            if (item.SlTra > 0)
                            {
                                decimal slTraNegative = -item.SlTra;
                                decimal thanhTienNegative = -(item.SlTra * item.DonGia * (1 - (item.ChietKhauPt / 100m)));

                                string matHangId = item.MatHangId;
                                if (string.IsNullOrEmpty(matHangId))
                                {
                                    matHangId = await conn.QueryFirstOrDefaultAsync<string>(
                                        "SELECT CAST(DMATHANGID AS VARCHAR(50)) FROM TDONHANGCHITIET WHERE CAST(ID AS VARCHAR(50)) = @Id", 
                                        new { Id = item.Id }, trans);
                                }

                                string sqlInsertNegative = @"
                                    INSERT INTO TDONHANGCHITIET (
                                        ID, TDONHANGID, DMATHANGID, TENHANG, SLXUAT, DONGIA, TILEGIAMGIA, THANHTIEN, TIMECREATED, STATUS, USERCREATEDID, NOTE
                                    ) VALUES (
                                        @Id, @DonHangId, @MatHangId, @TenHang, @SlXuat, @DonGia, @TiLeGiamGia, @ThanhTien, @TimeCreated, 1, @UserCreatedId, @Note
                                    )";

                                await conn.ExecuteAsync(sqlInsertNegative, new
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    DonHangId = donHangId,
                                    MatHangId = matHangId,
                                    TenHang = item.MatHang,
                                    SlXuat = slTraNegative,
                                    DonGia = item.DonGia,
                                    TiLeGiamGia = item.ChietKhauPt,
                                    ThanhTien = thanhTienNegative,
                                    TimeCreated = DateTime.Now,
                                    UserCreatedId = userCreatedId,
                                    Note = ""
                                }, trans);
                            }
                        }

                        // Tính lại tổng tiền hàng của đơn hàng
                        string sqlSum = @"
                            SELECT CAST(COALESCE(SUM(THANHTIEN), 0) AS DECIMAL(18,0)) 
                            FROM TDONHANGCHITIET 
                            WHERE CAST(TDONHANGID AS VARCHAR(50)) = @DonHangId";
                        decimal newTienHang = await conn.ExecuteScalarAsync<decimal>(sqlSum, new { DonHangId = donHangId }, trans);

                        // Lấy % giảm giá và giảm theo tiền của đơn hàng
                        string sqlOrderInfo = @"
                            SELECT 
                                CAST(COALESCE(TILEGIAMGIA, 0) AS DECIMAL(18,2)) as TiLeGiamGia,
                                CAST(COALESCE(TIENGIAMGIA, 0) AS DECIMAL(18,0)) as TienGiamGia
                            FROM TDONHANG 
                            WHERE CAST(ID AS VARCHAR(50)) = @DonHangId";
                        var orderInfo = await conn.QueryFirstOrDefaultAsync(sqlOrderInfo, new { DonHangId = donHangId }, trans);

                        decimal tiLeGiam = orderInfo?.TILEGIAMGIA ?? 0;
                        decimal tienGiam = orderInfo?.TIENGIAMGIA ?? 0;
                        decimal newTongCong = Math.Max(0, newTienHang - (newTienHang * tiLeGiam / 100m) - tienGiam);

                        // Cập nhật lại TDONHANG
                        string sqlUpdateOrder = @"
                            UPDATE TDONHANG 
                            SET TIENHANG = @TienHang, 
                                TONGCONG = @TongCong,
                                KHACHDUA = @TongCong
                            WHERE CAST(ID AS VARCHAR(50)) = @DonHangId";
                        await conn.ExecuteAsync(sqlUpdateOrder, new 
                        { 
                            TienHang = newTienHang, 
                            TongCong = newTongCong, 
                            DonHangId = donHangId 
                        }, trans);

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<List<KqkdRowViewModel>> GetTongHopKqkdAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                
                string sqlDoanhThu = @"
                    SELECT 
                        CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) as TongTien
                    FROM TDONHANG h
                    JOIN TDONHANGCHITIET c ON h.ID = c.TDONHANGID
                    WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                      AND CAST(h.NGAY AS DATE) <= @DenNgay
                ";
                
                var parameters = new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date };
                decimal tongDoanhThu = await conn.ExecuteScalarAsync<decimal>(sqlDoanhThu, parameters);
                
                var list = new List<KqkdRowViewModel>
                {
                    new KqkdRowViewModel { Stt = "I.", ChiTieu = "DOANH THU", GiaTri = tongDoanhThu.ToString("N0"), PhanTram = "100%", TangGiam = tongDoanhThu.ToString("N0"), KqThangTruoc = "0" },
                    new KqkdRowViewModel { Stt = "1", ChiTieu = "DOANH THU BÁN HÀNG", GiaTri = tongDoanhThu.ToString("N0"), PhanTram = "100%", TangGiam = tongDoanhThu.ToString("N0"), KqThangTruoc = "0" },
                    new KqkdRowViewModel { Stt = "II.", ChiTieu = "CHI PHÍ", GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" },
                    new KqkdRowViewModel { Stt = "III.", ChiTieu = "LÃI/LỖ", GiaTri = tongDoanhThu.ToString("N0"), PhanTram = "100%", TangGiam = tongDoanhThu.ToString("N0"), KqThangTruoc = "0" }
                };
                
                return list;
            }
        }

        public async Task<List<HoaDonHuyViewModel>> GetHoaDonHuyListAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                
                string sql = @"
                    SELECT 
                        h.ID as Id, 
                        CAST(h.NGAY AS DATE) as Ngay, 
                        h.NAME as SoPhieu, 
                        h.NOTE as GhiChu, 
                        h.KHACHHANG as KhachHang, 
                        h.NHANVEN as NhanVien, 
                        'Administrator' as ThuNganHuy, 
                        h.GIOHUY as GioHuy, 
                        CAST(h.NGAYHUY AS DATE) as NgayHuy, 
                        CAST(COALESCE(h.DOITRA, '0') AS DECIMAL(18,0)) as Doitra, 
                        CASE WHEN h.DATHANHTOAN = 'True' THEN 1 ELSE 0 END as DaThanhToan, 
                        h.GIOTHANHTOAN as GioThanhToan, 
                        CAST(COALESCE(h.TRALAI, '0') AS DECIMAL(18,0)) as TraLai, 
                        h.TIENHANG as TienHang, 
                        h.TILETHUE as TiLeThue, 
                        h.TIENTHUE as TienThue, 
                        h.TILEGIAMGIA as TiLeGiamGia, 
                        h.TIENGIAMGIA as TienGiamGia, 
                        h.THANHTOANBOI as ThanhToanBoi, 
                        CAST(COALESCE(h.PHIVANCHUYEN, '0') AS DECIMAL(18,0)) as PhiVanChuyen, 
                        h.LYDOHUY as LyDoHuy, 
                        h.TIENGIO as TienGio, 
                        CAST(COALESCE(h.PHIDICHVU, '0') AS DECIMAL(18,0)) as PhiDichVu
                    FROM TDONHANGHUY h
                    WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                      AND CAST(h.NGAY AS DATE) <= @DenNgay
                    ORDER BY h.NGAY DESC, h.GIOHUY DESC
                ";

                var parameters = new 
                { 
                    TuNgay = tuNgay.Date, 
                    DenNgay = denNgay.Date 
                };

                var list = (await conn.QueryAsync<HoaDonHuyViewModel>(sql, parameters)).ToList();
                return list;
            }
        }

        public async Task<List<ChiTietHoaDonHuyViewModel>> GetChiTietHoaDonHuyAsync(int donHangHuyId)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT 
                        c.ID as Id,
                        c.MAHANG as MaHang,
                        c.TENHANG as TenHang,
                        c.DVT as Dvt,
                        c.SOLUONG as SoLuong,
                        c.DONGIA as DonGia,
                        c.THANHTIEN as ThanhTien,
                        c.NOTE as GhiChu
                    FROM TDONHANGHUYCHITIET c
                    WHERE c.TDONHANGHUYID = @DonHangHuyId
                ";
                
                var parameters = new { DonHangHuyId = donHangHuyId };
                var list = (await conn.QueryAsync<ChiTietHoaDonHuyViewModel>(sql, parameters)).ToList();
                return list;
            }
        }
    }
}
