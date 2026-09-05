using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NhanSu
{
    public partial class ThemPhieuTamUngLuongWindow : Window
    {
        public event Action OnSaved;
        private string _id;
        private List<dynamic> _nhanVienList = new List<dynamic>();

        public ThemPhieuTamUngLuongWindow(string id = null)
        {
            InitializeComponent();
            _id = id;

            Loaded += async (s, e) =>
            {
                await LoadNhanVienListAsync();

                if (!string.IsNullOrEmpty(_id))
                {
                    Title = "TẠM ỨNG LƯƠNG - CHỈNH SỬA";
                    TxtHeaderTitle.Text = "Chỉnh sửa phiếu tạm ứng";
                    await LoadDetailAsync(_id);
                }
                else
                {
                    Title = "TẠM ỨNG LƯƠNG - TẠO MỚI";
                    TxtHeaderTitle.Text = "Tạo mới phiếu tạm ứng";
                    await ResetFormAsync();
                }
            };

            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
            };
        }

        private async Task LoadNhanVienListAsync()
        {
            try
            {
                _nhanVienList = await LocalPhieuThuChiService.GetNhanVienLookupAsync();
                CboNhanVien.ItemsSource = _nhanVienList;
                if (_nhanVienList.Count > 0 && CboNhanVien.SelectedIndex < 0)
                {
                    CboNhanVien.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private async Task ResetFormAsync()
        {
            _id = null;
            TxtSoPhieu.Text = await LocalTamUngLuongService.GetNextSoPhieuAsync();
            DpNgay.SelectedDate = DateTime.Now;
            TxtSoTien.Text = "0";
            TxtGhiChu.Text = "Tạm ứng lương";
            if (_nhanVienList.Count > 0) CboNhanVien.SelectedIndex = 0;
            TxtSoTien.Focus();
            TxtSoTien.SelectAll();
        }

        private async Task LoadDetailAsync(string id)
        {
            var item = await LocalTamUngLuongService.GetByIdAsync(id);
            if (item != null)
            {
                _id = item.Id;
                TxtSoPhieu.Text = item.SoPhieu ?? "";
                DpNgay.SelectedDate = item.Ngay ?? DateTime.Now;
                if (!string.IsNullOrEmpty(item.NhanVienId))
                {
                    CboNhanVien.SelectedValue = item.NhanVienId;
                }
                TxtSoTien.Text = item.SoTien.ToString("N0");
                TxtGhiChu.Text = !string.IsNullOrEmpty(item.DienGiai) ? item.DienGiai : (item.Note ?? "");
            }
        }

        private bool _isFormatting = false;
        private void TxtSoTien_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;
            _isFormatting = true;
            try
            {
                string clean = TxtSoTien.Text.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal val))
                {
                    TxtSoTien.Text = val.ToString("N0");
                    TxtSoTien.CaretIndex = TxtSoTien.Text.Length;
                }
            }
            catch { }
            finally
            {
                _isFormatting = false;
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string clean = text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(clean, out decimal val)) return val;
            return 0;
        }

        private async Task<bool> SaveDataAsync()
        {
            string soPhieu = TxtSoPhieu.Text.Trim();
            DateTime ngay = DpNgay.SelectedDate ?? DateTime.Now;
            string nhanVienId = CboNhanVien.SelectedValue?.ToString() ?? "";
            string tenNhanVien = (CboNhanVien.SelectedItem as dynamic)?.NAME?.ToString() ?? "";
            decimal soTien = ParseDecimal(TxtSoTien.Text);
            string ghiChu = TxtGhiChu.Text.Trim();

            if (string.IsNullOrWhiteSpace(nhanVienId) && string.IsNullOrWhiteSpace(tenNhanVien))
            {
                MessageBox.Show("Vui lòng chọn nhân viên nhận tạm ứng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                CboNhanVien.Focus();
                return false;
            }

            if (soTien <= 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền tạm ứng lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoTien.Focus();
                return false;
            }

            bool ok = await LocalTamUngLuongService.SaveAsync(_id, soPhieu, ngay, nhanVienId, tenNhanVien, soTien, ghiChu);
            if (ok)
            {
                OnSaved?.Invoke();
                return true;
            }
            else
            {
                MessageBox.Show("Lưu phiếu tạm ứng không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu phiếu tạm ứng lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                MessageBox.Show("Đã lưu phiếu tạm ứng lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await ResetFormAsync();
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDataAsync())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
