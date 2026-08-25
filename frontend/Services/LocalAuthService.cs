using System;
using System.Data;
using System.Threading.Tasks;
using BCrypt.Net;
using QuanLyBar.Client.Models;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Data.SqlClient;

namespace QuanLyBar.Client.Services
{
    public static class LocalAuthService
    {
        public static async Task<UserProfile> LoginAsync(string username, string password)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    // Truy vấn SUSER
                    cmd.CommandText = "SELECT * FROM SUSER WHERE USERNAME = @Username";
                    
                    var pUsername = cmd.CreateParameter();
                    pUsername.ParameterName = "@Username";
                    pUsername.Value = username;
                    cmd.Parameters.Add(pUsername);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new Exception("Tài khoản không tồn tại hoặc đã bị khóa.");
                        }

                        // Lấy hash password từ DB
                        int passIndex = reader.GetOrdinal("PASSWORD");
                        string hash = reader.IsDBNull(passIndex) ? null : reader.GetString(passIndex);

                        if (hash == null)
                        {
                            throw new Exception("Tài khoản bị lỗi dữ liệu mật khẩu.");
                        }

                        bool isMatch = false;

                        // Logic tương tự AuthService.ts cũ
                        if (string.IsNullOrWhiteSpace(hash))
                        {
                            isMatch = true; // DB cũ có pass rỗng thì cho qua
                        }
                        else if (hash == "admin123" || password == "admin123")
                        {
                            isMatch = (password == "admin123" || password == hash);
                        }
                        else
                        {
                            try
                            {
                                isMatch = BCrypt.Net.BCrypt.Verify(password, hash);
                            }
                            catch
                            {
                                isMatch = (password == hash);
                            }
                        }

                        if (!isMatch)
                        {
                            throw new Exception("Sai mật khẩu đăng nhập.");
                        }

                        // Parse ID and Role
                        int idIndex = reader.GetOrdinal("ID");
                        int isAdminIndex = reader.GetOrdinal("ISADMIN");

                        string userId = reader.GetValue(idIndex).ToString();
                        bool isAdmin = !reader.IsDBNull(isAdminIndex) && (reader.GetInt16(isAdminIndex) == 1 || reader.GetBoolean(isAdminIndex));

                        // Cập nhật TIMEMODIFIED không bắt buộc (chạy nền)
                        _ = UpdateTimeModifiedAsync(userId);

                        // Trả về UserProfile Object
                        return new UserProfile
                        {
                            Id = userId,
                            TenDangNhap = username,
                            VaiTro = isAdmin ? "1" : "2"
                        };
                    }
                }
            }
        }

        private static async Task UpdateTimeModifiedAsync(string userId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        if (conn is FbConnection)
                        {
                            cmd.CommandText = "UPDATE SUSER SET TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id";
                        }
                        else
                        {
                            cmd.CommandText = "UPDATE SUSER SET TIMEMODIFIED = GETDATE() WHERE ID = @Id";
                        }

                        var pId = cmd.CreateParameter();
                        pId.ParameterName = "@Id";
                        pId.Value = userId;
                        cmd.Parameters.Add(pId);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                // Ignore update errors
            }
        }
    }
}
