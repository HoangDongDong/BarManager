using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;

namespace QuanLyBar.Client.Services
{
    public class QuyFilterTreeItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded = true;

        public string Code { get; set; } = ""; // ALL, TIEN_MAT, QUET_THE, NGAN_HANG, TK_xxx
        public string Name { get; set; } = "";
        public string TaiKhoanId { get; set; } = "";
        public string Icon { get; set; } = "📁";
        public ObservableCollection<QuyFilterTreeItem> Children { get; set; } = new ObservableCollection<QuyFilterTreeItem>();

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class GiaoDichTonQuyItem
    {
        public string Stt { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public DateTime? Ngay { get; set; }
        public string NgayHienThi => Ngay?.ToString("dd/MM/yyyy") ?? "";
        public string DienGiai { get; set; } = "";
        public decimal Thu { get; set; }
        public decimal Chi { get; set; }
        public string ThuHienThi => Thu > 0 ? Thu.ToString("N0") : (Thu == 0 && Chi == 0 ? "0" : (Thu == 0 && (SoPhieu.StartsWith("PN") || SoPhieu.StartsWith("PC")) ? "0" : ""));
        public string ChiHienThi => Chi > 0 ? Chi.ToString("N0") : (Chi == 0 && Thu == 0 ? "0" : "");

        public string LoaiQuy { get; set; } = "TIEN_MAT"; // TIEN_MAT, QUET_THE, NGAN_HANG
        public string TaiKhoanId { get; set; } = "";
        public string TaiKhoanTen { get; set; } = "";
        public string CuaHangId { get; set; } = "";
        public string LoaiChungTu { get; set; } = ""; // PHIEU_THU, PHIEU_CHI, HOA_DON, NHAP_KHO, DAT_HANG
        public string ChungTuId { get; set; } = "";
    }

    public class TonQuyResult
    {
        public decimal TonDau { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal TonQuy => TonDau + TongThu - TongChi;
        public List<GiaoDichTonQuyItem> DanhSachGiaoDich { get; set; } = new List<GiaoDichTonQuyItem>();
    }

    public static class LocalTonQuyService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static object GetValue(IDictionary<string, object> d, string name)
        {
            if (d == null) return null;
            foreach (var kv in d)
            {
                if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
            return null;
        }

        public static async Task<List<QuyFilterTreeItem>> GetTreeQuyAsync()
        {
            var root = new QuyFilterTreeItem
            {
                Code = "ALL",
                Name = "Tất cả",
                Icon = "🌐",
                IsExpanded = true,
                IsSelected = true
            };

            var tienMat = new QuyFilterTreeItem
            {
                Code = "TIEN_MAT",
                Name = "Tiền mặt",
                Icon = "📁"
            };

            var quetThe = new QuyFilterTreeItem
            {
                Code = "QUET_THE",
                Name = "Quẹt thẻ",
                Icon = "📁"
            };

            var nganHang = new QuyFilterTreeItem
            {
                Code = "NGAN_HANG",
                Name = "Ngân hàng",
                Icon = "📁",
                IsExpanded = true
            };

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var listTk = (await conn.QueryAsync("SELECT ID, NAME, NOTE FROM DTAIKHOANNGANHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY COALESCE(SORTORDER, 0), ID")).ToList();

                    foreach (var tk in listTk)
                    {
                        var dict = tk as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string name = GetValue(dict, "NAME")?.ToString() ?? "";
                        string note = GetValue(dict, "NOTE")?.ToString() ?? "";

                        string displayName = name;
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            displayName = !string.IsNullOrWhiteSpace(note) ? note : "Tài khoản " + id;
                        }
                        else if (!string.IsNullOrWhiteSpace(note) && !displayName.Contains(note, StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = $"{displayName} ({note})";
                        }

                        nganHang.Children.Add(new QuyFilterTreeItem
                        {
                            Code = "TK_" + id,
                            Name = displayName,
                            TaiKhoanId = id,
                            Icon = "🏦"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetTreeQuyAsync: " + ex.Message);
            }

            root.Children.Add(tienMat);
            root.Children.Add(quetThe);
            root.Children.Add(nganHang);

            return new List<QuyFilterTreeItem> { root };
        }

        public static async Task<List<dynamic>> GetCuaHangListAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var list = (await conn.QueryAsync("SELECT ID, NAME FROM DCUAHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY ID")).ToList();
                    return list;
                }
            }
            catch
            {
                return new List<dynamic>();
            }
        }

        public static async Task<TonQuyResult> GetBaoCaoTonQuyAsync(DateTime? tuNgay, DateTime? denNgay, string cuaHangId = null, string quyCode = "ALL", string taiKhoanId = null)
        {
            var result = new TonQuyResult();
            var allTransactions = new List<GiaoDichTonQuyItem>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Tải TTHUCHI
                    string sqlThuChi = @"
                        SELECT 
                            ID,
                            NAME AS SOPHIEU,
                            NGAY,
                            DIENGIAI,
                            THU,
                            CHI,
                            CHUYENKHOAN,
                            DTAIKHOANNGANHANGID,
                            DTHETRATRUOCID,
                            DCUAHANGID,
                            LOAI,
                            STATUS
                        FROM TTHUCHI
                        WHERE (STATUS IS NULL OR STATUS <> 0)";

                    var thuChiRows = (await conn.QueryAsync(sqlThuChi)).ToList();
                    foreach (var row in thuChiRows)
                    {
                        var dict = row as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string soPhieu = GetValue(dict, "SOPHIEU")?.ToString() ?? "";
                        DateTime? ngay = null;
                        var rawNgay = GetValue(dict, "NGAY");
                        if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dt)) ngay = dt;

                        string dienGiai = GetValue(dict, "DIENGIAI")?.ToString() ?? "";
                        decimal thu = 0;
                        decimal chi = 0;

                        var rawThu = GetValue(dict, "THU");
                        if (rawThu != null && decimal.TryParse(rawThu.ToString(), out var dThu)) thu = dThu;

                        var rawChi = GetValue(dict, "CHI");
                        if (rawChi != null && decimal.TryParse(rawChi.ToString(), out var dChi)) chi = dChi;

                        if (thu <= 0 && chi <= 0) continue;

                        string ckRaw = GetValue(dict, "CHUYENKHOAN")?.ToString() ?? "";
                        string tkId = GetValue(dict, "DTAIKHOANNGANHANGID")?.ToString()?.Trim() ?? "";
                        string theId = GetValue(dict, "DTHETRATRUOCID")?.ToString()?.Trim() ?? "";
                        string chId = GetValue(dict, "DCUAHANGID")?.ToString()?.Trim() ?? "";

                        string loaiQuy = "TIEN_MAT";
                        if (ckRaw == "1" || ckRaw.Equals("True", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(tkId))
                        {
                            loaiQuy = "NGAN_HANG";
                        }
                        else if (!string.IsNullOrEmpty(theId))
                        {
                            loaiQuy = "QUET_THE";
                        }

                        allTransactions.Add(new GiaoDichTonQuyItem
                        {
                            SoPhieu = soPhieu,
                            Ngay = ngay,
                            DienGiai = dienGiai,
                            Thu = thu,
                            Chi = chi,
                            LoaiQuy = loaiQuy,
                            TaiKhoanId = tkId,
                            CuaHangId = chId,
                            LoaiChungTu = thu > 0 ? "PHIEU_THU" : "PHIEU_CHI",
                            ChungTuId = id
                        });
                    }

                    // 2. Tải TDONHANG (Bán hàng & Nhập kho)
                    string sqlDonHang = @"
                        SELECT 
                            ID,
                            NAME AS SOPHIEU,
                            SOHD,
                            NGAY,
                            DIENGIAI,
                            TONGCONG,
                            TIENTHANHTOAN,
                            THANHTOAN,
                            DATHANHTOAN,
                            TIENMAT,
                            CHUYENKHOAN,
                            THE,
                            DTAIKHOANNGANHANGID,
                            DCUAHANGID,
                            LOAI,
                            STATUS
                        FROM TDONHANG
                        WHERE (STATUS IS NULL OR STATUS <> 0)";

                    var donHangRows = (await conn.QueryAsync(sqlDonHang)).ToList();
                    foreach (var row in donHangRows)
                    {
                        var dict = row as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string soPhieu = GetValue(dict, "SOPHIEU")?.ToString() ?? "";
                        string soHd = GetValue(dict, "SOHD")?.ToString() ?? "";
                        if (string.IsNullOrEmpty(soPhieu)) soPhieu = soHd;

                        DateTime? ngay = null;
                        var rawNgay = GetValue(dict, "NGAY");
                        if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dt)) ngay = dt;

                        string dienGiai = GetValue(dict, "DIENGIAI")?.ToString() ?? "";
                        string loai = GetValue(dict, "LOAI")?.ToString() ?? "";
                        string chId = GetValue(dict, "DCUAHANGID")?.ToString()?.Trim() ?? "";
                        string tkId = GetValue(dict, "DTAIKHOANNGANHANGID")?.ToString()?.Trim() ?? "";

                        decimal tienMat = 0, chuyenKhoan = 0, the = 0;
                        var rawTm = GetValue(dict, "TIENMAT");
                        if (rawTm != null && decimal.TryParse(rawTm.ToString(), out var dTm)) tienMat = dTm;

                        var rawCk = GetValue(dict, "CHUYENKHOAN");
                        if (rawCk != null && decimal.TryParse(rawCk.ToString(), out var dCk)) chuyenKhoan = dCk;

                        var rawThe = GetValue(dict, "THE");
                        if (rawThe != null && decimal.TryParse(rawThe.ToString(), out var dThe)) the = dThe;

                        decimal daThanhToan = 0;
                        var rawTt = GetValue(dict, "THANHTOAN") ?? GetValue(dict, "DATHANHTOAN") ?? GetValue(dict, "TIENTHANHTOAN");
                        if (rawTt != null && decimal.TryParse(rawTt.ToString(), out var dTt)) daThanhToan = dTt;

                        // Bỏ qua phiếu chuyển kho (PCK, CK, LOAI=3) và phiếu xuất kho nội bộ (PX, LOAI=2)
                        if (loai == "3" || loai == "2" ||
                            soPhieu.StartsWith("PCK", StringComparison.OrdinalIgnoreCase) ||
                            soPhieu.StartsWith("CK", StringComparison.OrdinalIgnoreCase) ||
                            soPhieu.StartsWith("PX", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        bool isNhapKho = loai == "1" || loai.Equals("Nhập kho", StringComparison.OrdinalIgnoreCase) ||
                                         soPhieu.StartsWith("PN", StringComparison.OrdinalIgnoreCase) ||
                                         soPhieu.StartsWith("NK", StringComparison.OrdinalIgnoreCase);

                        if (isNhapKho)
                        {
                            decimal chiAmount = daThanhToan > 0 ? daThanhToan : (tienMat + chuyenKhoan + the);
                            if (string.IsNullOrWhiteSpace(dienGiai)) dienGiai = "Nhập mua hàng";

                            string loaiQuy = "TIEN_MAT";
                            if (chuyenKhoan > 0 || !string.IsNullOrEmpty(tkId)) loaiQuy = "NGAN_HANG";
                            else if (the > 0) loaiQuy = "QUET_THE";

                            allTransactions.Add(new GiaoDichTonQuyItem
                            {
                                SoPhieu = soPhieu,
                                Ngay = ngay,
                                DienGiai = dienGiai,
                                Thu = 0,
                                Chi = chiAmount,
                                LoaiQuy = loaiQuy,
                                TaiKhoanId = tkId,
                                CuaHangId = chId,
                                LoaiChungTu = "NHAP_KHO",
                                ChungTuId = id
                            });
                        }
                        else
                        {
                            // Hóa đơn bán hàng / Dịch vụ
                            decimal thuAmount = (tienMat + chuyenKhoan + the) > 0 ? (tienMat + chuyenKhoan + the) : daThanhToan;
                            if (thuAmount > 0)
                            {
                                string loaiQuy = "TIEN_MAT";
                                if (chuyenKhoan > 0) loaiQuy = "NGAN_HANG";
                                else if (the > 0) loaiQuy = "QUET_THE";

                                allTransactions.Add(new GiaoDichTonQuyItem
                                {
                                    SoPhieu = soPhieu,
                                    Ngay = ngay,
                                    DienGiai = dienGiai,
                                    Thu = thuAmount,
                                    Chi = 0,
                                    LoaiQuy = loaiQuy,
                                    TaiKhoanId = tkId,
                                    CuaHangId = chId,
                                    LoaiChungTu = "HOA_DON",
                                    ChungTuId = id
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBaoCaoTonQuyAsync: " + ex.Message);
            }

            // Sắp xếp các giao dịch theo Ngày tăng dần, sau đó theo Số phiếu
            allTransactions = allTransactions
                .OrderBy(x => x.Ngay ?? DateTime.MinValue)
                .ThenBy(x => x.SoPhieu)
                .ToList();

            // Hàm kiểm tra xem 1 transaction có thỏa mãn filter Quỹ & Cửa hàng không
            bool MatchFilter(GiaoDichTonQuyItem item)
            {
                if (!string.IsNullOrEmpty(cuaHangId) && cuaHangId != "0" && item.CuaHangId != cuaHangId)
                {
                    return false;
                }

                if (quyCode == "ALL") return true;
                if (quyCode == "TIEN_MAT") return item.LoaiQuy == "TIEN_MAT";
                if (quyCode == "QUET_THE") return item.LoaiQuy == "QUET_THE";
                if (quyCode == "NGAN_HANG")
                {
                    if (!string.IsNullOrEmpty(taiKhoanId))
                    {
                        return item.LoaiQuy == "NGAN_HANG" && item.TaiKhoanId == taiKhoanId;
                    }
                    return item.LoaiQuy == "NGAN_HANG";
                }
                if (quyCode.StartsWith("TK_"))
                {
                    string targetTk = quyCode.Substring(3);
                    return item.LoaiQuy == "NGAN_HANG" && (item.TaiKhoanId == targetTk || string.IsNullOrEmpty(item.TaiKhoanId));
                }

                return true;
            }

            // Tính Tồn đầu (trước tuNgay)
            decimal tonDau = 0;
            if (tuNgay.HasValue)
            {
                DateTime tuDate = tuNgay.Value.Date;
                var truocKy = allTransactions.Where(x => MatchFilter(x) && (x.Ngay.HasValue && x.Ngay.Value.Date < tuDate));
                tonDau = truocKy.Sum(x => x.Thu - x.Chi);
            }

            // Lọc giao dịch trong kỳ
            var trongKy = allTransactions.Where(x => MatchFilter(x)).Where(x =>
            {
                if (!x.Ngay.HasValue) return true;
                DateTime d = x.Ngay.Value.Date;
                if (tuNgay.HasValue && d < tuNgay.Value.Date) return false;
                if (denNgay.HasValue && d > denNgay.Value.Date) return false;
                return true;
            }).ToList();

            int stt = 1;
            foreach (var item in trongKy)
            {
                item.Stt = (stt++).ToString("D2");
            }

            result.TonDau = tonDau;
            result.TongThu = trongKy.Sum(x => x.Thu);
            result.TongChi = trongKy.Sum(x => x.Chi);
            result.DanhSachGiaoDich = trongKy;

            return result;
        }
    }
}
