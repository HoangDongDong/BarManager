using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.InAn;

namespace QuanLyBar.Client.Views
{
    public partial class InCheBienWindow : Window
    {
        private string _banName;
        private List<PosDonHangChiTietViewModel> _items;
        private string _orderId;
        private bool _isAuto;
        private int _soLienIn = 1;

        public string PrinterDoAn
        {
            get => (string)GetValue(PrinterDoAnProperty);
            set => SetValue(PrinterDoAnProperty, value);
        }
        public static readonly DependencyProperty PrinterDoAnProperty =
            DependencyProperty.Register("PrinterDoAn", typeof(string), typeof(InCheBienWindow), new PropertyMetadata("(CHỌN MÁY IN)"));

        public string PrinterDoUong
        {
            get => (string)GetValue(PrinterDoUongProperty);
            set => SetValue(PrinterDoUongProperty, value);
        }
        public static readonly DependencyProperty PrinterDoUongProperty =
            DependencyProperty.Register("PrinterDoUong", typeof(string), typeof(InCheBienWindow), new PropertyMetadata("(CHỌN MÁY IN)"));

        public string PrinterDichVu
        {
            get => (string)GetValue(PrinterDichVuProperty);
            set => SetValue(PrinterDichVuProperty, value);
        }
        public static readonly DependencyProperty PrinterDichVuProperty =
            DependencyProperty.Register("PrinterDichVu", typeof(string), typeof(InCheBienWindow), new PropertyMetadata("(CHỌN MÁY IN)"));

        public string PrinterDoKhac
        {
            get => (string)GetValue(PrinterDoKhacProperty);
            set => SetValue(PrinterDoKhacProperty, value);
        }
        public static readonly DependencyProperty PrinterDoKhacProperty =
            DependencyProperty.Register("PrinterDoKhac", typeof(string), typeof(InCheBienWindow), new PropertyMetadata("(CHỌN MÁY IN)"));

        private const string CONFIG_FILE = "printer_kitchen_config.json";

        public InCheBienWindow(string banName, List<PosDonHangChiTietViewModel> itemsToPrint, string orderId = null, bool isAuto = false)
        {
            InitializeComponent();
            _banName = banName;
            _items = itemsToPrint ?? new List<PosDonHangChiTietViewModel>();
            _orderId = orderId;
            _isAuto = isAuto;
        }

        public void PrintDirectly()
        {
            LoadPrinterConfigs();
            _ = LoadSysConfigsAsync().ContinueWith(_ => Dispatcher.Invoke(PrintKitchenTicket));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPrinterConfigs();
            LoadCategorizedItems();
            await LoadSysConfigsAsync();
        }

        private async Task LoadSysConfigsAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                if (configs.TryGetValue("SoLienInCheBien", out var sl) && int.TryParse(sl, out int soLien) && soLien > 0)
                {
                    _soLienIn = soLien;
                }
            }
            catch { }
        }

        private void LoadPrinterConfigs()
        {
            string defaultPrinter = "Microsoft Print to PDF";
            try
            {
                foreach (string p in PrinterSettings.InstalledPrinters)
                {
                    defaultPrinter = p;
                    break;
                }
            }
            catch { }

            PrinterDoAn = defaultPrinter;
            PrinterDoUong = defaultPrinter;
            PrinterDichVu = defaultPrinter;
            PrinterDoKhac = defaultPrinter;

            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        if (dict.TryGetValue("DoAn", out var pAn) && !string.IsNullOrWhiteSpace(pAn)) PrinterDoAn = pAn;
                        if (dict.TryGetValue("DoUong", out var pUong) && !string.IsNullOrWhiteSpace(pUong)) PrinterDoUong = pUong;
                        if (dict.TryGetValue("DichVu", out var pDv) && !string.IsNullOrWhiteSpace(pDv)) PrinterDichVu = pDv;
                        if (dict.TryGetValue("DoKhac", out var pKhac) && !string.IsNullOrWhiteSpace(pKhac)) PrinterDoKhac = pKhac;
                    }
                }
            }
            catch { }
        }

        private void SavePrinterConfigs()
        {
            try
            {
                var dict = new Dictionary<string, string>
                {
                    { "DoAn", PrinterDoAn },
                    { "DoUong", PrinterDoUong },
                    { "DichVu", PrinterDichVu },
                    { "DoKhac", PrinterDoKhac }
                };
                string json = JsonSerializer.Serialize(dict);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch { }
        }

        private void LoadCategorizedItems()
        {
            var doAnList = new List<PosDonHangChiTietViewModel>();
            var doUongList = new List<PosDonHangChiTietViewModel>();
            var dichVuList = new List<PosDonHangChiTietViewModel>();
            var doKhacList = new List<PosDonHangChiTietViewModel>();

            foreach (var item in _items)
            {
                string cat = item.ItemCategory;
                if (cat == "DoAn") doAnList.Add(item);
                else if (cat == "DoUong") doUongList.Add(item);
                else if (cat == "DichVu") dichVuList.Add(item);
                else if (cat == "DoKhac") doKhacList.Add(item);
                else doAnList.Add(item);
            }

            IcDoAn.ItemsSource = doAnList;
            IcDoUong.ItemsSource = doUongList;
            IcDichVu.ItemsSource = dichVuList;
            IcDoKhac.ItemsSource = doKhacList;
        }

        private void BtnPrinterDoAn_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonMayInTouchWindow(PrinterDoAn);
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrEmpty(win.SelectedPrinter))
            {
                PrinterDoAn = win.SelectedPrinter;
                SavePrinterConfigs();
            }
        }

        private void BtnPrinterDoUong_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonMayInTouchWindow(PrinterDoUong);
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrEmpty(win.SelectedPrinter))
            {
                PrinterDoUong = win.SelectedPrinter;
                SavePrinterConfigs();
            }
        }

        private void BtnPrinterDichVu_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonMayInTouchWindow(PrinterDichVu);
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrEmpty(win.SelectedPrinter))
            {
                PrinterDichVu = win.SelectedPrinter;
                SavePrinterConfigs();
            }
        }

        private void BtnPrinterDoKhac_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonMayInTouchWindow(PrinterDoKhac);
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrEmpty(win.SelectedPrinter))
            {
                PrinterDoKhac = win.SelectedPrinter;
                SavePrinterConfigs();
            }
        }

        private async void BtnThucHienIn_Click(object sender, RoutedEventArgs e)
        {
            if (_items == null || _items.Count == 0)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "KHÔNG CÓ MẶT HÀNG NÀO ĐỂ IN!", "CẢNH BÁO");
                return;
            }

            try
            {
                PrintKitchenTicket();

                // Mark items as printed in DB
                if (!string.IsNullOrEmpty(_orderId))
                {
                    try
                    {
                        using var conn = DbConnectionManager.GetConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        string updateSql = "UPDATE TDONHANGCHITIET SET DTRANGTHAICHEBIENID = 1 WHERE CAST(TDONHANGID AS VARCHAR(50)) = @OrderId";
                        await conn.ExecuteAsync(updateSql, new { OrderId = _orderId });
                    }
                    catch { }
                }

                foreach (var it in _items)
                {
                    it.DaInCheBien = true;
                }

                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"ĐÃ GỬI LỆNH IN CHẾ BIẾN {_items.Count} MÓN CỦA BÀN '{_banName.ToUpper()}' XUỐNG BẾP THÀNH CÔNG!", "THÔNG BÁO");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"LỖI KHI IN CHẾ BIẾN: {ex.Message}", "LỖI");
            }
        }

        private void PrintKitchenTicket()
        {
            try
            {
                var groups = _items.GroupBy(x => x.ItemCategory).ToList();
                foreach (var grp in groups)
                {
                    string cat = grp.Key;
                    string printerName = PrinterDoAn;
                    if (cat == "DoUong") printerName = PrinterDoUong;
                    else if (cat == "DichVu") printerName = PrinterDichVu;
                    else if (cat == "DoKhac") printerName = PrinterDoKhac;

                    if (string.IsNullOrEmpty(printerName) || printerName.Contains("CHỌN MÁY IN"))
                    {
                        printerName = PrinterDoAn;
                    }

                    var itemsInGrp = grp.ToList();
                    var ticketPages = new List<List<PosDonHangChiTietViewModel>> { itemsInGrp };
                    PrintMultiPageTicket(ticketPages, printerName, _soLienIn);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi PrintKitchenTicket: {ex.Message}");
            }
        }

        private void PrintMultiPageTicket(List<List<PosDonHangChiTietViewModel>> ticketPages, string printerName, int copies)
        {
            if (ticketPages == null || ticketPages.Count == 0 || string.IsNullOrEmpty(printerName)) return;

            string currentUser = SessionContext.CurrentUser?.TenDangNhap ?? SessionContext.CurrentUser?.TenHienThi ?? "admin";

            for (int c = 0; c < copies; c++)
            {
                foreach (var pageItems in ticketPages)
                {
                    try
                    {
                        PrintDocument pd = new PrintDocument();
                        pd.PrinterSettings.PrinterName = printerName;

                        pd.PrintPage += (s, e) =>
                        {
                            var g = e.Graphics;
                            if (g == null) return;

                            System.Drawing.Font titleFont = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
                            System.Drawing.Font boldFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
                            System.Drawing.Font regularFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Regular);
                            System.Drawing.Font italicFont = new System.Drawing.Font("Arial", 9.5f, System.Drawing.FontStyle.Italic);

                            float y = 15;
                            float leftMargin = 10;
                            float rightMargin = e.PageBounds.Width - 10;
                            if (rightMargin - leftMargin > 280) rightMargin = leftMargin + 280; // Standard 80mm paper limit
                            float width = rightMargin - leftMargin;

                            System.Drawing.StringFormat centerFormat = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center };
                            System.Drawing.StringFormat leftFormat = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Near, LineAlignment = System.Drawing.StringAlignment.Center };
                            System.Drawing.StringFormat rightFormat = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Far, LineAlignment = System.Drawing.StringAlignment.Center };

                            // 1. Title: PHIẾU IN CHẾ BIẾN
                            g.DrawString("PHIẾU IN CHẾ BIẾN", titleFont, System.Drawing.Brushes.Black, leftMargin, y);
                            y += 26;

                            // 2. Info section
                            g.DrawString($"Bàn: {_banName}", boldFont, System.Drawing.Brushes.Black, leftMargin, y);
                            g.DrawString($"Lần: 1", boldFont, System.Drawing.Brushes.Black, leftMargin + 140, y);
                            y += 20;

                            g.DrawString($"Gửi bởi: {currentUser}", boldFont, System.Drawing.Brushes.Black, leftMargin, y);
                            y += 20;

                            g.DrawString($"Gửi lúc: {DateTime.Now:dd/MM/yyyy HH:mm}", boldFont, System.Drawing.Brushes.Black, leftMargin, y);
                            y += 24;

                            // 3. Table Box
                            float col1W = 32;  // TT
                            float col3W = 40;  // SL
                            float col2W = width - col1W - col3W; // Mặt hàng

                            float tableTopY = y;
                            float headerHeight = 24;
                            float rowHeight = 24;

                            int itemCount = pageItems.Count;
                            float totalTableHeight = headerHeight + (itemCount * rowHeight);

                            // Outer rectangle border
                            g.DrawRectangle(System.Drawing.Pens.Black, leftMargin, tableTopY, width, totalTableHeight);

                            // Header background/line
                            g.DrawLine(System.Drawing.Pens.Black, leftMargin, tableTopY + headerHeight, leftMargin + width, tableTopY + headerHeight);

                            // Vertical column divider lines
                            float col1X = leftMargin + col1W;
                            float col2X = col1X + col2W;
                            g.DrawLine(System.Drawing.Pens.Black, col1X, tableTopY, col1X, tableTopY + totalTableHeight);
                            g.DrawLine(System.Drawing.Pens.Black, col2X, tableTopY, col2X, tableTopY + totalTableHeight);

                            // Header texts
                            g.DrawString("TT", boldFont, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(leftMargin, tableTopY, col1W, headerHeight), centerFormat);
                            g.DrawString("Mặt hàng", boldFont, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col1X + 4, tableTopY, col2W - 8, headerHeight), centerFormat);
                            g.DrawString("SL", boldFont, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col2X, tableTopY, col3W, headerHeight), centerFormat);

                            // Data rows
                            using var dashedPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                            float currentY = tableTopY + headerHeight;

                            for (int i = 0; i < itemCount; i++)
                            {
                                var item = pageItems[i];
                                var rowRect1 = new System.Drawing.RectangleF(leftMargin, currentY, col1W, rowHeight);
                                var rowRect2 = new System.Drawing.RectangleF(col1X + 4, currentY, col2W - 8, rowHeight);
                                var rowRect3 = new System.Drawing.RectangleF(col2X, currentY, col3W, rowHeight);

                                g.DrawString((i + 1).ToString(), regularFont, System.Drawing.Brushes.Black, rowRect1, centerFormat);
                                g.DrawString(item.MatHangName ?? "", boldFont, System.Drawing.Brushes.Black, rowRect2, leftFormat);
                                g.DrawString(item.SoLuong.ToString("N0"), boldFont, System.Drawing.Brushes.Black, rowRect3, centerFormat);

                                currentY += rowHeight;

                                // Dashed divider line under row (except last row)
                                if (i < itemCount - 1)
                                {
                                    g.DrawLine(dashedPen, leftMargin, currentY, leftMargin + width, currentY);
                                }
                            }

                            y = tableTopY + totalTableHeight + 15;

                            // 4. Footer: ---Kết thúc---
                            g.DrawString("---Kết thúc---", italicFont, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(leftMargin, y, width, 20), centerFormat);
                        };

                        pd.Print();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi gửi trang in tới '{printerName}': {ex.Message}");
                    }
                }
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
