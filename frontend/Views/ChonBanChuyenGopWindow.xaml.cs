using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public class BanChuyenGopItem
    {
        public int STT { get; set; }
        public PosBanViewModel Ban { get; set; }
        public string Id => Ban?.Id;
        public string Name => Ban?.Name;
        public string KhuVucName => Ban?.KhuVucName;
        public bool IsOccupied => Ban?.IsOccupied ?? false;
        public string TrangThaiText => IsOccupied ? "Đang mở" : "Trống";
        public Brush TrangThaiColor => IsOccupied ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c0392b")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27ae60"));
    }

    public partial class ChonBanChuyenGopWindow : Window
    {
        private PosBanViewModel _sourceBan;
        private List<PosBanViewModel> _allBans;
        private bool _isMergeMode;

        public PosBanViewModel SelectedTargetBan { get; private set; }

        public ChonBanChuyenGopWindow(PosBanViewModel sourceBan, List<PosBanViewModel> allBans, bool isMergeMode = false)
        {
            InitializeComponent();
            _sourceBan = sourceBan;
            _allBans = allBans ?? new List<PosBanViewModel>();
            _isMergeMode = isMergeMode;

            Title = _isMergeMode ? "Gộp bàn" : "Chuyển bàn";
            TxtTitleInfo.Text = _isMergeMode ? "Gộp từ bàn: " : "Chuyển từ bàn: ";
            TxtCurrentBanName.Text = _sourceBan?.Name ?? "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBanList();
            TxtTimKiem.Focus();
        }

        private void LoadBanList()
        {
            string search = TxtTimKiem.Text?.Trim().ToLower() ?? "";
            
            // Loại trừ bàn hiện tại
            var query = _allBans.Where(b => b.Id != _sourceBan?.Id);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => (b.Name ?? "").ToLower().Contains(search) || (b.KhuVucName ?? "").ToLower().Contains(search));
            }

            var list = new List<BanChuyenGopItem>();
            int idx = 1;
            foreach (var b in query)
            {
                list.Add(new BanChuyenGopItem
                {
                    STT = idx++,
                    Ban = b
                });
            }

            DgBanDich.ItemsSource = list;
            if (list.Count > 0)
            {
                DgBanDich.SelectedIndex = 0;
            }
        }

        private void TxtTimKiem_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LoadBanList();
        }

        private void DgBanDich_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (DgBanDich.SelectedItem is BanChuyenGopItem item && item.Ban != null)
            {
                if (!_isMergeMode && item.Ban.IsOccupied)
                {
                    MessageBox.Show($"Bàn '{item.Ban.Name}' hiện đang có khách mở.\nChuyển bàn chỉ thực hiện sang bàn TRỐNG!\nNếu bạn muốn dồn món, vui lòng sử dụng chức năng 'Gộp bàn'.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string actionText = _isMergeMode ? "gộp" : "chuyển";
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn {actionText} từ bàn '{_sourceBan.Name}' sang bàn '{item.Ban.Name}' không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }

                SelectedTargetBan = item.Ban;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một bàn đích từ danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
