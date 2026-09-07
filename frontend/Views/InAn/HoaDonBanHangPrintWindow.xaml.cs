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

        private int _soLanIn = 1;

        private async System.Threading.Tasks.Task LoadStoreInfoAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();

                if (configs.TryGetValue("SoLanIn", out var sli) && int.TryParse(sli, out int times) && times > 0)
                {
                    _soLanIn = times;
                }

                // Cấu hình độ rộng giấy in theo Mẫu hóa đơn
                if (configs.TryGetValue("MauHoaDon", out var mhd) && !string.IsNullOrWhiteSpace(mhd))
                {
                    if (mhd.Contains("54"))
                    {
                        BillPaper.Width = 230; // ~58mm/54mm
                    }
                    else if (mhd.Contains("A4"))
                    {
                        BillPaper.Width = 794; // A4 standard width (96 DPI)
                    }
                    else if (mhd.Contains("A5"))
                    {
                        BillPaper.Width = 559; // A5 standard width (96 DPI)
                    }
                    else
                    {
                        BillPaper.Width = 300; // 80mm standard
                    }
                }

                string compName = configs.TryGetValue("CompanyName", out var cn) && !string.IsNullOrWhiteSpace(cn) ? cn : "NÀNG HƯƠNG QUÁN";
                string compAddr = configs.TryGetValue("CompanyAddress", out var ca) && !string.IsNullOrWhiteSpace(ca) ? ca : "";
                string compPhone = configs.TryGetValue("CompanyPhone", out var cp) && !string.IsNullOrWhiteSpace(cp) ? cp : "";
                string compEmail = configs.TryGetValue("CompanyEmail", out var ce) && !string.IsNullOrWhiteSpace(ce) && ce != "0" ? ce : "";
                string compFax = configs.TryGetValue("CompanyFax", out var cf) && !string.IsNullOrWhiteSpace(cf) && cf != "0" ? cf : "";
                string loiCamOn = configs.TryGetValue("LoiCamOn", out var lc) && !string.IsNullOrWhiteSpace(lc) ? lc : "Cảm ơn Quý khách. Hẹn gặp lại.!";

                TxtTenQuan.Text = compName.Trim();

                if (!string.IsNullOrWhiteSpace(compAddr))
                {
                    TxtDiaChi.Text = compAddr.StartsWith("ĐC", StringComparison.OrdinalIgnoreCase) ? compAddr.Trim() : "ĐC: " + compAddr.Trim();
                    TxtDiaChi.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtDiaChi.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrWhiteSpace(compPhone))
                {
                    TxtDienThoai.Text = compPhone.StartsWith("ĐT", StringComparison.OrdinalIgnoreCase) || compPhone.StartsWith("Điện thoại", StringComparison.OrdinalIgnoreCase)
                        ? compPhone.Trim()
                        : "ĐT: " + compPhone.Trim();
                    TxtDienThoai.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtDienThoai.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrWhiteSpace(compEmail))
                {
                    TxtEmail.Text = "Email: " + compEmail.Trim();
                    TxtEmail.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtEmail.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrWhiteSpace(compFax))
                {
                    TxtFax.Text = "Fax: " + compFax.Trim();
                    TxtFax.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtFax.Visibility = Visibility.Collapsed;
                }

                TxtLoiCamOn.Text = loiCamOn.Trim();

                // Logo
                try
                {
                    var logoBytes = await LocalCauHinhService.LoadCompanyLogoAsync();
                    if (logoBytes != null && logoBytes.Length > 0)
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        using (var ms = new System.IO.MemoryStream(logoBytes))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                        }
                        ImgLogo.Source = bitmap;
                        BorderLogoGraphic.Visibility = Visibility.Collapsed;
                        ImgLogo.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ImgLogo.Visibility = Visibility.Collapsed;
                        BorderLogoGraphic.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    ImgLogo.Visibility = Visibility.Collapsed;
                    BorderLogoGraphic.Visibility = Visibility.Visible;
                }

                // In mật khẩu Wifi nếu có bật
                bool inWifi = configs.TryGetValue("InMatKhauWifiTrenBill", out var iw) && (iw == "1" || iw.Equals("true", StringComparison.OrdinalIgnoreCase) || iw == "30");
                if (inWifi)
                {
                    string wifiName = configs.TryGetValue("WifiName", out var wn) && !string.IsNullOrWhiteSpace(wn) ? wn : compName.Trim();
                    string wifiPass = configs.TryGetValue("WifiPass", out var wp) && !string.IsNullOrWhiteSpace(wp) ? wp : "";
                    if (!string.IsNullOrWhiteSpace(wifiPass))
                    {
                        TxtWifiInfo.Text = $"Wifi: {wifiName} - Pass: {wifiPass}";
                        TxtWifiInfo.Visibility = Visibility.Visible;
                    }
                }
                // Check if tax or service fee applies from config if not yet computed on PosBanViewModel
                bool coThue = configs.TryGetValue("CoThueSuat", out var cts) && (cts == "1" || cts.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (coThue && _ban != null && _ban.TienThue == 0)
                {
                    if (configs.TryGetValue("MacDinhThueSuat", out var mdts) && decimal.TryParse(mdts.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var thuePt) && thuePt > 0)
                    {
                        _ban.ThueSuatPt = thuePt;
                        decimal sauGiam = Math.Max(0, _ban.TienHang - _ban.GiamGia);
                        _ban.TienThue = Math.Round(sauGiam * (thuePt / 100m));
                        _ban.TongCong = sauGiam + _ban.TienPhiDichVu + _ban.TienThue;
                    }
                }

                bool coPhi = configs.TryGetValue("CoPhiDichVu", out var cpdv) && (cpdv == "1" || cpdv.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (coPhi && _ban != null && _ban.TienPhiDichVu == 0)
                {
                    if (configs.TryGetValue("MacDinhPhiDichVu", out var mdpdv) && decimal.TryParse(mdpdv.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var phiPt) && phiPt > 0)
                    {
                        _ban.PhiDichVuPt = phiPt;
                        decimal sauGiam = Math.Max(0, _ban.TienHang - _ban.GiamGia);
                        _ban.TienPhiDichVu = Math.Round(sauGiam * (phiPt / 100m));
                        _ban.TongCong = sauGiam + _ban.TienPhiDichVu + _ban.TienThue;
                    }
                }

                // Hiển thị điểm của khách hàng trên hóa đơn nếu có cấu hình
                bool hienThiDiem = configs.TryGetValue("HienThiDiemCuaKhachHangTrenHoaDon", out var htd) && (htd == "1" || htd.Equals("true", StringComparison.OrdinalIgnoreCase));
                decimal doanhSo1Diem = 20000;
                if (configs.TryGetValue("DoanhSoTuongUngVoi1Diem", out var ds1d) && decimal.TryParse(ds1d.Replace(",", "").Replace(".", "").Trim(), out var dsVal) && dsVal > 0)
                {
                    doanhSo1Diem = dsVal;
                }

                if (hienThiDiem && _ban != null)
                {
                    try
                    {
                        string cachTinhDiem = configs.TryGetValue("CachTinhDiem", out var ctd) ? ctd : "Điểm được tính trên từng hóa đơn";
                        string khId = _ban.KhachHangId;
                        string khName = _ban.KhachHangName;

                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            await conn.OpenAsync();
                            if (string.IsNullOrEmpty(khId) && !string.IsNullOrEmpty(_ban.ActiveOrderId))
                            {
                                var ord = await conn.QueryFirstOrDefaultAsync("SELECT DKHACHHANGID FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @OrderId", new { OrderId = _ban.ActiveOrderId });
                                khId = ord?.DKHACHHANGID?.ToString();
                            }

                            if (string.IsNullOrEmpty(khId) && !string.IsNullOrEmpty(_ban.SoPhieu))
                            {
                                var ord = await conn.QueryFirstOrDefaultAsync("SELECT DKHACHHANGID FROM TDONHANG WHERE NAME = @SoPhieu", new { SoPhieu = _ban.SoPhieu });
                                khId = ord?.DKHACHHANGID?.ToString();
                            }

                            if (string.IsNullOrEmpty(khId) && !string.IsNullOrWhiteSpace(khName))
                            {
                                var khRow = await conn.QueryFirstOrDefaultAsync("SELECT ID FROM DKHACHHANG WHERE UPPER(NAME) = UPPER(@KhName)", new { KhName = khName.Trim() });
                                khId = khRow?.ID?.ToString();
                            }

                            int diemLanNay = 0;
                            if (cachTinhDiem == "Điểm được tính trên tổng doanh số" && !string.IsNullOrEmpty(khId))
                            {
                                var pastSales = await conn.ExecuteScalarAsync<decimal?>("SELECT SUM(COALESCE(TONGCONG, 0)) FROM TDONHANG WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId AND STATUS = 2 AND CAST(ID AS VARCHAR(50)) <> @CurrentOrderId", new { KhId = khId, CurrentOrderId = _ban.ActiveOrderId ?? "" }) ?? 0;
                                decimal tongSales = pastSales + _ban.TongCong;
                                int tongDiemMoi = doanhSo1Diem > 0 ? (int)(tongSales / doanhSo1Diem) : 0;
                                int diemCu = doanhSo1Diem > 0 ? (int)(pastSales / doanhSo1Diem) : 0;
                                diemLanNay = Math.Max(0, tongDiemMoi - diemCu);
                            }
                            else
                            {
                                diemLanNay = doanhSo1Diem > 0 ? (int)(_ban.TongCong / doanhSo1Diem) : 0;
                            }

                            if (!string.IsNullOrEmpty(khId))
                            {
                                var khInfo = await conn.QueryFirstOrDefaultAsync("SELECT NAME, DIEMTICHLUYBANDAU FROM DKHACHHANG WHERE CAST(ID AS VARCHAR(50)) = @KhId", new { KhId = khId });
                                if (khInfo != null)
                                {
                                    khName = khInfo.NAME?.ToString() ?? khName;
                                    decimal diemBanDau = khInfo.DIEMTICHLUYBANDAU != null ? Convert.ToDecimal(khInfo.DIEMTICHLUYBANDAU) : 0;
                                    
                                    int tongDiem = 0;
                                    if (cachTinhDiem == "Điểm được tính trên tổng doanh số")
                                    {
                                        var pastSales = await conn.ExecuteScalarAsync<decimal?>("SELECT SUM(COALESCE(TONGCONG, 0)) FROM TDONHANG WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId AND STATUS = 2 AND CAST(ID AS VARCHAR(50)) <> @CurrentOrderId", new { KhId = khId, CurrentOrderId = _ban.ActiveOrderId ?? "" }) ?? 0;
                                        tongDiem = (int)(diemBanDau + ((pastSales + _ban.TongCong) / (doanhSo1Diem > 0 ? doanhSo1Diem : 20000)));
                                    }
                                    else
                                    {
                                        var pastPoints = await conn.ExecuteScalarAsync<decimal?>("SELECT SUM(CAST(DIEM AS DECIMAL(18,2))) FROM TDONHANG WHERE CAST(DKHACHHANGID AS VARCHAR(50)) = @KhId AND STATUS = 2 AND CAST(ID AS VARCHAR(50)) <> @CurrentOrderId", new { KhId = khId, CurrentOrderId = _ban.ActiveOrderId ?? "" }) ?? 0;
                                        tongDiem = (int)(diemBanDau + pastPoints + diemLanNay);
                                    }

                                    if (PanelDiemTichLuy != null)
                                    {
                                        PanelDiemTichLuy.Visibility = Visibility.Visible;
                                        TxtKhachHangTen.Text = string.IsNullOrWhiteSpace(khName) ? "Khách hàng" : khName;
                                        TxtDiemTichLuyHienTai.Text = $"{tongDiem:N0} điểm";
                                        TxtDiemTichLuyLanNay.Text = $"+{diemLanNay:N0} điểm";
                                    }
                                }
                                else if (PanelDiemTichLuy != null)
                                {
                                    PanelDiemTichLuy.Visibility = Visibility.Visible;
                                    TxtKhachHangTen.Text = string.IsNullOrWhiteSpace(khName) ? "Khách lẻ" : khName;
                                    TxtDiemTichLuyHienTai.Text = $"{diemLanNay:N0} điểm";
                                    TxtDiemTichLuyLanNay.Text = $"+{diemLanNay:N0} điểm";
                                }
                            }
                            else if (PanelDiemTichLuy != null)
                            {
                                PanelDiemTichLuy.Visibility = Visibility.Visible;
                                TxtKhachHangTen.Text = string.IsNullOrWhiteSpace(khName) ? "Khách lẻ" : khName;
                                TxtDiemTichLuyHienTai.Text = $"{diemLanNay:N0} điểm";
                                TxtDiemTichLuyLanNay.Text = $"+{diemLanNay:N0} điểm";
                            }
                        }
                    }
                    catch (Exception exPoint)
                    {
                        Console.WriteLine("Error calculate points: " + exPoint.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadStoreInfoAsync: " + ex.Message);
            }
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

            bool hasBreakdown = _ban.GiamGia > 0 || _ban.TienThue > 0 || _ban.TienPhiDichVu > 0;
            if (PanelTienHang != null)
            {
                PanelTienHang.Visibility = hasBreakdown ? Visibility.Visible : Visibility.Collapsed;
                TxtTienHang.Text = _ban.TienHang.ToString("N0");
            }

            if (_ban.GiamGia > 0)
            {
                PanelGiamGia.Visibility = Visibility.Visible;
                TxtGiamGiaLabel.Text = _ban.GiamGiaPhanTram > 0 ? $"Giảm giá ({_ban.GiamGiaPhanTram:0.##}%):" : "Giảm giá:";
                TxtGiamGia.Text = _ban.GiamGia.ToString("N0");
            }
            else
            {
                PanelGiamGia.Visibility = Visibility.Collapsed;
            }

            if (_ban.TienPhiDichVu > 0)
            {
                PanelPhiDichVu.Visibility = Visibility.Visible;
                TxtPhiDichVuLabel.Text = _ban.PhiDichVuPt > 0 ? $"Phí dịch vụ ({_ban.PhiDichVuPt:0.##}%):" : "Phí dịch vụ:";
                TxtPhiDichVu.Text = _ban.TienPhiDichVu.ToString("N0");
            }
            else
            {
                PanelPhiDichVu.Visibility = Visibility.Collapsed;
            }

            if (_ban.TienThue > 0)
            {
                PanelThueVAT.Visibility = Visibility.Visible;
                TxtThueVATLabel.Text = _ban.ThueSuatPt > 0 ? $"Thuế VAT ({_ban.ThueSuatPt:0.##}%):" : "Thuế VAT:";
                TxtThueVAT.Text = _ban.TienThue.ToString("N0");
            }
            else
            {
                PanelThueVAT.Visibility = Visibility.Collapsed;
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

                for (int i = 0; i < _soLanIn; i++)
                {
                    printDlg.PrintVisual(BillPaper, "In Hóa Đơn - " + _ban?.Name);
                }

                string msg = _soLanIn > 1 ? $"Đã gửi lệnh in {_soLanIn} bản đến máy in thành công!" : "Đã gửi lệnh in đến máy in thành công!";
                MessageBox.Show(msg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
