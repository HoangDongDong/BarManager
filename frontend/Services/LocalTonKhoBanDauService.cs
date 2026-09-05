using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Dapper;
using ExcelDataReader;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class TonKhoBanDauItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Id { get; set; } = "";
        public string MaSanCo { get; set; } = "";
        public string MaHang { get; set; } = "";
        public string TenHang { get; set; } = "";
        public string DmathangId { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";
        public string Dvt { get; set; } = "";

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
                    OnPropertyChanged(nameof(GiaTri));
                    OnPropertyChanged(nameof(GiaTriFormatted));
                }
            }
        }
        public string TonFormatted => Ton != 0 ? Ton.ToString("N0") : "0";

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
                    OnPropertyChanged(nameof(GiaTri));
                    OnPropertyChanged(nameof(GiaTriFormatted));
                }
            }
        }
        public string GiaVonFormatted => GiaVon != 0 ? GiaVon.ToString("N0") : "0";

        public decimal GiaTri => Ton * GiaVon;
        public string GiaTriFormatted => GiaTri != 0 ? GiaTri.ToString("N0") : "0";
    }

    public static class LocalTonKhoBanDauService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        public static async Task<List<KhoHangComboItem>> GetKhoHangListAsync()
        {
            var list = new List<KhoHangComboItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DKHOHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();
                    foreach (var r in rows)
                    {
                        list.Add(new KhoHangComboItem
                        {
                            Id = r.ID?.ToString()?.Trim() ?? "",
                            Name = r.NAME?.ToString() ?? "",
                            Code = r.ID?.ToString()?.Trim() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetKhoHangListAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<(DateTime NgayChot, List<TonKhoBanDauItemViewModel> Items)> GetTonKhoBanDauListAsync(string khoId)
        {
            var list = new List<TonKhoBanDauItemViewModel>();
            DateTime ngayChot = DateTime.Today;

            if (string.IsNullOrEmpty(khoId)) return (ngayChot, list);

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Tải toàn bộ danh mục mặt hàng
                    string sqlMatHang = @"
                        SELECT 
                            CAST(m.ID AS VARCHAR(50)) as DmathangId,
                            m.CODE as MaHang,
                            m.NAME as TenHang,
                            CAST(m.DDONVITINHID AS VARCHAR(50)) as DdonvitinhId,
                            d.NAME as TenDonViTinh,
                            COALESCE(m.GIAVON, m.GIANHAP, 0) as GiaVon
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH d ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                        ORDER BY m.NAME";

                    var matHangs = (await conn.QueryAsync(sqlMatHang)).ToList();

                    // 2. Tìm phiếu tồn kho ban đầu hiện tại của kho
                    string sqlExisting = @"
                        SELECT FIRST 1 ID, NGAY 
                        FROM TDONHANG 
                        WHERE (STATUS IS NULL OR STATUS <> 0)
                          AND (TRIM(CAST(DKHONHAPID AS VARCHAR(50))) = @KhoId OR UPPER(TRIM(CAST(DKHONHAPID AS VARCHAR(50)))) = UPPER(@KhoId))
                          AND (LOAI = 1 OR LOAI IS NULL)
                          AND (NOTE = 'Tồn kho ban đầu' OR NAME = @OrderName OR NAME LIKE 'TKBD_%')
                        ORDER BY TIMECREATED DESC";

                    string orderName = $"TKBD_{khoId.Trim()}";
                    var existingOrder = await conn.QueryFirstOrDefaultAsync(sqlExisting, new { KhoId = khoId.Trim(), OrderName = orderName });

                    var existingDetails = new Dictionary<string, (decimal Ton, decimal GiaVon)>();
                    if (existingOrder != null)
                    {
                        if (existingOrder.NGAY != null)
                        {
                            if (existingOrder.NGAY is DateTime dtVal) ngayChot = dtVal;
                            else if (DateTime.TryParse(existingOrder.NGAY.ToString(), out DateTime dtParsed)) ngayChot = dtParsed;
                        }

                        string orderId = existingOrder.ID?.ToString();
                        string sqlDetails = @"
                            SELECT 
                                CAST(DMATHANGID AS VARCHAR(50)) as DMATHANGID,
                                COALESCE(SLNHAP, 0) as SLNHAP,
                                COALESCE(DONGIA, 0) as DONGIA
                            FROM TDONHANGCHITIET
                            WHERE (STATUS IS NULL OR STATUS <> 0)
                              AND CAST(TDONHANGID AS VARCHAR(50)) = @OrderId";

                        var dRows = (await conn.QueryAsync(sqlDetails, new { OrderId = orderId })).ToList();
                        foreach (var dr in dRows)
                        {
                            string mhId = dr.DMATHANGID?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(mhId))
                            {
                                decimal ton = 0;
                                if (dr.SLNHAP != null) decimal.TryParse(dr.SLNHAP.ToString(), out ton);
                                decimal gv = 0;
                                if (dr.DONGIA != null) decimal.TryParse(dr.DONGIA.ToString(), out gv);

                                existingDetails[mhId] = (ton, gv);
                            }
                        }
                    }

                    foreach (var m in matHangs)
                    {
                        string mhId = m.DMATHANGID?.ToString()?.Trim() ?? "";
                        decimal ton = 0;
                        decimal giaVon = 0;

                        if (existingDetails.TryGetValue(mhId, out var saved))
                        {
                            ton = saved.Ton;
                            giaVon = saved.GiaVon;
                        }

                        list.Add(new TonKhoBanDauItemViewModel
                        {
                            Id = mhId,
                            MaSanCo = "",
                            MaHang = m.MAHANG?.ToString() ?? "",
                            TenHang = m.TENHANG?.ToString() ?? "",
                            DmathangId = mhId,
                            DdonvitinhId = m.DDONVITINHID?.ToString() ?? "",
                            Dvt = m.TENDONVITINH?.ToString() ?? "",
                            Ton = ton,
                            GiaVon = giaVon
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetTonKhoBanDauListAsync: " + ex.Message);
            }

            return (ngayChot, list);
        }

        public static async Task<bool> SaveTonKhoBanDauAsync(string khoId, DateTime ngayChot, List<TonKhoBanDauItemViewModel> items)
        {
            if (string.IsNullOrEmpty(khoId) || items == null) return false;

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string userId = null;
                            try
                            {
                                userId = await conn.QueryFirstOrDefaultAsync<string>(
                                    "SELECT FIRST 1 CAST(ID AS VARCHAR(50)) FROM SUSER WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY TIMECREATED", 
                                    transaction: trans);
                            }
                            catch { }
                            if (string.IsNullOrEmpty(userId))
                            {
                                userId = "1";
                            }

                            string orderName = $"TKBD_{khoId.Trim()}";

                            // Tìm phiếu cũ
                            string sqlFind = @"
                                SELECT FIRST 1 ID 
                                FROM TDONHANG 
                                WHERE (TRIM(CAST(DKHONHAPID AS VARCHAR(50))) = @KhoId OR UPPER(TRIM(CAST(DKHONHAPID AS VARCHAR(50)))) = UPPER(@KhoId))
                                  AND (LOAI = 1 OR LOAI IS NULL)
                                  AND (NOTE = 'Tồn kho ban đầu' OR NAME = @OrderName OR NAME LIKE 'TKBD_%')";

                            var oldOrderId = await conn.QueryFirstOrDefaultAsync<string>(sqlFind, new { KhoId = khoId.Trim(), OrderName = orderName }, transaction: trans);

                            string orderId = oldOrderId;
                            if (string.IsNullOrEmpty(orderId))
                            {
                                orderId = Guid.NewGuid().ToString();
                            }

                            decimal tongGiaTri = items.Sum(x => x.GiaTri);

                            // Xóa chi tiết cũ nếu đã có
                            if (!string.IsNullOrEmpty(oldOrderId))
                            {
                                await conn.ExecuteAsync("DELETE FROM TDONHANGCHITIET WHERE CAST(TDONHANGID AS VARCHAR(50)) = @OrderId", new { OrderId = oldOrderId }, transaction: trans);

                                string sqlUpdateOrder = @"
                                    UPDATE TDONHANG 
                                    SET NGAY = @Ngay,
                                        TONGCONG = @TongCong,
                                        TIENHANG = @TongCong,
                                        CONLAI = @TongCong,
                                        STATUS = 30,
                                        NOTE = 'Tồn kho ban đầu',
                                        USERMODIFIEDID = @UserId,
                                        TIMEMODIFIED = @Now
                                    WHERE CAST(ID AS VARCHAR(50)) = @OrderId";

                                await conn.ExecuteAsync(sqlUpdateOrder, new {
                                    Ngay = ngayChot.Date,
                                    TongCong = tongGiaTri,
                                    Now = DateTime.Now,
                                    UserId = userId,
                                    OrderId = oldOrderId
                                }, transaction: trans);
                            }
                            else
                            {
                                string sqlInsertOrder = @"
                                    INSERT INTO TDONHANG (
                                        ID, NAME, NGAY, LOAI, DKHONHAPID, TONGCONG, TIENHANG, CONLAI, THANHTOAN, NOTE, STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @Id, @Name, @Ngay, 1, @KhoId, @TongCong, @TongCong, @TongCong, 0, 'Tồn kho ban đầu', 30, @UserId, @Now
                                    )";

                                await conn.ExecuteAsync(sqlInsertOrder, new {
                                    Id = orderId,
                                    Name = orderName,
                                    Ngay = ngayChot.Date,
                                    KhoId = khoId.Trim(),
                                    TongCong = tongGiaTri,
                                    UserId = userId,
                                    Now = DateTime.Now
                                }, transaction: trans);
                            }

                            // Chèn các chi tiết tồn kho
                            string sqlInsertDetail = @"
                                INSERT INTO TDONHANGCHITIET (
                                    ID, TDONHANGID, DMATHANGID, TENHANG, DDONVITINHID, SLNHAP, SLXUAT, DONGIA, THANHTIEN, DKHOHANGID, NOTE, STATUS, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @TdonhangId, @DmathangId, @TenHang, @DdonvitinhId, @SlNhap, 0, @DonGia, @ThanhTien, @KhoId, 'Tồn kho ban đầu', 30, @UserId, @Now
                                )";

                            foreach (var item in items)
                            {
                                if (item.Ton != 0 || item.GiaVon != 0)
                                {
                                    string detailId = Guid.NewGuid().ToString();
                                    await conn.ExecuteAsync(sqlInsertDetail, new {
                                        Id = detailId,
                                        TdonhangId = orderId,
                                        DmathangId = item.DmathangId,
                                        TenHang = item.TenHang ?? "",
                                        DdonvitinhId = string.IsNullOrEmpty(item.DdonvitinhId) ? null : item.DdonvitinhId,
                                        SlNhap = item.Ton,
                                        DonGia = item.GiaVon,
                                        ThanhTien = item.GiaTri,
                                        KhoId = khoId.Trim(),
                                        UserId = userId,
                                        Now = DateTime.Now
                                    }, transaction: trans);

                                    // Cập nhật giá vốn mặt hàng nếu có
                                    if (item.GiaVon > 0)
                                    {
                                        await conn.ExecuteAsync(@"
                                            UPDATE DMATHANG 
                                            SET GIAVON = @GiaVon 
                                            WHERE CAST(ID AS VARCHAR(50)) = @MhId", 
                                            new { GiaVon = item.GiaVon, MhId = item.DmathangId }, 
                                            transaction: trans);
                                    }
                                }
                            }

                            trans.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            Console.WriteLine("Error SaveTonKhoBanDauAsync inside trans: " + ex.Message);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveTonKhoBanDauAsync: " + ex.Message);
                return false;
            }
        }

        public static bool ExportMauTonKho(string filePath, List<TonKhoBanDauItemViewModel> items)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    // Header matching MauTonKho.xls: Mã sẵn có | Mã hàng | Tên hàng | Tồn | Giá vốn
                    worksheet.Cell(1, 1).Value = "Mã sẵn có";
                    worksheet.Cell(1, 2).Value = "Mã hàng";
                    worksheet.Cell(1, 3).Value = "Tên hàng";
                    worksheet.Cell(1, 4).Value = "Tồn";
                    worksheet.Cell(1, 5).Value = "Giá vốn";

                    var headerRange = worksheet.Range(1, 1, 1, 5);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#dce6f1");
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int row = 2;
                    if (items != null)
                    {
                        foreach (var it in items)
                        {
                            worksheet.Cell(row, 1).Value = ""; // Không xuất dữ liệu của Mã sẵn có
                            worksheet.Cell(row, 2).Value = it.MaHang;
                            worksheet.Cell(row, 3).Value = it.TenHang;
                            worksheet.Cell(row, 4).Value = it.Ton;
                            worksheet.Cell(row, 5).Value = it.GiaVon;
                            row++;
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ExportMauTonKho: " + ex.Message);
                return false;
            }
        }

        public static List<string> GetExcelColumns(string filePath)
        {
            var cols = new List<string>();
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    if (ds.Tables.Count > 0)
                    {
                        var dt = ds.Tables[0];
                        foreach (DataColumn c in dt.Columns)
                        {
                            string colName = c.ColumnName?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(colName))
                            {
                                cols.Add(colName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetExcelColumns with ExcelDataReader: " + ex.Message);
            }

            // Fallback using ClosedXML if cols is empty and file is .xlsx
            if (cols.Count == 0 && filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var ws = workbook.Worksheets.FirstOrDefault();
                        if (ws != null)
                        {
                            var firstRow = ws.Row(1);
                            foreach (var cell in firstRow.CellsUsed())
                            {
                                string val = cell.GetString().Trim();
                                if (!string.IsNullOrEmpty(val))
                                {
                                    cols.Add(val);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error GetExcelColumns with ClosedXML: " + ex.Message);
                }
            }

            return cols;
        }

        public static (List<(string DmathangId, string MaSanCo, string MaHang, string TenHang, decimal Ton, decimal GiaVon)> Data, List<string> UnmatchedItems) 
            ReadExcelDataWithMapping(string filePath, Dictionary<string, string> columnMappings, List<TonKhoBanDauItemViewModel> currentItems)
        {
            var results = new List<(string, string, string, string, decimal, decimal)>();
            var unmatched = new List<string>();

            try
            {
                var dictByName = new Dictionary<string, TonKhoBanDauItemViewModel>(StringComparer.OrdinalIgnoreCase);
                var dictByCode = new Dictionary<string, TonKhoBanDauItemViewModel>(StringComparer.OrdinalIgnoreCase);
                var dictByMaSanCo = new Dictionary<string, TonKhoBanDauItemViewModel>(StringComparer.OrdinalIgnoreCase);

                foreach (var it in currentItems)
                {
                    if (!string.IsNullOrEmpty(it.TenHang) && !dictByName.ContainsKey(it.TenHang.Trim()))
                    {
                        dictByName[it.TenHang.Trim()] = it;
                    }
                    if (!string.IsNullOrEmpty(it.MaHang) && !dictByCode.ContainsKey(it.MaHang.Trim()))
                    {
                        dictByCode[it.MaHang.Trim()] = it;
                    }
                    if (!string.IsNullOrEmpty(it.MaSanCo) && !dictByMaSanCo.ContainsKey(it.MaSanCo.Trim()))
                    {
                        dictByMaSanCo[it.MaSanCo.Trim()] = it;
                    }
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                bool readSuccess = false;
                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        if (ds.Tables.Count > 0)
                        {
                            var table = ds.Tables[0];
                            int colMaHangHoaIdx = -1;
                            int colTenHangIdx = -1;
                            int colTonIdx = -1;
                            int colGiaVonIdx = -1;
                            bool isMaSanCoMapped = false;

                            for (int c = 0; c < table.Columns.Count; c++)
                            {
                                string headerName = table.Columns[c].ColumnName?.Trim() ?? "";
                                string lowerH = headerName.ToLower();

                                if (columnMappings.TryGetValue(headerName, out var mapped))
                                {
                                    if (mapped == "Mã hàng hóa" || mapped == "Mã sẵn có" || mapped == "Mã hàng")
                                    {
                                        colMaHangHoaIdx = c;
                                        if (lowerH.Contains("sẵn có") || lowerH.Contains("san co"))
                                        {
                                            isMaSanCoMapped = true;
                                        }
                                    }
                                    else if (mapped == "Tồn") colTonIdx = c;
                                    else if (mapped == "Giá vốn") colGiaVonIdx = c;
                                }

                                if (colTenHangIdx < 0 && (lowerH == "tên hàng" || lowerH.Contains("tenhang") || lowerH == "tên")) colTenHangIdx = c;
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                string maHangHoa = colMaHangHoaIdx >= 0 ? row[colMaHangHoaIdx]?.ToString()?.Trim() ?? "" : "";
                                string tenHang = colTenHangIdx >= 0 ? row[colTenHangIdx]?.ToString()?.Trim() ?? "" : "";

                                if (string.IsNullOrEmpty(maHangHoa) && string.IsNullOrEmpty(tenHang))
                                {
                                    continue;
                                }

                                string tonStr = colTonIdx >= 0 ? row[colTonIdx]?.ToString()?.Trim() ?? "0" : "0";
                                string giaVonStr = colGiaVonIdx >= 0 ? row[colGiaVonIdx]?.ToString()?.Trim() ?? "0" : "0";

                                decimal.TryParse(tonStr.Replace(",", "").Replace(".", ","), out decimal ton);
                                if (ton == 0) decimal.TryParse(tonStr.Replace(",", ""), out ton);

                                decimal.TryParse(giaVonStr.Replace(",", "").Replace(".", ","), out decimal giaVon);
                                if (giaVon == 0) decimal.TryParse(giaVonStr.Replace(",", ""), out giaVon);

                                TonKhoBanDauItemViewModel matched = null;

                                // 1. Nếu có mã ở cột được ánh xạ: kiểm tra nghiêm ngặt
                                if (!string.IsNullOrEmpty(maHangHoa))
                                {
                                    if (isMaSanCoMapped)
                                    {
                                        if (dictByMaSanCo.TryGetValue(maHangHoa, out var mBySanCo))
                                        {
                                            matched = mBySanCo;
                                        }
                                        else if (int.TryParse(maHangHoa, out int num) && dictByMaSanCo.TryGetValue(num.ToString("D3"), out var mBySanCoPadded))
                                        {
                                            matched = mBySanCoPadded;
                                        }
                                    }
                                    else
                                    {
                                        if (dictByCode.TryGetValue(maHangHoa, out var mByCode))
                                        {
                                            matched = mByCode;
                                        }
                                    }

                                    // Nếu có mã nhưng không tìm thấy trong danh mục -> báo lỗi mã không tồn tại, không ghi
                                    if (matched == null)
                                    {
                                        if (!unmatched.Contains(maHangHoa))
                                        {
                                            unmatched.Add(maHangHoa);
                                        }
                                        continue;
                                    }
                                }
                                // 2. Nếu ô mã để trống: đối chiếu theo Tên hàng
                                else if (!string.IsNullOrEmpty(tenHang))
                                {
                                    if (dictByName.TryGetValue(tenHang, out var mByName))
                                    {
                                        matched = mByName;
                                    }
                                    else
                                    {
                                        if (!unmatched.Contains(tenHang))
                                        {
                                            unmatched.Add(tenHang);
                                        }
                                        continue;
                                    }
                                }

                                if (matched != null)
                                {
                                    results.Add((matched.Id, matched.MaSanCo, matched.MaHang, matched.TenHang, ton, giaVon));
                                }
                            }

                            readSuccess = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error ReadExcelDataWithMapping with ExcelDataReader: " + ex.Message);
                }

                // Fallback using ClosedXML
                if (!readSuccess && filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var ws = workbook.Worksheets.FirstOrDefault();
                        if (ws != null)
                        {
                            var headerRow = ws.Row(1);
                            int colMaHangHoaIdx = -1;
                            int colTenHangIdx = -1;
                            int colTonIdx = -1;
                            int colGiaVonIdx = -1;
                            bool isMaSanCoMapped = false;

                            int colIdx = 1;
                            foreach (var cell in headerRow.CellsUsed())
                            {
                                string headerName = cell.GetString().Trim();
                                string lowerH = headerName.ToLower();

                                if (columnMappings.TryGetValue(headerName, out var mapped))
                                {
                                    if (mapped == "Mã hàng hóa" || mapped == "Mã sẵn có" || mapped == "Mã hàng")
                                    {
                                        colMaHangHoaIdx = colIdx;
                                        if (lowerH.Contains("sẵn có") || lowerH.Contains("san co"))
                                        {
                                            isMaSanCoMapped = true;
                                        }
                                    }
                                    else if (mapped == "Tồn") colTonIdx = colIdx;
                                    else if (mapped == "Giá vốn") colGiaVonIdx = colIdx;
                                }

                                if (colTenHangIdx < 0 && (lowerH == "tên hàng" || lowerH.Contains("tenhang") || lowerH == "tên")) colTenHangIdx = colIdx;

                                colIdx++;
                            }

                            int rowCount = ws.LastRowUsed()?.RowNumber() ?? 0;
                            for (int r = 2; r <= rowCount; r++)
                            {
                                var row = ws.Row(r);
                                string maHangHoa = colMaHangHoaIdx >= 1 ? row.Cell(colMaHangHoaIdx).GetString()?.Trim() ?? "" : "";
                                string tenHang = colTenHangIdx >= 1 ? row.Cell(colTenHangIdx).GetString()?.Trim() ?? "" : "";

                                if (string.IsNullOrEmpty(maHangHoa) && string.IsNullOrEmpty(tenHang))
                                {
                                    continue;
                                }

                                string tonStr = colTonIdx >= 1 ? row.Cell(colTonIdx).GetString()?.Trim() ?? "0" : "0";
                                string giaVonStr = colGiaVonIdx >= 1 ? row.Cell(colGiaVonIdx).GetString()?.Trim() ?? "0" : "0";

                                decimal.TryParse(tonStr.Replace(",", "").Replace(".", ","), out decimal ton);
                                if (ton == 0) decimal.TryParse(tonStr.Replace(",", ""), out ton);

                                decimal.TryParse(giaVonStr.Replace(",", "").Replace(".", ","), out decimal giaVon);
                                if (giaVon == 0) decimal.TryParse(giaVonStr.Replace(",", ""), out giaVon);

                                TonKhoBanDauItemViewModel matched = null;

                                if (!string.IsNullOrEmpty(maHangHoa))
                                {
                                    if (isMaSanCoMapped)
                                    {
                                        if (dictByMaSanCo.TryGetValue(maHangHoa, out var mBySanCo))
                                        {
                                            matched = mBySanCo;
                                        }
                                        else if (int.TryParse(maHangHoa, out int num) && dictByMaSanCo.TryGetValue(num.ToString("D3"), out var mBySanCoPadded))
                                        {
                                            matched = mBySanCoPadded;
                                        }
                                    }
                                    else
                                    {
                                        if (dictByCode.TryGetValue(maHangHoa, out var mByCode))
                                        {
                                            matched = mByCode;
                                        }
                                    }

                                    if (matched == null)
                                    {
                                        if (!unmatched.Contains(maHangHoa))
                                        {
                                            unmatched.Add(maHangHoa);
                                        }
                                        continue;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(tenHang))
                                {
                                    if (dictByName.TryGetValue(tenHang, out var mByName))
                                    {
                                        matched = mByName;
                                    }
                                    else
                                    {
                                        if (!unmatched.Contains(tenHang))
                                        {
                                            unmatched.Add(tenHang);
                                        }
                                        continue;
                                    }
                                }

                                if (matched != null)
                                {
                                    results.Add((matched.Id, matched.MaSanCo, matched.MaHang, matched.TenHang, ton, giaVon));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ReadExcelDataWithMapping: " + ex.Message);
            }

            return (results, unmatched);
        }
    }
}
