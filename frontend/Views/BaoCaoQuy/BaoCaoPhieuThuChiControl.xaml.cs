using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoQuy
{
    public class ComboLookupItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public partial class BaoCaoPhieuThuChiControl : UserControl
    {
        private readonly string _reportType;
        private readonly bool _isThu;
        private readonly bool _isByReason;
        private bool _isLoaded = false;
        private List<PhieuThuChiGridItem> _rawItems = new List<PhieuThuChiGridItem>();

        public BaoCaoPhieuThuChiControl(string reportType = "DANH SÁCH PHIẾU THU THEO NGÀY")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "DANH SÁCH PHIẾU THU THEO NGÀY" : reportType.Trim().ToUpper();

            _isThu = !_reportType.Contains("CHI") || _reportType.Contains("THU");
            _isByReason = _reportType.Contains("LÝ DO") || _reportType.Contains("LY DO");

            TxtReportTitle.Text = _reportType;

            // Keyboard Shortcuts: F5 (Refresh), F3 (Search), Ctrl+P (Print), F12 (Excel)
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F5)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
                else if (e.Key == Key.F3)
                {
                    TxtSearch.Focus();
                    TxtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.F12)
                {
                    ExportToCsv();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
                {
                    PrintReport();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
            };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            // Mặc định từ ngày đầu tháng đến cuối tháng hiện tại
            var now = DateTime.Now;
            DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
            DpDenNgay.SelectedDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAndLogoAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                string cName = configs.TryGetValue("CompanyName", out var cn) && !string.IsNullOrWhiteSpace(cn) ? cn : "NÀNG HƯƠNG QUÁN";
                string cAddr = configs.TryGetValue("CompanyAddress", out var ca) && !string.IsNullOrWhiteSpace(ca) ? ca : "Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội";
                string cPhone = configs.TryGetValue("CompanyPhone", out var cp) && !string.IsNullOrWhiteSpace(cp) ? cp : "Điện thoại: 0909090880";
                string cEmail = configs.TryGetValue("CompanyEmail", out var ce) ? ce : "";

                TxtCompanyName.Text = cName;
                TxtCompanyAddress.Text = $"Địa chỉ: {cAddr}";
                TxtCompanyContact.Text = $"{cPhone}, Email: {cEmail}";

                var logoBytes = await LocalCauHinhService.LoadCompanyLogoAsync();
                if (logoBytes != null && logoBytes.Length > 0)
                {
                    var bi = new BitmapImage();
                    using (var ms = new MemoryStream(logoBytes))
                    {
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;
                        bi.EndInit();
                    }
                    bi.Freeze();
                    ImgLogo.Source = bi;
                    ImgLogo.Visibility = Visibility.Visible;
                    VbDefaultLogo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImgLogo.Visibility = Visibility.Collapsed;
                    VbDefaultLogo.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                // 1. Lý do thu / chi
                var listLyDo = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };

                var rawLyDo = _isThu 
                    ? await LocalPhieuThuChiService.GetLyDoThuLookupAsync() 
                    : await LocalPhieuThuChiService.GetLyDoChiLookupAsync();

                foreach (var item in rawLyDo)
                {
                    var dict = item as IDictionary<string, object>;
                    string id = dict != null && dict.ContainsKey("ID") ? dict["ID"]?.ToString() ?? "" : (item?.ID?.ToString() ?? "");
                    string name = dict != null && dict.ContainsKey("NAME") ? dict["NAME"]?.ToString() ?? "" : (item?.NAME?.ToString() ?? "");
                    if (!string.IsNullOrEmpty(name))
                    {
                        listLyDo.Add(new ComboLookupItem { Id = id, Name = name, Icon = "👤" });
                    }
                }
                CboLyDo.ItemsSource = listLyDo;
                CboLyDo.SelectedIndex = 0;

                // 2. Cửa hàng
                var listCuaHang = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };

                var rawCuaHang = await LocalPhieuThuChiService.GetCuaHangLookupAsync();
                foreach (var item in rawCuaHang)
                {
                    var dict = item as IDictionary<string, object>;
                    string id = dict != null && dict.ContainsKey("ID") ? dict["ID"]?.ToString() ?? "" : (item?.ID?.ToString() ?? "");
                    string name = dict != null && dict.ContainsKey("NAME") ? dict["NAME"]?.ToString() ?? "" : (item?.NAME?.ToString() ?? "");
                    if (!string.IsNullOrEmpty(name))
                    {
                        string icon = name.Contains("TRỤ SỞ") ? "🖥️" : "🟢";
                        listCuaHang.Add(new ComboLookupItem { Id = id, Name = name, Icon = icon });
                    }
                }
                CboCuaHang.ItemsSource = listCuaHang;
                CboCuaHang.SelectedIndex = 0;
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (!_isLoaded) return;

            try
            {
                DateTime? fromDate = DpTuNgay.SelectedDate;
                DateTime? toDate = DpDenNgay.SelectedDate;

                string lyDoId = (CboLyDo.SelectedItem as ComboLookupItem)?.Id;
                string cuaHangId = (CboCuaHang.SelectedItem as ComboLookupItem)?.Id;

                _rawItems = await LocalPhieuThuChiService.GetDanhSachPhieuThuChiAsync(
                    isThu: _isThu,
                    fromDate: fromDate,
                    toDate: toDate,
                    cuaHangId: string.IsNullOrEmpty(cuaHangId) ? null : cuaHangId,
                    lyDoId: string.IsNullOrEmpty(lyDoId) ? null : lyDoId
                );

                UpdateReportSubHeader();
                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReportSubHeader()
        {
            string fDate = DpTuNgay.SelectedDate?.ToString("dd/MM/yyyy") ?? "--";
            string tDate = DpDenNgay.SelectedDate?.ToString("dd/MM/yyyy") ?? "--";
            TxtDateRange.Text = $"Ngày từ {fDate} đến {tDate}";

            string lyDoName = (CboLyDo.SelectedItem as ComboLookupItem)?.Name;
            if (!string.IsNullOrEmpty(lyDoName))
            {
                TxtLyDoDisplay.Text = $"Lý do thu chi: {lyDoName}";
                TxtLyDoDisplay.Visibility = Visibility.Visible;
            }
            else if (_isByReason)
            {
                TxtLyDoDisplay.Text = "Lý do thu chi: Tất cả";
                TxtLyDoDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                TxtLyDoDisplay.Visibility = Visibility.Collapsed;
            }

            string cuaHangName = (CboCuaHang.SelectedItem as ComboLookupItem)?.Name;
            if (!string.IsNullOrEmpty(cuaHangName))
            {
                TxtCuaHangDisplay.Text = $"Cửa hàng: {cuaHangName}";
                TxtCuaHangDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                TxtCuaHangDisplay.Visibility = Visibility.Collapsed;
            }
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.TenDoiTuong?.ToLower().Contains(search) == true) ||
                (x.DienGiai?.ToLower().Contains(search) == true) ||
                (x.LyDoThuChi?.ToLower().Contains(search) == true) ||
                (x.CuaHang?.ToLower().Contains(search) == true)
            ).ToList();

            string moneyColumnHeader = _isThu ? "Số tiền thu" : "Số tiền chi";

            // Tạo khung bảng tổng thể viền đen 1px
            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            if (!_isByReason)
            {
                // === BÁO CÁO THEO NGÀY ===
                // Columns: STT(40), Số phiếu(100), Tên đối tượng(170), Diễn giải(Star), Số tiền(125)

                // 1. Dòng Tiêu Đề Bảng (Header)
                stackTable.Children.Add(CreateRowTheoNgay(
                    stt: "STT",
                    soPhieu: "Số phiếu",
                    tenDoiTuong: "Tên đối tượng",
                    dienGiai: "Diễn giải",
                    soTien: moneyColumnHeader,
                    isHeader: true,
                    isSummary: false
                ));

                decimal totalMoney = 0;
                int globalStt = 1;

                var dateGroups = filtered
                    .GroupBy(x => x.Ngay?.Date ?? DateTime.MinValue)
                    .OrderBy(g => g.Key)
                    .ToList();

                if (dateGroups.Count == 0)
                {
                    // Khi chưa có dữ liệu, hiển thị 1 dòng Ngày: dd/MM/yyyy và dòng TỔNG CỘNG 0
                    string curDateStr = DpTuNgay.SelectedDate?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
                    stackTable.Children.Add(CreateGroupHeaderRowTheoNgay($"Ngày: {curDateStr}"));
                    stackTable.Children.Add(CreateRowTheoNgay("", "", "", "TỔNG CỘNG", "0", isHeader: false, isSummary: true));
                }
                else
                {
                    foreach (var group in dateGroups)
                    {
                        string dateStr = group.Key == DateTime.MinValue ? "Khác" : group.Key.ToString("dd/MM/yyyy");
                        // Dòng Tiêu Đề Nhóm Ngày
                        stackTable.Children.Add(CreateGroupHeaderRowTheoNgay($"Ngày: {dateStr}"));

                        decimal groupSum = 0;
                        foreach (var item in group.OrderBy(x => x.SoPhieu))
                        {
                            totalMoney += item.SoTien;
                            groupSum += item.SoTien;

                            stackTable.Children.Add(CreateRowTheoNgay(
                                stt: globalStt.ToString(),
                                soPhieu: item.SoPhieu ?? "",
                                tenDoiTuong: item.TenDoiTuong ?? "",
                                dienGiai: item.DienGiai ?? "",
                                soTien: item.SoTien.ToString("N0"),
                                isHeader: false,
                                isSummary: false
                            ));
                            globalStt++;
                        }

                        // Dòng Tổng cộng của ngày (chữ thường, nền trắng)
                        stackTable.Children.Add(CreateRowTheoNgay(
                            stt: "",
                            soPhieu: "",
                            tenDoiTuong: "",
                            dienGiai: "Tổng cộng",
                            soTien: groupSum.ToString("N0"),
                            isHeader: false,
                            isSummary: false,
                            isGroupSubtotal: true
                        ));
                    }

                    // Dòng TỔNG CỘNG Cuối Bảng (In hoa, nền xanh xám #9bbfe6)
                    stackTable.Children.Add(CreateRowTheoNgay(
                        stt: "",
                        soPhieu: "",
                        tenDoiTuong: "",
                        dienGiai: "TỔNG CỘNG",
                        soTien: totalMoney.ToString("N0"),
                        isHeader: false,
                        isSummary: true
                    ));
                }
            }
            else
            {
                // === BÁO CÁO THEO LÝ DO THU CHI ===
                // Columns: STT(40), Số phiếu(100), Ngày(85), Tên đối tượng(170), Diễn giải(Star), Số tiền(125)

                stackTable.Children.Add(CreateRowTheoLyDo(
                    stt: "STT",
                    soPhieu: "Số phiếu",
                    ngay: "Ngày",
                    tenDoiTuong: "Tên đối tượng",
                    dienGiai: "Diễn giải",
                    soTien: moneyColumnHeader,
                    isHeader: true,
                    isSummary: false
                ));

                decimal totalMoney = 0;
                int stt = 1;
                foreach (var item in filtered.OrderBy(x => x.LyDoThuChi).ThenBy(x => x.Ngay).ThenBy(x => x.SoPhieu))
                {
                    totalMoney += item.SoTien;
                    string dateStr = item.Ngay?.ToString("dd/MM/yyyy") ?? "";

                    stackTable.Children.Add(CreateRowTheoLyDo(
                        stt: stt.ToString(),
                        soPhieu: item.SoPhieu ?? "",
                        ngay: dateStr,
                        tenDoiTuong: item.TenDoiTuong ?? "",
                        dienGiai: item.DienGiai ?? "",
                        soTien: item.SoTien.ToString("N0"),
                        isHeader: false,
                        isSummary: false
                    ));
                    stt++;
                }

                stackTable.Children.Add(CreateRowTheoLyDo(
                    stt: "",
                    soPhieu: "",
                    ngay: "",
                    tenDoiTuong: "",
                    dienGiai: "TỔNG CỘNG",
                    soTien: totalMoney.ToString("N0"),
                    isHeader: false,
                    isSummary: true
                ));
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateGroupHeaderRowTheoNgay(string title)
        {
            var grid = new Grid { MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(125) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = Brushes.White
            };
            Grid.SetColumnSpan(border, 5);

            var tb = new TextBlock
            {
                Text = title,
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            grid.Children.Add(border);
            return grid;
        }

        private UIElement CreateRowTheoNgay(string stt, string soPhieu, string tenDoiTuong, string dienGiai, string soTien, bool isHeader, bool isSummary, bool isGroupSubtotal = false)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(125) });

            if (isSummary || isGroupSubtotal)
            {
                var summaryLabelCell = CreateTableCell(dienGiai, 0, HorizontalAlignment.Right, isSummary ? FontWeights.Bold : FontWeights.Normal, isHeader: false, isSummary: isSummary);
                Grid.SetColumnSpan(summaryLabelCell, 4);
                grid.Children.Add(summaryLabelCell);

                var summaryValueCell = CreateTableCell(soTien, 4, HorizontalAlignment.Right, isSummary ? FontWeights.Bold : FontWeights.Normal, isHeader: false, isSummary: isSummary);
                grid.Children.Add(summaryValueCell);

                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soPhieu, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tenDoiTuong, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(dienGiai, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soTien, 4, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));

            return grid;
        }

        private UIElement CreateRowTheoLyDo(string stt, string soPhieu, string ngay, string tenDoiTuong, string dienGiai, string soTien, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(125) });

            if (isSummary)
            {
                var summaryLabelCell = CreateTableCell("TỔNG CỘNG", 0, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                Grid.SetColumnSpan(summaryLabelCell, 5);
                grid.Children.Add(summaryLabelCell);

                var summaryValueCell = CreateTableCell(soTien, 5, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryValueCell);

                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soPhieu, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(ngay, 2, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(tenDoiTuong, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(dienGiai, 4, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soTien, 5, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight, bool isHeader, bool isSummary)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 2, 4, 2),
                Background = isSummary 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bbfe6")) 
                    : (isHeader ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9")) : Brushes.White)
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text ?? "",
                FontSize = isHeader ? 10.5 : (isSummary ? 10.5 : 10),
                FontWeight = weight,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            return border;
        }

        #region Event Handlers

        private void Filter_Changed(object sender, EventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderReportTable();
        }

        private void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
            ReportScrollViewer.ScrollToTop();
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }

        private void PrintReport()
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(A4PageBorder, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"{TxtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtDateRange.Text}\" - \"{TxtLyDoDisplay.Text}\" - \"{TxtCuaHangDisplay.Text}\"");
                    sb.AppendLine("");

                    string moneyHeader = _isThu ? "Số tiền thu" : "Số tiền chi";
                    sb.AppendLine($"STT,Số phiếu,Ngày,Tên đối tượng,Diễn giải,{moneyHeader}");

                    int stt = 1;
                    decimal total = 0;
                    foreach (var item in _rawItems)
                    {
                        total += item.SoTien;
                        sb.AppendLine($"\"{stt}\",\"{item.SoPhieu}\",\"{item.Ngay:dd/MM/yyyy}\",\"{item.TenDoiTuong?.Replace("\"", "\"\"")}\",\"{item.DienGiai?.Replace("\"", "\"\"")}\",\"{item.SoTien}\"");
                        stt++;
                    }

                    sb.AppendLine($"\"\",\"\",\"\",\"\",\"TỔNG CỘNG\",\"{total}\"");
                    File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnQuickDate_Click(object sender, RoutedEventArgs e)
        {
            if (BtnQuickDate.ContextMenu != null)
            {
                BtnQuickDate.ContextMenu.PlacementTarget = BtnQuickDate;
                BtnQuickDate.ContextMenu.IsOpen = true;
            }
        }

        private void QuickDate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string tag)
            {
                var now = DateTime.Now;
                switch (tag)
                {
                    case "Today":
                        DpTuNgay.SelectedDate = now.Date;
                        DpDenNgay.SelectedDate = now.Date;
                        break;
                    case "Yesterday":
                        DpTuNgay.SelectedDate = now.Date.AddDays(-1);
                        DpDenNgay.SelectedDate = now.Date.AddDays(-1);
                        break;
                    case "ThisWeek":
                        int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                        DpTuNgay.SelectedDate = now.Date.AddDays(-1 * diff);
                        DpDenNgay.SelectedDate = now.Date.AddDays(-1 * diff + 6);
                        break;
                    case "LastWeek":
                        int diffLast = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                        DpTuNgay.SelectedDate = now.Date.AddDays(-1 * diffLast - 7);
                        DpDenNgay.SelectedDate = now.Date.AddDays(-1 * diffLast - 1);
                        break;
                    case "ThisMonth":
                        DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
                        DpDenNgay.SelectedDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                        break;
                    case "LastMonth":
                        var prevMonth = now.AddMonths(-1);
                        DpTuNgay.SelectedDate = new DateTime(prevMonth.Year, prevMonth.Month, 1);
                        DpDenNgay.SelectedDate = new DateTime(prevMonth.Year, prevMonth.Month, DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));
                        break;
                    case "ThisQuarter":
                        int quarter = (now.Month - 1) / 3 + 1;
                        int startMonth = (quarter - 1) * 3 + 1;
                        DpTuNgay.SelectedDate = new DateTime(now.Year, startMonth, 1);
                        DpDenNgay.SelectedDate = new DateTime(now.Year, startMonth + 2, DateTime.DaysInMonth(now.Year, startMonth + 2));
                        break;
                    case "LastQuarter":
                        int prevQ = (now.Month - 1) / 3;
                        int qYear = now.Year;
                        if (prevQ == 0) { prevQ = 4; qYear--; }
                        int qStartMonth = (prevQ - 1) * 3 + 1;
                        DpTuNgay.SelectedDate = new DateTime(qYear, qStartMonth, 1);
                        DpDenNgay.SelectedDate = new DateTime(qYear, qStartMonth + 2, DateTime.DaysInMonth(qYear, qStartMonth + 2));
                        break;
                    case "ThisYear":
                        DpTuNgay.SelectedDate = new DateTime(now.Year, 1, 1);
                        DpDenNgay.SelectedDate = new DateTime(now.Year, 12, 31);
                        break;
                    case "All":
                        DpTuNgay.SelectedDate = new DateTime(2000, 1, 1);
                        DpDenNgay.SelectedDate = new DateTime(2099, 12, 31);
                        break;
                }
            }
        }

        #endregion
    }
}
