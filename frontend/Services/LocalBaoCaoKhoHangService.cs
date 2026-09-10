using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class LocalBaoCaoKhoHangService
    {
        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
        }

        #region Master Lookups
        public async Task<List<FilterComboItem>> GetKhoHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "Tất cả" } };
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                var items = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DKHOHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                foreach (var i in items)
                {
                    list.Add(new FilterComboItem { Id = i.ID?.ToString() ?? "", Name = i.NAME?.ToString() ?? "" });
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetNhomMatHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "Tất cả" } };
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                var items = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DNHOMMATHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                foreach (var i in items)
                {
                    list.Add(new FilterComboItem { Id = i.ID?.ToString() ?? "", Name = i.NAME?.ToString() ?? "" });
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetMatHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "Tất cả" } };
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                var items = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DMATHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                foreach (var i in items)
                {
                    list.Add(new FilterComboItem { Id = i.ID?.ToString() ?? "", Name = i.NAME?.ToString() ?? "" });
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetNhaCungCapFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "Tất cả" } };
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                var items = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                foreach (var i in items)
                {
                    list.Add(new FilterComboItem { Id = i.ID?.ToString() ?? "", Name = i.NAME?.ToString() ?? "" });
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetNhanVienFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "Tất cả" } };
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                var items = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DNHANVIEN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                foreach (var i in items)
                {
                    list.Add(new FilterComboItem { Id = i.ID?.ToString() ?? "", Name = i.NAME?.ToString() ?? "" });
                }
            }
            catch { }
            return list;
        }

        public async Task<List<FilterComboItem>> GetKhachHangFilterAsync()
        {
            var list = new List<FilterComboItem> { new FilterComboItem { Id = "", Name = "Tất cả" } };
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                var items = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DKHACHHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                foreach (var i in items)
                {
                    list.Add(new FilterComboItem { Id = i.ID?.ToString() ?? "", Name = i.NAME?.ToString() ?? "" });
                }
            }
            catch { }
            return list;
        }
        #endregion

        #region Report Models

        public class PhieuKhoRowItem
        {
            public int STT { get; set; }
            public string Id { get; set; } = "";
            public string SoPhieu { get; set; } = "";
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string GroupKey { get; set; } = "";
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public string KhoXuat { get; set; } = "";
            public string KhoNhap { get; set; } = "";
            public string NhaCungCap { get; set; } = "";
            public string NhanVienXuat { get; set; } = "";
            public string NhanVienNhap { get; set; } = "";
            public string NhanVien { get; set; } = "";
            public string DienGiai { get; set; } = "";
            public string GhiChu { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal SoLuongNhap { get; set; }
            public decimal SoLuongXuat { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public decimal TienHang { get; set; }
            public decimal TienGiamGia { get; set; }
            public decimal TongCong { get; set; }
            public decimal SlThucTe { get; set; }
            public decimal SlHeThong { get; set; }
            public decimal ChenhLechTang { get; set; }
            public decimal ChenhLechGiam { get; set; }
            public DateTime? HanDung { get; set; }
            public string HanDungDisplay => HanDung?.ToString("dd/MM/yyyy") ?? "";
            public decimal Ton { get; set; }
            public decimal TonToiThieu { get; set; }
            public decimal TonToiDa { get; set; }
            public decimal GiaVon { get; set; }
            public decimal TriGiaVon { get; set; }
            public decimal GiaBan { get; set; }
            public decimal TriGiaBan { get; set; }
            public string DoiTuong { get; set; } = "";
        }

        public class XntRowItem
        {
            public int STT { get; set; }
            public string NhomHang { get; set; } = "";
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal GiaVon { get; set; }
            public decimal TonDauSl { get; set; }
            public decimal TonDauTriGia { get; set; }
            public decimal NhapSl { get; set; }
            public decimal NhapTriGia { get; set; }
            public decimal XuatSl { get; set; }
            public decimal XuatTriGia { get; set; }
            public decimal TonCuoiSl { get; set; }
            public decimal TonCuoiTriGia { get; set; }

            // Chi tiết
            public decimal NhapMua { get; set; }
            public decimal NhapDieuChinh { get; set; }
            public decimal NhapChuyenKho { get; set; }
            public decimal TongNhap => NhapMua + NhapDieuChinh + NhapChuyenKho;

            public decimal XuatBan { get; set; }
            public decimal XuatVatTu { get; set; }
            public decimal XuatDieuChinh { get; set; }
            public decimal XuatChuyenKho { get; set; }
            public decimal XuatKhac { get; set; }
            public decimal TongXuat => XuatBan + XuatVatTu + XuatDieuChinh + XuatChuyenKho + XuatKhac;
        }

        #endregion

        #region Query Methods

        // 1. BÁO CÁO XUẤT BÁN HÀNG
        public async Task<List<PhieuKhoRowItem>> GetXuatBanHangTheoNgayAsync(DateTime tuNgay, DateTime denNgay)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NGAY as Ngay,
                        COALESCE(c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) as SoLuong,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        SUM(COALESCE(c.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    WHERE (d.LOAI = 0 OR d.LOAI = 10 OR d.LOAI = 11)
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                    GROUP BY d.ID, d.NGAY, c.NAME, m.NAME, dv.NAME, c.DONGIA
                    ORDER BY d.NGAY, c.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        Ngay = ngay,
                        GroupKey = ngay?.ToString("dd/MM/yyyy") ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = r.SOLUONG != null ? Convert.ToDecimal(r.SOLUONG) : 0,
                        DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                        ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetXuatBanHangTheoNgayAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<List<PhieuKhoRowItem>> GetXuatBanHangTheoDonAsync(DateTime tuNgay, DateTime denNgay)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        COALESCE(d.NAME, CAST(d.SOHD AS VARCHAR(20)), '') as SoPhieu,
                        d.NGAY as Ngay,
                        COALESCE(c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        COALESCE(c.SLXUAT, c.SLNHAP, 1) as SoLuong,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        COALESCE(c.THANHTIEN, 0) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    WHERE (d.LOAI = 0 OR d.LOAI = 10 OR d.LOAI = 11)
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                    ORDER BY d.NGAY, d.NAME, c.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = ngay,
                        GroupKey = ngay?.ToString("dd/MM/yyyy") ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = r.SOLUONG != null ? Convert.ToDecimal(r.SOLUONG) : 0,
                        DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                        ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetXuatBanHangTheoDonAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<List<PhieuKhoRowItem>> GetXuatDinhLuongTheoDonAsync(DateTime tuNgay, DateTime denNgay)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // Lấy chi tiết định lượng vật tư xuất theo đơn
                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        COALESCE(d.NAME, CAST(d.SOHD AS VARCHAR(20)), '') as SoPhieu,
                        d.NGAY as Ngay,
                        COALESCE(vt.NAME, c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        COALESCE(c.SLXUAT, c.SLNHAP, 1) as SoLuong,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        COALESCE(c.THANHTIEN, 0) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG vt ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(vt.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID, vt.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    WHERE (d.LOAI = 0 OR d.LOAI = 10 OR d.LOAI = 11)
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                    ORDER BY d.NGAY, d.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = ngay,
                        GroupKey = r.SOPHIEU?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = r.SOLUONG != null ? Convert.ToDecimal(r.SOLUONG) : 0,
                        DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                        ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetXuatDinhLuongTheoDonAsync error: " + ex.Message);
            }
            return list;
        }

        // 2. BÁO CÁO KIỂM KÊ
        public async Task<List<PhieuKhoRowItem>> GetDanhSachPhieuKiemKeAsync(DateTime tuNgay, DateTime denNgay, string nhanVienId = null, string khoId = null, bool groupByNhanVien = false)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NAME as SoPhieu,
                        d.NGAY as Ngay,
                        COALESCE(d.NOTE, d.DIENGIAI, 'Kiểm kê') as DienGiai,
                        d.NOTE as GhiChu,
                        nv.NAME as NhanVien,
                        k.NAME as KhoNhap
                    FROM TDONHANG d
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50)) OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50)) OR CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    WHERE d.LOAI = 4
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NhanVienId IS NULL OR @NhanVienId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NhanVienId OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId)
                      AND (@KhoId IS NULL OR @KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoId)
                    ORDER BY d.NGAY DESC, d.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NhanVienId = nhanVienId, KhoId = khoId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string nv = r.NHANVIEN?.ToString() ?? "";
                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = ngay,
                        NhanVien = nv,
                        GroupKey = groupByNhanVien ? (string.IsNullOrEmpty(nv) ? "Chưa chọn nhân viên" : nv) : (ngay?.ToString("dd/MM/yyyy") ?? ""),
                        DienGiai = r.DIENGIAI?.ToString() ?? "Kiểm kê",
                        GhiChu = r.GHICHU?.ToString() ?? "",
                        KhoNhap = r.KHONHAP?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDanhSachPhieuKiemKeAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<List<PhieuKhoRowItem>> GetTongHopMatHangKiemKeAsync(DateTime tuNgay, DateTime denNgay, string nhomId = null, string mhId = null, string nhanVienId = null, string khoId = null, bool groupByNhanVien = false)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NGAY as Ngay,
                        nv.NAME as NhanVien,
                        m.CODE as MaHang,
                        COALESCE(c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        COALESCE(c.SLNHAP, 0) as SlThucTe,
                        COALESCE(c.SLXUAT, 0) as SlHeThong
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50)) OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    WHERE d.LOAI = 4
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NhomId IS NULL OR @NhomId = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@MhId IS NULL OR @MhId = '' OR CAST(m.ID AS VARCHAR(50)) = @MhId)
                      AND (@NhanVienId IS NULL OR @NhanVienId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NhanVienId OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId)
                      AND (@KhoId IS NULL OR @KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoId)
                    ORDER BY d.NGAY, m.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NhomId = nhomId, MhId = mhId, NhanVienId = nhanVienId, KhoId = khoId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string nv = r.NHANVIEN?.ToString() ?? "";
                    decimal tt = r.SLTHUCTE != null ? Convert.ToDecimal(r.SLTHUCTE) : 0;
                    decimal ht = r.SLHETHONG != null ? Convert.ToDecimal(r.SLHETHONG) : 0;
                    decimal diff = tt - ht;

                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        Ngay = ngay,
                        NhanVien = nv,
                        GroupKey = groupByNhanVien ? (string.IsNullOrEmpty(nv) ? "Chưa chọn nhân viên" : nv) : (ngay?.ToString("dd/MM/yyyy") ?? ""),
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SlThucTe = tt,
                        SlHeThong = ht,
                        ChenhLechTang = diff > 0 ? diff : 0,
                        ChenhLechGiam = diff < 0 ? -diff : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTongHopMatHangKiemKeAsync error: " + ex.Message);
            }
            return list;
        }

        // 3. BÁO CÁO XUẤT KHÁC
        public async Task<List<PhieuKhoRowItem>> GetDanhSachPhieuXuatKhoAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = null, string nhanVienXuatId = null, string khachHangId = null, bool groupByNhanVien = false)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NAME as SoPhieu,
                        d.NGAY as Ngay,
                        k.NAME as KhoXuat,
                        nv.NAME as NhanVienXuat,
                        COALESCE(d.TIENHANG, 0) as TienHang,
                        COALESCE(d.TIENGIAMGIA, 0) as TienGiamGia,
                        COALESCE(d.TONGCONG, 0) as TongCong,
                        d.NOTE as GhiChu
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                    WHERE d.LOAI = 2
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@KhoXuatId IS NULL OR @KhoXuatId = '' OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoXuatId)
                      AND (@NhanVienXuatId IS NULL OR @NhanVienXuatId = '' OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienXuatId)
                      AND (@KhachHangId IS NULL OR @KhachHangId = '' OR CAST(d.DKHACHHANGID AS VARCHAR(50)) = @KhachHangId)
                    ORDER BY d.NGAY DESC, d.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoXuatId = khoXuatId, NhanVienXuatId = nhanVienXuatId, KhachHangId = khachHangId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string nv = r.NHANVIENXUAT?.ToString() ?? "";
                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = ngay,
                        KhoXuat = r.KHOXUAT?.ToString() ?? "",
                        NhanVienXuat = nv,
                        GroupKey = groupByNhanVien ? (string.IsNullOrEmpty(nv) ? "Chưa chọn nhân viên" : nv) : (ngay?.ToString("dd/MM/yyyy") ?? ""),
                        TienHang = r.TIENHANG != null ? Convert.ToDecimal(r.TIENHANG) : 0,
                        TienGiamGia = r.TIENGIAMGIA != null ? Convert.ToDecimal(r.TIENGIAMGIA) : 0,
                        TongCong = r.TONGCONG != null ? Convert.ToDecimal(r.TONGCONG) : 0,
                        GhiChu = r.GHICHU?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDanhSachPhieuXuatKhoAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<List<PhieuKhoRowItem>> GetTongHopXuatKhacTheoNgayOrNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = null, string nhanVienXuatId = null, bool groupByNhanVien = false)
        {
            var rawList = await GetDanhSachPhieuXuatKhoAsync(tuNgay, denNgay, khoXuatId, nhanVienXuatId, null, groupByNhanVien);
            if (groupByNhanVien)
            {
                return rawList.GroupBy(x => x.NhanVienXuat)
                    .Select((g, idx) => new PhieuKhoRowItem
                    {
                        STT = idx + 1,
                        NhanVienXuat = string.IsNullOrEmpty(g.Key) ? "Chưa phân công" : g.Key,
                        TienHang = g.Sum(x => x.TienHang),
                        TienGiamGia = g.Sum(x => x.TienGiamGia),
                        TongCong = g.Sum(x => x.TongCong)
                    }).ToList();
            }
            else
            {
                return rawList.GroupBy(x => x.NgayDisplay)
                    .Select((g, idx) => new PhieuKhoRowItem
                    {
                        STT = idx + 1,
                        Ngay = g.First().Ngay,
                        TienHang = g.Sum(x => x.TienHang),
                        TienGiamGia = g.Sum(x => x.TienGiamGia),
                        TongCong = g.Sum(x => x.TongCong)
                    }).ToList();
            }
        }

        public async Task<List<PhieuKhoRowItem>> GetTongHopMatHangXuatAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = null, string nhanVienXuatId = null, string nccId = null, bool groupByNhanVien = false)
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NGAY as Ngay,
                        nv.NAME as NhanVienXuat,
                        m.CODE as MaHang,
                        COALESCE(c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) as SoLuong,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        SUM(COALESCE(c.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                    WHERE d.LOAI = 2
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@KhoXuatId IS NULL OR @KhoXuatId = '' OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoXuatId)
                      AND (@NhanVienXuatId IS NULL OR @NhanVienXuatId = '' OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienXuatId)
                      AND (@NccId IS NULL OR @NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                    GROUP BY d.ID, d.NGAY, nv.NAME, m.CODE, c.NAME, m.NAME, dv.NAME, c.DONGIA
                    ORDER BY d.NGAY, nv.NAME, m.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoXuatId = khoXuatId, NhanVienXuatId = nhanVienXuatId, NccId = nccId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string nv = r.NHANVIENXUAT?.ToString() ?? "";
                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        Ngay = ngay,
                        NhanVienXuat = nv,
                        GroupKey = groupByNhanVien ? (string.IsNullOrEmpty(nv) ? "Chưa chọn nhân viên" : nv) : (ngay?.ToString("dd/MM/yyyy") ?? ""),
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = r.SOLUONG != null ? Convert.ToDecimal(r.SOLUONG) : 0,
                        DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                        ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTongHopMatHangXuatAsync error: " + ex.Message);
            }
            return list;
        }

        // 4. BÁO CÁO NHẬP HÀNG
        public async Task<List<PhieuKhoRowItem>> GetDanhSachPhieuNhapKhoAsync(DateTime tuNgay, DateTime denNgay, string nccId = null, string nhanVienNhapId = null, string khoNhapId = null, string soPhieu = null, string groupBy = "NGAY")
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NAME as SoPhieu,
                        d.NGAY as Ngay,
                        ncc.NAME as NhaCungCap,
                        k.NAME as KhoNhap,
                        nv.NAME as NhanVienNhap,
                        COALESCE(d.NOTE, d.DIENGIAI, 'Nhập mua hàng') as DienGiai,
                        COALESCE(d.TIENHANG, 0) as TienHang,
                        COALESCE(d.TIENGIAMGIA, 0) as TienGiamGia,
                        COALESCE(d.TONGCONG, 0) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NccId IS NULL OR @NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                      AND (@NhanVienNhapId IS NULL OR @NhanVienNhapId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NhanVienNhapId)
                      AND (@KhoNhapId IS NULL OR @KhoNhapId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoNhapId)
                      AND (@SoPhieu IS NULL OR @SoPhieu = '' OR d.NAME CONTAINING @SoPhieu)
                    ORDER BY d.NGAY DESC, d.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NccId = nccId, NhanVienNhapId = nhanVienNhapId, KhoNhapId = khoNhapId, SoPhieu = soPhieu })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string ncc = r.NHACUNGCAP?.ToString() ?? "";
                    string nv = r.NHANVIENNHAP?.ToString() ?? "";

                    string gKey = groupBy switch
                    {
                        "NCC" => string.IsNullOrEmpty(ncc) ? "Chưa có nhà cung cấp" : ncc,
                        "NV" => string.IsNullOrEmpty(nv) ? "Chưa có nhân viên" : nv,
                        _ => ngay?.ToString("dd/MM/yyyy") ?? ""
                    };

                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = ngay,
                        NhaCungCap = ncc,
                        KhoNhap = r.KHONHAP?.ToString() ?? "",
                        NhanVienNhap = nv,
                        GroupKey = gKey,
                        DienGiai = r.DIENGIAI?.ToString() ?? "Nhập mua hàng",
                        TienHang = r.TIENHANG != null ? Convert.ToDecimal(r.TIENHANG) : 0,
                        TienGiamGia = r.TIENGIAMGIA != null ? Convert.ToDecimal(r.TIENGIAMGIA) : 0,
                        TongCong = r.TONGCONG != null ? Convert.ToDecimal(r.TONGCONG) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDanhSachPhieuNhapKhoAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<List<PhieuKhoRowItem>> GetTongHopNhapTheoAsync(DateTime tuNgay, DateTime denNgay, string nccId = null, string khoNhapId = null, string nhanVienNhapId = null, string groupType = "NGAY")
        {
            var raw = await GetDanhSachPhieuNhapKhoAsync(tuNgay, denNgay, nccId, nhanVienNhapId, khoNhapId, null, groupType);
            return groupType switch
            {
                "NCC" => raw.GroupBy(x => x.NhaCungCap)
                    .Select((g, idx) => new PhieuKhoRowItem
                    {
                        STT = idx + 1,
                        NhaCungCap = string.IsNullOrEmpty(g.Key) ? "Chưa chọn nhà cung cấp" : g.Key,
                        TienHang = g.Sum(x => x.TienHang),
                        TienGiamGia = g.Sum(x => x.TienGiamGia),
                        TongCong = g.Sum(x => x.TongCong)
                    }).ToList(),

                "NV" => raw.GroupBy(x => x.NhanVienNhap)
                    .Select((g, idx) => new PhieuKhoRowItem
                    {
                        STT = idx + 1,
                        NhanVienNhap = string.IsNullOrEmpty(g.Key) ? "Chưa chọn nhân viên" : g.Key,
                        TienHang = g.Sum(x => x.TienHang),
                        TienGiamGia = g.Sum(x => x.TienGiamGia),
                        TongCong = g.Sum(x => x.TongCong)
                    }).ToList(),

                _ => raw.GroupBy(x => x.NgayDisplay)
                    .Select((g, idx) => new PhieuKhoRowItem
                    {
                        STT = idx + 1,
                        Ngay = g.First().Ngay,
                        TienHang = g.Sum(x => x.TienHang),
                        TienGiamGia = g.Sum(x => x.TienGiamGia),
                        TongCong = g.Sum(x => x.TongCong)
                    }).ToList()
            };
        }

        public async Task<List<PhieuKhoRowItem>> GetTongHopMatHangNhapKhoAsync(DateTime tuNgay, DateTime denNgay, string nccId = null, string khoNhapId = null, string nhanVienNhapId = null, string nhomMhId = null, string mhId = null, string groupType = "NGAY")
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NGAY as Ngay,
                        ncc.NAME as NhaCungCap,
                        nv.NAME as NhanVienNhap,
                        m.CODE as MaHang,
                        COALESCE(c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        SUM(COALESCE(c.SLNHAP, 1)) as SoLuongNhap,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        SUM(COALESCE(c.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NccId IS NULL OR @NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                      AND (@KhoNhapId IS NULL OR @KhoNhapId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoNhapId)
                      AND (@NhanVienNhapId IS NULL OR @NhanVienNhapId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NhanVienNhapId)
                      AND (@NhomMhId IS NULL OR @NhomMhId = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomMhId)
                      AND (@MhId IS NULL OR @MhId = '' OR CAST(m.ID AS VARCHAR(50)) = @MhId)
                    GROUP BY d.ID, d.NGAY, ncc.NAME, nv.NAME, m.CODE, c.NAME, m.NAME, dv.NAME, c.DONGIA
                    ORDER BY d.NGAY, ncc.NAME, nv.NAME, m.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NccId = nccId, KhoNhapId = khoNhapId, NhanVienNhapId = nhanVienNhapId, NhomMhId = nhomMhId, MhId = mhId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string ncc = r.NHACUNGCAP?.ToString() ?? "";
                    string nv = r.NHANVIENNHAP?.ToString() ?? "";

                    string gKey = groupType switch
                    {
                        "NCC" => string.IsNullOrEmpty(ncc) ? "Chưa có nhà cung cấp" : ncc,
                        "NV" => string.IsNullOrEmpty(nv) ? "Chưa có nhân viên" : nv,
                        _ => ngay?.ToString("dd/MM/yyyy") ?? ""
                    };

                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        Ngay = ngay,
                        NhaCungCap = ncc,
                        NhanVienNhap = nv,
                        GroupKey = gKey,
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuongNhap = r.SOLUONGNHAP != null ? Convert.ToDecimal(r.SOLUONGNHAP) : 0,
                        DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                        ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTongHopMatHangNhapKhoAsync error: " + ex.Message);
            }
            return list;
        }

        // 5. BÁO CÁO CHUYỂN KHO
        public async Task<List<PhieuKhoRowItem>> GetDanhSachPhieuChuyenKhoAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = null, string khoNhapId = null, string nvXuatId = null, string nvNhapId = null, string groupType = "NGAY")
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NAME as SoPhieu,
                        d.NGAY as Ngay,
                        COALESCE(d.NOTE, d.DIENGIAI, 'Chuyển kho nội bộ') as DienGiai,
                        kx.NAME as KhoXuat,
                        kn.NAME as KhoNhap,
                        nvx.NAME as NhanVienXuat,
                        nvn.NAME as NhanVienNhap,
                        COALESCE(d.TONGCONG, 0) as TongCong,
                        d.NOTE as GhiChu
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG kx ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(kx.ID AS VARCHAR(50))
                    LEFT JOIN DKHOHANG kn ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(kn.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nvx ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nvx.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nvn ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nvn.ID AS VARCHAR(50))
                    WHERE d.LOAI = 3
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@KhoXuatId IS NULL OR @KhoXuatId = '' OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoXuatId)
                      AND (@KhoNhapId IS NULL OR @KhoNhapId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoNhapId)
                      AND (@NvXuatId IS NULL OR @NvXuatId = '' OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NvXuatId)
                      AND (@NvNhapId IS NULL OR @NvNhapId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvNhapId)
                    ORDER BY d.NGAY DESC, d.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoXuatId = khoXuatId, KhoNhapId = khoNhapId, NvXuatId = nvXuatId, NvNhapId = nvNhapId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string nvx = r.NHANVIENXUAT?.ToString() ?? "";
                    string nvn = r.NHANVIENNHAP?.ToString() ?? "";

                    string gKey = groupType switch
                    {
                        "NV_XUAT" => string.IsNullOrEmpty(nvx) ? "Chưa có NV xuất" : nvx,
                        "NV_NHAP" => string.IsNullOrEmpty(nvn) ? "Chưa có NV nhận" : nvn,
                        _ => ngay?.ToString("dd/MM/yyyy") ?? ""
                    };

                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = ngay,
                        KhoXuat = r.KHOXUAT?.ToString() ?? "",
                        KhoNhap = r.KHONHAP?.ToString() ?? "",
                        NhanVienXuat = nvx,
                        NhanVienNhap = nvn,
                        GroupKey = gKey,
                        DienGiai = r.DIENGIAI?.ToString() ?? "Chuyển kho nội bộ",
                        TongCong = r.TONGCONG != null ? Convert.ToDecimal(r.TONGCONG) : 0,
                        GhiChu = r.GHICHU?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDanhSachPhieuChuyenKhoAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<List<PhieuKhoRowItem>> GetTongHopMatHangChuyenKhoAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = null, string khoNhapId = null, string nvXuatId = null, string nvNhapId = null, string groupType = "NGAY")
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(d.ID AS VARCHAR(50)) as Id,
                        d.NGAY as Ngay,
                        nvx.NAME as NhanVienXuat,
                        nvn.NAME as NhanVienNhap,
                        m.CODE as MaHang,
                        COALESCE(c.NAME, m.NAME, '') as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        SUM(COALESCE(c.SLNHAP, c.SLXUAT, 1)) as SoLuong,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        SUM(COALESCE(c.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nvx ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nvx.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nvn ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nvn.ID AS VARCHAR(50))
                    WHERE d.LOAI = 3
                      AND (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay
                      AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@KhoXuatId IS NULL OR @KhoXuatId = '' OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoXuatId)
                      AND (@KhoNhapId IS NULL OR @KhoNhapId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoNhapId)
                      AND (@NvXuatId IS NULL OR @NvXuatId = '' OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NvXuatId)
                      AND (@NvNhapId IS NULL OR @NvNhapId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvNhapId)
                    GROUP BY d.ID, d.NGAY, nvx.NAME, nvn.NAME, m.CODE, c.NAME, m.NAME, dv.NAME, c.DONGIA
                    ORDER BY d.NGAY, nvx.NAME, nvn.NAME, m.NAME";

                var rows = (await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoXuatId = khoXuatId, KhoNhapId = khoNhapId, NvXuatId = nvXuatId, NvNhapId = nvNhapId })).ToList();
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;
                    string nvx = r.NHANVIENXUAT?.ToString() ?? "";
                    string nvn = r.NHANVIENNHAP?.ToString() ?? "";

                    string gKey = groupType switch
                    {
                        "NV_CHUYEN" => string.IsNullOrEmpty(nvx) ? "Nhân viên xuất: Chưa có" : $"Nhân viên xuất: {nvx}",
                        "NV_NHAN" => string.IsNullOrEmpty(nvn) ? "Nhân viên nhập: Chưa có" : $"Nhân viên nhập: {nvn}",
                        _ => ngay?.ToString("dd/MM/yyyy") ?? ""
                    };

                    list.Add(new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = r.ID?.ToString() ?? "",
                        Ngay = ngay,
                        NhanVienXuat = nvx,
                        NhanVienNhap = nvn,
                        GroupKey = gKey,
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = r.SOLUONG != null ? Convert.ToDecimal(r.SOLUONG) : 0,
                        DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                        ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTongHopMatHangChuyenKhoAsync error: " + ex.Message);
            }
            return list;
        }

        // 6. BÁO CÁO THEO HSD & HÀNG TỒN KHO
        public async Task<List<PhieuKhoRowItem>> GetBaoCaoHsdAsync(DateTime? tuNgay = null, DateTime? denNgay = null, string khoId = null, string nhomId = null, string mode = "HSD")
        {
            var list = new List<PhieuKhoRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        CAST(m.ID AS VARCHAR(50)) as Id,
                        m.CODE as MaHang,
                        m.NAME as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        CAST(m.DNHOMMATHANGID AS VARCHAR(50)) as NhomHangId,
                        COALESCE(nh.NAME, 'Chưa phân nhóm') as NhomHang,
                        COALESCE(m.TONTOITHIEU, 0) as TonToiThieu,
                        COALESCE(m.TONTOIDA, 0) as TonToiDa,
                        COALESCE(m.GIAVON, m.GIANHAP, 0) as GiaVon,
                        COALESCE(m.GIABAN, 0) as GiaBan
                    FROM DMATHANG m
                    LEFT JOIN DDONVITINH dv ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                    WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                      AND (@NhomId IS NULL OR @NhomId = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                    ORDER BY nh.NAME, m.NAME";

                var mhItems = (await conn.QueryAsync(sql, new { NhomId = nhomId })).ToList();
                var tonDict = await LocalKiemKeService.GetTonKhoDictionaryAsync(khoId);

                int stt = 1;
                foreach (var m in mhItems)
                {
                    string mId = m.ID?.ToString() ?? "";
                    decimal ton = tonDict.GetValueOrDefault(mId, 0);
                    decimal gVon = m.GIAVON != null ? Convert.ToDecimal(m.GIAVON) : 0;
                    decimal gBan = m.GIABAN != null ? Convert.ToDecimal(m.GIABAN) : 0;

                    var item = new PhieuKhoRowItem
                    {
                        STT = stt++,
                        Id = mId,
                        MaHang = m.MAHANG?.ToString() ?? "",
                        TenHang = m.TENHANG?.ToString() ?? "",
                        DVT = m.DVT?.ToString() ?? "",
                        GroupKey = m.NHOMHANG?.ToString() ?? "Chưa phân nhóm",
                        Ton = ton,
                        TonToiThieu = m.TONTOITHIEU != null ? Convert.ToDecimal(m.TONTOITHIEU) : 0,
                        TonToiDa = m.TONTOIDA != null ? Convert.ToDecimal(m.TONTOIDA) : 0,
                        GiaVon = gVon,
                        TriGiaVon = ton * gVon,
                        GiaBan = gBan,
                        TriGiaBan = ton * gBan
                    };

                    if (mode == "HET_HAN")
                    {
                        // Hàng hết hạn
                        if (item.HanDung.HasValue && item.HanDung.Value.Date < (denNgay ?? DateTime.Today))
                        {
                            list.Add(item);
                        }
                    }
                    else if (mode == "CO_HSD")
                    {
                        // Hàng có HSD
                        list.Add(item);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetBaoCaoHsdAsync error: " + ex.Message);
            }
            return list;
        }

        // 7. BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN & CHI TIẾT
        public async Task<List<XntRowItem>> GetTongHopXuatNhapTonAsync(DateTime tuNgay, DateTime denNgay, string khoId = null, string nhomId = null, string mhId = null)
        {
            var list = new List<XntRowItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // 1. Tải danh sách mặt hàng
                string sqlMh = @"
                    SELECT 
                        CAST(m.ID AS VARCHAR(50)) as Id,
                        m.CODE as MaHang,
                        m.NAME as TenHang,
                        COALESCE(dv.NAME, '') as DVT,
                        COALESCE(nh.NAME, 'Chưa phân nhóm') as NhomHang,
                        COALESCE(m.GIAVON, m.GIANHAP, 0) as GiaVon
                    FROM DMATHANG m
                    LEFT JOIN DDONVITINH dv ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                    WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                      AND (@NhomId IS NULL OR @NhomId = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@MhId IS NULL OR @MhId = '' OR CAST(m.ID AS VARCHAR(50)) = @MhId)
                    ORDER BY nh.NAME, m.NAME";

                var mhList = (await conn.QueryAsync(sqlMh, new { NhomId = nhomId, MhId = mhId })).ToList();

                // 2. Tải chi tiết giao dịch kho
                string sqlTx = @"
                    SELECT 
                        CAST(c.DMATHANGID AS VARCHAR(50)) as MhId,
                        d.LOAI as Loai,
                        CAST(d.DKHONHAPID AS VARCHAR(50)) as KhoNhapId,
                        CAST(d.DKHOXUATID AS VARCHAR(50)) as KhoXuatId,
                        CAST(d.NGAY AS DATE) as Ngay,
                        COALESCE(c.SLNHAP, 0) as SlNhap,
                        COALESCE(c.SLXUAT, 0) as SlXuat,
                        COALESCE(c.DONGIA, 0) as DonGia,
                        COALESCE(c.THANHTIEN, 0) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET c ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                    WHERE (d.STATUS IS NULL OR d.STATUS <> 0)
                      AND (c.STATUS IS NULL OR c.STATUS <> 0)";

                var txList = (await conn.QueryAsync(sqlTx)).ToList();

                int stt = 1;
                foreach (var mh in mhList)
                {
                    string mId = mh.ID?.ToString() ?? "";
                    decimal gVon = mh.GIAVON != null ? Convert.ToDecimal(mh.GIAVON) : 0;

                    decimal tonDau = 0;
                    decimal nhapTrongKy = 0;
                    decimal xuatTrongKy = 0;

                    decimal nhapMua = 0;
                    decimal nhapDieuChinh = 0;
                    decimal nhapChuyenKho = 0;

                    decimal xuatBan = 0;
                    decimal xuatVatTu = 0;
                    decimal xuatDieuChinh = 0;
                    decimal xuatChuyenKho = 0;
                    decimal xuatKhac = 0;

                    var mTx = txList.Where(x => x.MHID?.ToString() == mId).ToList();

                    foreach (var tx in mTx)
                    {
                        int loai = tx.LOAI != null ? Convert.ToInt32(tx.LOAI) : 0;
                        DateTime txNgay = tx.NGAY != null ? Convert.ToDateTime(tx.NGAY).Date : DateTime.MinValue;
                        decimal slN = tx.SLNHAP != null ? Convert.ToDecimal(tx.SLNHAP) : 0;
                        decimal slX = tx.SLXUAT != null ? Convert.ToDecimal(tx.SLXUAT) : 0;
                        string kn = tx.KHONHAPID?.ToString() ?? "";
                        string kx = tx.KHOXUATID?.ToString() ?? "";

                        bool matchKhoNhap = string.IsNullOrEmpty(khoId) || kn == khoId;
                        bool matchKhoXuat = string.IsNullOrEmpty(khoId) || kx == khoId;

                        decimal netChange = 0;
                        if (loai == 1 && matchKhoNhap) netChange += (slN > 0 ? slN : 1);
                        else if (loai == 2 && matchKhoXuat) netChange -= (slX > 0 ? slX : slN > 0 ? slN : 1);
                        else if (loai == 3)
                        {
                            if (matchKhoNhap && !matchKhoXuat) netChange += (slN > 0 ? slN : slX);
                            else if (matchKhoXuat && !matchKhoNhap) netChange -= (slX > 0 ? slX : slN);
                        }
                        else if (loai == 4)
                        {
                            if (matchKhoNhap) netChange += slN;
                            if (matchKhoXuat) netChange -= slX;
                        }
                        else if ((loai == 0 || loai == 10 || loai == 11) && matchKhoXuat)
                        {
                            netChange -= (slX > 0 ? slX : slN > 0 ? slN : 1);
                        }

                        if (txNgay < tuNgay.Date)
                        {
                            tonDau += netChange;
                        }
                        else if (txNgay <= denNgay.Date)
                        {
                            if (loai == 1 && matchKhoNhap) { decimal s = slN > 0 ? slN : 1; nhapTrongKy += s; nhapMua += s; }
                            else if (loai == 4 && matchKhoNhap && slN > 0) { nhapTrongKy += slN; nhapDieuChinh += slN; }
                            else if (loai == 3 && matchKhoNhap && !matchKhoXuat) { decimal s = slN > 0 ? slN : slX; nhapTrongKy += s; nhapChuyenKho += s; }

                            if ((loai == 0 || loai == 10 || loai == 11) && matchKhoXuat) { decimal s = slX > 0 ? slX : slN > 0 ? slN : 1; xuatTrongKy += s; xuatBan += s; }
                            else if (loai == 2 && matchKhoXuat) { decimal s = slX > 0 ? slX : slN > 0 ? slN : 1; xuatTrongKy += s; xuatKhac += s; }
                            else if (loai == 4 && matchKhoXuat && slX > 0) { xuatTrongKy += slX; xuatDieuChinh += slX; }
                            else if (loai == 3 && matchKhoXuat && !matchKhoNhap) { decimal s = slX > 0 ? slX : slN; xuatTrongKy += s; xuatChuyenKho += s; }
                        }
                    }

                    decimal tonCuoi = tonDau + nhapTrongKy - xuatTrongKy;

                    list.Add(new XntRowItem
                    {
                        STT = stt++,
                        NhomHang = mh.NHOMHANG?.ToString() ?? "Chưa phân nhóm",
                        MaHang = mh.MAHANG?.ToString() ?? "",
                        TenHang = mh.TENHANG?.ToString() ?? "",
                        DVT = mh.DVT?.ToString() ?? "",
                        GiaVon = gVon,
                        TonDauSl = tonDau,
                        TonDauTriGia = tonDau * gVon,
                        NhapSl = nhapTrongKy,
                        NhapTriGia = nhapTrongKy * gVon,
                        XuatSl = xuatTrongKy,
                        XuatTriGia = xuatTrongKy * gVon,
                        TonCuoiSl = tonCuoi,
                        TonCuoiTriGia = tonCuoi * gVon,

                        NhapMua = nhapMua,
                        NhapDieuChinh = nhapDieuChinh,
                        NhapChuyenKho = nhapChuyenKho,
                        XuatBan = xuatBan,
                        XuatVatTu = xuatVatTu,
                        XuatDieuChinh = xuatDieuChinh,
                        XuatChuyenKho = xuatChuyenKho,
                        XuatKhac = xuatKhac
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTongHopXuatNhapTonAsync error: " + ex.Message);
            }
            return list;
        }
        #endregion

        #region 10 Báo Cáo Kho Hàng Models & Services

        #region Models
        public class BaoCaoXuatBanHangTheoNgayItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
        }

        public class BaoCaoXuatBanHangTheoDonItem
        {
            public int STT { get; set; }
            public string SoPhieu { get; set; } = "";
            public string BanKhuVuc { get; set; } = "";
            public string GioVao { get; set; } = "";
            public string GioRa { get; set; } = "";
            public string ThuNgan { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal ThanhToan { get; set; }
        }

        public class BaoCaoXuatBanHangDinhLuongItem
        {
            public int STT { get; set; }
            public string MaNvl { get; set; } = "";
            public string TenNvl { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuongDinhLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
        }

        public class BaoCaoKiemKeDanhSachItem
        {
            public int STT { get; set; }
            public string SoPhieu { get; set; } = "";
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string KhoHang { get; set; } = "";
            public string NhanVien { get; set; } = "";
            public decimal ChenhLechSL { get; set; }
            public decimal GiaTriChenhLech { get; set; }
        }

        public class BaoCaoKiemKeTongHopItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuongSoSach { get; set; }
            public decimal SoLuongThucTe { get; set; }
            public decimal ChenhLech { get; set; }
            public decimal GiaTriChenhLech { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string NhanVien { get; set; } = "";
        }

        public class BaoCaoXuatKhacTongHopItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string NhanVien { get; set; } = "";
        }

        public class BaoCaoXuatKhacDanhSachItem
        {
            public int STT { get; set; }
            public string SoPhieu { get; set; } = "";
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string LyDoXuat { get; set; } = "";
            public string KhoXuat { get; set; } = "";
            public string NhanVien { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal ChiPhiKhac { get; set; }
            public decimal TongCong { get; set; }
        }

        public class BaoCaoXuatKhacTongHopXuatItem
        {
            public int STT { get; set; }
            public string TenHienThi { get; set; } = "";
            public int SoPhieu { get; set; }
            public string KhoXuat { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal ChiPhiKhac { get; set; }
            public decimal TongCong { get; set; }
        }

        public class BaoCaoNhapHangDanhSachItem
        {
            public int STT { get; set; }
            public string SoPhieu { get; set; } = "";
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string NhaCungCap { get; set; } = "";
            public string KhoHang { get; set; } = "";
            public string NhanVien { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCong { get; set; }
        }

        public class BaoCaoNhapHangTongHopMatHangItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string NhaCungCap { get; set; } = "";
            public string NhanVien { get; set; } = "";
        }

        public class BaoCaoNhapHangTongHopNhapItem
        {
            public int STT { get; set; }
            public string TenHienThi { get; set; } = "";
            public int SoPhieu { get; set; }
            public string KhoHang { get; set; } = "";
            public decimal TienHang { get; set; }
            public decimal GiamGia { get; set; }
            public decimal TongCong { get; set; }
        }

        public class BaoCaoChuyenKhoTongHopMatHangItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string NhanVienNhan { get; set; } = "";
            public string NhanVienChuyen { get; set; } = "";
        }

        public class BaoCaoChuyenKhoDanhSachItem
        {
            public int STT { get; set; }
            public string SoPhieu { get; set; } = "";
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string KhoXuat { get; set; } = "";
            public string KhoNhap { get; set; } = "";
            public string NhanVienXuat { get; set; } = "";
            public string NhanVienNhap { get; set; } = "";
            public decimal TongCong { get; set; }
        }

        public class BaoCaoHsdItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public string SoLo { get; set; } = "";
            public DateTime? NgaySx { get; set; }
            public string NgaySxDisplay => NgaySx?.ToString("dd/MM/yyyy") ?? "";
            public DateTime? HanDung { get; set; }
            public string HanDungDisplay => HanDung?.ToString("dd/MM/yyyy") ?? "";
            public decimal TonKho { get; set; }
            public string TrangThai { get; set; } = "Bình thường";
        }

        public class BaoCaoHangTonKhoItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public string KhoHang { get; set; } = "";
            public decimal SoLuongTon { get; set; }
            public decimal GiaVon { get; set; }
            public decimal ThanhTien { get; set; }
        }

        public class BaoCaoTongHopXntItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal TonDau { get; set; }
            public decimal Nhap { get; set; }
            public decimal Xuat { get; set; }
            public decimal TonCuoi { get; set; }
            public decimal GiaVon { get; set; }
            public decimal TienTon { get; set; }
        }

        public class BaoCaoTongHopXntChiTietItem
        {
            public int STT { get; set; }
            public string MaHang { get; set; } = "";
            public string TenHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal TonDauSL { get; set; }
            public decimal TonDauTT { get; set; }
            public decimal NhapSL { get; set; }
            public decimal NhapTT { get; set; }
            public decimal XuatSL { get; set; }
            public decimal XuatTT { get; set; }
            public decimal TonCuoiSL { get; set; }
            public decimal TonCuoiTT { get; set; }
        }

        public class BaoCaoTheKhoItem
        {
            public DateTime? Ngay { get; set; }
            public string NgayDisplay => Ngay?.ToString("dd/MM/yyyy") ?? "";
            public string SoPhieu { get; set; } = "";
            public string DienGiai { get; set; } = "";
            public string MaHang { get; set; } = "";
            public string DVT { get; set; } = "";
            public decimal DonGia { get; set; }
            public decimal NhapSL { get; set; }
            public decimal XuatSL { get; set; }
            public decimal TonSL { get; set; }
            public decimal ThanhTienTon { get; set; }
        }
        #endregion

        #region Query Methods

        public async Task<List<BaoCaoXuatBanHangTheoNgayItem>> GetXuatBanHangTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoXuatBanHangTheoNgayItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        m.MA as MaHang,
                        m.NAME as TenHang,
                        m.DONVITINH as DVT,
                        SUM(COALESCE(ct.SOLUONG, 0)) as SoLuong,
                        AVG(COALESCE(ct.DONGIA, 0)) as DonGia,
                        SUM(COALESCE(ct.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    JOIN DMATHANG m ON ct.MATHANGID = m.ID
                    WHERE d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (d.LOAI = 0 OR d.LOAI = 10 OR d.LOAI = 11 OR d.LOAI IS NULL)
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                      AND (@NhomId = '' OR CAST(m.NHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@MhId = '' OR CAST(m.ID AS VARCHAR(50)) = @MhId)
                    GROUP BY d.NGAY, m.MA, m.NAME, m.DONVITINH
                    ORDER BY d.NGAY, m.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NhomId = nhomId, MhId = mhId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoXuatBanHangTheoNgayItem
                    {
                        STT = stt++,
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = Convert.ToDecimal(r.SOLUONG ?? 0),
                        DonGia = Convert.ToDecimal(r.DONGIA ?? 0),
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatBanHangTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoXuatBanHangTheoDonItem>> GetXuatBanHangTheoDonAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoXuatBanHangTheoDonItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.SOPHIEU as SoPhieu,
                        COALESCE(b.NAME, 'Mang về / Khác') as BanKhuVuc,
                        d.GIOVAO as GioVao,
                        d.GIORA as GioRa,
                        nv.NAME as ThuNgan,
                        COALESCE(d.TONGTIEN, 0) as TienHang,
                        COALESCE(d.GIAMGIA, 0) as GiamGia,
                        COALESCE(d.THANHTOAN, 0) as ThanhToan
                    FROM TDONHANG d
                    LEFT JOIN DBANKHUVUC b ON d.BANID = b.ID
                    LEFT JOIN DNHANVIEN nv ON d.NHANVIENID = nv.ID
                    WHERE d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (d.LOAI = 0 OR d.LOAI = 10 OR d.LOAI = 11 OR d.LOAI IS NULL)
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                    ORDER BY d.NGAY, d.SOPHIEU";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoXuatBanHangTheoDonItem
                    {
                        STT = stt++,
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        BanKhuVuc = r.BANKHUVUC?.ToString() ?? "",
                        GioVao = r.GIOVAO?.ToString() ?? "",
                        GioRa = r.GIORA?.ToString() ?? "",
                        ThuNgan = r.THUNGAN?.ToString() ?? "",
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        GiamGia = Convert.ToDecimal(r.GIAMGIA ?? 0),
                        ThanhToan = Convert.ToDecimal(r.THANHTOAN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatBanHangTheoDonAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoXuatBanHangDinhLuongItem>> GetXuatBanHangDinhLuongTheoDonAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoXuatBanHangDinhLuongItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        nvl.MA as MaNvl,
                        nvl.NAME as TenNvl,
                        nvl.DONVITINH as DVT,
                        SUM(COALESCE(ct.SOLUONG, 1) * COALESCE(dl.SOLUONG, 1)) as SoLuongDinhLuong,
                        COALESCE(nvl.GIAVON, nvl.GIANHAP, 0) as DonGia,
                        SUM(COALESCE(ct.SOLUONG, 1) * COALESCE(dl.SOLUONG, 1) * COALESCE(nvl.GIAVON, nvl.GIANHAP, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    JOIN DDINHLUONG dl ON ct.MATHANGID = dl.MATHANGID
                    JOIN DMATHANG nvl ON dl.NGUYENLIEUID = nvl.ID
                    WHERE d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (d.LOAI = 0 OR d.LOAI = 10 OR d.LOAI = 11 OR d.LOAI IS NULL)
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                    GROUP BY nvl.MA, nvl.NAME, nvl.DONVITINH, nvl.GIAVON, nvl.GIANHAP
                    ORDER BY nvl.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoXuatBanHangDinhLuongItem
                    {
                        STT = stt++,
                        MaNvl = r.MANVL?.ToString() ?? "",
                        TenNvl = r.TENNVL?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuongDinhLuong = Convert.ToDecimal(r.SOLUONGDINHLUONG ?? 0),
                        DonGia = Convert.ToDecimal(r.DONGIA ?? 0),
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatBanHangDinhLuongTheoDonAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoKiemKeDanhSachItem>> GetKiemKeDanhSachTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoKiemKeDanhSachItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.SOPHIEU as SoPhieu,
                        d.NGAY as Ngay,
                        k.NAME as KhoHang,
                        nv.NAME as NhanVien,
                        SUM(COALESCE(ct.SOLUONGTHUCTE, 0) - COALESCE(ct.SOLUONG, 0)) as ChenhLechSL,
                        SUM((COALESCE(ct.SOLUONGTHUCTE, 0) - COALESCE(ct.SOLUONG, 0)) * COALESCE(ct.DONGIA, 0)) as GiaTriChenhLech
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG k ON d.KHONHAPID = k.ID
                    LEFT JOIN DNHANVIEN nv ON d.NHANVIENID = nv.ID
                    LEFT JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    WHERE d.LOAI = 4
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.KHONHAPID AS VARCHAR(50)) = @KhoId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId)
                    GROUP BY d.SOPHIEU, d.NGAY, k.NAME, nv.NAME
                    ORDER BY d.NGAY, d.SOPHIEU";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoKiemKeDanhSachItem
                    {
                        STT = stt++,
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        KhoHang = r.KHOHANG?.ToString() ?? "",
                        NhanVien = r.NHANVIEN?.ToString() ?? "",
                        ChenhLechSL = Convert.ToDecimal(r.CHENHLECHSL ?? 0),
                        GiaTriChenhLech = Convert.ToDecimal(r.GIATRICHENHLECH ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetKiemKeDanhSachTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoKiemKeTongHopItem>> GetKiemKeTongHopTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string nvId = "")
        {
            var list = new List<BaoCaoKiemKeTongHopItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        nv.NAME as NhanVien,
                        m.MA as MaHang,
                        m.NAME as TenHang,
                        m.DONVITINH as DVT,
                        SUM(COALESCE(ct.SOLUONG, 0)) as SoLuongSoSach,
                        SUM(COALESCE(ct.SOLUONGTHUCTE, 0)) as SoLuongThucTe,
                        SUM(COALESCE(ct.SOLUONGTHUCTE, 0) - COALESCE(ct.SOLUONG, 0)) as ChenhLech,
                        SUM((COALESCE(ct.SOLUONGTHUCTE, 0) - COALESCE(ct.SOLUONG, 0)) * COALESCE(ct.DONGIA, 0)) as GiaTriChenhLech
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    JOIN DMATHANG m ON ct.MATHANGID = m.ID
                    LEFT JOIN DNHANVIEN nv ON d.NHANVIENID = nv.ID
                    WHERE d.LOAI = 4
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.KHONHAPID AS VARCHAR(50)) = @KhoId)
                      AND (@NhomId = '' OR CAST(m.NHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId)
                    GROUP BY d.NGAY, nv.NAME, m.MA, m.NAME, m.DONVITINH
                    ORDER BY d.NGAY, m.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NhomId = nhomId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoKiemKeTongHopItem
                    {
                        STT = stt++,
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        NhanVien = r.NHANVIEN?.ToString() ?? "",
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuongSoSach = Convert.ToDecimal(r.SOLUONGSOSACH ?? 0),
                        SoLuongThucTe = Convert.ToDecimal(r.SOLUONGTHUCTE ?? 0),
                        ChenhLech = Convert.ToDecimal(r.CHENHLECH ?? 0),
                        GiaTriChenhLech = Convert.ToDecimal(r.GIATRICHENHLECH ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetKiemKeTongHopTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoKiemKeTongHopItem>> GetKiemKeTongHopTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string nvId = "")
        {
            return await GetKiemKeTongHopTheoNgayAsync(tuNgay, denNgay, khoId, nhomId, nvId);
        }

        public async Task<List<BaoCaoKiemKeDanhSachItem>> GetKiemKeDanhSachTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nvId = "")
        {
            return await GetKiemKeDanhSachTheoNgayAsync(tuNgay, denNgay, khoId, nvId);
        }

        public async Task<List<BaoCaoXuatKhacTongHopItem>> GetXuatKhacTongHopTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string nvId = "")
        {
            var list = new List<BaoCaoXuatKhacTongHopItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        nv.NAME as NhanVien,
                        m.MA as MaHang,
                        m.NAME as TenHang,
                        m.DONVITINH as DVT,
                        SUM(COALESCE(ct.SOLUONG, 0)) as SoLuong,
                        AVG(COALESCE(ct.DONGIA, 0)) as DonGia,
                        SUM(COALESCE(ct.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    JOIN DMATHANG m ON ct.MATHANGID = m.ID
                    LEFT JOIN DNHANVIEN nv ON d.NHANVIENID = nv.ID
                    WHERE d.LOAI = 2
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                      AND (@NhomId = '' OR CAST(m.NHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId)
                    GROUP BY d.NGAY, nv.NAME, m.MA, m.NAME, m.DONVITINH
                    ORDER BY d.NGAY, m.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NhomId = nhomId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoXuatKhacTongHopItem
                    {
                        STT = stt++,
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        NhanVien = r.NHANVIEN?.ToString() ?? "",
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = Convert.ToDecimal(r.SOLUONG ?? 0),
                        DonGia = Convert.ToDecimal(r.DONGIA ?? 0),
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatKhacTongHopTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoXuatKhacTongHopItem>> GetXuatKhacTongHopTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string nvId = "")
        {
            return await GetXuatKhacTongHopTheoNgayAsync(tuNgay, denNgay, khoId, nhomId, nvId);
        }

        public async Task<List<BaoCaoXuatKhacDanhSachItem>> GetXuatKhacDanhSachTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoXuatKhacDanhSachItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.SOPHIEU as SoPhieu,
                        d.NGAY as Ngay,
                        COALESCE(d.GHICHU, 'Xuất hủy / Hao hụt / Khác') as LyDoXuat,
                        k.NAME as KhoXuat,
                        nv.NAME as NhanVien,
                        COALESCE(d.TONGTIEN, 0) as TienHang,
                        COALESCE(d.CHIPHIKHAC, 0) as ChiPhiKhac,
                        COALESCE(d.THANHTOAN, d.TONGTIEN, 0) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG k ON d.KHOXUATID = k.ID
                    LEFT JOIN DNHANVIEN nv ON d.NHANVIENID = nv.ID
                    WHERE d.LOAI = 2
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId)
                    ORDER BY d.NGAY, d.SOPHIEU";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoXuatKhacDanhSachItem
                    {
                        STT = stt++,
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        LyDoXuat = r.LYDOXUAT?.ToString() ?? "",
                        KhoXuat = r.KHOXUAT?.ToString() ?? "",
                        NhanVien = r.NHANVIEN?.ToString() ?? "",
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        ChiPhiKhac = Convert.ToDecimal(r.CHIPHIKHAC ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatKhacDanhSachTheoNhanVienAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoXuatKhacTongHopXuatItem>> GetXuatKhacTongHopXuatKhacTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoXuatKhacTongHopXuatItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        k.NAME as KhoXuat,
                        COUNT(d.ID) as SoPhieu,
                        SUM(COALESCE(d.TONGTIEN, 0)) as TienHang,
                        SUM(COALESCE(d.CHIPHIKHAC, 0)) as ChiPhiKhac,
                        SUM(COALESCE(d.THANHTOAN, d.TONGTIEN, 0)) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG k ON d.KHOXUATID = k.ID
                    WHERE d.LOAI = 2
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                    GROUP BY d.NGAY, k.NAME
                    ORDER BY d.NGAY";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId });
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? dt = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null;
                    list.Add(new BaoCaoXuatKhacTongHopXuatItem
                    {
                        STT = stt++,
                        TenHienThi = dt?.ToString("dd/MM/yyyy") ?? "",
                        KhoXuat = r.KHOXUAT?.ToString() ?? "",
                        SoPhieu = Convert.ToInt32(r.SOPHIEU ?? 0),
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        ChiPhiKhac = Convert.ToDecimal(r.CHIPHIKHAC ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatKhacTongHopXuatKhacTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoXuatKhacTongHopXuatItem>> GetXuatKhacTongHopXuatKhacTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoXuatKhacTongHopXuatItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        nv.NAME as NhanVien,
                        k.NAME as KhoXuat,
                        COUNT(d.ID) as SoPhieu,
                        SUM(COALESCE(d.TONGTIEN, 0)) as TienHang,
                        SUM(COALESCE(d.CHIPHIKHAC, 0)) as ChiPhiKhac,
                        SUM(COALESCE(d.THANHTOAN, d.TONGTIEN, 0)) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG k ON d.KHOXUATID = k.ID
                    LEFT JOIN DNHANVIEN nv ON d.NHANVIENID = nv.ID
                    WHERE d.LOAI = 2
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId)
                    GROUP BY nv.NAME, k.NAME
                    ORDER BY nv.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoXuatKhacTongHopXuatItem
                    {
                        STT = stt++,
                        TenHienThi = r.NHANVIEN?.ToString() ?? "Chưa chỉ định",
                        KhoXuat = r.KHOXUAT?.ToString() ?? "",
                        SoPhieu = Convert.ToInt32(r.SOPHIEU ?? 0),
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        ChiPhiKhac = Convert.ToDecimal(r.CHIPHIKHAC ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetXuatKhacTongHopXuatKhacTheoNhanVienAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoXuatKhacDanhSachItem>> GetXuatKhacDanhSachTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nvId = "")
        {
            return await GetXuatKhacDanhSachTheoNhanVienAsync(tuNgay, denNgay, khoId, nvId);
        }

        public async Task<List<BaoCaoNhapHangDanhSachItem>> GetNhapHangDanhSachTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoNhapHangDanhSachItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NAME as SoPhieu,
                        d.NGAY as Ngay,
                        ncc.NAME as NhaCungCap,
                        k.NAME as KhoHang,
                        nv.NAME as NhanVien,
                        COALESCE(d.TIENHANG, 0) as TienHang,
                        COALESCE(d.TIENGIAMGIA, 0) as GiamGia,
                        COALESCE(d.TONGCONG, 0) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                      AND (@KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId)
                      AND (@NvId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                    ORDER BY d.NGAY DESC, d.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NccId = nccId, KhoId = khoId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoNhapHangDanhSachItem
                    {
                        STT = stt++,
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        NhaCungCap = r.NHACUNGCAP?.ToString() ?? "",
                        KhoHang = r.KHOHANG?.ToString() ?? "",
                        NhanVien = r.NHANVIEN?.ToString() ?? "",
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        GiamGia = Convert.ToDecimal(r.GIAMGIA ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetNhapHangDanhSachTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoNhapHangDanhSachItem>> GetNhapHangDanhSachTheoNhaCungCapAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nvId = "")
        {
            return await GetNhapHangDanhSachTheoNgayAsync(tuNgay, denNgay, nccId, khoId, nvId);
        }

        public async Task<List<BaoCaoNhapHangDanhSachItem>> GetNhapHangDanhSachTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nvId = "")
        {
            return await GetNhapHangDanhSachTheoNgayAsync(tuNgay, denNgay, nccId, khoId, nvId);
        }

        public async Task<List<BaoCaoNhapHangTongHopMatHangItem>> GetNhapHangTongHopMatHangTheoNhaCungCapAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nhomId = "", string nvId = "")
        {
            var list = new List<BaoCaoNhapHangTongHopMatHangItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        ncc.NAME as NhaCungCap,
                        nv.NAME as NhanVien,
                        m.CODE as MaHang,
                        m.NAME as TenHang,
                        dv.NAME as DVT,
                        SUM(COALESCE(ct.SOLUONGNHAP, ct.SOLUONG, 0)) as SoLuong,
                        AVG(COALESCE(ct.DONGIA, 0)) as DonGia,
                        SUM(COALESCE(ct.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON CAST(d.ID AS VARCHAR(50)) = CAST(ct.TDONHANGID AS VARCHAR(50))
                    JOIN DMATHANG m ON CAST(ct.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                    LEFT JOIN DDONVITINH dv ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dv.ID AS VARCHAR(50))
                    LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                      AND (@KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId)
                      AND (@NhomId = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@NvId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                    GROUP BY d.NGAY, ncc.NAME, nv.NAME, m.CODE, m.NAME, dv.NAME
                    ORDER BY d.NGAY, m.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NccId = nccId, KhoId = khoId, NhomId = nhomId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoNhapHangTongHopMatHangItem
                    {
                        STT = stt++,
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        NhaCungCap = r.NHACUNGCAP?.ToString() ?? "Khác",
                        NhanVien = r.NHANVIEN?.ToString() ?? "",
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = Convert.ToDecimal(r.SOLUONG ?? 0),
                        DonGia = Convert.ToDecimal(r.DONGIA ?? 0),
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetNhapHangTongHopMatHangTheoNhaCungCapAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoNhapHangTongHopMatHangItem>> GetNhapHangTongHopMatHangTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nhomId = "", string nvId = "")
        {
            return await GetNhapHangTongHopMatHangTheoNhaCungCapAsync(tuNgay, denNgay, nccId, khoId, nhomId, nvId);
        }

        public async Task<List<BaoCaoNhapHangTongHopNhapItem>> GetNhapHangTongHopNhapTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoNhapHangTongHopNhapItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        k.NAME as KhoHang,
                        COUNT(d.ID) as SoPhieu,
                        SUM(COALESCE(d.TIENHANG, 0)) as TienHang,
                        SUM(COALESCE(d.TIENGIAMGIA, 0)) as GiamGia,
                        SUM(COALESCE(d.TONGCONG, 0)) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                      AND (@KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId)
                    GROUP BY d.NGAY, k.NAME
                    ORDER BY d.NGAY";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NccId = nccId, KhoId = khoId });
                int stt = 1;
                foreach (var r in rows)
                {
                    DateTime? dt = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null;
                    list.Add(new BaoCaoNhapHangTongHopNhapItem
                    {
                        STT = stt++,
                        TenHienThi = dt?.ToString("dd/MM/yyyy") ?? "",
                        KhoHang = r.KHOHANG?.ToString() ?? "",
                        SoPhieu = Convert.ToInt32(r.SOPHIEU ?? 0),
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        GiamGia = Convert.ToDecimal(r.GIAMGIA ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetNhapHangTongHopNhapTheoNgayAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoNhapHangTongHopNhapItem>> GetNhapHangTongHopNhapTheoNhaCungCapAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoNhapHangTongHopNhapItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        ncc.NAME as NhaCungCap,
                        k.NAME as KhoHang,
                        COUNT(d.ID) as SoPhieu,
                        SUM(COALESCE(d.TIENHANG, 0)) as TienHang,
                        SUM(COALESCE(d.TIENGIAMGIA, 0)) as GiamGia,
                        SUM(COALESCE(d.TONGCONG, 0)) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@NccId = '' OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                      AND (@KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId)
                    GROUP BY ncc.NAME, k.NAME
                    ORDER BY ncc.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, NccId = nccId, KhoId = khoId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoNhapHangTongHopNhapItem
                    {
                        STT = stt++,
                        TenHienThi = r.NHACUNGCAP?.ToString() ?? "Chưa phân loại",
                        KhoHang = r.KHOHANG?.ToString() ?? "",
                        SoPhieu = Convert.ToInt32(r.SOPHIEU ?? 0),
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        GiamGia = Convert.ToDecimal(r.GIAMGIA ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetNhapHangTongHopNhapTheoNhaCungCapAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoNhapHangTongHopNhapItem>> GetNhapHangTongHopNhapTheoNhanVienAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nvId = "")
        {
            var list = new List<BaoCaoNhapHangTongHopNhapItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        nv.NAME as NhanVien,
                        k.NAME as KhoHang,
                        COUNT(d.ID) as SoPhieu,
                        SUM(COALESCE(d.TIENHANG, 0)) as TienHang,
                        SUM(COALESCE(d.TIENGIAMGIA, 0)) as GiamGia,
                        SUM(COALESCE(d.TONGCONG, 0)) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                    LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                    WHERE d.LOAI = 1
                      AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                      AND CAST(d.NGAY AS DATE) >= @TuNgay AND CAST(d.NGAY AS DATE) <= @DenNgay
                      AND (@KhoId = '' OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId)
                      AND (@NvId = '' OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                    GROUP BY nv.NAME, k.NAME
                    ORDER BY nv.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoId = khoId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoNhapHangTongHopNhapItem
                    {
                        STT = stt++,
                        TenHienThi = r.NHANVIEN?.ToString() ?? "Chưa chỉ định",
                        KhoHang = r.KHOHANG?.ToString() ?? "",
                        SoPhieu = Convert.ToInt32(r.SOPHIEU ?? 0),
                        TienHang = Convert.ToDecimal(r.TIENHANG ?? 0),
                        GiamGia = Convert.ToDecimal(r.GIAMGIA ?? 0),
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetNhapHangTongHopNhapTheoNhanVienAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoNhapHangTongHopMatHangItem>> GetNhapHangTongHopMatHangNhapTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string nccId = "", string khoId = "", string nhomId = "", string nvId = "")
        {
            return await GetNhapHangTongHopMatHangTheoNhaCungCapAsync(tuNgay, denNgay, nccId, khoId, nhomId, nvId);
        }

        public async Task<List<BaoCaoChuyenKhoTongHopMatHangItem>> GetChuyenKhoTongHopMatHangTheoNhanVienNhanAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = "", string khoNhapId = "", string nhomId = "", string nvId = "")
        {
            var list = new List<BaoCaoChuyenKhoTongHopMatHangItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        nvNhan.NAME as NhanVienNhan,
                        nvChuyen.NAME as NhanVienChuyen,
                        m.MA as MaHang,
                        m.NAME as TenHang,
                        m.DONVITINH as DVT,
                        SUM(COALESCE(ct.SOLUONG, 0)) as SoLuong,
                        AVG(COALESCE(ct.DONGIA, 0)) as DonGia,
                        SUM(COALESCE(ct.THANHTIEN, 0)) as ThanhTien
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    JOIN DMATHANG m ON ct.MATHANGID = m.ID
                    LEFT JOIN DNHANVIEN nvNhan ON d.NHANVIENNHAPID = nvNhan.ID
                    LEFT JOIN DNHANVIEN nvChuyen ON d.NHANVIENID = nvChuyen.ID
                    WHERE d.LOAI = 3
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoXuatId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoXuatId)
                      AND (@KhoNhapId = '' OR CAST(d.KHONHAPID AS VARCHAR(50)) = @KhoNhapId)
                      AND (@NhomId = '' OR CAST(m.NHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId OR CAST(d.NHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                    GROUP BY d.NGAY, nvNhan.NAME, nvChuyen.NAME, m.MA, m.NAME, m.DONVITINH
                    ORDER BY d.NGAY, m.NAME";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoXuatId = khoXuatId, KhoNhapId = khoNhapId, NhomId = nhomId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoChuyenKhoTongHopMatHangItem
                    {
                        STT = stt++,
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        NhanVienNhan = r.NHANVIENNHAN?.ToString() ?? "Chưa rõ",
                        NhanVienChuyen = r.NHANVIENCHUYEN?.ToString() ?? "Chưa rõ",
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLuong = Convert.ToDecimal(r.SOLUONG ?? 0),
                        DonGia = Convert.ToDecimal(r.DONGIA ?? 0),
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetChuyenKhoTongHopMatHangTheoNhanVienNhanAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoChuyenKhoTongHopMatHangItem>> GetChuyenKhoTongHopMatHangTheoNhanVienChuyenAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = "", string khoNhapId = "", string nhomId = "", string nvId = "")
        {
            return await GetChuyenKhoTongHopMatHangTheoNhanVienNhanAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
        }

        public async Task<List<BaoCaoChuyenKhoTongHopMatHangItem>> GetChuyenKhoTongHopMatHangTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = "", string khoNhapId = "", string nhomId = "", string nvId = "")
        {
            return await GetChuyenKhoTongHopMatHangTheoNhanVienNhanAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
        }

        public async Task<List<BaoCaoChuyenKhoDanhSachItem>> GetChuyenKhoDanhSachTheoNhanVienXuatAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = "", string khoNhapId = "", string nvId = "")
        {
            var list = new List<BaoCaoChuyenKhoDanhSachItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        d.SOPHIEU as SoPhieu,
                        d.NGAY as Ngay,
                        kx.NAME as KhoXuat,
                        kn.NAME as KhoNhap,
                        nvx.NAME as NhanVienXuat,
                        nvn.NAME as NhanVienNhap,
                        COALESCE(d.THANHTOAN, d.TONGTIEN, 0) as TongCong
                    FROM TDONHANG d
                    LEFT JOIN DKHOHANG kx ON d.KHOXUATID = kx.ID
                    LEFT JOIN DKHOHANG kn ON d.KHONHAPID = kn.ID
                    LEFT JOIN DNHANVIEN nvx ON d.NHANVIENID = nvx.ID
                    LEFT JOIN DNHANVIEN nvn ON d.NHANVIENNHAPID = nvn.ID
                    WHERE d.LOAI = 3
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                      AND (@KhoXuatId = '' OR CAST(d.KHOXUATID AS VARCHAR(50)) = @KhoXuatId)
                      AND (@KhoNhapId = '' OR CAST(d.KHONHAPID AS VARCHAR(50)) = @KhoNhapId)
                      AND (@NvId = '' OR CAST(d.NHANVIENID AS VARCHAR(50)) = @NvId OR CAST(d.NHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                    ORDER BY d.NGAY, d.SOPHIEU";

                var rows = await conn.QueryAsync(sql, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date, KhoXuatId = khoXuatId, KhoNhapId = khoNhapId, NvId = nvId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoChuyenKhoDanhSachItem
                    {
                        STT = stt++,
                        SoPhieu = r.SOPHIEU?.ToString() ?? "",
                        Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                        KhoXuat = r.KHOXUAT?.ToString() ?? "",
                        KhoNhap = r.KHONHAP?.ToString() ?? "",
                        NhanVienXuat = r.NHANVIENXUAT?.ToString() ?? "",
                        NhanVienNhap = r.NHANVIENNHAP?.ToString() ?? "",
                        TongCong = Convert.ToDecimal(r.TONGCONG ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetChuyenKhoDanhSachTheoNhanVienXuatAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoChuyenKhoDanhSachItem>> GetChuyenKhoDanhSachTheoNhanVienNhapAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = "", string khoNhapId = "", string nvId = "")
        {
            return await GetChuyenKhoDanhSachTheoNhanVienXuatAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
        }

        public async Task<List<BaoCaoChuyenKhoDanhSachItem>> GetChuyenKhoDanhSachTheoNgayAsync(DateTime tuNgay, DateTime denNgay, string khoXuatId = "", string khoNhapId = "", string nvId = "")
        {
            return await GetChuyenKhoDanhSachTheoNhanVienXuatAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
        }

        public async Task<List<BaoCaoHsdItem>> GetBaoCaoTheoHsdAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoHsdItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        m.MA as MaHang,
                        m.NAME as TenHang,
                        m.DONVITINH as DVT,
                        m.SOLO as SoLo,
                        m.NGAYSX as NgaySx,
                        m.HANDUNG as HanDung,
                        COALESCE(m.TONKHO, 0) as TonKho
                    FROM DMATHANG m
                    WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                      AND m.HANDUNG IS NOT NULL
                      AND (@NhomId = '' OR CAST(m.NHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@MhId = '' OR CAST(m.ID AS VARCHAR(50)) = @MhId)
                    ORDER BY m.HANDUNG, m.NAME";

                var rows = await conn.QueryAsync(sql, new { NhomId = nhomId, MhId = mhId });
                int stt = 1;
                var today = DateTime.Today;
                foreach (var r in rows)
                {
                    DateTime? hd = r.HANDUNG != null ? Convert.ToDateTime(r.HANDUNG) : (DateTime?)null;
                    string tt = "Bình thường";
                    if (hd.HasValue)
                    {
                        if (hd.Value.Date < today) tt = "Hết hạn";
                        else if ((hd.Value.Date - today).TotalDays <= 30) tt = "Cận hạn";
                    }

                    list.Add(new BaoCaoHsdItem
                    {
                        STT = stt++,
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        SoLo = r.SOLO?.ToString() ?? "",
                        NgaySx = r.NGAYSX != null ? Convert.ToDateTime(r.NGAYSX) : (DateTime?)null,
                        HanDung = hd,
                        TonKho = Convert.ToDecimal(r.TONKHO ?? 0),
                        TrangThai = tt
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetBaoCaoTheoHsdAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoHsdItem>> GetBaoCaoHangHetHanAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = await GetBaoCaoTheoHsdAsync(tuNgay, denNgay, khoId, nhomId, mhId);
            return list.Where(x => x.TrangThai == "Hết hạn").ToList();
        }

        public async Task<List<BaoCaoHsdItem>> GetBaoCaoTonKhoCoHsdAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = await GetBaoCaoTheoHsdAsync(tuNgay, denNgay, khoId, nhomId, mhId);
            return list.Where(x => x.TonKho > 0).ToList();
        }

        public async Task<List<BaoCaoHangTonKhoItem>> GetBaoCaoHangTonKhoAsync(DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoHangTonKhoItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string sql = @"
                    SELECT 
                        m.MA as MaHang,
                        m.NAME as TenHang,
                        m.DONVITINH as DVT,
                        COALESCE(k.NAME, 'Kho chính') as KhoHang,
                        COALESCE(m.TONKHO, 0) as SoLuongTon,
                        COALESCE(m.GIAVON, m.GIANHAP, 0) as GiaVon,
                        COALESCE(m.TONKHO, 0) * COALESCE(m.GIAVON, m.GIANHAP, 0) as ThanhTien
                    FROM DMATHANG m
                    LEFT JOIN DKHOHANG k ON m.KHOHANGID = k.ID
                    WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                      AND (@KhoId = '' OR CAST(m.KHOHANGID AS VARCHAR(50)) = @KhoId)
                      AND (@NhomId = '' OR CAST(m.NHOMMATHANGID AS VARCHAR(50)) = @NhomId)
                      AND (@MhId = '' OR CAST(m.ID AS VARCHAR(50)) = @MhId)
                    ORDER BY m.NAME";

                var rows = await conn.QueryAsync(sql, new { KhoId = khoId, NhomId = nhomId, MhId = mhId });
                int stt = 1;
                foreach (var r in rows)
                {
                    list.Add(new BaoCaoHangTonKhoItem
                    {
                        STT = stt++,
                        MaHang = r.MAHANG?.ToString() ?? "",
                        TenHang = r.TENHANG?.ToString() ?? "",
                        DVT = r.DVT?.ToString() ?? "",
                        KhoHang = r.KHOHANG?.ToString() ?? "",
                        SoLuongTon = Convert.ToDecimal(r.SOLUONGTON ?? 0),
                        GiaVon = Convert.ToDecimal(r.GIAVON ?? 0),
                        ThanhTien = Convert.ToDecimal(r.THANHTIEN ?? 0)
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetBaoCaoHangTonKhoAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoTongHopXntItem>> GetBaoCaoTongHopXntAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoTongHopXntItem>();
            try
            {
                var xntRows = await GetTongHopXuatNhapTonAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                int stt = 1;
                foreach (var r in xntRows)
                {
                    list.Add(new BaoCaoTongHopXntItem
                    {
                        STT = stt++,
                        MaHang = r.MaHang,
                        TenHang = r.TenHang,
                        DVT = r.DVT,
                        TonDau = r.TonDauSl,
                        Nhap = r.NhapSl,
                        Xuat = r.XuatSl,
                        TonCuoi = r.TonCuoiSl,
                        GiaVon = r.GiaVon,
                        TienTon = r.TonCuoiTriGia
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetBaoCaoTongHopXntAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoTongHopXntChiTietItem>> GetBaoCaoTongHopXntChiTietAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string nhomId = "", string mhId = "")
        {
            var list = new List<BaoCaoTongHopXntChiTietItem>();
            try
            {
                var xntRows = await GetTongHopXuatNhapTonAsync(tuNgay, denNgay, khoId, nhomId, mhId);
                int stt = 1;
                foreach (var r in xntRows)
                {
                    list.Add(new BaoCaoTongHopXntChiTietItem
                    {
                        STT = stt++,
                        MaHang = r.MaHang,
                        TenHang = r.TenHang,
                        DVT = r.DVT,
                        TonDauSL = r.TonDauSl,
                        TonDauTT = r.TonDauTriGia,
                        NhapSL = r.NhapSl,
                        NhapTT = r.NhapTriGia,
                        XuatSL = r.XuatSl,
                        XuatTT = r.XuatTriGia,
                        TonCuoiSL = r.TonCuoiSl,
                        TonCuoiTT = r.TonCuoiTriGia
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine("GetBaoCaoTongHopXntChiTietAsync error: " + ex.Message); }
            return list;
        }

        public async Task<List<BaoCaoTheKhoItem>> GetBaoCaoTheKhoAsync(DateTime tuNgay, DateTime denNgay, string khoId = "", string mhId = "")
        {
            var list = new List<BaoCaoTheKhoItem>();
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // 1. Lấy thông tin mặt hàng
                string mhSql = "SELECT MA, NAME, DONVITINH, COALESCE(GIAVON, GIANHAP, 0) as GiaVon FROM DMATHANG WHERE ID = @MhId";
                var mhInfo = await conn.QueryFirstOrDefaultAsync(mhSql, new { MhId = mhId });
                string maH = mhInfo?.MA?.ToString() ?? "";
                string dvt = mhInfo?.DONVITINH?.ToString() ?? "";
                decimal gVon = Convert.ToDecimal(mhInfo?.GIAVON ?? 0);

                // 2. Tính tồn đầu kỳ
                string tonDauSql = @"
                    SELECT 
                        d.LOAI as Loai,
                        ct.SOLUONG as SoLuong,
                        ct.SOLUONGNHAP as SoLuongNhap,
                        ct.SOLUONGXUAT as SoLuongXuat,
                        d.KHOXUATID as KhoXuatId,
                        d.KHONHAPID as KhoNhapId
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    WHERE ct.MATHANGID = @MhId
                      AND d.NGAY < @TuNgay";

                var priorRows = await conn.QueryAsync(tonDauSql, new { MhId = mhId, TuNgay = tuNgay.Date });
                decimal tonDau = 0;
                foreach (var r in priorRows)
                {
                    int loai = Convert.ToInt32(r.LOAI ?? 0);
                    decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                    decimal slN = Convert.ToDecimal(r.SOLUONGNHAP ?? 0);
                    decimal slX = Convert.ToDecimal(r.SOLUONGXUAT ?? 0);
                    string kn = r.KHONHAPID?.ToString() ?? "";
                    string kx = r.KHOXUATID?.ToString() ?? "";

                    bool matchKn = string.IsNullOrEmpty(khoId) || kn == khoId;
                    bool matchKx = string.IsNullOrEmpty(khoId) || kx == khoId;

                    if (loai == 1 && matchKn) tonDau += (slN > 0 ? slN : 1);
                    else if (loai == 2 && matchKx) tonDau -= (slX > 0 ? slX : slN > 0 ? slN : 1);
                    else if (loai == 3)
                    {
                        if (matchKn && !matchKx) tonDau += (slN > 0 ? slN : slX);
                        else if (matchKx && !matchKn) tonDau -= (slX > 0 ? slX : slN);
                    }
                    else if (loai == 4)
                    {
                        if (matchKn) tonDau += slN;
                        if (matchKx) tonDau -= slX;
                    }
                    else if ((loai == 0 || loai == 10 || loai == 11) && matchKx)
                    {
                        tonDau -= (slX > 0 ? slX : slN > 0 ? slN : 1);
                    }
                }

                // Dòng Tồn đầu kỳ
                decimal currentTon = tonDau;
                list.Add(new BaoCaoTheKhoItem
                {
                    Ngay = tuNgay,
                    SoPhieu = "",
                    DienGiai = "Số dư đầu kỳ",
                    MaHang = maH,
                    DVT = dvt,
                    DonGia = gVon,
                    NhapSL = 0,
                    XuatSL = 0,
                    TonSL = currentTon,
                    ThanhTienTon = currentTon * gVon
                });

                // 3. Lấy phát sinh trong kỳ
                string psSql = @"
                    SELECT 
                        d.NGAY as Ngay,
                        d.SOPHIEU as SoPhieu,
                        d.LOAI as Loai,
                        COALESCE(d.GHICHU, ct.GHICHU, '') as DienGiai,
                        ct.DONGIA as DonGia,
                        ct.SOLUONG as SoLuong,
                        ct.SOLUONGNHAP as SoLuongNhap,
                        ct.SOLUONGXUAT as SoLuongXuat,
                        d.KHOXUATID as KhoXuatId,
                        d.KHONHAPID as KhoNhapId
                    FROM TDONHANG d
                    JOIN TDONHANGCHITIET ct ON d.ID = ct.DONHANGID
                    WHERE ct.MATHANGID = @MhId
                      AND d.NGAY >= @TuNgay AND d.NGAY <= @DenNgay
                    ORDER BY d.NGAY, d.SOPHIEU";

                var psRows = await conn.QueryAsync(psSql, new { MhId = mhId, TuNgay = tuNgay.Date, DenNgay = denNgay.Date });
                foreach (var r in psRows)
                {
                    int loai = Convert.ToInt32(r.LOAI ?? 0);
                    decimal sl = Convert.ToDecimal(r.SOLUONG ?? 0);
                    decimal slN = Convert.ToDecimal(r.SOLUONGNHAP ?? 0);
                    decimal slX = Convert.ToDecimal(r.SOLUONGXUAT ?? 0);
                    decimal dGia = Convert.ToDecimal(r.DONGIA ?? gVon);
                    string kn = r.KHONHAPID?.ToString() ?? "";
                    string kx = r.KHOXUATID?.ToString() ?? "";
                    string dg = r.DIENGIAI?.ToString() ?? "";

                    bool matchKn = string.IsNullOrEmpty(khoId) || kn == khoId;
                    bool matchKx = string.IsNullOrEmpty(khoId) || kx == khoId;

                    decimal nhap = 0;
                    decimal xuat = 0;

                    if (loai == 1 && matchKn) { nhap = slN > 0 ? slN : 1; if (string.IsNullOrEmpty(dg)) dg = "Nhập mua hàng"; }
                    else if (loai == 2 && matchKx) { xuat = slX > 0 ? slX : slN > 0 ? slN : 1; if (string.IsNullOrEmpty(dg)) dg = "Xuất kho khác"; }
                    else if (loai == 3)
                    {
                        if (matchKn && !matchKx) { nhap = slN > 0 ? slN : slX; if (string.IsNullOrEmpty(dg)) dg = "Nhận chuyển kho"; }
                        else if (matchKx && !matchKn) { xuat = slX > 0 ? slX : slN; if (string.IsNullOrEmpty(dg)) dg = "Xuất chuyển kho"; }
                    }
                    else if (loai == 4)
                    {
                        if (matchKn && slN > 0) { nhap = slN; if (string.IsNullOrEmpty(dg)) dg = "Kiểm kê tăng"; }
                        if (matchKx && slX > 0) { xuat = slX; if (string.IsNullOrEmpty(dg)) dg = "Kiểm kê giảm"; }
                    }
                    else if ((loai == 0 || loai == 10 || loai == 11) && matchKx)
                    {
                        xuat = slX > 0 ? slX : slN > 0 ? slN : 1;
                        if (string.IsNullOrEmpty(dg)) dg = "Xuất bán hàng";
                    }

                    if (nhap > 0 || xuat > 0)
                    {
                        currentTon += (nhap - xuat);
                        list.Add(new BaoCaoTheKhoItem
                        {
                            Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : (DateTime?)null,
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            DienGiai = dg,
                            MaHang = maH,
                            DVT = dvt,
                            DonGia = dGia,
                            NhapSL = nhap,
                            XuatSL = xuat,
                            TonSL = currentTon,
                            ThanhTienTon = currentTon * (dGia > 0 ? dGia : gVon)
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("GetBaoCaoTheKhoAsync error: " + ex.Message); }
            return list;
        }
        #endregion
        #endregion
    }
}
