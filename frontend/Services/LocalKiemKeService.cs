using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class PhieuKiemKeItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt { get => _stt; set { _stt = value; OnPropertyChanged(nameof(Stt)); } }

        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string NgayHienThi => Ngay?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public string KhoHangId { get; set; } = "";
        public string TenKhoHang { get; set; } = "";

        public string DienGiai { get; set; } = "";
        public string NhanVienId { get; set; } = "";
        public string TenNhanVien { get; set; } = "";
        public string Note { get; set; } = "";

        public string CuaHangId { get; set; } = "";
        public string TenCuaHang { get; set; } = "";

        public decimal ConLai { get; set; }
        public string ConLaiFormatted => ConLai != 0 ? ConLai.ToString("N0") : "";

        public decimal ThanhToan { get; set; }
        public string ThanhToanFormatted => ThanhToan != 0 ? ThanhToan.ToString("N0") : "";

        public string TaiKhoanNganHangId { get; set; } = "";
        public string TenTaiKhoanNganHang { get; set; } = "";

        public string MaVoucher { get; set; } = "";
        public decimal TheTt { get; set; }
        public string TheTtFormatted => TheTt != 0 ? TheTt.ToString("N0") : "";

        public decimal TheTraTruoc { get; set; }
        public string TheTraTruocFormatted => TheTraTruoc != 0 ? TheTraTruoc.ToString("N0") : "";

        public decimal DiemGiam { get; set; }
        public string DiemGiamFormatted => DiemGiam != 0 ? DiemGiam.ToString("N0") : "";

        public decimal TienMat { get; set; }
        public string TienMatFormatted => TienMat != 0 ? TienMat.ToString("N0") : "";

        public decimal The { get; set; }
        public string TheFormatted => The != 0 ? The.ToString("N0") : "";

        public string Ban { get; set; } = "";
        public DateTime? BatDau { get; set; }
        public string BatDauHienThi => BatDau?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public DateTime? KetThuc { get; set; }
        public string KetThucHienThi => KetThuc?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public decimal TiLeGiamGiaGio { get; set; }
        public decimal TienGiamGiaGio { get; set; }
        public string TienGiamGiaGioFormatted => TienGiamGiaGio != 0 ? TienGiamGiaGio.ToString("N0") : "";

        public int SoKhach { get; set; }
        public decimal TiLeGiamGiaTong { get; set; }
        public decimal TienGiamGiaTong { get; set; }
        public string TienGiamGiaTongFormatted => TienGiamGiaTong != 0 ? TienGiamGiaTong.ToString("N0") : "";

        public string SoOrder { get; set; } = "";
        public string SoHoaDon { get; set; } = "";
        public string SoThanhToan { get; set; } = "";
        public int SoLanInTamTinh { get; set; }
        public decimal DonGia { get; set; }
        public string DonGiaFormatted => DonGia != 0 ? DonGia.ToString("N0") : "";

        public string BangGiaId { get; set; } = "";
        public string TenBangGia { get; set; } = "";

        public decimal TienGioPhongCuoi { get; set; }
        public DateTime? BatDauPhongCuoi { get; set; }
        public decimal TienMoBan { get; set; }
        public int LanInHoaDon { get; set; }
        public int PhutKhuyenMai { get; set; }
        public DateTime? InTamTinhLuc { get; set; }
        public string InTamTinhLucHienThi => InTamTinhLuc?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public string DatTruoc { get; set; } = "";
        public decimal TienHangChuaGiam { get; set; }
        public string TienHangChuaGiamFormatted => TienHangChuaGiam != 0 ? TienHangChuaGiam.ToString("N0") : "";

        public decimal GiamGiaMatHang { get; set; }
        public string GiamGiaMatHangFormatted => GiamGiaMatHang != 0 ? GiamGiaMatHang.ToString("N0") : "";

        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string PassWifi { get; set; } = "";

        public decimal TongCong { get; set; }
        public string TongCongFormatted => TongCong != 0 ? TongCong.ToString("N0") : "0";

        public int Status { get; set; } = 30;

        public string UserCreatedName { get; set; } = "Admin";
        public DateTime? TimeCreated { get; set; }
        public string TimeCreatedHienThi => TimeCreated?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";

        public string UserModifiedName { get; set; } = "Admin";
        public DateTime? TimeModified { get; set; }
        public string TimeModifiedHienThi => TimeModified?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class PhieuKiemKeChiTietItem : INotifyPropertyChanged
    {
        private int _stt;
        public int Stt 
        { 
            get => _stt; 
            set 
            { 
                _stt = value; 
                OnPropertyChanged(nameof(Stt)); 
                OnPropertyChanged(nameof(SttHienThi)); 
            } 
        }
        public string SttHienThi => Stt.ToString("D3");

        public string Id { get; set; } = "";
        public string TdonhangId { get; set; } = "";
        public string DmathangId { get; set; } = "";
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";
        public string TenDonViTinh { get; set; } = "";

        private decimal _soLuongTon;
        public decimal SoLuongTon 
        { 
            get => _soLuongTon; 
            set 
            { 
                _soLuongTon = value; 
                OnPropertyChanged(nameof(SoLuongTon));
                OnPropertyChanged(nameof(SoLuongHeThong));
                TinhLaiThanhTien();
            } 
        }
        public decimal SoLuongHeThong 
        { 
            get => _soLuongTon; 
            set => SoLuongTon = value; 
        }

        private decimal _soLuongThucTe;
        public decimal SoLuongThucTe 
        { 
            get => _soLuongThucTe; 
            set 
            { 
                _soLuongThucTe = value; 
                OnPropertyChanged(nameof(SoLuongThucTe));
                TinhLaiThanhTien();
            } 
        }

        public decimal SoLuongChenhLech => _soLuongThucTe - _soLuongTon;
        public string SoLuongChenhLechFormatted => SoLuongChenhLech.ToString("N2");

        public decimal DieuChinhNhap => SoLuongChenhLech > 0 ? SoLuongChenhLech : 0;
        public string DieuChinhNhapFormatted => DieuChinhNhap != 0 ? DieuChinhNhap.ToString("N2") : "";

        public decimal DieuChinhXuat => SoLuongChenhLech < 0 ? Math.Abs(SoLuongChenhLech) : 0;
        public string DieuChinhXuatFormatted => DieuChinhXuat != 0 ? DieuChinhXuat.ToString("N2") : "0";

        public bool IsKhongKhop => _soLuongThucTe != _soLuongTon;
        public string RowForeground => IsKhongKhop ? "#cc0000" : "#111111";

        private decimal _donGia;
        public decimal DonGia 
        { 
            get => _donGia; 
            set 
            { 
                _donGia = value; 
                OnPropertyChanged(nameof(DonGia));
                TinhLaiThanhTien();
            } 
        }

        private decimal _thanhTien;
        public decimal ThanhTien 
        { 
            get => _thanhTien; 
            set 
            { 
                _thanhTien = value; 
                OnPropertyChanged(nameof(ThanhTien));
                OnPropertyChanged(nameof(ThanhTienFormatted));
            } 
        }
        public string ThanhTienFormatted => _thanhTien.ToString("N0");

        public string GhiChu { get; set; } = "";

        public decimal TiLeGiamGia { get; set; } = 0;
        public decimal TienGiamGia { get; set; } = 0;
        public string KichThuoc { get; set; } = "";
        public DateTime? HanSuDung { get; set; }
        public string HanSuDungFormatted => HanSuDung?.ToString("dd/MM/yyyy") ?? "";
        public string NhanVien1 { get; set; } = "";
        public string NhanVien2 { get; set; } = "";
        public string NhanVien3 { get; set; } = "";
        public string TuGio { get; set; } = "";
        public string DenGio { get; set; } = "";
        public decimal GiaVon { get; set; } = 0;
        public string GiaVonFormatted => GiaVon > 0 ? GiaVon.ToString("N0") : "";
        public string ComboSl { get; set; } = "";
        public string TrangThaiCheBien { get; set; } = "";
        public string GioTinhLuong { get; set; } = "";

        private void TinhLaiThanhTien()
        {
            ThanhTien = SoLuongChenhLech * DonGia;
            OnPropertyChanged(nameof(SoLuongChenhLech));
            OnPropertyChanged(nameof(SoLuongChenhLechFormatted));
            OnPropertyChanged(nameof(DieuChinhNhap));
            OnPropertyChanged(nameof(DieuChinhNhapFormatted));
            OnPropertyChanged(nameof(DieuChinhXuat));
            OnPropertyChanged(nameof(DieuChinhXuatFormatted));
            OnPropertyChanged(nameof(IsKhongKhop));
            OnPropertyChanged(nameof(RowForeground));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public static class LocalKiemKeService
    {
        public static async Task<Dictionary<string, decimal>> GetTonKhoDictionaryAsync(string khoId)
        {
            var dict = new Dictionary<string, decimal>();
            if (string.IsNullOrEmpty(khoId)) return dict;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(c.DMATHANGID AS VARCHAR(50)) as MatHangId,
                            SUM(
                                CASE 
                                    WHEN (d.LOAI = 1 AND (CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId OR CAST(c.DKHOHANGID AS VARCHAR(50)) = @KhoId)) THEN COALESCE(c.SLNHAP, 0)
                                    WHEN (d.LOAI = 3 AND CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId) THEN COALESCE(c.SLNHAP, c.SLXUAT, 0)
                                    WHEN (d.LOAI = 4 AND (CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId OR CAST(c.DKHOHANGID AS VARCHAR(50)) = @KhoId)) THEN COALESCE(c.SLNHAP, 0)
                                    
                                    WHEN (d.LOAI = 2 AND (CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoId OR CAST(c.DKHOHANGID AS VARCHAR(50)) = @KhoId)) THEN -COALESCE(c.SLXUAT, c.SLNHAP, 0)
                                    WHEN (d.LOAI = 0 AND (CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoId OR CAST(c.DKHOHANGID AS VARCHAR(50)) = @KhoId)) THEN -COALESCE(c.SLXUAT, c.SLNHAP, 1)
                                    WHEN (d.LOAI = 3 AND CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoId) THEN -COALESCE(c.SLXUAT, c.SLNHAP, 0)
                                    WHEN (d.LOAI = 4 AND (CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoId OR CAST(c.DKHOHANGID AS VARCHAR(50)) = @KhoId)) THEN -COALESCE(c.SLXUAT, 0)
                                    
                                    ELSE 0
                                END
                            ) AS TonKho
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        WHERE (d.STATUS IS NULL OR d.STATUS <> 0) AND (c.STATUS IS NULL OR c.STATUS <> 0)
                        GROUP BY c.DMATHANGID";

                    var rows = await conn.QueryAsync(sql, new { KhoId = khoId });
                    foreach (var r in rows)
                    {
                        string mhId = r.MATHANGID?.ToString() ?? "";
                        decimal ton = r.TONKHO != null ? Convert.ToDecimal(r.TONKHO) : 0;
                        if (!string.IsNullOrEmpty(mhId))
                        {
                            dict[mhId] = ton;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTonKhoDictionaryAsync error: " + ex.Message);
            }
            return dict;
        }

        public static async Task<List<PhieuKiemKeItem>> GetPhieuKiemKeListAsync(
            DateTime? tuNgay = null,
            DateTime? denNgay = null,
            string khoHangId = null,
            string nhanVienId = null,
            string cuaHangId = null,
            string taiKhoanNganHangId = null,
            string bangGiaId = null,
            int status = 30)
        {
            var list = new List<PhieuKiemKeItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            d.ID,
                            d.NAME as SoPhieu,
                            d.NGAY,
                            d.DKHONHAPID,
                            d.DKHOXUATID,
                            COALESCE(khon.NAME, khox.NAME, '') as TenKhoHang,
                            d.DIENGIAI,
                            d.DNHANVIENNHAPID,
                            d.DNHANVIENXUATID,
                            COALESCE(nvn.NAME, nvx.NAME, '') as TenNhanVien,
                            d.NOTE,
                            d.DCUAHANGID,
                            ch.NAME as TenCuaHang,
                            COALESCE(d.CONLAI, 0) as ConLai,
                            COALESCE(d.THANHTOAN, 0) as ThanhToan,
                            d.DTAIKHOANNGANHANGID,
                            tk.NAME as TenTaiKhoanNganHang,
                            d.VOUCHER as MaVoucher,
                            COALESCE(d.THE, 0) as TheTt,
                            COALESCE(d.THETRATRUOC, 0) as TheTraTruoc,
                            COALESCE(d.DIEMGIAM, 0) as DiemGiam,
                            COALESCE(d.TIENMAT, 0) as TienMat,
                            COALESCE(d.THE, 0) as The,
                            b.NAME as TenBan,
                            d.BATDAU,
                            d.KETTHUC,
                            COALESCE(d.TILEGIAMGIAGIO, 0) as TiLeGiamGiaGio,
                            COALESCE(d.TIENGIAMGIAGIO, 0) as TienGiamGiaGio,
                            COALESCE(d.SOKHACH, 0) as SoKhach,
                            COALESCE(d.TILEGIAMGIA, 0) as TiLeGiamGiaTong,
                            COALESCE(d.TIENGIAMGIA, 0) as TienGiamGiaTong,
                            d.SOORDER,
                            d.SOHD as SoHoaDon,
                            d.SOTT as SoThanhToan,
                            COALESCE(d.SOLANINTAMTINH, 0) as SoLanInTamTinh,
                            COALESCE(d.DONGIA, 0) as DonGia,
                            d.DBANGGIAID,
                            bg.NAME as TenBangGia,
                            COALESCE(d.TIENGIOPHONGCUOI, 0) as TienGioPhongCuoi,
                            d.BATDAUPHONGCUOI,
                            COALESCE(d.TIENMOBAN, 0) as TienMoBan,
                            COALESCE(d.LANINHOADON, 0) as LanInHoaDon,
                            COALESCE(d.PHUTKHUYENMAI, 0) as PhutKhuyenMai,
                            d.INTAMTINHLUC,
                            d.DATTRUOC,
                            COALESCE(d.TIENHANG, 0) as TienHangChuaGiam,
                            COALESCE(d.GIAMGIAMATHANG, 0) as GiamGiaMatHang,
                            COALESCE(d.TILEKHUYENMAIPHUTDAU, 0) as TiLeKhuyenMaiPhutDau,
                            d.PASSWIFI,
                            COALESCE(d.TONGCONG, 0) as TongCong,
                            d.STATUS,
                            u1.NAME as UserCreated,
                            d.TIMECREATED,
                            u2.NAME as UserModified,
                            d.TIMEMODIFIED
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG khon ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(khon.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG khox ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(khox.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nvn ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nvn.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nvx ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nvx.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(d.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                        LEFT JOIN DBANGGIA bg ON CAST(d.DBANGGIAID AS VARCHAR(50)) = CAST(bg.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u1 ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u1.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u2 ON CAST(d.USERMODIFIEDID AS VARCHAR(50)) = CAST(u2.ID AS VARCHAR(50))
                        WHERE d.LOAI = 4 AND (d.STATUS = @Status OR (@Status = 0 AND d.STATUS = 0))";

                    var dynParams = new DynamicParameters();
                    dynParams.Add("Status", status);

                    if (tuNgay.HasValue)
                    {
                        sql += " AND d.NGAY >= @TuNgay";
                        dynParams.Add("TuNgay", tuNgay.Value.Date);
                    }
                    if (denNgay.HasValue)
                    {
                        sql += " AND d.NGAY < @DenNgay";
                        dynParams.Add("DenNgay", denNgay.Value.Date.AddDays(1));
                    }

                    if (!string.IsNullOrEmpty(khoHangId) && khoHangId != "ALL" && khoHangId != "UNASSIGNED" && khoHangId != "TRASH")
                    {
                        sql += " AND (CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoHangId OR CAST(d.DKHOXUATID AS VARCHAR(50)) = @KhoHangId)";
                        dynParams.Add("KhoHangId", khoHangId);
                    }

                    if (!string.IsNullOrEmpty(nhanVienId))
                    {
                        sql += " AND (CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NhanVienId OR CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = @NhanVienId)";
                        dynParams.Add("NhanVienId", nhanVienId);
                    }

                    if (!string.IsNullOrEmpty(cuaHangId))
                    {
                        sql += " AND CAST(d.DCUAHANGID AS VARCHAR(50)) = @CuaHangId";
                        dynParams.Add("CuaHangId", cuaHangId);
                    }

                    if (!string.IsNullOrEmpty(taiKhoanNganHangId))
                    {
                        sql += " AND CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) = @TaiKhoanNganHangId";
                        dynParams.Add("TaiKhoanNganHangId", taiKhoanNganHangId);
                    }

                    if (!string.IsNullOrEmpty(bangGiaId))
                    {
                        sql += " AND CAST(d.DBANGGIAID AS VARCHAR(50)) = @BangGiaId";
                        dynParams.Add("BangGiaId", bangGiaId);
                    }

                    sql += " ORDER BY d.NGAY DESC, d.TIMECREATED DESC";

                    var rows = (await conn.QueryAsync(sql, dynParams)).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? dtNgay = null;
                        if (r.NGAY != null) { try { dtNgay = Convert.ToDateTime(r.NGAY); } catch { } }

                        DateTime? dtBatDau = null;
                        if (r.BATDAU != null) { try { dtBatDau = Convert.ToDateTime(r.BATDAU); } catch { } }

                        DateTime? dtKetThuc = null;
                        if (r.KETTHUC != null) { try { dtKetThuc = Convert.ToDateTime(r.KETTHUC); } catch { } }

                        DateTime? dtInTamTinh = null;
                        if (r.INTAMTINHLUC != null) { try { dtInTamTinh = Convert.ToDateTime(r.INTAMTINHLUC); } catch { } }

                        DateTime? dtTimeCreated = null;
                        if (r.TIMECREATED != null) { try { dtTimeCreated = Convert.ToDateTime(r.TIMECREATED); } catch { } }

                        DateTime? dtTimeModified = null;
                        if (r.TIMEMODIFIED != null) { try { dtTimeModified = Convert.ToDateTime(r.TIMEMODIFIED); } catch { } }

                        list.Add(new PhieuKiemKeItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = dtNgay,
                            KhoHangId = r.DKHOHANGID?.ToString() ?? r.DKHONHAPID?.ToString() ?? r.DKHOXUATID?.ToString() ?? "",
                            TenKhoHang = r.TENKHOHANG?.ToString() ?? "",
                            DienGiai = r.DIENGIAI?.ToString() ?? "",
                            NhanVienId = r.DNHANVIENID?.ToString() ?? r.DNHANVIENNHAPID?.ToString() ?? r.DNHANVIENXUATID?.ToString() ?? "",
                            TenNhanVien = r.TENNHANVIEN?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            CuaHangId = r.DCUAHANGID?.ToString() ?? "",
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            ConLai = r.CONLAI != null ? Convert.ToDecimal(r.CONLAI) : 0,
                            ThanhToan = r.THANHTOAN != null ? Convert.ToDecimal(r.THANHTOAN) : 0,
                            TaiKhoanNganHangId = r.DTAIKHOANNGANHANGID?.ToString() ?? "",
                            TenTaiKhoanNganHang = r.TENTAIKHOANNGANHANG?.ToString() ?? "",
                            MaVoucher = r.MAVOUCHER?.ToString() ?? "",
                            TheTt = r.THETT != null ? Convert.ToDecimal(r.THETT) : 0,
                            TheTraTruoc = r.THETRATRUOC != null ? Convert.ToDecimal(r.THETRATRUOC) : 0,
                            DiemGiam = r.DIEMGIAM != null ? Convert.ToDecimal(r.DIEMGIAM) : 0,
                            TienMat = r.TIENMAT != null ? Convert.ToDecimal(r.TIENMAT) : 0,
                            The = r.THE != null ? Convert.ToDecimal(r.THE) : 0,
                            Ban = r.TENBAN?.ToString() ?? "",
                            BatDau = dtBatDau,
                            KetThuc = dtKetThuc,
                            TiLeGiamGiaGio = r.TILEGIAMGIAGIO != null ? Convert.ToDecimal(r.TILEGIAMGIAGIO) : 0,
                            TienGiamGiaGio = r.TIENGIAMGIAGIO != null ? Convert.ToDecimal(r.TIENGIAMGIAGIO) : 0,
                            SoKhach = r.SOKHACH != null ? Convert.ToInt32(r.SOKHACH) : 0,
                            TiLeGiamGiaTong = r.TILEGIAMGIATONG != null ? Convert.ToDecimal(r.TILEGIAMGIATONG) : 0,
                            TienGiamGiaTong = r.TIENGIAMGIATONG != null ? Convert.ToDecimal(r.TIENGIAMGIATONG) : 0,
                            SoOrder = r.SOORDER?.ToString() ?? "",
                            SoHoaDon = r.SOHOADON?.ToString() ?? "",
                            SoThanhToan = r.SOTHANHTOAN?.ToString() ?? "",
                            SoLanInTamTinh = r.SOLANINTAMTINH != null ? Convert.ToInt32(r.SOLANINTAMTINH) : 0,
                            DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                            BangGiaId = r.DBANGGIAID?.ToString() ?? "",
                            TenBangGia = r.TENBANGGIA?.ToString() ?? "",
                            TienGioPhongCuoi = r.TIENGIOPHONGCUOI != null ? Convert.ToDecimal(r.TIENGIOPHONGCUOI) : 0,
                            TienMoBan = r.TIENMOBAN != null ? Convert.ToDecimal(r.TIENMOBAN) : 0,
                            LanInHoaDon = r.LANINHOADON != null ? Convert.ToInt32(r.LANINHOADON) : 0,
                            PhutKhuyenMai = r.PHUTKHUYENMAI != null ? Convert.ToInt32(r.PHUTKHUYENMAI) : 0,
                            InTamTinhLuc = dtInTamTinh,
                            DatTruoc = r.DATTRUOC?.ToString() ?? "",
                            TienHangChuaGiam = r.TIENHANGCHUAGIAM != null ? Convert.ToDecimal(r.TIENHANGCHUAGIAM) : 0,
                            GiamGiaMatHang = r.GIAMGIAMATHANG != null ? Convert.ToDecimal(r.GIAMGIAMATHANG) : 0,
                            TiLeKhuyenMaiPhutDau = r.TILEKHUYENMAIPHUTDAU != null ? Convert.ToDecimal(r.TILEKHUYENMAIPHUTDAU) : 0,
                            PassWifi = r.PASSWIFI?.ToString() ?? "",
                            TongCong = r.TONGCONG != null ? Convert.ToDecimal(r.TONGCONG) : 0,
                            Status = r.STATUS != null ? Convert.ToInt32(r.STATUS) : 30,
                            UserCreatedName = r.USERCREATED?.ToString() ?? "Admin",
                            TimeCreated = dtTimeCreated,
                            UserModifiedName = r.USERMODIFIED?.ToString() ?? "Admin",
                            TimeModified = dtTimeModified
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPhieuKiemKeListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<PhieuKiemKeChiTietItem>> GetPhieuKiemKeChiTietAsync(string donHangId)
        {
            var list = new List<PhieuKiemKeChiTietItem>();
            if (string.IsNullOrEmpty(donHangId)) return list;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            c.ID,
                            c.TDONHANGID,
                            c.DMATHANGID,
                            COALESCE(m.CODE, '') as MaHang,
                            COALESCE(c.TENHANG, m.NAME, '') as TenHang,
                            c.DDONVITINHID,
                            COALESCE(dvt.NAME, '') as TenDonViTinh,
                            COALESCE(c.SLHETHONG, c.SLNHAP, 0) as SoLuongHeThong,
                            COALESCE(c.SLTHUCTE, c.SLXUAT, 0) as SoLuongThucTe,
                            COALESCE(c.SLNHAP, 0) as DieuChinhNhap,
                            COALESCE(c.SLXUAT, 0) as DieuChinhXuat,
                            COALESCE(c.DONGIA, 0) as DonGia,
                            COALESCE(c.THANHTIEN, 0) as ThanhTien,
                            c.NOTE as GhiChu
                        FROM TDONHANGCHITIET c
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(c.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @DonHangId AND (c.STATUS IS NULL OR c.STATUS <> 0)
                        ORDER BY c.ID";

                    var rows = (await conn.QueryAsync(sql, new { DonHangId = donHangId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new PhieuKiemKeChiTietItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            TdonhangId = r.TDONHANGID?.ToString() ?? "",
                            DmathangId = r.DMATHANGID?.ToString() ?? "",
                            MaHang = r.MAHANG?.ToString() ?? "",
                            TenHang = r.TENHANG?.ToString() ?? "",
                            DdonvitinhId = r.DDONVITINHID?.ToString() ?? "",
                            TenDonViTinh = r.TENDONVITINH?.ToString() ?? "",
                            SoLuongTon = r.SOLUONGHETHONG != null ? Convert.ToDecimal(r.SOLUONGHETHONG) : 0,
                            SoLuongHeThong = r.SOLUONGHETHONG != null ? Convert.ToDecimal(r.SOLUONGHETHONG) : 0,
                            SoLuongThucTe = r.SOLUONGTHUCTE != null ? Convert.ToDecimal(r.SOLUONGTHUCTE) : 0,
                            DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                            ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0,
                            GhiChu = r.GHICHU?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPhieuKiemKeChiTietAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<string> GenerateSoPhieuAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string yearSuffix = DateTime.Now.ToString("yy");
                    string prefix = $"PKK{yearSuffix}/";

                    string sql = @"
                        SELECT NAME 
                        FROM TDONHANG 
                        WHERE LOAI = 4 AND NAME STARTING WITH @Prefix 
                        ORDER BY NAME DESC";

                    var names = (await conn.QueryAsync<string>(sql, new { Prefix = prefix })).ToList();

                    int maxNum = 0;
                    foreach (var n in names)
                    {
                        if (n.Length > prefix.Length)
                        {
                            string numPart = n.Substring(prefix.Length);
                            if (int.TryParse(numPart, out int num))
                            {
                                if (num > maxNum) maxNum = num;
                            }
                        }
                    }

                    int nextNum = maxNum + 1;
                    return $"{prefix}{nextNum:D5}";
                }
            }
            catch
            {
                return $"PKK{DateTime.Now:yy/00001}";
            }
        }

        public static async Task<bool> SavePhieuKiemKeAsync(PhieuKiemKeItem phieu, List<PhieuKiemKeChiTietItem> chiTietList)
        {
            if (phieu == null) return false;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string userId = await conn.QueryFirstOrDefaultAsync<string>(
                                "SELECT FIRST 1 CAST(ID AS VARCHAR(50)) FROM SUSER WHERE STATUS = 30 ORDER BY TIMECREATED", transaction: trans) ?? Guid.NewGuid().ToString();

                            bool isNew = string.IsNullOrEmpty(phieu.Id);
                            if (isNew)
                            {
                                phieu.Id = Guid.NewGuid().ToString();
                            }

                            string checkSql = "SELECT COUNT(*) FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @Id";
                            int exists = await conn.ExecuteScalarAsync<int>(checkSql, new { Id = phieu.Id }, trans);

                            if (exists == 0)
                            {
                                string insertSql = @"
                                    INSERT INTO TDONHANG (
                                        ID, NAME, NGAY, LOAI, STATUS,
                                        DKHONHAPID, DKHOXUATID, DNHANVIENNHAPID, DNHANVIENXUATID,
                                        DIENGIAI, NOTE, DCUAHANGID, DTAIKHOANNGANHANGID, DBANGGIAID,
                                        TONGCONG, TIENHANG, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @Id, @SoPhieu, @Ngay, 4, 30,
                                        @KhoHangId, @KhoHangId, @NhanVienId, @NhanVienId,
                                        @DienGiai, @Note, @CuaHangId, @TaiKhoanNganHangId, @BangGiaId,
                                        @TongCong, @TienHangChuaGiam, @UserCreatedId, @TimeCreated
                                    )";

                                await conn.ExecuteAsync(insertSql, new
                                {
                                    Id = phieu.Id,
                                    SoPhieu = phieu.SoPhieu,
                                    Ngay = phieu.Ngay ?? DateTime.Now,
                                    KhoHangId = string.IsNullOrEmpty(phieu.KhoHangId) ? null : phieu.KhoHangId,
                                    NhanVienId = string.IsNullOrEmpty(phieu.NhanVienId) ? null : phieu.NhanVienId,
                                    DienGiai = phieu.DienGiai,
                                    Note = phieu.Note,
                                    CuaHangId = string.IsNullOrEmpty(phieu.CuaHangId) ? null : phieu.CuaHangId,
                                    TaiKhoanNganHangId = string.IsNullOrEmpty(phieu.TaiKhoanNganHangId) ? null : phieu.TaiKhoanNganHangId,
                                    BangGiaId = string.IsNullOrEmpty(phieu.BangGiaId) ? null : phieu.BangGiaId,
                                    TongCong = phieu.TongCong,
                                    TienHangChuaGiam = phieu.TienHangChuaGiam,
                                    UserCreatedId = userId,
                                    TimeCreated = DateTime.Now
                                }, trans);
                            }
                            else
                            {
                                string updateSql = @"
                                    UPDATE TDONHANG SET
                                        NAME = @SoPhieu,
                                        NGAY = @Ngay,
                                        DKHONHAPID = @KhoHangId,
                                        DKHOXUATID = @KhoHangId,
                                        DNHANVIENNHAPID = @NhanVienId,
                                        DNHANVIENXUATID = @NhanVienId,
                                        DIENGIAI = @DienGiai,
                                        NOTE = @Note,
                                        DCUAHANGID = @CuaHangId,
                                        DTAIKHOANNGANHANGID = @TaiKhoanNganHangId,
                                        DBANGGIAID = @BangGiaId,
                                        TONGCONG = @TongCong,
                                        TIENHANG = @TienHangChuaGiam,
                                        USERMODIFIEDID = @UserModifiedId,
                                        TIMEMODIFIED = @TimeModified
                                    WHERE CAST(ID AS VARCHAR(50)) = @Id";

                                await conn.ExecuteAsync(updateSql, new
                                {
                                    Id = phieu.Id,
                                    SoPhieu = phieu.SoPhieu,
                                    Ngay = phieu.Ngay ?? DateTime.Now,
                                    KhoHangId = string.IsNullOrEmpty(phieu.KhoHangId) ? null : phieu.KhoHangId,
                                    NhanVienId = string.IsNullOrEmpty(phieu.NhanVienId) ? null : phieu.NhanVienId,
                                    DienGiai = phieu.DienGiai,
                                    Note = phieu.Note,
                                    CuaHangId = string.IsNullOrEmpty(phieu.CuaHangId) ? null : phieu.CuaHangId,
                                    TaiKhoanNganHangId = string.IsNullOrEmpty(phieu.TaiKhoanNganHangId) ? null : phieu.TaiKhoanNganHangId,
                                    BangGiaId = string.IsNullOrEmpty(phieu.BangGiaId) ? null : phieu.BangGiaId,
                                    TongCong = phieu.TongCong,
                                    TienHangChuaGiam = phieu.TienHangChuaGiam,
                                    UserModifiedId = userId,
                                    TimeModified = DateTime.Now
                                }, trans);

                                await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = phieu.Id }, trans);
                            }

                            if (chiTietList != null && chiTietList.Count > 0)
                            {
                                string insertDetailSql = @"
                                    INSERT INTO TDONHANGCHITIET (
                                        ID, TDONHANGID, DMATHANGID, TENHANG, DDONVITINHID,
                                        SLNHAP, SLXUAT, SLHETHONG, SLTHUCTE, DONGIA, THANHTIEN, NOTE, STATUS, DKHOHANGID, TIMECREATED, USERCREATEDID
                                    ) VALUES (
                                        @Id, @TdonhangId, @DmathangId, @TenHang, @DdonvitinhId,
                                        @DieuChinhNhap, @DieuChinhXuat, @SoLuongHeThong, @SoLuongThucTe, @DonGia, @ThanhTien, @GhiChu, 30, @KhoHangId, @TimeCreated, @UserCreatedId
                                    )";

                                foreach (var ct in chiTietList)
                                {
                                    await conn.ExecuteAsync(insertDetailSql, new
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        TdonhangId = phieu.Id,
                                        DmathangId = string.IsNullOrEmpty(ct.DmathangId) ? null : ct.DmathangId,
                                        TenHang = ct.TenHang,
                                        DdonvitinhId = string.IsNullOrEmpty(ct.DdonvitinhId) ? null : ct.DdonvitinhId,
                                        DieuChinhNhap = ct.DieuChinhNhap,
                                        DieuChinhXuat = ct.DieuChinhXuat,
                                        SoLuongHeThong = ct.SoLuongHeThong,
                                        SoLuongThucTe = ct.SoLuongThucTe,
                                        DonGia = ct.DonGia,
                                        ThanhTien = ct.ThanhTien,
                                        GhiChu = ct.GhiChu,
                                        KhoHangId = string.IsNullOrEmpty(phieu.KhoHangId) ? null : phieu.KhoHangId,
                                        TimeCreated = DateTime.Now,
                                        UserCreatedId = userId
                                    }, trans);
                                }
                            }

                            trans.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            Console.WriteLine("SavePhieuKiemKeAsync transaction error: " + ex.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SavePhieuKiemKeAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeletePhieuKiemKeAsync(string id, bool isTrash = false)
        {
            if (string.IsNullOrEmpty(id)) return false;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    if (!isTrash)
                    {
                        await conn.ExecuteAsync("UPDATE TDONHANG SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    else
                    {
                        await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = id });
                        await conn.ExecuteAsync("DELETE FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeletePhieuKiemKeAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestorePhieuKiemKeAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    await conn.ExecuteAsync("UPDATE TDONHANG SET STATUS = 30 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    await conn.ExecuteAsync("UPDATE TDONHANGCHITIET SET STATUS = 30 WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = id });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RestorePhieuKiemKeAsync error: " + ex.Message);
                return false;
            }
        }
    }
}
