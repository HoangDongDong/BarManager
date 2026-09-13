using System;
using System.Windows;

namespace QuanLyBar.Views.TouchPOS
{
    public partial class TouchConfirmWindow : Window
    {
        public TouchConfirmWindow(string message, string title = "XÁC NHẬN", bool isAlert = false, string icon = "?")
        {
            InitializeComponent();
            if (TxtTitle != null) TxtTitle.Text = title.ToUpper();
            if (TxtMessage != null) TxtMessage.Text = message;
            if (TxtIcon != null) TxtIcon.Text = icon;

            if (isAlert)
            {
                if (BtnNo != null) BtnNo.Visibility = Visibility.Collapsed;
                if (BtnYes != null) BtnYes.Content = "OK";
                if (TxtIcon != null && icon == "?") TxtIcon.Text = "⚠";
            }
        }

        public static bool Show(Window owner, string message, string title = "XÁC NHẬN")
        {
            var dlg = new TouchConfirmWindow(message, title, isAlert: false, icon: "?");
            dlg.Owner = owner;
            return dlg.ShowDialog() == true;
        }

        public static void ShowAlert(Window owner, string message, string title = "CẢNH BÁO")
        {
            var dlg = new TouchConfirmWindow(message, title, isAlert: true, icon: "⚠");
            dlg.Owner = owner;
            dlg.ShowDialog();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { DialogResult = true; } catch { }
            }));
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { DialogResult = false; } catch { }
            }));
        }
    }
}
