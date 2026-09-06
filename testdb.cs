using System;
using FirebirdSql.Data.FirebirdClient;

class Program {
    static void Main() {
        string connStr = "User=SYSDBA;Password=masterkey;Database=D:\\saoluu\\HIHI.FDB;DataSource=localhost;Port=3050;Charset=UTF8;";
        try {
            using (var conn = new FbConnection(connStr)) {
                conn.Open();
                Console.WriteLine("--- SGROUPUSER ---");
                using (var cmd = new FbCommand("SELECT ID, NAME, STATUS FROM SGROUPUSER", conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) Console.WriteLine($"Group: ID={r[0]}, NAME={r[1]}, STATUS={r[2]}");
                }
                
                Console.WriteLine("--- SUSER ---");
                using (var cmd = new FbCommand("SELECT ID, USERNAME, NAME, SGROUPUSERID, ISADMIN, STATUS FROM SUSER", conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) Console.WriteLine($"User: ID={r[0]}, USERNAME={r[1]}, NAME={r[2]}, SGROUPUSERID={r[3]}, ISADMIN={r[4]}, STATUS={r[5]}");
                }

                Console.WriteLine("--- SGROUPROLE samples ---");
                using (var cmd = new FbCommand("SELECT FIRST 20 SGROUPUSERID, SFUNCTIONID, MODE FROM SGROUPROLE", conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) Console.WriteLine($"Role: SGROUPUSERID={r[0]}, SFUNCTIONID={r[1]}, MODE={r[2]}");
                }

                Console.WriteLine("--- SREPORTROLE samples ---");
                using (var cmd = new FbCommand("SELECT FIRST 20 SGROUPUSERID, SREPORTID, MODE FROM SREPORTROLE", conn))
                using (var r = cmd.ExecuteReader()) {
                    while (r.Read()) Console.WriteLine($"ReportRole: SGROUPUSERID={r[0]}, SREPORTID={r[1]}, MODE={r[2]}");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("DB Error: " + ex);
        }
    }
}
