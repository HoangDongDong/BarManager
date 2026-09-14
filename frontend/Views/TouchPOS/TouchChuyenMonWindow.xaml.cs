using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchChuyenMonWindow : Window
    {
        public TouchCartItemVM Item { get; }
        public decimal SelectedQuantity { get; private set; }
        public bool IsTransferAll { get; private set; }

        public TouchChuyenMonWindow(TouchCartItemVM item)
        {
            InitializeComponent();
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                BtnChuyenTatCa.Content = $"CHUYỂN TẤT CẢ ({Item.SoLuong:0.#})";

                Brush greenBg = (Brush)new BrushConverter().ConvertFromString("#145220")!;
                Brush greenBorder = (Brush)new BrushConverter().ConvertFromString("#1E6C2B")!;

                Brush grayBg = (Brush)new BrushConverter().ConvertFromString("#757575")!;
                Brush grayBorder = (Brush)new BrushConverter().ConvertFromString("#9E9E9E")!;

                Button[] qtyButtons = { BtnQty1, BtnQty2, BtnQty3, BtnQty4, BtnQty5 };

                foreach (var btn in qtyButtons)
                {
                    if (btn.Tag != null && decimal.TryParse(btn.Tag.ToString(), out decimal val))
                    {
                        if (val <= Item.SoLuong)
                        {
                            btn.Background = greenBg;
                            btn.BorderBrush = greenBorder;
                            btn.IsEnabled = true;
                        }
                        else
                        {
                            btn.Background = grayBg;
                            btn.BorderBrush = grayBorder;
                            btn.IsEnabled = false; // Disabled & Grayed out!
                        }
                    }
                }

                BtnQtyKhac.Background = grayBg;
                BtnQtyKhac.BorderBrush = grayBorder;
            }
        }

        private void BtnChuyenTatCa_Click(object sender, RoutedEventArgs e)
        {
            SelectedQuantity = Item.SoLuong;
            IsTransferAll = true;
            DialogResult = true;
            Close();
        }

        private void BtnQtyNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && decimal.TryParse(btn.Tag.ToString(), out decimal qty))
            {
                if (qty <= Item.SoLuong)
                {
                    SelectedQuantity = qty;
                    IsTransferAll = (qty == Item.SoLuong);
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void BtnQtyKhac_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchNhapSoLuongWindow($"Số lượng chuyển món {Item.TenMatHang}", 1)
            {
                Owner = this
            };

            if (win.ShowDialog() == true && win.NewQuantity > 0)
            {
                if (win.NewQuantity > Item.SoLuong)
                {
                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"SỐ LƯỢNG CHUYỂN KHÔNG THỂ LỚN HƠN SỐ LƯỢNG HIỆN CÓ ({Item.SoLuong:0.#})!", "CẢNH BÁO");
                    return;
                }

                SelectedQuantity = win.NewQuantity;
                IsTransferAll = (win.NewQuantity == Item.SoLuong);
                DialogResult = true;
                Close();
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
