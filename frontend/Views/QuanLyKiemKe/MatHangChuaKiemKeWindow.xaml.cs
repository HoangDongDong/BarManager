using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.QuanLyKiemKe
{
    public class NhomMatHangTreeDisplayItem : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "📁";
        public ObservableCollection<NhomMatHangTreeDisplayItem> Children { get; set; } = new ObservableCollection<NhomMatHangTreeDisplayItem>();

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    public partial class MatHangChuaKiemKeWindow : Window
    {
        private HashSet<string> _daKiemKeIds;
        private List<MatHangNhapKhoItem> _allChuaKiemKe = new List<MatHangNhapKhoItem>();
        private ObservableCollection<NhomMatHangTreeDisplayItem> _treeItems = new ObservableCollection<NhomMatHangTreeDisplayItem>();
        private NhomMatHangTreeDisplayItem _selectedTreeItem;

        public event Action<MatHangNhapKhoItem> OnItemChosen;

        public MatHangChuaKiemKeWindow(HashSet<string> daKiemKeIds)
        {
            InitializeComponent();
            _daKiemKeIds = daKiemKeIds ?? new HashSet<string>();

            Loaded += MatHangChuaKiemKeWindow_Loaded;
            PreviewKeyDown += MatHangChuaKiemKeWindow_PreviewKeyDown;
        }

        private async void MatHangChuaKiemKeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTreeAsync();
            await LoadDataAsync();
        }

        private async Task LoadTreeAsync()
        {
            try
            {
                var service = new LocalMatHangService();
                var rawTree = await service.GetNhomMatHangTreeAsync();

                _treeItems.Clear();
                foreach (var node in rawTree)
                {
                    _treeItems.Add(ConvertToDisplayTree(node));
                }
                TvNhomMatHang.ItemsSource = _treeItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadTreeAsync error: " + ex.Message);
            }
        }

        private NhomMatHangTreeDisplayItem ConvertToDisplayTree(NhomMatHangViewModel vm)
        {
            string icon = "📁";
            if (string.IsNullOrEmpty(vm.Id)) icon = "🌐";
            else if (vm.Id == "-1") icon = "🗑️";
            else
            {
                string nameUpper = vm.Name?.ToUpperInvariant() ?? "";
                if (nameUpper.Contains("BÒ") || nameUpper.Contains("DÊ")) icon = "🏢";
                else if (nameUpper.Contains("CÁ")) icon = "🎗️";
                else if (nameUpper.Contains("LẨU")) icon = "🥗";
                else if (nameUpper.Contains("CHIM")) icon = "🍸";
                else if (nameUpper.Contains("NHÂN VIÊN")) icon = "👥";
                else if (nameUpper.Contains("GIA VỊ")) icon = "📦";
                else if (nameUpper.Contains("HẢI SẢN")) icon = "🔬";
                else if (nameUpper.Contains("LƯƠN") || nameUpper.Contains("ỐC")) icon = "🥢";
                else if (nameUpper.Contains("CHẾ BIẾN")) icon = "🍲";
                else if (nameUpper.Contains("RAU")) icon = "🍏";
                else if (nameUpper.Contains("TƯƠI SỐNG")) icon = "🥩";
                else if (nameUpper.Contains("LỢN") || nameUpper.Contains("HEO")) icon = "🐷";
            }

            var item = new NhomMatHangTreeDisplayItem
            {
                Id = vm.Id ?? "",
                Name = vm.Name ?? "",
                Icon = icon
            };

            if (vm.Children != null)
            {
                foreach (var child in vm.Children)
                {
                    item.Children.Add(ConvertToDisplayTree(child));
                }
            }
            return item;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var fullCatalog = await LocalNhapKhoService.GetMatHangForNhapKhoAsync();
                
                // Lọc bỏ tất cả các mặt hàng đã có trong phiếu kiểm kê
                _allChuaKiemKe = fullCatalog.Where(x => !_daKiemKeIds.Contains(x.Id)).ToList();

                ApplyFilter();
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadDataAsync error: " + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            var filtered = _allChuaKiemKe;

            if (_selectedTreeItem != null && !string.IsNullOrEmpty(_selectedTreeItem.Id) && _selectedTreeItem.Id != "-1")
            {
                filtered = filtered.Where(x => x.DnhommathangId == _selectedTreeItem.Id).ToList();
            }

            int stt = 1;
            foreach (var item in filtered)
            {
                item.Stt = stt++;
            }

            DgMatHangChuaKiemKe.ItemsSource = filtered;
        }

        private void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as NhomMatHangTreeDisplayItem;
            ApplyFilter();
        }

        private void DgMatHangChuaKiemKe_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgMatHangChuaKiemKe.SelectedItem is MatHangNhapKhoItem sel)
            {
                OnItemChosen?.Invoke(sel);
                _daKiemKeIds.Add(sel.Id);
                _allChuaKiemKe.Remove(sel);
                ApplyFilter();
            }
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MatHangChuaKiemKeWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (DgMatHangChuaKiemKe.SelectedItem is MatHangNhapKhoItem sel)
                {
                    OnItemChosen?.Invoke(sel);
                    _daKiemKeIds.Add(sel.Id);
                    _allChuaKiemKe.Remove(sel);
                    ApplyFilter();
                    e.Handled = true;
                }
            }
        }
    }
}
