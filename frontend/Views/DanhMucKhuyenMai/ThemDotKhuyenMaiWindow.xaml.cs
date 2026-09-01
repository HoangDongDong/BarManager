using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class GiamGiaSpItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MathangId { get; set; } = string.Empty;
        public string Mahang { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
        public decimal Giaban { get; set; }
        public decimal TileGiamGia { get; set; }
        public decimal GiaKhuyenMai { get; set; }
        public string Dvt { get; set; } = string.Empty;
    }

    public class GiamGiaNhomHangItem
    {
        public int Stt { get; set; }
        public string SttStr => Stt < 10 ? $"0{Stt}" : Stt.ToString();
        public string NhomHangId { get; set; } = string.Empty;
        public string TenNhom { get; set; } = string.Empty;
        public decimal TileGiamGia { get; set; }
    }

    public class MuaXTangYItem
    {
        public int Stt { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MathangId { get; set; } = string.Empty;
        public string Mahang { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
        public string Dvt { get; set; } = string.Empty;
        public decimal SoLuongMua { get; set; } = 1;
        public decimal SoLuongTang { get; set; } = 1;
        public string MathangTangId { get; set; } = string.Empty;
        public string MahangTang { get; set; } = string.Empty;
        public string TenHangTang { get; set; } = string.Empty;
        public string DvtTang { get; set; } = string.Empty;
    }

    public partial class ThemDotKhuyenMaiWindow : Window
    {
        private string _id;
        private string _defaultLoaiHinhId;
        private bool _isNew = true;
        private ObservableCollection<NhomKhachHangTreeItem> _nhomHangTree = new();

        private ObservableCollection<GiamGiaSpItem> _listSp1 = new();
        private ObservableCollection<GiamGiaNhomHangItem> _listNhom2 = new();
        private ObservableCollection<GiamGiaSpItem> _listSp3 = new();
        private ObservableCollection<MuaXTangYItem> _listSp5 = new();

        private string _selectedNhomHangId = "ALL";
        private Dictionary<string, string> _loaiHinhMap = new(); // Name -> ID

        public event Action OnSaved;

        public ThemDotKhuyenMaiWindow(string id = null, string defaultLoaiHinhId = null)
        {
            InitializeComponent();
            _id = id;
            _defaultLoaiHinhId = defaultLoaiHinhId;
            _isNew = string.IsNullOrEmpty(id);

            DgGiamGiaPhanTramSp.ItemsSource = _listSp1;
            DgGiamGiaTheoNhomHang.ItemsSource = _listNhom2;
            DgGiamGiaTienSp.ItemsSource = _listSp3;
            DgMuaXTangY.ItemsSource = _listSp5;

            Loaded += ThemDotKhuyenMaiWindow_Loaded;
        }

        private async void ThemDotKhuyenMaiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadLoaiHinhKhuyenMaiMapAsync();
                await LoadNhomHangTreeAsync();

                if (_isNew)
                {
                    TxtHeaderTitle.Text = "Đợt khuyến mại - THÊM MỚI";
                    this.Title = "ĐỢT KHUYẾN MẠI - THÊM MỚI";
                    DpTuNgay.SelectedDate = DateTime.Today;
                    DpDenNgay.SelectedDate = DateTime.Today.AddDays(30);

                    // Chọn loại hình mặc định nếu được truyền vào
                    if (!string.IsNullOrEmpty(_defaultLoaiHinhId))
                    {
                        SelectLoaiHinhById(_defaultLoaiHinhId);
                    }
                    else
                    {
                        CboLoaiHinh.SelectedIndex = 0;
                    }
                }
                else
                {
                    TxtHeaderTitle.Text = "Đợt khuyến mại - CHỈNH SỬA";
                    this.Title = "ĐỢT KHUYẾN MẠI - CHỈNH SỬA";
                    await LoadDataAsync();
                }

                TxtTenDot.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLoaiHinhKhuyenMaiMapAsync()
        {
            _loaiHinhMap.Clear();
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME FROM DLOAIHINHKHUYENMAI WHERE (STATUS = 30 OR STATUS > 0) ORDER BY SORTORDER, NAME";
                    var rows = (await conn.QueryAsync(sql)).ToList();
                    foreach (var r in rows)
                    {
                        string id = r.ID?.ToString()?.Trim() ?? "";
                        string name = r.NAME?.ToString()?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(name) && !_loaiHinhMap.ContainsKey(name))
                        {
                            _loaiHinhMap[name] = id;
                        }
                    }
                }
            }
            catch { }
        }

        private void SelectLoaiHinhById(string loaiHinhId)
        {
            string matchedName = _loaiHinhMap.FirstOrDefault(x => x.Value == loaiHinhId).Key;
            if (!string.IsNullOrEmpty(matchedName))
            {
                for (int i = 0; i < CboLoaiHinh.Items.Count; i++)
                {
                    if (CboLoaiHinh.Items[i] is ComboBoxItem cbi && cbi.Content?.ToString() == matchedName)
                    {
                        CboLoaiHinh.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        private async Task LoadNhomHangTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME, PARENTID, PARENTDIR FROM DNHOMMATHANG WHERE (STATUS = 30 OR STATUS > 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME";
                    var groups = (await conn.QueryAsync(sql)).ToList();

                    _nhomHangTree = new ObservableCollection<NhomKhachHangTreeItem>();
                    var root = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        Icon = "🌐",
                        IsExpanded = true
                    };

                    _listNhom2.Clear();
                    int sttNhom = 1;

                    var lookup = new Dictionary<string, NhomKhachHangTreeItem>();
                    var rawItems = new List<dynamic>();

                    foreach (var g in groups)
                    {
                        string id = g.ID?.ToString()?.Trim() ?? "";
                        string name = g.NAME?.ToString()?.Trim() ?? "";
                        string parentId = g.PARENTID?.ToString()?.Trim();
                        var item = new NhomKhachHangTreeItem
                        {
                            Id = id,
                            Name = name,
                            ParentId = parentId,
                            Icon = "📁",
                            IsExpanded = true
                        };
                        lookup[id] = item;
                        rawItems.Add(g);

                        _listNhom2.Add(new GiamGiaNhomHangItem
                        {
                            Stt = sttNhom++,
                            NhomHangId = id,
                            TenNhom = name,
                            TileGiamGia = 0
                        });
                    }

                    foreach (var g in rawItems)
                    {
                        string id = g.ID?.ToString()?.Trim() ?? "";
                        string parentId = g.PARENTID?.ToString()?.Trim();
                        var item = lookup[id];

                        if (!string.IsNullOrEmpty(parentId) && lookup.ContainsKey(parentId))
                        {
                            lookup[parentId].Children.Add(item);
                        }
                        else
                        {
                            root.Children.Add(item);
                        }
                    }

                    _nhomHangTree.Add(root);

                    TvNhomHang1.ItemsSource = _nhomHangTree;
                    TvNhomHang3.ItemsSource = _nhomHangTree;
                    TvNhomHang5.ItemsSource = _nhomHangTree;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadNhomHangTreeAsync: " + ex.Message);
            }
        }

        private void CboLoaiHinh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewGiamGiaPhanTramSanPham == null) return;

            int idx = CboLoaiHinh.SelectedIndex;
            ViewGiamGiaPhanTramSanPham.Visibility = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
            ViewGiamGiaTheoNhomHang.Visibility = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
            ViewGiamGiaTienTheoSanPham.Visibility = idx == 2 ? Visibility.Visible : Visibility.Collapsed;
            ViewGiamGiaTongBill.Visibility = idx == 3 ? Visibility.Visible : Visibility.Collapsed;
            ViewMuaXTangY.Visibility = idx == 4 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TvNhomHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem selected)
            {
                _selectedNhomHangId = selected.Id;
            }
        }

        private void ReindexListSp1()
        {
            int stt = 1;
            foreach (var item in _listSp1) item.Stt = stt++;
            DgGiamGiaPhanTramSp.Items.Refresh();
        }

        private void ReindexListSp3()
        {
            int stt = 1;
            foreach (var item in _listSp3) item.Stt = stt++;
            DgGiamGiaTienSp.Items.Refresh();
        }

        private void BtnThemSp_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonMatHangKhuyenMaiWindow(_selectedNhomHangId);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && win.SelectedItems.Count > 0)
            {
                int loaiIdx = CboLoaiHinh.SelectedIndex;
                foreach (var sp in win.SelectedItems)
                {
                    if (loaiIdx == 0) // Giảm giá % theo sản phẩm
                    {
                        if (!_listSp1.Any(x => x.MathangId == sp.Id))
                        {
                            _listSp1.Add(new GiamGiaSpItem
                            {
                                MathangId = sp.Id,
                                Mahang = sp.Mahang,
                                TenHang = sp.Name,
                                Giaban = sp.Giaban,
                                TileGiamGia = 0,
                                Dvt = sp.Dvt
                            });
                        }
                    }
                    else if (loaiIdx == 2) // Giảm giá tiền theo sản phẩm
                    {
                        if (!_listSp3.Any(x => x.MathangId == sp.Id))
                        {
                            _listSp3.Add(new GiamGiaSpItem
                            {
                                MathangId = sp.Id,
                                Mahang = sp.Mahang,
                                TenHang = sp.Name,
                                Giaban = sp.Giaban,
                                GiaKhuyenMai = sp.Giaban,
                                Dvt = sp.Dvt
                            });
                        }
                    }
                }

                if (loaiIdx == 0) ReindexListSp1();
                else if (loaiIdx == 2) ReindexListSp3();
            }
        }

        private void ReindexListSp5()
        {
            int stt = 1;
            foreach (var item in _listSp5) item.Stt = stt++;
            DgMuaXTangY.Items.Refresh();
        }

        private void BtnThemSp5_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonMatHangMuaTangWindow(_selectedNhomHangId);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && win.SelectedItemMua != null && win.SelectedItemTang != null)
            {
                var mua = win.SelectedItemMua;
                var tang = win.SelectedItemTang;

                _listSp5.Add(new MuaXTangYItem
                {
                    MathangId = mua.Id,
                    Mahang = mua.Mahang,
                    TenHang = mua.Name,
                    Dvt = mua.Dvt,
                    SoLuongMua = 1,
                    SoLuongTang = 1,
                    MathangTangId = tang.Id,
                    MahangTang = tang.Mahang,
                    TenHangTang = tang.Name,
                    DvtTang = tang.Dvt
                });
                ReindexListSp5();
            }
        }

        private async void BtnThemTheoNhom5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = @"
                        SELECT m.ID, 
                               COALESCE(m.CODE, m.MASANCO, '') as Mahang, 
                               m.NAME, 
                               m.GIABAN, 
                               m.DNHOMMATHANGID as NhomHangId, 
                               d.NAME as Dvt
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH d ON m.DDONVITINHID = d.ID
                        WHERE (m.STATUS = 30 OR m.STATUS > 0 OR m.STATUS IS NULL)";

                    if (_selectedNhomHangId != "ALL" && !string.IsNullOrEmpty(_selectedNhomHangId))
                    {
                        sql += $" AND m.DNHOMMATHANGID = '{_selectedNhomHangId.Replace("'", "''")}'";
                    }

                    sql += " ORDER BY m.NAME";

                    var items = (await conn.QueryAsync<MatHangKhuyenMaiItem>(sql)).ToList();
                    if (items.Count == 0)
                    {
                        MessageBox.Show("Không có mặt hàng nào trong nhóm được chọn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    int added = 0;
                    foreach (var sp in items)
                    {
                        if (!_listSp5.Any(x => x.MathangId == sp.Id))
                        {
                            _listSp5.Add(new MuaXTangYItem
                            {
                                MathangId = sp.Id,
                                Mahang = sp.Mahang,
                                TenHang = sp.Name,
                                Dvt = sp.Dvt,
                                SoLuongMua = 1,
                                SoLuongTang = 1,
                                MathangTangId = sp.Id,
                                MahangTang = sp.Mahang,
                                TenHangTang = sp.Name,
                                DvtTang = sp.Dvt
                            });
                            added++;
                        }
                    }
                    ReindexListSp5();
                    MessageBox.Show($"Đã thêm {added} mặt hàng vào danh sách Mua x tặng y!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm theo nhóm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXoaSp1_Click(object sender, RoutedEventArgs e)
        {
            if (DgGiamGiaPhanTramSp.SelectedItem is GiamGiaSpItem selected)
            {
                _listSp1.Remove(selected);
                ReindexListSp1();
            }
        }

        private void BtnXoaSp3_Click(object sender, RoutedEventArgs e)
        {
            if (DgGiamGiaTienSp.SelectedItem is GiamGiaSpItem selected)
            {
                _listSp3.Remove(selected);
                ReindexListSp3();
            }
        }

        private void BtnXoaSp5_Click(object sender, RoutedEventArgs e)
        {
            if (DgMuaXTangY.SelectedItem is MuaXTangYItem selected)
            {
                _listSp5.Remove(selected);
                ReindexListSp5();
            }
        }

        private void BtnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Import Excel mặt hàng khuyến mại đang được cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Nạp dữ liệu chỉnh sửa
        private async Task LoadDataAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sqlMaster = $"SELECT * FROM DDOTKHUYENMAI WHERE ID = '{_id.Replace("'", "''")}'";
                    var d = (await conn.QueryAsync(sqlMaster)).FirstOrDefault();
                    if (d != null)
                    {
                        TxtTenDot.Text = d.NAME?.ToString() ?? "";
                        TxtGhiChu.Text = d.NOTE?.ToString() ?? "";
                        string loaiId = d.DLOAIHINHKHUYENMAIID?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(loaiId)) SelectLoaiHinhById(loaiId);

                        if (d.TUNGAY != null) DpTuNgay.SelectedDate = Convert.ToDateTime(d.TUNGAY);
                        if (d.DENNGAY != null) DpDenNgay.SelectedDate = Convert.ToDateTime(d.DENNGAY);

                        string ngung = d.NGUNGAPDUNG?.ToString()?.Trim();
                        ChkNgungApDung.IsChecked = ngung == "1" || ngung == "True";

                        if (d.TUGIO != null) TxtTuGio.Text = Convert.ToDateTime(d.TUGIO).ToString("HH:mm");
                        if (d.DENGIO != null) TxtDenGio.Text = Convert.ToDateTime(d.DENGIO).ToString("HH:mm");

                        if (d.TILEGIAMGIA != null) TxtTileGiamTienHang.Text = d.TILEGIAMGIA.ToString();
                        if (d.TILEGIAMGIATONG != null) TxtTileGiamTienPhong.Text = d.TILEGIAMGIATONG.ToString();
                    }

                    // Nạp chi tiết
                    string sqlDetails = $@"
                        SELECT c.*, 
                               m.NAME as TenHang, 
                               COALESCE(m.CODE, m.MASANCO, '') as Mahang, 
                               d.NAME as Dvt,
                               mt.NAME as TenHangTang, 
                               COALESCE(mt.CODE, mt.MASANCO, '') as MahangTang,
                               dt.NAME as DvtTang,
                               nh.NAME as TenNhom
                        FROM DDOTKHUYENMAICHITIET c
                        LEFT JOIN DMATHANG m ON c.DMATHANGID = m.ID
                        LEFT JOIN DDONVITINH d ON m.DDONVITINHID = d.ID
                        LEFT JOIN DMATHANG mt ON c.DMATHANGTANGID = mt.ID
                        LEFT JOIN DDONVITINH dt ON mt.DDONVITINHID = dt.ID
                        LEFT JOIN DNHOMMATHANG nh ON c.DNHOMMATHANGID = nh.ID
                        WHERE c.DDOTKHUYENMAIID = '{_id.Replace("'", "''")}'";

                    var details = (await conn.QueryAsync(sqlDetails)).ToList();
                    _listSp1.Clear();
                    _listSp3.Clear();
                    _listSp5.Clear();

                    foreach (var row in details)
                    {
                        decimal tile = row.TILEGIAMGIA != null ? Convert.ToDecimal(row.TILEGIAMGIA) : 0;
                        decimal giaBan = row.GIABAN != null ? Convert.ToDecimal(row.GIABAN) : 0;
                        string nhomId = row.DNHOMMATHANGID?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(nhomId))
                        {
                            var targetNhom = _listNhom2.FirstOrDefault(x => x.NhomHangId == nhomId);
                            if (targetNhom != null) targetNhom.TileGiamGia = tile;
                        }

                        _listSp1.Add(new GiamGiaSpItem
                        {
                            MathangId = row.DMATHANGID?.ToString()?.Trim(),
                            Mahang = row.MAHANG?.ToString(),
                            TenHang = row.TENHANG?.ToString(),
                            Giaban = giaBan,
                            TileGiamGia = tile,
                            Dvt = row.DVT?.ToString()
                        });

                        _listSp3.Add(new GiamGiaSpItem
                        {
                            MathangId = row.DMATHANGID?.ToString()?.Trim(),
                            Mahang = row.MAHANG?.ToString(),
                            TenHang = row.TENHANG?.ToString(),
                            Giaban = giaBan,
                            GiaKhuyenMai = giaBan > 0 ? giaBan - tile : 0,
                            Dvt = row.DVT?.ToString()
                        });

                        _listSp5.Add(new MuaXTangYItem
                        {
                            MathangId = row.DMATHANGID?.ToString()?.Trim(),
                            Mahang = row.MAHANG?.ToString(),
                            TenHang = row.TENHANG?.ToString(),
                            Dvt = row.DVT?.ToString(),
                            SoLuongMua = row.SOLUONGMUA != null ? Convert.ToDecimal(row.SOLUONGMUA) : 1,
                            MathangTangId = row.DMATHANGTANGID?.ToString()?.Trim(),
                            MahangTang = row.MAHANGTANG?.ToString(),
                            TenHangTang = row.TENHANGTANG?.ToString(),
                            DvtTang = row.DVTTANG?.ToString() ?? row.DVT?.ToString(),
                            SoLuongTang = row.SOLUONGTANG != null ? Convert.ToDecimal(row.SOLUONGTANG) : 1
                        });
                    }

                    ReindexListSp1();
                    ReindexListSp3();
                    ReindexListSp5();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadDataAsync details: " + ex.Message);
            }
        }
        #endregion

        #region Lưu dữ liệu
        private async Task<bool> SaveAsync()
        {
            string name = TxtTenDot.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên đợt khuyến mại không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenDot.Focus();
                return false;
            }

            string selectedLoaiHinhName = (CboLoaiHinh.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string loaiHinhId = _loaiHinhMap.ContainsKey(selectedLoaiHinhName) ? _loaiHinhMap[selectedLoaiHinhName] : null;

            string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";
            DateTime tuNgay = DpTuNgay.SelectedDate ?? DateTime.Today;
            DateTime denNgay = DpDenNgay.SelectedDate ?? DateTime.Today.AddDays(30);
            string ngungApDung = ChkNgungApDung.IsChecked == true ? "1" : "0";
            string note = TxtGhiChu.Text.Trim();

            decimal tileGiamGia = 0;
            decimal tileGiamGiaTong = 0;
            decimal.TryParse(TxtTileGiamTienHang.Text.Trim(), out tileGiamGia);
            decimal.TryParse(TxtTileGiamTienPhong.Text.Trim(), out tileGiamGiaTong);

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        string dotId = _isNew ? Guid.NewGuid().ToString() : _id;

                        if (_isNew)
                        {
                            string sqlInsertMaster = $@"
                                INSERT INTO DDOTKHUYENMAI (
                                    ID, NAME, NOTE, STATUS, USERCREATEDID, USERMODIFIEDID, 
                                    TIMECREATED, TIMEMODIFIED, DLOAIHINHKHUYENMAIID,
                                    TUNGAY, DENNGAY, NGUNGAPDUNG, TILEGIAMGIA, TILEGIAMGIATONG
                                ) VALUES (
                                    '{dotId}', '{name.Replace("'", "''")}', '{note.Replace("'", "''")}', 30,
                                    '{userId}', '{userId}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
                                    {(string.IsNullOrEmpty(loaiHinhId) ? "NULL" : $"'{loaiHinhId}'")},
                                    '{tuNgay:yyyy-MM-dd}', '{denNgay:yyyy-MM-dd}', '{ngungApDung}',
                                    {tileGiamGia}, {tileGiamGiaTong}
                                )";
                            await conn.ExecuteAsync(sqlInsertMaster, transaction: trans);
                        }
                        else
                        {
                            string sqlUpdateMaster = $@"
                                UPDATE DDOTKHUYENMAI SET
                                    NAME = '{name.Replace("'", "''")}',
                                    NOTE = '{note.Replace("'", "''")}',
                                    USERMODIFIEDID = '{userId}',
                                    TIMEMODIFIED = CURRENT_TIMESTAMP,
                                    DLOAIHINHKHUYENMAIID = {(string.IsNullOrEmpty(loaiHinhId) ? "NULL" : $"'{loaiHinhId}'")},
                                    TUNGAY = '{tuNgay:yyyy-MM-dd}',
                                    DENNGAY = '{denNgay:yyyy-MM-dd}',
                                    NGUNGAPDUNG = '{ngungApDung}',
                                    TILEGIAMGIA = {tileGiamGia},
                                    TILEGIAMGIATONG = {tileGiamGiaTong}
                                WHERE ID = '{dotId}'";
                            await conn.ExecuteAsync(sqlUpdateMaster, transaction: trans);
                        }

                        // Xóa chi tiết cũ
                        string sqlDelDetails = $"DELETE FROM DDOTKHUYENMAICHITIET WHERE DDOTKHUYENMAIID = '{dotId}'";
                        await conn.ExecuteAsync(sqlDelDetails, transaction: trans);

                        int loaiIdx = CboLoaiHinh.SelectedIndex;

                        if (loaiIdx == 0) // Giảm giá % theo sản phẩm
                        {
                            foreach (var item in _listSp1)
                            {
                                string cId = Guid.NewGuid().ToString();
                                string sqlDetail = $@"
                                    INSERT INTO DDOTKHUYENMAICHITIET (
                                        ID, DDOTKHUYENMAIID, DMATHANGID, GIABAN, TILEGIAMGIA, 
                                        STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        '{cId}', '{dotId}', '{item.MathangId}', {item.Giaban}, {item.TileGiamGia},
                                        30, '{userId}', CURRENT_TIMESTAMP
                                    )";
                                await conn.ExecuteAsync(sqlDetail, transaction: trans);
                            }
                        }
                        else if (loaiIdx == 1) // Giảm giá theo nhóm hàng
                        {
                            foreach (var item in _listNhom2.Where(x => x.TileGiamGia > 0))
                            {
                                string cId = Guid.NewGuid().ToString();
                                string sqlDetail = $@"
                                    INSERT INTO DDOTKHUYENMAICHITIET (
                                        ID, DDOTKHUYENMAIID, DNHOMMATHANGID, TILEGIAMGIA, 
                                        STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        '{cId}', '{dotId}', '{item.NhomHangId}', {item.TileGiamGia},
                                        30, '{userId}', CURRENT_TIMESTAMP
                                    )";
                                await conn.ExecuteAsync(sqlDetail, transaction: trans);
                            }
                        }
                        else if (loaiIdx == 2) // Giảm giá tiền theo sản phẩm
                        {
                            foreach (var item in _listSp3)
                            {
                                string cId = Guid.NewGuid().ToString();
                                decimal tienGiam = item.Giaban > item.GiaKhuyenMai ? item.Giaban - item.GiaKhuyenMai : item.GiaKhuyenMai;
                                string sqlDetail = $@"
                                    INSERT INTO DDOTKHUYENMAICHITIET (
                                        ID, DDOTKHUYENMAIID, DMATHANGID, GIABAN, TILEGIAMGIA, 
                                        STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        '{cId}', '{dotId}', '{item.MathangId}', {item.Giaban}, {tienGiam},
                                        30, '{userId}', CURRENT_TIMESTAMP
                                    )";
                                await conn.ExecuteAsync(sqlDetail, transaction: trans);
                            }
                        }
                        else if (loaiIdx == 4) // Mua x tặng y
                        {
                            foreach (var item in _listSp5)
                            {
                                string cId = Guid.NewGuid().ToString();
                                string sqlDetail = $@"
                                    INSERT INTO DDOTKHUYENMAICHITIET (
                                        ID, DDOTKHUYENMAIID, DMATHANGID, SOLUONGMUA, DMATHANGTANGID, SOLUONGTANG,
                                        STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        '{cId}', '{dotId}', '{item.MathangId}', {item.SoLuongMua}, '{item.MathangTangId}', {item.SoLuongTang},
                                        30, '{userId}', CURRENT_TIMESTAMP
                                    )";
                                await conn.ExecuteAsync(sqlDetail, transaction: trans);
                            }
                        }

                        trans.Commit();
                    }
                }

                OnSaved?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu đợt khuyến mại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                MessageBox.Show("Lưu đợt khuyến mại thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                _id = null;
                _isNew = true;
                TxtTenDot.Text = "";
                TxtGhiChu.Text = "";
                _listSp1.Clear();
                _listSp3.Clear();
                _listSp5.Clear();
                foreach (var nhom in _listNhom2) nhom.TileGiamGia = 0;
                DgGiamGiaTheoNhomHang.Items.Refresh();
                TxtTenDot.Focus();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveAsync())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _id = null;
            _isNew = true;
            TxtTenDot.Text = "";
            TxtGhiChu.Text = "";
            _listSp1.Clear();
            _listSp3.Clear();
            _listSp5.Clear();
            foreach (var nhom in _listNhom2) nhom.TileGiamGia = 0;
            DgGiamGiaTheoNhomHang.Items.Refresh();
            TxtTenDot.Focus();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuVaMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuu_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnTaoMoi_Click(sender, e);
                e.Handled = true;
            }
        }
        #endregion
    }
}
