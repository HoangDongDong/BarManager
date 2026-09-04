using System;
using System.Windows;

namespace QuanLyBar.Client.Views.CongNo
{
    public partial class LocCongNoKhachHangWindow : Window
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int DebtFilterMode { get; set; } = 1; // 0: Tất cả, 1: Chỉ còn nợ, 2: Có phát sinh
        public string Keyword { get; set; } = "";

        public LocCongNoKhachHangWindow(DateTime? tuNgay, DateTime? denNgay, int debtFilterMode, string keyword)
        {
            InitializeComponent();
            TuNgay = tuNgay;
            DenNgay = denNgay;
            DebtFilterMode = debtFilterMode;
            Keyword = keyword ?? "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DpTuNgay.SelectedDate = TuNgay;
            DpDenNgay.SelectedDate = DenNgay;
            TxtKeyword.Text = Keyword;

            if (DebtFilterMode == 0) RbTatCa.IsChecked = true;
            else if (DebtFilterMode == 1) RbChiConNo.IsChecked = true;
            else if (DebtFilterMode == 2) RbPhatSinh.IsChecked = true;
        }

        private void BtnDongY_Click(object sender, RoutedEventArgs e)
        {
            TuNgay = DpTuNgay.SelectedDate;
            DenNgay = DpDenNgay.SelectedDate;
            Keyword = TxtKeyword.Text.Trim();

            if (RbTatCa.IsChecked == true) DebtFilterMode = 0;
            else if (RbChiConNo.IsChecked == true) DebtFilterMode = 1;
            else if (RbPhatSinh.IsChecked == true) DebtFilterMode = 2;

            this.DialogResult = true;
            this.Close();
        }

        private void BtnBoQua_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
