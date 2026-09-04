using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public partial class CongNoKhachHangControl : UserControl
    {
        private ObservableCollection<NhomKhachHangTreeItem> _nhomTree;
        private List<CongNoKhachHangViewModel> _customerList = new List<CongNoKhachHangViewModel>();
        private List<ChiTietCongNoItemViewModel> _currentDetails = new List<ChiTietCongNoItemViewModel>();
        private CongNoKhachHangViewModel _selectedCustomer = null;
        private string _selectedNhomId = "ALL";

        // Filter states
        private DateTime? _tuNgay = null;
        private DateTime? _denNgay = null;
        private int _debtFilterMode = 0; // 0: Tất cả, 1: Chỉ còn nợ > 0, 2: Có phát sinh
        private string _keyword = "";

        public CongNoKhachHangControl()
        {
            InitializeComponent();
            this.KeyDown += CongNoKhachHangControl_KeyDown;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
            await LoadDataAsync();
        }

        private void CongNoKhachHangControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                TxtTimKiem.Focus();
                TxtTimKiem.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                BtnThuNo_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                BtnRefresh_Click(null, null);
                e.Handled = true;
            }
        }

        private async Task LoadTreeAsync()
        {
            try
            {
                _nhomTree = await LocalCongNoKhachHangService.GetNhomKhachHangTreeAsync();
                TvNhomKhachHang.ItemsSource = _nhomTree;

                if (_nhomTree.Count > 0)
                {
                    _selectedNhomId = _nhomTree[0].Id;
                    _nhomTree[0].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTreeAsync in CongNoKhachHangControl: " + ex.Message);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _customerList = await LocalCongNoKhachHangService.GetCongNoKhachHangListAsync(
                    _selectedNhomId,
                    _keyword,
                    _debtFilterMode,
                    _tuNgay,
                    _denNgay
                );

                DgCongNoKhachHang.ItemsSource = _customerList;

                // Tính tổng công nợ
                decimal tongNo = _customerList.Sum(x => x.ConNo);
                TxtTongNo.Text = tongNo.ToString("N0");

                // Chọn lại khách hàng nếu trước đó đã chọn
                if (_selectedCustomer != null)
                {
                    var found = _customerList.FirstOrDefault(x => x.Id == _selectedCustomer.Id);
                    if (found != null)
                    {
                        DgCongNoKhachHang.SelectedItem = found;
                    }
                    else if (_customerList.Count > 0)
                    {
                        DgCongNoKhachHang.SelectedIndex = 0;
                    }
                    else
                    {
                        ClearDetails();
                    }
                }
                else if (_customerList.Count > 0)
                {
                    DgCongNoKhachHang.SelectedIndex = 0;
                }
                else
                {
                    ClearDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách công nợ khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearDetails()
        {
            _selectedCustomer = null;
            _currentDetails.Clear();
            DgChiTietCongNo.ItemsSource = null;
        }

        private async void DgCongNoKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgCongNoKhachHang.SelectedItem is CongNoKhachHangViewModel item)
            {
                _selectedCustomer = item;
                try
                {
                    _currentDetails = await LocalCongNoKhachHangService.GetChiTietCongNoKhachHangAsync(item.Id, _tuNgay, _denNgay);
                    DgChiTietCongNo.ItemsSource = _currentDetails;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading customer debt details: " + ex.Message);
                }
            }
            else
            {
                ClearDetails();
            }
        }

        private void DgCongNoKhachHang_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private async void TvNhomKhachHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem node)
            {
                _selectedNhomId = node.Id;
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
                DgCongNoKhachHang.Focus();
                if (DgCongNoKhachHang.Items.Count > 0 && DgCongNoKhachHang.SelectedIndex < 0)
                {
                    DgCongNoKhachHang.SelectedIndex = 0;
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

        private async void BtnThuNo_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn một khách hàng trong danh sách trước khi thực hiện thanh toán nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new ThuCongNoKhachHangWindow(_selectedCustomer);
            var parent = Window.GetWindow(this);
            if (parent != null) win.Owner = parent;

            if (win.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private void DgCongNoKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedCustomer != null)
            {
                BtnThuNo_Click(null, null);
            }
        }

        private void BtnInTongHopNo_Click(object sender, RoutedEventArgs e)
        {
            if (_customerList == null || _customerList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu công nợ để in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"BaoCao_TongHopCongNo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Xuất bảng tổng hợp công nợ khách hàng"
                };

                if (sfd.ShowDialog() == true)
                {
                    LocalCongNoKhachHangService.ExportCongNoToExcel(_customerList, sfd.FileName);
                    var res = MessageBox.Show("Đã xuất báo cáo tổng hợp công nợ thành công! Bạn có muốn mở file ngay không?", "Thành công", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MiInSoChiTiet_Click(object sender, RoutedEventArgs e)
        {
            MiXuatChiTietExcel_Click(sender, e);
        }

        private void MiXuatChiTietExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xuất sổ chi tiết công nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentDetails == null || _currentDetails.Count == 0)
            {
                MessageBox.Show("Khách hàng này chưa có giao dịch phát sinh công nợ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"ChiTietCongNo_{_selectedCustomer.Makhach}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Xuất sổ chi tiết công nợ khách hàng"
                };

                if (sfd.ShowDialog() == true)
                {
                    LocalCongNoKhachHangService.ExportChiTietCongNoToExcel(_selectedCustomer, _currentDetails, sfd.FileName);
                    var res = MessageBox.Show("Đã xuất sổ chi tiết công nợ thành công! Bạn có muốn mở file ngay không?", "Thành công", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file chi tiết: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
