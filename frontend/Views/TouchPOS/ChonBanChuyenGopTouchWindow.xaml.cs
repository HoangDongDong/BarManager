using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public class TouchBanItemVM : System.ComponentModel.INotifyPropertyChanged
    {
        public DBAN Data { get; set; } = new();
        public string Id => Data.Id;
        public string Name => !string.IsNullOrWhiteSpace(Data.Name) ? Data.Name : Data.TENBAN;
        public bool IsOccupied => Data.IsOpened || !string.IsNullOrEmpty(Data.ActiveOrderId);
        public string MABAN => Data.MABAN;
        public string MAKHUVUC => Data.MAKHUVUC;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        public string BorderColor => IsSelected ? "#FFEB3B" : "#1976D2";
        public string CardBackground => IsOccupied ? "#D32F2F" : "#757575";

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
    }

    public partial class ChonBanChuyenGopTouchWindow : Window
    {
        public DBAN? SelectedTargetBan { get; private set; }
        private readonly DBAN _sourceBan;
        private readonly bool _isMergeMode;

        private List<TouchBanItemVM> _allBanVM = new();
        private List<DKHUVUC> _khuVucList = new();
        private string? _selectedKhuVucId = null;

        public ChonBanChuyenGopTouchWindow(DBAN sourceBan, bool isMergeMode = false)
        {
            InitializeComponent();
            _sourceBan = sourceBan;
            _isMergeMode = isMergeMode;

            string actionName = _isMergeMode ? "GỘP BÀN" : "CHUYỂN BÀN";
            TxtHeaderTitle.Text = $"{actionName} từ [{_sourceBan.Name}]";
        }

        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register("ColumnCount", typeof(int), typeof(ChonBanChuyenGopTouchWindow), new PropertyMetadata(2));

        public double TileHeight
        {
            get => (double)GetValue(TileHeightProperty);
            set => SetValue(TileHeightProperty, value);
        }
        public static readonly DependencyProperty TileHeightProperty =
            DependencyProperty.Register("TileHeight", typeof(double), typeof(ChonBanChuyenGopTouchWindow), new PropertyMetadata(120.0));

        public double AreaButtonHeight
        {
            get => (double)GetValue(AreaButtonHeightProperty);
            set => SetValue(AreaButtonHeightProperty, value);
        }
        public static readonly DependencyProperty AreaButtonHeightProperty =
            DependencyProperty.Register("AreaButtonHeight", typeof(double), typeof(ChonBanChuyenGopTouchWindow), new PropertyMetadata(48.0));

        private int _tableRowsConfig = 2;
        private int _areaRowsConfig = 5;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLayoutConfigsAsync();
            LoadData();
        }

        private async Task LoadLayoutConfigsAsync()
        {
            try
            {
                // 1. Ban Layout Config: cols|rows|color|showTitle
                string banVal = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_Ban", "2|2|#808080|1");
                string[] bParts = banVal.Split('|');
                int bCols = 2, bRows = 2;
                if (bParts.Length > 0 && int.TryParse(bParts[0], out int bc) && bc >= 1) bCols = bc;
                if (bParts.Length > 1 && int.TryParse(bParts[1], out int br) && br >= 1) bRows = br;

                ColumnCount = bCols;
                _tableRowsConfig = bRows;

                // 2. KhuVuc Layout Config: cols|rows|color|showTitle
                string kvVal = await LocalCauHinhService.GetConfigValueAsync("TOUCH_LAYOUT_KhuVuc", "1|5|#1976D2|1");
                string[] kvParts = kvVal.Split('|');
                int kvRows = 5;
                if (kvParts.Length > 1 && int.TryParse(kvParts[1], out int kr) && kr >= 1) kvRows = kr;
                _areaRowsConfig = kvRows;

                UpdateCalculatedHeights();
            }
            catch { }
        }

        private void UpdateCalculatedHeights()
        {
            try
            {
                double tableContainerHeight = SvTableGrid != null && SvTableGrid.ActualHeight > 50 ? SvTableGrid.ActualHeight : 600;
                int tRows = _tableRowsConfig > 0 ? _tableRowsConfig : 2;
                TileHeight = Math.Max(60, (tableContainerHeight - (tRows * 14)) / tRows);

                double areaContainerHeight = SvAreaGrid != null && SvAreaGrid.ActualHeight > 50 ? SvAreaGrid.ActualHeight : 500;
                int aRows = _areaRowsConfig > 0 ? _areaRowsConfig : 5;
                AreaButtonHeight = Math.Max(36, (areaContainerHeight - (aRows * 6)) / aRows);
            }
            catch { }
        }

        private void SvTableGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCalculatedHeights();
        }

        private void SvAreaGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCalculatedHeights();
        }

        private async void LoadData()
        {
            try
            {
                var service = new LocalSuDungDichVuService();
                var kvBanList = await service.GetKhuVucBanListAsync();

                _khuVucList = new List<DKHUVUC>
                {
                    new DKHUVUC { Id = "", Name = "TẤT CẢ", MAKHUVUC = "", ColorHex = "#1976D2" }
                };

                string[] colors = { "#E65100", "#00ACC1", "#7CB342", "#5E35B1", "#00E676", "#D81B60", "#F57C00" };
                int ci = 0;

                _allBanVM = new List<TouchBanItemVM>();

                foreach (var kv in kvBanList)
                {
                    var dkhuVuc = new DKHUVUC
                    {
                        Id = kv.Id,
                        MAKHUVUC = kv.Id,
                        Name = kv.Name,
                        TenKhuVuc = kv.Name,
                        ColorHex = colors[ci % colors.Length]
                    };
                    ci++;
                    _khuVucList.Add(dkhuVuc);

                    foreach (var b in kv.BanList)
                    {
                        if (b.Id == _sourceBan.Id) continue; // Exclude source table

                        var model = new DBAN
                        {
                            Id = b.Id,
                            Name = b.Name,
                            MABAN = b.Id,
                            MAKHUVUC = kv.Id,
                            ActiveOrderId = b.ActiveOrderId,
                            SoPhieu = b.SoPhieu,
                            IsOpened = b.IsOccupied
                        };

                        // For Move mode: destination must be EMPTY
                        // For Merge mode: destination must be OCCUPIED/OPENED
                        if (_isMergeMode)
                        {
                            if (!model.IsOpened) continue;
                        }
                        else
                        {
                            if (model.IsOpened) continue;
                        }

                        _allBanVM.Add(new TouchBanItemVM { Data = model });
                    }
                }

                IcKhuVuc.ItemsSource = _khuVucList;
                FilterTables();
            }
            catch (Exception ex)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"LỖI TẢI DANH SÁCH BÀN: {ex.Message}", "LỖI");
            }
        }


        private void FilterTables()
        {
            if (string.IsNullOrEmpty(_selectedKhuVucId))
            {
                IcTables.ItemsSource = _allBanVM;
                return;
            }

            var filtered = _allBanVM.Where(x => x.MAKHUVUC == _selectedKhuVucId || x.Data.MAKHUVUC == _selectedKhuVucId).ToList();
            IcTables.ItemsSource = filtered;
        }

        private void BtnKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DKHUVUC kv)
            {
                _selectedKhuVucId = kv.MAKHUVUC;
                FilterTables();
            }
        }

        private void TableCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is TouchBanItemVM item)
            {
                e.Handled = true;
                foreach (var vm in _allBanVM) vm.IsSelected = false;
                item.IsSelected = true;
                SelectedTargetBan = item.Data;

                ConfirmTargetSelection();
            }
        }


        private void ConfirmTargetSelection()
        {
            if (SelectedTargetBan == null)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "VUI LÒNG CHỌN BÀN ĐÍCH!", "THÔNG BÁO");
                return;
            }

            string actionText = _isMergeMode ? "GỘP" : "CHUYỂN";
            string msg = $"BẠN CÓ CHẮC CHẮN MUỐN {actionText} BÀN '{_sourceBan.Name.ToUpper()}' SANG BÀN '{SelectedTargetBan.Name.ToUpper()}' KHÔNG?";
            if (QuanLyBar.Views.TouchPOS.TouchConfirmWindow.Show(this, msg, "XÁC NHẬN"))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { DialogResult = true; } catch { }
                }));
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { DialogResult = false; } catch { }
            }));
        }

        private async void BtnConfigKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("KhuVuc");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadLayoutConfigsAsync();
                    LoadData();
                }
            }
            catch { }
        }

        private async void BtnConfigBan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CauHinhHeThong.ThietLapDinhDangThanhPhanWindow("Ban");
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    await LoadLayoutConfigsAsync();
                    LoadData();
                }
            }
            catch { }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { DialogResult = false; } catch { }
                }));
            }
        }
    }
}
