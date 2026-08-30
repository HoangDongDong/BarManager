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
            TxtTongSoLuong.Text = DinhLuongList.Sum(x => x.SoLuong).ToString("N0");
            TxtTongTienNhap.Text = DinhLuongList.Sum(x => x.ThanhTienNhap).ToString("N0");
            TxtTongTienVon.Text = DinhLuongList.Sum(x => x.ThanhTienVon).ToString("N0");
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

        private async System.Threading.Tasks.Task LoadDataById(string id)
        {
            var matHang = await _matHangService.GetMatHangByIdAsync(id);
            if (matHang != null)
            {
                TxtMaHang.Text = matHang.Code;
                TxtTenHang.Text = matHang.Name;
                TxtGiaBan.Text = matHang.Giaban?.ToString("N0") ?? "0";
                TxtGiaNhap.Text = matHang.Gianhap?.ToString("N0") ?? "0";
                TxtQuyDoi.Text = matHang.Quydoi;
                ChkGiaTheoThoiGia.IsChecked = matHang.Giatheothoigia == 1;
                CboNhomMatHang.SelectedValue = matHang.DnhommathangId;
                CboDvtBan.SelectedValue = matHang.DdonvitinhId;
                ChkTamKhoa.IsChecked = matHang.Tamkhoa == "1" || matHang.Tamkhoa == "True";

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
            TxtGiaBan.Text = "0";
            TxtGiaNhap.Text = "0";
            TxtQuyDoi.Text = "1";
            ChkGiaTheoThoiGia.IsChecked = false;
            ChkTamKhoa.IsChecked = false;
            DinhLuongList.Clear();
            this.Title = "MẶT HÀNG - THÊM MỚI";
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
                    Giaban = decimal.TryParse(TxtGiaBan.Text, out var gb) ? gb : 0,
                    Gianhap = decimal.TryParse(TxtGiaNhap.Text, out var gn) ? gn : 0,
                    Quydoi = TxtQuyDoi.Text,
                    Giatheothoigia = ChkGiaTheoThoiGia.IsChecked == true ? 1 : 0,
                    DnhommathangId = CboNhomMatHang.SelectedValue?.ToString(),
                    DdonvitinhId = CboDvtBan.SelectedValue?.ToString(),
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
