using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using Dapper;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalSuDungDichVuService
    {
        private class DbanRowDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DkhuvucId { get; set; }
        }

        private class ActiveOrderDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DbanId { get; set; }
            public DateTime? BatDau { get; set; }
            public DateTime? Ngay { get; set; }
            public int? SoKhach { get; set; }
            public string DkhachHangId { get; set; }
            public string Note { get; set; }
            public decimal? TienHang { get; set; }
            public decimal? TienGiamGia { get; set; }
            public decimal? TongCong { get; set; }
        }

        public async Task<List<PosKhuVucViewModel>> GetKhuVucBanListAsync()
        {
            var result = new List<PosKhuVucViewModel>();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Lấy danh sách khu vực
                    var khuvucList = (await conn.QueryAsync<PosKhuVucViewModel>(
                        "SELECT ID as Id, NAME as Name FROM DKHUVUC WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME"
                    )).ToList();

                    // Lấy danh sách bàn
                    var banList = (await conn.QueryAsync<DbanRowDto>(
                        "SELECT ID as Id, NAME as Name, DKHUVUCID as DkhuvucId FROM DBAN WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME"
                    )).ToList();

                    // Lấy các đơn hàng đang mở (chưa kết thúc)
                    string sqlActiveOrders = @"
                        SELECT ID as Id, NAME as Name, DBANID as DbanId, BATDAU as BatDau, 
                               NGAY as Ngay, SOKHACH as SoKhach, DKHACHHANGID as DkhachHangId, 
                               NOTE as Note, TIENHANG as TienHang, TIENGIAMGIA as TienGiamGia, TONGCONG as TongCong
                        FROM TDONHANG
                        WHERE (STATUS = 1 OR STATUS IS NULL) AND KETTHUC IS NULL AND DBANID IS NOT NULL";
                    
                    var activeOrders = (await conn.QueryAsync<ActiveOrderDto>(sqlActiveOrders)).ToList();

                    foreach (var kv in khuvucList)
                    {
                        var kvBans = banList.Where(b => b.DkhuvucId == kv.Id).ToList();
                        foreach (var b in kvBans)
                        {
                            var activeOrder = activeOrders.FirstOrDefault(o => o.DbanId == b.Id);
                            bool isOcc = activeOrder != null;

                            var banModel = new PosBanViewModel
                            {
                                Id = b.Id,
                                Name = b.Name,
                                KhuVucId = kv.Id,
                                KhuVucName = kv.Name,
                                IsOccupied = isOcc,
                                ActiveOrderId = activeOrder?.Id,
                                StartTime = activeOrder?.BatDau ?? (isOcc ? (activeOrder?.Ngay ?? DateTime.Now) : (DateTime?)null),
                                SoPhieu = activeOrder?.Name ?? "",
                                SoKhach = activeOrder?.SoKhach ?? 0,
                                GhiChu = activeOrder?.Note ?? "",
                                TienHang = activeOrder?.TienHang ?? 0,
                                GiamGia = activeOrder?.TienGiamGia ?? 0,
                                TongCong = activeOrder?.TongCong ?? 0
                            };

                            banModel.UpdateTimerText();
                            kv.BanList.Add(banModel);
                        }

                        result.Add(kv);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy danh sách bàn khu vực: " + ex.Message);
                return result;
            }
        }

        public class StartOrderResult
        {
            public string OrderId { get; set; }
            public string SoPhieu { get; set; }
            public int SoHd { get; set; }
        }

        public async Task<string> GetNextSoPhieuAsync(DateTime dateTime)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    DateTime monthStart = new DateTime(dateTime.Year, dateTime.Month, 1);
                    DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    string prefix = $"{dateTime:MM}{dateTime:yy}";

                    int maxFromDonHang = await conn.ExecuteScalarAsync<int>(
                        "SELECT COALESCE(MAX(SOHD), 0) FROM TDONHANG WHERE NAME LIKE @Prefix", 
                        new { Prefix = prefix + "%" });

                    string tsoSql = "SELECT FIRST 1 ID, SO FROM TSOHOADON WHERE CAST(NGAY AS DATE) >= @MonthStart AND CAST(NGAY AS DATE) <= @MonthEnd ORDER BY TIMECREATED DESC";
                    var tsoRow = await conn.QueryFirstOrDefaultAsync(tsoSql, new { MonthStart = monthStart.Date, MonthEnd = monthEnd.Date });
                    int maxFromTso = 0;
                    if (tsoRow != null)
                    {
                        int.TryParse(tsoRow.SO?.ToString(), out maxFromTso);
                    }

                    int nextNumber = Math.Max(maxFromDonHang, maxFromTso) + 1;
                    return $"{prefix}{nextNumber:D5}";
                }
            }
            catch
            {
                return $"{dateTime:MM}{dateTime:yy}00001";
            }
        }

        public async Task<StartOrderResult> StartTableOrderAsync(string banId, DateTime startTime, int soKhach, string khachHangId, string ghiChu)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string orderId = Guid.NewGuid().ToString();
                    DateTime monthStart = new DateTime(startTime.Year, startTime.Month, 1);
                    DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    string prefix = $"{startTime:MM}{startTime:yy}";

                    // Lấy số thứ tự lớn nhất từ TDONHANG và TSOHOADON
                    int maxFromDonHang = await conn.ExecuteScalarAsync<int>(
                        "SELECT COALESCE(MAX(SOHD), 0) FROM TDONHANG WHERE NAME LIKE @Prefix", 
                        new { Prefix = prefix + "%" });

                    string tsoSql = "SELECT FIRST 1 ID, SO FROM TSOHOADON WHERE CAST(NGAY AS DATE) >= @MonthStart AND CAST(NGAY AS DATE) <= @MonthEnd ORDER BY TIMECREATED DESC";
                    var tsoRow = await conn.QueryFirstOrDefaultAsync(tsoSql, new { MonthStart = monthStart.Date, MonthEnd = monthEnd.Date });
                    int maxFromTso = 0;
                    string tsoId = null;
                    if (tsoRow != null)
                    {
                        tsoId = tsoRow.ID?.ToString();
                        int.TryParse(tsoRow.SO?.ToString(), out maxFromTso);
                    }

                    int nextSo = Math.Max(maxFromDonHang, maxFromTso) + 1;
                    string soPhieu = $"{prefix}{nextSo:D5}";

                    int userCreatedId = 1;
                    if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUserId))
                    {
                        userCreatedId = parsedUserId;
                    }

                    // Cập nhật hoặc thêm mới vào TSOHOADON
                    if (!string.IsNullOrEmpty(tsoId))
                    {
                        string updateTsoSql = "UPDATE TSOHOADON SET SO = @So, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = @UserId WHERE CAST(ID AS VARCHAR(50)) = @TsoId";
                        await conn.ExecuteAsync(updateTsoSql, new { So = nextSo.ToString(), UserId = userCreatedId, TsoId = tsoId });
                    }
                    else
                    {
                        string insertTsoSql = @"
                            INSERT INTO TSOHOADON (
                                ID, NGAY, SO, STATUS, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @Ngay, @So, 1, @UserId, CURRENT_TIMESTAMP
                            )";
                        await conn.ExecuteAsync(insertTsoSql, new { 
                            Id = Guid.NewGuid().ToString(), 
                            Ngay = monthStart, 
                            So = nextSo.ToString(), 
                            UserId = userCreatedId 
                        });
                    }

                    // Tạo mới đơn hàng trong TDONHANG
                    string insertSql = @"
                        INSERT INTO TDONHANG (
                            ID, NAME, SOHD, SOTT, DBANID, BATDAU, NGAY, SOKHACH, DKHACHHANGID, NOTE, STATUS, USERCREATEDID, TIMECREATED
                        ) VALUES (
                            @Id, @SoPhieu, @SoHd, @SoHd, @DbanId, @BatDau, @Ngay, @SoKhach, @KhachHangId, @Note, 1, @UserCreatedId, CURRENT_TIMESTAMP
                        )";

                    await conn.ExecuteAsync(insertSql, new
                    {
                        Id = orderId,
                        SoPhieu = soPhieu,
                        SoHd = nextSo,
                        DbanId = banId,
                        BatDau = startTime,
                        Ngay = startTime.Date,
                        SoKhach = soKhach.ToString(),
                        KhachHangId = !string.IsNullOrEmpty(khachHangId) ? khachHangId : null,
                        Note = ghiChu,
                        UserCreatedId = userCreatedId
                    });

                    return new StartOrderResult
                    {
                        OrderId = orderId,
                        SoPhieu = soPhieu,
                        SoHd = nextSo
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi bắt đầu mở bàn: " + ex.Message);
                return null;
            }
        }

        public async Task<List<PosDonHangChiTietViewModel>> GetOrderDetailsAsync(string orderId)
        {
            var result = new List<PosDonHangChiTietViewModel>();
            if (string.IsNullOrEmpty(orderId)) return result;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT c.ID as Id, c.DMATHANGID as MatHangId, c.TENHANG as MatHangName, 
                               COALESCE(dvt.NAME, 'đĩa') as DonViTinh, COALESCE(c.SLXUAT, 1) as SoLuong, 
                               c.DONGIA as DonGia, c.TILEGIAMGIA as ChietKhauPhanTram, 
                               c.THANHTIEN as ThanhTien, c.NOTE as GhiChu,
                               COALESCE(n.DLOAIDOID, 1) as LoaiDoId,
                               COALESCE(ld.NAME, 'Đồ ăn') as LoaiDoName,
                               CASE WHEN c.DTRANGTHAICHEBIENID = 1 THEN 1 ELSE 0 END as DaInCheBien
                        FROM TDONHANGCHITIET c
                        LEFT JOIN DMATHANG m ON c.DMATHANGID = m.ID
                        LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                        LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID
                        LEFT JOIN DLOAIDO ld ON n.DLOAIDOID = ld.ID
                        WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @OrderId AND (c.STATUS <> 0 OR c.STATUS IS NULL)
                        ORDER BY c.ID";

                    var items = (await conn.QueryAsync<PosDonHangChiTietViewModel>(sql, new { OrderId = orderId })).ToList();
                    return items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy chi tiết đơn hàng: " + ex.Message);
                return result;
            }
        }

        public async Task<bool> SaveOrderAsync(string orderId, List<PosDonHangChiTietViewModel> items, decimal tienHang, decimal giamGia, decimal tongCong, string ghiChu, int soKhach)
        {
            if (string.IsNullOrEmpty(orderId)) return false;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        int userCreatedId = 1;
                        if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUserId))
                        {
                            userCreatedId = parsedUserId;
                        }

                        // 1. Cập nhật TDONHANG
                        string updateOrderSql = @"
                            UPDATE TDONHANG 
                            SET TIENHANG = @TienHang, TIENGIAMGIA = @GiamGia, TONGCONG = @TongCong, 
                                NOTE = @Note, SOKHACH = @SoKhach, USERMODIFIEDID = @UserModifiedId, TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(updateOrderSql, new
                        {
                            Id = orderId,
                            TienHang = tienHang,
                            GiamGia = giamGia,
                            TongCong = tongCong,
                            Note = ghiChu,
                            SoKhach = soKhach.ToString(),
                            UserModifiedId = userCreatedId
                        }, trans);

                        // 2. Xóa chi tiết cũ và thêm mới
                        await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @OrderId", new { OrderId = orderId }, trans);

                        string insertDetailSql = @"
                            INSERT INTO TDONHANGCHITIET (
                                ID, TDONHANGID, DMATHANGID, TENHANG, DONGIA, SLXUAT, THANHTIEN, TILEGIAMGIA, NOTE, STATUS, DTRANGTHAICHEBIENID, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @OrderId, @MatHangId, @MatHangName, @DonGia, @SoLuong, @ThanhTien, @ChietKhauPhanTram, @GhiChu, 1, @DaInCheBien, @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        foreach (var it in items)
                        {
                            string detailId = Guid.NewGuid().ToString();
                            await conn.ExecuteAsync(insertDetailSql, new
                            {
                                Id = detailId,
                                OrderId = orderId,
                                MatHangId = it.MatHangId,
                                MatHangName = it.MatHangName,
                                DonGia = it.DonGia,
                                SoLuong = it.SoLuong.ToString(),
                                ThanhTien = it.ThanhTien,
                                ChietKhauPhanTram = it.ChietKhauPhanTram,
                                GhiChu = it.GhiChu,
                                DaInCheBien = it.DaInCheBien ? 1 : 0,
                                UserCreatedId = userCreatedId
                            }, trans);
                        }

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu đơn hàng: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateOrderDateToTodayAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE TDONHANG SET NGAY = CURRENT_DATE, TIMECREATED = CURRENT_TIMESTAMP WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    await conn.ExecuteAsync(sql, new { OrderId = orderId });
                    return true;
                }
            }
            catch { return false; }
        }

        public async Task<bool> FinishTableOrderWithDetailsAsync(string orderId, decimal khachDua, decimal traLai, decimal theATM, decimal theTraTruoc, string loaiThanhToan)
        {
            if (string.IsNullOrEmpty(orderId)) return false;

            try
            {
                int loaiTtInt = 0;
                if (loaiThanhToan == "ChuyenKhoan") loaiTtInt = 1;
                else if (loaiThanhToan == "TheATM" || loaiThanhToan == "The") loaiTtInt = 2;
                else if (loaiThanhToan == "TheTraTruoc") loaiTtInt = 3;
                else if (loaiThanhToan == "CongNo" || loaiThanhToan == "KhachNo") loaiTtInt = 4;
                else if (loaiThanhToan == "Voucher") loaiTtInt = 5;

                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        UPDATE TDONHANG 
                        SET KETTHUC = CURRENT_TIMESTAMP, 
                            GIOTHANHTOAN = CURRENT_TIMESTAMP, 
                            STATUS = 2,
                            KHACHDUA = @KhachDua,
                            TRALAI = @TraLai,
                            TIENMAT = @TienMat,
                            THE = @TheATM,
                            THETRATRUOC = @TheTraTruoc,
                            LOAITHANHTOAN = @LoaiTtInt
                        WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    await conn.ExecuteAsync(sql, new { 
                        OrderId = orderId, 
                        KhachDua = khachDua.ToString("0.##"),
                        TraLai = traLai.ToString("0.##"),
                        TienMat = loaiTtInt == 0 ? khachDua : 0,
                        TheATM = theATM.ToString("0.##"),
                        TheTraTruoc = theTraTruoc.ToString("0.##"),
                        LoaiTtInt = loaiTtInt
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết thúc đơn hàng: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> FinishTableOrderAsync(string orderId)
        {
            return await FinishTableOrderWithDetailsAsync(orderId, 0, 0, 0, 0, "TienMat");
        }

        public async Task<int> GetThoiGianChoPhepHuyBillMinutesAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    var minutes = await conn.ExecuteScalarAsync<int?>(
                        "SELECT FIRST 1 INTVALUE FROM SCONFIG WHERE NAME = 'ThoiGianChoPhepHuyBill' AND STATUS > 0"
                    );
                    return (minutes.HasValue && minutes.Value > 0) ? minutes.Value : 50;
                }
            }
            catch
            {
                return 50;
            }
        }

        public async Task<bool> CancelOrderAsync(string orderId, string lyDoHuy = "")
        {
            if (string.IsNullOrEmpty(orderId)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        int userCreatedId = 1;
                        if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUserId))
                        {
                            userCreatedId = parsedUserId;
                        }

                        string noteSuffix = string.IsNullOrWhiteSpace(lyDoHuy) ? " [Đã hủy]" : $" [Hủy: {lyDoHuy}]";

                        // 1. Update TDONHANG
                        string sqlOrder = @"
                            UPDATE TDONHANG 
                            SET STATUS = 0, 
                                KETTHUC = CURRENT_TIMESTAMP, 
                                TIMEMODIFIED = CURRENT_TIMESTAMP,
                                NOTE = COALESCE(NOTE, '') || @NoteSuffix
                            WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                        await conn.ExecuteAsync(sqlOrder, new { OrderId = orderId, NoteSuffix = noteSuffix }, trans);

                        // 2. Update TDONHANGCHITIET
                        string sqlDetails = @"
                            UPDATE TDONHANGCHITIET 
                            SET STATUS = 0, 
                                TIMEMODIFIED = CURRENT_TIMESTAMP 
                            WHERE CAST(TDONHANGID AS VARCHAR(50)) = @OrderId";
                        await conn.ExecuteAsync(sqlDetails, new { OrderId = orderId }, trans);

                        // 3. Copy sang TDONHANGHUY (nếu có bảng)
                        try
                        {
                            string copyHuySql = @"
                                INSERT INTO TDONHANGHUY (
                                    ID, NAME, NOTE, STATUS, USERMODIFIEDID, TIMEMODIFIED, TIMECREATED, 
                                    NGAY, USERCREATEDID, KHACHHANG, NHANVEN, THUNGAN, TIENHANG, 
                                    TIENGIAMGIA, TILEGIAMGIA, TONGCONG, NGAYHUY, GIOHUY, TDONHANGID, LYDOHUY, BAN
                                )
                                SELECT 
                                    h.ID, h.NAME, h.NOTE, 0, @UserId, CURRENT_TIMESTAMP, h.TIMECREATED,
                                    h.NGAY, @UserId, k.NAME, u.NAME, u.NAME, h.TIENHANG,
                                    h.TIENGIAMGIA, h.TILEGIAMGIA, h.TONGCONG, CURRENT_DATE, CURRENT_TIMESTAMP, h.ID, @LyDoHuy, b.NAME
                                FROM TDONHANG h
                                LEFT JOIN DBAN b ON h.DBANID = b.ID
                                LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                                LEFT JOIN SUSER u ON CAST(u.ID AS VARCHAR(50)) = CAST(@UserId AS VARCHAR(50))
                                WHERE CAST(h.ID AS VARCHAR(50)) = @OrderId";

                            await conn.ExecuteAsync(copyHuySql, new { OrderId = orderId, UserId = userCreatedId, LyDoHuy = lyDoHuy }, trans);
                        }
                        catch
                        {
                            // Bỏ qua nếu có cột bảng lịch sử hủy khác biệt
                        }

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hủy hóa đơn: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> TransferTableAsync(string orderId, string newBanId)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(newBanId)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        UPDATE TDONHANG 
                        SET DBANID = @BanId, TIMEMODIFIED = CURRENT_TIMESTAMP
                        WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    int rows = await conn.ExecuteAsync(sql, new { BanId = newBanId, OrderId = orderId });
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chuyển bàn: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @OrderId", new { OrderId = orderId }, trans);
                        await conn.ExecuteAsync("DELETE FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @OrderId", new { OrderId = orderId }, trans);
                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa đơn hàng khi gộp: " + ex.Message);
                return false;
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

        public async Task<bool> UpdateOrderCustomerAsync(string orderId, string khachHangId)
        {
            if (string.IsNullOrEmpty(orderId)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE TDONHANG SET DKHACHHANGID = @KhachHangId WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    await conn.ExecuteAsync(sql, new { KhachHangId = khachHangId, OrderId = orderId });
                    return true;
                }
            }
            catch { return false; }
        }

        public async Task<ObservableCollection<PosNhomMatHangViewModel>> GetNhomMatHangTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    var flatList = (await conn.QueryAsync<PosNhomMatHangViewModel>(
                        "SELECT ID as Id, NAME as Name, PARENTID as ParentId FROM DNHOMMATHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME"
                    )).ToList();

                    if (flatList.Count == 0)
                    {
                        try 
                        {
                            flatList = (await conn.QueryAsync<PosNhomMatHangViewModel>(
                                "SELECT ID as Id, NAME as Name, PARENTID as ParentId FROM DLOAIMATHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME"
                            )).ToList();
                        } 
                        catch { }
                    }

                    var rootItems = new ObservableCollection<PosNhomMatHangViewModel>();
                    var rootAll = new PosNhomMatHangViewModel { Id = string.Empty, Name = "Tất cả", ParentId = null };
                    rootItems.Add(rootAll);

                    var lookup = flatList.ToDictionary(g => g.Id);

                    foreach (var item in flatList)
                    {
                        if (!string.IsNullOrEmpty(item.ParentId) && lookup.ContainsKey(item.ParentId))
                        {
                            lookup[item.ParentId].Children.Add(item);
                        }
                        else
                        {
                            rootAll.Children.Add(item);
                        }
                    }

                    return rootItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy nhóm món: " + ex.Message);
                return new ObservableCollection<PosNhomMatHangViewModel>();
            }
        }

        public async Task<List<PosMatHangViewModel>> GetMatHangListAsync(string nhomId, string searchKeyword = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT 
                            m.ID as Id, m.CODE as Code, m.NAME as Name, m.GIABAN as GiaBan,
                            COALESCE(dvt.NAME, 'đĩa') as DonViTinh,
                            COALESCE(n.DLOAIDOID, 1) as LoaiDoId,
                            COALESCE(ld.NAME, 'Đồ ăn') as LoaiDoName
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                        LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID
                        LEFT JOIN DLOAIDO ld ON n.DLOAIDOID = ld.ID
                        WHERE (m.STATUS <> 0 OR m.STATUS IS NULL)
                    ";

                    if (!string.IsNullOrEmpty(nhomId))
                    {
                        sql += " AND (m.DNHOMMATHANGID = @NhomId OR m.DLOAIMATHANGID = @NhomId)";
                    }

                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        sql += " AND (UPPER(m.NAME) LIKE UPPER(@Keyword) OR UPPER(m.CODE) LIKE UPPER(@Keyword))";
                    }

                    sql += " ORDER BY m.NAME";

                    var result = await conn.QueryAsync<PosMatHangViewModel>(sql, new { 
                        NhomId = nhomId, 
                        Keyword = $"%{searchKeyword}%" 
                    });
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy món ăn: " + ex.Message);
                return new List<PosMatHangViewModel>();
            }
        }
        public async Task<List<DichVuYeuCauViewModel>> GetDichVuYeuCauListAsync()
        {
            var list = new List<DichVuYeuCauViewModel>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT 
                            CAST(d.ID AS VARCHAR(50)) as Id, 
                            CAST(d.DBANID AS VARCHAR(50)) as BanId, 
                            b.NAME as Phong, 
                            COALESCE(d.NOTE, 'Yêu cầu phục vụ') as NoiDung,
                            1 as SoLan,
                            d.TIMECREATED as ThoiGian
                        FROM TDONHANG d
                        JOIN DBAN b ON d.DBANID = b.ID
                        WHERE d.STATUS = 1 AND d.NOTE IS NOT NULL AND TRIM(d.NOTE) <> ''
                        ORDER BY d.TIMECREATED DESC";
                    
                    var rows = (await conn.QueryAsync<DichVuYeuCauViewModel>(sql)).ToList();
                    return rows;
                }
            }
            catch
            {
                return list;
            }
        }
    }
}
