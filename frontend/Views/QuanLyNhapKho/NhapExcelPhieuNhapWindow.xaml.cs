using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.QuanLyNhapKho
{
    public class NhapExcelPhieuNhapRowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _stt;
        public int Stt
        {
            get => _stt;
            set { if (_stt != value) { _stt = value; OnPropertyChanged(); } }
        }

        private string _maHang = "";
        public string MaHang
        {
            get => _maHang;
            set { if (_maHang != value) { _maHang = value; OnPropertyChanged(); } }
        }

        private string _tenHang = "";
        public string TenHang
        {
            get => _tenHang;
            set { if (_tenHang != value) { _tenHang = value; OnPropertyChanged(); } }
        }

        private string _tenDonViTinh = "";
        public string TenDonViTinh
        {
            get => _tenDonViTinh;
            set { if (_tenDonViTinh != value) { _tenDonViTinh = value; OnPropertyChanged(); } }
        }

        private decimal _soLuong = 1;
        public decimal SoLuong
        {
            get => _soLuong;
            set
            {
                if (_soLuong != value)
                {
                    _soLuong = value;
                    Recalculate();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _donGia;
        public decimal DonGia
        {
            get => _donGia;
            set
            {
                if (_donGia != value)
                {
                    _donGia = value;
                    Recalculate();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _tiLeGiamGia;
        public decimal TiLeGiamGia
        {
            get => _tiLeGiamGia;
            set
            {
                if (_tiLeGiamGia != value)
                {
                    _tiLeGiamGia = value;
                    Recalculate();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _tienGiamGia;
        public decimal TienGiamGia
        {
            get => _tienGiamGia;
            set { if (_tienGiamGia != value) { _tienGiamGia = value; OnPropertyChanged(); } }
        }

        private decimal _thanhTien;
        public decimal ThanhTien
        {
            get => _thanhTien;
            set { if (_thanhTien != value) { _thanhTien = value; OnPropertyChanged(); } }
        }

        private string _ghiChu = "";
        public string GhiChu
        {
            get => _ghiChu;
            set { if (_ghiChu != value) { _ghiChu = value; OnPropertyChanged(); } }
        }

        public string DmathangId { get; set; } = "";
        public string DdonvitinhId { get; set; } = "";

        public void Recalculate()
        {
            decimal tienGoc = _soLuong * _donGia;
            if (_tiLeGiamGia > 0)
            {
                TienGiamGia = tienGoc * (_tiLeGiamGia / 100m);
            }
            else
            {
                TienGiamGia = 0;
            }
            ThanhTien = tienGoc - TienGiamGia;
        }
    }

    public partial class NhapExcelPhieuNhapWindow : Window
    {
        private ObservableCollection<NhapExcelPhieuNhapRowModel> _rows = new();
        private List<MatHangNhapKhoItem> _allMatHang = new();
        public List<PhieuNhapChiTietItem> ImportedItems { get; private set; } = new();

        public NhapExcelPhieuNhapWindow(List<MatHangNhapKhoItem> allMatHang = null)
        {
            InitializeComponent();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _allMatHang = allMatHang ?? new List<MatHangNhapKhoItem>();
            DgDuLieu.ItemsSource = _rows;
            _rows.CollectionChanged += (s, e) =>
            {
                ReindexRows();
                UpdateSummary();
            };

            Loaded += NhapExcelPhieuNhapWindow_Loaded;
        }

        private async void NhapExcelPhieuNhapWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_allMatHang.Count == 0)
            {
                _allMatHang = await LocalNhapKhoService.GetMatHangForNhapKhoAsync();
            }

            if (_rows.Count == 0)
            {
                _rows.Add(new NhapExcelPhieuNhapRowModel { Stt = 1, SoLuong = 1 });
            }
        }

        private void ReindexRows()
        {
            int stt = 1;
            foreach (var r in _rows) r.Stt = stt++;
        }

        private void UpdateSummary()
        {
            TxtTongSoDong.Text = $"Tổng: {_rows.Count(x => !string.IsNullOrWhiteSpace(x.TenHang) || !string.IsNullOrWhiteSpace(x.MaHang))} dòng có dữ liệu / {_rows.Count} dòng";
        }

        private void BtnThemDuLieu_Click(object sender, RoutedEventArgs e)
        {
            int count = 1;
            int.TryParse(TxtSoDong.Text, out count);
            if (count <= 0) count = 1;
            for (int i = 0; i < count; i++)
            {
                _rows.Add(new NhapExcelPhieuNhapRowModel { SoLuong = 1 });
            }
        }

        private void BtnXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (_rows.Count > 0 && MessageBox.Show("Bạn có muốn xóa toàn bộ dữ liệu trên bảng không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _rows.Clear();
            }
        }

        private void BtnDanDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Bộ nhớ tạm (Clipboard) không có dữ liệu để dán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                int countAdded = 0;
                foreach (var line in lines)
                {
                    var parts = line.Split('\t');
                    if (parts.Length == 0) continue;

                    var row = new NhapExcelPhieuNhapRowModel();
                    if (parts.Length >= 1) row.MaHang = parts[0].Trim();
                    if (parts.Length >= 2) row.TenHang = parts[1].Trim();
                    if (parts.Length >= 3) row.TenDonViTinh = parts[2].Trim();
                    if (parts.Length >= 4)
                    {
                        decimal.TryParse(parts[3].Replace(",", "").Replace(".", "").Trim(), out decimal sl);
                        row.SoLuong = sl > 0 ? sl : 1;
                    }
                    if (parts.Length >= 5)
                    {
                        decimal.TryParse(parts[4].Replace(",", "").Replace(".", "").Trim(), out decimal dg);
                        row.DonGia = dg;
                    }
                    if (parts.Length >= 6)
                    {
                        decimal.TryParse(parts[5].Replace(",", "").Replace(".", "").Trim(), out decimal ck);
                        row.TiLeGiamGia = ck;
                    }
                    if (parts.Length >= 7) row.GhiChu = parts[6].Trim();

                    // If parts[0] looked like name instead of code
                    if (string.IsNullOrWhiteSpace(row.TenHang) && !string.IsNullOrWhiteSpace(row.MaHang))
                    {
                        var match = _allMatHang.FirstOrDefault(m => m.Code.Equals(row.MaHang, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            row.TenHang = row.MaHang;
                            row.MaHang = "";
                        }
                    }

                    row.Recalculate();
                    _rows.Add(row);
                    countAdded++;
                }

                MessageBox.Show($"Đã dán thành công {countAdded} dòng dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi dán dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "File Excel (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*",
                Title = "Chọn file Excel danh sách mặt hàng nhập kho"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });

                    if (result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("File Excel không có dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dt = result.Tables[0];
                    int colMaHang = -1, colTenHang = -1, colDvt = -1, colSoLuong = -1, colDonGia = -1, colGiamGia = -1, colGhiChu = -1;

                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        string header = dt.Columns[c].ColumnName.Trim().ToLowerInvariant();
                        if (colMaHang == -1 && (header.Contains("mã") || header.Contains("code"))) colMaHang = c;
                        else if (colTenHang == -1 && (header.Contains("tên") || header.Contains("name") || header.Contains("mặt hàng") || header.Contains("hàng hóa"))) colTenHang = c;
                        else if (colDvt == -1 && (header.Contains("đvt") || header.Contains("đơn vị") || header.Contains("dvt") || header.Contains("unit"))) colDvt = c;
                        else if (colSoLuong == -1 && (header.Contains("số lượng") || header.Contains("sl") || header.Contains("qty") || header.Contains("quantity"))) colSoLuong = c;
                        else if (colDonGia == -1 && (header.Contains("đơn giá") || header.Contains("giá nhập") || header.Contains("giá") || header.Contains("price"))) colDonGia = c;
                        else if (colGiamGia == -1 && (header.Contains("giảm") || header.Contains("chiết khấu") || header.Contains("%") || header.Contains("discount"))) colGiamGia = c;
                        else if (colGhiChu == -1 && (header.Contains("ghi chú") || header.Contains("note") || header.Contains("diễn giải"))) colGhiChu = c;
                    }

                    // Fallback column indexing if headers not detected
                    if (colTenHang == -1 && dt.Columns.Count >= 2) colTenHang = 1;
                    if (colMaHang == -1 && dt.Columns.Count >= 1) colMaHang = 0;
                    if (colDvt == -1 && dt.Columns.Count >= 3) colDvt = 2;
                    if (colSoLuong == -1 && dt.Columns.Count >= 4) colSoLuong = 3;
                    if (colDonGia == -1 && dt.Columns.Count >= 5) colDonGia = 4;

                    _rows.Clear();
                    int count = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        string ten = colTenHang >= 0 ? row[colTenHang]?.ToString()?.Trim() ?? "" : "";
                        string ma = colMaHang >= 0 ? row[colMaHang]?.ToString()?.Trim() ?? "" : "";
                        string dvt = colDvt >= 0 ? row[colDvt]?.ToString()?.Trim() ?? "" : "";
                        string slStr = colSoLuong >= 0 ? row[colSoLuong]?.ToString()?.Trim() ?? "1" : "1";
                        string dgStr = colDonGia >= 0 ? row[colDonGia]?.ToString()?.Trim() ?? "0" : "0";
                        string ggStr = colGiamGia >= 0 ? row[colGiamGia]?.ToString()?.Trim() ?? "0" : "0";
                        string note = colGhiChu >= 0 ? row[colGhiChu]?.ToString()?.Trim() ?? "" : "";

                        if (string.IsNullOrWhiteSpace(ten) && string.IsNullOrWhiteSpace(ma)) continue;

                        decimal.TryParse(slStr.Replace(",", "").Replace(".", ""), out decimal sl);
                        decimal.TryParse(dgStr.Replace(",", "").Replace(".", ""), out decimal dg);
                        decimal.TryParse(ggStr.Replace(",", "").Replace(".", ""), out decimal gg);

                        var rModel = new NhapExcelPhieuNhapRowModel
                        {
                            MaHang = ma,
                            TenHang = ten,
                            TenDonViTinh = dvt,
                            SoLuong = sl > 0 ? sl : 1,
                            DonGia = dg,
                            TiLeGiamGia = gg,
                            GhiChu = note
                        };
                        rModel.Recalculate();
                        _rows.Add(rModel);
                        count++;
                    }

                    MessageBox.Show($"Đã đọc thành công {count} mặt hàng từ file Excel!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnTaiFileMau_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "Mau_NhapKho_MatHang.xlsx",
                Title = "Lưu file mẫu Excel nhập kho"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("NhapKho");
                        string[] headers = new[] { "Mã hàng", "Tên mặt hàng", "ĐVT", "Số lượng", "Đơn giá", "Giảm giá %", "Ghi chú" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        // Mẫu vài dòng
                        ws.Cell(2, 1).Value = "HH01";
                        ws.Cell(2, 2).Value = "Aquafina 500ml";
                        ws.Cell(2, 3).Value = "chai";
                        ws.Cell(2, 4).Value = 24;
                        ws.Cell(2, 5).Value = 5000;
                        ws.Cell(2, 6).Value = 0;
                        ws.Cell(2, 7).Value = "Nhập mới";

                        ws.Cell(3, 1).Value = "HH02";
                        ws.Cell(3, 2).Value = "Bia Heineken lon";
                        ws.Cell(3, 3).Value = "thùng";
                        ws.Cell(3, 4).Value = 10;
                        ws.Cell(3, 5).Value = 380000;
                        ws.Cell(3, 6).Value = 0;
                        ws.Cell(3, 7).Value = "";

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(sfd.FileName);
                    }

                    var res = MessageBox.Show("Đã tạo file mẫu Excel thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tạo file mẫu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDongY_Click(object sender, RoutedEventArgs e)
        {
            var validRows = _rows.Where(x => !string.IsNullOrWhiteSpace(x.TenHang) || !string.IsNullOrWhiteSpace(x.MaHang)).ToList();
            if (validRows.Count == 0)
            {
                MessageBox.Show("Chưa có dòng dữ liệu hợp lệ nào để nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ImportedItems.Clear();
            int stt = 1;

            foreach (var r in validRows)
            {
                // Tìm kiếm khớp với mặt hàng trong hệ thống
                MatHangNhapKhoItem matched = null;
                if (!string.IsNullOrWhiteSpace(r.MaHang))
                {
                    matched = _allMatHang.FirstOrDefault(m => m.Code != null && m.Code.Equals(r.MaHang, StringComparison.OrdinalIgnoreCase));
                }
                if (matched == null && !string.IsNullOrWhiteSpace(r.TenHang))
                {
                    matched = _allMatHang.FirstOrDefault(m => m.Name != null && m.Name.Equals(r.TenHang, StringComparison.OrdinalIgnoreCase));
                }

                var item = new PhieuNhapChiTietItem
                {
                    Stt = stt++,
                    Id = Guid.NewGuid().ToString(),
                    DmathangId = matched?.Id ?? Guid.NewGuid().ToString(),
                    MaHang = !string.IsNullOrWhiteSpace(r.MaHang) ? r.MaHang : (matched?.Code ?? ""),
                    TenHang = !string.IsNullOrWhiteSpace(r.TenHang) ? r.TenHang : (matched?.Name ?? ""),
                    DdonvitinhId = matched?.DdonvitinhId ?? "",
                    TenDonViTinh = !string.IsNullOrWhiteSpace(r.TenDonViTinh) ? r.TenDonViTinh : (matched?.TenDonViTinh ?? ""),
                    SlNhap = r.SoLuong > 0 ? r.SoLuong : 1,
                    DonGia = r.DonGia > 0 ? r.DonGia : (matched?.GiaNhap ?? 0),
                    TiLeGiamGia = r.TiLeGiamGia,
                    Note = r.GhiChu
                };
                item.Recalculate();
                ImportedItems.Add(item);
            }

            DialogResult = true;
            Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
