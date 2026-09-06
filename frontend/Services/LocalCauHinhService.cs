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

        public static async Task<Dictionary<string, string>> LoadAllConfigsAsync()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var rows = (await conn.QueryAsync("SELECT NAME, VALUE FROM SCONFIG")).ToList();
                    foreach (object r in rows)
                    {
                        var rowDict = r as IDictionary<string, object>;
                        string name = GetValue(rowDict, "NAME")?.ToString()?.Trim() ?? "";
                        string val = GetValue(rowDict, "VALUE")?.ToString() ?? "";
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTableFormatsAsync: " + ex.Message);
            }
            return dict;
        }

        public static async Task<byte[]> LoadCompanyLogoAsync()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Tìm logo từ SIMAGE hoặc SCONFIG (Logo ID)
                    var logoId = await conn.ExecuteScalarAsync<string>("SELECT VALUE FROM SCONFIG WHERE NAME = 'Logo'");
                    if (!string.IsNullOrWhiteSpace(logoId))
                    {
                        var imgBytes = await conn.ExecuteScalarAsync<byte[]>("SELECT FIRST 1 IMAGE FROM SIMAGE WHERE ID = @Id", new { Id = logoId });
                        if (imgBytes != null && imgBytes.Length > 0) return imgBytes;
                    }

                    // Fallback logo đầu tiên trong SIMAGE
                    var defaultImg = await conn.ExecuteScalarAsync<byte[]>("SELECT FIRST 1 IMAGE FROM SIMAGE WHERE IMAGE IS NOT NULL");
                    return defaultImg;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCompanyLogoAsync: " + ex.Message);
                return null;
            }
        }

        public static async Task<bool> SaveAllConfigsAsync(Dictionary<string, string> configs, Dictionary<string, string> tableFormats, byte[] newLogoBytes = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        string logoId = null;
                        if (newLogoBytes != null && newLogoBytes.Length > 0)
                        {
                            logoId = Guid.NewGuid().ToString();
                            await conn.ExecuteAsync(@"
                                INSERT INTO SIMAGE (ID, NAME, IMAGE, STATUS, TIMECREATED) 
                                VALUES (@Id, 'CompanyLogo', @Image, 30, CURRENT_TIMESTAMP)",
                                new { Id = logoId, Image = newLogoBytes }, transaction: trans);
                            configs["Logo"] = logoId;
                        }

                        // Cập nhật SCONFIG
                        foreach (var kv in configs)
                        {
                            int affected = await conn.ExecuteAsync(@"
                                UPDATE SCONFIG SET VALUE = @Value WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                new { Name = kv.Key, Value = kv.Value ?? "" }, transaction: trans);

                            if (affected == 0)
                            {
                                // Insert nếu chưa có
                                await conn.ExecuteAsync(@"
                                    INSERT INTO SCONFIG (ID, NAME, VALUE, STATUS, TIMECREATED)
                                    VALUES (@Id, @Name, @Value, 30, CURRENT_TIMESTAMP)",
                                    new { Id = Guid.NewGuid().ToString(), Name = kv.Key, Value = kv.Value ?? "" }, transaction: trans);
                            }

                            _configCache[kv.Key] = kv.Value ?? "";
                        }

                        // Cập nhật STABLEDESC (định dạng số phiếu)
                        if (tableFormats != null)
                        {
                            foreach (var tf in tableFormats)
                            {
                                await conn.ExecuteAsync(@"
                                    UPDATE STABLEDESC SET FORMAT = @Format WHERE UPPER(TRIM(NAME)) = UPPER(TRIM(@Name))",
                                    new { Name = tf.Key, Format = tf.Value ?? "" }, transaction: trans);
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveAllConfigsAsync: " + ex.Message);
                return false;
            }
        }
    }
}
