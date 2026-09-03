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
    public class PhieuNhapItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string NgayHienThi => Ngay?.ToString("dd/MM/yyyy") ?? "";
        
        public string DnhacungcapId { get; set; } = "";
        public string TenNhaCungCap { get; set; } = "";
        
        public string DkhoNhapId { get; set; } = "";
        public string TenKhoNhap { get; set; } = "";
        
        public string DnhanVienNhapId { get; set; } = "";
        public string TenNhanVienNhap { get; set; } = "";
        public bool HasNhanVienNhap => !string.IsNullOrWhiteSpace(TenNhanVienNhap);
        
        public decimal TienHang { get; set; }
        public string TienHangFormatted => TienHang.ToString("N0");

        public decimal TienGiamGia { get; set; }
        public string TienGiamGiaFormatted => TienGiamGia.ToString("N0");

        public decimal TiLeGiamGia { get; set; }
        public string TiLeGiamGiaFormatted => TiLeGiamGia.ToString("N0");

        public string DtaiKhoanNganHangId { get; set; } = "";
        public string TenTaiKhoanNganHang { get; set; } = "";

        public decimal TongCong { get; set; }
        public string TongCongFormatted => TongCong.ToString("N0");

        public string DcuaHangId { get; set; } = "";
        public string TenCuaHang { get; set; } = "";

        public decimal ThanhToan { get; set; }
        public string ThanhToanFormatted => ThanhToan.ToString("N0");

        public decimal ConLai { get; set; }
        public string ConLaiFormatted => ConLai.ToString("N0");

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

    public class PhieuNhapChiTietItem : INotifyPropertyChanged
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

        private decimal _slNhap;
        public decimal SlNhap
        {
            get => _slNhap;
            set
            {
                if (_slNhap != value)
                {
                    _slNhap = value;
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
            set { if (_tienGiamGia != value) { _tienGiamGia = value; OnPropertyChanged(); } }
        }

        private decimal _thanhTien;
        public decimal ThanhTien
        {
            get => _thanhTien;
            set { if (_thanhTien != value) { _thanhTien = value; OnPropertyChanged(); } }
        }

        private decimal _giaBan;
        public decimal GiaBan
        {
            get => _giaBan;
            set { if (_giaBan != value) { _giaBan = value; OnPropertyChanged(); } }
        }

        private string _note = "";
        public string Note
        {
            get => _note;
            set { if (_note != value) { _note = value; OnPropertyChanged(); } }
        }

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

        public void Recalculate()
        {
            decimal tienGoc = _slNhap * _donGia;
            if (_tiLeGiamGia > 0)
            {
                TienGiamGia = tienGoc * (_tiLeGiamGia / 100m);
            }
            else
            {
                TienGiamGia = 0;
            }
            ThanhTien = tienGoc - TienGiamGia;
        }
    }

    public class NhapKhoLookupItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string Icon { get; set; } = "👤";
    }

    public class MatHangNhapKhoItem
    {
        public int Stt { get; set; }
        public string SttHienThi => Stt.ToString("D3");
        public string Id { get; set; } = "";
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";
        public string TenDonViTinh { get; set; } = "";
        public string DnhommathangId { get; set; } = "";
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
        public string GiaBanFormatted => GiaBan.ToString("N0");
        public decimal QuyDoi { get; set; } = 1;
        public string QuyDoiFormatted => QuyDoi > 0 ? QuyDoi.ToString("N0") : "1";
    }

    public static class LocalNhapKhoService
    {
        public static async Task<string> GetCurrentUserIdAsync(IDbConnection conn)
        {
            try
            {
                var id = await conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT FIRST 1 CAST(ID AS VARCHAR(50)) FROM SUSER WHERE STATUS = 30 ORDER BY TIMECREATED");
                return id ?? Guid.NewGuid().ToString();
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        public static async Task<List<KhoHangTreeItem>> GetKhoHangTreeAsync()
        {
            var result = new List<KhoHangTreeItem>();

            var allNode = new KhoHangTreeItem
            {
                Id = "ALL",
                Name = "Tất cả",
                ItemType = "1",
                IsExpanded = true
            };

            var unassignedNode = new KhoHangTreeItem
            {
                Id = "UNASSIGNED",
                Name = "Chưa thiết lập",
                ItemType = "0"
            };

            var trashNode = new KhoHangTreeItem
            {
                Id = "TRASH",
                Name = "Thùng rác",
                ItemType = "0"
            };

            try
            {
                var activeTree = await LocalKhoHangService.GetKhoHangTreeAsync(false);
                allNode.Children.Add(unassignedNode);
                foreach (var item in activeTree)
                {
                    allNode.Children.Add(item);
                }
                allNode.Children.Add(trashNode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetKhoHangTreeAsync error: " + ex.Message);
                allNode.Children.Add(unassignedNode);
                allNode.Children.Add(trashNode);
            }

            result.Add(allNode);
            return result;
        }

        public static async Task<List<KhoHangTreeItem>> GetCuaHangTreeAsync()
        {
            var result = new List<KhoHangTreeItem>();
            var rootAll = new KhoHangTreeItem { Id = "ALL", Name = "Tất cả", ItemType = "FOLDER", CustomIcon = "🌐" };
            var rootUnassigned = new KhoHangTreeItem { Id = "UNASSIGNED", Name = "Chưa thiết lập", ItemType = "FOLDER", CustomIcon = "✳️" };
            var rootTrash = new KhoHangTreeItem { Id = "TRASH", Name = "Thùng rác", ItemType = "FOLDER", CustomIcon = "🗑️" };

            result.Add(rootAll);
            result.Add(rootUnassigned);

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var list = (await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME FROM DCUAHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    foreach (var ch in list)
                    {
                        result.Add(new KhoHangTreeItem
                        {
                            Id = ch.ID?.ToString(),
                            Name = ch.NAME?.ToString(),
                            ItemType = "0",
                            CustomIcon = "🖥️"
                        });
                    }
                }
            }
            catch { }

            result.Add(rootTrash);
            return result;
        }

        public static async Task<List<KhoHangTreeItem>> GetNhanVienTreeAsync()
        {
            var result = new List<KhoHangTreeItem>();
            var rootAll = new KhoHangTreeItem { Id = "ALL", Name = "Tất cả", ItemType = "FOLDER", CustomIcon = "🌐" };
            var rootUnassigned = new KhoHangTreeItem { Id = "UNASSIGNED", Name = "Chưa thiết lập", ItemType = "FOLDER", CustomIcon = "✳️" };
            var rootTrash = new KhoHangTreeItem { Id = "TRASH", Name = "Thùng rác", ItemType = "FOLDER", CustomIcon = "🗑️" };

            result.Add(rootAll);
            result.Add(rootUnassigned);

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var list = (await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME FROM DNHANVIEN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    foreach (var nv in list)
                    {
                        result.Add(new KhoHangTreeItem
                        {
                            Id = nv.ID?.ToString(),
                            Name = nv.NAME?.ToString(),
                            ItemType = "0",
                            CustomIcon = "👩"
                        });
                    }
                }
            }
            catch { }

            result.Add(rootTrash);
            return result;
        }

        public static async Task<List<KhoHangTreeItem>> GetTaiKhoanNganHangTreeAsync()
        {
            var result = new List<KhoHangTreeItem>();
            var rootAll = new KhoHangTreeItem { Id = "ALL", Name = "Tất cả", ItemType = "FOLDER", CustomIcon = "🌐" };
            var rootUnassigned = new KhoHangTreeItem { Id = "UNASSIGNED", Name = "Chưa thiết lập", ItemType = "FOLDER", CustomIcon = "✳️" };
            var rootTrash = new KhoHangTreeItem { Id = "TRASH", Name = "Thùng rác", ItemType = "FOLDER", CustomIcon = "🗑️" };

            result.Add(rootAll);
            result.Add(rootUnassigned);

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var list = (await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME FROM DTAIKHOANNGANHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    foreach (var tk in list)
                    {
                        result.Add(new KhoHangTreeItem
                        {
                            Id = tk.ID?.ToString(),
                            Name = tk.NAME?.ToString(),
                            ItemType = "0",
                            CustomIcon = "🏛️"
                        });
                    }
                }
            }
            catch { }

            result.Add(rootTrash);
            return result;
        }

        public static async Task<List<KhoHangTreeItem>> GetBangGiaTreeAsync()
        {
            var result = new List<KhoHangTreeItem>();
            var rootAll = new KhoHangTreeItem { Id = "ALL", Name = "Tất cả", ItemType = "FOLDER", CustomIcon = "🌐" };
            var rootUnassigned = new KhoHangTreeItem { Id = "UNASSIGNED", Name = "Chưa thiết lập", ItemType = "FOLDER", CustomIcon = "✳️" };
            var rootTrash = new KhoHangTreeItem { Id = "TRASH", Name = "Thùng rác", ItemType = "FOLDER", CustomIcon = "🗑️" };

            result.Add(rootAll);
            result.Add(rootUnassigned);

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var list = (await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME FROM DBANGGIA WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                    foreach (var bg in list)
                    {
                        result.Add(new KhoHangTreeItem
                        {
                            Id = bg.ID?.ToString(),
                            Name = bg.NAME?.ToString(),
                            ItemType = "0",
                            CustomIcon = "📅"
                        });
                    }
                }
            }
            catch { }

            result.Add(rootTrash);
            return result;
        }

        public static async Task<List<PhieuNhapItem>> GetPhieuNhapListAsync(
            DateTime? tuNgay = null,
            DateTime? denNgay = null,
            string khoId = null,
            string nhanVienId = null,
            string nccId = null,
            string cuaHangId = null,
            string taiKhoanNganHangId = null,
            bool isTrash = false)
        {
            var list = new List<PhieuNhapItem>();

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
                            CAST(d.DNHACUNGCAPID AS VARCHAR(50)) as DnhacungcapId,
                            ncc.NAME as TenNhaCungCap,
                            CAST(d.DKHONHAPID AS VARCHAR(50)) as DkhoNhapId,
                            k.NAME as TenKhoNhap,
                            CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) as DnhanVienNhapId,
                            nv.NAME as TenNhanVienNhap,
                            d.TIENHANG as TienHang,
                            d.TIENGIAMGIA as TienGiamGia,
                            d.TILEGIAMGIA as TiLeGiamGia,
                            CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) as DtaiKhoanNganHangId,
                            tk.NAME as TenTaiKhoanNganHang,
                            d.TONGCONG as TongCong,
                            CAST(d.DCUAHANGID AS VARCHAR(50)) as DcuaHangId,
                            ch.NAME as TenCuaHang,
                            d.THANHTOAN as ThanhToan,
                            d.CONLAI as ConLai,
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
                            d.NOTE as Note,
                            d.STATUS as Status,
                            CAST(d.USERCREATEDID AS VARCHAR(50)) as UserCreatedId,
                            u1.NAME as UserCreatedName,
                            d.TIMECREATED as TimeCreated,
                            CAST(d.USERMODIFIEDID AS VARCHAR(50)) as UserModifiedId,
                            u2.NAME as UserModifiedName,
                            d.TIMEMODIFIED as TimeModified
                        FROM TDONHANG d
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(d.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                        LEFT JOIN DCUAHANG ch ON CAST(d.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DBANGGIA bg ON CAST(d.DBANGGIAID AS VARCHAR(50)) = CAST(bg.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u1 ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u1.ID AS VARCHAR(50))
                        LEFT JOIN SUSER u2 ON CAST(d.USERMODIFIEDID AS VARCHAR(50)) = CAST(u2.ID AS VARCHAR(50))
                        WHERE d.LOAI = 1 AND (d.STATUS = @Status OR (@Status = 0 AND d.STATUS = 0))
                        ORDER BY d.NGAY DESC, d.TIMECREATED DESC";

                    int statusVal = isTrash ? 0 : 30;
                    var rows = (await conn.QueryAsync(sql, new { Status = statusVal })).ToList();

                    int stt = 1;
                    foreach (var r in rows)
                    {
                        DateTime? ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null;

                        // Date filter
                        if (tuNgay.HasValue && ngay.HasValue && ngay.Value.Date < tuNgay.Value.Date) continue;
                        if (denNgay.HasValue && ngay.HasValue && ngay.Value.Date > denNgay.Value.Date) continue;

                        string rowKhoId = r.DKHONHAPID?.ToString();
                        string rowNccId = r.DNHACUNGCAPID?.ToString();
                        string rowNvId = r.DNHANVIENNHAPID?.ToString();
                        string rowChId = r.DCUAHANGID?.ToString();
                        string rowTkId = r.DTAIKHOANNGANHANGID?.ToString();

                        if (!string.IsNullOrEmpty(khoId) && khoId != "ALL" && khoId != "UNASSIGNED" && khoId != "TRASH")
                        {
                            if (rowKhoId != khoId) continue;
                        }
                        else if (khoId == "UNASSIGNED")
                        {
                            if (!string.IsNullOrEmpty(rowKhoId)) continue;
                        }

                        if (!string.IsNullOrEmpty(cuaHangId) && cuaHangId != "ALL" && cuaHangId != "UNASSIGNED" && cuaHangId != "TRASH")
                        {
                            if (rowChId != cuaHangId) continue;
                        }
                        else if (cuaHangId == "UNASSIGNED")
                        {
                            if (!string.IsNullOrEmpty(rowChId)) continue;
                        }

                        if (!string.IsNullOrEmpty(taiKhoanNganHangId) && taiKhoanNganHangId != "ALL" && taiKhoanNganHangId != "UNASSIGNED" && taiKhoanNganHangId != "TRASH")
                        {
                            if (rowTkId != taiKhoanNganHangId) continue;
                        }
                        else if (taiKhoanNganHangId == "UNASSIGNED")
                        {
                            if (!string.IsNullOrEmpty(rowTkId)) continue;
                        }

                        if (!string.IsNullOrEmpty(nccId) && rowNccId != nccId) continue;
                        if (!string.IsNullOrEmpty(nhanVienId) && rowNvId != nhanVienId) continue;

                        list.Add(new PhieuNhapItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString(),
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = ngay,
                            DnhacungcapId = rowNccId ?? "",
                            TenNhaCungCap = r.TENNHACUNGCAP?.ToString() ?? "",
                            DkhoNhapId = rowKhoId ?? "",
                            TenKhoNhap = r.TENKHONHAP?.ToString() ?? "",
                            DnhanVienNhapId = rowNvId ?? "",
                            TenNhanVienNhap = r.TENNHANVIENNHAP?.ToString() ?? "",
                            TienHang = r.TIENHANG != null ? Convert.ToDecimal(r.TIENHANG) : 0,
                            TienGiamGia = r.TIENGIAMGIA != null ? Convert.ToDecimal(r.TIENGIAMGIA) : 0,
                            TiLeGiamGia = r.TILEGIAMGIA != null ? Convert.ToDecimal(r.TILEGIAMGIA) : 0,
                            DtaiKhoanNganHangId = r.DTAIKHOANNGANHANGID?.ToString() ?? "",
                            TenTaiKhoanNganHang = r.TENTAIKHOANNGANHANG?.ToString() ?? "",
                            TongCong = r.TONGCONG != null ? Convert.ToDecimal(r.TONGCONG) : 0,
                            DcuaHangId = r.DCUAHANGID?.ToString() ?? "",
                            TenCuaHang = r.TENCUAHANG?.ToString() ?? "",
                            ThanhToan = r.THANHTOAN != null ? Convert.ToDecimal(r.THANHTOAN) : 0,
                            ConLai = r.CONLAI != null ? Convert.ToDecimal(r.CONLAI) : 0,
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
                Console.WriteLine("GetPhieuNhapListAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<List<PhieuNhapChiTietItem>> GetPhieuNhapChiTietAsync(string phieuNhapId)
        {
            var list = new List<PhieuNhapChiTietItem>();
            if (string.IsNullOrEmpty(phieuNhapId)) return list;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(d.ID AS VARCHAR(50)) as Id,
                            CAST(d.TDONHANGID AS VARCHAR(50)) as TdonhangId,
                            CAST(d.DMATHANGID AS VARCHAR(50)) as DmathangId,
                            m.CODE as MaHang,
                            COALESCE(d.TENHANG, m.NAME) as TenHang,
                            CAST(d.DDONVITINHID AS VARCHAR(50)) as DdonvitinhId,
                            dvt.NAME as TenDonViTinh,
                            d.SLNHAP as SlNhap,
                            d.DONGIA as DonGia,
                            d.TILEGIAMGIA as TiLeGiamGia,
                            d.TIENGIAMGIA as TienGiamGia,
                            d.THANHTIEN as ThanhTien,
                            d.NOTE as Note,
                            COALESCE(m.GIABAN, 0) as GiaBan
                        FROM TDONHANGCHITIET d
                        LEFT JOIN DMATHANG m ON CAST(d.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(d.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE CAST(d.TDONHANGID AS VARCHAR(50)) = @PhieuNhapId
                        ORDER BY d.TIMECREATED";

                    var rows = (await conn.QueryAsync(sql, new { PhieuNhapId = phieuNhapId })).ToList();
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new PhieuNhapChiTietItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString(),
                            TdonhangId = r.TDONHANGID?.ToString(),
                            DmathangId = r.DMATHANGID?.ToString(),
                            MaHang = r.MAHANG?.ToString() ?? "",
                            TenHang = r.TENHANG?.ToString() ?? "",
                            DdonvitinhId = r.DDONVITINHID?.ToString(),
                            TenDonViTinh = r.TENDONVITINH?.ToString() ?? "",
                            SlNhap = r.SLNHAP != null ? Convert.ToDecimal(r.SLNHAP) : 0,
                            DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                            TiLeGiamGia = r.TILEGIAMGIA != null ? Convert.ToDecimal(r.TILEGIAMGIA) : 0,
                            TienGiamGia = r.TIENGIAMGIA != null ? Convert.ToDecimal(r.TIENGIAMGIA) : 0,
                            ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0,
                            GiaBan = r.GIABAN != null ? Convert.ToDecimal(r.GIABAN) : 0,
                            Note = r.NOTE?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPhieuNhapChiTietAsync error: " + ex.Message);
            }

            return list;
        }

        public static async Task<string> GetNextSoPhieuNhapAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string yearSuffix = DateTime.Now.ToString("yy");
                    string prefix = $"PN{yearSuffix}/";

                    string sql = @"
                        SELECT NAME 
                        FROM TDONHANG 
                        WHERE LOAI = 1 AND NAME LIKE @Prefix
                        ORDER BY NAME DESC";

                    var names = (await conn.QueryAsync<string>(sql, new { Prefix = prefix + "%" })).ToList();

                    int maxNumber = 0;
                    foreach (var name in names)
                    {
                        if (name.StartsWith(prefix))
                        {
                            string numPart = name.Substring(prefix.Length);
                            if (int.TryParse(numPart, out int num))
                            {
                                if (num > maxNumber) maxNumber = num;
                            }
                        }
                    }

                    return $"{prefix}{(maxNumber + 1):D5}";
                }
            }
            catch
            {
                return $"PN{DateTime.Now:yy}/00001";
            }
        }

        public static async Task<List<NhapKhoLookupItem>> GetKhoHangListFlatAsync()
        {
            var list = new List<NhapKhoLookupItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var rows = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DKHOHANG WHERE (STATUS IS NULL OR STATUS <> 0) AND (ITEMTYPE IS NULL OR ITEMTYPE = 0) ORDER BY NAME");
                    foreach (var r in rows)
                    {
                        list.Add(new NhapKhoLookupItem { Id = r.ID?.ToString(), Name = r.NAME?.ToString() ?? "", Code = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetKhoHangListFlatAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<NhapKhoLookupItem>> GetNhaCungCapLookupListAsync()
        {
            var list = new List<NhapKhoLookupItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var rows = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name, MANHACUNGCAP as Code, DIENTHOAI as DienThoai, DIACHI as DiaChi FROM DNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                    foreach (var r in rows)
                    {
                        list.Add(new NhapKhoLookupItem { 
                            Id = r.ID?.ToString(), 
                            Name = r.NAME?.ToString() ?? "", 
                            Code = r.CODE?.ToString() ?? "",
                            DienThoai = r.DIENTHOAI?.ToString() ?? "",
                            DiaChi = r.DIACHI?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetNhaCungCapLookupListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<NhapKhoLookupItem>> GetNhanVienLookupListAsync()
        {
            var list = new List<NhapKhoLookupItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var rows = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name FROM DNHANVIEN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME");
                    foreach (var r in rows)
                    {
                        list.Add(new NhapKhoLookupItem { Id = r.ID?.ToString(), Name = r.NAME?.ToString() ?? "" });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetNhanVienLookupListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<NhapKhoLookupItem>> GetTaiKhoanNganHangLookupListAsync()
        {
            var list = new List<NhapKhoLookupItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var rows = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name, SOTAIKHOAN as Code FROM DTAIKHOANNGANHANG WHERE STATUS = 30 ORDER BY NAME");
                    foreach (var r in rows)
                    {
                        list.Add(new NhapKhoLookupItem { Id = r.ID?.ToString(), Name = r.NAME?.ToString() ?? "", Code = r.CODE?.ToString() ?? "" });
                    }
                }
            }
            catch { }
            return list;
        }

        public static async Task<List<NhapKhoLookupItem>> GetMatHangLookupListAsync()
        {
            var list = new List<NhapKhoLookupItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    var rows = await conn.QueryAsync("SELECT CAST(ID AS VARCHAR(50)) as Id, NAME as Name, CODE as Code, GIANHAP as GiaNhap FROM DMATHANG WHERE STATUS = 30 ORDER BY NAME");
                    foreach (var r in rows)
                    {
                        list.Add(new NhapKhoLookupItem { Id = r.ID?.ToString(), Name = r.NAME?.ToString() ?? "", Code = r.CODE?.ToString() ?? "" });
                    }
                }
            }
            catch { }
            return list;
        }

        public static async Task<List<MatHangNhapKhoItem>> GetMatHangForNhapKhoAsync()
        {
            var list = new List<MatHangNhapKhoItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    string sql = @"
                        SELECT 
                            CAST(m.ID AS VARCHAR(50)) as Id,
                            m.CODE as Code,
                            m.NAME as Name,
                            CAST(m.DNHOMMATHANGID AS VARCHAR(50)) as DnhommathangId,
                            CAST(m.DDONVITINHID AS VARCHAR(50)) as DdonvitinhId,
                            dvt.NAME as TenDonViTinh,
                            COALESCE(m.GIANHAP, 0) as GiaNhap,
                            COALESCE(m.GIABAN, 0) as GiaBan,
                            COALESCE(m.QUYDOI, 1) as QuyDoi
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS = 30)
                        ORDER BY m.NAME";

                    var rows = await conn.QueryAsync(sql);
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        list.Add(new MatHangNhapKhoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            Code = r.CODE?.ToString() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            DnhommathangId = r.DNHOMMATHANGID?.ToString() ?? "",
                            DdonvitinhId = r.DDONVITINHID?.ToString() ?? "",
                            TenDonViTinh = r.TENDONVITINH?.ToString() ?? "",
                            GiaNhap = r.GIANHAP != null ? Convert.ToDecimal(r.GIANHAP) : 0,
                            GiaBan = r.GIABAN != null ? Convert.ToDecimal(r.GIABAN) : 0,
                            QuyDoi = r.QUYDOI != null ? Convert.ToDecimal(r.QUYDOI) : 1
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetMatHangForNhapKhoAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<(bool Success, string Message, string Id)> SavePhieuNhapAsync(
            PhieuNhapItem item,
            List<PhieuNhapChiTietItem> details,
            bool isNew)
        {
            if (item == null) return (false, "Dữ liệu phiếu nhập rỗng!", "");

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string userId = await GetCurrentUserIdAsync(conn);

                    if (isNew || string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = Guid.NewGuid().ToString();
                        if (string.IsNullOrWhiteSpace(item.SoPhieu))
                        {
                            item.SoPhieu = await GetNextSoPhieuNhapAsync();
                        }

                        string sqlInsert = @"
                            INSERT INTO TDONHANG (
                                ID, NAME, NGAY, LOAI, STATUS,
                                DNHACUNGCAPID, DKHONHAPID, DNHANVIENNHAPID,
                                TIENHANG, TIENGIAMGIA, TILEGIAMGIA,
                                DTAIKHOANNGANHANGID, TONGCONG, DCUAHANGID, NOTE,
                                USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @SoPhieu, @Ngay, 1, 30,
                                @DnhacungcapId, @DkhoNhapId, @DnhanVienNhapId,
                                @TienHang, @TienGiamGia, @TiLeGiamGia,
                                @DtaiKhoanNganHangId, @TongCong, @DcuaHangId, @Note,
                                @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(sqlInsert, new
                        {
                            Id = item.Id,
                            SoPhieu = item.SoPhieu.Trim(),
                            Ngay = item.Ngay ?? DateTime.Now,
                            DnhacungcapId = string.IsNullOrEmpty(item.DnhacungcapId) ? null : item.DnhacungcapId,
                            DkhoNhapId = string.IsNullOrEmpty(item.DkhoNhapId) ? null : item.DkhoNhapId,
                            DnhanVienNhapId = string.IsNullOrEmpty(item.DnhanVienNhapId) ? null : item.DnhanVienNhapId,
                            TienHang = item.TienHang,
                            TienGiamGia = item.TienGiamGia,
                            TiLeGiamGia = item.TiLeGiamGia,
                            DtaiKhoanNganHangId = string.IsNullOrEmpty(item.DtaiKhoanNganHangId) ? null : item.DtaiKhoanNganHangId,
                            TongCong = item.TongCong,
                            DcuaHangId = string.IsNullOrEmpty(item.DcuaHangId) ? null : item.DcuaHangId,
                            Note = item.Note?.Trim() ?? "",
                            UserCreatedId = userId
                        });
                    }
                    else
                    {
                        string sqlUpdate = @"
                            UPDATE TDONHANG SET
                                NAME = @SoPhieu,
                                NGAY = @Ngay,
                                DNHACUNGCAPID = @DnhacungcapId,
                                DKHONHAPID = @DkhoNhapId,
                                DNHANVIENNHAPID = @DnhanVienNhapId,
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
                            DnhacungcapId = string.IsNullOrEmpty(item.DnhacungcapId) ? null : item.DnhacungcapId,
                            DkhoNhapId = string.IsNullOrEmpty(item.DkhoNhapId) ? null : item.DkhoNhapId,
                            DnhanVienNhapId = string.IsNullOrEmpty(item.DnhanVienNhapId) ? null : item.DnhanVienNhapId,
                            TienHang = item.TienHang,
                            TienGiamGia = item.TienGiamGia,
                            TiLeGiamGia = item.TiLeGiamGia,
                            DtaiKhoanNganHangId = string.IsNullOrEmpty(item.DtaiKhoanNganHangId) ? null : item.DtaiKhoanNganHangId,
                            TongCong = item.TongCong,
                            DcuaHangId = string.IsNullOrEmpty(item.DcuaHangId) ? null : item.DcuaHangId,
                            Note = item.Note?.Trim() ?? "",
                            UserModifiedId = userId
                        });

                        // Delete existing details to re-insert
                        await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = item.Id });
                    }

                    // Save line items
                    if (details != null && details.Count > 0)
                    {
                        foreach (var dt in details)
                        {
                            if (string.IsNullOrWhiteSpace(dt.DmathangId) && string.IsNullOrWhiteSpace(dt.TenHang)) continue;

                            string detailId = string.IsNullOrEmpty(dt.Id) ? Guid.NewGuid().ToString() : dt.Id;
                            string sqlDetail = @"
                                INSERT INTO TDONHANGCHITIET (
                                    ID, TDONHANGID, DMATHANGID, TENHANG, DDONVITINHID,
                                    SLNHAP, DONGIA, TILEGIAMGIA, TIENGIAMGIA, THANHTIEN,
                                    DKHOHANGID, NOTE, STATUS, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @TdonhangId, @DmathangId, @TenHang, @DdonvitinhId,
                                    @SlNhap, @DonGia, @TiLeGiamGia, @TienGiamGia, @ThanhTien,
                                    @DkhoHangId, @Note, 30, @UserCreatedId, CURRENT_TIMESTAMP
                                )";

                            await conn.ExecuteAsync(sqlDetail, new
                            {
                                Id = detailId,
                                TdonhangId = item.Id,
                                DmathangId = string.IsNullOrEmpty(dt.DmathangId) ? null : dt.DmathangId,
                                TenHang = dt.TenHang?.Trim() ?? "",
                                DdonvitinhId = string.IsNullOrEmpty(dt.DdonvitinhId) ? null : dt.DdonvitinhId,
                                SlNhap = dt.SlNhap,
                                DonGia = dt.DonGia,
                                TiLeGiamGia = dt.TiLeGiamGia,
                                TienGiamGia = dt.TienGiamGia,
                                ThanhTien = dt.ThanhTien,
                                DkhoHangId = string.IsNullOrEmpty(item.DkhoNhapId) ? null : item.DkhoNhapId,
                                Note = dt.Note?.Trim() ?? "",
                                UserCreatedId = userId
                            });
                        }
                    }

                    return (true, "", item.Id);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, "");
            }
        }

        public static async Task<bool> DeletePhieuNhapAsync(string id, bool permanent = false)
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
                        await conn.ExecuteAsync("UPDATE TDONHANGCHITIET SET STATUS = 0 WHERE CAST(TDONHANGID AS VARCHAR(50)) = @Id", new { Id = id });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeletePhieuNhapAsync error: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestorePhieuNhapAsync(string id)
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
                Console.WriteLine("RestorePhieuNhapAsync error: " + ex.Message);
                return false;
            }
        }
    }
}
