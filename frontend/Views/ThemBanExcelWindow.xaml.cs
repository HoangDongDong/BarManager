using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using ExcelDataReader;

namespace QuanLyBar.Client.Views
{
    public partial class ThemBanExcelWindow : Window
    {
        private readonly LocalBanKhuVucService _service;
        private ObservableCollection<BanViewModel> _quickAddList;
        private Action _onDataSaved;

        public ThemBanExcelWindow(Action onDataSaved = null)
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
            _onDataSaved = onDataSaved;
            
            _quickAddList = new ObservableCollection<BanViewModel>();
            DgThemNhanh.ItemsSource = _quickAddList;
            
            this.Loaded += ThemBanExcelWindow_Loaded;
        }

        private async void ThemBanExcelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var khuVucs = await _service.GetLookupAsync("DKHUVUC");
                CboKhuVuc.ItemsSource = khuVucs;

                var nhomHienThi = await _service.GetLookupAsync("DNHOMHIENTHI");
                CboNhomHienThi.ItemsSource = nhomHienThi;

                var loaiPhong = await _service.GetLookupAsync("DLOAIPHONG");
                CboLoaiPhong.ItemsSource = loaiPhong;

                var bangGia = await _service.GetLookupAsync("DBANGGIA");
                CboBangGia.ItemsSource = bangGia;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu danh mục: " + ex.Message);
            }
        }

        private void BtnThemDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtSoDong.Text, out int soDong) && soDong > 0)
            {
                for (int i = 0; i < soDong; i++)
                {
                    _quickAddList.Add(new BanViewModel { Name = "" });
                }
            }
        }

        private void BtnXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DgThemNhanh.SelectedItems.Cast<BanViewModel>().ToList();
            foreach (var item in selectedItems)
            {
                _quickAddList.Remove(item);
            }
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (_quickAddList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để thêm!", "Thông báo");
                return;
            }

            int successCount = 0;
            string dkhuvucId = (ChkKhuVuc.IsChecked == true && CboKhuVuc.SelectedValue != null) ? CboKhuVuc.SelectedValue.ToString() : null;
            string dnhomhienthiId = (ChkNhomHienThi.IsChecked == true && CboNhomHienThi.SelectedValue != null) ? CboNhomHienThi.SelectedValue.ToString() : null;
            string dloaiphongId = (ChkLoaiPhong.IsChecked == true && CboLoaiPhong.SelectedValue != null) ? CboLoaiPhong.SelectedValue.ToString() : null;
            string dbanggiaId = (ChkBangGia.IsChecked == true && CboBangGia.SelectedValue != null) ? CboBangGia.SelectedValue.ToString() : null;
            
            decimal donGia = (ChkDonGia.IsChecked == true && decimal.TryParse(TxtDonGia.Text, out var dg)) ? dg : 0;
            decimal tienMoBan = (ChkTienMoBan.IsChecked == true && decimal.TryParse(TxtTienMoBan.Text, out var tm)) ? tm : 0;
            string ghiChu = ChkGhiChu.IsChecked == true ? TxtGhiChu.Text : "";

            var dbKhuVucs = await _service.GetLookupAsync("DKHUVUC");
            var dbNhomHienThi = await _service.GetLookupAsync("DNHOMHIENTHI");
            var dbLoaiPhong = await _service.GetLookupAsync("DLOAIPHONG");
            var dbBangGia = await _service.GetLookupAsync("DBANGGIA");

            foreach (var item in _quickAddList)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue; // Skip empty rows

                string itemKvId = dkhuvucId;
                if (!string.IsNullOrWhiteSpace(item.KhuVucName))
                {
                    var found = dbKhuVucs.FirstOrDefault(x => x.Name != null && x.Name.Trim().Equals(item.KhuVucName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (found != null) 
                    {
                        itemKvId = found.Id.ToString();
                    }
                    else
                    {
                        // Khu vực không tồn tại trong CSDL -> Bỏ qua không thêm
                        continue;
                    }
                }
                else if (string.IsNullOrEmpty(itemKvId))
                {
                    // Không có khu vực hợp lệ -> Bỏ qua
                    continue;
                }

                string itemNhId = dnhomhienthiId;
                if (!string.IsNullOrWhiteSpace(item.NhomHienThiName))
                {
                    var found = dbNhomHienThi.FirstOrDefault(x => x.Name != null && x.Name.Trim().Equals(item.NhomHienThiName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemNhId = found.Id.ToString();
                }

                string itemLpId = dloaiphongId;
                if (!string.IsNullOrWhiteSpace(item.LoaiPhongName))
                {
                    var found = dbLoaiPhong.FirstOrDefault(x => x.Name != null && x.Name.Trim().Equals(item.LoaiPhongName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemLpId = found.Id.ToString();
                }

                string itemBgId = dbanggiaId;
                if (!string.IsNullOrWhiteSpace(item.BanggiaName))
                {
                    var found = dbBangGia.FirstOrDefault(x => x.Name != null && x.Name.Trim().Equals(item.BanggiaName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemBgId = found.Id.ToString();
                }

                var ban = new DBAN
                {
                    Name = item.Name,
                    DkhuvucId = itemKvId,
                    DnhomhienthiId = itemNhId,
                    DloaiphongId = itemLpId,
                    DbanggiaId = itemBgId,
                    Dongia = item.Dongia ?? donGia,
                    Tienmoban = item.Tienmoban ?? tienMoBan,
                    Note = string.IsNullOrWhiteSpace(item.Note) ? ghiChu : item.Note,
                    Status = true
                };

                bool result = await _service.InsertBanAsync(ban);
                if (result) successCount++;
            }

            MessageBox.Show($"Thêm thành công {successCount}/{_quickAddList.Count} bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            
            if (successCount > 0)
            {
                _onDataSaved?.Invoke();
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void BtnDanDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Clipboard.ContainsText())
                {
                    MessageBox.Show("Không có dữ liệu văn bản trong clipboard để dán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string clipText = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(clipText)) return;

                var dbKhuVucs = await _service.GetLookupAsync("DKHUVUC");
                var lines = clipText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                int addedCount = 0;
                int skippedCount = 0;

                // Lấy danh sách headers hiện có trên lưới
                var columns = DgThemNhanh.Columns.Select(c => c.Header?.ToString()).ToList();

                foreach (var line in lines)
                {
                    var cols = line.Split('\t');
                    if (cols.Length == 0) continue;

                    var ban = new BanViewModel { Name = "" };

                    for (int i = 0; i < cols.Length && i < columns.Count; i++)
                    {
                        string header = columns[i];
                        string val = cols[i]?.Trim() ?? "";

                        if (header == "Tên bàn") ban.Name = val;
                        else if (header == "Khu vực") ban.KhuVucName = val;
                        else if (header == "Nhóm hiển thị") ban.NhomHienThiName = val;
                        else if (header == "Loại phòng") ban.LoaiPhongName = val;
                        else if (header == "Ghi chú") ban.Note = val;
                        else if (header == "Bảng giá") ban.BanggiaName = val;
                        else if (header == "Đơn giá" && decimal.TryParse(val, out decimal dg)) ban.Dongia = dg;
                        else if (header == "Tiền mở bàn" && decimal.TryParse(val, out decimal tm)) ban.Tienmoban = tm;
                    }

                    if (string.IsNullOrWhiteSpace(ban.Name)) continue;

                    // Kiểm tra khu vực nếu có cột Khu vực
                    if (!string.IsNullOrWhiteSpace(ban.KhuVucName))
                    {
                        bool exists = dbKhuVucs.Any(k => k.Name != null && k.Name.Trim().Equals(ban.KhuVucName.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (!exists)
                        {
                            // Khu vực không tồn tại trong CSDL -> Bỏ qua không thêm
                            skippedCount++;
                            continue;
                        }
                    }

                    _quickAddList.Add(ban);
                    addedCount++;
                }

                if (skippedCount > 0)
                {
                    MessageBox.Show($"Đã dán {addedCount} bàn. Bỏ qua {skippedCount} bàn do khu vực không tồn tại trong CSDL.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi dán dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnXoaBanLoi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dbKhuVucs = await _service.GetLookupAsync("DKHUVUC");
                var invalidList = new List<BanViewModel>();

                foreach (var item in _quickAddList)
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        invalidList.Add(item);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(item.KhuVucName))
                    {
                        bool exists = dbKhuVucs.Any(k => k.Name != null && k.Name.Trim().Equals(item.KhuVucName.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (!exists)
                        {
                            invalidList.Add(item);
                            continue;
                        }
                    }
                }

                foreach (var item in invalidList)
                {
                    _quickAddList.Remove(item);
                }

                MessageBox.Show($"Đã xóa {invalidList.Count} dòng bàn lỗi (thiếu tên hoặc khu vực không tồn tại trong CSDL).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa bàn lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonFileExcelWindow();
            if (win.ShowDialog() == true && win.SelectedFilePaths != null && win.SelectedFilePaths.Length > 0)
            {
                string firstFile = win.SelectedFilePaths[0];
                var actualColumns = new System.Collections.Generic.List<string>();
                System.Data.DataTable dataTable = null;

                try
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = System.IO.File.Open(firstFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataReader.ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataReader.ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = true
                                }
                            });
                            dataTable = result.Tables[0];
                            foreach (System.Data.DataColumn col in dataTable.Columns)
                            {
                                actualColumns.Add(col.ColumnName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Custom system fields cho Bàn
                var banSystemFields = new List<string> {
                    "",
                    "Tên bàn",
                    "Khu vực",
                    "Nhóm hiển thị",
                    "Loại phòng",
                    "Ghi chú",
                    "Bảng giá",
                    "Đơn giá",
                    "Tiền mở bàn"
                };

                var mappingWin = new MappingExcelWindow(actualColumns, banSystemFields);
                if (mappingWin.ShowDialog() == true)
                {
                    var mappings = mappingWin.MappingList.Where(m => !string.IsNullOrEmpty(m.MappedField)).ToList();
                    var dbKhuVucs = await _service.GetLookupAsync("DKHUVUC");

                    int addedCount = 0;
                    int skippedCount = 0;

                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        var ban = new BanViewModel { Name = "" };
                        
                        foreach (var map in mappings)
                        {
                            string val = row[map.ExcelColumn]?.ToString()?.Trim() ?? "";
                            if (map.MappedField == "Tên bàn") ban.Name = val;
                            if (map.MappedField == "Khu vực") ban.KhuVucName = val;
                            if (map.MappedField == "Nhóm hiển thị") ban.NhomHienThiName = val;
                            if (map.MappedField == "Loại phòng") ban.LoaiPhongName = val;
                            if (map.MappedField == "Ghi chú") ban.Note = val;
                            if (map.MappedField == "Bảng giá") ban.BanggiaName = val;
                            if (map.MappedField == "Đơn giá" && decimal.TryParse(val, out decimal dg)) ban.Dongia = dg;
                            if (map.MappedField == "Tiền mở bàn" && decimal.TryParse(val, out decimal tmb)) ban.Tienmoban = tmb;
                        }
                        
                        if (string.IsNullOrWhiteSpace(ban.Name)) continue;

                        // Kiểm tra nếu khu vực không có trong CSDL thì không thêm hàng đó
                        if (!string.IsNullOrWhiteSpace(ban.KhuVucName))
                        {
                            bool exists = dbKhuVucs.Any(k => k.Name != null && k.Name.Trim().Equals(ban.KhuVucName.Trim(), StringComparison.OrdinalIgnoreCase));
                            if (!exists)
                            {
                                skippedCount++;
                                continue; // Bỏ qua không thêm hàng này vào bảng
                            }
                        }

                        _quickAddList.Add(ban);
                        addedCount++;
                    }

                    if (skippedCount > 0)
                    {
                        MessageBox.Show($"Đã nhập {addedCount} bàn từ Excel. Bỏ qua {skippedCount} hàng do Khu vực không tồn tại trong CSDL.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void ChkColumn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (DgThemNhanh == null) return;
            
            var chk = sender as System.Windows.Controls.CheckBox;
            if (chk == null || string.IsNullOrEmpty(chk.Name)) return;

            string header = chk.Name switch
            {
                "ChkKhuVuc" => "Khu vực",
                "ChkNhomHienThi" => "Nhóm hiển thị",
                "ChkLoaiPhong" => "Loại phòng",
                "ChkGhiChu" => "Ghi chú",
                "ChkBangGia" => "Bảng giá",
                "ChkDonGia" => "Đơn giá",
                "ChkTienMoBan" => "Tiền mở bàn",
                _ => null
            };

            string propertyName = chk.Name switch
            {
                "ChkKhuVuc" => "KhuVucName",
                "ChkNhomHienThi" => "NhomHienThiName",
                "ChkLoaiPhong" => "LoaiPhongName",
                "ChkGhiChu" => "Note",
                "ChkBangGia" => "BanggiaName",
                "ChkDonGia" => "Dongia",
                "ChkTienMoBan" => "Tienmoban",
                _ => null
            };

            if (header == null || propertyName == null) return;

            var existingCol = DgThemNhanh.Columns.FirstOrDefault(c => c.Header?.ToString() == header);

            if (chk.IsChecked == true)
            {
                // Remove column if it exists (so the common properties are used)
                if (existingCol != null)
                {
                    DgThemNhanh.Columns.Remove(existingCol);
                }
            }
            else
            {
                // Add column if it doesn't exist (so user can type into DataGrid)
                if (existingCol == null)
                {
                    DgThemNhanh.Columns.Add(new System.Windows.Controls.DataGridTextColumn 
                    { 
                        Header = header, 
                        Binding = new System.Windows.Data.Binding(propertyName)
                    });
                }
            }
        }
    }
}




