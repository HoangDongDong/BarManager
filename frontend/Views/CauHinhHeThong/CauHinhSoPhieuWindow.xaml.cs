using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
    public partial class CauHinhSoPhieuWindow : Window
    {
        public string ResultPattern { get; private set; }
        private string _loaiPhieuName;

        public CauHinhSoPhieuWindow(string loaiPhieuName, string initialPattern)
        {
            InitializeComponent();
            _loaiPhieuName = loaiPhieuName;
            Title = $"Cấu hình cách sinh {loaiPhieuName}";
            TxtHeaderTitle.Text = $"Thiết lập cách sinh {loaiPhieuName}";
            TxtPattern.Text = initialPattern ?? "";
            ResultPattern = initialPattern ?? "";
            UpdateExplanation();
        }

        private void TxtPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExplanation();
        }

        private void UpdateExplanation()
        {
            if (TxtExplanation == null || TxtPattern == null) return;

            string pattern = TxtPattern.Text.Trim();
            if (string.IsNullOrEmpty(pattern))
            {
                TxtExplanation.Text = "";
                return;
            }

            // Tìm mẫu chứa dấu sao ví dụ (*****) hoặc (**)
            var match = Regex.Match(pattern, @"\(\*+\)");
            if (match.Success)
            {
                int starCount = match.Value.Length - 2; // trừ đi 2 dấu ngoặc đơn ()
                if (starCount > 0 && starCount <= 9)
                {
                    long maxNumber = (long)Math.Pow(10, starCount) - 1;
                    string minSuffix = "1".PadLeft(starCount, '0');
                    string maxSuffix = new string('9', starCount);

                    string minStr = pattern.Replace(match.Value, minSuffix);
                    string maxStr = pattern.Replace(match.Value, maxSuffix);

                    TxtExplanation.Text = $"Có {maxNumber} chuỗi từ {minStr} đến {maxStr}";
                    return;
                }
            }

            TxtExplanation.Text = "";
        }

        private void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            ResultPattern = TxtPattern.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
