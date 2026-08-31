using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class TongHopKqkdControl : UserControl
    {
        private readonly LocalHoaDonService _hoaDonService;
        private bool _isInitialized = false;
        private bool _isLoading = false;
        private List<KqkdRowViewModel> _cachedList = new List<KqkdRowViewModel>();

        public TongHopKqkdControl()
        {
            InitializeComponent();
            _hoaDonService = new LocalHoaDonService();
            this.Loaded += TongHopKqkdControl_Loaded;
            this.IsVisibleChanged += TongHopKqkdControl_IsVisibleChanged;
        }

        private async void TongHopKqkdControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && _isInitialized)
            {
                await LoadDataAsync();
            }
        }

        private async void TongHopKqkdControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                await LoadDataAsync();
                return;
            }

            dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpDenNgay.SelectedDate = DateTime.Today;
            _isInitialized = true;

            await LoadCuaHangListAsync();
            await LoadDataAsync();
        }

        private async Task LoadCuaHangListAsync()
        {
            try
            {
                var stores = await _hoaDonService.GetCuaHangListAsync();
                LstCuaHang.ItemsSource = stores;
                if (stores.Count > 0 && string.IsNullOrEmpty(TxtSelectedCuaHang.Text))
                {
                    TxtSelectedCuaHang.Text = stores[0].Name;
                    TxtTenCuaHang.Text = stores[0].Name.ToUpper();
                }
            }
            catch { }
        }

        private void TxtSelectedCuaHang_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnToggleCuaHang.IsChecked = !BtnToggleCuaHang.IsChecked;
        }

        private void LstCuaHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCuaHang.SelectedItem is CuaHangViewModel sel)
            {
                TxtSelectedCuaHang.Text = sel.Name;
                TxtTenCuaHang.Text = sel.Name.ToUpper();
                BtnToggleCuaHang.IsChecked = false;
            }
        }

        private async void BtnThemCuaHang_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new Window
            {
                Title = "Thêm Trụ sở / Cửa hàng mới",
                Width = 360,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(220, 232, 245))
            };

            var grid = new Grid { Margin = new Thickness(12) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lbl = new TextBlock { Text = "Tên trụ sở / cửa hàng:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
            var txt = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10), VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetRow(lbl, 0);
            Grid.SetRow(txt, 1);

            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnSave = new Button { Content = "Lưu", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true, Background = Brushes.White };
            var btnCancel = new Button { Content = "Đóng", Width = 75, Height = 26, IsCancel = true, Background = Brushes.White };
            sp.Children.Add(btnSave);
            sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2);

            grid.Children.Add(lbl);
            grid.Children.Add(txt);
            grid.Children.Add(sp);
            inputWin.Content = grid;

            btnSave.Click += async (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên cửa hàng/trụ sở!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await _hoaDonService.InsertCuaHangAsync(txt.Text.Trim());
                inputWin.DialogResult = true;
                inputWin.Close();
            };

            if (inputWin.ShowDialog() == true)
            {
                await LoadCuaHangListAsync();
            }
        }

        private async void BtnTaiCuaHang_Click(object sender, RoutedEventArgs e)
        {
            await LoadCuaHangListAsync();
        }

        private void BtnDanhMucCuaHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Danh mục cửa hàng/trụ sở đã được liệt kê trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized && !_isLoading)
            {
                await LoadDataAsync();
            }
        }

        private async void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                var tuNgay = dpTuNgay.SelectedDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var denNgay = dpDenNgay.SelectedDate ?? DateTime.Today;

                TxtKhoangThoiGian.Text = $"Từ ngày {tuNgay:dd/MM/yyyy} Đến ngày {denNgay:dd/MM/yyyy}";
                TxtNgayLap.Text = $"Ngày {DateTime.Now.Day:D2} tháng {DateTime.Now.Month:D2} năm {DateTime.Now.Year}";

                _cachedList = await Task.Run(async () => await _hoaDonService.GetTongHopKqkdAsync(tuNgay, denNgay));
                IctKqkd.ItemsSource = _cachedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(PaperContainer, "BÁO CÁO KẾT QUẢ KINH DOANH");
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
                    FileName = $"BaoCaoKQKD_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (_cachedList != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("STT,Khoản mục,% / DT,Giá trị,%,%/CP,Tăng giảm so với tháng trước,KQKD tháng trước");
                        foreach (var item in _cachedList)
                        {
                            sb.AppendLine($"\"{item.Stt}\",\"{item.ChiTieu}\",\"{item.PhanTramDt}\",\"{item.GiaTri}\",\"{item.PhanTram}\",\"{item.PhanTramCp}\",\"{item.TangGiam}\",\"{item.KqThangTruoc}\"");
                        }
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
