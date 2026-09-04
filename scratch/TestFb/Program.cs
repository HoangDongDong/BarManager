using System;
using FirebirdSql.Data.FirebirdClient;
using Dapper;

class Program
{
    static void Main()
    {
        var builder = new FbConnectionStringBuilder();
        builder.DataSource = "localhost";
        builder.Database = @"D:\saoluu\HIHI.FDB";
        builder.UserID = "SYSDBA";
        builder.Password = "masterkey";
        builder.ServerType = FbServerType.Default;
        builder.Charset = "UTF8";

        using (var conn = new FbConnection(builder.ToString()))
        {
            conn.Open();

            var rows = conn.Query("SELECT ID, NAME, LALYDOTHU, LOAILYDO, NOTE, STATUS, SORTORDER FROM DLYDOTHUCHI ORDER BY SORTORDER, NAME");
            foreach (var r in rows)
            {
                Console.WriteLine($"ID: {r.ID} | NAME: {r.NAME} | LALYDOTHU: {r.LALYDOTHU} | LOAILYDO: {r.LOAILYDO} | STATUS: {r.STATUS}");
            }
        }
    }
}
