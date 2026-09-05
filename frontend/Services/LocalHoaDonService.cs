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
    }
}
