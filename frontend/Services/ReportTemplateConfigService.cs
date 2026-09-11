using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class ReportColumnConfigItem
    {
        public string Cot { get; set; } = "";
        public string DataField { get; set; } = "";
        public string Caption { get; set; } = "";
        public bool HienThi { get; set; } = true;
        public bool Nhom1 { get; set; } = false;
        public bool TongNhom1 { get; set; } = false;
        public bool Nhom2 { get; set; } = false;
        public bool TongNhom2 { get; set; } = false;
        public bool TongCong { get; set; } = false;
        public string CanLe { get; set; } = "TRÁI";
        public string Format { get; set; } = "";
        public int Width { get; set; } = 100;
        public int Index { get; set; } = 0;
    }

    public class ReportFilterConfigItem
    {
        public string Name { get; set; } = "";
        public int SortOrder { get; set; } = 0;
        public string Value { get; set; } = "";
    }

    public class ReportPaperLayout
    {
        public string TemplateName { get; set; } = "Mẫu A4";
        public string BaseTemplateId { get; set; } = "";
        public string BaseTemplateName { get; set; } = "Mẫu A4 thẳng đứng";
        public bool Is80mm { get; set; } = false;
        public bool IsLandscape { get; set; } = false;
        public double PaperWidth { get; set; } = 800;
        public double PaperMinHeight { get; set; } = 1050;
        public Thickness PaperPadding { get; set; } = new Thickness(40, 30, 40, 40);
        public double TitleFontSize { get; set; } = 16;
        public double HeaderFontSize { get; set; } = 12;
        public double CellFontSize { get; set; } = 12;
        public double RowHeight { get; set; } = 26;
        public bool IsCompactBill { get; set; } = false;
    }

    public class ReportFullTemplateConfig
    {
        public ReportPaperLayout Layout { get; set; } = new ReportPaperLayout();
        public List<ReportColumnConfigItem> Columns { get; set; } = new List<ReportColumnConfigItem>();
    }

    public static class ReportTemplateConfigService
    {
        public static async Task<List<ReportFilterConfigItem>> GetFilterConfigAsync(string reportName)
        {
            var list = new List<ReportFilterConfigItem>();
            if (string.IsNullOrWhiteSpace(reportName)) return list;

            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                var rep = await conn.QueryFirstOrDefaultAsync(
                    "SELECT FILTERCONFIG FROM SREPORT WHERE UPPER(TRIM(NAME)) = @Name",
                    new { Name = reportName.Trim().ToUpper() });

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
                                int order = int.TryParse(dataElem.Element("SORTORDER")?.Value, out int o) ? o : list.Count;
                                string val = dataElem.Element("VALUE")?.Value?.Trim() ?? "Tất cả";
                                list.Add(new ReportFilterConfigItem
                                {
                                    Name = name,
                                    SortOrder = order,
                                    Value = val
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFilterConfigAsync error: " + ex.Message);
            }

            return list.OrderBy(x => x.SortOrder).ToList();
        }

        public static async Task<ReportFullTemplateConfig> GetFullTemplateConfigAsync(string reportName)
        {
            var result = new ReportFullTemplateConfig();
            if (string.IsNullOrWhiteSpace(reportName)) return result;

            string persistedStemName = ReportLayoutPersist.Load(reportName);
            if (!string.IsNullOrWhiteSpace(persistedStemName))
            {
                result.Layout = BuildLayoutFromName(persistedStemName);
            }

            try
            {
                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                // --- Bước 1: Tìm SREPORT theo tên báo cáo (exact match) ---
                var rep = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID FROM SREPORT WHERE UPPER(TRIM(NAME)) = @Name",
                    new { Name = reportName.Trim().ToUpper() });

                dynamic? template = null;

                // --- Bước 2: Tìm SREPORTTEMPLATE qua SREPORTID ---
                if (rep != null)
                {
                    template = await conn.QueryFirstOrDefaultAsync(
                        @"SELECT CAST(t.ID AS VARCHAR(50)) AS ID, 
                                 t.NAME, 
                                 CAST(t.STEMPLATEID AS VARCHAR(50)) AS STEMPLATEID, 
                                 s.NAME AS STEMPLATENAME, 
                                 t.CONFIG 
                          FROM SREPORTTEMPLATE t
                          LEFT JOIN STEMPLATE s ON TRIM(CAST(t.STEMPLATEID AS VARCHAR(50))) = TRIM(CAST(s.ID AS VARCHAR(50)))
                          WHERE t.SREPORTID = @RepId AND (t.STATUS IS NULL OR t.STATUS <> 0)
                          ORDER BY t.TIMEMODIFIED DESC",
                        new { RepId = rep.ID });
                }

                // --- Bước 3: Fallback tìm qua tên mẫu chứa từ khóa tên báo cáo ---
                if (template == null)
                {
                    // Lấy từ đầu tiên của tên báo cáo để tìm fuzzy
                    string firstWord = reportName.Trim().Split(' ').FirstOrDefault() ?? reportName.Trim();
                    template = await conn.QueryFirstOrDefaultAsync(
                        @"SELECT CAST(t.ID AS VARCHAR(50)) AS ID, 
                                 t.NAME, 
                                 CAST(t.STEMPLATEID AS VARCHAR(50)) AS STEMPLATEID, 
                                 s.NAME AS STEMPLATENAME, 
                                 t.CONFIG 
                          FROM SREPORTTEMPLATE t
                          LEFT JOIN STEMPLATE s ON TRIM(CAST(t.STEMPLATEID AS VARCHAR(50))) = TRIM(CAST(s.ID AS VARCHAR(50)))
                          WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                            AND (UPPER(TRIM(t.NAME)) = @ExactName
                              OR UPPER(t.NAME) CONTAINING @FirstWord)
                          ORDER BY t.TIMEMODIFIED DESC",
                        new { ExactName = reportName.Trim().ToUpper(), FirstWord = firstWord.ToUpper() });
                }

                if (template != null)
                {
                    result.Layout.TemplateName = template.NAME ?? "";
                    string stemId = (template.STEMPLATEID ?? "").ToString().Trim();
                    result.Layout.BaseTemplateId = stemId;
                    string baseName = (template.STEMPLATENAME ?? "").ToString().Trim();

                    // --- Bước 0: Đọc BASETEMPNAME từ CONFIG XML (đáng tin cậy nhất, không phụ thuộc GUID) ---
                    string xmlStrForMeta = "";
                    if (template.CONFIG != null)
                    {
                        try
                        {
                            xmlStrForMeta = (template.CONFIG is byte[] bMeta)
                                ? Encoding.UTF8.GetString(bMeta)
                                : template.CONFIG.ToString() ?? "";
                        }
                        catch { }
                    }

                    if (!string.IsNullOrWhiteSpace(xmlStrForMeta))
                    {
                        try
                        {
                            var metaDoc = XDocument.Parse(xmlStrForMeta);
                            string xmlBaseName = metaDoc.Root?.Element("META")?.Element("BASETEMPNAME")?.Value?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(xmlBaseName))
                                baseName = xmlBaseName; // Ưu tiên tên từ XML (chính xác nhất)
                        }
                        catch { }
                    }

                    // --- Fallback 1: query STEMPLATE trực tiếp nếu XML không có META ---
                    if (string.IsNullOrEmpty(baseName) && !string.IsNullOrEmpty(stemId))
                    {
                        try
                        {
                            baseName = await conn.QueryFirstOrDefaultAsync<string>(
                                "SELECT NAME FROM STEMPLATE WHERE TRIM(CAST(ID AS VARCHAR(50))) = @StemId OR REPLACE(TRIM(CAST(ID AS VARCHAR(50))),'-','') = REPLACE(@StemId,'-','')",
                                new { StemId = stemId }) ?? "";
                        }
                        catch { }
                    }

                    // --- Fallback 2: dùng tên mẫu báo cáo nếu vẫn rỗng ---
                    if (string.IsNullOrEmpty(baseName))
                    {
                        baseName = result.Layout.TemplateName;
                    }
                    string templateName = (template.NAME ?? "").ToString();
                    string effectiveStemName = string.IsNullOrWhiteSpace(persistedStemName)
                        ? baseName
                        : persistedStemName;
                    result.Layout = BuildLayoutFromName(effectiveStemName, templateName);
                    result.Layout.BaseTemplateId = stemId;

                    if (template.CONFIG != null)
                    {
                        string xmlStr = (template.CONFIG is byte[] b) ? Encoding.UTF8.GetString(b) : template.CONFIG.ToString();
                        if (!string.IsNullOrWhiteSpace(xmlStr))
                        {
                            var doc = XDocument.Parse(xmlStr);
                            int idx = 0;
                            foreach (var dataElem in doc.Descendants("Data"))
                            {
                                string cot = dataElem.Element("COT")?.Value?.Trim() ?? "";
                                string dataField = dataElem.Element("DATAFIELD")?.Value?.Trim() ?? "";
                                string caption = dataElem.Element("CAPTION")?.Value?.Trim() ?? "";
                                string hienThiVal = dataElem.Element("HIENTHI")?.Value?.Trim() ?? "30";
                                string nhom1Val = dataElem.Element("NHOM1")?.Value?.Trim() ?? "0";
                                string tongNhom1Val = dataElem.Element("TONGNHOM1")?.Value?.Trim() ?? "0";
                                string nhom2Val = dataElem.Element("NHOM2")?.Value?.Trim() ?? "0";
                                string tongNhom2Val = dataElem.Element("TONGNHOM2")?.Value?.Trim() ?? "0";
                                string tongCongVal = dataElem.Element("TONGCONG")?.Value?.Trim() ?? "0";
                                string align = dataElem.Element("ALIGN")?.Value?.Trim() ?? "TRÁI";
                                string format = dataElem.Element("FORMAT")?.Value?.Trim() ?? "";
                                int width = int.TryParse(dataElem.Element("WIDTH")?.Value, out int w) && w > 0 ? w : 100;

                                result.Columns.Add(new ReportColumnConfigItem
                                {
                                    Cot = string.IsNullOrEmpty(cot) ? dataField : cot,
                                    DataField = dataField,
                                    Caption = string.IsNullOrEmpty(caption) ? (string.IsNullOrEmpty(cot) ? dataField : cot) : caption,
                                    HienThi = (hienThiVal == "30" || hienThiVal == "1" || hienThiVal.ToLower() == "true"),
                                    Nhom1 = (nhom1Val == "30" || nhom1Val == "1"),
                                    TongNhom1 = (tongNhom1Val == "30" || tongNhom1Val == "1"),
                                    Nhom2 = (nhom2Val == "30" || nhom2Val == "1"),
                                    TongNhom2 = (tongNhom2Val == "30" || tongNhom2Val == "1"),
                                    TongCong = (tongCongVal == "30" || tongCongVal == "1"),
                                    CanLe = align,
                                    Format = format,
                                    Width = width,
                                    Index = idx++
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFullTemplateConfigAsync error: " + ex.Message);
            }

            return result;
        }

        public static ReportPaperLayout BuildLayoutFromName(string stemName, string templateName = "")
        {
            var layout = new ReportPaperLayout
            {
                TemplateName = templateName,
                BaseTemplateName = stemName
            };

            string layoutName = $"{stemName} {templateName}".ToLowerInvariant();
            if (layoutName.Contains("80") || layoutName.Contains("58") || layoutName.Contains("54") ||
                layoutName.Contains("bill") || layoutName.Contains("nhiet") || layoutName.Contains("nhiệt"))
            {
                layout.Is80mm = true;
                layout.IsCompactBill = true;
                layout.PaperWidth = 340;
                layout.PaperMinHeight = 450;
                layout.PaperPadding = new Thickness(12, 16, 12, 22);
                layout.TitleFontSize = 13;
                layout.HeaderFontSize = 10.5;
                layout.CellFontSize = 10;
                layout.RowHeight = 22;
            }
            else if (layoutName.Contains("ngang") || layoutName.Contains("landscape") || layoutName.Contains("horizontal"))
            {
                layout.IsLandscape = true;
                layout.PaperWidth = 1120;
                layout.PaperMinHeight = 780;
            }

            return layout;
        }

        public static async Task<List<ReportColumnConfigItem>> GetColumnsConfigAsync(string reportName)
        {
            var full = await GetFullTemplateConfigAsync(reportName);
            return full.Columns;
        }

        public static bool IsColumnVisible(this List<ReportColumnConfigItem>? configs, string colName, bool defaultVal = true)
        {
            return IsColumnVisible(configs, new[] { colName }, defaultVal);
        }

        public static bool IsColumnVisible(this List<ReportColumnConfigItem>? configs, string[] colAliases, bool defaultVal = true)
        {
            if (configs == null || configs.Count == 0) return defaultVal;
            foreach (var alias in colAliases)
            {
                if (string.IsNullOrWhiteSpace(alias)) continue;
                var item = configs.FirstOrDefault(c =>
                    c.Cot.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                    c.DataField.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                    c.Caption.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (item != null) return item.HienThi;
            }
            return defaultVal;
        }

        public static string GetColumnCaption(this List<ReportColumnConfigItem>? configs, string colName, string defaultCaption)
        {
            return GetColumnCaption(configs, new[] { colName }, defaultCaption);
        }

        public static string GetColumnCaption(this List<ReportColumnConfigItem>? configs, string[] colAliases, string defaultCaption)
        {
            if (configs == null || configs.Count == 0) return defaultCaption;
            foreach (var alias in colAliases)
            {
                if (string.IsNullOrWhiteSpace(alias)) continue;
                var item = configs.FirstOrDefault(c =>
                    c.Cot.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                    c.DataField.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                    c.Caption.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (item != null && !string.IsNullOrWhiteSpace(item.Caption)) return item.Caption;
            }
            return defaultCaption;
        }

        public static HorizontalAlignment GetColumnAlign(this List<ReportColumnConfigItem>? configs, string colName, HorizontalAlignment defaultAlign)
        {
            return GetColumnAlign(configs, new[] { colName }, defaultAlign);
        }

        public static HorizontalAlignment GetColumnAlign(this List<ReportColumnConfigItem>? configs, string[] colAliases, HorizontalAlignment defaultAlign)
        {
            if (configs == null || configs.Count == 0) return defaultAlign;
            foreach (var alias in colAliases)
            {
                if (string.IsNullOrWhiteSpace(alias)) continue;
                var item = configs.FirstOrDefault(c =>
                    c.Cot.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                    c.DataField.Equals(alias, StringComparison.OrdinalIgnoreCase) ||
                    c.Caption.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    if (item.CanLe.Equals("PHẢI", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Right;
                    if (item.CanLe.Equals("GIỮA", StringComparison.OrdinalIgnoreCase)) return HorizontalAlignment.Center;
                    return HorizontalAlignment.Left;
                }
            }
            return defaultAlign;
        }
    }
}
