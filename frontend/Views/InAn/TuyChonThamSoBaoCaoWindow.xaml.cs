using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.InAn
{
    public class ThamSoModel : INotifyPropertyChanged
    {
        private bool _hienThi;
        private string _tenThamSo = "";
        private string _maThamSo = "";
        private int _sortOrder;
        private string _defaultValue = "Tất cả";

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public bool HienThi
        {
            get => _hienThi;
            set { _hienThi = value; OnPropertyChanged(nameof(HienThi)); }
        }

        public string TenThamSo
        {
            get => _tenThamSo;
            set { _tenThamSo = value; OnPropertyChanged(nameof(TenThamSo)); }
        }

        public string MaThamSo
        {
            get => _maThamSo;
            set { _maThamSo = value; OnPropertyChanged(nameof(MaThamSo)); }
        }

        public int SortOrder
        {
            get => _sortOrder;
            set { _sortOrder = value; OnPropertyChanged(nameof(SortOrder)); }
        }

        public string DefaultValue
        {
            get => _defaultValue;
            set { _defaultValue = value; OnPropertyChanged(nameof(DefaultValue)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class TuyChonThamSoBaoCaoWindow : Window
    {
        private readonly string _reportName;
        private readonly Action? _onSavedCallback;
        private ObservableCollection<ThamSoModel> _list = new ObservableCollection<ThamSoModel>();
        private string? _sReportId;

        private static readonly List<(string Code, string Name, string DefaultVal)> AllStandardFilters = new()
        {
            ("NGAY", "Khoảng thời gian (Từ ngày - Đến ngày)", "Tháng này"),
            ("DCUAHANGID", "Cửa hàng", "Tất cả"),
            ("DKHOXUATID", "Kho xuất", "Tất cả"),
            ("DKHONHAPID", "Kho nhập", "Tất cả"),
            ("DKHOHANGID", "Kho hàng", "Tất cả"),
            ("DNHANVIENID", "Nhân viên", "Tất cả"),
            ("TDONHANG.DNHANVIENNHAPID", "Nhân viên nhập", "Tất cả"),
            ("TDONHANG.DNHANVIENXUATID", "Nhân viên xuất", "Tất cả"),
            ("TDONHANG.USERTHANHTOANID", "Thu ngân", "Tất cả"),
            ("DKHACHHANGID", "Khách hàng", "Tất cả"),
            ("DNHOMKHACHHANGID", "Nhóm khách hàng", "Tất cả"),
            ("DNHACUNGCAPID", "Nhà cung cấp", "Tất cả"),
            ("DNHOMNHACUNGCAPID", "Nhóm nhà cung cấp", "Tất cả"),
            ("DNHOMMATHANGID", "Nhóm mặt hàng", "Tất cả"),
            ("DMATHANGID", "Mặt hàng", "Tất cả"),
            ("DHANGSANXUATID", "Hãng sản xuất", "Tất cả"),
            ("DLYDOTHUCHIID", "Lý do thu chi", "Tất cả"),
            ("DTAIKHOANNGANHANGID", "Tài khoản ngân hàng", "Tất cả"),
            ("DKHUVUCID", "Khu vực", "Tất cả"),
            ("DNHOMHIENTHIID", "Nhóm hiển thị", "Tất cả"),
            ("DLOAIMATHANGID", "Loại mặt hàng", "Tất cả"),
            ("DNHANVIENGIAOID", "Nhân viên giao hàng", "Tất cả"),
            ("DTHETRATRUOCID", "Thẻ trả trước", "Tất cả"),
            ("DVOUCHERID", "Voucher", "Tất cả"),
            ("DBANID", "Bàn / Phòng", "Tất cả"),
            ("SGROUPUSERID", "Nhóm người dùng", "Tất cả"),
            ("SOPHIEU", "Số phiếu", "")
        };

        public TuyChonThamSoBaoCaoWindow(string reportName, Action? onSavedCallback = null)
        {
            InitializeComponent();
            _reportName = reportName ?? "";
            _onSavedCallback = onSavedCallback;

            TxtTitle.Text = $"TÙY CHỌN THAM SỐ: {_reportName.ToUpper()}";
            Loaded += TuyChonThamSoBaoCaoWindow_Loaded;
        }

        private async void TuyChonThamSoBaoCaoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                var rep = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID, FILTERCONFIG FROM SREPORT WHERE UPPER(TRIM(NAME)) = @Name",
                    new { Name = _reportName.Trim().ToUpper() });

                _sReportId = rep?.ID;

                var existingFilters = new Dictionary<string, (int Order, string Val, string Id)>(StringComparer.OrdinalIgnoreCase);

                if (rep != null && rep.FILTERCONFIG != null)
                {
                    string xmlStr = (rep.FILTERCONFIG is byte[] b) ? Encoding.UTF8.GetString(b) : rep.FILTERCONFIG.ToString();
                    if (!string.IsNullOrWhiteSpace(xmlStr))
                    {
                        var doc = XDocument.Parse(xmlStr);
                        foreach (var dataElem in doc.Descendants("Data"))
                        {
                            string name = dataElem.Element("NAME")?.Value?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(name))
                            {
                                int order = int.TryParse(dataElem.Element("SORTORDER")?.Value, out int o) ? o : existingFilters.Count;
                                string val = dataElem.Element("VALUE")?.Value?.Trim() ?? "Tất cả";
                                string id = dataElem.Element("ID")?.Value?.Trim() ?? Guid.NewGuid().ToString();
                                existingFilters[name] = (order, val, id);
                            }
                        }
                    }
                }

                _list.Clear();
                int idx = 0;

                // First add filters that are enabled in FILTERCONFIG
                foreach (var kvp in existingFilters.OrderBy(x => x.Value.Order))
                {
                    string code = kvp.Key;
                    string friendlyName = ReportTextLocalizationService.GetParameterLabel(code);

                    _list.Add(new ThamSoModel
                    {
                        Id = kvp.Value.Id,
                        MaThamSo = code,
                        TenThamSo = friendlyName,
                        HienThi = true,
                        SortOrder = kvp.Value.Order,
                        DefaultValue = string.IsNullOrWhiteSpace(kvp.Value.Val) ? "Tất cả" : kvp.Value.Val
                    });
                    idx++;
                }

                // Next add other standard filters as unchecked
                foreach (var sf in AllStandardFilters)
                {
                    if (!existingFilters.Keys.Any(code => AreEquivalentFilterCodes(code, sf.Code)))
                    {
                        _list.Add(new ThamSoModel
                        {
                            MaThamSo = sf.Code,
                            TenThamSo = sf.Name,
                            HienThi = false,
                            SortOrder = idx++,
                            DefaultValue = sf.DefaultVal
                        });
                    }
                }

                DgThamSo.ItemsSource = _list;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TuyChonThamSoBaoCaoWindow LoadData error: " + ex.Message);
            }
        }

        private static bool AreEquivalentFilterCodes(string left, string right)
        {
            static string FieldPart(string code)
            {
                string value = code.Trim().ToUpperInvariant();
                int separator = value.LastIndexOf('.');
                return separator >= 0 ? value[(separator + 1)..] : value;
            }

            return string.Equals(FieldPart(left), FieldPart(right), StringComparison.OrdinalIgnoreCase);
        }

        private void DgThamSo_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString("D2");
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_sReportId))
                {
                    MessageBox.Show("Không tìm thấy thông tin báo cáo trong cơ sở dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var activeList = _list.Where(x => x.HienThi).OrderBy(x => x.SortOrder).ToList();
                if (activeList.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một tham số để hiển thị!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Build XML DocumentElement
                var doc = new XDocument(
                    new XElement("DocumentElement",
                        activeList.Select((item, sortIdx) =>
                        {
                            var dataElem = new XElement("Data",
                                new XElement("NAME", item.MaThamSo),
                                new XElement("ID", item.Id),
                                new XElement("SORTORDER", sortIdx)
                            );
                            if (!string.IsNullOrWhiteSpace(item.DefaultValue) && item.DefaultValue != "Tất cả")
                            {
                                dataElem.Add(new XElement("VALUE", item.DefaultValue));
                            }
                            return dataElem;
                        })
                    )
                );

                string xmlConfig = doc.ToString();
                byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlConfig);

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                await conn.ExecuteAsync(
                    @"UPDATE SREPORT 
                      SET FILTERCONFIG = @Config, TIMEMODIFIED = CURRENT_TIMESTAMP 
                      WHERE ID = @Id",
                    new { Config = xmlBytes, Id = _sReportId });

                _onSavedCallback?.Invoke();

                MessageBox.Show("Đã lưu tùy chọn tham số báo cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghi dữ liệu tham số: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
