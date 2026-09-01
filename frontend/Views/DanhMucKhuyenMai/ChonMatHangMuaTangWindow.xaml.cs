using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Models;

namespace QuanLyBar.Client.Views
{
    public partial class ChonMatHangMuaTangWindow : Window
    {
        private ObservableCollection<NhomKhachHangTreeItem> _nhomHangTree = new ObservableCollection<NhomKhachHangTreeItem>();
        private List<MatHangKhuyenMaiItem> _allList = new List<MatHangKhuyenMaiItem>();
        private ObservableCollection<MatHangKhuyenMaiItem> _filteredListMua = new ObservableCollection<MatHangKhuyenMaiItem>();
        private ObservableCollection<MatHangKhuyenMaiItem> _filteredListTang = new ObservableCollection<MatHangKhuyenMaiItem>();

        private string _currentNhomHangId = "ALL";

        public MatHangKhuyenMaiItem SelectedItemMua { get; private set; }
        public MatHangKhuyenMaiItem SelectedItemTang { get; private set; }

        public ChonMatHangMuaTangWindow(string defaultNhomHangId = null)
        {
            InitializeComponent();
            _currentNhomHangId = string.IsNullOrEmpty(defaultNhomHangId) ? "ALL" : defaultNhomHangId;
            Loaded += ChonMatHangMuaTangWindow_Loaded;
        }

        private async void ChonMatHangMuaTangWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNhomHangTreeAsync();
            await LoadDataAsync();
            TxtTimMua.Focus();
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
                Console.WriteLine("Error LoadNhomHangTreeAsync in ChonMatHangMuaTang: " + ex.Message);
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
                    ApplyFilterMua();
                    ApplyFilterTang();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách mặt hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilterMua()
        {
            string kw = TxtTimMua.Text.Trim().ToLower();
            var filtered = _allList.Where(x =>
                (_currentNhomHangId == "ALL" || string.IsNullOrEmpty(_currentNhomHangId) || x.NhomHangId == _currentNhomHangId) &&
                (string.IsNullOrEmpty(kw) ||
                 (x.Name != null && x.Name.ToLower().Contains(kw)) ||
                 (x.Mahang != null && x.Mahang.ToLower().Contains(kw)))
            ).ToList();

            _filteredListMua.Clear();
            int stt = 1;
            foreach (var sp in filtered)
            {
                sp.Stt = stt++;
                _filteredListMua.Add(sp);
            }
            DgMatHangMua.ItemsSource = _filteredListMua;
            if (_filteredListMua.Count > 0 && DgMatHangMua.SelectedItem == null)
            {
                DgMatHangMua.SelectedIndex = 0;
            }
        }

        private void ApplyFilterTang()
        {
            string kw = TxtTimTang.Text.Trim().ToLower();
            var filtered = _allList.Where(x =>
                (_currentNhomHangId == "ALL" || string.IsNullOrEmpty(_currentNhomHangId) || x.NhomHangId == _currentNhomHangId) &&
                (string.IsNullOrEmpty(kw) ||
                 (x.Name != null && x.Name.ToLower().Contains(kw)) ||
                 (x.Mahang != null && x.Mahang.ToLower().Contains(kw)))
            ).ToList();

            _filteredListTang.Clear();
            int stt = 1;
            foreach (var sp in filtered)
            {
                sp.Stt = stt++;
                _filteredListTang.Add(sp);
            }
            DgMatHangTang.ItemsSource = _filteredListTang;
            if (_filteredListTang.Count > 0 && DgMatHangTang.SelectedItem == null)
            {
                DgMatHangTang.SelectedIndex = 0;
            }
        }

        private void TvNhomHang_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NhomKhachHangTreeItem selected)
            {
                _currentNhomHangId = selected.Id;
                ApplyFilterMua();
                ApplyFilterTang();
            }
        }

        private void TxtTimMua_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilterMua();
        }

        private void TxtTimTang_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilterTang();
        }

        private void DgMatHangMua_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChkTangCungMatHang.IsChecked == true && DgMatHangMua.SelectedItem is MatHangKhuyenMaiItem selectedMua)
            {
                var matchInTang = _filteredListTang.FirstOrDefault(x => x.Id == selectedMua.Id);
                if (matchInTang != null)
                {
                    DgMatHangTang.SelectedItem = matchInTang;
                    DgMatHangTang.ScrollIntoView(matchInTang);
                }
            }
        }

        private void ChkTangCungMatHang_Changed(object sender, RoutedEventArgs e)
        {
            if (ChkTangCungMatHang.IsChecked == true && DgMatHangMua.SelectedItem is MatHangKhuyenMaiItem selectedMua)
            {
                var matchInTang = _filteredListTang.FirstOrDefault(x => x.Id == selectedMua.Id);
                if (matchInTang != null)
                {
                    DgMatHangTang.SelectedItem = matchInTang;
                    DgMatHangTang.ScrollIntoView(matchInTang);
                }
            }
        }

        private void BtnChon_Click(object sender, RoutedEventArgs e)
        {
            if (DgMatHangMua.SelectedItem is MatHangKhuyenMaiItem mua)
            {
                SelectedItemMua = mua;
                if (ChkTangCungMatHang.IsChecked == true)
                {
                    SelectedItemTang = mua;
                }
                else if (DgMatHangTang.SelectedItem is MatHangKhuyenMaiItem tang)
                {
                    SelectedItemTang = tang;
                }
                else
                {
                    SelectedItemTang = mua;
                }

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một mặt hàng mua!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
