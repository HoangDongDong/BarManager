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
    public partial class BaoCaoDoiChieuCongNoNccControl : UserControl
    {
        private bool _isLoaded = false;
        private DoiChieuCongNoResult _data = new DoiChieuCongNoResult();

        public BaoCaoDoiChieuCongNoNccControl()
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
                var nccList = await LocalBaoCaoCongNoService.GetNccDropdownAsync();
                CboNhaCungCap.ItemsSource = nccList;
                if (nccList.Count > 0)
                {
                    CboNhaCungCap.SelectedIndex = 0;
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

            string nccId = (CboNhaCungCap.SelectedItem as CongNoFilterComboItem)?.Id ?? "";
            string nccName = (CboNhaCungCap.SelectedItem as CongNoFilterComboItem)?.Name ?? "";

            TxtSubTitleDate.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(nccName)) parts.Add($"Nhà cung cấp: {nccName}");
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

            _data = await LocalBaoCaoCongNoService.GetDoiChieuCongNoNccAsync(tuNgay, denNgay, nccId);

            TxtNoDauKy.Text = _data.NoDauKy.ToString("N0");
            TxtPhatSinhMua.Text = _data.PhatSinhMua.ToString("N0");
            TxtThanhToan.Text = _data.ThanhToan.ToString("N0");
            TxtNoCuoiKy.Text = _data.NoCuoiKy.ToString("N0");

            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            StkPaymentRows.Children.Clear();

            string keyword = TxtFilter.Text.Trim().ToLower();

            // 1. Render Mua - Trả
            var phieuList = _data.DanhSachMua;
            if (!string.IsNullOrEmpty(keyword))
            {
                phieuList = phieuList.Where(p => 
                    p.SoPhieu.ToLower().Contains(keyword) || 
                    p.ChiTiet.Any(c => c.TenHang.ToLower().Contains(keyword) || c.MaHang.ToLower().Contains(keyword))
                ).ToList();
            }

            foreach (var phieu in phieuList)
            {
                // Group Header row per Invoice / Receipt
                Border groupHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 248, 252)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 4, 6, 4)
                };

                Grid headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

                TextBlock txtSo = new TextBlock { Text = $"Số: {phieu.SoPhieu}", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
                TextBlock txtNgay = new TextBlock { Text = $"Ngày: {phieu.Ngay:dd/MM/yyyy}", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
                TextBlock txtTong = new TextBlock { Text = $"Tổng: {phieu.TongTien:N0}", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                TextBlock txtTra = new TextBlock { Text = $"Thanh toán: {phieu.DaThanhToan:N0}", FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 50)), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };

                Grid.SetColumn(txtSo, 0);
                Grid.SetColumn(txtNgay, 1);
                Grid.SetColumn(txtTong, 2);
                Grid.SetColumn(txtTra, 3);

                headerGrid.Children.Add(txtSo);
                headerGrid.Children.Add(txtNgay);
                headerGrid.Children.Add(txtTong);
                headerGrid.Children.Add(txtTra);

                groupHeader.Child = headerGrid;
                StkDataRows.Children.Add(groupHeader);

                // Chi tiết hàng
                int ctIndex = 0;
                foreach (var item in phieu.ChiTiet)
                {
                    if (!string.IsNullOrEmpty(keyword) && !item.TenHang.ToLower().Contains(keyword) && !item.MaHang.ToLower().Contains(keyword) && !phieu.SoPhieu.ToLower().Contains(keyword))
                    {
                        continue;
                    }

                    Border rowBorder = new Border
                    {
                        Background = ctIndex % 2 == 1 ? new SolidColorBrush(Color.FromRgb(250, 250, 250)) : Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };

                    Grid rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                    rowGrid.Children.Add(CreateCell(item.Stt.ToString(), 0, TextAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.MaHang, 1, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.TenHang, 2, TextAlignment.Left));
                    rowGrid.Children.Add(CreateCell(item.Dvt, 3, TextAlignment.Center));
                    rowGrid.Children.Add(CreateCell(item.SoLuong.ToString("N0"), 4, TextAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.DonGia.ToString("N0"), 5, TextAlignment.Right));
                    rowGrid.Children.Add(CreateCell(item.ThanhTien.ToString("N0"), 6, TextAlignment.Right, isLast: true));

                    rowBorder.Child = rowGrid;
                    StkDataRows.Children.Add(rowBorder);
                    ctIndex++;
                }
            }

            // 2. Render Thanh toán phiếu chi
            if (_data.DanhSachThanhToan.Count == 0)
            {
                LblThanhToanSection.Visibility = Visibility.Collapsed;
                BrdThanhToan.Visibility = Visibility.Collapsed;
            }
            else
            {
                LblThanhToanSection.Visibility = Visibility.Visible;
                BrdThanhToan.Visibility = Visibility.Visible;

                int payIndex = 0;
                foreach (var pay in _data.DanhSachThanhToan)
                {
                    Border payBorder = new Border
                    {
                        Background = payIndex % 2 == 1 ? new SolidColorBrush(Color.FromRgb(250, 250, 250)) : Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };

                    Grid payGrid = new Grid();
                    payGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                    payGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                    payGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                    payGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    payGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                    payGrid.Children.Add(CreateCell(pay.Stt.ToString(), 0, TextAlignment.Center));
                    payGrid.Children.Add(CreateCell(pay.SoPhieu, 1, TextAlignment.Left));
                    payGrid.Children.Add(CreateCell(pay.Ngay?.ToString("dd/MM/yyyy") ?? "", 2, TextAlignment.Center));
                    payGrid.Children.Add(CreateCell(pay.DienGiai, 3, TextAlignment.Left));
                    payGrid.Children.Add(CreateCell(pay.SoTien.ToString("N0"), 4, TextAlignment.Right, isLast: true, isBold: true));

                    payBorder.Child = payGrid;
                    StkPaymentRows.Children.Add(payBorder);
                    payIndex++;
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
                    printDlg.PrintVisual(ReportPaper, "Doi Chieu Cong No Nha Cung Cap");
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
                    FileName = $"DoiChieuCongNoNCC_{_data.MaDoiTuong}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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
                    sb.AppendLine("MUA - TRẢ");
                    sb.AppendLine("Số phiếu,Ngày,Mã hàng,Tên hàng,ĐVT,Số lượng,Đơn giá,Thành tiền");

                    foreach (var p in _data.DanhSachMua)
                    {
                        foreach (var c in p.ChiTiet)
                        {
                            sb.AppendLine($"\"{p.SoPhieu}\",\"{p.Ngay:dd/MM/yyyy}\",\"{c.MaHang}\",\"{c.TenHang}\",\"{c.Dvt}\",{c.SoLuong},{c.DonGia},{c.ThanhTien}");
                        }
                    }

                    sb.AppendLine("");
                    sb.AppendLine("TỔNG HỢP");
                    sb.AppendLine($"NỢ ĐẦU KỲ,{_data.NoDauKy}");
                    sb.AppendLine($"PHÁT SINH MUA TRONG KỲ,{_data.PhatSinhMua}");
                    sb.AppendLine($"THANH TOÁN TRONG KỲ,{_data.ThanhToan}");
                    sb.AppendLine($"NỢ CUỐI KỲ,{_data.NoCuoiKy}");

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
