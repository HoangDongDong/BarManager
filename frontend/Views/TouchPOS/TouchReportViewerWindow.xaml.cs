using System.Windows;

namespace QuanLyBar.Views.TouchPOS
{
    public partial class TouchReportViewerWindow : Window
    {
        public TouchReportViewerWindow(UIElement reportContent, string title)
        {
            InitializeComponent();
            TxtTitle.Text = title.ToUpper();
            ReportHost.Content = reportContent;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
