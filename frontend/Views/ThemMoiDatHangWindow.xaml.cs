using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        
        public ThemMoiDatHangWindow()
        {
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
            // Init default values
            DpNgay.SelectedDate = DateTime.Now;
            TxtSoPhieu.Text = $"DH{DateTime.Now:yy}/{new Random().Next(1000, 9999)}";
            TxtGioVao.Text = DateTime.Now.ToString("HH:mm");
            TxtGioRa.Text = DateTime.Now.AddHours(2).ToString("HH:mm");
            TxtDatLuc.Text = DateTime.Now.ToString("HH:mm");

            await LoadLookupsAsync();
            await LoadMatHangsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            // Dummy for lookups (since we haven't implemented GetLookupsAsync completely yet)
            // CmbKhachHang.ItemsSource = await _service.GetLookupsAsync("DKHACHHANG");
            // CmbDatQua.ItemsSource = await _service.GetLookupsAsync("DPHUONGTHUCDAT");
            // CmbMucDich.ItemsSource = await _service.GetLookupsAsync("DMUCDICHDAT");
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
            var existing = _chiTiets.FirstOrDefault(x => x.MaHang == mh.Code);
            if (existing != null)
            {
                existing.SoLuong = (existing.SoLuong ?? 0) + soLuong;
                existing.ThanhTien = existing.SoLuong * existing.DonGia;
                
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

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _chiTiets.Clear();
            UpdateTotals();
            TxtSoPhieu.Text = $"DH{DateTime.Now:yy}/{new Random().Next(1000, 9999)}";
            TxtPhong.Text = "";
            TxtNguoiDat.Text = "";
            TxtDienThoai.Text = "";
            TxtDiaChi.Text = "";
            TxtYeuCauKhac.Text = "";
            TxtKhachHang.Text = "";
            TxtKhachHang.Tag = null;
        }

        private void Menu_TaoMoi_Click(object sender, RoutedEventArgs e) => BtnTaoMoi_Click(sender, e);
        
        private void Menu_Luu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Lưu đang được phát triển!", "Thông báo");
        }

        private void Menu_LuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            Menu_Luu_Click(sender, e);
            // this.DialogResult = true;
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
            else if (e.Key == Key.F10)
            {
                // Trước
            }
            else if (e.Key == Key.F11)
            {
                // Sau
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
                // Lưu và mới
            }
        }

        private void BtnChonPhong_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChonPhongWindow();
            window.Owner = this;
            if (window.ShowDialog() == true && window.SelectedBans != null && window.SelectedBans.Count > 0)
            {
                TxtPhong.Text = string.Join(", ", window.SelectedBans.Select(x => x.Name));
                TxtPhong.Tag = window.SelectedBans; // Lưu ID vào Tag để dành cho lúc lưu
            }
        }

        // --- Logic Khách Hàng ---
        private ObservableCollection<KhachHangViewModel> _khachHangs = new ObservableCollection<KhachHangViewModel>();

        private void TxtKhachHang_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Có thể thêm logic tìm kiếm nhanh ở đây, tự mở popup nếu cần
            if (PopupKhachHang != null && !PopupKhachHang.IsOpen && TxtKhachHang.IsFocused)
            {
                PopupKhachHang.IsOpen = true;
                _ = LoadKhachHangAsync(TxtKhachHang.Text);
            }
        }

        private async void BtnKhachHangToggle_Click(object sender, RoutedEventArgs e)
        {
            if (BtnKhachHangToggle.IsChecked == true)
            {
                await LoadKhachHangAsync("");
            }
        }

        private async Task LoadKhachHangAsync(string filter)
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT ID as Id, MAKHACH as Ma, NAME as Name, DIACHI as DiaChi, DIENTHOAI as DienThoai
                        FROM DKHACHHANG
                        WHERE STATUS = 1";
                    
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        sql += " AND (LOWER(NAME) LIKE @Filter OR LOWER(MAKHACH) LIKE @Filter OR DIENTHOAI LIKE @Filter)";
                    }
                    sql += " ORDER BY NAME";

                    var result = await Dapper.SqlMapper.QueryAsync<KhachHangViewModel>(conn, sql, new { Filter = $"%{filter.ToLower()}%" });
                    _khachHangs.Clear();
                    foreach (var item in result)
                    {
                        _khachHangs.Add(item);
                    }
                    DgKhachHang.ItemsSource = _khachHangs;
                }
            }
            catch (System.Exception ex)
            {
                // Bỏ qua lỗi kết nối tạm thời
            }
        }

        private void DgKhachHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = DgKhachHang.SelectedItem as KhachHangViewModel;
            if (selected != null)
            {
                TxtKhachHang.Text = selected.Name;
                TxtKhachHang.Tag = selected.Id;
                PopupKhachHang.IsOpen = false;
                BtnKhachHangToggle.IsChecked = false;
            }
        }

        private void BtnThemKhachHang_Click(object sender, RoutedEventArgs e)
        {
            // Mở form thêm khách hàng mới (chưa có)
            MessageBox.Show("Chức năng thêm khách hàng đang được xây dựng!");
        }
    }

    public class KhachHangViewModel
    {
        public string Id { get; set; }
        public string Ma { get; set; }
        public string Name { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
    }
}
