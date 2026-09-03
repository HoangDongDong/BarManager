using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.QuanLyNhapKho
{
    public class LabelPrintItem
    {
        public string TenHang { get; set; } = "";
        public string MaHang { get; set; } = "";
        public decimal Gia { get; set; }
    }

    public partial class XemInMaVachWindow : Window
    {
        private MauInMaVachItem _mau;
        private List<PhieuNhapChiTietItem> _details;
        private List<LabelPrintItem> _flatLabels = new();
        private List<Border> _renderedPageBorders = new();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private double _zoomFactor = 1.0;

        public XemInMaVachWindow(MauInMaVachItem mau, List<PhieuNhapChiTietItem> details)
        {
            InitializeComponent();
            _mau = mau ?? new MauInMaVachItem { Columns = 2 };
            _details = details ?? new List<PhieuNhapChiTietItem>();

            Loaded += XemInMaVachWindow_Loaded;
        }

        private void XemInMaVachWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareFlatLabels();
            RenderLabels();
        }

        private void PrepareFlatLabels()
        {
            _flatLabels.Clear();
            foreach (var item in _details)
            {
                int qty = (int)Math.Max(1, Math.Round(item.SlNhap));
                string code = item.MaHang?.Trim() ?? "";
                for (int i = 0; i < qty; i++)
                {
                    _flatLabels.Add(new LabelPrintItem
                    {
                        TenHang = item.TenHang,
                        MaHang = code,
                        Gia = item.GiaBan
                    });
                }
            }

            if (_flatLabels.Count == 0)
            {
                _flatLabels.Add(new LabelPrintItem
                {
                    TenHang = "Mặt hàng mẫu",
                    MaHang = "12345678",
                    Gia = 50000
                });
            }
        }

        private void RenderLabels()
        {
            WpPagesContainer.Children.Clear();
            _renderedPageBorders.Clear();

            int cols = _mau.Columns > 0 ? _mau.Columns : 2;
            int rows = _mau.RowsPerPage > 0 ? _mau.RowsPerPage : 1;
            int perPage = cols * rows;

            _totalPages = (int)Math.Ceiling((double)_flatLabels.Count / perPage);
            if (_totalPages < 1) _totalPages = 1;

            TxtTotalPages.Text = $"of {_totalPages}";
            TxtCurrentPage.Text = "1";
            _currentPage = 1;

            double labelWidth = cols == 2 ? 160 : (cols == 3 ? 125 : 110);
            double labelHeight = 82;
            double pageBorderWidth = (labelWidth * cols) + 20;
            double pageBorderHeight = (labelHeight * rows) + 16;

            if (_mau.IsPaperA4)
            {
                pageBorderWidth = 650;
                pageBorderHeight = 850;
            }

            for (int p = 0; p < _totalPages; p++)
            {
                var pageLabels = _flatLabels.Skip(p * perPage).Take(perPage).ToList();

                var pageBorder = new Border
                {
                    Width = pageBorderWidth * _zoomFactor,
                    Height = pageBorderHeight * _zoomFactor,
                    Background = Brushes.White,
                    BorderBrush = p == 0 ? new SolidColorBrush(Color.FromRgb(245, 166, 35)) : new SolidColorBrush(Color.FromRgb(200, 210, 225)),
                    BorderThickness = p == 0 ? new Thickness(2.5) : new Thickness(1),
                    CornerRadius = new CornerRadius(1),
                    Margin = new Thickness(12, 10, 12, 10),
                    Padding = new Thickness(8),
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Opacity = 0.25,
                        BlurRadius = 8,
                        ShadowDepth = 3
                    },
                    Tag = p + 1
                };

                pageBorder.MouseDown += (s, e) =>
                {
                    if (s is Border b && b.Tag is int pageNum)
                    {
                        SelectPage(pageNum);
                    }
                };

                var uniformGrid = new UniformGrid
                {
                    Columns = cols,
                    Rows = rows,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                for (int i = 0; i < perPage; i++)
                {
                    if (i < pageLabels.Count)
                    {
                        var lbl = pageLabels[i];
                        var labelContainer = CreateSingleLabelVisual(lbl, labelWidth, labelHeight);
                        uniformGrid.Children.Add(labelContainer);
                    }
                    else
                    {
                        // Empty slot to keep grid aligned
                        var emptyBox = new Border
                        {
                            Background = Brushes.Transparent,
                            Margin = new Thickness(4)
                        };
                        uniformGrid.Children.Add(emptyBox);
                    }
                }

                pageBorder.Child = uniformGrid;
                _renderedPageBorders.Add(pageBorder);
                WpPagesContainer.Children.Add(pageBorder);
            }
        }

        private UIElement CreateSingleLabelVisual(LabelPrintItem item, double width, double height)
        {
            var labelBorder = new Border
            {
                Background = Brushes.White,
                Margin = new Thickness(3, 2, 3, 2),
                Padding = new Thickness(2)
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 1. Tên mặt hàng
            var txtName = new TextBlock
            {
                Text = item.TenHang,
                FontWeight = FontWeights.Bold,
                FontSize = 9.5 * _zoomFactor,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = (width - 10) * _zoomFactor,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 1)
            };
            stack.Children.Add(txtName);

            // 2 & 3. Hình mã vạch Code 128 và số mã (nếu không có mã hàng thì để trắng hoàn toàn)
            if (!string.IsNullOrWhiteSpace(item.MaHang))
            {
                try
                {
                    var barcodeImg = BarcodeHelper.GenerateCode128Barcode(item.MaHang, (int)(32 * _zoomFactor), 2);
                    var img = new Image
                    {
                        Source = barcodeImg,
                        Height = 28 * _zoomFactor,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 1, 0, 0)
                    };
                    stack.Children.Add(img);
                }
                catch
                {
                    var txtErr = new TextBlock
                    {
                        Text = item.MaHang,
                        Foreground = Brushes.Red,
                        FontSize = 9,
                        TextAlignment = TextAlignment.Center
                    };
                    stack.Children.Add(txtErr);
                }

                var txtCode = new TextBlock
                {
                    Text = item.MaHang,
                    FontSize = 8.5 * _zoomFactor,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, -1, 0, 1)
                };
                stack.Children.Add(txtCode);
            }
            else
            {
                // Để khoảng trống giữ chiều cao tem đồng đều
                var emptySpace = new FrameworkElement
                {
                    Height = (28 + 12) * _zoomFactor
                };
                stack.Children.Add(emptySpace);
            }

            // 4. Giá bán
            var txtPrice = new TextBlock
            {
                Text = $"Giá: {item.Gia:N0}",
                FontSize = 9 * _zoomFactor,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(txtPrice);

            labelBorder.Child = stack;
            return labelBorder;
        }

        private void SelectPage(int pageNum)
        {
            if (pageNum < 1 || pageNum > _totalPages) return;
            _currentPage = pageNum;
            TxtCurrentPage.Text = _currentPage.ToString();

            for (int i = 0; i < _renderedPageBorders.Count; i++)
            {
                if (i == _currentPage - 1)
                {
                    _renderedPageBorders[i].BorderBrush = new SolidColorBrush(Color.FromRgb(245, 166, 35));
                    _renderedPageBorders[i].BorderThickness = new Thickness(2.5);
                    _renderedPageBorders[i].BringIntoView();
                }
                else
                {
                    _renderedPageBorders[i].BorderBrush = new SolidColorBrush(Color.FromRgb(200, 210, 225));
                    _renderedPageBorders[i].BorderThickness = new Thickness(1);
                }
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    if (_renderedPageBorders.Count > 0)
                    {
                        var activeBorder = _renderedPageBorders[_currentPage - 1];
                        printDlg.PrintVisual(activeBorder, "In mã vạch");
                        MessageBox.Show("Đã gửi lệnh in thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in mã vạch: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã lưu mẫu thiết kế in mã vạch thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFirstPage_Click(object sender, RoutedEventArgs e) => SelectPage(1);
        private void BtnPrevPage_Click(object sender, RoutedEventArgs e) => SelectPage(_currentPage - 1);
        private void BtnNextPage_Click(object sender, RoutedEventArgs e) => SelectPage(_currentPage + 1);
        private void BtnLastPage_Click(object sender, RoutedEventArgs e) => SelectPage(_totalPages);

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_zoomFactor < 2.0)
            {
                _zoomFactor += 0.15;
                RenderLabels();
                SelectPage(_currentPage);
            }
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_zoomFactor > 0.6)
            {
                _zoomFactor -= 0.15;
                RenderLabels();
                SelectPage(_currentPage);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
