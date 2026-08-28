using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThuocTinhWindow : Window
    {
        private DBAN _ban;

        public ThuocTinhWindow(DBAN ban)
        {
            InitializeComponent();
            _ban = ban;
            LoadInfo();
        }

        private async void LoadInfo()
        {
            if (_ban == null) return;

            TxtTitle.Text = $"Thông tin Bàn: {_ban.Name}";
            
            // Format ngày tạo / sửa đổi từ Database
            string timeCreated = _ban.Timecreated?.ToString("dd/MM/yyyy hh:mm tt") ?? "";
            string timeModified = _ban.Timemodified?.ToString("dd/MM/yyyy hh:mm tt") ?? "";

            if (string.IsNullOrEmpty(timeCreated) && _ban.Timemodified.HasValue)
            {
                timeCreated = _ban.Timemodified.Value.ToString("dd/MM/yyyy hh:mm tt");
            }

            TxtTimeCreated.Text = !string.IsNullOrEmpty(timeCreated) ? timeCreated : "--/--/---- --:-- --";
            TxtTimeModified.Text = !string.IsNullOrEmpty(timeModified) ? timeModified : "--/--/---- --:-- --";

            var service = new LocalBanKhuVucService();
            string userCreated = await service.GetUserNameAsync(_ban.UsercreatedId);
            string userModified = await service.GetUserNameAsync(_ban.UsermodifiedId);

            TxtUserCreated.Text = userCreated;
            TxtUserModified.Text = userModified;

            await LoadReferenceCountsAsync();
        }

        private async Task LoadReferenceCountsAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();

                    int countDatHang = 0;
                    try
                    {
                        countDatHang = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM TDATHANG WHERE DBANID = @BanId", new { BanId = _ban.Id });
                    }
                    catch { }

                    int countDonHang = 0;
                    try
                    {
                        countDonHang = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM TDONHANG WHERE DBANID = @BanId", new { BanId = _ban.Id });
                    }
                    catch { }

                    int countHoaDon = 0;
                    try
                    {
                        countHoaDon = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM THOADON WHERE DBANID = @BanId", new { BanId = _ban.Id });
                    }
                    catch { }

                    ItemDatHang.Header = $"Đặt hàng({countDatHang})";
                    ItemDonHang.Header = $"Đơn hàng({countDonHang})";
                    ItemHoaDon.Header = $"Hóa đơn({countHoaDon})";
                    ItemSuaChua.Header = $"Sửa chữa(0)";
                }
            }
            catch
            {
                ItemDatHang.Header = "Đặt hàng(0)";
                ItemDonHang.Header = "Đơn hàng(0)";
                ItemHoaDon.Header = "Hóa đơn(0)";
                ItemSuaChua.Header = "Sửa chữa(0)";
            }
        }

        private void BtnDoiTuongThamChieu_Click(object sender, RoutedEventArgs e)
        {
            if (BtnDoiTuongThamChieu.IsChecked == true)
            {
                SecThamChieu.Visibility = Visibility.Visible;
                Height = 490;
            }
            else
            {
                SecThamChieu.Visibility = Visibility.Collapsed;
                Height = 210;
            }
        }

        private async void TvThamChieu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && _ban != null)
            {
                string header = item.Header?.ToString() ?? "";
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    if (header.StartsWith("Đặt hàng"))
                    {
                        var data = await conn.QueryAsync("SELECT ID, NAME, TIMECREATED, STATUS FROM TDATHANG WHERE DBANID = @BanId", new { BanId = _ban.Id });
                        DgChiTietThamChieu.ItemsSource = data;
                    }
                    else if (header.StartsWith("Đơn hàng"))
                    {
                        var data = await conn.QueryAsync("SELECT ID, NAME, TIMECREATED, STATUS FROM TDONHANG WHERE DBANID = @BanId", new { BanId = _ban.Id });
                        DgChiTietThamChieu.ItemsSource = data;
                    }
                    else
                    {
                        DgChiTietThamChieu.ItemsSource = null;
                    }
                }
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
