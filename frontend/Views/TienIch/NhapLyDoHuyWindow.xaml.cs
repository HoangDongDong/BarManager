using System.Windows;

namespace QuanLyBar.Client.Views
{
    public partial class NhapLyDoHuyWindow : Window
    {
        public string LyDo { get; private set; }

        public NhapLyDoHuyWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                TxtLyDo.Focus();
            };
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            LyDo = TxtLyDo.Text?.Trim() ?? "";
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
