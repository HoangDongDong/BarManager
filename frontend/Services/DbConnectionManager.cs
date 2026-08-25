using System;
using System.IO;
using Newtonsoft.Json;
using QuanLyBar.Client.Models;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows;

namespace QuanLyBar.Client.Services
{
    public static class DbConnectionManager
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.json");
        public static DatabaseInfo CurrentConfig { get; private set; }

        static DbConnectionManager()
        {
            LoadConfig();
        }

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    CurrentConfig = JsonConvert.DeserializeObject<DatabaseInfo>(json);
                    Application.Current.Properties["SelectedDbName"] = CurrentConfig?.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file cấu hình DB: " + ex.Message);
            }
        }

        public static void SaveConfig(DatabaseInfo config)
        {
            try
            {
                CurrentConfig = config;
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
                Application.Current.Properties["SelectedDbName"] = config.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu file cấu hình DB: " + ex.Message);
            }
        }

        public static System.Data.Common.DbConnection GetConnection(DatabaseInfo config = null)
        {
            var cfg = config ?? CurrentConfig;
            
            if (cfg == null)
            {
                throw new InvalidOperationException("Chưa cấu hình cơ sở dữ liệu. Vui lòng cấu hình trước khi đăng nhập.");
            }

            if (cfg.ConnectionType == 2 || cfg.ConnectionType == 0) // File (Firebird) hoặc Firebird Server
            {
                var builder = new FbConnectionStringBuilder();
                builder.DataSource = string.IsNullOrEmpty(cfg.Server) ? "localhost" : cfg.Server;
                
                builder.Database = cfg.Path;
                builder.UserID = string.IsNullOrEmpty(cfg.Username) ? "SYSDBA" : cfg.Username;
                builder.Password = string.IsNullOrEmpty(cfg.Password) ? "masterkey" : cfg.Password;
                builder.ServerType = FbServerType.Default;
                builder.Charset = "UTF8";
                
                return new FbConnection(builder.ToString());
            }
            else if (cfg.ConnectionType == 1) // SQL Server
            {
                var builder = new SqlConnectionStringBuilder();
                builder.DataSource = cfg.Server;
                builder.InitialCatalog = cfg.Path; // Tên DB trong SQL Server
                builder.UserID = cfg.Username;
                builder.Password = cfg.Password;
                builder.TrustServerCertificate = true;

                return new SqlConnection(builder.ToString());
            }

            throw new NotSupportedException("Loại kết nối CSDL không được hỗ trợ.");
        }
        
        public static bool TestConnection(DatabaseInfo config, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var conn = GetConnection(config))
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
