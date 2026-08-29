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

        public async Task<string> StartTableOrderAsync(string banId, DateTime startTime, int soKhach, string khachHangId, string ghiChu)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string orderId = Guid.NewGuid().ToString();
                    
                    // Tạo số phiếu HDxx/xxxxx
                    string countSql = "SELECT COUNT(*) FROM TDONHANG WHERE CAST(NGAY AS DATE) = @Today";
                    int countToday = await conn.ExecuteScalarAsync<int>(countSql, new { Today = startTime.Date }) + 1;
                    string soPhieu = $"HD{startTime:yy}/{countToday:D5}";

                    int userCreatedId = 1;
                    if (SessionContext.CurrentUser != null && int.TryParse(SessionContext.CurrentUser.Id, out int parsedUserId))
                    {
                        userCreatedId = parsedUserId;
                    }

                    string insertSql = @"
                        INSERT INTO TDONHANG (
                            ID, NAME, DBANID, BATDAU, NGAY, SOKHACH, DKHACHHANGID, NOTE, STATUS, USERCREATEDID, TIMECREATED
                        ) VALUES (
                            @Id, @SoPhieu, @DbanId, @BatDau, @Ngay, @SoKhach, @KhachHangId, @Note, 1, @UserCreatedId, CURRENT_TIMESTAMP
                        )";

                    await conn.ExecuteAsync(insertSql, new
                    {
                        Id = orderId,
                        SoPhieu = soPhieu,
                        DbanId = banId,
                        BatDau = startTime,
                        Ngay = startTime.Date,
                        SoKhach = soKhach.ToString(),
                        KhachHangId = !string.IsNullOrEmpty(khachHangId) ? khachHangId : null,
                        Note = ghiChu,
                        UserCreatedId = userCreatedId
                    });

                    return orderId;
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

        public async Task<bool> FinishTableOrderAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) return false;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE TDONHANG SET KETTHUC = CURRENT_TIMESTAMP, STATUS = 2 WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                    await conn.ExecuteAsync(sql, new { OrderId = orderId });
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết thúc bàn: " + ex.Message);
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
    }
}
