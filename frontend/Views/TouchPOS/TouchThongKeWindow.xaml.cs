using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dapper;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.CauHinhHeThong;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public class PaidInvoiceItemVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string OrderId { get; set; } = "";
        public string BanId { get; set; } = "";
        public string BanName { get; set; } = "";
        public string SoPhieu { get; set; } = "";
        public string SoHD { get; set; } = "";
        public decimal TongCong { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
        public decimal TienThe { get; set; }
        public decimal CongNo { get; set; }
        public decimal Voucher { get; set; }
        public decimal TruTichLuy { get; set; }
        public decimal TheTraTruoc { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BorderBrushColor));
                    OnPropertyChanged(nameof(BorderThicknessVal));
                }
            }
        }

        public Brush BorderBrushColor => IsSelected ? Brushes.Yellow : (Brush)new BrushConverter().ConvertFromString("#5F8F93")!;
        public Thickness BorderThicknessVal => IsSelected ? new Thickness(3) : new Thickness(1.5);
        public Brush CardBackgroundBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#134E54"));

        public string TongCongDisplay => TongCong.ToString("N0");
        public string SoPhieuDisplay => !string.IsNullOrWhiteSpace(SoHD) ? SoHD : (!string.IsNullOrWhiteSpace(SoPhieu) ? SoPhieu : "");
    }

    public partial class TouchThongKeWindow : Window
    {
        private int _rowsConfig = 2;
        private DateTime _selectedDate = DateTime.Today;
        private List<PaidInvoiceItemVM> _paidInvoiceList = new();
        private PaidInvoiceItemVM? _selectedInvoiceItem = null;

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(TouchThongKeWindow), new PropertyMetadata(2));

        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        public static readonly DependencyProperty CardHeightProperty =
            DependencyProperty.Register(nameof(CardHeight), typeof(double), typeof(TouchThongKeWindow), new PropertyMetadata(120.0));

        public double CardHeight
        {
            get => (double)GetValue(CardHeightProperty);
            set => SetValue(CardHeightProperty, value);
        }

        public TouchThongKeWindow()
        {
            InitializeComponent();
            BtnDate.Content = _selectedDate.ToString("dd/MM/yyyy");
            UpdateTabStyles("HoaDon");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                DateTime selectedDate = _selectedDate;

                // Load Settings layout config
                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLUMNS_ThongKeHoaDon", "");
                string savedLayoutStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_ThongKeHoaDon", "2|2|#134E54|1");
                if (string.IsNullOrWhiteSpace(savedLayoutStr) || savedLayoutStr == "2|2|#134E54|1")
                {
                    savedLayoutStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_HoaDonChuaThanhToan", "2|2|#134E54|1");
                }

                string[] layoutParts = savedLayoutStr.Split('|');
                int columns = (layoutParts.Length > 0 && int.TryParse(layoutParts[0], out int cols) && cols >= 1) ? cols : 2;
                int rows = (layoutParts.Length > 1 && int.TryParse(layoutParts[1], out int rws) && rws >= 1) ? rws : 2;
                string colorHex = (layoutParts.Length > 2 && !string.IsNullOrWhiteSpace(layoutParts[2])) ? layoutParts[2] : "#134E54";

                ColumnCount = Math.Max(1, columns);
                _rowsConfig = Math.Max(1, rows);

                Brush cardBgBrush;
                try
                {
                    cardBgBrush = (Brush)new BrushConverter().ConvertFromString(colorHex)!;
                }
                catch
                {
                    cardBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#134E54"));
                }

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                // 1. Fetch Paid Orders for the selected date
                string sqlPaidOrders = @"
                    SELECT 
                        CAST(h.ID AS VARCHAR(50)) as OrderId, 
                        CAST(h.DBANID AS VARCHAR(50)) as BanId,
                        COALESCE(b.NAME, 'Bàn') as BanName, 
                        COALESCE(h.NAME, '') as SoPhieu,
                        COALESCE(h.SOHD, '') as SoHD,
                        COALESCE(h.TONGCONG, 0) as TongCong,
                        COALESCE(h.TIENMAT, 0) as TienMat,
                        COALESCE(h.TIENCHUYENKHOAN, 0) as ChuyenKhoan,
                        COALESCE(h.TIENTHE, 0) as TienThe,
                        COALESCE(h.TIENCONGNO, 0) as CongNo,
                        COALESCE(h.TIENVOUCHER, 0) as Voucher,
                        COALESCE(h.TIENTICHLUY, 0) as TruTichLuy,
                        COALESCE(h.TIENTHETRATRUOC, 0) as TheTraTruoc
                    FROM TDONHANG h 
                    LEFT JOIN DBAN b ON CAST(h.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50)) 
                    WHERE (CAST(h.NGAY AS DATE) = CAST(@DateVal AS DATE) 
                        OR (h.NGAY IS NULL AND CAST(h.TIMECREATED AS DATE) = CAST(@DateVal AS DATE)))
                      AND (h.STATUS = 2 OR h.STATUS = 10 OR h.STATUS IS NULL OR h.NAME IS NOT NULL)
                    ORDER BY h.TIMECREATED DESC, h.NAME ASC";

                _paidInvoiceList = (await conn.QueryAsync<PaidInvoiceItemVM>(sqlPaidOrders, new { DateVal = selectedDate.Date })).ToList();
                foreach (var item in _paidInvoiceList)
                {
                    item.CardBackgroundBrush = cardBgBrush;
                    if (string.IsNullOrWhiteSpace(item.BanName) && !string.IsNullOrWhiteSpace(item.BanId))
                    {
                        item.BanName = $"Bàn {item.BanId}";
                    }
                }

                IcHoaDonCards.ItemsSource = _paidInvoiceList;
                UpdateCalculatedCardHeights();

                // Calculate Totals for metrics panel
                decimal totalCash = _paidInvoiceList.Sum(x => x.TienMat > 0 ? x.TienMat : (x.ChuyenKhoan == 0 && x.TienThe == 0 && x.CongNo == 0 ? x.TongCong : 0));
                decimal totalBank = _paidInvoiceList.Sum(x => x.ChuyenKhoan);
                decimal totalATM = _paidInvoiceList.Sum(x => x.TienThe);
                decimal totalVoucher = _paidInvoiceList.Sum(x => x.Voucher);
                decimal totalPoints = _paidInvoiceList.Sum(x => x.TruTichLuy);
                decimal totalPrepaid = _paidInvoiceList.Sum(x => x.TheTraTruoc);
                decimal totalDebt = _paidInvoiceList.Sum(x => x.CongNo);
                decimal grandTotal = _paidInvoiceList.Sum(x => x.TongCong);

                TxtTienMat.Text = totalCash.ToString("N0");
                TxtChuyenKhoan.Text = totalBank.ToString("N0");
                TxtATM.Text = totalATM.ToString("N0");
                TxtVoucher.Text = totalVoucher.ToString("N0");
                TxtTruTichLuy.Text = totalPoints.ToString("N0");
                TxtTheTraTruoc.Text = totalPrepaid.ToString("N0");
                TxtCongNo.Text = totalDebt.ToString("N0");
                TxtThuChi.Text = "0";
                TxtTong.Text = grandTotal.ToString("N0");

                // 2. Fetch Thu Chi list for tab
                try
                {
                    string sqlThuChi = @"
                        SELECT 
                            NAME as SoPhieu, 
                            TENDOI TUONG as TenDoiTuong, 
                            DIENGIAI as DienGiai, 
                            COALESCE(THU, 0) as Thu, 
                            COALESCE(CHI, 0) as Chi 
                        FROM TTHUCHI 
                        WHERE CAST(NGAY AS DATE) = CAST(@DateVal AS DATE)
                        ORDER BY TIMECREATED DESC";
                    var thuChiList = (await conn.QueryAsync(sqlThuChi, new { DateVal = selectedDate.Date })).ToList();
                    DgThuChi.ItemsSource = thuChiList;
                }
                catch { }

                // 3. Fetch Sold Items list for tab
                try
                {
                    string sqlMatHang = @"
                        SELECT 
                            c.TENHANG as TenHang, 
                            dvt.NAME as DonViTinh, 
                            SUM(COALESCE(c.SLXUAT, 0)) as SoLuong, 
                            c.DONGIA as DonGia, 
                            SUM(COALESCE(c.THANHTIEN, 0)) as ThanhTien 
                        FROM TDONHANGCHITIET c 
                        JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50)) 
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50)) 
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50)) 
                        WHERE (CAST(h.NGAY AS DATE) = CAST(@DateVal AS DATE) 
                            OR (h.NGAY IS NULL AND CAST(h.TIMECREATED AS DATE) = CAST(@DateVal AS DATE))) 
                        GROUP BY c.TENHANG, dvt.NAME, c.DONGIA 
                        ORDER BY c.TENHANG ASC";
                    var matHangList = (await conn.QueryAsync(sqlMatHang, new { DateVal = selectedDate.Date })).ToList();
                    DgMatHang.ItemsSource = matHangList;
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadDataAsync TouchThongKeWindow: {ex}");
            }
        }

        private void UpdateCalculatedCardHeights()
        {
            try
            {
                double containerHeight = SvHoaDonCards != null && SvHoaDonCards.ActualHeight > 50 ? SvHoaDonCards.ActualHeight : 500;
                int rows = _rowsConfig > 0 ? _rowsConfig : 2;
                CardHeight = Math.Max(70.0, Math.Floor((containerHeight - (rows * 12.0)) / rows));
            }
            catch { }
        }

        private void SvHoaDonCards_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCalculatedCardHeights();
        }

        private async void BtnDate_Click(object sender, RoutedEventArgs e)
        {
            var newDate = TouchDatePickerWindow.SelectDate(this, _selectedDate);
            if (newDate.HasValue)
            {
                _selectedDate = newDate.Value;
                BtnDate.Content = _selectedDate.ToString("dd/MM/yyyy");
                await LoadDataAsync();
            }
        }

        private void UpdateTabStyles(string activeTab)
        {
            var blueBrush = (Brush)new BrushConverter().ConvertFromString("#0088FF")!;
            var greyBrush = (Brush)new BrushConverter().ConvertFromString("#5C6B73")!;

            BtnTabHoaDon.Background = activeTab == "HoaDon" ? blueBrush : greyBrush;
            BtnTabThuChi.Background = activeTab == "ThuChi" ? blueBrush : greyBrush;
            BtnTabMatHang.Background = activeTab == "MatHang" ? blueBrush : greyBrush;

            BtnTabHoaDon.Foreground = Brushes.White;
            BtnTabThuChi.Foreground = Brushes.White;
            BtnTabMatHang.Foreground = Brushes.White;

            GridTabHoaDon.Visibility = activeTab == "HoaDon" ? Visibility.Visible : Visibility.Collapsed;
            GridTabThuChi.Visibility = activeTab == "ThuChi" ? Visibility.Visible : Visibility.Collapsed;
            GridTabMatHang.Visibility = activeTab == "MatHang" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnTabHoaDon_Click(object sender, RoutedEventArgs e)
        {
            UpdateTabStyles("HoaDon");
        }

        private void BtnTabThuChi_Click(object sender, RoutedEventArgs e)
        {
            UpdateTabStyles("ThuChi");
        }

        private void BtnTabMatHang_Click(object sender, RoutedEventArgs e)
        {
            UpdateTabStyles("MatHang");
        }

        private void InvoiceCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is PaidInvoiceItemVM item)
            {
                foreach (var x in _paidInvoiceList)
                {
                    x.IsSelected = false;
                }
                item.IsSelected = true;
                _selectedInvoiceItem = item;
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThietLapDinhDangThanhPhanWindow("ThongKeHoaDon");
            win.Owner = this;
            win.ShowDialog();
            _ = LoadDataAsync();
        }

        private void BtnInBaoCaoKetCa_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang gửi lệnh in báo cáo kết ca...", "In báo cáo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnInLai_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInvoiceItem == null)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để in lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show($"Đang in lại hóa đơn [{_selectedInvoiceItem.SoPhieuDisplay}] cho {_selectedInvoiceItem.BanName}...", "In lại hóa đơn", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
