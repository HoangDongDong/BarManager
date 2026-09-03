using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class KhoHangComboItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
    }

    public class TonKhoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _sttNumber = 1;
        public int SttNumber
        {
            get => _sttNumber;
            set
            {
                if (_sttNumber != value)
                {
                    _sttNumber = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Stt));
                }
            }
        }

        public string Stt => SttNumber.ToString("D2");

        public string DmathangId { get; set; } = "";
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string DnhommathangId { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";
        public string TenDonViTinh { get; set; } = "";

        private decimal _ton;
        public decimal Ton
        {
            get => _ton;
            set
            {
                if (_ton != value)
                {
                    _ton = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TonFormatted));
                    OnPropertyChanged(nameof(IsTonAm));
                    OnPropertyChanged(nameof(GiaTriBan));
                    OnPropertyChanged(nameof(GiaTriBanFormatted));
                    OnPropertyChanged(nameof(GiaTriVon));
                    OnPropertyChanged(nameof(GiaTriVonFormatted));
                }
            }
        }
        public string TonFormatted => Ton != 0 ? Ton.ToString("N0") : "0";
        public bool IsTonAm => Ton < 0;

        public string Ton2Dvt { get; set; } = "";

        private decimal _giaBan;
        public decimal GiaBan
        {
            get => _giaBan;
            set
            {
                if (_giaBan != value)
                {
                    _giaBan = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GiaBanFormatted));
                    OnPropertyChanged(nameof(GiaTriBan));
                    OnPropertyChanged(nameof(GiaTriBanFormatted));
                }
            }
        }
        public string GiaBanFormatted => GiaBan > 0 ? GiaBan.ToString("N0") : "0";

        public decimal GiaTriBan => Ton * GiaBan;
        public string GiaTriBanFormatted => GiaTriBan > 0 ? GiaTriBan.ToString("N0") : "0";

        private decimal _giaVon;
        public decimal GiaVon
        {
            get => _giaVon;
            set
            {
                if (_giaVon != value)
                {
                    _giaVon = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GiaVonFormatted));
                    OnPropertyChanged(nameof(GiaTriVon));
                    OnPropertyChanged(nameof(GiaTriVonFormatted));
                }
            }
        }
        public string GiaVonFormatted => GiaVon > 0 ? GiaVon.ToString("N0") : "0";

        public decimal GiaTriVon => Ton * GiaVon;
        public string GiaTriVonFormatted => GiaTriVon > 0 ? GiaTriVon.ToString("N0") : "0";

        public decimal QuyDoi { get; set; } = 1;
        public string QuyDoiFormatted => QuyDoi > 0 ? QuyDoi.ToString("N0") : "1";

        public string GhiChu { get; set; } = "";
    }

    public class TheKhoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Stt { get; set; } = 1;
        public string Id { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string NgayFormatted => Ngay?.ToString("dd/MM/yyyy") ?? "";
        public string DienGiai { get; set; } = "";
        public decimal DonGia { get; set; } = 0;
        public string DonGiaFormatted => DonGia > 0 ? DonGia.ToString("N0") : "0";
        public decimal ThanhTien { get; set; } = 0;
        public string ThanhTienFormatted => ThanhTien > 0 ? ThanhTien.ToString("N0") : "0";
        public decimal GiamGia { get; set; } = 0;
        public string GiamGiaFormatted => GiamGia > 0 ? GiamGia.ToString("N0") : "0";
        public string TenDonViTinh { get; set; } = "";
        public decimal SlNhap { get; set; } = 0;
        public string SlNhapFormatted => SlNhap > 0 ? SlNhap.ToString("N0") : "0";
        public decimal SlXuat { get; set; } = 0;
        public string SlXuatFormatted => SlXuat > 0 ? SlXuat.ToString("N0") : "0";
        public decimal Ton { get; set; } = 0;
        public string TonFormatted => Ton.ToString("N0");
        public string DoiTuong { get; set; } = "";
    }

    public static class LocalTonKhoService
    {
        public static async Task<List<KhoHangComboItem>> GetKhoHangListAsync()
        {
            var list = new List<KhoHangComboItem>();
            try
            {
                var tree = await LocalKhoHangService.GetKhoHangTreeAsync(false);
                void Extract(IEnumerable<KhoHangTreeItem> items)
                {
                    if (items == null) return;
                    foreach (var item in items)
                    {
                        if (!string.IsNullOrEmpty(item.Id) && item.Id != "-1")
                        {
                            list.Add(new KhoHangComboItem
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Code = item.Id
                            });
                        }
                        if (item.Children != null && item.Children.Count > 0)
                        {
                            Extract(item.Children);
                        }
                    }
                }
                Extract(tree);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetKhoHangListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<TonKhoItem>> GetTonKhoListAsync(string khoId, string nhomId = null, string searchText = null, bool chiTonKhacKhong = false)
        {
            var list = new List<TonKhoItem>();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    // 1. Tải danh mục mặt hàng
                    string sqlMatHang = @"
                        SELECT 
                            CAST(m.ID AS VARCHAR(50)) as DmathangId,
                            m.CODE as MaHang,
                            m.NAME as TenHang,
                            CAST(m.DNHOMMATHANGID AS VARCHAR(50)) as DnhommathangId,
                            CAST(m.DDONVITINHID AS VARCHAR(50)) as DdonvitinhId,
                            d.NAME as TenDonViTinh,
                            COALESCE(m.GIABAN, 0) as GiaBan,
                            COALESCE(m.GIAVON, m.GIANHAP, 0) as GiaVon,
                            COALESCE(m.QUYDOI, 1) as QuyDoi,
                            m.NOTE as GhiChu
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH d ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                        ORDER BY m.NAME";

                    var items = (await conn.QueryAsync<TonKhoItem>(sqlMatHang)).ToList();

                    // 2. Tải tồn kho hiện tại
                    var tonDict = await LocalKiemKeService.GetTonKhoDictionaryAsync(khoId);

                    int stt = 1;
                    foreach (var item in items)
                    {
                        item.Ton = tonDict.GetValueOrDefault(item.DmathangId, 0);

                        // Lọc theo nhóm
                        if (!string.IsNullOrEmpty(nhomId) && nhomId != "-1" && nhomId != "all" && item.DnhommathangId != nhomId)
                        {
                            continue;
                        }

                        // Lọc theo tìm kiếm
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            string s = searchText.ToLowerInvariant();
                            if (!(item.TenHang?.ToLowerInvariant().Contains(s) == true ||
                                  item.MaHang?.ToLowerInvariant().Contains(s) == true ||
                                  item.GhiChu?.ToLowerInvariant().Contains(s) == true))
                            {
                                continue;
                            }
                        }

                        // Lọc chỉ có tồn khác 0
                        if (chiTonKhacKhong && item.Ton == 0)
                        {
                            continue;
                        }

                        item.SttNumber = stt++;
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTonKhoListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<TheKhoItem>> GetTheKhoListAsync(string dmathangId, string khoId, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var list = new List<TheKhoItem>();
            if (string.IsNullOrEmpty(dmathangId)) return list;

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as Id,
                            COALESCE(h.NAME, h.CODE, '') as SoPhieu,
                            h.NGAY,
                            h.LOAI,
                            COALESCE(c.NOTE, h.NOTE, '') as DienGiai,
                            COALESCE(c.DONGIA, 0) as DonGia,
                            COALESCE(c.THANHTIEN, 0) as ThanhTien,
                            COALESCE(c.TILEGIAMGIA, 0) as GiamGia,
                            COALESCE(d.NAME, '') as TenDonViTinh,
                            COALESCE(c.SLNHAP, 0) as SlNhap,
                            COALESCE(c.SLXUAT, 0) as SlXuat,
                            COALESCE(k.NAME, n.NAME, b.NAME, h.BAN, '') as DoiTuong
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH d ON CAST(COALESCE(c.DDONVITINHID, m.DDONVITINHID) AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG k ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DNHACUNGCAP n ON CAST(h.DNHACUNGCAPID AS VARCHAR(50)) = CAST(n.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE CAST(c.DMATHANGID AS VARCHAR(50)) = @DmathangId
                          AND (h.STATUS IS NULL OR h.STATUS <> 0)
                          AND (c.STATUS IS NULL OR c.STATUS <> 0)
                          AND (@KhoId IS NULL OR @KhoId = '' OR CAST(h.DKHOHANGID AS VARCHAR(50)) = @KhoId OR CAST(h.DKHONHAPID AS VARCHAR(50)) = @KhoId OR CAST(h.DKHOXUATID AS VARCHAR(50)) = @KhoId OR CAST(c.DKHOHANGID AS VARCHAR(50)) = @KhoId)
                          AND (@TuNgay IS NULL OR CAST(h.NGAY AS DATE) >= CAST(@TuNgay AS DATE))
                          AND (@DenNgay IS NULL OR CAST(h.NGAY AS DATE) <= CAST(@DenNgay AS DATE))
                        ORDER BY h.NGAY, h.TIMECREATED, h.ID";

                    var rows = (await conn.QueryAsync(sql, new { DmathangId = dmathangId, KhoId = khoId, TuNgay = tuNgay, DenNgay = denNgay })).ToList();

                    decimal runningTon = 0;
                    int stt = 1;
                    foreach (var r in rows)
                    {
                        int loai = r.LOAI != null ? Convert.ToInt32(r.LOAI) : 0;
                        decimal slNhap = r.SLNHAP != null ? Convert.ToDecimal(r.SLNHAP) : 0;
                        decimal slXuat = r.SLXUAT != null ? Convert.ToDecimal(r.SLXUAT) : 0;

                        string loaiText = loai switch
                        {
                            1 => "Nhập kho",
                            2 => "Xuất kho",
                            3 => "Bán hàng",
                            4 => "Chuyển kho",
                            5 => "Kiểm kê",
                            _ => "Phiếu kho"
                        };

                        string dienGiai = r.DIENGIAI?.ToString() ?? "";
                        if (string.IsNullOrEmpty(dienGiai)) dienGiai = loaiText;

                        runningTon += (slNhap - slXuat);

                        list.Add(new TheKhoItem
                        {
                            Stt = stt++,
                            Id = r.ID?.ToString() ?? "",
                            SoPhieu = r.SOPHIEU?.ToString() ?? "",
                            Ngay = r.NGAY != null ? Convert.ToDateTime(r.NGAY) : null,
                            DienGiai = dienGiai,
                            DonGia = r.DONGIA != null ? Convert.ToDecimal(r.DONGIA) : 0,
                            ThanhTien = r.THANHTIEN != null ? Convert.ToDecimal(r.THANHTIEN) : 0,
                            GiamGia = r.GIAMGIA != null ? Convert.ToDecimal(r.GIAMGIA) : 0,
                            TenDonViTinh = r.TENDONVITINH?.ToString() ?? "",
                            SlNhap = slNhap,
                            SlXuat = slXuat,
                            Ton = runningTon,
                            DoiTuong = r.DOITUONG?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTheKhoListAsync error: " + ex.Message);
            }
            return list;
        }

        public static async Task<(List<KhoHangComboItem> KhoList, List<TonNhieuKhoItem> Items)> GetTonNhieuKhoDataAsync(string nhomId = null, string searchText = null, bool chiTonKhacKhong = false)
        {
            var khoList = await GetKhoHangListAsync();
            var items = new List<TonNhieuKhoItem>();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string sqlMatHang = @"
                        SELECT 
                            CAST(m.ID AS VARCHAR(50)) as DmathangId,
                            m.CODE as MaHang,
                            m.NAME as TenHang,
                            CAST(m.DNHOMMATHANGID AS VARCHAR(50)) as DnhommathangId,
                            CAST(m.DDONVITINHID AS VARCHAR(50)) as DdonvitinhId,
                            d.NAME as TenDonViTinh,
                            COALESCE(m.QUYDOI, 1) as QuyDoi,
                            m.NOTE as GhiChu
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH d ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                        ORDER BY m.NAME";

                    var rawItems = (await conn.QueryAsync<TonNhieuKhoItem>(sqlMatHang)).ToList();

                    // Tải tồn kho cho từng kho
                    var khoDicts = new Dictionary<string, Dictionary<string, decimal>>();
                    foreach (var kho in khoList)
                    {
                        var dict = await LocalKiemKeService.GetTonKhoDictionaryAsync(kho.Id);
                        khoDicts[kho.Id] = dict;
                    }

                    int stt = 1;
                    foreach (var item in rawItems)
                    {
                        // Gán tồn từng kho
                        foreach (var kho in khoList)
                        {
                            decimal ton = khoDicts[kho.Id].GetValueOrDefault(item.DmathangId, 0);
                            item.KhoTonDict[kho.Id] = ton;
                        }

                        // Lọc theo nhóm
                        if (!string.IsNullOrEmpty(nhomId) && nhomId != "-1" && nhomId != "all" && item.DnhommathangId != nhomId)
                        {
                            continue;
                        }

                        // Lọc theo tìm kiếm
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            string s = searchText.ToLowerInvariant();
                            if (!(item.TenHang?.ToLowerInvariant().Contains(s) == true ||
                                  item.MaHang?.ToLowerInvariant().Contains(s) == true ||
                                  item.GhiChu?.ToLowerInvariant().Contains(s) == true))
                            {
                                continue;
                            }
                        }

                        // Lọc chỉ có tồn khác 0
                        if (chiTonKhacKhong && item.TongTon == 0)
                        {
                            continue;
                        }

                        item.SttNumber = stt++;
                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTonNhieuKhoDataAsync error: " + ex.Message);
            }

            return (khoList, items);
        }
    }

    public class TonNhieuKhoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _sttNumber = 1;
        public int SttNumber
        {
            get => _sttNumber;
            set
            {
                if (_sttNumber != value)
                {
                    _sttNumber = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Stt));
                }
            }
        }

        public string Stt => SttNumber.ToString("D03");

        public string DmathangId { get; set; } = "";
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string DnhommathangId { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";
        public string TenDonViTinh { get; set; } = "";

        public Dictionary<string, decimal> KhoTonDict { get; set; } = new Dictionary<string, decimal>();

        public decimal TongTon => KhoTonDict.Values.Sum();
        public string TongTonFormatted => TongTon != 0 ? TongTon.ToString("N0") : "0";

        public decimal QuyDoi { get; set; } = 1;
        public string QuyDoiFormatted => QuyDoi > 0 ? QuyDoi.ToString("N0") : "1";

        public string Ton2Dvt { get; set; } = "";
        public string GhiChu { get; set; } = "";

        public bool IsAm => TongTon < 0;

        public decimal GetKhoTon(string khoId) => KhoTonDict.GetValueOrDefault(khoId, 0);
        public string GetKhoTonFormatted(string khoId)
        {
            decimal ton = GetKhoTon(khoId);
            return ton != 0 ? ton.ToString("N0") : "0";
        }
    }
}
