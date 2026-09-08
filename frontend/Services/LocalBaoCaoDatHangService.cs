using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalBaoCaoDatHangService
    {
        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        #region View Models
        public class BaoCaoDanhSachDatHangTheoNgayItem
        {
            public int STT { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
            public string SoPhieu { get; set; } = "";
            public string KhachHang { get; set; } = "";
            public string DiaChi { get; set; } = "";
            public string DienThoai { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCong { get; set; }
            public string NhanVien { get; set; } = "";
            public string NhomKhachHangId { get; set; } = "";
        }

        public class BaoCaoDanhSachDatHangTheoKhachHangItem
        {
            public int STT { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
            public string SoPhieu { get; set; } = "";
            public string KhachHang { get; set; } = "";
            public string MaKhach { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCong { get; set; }
            public string NhanVien { get; set; } = "";
        }

        public class BaoCaoTongHopDatHangTheoNgayItem
        {
            public int STT { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCong { get; set; }
        }

        public class BaoCaoTongHopDatHangTheoKhachHangItem
        {
            public int STT { get; set; }
            public string MaKhach { get; set; } = "";
            public string TenKhachHang { get; set; } = "";
            public string DiaChi { get; set; } = "";
            public string DienThoai { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCong { get; set; }
        }

        public class BaoCaoTongHopMatHangDatTheoKhachHangItem
        {
            public int STT { get; set; }
            public string KhachHang { get; set; } = "";
            public string TenMatHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public string MatHangId { get; set; } = "";
            public string NhomMatHangId { get; set; } = "";
            public string NhanVien { get; set; } = "";
        }

        public class BaoCaoTongHopMatHangDatTheoNgayItem
        {
            public int STT { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
            public string TenMatHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public string KhachHang { get; set; } = "";
            public string MatHangId { get; set; } = "";
            public string NhomMatHangId { get; set; } = "";
        }
        #endregion

        #region Lookups
        public async Task<List<FilterComboItem>> GetKhachHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👥" } };
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DKHACHHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME";
                    var items = await conn.QueryAsync<dynamic>(sql);
                    foreach (var it in items)
                    {
                        list.Add(new FilterComboItem { Id = it.ID?.ToString() ?? "", Name = it.NAME?.ToString() ?? "", Icon = "👥" });
                    }
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetNhomKhachHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "📁" } };
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DNHOMKHACHHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME";
                    var items = await conn.QueryAsync<dynamic>(sql);
                    foreach (var it in items)
                    {
                        list.Add(new FilterComboItem { Id = it.ID?.ToString() ?? "", Name = it.NAME?.ToString() ?? "", Icon = "📁" });
                    }
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetNhanVienFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "👤" } };
            try
            {
                var dbNv = await LocalNhanVienService.GetNhanVienFlatListAsync();
                if (dbNv != null)
                {
                    foreach (var n in dbNv)
                    {
                        list.Add(new FilterComboItem { Id = n.Id ?? "", Name = n.Name ?? "", Icon = "👤" });
                    }
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetNhomMatHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "📁" } };
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DNHOMMATHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME";
                    var items = await conn.QueryAsync<dynamic>(sql);
                    foreach (var it in items)
                    {
                        list.Add(new FilterComboItem { Id = it.ID?.ToString() ?? "", Name = it.NAME?.ToString() ?? "", Icon = "📁" });
                    }
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetMatHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "🏷️" } };
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name, CODE as Code FROM DMATHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME";
                    var items = await conn.QueryAsync<dynamic>(sql);
                    foreach (var it in items)
                    {
                        string name = it.NAME?.ToString() ?? "";
                        string code = it.CODE?.ToString() ?? "";
                        string display = !string.IsNullOrEmpty(code) ? $"{code} - {name}" : name;
                        list.Add(new FilterComboItem { Id = it.ID?.ToString() ?? "", Name = display, Icon = "🏷️" });
                    }
                }
            }
            catch { }
            return list;
        }
        #endregion

        #region Query Reports

        // 1. DANH SÁCH ĐẶT HÀNG THEO NGÀY
        public async Task<List<BaoCaoDanhSachDatHangTheoNgayItem>> GetDanhSachDatHangTheoNgayAsync(
            DateTime tuNgay, DateTime denNgay, string khachHangId = "", string nhomKhachHangId = "", string nhanVienId = "")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            d.NGAY as Ngay,
                            d.NAME as SoPhieu,
                            COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ') as KhachHang,
                            COALESCE(kh.DIACHI, d.DIACHI, '') as DiaChi,
                            COALESCE(kh.DIENTHOAI, d.DIENTHOAI, '') as DienThoai,
                            COALESCE(d.TIENHANG, 0) as TienHang,
                            COALESCE(d.TIENGIAMGIA, 0) as GiamGia,
                            COALESCE(d.TONGCONG, 0) as TongCong,
                            COALESCE(nv.NAME, '') as NhanVien,
                            CAST(kh.DNHOMKHACHHANGID AS VARCHAR(50)) as NhomKhachHangId
                        FROM TDATHANG d
                        LEFT JOIN DKHACHHANG kh ON d.DKHACHHANGID = kh.ID
                        LEFT JOIN DNHANVIEN nv ON d.USERCREATEDID = nv.ID OR d.DNHANVIENID = nv.ID
                        WHERE (d.STATUS <> 0 OR d.STATUS IS NULL)
                          AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay";

                    var p = new DynamicParameters();
                    p.Add("TuNgay", tuNgay.Date);
                    p.Add("DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        p.Add("KhachHangId", khachHangId);
                    }
                    if (!string.IsNullOrEmpty(nhomKhachHangId))
                    {
                        sql += " AND CAST(kh.DNHOMKHACHHANGID AS VARCHAR(50)) = @NhomKhachHangId";
                        p.Add("NhomKhachHangId", nhomKhachHangId);
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(d.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(d.DNHANVIENID AS VARCHAR(50)) = @NhanVienId)";
                        p.Add("NhanVienId", nhanVienId);
                    }

                    sql += " ORDER BY d.NGAY DESC, d.NAME";

                    var rows = (await conn.QueryAsync<BaoCaoDanhSachDatHangTheoNgayItem>(sql, p)).ToList();
                    return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDanhSachDatHangTheoNgayAsync: {ex.Message}");
                return new List<BaoCaoDanhSachDatHangTheoNgayItem>();
            }
        }

        // 2. DANH SÁCH ĐẶT HÀNG THEO KHÁCH HÀNG
        public async Task<List<BaoCaoDanhSachDatHangTheoKhachHangItem>> GetDanhSachDatHangTheoKhachHangAsync(
            DateTime tuNgay, DateTime denNgay, string khachHangId = "", string nhanVienId = "")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            d.NGAY as Ngay,
                            d.NAME as SoPhieu,
                            COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ') as KhachHang,
                            COALESCE(kh.MAKHACH, kh.CODE, '') as MaKhach,
                            COALESCE(d.TIENHANG, 0) as TienHang,
                            COALESCE(d.TIENGIAMGIA, 0) as GiamGia,
                            COALESCE(d.TONGCONG, 0) as TongCong,
                            COALESCE(nv.NAME, '') as NhanVien
                        FROM TDATHANG d
                        LEFT JOIN DKHACHHANG kh ON d.DKHACHHANGID = kh.ID
                        LEFT JOIN DNHANVIEN nv ON d.USERCREATEDID = nv.ID OR d.DNHANVIENID = nv.ID
                        WHERE (d.STATUS <> 0 OR d.STATUS IS NULL)
                          AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay";

                    var p = new DynamicParameters();
                    p.Add("TuNgay", tuNgay.Date);
                    p.Add("DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        p.Add("KhachHangId", khachHangId);
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(d.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(d.DNHANVIENID AS VARCHAR(50)) = @NhanVienId)";
                        p.Add("NhanVienId", nhanVienId);
                    }

                    sql += " ORDER BY KhachHang, d.NGAY DESC, d.NAME";

                    var rows = (await conn.QueryAsync<BaoCaoDanhSachDatHangTheoKhachHangItem>(sql, p)).ToList();
                    return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDanhSachDatHangTheoKhachHangAsync: {ex.Message}");
                return new List<BaoCaoDanhSachDatHangTheoKhachHangItem>();
            }
        }

        // 3. TỔNG HỢP ĐẶT HÀNG THEO NGÀY
        public async Task<List<BaoCaoTongHopDatHangTheoNgayItem>> GetTongHopDatHangTheoNgayAsync(
            DateTime tuNgay, DateTime denNgay, string khachHangId = "")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(d.NGAY AS DATE) as Ngay,
                            SUM(COALESCE(d.TIENHANG, 0)) as TienHang,
                            SUM(COALESCE(d.TIENGIAMGIA, 0)) as GiamGia,
                            SUM(COALESCE(d.TONGCONG, 0)) as TongCong
                        FROM TDATHANG d
                        WHERE (d.STATUS <> 0 OR d.STATUS IS NULL)
                          AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay";

                    var p = new DynamicParameters();
                    p.Add("TuNgay", tuNgay.Date);
                    p.Add("DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        p.Add("KhachHangId", khachHangId);
                    }

                    sql += " GROUP BY CAST(d.NGAY AS DATE) ORDER BY CAST(d.NGAY AS DATE)";

                    var rows = (await conn.QueryAsync<BaoCaoTongHopDatHangTheoNgayItem>(sql, p)).ToList();
                    return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetTongHopDatHangTheoNgayAsync: {ex.Message}");
                return new List<BaoCaoTongHopDatHangTheoNgayItem>();
            }
        }

        // 4. TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG
        public async Task<List<BaoCaoTongHopDatHangTheoKhachHangItem>> GetTongHopDatHangTheoKhachHangAsync(
            DateTime tuNgay, DateTime denNgay, string khachHangId = "")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kh.MAKHACH, kh.CODE, '') as MaKhach,
                            COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ') as TenKhachHang,
                            COALESCE(kh.DIACHI, d.DIACHI, '') as DiaChi,
                            COALESCE(kh.DIENTHOAI, d.DIENTHOAI, '') as DienThoai,
                            SUM(COALESCE(d.TIENHANG, 0)) as TienHang,
                            SUM(COALESCE(d.TIENGIAMGIA, 0)) as GiamGia,
                            SUM(COALESCE(d.TONGCONG, 0)) as TongCong
                        FROM TDATHANG d
                        LEFT JOIN DKHACHHANG kh ON d.DKHACHHANGID = kh.ID
                        WHERE (d.STATUS <> 0 OR d.STATUS IS NULL)
                          AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay";

                    var p = new DynamicParameters();
                    p.Add("TuNgay", tuNgay.Date);
                    p.Add("DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        p.Add("KhachHangId", khachHangId);
                    }

                    sql += @" GROUP BY COALESCE(kh.MAKHACH, kh.CODE, ''), 
                                      COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ'),
                                      COALESCE(kh.DIACHI, d.DIACHI, ''), 
                                      COALESCE(kh.DIENTHOAI, d.DIENTHOAI, '')
                              ORDER BY TenKhachHang";

                    var rows = (await conn.QueryAsync<BaoCaoTongHopDatHangTheoKhachHangItem>(sql, p)).ToList();
                    return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetTongHopDatHangTheoKhachHangAsync: {ex.Message}");
                return new List<BaoCaoTongHopDatHangTheoKhachHangItem>();
            }
        }

        // 5. TỔNG HỢP MẶT HÀNG ĐẶT THEO KHÁCH HÀNG
        public async Task<List<BaoCaoTongHopMatHangDatTheoKhachHangItem>> GetTongHopMatHangDatTheoKhachHangAsync(
            DateTime tuNgay, DateTime denNgay, string khachHangId = "", string nhanVienId = "", string nhomMatHangId = "", string matHangId = "")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ') as KhachHang,
                            COALESCE(mh.NAME, N'Mặt hàng') as TenMatHang,
                            COALESCE(dvt.NAME, '') as DVT,
                            SUM(COALESCE(ct.SOLUONG, 1)) as SoLuong,
                            COALESCE(ct.DONGIA, 0) as DonGia,
                            SUM(COALESCE(ct.THANHTIEN, 0)) as ThanhTien,
                            CAST(ct.DMATHANGID AS VARCHAR(50)) as MatHangId,
                            CAST(mh.DNHOMMATHANGID AS VARCHAR(50)) as NhomMatHangId,
                            COALESCE(nv.NAME, '') as NhanVien
                        FROM TDATHANGCHITIET ct
                        JOIN TDATHANG d ON ct.TDATHANGID = d.ID
                        LEFT JOIN DKHACHHANG kh ON d.DKHACHHANGID = kh.ID
                        LEFT JOIN DMATHANG mh ON ct.DMATHANGID = mh.ID
                        LEFT JOIN DDONVITINH dvt ON mh.DDONVITINHID = dvt.ID
                        LEFT JOIN DNHANVIEN nv ON d.USERCREATEDID = nv.ID OR d.DNHANVIENID = nv.ID
                        WHERE (d.STATUS <> 0 OR d.STATUS IS NULL)
                          AND (ct.STATUS <> 0 OR ct.STATUS IS NULL)
                          AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay";

                    var p = new DynamicParameters();
                    p.Add("TuNgay", tuNgay.Date);
                    p.Add("DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        p.Add("KhachHangId", khachHangId);
                    }
                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(d.USERCREATEDID AS VARCHAR(50)) = @NhanVienId OR CAST(d.DNHANVIENID AS VARCHAR(50)) = @NhanVienId)";
                        p.Add("NhanVienId", nhanVienId);
                    }
                    if (!string.IsNullOrEmpty(nhomMatHangId))
                    {
                        sql += " AND CAST(mh.DNHOMMATHANGID AS VARCHAR(50)) = @NhomMatHangId";
                        p.Add("NhomMatHangId", nhomMatHangId);
                    }
                    if (!string.IsNullOrEmpty(matHangId))
                    {
                        sql += " AND CAST(ct.DMATHANGID AS VARCHAR(50)) = @MatHangId";
                        p.Add("MatHangId", matHangId);
                    }

                    sql += @" GROUP BY COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ'),
                                      COALESCE(mh.NAME, N'Mặt hàng'),
                                      COALESCE(dvt.NAME, ''),
                                      COALESCE(ct.DONGIA, 0),
                                      CAST(ct.DMATHANGID AS VARCHAR(50)),
                                      CAST(mh.DNHOMMATHANGID AS VARCHAR(50)),
                                      COALESCE(nv.NAME, '')
                              ORDER BY KhachHang, TenMatHang";

                    var rows = (await conn.QueryAsync<BaoCaoTongHopMatHangDatTheoKhachHangItem>(sql, p)).ToList();
                    return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetTongHopMatHangDatTheoKhachHangAsync: {ex.Message}");
                return new List<BaoCaoTongHopMatHangDatTheoKhachHangItem>();
            }
        }

        // 6. TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY
        public async Task<List<BaoCaoTongHopMatHangDatTheoNgayItem>> GetTongHopMatHangDatTheoNgayAsync(
            DateTime tuNgay, DateTime denNgay, string matHangId = "", string nhomMatHangId = "", string khachHangId = "")
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(d.NGAY AS DATE) as Ngay,
                            COALESCE(mh.NAME, N'Mặt hàng') as TenMatHang,
                            COALESCE(dvt.NAME, '') as DVT,
                            SUM(COALESCE(ct.SOLUONG, 1)) as SoLuong,
                            COALESCE(ct.DONGIA, 0) as DonGia,
                            SUM(COALESCE(ct.THANHTIEN, 0)) as ThanhTien,
                            COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ') as KhachHang,
                            CAST(ct.DMATHANGID AS VARCHAR(50)) as MatHangId,
                            CAST(mh.DNHOMMATHANGID AS VARCHAR(50)) as NhomMatHangId
                        FROM TDATHANGCHITIET ct
                        JOIN TDATHANG d ON ct.TDATHANGID = d.ID
                        LEFT JOIN DKHACHHANG kh ON d.DKHACHHANGID = kh.ID
                        LEFT JOIN DMATHANG mh ON ct.DMATHANGID = mh.ID
                        LEFT JOIN DDONVITINH dvt ON mh.DDONVITINHID = dvt.ID
                        WHERE (d.STATUS <> 0 OR d.STATUS IS NULL)
                          AND (ct.STATUS <> 0 OR ct.STATUS IS NULL)
                          AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay";

                    var p = new DynamicParameters();
                    p.Add("TuNgay", tuNgay.Date);
                    p.Add("DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(matHangId))
                    {
                        sql += " AND CAST(ct.DMATHANGID AS VARCHAR(50)) = @MatHangId";
                        p.Add("MatHangId", matHangId);
                    }
                    if (!string.IsNullOrEmpty(nhomMatHangId))
                    {
                        sql += " AND CAST(mh.DNHOMMATHANGID AS VARCHAR(50)) = @NhomMatHangId";
                        p.Add("NhomMatHangId", nhomMatHangId);
                    }
                    if (!string.IsNullOrEmpty(khachHangId))
                    {
                        sql += " AND CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId";
                        p.Add("KhachHangId", khachHangId);
                    }

                    sql += @" GROUP BY CAST(d.NGAY AS DATE),
                                      COALESCE(mh.NAME, N'Mặt hàng'),
                                      COALESCE(dvt.NAME, ''),
                                      COALESCE(ct.DONGIA, 0),
                                      COALESCE(kh.NAME, d.TENKHACH, N'Khách lẻ'),
                                      CAST(ct.DMATHANGID AS VARCHAR(50)),
                                      CAST(mh.DNHOMMATHANGID AS VARCHAR(50))
                              ORDER BY CAST(d.NGAY AS DATE) DESC, TenMatHang";

                    var rows = (await conn.QueryAsync<BaoCaoTongHopMatHangDatTheoNgayItem>(sql, p)).ToList();
                    return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetTongHopMatHangDatTheoNgayAsync: {ex.Message}");
                return new List<BaoCaoTongHopMatHangDatTheoNgayItem>();
            }
        }

        #endregion
    }
}
