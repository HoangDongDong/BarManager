using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace QuanLyBar.Client.Views.DanhMucNhaCungCap
{
    public partial class ChonFileExcelNhaCungCapWindow : Window
    {
        public string SelectedFilePath { get; private set; }

        public ChonFileExcelNhaCungCapWindow()
        {
            InitializeComponent();
        }

        private void BtnXuatMauHienThi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel 97-2003 (*.xls)|*.xls|Excel Workbook (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    FileName = "MauThemNhaCungCap.xls"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("NhaCungCap");
                        string[] headers = new[] { "Ghi chú", "Tên nhà cung cấp", "Nhóm nhà cung cấp", "Mã nhà cung cấp", "Địa chỉ", "Điện thoại", "Email", "Website" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Xuất file mẫu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file mẫu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXuatMauTatCa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel 97-2003 (*.xls)|*.xls|Excel Workbook (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    FileName = "MauThemNhaCungCapAll.xls"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("NhaCungCap");
                        string[] headers = new[] { "Ghi chú", "Tên nhà cung cấp", "Nhóm nhà cung cấp", "Mã nhà cung cấp", "Địa chỉ", "Điện thoại", "Email", "Website" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = ws.Cell(1, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Xuất file mẫu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file mẫu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChonFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*",
                Title = "Chọn file Excel dữ liệu Nhà cung cấp"
            };

            if (ofd.ShowDialog() == true)
            {
                SelectedFilePath = ofd.FileName;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
