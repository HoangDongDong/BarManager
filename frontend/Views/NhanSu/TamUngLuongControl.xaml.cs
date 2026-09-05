using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.PhieuThuChi;

namespace QuanLyBar.Client.Views.NhanSu
{
    public partial class TamUngLuongControl : UserControl
    {
        private List<TamUngLuongItemViewModel> _list = new List<TamUngLuongItemViewModel>();
        private bool _isLoaded = false;
        private DataGridColumn _clickedColumn = null;
        private string _clickedCellValue = "";

        public TamUngLuongControl()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    SetDefaultDates();
                    await LoadDataAsync();
                }
            };
        }

        private void SetDefaultDates()
        {
            var now = DateTime.Now;
            DpTuNgay.SelectedDate = new DateTime(2001, 1, 1);
            DpDenNgay.SelectedDate = now;
        }

        private void BtnDateFilter_Click(object sender, RoutedEventArgs e)
        {
            var ctx = new ContextMenu();
            var now = DateTime.Today;

            // 1. Từ trước ngày hôm nay
            var m0 = new MenuItem { Header = "Từ trước ngày hôm nay" };
            m0.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = new DateTime(2001, 1, 1);
                DpDenNgay.SelectedDate = now.AddDays(-1);
            };
            ctx.Items.Add(m0);

            // 2. Hôm nay
            var m1 = new MenuItem { Header = "Hôm nay" };
            m1.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = now;
                DpDenNgay.SelectedDate = now;
            };
            ctx.Items.Add(m1);

            // 3. Hôm qua
            var m2 = new MenuItem { Header = "Hôm qua" };
            m2.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = now.AddDays(-1);
                DpDenNgay.SelectedDate = now.AddDays(-1);
            };
            ctx.Items.Add(m2);

            ctx.Items.Add(new Separator());

            // 4. Tuần này
            var m3 = new MenuItem { Header = "Tuần này" };
            m3.Click += (s, ev) =>
            {
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                var mon = now.AddDays(-1 * diff);
                DpTuNgay.SelectedDate = mon;
                DpDenNgay.SelectedDate = mon.AddDays(6);
            };
            ctx.Items.Add(m3);

            // 5. Tuần trước
            var m4 = new MenuItem { Header = "Tuần trước" };
            m4.Click += (s, ev) =>
            {
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                var lastMon = now.AddDays(-1 * diff - 7);
                DpTuNgay.SelectedDate = lastMon;
                DpDenNgay.SelectedDate = lastMon.AddDays(6);
            };
            ctx.Items.Add(m4);

            ctx.Items.Add(new Separator());

            // 6. Tháng này
            var m5 = new MenuItem { Header = "Tháng này" };
            m5.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
                DpDenNgay.SelectedDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            };
            ctx.Items.Add(m5);

            // 7. Tháng trước
            var m6 = new MenuItem { Header = "Tháng trước" };
            m6.Click += (s, ev) =>
            {
                var first = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                DpTuNgay.SelectedDate = first;
                DpDenNgay.SelectedDate = first.AddMonths(1).AddDays(-1);
            };
            ctx.Items.Add(m6);

            // 8. Tháng ▸ (Submenu: Tháng 1 .. Tháng 12)
            var mThang = new MenuItem { Header = "Tháng" };
            for (int m = 1; m <= 12; m++)
            {
                int month = m;
                var subM = new MenuItem { Header = $"Tháng {month}" };
                subM.Click += (s, ev) =>
                {
                    DpTuNgay.SelectedDate = new DateTime(now.Year, month, 1);
                    DpDenNgay.SelectedDate = new DateTime(now.Year, month, DateTime.DaysInMonth(now.Year, month));
                };
                mThang.Items.Add(subM);
            }
            ctx.Items.Add(mThang);

            ctx.Items.Add(new Separator());

            // 9. Quý này
            var m7 = new MenuItem { Header = "Quý này" };
            m7.Click += (s, ev) =>
            {
                int q = (now.Month - 1) / 3 + 1;
                int sm = (q - 1) * 3 + 1;
                DpTuNgay.SelectedDate = new DateTime(now.Year, sm, 1);
                DpDenNgay.SelectedDate = new DateTime(now.Year, sm + 2, DateTime.DaysInMonth(now.Year, sm + 2));
            };
            ctx.Items.Add(m7);

            // 10. Quý trước
            var m8 = new MenuItem { Header = "Quý trước" };
            m8.Click += (s, ev) =>
            {
                int q = (now.Month - 1) / 3 + 1;
                int prevQ = q - 1;
                int y = now.Year;
                if (prevQ < 1) { prevQ = 4; y--; }
                int sm = (prevQ - 1) * 3 + 1;
                DpTuNgay.SelectedDate = new DateTime(y, sm, 1);
                DpDenNgay.SelectedDate = new DateTime(y, sm + 2, DateTime.DaysInMonth(y, sm + 2));
            };
            ctx.Items.Add(m8);

            // 11. Quý ▸ (Submenu: Quý 1 .. Quý 4)
            var mQuy = new MenuItem { Header = "Quý" };
            for (int k = 1; k <= 4; k++)
            {
                int qIdx = k;
                var subQ = new MenuItem { Header = $"Quý {qIdx}" };
                subQ.Click += (s, ev) =>
                {
                    int sm = (qIdx - 1) * 3 + 1;
                    DpTuNgay.SelectedDate = new DateTime(now.Year, sm, 1);
                    DpDenNgay.SelectedDate = new DateTime(now.Year, sm + 2, DateTime.DaysInMonth(now.Year, sm + 2));
                };
                mQuy.Items.Add(subQ);
            }
            ctx.Items.Add(mQuy);

            ctx.Items.Add(new Separator());

            // 12. Năm nay
            var m9 = new MenuItem { Header = "Năm nay" };
            m9.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = new DateTime(now.Year, 1, 1);
                DpDenNgay.SelectedDate = new DateTime(now.Year, 12, 31);
            };
            ctx.Items.Add(m9);

            // 13. Năm ngoái
            var m10 = new MenuItem { Header = "Năm ngoái" };
            m10.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = new DateTime(now.Year - 1, 1, 1);
                DpDenNgay.SelectedDate = new DateTime(now.Year - 1, 12, 31);
            };
            ctx.Items.Add(m10);

            ctx.Items.Add(new Separator());

            // 14. Tất cả
            var m11 = new MenuItem { Header = "Tất cả" };
            m11.Click += (s, ev) =>
            {
                DpTuNgay.SelectedDate = new DateTime(2001, 1, 1);
                DpDenNgay.SelectedDate = now.AddYears(1);
            };
            ctx.Items.Add(m11);

            if (sender is Button btn)
            {
                ctx.PlacementTarget = btn;
                ctx.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                ctx.IsOpen = true;
            }
        }

        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                await LoadDataAsync();
            }
        }

        private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            string kw = TxtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(kw))
            {
                DgTamUngLuong.ItemsSource = _list;
            }
            else
            {
                DgTamUngLuong.ItemsSource = _list.Where(x =>
                    (x.SoPhieu?.ToLower().Contains(kw) ?? false) ||
                    (x.NguoiNhan?.ToLower().Contains(kw) ?? false) ||
                    (x.DienGiai?.ToLower().Contains(kw) ?? false) ||
                    (x.SoTienStr?.ToLower().Contains(kw) ?? false)
                ).ToList();
            }
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                DateTime tuNgay = DpTuNgay.SelectedDate ?? DateTime.Today.AddMonths(-1);
                DateTime denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;

                _list = await LocalTamUngLuongService.GetListAsync(tuNgay, denNgay);
                DgTamUngLuong.ItemsSource = _list;

                TxtTotalCount.Text = _list.Count.ToString("N0");
                decimal totalMoney = _list.Sum(x => x.SoTien);
                TxtTotalAmount.Text = totalMoney.ToString("N0") + " VNĐ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách tạm ứng lương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new TaoPhieuChiWindow(null, isTamUng: true);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadDataAsync();
            };
            win.ShowDialog();
            if (win.IsSaved || win.DialogResult == true)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgTamUngLuong.SelectedItem as TamUngLuongItemViewModel;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu tạm ứng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new TaoPhieuChiWindow(selected.Id, isTamUng: true);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadDataAsync();
            };
            win.ShowDialog();
            if (win.IsSaved || win.DialogResult == true)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgTamUngLuong.SelectedItem as TamUngLuongItemViewModel;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu tạm ứng để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ask = MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu tạm ứng '{selected.SoPhieu}' của nhân viên '{selected.NguoiNhan}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ask == MessageBoxResult.Yes)
            {
                bool ok = await LocalTamUngLuongService.DeleteAsync(selected.Id);
                if (ok)
                {
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Xóa phiếu tạm ứng không thành công!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgTamUngLuong_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgTamUngLuong.SelectedItem != null)
            {
                BtnChinhSua_Click(null, null);
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            var parentObj = VisualTreeHelper.GetParent(child);
            if (parentObj == null) return null;
            if (parentObj is T parent) return parent;
            return FindVisualParent<T>(parentObj);
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
                var pt = e.GetPosition(DgTamUngLuong);
                var element = DgTamUngLuong.InputHitTest(pt) as DependencyObject;
                var cell = FindVisualParent<DataGridCell>(element);
                if (cell != null)
                {
                    _clickedColumn = cell.Column;
                    if (cell.Content is TextBlock tb)
                    {
                        _clickedCellValue = tb.Text;
                    }
                    else if (cell.Content != null)
                    {
                        _clickedCellValue = cell.Content.ToString();
                    }
                }
            }
        }

        private void GridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            string colHeader = _clickedColumn?.Header?.ToString() ?? "Số phiếu";
            if (MenuDatCot != null)
            {
                MenuDatCot.Header = $"Đặt {colHeader}";
            }
        }

        private void MenuDatCot_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                TxtSearch.Text = _clickedCellValue;
                TxtSearch.Focus();
                TxtSearch.SelectAll();
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Không thể thực hiện import/cập nhật dữ liệu từ excel với dữ liệu này", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MenuItem_SortAsc_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null && _list.Count > 0)
            {
                string colHeader = _clickedColumn?.Header?.ToString() ?? "";
                if (colHeader == "Ngày")
                    _list = _list.OrderBy(x => x.Ngay).ToList();
                else if (colHeader == "Người nhận")
                    _list = _list.OrderBy(x => x.NguoiNhan).ToList();
                else if (colHeader == "Số tiền")
                    _list = _list.OrderBy(x => x.SoTien).ToList();
                else
                    _list = _list.OrderBy(x => x.SoPhieu).ToList();

                DgTamUngLuong.ItemsSource = null;
                DgTamUngLuong.ItemsSource = _list;
            }
        }

        private void MenuItem_SortDesc_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null && _list.Count > 0)
            {
                string colHeader = _clickedColumn?.Header?.ToString() ?? "";
                if (colHeader == "Ngày")
                    _list = _list.OrderByDescending(x => x.Ngay).ToList();
                else if (colHeader == "Người nhận")
                    _list = _list.OrderByDescending(x => x.NguoiNhan).ToList();
                else if (colHeader == "Số tiền")
                    _list = _list.OrderByDescending(x => x.SoTien).ToList();
                else
                    _list = _list.OrderByDescending(x => x.SoPhieu).ToList();

                DgTamUngLuong.ItemsSource = null;
                DgTamUngLuong.ItemsSource = _list;
            }
        }

        private void MenuItem_SortBySoPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null)
            {
                _list = _list.OrderBy(x => x.SoPhieu).ToList();
                DgTamUngLuong.ItemsSource = null;
                DgTamUngLuong.ItemsSource = _list;
            }
        }

        private void MenuItem_SortByNgay_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null)
            {
                _list = _list.OrderBy(x => x.Ngay).ToList();
                DgTamUngLuong.ItemsSource = null;
                DgTamUngLuong.ItemsSource = _list;
            }
        }

        private void MenuItem_SortByNguoiNhan_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null)
            {
                _list = _list.OrderBy(x => x.NguoiNhan).ToList();
                DgTamUngLuong.ItemsSource = null;
                DgTamUngLuong.ItemsSource = _list;
            }
        }

        private void MenuItem_SortBySoTien_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null)
            {
                _list = _list.OrderBy(x => x.SoTien).ToList();
                DgTamUngLuong.ItemsSource = null;
                DgTamUngLuong.ItemsSource = _list;
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng in danh sách đang được chuẩn bị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_SaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                Clipboard.SetText(_clickedCellValue);
            }
        }

        private void MenuItem_SaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgTamUngLuong.SelectedItem as TamUngLuongItemViewModel;
            if (selected != null)
            {
                string text = $"{selected.SoPhieu}\t{selected.NgayStr}\t{selected.NguoiNhan}\t{selected.SoTienStr}\t{selected.DienGiai}";
                Clipboard.SetText(text);
            }
        }

        private void MenuItem_TuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgTamUngLuong.Columns)
            {
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }

        private void MenuCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tùy chỉnh cột hiển thị đang được áp dụng mặc định.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_ThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgTamUngLuong.SelectedItem as TamUngLuongItemViewModel;
            if (selected != null)
            {
                MessageBox.Show($"Phiếu: {selected.SoPhieu}\nNgày: {selected.NgayStr}\nNgười nhận: {selected.NguoiNhan}\nSố tiền: {selected.SoTienStr}\nDiễn giải: {selected.DienGiai}", "Thuộc tính phiếu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
