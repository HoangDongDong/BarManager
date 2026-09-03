using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly List<HoaDonViewModel> _hoaDonList;
        private readonly IEnumerable<object> _rawData;
        private readonly List<InLuoiWindow.ColumnInfo> _columns;
        private readonly string _reportTitle;
        private readonly string _note;
        private readonly string _templateType;
        private readonly string _storeName;
        private readonly DateTime _tuNgay;
        private readonly DateTime _denNgay;
        private readonly bool _inSTT;

        public PrintPreviewWindow(
            IEnumerable<object> data,
            string templateType = "Báo cáo tổng hợp thanh toán A4",
            string storeName = "NÀNG HƯƠNG QUÁN",
            DateTime? tuNgay = null,
            DateTime? denNgay = null)
        {
            InitializeComponent();

            _rawData = data;
            _templateType = templateType;
            _storeName = string.IsNullOrEmpty(storeName) ? "NÀNG HƯƠNG QUÁN" : storeName;
            _tuNgay = tuNgay ?? DateTime.Today;
            _denNgay = denNgay ?? DateTime.Today;

            _hoaDonList = new List<HoaDonViewModel>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item is HoaDonViewModel hd)
                    {
                        _hoaDonList.Add(hd);
                    }
                }
            }

            BuildReportView();
        }

        // Constructor gọi từ InLuoiWindow
        public PrintPreviewWindow(IEnumerable<object> data, List<InLuoiWindow.ColumnInfo> columns, string title, string note, string templateType, bool inSTT)
        {
            InitializeComponent();

            _rawData = data;
            _columns = columns?.Where(c => c.IsChecked).ToList();
            _reportTitle = string.IsNullOrWhiteSpace(title) ? "BÁO CÁO TỔNG HỢP" : title;
            _note = note;
            _templateType = string.IsNullOrWhiteSpace(templateType) ? "Mẫu A4 thẳng đứng" : templateType;
            _inSTT = inSTT;
            _storeName = "NÀNG HƯƠNG QUÁN";
            _tuNgay = DateTime.Today;
            _denNgay = DateTime.Today;

            _hoaDonList = new List<HoaDonViewModel>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item is HoaDonViewModel hd) _hoaDonList.Add(hd);
                }
            }

            if (_columns != null && _columns.Count > 0)
            {
                BuildGenericReportView();
            }
            else
            {
                BuildReportView();
            }
        }

        private void BuildGenericReportView()
        {
            TxtTenCuaHang.Text = _storeName.ToUpper();
            TxtTieuDeBaoCao.Text = _reportTitle.ToUpper();

            if (!string.IsNullOrWhiteSpace(_note))
            {
                TxtKhoangThoiGian.Text = _note;
                TxtKhoangThoiGian.Visibility = Visibility.Visible;
            }
            else
            {
                TxtKhoangThoiGian.Visibility = Visibility.Collapsed;
            }

            TxtNgayLap.Text = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";

            // Khổ in
            int totalCols = _columns.Count + (_inSTT ? 1 : 0);
            if (_templateType.Contains("80", StringComparison.OrdinalIgnoreCase) || _templateType.Contains("58", StringComparison.OrdinalIgnoreCase))
            {
                PrintContentPanel.Width = 340;
                PaperContainer.Padding = new Thickness(10);
            }
            else if (_templateType.Contains("ngang", StringComparison.OrdinalIgnoreCase))
            {
                PrintContentPanel.Width = Math.Max(980, totalCols * 85);
                PaperContainer.Padding = new Thickness(25);
            }
            else
            {
                PrintContentPanel.Width = Math.Max(780, totalCols * 75);
                PaperContainer.Padding = new Thickness(20);
            }

            GridTable.Children.Clear();
            GridTable.ColumnDefinitions.Clear();
            GridTable.RowDefinitions.Clear();

            int colIdx = 0;
            if (_inSTT)
            {
                GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
                colIdx++;
            }

            foreach (var c in _columns)
            {
                string hNorm = RemoveDiacritics(c.Header).ToLowerInvariant();
                double weight = 1.0;
                if (hNorm.Contains("diachi")) weight = 1.8;
                else if (hNorm.Contains("tenkhach") || hNorm.Contains("khachhang")) weight = 1.5;
                else if (hNorm.Contains("email")) weight = 1.4;
                else if (hNorm.Contains("nhom")) weight = 1.3;

                GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(weight, GridUnitType.Star) });
            }

            // Header Row
            GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            colIdx = 0;
            if (_inSTT)
            {
                AddTableCell(0, colIdx++, "STT", FontWeights.Bold, HorizontalAlignment.Center);
            }
            foreach (var c in _columns)
            {
                AddTableCell(0, colIdx++, c.Header, FontWeights.Bold, HorizontalAlignment.Center);
            }

            if (_rawData == null) return;

            int rowIndex = 1;
            foreach (var obj in _rawData)
            {
                GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                colIdx = 0;

                if (_inSTT)
                {
                    AddTableCell(rowIndex, colIdx++, rowIndex.ToString(), FontWeights.Normal, HorizontalAlignment.Center);
                }

                foreach (var c in _columns)
                {
                    string val = GetObjectPropertyValue(obj, c.Header);
                    HorizontalAlignment align = HorizontalAlignment.Left;
                    if (decimal.TryParse(val.Replace(",", "").Replace(".", ""), out _) && !val.StartsWith("0") && val.Length > 0 && !val.Contains("@"))
                    {
                        align = HorizontalAlignment.Right;
                    }
                    AddTableCell(rowIndex, colIdx++, val, FontWeights.Normal, align, 4);
                }

                rowIndex++;
            }
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace("đ", "d").Replace("Đ", "D");
        }

        private string GetObjectPropertyValue(object obj, string header)
        {
            if (obj == null) return "";

            string hRaw = header?.Trim() ?? "";
            string hNorm = RemoveDiacritics(hRaw).ToLowerInvariant().Replace(" ", "").Replace("/", "").Replace("-", "").Replace("_", "");

            if (obj is KhachHangViewModel kh)
            {
                if (hNorm.Contains("makhach") || hNorm == "ma") return kh.Makhach ?? "";
                if (hNorm.Contains("tenkhach") || hNorm.Contains("khachhang") || hNorm == "ten") return kh.Name ?? "";
                if (hNorm.Contains("diachi")) return kh.Diachi ?? "";
                if (hNorm.Contains("dienthoai") || hNorm.Contains("sdt") || hNorm.Contains("phone")) return kh.Dienthoai ?? "";
                if (hNorm.Contains("email")) return kh.Email ?? "";
                if (hNorm.Contains("nhom")) return kh.TenNhomKhachHang ?? "";
                if (hNorm.Contains("masothue") || hNorm.Contains("mst")) return kh.Masothue ?? "";
                if (hNorm.Contains("nhanvien")) return kh.TenNhanVien ?? "";
                if (hNorm.Contains("tinh")) return kh.TinhThanh ?? "";
                if (hNorm.Contains("facebook") || hNorm == "fb") return kh.Facebook ?? "";
                if (hNorm.Contains("the")) return kh.TheTraTruoc ?? "";
                if (hNorm.Contains("ghichu") || hNorm == "note") return kh.Note ?? "";
                if (hNorm.Contains("diem")) return kh.Diemtichluy.ToString("N0");
                if (hNorm.Contains("ngaysinh") || hNorm.Contains("thanhlap") || hNorm.Contains("sinhnhat")) return kh.Ngaysinh?.ToString("dd/MM/yyyy") ?? "";
            }

            if (obj is NhaCungCapItem ncc)
            {
                if (hNorm.Contains("manhacungcap") || hNorm == "ma" || hNorm.Contains("mancc")) return ncc.MaNhaCungCap ?? "";
                if (hNorm.Contains("tennhacungcap") || hNorm.Contains("nhacungcap") || hNorm == "ten") return ncc.Name ?? "";
                if (hNorm.Contains("diachi")) return ncc.DiaChi ?? "";
                if (hNorm.Contains("dienthoai") || hNorm.Contains("sdt") || hNorm.Contains("phone")) return ncc.DienThoai ?? "";
                if (hNorm.Contains("email")) return ncc.Email ?? "";
                if (hNorm.Contains("website") || hNorm == "web") return ncc.Website ?? "";
                if (hNorm.Contains("nhom")) return ncc.TenNhom ?? "";
                if (hNorm.Contains("ghichu") || hNorm == "note") return ncc.Note ?? "";
            }

            if (obj is PhieuNhapItem pn)
            {
                if (hNorm.Contains("sophieu") || hNorm == "ma") return pn.SoPhieu ?? "";
                if (hNorm.Contains("ngay")) return pn.NgayHienThi ?? "";
                if (hNorm.Contains("nhacungcap") || hNorm.Contains("ncc")) return pn.TenNhaCungCap ?? "";
                if (hNorm.Contains("kho")) return pn.TenKhoNhap ?? "";
                if (hNorm.Contains("nhanvien") || hNorm.Contains("nv")) return pn.TenNhanVienNhap ?? "";
                if (hNorm.Contains("tienhang")) return pn.TienHang.ToString("N0");
                if (hNorm.Contains("tiengiamgia") || (hNorm.Contains("giamgia") && !hNorm.Contains("ti"))) return pn.TienGiamGia.ToString("N0");
                if (hNorm.Contains("tilegiamgia") || hNorm.Contains("tile") || hNorm.Contains("%")) return pn.TiLeGiamGia.ToString("N1") + "%";
                if (hNorm.Contains("taikhoan") || hNorm.Contains("nganhang")) return pn.TenTaiKhoanNganHang ?? "";
                if (hNorm.Contains("tongcong") || hNorm.Contains("tong")) return pn.TongCong.ToString("N0");
                if (hNorm.Contains("cuahang")) return pn.TenCuaHang ?? "";
                if (hNorm.Contains("ghichu") || hNorm == "note") return pn.Note ?? "";
            }

            if (obj is PhieuNhapChiTietItem pndt)
            {
                if (hNorm.Contains("mahang") || hNorm == "ma") return pndt.MaHang ?? "";
                if (hNorm.Contains("tenhang") || hNorm == "ten") return pndt.TenHang ?? "";
                if (hNorm.Contains("donvitinh") || hNorm == "dvt") return pndt.TenDonViTinh ?? "";
                if (hNorm.Contains("soluong") || hNorm == "sl") return pndt.SlNhap.ToString("N2");
                if (hNorm.Contains("dongia") || hNorm == "gia") return pndt.DonGia.ToString("N0");
                if (hNorm.Contains("giamgia")) return pndt.TienGiamGia.ToString("N0");
                if (hNorm.Contains("thanhtien")) return pndt.ThanhTien.ToString("N0");
                if (hNorm.Contains("ghichu") || hNorm == "note") return pndt.Note ?? "";
            }

            // Reflection fallback
            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                string pNorm = RemoveDiacritics(prop.Name).ToLowerInvariant().Replace(" ", "").Replace("_", "");
                if (pNorm == hNorm || prop.Name.Equals(header, StringComparison.OrdinalIgnoreCase))
                {
                    var val = prop.GetValue(obj);
                    if (val is DateTime dt) return dt.ToString("dd/MM/yyyy");
                    if (val is decimal dec) return dec.ToString("N0");
                    return val?.ToString() ?? "";
                }
            }

            return "";
        }

        private void BuildReportView()
        {
            TxtTenCuaHang.Text = _storeName.ToUpper();
            
            if (_templateType.Contains("bán hàng", StringComparison.OrdinalIgnoreCase))
            {
                TxtTieuDeBaoCao.Text = "BÁO CÁO TỔNG HỢP BÁN HÀNG";
            }
            else if (_templateType.Contains("thanh toán", StringComparison.OrdinalIgnoreCase))
            {
                TxtTieuDeBaoCao.Text = "BÁO CÁO TỔNG HỢP THANH TOÁN";
            }
            else
            {
                TxtTieuDeBaoCao.Text = $"BÁO CÁO TỔNG HỢP ({_templateType})";
            }

            TxtKhoangThoiGian.Text = $"Từ ngày {_tuNgay:dd/MM/yyyy} đến ngày {_denNgay:dd/MM/yyyy}";
            TxtNgayLap.Text = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";

            // Định dạng kích thước khổ in
            if (_templateType.StartsWith("58", StringComparison.OrdinalIgnoreCase))
            {
                PrintContentPanel.Width = 240;
                PaperContainer.Padding = new Thickness(10);
            }
            else if (_templateType.StartsWith("80", StringComparison.OrdinalIgnoreCase))
            {
                PrintContentPanel.Width = 320;
                PaperContainer.Padding = new Thickness(12);
            }
            else
            {
                PrintContentPanel.Width = 520;
                PaperContainer.Padding = new Thickness(25);
            }

            // Xây dựng bảng dữ liệu
            GridTable.Children.Clear();
            GridTable.ColumnDefinitions.Clear();
            GridTable.RowDefinitions.Clear();

            // Cột: TT (40), Số phiếu (100), Giờ (70), Tổng cộng (110)
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            // Hàng Tiêu đề Cột (Header)
            GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddTableCell(0, 0, "TT", FontWeights.Bold, HorizontalAlignment.Center);
            AddTableCell(0, 1, "Số phiếu", FontWeights.Bold, HorizontalAlignment.Center);
            AddTableCell(0, 2, "Giờ", FontWeights.Bold, HorizontalAlignment.Center);
            AddTableCell(0, 3, "Tổng cộng", FontWeights.Bold, HorizontalAlignment.Center);

            int rowIndex = 1;
            decimal tongCongTatCa = 0;

            foreach (var item in _hoaDonList)
            {
                GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                string sttStr = rowIndex.ToString();
                string soPhieu = item.SoPhieu ?? "";
                string gioStr = item.GioThanhToan?.ToString("HH:mm") ?? item.KetThuc?.ToString("HH:mm") ?? item.BatDau?.ToString("HH:mm") ?? "";
                string tongCongStr = item.TongCong.ToString("N0");
                tongCongTatCa += item.TongCong;

                AddTableCell(rowIndex, 0, sttStr, FontWeights.Normal, HorizontalAlignment.Center);
                AddTableCell(rowIndex, 1, soPhieu, FontWeights.Normal, HorizontalAlignment.Center);
                AddTableCell(rowIndex, 2, gioStr, FontWeights.Normal, HorizontalAlignment.Center);
                AddTableCell(rowIndex, 3, tongCongStr, FontWeights.Normal, HorizontalAlignment.Right, 6);

                rowIndex++;
            }

            // Hàng Tổng Cộng cuối cùng
            GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Cell Tổng cộng span 3 cột đầu
            var totalHeaderCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 4, 6, 4)
            };
            var totalHeaderTxt = new TextBlock
            {
                Text = "TỔNG CỘNG",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            totalHeaderCell.Child = totalHeaderTxt;
            Grid.SetRow(totalHeaderCell, rowIndex);
            Grid.SetColumn(totalHeaderCell, 0);
            Grid.SetColumnSpan(totalHeaderCell, 3);
            GridTable.Children.Add(totalHeaderCell);

            // Cell Giá trị Tổng cộng
            var totalValCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 4, 6, 4)
            };
            var totalValTxt = new TextBlock
            {
                Text = tongCongTatCa.ToString("N0"),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            totalValCell.Child = totalValTxt;
            Grid.SetRow(totalValCell, rowIndex);
            Grid.SetColumn(totalValCell, 3);
            GridTable.Children.Add(totalValCell);
        }

        private void AddTableCell(int row, int col, string text, FontWeight weight, HorizontalAlignment align, double rightPadding = 0)
        {
            var cell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 3, Math.Max(4, rightPadding), 3)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = weight,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11
            };
            cell.Child = tb;
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            GridTable.Children.Add(cell);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(PaperContainer, TxtTieuDeBaoCao.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"{TxtTieuDeBaoCao.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("TT,Số phiếu,Giờ,Tổng cộng");
                    int stt = 1;
                    foreach (var hd in _hoaDonList)
                    {
                        string gio = hd.GioThanhToan?.ToString("HH:mm") ?? hd.KetThuc?.ToString("HH:mm") ?? hd.BatDau?.ToString("HH:mm") ?? "";
                        sb.AppendLine($"\"{stt++}\",\"{hd.SoPhieu}\",\"{gio}\",\"{hd.TongCong}\"");
                    }
                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
