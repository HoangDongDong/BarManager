using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhomKhachHangWindow : Window
    {
        private string _id;
        private string _parentId;
        private bool _isNew = true;

        public ThemNhomKhachHangWindow(string id = null, string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _parentId = parentId;
            _isNew = string.IsNullOrEmpty(id);

            Loaded += ThemNhomKhachHangWindow_Loaded;
        }

        private List<SImageViewModel> _images = new();

        private async void ThemNhomKhachHangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var service = new LocalKhachDatHangService();
                _images = await service.GetSImagesAsync();
                CboAnh.ItemsSource = _images;
                if (_images.Count > 0) CboAnh.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading SImages in ThemNhomKhachHangWindow: " + ex.Message);
            }

            if (!_isNew)
            {
                TxtHeaderTitle.Text = "Chỉnh sửa nhóm khách hàng";
                this.Title = "NHÓM KHÁCH HÀNG - CHỈNH SỬA";

                var data = await LocalKhachHangService.GetNhomKhachHangByIdAsync(_id);
                if (data != null)
                {
                    TxtTenNhom.Text = data.NAME?.ToString() ?? "";

                    string simageId = data.SIMAGEID?.ToString() ?? data.IMAGE?.ToString();
                    if (!string.IsNullOrEmpty(simageId) && _images != null)
                    {
                        var matched = _images.FirstOrDefault(x => x.Id == simageId);
                        if (matched != null) CboAnh.SelectedItem = matched;
                    }

                    TxtDiemTichLuy.Text = (data.DIEMTICHLUY != null ? Convert.ToDecimal(data.DIEMTICHLUY) : 0).ToString("N0");
                    TxtGiamTienHang.Text = (data.TILEGIAMGIATIENHANG != null ? Convert.ToDecimal(data.TILEGIAMGIATIENHANG) : 0).ToString("N0");
                    TxtGiamDoAn.Text = (data.TILEGIAMDOAN != null ? Convert.ToDecimal(data.TILEGIAMDOAN) : 0).ToString("N0");
                    TxtGiamDoUong.Text = (data.TILEGIAMDOUONG != null ? Convert.ToDecimal(data.TILEGIAMDOUONG) : 0).ToString("N0");
                    TxtGiamDichVu.Text = (data.TILEGIAMDICHVU != null ? Convert.ToDecimal(data.TILEGIAMDICHVU) : 0).ToString("N0");
                    TxtGiamDoKhac.Text = (data.TILEGIAMDOKHAC != null ? Convert.ToDecimal(data.TILEGIAMDOKHAC) : 0).ToString("N0");
                    TxtGhiChu.Text = data.NOTE?.ToString() ?? "";
                }
            }
            else
            {
                TxtHeaderTitle.Text = "Nhóm khách hàng";
                this.Title = "NHÓM KHÁCH HÀNG - THÊM MỚI";
                TxtTenNhom.Focus();
            }
        }

        private async Task<bool> SaveDataAsync()
        {
            string name = TxtTenNhom.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập Tên nhóm khách hàng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenNhom.Focus();
                return false;
            }

            decimal.TryParse(TxtDiemTichLuy.Text.Trim(), out decimal diemTichLuy);
            decimal.TryParse(TxtGiamTienHang.Text.Trim(), out decimal giamTienHang);
            decimal.TryParse(TxtGiamDoAn.Text.Trim(), out decimal giamDoAn);
            decimal.TryParse(TxtGiamDoUong.Text.Trim(), out decimal giamDoUong);
            decimal.TryParse(TxtGiamDichVu.Text.Trim(), out decimal giamDichVu);
            decimal.TryParse(TxtGiamDoKhac.Text.Trim(), out decimal giamDoKhac);
            string note = TxtGhiChu.Text.Trim();

            bool ok = await LocalKhachHangService.SaveNhomKhachHangAsync(_id, name, diemTichLuy, giamTienHang, giamDoAn, giamDoUong, giamDichVu, giamDoKhac, note, _isNew, _parentId);
            if (ok)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Không thể lưu nhóm khách hàng. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public event Action OnSaved;

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                MessageBox.Show("Lưu nhóm khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                MessageBox.Show("Lưu nhóm khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                OnSaved?.Invoke();
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _id = Guid.NewGuid().ToString();
            _isNew = true;
            TxtTenNhom.Text = "";
            TxtDiemTichLuy.Text = "0";
            TxtGiamTienHang.Text = "0";
            TxtGiamDoAn.Text = "0";
            TxtGiamDoUong.Text = "0";
            TxtGiamDichVu.Text = "0";
            TxtGiamDoKhac.Text = "0";
            TxtGhiChu.Text = "";
            TxtTenNhom.Focus();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
