using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class ThongKeNhanhWindow : Window
    {
        public class ThongKeItem
        {
            public int Stt { get; set; }
            public string TenTieuChi { get; set; }
            public int Dem { get; set; }
            public decimal TongCong { get; set; }
            public double PhanTram { get; set; }
        }

        private List<DatHangViewModel> _dataList;
        private bool _isLoaded = false;

        public ThongKeNhanhWindow(IEnumerable<DatHangViewModel> dataList)
        {
            _dataList = dataList?.Where(d => d != null).ToList() ?? new List<DatHangViewModel>();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            ThongKeTheoTieuChi("Ngay", "Ngày");
        }

        private void TvTieuChi_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            ThucHienThongKe();
        }

        private void TvTieuChi_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isLoaded) return;
            ThucHienThongKe();
        }

        private void ThucHienThongKe()
        {
            if (TvTieuChi?.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag != null)
            {
                string tag = selectedItem.Tag.ToString();
                string header = "";
                if (selectedItem.Header is StackPanel sp)
                {
                    foreach (var child in sp.Children)
                    {
                        if (child is TextBlock tb && !string.IsNullOrWhiteSpace(tb.Text) && !tb.Text.Contains("🗓") && !tb.Text.Contains("🔘") && !tb.Text.Contains("🔍") && !tb.Text.Contains("🎯") && !tb.Text.Contains("📅") && !tb.Text.Contains("👥") && !tb.Text.Contains("🪑"))
                        {
                            header = tb.Text.Replace("Theo ", "").Trim();
                            break;
                        }
                    }
                }
                else if (selectedItem.Header is string str)
                {
                    header = str.Trim();
                }

                if (string.IsNullOrEmpty(header)) header = "Tiêu chí";
                ThongKeTheoTieuChi(tag, header);
            }
        }

        private void ThongKeTheoTieuChi(string tag, string columnHeader)
        {
            if (ColTieuChi == null || DgThongKe == null) return;
            ColTieuChi.Header = columnHeader ?? "Tiêu chí";

            if (_dataList == null || _dataList.Count == 0)
            {
                DgThongKe.ItemsSource = null;
                if (TxtTongDem != null) TxtTongDem.Text = "0";
                if (TxtTongTien != null) TxtTongTien.Text = "0";
                return;
            }

            Func<DatHangViewModel, string> keySelector = tag switch
            {
                "Ngay" => d => d.Ngay.HasValue ? d.Ngay.Value.ToString("d/M/yyyy") : "Chưa xác định",
                "Thang" => d => d.Ngay.HasValue ? d.Ngay.Value.ToString("M/yyyy") : "Chưa xác định",
                "Nam" => d => d.Ngay.HasValue ? d.Ngay.Value.ToString("yyyy") : "Chưa xác định",
                "KhachHang" => d => !string.IsNullOrEmpty(d.TenKhach) ? d.TenKhach : "Chưa có tên khách",
                "NhomKhachHang" => d => !string.IsNullOrEmpty(d.TenKhach) ? "Khách lẻ" : "Chưa phân nhóm",
                "NhanVien" => d => !string.IsNullOrEmpty(d.UsercreatedName) ? d.UsercreatedName : "Admin",
                "TinhThanh" => d => !string.IsNullOrEmpty(d.DiaChi) ? (d.DiaChi.Contains("Hà Nội") ? "Hà Nội" : d.DiaChi) : "Chưa có địa chỉ",
                "TheTraTruoc" => d => "Không",
                "PhuongThucDat" => d => !string.IsNullOrEmpty(d.PhuongThucDatName) ? d.PhuongThucDatName : "Chưa chọn",
                "MucDichDat" => d => !string.IsNullOrEmpty(d.MucDichDatName) ? d.MucDichDatName : "Chưa chọn",
                "Ban" => d => "Bàn chung",
                "KhuVuc" => d => "Khu vực chung",
                "BangGia" => d => "Bảng giá chuẩn",
                "NhomHienThi" => d => "Tất cả",
                "LoaiPhong" => d => "Thường",
                _ => d => "Khác"
            };

            decimal totalAllMoney = 0;
            foreach (var item in _dataList)
            {
                if (item != null && decimal.TryParse(item.TongCong?.Replace(",", "")?.Replace(".", ""), out decimal tc))
                {
                    totalAllMoney += tc;
                }
            }

            var groups = _dataList.Where(d => d != null)
                                  .GroupBy(keySelector)
                                  .Select(g =>
                                  {
                                      decimal sumMoney = 0;
                                      foreach (var item in g)
                                      {
                                          if (item != null && decimal.TryParse(item.TongCong?.Replace(",", "")?.Replace(".", ""), out decimal tc))
                                          {
                                              sumMoney += tc;
                                          }
                                      }
                                      double phanTram = totalAllMoney > 0 ? (double)(sumMoney / totalAllMoney * 100) : 0;
                                      return new
                                      {
                                          Key = g.Key,
                                          Count = g.Count(),
                                          SumMoney = sumMoney,
                                          PhanTram = Math.Round(phanTram, 0)
                                      };
                                  }).ToList();

            var resultList = new List<ThongKeItem>();
            int stt = 1;
            int totalDem = 0;
            decimal totalTien = 0;

            foreach (var grp in groups)
            {
                resultList.Add(new ThongKeItem
                {
                    Stt = stt++,
                    TenTieuChi = grp.Key,
                    Dem = grp.Count,
                    TongCong = grp.SumMoney,
                    PhanTram = grp.PhanTram
                });
                totalDem += grp.Count;
                totalTien += grp.SumMoney;
            }

            DgThongKe.ItemsSource = resultList;
            if (resultList.Count > 0)
            {
                DgThongKe.SelectedIndex = 0;
            }

            if (TxtTongDem != null) TxtTongDem.Text = totalDem.ToString("N0");
            if (TxtTongTien != null) TxtTongTien.Text = totalTien.ToString("N0");
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
