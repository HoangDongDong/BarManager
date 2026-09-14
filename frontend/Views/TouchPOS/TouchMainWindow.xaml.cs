using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dapper;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Views.CauHinhHeThong;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchMainWindow : Window
    {
        private DBAN? _currentBan;

        public TouchMainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.Left = 0;
                this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.WindowState = WindowState.Maximized;
            }
            catch { }

            ShowLoginScreen();
        }

        private void ShowLoginScreen()
        {
            GridTouchLogin.Visibility = Visibility.Visible;
            GridTouchMenu.Visibility = Visibility.Collapsed;
            GridTouchTables.Visibility = Visibility.Collapsed;
            GridTouchOrder.Visibility = Visibility.Collapsed;
        }

        private void ShowMenuScreen()
        {
            GridTouchLogin.Visibility = Visibility.Collapsed;
            GridTouchMenu.Visibility = Visibility.Visible;
            GridTouchTables.Visibility = Visibility.Collapsed;
            GridTouchOrder.Visibility = Visibility.Collapsed;

            try
            {
                TxtMenuHeaderTitle.Text = "PHẦN MỀM QUẢN LÝ BAR, NHÀ HÀNG V6.0 TÂN AN PHÁT - HOTLINE: 0967041111";
                TxtFooterUser.Text = SessionContext.CurrentUser?.TenHienThi ?? "Administrator";
                TxtFooterDb.Text = "DEMO";
                SelectMenuCategory(null);
            }
            catch { }
        }

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(TouchMainWindow), new PropertyMetadata(1));

        public static readonly DependencyProperty TileHeightProperty =
            DependencyProperty.Register(nameof(TileHeight), typeof(double), typeof(TouchMainWindow), new PropertyMetadata(85.0));

        public static readonly DependencyProperty MatHangColumnsProperty =
            DependencyProperty.Register(nameof(MatHangColumns), typeof(int), typeof(TouchMainWindow), new PropertyMetadata(4));

        public static readonly DependencyProperty MatHangTileHeightProperty =
            DependencyProperty.Register(nameof(MatHangTileHeight), typeof(double), typeof(TouchMainWindow), new PropertyMetadata(95.0));

        public static readonly DependencyProperty NhomColumnsProperty =
            DependencyProperty.Register(nameof(NhomColumns), typeof(int), typeof(TouchMainWindow), new PropertyMetadata(2));

        public static readonly DependencyProperty NhomTileMinHeightProperty =
            DependencyProperty.Register(nameof(NhomTileMinHeight), typeof(double), typeof(TouchMainWindow), new PropertyMetadata(140.0));

        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        public double TileHeight
        {
            get => (double)GetValue(TileHeightProperty);
            set => SetValue(TileHeightProperty, value);
        }

        public int MatHangColumns
        {
            get => (int)GetValue(MatHangColumnsProperty);
            set => SetValue(MatHangColumnsProperty, value);
        }

        public double MatHangTileHeight
        {
            get => (double)GetValue(MatHangTileHeightProperty);
            set => SetValue(MatHangTileHeightProperty, value);
        }

        public int NhomColumns
        {
            get => (int)GetValue(NhomColumnsProperty);
            set => SetValue(NhomColumnsProperty, value);
        }

        public double NhomTileMinHeight
        {
            get => (double)GetValue(NhomTileMinHeightProperty);
            set => SetValue(NhomTileMinHeightProperty, value);
        }

        private int _tableRowsConfig = 5;
        private int _matHangRowsConfig = 4;
        private int _nhomRowsConfig = 2;
        private string _matHangTileColorHex = "#EF4423";
        private string _nhomTileColorHex = "#FFFF33";

        private bool _coThueSuat = false;
        private decimal _macDinhThueSuat = 0;
        private bool _coPhiDichVu = false;
        private decimal _macDinhPhiDichVu = 0;
        private decimal _lamTronTien = 0;

        private async Task RefreshSystemConfigsAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                _coThueSuat = configs.TryGetValue("CoThueSuat", out var cts) && (cts == "1" || cts.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (configs.TryGetValue("MacDinhThueSuat", out var mdts) && decimal.TryParse(mdts.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var thueVal))
                {
                    _macDinhThueSuat = thueVal;
                }
                else _macDinhThueSuat = 0;

                _coPhiDichVu = configs.TryGetValue("CoPhiDichVu", out var cpdv) && (cpdv == "1" || cpdv.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (configs.TryGetValue("MacDinhPhiDichVu", out var mdpdv) && decimal.TryParse(mdpdv.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var phiVal))
                {
                    _macDinhPhiDichVu = phiVal;
                }
                else _macDinhPhiDichVu = 0;

                if (configs.TryGetValue("LamTronTien", out var lt) && decimal.TryParse(lt.Replace(",", "").Replace(".", "").Trim(), out var ltVal) && ltVal > 0)
                {
                    _lamTronTien = ltVal;
                }
                else
                {
                    _lamTronTien = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi RefreshSystemConfigsAsync: {ex.Message}");
            }
        }

        private async Task LoadTableLayoutConfigAsync()
        {
            try
            {
                string value = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_Ban", "1|5|#808080|1");
                string[] parts = value.Split('|');
                int cols = 1;
                int rows = 5;
                if (parts.Length > 0 && int.TryParse(parts[0], out int c) && c >= 1) cols = c;
                if (parts.Length > 1 && int.TryParse(parts[1], out int r) && r >= 1) rows = r;

                _tableRowsConfig = rows;
                ColumnCount = cols;
                UpdateTileHeight();

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLUMNS_Ban", "BÀN");
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    DBAN.SavedDisplayColumns = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    DBAN.SavedDisplayColumns = new List<string> { "BÀN" };
                }
            }
            catch { }
        }

        private async Task LoadOrderLayoutConfigAsync()
        {
            try
            {
                // 1. MatHang layout config
                string matHangVal = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_MatHang", "8|4|#EF4423|1");
                string[] mParts = matHangVal.Split('|');
                int mCols = 8, mRows = 4;
                string mColor = "#EF4423";
                if (mParts.Length > 0 && int.TryParse(mParts[0], out int mc) && mc >= 1) mCols = mc;
                if (mParts.Length > 1 && int.TryParse(mParts[1], out int mr) && mr >= 1) mRows = mr;
                if (mParts.Length > 2 && !string.IsNullOrWhiteSpace(mParts[2])) mColor = mParts[2].Trim();

                MatHangColumns = mCols;
                _matHangRowsConfig = mRows;
                _matHangTileColorHex = mColor;
                UpdateMatHangTileHeight();

                string savedColsStr = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLUMNS_MatHang", "");
                if (!string.IsNullOrWhiteSpace(savedColsStr))
                {
                    TouchMatHangVM.SavedDisplayColumns = savedColsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
                else
                {
                    TouchMatHangVM.SavedDisplayColumns = new List<string>();
                }

                if (_allMatHangList != null)
                {
                    foreach (var item in _allMatHangList)
                    {
                        if (string.IsNullOrWhiteSpace(item.MauSac) || item.MauSac == "0")
                        {
                            item.TileColorHex = _matHangTileColorHex;
                        }
                        else
                        {
                            item.TileColorHex = QuanLyBar.Client.Services.ColorUtils.ColorIntToHex(item.MauSac, _matHangTileColorHex);
                        }
                        item.RefreshDisplayLines();
                    }
                }
                if (_selectedNhom != null)
                {
                    SelectCategory(_selectedNhom);
                }

                // 2. Nhom layout config
                string nhomVal = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_Nhom", "2|2|#FFFF33|1");
                string[] nParts = nhomVal.Split('|');
                int nCols = 2, nRows = 2;
                string nColor = "#FFFF33";
                if (nParts.Length > 0 && int.TryParse(nParts[0], out int nc) && nc >= 1) nCols = nc;
                if (nParts.Length > 1 && int.TryParse(nParts[1], out int nr) && nr >= 1) nRows = nr;
                if (nParts.Length > 2 && !string.IsNullOrWhiteSpace(nParts[2])) nColor = nParts[2].Trim();

                NhomColumns = nCols;
                _nhomRowsConfig = nRows;
                _nhomTileColorHex = nColor;
                UpdateNhomTileHeight();

                if (_nhomList != null)
                {
                    foreach (var cat in _nhomList)
                    {
                        cat.ColorBrushHex = _nhomTileColorHex;
                        cat.TextColorHex = QuanLyBar.Client.Services.ColorUtils.GetContrastTextColor(_nhomTileColorHex);
                    }
                }
            }
            catch { }
        }

        private void UpdateTileHeight()
        {
            try
            {
                double containerHeight = SvTableTiles != null && SvTableTiles.ActualHeight > 100 ? SvTableTiles.ActualHeight : 600;
                int rows = _tableRowsConfig > 0 ? _tableRowsConfig : 5;
                TileHeight = Math.Max(40, (containerHeight - (rows * 10)) / rows);
            }
            catch { }
        }

        private void UpdateMatHangTileHeight()
        {
            try
            {
                double containerHeight = SvTouchMatHang != null && SvTouchMatHang.ActualHeight > 100 ? SvTouchMatHang.ActualHeight : 550;
                int rows = _matHangRowsConfig > 0 ? _matHangRowsConfig : 4;
                MatHangTileHeight = Math.Max(70, (containerHeight - (rows * 8)) / rows);
            }
            catch { }
        }

        private void UpdateNhomTileHeight()
        {
            try
            {
                double containerHeight = SvTouchNhomHang != null && SvTouchNhomHang.ActualHeight > 100 ? SvTouchNhomHang.ActualHeight : 550;
                int rows = _nhomRowsConfig > 0 ? _nhomRowsConfig : 2;
                NhomTileMinHeight = Math.Max(40, (containerHeight - (rows * 8)) / rows);
            }
            catch { }
        }

        private void SvTouchMatHang_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMatHangTileHeight();
        }

        private void SvTouchNhomHang_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateNhomTileHeight();
        }

        private async void ShowTableScreen()
        {
            GridTouchLogin.Visibility = Visibility.Collapsed;
            GridTouchMenu.Visibility = Visibility.Collapsed;
            GridTouchTables.Visibility = Visibility.Visible;
            GridTouchOrder.Visibility = Visibility.Collapsed;
            await LoadTableLayoutConfigAsync();
            LoadTables();
        }

        public DBAN? CurrentBan => _currentBan;

        public void ReloadCurrentOrder()
        {
            if (_currentBan != null && _currentBan.IsOpened)
            {
                LoadOrderItems();
            }
            else
            {
                ShowTableScreen();
            }
        }

        private async void ShowOrderScreen(DBAN ban)
        {
            _currentBan = ban;
            GridTouchLogin.Visibility = Visibility.Collapsed;
            GridTouchMenu.Visibility = Visibility.Collapsed;
            GridTouchTables.Visibility = Visibility.Collapsed;
            GridTouchOrder.Visibility = Visibility.Visible;

            TxtOrderTableName.Text = ban.TENBAN;
            TxtOrderDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            TxtOrderNo.Text = !string.IsNullOrEmpty(ban.SoPhieu) ? $"HĐ: {ban.SoPhieu}" : "HĐ: ...";
            TxtOrderGuestCount.Text = ban.SoKhach > 0 ? ban.SoKhach.ToString() : "1";
            TxtOrderCustomer.Text = ban.KhachHangName ?? "";

            await RefreshSystemConfigsAsync();
            LoadOrderItems();
            await LoadMenuItems();
            await LoadOrderLayoutConfigAsync();
        }

        private List<DBAN> _allBanList = new();
        private List<DKHUVUC> _allKhuVucList = new();
        private string? _selectedAreaId = null;
        private bool _filterOnlyOpened = false;

        private async void LoadTables()
        {
            try
            {
                TxtCurrentPOSUser.Text = SessionContext.CurrentUser?.TenHienThi ?? "Administrator";

                var service = new LocalSuDungDichVuService();
                var kvBanList = await service.GetKhuVucBanListAsync();

                _allKhuVucList = new List<DKHUVUC>();
                _allBanList = new List<DBAN>();

                string defaultKvColor = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_KhuVuc", "#E65100");
                string[] presetColors = new string[] { "#E65100", "#414BEA", "#00C800", "#00838F", "#C2185B", "#6A1B9A", "#D84315" };
                int colorIndex = 0;

                foreach (var kv in kvBanList)
                {
                    string kvColor = !string.IsNullOrWhiteSpace(kv.MauSac) ? QuanLyBar.Client.Services.ColorUtils.ColorIntToHex(kv.MauSac, defaultKvColor) :
                                     (!string.IsNullOrWhiteSpace(defaultKvColor) ? defaultKvColor : presetColors[colorIndex % presetColors.Length]);

                    _allKhuVucList.Add(new DKHUVUC
                    {
                        Id = kv.Id,
                        MAKHUVUC = kv.Id,
                        Name = kv.Name,
                        TenKhuVuc = kv.Name,
                        ColorHex = kvColor,
                        MAUSAC = kvColor
                    });
                    colorIndex++;

                    foreach (var b in kv.BanList)
                    {
                        _allBanList.Add(new DBAN
                        {
                            Id = b.Id,
                            MABAN = b.Id,
                            Name = b.Name,
                            TENBAN = b.Name,
                            MAKHUVUC = kv.Id,
                            KhuVucName = kv.Name,
                            IsOpened = b.IsOccupied,
                            ActiveOrderId = b.ActiveOrderId,
                            SoPhieu = b.SoPhieu,
                            SoKhach = b.SoKhach > 0 ? b.SoKhach : 1,
                            KhachHangName = b.KhachHangName,
                            ThoiGianMo = b.StartTime,
                            TongCong = b.TongCong,
                            Anh = b.Anh
                        });
                    }
                }

                IcAreaButtons.ItemsSource = _allKhuVucList;
                ApplyTableFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải bàn: {ex.Message}");
            }
        }

        private void ApplyTableFilters()
        {
            var query = _allBanList.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedAreaId))
            {
                query = query.Where(x => x.MAKHUVUC == _selectedAreaId);
            }

            if (_filterOnlyOpened)
            {
                query = query.Where(x => x.IsOpened);
            }

            var filteredList = query.ToList();
            IcTableTiles.ItemsSource = filteredList;
            UpdateTableOverviewStats(filteredList);
        }

        private void UpdateTableOverviewStats(List<DBAN> list)
        {
            if (list == null) return;
            int total = list.Count;
            int occupied = list.Count(x => x.IsOpened);
            int empty = total - occupied;

            if (TxtOverviewTotalTables != null) TxtOverviewTotalTables.Text = total.ToString();
            if (TxtOverviewOccupiedTables != null) TxtOverviewOccupiedTables.Text = occupied.ToString();
            if (TxtOverviewEmptyTables != null) TxtOverviewEmptyTables.Text = empty.ToString();
        }

        private void BtnAreaSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DKHUVUC kv)
            {
                if (_selectedAreaId == kv.MAKHUVUC)
                    _selectedAreaId = null;
                else
                    _selectedAreaId = kv.MAKHUVUC;

                ApplyTableFilters();
            }
        }

        private void BtnFilterDangMo_Click(object sender, RoutedEventArgs e)
        {
            var win = new DanhSachHoaDonChuaThanhToanWindow();
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrEmpty(win.SelectedBanId))
            {
                var targetBan = _allBanList.FirstOrDefault(x => x.Id == win.SelectedBanId);
                if (targetBan != null)
                {
                    ShowOrderScreen(targetBan);
                }
            }
        }

        private void TableTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is DBAN ban)
            {
                ShowOrderScreen(ban);
            }
        }

        private ObservableCollection<TouchCartItemVM> _cartItems = new();

        private async void LoadOrderItems()
        {
            try
            {
                if (_currentBan == null) return;
                _cartItems.Clear();

                string? activeOrderId = _currentBan.ActiveOrderId;

                if (string.IsNullOrEmpty(activeOrderId))
                {
                    try
                    {
                        using var conn = DbConnectionManager.GetConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        
                        string sqlOrder = @"
                            SELECT FIRST 1 CAST(o.ID AS VARCHAR(50)) as ActiveOrderId, o.NAME as SoPhieu, o.SOKHACH as SoKhach,
                                   CAST(COALESCE(o.TILETHUE, 0) AS DECIMAL(18,2)) as TiLeThue,
                                   CAST(COALESCE(o.TIENTHUE, 0) AS DECIMAL(18,2)) as TienThue,
                                   CAST(COALESCE(o.TILEPHIDICHVU, 0) AS DECIMAL(18,2)) as TiLePhiDichVu,
                                   CAST(COALESCE(o.PHIDICHVU, 0) AS DECIMAL(18,2)) as TienPhiDichVu,
                                   CAST(COALESCE(o.TILEGIAMGIA, 0) AS DECIMAL(18,2)) as TiLeGiamGia,
                                   CAST(COALESCE(o.TIENGIAMGIA, 0) AS DECIMAL(18,2)) as TienGiamGia,
                                   kh.NAME as KhachHangName
                            FROM TDONHANG o
                            LEFT JOIN DKHACHHANG kh ON CAST(o.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                            WHERE CAST(o.DBANID AS VARCHAR(50)) = @BanId 
                              AND (o.STATUS = 1 OR o.STATUS IS NULL) 
                              AND o.KETTHUC IS NULL
                            ORDER BY o.TIMECREATED DESC";
                        
                        var orderRow = await conn.QueryFirstOrDefaultAsync(sqlOrder, new { BanId = _currentBan.Id });
                        if (orderRow != null)
                        {
                            activeOrderId = orderRow.ActiveOrderId?.ToString();
                            _currentBan.ActiveOrderId = activeOrderId;
                            _currentBan.SoPhieu = orderRow.SoPhieu?.ToString();
                            if (int.TryParse(orderRow.SoKhach?.ToString(), out int sk) && sk > 0)
                            {
                                _currentBan.SoKhach = sk;
                            }
                            _currentBan.KhachHangName = orderRow.KhachHangName?.ToString();
                            
                            decimal thuePt = orderRow.TiLeThue != null ? Convert.ToDecimal(orderRow.TiLeThue) : 0;
                            decimal tienThue = orderRow.TienThue != null ? Convert.ToDecimal(orderRow.TienThue) : 0;
                            decimal phiPt = orderRow.TiLePhiDichVu != null ? Convert.ToDecimal(orderRow.TiLePhiDichVu) : 0;
                            decimal tienPhi = orderRow.TienPhiDichVu != null ? Convert.ToDecimal(orderRow.TienPhiDichVu) : 0;
                            decimal giamPt = orderRow.TiLeGiamGia != null ? Convert.ToDecimal(orderRow.TiLeGiamGia) : 0;
                            decimal tienGiam = orderRow.TienGiamGia != null ? Convert.ToDecimal(orderRow.TienGiamGia) : 0;

                            _currentBan.ThueSuatPt = thuePt;
                            _currentBan.TienThue = tienThue;
                            _currentBan.PhiDichVuPt = phiPt;
                            _currentBan.TienPhiDichVu = tienPhi;
                            _currentBan.GiamGiaPhanTram = giamPt;
                            _currentBan.GiamGia = tienGiam;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi truy vấn TDONHANG: {ex}");
                    }
                }

                if (!string.IsNullOrEmpty(activeOrderId))
                {
                    TxtOrderNo.Text = !string.IsNullOrEmpty(_currentBan.SoPhieu) ? $"HĐ: {_currentBan.SoPhieu}" : "HĐ: ...";
                    TxtOrderGuestCount.Text = _currentBan.SoKhach > 0 ? _currentBan.SoKhach.ToString() : "1";
                    TxtOrderCustomer.Text = _currentBan.KhachHangName ?? "";

                    var service = new LocalSuDungDichVuService();
                    var orderDetails = await service.GetOrderDetailsAsync(activeOrderId);
                    if (orderDetails != null && orderDetails.Any())
                    {
                        foreach (var d in orderDetails)
                        {
                            _cartItems.Add(new TouchCartItemVM
                            {
                                MAMATHANG = d.MatHangId ?? "",
                                TenMatHang = d.MatHangName ?? "",
                                SoLuong = d.SoLuong,
                                DonGiaGoc = d.DonGiaGoc > 0 ? d.DonGiaGoc : d.DonGia,
                                DonGia = d.DonGia,
                                ChietKhauPhanTram = d.ChietKhauPhanTram,
                                ThanhTien = d.ThanhTien,
                                LoaiDoId = d.LoaiDoId,
                                LoaiDoName = d.LoaiDoName ?? "",
                                DaInCheBien = d.DaInCheBien
                            });
                        }
                    }
                }

                DgOrderItems.ItemsSource = _cartItems;
                UpdateTotals();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadOrderItems: {ex}");
            }
        }

        private List<TouchNhomHangVM> _nhomList = new();
        private List<TouchMatHangVM> _allMatHangList = new();
        private TouchNhomHangVM? _selectedNhom = null;

        private void AddStaticItem(string mamathang, string manhomCode, string tenMatHang, decimal giaBan)
        {
            if (!_allMatHangList.Any(x => x.TenMatHang.Equals(tenMatHang, StringComparison.OrdinalIgnoreCase)))
            {
                _allMatHangList.Add(new TouchMatHangVM 
                { 
                    MAMATHANG = mamathang, 
                    MANHOM = manhomCode, 
                    TenMatHang = tenMatHang, 
                    GiaBan = giaBan, 
                    TileColorHex = _matHangTileColorHex 
                });
            }
        }

        private async System.Threading.Tasks.Task LoadMenuItems()
        {
            _nhomList.Clear();
            _allMatHangList.Clear();

            // 1. Try loading Categories directly from Database
            bool hasDbCategories = false;
            try
            {
                string defaultNhomColor = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_NhomHang", 
                    await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_Nhom", "#D96414"));

                var dbNhom = LocalDatabaseService.GetAll<DNHOMMATHANG>("SELECT ID, NAME as TenNhom, MAUSAC FROM DNHOMMATHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME");
                if (dbNhom != null && dbNhom.Any())
                {
                    hasDbCategories = true;
                    int idx = 1;
                    foreach (var n in dbNhom)
                    {
                        if (!string.IsNullOrWhiteSpace(n.TenNhom))
                        {
                            string dbId = !string.IsNullOrEmpty(n.ID) ? n.ID.Trim() : idx.ToString();
                            string nhomColor = !string.IsNullOrWhiteSpace(n.MAUSAC) ? QuanLyBar.Client.Services.ColorUtils.ColorIntToHex(n.MAUSAC, defaultNhomColor) : defaultNhomColor;
                            var item = new TouchNhomHangVM
                            {
                                MANHOM = dbId,
                                TenNhom = n.TenNhom.Trim(),
                                OriginalColorBrushHex = nhomColor,
                                ColorBrushHex = nhomColor,
                                TextColorHex = QuanLyBar.Client.Services.ColorUtils.GetContrastTextColor(nhomColor)
                            };
                            item.AssociatedIds.Add(dbId);
                            item.AssociatedIds.Add(n.TenNhom.Trim());
                            _nhomList.Add(item);
                            idx++;
                        }
                    }
                }
            }
            catch { }

            // 2. If DB has no categories, fallback to 10 master categories
            if (!hasDbCategories)
            {
                var cat1 = new TouchNhomHangVM { MANHOM = "CAT_KHAI_VI", TenNhom = "MÓN\nKHAI\nVỊ", OriginalColorBrushHex = "#D96414", ColorBrushHex = "#D96414", TextColorHex = "#FFFFFF" };
                var cat2 = new TouchNhomHangVM { MANHOM = "CAT_BO_BE", TenNhom = "BÒ -\nBÊ -\nTRÂU -\nDÊ", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat3 = new TouchNhomHangVM { MANHOM = "CAT_CA", TenNhom = "CÁC\nMÓN\nCÁ", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat4 = new TouchNhomHangVM { MANHOM = "CAT_HAI_SAN", TenNhom = "HẢI\nSẢN", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat5 = new TouchNhomHangVM { MANHOM = "CAT_DO_UONG", TenNhom = "ĐỒ\nUỐNG\nCÁC\nLOẠI", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat6 = new TouchNhomHangVM { MANHOM = "CAT_THIT_LON", TenNhom = "THỊT\nLỢN", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat7 = new TouchNhomHangVM { MANHOM = "CAT_LUON_ECH", TenNhom = "LƯƠN\n-\nCUA\n-\nỐC\n-\nẾCH", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat8 = new TouchNhomHangVM { MANHOM = "CAT_LAU", TenNhom = "CÁC\nMÓN\nLẨU\nVÀ\nMÓN\nĂN\nKÈM", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat9 = new TouchNhomHangVM { MANHOM = "CAT_COM", TenNhom = "CƠM\nVÀ\nMÓN\nNĂN", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat10 = new TouchNhomHangVM { MANHOM = "CAT_RAU", TenNhom = "CÁC\nMÓN\nRAU", OriginalColorBrushHex = "#FFFF33", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };

                _nhomList.Add(cat1);
                _nhomList.Add(cat2);
                _nhomList.Add(cat3);
                _nhomList.Add(cat4);
                _nhomList.Add(cat5);
                _nhomList.Add(cat6);
                _nhomList.Add(cat7);
                _nhomList.Add(cat8);
                _nhomList.Add(cat9);
                _nhomList.Add(cat10);

                foreach (var cat in _nhomList)
                {
                    cat.AssociatedIds.Add(cat.MANHOM);
                }
            }

            IcTouchNhomHang.ItemsSource = null;
            IcTouchNhomHang.ItemsSource = _nhomList;

            // 3. Try loading Menu Items directly from Database
            bool hasDbItems = false;
            try
            {
                string defaultMhColor = await LocalCauHinhService.GetConfigValueAsync("TOUCH_COLOR_MatHang", "#5B7F95");

                string sql = @"
                    SELECT m.ID, m.DNHOMMATHANGID, m.DLOAIMATHANGID, m.NAME as TenMatHang, m.GIABAN as GiaBan, m.ANH, m.MAUSAC,
                           n.NAME as NhomName, n.ID as NhomId
                    FROM DMATHANG m
                    LEFT JOIN DNHOMMATHANG n ON m.DNHOMMATHANGID = n.ID OR m.DLOAIMATHANGID = n.ID
                    WHERE (m.STATUS <> 0 OR m.STATUS IS NULL)
                    ORDER BY m.NAME";

                var dbMatHang = LocalDatabaseService.GetAll<DMATHANG>(sql);
                if (dbMatHang != null && dbMatHang.Any())
                {
                    hasDbItems = true;
                    int mIdx = 1;
                    foreach (var m in dbMatHang)
                    {
                        if (!string.IsNullOrWhiteSpace(m.TenMatHang))
                        {
                            string primaryNhomId = !string.IsNullOrWhiteSpace(m.DNHOMMATHANGID) ? m.DNHOMMATHANGID.Trim() : 
                                                  (!string.IsNullOrWhiteSpace(m.NhomId) ? m.NhomId.Trim() : 
                                                  (!string.IsNullOrWhiteSpace(m.DLOAIMATHANGID) ? m.DLOAIMATHANGID.Trim() : ""));

                            string mhColor = (!string.IsNullOrWhiteSpace(m.MAUSAC) && m.MAUSAC != "0")
                                ? QuanLyBar.Client.Services.ColorUtils.ColorIntToHex(m.MAUSAC, _matHangTileColorHex)
                                : _matHangTileColorHex;

                            var vm = new TouchMatHangVM
                            {
                                MAMATHANG = !string.IsNullOrEmpty(m.ID) ? m.ID.Trim() : mIdx.ToString(),
                                MANHOM = primaryNhomId,
                                NhomName = m.NhomName?.Trim() ?? "",
                                TenMatHang = m.TenMatHang,
                                GiaBan = m.GiaBan > 0 ? m.GiaBan : m.GIABAN,
                                TileColorHex = mhColor,
                                MauSac = m.MAUSAC ?? ""
                            };
                            if (m.ANH != null && m.ANH.Length > 0)
                            {
                                vm.ImageBytes = m.ANH;
                            }
                            _allMatHangList.Add(vm);
                            mIdx++;
                        }
                    }
                }
            }
            catch { }

            // 4. Fallback: Only add static items if DB returned 0 items total
            if (!hasDbItems)
            {
                var cat1 = _nhomList.FirstOrDefault();
                if (cat1 != null)
                {
                    AddStaticItem("101", cat1.MANHOM, "Đậu rán cà cái", 5000);
                    AddStaticItem("102", cat1.MANHOM, "Mì tôm úp", 15000);
                    AddStaticItem("103", cat1.MANHOM, "Xúc xích", 10000);
                }
            }

            foreach (var item in _allMatHangList)
            {
                if (!item.HasImage)
                {
                    CheckLocalImage(item);
                }
            }

            SelectCategory(_nhomList.FirstOrDefault());
        }

        private void CheckLocalImage(TouchMatHangVM vm)
        {
            if (vm.HasImage) return;
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] exts = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                foreach (var ext in exts)
                {
                    string p1 = System.IO.Path.Combine(baseDir, "Images", vm.MAMATHANG + ext);
                    string p2 = System.IO.Path.Combine(baseDir, "Images", vm.TenMatHang + ext);
                    string p3 = System.IO.Path.Combine(baseDir, "images", vm.MAMATHANG + ext);
                    string p4 = System.IO.Path.Combine(baseDir, "images", vm.TenMatHang + ext);
                    if (System.IO.File.Exists(p1)) { vm.ImageUrl = p1; return; }
                    if (System.IO.File.Exists(p2)) { vm.ImageUrl = p2; return; }
                    if (System.IO.File.Exists(p3)) { vm.ImageUrl = p3; return; }
                    if (System.IO.File.Exists(p4)) { vm.ImageUrl = p4; return; }
                }
            }
            catch { }
        }

        private void SelectCategory(TouchNhomHangVM? selectedNhom)
        {
            if (_nhomList == null || _nhomList.Count == 0) return;
            if (selectedNhom == null) selectedNhom = _nhomList.FirstOrDefault();

            _selectedNhom = selectedNhom;

            foreach (var n in _nhomList)
            {
                if (selectedNhom != null && (n == selectedNhom || (n.MANHOM == selectedNhom.MANHOM && !string.IsNullOrEmpty(n.MANHOM))))
                {
                    n.ColorBrushHex = "#D96414";
                    n.TextColorHex = "#FFFFFF";
                }
                else
                {
                    n.ColorBrushHex = !string.IsNullOrWhiteSpace(n.OriginalColorBrushHex) ? n.OriginalColorBrushHex : "#FFFF33";
                    n.TextColorHex = QuanLyBar.Client.Services.ColorUtils.GetContrastTextColor(n.ColorBrushHex);
                }
            }

            if (_allMatHangList != null && selectedNhom != null)
            {
                string targetManhom = selectedNhom.MANHOM?.Trim() ?? "";
                string targetTenNhom = selectedNhom.TenNhom.Replace("\n", " ").Trim();

                // Direct matching by MANHOM, AssociatedIds, or NhomName
                var filtered = _allMatHangList.Where(x => 
                    x.MANHOM.Equals(targetManhom, StringComparison.OrdinalIgnoreCase) ||
                    selectedNhom.AssociatedIds.Contains(x.MANHOM.Trim()) ||
                    (!string.IsNullOrEmpty(x.NhomName) && x.NhomName.Trim().Equals(targetTenNhom, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.NhomName) && selectedNhom.AssociatedIds.Contains(x.NhomName.Trim()))
                ).ToList();

                IcTouchMatHang.ItemsSource = null;
                IcTouchMatHang.ItemsSource = filtered;
            }
        }

        private void UpdateTotals()
        {
            decimal tienHang = 0;
            if (_cartItems != null)
            {
                foreach (var item in _cartItems)
                {
                    tienHang += item.ThanhTien;
                }
            }

            decimal giamGia = _currentBan != null ? _currentBan.GiamGia : 0;
            if (_currentBan != null && _currentBan.GiamGiaPhanTram > 0)
            {
                giamGia = Math.Round(tienHang * (_currentBan.GiamGiaPhanTram / 100m));
                _currentBan.GiamGia = giamGia;
            }

            decimal sauGiam = Math.Max(0, tienHang - giamGia);

            // Phí dịch vụ
            decimal phiPt = 0;
            if (_coPhiDichVu)
            {
                phiPt = (_currentBan != null && _currentBan.PhiDichVuPt > 0) ? _currentBan.PhiDichVuPt : _macDinhPhiDichVu;
            }
            decimal tienPhi = (_coPhiDichVu && phiPt > 0) ? Math.Round(sauGiam * (phiPt / 100m)) : 0;

            // Thuế VAT
            decimal thuePt = 0;
            if (_coThueSuat)
            {
                thuePt = (_currentBan != null && _currentBan.ThueSuatPt > 0) ? _currentBan.ThueSuatPt : _macDinhThueSuat;
            }
            decimal tienThue = (_coThueSuat && thuePt > 0) ? Math.Round((sauGiam + tienPhi) * (thuePt / 100m)) : 0;

            decimal tongCong = sauGiam + tienPhi + tienThue;
            if (_lamTronTien > 1 && tongCong > 0)
            {
                tongCong = Math.Round(tongCong / _lamTronTien, MidpointRounding.AwayFromZero) * _lamTronTien;
            }

            if (_currentBan != null)
            {
                _currentBan.TienHang = tienHang;
                _currentBan.GiamGia = giamGia;
                _currentBan.PhiDichVuPt = phiPt;
                _currentBan.TienPhiDichVu = tienPhi;
                _currentBan.ThueSuatPt = thuePt;
                _currentBan.TienThue = tienThue;
                _currentBan.TongCong = tongCong;
            }

            TxtTotalSub.Text = tienHang.ToString("N0");
            if (TxtTotalDiscountPt != null) TxtTotalDiscountPt.Text = _currentBan != null && _currentBan.GiamGiaPhanTram > 0 ? $"{_currentBan.GiamGiaPhanTram:0.##}%" : "0";
            TxtTotalDiscount.Text = giamGia.ToString("N0");

            if (RowTotalServiceFee != null)
            {
                RowTotalServiceFee.Visibility = _coPhiDichVu ? Visibility.Visible : Visibility.Collapsed;
                if (TxtTotalServiceFeePt != null) TxtTotalServiceFeePt.Text = $"{phiPt:0.##}%";
                if (TxtTotalServiceFee != null) TxtTotalServiceFee.Text = tienPhi.ToString("N0");
            }

            if (RowTotalVAT != null)
            {
                RowTotalVAT.Visibility = _coThueSuat ? Visibility.Visible : Visibility.Collapsed;
                if (TxtTotalVatPt != null) TxtTotalVatPt.Text = $"{thuePt:0.##}%";
                if (TxtTotalVat != null) TxtTotalVat.Text = tienThue.ToString("N0");
            }

            TxtTotalFinal.Text = tongCong.ToString("N0");
        }

        private async void DoLogin(string userName, string password)
        {
            try
            {
                var user = await LocalAuthService.LoginAsync(userName, password);
                if (user != null)
                {
                    SessionContext.CurrentUser = user;
                    ShowMenuScreen();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Đăng nhập thất bại",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== LOGIN MODES (MODE 1: KEYPAD #, MODE 2: FORM **|, MODE 3: CARD) =====
        private string _keypadPin = "";

        private void BtnModeKeypad_Click(object sender, RoutedEventArgs e)
        {
            SwitchLoginMode(1);
        }

        private void BtnModeForm_Click(object sender, RoutedEventArgs e)
        {
            SwitchLoginMode(2);
        }

        private void BtnModeCard_Click(object sender, RoutedEventArgs e)
        {
            SwitchLoginMode(3);
        }

        private void SwitchLoginMode(int mode)
        {
            var activeBg = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D97706"));
            var activeBorder = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F59E0B"));
            var inactiveBg = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#5A0F0F"));
            var inactiveBorder = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8A2525"));

            if (BtnModeKeypad.Template.FindName("BdModeKeypad", BtnModeKeypad) is Border bdKeypad)
            {
                bdKeypad.Background = mode == 1 ? activeBg : inactiveBg;
                bdKeypad.BorderBrush = mode == 1 ? activeBorder : inactiveBorder;
            }
            if (BtnModeForm.Template.FindName("BdModeForm", BtnModeForm) is Border bdForm)
            {
                bdForm.Background = mode == 2 ? activeBg : inactiveBg;
                bdForm.BorderBrush = mode == 2 ? activeBorder : inactiveBorder;
            }
            if (BtnModeCard.Template.FindName("BdModeCard", BtnModeCard) is Border bdCard)
            {
                bdCard.Background = mode == 3 ? activeBg : inactiveBg;
                bdCard.BorderBrush = mode == 3 ? activeBorder : inactiveBorder;
            }

            PanelLoginKeypad.Visibility = mode == 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelLoginForm.Visibility = mode == 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelLoginCard.Visibility = mode == 3 ? Visibility.Visible : Visibility.Collapsed;

            if (mode == 1)
            {
                _keypadPin = "";
                TxtKeypadDisplay.Text = "*";
            }
            else if (mode == 3)
            {
                TxtCardCodeInput.Text = "";
                TxtCardCodeInput.Focus();
            }
        }

        private void BtnKeypadNum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string num)
            {
                if (_keypadPin.Length < 12)
                {
                    _keypadPin += num;
                    TxtKeypadDisplay.Text = new string('*', _keypadPin.Length);
                }
            }
        }

        private void BtnKeypadBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (_keypadPin.Length > 0)
            {
                _keypadPin = _keypadPin.Substring(0, _keypadPin.Length - 1);
                TxtKeypadDisplay.Text = _keypadPin.Length > 0 ? new string('*', _keypadPin.Length) : "*";
            }
        }

        private void BtnKeypadSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_keypadPin))
            {
                DoLoginKeypadOrCode(_keypadPin);
            }
            else
            {
                MessageBox.Show("Vui lòng nhập mã ID để đăng nhập!", "Chưa nhập mã ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TxtCardCodeInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var code = TxtCardCodeInput.Text.Trim();
                if (!string.IsNullOrWhiteSpace(code))
                {
                    DoLoginKeypadOrCode(code);
                }
                else
                {
                    MessageBox.Show("Vui lòng quét thẻ từ hoặc nhập mã thẻ!", "Chưa nhập mã thẻ", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DoLoginKeypadOrCode(string code)
        {
            try
            {
                var user = await LocalAuthService.LoginByIdOrCodeAsync(code);
                if (user != null)
                {
                    SessionContext.CurrentUser = user;
                    ShowMenuScreen();
                }
                else
                {
                    MessageBox.Show("Mã ID hoặc Thẻ không hợp lệ!", "Đăng nhập thất bại",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== LOGIN EVENTS =====
        private void BtnPowerOff_Click(object sender, RoutedEventArgs e)
        {
            if (QuanLyBar.Views.TouchPOS.TouchConfirmWindow.Show(this, "BẠN CÓ MUỐN THOÁT KHỎI HỆ THỐNG KHÔNG?"))
                Application.Current.Shutdown();
        }

        private void BtnTouchKbUser_Click(object sender, RoutedEventArgs e)
        {
            TxtLoginUser.Focus();
            var win = new TouchKeyboardWindow(TxtLoginUser.Text, isPasswordMode: false, title: "MỜI BẠN NHẬP TÀI KHOẢN ĐĂNG NHẬP")
            {
                Owner = this
            };
            if (win.ShowDialog() == true)
            {
                TxtLoginUser.Text = win.ResultText;
            }
        }

        private void BtnTouchKbPass_Click(object sender, RoutedEventArgs e)
        {
            TxtLoginPass.Focus();
            var win = new TouchKeyboardWindow(TxtLoginPass.Password, isPasswordMode: true, title: "MỜI BẠN NHẬP MẬT KHẨU ĐĂNG NHẬP")
            {
                Owner = this
            };
            if (win.ShowDialog() == true)
            {
                TxtLoginPass.Password = win.ResultText;
            }
        }

        private void TxtLoginInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                DoLogin(TxtLoginUser.Text.Trim(), TxtLoginPass.Password);
            }
        }

        private void BtnSubmitLogin_Click(object sender, RoutedEventArgs e)
        {
            DoLogin(TxtLoginUser.Text.Trim(), TxtLoginPass.Password);
        }

        private void BtnResetLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtLoginUser.Text = "";
            TxtLoginPass.Password = "";
        }

        // ===== MENU EVENTS =====
        private string? _currentSelectedCategory = null;

        private void BtnMenuCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string categoryTag)
            {
                if (_currentSelectedCategory == categoryTag)
                {
                    SelectMenuCategory(null);
                }
                else
                {
                    SelectMenuCategory(categoryTag);
                }
            }
        }

        private void SelectMenuCategory(string? categoryTag)
        {
            _currentSelectedCategory = categoryTag;

            // Reset styles
            var tealBrush = (Brush)new BrushConverter().ConvertFromString("#009688")!;
            var orangeBrush = (Brush)new BrushConverter().ConvertFromString("#E65100")!;

            BtnMenuHeThong.Background = categoryTag == "HeThong" ? orangeBrush : tealBrush;
            BtnMenuQuanTri.Background = categoryTag == "QuanTri" ? orangeBrush : tealBrush;
            BtnMenuBaoCao.Background = categoryTag == "BaoCao" ? orangeBrush : tealBrush;
            BtnMenuTroGiup.Background = categoryTag == "TroGiup" ? orangeBrush : tealBrush;

            // Show center panel corresponding to category
            PanelCenterHeThong.Visibility = categoryTag == "HeThong" ? Visibility.Visible : Visibility.Collapsed;
            PanelCenterCoSoDuLieu.Visibility = Visibility.Collapsed;
            PanelCenterQuanTri.Visibility = categoryTag == "QuanTri" ? Visibility.Visible : Visibility.Collapsed;
            PanelCenterBaoCao.Visibility = categoryTag == "BaoCao" ? Visibility.Visible : Visibility.Collapsed;
            HideAllSubReportPanels();
            PanelCenterTroGiup.Visibility = categoryTag == "TroGiup" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HideAllSubReportPanels()
        {
            PanelSubBaoCaoQuy.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoDanhMuc.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoBanHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoDatHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoKhoHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoCongNo.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoQuanTri.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoBieuDo.Visibility = Visibility.Collapsed;

            // Sub-level 3 panels (Kho hàng)
            PanelSubBaoCaoXuatBanHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoKiemKe.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoXuatKhac.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoNhapHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoChuyenKho.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoHSD.Visibility = Visibility.Collapsed;

            // Sub-level 3 panels (Bán hàng)
            PanelSubBaoCaoTheoNhanVienPhucVu.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoKhuVuc.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoThuNgan.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoTheoKhachHang.Visibility = Visibility.Collapsed;
            PanelSubBaoCaoBanHangKhac.Visibility = Visibility.Collapsed;
        }

        // ===== HỆ THỐNG HANDLERS =====
        private void BtnMenuCoSoDuLieuSub_Click(object sender, RoutedEventArgs e)
        {
            PanelCenterHeThong.Visibility = Visibility.Collapsed;
            PanelCenterCoSoDuLieu.Visibility = Visibility.Visible;
        }

        private void BtnMenuBackHeThong_Click(object sender, RoutedEventArgs e)
        {
            PanelCenterCoSoDuLieu.Visibility = Visibility.Collapsed;
            PanelCenterHeThong.Visibility = Visibility.Visible;
        }
        private void BtnMenuCoSoDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.DataManagerWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi quản lý CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuTaoMoiCsdl_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Database file (*.fdb)|*.fdb|All files (*.*)|*.*",
                    Title = "Tạo mới cơ sở dữ liệu trắng",
                    DefaultExt = ".fdb",
                    FileName = ""
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filename = saveFileDialog.FileName;
                    string dbName = System.IO.Path.GetFileNameWithoutExtension(filename);

                    string templatePath = @"D:\QuanLyBar\frontend\CSDL\TEMPLATE.FDB";
                    if (!System.IO.File.Exists(templatePath))
                    {
                        templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSDL", "TEMPLATE.FDB");
                    }

                    if (System.IO.File.Exists(templatePath))
                    {
                        System.IO.File.Copy(templatePath, filename, true);
                        MessageBox.Show($"Đã tạo mới cơ sở dữ liệu trắng thành công tại:\n{filename}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file CSDL mẫu TEMPLATE.FDB!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo mới CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuSaoLuuCsdl_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Backup file (*.fbk;*.gbk)|*.fbk;*.gbk|All files (*.*)|*.*",
                    Title = "Sao lưu cơ sở dữ liệu",
                    FileName = $"SAOLUU_CSDL_{DateTime.Now:yyyyMMdd_HHmmss}.fbk"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    string currentDbPath = QuanLyBar.Client.Services.DbConnectionManager.CurrentConfig?.Path ?? "";
                    if (string.IsNullOrEmpty(currentDbPath) || !System.IO.File.Exists(currentDbPath))
                    {
                        currentDbPath = @"D:\taifirebird\HIHI.FDB";
                    }

                    if (System.IO.File.Exists(currentDbPath))
                    {
                        System.IO.File.Copy(currentDbPath, saveDlg.FileName, true);
                        MessageBox.Show($"Sao lưu cơ sở dữ liệu thành công!\nFile lưu tại: {saveDlg.FileName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file CSDL hiện tại để sao lưu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi sao lưu CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuPhucHoiCsdl_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.Views.KhoiPhucCsdlWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khôi phục CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Client.Views.NguoiDungPhanQuyen.DoiMatKhauWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Đổi mật khẩu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== QUẢN TRỊ HANDLERS =====
        private void BtnMenuXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Xóa dữ liệu", "View", this)) return;
            MessageBox.Show("Chức năng Xóa dữ liệu hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuCongCuDeveloper_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Công cụ nhà phát triển", "View", this)) return;
            MessageBox.Show("Công cụ nhà phát triển hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuNguoiDungPhanQuyen_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Quản lý người dùng", "View", this)) return;
            try
            {
                var win = new TouchNguoiDungPhanQuyenWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Người dùng & Phân quyền: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnMenuCauHinhHeThong_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.Views.TouchPOS.TouchCauHinhHeThongWindow();
                win.Owner = this;
                win.ShowDialog();

                await RefreshSystemConfigsAsync();
                if (GridTouchOrder.Visibility == Visibility.Visible)
                {
                    UpdateTotals();
                    await AutoSaveOrderAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Cấu hình hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== BÁO CÁO HANDLERS =====
        private void BtnSubReportGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string group)
            {
                PanelCenterBaoCao.Visibility = Visibility.Collapsed;
                HideAllSubReportPanels();

                switch (group)
                {
                    case "BaoCaoQuy": PanelSubBaoCaoQuy.Visibility = Visibility.Visible; break;
                    case "BaoCaoDanhMuc": PanelSubBaoCaoDanhMuc.Visibility = Visibility.Visible; break;
                    case "BaoCaoBanHang": PanelSubBaoCaoBanHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoDatHang": PanelSubBaoCaoDatHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoKhoHang": PanelSubBaoCaoKhoHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoCongNo": PanelSubBaoCaoCongNo.Visibility = Visibility.Visible; break;
                    case "BaoCaoQuanTri": PanelSubBaoCaoQuanTri.Visibility = Visibility.Visible; break;
                    case "BaoCaoBieuDo": PanelSubBaoCaoBieuDo.Visibility = Visibility.Visible; break;

                    // Level 3 (Kho hàng)
                    case "BaoCaoXuatBanHang": PanelSubBaoCaoXuatBanHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoKiemKe": PanelSubBaoCaoKiemKe.Visibility = Visibility.Visible; break;
                    case "BaoCaoXuatKhac": PanelSubBaoCaoXuatKhac.Visibility = Visibility.Visible; break;
                    case "BaoCaoNhapHang": PanelSubBaoCaoNhapHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoChuyenKho": PanelSubBaoCaoChuyenKho.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoHSD": PanelSubBaoCaoTheoHSD.Visibility = Visibility.Visible; break;

                    // Level 3 (Bán hàng)
                    case "BaoCaoTheoNhanVienPhucVu": PanelSubBaoCaoTheoNhanVienPhucVu.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoKhuVuc": PanelSubBaoCaoTheoKhuVuc.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoThuNgan": PanelSubBaoCaoTheoThuNgan.Visibility = Visibility.Visible; break;
                    case "BaoCaoTheoKhachHang": PanelSubBaoCaoTheoKhachHang.Visibility = Visibility.Visible; break;
                    case "BaoCaoBanHangKhac": PanelSubBaoCaoBanHangKhac.Visibility = Visibility.Visible; break;
                }
            }
        }

        private void BtnBackBaoCaoMain_Click(object sender, RoutedEventArgs e)
        {
            HideAllSubReportPanels();
            PanelCenterBaoCao.Visibility = Visibility.Visible;
        }

        private void BtnBackBaoCaoBanHang_Click(object sender, RoutedEventArgs e)
        {
            HideAllSubReportPanels();
            PanelSubBaoCaoBanHang.Visibility = Visibility.Visible;
        }

        private void BtnBackBaoCaoKhoHang_Click(object sender, RoutedEventArgs e)
        {
            HideAllSubReportPanels();
            PanelSubBaoCaoKhoHang.Visibility = Visibility.Visible;
        }

        private void BtnOpenReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string reportTitle)
            {
                if (!LocalPhanQuyenService.CheckPermissionAndAlert(reportTitle, "View", this)) return;

                UIElement? content = null;
                switch (reportTitle)
                {
                    case "DANH SÁCH PHIẾU THU THEO NGÀY":
                    case "DANH SÁCH PHIẾU THU THEO LÝ DO THU CHI":
                    case "DANH SÁCH PHIẾU CHI THEO NGÀY":
                    case "DANH SÁCH PHIẾU CHI THEO LÝ DO THU CHI":
                    case "TỔNG HỢP THU CHI THEO NGÀY":
                    case "TỔNG HỢP THU CHI THEO LÝ DO":
                    case "BÁO CÁO TỒN QUỸ":
                        content = new QuanLyBar.Client.Views.BaoCaoQuy.BaoCaoPhieuThuChiControl(reportTitle);
                        break;

                    case "DANH SÁCH KHÁCH HÀNG THEO NHÓM":
                    case "DANH SÁCH KHÁCH HÀNG THEO NHÂN VIÊN":
                    case "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM":
                    case "DANH SÁCH ĐỢT KHUYẾN MẠI":
                    case "DANH SÁCH MẶT HÀNG THEO NHÓM":
                    case "DANH SÁCH MẶT HÀNG THEO HÃNG SẢN XUẤT":
                    case "KHÁCH HÀNG ĐẾN NGÀY SINH NHẬT":
                    case "BÁO CÁO CẤU HÌNH BÀN KHU VỰC":
                    case "CÔNG THỨC ĐỊNH LƯỢNG":
                    case "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG":
                        content = new QuanLyBar.Client.Views.BaoCaoDanhMuc.BaoCaoMatHangControl(reportTitle);
                        break;

                    case "TỔNG HỢP BÁN HÀNG THEO NGÀY":
                    case "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY":
                    case "BÁO CÁO CHI TIẾT BÁN HÀNG THEO NGÀY":
                    case "TỔNG HỢP DOANH THU THEO LOẠI ĐỒ":
                    case "TỔNG HỢP DOANH THU CHƯA THANH TOÁN":
                    case "BÁO CÁO BÁN HÀNG THEO NGÀY":
                    case "TỔNG HỢP BÁN THEO NHÂN VIÊN":
                    case "TỔNG HỢP MẶT HÀNG BÁN THEO NHÂN VIÊN":
                    case "TỔNG HỢP BÁN HÀNG THEO KHU VỰC":
                    case "TỔNG HỢP BÁN HÀNG THEO BÀN PHÒNG":
                        content = new QuanLyBar.Client.Views.BaoCaoBanHang.BaoCaoBanHangTheoNgayControl(reportTitle);
                        break;

                    case "DANH SÁCH ĐẶT HÀNG THEO NGÀY":
                    case "DANH SÁCH ĐẶT HÀNG THEO KHÁCH HÀNG":
                    case "TỔNG HỢP ĐẶT HÀNG THEO NGÀY":
                    case "TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG":
                    case "TỔNG HỢP MẶT HÀNG ĐẶT THEO KHÁCH HÀNG":
                    case "TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY":
                        content = new QuanLyBar.Client.Views.BaoCaoDatHang.BaoCaoDanhSachDatHangTheoNgayControl(reportTitle);
                        break;

                    case "BÁO CÁO HÀNG TỒN KHO":
                    case "BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN":
                    case "BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN CHI TIẾT":
                    case "THẺ KHO":
                    case "DANH SÁCH PHIẾU NHẬP HÀNG THEO NGÀY":
                    case "DANH SÁCH PHIẾU KIỂM KÊ THEO NGÀY":
                        content = new QuanLyBar.Client.Views.BaoCaoKhoHang.BaoCaoHangTonKhoControl();
                        break;

                    case "BÁO CÁO CÔNG NỢ KHÁCH HÀNG":
                    case "TỔNG HỢP CÔNG NỢ KHÁCH HÀNG":
                    case "ĐỐI CHIẾU CÔNG NỢ KHÁCH HÀNG":
                    case "BÁO CÁO CÔNG NỢ NHÀ CUNG CẤP":
                    case "TỔNG HỢP CÔNG NỢ NHÀ CUNG CẤP":
                    case "ĐỐI CHIẾU CÔNG NỢ NHÀ CUNG CẤP":
                        content = new QuanLyBar.Client.Views.BaoCaoCongNo.BaoCaoTongHopCongNoKhachHangControl();
                        break;

                    case "BÁO CÁO KẾT QUẢ KINH DOANH":
                    case "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ VỐN)":
                    case "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ NHẬP)":
                    case "CHI TIẾT LÃI THEO HÓA ĐƠN (GIÁ VỐN)":
                    case "BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT":
                    case "BÁO CÁO BÁN HÀNG THEO GIỜ":
                    case "DANH SÁCH MÓN XÓA, GIẢM, TRẢ LẠI":
                        content = new QuanLyBar.Client.Views.TongHopKqkdControl();
                        break;

                    default:
                        MessageBox.Show($"Báo cáo '{reportTitle}' đang được mở.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }

                if (content != null)
                {
                    OpenReportWindow(content, reportTitle);
                }
            }
        }

        private void OpenReportWindow(UIElement content, string title)
        {
            try
            {
                var win = new QuanLyBar.Views.TouchPOS.TouchReportViewerWindow(content, title);
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== TRỢ GIÚP HANDLERS =====
        private void BtnMenuHuongDanSuDung_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hướng dẫn sử dụng Phần mềm Quản Lý Bar / Nhà Hàng V6.0.", "Hướng dẫn sử dụng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuHoTroTeamViewer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://teamviewer.com", UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show("Hỗ trợ kỹ thuật từ xa qua TeamViewer / UltraViewer.\nHotline: 0967041111", "Hỗ trợ kỹ thuật", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnMenuDangKyBanQuyen_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đăng ký bản quyền Phần mềm Quản Lý Bar V6.0 Tân An Phát.\nHotline hỗ trợ: 0967041111", "Đăng ký bản quyền", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuThongTinPhanMem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PHẦN MỀM QUẢN LÝ BAR, NHÀ HÀNG V6.0 TÂN AN PHÁT\nHotline: 0967041111\nPhiên bản: 6.0.0.0", "Thông tin phần mềm", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ===== CỘT BÊN PHẢI HANDLERS =====
        private void BtnMenuSuDungDichVu_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Hóa đơn bán hàng", "View", this)) return;
            ShowTableScreen();
        }

        private void BtnMenuTonKho_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Tồn kho", "View", this)) return;
            try
            {
                var win = new TouchTonKhoWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở giao diện Tồn kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuGhiChu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new TouchGhiChuNhanhWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Ghi chú nhanh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuDangXuat_Click(object sender, RoutedEventArgs e)
        {
            BtnLogoutPOS_Click(sender, e);
        }

        // ===== TABLE EVENTS =====
        private void SvTableTiles_ScrollChanged(object sender, ScrollChangedEventArgs e) { }

        private void SvTableTiles_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTileHeight();
        }

        private void SbTableTiles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }

        private async void BtnConfigTables_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("Ban");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadTableLayoutConfigAsync();
                }
            }
            catch { }
        }

        private void BtnConfigAreas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("KhuVuc");
                win.Owner = this;
                win.ShowDialog();
            }
            catch { }
        }

        private async void BtnConfigCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("GioHang");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadOrderLayoutConfigAsync();
                }
            }
            catch { }
        }

        private async void BtnConfigNhomHang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("Nhom");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadOrderLayoutConfigAsync();
                }
            }
            catch { }
        }

        private async void BtnConfigMatHang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("MatHang");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadOrderLayoutConfigAsync();
                }
            }
            catch { }
        }

        private void BtnOpenTablesList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new DanhSachHoaDonChuaThanhToanWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch { }
        }

        private void BtnShiftStats_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchThongKeWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void BtnClosePOS_Click(object sender, RoutedEventArgs e)
        {
            ShowMenuScreen();
        }

        private void BtnLogoutPOS_Click(object sender, RoutedEventArgs e)
        {
            if (QuanLyBar.Views.TouchPOS.TouchConfirmWindow.Show(this, "BẠN CÓ MUỐN ĐĂNG XUẤT KHỎI HỆ THỐNG KHÔNG?"))
            {
                SessionContext.CurrentUser = null;
                ShowLoginScreen();
            }
        }

        // ===== ORDER EVENTS =====
        private async void BtnOrderMoveTable_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Chuyển bàn", "View")) return;

            if (_currentBan == null || !_currentBan.IsOpened || string.IsNullOrEmpty(_currentBan.ActiveOrderId))
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "BÀN HIỆN TẠI CHƯA MỞ HOẶC KHÔNG CÓ ĐƠN HÀNG ĐỂ CHUYỂN!", "THÔNG BÁO");
                return;
            }

            try
            {
                var win = new ChonBanChuyenGopTouchWindow(_currentBan, isMergeMode: false);
                win.Owner = this;
                if (win.ShowDialog() == true && win.SelectedTargetBan != null)
                {
                    var targetBan = win.SelectedTargetBan;
                    string sourceOrderId = _currentBan.ActiveOrderId;
                    string oldName = _currentBan.Name;

                    var service = new LocalSuDungDichVuService();
                    bool success = await service.TransferTableAsync(sourceOrderId, targetBan.Id);
                    if (success)
                    {
                        // Reset source table in DB
                        try
                        {
                            using var conn = DbConnectionManager.GetConnection();
                            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                            await conn.ExecuteAsync("UPDATE DBAN SET ISOPENED = 0, ACTIVEORDERID = NULL WHERE CAST(ID AS VARCHAR(50)) = @BanId", new { BanId = _currentBan.Id });
                        }
                        catch { }

                        // Reset source table in memory
                        _currentBan.IsOpened = false;
                        _currentBan.ActiveOrderId = null;
                        _currentBan.SoPhieu = "";
                        _currentBan.TrangThai = "Trống";
                        _currentBan.MauNen = "#16213E";
                        _currentBan.KhachHangName = "";

                        _cartItems.Clear();
                        UpdateTotals();

                        QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"ĐÃ CHUYỂN TOÀN BỘ DỮ LIỆU BÀN '{oldName.ToUpper()}' SANG BÀN '{targetBan.Name.ToUpper()}' THÀNH CÔNG!", "THÔNG BÁO");

                        // Return to table selection screen
                        ShowTableScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"LỖI CHUYỂN BÀN: {ex.Message}", "LỖI");
            }
        }

        private async void BtnOrderMergeTable_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Gộp bàn", "View")) return;

            if (_currentBan == null || !_currentBan.IsOpened || string.IsNullOrEmpty(_currentBan.ActiveOrderId))
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "BÀN HIỆN TẠI CHƯA MỞ HOẶC KHÔNG CÓ ĐƠN HÀNG ĐỂ GỘP!", "THÔNG BÁO");
                return;
            }

            try
            {
                var win = new ChonBanChuyenGopTouchWindow(_currentBan, isMergeMode: true);
                win.Owner = this;
                if (win.ShowDialog() == true && win.SelectedTargetBan != null)
                {
                    var targetBan = win.SelectedTargetBan;
                    string sourceOrderId = _currentBan.ActiveOrderId;
                    string oldName = _currentBan.Name;

                    var service = new LocalSuDungDichVuService();

                    // If target table is empty, start order on target table
                    string targetOrderId = targetBan.ActiveOrderId ?? "";
                    if (!targetBan.IsOpened || string.IsNullOrEmpty(targetOrderId))
                    {
                        DateTime startTime = _currentBan.ThoiGianMo ?? DateTime.Now;
                        var startRes = await service.StartTableOrderAsync(targetBan.Id, startTime, _currentBan.SoKhach, _currentBan.KhachHangName, "");
                        if (startRes == null || string.IsNullOrEmpty(startRes.OrderId))
                        {
                            QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "KHÔNG THỂ MỞ BÀN ĐÍCH ĐỂ GỘP MÓN!", "LỖI");
                            return;
                        }
                        targetOrderId = startRes.OrderId;
                    }

                    // Query items from source & target order details
                    var sourceDetails = await service.GetOrderDetailsAsync(sourceOrderId);
                    var targetDetails = (await service.GetOrderDetailsAsync(targetOrderId))?.ToList() ?? new List<PosDonHangChiTietViewModel>();

                    foreach (var sItem in sourceDetails)
                    {
                        var exist = targetDetails.FirstOrDefault(x => x.MatHangId == sItem.MatHangId);
                        if (exist != null)
                        {
                            exist.SoLuong += sItem.SoLuong;
                            exist.Recalculate();
                        }
                        else
                        {
                            targetDetails.Add(new PosDonHangChiTietViewModel
                            {
                                Id = Guid.NewGuid().ToString("N").Substring(0, 20),
                                MatHangId = sItem.MatHangId,
                                MatHangName = sItem.MatHangName,
                                DonViTinh = sItem.DonViTinh,
                                DonGia = sItem.DonGia,
                                SoLuong = sItem.SoLuong,
                                ChietKhauPhanTram = sItem.ChietKhauPhanTram,
                                GhiChu = sItem.GhiChu
                            });
                        }
                    }

                    decimal targetTienHang = targetDetails.Sum(x => x.ThanhTien);
                    await service.SaveOrderAsync(targetOrderId, targetDetails, targetTienHang, 0, targetTienHang, "", _currentBan.SoKhach);

                    // Delete source order
                    await service.DeleteOrderAsync(sourceOrderId);

                    // Reset source table in DB
                    try
                    {
                        using var conn = DbConnectionManager.GetConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        await conn.ExecuteAsync("UPDATE DBAN SET ISOPENED = 0, ACTIVEORDERID = NULL WHERE CAST(ID AS VARCHAR(50)) = @BanId", new { BanId = _currentBan.Id });
                    }
                    catch { }

                    // Reset source table in memory
                    _currentBan.IsOpened = false;
                    _currentBan.ActiveOrderId = null;
                    _currentBan.SoPhieu = "";
                    _currentBan.TrangThai = "Trống";
                    _currentBan.MauNen = "#16213E";
                    _currentBan.KhachHangName = "";

                    _cartItems.Clear();
                    UpdateTotals();

                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"ĐÃ GỘP TOÀN BỘ MÓN TỪ BÀN '{oldName.ToUpper()}' VÀO BÀN '{targetBan.Name.ToUpper()}' THÀNH CÔNG!", "THÔNG BÁO");

                    // Return to table selection screen
                    ShowTableScreen();
                }
            }
            catch (Exception ex)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"LỖI GỘP BÀN: {ex.Message}", "LỖI");
            }
        }

        private async void BtnOrderBack_Click(object sender, RoutedEventArgs e)
        {
            await AutoSaveOrderAsync();
            ShowTableScreen();
        }

        private async void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Permission check
                if (!LocalPhanQuyenService.CheckPermissionAndAlert("Hủy hóa đơn", "View")) return;

                // 2. Validate current table & active order
                if (_currentBan == null || !_currentBan.IsOpened || string.IsNullOrEmpty(_currentBan.ActiveOrderId))
                {
                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "BÀN HIỆN TẠI KHÔNG CÓ ĐƠN HÀNG ĐỂ HỦY!", "THÔNG BÁO");
                    return;
                }

                var service = new LocalSuDungDichVuService();

                // 3. Time limit check from system configuration (SCONFIG: ThoiGianChoPhepHuyBill)
                int maxMinutes = await service.GetThoiGianChoPhepHuyBillMinutesAsync();
                DateTime? startTime = _currentBan.ThoiGianMo;

                if (!startTime.HasValue && !string.IsNullOrEmpty(_currentBan.ActiveOrderId))
                {
                    try
                    {
                        using var conn = DbConnectionManager.GetConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        var dt = await conn.QueryFirstOrDefaultAsync<DateTime?>(
                            "SELECT COALESCE(BATDAU, TIMECREATED) FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @OrderId",
                            new { OrderId = _currentBan.ActiveOrderId });
                        if (dt.HasValue) startTime = dt;
                    }
                    catch { }
                }

                if (startTime.HasValue)
                {
                    double elapsedMinutes = (DateTime.Now - startTime.Value).TotalMinutes;
                    if (elapsedMinutes > maxMinutes)
                    {
                        QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"KHÔNG ĐƯỢC PHÉP HỦY HÓA ĐƠN CÓ THỜI GIAN LỚN HƠN {maxMinutes} PHÚT (THEO CẤU HÌNH HỆ THỐNG).", "CẢNH BÁO");
                        return;
                    }
                }

                // 4. Prompt cancellation reason configuration check
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                bool nhapLyDo = !configs.TryGetValue("NhapLyDoKhiHuyHoaDon", out var nld) || nld == "1" || nld.Equals("true", StringComparison.OrdinalIgnoreCase);

                string lyDo = "Hủy hóa đơn TouchPOS";
                if (nhapLyDo)
                {
                    var lyDoWin = new QuanLyBar.Client.Views.NhapLyDoHuyWindow();
                    lyDoWin.Owner = this;
                    if (lyDoWin.ShowDialog() != true)
                    {
                        return;
                    }
                    lyDo = string.IsNullOrWhiteSpace(lyDoWin.LyDo) ? "Hủy hóa đơn TouchPOS" : lyDoWin.LyDo;
                }
                else
                {
                    if (!QuanLyBar.Views.TouchPOS.TouchConfirmWindow.Show(this, $"BẠN CÓ CHẮC CHẮN MUỐN HỦY HÓA ĐƠN CỦA BÀN '{_currentBan.Name.ToUpper()}' KHÔNG?", "XÁC NHẬN"))
                    {
                        return;
                    }
                }

                // 5. Execute CancelOrderAsync
                bool ok = await service.CancelOrderAsync(_currentBan.ActiveOrderId, lyDo);
                if (ok)
                {
                    // Update table status in database
                    try
                    {
                        using var conn = DbConnectionManager.GetConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        await conn.ExecuteAsync("UPDATE DBAN SET ISOPENED = 0, ACTIVEORDERID = NULL WHERE CAST(ID AS VARCHAR(50)) = @BanId", new { BanId = _currentBan.Id });
                    }
                    catch { }

                    // Reset table state in memory
                    _currentBan.IsOpened = false;
                    _currentBan.ActiveOrderId = null;
                    _currentBan.SoPhieu = "";
                    _currentBan.TrangThai = "Trống";
                    _currentBan.MauNen = "#16213E";
                    _currentBan.KhachHangName = "";

                    _cartItems.Clear();
                    UpdateTotals();

                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"ĐÃ HỦY HÓA ĐƠN CỦA BÀN '{_currentBan.Name.ToUpper()}' THÀNH CÔNG!", "THÔNG BÁO");

                    // 6. Return to table grid screen
                    ShowTableScreen();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hủy hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtOrderCustomer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e != null) e.Handled = true;
            BtnSelectCustomer_Click(sender, e);
        }

        private async void BtnSelectCustomer_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e != null) e.Handled = true;
            try
            {
                var win = new ChonKhachHangTouchWindow();
                win.Owner = this;
                if (win.ShowDialog() == true && win.SelectedKhachHang != null)
                {
                    string custName = !string.IsNullOrWhiteSpace(win.SelectedKhachHang.Name)
                        ? win.SelectedKhachHang.Name
                        : (!string.IsNullOrWhiteSpace(win.SelectedKhachHang.Makhach) ? win.SelectedKhachHang.Makhach : "");

                    TxtOrderCustomer.Text = custName;

                    if (_currentBan != null)
                    {
                        _currentBan.KhachHangName = custName;

                        if (!string.IsNullOrEmpty(_currentBan.ActiveOrderId))
                        {
                            try
                            {
                                using var conn = DbConnectionManager.GetConnection();
                                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                                string sql = "UPDATE TDONHANG SET DKHACHHANGID = @KhId WHERE CAST(ID AS VARCHAR(50)) = @OrderId";
                                await conn.ExecuteAsync(sql, new { KhId = win.SelectedKhachHang.Id ?? "", OrderId = _currentBan.ActiveOrderId });
                            }
                            catch (Exception dbEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error updating customer on order: {dbEx}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chọn khách hàng: {ex.Message}");
            }
        }

        private void BtnNhomHangSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TouchNhomHangVM nhom)
            {
                SelectCategory(nhom);
            }
        }

        private async Task<string> EnsureActiveOrderAsync()
        {
            if (_currentBan == null) return string.Empty;
            if (!string.IsNullOrEmpty(_currentBan.ActiveOrderId)) return _currentBan.ActiveOrderId;

            try
            {
                var service = new LocalSuDungDichVuService();
                DateTime startTime = _currentBan.ThoiGianMo ?? DateTime.Now;
                int soKhach = _currentBan.SoKhach > 0 ? _currentBan.SoKhach : 1;
                var startRes = await service.StartTableOrderAsync(_currentBan.Id, startTime, soKhach, _currentBan.KhachHangName ?? "", "");
                if (startRes != null && !string.IsNullOrEmpty(startRes.OrderId))
                {
                    _currentBan.ActiveOrderId = startRes.OrderId;
                    _currentBan.SoPhieu = startRes.SoPhieu;
                    _currentBan.IsOpened = true;
                    _currentBan.TrangThai = "Có khách";
                    TxtOrderNo.Text = $"HĐ: {startRes.SoPhieu}";
                    return startRes.OrderId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi EnsureActiveOrderAsync: {ex.Message}");
            }
            return string.Empty;
        }

        private async Task AutoSaveOrderAsync()
        {
            if (_currentBan == null) return;

            string orderId = await EnsureActiveOrderAsync();
            if (string.IsNullOrEmpty(orderId)) return;

            try
            {
                var service = new LocalSuDungDichVuService();
                var itemsToSave = new List<PosDonHangChiTietViewModel>();

                foreach (var ci in _cartItems)
                {
                    itemsToSave.Add(new PosDonHangChiTietViewModel
                    {
                        Id = Guid.NewGuid().ToString("N").Substring(0, 20),
                        MatHangId = ci.MAMATHANG,
                        MatHangName = ci.TenMatHang,
                        SoLuong = ci.SoLuong,
                        DonGiaGoc = ci.DonGiaGoc,
                        DonGia = ci.DonGia,
                        ThanhTien = ci.ThanhTien,
                        LoaiDoId = ci.LoaiDoId,
                        LoaiDoName = ci.LoaiDoName,
                        ChietKhauPhanTram = ci.ChietKhauPhanTram,
                        GhiChu = "",
                        DaInCheBien = ci.DaInCheBien
                    });
                }

                decimal tienHang = _currentBan != null ? _currentBan.TienHang : _cartItems.Sum(x => x.ThanhTien);
                decimal giamGia = _currentBan != null ? _currentBan.GiamGia : 0;
                decimal tongCong = _currentBan != null ? _currentBan.TongCong : tienHang;
                decimal tienThue = _currentBan != null ? _currentBan.TienThue : 0;
                decimal thueSuatPt = _currentBan != null ? _currentBan.ThueSuatPt : 0;
                decimal tienPhiDichVu = _currentBan != null ? _currentBan.TienPhiDichVu : 0;
                decimal phiDichVuPt = _currentBan != null ? _currentBan.PhiDichVuPt : 0;

                int.TryParse(TxtOrderGuestCount?.Text?.Trim(), out int soKhach);
                if (soKhach <= 0) soKhach = _currentBan != null && _currentBan.SoKhach > 0 ? _currentBan.SoKhach : 1;

                await service.SaveOrderAsync(
                    orderId,
                    itemsToSave,
                    tienHang,
                    giamGia,
                    tongCong,
                    "",
                    soKhach,
                    tienThue,
                    thueSuatPt,
                    tienPhiDichVu,
                    phiDichVuPt
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi AutoSaveOrderAsync: {ex.Message}");
            }
        }

        public async void AddItemToCart(TouchMatHangVM item)
        {
            if (item == null) return;
            var existing = _cartItems.FirstOrDefault(x => x.TenMatHang == item.TenMatHang);
            if (existing != null)
            {
                existing.SoLuong += 1;
                existing.DonGia = item.GiaBan;
                existing.ThanhTien = existing.SoLuong * existing.DonGia;
            }
            else
            {
                _cartItems.Add(new TouchCartItemVM
                {
                    MAMATHANG = item.MAMATHANG,
                    TenMatHang = item.TenMatHang,
                    SoLuong = 1,
                    DonGia = item.GiaBan,
                    ThanhTien = item.GiaBan
                });
            }
            DgOrderItems.Items.Refresh();
            UpdateTotals();
            await AutoSaveOrderAsync();
        }

        private void MatHangTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is TouchMatHangVM item)
            {
                AddItemToCart(item);
            }
        }

        private async void BtnOrderDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Giảm giá mặt hàng", "View")) return;

            if (_currentBan == null || string.IsNullOrEmpty(_currentBan.ActiveOrderId) || _cartItems == null || _cartItems.Count == 0)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "BÀN HIỆN TẠI CHƯA CÓ MÓN ĂN NÀO ĐỂ GIẢM GIÁ!", "THÔNG BÁO");
                return;
            }

            var win = new QuanLyBar.Client.Views.GiamGiaTheoNhomWindow();
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                decimal doAnPt = win.DoAnPercent;
                decimal doUongPt = win.DoUongPercent;
                decimal dichVuPt = win.DichVuPercent;
                decimal doKhacPt = win.DoKhacPercent;

                foreach (var item in _cartItems)
                {
                    string cat = item.ItemCategory;
                    if (cat == "DoAn")
                    {
                        item.ChietKhauPhanTram = doAnPt;
                    }
                    else if (cat == "DoUong")
                    {
                        item.ChietKhauPhanTram = doUongPt;
                    }
                    else if (cat == "DichVu")
                    {
                        item.ChietKhauPhanTram = dichVuPt;
                    }
                    else if (cat == "DoKhac")
                    {
                        item.ChietKhauPhanTram = doKhacPt;
                    }
                }

                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }
        private async void BtnOrderPrintKitchen_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("In chế biến", "View")) return;

            if (_currentBan == null || string.IsNullOrEmpty(_currentBan.ActiveOrderId) || _cartItems == null || _cartItems.Count == 0)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "BÀN HIỆN TẠI CHƯA CÓ MÓN ĂN NÀO ĐỂ IN CHẾ BIẾN!", "THÔNG BÁO");
                return;
            }

            var configs = await LocalCauHinhService.LoadAllConfigsAsync();
            bool suDungInBep = !configs.TryGetValue("SuDungChucNangInXuongBep", out var sd) || sd == "1" || sd.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (!suDungInBep)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "CHỨC NĂNG IN XUỐNG BẾP ĐANG BỊ TẮT TRONG CẤU HÌNH HỆ THỐNG.", "THÔNG BÁO");
                return;
            }

            bool inDoAn = !configs.TryGetValue("InDoAn", out var ida) || ida == "1" || ida.Equals("true", StringComparison.OrdinalIgnoreCase);
            bool inDoUong = !configs.TryGetValue("InDoUong", out var idu) || idu == "1" || idu.Equals("true", StringComparison.OrdinalIgnoreCase);
            bool inDichVu = configs.TryGetValue("InDichVu", out var idv) && (idv == "1" || idv.Equals("true", StringComparison.OrdinalIgnoreCase));
            bool inDoKhac = !configs.TryGetValue("InDoKhac", out var idk) || idk == "1" || idk.Equals("true", StringComparison.OrdinalIgnoreCase);

            var unprintedCartItems = _cartItems.Where(x => !x.DaInCheBien && (
                (x.ItemCategory == "DoAn" && inDoAn) ||
                (x.ItemCategory == "DoUong" && inDoUong) ||
                (x.ItemCategory == "DichVu" && inDichVu) ||
                (x.ItemCategory == "DoKhac" && inDoKhac)
            )).ToList();

            if (unprintedCartItems.Count == 0)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "TẤT CẢ MÓN CỦA BÀN NÀY ĐÃ ĐƯỢC IN CHẾ BIẾN XUỐNG BẾP/BAR THÀNH CÔNG!", "THÔNG BÁO");
                return;
            }

            var unprintedItems = new List<PosDonHangChiTietViewModel>();
            foreach (var ci in unprintedCartItems)
            {
                unprintedItems.Add(new PosDonHangChiTietViewModel
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 20),
                    MatHangId = ci.MAMATHANG,
                    MatHangName = ci.TenMatHang,
                    SoLuong = ci.SoLuong,
                    DonGiaGoc = ci.DonGiaGoc,
                    DonGia = ci.DonGia,
                    ThanhTien = ci.ThanhTien,
                    LoaiDoId = ci.LoaiDoId,
                    LoaiDoName = ci.LoaiDoName,
                    ChietKhauPhanTram = ci.ChietKhauPhanTram,
                    GhiChu = "",
                    DaInCheBien = false
                });
            }

            string banDisplay = !string.IsNullOrEmpty(_currentBan.KhuVucName) ? $"{_currentBan.Name} - {_currentBan.KhuVucName}" : _currentBan.Name;
            var win = new InCheBienWindow(banDisplay, unprintedItems, _currentBan.ActiveOrderId, isAuto: false);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                foreach (var ci in unprintedCartItems)
                {
                    ci.DaInCheBien = true;
                }
                await AutoSaveOrderAsync();
            }
        }
        
        private void BtnSearchItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new TimKiemMatHangTouchWindow(_allMatHangList);
                win.Owner = this;
                if (win.ShowDialog() == true && win.SelectedItem != null)
                {
                    AddItemToCart(win.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở cửa sổ tìm kiếm: {ex.Message}");
            }
        }

        private async void BtnOrderPayment_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Thanh toán", "View")) return;

            if (_currentBan == null || !_currentBan.IsOpened || string.IsNullOrEmpty(_currentBan.ActiveOrderId) || _cartItems == null || _cartItems.Count == 0)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "BÀN HIỆN TẠI KHÔNG CÓ ĐƠN HÀNG HOẶC MÓN ĂN NÀO ĐỂ THANH TOÁN!", "THÔNG BÁO");
                return;
            }

            var configs = await LocalCauHinhService.LoadAllConfigsAsync();

            DateTime? startTime = _currentBan.ThoiGianMo;
            if (!startTime.HasValue && !string.IsNullOrEmpty(_currentBan.ActiveOrderId))
            {
                try
                {
                    using var conn = DbConnectionManager.GetConnection();
                    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                    startTime = await conn.ExecuteScalarAsync<DateTime?>(
                        "SELECT TIMECREATED FROM TDONHANG WHERE CAST(ID AS VARCHAR(50)) = @OrderId",
                        new { OrderId = _currentBan.ActiveOrderId });
                    _currentBan.ThoiGianMo = startTime;
                }
                catch { }
            }

            // 1. Cảnh báo và tự động chuyển nếu thanh toán hóa đơn mở từ ngày cũ (Ảnh 1)
            if (startTime.HasValue && startTime.Value.Date < DateTime.Today)
            {
                string oldDateStr = startTime.Value.ToString("dd/MM/yyyy");
                string todayStr = DateTime.Today.ToString("dd/MM/yyyy");
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(
                    this,
                    $"Bạn đang thanh toán hóa đơn còn mở từ ngày cũ '{oldDateStr}'\nHệ thống sẽ chuyển hóa đơn này sang ngày hiện tại '{todayStr}'",
                    "CẢNH BÁO");

                var service = new LocalSuDungDichVuService();
                await service.UpdateOrderDateToTodayAsync(_currentBan.ActiveOrderId);
                _currentBan.ThoiGianMo = DateTime.Today;
            }

            // 2. Kiểm tra cấu hình: Không cho thanh toán khi chưa in chế biến
            bool khongChoThanhToanChuaIn = configs.TryGetValue("KhongChoThanhToanKhiChuaInCheBien", out var kctt) && (kctt == "1" || kctt.Equals("true", StringComparison.OrdinalIgnoreCase));
            if (khongChoThanhToanChuaIn && _cartItems.Any(x => !x.DaInCheBien))
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "CẤU HÌNH HỆ THỐNG KHÔNG CHO PHÉP THANH TOÁN KHI CÓ MÓN CHƯA IN CHẾ BIẾN!\nVUI LÒNG IN CHẾ BIẾN TRƯỚC KHI THANH TOÁN.", "CẢNH BÁO");
                return;
            }

            // 2.1. Kiểm tra cấu hình Cách chọn khách hàng khi thanh toán
            string cachChonKh = configs.TryGetValue("CachChonKhachHang", out var ckh) && !string.IsNullOrWhiteSpace(ckh) ? ckh : "Chọn bằng chuột và bàn phím";
            if (cachChonKh == "Tự động chọn khách lẻ")
            {
                if (string.IsNullOrWhiteSpace(_currentBan.KhachHangName))
                {
                    _currentBan.KhachHangName = "KHÁCH LẺ";
                    if (TxtOrderCustomer != null) TxtOrderCustomer.Text = "KHÁCH LẺ";
                }
            }
            else if (cachChonKh == "Bắt buộc chọn khách hàng")
            {
                if (string.IsNullOrWhiteSpace(_currentBan.KhachHangName) ||
                    _currentBan.KhachHangName.Equals("Khách lẻ", StringComparison.OrdinalIgnoreCase) ||
                    _currentBan.KhachHangName.Equals("KHÁCH LẺ", StringComparison.OrdinalIgnoreCase))
                {
                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "CẤU HÌNH HỆ THỐNG YÊU CẦU BẮT BUỘC CHỌN KHÁCH HÀNG TRƯỚC KHI THANH TOÁN!\nVUI LÒNG CHỌN KHÁCH HÀNG.", "CẢNH BÁO");
                    BtnSelectCustomer_Click(sender, null);
                    return;
                }
            }

            UpdateTotals();

            var posBan = new PosBanViewModel
            {
                Id = _currentBan.Id,
                Name = _currentBan.Name,
                ActiveOrderId = _currentBan.ActiveOrderId,
                SoPhieu = _currentBan.SoPhieu,
                StartTime = _currentBan.ThoiGianMo,
                TienHang = _currentBan.TienHang,
                GiamGiaPhanTram = _currentBan.GiamGiaPhanTram,
                GiamGia = _currentBan.GiamGia,
                PhiDichVuPt = _currentBan.PhiDichVuPt,
                TienPhiDichVu = _currentBan.TienPhiDichVu,
                ThueSuatPt = _currentBan.ThueSuatPt,
                TienThue = _currentBan.TienThue,
                TongCong = _currentBan.TongCong,
                KhachHangName = _currentBan.KhachHangName
            };

            // 3. Mở cửa sổ Xác nhận thanh toán POS
            var win = new XacNhanThanhToanTouchWindow(posBan, _cartItems.ToList());
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                decimal khachDua = win.KhachDua;
                decimal traLai = win.TraLai;
                decimal theATM = win.TheATM;
                decimal chuyenKhoan = win.ChuyenKhoan;
                decimal theTraTruoc = win.TheTraTruoc;
                bool inBill = win.IsInBill;

                string loaiTT = (theATM > 0) ? "TheATM" : ((chuyenKhoan > 0) ? "ChuyenKhoan" : ((theTraTruoc > 0) ? "The" : "TienMat"));

                string taiKhoanNganHangId = win.SelectedTaiKhoanNganHangId;

                var service = new LocalSuDungDichVuService();
                bool success = await service.FinishTableOrderWithDetailsAsync(
                    _currentBan.ActiveOrderId, khachDua, traLai, theATM, theTraTruoc, loaiTT, chuyenKhoan, inBill: inBill, taiKhoanNganHangId: taiKhoanNganHangId);

                if (success)
                {
                    if (inBill)
                    {
                        try
                        {
                            var printWin = new HoaDonBanHangPrintWindow(posBan, isTamTinh: false);
                            printWin.Owner = this;
                            printWin.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Lỗi in hóa đơn: {ex.Message}");
                        }
                    }

                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"THANH TOÁN HÓA ĐƠN BÀN '{_currentBan.Name.ToUpper()}' THÀNH CÔNG!", "THÔNG BÁO");

                    // Reset bàn & giỏ hàng
                    _cartItems.Clear();
                    DgOrderItems.Items.Refresh();
                    _currentBan.IsOpened = false;
                    _currentBan.ActiveOrderId = null;
                    _currentBan.SoPhieu = "";
                    _currentBan.TrangThai = "Trống";
                    _currentBan.MauNen = "#16213E";
                    _currentBan.KhachHangName = "";
                    _currentBan.ThoiGianMo = null;

                    UpdateTotals();
                    ShowTableScreen();
                }
            }
        }

        private async void BtnIncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TouchCartItemVM item)
            {
                item.SoLuong += 1;
                item.ThanhTien = item.SoLuong * item.DonGia;
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnDecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TouchCartItemVM item)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong -= 1;
                    item.ThanhTien = item.SoLuong * item.DonGia;
                }
                else
                {
                    _cartItems.Remove(item);
                }
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnRemoveCartItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TouchCartItemVM item)
            {
                _cartItems.Remove(item);
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnCartPlus_Click(object sender, RoutedEventArgs e)
        {
            var item = DgOrderItems.SelectedItem as TouchCartItemVM;
            if (item == null && _cartItems.Count > 0) item = _cartItems[_cartItems.Count - 1];
            if (item != null)
            {
                item.SoLuong += 1;
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnCartMinus_Click(object sender, RoutedEventArgs e)
        {
            var item = DgOrderItems.SelectedItem as TouchCartItemVM;
            if (item == null && _cartItems.Count > 0) item = _cartItems[_cartItems.Count - 1];
            if (item != null)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong -= 1;
                }
                else
                {
                    _cartItems.Remove(item);
                }
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnCartRemove_Click(object sender, RoutedEventArgs e)
        {
            var item = DgOrderItems.SelectedItem as TouchCartItemVM;
            if (item == null && _cartItems.Count > 0) item = _cartItems[_cartItems.Count - 1];
            if (item != null)
            {
                _cartItems.Remove(item);
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnCartKhac_Click(object sender, RoutedEventArgs e)
        {
            var item = DgOrderItems.SelectedItem as TouchCartItemVM;
            if (item == null && _cartItems.Count > 0) item = _cartItems[_cartItems.Count - 1];
            if (item != null)
            {
                var win = new TouchDieuChinhMatHangWindow(item) { Owner = this };
                win.ShowDialog();
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
            else
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "VUI LÒNG CHỌN MÓN ĂN TRONG DANH SÁCH ĐỂ ĐIỀU CHỈNH!", "THÔNG BÁO");
            }
        }

        private async void DgOrderItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgOrderItems.SelectedItem is TouchCartItemVM item)
            {
                var win = new TouchDieuChinhMatHangWindow(item) { Owner = this };
                win.ShowDialog();
                DgOrderItems.Items.Refresh();
                UpdateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private void DgOrderItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridCartItemActions != null)
            {
                GridCartItemActions.Visibility = DgOrderItems.SelectedItem != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void DgOrderItems_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Allow row selection natively
        }
    }

    public class TouchNhomHangVM : System.ComponentModel.INotifyPropertyChanged
    {
        public string OriginalColorBrushHex { get; set; } = "#FFFF33";
        public string MANHOM { get; set; } = "";
        public string TenNhom { get; set; } = "";
        public HashSet<string> AssociatedIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        private string _colorBrushHex = "#D96414";
        public string ColorBrushHex
        {
            get => _colorBrushHex;
            set
            {
                if (_colorBrushHex != value)
                {
                    _colorBrushHex = value;
                    OnPropertyChanged(nameof(ColorBrushHex));
                }
            }
        }

        private string _textColorHex = "#FFFFFF";
        public string TextColorHex
        {
            get => _textColorHex;
            set
            {
                if (_textColorHex != value)
                {
                    _textColorHex = value;
                    OnPropertyChanged(nameof(TextColorHex));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class MatHangDisplayLine
    {
        public string Text { get; set; } = "";
        public System.Windows.HorizontalAlignment Alignment { get; set; } = System.Windows.HorizontalAlignment.Left;
        public double FontSize { get; set; } = 14;
        public System.Windows.FontWeight FontWeight { get; set; } = System.Windows.FontWeights.Bold;
        public System.Windows.FontStyle FontStyle { get; set; } = System.Windows.FontStyles.Normal;
        public System.Windows.Media.Brush ForegroundBrush { get; set; } = System.Windows.Media.Brushes.White;

        public string RightText { get; set; } = "";
        public double RightFontSize { get; set; } = 14;
        public System.Windows.FontWeight RightFontWeight { get; set; } = System.Windows.FontWeights.Bold;
        public System.Windows.FontStyle RightFontStyle { get; set; } = System.Windows.FontStyles.Normal;
        public System.Windows.Media.Brush RightForegroundBrush { get; set; } = System.Windows.Media.Brushes.White;
        public System.Windows.Visibility RightVisibility => string.IsNullOrEmpty(RightText) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public class TouchMatHangVM : System.ComponentModel.INotifyPropertyChanged
    {
        public static List<string> SavedDisplayColumns { get; set; } = new List<string>();

        public void RefreshDisplayLines()
        {
            OnPropertyChanged(nameof(DisplayLines));
            OnPropertyChanged(nameof(TileColorBrush));
        }

        public List<MatHangDisplayLine> DisplayLines
        {
            get
            {
                var result = new List<MatHangDisplayLine>();
                if (SavedDisplayColumns == null || SavedDisplayColumns.Count == 0)
                {
                    result.Add(new MatHangDisplayLine
                    {
                        Text = TenMatHang,
                        Alignment = System.Windows.HorizontalAlignment.Center,
                        FontSize = 13.5,
                        FontWeight = System.Windows.FontWeights.Bold,
                        FontStyle = System.Windows.FontStyles.Normal,
                        ForegroundBrush = System.Windows.Media.Brushes.White
                    });
                    if (GiaBan > 0)
                    {
                        result.Add(new MatHangDisplayLine
                        {
                            Text = GiaBanFormatted,
                            Alignment = System.Windows.HorizontalAlignment.Center,
                            FontSize = 12.5,
                            FontWeight = System.Windows.FontWeights.Bold,
                            FontStyle = System.Windows.FontStyles.Normal,
                            ForegroundBrush = System.Windows.Media.Brushes.White
                        });
                    }
                    return result;
                }

                var leftList = new List<(string lineText, double fontSize, bool isBold, bool isItalic, System.Windows.Media.Brush brush)>();
                var rightList = new List<(string lineText, double fontSize, bool isBold, bool isItalic, System.Windows.Media.Brush brush)>();

                foreach (var rawCol in SavedDisplayColumns)
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

                    string lineText = "";
                    if (colName == "GIÁ BÁN" || colName == "ĐƠN GIÁ")
                    {
                        string priceStr = GiaBan > 0 ? $"{GiaBan:N0}đ" : "0đ";
                        lineText = !string.IsNullOrWhiteSpace(shortTitle) ? shortTitle : priceStr;
                    }
                    else if (colName == "MẶT HÀNG" || colName == "TÊN HÀNG")
                    {
                        lineText = !string.IsNullOrWhiteSpace(shortTitle) ? shortTitle : TenMatHang;
                    }
                    else if (colName == "MÃ HÀNG")
                    {
                        lineText = !string.IsNullOrWhiteSpace(shortTitle) ? shortTitle : MAMATHANG;
                    }
                    else if (colName == "NHÓM")
                    {
                        lineText = !string.IsNullOrWhiteSpace(shortTitle) ? shortTitle : NhomName;
                    }
                    else
                    {
                        lineText = !string.IsNullOrWhiteSpace(shortTitle) ? shortTitle : colName;
                    }

                    System.Windows.Media.Brush brush = System.Windows.Media.Brushes.White;
                    try
                    {
                        brush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(colorHex)!;
                    }
                    catch { }

                    if (isRight)
                    {
                        rightList.Add((lineText, fontSize, isBold, isItalic, brush));
                    }
                    else
                    {
                        leftList.Add((lineText, fontSize, isBold, isItalic, brush));
                    }
                }

                if (leftList.Count > 0 && rightList.Count > 0)
                {
                    int maxRows = Math.Max(leftList.Count, rightList.Count);
                    for (int i = 0; i < maxRows; i++)
                    {
                        var line = new MatHangDisplayLine();
                        if (i < leftList.Count)
                        {
                            var l = leftList[i];
                            line.Text = l.lineText;
                            line.Alignment = System.Windows.HorizontalAlignment.Left;
                            line.FontSize = l.fontSize;
                            line.FontWeight = l.isBold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
                            line.FontStyle = l.isItalic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal;
                            line.ForegroundBrush = l.brush;
                        }
                        if (i < rightList.Count)
                        {
                            var r = rightList[i];
                            line.RightText = r.lineText;
                            line.RightFontSize = r.fontSize;
                            line.RightFontWeight = r.isBold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
                            line.RightFontStyle = r.isItalic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal;
                            line.RightForegroundBrush = r.brush;
                        }
                        result.Add(line);
                    }
                }
                else
                {
                    bool allLeft = leftList.Count > 0;
                    var activeList = allLeft ? leftList : rightList;
                    System.Windows.HorizontalAlignment effectiveAlign = allLeft ? System.Windows.HorizontalAlignment.Center : System.Windows.HorizontalAlignment.Right;

                    foreach (var item in activeList)
                    {
                        result.Add(new MatHangDisplayLine
                        {
                            Text = item.lineText,
                            Alignment = effectiveAlign,
                            FontSize = item.fontSize,
                            FontWeight = item.isBold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                            FontStyle = item.isItalic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal,
                            ForegroundBrush = item.brush
                        });
                    }
                }

                return result;
            }
        }

        public string MAMATHANG { get; set; } = "";
        public string MANHOM { get; set; } = "";
        public string NhomName { get; set; } = "";
        public string TenMatHang { get; set; } = "";
        public string MauSac { get; set; } = "";
        public decimal GiaBan { get; set; }
        public string GiaBanFormatted => GiaBan > 0 ? GiaBan.ToString("N0") : "0";

        private System.Windows.Media.ImageSource? _imageSource;
        public System.Windows.Media.ImageSource? ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
                OnPropertyChanged(nameof(HasImage));
                OnPropertyChanged(nameof(HasImageVisibility));
                OnPropertyChanged(nameof(NoImageVisibility));
            }
        }

        private string _imageUrl = "";
        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                _imageUrl = value;
                OnPropertyChanged(nameof(ImageUrl));
                LoadImageFromSource(value);
            }
        }

        public byte[]? ImageBytes
        {
            set
            {
                if (value != null && value.Length > 0)
                {
                    try
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new System.IO.MemoryStream(value);
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        ImageSource = bitmap;
                    }
                    catch { }
                }
            }
        }

        private void LoadImageFromSource(string src)
        {
            if (string.IsNullOrWhiteSpace(src)) return;
            try
            {
                if (System.IO.File.Exists(src))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(src, UriKind.RelativeOrAbsolute);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ImageSource = bitmap;
                }
                else if (src.StartsWith("http://") || src.StartsWith("https://"))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(src));
                    ImageSource = bitmap;
                }
                else if (src.Length > 100)
                {
                    byte[] binaryData = Convert.FromBase64String(src);
                    ImageBytes = binaryData;
                }
            }
            catch { }
        }

        public bool HasImage => ImageSource != null;
        public System.Windows.Visibility HasImageVisibility => HasImage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility NoImageVisibility => HasImage ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        private string _tileColorHex = "#EF4423";
        public string TileColorHex
        {
            get => _tileColorHex;
            set
            {
                if (_tileColorHex != value)
                {
                    _tileColorHex = value;
                    OnPropertyChanged(nameof(TileColorHex));
                    OnPropertyChanged(nameof(TileColorBrush));
                }
            }
        }

        public System.Windows.Media.SolidColorBrush TileColorBrush
        {
            get
            {
                try
                {
                    string hex = TileColorHex?.Trim() ?? "#0D4B5B";
                    if (!hex.StartsWith("#")) hex = "#" + hex;
                    if (hex.Length == 7) hex = "#FF" + hex.Substring(1);
                    return (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString(hex)!;
                }
                catch
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(13, 75, 91));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class TouchCartItemVM : System.ComponentModel.INotifyPropertyChanged
    {
        private decimal _soLuong;
        private decimal _donGia;
        private decimal _donGiaGoc;
        private decimal _chietKhauPhanTram;
        private decimal _thanhTien;

        public string MAMATHANG { get; set; } = "";
        public string TenMatHang { get; set; } = "";
        public int LoaiDoId { get; set; } = 0;
        public string LoaiDoName { get; set; } = "";
        public string GhiChu { get; set; } = "";

        private bool _daInCheBien;
        public bool DaInCheBien
        {
            get => _daInCheBien;
            set
            {
                _daInCheBien = value;
                OnPropertyChanged(nameof(DaInCheBien));
            }
        }

        public decimal DonGiaGoc
        {
            get => _donGiaGoc > 0 ? _donGiaGoc : _donGia;
            set => _donGiaGoc = value;
        }

        public decimal ChietKhauPhanTram
        {
            get => _chietKhauPhanTram;
            set
            {
                _chietKhauPhanTram = value;
                if (_donGiaGoc <= 0) _donGiaGoc = _donGia;
                if (_chietKhauPhanTram > 0)
                {
                    _donGia = _donGiaGoc * (1 - (_chietKhauPhanTram / 100m));
                }
                else
                {
                    _donGia = _donGiaGoc;
                }
                _thanhTien = _soLuong * _donGia;
                OnPropertyChanged(nameof(ChietKhauPhanTram));
                OnPropertyChanged(nameof(DonGia));
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                _thanhTien = _soLuong * _donGia;
                OnPropertyChanged(nameof(SoLuong));
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal DonGia
        {
            get => _donGia;
            set
            {
                _donGia = value;
                _thanhTien = _soLuong * _donGia;
                OnPropertyChanged(nameof(DonGia));
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal ThanhTien
        {
            get => _thanhTien;
            set
            {
                _thanhTien = value;
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public string ItemCategory
        {
            get
            {
                string name = (LoaiDoName ?? "").ToLower();
                string itemName = (TenMatHang ?? "").ToLower();

                if (LoaiDoId == 2 || name.Contains("uống") || name.Contains("nước") || name.Contains("bia") || name.Contains("rượu") || name.Contains("trà") || name.Contains("cà phê")
                    || itemName.Contains("bia") || itemName.Contains("rượu") || itemName.Contains("aquafina") || itemName.Contains("nước") || itemName.Contains("trà ") || itemName.Contains("c2") || itemName.Contains("sting"))
                {
                    return "DoUong";
                }
                if (LoaiDoId == 3 || name.Contains("dịch vụ") || name.Contains("hát") || name.Contains("phòng") || name.Contains("karaoke")
                    || itemName.Contains("khăn lạnh") || itemName.Contains("karaoke") || itemName.Contains("tiền giờ"))
                {
                    return "DichVu";
                }
                if (LoaiDoId == 4 || name.Contains("khác") || itemName.Contains("thuốc lá") || itemName.Contains("ba số") || itemName.Contains("thăng long") || itemName.Contains("vinataba") || itemName.Contains("ngựa") || itemName.Contains("marlboro"))
                {
                    return "DoKhac";
                }
                return "DoAn";
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }
    }

    public class TableDisplayLine
    {
        public string Text { get; set; } = "";
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Center;
        public double FontSize { get; set; } = 14;
        public FontWeight FontWeight { get; set; } = FontWeights.Bold;
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;
        public SolidColorBrush ForegroundBrush { get; set; } = Brushes.White;

        public string RightText { get; set; } = "";
        public double RightFontSize { get; set; } = 14;
        public FontWeight RightFontWeight { get; set; } = FontWeights.Bold;
        public FontStyle RightFontStyle { get; set; } = FontStyles.Normal;
        public SolidColorBrush RightForegroundBrush { get; set; } = Brushes.White;
        public Visibility RightVisibility => string.IsNullOrEmpty(RightText) ? Visibility.Collapsed : Visibility.Visible;
    }

    // Simple model classes / helpers for TouchPOS context
    public class DBAN : System.ComponentModel.INotifyPropertyChanged
    { 
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }

        private string _id = "";
        private string _name = "";
        private string _maban = "";
        private string _tenban = "";
        private bool _isOpened = false;
        private DateTime? _thoiGianMo;
        private string? _khachHangName;
        private decimal _tongCong = 0;
        private byte[]? _anh;

        public static List<string> SavedDisplayColumns { get; set; } = new List<string> { "BÀN" };

        public byte[]? Anh
        {
            get => _anh;
            set
            {
                _anh = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AnhSource));
                OnPropertyChanged(nameof(HasAnh));
                OnPropertyChanged(nameof(HasImageVisibility));
                OnPropertyChanged(nameof(NoImageVisibility));
            }
        }
        public ImageSource? AnhSource => QuanLyBar.Client.Services.ImageHelper.BytesToBitmapImage(Anh);
        public bool HasAnh => Anh != null && Anh.Length > 0;
        public System.Windows.Visibility HasImageVisibility => HasAnh ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility NoImageVisibility => HasAnh ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public string Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string MABAN { get => _maban; set { _maban = value; OnPropertyChanged(); } }
        public string TENBAN { get => _tenban; set { _tenban = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); OnPropertyChanged(nameof(DisplayLines)); } }
        public string MAKHUVUC { get; set; } = "";
        public string KhuVucName { get; set; } = "";
        public string MauNen { get; set; } = "#16213E"; 
        public string TrangThai { get; set; } = "Trống"; 
        public bool IsOpened
        {
            get => _isOpened;
            set { _isOpened = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); OnPropertyChanged(nameof(DisplayLines)); }
        }
        public DateTime? ThoiGianMo
        {
            get => _thoiGianMo;
            set { _thoiGianMo = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); OnPropertyChanged(nameof(DisplayLines)); }
        }
        public string? ActiveOrderId { get; set; }
        public string? SoPhieu { get; set; }
        public int SoKhach { get; set; } = 1;
        public string? KhachHangName
        {
            get => _khachHangName;
            set { _khachHangName = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); OnPropertyChanged(nameof(DisplayLines)); }
        }

        public decimal TienHang { get; set; } = 0;
        public decimal GiamGiaPhanTram { get; set; } = 0;
        public decimal GiamGia { get; set; } = 0;
        public decimal ThueSuatPt { get; set; } = 0;
        public decimal TienThue { get; set; } = 0;
        public decimal PhiDichVuPt { get; set; } = 0;
        public decimal TienPhiDichVu { get; set; } = 0;
        public decimal TongCong
        {
            get => _tongCong;
            set { _tongCong = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); OnPropertyChanged(nameof(DisplayLines)); }
        }

        public List<TableDisplayLine> DisplayLines
        {
            get
            {
                var result = new List<TableDisplayLine>();
                if (SavedDisplayColumns == null || SavedDisplayColumns.Count == 0)
                {
                    result.Add(new TableDisplayLine
                    {
                        Text = TENBAN,
                        Alignment = HorizontalAlignment.Center,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        FontStyle = FontStyles.Normal,
                        ForegroundBrush = Brushes.White
                    });
                    result.Add(new TableDisplayLine
                    {
                        Text = IsOpened && ThoiGianMo.HasValue ? $"Vào: {ThoiGianMo.Value:HH:mm}" : "Bàn trống",
                        Alignment = HorizontalAlignment.Center,
                        FontSize = 13,
                        FontWeight = FontWeights.Normal,
                        FontStyle = FontStyles.Normal,
                        ForegroundBrush = Brushes.White
                    });
                    return result;
                }

                var leftList = new List<(string lineText, double fontSize, bool isBold, bool isItalic, SolidColorBrush brush)>();
                var rightList = new List<(string lineText, double fontSize, bool isBold, bool isItalic, SolidColorBrush brush)>();

                foreach (var rawCol in SavedDisplayColumns)
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

                    string lineText = "";
                    if (colName == "BÀN")
                    {
                        lineText = !string.IsNullOrWhiteSpace(shortTitle) ? shortTitle : TENBAN;
                    }
                    else if (colName == "GIỜ VÀO")
                    {
                        if (ThoiGianMo.HasValue)
                        {
                            lineText = !string.IsNullOrWhiteSpace(shortTitle) ? $"{shortTitle}: {ThoiGianMo.Value:HH:mm}" : $"Vào: {ThoiGianMo.Value:HH:mm}";
                        }
                        else
                        {
                            lineText = "Bàn trống";
                        }
                    }
                    else if (colName == "SỐ TIỀN")
                    {
                        if (TongCong > 0)
                        {
                            lineText = !string.IsNullOrWhiteSpace(shortTitle) ? $"{shortTitle}: {TongCong:N0}đ" : $"{TongCong:N0}đ";
                        }
                    }
                    else if (colName == "KHÁCH HÀNG")
                    {
                        if (!string.IsNullOrWhiteSpace(KhachHangName))
                        {
                            lineText = !string.IsNullOrWhiteSpace(shortTitle) ? $"{shortTitle}: {KhachHangName.Trim()}" : KhachHangName.Trim();
                        }
                    }

                    if (!string.IsNullOrEmpty(lineText))
                    {
                        SolidColorBrush brush = Brushes.White;
                        try
                        {
                            brush = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex)!;
                        }
                        catch { }

                        if (isRight)
                        {
                            rightList.Add((lineText, fontSize, isBold, isItalic, brush));
                        }
                        else
                        {
                            leftList.Add((lineText, fontSize, isBold, isItalic, brush));
                        }
                    }
                }

                if (leftList.Count > 0 && rightList.Count > 0)
                {
                    int maxRows = Math.Max(leftList.Count, rightList.Count);
                    for (int i = 0; i < maxRows; i++)
                    {
                        var line = new TableDisplayLine();
                        if (i < leftList.Count)
                        {
                            var l = leftList[i];
                            line.Text = l.lineText;
                            line.Alignment = HorizontalAlignment.Left;
                            line.FontSize = l.fontSize;
                            line.FontWeight = l.isBold ? FontWeights.Bold : FontWeights.Normal;
                            line.FontStyle = l.isItalic ? FontStyles.Italic : FontStyles.Normal;
                            line.ForegroundBrush = l.brush;
                        }
                        if (i < rightList.Count)
                        {
                            var r = rightList[i];
                            line.RightText = r.lineText;
                            line.RightFontSize = r.fontSize;
                            line.RightFontWeight = r.isBold ? FontWeights.Bold : FontWeights.Normal;
                            line.RightFontStyle = r.isItalic ? FontStyles.Italic : FontStyles.Normal;
                            line.RightForegroundBrush = r.brush;
                        }
                        result.Add(line);
                    }
                }
                else
                {
                    bool allLeft = leftList.Count > 0;
                    var activeList = allLeft ? leftList : rightList;
                    HorizontalAlignment effectiveAlign = allLeft ? HorizontalAlignment.Center : HorizontalAlignment.Right;

                    foreach (var item in activeList)
                    {
                        result.Add(new TableDisplayLine
                        {
                            Text = item.lineText,
                            Alignment = effectiveAlign,
                            FontSize = item.fontSize,
                            FontWeight = item.isBold ? FontWeights.Bold : FontWeights.Normal,
                            FontStyle = item.isItalic ? FontStyles.Italic : FontStyles.Normal,
                            ForegroundBrush = item.brush
                        });
                    }
                }

                if (result.Count == 0)
                {
                    result.Add(new TableDisplayLine
                    {
                        Text = TENBAN,
                        Alignment = HorizontalAlignment.Center,
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        FontStyle = FontStyles.Normal,
                        ForegroundBrush = Brushes.White
                    });
                }

                return result;
            }
        }

        public string DisplayText => string.Join("\n", DisplayLines.Select(x => x.Text));
    }
    public class DKHUVUC 
    { 
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string MAKHUVUC { get; set; } = ""; 
        public string TenKhuVuc { get; set; } = ""; 
        public string ColorHex { get; set; } = "#E65100";
        public string MAUSAC { get; set; } = "";
        public SolidColorBrush ColorBrush
        {
            get
            {
                try
                {
                    string hex = ColorHex?.Trim() ?? "#E65100";
                    if (!hex.StartsWith("#")) hex = "#" + hex;
                    if (hex.Length == 7) hex = "#FF" + hex.Substring(1);
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
                }
                catch
                {
                    return new SolidColorBrush(Color.FromRgb(230, 81, 0));
                }
            }
        }
        public int THUTU { get; set; } 
    }
    public class DNHOMMATHANG { public string ID { get; set; } = ""; public string MANHOMMATHANG { get; set; } = ""; public string TenNhom { get; set; } = ""; public string MAUSAC { get; set; } = ""; public int THUTU { get; set; } }
    public class DMATHANG { public string ID { get; set; } = ""; public string MAMATHANG { get; set; } = ""; public string DNHOMMATHANGID { get; set; } = ""; public string DLOAIMATHANGID { get; set; } = ""; public string NhomId { get; set; } = ""; public string NhomName { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal GiaBan { get; set; } public decimal GIABAN { get; set; } public string MauNen { get; set; } = "#1976D2"; public string MAUSAC { get; set; } = ""; public byte[]? ANH { get; set; } }
    public class TDONHANGCHITIET { public string MAMATHANG { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal SoLuong { get; set; } public decimal DonGia { get; set; } public decimal THANHTIEN { get; set; } }
}
