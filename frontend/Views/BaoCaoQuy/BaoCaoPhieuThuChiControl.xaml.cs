using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    public enum BaoCaoQuyMode
    {
        PhieuThuTheoNgay,
        PhieuThuTheoLyDo,
        PhieuChiTheoNgay,
        PhieuChiTheoLyDo,
        TongHopThuChiTheoNgay,
        TongHopThuChiTheoLyDo,
        TonQuy
    }

    public class ComboLookupItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public partial class BaoCaoPhieuThuChiControl : UserControl
    {
        private readonly string _rawReportType;
        private readonly BaoCaoQuyMode _mode;
        private bool _isLoaded = false;
        private List<PhieuThuChiGridItem> _rawItems = new List<PhieuThuChiGridItem>();
        private TonQuyResult _tonQuyResult = new TonQuyResult();

        public BaoCaoPhieuThuChiControl(string reportType = "DANH SÁCH PHIẾU THU THEO NGÀY")
        {
            InitializeComponent();
            _rawReportType = string.IsNullOrWhiteSpace(reportType) ? "DANH SÁCH PHIẾU THU THEO NGÀY" : reportType.Trim().ToUpper();

            // Phân loại báo cáo
            if (_rawReportType.Contains("TỒN QUỸ") || _rawReportType.Contains("TON QUY"))
            {
                _mode = BaoCaoQuyMode.TonQuy;
                TxtReportTitle.Text = "BÁO CÁO TỒN QUỸ";
            }
            else if (_rawReportType.Contains("TỔNG HỢP") || _rawReportType.Contains("TONG HOP"))
            {
                if (_rawReportType.Contains("LÝ DO") || _rawReportType.Contains("LY DO"))
                {
                    _mode = BaoCaoQuyMode.TongHopThuChiTheoLyDo;
                    TxtReportTitle.Text = "TỔNG HỢP THU CHI THEO LÝ DO";
                }
                else
                {
                    _mode = BaoCaoQuyMode.TongHopThuChiTheoNgay;
                    TxtReportTitle.Text = "TỔNG HỢP THU CHI THEO NGÀY";
                }
            }
            else if (_rawReportType.Contains("CHI"))
            {
                if (_rawReportType.Contains("LÝ DO") || _rawReportType.Contains("LY DO"))
                {
                    _mode = BaoCaoQuyMode.PhieuChiTheoLyDo;
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHI THEO LÝ DO THU CHI";
                }
                else
                {
                    _mode = BaoCaoQuyMode.PhieuChiTheoNgay;
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHI THEO NGÀY";
                }
            }
            else
            {
                if (_rawReportType.Contains("LÝ DO") || _rawReportType.Contains("LY DO"))
                {
                    _mode = BaoCaoQuyMode.PhieuThuTheoLyDo;
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU THU THEO LÝ DO THU CHI";
                }
                else
                {
                    _mode = BaoCaoQuyMode.PhieuThuTheoNgay;
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU THU THEO NGÀY";
                }
            }

            // Cập nhật giao diện thanh lọc phù hợp với loại báo cáo
            UpdateFilterLabels();

            // Phím tắt: F5 (Refresh), F3 (Search), Ctrl+P (Print), F12 (Excel)
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

        private void UpdateFilterLabels()
        {
            if (_mode == BaoCaoQuyMode.TonQuy)
            {
                TxtNgayPrefix.Visibility = Visibility.Collapsed;
                TxtTuNgayLabel.Text = "Từ ngày";
                TxtDenNgayLabel.Text = "Đến ngày";
                TxtLyDoLabel.Text = "Loại quỹ";
                TxtCuaHangLabel.Text = "Tài khoản";
            }
            else
            {
                TxtNgayPrefix.Visibility = Visibility.Visible;
                TxtTuNgayLabel.Text = "Từ:";
                TxtDenNgayLabel.Text = "Đến";
                TxtLyDoLabel.Text = "Lý do thu chi";
                TxtCuaHangLabel.Text = "Cửa hàng";
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var now = DateTime.Now;
            if (_mode == BaoCaoQuyMode.TonQuy)
            {
                // Báo cáo tồn quỹ thường xem từ ngày 1 đến ngày hiện tại
                DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
                DpDenNgay.SelectedDate = now.Date;
            }
            else
            {
                // Mặc định từ ngày đầu tháng đến cuối tháng hiện tại
                DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
                DpDenNgay.SelectedDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            }

            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAndLogoAsync()
        {
            try
            {
                var comp = await LocalCauHinhService.GetCompanyInfoAsync();
                TxtCompanyName.Text = comp.Name;
                TxtCompanyAddress.Text = comp.FormattedAddress;
                TxtCompanyContact.Text = comp.FormattedContact;

                if (comp.LogoBytes != null && comp.LogoBytes.Length > 0)
                {
                    var bi = LocalCauHinhService.ImageFromBytes(comp.LogoBytes);
                    if (bi != null)
                    {
                        ImgLogo.Source = bi;
                        ImgLogo.Visibility = Visibility.Visible;
                        if (FindName("VbDefaultLogo") is UIElement vb) vb.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                if (_mode == BaoCaoQuyMode.TonQuy)
                {
                    var listQuy = new List<ComboLookupItem>
                    {
                        new ComboLookupItem { Id = "ALL", Name = "Tất cả", Icon = "🌐" },
                        new ComboLookupItem { Id = "TIEN_MAT", Name = "Tiền mặt", Icon = "💵" },
                        new ComboLookupItem { Id = "QUET_THE", Name = "Quẹt thẻ", Icon = "💳" },
                        new ComboLookupItem { Id = "NGAN_HANG", Name = "Ngân hàng", Icon = "🏦" }
                    };
                    CboLyDo.ItemsSource = listQuy;
                    CboLyDo.SelectedIndex = 0;

                    var listTk = new List<ComboLookupItem>
                    {
                        new ComboLookupItem { Id = "", Name = "", Icon = "" }
                    };
                    try
                    {
                        var rawTk = await LocalTaiKhoanNganHangService.GetTaiKhoanNganHangTreeAsync();
                        foreach (var tk in rawTk)
                        {
                            listTk.Add(new ComboLookupItem { Id = tk.Id ?? "", Name = tk.Name ?? "", Icon = "🏦" });
                        }
                    }
                    catch { }
                    CboCuaHang.ItemsSource = listTk;
                    CboCuaHang.SelectedIndex = 0;
                    return;
                }

                // 1. Lý do thu / chi
                var listLyDo = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "", Icon = "" }
                };

                if (_mode == BaoCaoQuyMode.PhieuThuTheoNgay || _mode == BaoCaoQuyMode.PhieuThuTheoLyDo)
                {
                    var rawLyDo = await LocalPhieuThuChiService.GetLyDoThuLookupAsync();
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
                }
                else if (_mode == BaoCaoQuyMode.PhieuChiTheoNgay || _mode == BaoCaoQuyMode.PhieuChiTheoLyDo)
                {
                    var rawLyDo = await LocalPhieuThuChiService.GetLyDoChiLookupAsync();
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
                }
                else
                {
                    // Tổng hợp thu chi: tải tất cả lý do thu & chi
                    var rawLyDoThu = await LocalPhieuThuChiService.GetLyDoThuLookupAsync();
                    var rawLyDoChi = await LocalPhieuThuChiService.GetLyDoChiLookupAsync();
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var item in rawLyDoThu.Concat(rawLyDoChi))
                    {
                        var dict = item as IDictionary<string, object>;
                        string id = dict != null && dict.ContainsKey("ID") ? dict["ID"]?.ToString() ?? "" : (item?.ID?.ToString() ?? "");
                        string name = dict != null && dict.ContainsKey("NAME") ? dict["NAME"]?.ToString() ?? "" : (item?.NAME?.ToString() ?? "");
                        if (!string.IsNullOrEmpty(id) && !seen.Contains(id) && !string.IsNullOrEmpty(name))
                        {
                            seen.Add(id);
                            listLyDo.Add(new ComboLookupItem { Id = id, Name = name, Icon = "👤" });
                        }
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

                if (_mode == BaoCaoQuyMode.TonQuy)
                {
                    string quyCode = (CboLyDo.SelectedItem as ComboLookupItem)?.Id ?? "ALL";
                    string taiKhoanId = (CboCuaHang.SelectedItem as ComboLookupItem)?.Id ?? "";

                    _tonQuyResult = await LocalTonQuyService.GetBaoCaoTonQuyAsync(fromDate, toDate, null, quyCode, taiKhoanId);
                    UpdateReportSubHeader();
                    RenderReportTable();
                    return;
                }

                string lyDoId = (CboLyDo.SelectedItem as ComboLookupItem)?.Id;
                string cuaHangId = (CboCuaHang.SelectedItem as ComboLookupItem)?.Id;

                bool? queryIsThu = null;
                if (_mode == BaoCaoQuyMode.PhieuThuTheoNgay || _mode == BaoCaoQuyMode.PhieuThuTheoLyDo)
                {
                    queryIsThu = true;
                }
                else if (_mode == BaoCaoQuyMode.PhieuChiTheoNgay || _mode == BaoCaoQuyMode.PhieuChiTheoLyDo)
                {
                    queryIsThu = false;
                }
                else
                {
                    queryIsThu = null; // Cả thu và chi
                }

                _rawItems = await LocalPhieuThuChiService.GetDanhSachPhieuThuChiAsync(
                    isThu: queryIsThu,
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

            if (_mode == BaoCaoQuyMode.TonQuy)
            {
                TxtDateRange.Text = $"Từ ngày: {fDate} đến ngày {tDate}";
                string qName = (CboLyDo.SelectedItem as ComboLookupItem)?.Name;
                string tkName = (CboCuaHang.SelectedItem as ComboLookupItem)?.Name;
                if (Utilities.IsSpecificFilter(qName))
                {
                    TxtLyDoDisplay.Text = $"Quỹ tiền: {qName}";
                    TxtLyDoDisplay.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtLyDoDisplay.Visibility = Visibility.Collapsed;
                }

                if (Utilities.IsSpecificFilter(tkName))
                {
                    TxtCuaHangDisplay.Text = $"Tài khoản NH: {tkName}";
                    TxtCuaHangDisplay.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtCuaHangDisplay.Visibility = Visibility.Collapsed;
                }
                return;
            }

            TxtDateRange.Text = $"Ngày từ {fDate} đến {tDate}";

            string lyDoName = (CboLyDo.SelectedItem as ComboLookupItem)?.Name;
            if (Utilities.IsSpecificFilter(lyDoName))
            {
                TxtLyDoDisplay.Text = $"Lý do thu chi: {lyDoName}";
                TxtLyDoDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                TxtLyDoDisplay.Visibility = Visibility.Collapsed;
            }

            string cuaHangName = (CboCuaHang.SelectedItem as ComboLookupItem)?.Name;
            if (Utilities.IsSpecificFilter(cuaHangName))
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

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            switch (_mode)
            {
                case BaoCaoQuyMode.TonQuy:
                    RenderTableTonQuy(stackTable, search);
                    break;

                case BaoCaoQuyMode.PhieuThuTheoNgay:
                    RenderTablePhieuThuChiTheoNgay(stackTable, search, isThu: true);
                    break;

                case BaoCaoQuyMode.PhieuChiTheoNgay:
                    RenderTablePhieuThuChiTheoNgay(stackTable, search, isThu: false);
                    break;

                case BaoCaoQuyMode.PhieuThuTheoLyDo:
                    RenderTablePhieuThuChiTheoLyDo(stackTable, search, isThu: true);
                    break;

                case BaoCaoQuyMode.PhieuChiTheoLyDo:
                    RenderTablePhieuThuChiTheoLyDo(stackTable, search, isThu: false);
                    break;

                case BaoCaoQuyMode.TongHopThuChiTheoNgay:
                    RenderTableTongHopTheoNgay(stackTable, search);
                    break;

                case BaoCaoQuyMode.TongHopThuChiTheoLyDo:
                    RenderTableTongHopTheoLyDo(stackTable, search);
                    break;
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        #region Render Specific Tables

        // 1 & 3: DANH SÁCH PHIẾU THU / CHI THEO NGÀY
        private void RenderTablePhieuThuChiTheoNgay(StackPanel stackTable, string search, bool isThu)
        {
            string moneyColHeader = isThu ? "Số tiền thu" : "Số tiền chi";

            // Header: STT, Số phiếu, Tên đối tượng, Diễn giải, Số tiền
            stackTable.Children.Add(CreateRowTheoNgay("STT", "Số phiếu", "Tên đối tượng", "Diễn giải", moneyColHeader, isHeader: true, isSummary: false));

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.TenDoiTuong?.ToLower().Contains(search) == true) ||
                (x.DienGiai?.ToLower().Contains(search) == true) ||
                (x.LyDoThuChi?.ToLower().Contains(search) == true) ||
                (x.CuaHang?.ToLower().Contains(search) == true)
            ).ToList();

            decimal totalMoney = 0;
            int globalStt = 1;

            var dateGroups = filtered
                .GroupBy(x => x.Ngay?.Date ?? DateTime.MinValue)
                .OrderBy(g => g.Key)
                .ToList();

            if (dateGroups.Count == 0)
            {
                // Khi không có dữ liệu: hiển thị dòng ngày và tổng cộng 0
                string curDateStr = DpTuNgay.SelectedDate?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
                stackTable.Children.Add(CreateGroupHeaderRowTheoNgay($"Ngày: {curDateStr}"));
                stackTable.Children.Add(CreateRowTheoNgay("", "", "", "TỔNG CỘNG", "0", isHeader: false, isSummary: true));
            }
            else
            {
                foreach (var group in dateGroups)
                {
                    string dateStr = group.Key == DateTime.MinValue ? "Khác" : group.Key.ToString("dd/MM/yyyy");
                    stackTable.Children.Add(CreateGroupHeaderRowTheoNgay($"Ngày: {dateStr}"));

                    decimal groupSum = 0;
                    foreach (var item in group.OrderBy(x => x.SoPhieu))
                    {
                        decimal itemAmount = isThu ? (item.Thu > 0 ? item.Thu : item.SoTien) : (item.Chi > 0 ? item.Chi : item.SoTien);
                        totalMoney += itemAmount;
                        groupSum += itemAmount;

                        stackTable.Children.Add(CreateRowTheoNgay(
                            stt: (globalStt++).ToString(),
                            soPhieu: item.SoPhieu ?? "",
                            tenDoiTuong: item.TenDoiTuong ?? "",
                            dienGiai: item.DienGiai ?? "",
                            soTien: itemAmount.ToString("N0"),
                            isHeader: false,
                            isSummary: false
                        ));
                    }

                    // Dòng Tổng cộng của ngày
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

                // Dòng TỔNG CỘNG cuối bảng
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

        // 2 & 4: DANH SÁCH PHIẾU THU / CHI THEO LÝ DO THU CHI
        private void RenderTablePhieuThuChiTheoLyDo(StackPanel stackTable, string search, bool isThu)
        {
            string moneyColHeader = isThu ? "Số tiền thu" : "Số tiền chi";

            // Header: STT, Số phiếu, Ngày, Tên đối tượng, Diễn giải, Số tiền
            stackTable.Children.Add(CreateRowTheoLyDo("STT", "Số phiếu", "Ngày", "Tên đối tượng", "Diễn giải", moneyColHeader, isHeader: true, isSummary: false));

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.TenDoiTuong?.ToLower().Contains(search) == true) ||
                (x.DienGiai?.ToLower().Contains(search) == true) ||
                (x.LyDoThuChi?.ToLower().Contains(search) == true) ||
                (x.CuaHang?.ToLower().Contains(search) == true)
            ).ToList();

            decimal totalMoney = 0;
            int stt = 1;

            foreach (var item in filtered.OrderBy(x => x.LyDoThuChi).ThenBy(x => x.Ngay).ThenBy(x => x.SoPhieu))
            {
                decimal itemAmount = isThu ? (item.Thu > 0 ? item.Thu : item.SoTien) : (item.Chi > 0 ? item.Chi : item.SoTien);
                totalMoney += itemAmount;
                string dateStr = item.Ngay?.ToString("dd/MM/yyyy") ?? "";

                stackTable.Children.Add(CreateRowTheoLyDo(
                    stt: (stt++).ToString(),
                    soPhieu: item.SoPhieu ?? "",
                    ngay: dateStr,
                    tenDoiTuong: item.TenDoiTuong ?? "",
                    dienGiai: item.DienGiai ?? "",
                    soTien: itemAmount.ToString("N0"),
                    isHeader: false,
                    isSummary: false
                ));
            }

            // Dòng TỔNG CỘNG cuối bảng
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

        // 5: TỔNG HỢP THU CHI THEO NGÀY
        private void RenderTableTongHopTheoNgay(StackPanel stackTable, string search)
        {
            // Columns: STT, Ngày, Số tiền chi, Số tiền thu
            stackTable.Children.Add(CreateRowTongHopTheoNgay("STT", "Ngày", "Số tiền chi", "Số tiền thu", isHeader: true, isSummary: false));

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.TenDoiTuong?.ToLower().Contains(search) == true) ||
                (x.DienGiai?.ToLower().Contains(search) == true) ||
                (x.LyDoThuChi?.ToLower().Contains(search) == true)
            ).ToList();

            var dateGroups = filtered
                .GroupBy(x => x.Ngay?.Date ?? DateTime.MinValue)
                .OrderBy(g => g.Key)
                .ToList();

            decimal grandTotalChi = 0;
            decimal grandTotalThu = 0;
            int stt = 1;

            foreach (var g in dateGroups)
            {
                string dateStr = g.Key == DateTime.MinValue ? "Khác" : g.Key.ToString("dd/MM/yyyy");
                decimal sumChi = g.Sum(x => x.Chi);
                decimal sumThu = g.Sum(x => x.Thu);

                grandTotalChi += sumChi;
                grandTotalThu += sumThu;

                stackTable.Children.Add(CreateRowTongHopTheoNgay(
                    stt: (stt++).ToString(),
                    ngay: dateStr,
                    soTienChi: sumChi > 0 ? sumChi.ToString("N0") : "0",
                    soTienThu: sumThu > 0 ? sumThu.ToString("N0") : "0",
                    isHeader: false,
                    isSummary: false
                ));
            }

            stackTable.Children.Add(CreateRowTongHopTheoNgay(
                stt: "",
                ngay: "TỔNG CỘNG",
                soTienChi: grandTotalChi.ToString("N0"),
                soTienThu: grandTotalThu.ToString("N0"),
                isHeader: false,
                isSummary: true
            ));
        }

        // 6: TỔNG HỢP THU CHI THEO LÝ DO
        private void RenderTableTongHopTheoLyDo(StackPanel stackTable, string search)
        {
            // Columns: STT, Lý do thu chi, Số tiền chi, Số tiền thu
            stackTable.Children.Add(CreateRowTongHopTheoLyDo("STT", "Lý do thu chi", "Số tiền chi", "Số tiền thu", isHeader: true, isSummary: false));

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.TenDoiTuong?.ToLower().Contains(search) == true) ||
                (x.DienGiai?.ToLower().Contains(search) == true) ||
                (x.LyDoThuChi?.ToLower().Contains(search) == true)
            ).ToList();

            var lyDoGroups = filtered
                .GroupBy(x => string.IsNullOrWhiteSpace(x.LyDoThuChi) ? "Chưa thiết lập lý do" : x.LyDoThuChi.Trim())
                .OrderBy(g => g.Key)
                .ToList();

            decimal grandTotalChi = 0;
            decimal grandTotalThu = 0;
            int stt = 1;

            foreach (var g in lyDoGroups)
            {
                string lyDoName = g.Key;
                decimal sumChi = g.Sum(x => x.Chi);
                decimal sumThu = g.Sum(x => x.Thu);

                grandTotalChi += sumChi;
                grandTotalThu += sumThu;

                stackTable.Children.Add(CreateRowTongHopTheoLyDo(
                    stt: (stt++).ToString(),
                    lyDo: lyDoName,
                    soTienChi: sumChi > 0 ? sumChi.ToString("N0") : "0",
                    soTienThu: sumThu > 0 ? sumThu.ToString("N0") : "0",
                    isHeader: false,
                    isSummary: false
                ));
            }

            stackTable.Children.Add(CreateRowTongHopTheoLyDo(
                stt: "",
                lyDo: "TỔNG CỘNG",
                soTienChi: grandTotalChi.ToString("N0"),
                soTienThu: grandTotalThu.ToString("N0"),
                isHeader: false,
                isSummary: true
            ));
        }

        // 7: BÁO CÁO TỒN QUỸ
        private void RenderTableTonQuy(StackPanel stackTable, string search)
        {
            // Columns: TT(35), Số phiếu(95), Ngày(85), Diễn giải(Star), Tạo bởi(95), Thời gian(60), Thu(85), Chi(85), Tồn(100)
            stackTable.Children.Add(CreateRowTonQuy("TT", "Số phiếu", "Ngày", "Diễn giải", "Tạo bởi", "Thời gian", "Thu", "Chi", "Tồn", isHeader: true, isSummary: false));

            var list = _tonQuyResult.DanhSachGiaoDich.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.SoPhieu?.ToLower().Contains(search) == true) ||
                (x.DienGiai?.ToLower().Contains(search) == true) ||
                (x.TaoBoi?.ToLower().Contains(search) == true)
            ).ToList();

            int stt = 1;
            foreach (var item in list)
            {
                stackTable.Children.Add(CreateRowTonQuy(
                    stt: (stt++).ToString(),
                    soPhieu: item.SoPhieu ?? "",
                    ngay: item.NgayHienThi,
                    dienGiai: item.DienGiai ?? "",
                    taoBoi: string.IsNullOrEmpty(item.TaoBoi) ? "Administrator" : item.TaoBoi,
                    thoiGian: item.ThoiGian ?? "",
                    thu: item.Thu > 0 ? item.Thu.ToString("N0") : "0",
                    chi: item.Chi > 0 ? item.Chi.ToString("N0") : "0",
                    ton: item.RunningTon.ToString("N0"),
                    isHeader: false,
                    isSummary: false
                ));
            }

            stackTable.Children.Add(CreateRowTonQuy(
                stt: "",
                soPhieu: "",
                ngay: "",
                dienGiai: "TỔNG CỘNG",
                taoBoi: "",
                thoiGian: "",
                thu: _tonQuyResult.TongThu.ToString("N0"),
                chi: _tonQuyResult.TongChi.ToString("N0"),
                ton: _tonQuyResult.TonQuy.ToString("N0"),
                isHeader: false,
                isSummary: true
            ));
        }

        #endregion

        #region Create Grid Rows

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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
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

        private UIElement CreateRowTongHopTheoNgay(string stt, string ngay, string soTienChi, string soTienThu, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (isSummary)
            {
                var summaryLabelCell = CreateTableCell("TỔNG CỘNG", 0, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                Grid.SetColumnSpan(summaryLabelCell, 2);
                grid.Children.Add(summaryLabelCell);

                var summaryChiCell = CreateTableCell(soTienChi, 2, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryChiCell);

                var summaryThuCell = CreateTableCell(soTienThu, 3, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryThuCell);

                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(ngay, 1, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soTienChi, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soTienThu, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));

            return grid;
        }

        private UIElement CreateRowTongHopTheoLyDo(string stt, string lyDo, string soTienChi, string soTienThu, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

            if (isSummary)
            {
                var summaryLabelCell = CreateTableCell("TỔNG CỘNG", 0, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                Grid.SetColumnSpan(summaryLabelCell, 2);
                grid.Children.Add(summaryLabelCell);

                var summaryChiCell = CreateTableCell(soTienChi, 2, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryChiCell);

                var summaryThuCell = CreateTableCell(soTienThu, 3, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryThuCell);

                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(lyDo, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soTienChi, 2, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soTienThu, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));

            return grid;
        }

        private UIElement CreateRowTonQuy(string stt, string soPhieu, string ngay, string dienGiai, string taoBoi, string thoiGian, string thu, string chi, string ton, bool isHeader, bool isSummary)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : (isSummary ? 24 : 21) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });  // TT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });  // Số phiếu
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });  // Ngày
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Diễn giải
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });  // Tạo bởi
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });  // Thời gian
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Thu
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Chi
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });  // Tồn

            if (isSummary)
            {
                var summaryLabelCell = CreateTableCell("TỔNG CỘNG", 0, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                Grid.SetColumnSpan(summaryLabelCell, 6);
                grid.Children.Add(summaryLabelCell);

                var summaryThuCell = CreateTableCell(thu, 6, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryThuCell);

                var summaryChiCell = CreateTableCell(chi, 7, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryChiCell);

                var summaryTonCell = CreateTableCell(ton, 8, HorizontalAlignment.Right, FontWeights.Bold, isHeader: false, isSummary: true);
                grid.Children.Add(summaryTonCell);

                return grid;
            }

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(soPhieu, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(ngay, 2, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(dienGiai, 3, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(taoBoi, 4, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(thoiGian, 5, HorizontalAlignment.Center, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(thu, 6, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(chi, 7, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));
            grid.Children.Add(CreateTableCell(ton, 8, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Right, weight, isHeader, false));

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

        #endregion

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
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtDateRange.Text}\"");
                    sb.AppendLine("");

                    switch (_mode)
                    {
                        case BaoCaoQuyMode.TonQuy:
                            sb.AppendLine("STT,Số phiếu,Ngày,Diễn giải,Tạo bởi,Thời gian,Thu,Chi,Tồn");
                            int sttTq = 1;
                            foreach (var item in _tonQuyResult.DanhSachGiaoDich)
                            {
                                sb.AppendLine($"\"{sttTq++}\",\"{item.SoPhieu}\",\"{item.NgayHienThi}\",\"{item.DienGiai?.Replace("\"", "\"\"")}\",\"{item.TaoBoi}\",\"{item.ThoiGian}\",\"{item.Thu}\",\"{item.Chi}\",\"{item.RunningTon}\"");
                            }
                            sb.AppendLine($"\"\",\"\",\"\",\"\",\"\",\"TỔNG CỘNG\",\"{_tonQuyResult.TongThu}\",\"{_tonQuyResult.TongChi}\",\"{_tonQuyResult.TonQuy}\"");
                            break;

                        case BaoCaoQuyMode.PhieuThuTheoNgay:
                        case BaoCaoQuyMode.PhieuChiTheoNgay:
                            string mCol = (_mode == BaoCaoQuyMode.PhieuThuTheoNgay) ? "Số tiền thu" : "Số tiền chi";
                            sb.AppendLine($"STT,Số phiếu,Tên đối tượng,Diễn giải,{mCol}");
                            int sttNgay = 1;
                            decimal totNgay = 0;
                            foreach (var item in _rawItems)
                            {
                                decimal amt = (_mode == BaoCaoQuyMode.PhieuThuTheoNgay) ? (item.Thu > 0 ? item.Thu : item.SoTien) : (item.Chi > 0 ? item.Chi : item.SoTien);
                                totNgay += amt;
                                sb.AppendLine($"\"{sttNgay++}\",\"{item.SoPhieu}\",\"{item.TenDoiTuong?.Replace("\"", "\"\"")}\",\"{item.DienGiai?.Replace("\"", "\"\"")}\",\"{amt}\"");
                            }
                            sb.AppendLine($"\"\",\"\",\"\",\"TỔNG CỘNG\",\"{totNgay}\"");
                            break;

                        case BaoCaoQuyMode.PhieuThuTheoLyDo:
                        case BaoCaoQuyMode.PhieuChiTheoLyDo:
                            string mColLd = (_mode == BaoCaoQuyMode.PhieuThuTheoLyDo) ? "Số tiền thu" : "Số tiền chi";
                            sb.AppendLine($"STT,Số phiếu,Ngày,Tên đối tượng,Diễn giải,{mColLd}");
                            int sttLd = 1;
                            decimal totLd = 0;
                            foreach (var item in _rawItems.OrderBy(x => x.LyDoThuChi).ThenBy(x => x.Ngay).ThenBy(x => x.SoPhieu))
                            {
                                decimal amt = (_mode == BaoCaoQuyMode.PhieuThuTheoLyDo) ? (item.Thu > 0 ? item.Thu : item.SoTien) : (item.Chi > 0 ? item.Chi : item.SoTien);
                                totLd += amt;
                                sb.AppendLine($"\"{sttLd++}\",\"{item.SoPhieu}\",\"{item.Ngay:dd/MM/yyyy}\",\"{item.TenDoiTuong?.Replace("\"", "\"\"")}\",\"{item.DienGiai?.Replace("\"", "\"\"")}\",\"{amt}\"");
                            }
                            sb.AppendLine($"\"\",\"\",\"\",\"\",\"TỔNG CỘNG\",\"{totLd}\"");
                            break;

                        case BaoCaoQuyMode.TongHopThuChiTheoNgay:
                            sb.AppendLine("STT,Ngày,Số tiền chi,Số tiền thu");
                            int sttThNgay = 1;
                            decimal totChiN = 0, totThuN = 0;
                            var dateGroups = _rawItems.GroupBy(x => x.Ngay?.Date ?? DateTime.MinValue).OrderBy(g => g.Key);
                            foreach (var g in dateGroups)
                            {
                                string dStr = g.Key == DateTime.MinValue ? "Khác" : g.Key.ToString("dd/MM/yyyy");
                                decimal sumC = g.Sum(x => x.Chi);
                                decimal sumT = g.Sum(x => x.Thu);
                                totChiN += sumC;
                                totThuN += sumT;
                                sb.AppendLine($"\"{sttThNgay++}\",\"{dStr}\",\"{sumC}\",\"{sumT}\"");
                            }
                            sb.AppendLine($"\"\",\"TỔNG CỘNG\",\"{totChiN}\",\"{totThuN}\"");
                            break;

                        case BaoCaoQuyMode.TongHopThuChiTheoLyDo:
                            sb.AppendLine("STT,Lý do thu chi,Số tiền chi,Số tiền thu");
                            int sttThLd = 1;
                            decimal totChiL = 0, totThuL = 0;
                            var ldGroups = _rawItems.GroupBy(x => string.IsNullOrWhiteSpace(x.LyDoThuChi) ? "Chưa thiết lập lý do" : x.LyDoThuChi.Trim()).OrderBy(g => g.Key);
                            foreach (var g in ldGroups)
                            {
                                decimal sumC = g.Sum(x => x.Chi);
                                decimal sumT = g.Sum(x => x.Thu);
                                totChiL += sumC;
                                totThuL += sumT;
                                sb.AppendLine($"\"{sttThLd++}\",\"{g.Key?.Replace("\"", "\"\"")}\",\"{sumC}\",\"{sumT}\"");
                            }
                            sb.AppendLine($"\"\",\"TỔNG CỘNG\",\"{totChiL}\",\"{totThuL}\"");
                            break;
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
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
