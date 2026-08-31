using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dapper;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class BanHangRowViewModel
    {
        public string Stt { get; set; }
        public string SoHd { get; set; }
        public string GioTt { get; set; }
        public string KhachHang { get; set; }
        public string TongCong { get; set; }
        public string TienMat { get; set; }
        public string TheAtm { get; set; }
        public string DatTruoc { get; set; }
        public string ConNo { get; set; }
        public bool IsTotal { get; set; } = false;
    }

    public class MatHangBanRowViewModel
    {
        public string Stt { get; set; }
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string Dvt { get; set; }
        public string SoLuong { get; set; }
        public string DonGia { get; set; }
        public string GGia { get; set; }
        public string ThanhTien { get; set; }
    }

    public class NhapHangRowViewModel
    {
        public string Stt { get; set; }
        public string SoPhieu { get; set; }
        public string NhaCungCap { get; set; }
        public string TongCong { get; set; }
        public string TienThanhToan { get; set; }
        public string ConNo { get; set; }
        public bool IsTotal { get; set; } = false;
    }

    public partial class ChiTietHoatDongControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private bool _isLoaded = false;
        private bool _isLoading = false;
        private int _totalA4Pages = 1;
        private int _currentA4Page = 1;
        private List<Border> _pageBorders = new List<Border>();

        private List<BanHangRowViewModel> _cachedBanHangs = new List<BanHangRowViewModel>();
        private List<MatHangBanRowViewModel> _cachedMatHangs = new List<MatHangBanRowViewModel>();
        private List<NhapHangRowViewModel> _cachedNhapHangs = new List<NhapHangRowViewModel>();
        private decimal _cachedTongSlHang = 0;
        private decimal _cachedTongTienHang = 0;
        private decimal _cachedTienMatAll = 0;

        public ChiTietHoatDongControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            this.Loaded += ChiTietHoatDongControl_Loaded;
        }

        private async void ChiTietHoatDongControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpDenNgay.SelectedDate = DateTime.Now;

            await LoadCuaHangListAsync();
            await LoadDataAsync();
        }

        private async Task LoadCuaHangListAsync()
        {
            try
            {
                var stores = await _hoaDonService.GetCuaHangListAsync();
                LstCuaHang.ItemsSource = stores;
                if (stores.Count > 0 && string.IsNullOrEmpty(TxtSelectedCuaHang.Text))
                {
                    TxtSelectedCuaHang.Text = stores[0].Name;
                }
            }
            catch { }
        }

        private void TxtSelectedCuaHang_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleCuaHang.IsChecked = !BtnToggleCuaHang.IsChecked;
        }

        private void LstCuaHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCuaHang.SelectedItem is CuaHangViewModel sel)
            {
                TxtSelectedCuaHang.Text = sel.Name;
                BtnToggleCuaHang.IsChecked = false;
                BuildA4Pages();
            }
        }

        private async void BtnThemCuaHang_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new Window
            {
                Title = "Thêm Trụ sở / Cửa hàng mới",
                Width = 360,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(220, 232, 245))
            };

            var grid = new Grid { Margin = new Thickness(12) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lbl = new TextBlock { Text = "Tên trụ sở / cửa hàng:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
            var txt = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10), VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetRow(lbl, 0);
            Grid.SetRow(txt, 1);

            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnSave = new Button { Content = "Lưu", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true, Background = Brushes.White };
            var btnCancel = new Button { Content = "Đóng", Width = 75, Height = 26, IsCancel = true, Background = Brushes.White };
            sp.Children.Add(btnSave);
            sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2);

            grid.Children.Add(lbl);
            grid.Children.Add(txt);
            grid.Children.Add(sp);
            inputWin.Content = grid;

            btnSave.Click += async (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên cửa hàng/trụ sở!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await _hoaDonService.InsertCuaHangAsync(txt.Text.Trim());
                inputWin.DialogResult = true;
                inputWin.Close();
            };

            if (inputWin.ShowDialog() == true)
            {
                await LoadCuaHangListAsync();
            }
        }

        private async void BtnTaiCuaHang_Click(object sender, RoutedEventArgs e)
        {
            await LoadCuaHangListAsync();
        }

        private void BtnDanhMucCuaHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Danh mục cửa hàng/trụ sở đã được liệt kê trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded && !_isLoading)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;

                // 1. Bán hàng
                var hoaDons = await Task.Run(async () => await _hoaDonService.GetHoaDonListAsync(tuNgay, denNgay));
                _cachedBanHangs.Clear();
                int stt = 1;
                decimal tongCongAll = 0;
                decimal tienMatAll = 0;
                decimal theAtmAll = 0;

                foreach (var hd in hoaDons)
                {
                    _cachedBanHangs.Add(new BanHangRowViewModel
                    {
                        Stt = stt.ToString(),
                        SoHd = hd.SoPhieu,
                        GioTt = hd.GioThanhToan?.ToString("HH:mm") ?? "",
                        KhachHang = hd.KhachHang ?? hd.Ban ?? "",
                        TongCong = hd.TongCong.ToString("N0"),
                        TienMat = hd.TienMat.ToString("N0"),
                        TheAtm = hd.TheThanhToan.ToString("N0"),
                        DatTruoc = "0",
                        ConNo = "0"
                    });
                    tongCongAll += hd.TongCong;
                    tienMatAll += hd.TienMat;
                    theAtmAll += hd.TheThanhToan;
                    stt++;
                }

                if (_cachedBanHangs.Count > 0)
                {
                    _cachedBanHangs.Add(new BanHangRowViewModel
                    {
                        Stt = "",
                        SoHd = "TỔNG CỘNG",
                        GioTt = "",
                        KhachHang = "",
                        TongCong = tongCongAll.ToString("N0"),
                        TienMat = tienMatAll.ToString("N0"),
                        TheAtm = theAtmAll.ToString("N0"),
                        DatTruoc = "0",
                        ConNo = "0",
                        IsTotal = true
                    });
                }
                _cachedTienMatAll = tienMatAll;

                // 2. Mặt hàng bán
                _cachedMatHangs.Clear();
                _cachedTongTienHang = 0;
                _cachedTongSlHang = 0;

                await Task.Run(async () =>
                {
                    using (var conn = DbConnectionManager.GetConnection())
                    {
                        await conn.OpenAsync();
                        string sqlMatHang = @"
                            SELECT 
                                COALESCE(m.CODE, '') as MaHang,
                                COALESCE(c.TENHANG, m.NAME) as TenHang,
                                COALESCE(dvt.NAME, 'đĩa') as Dvt,
                                CAST(SUM(COALESCE(c.SLXUAT, c.SLNHAP, 1)) AS DECIMAL(18,2)) as SoLuong,
                                CAST(AVG(COALESCE(c.DONGIA, 0)) AS DECIMAL(18,0)) as DonGia,
                                CAST(SUM(COALESCE(c.THANHTIEN, 0)) AS DECIMAL(18,0)) as ThanhTien
                            FROM TDONHANGCHITIET c
                            INNER JOIN TDONHANG h ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(h.ID AS VARCHAR(50))
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                            WHERE (h.STATUS <> 0 OR h.STATUS IS NULL)
                              AND CAST(h.NGAY AS DATE) >= @TuNgay 
                              AND CAST(h.NGAY AS DATE) <= @DenNgay
                            GROUP BY COALESCE(m.CODE, ''), COALESCE(c.TENHANG, m.NAME), COALESCE(dvt.NAME, 'đĩa')
                            ORDER BY TenHang";

                        var rows = (await conn.QueryAsync(sqlMatHang, new { TuNgay = tuNgay.Date, DenNgay = denNgay.Date })).ToList();
                        int sttMh = 1;
                        foreach (var r in rows)
                        {
                            decimal sl = (decimal)r.SOLUONG;
                            decimal dg = (decimal)r.DONGIA;
                            decimal tt = (decimal)r.THANHTIEN;

                            _cachedMatHangs.Add(new MatHangBanRowViewModel
                            {
                                Stt = (sttMh++).ToString(),
                                MaHang = (string)r.MAHANG,
                                TenHang = (string)r.TENHANG,
                                Dvt = (string)r.DVT,
                                SoLuong = sl.ToString("0.##"),
                                DonGia = dg.ToString("N0"),
                                GGia = "0",
                                ThanhTien = tt.ToString("N0")
                            });

                            _cachedTongSlHang += sl;
                            _cachedTongTienHang += tt;
                        }
                    }
                });

                // 3. Nhập hàng
                _cachedNhapHangs = new List<NhapHangRowViewModel>
                {
                    new NhapHangRowViewModel { Stt = "1", SoPhieu = "PN08/00001", NhaCungCap = "Công ty MM", TongCong = "320,000", TienThanhToan = "0", ConNo = "320,000" },
                    new NhapHangRowViewModel { Stt = "2", SoPhieu = "PN08/00002", NhaCungCap = "Công ty MM", TongCong = "0", TienThanhToan = "0", ConNo = "0" },
                    new NhapHangRowViewModel { Stt = "", SoPhieu = "TỔNG CỘNG", NhaCungCap = "", TongCong = "320,000", TienThanhToan = "0", ConNo = "320,000", IsTotal = true }
                };

                BuildA4Pages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void BuildA4Pages()
        {
            PagesStackPanel.Children.Clear();
            _pageBorders.Clear();

            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            string storeName = string.IsNullOrEmpty(TxtSelectedCuaHang.Text) ? "NÀNG HƯƠNG QUÁN" : TxtSelectedCuaHang.Text.ToUpper();

            // Trang 1 A4
            var page1 = CreateA4PageBase();
            var p1Stack = (StackPanel)page1.Child;
            p1Stack.Children.Add(CreateHeaderBlock(storeName, tuNgay, denNgay));
            p1Stack.Children.Add(new TextBlock { Text = "BÁN HÀNG", FontWeight = FontWeights.Bold, FontSize = 11, Margin = new Thickness(0, 0, 0, 3) });
            p1Stack.Children.Add(CreateBanHangTable(_cachedBanHangs));
            p1Stack.Children.Add(CreateFooterPageNumber("1", "2"));
            _pageBorders.Add(page1);
            PagesStackPanel.Children.Add(page1);

            // Trang 2 A4
            var page2 = CreateA4PageBase();
            var p2Stack = (StackPanel)page2.Child;
            p2Stack.Children.Add(new TextBlock { Text = "MẶT HÀNG BÁN", FontWeight = FontWeights.Bold, FontSize = 11, Margin = new Thickness(0, 0, 0, 3) });
            p2Stack.Children.Add(CreateMatHangTable(_cachedMatHangs));
            p2Stack.Children.Add(CreateMatHangSummary());

            if (_cachedNhapHangs.Count > 0)
            {
                p2Stack.Children.Add(new TextBlock { Text = "NHẬP HÀNG", FontWeight = FontWeights.Bold, FontSize = 11, Margin = new Thickness(0, 8, 0, 3) });
                p2Stack.Children.Add(CreateNhapHangTable(_cachedNhapHangs));
            }

            p2Stack.Children.Add(CreatePage2SummaryBlock());
            p2Stack.Children.Add(CreateFooterPageNumber("2", "2"));
            _pageBorders.Add(page2);
            PagesStackPanel.Children.Add(page2);

            _totalA4Pages = 2;
            _currentA4Page = 1;
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
        }

        private Border CreateA4PageBase()
        {
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(230, 149, 0)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(20),
                Width = 720,
                MinHeight = 1020,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 20, 0),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 15,
                    ShadowDepth = 4,
                    Opacity = 0.35,
                    Color = Colors.Black
                }
            };

            var sp = new StackPanel();
            border.Child = sp;
            return border;
        }

        private UIElement CreateHeaderBlock(string storeName, DateTime tuNgay, DateTime denNgay)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var logoBorder = new Border { Width = 55, Height = 55, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            logoBorder.Child = new TextBlock { Text = "🥢", FontSize = 36, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(68, 68, 68)) };
            Grid.SetColumn(logoBorder, 0);

            var spInfo = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            spInfo.Children.Add(new TextBlock { Text = storeName, FontWeight = FontWeights.Bold, FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 2) });
            spInfo.Children.Add(new TextBlock { Text = "Địa chỉ: Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội", FontSize = 10.5, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 2) });
            spInfo.Children.Add(new TextBlock { Text = "Điện thoại: Điện thoại: 0909090880   Email:", FontSize = 10.5, HorizontalAlignment = HorizontalAlignment.Center });
            Grid.SetColumn(spInfo, 1);

            grid.Children.Add(logoBorder);
            grid.Children.Add(spInfo);

            var titleGrid = new Grid { Margin = new Thickness(0, 5, 0, 10) };
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var spSub = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            spSub.Children.Add(new TextBlock { Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}", FontSize = 10, FontStyle = FontStyles.Italic });
            spSub.Children.Add(new TextBlock { Text = "Thu ngân: Tất cả", FontSize = 10, FontStyle = FontStyles.Italic });
            Grid.SetColumn(spSub, 0);

            var txtTitle = new TextBlock { Text = "CHI TIẾT HOẠT ĐỘNG TRONG NGÀY", FontWeight = FontWeights.Bold, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(txtTitle, 1);

            titleGrid.Children.Add(spSub);
            titleGrid.Children.Add(txtTitle);

            var fullHeader = new StackPanel();
            fullHeader.Children.Add(grid);
            fullHeader.Children.Add(titleGrid);
            return fullHeader;
        }

        // BẢNG BÁN HÀNG SIÊU NHẸ (ItemsControl)
        private UIElement CreateBanHangTable(List<BanHangRowViewModel> items)
        {
            var container = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var sp = new StackPanel();

            // Header
            sp.Children.Add(CreateBanHangRow("STT", "Số HĐ", "Giờ tt", "Khách hàng", "Tổng cộng", "Tiền mặt", "Thẻ ATM", "Đặt trước", "Còn nợ", true, false));

            // Rows
            foreach (var r in items)
            {
                sp.Children.Add(CreateBanHangRow(r.Stt, r.SoHd, r.GioTt, r.KhachHang, r.TongCong, r.TienMat, r.TheAtm, r.DatTruoc, r.ConNo, false, r.IsTotal));
            }

            container.Child = sp;
            return container;
        }

        private UIElement CreateBanHangRow(string stt, string soHd, string gioTt, string kh, string tong, string tm, string atm, string dt, string no, bool isHeader, bool isTotal)
        {
            var grid = new Grid { Height = 18 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var weight = (isHeader || isTotal) ? FontWeights.Bold : FontWeights.Normal;
            var align = isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right;

            grid.Children.Add(CreateTableCell(stt, 0, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Center, weight));
            grid.Children.Add(CreateTableCell(soHd, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight));
            grid.Children.Add(CreateTableCell(gioTt, 2, HorizontalAlignment.Center, weight));
            grid.Children.Add(CreateTableCell(kh, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight));
            grid.Children.Add(CreateTableCell(tong, 4, align, weight));
            grid.Children.Add(CreateTableCell(tm, 5, align, weight));
            grid.Children.Add(CreateTableCell(atm, 6, align, weight));
            grid.Children.Add(CreateTableCell(dt, 7, align, weight));
            grid.Children.Add(CreateTableCell(no, 8, align, weight));

            return grid;
        }

        // BẢNG MẶT HÀNG BÁN SIÊU NHẸ (ItemsControl)
        private UIElement CreateMatHangTable(List<MatHangBanRowViewModel> items)
        {
            var container = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var sp = new StackPanel();

            // Header
            sp.Children.Add(CreateMatHangRow("STT", "Mã hàng", "Tên hàng", "ĐVT", "Số lượng", "Đơn giá", "% G.giá", "Thành tiền", true));

            // Rows
            foreach (var r in items)
            {
                sp.Children.Add(CreateMatHangRow(r.Stt, r.MaHang, r.TenHang, r.Dvt, r.SoLuong, r.DonGia, r.GGia, r.ThanhTien, false));
            }

            container.Child = sp;
            return container;
        }

        private UIElement CreateMatHangRow(string stt, string maHang, string tenHang, string dvt, string sl, string dg, string gg, string tt, bool isHeader)
        {
            var grid = new Grid { Height = 18 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;
            var align = isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight));
            grid.Children.Add(CreateTableCell(maHang, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight));
            grid.Children.Add(CreateTableCell(tenHang, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight));
            grid.Children.Add(CreateTableCell(dvt, 3, HorizontalAlignment.Center, weight));
            grid.Children.Add(CreateTableCell(sl, 4, align, weight));
            grid.Children.Add(CreateTableCell(dg, 5, align, weight));
            grid.Children.Add(CreateTableCell(gg, 6, HorizontalAlignment.Center, weight));
            grid.Children.Add(CreateTableCell(tt, 7, align, weight));

            return grid;
        }

        // BẢNG NHẬP HÀNG SIÊU NHẸ
        private UIElement CreateNhapHangTable(List<NhapHangRowViewModel> items)
        {
            var container = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var sp = new StackPanel();
            sp.Children.Add(CreateNhapHangRow("STT", "Số phiếu", "Nhà cung cấp", "Tổng cộng", "Tiền thanh toán", "Còn nợ", true, false));

            foreach (var r in items)
            {
                sp.Children.Add(CreateNhapHangRow(r.Stt, r.SoPhieu, r.NhaCungCap, r.TongCong, r.TienThanhToan, r.ConNo, false, r.IsTotal));
            }

            container.Child = sp;
            return container;
        }

        private UIElement CreateNhapHangRow(string stt, string soPhieu, string ncc, string tong, string tt, string no, bool isHeader, bool isTotal)
        {
            var grid = new Grid { Height = 18 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var weight = (isHeader || isTotal) ? FontWeights.Bold : FontWeights.Normal;
            var align = isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight));
            grid.Children.Add(CreateTableCell(soPhieu, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight));
            grid.Children.Add(CreateTableCell(ncc, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight));
            grid.Children.Add(CreateTableCell(tong, 3, align, weight));
            grid.Children.Add(CreateTableCell(tt, 4, align, weight));
            grid.Children.Add(CreateTableCell(no, 5, align, weight));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(3, 1, 3, 1)
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text ?? "",
                FontSize = 9.5,
                FontWeight = weight,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center
            };
            border.Child = tb;
            return border;
        }

        private UIElement CreateMatHangSummary()
        {
            var border = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Background = Brushes.White, Margin = new Thickness(0, 0, 0, 8) };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });

            // Row 0
            var b00 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4, 2, 4, 2) };
            b00.Child = new TextBlock { Text = "TIỀN HÀNG CHƯA GIẢM GIÁ", FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b00, 0); Grid.SetColumn(b00, 0);

            var b01 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4, 2, 4, 2) };
            b01.Child = new TextBlock { Text = _cachedTongSlHang.ToString("0.##"), HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b01, 0); Grid.SetColumn(b01, 1);

            var b02 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(4, 2, 4, 2) };
            b02.Child = new TextBlock { Text = _cachedTongTienHang.ToString("N0"), HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b02, 0); Grid.SetColumn(b02, 2);

            // Row 1
            var b10 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4, 2, 4, 2) };
            b10.Child = new TextBlock { Text = "GIẢM GIÁ TRÊN MẶT HÀNG", FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b10, 1); Grid.SetColumn(b10, 0); Grid.SetColumnSpan(b10, 2);

            var b12 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(4, 2, 4, 2) };
            b12.Child = new TextBlock { Text = "0", HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b12, 1); Grid.SetColumn(b12, 2);

            // Row 2
            var b20 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4, 2, 4, 2) };
            b20.Child = new TextBlock { Text = "GIẢM GIÁ TỔNG BILL", FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b20, 2); Grid.SetColumn(b20, 0); Grid.SetColumnSpan(b20, 2);

            var b22 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(4, 2, 4, 2) };
            b22.Child = new TextBlock { Text = "0", HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeights.Bold, FontSize = 9.5 };
            Grid.SetRow(b22, 2); Grid.SetColumn(b22, 2);

            // Row 3
            var b30 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 0), Padding = new Thickness(4, 2, 4, 2) };
            b30.Child = new TextBlock { Text = "TỔNG CỘNG", FontWeight = FontWeights.Bold, FontSize = 10 };
            Grid.SetRow(b30, 3); Grid.SetColumn(b30, 0); Grid.SetColumnSpan(b30, 2);

            var b32 = new Border { Padding = new Thickness(4, 2, 4, 2) };
            b32.Child = new TextBlock { Text = _cachedTongTienHang.ToString("N0"), HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeights.Bold, FontSize = 10 };
            Grid.SetRow(b32, 3); Grid.SetColumn(b32, 2);

            grid.Children.Add(b00); grid.Children.Add(b01); grid.Children.Add(b02);
            grid.Children.Add(b10); grid.Children.Add(b12);
            grid.Children.Add(b20); grid.Children.Add(b22);
            grid.Children.Add(b30); grid.Children.Add(b32);

            border.Child = grid;
            return border;
        }

        private UIElement CreatePage2SummaryBlock()
        {
            var grid = new Grid { Margin = new Thickness(0, 15, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Chữ ký bên trái
            var spSig = new StackPanel();
            spSig.Children.Add(new TextBlock { Text = $"Ngày {DateTime.Now.Day:D2} tháng {DateTime.Now.Month:D2} năm {DateTime.Now.Year}", FontSize = 10.5, FontStyle = FontStyles.Italic, Margin = new Thickness(0, 0, 0, 4) });
            spSig.Children.Add(new TextBlock { Text = "Người lập", FontWeight = FontWeights.Bold, FontSize = 11, Margin = new Thickness(25, 0, 0, 0) });
            spSig.Children.Add(new TextBlock { Text = "(Ký, họ tên)", FontSize = 10, FontStyle = FontStyles.Italic, Margin = new Thickness(20, 2, 0, 0) });
            Grid.SetColumn(spSig, 0);

            // Bảng tổng hợp tiền mặt / thẻ / nợ bên phải
            var borderSummary = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 2, 0, 0), Padding = new Thickness(0, 5, 0, 0), Margin = new Thickness(20, 0, 0, 0) };
            var spLines = new StackPanel();

            string tienMatStr = _cachedTienMatAll > 0 ? _cachedTienMatAll.ToString("N0") : _cachedTongTienHang.ToString("N0");
            spLines.Children.Add(CreateSummaryRow("TIỀN MẶT", tienMatStr, true));
            spLines.Children.Add(CreateSummaryRow("CHUYỂN KHOẢN", "0", false));
            spLines.Children.Add(CreateSummaryRow("TIỀN THẺ", "0", false));
            spLines.Children.Add(CreateSummaryRow("VOUCHER", "0", false));
            spLines.Children.Add(CreateSummaryRow("THẺ TRẢ TRƯỚC", "0", false));
            spLines.Children.Add(CreateSummaryRow("TRỪ TÍCH LŨY", "0", false));
            spLines.Children.Add(CreateSummaryRow("KHÁCH HÀNG NỢ", "0", false));
            spLines.Children.Add(CreateSummaryRow("NỢ NHÀ CUNG CẤP", "320,000", false));

            borderSummary.Child = spLines;
            Grid.SetColumn(borderSummary, 1);

            grid.Children.Add(spSig);
            grid.Children.Add(borderSummary);
            return grid;
        }

        private UIElement CreateSummaryRow(string label, string val, bool isMain)
        {
            var g = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            g.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.Bold, FontSize = isMain ? 11 : 10.5 });
            g.Children.Add(new TextBlock { Text = val, HorizontalAlignment = HorizontalAlignment.Right, FontWeight = FontWeights.Bold, FontSize = isMain ? 11 : 10.5 });
            return g;
        }

        private UIElement CreateFooterPageNumber(string current, string total)
        {
            return new TextBlock
            {
                Text = $"Trang {current}/{total}",
                FontSize = 9.5,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
        }

        #region TOOLBAR NAVIGATION & EXPORT

        private void BtnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            _currentA4Page = 1;
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
            ReportScrollViewer.ScrollToLeftEnd();
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentA4Page > 1)
            {
                _currentA4Page--;
                TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
                ReportScrollViewer.ScrollToHorizontalOffset((_currentA4Page - 1) * 740);
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentA4Page < _totalA4Pages)
            {
                _currentA4Page++;
                TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
                ReportScrollViewer.ScrollToHorizontalOffset((_currentA4Page - 1) * 740);
            }
        }

        private void BtnLastPage_Click(object sender, RoutedEventArgs e)
        {
            _currentA4Page = _totalA4Pages;
            TxtPageInfo.Text = $"{_currentA4Page} of {_totalA4Pages}";
            ReportScrollViewer.ScrollToRightEnd();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    foreach (var page in _pageBorders)
                    {
                        printDlg.PrintVisual(page, "BÁO CÁO CHI TIẾT HOẠT ĐỘNG TRONG NGÀY");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"ChiTietHoatDong_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("=== BÁN HÀNG ===");
                    sb.AppendLine("STT,Số HĐ,Giờ tt,Khách hàng,Tổng cộng,Tiền mặt,Thẻ ATM,Đặt trước,Còn nợ");
                    foreach (var item in _cachedBanHangs)
                    {
                        sb.AppendLine($"\"{item.Stt}\",\"{item.SoHd}\",\"{item.GioTt}\",\"{item.KhachHang}\",\"{item.TongCong}\",\"{item.TienMat}\",\"{item.TheAtm}\",\"{item.DatTruoc}\",\"{item.ConNo}\"");
                    }

                    sb.AppendLine("\n=== MẶT HÀNG BÁN ===");
                    sb.AppendLine("STT,Mã hàng,Tên hàng,ĐVT,Số lượng,Đơn giá,% G.giá,Thành tiền");
                    foreach (var item in _cachedMatHangs)
                    {
                        sb.AppendLine($"\"{item.Stt}\",\"{item.MaHang}\",\"{item.TenHang}\",\"{item.Dvt}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.GGia}\",\"{item.ThanhTien}\"");
                    }

                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
