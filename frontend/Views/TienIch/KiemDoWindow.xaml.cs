using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class KiemDoWindow : Window
    {
        private readonly string _donHangId;
        private readonly LocalHoaDonService _hoaDonService;
        private ObservableCollection<KiemDoItemViewModel> _items;

        public KiemDoWindow(string donHangId, IEnumerable<ChiTietHoaDonViewModel> chiTietList)
        {
            InitializeComponent();
            _donHangId = donHangId;
            _hoaDonService = new LocalHoaDonService();

            _items = new ObservableCollection<KiemDoItemViewModel>();
            if (chiTietList != null)
            {
                int stt = 1;
                foreach (var ct in chiTietList)
                {
                    _items.Add(new KiemDoItemViewModel
                    {
                        Id = ct.Id,
                        MatHangId = ct.MatHangId,
                        Stt = stt++,
                        MatHang = ct.TenMon,
                        DonGia = ct.DonGia,
                        ChietKhauPt = ct.PhanTramGiamGia,
                        SlGoi = ct.SoLuong,
                        SlTra = 0
                    });
                }
            }

            DgKiemDo.ItemsSource = _items;
        }

        private async void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            var traItems = _items.Where(i => i.SlTra > 0).ToList();
            if (traItems.Count == 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hàng trả lại ở cột 'SL trả'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Xác nhận trả lại {traItems.Count} mặt hàng cho hóa đơn này?", "Xác nhận trả đồ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                bool success = await _hoaDonService.TraDoHoaDonAsync(_donHangId, _items.ToList());
                if (success)
                {
                    MessageBox.Show("Cập nhật trả hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật trả hàng, vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thực hiện trả đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
