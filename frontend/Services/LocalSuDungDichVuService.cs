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
            public string DnhanVienXuatId { get; set; }
            public string NhanVienName { get; set; }
            public string Note { get; set; }
            public decimal? TienHang { get; set; }
            public decimal? TienGiamGia { get; set; }
            public decimal? TienThue { get; set; }
            public decimal? ThueSuatPt { get; set; }
            public decimal? TienPhiDichVu { get; set; }
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
                        SELECT o.ID as Id, o.NAME as Name, o.DBANID as DbanId, o.BATDAU as BatDau, 
                               o.NGAY as Ngay, o.SOKHACH as SoKhach, o.DKHACHHANGID as DkhachHangId, 
                               CAST(o.DNHANVIENXUATID AS VARCHAR(50)) as DnhanVienXuatId,
                               nv.NAME as NhanVienName,
                               o.NOTE as Note, o.TIENHANG as TienHang, o.TIENGIAMGIA as TienGiamGia, 
                               COALESCE(o.TIENTHUE, 0) as TienThue, COALESCE(o.TILETHUE, 0) as ThueSuatPt,
                               COALESCE(o.PHIDICHVU, 0) as TienPhiDichVu, o.TONGCONG as TongCong
                        FROM TDONHANG o
                        LEFT JOIN DNHANVIEN nv ON CAST(o.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE (o.STATUS = 1 OR o.STATUS IS NULL) AND o.KETTHUC IS NULL AND o.DBANID IS NOT NULL";
                    
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
                                NhanVienId = activeOrder?.DnhanVienXuatId ?? "",
                                NhanVienName = activeOrder?.NhanVienName ?? "",
                                GhiChu = activeOrder?.Note ?? "",
                                TienHang = activeOrder?.TienHang ?? 0,
                                GiamGia = activeOrder?.TienGiamGia ?? 0,
                                TienThue = activeOrder?.TienThue ?? 0,
                                ThueSuatPt = activeOrder?.ThueSuatPt ?? 0,
                                TienPhiDichVu = activeOrder?.TienPhiDichVu ?? 0,
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

        private async Task<string> GetFormatPatternAsync(System.Data.Common.DbConnection conn, string tableName, string defaultPattern)
        {
            try
            {
                var format = await conn.ExecuteScalarAsync<string>("SELECT FIRST 1 FORMAT FROM STABLEDESC WHERE UPPER(NAME) = UPPER(@Name)", new { Name = tableName });
                if (!string.IsNullOrWhiteSpace(format))
                {
                    return format.Trim();
                }
            }
            catch { }
            return defaultPattern;
        }

        public static (DateTime? StartDate, DateTime? EndDate) GetResetPeriod(string pattern, DateTime date)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return (new DateTime(date.Year, 1, 1), new DateTime(date.Year, 12, 31));
            }

            if (pattern.IndexOf("(dd)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return (date.Date, date.Date);
            }
            else if (pattern.IndexOf("(MM)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var start = new DateTime(date.Year, date.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                return (start, end);
            }
            else if (pattern.IndexOf("(yyyy)", StringComparison.OrdinalIgnoreCase) >= 0 || pattern.IndexOf("(yy)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var start = new DateTime(date.Year, 1, 1);
                var end = new DateTime(date.Year, 12, 31);
                return (start, end);
            }
            else
            {
                return (null, null);
            }
        }

        public static string ApplyPattern(string pattern, DateTime date, int sequence)
        {
            if (string.IsNullOrWhiteSpace(pattern)) pattern = "(yy)(******)";

            string result = pattern;
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(yyyy\)", date.ToString("yyyy"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(yy\)", date.ToString("yy"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(MM\)", date.ToString("MM"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(dd\)", date.ToString("dd"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var match = System.Text.RegularExpressions.Regex.Match(result, @"\(\*+\)");
            if (match.Success)
            {
                int starCount = match.Value.Length - 2;
                if (starCount < 1) starCount = 5;
                if (starCount > 9) starCount = 9;
                string seqStr = sequence.ToString().PadLeft(starCount, '0');
                result = result.Replace(match.Value, seqStr);
            }
            else
            {
                result += sequence.ToString("D5");
            }

            return result;
        }

        public async Task<string> GetNextSoPhieuAsync(DateTime dateTime)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string pattern = await LocalCauHinhService.GetFormatPatternAsync("TSOHOADON", "(yy)(******)");
                    var (periodStart, periodEnd) = GetResetPeriod(pattern, dateTime);

                    int maxFromDonHang = 0;
                    int maxFromTso = 0;

                    if (periodStart.HasValue && periodEnd.HasValue)
                    {
                        maxFromDonHang = await conn.ExecuteScalarAsync<int>(
                            "SELECT COALESCE(MAX(SOHD), 0) FROM TDONHANG WHERE CAST(NGAY AS DATE) >= @PStart AND CAST(NGAY AS DATE) <= @PEnd", 
                            new { PStart = periodStart.Value.Date, PEnd = periodEnd.Value.Date });

                        string tsoSql = "SELECT FIRST 1 ID, SO FROM TSOHOADON WHERE CAST(NGAY AS DATE) >= @PStart AND CAST(NGAY AS DATE) <= @PEnd ORDER BY TIMECREATED DESC";
                        var tsoRow = await conn.QueryFirstOrDefaultAsync(tsoSql, new { PStart = periodStart.Value.Date, PEnd = periodEnd.Value.Date });
                        if (tsoRow != null)
                        {
                            int.TryParse(tsoRow.SO?.ToString(), out maxFromTso);
                        }
                    }
                    else
                    {
                        maxFromDonHang = await conn.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(SOHD), 0) FROM TDONHANG");
                        string tsoSql = "SELECT FIRST 1 ID, SO FROM TSOHOADON ORDER BY TIMECREATED DESC";
                        var tsoRow = await conn.QueryFirstOrDefaultAsync(tsoSql);
                        if (tsoRow != null)
                        {
                            int.TryParse(tsoRow.SO?.ToString(), out maxFromTso);
                        }
                    }

                    int nextNumber = Math.Max(maxFromDonHang, maxFromTso) + 1;
                    return ApplyPattern(pattern, dateTime, nextNumber);
                }
            }
            catch
            {
                return ApplyPattern("(yy)(******)", dateTime, 1);
            }
        }

        public async Task<StartOrderResult> StartTableOrderAsync(string banId, DateTime startTime, int soKhach, string khachHangId, string ghiChu, string nhanVienId = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string orderId = Guid.NewGuid().ToString();
                    string pattern = await LocalCauHinhService.GetFormatPatternAsync("TSOHOADON", "(yy)(******)");
                    var (periodStart, periodEnd) = GetResetPeriod(pattern, startTime);

                    // Lấy số thứ tự lớn nhất từ TDONHANG và TSOHOADON
                    int maxFromDonHang = 0;
                    int maxFromTso = 0;
                    string tsoId = null;

                    if (periodStart.HasValue && periodEnd.HasValue)
                    {
                        maxFromDonHang = await conn.ExecuteScalarAsync<int>(
                            "SELECT COALESCE(MAX(SOHD), 0) FROM TDONHANG WHERE CAST(NGAY AS DATE) >= @PStart AND CAST(NGAY AS DATE) <= @PEnd", 
                            new { PStart = periodStart.Value.Date, PEnd = periodEnd.Value.Date });

                        string tsoSql = "SELECT FIRST 1 ID, SO FROM TSOHOADON WHERE CAST(NGAY AS DATE) >= @PStart AND CAST(NGAY AS DATE) <= @PEnd ORDER BY TIMECREATED DESC";
                        var tsoRow = await conn.QueryFirstOrDefaultAsync(tsoSql, new { PStart = periodStart.Value.Date, PEnd = periodEnd.Value.Date });
                        if (tsoRow != null)
                        {
                            tsoId = tsoRow.ID?.ToString();
                            int.TryParse(tsoRow.SO?.ToString(), out maxFromTso);
                        }
                    }
                    else
                    {
                        maxFromDonHang = await conn.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(SOHD), 0) FROM TDONHANG");
                        string tsoSql = "SELECT FIRST 1 ID, SO FROM TSOHOADON ORDER BY TIMECREATED DESC";
                        var tsoRow = await conn.QueryFirstOrDefaultAsync(tsoSql);
                        if (tsoRow != null)
                        {
                            tsoId = tsoRow.ID?.ToString();
                            int.TryParse(tsoRow.SO?.ToString(), out maxFromTso);
                        }
                    }

                    int nextSo = Math.Max(maxFromDonHang, maxFromTso) + 1;
                    string soPhieu = ApplyPattern(pattern, startTime, nextSo);

                    int userCreatedId = 1;
                    if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUserId))
                    {
                        userCreatedId = parsedUserId;
                    }

                    // Cập nhật hoặc thêm mới vào TSOHOADON
                    DateTime saveNgay = periodStart ?? new DateTime(startTime.Year, startTime.Month, 1);
                    if (!string.IsNullOrEmpty(tsoId))
                    {
                        string updateTsoSql = "UPDATE TSOHOADON SET SO = @So, NAME = @Name, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = @UserId WHERE CAST(ID AS VARCHAR(50)) = @TsoId";
                        await conn.ExecuteAsync(updateTsoSql, new { So = nextSo.ToString(), Name = soPhieu, UserId = userCreatedId, TsoId = tsoId });
                    }
                    else
                    {
                        string insertTsoSql = @"
                            INSERT INTO TSOHOADON (
                                ID, NAME, NGAY, SO, STATUS, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @Name, @Ngay, @So, 1, @UserId, CURRENT_TIMESTAMP
                            )";
                        await conn.ExecuteAsync(insertTsoSql, new { 
                            Id = Guid.NewGuid().ToString(), 
                            Name = soPhieu,
                            Ngay = saveNgay, 
                            So = nextSo.ToString(), 
                            UserId = userCreatedId 
                        });
                    }

                    // Tạo mới đơn hàng trong TDONHANG
                    string insertSql = @"
                        INSERT INTO TDONHANG (
                            ID, NAME, SOHD, SOTT, DBANID, BATDAU, NGAY, SOKHACH, DKHACHHANGID, DNHANVIENXUATID, NOTE, STATUS, USERCREATEDID, TIMECREATED
                        ) VALUES (
                            @Id, @SoPhieu, @SoHd, @SoHd, @DbanId, @BatDau, @Ngay, @SoKhach, @KhachHangId, @NhanVienId, @Note, 1, @UserCreatedId, CURRENT_TIMESTAMP
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
                        NhanVienId = !string.IsNullOrEmpty(nhanVienId) ? nhanVienId : null,
                        Note = ghiChu,
                        UserCreatedId = userCreatedId
                    });

                    // Ghi lưu vết mở bàn
                    string banName = await conn.QueryFirstOrDefaultAsync<string>(
                        "SELECT NAME FROM DBAN WHERE CAST(ID AS VARCHAR(50)) = @BanId", new { BanId = banId });
                    _ = LocalLuuVetService.GhiLuuVetAsync(orderId, banName, "Sử dụng dịch vụ", $"Mở hóa đơn trên bàn '{banName}'", 0);

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
                               c.DONGIA as DonGia, COALESCE(m.GIABAN, c.DONGIA) as DonGiaGoc, c.TILEGIAMGIA as ChietKhauPhanTram, 
                               c.THANHTIEN as ThanhTien, c.NOTE as GhiChu,
                               m.DNHOMMATHANGID as NhomMatHangId,
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

        public async Task<bool> SaveOrderAsync(string orderId, List<PosDonHangChiTietViewModel> items, decimal tienHang, decimal giamGia, decimal tongCong, string ghiChu, int soKhach, decimal tienThue = 0, decimal thueSuatPt = 0, decimal tienPhiDichVu = 0, decimal phiDichVuPt = 0, string nhanVienId = null)
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
                                TIENTHUE = @TienThue, TILETHUE = @ThueSuatPt, PHIDICHVU = @TienPhiDichVu,
                                NOTE = @Note, SOKHACH = @SoKhach,
                                DNHANVIENXUATID = COALESCE(@NhanVienId, DNHANVIENXUATID),
                                USERMODIFIEDID = @UserModifiedId, TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(updateOrderSql, new
                        {
                            Id = orderId,
                            TienHang = tienHang,
                            GiamGia = giamGia,
                            TongCong = tongCong,
                            TienThue = tienThue,
                            ThueSuatPt = thueSuatPt,
                            TienPhiDichVu = tienPhiDichVu,
                            Note = ghiChu,
                            SoKhach = soKhach.ToString(),
                            NhanVienId = !string.IsNullOrEmpty(nhanVienId) ? nhanVienId : null,
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

        public async Task<bool> FinishTableOrderWithDetailsAsync(
            string orderId, 
            decimal khachDua, 
            decimal traLai, 
            decimal theATM, 
            decimal theTraTruoc, 
            string loaiThanhToan,
            decimal chuyenKhoan = 0,
            decimal voucher = 0,
            decimal diemGiam = 0,
            decimal truTichLuy = 0,
            decimal tamUng = 0)
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

                    var orderInfo = await conn.QueryFirstOrDefaultAsync(
                        "SELECT h.NAME as SoPhieu, h.TONGCONG as TongCong, h.NGAY as Ngay, b.NAME as BanName, h.DKHACHHANGID as DkhachhangId FROM TDONHANG h LEFT JOIN DBAN b ON h.DBANID = b.ID WHERE CAST(h.ID AS VARCHAR(50)) = @OrderId",
                        new { OrderId = orderId });

                    decimal tongCong = orderInfo?.TONGCONG != null ? Convert.ToDecimal(orderInfo.TONGCONG) : 0;
                    // Quy tắc: 20 000đ sẽ được 1 điểm
                    int diemTichLuy = (int)(tongCong / 20000m);

                    decimal tienMatThuc = loaiTtInt == 4 ? 0 : Math.Max(0, khachDua - traLai);

                    string sql = @"
                        UPDATE TDONHANG 
                        SET KETTHUC = CURRENT_TIMESTAMP, 
                            GIOTHANHTOAN = CURRENT_TIMESTAMP, 
                            STATUS = 2,
                            KHACHDUA = @KhachDua,
                            TRALAI = @TraLai,
                            TIENMAT = @TienMat,
                            THE = @TheATM,
                            CHUYENKHOAN = @ChuyenKhoan,
                            THETRATRUOC = @TheTraTruoc,
                            VOUCHER = @Voucher,
                            DIEMGIAM = @DiemGiam,
                            TRUTICHLUY = @TruTichLuy,
                            DATTRUOC = @DatTruoc,
                            LOAITHANHTOAN = @LoaiTtInt,
                            DIEM = @Diem
                        WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    await conn.ExecuteAsync(sql, new { 
                        OrderId = orderId, 
                        KhachDua = khachDua.ToString("0.##"),
                        TraLai = traLai.ToString("0.##"),
                        TienMat = tienMatThuc.ToString("0.##"),
                        TheATM = theATM.ToString("0.##"),
                        ChuyenKhoan = chuyenKhoan.ToString("0.##"),
                        TheTraTruoc = theTraTruoc.ToString("0.##"),
                        Voucher = voucher.ToString("0.##"),
                        DiemGiam = diemGiam.ToString("0.##"),
                        TruTichLuy = truTichLuy.ToString("0.##"),
                        DatTruoc = tamUng.ToString("0.##"),
                        LoaiTtInt = loaiTtInt,
                        Diem = diemTichLuy
                    });

                    if (orderInfo != null)
                    {
                        string soPhieu = orderInfo.SOPHIEU?.ToString() ?? "";
                        DateTime ngay = orderInfo.NGAY != null ? Convert.ToDateTime(orderInfo.NGAY) : DateTime.Today;
                        string banName = orderInfo.BANNAME?.ToString() ?? "";
                        _ = LocalLuuVetService.GhiLuuVetAsync(orderId, banName, "Sử dụng dịch vụ", $"Đóng hóa đơn (Có in), số phiếu:{soPhieu}, ngày: {ngay:dd/MM/yyyy}, tích lũy: {diemTichLuy} điểm", 1, 0, 0, tongCong);
                    }

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
                        string userId = "4f1466a0-0756-4ba9-afa8-053b96ca7569";
                        string userName = "Administrator";
                        if (SessionContext.CurrentUser != null)
                        {
                            if (!string.IsNullOrEmpty(SessionContext.CurrentUser.Id))
                                userId = SessionContext.CurrentUser.Id;
                            if (!string.IsNullOrEmpty(SessionContext.CurrentUser.TenDangNhap))
                                userName = SessionContext.CurrentUser.TenDangNhap;
                        }

                        // 1. Get original order info
                        string queryOrder = @"
                            SELECT 
                                h.NAME as SoPhieu,
                                h.NOTE as GhiChu,
                                h.NGAY as Ngay,
                                h.BATDAU as BatDau,
                                h.GIOTHANHTOAN as GioThanhToan,
                                COALESCE(h.TIENHANG, 0) as TienHang,
                                COALESCE(h.TIENTHUE, 0) as TienThue,
                                COALESCE(h.TILETHUE, 0) as TiLeThue,
                                COALESCE(h.TIENGIAMGIA, 0) as TienGiamGia,
                                COALESCE(h.TILEGIAMGIA, 0) as TiLeGiamGia,
                                COALESCE(h.TRALAI, 0) as TraLai,
                                COALESCE(h.PHIVANCHUYEN, 0) as PhiVanChuyen,
                                COALESCE(h.TIENGIO, 0) as TienGio,
                                COALESCE(h.PHIDICHVU, 0) as PhiDichVu,
                                k.NAME as KhachHang,
                                b.NAME as BanName,
                                u.NAME as NhanVien
                            FROM TDONHANG h
                            LEFT JOIN DBAN b ON h.DBANID = b.ID
                            LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                            LEFT JOIN SUSER u ON CAST(h.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                            WHERE CAST(h.ID AS VARCHAR(50)) = @OrderId";

                        var ord = await conn.QueryFirstOrDefaultAsync(queryOrder, new { OrderId = orderId }, trans);

                        string newHuyId = Guid.NewGuid().ToString();
                        string soPhieu = ord?.SOPHIEU?.ToString() ?? "";
                        string banName = ord?.BANNAME?.ToString() ?? "";
                        string khachHang = ord?.KHACHHANG?.ToString() ?? "";
                        string nhanVien = ord?.NHANVIEN?.ToString() ?? userName;
                        DateTime ngay = ord?.NGAY != null ? Convert.ToDateTime(ord.NGAY) : DateTime.Today;
                        decimal tienHang = ord?.TIENHANG != null ? Convert.ToDecimal(ord.TIENHANG) : 0;
                        decimal tienThue = ord?.TIENTHUE != null ? Convert.ToDecimal(ord.TIENTHUE) : 0;
                        decimal tiLeThue = ord?.TILETHUE != null ? Convert.ToDecimal(ord.TILETHUE) : 0;
                        decimal tienGiamGia = ord?.TIENGIAMGIA != null ? Convert.ToDecimal(ord.TIENGIAMGIA) : 0;
                        decimal tiLeGiamGia = ord?.TILEGIAMGIA != null ? Convert.ToDecimal(ord.TILEGIAMGIA) : 0;
                        decimal traLai = ord?.TRALAI != null ? Convert.ToDecimal(ord.TRALAI) : 0;
                        decimal phiVanChuyen = ord?.PHIVANCHUYEN != null ? Convert.ToDecimal(ord.PHIVANCHUYEN) : 0;
                        decimal tienGio = ord?.TIENGIO != null ? Convert.ToDecimal(ord.TIENGIO) : 0;
                        decimal phiDichVu = ord?.PHIDICHVU != null ? Convert.ToDecimal(ord.PHIDICHVU) : 0;

                        // 2. Insert into TDONHANGHUY
                        string insertHuySql = @"
                            INSERT INTO TDONHANGHUY (
                                ID, NAME, NOTE, STATUS, TIMECREATED, NGAY, USERCREATEDID,
                                KHACHHANG, NHANVEN, THUNGAN, DOITRA, DATHANHTOAN, GIOTHANHTOAN,
                                TRALAI, TIENHANG, TILETHUE, TIENTHUE, TILEGIAMGIA, TIENGIAMGIA,
                                PHIVANCHUYEN, THANHTOANBOI, NGAYHUY, GIOHUY, TDONHANGID, LYDOHUY,
                                TIENGIO, PHIDICHVU, BAN
                            ) VALUES (
                                @Id, @Name, @Note, 30, CURRENT_TIMESTAMP, @Ngay, @UserCreatedId,
                                @KhachHang, @NhanVien, @ThuNgan, 0, 0, NULL,
                                @TraLai, @TienHang, @TiLeThue, @TienThue, @TiLeGiamGia, @TienGiamGia,
                                @PhiVanChuyen, @ThanhToanBoi, CURRENT_DATE, CURRENT_TIMESTAMP, @OriginalOrderId, @LyDoHuy,
                                @TienGio, @PhiDichVu, @Ban
                            )";

                        await conn.ExecuteAsync(insertHuySql, new
                        {
                            Id = newHuyId,
                            Name = soPhieu,
                            Note = ord?.GHICHU?.ToString() ?? "",
                            Ngay = ngay.Date,
                            UserCreatedId = userId,
                            KhachHang = khachHang,
                            NhanVien = nhanVien,
                            ThuNgan = userName,
                            TraLai = traLai,
                            TienHang = tienHang,
                            TiLeThue = tiLeThue,
                            TienThue = tienThue,
                            TiLeGiamGia = tiLeGiamGia,
                            TienGiamGia = tienGiamGia,
                            PhiVanChuyen = phiVanChuyen,
                            ThanhToanBoi = userName,
                            OriginalOrderId = orderId,
                            LyDoHuy = lyDoHuy,
                            TienGio = tienGio,
                            PhiDichVu = phiDichVu,
                            Ban = banName
                        }, trans);

                        // 3. Query all order details from TDONHANGCHITIET
                        string queryDetails = @"
                            SELECT 
                                c.NOTE as GhiChu,
                                COALESCE(m.CODE, '') as MaHang,
                                COALESCE(c.TENHANG, m.NAME) as TenHang,
                                COALESCE(dvt.NAME, 'đĩa') as Dvt,
                                COALESCE(c.DONGIA, 0) as DonGia,
                                COALESCE(c.THANHTIEN, 0) as ThanhTien,
                                COALESCE(c.SLXUAT, c.SLNHAP, 1) as SoLuong
                            FROM TDONHANGCHITIET c
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                            WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @OrderId";

                        var details = (await conn.QueryAsync(queryDetails, new { OrderId = orderId }, trans)).ToList();

                        string insertHuyDetailSql = @"
                            INSERT INTO TDONHANGHUYCHITIET (
                                ID, NOTE, TDONHANGHUYID, STATUS, TIMECREATED, USERCREATEDID,
                                MAHANG, TENHANG, DVT, DONGIA, THANHTIEN, SOLUONG
                            ) VALUES (
                                @Id, @Note, @TdonhangHuyId, 30, CURRENT_TIMESTAMP, @UserCreatedId,
                                @MaHang, @TenHang, @Dvt, @DonGia, @ThanhTien, @SoLuong
                            )";

                        foreach (var d in details)
                        {
                            await conn.ExecuteAsync(insertHuyDetailSql, new
                            {
                                Id = Guid.NewGuid().ToString(),
                                Note = d.GHICHU?.ToString() ?? "",
                                TdonhangHuyId = newHuyId,
                                UserCreatedId = userId,
                                MaHang = d.MAHANG?.ToString() ?? "",
                                TenHang = d.TENHANG?.ToString() ?? "",
                                Dvt = d.DVT?.ToString() ?? "",
                                DonGia = Convert.ToDecimal(d.DONGIA),
                                ThanhTien = Convert.ToDecimal(d.THANHTIEN),
                                SoLuong = Convert.ToDecimal(d.SOLUONG)
                            }, trans);
                        }

                        // 4. Update TDONHANG and TDONHANGCHITIET
                        string noteSuffix = string.IsNullOrWhiteSpace(lyDoHuy) ? " [Đã hủy]" : $" [Hủy: {lyDoHuy}]";
                        string sqlOrder = @"
                            UPDATE TDONHANG 
                            SET STATUS = 0, 
                                KETTHUC = CURRENT_TIMESTAMP, 
                                TIMEMODIFIED = CURRENT_TIMESTAMP,
                                NOTE = COALESCE(NOTE, '') || @NoteSuffix
                            WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                        await conn.ExecuteAsync(sqlOrder, new { OrderId = orderId, NoteSuffix = noteSuffix }, trans);

                        string sqlDetails = @"
                            UPDATE TDONHANGCHITIET 
                            SET STATUS = 0, 
                                TIMEMODIFIED = CURRENT_TIMESTAMP 
                            WHERE CAST(TDONHANGID AS VARCHAR(50)) = @OrderId";
                        await conn.ExecuteAsync(sqlDetails, new { OrderId = orderId }, trans);

                        trans.Commit();

                        // 5. Ghi lưu vết hủy bill
                        _ = LocalLuuVetService.GhiLuuVetAsync(orderId, banName, "Sử dụng dịch vụ", $"Hủy hóa đơn số phiếu: {soPhieu}, lý do: {lyDoHuy}", 1);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hủy hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    var info = await conn.QueryFirstOrDefaultAsync(
                        "SELECT b1.NAME as OldBan, b2.NAME as NewBan FROM TDONHANG h LEFT JOIN DBAN b1 ON h.DBANID = b1.ID, DBAN b2 WHERE CAST(h.ID AS VARCHAR(50)) = @OrderId AND CAST(b2.ID AS VARCHAR(50)) = @NewBanId",
                        new { OrderId = orderId, NewBanId = newBanId });

                    string sql = @"
                        UPDATE TDONHANG 
                        SET DBANID = @BanId, TIMEMODIFIED = CURRENT_TIMESTAMP
                        WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    int rows = await conn.ExecuteAsync(sql, new { BanId = newBanId, OrderId = orderId });

                    if (rows > 0 && info != null)
                    {
                        string oldBan = info.OLDBAN?.ToString() ?? "";
                        string newBan = info.NEWBAN?.ToString() ?? "";
                        _ = LocalLuuVetService.GhiLuuVetAsync(orderId, newBan, "Sử dụng dịch vụ", $"Chuyển bàn từ '{oldBan}' sang '{newBan}'", 2);
                    }

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

        public async Task<List<NhanVienLookupViewModel>> GetNhanVienLookupAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT CAST(ID AS VARCHAR(50)) as Id,
                               CODE as Code,
                               NAME as Name,
                               DIENTHOAI as Dienthoai
                        FROM DNHANVIEN
                        WHERE (STATUS <> 0 OR STATUS IS NULL) AND (ITEMTYPE IS NULL OR UPPER(ITEMTYPE) <> 'FOLDER')
                        ORDER BY SORTORDER, NAME";
                    var items = (await conn.QueryAsync<NhanVienLookupViewModel>(sql)).ToList();
                    return items;
                }
            }
            catch
            {
                return new List<NhanVienLookupViewModel>();
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

        public async Task<bool> UpdateOrderCustomerAsync(string orderId, string khachHangId, string chucNang = "Sử dụng dịch vụ")
        {
            if (string.IsNullOrEmpty(orderId)) return false;
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE TDONHANG SET DKHACHHANGID = @KhachHangId WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    await conn.ExecuteAsync(sql, new { KhachHangId = khachHangId, OrderId = orderId });

                    string khName = await conn.QueryFirstOrDefaultAsync<string>(
                        "SELECT NAME FROM DKHACHHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = khachHangId });
                    if (!string.IsNullOrEmpty(khName))
                    {
                        _ = LocalLuuVetService.GhiLuuVetAsync(orderId, null, chucNang, $"Đặt khách hàng '{khName}'", 3);
                    }

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
                    var rootAll = new PosNhomMatHangViewModel { Id = string.Empty, Name = "Tất cả", ParentId = null, Icon = "🌐", IsExpanded = true };
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
                            m.DNHOMMATHANGID as NhomMatHangId,
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
