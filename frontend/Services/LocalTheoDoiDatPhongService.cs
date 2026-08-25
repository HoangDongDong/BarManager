using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using QuanLyBar.Client.Models;
using System.Windows;
using System.Windows.Media;

namespace QuanLyBar.Client.Services
{
    public class LocalTheoDoiDatPhongService
    {
        public async Task<List<TheoDoiDatPhongViewModel>> GetTheoDoiDatPhongAsync(DateTime startDate)
        {
            var list = new List<TheoDoiDatPhongViewModel>();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    // 1. Tính tổng số bàn (Tổng cộng)
                    string sqlTotalRooms = "SELECT COUNT(*) FROM DBAN WHERE STATUS = 1";
                    int totalRooms = await conn.ExecuteScalarAsync<int>(sqlTotalRooms);

                    // 2. Tính số lượng Đã bán cho 7 ngày
                    int[] soldPerDay = new int[7];
                    int totalSold = 0;

                    for (int i = 0; i < 7; i++)
                    {
                        DateTime currentDate = startDate.AddDays(i);
                        
                        // Đếm số đơn đặt hàng trong ngày
                        // Ở thực tế có thể phức tạp hơn (ví dụ: trạng thái, phòng cụ thể),
                        // nhưng đây là truy vấn mô phỏng đúng bảng TDATHANG
                        string sqlSold = @"
                            SELECT COUNT(*) 
                            FROM TDATHANG 
                            WHERE CAST(NGAY AS DATE) = @CheckDate";
                            
                        int sold = await conn.ExecuteScalarAsync<int>(sqlSold, new { CheckDate = currentDate.Date });
                        
                        soldPerDay[i] = sold;
                        totalSold += sold;
                    }

                    // 3. Xây dựng 4 dòng dữ liệu

                    // Dòng 1: Đã bán
                    var rowDaBan = new TheoDoiDatPhongViewModel
                    {
                        TieuDe = "Đã bán",
                        Ngay1 = FormatNumber(soldPerDay[0]),
                        Ngay2 = FormatNumber(soldPerDay[1]),
                        Ngay3 = FormatNumber(soldPerDay[2]),
                        Ngay4 = FormatNumber(soldPerDay[3]),
                        Ngay5 = FormatNumber(soldPerDay[4]),
                        Ngay6 = FormatNumber(soldPerDay[5]),
                        Ngay7 = FormatNumber(soldPerDay[6]),
                        Tong = FormatNumber(totalSold),
                        MauChu = Brushes.Red,
                        DoDam = FontWeights.Bold
                    };
                    list.Add(rowDaBan);

                    // Dòng 2: Còn lại
                    int[] remainingPerDay = new int[7];
                    int totalRemaining = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        remainingPerDay[i] = totalRooms - soldPerDay[i];
                        totalRemaining += remainingPerDay[i];
                    }
                    var rowConLai = new TheoDoiDatPhongViewModel
                    {
                        TieuDe = "Còn lại",
                        Ngay1 = FormatNumber(remainingPerDay[0]),
                        Ngay2 = FormatNumber(remainingPerDay[1]),
                        Ngay3 = FormatNumber(remainingPerDay[2]),
                        Ngay4 = FormatNumber(remainingPerDay[3]),
                        Ngay5 = FormatNumber(remainingPerDay[4]),
                        Ngay6 = FormatNumber(remainingPerDay[5]),
                        Ngay7 = FormatNumber(remainingPerDay[6]),
                        Tong = FormatNumber(totalRemaining),
                        MauChu = Brushes.Red,
                        DoDam = FontWeights.Bold
                    };
                    list.Add(rowConLai);

                    // Dòng 3: Tổng cộng
                    var rowTongCong = new TheoDoiDatPhongViewModel
                    {
                        TieuDe = "Tổng cộng",
                        Ngay1 = FormatNumber(totalRooms),
                        Ngay2 = FormatNumber(totalRooms),
                        Ngay3 = FormatNumber(totalRooms),
                        Ngay4 = FormatNumber(totalRooms),
                        Ngay5 = FormatNumber(totalRooms),
                        Ngay6 = FormatNumber(totalRooms),
                        Ngay7 = FormatNumber(totalRooms),
                        Tong = FormatNumber(totalRooms * 7),
                        MauChu = Brushes.Red,
                        DoDam = FontWeights.Bold
                    };
                    list.Add(rowTongCong);

                    // Dòng 4: Công suất
                    var rowCongSuat = new TheoDoiDatPhongViewModel
                    {
                        TieuDe = "Công suất",
                        Ngay1 = FormatPercent(soldPerDay[0], totalRooms),
                        Ngay2 = FormatPercent(soldPerDay[1], totalRooms),
                        Ngay3 = FormatPercent(soldPerDay[2], totalRooms),
                        Ngay4 = FormatPercent(soldPerDay[3], totalRooms),
                        Ngay5 = FormatPercent(soldPerDay[4], totalRooms),
                        Ngay6 = FormatPercent(soldPerDay[5], totalRooms),
                        Ngay7 = FormatPercent(soldPerDay[6], totalRooms),
                        Tong = FormatPercent(totalSold, totalRooms * 7),
                        MauChu = Brushes.Red,
                        DoDam = FontWeights.Bold
                    };
                    list.Add(rowCongSuat);

                    return list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính toán dữ liệu đặt phòng: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                return list; // Return empty/partial list
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
            if (percent == 0) return "-";
            return percent.ToString("0.0") + "%";
        }
    }
}
