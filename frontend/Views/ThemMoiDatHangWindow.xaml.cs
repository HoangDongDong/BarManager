using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemMoiDatHangWindow : Window
    {
        private LocalKhachDatHangService _service;
        private ObservableCollection<MatHangViewModel> _allMatHangs;
        private ObservableCollection<MatHangViewModel> _filteredMatHangs;
        private ObservableCollection<DatHangChiTietViewModel> _chiTiets;
        
        private List<DatHangViewModel> _orderList;
        private int _currentIndex = -1;
        
        public ThemMoiDatHangWindow(string orderId = null, List<DatHangViewModel> orderList = null)
        {
            _currentOrderId = orderId;
            _orderList = orderList;
            _chiTiets = new ObservableCollection<DatHangChiTietViewModel>();
            _allMatHangs = new ObservableCollection<MatHangViewModel>();
            _filteredMatHangs = new ObservableCollection<MatHangViewModel>();

            InitializeComponent();
            
            _service = new LocalKhachDatHangService();
            
            DgMatHang.ItemsSource = _filteredMatHangs;
            DgChiTiet.ItemsSource = _chiTiets;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupsAsync();
            await LoadMatHangsAsync();

            if (_orderList == null)
            {
                _orderList = await _service.GetDatHangListAsync(null, false);
            }

            if (!string.IsNullOrEmpty(_currentOrderId))
            {
                if (_orderList != null)
                {
                    _currentIndex = _orderList.FindIndex(x => x.Id == _currentOrderId);
                }
                await LoadOrderDataAsync(_currentOrderId);
            }
            else
            {
                // Init default values
                DpNgay.SelectedDate = DateTime.Now;
                DpTuNgay.SelectedDate = DateTime.Now;
                DpDenNgay.SelectedDate = DateTime.Now;
                TxtSoPhieu.Text = $"DH{DateTime.Now:yy}/{new Random().Next(1000, 9999)}";
                TxtGioVao.Text = DateTime.Now.ToString("HH:mm");
                TxtGioRa.Text = DateTime.Now.AddHours(2).ToString("HH:mm");
                TxtDatLuc.Text = DateTime.Now.ToString("HH:mm");
                _currentIndex = -1;
            }
            UpdateNavigationButtons();
        }

        private async Task LoadOrderDataAsync(string orderId)
        {
            _currentOrderId = orderId;
            if (string.IsNullOrEmpty(orderId)) return;

            Title = "Chỉnh sửa đơn đặt hàng";
            var order = await _service.GetDatHangByIdAsync(orderId);
            if (order != null)
            {
                DpNgay.SelectedDate = order.Ngay;
                DpTuNgay.SelectedDate = order.Tungay;
                DpDenNgay.SelectedDate = order.Denngay;
                TxtSoPhieu.Text = order.Name;
                TxtNguoiDat.Text = order.Tenkhach;
                TxtDiaChi.Text = order.Diachi;
                TxtDienThoai.Text = order.Dienthoai;
                TxtYeuCauKhac.Text = order.Note;

                if (order.Tugio.HasValue) TxtGioVao.Text = order.Tugio.Value.ToString("HH:mm");
                if (order.Dengio.HasValue) TxtGioRa.Text = order.Dengio.Value.ToString("HH:mm");
                if (order.Giodat.HasValue) TxtDatLuc.Text = order.Giodat.Value.ToString("HH:mm");

                if (!string.IsNullOrEmpty(order.DphuongthucdatId)) CmbDatQua.SelectedValue = order.DphuongthucdatId;
                if (!string.IsNullOrEmpty(order.DmucdichdatId)) CmbMucDich.SelectedValue = order.DmucdichdatId;

                if (!string.IsNullOrEmpty(order.DbanId))
                {
                    var banService = new LocalBanKhuVucService();
                    var bans = await banService.GetBanListAsync(null);
                    var foundBan = bans.FirstOrDefault(b => b.Id == order.DbanId);
                    if (foundBan != null)
                    {
                        TxtPhong.Text = foundBan.Name;
                        TxtPhong.Tag = foundBan.Id;
                    }
                    else
                    {
                        TxtPhong.Text = "";
                        TxtPhong.Tag = null;
                    }
                }
                else
                {
                    TxtPhong.Text = "";
                    TxtPhong.Tag = null;
                }

                if (!string.IsNullOrEmpty(order.DkhachhangId))
                {
                    var khList = await _service.GetKhachHangLookupAsync();
                    var foundKh = khList.FirstOrDefault(k => k.Id == order.DkhachhangId);
                    if (foundKh != null)
                    {
                        TxtKhachHang.Text = foundKh.Name;
                        TxtKhachHang.Tag = foundKh.Id;
                    }
                    else
                    {
                        TxtKhachHang.Text = order.Tenkhach ?? "";
                        TxtKhachHang.Tag = null;
                    }
                }
                else
                {
                    TxtKhachHang.Text = order.Tenkhach ?? "";
                    TxtKhachHang.Tag = null;
                }

                var chiTiets = await _service.GetDatHangChiTietListAsync(orderId);
                _chiTiets.Clear();
                foreach (var ct in chiTiets) _chiTiets.Add(ct);

                TxtGiamGiaPhanTram.Text = order.Tilegiamgia.HasValue ? order.Tilegiamgia.Value.ToString("0.##") : "0";
                TxtGiamGiaTien.Text = order.Tiengiamgia.HasValue ? order.Tiengiamgia.Value.ToString("N0") : "0";
                TxtThuePhanTram.Text = order.Tilethue.HasValue ? order.Tilethue.Value.ToString("0.##") : "0";
                TxtPhiVanChuyen.Text = !string.IsNullOrEmpty(order.Phivanchuyen) ? order.Phivanchuyen : "0";

                UpdateTotals();
            }
        }

        private void UpdateNavigationButtons()
        {
            if (BtnTruoc != null)
            {
                BtnTruoc.IsEnabled = _orderList != null && _currentIndex > 0;
            }
            if (BtnSau != null)
            {
                BtnSau.IsEnabled = _orderList != null && _currentIndex >= 0 && _currentIndex < _orderList.Count - 1;
            }
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_orderList != null && _currentIndex > 0)
            {
                _currentIndex--;
                var prevOrder = _orderList[_currentIndex];
                await LoadOrderDataAsync(prevOrder.Id);
                UpdateNavigationButtons();
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_orderList != null && _currentIndex >= 0 && _currentIndex < _orderList.Count - 1)
            {
                _currentIndex++;
                var nextOrder = _orderList[_currentIndex];
                await LoadOrderDataAsync(nextOrder.Id);
                UpdateNavigationButtons();
            }
        }

        private void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpNgay.SelectedDate.HasValue)
            {
                if (DpTuNgay != null) DpTuNgay.SelectedDate = DpNgay.SelectedDate.Value;
                if (DpDenNgay != null) DpDenNgay.SelectedDate = DpNgay.SelectedDate.Value;
            }
        }

        private async Task LoadLookupsAsync()
        {
            var phuongThucList = await _service.GetLookupListAsync(false);
            var mucDichList = await _service.GetLookupListAsync(true);

            CmbDatQua.ItemsSource = phuongThucList;
            CmbMucDich.ItemsSource = mucDichList;

            if (phuongThucList.Count > 0) CmbDatQua.SelectedIndex = 0;
            if (mucDichList.Count > 0) CmbMucDich.SelectedIndex = 0;
        }

        private async Task LoadMatHangsAsync()
        {
            var matHangsService = new LocalMatHangService();
            var allGroups = await matHangsService.GetAllNhomMatHangAsync();
            
            _allMatHangs.Clear();
            foreach (var grp in allGroups)
            {
                var mhs = await matHangsService.GetMatHangListAsync(grp.Id?.ToString());
                foreach (var mh in mhs)
                {
                    _allMatHangs.Add(mh);
                }
            }
            
            FilterMatHangs("");
        }

        private void FilterMatHangs(string searchText)
        {
            _filteredMatHangs.Clear();
            var query = _allMatHangs.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(x => 
                    (x.Name != null && x.Name.ToLower().Contains(searchText)) ||
                    (x.Code != null && x.Code.ToLower().Contains(searchText))
                );
            }

            int stt = 1;
            foreach (var item in query)
            {
                item.Stt = stt++;
                _filteredMatHangs.Add(item);
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterMatHangs(TxtTimKiem.Text);
        }

        private void BtnThemMatHang_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHang.SelectedItem as MatHangViewModel;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng để thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtSoLuong.Text, out decimal sl) || sl <= 0)
            {
                MessageBox.Show("Số lượng không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddMatHangToChiTiet(selected, sl);
        }

        private void DgMatHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = DgMatHang.SelectedItem as MatHangViewModel;
            // Lấy ID Khách hàng từ Tag của TxtKhachHang
            int? khachHangId = null;
            if (TxtKhachHang.Tag != null && int.TryParse(TxtKhachHang.Tag.ToString(), out int parsedKhachHangId))
            {
                khachHangId = parsedKhachHangId;
            }
            if (selected != null)
            {
                decimal sl = 1;
                if (decimal.TryParse(TxtSoLuong.Text, out decimal parsedSl) && parsedSl > 0)
                {
                    sl = parsedSl;
                }
                AddMatHangToChiTiet(selected, sl);
            }
        }

        private void AddMatHangToChiTiet(MatHangViewModel mh, decimal soLuong)
        {
            if (mh == null) return;

            var existing = _chiTiets.FirstOrDefault(x => 
                (!string.IsNullOrEmpty(x.MatHangId) && x.MatHangId == mh.Id?.ToString()) ||
                (!string.IsNullOrEmpty(x.MaHang) && !string.IsNullOrEmpty(mh.Code) && x.MaHang == mh.Code) ||
                (!string.IsNullOrEmpty(x.MatHangName) && !string.IsNullOrEmpty(mh.Name) && x.MatHangName.Equals(mh.Name, StringComparison.OrdinalIgnoreCase))
            );

            if (existing != null)
            {
                existing.SoLuong = (existing.SoLuong ?? 0) + soLuong;
                existing.ThanhTien = existing.SoLuong * (existing.DonGia ?? 0) * (1 - (existing.GiamGiaPhanTram ?? 0) / 100);
                
                // Refresh UI
                int index = _chiTiets.IndexOf(existing);
                _chiTiets.RemoveAt(index);
                _chiTiets.Insert(index, existing);
            }
            else
            {
                var ct = new DatHangChiTietViewModel
                {
                    Stt = _chiTiets.Count + 1,
                    MatHangId = mh.Id?.ToString(),
                    MaHang = mh.Code,
                    MatHangName = mh.Name,
                    DonViTinhName = mh.DonViTinhName,
                    SoLuong = soLuong,
                    DonGia = mh.Giaban ?? 0,
                    GiamGiaPhanTram = 0,
                    ThanhTien = soLuong * (mh.Giaban ?? 0)
                };
                _chiTiets.Add(ct);
            }

            UpdateTotals();
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgChiTiet.SelectedItem as DatHangChiTietViewModel;
            if (selected != null)
            {
                _chiTiets.Remove(selected);
                for (int i = 0; i < _chiTiets.Count; i++)
                {
                    _chiTiets[i].Stt = i + 1;
                }
                UpdateTotals();
            }
        }

        private void BtnNhapExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Không thể thực hiện import/cập nhật dữ liệu từ excel với dữ liệu này", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_chiTiets == null || _chiTiets.Count == 0)
                {
                    MessageBox.Show("Không có mặt hàng nào trong đơn đặt hàng để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string soPhieu = string.IsNullOrWhiteSpace(TxtSoPhieu.Text) ? DateTime.Now.ToString("yyyyMMddHHmm") : TxtSoPhieu.Text.Replace("/", "_");
                var saveDlg = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"ChiTietDatHang_{soPhieu}.xlsx",
                    Title = "Xuất danh sách chi tiết đơn đặt hàng ra Excel"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Chi Tiết Đặt Hàng");

                        // Tiêu đề
                        ws.Cell(1, 1).Value = "BAR & LOUNGE RESTAURANT";
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 13;

                        ws.Cell(2, 1).Value = "Địa chỉ: 12 Xuân Thủy, Cầu Giấy, Hà Nội - Hotline: (024) 3756 8888";
                        ws.Cell(2, 1).Style.Font.Italic = true;

                        ws.Cell(4, 1).Value = "DANH SÁCH MẶT HÀNG ĐẶT TRƯỚC";
                        ws.Cell(4, 1).Style.Font.Bold = true;
                        ws.Cell(4, 1).Style.Font.FontSize = 15;
                        ws.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(4, 1, 4, 8).Merge();

                        // Thông tin đơn hàng
                        ws.Cell(5, 1).Value = $"Số phiếu: {TxtSoPhieu.Text}";
                        ws.Cell(5, 1).Style.Font.Bold = true;
                        ws.Cell(5, 5).Value = $"Ngày: {DpNgay.SelectedDate?.ToString("dd/MM/yyyy")}";

                        ws.Cell(6, 1).Value = $"Khách hàng: {TxtKhachHang.Text}";
                        ws.Cell(6, 5).Value = $"Điện thoại: {TxtDienThoai.Text}";

                        ws.Cell(7, 1).Value = $"Địa chỉ: {TxtDiaChi.Text}";
                        ws.Cell(7, 5).Value = $"Phòng/Bàn: {TxtPhong.Text}";

                        ws.Cell(8, 1).Value = $"Giờ vào - ra: {TxtGioVao.Text} - {TxtGioRa.Text}";
                        ws.Cell(8, 5).Value = $"Phương thức: {CmbDatQua.Text} / {CmbMucDich.Text}";

                        // Bảng dữ liệu
                        int startRow = 10;
                        ws.Cell(startRow, 1).Value = "STT";
                        ws.Cell(startRow, 2).Value = "Mặt hàng";
                        ws.Cell(startRow, 3).Value = "ĐVT";
                        ws.Cell(startRow, 4).Value = "Số lượng";
                        ws.Cell(startRow, 5).Value = "Đơn giá";
                        ws.Cell(startRow, 6).Value = "Giảm giá %";
                        ws.Cell(startRow, 7).Value = "Thành tiền";
                        ws.Cell(startRow, 8).Value = "Ghi chú";

                        var headerRange = ws.Range(startRow, 1, startRow, 8);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2e75b6");
                        headerRange.Style.Font.FontColor = XLColor.White;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        int currentRow = startRow + 1;
                        int stt = 1;
                        foreach (var item in _chiTiets)
                        {
                            decimal tt = item.ThanhTien ?? ((item.SoLuong ?? 0) * (item.DonGia ?? 0));
                            ws.Cell(currentRow, 1).Value = stt++;
                            ws.Cell(currentRow, 2).Value = item.MatHangName;
                            ws.Cell(currentRow, 3).Value = item.DonViTinhName ?? "";
                            ws.Cell(currentRow, 4).Value = item.SoLuong ?? 1;
                            ws.Cell(currentRow, 5).Value = item.DonGia ?? 0;
                            ws.Cell(currentRow, 6).Value = item.GiamGiaPhanTram ?? 0;
                            ws.Cell(currentRow, 7).Value = tt;
                            ws.Cell(currentRow, 8).Value = item.GhiChu ?? "";

                            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";

                            currentRow++;
                        }

                        var dataTableRange = ws.Range(startRow, 1, currentRow - 1, 8);
                        dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Tổng cộng
                        decimal tienHang = _chiTiets.Sum(x => x.ThanhTien ?? ((x.SoLuong ?? 0) * (x.DonGia ?? 0)));
                        ws.Cell(currentRow, 6).Value = "Tiền hàng:";
                        ws.Cell(currentRow, 6).Style.Font.Bold = true;
                        ws.Cell(currentRow, 7).Value = tienHang;
                        ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                        currentRow++;

                        if (decimal.TryParse(TxtGiamGiaTien.Text, out decimal ggTien) && ggTien > 0)
                        {
                            ws.Cell(currentRow, 6).Value = $"Giảm giá ({TxtGiamGiaPhanTram.Text}%):";
                            ws.Cell(currentRow, 7).Value = ggTien;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "-#,##0";
                            currentRow++;
                        }

                        if (decimal.TryParse(TxtThueTien.Text, out decimal thueTien) && thueTien > 0)
                        {
                            ws.Cell(currentRow, 6).Value = $"Thuế VAT ({TxtThuePhanTram.Text}%):";
                            ws.Cell(currentRow, 7).Value = thueTien;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "+#,##0";
                            currentRow++;
                        }

                        if (decimal.TryParse(TxtPhiVanChuyen.Text, out decimal pvc) && pvc > 0)
                        {
                            ws.Cell(currentRow, 6).Value = "Phí vận chuyển:";
                            ws.Cell(currentRow, 7).Value = pvc;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "+#,##0";
                            currentRow++;
                        }

                        decimal tongCong = tienHang;
                        if (decimal.TryParse(TxtTongCong.Text?.Replace(",", "")?.Replace(".", ""), out decimal tc) && tc > 0)
                        {
                            tongCong = tc;
                        }
                        ws.Cell(currentRow, 6).Value = "TỔNG CỘNG:";
                        ws.Cell(currentRow, 6).Style.Font.Bold = true;
                        ws.Cell(currentRow, 6).Style.Font.FontSize = 12;
                        ws.Cell(currentRow, 7).Value = tongCong;
                        ws.Cell(currentRow, 7).Style.Font.Bold = true;
                        ws.Cell(currentRow, 7).Style.Font.FontSize = 12;
                        ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                        currentRow += 2;

                        if (!string.IsNullOrWhiteSpace(TxtYeuCauKhac.Text))
                        {
                            ws.Cell(currentRow, 1).Value = $"Yêu cầu khác: {TxtYeuCauKhac.Text}";
                            ws.Cell(currentRow, 1).Style.Font.Italic = true;
                        }

                        ws.Columns().AdjustToContents();

                        workbook.SaveAs(saveDlg.FileName);
                    }

                    var result = MessageBox.Show("Xuất file Excel thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtLocChiTiet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DgChiTiet == null || _chiTiets == null) return;
            string filter = TxtLocChiTiet?.Text?.Trim()?.ToLower() ?? "";
            if (string.IsNullOrEmpty(filter))
            {
                DgChiTiet.ItemsSource = _chiTiets;
            }
            else
            {
                var filtered = _chiTiets.Where(x =>
                    (x.MatHangName != null && x.MatHangName.ToLower().Contains(filter)) ||
                    (x.MaHang != null && x.MaHang.ToLower().Contains(filter)) ||
                    (x.GhiChu != null && x.GhiChu.ToLower().Contains(filter))
                ).ToList();
                DgChiTiet.ItemsSource = filtered;
            }
        }

        private void DgChiTiet_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var ct = e.Row.Item as DatHangChiTietViewModel;
            if (ct != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ct.ThanhTien = (ct.SoLuong ?? 0) * (ct.DonGia ?? 0) * (1 - (ct.GiamGiaPhanTram ?? 0) / 100);
                    UpdateTotals();
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void UpdateTotals()
        {
            if (_chiTiets == null || TxtTienHang == null || TxtGiamGiaPhanTram == null || TxtGiamGiaTien == null || 
                TxtThuePhanTram == null || TxtThueTien == null || TxtPhiVanChuyen == null || TxtTongCong == null) 
                return;
            
            decimal tienHang = _chiTiets.Sum(x => x.ThanhTien ?? 0);
            TxtTienHang.Text = tienHang.ToString("N0");

            decimal giamGiaTien = 0;
            if (decimal.TryParse(TxtGiamGiaPhanTram.Text, out decimal ggPhanTram) && ggPhanTram > 0)
            {
                giamGiaTien = tienHang * ggPhanTram / 100;
                TxtGiamGiaTien.Text = giamGiaTien.ToString("N0");
            }
            else if (decimal.TryParse(TxtGiamGiaTien.Text, out decimal ggTien) && ggTien > 0)
            {
                giamGiaTien = ggTien;
            }

            decimal sauGiamGia = tienHang - giamGiaTien;

            decimal thueTien = 0;
            if (decimal.TryParse(TxtThuePhanTram.Text, out decimal thuePhanTram) && thuePhanTram > 0)
            {
                thueTien = sauGiamGia * thuePhanTram / 100;
                TxtThueTien.Text = thueTien.ToString("N0");
            }

            decimal phiVanChuyen = 0;
            if (decimal.TryParse(TxtPhiVanChuyen.Text, out decimal pvc))
            {
                phiVanChuyen = pvc;
            }

            decimal tongCong = sauGiamGia + thueTien + phiVanChuyen;
            TxtTongCong.Text = tongCong.ToString("N0");
        }

        private void TxtGiamGiaPhanTram_TextChanged(object sender, TextChangedEventArgs e) { UpdateTotals(); }
        private void TxtGiamGiaTien_TextChanged(object sender, TextChangedEventArgs e) { UpdateTotals(); }
        private void TxtThuePhanTram_TextChanged(object sender, TextChangedEventArgs e) { UpdateTotals(); }
        private void TxtPhiVanChuyen_TextChanged(object sender, TextChangedEventArgs e) { UpdateTotals(); }

        public event System.Action OrderSaved;
        private string _currentOrderId = null;

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _currentOrderId = null;
            _currentIndex = -1;
            _chiTiets.Clear();
            UpdateTotals();
            DpNgay.SelectedDate = DateTime.Now;
            DpTuNgay.SelectedDate = DateTime.Now;
            DpDenNgay.SelectedDate = DateTime.Now;
            TxtSoPhieu.Text = $"DH{DateTime.Now:yy}/{new Random().Next(1000, 9999)}";
            TxtGioVao.Text = DateTime.Now.ToString("HH:mm");
            TxtGioRa.Text = DateTime.Now.AddHours(2).ToString("HH:mm");
            TxtDatLuc.Text = DateTime.Now.ToString("HH:mm");
            TxtPhong.Text = "";
            TxtPhong.Tag = null;
            TxtNguoiDat.Text = "";
            TxtDienThoai.Text = "";
            TxtDiaChi.Text = "";
            TxtYeuCauKhac.Text = "";
            TxtKhachHang.Text = "";
            TxtKhachHang.Tag = null;
            Title = "Thêm mới đơn đặt hàng";
            UpdateNavigationButtons();
        }

        private void Menu_TaoMoi_Click(object sender, RoutedEventArgs e) => BtnTaoMoi_Click(sender, e);
        
        private async Task<bool> SaveCurrentOrderAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtPhong.Text))
                {
                    MessageBox.Show("Vui lòng chọn phòng/bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    BtnChonPhong_Click(this, null);
                    return false;
                }

                string customerName = !string.IsNullOrWhiteSpace(TxtKhachHang.Text) ? TxtKhachHang.Text.Trim() : TxtNguoiDat.Text?.Trim();
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    MessageBox.Show("Vui lòng nhập tên khách hàng hoặc người đặt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtKhachHang.Focus();
                    return false;
                }

                DateTime selectedDate = DpNgay.SelectedDate ?? DateTime.Today;
                DateTime tuNgay = DpTuNgay.SelectedDate ?? selectedDate;
                DateTime denNgay = DpDenNgay.SelectedDate ?? selectedDate;

                var datHang = new LocalKhachDatHangService.DatHangSaveParam();
                datHang.Id = _currentOrderId;
                datHang.Name = TxtSoPhieu.Text?.Trim();
                datHang.Ngay = selectedDate;
                datHang.Tungay = tuNgay;
                datHang.Denngay = denNgay;
                datHang.Tenkhach = customerName;
                datHang.Diachi = TxtDiaChi.Text?.Trim();
                datHang.Dienthoai = TxtDienThoai.Text?.Trim();
                datHang.Note = TxtYeuCauKhac.Text?.Trim();

                if (decimal.TryParse(TxtTienHang.Text.Replace(",", "").Replace(".", ""), out decimal tienHang))
                    datHang.Tienhang = tienHang;
                if (decimal.TryParse(TxtGiamGiaPhanTram.Text, out decimal ggPt))
                    datHang.Tilegiamgia = ggPt;
                if (decimal.TryParse(TxtGiamGiaTien.Text.Replace(",", "").Replace(".", ""), out decimal ggTien))
                    datHang.Tiengiamgia = ggTien;
                if (decimal.TryParse(TxtThuePhanTram.Text, out decimal thuePt))
                    datHang.Tilethue = thuePt;
                if (decimal.TryParse(TxtThueTien.Text.Replace(",", "").Replace(".", ""), out decimal thueTien))
                    datHang.Tienthue = thueTien;
                
                datHang.Phivanchuyen = TxtPhiVanChuyen.Text?.Trim();
                datHang.Tongcong = TxtTongCong.Text?.Trim();

                if (TxtKhachHang.Tag != null)
                    datHang.DkhachhangId = TxtKhachHang.Tag.ToString();

                if (CmbDatQua.SelectedValue != null)
                    datHang.DphuongthucdatId = CmbDatQua.SelectedValue.ToString();

                if (CmbMucDich.SelectedValue != null)
                    datHang.DmucdichdatId = CmbMucDich.SelectedValue.ToString();

                // Kiểm tra định dạng giờ vào
                if (!TryParseTime(TxtGioVao.Text, out TimeSpan tsVao))
                {
                    MessageBox.Show("Giờ vào không hợp lệ! Vui lòng nhập đúng định dạng giờ:phút (ví dụ: 08:30 hoặc 18:00).", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtGioVao.Focus();
                    return false;
                }

                // Kiểm tra định dạng giờ ra
                if (!TryParseTime(TxtGioRa.Text, out TimeSpan tsRa))
                {
                    MessageBox.Show("Giờ ra không hợp lệ! Vui lòng nhập đúng định dạng giờ:phút (ví dụ: 20:30 hoặc 22:00).", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtGioRa.Focus();
                    return false;
                }

                // Kiểm tra định dạng đặt lúc nếu có nhập
                TimeSpan tsDat = TimeSpan.Zero;
                if (!string.IsNullOrWhiteSpace(TxtDatLuc.Text))
                {
                    if (!TryParseTime(TxtDatLuc.Text, out tsDat))
                    {
                        MessageBox.Show("Giờ 'Đặt lúc' không hợp lệ! Vui lòng nhập đúng định dạng giờ:phút (ví dụ: 08:00).", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtDatLuc.Focus();
                        return false;
                    }
                }
                else
                {
                    tsDat = DateTime.Now.TimeOfDay;
                }

                DateTime dateTimeVao = tuNgay.Date + tsVao;
                DateTime dateTimeRa = denNgay.Date + tsRa;

                if (dateTimeRa < dateTimeVao)
                {
                    MessageBox.Show("Thời gian ra (ngày và giờ ra) không thể trước thời gian vào!", "Lỗi thời gian", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtGioRa.Focus();
                    return false;
                }

                datHang.Tugio = dateTimeVao;
                datHang.Dengio = dateTimeRa;
                datHang.Giodat = selectedDate.Date + tsDat;

                if (TxtPhong.Tag is List<BanViewModel> selectedBans && selectedBans.Count > 0)
                {
                    datHang.DbanId = selectedBans[0].Id;
                }

                var savedId = await _service.SaveDatHangAsync(datHang, _chiTiets.ToList());
                if (!string.IsNullOrEmpty(savedId))
                {
                    _currentOrderId = savedId;
                    try
                    {
                        OrderSaved?.Invoke();
                    }
                    catch { }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu đơn đặt hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void Menu_Luu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentOrderAsync())
            {
                MessageBox.Show("Lưu đơn đặt hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void Menu_LuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentOrderAsync())
            {
                MessageBox.Show("Lưu đơn đặt hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnTaoMoi_Click(sender, e);
            }
        }

        private async void Menu_LuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentOrderAsync())
            {
                MessageBox.Show("Lưu đơn đặt hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void Menu_LuuVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentOrderAsync())
            {
                var order = await _service.GetDatHangByIdAsync(_currentOrderId);
                var details = _chiTiets.ToList();
                string phongBan = TxtPhong.Text;
                string phuongThuc = (CmbDatQua.SelectedItem as LookupItem)?.Name ?? CmbDatQua.Text;
                string mucDich = (CmbMucDich.SelectedItem as LookupItem)?.Name ?? CmbMucDich.Text;

                var dlg = new InBaoCaoWindow(order, details, phongBan, phuongThuc, mucDich, isPrintToPrinter: true);
                dlg.Owner = this;
                dlg.ShowDialog();
            }
        }

        private async void Menu_LuuVaXemIn_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveCurrentOrderAsync())
            {
                var order = await _service.GetDatHangByIdAsync(_currentOrderId);
                var details = _chiTiets.ToList();
                string phongBan = TxtPhong.Text;
                string phuongThuc = (CmbDatQua.SelectedItem as LookupItem)?.Name ?? CmbDatQua.Text;
                string mucDich = (CmbMucDich.SelectedItem as LookupItem)?.Name ?? CmbMucDich.Text;

                var dlg = new InBaoCaoWindow(order, details, phongBan, phuongThuc, mucDich, isPrintToPrinter: false);
                dlg.Owner = this;
                dlg.ShowDialog();
            }
        }

        private void Menu_Thoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Menu_Thoat_Click(sender, e);
            }
            else if (e.Key == Key.F10 || (e.Key == Key.System && e.SystemKey == Key.F10))
            {
                e.Handled = true;
                BtnTruoc_Click(sender, e);
            }
            else if (e.Key == Key.F11 || (e.Key == Key.System && e.SystemKey == Key.F11))
            {
                e.Handled = true;
                BtnSau_Click(sender, e);
            }
            else if (e.Key == Key.F8)
            {
                BtnChonPhong_Click(sender, e);
            }
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                BtnTaoMoi_Click(sender, e);
            }
            else if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Menu_Luu_Click(sender, e);
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Menu_LuuVaMoi_Click(sender, e);
            }
            else if (e.Key == Key.P && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                Menu_LuuVaXemIn_Click(sender, e);
            }
            else if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Menu_LuuVaIn_Click(sender, e);
            }
        }

        private void BtnChonPhong_Click(object sender, RoutedEventArgs e)
        {
            var currentSelected = TxtPhong.Tag as List<BanViewModel>;
            var preselectedIds = currentSelected?.Select(x => x.Id).ToList();

            var window = new ChonPhongWindow(preselectedIds);
            window.Owner = this;
            if (window.ShowDialog() == true && window.SelectedBans != null)
            {
                TxtPhong.Text = string.Join(", ", window.SelectedBans.Select(x => x.Name));
                TxtPhong.Tag = window.SelectedBans; // Lưu danh sách phòng vào Tag
            }
        }

        // --- Logic Khách Hàng ---
        private List<KhachHangLookupViewModel> _allKhachHangs = new List<KhachHangLookupViewModel>();
        private ObservableCollection<KhachHangLookupViewModel> _filteredKhachHangs = new ObservableCollection<KhachHangLookupViewModel>();

        private async void TxtKhachHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PopupKhachHang != null && !PopupKhachHang.IsOpen && TxtKhachHang.IsFocused)
            {
                PopupKhachHang.IsOpen = true;
                BtnKhachHangToggle.IsChecked = true;
            }
            await FilterKhachHangAsync(TxtKhachHang.Text);
        }

        private async void BtnKhachHangToggle_Click(object sender, RoutedEventArgs e)
        {
            if (BtnKhachHangToggle.IsChecked == true)
            {
                await LoadKhachHangAsync();
                await FilterKhachHangAsync(TxtKhachHang.Text);
            }
        }

        private async void BtnKhachHangReload_Click(object sender, RoutedEventArgs e)
        {
            _allKhachHangs.Clear();
            await LoadKhachHangAsync();
            await FilterKhachHangAsync(TxtKhachHang.Text);
        }

        private async Task LoadKhachHangAsync()
        {
            if (_allKhachHangs.Count == 0)
            {
                var list = await _service.GetKhachHangLookupAsync();
                _allKhachHangs = list.Where(x => !string.IsNullOrEmpty(x.Id) || !string.IsNullOrEmpty(x.Name)).ToList();
            }
        }

        private async Task FilterKhachHangAsync(string filter)
        {
            await LoadKhachHangAsync();
            _filteredKhachHangs.Clear();
            
            var query = _allKhachHangs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(filter)) ||
                    (x.Makhach != null && x.Makhach.ToLower().Contains(filter)) ||
                    (x.Dienthoai != null && x.Dienthoai.ToLower().Contains(filter)) ||
                    (x.Diachi != null && x.Diachi.ToLower().Contains(filter))
                );
            }

            foreach (var item in query)
            {
                _filteredKhachHangs.Add(item);
            }
            DgKhachHang.ItemsSource = _filteredKhachHangs;
        }

        private void DgKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgKhachHang.SelectedItem is KhachHangLookupViewModel selected)
            {
                TxtKhachHang.Text = selected.Name;
                TxtKhachHang.Tag = selected.Id;
                TxtNguoiDat.Text = selected.Name;
                TxtDienThoai.Text = selected.Dienthoai ?? "";
                TxtDiaChi.Text = selected.Diachi ?? "";
                PopupKhachHang.IsOpen = false;
                BtnKhachHangToggle.IsChecked = false;
            }
        }

        private void BtnThemKhachHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm khách hàng đang được xây dựng!");
        }

        private bool TryParseTime(string text, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();

            string[] parts = text.Split(new[] { ':', '.', '-', 'h', 'H' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
            {
                if (h >= 0 && h <= 23 && m >= 0 && m <= 59)
                {
                    time = new TimeSpan(h, m, 0);
                    return true;
                }
            }
            else if (TimeSpan.TryParse(text, out time))
            {
                if (time.TotalHours >= 0 && time.TotalHours < 24)
                    return true;
            }
            return false;
        }

        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
            {
                if (TryParseTime(tb.Text, out TimeSpan ts))
                {
                    tb.Text = $"{ts.Hours:D2}:{ts.Minutes:D2}";
                }
            }
        }
    }
}
