using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.InAn
{
    public class MauInCotModel : INotifyPropertyChanged
    {
        private string _cot = "";
        private string _tieuDe = "";
        private bool _hienThi = true;
        private bool _nhom1 = false;
        private bool _tongNhom1 = false;
        private bool _nhom2 = false;
        private bool _tongNhom2 = false;
        private bool _tongCong = false;
        private string _canLe = "TRÁI";
        private string _dinhDang = "";

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DataField { get; set; } = "";
        public int Width { get; set; } = 100;
        public string ControlType { get; set; } = "Text";
        public int Index { get; set; } = 0;

        public string Cot
        {
            get => _cot;
            set { _cot = value; OnPropertyChanged(nameof(Cot)); }
        }

        public string TieuDe
        {
            get => _tieuDe;
            set { _tieuDe = value; OnPropertyChanged(nameof(TieuDe)); }
        }

        public bool HienThi
        {
            get => _hienThi;
            set { _hienThi = value; OnPropertyChanged(nameof(HienThi)); }
        }

        public bool Nhom1
        {
            get => _nhom1;
            set { _nhom1 = value; OnPropertyChanged(nameof(Nhom1)); }
        }

        public bool TongNhom1
        {
            get => _tongNhom1;
            set { _tongNhom1 = value; OnPropertyChanged(nameof(TongNhom1)); }
        }

        public bool Nhom2
        {
            get => _nhom2;
            set { _nhom2 = value; OnPropertyChanged(nameof(Nhom2)); }
        }

        public bool TongNhom2
        {
            get => _tongNhom2;
            set { _tongNhom2 = value; OnPropertyChanged(nameof(TongNhom2)); }
        }

        public bool TongCong
        {
            get => _tongCong;
            set { _tongCong = value; OnPropertyChanged(nameof(TongCong)); }
        }

        public string CanLe
        {
            get => _canLe;
            set { _canLe = value; OnPropertyChanged(nameof(CanLe)); }
        }

        public string DinhDang
        {
            get => _dinhDang;
            set { _dinhDang = value; OnPropertyChanged(nameof(DinhDang)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TemplateLookupItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public partial class ThietKeMauInWindow : Window
    {
        private readonly string _reportName;
        private readonly DataGrid? _sourceGrid;
        private readonly Action<List<MauInCotModel>>? _onSaveCallback;
        private readonly Action<ReportPaperLayout>? _onLayoutCallback;
        private ObservableCollection<MauInCotModel> _cotList = new ObservableCollection<MauInCotModel>();
        private string? _currentReportTemplateId;
        private string? _sReportId;
        private bool _isInitializing = true;

        public ThietKeMauInWindow(
            string reportName,
            DataGrid? sourceGrid = null,
            Action<List<MauInCotModel>>? onSaveCallback = null,
            Action<ReportPaperLayout>? onLayoutCallback = null)
        {
            InitializeComponent();
            _reportName = reportName ?? "";
            _sourceGrid = sourceGrid;
            _onSaveCallback = onSaveCallback;
            _onLayoutCallback = onLayoutCallback;

            Loaded += ThietKeMauInWindow_Loaded;
        }

        private async void ThietKeMauInWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            _isInitializing = true;
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // 1. Load Mẫu cơ bản (STEMPLATE where REPORTBASE > 0)
                var stemplateList = (await conn.QueryAsync<TemplateLookupItem>(
                    "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM STEMPLATE WHERE (STATUS IS NULL OR STATUS <> 0) AND (REPORTBASE > 0 OR NAME LIKE '%A4%' OR NAME LIKE '%80%') ORDER BY NAME")).ToList();

                if (stemplateList.Count == 0)
                {
                    stemplateList = new List<TemplateLookupItem>
                    {
                        new TemplateLookupItem { Id = "654ab97d-c24e-4a09-ac67-cfd7ef6f3a65", Name = "Mẫu A4 thẳng đứng" },
                        new TemplateLookupItem { Id = "8d8e3b03-ee60-4319-a1d7-d284848b673b", Name = "Mẫu A4 nằm ngang" },
                        new TemplateLookupItem { Id = "13645052-fffe-4146-be1a-e99348a1c86a", Name = "Mẫu 80" }
                    };
                }

                CboMauCoBan.ItemsSource = stemplateList;
                if (stemplateList.Count > 0)
                {
                    var defaultStem = stemplateList.FirstOrDefault(s => s.Name.Contains("A4 thẳng")) ?? stemplateList[0];
                    CboMauCoBan.SelectedValue = defaultStem.Id;
                }

                // 2. Find Report ID in SREPORT
                var rep = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME FROM SREPORT WHERE UPPER(TRIM(NAME)) = @Name",
                    new { Name = _reportName.Trim().ToUpper() });

                _sReportId = rep?.ID;

                // 3. Find existing SREPORTTEMPLATE
                dynamic? repTemplate = null;
                if (!string.IsNullOrEmpty(_sReportId))
                {
                    repTemplate = await conn.QueryFirstOrDefaultAsync(
                        "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME, NOTE, CAST(STEMPLATEID AS VARCHAR(50)) AS STEMPLATEID, AUTOGENREPORT, CONFIG FROM SREPORTTEMPLATE WHERE SREPORTID = @RepId AND (STATUS IS NULL OR STATUS <> 0)",
                        new { RepId = _sReportId });
                }

                _cotList.Clear();

                if (repTemplate != null)
                {
                    _currentReportTemplateId = repTemplate.ID;
                    TxtTenMau.Text = repTemplate.NAME ?? "Mẫu A4";
                    TxtGhiChu.Text = repTemplate.NOTE ?? "";
                    ChkTuDongSinh.IsChecked = (repTemplate.AUTOGENREPORT == 30 || repTemplate.AUTOGENREPORT == 1);

                    if (!string.IsNullOrEmpty(repTemplate.STEMPLATEID?.ToString()))
                    {
                        CboMauCoBan.SelectedValue = repTemplate.STEMPLATEID.ToString();
                    }

                    // Parse XML CONFIG
                    string xmlStr = "";
                    if (repTemplate.CONFIG is byte[] bytes)
                    {
                        xmlStr = Encoding.UTF8.GetString(bytes);
                    }
                    else if (repTemplate.CONFIG != null)
                    {
                        xmlStr = repTemplate.CONFIG.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(xmlStr))
                    {
                        ParseConfigXml(xmlStr);
                    }
                }

                // If XML had no columns or no template, generate from current DataGrid
                if (_cotList.Count == 0)
                {
                    GenerateDefaultColumns();
                }

                DgCacCot.ItemsSource = _cotList;
                RefreshBottomTags();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ThietKeMauInWindow LoadData error: " + ex.Message);
                if (_cotList.Count == 0)
                {
                    GenerateDefaultColumns();
                    DgCacCot.ItemsSource = _cotList;
                    RefreshBottomTags();
                }
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void ParseConfigXml(string xmlStr)
        {
            try
            {
                var doc = XDocument.Parse(xmlStr);
                int index = 0;
                foreach (var dataElem in doc.Descendants("Data"))
                {
                    string cot = dataElem.Element("COT")?.Value ?? "";
                    string dataField = dataElem.Element("DATAFIELD")?.Value ?? "";
                    string caption = dataElem.Element("CAPTION")?.Value ?? "";
                    string hienThiVal = dataElem.Element("HIENTHI")?.Value ?? "30";
                    string nhom1Val = dataElem.Element("NHOM1")?.Value ?? "0";
                    string tongNhom1Val = dataElem.Element("TONGNHOM1")?.Value ?? "0";
                    string nhom2Val = dataElem.Element("NHOM2")?.Value ?? "0";
                    string tongNhom2Val = dataElem.Element("TONGNHOM2")?.Value ?? "0";
                    string tongCongVal = dataElem.Element("TONGCONG")?.Value ?? "0";
                    string align = dataElem.Element("ALIGN")?.Value ?? "";
                    string format = dataElem.Element("FORMAT")?.Value ?? "";
                    string controlType = dataElem.Element("CONTROLTYPE")?.Value ?? "Text";
                    string id = dataElem.Element("ID")?.Value ?? Guid.NewGuid().ToString();

                    if (string.IsNullOrEmpty(align))
                    {
                        if (controlType == "Number") align = "PHẢI";
                        else if (controlType == "Date") align = "GIỮA";
                        else align = "TRÁI";
                    }

                    var item = new MauInCotModel
                    {
                        Id = id,
                        Cot = string.IsNullOrEmpty(cot) ? dataField : cot,
                        DataField = dataField,
                        TieuDe = string.IsNullOrEmpty(caption) ? (string.IsNullOrEmpty(cot) ? dataField : cot) : caption,
                        HienThi = (hienThiVal == "30" || hienThiVal == "1" || hienThiVal.ToLower() == "true"),
                        Nhom1 = (nhom1Val == "30" || nhom1Val == "1"),
                        TongNhom1 = (tongNhom1Val == "30" || tongNhom1Val == "1"),
                        Nhom2 = (nhom2Val == "30" || nhom2Val == "1"),
                        TongNhom2 = (tongNhom2Val == "30" || tongNhom2Val == "1"),
                        TongCong = (tongCongVal == "30" || tongCongVal == "1"),
                        CanLe = align,
                        DinhDang = format,
                        ControlType = controlType,
                        Index = index++
                    };

                    _cotList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ParseConfigXml error: " + ex.Message);
            }
        }

        private void GenerateDefaultColumns()
        {
            if (_sourceGrid != null && _sourceGrid.Columns.Count > 0)
            {
                int index = 0;
                foreach (var col in _sourceGrid.Columns)
                {
                    string header = col.Header?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(header)) continue;

                    string propName = "";
                    if (col is DataGridBoundColumn boundCol && boundCol.Binding is System.Windows.Data.Binding b)
                    {
                        propName = b.Path.Path;
                    }
                    if (string.IsNullOrEmpty(propName)) propName = header;

                    bool isNumber = header.Contains("Tiền") || header.Contains("Cộng") || header.Contains("Giá") ||
                                    header.Contains("Số lượng") || header.Contains("Thu") || header.Contains("Chi") ||
                                    header.Contains("Nợ") || header.Contains("Doanh số") || header.Contains("Lãi");

                    bool isDate = header.Contains("Ngày") || header.Contains("Giờ");

                    _cotList.Add(new MauInCotModel
                    {
                        Cot = header,
                        DataField = propName,
                        TieuDe = header,
                        HienThi = true,
                        TongCong = isNumber,
                        CanLe = isNumber ? "PHẢI" : (isDate ? "GIỮA" : "TRÁI"),
                        DinhDang = isNumber ? "#,##0" : (isDate ? "dd/MM/yyyy" : ""),
                        ControlType = isNumber ? "Number" : (isDate ? "Date" : "Text"),
                        Index = index++
                    });
                }
            }
            else
            {
                // Fallback default sample columns
                _cotList.Add(new MauInCotModel { Cot = "Ngày", DataField = "NGAY", TieuDe = "Ngày", HienThi = true, CanLe = "GIỮA", DinhDang = "dd/MM/yyyy", ControlType = "Date" });
                _cotList.Add(new MauInCotModel { Cot = "Số phiếu", DataField = "SOPHIEU", TieuDe = "Số phiếu", HienThi = true, CanLe = "TRÁI" });
                _cotList.Add(new MauInCotModel { Cot = "Mặt hàng", DataField = "MATHANG", TieuDe = "Mặt hàng", HienThi = true, CanLe = "TRÁI" });
                _cotList.Add(new MauInCotModel { Cot = "Số lượng", DataField = "SOLUONG", TieuDe = "Số lượng", HienThi = true, TongCong = true, CanLe = "PHẢI", DinhDang = "#,##0.##", ControlType = "Number" });
                _cotList.Add(new MauInCotModel { Cot = "Đơn giá", DataField = "DONGIA", TieuDe = "Đơn giá", HienThi = true, CanLe = "PHẢI", DinhDang = "#,##0", ControlType = "Number" });
                _cotList.Add(new MauInCotModel { Cot = "Thành tiền", DataField = "THANHTIEN", TieuDe = "Thành tiền", HienThi = true, TongCong = true, CanLe = "PHẢI", DinhDang = "#,##0", ControlType = "Number" });
            }
        }

        private void RefreshBottomTags()
        {
            IcChiTietColumns.ItemsSource = _cotList
                .Where(x => x.HienThi || x.TongCong)
                .Select(x => string.IsNullOrWhiteSpace(x.TieuDe) ? x.Cot : x.TieuDe)
                .ToList();

            IcNhom1Columns.ItemsSource = _cotList
                .Where(x => x.Nhom1)
                .Select(x => string.IsNullOrWhiteSpace(x.TieuDe) ? x.Cot : x.TieuDe)
                .ToList();

            IcNhom2Columns.ItemsSource = _cotList
                .Where(x => x.Nhom2)
                .Select(x => string.IsNullOrWhiteSpace(x.TieuDe) ? x.Cot : x.TieuDe)
                .ToList();
        }

        private void ColCheckbox_Click(object sender, RoutedEventArgs e)
        {
            RefreshBottomTags();
        }

        private void DgCacCot_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString("D2");
        }

        private void CboMauCoBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var selected = CboMauCoBan.SelectedItem as TemplateLookupItem;
            if (selected == null) return;

            // 1. Update template name if default or empty
            string currentName = TxtTenMau.Text.Trim();
            if (string.IsNullOrEmpty(currentName) ||
                currentName == "Mẫu A4" ||
                currentName == "Mẫu 80mm" ||
                currentName.StartsWith("Mẫu A4 ") ||
                currentName.StartsWith("Mẫu 80"))
            {
                TxtTenMau.Text = selected.Name;
            }

            // 2. Adjust columns based on selected basic template
            string nameLower = selected.Name.ToLower();
            bool is80mm = nameLower.Contains("80") || nameLower.Contains("58") || nameLower.Contains("bill");

            if (is80mm)
            {
                // Narrow receipt: keep only essential columns visible
                foreach (var col in _cotList)
                {
                    string colName = (col.Cot ?? "").ToLower();
                    string fieldName = (col.DataField ?? "").ToLower();

                    if (colName.Contains("mặt hàng") || colName.Contains("tên hàng") || fieldName.Contains("tenhang") ||
                        colName == "đvt" || fieldName.Contains("dvt") ||
                        colName.Contains("số lượng") || fieldName.Contains("soluong") ||
                        colName.Contains("đơn giá") || fieldName.Contains("dongia") ||
                        colName.Contains("thành tiền") || fieldName.Contains("thanhtien") ||
                        colName == "stt")
                    {
                        col.HienThi = true;
                    }
                    else
                    {
                        col.HienThi = false;
                    }
                }
            }
            else
            {
                // A4 (Portrait or Landscape): standard columns visible
                foreach (var col in _cotList)
                {
                    string colName = (col.Cot ?? "").ToLower();
                    string fieldName = (col.DataField ?? "").ToLower();

                    if (colName.Contains("mã hàng") || colName.Contains("mặt hàng") || colName.Contains("tên hàng") ||
                        colName == "đvt" || colName.Contains("số lượng") ||
                        colName.Contains("đơn giá") || colName.Contains("thành tiền") ||
                        colName == "stt")
                    {
                        col.HienThi = true;
                    }
                }
            }

            DgCacCot.Items.Refresh();
            RefreshBottomTags();
        }

        private void BtnThietKeMauNangCao_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tính năng mở trình thiết kế bố cục in trực quan (FastReports Designer / Report Visual Designer) sẽ sẵn sàng khi tích hợp file mẫu .frx.",
                "Thiết kế mẫu nâng cao",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tenMau = TxtTenMau.Text.Trim();
                if (string.IsNullOrEmpty(tenMau))
                {
                    MessageBox.Show("Vui lòng nhập tên mẫu in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtTenMau.Focus();
                    return;
                }

                string note = TxtGhiChu.Text?.Trim() ?? "";
                string stemplateId = CboMauCoBan.SelectedValue?.ToString() ?? "654ab97d-c24e-4a09-ac67-cfd7ef6f3a65";
                int autoGen = (ChkTuDongSinh.IsChecked == true) ? 30 : 0;

                // Lấy tên mẫu cơ bản được chọn (dùng để phát hiện kiểu giấy sau này)
                string stemName = (CboMauCoBan.SelectedItem as TemplateLookupItem)?.Name ?? "";

                // Layout không phụ thuộc việc ghi DB: áp dụng và lưu local ngay theo lựa chọn hiện tại.
                string selectedStemName = string.IsNullOrWhiteSpace(stemName) ? tenMau : stemName;
                ReportLayoutPersist.Save(_reportName, selectedStemName);
                var immediateLayout = ReportTemplateConfigService.BuildLayoutFromName(selectedStemName, tenMau);
                _onLayoutCallback?.Invoke(immediateLayout);

                // Build XML CONFIG (thêm META để lưu tên mẫu cơ bản)
                var doc = new XDocument(
                    new XElement("DocumentElement",
                        new XElement("META",
                            new XElement("BASETEMPNAME", stemName),
                            new XElement("TEMPLATENAME", tenMau)
                        ),
                        _cotList.Select((col, idx) => new XElement("Data",
                            new XElement("ID", col.Id),
                            new XElement("COT", col.Cot),
                            new XElement("DATAFIELD", col.DataField),
                            new XElement("CAPTION", col.TieuDe),
                            new XElement("HIENTHI", col.HienThi ? 30 : 0),
                            new XElement("NHOM1", col.Nhom1 ? 30 : 0),
                            new XElement("TONGNHOM1", col.TongNhom1 ? 30 : 0),
                            new XElement("NHOM2", col.Nhom2 ? 30 : 0),
                            new XElement("TONGNHOM2", col.TongNhom2 ? 30 : 0),
                            new XElement("TONGCONG", col.TongCong ? 30 : 0),
                            new XElement("INDEX", idx),
                            new XElement("WIDTH", col.Width),
                            new XElement("ALIGN", col.CanLe),
                            new XElement("FORMAT", col.DinhDang),
                            new XElement("CONTROLTYPE", col.ControlType)
                        ))
                    )
                );

                string xmlConfig = doc.ToString();
                byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlConfig);

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // Đảm bảo SREPORT tồn tại (tạo mới nếu chưa có)
                if (string.IsNullOrEmpty(_sReportId))
                {
                    // Kiểm tra lại xem đã có chưa (phòng trường hợp race condition)
                    var existRep = await conn.QueryFirstOrDefaultAsync(
                        "SELECT CAST(ID AS VARCHAR(50)) AS ID FROM SREPORT WHERE UPPER(TRIM(NAME)) = @Name",
                        new { Name = _reportName.Trim().ToUpper() });

                    if (existRep != null)
                    {
                        _sReportId = existRep.ID?.ToString();
                    }
                    else
                    {
                        // Tạo mới dòng SREPORT (chỉ dùng cột cơ bản ID và NAME)
                        string newRepId = Guid.NewGuid().ToString();
                        await conn.ExecuteAsync(
                            "INSERT INTO SREPORT (ID, NAME) VALUES (@Id, @Name)",
                            new { Id = newRepId, Name = _reportName.Trim() });
                        _sReportId = newRepId;
                    }
                }

                if (!string.IsNullOrEmpty(_currentReportTemplateId))
                {
                    // Update existing
                    await conn.ExecuteAsync(
                        @"UPDATE SREPORTTEMPLATE 
                          SET NAME = @Name, NOTE = @Note, STEMPLATEID = @StemId, AUTOGENREPORT = @AutoGen, CONFIG = @Config, TIMEMODIFIED = CURRENT_TIMESTAMP 
                          WHERE ID = @Id",
                        new { Name = tenMau, Note = note, StemId = stemplateId, AutoGen = autoGen, Config = xmlBytes, Id = _currentReportTemplateId });
                }
                else
                {
                    // Insert new
                    string newId = Guid.NewGuid().ToString();
                    await conn.ExecuteAsync(
                        @"INSERT INTO SREPORTTEMPLATE (ID, NAME, NOTE, STEMPLATEID, SREPORTID, AUTOGENREPORT, STATUS, CONFIG, TIMEMODIFIED, TIMECREATED) 
                          VALUES (@Id, @Name, @Note, @StemId, @RepId, @AutoGen, 30, @Config, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                        new { Id = newId, Name = tenMau, Note = note, StemId = stemplateId, RepId = _sReportId, AutoGen = autoGen, Config = xmlBytes });
                    _currentReportTemplateId = newId;
                }

                _onSaveCallback?.Invoke(_cotList.ToList());

                MessageBox.Show("Đã lưu cấu hình mẫu in thành công!", "MẪU IN", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu mẫu in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
