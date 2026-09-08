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
                        CAST(k.DIACHI AS VARCHAR(255)) as DiaChi,
                        k.MAKHACH as MaKhach,
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
                        h.NOTE as GhiChu,
                        COALESCE(u.NAME, u.USERNAME, 'Administrator') as ThanhToanBoi,
                        h.DIENGIAI as DienGiai
                    FROM TDONHANG h
                    LEFT JOIN DBAN b ON h.DBANID = b.ID
                    LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                    LEFT JOIN SUSER u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
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

        public async Task<HoaDonViewModel> GetHoaDonByIdOrSoPhieuAsync(string idOrSoPhieu)
        {
            if (string.IsNullOrWhiteSpace(idOrSoPhieu)) return null;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT FIRST 1
                            CAST(h.ID AS VARCHAR(50)) as Id, 
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as SoPhieu, 
                            h.NGAY as Ngay, 
                            b.NAME as Ban, 
                            h.BATDAU as BatDau, 
                            h.KETTHUC as KetThuc,
                            h.GIOTHANHTOAN as GioThanhToan, 
                            CAST(COALESCE(h.TONGCONG, '0') AS DECIMAL(18,0)) as TongCong, 
                            k.NAME as KhachHang, 
                            CAST(k.DIACHI AS VARCHAR(255)) as DiaChi,
                            k.MAKHACH as MaKhach,
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
                            h.NOTE as GhiChu,
                            COALESCE(u.NAME, u.USERNAME, 'Administrator') as ThanhToanBoi,
                            h.DIENGIAI as DienGiai
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON h.DBANID = b.ID
                        LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                        LEFT JOIN SUSER u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE CAST(h.ID AS VARCHAR(50)) = @Query 
                           OR UPPER(h.NAME) = UPPER(@Query)
                           OR UPPER(CAST(h.SOHD AS VARCHAR(50))) = UPPER(@Query)
                    ";

                    return await conn.QueryFirstOrDefaultAsync<HoaDonViewModel>(sql, new { Query = idOrSoPhieu.Trim() });
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<HoaDonHuyViewModel>> GetHoaDonHuyListAsync(DateTime tuNgay, DateTime denNgay, string keyword = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            h.ID as Id, 
                            CAST(h.NGAY AS DATE) as Ngay, 
                            h.NAME as SoPhieu, 
                            COALESCE(h.NOTE, '') as GhiChu, 
                            COALESCE(h.KHACHHANG, '') as KhachHang, 
                            COALESCE(h.NHANVEN, '') as NhanVien, 
                            COALESCE(h.THUNGAN, 'Administrator') as ThuNganHuy, 
                            h.GIOHUY as GioHuy, 
                            CAST(h.NGAYHUY AS DATE) as NgayHuy, 
                            COALESCE(h.DOITRA, 0) as Doitra, 
                            CASE WHEN h.DATHANHTOAN = 1 THEN 1 ELSE 0 END as DaThanhToan, 
                            h.GIOTHANHTOAN as GioThanhToan, 
                            COALESCE(h.TRALAI, 0) as TraLai, 
                            COALESCE(h.TIENHANG, 0) as TienHang, 
                            COALESCE(h.TILETHUE, 0) as TiLeThue, 
                            COALESCE(h.TIENTHUE, 0) as TienThue, 
                            COALESCE(h.TILEGIAMGIA, 0) as TiLeGiamGia, 
                            COALESCE(h.TIENGIAMGIA, 0) as TienGiamGia, 
                            COALESCE(h.THANHTOANBOI, '') as ThanhToanBoi, 
                            COALESCE(h.PHIVANCHUYEN, 0) as PhiVanChuyen, 
                            COALESCE(h.LYDOHUY, '') as LyDoHuy, 
                            COALESCE(h.TIENGIO, 0) as TienGio, 
                            COALESCE(h.PHIDICHVU, 0) as PhiDichVu
                        FROM TDONHANGHUY h
                        WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                    ";

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " AND (UPPER(h.NAME) LIKE UPPER(@Kw) OR UPPER(h.LYDOHUY) LIKE UPPER(@Kw) OR UPPER(h.KHACHHANG) LIKE UPPER(@Kw)) ";
                    }

                    sql += " ORDER BY h.NGAY DESC, h.GIOHUY DESC";

                    var parameters = new 
                    { 
                        TuNgay = tuNgay.Date, 
                        DenNgay = denNgay.Date,
                        Kw = $"%{keyword?.Trim()}%"
                    };

                    var list = (await conn.QueryAsync<HoaDonHuyViewModel>(sql, parameters)).ToList();
                    return list;
                }
            }
            catch (Exception)
            {
                return new List<HoaDonHuyViewModel>();
            }
        }

        public async Task<List<ChiTietHoaDonHuyViewModel>> GetChiTietHoaDonHuyAsync(string donHangHuyId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            c.ID as Id, 
                            COALESCE(c.MAHANG, '') as MaHang, 
                            COALESCE(c.TENHANG, '') as TenHang, 
                            COALESCE(c.DVT, 'đĩa') as Dvt, 
                            COALESCE(c.SOLUONG, 1) as SoLuong, 
                            COALESCE(c.DONGIA, 0) as DonGia, 
                            COALESCE(c.THANHTIEN, 0) as ThanhTien, 
                            COALESCE(c.NOTE, '') as GhiChu 
                        FROM TDONHANGHUYCHITIET c
                        WHERE CAST(c.TDONHANGHUYID AS VARCHAR(50)) = @HuyId
                        ORDER BY c.TENHANG
                    ";

                    var list = (await conn.QueryAsync<ChiTietHoaDonHuyViewModel>(sql, new { HuyId = donHangHuyId })).ToList();
                    return list;
                }
            }
            catch (Exception)
            {
                return new List<ChiTietHoaDonHuyViewModel>();
            }
        }

        public async Task<List<CuaHangViewModel>> GetCuaHangListAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT CAST(ID AS VARCHAR(50)) as Id,
                               NAME as Name,
                               CODE as Code,
                               NOTE as Note
                        FROM DCUAHANG
                        WHERE STATUS <> 0 OR STATUS IS NULL
                        ORDER BY ID";
                    var items = (await conn.QueryAsync<CuaHangViewModel>(sql)).ToList();
                    if (items.Count == 0)
                    {
                        items.Add(new CuaHangViewModel { Id = "1", Name = "TRỤ SỞ CHÍNH", Code = "TS" });
                    }
                    return items;
                }
            }
            catch
            {
                return new List<CuaHangViewModel> { new CuaHangViewModel { Id = "1", Name = "TRỤ SỞ CHÍNH", Code = "TS" } };
            }
        }

        public async Task<bool> InsertCuaHangAsync(string name)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "INSERT INTO DCUAHANG (NAME, STATUS, TIMECREATED) VALUES (@Name, 1, CURRENT_TIMESTAMP)";
                    await conn.ExecuteAsync(sql, new { Name = name });
                    return true;
                }
            }
            catch { return false; }
        }

        public async Task<List<ThongKeMatHangBanItemViewModel>> GetThongKeMatHangBanAsync(DateTime tuNgay, DateTime denNgay, string nhomId = null, bool isTheoGiaVon = true)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string giaField = isTheoGiaVon ? "COALESCE(m.GIAVON, 0)" : "COALESCE(m.GIANHAP, 0)";
                    string sql = $@"
                        SELECT 
                            COALESCE(m.CODE, '') as MaHang,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, 'đĩa') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(SUM(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,0)) as TienGiam,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTienBan,
                            CAST({giaField} AS DECIMAL(18,0)) as GiaVon,
                            CAST(0 AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) * {giaField} AS DECIMAL(18,0)) as ThanhTienNhap,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) - (SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) * {giaField}) AS DECIMAL(18,0)) as Lai,
                            CAST(0 AS DECIMAL(18,2)) as TiLeLai,
                            CAST(m.DNHOMMATHANGID AS VARCHAR(50)) as NhomId
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                    ";

                    if (!string.IsNullOrEmpty(nhomId) && nhomId != "0" && nhomId != "ALL")
                    {
                        sql += " AND (CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomId OR CAST(n.PARENTID AS VARCHAR(50)) = @NhomId OR n.PARENTDIR LIKE '%' || @NhomId || ',%') ";
                    }

                    sql += $" GROUP BY m.CODE, COALESCE(c.TENHANG, m.NAME), dvt.NAME, {giaField}, m.DNHOMMATHANGID ORDER BY TenHang";

                    var parameters = new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        NhomId = nhomId
                    };

                    var items = (await conn.QueryAsync<ThongKeMatHangBanItemViewModel>(sql, parameters)).ToList();
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].Stt = i + 1;
                        if (items[i].ThanhTienBan > 0)
                        {
                            items[i].TiLeLai = Math.Round((items[i].Lai / items[i].ThanhTienBan) * 100, 0);
                        }
                        else if (items[i].Lai > 0)
                        {
                            items[i].TiLeLai = 100;
                        }
                        else
                        {
                            items[i].TiLeLai = 0;
                        }
                    }
                    return items;
                }
            }
            catch (Exception)
            {
                return new List<ThongKeMatHangBanItemViewModel>();
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

        public async Task<bool> UpdateHoaDonKhachHangAsync(string donHangId, string khachHangId, string chucNang = "Điều chỉnh hóa đơn")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE TDONHANG SET DKHACHHANGID = @KhachHangId WHERE CAST(ID AS VARCHAR(50)) = @DonHangId";
                    await conn.ExecuteAsync(sql, new { KhachHangId = khachHangId, DonHangId = donHangId });

                    string khName = await conn.QueryFirstOrDefaultAsync<string>(
                        "SELECT NAME FROM DKHACHHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = khachHangId });
                    if (!string.IsNullOrEmpty(khName))
                    {
                        _ = LocalLuuVetService.GhiLuuVetAsync(donHangId, null, chucNang, $"Đặt khách hàng '{khName}'", 3);
                    }

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
                        COALESCE(m.CODE, '') as MaHang,
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
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sqlGroups = @"
                        SELECT 
                            COALESCE(n.NAME, 'MẶT HÀNG KHÁC') as NhomName,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as DoanhThu,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1) * COALESCE(m.GIAVON, 0)) AS DECIMAL(18,0)) as GiaVon
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                        GROUP BY COALESCE(n.NAME, 'MẶT HÀNG KHÁC')
                        HAVING SUM(COALESCE(c.THANHTIEN, 0)) > 0
                        ORDER BY DoanhThu DESC";

                    var parameters = new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date };
                    var groupRows = (await conn.QueryAsync(sqlGroups, parameters)).ToList();

                    decimal tongDoanhThu = 0;
                    decimal tongBienPhi = 0;

                    foreach (var g in groupRows)
                    {
                        tongDoanhThu += (decimal)g.DOANHTHU;
                        tongBienPhi += (decimal)g.GIAVON;
                    }

                    decimal tongDinhPhi = 0;
                    decimal tongChiKhac = 0;
                    decimal tongChiPhi = tongDinhPhi + tongBienPhi + tongChiKhac;
                    decimal laiLo = tongDoanhThu - tongChiPhi;

                    decimal ptCpTrenDt = tongDoanhThu > 0 ? Math.Round((tongChiPhi / tongDoanhThu) * 100, 0) : 0;
                    decimal ptLaiTrenDt = tongDoanhThu > 0 ? Math.Round((laiLo / tongDoanhThu) * 100, 0) : 0;
                    decimal ptLaiTrenCp = tongChiPhi > 0 ? Math.Round((laiLo / tongChiPhi) * 100, 0) : 0;

                    var list = new List<KqkdRowViewModel>();

                    // I. DOANH THU
                    list.Add(new KqkdRowViewModel
                    {
                        Stt = "I.",
                        ChiTieu = "DOANH THU",
                        PhanTramDt = "",
                        GiaTri = tongDoanhThu.ToString("N0"),
                        PhanTram = "100%",
                        PhanTramCp = "",
                        TangGiam = tongDoanhThu.ToString("N0"),
                        KqThangTruoc = "0",
                        IsBold = true,
                        IsHeader = true
                    });

                    int sttDt = 1;
                    foreach (var g in groupRows)
                    {
                        decimal dt = (decimal)g.DOANHTHU;
                        decimal pt = tongDoanhThu > 0 ? Math.Round((dt / tongDoanhThu) * 100, 0) : 0;
                        list.Add(new KqkdRowViewModel
                        {
                            Stt = (sttDt++).ToString(),
                            ChiTieu = (string)g.NHOMNAME,
                            PhanTramDt = "",
                            GiaTri = dt.ToString("N0"),
                            PhanTram = $"{pt}%",
                            PhanTramCp = "",
                            TangGiam = dt.ToString("N0"),
                            KqThangTruoc = "0"
                        });
                    }

                    list.Add(new KqkdRowViewModel { Stt = (sttDt++).ToString(), ChiTieu = "Tiền giờ", GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" });
                    list.Add(new KqkdRowViewModel { Stt = (sttDt++).ToString(), ChiTieu = "Giảm giá", GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" });
                    list.Add(new KqkdRowViewModel { Stt = (sttDt++).ToString(), ChiTieu = "Phí dịch vụ", GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" });
                    list.Add(new KqkdRowViewModel { Stt = (sttDt++).ToString(), ChiTieu = "Thuế", GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" });

                    // II. CHI PHÍ
                    list.Add(new KqkdRowViewModel
                    {
                        Stt = "II.",
                        ChiTieu = "CHI PHÍ",
                        PhanTramDt = ptCpTrenDt > 0 ? $"{ptCpTrenDt}%" : "",
                        GiaTri = tongChiPhi.ToString("N0"),
                        PhanTram = "",
                        PhanTramCp = "100%",
                        TangGiam = tongChiPhi.ToString("N0"),
                        KqThangTruoc = "0",
                        IsBold = true,
                        IsHeader = true
                    });

                    // A. Định phí
                    list.Add(new KqkdRowViewModel
                    {
                        Stt = "A.",
                        ChiTieu = "Định phí",
                        PhanTramDt = "",
                        GiaTri = "0",
                        PhanTram = "",
                        PhanTramCp = "100%",
                        TangGiam = "0",
                        KqThangTruoc = "0",
                        IsBold = true
                    });

                    string[] dinhPhiItems = { "Tiền nhà", "Tiền điện thoại", "Tiền nước", "Tiền điện", "Lương nhân viên", "Lương quản lý", "Thưởng nhân viên", "Chi lương nhân viên" };
                    for (int i = 0; i < dinhPhiItems.Length; i++)
                    {
                        list.Add(new KqkdRowViewModel { Stt = (i + 1).ToString(), ChiTieu = dinhPhiItems[i], GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" });
                    }

                    // B. Biến phí
                    list.Add(new KqkdRowViewModel
                    {
                        Stt = "B.",
                        ChiTieu = "Biến phí",
                        PhanTramDt = ptCpTrenDt > 0 ? $"{ptCpTrenDt}%" : "",
                        GiaTri = tongBienPhi.ToString("N0"),
                        PhanTram = "100%",
                        PhanTramCp = "100%",
                        TangGiam = tongBienPhi.ToString("N0"),
                        KqThangTruoc = "0",
                        IsBold = true
                    });

                    int sttBp = 1;
                    foreach (var g in groupRows)
                    {
                        decimal gv = (decimal)g.GIAVON;
                        if (gv > 0)
                        {
                            decimal ptDt = tongDoanhThu > 0 ? Math.Round((gv / tongDoanhThu) * 100, 0) : 0;
                            decimal ptCp = tongBienPhi > 0 ? Math.Round((gv / tongBienPhi) * 100, 0) : 0;
                            list.Add(new KqkdRowViewModel
                            {
                                Stt = (sttBp++).ToString(),
                                ChiTieu = (string)g.NHOMNAME,
                                PhanTramDt = ptDt > 0 ? $"{ptDt}%" : "",
                                GiaTri = gv.ToString("N0"),
                                PhanTram = $"{ptCp}%",
                                PhanTramCp = "",
                                TangGiam = gv.ToString("N0"),
                                KqThangTruoc = "0"
                            });
                        }
                    }

                    // C. Chi khác
                    list.Add(new KqkdRowViewModel
                    {
                        Stt = "C.",
                        ChiTieu = "Chi khác",
                        PhanTramDt = "",
                        GiaTri = "0",
                        PhanTram = "",
                        PhanTramCp = "100%",
                        TangGiam = "0",
                        KqThangTruoc = "0",
                        IsBold = true
                    });

                    string[] chiKhacItems = { "Văn phòng phẩm, in ấn", "Xây dựng, sửa chữa, thiết kế", "Đồ dùng, dụng cụ", "Vận chuyển", "Ngoại giao", "Chi khác", "Đặt trước" };
                    for (int i = 0; i < chiKhacItems.Length; i++)
                    {
                        list.Add(new KqkdRowViewModel { Stt = (i + 1).ToString(), ChiTieu = chiKhacItems[i], GiaTri = "0", PhanTram = "-", TangGiam = "0", KqThangTruoc = "0" });
                    }

                    // III. LÃI/LỖ
                    list.Add(new KqkdRowViewModel
                    {
                        Stt = "III.",
                        ChiTieu = "LÃI/LỖ",
                        PhanTramDt = "",
                        GiaTri = laiLo.ToString("N0"),
                        PhanTram = $"{ptLaiTrenDt}%",
                        PhanTramCp = $"{ptLaiTrenCp}%",
                        TangGiam = laiLo.ToString("N0"),
                        KqThangTruoc = "0",
                        IsBold = true,
                        IsHeader = true
                    });

                    return list;
                }
            }
            catch (Exception)
            {
                return new List<KqkdRowViewModel>();
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

        #region NGHIỆP VỤ THÊM / GIẢM / XÓA / ĐỔI CHI TIẾT HÓA ĐƠN

        public async Task<bool> AddMonToHoaDonAsync(string donHangId, PosMatHangViewModel matHang, decimal soLuong = 1, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(donHangId) || matHang == null || soLuong <= 0) return false;

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
                        // Kiểm tra xem món đã có trong hóa đơn chưa
                        string sqlCheck = @"
                            SELECT FIRST 1 CAST(ID AS VARCHAR(50)) as Id, 
                                          CAST(COALESCE(SLXUAT, SLNHAP, 0) AS DECIMAL(18,2)) as SlXuat,
                                          CAST(COALESCE(DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                                          CAST(COALESCE(TILEGIAMGIA, 0) AS DECIMAL(18,2)) as TiLeGiamGia
                            FROM TDONHANGCHITIET 
                            WHERE CAST(TDONHANGID AS VARCHAR(50)) = @DonHangId 
                              AND CAST(DMATHANGID AS VARCHAR(50)) = @MatHangId
                              AND COALESCE(SLXUAT, 0) > 0";

                        var existing = await conn.QueryFirstOrDefaultAsync(sqlCheck, new { DonHangId = donHangId, MatHangId = matHang.Id }, trans);
                        decimal donGia = matHang.GiaBan ?? 0;
                        if (existing != null)
                        {
                            decimal currentSl = existing.SLXUAT;
                            donGia = existing.DONGIA > 0 ? existing.DONGIA : (matHang.GiaBan ?? 0);
                            decimal tiLeGiam = existing.TILEGIAMGIA;
                            decimal newSl = currentSl + soLuong;
                            decimal newThanhTien = newSl * donGia * (1 - (tiLeGiam / 100m));

                            string sqlUpdate = @"
                                UPDATE TDONHANGCHITIET 
                                SET SLXUAT = @SlXuat, 
                                    THANHTIEN = @ThanhTien 
                                WHERE CAST(ID AS VARCHAR(50)) = @Id";
                            await conn.ExecuteAsync(sqlUpdate, new { SlXuat = newSl, ThanhTien = newThanhTien, Id = existing.ID }, trans);
                        }
                        else
                        {
                            decimal thanhTien = soLuong * donGia;
                            string newId = Guid.NewGuid().ToString();

                            string sqlInsert = @"
                                INSERT INTO TDONHANGCHITIET (
                                    ID, TDONHANGID, DMATHANGID, TENHANG, SLXUAT, DONGIA, TILEGIAMGIA, THANHTIEN, TIMECREATED, STATUS, USERCREATEDID, NOTE
                                ) VALUES (
                                    @Id, @DonHangId, @MatHangId, @TenHang, @SlXuat, @DonGia, 0, @ThanhTien, @TimeCreated, 1, @UserCreatedId, ''
                                )";
                            await conn.ExecuteAsync(sqlInsert, new
                            {
                                Id = newId,
                                DonHangId = donHangId,
                                MatHangId = matHang.Id,
                                TenHang = matHang.Name,
                                SlXuat = soLuong,
                                DonGia = donGia,
                                ThanhTien = thanhTien,
                                TimeCreated = DateTime.Now,
                                UserCreatedId = userCreatedId
                            }, trans);
                        }

                        await RecalculateOrderTotalsAsync(conn, trans, donHangId);
                        trans.Commit();

                        // Ghi lưu vết hoạt động
                        _ = LocalLuuVetService.GhiLuuVetAsync(
                            donHangId, null, chucNang, 
                            $"Thêm '{matHang.Name}' vào bill, số lượng: {soLuong:0.##}", 
                            4, soLuong, donGia, soLuong * donGia, matHang.Name);

                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> GiamSoLuongMonHoaDonAsync(string chiTietId, string donHangId, decimal soLuongGiam = 1, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(chiTietId) || string.IsNullOrEmpty(donHangId) || soLuongGiam <= 0) return false;

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = @"
                            SELECT TENHANG as TenHang,
                                   CAST(COALESCE(SLXUAT, SLNHAP, 0) AS DECIMAL(18,2)) as SlXuat,
                                   CAST(COALESCE(DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                                   CAST(COALESCE(TILEGIAMGIA, 0) AS DECIMAL(18,2)) as TiLeGiamGia
                            FROM TDONHANGCHITIET 
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        var item = await conn.QueryFirstOrDefaultAsync(sqlCheck, new { Id = chiTietId }, trans);
                        if (item != null)
                        {
                            string tenHang = item.TENHANG?.ToString() ?? "";
                            decimal currentSl = item.SLXUAT;
                            decimal donGia = item.DONGIA;
                            if (currentSl > soLuongGiam)
                            {
                                decimal newSl = currentSl - soLuongGiam;
                                decimal newThanhTien = newSl * item.DONGIA * (1 - (item.TILEGIAMGIA / 100m));
                                string sqlUpdate = @"
                                    UPDATE TDONHANGCHITIET 
                                    SET SLXUAT = @SlXuat, 
                                        THANHTIEN = @ThanhTien 
                                    WHERE CAST(ID AS VARCHAR(50)) = @Id";
                                await conn.ExecuteAsync(sqlUpdate, new { SlXuat = newSl, ThanhTien = newThanhTien, Id = chiTietId }, trans);
                            }
                            else
                            {
                                string sqlDelete = "DELETE FROM TDONHANGCHITIET WHERE CAST(ID AS VARCHAR(50)) = @Id";
                                await conn.ExecuteAsync(sqlDelete, new { Id = chiTietId }, trans);
                            }

                            await RecalculateOrderTotalsAsync(conn, trans, donHangId);
                            trans.Commit();

                            // Ghi lưu vết
                            _ = LocalLuuVetService.GhiLuuVetAsync(
                                donHangId, null, chucNang, 
                                $"Giảm '{tenHang}', số lượng: {soLuongGiam:0.##}", 
                                4, soLuongGiam, donGia, soLuongGiam * donGia, tenHang);

                            return true;
                        }
                        return false;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> XoaMonHoaDonAsync(string chiTietId, string donHangId, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(chiTietId) || string.IsNullOrEmpty(donHangId)) return false;

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = @"
                            SELECT TENHANG as TenHang,
                                   CAST(COALESCE(SLXUAT, SLNHAP, 0) AS DECIMAL(18,2)) as SlXuat,
                                   CAST(COALESCE(DONGIA, 0) AS DECIMAL(18,0)) as DonGia
                            FROM TDONHANGCHITIET 
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        var item = await conn.QueryFirstOrDefaultAsync(sqlCheck, new { Id = chiTietId }, trans);

                        string sqlDelete = "DELETE FROM TDONHANGCHITIET WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        await conn.ExecuteAsync(sqlDelete, new { Id = chiTietId }, trans);

                        await RecalculateOrderTotalsAsync(conn, trans, donHangId);
                        trans.Commit();

                        if (item != null)
                        {
                            string tenHang = item.TENHANG?.ToString() ?? "";
                            decimal sl = item.SLXUAT;
                            decimal donGia = item.DONGIA;

                            _ = LocalLuuVetService.GhiLuuVetAsync(
                                donHangId, null, chucNang, 
                                $"Xóa mặt hàng '{tenHang}' Số lượng '{sl:0.##}'", 
                                4, sl, donGia, sl * donGia, tenHang);
                        }

                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> UpdateSoLuongMonHoaDonAsync(string chiTietId, string donHangId, decimal newSoLuong, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(chiTietId) || string.IsNullOrEmpty(donHangId)) return false;

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = @"
                            SELECT TENHANG as TenHang,
                                   CAST(COALESCE(DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                                   CAST(COALESCE(TILEGIAMGIA, 0) AS DECIMAL(18,2)) as TiLeGiamGia
                            FROM TDONHANGCHITIET 
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        var item = await conn.QueryFirstOrDefaultAsync(sqlCheck, new { Id = chiTietId }, trans);

                        if (newSoLuong <= 0)
                        {
                            string sqlDelete = "DELETE FROM TDONHANGCHITIET WHERE CAST(ID AS VARCHAR(50)) = @Id";
                            await conn.ExecuteAsync(sqlDelete, new { Id = chiTietId }, trans);
                        }
                        else
                        {
                            if (item != null)
                            {
                                decimal newThanhTien = newSoLuong * item.DONGIA * (1 - (item.TILEGIAMGIA / 100m));
                                string sqlUpdate = "UPDATE TDONHANGCHITIET SET SLXUAT = @SlXuat, THANHTIEN = @ThanhTien WHERE CAST(ID AS VARCHAR(50)) = @Id";
                                await conn.ExecuteAsync(sqlUpdate, new { SlXuat = newSoLuong, ThanhTien = newThanhTien, Id = chiTietId }, trans);
                            }
                        }

                        await RecalculateOrderTotalsAsync(conn, trans, donHangId);
                        trans.Commit();

                        if (item != null)
                        {
                            string tenHang = item.TENHANG?.ToString() ?? "";
                            decimal donGia = item.DONGIA;

                            _ = LocalLuuVetService.GhiLuuVetAsync(
                                donHangId, null, chucNang, 
                                $"Đổi số lượng mặt hàng '{tenHang}' thành '{newSoLuong:0.##}'", 
                                4, newSoLuong, donGia, newSoLuong * donGia, tenHang);
                        }

                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> UpdateDonGiaMonHoaDonAsync(string chiTietId, string donHangId, decimal newDonGia, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(chiTietId) || string.IsNullOrEmpty(donHangId) || newDonGia < 0) return false;

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = @"
                            SELECT TENHANG as TenHang,
                                   CAST(COALESCE(SLXUAT, SLNHAP, 1) AS DECIMAL(18,2)) as SlXuat,
                                   CAST(COALESCE(TILEGIAMGIA, 0) AS DECIMAL(18,2)) as TiLeGiamGia
                            FROM TDONHANGCHITIET 
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        var item = await conn.QueryFirstOrDefaultAsync(sqlCheck, new { Id = chiTietId }, trans);
                        if (item != null)
                        {
                            decimal newThanhTien = item.SLXUAT * newDonGia * (1 - (item.TILEGIAMGIA / 100m));
                            string sqlUpdate = "UPDATE TDONHANGCHITIET SET DONGIA = @DonGia, THANHTIEN = @ThanhTien WHERE CAST(ID AS VARCHAR(50)) = @Id";
                            await conn.ExecuteAsync(sqlUpdate, new { DonGia = newDonGia, ThanhTien = newThanhTien, Id = chiTietId }, trans);

                            await RecalculateOrderTotalsAsync(conn, trans, donHangId);
                            trans.Commit();

                            string tenHang = item.TENHANG?.ToString() ?? "";
                            decimal sl = item.SLXUAT;

                            _ = LocalLuuVetService.GhiLuuVetAsync(
                                donHangId, null, chucNang, 
                                $"Đổi đơn giá mặt hàng '{tenHang}' thành '{newDonGia:N0}'", 
                                4, sl, newDonGia, sl * newDonGia, tenHang);

                            return true;
                        }

                        trans.Rollback();
                        return false;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> UpdateGhiChuMonHoaDonAsync(string chiTietId, string ghiChu, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(chiTietId)) return false;

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                var item = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(TDONHANGID AS VARCHAR(50)) as DonHangId, TENHANG FROM TDONHANGCHITIET WHERE CAST(ID AS VARCHAR(50)) = @Id", 
                    new { Id = chiTietId });

                string sqlUpdate = "UPDATE TDONHANGCHITIET SET NOTE = @Note WHERE CAST(ID AS VARCHAR(50)) = @Id";
                await conn.ExecuteAsync(sqlUpdate, new { Note = ghiChu ?? "", Id = chiTietId });

                if (item != null)
                {
                    string donHangId = item.DONHANGID?.ToString();
                    string tenHang = item.TENHANG?.ToString() ?? "";
                    _ = LocalLuuVetService.GhiLuuVetAsync(
                        donHangId, null, chucNang, 
                        $"Ghi chú mặt hàng '{tenHang}': '{ghiChu}'", 
                        4, 0, 0, 0, tenHang);
                }

                return true;
            }
        }

        public async Task<bool> UpdateChietKhauMonHoaDonAsync(string chiTietId, string donHangId, decimal chietKhauPt, string chucNang = "Điều chỉnh hóa đơn")
        {
            if (string.IsNullOrEmpty(chiTietId) || string.IsNullOrEmpty(donHangId) || chietKhauPt < 0 || chietKhauPt > 100) return false;

            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = @"
                            SELECT TENHANG as TenHang,
                                   CAST(COALESCE(SLXUAT, SLNHAP, 1) AS DECIMAL(18,2)) as SlXuat,
                                   CAST(COALESCE(DONGIA, 0) AS DECIMAL(18,0)) as DonGia
                            FROM TDONHANGCHITIET 
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        var item = await conn.QueryFirstOrDefaultAsync(sqlCheck, new { Id = chiTietId }, trans);
                        if (item != null)
                        {
                            decimal newThanhTien = item.SLXUAT * item.DONGIA * (1 - (chietKhauPt / 100m));
                            string sqlUpdate = "UPDATE TDONHANGCHITIET SET TILEGIAMGIA = @TiLeGiamGia, THANHTIEN = @ThanhTien WHERE CAST(ID AS VARCHAR(50)) = @Id";
                            await conn.ExecuteAsync(sqlUpdate, new { TiLeGiamGia = chietKhauPt, ThanhTien = newThanhTien, Id = chiTietId }, trans);

                            await RecalculateOrderTotalsAsync(conn, trans, donHangId);
                            trans.Commit();

                            string tenHang = item.TENHANG?.ToString() ?? "";
                            decimal sl = item.SLXUAT;
                            decimal donGia = item.DONGIA;

                            _ = LocalLuuVetService.GhiLuuVetAsync(
                                donHangId, null, chucNang, 
                                $"Chiết khấu mặt hàng '{tenHang}' {chietKhauPt}%", 
                                4, sl, donGia, 0, tenHang);

                            return true;
                        }

                        trans.Rollback();
                        return false;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        #endregion

        private async Task RecalculateOrderTotalsAsync(System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction trans, string donHangId)
        {
            string sqlSum = @"
                SELECT CAST(COALESCE(SUM(THANHTIEN), 0) AS DECIMAL(18,0)) 
                FROM TDONHANGCHITIET 
                WHERE CAST(TDONHANGID AS VARCHAR(50)) = @DonHangId";
            decimal newTienHang = await conn.ExecuteScalarAsync<decimal>(sqlSum, new { DonHangId = donHangId }, trans);

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
        }

        public async Task<List<TongHopBanHangTheoNgayItem>> GetTongHopBanHangTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = null, string nhanVienId = null, string khachHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(h.NGAY AS DATE) as Ngay,
                            SUM(CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,2))) as TienHang,
                            SUM(CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,2))) as GiamGia,
                            SUM(CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,2))) as TongCong
                        FROM TDONHANG h
                        WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                          AND (h.STATUS IS NULL OR h.STATUS <> 0) ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId) ";
                    }
                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }

                    sql += " GROUP BY CAST(h.NGAY AS DATE) ORDER BY CAST(h.NGAY AS DATE) ASC";

                    var result = await conn.QueryAsync<TongHopBanHangTheoNgayItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        NhanVienId = nhanVienId,
                        KhachHangId = khachHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải tổng hợp bán hàng theo ngày: " + ex.Message);
                return new List<TongHopBanHangTheoNgayItem>();
            }
        }

        public async Task<List<TongHopMatHangBanItem>> GetTongHopMatHangBanTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = null, string nhanVienId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(n.NAME, 'KHÁC') as TenNhom,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, 'đĩa') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(AVG(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId) ";
                    }

                    sql += " GROUP BY COALESCE(n.NAME, 'KHÁC'), COALESCE(c.TENHANG, m.NAME), dvt.NAME ORDER BY TenNhom ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangBanItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        NhanVienId = nhanVienId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải tổng hợp mặt hàng bán theo ngày: " + ex.Message);
                return new List<TongHopMatHangBanItem>();
            }
        }

        public async Task<List<BaoCaoChiTietBanHangOrderModel>> GetBaoCaoChiTietBanHangTheoNgayAsync(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sqlOrders = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as DonHangId,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as SoPhieu,
                            CAST(h.NGAY AS DATE) as Ngay,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIO, 0) AS DECIMAL(18,0)) as TienGio,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamTongBill,
                            CAST(COALESCE(h.PHIDICHVU, 0) AS DECIMAL(18,0)) as PhiDv,
                            CAST(COALESCE(h.TIENTHUE, 0) AS DECIMAL(18,0)) as Thue,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong,
                            CAST(COALESCE(h.DATHANHTOAN, h.TONGCONG, 0) AS DECIMAL(18,0)) as ThanhToan,
                            CAST(COALESCE(h.CONNO, 0) AS DECIMAL(18,0)) as ConNo
                        FROM TDONHANG h
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                        ORDER BY CAST(h.NGAY AS DATE) ASC, h.NAME ASC";

                    var orders = (await conn.QueryAsync<BaoCaoChiTietBanHangOrderModel>(sqlOrders, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date
                    })).ToList();

                    if (orders.Count == 0) return orders;

                    var orderIds = orders.Select(x => x.DonHangId).ToList();

                    string sqlItems = @"
                        SELECT 
                            CAST(c.TDONHANGID AS VARCHAR(50)) as DonHangId,
                            COALESCE(c.TENHANG, m.NAME) as MatHangBan,
                            CAST(COALESCE(c.SLXUAT, c.SLNHAP, 1) AS DECIMAL(18,2)) as Sl,
                            CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                            CAST(COALESCE(c.TILEGIAMGIA, 0) AS DECIMAL(18,2)) as PtCk,
                            CAST(COALESCE(c.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as TienGiamMh,
                            CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        WHERE CAST(c.TDONHANGID AS VARCHAR(50)) IN @OrderIds
                        ORDER BY c.ID ASC";

                    var items = (await conn.QueryAsync<BaoCaoChiTietBanHangItemModel>(sqlItems, new { OrderIds = orderIds })).ToList();

                    var itemsByOrder = items.GroupBy(x => x.DonHangId).ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var order in orders)
                    {
                        if (itemsByOrder.TryGetValue(order.DonHangId, out var itemList))
                        {
                            order.Items = itemList;
                        }
                        else
                        {
                            order.Items = new List<BaoCaoChiTietBanHangItemModel>();
                        }
                    }

                    return orders;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải báo cáo chi tiết bán hàng theo ngày: " + ex.Message);
                return new List<BaoCaoChiTietBanHangOrderModel>();
            }
        }

        public async Task<List<TongHopDoanhThuTheoLoaiDoItem>> GetTongHopDoanhThuTheoLoaiDoAsync(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sqlItems = @"
                        SELECT 
                            CAST(h.NGAY AS DATE) as Ngay,
                            COALESCE(n.NAME, '') as TenNhom,
                            COALESCE(c.TENHANG, m.NAME, '') as TenHang,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND h.NGAY IS NOT NULL
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                        GROUP BY CAST(h.NGAY AS DATE), COALESCE(n.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";

                    var items = (await conn.QueryAsync(sqlItems, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date
                    })).ToList();

                    string sqlHeaderTotals = @"
                        SELECT 
                            CAST(h.NGAY AS DATE) as Ngay,
                            CAST(SUM(COALESCE(h.TIENGIO, 0) + COALESCE(h.PHIDICHVU, 0)) AS DECIMAL(18,0)) as TienGioVaDichVu
                        FROM TDONHANG h
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND h.NGAY IS NOT NULL
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                        GROUP BY CAST(h.NGAY AS DATE)";

                    var headerRows = (await conn.QueryAsync(sqlHeaderTotals, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date
                    })).ToList();

                    var headerTotals = new Dictionary<DateTime, decimal>();
                    foreach (var hr in headerRows)
                    {
                        if (hr.Ngay != null)
                        {
                            DateTime d = Convert.ToDateTime(hr.Ngay);
                            decimal v = Convert.ToDecimal(hr.TienGioVaDichVu ?? 0);
                            headerTotals[d] = v;
                        }
                    }

                    var dictByDate = new Dictionary<DateTime, TongHopDoanhThuTheoLoaiDoItem>();

                    foreach (var row in items)
                    {
                        if (row.Ngay == null) continue;
                        DateTime dt = Convert.ToDateTime(row.Ngay);
                        string nhom = Convert.ToString(row.TenNhom ?? "").Trim();
                        string hang = Convert.ToString(row.TenHang ?? "").Trim();
                        decimal tt = Convert.ToDecimal(row.ThanhTien ?? 0);

                        if (!dictByDate.TryGetValue(dt, out var item))
                        {
                            item = new TongHopDoanhThuTheoLoaiDoItem { Ngay = dt };
                            dictByDate[dt] = item;
                        }

                        string combined = (nhom + " " + hang).ToUpper();

                        if (IsDrinkCategory(combined))
                        {
                            item.DoUong += tt;
                        }
                        else if (IsServiceCategory(combined))
                        {
                            item.DichVu += tt;
                        }
                        else if (IsOtherCategory(combined))
                        {
                            item.DoKhac += tt;
                        }
                        else
                        {
                            item.DoAn += tt;
                        }
                    }

                    foreach (var kvp in headerTotals)
                    {
                        DateTime dt = kvp.Key;
                        if (!dictByDate.TryGetValue(dt, out var item))
                        {
                            item = new TongHopDoanhThuTheoLoaiDoItem { Ngay = dt };
                            dictByDate[dt] = item;
                        }
                        item.DichVu += kvp.Value;
                    }

                    return dictByDate.Values.OrderBy(x => x.Ngay).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải tổng hợp doanh thu theo loại đồ: " + ex.Message);
                return new List<TongHopDoanhThuTheoLoaiDoItem>();
            }
        }

        private static bool IsDrinkCategory(string text)
        {
            string[] drinkKeywords = new[] { "UỐNG", "RƯỢU", "BIA", "NƯỚC", "GIẢI KHÁT", "NƯỚC NGỌT", "TRÀ", "CÀ PHÊ", "CAFE", "BEVERAGE", "DRINK", "SINH TỐ", "JUICE", "VỌC" };
            return drinkKeywords.Any(k => text.Contains(k));
        }

        private static bool IsServiceCategory(string text)
        {
            string[] serviceKeywords = new[] { "DỊCH VỤ", "DICH VU", "TIỀN GIỜ", "TIEN GIO", "KARAOKE", "SERVICE", "BIDA", "PHÒNG" };
            return serviceKeywords.Any(k => text.Contains(k));
        }

        private static bool IsOtherCategory(string text)
        {
            string[] otherKeywords = new[] { "ĐỒ KHÁC", "KHÁC", "OTHER", "DỤNG CỤ", "THUỐC LÁ", "VĂN PHÒNG PHẨM" };
            return otherKeywords.Any(k => text.Contains(k));
        }

        public async Task<List<TongHopDoanhThuChuaThanhToanItem>> GetTongHopDoanhThuChuaThanhToanAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as DonHangId,
                            CAST(h.NGAY AS DATE) as Ngay,
                            COALESCE(b.NAME, 'Bàn ' || h.DBANID, 'Mang về') as BanPhong,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as SoPhieu,
                            h.BATDAU as BatDauRaw,
                            h.KETTHUC as KetThucRaw,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND (h.STATUS <> 2 OR h.CONNO > 0 OR COALESCE(h.DATHANHTOAN, 0) < COALESCE(h.TONGCONG, 0) OR h.KETTHUC IS NULL)
                        ORDER BY CAST(h.NGAY AS DATE) ASC, h.NAME ASC";

                    var result = await conn.QueryAsync<TongHopDoanhThuChuaThanhToanItem>(sql);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải tổng hợp doanh thu chưa thanh toán: " + ex.Message);
                return new List<TongHopDoanhThuChuaThanhToanItem>();
            }
        }

        public async Task<List<BaoCaoBanHangTheoNgayOrderItem>> GetBaoCaoBanHangTheoNgayListAsync(DateTime tuNgay, DateTime denNgay, string nhanVienId = null, string khachHangId = null, string cuaHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as DonHangId,
                            CAST(h.NGAY AS DATE) as Ngay,
                            COALESCE(u.NAME, 'Administrator') as ThuNgan,
                            COALESCE(nv.NAME, '') as NhanVienBan,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as SoPhieu,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong,
                            CAST(COALESCE(h.TIENMAT, h.TONGCONG, 0) AS DECIMAL(18,0)) as TienMat,
                            CAST(COALESCE(h.CHUYENKHOAN, 0) AS DECIMAL(18,0)) as ChuyenKhoan,
                            CAST(COALESCE(h.THE, 0) AS DECIMAL(18,0)) as The,
                            CAST(COALESCE(h.THETRATRUOC, 0) AS DECIMAL(18,0)) as TheTt
                        FROM TDONHANG h
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId) ";
                    }
                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }
                    if (!string.IsNullOrEmpty(cuaHangId))
                    {
                        sql += " AND CAST(h.DCUAHANGID AS VARCHAR(50)) = @CuaHangId ";
                    }

                    sql += " ORDER BY CAST(h.NGAY AS DATE) ASC, ThuNgan ASC, h.NAME ASC";

                    var result = await conn.QueryAsync<BaoCaoBanHangTheoNgayOrderItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        NhanVienId = nhanVienId,
                        KhachHangId = khachHangId,
                        CuaHangId = cuaHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải báo cáo bán hàng theo ngày: " + ex.Message);
                return new List<BaoCaoBanHangTheoNgayOrderItem>();
            }
        }

        public async Task<List<TongHopBanTheoNhanVienItem>> GetTongHopBanTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoId = null, string nhanVienId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(nv.NAME, 'Chưa xác định') as NhanVien,
                            CAST(SUM(COALESCE(h.TIENHANG, 0)) AS DECIMAL(18,0)) as TienHang,
                            CAST(SUM(COALESCE(h.TIENGIAMGIA, 0)) AS DECIMAL(18,0)) as GiamGia,
                            CAST(SUM(COALESCE(h.TONGCONG, 0)) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DNHANVIEN nv ON (CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50)) OR CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50)))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId) ";
                    }

                    sql += " GROUP BY COALESCE(nv.NAME, 'Chưa xác định') ORDER BY NhanVien ASC";

                    var result = await conn.QueryAsync<TongHopBanTheoNhanVienItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        NhanVienId = nhanVienId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi tải tổng hợp bán theo nhân viên: " + ex.Message);
                return new List<TongHopBanTheoNhanVienItem>();
            }
        }

        public async Task<List<BaoCaoBanHangTheoNhanVienOrderItem>> GetBaoCaoBanHangTheoNhanVienListAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khachHangId = null, 
            string nhanVienXuatId = null, 
            string thanhToanBoiId = null, 
            string cuaHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as DonHangId,
                            CAST(h.NGAY AS DATE) as Ngay,
                            COALESCE(nv.NAME, u.NAME, 'Chưa xác định') as NhanVienBan,
                            CAST(COALESCE(nv.ID, u.ID, '0') AS VARCHAR(50)) as NhanVienId,
                            COALESCE(u.NAME, 'Administrator') as ThuNgan,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(20))) as SoPhieu,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong,
                            CAST(COALESCE(h.DKHACHHANGID, '0') AS VARCHAR(50)) as KhachHangId,
                            CAST(COALESCE(h.DCUAHANGID, '0') AS VARCHAR(50)) as CuaHangId
                        FROM TDONHANG h
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(nhanVienXuatId))
                    {
                        sql += " AND (CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienXuatId OR CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienXuatId) ";
                    }
                    if (!string.IsNullOrEmpty(thanhToanBoiId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @ThanhToanBoiId ";
                    }
                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }
                    if (!string.IsNullOrEmpty(cuaHangId))
                    {
                        sql += " AND CAST(h.DCUAHANGID AS VARCHAR(50)) = @CuaHangId ";
                    }

                    sql += " ORDER BY NhanVienBan ASC, CAST(h.NGAY AS DATE) ASC, h.NAME ASC";

                    var result = await conn.QueryAsync<BaoCaoBanHangTheoNhanVienOrderItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        NhanVienXuatId = nhanVienXuatId,
                        ThanhToanBoiId = thanhToanBoiId,
                        KhachHangId = khachHangId,
                        CuaHangId = cuaHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetBaoCaoBanHangTheoNhanVienListAsync: {ex.Message}");
                return new List<BaoCaoBanHangTheoNhanVienOrderItem>();
            }
        }

        public async Task<List<TongHopMatHangBanTheoNhanVienItem>> GetTongHopMatHangBanTheoNhanVienAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khoId = null, 
            string nhanVienId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(nv.NAME, u.NAME, 'Chưa xác định') as NhanVien,
                            COALESCE(n.NAME, 'KHÁC') as TenNhom,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, 'đĩa') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(AVG(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId) ";
                    }

                    sql += " GROUP BY COALESCE(nv.NAME, u.NAME, 'Chưa xác định'), COALESCE(n.NAME, 'KHÁC'), COALESCE(c.TENHANG, m.NAME), COALESCE(dvt.NAME, dvt2.NAME, 'đĩa') ORDER BY NhanVien ASC, TenNhom ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangBanTheoNhanVienItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        NhanVienId = nhanVienId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopMatHangBanTheoNhanVienAsync: {ex.Message}");
                return new List<TongHopMatHangBanTheoNhanVienItem>();
            }
        }

        public async Task<List<TongHopMatHangTheoNhomHienThiItem>> GetTongHopMatHangTheoNhomHienThiAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null, 
            string nhomHienThiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(n.NAME, 'KHÁC') as NhomHienThi,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, '') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(AVG(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomHienThiId))
                    {
                        sql += " AND (CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomHienThiId OR CAST(n.PARENTID AS VARCHAR(50)) = @NhomHienThiId) ";
                    }

                    sql += " GROUP BY COALESCE(n.NAME, 'KHÁC'), COALESCE(c.TENHANG, m.NAME), COALESCE(dvt.NAME, dvt2.NAME, '') ORDER BY NhomHienThi ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangTheoNhomHienThiItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomHienThiId = nhomHienThiId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopMatHangTheoNhomHienThiAsync: {ex.Message}");
                return new List<TongHopMatHangTheoNhomHienThiItem>();
            }
        }

        public async Task<List<TongHopBanHangTheoKhuVucItem>> GetTongHopBanHangTheoKhuVucAsync(
            DateTime tuNgay, 
            DateTime denNgay)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kv.NAME, 'Chưa xác định') as KhuVuc,
                            CAST(SUM(COALESCE(h.TIENHANG, 0)) AS DECIMAL(18,0)) as TienHang,
                            CAST(SUM(COALESCE(h.TIENGIAMGIA, 0)) AS DECIMAL(18,0)) as GiamGia,
                            CAST(SUM(COALESCE(h.TONGCONG, 0)) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                        GROUP BY COALESCE(kv.NAME, 'Chưa xác định')
                        ORDER BY KhuVuc ASC";

                    var result = await conn.QueryAsync<TongHopBanHangTheoKhuVucItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopBanHangTheoKhuVucAsync: {ex.Message}");
                return new List<TongHopBanHangTheoKhuVucItem>();
            }
        }

        public async Task<List<TongHopBanHangTheoBanPhongItem>> GetTongHopBanHangTheoBanPhongAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null, 
            string nhomHienThiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kv.NAME, 'Chưa xác định') as KhuVuc,
                            COALESCE(b.NAME, 'Chưa xác định') as BanPhong,
                            CAST(SUM(COALESCE(h.TIENHANG, 0)) AS DECIMAL(18,0)) as TienHang,
                            CAST(SUM(COALESCE(h.TIENGIAMGIA, 0)) AS DECIMAL(18,0)) as GiamGia,
                            CAST(SUM(COALESCE(h.TONGCONG, 0)) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomHienThiId))
                    {
                        sql += " AND CAST(b.DNHOMHIENTHIID AS VARCHAR(50)) = @NhomHienThiId ";
                    }

                    sql += " GROUP BY COALESCE(kv.NAME, 'Chưa xác định'), COALESCE(b.NAME, 'Chưa xác định') ORDER BY KhuVuc ASC, BanPhong ASC";

                    var result = await conn.QueryAsync<TongHopBanHangTheoBanPhongItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomHienThiId = nhomHienThiId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopBanHangTheoBanPhongAsync: {ex.Message}");
                return new List<TongHopBanHangTheoBanPhongItem>();
            }
        }

        public async Task<List<DanhSachHoaDonTheoKhuVucItem>> GetDanhSachHoaDonTheoKhuVucAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kv.NAME, 'Chưa xác định') as KhuVuc,
                            h.NGAY as Ngay,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            COALESCE(b.NAME, 'Chưa xác định') as BanPhong,
                            COALESCE(kh.NAME, '') as KhachHang,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }

                    sql += " ORDER BY KhuVuc ASC, h.NGAY ASC, h.TIMECREATED ASC";

                    var result = await conn.QueryAsync<DanhSachHoaDonTheoKhuVucItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId
                    });

                    var list = result.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].STT = i + 1;
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetDanhSachHoaDonTheoKhuVucAsync: {ex.Message}");
                return new List<DanhSachHoaDonTheoKhuVucItem>();
            }
        }

        public async Task<List<DanhSachHoaDonTheoBanItem>> GetDanhSachHoaDonTheoBanAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null,
            string nhomHienThiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(b.NAME, 'Chưa xác định') as BanPhong,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            h.NGAY as Ngay,
                            COALESCE(kh.NAME, '') as KhachHang,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong,
                            COALESCE(kv.NAME, '') as KhuVuc
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomHienThiId))
                    {
                        sql += " AND CAST(b.DNHOMHIENTHIID AS VARCHAR(50)) = @NhomHienThiId ";
                    }

                    sql += " ORDER BY BanPhong ASC, h.NGAY ASC, h.TIMECREATED ASC";

                    var result = await conn.QueryAsync<DanhSachHoaDonTheoBanItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomHienThiId = nhomHienThiId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetDanhSachHoaDonTheoBanAsync: {ex.Message}");
                return new List<DanhSachHoaDonTheoBanItem>();
            }
        }

        public async Task<List<TongHopMatHangBanTheoKhuVucItem>> GetTongHopMatHangBanTheoKhuVucAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null, 
            string nhomMatHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kv.NAME, 'Chưa xác định') as KhuVuc,
                            COALESCE(n.NAME, 'KHÁC') as TenNhom,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, '') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(AVG(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomMatHangId))
                    {
                        sql += " AND CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomMatHangId ";
                    }

                    sql += " GROUP BY COALESCE(kv.NAME, 'Chưa xác định'), COALESCE(n.NAME, 'KHÁC'), COALESCE(c.TENHANG, m.NAME), COALESCE(dvt.NAME, dvt2.NAME, '') ORDER BY KhuVuc ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangBanTheoKhuVucItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomMatHangId = nhomMatHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopMatHangBanTheoKhuVucAsync: {ex.Message}");
                return new List<TongHopMatHangBanTheoKhuVucItem>();
            }
        }

        public async Task<List<TongHopMatHangBanTheoBanItem>> GetTongHopMatHangBanTheoBanAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null, 
            string nhomMatHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(b.NAME, 'Chưa xác định') as BanPhong,
                            COALESCE(n.NAME, 'KHÁC') as TenNhom,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, '') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(AVG(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien,
                            COALESCE(kv.NAME, '') as KhuVuc
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomMatHangId))
                    {
                        sql += " AND CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomMatHangId ";
                    }

                    sql += " GROUP BY COALESCE(b.NAME, 'Chưa xác định'), COALESCE(n.NAME, 'KHÁC'), COALESCE(c.TENHANG, m.NAME), COALESCE(dvt.NAME, dvt2.NAME, ''), COALESCE(kv.NAME, '') ORDER BY BanPhong ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangBanTheoBanItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomMatHangId = nhomMatHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopMatHangBanTheoBanAsync: {ex.Message}");
                return new List<TongHopMatHangBanTheoBanItem>();
            }
        }

        public async Task<List<TongHopBanHangTheoNhomHienThiItem>> GetTongHopBanHangTheoNhomHienThiAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null, 
            string nhomHienThiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(nh.NAME, 'KHÁC') as NhomHienThi,
                            CAST(SUM(COALESCE(h.TIENHANG, 0)) AS DECIMAL(18,0)) as TienHang,
                            CAST(SUM(COALESCE(h.TIENGIAMGIA, 0)) AS DECIMAL(18,0)) as GiamGia,
                            CAST(SUM(COALESCE(h.TONGCONG, 0)) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(b.DNHOMHIENTHIID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomHienThiId))
                    {
                        sql += " AND CAST(b.DNHOMHIENTHIID AS VARCHAR(50)) = @NhomHienThiId ";
                    }

                    sql += " GROUP BY COALESCE(nh.NAME, 'KHÁC') ORDER BY NhomHienThi ASC";

                    var result = await conn.QueryAsync<TongHopBanHangTheoNhomHienThiItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomHienThiId = nhomHienThiId
                    });

                    var list = result.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].STT = i + 1;
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopBanHangTheoNhomHienThiAsync: {ex.Message}");
                return new List<TongHopBanHangTheoNhomHienThiItem>();
            }
        }

        public async Task<List<DanhSachHoaDonTheoNhomHienThiItem>> GetDanhSachHoaDonTheoNhomHienThiAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khuVucId = null, 
            string nhomHienThiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(nh.NAME, 'Chưa xác định') as NhomHienThi,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            h.NGAY as Ngay,
                            COALESCE(kh.NAME, '') as KhachHang,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong,
                            COALESCE(b.NAME, '') as BanPhong,
                            COALESCE(kv.NAME, '') as KhuVuc
                        FROM TDONHANG h
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(b.DNHOMHIENTHIID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sql += " AND (CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVucId OR CAST(kv.PARENTID AS VARCHAR(50)) = @KhuVucId) ";
                    }
                    if (!string.IsNullOrEmpty(nhomHienThiId))
                    {
                        sql += " AND CAST(b.DNHOMHIENTHIID AS VARCHAR(50)) = @NhomHienThiId ";
                    }

                    sql += " ORDER BY NhomHienThi ASC, h.NGAY ASC, h.TIMECREATED ASC";

                    var result = await conn.QueryAsync<DanhSachHoaDonTheoNhomHienThiItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhuVucId = khuVucId,
                        NhomHienThiId = nhomHienThiId
                    });

                    var list = result.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].STT = i + 1;
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetDanhSachHoaDonTheoNhomHienThiAsync: {ex.Message}");
                return new List<DanhSachHoaDonTheoNhomHienThiItem>();
            }
        }

        public async Task<List<TongHopBanTheoThuNganItem>> GetTongHopBanTheoThuNganAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khoId = null,
            string nhanVienXuatId = null,
            string thanhToanBoiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(u.NAME, su.NAME, u.USERNAME, 'Administrator') as ThuNgan,
                            CAST(SUM(COALESCE(h.TIENHANG, 0)) AS DECIMAL(18,0)) as TienHang,
                            CAST(SUM(COALESCE(h.TIENGIAMGIA, 0)) AS DECIMAL(18,0)) as GiamGia,
                            CAST(SUM(COALESCE(h.TONGCONG, 0)) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN SUSER su ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(su.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienXuatId))
                    {
                        sql += " AND CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienXuatId ";
                    }
                    if (!string.IsNullOrEmpty(thanhToanBoiId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @ThanhToanBoiId ";
                    }

                    sql += " GROUP BY COALESCE(u.NAME, su.NAME, u.USERNAME, 'Administrator') ORDER BY ThuNgan ASC";

                    var result = await conn.QueryAsync<TongHopBanTheoThuNganItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        NhanVienXuatId = nhanVienXuatId,
                        ThanhToanBoiId = thanhToanBoiId
                    });

                    var list = result.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].STT = i + 1;
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopBanTheoThuNganAsync: {ex.Message}");
                return new List<TongHopBanTheoThuNganItem>();
            }
        }

        public async Task<List<TongHopMatHangBanTheoThuNganItem>> GetTongHopMatHangBanTheoThuNganAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khoId = null,
            string thanhToanBoiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(u.NAME, su.NAME, u.USERNAME, 'Administrator') as ThuNgan,
                            COALESCE(n.NAME, 'KHÁC') as TenNhom,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, '') as Dvt,
                            CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                            CAST(AVG(COALESCE(c.TILEGIAMGIA, 0)) AS DECIMAL(18,2)) as GiamGiaPhanTram,
                            CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN SUSER su ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(su.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(thanhToanBoiId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @ThanhToanBoiId ";
                    }

                    sql += " GROUP BY COALESCE(u.NAME, su.NAME, u.USERNAME, 'Administrator'), COALESCE(n.NAME, 'KHÁC'), COALESCE(c.TENHANG, m.NAME), COALESCE(dvt.NAME, dvt2.NAME, '') ORDER BY ThuNgan ASC, TenNhom ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangBanTheoThuNganItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        ThanhToanBoiId = thanhToanBoiId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopMatHangBanTheoThuNganAsync: {ex.Message}");
                return new List<TongHopMatHangBanTheoThuNganItem>();
            }
        }

        public async Task<List<BaoCaoBanHangTheoThuNganItem>> GetBaoCaoBanHangTheoThuNganAsync(
            DateTime tuNgay, 
            DateTime denNgay, 
            string khachHangId = null,
            string nhanVienXuatId = null,
            string thanhToanBoiId = null,
            string cuaHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(u.NAME, su.NAME, u.USERNAME, 'Administrator') as ThuNgan,
                            h.NGAY as Ngay,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            COALESCE(kh.NAME, '') as KhachHang,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN SUSER su ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(su.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienXuatId))
                    {
                        sql += " AND CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienXuatId ";
                    }
                    if (!string.IsNullOrEmpty(thanhToanBoiId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @ThanhToanBoiId ";
                    }
                    if (!string.IsNullOrEmpty(cuaHangId))
                    {
                        sql += " AND CAST(h.DCUAHANGID AS VARCHAR(50)) = @CuaHangId ";
                    }

                    sql += " ORDER BY ThuNgan ASC, h.NGAY ASC, h.TIMECREATED ASC";

                    var result = await conn.QueryAsync<BaoCaoBanHangTheoThuNganItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhachHangId = khachHangId,
                        NhanVienXuatId = nhanVienXuatId,
                        ThanhToanBoiId = thanhToanBoiId,
                        CuaHangId = cuaHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetBaoCaoBanHangTheoThuNganAsync: {ex.Message}");
                return new List<BaoCaoBanHangTheoThuNganItem>();
            }
        }

        public async Task<List<TongHopHoaHongTheoNvkdItem>> GetTongHopHoaHongTheoNvkdAsync(
            DateTime tuNgay,
            DateTime denNgay,
            string khoId = null,
            string nhanVienBanId = null,
            string nhanVienXuatId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(nv.NAME, 'Nhân viên bán:') as NhanVienBan,
                            COALESCE(c.TENHANG, m.NAME, '') as TenHang,
                            SUM(CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,3))) as SoLuong,
                            SUM(CAST(COALESCE(c.CHIETKHAU, 0) AS DECIMAL(18,2))) as HoaHong,
                            SUM(CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0))) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienBanId))
                    {
                        sql += " AND CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienBanId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienXuatId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienXuatId ";
                    }

                    sql += " GROUP BY nv.ID, nv.NAME, c.TENHANG, m.NAME ORDER BY NhanVienBan ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopHoaHongTheoNvkdItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhoId = khoId,
                        NhanVienBanId = nhanVienBanId,
                        NhanVienXuatId = nhanVienXuatId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopHoaHongTheoNvkdAsync: {ex.Message}");
                return new List<TongHopHoaHongTheoNvkdItem>();
            }
        }

        public async Task<List<ChiTietBanHangTheoHoaDonItem>> GetChiTietBanHangTheoHoaDonAsync(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as DonHangId,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            h.NGAY as Ngay,
                            CAST(COALESCE(h.BATDAU, h.TIMECREATED) AS VARCHAR(50)) as Gio,
                            COALESCE(su.NAME, u.NAME, u.USERNAME, 'Administrator') as ThuNgan,
                            COALESCE(nvb.NAME, '') as NvBan,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGiaHoaDon,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCongHoaDon,
                            COALESCE(c.TENHANG, m.NAME, '') as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, '') as Dvt,
                            CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,3)) as SoLuong,
                            CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                            CAST(COALESCE(c.GIAMGIA, 0) AS DECIMAL(18,2)) as CkPercent,
                            CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN SUSER su ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(su.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nvb ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nvb.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay
                        ORDER BY h.NGAY ASC, h.ID ASC, c.ID ASC";

                    var result = await conn.QueryAsync<ChiTietBanHangTheoHoaDonItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetChiTietBanHangTheoHoaDonAsync: {ex.Message}");
                return new List<ChiTietBanHangTheoHoaDonItem>();
            }
        }

        public async Task<List<BaoCaoChiTietHangKhuyenMaiItem>> GetBaoCaoChiTietHangKhuyenMaiAsync(
            DateTime tuNgay,
            DateTime denNgay,
            string thanhToanBoiId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            h.NGAY as Ngay,
                            COALESCE(kh.NAME, 'Khách lẻ') as KhachHang,
                            COALESCE(c.TENHANG, m.NAME, '') as MatHang,
                            CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,3)) as SoLuong
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND (c.DONGIA = 0 OR c.GIAMGIA = 100 OR c.THANHTIEN = 0)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(thanhToanBoiId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @ThanhToanBoiId ";
                    }

                    sql += " ORDER BY h.NGAY ASC, h.ID ASC, c.ID ASC";

                    var result = await conn.QueryAsync<BaoCaoChiTietHangKhuyenMaiItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        ThanhToanBoiId = thanhToanBoiId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetBaoCaoChiTietHangKhuyenMaiAsync: {ex.Message}");
                return new List<BaoCaoChiTietHangKhuyenMaiItem>();
            }
        }

        public async Task<List<TongHopBanTheoKhachHangItem>> GetTongHopBanTheoKhachHangAsync(
            DateTime tuNgay,
            DateTime denNgay,
            string khachHangId = null,
            string khoId = null,
            string nhanVienId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kh.MAKHACH, '') as MaKhach,
                            COALESCE(kh.NAME, 'Khách lẻ / Chưa xác định') as TenKhach,
                            CAST(COALESCE(kh.DIACHI, '') AS VARCHAR(255)) as DiaChi,
                            SUM(CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0))) as TienHang,
                            SUM(CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0))) as GiamGia,
                            SUM(CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }
                    if (!string.IsNullOrEmpty(khoId))
                    {
                        sql += " AND CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId OR CAST(h.USERCREATEDID AS VARCHAR(50)) = @NhanVienId) ";
                    }

                    sql += " GROUP BY kh.ID, kh.MAKHACH, kh.NAME, kh.DIACHI ORDER BY TenKhach ASC";

                    var result = await conn.QueryAsync<TongHopBanTheoKhachHangItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhachHangId = khachHangId,
                        KhoId = khoId,
                        NhanVienId = nhanVienId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopBanTheoKhachHangAsync: {ex.Message}");
                return new List<TongHopBanTheoKhachHangItem>();
            }
        }

        public async Task<List<TongHopMatHangBanTheoKhachHangItem>> GetTongHopMatHangBanTheoKhachHangAsync(
            DateTime tuNgay,
            DateTime denNgay,
            string nhomKhachId = null,
            string khachHangId = null,
            string matHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kh.NAME, 'Khách lẻ / Chưa xác định') as KhachHang,
                            COALESCE(c.TENHANG, m.NAME, '') as TenHang,
                            COALESCE(dvt.NAME, dvt2.NAME, '') as Dvt,
                            SUM(CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,3))) as SoLuong,
                            MAX(CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0))) as DonGia,
                            MAX(CAST(COALESCE(c.GIAMGIA, 0) AS DECIMAL(18,2))) as GiamGiaPhanTram,
                            SUM(CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0))) as ThanhTien
                        FROM TDONHANGCHITIET c
                        INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt2 ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt2.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(nhomKhachId))
                    {
                        sql += " AND CAST(kh.NHOMID AS VARCHAR(50)) = @NhomKhachId ";
                    }
                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }
                    if (!string.IsNullOrEmpty(matHangId))
                    {
                        sql += " AND CAST(c.DMATHANGID AS VARCHAR(50)) = @MatHangId ";
                    }

                    sql += " GROUP BY kh.ID, kh.NAME, c.TENHANG, m.NAME, dvt.NAME, dvt2.NAME ORDER BY KhachHang ASC, TenHang ASC";

                    var result = await conn.QueryAsync<TongHopMatHangBanTheoKhachHangItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        NhomKhachId = nhomKhachId,
                        KhachHangId = khachHangId,
                        MatHangId = matHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTongHopMatHangBanTheoKhachHangAsync: {ex.Message}");
                return new List<TongHopMatHangBanTheoKhachHangItem>();
            }
        }

        public async Task<List<BaoCaoBanHangTheoKhachHangItem>> GetBaoCaoBanHangTheoKhachHangAsync(
            DateTime tuNgay,
            DateTime denNgay,
            string khachHangId = null,
            string nhanVienXuatId = null,
            string thanhToanBoiId = null,
            string cuaHangId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kh.NAME, 'Khách lẻ / Chưa xác định') as KhachHang,
                            h.NGAY as Ngay,
                            COALESCE(h.NAME, CAST(h.SOHD AS VARCHAR(50))) as SoPhieu,
                            CAST(COALESCE(h.TIENHANG, 0) AS DECIMAL(18,0)) as TienHang,
                            CAST(COALESCE(h.TIENGIAMGIA, 0) AS DECIMAL(18,0)) as GiamGia,
                            CAST(COALESCE(h.TONGCONG, 0) AS DECIMAL(18,0)) as TongCong
                        FROM TDONHANG h
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND CAST(h.NGAY AS DATE) >= @TuNgay 
                          AND CAST(h.NGAY AS DATE) <= @DenNgay ";

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(h.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId ";
                    }
                    if (!string.IsNullOrEmpty(nhanVienXuatId))
                    {
                        sql += " AND CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienXuatId ";
                    }
                    if (!string.IsNullOrEmpty(thanhToanBoiId))
                    {
                        sql += " AND CAST(h.USERCREATEDID AS VARCHAR(50)) = @ThanhToanBoiId ";
                    }
                    if (!string.IsNullOrEmpty(cuaHangId))
                    {
                        sql += " AND CAST(h.DCUAHANGID AS VARCHAR(50)) = @CuaHangId ";
                    }

                    sql += " ORDER BY KhachHang ASC, h.NGAY ASC, h.TIMECREATED ASC";

                    var result = await conn.QueryAsync<BaoCaoBanHangTheoKhachHangItem>(sql, new
                    {
                        TuNgay = tuNgay.Date,
                        DenNgay = denNgay.Date,
                        KhachHangId = khachHangId,
                        NhanVienXuatId = nhanVienXuatId,
                        ThanhToanBoiId = thanhToanBoiId,
                        CuaHangId = cuaHangId
                    });

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetBaoCaoBanHangTheoKhachHangAsync: {ex.Message}");
                return new List<BaoCaoBanHangTheoKhachHangItem>();
            }
        }

        public async Task<List<BaoCaoTongHopBanTheoThangGroup>> GetBaoCaoTongHopBanTheoThangAsync(int thang, int nam, bool isGiaTri)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    DateTime tuNgay = new DateTime(nam, thang, 1);
                    DateTime denNgay = tuNgay.AddMonths(1).AddDays(-1);

                    string sql = @"
                        SELECT 
                            COALESCE(m.CODE, '') as MaSP,
                            COALESCE(m.NAME, ct.TENHANG, '') as SanPham,
                            COALESCE(d.NAME, '') as Dvt,
                            COALESCE(n.NAME, 'KHÁC') as TenNhom,
                            CAST(EXTRACT(DAY FROM h.NGAY) AS INTEGER) as Ngay,
                            CAST(SUM(COALESCE(ct.SLXUAT, ct.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                            CAST(SUM(COALESCE(ct.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                        FROM TDONHANGCHITIET ct
                        JOIN TDONHANG h ON CAST(ct.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(ct.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH d ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG n ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                          AND h.NGAY IS NOT NULL
                          AND CAST(h.NGAY AS DATE) >= @TuNgay AND CAST(h.NGAY AS DATE) <= @DenNgay
                        GROUP BY COALESCE(m.CODE, ''), COALESCE(m.NAME, ct.TENHANG, ''), COALESCE(d.NAME, ''), COALESCE(n.NAME, 'KHÁC'), EXTRACT(DAY FROM h.NGAY)
                        ORDER BY TenNhom ASC, SanPham ASC
                    ";

                    var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay, DenNgay = denNgay })).ToList();

                    var groupedDict = new Dictionary<string, Dictionary<string, BaoCaoTongHopBanTheoThangItem>>();

                    foreach (var row in rows)
                    {
                        string tenNhom = row.TENNHOM ?? "KHÁC";
                        string maSP = row.MASP ?? "";
                        string sanPham = row.SANPHAM ?? "";
                        string dvt = row.DVT ?? "";
                        int ngay = Convert.ToInt32(row.NGAY);
                        decimal soLuong = Convert.ToDecimal(row.SOLUONG);
                        decimal thanhTien = Convert.ToDecimal(row.THANHTIEN);

                        // If isGiaTri (Giá trị bán in thousands like 450 for 450,000)
                        decimal val = isGiaTri ? (thanhTien / 1000m) : soLuong;

                        if (!groupedDict.ContainsKey(tenNhom))
                            groupedDict[tenNhom] = new Dictionary<string, BaoCaoTongHopBanTheoThangItem>();

                        string itemKey = maSP + "_" + sanPham;
                        if (!groupedDict[tenNhom].ContainsKey(itemKey))
                        {
                            groupedDict[tenNhom][itemKey] = new BaoCaoTongHopBanTheoThangItem
                            {
                                MaSP = maSP,
                                SanPham = sanPham,
                                Dvt = dvt,
                                TenNhom = tenNhom
                            };
                        }

                        var item = groupedDict[tenNhom][itemKey];
                        if (ngay >= 1 && ngay <= 31)
                        {
                            item.NgayVal[ngay] += val;
                            item.TongBan += val;
                        }
                    }

                    var resultGroups = new List<BaoCaoTongHopBanTheoThangGroup>();

                    foreach (var groupKv in groupedDict)
                    {
                        var grp = new BaoCaoTongHopBanTheoThangGroup
                        {
                            TenNhom = groupKv.Key,
                            Items = groupKv.Value.Values.ToList()
                        };

                        int stt = 1;
                        foreach (var it in grp.Items)
                        {
                            it.STT = stt++;
                            grp.TongNhom += it.TongBan;
                            for (int d = 1; d <= 31; d++)
                            {
                                grp.TongNgayNhom[d] += it.NgayVal[d];
                            }
                        }

                        resultGroups.Add(grp);
                    }

                    return resultGroups;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetBaoCaoTongHopBanTheoThangAsync: {ex.Message}");
                return new List<BaoCaoTongHopBanTheoThangGroup>();
            }
        }
    }

    public class BaoCaoTongHopBanTheoThangItem
    {
        public int STT { get; set; }
        public string MaSP { get; set; } = "";
        public string SanPham { get; set; } = "";
        public string Dvt { get; set; } = "";
        public string TenNhom { get; set; } = "KHÁC";
        public decimal TongBan { get; set; }
        public decimal[] NgayVal { get; set; } = new decimal[32]; // index 1..31

        public string TongBanDisplay => TongBan != 0 ? TongBan.ToString("#,##0.##") : "";
        public string D01 => NgayVal[1] != 0 ? NgayVal[1].ToString("#,##0.##") : "";
        public string D02 => NgayVal[2] != 0 ? NgayVal[2].ToString("#,##0.##") : "";
        public string D03 => NgayVal[3] != 0 ? NgayVal[3].ToString("#,##0.##") : "";
        public string D04 => NgayVal[4] != 0 ? NgayVal[4].ToString("#,##0.##") : "";
        public string D05 => NgayVal[5] != 0 ? NgayVal[5].ToString("#,##0.##") : "";
        public string D06 => NgayVal[6] != 0 ? NgayVal[6].ToString("#,##0.##") : "";
        public string D07 => NgayVal[7] != 0 ? NgayVal[7].ToString("#,##0.##") : "";
        public string D08 => NgayVal[8] != 0 ? NgayVal[8].ToString("#,##0.##") : "";
        public string D09 => NgayVal[9] != 0 ? NgayVal[9].ToString("#,##0.##") : "";
        public string D10 => NgayVal[10] != 0 ? NgayVal[10].ToString("#,##0.##") : "";
        public string D11 => NgayVal[11] != 0 ? NgayVal[11].ToString("#,##0.##") : "";
        public string D12 => NgayVal[12] != 0 ? NgayVal[12].ToString("#,##0.##") : "";
        public string D13 => NgayVal[13] != 0 ? NgayVal[13].ToString("#,##0.##") : "";
        public string D14 => NgayVal[14] != 0 ? NgayVal[14].ToString("#,##0.##") : "";
        public string D15 => NgayVal[15] != 0 ? NgayVal[15].ToString("#,##0.##") : "";
        public string D16 => NgayVal[16] != 0 ? NgayVal[16].ToString("#,##0.##") : "";
        public string D17 => NgayVal[17] != 0 ? NgayVal[17].ToString("#,##0.##") : "";
        public string D18 => NgayVal[18] != 0 ? NgayVal[18].ToString("#,##0.##") : "";
        public string D19 => NgayVal[19] != 0 ? NgayVal[19].ToString("#,##0.##") : "";
        public string D20 => NgayVal[20] != 0 ? NgayVal[20].ToString("#,##0.##") : "";
        public string D21 => NgayVal[21] != 0 ? NgayVal[21].ToString("#,##0.##") : "";
        public string D22 => NgayVal[22] != 0 ? NgayVal[22].ToString("#,##0.##") : "";
        public string D23 => NgayVal[23] != 0 ? NgayVal[23].ToString("#,##0.##") : "";
        public string D24 => NgayVal[24] != 0 ? NgayVal[24].ToString("#,##0.##") : "";
        public string D25 => NgayVal[25] != 0 ? NgayVal[25].ToString("#,##0.##") : "";
        public string D26 => NgayVal[26] != 0 ? NgayVal[26].ToString("#,##0.##") : "";
        public string D27 => NgayVal[27] != 0 ? NgayVal[27].ToString("#,##0.##") : "";
        public string D28 => NgayVal[28] != 0 ? NgayVal[28].ToString("#,##0.##") : "";
        public string D29 => NgayVal[29] != 0 ? NgayVal[29].ToString("#,##0.##") : "";
        public string D30 => NgayVal[30] != 0 ? NgayVal[30].ToString("#,##0.##") : "";
        public string D31 => NgayVal[31] != 0 ? NgayVal[31].ToString("#,##0.##") : "";
    }

    public class BaoCaoTongHopBanTheoThangGroup
    {
        public string TenNhom { get; set; } = "";
        public List<BaoCaoTongHopBanTheoThangItem> Items { get; set; } = new List<BaoCaoTongHopBanTheoThangItem>();
        public decimal TongNhom { get; set; }
        public decimal[] TongNgayNhom { get; set; } = new decimal[32];

        public string TongNhomDisplay => TongNhom != 0 ? TongNhom.ToString("#,##0.##") : "";
        public string D01 => TongNgayNhom[1] != 0 ? TongNgayNhom[1].ToString("#,##0.##") : "";
        public string D02 => TongNgayNhom[2] != 0 ? TongNgayNhom[2].ToString("#,##0.##") : "";
        public string D03 => TongNgayNhom[3] != 0 ? TongNgayNhom[3].ToString("#,##0.##") : "";
        public string D04 => TongNgayNhom[4] != 0 ? TongNgayNhom[4].ToString("#,##0.##") : "";
        public string D05 => TongNgayNhom[5] != 0 ? TongNgayNhom[5].ToString("#,##0.##") : "";
        public string D06 => TongNgayNhom[6] != 0 ? TongNgayNhom[6].ToString("#,##0.##") : "";
        public string D07 => TongNgayNhom[7] != 0 ? TongNgayNhom[7].ToString("#,##0.##") : "";
        public string D08 => TongNgayNhom[8] != 0 ? TongNgayNhom[8].ToString("#,##0.##") : "";
        public string D09 => TongNgayNhom[9] != 0 ? TongNgayNhom[9].ToString("#,##0.##") : "";
        public string D10 => TongNgayNhom[10] != 0 ? TongNgayNhom[10].ToString("#,##0.##") : "";
        public string D11 => TongNgayNhom[11] != 0 ? TongNgayNhom[11].ToString("#,##0.##") : "";
        public string D12 => TongNgayNhom[12] != 0 ? TongNgayNhom[12].ToString("#,##0.##") : "";
        public string D13 => TongNgayNhom[13] != 0 ? TongNgayNhom[13].ToString("#,##0.##") : "";
        public string D14 => TongNgayNhom[14] != 0 ? TongNgayNhom[14].ToString("#,##0.##") : "";
        public string D15 => TongNgayNhom[15] != 0 ? TongNgayNhom[15].ToString("#,##0.##") : "";
        public string D16 => TongNgayNhom[16] != 0 ? TongNgayNhom[16].ToString("#,##0.##") : "";
        public string D17 => TongNgayNhom[17] != 0 ? TongNgayNhom[17].ToString("#,##0.##") : "";
        public string D18 => TongNgayNhom[18] != 0 ? TongNgayNhom[18].ToString("#,##0.##") : "";
        public string D19 => TongNgayNhom[19] != 0 ? TongNgayNhom[19].ToString("#,##0.##") : "";
        public string D20 => TongNgayNhom[20] != 0 ? TongNgayNhom[20].ToString("#,##0.##") : "";
        public string D21 => TongNgayNhom[21] != 0 ? TongNgayNhom[21].ToString("#,##0.##") : "";
        public string D22 => TongNgayNhom[22] != 0 ? TongNgayNhom[22].ToString("#,##0.##") : "";
        public string D23 => TongNgayNhom[23] != 0 ? TongNgayNhom[23].ToString("#,##0.##") : "";
        public string D24 => TongNgayNhom[24] != 0 ? TongNgayNhom[24].ToString("#,##0.##") : "";
        public string D25 => TongNgayNhom[25] != 0 ? TongNgayNhom[25].ToString("#,##0.##") : "";
        public string D26 => TongNgayNhom[26] != 0 ? TongNgayNhom[26].ToString("#,##0.##") : "";
        public string D27 => TongNgayNhom[27] != 0 ? TongNgayNhom[27].ToString("#,##0.##") : "";
        public string D28 => TongNgayNhom[28] != 0 ? TongNgayNhom[28].ToString("#,##0.##") : "";
        public string D29 => TongNgayNhom[29] != 0 ? TongNgayNhom[29].ToString("#,##0.##") : "";
        public string D30 => TongNgayNhom[30] != 0 ? TongNgayNhom[30].ToString("#,##0.##") : "";
        public string D31 => TongNgayNhom[31] != 0 ? TongNgayNhom[31].ToString("#,##0.##") : "";
    }

    public class TongHopBanHangTheoNgayItem
    {
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class TongHopMatHangBanItem
    {
        public string TenNhom { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class BaoCaoChiTietBanHangOrderModel
    {
        public string DonHangId { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public decimal TienHang { get; set; }
        public decimal TienGio { get; set; }
        public decimal GiamTongBill { get; set; }
        public decimal PhiDv { get; set; }
        public decimal Thue { get; set; }
        public decimal TongCong { get; set; }
        public decimal ThanhToan { get; set; }
        public decimal ConNo { get; set; }
        public List<BaoCaoChiTietBanHangItemModel> Items { get; set; } = new List<BaoCaoChiTietBanHangItemModel>();
    }

    public class BaoCaoChiTietBanHangItemModel
    {
        public string DonHangId { get; set; } = "";
        public string MatHangBan { get; set; } = "";
        public decimal Sl { get; set; }
        public decimal DonGia { get; set; }
        public decimal PtCk { get; set; }
        public decimal TienGiamMh { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class TongHopDoanhThuTheoLoaiDoItem
    {
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public decimal DoAn { get; set; }
        public decimal DoUong { get; set; }
        public decimal DichVu { get; set; }
        public decimal DoKhac { get; set; }
        public decimal Cong => DoAn + DoUong + DichVu + DoKhac;
    }

    public class TongHopDoanhThuChuaThanhToanItem
    {
        public string DonHangId { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string BanPhong { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public object BatDauRaw { get; set; }
        public object KetThucRaw { get; set; }
        public string BatDau => FormatTime(BatDauRaw);
        public string KetThuc => FormatTime(KetThucRaw);
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }

        private static string FormatTime(object raw)
        {
            if (raw == null || raw == DBNull.Value) return "";
            if (raw is DateTime dt) return dt.ToString("HH:mm");
            if (raw is TimeSpan ts) return ts.ToString(@"hh\:mm");
            string str = raw.ToString() ?? "";
            if (DateTime.TryParse(str, out var parsedDt)) return parsedDt.ToString("HH:mm");
            if (TimeSpan.TryParse(str, out var parsedTs)) return parsedTs.ToString(@"hh\:mm");
            return str;
        }
    }

    public class BaoCaoBanHangTheoNgayOrderItem
    {
        public string DonHangId { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string ThuNgan { get; set; } = "Administrator";
        public string NhanVienBan { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal The { get; set; }
        public decimal TheTt { get; set; }
    }

    public class TongHopBanTheoNhanVienItem
    {
        public string NhanVien { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class BaoCaoBanHangTheoNhanVienOrderItem
    {
        public string DonHangId { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string NhanVienBan { get; set; } = "";
        public string NhanVienId { get; set; } = "";
        public string ThuNgan { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
        public string KhachHangId { get; set; } = "";
        public string CuaHangId { get; set; } = "";
    }

    public class TongHopMatHangBanTheoNhanVienItem
    {
        public string NhanVien { get; set; } = "Chưa xác định";
        public string TenNhom { get; set; } = "KHÁC";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class TongHopMatHangTheoNhomHienThiItem
    {
        public string NhomHienThi { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class TongHopBanHangTheoKhuVucItem
    {
        public string KhuVuc { get; set; } = "Chưa xác định";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class TongHopBanHangTheoBanPhongItem
    {
        public string KhuVuc { get; set; } = "Chưa xác định";
        public string BanPhong { get; set; } = "Chưa xác định";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class DanhSachHoaDonTheoKhuVucItem
    {
        public int STT { get; set; }
        public string KhuVuc { get; set; } = "Chưa xác định";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string SoPhieu { get; set; } = "";
        public string BanPhong { get; set; } = "";
        public string KhachHang { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class DanhSachHoaDonTheoBanItem
    {
        public int STT { get; set; }
        public string BanPhong { get; set; } = "Chưa xác định";
        public string SoPhieu { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string KhachHang { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
        public string KhuVuc { get; set; } = "";
    }

    public class TongHopMatHangBanTheoKhuVucItem
    {
        public int STT { get; set; }
        public string KhuVuc { get; set; } = "Chưa xác định";
        public string TenNhom { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class TongHopMatHangBanTheoBanItem
    {
        public int STT { get; set; }
        public string BanPhong { get; set; } = "Chưa xác định";
        public string TenNhom { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
        public string KhuVuc { get; set; } = "";
    }

    public class TongHopBanHangTheoNhomHienThiItem
    {
        public int STT { get; set; }
        public string NhomHienThi { get; set; } = "KHÁC";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class DanhSachHoaDonTheoNhomHienThiItem
    {
        public int STT { get; set; }
        public string NhomHienThi { get; set; } = "Chưa xác định";
        public string SoPhieu { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string KhachHang { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
        public string BanPhong { get; set; } = "";
        public string KhuVuc { get; set; } = "";
    }

    public class TongHopBanTheoThuNganItem
    {
        public int STT { get; set; }
        public string ThuNgan { get; set; } = "Administrator";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class TongHopMatHangBanTheoThuNganItem
    {
        public int STT { get; set; }
        public string ThuNgan { get; set; } = "Administrator";
        public string TenNhom { get; set; } = "KHÁC";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class BaoCaoBanHangTheoThuNganItem
    {
        public int STT { get; set; }
        public string ThuNgan { get; set; } = "Administrator";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string SoPhieu { get; set; } = "";
        public string KhachHang { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class TongHopBanTheoKhachHangItem
    {
        public int STT { get; set; }
        public string MaKhach { get; set; } = "";
        public string TenKhach { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class TongHopMatHangBanTheoKhachHangItem
    {
        public int STT { get; set; }
        public string KhachHang { get; set; } = "Khách lẻ / Chưa xác định";
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiamGiaPhanTram { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class BaoCaoBanHangTheoKhachHangItem
    {
        public int STT { get; set; }
        public string KhachHang { get; set; } = "Khách lẻ / Chưa xác định";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string SoPhieu { get; set; } = "";
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongCong { get; set; }
    }

    public class TongHopHoaHongTheoNvkdItem
    {
        public int STT { get; set; }
        public string NhanVienBan { get; set; } = "Nhân viên bán:";
        public string TenHang { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal HoaHong { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class ChiTietBanHangTheoHoaDonItem
    {
        public string DonHangId { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string Gio { get; set; } = "";
        public string ThuNgan { get; set; } = "";
        public string NvBan { get; set; } = "";
        public decimal GiamGiaHoaDon { get; set; }
        public decimal TongCongHoaDon { get; set; }
        public int STT { get; set; }
        public string TenHang { get; set; } = "";
        public string Dvt { get; set; } = "";
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal CkPercent { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class BaoCaoChiTietHangKhuyenMaiItem
    {
        public int STT { get; set; }
        public string SoPhieu { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string NgayDisplay => Ngay.ToString("dd/MM/yyyy");
        public string KhachHang { get; set; } = "";
        public string MatHang { get; set; } = "";
        public decimal SoLuong { get; set; }
    }
}
