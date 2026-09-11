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
using static QuanLyBar.Client.Services.LocalBaoCaoQuanTriService;

namespace QuanLyBar.Client.Views.BaoCaoQuanTri
{
    public partial class BaoCao20MatHangBanChayControl : UserControl
    {
        private readonly LocalBaoCaoQuanTriService _service = new LocalBaoCaoQuanTriService();
        private bool _isLoaded = false;
        private List<MatHangBanChayItem> _allData = new List<MatHangBanChayItem>();

        public BaoCao20MatHangBanChayControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = new DateTime(today.Year, today.Month, 1);
            DpDenNgay.SelectedDate = today;
            TxtSignDate.Text = $"Ngày {today:dd} tháng {today:MM} năm {today:yyyy}";

            await LoadCompanyInfoAndLogoAsync();
            await LoadNhomHangAsync();
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

        private async Task LoadNhomHangAsync()
        {
            try
            {
                var nhomList = await _service.GetNhomHangListAsync();
                CboNhomHang.ItemsSource = nhomList;
                if (nhomList.Count > 0) CboNhomHang.SelectedIndex = 0;
            }
            catch { }
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var tuNgay = DpTuNgay.SelectedDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var denNgay = DpDenNgay.SelectedDate ?? DateTime.Today;
                string nhomHang = CboNhomHang.SelectedItem as string ?? "[Tất cả]";

                TxtFilterSummary.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} đến ngày {denNgay:dd/MM/yyyy}";
                TxtNhomHangSummary.Text = $"Nhóm hàng: {(nhomHang == "[Tất cả]" ? "Tất cả" : nhomHang)}";

                _allData = await _service.GetBaoCao20MatHangBanChayAsync(tuNgay, denNgay, nhomHang);

                RenderTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderTable();
        }

        private void RenderTable()
        {
            PnlReportContent.Children.Clear();

            string filter = TxtSearch.Text?.Trim().ToLower() ?? "";

            var filteredItems = string.IsNullOrEmpty(filter)
                ? _allData
                : _allData.Where(x => (x.MatHang?.ToLower().Contains(filter) ?? false) ||
                                       (x.MaHang?.ToLower().Contains(filter) ?? false)).ToList();

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Mã hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) }); // Mặt hàng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });  // ĐVT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });  // Số lượng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Giá bán

            // Header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderCell(grid, "STT", 0, 0);
            AddHeaderCell(grid, "Mã hàng", 0, 1);
            AddHeaderCell(grid, "Mặt hàng", 0, 2);
            AddHeaderCell(grid, "ĐVT", 0, 3);
            AddHeaderCell(grid, "Số lượng", 0, 4);
            AddHeaderCell(grid, "Giá bán", 0, 5);

            int rowIdx = 1;
            int stt = 1;
            foreach (var it in filteredItems)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                AddDataCell(grid, stt.ToString(), rowIdx, 0, TextAlignment.Center);
                AddDataCell(grid, it.MaHang, rowIdx, 1, TextAlignment.Left);
                AddDataCell(grid, it.MatHang, rowIdx, 2, TextAlignment.Left);
                AddDataCell(grid, it.Dvt, rowIdx, 3, TextAlignment.Center);
                AddDataCell(grid, it.SoLuong.ToString("#,##0"), rowIdx, 4, TextAlignment.Right);
                AddDataCell(grid, it.GiaBan > 0 ? it.GiaBan.ToString("#,##0") : "0", rowIdx, 5, TextAlignment.Right);

                stt++;
                rowIdx++;
            }

            PnlReportContent.Children.Add(grid);
        }

        private void AddHeaderCell(Grid g, string text, int row, int col)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 1, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(4, 5, 4, 5)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            g.Children.Add(b);
        }

        private void AddDataCell(Grid g, string text, int row, int col, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = Brushes.White,
                Padding = new Thickness(5, 4, 5, 4)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 11,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            g.Children.Add(b);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"BaoCao_20MatHangBanChay_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT");
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine(TxtNhomHangSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine("STT,Mã hàng,Mặt hàng,ĐVT,Số lượng,Giá bán");

                    int idx = 1;
                    foreach (var it in _allData)
                    {
                        sb.AppendLine($"{idx++},\"{it.MaHang}\",\"{it.MatHang}\",\"{it.Dvt}\",{it.SoLuong},{it.GiaBan}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file báo cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
