using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.KhoHang
{
    public class STemplateItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SimageId { get; set; }
        public byte[] ImageBytes { get; set; }
        public ImageSource ImageSource { get; set; }
    }

    public partial class ThemCuaHangWindow : Window
    {
        public event Action OnSaved;
        private string _id;
        private bool _isNew = true;
        private List<KhoHangTreeItem> _warehouses = new List<KhoHangTreeItem>();
        private string _selectedKhoId;

        private List<STemplateItem> _templates = new List<STemplateItem>();
        private string _selectedTemplateId;

        public ThemCuaHangWindow(string id = null)
        {
            InitializeComponent();
            _id = id;
            _isNew = string.IsNullOrEmpty(id);
            if (!_isNew)
            {
                Title = "CỬA HÀNG - SỬA";
            }
            Loaded += ThemCuaHangWindow_Loaded;
            UpdateButtonsState();
        }

        private async void ThemCuaHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWarehousesAsync();
            await LoadTemplatesAsync();

            if (!_isNew && !string.IsNullOrEmpty(_id))
            {
                try
                {
                    using (var conn = DbConnectionManager.GetConnection())
                    {
                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                        var row = await conn.QueryFirstOrDefaultAsync("SELECT * FROM DCUAHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = _id });
                        if (row != null)
                        {
                            IDictionary<string, object> d = row as IDictionary<string, object>;
                            TxtMaCuaHang.Text = d.ContainsKey("CODE") && d["CODE"] != null ? d["CODE"].ToString() : "";
                            TxtTenCuaHang.Text = d.ContainsKey("NAME") && d["NAME"] != null ? d["NAME"].ToString() : "";
                            TxtDiaChi.Text = d.ContainsKey("DIACHI") && d["DIACHI"] != null ? d["DIACHI"].ToString() : "";
                            TxtDienThoai.Text = d.ContainsKey("DIENTHOAI") && d["DIENTHOAI"] != null ? d["DIENTHOAI"].ToString() : "";
                            TxtGhiChu.Text = d.ContainsKey("NOTE") && d["NOTE"] != null ? d["NOTE"].ToString() : "";
                            _selectedKhoId = d.ContainsKey("DKHOHANGID") && d["DKHOHANGID"] != null ? d["DKHOHANGID"].ToString() : null;
                            if (!string.IsNullOrEmpty(_selectedKhoId))
                            {
                                var k = _warehouses.FirstOrDefault(x => x.Id == _selectedKhoId);
                                if (k != null) SelectKho(k);
                            }
                        }
                    }
                }
                catch { }
            }

            TxtMaCuaHang.Focus();
        }

        private async Task LoadWarehousesAsync()
        {
            try
            {
                _warehouses = await LocalKhoHangService.GetAllWarehousesFlatAsync();
                LstKhoHang.ItemsSource = _warehouses;
                if (_warehouses.Count > 0)
                {
                    if (string.IsNullOrEmpty(_selectedKhoId) || !_warehouses.Any(x => x.Id == _selectedKhoId))
                    {
                        SelectKho(_warehouses[0]);
                    }
                    else
                    {
                        var cur = _warehouses.FirstOrDefault(x => x.Id == _selectedKhoId);
                        if (cur != null) SelectKho(cur);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadWarehousesAsync: " + ex.Message);
            }
        }

        private void SelectKho(KhoHangTreeItem kho)
        {
            if (kho == null) return;
            _selectedKhoId = kho.Id;
            TxtSelectedKho.Text = kho.Name;
            LstKhoHang.SelectedItem = kho;
        }

        private void TxtSelectedKho_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PopupKho.IsOpen = !PopupKho.IsOpen;
        }

        private void LstKhoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstKhoHang.SelectedItem is KhoHangTreeItem kho)
            {
                SelectKho(kho);
                PopupKho.IsOpen = false;
                BtnToggleKho.IsChecked = false;
            }
        }

        private void BtnThemKho_Click(object sender, RoutedEventArgs e)
        {
            PopupKho.IsOpen = false;
            BtnToggleKho.IsChecked = false;

            var win = new ThemKhoHangWindow();
            win.Owner = this;
            win.OnSaved += async () =>
            {
                await LoadWarehousesAsync();
            };
            win.ShowDialog();
        }

        private async void BtnTaiKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadWarehousesAsync();
        }

        private void BtnDanhMucKho_Click(object sender, RoutedEventArgs e)
        {
            PopupKho.IsOpen = false;
            BtnToggleKho.IsChecked = false;

            var win = new DanhMucKhoHangWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private async Task LoadTemplatesAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    string sql = @"
                        SELECT s.ID, s.NAME, s.SIMAGEID, sim.IMAGE as ImageBytes
                        FROM STEMPLATE s
                        LEFT JOIN SIMAGE sim ON CAST(s.SIMAGEID AS VARCHAR(50)) = CAST(sim.ID AS VARCHAR(50))
                        WHERE (s.STATUS IS NULL OR s.STATUS <> 0)
                          AND (CAST(s.SFORMID AS VARCHAR(50)) = '5f029a5b-8ec2-4229-bd1d-03fed9569db2' OR s.NAME LIKE 'Mẫu in%' OR s.NAME LIKE 'Mẫu 54%' OR s.NAME LIKE 'Mẫu 80%')
                        ORDER BY s.NAME";
                    var rows = await conn.QueryAsync(sql);
                    _templates = new List<STemplateItem>();
                    foreach (var r in rows)
                    {
                        byte[] b = r.IMAGEBYTES as byte[];
                        _templates.Add(new STemplateItem
                        {
                            Id = r.ID?.ToString(),
                            Name = r.NAME?.ToString(),
                            SimageId = r.SIMAGEID?.ToString(),
                            ImageBytes = b,
                            ImageSource = LocalKhoHangService.BytesToBitmapImage(b)
                        });
                    }

                    LstMauHoaDon.ItemsSource = _templates;
                    if (_templates.Count > 0)
                    {
                        if (string.IsNullOrEmpty(_selectedTemplateId) || !_templates.Any(x => x.Id == _selectedTemplateId))
                        {
                            SelectTemplate(_templates[0]);
                        }
                        else
                        {
                            var cur = _templates.FirstOrDefault(x => x.Id == _selectedTemplateId);
                            if (cur != null) SelectTemplate(cur);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTemplatesAsync: " + ex.Message);
            }
        }

        private void SelectTemplate(STemplateItem template)
        {
            if (template == null) return;
            _selectedTemplateId = template.Id;
            TxtSelectedMauHoaDon.Text = template.Name;
            LstMauHoaDon.SelectedItem = template;
        }

        private void TxtSelectedMauHoaDon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PopupMauHoaDon.IsOpen = !PopupMauHoaDon.IsOpen;
        }

        private void LstMauHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstMauHoaDon.SelectedItem is STemplateItem template)
            {
                SelectTemplate(template);
                PopupMauHoaDon.IsOpen = false;
                BtnToggleMauHoaDon.IsChecked = false;
            }
        }

        private void BtnThemMauHoaDon_Click(object sender, RoutedEventArgs e)
        {
            PopupMauHoaDon.IsOpen = false;
            BtnToggleMauHoaDon.IsChecked = false;
            MessageBox.Show("Chức năng thêm mẫu hóa đơn đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnTaiMauHoaDon_Click(object sender, RoutedEventArgs e)
        {
            await LoadTemplatesAsync();
        }

        private void BtnDanhMucMauHoaDon_Click(object sender, RoutedEventArgs e)
        {
            PopupMauHoaDon.IsOpen = false;
            BtnToggleMauHoaDon.IsChecked = false;
            MessageBox.Show("Chức năng danh mục mẫu hóa đơn đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateButtonsState()
        {
            bool hasValidName = !string.IsNullOrWhiteSpace(TxtTenCuaHang?.Text);
            if (BtnLuu != null) BtnLuu.IsEnabled = hasValidName;
            if (BtnLuuVaMoi != null) BtnLuuVaMoi.IsEnabled = hasValidName;
            if (BtnLuuVaThoat != null) BtnLuuVaThoat.IsEnabled = hasValidName;
        }

        private void TxtMaCuaHang_TextChanged(object sender, TextChangedEventArgs e) => UpdateButtonsState();
        private void TxtTenCuaHang_TextChanged(object sender, TextChangedEventArgs e) => UpdateButtonsState();

        private void BtnMaCuaHangDropdown_Click(object sender, RoutedEventArgs e)
        {
            // Tự động sinh mã nếu trống
            if (string.IsNullOrWhiteSpace(TxtMaCuaHang.Text))
            {
                TxtMaCuaHang.Text = "CH" + DateTime.Now.ToString("HHmmss");
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtTenCuaHang.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên cửa hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenCuaHang.Focus();
                return false;
            }

            string code = TxtMaCuaHang.Text.Trim();
            string diachi = TxtDiaChi.Text.Trim();
            string dienthoai = TxtDienThoai.Text.Trim();
            string khoId = _selectedKhoId;
            string note = TxtGhiChu.Text.Trim();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                    string userId = SessionContext.CurrentUser?.Id;
                    if (string.IsNullOrEmpty(userId))
                    {
                        try
                        {
                            var uObj = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                            userId = uObj?.ToString();
                        }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(userId)) userId = "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                    if (_isNew || string.IsNullOrEmpty(_id))
                    {
                        string newId = Guid.NewGuid().ToString();
                        string insertSql = @"
                            INSERT INTO DCUAHANG (
                                ID, NAME, CODE, DIACHI, DIENTHOAI, DKHOHANGID, STEMPLATEID, NOTE, STATUS, USERCREATEDID, TIMECREATED, SORTORDER
                            ) VALUES (
                                @Id, @Name, @Code, @DiaChi, @DienThoai, @DkhohangId, @StemplateId, @Note, 30, @UserCreatedId, CURRENT_TIMESTAMP, 'ZZZZ'
                            )";

                        await conn.ExecuteAsync(insertSql, new
                        {
                            Id = newId,
                            Name = name,
                            Code = code,
                            DiaChi = diachi,
                            DienThoai = dienthoai,
                            DkhohangId = khoId,
                            StemplateId = _selectedTemplateId,
                            Note = note,
                            UserCreatedId = userId
                        });
                    }
                    else
                    {
                        string updateSql = @"
                            UPDATE DCUAHANG SET
                                NAME = @Name,
                                CODE = @Code,
                                DIACHI = @DiaChi,
                                DIENTHOAI = @DienThoai,
                                DKHOHANGID = @DkhohangId,
                                STEMPLATEID = @StemplateId,
                                NOTE = @Note,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";

                        await conn.ExecuteAsync(updateSql, new
                        {
                            Id = _id,
                            Name = name,
                            Code = code,
                            DiaChi = diachi,
                            DienThoai = dienthoai,
                            DkhohangId = khoId,
                            StemplateId = _selectedTemplateId,
                            Note = note,
                            UserModifiedId = userId
                        });
                    }

                    OnSaved?.Invoke();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu cửa hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                MessageBox.Show("Đã lưu thông tin cửa hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                TxtMaCuaHang.Text = "";
                TxtTenCuaHang.Text = "";
                TxtDiaChi.Text = "";
                TxtDienThoai.Text = "";
                TxtGhiChu.Text = "";
                TxtMaCuaHang.Focus();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await SaveDataAsync();
            if (ok)
            {
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}
