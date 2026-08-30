using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class HoaDonBanHangPrintWindow : Window
    {
        private PosBanViewModel _ban;
        private bool _isTamTinh;

        public HoaDonBanHangPrintWindow(PosBanViewModel ban, bool isTamTinh = false)
        {
            InitializeComponent();
            _ban = ban;
            _isTamTinh = isTamTinh;

            Loaded += HoaDonBanHangPrintWindow_Loaded;
        }

        private async void HoaDonBanHangPrintWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInstalledPrinters();
            await LoadStoreInfoAsync();
            PopulateBillData();
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                var printers = new List<string>();
                var printServer = new LocalPrintServer();
                foreach (var pq in printServer.GetPrintQueues())
                {
                    printers.Add(pq.Name);
                }

                CmbPrinters.ItemsSource = printers;
                if (printers.Count > 0)
                {
                    try
                    {
                        var defaultQueue = LocalPrintServer.GetDefaultPrintQueue();
                        if (defaultQueue != null && printers.Contains(defaultQueue.Name))
                        {
                            CmbPrinters.SelectedItem = defaultQueue.Name;
                        }
                        else
                        {
                            CmbPrinters.SelectedIndex = 0;
                        }
                    }
                    catch
                    {
                        CmbPrinters.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadStoreInfoAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    var configs = (await conn.QueryAsync<(string Name, string TextValue)>("SELECT NAME, TEXTVALUE FROM SCONFIG WHERE NAME IN ('CompanyName', 'CompanyAddress', 'CompanyPhone')")).ToList();
                    
                    var compName = configs.FirstOrDefault(c => c.Name == "CompanyName").TextValue;
                    var compAddr = configs.FirstOrDefault(c => c.Name == "CompanyAddress").TextValue;
                    var compPhone = configs.FirstOrDefault(c => c.Name == "CompanyPhone").TextValue;

                    if (!string.IsNullOrWhiteSpace(compName)) TxtTenQuan.Text = compName.Trim();
                    if (!string.IsNullOrWhiteSpace(compAddr)) TxtDiaChi.Text = "ĐC: " + compAddr.Trim();
                    if (!string.IsNullOrWhiteSpace(compPhone)) TxtDienThoai.Text = "ĐT: " + compPhone.Trim();
                }
            }
            catch { }
        }

        private void PopulateBillData()
        {
            if (_ban == null) return;

            TxtTitle.Text = _isTamTinh ? "PHIẾU TẠM TÍNH" : "HÓA ĐƠN BÁN HÀNG";
            TxtTenBan.Text = _ban.Name;

            DateTime now = DateTime.Now;
            RunNgay.Text = now.ToString("dd/MM/yyyy");
            RunInLuc.Text = now.ToString("HH:mm");
            RunGioRa.Text = now.ToString("HH:mm");

            RunSoPhieu.Text = string.IsNullOrEmpty(_ban.SoPhieu) ? _ban.ActiveOrderId ?? "" : _ban.SoPhieu;
            RunThuNgan.Text = SessionContext.CurrentUser?.TenDangNhap ?? "Administrator";
            RunGioVao.Text = _ban.StartTime.HasValue ? _ban.StartTime.Value.ToString("HH:mm") : now.ToString("HH:mm");

            ListItems.ItemsSource = _ban.OrderItems;

            if (_ban.GiamGia > 0)
            {
                PanelGiamGia.Visibility = Visibility.Visible;
                TxtGiamGia.Text = _ban.GiamGia.ToString("N0");
            }
            else
            {
                PanelGiamGia.Visibility = Visibility.Collapsed;
            }

            TxtTongCong.Text = _ban.TongCong.ToString("N0");
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (CmbPrinters.SelectedItem is string printerName && !string.IsNullOrEmpty(printerName))
                {
                    printDlg.PrintQueue = new PrintQueue(new LocalPrintServer(), printerName);
                }

                printDlg.PrintVisual(BillPaper, "In Hóa Đơn - " + _ban?.Name);
                MessageBox.Show("Đã gửi lệnh in đến máy in thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in hóa đơn: " + ex.Message, "Lỗi in ấn", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
