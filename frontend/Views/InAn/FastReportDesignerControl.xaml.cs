using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Dapper;
using FastReport;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.InAn
{
    public partial class FastReportDesignerControl : UserControl
    {
        private string _reportName;
        private string? _stemplateId;
        private string? _sreportTemplateId;

        private Report _report = new Report();

        public FastReportDesignerControl(string reportName = "Mẫu 80", string stemplateId = "")
        {
            InitializeComponent();
            _reportName = string.IsNullOrWhiteSpace(reportName) ? "Mẫu 80" : reportName;
            _stemplateId = stemplateId;

            TxtTabReportName.Text = _reportName;
            TxtInfoTemplate.Text = $"Mẫu báo cáo: {_reportName}";

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

                var repTempRow = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID, NAME, CONFIG FROM SREPORTTEMPLATE WHERE UPPER(TRIM(NAME)) = @Name ORDER BY TIMEMODIFIED DESC",
                    new { Name = _reportName.Trim().ToUpper() });

                string loadedXmlConfig = "";

                if (repTempRow != null)
                {
                    _sreportTemplateId = repTempRow.ID;
                    if (repTempRow.CONFIG is byte[] b)
                    {
                        loadedXmlConfig = Encoding.UTF8.GetString(b);
                    }
                    else if (repTempRow.CONFIG != null)
                    {
                        loadedXmlConfig = repTempRow.CONFIG.ToString() ?? "";
                    }
                }

                if (string.IsNullOrEmpty(loadedXmlConfig) && stemRow != null && stemRow.TEMPLATE != null)
                {
                    if (stemRow.TEMPLATE is byte[] bStem)
                    {
                        loadedXmlConfig = Encoding.UTF8.GetString(bStem);
                    }
                    else
                    {
                        loadedXmlConfig = stemRow.TEMPLATE.ToString() ?? "";
                    }
                }

                _report = new Report();
                _report.ReportInfo.Name = _reportName;

                if (!string.IsNullOrWhiteSpace(loadedXmlConfig))
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(loadedXmlConfig)))
                    {
                        _report.Load(ms);
                    }
                }
                else
                {
                    var page = new ReportPage { Name = "Page1" };
                    _report.Pages.Add(page);
                    var titleBand = new ReportTitleBand { Name = "ReportTitle1", Height = 100 };
                    page.ReportTitle = titleBand;
                    var txt = new TextObject
                    {
                        Name = "Text1",
                        Text = $"[{_reportName.ToUpper()}]",
                        Bounds = new System.Drawing.RectangleF(100, 20, 400, 30),
                        Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold)
                    };
                    titleBand.Objects.Add(txt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FastReport Load Error: " + ex.Message);
            }
        }

        private void BtnOpenFastReportDesigner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _report.Design();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở FastReport Designer: " + ex.Message, "Lỗi Designer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnCapNhatVaoCSDL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnCapNhatVaoCSDL.IsEnabled = false;

                byte[] xmlBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    _report.Save(ms);
                    xmlBytes = ms.ToArray();
                }

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                string defaultUserId = "4f1466a0-0756-4ba9-afa8-053b96ca7569";

                await conn.ExecuteAsync(
                    @"UPDATE STEMPLATE 
                      SET TEMPLATE = @Template, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = @UserId 
                      WHERE UPPER(TRIM(NAME)) = @Name OR (ID = @Id)",
                    new { Template = xmlBytes, Name = _reportName.Trim().ToUpper(), Id = _stemplateId ?? "", UserId = defaultUserId });

                if (!string.IsNullOrEmpty(_sreportTemplateId))
                {
                    await conn.ExecuteAsync(
                        @"UPDATE SREPORTTEMPLATE 
                          SET CONFIG = @Config, TIMEMODIFIED = CURRENT_TIMESTAMP, USERMODIFIEDID = @UserId 
                          WHERE ID = @Id",
                        new { Config = xmlBytes, Id = _sreportTemplateId, UserId = defaultUserId });
                }
                else
                {
                    string newId = Guid.NewGuid().ToString();
                    await conn.ExecuteAsync(
                        @"INSERT INTO SREPORTTEMPLATE (ID, NAME, STATUS, CONFIG, USERCREATEDID, USERMODIFIEDID, TIMEMODIFIED, TIMECREATED) 
                          VALUES (@Id, @Name, 30, @Config, @UserId, @UserId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)",
                        new { Id = newId, Name = _reportName.Trim(), Config = xmlBytes, UserId = defaultUserId });
                    _sreportTemplateId = newId;
                }

                MessageBox.Show(
                    $"Đã lưu mẫu báo cáo FastReport [{_reportName}] vào CSDL Firebird (STEMPLATE & SREPORTTEMPLATE) thành công!",
                    "CẬP NHẬT CSDL THÀNH CÔNG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật CSDL Firebird: " + ex.Message, "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnCapNhatVaoCSDL.IsEnabled = true;
            }
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _report.Prepare();
                _report.ShowPrepared();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xem trước FastReport: " + ex.Message, "FastReport Preview Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
