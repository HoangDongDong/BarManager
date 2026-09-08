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
    public partial class BaoCaoTongHopBanHangTheoNhomHienThiControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();
        private readonly LocalTheoDoiDatPhongService _theoDoiDatPhongService = new LocalTheoDoiDatPhongService();
        private readonly LocalMatHangService _matHangService = new LocalMatHangService();

        private bool _isLoaded = false;
        private List<TongHopBanHangTheoNhomHienThiItem> _allData = new List<TongHopBanHangTheoNhomHienThiItem>();

        public class FilterComboItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
        }

        public BaoCaoTongHopBanHangTheoNhomHienThiControl(string tabName = "TỔNG HỢP BÁN HÀNG THEO NHÓM HIỂN THỊ")
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            DpTuNgay.SelectedDate = today;
            DpDenNgay.SelectedDate = today;

            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
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

        private async Task LoadFiltersAsync()
        {
            try
            {
                // 1. Khu vực
                var kvList = new List<FilterComboItem>();
                try
                {
                    var dbKv = await _theoDoiDatPhongService.GetKhuVucLookupAsync();
                    if (dbKv != null)
                    {
                        kvList.AddRange(dbKv.Select(k => new FilterComboItem { Id = k.Id ?? "", Name = string.IsNullOrEmpty(k.Id) ? "--- Tất cả ---" : (k.Name ?? ""), Icon = "📍" }));
                    }
                }
                catch { }
                if (kvList.Count == 0)
                {
                    kvList.Add(new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "📍" });
                }
                CboKhuVuc.ItemsSource = kvList;
                CboKhuVuc.SelectedIndex = 0;

                // 2. Nhóm hiển thị
                var nhomList = new List<FilterComboItem>
                {
                    new FilterComboItem { Id = "", Name = "--- Tất cả ---", Icon = "📁" }
                };
                try
                {
                    var dbNhom = await _matHangService.GetNhomMatHangListAsync();
                    if (dbNhom != null)
                    {
                        nhomList.AddRange(dbNhom.Select(n => new FilterComboItem { Id = n.Id ?? "", Name = n.Name ?? "", Icon = "📁" }));
                    }
                }
                catch { }

                CboNhomHienThi.ItemsSource = nhomList;
                CboNhomHienThi.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadFiltersAsync: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string khuVucId = (CboKhuVuc.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomHienThiId = (CboNhomHienThi.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
            {
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            }
            else
            {
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";
            }

            string nhomText = (CboNhomHienThi.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(nhomText)) nhomText = "Tất cả";
            string kvText = (CboKhuVuc.SelectedItem as FilterComboItem)?.Name;
            if (string.IsNullOrWhiteSpace(kvText)) kvText = "Tất cả";
            TxtFilterSummary.Text = $"Nhóm hiển thị: {nhomText} | Khu vực: {kvText}";

            _allData = await _hoaDonService.GetTongHopBanHangTheoNhomHienThiAsync(tuNgay, denNgay, khuVucId, nhomHienThiId);

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
                    x.NhomHienThi.ToLower().Contains(keyword)
                ).ToList();
            }

            if (filtered.Count == 0)
            {
                var emptyRow = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(6) };
                emptyRow.Child = new TextBlock { Text = "Không có dữ liệu báo cáo", FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Gray };
                StkDataRows.Children.Add(emptyRow);
                return;
            }

            decimal grandTienHang = 0;
            decimal grandGiamGia = 0;
            decimal grandTongCong = 0;
            int stt = 1;

            foreach (var item in filtered)
            {
                var rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                // STT
                rowGrid.Children.Add(CreateCell(stt.ToString(), 0, HorizontalAlignment.Center));
                stt++;

                // Nhóm hiển thị
                rowGrid.Children.Add(CreateCell(item.NhomHienThi, 1, HorizontalAlignment.Left));

                // Tiền hàng
                rowGrid.Children.Add(CreateCell(item.TienHang == 0 ? "0" : item.TienHang.ToString("N0"), 2, HorizontalAlignment.Right));

                // Giảm giá
                rowGrid.Children.Add(CreateCell(item.GiamGia == 0 ? "0" : item.GiamGia.ToString("N0"), 3, HorizontalAlignment.Right));

                // Tổng cộng
                rowGrid.Children.Add(CreateCell(item.TongCong == 0 ? "0" : item.TongCong.ToString("N0"), 4, HorizontalAlignment.Right, isLast: true));

                StkDataRows.Children.Add(rowGrid);

                grandTienHang += item.TienHang;
                grandGiamGia += item.GiamGia;
                grandTongCong += item.TongCong;
            }

            // Grand Total Row (TỔNG CỘNG)
            var grandGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)) };
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grandGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

            var grandLabelBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4) };
            Grid.SetColumnSpan(grandLabelBorder, 2);
            grandLabelBorder.Child = new TextBlock { Text = "TỔNG CỘNG", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 11 };
            grandGrid.Children.Add(grandLabelBorder);

            grandGrid.Children.Add(CreateCell(grandTienHang == 0 ? "0" : grandTienHang.ToString("N0"), 2, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell(grandGiamGia == 0 ? "0" : grandGiamGia.ToString("N0"), 3, HorizontalAlignment.Right, isBold: true, isGrandTotal: true));
            grandGrid.Children.Add(CreateCell(grandTongCong == 0 ? "0" : grandTongCong.ToString("N0"), 4, HorizontalAlignment.Right, isBold: true, isLast: true, isGrandTotal: true));

            StkDataRows.Children.Add(grandGrid);
        }

        private Border CreateCell(string text, int col, HorizontalAlignment align, bool isBold = false, bool isLast = false, bool isGrandTotal = false)
        {
            var border = new Border
            {
                BorderBrush = isGrandTotal ? Brushes.Black : Brushes.Gray,
                BorderThickness = new Thickness(0, 0, isLast ? 0 : 1, 1),
                Padding = new Thickness(4)
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
            };
            border.Child = tb;
            return border;
        }

        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
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
            MessageBox.Show("Đang mở chế độ xem trước trang in...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "Tong Hop Ban Hang Theo Nhom Hien Thi");
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
                    FileName = $"TongHopBanHangTheoNhomHienThi_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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

                    sb.AppendLine("STT,Nhóm hiển thị,Tiền hàng,Giảm giá,Tổng cộng");

                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"\"{item.STT}\",\"{item.NhomHienThi}\",\"{item.TienHang}\",\"{item.GiamGia}\",\"{item.TongCong}\"");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất báo cáo ra CSV/Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
