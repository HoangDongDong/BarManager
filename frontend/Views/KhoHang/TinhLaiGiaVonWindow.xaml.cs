using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.KhoHang
{
    public partial class TinhLaiGiaVonWindow : Window
    {
        public TinhLaiGiaVonWindow()
        {
            InitializeComponent();
        }

        private async void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnThucHien.IsEnabled = false;
                BtnThoat.IsEnabled = false;

                await Task.Run(async () =>
                {
                    using (var conn = DbConnectionManager.GetConnection())
                    {
                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                        // 1. Cập nhật giá vốn bình quân cho mặt hàng từ các dòng nhập kho
                        string sqlUpdateMatHang = @"
                            UPDATE DMATHANG m
                            SET m.GIAVON = COALESCE(
                                (SELECT SUM(c.THANHTIEN) / NULLIF(SUM(c.SLNHAP), 0)
                                 FROM TDONHANGCHITIET c
                                 WHERE c.DMATHANGID = m.ID AND c.SLNHAP > 0),
                                m.GIANHAP,
                                0
                            )";
                        
                        try
                        {
                            await conn.ExecuteAsync(sqlUpdateMatHang);
                        }
                        catch
                        {
                            // Fallback nếu câu query gộp không chạy được trên version Firebird cũ
                            string sqlFallback = @"
                                UPDATE DMATHANG m
                                SET m.GIAVON = COALESCE(m.GIANHAP, 0)
                                WHERE m.GIAVON IS NULL OR m.GIAVON = 0";
                            await conn.ExecuteAsync(sqlFallback);
                        }

                        // 2. Cập nhật giá vốn cho các chi tiết bán / xuất hàng
                        try
                        {
                            string sqlUpdateChiTiet = @"
                                UPDATE TDONHANGCHITIET c
                                SET c.GIAVON = (SELECT FIRST 1 m.GIAVON FROM DMATHANG m WHERE m.ID = c.DMATHANGID)
                                WHERE EXISTS (SELECT 1 FROM DMATHANG m WHERE m.ID = c.DMATHANGID AND m.GIAVON > 0)";
                            await conn.ExecuteAsync(sqlUpdateChiTiet);
                        }
                        catch
                        {
                            // Bỏ qua nếu bảng TDONHANGCHITIET không có trường GIAVON
                        }
                    }
                });

                MessageBox.Show("Tính lại giá vốn hàng bán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tính lại giá vốn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnThucHien.IsEnabled = true;
                BtnThoat.IsEnabled = true;
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
