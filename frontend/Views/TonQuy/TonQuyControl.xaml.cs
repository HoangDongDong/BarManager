using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TonQuy
{
    public partial class TonQuyControl : UserControl
    {
        private bool _isLoaded = false;
        private List<QuyFilterTreeItem> _treeItems = new List<QuyFilterTreeItem>();

        public TonQuyControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            // Thiết lập ngày mặc định: Đầu tháng hiện tại -> Hôm nay
            DpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DpDenNgay.SelectedDate = null; // Hoặc để trống như mẫu

            await LoadLookupsAsync();
            await LoadDataAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                // 1. Tải danh mục cửa hàng
                var chList = await LocalTonQuyService.GetCuaHangListAsync();
                var fullChList = new List<dynamic>();
                fullChList.Add(new { ID = "0", NAME = "Tất cả" });
                fullChList.AddRange(chList);

                CboCuaHang.ItemsSource = fullChList;
                CboCuaHang.SelectedIndex = 0;

                // 2. Tải cây Quỹ
                _treeItems = await LocalTonQuyService.GetTreeQuyAsync();
                TvQuy.ItemsSource = _treeItems;

                if (_treeItems.Count > 0)
                {
                    _treeItems[0].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                DateTime? tuNgay = DpTuNgay.SelectedDate;
                DateTime? denNgay = DpDenNgay.SelectedDate;

                string cuaHangId = CboCuaHang.SelectedValue?.ToString();
                if (cuaHangId == "0") cuaHangId = null;

                string quyCode = "ALL";
                string taiKhoanId = null;

                if (TvQuy.SelectedItem is QuyFilterTreeItem selectedNode)
                {
                    quyCode = selectedNode.Code;
                    taiKhoanId = selectedNode.TaiKhoanId;
                }

                var result = await LocalTonQuyService.GetBaoCaoTonQuyAsync(tuNgay, denNgay, cuaHangId, quyCode, taiKhoanId);

                DgGiaoDich.ItemsSource = result.DanhSachGiaoDich;

                TxtTonDau.Text = result.TonDau.ToString("N0");
                TxtTongThu.Text = result.TongThu.ToString("N0");
                TxtTongChi.Text = result.TongChi.ToString("N0");
                TxtTonQuy.Text = result.TonQuy.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu tồn quỹ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private async void TvQuy_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private void TvQuy_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void DgGiaoDich_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenDetailCurrentRow();
        }

        private void MenuXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            OpenDetailCurrentRow();
        }

        private void OpenDetailCurrentRow()
        {
            if (DgGiaoDich.SelectedItem is GiaoDichTonQuyItem item)
            {
                try
                {
                    string soPhieu = item.SoPhieu?.Trim() ?? "";
                    string upper = soPhieu.ToUpperInvariant();

                    string idOrSoPhieu = !string.IsNullOrEmpty(item.ChungTuId) ? item.ChungTuId : item.SoPhieu;

                    // Phiếu nhập kho: PN... hoặc NK...
                    if (item.LoaiChungTu == "NHAP_KHO" || upper.StartsWith("PN") || upper.StartsWith("NK"))
                    {
                        var win = new QuanLyBar.Client.Views.QuanLyNhapKho.ThemPhieuNhapKhoWindow(idOrSoPhieu, null);
                        win.Owner = Window.GetWindow(this);
                        win.ShowDialog();
                        _ = LoadDataAsync();
                    }
                    // Phiếu chi: PC...
                    else if (item.LoaiChungTu == "PHIEU_CHI" || upper.StartsWith("PC"))
                    {
                        var win = new QuanLyBar.Client.Views.PhieuThuChi.TaoPhieuChiWindow(idOrSoPhieu);
                        win.Owner = Window.GetWindow(this);
                        win.ShowDialog();
                        _ = LoadDataAsync();
                    }
                    // Phiếu thu: PT...
                    else if (item.LoaiChungTu == "PHIEU_THU" || upper.StartsWith("PT"))
                    {
                        var win = new QuanLyBar.Client.Views.PhieuThuChi.TaoPhieuThuWindow(idOrSoPhieu);
                        win.Owner = Window.GetWindow(this);
                        win.ShowDialog();
                        _ = LoadDataAsync();
                    }
                    // Hóa đơn bán hàng (092600001, HD26/..., số thuần)
                    else if (item.LoaiChungTu == "HOA_DON" || char.IsDigit(upper.FirstOrDefault()) || upper.StartsWith("HD"))
                    {
                        var win = new QuanLyBar.Client.Views.QuanLyBanHang.HoaDonNhaHangWindow(idOrSoPhieu);
                        win.Owner = Window.GetWindow(this);
                        win.ShowDialog();
                        _ = LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Chứng từ: {item.SoPhieu}\nNgày: {item.NgayHienThi}\nDiễn giải: {item.DienGiai}\nThu: {item.Thu:N0}\nChi: {item.Chi:N0}",
                            "Thông tin chứng từ", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi mở chứng từ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void MenuSaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (DgGiaoDich.CurrentCell.Item is GiaoDichTonQuyItem item && DgGiaoDich.CurrentColumn != null)
            {
                var colHeader = DgGiaoDich.CurrentColumn.Header?.ToString();
                string val = "";
                if (colHeader == "Số phiếu") val = item.SoPhieu;
                else if (colHeader == "Ngày") val = item.NgayHienThi;
                else if (colHeader == "Diễn giải") val = item.DienGiai;
                else if (colHeader == "Thu") val = item.ThuHienThi;
                else if (colHeader == "Chi") val = item.ChiHienThi;

                if (!string.IsNullOrEmpty(val)) Clipboard.SetText(val);
            }
        }

        private void MenuSaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgGiaoDich.SelectedItem is GiaoDichTonQuyItem item)
            {
                string rowText = $"{item.Stt}\t{item.SoPhieu}\t{item.NgayHienThi}\t{item.DienGiai}\t{item.ThuHienThi}\t{item.ChiHienThi}";
                Clipboard.SetText(rowText);
            }
        }

        private void MenuXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgGiaoDich.ItemsSource is List<GiaoDichTonQuyItem> list && list.Count > 0)
                {
                    var sfd = new SaveFileDialog
                    {
                        Filter = "CSV File (*.csv)|*.csv",
                        FileName = $"TonQuy_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                    };

                    if (sfd.ShowDialog() == true)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,SoPhieu,Ngay,DienGiai,Thu,Chi");
                        foreach (var item in list)
                        {
                            sb.AppendLine($"\"{item.Stt}\",\"{item.SoPhieu}\",\"{item.NgayHienThi}\",\"{item.DienGiai.Replace("\"", "\"\"")}\",\"{item.Thu}\",\"{item.Chi}\"");
                        }
                        System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuInDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgGiaoDich, "Báo cáo tồn quỹ");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }
    }
}
