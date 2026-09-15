using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views.InAn
{
    public partial class FastReportDesignerWindow : Window
    {
        private string _reportName;

        public FastReportDesignerWindow(string reportName = "Báo cáo kết ca 80mm")
        {
            InitializeComponent();
            _reportName = string.IsNullOrWhiteSpace(reportName) ? "Báo cáo kết ca 80mm" : reportName;
            Title = $"PHẦN MỀM QUẢN LÝ BAR, NHÀ HÀNG v6.0 TÂN AN PHÁT - {reportName.ToUpper()} (FastReport Designer)";
            
            Loaded += FastReportDesignerWindow_Loaded;
        }

        private void FastReportDesignerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectTabForReport(_reportName);
        }

        private void SelectTabForReport(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            string n = name.Trim().ToLower();

            foreach (var item in TcReportTemplates.Items)
            {
                if (item is TabItem tab && tab.Header != null)
                {
                    string h = tab.Header.ToString()!.Trim().ToLower();
                    if (h.Contains(n) || n.Contains(h))
                    {
                        TcReportTemplates.SelectedItem = tab;
                        UpdateReportTitle(tab.Header.ToString()!);
                        return;
                    }
                }
            }

            // Update title text if not mapped to static tab
            TxtDesignerReportTitle.Text = name.ToUpper();
        }

        private void TcReportTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TcReportTemplates.SelectedItem is TabItem tab && tab.Header != null)
            {
                UpdateReportTitle(tab.Header.ToString()!);
            }
        }

        private void UpdateReportTitle(string headerName)
        {
            if (TxtDesignerReportTitle != null)
            {
                TxtDesignerReportTitle.Text = headerName.ToUpper();
            }
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Đang xem trước mẫu báo cáo FastReport: {TxtDesignerReportTitle.Text}", "FastReport Preview", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Đã lưu mẫu báo cáo FastReport {TxtDesignerReportTitle.Text} thành công!", "MẪU IN", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
