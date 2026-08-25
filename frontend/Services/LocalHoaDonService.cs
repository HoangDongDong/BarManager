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
                        h.ID as Id, 
                        h.SOHD as SoPhieu, 
                        h.NGAY as Ngay, 
                        b.NAME as Ban, 
                        h.BATDAU as BatDau, 
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
                        h.SOORDER as SoOrder, 
                        h.TIENGIAMGIAGIO as TienGiamGiaGio
                    FROM TDONHANG h
                    LEFT JOIN DBAN b ON h.DBANID = b.ID
                    LEFT JOIN DKHACHHANG k ON h.DKHACHHANGID = k.ID
                    WHERE CAST(h.NGAY AS DATE) >= @TuNgay 
                      AND CAST(h.NGAY AS DATE) <= @DenNgay
                    ORDER BY h.NGAY DESC, h.GIOTHANHTOAN DESC
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

        public async Task<List<ChiTietHoaDonViewModel>> GetChiTietHoaDonAsync(int donHangId)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                string sql = @"
                    SELECT 
                        ROW_NUMBER() OVER(ORDER BY c.ID) as Stt,
                        m.NAME as TenMon,
                        m.DVT as Dvt,
                        CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) as SoLuong,
                        CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) as DonGia,
                        CAST(COALESCE(c.TILEGIAMGIA, 0) AS DECIMAL(18,2)) as PhanTramGiamGia,
                        CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) as ThanhTien
                    FROM TDONHANGCHITIET c
                    LEFT JOIN DMATHANG m ON c.DMATHANGID = m.ID
                    WHERE c.TDONHANGID = @DonHangId
                ";
                
                var parameters = new { DonHangId = donHangId };
                var list = (await conn.QueryAsync<ChiTietHoaDonViewModel>(sql, parameters)).ToList();
                return list;
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
