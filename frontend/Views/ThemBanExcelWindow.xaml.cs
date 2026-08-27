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
            int? dkhuvucId = (ChkKhuVuc.IsChecked == true && CboKhuVuc.SelectedValue != null && int.TryParse(CboKhuVuc.SelectedValue.ToString(), out int kv)) ? kv : (int?)null;
            int? dnhomhienthiId = (ChkNhomHienThi.IsChecked == true && CboNhomHienThi.SelectedValue != null && int.TryParse(CboNhomHienThi.SelectedValue.ToString(), out int nh)) ? nh : (int?)null;
            int? dloaiphongId = (ChkLoaiPhong.IsChecked == true && CboLoaiPhong.SelectedValue != null && int.TryParse(CboLoaiPhong.SelectedValue.ToString(), out int lp)) ? lp : (int?)null;
            int? dbanggiaId = (ChkBangGia.IsChecked == true && CboBangGia.SelectedValue != null && int.TryParse(CboBangGia.SelectedValue.ToString(), out int bg)) ? bg : (int?)null;
            
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

                int? itemKvId = dkhuvucId;
                if (!string.IsNullOrWhiteSpace(item.KhuVucName))
                {
                    var found = dbKhuVucs.FirstOrDefault(x => x.Name.Equals(item.KhuVucName, StringComparison.OrdinalIgnoreCase));
                    if (found != null && int.TryParse(found.Id.ToString(), out int parsed)) itemKvId = parsed;
                }

                int? itemNhId = dnhomhienthiId;
                if (!string.IsNullOrWhiteSpace(item.NhomHienThiName))
                {
                    var found = dbNhomHienThi.FirstOrDefault(x => x.Name.Equals(item.NhomHienThiName, StringComparison.OrdinalIgnoreCase));
                    if (found != null && int.TryParse(found.Id.ToString(), out int parsed)) itemNhId = parsed;
                }

                int? itemLpId = dloaiphongId;
                if (!string.IsNullOrWhiteSpace(item.LoaiPhongName))
                {
                    var found = dbLoaiPhong.FirstOrDefault(x => x.Name.Equals(item.LoaiPhongName, StringComparison.OrdinalIgnoreCase));
                    if (found != null && int.TryParse(found.Id.ToString(), out int parsed)) itemLpId = parsed;
                }

                int? itemBgId = dbanggiaId;
                if (!string.IsNullOrWhiteSpace(item.BanggiaName))
                {
                    var found = dbBangGia.FirstOrDefault(x => x.Name.Equals(item.BanggiaName, StringComparison.OrdinalIgnoreCase));
                    if (found != null && int.TryParse(found.Id.ToString(), out int parsed)) itemBgId = parsed;
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

        private void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
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

                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        var ban = new BanViewModel { Name = "" };
                        
                        foreach (var map in mappings)
                        {
                            string val = row[map.ExcelColumn]?.ToString() ?? "";
                            if (map.MappedField == "Tên bàn") ban.Name = val;
                            if (map.MappedField == "Khu vực") ban.KhuVucName = val;
                            if (map.MappedField == "Nhóm hiển thị") ban.NhomHienThiName = val;
                            if (map.MappedField == "Loại phòng") ban.LoaiPhongName = val;
                            if (map.MappedField == "Ghi chú") ban.Note = val;
                            if (map.MappedField == "Bảng giá") ban.BanggiaName = val;
                            if (map.MappedField == "Đơn giá" && decimal.TryParse(val, out decimal dg)) ban.Dongia = dg;
                            if (map.MappedField == "Tiền mở bàn" && decimal.TryParse(val, out decimal tmb)) ban.Tienmoban = tmb;
                        }
                        
                        _quickAddList.Add(ban);
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
