using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class PhieuXuatItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        
        public DateTime? Ngay { get; set; }
        public string NgayHienThi => Ngay?.ToString("dd/MM/yyyy") ?? "";
        
        public string DkhoXuatId { get; set; } = "";
        public string TenKhoXuat { get; set; } = "";
        
        public string DnhanVienXuatId { get; set; } = "";
        public string TenNhanVienXuat { get; set; } = "";
        public bool HasNhanVienXuat => !string.IsNullOrWhiteSpace(TenNhanVienXuat);

        public string DnhaCungCapId { get; set; } = "";
        public string TenNhaCungCap { get; set; } = "";
        public bool HasNhaCungCap => !string.IsNullOrWhiteSpace(TenNhaCungCap);
        
        public decimal TongCong { get; set; }
        public string TongCongFormatted => TongCong.ToString("N0");

        public string SoPhieu { get; set; } = "";

        public decimal TienGiamGia { get; set; }
        public string TienGiamGiaFormatted => TienGiamGia.ToString("N0");

        public decimal TiLeGiamGia { get; set; }
        public string TiLeGiamGiaFormatted => TiLeGiamGia.ToString("N0");

        public decimal TienHang { get; set; }
        public string TienHangFormatted => TienHang.ToString("N0");

        public string DcuaHangId { get; set; } = "";
        public string TenCuaHang { get; set; } = "";

        public decimal ConLai { get; set; }
        public string ConLaiFormatted => ConLai.ToString("N0");

        public decimal ThanhToan { get; set; }
        public string ThanhToanFormatted => ThanhToan.ToString("N0");

        public string DtaiKhoanNganHangId { get; set; } = "";
        public string TenTaiKhoanNganHang { get; set; } = "";

        public string MaVoucher { get; set; } = "";
        public string Voucher { get; set; } = "";

        public decimal TheTt { get; set; }
        public string TheTtFormatted => TheTt.ToString("N0");

        public decimal TheTraTruoc { get; set; }
        public string TheTraTruocFormatted => TheTraTruoc.ToString("N0");

        public decimal DiemGiam { get; set; }
        public string DiemGiamFormatted => DiemGiam.ToString("N0");

        public decimal TienMat { get; set; }
        public string TienMatFormatted => TienMat.ToString("N0");

        public decimal The { get; set; }
        public string TheFormatted => The.ToString("N0");

        public string DbanId { get; set; } = "";
        public string TenBan { get; set; } = "";

        public DateTime? BatDau { get; set; }
        public string BatDauHienThi => BatDau?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public DateTime? KetThuc { get; set; }
        public string KetThucHienThi => KetThuc?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public decimal TiLeGiamGiaGio { get; set; }
        public string TiLeGiamGiaGioFormatted => TiLeGiamGiaGio.ToString("N0");

        public decimal TienGiamGiaGio { get; set; }
        public string TienGiamGiaGioFormatted => TienGiamGiaGio.ToString("N0");

        public decimal SoKhach { get; set; }
        public string SoKhachFormatted => SoKhach.ToString("N0");

        public decimal TiLeGiamGiaTong { get; set; }
        public string TiLeGiamGiaTongFormatted => TiLeGiamGiaTong.ToString("N0");

        public decimal TienGiamGiaTong { get; set; }
        public string TienGiamGiaTongFormatted => TienGiamGiaTong.ToString("N0");

        public string SoOrder { get; set; } = "0";
        public string SoHd { get; set; } = "0";
        public string SoThanhToan { get; set; } = "0";
        public string SoLanInTamTinh { get; set; } = "0";

        public decimal DonGia { get; set; }
        public string DonGiaFormatted => DonGia.ToString("N0");

        public string DbangGiaId { get; set; } = "";
        public string TenBangGia { get; set; } = "";

        public decimal TienGioPhongCuoi { get; set; }
        public string TienGioPhongCuoiFormatted => TienGioPhongCuoi.ToString("N0");

        public DateTime? BatDauPhongCuoi { get; set; }
        public string BatDauPhongCuoiHienThi => BatDauPhongCuoi?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public decimal TienMoBan { get; set; }
        public string TienMoBanFormatted => TienMoBan.ToString("N0");

        public string LanInHoaDon { get; set; } = "0";

        public decimal PhutKhuyenMai { get; set; }
        public string PhutKhuyenMaiFormatted => PhutKhuyenMai.ToString("N0");

        public DateTime? InTamTinhLuc { get; set; }
        public string InTamTinhLucHienThi => InTamTinhLuc?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public decimal DatTruoc { get; set; }
        public string DatTruocFormatted => DatTruoc.ToString("N0");

        public decimal TienHangChuaGiam { get; set; }
        public string TienHangChuaGiamFormatted => TienHangChuaGiam.ToString("N0");

        public decimal GiamGiaMatHang { get; set; }
        public string GiamGiaMatHangFormatted => GiamGiaMatHang.ToString("N0");

        public decimal TiLeKhuyenMaiPhutDau { get; set; }
        public string TiLeKhuyenMaiPhutDauFormatted => TiLeKhuyenMaiPhutDau.ToString("N0");

        public string PassWifi { get; set; } = "";

        public string DkhachHangId { get; set; } = "";
        public string TenKhachHang { get; set; } = "";

        public string Note { get; set; } = "";
        public int Status { get; set; } = 30;

        public string UserCreatedId { get; set; } = "";
        public string UserCreatedName { get; set; } = "";
        public DateTime? TimeCreated { get; set; }
        public string TimeCreatedHienThi => TimeCreated?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
        public string UserModifiedId { get; set; } = "";
        public string UserModifiedName { get; set; } = "";
        public DateTime? TimeModified { get; set; }
        public string TimeModifiedHienThi => TimeModified?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
    }

    public class PhieuXuatChiTietItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _stt;
        public int Stt
        {
            get => _stt;
            set { if (_stt != value) { _stt = value; OnPropertyChanged(); } }
        }

        public string Id { get; set; } = "";
        public string TdonhangId { get; set; } = "";
        public string DmathangId { get; set; } = "";
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";
        public string TenDonViTinh { get; set; } = "";

        private decimal _slXuat = 1;
        public decimal SlXuat
        {
            get => _slXuat;
            set
            {
                if (_slXuat != value)
                {
                    _slXuat = value;
                    Recalculate();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _donGia;
        public decimal DonGia
        {
            get => _donGia;
            set
            {
                if (_donGia != value)
                {
                    _donGia = value;
                    Recalculate();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _tiLeGiamGia;
        public decimal TiLeGiamGia
        {
            get => _tiLeGiamGia;
            set
            {
                if (_tiLeGiamGia != value)
                {
                    _tiLeGiamGia = value;
                    Recalculate();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _tienGiamGia;
        public decimal TienGiamGia
        {
            get => _tienGiamGia;
            set
            {
                if (_tienGiamGia != value)
                {
                    _tienGiamGia = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _thanhTien;
        public decimal ThanhTien
        {
            get => _thanhTien;
            set
            {
                if (_thanhTien != value)
                {
                    _thanhTien = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GhiChu { get; set; } = "";

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

        public string SlXuatFormatted => SlXuat.ToString("N0");
        public string DonGiaFormatted => DonGia.ToString("N0");
        public string TienGiamGiaFormatted => TienGiamGia.ToString("N0");
        public string ThanhTienFormatted => ThanhTien.ToString("N0");

        public void Recalculate()
        {
            decimal tienGoc = SlXuat * DonGia;
            if (TiLeGiamGia > 0)
            {
                TienGiamGia = tienGoc * (TiLeGiamGia / 100m);
            }
            ThanhTien = tienGoc - TienGiamGia;
        }
    }

    public static class LocalXuatKhoService
    {
        public static async Task<List<PhieuXuatItem>> GetPhieuXuatListAsync(
            DateTime? tuNgay = null,
            DateTime? denNgay = null,
            string khoId = null,
            string nhanVienId = null,
            bool isTrash = false)
        {
            var list = new List<PhieuXuatItem>();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(d.ID AS VARCHAR(50)) as Id,
                            d.NAME as SoPhieu,
                            d.NGAY as Ngay,
                            CAST(d.DKHOXUATID AS VARCHAR(50)) as DkhoXuatId,
                            k.NAME as TenKhoXuat,
                            CAST(d.DNHANVIENXUATID AS VARCHAR(50)) as DnhanVienXuatId,
                            nv.NAME as TenNhanVienXuat,
                            d.TONGCONG as TongCong,
                            d.TIENGIAMGIA as TienGiamGia,
                            d.TILEGIAMGIA as TiLeGiamGia,
                            d.TIENHANG as TienHang,
                            CAST(d.DCUAHANGID AS VARCHAR(50)) as DcuaHangId,
                            ch.NAME as TenCuaHang,
                            d.CONLAI as ConLai,
                            d.THANHTOAN as ThanhToan,
                            CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) as DtaiKhoanNganHangId,
                            tk.NAME as TenTaiKhoanNganHang,
                            d.VOUCHER as MaVoucher,
                            d.THETRATRUOC as TheTt,
                            d.THETRATRUOC as TheTraTruoc,
                            d.DIEMGIAM as DiemGiam,
                            d.TIENMAT as TienMat,
                            d.THE as The,
                            CAST(d.DBANID AS VARCHAR(50)) as DbanId,
                            b.NAME as TenBan,
                            d.BATDAU as BatDau,
                            d.KETTHUC as KetThuc,
                            d.TILEGIAMGIAGIO as TiLeGiamGiaGio,
                            d.TIENGIAMGIAGIO as TienGiamGiaGio,
                            d.SOKHACH as SoKhach,
                            d.TILEGIAMGIATONG as TiLeGiamGiaTong,
                            d.TIENGIAMGIATONG as TienGiamGiaTong,
                            d.SOORDER as SoOrder,
                            d.SOHD as SoHd,
                            d.SOTT as SoThanhToan,
                            d.SOLANINTAMTINH as SoLanInTamTinh,
                            d.DONGIA as DonGia,
                            CAST(d.DBANGGIAID AS VARCHAR(50)) as DbangGiaId,
                            bg.NAME as TenBangGia,
                            d.TIENGIOPHONGCUOI as TienGioPhongCuoi,
                            d.BATDAUPHONGCUOI as BatDauPhongCuoi,
                            d.TIENMOBAN as TienMoBan,
                            d.LANINHOADON as LanInHoaDon,
                            d.PHUTKHUYENMAI as PhutKhuyenMai,
                            d.INTAMTINHLUC as InTamTinhLuc,
                            d.DATTRUOC as DatTruoc,
                            d.TIENHANGCHUAGIAM as TienHangChuaGiam,
                            d.GIAMGIAMATHANG as GiamGiaMatHang,
                            d.TILEKHUYENMAIPHUTDAU as TiLeKhuyenMaiPhutDau,
                            d.PASSWIFI as PassWifi,
                            CAST(d.DKHACHHANGID AS VARCHAR(50)) as DkhachHangId,
                            kh.NAME as TenKhachHang,
                            d.NOTE as Note,
                            d.STATUS as Status,
                            CAST(d.USERCREATEDID AS VARCHAR(50)) as UserCreatedId,
                            u1.NAME as UserCreatedName,
                            d.TIMECREATED as TimeCreated,
                            CAST(d.USERMODIFIEDID AS VARCHAR(50)) as UserModifiedId,
                            u2.NAME as UserModifiedName,
                            d.TIMEMODIFIED as TimeModified
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(d.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DBANGGIA bg ON CAST(d.DBANGGIAID AS VARCHAR(50)) = CAST(bg.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u1 ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u1.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u2 ON CAST(d.USERMODIFIEDID AS VARCHAR(50)) = CAST(u2.ID AS VARCHAR(50))
                        WHERE d.LOAI = 2 AND (d.STATUS = @Status OR (@Status = 0 AND d.STATUS = 0))
                        ORDER BY d.NGAY DESC, d.TIMECREATED DESC";

                    int statusVal = isTrash ? 0 : 30;
                    var rows = (await conn.QueryAsync(sql, new { Status = statusVal })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;

                        if (tuNgay.HasValue && ngay.HasValue && ngay.Value.Date < tuNgay.Value.Date) continue;
                        if (denNgay.HasValue && ngay.HasValue && ngay.Value.Date > denNgay.Value.Date) continue;

                        string rowKhoId = r.DKHOXUATID?.ToString();
                        string rowNvId = r.DNHANVIENXUATID?.ToString();

                        if (!string.IsNullOrEmpty(khoId) && khoId != "ALL" && khoId != "UNASSIGNED" && khoId != "TRASH")
                        {
                            if (rowKhoId != khoId) continue;
                        }
                        else if (khoId == "UNASSIGNED")
                        {
                            if (!string.IsNullOrEmpty(rowKhoId)) continue;
                        }

                        if (!string.IsNullOrEmpty(nhanVienId) && rowNvId != nhanVienId) continue;

                        list.Add(new PhieuXuatItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString(),
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = ngay,
                            DkhoXuatId = rowKhoId ?? "",
                            TenKhoXuat = r.TENKHOXUAT?.ToString() ?? "",
                            DnhanVienXuatId = rowNvId ?? "",
                            TenNhanVienXuat = r.TENNHANVIENXUAT?.ToString() ?? "",
                            TongCong = r.TONGCONG != null ? Convert.ToDecimal(r.TONGCONG) : 0,
                            TienGiamGia = r.TIENGIAMGIA != null ? Convert.ToDecimal(r.TIENGIAMGIA) : 0,
                            TiLeGiamGia = r.TILEGIAMGIA != null ? Convert.ToDecimal(r.TILEGIAMGIA) : 0,
                            TienHang = r.TIENHANG != null ? Convert.ToDecimal(r.TIENHANG) : 0,
                            DcuaHangId = r.DCUAHANGID?.ToString() ?? "",
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            ConLai = r.CONLAI != null ? Convert.ToDecimal(r.CONLAI) : 0,
                            ThanhToan = r.THANHTOAN != null ? Convert.ToDecimal(r.THANHTOAN) : 0,
                            DtaiKhoanNganHangId = r.DTAIKHOANNGANHANGID?.ToString() ?? "",
                            TenTaiKhoanNganHang = r.TENTAIKHOANNGANHANG?.ToString() ?? "",
                            MaVoucher = r.MAVOUCHER?.ToString() ?? "",
                            Voucher = r.MAVOUCHER?.ToString() ?? "",
                            TheTt = r.THETT != null ? Convert.ToDecimal(r.THETT) : 0,
                            TheTraTruoc = r.THETRATRUOC != null ? Convert.ToDecimal(r.THETRATRUOC) : 0,
                            DiemGiam = r.DIEMGIAM != null ? Convert.ToDecimal(r.DIEMGIAM) : 0,
                            TienMat = r.TIENMAT != null ? Convert.ToDecimal(r.TIENMAT) : 0,
                            The = r.THE != null ? Convert.ToDecimal(r.THE) : 0,
                            DbanId = r.DBANID?.ToString() ?? "",
                            TenBan = r.TENBAN?.ToString() ?? "",
                            BatDau = r.BATDAU != null ? Convert.ToDateTime(r.BATDAU) : null,
                            KetThuc = r.KETTHUC != null ? Convert.ToDateTime(r.KETTHUC) : null,
                            TiLeGiamGiaGio = r.TILEGIAMGIAGIO != null ? Convert.ToDecimal(r.TILEGIAMGIAGIO) : 0,
                            TienGiamGiaGio = r.TIENGIAMGIAGIO != null ? Convert.ToDecimal(r.TIENGIAMGIAGIO) : 0,
                            SoKhach = r.SOKHACH != null ? Convert.ToDecimal(r.SOKHACH) : 0,
                            TiLeGiamGiaTong = r.TILEGIAMGIATONG != null ? Convert.ToDecimal(r.TILEGIAMGIATONG) : 0,
                            TienGiamGiaTong = r.TIENGIAMGIATONG != null ? Convert.ToDecimal(r.TIENGIAMGIATONG) : 0,
                            SoOrder = r.SOORDER?.ToString() ?? "0",
                            SoHd = r.SOHD?.ToString() ?? "0",
                            SoThanhToan = r.SOTHANHTOAN?.ToString() ?? "0",
                            SoLanInTamTinh = r.SOLANINTAMTINH?.ToString() ?? "0",
                            DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                            DbangGiaId = r.DBANGGIAID?.ToString() ?? "",
                            TenBangGia = r.TENBANGGIA?.ToString() ?? "",
                            TienGioPhongCuoi = r.TIENGIOPHONGCUOI != null ? Convert.ToDecimal(r.TIENGIOPHONGCUOI) : 0,
                            BatDauPhongCuoi = r.BATDAUPHONGCUOI != null ? Convert.ToDateTime(r.BATDAUPHONGCUOI) : null,
                            TienMoBan = r.TIENMOBAN != null ? Convert.ToDecimal(r.TIENMOBAN) : 0,
                            LanInHoaDon = r.LANINHOADON?.ToString() ?? "0",
                            PhutKhuyenMai = r.PHUTKHUYENMAI != null ? Convert.ToDecimal(r.PHUTKHUYENMAI) : 0,
                            InTamTinhLuc = r.INTAMTINHLUC != null ? Convert.ToDateTime(r.INTAMTINHLUC) : null,
                            DatTruoc = r.DATTRUOC != null ? Convert.ToDecimal(r.DATTRUOC) : 0,
                            TienHangChuaGiam = r.TIENHANGCHUAGIAM != null ? Convert.ToDecimal(r.TIENHANGCHUAGIAM) : 0,
                            GiamGiaMatHang = r.GIAMGIAMATHANG != null ? Convert.ToDecimal(r.GIAMGIAMATHANG) : 0,
                            TiLeKhuyenMaiPhutDau = r.TILEKHUYENMAIPHUTDAU != null ? Convert.ToDecimal(r.TILEKHUYENMAIPHUTDAU) : 0,
                            PassWifi = r.PASSWIFI?.ToString() ?? "",
                            DkhachHangId = r.DKHACHHANGID?.ToString() ?? "",
                            TenKhachHang = r.TENKHACHHANG?.ToString() ?? "",
                            Note = r.NOTE?.ToString() ?? "",
                            Status = r.STATUS != null ? Convert.ToInt32(r.STATUS) : 30,
                            UserCreatedId = r.USERCREATEDID?.ToString() ?? "",
                            UserCreatedName = r.USERCREATEDNAME?.ToString() ?? "",
                            TimeCreated = r.TIMECREATED != null ? Convert.ToDateTime(r.TIMECREATED) : null,
                            UserModifiedId = r.USERMODIFIEDID?.ToString() ?? "",
                            UserModifiedName = r.USERMODIFIEDNAME?.ToString() ?? "",
                            TimeModified = r.TIMEMODIFIED != null ? Convert.ToDateTime(r.TIMEMODIFIED) : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPhieuXuatListAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<PhieuXuatChiTietItem>> GetPhieuXuatChiTietAsync(string tdonhangId)
        {
            var list = new List<PhieuXuatChiTietItem>();
            if (string.IsNullOrEmpty(tdonhangId)) return list;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(c.ID AS VARCHAR(50)) as Id,
                            CAST(c.TDONHANGID AS VARCHAR(50)) as TdonhangId,
                            CAST(c.DMATHANGID AS VARCHAR(50)) as DmathangId,
                            m.CODE as MaHang,
                            COALESCE(c.TENHANG, m.NAME) as TenHang,
                            CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) as DdonvitinhId,
                            dvt.NAME as TenDonViTinh,
                            COALESCE(c.SLXUAT, c.SLXUATCHUAQUYDOI, 0) as SlXuat,
                            c.DONGIA as DonGia,
                            c.TILEGIAMGIA as TiLeGiamGia,
                            c.TIENGIAMGIA as TienGiamGia,
                            c.THANHTIEN as ThanhTien,
                            c.NOTE as GhiChu
                        FROM TDONHANGCHITIET c
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE CAST(c.TDONHANGID AS VARCHAR(50)) = @TdonhangId
                        ORDER BY c.TIMECREATED, c.ID";

                    var rows = (await conn.QueryAsync(sql, new { TdonhangId = tdonhangId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new PhieuXuatChiTietItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString(),
                            TdonhangId = r.TDONHANGID?.ToString() ?? "",
                            DmathangId = r.DMATHANGID?.ToString() ?? "",
                            MaHang = r.MAHANG?.ToString() ?? "",
                            TenHang = r.TENHANG?.ToString() ?? "",
                            DdonvitinhId = r.DDONVITINHID?.ToString() ?? "",
                            TenDonViTinh = r.TENDONVITINH?.ToString() ?? "",
                            SlXuat = r.SLXUAT != null ? Convert.ToDecimal(r.SLXUAT) : 1,
                            DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                            TiLeGiamGia = r.TILEGIAMGIA != null ? Convert.ToDecimal(r.TILEGIAMGIA) : 0,
                            TienGiamGia = r.TIENGIAMGIA != null ? Convert.ToDecimal(r.TIENGIAMGIA) : 0,
                            ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0,
                            GhiChu = r.GHICHU?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPhieuXuatChiTietAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<string> GetNextSoPhieuXuatAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string prefix = $"PX{DateTime.Now:yy}/";
                    string sql = @"
                        SELECT FIRST 1 NAME 
                        FROM TDONHANG 
                        WHERE LOAI = 2 AND NAME STARTING WITH @Prefix 
                        ORDER BY NAME DESC";

                    string lastCode = await conn.QueryFirstOrDefaultAsync<string>(sql, new { Prefix = prefix });
                    if (!string.IsNullOrEmpty(lastCode))
                    {
                        string[] parts = lastCode.Split('/');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int num))
                        {
                            return $"{prefix}{(num + 1):D5}";
                        }
                    }

                    return $"{prefix}00001";
                }
            }
            catch
            {
                return $"PX{DateTime.Now:yy}/00001";
            }
        }

        public static async Task<(bool Success, string Message, string Id)> SavePhieuXuatAsync(
            PhieuXuatItem item,
            List<PhieuXuatChiTietItem> details,
            bool isNew)
        {
            if (item == null) return (false, "Dữ liệu phiếu xuất rỗng!", "");

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string userId = null;
                    try
                    {
                        userId = await conn.QueryFirstOrDefaultAsync<string>("SELECT FIRST 1 CAST(ID AS VARCHAR(50)) FROM SUSER WHERE STATUS = 30");
                    }
                    catch { }

                    if (isNew || string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = Guid.NewGuid().ToString();
                        if (string.IsNullOrWhiteSpace(item.SoPhieu))
                        {
                            item.SoPhieu = await GetNextSoPhieuXuatAsync();
                        }

                        string sqlInsert = @"
                            INSERT INTO TDONHANG (
                                ID, NAME, NGAY, LOAI, STATUS,
                                DKHOXUATID, DNHANVIENXUATID, DNHACUNGCAPID,
                                TIENHANG, TIENGIAMGIA, TILEGIAMGIA,
                                DTAIKHOANNGANHANGID, TONGCONG, DCUAHANGID, NOTE,
                                USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @SoPhieu, @Ngay, 2, 30,
                                @DkhoXuatId, @DnhanVienXuatId, @DnhaCungCapId,
                                @TienHang, @TienGiamGia, @TiLeGiamGia,
                                @DtaiKhoanNganHangId, @TongCong, @DcuaHangId, @Note,
                                @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(sqlInsert, new
                        {
                            Id = item.Id,
                            SoPhieu = item.SoPhieu.Trim(),
                            Ngay = item.Ngay ?? DateTime.Now,
                            DkhoXuatId = string.IsNullOrEmpty(item.DkhoXuatId) ? null : item.DkhoXuatId,
                            DnhanVienXuatId = string.IsNullOrEmpty(item.DnhanVienXuatId) ? null : item.DnhanVienXuatId,
                            DnhaCungCapId = string.IsNullOrEmpty(item.DnhaCungCapId) ? null : item.DnhaCungCapId,
                            TienHang = item.TienHang,
                            TienGiamGia = item.TienGiamGia,
                            TiLeGiamGia = item.TiLeGiamGia,
                            DtaiKhoanNganHangId = string.IsNullOrEmpty(item.DtaiKhoanNganHangId) ? null : item.DtaiKhoanNganHangId,
                            TongCong = item.TongCong,
                            DcuaHangId = string.IsNullOrEmpty(item.DcuaHangId) ? null : item.DcuaHangId,
                            Note = item.Note,
                            UserCreatedId = userId
                        });
                    }
                    else
                    {
                        string sqlUpdate = @"
                            UPDATE TDONHANG SET
                                NAME = @SoPhieu,
                                NGAY = @Ngay,
                                DKHOXUATID = @DkhoXuatId,
                                DNHANVIENXUATID = @DnhanVienXuatId,
                                DNHACUNGCAPID = @DnhaCungCapId,
                                TIENHANG = @TienHang,
                                TIENGIAMGIA = @TienGiamGia,
                                TILEGIAMGIA = @TiLeGiamGia,
                                DTAIKHOANNGANHANGID = @DtaiKhoanNganHangId,
                                TONGCONG = @TongCong,
                                DCUAHANGID = @DcuaHangId,
                                NOTE = @Note,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(sqlUpdate, new
                        {
                            Id = item.Id,
                            SoPhieu = item.SoPhieu.Trim(),
                            Ngay = item.Ngay ?? DateTime.Now,
                            DkhoXuatId = string.IsNullOrEmpty(item.DkhoXuatId) ? null : item.DkhoXuatId,
                            DnhanVienXuatId = string.IsNullOrEmpty(item.DnhanVienXuatId) ? null : item.DnhanVienXuatId,
                            DnhaCungCapId = string.IsNullOrEmpty(item.DnhaCungCapId) ? null : item.DnhaCungCapId,
                            TienHang = item.TienHang,
                            TienGiamGia = item.TienGiamGia,
                            TiLeGiamGia = item.TiLeGiamGia,
                            DtaiKhoanNganHangId = string.IsNullOrEmpty(item.DtaiKhoanNganHangId) ? null : item.DtaiKhoanNganHangId,
                            TongCong = item.TongCong,
                            DcuaHangId = string.IsNullOrEmpty(item.DcuaHangId) ? null : item.DcuaHangId,
                            Note = item.Note,
                            UserModifiedId = userId
                        });

                        await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = item.Id });
                    }

                    // Insert detail rows
                    foreach (var d in details)
                    {
                        string sqlDetail = @"
                            INSERT INTO TDONHANGCHITIET (
                                ID, TDONHANGID, DMATHANGID, SLXUAT, SLXUATCHUAQUYDOI, DONGIA,
                                TILEGIAMGIA, TIENGIAMGIA, THANHTIEN, NOTE, STATUS,
                                DKHOHANGID, DDONVITINHID, TENHANG,
                                USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @TdonhangId, @DmathangId, @SlXuat, @SlXuat, @DonGia,
                                @TiLeGiamGia, @TienGiamGia, @ThanhTien, @GhiChu, 30,
                                @DkhoHangId, @DdonvitinhId, @TenHang,
                                @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(sqlDetail, new
                        {
                            Id = Guid.NewGuid().ToString(),
                            TdonhangId = item.Id,
                            DmathangId = string.IsNullOrEmpty(d.DmathangId) ? null : d.DmathangId,
                            SlXuat = d.SlXuat,
                            DonGia = d.DonGia,
                            TiLeGiamGia = d.TiLeGiamGia,
                            TienGiamGia = d.TienGiamGia,
                            ThanhTien = d.ThanhTien,
                            GhiChu = d.GhiChu,
                            DkhoHangId = string.IsNullOrEmpty(item.DkhoXuatId) ? null : item.DkhoXuatId,
                            DdonvitinhId = string.IsNullOrEmpty(d.DdonvitinhId) ? null : d.DdonvitinhId,
                            TenHang = d.TenHang,
                            UserCreatedId = userId
                        });
                    }

                    return (true, "Lưu thành công", item.Id);
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi lưu phiếu xuất: " + ex.Message, "");
            }
        }

        public static async Task<bool> DeletePhieuXuatAsync(string id, bool permanent = false)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    if (permanent)
                    {
                        await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = id });
                        await conn.ExecuteAsync("DELETE FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    else
                    {
                        await conn.ExecuteAsync("UPDATE TDONHANG SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RestorePhieuXuatAsync(string id)
        {
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
                Console.WriteLine("RestorePhieuXuatAsync error: " + ex.Message);
                return false;
            }
        }
    }
}
