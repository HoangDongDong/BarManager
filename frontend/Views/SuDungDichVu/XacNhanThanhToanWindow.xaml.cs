using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class XacNhanThanhToanWindow : Window
    {
        private PosBanViewModel _ban;
        public decimal TongTien { get; private set; }
        public decimal KhachDua { get; private set; }
        public decimal TheATM { get; private set; }
        public decimal TheTraTruoc { get; private set; }
        public decimal TraLai { get; private set; }
        public bool IsKhachNo { get; private set; }
        public bool IsInBill { get; private set; } = true;
        public string MaTheTraTruoc { get; private set; }

        public XacNhanThanhToanWindow(PosBanViewModel ban)
        {
            InitializeComponent();
            _ban = ban;

            if (_ban != null)
            {
                TxtTenBanHeader.Text = _ban.Name;
                TongTien = _ban.TongCong;
                KhachDua = _ban.TongCong;
                TxtTongTien.Text = TongTien.ToString("N0");
                TxtKhachDua.Text = KhachDua.ToString("N0");
                TxtTheATM.Text = "0";
                TxtTheTraTruoc.Text = "0";
                TxtTraLai.Text = "0";
            }

            Loaded += (s, e) =>
            {
                TxtKhachDua.Focus();
                TxtKhachDua.SelectAll();
            };
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                e.Handled = true;
                BtnInTamTinh_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.F9)
            {
                e.Handled = true;
                BtnDongBillKhongIn_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                BtnHuyBo_Click(this, new RoutedEventArgs());
            }
        }

        private void MoneyInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            decimal kd = ParseDecimal(TxtKhachDua?.Text);
            decimal atm = ParseDecimal(TxtTheATM?.Text);
            decimal tt = ParseDecimal(TxtTheTraTruoc?.Text);

            KhachDua = kd;
            TheATM = atm;
            TheTraTruoc = tt;

            decimal totalPaid = kd + atm + tt;
            decimal change = totalPaid - TongTien;

            if (change < 0) change = 0;
            TraLai = change;
            if (TxtTraLai != null)
            {
                TxtTraLai.Text = change.ToString("N0");
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string clean = text.Replace(",", "").Replace(".", "").Trim();
            decimal.TryParse(clean, out decimal val);
            return val;
        }

        private void BtnNhapTheTraTruoc_Click(object sender, RoutedEventArgs e)
        {
            var win = new InputWindow("NHẬP THẺ TRẢ TRƯỚC", "MÃ THẺ", MaTheTraTruoc ?? "");
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.InputText))
            {
                MaTheTraTruoc = win.InputText.Trim();
                TxtTheTraTruoc.Text = TongTien.ToString("N0");
                TxtKhachDua.Text = "0";
            }
        }

        private void ChkKhachHangNo_Checked(object sender, RoutedEventArgs e)
        {
            IsKhachNo = true;
            TxtKhachDua.Text = "0";
            TxtTheATM.Text = "0";
            TxtTheTraTruoc.Text = "0";
        }

        private void ChkKhachHangNo_Unchecked(object sender, RoutedEventArgs e)
        {
            IsKhachNo = false;
            TxtKhachDua.Text = TongTien.ToString("N0");
        }

        private void BtnInTamTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_ban != null)
            {
                var printWin = new HoaDonBanHangPrintWindow(_ban, isTamTinh: true);
                printWin.Owner = this;
                printWin.ShowDialog();
            }
        }

        private void BtnDongBillVaIn_Click(object sender, RoutedEventArgs e)
        {
            IsInBill = true;
            DialogResult = true;
            Close();
        }

        private void BtnDongBillKhongIn_Click(object sender, RoutedEventArgs e)
        {
            IsInBill = false;
            DialogResult = true;
            Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
