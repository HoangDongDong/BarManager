using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoCongNo
{
    public partial class BaoCaoCongNoNccControl : UserControl
    {
        private bool _isLoaded = false;
        private List<BaoCaoCongNoRowItem> _allData = new List<BaoCaoCongNoRowItem>();

        public BaoCaoCongNoNccControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;

            DpTuNgay.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DpDenNgay.SelectedDate = DateTime.Today;

            await LoadCompanyInfoAsync();
            await LoadFiltersAsync();

            _isLoaded = true;
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAsync()
        {
            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                string cName = configs.TryGetValue("CompanyName", out var cn) ? cn : "NÀNG HƯƠNG QUÁN";
                string cAddr = configs.TryGetValue("CompanyAddress", out var ca) ? ca : "Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội";
                string cPhone = configs.TryGetValue("CompanyPhone", out var cp) ? cp : "0909090880";
                string cEmail = configs.TryGetValue("CompanyEmail", out var ce) ? ce : "";

                TxtCompanyName.Text = cName;
                TxtCompanyAddress.Text = $"Địa chỉ: {cAddr}";
                TxtCompanyContact.Text = string.IsNullOrEmpty(cEmail) ? $"Điện thoại: {cPhone}" : $"Điện thoại: {cPhone}, Email: {cEmail}";

                var logoBytes = await LocalCauHinhService.LoadCompanyLogoAsync();
                if (logoBytes != null && logoBytes.Length > 0)
                {
                    var bi = new BitmapImage();
                    using (var ms = new MemoryStream(logoBytes))
                    {
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;
                        bi.EndInit();
                    }
                    bi.Freeze();
                    ImgLogo.Source = bi;
                    ImgLogo.Visibility = Visibility.Visible;
                    VbDefaultLogo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImgLogo.Visibility = Visibility.Collapsed;
                    VbDefaultLogo.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var nhomList = await LocalBaoCaoCongNoService.GetNhomNccDropdownAsync();
                CboNhomNcc.ItemsSource = nhomList;
                if (nhomList.Count > 0)
                {
                    CboNhomNcc.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadFilters: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string nhomId = (CboNhomNcc.SelectedItem as CongNoFilterComboItem)?.Id ?? "";
            string nhomName = (CboNhomNcc.SelectedItem as CongNoFilterComboItem)?.Name ?? "Tất cả";

            TxtSubTitleDate.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";
            TxtFilterSummary.Text = $"Nhóm nhà cung cấp: {nhomName}";
            TxtSignDate.Text = $"Ngày {DateTime.Today:dd} tháng {DateTime.Today:MM} năm {DateTime.Today:yyyy}";

            _allData = await LocalBaoCaoCongNoService.GetBaoCaoCongNoNccAsync(tuNgay, denNgay, nhomId);

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();

            string keyword = TxtFilter.Text.Trim().ToLower();
            var filtered = _allData;
            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = _allData.Where(x =>
                    x.Ma.ToLower().Contains(keyword) ||
                    x.Ten.ToLower().Contains(keyword) ||
                    x.DienThoai.ToLower().Contains(keyword) ||
                    x.TenNhom.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totNoDau = filtered.Sum(x => x.NoDau);
            decimal totMua = filtered.Sum(x => x.Mua);
            decimal totThanhToan = filtered.Sum(x => x.ThanhToan);
            decimal totNoCuoi = filtered.Sum(x => x.NoCuoi);

            TxtTotalNoDau.Text = totNoDau.ToString("N0");
            TxtTotalMua.Text = totMua.ToString("N0");
            TxtTotalThanhToan.Text = totThanhToan.ToString("N0");
            TxtTotalNoCuoi.Text = totNoCuoi.ToString("N0");

            var groups = filtered.GroupBy(x => x.TenNhom).OrderBy(g => g.Key);

            foreach (var grp in groups)
            {
                // Group row
                Border grpBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 244, 250)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };
                grpBorder.Child = new TextBlock
                {
                    Text = $"Nhóm nhà cung cấp: {grp.Key}",
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    FontSize = 11.5
                };
                StkDataRows.Children.Add(grpBorder);

                int rowIndex = 0;
                foreach (var item in grp)
                {
                    Border rowBorder = new Border
                    {
                        Background = rowIndex % 2 == 1 ? new SolidColorBrush(Color.FromRgb(250, 250, 250)) : Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };

                    Grid rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });

                    rowGrid.Children.Add(CreateCell(item.Stt.ToString(), 0, TextAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.Ma, 1, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.Ten, 2, TextAlignment.Left, isBlue: true));
                    rowGrid.Children.Add(CreateCell(item.DienThoai, 3, TextAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.NoDau.ToString("N0"), 4, TextAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.Mua.ToString("N0"), 5, TextAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhToan.ToString("N0"), 6, TextAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.NoCuoi.ToString("N0"), 7, TextAlignment.Right, isLast: true, isBold: true));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                    rowIndex++;
                }
            }
        }

        private Border CreateCell(string text, int col, TextAlignment align, bool isLast = false, bool isBold = false, bool isBlue = false)
        {
            var b = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(0, 0, isLast ? 0 : 1, 0),
                Padding = new Thickness(4, 3, 4, 3)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                TextAlignment = align,
                FontSize = 11,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isBlue ? new SolidColorBrush(Color.FromRgb(0, 50, 180)) : Brushes.Black,
                VerticalAlignment = VerticalAlignment.Center
            };
            return b;
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderTable();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            BtnPrint_Click(sender, e);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "Bao Cao Cong No Nha Cung Cap");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"BaoCaoCongNoNCC_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtSubTitleDate.Text}\"");
                    sb.AppendLine($"\"{TxtFilterSummary.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine("STT,Mã NCC,Tên nhà cung cấp,Điện thoại,Nợ đầu,Mua,Thanh toán,Nợ cuối");

                    var groups = _allData.GroupBy(x => x.TenNhom).OrderBy(g => g.Key);
                    foreach (var grp in groups)
                    {
                        sb.AppendLine($"\"Nhóm: {grp.Key}\",,,,,,,");
                        foreach (var item in grp)
                        {
                            sb.AppendLine($"{item.Stt},\"{item.Ma}\",\"{item.Ten}\",\"{item.DienThoai}\",{item.NoDau},{item.Mua},{item.ThanhToan},{item.NoCuoi}");
                        }
                    }

                    sb.AppendLine($",,,TỔNG CỘNG,{_allData.Sum(x => x.NoDau)},{_allData.Sum(x => x.Mua)},{_allData.Sum(x => x.ThanhToan)},{_allData.Sum(x => x.NoCuoi)}");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV/Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
