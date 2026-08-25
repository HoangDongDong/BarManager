using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalLuuVetService
    {
        public async Task<List<LuuVetViewModel>> GetLuuVetListAsync(DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = DbConnectionManager.GetConnection())
            {
                await conn.OpenAsync();
                
                string sql = @"
                    SELECT 
                        NGAY as Ngay, 
                        GIO as Gio, 
                        SODONHANG as Sodonhang, 
                        NOTE as Note, 
                        TAIKHOAN as Taikhoan, 
                        THIETBI as Thietbi, 
                        BAN as Ban, 
                        CHUCNANG as Chucnang
                    FROM TLUUVET
                    WHERE CAST(NGAY AS DATE) >= @TuNgay 
                      AND CAST(NGAY AS DATE) <= @DenNgay
                    ORDER BY NGAY DESC, GIO DESC
                ";

                var parameters = new 
                { 
                    TuNgay = tuNgay.Date, 
                    DenNgay = denNgay.Date 
                };

                var list = (await conn.QueryAsync<LuuVetViewModel>(sql, parameters)).ToList();
                return list;
            }
        }
    }
}
