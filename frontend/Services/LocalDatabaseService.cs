using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public static class LocalDatabaseService
    {
        public static IEnumerable<T> GetAll<T>(string sql, object? parameters = null)
        {
            using var connection = DbConnectionManager.GetConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
            return connection.Query<T>(sql, parameters).ToList();
        }

        public static int Execute(string sql, object? parameters = null)
        {
            using var connection = DbConnectionManager.GetConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
            return connection.Execute(sql, parameters);
        }
    }
}
