using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucBanKhuVucControl : UserControl
    {
        private LocalBanKhuVucService _service;
        private List<BanViewModel> _allBans = new List<BanViewModel>();
        private string _currentSortColumn = "Name";
        private bool _isSortAscending = true;

        public DanhMucBanKhuVucControl()
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load tree khu vực
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;

            // Load tất cả bàn mặc định
            await LoadBanData(null);
        }

        private async void TvKhuVuc_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is KhuVucViewModel selectedKhuVuc)
            {
                await LoadBanData(selectedKhuVuc.Id);
            }
        }

        private async Task LoadBanData(string khuVucId)
        {
            _allBans = await _service.GetBanListAsync(khuVucId);
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            if (_allBans == null)
            {
                DgBan.ItemsSource = null;
                return;
            }

            var query = _allBans.AsEnumerable();

            // Lọc theo từ khóa
            string filter = TxtLocBan?.Text?.Trim();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(b => 
                    (!string.IsNullOrEmpty(b.Name) && b.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.KhuVucName) && b.KhuVucName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.NhomHienThiName) && b.NhomHienThiName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.LoaiPhongName) && b.LoaiPhongName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.Note) && b.Note.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // Sắp xếp
            query = _currentSortColumn switch
            {
                "KhuVuc" => _isSortAscending ? query.OrderBy(b => b.KhuVucName) : query.OrderByDescending(b => b.KhuVucName),
                "NhomHienThi" => _isSortAscending ? query.OrderBy(b => b.NhomHienThiName) : query.OrderByDescending(b => b.NhomHienThiName),
                "LoaiPhong" => _isSortAscending ? query.OrderBy(b => b.LoaiPhongName) : query.OrderByDescending(b => b.LoaiPhongName),
                "Note" => _isSortAscending ? query.OrderBy(b => b.Note) : query.OrderByDescending(b => b.Note),
                _ => _isSortAscending ? query.OrderBy(b => b.Name) : query.OrderByDescending(b => b.Name)
            };

            var list = query.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Stt = i + 1;
            }

            DgBan.ItemsSource = list;
        }

        private void TxtLocBan_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            int total = _allBans?.Count ?? 0;
            int displayed = (DgBan.ItemsSource as List<BanViewModel>)?.Count ?? 0;
            MessageBox.Show($"Tổng số bàn: {total} bàn (Đang hiển thị: {displayed} bàn)", "Tổng kết", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private DataGridColumn _clickedColumn = null;
        private DataGridCell _clickedCell = null;

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    DgBan.SelectedItems.Clear();
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
                    }
                }
            }
        }

        private void DgBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selected)
            {
                TxtInfoKhoiTao.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                TxtInfoSuaDoi.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
            else
            {
                TxtInfoKhoiTao.Text = "--/--/---- --:--:--";
                TxtInfoSuaDoi.Text = "--/--/---- --:--:--";
            }
        }

        private async void BtnThemKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var selectedKhuVuc = TvKhuVuc.SelectedItem as KhuVucViewModel;

            var win = new ThemKhuVucWindow("THÊM MỚI KHU VỰC");
            if (win.ShowDialog() == true)
            {
                string parentId = null;
                if (selectedKhuVuc != null && !string.IsNullOrEmpty(selectedKhuVuc.Id))
                {
                    parentId = selectedKhuVuc.Id;
                }

                var newKhuVuc = new DKHUVUC
                {
                    Name = win.TenKhuVuc,
                    ParentId = parentId
                };

                bool success = await _service.InsertKhuVucAsync(newKhuVuc.Name, newKhuVuc.ParentId);
                if (success)
                {
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async void BtnSuaKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var selectedKhuVuc = TvKhuVuc.SelectedItem as KhuVucViewModel;
            if (selectedKhuVuc != null && !string.IsNullOrEmpty(selectedKhuVuc.Id))
            {
                var win = new ThemKhuVucWindow("SỬA KHU VỰC");
                if (win.ShowDialog() == true)
                {
                    var updatedKhuVuc = new DKHUVUC
                    {
                        Id = selectedKhuVuc.Id,
                        Name = win.TenKhuVuc
                    };

                    bool success = await _service.UpdateKhuVucAsync(updatedKhuVuc.Id, updatedKhuVuc.Name);
                    if (success)
                    {
                        await ReloadTreeViewAsync();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khu vực cụ thể cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnThemThuMucKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemKhuVucWindow("TẠO MỚI THƯ MỤC KHU VỰC");
            if (win.ShowDialog() == true)
            {
                var newKhuVuc = new DKHUVUC
                {
                    Name = win.TenKhuVuc,
                    ParentId = null // Thư mục nằm ở gốc
                };

                bool success = await _service.InsertKhuVucAsync(newKhuVuc.Name, newKhuVuc.ParentId);
                if (success)
                {
                    await ReloadTreeViewAsync();
                }
            }
        }

        private async Task ReloadTreeViewAsync()
        {
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;
        }

        private async void BtnHienThiTatCa_Click(object sender, RoutedEventArgs e)
        {
            await LoadBanData(null);
        }

        private async void BtnTaiLaiKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var treeData = await _service.GetKhuVucTreeAsync();
            TvKhuVuc.ItemsSource = treeData;
            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
            await LoadBanData(khuVucId);
        }

        private async void BtnThemNhanhBan_Click(object sender, RoutedEventArgs e)
        {
            var window = new ThemNhanhBanKhuVucWindow();
            if (window.ShowDialog() == true)
            {
                BtnTaiLaiKhuVuc_Click(null, null);
            }
        }

        private void BtnThemBanExcel_Click(object sender, RoutedEventArgs e)
        {
            var window = new ThemBanExcelWindow(() =>
            {
                BtnTaiLaiKhuVuc_Click(null, null);
            });
            window.ShowDialog();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = DgBan.ItemsSource as IEnumerable<BanViewModel>;
                if (items == null || !items.Any())
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = "DanhSachBan.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("DanhSachBan");
                        
                        worksheet.Cell(1, 1).Value = "STT";
                        worksheet.Cell(1, 2).Value = "Tên bàn";
                        worksheet.Cell(1, 3).Value = "Khu vực";
                        worksheet.Cell(1, 4).Value = "Nhóm hiển thị";
                        worksheet.Cell(1, 5).Value = "Loại phòng";
                        worksheet.Cell(1, 6).Value = "Ghi chú";
                        
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int row = 2;
                        foreach (var item in items)
                        {
                            worksheet.Cell(row, 1).Value = item.Stt;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.KhuVucName;
                            worksheet.Cell(row, 4).Value = item.NhomHienThiName;
                            worksheet.Cell(row, 5).Value = item.LoaiPhongName;
                            worksheet.Cell(row, 6).Value = item.Note;
                            row++;
                        }
                        
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    var result = MessageBox.Show("Xuất Excel thành công! Bạn có muốn mở file vừa xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgBan);
            win.TxtTieuDe.Text = "Bàn";
            win.ShowDialog();
        }

        private async void BtnThemMoiBan_Click(object sender, RoutedEventArgs e)
        {
            string selectedKhuVucId = null;

            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                selectedKhuVucId = selected.Id == "-1" ? null : selected.Id;
            }
            
            // Bật popup chọn khu vực nếu chưa có
            if (selectedKhuVucId == null)
            {
                var chonKhuVucWin = new ChonKhuVucWindow();
                if (chonKhuVucWin.ShowDialog() == true)
                {
                    if (chonKhuVucWin.SelectedKhuVuc != null)
                    {
                        selectedKhuVucId = chonKhuVucWin.SelectedKhuVuc.Id;
                    }
                }
                else
                {
                    return; // Người dùng hủy
                }
            }

            var newBan = new DBAN { DkhuvucId = selectedKhuVucId };
            var banList = DgBan.ItemsSource as List<BanViewModel>;
            var win = new ThemMoiBanWindow(newBan, banList);
            if (win.ShowDialog() == true)
            {
                await LoadBanData(selectedKhuVucId);
            }
        }

        private async void BtnSuaBan_Click(object sender, RoutedEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selectedRow)
            {
                if (int.TryParse(selectedRow.Id, out int banId))
                {
                    var editBan = await _service.GetBanByIdAsync(banId.ToString());
                    if (editBan != null)
                    {
                        var banList = DgBan.ItemsSource as List<BanViewModel>;
                        var win = new ThemMoiBanWindow(editBan, banList);
                        if (win.ShowDialog() == true)
                        {
                            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                            await LoadBanData(khuVucId);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông tin bàn trong CSDL!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một Bàn để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DgBan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                BtnXoaBan_Click(sender, e);
            }
        }

        private async void BtnXoaBan_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgBan.SelectedItems.Cast<BanViewModel>().ToList();
            if (selectedList.Count == 0 && DgBan.SelectedItem is BanViewModel single)
            {
                selectedList.Add(single);
            }

            if (selectedList.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một Bàn để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
            bool isTrash = khuVucId == "-1";

            string message = selectedList.Count == 1
                ? (isTrash ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN bàn '{selectedList[0].Name}' không?" : $"Bạn có chắc muốn đưa bàn '{selectedList[0].Name}' vào Thùng rác không?")
                : (isTrash ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN {selectedList.Count} bàn đã chọn không?" : $"Bạn có chắc muốn đưa {selectedList.Count} bàn đã chọn vào Thùng rác không?");

            var result = MessageBox.Show(message, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var ids = selectedList.Select(b => b.Id).Where(id => !string.IsNullOrEmpty(id)).ToList();
                bool success = await _service.DeleteBansAsync(ids, isTrash);
                if (success)
                {
                    await LoadBanData(khuVucId);
                }
            }
        }

        #region Context Menu Handlers

        private async void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (MenuDatCot == null || MenuLocCot == null) return;

            string colHeader = _clickedColumn?.Header?.ToString() ?? "Tên bàn";
            var selectedList = DgBan.SelectedItems.Cast<BanViewModel>().ToList();
            if (selectedList.Count == 0 && DgBan.SelectedItem is BanViewModel single) selectedList.Add(single);
            var selectedBan = DgBan.SelectedItem as BanViewModel;

            // Xóa subitems cũ và gỡ sự kiện cũ
            MenuDatCot.Items.Clear();
            MenuLocCot.Items.Clear();
            MenuDatCot.Click -= MenuDatCot_Click;
            MenuLocCot.Click -= MenuLocCot_Click;

            if (colHeader == "Khu vực")
            {
                MenuDatCot.Header = "Đặt Khu vực";
                MenuLocCot.Header = "Lọc Khu vực";

                var khuVucs = await _service.GetLookupAsync("DKHUVUC");
                foreach (var kv in khuVucs)
                {
                    var miDat = new MenuItem { Header = kv.Name, Tag = kv.Id };
                    miDat.Click += async (s, args) =>
                    {
                        var ids = selectedList.Select(b => b.Id).ToList();
                        if (await _service.UpdateBansColumnAsync(ids, "DKHUVUCID", kv.Id))
                        {
                            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                            await LoadBanData(khuVucId);
                        }
                    };
                    MenuDatCot.Items.Add(miDat);

                    var miLoc = new MenuItem { Header = kv.Name };
                    miLoc.Click += (s, args) =>
                    {
                        TxtLocBan.Text = kv.Name;
                    };
                    MenuLocCot.Items.Add(miLoc);
                }
            }
            else if (colHeader == "Nhóm hiển thị")
            {
                MenuDatCot.Header = "Đặt Nhóm hiển thị";
                MenuLocCot.Header = "Lọc Nhóm hiển thị";

                var nhomList = await _service.GetLookupAsync("DNHOMHIENTHI");
                foreach (var nh in nhomList)
                {
                    var miDat = new MenuItem { Header = nh.Name, Tag = nh.Id };
                    miDat.Click += async (s, args) =>
                    {
                        var ids = selectedList.Select(b => b.Id).ToList();
                        if (await _service.UpdateBansColumnAsync(ids, "DNHOMHIENTHIID", nh.Id))
                        {
                            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                            await LoadBanData(khuVucId);
                        }
                    };
                    MenuDatCot.Items.Add(miDat);

                    var miLoc = new MenuItem { Header = nh.Name };
                    miLoc.Click += (s, args) =>
                    {
                        TxtLocBan.Text = nh.Name;
                    };
                    MenuLocCot.Items.Add(miLoc);
                }
            }
            else if (colHeader == "Loại phòng")
            {
                MenuDatCot.Header = "Đặt Loại phòng";
                MenuLocCot.Header = "Lọc Loại phòng";

                var loaiPhongList = await _service.GetLookupAsync("DLOAIPHONG");
                foreach (var lp in loaiPhongList)
                {
                    var miDat = new MenuItem { Header = lp.Name, Tag = lp.Id };
                    miDat.Click += async (s, args) =>
                    {
                        var ids = selectedList.Select(b => b.Id).ToList();
                        if (await _service.UpdateBansColumnAsync(ids, "DLOAIPHONGID", lp.Id))
                        {
                            var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                            await LoadBanData(khuVucId);
                        }
                    };
                    MenuDatCot.Items.Add(miDat);

                    var miLoc = new MenuItem { Header = lp.Name };
                    miLoc.Click += (s, args) =>
                    {
                        TxtLocBan.Text = lp.Name;
                    };
                    MenuLocCot.Items.Add(miLoc);
                }
            }
            else if (colHeader == "Ghi chú")
            {
                MenuDatCot.Header = "Đặt Ghi chú";
                MenuLocCot.Header = "Lọc Ghi chú";
                MenuDatCot.Click += MenuDatCot_Click;
                MenuLocCot.Click += MenuLocCot_Click;
            }
            else // "Tên bàn" hoặc STT
            {
                MenuDatCot.Header = "Đặt Tên bàn";
                MenuLocCot.Header = "Lọc Tên bàn";
                MenuDatCot.Click += MenuDatCot_Click;
                MenuLocCot.Click += MenuLocCot_Click;
            }
        }

        private async void MenuDatCot_Click(object sender, RoutedEventArgs e)
        {
            string colHeader = _clickedColumn?.Header?.ToString() ?? "Tên bàn";
            var selectedList = DgBan.SelectedItems.Cast<BanViewModel>().ToList();
            if (selectedList.Count == 0 && DgBan.SelectedItem is BanViewModel single) selectedList.Add(single);
            var selectedBan = DgBan.SelectedItem as BanViewModel;

            if (colHeader == "Ghi chú")
            {
                string currentVal = selectedBan?.Note ?? "";
                var inputWin = new InputWindow("Đặt Ghi chú", "Nhập ghi chú mới:", currentVal);
                if (inputWin.ShowDialog() == true)
                {
                    string newNote = inputWin.InputText ?? "";
                    var ids = selectedList.Select(b => b.Id).ToList();
                    if (await _service.UpdateBansColumnAsync(ids, "NOTE", newNote))
                    {
                        var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                        await LoadBanData(khuVucId);
                    }
                }
            }
            else // Tên bàn
            {
                if (selectedBan != null)
                {
                    var inputWin = new InputWindow("Đặt Tên bàn", "Nhập tên mới cho bàn:", selectedBan.Name);
                    if (inputWin.ShowDialog() == true)
                    {
                        string newName = inputWin.InputText?.Trim();
                        if (!string.IsNullOrWhiteSpace(newName) && newName != selectedBan.Name)
                        {
                            var ban = await _service.GetBanByIdAsync(selectedBan.Id);
                            if (ban != null)
                            {
                                ban.Name = newName;
                                if (await _service.UpdateBanAsync(ban))
                                {
                                    var khuVucId = (TvKhuVuc.SelectedItem as KhuVucViewModel)?.Id;
                                    await LoadBanData(khuVucId);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MenuLocCot_Click(object sender, RoutedEventArgs e)
        {
            string colHeader = _clickedColumn?.Header?.ToString() ?? "Tên bàn";
            var selectedBan = DgBan.SelectedItem as BanViewModel;
            if (selectedBan == null) return;

            if (colHeader == "Ghi chú")
            {
                if (!string.IsNullOrEmpty(selectedBan.Note))
                    TxtLocBan.Text = selectedBan.Note;
            }
            else
            {
                if (!string.IsNullOrEmpty(selectedBan.Name))
                    TxtLocBan.Text = selectedBan.Name;
            }
        }

        private void MenuItem_DatTenBan_Click(object sender, RoutedEventArgs e) { MenuDatCot_Click(sender, e); }
        private void MenuItem_LocTenBan_Click(object sender, RoutedEventArgs e) { MenuLocCot_Click(sender, e); }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            _isSortAscending = true;
            ApplyFilterAndSort();
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            _isSortAscending = false;
            ApplyFilterAndSort();
        }

        private void MenuItem_SortByName_Click(object sender, RoutedEventArgs e)
        {
            _currentSortColumn = "Name";
            ApplyFilterAndSort();
        }

        private void MenuItem_SortByKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortColumn = "KhuVuc";
            ApplyFilterAndSort();
        }

        private void MenuItem_SortByNhomHienThi_Click(object sender, RoutedEventArgs e)
        {
            _currentSortColumn = "NhomHienThi";
            ApplyFilterAndSort();
        }

        private void MenuItem_SortByLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            _currentSortColumn = "LoaiPhong";
            ApplyFilterAndSort();
        }

        private void MenuItem_SortByNote_Click(object sender, RoutedEventArgs e)
        {
            _currentSortColumn = "Note";
            ApplyFilterAndSort();
        }

        private void MenuItem_SaoChepOClick(object sender, RoutedEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selected)
            {
                string colHeader = _clickedColumn?.Header?.ToString() ?? "Tên bàn";
                string textToCopy = colHeader switch
                {
                    "Khu vực" => selected.KhuVucName ?? "",
                    "Nhóm hiển thị" => selected.NhomHienThiName ?? "",
                    "Loại phòng" => selected.LoaiPhongName ?? "",
                    "Ghi chú" => selected.Note ?? "",
                    _ => selected.Name ?? ""
                };
                Clipboard.SetText(textToCopy);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgBan.SelectedItems.Cast<BanViewModel>().ToList();
            if (selectedList.Count == 0 && DgBan.SelectedItem is BanViewModel single) selectedList.Add(single);
            if (selectedList.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("STT\tTên bàn\tKhu vực\tNhóm hiển thị\tLoại phòng\tGhi chú");
                foreach (var b in selectedList)
                {
                    sb.AppendLine($"{b.Stt}\t{b.Name}\t{b.KhuVucName}\t{b.NhomHienThiName}\t{b.LoaiPhongName}\t{b.Note}");
                }
                Clipboard.SetText(sb.ToString());
                MessageBox.Show($"Đã sao chép {selectedList.Count} dòng vào Clipboard.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgBan.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonCotHienThiWindow(DgBan, new List<string> { "Tên bàn", "Ghi chú", "Khu vực", "Nhóm hiển thị", "Loại phòng", "Đơn giá" });
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private async void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selected)
            {
                var ban = await _service.GetBanByIdAsync(selected.Id);
                if (ban != null)
                {
                    var win = new ThuocTinhWindow(ban);
                    win.Owner = Window.GetWindow(this);
                    win.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một Bàn để xem thuộc tính!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        private void MenuItem_Placeholder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang được cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void MenuItem_BieuTuongKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                if (int.TryParse(selected.Id, out int khuVucId))
                {
                    var win = new ThuVienAnhWindow();
                    if (win.ShowDialog() == true && win.SelectedIcon != null)
                    {
                        if (await _service.UpdateKhuVucIconAsync(khuVucId, win.SelectedIcon.Id, win.SelectedIcon.Anh))
                        {
                            BtnTaiLaiKhuVuc_Click(null, null);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Không thể đổi biểu tượng của node này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void MenuItem_DoiTenKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể đổi tên thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(selected.Id, out int khuVucId))
                {
                    var inputWin = new InputWindow("Đổi tên", "Nhập tên mới:", selected.Name);
                    if (inputWin.ShowDialog() == true)
                    {
                        string newName = inputWin.InputText;
                        if (!string.IsNullOrWhiteSpace(newName) && newName != selected.Name)
                        {
                            if (await _service.RenameKhuVucAsync(khuVucId, newName))
                            {
                                BtnTaiLaiKhuVuc_Click(null, null);
                            }
                        }
                    }
                }
            }
        }

        private void MenuItem_ThemMoiKhuVuc_Click(object sender, RoutedEventArgs e) { BtnThemThuMucKhuVuc_Click(sender, e); }
        private void MenuItem_ThemMoiConKhuVuc_Click(object sender, RoutedEventArgs e) { BtnThemKhuVuc_Click(sender, e); }
        private void MenuItem_SuaDoiKhuVuc_Click(object sender, RoutedEventArgs e) { BtnSuaKhuVuc_Click(sender, e); }
        private async void MenuItem_XoaKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (TvKhuVuc.SelectedItem is KhuVucViewModel selected)
            {
                if (selected.Id == null)
                {
                    MessageBox.Show("Không thể xóa thư mục gốc.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isTrash = selected.Id == "-1";
                string msg = isTrash
                    ? $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN khu vực '{selected.Name}' không?"
                    : $"Bạn có chắc muốn đưa khu vực '{selected.Name}' vào Thùng rác không?";

                var result = MessageBox.Show(msg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _service.DeleteKhuVucAsync(selected.Id, isTrash);
                    if (success)
                    {
                        await ReloadTreeViewAsync();
                        await LoadBanData(null);
                    }
                }
            }
        }
        private void InlineEditTextBox_Loaded(object sender, RoutedEventArgs e) { }
        private void InlineEditTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) { }
        private void InlineEditTextBox_LostFocus(object sender, RoutedEventArgs e) { }
        private void InlineEditTextBox_KeyDown(object sender, KeyEventArgs e) { }
    }
}


