using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoBanHang
{
    public partial class BaoCaoTongHopBanTheoThangControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();
        private bool _isLoaded = false;
        private string _reportType = "BÁO CÁO TỔNG HỢP SỐ LƯỢNG BÁN THEO THÁNG";
        private bool _isGiaTri = false;
        private List<BaoCaoTongHopBanTheoThangGroup> _allGroups = new List<BaoCaoTongHopBanTheoThangGroup>();

        public BaoCaoTongHopBanTheoThangControl(string reportType = "BÁO CÁO TỔNG HỢP SỐ LƯỢNG BÁN THEO THÁNG")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "BÁO CÁO TỔNG HỢP SỐ LƯỢNG BÁN THEO THÁNG" : reportType.Trim().ToUpper();
            _isGiaTri = _reportType.Contains("GIÁ TRỊ") || _reportType.Contains("GIA TRI");

            TxtReportTitle.Text = _isGiaTri ? "BÁO CÁO TỔNG HỢP GIÁ TRỊ BÁN THEO THÁNG" : "BÁO CÁO TỔNG HỢP SỐ LƯỢNG BÁN THEO THÁNG";
            TxtDonViTinh.Visibility = _isGiaTri ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            // Load Thang (1..12)
            for (int i = 1; i <= 12; i++)
            {
                CboThang.Items.Add(i.ToString());
            }

            var now = DateTime.Now;
            CboThang.SelectedItem = now.Month.ToString();
            TxtNam.Text = now.Year.ToString();

            await LoadCompanyInfoAndLogoAsync();
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAndLogoAsync()
        {
            try
            {
                var comp = await LocalCauHinhService.GetCompanyInfoAsync();
                TxtCompanyName.Text = comp.Name;
                TxtCompanyAddress.Text = comp.FormattedAddress;
                TxtCompanyContact.Text = comp.FormattedContact;

                if (comp.LogoBytes != null && comp.LogoBytes.Length > 0)
                {
                    var bi = LocalCauHinhService.ImageFromBytes(comp.LogoBytes);
                    if (bi != null)
                    {
                        ImgLogo.Source = bi;
                        ImgLogo.Visibility = Visibility.Visible;
                        if (FindName("VbDefaultLogo") is UIElement vb) vb.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            int thang = DateTime.Now.Month;
            int nam = DateTime.Now.Year;

            if (CboThang.SelectedItem != null && int.TryParse(CboThang.SelectedItem.ToString(), out int parsedThang))
            {
                thang = parsedThang;
            }

            if (!string.IsNullOrWhiteSpace(TxtNam.Text) && int.TryParse(TxtNam.Text.Trim(), out int parsedNam))
            {
                nam = parsedNam;
            }

            TxtHeaderThangNam.Text = $"THÁNG: {thang}/{nam}";

            _allGroups = await _hoaDonService.GetBaoCaoTongHopBanTheoThangAsync(thang, nam, _isGiaTri);
            IcReportGroups.ItemsSource = _allGroups;
        }

        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(A4PageBorder, _reportType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in báo cáo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_allGroups == null || _allGroups.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"{_reportType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"NÀNG HƯƠNG QUÁN");
                    sb.AppendLine($"{_reportType}");
                    sb.AppendLine($"{TxtHeaderThangNam.Text}");
                    sb.AppendLine();

                    var headerRow = new List<string> { "STT", "Mã SP", "Sản phẩm", "ĐVT", "Tổng bán" };
                    for (int d = 1; d <= 31; d++) headerRow.Add(d.ToString("00"));
                    sb.AppendLine(string.Join(",", headerRow.Select(x => $"\"{x}\"")));

                    foreach (var grp in _allGroups)
                    {
                        sb.AppendLine($"\"Nhóm hàng: {grp.TenNhom}\"");
                        foreach (var it in grp.Items)
                        {
                            var row = new List<string>
                            {
                                it.STT.ToString(),
                                $"\"{it.MaSP}\"",
                                $"\"{it.SanPham}\"",
                                $"\"{it.Dvt}\"",
                                it.TongBan.ToString()
                            };
                            for (int d = 1; d <= 31; d++) row.Add(it.NgayVal[d] != 0 ? it.NgayVal[d].ToString() : "");
                            sb.AppendLine(string.Join(",", row));
                        }

                        var grpRow = new List<string> { "", "", $"\"Tổng nhóm hàng: {grp.TenNhom}\"", "", grp.TongNhom.ToString() };
                        for (int d = 1; d <= 31; d++) grpRow.Add(grp.TongNgayNhom[d] != 0 ? grp.TongNgayNhom[d].ToString() : "");
                        sb.AppendLine(string.Join(",", grpRow));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
