using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
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

        public string KhuVucId { get; set; }

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
            if (int.TryParse(TuSo?.Trim(), out int t) && int.TryParse(DenSo?.Trim(), out int d))
            {
                if (t <= d)
                {
                    int len = GetChieuDaiCallback?.Invoke() ?? 0;
                    string prefix = BatDauBang ?? "";
                    string mauTu = prefix + (len > 0 ? t.ToString().PadLeft(len, '0') : t.ToString());
                    string mauDen = prefix + (len > 0 ? d.ToString().PadLeft(len, '0') : d.ToString());
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
            DgThemNhanh.ItemsSource = _rows;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKhuVucDataAsync();
        }

        private async Task LoadKhuVucDataAsync()
        {
            try
            {
                _rows.Clear();
                var khuVucs = await _service.GetLookupAsync("DKHUVUC");

                if (khuVucs == null || khuVucs.Count == 0)
                {
                    // Fallback to direct query if lookup was empty
                    using (var conn = DbConnectionManager.GetConnection())
                    {
                        await conn.OpenAsync();
                        var items = await conn.QueryAsync<dynamic>("SELECT ID, NAME FROM DKHUVUC WHERE (STATUS <> 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME");
                        khuVucs = items.Select(x => new LookupItem
                        {
                            Id = ((object)x.ID)?.ToString()?.Trim(),
                            Name = ((object)x.NAME)?.ToString()?.Trim()
                        }).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
                    }
                }

                foreach (var kv in khuVucs)
                {
                    // Lọc những cái không phải thùng rác hoặc nhóm gốc ảo
                    if (!string.IsNullOrEmpty(kv.Id) && kv.Id != "-1" && kv.Name != "Thùng rác" && kv.Name != "Tất cả")
                    {
                        var row = new ThemNhanhBanKhuVucRowViewModel
                        {
                            KhuVucId = kv.Id,
                            KhuVucName = kv.Name,
                            GetChieuDaiCallback = GetChieuDaiVungSo
                        };
                        _rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải khu vực: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetChieuDaiVungSo()
        {
            if (int.TryParse(TxtChieuDai.Text?.Trim(), out int len) && len >= 0)
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
                var listToInsert = new List<DBAN>();

                foreach (var row in _rows)
                {
                    if (int.TryParse(row.TuSo?.Trim(), out int tu) && int.TryParse(row.DenSo?.Trim(), out int den))
                    {
                        if (tu <= den)
                        {
                            string prefix = row.BatDauBang ?? "";
                            for (int i = tu; i <= den; i++)
                            {
                                string numPart = len > 0 ? i.ToString().PadLeft(len, '0') : i.ToString();
                                string name = prefix + numPart;
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
                    MessageBox.Show("Vui lòng nhập 'Bàn từ số' và 'Bàn đến số' hợp lệ cho ít nhất 1 khu vực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Hệ thống sẽ thêm {listToInsert.Count} bàn vào hệ thống.\nBạn có chắc chắn muốn thực hiện không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Batch insert with transaction
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    
                    int currentMaxId = 0;
                    try
                    {
                        var maxVal = await conn.QueryFirstOrDefaultAsync<int?>("SELECT MAX(CAST(ID AS INTEGER)) FROM DBAN WHERE ID SIMILAR TO '[0-9]+'");
                        currentMaxId = maxVal ?? 0;
                    }
                    catch
                    {
                        // Fallback in case SIMILAR TO is not supported in some DB dialect
                        try
                        {
                            var allIds = await conn.QueryAsync<string>("SELECT ID FROM DBAN");
                            currentMaxId = allIds.Select(id => int.TryParse(id, out int parsed) ? parsed : 0).DefaultIfEmpty(0).Max();
                        }
                        catch { }
                    }

                    using (var trans = conn.BeginTransaction())
                    {
                        string sql = @"
                            INSERT INTO DBAN (ID, NAME, NOTE, DKHUVUCID, DNHOMHIENTHIID, DLOAIPHONGID, STATUS, USERCREATEDID, TIMECREATED) 
                            VALUES (@Id, @Name, @Note, @DkhuvucId, @DnhomhienthiId, @DloaiphongId, 1, 1, CURRENT_TIMESTAMP)";

                        foreach (var ban in listToInsert)
                        {
                            currentMaxId++;
                            ban.Id = currentMaxId.ToString();
                            await conn.ExecuteAsync(sql, ban, transaction: trans);
                        }

                        trans.Commit();
                    }
                }

                MessageBox.Show($"Thêm thành công {listToInsert.Count} bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thực hiện: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

