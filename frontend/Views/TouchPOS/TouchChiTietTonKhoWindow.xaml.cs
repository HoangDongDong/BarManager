using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchChiTietTonKhoWindow : Window
    {
        public string ItemName { get; }
        public string ItemId { get; }
        private List<ChiTietTonKhoEntry> _allEntries = new();
        private List<string> _usedColumns = new();

        public TouchChiTietTonKhoWindow(string itemId, string itemName)
        {
            InitializeComponent();
            ItemId = itemId;
            ItemName = itemName;
            TxtHeaderTitle.Text = $"CHI TIẾT TỒN KHO - {itemName.ToUpperInvariant()}";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Mẫu dữ liệu chi tiết phiếu xuất/nhập/tồn cho mặt hàng
                _allEntries = new List<ChiTietTonKhoEntry>
                {
                    new ChiTietTonKhoEntry
                    {
                        DonGia = 30000,
                        ThanhTien = 30000,
                        Dvt = "đĩa",
                        SoLuongXuat = 1,
                        SoLuongNhap = 0,
                        SoPhieu = "121600002",
                        DoiTuong = "Bán hàng",
                        Ngay = "20/06/2017",
                        GiamGiaPt = 0,
                        DienGiai = "Bán hàng"
                    }
                };

                await LoadColumnsConfigAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết tồn kho: " + ex.Message);
            }
        }

        private async Task LoadColumnsConfigAsync()
        {
            try
            {
                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLUMNS_ChiTietTonKho", "");
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    _usedColumns = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    _usedColumns = new List<string>
                    {
                        "ĐƠN GIÁ", "THÀNH TIỀN", "ĐVT", "SỐ LƯỢNG XUẤT", "SỐ LƯỢNG NHẬP",
                        "SỐ PHIẾU", "ĐỐI TƯỢNG", "NGÀY", "GIẢM GIÁ %", "DIỄN GIẢI"
                    };
                }

                BuildDisplayList();
            }
            catch { }
        }

        private void BuildDisplayList()
        {
            var displayItems = _allEntries.Select(entry => new
            {
                DisplayLines = entry.BuildLines(_usedColumns)
            }).ToList();

            DetailItemsControl.ItemsSource = displayItems;
        }

        private async void BtnGearSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("ChiTietTonKho");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadColumnsConfigAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cài đặt cột hiển thị: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ChiTietTonKhoDisplayLine
    {
        public string TextToDisplay { get; set; } = "";
        public double FontSize { get; set; } = 14;
        public FontWeight FontWeight { get; set; } = FontWeights.Bold;
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;
        public Brush ForegroundBrush { get; set; } = Brushes.White;
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Center;
    }

    public class ChiTietTonKhoEntry
    {
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string Dvt { get; set; } = "";
        public decimal SoLuongXuat { get; set; }
        public decimal SoLuongNhap { get; set; }
        public string SoPhieu { get; set; } = "";
        public string DoiTuong { get; set; } = "";
        public string Ngay { get; set; } = "";
        public decimal GiamGiaPt { get; set; }
        public string DienGiai { get; set; } = "";

        public List<ChiTietTonKhoDisplayLine> BuildLines(List<string> usedColumns)
        {
            var lines = new List<ChiTietTonKhoDisplayLine>();

            if (usedColumns == null || usedColumns.Count == 0)
            {
                usedColumns = new List<string>
                {
                    "ĐƠN GIÁ", "THÀNH TIỀN", "ĐVT", "SỐ LƯỢNG XUẤT", "SỐ LƯỢNG NHẬP",
                    "SỐ PHIẾU", "ĐỐI TƯỢNG", "NGÀY", "GIẢM GIÁ %", "DIỄN GIẢI"
                };
            }

            foreach (var rawCol in usedColumns)
            {
                if (string.IsNullOrWhiteSpace(rawCol)) continue;
                string[] parts = rawCol.Split(';');
                string colName = parts[0].Trim().ToUpperInvariant();
                bool isRight = parts.Length > 1 && parts[1].Trim().ToUpperInvariant() == "R";
                bool isBold = parts.Length <= 2 || parts[2].Trim() == "1";
                bool isItalic = parts.Length > 3 && parts[3].Trim() == "1";
                double fontSize = parts.Length > 4 && double.TryParse(parts[4].Trim(), out double fs) && fs > 0 ? fs : 14;
                string colorHex = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5].Trim() : "#FFFFFF";
                string shortTitle = parts.Length > 6 ? parts[6].Trim() : "";

                string val = colName switch
                {
                    "ĐƠN GIÁ" => DonGia != 0 ? DonGia.ToString("N0") : "",
                    "THÀNH TIỀN" => ThanhTien != 0 ? ThanhTien.ToString("N0") : "",
                    "ĐVT" => Dvt?.Trim() ?? "",
                    "SỐ LƯỢNG XUẤT" => SoLuongXuat != 0 ? SoLuongXuat.ToString("N0") : "",
                    "SỐ LƯỢNG NHẬP" => SoLuongNhap != 0 ? SoLuongNhap.ToString("N0") : "",
                    "SỐ PHIẾU" => SoPhieu?.Trim() ?? "",
                    "ĐỐI TƯỢNG" => DoiTuong?.Trim() ?? "",
                    "NGÀY" => Ngay?.Trim() ?? "",
                    "GIẢM GIÁ %" => GiamGiaPt != 0 ? $"{GiamGiaPt:N0}%" : "",
                    "DIỄN GIẢI" => DienGiai?.Trim() ?? "",
                    _ => ""
                };

                if (!string.IsNullOrWhiteSpace(val))
                {
                    string lineText = !string.IsNullOrWhiteSpace(shortTitle) ? $"{shortTitle}: {val}" : val;

                    Brush brush = Brushes.White;
                    try
                    {
                        brush = (Brush)new BrushConverter().ConvertFromString(colorHex)!;
                    }
                    catch { }

                    lines.Add(new ChiTietTonKhoDisplayLine
                    {
                        TextToDisplay = lineText,
                        FontSize = fontSize,
                        FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                        FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                        ForegroundBrush = brush,
                        Alignment = isRight ? HorizontalAlignment.Right : HorizontalAlignment.Center
                    });
                }
            }

            return lines;
        }
    }
}
