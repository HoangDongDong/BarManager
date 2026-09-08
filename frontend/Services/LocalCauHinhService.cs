using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Data.SqlClient;

namespace QuanLyBar.Client.Services
{
    public static class LocalCauHinhService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static readonly Dictionary<string, string> _configCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string GetConfig(string key, string defaultValue = "")
        {
            if (_configCache.TryGetValue(key, out string val) && val != null)
                return val;
            return defaultValue;
        }

        public static bool GetBoolConfig(string key, bool defaultValue = false)
        {
            string val = GetConfig(key, defaultValue ? "1" : "0");
            return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase) || val == "30";
        }

        public static int GetIntConfig(string key, int defaultValue = 0)
        {
            string val = GetConfig(key, defaultValue.ToString());
            return int.TryParse(val, out int v) ? v : defaultValue;
        }

        public static decimal GetDecimalConfig(string key, decimal defaultValue = 0)
        {
            string val = GetConfig(key, defaultValue.ToString());
            return decimal.TryParse(val, out decimal v) ? v : defaultValue;
        }

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

        private static async Task<object> GetCurrentUserIdAsync(IDbConnection conn, IDbTransaction trans = null)
        {
            if (SessionContext.CurrentUser != null && !string.IsNullOrEmpty(SessionContext.CurrentUser.Id))
            {
                if (int.TryParse(SessionContext.CurrentUser.Id, out int intId)) return intId;
                return SessionContext.CurrentUser.Id;
            }

            try
            {
                var userId = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0", transaction: trans);
                if (userId != null && !Convert.IsDBNull(userId)) return userId;
            }
            catch { }

            try
            {
                var userId = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER", transaction: trans);
                if (userId != null && !Convert.IsDBNull(userId)) return userId;
            }
            catch { }

            return 1;
        }

        private static async Task<object> GetNextSConfigIdAsync(IDbConnection conn, IDbTransaction trans = null)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT FIRST 5 ID FROM SCONFIG", transaction: trans)).Cast<IDictionary<string, object>>().ToList();
                if (rows.Count > 0)
                {
                    object sampleId = GetValue(rows[0], "ID");
                    if (sampleId != null && int.TryParse(sampleId.ToString(), out _))
                    {
                        var allRows = (await conn.QueryAsync("SELECT ID FROM SCONFIG", transaction: trans)).Cast<IDictionary<string, object>>().ToList();
                        int maxId = 0;
                        foreach (var dict in allRows)
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
                    else
                    {
                        return Guid.NewGuid().ToString();
                    }
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

        private static async Task<object> GetNextSImageIdAsync(IDbConnection conn, IDbTransaction trans = null)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT FIRST 5 ID FROM SIMAGE", transaction: trans)).Cast<IDictionary<string, object>>().ToList();
                if (rows.Count > 0)
                {
                    object sampleId = GetValue(rows[0], "ID");
                    if (sampleId != null && int.TryParse(sampleId.ToString(), out _))
                    {
                        var allRows = (await conn.QueryAsync("SELECT ID FROM SIMAGE", transaction: trans)).Cast<IDictionary<string, object>>().ToList();
                        int maxId = 0;
                        foreach (var dict in allRows)
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
                    else
                    {
                        return Guid.NewGuid().ToString();
                    }
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

        public static async Task<Dictionary<string, string>> LoadAllConfigsAsync()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var rows = (await conn.QueryAsync("SELECT * FROM SCONFIG")).ToList();
                    foreach (object r in rows)
                    {
                        var rowDict = r as IDictionary<string, object>;
                        string name = GetValue(rowDict, "NAME")?.ToString()?.Trim() ?? "";
                        string val = GetValue(rowDict, "TEXTVALUE")?.ToString();
                        if (string.IsNullOrEmpty(val))
                            val = GetValue(rowDict, "INTVALUE")?.ToString();
                        if (string.IsNullOrEmpty(val))
                            val = GetValue(rowDict, "DECIMALVALUE")?.ToString();
                        if (string.IsNullOrEmpty(val))
                            val = GetValue(rowDict, "VALUE")?.ToString();
                        if (val == null) val = "";

                        if (!string.IsNullOrEmpty(name))
                        {
                            dict[name] = val;
                            _configCache[name] = val;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadAllConfigsAsync: " + ex.Message);
            }
            return dict;
        }

        public static async Task<Dictionary<string, string>> LoadTableFormatsAsync()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    try
                    {
                        var rows = (await conn.QueryAsync("SELECT NAME, FORMAT FROM STABLEDESC WHERE FORMAT IS NOT NULL")).ToList();
                        foreach (object r in rows)
                        {
                            var rowDict = r as IDictionary<string, object>;
                            string name = GetValue(rowDict, "NAME")?.ToString()?.Trim() ?? "";
                            string format = GetValue(rowDict, "FORMAT")?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(name))
                            {
                                dict[name] = format;
                            }
                        }
                    }
                    catch (Exception exSd)
                    {
                        Console.WriteLine("Error reading STABLEDESC: " + exSd.Message);
                    }

                    try
                    {
                        var sconfigFormats = (await conn.QueryAsync("SELECT NAME, TEXTVALUE FROM SCONFIG WHERE UPPER(NAME) STARTING WITH 'FORMAT_'")).ToList();
                        foreach (object r in sconfigFormats)
                        {
                            var rowDict = r as IDictionary<string, object>;
                            string sname = GetValue(rowDict, "NAME")?.ToString()?.Trim() ?? "";
                            string sval = GetValue(rowDict, "TEXTVALUE")?.ToString() ?? "";
                            if (sname.Length > 7 && !string.IsNullOrEmpty(sval))
                            {
                                string originalName = sname.Substring(7); // Remove 'FORMAT_'
                                dict[originalName] = sval;
                            }
                        }
                    }
                    catch (Exception exSc)
                    {
                        Console.WriteLine("Error reading SCONFIG formats: " + exSc.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTableFormatsAsync: " + ex.Message);
            }
            return dict;
        }

        public static async Task<string> GetFormatPatternAsync(string tableName, string defaultPattern)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Check SCONFIG first
                    try
                    {
                        string sconfigVal = await conn.ExecuteScalarAsync<string>(
                            "SELECT FIRST 1 TEXTVALUE FROM SCONFIG WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                            new { Name = "FORMAT_" + tableName });
                        if (!string.IsNullOrWhiteSpace(sconfigVal)) return sconfigVal.Trim();
                    }
                    catch { }

                    // 2. Check STABLEDESC
                    try
                    {
                        string tableDescVal = await conn.ExecuteScalarAsync<string>(
                            "SELECT FIRST 1 FORMAT FROM STABLEDESC WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                            new { Name = tableName });
                        if (!string.IsNullOrWhiteSpace(tableDescVal)) return tableDescVal.Trim();
                    }
                    catch { }
                }
            }
            catch { }
            return defaultPattern;
        }

        public static (DateTime? StartDate, DateTime? EndDate) GetResetPeriod(string pattern, DateTime date)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return (new DateTime(date.Year, 1, 1), new DateTime(date.Year, 12, 31));
            }

            if (pattern.IndexOf("(dd)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return (date.Date, date.Date);
            }
            else if (pattern.IndexOf("(MM)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var start = new DateTime(date.Year, date.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                return (start, end);
            }
            else if (pattern.IndexOf("(yyyy)", StringComparison.OrdinalIgnoreCase) >= 0 || pattern.IndexOf("(yy)", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var start = new DateTime(date.Year, 1, 1);
                var end = new DateTime(date.Year, 12, 31);
                return (start, end);
            }
            else
            {
                return (null, null);
            }
        }

        public static string ApplyPattern(string pattern, DateTime date, int sequence)
        {
            if (string.IsNullOrWhiteSpace(pattern)) pattern = "(yy)(******)";

            string result = pattern;
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(yyyy\)", date.ToString("yyyy"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(yy\)", date.ToString("yy"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(MM\)", date.ToString("MM"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\(dd\)", date.ToString("dd"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var match = System.Text.RegularExpressions.Regex.Match(result, @"\(\*+\)");
            if (match.Success)
            {
                int starCount = match.Value.Length - 2;
                if (starCount < 1) starCount = 5;
                if (starCount > 9) starCount = 9;
                string seqStr = sequence.ToString().PadLeft(starCount, '0');
                result = result.Replace(match.Value, seqStr);
            }
            else
            {
                result += sequence.ToString("D5");
            }

            return result;
        }

        public class CompanyInfoModel
        {
            public string Name { get; set; } = "TRỤ SỞ CHÍNH";
            public string Address { get; set; } = "Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội";
            public string Phone { get; set; } = "0909090880";
            public string Email { get; set; } = "";
            public string TaxCode { get; set; } = "";
            public string Fax { get; set; } = "";
            public string FormattedAddress
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(Address)) return "";
                    var a = Address.Trim();
                    if (a.StartsWith("Địa chỉ", StringComparison.OrdinalIgnoreCase) || a.StartsWith("ĐC", StringComparison.OrdinalIgnoreCase)) return a;
                    return $"Địa chỉ: {a}";
                }
            }
            public string FormattedContact
            {
                get
                {
                    var parts = new List<string>();
                    string p = CleanPhone(Phone);
                    if (!string.IsNullOrWhiteSpace(p) && p != "0") parts.Add($"Điện thoại: {p}");
                    if (!string.IsNullOrWhiteSpace(Fax) && Fax != "0") parts.Add($"Fax: {Fax}");
                    if (!string.IsNullOrWhiteSpace(Email) && Email != "0" && !Email.Equals("null", StringComparison.OrdinalIgnoreCase)) parts.Add($"Email: {Email}");
                    if (!string.IsNullOrWhiteSpace(TaxCode) && TaxCode != "0") parts.Add($"MST: {TaxCode}");
                    return string.Join("   ", parts);
                }
            }
            public byte[] LogoBytes { get; set; }

            public static string CleanPhone(string raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return "";
                return System.Text.RegularExpressions.Regex.Replace(raw, @"^(Điện thoại|ĐT|Phone|Tel)\s*[:：]\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            }
        }

        public static System.Windows.Media.Imaging.BitmapImage ImageFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            try
            {
                var bi = new System.Windows.Media.Imaging.BitmapImage();
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    bi.BeginInit();
                    bi.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                }
                bi.Freeze();
                return bi;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<CompanyInfoModel> GetCompanyInfoAsync(string branchNameOrId = null)
        {
            var configs = await LoadAllConfigsAsync();
            string cName = configs.TryGetValue("CompanyName", out var cn) && !string.IsNullOrWhiteSpace(cn) ? cn.Trim() : "";
            string cAddr = configs.TryGetValue("CompanyAddress", out var ca) && !string.IsNullOrWhiteSpace(ca) ? ca.Trim() : "";
            string cPhone = configs.TryGetValue("CompanyPhone", out var cp) && !string.IsNullOrWhiteSpace(cp) ? cp.Trim() : "";
            string cEmail = configs.TryGetValue("CompanyEmail", out var ce) && !string.IsNullOrWhiteSpace(ce) ? ce.Trim() : "";
            string cFax = configs.TryGetValue("CompanyFax", out var cf) && !string.IsNullOrWhiteSpace(cf) ? cf.Trim() : "";
            string cTax = configs.TryGetValue("CompanyTaxCode", out var ct) && !string.IsNullOrWhiteSpace(ct) ? ct.Trim() : "";

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    if (!string.IsNullOrWhiteSpace(branchNameOrId) && branchNameOrId != "Tất cả" && branchNameOrId != "[Tất cả]")
                    {
                        var storeRow = await conn.QueryFirstOrDefaultAsync("SELECT FIRST 1 * FROM DCUAHANG WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name)) OR CAST(ID AS VARCHAR(50)) = @Name", new { Name = branchNameOrId.Trim() });
                        if (storeRow != null)
                        {
                            var d = storeRow as IDictionary<string, object>;
                            string sName = GetValue(d, "NAME")?.ToString()?.Trim();
                            string sAddr = GetValue(d, "DIACHI")?.ToString()?.Trim();
                            string sPhone = GetValue(d, "DIENTHOAI")?.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(sName)) cName = sName;
                            if (!string.IsNullOrWhiteSpace(sAddr)) cAddr = sAddr;
                            if (!string.IsNullOrWhiteSpace(sPhone)) cPhone = sPhone;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(cName) || string.IsNullOrWhiteSpace(cAddr))
                        {
                            var storeRow = await conn.QueryFirstOrDefaultAsync("SELECT FIRST 1 * FROM DCUAHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY ID");
                            if (storeRow != null)
                            {
                                var d = storeRow as IDictionary<string, object>;
                                string sName = GetValue(d, "NAME")?.ToString()?.Trim();
                                string sAddr = GetValue(d, "DIACHI")?.ToString()?.Trim();
                                string sPhone = GetValue(d, "DIENTHOAI")?.ToString()?.Trim();
                                if (string.IsNullOrWhiteSpace(cName) && !string.IsNullOrWhiteSpace(sName)) cName = sName;
                                if (string.IsNullOrWhiteSpace(cAddr) && !string.IsNullOrWhiteSpace(sAddr)) cAddr = sAddr;
                                if (string.IsNullOrWhiteSpace(cPhone) && !string.IsNullOrWhiteSpace(sPhone)) cPhone = sPhone;
                            }
                        }
                    }
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(cName)) cName = "TRỤ SỞ CHÍNH";
            if (string.IsNullOrWhiteSpace(cAddr)) cAddr = "Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội";
            if (string.IsNullOrWhiteSpace(cPhone)) cPhone = "0909090880";

            var logo = await LoadCompanyLogoAsync();

            return new CompanyInfoModel
            {
                Name = cName,
                Address = cAddr,
                Phone = CompanyInfoModel.CleanPhone(cPhone),
                Email = (cEmail == "0" || cEmail.Equals("null", StringComparison.OrdinalIgnoreCase)) ? "" : cEmail,
                Fax = (cFax == "0" || cFax.Equals("null", StringComparison.OrdinalIgnoreCase)) ? "" : cFax,
                TaxCode = (cTax == "0" || cTax.Equals("null", StringComparison.OrdinalIgnoreCase)) ? "" : cTax,
                LogoBytes = logo
            };
        }

        public static async Task<byte[]> LoadCompanyLogoAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Tìm logo từ SCONFIG (SIMAGEID hoặc TEXTVALUE)
                    string logoId = null;
                    try
                    {
                        var rows = (await conn.QueryAsync("SELECT * FROM SCONFIG WHERE UPPER(TRIM(NAME)) = 'LOGO'")).ToList();
                        if (rows.Count > 0)
                        {
                            var r = rows[0] as IDictionary<string, object>;
                            logoId = GetValue(r, "SIMAGEID")?.ToString() ?? GetValue(r, "TEXTVALUE")?.ToString() ?? GetValue(r, "VALUE")?.ToString();
                        }
                    }
                    catch { }

                    if (!string.IsNullOrWhiteSpace(logoId))
                    {
                        try
                        {
                            var imgBytes = await conn.ExecuteScalarAsync<byte[]>("SELECT FIRST 1 IMAGE FROM SIMAGE WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = logoId });
                            if (imgBytes != null && imgBytes.Length > 0) return imgBytes;
                        }
                        catch { }
                    }

                    // Fallback logo có tên CompanyLogo
                    try
                    {
                        var compLogo = await conn.ExecuteScalarAsync<byte[]>("SELECT FIRST 1 IMAGE FROM SIMAGE WHERE UPPER(TRIM(NAME)) = 'COMPANYLOGO' AND IMAGE IS NOT NULL");
                        if (compLogo != null && compLogo.Length > 0) return compLogo;
                    }
                    catch { }

                    // Fallback logo đầu tiên trong SIMAGE
                    try
                    {
                        var defaultImg = await conn.ExecuteScalarAsync<byte[]>("SELECT FIRST 1 IMAGE FROM SIMAGE WHERE IMAGE IS NOT NULL");
                        return defaultImg;
                    }
                    catch { }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCompanyLogoAsync: " + ex.Message);
                return null;
            }
        }

        public static async Task<(bool Success, string ErrorMessage)> SaveAllConfigsAsync(Dictionary<string, string> configs, Dictionary<string, string> tableFormats, byte[] newLogoBytes = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        object userId = await GetCurrentUserIdAsync(conn, trans);

                        if (newLogoBytes != null && newLogoBytes.Length > 0)
                        {
                            object nextLogoId = await GetNextSImageIdAsync(conn, trans);
                            string logoIdStr = nextLogoId.ToString();
                            try
                            {
                                await conn.ExecuteAsync(@"
                                    INSERT INTO SIMAGE (ID, NAME, NOTE, IMAGE, STATUS, SORTORDER, USERCREATEDID, USERMODIFIEDID, TIMECREATED, TIMEMODIFIED) 
                                    VALUES (@Id, 'CompanyLogo', 'Logo Công Ty', @Image, 30, 1, @UserId, @UserId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                                    new { Id = nextLogoId, Image = newLogoBytes, UserId = userId }, transaction: trans);
                            }
                            catch
                            {
                                try
                                {
                                    await conn.ExecuteAsync(@"
                                        INSERT INTO SIMAGE (ID, NAME, IMAGE, STATUS, USERCREATEDID, TIMECREATED) 
                                        VALUES (@Id, 'CompanyLogo', @Image, 30, @UserId, CURRENT_TIMESTAMP)",
                                        new { Id = nextLogoId, Image = newLogoBytes, UserId = userId }, transaction: trans);
                                }
                                catch
                                {
                                    await conn.ExecuteAsync(@"
                                        INSERT INTO SIMAGE (ID, NAME, IMAGE, STATUS, USERCREATEDID) 
                                        VALUES (@Id, 'CompanyLogo', @Image, 30, @UserId)",
                                        new { Id = nextLogoId, Image = newLogoBytes, UserId = userId }, transaction: trans);
                                }
                            }
                            configs["Logo"] = logoIdStr;
                        }

                        // Cập nhật SCONFIG
                        foreach (var kv in configs)
                        {
                            string val = kv.Value ?? "";

                            int affected = 0;
                            try
                            {
                                affected = await conn.ExecuteAsync(@"
                                    UPDATE SCONFIG 
                                    SET TEXTVALUE = @Value,
                                        STATUS = 30,
                                        USERMODIFIEDID = @UserId,
                                        TIMEMODIFIED = CURRENT_TIMESTAMP
                                    WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                    new { Name = kv.Key, Value = val, UserId = userId }, transaction: trans);
                            }
                            catch
                            {
                                try
                                {
                                    affected = await conn.ExecuteAsync(@"
                                        UPDATE SCONFIG 
                                        SET TEXTVALUE = @Value,
                                            STATUS = 30,
                                            USERMODIFIEDID = @UserId
                                        WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                        new { Name = kv.Key, Value = val, UserId = userId }, transaction: trans);
                                }
                                catch
                                {
                                    try
                                    {
                                        affected = await conn.ExecuteAsync(@"
                                            UPDATE SCONFIG 
                                            SET VALUE = @Value,
                                                STATUS = 30
                                            WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                            new { Name = kv.Key, Value = val }, transaction: trans);
                                    }
                                    catch
                                    {
                                        affected = 0;
                                    }
                                }
                            }

                            if (affected == 0)
                            {
                                object nextId = await GetNextSConfigIdAsync(conn, trans);
                                try
                                {
                                    await conn.ExecuteAsync(@"
                                        INSERT INTO SCONFIG (ID, NAME, TEXTVALUE, STATUS, USERCREATEDID, USERMODIFIEDID, TIMECREATED, TIMEMODIFIED)
                                        VALUES (@Id, @Name, @Value, 30, @UserId, @UserId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                                        new { Id = nextId, Name = kv.Key, Value = val, UserId = userId }, transaction: trans);
                                }
                                catch
                                {
                                    try
                                    {
                                        await conn.ExecuteAsync(@"
                                            INSERT INTO SCONFIG (ID, NAME, TEXTVALUE, STATUS, USERCREATEDID, TIMECREATED)
                                            VALUES (@Id, @Name, @Value, 30, @UserId, CURRENT_TIMESTAMP)",
                                            new { Id = nextId, Name = kv.Key, Value = val, UserId = userId }, transaction: trans);
                                    }
                                    catch
                                    {
                                        await conn.ExecuteAsync(@"
                                            INSERT INTO SCONFIG (ID, NAME, TEXTVALUE, STATUS, USERCREATEDID)
                                            VALUES (@Id, @Name, @Value, 30, @UserId)",
                                            new { Id = nextId, Name = kv.Key, Value = val, UserId = userId }, transaction: trans);
                                    }
                                }
                            }

                            _configCache[kv.Key] = val;
                        }

                        // Cập nhật STABLEDESC và SCONFIG (định dạng số phiếu)
                        if (tableFormats != null)
                        {
                            foreach (var tf in tableFormats)
                            {
                                if (string.IsNullOrWhiteSpace(tf.Key)) continue;
                                string formatVal = tf.Value?.Trim() ?? "";
                                string sconfigKey = "FORMAT_" + tf.Key.Trim().ToUpper();

                                // 1. Lưu vào SCONFIG
                                try
                                {
                                    int aff = await conn.ExecuteAsync(@"
                                        UPDATE SCONFIG 
                                        SET TEXTVALUE = @Value, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = @UserId 
                                        WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                        new { Name = sconfigKey, Value = formatVal, UserId = userId }, transaction: trans);

                                    if (aff == 0)
                                    {
                                        object nextId = await GetNextSConfigIdAsync(conn, trans);
                                        try
                                        {
                                            await conn.ExecuteAsync(@"
                                                INSERT INTO SCONFIG (ID, NAME, TEXTVALUE, STATUS, USERCREATEDID, USERMODIFIEDID, TIMECREATED, TIMEMODIFIED)
                                                VALUES (@Id, @Name, @Value, 30, @UserId, @UserId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                                                new { Id = nextId, Name = sconfigKey, Value = formatVal, UserId = userId }, transaction: trans);
                                        }
                                        catch
                                        {
                                            await conn.ExecuteAsync(@"
                                                INSERT INTO SCONFIG (ID, NAME, TEXTVALUE, STATUS, USERCREATEDID)
                                                VALUES (@Id, @Name, @Value, 30, @UserId)",
                                                new { Id = nextId, Name = sconfigKey, Value = formatVal, UserId = userId }, transaction: trans);
                                        }
                                    }
                                }
                                catch (Exception exSc)
                                {
                                    Console.WriteLine($"Error saving SCONFIG format {tf.Key}: {exSc.Message}");
                                }

                                // 2. Cập nhật STABLEDESC
                                try
                                {
                                    int affDesc = await conn.ExecuteAsync(@"
                                        UPDATE STABLEDESC 
                                        SET FORMAT = @Format 
                                        WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                        new { Name = tf.Key, Format = formatVal }, transaction: trans);

                                    if (affDesc == 0)
                                    {
                                        int maxId = 0;
                                        try { maxId = await conn.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(ID), 0) FROM STABLEDESC", transaction: trans); } catch { }
                                        await conn.ExecuteAsync(@"
                                            INSERT INTO STABLEDESC (ID, NAME, FORMAT, STATUS)
                                            VALUES (@Id, @Name, @Format, 1)",
                                            new { Id = maxId + 1, Name = tf.Key.Trim().ToUpper(), Format = formatVal }, transaction: trans);
                                    }
                                }
                                catch (Exception exTf)
                                {
                                    Console.WriteLine($"Error updating STABLEDESC format {tf.Key}: {exTf.Message}");
                                }
                            }
                        }

                        trans.Commit();
                        return (true, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveAllConfigsAsync: " + ex.Message);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Làm tròn số phút sử dụng theo cấu hình hệ thống: Làm tròn mặt hàng theo giờ (phút) và Cách làm tròn
        /// </summary>
        public static double RoundMinutesByConfig(double totalMinutes, int roundStepMinutes, string roundMethod)
        {
            if (roundStepMinutes <= 0 || totalMinutes <= 0) return totalMinutes;

            double steps = totalMinutes / roundStepMinutes;
            double roundedSteps;

            if (string.Equals(roundMethod, "Làm tròn lên", StringComparison.OrdinalIgnoreCase))
            {
                roundedSteps = Math.Ceiling(steps);
            }
            else if (string.Equals(roundMethod, "Làm tròn giữa", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(roundMethod, "Làm tròn chuẩn", StringComparison.OrdinalIgnoreCase))
            {
                roundedSteps = Math.Round(steps, MidpointRounding.AwayFromZero);
            }
            else // "Làm tròn xuống"
            {
                roundedSteps = Math.Floor(steps);
            }

            return roundedSteps * roundStepMinutes;
        }

        public static double RoundMinutesWithSystemConfig(double totalMinutes)
        {
            int step = GetIntConfig("LamTronMatHangDichVuTheoGio", 0);
            if (step <= 0) return totalMinutes;
            string method = GetConfig("CachLamTron", "Làm tròn xuống");
            return RoundMinutesByConfig(totalMinutes, step, method);
        }
    }
}
