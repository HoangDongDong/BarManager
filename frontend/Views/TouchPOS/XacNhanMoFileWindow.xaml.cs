using System.Windows;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class XacNhanMoFileWindow : Window
    {
        public XacNhanMoFileWindow()
        {
            InitializeComponent();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
