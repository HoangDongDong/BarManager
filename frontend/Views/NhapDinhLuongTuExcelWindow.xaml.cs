using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class NhapDinhLuongTuExcelWindow : Window
    {
        private List<DinhLuongImportViewModel> _allData;
        private ObservableCollection<DinhLuongImportViewModel> _displayData;
        private LocalMatHangService _matHangService;
        
        public NhapDinhLuongTuExcelWindow(List<DinhLuongImportViewModel> data)
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _allData = data ?? new List<DinhLuongImportViewModel>();
            _displayData = new ObservableCollection<DinhLuongImportViewModel>();
            DgDinhLuong.ItemsSource = _displayData;
            
            this.Loaded += NhapDinhLuongTuExcelWindow_Loaded;
        }

        private async void NhapDinhLuongTuExcelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var nhomList = await _matHangService.GetNhomMatHangTreeAsync();
                TvNhomMatHang.ItemsSource = nhomList;
                
                FilterData(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhóm mặt hàng: " + ex.Message);
            }
        }

        private void FilterData(string nhomId)
        {
            _displayData.Clear();
            
            List<DinhLuongImportViewModel> filtered;
            if (string.IsNullOrEmpty(nhomId))
            {
                filtered = _allData;
            }
            else
            {
                // Find all descendant IDs for the selected nhomId
                var descendantIds = new HashSet<string> { nhomId };
                var allGroups = TvNhomMatHang.ItemsSource as IEnumerable<NhomMatHangViewModel>;
                if (allGroups != null)
                {
                    var selectedGroup = FindGroupById(allGroups, nhomId);
                    if (selectedGroup != null)
                    {
                        AddDescendantIds(selectedGroup, descendantIds);
                    }
                }

                filtered = _allData.Where(x => descendantIds.Contains(x.NhomMatHangId)).ToList();
            }

            foreach (var item in filtered)
            {
                _displayData.Add(item);
            }
        }

        private NhomMatHangViewModel FindGroupById(IEnumerable<NhomMatHangViewModel> groups, string id)
        {
            foreach (var g in groups)
            {
                if (g.Id == id) return g;
                var child = FindGroupById(g.Children, id);
                if (child != null) return child;
            }
            return null;
        }

        private void AddDescendantIds(NhomMatHangViewModel group, HashSet<string> ids)
        {
            foreach (var child in group.Children)
            {
                ids.Add(child.Id);
                AddDescendantIds(child, ids);
            }
        }

        private void TvNhomMatHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomMatHangViewModel selectedNhom)
            {
                string filterId = string.IsNullOrEmpty(selectedNhom.Id) ? null : selectedNhom.Id;
                FilterData(filterId);
            }
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            var validItems = _allData.Where(x => !string.IsNullOrEmpty(x.DmathangId) && !string.IsNullOrEmpty(x.DvattuId)).ToList();
            var invalidItems = _allData.Count - validItems.Count;

            if (invalidItems > 0)
            {
                var msgResult = MessageBox.Show($"Có {invalidItems} dòng định lượng bị sai (Mặt hàng hoặc Nguyên liệu không tồn tại). Hệ thống sẽ bỏ qua các dòng này và chỉ import {validItems.Count} dòng hợp lệ. Bạn có muốn tiếp tục?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgResult != MessageBoxResult.Yes) return;
            }

            if (validItems.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu hợp lệ nào để import!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int successCount = 0;
            foreach (var item in validItems)
            {
                var dl = new DDINHLUONG
                {
                    DmathangId = item.DmathangId,
                    DvattuId = item.DvattuId,
                    Soluong = item.SoLuong,
                    Status = true,
                    Timecreated = DateTime.Now
                };

                bool res = await _matHangService.InsertOrUpdateDinhLuongAsync(dl);
                if (res) successCount++;
            }

            MessageBox.Show($"Cập nhật định lượng thành công {successCount}/{validItems.Count} dòng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
