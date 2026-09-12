using System.Windows;

namespace QuanLyBar.Views.TouchPOS
{
    public partial class TouchConfirmWindow : Window
    {
        public TouchConfirmWindow(string message)
        {
            InitializeComponent();
            TxtMessage.Text = message;
        }

        public static bool Show(Window owner, string message)
        {
            var dlg = new TouchConfirmWindow(message);
            dlg.Owner = owner;
            return dlg.ShowDialog() == true;
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
