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

            Console.WriteLine("=== DNHOMMATHANG ===");
            var nhoms = conn.Query("SELECT ID, NAME, MAUSAC FROM DNHOMMATHANG WHERE STATUS <> 0 OR STATUS IS NULL");
            foreach (var r in nhoms)
            {
                string hex = ColorIntToHex(r.MAUSAC);
                Console.WriteLine($"ID: {r.ID} | NAME: {r.NAME} | MAUSAC_RAW: {r.MAUSAC} | HEX: {hex}");
            }

            Console.WriteLine("\n=== SCAUHINH TOUCH CONFIGS ===");
            var configs = conn.Query("SELECT ID, VAL FROM SCAUHINH WHERE ID LIKE '%TOUCH%'");
            foreach (var r in configs)
            {
                Console.WriteLine($"ID: {r.ID} | VAL: '{r.VAL}'");
            }
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
