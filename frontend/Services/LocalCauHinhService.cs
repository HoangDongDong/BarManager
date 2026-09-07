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
    }
}
