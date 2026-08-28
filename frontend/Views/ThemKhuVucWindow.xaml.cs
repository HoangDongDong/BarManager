using System.Windows;

namespace QuanLyBar.Client.Views
{
    public partial class ThemKhuVucWindow : Window
    {
        public string TenKhuVuc { get; private set; }

        public ThemKhuVucWindow(string title = "THÊM MỚI KHU VỰC", string initialName = "")
        {
            InitializeComponent();
            this.Title = title;
            if (!string.IsNullOrEmpty(initialName))
            {
                TxtTenKhuVuc.Text = initialName;
                TxtTenKhuVuc.SelectAll();
            }
            TxtTenKhuVuc.Focus();
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTenKhuVuc.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khu vực / thư mục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TenKhuVuc = TxtTenKhuVuc.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
