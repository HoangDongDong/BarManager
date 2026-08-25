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
    public class LocalKhachDatHangService
    {
        public async Task<ObservableCollection<PhuongThucDatViewModel>> GetPhuongThucDatTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    string sql = @"
                        SELECT ID as Id, 
                               NAME as Name, 
                               PARENTID as ParentId, 
                               PARENTDIR as ParentDir, 
                               SORTORDER as SortOrder
                        FROM DPHUONGTHUCDAT
                        ORDER BY SORTORDER, NAME";

                    var allItems = (await conn.QueryAsync<PhuongThucDatViewModel>(sql)).ToList();

                    var tree = new ObservableCollection<PhuongThucDatViewModel>();
                    var lookup = new Dictionary<string, PhuongThucDatViewModel>();

                    var rootNode = new PhuongThucDatViewModel { Id = null, Name = "Tất cả" };
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
                MessageBox.Show("Lỗi tải cây Phương thức đặt: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ObservableCollection<PhuongThucDatViewModel>();
            }
        }

        public async Task<List<DatHangViewModel>> GetDatHangListAsync(string phuongThucId, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT d.ID as Id, 
                               d.NGAY as Ngay,
                               d.NAME as SoPhieu,
                               d.TENKHACH as TenKhach,
                               CAST(d.DIACHI AS VARCHAR(255)) as DiaChi,
                               d.DIENTHOAI as DienThoai,
                               d.EMAIL as Email,
                               d.TONGCONG as TongCong,
                               p.NAME as PhuongThucDatName,
                               m.NAME as MucDichDatName,
                               d.TUGIO as TuGio,
                               d.DENGIO as DenGio,
                               d.TUNGAY as TuNgay,
                               d.DENNGAY as DenNgay
                        FROM TDATHANG d
                        LEFT JOIN DPHUONGTHUCDAT p ON d.DPHUONGTHUCDATID = p.ID
                        LEFT JOIN DMUCDICHDAT m ON d.DMUCDICHDATID = m.ID
                        WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(phuongThucId))
                    {
                        sql += " AND (d.DPHUONGTHUCDATID = @PhuongThucId OR p.PARENTID = @PhuongThucId OR p.PARENTDIR LIKE '%' || @PhuongThucId || ',%')";
                    }
                    
                    if (tuNgay.HasValue)
                    {
                        sql += " AND d.NGAY >= @TuNgay";
                    }
                    
                    if (denNgay.HasValue)
                    {
                        sql += " AND d.NGAY <= @DenNgay";
                    }

                    sql += " ORDER BY d.NGAY DESC, d.NAME DESC";

                    var result = await conn.QueryAsync<DatHangViewModel>(sql, new { PhuongThucId = phuongThucId, TuNgay = tuNgay, DenNgay = denNgay });
                    var list = result.ToList();
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Stt = i + 1;
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách khách đặt hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DatHangViewModel>();
            }
        }

        public async Task<List<DatHangChiTietViewModel>> GetDatHangChiTietListAsync(string datHangId)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            m.NAME as MatHangName,
                            m.CODE as MaHang,
                            dvt.NAME as DonViTinhName,
                            c.SOLUONG as SoLuong,
                            c.DONGIA as DonGia,
                            c.TILEGIAMGIA as GiamGiaPhanTram,
                            c.THANHTIEN as ThanhTien,
                            c.NOTE as GhiChu
                        FROM TDATHANGCHITIET c
                        LEFT JOIN DMATHANG m ON c.DMATHANGID = m.ID
                        LEFT JOIN DDONVITINH dvt ON c.DDONVITINHID = dvt.ID
                        WHERE c.TDATHANGID = @DatHangId
                    ";

                    var result = await conn.QueryAsync<DatHangChiTietViewModel>(sql, new { DatHangId = datHangId });
                    var list = result.ToList();
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Stt = i + 1;
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết đơn hàng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<DatHangChiTietViewModel>();
            }
        }
    }
}
