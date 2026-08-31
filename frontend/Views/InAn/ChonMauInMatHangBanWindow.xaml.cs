using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class ChonMauInMatHangBanWindow : Window
    {
        private readonly DataGrid _dataGrid;
        private readonly string _reportTitle;
        private readonly string _storeName;
        private readonly DateTime _tuNgay;
        private readonly DateTime _denNgay;

        public ChonMauInMatHangBanWindow(
            DataGrid dataGrid,
            string reportTitle = "BÁO CÁO THỐNG KÊ MẶT HÀNG BÁN",
            string storeName = "NÀNG HƯƠNG QUÁN",
            DateTime? tuNgay = null,
            DateTime? denNgay = null)
        {
            InitializeComponent();
            _dataGrid = dataGrid;
            _reportTitle = reportTitle;
            _storeName = storeName;
            _tuNgay = tuNgay ?? DateTime.Today;
            _denNgay = denNgay ?? DateTime.Today;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInstalledPrinters();
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                CmbPrinters.Items.Clear();
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    CmbPrinters.Items.Add(printer);
                }

                if (CmbPrinters.Items.Count > 0)
                {
                    var printDoc = new PrintDocument();
                    string defaultPrinter = printDoc.PrinterSettings.PrinterName;
                    int defIndex = CmbPrinters.Items.IndexOf(defaultPrinter);
                    CmbPrinters.SelectedIndex = defIndex >= 0 ? defIndex : 0;
                }
            }
            catch
            {
                CmbPrinters.Items.Add("Microsoft Print to PDF");
                CmbPrinters.SelectedIndex = 0;
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (CmbPrinters != null)
            {
                CmbPrinters.IsEnabled = (RbInMayIn.IsChecked == true);
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            string selectedTemplateName = "A4";
            if (TvMauIn.SelectedItem is TreeViewItem selItem && selItem.Header is StackPanel sp)
            {
                foreach (var child in sp.Children)
                {
                    if (child is TextBlock tb && !string.IsNullOrEmpty(tb.Text) && tb.Text != "🌸" && tb.Text != "🟩" && tb.Text != "⭐")
                    {
                        selectedTemplateName = tb.Text;
                        break;
                    }
                }
            }

            if (RbXemManHinh.IsChecked == true)
            {
                this.Close();
                var win = new PrintPreviewWindow(
                    _dataGrid?.ItemsSource?.Cast<object>(),
                    selectedTemplateName,
                    _storeName,
                    _tuNgay,
                    _denNgay);
                if (this.Owner != null) win.Owner = this.Owner;
                win.ShowDialog();
            }
            else if (RbInMayIn.IsChecked == true)
            {
                try
                {
                    var printDlg = new PrintDialog();
                    if (printDlg.ShowDialog() == true)
                    {
                        var previewWin = new PrintPreviewWindow(
                            _dataGrid?.ItemsSource?.Cast<object>(),
                            selectedTemplateName,
                            _storeName,
                            _tuNgay,
                            _denNgay);
                        printDlg.PrintVisual(previewWin.PaperContainer, $"{_reportTitle} - {selectedTemplateName}");
                        MessageBox.Show("Đã gửi lệnh in thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (RbXuatExcel.IsChecked == true)
            {
                this.Close();
                ExportGridToExcel();
            }
            else if (RbXuatPdf.IsChecked == true)
            {
                this.Close();
                var win = new PrintPreviewWindow(
                    _dataGrid?.ItemsSource?.Cast<object>(),
                    selectedTemplateName,
                    _storeName,
                    _tuNgay,
                    _denNgay);
                if (this.Owner != null) win.Owner = this.Owner;
                win.ShowDialog();
            }
            else if (RbThietKe.IsChecked == true)
            {
                MessageBox.Show("Chức năng thiết kế mẫu in đang sẵn sàng trong bản nâng cấp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportGridToExcel()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"{_reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();

                    // Header
                    var columns = _dataGrid.Columns.Where(c => c.Visibility == Visibility.Visible).ToList();
                    sb.AppendLine(string.Join(",", columns.Select(c => $"\"{c.Header}\"")));

                    // Data Rows
                    if (_dataGrid.ItemsSource != null)
                    {
                        foreach (var item in _dataGrid.ItemsSource)
                        {
                            var rowValues = new List<string>();
                            foreach (var col in columns)
                            {
                                string val = "";
                                if (col is DataGridBoundColumn boundCol && boundCol.Binding is System.Windows.Data.Binding binding)
                                {
                                    var propName = binding.Path?.Path;
                                    if (!string.IsNullOrEmpty(propName))
                                    {
                                        var propVal = item.GetType().GetProperty(propName)?.GetValue(item, null);
                                        val = propVal?.ToString() ?? "";
                                    }
                                }
                                rowValues.Add($"\"{val.Replace("\"", "\"\"")}\"");
                            }
                            sb.AppendLine(string.Join(",", rowValues));
                        }
                    }

                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
