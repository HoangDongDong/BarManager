using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.CongNo
{
    public partial class CongNoNhaCungCapControl : UserControl
    {
        private ObservableCollection<NhomKhachHangTreeItem> _nhomTree = new ObservableCollection<NhomKhachHangTreeItem>();
        private List<CongNoNhaCungCapViewModel> _nccList = new List<CongNoNhaCungCapViewModel>();
        private List<ChiTietCongNoNccItemViewModel> _currentDetails = new List<ChiTietCongNoNccItemViewModel>();
        private CongNoNhaCungCapViewModel _selectedNcc = null;

        private string _selectedNhomId = "ALL";
        private string _keyword = "";
        private int _debtFilterMode = 0; // 0: Tất cả, 1: Chỉ còn nợ, 2: Có phát sinh
        private DateTime? _tuNgay = null;
        private DateTime? _denNgay = null;
        private bool _isComboUpdating = false;

        public CongNoNhaCungCapControl()
        {
            InitializeComponent();
            this.KeyDown += CongNoNhaCungCapControl_KeyDown;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
            await LoadDropdownNhomAsync();
            await LoadDataAsync();
        }

        private void CongNoNhaCungCapControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                TxtTimKiem.Focus();
                TxtTimKiem.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                BtnChiNo_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                BtnRefresh_Click(null, null);
                e.Handled = true;
            }
        }

        private int _treeSortMode = 0; // 0: Tên, 1: Thứ tự tùy chọn

        private async Task LoadTreeAsync()
        {
            try
            {
                _nhomTree = await LocalCongNoNhaCungCapService.GetNhomNhaCungCapTreeAsync(_treeSortMode);
                TvNhomNhaCungCap.ItemsSource = _nhomTree;

                if (_nhomTree.Count > 0)
                {
                    _selectedNhomId = _nhomTree[0].Id;
                    _nhomTree[0].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTreeAsync in CongNoNhaCungCapControl: " + ex.Message);
            }
        }

        private async void MiTreeSortTen_Click(object sender, RoutedEventArgs e)
        {
            _treeSortMode = 0;
            if (MiTreeSortTen != null) MiTreeSortTen.IsChecked = true;
            if (MiTreeSortTuyChon != null) MiTreeSortTuyChon.IsChecked = false;
            await LoadTreeAsync();
        }

        private async void MiTreeSortTuyChon_Click(object sender, RoutedEventArgs e)
        {
            _treeSortMode = 1;
            if (MiTreeSortTen != null) MiTreeSortTen.IsChecked = false;
            if (MiTreeSortTuyChon != null) MiTreeSortTuyChon.IsChecked = true;
            await LoadTreeAsync();
        }

        private async Task LoadDropdownNhomAsync()
        {
            try
            {
                _isComboUpdating = true;
                var list = await LocalCongNoNhaCungCapService.GetNhomNhaCungCapDropdownAsync();
                CboBottomNhomNcc.ItemsSource = list;
                if (list.Count > 0)
                {
                    CboBottomNhomNcc.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadDropdownNhomAsync: " + ex.Message);
            }
            finally
            {
                _isComboUpdating = false;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _nccList = await LocalCongNoNhaCungCapService.GetCongNoNhaCungCapListAsync(
                    _selectedNhomId,
                    _keyword,
                    _debtFilterMode,
                    _tuNgay,
                    _denNgay
                );

                DgCongNoNhaCungCap.ItemsSource = _nccList;

                // Tính tổng công nợ
                decimal tongNo = _nccList.Sum(x => x.ConNo);
                TxtTongNo.Text = tongNo.ToString("N0");

                // Chọn lại nhà cung cấp nếu trước đó đã chọn
                if (_selectedNcc != null)
                {
                    var found = _nccList.FirstOrDefault(x => x.Id == _selectedNcc.Id);
                    if (found != null)
                    {
                        DgCongNoNhaCungCap.SelectedItem = found;
                    }
                    else if (_nccList.Count > 0)
                    {
                        DgCongNoNhaCungCap.SelectedIndex = 0;
                    }
                    else
                    {
                        ClearDetails();
                    }
                }
                else if (_nccList.Count > 0)
                {
                    DgCongNoNhaCungCap.SelectedIndex = 0;
                }
                else
                {
                    ClearDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách công nợ nhà cung cấp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearDetails()
        {
            _selectedNcc = null;
            _currentDetails.Clear();
            DgChiTietCongNo.ItemsSource = null;
        }

        private async void DgCongNoNhaCungCap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgCongNoNhaCungCap.SelectedItem is CongNoNhaCungCapViewModel item)
            {
                _selectedNcc = item;
                try
                {
                    _currentDetails = await LocalCongNoNhaCungCapService.GetChiTietCongNoNhaCungCapAsync(
                        item.Id,
                        _tuNgay,
                        _denNgay
                    );
                    DgChiTietCongNo.ItemsSource = _currentDetails;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading supplier debt details: " + ex.Message);
                }
            }
            else
            {
                ClearDetails();
            }
        }

        private void DgCongNoNhaCungCap_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void DgChiTietCongNo_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private async void TvNhomNhaCungCap_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem node)
            {
                _selectedNhomId = node.Id;

                // Đồng bộ sang dropdown dưới
                _isComboUpdating = true;
                if (CboBottomNhomNcc.ItemsSource is List<NhomNccDropdownItem> list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Id == node.Id)
                        {
                            CboBottomNhomNcc.SelectedIndex = i;
                            break;
                        }
                    }
                }
                _isComboUpdating = false;

                await LoadDataAsync();
            }
        }

        private async void CboBottomNhomNcc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isComboUpdating) return;

            if (CboBottomNhomNcc.SelectedValue != null)
            {
                _selectedNhomId = CboBottomNhomNcc.SelectedValue.ToString();
                await LoadDataAsync();
            }
        }

        private async void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            _keyword = TxtTimKiem.Text.Trim();
            await LoadDataAsync();
        }

        private void TxtTimKiem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                DgCongNoNhaCungCap.Focus();
                if (DgCongNoNhaCungCap.Items.Count > 0 && DgCongNoNhaCungCap.SelectedIndex < 0)
                {
                    DgCongNoNhaCungCap.SelectedIndex = 0;
                }
            }
        }

        private void BtnCheDoHienThi_Click(object sender, RoutedEventArgs e)
        {
            CmCheDoHienThi.PlacementTarget = BtnCheDoHienThi;
            CmCheDoHienThi.IsOpen = true;
        }

        private async void MenuItem_HienThiTatCa_Click(object sender, RoutedEventArgs e)
        {
            _debtFilterMode = 0;
            TxtTenCheDoHienThi.Text = "Hiển thị tất cả";
            await LoadDataAsync();
        }

        private async void MenuItem_ChiConNo_Click(object sender, RoutedEventArgs e)
        {
            _debtFilterMode = 1;
            TxtTenCheDoHienThi.Text = "Chỉ còn nợ";
            await LoadDataAsync();
        }

        private async void MenuItem_CoPhatSinh_Click(object sender, RoutedEventArgs e)
        {
            _debtFilterMode = 2;
            TxtTenCheDoHienThi.Text = "Có phát sinh";
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void BtnChiNo_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNcc == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp trong danh sách trước khi thực hiện thanh toán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new ChiCongNoNhaCungCapWindow(_selectedNcc);
            var parent = Window.GetWindow(this);
            if (parent != null) win.Owner = parent;

            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private void DgCongNoNhaCungCap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject dep)
            {
                // Bỏ qua nếu double click vào header hoặc scrollbar
                var header = FindVisualParent<System.Windows.Controls.Primitives.DataGridColumnHeader>(dep);
                if (header != null) return;

                var scrollBar = FindVisualParent<System.Windows.Controls.Primitives.ScrollBar>(dep);
                if (scrollBar != null) return;

                var row = FindVisualParent<DataGridRow>(dep);
                if (row != null && row.Item is CongNoNhaCungCapViewModel item)
                {
                    _selectedNcc = item;
                    DgCongNoNhaCungCap.SelectedItem = item;
                    e.Handled = true;
                    BtnChiNo_Click(null, null);
                    return;
                }
            }

            if (_selectedNcc != null)
            {
                BtnChiNo_Click(null, null);
            }
        }

        private void DgChiTietCongNo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject dep)
            {
                var header = FindVisualParent<System.Windows.Controls.Primitives.DataGridColumnHeader>(dep);
                if (header != null) return;

                var scrollBar = FindVisualParent<System.Windows.Controls.Primitives.ScrollBar>(dep);
                if (scrollBar != null) return;
            }

            if (_selectedNcc != null)
            {
                BtnChiNo_Click(null, null);
            }
        }

        private void BtnInTongHopNo_Click(object sender, RoutedEventArgs e)
        {
            if (_nccList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu công nợ để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"TongHopCongNoNhaCungCap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                bool success = LocalCongNoNhaCungCapService.ExportCongNoToExcel(_nccList, sfd.FileName);
                if (success)
                {
                    var res = MessageBox.Show("Xuất báo cáo tổng hợp công nợ thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Xuất báo cáo thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region ContextMenu Handlers Nhà Cung Cấp
        private void MenuItem_Ncc_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNcc != null)
            {
                Clipboard.SetText(_selectedNcc.Name);
            }
        }

        private void MenuItem_Ncc_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNcc != null)
            {
                string text = $"{_selectedNcc.Name}\t{_selectedNcc.MaNhaCungCap}\t{_selectedNcc.DiaChi}\t{_selectedNcc.DienThoai}\t{_selectedNcc.Email}\t{_selectedNcc.ConNoFormatted}";
                Clipboard.SetText(text);
            }
        }

        private void MenuItem_Ncc_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgCongNoNhaCungCap.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MenuItem_Ncc_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgCongNoNhaCungCap, "Công nợ nhà cung cấp");
            var parent = Window.GetWindow(this);
            if (parent != null) win.Owner = parent;
            win.ShowDialog();
        }

        private void MenuItem_Ncc_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            BtnInTongHopNo_Click(sender, e);
        }

        private void MenuItem_Ncc_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgCongNoNhaCungCap, "TỔNG HỢP CÔNG NỢ NHÀ CUNG CẤP");
            var parent = Window.GetWindow(this);
            if (parent != null) win.Owner = parent;
            win.ShowDialog();
        }
        #endregion

        #region ContextMenu Handlers Chi Tiết Công Nợ
        private void MenuItem_ChiTiet_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTietCongNo.SelectedItem is ChiTietCongNoNccItemViewModel ct)
            {
                Clipboard.SetText(ct.SoPhieu);
            }
        }

        private void MenuItem_ChiTiet_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTietCongNo.SelectedItem is ChiTietCongNoNccItemViewModel ct)
            {
                string text = $"{ct.SoPhieu}\t{ct.NgayFormatted}\t{ct.TongCongFormatted}\t{ct.DienGiai}\t{ct.TienThanhToanFormatted}\t{ct.LuyKeFormatted}";
                Clipboard.SetText(text);
            }
        }

        private void MenuItem_ChiTiet_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgChiTietCongNo.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MenuItem_ChiTiet_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgChiTietCongNo, "Sổ chi tiết công nợ");
            var parent = Window.GetWindow(this);
            if (parent != null) win.Owner = parent;
            win.ShowDialog();
        }

        private void MenuItem_ChiTiet_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MiXuatChiTietExcel_Click(sender, e);
        }

        private void MiXuatChiTietExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNcc == null || _currentDetails.Count == 0)
            {
                MessageBox.Show("Không có chi tiết phát sinh công nợ để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"SoChiTietCongNo_{_selectedNcc.MaNhaCungCap}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                bool success = LocalCongNoNhaCungCapService.ExportChiTietCongNoToExcel(_selectedNcc.Name, _currentDetails, sfd.FileName);
                if (success)
                {
                    var res = MessageBox.Show("Xuất sổ chi tiết công nợ thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Xuất sổ chi tiết thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_ChiTiet_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            string title = _selectedNcc != null ? $"SỔ CHI TIẾT CÔNG NỢ - {_selectedNcc.Name.ToUpper()}" : "SỔ CHI TIẾT CÔNG NỢ";
            var win = new InLuoiWindow(DgChiTietCongNo, title);
            var parent = Window.GetWindow(this);
            if (parent != null) win.Owner = parent;
            win.ShowDialog();
        }
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typed) return typed;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        #endregion
    }
}
