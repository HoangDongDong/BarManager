using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class SuDungDichVuControl : UserControl
    {
        private LocalSuDungDichVuService _service;
        private PosBanViewModel _currentBan;
        private List<PosKhuVucViewModel> _khuVucList;
        private string _selectedNhomId = string.Empty;
        private DispatcherTimer _timer;

        public SuDungDichVuControl()
        {
            InitializeComponent();
            _service = new LocalSuDungDichVuService();
            
            if (DpNgayOrder != null) DpNgayOrder.SelectedDate = DateTime.Now;
            if (TxtGioBatDau != null) TxtGioBatDau.Text = DateTime.Now.ToString("HH:mm:ss");

            // Khởi tạo timer cập nhật thời gian trôi qua của các bàn đang hoạt động
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _timer.Tick += Timer_Tick;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhuVucBansAsync();
            await LoadMenuTreeAsync();
            await LoadMatHangListAsync();

            _timer?.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_khuVucList != null)
            {
                foreach (var kv in _khuVucList)
                {
                    if (kv.BanList != null)
                    {
                        foreach (var b in kv.BanList)
                        {
                            if (b.IsOccupied)
                            {
                                b.UpdateTimerText();
                            }
                        }
                    }
                }
            }

            if (_currentBan != null && _currentBan.IsOccupied && _currentBan.StartTime.HasValue)
            {
                var elapsed = DateTime.Now - _currentBan.StartTime.Value;
                if (TxtElapsedMinutes != null)
                {
                    TxtElapsedMinutes.Text = $"{(int)elapsed.TotalMinutes} phút";
                }
            }
        }

        private async Task LoadKhuVucBansAsync()
        {
            if (_service == null) return;
            _khuVucList = await _service.GetKhuVucBanListAsync();
            if (IcKhuVuc != null) IcKhuVuc.ItemsSource = _khuVucList;

            // Nếu chưa chọn bàn nào, tự động chọn bàn đầu tiên
            if (_currentBan == null && _khuVucList != null && _khuVucList.Count > 0 && _khuVucList[0].BanList != null && _khuVucList[0].BanList.Count > 0)
            {
                SelectBan(_khuVucList[0].BanList[0]);
            }
            else if (_currentBan != null && _khuVucList != null)
            {
                // Tìm lại bàn đang chọn trong danh sách mới
                var reloadedBan = _khuVucList.SelectMany(k => k.BanList ?? Enumerable.Empty<PosBanViewModel>()).FirstOrDefault(b => b.Id == _currentBan.Id);
                if (reloadedBan != null)
                {
                    SelectBan(reloadedBan);
                }
            }
        }

        private async Task LoadMatHangListAsync()
        {
            if (_service == null || DgMatHang == null) return;
            string keyword = TxtTimMatHang?.Text?.Trim();
            var allItems = await _service.GetMatHangListAsync(_selectedNhomId, keyword);
            DgMatHang.ItemsSource = allItems;
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PosBanViewModel ban)
            {
                SelectBan(ban);
            }
        }

        private async void SelectBan(PosBanViewModel ban)
        {
            if (ban == null) return;

            if (_khuVucList != null)
            {
                foreach (var kv in _khuVucList)
                {
                    if (kv.BanList != null)
                    {
                        foreach (var b in kv.BanList)
                        {
                            b.IsSelected = (b.Id == ban.Id);
                        }
                    }
                }
            }

            _currentBan = ban;
            if (TxtSelectedBanHeader != null) TxtSelectedBanHeader.Text = $"Bàn: [{ban.Name}]";

            if (ban.IsOccupied)
            {
                if (TxtGioBatDau != null) TxtGioBatDau.Text = ban.StartTime.HasValue ? ban.StartTime.Value.ToString("HH:mm:ss") : DateTime.Now.ToString("HH:mm:ss");
                if (BtnBatDau != null)
                {
                    BtnBatDau.Content = "Đang mở";
                    BtnBatDau.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c0392b"));
                }

                if (ban.StartTime.HasValue)
                {
                    var elapsed = DateTime.Now - ban.StartTime.Value;
                    if (TxtElapsedMinutes != null) TxtElapsedMinutes.Text = $"{(int)elapsed.TotalMinutes} phút";
                }
                else
                {
                    if (TxtElapsedMinutes != null) TxtElapsedMinutes.Text = "0 phút";
                }

                // Tải chi tiết đơn hàng
                if (!string.IsNullOrEmpty(ban.ActiveOrderId))
                {
                    var items = await _service.GetOrderDetailsAsync(ban.ActiveOrderId);
                    ban.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>(items);
                }
                if (DgChiTiet != null) DgChiTiet.ItemsSource = ban.OrderItems;

                if (TxtSoPhieu != null) TxtSoPhieu.Text = ban.SoPhieu ?? "";
                if (TxtSoKhach != null) TxtSoKhach.Text = ban.SoKhach.ToString();
                if (TxtKhachHang != null) TxtKhachHang.Text = ban.KhachHangName ?? "";
                if (TxtOrderGhiChu != null) TxtOrderGhiChu.Text = ban.GhiChu ?? "";
            }
            else
            {
                if (TxtGioBatDau != null) TxtGioBatDau.Text = DateTime.Now.ToString("HH:mm:ss");
                if (BtnBatDau != null)
                {
                    BtnBatDau.Content = "Bắt đầu";
                    BtnBatDau.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27ae60"));
                }
                if (TxtElapsedMinutes != null) TxtElapsedMinutes.Text = "0 phút";

                ban.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>();
                if (DgChiTiet != null) DgChiTiet.ItemsSource = ban.OrderItems;

                if (TxtSoPhieu != null) TxtSoPhieu.Text = "";
                if (TxtSoKhach != null) TxtSoKhach.Text = "0";
                if (TxtKhachHang != null) TxtKhachHang.Text = "";
                if (TxtOrderGhiChu != null) TxtOrderGhiChu.Text = "";
            }

            RecalculateTotals();
            UpdateOrderControlState();
        }

        private void UpdateOrderControlState()
        {
            bool isStarted = _currentBan != null && _currentBan.IsOccupied;

            if (BorderOrderToolbar != null) BorderOrderToolbar.IsEnabled = isStarted;
            if (PanelActionsDoc != null) PanelActionsDoc.IsEnabled = isStarted;
            if (DgChiTiet != null) DgChiTiet.IsEnabled = isStarted;
            if (TxtGiamGiaPt != null) TxtGiamGiaPt.IsEnabled = isStarted;
            if (TxtGiamGia != null) TxtGiamGia.IsEnabled = isStarted;
            if (TxtOrderGhiChu != null) TxtOrderGhiChu.IsEnabled = isStarted;
            if (TxtKhachHang != null) TxtKhachHang.IsEnabled = isStarted;
            if (TxtSoKhach != null) TxtSoKhach.IsEnabled = isStarted;
            if (TxtSoPhieu != null) TxtSoPhieu.IsEnabled = isStarted;
        }

        private async void BtnBatDau_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBan == null)
            {
                MessageBox.Show("Vui lòng chọn một bàn để bắt đầu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentBan.IsOccupied)
            {
                var ask = MessageBox.Show($"Bàn '{_currentBan.Name}' đang được sử dụng.\nBạn có muốn cập nhật lại giờ bắt đầu theo giờ hiện tại ({DateTime.Now:HH:mm:ss}) không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    _currentBan.StartTime = DateTime.Now;
                    if (TxtGioBatDau != null) TxtGioBatDau.Text = _currentBan.StartTime.Value.ToString("HH:mm:ss");
                    _currentBan.UpdateTimerText();
                }
                return;
            }

            // Bắt đầu mở bàn mới
            DateTime startTime = DateTime.Now;
            if (TxtGioBatDau != null && DateTime.TryParse(TxtGioBatDau.Text?.Trim(), out DateTime parsedTime))
            {
                startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, parsedTime.Hour, parsedTime.Minute, parsedTime.Second);
            }

            int.TryParse(TxtSoKhach?.Text?.Trim(), out int soKhach);
            string khachHang = TxtKhachHang?.Text?.Trim();
            string ghiChu = TxtOrderGhiChu?.Text?.Trim();

            string orderId = await _service.StartTableOrderAsync(_currentBan.Id, startTime, soKhach, khachHang, ghiChu);
            if (!string.IsNullOrEmpty(orderId))
            {
                _currentBan.IsOccupied = true;
                _currentBan.StartTime = startTime;
                _currentBan.ActiveOrderId = orderId;
                _currentBan.SoKhach = soKhach;
                _currentBan.KhachHangName = khachHang;
                _currentBan.GhiChu = ghiChu;
                _currentBan.UpdateTimerText();

                if (BtnBatDau != null)
                {
                    BtnBatDau.Content = "Đang mở";
                    BtnBatDau.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c0392b"));
                }

                UpdateOrderControlState();
                MessageBox.Show($"Đã bắt đầu mở bàn '{_currentBan.Name}' thành công lúc {startTime:HH:mm:ss}!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void AddItemToCurrentOrder(PosMatHangViewModel matHang)
        {
            if (_currentBan == null)
            {
                MessageBox.Show("Vui lòng chọn một bàn trước khi thêm món!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Khi chưa bắt đầu thì không được thêm món
            if (!_currentBan.IsOccupied)
            {
                MessageBox.Show($"Bàn '{_currentBan.Name}' chưa bắt đầu.\nVui lòng bấm nút 'Bắt đầu' trước khi thêm món!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentBan.OrderItems == null)
            {
                _currentBan.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>();
                if (DgChiTiet != null) DgChiTiet.ItemsSource = _currentBan.OrderItems;
            }

            // Kiểm tra xem món đã có trong đơn chưa
            var existing = _currentBan.OrderItems.FirstOrDefault(x => x.MatHangId == matHang.Id);
            if (existing != null)
            {
                existing.SoLuong += 1;
            }
            else
            {
                var newItem = new PosDonHangChiTietViewModel
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 20),
                    MatHangId = matHang.Id,
                    MatHangName = matHang.Name,
                    DonViTinh = matHang.DonViTinh,
                    DonGia = matHang.GiaBan ?? 0,
                    SoLuong = 1,
                    ChietKhauPhanTram = 0,
                    GhiChu = "",
                    LoaiDoId = matHang.LoaiDoId,
                    LoaiDoName = matHang.LoaiDoName
                };
                newItem.Recalculate();
                _currentBan.OrderItems.Add(newItem);
            }

            RecalculateTotals();
            await AutoSaveOrderAsync();
        }

        private async void DgMatHang_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgMatHang?.SelectedItem is PosMatHangViewModel matHang)
            {
                AddItemToCurrentOrder(matHang);
            }
        }

        private void BtnThemMon_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang?.SelectedItem is PosMatHangViewModel matHang)
            {
                AddItemToCurrentOrder(matHang);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn món ăn trong thực đơn bên phải!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnTangSoLuong_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item)
            {
                item.SoLuong += 1;
                RecalculateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnGiamSoLuong_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item)
            {
                if (item.SoLuong > 1)
                {
                    item.SoLuong -= 1;
                }
                else if (_currentBan?.OrderItems != null)
                {
                    _currentBan.OrderItems.Remove(item);
                }
                RecalculateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnXoaMon_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item && _currentBan?.OrderItems != null)
            {
                _currentBan.OrderItems.Remove(item);
                RecalculateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnDatSl_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item)
            {
                var win = new InputWindow("Đặt số lượng", $"Nhập số lượng cho '{item.MatHangName}':", item.SoLuong.ToString("0"));
                if (win.ShowDialog() == true && decimal.TryParse(win.InputText?.Trim(), out decimal sl) && sl > 0)
                {
                    item.SoLuong = sl;
                    RecalculateTotals();
                    await AutoSaveOrderAsync();
                }
            }
        }

        private async void BtnDoiGia_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item)
            {
                var win = new InputWindow("Đổi đơn giá", $"Nhập đơn giá mới cho '{item.MatHangName}':", item.DonGia.ToString("0"));
                if (win.ShowDialog() == true && decimal.TryParse(win.InputText?.Trim(), out decimal gia) && gia >= 0)
                {
                    item.DonGia = gia;
                    RecalculateTotals();
                    await AutoSaveOrderAsync();
                }
            }
        }

        private async void BtnGhiChu_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item)
            {
                var win = new InputWindow("Ghi chú món", $"Nhập ghi chú cho '{item.MatHangName}':", item.GhiChu ?? "");
                if (win.ShowDialog() == true)
                {
                    item.GhiChu = win.InputText?.Trim();
                    await AutoSaveOrderAsync();
                }
            }
        }

        private async void BtnChietKhau_Click(object sender, RoutedEventArgs e)
        {
            if (DgChiTiet?.SelectedItem is PosDonHangChiTietViewModel item)
            {
                var win = new InputWindow("Chiết khấu %", $"Nhập tỷ lệ chiết khấu % cho '{item.MatHangName}':", item.ChietKhauPhanTram.ToString("0"));
                if (win.ShowDialog() == true && decimal.TryParse(win.InputText?.Trim(), out decimal ck) && ck >= 0 && ck <= 100)
                {
                    item.ChietKhauPhanTram = ck;
                    RecalculateTotals();
                    await AutoSaveOrderAsync();
                }
            }
        }

        private void RecalculateTotals()
        {
            if (TxtTienHang == null || TxtTongCong == null || TxtGiamGia == null || TxtGiamGiaPt == null)
            {
                return;
            }

            if (_currentBan == null || _currentBan.OrderItems == null)
            {
                TxtTienHang.Text = "0";
                TxtTongCong.Text = "0";
                return;
            }

            decimal tienHang = _currentBan.OrderItems.Sum(x => x.ThanhTien);
            _currentBan.TienHang = tienHang;

            decimal.TryParse(TxtGiamGiaPt.Text?.Trim(), out decimal giamPt);
            decimal giamTien = 0;
            if (giamPt > 0)
            {
                giamTien = tienHang * (giamPt / 100m);
            }
            else
            {
                decimal.TryParse(TxtGiamGia.Text?.Trim(), out giamTien);
            }

            _currentBan.GiamGia = giamTien;
            decimal tongCong = Math.Max(0, tienHang - giamTien);
            _currentBan.TongCong = tongCong;

            TxtTienHang.Text = tienHang.ToString("N0");
            TxtGiamGia.Text = giamTien.ToString("N0");
            TxtTongCong.Text = tongCong.ToString("N0");
        }

        private void TxtGiamGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private async Task AutoSaveOrderAsync()
        {
            if (_currentBan != null && !string.IsNullOrEmpty(_currentBan.ActiveOrderId) && _service != null)
            {
                int.TryParse(TxtSoKhach?.Text?.Trim(), out int soKhach);
                await _service.SaveOrderAsync(
                    _currentBan.ActiveOrderId, 
                    _currentBan.OrderItems.ToList(), 
                    _currentBan.TienHang, 
                    _currentBan.GiamGia, 
                    _currentBan.TongCong, 
                    TxtOrderGhiChu?.Text?.Trim(), 
                    soKhach
                );
            }
        }

        private async void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBan == null || !_currentBan.IsOccupied)
            {
                MessageBox.Show("Bàn hiện tại không có đơn hàng đang hoạt động!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Xác nhận thanh toán và kết thúc bàn '{_currentBan.Name}'?\nTổng cộng: {_currentBan.TongCong:N0} VNĐ", "Xác nhận thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                if (await _service.FinishTableOrderAsync(_currentBan.ActiveOrderId))
                {
                    MessageBox.Show($"Thanh toán bàn '{_currentBan.Name}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    _currentBan.IsOccupied = false;
                    _currentBan.StartTime = null;
                    _currentBan.ActiveOrderId = null;
                    _currentBan.OrderItems.Clear();
                    _currentBan.UpdateTimerText();

                    SelectBan(_currentBan);
                }
            }
        }

        private async void BtnChuyenBan_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBan == null || !_currentBan.IsOccupied || string.IsNullOrEmpty(_currentBan.ActiveOrderId))
            {
                MessageBox.Show("Bàn hiện tại chưa mở hoặc không có đơn hàng để chuyển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allBans = _khuVucList?.SelectMany(k => k.BanList).ToList() ?? new List<PosBanViewModel>();
            var win = new ChonBanChuyenGopWindow(_currentBan, allBans, isMergeMode: false);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && win.SelectedTargetBan != null)
            {
                var targetBan = win.SelectedTargetBan;
                string sourceOrderId = _currentBan.ActiveOrderId;
                string oldName = _currentBan.Name;

                bool success = await _service.TransferTableAsync(sourceOrderId, targetBan.Id);
                if (success)
                {
                    // Chuyển toàn bộ dữ liệu bàn sang targetBan
                    targetBan.IsOccupied = true;
                    targetBan.StartTime = _currentBan.StartTime;
                    targetBan.ActiveOrderId = sourceOrderId;
                    targetBan.SoKhach = _currentBan.SoKhach;
                    targetBan.KhachHangName = _currentBan.KhachHangName;
                    targetBan.GhiChu = _currentBan.GhiChu;
                    targetBan.OrderItems = _currentBan.OrderItems;
                    targetBan.TienHang = _currentBan.TienHang;
                    targetBan.GiamGia = _currentBan.GiamGia;
                    targetBan.TongCong = _currentBan.TongCong;
                    targetBan.UpdateTimerText();

                    // Đặt lại bàn nguồn thành trống
                    _currentBan.IsOccupied = false;
                    _currentBan.StartTime = null;
                    _currentBan.ActiveOrderId = null;
                    _currentBan.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>();
                    _currentBan.TienHang = 0;
                    _currentBan.GiamGia = 0;
                    _currentBan.TongCong = 0;
                    _currentBan.UpdateTimerText();

                    // Chọn bàn mới
                    SelectBan(targetBan);

                    MessageBox.Show($"Đã chuyển toàn bộ dữ liệu bàn '{oldName}' sang bàn '{targetBan.Name}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void BtnGopBan_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBan == null || !_currentBan.IsOccupied || string.IsNullOrEmpty(_currentBan.ActiveOrderId))
            {
                MessageBox.Show("Bàn hiện tại chưa mở hoặc không có đơn hàng để gộp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allBans = _khuVucList?.SelectMany(k => k.BanList).ToList() ?? new List<PosBanViewModel>();
            var win = new ChonBanChuyenGopWindow(_currentBan, allBans, isMergeMode: true);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && win.SelectedTargetBan != null)
            {
                var targetBan = win.SelectedTargetBan;
                string sourceOrderId = _currentBan.ActiveOrderId;
                string oldName = _currentBan.Name;

                // Nếu bàn đích chưa mở, mở bàn đích trước
                if (!targetBan.IsOccupied || string.IsNullOrEmpty(targetBan.ActiveOrderId))
                {
                    DateTime startTime = _currentBan.StartTime ?? DateTime.Now;
                    string newOrderId = await _service.StartTableOrderAsync(targetBan.Id, startTime, _currentBan.SoKhach, _currentBan.KhachHangName, _currentBan.GhiChu);
                    if (string.IsNullOrEmpty(newOrderId)) return;

                    targetBan.IsOccupied = true;
                    targetBan.StartTime = startTime;
                    targetBan.ActiveOrderId = newOrderId;
                    targetBan.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>();
                }
                else if (targetBan.OrderItems == null)
                {
                    var details = await _service.GetOrderDetailsAsync(targetBan.ActiveOrderId);
                    targetBan.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>(details);
                }

                // Dồn món từ bàn nguồn sang bàn đích
                if (_currentBan.OrderItems != null)
                {
                    foreach (var sItem in _currentBan.OrderItems)
                    {
                        var exist = targetBan.OrderItems.FirstOrDefault(x => x.MatHangId == sItem.MatHangId);
                        if (exist != null)
                        {
                            exist.SoLuong += sItem.SoLuong;
                            exist.Recalculate();
                        }
                        else
                        {
                            var copyItem = new PosDonHangChiTietViewModel
                            {
                                Id = Guid.NewGuid().ToString("N").Substring(0, 20),
                                MatHangId = sItem.MatHangId,
                                MatHangName = sItem.MatHangName,
                                DonViTinh = sItem.DonViTinh,
                                DonGia = sItem.DonGia,
                                SoLuong = sItem.SoLuong,
                                ChietKhauPhanTram = sItem.ChietKhauPhanTram,
                                GhiChu = sItem.GhiChu,
                                LoaiDoId = sItem.LoaiDoId,
                                LoaiDoName = sItem.LoaiDoName,
                                DaInCheBien = sItem.DaInCheBien
                            };
                            copyItem.Recalculate();
                            targetBan.OrderItems.Add(copyItem);
                        }
                    }
                }

                // Tính lại tiền bàn đích
                decimal targetTienHang = targetBan.OrderItems.Sum(x => x.ThanhTien);
                targetBan.TienHang = targetTienHang;
                targetBan.TongCong = targetTienHang;

                // Lưu đơn hàng bàn đích
                await _service.SaveOrderAsync(targetBan.ActiveOrderId, targetBan.OrderItems.ToList(), targetBan.TienHang, targetBan.GiamGia, targetBan.TongCong, targetBan.GhiChu, targetBan.SoKhach);

                // Xóa đơn bàn nguồn
                await _service.DeleteOrderAsync(sourceOrderId);

                // Đặt lại bàn nguồn thành trống
                _currentBan.IsOccupied = false;
                _currentBan.StartTime = null;
                _currentBan.ActiveOrderId = null;
                _currentBan.OrderItems = new ObservableCollection<PosDonHangChiTietViewModel>();
                _currentBan.TienHang = 0;
                _currentBan.GiamGia = 0;
                _currentBan.TongCong = 0;
                _currentBan.UpdateTimerText();

                // Chuyển chọn sang bàn đích
                SelectBan(targetBan);

                MessageBox.Show($"Đã gộp toàn bộ món từ bàn '{oldName}' vào bàn '{targetBan.Name}' thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnGiamGiaTheoNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBan == null || _currentBan.OrderItems == null || _currentBan.OrderItems.Count == 0)
            {
                MessageBox.Show("Bàn hiện tại chưa có món ăn nào để giảm giá theo nhóm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new GiamGiaTheoNhomWindow();
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                decimal doAnPt = win.DoAnPercent;
                decimal doUongPt = win.DoUongPercent;
                decimal dichVuPt = win.DichVuPercent;
                decimal doKhacPt = win.DoKhacPercent;

                foreach (var item in _currentBan.OrderItems)
                {
                    string cat = item.ItemCategory;
                    if (cat == "DoAn")
                    {
                        item.ChietKhauPhanTram = doAnPt;
                    }
                    else if (cat == "DoUong")
                    {
                        item.ChietKhauPhanTram = doUongPt;
                    }
                    else if (cat == "DichVu")
                    {
                        item.ChietKhauPhanTram = dichVuPt;
                    }
                    else if (cat == "DoKhac")
                    {
                        item.ChietKhauPhanTram = doKhacPt;
                    }
                    item.Recalculate();
                }

                RecalculateTotals();
                await AutoSaveOrderAsync();
            }
        }

        private async void BtnInCheBien_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBan == null || _currentBan.OrderItems == null || _currentBan.OrderItems.Count == 0)
            {
                MessageBox.Show("Không có mặt hàng nào để in", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Lọc danh sách các món chưa in chế biến
            var unprintedItems = _currentBan.OrderItems.Where(x => !x.DaInCheBien).ToList();
            if (unprintedItems.Count == 0)
            {
                MessageBox.Show("Không có mặt hàng nào để in", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new InCheBienWindow(_currentBan.Name, unprintedItems);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await AutoSaveOrderAsync();
            }
        }

        private async void TvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_service == null || DgMatHang == null) return;
            if (e.NewValue is PosNhomMatHangViewModel nhom)
            {
                _selectedNhomId = nhom.Id;
                await LoadMatHangListAsync();
            }
        }

        private async void TxtTimMatHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_service == null || DgMatHang == null) return;
            await LoadMatHangListAsync();
        }

        // ======================= XỬ LÝ KHÁCH HÀNG (F12) =======================
        private List<KhachHangLookupViewModel> _allKhachHangs = new List<KhachHangLookupViewModel>();
        private ObservableCollection<KhachHangLookupViewModel> _filteredKhachHangs = new ObservableCollection<KhachHangLookupViewModel>();

        private async void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                e.Handled = true;
                if (TxtKhachHang != null)
                {
                    TxtKhachHang.Focus();
                    TxtKhachHang.SelectAll();
                    if (PopupKhachHang != null) PopupKhachHang.IsOpen = true;
                    if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = true;
                    await LoadKhachHangAsync();
                    await FilterKhachHangAsync(TxtKhachHang.Text);
                }
            }
            else if (e.Key == Key.F11)
            {
                e.Handled = true;
                BtnThanhToan_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.F10)
            {
                e.Handled = true;
                BtnInCheBien_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.F3)
            {
                e.Handled = true;
                TxtTimMatHang?.Focus();
                TxtTimMatHang?.SelectAll();
            }
        }

        private async void TxtKhachHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PopupKhachHang != null && !PopupKhachHang.IsOpen && TxtKhachHang.IsFocused)
            {
                PopupKhachHang.IsOpen = true;
                if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = true;
            }
            await FilterKhachHangAsync(TxtKhachHang?.Text ?? "");
        }

        private void TxtKhachHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && PopupKhachHang != null && PopupKhachHang.IsOpen)
            {
                DgKhachHang?.Focus();
                if (DgKhachHang?.Items.Count > 0)
                {
                    DgKhachHang.SelectedIndex = 0;
                }
            }
        }

        private async void BtnKhachHangToggle_Click(object sender, RoutedEventArgs e)
        {
            if (BtnKhachHangToggle?.IsChecked == true)
            {
                await LoadKhachHangAsync();
                await FilterKhachHangAsync(TxtKhachHang?.Text ?? "");
            }
        }

        private async Task LoadKhachHangAsync()
        {
            if (_allKhachHangs.Count == 0 && _service != null)
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
                string lower = filter.Trim().ToLower();
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Contains(lower))
                                      || (x.Makhach != null && x.Makhach.ToLower().Contains(lower))
                                      || (x.Dienthoai != null && x.Dienthoai.ToLower().Contains(lower))
                                      || (x.Diachi != null && x.Diachi.ToLower().Contains(lower)));
            }

            foreach (var item in query)
            {
                _filteredKhachHangs.Add(item);
            }

            if (DgKhachHang != null)
            {
                DgKhachHang.ItemsSource = _filteredKhachHangs;
                if (_filteredKhachHangs.Count > 0)
                {
                    DgKhachHang.SelectedIndex = 0;
                }
            }
        }

        private async void DgKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await SelectKhachHangItemAsync();
        }

        private async void DgKhachHang_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SelectKhachHangItemAsync();
            }
        }

        private async Task SelectKhachHangItemAsync()
        {
            if (DgKhachHang?.SelectedItem is KhachHangLookupViewModel selected)
            {
                if (TxtKhachHang != null)
                {
                    TxtKhachHang.Text = selected.Name;
                    TxtKhachHang.Tag = selected.Id;
                }

                if (_currentBan != null)
                {
                    _currentBan.KhachHangName = selected.Name;
                    if (!string.IsNullOrEmpty(_currentBan.ActiveOrderId))
                    {
                        await _service.UpdateOrderCustomerAsync(_currentBan.ActiveOrderId, selected.Id);
                    }
                }

                if (PopupKhachHang != null) PopupKhachHang.IsOpen = false;
                if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = false;
            }
        }

        private async void BtnThemKhachHang_Click(object sender, RoutedEventArgs e)
        {
            var win = new InputWindow("Thêm khách hàng", "Nhập tên khách hàng mới:", "");
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.InputText))
            {
                string newName = win.InputText.Trim();
                string maKhach = (_allKhachHangs.Count + 1).ToString("D3");
                bool ok = await _service.InsertKhachHangAsync(newName, maKhach, "", "");
                if (ok)
                {
                    _allKhachHangs.Clear();
                    await LoadKhachHangAsync();
                    await FilterKhachHangAsync(newName);

                    if (TxtKhachHang != null) TxtKhachHang.Text = newName;
                    if (_currentBan != null) _currentBan.KhachHangName = newName;
                    if (PopupKhachHang != null) PopupKhachHang.IsOpen = false;
                    if (BtnKhachHangToggle != null) BtnKhachHangToggle.IsChecked = false;
                }
            }
        }

        private void BtnSuaKhachHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Vui lòng vào menu 'Khách hàng' để cập nhật chi tiết thông tin khách hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnKhachHangReload_Click(object sender, RoutedEventArgs e)
        {
            _allKhachHangs.Clear();
            await LoadKhachHangAsync();
            await FilterKhachHangAsync(TxtKhachHang?.Text ?? "");
        }

        private void BtnKhachHangDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở danh mục quản lý khách hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ======================= XỬ LÝ CÂY THỰC ĐƠN & CHUỘT PHẢI (TvMenu) =======================
        private ObservableCollection<PosNhomMatHangViewModel> _menuTreeList;

        private async Task LoadMenuTreeAsync()
        {
            if (_service == null || TvMenu == null) return;
            _menuTreeList = await _service.GetNhomMatHangTreeAsync();
            TvMenu.ItemsSource = _menuTreeList;
            if (_menuTreeList != null && _menuTreeList.Count > 0)
            {
                _menuTreeList[0].IsExpanded = true;
            }
        }

        private async void MenuTree_ThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomWindow(false);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadMenuTreeAsync();
            }
        }

        private async void MenuTree_ThemCon_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomWindow(false);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadMenuTreeAsync();
            }
        }

        private async void MenuTree_ChinhSua_Click(object sender, RoutedEventArgs e)
        {
            var selected = TvMenu?.SelectedItem as PosNhomMatHangViewModel;
            if (selected == null || string.IsNullOrEmpty(selected.Id))
            {
                MessageBox.Show("Vui lòng chọn một nhóm để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var win = new ThemNhomWindow(false, selected.Name);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                await LoadMenuTreeAsync();
            }
        }

        private void MenuTree_SortAZ_Click(object sender, RoutedEventArgs e)
        {
            if (_menuTreeList != null && _menuTreeList.Count > 0 && _menuTreeList[0].Children != null)
            {
                var sorted = _menuTreeList[0].Children.OrderBy(x => x.Name).ToList();
                _menuTreeList[0].Children.Clear();
                foreach (var item in sorted)
                {
                    _menuTreeList[0].Children.Add(item);
                }
            }
        }

        private async void MenuTree_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadMenuTreeAsync();
            await LoadMatHangListAsync();
        }

        private void MenuTree_SaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (TvMenu?.SelectedItem is PosNhomMatHangViewModel selected)
            {
                Clipboard.SetText(selected.Name ?? "");
                MessageBox.Show($"Đã sao chép tên nhóm '{selected.Name}' vào Clipboard!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuTree_MoRong_Click(object sender, RoutedEventArgs e)
        {
            SetTreeExpandState(_menuTreeList, true);
        }

        private void MenuTree_ThuGon_Click(object sender, RoutedEventArgs e)
        {
            SetTreeExpandState(_menuTreeList, false);
            if (_menuTreeList != null && _menuTreeList.Count > 0)
            {
                _menuTreeList[0].IsExpanded = true;
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
            if (TvMenu?.SelectedItem is PosNhomMatHangViewModel selected && !string.IsNullOrEmpty(selected.Id))
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
                        await LoadMenuTreeAsync();
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
            if (TvMenu?.SelectedItem is PosNhomMatHangViewModel selected && !string.IsNullOrEmpty(selected.Id))
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
            if (TvMenu?.SelectedItem is PosNhomMatHangViewModel selected)
            {
                MessageBox.Show($"Tên nhóm: {selected.Name}\nMã nhóm: {selected.Id}", "Thuộc tính nhóm", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ======================= XỬ LÝ CHUỘT PHẢI DANH SÁCH MẶT HÀNG (DgMatHang) =======================
        private void MenuMatHang_ThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemMoiMatHangWindow(_selectedNhomId, null, null, -1, async () => { await LoadMatHangListAsync(); });
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuMatHang_ChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang?.SelectedItem is PosMatHangViewModel selected)
            {
                var win = new ThemMoiMatHangWindow(_selectedNhomId, selected.Id, null, -1, async () => { await LoadMatHangListAsync(); });
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void MenuMatHang_ThungRac_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang?.SelectedItem is PosMatHangViewModel selected)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn đưa mặt hàng '{selected.Name}' vào Thùng rác không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            await conn.OpenAsync();
                            await conn.ExecuteAsync("UPDATE DMATHANG SET STATUS = 0 WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = selected.Id });
                        }
                        await LoadMatHangListAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi chuyển vào thùng rác: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void MenuMatHang_XoaVinhVien_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang?.SelectedItem is PosMatHangViewModel selected)
            {
                var ask = MessageBox.Show($"Bạn có chắc chắn muốn XÓA VĨNH VIỄN mặt hàng '{selected.Name}' khỏi hệ thống không?\nThao tác này không thể hoàn tác!", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (ask == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DbConnectionManager.GetConnection())
                        {
                            await conn.OpenAsync();
                            await conn.ExecuteAsync("DELETE FROM DMATHANG WHERE CAST(ID AS VARCHAR(50)) = @Id", new { Id = selected.Id });
                        }
                        await LoadMatHangListAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể xóa mặt hàng do đã phát sinh giao dịch trong hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void MenuMatHang_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadMatHangListAsync();
        }

        private void MenuMatHang_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang?.CurrentCell.Item is PosMatHangViewModel row)
            {
                var col = DgMatHang.CurrentCell.Column as DataGridTextColumn;
                string cellValue = "";
                if (col != null && col.Header != null)
                {
                    string header = col.Header.ToString();
                    if (header.Contains("Tên")) cellValue = row.Name;
                    else if (header.Contains("ĐVT")) cellValue = row.DonViTinh;
                    else if (header.Contains("Giá")) cellValue = row.GiaBan?.ToString("N0") ?? "0";
                    else if (header.Contains("Mã")) cellValue = row.Code;
                    else cellValue = row.Name;
                }
                else
                {
                    cellValue = row.Name;
                }
                Clipboard.SetText(cellValue ?? "");
                MessageBox.Show($"Đã sao chép ô: {cellValue}", "Sao chép ô", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuMatHang_SaoChepDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang?.SelectedItem is PosMatHangViewModel row)
            {
                string rowText = $"{row.Name}\t{row.DonViTinh}\t{row.GiaBan:N0}\t{row.Code}";
                Clipboard.SetText(rowText);
                MessageBox.Show($"Đã sao chép dòng '{row.Name}' vào Clipboard!", "Sao chép dòng", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuMatHang_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHang != null)
            {
                foreach (var col in DgMatHang.Columns)
                {
                    col.Width = DataGridLength.Auto;
                    col.Width = DataGridLength.SizeToHeader;
                }
            }
        }

        private void MenuMatHang_CotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonCotHienThiWindow(DgMatHang, new List<string> { "Tên hàng", "ĐVT", "Giá bán", "Mã hàng" });
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MenuMatHang_XuatExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"DanhSachMatHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var items = DgMatHang?.ItemsSource as IEnumerable<PosMatHangViewModel>;
                    if (items != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("Mã hàng,Tên hàng,Đơn vị tính,Giá bán");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.Code}\",\"{item.Name}\",\"{item.DonViTinh}\",{item.GiaBan ?? 0}");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất file Excel CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuMatHang_InDanhSach_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgMatHang, "Danh sách mặt hàng");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }
    }
}

