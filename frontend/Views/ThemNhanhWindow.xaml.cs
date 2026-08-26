using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemNhanhWindow : Window
    {
        private readonly LocalMatHangService _matHangService;
        private ObservableCollection<MatHangViewModel> _quickAddList;
        private Action _onDataSaved;

        public ThemNhanhWindow(Action onDataSaved = null)
        {
            InitializeComponent();
            _matHangService = new LocalMatHangService();
            _onDataSaved = onDataSaved;
            
            _quickAddList = new ObservableCollection<MatHangViewModel>();
            DgThemNhanh.ItemsSource = _quickAddList;
            
            this.Loaded += ThemNhanhWindow_Loaded;
        }

        private async void ThemNhanhWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var nhomList = await _matHangService.GetNhomMatHangTreeAsync();
                var flatNhomList = nhomList.SelectMany(x => x.Children.Count > 0 ? x.Children : new ObservableCollection<NhomMatHangViewModel> { x }).ToList();
                CboNhomMatHang.ItemsSource = flatNhomList;

                var dvtList = await _matHangService.GetDonViTinhListAsync();
                CboDonViTinh.ItemsSource = dvtList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu danh mục: " + ex.Message);
            }
        }

        private void BtnThemDuLieu_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtSoDong.Text, out int soDong) && soDong > 0)
            {
                for (int i = 0; i < soDong; i++)
                {
                    _quickAddList.Add(new MatHangViewModel { Name = "", Code = "" });
                }
            }
        }

        private void BtnXoaDuLieu_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DgThemNhanh.SelectedItems.Cast<MatHangViewModel>().ToList();
            foreach (var item in selectedItems)
            {
                _quickAddList.Remove(item);
            }
        }

        private async void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            if (_quickAddList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để thêm!", "Thông báo");
                return;
            }

            int successCount = 0;
            string dnhommathangId = CboNhomMatHang.SelectedValue?.ToString();
            string ddonvitinhId = CboDonViTinh.SelectedValue?.ToString();
            decimal giaBan = decimal.TryParse(TxtGiaBan.Text, out var gb) ? gb : 0;
            decimal giaNhap = decimal.TryParse(TxtGiaNhap.Text, out var gn) ? gn : 0;

            foreach (var item in _quickAddList)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue; // Skip empty rows

                var matHang = new MatHangViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = string.IsNullOrWhiteSpace(item.Code) ? "" : item.Code,
                    Name = item.Name,
                    Giaban = giaBan,
                    Gianhap = giaNhap,
                    Quydoi = "1",
                    Giatheothoigia = ChkGiaTheoThoiGiaVal.IsChecked == true ? 1 : 0,
                    DnhommathangId = dnhommathangId,
                    DdonvitinhId = ddonvitinhId,
                    Tamkhoa = ChkTamKhoaVal.IsChecked == true ? "1" : "0"
                };

                bool result = await _matHangService.InsertMatHangAsync(matHang);
                if (result) successCount++;
            }

            MessageBox.Show($"Thêm thành công {successCount}/{_quickAddList.Count} mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            
            if (successCount > 0)
            {
                _onDataSaved?.Invoke();
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnChonFileExcel_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChonFileExcelWindow();
            if (win.ShowDialog() == true && win.SelectedFilePaths != null)
            {
                string files = string.Join(", ", win.SelectedFilePaths);
                MessageBox.Show($"Bạn đã chọn file: {files}\nChức năng import sẽ được thực hiện sau theo hướng dẫn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ChkColumn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (DgThemNhanh == null) return;
            
            var chk = sender as System.Windows.Controls.CheckBox;
            if (chk == null || string.IsNullOrEmpty(chk.Name)) return;

            string header = chk.Name switch
            {
                "ChkNhomMatHang" => "Nhóm mặt hàng",
                "ChkLoaiMatHang" => "Loại mặt hàng",
                "ChkDonViTinh" => "Đơn vị tính",
                "ChkGiaBan" => "Giá bán",
                "ChkGiaNhap" => "Giá nhập",
                "ChkDonViTinhChan" => "Đơn vị tính chẵn",
                "ChkQuyDoi" => "Quy đổi",
                "ChkGiaBanChan" => "Giá bán chẵn",
                "ChkTmKhoa" => "Tạm khóa",
                "ChkGiaTheoThoi" => "Giá theo thời giá",
                "ChkGhiChu" => "Ghi chú",
                "ChkTToiThieu" => "Tồn tối thiểu",
                "ChkTToiDa" => "Tồn tối đa",
                "ChkAnh" => "Ảnh",
                "ChkHoaHong" => "Hoa hồng",
                "ChkGiaVon" => "Giá vốn",
                _ => null
            };

            if (header == null) return;

            // Find existing column
            var existingCol = DgThemNhanh.Columns.FirstOrDefault(c => c.Header?.ToString() == header);

            if (chk.IsChecked == true)
            {
                // Remove column if it exists
                if (existingCol != null)
                {
                    DgThemNhanh.Columns.Remove(existingCol);
                }
            }
            else
            {
                // Add column if it doesn't exist
                if (existingCol == null)
                {
                    DgThemNhanh.Columns.Add(new System.Windows.Controls.DataGridTextColumn 
                    { 
                        Header = header, 
                        Binding = new System.Windows.Data.Binding(chk.Name.Replace("Chk", "")) // Basic binding attempt, actual model needs these properties
                    });
                }
            }
        }
    }
}
