using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace QuanLyBar.Client.Views
{
    public partial class ChonFileExcelKhachHangWindow : Window
    {
        public string SelectedFilePath { get; private set; }

        public ChonFileExcelKhachHangWindow()
        {
            InitializeComponent();
        }

        private void BtnXuatMauHienThi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xls)|*.xls|All Files (*.*)|*.*",
                    FileName = "MauThemKhach.xls"
                };

                if (sfd.ShowDialog() == true)
                {
                    string defaultTemplate = @"d:\QuanLyBar\MauThemKhach.xls";
                    if (File.Exists(defaultTemplate))
                    {
                        File.Copy(defaultTemplate, sfd.FileName, true);
                    }
                    else
                    {
                        // Fallback create using ClosedXML
                        using (var workbook = new ClosedXML.Excel.XLWorkbook())
                        {
                            var ws = workbook.Worksheets.Add("KhachHang");
                            string[] headers = new[] { "Tên khách hàng", "Nhóm khách hàng", "Mã khách", "Địa chỉ", "Điện thoại", "Email", "Mã số thuế", "Nhân viên", "Tỉnh thành", "Facebook", "Thẻ trả trước" };
                            for (int i = 0; i < headers.Length; i++)
                            {
                                ws.Cell(1, i + 1).Value = headers[i];
                            }
                            workbook.SaveAs(sfd.FileName);
                        }
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
                    Filter = "Excel Files (*.xls)|*.xls|All Files (*.*)|*.*",
                    FileName = "MauThemKhachAll.xls"
                };

                if (sfd.ShowDialog() == true)
                {
                    string defaultTemplate = @"d:\QuanLyBar\MauThemKhachAll.xls";
                    if (File.Exists(defaultTemplate))
                    {
                        File.Copy(defaultTemplate, sfd.FileName, true);
                    }
                    else
                    {
                        using (var workbook = new ClosedXML.Excel.XLWorkbook())
                        {
                            var ws = workbook.Worksheets.Add("KhachHang");
                            string[] headers = new[] { "Ghi chú", "Tên khách hàng", "Nhóm khách hàng", "Mã khách", "Địa chỉ", "Điện thoại", "Email", "Mã số thuế", "Nhân viên", "Điểm tích lũy ban đầu", "Ngày thành lập/sinh nhật", "Tỉnh thành", "Facebook", "Thẻ trả trước" };
                            for (int i = 0; i < headers.Length; i++)
                            {
                                ws.Cell(1, i + 1).Value = headers[i];
                            }
                            workbook.SaveAs(sfd.FileName);
                        }
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
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"
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
