using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;
using System.Windows;
using System.Collections.ObjectModel;

namespace QuanLyBar.Client.Services
{
    public class LocalSuDungDichVuService
    {
        public async Task<List<PosKhuVucViewModel>> GetKhuVucBanListAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // Lấy danh sách khu vực
                    var khuvucList = (await conn.QueryAsync<PosKhuVucViewModel>(
                        "SELECT ID, NAME FROM DKHUVUC ORDER BY NAME"
                    )).ToList();

                    // Lấy danh sách bàn
                    // Giả lập trạng thái IsOccupied ngẫu nhiên để thấy được màu đỏ và bộ đếm thời gian
                    var banList = (await conn.QueryAsync<dynamic>(
                        "SELECT ID, NAME, DKHUVUCID FROM DBAN ORDER BY NAME"
                    )).ToList();

                    Random rnd = new Random();

                    foreach (var kv in khuvucList)
                    {
                        var bans = banList.Where(b => b.DKHUVUCID == kv.Id).ToList();
                        foreach (var b in bans)
                        {
                            bool isOcc = rnd.Next(100) < 30; // 30% có khách
                            kv.BanList.Add(new PosBanViewModel
                            {
                                Id = b.ID,
                                Name = b.NAME,
                                IsOccupied = isOcc,
                                TimerText = isOcc ? $"{rnd.Next(1, 5)}h {rnd.Next(1, 59)}'" : ""
                            });
                        }
                    }

                    return khuvucList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy khu vực bàn: " + ex.Message);
                return new List<PosKhuVucViewModel>();
            }
        }

        public async Task<ObservableCollection<PosNhomMatHangViewModel>> GetNhomMatHangTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    var flatList = (await conn.QueryAsync<PosNhomMatHangViewModel>(
                        "SELECT ID, NAME, PARENTID FROM DNHOMMATHANG ORDER BY NAME"
                    )).ToList();

                    // Fallback nếu bảng DNHOMMATHANG không được sử dụng
                    if (flatList.Count == 0)
                    {
                        try 
                        {
                            flatList = (await conn.QueryAsync<PosNhomMatHangViewModel>(
                                "SELECT ID, NAME, PARENTID FROM DLOAIMATHANG ORDER BY NAME"
                            )).ToList();
                        } 
                        catch { }
                    }
                    
                    if (flatList.Count == 0)
                    {
                        try 
                        {
                            flatList = (await conn.QueryAsync<PosNhomMatHangViewModel>(
                                "SELECT ID, NAME, PARENTID FROM DLOAIDO ORDER BY NAME"
                            )).ToList();
                        } 
                        catch { }
                    }

                    var rootItems = new ObservableCollection<PosNhomMatHangViewModel>();
                    var rootAll = new PosNhomMatHangViewModel { Id = string.Empty, Name = "Tất cả", ParentId = null };
                    rootItems.Add(rootAll);

                    var lookup = flatList.ToDictionary(g => g.Id);

                    foreach (var item in flatList)
                    {
                        if (!string.IsNullOrEmpty(item.ParentId) && lookup.ContainsKey(item.ParentId))
                        {
                            lookup[item.ParentId].Children.Add(item);
                        }
                        else
                        {
                            rootAll.Children.Add(item);
                        }
                    }

                    return rootItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy nhóm món: " + ex.Message);
                return new ObservableCollection<PosNhomMatHangViewModel>();
            }
        }

        public async Task<List<PosMatHangViewModel>> GetMatHangListAsync(string nhomId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT 
                            m.ID, m.CODE as Code, m.NAME as Name, m.GIABAN as GiaBan,
                            dvt.NAME as DonViTinh
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH dvt ON m.DDONVITINHID = dvt.ID
                        WHERE 1=1
                    ";

                    if (!string.IsNullOrEmpty(nhomId))
                    {
                        sql += " AND (m.DNHOMMATHANGID = @NhomId OR m.DLOAIMATHANGID = @NhomId)";
                    }

                    sql += " ORDER BY m.NAME";

                    var result = await conn.QueryAsync<PosMatHangViewModel>(sql, new { NhomId = nhomId });
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy món ăn: " + ex.Message);
                return new List<PosMatHangViewModel>();
            }
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}
