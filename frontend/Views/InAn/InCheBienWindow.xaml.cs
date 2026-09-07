using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class PrinterMappingItem
    {
        public string LoaiDoId { get; set; }
        public string LoaiDoName { get; set; }
        public string SelectedPrinter { get; set; }
    }

    public class PrintItemViewModel
    {
        public int STT { get; set; }
        public PosDonHangChiTietViewModel Item { get; set; }
        public string MatHangName => Item?.MatHangName ?? "";
        public string DonViTinh => Item?.DonViTinh ?? "";
        public string GhiChu => Item?.GhiChu ?? "";
        public decimal SoLuong => Item?.SoLuong ?? 0;
    }

    public partial class InCheBienWindow : Window
    {
        private string _banName;
        private List<PosDonHangChiTietViewModel> _items;
        private string _orderId;
        private bool _isAuto;
        private int _soLienIn = 1;
        private bool _inThemTaiQuay = false;
        private bool _inMoiDoRa1To = false;
        private bool _inRiengDoAnUong = false;
        private string _mauInCheBien = "Mẫu 80mm";
        public List<string> AvailablePrinters { get; set; } = new List<string>();
        public ObservableCollection<PrinterMappingItem> PrinterConfigs { get; set; } = new ObservableCollection<PrinterMappingItem>();

        private const string CONFIG_FILE = "printer_kitchen_config.json";

        public InCheBienWindow(string banName, List<PosDonHangChiTietViewModel> itemsToPrint, string orderId = null, bool isAuto = false)
        {
            InitializeComponent();
            _banName = banName;
            _items = itemsToPrint;
            _orderId = orderId;
            _isAuto = isAuto;
            DataContext = this;
        }

        public void PrintDirectly()
        {
            LoadAvailablePrinters();
            LoadPrinterConfigs();
            _ = LoadSysConfigsAsync().ContinueWith(_ => Dispatcher.Invoke(PrintKitchenTicket));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvailablePrinters();
            LoadPrinterConfigs();
            LoadItemsToPrint();
            await LoadSysConfigsAsync();
        }

        private async System.Threading.Tasks.Task LoadSysConfigsAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                if (configs.TryGetValue("SoLienInCheBien", out var sl) && int.TryParse(sl, out int soLien) && soLien > 0)
                {
                    _soLienIn = soLien;
                }
                if (configs.TryGetValue("InThem1LienTaiQuay", out var itq))
                {
                    _inThemTaiQuay = itq == "1" || itq.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                if (configs.TryGetValue("InMoiDoRa1To", out var imd))
                {
                    _inMoiDoRa1To = imd == "1" || imd.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                if (configs.TryGetValue("InRiengDoAnUong", out var ir))
                {
                    _inRiengDoAnUong = ir == "1" || ir.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                if (configs.TryGetValue("MauInCheBien", out var mcb) && !string.IsNullOrWhiteSpace(mcb))
                {
                    _mauInCheBien = mcb;
                }
            }
            catch { }
        }

        private void LoadAvailablePrinters()
        {
            AvailablePrinters.Clear();
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    AvailablePrinters.Add(printer);
                }
            }
            catch { }

            if (AvailablePrinters.Count == 0)
            {
                AvailablePrinters.Add("Microsoft Print to PDF");
            }
        }

        private void LoadPrinterConfigs()
        {
            PrinterConfigs.Clear();
            string defaultPrinter = AvailablePrinters.FirstOrDefault(p => p.Contains("PDF")) ?? AvailablePrinters.FirstOrDefault() ?? "Microsoft Print to PDF";

            var defaultConfigs = new List<PrinterMappingItem>
            {
                new PrinterMappingItem { LoaiDoId = "1", LoaiDoName = "Đồ ăn", SelectedPrinter = defaultPrinter },
                new PrinterMappingItem { LoaiDoId = "2", LoaiDoName = "Đồ uống", SelectedPrinter = defaultPrinter },
                new PrinterMappingItem { LoaiDoId = "4", LoaiDoName = "Đồ khác", SelectedPrinter = defaultPrinter }
            };

            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    var saved = JsonSerializer.Deserialize<List<PrinterMappingItem>>(json);
                    if (saved != null && saved.Count > 0)
                    {
                        defaultConfigs = saved;
                    }
                }
            }
            catch { }

            foreach (var cfg in defaultConfigs)
            {
                if (!AvailablePrinters.Contains(cfg.SelectedPrinter))
                {
                    cfg.SelectedPrinter = defaultPrinter;
                }
                PrinterConfigs.Add(cfg);
            }

            DgConfigMayIn.ItemsSource = PrinterConfigs;
            TxtSelectedPrinterName.Text = PrinterConfigs.FirstOrDefault()?.SelectedPrinter ?? defaultPrinter;
        }

        private void LoadItemsToPrint()
        {
            var list = new List<PrintItemViewModel>();
            int idx = 1;
            foreach (var it in _items)
            {
                list.Add(new PrintItemViewModel
                {
                    STT = idx++,
                    Item = it
                });
            }
            DgMonIn.ItemsSource = list;
        }

        private void BtnThietLapMayIn_Click(object sender, RoutedEventArgs e)
        {
            GridInCheBien.Visibility = Visibility.Collapsed;
            GridButtonsInCheBien.Visibility = Visibility.Collapsed;

            GridThietLapMayIn.Visibility = Visibility.Visible;
            GridButtonsThietLap.Visibility = Visibility.Visible;
        }

        private void BtnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = JsonSerializer.Serialize(PrinterConfigs.ToList());
                File.WriteAllText(CONFIG_FILE, json);
                MessageBox.Show("Lưu cấu hình máy in thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu cấu hình: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TxtSelectedPrinterName.Text = PrinterConfigs.FirstOrDefault()?.SelectedPrinter ?? "Microsoft Print to PDF";

            GridThietLapMayIn.Visibility = Visibility.Collapsed;
            GridButtonsThietLap.Visibility = Visibility.Collapsed;

            GridInCheBien.Visibility = Visibility.Visible;
            GridButtonsInCheBien.Visibility = Visibility.Visible;
        }

        private void BtnHuyBoThietLap_Click(object sender, RoutedEventArgs e)
        {
            LoadPrinterConfigs();
            GridThietLapMayIn.Visibility = Visibility.Collapsed;
            GridButtonsThietLap.Visibility = Visibility.Collapsed;

            GridInCheBien.Visibility = Visibility.Visible;
            GridButtonsInCheBien.Visibility = Visibility.Visible;
        }

        private void BtnThucHienIn_Click(object sender, RoutedEventArgs e)
        {
            if (_items == null || _items.Count == 0)
            {
                MessageBox.Show("Không có mặt hàng nào để in", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Thực hiện gửi lệnh in bếp
                PrintKitchenTicket();

                // Đánh dấu các món đã in chế biến
                foreach (var it in _items)
                {
                    it.DaInCheBien = true;
                }

                MessageBox.Show($"Đã gửi lệnh in chế biến {_items.Count} món của bàn '{_banName}' xuống bếp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in chế biến: " + ex.Message, "Lỗi in", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintKitchenTicket()
        {
            try
            {
                string kitchenPrinter = TxtSelectedPrinterName.Text;
                string defaultPrinter = AvailablePrinters.FirstOrDefault(p => !p.Contains("PDF")) ?? AvailablePrinters.FirstOrDefault() ?? kitchenPrinter;

                // Xây dựng danh sách các phiếu cần in (Mỗi phiếu là 1 trang in / 1 lần cắt giấy)
                var ticketPages = new List<List<PosDonHangChiTietViewModel>>();

                if (_inMoiDoRa1To)
                {
                    // In mỗi đồ ra 1 tờ (mỗi món là 1 trang / 1 lần cắt giấy)
                    foreach (var item in _items)
                    {
                        ticketPages.Add(new List<PosDonHangChiTietViewModel> { item });
                    }
                }
                else if (_inRiengDoAnUong)
                {
                    // In riêng theo từng nhóm đồ ăn / đồ uống / đồ khác
                    var groups = _items.GroupBy(x => x.ItemCategory).ToList();
                    foreach (var grp in groups)
                    {
                        ticketPages.Add(grp.ToList());
                    }
                }
                else
                {
                    // In tất cả các món trên cùng 1 trang
                    ticketPages.Add(_items);
                }

                PrintMultiPageTicket(ticketPages, kitchenPrinter, _soLienIn);
                if (_inThemTaiQuay)
                {
                    PrintMultiPageTicket(ticketPages, defaultPrinter, 1);
                }

                // Ghi lưu vết hoạt động in pha chế theo định dạng hệ thống
                if (!string.IsNullOrEmpty(_orderId) && _items != null && _items.Count > 0)
                {
                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        string header = _isAuto ? "In pha chế tự động" : "In pha chế";
                        await LocalLuuVetService.GhiLuuVetAsync(_orderId, _banName, "Sử dụng dịch vụ", header, 0);

                        string nhomStr = "Đồ ăn, đồ uống, đồ khác";
                        await LocalLuuVetService.GhiLuuVetAsync(_orderId, _banName, "Sử dụng dịch vụ", $"--Loại đồ '{nhomStr}', máy in: {kitchenPrinter}, số liên: {_soLienIn}, lần số 1", 0);

                        if (_inThemTaiQuay)
                        {
                            await LocalLuuVetService.GhiLuuVetAsync(_orderId, _banName, "Sử dụng dịch vụ", $"--In 1 liên tại quầy: {defaultPrinter}", 0);
                        }

                        foreach (var it in _items)
                        {
                            await LocalLuuVetService.GhiLuuVetAsync(_orderId, _banName, "Sử dụng dịch vụ", $"  Mặt hàng: {it.MatHangName}, Số lượng: {it.SoLuong:0.##}", 0);
                        }

                        await LocalLuuVetService.GhiLuuVetAsync(_orderId, _banName, "Sử dụng dịch vụ", $"--In chế biến hoàn thành: {kitchenPrinter}", 0);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrintKitchenTicket: " + ex.Message);
            }
        }

        private void PrintMultiPageTicket(List<List<PosDonHangChiTietViewModel>> ticketPages, string printerName, int copies)
        {
            if (ticketPages == null || ticketPages.Count == 0) return;

            try
            {
                var doc = new PrintDocument();
                if (!string.IsNullOrEmpty(printerName))
                {
                    doc.PrinterSettings.PrinterName = printerName;
                }
                doc.PrinterSettings.Copies = (short)Math.Max(1, copies);

                bool is58mm = _mauInCheBien.Contains("58");
                float leftX = 10;
                float tableWidth = is58mm ? 190 : 270;
                float colTTWidth = is58mm ? 28 : 35;
                float colSLWidth = is58mm ? 32 : 42;
                float colNameWidth = tableWidth - colTTWidth - colSLWidth;

                int currentPageIndex = 0;

                doc.PrintPage += (s, ev) =>
                {
                    if (currentPageIndex >= ticketPages.Count)
                    {
                        ev.HasMorePages = false;
                        return;
                    }

                    var itemsToPrint = ticketPages[currentPageIndex];
                    var g = ev.Graphics;
                    var fontTitle = new System.Drawing.Font("Arial", is58mm ? 12 : 14, System.Drawing.FontStyle.Bold);
                    var fontLabelBold = new System.Drawing.Font("Arial", is58mm ? 8.5f : 9.5f, System.Drawing.FontStyle.Bold);
                    var fontLabel = new System.Drawing.Font("Arial", is58mm ? 8.5f : 9.5f, System.Drawing.FontStyle.Regular);
                    var fontItemName = new System.Drawing.Font("Arial", is58mm ? 8.5f : 9.5f, System.Drawing.FontStyle.Bold);
                    var fontFooter = new System.Drawing.Font("Arial", is58mm ? 8.5f : 9.5f, System.Drawing.FontStyle.Italic);

                    using var solidPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1f);
                    using var dashPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1f)
                    {
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                    };

                    using var sfCenter = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center };
                    using var sfLeft = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Near, LineAlignment = System.Drawing.StringAlignment.Center };
                    using var sfRight = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Far, LineAlignment = System.Drawing.StringAlignment.Center };

                    float y = 15;

                    // 1. Tiêu đề: PHIẾU IN CHẾ BIẾN
                    var titleRect = new System.Drawing.RectangleF(leftX, y, tableWidth, 24);
                    g.DrawString("PHIẾU IN CHẾ BIẾN", fontTitle, System.Drawing.Brushes.Black, titleRect, sfCenter);
                    y += 28;

                    // 2. Bàn & Lần in
                    string banText = _banName.StartsWith("Bàn", StringComparison.OrdinalIgnoreCase) ? _banName : $"Bàn: {_banName}";
                    if (!banText.StartsWith("Bàn:", StringComparison.OrdinalIgnoreCase))
                    {
                        banText = $"Bàn: {banText}";
                    }
                    g.DrawString(banText, fontLabelBold, System.Drawing.Brushes.Black, leftX, y);
                    g.DrawString("Lần: 1", fontLabelBold, System.Drawing.Brushes.Black, leftX + tableWidth - 45, y);
                    y += 20;

                    // 3. Gửi bởi
                    string userName = SessionContext.CurrentUser?.TenDangNhap ?? "Administrator";
                    g.DrawString($"Gửi bởi: {userName}", fontLabelBold, System.Drawing.Brushes.Black, leftX, y);
                    y += 20;

                    // 4. Gửi lúc
                    g.DrawString($"Gửi lúc: {DateTime.Now:dd/MM/yyyy HH:mm}", fontLabelBold, System.Drawing.Brushes.Black, leftX, y);
                    y += 24;

                    // 5. Bảng món ăn
                    float tableTopY = y;
                    float headerHeight = 22;

                    // Header text
                    float col1X = leftX;
                    float col2X = leftX + colTTWidth;
                    float col3X = leftX + colTTWidth + colNameWidth;

                    g.DrawString("TT", fontLabelBold, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col1X, y, colTTWidth, headerHeight), sfCenter);
                    g.DrawString("Mặt hàng", fontLabelBold, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col2X, y, colNameWidth, headerHeight), sfCenter);
                    g.DrawString("SL", fontLabelBold, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col3X, y, colSLWidth, headerHeight), sfCenter);

                    y += headerHeight;
                    g.DrawLine(solidPen, leftX, y, leftX + tableWidth, y);

                    // Danh sách món
                    int stt = 1;
                    for (int idx = 0; idx < itemsToPrint.Count; idx++)
                    {
                        var item = itemsToPrint[idx];
                        float rowStartY = y;

                        // Đo chiều cao của tên món khi wrap
                        var nameSize = g.MeasureString(item.MatHangName, fontItemName, (int)colNameWidth - 4);
                        float textHeight = Math.Max(22, nameSize.Height + 6);
                        if (!string.IsNullOrEmpty(item.GhiChu))
                        {
                            var noteSize = g.MeasureString($"*{item.GhiChu}", fontLabel, (int)colNameWidth - 4);
                            textHeight += noteSize.Height + 2;
                        }

                        float rowHeight = textHeight;

                        // Cột TT
                        g.DrawString(stt.ToString(), fontLabel, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col1X, rowStartY, colTTWidth, rowHeight), sfCenter);

                        // Cột Mặt hàng
                        float nameY = rowStartY + 3;
                        g.DrawString(item.MatHangName, fontItemName, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col2X + 2, nameY, colNameWidth - 4, nameSize.Height));
                        if (!string.IsNullOrEmpty(item.GhiChu))
                        {
                            g.DrawString($"*{item.GhiChu}", fontLabel, System.Drawing.Brushes.DimGray, new System.Drawing.RectangleF(col2X + 2, nameY + nameSize.Height + 1, colNameWidth - 4, 18));
                        }

                        // Cột SL
                        g.DrawString(item.SoLuong.ToString("0"), fontItemName, System.Drawing.Brushes.Black, new System.Drawing.RectangleF(col3X, rowStartY, colSLWidth, rowHeight), sfCenter);

                        y += rowHeight;

                        // Đường kẻ ngang nét đứt giữa các dòng (nếu chưa phải dòng cuối)
                        if (idx < itemsToPrint.Count - 1)
                        {
                            g.DrawLine(dashPen, leftX, y, leftX + tableWidth, y);
                        }
                        stt++;
                    }

                    float tableBottomY = y;
                    float tableTotalHeight = tableBottomY - tableTopY;

                    // Vẽ khung viền bao quanh bảng (nét liền)
                    g.DrawRectangle(solidPen, leftX, tableTopY, tableWidth, tableTotalHeight);

                    // Đường chia cột dọc (header nét liền, thân bảng nét đứt)
                    // Cột TT
                    g.DrawLine(solidPen, col2X, tableTopY, col2X, tableTopY + headerHeight);
                    g.DrawLine(dashPen, col2X, tableTopY + headerHeight, col2X, tableBottomY);

                    // Cột SL
                    g.DrawLine(solidPen, col3X, tableTopY, col3X, tableTopY + headerHeight);
                    g.DrawLine(dashPen, col3X, tableTopY + headerHeight, col3X, tableBottomY);

                    y += 12;

                    // 6. Chân phiếu: ---Kết thúc---
                    var footerRect = new System.Drawing.RectangleF(leftX, y, tableWidth, 20);
                    g.DrawString("---Kết thúc---", fontFooter, System.Drawing.Brushes.Black, footerRect, sfCenter);

                    currentPageIndex++;
                    ev.HasMorePages = (currentPageIndex < ticketPages.Count);
                };

                // Chỉ thực hiện in thực tế nếu máy in hợp lệ
                if (doc.PrinterSettings.IsValid)
                {
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrintMultiPageTicket: " + ex.Message);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
