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

namespace QuanLyBar.Client.Views.QuanLyXuatKho
{
    public partial class QuanLyXuatKhoControl : UserControl
    {
        private ObservableCollection<KhoHangTreeItem> _treeItems = new();
        private List<PhieuXuatItem> _allPhieuXuat = new();
        private KhoHangTreeItem _selectedTreeItem;
        private PhieuXuatItem _selectedPhieuXuat;

        public QuanLyXuatKhoControl()
        {
            InitializeComponent();
            Loaded += QuanLyXuatKhoControl_Loaded;
            PreviewKeyDown += QuanLyXuatKhoControl_PreviewKeyDown;
        }

        private async void QuanLyXuatKhoControl_Loaded(object sender, RoutedEventArgs e)
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
            CboKhoXuat.ItemsSource = khoCombo;
            CboKhoXuat.SelectedIndex = 0;

            var nvList = await LocalNhapKhoService.GetNhanVienLookupListAsync();
            var nvCombo = new List<NhapKhoLookupItem> { new NhapKhoLookupItem { Id = "", Name = "-- Tất cả --" } };
            nvCombo.AddRange(nvList);
            CboNhanVienXuat.ItemsSource = nvCombo;
            CboNhanVienXuat.SelectedIndex = 0;
        }

        private string _currentTreeMode = "KhoXuat";

        public async Task LoadTreeAsync()
        {
            try
            {
                List<KhoHangTreeItem> tree;
                switch (_currentTreeMode)
                {
                    case "NhanVienXuat":
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
                string nvId = CboNhanVienXuat.SelectedValue?.ToString();
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
                            case "KhoXuat":
                                khoId = _selectedTreeItem.Id;
                                break;
                            case "NhanVienXuat":
                                nvId = _selectedTreeItem.Id;
                                break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(CboKhoXuat.SelectedValue?.ToString()))
                {
                    khoId = CboKhoXuat.SelectedValue.ToString();
                }

                _allPhieuXuat = await LocalXuatKhoService.GetPhieuXuatListAsync(
                    DpTuNgay.SelectedDate,
                    DpDenNgay.SelectedDate,
                    khoId,
                    nvId,
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
            if (_allPhieuXuat == null) return;

            string keyword = TxtLoc.Text?.Trim().ToLowerInvariant() ?? "";
            var filtered = _allPhieuXuat.AsEnumerable();

            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = filtered.Where(x =>
                    (x.SoPhieu != null && x.SoPhieu.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenKhoXuat != null && x.TenKhoXuat.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenNhanVienXuat != null && x.TenNhanVienXuat.ToLowerInvariant().Contains(keyword)) ||
                    (x.TenKhachHang != null && x.TenKhachHang.ToLowerInvariant().Contains(keyword)) ||
                    (x.Note != null && x.Note.ToLowerInvariant().Contains(keyword))
                );
            }

            var list = filtered.ToList();
            int stt = 1;
            foreach (var item in list) item.Stt = stt++;

            DgPhieuXuat.ItemsSource = list;

            if (list.Count > 0)
            {
                DgPhieuXuat.SelectedIndex = 0;
                _selectedPhieuXuat = list[0];
            }
            else
            {
                _selectedPhieuXuat = null;
            }

            UpdateDetailTabs();
        }

        private void UpdateDetailTabs()
        {
            if (_selectedPhieuXuat != null)
            {
                TxtDetailTimeCreated.Text = _selectedPhieuXuat.TimeCreatedHienThi;
                TxtDetailUserCreated.Text = string.IsNullOrEmpty(_selectedPhieuXuat.UserCreatedName) ? "Administrator" : _selectedPhieuXuat.UserCreatedName;
                TxtDetailTimeModified.Text = _selectedPhieuXuat.TimeModifiedHienThi;
                TxtDetailUserModified.Text = _selectedPhieuXuat.UserModifiedName ?? "";

                _ = LoadDetailsAsync(_selectedPhieuXuat.Id);
            }
            else
            {
                TxtDetailTimeCreated.Text = "";
                TxtDetailUserCreated.Text = "";
                TxtDetailTimeModified.Text = "";
                TxtDetailUserModified.Text = "";
                DgXuatKhoChiTiet.ItemsSource = null;
                DgPhieuThu.ItemsSource = null;
                DgPhieuChi.ItemsSource = null;
                DgPhieuThuCongNo.ItemsSource = null;
                DgDonHangChiTietGio.ItemsSource = null;
                DgInCheBien.ItemsSource = null;
                DgPhieuThu2.ItemsSource = null;
                DgPhieuChi2.ItemsSource = null;
                DgPhieuThuCongNo2.ItemsSource = null;
            }
        }

        private async Task LoadDetailsAsync(string phieuXuatId)
        {
            try
            {
                var details = await LocalXuatKhoService.GetPhieuXuatChiTietAsync(phieuXuatId);
                DgXuatKhoChiTiet.ItemsSource = details;
            }
            catch { }
        }

        private void DgPhieuXuat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPhieuXuat = DgPhieuXuat.SelectedItem as PhieuXuatItem;
            UpdateDetailTabs();
        }

        private void DgPhieuXuat_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPhieuXuat != null)
            {
                BtnChinhSua_Click(sender, e);
            }
        }

        private async void TvKhoHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as KhoHangTreeItem;
            await LoadDataGridAsync();
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            await LoadDataGridAsync();
        }

        private void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ThemPhieuXuatKhoWindow(allPhieuXuat: _allPhieuXuat);
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
                MessageBox.Show("Lỗi mở màn hình Thêm mới phiếu xuất: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuXuat == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu xuất để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var win = new ThemPhieuXuatKhoWindow(_selectedPhieuXuat.Id, _allPhieuXuat);
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
                MessageBox.Show("Lỗi mở màn hình Chỉnh sửa phiếu xuất: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuXuat == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu xuất cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isTrash = (_selectedTreeItem?.Id == "TRASH");
            string confirmMsg = isTrash
                ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN phiếu xuất '{_selectedPhieuXuat.SoPhieu}'?"
                : $"Bạn có chắc chắn muốn chuyển phiếu xuất '{_selectedPhieuXuat.SoPhieu}' vào Thùng rác?";

            if (MessageBox.Show(confirmMsg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool ok = await LocalXuatKhoService.DeletePhieuXuatAsync(_selectedPhieuXuat.Id, isTrash);
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
            if (_allPhieuXuat == null || _allPhieuXuat.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu phiếu xuất để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"DanhSachPhieuXuat_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("PhieuXuat");
                        string[] headers = new[] { "STT", "Ngày", "Kho xuất", "Nhân viên xuất", "Tổng cộng", "Số phiếu", "Tiền giảm giá", "Tỉ lệ giảm giá", "Tiền hàng", "Cửa hàng", "Còn lại", "Thanh toán", "Tài khoản ngân hàng", "Ghi chú" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            ws.Cell(1, i + 1).Value = headers[i];
                            ws.Cell(1, i + 1).Style.Font.Bold = true;
                            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                        }

                        int r = 2;
                        foreach (var item in _allPhieuXuat)
                        {
                            ws.Cell(r, 1).Value = item.Stt;
                            ws.Cell(r, 2).Value = item.NgayHienThi;
                            ws.Cell(r, 3).Value = item.TenKhoXuat;
                            ws.Cell(r, 4).Value = item.TenNhanVienXuat;
                            ws.Cell(r, 5).Value = item.TongCong;
                            ws.Cell(r, 6).Value = item.SoPhieu;
                            ws.Cell(r, 7).Value = item.TienGiamGia;
                            ws.Cell(r, 8).Value = item.TiLeGiamGia;
                            ws.Cell(r, 9).Value = item.TienHang;
                            ws.Cell(r, 10).Value = item.TenCuaHang;
                            ws.Cell(r, 11).Value = item.ConLai;
                            ws.Cell(r, 12).Value = item.ThanhToan;
                            ws.Cell(r, 13).Value = item.TenTaiKhoanNganHang;
                            ws.Cell(r, 14).Value = item.Note;
                            r++;
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show($"Đã xuất thành công {_allPhieuXuat.Count} phiếu xuất ra file Excel!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgPhieuXuat, "Phiếu xuất kho");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            if (_allPhieuXuat == null || _allPhieuXuat.Count == 0)
            {
                MessageBox.Show("Không có phiếu xuất nào.", "Tổng hợp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal total = _allPhieuXuat.Sum(x => x.TongCong);
            MessageBox.Show($"Tổng số phiếu xuất: {_allPhieuXuat.Count}\nTổng tiền: {total:N0} đ", "Tổng hợp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPhanTich_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng phân tích phiếu xuất đang được phát triển!", "Phân tích", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCauHinhKho_Click(object sender, MouseButtonEventArgs e)
        {
            if (_currentTreeMode == "KhoXuat")
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
                case "NhanVienXuat":
                    {
                        var win = new ThemNhanVienWindow();
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () =>
                        {
                            await LoadTreeAsync();
                            await LoadLookupsAsync();
                        };
                        win.ShowDialog();
                    }
                    break;
                case "CuaHang":
                    {
                        var win = new ThemCuaHangWindow();
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () =>
                        {
                            await LoadTreeAsync();
                        };
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
                default:
                    {
                        var win = new ThemKhoHangWindow();
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () =>
                        {
                            await LoadTreeAsync();
                            await LoadLookupsAsync();
                        };
                        win.ShowDialog();
                    }
                    break;
            }
        }

        private void BtnSuaKho_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreeItem == null || _selectedTreeItem.Id == "ALL" || _selectedTreeItem.Id == "TRASH" || _selectedTreeItem.Id == "UNASSIGNED")
            {
                MessageBox.Show($"Vui lòng chọn {GetTreeItemTypeName()} cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (_currentTreeMode)
            {
                case "NhanVienXuat":
                    {
                        var win = new ThemNhanVienWindow(id: _selectedTreeItem.Id);
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () =>
                        {
                            await LoadTreeAsync();
                            await LoadLookupsAsync();
                        };
                        win.ShowDialog();
                    }
                    break;
                case "CuaHang":
                    {
                        var win = new ThemCuaHangWindow(_selectedTreeItem.Id);
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () =>
                        {
                            await LoadTreeAsync();
                        };
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
                default:
                    {
                        var win = new ThemKhoHangWindow(_selectedTreeItem);
                        win.Owner = Window.GetWindow(this);
                        win.OnSaved += async () =>
                        {
                            await LoadTreeAsync();
                            await LoadLookupsAsync();
                        };
                        win.ShowDialog();
                    }
                    break;
            }
        }

        private string GetTreeItemTypeName()
        {
            return _currentTreeMode switch
            {
                "NhanVienXuat" => "nhân viên",
                "CuaHang" => "cửa hàng",
                "TaiKhoanNganHang" => "tài khoản ngân hàng",
                "BangGia" => "bảng giá",
                _ => "kho hàng"
            };
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

        private bool _isFlatMode = false;
        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _isFlatMode = !_isFlatMode;
            if (_isFlatMode)
            {
                BtnXemTheoThuMuc.Background = System.Windows.Media.Brushes.LightYellow;
                BtnXemTheoThuMuc.BorderBrush = System.Windows.Media.Brushes.Goldenrod;
            }
            else
            {
                BtnXemTheoThuMuc.Background = System.Windows.Media.Brushes.Transparent;
                BtnXemTheoThuMuc.BorderBrush = System.Windows.Media.Brushes.Transparent;
            }
        }

        private async void BtnTaiLaiKho_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            if (BtnMenuCheDo.ContextMenu != null)
            {
                BtnMenuCheDo.ContextMenu.PlacementTarget = BtnMenuCheDo;
                BtnMenuCheDo.ContextMenu.IsOpen = true;
            }
        }

        #region Kho Tree Action Buttons & ContextMenu Handlers
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

        private void CmTreeView_Opened(object sender, RoutedEventArgs e)
        {
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
                    DgPhieuXuat.SelectedItems.Clear();
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
                TxtLoc.Text = _clickedCellValue;
                TxtLoc.Focus();
                TxtLoc.SelectAll();
            }
        }

        private async void MenuItem_KhoiPhuc_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuXuat == null) return;
            bool ok = await LocalXuatKhoService.RestorePhieuXuatAsync(_selectedPhieuXuat.Id);
            if (ok)
            {
                await LoadDataGridAsync();
            }
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderBy(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderByDescending(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortBySoPhieu_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderBy(x => x.SoPhieu).ToList();
        }

        private void MenuItem_SortByNgay_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderByDescending(x => x.Ngay).ToList();
        }

        private void MenuItem_SortByKhachHang_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderBy(x => x.TenKhachHang).ToList();
        }

        private void MenuItem_SortByKho_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderBy(x => x.TenKhoXuat).ToList();
        }

        private void MenuItem_SortByNv_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderBy(x => x.TenNhanVienXuat).ToList();
        }

        private void MenuItem_SortByTongCong_Click(object sender, RoutedEventArgs e)
        {
            var list = DgPhieuXuat.ItemsSource as List<PhieuXuatItem>;
            if (list != null) DgPhieuXuat.ItemsSource = list.OrderByDescending(x => x.TongCong).ToList();
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuXuat != null)
            {
                Clipboard.SetText(_selectedPhieuXuat.SoPhieu);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuXuat != null)
            {
                Clipboard.SetText($"{_selectedPhieuXuat.SoPhieu}\t{_selectedPhieuXuat.NgayHienThi}\t{_selectedPhieuXuat.TenKhachHang}\t{_selectedPhieuXuat.TongCong:N0}");
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgPhieuXuat.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgPhieuXuat, "Phiếu xuất");
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

        private void QuanLyXuatKhoControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                TxtLoc.Focus();
                TxtLoc.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                _ = LoadDataGridAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.Insert)
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
        }

        #region Chế độ phân nhóm (Kho xuất, Nhân viên xuất, Cửa hàng, Tài khoản NH, Bảng giá)
        private async void MiCheDoKhoXuat_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "KhoXuat";
            TxtTreeHeader.Text = "Kho hàng";
            _selectedTreeItem = null;
            await LoadTreeAsync();
            await LoadDataGridAsync();
        }

        private async void MiCheDoNhanVienXuat_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = "NhanVienXuat";
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
