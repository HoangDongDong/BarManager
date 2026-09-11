using System.Windows;
using System.Windows.Media;

namespace QuanLyBar.Client.Views.CauHinhHeThong
{
    public partial class ThietLapMauNhanhWindow : Window
    {
        public string SelectedColor { get; private set; } = "#1976D2";
        public string SelectedTarget { get; private set; } = "MatHang";

        public ThietLapMauNhanhWindow()
        {
            InitializeComponent();
            RbThuCong.Checked += ColorModeChanged;
            RbTuDong.Checked += ColorModeChanged;
        }

        private void ColorModeChanged(object sender, RoutedEventArgs e)
        {
            TxtColor.IsEnabled = RbThuCong.IsChecked == true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            string color = RbTuDong.IsChecked == true ? "#2F6F9F" : TxtColor.Text.Trim();
            try
            {
                ColorConverter.ConvertFromString(color);
            }
            catch
            {
                MessageBox.Show("Màu không hợp lệ. Hãy nhập dạng #RRGGBB.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedColor = color;
            SelectedTarget = RbKhuVuc.IsChecked == true
                ? "KhuVuc"
                : RbNhomHang.IsChecked == true
                    ? "NhomHang"
                    : RbMatHangTrongNhom.IsChecked == true
                        ? "MatHangTrongNhom"
                        : "MatHang";
            DialogResult = true;
        }

        private void BtnChooseColor_Click(object sender, RoutedEventArgs e)
        {
            TxtColor.Focus();
            TxtColor.SelectAll();
        }
    }
}
