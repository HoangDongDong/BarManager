using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.CauHinhHeThong;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public class CardFieldVM
    {
        public string Text { get; set; } = "";
        public bool IsBold { get; set; } = false;
        public double FontSize { get; set; } = 13;
        public string ForegroundHex { get; set; } = "#E0E0E0";
        public System.Windows.Media.Brush ForegroundBrush =>
            (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(ForegroundHex)!;
        public FontWeight FontWeight => IsBold ? FontWeights.Bold : FontWeights.Normal;
    }

    public class OpenOrderItemVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double _cardHeight = 100;
        public double CardHeight
        {
            get => _cardHeight;
            set
            {
                if (Math.Abs(_cardHeight - value) > 0.1)
                {
                    _cardHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OrderId { get; set; } = "";
        public string BanId { get; set; } = "";
        public string BanName { get; set; } = "";
        public decimal TongCong { get; set; }
        public DateTime? Ngay { get; set; }
        public string SoPhieu { get; set; } = "";
        public string SoHD { get; set; } = "";
        public string KhachHangName { get; set; } = "";
        public string NhanVienName { get; set; } = "";

        public bool ShowTongCong { get; set; } = true;
        public List<CardFieldVM> DisplayFields { get; set; } = new();

        public string NgayDisplay => Ngay.HasValue ? Ngay.Value.ToString("dd/MM/yyyy") : "";
        public string SoPhieuDisplay => !string.IsNullOrWhiteSpace(SoPhieu) ? SoPhieu : (!string.IsNullOrWhiteSpace(SoHD) ? $"HD{SoHD}" : "");

        public System.Windows.Media.Brush CardBackgroundBrush { get; set; } = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#134E54"));

        public Visibility TongCongVisibility => ShowTongCong ? Visibility.Visible : Visibility.Collapsed;
    }

    public partial class DanhSachHoaDonChuaThanhToanWindow : Window
    {
        public string? SelectedBanId { get; private set; }
        public string? SelectedOrderId { get; private set; }
        private int _rowsConfig = 2;

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(DanhSachHoaDonChuaThanhToanWindow), new PropertyMetadata(2));

        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        public static readonly DependencyProperty CardHeightProperty =
            DependencyProperty.Register(nameof(CardHeight), typeof(double), typeof(DanhSachHoaDonChuaThanhToanWindow), new PropertyMetadata(120.0));

        public double CardHeight
        {
            get => (double)GetValue(CardHeightProperty);
            set => SetValue(CardHeightProperty, value);
        }

        public DanhSachHoaDonChuaThanhToanWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var list = new List<OpenOrderItemVM>();
            try
            {
                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLUMNS_HoaDonChuaThanhToan", "");
                List<string> usedCols;
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    usedCols = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    usedCols = new List<string> { "BÀN", "TỔNG CỘNG", "NHÂN VIÊN", "NGÀY", "KHÁCH HÀNG", "SỐ PHIẾU" };
                }

                string savedLayoutStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_HoaDonChuaThanhToan", "2|2|#134E54|1");
                string[] layoutParts = savedLayoutStr.Split('|');
                int columns = (layoutParts.Length > 0 && int.TryParse(layoutParts[0], out int cols) && cols >= 1) ? cols : 2;
                int rows = (layoutParts.Length > 1 && int.TryParse(layoutParts[1], out int rws) && rws >= 1) ? rws : 2;
                string colorHex = (layoutParts.Length > 2 && !string.IsNullOrWhiteSpace(layoutParts[2])) ? layoutParts[2] : "#134E54";

                ColumnCount = Math.Max(1, columns);
                _rowsConfig = Math.Max(1, rows);

                System.Windows.Media.Brush cardBgBrush;
                try
                {
                    cardBgBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(colorHex)!;
                }
                catch
                {
                    cardBgBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#134E54"));
                }

                bool showTongCong = usedCols.Contains("TỔNG CỘNG");

                void SetupItemDisplayFields(OpenOrderItemVM item)
                {
                    item.ShowTongCong = showTongCong;
                    item.CardBackgroundBrush = cardBgBrush;
                    item.DisplayFields.Clear();

                    foreach (var col in usedCols)
                    {
                        switch (col)
                        {
                            case "BÀN":
                                if (!string.IsNullOrWhiteSpace(item.BanName))
                                {
                                    item.DisplayFields.Add(new CardFieldVM
                                    {
                                        Text = item.BanName,
                                        IsBold = true,
                                        FontSize = 15,
                                        ForegroundHex = "#FFFFFF"
                                    });
                                }
                                break;

                            case "NGÀY":
                                if (!string.IsNullOrWhiteSpace(item.NgayDisplay))
                                {
                                    item.DisplayFields.Add(new CardFieldVM
                                    {
                                        Text = item.NgayDisplay,
                                        IsBold = false,
                                        FontSize = 13,
                                        ForegroundHex = "#E0E0E0"
                                    });
                                }
                                break;

                            case "NHÂN VIÊN":
                                if (!string.IsNullOrWhiteSpace(item.NhanVienName))
                                {
                                    item.DisplayFields.Add(new CardFieldVM
                                    {
                                        Text = item.NhanVienName,
                                        IsBold = false,
                                        FontSize = 13,
                                        ForegroundHex = "#E0E0E0"
                                    });
                                }
                                break;

                            case "KHÁCH HÀNG":
                                if (!string.IsNullOrWhiteSpace(item.KhachHangName))
                                {
                                    item.DisplayFields.Add(new CardFieldVM
                                    {
                                        Text = item.KhachHangName,
                                        IsBold = false,
                                        FontSize = 13,
                                        ForegroundHex = "#E0E0E0"
                                    });
                                }
                                break;

                            case "SỐ PHIẾU":
                                if (!string.IsNullOrWhiteSpace(item.SoPhieuDisplay))
                                {
                                    item.DisplayFields.Add(new CardFieldVM
                                    {
                                        Text = item.SoPhieuDisplay,
                                        IsBold = false,
                                        FontSize = 13,
                                        ForegroundHex = "#E0E0E0"
                                    });
                                }
                                break;
                        }
                    }
                }

                // 1. Fetch active occupied tables from LocalSuDungDichVuService (Exact same logic as main screen)
                try
                {
                    var dichVuService = new LocalSuDungDichVuService();
                    var kvBanList = await dichVuService.GetKhuVucBanListAsync();

                    foreach (var kv in kvBanList)
                    {
                        foreach (var b in kv.BanList)
                        {
                            if (b.IsOccupied && !string.IsNullOrEmpty(b.ActiveOrderId))
                            {
                                var item = new OpenOrderItemVM
                                {
                                    OrderId = b.ActiveOrderId,
                                    BanId = b.Id,
                                    BanName = !string.IsNullOrWhiteSpace(b.Name) ? b.Name : $"Bàn {b.Id}",
                                    TongCong = b.TongCong,
                                    Ngay = b.StartTime,
                                    SoPhieu = b.SoPhieu,
                                    KhachHangName = b.KhachHangName,
                                    NhanVienName = !string.IsNullOrWhiteSpace(b.NhanVienName) ? b.NhanVienName : "Admin"
                                };
                                SetupItemDisplayFields(item);
                                list.Add(item);
                            }
                        }
                    }
                }
                catch (Exception exService)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi service.GetKhuVucBanListAsync: {exService}");
                }

                // 2. Fetch open orders directly in TDONHANG if any exist and not yet added
                try
                {
                    using var conn = DbConnectionManager.GetConnection();
                    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                    string sqlFallback = @"
                        SELECT 
                            CAST(h.ID AS VARCHAR(50)) as OrderId, 
                            CAST(h.DBANID AS VARCHAR(50)) as BanId,
                            b.NAME as BanName, 
                            COALESCE(h.TONGCONG, 0) as TongCong,
                            h.NGAY as Ngay,
                            h.NAME as SoPhieu,
                            h.SOHD as SoHD,
                            kh.NAME as KhachHangName,
                            nv.NAME as NhanVienName
                        FROM TDONHANG h 
                        LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50)) 
                        LEFT JOIN DKHACHHANG kh ON CAST(h.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(h.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE (h.STATUS = 1 OR h.STATUS IS NULL)
                          AND (h.NAME IS NULL OR h.NAME NOT LIKE 'CNBD%')
                        ORDER BY b.NAME ASC, h.TIMECREATED DESC";

                    var fallbackList = (await conn.QueryAsync<OpenOrderItemVM>(sqlFallback)).ToList();
                    foreach (var item in fallbackList)
                    {
                        if (!list.Any(x => x.OrderId == item.OrderId))
                        {
                            if (string.IsNullOrWhiteSpace(item.BanName) && !string.IsNullOrWhiteSpace(item.BanId))
                            {
                                item.BanName = $"Bàn {item.BanId}";
                            }
                            if (string.IsNullOrWhiteSpace(item.BanName))
                            {
                                item.BanName = "Bàn";
                            }
                            if (string.IsNullOrWhiteSpace(item.NhanVienName))
                            {
                                item.NhanVienName = "Admin";
                            }

                            SetupItemDisplayFields(item);
                            list.Add(item);
                        }
                    }
                }
                catch (Exception exSql)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi sqlFallback: {exSql}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadDataAsync: {ex}");
            }
            finally
            {
                IcOpenOrders.ItemsSource = list;
                UpdateCalculatedCardHeights();
            }
        }

        private void UpdateCalculatedCardHeights()
        {
            try
            {
                double containerHeight = SvOpenOrders != null && SvOpenOrders.ActualHeight > 50 ? SvOpenOrders.ActualHeight : 600;
                int rows = _rowsConfig > 0 ? _rowsConfig : 2;
                CardHeight = Math.Max(70.0, Math.Floor((containerHeight - (rows * 12.0)) / rows));
            }
            catch { }
        }

        private void SvOpenOrders_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCalculatedCardHeights();
        }

        private void OrderCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is OpenOrderItemVM item)
            {
                SelectedBanId = item.BanId;
                SelectedOrderId = item.OrderId;
                DialogResult = true;
                Close();
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThietLapDinhDangThanhPhanWindow("HoaDonChuaThanhToan");
            win.Owner = this;
            win.ShowDialog();
            _ = LoadDataAsync();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
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
