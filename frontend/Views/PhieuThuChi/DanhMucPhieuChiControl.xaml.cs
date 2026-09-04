using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.DanhMucLyDoThuChi;

namespace QuanLyBar.Client.Views.PhieuThuChi
{
    public partial class DanhMucPhieuChiControl : UserControl
    {
        private int _currentTreeMode = 0; // 0: LyDo, 1: CuaHang, 2: TaiKhoan
        private string _selectedLyDoId = null;
        private string _selectedCuaHangId = null;
        private string _selectedTaiKhoanId = null;
        private bool _isTrashSelected = false;
        private bool _isLoaded = false;

        private List<PhieuThuChiGridItem> _allList = new List<PhieuThuChiGridItem>();

        public DanhMucPhieuChiControl()
        {
            InitializeComponent();

            TvFilter.PreviewMouseRightButtonDown += TvFilter_PreviewMouseRightButtonDown;

            Loaded += async (s, e) =>
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    DpToDate.SelectedDate = DateTime.Today;
                    DpFromDate.SelectedDate = DateTime.Today.AddDays(-30);

                    await LoadCuaHangFilterAsync();
                    await LoadTreeDataAsync();
                    await LoadDataGridAsync();
                }
            };
        }

        private void TvFilter_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as TreeViewItem;
        }

        private async Task LoadCuaHangFilterAsync()
        {
            try
            {
                var list = await LocalPhieuThuChiService.GetCuaHangLookupAsync();
                var fullList = new List<dynamic> { new { ID = (int?)null, NAME = "-- Tất cả cửa hàng --" } };
                fullList.AddRange(list);
                CboCuaHangFilter.ItemsSource = fullList;
                CboCuaHangFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private async Task LoadTreeDataAsync()
        {
            TvFilter.Items.Clear();
            try
            {
                if (_currentTreeMode == 0) // Lý do thu chi
                {
                    TxtLeftHeaderTitle.Text = "Lý do thu chi";
                    BtnThem.ToolTip = "Thêm lý do thu chi";
                    BtnSua.ToolTip = "Chỉnh sửa lý do thu chi";
                    if (MenuThemTreeCap1 != null) MenuThemTreeCap1.Header = "Thêm lý do thu chi";
                    if (MenuThemTreeCon != null) MenuThemTreeCon.Header = "Thêm lý do thu chi con";
                    if (MenuSuaTree != null) MenuSuaTree.Header = "Chỉnh sửa lý do thu chi";

                    var itemTatCa = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader("🌐", "Tất cả"),
                        Tag = "ALL",
                        IsSelected = (_selectedLyDoId == null && !_isTrashSelected)
                    };
                    TvFilter.Items.Add(itemTatCa);

                    var itemChuaTL = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader("✳️", "Chưa thiết lập"),
                        Tag = "NOT_SET",
                        IsSelected = (_selectedLyDoId == "NOT_SET")
                    };
                    TvFilter.Items.Add(itemChuaTL);

                    var reasons = await LocalLyDoThuChiService.GetLyDoThuChiListAsync(isTrash: false);
                    foreach (var r in reasons)
                    {
                        if (string.IsNullOrEmpty(r.Name) || r.ItemType == "SEPARATOR") continue;
                        string icon = !string.IsNullOrEmpty(r.IconText) ? r.IconText : "📄";
                        var tvi = new TreeViewItem
                        {
                            Header = CreateTreeItemHeader(icon, r.Name),
                            Tag = r.Id,
                            IsSelected = (r.Id == _selectedLyDoId?.ToString())
                        };
                        TvFilter.Items.Add(tvi);
                    }

                    var itemTrash = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader("🗑️", "Thùng rác"),
                        Tag = "TRASH",
                        IsSelected = _isTrashSelected
                    };
                    TvFilter.Items.Add(itemTrash);
                }
                else if (_currentTreeMode == 1) // Cửa hàng
                {
                    TxtLeftHeaderTitle.Text = "Cửa hàng";
                    BtnThem.ToolTip = "Thêm cửa hàng";
                    BtnSua.ToolTip = "Chỉnh sửa cửa hàng";
                    if (MenuThemTreeCap1 != null) MenuThemTreeCap1.Header = "Thêm cửa hàng";
                    if (MenuThemTreeCon != null) MenuThemTreeCon.Header = "Thêm cửa hàng";
                    if (MenuSuaTree != null) MenuSuaTree.Header = "Chỉnh sửa cửa hàng";

                    var itemTatCa = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader("🌐", "Tất cả cửa hàng"),
                        Tag = "ALL",
                        IsSelected = (_selectedCuaHangId == null)
                    };
                    TvFilter.Items.Add(itemTatCa);

                    var stores = await LocalPhieuThuChiService.GetCuaHangLookupAsync();
                    foreach (var s in stores)
                    {
                        var tvi = new TreeViewItem
                        {
                            Header = CreateTreeItemHeader("🏪", s.NAME?.ToString() ?? ""),
                            Tag = s.ID?.ToString(),
                            IsSelected = (s.ID?.ToString() == _selectedCuaHangId?.ToString())
                        };
                        TvFilter.Items.Add(tvi);
                    }
                }
                else if (_currentTreeMode == 2) // Tài khoản ngân hàng
                {
                    TxtLeftHeaderTitle.Text = "Tài khoản ngân hàng";
                    BtnThem.ToolTip = "Thêm tài khoản ngân hàng";
                    BtnSua.ToolTip = "Chỉnh sửa tài khoản ngân hàng";
                    if (MenuThemTreeCap1 != null) MenuThemTreeCap1.Header = "Thêm tài khoản ngân hàng";
                    if (MenuThemTreeCon != null) MenuThemTreeCon.Header = "Thêm tài khoản ngân hàng con";
                    if (MenuSuaTree != null) MenuSuaTree.Header = "Chỉnh sửa tài khoản ngân hàng";

                    var itemTatCa = new TreeViewItem
                    {
                        Header = CreateTreeItemHeader("🌐", "Tất cả tài khoản"),
                        Tag = "ALL",
                        IsSelected = (_selectedTaiKhoanId == null)
                    };
                    TvFilter.Items.Add(itemTatCa);

                    var accounts = await LocalPhieuThuChiService.GetTaiKhoanNganHangLookupAsync();
                    foreach (var acc in accounts)
                    {
                        var tvi = new TreeViewItem
                        {
                            Header = CreateTreeItemHeader("🏛️", acc.NAME?.ToString() ?? ""),
                            Tag = acc.ID?.ToString(),
                            IsSelected = (acc.ID?.ToString() == _selectedTaiKhoanId?.ToString())
                        };
                        TvFilter.Items.Add(tvi);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTreeDataAsync: " + ex.Message);
            }
        }

        private static StackPanel CreateTreeItemHeader(string icon, string text)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 1, 2, 1) };
            sp.Children.Add(new TextBlock { Text = icon, FontSize = 12, Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(new TextBlock { Text = text, FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center });
            return sp;
        }

        private async Task LoadDataGridAsync()
        {
            try
            {
                DateTime? fromDate = DpFromDate.SelectedDate;
                DateTime? toDate = DpToDate.SelectedDate;

                string cuaHangId = null;
                if (CboCuaHangFilter.SelectedValue != null)
                {
                    cuaHangId = CboCuaHangFilter.SelectedValue.ToString();
                }
                if (_currentTreeMode == 1 && !string.IsNullOrEmpty(_selectedCuaHangId))
                {
                    cuaHangId = _selectedCuaHangId;
                }

                string lyDoId = _currentTreeMode == 0 ? _selectedLyDoId : null;
                string taiKhoanId = _currentTreeMode == 2 ? _selectedTaiKhoanId : null;
                string searchText = TxtSearch.Text.Trim();

                _allList = await LocalPhieuThuChiService.GetDanhSachPhieuThuChiAsync(
                    isThu: false,
                    fromDate: fromDate,
                    toDate: toDate,
                    cuaHangId: cuaHangId,
                    lyDoId: lyDoId,
                    taiKhoanNganHangId: taiKhoanId,
                    searchText: searchText,
                    isTrash: _isTrashSelected
                );

                DgPhieuChi.ItemsSource = _allList;

                if (_allList.Count > 0)
                {
                    DgPhieuChi.SelectedIndex = 0;
                    UpdateDetailInfo(_allList[0]);
                }
                else
                {
                    ClearDetailInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục phiếu chi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_isLoaded)
            {
                _ = LoadDataGridAsync();
            }
        }

        private void TvFilter_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TvFilter.SelectedItem is TreeViewItem tvi && tvi.Tag != null)
            {
                string tag = tvi.Tag.ToString();
                if (_currentTreeMode == 0) // Lý do
                {
                    if (tag == "ALL")
                    {
                        _selectedLyDoId = null;
                        _isTrashSelected = false;
                    }
                    else if (tag == "NOT_SET")
                    {
                        _selectedLyDoId = "NOT_SET";
                        _isTrashSelected = false;
                    }
                    else if (tag == "TRASH")
                    {
                        _selectedLyDoId = null;
                        _isTrashSelected = true;
                    }
                    else
                    {
                        _selectedLyDoId = tag;
                        _isTrashSelected = false;
                    }
                }
                else if (_currentTreeMode == 1) // Cửa hàng
                {
                    _isTrashSelected = false;
                    _selectedCuaHangId = (tag == "ALL") ? null : tag;
                }
                else if (_currentTreeMode == 2) // Tài khoản
                {
                    _isTrashSelected = false;
                    _selectedTaiKhoanId = (tag == "ALL") ? null : tag;
                }

                _ = LoadDataGridAsync();
            }
        }

        private void DgPhieuChi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPhieuChi.SelectedItem is PhieuThuChiGridItem item)
            {
                UpdateDetailInfo(item);
            }
            else
            {
                ClearDetailInfo();
            }
        }

        private void UpdateDetailInfo(PhieuThuChiGridItem item)
        {
            if (item == null)
            {
                ClearDetailInfo();
                return;
            }

            TxtKhoiTao.Text = item.TimeCreated.HasValue ? item.TimeCreated.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : "";
            TxtKhoiTaoBoi.Text = !string.IsNullOrEmpty(item.UserCreated) ? item.UserCreated : "";
            TxtSuaDoi.Text = item.TimeModified.HasValue ? item.TimeModified.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : "";
            TxtSuaDoiBoi.Text = !string.IsNullOrEmpty(item.UserModified) ? item.UserModified : "";

            string hinhThuc = item.ChuyenKhoan == "Có" ? "Chuyển khoản" : "Tiền mặt";
            TxtChiTietThanhToan.Text = $"Hình thức: {hinhThuc} | Số tiền: {item.SoTien:N0} VNĐ | Đối tượng: {item.TenDoiTuong} | Lý do: {item.LyDoThuChi}";
        }

        private void ClearDetailInfo()
        {
            TxtKhoiTao.Text = "";
            TxtKhoiTaoBoi.Text = "";
            TxtSuaDoi.Text = "";
            TxtSuaDoiBoi.Text = "";
            TxtChiTietThanhToan.Text = "Chưa có dữ liệu thanh toán chi tiết.";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ = LoadDataGridAsync();
        }

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new TaoPhieuChiWindow();
            var parentWin = Window.GetWindow(this);
            if (parentWin != null) win.Owner = parentWin;

            if (win.ShowDialog() == true)
            {
                await LoadDataGridAsync();
            }
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgPhieuChi.SelectedItem is PhieuThuChiGridItem item)
            {
                var win = new TaoPhieuChiWindow(item.Id);
                var parentWin = Window.GetWindow(this);
                if (parentWin != null) win.Owner = parentWin;

                if (win.ShowDialog() == true)
                {
                    await LoadDataGridAsync();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu chi để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgPhieuChi_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnChinhSua_Click(null, null);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (DgPhieuChi.SelectedItem is PhieuThuChiGridItem item)
            {
                string msg = _isTrashSelected
                    ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN phiếu chi '{item.SoPhieu}' không?\nDữ liệu đã xóa sẽ không thể phục hồi!"
                    : $"Bạn có chắc chắn muốn chuyển phiếu chi '{item.SoPhieu}' vào thùng rác không?";

                var confirm = MessageBox.Show(msg, "Xác nhận", MessageBoxButton.YesNo, _isTrashSelected ? MessageBoxImage.Warning : MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    bool ok = await LocalPhieuThuChiService.DeletePhieuThuChiAsync(item.Id, permanent: _isTrashSelected);
                    if (ok)
                    {
                        await LoadDataGridAsync();
                    }
                    else
                    {
                        MessageBox.Show("Xóa không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phiếu chi để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng nhập từ Excel đang được chuẩn bị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allList == null || _allList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"DanhSachPhieuChi_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("Số phiếu,Ngày,Tên đối tượng,Địa chỉ,Lý do thu chi,Diễn giải,Chứng từ gốc,Số tiền,Ghi chú,Chuyển khoản,Đặt hàng,Cửa hàng,Là phiếu thu công nợ");
                        foreach (var item in _allList)
                        {
                            string line = $"\"{item.SoPhieu}\",\"{item.Ngay:dd/MM/yyyy}\",\"{item.TenDoiTuong}\",\"{item.DiaChi}\",\"{item.LyDoThuChi}\",\"{item.DienGiai}\",\"{item.ChungTuGoc}\",{item.SoTien},\"{item.GhiChu}\",\"{item.ChuyenKhoan}\",\"{item.DatHang}\",\"{item.CuaHang}\",\"{item.LaPhieuThuCongNo}\"";
                            writer.WriteLine(line);
                        }
                    }
                    MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            if (DgPhieuChi.SelectedItem is PhieuThuChiGridItem item)
            {
                MessageBox.Show($"Đang gửi lệnh in phiếu chi '{item.SoPhieu}'...", "In phiếu chi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Đang gửi lệnh in danh sách gồm {_allList.Count} phiếu chi...", "In danh sách phiếu chi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            decimal total = _allList?.Sum(x => x.SoTien) ?? 0;
            int count = _allList?.Count ?? 0;
            MessageBox.Show($"Tổng số lượng: {count:N0} phiếu chi\nTổng số tiền: {total:N0} VNĐ", "Tổng cộng phiếu chi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPhanTich_Click(object sender, RoutedEventArgs e)
        {
            decimal total = _allList?.Sum(x => x.SoTien) ?? 0;
            var byReason = _allList?
                .GroupBy(x => string.IsNullOrEmpty(x.LyDoThuChi) ? "(Chưa phân loại)" : x.LyDoThuChi)
                .Select(g => new { Reason = g.Key, Count = g.Count(), Sum = g.Sum(x => x.SoTien) })
                .OrderByDescending(x => x.Sum)
                .ToList();

            string info = $"PHÂN TÍCH CHI ({_allList?.Count} phiếu - Tổng: {total:N0} VNĐ):\n\n";
            if (byReason != null)
            {
                foreach (var r in byReason)
                {
                    info += $"- {r.Reason}: {r.Count} phiếu | {r.Sum:N0} VNĐ\n";
                }
            }

            MessageBox.Show(info, "Phân tích phiếu chi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            if (BtnMenuCheDo.ContextMenu != null)
            {
                BtnMenuCheDo.ContextMenu.PlacementTarget = BtnMenuCheDo;
                BtnMenuCheDo.ContextMenu.IsOpen = true;
            }
        }

        private async void MenuCheDoLyDo_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = 0;
            _selectedLyDoId = null;
            await LoadTreeDataAsync();
            await LoadDataGridAsync();
        }

        private async void MenuCheDoCuaHang_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = 1;
            _selectedCuaHangId = null;
            await LoadTreeDataAsync();
            await LoadDataGridAsync();
        }

        private async void MenuCheDoTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            _currentTreeMode = 2;
            _selectedTaiKhoanId = null;
            await LoadTreeDataAsync();
            await LoadDataGridAsync();
        }

        private async void BtnTaiLai_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeDataAsync();
            await LoadDataGridAsync();
        }

        private void BtnCauHinh_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (_currentTreeMode == 0)
            {
                var win = new DanhMucLyDoThuChiWindow();
                if (parentWin != null) win.Owner = parentWin;
                win.ShowDialog();
                _ = LoadTreeDataAsync();
            }
            else if (_currentTreeMode == 1)
            {
                var win = new QuanLyBar.Client.Views.KhoHang.ThemCuaHangWindow();
                if (parentWin != null) win.Owner = parentWin;
                win.ShowDialog();
                _ = LoadTreeDataAsync();
            }
            else if (_currentTreeMode == 2)
            {
                var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.DanhMucTaiKhoanNganHangWindow();
                if (parentWin != null) win.Owner = parentWin;
                win.ShowDialog();
                _ = LoadTreeDataAsync();
            }
        }

        private void BtnThemLyDo_Click(object sender, RoutedEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (_currentTreeMode == 0) // Lý do thu chi
            {
                var win = new ThemSuaLyDoThuChiWindow();
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
            else if (_currentTreeMode == 1) // Cửa hàng
            {
                var win = new QuanLyBar.Client.Views.KhoHang.ThemCuaHangWindow();
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                    _ = LoadCuaHangFilterAsync();
                }
            }
            else if (_currentTreeMode == 2) // Tài khoản ngân hàng
            {
                var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.ThemSuaTaiKhoanNganHangWindow();
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
        }

        private void BtnSuaLyDo_Click(object sender, RoutedEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (_currentTreeMode == 0) // Lý do thu chi
            {
                if (!string.IsNullOrEmpty(_selectedLyDoId) && _selectedLyDoId != "ALL" && _selectedLyDoId != "NOT_SET" && _selectedLyDoId != "TRASH")
                {
                    var win = new ThemSuaLyDoThuChiWindow(_selectedLyDoId);
                    if (parentWin != null) win.Owner = parentWin;
                    if (win.ShowDialog() == true)
                    {
                        _ = LoadTreeDataAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một lý do thu chi để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (_currentTreeMode == 1) // Cửa hàng
            {
                if (!string.IsNullOrEmpty(_selectedCuaHangId) && _selectedCuaHangId != "ALL")
                {
                    var win = new QuanLyBar.Client.Views.KhoHang.ThemCuaHangWindow(_selectedCuaHangId);
                    if (parentWin != null) win.Owner = parentWin;
                    if (win.ShowDialog() == true)
                    {
                        _ = LoadTreeDataAsync();
                        _ = LoadCuaHangFilterAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một cửa hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (_currentTreeMode == 2) // Tài khoản ngân hàng
            {
                if (!string.IsNullOrEmpty(_selectedTaiKhoanId) && _selectedTaiKhoanId != "ALL")
                {
                    var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.ThemSuaTaiKhoanNganHangWindow(_selectedTaiKhoanId);
                    if (parentWin != null) win.Owner = parentWin;
                    if (win.ShowDialog() == true)
                    {
                        _ = LoadTreeDataAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một tài khoản ngân hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (_currentTreeMode == 0)
            {
                var win = new ThemSuaLyDoThuChiWindow(null, "FOLDER");
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
            else if (_currentTreeMode == 2)
            {
                var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.ThemSuaTaiKhoanNganHangWindow(null, "FOLDER");
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
        }

        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadTreeDataAsync();
        }

        // ================= CONTEXT MENU TREEVIEW =================

        private void MenuThemLyDo_Click(object sender, RoutedEventArgs e)
        {
            BtnThemLyDo_Click(sender, e);
        }

        private void MenuThemNhanh_Click(object sender, RoutedEventArgs e)
        {
            BtnThemLyDo_Click(sender, e);
        }

        private async void MenuThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTreeMode == 0)
            {
                string parentId = _selectedLyDoId ?? "";
                var (ok, _, _) = await LocalLyDoThuChiService.SaveLyDoThuChiAsync(
                    null,
                    "----------------------------------------",
                    0,
                    "",
                    parentId,
                    "",
                    "SEPARATOR"
                );
                if (ok)
                {
                    await LoadTreeDataAsync();
                }
            }
            else if (_currentTreeMode == 2)
            {
                string parentId = _selectedTaiKhoanId ?? "";
                var (ok, _, _) = await LocalTaiKhoanNganHangService.SaveTaiKhoanNganHangAsync(
                    null,
                    "----------------------------------------",
                    parentId,
                    "",
                    "SEPARATOR"
                );
                if (ok)
                {
                    await LoadTreeDataAsync();
                }
            }
        }

        private void MenuThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            BtnThemThuMuc_Click(sender, e);
        }

        private void MenuThemLyDoCon_Click(object sender, RoutedEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (_currentTreeMode == 0)
            {
                string parentId = _selectedLyDoId ?? "";
                var win = new ThemSuaLyDoThuChiWindow(null, "", parentId);
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
            else if (_currentTreeMode == 2)
            {
                string parentId = _selectedTaiKhoanId ?? "";
                var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.ThemSuaTaiKhoanNganHangWindow(null, "", parentId);
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
        }

        private void MenuThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            var parentWin = Window.GetWindow(this);
            if (_currentTreeMode == 0)
            {
                string parentId = _selectedLyDoId ?? "";
                var win = new ThemSuaLyDoThuChiWindow(null, "FOLDER", parentId);
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
            else if (_currentTreeMode == 2)
            {
                string parentId = _selectedTaiKhoanId ?? "";
                var win = new QuanLyBar.Client.Views.DanhMucTaiKhoanNganHang.ThemSuaTaiKhoanNganHangWindow(null, "FOLDER", parentId);
                if (parentWin != null) win.Owner = parentWin;
                if (win.ShowDialog() == true)
                {
                    _ = LoadTreeDataAsync();
                }
            }
        }

        private void MenuSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            MenuSortByName.IsChecked = true;
            MenuSortByCustom.IsChecked = false;
            _ = LoadTreeDataAsync();
        }

        private void MenuSapXepThuTu_Click(object sender, RoutedEventArgs e)
        {
            MenuSortByName.IsChecked = false;
            MenuSortByCustom.IsChecked = true;
            _ = LoadTreeDataAsync();
        }

        private async void MenuSaoChepTree_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTreeMode == 0 && !string.IsNullOrEmpty(_selectedLyDoId) && _selectedLyDoId != "ALL" && _selectedLyDoId != "NOT_SET" && _selectedLyDoId != "TRASH")
            {
                var list = await LocalLyDoThuChiService.GetLyDoThuChiListAsync(false);
                var cur = list.FirstOrDefault(x => x.Id == _selectedLyDoId);
                if (cur != null)
                {
                    string copyName = $"{cur.Name} (Copy)";
                    var (ok, _, _) = await LocalLyDoThuChiService.SaveLyDoThuChiAsync(
                        null,
                        copyName,
                        cur.Lalydothu ?? 0,
                        cur.Loailydo ?? "-1",
                        cur.ParentId ?? "",
                        cur.Note ?? "",
                        cur.ItemType ?? ""
                    );
                    if (ok)
                    {
                        MessageBox.Show($"Đã sao chép thành công lý do '{copyName}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadTreeDataAsync();
                    }
                }
            }
            else if (_currentTreeMode == 2 && !string.IsNullOrEmpty(_selectedTaiKhoanId) && _selectedTaiKhoanId != "ALL")
            {
                var list = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangListAsync(false);
                var cur = list.FirstOrDefault(x => x.Id == _selectedTaiKhoanId);
                if (cur != null)
                {
                    string copyName = $"{cur.Name} (Copy)";
                    var (ok, _, _) = await LocalTaiKhoanNganHangService.SaveTaiKhoanNganHangAsync(
                        null,
                        copyName,
                        cur.ParentId ?? "",
                        cur.Note ?? "",
                        cur.ItemType ?? ""
                    );
                    if (ok)
                    {
                        MessageBox.Show($"Đã sao chép thành công tài khoản '{copyName}'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadTreeDataAsync();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mục để sao chép!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuMoRong_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(true);
        }

        private void MenuThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetExpandAll(false);
        }

        private void SetExpandAll(bool expand)
        {
            foreach (var item in TvFilter.Items)
            {
                if (item is TreeViewItem tvi)
                {
                    tvi.IsExpanded = expand;
                }
            }
        }

        private async void MenuXoaTree_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTreeMode == 0 && !string.IsNullOrEmpty(_selectedLyDoId) && _selectedLyDoId != "ALL" && _selectedLyDoId != "NOT_SET" && _selectedLyDoId != "TRASH")
            {
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn chuyển lý do này vào thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    bool ok = await LocalLyDoThuChiService.DeleteLyDoThuChiAsync(_selectedLyDoId, permanent: false);
                    if (ok)
                    {
                        await LoadTreeDataAsync();
                    }
                }
            }
            else if (_currentTreeMode == 2 && !string.IsNullOrEmpty(_selectedTaiKhoanId) && _selectedTaiKhoanId != "ALL")
            {
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn chuyển tài khoản này vào thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    bool ok = await LocalTaiKhoanNganHangService.DeleteTaiKhoanNganHangAsync(_selectedTaiKhoanId, permanent: false);
                    if (ok)
                    {
                        await LoadTreeDataAsync();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mục để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuThungRac_Click(object sender, RoutedEventArgs e)
        {
            _isTrashSelected = true;
            _selectedLyDoId = null;
            _selectedCuaHangId = null;
            _selectedTaiKhoanId = null;
            foreach (var item in TvFilter.Items)
            {
                if (item is TreeViewItem tvi && tvi.Tag?.ToString() == "TRASH")
                {
                    tvi.IsSelected = true;
                    break;
                }
            }
            _ = LoadDataGridAsync();
        }

        private async void MenuThuocTinhTree_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTreeMode == 0 && !string.IsNullOrEmpty(_selectedLyDoId) && _selectedLyDoId != "ALL" && _selectedLyDoId != "NOT_SET" && _selectedLyDoId != "TRASH")
            {
                var list = await LocalLyDoThuChiService.GetLyDoThuChiListAsync(false);
                var cur = list.FirstOrDefault(x => x.Id == _selectedLyDoId);
                if (cur != null)
                {
                    string info = $"Mã lý do: {cur.Id}\n" +
                                  $"Tên lý do: {cur.Name}\n" +
                                  $"Phân loại: " + (cur.Lalydothu.HasValue && cur.Lalydothu.Value > 0 ? "Lý do thu" : "Lý do chi") + "\n" +
                                  $"Ghi chú: {cur.Note}";
                    MessageBox.Show(info, $"Thuộc tính: {cur.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else if (_currentTreeMode == 2 && !string.IsNullOrEmpty(_selectedTaiKhoanId) && _selectedTaiKhoanId != "ALL")
            {
                var list = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangListAsync(false);
                var cur = list.FirstOrDefault(x => x.Id == _selectedTaiKhoanId);
                if (cur != null)
                {
                    string info = $"Mã TK: {cur.Id}\n" +
                                  $"Tên TK: {cur.Name}\n" +
                                  $"Ghi chú: {cur.Note}";
                    MessageBox.Show(info, $"Thuộc tính: {cur.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            MessageBox.Show("Vui lòng chọn một mục để xem thuộc tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
