using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class InBaoCaoWindow : Window
    {
        private LocalKhachDatHangService.DatHangSaveParam _order;
        private List<DatHangChiTietViewModel> _details;
        private string _phongBanName;
        private string _phuongThucDatName;
        private string _mucDichDatName;
        private bool _isPrintToPrinter;

        public InBaoCaoWindow(
            LocalKhachDatHangService.DatHangSaveParam order,
            List<DatHangChiTietViewModel> details,
            string phongBanName = "",
            string phuongThucDatName = "",
            string mucDichDatName = "",
            bool isPrintToPrinter = false)
        {
            InitializeComponent();
            _order = order ?? new LocalKhachDatHangService.DatHangSaveParam();
            _details = details ?? new List<DatHangChiTietViewModel>();
            _phongBanName = phongBanName;
            _phuongThucDatName = phuongThucDatName;
            _mucDichDatName = mucDichDatName;
            _isPrintToPrinter = isPrintToPrinter;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInstalledPrinters();
            if (_isPrintToPrinter)
            {
                RbInMayIn.IsChecked = true;
            }
            else
            {
                RbXemManHinh.IsChecked = true;
            }
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                var printers = new List<string>();
                try
                {
                    var printServer = new LocalPrintServer();
                    foreach (var pq in printServer.GetPrintQueues())
                    {
                        printers.Add(pq.Name);
                    }
                }
                catch { }

                CmbPrinters.ItemsSource = printers;
                if (printers.Count > 0)
                {
                    CmbPrinters.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (CmbPrinters != null)
            {
                CmbPrinters.IsEnabled = (RbInMayIn != null && RbInMayIn.IsChecked == true);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            string templateType = "54_2";
            if (CmbMauIn.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                templateType = selectedItem.Tag.ToString();
            }

            if (RbXemManHinh.IsChecked == true)
            {
                ExecuteXemManHinh(templateType);
            }
            else if (RbInMayIn.IsChecked == true)
            {
                ExecuteInMayIn(templateType);
            }
            else if (RbXuatExcel.IsChecked == true)
            {
                ExecuteXuatExcel();
            }
            else if (RbXuatPdf.IsChecked == true)
            {
                ExecuteXuatPdf(templateType);
            }
            else if (RbGuiMail.IsChecked == true)
            {
                MessageBox.Show("Tính năng gửi email đang được thiết lập kết nối máy chủ mail.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (RbThietKe.IsChecked == true)
            {
                MessageBox.Show("Chức năng thiết kế mẫu in trực quan đang được nâng cấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private FlowDocument CreateOrderFlowDocument(string templateType)
        {
            var doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Segoe UI, Arial");
            doc.FontSize = 12;

            double pageWidth;
            Thickness pagePadding;

            if (templateType == "54" || templateType == "54_2")
            {
                pageWidth = 220; // 54mm
                pagePadding = new Thickness(8);
                doc.FontSize = 10;
            }
            else if (templateType == "80")
            {
                pageWidth = 310; // 80mm
                pagePadding = new Thickness(12);
                doc.FontSize = 11;
            }
            else if (templateType == "A5")
            {
                pageWidth = 560; // A5
                pagePadding = new Thickness(25);
            }
            else // A4
            {
                pageWidth = 794; // A4
                pagePadding = new Thickness(35);
            }

            doc.PageWidth = pageWidth;
            doc.PagePadding = pagePadding;

            var section = new Section();

            // Header Công ty
            var pHeader = new Paragraph();
            pHeader.TextAlignment = TextAlignment.Center;
            pHeader.Margin = new Thickness(0, 0, 0, 5);
            pHeader.Inlines.Add(new Run("BAR & LOUNGE RESTAURANT\n") { FontWeight = FontWeights.Bold, FontSize = doc.FontSize + 2 });
            pHeader.Inlines.Add(new Run("Địa chỉ: 12 Xuân Thủy, Cầu Giấy, Hà Nội\n") { FontSize = doc.FontSize - 1 });
            pHeader.Inlines.Add(new Run("Điện thoại: (024) 3756 8888\n") { FontSize = doc.FontSize - 1 });
            section.Blocks.Add(pHeader);

            // Đường kẻ
            var pLine1 = new Paragraph();
            pLine1.Margin = new Thickness(0, 2, 0, 6);
            pLine1.BorderBrush = Brushes.Black;
            pLine1.BorderThickness = new Thickness(0, 1, 0, 0);
            section.Blocks.Add(pLine1);

            // Tiêu đề phiếu
            var pTitle = new Paragraph();
            pTitle.TextAlignment = TextAlignment.Center;
            pTitle.Margin = new Thickness(0, 0, 0, 8);
            pTitle.Inlines.Add(new Run("PHIẾU ĐẶT HÀNG\n") { FontWeight = FontWeights.Bold, FontSize = doc.FontSize + 4 });
            pTitle.Inlines.Add(new Run($"Số phiếu: {_order.Name ?? "---"}\n") { FontWeight = FontWeights.SemiBold, FontStyle = FontStyles.Italic });
            pTitle.Inlines.Add(new Run($"Ngày: {(_order.Ngay.HasValue ? _order.Ngay.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy"))}") { FontSize = doc.FontSize - 1 });
            section.Blocks.Add(pTitle);

            // Thông tin chi tiết đơn
            var pInfo = new Paragraph();
            pInfo.Margin = new Thickness(0, 0, 0, 8);
            pInfo.LineHeight = doc.FontSize * 1.5;
            
            pInfo.Inlines.Add(new Run($"• Khách hàng: ") { FontWeight = FontWeights.Bold });
            pInfo.Inlines.Add(new Run($"{_order.Tenkhach ?? "Khách lẻ"}\n"));

            if (!string.IsNullOrWhiteSpace(_order.Dienthoai))
            {
                pInfo.Inlines.Add(new Run($"• Điện thoại: ") { FontWeight = FontWeights.Bold });
                pInfo.Inlines.Add(new Run($"{_order.Dienthoai}\n"));
            }

            if (!string.IsNullOrWhiteSpace(_order.Diachi))
            {
                pInfo.Inlines.Add(new Run($"• Địa chỉ: ") { FontWeight = FontWeights.Bold });
                pInfo.Inlines.Add(new Run($"{_order.Diachi}\n"));
            }

            if (!string.IsNullOrWhiteSpace(_phongBanName))
            {
                pInfo.Inlines.Add(new Run($"• Phòng/Bàn: ") { FontWeight = FontWeights.Bold });
                pInfo.Inlines.Add(new Run($"{_phongBanName}\n"));
            }

            string tuGio = _order.Tugio.HasValue ? _order.Tugio.Value.ToString("HH:mm") : "";
            string denGio = _order.Dengio.HasValue ? _order.Dengio.Value.ToString("HH:mm") : "";
            if (!string.IsNullOrEmpty(tuGio) || !string.IsNullOrEmpty(denGio))
            {
                pInfo.Inlines.Add(new Run($"• Thời gian: ") { FontWeight = FontWeights.Bold });
                pInfo.Inlines.Add(new Run($"{tuGio} - {denGio}\n"));
            }

            if (!string.IsNullOrWhiteSpace(_phuongThucDatName))
            {
                pInfo.Inlines.Add(new Run($"• Đặt qua: ") { FontWeight = FontWeights.Bold });
                pInfo.Inlines.Add(new Run($"{_phuongThucDatName}  "));
            }
            if (!string.IsNullOrWhiteSpace(_mucDichDatName))
            {
                pInfo.Inlines.Add(new Run($"• Mục đích: ") { FontWeight = FontWeights.Bold });
                pInfo.Inlines.Add(new Run($"{_mucDichDatName}\n"));
            }

            section.Blocks.Add(pInfo);

            // Bảng danh sách mặt hàng
            var table = new Table();
            table.CellSpacing = 0;
            table.BorderBrush = Brushes.Black;
            table.BorderThickness = new Thickness(0, 1, 0, 1);
            table.Margin = new Thickness(0, 0, 0, 8);

            bool isSmallFormat = (templateType == "54" || templateType == "54_2" || templateType == "80");

            if (isSmallFormat)
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(pageWidth * 0.45) });
                table.Columns.Add(new TableColumn { Width = new GridLength(pageWidth * 0.15) });
                table.Columns.Add(new TableColumn { Width = new GridLength(pageWidth * 0.40) });
            }
            else
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(40) }); // STT
                table.Columns.Add(new TableColumn { Width = new GridLength(200) }); // Tên
                table.Columns.Add(new TableColumn { Width = new GridLength(60) }); // ĐVT
                table.Columns.Add(new TableColumn { Width = new GridLength(50) }); // SL
                table.Columns.Add(new TableColumn { Width = new GridLength(90) }); // Đơn giá
                table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Thành tiền
            }

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            // Table Header
            var headerRow = new TableRow();
            if (isSmallFormat)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Tên món")) { FontWeight = FontWeights.Bold }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("SL")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("T.Tiền")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
            }
            else
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("STT")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Tên mặt hàng")) { FontWeight = FontWeights.Bold }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("ĐVT")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("SL")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Đơn giá")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Thành tiền")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
            }
            rowGroup.Rows.Add(headerRow);

            // Table Data Rows
            int stt = 1;
            foreach (var item in _details)
            {
                var row = new TableRow();
                decimal tt = item.ThanhTien ?? ((item.SoLuong ?? 0) * (item.DonGia ?? 0));
                
                if (isSmallFormat)
                {
                    string nameText = templateType == "54_2" ? $"{stt}. {item.MatHangName}" : item.MatHangName;
                    row.Cells.Add(new TableCell(new Paragraph(new Run(nameText))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run((item.SoLuong ?? 1).ToString("N0"))) { TextAlignment = TextAlignment.Center }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(tt.ToString("N0"))) { TextAlignment = TextAlignment.Right }));
                }
                else
                {
                    row.Cells.Add(new TableCell(new Paragraph(new Run(stt.ToString())) { TextAlignment = TextAlignment.Center }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.MatHangName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.DonViTinhName ?? "")) { TextAlignment = TextAlignment.Center }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run((item.SoLuong ?? 1).ToString("N0"))) { TextAlignment = TextAlignment.Center }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run((item.DonGia ?? 0).ToString("N0"))) { TextAlignment = TextAlignment.Right }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(tt.ToString("N0"))) { TextAlignment = TextAlignment.Right }));
                }
                rowGroup.Rows.Add(row);
                stt++;
            }

            section.Blocks.Add(table);

            // Tổng tiền & chi phí
            var pTotals = new Paragraph();
            pTotals.Margin = new Thickness(0, 0, 0, 10);
            pTotals.TextAlignment = TextAlignment.Right;
            pTotals.LineHeight = doc.FontSize * 1.6;

            decimal tienHang = _details.Sum(x => x.ThanhTien ?? ((x.SoLuong ?? 0) * (x.DonGia ?? 0)));
            pTotals.Inlines.Add(new Run($"Tiền hàng: {tienHang:N0} đ\n"));

            if (_order.Tiengiamgia.HasValue && _order.Tiengiamgia.Value > 0)
            {
                pTotals.Inlines.Add(new Run($"Giảm giá ({_order.Tilegiamgia ?? 0}%): -{_order.Tiengiamgia.Value:N0} đ\n"));
            }
            if (_order.Tienthue.HasValue && _order.Tienthue.Value > 0)
            {
                pTotals.Inlines.Add(new Run($"Thuế ({_order.Tilethue ?? 0}%): +{_order.Tienthue.Value:N0} đ\n"));
            }
            if (decimal.TryParse(_order.Phivanchuyen, out decimal pvc) && pvc > 0)
            {
                pTotals.Inlines.Add(new Run($"Phí vận chuyển: +{pvc:N0} đ\n"));
            }

            decimal tongCong = tienHang;
            if (decimal.TryParse(_order.Tongcong?.Replace(",", "")?.Replace(".", ""), out decimal tc) && tc > 0)
            {
                tongCong = tc;
            }
            else
            {
                tongCong = (tienHang - (_order.Tiengiamgia ?? 0)) + (_order.Tienthue ?? 0) + pvc;
            }

            pTotals.Inlines.Add(new Run($"TỔNG CỘNG: {tongCong:N0} đ\n") { FontWeight = FontWeights.Bold, FontSize = doc.FontSize + 2, Foreground = Brushes.DarkRed });
            section.Blocks.Add(pTotals);

            // Ghi chú nếu có
            if (!string.IsNullOrWhiteSpace(_order.Note))
            {
                var pNote = new Paragraph();
                pNote.Margin = new Thickness(0, 0, 0, 10);
                pNote.Inlines.Add(new Run($"Ghi chú: {_order.Note}") { FontStyle = FontStyles.Italic });
                section.Blocks.Add(pNote);
            }

            // Footer ký tên cho A4/A5
            if (!isSmallFormat)
            {
                var pSign = new Paragraph();
                pSign.Margin = new Thickness(0, 20, 0, 0);
                pSign.TextAlignment = TextAlignment.Center;
                
                var signTable = new Table();
                signTable.Columns.Add(new TableColumn());
                signTable.Columns.Add(new TableColumn());
                var signGroup = new TableRowGroup();
                signTable.RowGroups.Add(signGroup);
                
                var signRow = new TableRow();
                var cell1 = new TableCell(new Paragraph(new Run("Khách hàng ký\n(Ký, ghi rõ họ tên)\n\n\n\n__________________")) { TextAlignment = TextAlignment.Center });
                var cell2 = new TableCell(new Paragraph(new Run("Người lập phiếu\n(Ký, ghi rõ họ tên)\n\n\n\n__________________")) { TextAlignment = TextAlignment.Center });
                signRow.Cells.Add(cell1);
                signRow.Cells.Add(cell2);
                signGroup.Rows.Add(signRow);
                section.Blocks.Add(signTable);
            }
            else
            {
                var pThank = new Paragraph();
                pThank.TextAlignment = TextAlignment.Center;
                pThank.Margin = new Thickness(0, 10, 0, 0);
                pThank.Inlines.Add(new Run("--- Cảm ơn Quý khách! Hẹn gặp lại! ---") { FontStyle = FontStyles.Italic });
                section.Blocks.Add(pThank);
            }

            doc.Blocks.Add(section);
            return doc;
        }

        private void ExecuteXemManHinh(string templateType)
        {
            var doc = CreateOrderFlowDocument(templateType);
            var win = new Window
            {
                Title = $"Xem in phiếu đặt hàng - {_order.Name}",
                Width = 700,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var viewer = new FlowDocumentReader
            {
                Document = doc,
                ViewingMode = FlowDocumentReaderViewingMode.Page,
                IsFindEnabled = true,
                IsPrintEnabled = true
            };

            win.Content = viewer;
            win.Owner = this;
            win.ShowDialog();
        }

        private void ExecuteInMayIn(string templateType)
        {
            try
            {
                var doc = CreateOrderFlowDocument(templateType);
                var printDialog = new PrintDialog();

                string selectedPrinter = CmbPrinters.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectedPrinter))
                {
                    try
                    {
                        var printServer = new LocalPrintServer();
                        var pq = printServer.GetPrintQueue(selectedPrinter);
                        if (pq != null) printDialog.PrintQueue = pq;
                    }
                    catch { }
                }

                IDocumentPaginatorSource dps = doc;
                printDialog.PrintDocument(dps.DocumentPaginator, $"In đơn đặt hàng {_order.Name}");
                MessageBox.Show("Đã gửi lệnh in tới máy in!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi in: " + ex.Message, "Lỗi in ấn", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteXuatExcel()
        {
            try
            {
                string defaultFileName = $"PhieuDatHang_{_order.Name?.Replace("/", "_") ?? DateTime.Now.ToString("yyyyMMddHHmm")}.xlsx";
                var saveDlg = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = defaultFileName,
                    Title = "Xuất phiếu đặt hàng ra Excel"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Phiếu Đặt Hàng");

                        // Tiêu đề cửa hàng
                        ws.Cell(1, 1).Value = "BAR & LOUNGE RESTAURANT";
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 14;

                        ws.Cell(2, 1).Value = "Địa chỉ: 12 Xuân Thủy, Cầu Giấy, Hà Nội - Hotline: (024) 3756 8888";
                        ws.Cell(2, 1).Style.Font.Italic = true;

                        // Tiêu đề phiếu
                        ws.Cell(4, 1).Value = "PHIẾU ĐẶT HÀNG";
                        ws.Cell(4, 1).Style.Font.Bold = true;
                        ws.Cell(4, 1).Style.Font.FontSize = 16;
                        ws.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(4, 1, 4, 7).Merge();

                        // Thông tin đơn hàng
                        ws.Cell(5, 1).Value = $"Số phiếu: {_order.Name}";
                        ws.Cell(5, 1).Style.Font.Bold = true;
                        ws.Cell(5, 5).Value = $"Ngày: {(_order.Ngay.HasValue ? _order.Ngay.Value.ToString("dd/MM/yyyy") : "")}";

                        ws.Cell(6, 1).Value = $"Khách hàng: {_order.Tenkhach}";
                        ws.Cell(6, 5).Value = $"Điện thoại: {_order.Dienthoai}";

                        ws.Cell(7, 1).Value = $"Địa chỉ: {_order.Diachi}";
                        ws.Cell(7, 5).Value = $"Phòng/Bàn: {_phongBanName}";

                        string tuGio = _order.Tugio.HasValue ? _order.Tugio.Value.ToString("HH:mm") : "";
                        string denGio = _order.Dengio.HasValue ? _order.Dengio.Value.ToString("HH:mm") : "";
                        ws.Cell(8, 1).Value = $"Giờ vào - ra: {tuGio} - {denGio}";
                        ws.Cell(8, 5).Value = $"Phương thức: {_phuongThucDatName} / {_mucDichDatName}";

                        // Bảng mặt hàng
                        int startRow = 10;
                        ws.Cell(startRow, 1).Value = "STT";
                        ws.Cell(startRow, 2).Value = "Tên mặt hàng";
                        ws.Cell(startRow, 3).Value = "ĐVT";
                        ws.Cell(startRow, 4).Value = "Số lượng";
                        ws.Cell(startRow, 5).Value = "Đơn giá";
                        ws.Cell(startRow, 6).Value = "Giảm giá %";
                        ws.Cell(startRow, 7).Value = "Thành tiền";

                        var headerRange = ws.Range(startRow, 1, startRow, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#3a75c4");
                        headerRange.Style.Font.FontColor = XLColor.White;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        int currentRow = startRow + 1;
                        int stt = 1;
                        foreach (var item in _details)
                        {
                            decimal tt = item.ThanhTien ?? ((item.SoLuong ?? 0) * (item.DonGia ?? 0));
                            ws.Cell(currentRow, 1).Value = stt++;
                            ws.Cell(currentRow, 2).Value = item.MatHangName;
                            ws.Cell(currentRow, 3).Value = item.DonViTinhName ?? "";
                            ws.Cell(currentRow, 4).Value = item.SoLuong ?? 1;
                            ws.Cell(currentRow, 5).Value = item.DonGia ?? 0;
                            ws.Cell(currentRow, 6).Value = item.GiamGiaPhanTram ?? 0;
                            ws.Cell(currentRow, 7).Value = tt;

                            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";

                            currentRow++;
                        }

                        // Border bảng dữ liệu
                        var dataTableRange = ws.Range(startRow, 1, currentRow - 1, 7);
                        dataTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Tổng cộng
                        decimal tienHang = _details.Sum(x => x.ThanhTien ?? ((x.SoLuong ?? 0) * (x.DonGia ?? 0)));
                        ws.Cell(currentRow, 6).Value = "Tiền hàng:";
                        ws.Cell(currentRow, 6).Style.Font.Bold = true;
                        ws.Cell(currentRow, 7).Value = tienHang;
                        ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                        currentRow++;

                        if (_order.Tiengiamgia.HasValue && _order.Tiengiamgia.Value > 0)
                        {
                            ws.Cell(currentRow, 6).Value = $"Giảm giá ({_order.Tilegiamgia ?? 0}%):";
                            ws.Cell(currentRow, 7).Value = _order.Tiengiamgia.Value;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "-#,##0";
                            currentRow++;
                        }

                        if (_order.Tienthue.HasValue && _order.Tienthue.Value > 0)
                        {
                            ws.Cell(currentRow, 6).Value = $"Thuế VAT ({_order.Tilethue ?? 0}%):";
                            ws.Cell(currentRow, 7).Value = _order.Tienthue.Value;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "+#,##0";
                            currentRow++;
                        }

                        if (decimal.TryParse(_order.Phivanchuyen, out decimal pvc) && pvc > 0)
                        {
                            ws.Cell(currentRow, 6).Value = "Phí vận chuyển:";
                            ws.Cell(currentRow, 7).Value = pvc;
                            ws.Cell(currentRow, 7).Style.NumberFormat.Format = "+#,##0";
                            currentRow++;
                        }

                        decimal tongCong = tienHang;
                        if (decimal.TryParse(_order.Tongcong?.Replace(",", "")?.Replace(".", ""), out decimal tc) && tc > 0)
                        {
                            tongCong = tc;
                        }
                        else
                        {
                            tongCong = (tienHang - (_order.Tiengiamgia ?? 0)) + (_order.Tienthue ?? 0) + pvc;
                        }

                        ws.Cell(currentRow, 6).Value = "TỔNG CỘNG:";
                        ws.Cell(currentRow, 6).Style.Font.Bold = true;
                        ws.Cell(currentRow, 6).Style.Font.FontSize = 12;
                        ws.Cell(currentRow, 7).Value = tongCong;
                        ws.Cell(currentRow, 7).Style.Font.Bold = true;
                        ws.Cell(currentRow, 7).Style.Font.FontSize = 12;
                        ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                        currentRow += 2;

                        // Ghi chú
                        if (!string.IsNullOrWhiteSpace(_order.Note))
                        {
                            ws.Cell(currentRow, 1).Value = $"Ghi chú: {_order.Note}";
                            ws.Cell(currentRow, 1).Style.Font.Italic = true;
                            currentRow += 2;
                        }

                        // Ký tên
                        ws.Cell(currentRow, 2).Value = "Khách hàng";
                        ws.Cell(currentRow, 2).Style.Font.Bold = true;
                        ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(currentRow, 6).Value = "Người lập phiếu";
                        ws.Cell(currentRow, 6).Style.Font.Bold = true;
                        ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Columns().AdjustToContents();

                        workbook.SaveAs(saveDlg.FileName);
                    }

                    var result = MessageBox.Show("Xuất file Excel thành công! Bạn có muốn mở file ngay không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteXuatPdf(string templateType)
        {
            try
            {
                var doc = CreateOrderFlowDocument(templateType);
                var printDialog = new PrintDialog();
                
                // Try to find Microsoft Print to PDF
                try
                {
                    var printServer = new LocalPrintServer();
                    var pdfPq = printServer.GetPrintQueues().FirstOrDefault(x => x.Name.IndexOf("PDF", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (pdfPq != null) printDialog.PrintQueue = pdfPq;
                }
                catch { }

                if (printDialog.ShowDialog() == true)
                {
                    IDocumentPaginatorSource dps = doc;
                    printDialog.PrintDocument(dps.DocumentPaginator, $"DonDatHang_{_order.Name}");
                    MessageBox.Show("Đã xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất PDF: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
