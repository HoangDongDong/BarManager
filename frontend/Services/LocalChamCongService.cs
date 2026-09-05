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
    public class BangLuongItemViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Thang { get; set; } = "";
        public string Nam { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; } = 30;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class ChamCongCellItem : INotifyPropertyChanged
    {
        private string _status = "0"; // 0: Không có lịch, 1: Đi làm, P: Nghỉ có phép, KP: Nghỉ không phép

        public int Day { get; set; }
        public DateTime Date { get; set; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        public string DisplayText
        {
            get
            {
                return _status switch
                {
                    "1" => "",
                    "P" => "",
                    "KP" => "",
                    _ => ""
                };
            }
        }

        public string BackgroundColor
        {
            get
            {
                return _status switch
                {
                    "1" => "#e60000",             // Đi làm (Đỏ)
                    "2" or "P" => "#00cc00",     // Nghỉ có phép (Xanh lá)
                    "3" or "KP" => "#00e6d2",    // Nghỉ không phép (Xanh ngọc / Cyan)
                    _ => "#ffffff"                // Không có lịch (Trắng)
                };
            }
        }

        public string ForegroundColor
        {
            get
            {
                return _status switch
                {
                    "1" => "#ffffff",
                    "2" or "P" => "#ffffff",
                    "3" or "KP" => "#000000",
                    _ => "#94a3b8"
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class ChamCongNhanVienCaRow : INotifyPropertyChanged
    {
        public string NhanVienId { get; set; } = "";
        public string MaNhanVien { get; set; } = "";
        public string TenNhanVien { get; set; } = "";
        public string TenNhanVienDisplay => IsFirstShiftOfEmployee ? TenNhanVien : "";
        public bool IsFirstShiftOfEmployee { get; set; } = false;
        public string EmployeeBackground => "#d4e8fc"; // Light blue for employee column
        
        public string CaLamViecId { get; set; } = "";
        public string TenCaLamViec { get; set; } = "";

        // Map day 1..31 -> ChamCongCellItem
        public Dictionary<int, ChamCongCellItem> DaysMap { get; set; } = new Dictionary<int, ChamCongCellItem>();

        // Salary Fields
        private decimal _luongCoBan = 0;
        private decimal _tongLuong = 0;
        private decimal _thuong = 0;
        private decimal _phat = 0;
        private decimal _tamUng = 0;
        private decimal _thucNhan = 0;
        private int _cachTinhLuong = 0;
        private decimal _luongCa = 0;

        public decimal LuongCoBan
        {
            get => _luongCoBan;
            set
            {
                _luongCoBan = value;
                OnPropertyChanged(nameof(LuongCoBan));
                OnPropertyChanged(nameof(LuongCoBanDisplay));
            }
        }

        public decimal TongLuong
        {
            get => _tongLuong;
            set
            {
                _tongLuong = value;
                OnPropertyChanged(nameof(TongLuong));
                OnPropertyChanged(nameof(TongLuongDisplay));
            }
        }

        public decimal Thuong
        {
            get => _thuong;
            set
            {
                _thuong = value;
                OnPropertyChanged(nameof(Thuong));
                OnPropertyChanged(nameof(ThuongDisplay));
            }
        }

        public decimal Phat
        {
            get => _phat;
            set
            {
                _phat = value;
                OnPropertyChanged(nameof(Phat));
                OnPropertyChanged(nameof(PhatDisplay));
            }
        }

        public decimal TamUng
        {
            get => _tamUng;
            set
            {
                _tamUng = value;
                OnPropertyChanged(nameof(TamUng));
                OnPropertyChanged(nameof(TamUngDisplay));
            }
        }

        public decimal ThucNhan
        {
            get => _thucNhan;
            set
            {
                _thucNhan = value;
                OnPropertyChanged(nameof(ThucNhan));
                OnPropertyChanged(nameof(ThucNhanDisplay));
            }
        }

        public int CachTinhLuong
        {
            get => _cachTinhLuong;
            set => _cachTinhLuong = value;
        }

        public decimal LuongCa
        {
            get => _luongCa;
            set => _luongCa = value;
        }

        public string LuongCoBanDisplay => IsFirstShiftOfEmployee ? (LuongCoBan > 0 ? LuongCoBan.ToString("N0") : "0") : "";
        public string TongLuongDisplay => IsFirstShiftOfEmployee ? (TongLuong > 0 ? TongLuong.ToString("N0") : "0") : "";
        public string ThuongDisplay => IsFirstShiftOfEmployee ? (Thuong > 0 ? Thuong.ToString("N0") : "0") : "";
        public string PhatDisplay => IsFirstShiftOfEmployee ? (Phat > 0 ? Phat.ToString("N0") : "0") : "";
        public string TamUngDisplay => IsFirstShiftOfEmployee ? (TamUng > 0 ? TamUng.ToString("N0") : "0") : "";
        public string ThucNhanDisplay => IsFirstShiftOfEmployee ? (ThucNhan != 0 ? ThucNhan.ToString("N0") : "0") : "";

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class TinhLuongItemViewModel : INotifyPropertyChanged
    {
        private int _stt;
        private string _nhanVienId = "";
        private string _maNhanVien = "";
        private string _tenNhanVien = "";
        private string _chucVu = "";
        private int _soNgayLam = 0;
        private int _soNgayNghiCoPhep = 0;
        private int _soNgayNghiKhongPhep = 0;
        private decimal _luongCoBan = 0;
        private decimal _luongCa = 0;
        private int _cachTinhLuong = 0; // 0: tháng, 1: ca
        private decimal _tongLuong = 0;
        private decimal _thuong = 0;
        private decimal _phat = 0;
        private decimal _tamUng = 0;
        private decimal _thucNhan = 0;
        private string _ghiChu = "";

        public int STT { get => _stt; set { _stt = value; OnPropertyChanged(nameof(STT)); } }
        public string NhanVienId { get => _nhanVienId; set { _nhanVienId = value; OnPropertyChanged(nameof(NhanVienId)); } }
        public string MaNhanVien { get => _maNhanVien; set { _maNhanVien = value; OnPropertyChanged(nameof(MaNhanVien)); } }
        public string TenNhanVien { get => _tenNhanVien; set { _tenNhanVien = value; OnPropertyChanged(nameof(TenNhanVien)); } }
        public string ChucVu { get => _chucVu; set { _chucVu = value; OnPropertyChanged(nameof(ChucVu)); } }
        public int SoNgayLam { get => _soNgayLam; set { _soNgayLam = value; OnPropertyChanged(nameof(SoNgayLam)); } }
        public int SoNgayNghiCoPhep { get => _soNgayNghiCoPhep; set { _soNgayNghiCoPhep = value; OnPropertyChanged(nameof(SoNgayNghiCoPhep)); } }
        public int SoNgayNghiKhongPhep { get => _soNgayNghiKhongPhep; set { _soNgayNghiKhongPhep = value; OnPropertyChanged(nameof(SoNgayNghiKhongPhep)); } }
        public decimal LuongCoBan { get => _luongCoBan; set { _luongCoBan = value; OnPropertyChanged(nameof(LuongCoBan)); OnPropertyChanged(nameof(LuongCoBanDisplay)); } }
        public decimal LuongCa { get => _luongCa; set { _luongCa = value; OnPropertyChanged(nameof(LuongCa)); OnPropertyChanged(nameof(LuongCaDisplay)); OnPropertyChanged(nameof(LuongCoBanDisplay)); } }
        public int CachTinhLuong { get => _cachTinhLuong; set { _cachTinhLuong = value; OnPropertyChanged(nameof(CachTinhLuong)); OnPropertyChanged(nameof(CachTinhLuongDisplay)); OnPropertyChanged(nameof(LuongCoBanDisplay)); } }
        public decimal TongLuong { get => _tongLuong; set { _tongLuong = value; OnPropertyChanged(nameof(TongLuong)); OnPropertyChanged(nameof(TongLuongDisplay)); } }
        public decimal Thuong { get => _thuong; set { _thuong = value; OnPropertyChanged(nameof(Thuong)); OnPropertyChanged(nameof(ThuongDisplay)); } }
        public decimal Phat { get => _phat; set { _phat = value; OnPropertyChanged(nameof(Phat)); OnPropertyChanged(nameof(PhatDisplay)); } }
        public decimal TamUng { get => _tamUng; set { _tamUng = value; OnPropertyChanged(nameof(TamUng)); OnPropertyChanged(nameof(TamUngDisplay)); } }
        public decimal ThucNhan { get => _thucNhan; set { _thucNhan = value; OnPropertyChanged(nameof(ThucNhan)); OnPropertyChanged(nameof(ThucNhanDisplay)); } }
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(nameof(GhiChu)); } }

        public string CachTinhLuongDisplay => CachTinhLuong == 1 ? "Theo ca" : "Theo tháng";
        public string LuongCoBanDisplay => LuongCoBan > 0 ? LuongCoBan.ToString("N0") : (LuongCa > 0 ? $"{LuongCa:N0}/ca" : "0");
        public string LuongCaDisplay => LuongCa > 0 ? LuongCa.ToString("N0") : "0";
        public string TongLuongDisplay => TongLuong > 0 ? TongLuong.ToString("N0") : "0";
        public string ThuongDisplay => Thuong > 0 ? Thuong.ToString("N0") : "0";
        public string PhatDisplay => Phat > 0 ? Phat.ToString("N0") : "0";
        public string TamUngDisplay => TamUng > 0 ? TamUng.ToString("N0") : "0";
        public string ThucNhanDisplay => ThucNhan != 0 ? ThucNhan.ToString("N0") : "0";

        public void Recalculate()
        {
            ThucNhan = TongLuong + Thuong - Phat - TamUng;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalChamCongService
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

        private static async Task<int> GetNextTBangLuongIdAsync(IDbConnection conn)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT ID FROM TBANGLUONG")).Cast<IDictionary<string, object>>().ToList();
                int maxId = 0;
                foreach (var dict in rows)
                {
                    object valObj = GetValue(dict, "ID");
                    if (valObj != null && !Convert.IsDBNull(valObj))
                    {
                        if (int.TryParse(valObj.ToString(), out int val) && val > maxId)
                            maxId = val;
                    }
                }
                return maxId + 1;
            }
            catch
            {
                return 1;
            }
        }

        private static async Task<int> GetNextTBangLuongChiTietIdAsync(IDbConnection conn)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT ID FROM TBANGLUONGCHITIET")).Cast<IDictionary<string, object>>().ToList();
                int maxId = 0;
                foreach (var dict in rows)
                {
                    object valObj = GetValue(dict, "ID");
                    if (valObj != null && !Convert.IsDBNull(valObj))
                    {
                        if (int.TryParse(valObj.ToString(), out int val) && val > maxId)
                            maxId = val;
                    }
                }
                return maxId + 1;
            }
            catch
            {
                return 1;
            }
        }

        public static async Task<List<BangLuongItemViewModel>> GetBangLuongListAsync()
        {
            var list = new List<BangLuongItemViewModel>();
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                string query = "SELECT * FROM TBANGLUONG WHERE STATUS IS NULL OR STATUS >= 0 ORDER BY NAM DESC, THANG DESC, ID DESC";
                var rows = (await conn.QueryAsync(query)).Cast<IDictionary<string, object>>().ToList();

                foreach (var r in rows)
                {
                    string id = GetValue(r, "ID")?.ToString() ?? "";
                    string name = GetValue(r, "NAME")?.ToString() ?? "";
                    string thang = GetValue(r, "THANG")?.ToString() ?? "";
                    string nam = GetValue(r, "NAM")?.ToString() ?? "";
                    string note = GetValue(r, "NOTE")?.ToString() ?? "";
                    int? status = Convert.IsDBNull(GetValue(r, "STATUS")) ? (int?)null : Convert.ToInt32(GetValue(r, "STATUS"));

                    list.Add(new BangLuongItemViewModel
                    {
                        Id = id,
                        Name = name,
                        Thang = thang,
                        Nam = nam,
                        Note = note,
                        Status = status
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBangLuongList: " + ex.Message);
            }
            return list;
        }

        public static async Task<BangLuongItemViewModel> GetBangLuongByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                string query = "SELECT * FROM TBANGLUONG WHERE CAST(ID AS VARCHAR(50)) = @Id";
                var r = (await conn.QueryFirstOrDefaultAsync(query, new { Id = id })) as IDictionary<string, object>;
                if (r != null)
                {
                    return new BangLuongItemViewModel
                    {
                        Id = GetValue(r, "ID")?.ToString() ?? "",
                        Name = GetValue(r, "NAME")?.ToString() ?? "",
                        Thang = GetValue(r, "THANG")?.ToString() ?? "",
                        Nam = GetValue(r, "NAM")?.ToString() ?? "",
                        Note = GetValue(r, "NOTE")?.ToString() ?? "",
                        Status = Convert.IsDBNull(GetValue(r, "STATUS")) ? (int?)null : Convert.ToInt32(GetValue(r, "STATUS"))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBangLuongById: " + ex.Message);
            }
            return null;
        }

        public static async Task<(bool success, string error, string savedId)> SaveBangLuongAsync(
            string id,
            string soPhieu,
            int thang,
            int nam,
            string ghiChu)
        {
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                string thangStr = thang.ToString();
                string namStr = nam.ToString();

                if (string.IsNullOrEmpty(id))
                {
                    // Check duplicate month/year
                    var existing = await conn.ExecuteScalarAsync<object>(
                        "SELECT FIRST 1 ID FROM TBANGLUONG WHERE THANG = @Thang AND NAM = @Nam AND (STATUS IS NULL OR STATUS >= 0)",
                        new { Thang = thangStr, Nam = namStr });
                    if (existing != null && existing != DBNull.Value)
                    {
                        return (false, $"Bảng lương cho Tháng {thang}/{nam} đã tồn tại!", null);
                    }

                    int nextId = await GetNextTBangLuongIdAsync(conn);
                    string newIdStr = nextId.ToString();

                    try
                    {
                        string insertSql = @"
                            INSERT INTO TBANGLUONG (ID, NAME, THANG, NAM, NOTE, STATUS, USERCREATEDID, TIMECREATED)
                            VALUES (@Id, @Name, @Thang, @Nam, @Note, 30, 1, CURRENT_TIMESTAMP)";
                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = nextId,
                            Name = soPhieu,
                            Thang = thangStr,
                            Nam = namStr,
                            Note = ghiChu ?? ""
                        });
                    }
                    catch
                    {
                        string insertSql = @"
                            INSERT INTO TBANGLUONG (ID, NAME, THANG, NAM, NOTE, STATUS)
                            VALUES (@Id, @Name, @Thang, @Nam, @Note, 30)";
                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = nextId,
                            Name = soPhieu,
                            Thang = thangStr,
                            Nam = namStr,
                            Note = ghiChu ?? ""
                        });
                    }

                    return (true, null, newIdStr);
                }
                else
                {
                    try
                    {
                        string updateSql = @"
                            UPDATE TBANGLUONG SET 
                                NAME = @Name,
                                THANG = @Thang,
                                NAM = @Nam,
                                NOTE = @Note,
                                USERMODIFIEDID = 1,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                        await conn.ExecuteAsync(updateSql, new
                        {
                            IdStr = id,
                            Name = soPhieu,
                            Thang = thangStr,
                            Nam = namStr,
                            Note = ghiChu ?? ""
                        });
                    }
                    catch
                    {
                        string updateSql = @"
                            UPDATE TBANGLUONG SET 
                                NAME = @Name,
                                THANG = @Thang,
                                NAM = @Nam,
                                NOTE = @Note
                            WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                        await conn.ExecuteAsync(updateSql, new
                        {
                            IdStr = id,
                            Name = soPhieu,
                            Thang = thangStr,
                            Nam = namStr,
                            Note = ghiChu ?? ""
                        });
                    }

                    return (true, null, id);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<bool> DeleteBangLuongAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                await conn.ExecuteAsync("DELETE FROM TBANGLUONGCHITIET WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id", new { Id = id });
                await conn.ExecuteAsync("DELETE FROM TBANGLUONGTONGHOP WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id", new { Id = id });
                await conn.ExecuteAsync("DELETE FROM TBANGLUONG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = id });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteBangLuong: " + ex.Message);
                return false;
            }
        }

        private static async Task<int> GetNextTBangLuongTongHopIdAsync(IDbConnection conn)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT ID FROM TBANGLUONGTONGHOP")).Cast<IDictionary<string, object>>().ToList();
                int maxId = 0;
                foreach (var dict in rows)
                {
                    object valObj = GetValue(dict, "ID");
                    if (valObj != null && !Convert.IsDBNull(valObj))
                    {
                        if (int.TryParse(valObj.ToString(), out int val) && val > maxId)
                            maxId = val;
                    }
                }
                return maxId + 1;
            }
            catch
            {
                return 1;
            }
        }

        public static void RecalculateAllSalaries(List<ChamCongNhanVienCaRow> rows, int month, int year)
        {
            if (rows == null || rows.Count == 0) return;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var groups = rows.GroupBy(x => x.NhanVienId);
            foreach (var g in groups)
            {
                var firstRow = g.FirstOrDefault(x => x.IsFirstShiftOfEmployee) ?? g.First();
                
                // Count worked shifts (Status == 1) and paid leave (Status == 2 or P)
                int workedShifts = 0;
                int paidLeave = 0;
                foreach (var r in g)
                {
                    foreach (var kv in r.DaysMap)
                    {
                        if (kv.Value.Status == "1") workedShifts++;
                        else if (kv.Value.Status == "2" || kv.Value.Status == "P") paidLeave++;
                    }
                }

                decimal calculatedTongLuong = 0;
                if (firstRow.CachTinhLuong == 1)
                {
                    // Lương theo ca
                    decimal rate = firstRow.LuongCa > 0 ? firstRow.LuongCa : (firstRow.LuongCoBan > 0 ? firstRow.LuongCoBan / daysInMonth : 0);
                    calculatedTongLuong = Math.Round(workedShifts * rate, 0);
                }
                else
                {
                    // Lương theo tháng
                    if (firstRow.LuongCoBan > 0)
                    {
                        int effectiveDays = workedShifts + paidLeave;
                        if (effectiveDays >= daysInMonth)
                        {
                            calculatedTongLuong = firstRow.LuongCoBan;
                        }
                        else
                        {
                            calculatedTongLuong = Math.Round((firstRow.LuongCoBan / (decimal)daysInMonth) * effectiveDays, 0);
                        }
                    }
                    else if (firstRow.LuongCa > 0)
                    {
                        calculatedTongLuong = Math.Round(workedShifts * firstRow.LuongCa, 0);
                    }
                }

                firstRow.TongLuong = calculatedTongLuong;
                firstRow.ThucNhan = firstRow.TongLuong + firstRow.Thuong - firstRow.Phat - firstRow.TamUng;

                // Sync to all rows of employee so any row binding works
                foreach (var r in g)
                {
                    if (r != firstRow)
                    {
                        r.LuongCoBan = firstRow.LuongCoBan;
                        r.TongLuong = firstRow.TongLuong;
                        r.Thuong = firstRow.Thuong;
                        r.Phat = firstRow.Phat;
                        r.TamUng = firstRow.TamUng;
                        r.ThucNhan = firstRow.ThucNhan;
                        r.CachTinhLuong = firstRow.CachTinhLuong;
                        r.LuongCa = firstRow.LuongCa;
                    }
                }
            }
        }

        public static async Task<List<ChamCongNhanVienCaRow>> GetChamCongMatrixAsync(string bangLuongId, int month, int year)
        {
            var result = new List<ChamCongNhanVienCaRow>();
            if (string.IsNullOrEmpty(bangLuongId)) return result;

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                var nvList = await LocalNhanVienService.GetNhanVienFlatListAsync(false);
                var caList = await LocalCaLamViecService.GetCaLamViecFlatListAsync(false);
                
                // Fallback default shifts if none in database
                if (caList.Count == 0)
                {
                    caList.Add(new CaLamViecTreeItem { Id = "1", Name = "Ca sáng" });
                    caList.Add(new CaLamViecTreeItem { Id = "2", Name = "Ca chiều" });
                    caList.Add(new CaLamViecTreeItem { Id = "3", Name = "Ca tối" });
                    caList.Add(new CaLamViecTreeItem { Id = "4", Name = "Ca Đêm" });
                }

                int daysInMonth = DateTime.DaysInMonth(year, month);

                // Fetch all details for this bang luong
                string query = "SELECT * FROM TBANGLUONGCHITIET WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id";
                var details = (await conn.QueryAsync(query, new { Id = bangLuongId })).Cast<IDictionary<string, object>>().ToList();

                // Fetch existing TBANGLUONGTONGHOP if available
                var tongHopRows = (await conn.QueryAsync("SELECT * FROM TBANGLUONGTONGHOP WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id", new { Id = bangLuongId })).Cast<IDictionary<string, object>>().ToList();
                var tongHopMap = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                foreach (var th in tongHopRows)
                {
                    string nvId = GetValue(th, "DNHANVIENID")?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(nvId))
                        tongHopMap[nvId] = th;
                }

                // Fetch Thuong / Phat for this month/year
                var thuongPhatMap = new Dictionary<string, (decimal thuong, decimal phat)>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var tpRows = (await conn.QueryAsync("SELECT * FROM TTHUONGPHAT WHERE (STATUS IS NULL OR STATUS >= 0)")).Cast<IDictionary<string, object>>().ToList();
                    foreach (var tp in tpRows)
                    {
                        object rawNgay = GetValue(tp, "NGAY");
                        DateTime? dt = null;
                        if (rawNgay is DateTime dVal) dt = dVal;
                        else if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dVal2)) dt = dVal2;

                        if (dt.HasValue && dt.Value.Month == month && dt.Value.Year == year)
                        {
                            string nvId = GetValue(tp, "DNHANVIENID")?.ToString()?.Trim() ?? "";
                            decimal thVal = 0;
                            decimal phVal = 0;
                            object thObj = GetValue(tp, "THUONG");
                            object phObj = GetValue(tp, "PHAT");
                            if (thObj != null && decimal.TryParse(thObj.ToString(), out decimal parsedTh)) thVal = parsedTh;
                            if (phObj != null && decimal.TryParse(phObj.ToString(), out decimal parsedPh)) phVal = parsedPh;

                            if (!string.IsNullOrEmpty(nvId))
                            {
                                if (!thuongPhatMap.ContainsKey(nvId)) thuongPhatMap[nvId] = (0, 0);
                                var current = thuongPhatMap[nvId];
                                thuongPhatMap[nvId] = (current.thuong + thVal, current.phat + phVal);
                            }
                        }
                    }
                }
                catch (Exception exTp)
                {
                    Console.WriteLine("Error fetch TTHUONGPHAT: " + exTp.Message);
                }

                // Fetch Tam Ung for this month/year
                var tamUngMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var tuRows = (await conn.QueryAsync("SELECT * FROM TTHUCHI WHERE (LATAMUNG = '1' OR LATAMUNG = 1) AND (STATUS IS NULL OR STATUS <> 0)")).Cast<IDictionary<string, object>>().ToList();
                    foreach (var tu in tuRows)
                    {
                        object rawNgay = GetValue(tu, "NGAY");
                        DateTime? dt = null;
                        if (rawNgay is DateTime dVal) dt = dVal;
                        else if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dVal2)) dt = dVal2;

                        if (dt.HasValue && dt.Value.Month == month && dt.Value.Year == year)
                        {
                            string nvId = GetValue(tu, "DNHANVIENID")?.ToString()?.Trim() ?? "";
                            decimal tuVal = 0;
                            object chiObj = GetValue(tu, "CHI");
                            if (chiObj != null && decimal.TryParse(chiObj.ToString(), out decimal parsedChi)) tuVal = parsedChi;

                            if (!string.IsNullOrEmpty(nvId))
                            {
                                if (!tamUngMap.ContainsKey(nvId)) tamUngMap[nvId] = 0;
                                tamUngMap[nvId] += tuVal;
                            }
                        }
                    }
                }
                catch (Exception exTu)
                {
                    Console.WriteLine("Error fetch TTHUCHI tam ung: " + exTu.Message);
                }

                // Lookup maps to handle both ID matching and Name matching
                var nvLookup = nvList.ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);
                var caLookup = caList.ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);

                // Map (nvKey, caKey, day) -> Trangthai
                var statusMap = new Dictionary<string, string>();
                foreach (var d in details)
                {
                    string nvId = GetValue(d, "DNHANVIENID")?.ToString()?.Trim() ?? "";
                    string caId = GetValue(d, "DCALAMVIECID")?.ToString()?.Trim() ?? "";
                    object rawNgay = GetValue(d, "NGAY");
                    DateTime? dt = null;
                    if (rawNgay is DateTime dateVal) dt = dateVal;
                    else if (rawNgay != null && DateTime.TryParse(rawNgay.ToString(), out var dt2)) dt = dt2;

                    if (dt.HasValue)
                    {
                        string rawTt = GetValue(d, "TRANGTHAI")?.ToString()?.Trim() ?? "0";
                        string tt = rawTt switch
                        {
                            "1" => "1",
                            "2" or "P" => "2",
                            "3" or "KP" => "3",
                            _ => rawTt
                        };
                        int day = dt.Value.Day;
                        
                        // Register by nvId + caId
                        statusMap[$"{nvId}_{caId}_{day}"] = tt;
                        
                        // Register by employee name / code if needed
                        if (nvLookup.TryGetValue(nvId, out var foundNv))
                        {
                            statusMap[$"{foundNv.Name}_{caId}_{day}"] = tt;
                            if (caLookup.TryGetValue(caId, out var foundCa))
                            {
                                statusMap[$"{foundNv.Name}_{foundCa.Name}_{day}"] = tt;
                                statusMap[$"{nvId}_{foundCa.Name}_{day}"] = tt;
                            }
                        }
                    }
                }

                foreach (var nv in nvList)
                {
                    if (string.IsNullOrEmpty(nv.Id)) continue;

                    // Determine base salary and bonuses
                    decimal luongCoBan = nv.LuongThang > 0 ? nv.LuongThang : 0;
                    decimal luongCa = nv.LuongCa;
                    int cachTinh = nv.CachTinhLuong;

                    decimal thuongVal = 0;
                    decimal phatVal = 0;
                    decimal tamUngVal = 0;

                    if (thuongPhatMap.TryGetValue(nv.Id, out var tpInfo))
                    {
                        thuongVal = tpInfo.thuong;
                        phatVal = tpInfo.phat;
                    }
                    if (tamUngMap.TryGetValue(nv.Id, out var tuInfo))
                    {
                        tamUngVal = tuInfo;
                    }

                    // If existing TBANGLUONGTONGHOP exists, read saved values
                    if (tongHopMap.TryGetValue(nv.Id, out var thObj))
                    {
                        object ltObj = GetValue(thObj, "LUONGTHANG");
                        if (ltObj != null && decimal.TryParse(ltObj.ToString(), out decimal ltSaved) && ltSaved > 0)
                            luongCoBan = ltSaved;

                        object lcObj = GetValue(thObj, "LUONGCA");
                        if (lcObj != null && decimal.TryParse(lcObj.ToString(), out decimal lcSaved) && lcSaved > 0)
                            luongCa = lcSaved;

                        object thSavedObj = GetValue(thObj, "THUONG");
                        if (thSavedObj != null && decimal.TryParse(thSavedObj.ToString(), out decimal thSaved) && thSaved > 0)
                            thuongVal = thSaved;

                        object phSavedObj = GetValue(thObj, "PHAT");
                        if (phSavedObj != null && decimal.TryParse(phSavedObj.ToString(), out decimal phSaved) && phSaved > 0)
                            phatVal = phSaved;

                        object tuSavedObj = GetValue(thObj, "TAMUNG");
                        if (tuSavedObj != null && decimal.TryParse(tuSavedObj.ToString(), out decimal tuSaved) && tuSaved > 0)
                            tamUngVal = tuSaved;
                    }

                    bool isFirst = true;
                    foreach (var ca in caList)
                    {
                        var row = new ChamCongNhanVienCaRow
                        {
                            NhanVienId = nv.Id,
                            MaNhanVien = nv.Code,
                            TenNhanVien = nv.Name,
                            IsFirstShiftOfEmployee = isFirst,
                            CaLamViecId = ca.Id,
                            TenCaLamViec = ca.Name,
                            LuongCoBan = luongCoBan,
                            LuongCa = luongCa,
                            CachTinhLuong = cachTinh,
                            Thuong = thuongVal,
                            Phat = phatVal,
                            TamUng = tamUngVal
                        };
                        isFirst = false;

                        for (int d = 1; d <= daysInMonth; d++)
                        {
                            DateTime date = new DateTime(year, month, d);
                            string st = "0";

                            if (statusMap.TryGetValue($"{nv.Id}_{ca.Id}_{d}", out var s1))
                                st = s1;
                            else if (statusMap.TryGetValue($"{nv.Name}_{ca.Name}_{d}", out var s2))
                                st = s2;
                            else if (statusMap.TryGetValue($"{nv.Id}_{ca.Name}_{d}", out var s3))
                                st = s3;
                            else if (statusMap.TryGetValue($"{nv.Name}_{ca.Id}_{d}", out var s4))
                                st = s4;

                            row.DaysMap[d] = new ChamCongCellItem
                            {
                                Day = d,
                                Date = date,
                                Status = st
                            };
                        }

                        result.Add(row);
                    }
                }

                // Recalculate salaries for all rows
                RecalculateAllSalaries(result, month, year);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetChamCongMatrix: " + ex.Message);
            }

            return result;
        }

        public static async Task<(bool ok, string error)> SaveChamCongMatrixAsync(string bangLuongId, int month, int year, List<ChamCongNhanVienCaRow> rows)
        {
            if (string.IsNullOrEmpty(bangLuongId) || rows == null || rows.Count == 0) return (false, "Dữ liệu trống");

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                // Recalculate first to ensure accuracy
                RecalculateAllSalaries(rows, month, year);

                // 1. Delete old detail records
                await conn.ExecuteAsync("DELETE FROM TBANGLUONGCHITIET WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id", new { Id = bangLuongId });

                int nextCtId = await GetNextTBangLuongChiTietIdAsync(conn);
                object blIdParam = bangLuongId;
                if (int.TryParse(bangLuongId, out int blInt)) blIdParam = blInt;

                // Pre-fetch DB IDs for employees and shifts
                var dbNvRows = (await conn.QueryAsync("SELECT ID, NAME, CODE FROM DNHANVIEN")).Cast<IDictionary<string, object>>().ToList();
                var nvNameToId = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in dbNvRows)
                {
                    object idObj = GetValue(r, "ID");
                    string name = GetValue(r, "NAME")?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && idObj != null)
                        nvNameToId[name.Trim()] = idObj;
                }

                var dbCaRows = (await conn.QueryAsync("SELECT ID, NAME FROM DCALAMVIEC")).Cast<IDictionary<string, object>>().ToList();
                var caNameToId = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in dbCaRows)
                {
                    object idObj = GetValue(r, "ID");
                    string name = GetValue(r, "NAME")?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && idObj != null)
                        caNameToId[name.Trim()] = idObj;
                }

                int savedCount = 0;
                Exception lastEx = null;

                foreach (var row in rows)
                {
                    object nvIdParam = row.NhanVienId;
                    if (int.TryParse(row.NhanVienId, out int nvInt))
                    {
                        nvIdParam = nvInt;
                    }
                    else if (!string.IsNullOrEmpty(row.TenNhanVien) && nvNameToId.TryGetValue(row.TenNhanVien.Trim(), out var foundNvId))
                    {
                        if (int.TryParse(foundNvId?.ToString(), out int fInt))
                            nvIdParam = fInt;
                        else
                            nvIdParam = foundNvId;
                    }

                    object caIdParam = DBNull.Value;
                    if (!string.IsNullOrEmpty(row.CaLamViecId) && int.TryParse(row.CaLamViecId, out int caInt))
                    {
                        caIdParam = caInt;
                    }
                    else if (!string.IsNullOrEmpty(row.TenCaLamViec) && caNameToId.TryGetValue(row.TenCaLamViec.Trim(), out var foundCaId))
                    {
                        if (int.TryParse(foundCaId?.ToString(), out int fCaInt))
                            caIdParam = fCaInt;
                        else
                            caIdParam = foundCaId;
                    }
                    else if (!string.IsNullOrEmpty(row.CaLamViecId))
                    {
                        caIdParam = row.CaLamViecId;
                    }

                    foreach (var kv in row.DaysMap)
                    {
                        int day = kv.Key;
                        var cell = kv.Value;
                        if (cell.Status == "0" || string.IsNullOrEmpty(cell.Status)) continue; // only save marked cells

                        int trangThaiInt = 0;
                        if (cell.Status == "1") trangThaiInt = 1;
                        else if (cell.Status == "2" || cell.Status == "P") trangThaiInt = 2;
                        else if (cell.Status == "3" || cell.Status == "KP") trangThaiInt = 3;

                        if (trangThaiInt == 0) continue;

                        DateTime date = cell.Date != default ? cell.Date : new DateTime(year, month, day);
                        int currentId = nextCtId++;
                        bool savedThisCell = false;

                        // Attempt 1: Full insert with STATUS=1, USERCREATEDID=1, TIMECREATED
                        try
                        {
                            string sql = @"
                                INSERT INTO TBANGLUONGCHITIET 
                                (ID, TBANGLUONGID, DNHANVIENID, DCALAMVIECID, NGAY, TRANGTHAI, STATUS, USERCREATEDID, TIMECREATED)
                                VALUES 
                                (@Id, @BangLuongId, @NvId, @CaId, @Ngay, @TrangThai, 1, 1, CURRENT_TIMESTAMP)";
                            await conn.ExecuteAsync(sql, new
                            {
                                Id = currentId,
                                BangLuongId = blIdParam,
                                NvId = nvIdParam,
                                CaId = caIdParam,
                                Ngay = date,
                                TrangThai = trangThaiInt
                            });
                            savedCount++;
                            savedThisCell = true;
                        }
                        catch (Exception ex1) { lastEx = ex1; }

                        if (!savedThisCell)
                        {
                            // Attempt 2: Minimal insert (ID, TBANGLUONGID, DNHANVIENID, DCALAMVIECID, NGAY, TRANGTHAI)
                            try
                            {
                                string sql = @"
                                    INSERT INTO TBANGLUONGCHITIET 
                                    (ID, TBANGLUONGID, DNHANVIENID, DCALAMVIECID, NGAY, TRANGTHAI)
                                    VALUES 
                                    (@Id, @BangLuongId, @NvId, @CaId, @Ngay, @TrangThai)";
                                await conn.ExecuteAsync(sql, new
                                {
                                    Id = currentId,
                                    BangLuongId = blIdParam,
                                    NvId = nvIdParam,
                                    CaId = caIdParam,
                                    Ngay = date,
                                    TrangThai = trangThaiInt
                                });
                                savedCount++;
                                savedThisCell = true;
                            }
                            catch (Exception ex2) { lastEx = ex2; }
                        }

                        if (!savedThisCell)
                        {
                            // Attempt 3: Insert with STATUS=30
                            try
                            {
                                string sql = @"
                                    INSERT INTO TBANGLUONGCHITIET 
                                    (ID, TBANGLUONGID, DNHANVIENID, DCALAMVIECID, NGAY, TRANGTHAI, STATUS)
                                    VALUES 
                                    (@Id, @BangLuongId, @NvId, @CaId, @Ngay, @TrangThai, 30)";
                                await conn.ExecuteAsync(sql, new
                                {
                                    Id = currentId,
                                    BangLuongId = blIdParam,
                                    NvId = nvIdParam,
                                    CaId = caIdParam,
                                    Ngay = date,
                                    TrangThai = trangThaiInt
                                });
                                savedCount++;
                                savedThisCell = true;
                            }
                            catch (Exception ex3) { lastEx = ex3; }
                        }

                        if (!savedThisCell && lastEx != null)
                        {
                            return (false, "Lỗi lưu chi tiết chấm công: " + lastEx.Message);
                        }
                    }
                }

                // 2. Save TBANGLUONGTONGHOP (Salary summary per employee)
                try
                {
                    await conn.ExecuteAsync("DELETE FROM TBANGLUONGTONGHOP WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id", new { Id = bangLuongId });
                    int nextThId = await GetNextTBangLuongTongHopIdAsync(conn);

                    var firstRows = rows.Where(x => x.IsFirstShiftOfEmployee).ToList();
                    foreach (var empRow in firstRows)
                    {
                        object nvIdParam = empRow.NhanVienId;
                        if (int.TryParse(empRow.NhanVienId, out int nvInt))
                            nvIdParam = nvInt;
                        else if (!string.IsNullOrEmpty(empRow.TenNhanVien) && nvNameToId.TryGetValue(empRow.TenNhanVien.Trim(), out var foundNvId))
                        {
                            if (int.TryParse(foundNvId?.ToString(), out int fInt))
                                nvIdParam = fInt;
                            else
                                nvIdParam = foundNvId;
                        }

                        int currentThId = nextThId++;

                        try
                        {
                            string sqlTh = @"
                                INSERT INTO TBANGLUONGTONGHOP 
                                (ID, TBANGLUONGID, DNHANVIENID, LUONGTHANG, LUONGCA, TONGLUONG, THUONG, PHAT, TAMUNG, THUCNHAN, CACHTINHLUONG, STATUS, USERCREATEDID, TIMECREATED)
                                VALUES 
                                (@Id, @BangLuongId, @NvId, @LuongThang, @LuongCa, @TongLuong, @Thuong, @Phat, @TamUng, @ThucNhan, @CachTinhLuong, 1, 1, CURRENT_TIMESTAMP)";
                            await conn.ExecuteAsync(sqlTh, new
                            {
                                Id = currentThId,
                                BangLuongId = blIdParam,
                                NvId = nvIdParam,
                                LuongThang = empRow.LuongCoBan.ToString("0"),
                                LuongCa = empRow.LuongCa.ToString("0"),
                                TongLuong = empRow.TongLuong.ToString("0"),
                                Thuong = empRow.Thuong,
                                Phat = empRow.Phat.ToString("0"),
                                TamUng = empRow.TamUng.ToString("0"),
                                ThucNhan = empRow.ThucNhan,
                                CachTinhLuong = empRow.CachTinhLuong.ToString()
                            });
                        }
                        catch
                        {
                            string sqlThMin = @"
                                INSERT INTO TBANGLUONGTONGHOP 
                                (ID, TBANGLUONGID, DNHANVIENID, LUONGTHANG, LUONGCA, TONGLUONG, THUONG, PHAT, TAMUNG, THUCNHAN, CACHTINHLUONG)
                                VALUES 
                                (@Id, @BangLuongId, @NvId, @LuongThang, @LuongCa, @TongLuong, @Thuong, @Phat, @TamUng, @ThucNhan, @CachTinhLuong)";
                            await conn.ExecuteAsync(sqlThMin, new
                            {
                                Id = currentThId,
                                BangLuongId = blIdParam,
                                NvId = nvIdParam,
                                LuongThang = empRow.LuongCoBan.ToString("0"),
                                LuongCa = empRow.LuongCa.ToString("0"),
                                TongLuong = empRow.TongLuong.ToString("0"),
                                Thuong = empRow.Thuong,
                                Phat = empRow.Phat.ToString("0"),
                                TamUng = empRow.TamUng.ToString("0"),
                                ThucNhan = empRow.ThucNhan,
                                CachTinhLuong = empRow.CachTinhLuong.ToString()
                            });
                        }
                    }
                }
                catch (Exception exTh)
                {
                    Console.WriteLine("Warning saving TBANGLUONGTONGHOP: " + exTh.Message);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveChamCongMatrix: " + ex.Message);
                return (false, ex.Message);
            }
        }

        public static async Task<List<TinhLuongItemViewModel>> GetTinhLuongSummaryAsync(string bangLuongId, int month, int year)
        {
            var list = new List<TinhLuongItemViewModel>();
            if (string.IsNullOrEmpty(bangLuongId)) return list;

            try
            {
                var matrixRows = await GetChamCongMatrixAsync(bangLuongId, month, year);
                if (matrixRows == null || matrixRows.Count == 0) return list;

                int stt = 1;
                var groups = matrixRows.GroupBy(x => x.NhanVienId);
                foreach (var g in groups)
                {
                    var first = g.FirstOrDefault(x => x.IsFirstShiftOfEmployee) ?? g.First();
                    int workedShifts = 0;
                    int paidLeave = 0;
                    int unpaidLeave = 0;

                    foreach (var r in g)
                    {
                        foreach (var kv in r.DaysMap)
                        {
                            if (kv.Value.Status == "1") workedShifts++;
                            else if (kv.Value.Status == "2" || kv.Value.Status == "P") paidLeave++;
                            else if (kv.Value.Status == "3" || kv.Value.Status == "KP") unpaidLeave++;
                        }
                    }

                    var item = new TinhLuongItemViewModel
                    {
                        STT = stt++,
                        NhanVienId = first.NhanVienId,
                        MaNhanVien = first.MaNhanVien,
                        TenNhanVien = first.TenNhanVien,
                        SoNgayLam = workedShifts,
                        SoNgayNghiCoPhep = paidLeave,
                        SoNgayNghiKhongPhep = unpaidLeave,
                        LuongCoBan = first.LuongCoBan,
                        LuongCa = first.LuongCa,
                        CachTinhLuong = first.CachTinhLuong,
                        TongLuong = first.TongLuong,
                        Thuong = first.Thuong,
                        Phat = first.Phat,
                        TamUng = first.TamUng,
                        ThucNhan = first.ThucNhan
                    };
                    list.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetTinhLuongSummaryAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<(bool ok, string error)> SaveTinhLuongSummaryAsync(string bangLuongId, List<TinhLuongItemViewModel> rows)
        {
            if (string.IsNullOrEmpty(bangLuongId) || rows == null || rows.Count == 0) return (false, "Dữ liệu trống");

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                await conn.ExecuteAsync("DELETE FROM TBANGLUONGTONGHOP WHERE CAST(TBANGLUONGID AS VARCHAR(50)) = @Id", new { Id = bangLuongId });
                int nextThId = await GetNextTBangLuongTongHopIdAsync(conn);

                var dbNvRows = (await conn.QueryAsync("SELECT ID, NAME, CODE FROM DNHANVIEN")).Cast<IDictionary<string, object>>().ToList();
                var nvNameToId = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in dbNvRows)
                {
                    object idObj = GetValue(r, "ID");
                    string name = GetValue(r, "NAME")?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && idObj != null)
                        nvNameToId[name.Trim()] = idObj;
                }

                object blIdParam = bangLuongId;
                if (int.TryParse(bangLuongId, out int blInt)) blIdParam = blInt;

                foreach (var empRow in rows)
                {
                    object nvIdParam = empRow.NhanVienId;
                    if (int.TryParse(empRow.NhanVienId, out int nvInt))
                        nvIdParam = nvInt;
                    else if (!string.IsNullOrEmpty(empRow.TenNhanVien) && nvNameToId.TryGetValue(empRow.TenNhanVien.Trim(), out var foundNvId))
                    {
                        if (int.TryParse(foundNvId?.ToString(), out int fInt))
                            nvIdParam = fInt;
                        else
                            nvIdParam = foundNvId;
                    }

                    int currentThId = nextThId++;

                    try
                    {
                        string sqlTh = @"
                            INSERT INTO TBANGLUONGTONGHOP 
                            (ID, TBANGLUONGID, DNHANVIENID, LUONGTHANG, LUONGCA, TONGLUONG, THUONG, PHAT, TAMUNG, THUCNHAN, CACHTINHLUONG, NOTE, STATUS, USERCREATEDID, TIMECREATED)
                            VALUES 
                            (@Id, @BangLuongId, @NvId, @LuongThang, @LuongCa, @TongLuong, @Thuong, @Phat, @TamUng, @ThucNhan, @CachTinhLuong, @Note, 1, 1, CURRENT_TIMESTAMP)";
                        await conn.ExecuteAsync(sqlTh, new
                        {
                            Id = currentThId,
                            BangLuongId = blIdParam,
                            NvId = nvIdParam,
                            LuongThang = empRow.LuongCoBan.ToString("0"),
                            LuongCa = empRow.LuongCa.ToString("0"),
                            TongLuong = empRow.TongLuong.ToString("0"),
                            Thuong = empRow.Thuong,
                            Phat = empRow.Phat.ToString("0"),
                            TamUng = empRow.TamUng.ToString("0"),
                            ThucNhan = empRow.ThucNhan,
                            CachTinhLuong = empRow.CachTinhLuong.ToString(),
                            Note = empRow.GhiChu ?? ""
                        });
                    }
                    catch
                    {
                        string sqlThMin = @"
                            INSERT INTO TBANGLUONGTONGHOP 
                            (ID, TBANGLUONGID, DNHANVIENID, LUONGTHANG, LUONGCA, TONGLUONG, THUONG, PHAT, TAMUNG, THUCNHAN, CACHTINHLUONG, NOTE)
                            VALUES 
                            (@Id, @BangLuongId, @NvId, @LuongThang, @LuongCa, @TongLuong, @Thuong, @Phat, @TamUng, @ThucNhan, @CachTinhLuong, @Note)";
                        await conn.ExecuteAsync(sqlThMin, new
                        {
                            Id = currentThId,
                            BangLuongId = blIdParam,
                            NvId = nvIdParam,
                            LuongThang = empRow.LuongCoBan.ToString("0"),
                            LuongCa = empRow.LuongCa.ToString("0"),
                            TongLuong = empRow.TongLuong.ToString("0"),
                            Thuong = empRow.Thuong,
                            Phat = empRow.Phat.ToString("0"),
                            TamUng = empRow.TamUng.ToString("0"),
                            ThucNhan = empRow.ThucNhan,
                            CachTinhLuong = empRow.CachTinhLuong.ToString(),
                            Note = empRow.GhiChu ?? ""
                        });
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveTinhLuongSummary: " + ex.Message);
                return (false, ex.Message);
            }
        }
    }
}
