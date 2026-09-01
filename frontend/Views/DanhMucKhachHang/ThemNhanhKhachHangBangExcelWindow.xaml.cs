using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExcelDataReader;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class ThemNhanhKhachHangRowModel
    {
        public string Makhach { get; set; } = "";
        public string Tenkhach { get; set; } = "";
        public string Diachi { get; set; } = "";
        public string Dienthoai { get; set; } = "";
        public string Email { get; set; } = "";
        public string Nhomkhach { get; set; } = "";
        public string Masothue { get; set; } = "";
        public string Nhanvien { get; set; } = "";
        public string Tinhthanh { get; set; } = "";
        public string Facebook { get; set; } = "";
        public string Thetratruoc { get; set; } = "";
        public string Ghichu { get; set; } = "";
        public string Diemtichluy { get; set; } = "0";
        public string Ngaysinh { get; set; } = "";
    }

    public partial class ThemNhanhKhachHangBangExcelWindow : Window
    {
        private ObservableCollection<ThemNhanhKhachHangRowModel> _rows = new();
        private List<dynamic> _nhomLookups = new();
        private List<dynamic> _nhanVienLookups = new();
        private List<dynamic> _tinhThanhLookups = new();

        public ThemNhanhKhachHangBangExcelWindow()
        {
            InitializeComponent();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DgDuLieu.ItemsSource = _rows;
            Loaded += ThemNhanhKhachHangBangExcelWindow_Loaded;
        }

        private async void ThemNhanhKhachHangBangExcelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _nhomLookups = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                CboChungNhomKhach.ItemsSource = _nhomLookups;

                _nhanVienLookups = await LocalKhachHangService.GetNhanVienLookupAsync();
                CboChungNhanVien.ItemsSource = _nhanVienLookups;

                _tinhThanhLookups = await LocalKhachHangService.GetTinhThanhLookupAsync();
                CboChungTinhThanh.ItemsSource = _tinhThanhLookups;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading lookups: " + ex.Message);
            }

            if (_rows.Count == 0)
            {
                _rows.Add(new ThemNhanhKhachHangRowModel());
            }

            UpdateColumnVisibility();
        }

        private void ChkThuocTinh_Changed(object sender, RoutedEventArgs e)
        {
            UpdateColumnVisibility();
        }

        private void UpdateColumnVisibility()
        {
            if (ColMaKhach != null && ChkMaKhach != null)
                ColMaKhach.Visibility = (ChkMaKhach.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColTenKhach != null && ChkTenKhach != null)
                ColTenKhach.Visibility = (ChkTenKhach.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColDiaChi != null && ChkDiaChi != null)
                ColDiaChi.Visibility = (ChkDiaChi.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColDienThoai != null && ChkDienThoai != null)
                ColDienThoai.Visibility = (ChkDienThoai.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColEmail != null && ChkEmail != null)
                ColEmail.Visibility = (ChkEmail.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColNhomKhach != null && ChkNhomKhach != null)
                ColNhomKhach.Visibility = (ChkNhomKhach.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColMasothue != null && ChkMaSoThue != null)
                ColMasothue.Visibility = (ChkMaSoThue.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColNhanVien != null && ChkNhanVien != null)
                ColNhanVien.Visibility = (ChkNhanVien.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColTinhThanh != null && ChkTinhThanh != null)
                ColTinhThanh.Visibility = (ChkTinhThanh.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColFacebook != null && ChkFacebook != null)
                ColFacebook.Visibility = (ChkFacebook.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColTheTraTruoc != null && ChkTheTraTruoc != null)
                ColTheTraTruoc.Visibility = (ChkTheTraTruoc.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColGhiChu != null && ChkGhiChu != null)
                ColGhiChu.Visibility = (ChkGhiChu.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColDiemTichLuy != null && ChkDiemTichLuy != null)
                ColDiemTichLuy.Visibility = (ChkDiemTichLuy.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColNgaySinh != null && ChkNgaySinh != null)
                ColNgaySinh.Visibility = (ChkNgaySinh.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnThemDuLieu_Click(object sender, RoutedEventArgs e)
        {
            int count = 1;
            if (int.TryParse(TxtSoDong.Text.Trim(), out int val) && val > 0)
            {
                count = val;
            }

            for (int i = 0; i < count; i++)
            {
                _rows.Add(new ThemNhanhKhachHangRowModel());
            }
        }

        private void BtnXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgDuLieu.SelectedItems.Cast<ThemNhanhKhachHangRowModel>().ToList();
            if (selected.Count > 0)
            {
                foreach (var item in selected)
                {
                    _rows.Remove(item);
                }
            }
            else if (_rows.Count > 0)
            {
                _rows.RemoveAt(_rows.Count - 1);
            }
        }

        private void BtnDanDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Không có dữ liệu trong bộ nhớ tạm (Clipboard)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int added = 0;

                foreach (var line in lines)
                {
                    var cols = line.Split('\t');
                    if (cols.Length == 0 || cols.All(string.IsNullOrWhiteSpace)) continue;

                    var row = new ThemNhanhKhachHangRowModel();
                    if (cols.Length > 0) row.Makhach = cols[0].Trim();
                    if (cols.Length > 1) row.Tenkhach = cols[1].Trim();
                    if (cols.Length > 2) row.Diachi = cols[2].Trim();
                    if (cols.Length > 3) row.Dienthoai = cols[3].Trim();
                    if (cols.Length > 4) row.Email = cols[4].Trim();
                    if (cols.Length > 5) row.Nhomkhach = cols[5].Trim();
                    if (cols.Length > 6) row.Masothue = cols[6].Trim();
                    if (cols.Length > 7) row.Nhanvien = cols[7].Trim();
                    if (cols.Length > 8) row.Tinhthanh = cols[8].Trim();
                    if (cols.Length > 9) row.Facebook = cols[9].Trim();
                    if (cols.Length > 10) row.Thetratruoc = cols[10].Trim();
                    if (cols.Length > 11) row.Ghichu = cols[11].Trim();

                    _rows.Add(row);
                    added++;
                }

                MessageBox.Show($"Đã dán thành công {added} dòng dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dán dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
        {
            var modal = new ChonFileExcelKhachHangWindow();
            modal.Owner = this;
            if (modal.ShowDialog() == true && !string.IsNullOrEmpty(modal.SelectedFilePath))
            {
                LoadExcelFile(modal.SelectedFilePath);
            }
        }

        private async void LoadExcelFile(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count < 2)
                        {
                            MessageBox.Show("File Excel không có dữ liệu để nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var table = dataSet.Tables[0];
                        var actualColumns = new List<string>();
                        var colIndexMap = new Dictionary<string, int>();

                        for (int c = 0; c < table.Columns.Count; c++)
                        {
                            string colName = table.Rows[0][c]?.ToString()?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(colName) && !colIndexMap.ContainsKey(colName))
                            {
                                actualColumns.Add(colName);
                                colIndexMap[colName] = c;
                            }
                        }

                        var khachHangSystemFields = new List<string>
                        {
                            "",
                            "Tên khách hàng",
                            "Nhóm khách hàng",
                            "Mã khách",
                            "Địa chỉ",
                            "Điện thoại",
                            "Email",
                            "Mã số thuế",
                            "Nhân viên",
                            "Tỉnh thành",
                            "Facebook",
                            "Thẻ trả trước",
                            "Ghi chú",
                            "Điểm tích lũy ban đầu",
                            "Ngày thành lập/sinh nhật"
                        };

                        var mappingWin = new MappingExcelWindow(actualColumns, khachHangSystemFields);
                        mappingWin.Owner = this;
                        if (mappingWin.ShowDialog() != true) return;

                        var mappings = mappingWin.MappingList.Where(m => !string.IsNullOrEmpty(m.MappedField)).ToList();
                        if (mappings.Count == 0)
                        {
                            MessageBox.Show("Chưa chọn cột dữ liệu nào để nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Tự động bỏ tích các thuộc tính chung nếu cột đó đã có trong file Excel được map
                        foreach (var map in mappings)
                        {
                            if (map.MappedField == "Mã khách") ChkMaKhach.IsChecked = false;
                            if (map.MappedField == "Tên khách hàng") ChkTenKhach.IsChecked = false;
                            if (map.MappedField == "Địa chỉ") ChkDiaChi.IsChecked = false;
                            if (map.MappedField == "Điện thoại") ChkDienThoai.IsChecked = false;
                            if (map.MappedField == "Email") ChkEmail.IsChecked = false;
                            if (map.MappedField == "Nhóm khách hàng") ChkNhomKhach.IsChecked = false;
                            if (map.MappedField == "Mã số thuế") ChkMaSoThue.IsChecked = false;
                            if (map.MappedField == "Nhân viên") ChkNhanVien.IsChecked = false;
                            if (map.MappedField == "Tỉnh thành") ChkTinhThanh.IsChecked = false;
                            if (map.MappedField == "Facebook") ChkFacebook.IsChecked = false;
                            if (map.MappedField == "Thẻ trả trước") ChkTheTraTruoc.IsChecked = false;
                            if (map.MappedField == "Ghi chú") ChkGhiChu.IsChecked = false;
                            if (map.MappedField == "Điểm tích lũy ban đầu") ChkDiemTichLuy.IsChecked = false;
                            if (map.MappedField == "Ngày thành lập/sinh nhật") ChkNgaySinh.IsChecked = false;
                        }
                        UpdateColumnVisibility();

                        // Kiểm tra Nhóm khách hàng chưa có trong hệ thống
                        var nhomMap = mappings.FirstOrDefault(m => m.MappedField == "Nhóm khách hàng");
                        if (nhomMap != null && colIndexMap.TryGetValue(nhomMap.ExcelColumn, out int nhomColIdx))
                        {
                            var excelGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            for (int r = 1; r < table.Rows.Count; r++)
                            {
                                string grpName = table.Rows[r][nhomColIdx]?.ToString()?.Trim() ?? "";
                                if (!string.IsNullOrEmpty(grpName))
                                {
                                    excelGroups.Add(grpName);
                                }
                            }

                            _nhomLookups = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                            var existingGroupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var item in _nhomLookups)
                            {
                                string n = item.NAME?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(n)) existingGroupNames.Add(n);
                            }

                            var missingGroups = excelGroups.Where(g => !existingGroupNames.Contains(g)).ToList();
                            if (missingGroups.Count > 0)
                            {
                                var confirmResult = MessageBox.Show(
                                    "Dữ liệu excel bạn vừa chọn có một số 'Nhóm khách hàng' chưa tồn tại trong hệ thống\nBạn có muốn thêm vào không?",
                                    "Xác nhận",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (confirmResult == MessageBoxResult.Yes)
                                {
                                    var themNhanhWin = new ThemNhanhKhachHangWindow(0, null, missingGroups);
                                    themNhanhWin.Owner = this;
                                    if (themNhanhWin.ShowDialog() == true)
                                    {
                                        _nhomLookups = await LocalKhachHangService.GetNhomKhachHangLookupAsync();
                                        CboChungNhomKhach.ItemsSource = _nhomLookups;
                                    }
                                }
                            }
                        }

                        _rows.Clear();
                        int loadedCount = 0;

                        for (int r = 1; r < table.Rows.Count; r++)
                        {
                            var rowData = table.Rows[r];
                            bool hasData = false;
                            for (int c = 0; c < table.Columns.Count; c++)
                            {
                                if (!string.IsNullOrWhiteSpace(rowData[c]?.ToString()))
                                {
                                    hasData = true;
                                    break;
                                }
                            }
                            if (!hasData) continue;

                            var rowModel = new ThemNhanhKhachHangRowModel();

                            foreach (var map in mappings)
                            {
                                if (colIndexMap.TryGetValue(map.ExcelColumn, out int colIdx) && colIdx < rowData.ItemArray.Length)
                                {
                                    string val = rowData[colIdx]?.ToString()?.Trim() ?? "";

                                    if (map.MappedField == "Mã khách") rowModel.Makhach = val;
                                    else if (map.MappedField == "Tên khách hàng") rowModel.Tenkhach = val;
                                    else if (map.MappedField == "Địa chỉ") rowModel.Diachi = val;
                                    else if (map.MappedField == "Điện thoại") rowModel.Dienthoai = val;
                                    else if (map.MappedField == "Email") rowModel.Email = val;
                                    else if (map.MappedField == "Nhóm khách hàng") rowModel.Nhomkhach = val;
                                    else if (map.MappedField == "Mã số thuế") rowModel.Masothue = val;
                                    else if (map.MappedField == "Nhân viên") rowModel.Nhanvien = val;
                                    else if (map.MappedField == "Tỉnh thành") rowModel.Tinhthanh = val;
                                    else if (map.MappedField == "Facebook") rowModel.Facebook = val;
                                    else if (map.MappedField == "Thẻ trả trước") rowModel.Thetratruoc = val;
                                    else if (map.MappedField == "Ghi chú") rowModel.Ghichu = val;
                                    else if (map.MappedField == "Điểm tích lũy ban đầu") rowModel.Diemtichluy = val;
                                    else if (map.MappedField == "Ngày thành lập/sinh nhật") rowModel.Ngaysinh = val;
                                }
                            }

                            _rows.Add(rowModel);
                            loadedCount++;
                        }

                        MessageBox.Show($"Đã đọc thành công {loadedCount} khách hàng từ file Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string NormalizeHeader(string header)
        {
            return header.Trim().ToLowerInvariant().Replace(" ", "").Replace("/", "").Replace("-", "").Replace("_", "");
        }

        private string GetVal(DataRow row, Dictionary<string, int> headerMap, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                string norm = NormalizeHeader(name);
                if (headerMap.TryGetValue(norm, out int colIdx) && colIdx < row.ItemArray.Length)
                {
                    var val = row[colIdx]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            return "";
        }

        private void BtnXoaDongLoi_Click(object sender, RoutedEventArgs e)
        {
            var invalid = _rows.Where(x => string.IsNullOrWhiteSpace(x.Tenkhach)).ToList();
            foreach (var item in invalid)
            {
                _rows.Remove(item);
            }
            MessageBox.Show($"Đã xóa {invalid.Count} dòng không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            var validRows = _rows.Where(x => !string.IsNullOrWhiteSpace(x.Tenkhach) || !string.IsNullOrWhiteSpace(x.Makhach)).ToList();
            if (validRows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu khách hàng hợp lệ để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int successCount = 0;
            string defaultNextCode = await LocalKhachHangService.GetNextMaKhachAsync();
            int currentNum = 1;
            if (defaultNextCode.StartsWith("KH") && int.TryParse(defaultNextCode.Substring(2), out int n))
            {
                currentNum = n;
            }

            foreach (var r in validRows)
            {
                string tenKhach = !string.IsNullOrWhiteSpace(r.Tenkhach) ? r.Tenkhach : (ChkTenKhach.IsChecked == true ? TxtChungTenKhach.Text.Trim() : "");
                if (string.IsNullOrWhiteSpace(tenKhach)) continue;

                string maKhach = !string.IsNullOrWhiteSpace(r.Makhach) ? r.Makhach : (ChkMaKhach.IsChecked == true && !string.IsNullOrWhiteSpace(TxtChungMaKhach.Text) ? TxtChungMaKhach.Text.Trim() : $"KH{currentNum++:D5}");
                string diaChi = !string.IsNullOrWhiteSpace(r.Diachi) ? r.Diachi : (ChkDiaChi.IsChecked == true ? TxtChungDiaChi.Text.Trim() : "");
                string dienThoai = !string.IsNullOrWhiteSpace(r.Dienthoai) ? r.Dienthoai : (ChkDienThoai.IsChecked == true ? TxtChungDienThoai.Text.Trim() : "");
                string email = !string.IsNullOrWhiteSpace(r.Email) ? r.Email : (ChkEmail.IsChecked == true ? TxtChungEmail.Text.Trim() : "");
                string maSoThue = !string.IsNullOrWhiteSpace(r.Masothue) ? r.Masothue : (ChkMaSoThue.IsChecked == true ? TxtChungMaSoThue.Text.Trim() : "");
                string facebook = !string.IsNullOrWhiteSpace(r.Facebook) ? r.Facebook : (ChkFacebook.IsChecked == true ? TxtChungFacebook.Text.Trim() : "");
                string ghiChu = !string.IsNullOrWhiteSpace(r.Ghichu) ? r.Ghichu : (ChkGhiChu.IsChecked == true ? TxtChungGhiChu.Text.Trim() : "");

                decimal diemTichLuy = 0;
                if (!string.IsNullOrWhiteSpace(r.Diemtichluy))
                {
                    decimal.TryParse(r.Diemtichluy.Replace(",", "").Replace(".", ""), out diemTichLuy);
                }
                else if (ChkDiemTichLuy.IsChecked == true)
                {
                    decimal.TryParse(TxtChungDiemTichLuy.Text.Replace(",", "").Replace(".", ""), out diemTichLuy);
                }

                DateTime? ngaySinh = null;
                if (!string.IsNullOrWhiteSpace(r.Ngaysinh))
                {
                    if (DateTime.TryParse(r.Ngaysinh, out DateTime dt)) ngaySinh = dt;
                }
                else if (ChkNgaySinh.IsChecked == true && DpChungNgaySinh.SelectedDate != null)
                {
                    ngaySinh = DpChungNgaySinh.SelectedDate;
                }

                // Resolve Lookups
                string nhomId = null;
                if (!string.IsNullOrWhiteSpace(r.Nhomkhach))
                {
                    var match = _nhomLookups.FirstOrDefault(x => string.Equals(x.NAME?.ToString()?.Trim(), r.Nhomkhach.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (match != null) nhomId = match.ID?.ToString();
                }
                if (string.IsNullOrEmpty(nhomId) && ChkNhomKhach.IsChecked == true)
                {
                    nhomId = CboChungNhomKhach.SelectedValue?.ToString();
                }

                string nhanVienId = null;
                if (!string.IsNullOrWhiteSpace(r.Nhanvien))
                {
                    var match = _nhanVienLookups.FirstOrDefault(x => string.Equals(x.NAME?.ToString()?.Trim(), r.Nhanvien.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (match != null) nhanVienId = match.ID?.ToString();
                }
                if (string.IsNullOrEmpty(nhanVienId) && ChkNhanVien.IsChecked == true)
                {
                    nhanVienId = CboChungNhanVien.SelectedValue?.ToString();
                }

                string tinhThanhId = null;
                if (!string.IsNullOrWhiteSpace(r.Tinhthanh))
                {
                    var match = _tinhThanhLookups.FirstOrDefault(x => string.Equals(x.NAME?.ToString()?.Trim(), r.Tinhthanh.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (match != null) tinhThanhId = match.ID?.ToString();
                }
                if (string.IsNullOrEmpty(tinhThanhId) && ChkTinhThanh.IsChecked == true)
                {
                    tinhThanhId = CboChungTinhThanh.SelectedValue?.ToString();
                }

                var model = new KhachHangViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Makhach = maKhach,
                    Name = tenKhach,
                    Diachi = diaChi,
                    Dienthoai = dienThoai,
                    Email = email,
                    Masothue = maSoThue,
                    Facebook = facebook,
                    Note = ghiChu,
                    Diemtichluy = diemTichLuy,
                    Ngaysinh = ngaySinh,
                    DnhomkhachhangId = nhomId,
                    TenNhanVien = nhanVienId,
                    TinhThanh = tinhThanhId
                };

                bool ok = await LocalKhachHangService.SaveKhachHangAsync(model, true);
                if (ok) successCount++;
            }

            MessageBox.Show($"Đã thêm thành công {successCount} khách hàng vào hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
