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

            string sql = @"
                SELECT 
                    tc.ID,
                    tc.NOTE AS GHICHU,
                    tc.NAME AS SOPHIEU,
                    tc.NGAY,
                    tc.TENDOITUONG,
                    tc.DIACHI,
                    nv.NAME AS NHANVIEN,
                    kh.NAME AS KHACHHANG,
                    tc.LOAIDOITUONG,
                    lydo.NAME AS LYDOTHUCHI,
                    tc.DIENGIAI,
                    tc.CHUNGTUGOC,
                    tc.THU AS SOTIENTHU,
                    tc.CHI AS SOTIENCHI,
                    ncc.NAME AS NHACUNGCAP,
                    tc.CHUYENKHOAN,
                    dh.NAME AS DATHANG,
                    ch.NAME AS CUAHANG,
                    tc.LAPHIEUTHUCONGNO,
                    tc.KHONGTHAYDOICONGNO,
                    tk.NAME AS TAIKHOANNGANHANG,
                    t.NAME AS THETRATRUOC,
                    donhang.NAME AS DONHANG
                FROM TTHUCHI tc
                LEFT JOIN DNHANVIEN nv ON CAST(tc.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                LEFT JOIN DKHACHHANG kh ON CAST(tc.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                LEFT JOIN DLYDOTHUCHI lydo ON CAST(tc.DLYDOTHUCHIID AS VARCHAR(50)) = CAST(lydo.ID AS VARCHAR(50))
                LEFT JOIN DNHACUNGCAP ncc ON CAST(tc.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                LEFT JOIN TDATHANG dh ON CAST(tc.TDATHANGID AS VARCHAR(50)) = CAST(dh.ID AS VARCHAR(50))
                LEFT JOIN DCUAHANG ch ON CAST(tc.DCUAHANGID AS VARCHAR(50)) = CAST(ch.ID AS VARCHAR(50))
                LEFT JOIN DTAIKHOANNGANHANG tk ON CAST(tc.DTAIKHOANNGANHANGID AS VARCHAR(50)) = CAST(tk.ID AS VARCHAR(50))
                LEFT JOIN DTHETRATRUOC t ON CAST(tc.DTHETRATRUOCID AS VARCHAR(50)) = CAST(t.ID AS VARCHAR(50))
                LEFT JOIN TDONHANG donhang ON CAST(tc.TDONHANGID AS VARCHAR(50)) = CAST(donhang.ID AS VARCHAR(50))
                WHERE tc.LAPHIEUTHUCONGNO > 0 OR tc.STATUS > 0";

            var rows = conn.Query(sql);
            Console.WriteLine("Query Phiếu thu công nợ total rows: " + System.Linq.Enumerable.Count(rows));
        }
    }
}
