using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Dapper;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Services
{
    public class LocalTheoDoiDatPhongService
    {
        private class BanDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DkhuvucId { get; set; }
            public string KhuVucName { get; set; }
        }

        private class BookingDto
        {
            public string Id { get; set; }
            public string SoPhieu { get; set; }
            public string TenKhach { get; set; }
            public string DienThoai { get; set; }
            public DateTime? Ngay { get; set; }
            public DateTime? TuNgay { get; set; }
            public DateTime? DenNgay { get; set; }
            public string DbanId { get; set; }
            public int? Status { get; set; }
            public string Note { get; set; }
        }

        public async Task<List<LookupItem>> GetKhuVucLookupAsync()
        {
            var list = new List<LookupItem>();
            list.Add(new LookupItem { Id = "", Name = "Tất cả" });

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID as Id, NAME as Name FROM DKHUVUC WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME";
                    var items = await conn.QueryAsync<LookupItem>(sql);
                    list.AddRange(items);
                }
            }
            catch { }

            return list;
        }

        public async Task<List<TheoDoiDatPhongRowViewModel>> GetTheoDoiDataAsync(DateTime startDate, int dayCount, string khuVucId = null)
        {
            var result = new List<TheoDoiDatPhongRowViewModel>();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // 1. Tải danh sách phòng / bàn
                    string sqlBans = @"
                        SELECT b.ID as Id, b.NAME as Name, b.DKHUVUCID as DkhuvucId, 
                               COALESCE(k.NAME, 'Chưa thiết lập') as KhuVucName
                        FROM DBAN b
                        LEFT JOIN DKHUVUC k ON b.DKHUVUCID = k.ID
                        WHERE (b.STATUS <> 0 OR b.STATUS IS NULL) ";

                    if (!string.IsNullOrEmpty(khuVucId))
                    {
                        sqlBans += " AND (b.DKHUVUCID = @KhuVucId OR k.PARENTID = @KhuVucId) ";
                    }

                    sqlBans += " ORDER BY k.SORTORDER, k.NAME, b.NAME";

                    var banList = (await conn.QueryAsync<BanDto>(sqlBans, new { KhuVucId = khuVucId })).ToList();
                    int totalRooms = banList.Count;

                    // 2. Tải danh sách đơn đặt hàng
                    DateTime endDate = startDate.AddDays(dayCount).Date;
                    string sqlDatHang = @"
                        SELECT ID as Id, NAME as SoPhieu, TENKHACH as TenKhach, DIENTHOAI as DienThoai, 
                               NGAY as Ngay, TUNGAY as TuNgay, DENNGAY as DenNgay, DBANID as DbanId, 
                               STATUS as Status, NOTE as Note
                        FROM TDATHANG
                        WHERE (STATUS <> 0 OR STATUS IS NULL)";

                    var allBookings = (await conn.QueryAsync<BookingDto>(sqlDatHang)).ToList();

                    // Lọc booking trong khoảng ngày ở bộ nhớ C# để tránh lỗi cú pháp DATE giữa các DBMS
                    var bookings = allBookings.Where(x =>
                        (x.Ngay.HasValue && x.Ngay.Value.Date >= startDate.Date && x.Ngay.Value.Date < endDate) ||
                        (x.TuNgay.HasValue && x.DenNgay.HasValue && x.TuNgay.Value.Date < endDate && x.DenNgay.Value.Date >= startDate.Date)
                    ).ToList();

                    // Helper kiểm tra booking có rơi vào ngày curDate không
                    bool IsDateMatch(BookingDto b, DateTime curDate)
                    {
                        if (b.TuNgay.HasValue && b.DenNgay.HasValue)
                        {
                            return b.TuNgay.Value.Date <= curDate && b.DenNgay.Value.Date >= curDate;
                        }
                        if (b.Ngay.HasValue)
                        {
                            return b.Ngay.Value.Date == curDate;
                        }
                        return false;
                    }

                    // 3. Xây dựng các dòng phòng cụ thể
                    var redBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cc0000"));
                    var greenBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d4edda"));
                    var greenFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#155724"));
                    var summaryBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fffae6"));

                    int[] soldPerDay = new int[dayCount];

                    foreach (var ban in banList)
                    {
                        string banId = ban.Id ?? "";
                        string banName = ban.Name ?? "";
                        string kvName = ban.KhuVucName ?? "";

                        var row = new TheoDoiDatPhongRowViewModel
                        {
                            KhuVucName = kvName,
                            PhongName = banName,
                            BanId = banId,
                            IsSummary = false,
                            RowForeground = Brushes.Black,
                            RowFontWeight = FontWeights.Normal
                        };

                        int roomBookedCount = 0;

                        for (int i = 0; i < dayCount; i++)
                        {
                            DateTime curDate = startDate.AddDays(i).Date;
                            var cell = new TheoDoiDatPhongCell
                            {
                                Date = curDate,
                                Text = "-",
                                IsBooked = false
                            };

                            // Tìm booking cho phòng này vào ngày curDate
                            var b = bookings.FirstOrDefault(x => 
                                !string.IsNullOrEmpty(x.DbanId) && x.DbanId == banId && IsDateMatch(x, curDate)
                            );

                            if (b != null)
                            {
                                cell.IsBooked = true;
                                cell.BookingId = b.Id;
                                cell.SoPhieu = b.SoPhieu;
                                cell.CustomerName = b.TenKhach;
                                cell.PhoneNumber = b.DienThoai;
                                cell.Note = b.Note;
                                cell.Text = !string.IsNullOrWhiteSpace(cell.CustomerName) ? cell.CustomerName : (cell.SoPhieu ?? "Đã đặt");
                                cell.CellBackground = greenBg;
                                cell.CellForeground = greenFg;
                                cell.FontWeight = FontWeights.SemiBold;

                                roomBookedCount++;
                                soldPerDay[i]++;
                            }

                            row.Cells.Add(cell);
                        }

                        row.Tong = roomBookedCount > 0 ? roomBookedCount.ToString() : "-";
                        result.Add(row);
                    }

                    // Nếu có các booking chưa gán bàn cụ thể, cộng thêm vào soldPerDay
                    for (int i = 0; i < dayCount; i++)
                    {
                        DateTime curDate = startDate.AddDays(i).Date;
                        int unassignedBookings = bookings.Count(x => 
                            string.IsNullOrEmpty(x.DbanId) && IsDateMatch(x, curDate)
                        );
                        soldPerDay[i] += unassignedBookings;
                    }

                    // 4. Xây dựng 4 dòng tổng kết (Đã bán, Còn lại, Tổng cộng, Công suất)
                    // Dòng 1: Đã bán
                    int totalSold = soldPerDay.Sum();
                    var rowDaBan = new TheoDoiDatPhongRowViewModel
                    {
                        PhongName = "Đã bán",
                        IsSummary = true,
                        RowForeground = redBrush,
                        RowFontWeight = FontWeights.Bold,
                        RowBackground = summaryBg
                    };
                    for (int i = 0; i < dayCount; i++)
                    {
                        rowDaBan.Cells.Add(new TheoDoiDatPhongCell
                        {
                            Date = startDate.AddDays(i),
                            Text = FormatNumber(soldPerDay[i]),
                            CellForeground = redBrush,
                            FontWeight = FontWeights.Bold
                        });
                    }
                    rowDaBan.Tong = FormatNumber(totalSold);
                    result.Insert(0, rowDaBan);

                    // Dòng 2: Còn lại
                    int[] remainingPerDay = new int[dayCount];
                    for (int i = 0; i < dayCount; i++)
                    {
                        remainingPerDay[i] = Math.Max(0, totalRooms - soldPerDay[i]);
                    }
                    int totalRemaining = remainingPerDay.Sum();

                    var rowConLai = new TheoDoiDatPhongRowViewModel
                    {
                        PhongName = "Còn lại",
                        IsSummary = true,
                        RowForeground = redBrush,
                        RowFontWeight = FontWeights.Bold,
                        RowBackground = summaryBg
                    };
                    for (int i = 0; i < dayCount; i++)
                    {
                        rowConLai.Cells.Add(new TheoDoiDatPhongCell
                        {
                            Date = startDate.AddDays(i),
                            Text = FormatNumber(remainingPerDay[i]),
                            CellForeground = redBrush,
                            FontWeight = FontWeights.Bold
                        });
                    }
                    rowConLai.Tong = FormatNumber(totalRemaining);
                    result.Insert(1, rowConLai);

                    // Dòng 3: Tổng cộng
                    var rowTongCong = new TheoDoiDatPhongRowViewModel
                    {
                        PhongName = "Tổng cộng",
                        IsSummary = true,
                        RowForeground = redBrush,
                        RowFontWeight = FontWeights.Bold,
                        RowBackground = summaryBg
                    };
                    for (int i = 0; i < dayCount; i++)
                    {
                        rowTongCong.Cells.Add(new TheoDoiDatPhongCell
                        {
                            Date = startDate.AddDays(i),
                            Text = FormatNumber(totalRooms),
                            CellForeground = redBrush,
                            FontWeight = FontWeights.Bold
                        });
                    }
                    rowTongCong.Tong = FormatNumber(totalRooms * dayCount);
                    result.Insert(2, rowTongCong);

                    // Dòng 4: Công suất
                    var rowCongSuat = new TheoDoiDatPhongRowViewModel
                    {
                        PhongName = "Công suất",
                        IsSummary = true,
                        RowForeground = redBrush,
                        RowFontWeight = FontWeights.Bold,
                        RowBackground = summaryBg
                    };
                    for (int i = 0; i < dayCount; i++)
                    {
                        rowCongSuat.Cells.Add(new TheoDoiDatPhongCell
                        {
                            Date = startDate.AddDays(i),
                            Text = FormatPercent(soldPerDay[i], totalRooms),
                            CellForeground = redBrush,
                            FontWeight = FontWeights.Bold
                        });
                    }
                    rowCongSuat.Tong = FormatPercent(totalSold, totalRooms * dayCount);
                    result.Insert(3, rowCongSuat);

                    return result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính toán dữ liệu theo dõi đặt phòng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return result;
            }
        }

        private string FormatNumber(int value)
        {
            return value == 0 ? "-" : value.ToString();
        }

        private string FormatPercent(int sold, int total)
        {
            if (total == 0) return "-";
            double percent = (double)sold / total * 100;
            if (percent <= 0) return "-";
            return percent.ToString("0.0") + "%";
        }
    }
}
