using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyBar.Client.Views.CauHinhHeThong
{
    public partial class ThietLapMauNhanhWindow : Window
    {
        public string SelectedColor { get; private set; } = "#1976D2";
        public string SelectedTarget { get; private set; } = "MatHang";
        public bool IsAutoColor => RbTuDong.IsChecked == true;

        public ThietLapMauNhanhWindow()
        {
            InitializeComponent();
            BuildColorPalette();
            RbThuCong.Checked += ColorModeChanged;
            RbTuDong.Checked += ColorModeChanged;
        }

        private void ColorModeChanged(object sender, RoutedEventArgs e)
        {
            if (BtnColorPreview != null)
            {
                BtnColorPreview.IsEnabled = RbThuCong.IsChecked == true;
                BtnColorPreview.Opacity = RbThuCong.IsChecked == true ? 1.0 : 0.5;
            }
        }

        private void BuildColorPalette()
        {
            var colors = new[]
            {
                "#002060","#004080","#0066CC","#0080FF","#3399FF","#66B2FF","#99CCFF","#CCE6FF","#000000","#FFFFFF",
                "#0B3C5D","#1D2731","#328CC1","#D9B310","#005A44","#00875A","#36B37E","#57D9A3","#79F2C0","#ABF5D1",
                "#172B4D","#091E42","#253858","#42526E","#5E6C84","#7A869A","#97A0AF","#B3BAC5","#C1C7D0","#DFE1E6",
                "#0052CC","#0747A6","#172B4D","#2684FF","#4C9AFF","#B3D4FF","#DEEBFF","#00A3BF","#00B8D9","#79E2F2",
                "#006644","#00875A","#36B37E","#57D9A3","#79F2C0","#FFAB00","#FFC400","#FFE380","#FFF0B3","#FF9900",
                "#DE350B","#FF5630","#FF8F73","#FFBDAD","#FFEBE6","#BF2600","#E01E5A","#ECB22E","#2BAC76","#1264A3",
                "#4A154B","#611B60","#800080","#8B008B","#9932CC","#BA55D3","#DA70D6","#EE82EE","#DDA0DD","#E6E6FA",
                "#8B0000","#A52A2A","#B22222","#DC143C","#FF0000","#FF4500","#FF6347","#FF7F50","#FFA07A","#FFE4E1",
                "#D2691E","#8B4513","#A0522D","#CD853F","#DEB887","#F5DEB3","#F4A460","#BC8F8F","#C0C0C0","#808080"
            };

            foreach (var hex in colors)
            {
                var border = new Border
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(2),
                    CornerRadius = new CornerRadius(2),
                    Background = (Brush)new BrushConverter().ConvertFromString(hex)!,
                    Cursor = Cursors.Hand,
                    ToolTip = hex
                };
                border.MouseDown += (s, e) =>
                {
                    SelectedColor = hex;
                    BtnColorPreview.Background = (Brush)new BrushConverter().ConvertFromString(hex)!;
                    RbThuCong.IsChecked = true;
                    PopupColorPicker.IsOpen = false;
                };
                UgColorMatrix.Children.Add(border);
            }
        }

        private void RbThuCong_Click(object sender, RoutedEventArgs e)
        {
            RbThuCong.IsChecked = true;
        }

        private void BtnColorPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RbThuCong.IsChecked = true;
            PopupColorPicker.IsOpen = true;
        }

        private void BtnChooseColor_Click(object sender, RoutedEventArgs e)
        {
            RbThuCong.IsChecked = true;
            PopupColorPicker.IsOpen = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            SelectedTarget = RbKhuVuc.IsChecked == true
                ? "KhuVuc"
                : RbNhomHang.IsChecked == true
                    ? "NhomHang"
                    : RbMatHangTrongNhom.IsChecked == true
                        ? "MatHangTrongNhom"
                        : "MatHang";

            DialogResult = true;
        }
    }
}
