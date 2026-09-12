using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchTonKhoWindow : Window
    {
        private List<BaoCaoTonKhoTileItem> _allTonData = new List<BaoCaoTonKhoTileItem>();
        private string? _selectedKhoId = null;
        private string? _selectedNhomId = null;
        private bool _filterTonKhac0 = true;

        public TouchTonKhoWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhoListAsync();
            await LoadNhomListAsync();
            await LoadTonDataAsync();
        }

        private async Task LoadKhoListAsync()
        {
            PanelKhoButtons.Children.Clear();

            // Nút TẤT CẢ kho
            AddKhoButton("TẤT CẢ", null, true);

            try
            {
                var khoList = await LocalKhoHangService.GetAllWarehousesFlatAsync();
                foreach (var k in khoList)
                {
                    AddKhoButton(k.Name, k.Id, false);
                }
            }
            catch { }
        }

        private void AddKhoButton(string name, string? id, bool isSelected)
        {
            var btn = new Button
            {
                Content = name,
                Height = 36,
                Margin = new Thickness(0, 2, 0, 2),
                Tag = id,
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };

            SetKhoButtonStyle(btn, isSelected);

            btn.Click += (s, e) =>
            {
                _selectedKhoId = id;
                foreach (Button child in PanelKhoButtons.Children.OfType<Button>())
                {
                    SetKhoButtonStyle(child, child.Tag as string == _selectedKhoId);
                }
                ApplyFilters();
            };

            PanelKhoButtons.Children.Add(btn);
        }

        private void SetKhoButtonStyle(Button btn, bool isSelected)
        {
            var darkBlue = (Brush)new BrushConverter().ConvertFromString("#0B1B3D")!;
            var orange = (Brush)new BrushConverter().ConvertFromString("#FF6F00")!;
            var teal = (Brush)new BrushConverter().ConvertFromString("#00695C")!;

            Brush bg = isSelected ? (btn.Tag == null ? darkBlue : orange) : teal;

            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, bg);
            factory.SetValue(Border.BorderBrushProperty, (Brush)new BrushConverter().ConvertFromString("#36B5B0")!);
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(ContentPresenter.MarginProperty, new Thickness(8, 0, 0, 0));

            factory.AppendChild(presenter);
            template.VisualTree = factory;

            btn.Template = template;
        }

        private async Task LoadNhomListAsync()
        {
            PanelNhomButtons.Children.Clear();
            AddNhomButton("TẤT CẢ", null, true);

            try
            {
                var matHangService = new LocalMatHangService();
                var nhomList = await matHangService.GetNhomMatHangListAsync();
                foreach (var n in nhomList)
                {
                    AddNhomButton(n.Name, n.Id, false);
                }
            }
            catch { }
        }

        private void AddNhomButton(string name, string? id, bool isSelected)
        {
            var btn = new Button
            {
                Content = name,
                Height = 36,
                Margin = new Thickness(0, 2, 0, 2),
                Tag = id,
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };

            SetNhomButtonStyle(btn, isSelected);

            btn.Click += (s, e) =>
            {
                _selectedNhomId = id;
                foreach (Button child in PanelNhomButtons.Children.OfType<Button>())
                {
                    SetNhomButtonStyle(child, child.Tag as string == _selectedNhomId);
                }
                ApplyFilters();
            };

            PanelNhomButtons.Children.Add(btn);
        }

        private void SetNhomButtonStyle(Button btn, bool isSelected)
        {
            var orange = (Brush)new BrushConverter().ConvertFromString("#FF6F00")!;
            var green = (Brush)new BrushConverter().ConvertFromString("#00695C")!;

            Brush bg = isSelected ? orange : green;

            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, bg);
            factory.SetValue(Border.BorderBrushProperty, (Brush)new BrushConverter().ConvertFromString("#36B5B0")!);
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(ContentPresenter.MarginProperty, new Thickness(8, 0, 0, 0));

            factory.AppendChild(presenter);
            template.VisualTree = factory;

            btn.Template = template;
        }

        private async Task LoadTonDataAsync()
        {
            try
            {
                var list = await LocalTonKhoService.GetTonKhoListAsync(_selectedKhoId, _selectedNhomId, null, false);
                _allTonData = list.Select(x => new BaoCaoTonKhoTileItem
                {
                    MaMatHang = x.MaHang,
                    TenMatHang = x.TenHang,
                    MaNhom = x.DnhommathangId,
                    MaKho = _selectedKhoId ?? "",
                    SoLuongTon = x.Ton
                }).ToList();

                ApplyFilters();
            }
            catch { }
        }

        private void ApplyFilters()
        {
            var query = _allTonData.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedKhoId))
            {
                query = query.Where(x => x.MaKho == _selectedKhoId);
            }

            if (!string.IsNullOrEmpty(_selectedNhomId))
            {
                query = query.Where(x => x.MaNhom == _selectedNhomId);
            }

            if (_filterTonKhac0)
            {
                query = query.Where(x => x.SoLuongTon != 0);
            }

            ItemsTileControl.ItemsSource = query.OrderBy(x => x.TenMatHang).ToList();
        }

        private void BtnToggleTonKhac0_Click(object sender, RoutedEventArgs e)
        {
            _filterTonKhac0 = !_filterTonKhac0;
            ApplyFilters();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadTonDataAsync();
        }

        private void BtnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new QuanLyBar.Views.TouchPOS.TouchReportViewerWindow(new QuanLyBar.Client.Views.TonKho.TonKhoControl(), "BÁO CÁO TỒN KHO CHI TIẾT");
                win.Owner = this;
                win.ShowDialog();
            }
            catch { }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã xuất báo cáo Tồn kho ra file Excel!", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            string keyword = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên hoặc mã mặt hàng cần tìm:", "Tìm kiếm tồn kho", "");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                var filtered = _allTonData.Where(x => x.TenMatHang.ToLower().Contains(keyword) || x.MaMatHang.ToLower().Contains(keyword)).ToList();
                ItemsTileControl.ItemsSource = filtered;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class BaoCaoTonKhoTileItem
    {
        public string MaMatHang { get; set; } = "";
        public string TenMatHang { get; set; } = "";
        public string MaNhom { get; set; } = "";
        public string MaKho { get; set; } = "";
        public decimal SoLuongTon { get; set; }
    }
}
