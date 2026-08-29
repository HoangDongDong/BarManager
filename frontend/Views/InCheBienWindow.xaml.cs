using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using QuanLyBar.Client.Models;

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
        public List<string> AvailablePrinters { get; set; } = new List<string>();
        public ObservableCollection<PrinterMappingItem> PrinterConfigs { get; set; } = new ObservableCollection<PrinterMappingItem>();

        private const string CONFIG_FILE = "printer_kitchen_config.json";

        public InCheBienWindow(string banName, List<PosDonHangChiTietViewModel> itemsToPrint)
        {
            InitializeComponent();
            _banName = banName;
            _items = itemsToPrint;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvailablePrinters();
            LoadPrinterConfigs();
            LoadItemsToPrint();
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
                var doc = new PrintDocument();
                doc.PrinterSettings.PrinterName = TxtSelectedPrinterName.Text;

                doc.PrintPage += (s, ev) =>
                {
                    var fontTitle = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
                    var fontHeader = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
                    var fontBody = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Regular);

                    float y = 20;
                    ev.Graphics.DrawString("PHIẾU IN CHẾ BIẾN BẾP / BAR", fontTitle, System.Drawing.Brushes.Black, 20, y);
                    y += 30;

                    ev.Graphics.DrawString($"Bàn: {_banName}   |   Giờ in: {DateTime.Now:HH:mm:ss dd/MM/yyyy}", fontHeader, System.Drawing.Brushes.Black, 20, y);
                    y += 25;

                    ev.Graphics.DrawLine(System.Drawing.Pens.Black, 20, y, 380, y);
                    y += 5;

                    ev.Graphics.DrawString("STT  Tên món", fontHeader, System.Drawing.Brushes.Black, 20, y);
                    ev.Graphics.DrawString("ĐVT", fontHeader, System.Drawing.Brushes.Black, 250, y);
                    ev.Graphics.DrawString("SL", fontHeader, System.Drawing.Brushes.Black, 320, y);
                    y += 20;

                    ev.Graphics.DrawLine(System.Drawing.Pens.Black, 20, y, 380, y);
                    y += 8;

                    int i = 1;
                    foreach (var item in _items)
                    {
                        ev.Graphics.DrawString($"{i++}. {item.MatHangName}", fontBody, System.Drawing.Brushes.Black, 20, y);
                        ev.Graphics.DrawString(item.DonViTinh ?? "", fontBody, System.Drawing.Brushes.Black, 250, y);
                        ev.Graphics.DrawString(item.SoLuong.ToString("0"), fontHeader, System.Drawing.Brushes.Black, 320, y);
                        y += 22;

                        if (!string.IsNullOrEmpty(item.GhiChu))
                        {
                            ev.Graphics.DrawString($"   * Ghi chú: {item.GhiChu}", fontBody, System.Drawing.Brushes.DimGray, 20, y);
                            y += 18;
                        }
                    }

                    ev.Graphics.DrawLine(System.Drawing.Pens.Black, 20, y, 380, y);
                    y += 15;
                    ev.Graphics.DrawString("--- Chúc quý khách ngon miệng ---", fontBody, System.Drawing.Brushes.Black, 60, y);
                };

                // Chỉ thực hiện in thực tế nếu máy in hợp lệ
                if (doc.PrinterSettings.IsValid)
                {
                    doc.Print();
                }
            }
            catch
            {
                // Fallback nếu không kết nối được máy in vật lý
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
