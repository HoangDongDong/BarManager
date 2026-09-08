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
    public partial class BaoCaoPhanTichTinhHinhBanHangControl : UserControl
    {
        private readonly LocalBaoCaoQuanTriService _service = new LocalBaoCaoQuanTriService();
        private bool _isLoaded = false;
        private PhanTichTinhHinhBanHangResult _data = new PhanTichTinhHinhBanHangResult();

        public BaoCaoPhanTichTinhHinhBanHangControl()
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

                TxtFilterSummary.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";

                _data = await _service.GetBaoCaoPhanTichTinhHinhBanHangAsync(tuNgay, denNgay);

                TxtTongTonKho.Text = _data.TongTonKho.ToString("#,##0.0");
                TxtSoLuongBanRa.Text = _data.SoLuongBanRa.ToString("#,##0");
                TxtThoiGianBan.Text = _data.ThoiGianBan.ToString();
                TxtBinhQuanTrongNgay.Text = _data.BinhQuanTrongNgay.ToString("0.0");

                RenderTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderTables();
        }

        private void RenderTables()
        {
            string filter = TxtSearch.Text?.Trim().ToLower() ?? "";

            // Table 1: Top Selling Items
            PnlTable1.Children.Clear();
            var g1 = new Grid();
            g1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            g1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            g1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            g1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            g1.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderCell(g1, "Tên hàng", 0, 0);
            AddHeaderCell(g1, "Giá bán", 0, 1);
            AddHeaderCell(g1, "Số lượng", 0, 2);
            AddHeaderCell(g1, "Thành tiền", 0, 3);

            var t1Items = string.IsNullOrEmpty(filter)
                ? _data.TopBanItems
                : _data.TopBanItems.Where(x => x.TenHang?.ToLower().Contains(filter) ?? false).ToList();

            int r1 = 1;
            foreach (var it in t1Items)
            {
                g1.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                AddDataCell(g1, it.TenHang, r1, 0, TextAlignment.Left);
                AddDataCell(g1, it.GiaBan > 0 ? it.GiaBan.ToString("#,##0") : "0", r1, 1, TextAlignment.Right);
                AddDataCell(g1, it.SoLuong.ToString("#,##0"), r1, 2, TextAlignment.Right);
                AddDataCell(g1, it.ThanhTien > 0 ? it.ThanhTien.ToString("#,##0") : "0", r1, 3, TextAlignment.Right);
                r1++;
            }

            // Total Table 1
            g1.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddGrandTotalCell(g1, "TỔNG CỘNG", r1, 0, 1, 2, TextAlignment.Right);
            AddGrandTotalCell(g1, t1Items.Sum(x => x.SoLuong).ToString("#,##0"), r1, 2, 1, 1, TextAlignment.Right);
            AddGrandTotalCell(g1, t1Items.Sum(x => x.ThanhTien).ToString("#,##0"), r1, 3, 1, 1, TextAlignment.Right);

            PnlTable1.Children.Add(g1);

            // Table 2: All Items Sold
            PnlTable2.Children.Clear();
            var g2 = new Grid();
            g2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            g2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            g2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            g2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

            g2.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderCell(g2, "Tên hàng", 0, 0);
            AddHeaderCell(g2, "Số lượng", 0, 1);
            AddHeaderCell(g2, "Thành tiền", 0, 2);
            AddHeaderCell(g2, "Tỷ lệ", 0, 3);

            var t2Items = string.IsNullOrEmpty(filter)
                ? _data.AllBanItems
                : _data.AllBanItems.Where(x => x.TenHang?.ToLower().Contains(filter) ?? false).ToList();

            int r2 = 1;
            foreach (var it in t2Items)
            {
                g2.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                AddDataCell(g2, it.TenHang, r2, 0, TextAlignment.Left);
                AddDataCell(g2, it.SoLuong.ToString("#,##0"), r2, 1, TextAlignment.Right);
                AddDataCell(g2, it.ThanhTien > 0 ? it.ThanhTien.ToString("#,##0") : "0", r2, 2, TextAlignment.Right);
                AddDataCell(g2, it.TyLe.ToString("0.0"), r2, 3, TextAlignment.Right);
                r2++;
            }

            PnlTable2.Children.Add(g2);

            // Table 3: Price Points Breakdown
            PnlTable3.Children.Clear();
            var g3 = new Grid();
            g3.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            g3.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            g3.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            g3.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

            g3.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderCell(g3, "Đơn giá", 0, 0);
            AddHeaderCell(g3, "Số lượng", 0, 1);
            AddHeaderCell(g3, "Thành tiền", 0, 2);
            AddHeaderCell(g3, "Tỷ lệ", 0, 3);

            int r3 = 1;
            foreach (var it in _data.MucGiaItems)
            {
                g3.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                AddDataCell(g3, it.DonGia > 0 ? it.DonGia.ToString("#,##0") : "0", r3, 0, TextAlignment.Right);
                AddDataCell(g3, it.SoLuong.ToString("#,##0"), r3, 1, TextAlignment.Right);
                AddDataCell(g3, it.ThanhTien > 0 ? it.ThanhTien.ToString("#,##0") : "0", r3, 2, TextAlignment.Right);
                AddDataCell(g3, it.TyLe.ToString("0.0"), r3, 3, TextAlignment.Right);
                r3++;
            }

            // Total Table 3
            g3.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddGrandTotalCell(g3, "TỔNG", r3, 0, 1, 1, TextAlignment.Right);
            AddGrandTotalCell(g3, _data.MucGiaItems.Sum(x => x.SoLuong).ToString("#,##0"), r3, 1, 1, 1, TextAlignment.Right);
            AddGrandTotalCell(g3, _data.MucGiaItems.Sum(x => x.ThanhTien).ToString("#,##0"), r3, 2, 1, 1, TextAlignment.Right);
            AddGrandTotalCell(g3, "", r3, 3, 1, 1, TextAlignment.Right);

            PnlTable3.Children.Add(g3);
        }

        private void AddHeaderCell(Grid g, string text, int row, int col)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 1, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(3, 4, 3, 4)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 10.5,
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
                Padding = new Thickness(3, 3, 3, 3)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 10.5,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            g.Children.Add(b);
        }

        private void AddGrandTotalCell(Grid g, string text, int row, int col, int rowSpan, int colSpan, TextAlignment align)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(col == 0 ? 1 : 0, 0, 1, 1),
                Background = new SolidColorBrush(Color.FromRgb(235, 240, 248)),
                Padding = new Thickness(3, 4, 3, 4)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 10.5,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            b.Child = tb;
            Grid.SetRow(b, row);
            Grid.SetColumn(b, col);
            if (colSpan > 1) Grid.SetColumnSpan(b, colSpan);
            g.Children.Add(b);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, "BÁO CÁO PHÂN TÍCH TÌNH HÌNH BÁN HÀNG");
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
                    FileName = $"BaoCao_PhanTichBanHang_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine("BÁO CÁO PHÂN TÍCH TÌNH HÌNH BÁN HÀNG");
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();
                    sb.AppendLine($"Tổng hàng hóa trong cửa hàng: {_data.TongTonKho},Số lượng bán ra: {_data.SoLuongBanRa},Thời gian bán: {_data.ThoiGianBan},Bình quân trong ngày: {_data.BinhQuanTrongNgay}");
                    sb.AppendLine();
                    sb.AppendLine("--- HÀNG BÁN CHẠY ---");
                    sb.AppendLine("Tên hàng,Giá bán,Số lượng,Thành tiền");
                    foreach (var it in _data.TopBanItems)
                    {
                        sb.AppendLine($"\"{it.TenHang}\",{it.GiaBan},{it.SoLuong},{it.ThanhTien}");
                    }
                    sb.AppendLine();
                    sb.AppendLine("--- TỶ LỆ MẶT HÀNG BÁN ---");
                    sb.AppendLine("Tên hàng,Số lượng,Thành tiền,Tỷ lệ %");
                    foreach (var it in _data.AllBanItems)
                    {
                        sb.AppendLine($"\"{it.TenHang}\",{it.SoLuong},{it.ThanhTien},{it.TyLe}");
                    }
                    sb.AppendLine();
                    sb.AppendLine("--- PHÂN LOẠI THEO MỨC GIÁ ---");
                    sb.AppendLine("Đơn giá,Số lượng,Thành tiền,Tỷ lệ %");
                    foreach (var it in _data.MucGiaItems)
                    {
                        sb.AppendLine($"{it.DonGia},{it.SoLuong},{it.ThanhTien},{it.TyLe}");
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
    }
}
