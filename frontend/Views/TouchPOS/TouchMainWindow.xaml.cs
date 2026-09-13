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

                if (_allMatHangList != null)
                {
                    foreach (var item in _allMatHangList)
                    {
                        item.TileColorHex = _matHangTileColorHex;
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

            LoadOrderItems();
            LoadMenuItems();
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

                string[] presetColors = new string[] { "#E65100", "#414BEA", "#00C800", "#00838F", "#C2185B", "#6A1B9A", "#D84315" };
                int colorIndex = 0;

                foreach (var kv in kvBanList)
                {
                    _allKhuVucList.Add(new DKHUVUC
                    {
                        Id = kv.Id,
                        MAKHUVUC = kv.Id,
                        Name = kv.Name,
                        TenKhuVuc = kv.Name,
                        ColorHex = presetColors[colorIndex % presetColors.Length]
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
                            IsOpened = b.IsOccupied,
                            ActiveOrderId = b.ActiveOrderId,
                            SoPhieu = b.SoPhieu,
                            SoKhach = b.SoKhach > 0 ? b.SoKhach : 1,
                            KhachHangName = b.KhachHangName
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

            IcTableTiles.ItemsSource = query.ToList();
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
            _filterOnlyOpened = !_filterOnlyOpened;
            ApplyTableFilters();
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
                                DonGia = d.DonGia,
                                ThanhTien = d.ThanhTien
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

        private void LoadMenuItems()
        {
            _nhomList.Clear();
            _allMatHangList.Clear();

            // 1. Try loading Categories directly from Database
            bool hasDbCategories = false;
            try
            {
                var dbNhom = LocalDatabaseService.GetAll<DNHOMMATHANG>("SELECT ID, NAME as TenNhom FROM DNHOMMATHANG WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY NAME");
                if (dbNhom != null && dbNhom.Any())
                {
                    hasDbCategories = true;
                    int idx = 1;
                    foreach (var n in dbNhom)
                    {
                        if (!string.IsNullOrWhiteSpace(n.TenNhom))
                        {
                            string dbId = !string.IsNullOrEmpty(n.ID) ? n.ID.Trim() : idx.ToString();
                            var item = new TouchNhomHangVM
                            {
                                MANHOM = dbId,
                                TenNhom = n.TenNhom.Trim(),
                                ColorBrushHex = idx == 1 ? "#D96414" : "#FFFF33",
                                TextColorHex = idx == 1 ? "#FFFFFF" : "#4A3B00"
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
                var cat1 = new TouchNhomHangVM { MANHOM = "CAT_KHAI_VI", TenNhom = "MÓN\nKHAI\nVỊ", ColorBrushHex = "#D96414", TextColorHex = "#FFFFFF" };
                var cat2 = new TouchNhomHangVM { MANHOM = "CAT_BO_BE", TenNhom = "BÒ -\nBÊ -\nTRÂU -\nDÊ", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat3 = new TouchNhomHangVM { MANHOM = "CAT_CA", TenNhom = "CÁC\nMÓN\nCÁ", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat4 = new TouchNhomHangVM { MANHOM = "CAT_HAI_SAN", TenNhom = "HẢI\nSẢN", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat5 = new TouchNhomHangVM { MANHOM = "CAT_DO_UONG", TenNhom = "ĐỒ\nUỐNG\nCÁC\nLOẠI", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat6 = new TouchNhomHangVM { MANHOM = "CAT_THIT_LON", TenNhom = "THỊT\nLỢN", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat7 = new TouchNhomHangVM { MANHOM = "CAT_LUON_ECH", TenNhom = "LƯƠN\n-\nCUA\n-\nỐC\n-\nẾCH", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat8 = new TouchNhomHangVM { MANHOM = "CAT_LAU", TenNhom = "CÁC\nMÓN\nLẨU\nVÀ\nMÓN\nĂN\nKÈM", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat9 = new TouchNhomHangVM { MANHOM = "CAT_COM", TenNhom = "CƠM\nVÀ\nMÓN\nNĂN", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };
                var cat10 = new TouchNhomHangVM { MANHOM = "CAT_RAU", TenNhom = "CÁC\nMÓN\nRAU", ColorBrushHex = "#FFFF33", TextColorHex = "#4A3B00" };

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
                string sql = @"
                    SELECT m.ID, m.DNHOMMATHANGID, m.DLOAIMATHANGID, m.NAME as TenMatHang, m.GIABAN as GiaBan, m.ANH,
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

                            var vm = new TouchMatHangVM
                            {
                                MAMATHANG = !string.IsNullOrEmpty(m.ID) ? m.ID.Trim() : mIdx.ToString(),
                                MANHOM = primaryNhomId,
                                NhomName = m.NhomName?.Trim() ?? "",
                                TenMatHang = m.TenMatHang,
                                GiaBan = m.GiaBan > 0 ? m.GiaBan : m.GIABAN,
                                TileColorHex = _matHangTileColorHex
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
                    n.ColorBrushHex = "#FFFF33";
                    n.TextColorHex = "#4A3B00";
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
            decimal tamTinh = 0, giamGia = 0;
            if (_cartItems != null)
            {
                foreach (var item in _cartItems)
                {
                    tamTinh += item.ThanhTien;
                }
            }
            TxtTotalSub.Text = tamTinh.ToString("N0");
            TxtTotalDiscount.Text = giamGia.ToString("N0");
            TxtTotalFinal.Text = (tamTinh - giamGia).ToString("N0");
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
                var win = new QuanLyBar.Client.Views.NguoiDungPhanQuyen.NguoiDungPhanQuyenWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Người dùng & Phân quyền: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMenuCauHinhHeThong_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalPhanQuyenService.CheckPermissionAndAlert("Cấu hình hệ thống", "View", this)) return;
            try
            {
                var win = new QuanLyBar.Client.Views.CauHinhHeThong.CauHinhToanHeThongWindow();
                win.Owner = this;
                win.ShowDialog();
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
                var win = new QuanLyBar.Client.Views.TienIch.GhiChuNhanhWindow();
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

        private void BtnShiftStats_Click(object sender, RoutedEventArgs e) { }

        private void BtnClosePOS_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Đóng ca làm việc?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
        private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            ShowTableScreen();
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

        public void AddItemToCart(TouchMatHangVM item)
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
        }

        private void MatHangTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is TouchMatHangVM item)
            {
                AddItemToCart(item);
            }
        }

        private void BtnOrderDiscount_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrderMoveTable_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrderMergeTable_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrderPrintKitchen_Click(object sender, RoutedEventArgs e) { }
        
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

        private void BtnOrderPayment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thanh toán...", "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnIncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TouchCartItemVM item)
            {
                item.SoLuong += 1;
                UpdateTotals();
            }
        }

        private void BtnDecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TouchCartItemVM item)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong -= 1;
                }
                else
                {
                    _cartItems.Remove(item);
                }
                UpdateTotals();
            }
        }

        private void BtnRemoveCartItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TouchCartItemVM item)
            {
                _cartItems.Remove(item);
                UpdateTotals();
            }
        }
    }

    public class TouchNhomHangVM : System.ComponentModel.INotifyPropertyChanged
    {
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

    public class TouchMatHangVM : System.ComponentModel.INotifyPropertyChanged
    {
        public string MAMATHANG { get; set; } = "";
        public string MANHOM { get; set; } = "";
        public string NhomName { get; set; } = "";
        public string TenMatHang { get; set; } = "";
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
                    return (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom(TileColorHex)!;
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
        private decimal _thanhTien;

        public string MAMATHANG { get; set; } = "";
        public string TenMatHang { get; set; } = "";

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

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }
    }

    // Simple model classes / helpers for TouchPOS context
    public class DBAN 
    { 
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string MABAN { get; set; } = ""; 
        public string TENBAN { get; set; } = ""; 
        public string MAKHUVUC { get; set; } = "";
        public string MauNen { get; set; } = "#16213E"; 
        public string TrangThai { get; set; } = "Trống"; 
        public bool IsOpened { get; set; } = false;
        public DateTime? ThoiGianMo { get; set; } 
        public string? ActiveOrderId { get; set; }
        public string? SoPhieu { get; set; }
        public int SoKhach { get; set; } = 1;
        public string? KhachHangName { get; set; }
    }
    public class DKHUVUC 
    { 
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string MAKHUVUC { get; set; } = ""; 
        public string TenKhuVuc { get; set; } = ""; 
        public string ColorHex { get; set; } = "#E65100";
        public SolidColorBrush ColorBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(ColorHex);
        public int THUTU { get; set; } 
    }
    public class DNHOMMATHANG { public string ID { get; set; } = ""; public string MANHOMMATHANG { get; set; } = ""; public string TenNhom { get; set; } = ""; public int THUTU { get; set; } }
    public class DMATHANG { public string ID { get; set; } = ""; public string MAMATHANG { get; set; } = ""; public string DNHOMMATHANGID { get; set; } = ""; public string DLOAIMATHANGID { get; set; } = ""; public string NhomId { get; set; } = ""; public string NhomName { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal GiaBan { get; set; } public decimal GIABAN { get; set; } public string MauNen { get; set; } = "#1976D2"; public byte[]? ANH { get; set; } }
    public class TDONHANGCHITIET { public string MAMATHANG { get; set; } = ""; public string TenMatHang { get; set; } = ""; public decimal SoLuong { get; set; } public decimal DonGia { get; set; } public decimal THANHTIEN { get; set; } }
}
