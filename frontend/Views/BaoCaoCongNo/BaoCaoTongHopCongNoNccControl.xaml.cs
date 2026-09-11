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
    public partial class BaoCaoTongHopCongNoNccControl : UserControl
    {
        private bool _isLoaded = false;
        private List<BaoCaoCongNoRowItem> _allData = new List<BaoCaoCongNoRowItem>();

        public BaoCaoTongHopCongNoNccControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;

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
            string nhomId = (CboNhomNcc.SelectedItem as CongNoFilterComboItem)?.Id ?? "";
            string nhomName = (CboNhomNcc.SelectedItem as CongNoFilterComboItem)?.Name ?? "Tất cả";

            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm nhà cung cấp: {nhomName}");
            if (parts.Count > 0)
            {
                TxtFilterSummary.Text = string.Join("\n", parts);
                TxtFilterSummary.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                TxtFilterSummary.Text = "";
                TxtFilterSummary.Visibility = System.Windows.Visibility.Collapsed;
            }
            TxtSignDate.Text = $"Ngày {DateTime.Today:dd} tháng {DateTime.Today:MM} năm {DateTime.Today:yyyy}";

            _allData = await LocalBaoCaoCongNoService.GetTongHopCongNoNccAsync(nhomId);

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
                    x.DiaChi.ToLower().Contains(keyword) ||
                    x.Email.ToLower().Contains(keyword) ||
                    x.TenNhom.ToLower().Contains(keyword)
                ).ToList();
            }

            decimal totNo = filtered.Sum(x => x.TongNo);
            TxtTotalTongNo.Text = totNo.ToString("N0");

            var groups = filtered.GroupBy(x => x.TenNhom).OrderBy(g => g.Key);

            foreach (var grp in groups)
            {
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
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });

                    rowGrid.Children.Add(CreateCell(item.Stt.ToString(), 0, TextAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.Ma, 1, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.Ten, 2, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.DiaChi, 3, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.DienThoai, 4, TextAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.Email, 5, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.TongNo.ToString("N0"), 6, TextAlignment.Right, isLast: true, isBold: true));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                    rowIndex++;
                }
            }
        }

        private Border CreateCell(string text, int col, TextAlignment align, bool isLast = false, bool isBold = false)
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
                Foreground = Brushes.Black,
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
                    printDlg.PrintVisual(ReportPaper, "Tong Hop Cong No Nha Cung Cap");
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
                    FileName = $"TongHopCongNoNCC_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtFilterSummary.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine("STT,Mã NCC,Tên nhà cung cấp,Địa chỉ,Điện thoại,Email,Tổng nợ");

                    var groups = _allData.GroupBy(x => x.TenNhom).OrderBy(g => g.Key);
                    foreach (var grp in groups)
                    {
                        sb.AppendLine($"\"Nhóm: {grp.Key}\",,,,,,");
                        foreach (var item in grp)
                        {
                            sb.AppendLine($"{item.Stt},\"{item.Ma}\",\"{item.Ten}\",\"{item.DiaChi}\",\"{item.DienThoai}\",\"{item.Email}\",{item.TongNo}");
                        }
                    }

                    sb.AppendLine($",,,,,TỔNG CỘNG,{_allData.Sum(x => x.TongNo)}");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV/Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThietKeMau_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Chuc nang thiet ke mau bao cao cho phep ban tuy chinh:\n" +
                "  - Bo cuc va dinh dang bao cao\n" +
                "  - Font chu, co chu, mau sac\n" +
                "  - Logo va thong tin dau trang\n" +
                "  - Them/bo cac cot hien thi\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Thiết kế mẫu bao cao",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnThamSoTuyChinh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tham số tùy chỉnh báo cáo cho phep ban:\n" +
                "  - Chon cac cot hien thi trong bao cao\n" +
                "  - Thiet lap tieu chi nhom du lieu\n" +
                "  - Cau hinh cac dieu kien loc nang cao\n" +
                "  - Tuy chinh dinh dang so va ngay thang\n\n" +
                "Tinh nang nay se duoc cap nhat trong phien ban tiep theo.",
                "Tham số tuy chinh",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnXemDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (InlineDataBorder.Visibility == System.Windows.Visibility.Visible)
            {
                InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            // Use reflection to find the largest data list in this control
            var fields = this.GetType().GetFields(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object? dataSource = null;
            int maxCount = 0;
            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                if (val is System.Collections.IList list && list.Count > maxCount)
                {
                    maxCount = list.Count;
                    dataSource = val;
                }
            }

            if (dataSource != null)
                InlineDataGrid.ItemsSource = (System.Collections.IEnumerable)dataSource;
            else
                InlineDataGrid.ItemsSource = new[] { new { ThongBao = "Không có dữ liệu. Hãy tải dữ liệu trước (F5) roi mo lai." } };

            TxtSoBanGhi.Text = $"Tổng số: {maxCount} bản ghi";
            InlineDataBorder.Visibility = System.Windows.Visibility.Visible;

            // Scroll to bottom so user can see the panel
            var scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(this);
            scrollViewer?.ScrollToEnd();
        }

        private void BtnDongDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
