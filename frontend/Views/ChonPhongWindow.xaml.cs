using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ChonPhongWindow : Window
    {
        private ObservableCollection<BanViewModel> _allBans;
        private ObservableCollection<BanViewModel> _filteredBans;
        
        public List<BanViewModel> SelectedBans { get; private set; } = new List<BanViewModel>();

        public ChonPhongWindow()
        {
            InitializeComponent();
            _allBans = new ObservableCollection<BanViewModel>();
            _filteredBans = new ObservableCollection<BanViewModel>();
            DgBan.ItemsSource = _filteredBans;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var service = new LocalTheoDoiDatPhongService();
            // Assuming we can get all tables from TheoDoiDatPhongService or similar
            // Since we couldn't find a direct GetBanList in LocalBanKhuVucService that returns all tables
            // Let's use LocalTheoDoiDatPhongService or fetch manually.
            // Wait, I will use Dapper here just to be quick and safe since I don't have the exact service method.
            
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT b.ID as Id, 
                               b.NAME as Name, 
                               b.DKHUVUCID as KhuVucId,
                               k.NAME as KhuVucName,
                               b.DLOAIPHONGID as LoaiPhongId,
                               lp.NAME as LoaiPhongName
                        FROM DBAN b
                        LEFT JOIN DKHUVUC k ON b.DKHUVUCID = k.ID
                        LEFT JOIN DLOAIPHONG lp ON b.DLOAIPHONGID = lp.ID
                        WHERE b.STATUS = 1
                        ORDER BY b.NAME";
                    
                    var result = await Dapper.SqlMapper.QueryAsync<dynamic>(conn, sql);
                    _allBans.Clear();
                    int stt = 1;
                    foreach (var row in result)
                    {
                        var dict = (System.Collections.Generic.IDictionary<string, object>)row;
                        string GetVal(string key) 
                        {
                            var matchedKey = dict.Keys.FirstOrDefault(k => string.Equals(k, key, System.StringComparison.OrdinalIgnoreCase));
                            return matchedKey != null && dict[matchedKey] != null ? dict[matchedKey].ToString() : "";
                        }
                        
                        _allBans.Add(new BanViewModel
                        {
                            Stt = stt++,
                            Id = GetVal("Id"),
                            Name = GetVal("Name"),
                            KhuVucName = GetVal("KhuVucName"),
                            LoaiPhongName = GetVal("LoaiPhongName")
                        });
                    }
                }
                FilterData("");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phòng/bàn: " + ex.Message);
            }
        }

        private void FilterData(string filter)
        {
            _filteredBans.Clear();
            var query = _allBans.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(filter));
            }

            foreach (var item in query)
            {
                _filteredBans.Add(item);
            }
        }

        private void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData(TxtLoc.Text);
        }

        private void DgBan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = DgBan.SelectedItem as BanViewModel;
            if (selected != null)
            {
                SelectedBans.Add(selected);
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedBans = _allBans.Where(x => x.IsSelected).ToList();
            
            if (SelectedBans.Count == 0)
            {
                var selected = DgBan.SelectedItem as BanViewModel;
                if (selected != null)
                {
                    SelectedBans.Add(selected);
                }
            }

            if (SelectedBans.Count > 0)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn ít nhất một phòng/bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
