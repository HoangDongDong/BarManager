using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly List<HoaDonViewModel> _hoaDonList;
        private readonly string _templateType;
        private readonly string _storeName;
        private readonly DateTime _tuNgay;
        private readonly DateTime _denNgay;

        public PrintPreviewWindow(
            IEnumerable<object> data,
            string templateType = "Báo cáo tổng hợp thanh toán A4",
            string storeName = "NÀNG HƯƠNG QUÁN",
            DateTime? tuNgay = null,
            DateTime? denNgay = null)
        {
            InitializeComponent();

            _templateType = templateType;
            _storeName = string.IsNullOrEmpty(storeName) ? "NÀNG HƯƠNG QUÁN" : storeName;
            _tuNgay = tuNgay ?? DateTime.Today;
            _denNgay = denNgay ?? DateTime.Today;

            _hoaDonList = new List<HoaDonViewModel>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item is HoaDonViewModel hd)
                    {
                        _hoaDonList.Add(hd);
                    }
                }
            }

            BuildReportView();
        }

        // Backward compatibility constructor
        public PrintPreviewWindow(IEnumerable<object> data, List<InLuoiWindow.ColumnInfo> columns, string title, string note, string templateType, bool inSTT)
            : this(data, templateType, "NÀNG HƯƠNG QUÁN", DateTime.Today, DateTime.Today)
        {
        }

        private void BuildReportView()
        {
            TxtTenCuaHang.Text = _storeName.ToUpper();
            
            if (_templateType.Contains("bán hàng", StringComparison.OrdinalIgnoreCase))
            {
                TxtTieuDeBaoCao.Text = "BÁO CÁO TỔNG HỢP BÁN HÀNG";
            }
            else if (_templateType.Contains("thanh toán", StringComparison.OrdinalIgnoreCase))
            {
                TxtTieuDeBaoCao.Text = "BÁO CÁO TỔNG HỢP THANH TOÁN";
            }
            else
            {
                TxtTieuDeBaoCao.Text = $"BÁO CÁO TỔNG HỢP ({_templateType})";
            }

            TxtKhoangThoiGian.Text = $"Từ ngày {_tuNgay:dd/MM/yyyy} đến ngày {_denNgay:dd/MM/yyyy}";
            TxtNgayLap.Text = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";

            // Định dạng kích thước khổ in
            if (_templateType.StartsWith("58", StringComparison.OrdinalIgnoreCase))
            {
                PrintContentPanel.Width = 240;
                PaperContainer.Padding = new Thickness(10);
            }
            else if (_templateType.StartsWith("80", StringComparison.OrdinalIgnoreCase))
            {
                PrintContentPanel.Width = 320;
                PaperContainer.Padding = new Thickness(12);
            }
            else
            {
                PrintContentPanel.Width = 520;
                PaperContainer.Padding = new Thickness(25);
            }

            // Xây dựng bảng dữ liệu
            GridTable.Children.Clear();
            GridTable.ColumnDefinitions.Clear();
            GridTable.RowDefinitions.Clear();

            // Cột: TT (40), Số phiếu (100), Giờ (70), Tổng cộng (110)
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            GridTable.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            // Hàng Tiêu đề Cột (Header)
            GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddTableCell(0, 0, "TT", FontWeights.Bold, HorizontalAlignment.Center);
            AddTableCell(0, 1, "Số phiếu", FontWeights.Bold, HorizontalAlignment.Center);
            AddTableCell(0, 2, "Giờ", FontWeights.Bold, HorizontalAlignment.Center);
            AddTableCell(0, 3, "Tổng cộng", FontWeights.Bold, HorizontalAlignment.Center);

            int rowIndex = 1;
            decimal tongCongTatCa = 0;

            foreach (var item in _hoaDonList)
            {
                GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                string sttStr = rowIndex.ToString();
                string soPhieu = item.SoPhieu ?? "";
                string gioStr = item.GioThanhToan?.ToString("HH:mm") ?? item.KetThuc?.ToString("HH:mm") ?? item.BatDau?.ToString("HH:mm") ?? "";
                string tongCongStr = item.TongCong.ToString("N0");
                tongCongTatCa += item.TongCong;

                AddTableCell(rowIndex, 0, sttStr, FontWeights.Normal, HorizontalAlignment.Center);
                AddTableCell(rowIndex, 1, soPhieu, FontWeights.Normal, HorizontalAlignment.Center);
                AddTableCell(rowIndex, 2, gioStr, FontWeights.Normal, HorizontalAlignment.Center);
                AddTableCell(rowIndex, 3, tongCongStr, FontWeights.Normal, HorizontalAlignment.Right, 6);

                rowIndex++;
            }

            // Hàng Tổng Cộng cuối cùng
            GridTable.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Cell Tổng cộng span 3 cột đầu
            var totalHeaderCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 4, 6, 4)
            };
            var totalHeaderTxt = new TextBlock
            {
                Text = "TỔNG CỘNG",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            totalHeaderCell.Child = totalHeaderTxt;
            Grid.SetRow(totalHeaderCell, rowIndex);
            Grid.SetColumn(totalHeaderCell, 0);
            Grid.SetColumnSpan(totalHeaderCell, 3);
            GridTable.Children.Add(totalHeaderCell);

            // Cell Giá trị Tổng cộng
            var totalValCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 4, 6, 4)
            };
            var totalValTxt = new TextBlock
            {
                Text = tongCongTatCa.ToString("N0"),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            totalValCell.Child = totalValTxt;
            Grid.SetRow(totalValCell, rowIndex);
            Grid.SetColumn(totalValCell, 3);
            GridTable.Children.Add(totalValCell);
        }

        private void AddTableCell(int row, int col, string text, FontWeight weight, HorizontalAlignment align, double rightPadding = 0)
        {
            var cell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 3, Math.Max(4, rightPadding), 3)
            };
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = weight,
                HorizontalAlignment = align,
                FontSize = 11
            };
            cell.Child = tb;
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            GridTable.Children.Add(cell);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(PaperContainer, TxtTieuDeBaoCao.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"{TxtTieuDeBaoCao.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("TT,Số phiếu,Giờ,Tổng cộng");
                    int stt = 1;
                    foreach (var hd in _hoaDonList)
                    {
                        string gio = hd.GioThanhToan?.ToString("HH:mm") ?? hd.KetThuc?.ToString("HH:mm") ?? hd.BatDau?.ToString("HH:mm") ?? "";
                        sb.AppendLine($"\"{stt++}\",\"{hd.SoPhieu}\",\"{gio}\",\"{hd.TongCong}\"");
                    }
                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
