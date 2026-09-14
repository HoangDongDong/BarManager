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

            Console.WriteLine("=== ALL STEMPLATE ===");
            var stemp = conn.Query("SELECT ID, NAME FROM STEMPLATE");
            foreach (var st in stemp) Console.WriteLine($"STEMPLATE ID={st.ID}, NAME='{st.NAME}'");

            Console.WriteLine("\n=== ALL SREPORTTEMPLATE ===");
            var srt = conn.Query("SELECT ID, SREPORTID, STEMPLATEID, NAME FROM SREPORTTEMPLATE");
            foreach (var r in srt) Console.WriteLine($"SRT ID={r.ID}, SREPORTID={r.SREPORTID}, STEMPLATEID={r.STEMPLATEID}, NAME='{r.NAME}'");


        }
    }

    static string ColorIntToHex(object colorDbValue)
    {
        if (colorDbValue == null || colorDbValue == DBNull.Value) return "NONE";
        if (int.TryParse(colorDbValue.ToString(), out int argb))
        {
            uint uargb = (uint)argb;
            return "#" + uargb.ToString("X8");
        }
        return colorDbValue.ToString();
    }
}
