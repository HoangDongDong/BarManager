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
    public partial class BaoCaoTongHopBanHangTheoKhuVucControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService = new LocalHoaDonService();

        private bool _isLoaded = false;
        private List<TongHopBanHangTheoKhuVucItem> _allData = new List<TongHopBanHangTheoKhuVucItem>();

        public BaoCaoTongHopBanHangTheoKhuVucControl(string tabName = "TỔNG HỢP BÁN HÀNG THEO KHU VỰC")
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
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            _allData = await _hoaDonService.GetTongHopBanHangTheoKhuVucAsync(tuNgay, denNgay);

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();

            string keyword = TxtFilter.Text.Trim().ToLower();
            var filtered = _allData;
            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = _allData.Where(x => x.KhuVuc.ToLower().Contains(keyword)).ToList();
            }

            if (filtered.Count == 0)
            {
                var emptyRow = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(6) };
                emptyRow.Child = new TextBlock { Text = "Không có dữ liệu báo cáo", FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Gray };
                StkDataRows.Children.Add(emptyRow);
                return;
            }

            int stt = 1;
            decimal grandTienHang = 0;
            decimal grandGiamGia = 0;
            decimal grandTongCong = 0;

            foreach (var item in filtered)
            {
                grandTienHang += item.TienHang;
                grandGiamGia += item.GiamGia;
                grandTongCong += item.TongCong;

                var rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // STT
                rowGrid.Children.Add(CreateTableCell((stt++).ToString(), 0, HorizontalAlignment.Center));
                // Khu vực
                rowGrid.Children.Add(CreateTableCell(item.KhuVuc, 1, HorizontalAlignment.Left));
                // Tiền hàng
                rowGrid.Children.Add(CreateTableCell(item.TienHang.ToString("#,##0"), 2, HorizontalAlignment.Right));
                // Giảm giá
                rowGrid.Children.Add(CreateTableCell(item.GiamGia.ToString("#,##0"), 3, HorizontalAlignment.Right));
                // Tổng cộng
                rowGrid.Children.Add(CreateTableCell(item.TongCong.ToString("#,##0"), 4, HorizontalAlignment.Right, isLast: true));

                StkDataRows.Children.Add(rowGrid);
            }

            // Grand Total Row (TỔNG CỘNG)
            var totalGrid = new Grid { Background = new SolidColorBrush(Color.FromRgb(235, 240, 248)) };
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) }); // STT + KhuVuc
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            totalGrid.Children.Add(CreateTableCell("TỔNG CỘNG", 0, HorizontalAlignment.Center, isBold: true));
            totalGrid.Children.Add(CreateTableCell(grandTienHang.ToString("#,##0"), 1, HorizontalAlignment.Right, isBold: true));
            totalGrid.Children.Add(CreateTableCell(grandGiamGia.ToString("#,##0"), 2, HorizontalAlignment.Right, isBold: true));
            totalGrid.Children.Add(CreateTableCell(grandTongCong.ToString("#,##0"), 3, HorizontalAlignment.Right, isBold: true, isLast: true));

            StkDataRows.Children.Add(totalGrid);
        }

        private Border CreateTableCell(string text, int columnIndex, HorizontalAlignment align, bool isBold = false, bool isLast = false)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, isLast ? 0 : 1, 1),
                Padding = new Thickness(4, 3, 4, 3)
            };

            Grid.SetColumn(border, columnIndex);

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

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderTable();
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
                    printDlg.PrintVisual(ReportPaper, "Tổng hợp bán hàng theo khu vực");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"TongHopBanHangTheoKhuVuc_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtSubTitleDate.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("STT,Khu vực,Tiền hàng,Giảm giá,Tổng cộng");

                    int stt = 1;
                    foreach (var item in _allData)
                    {
                        sb.AppendLine($"{stt++},\"{item.KhuVuc}\",{item.TienHang},{item.GiamGia},{item.TongCong}");
                    }

                    sb.AppendLine($",TỔNG CỘNG,{_allData.Sum(x => x.TienHang)},{_allData.Sum(x => x.GiamGia)},{_allData.Sum(x => x.TongCong)}");

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel/CSV thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
