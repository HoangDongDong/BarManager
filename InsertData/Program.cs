using System;
using FirebirdSql.Data.FirebirdClient;

class Program {
    static void Main() {
        string connStr = "User=SYSDBA;Password=masterkey;Database=D:\\QuanLyBar\\new.fdb;DataSource=localhost;Port=3050;Charset=UTF8;";
        using (var conn = new FbConnection(connStr)) {
            conn.Open();
            
            // Delete old fake data
            new FbCommand("DELETE FROM TDATHANG WHERE NAME = 'FAKE_DATA'", conn).ExecuteNonQuery();
            
            // Insert 5 fake records for today
            for (int i=0; i<5; i++) {
                string sql = "INSERT INTO TDATHANG (NAME, NGAY) VALUES ('FAKE_DATA', '" + DateTime.Now.ToString("yyyy-MM-dd") + "');";
                new FbCommand(sql, conn).ExecuteNonQuery();
            }
            
            // Insert 3 fake records for tomorrow
            for (int i=0; i<3; i++) {
                string sql = "INSERT INTO TDATHANG (NAME, NGAY) VALUES ('FAKE_DATA', '" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + "');";
                new FbCommand(sql, conn).ExecuteNonQuery();
            }
            
            // Check count
            var cmd = new FbCommand("SELECT COUNT(*) FROM TDATHANG WHERE NAME = 'FAKE_DATA'", conn);
            Console.WriteLine("Inserted fake TDATHANG count: " + cmd.ExecuteScalar());
        }
    }
}
