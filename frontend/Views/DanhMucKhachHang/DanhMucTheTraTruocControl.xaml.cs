using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class DanhMucTheTraTruocControl : UserControl
    {
        private ObservableCollection<TheTraTruocViewModel> _cardList = new ObservableCollection<TheTraTruocViewModel>();
        private ObservableCollection<TheTraTruocHoaDonItem> _hoaDonList = new ObservableCollection<TheTraTruocHoaDonItem>();
        private ObservableCollection<TheTraTruocNhapKhoItem> _nhapKhoList = new ObservableCollection<TheTraTruocNhapKhoItem>();
        private ObservableCollection<TheTraTruocXuatKhoItem> _xuatKhoList = new ObservableCollection<TheTraTruocXuatKhoItem>();
        private ObservableCollection<TheTraTruocChuyenKhoItem> _chuyenKhoList = new ObservableCollection<TheTraTruocChuyenKhoItem>();
        private ObservableCollection<TheTraTruocKiemKeItem> _kiemKeList = new ObservableCollection<TheTraTruocKiemKeItem>();
        private ObservableCollection<TheTraTruocKhachHangItem> _khachHangList = new ObservableCollection<TheTraTruocKhachHangItem>();
        private ObservableCollection<TheTraTruocThuChiItem> _thuList = new ObservableCollection<TheTraTruocThuChiItem>();
        private ObservableCollection<TheTraTruocThuChiItem> _chiList = new ObservableCollection<TheTraTruocThuChiItem>();
        private ObservableCollection<TheTraTruocThuCongNoItem> _thuCongNoList = new ObservableCollection<TheTraTruocThuCongNoItem>();

        private NhomTheTraTruocTreeItem _selectedNhom;
        private string _currentNhomId = "ALL";
        private TheTraTruocViewModel _selectedCard;
        private string _clickedCellValue;

        public DanhMucTheTraTruocControl()
        {
            InitializeComponent();
            DgTheTraTruoc.ItemsSource = _cardList;
            DgHoaDonThe.ItemsSource = _hoaDonList;
            DgNhapKhoThe.ItemsSource = _nhapKhoList;
            DgXuatKhoThe.ItemsSource = _xuatKhoList;
            DgChuyenKhoThe.ItemsSource = _chuyenKhoList;
            DgKiemKeThe.ItemsSource = _kiemKeList;
            DgKhachHangThe.ItemsSource = _khachHangList;
            DgThuThe.ItemsSource = _thuList;
            DgChiThe.ItemsSource = _chiList;
            DgThuCongNoThe.ItemsSource = _thuCongNoList;

            EnsurePlaceholderRows();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomAsync();
            await LoadCardsAsync();
        }

        private async Task LoadTreeNhomAsync()
        {
            try
            {
                var tree = await LocalTheTraTruocService.GetNhomTheTraTruocTreeAsync();
                TvNhomTheTraTruoc.ItemsSource = tree;

                if (tree != null && tree.Count > 0)
                {
                    var allNode = tree[0];
                    allNode.IsExpanded = true;
                    allNode.IsSelected = true;
                    _selectedNhom = allNode;
                    _currentNhomId = allNode.Id;

                    foreach (var c in allNode.Children)
                    {
                        c.IsExpanded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTreeNhomAsync: " + ex.Message);
            }
        }

        private async Task LoadCardsAsync()
        {
            try
            {
                string kw = TxtLoc.Text?.Trim() ?? "";
                var data = await LocalTheTraTruocService.GetTheTraTruocListAsync(_currentNhomId, kw);

                _cardList.Clear();
                foreach (var item in data)
                {
                    _cardList.Add(item);
                }

                TxtTongSummary.Text = $"Tổng cộng: {_cardList.Count:N0} thẻ trả trước";

                if (_cardList.Count > 0)
                {
                    if (_selectedCard != null)
                    {
                        var found = _cardList.FirstOrDefault(x => x.Id == _selectedCard.Id);
                        DgTheTraTruoc.SelectedItem = found ?? _cardList[0];
                    }
                    else
                    {
                        DgTheTraTruoc.SelectedIndex = 0;
                    }

                    _selectedCard = DgTheTraTruoc.SelectedItem as TheTraTruocViewModel;
                    await LoadCardDetailsAsync();
                }
                else
                {
                    _selectedCard = null;
                    ClearCardDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp danh sách thẻ trả trước: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgTheTraTruoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCard = DgTheTraTruoc.SelectedItem as TheTraTruocViewModel;
            await LoadCardDetailsAsync();
        }

        private async Task LoadCardDetailsAsync()
        {
            if (_selectedCard == null)
            {
                ClearCardDetails();
                return;
            }

            // Tab Thông tin
            TxtKhoiTao.Text = _selectedCard.TimeCreated.HasValue 
                ? $"Khởi tạo: {_selectedCard.TimeCreated:dd/MM/yyyy HH:mm tt}" 
                : "Khởi tạo:";
            TxtKhoiTaoBoi.Text = !string.IsNullOrEmpty(_selectedCard.UserCreatedName) 
                ? $"Khởi tạo bởi: {_selectedCard.UserCreatedName}" 
                : "Khởi tạo bởi:";

            TxtSuaDoi.Text = _selectedCard.TimeModified.HasValue 
                ? $"Sửa đổi gần nhất: {_selectedCard.TimeModified:dd/MM/yyyy HH:mm tt}" 
                : "Sửa đổi gần nhất:";
            TxtSuaDoiBoi.Text = !string.IsNullOrEmpty(_selectedCard.UserModifiedName) 
                ? $"Sửa đổi bởi: {_selectedCard.UserModifiedName}" 
                : "Sửa đổi bởi:";

            // Tab Hóa đơn nhà hàng
            _hoaDonList.Clear();
            try
            {
                var invoices = await LocalTheTraTruocService.GetLichSuHoaDonTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var inv in invoices)
                {
                    _hoaDonList.Add(inv);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading invoices: " + ex.Message);
            }

            // Tab Phiếu nhập kho
            _nhapKhoList.Clear();
            try
            {
                var nhapKhoData = await LocalTheTraTruocService.GetLichSuNhapKhoTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var nk in nhapKhoData)
                {
                    _nhapKhoList.Add(nk);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading nhap kho: " + ex.Message);
            }

            // Tab Phiếu xuất kho
            _xuatKhoList.Clear();
            try
            {
                var xuatKhoData = await LocalTheTraTruocService.GetLichSuXuatKhoTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var xk in xuatKhoData)
                {
                    _xuatKhoList.Add(xk);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading xuat kho: " + ex.Message);
            }

            // Tab Phiếu chuyển kho
            _chuyenKhoList.Clear();
            try
            {
                var chuyenKhoData = await LocalTheTraTruocService.GetLichSuChuyenKhoTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var ck in chuyenKhoData)
                {
                    _chuyenKhoList.Add(ck);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading chuyen kho: " + ex.Message);
            }

            // Tab Phiếu kiểm kê
            _kiemKeList.Clear();
            try
            {
                var kiemKeData = await LocalTheTraTruocService.GetLichSuKiemKeTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var kk in kiemKeData)
                {
                    _kiemKeList.Add(kk);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading kiem ke: " + ex.Message);
            }

            // Tab Khách hàng
            _khachHangList.Clear();
            try
            {
                var khData = await LocalTheTraTruocService.GetKhachHangTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var kh in khData)
                {
                    _khachHangList.Add(kh);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading khach hang: " + ex.Message);
            }

            // Tab Phiếu thu
            _thuList.Clear();
            try
            {
                var thuData = await LocalTheTraTruocService.GetLichSuThuChiTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe, 0);
                foreach (var pt in thuData)
                {
                    _thuList.Add(pt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading phieu thu: " + ex.Message);
            }

            // Tab Phiếu chi
            _chiList.Clear();
            try
            {
                var chiData = await LocalTheTraTruocService.GetLichSuThuChiTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe, 1);
                foreach (var pc in chiData)
                {
                    _chiList.Add(pc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading phieu chi: " + ex.Message);
            }

            // Tab Phiếu thu công nợ
            _thuCongNoList.Clear();
            try
            {
                var congNoData = await LocalTheTraTruocService.GetLichSuThuCongNoTheTraTruocAsync(_selectedCard.Id, _selectedCard.MaThe);
                foreach (var cn in congNoData)
                {
                    _thuCongNoList.Add(cn);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading phieu thu cong no: " + ex.Message);
            }

            EnsurePlaceholderRows();
        }

        private void ClearCardDetails()
        {
            TxtKhoiTao.Text = "Khởi tạo:";
            TxtKhoiTaoBoi.Text = "Khởi tạo bởi:";
            TxtSuaDoi.Text = "Sửa đổi gần nhất:";
            TxtSuaDoiBoi.Text = "Sửa đổi bởi:";
            _hoaDonList.Clear();
            _nhapKhoList.Clear();
            _xuatKhoList.Clear();
            _chuyenKhoList.Clear();
            _kiemKeList.Clear();
            _khachHangList.Clear();
            _thuList.Clear();
            _chiList.Clear();
            _thuCongNoList.Clear();
            EnsurePlaceholderRows();
        }

        private void EnsurePlaceholderRows()
        {
            if (_hoaDonList.Count == 0) _hoaDonList.Add(new TheTraTruocHoaDonItem());
            if (_nhapKhoList.Count == 0) _nhapKhoList.Add(new TheTraTruocNhapKhoItem());
            if (_xuatKhoList.Count == 0) _xuatKhoList.Add(new TheTraTruocXuatKhoItem());
            if (_chuyenKhoList.Count == 0) _chuyenKhoList.Add(new TheTraTruocChuyenKhoItem());
            if (_kiemKeList.Count == 0) _kiemKeList.Add(new TheTraTruocKiemKeItem());
            if (_khachHangList.Count == 0) _khachHangList.Add(new TheTraTruocKhachHangItem());
            if (_thuList.Count == 0) _thuList.Add(new TheTraTruocThuChiItem());
            if (_chiList.Count == 0) _chiList.Add(new TheTraTruocThuChiItem());
            if (_thuCongNoList.Count == 0) _thuCongNoList.Add(new TheTraTruocThuCongNoItem());
        }

        private async void TvNhomTheTraTruoc_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomTheTraTruocTreeItem node)
            {
                _selectedNhom = node;
                _currentNhomId = node.Id;
                await LoadCardsAsync();
            }
        }

        private async void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadCardsAsync();
        }

        private async void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemTheTraTruocWindow(null, _currentNhomId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadCardsAsync();
            };
            win.ShowDialog();
            await LoadCardsAsync();
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            OpenEditCard();
        }

        private void DgTheTraTruoc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenEditCard();
        }

        private async void OpenEditCard()
        {
            if (_selectedCard == null)
            {
                MessageBox.Show("Vui lòng chọn một thẻ trả trước cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new ThemTheTraTruocWindow(_selectedCard);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadCardsAsync();
            };
            win.ShowDialog();
            await LoadCardsAsync();
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCard == null)
            {
                MessageBox.Show("Vui lòng chọn thẻ cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isTrash = (_currentNhomId == "TRASH");
            string msg = isTrash 
                ? $"Bạn có chắc muốn XÓA VĨNH VIỄN thẻ '{_selectedCard.MaThe}' không?" 
                : $"Bạn có chắc muốn chuyển thẻ '{_selectedCard.MaThe}' vào thùng rác không?";

            var dr = MessageBox.Show(msg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dr == MessageBoxResult.Yes)
            {
                bool ok = await LocalTheTraTruocService.DeleteTheTraTruocAsync(_selectedCard.Id, isTrash);
                if (ok)
                {
                    await LoadCardsAsync();
                    MessageBox.Show("Đã xóa thẻ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa thẻ. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnThemNhom_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomTheTraTruocWindow(null, "", _currentNhomId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
            };
            win.ShowDialog();
        }

        private void BtnSuaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom == null || _selectedNhom.Id == "ALL" || _selectedNhom.Id == "UNSET" || _selectedNhom.Id == "TRASH")
            {
                MessageBox.Show("Chỉ có thể sửa nhóm cụ thể do người dùng tạo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new ThemNhomTheTraTruocWindow(_selectedNhom.Id, _selectedNhom.Name, _selectedNhom.ParentId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
            };
            win.ShowDialog();
        }

        private async void BtnXoaNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom == null || _selectedNhom.Id == "ALL" || _selectedNhom.Id == "UNSET" || _selectedNhom.Id == "TRASH")
            {
                MessageBox.Show("Không thể xóa nhóm mặc định này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dr = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhóm '{_selectedNhom.Name}' không?", "Xác nhận xóa nhóm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dr == MessageBoxResult.Yes)
            {
                bool ok = await LocalTheTraTruocService.DeleteNhomTheTraTruocAsync(_selectedNhom.Id);
                if (ok)
                {
                    await LoadTreeNhomAsync();
                    MessageBox.Show("Đã xóa nhóm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa nhóm. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnRefreshNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomAsync();
            await LoadCardsAsync();
        }

        private async void BtnThemThuMuc_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Tạo thư mục mới", "Nhập tên thư mục:", "Thư mục mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "Thư mục mới";
                string newId = Guid.NewGuid().ToString();
                await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(newId, folderName, true, null);
                await LoadTreeNhomAsync();
            }
        }

        private void BtnXemTheoThuMuc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang hiển thị toàn bộ dữ liệu theo thư mục phân nhóm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnTaiLaiNhom_Click(object sender, RoutedEventArgs e)
        {
            await LoadTreeNhomAsync();
            await LoadCardsAsync();
        }

        private void BtnMenuCheDo_Click(object sender, RoutedEventArgs e)
        {
            if (BtnMenuCheDo.ContextMenu != null)
            {
                BtnMenuCheDo.ContextMenu.PlacementTarget = BtnMenuCheDo;
                BtnMenuCheDo.ContextMenu.IsOpen = true;
            }
        }

        private void MenuItem_CheDoNhom_Click(object sender, RoutedEventArgs e)
        {
            // Chế độ nhóm thẻ trả trước
        }

        private void MiThemMoiItem_Click(object sender, RoutedEventArgs e)
        {
            var win = new ThemNhomTheTraTruocWindow(null, "", null);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
                await LoadCardsAsync();
            };
            win.ShowDialog();
        }

        private async void MiThemNhanhGoc_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Thêm nhanh nhóm thẻ", "Nhập tên nhóm thẻ:", "Nhóm thẻ mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string name = inputWin.InputText?.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(Guid.NewGuid().ToString(), name, true, null);
                    await LoadTreeNhomAsync();
                }
            }
        }

        private async void MiThemPhanCach_Click(object sender, RoutedEventArgs e)
        {
            await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(Guid.NewGuid().ToString(), "-------------", true, _selectedNhom?.Id);
            await LoadTreeNhomAsync();
        }

        private async void MiThemThuMucGoc_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Tạo thư mục mới", "Nhập tên thư mục:", "Thư mục mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "Thư mục mới";
                await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(Guid.NewGuid().ToString(), folderName, true, null);
                await LoadTreeNhomAsync();
            }
        }

        private void MiThemConItem_Click(object sender, RoutedEventArgs e)
        {
            string parentId = _selectedNhom?.Id;
            var win = new ThemNhomTheTraTruocWindow(null, "", parentId);
            win.Owner = Window.GetWindow(this);
            win.OnSaved += async () =>
            {
                await LoadTreeNhomAsync();
                await LoadCardsAsync();
            };
            win.ShowDialog();
        }

        private async void MiThemNhanhCon_Click(object sender, RoutedEventArgs e)
        {
            var inputWin = new InputWindow("Thêm nhanh nhóm con", "Nhập tên nhóm con:", "Nhóm con mới");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string name = inputWin.InputText?.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(Guid.NewGuid().ToString(), name, true, _selectedNhom?.Id);
                    await LoadTreeNhomAsync();
                }
            }
        }

        private async void MiThemThuMucCon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom == null) return;
            var inputWin = new InputWindow("Tạo thư mục con", "Nhập tên thư mục con:", "Thư mục con");
            inputWin.Owner = Window.GetWindow(this);
            if (inputWin.ShowDialog() == true)
            {
                string folderName = inputWin.InputText?.Trim() ?? "Thư mục con";
                await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(Guid.NewGuid().ToString(), folderName, true, _selectedNhom.Id);
                await LoadTreeNhomAsync();
            }
        }

        private void MiChinhSua_Click(object sender, RoutedEventArgs e)
        {
            BtnSuaNhom_Click(sender, e);
        }

        private void MiSapXepTen_Click(object sender, RoutedEventArgs e)
        {
            MiSapXepTen.IsChecked = true;
            MiSapXepTuyChon.IsChecked = false;
            SortTreeNodes(byName: true);
        }

        private void MiSapXepTuyChon_Click(object sender, RoutedEventArgs e)
        {
            MiSapXepTen.IsChecked = false;
            MiSapXepTuyChon.IsChecked = true;
            SortTreeNodes(byName: false);
        }

        private void SortTreeNodes(bool byName)
        {
            if (TvNhomTheTraTruoc.ItemsSource is ObservableCollection<NhomTheTraTruocTreeItem> list && list.Count > 0)
            {
                var root = list[0];
                if (byName)
                {
                    var sorted = root.Children.OrderBy(x => x.Name).ToList();
                    root.Children.Clear();
                    foreach (var s in sorted) root.Children.Add(s);
                }
                else
                {
                    var sorted = root.Children.OrderBy(x => x.Id).ToList();
                    root.Children.Clear();
                    foreach (var s in sorted) root.Children.Add(s);
                }
            }
        }

        private void MiSaoChepNhom_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                Clipboard.SetText(_selectedNhom.Name ?? "");
                MessageBox.Show($"Đã sao chép tên nhóm '{_selectedNhom.Name}' vào bộ nhớ tạm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MiDoiTen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null && _selectedNhom.Id != "ALL" && _selectedNhom.Id != "UNSET" && _selectedNhom.Id != "TRASH")
            {
                var inputWin = new InputWindow("Đổi tên nhóm", "Nhập tên nhóm mới:", _selectedNhom.Name);
                inputWin.Owner = Window.GetWindow(this);
                if (inputWin.ShowDialog() == true)
                {
                    string newName = inputWin.InputText?.Trim();
                    if (!string.IsNullOrEmpty(newName))
                    {
                        await LocalTheTraTruocService.SaveNhomTheTraTruocFolderAsync(_selectedNhom.Id, newName, false);
                        _selectedNhom.Name = newName;
                    }
                }
            }
            else
            {
                MessageBox.Show("Chỉ có thể đổi tên nhóm hoặc thư mục cụ thể!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MiThungRac_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomTheTraTruoc.ItemsSource is ObservableCollection<NhomTheTraTruocTreeItem> list && list.Count > 0)
            {
                var trash = list[0].Children.FirstOrDefault(x => x.Id == "TRASH");
                if (trash != null)
                {
                    trash.IsExpanded = true;
                    _selectedNhom = trash;
                    _currentNhomId = trash.Id;
                    LoadCardsAsync();
                }
            }
        }

        private void MiBieuTuong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đổi biểu tượng đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiThuocTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhom != null)
            {
                MessageBox.Show($"Tên: {_selectedNhom.Name}\nID: {_selectedNhom.Id}\nLoại: Nhóm thẻ trả trước", "Thuộc tính", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnThemExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm thẻ trả trước từ Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_cardList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu thẻ trả trước để xuất Excel!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"TheTraTruoc_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("TheTraTruoc");

                        worksheet.Cell(1, 1).Value = "STT";
                        worksheet.Cell(1, 2).Value = "Mã thẻ";
                        worksheet.Cell(1, 3).Value = "Nhóm thẻ trả trước";
                        worksheet.Cell(1, 4).Value = "Khóa";
                        worksheet.Cell(1, 5).Value = "Ngày hết hạn";
                        worksheet.Cell(1, 6).Value = "Ghi chú";

                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#dfe9f5");

                        int row = 2;
                        foreach (var item in _cardList)
                        {
                            worksheet.Cell(row, 1).Value = item.Stt;
                            worksheet.Cell(row, 2).Value = item.MaThe;
                            worksheet.Cell(row, 3).Value = item.TenNhomTheTraTruoc;
                            worksheet.Cell(row, 4).Value = item.Khoa ? "Đã khóa" : "Mở";
                            worksheet.Cell(row, 5).Value = item.NgayHetHan.HasValue ? item.NgayHetHan.Value.ToString("dd/MM/yyyy") : "";
                            worksheet.Cell(row, 6).Value = item.GhiChu;
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            var win = new InLuoiWindow(DgTheTraTruoc, "Danh mục thẻ trả trước");
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void BtnTong_Click(object sender, RoutedEventArgs e)
        {
            BdrTong.Visibility = (BdrTong.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void DgTheTraTruoc_Row_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    DgTheTraTruoc.SelectedItems.Clear();
                    row.IsSelected = true;
                }
                row.Focus();

                var hit = VisualTreeHelper.HitTest(row, e.GetPosition(row));
                if (hit != null)
                {
                    DependencyObject dep = hit.VisualHit;
                    while (dep != null && !(dep is DataGridCell))
                    {
                        dep = VisualTreeHelper.GetParent(dep);
                    }
                    if (dep is DataGridCell cell)
                    {
                        _clickedCellValue = (cell.Content as TextBlock)?.Text ?? "";
                    }
                }
            }
        }

        private void TvNhomTheTraTruoc_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            while (obj != null && !(obj is TreeViewItem))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj is TreeViewItem tvi && tvi.DataContext is NhomTheTraTruocTreeItem item)
            {
                tvi.IsSelected = true;
                _selectedNhom = item;
                _currentNhomId = item.Id;
            }
        }

        private void MiSaoChepO_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_clickedCellValue))
            {
                Clipboard.SetText(_clickedCellValue);
            }
            else if (_selectedCard != null)
            {
                Clipboard.SetText(_selectedCard.MaThe ?? "");
            }
        }

        private void MiSaoChepVungChon_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = DgTheTraTruoc.SelectedItems.Cast<TheTraTruocViewModel>().ToList();
            if (selectedList.Count == 0 && _selectedCard != null) selectedList.Add(_selectedCard);
            if (selectedList.Count == 0) return;

            var sb = new StringBuilder();
            foreach (var item in selectedList)
            {
                sb.AppendLine($"{item.MaThe}\t{item.TenNhomTheTraTruoc}\t{(item.Khoa ? "Khóa" : "Mở")}\t{item.NgayHetHan:dd/MM/yyyy}\t{item.GhiChu}");
            }
            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private void MiTuDongGianCot_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in DgTheTraTruoc.Columns)
            {
                col.Width = DataGridLength.Auto;
                col.Width = DataGridLength.SizeToCells;
            }
        }

        private void MiCotHienThi_Click(object sender, RoutedEventArgs e)
        {
            var defaults = new List<string> { "Mã thẻ", "Nhóm thẻ trả trước", "Khóa", "Ngày hết hạn", "Ghi chú" };
            var win = new ChonCotHienThiWindow(DgTheTraTruoc, defaults);
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void MiMoRong_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomTheTraTruoc.ItemsSource is ObservableCollection<NhomTheTraTruocTreeItem> list)
            {
                void ExpandAll(IEnumerable<NhomTheTraTruocTreeItem> items)
                {
                    foreach (var it in items)
                    {
                        it.IsExpanded = true;
                        if (it.Children != null) ExpandAll(it.Children);
                    }
                }
                ExpandAll(list);
            }
        }

        private void MiThuGon_Click(object sender, RoutedEventArgs e)
        {
            if (TvNhomTheTraTruoc.ItemsSource is ObservableCollection<NhomTheTraTruocTreeItem> list)
            {
                void CollapseAll(IEnumerable<NhomTheTraTruocTreeItem> items)
                {
                    foreach (var it in items)
                    {
                        it.IsExpanded = false;
                        if (it.Children != null) CollapseAll(it.Children);
                    }
                }
                CollapseAll(list);
            }
        }

        private void MiCapNhatNhanhExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng cập nhật nhanh từ Excel đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MiSapXepMaThe_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _cardList.OrderBy(x => x.MaThe).ToList();
            _cardList.Clear();
            int stt = 1;
            foreach (var s in sorted) { s.Stt = stt++; _cardList.Add(s); }
        }

        private void MiSapXepNhomThe_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _cardList.OrderBy(x => x.TenNhomTheTraTruoc).ThenBy(x => x.MaThe).ToList();
            _cardList.Clear();
            int stt = 1;
            foreach (var s in sorted) { s.Stt = stt++; _cardList.Add(s); }
        }

        private void MiSapXepNgayHetHan_Click(object sender, RoutedEventArgs e)
        {
            var sorted = _cardList.OrderBy(x => x.NgayHetHan).ToList();
            _cardList.Clear();
            int stt = 1;
            foreach (var s in sorted) { s.Stt = stt++; _cardList.Add(s); }
        }

        private async void BtnRefreshThe_Click(object sender, RoutedEventArgs e)
        {
            await LoadCardsAsync();
        }

        private void MiThuocTinhThe_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCard != null)
            {
                MessageBox.Show($"Mã thẻ: {_selectedCard.MaThe}\nNhóm: {_selectedCard.TenNhomTheTraTruoc}\nNgày hết hạn: {_selectedCard.NgayHetHan:dd/MM/yyyy}\nGhi chú: {_selectedCard.GhiChu}", "Thuộc tính thẻ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
