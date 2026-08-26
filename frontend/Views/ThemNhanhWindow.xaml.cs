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
    public partial class ThemNhanhWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private ObservableCollection<MatHangViewModel> _quickAddList;
        private Action _onDataSaved;

        public ThemNhanhWindow(Action onDataSaved = null)
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _onDataSaved = onDataSaved;
            
            _quickAddList = new ObservableCollection<MatHangViewModel>();
            DgThemNhanh.ItemsSource = _quickAddList;
            
            this.Loaded += ThemNhanhWindow_Loaded;
        }

        private async void ThemNhanhWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var nhomList = await _matHangService.GetNhomMatHangTreeAsync();
                var flatNhomList = nhomList.SelectMany(x => x.Children.Count > 0 ? x.Children : new ObservableCollection<NhomMatHangViewModel> { x }).ToList();
                CboNhomMatHang.ItemsSource = flatNhomList;

                var dvtList = await _matHangService.GetDonViTinhListAsync();
                CboDonViTinh.ItemsSource = dvtList;
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
                    _quickAddList.Add(new MatHangViewModel { Name = "", Code = "" });
                }
            }
        }

        private void BtnXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DgThemNhanh.SelectedItems.Cast<MatHangViewModel>().ToList();
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
            string dnhommathangId = CboNhomMatHang.SelectedValue?.ToString();
            string ddonvitinhId = CboDonViTinh.SelectedValue?.ToString();
            decimal giaBan = decimal.TryParse(TxtGiaBan.Text, out var gb) ? gb : 0;
            decimal giaNhap = decimal.TryParse(TxtGiaNhap.Text, out var gn) ? gn : 0;

            var dbNhomMatHangs = await _matHangService.GetNhomMatHangListAsync();
            var dbLoaiMatHangs = await _matHangService.GetLoaiMatHangListAsync();
            var dbDonViTinhs = await _matHangService.GetDonViTinhListAsync();

            foreach (var item in _quickAddList)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue; // Skip empty rows

                string itemNhomId = dnhommathangId;
                if (!string.IsNullOrWhiteSpace(item.NhomMatHangName))
                {
                    var found = dbNhomMatHangs.FirstOrDefault(x => x.Name.Equals(item.NhomMatHangName, StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemNhomId = found.Id;
                }

                string itemLoaiId = null;
                if (!string.IsNullOrWhiteSpace(item.LoaiMatHangName))
                {
                    var found = dbLoaiMatHangs.FirstOrDefault(x => x.Name.Equals(item.LoaiMatHangName, StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemLoaiId = found.Id.ToString();
                }

                string itemDvtId = ddonvitinhId;
                if (!string.IsNullOrWhiteSpace(item.DonViTinhName))
                {
                    var found = dbDonViTinhs.FirstOrDefault(x => x.Name.Equals(item.DonViTinhName, StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemDvtId = found.Id;
                }

                string itemDvtChanId = null;
                if (!string.IsNullOrWhiteSpace(item.DonViTinhChanName))
                {
                    var found = dbDonViTinhs.FirstOrDefault(x => x.Name.Equals(item.DonViTinhChanName, StringComparison.OrdinalIgnoreCase));
                    if (found != null) itemDvtChanId = found.Id;
                }

                var matHang = new MatHangViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = string.IsNullOrWhiteSpace(item.Code) ? "" : item.Code,
                    Name = item.Name,
                    Giaban = item.Giaban ?? giaBan,
                    Gianhap = item.Gianhap ?? giaNhap,
                    Quydoi = string.IsNullOrWhiteSpace(item.Quydoi) ? "1" : item.Quydoi,
                    Giatheothoigia = item.Giatheothoigia ?? (ChkGiaTheoThoiGiaVal.IsChecked == true ? 1 : 0),
                    DnhommathangId = itemNhomId,
                    DloaimathangId = itemLoaiId,
                    DdonvitinhId = itemDvtId,
                    DdonvitinhchanId = itemDvtChanId,
                    Tamkhoa = string.IsNullOrWhiteSpace(item.Tamkhoa) ? (ChkTamKhoaVal.IsChecked == true ? "1" : "0") : item.Tamkhoa,
                    Ghichu = item.Ghichu,
                    Tontoithieu = item.Tontoithieu,
                    Tontoida = item.Tontoida,
                    Anh = item.Anh,
                    Hoahong = item.Hoahong,
                    Giavon = item.Giavon,
                    Doitackygui = item.Doitackygui,
                    Macdinhgiamgia = item.Macdinhgiamgia,
                    Macdinhgiamtien = item.Macdinhgiamtien,
                    Giabanchan = item.Giabanchan
                };

                bool result = await _matHangService.InsertMatHangAsync(matHang);
                if (result) successCount++;
            }

            MessageBox.Show($"Thêm thành công {successCount}/{_quickAddList.Count} mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            
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

                var mappingWin = new MappingExcelWindow(actualColumns);
                if (mappingWin.ShowDialog() == true)
                {
                    // Lấy cấu hình mapping
                    var mappings = mappingWin.MappingList.Where(m => !string.IsNullOrEmpty(m.MappedField)).ToList();

                    // Thêm dữ liệu từ excel vào lưới
                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        var matHang = new MatHangViewModel { Name = "", Code = "" };
                        
                        foreach (var map in mappings)
                        {
                            string val = row[map.ExcelColumn]?.ToString() ?? "";
                            if (map.MappedField == "Tên mặt hàng") matHang.Name = val;
                            if (map.MappedField == "Mã hàng") matHang.Code = val;
                            if (map.MappedField == "Nhóm mặt hàng") matHang.NhomMatHangName = val;
                            if (map.MappedField == "Loại mặt hàng") matHang.LoaiMatHangName = val;
                            if (map.MappedField == "Đơn vị tính") matHang.DonViTinhName = val;
                            if (map.MappedField == "Giá bán" && decimal.TryParse(val, out decimal gb)) matHang.Giaban = gb;
                            if (map.MappedField == "Giá nhập" && decimal.TryParse(val, out decimal gn)) matHang.Gianhap = gn;
                            if (map.MappedField == "Đơn vị tính chẵn") matHang.DonViTinhChanName = val;
                            if (map.MappedField == "Quy đổi") matHang.Quydoi = val;
                            if (map.MappedField == "Giá bán chẵn" && decimal.TryParse(val, out decimal gbc)) matHang.Giabanchan = gbc;
                            if (map.MappedField == "Tạm khóa") matHang.Tamkhoa = val;
                            if (map.MappedField == "Giá theo thời giá" && decimal.TryParse(val, out decimal gtt)) matHang.Giatheothoigia = gtt;
                            if (map.MappedField == "Ghi chú") matHang.Ghichu = val;
                            if (map.MappedField == "Tồn tối thiểu" && decimal.TryParse(val, out decimal ttt)) matHang.Tontoithieu = ttt;
                            if (map.MappedField == "Tồn tối đa" && decimal.TryParse(val, out decimal ttd)) matHang.Tontoida = ttd;
                            if (map.MappedField == "Ảnh") matHang.Anh = val;
                            if (map.MappedField == "Hoa hồng" && decimal.TryParse(val, out decimal hh)) matHang.Hoahong = hh;
                            if (map.MappedField == "Giá vốn" && decimal.TryParse(val, out decimal gv)) matHang.Giavon = gv;
                            if (map.MappedField == "Đối tác ký gửi") matHang.Doitackygui = val;
                            if (map.MappedField == "Mặc định giảm giá" && decimal.TryParse(val, out decimal mdgg)) matHang.Macdinhgiamgia = mdgg;
                            if (map.MappedField == "Mặc định giảm tiền" && decimal.TryParse(val, out decimal mdgt)) matHang.Macdinhgiamtien = mdgt;
                        }
                        
                        
                        _quickAddList.Add(matHang);
                    }

                    // Checking missing categories
                    var nhomMatHangs = _quickAddList.Select(x => x.NhomMatHangName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    var loaiMatHangs = _quickAddList.Select(x => x.LoaiMatHangName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    var donViTinhs = _quickAddList.Select(x => x.DonViTinhName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

                    var matHangService = new LocalMatHangService();
                    var dbNhomMatHangs = await matHangService.GetNhomMatHangListAsync();
                    var dbLoaiMatHangs = await matHangService.GetLoaiMatHangListAsync();
                    var dbDonViTinhs = await matHangService.GetDonViTinhListAsync();

                    foreach (var nhom in nhomMatHangs)
                    {
                        if (!dbNhomMatHangs.Any(x => x.Name.Equals(nhom, StringComparison.OrdinalIgnoreCase)))
                        {
                            var result = MessageBox.Show($"Nhóm mặt hàng '{nhom}' chưa tồn tại. Bạn có muốn thêm mới không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                var addNhomWin = new ThemNhomWindow(false, nhom);
                                addNhomWin.ShowDialog();
                            }
                        }
                    }

                    foreach (var loai in loaiMatHangs)
                    {
                        if (!dbLoaiMatHangs.Any(x => x.Name.Equals(loai, StringComparison.OrdinalIgnoreCase)))
                        {
                            var result = MessageBox.Show($"Loại mặt hàng '{loai}' chưa tồn tại. Bạn có muốn thêm mới không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                var addLoaiWin = new ThemLoaiMatHangWindow(null, loai);
                                addLoaiWin.ShowDialog();
                            }
                        }
                    }

                    foreach (var dvt in donViTinhs)
                    {
                        if (!dbDonViTinhs.Any(x => x.Name.Equals(dvt, StringComparison.OrdinalIgnoreCase)))
                        {
                            var result = MessageBox.Show($"Đơn vị tính '{dvt}' chưa tồn tại. Bạn có muốn thêm mới không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                var addDvtWin = new ThemDonViTinhWindow(null, dvt);
                                addDvtWin.ShowDialog();
                            }
                        }
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
                "ChkNhomMatHang" => "Nhóm mặt hàng",
                "ChkLoaiMatHang" => "Loại mặt hàng",
                "ChkDonViTinh" => "Đơn vị tính",
                "ChkGiaBan" => "Giá bán",
                "ChkGiaNhap" => "Giá nhập",
                "ChkDonViTinhChan" => "Đơn vị tính chẵn",
                "ChkQuyDoi" => "Quy đổi",
                "ChkGiaBanChan" => "Giá bán chẵn",
                "ChkTmKhoa" => "Tạm khóa",
                "ChkGiaTheoThoi" => "Giá theo thời giá",
                "ChkGhiChu" => "Ghi chú",
                "ChkTToiThieu" => "Tồn tối thiểu",
                "ChkTToiDa" => "Tồn tối đa",
                "ChkAnh" => "Ảnh",
                "ChkHoaHong" => "Hoa hồng",
                "ChkGiaVon" => "Giá vốn",
                _ => null
            };

            string propertyName = chk.Name switch
            {
                "ChkNhomMatHang" => "NhomMatHangName",
                "ChkLoaiMatHang" => "LoaiMatHangName",
                "ChkDonViTinh" => "DonViTinhName",
                "ChkGiaBan" => "Giaban",
                "ChkGiaNhap" => "Gianhap",
                "ChkDonViTinhChan" => "DonViTinhChanName",
                "ChkQuyDoi" => "Quydoi",
                "ChkGiaBanChan" => "Giabanchan",
                "ChkTmKhoa" => "Tamkhoa",
                "ChkGiaTheoThoi" => "Giatheothoigia",
                "ChkGhiChu" => "Ghichu",
                "ChkTToiThieu" => "Tontoithieu",
                "ChkTToiDa" => "Tontoida",
                "ChkAnh" => "Anh",
                "ChkHoaHong" => "Hoahong",
                "ChkGiaVon" => "Giavon",
                _ => null
            };

            if (header == null || propertyName == null) return;

            // Find existing column
            var existingCol = DgThemNhanh.Columns.FirstOrDefault(c => c.Header?.ToString() == header);

            if (chk.IsChecked == true)
            {
                // Remove column if it exists
                if (existingCol != null)
                {
                    DgThemNhanh.Columns.Remove(existingCol);
                }
            }
            else
            {
                // Add column if it doesn't exist
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
