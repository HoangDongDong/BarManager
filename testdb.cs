using System;
using FirebirdSql.Data.FirebirdClient;

class Program {
    static void Main() {
        string connStr = "User=SYSDBA;Password=masterkey;Database=D:\\QuanLyBar\\new.fdb;DataSource=localhost;Port=3050;Charset=UTF8;";
        using (var conn = new FbConnection(connStr)) {
            conn.Open();
            var cmd = new FbCommand("SELECT COUNT(*) FROM TDATHANG", conn);
            Console.WriteLine("TDATHANG count: " + cmd.ExecuteScalar());
            
            cmd.CommandText = "SELECT COUNT(*) FROM DBAN";
            Console.WriteLine("DBAN count: " + cmd.ExecuteScalar());
            
            cmd.CommandText = "SELECT MAX(NGAY) FROM TDATHANG";
            var maxDate = cmd.ExecuteScalar();
            Console.WriteLine("Max date in TDATHANG: " + (maxDate != DBNull.Value ? maxDate.ToString() : "NULL"));
        }
    }
}
