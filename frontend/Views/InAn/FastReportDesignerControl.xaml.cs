using System;
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
    public partial class FastReportDesignerControl : UserControl
    {
        private string _reportName;
        private string? _stemplateId;
        private string? _sreportTemplateId;
        private string _loadedXmlConfig = "";

        public FastReportDesignerControl(string reportName = "Mẫu A4 nằm ngang", string stemplateId = "")
        {
            InitializeComponent();
            _reportName = string.IsNullOrWhiteSpace(reportName) ? "Mẫu A4 nằm ngang" : reportName;
            _stemplateId = stemplateId;

            TxtTabReportName.Text = _reportName;
            TxtCanvasTitle.Text = $"[{_reportName.ToUpper()}]";

            Loaded += FastReportDesignerControl_Loaded;
        }

        private async void FastReportDesignerControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTemplateFromFirebirdDbAsync();
        }

        private async Task LoadTemplateFromFirebirdDbAsync()
        {
            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // 1. Tìm mẫu trong STEMPLATE (theo ID hoặc Tên mẫu)
                dynamic? stemRow = null;
                if (!string.IsNullOrEmpty(_stemplateId))
                {
                    stemRow = await conn.QueryFirstOrDefaultAsync(
                        "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME, TEMPLATE FROM STEMPLATE WHERE ID = @Id",
                        new { Id = _stemplateId });
                }

                if (stemRow == null)
                {
                    stemRow = await conn.QueryFirstOrDefaultAsync(
                        "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME, TEMPLATE FROM STEMPLATE WHERE UPPER(TRIM(NAME)) = @Name OR UPPER(TRIM(NAME)) LIKE '%' || @Name || '%'",
                        new { Name = _reportName.Trim().ToUpper() });
                }

                // 2. Tìm mẫu trong SREPORTTEMPLATE (nếu có cấu hình tùy chỉnh)
                var repTempRow = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME, CONFIG FROM SREPORTTEMPLATE WHERE UPPER(TRIM(NAME)) = @Name ORDER BY TIMEMODIFIED DESC",
                    new { Name = _reportName.Trim().ToUpper() });

                if (repTempRow != null)
                {
                    _sreportTemplateId = repTempRow.ID;
                    if (repTempRow.CONFIG is byte[] b)
                    {
                        _loadedXmlConfig = Encoding.UTF8.GetString(b);
                    }
                    else if (repTempRow.CONFIG != null)
                    {
                        _loadedXmlConfig = repTempRow.CONFIG.ToString() ?? "";
                    }
                }

                if (string.IsNullOrEmpty(_loadedXmlConfig) && stemRow != null && stemRow.TEMPLATE != null)
                {
                    if (stemRow.TEMPLATE is byte[] bStem)
                    {
                        _loadedXmlConfig = Encoding.UTF8.GetString(bStem);
                    }
                    else
                    {
                        _loadedXmlConfig = stemRow.TEMPLATE.ToString() ?? "";
                    }
                }

                // Nếu có XML cấu hình từ CSDL Firebird, parse để kiểm tra dữ liệu mẫu
                if (!string.IsNullOrWhiteSpace(_loadedXmlConfig))
                {
                    try
                    {
                        var doc = XDocument.Parse(_loadedXmlConfig);
                        string tTitle = doc.Root?.Element("META")?.Element("TEMPLATENAME")?.Value ?? "";
                        if (!string.IsNullOrEmpty(tTitle))
                        {
                            TxtCanvasTitle.Text = $"[{tTitle.ToUpper()}]";
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FastReportDesignerControl Load Error: " + ex.Message);
            }
        }

        private async void BtnCapNhatVaoCSDL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnCapNhatVaoCSDL.IsEnabled = false;

                // Tạo XML FastReport Template để ghi vào CSDL Firebird
                var doc = new XDocument(
                    new XElement("DocumentElement",
                        new XElement("META",
                            new XElement("TEMPLATENAME", _reportName),
                            new XElement("TIMEMODIFIED", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        ),
                        new XElement("FastReport",
                            new XElement("ReportInfo", new XAttribute("Name", _reportName)),
                            new XElement("PaperLayout", "A4 Landscape")
                        )
                    )
                );

                string xmlConfig = doc.ToString();
                byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlConfig);

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // 1. Cập nhật vào STEMPLATE nếu khớp ID hoặc Tên mẫu
                await conn.ExecuteAsync(
                    @"UPDATE STEMPLATE 
                      SET TEMPLATE = @Template, TIMEMODIFIED = CURRENT_TIMESTAMP 
                      WHERE UPPER(TRIM(NAME)) = @Name OR (ID = @Id)",
                    new { Template = xmlBytes, Name = _reportName.Trim().ToUpper(), Id = _stemplateId ?? "" });

                // 2. Cập nhật hoặc thêm mới vào SREPORTTEMPLATE
                if (!string.IsNullOrEmpty(_sreportTemplateId))
                {
                    await conn.ExecuteAsync(
                        @"UPDATE SREPORTTEMPLATE 
                          SET CONFIG = @Config, TIMEMODIFIED = CURRENT_TIMESTAMP 
                          WHERE ID = @Id",
                        new { Config = xmlBytes, Id = _sreportTemplateId });
                }
                else
                {
                    string newId = Guid.NewGuid().ToString();
                    await conn.ExecuteAsync(
                        @"INSERT INTO SREPORTTEMPLATE (ID, NAME, STATUS, CONFIG, TIMEMODIFIED, TIMECREATED) 
                          VALUES (@Id, @Name, 30, @Config, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                        new { Id = newId, Name = _reportName.Trim(), Config = xmlBytes });
                    _sreportTemplateId = newId;
                }

                MessageBox.Show(
                    $"Đã cập nhật mẫu in [{_reportName}] vào cơ sở dữ liệu Firebird (bảng STEMPLATE & SREPORTTEMPLATE) thành công!",
                    "CẬP NHẬT CSDL THÀNH CÔNG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật cơ sở dữ liệu: " + ex.Message, "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnCapNhatVaoCSDL.IsEnabled = true;
            }
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Xem trước mẫu báo cáo FastReport: {_reportName}", "FastReport Preview", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
