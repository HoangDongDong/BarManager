using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using ClosedXML.Excel;
using ExcelDataReader;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ImportDinhLuongWindow : Window
    {
        public ImportDinhLuongWindow()
        {
            InitializeComponent();
        }

        private void BtnXuatFileMau_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Lưu file mẫu định lượng",
                FileName = "Mau_DinhLuong.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("DinhLuong");
                        
                        // Header
                        worksheet.Cell(1, 1).Value = "Tên hàng";
                        worksheet.Cell(1, 2).Value = "Tên nguyên liệu";
                        worksheet.Cell(1, 3).Value = "Số lượng";
                        
                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }
                    
                    var result = MessageBox.Show("Xuất file mẫu thành công! Bạn có muốn mở file vừa xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file mẫu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx",
                Title = "Chọn file Excel chứa định lượng"
            };

            if (ofd.ShowDialog() == true)
            {
                string filePath = ofd.FileName;
                var actualColumns = new List<string>();
                System.Data.DataTable dataTable = null;

                try
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = true
                                }
                            });
                            dataTable = result.Tables[0];
                            foreach (System.Data.DataColumn col in dataTable.Columns)
                            {
                                actualColumns.Add(col.ColumnName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var customFields = new List<string> { "Tên hàng", "Tên nguyên liệu", "Số lượng" };
                var mappingWin = new MappingExcelWindow(actualColumns, customFields);
                if (mappingWin.ShowDialog() == true)
                {
                    var mappings = mappingWin.MappingList.Where(m => !string.IsNullOrEmpty(m.MappedField)).ToList();
                    
                    var allData = new List<DinhLuongImportViewModel>();
                    
                    var mhService = new LocalMatHangService();
                    var allItems = await mhService.GetMatHangListAsync(null);
                    
                    int stt = 1;
                    foreach (System.Data.DataRow row in dataTable.Rows)
                    {
                        var dl = new DinhLuongImportViewModel { Stt = stt++ };
                        
                        foreach (var map in mappings)
                        {
                            string val = row[map.ExcelColumn]?.ToString()?.Trim() ?? "";
                            if (map.MappedField == "Tên hàng") dl.MatHangName = val;
                            if (map.MappedField == "Tên nguyên liệu") dl.NguyenLieuName = val;
                            if (map.MappedField == "Số lượng" && decimal.TryParse(val, out decimal sl)) dl.SoLuong = sl;
                        }
                        
                        // Lookup
                        var matHang = allItems.FirstOrDefault(x => string.Equals(x.Name, dl.MatHangName, StringComparison.OrdinalIgnoreCase));
                        if (matHang != null)
                        {
                            dl.DmathangId = matHang.Id;
                            dl.MatHangDVT = matHang.DonViTinhName;
                            dl.NhomMatHangId = matHang.DnhommathangId;
                        }
                        
                        var nguyenLieu = allItems.FirstOrDefault(x => string.Equals(x.Name, dl.NguyenLieuName, StringComparison.OrdinalIgnoreCase));
                        if (nguyenLieu != null)
                        {
                            dl.DvattuId = nguyenLieu.Id;
                            dl.NguyenLieuDVT = nguyenLieu.DonViTinhName;
                        }
                        
                        allData.Add(dl);
                    }

                    // Validation check for missing items
                    var missingMatHang = allData.FirstOrDefault(x => string.IsNullOrEmpty(x.DmathangId));
                    var missingNguyenLieu = allData.FirstOrDefault(x => string.IsNullOrEmpty(x.DvattuId));

                    if (missingMatHang != null || missingNguyenLieu != null)
                    {
                        string msg = "";
                        if (missingMatHang != null)
                        {
                            msg = $"Tên hàng: '{missingMatHang.MatHangName}' không tồn tại trong hệ thống\n\n";
                        }
                        else
                        {
                            msg = $"Tên nguyên liệu: '{missingNguyenLieu.NguyenLieuName}' không tồn tại trong hệ thống\n\n";
                        }
                        
                        msg += "Bạn có muốn thực hiện tiếp không? Hay điều chỉnh lại file excel và thực hiện lại?";

                        var msgResult = MessageBox.Show(msg, "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgResult != MessageBoxResult.Yes)
                        {
                            return; // Stop here, don't open preview
                        }
                        
                        // Remove invalid rows
                        allData.RemoveAll(x => string.IsNullOrEmpty(x.DmathangId) || string.IsNullOrEmpty(x.DvattuId));
                        for (int i = 0; i < allData.Count; i++)
                        {
                            allData[i].Stt = i + 1;
                        }
                    }

                    this.Hide();
                    var previewWin = new NhapDinhLuongTuExcelWindow(allData);
                    previewWin.ShowDialog();
                    this.Close();
                }
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
