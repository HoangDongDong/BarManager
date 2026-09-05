using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Dapper;
using ExcelDataReader;

namespace QuanLyBar.Client.Services
{
    public class CongNoBanDauItemViewModel : INotifyPropertyChanged
    {
        public int Stt { get; set; }
        public string KhachHangId { get; set; } = "";
        public string MaKhach { get; set; } = "";
        public string TenKhach { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string DienThoai { get; set; } = "";

        private decimal _soTien;
        public decimal SoTien
        {
            get => _soTien;
            set
            {
                if (_soTien != value)
                {
                    _soTien = value;
                    OnPropertyChanged(nameof(SoTien));
                    OnPropertyChanged(nameof(SoTienDisplay));
                }
            }
        }

        public string SoTienDisplay => SoTien.ToString("N0");

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalCongNoBanDauService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        public static async Task<List<CongNoBanDauItemViewModel>> GetCongNoKhachHangBanDauListAsync()
        {
            var list = new List<CongNoBanDauItemViewModel>();

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                // 1. Get all active customers
                string sqlKhach = @"
                    SELECT ID, MAKHACH, NAME, DIACHI, DIENTHOAI
                    FROM DKHACHHANG
                    WHERE (STATUS IS NULL OR STATUS > 0)
                    ORDER BY MAKHACH, NAME";

                var customers = (await conn.QueryAsync(sqlKhach)).ToList();

                // 2. Get existing initial debt records from TDONHANG (prefix CNBD_ or Note = 'Công nợ ban đầu')
                string sqlExisting = @"
                    SELECT CAST(DKHACHHANGID AS VARCHAR(50)) AS KHACHID, COALESCE(TONGCONG, 0) AS SOTIEN
                    FROM TDONHANG
                    WHERE (STATUS IS NULL OR STATUS > 0)
                      AND (NAME LIKE 'CNBD_%' OR NOTE = 'Công nợ ban đầu')";

                var existingDebts = new Dictionary<string, decimal>();
                try
                {
                    var debtRows = (await conn.QueryAsync(sqlExisting)).ToList();
                    foreach (var r in debtRows)
                    {
                        string kId = r.KHACHID?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(kId))
                        {
                            decimal st = 0;
                            if (r.SOTIEN != null) decimal.TryParse(r.SOTIEN.ToString(), out st);
                            existingDebts[kId] = st;
                        }
                    }
                }
                catch { }

                int stt = 1;
                foreach (var c in customers)
                {
                    string id = c.ID?.ToString() ?? "";
                    string ma = c.MAKHACH?.ToString() ?? "";
                    string ten = c.NAME?.ToString() ?? "";
                    string dc = c.DIACHI?.ToString() ?? "";
                    string dt = c.DIENTHOAI?.ToString() ?? "";

                    decimal soTien = 0;
                    if (existingDebts.TryGetValue(id, out decimal st))
                    {
                        soTien = st;
                    }

                    list.Add(new CongNoBanDauItemViewModel
                    {
                        Stt = stt++,
                        KhachHangId = id,
                        MaKhach = ma,
                        TenKhach = ten,
                        DiaChi = dc,
                        DienThoai = dt,
                        SoTien = soTien
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCongNoKhachHangBanDauListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<(bool ok, string error)> SaveCongNoKhachHangBanDauAsync(DateTime ngayChot, List<CongNoBanDauItemViewModel> items)
        {
            if (items == null) return (false, "Không có dữ liệu");

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                object userId = 1;
                try
                {
                    var u = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                    if (u != null) userId = u;
                }
                catch { }

                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.KhachHangId)) continue;

                    string soDonHang = $"CNBD_{item.MaKhach ?? item.KhachHangId}";

                    // Check if already exists in TDONHANG
                    string checkSql = "SELECT FIRST 1 ID FROM TDONHANG WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhachId AND (NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_%')";
                    var existingId = await conn.ExecuteScalarAsync<object>(checkSql, new { KhachId = item.KhachHangId });

                    if (existingId != null)
                    {
                        if (item.SoTien > 0)
                        {
                            // Update
                            string updateSql = @"
                                UPDATE TDONHANG SET
                                    NGAY = @Ngay,
                                    TIENHANG = @SoTien,
                                    TONGCONG = @SoTien,
                                    CONNO = @SoTien,
                                    TIENTHANHTOAN = 0,
                                    STATUS = 1,
                                    NOTE = 'Công nợ ban đầu',
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE ID = @Id";
                            await conn.ExecuteAsync(updateSql, new { Id = existingId, Ngay = ngayChot, SoTien = item.SoTien });
                        }
                        else
                        {
                            // If debt is 0, delete or set to 0
                            string updateSql = @"
                                UPDATE TDONHANG SET
                                    NGAY = @Ngay,
                                    TIENHANG = 0,
                                    TONGCONG = 0,
                                    CONNO = 0,
                                    TIENTHANHTOAN = 0,
                                    STATUS = 0,
                                    NOTE = 'Công nợ ban đầu',
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE ID = @Id";
                            await conn.ExecuteAsync(updateSql, new { Id = existingId, Ngay = ngayChot });
                        }
                    }
                    else if (item.SoTien > 0)
                    {
                        // Insert new initial debt order
                        string newId = Guid.NewGuid().ToString();

                        string insertSql = @"
                            INSERT INTO TDONHANG (
                                ID, NAME, NGAY, DKHACHHANGID, TIENHANG, TONGCONG, TIENTHANHTOAN, CONNO, 
                                NOTE, STATUS, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @SoDonHang, @Ngay, @KhachId, @SoTien, @SoTien, 0, @SoTien,
                                'Công nợ ban đầu', 1, @UserId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = newId,
                            SoDonHang = soDonHang,
                            Ngay = ngayChot,
                            KhachId = item.KhachHangId,
                            SoTien = item.SoTien,
                            UserId = userId
                        });
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<List<CongNoBanDauItemViewModel>> GetCongNoNhaCungCapBanDauListAsync()
        {
            var list = new List<CongNoBanDauItemViewModel>();

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                // 1. Get all active suppliers
                string sqlNcc = @"
                    SELECT ID, MANHACUNGCAP, NAME, DIACHI, DIENTHOAI
                    FROM DNHACUNGCAP
                    WHERE (STATUS IS NULL OR STATUS <> 0)
                    ORDER BY MANHACUNGCAP, NAME";

                var suppliers = (await conn.QueryAsync(sqlNcc)).ToList();

                // 2. Get existing initial debt records from TDONHANG (prefix CNBD_NCC_ or Note = 'Công nợ ban đầu')
                string sqlExisting = @"
                    SELECT CAST(DNHACUNGCAPID AS VARCHAR(50)) AS NCCID, COALESCE(TONGCONG, 0) AS SOTIEN
                    FROM TDONHANG
                    WHERE (STATUS IS NULL OR STATUS <> 0)
                      AND (NAME LIKE 'CNBD_NCC_%' OR (NOTE = 'Công nợ ban đầu' AND DNHACUNGCAPID IS NOT NULL))
                      AND (LOAI = 1 OR LOAI IS NULL)";

                var existingDebts = new Dictionary<string, decimal>();
                try
                {
                    var debtRows = (await conn.QueryAsync(sqlExisting)).ToList();
                    foreach (var r in debtRows)
                    {
                        string nId = r.NCCID?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(nId))
                        {
                            decimal st = 0;
                            if (r.SOTIEN != null) decimal.TryParse(r.SOTIEN.ToString(), out st);
                            existingDebts[nId] = st;
                        }
                    }
                }
                catch { }

                int stt = 1;
                foreach (var c in suppliers)
                {
                    string id = c.ID?.ToString() ?? "";
                    string ma = c.MANHACUNGCAP?.ToString() ?? "";
                    string ten = c.NAME?.ToString() ?? "";
                    string dc = c.DIACHI?.ToString() ?? "";
                    string dt = c.DIENTHOAI?.ToString() ?? "";

                    decimal soTien = 0;
                    if (existingDebts.TryGetValue(id, out decimal st))
                    {
                        soTien = st;
                    }

                    list.Add(new CongNoBanDauItemViewModel
                    {
                        Stt = stt++,
                        KhachHangId = id,
                        MaKhach = ma,
                        TenKhach = ten,
                        DiaChi = dc,
                        DienThoai = dt,
                        SoTien = soTien
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetCongNoNhaCungCapBanDauListAsync: " + ex.Message);
            }

            return list;
        }

        public static async Task<(bool ok, string error)> SaveCongNoNhaCungCapBanDauAsync(DateTime ngayChot, List<CongNoBanDauItemViewModel> items)
        {
            if (items == null) return (false, "Không có dữ liệu");

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                object userId = 1;
                try
                {
                    var u = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                    if (u != null) userId = u;
                }
                catch { }

                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.KhachHangId)) continue;

                    string soDonHang = $"CNBD_NCC_{item.MaKhach ?? item.KhachHangId}";

                    // Check if already exists in TDONHANG
                    string checkSql = "SELECT FIRST 1 ID FROM TDONHANG WHERE CAST(DNHACUNGCAPID AS VARCHAR(50)) = @NccId AND (NOTE = 'Công nợ ban đầu' OR NAME LIKE 'CNBD_NCC_%')";
                    var existingId = await conn.ExecuteScalarAsync<object>(checkSql, new { NccId = item.KhachHangId });

                    if (existingId != null)
                    {
                        if (item.SoTien > 0)
                        {
                            string updateSql = @"
                                UPDATE TDONHANG SET
                                    NGAY = @Ngay,
                                    TIENHANG = @SoTien,
                                    TONGCONG = @SoTien,
                                    CONNO = @SoTien,
                                    CONLAI = @SoTien,
                                    THANHTOAN = 0,
                                    STATUS = 1,
                                    LOAI = 1,
                                    NOTE = 'Công nợ ban đầu',
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE ID = @Id";
                            await conn.ExecuteAsync(updateSql, new { Id = existingId, Ngay = ngayChot, SoTien = item.SoTien });
                        }
                        else
                        {
                            string updateSql = @"
                                UPDATE TDONHANG SET
                                    NGAY = @Ngay,
                                    TIENHANG = 0,
                                    TONGCONG = 0,
                                    CONNO = 0,
                                    CONLAI = 0,
                                    THANHTOAN = 0,
                                    STATUS = 0,
                                    NOTE = 'Công nợ ban đầu',
                                    TIMEMODIFIED = CURRENT_TIMESTAMP
                                WHERE ID = @Id";
                            await conn.ExecuteAsync(updateSql, new { Id = existingId, Ngay = ngayChot });
                        }
                    }
                    else if (item.SoTien > 0)
                    {
                        string newId = Guid.NewGuid().ToString();

                        string insertSql = @"
                            INSERT INTO TDONHANG (
                                ID, NAME, NGAY, DNHACUNGCAPID, LOAI, TIENHANG, TONGCONG, THANHTOAN, CONLAI, CONNO, 
                                NOTE, STATUS, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @SoDonHang, @Ngay, @NccId, 1, @SoTien, @SoTien, 0, @SoTien, @SoTien,
                                'Công nợ ban đầu', 1, @UserId, CURRENT_TIMESTAMP
                            )";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = newId,
                            SoDonHang = soDonHang,
                            Ngay = ngayChot,
                            NccId = item.KhachHangId,
                            SoTien = item.SoTien,
                            UserId = userId
                        });
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static void ExportMauCongNo(string destinationPath, List<CongNoBanDauItemViewModel> items)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("CongNo");

            // Header row with 3 columns: Mã, Tên, Công nợ đầu
            ws.Cell(1, 1).Value = "Mã";
            ws.Cell(1, 2).Value = "Tên";
            ws.Cell(1, 3).Value = "Công nợ đầu";

            var header = ws.Range(1, 1, 1, 3);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F6FB");
            header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Fill actual customer data from database
            if (items != null && items.Count > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    int row = i + 2;
                    var it = items[i];
                    ws.Cell(row, 1).SetValue(it.MaKhach ?? "");
                    ws.Cell(row, 2).SetValue(it.TenKhach ?? "");
                    ws.Cell(row, 3).SetValue(it.SoTien);

                    ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0";

                    var dataRange = ws.Range(row, 1, row, 3);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }
            }

            ws.Column(1).Width = 15;
            ws.Column(2).Width = 32;
            ws.Column(3).Width = 18;

            workbook.SaveAs(destinationPath);
        }

        public static List<string> GetExcelColumnNames(string filePath)
        {
            var cols = new List<string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            if (ds.Tables.Count > 0)
            {
                var dt = ds.Tables[0];
                foreach (DataColumn c in dt.Columns)
                {
                    cols.Add(c.ColumnName?.Trim() ?? "");
                }
            }

            return cols;
        }

        public static List<(string MaKhach, decimal SoTien)> ReadExcelWithMapping(string filePath, string colMaExcel, string colTienExcel)
        {
            var result = new List<(string MaKhach, decimal SoTien)>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            if (ds.Tables.Count > 0)
            {
                var dt = ds.Tables[0];
                int colMaIdx = -1;
                int colTienIdx = -1;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (!string.IsNullOrEmpty(colMaExcel) && string.Equals(dt.Columns[i].ColumnName?.Trim(), colMaExcel.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        colMaIdx = i;
                    }
                    if (!string.IsNullOrEmpty(colTienExcel) && string.Equals(dt.Columns[i].ColumnName?.Trim(), colTienExcel.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        colTienIdx = i;
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    string ma = colMaIdx >= 0 ? row[colMaIdx]?.ToString()?.Trim() ?? "" : "";
                    string tienStr = colTienIdx >= 0 ? row[colTienIdx]?.ToString()?.Trim() ?? "0" : "0";

                    decimal soTien = 0;
                    if (!string.IsNullOrEmpty(tienStr))
                    {
                        tienStr = tienStr.Replace(",", "").Replace(".", "").Replace(" ", "");
                        decimal.TryParse(tienStr, out soTien);
                    }

                    if (!string.IsNullOrEmpty(ma))
                    {
                        result.Add((ma, soTien));
                    }
                }
            }

            return result;
        }
    }
}
