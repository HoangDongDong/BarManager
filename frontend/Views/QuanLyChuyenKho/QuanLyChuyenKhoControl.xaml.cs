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

namespace QuanLyBar.Client.Views.QuanLyChuyenKho
{
    public partial class QuanLyChuyenKhoControl : UserControl
    {
        private ObservableCollection<KhoHangTreeItem> _treeItems = new();
        private List<PhieuChuyenKhoItem> _allPhieuChuyen = new();
        private KhoHangTreeItem _selectedTreeItem;
        private PhieuChuyenKhoItem _selectedPhieuChuyen;

        private string _currentTreeMode = "KhoXuat"; // KhoXuat, KhoNhap, NhanVienXuat, NhanVienNhap, CuaHang, TaiKhoanNganHang, BangGia

        public QuanLyChuyenKhoControl()
        {
            InitializeComponent();
            Loaded += QuanLyChuyenKhoControl_Loaded;
            PreviewKeyDown += QuanLyChuyenKhoControl_PreviewKeyDown;
        }

        private async void QuanLyChuyenKhoControl_Loaded(object sender, RoutedEventArgs e)
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
            
            var khoNhapCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            khoNhapCombo.AddRange(khoList);
            CboKhoNhap.ItemsSource = khoNhapCombo;
            CboKhoNhap.SelectedIndex = 0;

            var khoXuatCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            khoXuatCombo.AddRange(khoList);
            CboKhoXuat.ItemsSource = khoXuatCombo;
            CboKhoXuat.SelectedIndex = 0;

            var nvList = await LocalNhapKhoService.GetNhanVienLookupListAsync();

            var nvNhapCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            nvNhapCombo.AddRange(nvList);
            CboNhanVienNhap.ItemsSource = nvNhapCombo;
            CboNhanVienNhap.SelectedIndex = 0;

            var nvXuatCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            nvXuatCombo.AddRange(nvList);
            CboNhanVienXuat.ItemsSource = nvXuatCombo;
            CboNhanVienXuat.SelectedIndex = 0;
        }

        public async Task LoadTreeAsync()
        {
            try
            {
                List<KhoHangTreeItem> tree;
                switch (_currentTreeMode)
                {
                    case "KhoNhap":
                        tree = await LocalNhapKhoService.GetKhoHangTreeAsync();
                        break;
                    case "NhanVienXuat":
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
                    case "KhoXuat":
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

                if (TvKhoHang.Items.Count > 0)
                {
                    var firstItem = TvKhoHang.Items[0] as KhoHangTreeItem;
                    if (firstItem != null)
                    {
                        firstItem.IsSelected = true;
                        _selectedTreeItem = firstItem;
                    }
                }
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
                DateTime? tuNgay = DpTuNgay.SelectedDate;
                DateTime? denNgay = DpDenNgay.SelectedDate;

                string khoNhapId = CboKhoNhap.SelectedValue?.ToString();
                string khoXuatId = CboKhoXuat.SelectedValue?.ToString();
                string nvNhapId = CboNhanVienNhap.SelectedValue?.ToString();
                string nvXuatId = CboNhanVienXuat.SelectedValue?.ToString();
                string cuaHangId = null;
                string taiKhoanNganHangId = null;
                string bangGiaId = null;
                bool isTrash = false;
                string treeFilterKhoId = null;

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
                            case "KhoXuat":
                                khoXuatId = _selectedTreeItem.Id;
                                break;
                            case "KhoNhap":
                                khoNhapId = _selectedTreeItem.Id;
                                break;
                            case "NhanVienXuat":
                                nvXuatId = _selectedTreeItem.Id;
                                break;
                            case "NhanVienNhap":
                                nvNhapId = _selectedTreeItem.Id;
                                break;
                            case "CuaHang":
                                cuaHangId = _selectedTreeItem.Id;
                                break;
                            case "TaiKhoanNganHang":
                                taiKhoanNganHangId = _selectedTreeItem.Id;
                                break;
                            case "BangGia":
                                bangGiaId = _selectedTreeItem.Id;
                                break;
                        }
                    }
                }

                string filterText = TxtSearch.Text?.Trim();

                _allPhieuChuyen = await LocalChuyenKhoService.GetPhieuChuyenKhoListAsync(
                    tuNgay, denNgay, khoXuatId, khoNhapId, nvXuatId, nvNhapId, filterText, treeFilterKhoId, isTrash,
                    cuaHangId, taiKhoanNganHangId, bangGiaId);

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phiếu chuyển kho: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            string search = TxtSearch.Text?.Trim().ToLowerInvariant() ?? "";
            var filtered = _allPhieuChuyen;

            if (!string.IsNullOrEmpty(search))
            {
                filtered = _allPhieuChuyen.Where(x =>
                    x.SoPhieu.ToLowerInvariant().Contains(search) ||
                    x.TenKhoXuat.ToLowerInvariant().Contains(search) ||
                    x.TenKhoNhap.ToLowerInvariant().Contains(search) ||
                    x.TenNhanVienXuat.ToLowerInvariant().Contains(search) ||
                    x.TenNhanVienNhap.ToLowerInvariant().Contains(search) ||
                    x.DienGiai.ToLowerInvariant().Contains(search) ||
                    x.Note.ToLowerInvariant().Contains(search)
                ).ToList();
            }

            int stt = 1;
            foreach (var item in filtered)
            {
                item.Stt = stt++;
            }

            DgPhieuChuyen.ItemsSource = filtered;

            if (filtered.Count > 0)
            {
                DgPhieuChuyen.SelectedIndex = 0;
            }
            else
            {
                _selectedPhieuChuyen = null;
                UpdateBottomTabs(null);
            }
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                _ = LoadDataGridAsync();
            }
        }

        private void CboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                _ = LoadDataGridAsync();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TvKhoHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as KhoHangTreeItem;
            _ = LoadDataGridAsync();
        }

        private void DgPhieuChuyen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPhieuChuyen = DgPhieuChuyen.SelectedItem as PhieuChuyenKhoItem;
            UpdateBottomTabs(_selectedPhieuChuyen);
        }

        private async void UpdateBottomTabs(PhieuChuyenKhoItem item)
        {
            if (item == null)
            {
                TxtTimeCreated.Text = "";
                TxtUserCreated.Text = "";
                TxtTimeModified.Text = "";
                TxtUserModified.Text = "";
                DgChuyenKhoChiTietSub.ItemsSource = null;
                DgPhieuThu.ItemsSource = null;
                DgPhieuChi.ItemsSource = null;
                DgPhieuThuCongNo.ItemsSource = null;
                DgDonHangChiTietGio.ItemsSource = null;
                DgInCheBien.ItemsSource = null;
                DgPhieuThu2.ItemsSource = null;
                DgPhieuChi2.ItemsSource = null;
                DgPhieuThuCongNo2.ItemsSource = null;
                return;
            }

            TxtTimeCreated.Text = item.TimeCreatedHienThi;
            TxtUserCreated.Text = item.UserCreatedName;
            TxtTimeModified.Text = item.TimeModifiedHienThi;
            TxtUserModified.Text = item.UserModifiedName;

            try
            {
                var details = await LocalChuyenKhoService.GetPhieuChuyenKhoChiTietAsync(item.Id);
                DgChuyenKhoChiTietSub.ItemsSource = details;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateBottomTabs error: " + ex.Message);
            }
        }

        #region Chế độ phân nhóm (Menu Context)
        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            if (BtnMenuCheDo.ContextMenu != null)
            {
                BtnMenuCheDo.ContextMenu.PlacementTarget = BtnMenuCheDo;
                BtnMenuCheDo.ContextMenu.IsOpen = true;
            }
        }

        private void SwitchTreeMode(string mode, string headerTitle)
        {
            _currentTreeMode = mode;
            if (TxtHeaderPhanNhom != null) TxtHeaderPhanNhom.Text = headerTitle;
            _ = LoadTreeAsync();
            _ = LoadDataGridAsync();
        }

        private void MiCheDoKhoXuat_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("KhoXuat", "Kho xuất");
        }

        private void MiCheDoKhoNhap_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("KhoNhap", "Kho nhập");
        }

        private void MiCheDoNhanVienXuat_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("NhanVienXuat", "Nhân viên xuất");
        }

        private void MiCheDoNhanVienNhap_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("NhanVienNhap", "Nhân viên nhập");
        }

        private void MiCheDoCuaHang_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("CuaHang", "Cửa hàng");
        }

        private void MiCheDoTaiKhoanNganHang_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("TaiKhoanNganHang", "Tài khoản ngân hàng");
        }

        private void MiCheDoBangGia_Click(object sender, RoutedEventArgs e)
        {
            SwitchTreeMode("BangGia", "Bảng giá");
        }
        #endregion

        #region Thao tác Thêm / Sửa / Danh mục trên cây
        private void BtnThemKho_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentTreeMode)
            {
                case "NhanVienXuat":
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
                case "KhoXuat":
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
                MessageBox.Show($"Vui lòng chọn mục cần sửa trên cây!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (_currentTreeMode)
            {
                case "NhanVienXuat":
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
                case "KhoXuat":
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

        private void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            string folderName = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên thư mục mới:", "Thêm thư mục kho hàng", "Thư mục mới");
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                _ = Task.Run(async () =>
                {
                    await LocalKhoHangService.SaveKhoHangAsync(new KhoHangTreeItem
                    {
                        Name = folderName.Trim(),
                        ItemType = "1",
                        Status = true
                    }, true);
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        await LoadTreeAsync();
                    });
                });
            }
        }

        private void BtnDanhMucKho_Click(object sender, RoutedEventArgs e)
        {
            var win = new DanhMucKhoHangWindow();
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnRefreshTree_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadTreeAsync();
            _ = LoadDataGridAsync();
        }
        #endregion

        #region Actions Toolbar
        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ThemPhieuChuyenKhoWindow(allPhieuChuyen: _allPhieuChuyen);
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
                MessageBox.Show("Lỗi mở màn hình Thêm mới phiếu chuyển kho: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChuyen == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu chuyển kho để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var win = new ThemPhieuChuyenKhoWindow(_selectedPhieuChuyen.Id, _allPhieuChuyen);
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
                MessageBox.Show("Lỗi mở màn hình Chỉnh sửa phiếu chuyển kho: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChuyen == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu chuyển kho cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isTrash = (_selectedTreeItem?.Id == "TRASH");
            string confirmMsg = isTrash
                ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN phiếu chuyển '{_selectedPhieuChuyen.SoPhieu}' khỏi hệ thống?"
                : $"Bạn có chắc chắn muốn chuyển phiếu chuyển '{_selectedPhieuChuyen.SoPhieu}' vào Thùng rác?";

            if (MessageBox.Show(confirmMsg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                bool ok = await LocalChuyenKhoService.DeletePhieuChuyenKhoAsync(_selectedPhieuChuyen.Id, permanent: isTrash);
                if (ok)
                {
                    MessageBox.Show("Đã xóa phiếu chuyển kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataGridAsync();
                }
                else
                {
                    MessageBox.Show("Xóa phiếu chuyển kho không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgPhieuChuyen_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPhieuChuyen != null)
            {
                BtnChinhSua_Click(sender, e);
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm phiếu chuyển kho từ Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất danh sách phiếu chuyển kho ra Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng in danh sách phiếu chuyển kho đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuChuyen.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu phiếu chuyển kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal tongCong = _allPhieuChuyen.Sum(x => x.TongCong);
            MessageBox.Show($"Tổng cộng tiền chuyển kho ({_allPhieuChuyen.Count} phiếu): {tongCong:N0} VNĐ", "Tổng cộng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPhanTich_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng phân tích dữ liệu chuyển kho đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Tree Context Menu Handlers
        private void CmTreeView_Opened(object sender, RoutedEventArgs e)
        {
        }

        private async void BtnLamMoiKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
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

        private void MiThemMoiItem_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemNhanhGoc_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemThuMucGoc_Click(object sender, RoutedEventArgs e) => BtnThemThuMuc_Click(sender, e);
        private void MiThemConItem_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemNhanhCon_Click(object sender, RoutedEventArgs e) => BtnThemKho_Click(sender, e);
        private void MiThemThuMucCon_Click(object sender, RoutedEventArgs e) => BtnThemThuMuc_Click(sender, e);
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
                    DgPhieuChuyen.SelectedItems.Clear();
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
                TxtSearch.Text = _clickedCellValue;
                TxtSearch.Focus();
                TxtSearch.SelectAll();
            }
        }

        private async void MenuItem_KhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChuyen == null) return;
            bool ok = await LocalChuyenKhoService.RestorePhieuChuyenKhoAsync(_selectedPhieuChuyen.Id);
            if (ok)
            {
                await LoadDataGridAsync();
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderBy(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderByDescending(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortBySoPhieu_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderBy(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortByNgay_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderByDescending(x => x.Ngay).ToList();
        }

        private void MenuItem_SortByKhoXuat_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderBy(x => x.TenKhoXuat).ToList();
        }

        private void MenuItem_SortByKhoNhap_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderBy(x => x.TenKhoNhap).ToList();
        }

        private void MenuItem_SortByNv_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderBy(x => x.TenNhanVienXuat).ToList();
        }

        private void MenuItem_SortByTongCong_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuChuyen.ItemsSource as List<PhieuChuyenKhoItem>;
            if (list != null) DgPhieuChuyen.ItemsSource = list.OrderByDescending(x => x.TongCong).ToList();
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChuyen != null)
            {
                Clipboard.SetText(_selectedPhieuChuyen.SoPhieu);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChuyen != null)
            {
                Clipboard.SetText($"{_selectedPhieuChuyen.SoPhieu}\t{_selectedPhieuChuyen.NgayHienThi}\t{_selectedPhieuChuyen.TenKhoXuat}\t{_selectedPhieuChuyen.TenKhoNhap}\t{_selectedPhieuChuyen.TongCong:N0}");
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgPhieuChuyen.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgPhieuChuyen, "Phiếu chuyển kho");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            BtnChinhSua_Click(sender, e);
        }
        #endregion

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataGridAsync();
        }

        private void QuanLyChuyenKhoControl_PreviewKeyDown(object sender, KeyEventArgs e)
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
                TxtSearch.Focus();
                TxtSearch.SelectAll();
                e.Handled = true;
            }
        }
        #endregion
    }
}
