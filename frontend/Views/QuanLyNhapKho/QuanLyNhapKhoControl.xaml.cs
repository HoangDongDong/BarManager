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
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views;
using QuanLyBar.Client.Views.KhoHang;

namespace QuanLyBar.Client.Views.QuanLyNhapKho
{
    public partial class QuanLyNhapKhoControl : UserControl
    {
        private ObservableCollection<KhoHangTreeItem> _treeItems = new();
        private List<PhieuNhapItem> _allPhieuNhap = new();
        private KhoHangTreeItem _selectedTreeItem;
        private PhieuNhapItem _selectedPhieuNhap;

        public QuanLyNhapKhoControl()
        {
            InitializeComponent();
            Loaded += QuanLyNhapKhoControl_Loaded;
            PreviewKeyDown += QuanLyNhapKhoControl_PreviewKeyDown;
        }

        private async void QuanLyNhapKhoControl_Loaded(object sender, RoutedEventArgs e)
        {
            DpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DpDenNgay.SelectedDate = DateTime.Today;

            await LoadLookupsAsync();
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private async Task LoadLookupsAsync()
        {
            var khoList = await LocalNhapKhoService.GetKhoHangListFlatAsync();
            var khoCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            khoCombo.AddRange(khoList);
            CboKhoNhap.ItemsSource = khoCombo;
            CboKhoNhap.SelectedIndex = 0;

            var nvList = await LocalNhapKhoService.GetNhanVienLookupListAsync();
            var nvCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            nvCombo.AddRange(nvList);
            CboNhanVienNhap.ItemsSource = nvCombo;
            CboNhanVienNhap.SelectedIndex = 0;

            var nccList = await LocalNhapKhoService.GetNhaCungCapLookupListAsync();
            var nccCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            nccCombo.AddRange(nccList);
            CboNhaCungCap.ItemsSource = nccCombo;
            CboNhaCungCap.SelectedIndex = 0;
        }

        private string _currentTreeMode = "KhoNhap";

        public async Task LoadTreeAsync()
        {
            try
            {
                List<KhoHangTreeItem> tree;
                switch (_currentTreeMode)
                {
                    case "NhanVienNhap":
                        tree = await LocalNhapKhoService.GetNhanVienTreeAsync();
                        break;
                    case "CuaHang":
                        tree = await LocalNhapKhoService.GetCuaHangTreeAsync();
                        break;
                    case "TaiKhoanNganHang":
                        tree = await LocalNhapKhoService.GetTaiKhoanNganHangTreeAsync();
                        break;
                    case "BangGia":
                        tree = await LocalNhapKhoService.GetBangGiaTreeAsync();
                        break;
                    case "KhoNhap":
                    default:
                        tree = await LocalNhapKhoService.GetKhoHangTreeAsync();
                        break;
                }

                _treeItems.Clear();
                foreach (var item in tree)
                {
                    _treeItems.Add(item);
                }
                TvKhoHang.ItemsSource = _treeItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadTreeAsync error: " + ex.Message);
            }
        }

        public async Task LoadDataGridAsync()
        {
            try
            {
                string khoId = null;
                string nvId = CboNhanVienNhap.SelectedValue?.ToString();
                string nccId = CboNhaCungCap.SelectedValue?.ToString();
                string cuaHangId = null;
                string taiKhoanNganHangId = null;
                bool isTrash = false;

                if (_selectedTreeItem != null)
                {
                    if (_selectedTreeItem.Id == "TRASH")
                    {
                        isTrash = true;
                    }
                    else if (_selectedTreeItem.Id != "ALL")
                    {
                        switch (_currentTreeMode)
                        {
                            case "KhoNhap":
                                khoId = _selectedTreeItem.Id;
                                break;
                            case "NhanVienNhap":
                                nvId = _selectedTreeItem.Id;
                                break;
                            case "CuaHang":
                                cuaHangId = _selectedTreeItem.Id;
                                break;
                            case "TaiKhoanNganHang":
                                taiKhoanNganHangId = _selectedTreeItem.Id;
                                break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(CboKhoNhap.SelectedValue?.ToString()))
                {
                    khoId = CboKhoNhap.SelectedValue.ToString();
                }

                _allPhieuNhap = await LocalNhapKhoService.GetPhieuNhapListAsync(
                    DpTuNgay.SelectedDate,
                    DpDenNgay.SelectedDate,
                    khoId,
                    nvId,
                    nccId,
                    cuaHangId,
                    taiKhoanNganHangId,
                    isTrash
                );

                ApplyFilter();
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadDataGridAsync error: " + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            string keyword = TxtTimKiem.Text?.Trim().ToLowerInvariant() ?? "";
            var filtered = _allPhieuNhap.AsEnumerable();

            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = filtered.Where(x =>
                    (x.SoPhieu != null && x.SoPhieu.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenNhaCungCap != null && x.TenNhaCungCap.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenKhoNhap != null && x.TenKhoNhap.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenNhanVienNhap != null && x.TenNhanVienNhap.ToLowerInvariant().Contains(keyword)) ||
                    (x.Note != null && x.Note.ToLowerInvariant().Contains(keyword))
                );
            }

            var list = filtered.ToList();
            int stt = 1;
            foreach (var item in list) item.Stt = stt++;

            DgPhieuNhap.ItemsSource = list;
            if (list.Count > 0)
            {
                DgPhieuNhap.SelectedIndex = 0;
            }
            else
            {
                ClearDetails();
            }
        }

        private async void Filter_Changed(object sender, RoutedEventArgs e)
        {
            await LoadDataGridAsync();
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void TvKhoHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as KhoHangTreeItem;
            await LoadDataGridAsync();
        }

        private async void DgPhieuNhap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPhieuNhap = DgPhieuNhap.SelectedItem as PhieuNhapItem;
            if (_selectedPhieuNhap != null)
            {
                TxtTimeCreated.Text = _selectedPhieuNhap.TimeCreatedHienThi;
                TxtUserCreated.Text = string.IsNullOrEmpty(_selectedPhieuNhap.UserCreatedName) ? "Administrator" : _selectedPhieuNhap.UserCreatedName;
                TxtTimeModified.Text = _selectedPhieuNhap.TimeModifiedHienThi;
                TxtUserModified.Text = _selectedPhieuNhap.UserModifiedName ?? "";

                var details = await LocalNhapKhoService.GetPhieuNhapChiTietAsync(_selectedPhieuNhap.Id);
                DgChiTietNhapKho.ItemsSource = details;
            }
            else
            {
                ClearDetails();
            }
        }

        private void ClearDetails()
        {
            TxtTimeCreated.Text = "";
            TxtUserCreated.Text = "";
            TxtTimeModified.Text = "";
            TxtUserModified.Text = "";
            DgChiTietNhapKho.ItemsSource = null;
            DgPhieuThu.ItemsSource = null;
            DgPhieuChi.ItemsSource = null;
            DgPhieuThuCongNo.ItemsSource = null;
            DgDonHangGio.ItemsSource = null;
            DgInCheBien.ItemsSource = null;
        }

        private void DgPhieuNhap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ThemPhieuNhapKhoWindow(allPhieuNhap: _allPhieuNhap);
                var parentWin = Window.GetWindow(this);
                if (parentWin != null) win.Owner = parentWin;
                win.OnSaved += async () =>
                {
                    await LoadDataGridAsync();
                };
                if (win.ShowDialog() == true)
                {
                    _ = LoadDataGridAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở màn hình Thêm mới: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuNhap == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu nhập cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var win = new ThemPhieuNhapKhoWindow(_selectedPhieuNhap.Id, _allPhieuNhap);
                var parentWin = Window.GetWindow(this);
                if (parentWin != null) win.Owner = parentWin;
                win.OnSaved += async () =>
                {
                    await LoadDataGridAsync();
                };
                if (win.ShowDialog() == true)
                {
                    _ = LoadDataGridAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở màn hình Chỉnh sửa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuNhap == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu nhập cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isTrash = _selectedPhieuNhap.Status == 0;
            string confirmMsg = isTrash
                ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN phiếu nhập '{_selectedPhieuNhap.SoPhieu}' không?"
                : $"Bạn có chắc chắn muốn chuyển phiếu nhập '{_selectedPhieuNhap.SoPhieu}' vào thùng rác không?";

            if (MessageBox.Show(confirmMsg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalNhapKhoService.DeletePhieuNhapAsync(_selectedPhieuNhap.Id, isTrash);
                if (ok)
                {
                    await LoadDataGridAsync();
                }
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Không thể thực hiện import/cập nhật dữ liệu từ excel với dữ liệu này", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuNhap == null || _allPhieuNhap.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu phiếu nhập để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachPhieuNhap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("PhieuNhap");
                        string[] headers = new[] { "STT", "Số phiếu", "Ngày", "Nhà cung cấp", "Kho nhập", "Nhân viên nhập", "Tiền hàng", "Tiền giảm giá", "Tỉ lệ giảm giá", "Tài khoản ngân hàng", "Tổng cộng", "Cửa hàng", "Ghi chú" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                        }

                        int r = 2;
                        foreach (var item in _allPhieuNhap)
                        {
                            ws.Cell(r, 1).Value = item.Stt;
                            ws.Cell(r, 2).Value = item.SoPhieu;
                            ws.Cell(r, 3).Value = item.NgayHienThi;
                            ws.Cell(r, 4).Value = item.TenNhaCungCap;
                            ws.Cell(r, 5).Value = item.TenKhoNhap;
                            ws.Cell(r, 6).Value = item.TenNhanVienNhap;
                            ws.Cell(r, 7).Value = item.TienHang;
                            ws.Cell(r, 8).Value = item.TienGiamGia;
                            ws.Cell(r, 9).Value = item.TiLeGiamGia;
                            ws.Cell(r, 10).Value = item.TenTaiKhoanNganHang;
                            ws.Cell(r, 11).Value = item.TongCong;
                            ws.Cell(r, 12).Value = item.TenCuaHang;
                            ws.Cell(r, 13).Value = item.Note;
                            r++;
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show($"Đã xuất thành công {_allPhieuNhap.Count} phiếu nhập ra file Excel!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgPhieuNhap, "Phiếu nhập");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            int count = _allPhieuNhap.Count;
            decimal total = _allPhieuNhap.Sum(x => x.TongCong);
            MessageBox.Show($"Tổng số phiếu nhập: {count}\nTổng tiền nhập kho: {total:N0} VNĐ", "Tổng hợp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPhanTich_Click(object sender, RoutedEventArgs e)
        {
            BtnTong_Click(sender, e);
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataGridAsync();
        }

        #region Kho Tree Action Buttons & ContextMenu Handlers
        private void BtnCauHinhKho_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTreeMode == "KhoNhap")
            {
                var win = new DanhMucKhoHangWindow();
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
                _ = LoadTreeAsync();
            }
            else
            {
                MessageBox.Show($"Quản lý danh mục {GetTreeItemTypeName()}!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThemKho_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentTreeMode)
            {
                case "NhanVienNhap":
                    {
                        var win = new ThemNhanVienWindow();
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () => { await LoadTreeAsync(); await LoadLookupsAsync(); };
                        win.ShowDialog();
                    }
                    break;
                case "CuaHang":
                    {
                        var win = new ThemCuaHangWindow();
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () => { await LoadTreeAsync(); };
                        win.ShowDialog();
                    }
                    break;
                case "TaiKhoanNganHang":
                    {
                        string name = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên tài khoản ngân hàng mới:", "Thêm tài khoản ngân hàng", "Tài khoản mới");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _ = Task.Run(async () =>
                            {
                                using (var conn = DbConnectionManager.GetConnection())
                                {
                                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                                    await conn.ExecuteAsync("INSERT INTO DTAIKHOANNGANHANG (ID, NAME, STATUS) VALUES (@Id, @Name, 30)", new { Id = Guid.NewGuid().ToString(), Name = name.Trim() });
                                }
                                await Dispatcher.InvokeAsync(async () => { await LoadTreeAsync(); });
                            });
                        }
                    }
                    break;
                case "BangGia":
                    {
                        string name = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên bảng giá mới:", "Thêm bảng giá", "Bảng giá mới");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _ = Task.Run(async () =>
                            {
                                using (var conn = DbConnectionManager.GetConnection())
                                {
                                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                                    await conn.ExecuteAsync("INSERT INTO DBANGGIA (ID, NAME, STATUS) VALUES (@Id, @Name, 30)", new { Id = Guid.NewGuid().ToString(), Name = name.Trim() });
                                }
                                await Dispatcher.InvokeAsync(async () => { await LoadTreeAsync(); });
                            });
                        }
                    }
                    break;
                case "KhoNhap":
                default:
                    {
                        var win = new ThemKhoHangWindow();
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () => { await LoadTreeAsync(); await LoadLookupsAsync(); };
                        win.ShowDialog();
                    }
                    break;
            }
        }

        private void BtnSuaKho_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.Id == "ALL" || _selectedTreeItem.Id == "UNASSIGNED" || _selectedTreeItem.Id == "TRASH")
            {
                MessageBox.Show($"Vui lòng chọn {GetTreeItemTypeName()} cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (_currentTreeMode)
            {
                case "NhanVienNhap":
                    {
                        var win = new ThemNhanVienWindow(id: _selectedTreeItem.Id);
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () => { await LoadTreeAsync(); await LoadLookupsAsync(); };
                        win.ShowDialog();
                    }
                    break;
                case "CuaHang":
                    {
                        var win = new ThemCuaHangWindow(_selectedTreeItem.Id);
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () => { await LoadTreeAsync(); };
                        win.ShowDialog();
                    }
                    break;
                case "TaiKhoanNganHang":
                    {
                        string name = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên mới cho tài khoản ngân hàng:", "Sửa tài khoản ngân hàng", _selectedTreeItem.Name);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _ = Task.Run(async () =>
                            {
                                using (var conn = DbConnectionManager.GetConnection())
                                {
                                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                                    await conn.ExecuteAsync("UPDATE DTAIKHOANNGANHANG SET NAME = @Name WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = _selectedTreeItem.Id, Name = name.Trim() });
                                }
                                await Dispatcher.InvokeAsync(async () => { await LoadTreeAsync(); await LoadDataGridAsync(); });
                            });
                        }
                    }
                    break;
                case "BangGia":
                    {
                        string name = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên mới cho bảng giá:", "Sửa bảng giá", _selectedTreeItem.Name);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _ = Task.Run(async () =>
                            {
                                using (var conn = DbConnectionManager.GetConnection())
                                {
                                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                                    await conn.ExecuteAsync("UPDATE DBANGGIA SET NAME = @Name WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = _selectedTreeItem.Id, Name = name.Trim() });
                                }
                                await Dispatcher.InvokeAsync(async () => { await LoadTreeAsync(); await LoadDataGridAsync(); });
                            });
                        }
                    }
                    break;
                case "KhoNhap":
                default:
                    {
                        var win = new ThemKhoHangWindow(_selectedTreeItem);
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () => { await LoadTreeAsync(); await LoadLookupsAsync(); };
                        win.ShowDialog();
                    }
                    break;
            }
        }

        private string GetTreeItemTypeName()
        {
            return _currentTreeMode switch
            {
                "NhanVienNhap" => "nhân viên",
                "CuaHang" => "cửa hàng",
                "TaiKhoanNganHang" => "tài khoản ngân hàng",
                "BangGia" => "bảng giá",
                _ => "kho hàng"
            };
        }

        private void BtnThemThuMucKho_Click(object sender, RoutedEventArgs e)
        {
            BtnThemKho_Click(sender, e);
        }

        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadTreeAsync();
        }

        private async void BtnXoaKho_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.Id == "ALL" || _selectedTreeItem.Id == "UNASSIGNED" || _selectedTreeItem.Id == "TRASH")
            {
                MessageBox.Show("Vui lòng chọn kho hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa kho '{_selectedTreeItem.Name}' không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await LocalKhoHangService.DeleteKhoHangAsync(_selectedTreeItem.Id);
                await LoadTreeAsync();
            }
        }

        private async void BtnLamMoiKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
        }

        private void CmTreeView_Opened(object sender, RoutedEventArgs e)
        {
            // Update tree context menu state if needed
        }

        private void MiThemMoiItem_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemNhanhGoc_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemThuMucGoc_Click(object sender, RoutedEventArgs e) => BtnThemThuMucKho_Click(sender, e);
        private void MiThemConItem_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemNhanhCon_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemThuMucCon_Click(object sender, RoutedEventArgs e) => BtnThemThuMucKho_Click(sender, e);
        private void MiChinhSua_Click(object sender, RoutedEventArgs e) => BtnSuaKho_Click(sender, e);
        private void MiSapXepTen_Click(object sender, RoutedEventArgs e) => _ = LoadTreeAsync();
        private void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e) => _ = LoadTreeAsync();
        private void MiSaoChep_Click(object sender, RoutedEventArgs e) => BtnSuaKho_Click(sender, e);
        private void MiMoRong_Click(object sender, RoutedEventArgs e) { }
        private void MiThuGon_Click(object sender, RoutedEventArgs e) { }
        private void MiKhoiPhucTree_Click(object sender, RoutedEventArgs e) { }
        private void MiDoiTen_Click(object sender, RoutedEventArgs e) => BtnSuaKho_Click(sender, e);
        private void MiThungRac_Click(object sender, RoutedEventArgs e) { }
        private void MiBieuTuong_Click(object sender, RoutedEventArgs e) => BtnSuaKho_Click(sender, e);
        private void MiThuocTinh_Click(object sender, RoutedEventArgs e) => BtnSuaKho_Click(sender, e);
        #endregion

        #region DataGrid ContextMenu Handlers
        private DataGridCell _clickedCell;
        private DataGridColumn _clickedColumn;
        private string _clickedCellValue = "";

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t)
                    return t;
                T childItem = FindVisualChild<T>(child);
                if (childItem != null)
                    return childItem;
            }
            return null;
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    DgPhieuNhap.SelectedItems.Clear();
                    row.IsSelected = true;
                }
                row.Focus();

                var hit = System.Windows.Media.VisualTreeHelper.HitTest(row, e.GetPosition(row));
                if (hit != null)
                {
                    DependencyObject dep = hit.VisualHit;
                    while (dep != null && !(dep is DataGridCell))
                    {
                        dep = System.Windows.Media.VisualTreeHelper.GetParent(dep);
                    }
                    if (dep is DataGridCell cell)
                    {
                        _clickedCell = cell;
                        _clickedColumn = cell.Column;
                        if (cell.Content is TextBlock tb)
                        {
                            _clickedCellValue = tb.Text?.Trim() ?? "";
                        }
                        else if (cell.Content is FrameworkElement fe)
                        {
                            var innerTb = FindVisualChild<TextBlock>(fe);
                            _clickedCellValue = innerTb?.Text?.Trim() ?? "";
                        }
                        else
                        {
                            _clickedCellValue = "";
                        }
                    }
                }
            }
        }

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool isTrash = _selectedTreeItem != null && _selectedTreeItem.Id == "TRASH";
            if (MenuKhoiPhuc != null) MenuKhoiPhuc.Visibility = isTrash ? Visibility.Visible : Visibility.Collapsed;
            if (MenuThemMoi != null) MenuThemMoi.Visibility = isTrash ? Visibility.Collapsed : Visibility.Visible;
            if (MenuThemNhanhExcel != null) MenuThemNhanhExcel.Visibility = isTrash ? Visibility.Collapsed : Visibility.Visible;
            if (MenuCapNhatNhanhExcel != null) MenuCapNhatNhanhExcel.Visibility = isTrash ? Visibility.Collapsed : Visibility.Visible;
            if (MenuChinhSua != null) MenuChinhSua.Visibility = isTrash ? Visibility.Collapsed : Visibility.Visible;

            string colHeader = _clickedColumn?.Header?.ToString() ?? "Số phiếu";
            if (MenuLocCot != null)
            {
                MenuLocCot.Header = $"Lọc {colHeader}";
            }
        }

        private void MenuLocCot_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                TxtTimKiem.Text = _clickedCellValue;
                TxtTimKiem.Focus();
                TxtTimKiem.SelectAll();
            }
        }

        private async void MenuItem_KhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuNhap == null) return;
            bool ok = await LocalNhapKhoService.RestorePhieuNhapAsync(_selectedPhieuNhap.Id);
            if (ok)
            {
                await LoadDataGridAsync();
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null)
            {
                DgPhieuNhap.ItemsSource = list.OrderBy(x => x.SoPhieu).ToList();
            }
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null)
            {
                DgPhieuNhap.ItemsSource = list.OrderByDescending(x => x.SoPhieu).ToList();
            }
        }

        private void MenuItem_SortBySoPhieu_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null) DgPhieuNhap.ItemsSource = list.OrderBy(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortByNgay_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null) DgPhieuNhap.ItemsSource = list.OrderByDescending(x => x.Ngay).ToList();
        }

        private void MenuItem_SortByNcc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null) DgPhieuNhap.ItemsSource = list.OrderBy(x => x.TenNhaCungCap).ToList();
        }

        private void MenuItem_SortByKho_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null) DgPhieuNhap.ItemsSource = list.OrderBy(x => x.TenKhoNhap).ToList();
        }

        private void MenuItem_SortByNv_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null) DgPhieuNhap.ItemsSource = list.OrderBy(x => x.TenNhanVienNhap).ToList();
        }

        private void MenuItem_SortByTongCong_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuNhap.ItemsSource as List<PhieuNhapItem>;
            if (list != null) DgPhieuNhap.ItemsSource = list.OrderByDescending(x => x.TongCong).ToList();
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuNhap != null)
            {
                Clipboard.SetText(_selectedPhieuNhap.SoPhieu);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuNhap != null)
            {
                Clipboard.SetText($"{_selectedPhieuNhap.SoPhieu}\t{_selectedPhieuNhap.NgayHienThi}\t{_selectedPhieuNhap.TenNhaCungCap}\t{_selectedPhieuNhap.TongCong:N0}");
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgPhieuNhap.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgPhieuNhap, "Phiếu nhập");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }
        #endregion

        private void QuanLyNhapKhoControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
            {
                BtnThemMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                BtnChinhSua_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                BtnXoa_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                _ = LoadDataGridAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                TxtTimKiem.Focus();
                TxtTimKiem.SelectAll();
                e.Handled = true;
            }
        }

        #region Chế độ phân nhóm (Kho nhập, Nhân viên nhập, Cửa hàng, Tài khoản NH, Bảng giá)
        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            if (BtnMenuCheDo.ContextMenu != null)
            {
                BtnMenuCheDo.ContextMenu.PlacementTarget = BtnMenuCheDo;
                BtnMenuCheDo.ContextMenu.IsOpen = true;
            }
        }

        private async void MiCheDoKhoNhap_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "KhoNhap";
            TxtTreeHeader.Text = "Kho hàng";
            _selectedTreeItem = null;
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private async void MiCheDoNhanVienNhap_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "NhanVienNhap";
            TxtTreeHeader.Text = "Nhân viên";
            _selectedTreeItem = null;
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private async void MiCheDoCuaHang_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "CuaHang";
            TxtTreeHeader.Text = "Cửa hàng";
            _selectedTreeItem = null;
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private async void MiCheDoTaiKhoanNganHang_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "TaiKhoanNganHang";
            TxtTreeHeader.Text = "Tài khoản ngân hàng";
            _selectedTreeItem = null;
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private async void MiCheDoBangGia_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "BangGia";
            TxtTreeHeader.Text = "Bảng giá";
            _selectedTreeItem = null;
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }
        #endregion
    }
}
