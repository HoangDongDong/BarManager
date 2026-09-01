using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public class MatHangKhuyenMaiItem
    {
        public int Stt { get; set; }
        public string SttStr => Stt < 10 ? $"00{Stt}" : (Stt < 100 ? $"0{Stt}" : Stt.ToString());
        public string Id { get; set; } = string.Empty;
        public string Mahang { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Dvt { get; set; } = string.Empty;
        public decimal Giaban { get; set; }
        public string NhomHangId { get; set; } = string.Empty;
    }

    public partial class ChonMatHangKhuyenMaiWindow : Window
    {
        private string _initialNhomHangId;
        private string _currentNhomHangId = "ALL";
        private List<MatHangKhuyenMaiItem> _allList = new();
        private ObservableCollection<NhomKhachHangTreeItem> _nhomHangTree = new();

        public List<MatHangKhuyenMaiItem> SelectedItems { get; private set; } = new();

        public ChonMatHangKhuyenMaiWindow(string nhomHangId = null)
        {
            InitializeComponent();
            _initialNhomHangId = nhomHangId;
            if (!string.IsNullOrEmpty(nhomHangId)) _currentNhomHangId = nhomHangId;

            Loaded += ChonMatHangKhuyenMaiWindow_Loaded;
        }

        private async void ChonMatHangKhuyenMaiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNhomHangTreeAsync();
            await LoadDataAsync();
            TxtTimKiem.Focus();
        }

        private async Task LoadNhomHangTreeAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                    string sql = "SELECT ID, NAME, PARENTID, PARENTDIR FROM DNHOMMATHANG WHERE (STATUS = 30 OR STATUS > 0 OR STATUS IS NULL) ORDER BY SORTORDER, NAME";
                    var groups = (await conn.QueryAsync(sql)).ToList();

                    _nhomHangTree = new ObservableCollection<NhomKhachHangTreeItem>();
                    var root = new NhomKhachHangTreeItem
                    {
                        Id = "ALL",
                        Name = "Tất cả",
                        Icon = "🌐",
                        IsExpanded = true
                    };

                    var lookup = new Dictionary<string, NhomKhachHangTreeItem>();
                    var rawItems = new List<dynamic>();

                    foreach (var g in groups)
                    {
                        string id = g.ID?.ToString()?.Trim() ?? "";
                        string name = g.NAME?.ToString()?.Trim() ?? "";
                        string parentId = g.PARENTID?.ToString()?.Trim();
                        var item = new NhomKhachHangTreeItem
                        {
                            Id = id,
                            Name = name,
                            ParentId = parentId,
                            Icon = "📁",
                            IsExpanded = true
                        };
                        lookup[id] = item;
                        rawItems.Add(g);
                    }

                    foreach (var g in rawItems)
                    {
                        string id = g.ID?.ToString()?.Trim() ?? "";
                        string parentId = g.PARENTID?.ToString()?.Trim();
                        var item = lookup[id];

                        if (!string.IsNullOrEmpty(parentId) && lookup.ContainsKey(parentId))
                        {
                            lookup[parentId].Children.Add(item);
                        }
                        else
                        {
                            root.Children.Add(item);
                        }
                    }

                    _nhomHangTree.Add(root);
                    TvNhomHang.ItemsSource = _nhomHangTree;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadNhomHangTreeAsync in ChonMatHang: " + ex.Message);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                    string sql = @"
                        SELECT m.ID, 
                               COALESCE(m.CODE, m.MASANCO, '') as Mahang, 
                               m.NAME, 
                               m.GIABAN, 
                               m.DNHOMMATHANGID as NhomHangId, 
                               d.NAME as Dvt
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH d ON m.DDONVITINHID = d.ID
                        WHERE (m.STATUS = 30 OR m.STATUS > 0 OR m.STATUS IS NULL)
                        ORDER BY m.NAME";

                    _allList = (await conn.QueryAsync<MatHangKhuyenMaiItem>(sql)).ToList();
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách mặt hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            var filtered = _allList.AsEnumerable();

            if (!string.IsNullOrEmpty(_currentNhomHangId) && _currentNhomHangId != "ALL")
            {
                filtered = filtered.Where(x => x.NhomHangId == _currentNhomHangId);
            }

            string kw = TxtTimKiem.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(kw))
            {
                filtered = filtered.Where(x => 
                    (x.Name != null && x.Name.ToLower().Contains(kw)) ||
                    (x.Mahang != null && x.Mahang.ToLower().Contains(kw))
                );
            }

            var list = filtered.ToList();
            int stt = 1;
            foreach (var item in list)
            {
                item.Stt = stt++;
            }

            DgMatHang.ItemsSource = list;
        }

        private void TvNhomHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem selected)
            {
                _currentNhomHangId = selected.Id;
                ApplyFilter();
            }
        }

        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnChon_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgMatHang.SelectedItems.Cast<MatHangKhuyenMaiItem>().ToList();
            if (selected.Count == 0 && DgMatHang.SelectedItem is MatHangKhuyenMaiItem single)
            {
                selected.Add(single);
            }

            if (selected.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 mặt hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedItems = selected;
            this.DialogResult = true;
            this.Close();
        }

        private void DgMatHang_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChon_Click(sender, e);
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == Key.Enter)
            {
                BtnChon_Click(sender, e);
            }
        }
    }
}
