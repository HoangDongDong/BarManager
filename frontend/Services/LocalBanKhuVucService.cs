using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;
using System.Windows;

namespace QuanLyBar.Client.Services
{
    public class LocalBanKhuVucService
    {
        public async Task<ObservableCollection<KhuVucViewModel>> GetKhuVucTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    // Firebird schema typically uses uppercase column names without underscores
                    string sql = @"
                        SELECT ID as Id, 
                               NAME as Name, 
                               PARENTID as ParentId, 
                               PARENTDIR as ParentDir, 
                               SORTORDER as SortOrder, 
                               STATUS as Status
                        FROM DKHUVUC
                        ORDER BY SORTORDER, NAME";

                    var allItems = (await conn.QueryAsync<KhuVucViewModel>(sql)).ToList();

                    // Xây dựng cây
                    var tree = new ObservableCollection<KhuVucViewModel>();
                    var lookup = new Dictionary<string, KhuVucViewModel>();

                    // Fake root note "Tất cả"
                    var rootNode = new KhuVucViewModel { Id = null, Name = "Tất cả" };
                    tree.Add(rootNode);

                    foreach (var item in allItems)
                    {
                        lookup[item.Id] = item;
                    }

                    foreach (var item in allItems)
                    {
                        if (string.IsNullOrEmpty(item.ParentId))
                        {
                            rootNode.Children.Add(item);
                        }
                        else
                        {
                            if (lookup.TryGetValue(item.ParentId, out var parent))
                            {
                                parent.Children.Add(item);
                            }
                            else
                            {
                                rootNode.Children.Add(item);
                            }
                        }
                    }

                    return tree;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cây khu vực: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<KhuVucViewModel>();
            }
        }

        public async Task<List<BanViewModel>> GetBanListAsync(string khuvucId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT b.ID as Id, 
                               b.NAME as Name, 
                               b.NOTE as Note,
                               k.NAME as KhuVucName,
                               h.NAME as NhomHienThiName,
                               p.NAME as LoaiPhongName
                        FROM DBAN b
                        LEFT JOIN DKHUVUC k ON b.DKHUVUCID = k.ID
                        LEFT JOIN DNHOMHIENTHI h ON b.DNHOMHIENTHIID = h.ID
                        LEFT JOIN DLOAIPHONG p ON b.DLOAIPHONGID = p.ID
                        WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(khuvucId))
                    {
                        sql += " AND (b.DKHUVUCID = @KhuVucId OR k.PARENTID = @KhuVucId OR k.PARENTDIR LIKE '%' || @KhuVucId || ',%')";
                    }

                    sql += " ORDER BY b.NAME";

                    var result = await conn.QueryAsync<BanViewModel>(sql, new { KhuVucId = khuvucId });
                    var list = result.ToList();
                    
                    // Gán số thứ tự cho giao diện hiển thị
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Stt = i + 1;
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách bàn: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BanViewModel>();
            }
        }
    }
}
