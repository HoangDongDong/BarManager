using System;
using FirebirdSql.Data.FirebirdClient;
using Dapper;

class Program
{
    static void Main()
    {
        var builder = new FbConnectionStringBuilder();
        builder.DataSource = "localhost";
        builder.Database = @"D:\QuanLyBar\database\new.fdb";
        builder.UserID = "SYSDBA";
        builder.Password = "masterkey";
        builder.ServerType = FbServerType.Default;
        builder.Charset = "UTF8";

        using (var conn = new FbConnection(builder.ToString()))
        {
            conn.Open();
            var c1 = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM DNHOMMATHANG");
            var c2 = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM DLOAIMATHANG");
            var c3 = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM DLOAIDO");
            var c4 = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM DMATHANG");
            
            Console.WriteLine($"DNHOMMATHANG Count: {c1}");
            Console.WriteLine($"DLOAIMATHANG Count: {c2}");
            Console.WriteLine($"DLOAIDO Count: {c3}");
            Console.WriteLine($"DMATHANG Count: {c4}");

            var nhom = conn.Query("SELECT FIRST 5 ID, NAME, PARENTID FROM DNHOMMATHANG");
            foreach (var item in nhom)
            {
                Console.WriteLine($"NHOM: {item.NAME} - Parent: {item.PARENTID}");
            }
        }
    }
}
