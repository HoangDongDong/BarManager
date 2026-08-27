using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class ThemNhanhBanKhuVucRowViewModel : INotifyPropertyChanged
    {
        private string _khuVucName;
        public string KhuVucName
        {
            get => _khuVucName;
            set { _khuVucName = value; OnPropertyChanged(nameof(KhuVucName)); }
        }

        public int KhuVucId { get; set; }

        private string _tuSo;
        public string TuSo
        {
            get => _tuSo;
            set { _tuSo = value; UpdateDuLieuMau(); OnPropertyChanged(nameof(TuSo)); }
        }

        private string _denSo;
        public string DenSo
        {
            get => _denSo;
            set { _denSo = value; UpdateDuLieuMau(); OnPropertyChanged(nameof(DenSo)); }
        }

        private string _batDauBang;
        public string BatDauBang
        {
            get => _batDauBang;
            set { _batDauBang = value; UpdateDuLieuMau(); OnPropertyChanged(nameof(BatDauBang)); }
        }

        private string _duLieuMau;
        public string DuLieuMau
        {
            get => _duLieuMau;
            set { _duLieuMau = value; OnPropertyChanged(nameof(DuLieuMau)); }
        }

        public Func<int> GetChieuDaiCallback { get; set; }

        public void UpdateDuLieuMau()
        {
            if (int.TryParse(TuSo, out int t) && int.TryParse(DenSo, out int d))
            {
                if (t <= d)
                {
                    int len = GetChieuDaiCallback?.Invoke() ?? 0;
                    string prefix = BatDauBang ?? "";
                    string mauTu = prefix + t.ToString().PadLeft(len, '0');
                    string mauDen = prefix + d.ToString().PadLeft(len, '0');
                    if (t == d)
                    {
                        DuLieuMau = mauTu;
                    }
                    else
                    {
                        DuLieuMau = $"{mauTu} đến {mauDen}";
                    }
                }
                else
                {
                    DuLieuMau = "Lỗi: Từ số > Đến số";
                }
            }
            else
            {
                DuLieuMau = "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class ThemNhanhBanKhuVucWindow : Window
    {
        private ObservableCollection<ThemNhanhBanKhuVucRowViewModel> _rows;
        private LocalBanKhuVucService _service;

        public ThemNhanhBanKhuVucWindow()
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
            _rows = new ObservableCollection<ThemNhanhBanKhuVucRowViewModel>();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var khuVucs = await _service.GetLookupAsync("DKHUVUC"); 
                foreach (var kv in khuVucs)
                {
                    // Lọc những cái không phải thùng rác hoặc nhóm gốc ảo
                    if (kv.Id != null && int.TryParse(kv.Id.ToString(), out int id) && id > 0)
                    {
                        var row = new ThemNhanhBanKhuVucRowViewModel
                        {
                            KhuVucId = id,
                            KhuVucName = kv.Name,
                            GetChieuDaiCallback = GetChieuDaiVungSo
                        };
                        _rows.Add(row);
                    }
                }
                DgThemNhanh.ItemsSource = _rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải khu vực: " + ex.Message);
            }
        }

        private int GetChieuDaiVungSo()
        {
            if (int.TryParse(TxtChieuDai.Text, out int len))
            {
                return len;
            }
            return 0;
        }

        private void TxtChieuDai_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_rows != null)
            {
                foreach (var row in _rows)
                {
                    row.UpdateDuLieuMau();
                }
            }
        }

        private async void BtnThucHien_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int len = GetChieuDaiVungSo();
                List<DBAN> listToInsert = new List<DBAN>();

                foreach (var row in _rows)
                {
                    if (int.TryParse(row.TuSo, out int tu) && int.TryParse(row.DenSo, out int den))
                    {
                        if (tu <= den)
                        {
                            string prefix = row.BatDauBang ?? "";
                            for (int i = tu; i <= den; i++)
                            {
                                string name = prefix + i.ToString().PadLeft(len, '0');
                                var ban = new DBAN
                                {
                                    Name = name,
                                    DkhuvucId = row.KhuVucId,
                                    Status = true
                                };
                                listToInsert.Add(ban);
                            }
                        }
                    }
                }

                if (listToInsert.Count == 0)
                {
                    MessageBox.Show("Vui lòng nhập dải số hợp lệ cho ít nhất 1 khu vực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Hệ thống sẽ thêm {listToInsert.Count} bàn vào hệ thống. Bạn có chắc chắn không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var b in listToInsert)
                    {
                        await _service.InsertBanAsync(b);
                    }
                    MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
