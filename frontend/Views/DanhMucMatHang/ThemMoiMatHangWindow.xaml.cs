using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private bool _isLoading = false;
        private byte[] _currentImageBytes = null;
        private System.Collections.Generic.List<MatHangViewModel> _fullMaterials = new();

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
            if (e.PropertyName == nameof(DinhLuongChiTietViewModel.SelectedMatHang))
            {
                if (sender is DinhLuongChiTietViewModel item && item.SelectedMatHang != null)
                {
                    if (IsComboSelected() && IsNguyenVatLieu(item.SelectedMatHang))
                    {
                        MessageBox.Show("Mặt hàng là combo không được thêm sản phẩm là nguyên vật liệu", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        item.SelectedMatHang = null;
                        return;
                    }
                }
            }

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
                if (TabMain.SelectedItem == TabDinhLuong && IsDinhLuongAllowed())
                {
                    BtnThemDong_Click(null, null);
                }
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F8)
            {
                if (TabMain.SelectedItem == TabDinhLuong && IsDinhLuongAllowed())
                {
                    BtnXoaDong_Click(null, null);
                }
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
                var nhomTree = await _matHangService.GetNhomMatHangTreeAsync();
                var flatNhomList = new System.Collections.Generic.List<NhomMatHangViewModel>();
                void Flatten(System.Collections.Generic.IEnumerable<NhomMatHangViewModel> items)
                {
                    if (items == null) return;
                    foreach (var it in items)
                    {
                        if (!string.IsNullOrEmpty(it.Id) && it.Id != "-1" && it.Name != "Tất cả" && it.Name != "Thùng rác")
                        {
                            flatNhomList.Add(it);
                        }
                        if (it.Children != null && it.Children.Count > 0)
                        {
                            Flatten(it.Children);
                        }
                    }
                }
                Flatten(nhomTree);
                CboNhomMatHang.ItemsSource = flatNhomList;

                var dvtList = await _matHangService.GetDonViTinhListAsync();
                CboDvtBan.ItemsSource = dvtList;
                CboDvtNhap.ItemsSource = dvtList;

                var allMats = await _matHangService.GetMatHangListAsync(null);
                _fullMaterials = allMats != null ? allMats.ToList() : new System.Collections.Generic.List<MatHangViewModel>();
                RefreshAvailableMaterials();

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

                await LoadLoaiMatHangKhacListAsync();

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
                    RdoPhaChe.IsChecked = true;
                    _currentImageBytes = null;
                    if (ImgMatHang != null) ImgMatHang.Source = null;
                    if (GridLoaiKhac != null) GridLoaiKhac.Visibility = Visibility.Collapsed;
                }
                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task LoadLoaiMatHangKhacListAsync()
        {
            try
            {
                var list = await _matHangService.GetLoaiMatHangKhacListAsync();
                CboLoaiMatHangKhac.ItemsSource = list;
                if (list != null && list.Count > 0 && CboLoaiMatHangKhac.SelectedIndex < 0)
                {
                    CboLoaiMatHangKhac.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void RdoLoai_Checked(object sender, RoutedEventArgs e)
        {
            if (GridLoaiKhac == null) return;
            if (RdoKhac.IsChecked == true)
            {
                GridLoaiKhac.Visibility = Visibility.Visible;
                if (CboLoaiMatHangKhac.ItemsSource == null)
                {
                    _ = LoadLoaiMatHangKhacListAsync();
                }
            }
            else
            {
                GridLoaiKhac.Visibility = Visibility.Collapsed;
            }

            RefreshAvailableMaterials();

            if (_isLoading) return;

            // Nếu người dùng chọn loại mặt hàng không cho phép định lượng nhưng trước đó đã có định lượng
            if (!IsDinhLuongAllowed() && DinhLuongList != null && DinhLuongList.Count > 0)
            {
                MessageBox.Show("Mặt hàng này không có vật tư / định lượng đi kèm", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (IsComboSelected() && DinhLuongList != null && DinhLuongList.Any(x => IsNguyenVatLieu(x.SelectedMatHang)))
            {
                MessageBox.Show("Mặt hàng là combo không được thêm sản phẩm là nguyên vật liệu", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CboLoaiMatHangKhac_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAvailableMaterials();

            if (_isLoading) return;
            if (RdoKhac?.IsChecked == true && !IsDinhLuongAllowed() && DinhLuongList != null && DinhLuongList.Count > 0)
            {
                MessageBox.Show("Mặt hàng này không có vật tư / định lượng đi kèm", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (IsComboSelected() && DinhLuongList != null && DinhLuongList.Any(x => IsNguyenVatLieu(x.SelectedMatHang)))
            {
                MessageBox.Show("Mặt hàng là combo không được thêm sản phẩm là nguyên vật liệu", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnThemLoai_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemLoaiMatHangWindow();
            if (win.ShowDialog() == true)
            {
                await LoadLoaiMatHangKhacListAsync();
                if (win.SavedItem != null && !string.IsNullOrEmpty(win.SavedItem.Id))
                {
                    CboLoaiMatHangKhac.SelectedValue = win.SavedItem.Id;
                }
            }
        }

        private async void BtnTaiLoai_Click(object sender, RoutedEventArgs e)
        {
            string curr = CboLoaiMatHangKhac.SelectedValue?.ToString();
            await LoadLoaiMatHangKhacListAsync();
            if (!string.IsNullOrEmpty(curr))
            {
                CboLoaiMatHangKhac.SelectedValue = curr;
            }
        }

        private async void BtnDanhMucLoai_Click(object sender, RoutedEventArgs e)
        {
            var win = new DanhMucLoaiMatHangWindow();
            win.OnDataChanged = async () =>
            {
                await LoadLoaiMatHangKhacListAsync();
            };
            win.ShowDialog();
            await LoadLoaiMatHangKhacListAsync();
        }

        private void BtnDanAnh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Kiểm tra ảnh vừa chụp (PrtScn, Win+Shift+S) hoặc vừa Copy trên clipboard
                if (Clipboard.ContainsImage())
                {
                    var bitmapSource = Clipboard.GetImage();
                    if (bitmapSource != null)
                    {
                        using var ms = new System.IO.MemoryStream();
                        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));
                        encoder.Save(ms);
                        byte[] rawBytes = ms.ToArray();

                        // Tự động Resize và Nén tối ưu
                        _currentImageBytes = ImageHelper.OptimizeImage(rawBytes, maxWidth: 500, maxHeight: 500, jpegQuality: 80);
                        ImgMatHang.Source = ImageHelper.BytesToBitmapImage(_currentImageBytes);
                        return;
                    }
                }

                // 2. Kiểm tra nếu clipboard là tệp ảnh được copy từ thư mục máy tính
                if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList();
                    if (files != null && files.Count > 0)
                    {
                        foreach (string file in files)
                        {
                            string ext = System.IO.Path.GetExtension(file).ToLower();
                            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".webp" || ext == ".ico")
                            {
                                byte[] rawBytes = System.IO.File.ReadAllBytes(file);
                                _currentImageBytes = ImageHelper.OptimizeImage(rawBytes, maxWidth: 500, maxHeight: 500, jpegQuality: 80);
                                ImgMatHang.Source = ImageHelper.BytesToBitmapImage(_currentImageBytes);
                                return;
                            }
                        }
                    }
                }

                MessageBox.Show("Không tìm thấy ảnh vừa chụp hoặc sao chép trong bộ nhớ tạm!\n\nBạn có thể dùng tổ hợp phím Windows + Shift + S (hoặc phím PrtScn) để chụp ảnh màn hình, rồi bấm lại nút này để dán ảnh.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dán ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChonFileAnh_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Hình ảnh (*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp)|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|Tất cả tệp (*.*)|*.*",
                Title = "Chọn ảnh mặt hàng"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    byte[] rawBytes = System.IO.File.ReadAllBytes(ofd.FileName);
                    // Tự động Resize (max 500x500) và Nén ảnh chất lượng cao (JPEG 80% hoặc PNG trong suốt)
                    _currentImageBytes = ImageHelper.OptimizeImage(rawBytes, maxWidth: 500, maxHeight: 500, jpegQuality: 80);
                    ImgMatHang.Source = ImageHelper.BytesToBitmapImage(_currentImageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnXoaAnh_Click(object sender, RoutedEventArgs e)
        {
            _currentImageBytes = null;
            ImgMatHang.Source = null;
        }

        private static System.Windows.Media.Imaging.BitmapImage LoadBitmapFromBytes(byte[] bytes)
        {
            return ImageHelper.BytesToBitmapImage(bytes);
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
            _isLoading = true;
            try
            {
                DinhLuongList.Clear();
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

                    // Load Loại mặt hàng
                    string loaiId = matHang.DloaimathangId?.Trim();
                    if (loaiId == "1")
                    {
                        RdoPhaChe.IsChecked = true;
                        if (GridLoaiKhac != null) GridLoaiKhac.Visibility = Visibility.Collapsed;
                    }
                    else if (loaiId == "2")
                    {
                        RdoVatTu.IsChecked = true;
                        if (GridLoaiKhac != null) GridLoaiKhac.Visibility = Visibility.Collapsed;
                    }
                    else if (loaiId == "3")
                    {
                        RdoMo.IsChecked = true;
                        if (GridLoaiKhac != null) GridLoaiKhac.Visibility = Visibility.Collapsed;
                    }
                    else if (loaiId == "0" || string.IsNullOrEmpty(loaiId))
                    {
                        RdoKiemVatTu.IsChecked = true;
                        if (GridLoaiKhac != null) GridLoaiKhac.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        RdoKhac.IsChecked = true;
                        if (GridLoaiKhac != null) GridLoaiKhac.Visibility = Visibility.Visible;
                        await LoadLoaiMatHangKhacListAsync();
                        CboLoaiMatHangKhac.SelectedValue = loaiId;
                    }

                    _currentImageBytes = matHang.ImageBytes;
                    ImgMatHang.Source = LoadBitmapFromBytes(_currentImageBytes);

                    RefreshAvailableMaterials();
                    var dls = await _matHangService.GetDinhLuongByMatHangIdAsync(id, _fullMaterials != null && _fullMaterials.Count > 0 ? _fullMaterials : AllMaterials);
                    DinhLuongList.Clear();
                    foreach (var dl in dls)
                    {
                        DinhLuongList.Add(dl);
                    }
                }
            }
            finally
            {
                _isLoading = false;
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
            _isLoading = true;
            try
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
                RdoPhaChe.IsChecked = true;
                _currentImageBytes = null;
                if (ImgMatHang != null) ImgMatHang.Source = null;
                DinhLuongList.Clear();
                RefreshAvailableMaterials();
                this.Title = "MẶT HÀNG - THÊM MỚI";
                Apply2DvtConfig();
                ApplyGiaBanConfig();
                UpdateNavigationButtons();
            }
            finally
            {
                _isLoading = false;
            }
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

                string loaiId = "0";
                if (RdoPhaChe.IsChecked == true) loaiId = "1";
                else if (RdoKiemVatTu.IsChecked == true) loaiId = "0";
                else if (RdoVatTu.IsChecked == true) loaiId = "2";
                else if (RdoMo.IsChecked == true) loaiId = "3";
                else if (RdoKhac.IsChecked == true)
                {
                    loaiId = CboLoaiMatHangKhac.SelectedValue?.ToString();
                    if (string.IsNullOrEmpty(loaiId))
                    {
                        MessageBox.Show("Vui lòng chọn một loại mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        CboLoaiMatHangKhac.Focus();
                        return false;
                    }
                }

                // Nếu người dùng chọn loại mặt hàng không cho phép định lượng nhưng có định lượng đi kèm thì cảnh báo và chưa cho lưu
                if (!IsDinhLuongAllowed() && DinhLuongList != null && DinhLuongList.Count > 0)
                {
                    MessageBox.Show("Mặt hàng này không có vật tư / định lượng đi kèm", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Nếu là Combo: không được thêm nguyên vật liệu vào định lượng
                if (IsComboSelected() && DinhLuongList != null && DinhLuongList.Count > 0)
                {
                    var rawMat = DinhLuongList.FirstOrDefault(x => IsNguyenVatLieu(x.SelectedMatHang));
                    if (rawMat != null)
                    {
                        MessageBox.Show("Mặt hàng là combo không được thêm sản phẩm là nguyên vật liệu", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        TabMain.SelectedItem = TabDinhLuong;
                        return false;
                    }
                }

                // Kiểm tra số lượng cho định lượng
                if (IsDinhLuongAllowed() && DinhLuongList != null && DinhLuongList.Count > 0)
                {
                    var zeroQty = DinhLuongList.FirstOrDefault(x => x.SelectedMatHang != null && x.SoLuong <= 0);
                    if (zeroQty != null)
                    {
                        MessageBox.Show("Mời bạn nhập số lượng cho định lượng", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        TabMain.SelectedItem = TabDinhLuong;
                        return false;
                    }
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
                    DloaimathangId = loaiId,
                    DdonvitinhId = CboDvtBan.SelectedValue?.ToString(),
                    DdonvitinhchanId = _suDung2Dvt ? CboDvtNhap.SelectedValue?.ToString() : null,
                    Tamkhoa = ChkTamKhoa.IsChecked == true ? "1" : "0",
                    ImageBytes = _currentImageBytes
                };

                bool result = isEdit ? await _matHangService.UpdateMatHangAsync(matHang) : await _matHangService.InsertMatHangAsync(matHang);
                
                if (result)
                {
                    if (IsDinhLuongAllowed())
                    {
                        await _matHangService.SaveDinhLuongListAsync(matHang.Id, DinhLuongList.ToList());
                    }
                    else
                    {
                        await _matHangService.SaveDinhLuongListAsync(matHang.Id, new System.Collections.Generic.List<DinhLuongChiTietViewModel>());
                    }

                    if (_matHangList != null)
                    {
                        var cached = _matHangList.FirstOrDefault(x => x.Id == matHang.Id);
                        if (cached != null)
                        {
                            cached.DloaimathangId = loaiId;
                            cached.Name = matHang.Name;
                            cached.Code = matHang.Code;
                            cached.Giaban = matHang.Giaban;
                            cached.Gianhap = matHang.Gianhap;
                            cached.DnhommathangId = matHang.DnhommathangId;
                            cached.DdonvitinhId = matHang.DdonvitinhId;
                        }
                    }
                    _matHangIdToEdit = matHang.Id;

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

        private bool IsDinhLuongAllowed()
        {
            // 1. Mặt hàng pha chế: được phép định lượng
            if (RdoPhaChe?.IsChecked == true)
            {
                return true;
            }

            // 2. Mặt hàng kiêm vật tư, Vật tư nguyên liệu, Mặt hàng mở: không được phép định lượng
            if (RdoKiemVatTu?.IsChecked == true || RdoVatTu?.IsChecked == true || RdoMo?.IsChecked == true)
            {
                return false;
            }

            // 3. Khác: chỉ có Combo, Combo mở, Dịch vụ theo giờ là được phép định lượng
            if (RdoKhac?.IsChecked == true)
            {
                DLOAIMATHANG selectedLoai = CboLoaiMatHangKhac?.SelectedItem as DLOAIMATHANG;
                if (selectedLoai == null && CboLoaiMatHangKhac?.SelectedValue != null && CboLoaiMatHangKhac.ItemsSource is System.Collections.Generic.IEnumerable<DLOAIMATHANG> loaiList)
                {
                    string selId = CboLoaiMatHangKhac.SelectedValue.ToString();
                    selectedLoai = loaiList.FirstOrDefault(x => x.Id == selId);
                }

                if (selectedLoai != null)
                {
                    string name = selectedLoai.Name?.Trim().ToLower() ?? "";
                    string id = selectedLoai.Id?.Trim() ?? "";

                    // Chỉ có Combo (ID=4), Combo mở (ID=5) và Dịch vụ theo giờ (ID=7) là được phép định lượng
                    if (id == "4" || id == "5" || id == "7")
                        return true;

                    if (name.StartsWith("combo") || name.Contains("theo giờ") || name.Contains("thêm giờ"))
                        return true;
                }

                return false;
            }

            return false;
        }

        private bool IsComboSelected()
        {
            if (RdoKhac?.IsChecked != true) return false;

            DLOAIMATHANG selectedLoai = CboLoaiMatHangKhac?.SelectedItem as DLOAIMATHANG;
            if (selectedLoai == null && CboLoaiMatHangKhac?.SelectedValue != null && CboLoaiMatHangKhac.ItemsSource is System.Collections.Generic.IEnumerable<DLOAIMATHANG> loaiList)
            {
                string selId = CboLoaiMatHangKhac.SelectedValue.ToString();
                selectedLoai = loaiList.FirstOrDefault(x => x.Id == selId);
            }

            if (selectedLoai != null)
            {
                string name = selectedLoai.Name?.Trim().ToLower() ?? "";
                string id = selectedLoai.Id?.Trim() ?? "";
                return id == "4" || id == "5" || name.StartsWith("combo") || name.Contains("combo");
            }
            return false;
        }

        private static bool IsNguyenVatLieu(MatHangViewModel mat)
        {
            if (mat == null) return false;
            string loaiId = mat.DloaimathangId?.Trim() ?? "";
            if (loaiId == "2") return true;

            string loaiName = mat.LoaiMatHangName?.Trim().ToLower() ?? "";
            if (loaiName == "vật tư nguyên liệu" || loaiName == "nguyên liệu" || loaiName.Contains("nguyên vật liệu"))
                return true;

            return false;
        }

        private void RefreshAvailableMaterials()
        {
            if (_fullMaterials == null) return;

            bool isCombo = IsComboSelected();

            AllMaterials.Clear();
            foreach (var mat in _fullMaterials)
            {
                // Nếu mặt hàng hiện tại là Combo thì KHÔNG được thêm sản phẩm là nguyên vật liệu
                if (isCombo && IsNguyenVatLieu(mat))
                {
                    continue;
                }
                AllMaterials.Add(mat);
            }
        }

        private bool _warningShown = false;
        private bool _isRevertingTab = false;

        private void TabDinhLuong_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TabMain?.SelectedItem != TabDinhLuong)
            {
                if (!IsDinhLuongAllowed())
                {
                    e.Handled = true;
                    _warningShown = true;
                    MessageBox.Show("Mặt hàng này không có vật tư / định lượng đi kèm", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRevertingTab) return;
            if (e.Source != TabMain) return;

            if (TabMain?.SelectedItem == TabDinhLuong)
            {
                if (!IsDinhLuongAllowed())
                {
                    _isRevertingTab = true;
                    TabMain.SelectedItem = TabThongTin;
                    _isRevertingTab = false;

                    if (!_warningShown)
                    {
                        MessageBox.Show("Mặt hàng này không có vật tư / định lượng đi kèm", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            _warningShown = false;
        }

        private void BtnThemDong_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDinhLuongAllowed())
            {
                MessageBox.Show("Mặt hàng này không có vật tư / định lượng đi kèm", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            RefreshAvailableMaterials();
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
