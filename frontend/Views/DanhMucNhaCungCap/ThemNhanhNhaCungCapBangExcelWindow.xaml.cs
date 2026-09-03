using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExcelDataReader;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucNhaCungCap
{
    public class ThemNhanhNhaCungCapRowModel
    {
        public string MaNhaCungCap { get; set; } = "";
        public string TenNhaCungCap { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public string Email { get; set; } = "";
        public string Website { get; set; } = "";
        public string NhomNhaCungCap { get; set; } = "";
        public string GhiChu { get; set; } = "";
    }

    public partial class ThemNhanhNhaCungCapBangExcelWindow : Window
    {
        private ObservableCollection<ThemNhanhNhaCungCapRowModel> _rows = new();
        private List<NhomNhaCungCapTreeItem> _nhomLookups = new();

        public ThemNhanhNhaCungCapBangExcelWindow()
        {
            InitializeComponent();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DgDuLieu.ItemsSource = _rows;
            Loaded += ThemNhanhNhaCungCapBangExcelWindow_Loaded;
        }

        private async void ThemNhanhNhaCungCapBangExcelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _nhomLookups = await LocalNhaCungCapService.GetAllNhomListFlatAsync();
                CboChungNhomNcc.ItemsSource = _nhomLookups;
                if (_nhomLookups.Count > 0) CboChungNhomNcc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading lookups: " + ex.Message);
            }

            if (_rows.Count == 0)
            {
                _rows.Add(new ThemNhanhNhaCungCapRowModel());
            }

            UpdateColumnVisibility();
        }

        private void ChkThuocTinh_Changed(object sender, RoutedEventArgs e)
        {
            UpdateColumnVisibility();
        }

        private void UpdateColumnVisibility()
        {
            if (ColMaNcc != null && ChkMaNcc != null)
                ColMaNcc.Visibility = (ChkMaNcc.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColTenNcc != null && ChkTenNcc != null)
                ColTenNcc.Visibility = (ChkTenNcc.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColDiaChi != null && ChkDiaChi != null)
                ColDiaChi.Visibility = (ChkDiaChi.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColDienThoai != null && ChkDienThoai != null)
                ColDienThoai.Visibility = (ChkDienThoai.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColEmail != null && ChkEmail != null)
                ColEmail.Visibility = (ChkEmail.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColWebsite != null && ChkWebsite != null)
                ColWebsite.Visibility = (ChkWebsite.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColNhomNcc != null && ChkNhomNcc != null)
                ColNhomNcc.Visibility = (ChkNhomNcc.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;

            if (ColGhiChu != null && ChkGhiChu != null)
                ColGhiChu.Visibility = (ChkGhiChu.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
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
                _rows.Add(new ThemNhanhNhaCungCapRowModel());
            }
        }

        private void BtnXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgDuLieu.SelectedItems.Cast<ThemNhanhNhaCungCapRowModel>().ToList();
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
                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(clipboardText))
                {
                    MessageBox.Show("Bộ nhớ tạm (Clipboard) không có dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var lines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                int count = 0;
                foreach (var line in lines)
                {
                    var cells = line.Split('\t');
                    if (cells.Length == 0 || cells.All(string.IsNullOrWhiteSpace)) continue;

                    var model = new ThemNhanhNhaCungCapRowModel();
                    if (cells.Length > 0) model.MaNhaCungCap = cells[0].Trim();
                    if (cells.Length > 1) model.TenNhaCungCap = cells[1].Trim();
                    if (cells.Length > 2) model.DiaChi = cells[2].Trim();
                    if (cells.Length > 3) model.DienThoai = cells[3].Trim();
                    if (cells.Length > 4) model.Email = cells[4].Trim();
                    if (cells.Length > 5) model.Website = cells[5].Trim();
                    if (cells.Length > 6) model.NhomNhaCungCap = cells[6].Trim();
                    if (cells.Length > 7) model.GhiChu = cells[7].Trim();

                    _rows.Add(model);
                    count++;
                }

                MessageBox.Show($"Đã dán thành công {count} dòng dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dán dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
        {
            var modal = new ChonFileExcelNhaCungCapWindow();
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
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

                        var nhaCungCapSystemFields = new List<string>
                        {
                            "",
                            "Ghi chú",
                            "Tên nhà cung cấp",
                            "Nhóm nhà cung cấp",
                            "Mã nhà cung cấp",
                            "Địa chỉ",
                            "Điện thoại",
                            "Email",
                            "Website"
                        };

                        var mappingWin = new MappingExcelWindow(actualColumns, nhaCungCapSystemFields);
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
                            if (map.MappedField == "Mã nhà cung cấp") ChkMaNcc.IsChecked = false;
                            if (map.MappedField == "Tên nhà cung cấp") ChkTenNcc.IsChecked = false;
                            if (map.MappedField == "Địa chỉ") ChkDiaChi.IsChecked = false;
                            if (map.MappedField == "Điện thoại") ChkDienThoai.IsChecked = false;
                            if (map.MappedField == "Email") ChkEmail.IsChecked = false;
                            if (map.MappedField == "Website") ChkWebsite.IsChecked = false;
                            if (map.MappedField == "Nhóm nhà cung cấp") ChkNhomNcc.IsChecked = false;
                            if (map.MappedField == "Ghi chú") ChkGhiChu.IsChecked = false;
                        }
                        UpdateColumnVisibility();

                        // Kiểm tra Nhóm NCC chưa có trong hệ thống
                        var nhomMap = mappings.FirstOrDefault(m => m.MappedField == "Nhóm nhà cung cấp");
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

                            _nhomLookups = await LocalNhaCungCapService.GetAllNhomListFlatAsync();
                            var existingGroupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var item in _nhomLookups)
                            {
                                string n = item.Name?.Trim();
                                if (!string.IsNullOrEmpty(n)) existingGroupNames.Add(n);
                            }

                            var missingGroups = excelGroups.Where(g => !existingGroupNames.Contains(g)).ToList();
                            if (missingGroups.Count > 0)
                            {
                                var confirmResult = MessageBox.Show(
                                    $"Dữ liệu Excel có {missingGroups.Count} 'Nhóm nhà cung cấp' chưa tồn tại:\n- {string.Join("\n- ", missingGroups)}\n\nBạn có muốn tự động tạo mới các nhóm này không?",
                                    "Tạo nhóm mới",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (confirmResult == MessageBoxResult.Yes)
                                {
                                    foreach (var grp in missingGroups)
                                    {
                                        await LocalNhaCungCapService.SaveNhomNhaCungCapAsync(null, grp, "Tạo tự động từ Excel", null, true);
                                    }
                                    _nhomLookups = await LocalNhaCungCapService.GetAllNhomListFlatAsync();
                                    CboChungNhomNcc.ItemsSource = _nhomLookups;
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

                            var rowModel = new ThemNhanhNhaCungCapRowModel();

                            foreach (var map in mappings)
                            {
                                if (colIndexMap.TryGetValue(map.ExcelColumn, out int colIdx) && colIdx < rowData.ItemArray.Length)
                                {
                                    string val = rowData[colIdx]?.ToString()?.Trim() ?? "";

                                    if (map.MappedField == "Mã nhà cung cấp") rowModel.MaNhaCungCap = val;
                                    else if (map.MappedField == "Tên nhà cung cấp") rowModel.TenNhaCungCap = val;
                                    else if (map.MappedField == "Địa chỉ") rowModel.DiaChi = val;
                                    else if (map.MappedField == "Điện thoại") rowModel.DienThoai = val;
                                    else if (map.MappedField == "Email") rowModel.Email = val;
                                    else if (map.MappedField == "Website") rowModel.Website = val;
                                    else if (map.MappedField == "Nhóm nhà cung cấp") rowModel.NhomNhaCungCap = val;
                                    else if (map.MappedField == "Ghi chú") rowModel.GhiChu = val;
                                }
                            }

                            _rows.Add(rowModel);
                            loadedCount++;
                        }

                        MessageBox.Show($"Đã đọc thành công {loadedCount} nhà cung cấp từ file Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXoaDongLoi_Click(object sender, RoutedEventArgs e)
        {
            var invalid = _rows.Where(x => string.IsNullOrWhiteSpace(x.TenNhaCungCap)).ToList();
            foreach (var item in invalid)
            {
                _rows.Remove(item);
            }
            MessageBox.Show($"Đã xóa {invalid.Count} dòng không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            var validRows = _rows.Where(x => !string.IsNullOrWhiteSpace(x.TenNhaCungCap) || !string.IsNullOrWhiteSpace(x.MaNhaCungCap)).ToList();
            if (validRows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu nhà cung cấp hợp lệ để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int successCount = 0;
            string defaultNextCode = await LocalNhaCungCapService.GetNextMaNhaCungCapAsync();
            int currentNum = 1;
            if (defaultNextCode.StartsWith("NCC") && int.TryParse(defaultNextCode.Substring(3), out int n))
            {
                currentNum = n;
            }

            foreach (var r in validRows)
            {
                string tenNcc = !string.IsNullOrWhiteSpace(r.TenNhaCungCap) ? r.TenNhaCungCap : (ChkTenNcc.IsChecked == true ? TxtChungTenNcc.Text.Trim() : "");
                if (string.IsNullOrWhiteSpace(tenNcc)) continue;

                string maNcc = !string.IsNullOrWhiteSpace(r.MaNhaCungCap) ? r.MaNhaCungCap : (ChkMaNcc.IsChecked == true && !string.IsNullOrWhiteSpace(TxtChungMaNcc.Text) ? TxtChungMaNcc.Text.Trim() : $"NCC{currentNum++:D4}");
                string diaChi = !string.IsNullOrWhiteSpace(r.DiaChi) ? r.DiaChi : (ChkDiaChi.IsChecked == true ? TxtChungDiaChi.Text.Trim() : "");
                string dienThoai = !string.IsNullOrWhiteSpace(r.DienThoai) ? r.DienThoai : (ChkDienThoai.IsChecked == true ? TxtChungDienThoai.Text.Trim() : "");
                string email = !string.IsNullOrWhiteSpace(r.Email) ? r.Email : (ChkEmail.IsChecked == true ? TxtChungEmail.Text.Trim() : "");
                string website = !string.IsNullOrWhiteSpace(r.Website) ? r.Website : (ChkWebsite.IsChecked == true ? TxtChungWebsite.Text.Trim() : "");
                string ghiChu = !string.IsNullOrWhiteSpace(r.GhiChu) ? r.GhiChu : (ChkGhiChu.IsChecked == true ? TxtChungGhiChu.Text.Trim() : "");

                // Resolve Nhóm NCC ID
                string nhomId = null;
                if (!string.IsNullOrWhiteSpace(r.NhomNhaCungCap))
                {
                    var match = _nhomLookups.FirstOrDefault(x => string.Equals(x.Name?.Trim(), r.NhomNhaCungCap.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (match != null) nhomId = match.Id;
                }
                if (string.IsNullOrEmpty(nhomId) && ChkNhomNcc.IsChecked == true)
                {
                    nhomId = CboChungNhomNcc.SelectedValue?.ToString();
                }

                var item = new NhaCungCapItem
                {
                    Id = Guid.NewGuid().ToString(),
                    MaNhaCungCap = maNcc,
                    Name = tenNcc,
                    DiaChi = diaChi,
                    DienThoai = dienThoai,
                    Email = email,
                    Website = website,
                    Note = ghiChu,
                    DnhomnhacungcapId = nhomId
                };

                var (ok, msg, _) = await LocalNhaCungCapService.SaveNhaCungCapAsync(item, true);
                if (ok) successCount++;
            }

            MessageBox.Show($"Đã thêm thành công {successCount} nhà cung cấp vào hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
