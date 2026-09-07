using System;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using System.Linq;

namespace QuanLyBar.Client.Views
{
    public partial class ThemMoiMatHangWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private string _selectedNhomId;
        private string _matHangIdToEdit;
        private System.Collections.Generic.List<MatHangViewModel> _matHangList;
        private int _currentIndex = -1;
        private Action _onDataSaved;
        private bool _suDung2Dvt = true;
        private bool _tuDongCapNhatGiaNhapDinhLuong = true;
        private bool _tuDongCapNhatGiaVonDinhLuong = true;
        private bool _suDungGia2 = false;
        private string _dienGiaiGia2 = "Giá 2";
        private bool _suDungGia3 = false;
        private string _dienGiaiGia3 = "Giá 3";
        private bool _suDungGia4 = false;
        private string _dienGiaiGia4 = "Giá 4";
        private bool _suDungTenTiengAnh = false;

        public System.Collections.ObjectModel.ObservableCollection<DinhLuongChiTietViewModel> DinhLuongList { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<MatHangViewModel> AllMaterials { get; set; }

        public ThemMoiMatHangWindow(string selectedNhomId, string matHangIdToEdit = null, System.Collections.Generic.List<MatHangViewModel> matHangList = null, int initialIndex = -1, Action onDataSaved = null)
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _selectedNhomId = selectedNhomId;
            _matHangIdToEdit = matHangIdToEdit;
            _matHangList = matHangList;
            _onDataSaved = onDataSaved;
            
            if (_matHangList != null)
            {
                if (!string.IsNullOrEmpty(_matHangIdToEdit))
                {
                    _currentIndex = _matHangList.FindIndex(m => m.Id == _matHangIdToEdit);
                }
                else
                {
                    _currentIndex = initialIndex;
                }
            }

            if (!string.IsNullOrEmpty(_matHangIdToEdit))
            {
                this.Title = "MẶT HÀNG - SỬA";
            }
            else
            {
                this.Title = "MẶT HÀNG - THÊM MỚI";
            }

            DinhLuongList = new System.Collections.ObjectModel.ObservableCollection<DinhLuongChiTietViewModel>();
            DinhLuongList.CollectionChanged += DinhLuongList_CollectionChanged;
            AllMaterials = new System.Collections.ObjectModel.ObservableCollection<MatHangViewModel>();
            DgDinhLuong.ItemsSource = DinhLuongList;
            this.DataContext = this;

            TxtQuyDoi.TextChanged += (s, ev) => UpdateDonViQuyDoiHienThi();
        }

        private void DinhLuongList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DinhLuongChiTietViewModel item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (DinhLuongChiTietViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            CalculateTotals();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DinhLuongChiTietViewModel.ThanhTienNhap) || 
                e.PropertyName == nameof(DinhLuongChiTietViewModel.ThanhTienVon) ||
                e.PropertyName == nameof(DinhLuongChiTietViewModel.SoLuong))
            {
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            if (TxtTongSoLuong == null) return;
            var tongSl = DinhLuongList.Sum(x => x.SoLuong);
            var tongNhap = DinhLuongList.Sum(x => x.ThanhTienNhap);
            var tongVon = DinhLuongList.Sum(x => x.ThanhTienVon);

            TxtTongSoLuong.Text = tongSl.ToString("N0");
            TxtTongTienNhap.Text = tongNhap.ToString("N0");
            TxtTongTienVon.Text = tongVon.ToString("N0");

            if (DinhLuongList.Count > 0)
            {
                if (_tuDongCapNhatGiaNhapDinhLuong && TxtGiaNhap != null)
                {
                    TxtGiaNhap.Text = tongNhap.ToString("N0");
                }
                if (_tuDongCapNhatGiaVonDinhLuong && TxtGiaVon != null)
                {
                    TxtGiaVon.Text = tongVon.ToString("N0");
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F4)
            {
                BtnThemDong_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F8)
            {
                BtnXoaDong_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F10)
            {
                BtnTruoc_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F11)
            {
                BtnSau_Click(null, null);
                e.Handled = true;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load configs
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                if (configs != null)
                {
                    if (configs.TryGetValue("SuDung2DonViTinh", out var val2Dvt))
                    {
                        _suDung2Dvt = val2Dvt == "1" || val2Dvt.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        _suDung2Dvt = true;
                    }

                    if (configs.TryGetValue("TuDongCapNhatGiaNhapCuaMatHangDinhLuong", out var valGiaNhap))
                    {
                        _tuDongCapNhatGiaNhapDinhLuong = valGiaNhap == "1" || valGiaNhap.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }

                    if (configs.TryGetValue("TuDongCapNhatGiaVonCuaMatHangDinhLuong", out var valGiaVon))
                    {
                        _tuDongCapNhatGiaVonDinhLuong = valGiaVon == "1" || valGiaVon.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }

                    if (configs.TryGetValue("SuDungGia2", out var valG2))
                    {
                        _suDungGia2 = valG2 == "1" || valG2.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                    if (configs.TryGetValue("DienGiaiGia2", out var valDg2) && !string.IsNullOrWhiteSpace(valDg2))
                    {
                        _dienGiaiGia2 = valDg2.Trim();
                    }

                    if (configs.TryGetValue("SuDungGia3", out var valG3))
                    {
                        _suDungGia3 = valG3 == "1" || valG3.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                    if (configs.TryGetValue("DienGiaiGia3", out var valDg3) && !string.IsNullOrWhiteSpace(valDg3))
                    {
                        _dienGiaiGia3 = valDg3.Trim();
                    }

                    if (configs.TryGetValue("SuDungGia4", out var valG4))
                    {
                        _suDungGia4 = valG4 == "1" || valG4.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                    if (configs.TryGetValue("DienGiaiGia4", out var valDg4) && !string.IsNullOrWhiteSpace(valDg4))
                    {
                        _dienGiaiGia4 = valDg4.Trim();
                    }

                    if (configs.TryGetValue("SuDungTenTiengAnh", out var valTta))
                    {
                        _suDungTenTiengAnh = valTta == "1" || valTta.Equals("True", StringComparison.OrdinalIgnoreCase);
                    }
                }
                Apply2DvtConfig();
                ApplyGiaBanConfig();

                // Load combo box data
                var nhomList = await _matHangService.GetNhomMatHangTreeAsync();
                var flatNhomList = nhomList.SelectMany(x => x.Children.Count > 0 ? x.Children : new System.Collections.ObjectModel.ObservableCollection<NhomMatHangViewModel> { x }).ToList();
                CboNhomMatHang.ItemsSource = flatNhomList;

                var dvtList = await _matHangService.GetDonViTinhListAsync();
                CboDvtBan.ItemsSource = dvtList;
                CboDvtNhap.ItemsSource = dvtList;

                var allMats = await _matHangService.GetMatHangListAsync(null);
                AllMaterials.Clear();
                foreach (var mat in allMats)
                {
                    AllMaterials.Add(mat);
                }

                // Tự động tải danh sách mặt hàng để duyệt Trước/Sau nếu danh sách chưa được truyền vào
                if (_matHangList == null || _matHangList.Count == 0)
                {
                    var list = await _matHangService.GetMatHangListAsync(!string.IsNullOrEmpty(_selectedNhomId) ? _selectedNhomId : null);
                    if (list == null || list.Count == 0)
                    {
                        list = allMats;
                    }
                    _matHangList = list;
                    if (!string.IsNullOrEmpty(_matHangIdToEdit) && _matHangList != null)
                    {
                        _currentIndex = _matHangList.FindIndex(m => m.Id == _matHangIdToEdit);
                    }
                }

                if (!string.IsNullOrEmpty(_matHangIdToEdit))
                {
                    await LoadDataById(_matHangIdToEdit);
                }
                else
                {
                    // Chế độ Thêm mới
                    if (!string.IsNullOrEmpty(_selectedNhomId))
                    {
                        CboNhomMatHang.SelectedValue = _selectedNhomId;
                    }
                    TxtMaHang.Text = ""; 
                }
                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void ApplyGiaBanConfig()
        {
            // Tên tiếng anh
            if (LblTenTiengAnh != null) LblTenTiengAnh.IsEnabled = _suDungTenTiengAnh;
            if (TxtTenTiengAnh != null)
            {
                TxtTenTiengAnh.IsEnabled = _suDungTenTiengAnh;
                if (!_suDungTenTiengAnh && string.IsNullOrEmpty(_matHangIdToEdit)) TxtTenTiengAnh.Text = "";
            }

            // Giá 2
            if (LblGia2 != null)
            {
                LblGia2.Text = _dienGiaiGia2;
                LblGia2.IsEnabled = _suDungGia2;
            }
            if (TxtGia2 != null)
            {
                TxtGia2.IsEnabled = _suDungGia2;
                if (!_suDungGia2 && string.IsNullOrEmpty(_matHangIdToEdit)) TxtGia2.Text = "0";
            }

            // Giá 3
            if (LblGia3 != null)
            {
                LblGia3.Text = _dienGiaiGia3;
                LblGia3.IsEnabled = _suDungGia3;
            }
            if (TxtGia3 != null)
            {
                TxtGia3.IsEnabled = _suDungGia3;
                if (!_suDungGia3 && string.IsNullOrEmpty(_matHangIdToEdit)) TxtGia3.Text = "0";
            }

            // Giá 4
            if (LblGia4 != null)
            {
                LblGia4.Text = _dienGiaiGia4;
                LblGia4.IsEnabled = _suDungGia4;
            }
            if (TxtGia4 != null)
            {
                TxtGia4.IsEnabled = _suDungGia4;
                if (!_suDungGia4 && string.IsNullOrEmpty(_matHangIdToEdit)) TxtGia4.Text = "0";
            }
        }

        private void Apply2DvtConfig()
        {
            if (!_suDung2Dvt)
            {
                if (LblDvtBan != null) LblDvtBan.Text = "Đơn vị tính";
                if (LblDvtNhap != null) LblDvtNhap.IsEnabled = false;
                if (GridDvtNhap != null) GridDvtNhap.IsEnabled = false;
                if (CboDvtNhap != null)
                {
                    CboDvtNhap.IsEnabled = false;
                    CboDvtNhap.SelectedValue = null;
                }
                if (LblQuyDoi != null) LblQuyDoi.IsEnabled = false;
                if (TxtQuyDoi != null)
                {
                    TxtQuyDoi.IsEnabled = false;
                    TxtQuyDoi.Text = "1";
                }
                if (TxtDonViQuyDoiHienThi != null)
                {
                    TxtDonViQuyDoiHienThi.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (LblDvtBan != null) LblDvtBan.Text = "ĐVT bán, định lượng";
                if (LblDvtNhap != null) LblDvtNhap.IsEnabled = true;
                if (GridDvtNhap != null) GridDvtNhap.IsEnabled = true;
                if (CboDvtNhap != null) CboDvtNhap.IsEnabled = true;
                if (LblQuyDoi != null) LblQuyDoi.IsEnabled = true;
                if (TxtQuyDoi != null) TxtQuyDoi.IsEnabled = true;
                if (TxtDonViQuyDoiHienThi != null)
                {
                    TxtDonViQuyDoiHienThi.Visibility = Visibility.Visible;
                    UpdateDonViQuyDoiHienThi();
                }
            }
        }

        private void UpdateDonViQuyDoiHienThi()
        {
            if (TxtDonViQuyDoiHienThi == null) return;
            if (!_suDung2Dvt)
            {
                TxtDonViQuyDoiHienThi.Visibility = Visibility.Collapsed;
                return;
            }
            string qd = string.IsNullOrWhiteSpace(TxtQuyDoi?.Text) ? "1" : TxtQuyDoi.Text.Trim();
            TxtDonViQuyDoiHienThi.Text = $" (/{qd})";
        }

        private async System.Threading.Tasks.Task LoadDataById(string id)
        {
            var matHang = await _matHangService.GetMatHangByIdAsync(id);
            if (matHang != null)
            {
                TxtMaHang.Text = matHang.Code;
                TxtTenHang.Text = matHang.Name;
                if (TxtTenTiengAnh != null) TxtTenTiengAnh.Text = matHang.Tentienganh ?? "";
                TxtGiaBan.Text = matHang.Giaban?.ToString("N0") ?? "0";
                if (TxtGia2 != null) TxtGia2.Text = matHang.Giaban2?.ToString("N0") ?? "0";
                if (TxtGia3 != null) TxtGia3.Text = matHang.Giaban3?.ToString("N0") ?? "0";
                if (TxtGia4 != null) TxtGia4.Text = matHang.Giaban4?.ToString("N0") ?? "0";
                TxtGiaNhap.Text = matHang.Gianhap?.ToString("N0") ?? "0";
                if (TxtGiaVon != null) TxtGiaVon.Text = matHang.Giavon?.ToString("N0") ?? "0";
                TxtQuyDoi.Text = _suDung2Dvt ? (string.IsNullOrEmpty(matHang.Quydoi) ? "1" : matHang.Quydoi) : "1";
                ChkGiaTheoThoiGia.IsChecked = matHang.Giatheothoigia == 1;
                CboNhomMatHang.SelectedValue = matHang.DnhommathangId;
                CboDvtBan.SelectedValue = matHang.DdonvitinhId;
                CboDvtNhap.SelectedValue = _suDung2Dvt ? matHang.DdonvitinhchanId : null;
                ChkTamKhoa.IsChecked = matHang.Tamkhoa == "1" || matHang.Tamkhoa == "True";

                Apply2DvtConfig();
                ApplyGiaBanConfig();

                var dls = await _matHangService.GetDinhLuongByMatHangIdAsync(id, AllMaterials);
                DinhLuongList.Clear();
                foreach (var dl in dls)
                {
                    DinhLuongList.Add(dl);
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            if (_matHangList == null || _matHangList.Count == 0)
            {
                BtnTruoc.IsEnabled = false;
                BtnSau.IsEnabled = false;
                return;
            }
            BtnTruoc.IsEnabled = _currentIndex > 0;
            BtnSau.IsEnabled = _currentIndex >= 0 && _currentIndex < _matHangList.Count - 1;
        }

        private async void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_matHangList != null && _currentIndex > 0)
            {
                _currentIndex--;
                _matHangIdToEdit = _matHangList[_currentIndex].Id;
                this.Title = "MẶT HÀNG - SỬA";
                await LoadDataById(_matHangIdToEdit);
                UpdateNavigationButtons();
            }
        }

        private async void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_matHangList != null && _currentIndex >= 0 && _currentIndex < _matHangList.Count - 1)
            {
                _currentIndex++;
                _matHangIdToEdit = _matHangList[_currentIndex].Id;
                this.Title = "MẶT HÀNG - SỬA";
                await LoadDataById(_matHangIdToEdit);
                UpdateNavigationButtons();
            }
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _matHangIdToEdit = null;
            TxtMaHang.Text = "";
            TxtTenHang.Text = "";
            if (TxtTenTiengAnh != null) TxtTenTiengAnh.Text = "";
            TxtGiaBan.Text = "0";
            if (TxtGia2 != null) TxtGia2.Text = "0";
            if (TxtGia3 != null) TxtGia3.Text = "0";
            if (TxtGia4 != null) TxtGia4.Text = "0";
            TxtGiaNhap.Text = "0";
            if (TxtGiaVon != null) TxtGiaVon.Text = "0";
            TxtQuyDoi.Text = "1";
            ChkGiaTheoThoiGia.IsChecked = false;
            ChkTamKhoa.IsChecked = false;
            DinhLuongList.Clear();
            this.Title = "MẶT HÀNG - THÊM MỚI";
            Apply2DvtConfig();
            ApplyGiaBanConfig();
            UpdateNavigationButtons();
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            await SaveDataAsync();
        }

        private async void BtnLuuThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async System.Threading.Tasks.Task<bool> SaveDataAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtTenHang.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                bool isEdit = !string.IsNullOrEmpty(_matHangIdToEdit);
                var matHang = new MatHangViewModel
                {
                    Id = isEdit ? _matHangIdToEdit : Guid.NewGuid().ToString(),
                    Code = TxtMaHang.Text,
                    Name = TxtTenHang.Text,
                    Tentienganh = TxtTenTiengAnh != null ? TxtTenTiengAnh.Text : null,
                    Giaban = decimal.TryParse(TxtGiaBan.Text.Replace(",", "").Replace(".", ""), out var gb) ? gb : 0,
                    Giaban2 = TxtGia2 != null && decimal.TryParse(TxtGia2.Text.Replace(",", "").Replace(".", ""), out var g2) ? g2 : 0,
                    Giaban3 = TxtGia3 != null && decimal.TryParse(TxtGia3.Text.Replace(",", "").Replace(".", ""), out var g3) ? g3 : 0,
                    Giaban4 = TxtGia4 != null && decimal.TryParse(TxtGia4.Text.Replace(",", "").Replace(".", ""), out var g4) ? g4 : 0,
                    Gianhap = decimal.TryParse(TxtGiaNhap.Text.Replace(",", "").Replace(".", ""), out var gn) ? gn : 0,
                    Giavon = TxtGiaVon != null && decimal.TryParse(TxtGiaVon.Text.Replace(",", "").Replace(".", ""), out var gv) ? gv : 0,
                    Quydoi = _suDung2Dvt ? (string.IsNullOrWhiteSpace(TxtQuyDoi.Text) ? "1" : TxtQuyDoi.Text) : "1",
                    Giatheothoigia = ChkGiaTheoThoiGia.IsChecked == true ? 1 : 0,
                    DnhommathangId = CboNhomMatHang.SelectedValue?.ToString(),
                    DdonvitinhId = CboDvtBan.SelectedValue?.ToString(),
                    DdonvitinhchanId = _suDung2Dvt ? CboDvtNhap.SelectedValue?.ToString() : null,
                    Tamkhoa = ChkTamKhoa.IsChecked == true ? "1" : "0"
                };

                bool result = isEdit ? await _matHangService.UpdateMatHangAsync(matHang) : await _matHangService.InsertMatHangAsync(matHang);
                
                if (result)
                {
                    await _matHangService.SaveDinhLuongListAsync(matHang.Id, DinhLuongList.ToList());

                    string msg = isEdit ? "Cập nhật thành công!" : "Thêm mới thành công!";
                    MessageBox.Show(msg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _onDataSaved?.Invoke();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void BtnThemDong_Click(object sender, RoutedEventArgs e)
        {
            DinhLuongList.Add(new DinhLuongChiTietViewModel { SoLuong = 1 });
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (DgDinhLuong.SelectedItem is DinhLuongChiTietViewModel selected)
            {
                DinhLuongList.Remove(selected);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnThemDvt_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemDonViTinhWindow();
            if (win.ShowDialog() == true)
            {
                _ = LoadDvtDataAsync();
            }
        }

        private void BtnTaiDvt_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDvtDataAsync();
        }

        private void BtnDanhMucDvt_Click(object sender, RoutedEventArgs e)
        {
            var win = new DanhMucDonViTinhWindow();
            win.ShowDialog();
            _ = LoadDvtDataAsync(); // Refresh in case something was added/edited/deleted
        }

        private async System.Threading.Tasks.Task LoadDvtDataAsync()
        {
            try
            {
                var dvtList = await _matHangService.GetDonViTinhListAsync();
                CboDvtBan.ItemsSource = dvtList;
                CboDvtNhap.ItemsSource = dvtList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lại đơn vị tính: " + ex.Message);
            }
        }
    }
}
