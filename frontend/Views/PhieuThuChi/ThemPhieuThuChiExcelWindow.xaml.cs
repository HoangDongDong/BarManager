using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using ExcelDataReader;

namespace QuanLyBar.Client.Views.PhieuThuChi
{
    public class PhieuThuChiExcelItem : INotifyPropertyChanged
    {
        private string _soPhieu = "";
        private DateTime? _ngay = DateTime.Today;
        private decimal _soTien = 0;
        private string _tenDoiTuong = "";
        private string _diaChi = "";
        private string _lyDo = "";
        private string _dienGiai = "";
        private string _nhanVien = "";
        private string _taiKhoan = "";
        private string _cuaHang = "";
        private string _chungTuGoc = "";
        private string _ghiChu = "";

        public string SoPhieu
        {
            get => _soPhieu;
            set { _soPhieu = value; OnPropertyChanged(); }
        }

        public DateTime? Ngay
        {
            get => _ngay;
            set { _ngay = value; OnPropertyChanged(); }
        }

        public decimal SoTien
        {
            get => _soTien;
            set { _soTien = value; OnPropertyChanged(); }
        }

        public string TenDoiTuong
        {
            get => _tenDoiTuong;
            set { _tenDoiTuong = value; OnPropertyChanged(); }
        }

        public string DiaChi
        {
            get => _diaChi;
            set { _diaChi = value; OnPropertyChanged(); }
        }

        public string LyDo
        {
            get => _lyDo;
            set { _lyDo = value; OnPropertyChanged(); }
        }

        public string DienGiai
        {
            get => _dienGiai;
            set { _dienGiai = value; OnPropertyChanged(); }
        }

        public string NhanVien
        {
            get => _nhanVien;
            set { _nhanVien = value; OnPropertyChanged(); }
        }

        public string TaiKhoan
        {
            get => _taiKhoan;
            set { _taiKhoan = value; OnPropertyChanged(); }
        }

        public string CuaHang
        {
            get => _cuaHang;
            set { _cuaHang = value; OnPropertyChanged(); }
        }

        public string ChungTuGoc
        {
            get => _chungTuGoc;
            set { _chungTuGoc = value; OnPropertyChanged(); }
        }

        public string GhiChu
        {
            get => _ghiChu;
            set { _ghiChu = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public partial class ThemPhieuThuChiExcelWindow : Window
    {
        private readonly bool _isThu;
        private readonly Action _onDataSaved;
        private ObservableCollection<PhieuThuChiExcelItem> _items = new ObservableCollection<PhieuThuChiExcelItem>();

        private List<dynamic> _lyDoList = new List<dynamic>();
        private List<dynamic> _cuaHangList = new List<dynamic>();
        private List<dynamic> _taiKhoanList = new List<dynamic>();
        private List<dynamic> _nhanVienList = new List<dynamic>();

        public ThemPhieuThuChiExcelWindow(bool isThu, Action onDataSaved = null)
        {
            InitializeComponent();
            _isThu = isThu;
            _onDataSaved = onDataSaved;

            Title = _isThu ? "THÊM PHIẾU THU TỪ EXCEL" : "THÊM PHIẾU CHI TỪ EXCEL";
            TxtHeaderTitle.Text = _isThu ? "Thêm nhanh phiếu thu từ Excel" : "Thêm nhanh phiếu chi từ Excel";

            _items.CollectionChanged += (s, e) => UpdateSummary();
            DgThemNhanh.ItemsSource = _items;

            Loaded += async (s, e) => await LoadLookupsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                DpNgay.SelectedDate = DateTime.Today;

                _lyDoList = _isThu ? await LocalPhieuThuChiService.GetLyDoThuLookupAsync() : await LocalPhieuThuChiService.GetLyDoChiLookupAsync();
                CboLyDo.ItemsSource = _lyDoList;
                if (_lyDoList.Count > 0) CboLyDo.SelectedIndex = 0;

                _cuaHangList = await LocalPhieuThuChiService.GetCuaHangLookupAsync();
                CboCuaHang.ItemsSource = _cuaHangList;
                if (_cuaHangList.Count > 0) CboCuaHang.SelectedIndex = 0;

                _taiKhoanList = await LocalPhieuThuChiService.GetTaiKhoanNganHangLookupAsync();
                CboTaiKhoan.ItemsSource = _taiKhoanList;
                if (_taiKhoanList.Count > 0) CboTaiKhoan.SelectedIndex = 0;

                _nhanVienList = await LocalPhieuThuChiService.GetNhanVienLookupAsync();
                CboNhanVien.ItemsSource = _nhanVienList;
                if (_nhanVienList.Count > 0) CboNhanVien.SelectedIndex = 0;

                // Thêm sẵn 5 dòng mẫu
                BtnThemDong_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummary()
        {
            int count = _items.Count;
            decimal total = _items.Sum(x => x.SoTien);
            TxtSummary.Text = $"Tổng số dòng: {count} | Tổng tiền: {total:N0} VNĐ";
        }

        private void BtnThemDong_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(TxtSoDong.Text?.Trim(), out int count);
            if (count <= 0) count = 1;

            string defaultLyDo = (ChkLyDo.IsChecked == true && CboLyDo.SelectedItem != null) ? (CboLyDo.SelectedItem as dynamic)?.NAME?.ToString() : "";
            string defaultCuaHang = (ChkCuaHang.IsChecked == true && CboCuaHang.SelectedItem != null) ? (CboCuaHang.SelectedItem as dynamic)?.NAME?.ToString() : "";
            string defaultTaiKhoan = (ChkTaiKhoan.IsChecked == true && CboTaiKhoan.SelectedItem != null) ? (CboTaiKhoan.SelectedItem as dynamic)?.NAME?.ToString() : "";
            string defaultNhanVien = (ChkNhanVien.IsChecked == true && CboNhanVien.SelectedItem != null) ? (CboNhanVien.SelectedItem as dynamic)?.NAME?.ToString() : "";
            DateTime? defaultNgay = (ChkNgay.IsChecked == true && DpNgay.SelectedDate.HasValue) ? DpNgay.SelectedDate.Value : DateTime.Today;

            for (int i = 0; i < count; i++)
            {
                _items.Add(new PhieuThuChiExcelItem
                {
                    Ngay = defaultNgay,
                    LyDo = defaultLyDo,
                    CuaHang = defaultCuaHang,
                    TaiKhoan = defaultTaiKhoan,
                    NhanVien = defaultNhanVien
                });
            }
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgThemNhanh.SelectedItems.Cast<PhieuThuChiExcelItem>().ToList();
            foreach (var item in selected)
            {
                _items.Remove(item);
            }
            UpdateSummary();
        }

        private void BtnXoaTatCa_Click(object sender, RoutedEventArgs e)
        {
            _items.Clear();
            UpdateSummary();
        }

        private void BtnChonFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "Excel / CSV Files (*.xlsx;*.xls;*.csv)|*.xlsx;*.xls;*.csv|All files (*.*)|*.*",
                    Title = "Chọn file Excel hoặc CSV để nhập"
                };

                if (ofd.ShowDialog() == true)
                {
                    string ext = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                    if (ext == ".csv")
                    {
                        ReadCsvFile(ofd.FileName);
                    }
                    else
                    {
                        ReadExcelFile(ofd.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chọn file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadCsvFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (lines.Length <= 1)
            {
                MessageBox.Show("File CSV không có dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int addedCount = 0;
            // Bỏ qua dòng tiêu đề
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i]?.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = ParseCsvLine(line);
                if (cols.Count == 0) continue;

                var item = new PhieuThuChiExcelItem();
                if (cols.Count > 0) item.SoPhieu = cols[0];
                if (cols.Count > 1 && DateTime.TryParse(cols[1], out var dt)) item.Ngay = dt;
                if (cols.Count > 2) item.TenDoiTuong = cols[2];
                if (cols.Count > 3) item.DiaChi = cols[3];
                if (cols.Count > 4) item.LyDo = cols[4];
                if (cols.Count > 5) item.DienGiai = cols[5];
                if (cols.Count > 6) item.ChungTuGoc = cols[6];
                if (cols.Count > 7 && decimal.TryParse(cols[7].Replace(",", ""), out var tien)) item.SoTien = tien;
                if (cols.Count > 8) item.GhiChu = cols[8];
                if (cols.Count > 9) item.TaiKhoan = cols[9];
                if (cols.Count > 10) item.NhanVien = cols[10];
                if (cols.Count > 11) item.CuaHang = cols[11];

                _items.Add(item);
                addedCount++;
            }

            MessageBox.Show($"Đã đọc thành công {addedCount} dòng từ file CSV!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateSummary();
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var sb = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString().Trim());
            return result;
        }

        private void ReadExcelFile(string filePath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        if (result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                        {
                            MessageBox.Show("File Excel không có dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        var table = result.Tables[0];
                        int addedCount = 0;

                        foreach (System.Data.DataRow row in table.Rows)
                        {
                            var item = new PhieuThuChiExcelItem();

                            // Mapping thông minh theo tên cột hoặc thứ tự
                            for (int c = 0; c < table.Columns.Count; c++)
                            {
                                string colName = table.Columns[c].ColumnName?.Trim().ToLowerInvariant() ?? "";
                                string val = row[c]?.ToString()?.Trim() ?? "";

                                if (colName.Contains("phiếu") || colName.Contains("sophieu") || colName.Contains("mã") || c == 0)
                                {
                                    if (string.IsNullOrEmpty(item.SoPhieu)) item.SoPhieu = val;
                                }
                                if (colName.Contains("ngày") || colName.Contains("ngay") || c == 1)
                                {
                                    if (DateTime.TryParse(val, out var dt)) item.Ngay = dt;
                                }
                                if (colName.Contains("tiền") || colName.Contains("sotien") || colName.Contains("thu") || colName.Contains("chi") || c == 2)
                                {
                                    if (decimal.TryParse(val.Replace(",", "").Replace(".", ""), out var tien)) item.SoTien = tien;
                                }
                                if (colName.Contains("đối tượng") || colName.Contains("khách") || colName.Contains("người") || colName.Contains("tendoituong") || c == 3)
                                {
                                    if (string.IsNullOrEmpty(item.TenDoiTuong)) item.TenDoiTuong = val;
                                }
                                if (colName.Contains("địa chỉ") || colName.Contains("diachi") || c == 4)
                                {
                                    if (string.IsNullOrEmpty(item.DiaChi)) item.DiaChi = val;
                                }
                                if (colName.Contains("lý do") || colName.Contains("lydo") || c == 5)
                                {
                                    if (string.IsNullOrEmpty(item.LyDo)) item.LyDo = val;
                                }
                                if (colName.Contains("diễn giải") || colName.Contains("diengiai") || c == 6)
                                {
                                    if (string.IsNullOrEmpty(item.DienGiai)) item.DienGiai = val;
                                }
                                if (colName.Contains("nhân viên") || colName.Contains("nhanvien") || c == 7)
                                {
                                    if (string.IsNullOrEmpty(item.NhanVien)) item.NhanVien = val;
                                }
                                if (colName.Contains("tài khoản") || colName.Contains("ngân hàng") || c == 8)
                                {
                                    if (string.IsNullOrEmpty(item.TaiKhoan)) item.TaiKhoan = val;
                                }
                                if (colName.Contains("cửa hàng") || colName.Contains("cuahang") || c == 9)
                                {
                                    if (string.IsNullOrEmpty(item.CuaHang)) item.CuaHang = val;
                                }
                                if (colName.Contains("chứng từ gốc") || colName.Contains("chungtugoc") || c == 10)
                                {
                                    if (string.IsNullOrEmpty(item.ChungTuGoc)) item.ChungTuGoc = val;
                                }
                                if (colName.Contains("ghi chú") || colName.Contains("ghichu") || c == 11)
                                {
                                    if (string.IsNullOrEmpty(item.GhiChu)) item.GhiChu = val;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(item.TenDoiTuong) || item.SoTien > 0 || !string.IsNullOrWhiteSpace(item.SoPhieu))
                            {
                                _items.Add(item);
                                addedCount++;
                            }
                        }

                        MessageBox.Show($"Đã đọc thành công {addedCount} dòng từ file Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateSummary();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đọc file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXuatMau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = _isThu ? "MauNhap_PhieuThu.csv" : "MauNhap_PhieuChi.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Số phiếu,Ngày,Số tiền,Tên đối tượng,Địa chỉ,Lý do thu chi,Diễn giải,Nhân viên,Tài khoản NH,Cửa hàng,Chứng từ gốc,Ghi chú");
                        if (_isThu)
                        {
                            writer.WriteLine($"PT{DateTime.Now:yy}/00001,{DateTime.Today:dd/MM/yyyy},1000000,Công ty ABC,Hà Nội,Thu tiền dịch vụ,Thu cọc đặt phòng,Thu ngân 1,Vietcombank,TRỤ SỞ CHÍNH,HĐ001,Khách đặt trước");
                            writer.WriteLine($"PT{DateTime.Now:yy}/00002,{DateTime.Today:dd/MM/yyyy},500000,Nguyễn Văn A,Đà Nẵng,Thu khác,Thu tiền thêm giờ,Thu ngân 2,,TRỤ SỞ CHÍNH,,");
                        }
                        else
                        {
                            writer.WriteLine($"PC{DateTime.Now:yy}/00001,{DateTime.Today:dd/MM/yyyy},500000,Cửa hàng tạp hóa B,Hà Nội,Chi mua nguyên liệu,Mua rau củ,Nhân viên bếp 1,,TRỤ SỞ CHÍNH,BL001,Mua chợ sáng");
                            writer.WriteLine($"PC{DateTime.Now:yy}/00002,{DateTime.Today:dd/MM/yyyy},2000000,Công ty Điện lực,Đà Nẵng,Chi tiền điện nước,Tiền điện tháng này,Kế toán,Vietcombank,TRỤ SỞ CHÍNH,HD999,Chuyển khoản");
                        }
                    }

                    MessageBox.Show("Đã xuất file mẫu Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file mẫu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu nào để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var validItems = _items.Where(x => x.SoTien > 0 || !string.IsNullOrWhiteSpace(x.TenDoiTuong) || !string.IsNullOrWhiteSpace(x.SoPhieu)).ToList();
            if (validItems.Count == 0)
            {
                MessageBox.Show("Các dòng hiện tại chưa có dữ liệu hợp lệ (Số tiền hoặc Đối tượng)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string defaultLyDoId = (ChkLyDo.IsChecked == true && CboLyDo.SelectedValue != null) ? CboLyDo.SelectedValue.ToString() : null;
            string defaultCuaHangId = (ChkCuaHang.IsChecked == true && CboCuaHang.SelectedValue != null) ? CboCuaHang.SelectedValue.ToString() : null;
            string defaultTaiKhoanId = (ChkTaiKhoan.IsChecked == true && CboTaiKhoan.SelectedValue != null) ? CboTaiKhoan.SelectedValue.ToString() : null;
            string defaultNhanVienId = (ChkNhanVien.IsChecked == true && CboNhanVien.SelectedValue != null) ? CboNhanVien.SelectedValue.ToString() : null;
            bool khongThayDoiCongNo = ChkKhongThayDoiCongNo.IsChecked == true;

            int successCount = 0;
            int failCount = 0;
            string lastError = "";

            foreach (var item in validItems)
            {
                try
                {
                    string soPhieu = item.SoPhieu?.Trim();
                    if (string.IsNullOrEmpty(soPhieu))
                    {
                        soPhieu = await LocalPhieuThuChiService.GetNextSoPhieuAsync(_isThu);
                    }

                    DateTime ngay = item.Ngay ?? DateTime.Today;
                    decimal soTien = item.SoTien;
                    string tenDoiTuong = item.TenDoiTuong?.Trim();
                    string diaChi = item.DiaChi?.Trim();
                    string chungTuGoc = item.ChungTuGoc?.Trim();
                    string ghiChu = !string.IsNullOrEmpty(item.GhiChu) ? item.GhiChu : item.DienGiai;

                    // Match lý do id
                    string lyDoId = defaultLyDoId;
                    if (!string.IsNullOrEmpty(item.LyDo))
                    {
                        var found = _lyDoList.FirstOrDefault(x => string.Equals((x as dynamic)?.NAME?.ToString(), item.LyDo.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (found != null) lyDoId = (found as dynamic)?.ID?.ToString();
                    }

                    // Match nhân viên id
                    string nhanVienId = defaultNhanVienId;
                    if (!string.IsNullOrEmpty(item.NhanVien))
                    {
                        var found = _nhanVienList.FirstOrDefault(x => string.Equals((x as dynamic)?.NAME?.ToString(), item.NhanVien.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (found != null) nhanVienId = (found as dynamic)?.ID?.ToString();
                    }

                    // Match tài khoản ngân hàng
                    string taiKhoanId = defaultTaiKhoanId;
                    bool chuyenKhoan = false;
                    if (!string.IsNullOrEmpty(item.TaiKhoan))
                    {
                        var found = _taiKhoanList.FirstOrDefault(x => string.Equals((x as dynamic)?.NAME?.ToString(), item.TaiKhoan.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (found != null)
                        {
                            taiKhoanId = (found as dynamic)?.ID?.ToString();
                            chuyenKhoan = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(defaultTaiKhoanId))
                    {
                        chuyenKhoan = true;
                    }

                    // Match cửa hàng
                    string cuaHangId = defaultCuaHangId;
                    if (!string.IsNullOrEmpty(item.CuaHang))
                    {
                        var found = _cuaHangList.FirstOrDefault(x => string.Equals((x as dynamic)?.NAME?.ToString(), item.CuaHang.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (found != null) cuaHangId = (found as dynamic)?.ID?.ToString();
                    }

                    var saveRes = await LocalPhieuThuChiService.SavePhieuThuChiAsync(
                        id: null,
                        soPhieu: soPhieu,
                        ngay: ngay,
                        isThu: _isThu,
                        soTien: soTien,
                        tenDoiTuong: tenDoiTuong,
                        diaChi: diaChi,
                        loaiDoiTuong: "Khách hàng",
                        chungTuGoc: chungTuGoc,
                        ghiChu: ghiChu,
                        lyDoId: lyDoId,
                        nhanVienId: nhanVienId,
                        khachHangId: null,
                        nhaCungCapId: null,
                        taiKhoanNganHangId: taiKhoanId,
                        cuaHangId: cuaHangId,
                        chuyenKhoan: chuyenKhoan,
                        khongThayDoiCongNo: khongThayDoiCongNo
                    );

                    if (saveRes.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        lastError = saveRes.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    lastError = ex.Message;
                }
            }

            if (successCount > 0)
            {
                _onDataSaved?.Invoke();
                string msg = $"Đã thêm thành công {successCount} phiếu vào hệ thống!";
                if (failCount > 0)
                {
                    msg += $"\n({failCount} phiếu lỗi: {lastError})";
                }
                MessageBox.Show(msg, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show($"Không thể lưu phiếu nào. Chi tiết lỗi: {lastError}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
