using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
    public partial class PrintPreviewWindow : Window
    {
        public PrintPreviewWindow(IEnumerable<object> data, List<InLuoiWindow.ColumnInfo> columns, string title, string note, string templateType, bool inSTT)
        {
            InitializeComponent();
            GenerateDocument(data, columns, title, note, templateType, inSTT);
        }

        private void GenerateDocument(IEnumerable<object> data, List<InLuoiWindow.ColumnInfo> columns, string title, string note, string templateType, bool inSTT)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Arial");
            doc.FontSize = 12;

            // Common orange color used in the templates
            var orangeBrush = new SolidColorBrush(Color.FromRgb(255, 165, 0)); // Or Brushes.Orange

            // Apply page border (wrapper block)
            var pageWrapper = new Section();
            pageWrapper.BorderBrush = orangeBrush;
            pageWrapper.BorderThickness = new Thickness(2);
            pageWrapper.Padding = new Thickness(20);

            // Set Page Size based on template
            if (templateType == "Mẫu 80")
            {
                doc.PageWidth = 302; // 80mm
                doc.PagePadding = new Thickness(10);
            }
            else if (templateType == "Mẫu A4 nằm ngang")
            {
                doc.PageWidth = 1122; // A4 Landscape
                doc.PageHeight = 794;
                doc.PagePadding = new Thickness(30);
            }
            else // Mẫu A4 thẳng đứng
            {
                doc.PageWidth = 794; // A4 Portrait
                doc.PageHeight = 1122;
                doc.PagePadding = new Thickness(30);
            }

            // Header Section
            var headerPara = new Paragraph();
            headerPara.TextAlignment = TextAlignment.Center;
            headerPara.Inlines.Add(new Run("(TÊN CÔNG TY)\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
            headerPara.Inlines.Add(new Run("Địa chỉ: (ĐỊA CHỈ)\n"));
            headerPara.Inlines.Add(new Run("Điện thoại: (ĐIỆN THOẠI), Email: (EMAIL)"));
            
            pageWrapper.Blocks.Add(headerPara);

            // Orange Separator Line
            var separator = new Paragraph();
            separator.BorderBrush = orangeBrush;
            separator.BorderThickness = new Thickness(0, 1, 0, 0);
            separator.Margin = new Thickness(0, 5, 0, 10);
            pageWrapper.Blocks.Add(separator);

            // Title Section
            var titlePara = new Paragraph();
            var titleRun = new Run(string.IsNullOrEmpty(title) ? "Mặt hàng" : title) 
            { 
                FontWeight = FontWeights.Bold, 
                FontSize = 18 
            };
            titlePara.Inlines.Add(titleRun);

            // Align title based on template
            if (templateType == "Mẫu 80")
            {
                titlePara.TextAlignment = TextAlignment.Center;
            }
            else
            {
                titlePara.TextAlignment = TextAlignment.Right;
            }

            if (!string.IsNullOrEmpty(note))
            {
                titlePara.Inlines.Add(new Run("\n" + note) { FontStyle = FontStyles.Italic, FontSize = 12, FontWeight = FontWeights.Normal });
            }
            pageWrapper.Blocks.Add(titlePara);

            // Table
            var table = new Table();
            table.CellSpacing = 0;
            table.BorderBrush = Brushes.Black;
            table.BorderThickness = new Thickness(1, 1, 0, 0);

            var activeColumns = columns.Where(c => c.IsChecked).ToList();

            // Calculate absolute column widths to avoid Star sizing bugs in FlowDocument inside Section
            double availableWidth = doc.PageWidth 
                - doc.PagePadding.Left - doc.PagePadding.Right
                - pageWrapper.Padding.Left - pageWrapper.Padding.Right
                - pageWrapper.BorderThickness.Left - pageWrapper.BorderThickness.Right;

            double sttWidth = inSTT ? 40 : 0;
            double remainingWidth = availableWidth - sttWidth;
            if (remainingWidth < 100) remainingWidth = 100; // Fallback

            double totalWeight = 0;
            foreach (var col in activeColumns)
            {
                if (col.Header == "Tên mặt hàng") totalWeight += 3;
                else totalWeight += 1;
            }

            // Columns definition
            if (inSTT)
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(sttWidth) });
            }

            foreach (var col in activeColumns)
            {
                double weight = (col.Header == "Tên mặt hàng") ? 3 : 1;
                double colWidth = (weight / totalWeight) * remainingWidth;
                table.Columns.Add(new TableColumn { Width = new GridLength(colWidth) });
            }

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            // Header Row
            var headerRow = new TableRow();
            if (inSTT)
            {
                var cell = new TableCell(new Paragraph(new Run("STT")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center })
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(3)
                };
                headerRow.Cells.Add(cell);
            }

            foreach (var col in activeColumns)
            {
                var cell = new TableCell(new Paragraph(new Run(col.Header)) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center })
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(3)
                };
                headerRow.Cells.Add(cell);
            }
            rowGroup.Rows.Add(headerRow);

            // Data Rows
            int stt = 1;
            foreach (var item in data)
            {
                var row = new TableRow();
                
                if (inSTT)
                {
                    var cell = new TableCell(new Paragraph(new Run(stt.ToString())) { TextAlignment = TextAlignment.Center })
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Padding = new Thickness(3)
                    };
                    row.Cells.Add(cell);
                }

                foreach (var col in activeColumns)
                {
                    string val = GetPropertyValueByHeader(item, col.Header);
                    
                    var cell = new TableCell(new Paragraph(new Run(val)))
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Padding = new Thickness(3)
                    };
                    row.Cells.Add(cell);
                }
                rowGroup.Rows.Add(row);
                stt++;
            }

            pageWrapper.Blocks.Add(table);

            // Footer
            var footerTable = new Table();
            footerTable.Margin = new Thickness(0, 20, 0, 50);
            footerTable.Columns.Add(new TableColumn());
            footerTable.Columns.Add(new TableColumn());
            var footerRowGroup = new TableRowGroup();
            footerTable.RowGroups.Add(footerRowGroup);
            
            var fRow = new TableRow();
            
            var leftPara = new Paragraph();
            leftPara.TextAlignment = TextAlignment.Center;
            leftPara.Inlines.Add(new Run("Trưởng phòng\n") { FontWeight = FontWeights.Bold });
            leftPara.Inlines.Add(new Run("(Ký, họ tên)\n\n\n\n__________________"));
            fRow.Cells.Add(new TableCell(leftPara));
            
            var rightPara = new Paragraph();
            rightPara.TextAlignment = TextAlignment.Center;
            rightPara.Inlines.Add(new Run("Người lập\n") { FontWeight = FontWeights.Bold });
            rightPara.Inlines.Add(new Run("(Ký, họ tên)\n\n\n\n__________________"));
            fRow.Cells.Add(new TableCell(rightPara));
            
            footerRowGroup.Rows.Add(fRow);
            
            pageWrapper.Blocks.Add(footerTable);

            doc.Blocks.Add(pageWrapper);
            DocViewer.Document = doc;
        }

        private string GetPropertyValueByHeader(object item, string header)
        {
            var type = item.GetType();
            string propName = "";
            switch (header)
            {
                case "Tên mặt hàng": propName = "Name"; break;
                case "Nhóm mặt hàng": propName = "NhomMatHangName"; break;
                case "Loại mặt hàng": propName = "LoaiMatHangName"; break;
                case "Đơn vị tính": propName = "DonViTinhName"; break;
                case "Giá bán": propName = "Giaban"; break;
                case "Giá nhập": propName = "Gianhap"; break;
                case "ĐVT chẵn": propName = "DonViTinhChanName"; break;
                case "Quy đổi": propName = "Quydoi"; break;
                case "Giá bán chẵn": propName = "Giabanchan"; break;
                case "Mã hàng": propName = "Code"; break;
                case "Tạm khóa": propName = "Tamkhoa"; break;
                case "Giá theo thời giá": propName = "Giatheothoigia"; break;
                // Bàn
                case "Tên bàn": propName = "Name"; break;
                case "Khu vực": propName = "KhuVucName"; break;
                case "Nhóm hiển thị": propName = "NhomHienThiName"; break;
                case "Loại phòng": propName = "LoaiPhongName"; break;
                case "Ghi chú": propName = "Note"; break;
                // Khách đặt hàng
                case "Ngày": propName = "Ngay"; break;
                case "Số phiếu": propName = "SoPhieu"; break;
                case "Tên khách": propName = "TenKhach"; break;
                case "Địa chỉ": propName = "DiaChi"; break;
                case "Điện thoại": propName = "DienThoai"; break;
                case "Email": propName = "Email"; break;
                case "Tổng cộng": propName = "TongCong"; break;
                case "Phương thức đặt": propName = "PhuongThucDatName"; break;
                case "Mục đích đặt": propName = "MucDichDatName"; break;
                case "Từ giờ": propName = "TuGio"; break;
                case "Đến giờ": propName = "DenGio"; break;
                case "Từ ngày": propName = "TuNgay"; break;
                case "Đến ngày": propName = "DenNgay"; break;
            }

            if (!string.IsNullOrEmpty(propName))
            {
                var prop = type.GetProperty(propName);
                if (prop != null)
                {
                    var val = prop.GetValue(item);
                    if (val is System.DateTime dt)
                    {
                        if (header == "Từ giờ" || header == "Đến giờ")
                            return dt.ToString("HH:mm");
                        return dt.ToString("dd/MM/yyyy");
                    }
                    return val != null ? val.ToString() : "";
                }
            }
            return "";
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            DocViewer.Print();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng xuất file (Excel, PDF) có thể được tích hợp thêm ở đây.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
