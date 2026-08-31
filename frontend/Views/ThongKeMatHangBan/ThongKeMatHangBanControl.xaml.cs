using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ThongKeMatHangBanControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private readonly LocalMatHangService _matHangService;
        private ObservableCollection<PosNhomMatHangViewModel> _treeData;
        private bool _isLoaded = false;
        private string _selectedNhomId = "ALL";

        public ThongKeMatHangBanControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            _matHangService = new LocalMatHangService();
            _treeData = new ObservableCollection<PosNhomMatHangViewModel>();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            dpTuNgay.SelectedDate = DateTime.Today;
            dpDenNgay.SelectedDate = DateTime.Today;

            await LoadCuaHangListAsync();
            await LoadNhomMatHangTreeAsync();
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
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 232, 245))
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
            var btnSave = new Button { Content = "Lưu", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true, Background = System.Windows.Media.Brushes.White };
            var btnCancel = new Button { Content = "Đóng", Width = 75, Height = 26, IsCancel = true, Background = System.Windows.Media.Brushes.White };
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

        private async Task LoadNhomMatHangTreeAsync()
        {
            try
            {
                _treeData.Clear();

                var rootItem = new PosNhomMatHangViewModel
                {
                    Id = "ALL",
                    Name = "Tất cả",
                    Icon = "🌐",
                    IsExpanded = true,
                    IsSelected = true
                };

                var listNhom = await _matHangService.GetAllNhomMatHangAsync();
                if (listNhom != null)
                {
                    // Map items by Id
                    var dict = new Dictionary<string, PosNhomMatHangViewModel>();
                    foreach (var n in listNhom)
                    {
                        string idStr = n.Id?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(idStr))
                        {
                            dict[idStr] = new PosNhomMatHangViewModel
                            {
                                Id = idStr,
                                Name = n.Name ?? "",
                                ParentId = n.ParentId?.ToString(),
                                Icon = GetIconForGroupName(n.Name ?? ""),
                                IsExpanded = true
                            };
                        }
                    }

                    // Build hierarchy
                    foreach (var kvp in dict)
                    {
                        var item = kvp.Value;
                        if (!string.IsNullOrEmpty(item.ParentId) && dict.ContainsKey(item.ParentId))
                        {
                            dict[item.ParentId].Children.Add(item);
                        }
                        else
                        {
                            rootItem.Children.Add(item);
                        }
                    }
                }

                _treeData.Add(rootItem);
                TvNhomMatHang.ItemsSource = _treeData;
            }
            catch { }
        }

        private string GetIconForGroupName(string name)
        {
            string upper = name.ToUpper();
            if (upper.Contains("BÒ") || upper.Contains("BÊ") || upper.Contains("TRÂU") || upper.Contains("DÊ")) return "🍱";
            if (upper.Contains("CÁ")) return "🎗️";
            if (upper.Contains("LẨU")) return "🥗";
            if (upper.Contains("RAU")) return "🥬";
            if (upper.Contains("CHIM")) return "🍸";
            if (upper.Contains("CƠM")) return "🍚";
            if (upper.Contains("ĐỒ UỐNG") || upper.Contains("NƯỚC")) return "🥤";
            if (upper.Contains("GÀ") || upper.Contains("VỊT")) return "💬";
            if (upper.Contains("HẢI SẢN")) return "♞";
            if (upper.Contains("LƯƠN") || upper.Contains("CUA") || upper.Contains("ỐC") || upper.Contains("ẾCH")) return "🎸";
            if (upper.Contains("KHAI VỊ")) return "🥢";
            return "📁";
        }

        private async void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TvNhomMatHang.SelectedItem is PosNhomMatHangViewModel selectedItem)
            {
                _selectedNhomId = selectedItem.Id ?? "ALL";
                if (_isLoaded)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                await LoadDataAsync();
            }
        }

        private async void RbGia_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnInBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            string storeName = string.IsNullOrEmpty(TxtSelectedCuaHang.Text) ? "NÀNG HƯƠNG QUÁN" : TxtSelectedCuaHang.Text;
            var win = new ChonMauInMatHangBanWindow(DgMatHangBan, "BÁO CÁO THỐNG KÊ MẶT HÀNG BÁN", storeName, tuNgay, denNgay);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
                bool isTheoGiaVon = RbTheoGiaVon.IsChecked == true;

                var list = await _hoaDonService.GetThongKeMatHangBanAsync(tuNgay, denNgay, _selectedNhomId, isTheoGiaVon);
                DgMatHangBan.ItemsSource = list;

                decimal tongTienBan = 0;
                decimal tongTienNhap = 0;
                decimal tongTienGiamGia = 0;
                decimal tongLai = 0;

                foreach (var item in list)
                {
                    tongTienBan += item.ThanhTienBan;
                    tongTienNhap += item.ThanhTienNhap;
                    tongTienGiamGia += item.TienGiam;
                    tongLai += item.Lai;
                }

                TxtTongTienBan.Text = tongTienBan.ToString("N0");
                TxtTongTienNhap.Text = tongTienNhap.ToString("N0");
                TxtLaiBanHang.Text = tongLai.ToString("N0");
                TxtTongTienGiamGia.Text = tongTienGiamGia.ToString("N0");
                TxtTongTienLai.Text = tongLai.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region MENU CHUỘT PHẢI CÂY NHÓM MẶT HÀNG (TvNhomMatHang)

        private async void MenuTree_ThemMoiCungCap_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomWindow(false);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadNhomMatHangTreeAsync();
            }
        }

        private async void MenuTree_ThemMoiCha_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomWindow(false);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadNhomMatHangTreeAsync();
            }
        }

        private async void MenuTree_ThemCon_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomWindow(false);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadNhomMatHangTreeAsync();
            }
        }

        private async void MenuTree_ChinhSua_Click(object sender, RoutedEventArgs e)
        {
            var selected = TvNhomMatHang?.SelectedItem as PosNhomMatHangViewModel;
            if (selected == null || selected.Id == "ALL")
            {
                MessageBox.Show("Vui lòng chọn một nhóm để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var win = new ThemNhomWindow(false, selected.Name);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadNhomMatHangTreeAsync();
            }
        }

        private void MenuTree_SortAZ_Click(object sender, RoutedEventArgs e)
        {
            if (_treeData != null && _treeData.Count > 0 && _treeData[0].Children != null)
            {
                var sorted = _treeData[0].Children.OrderBy(x => x.Name).ToList();
                _treeData[0].Children.Clear();
                foreach (var item in sorted)
                {
                    _treeData[0].Children.Add(item);
                }
            }
        }

        private void MenuTree_SortZA_Click(object sender, RoutedEventArgs e)
        {
            if (_treeData != null && _treeData.Count > 0 && _treeData[0].Children != null)
            {
                var sorted = _treeData[0].Children.OrderByDescending(x => x.Name).ToList();
                _treeData[0].Children.Clear();
                foreach (var item in sorted)
                {
                    _treeData[0].Children.Add(item);
                }
            }
        }

        private async void MenuTree_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadNhomMatHangTreeAsync();
            await LoadDataAsync();
        }

        private void MenuTree_SaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang?.SelectedItem is PosNhomMatHangViewModel selected)
            {
                Clipboard.SetText(selected.Name ?? "");
                MessageBox.Show($"Đã sao chép tên nhóm '{selected.Name}' vào Clipboard!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuTree_MoRong_Click(object sender, RoutedEventArgs e)
        {
            SetTreeExpandState(_treeData, true);
        }

        private void MenuTree_ThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetTreeExpandState(_treeData, false);
            if (_treeData != null && _treeData.Count > 0)
            {
                _treeData[0].IsExpanded = true;
            }
        }

        private void SetTreeExpandState(IEnumerable<PosNhomMatHangViewModel> items, bool isExpanded)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                item.IsExpanded = isExpanded;
                SetTreeExpandState(item.Children, isExpanded);
            }
        }

        private async void MenuTree_Xoa_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang?.SelectedItem is PosNhomMatHangViewModel selected && selected.Id != "ALL")
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm '{selected.Name}' và đưa vào thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            await conn.OpenAsync();
                            await conn.ExecuteAsync("UPDATE DNHOMMATHANG SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = selected.Id });
                        }
                        await LoadNhomMatHangTreeAsync();
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa nhóm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Không thể xóa nhóm gốc 'Tất cả'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void MenuTree_DoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang?.SelectedItem is PosNhomMatHangViewModel selected && selected.Id != "ALL")
            {
                var win = new InputWindow("Đổi tên nhóm", "Nhập tên nhóm mới:", selected.Name);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.InputText))
                {
                    string newName = win.InputText.Trim();
                    try
                    {
                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            await conn.OpenAsync();
                            await conn.ExecuteAsync("UPDATE DNHOMMATHANG SET NAME = @Name WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Name = newName, Id = selected.Id });
                        }
                        selected.Name = newName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi đổi tên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuTree_ThungRac_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở danh mục thùng rác nhóm mặt hàng.", "Thùng rác", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuTree_BieuTuong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đổi biểu tượng nhóm đang sẵn sàng.", "Biểu tượng", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuTree_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomMatHang?.SelectedItem is PosNhomMatHangViewModel selected)
            {
                MessageBox.Show($"Tên nhóm: {selected.Name}\nMã nhóm: {selected.Id}", "Thuộc tính nhóm", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region MENU CHUỘT PHẢI BẢNG MẶT HÀNG BÁN (DgMatHangBan)

        private void MnuMatHangBan_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgMatHangBan.CurrentCell != null && DgMatHangBan.CurrentCell.Item is ThongKeMatHangBanItemViewModel row)
                {
                    var col = DgMatHangBan.CurrentCell.Column as DataGridBoundColumn;
                    if (col != null && col.Binding is System.Windows.Data.Binding binding)
                    {
                        var propName = binding.Path?.Path;
                        if (!string.IsNullOrEmpty(propName))
                        {
                            var val = row.GetType().GetProperty(propName)?.GetValue(row, null);
                            Clipboard.SetText(val?.ToString() ?? "");
                            return;
                        }
                    }
                }
                if (DgMatHangBan.SelectedItem is ThongKeMatHangBanItemViewModel sel)
                {
                    Clipboard.SetText(sel.TenHang ?? "");
                }
            }
            catch { }
        }

        private void MnuMatHangBan_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgMatHangBan.SelectedItem is ThongKeMatHangBanItemViewModel item)
                {
                    string rowText = $"{item.SttStr}\t{item.TenHang}\t{item.SoLuong:0.##}\t{item.Dvt}\t{item.DonGia:N0}\t{item.TienGiam:N0}\t{item.ThanhTienBan:N0}\t{item.GiaVon:N0}\t{item.GiamGiaPhanTram:0.##}\t{item.ThanhTienNhap:N0}\t{item.Lai:N0}\t{item.TiLeLai:0.##}\t{item.MaHang}";
                    Clipboard.SetText(rowText);
                }
            }
            catch { }
        }

        private void MnuMatHangBan_TuDongDanCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgMatHangBan.Columns)
            {
                col.Width = DataGridLength.Auto;
            }
        }

        private void MnuMatHangBan_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string>
            {
                "Mặt hàng", "Số lượng", "ĐVT", "Đơn giá", "Tiền giảm", "Thành tiền bán",
                "Giá vốn", "Giảm giá %", "Thành tiền nhập", "Lãi", "Tỉ lệ lãi", "Mã hàng"
            };
            var win = new ChonCotHienThiWindow(DgMatHangBan, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MnuMatHangBan_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"ThongKeMatHangBan_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgMatHangBan?.ItemsSource as IEnumerable<ThongKeMatHangBanItemViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,Mã hàng,Mặt hàng,ĐVT,Số lượng,Đơn giá,Tiền giảm,Thành tiền bán,Giá vốn,Giảm giá %,Thành tiền nhập,Lãi,Tỉ lệ lãi");
                        int stt = 1;
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{stt++}\",\"{item.MaHang}\",\"{item.TenHang}\",\"{item.Dvt}\",\"{item.SoLuong}\",\"{item.DonGia}\",\"{item.TienGiam}\",\"{item.ThanhTienBan}\",\"{item.GiaVon}\",\"{item.GiamGiaPhanTram}\",\"{item.ThanhTienNhap}\",\"{item.Lai}\",\"{item.TiLeLai}\"");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất dữ liệu Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MnuMatHangBan_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var tuNgay = dpTuNgay.SelectedDate ?? DateTime.Today;
            var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;
            var win = new InLuoiWindow(DgMatHangBan, $"DANH SÁCH MẶT HÀNG BÁN ({tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy})");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        #endregion
    }
}
