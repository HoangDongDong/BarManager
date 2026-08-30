using System.Windows;

namespace QuanLyBar.Client.Views
{
    public partial class InputWindow : Window
    {
        public string InputText { get; private set; }

        public InputWindow(string title, string prompt, string defaultText = "")
        {
            InitializeComponent();
            this.Title = title;
            LblPrompt.Text = prompt;
            TxtInput.Text = defaultText;
            TxtInput.SelectAll();
            TxtInput.Focus();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtInput.Text))
            {
                MessageBox.Show("Vui lòng nhập giá trị hợp lệ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InputText = TxtInput.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
