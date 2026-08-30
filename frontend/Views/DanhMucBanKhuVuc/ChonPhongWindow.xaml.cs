using System.Collections.Generic;
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
        private HashSet<string> _preselectedIds;
        
        public List<BanViewModel> SelectedBans { get; private set; } = new List<BanViewModel>();

        public ChonPhongWindow(IEnumerable<string> preselectedIds = null)
        {
            InitializeComponent();
            _preselectedIds = preselectedIds != null ? new HashSet<string>(preselectedIds) : new HashSet<string>();
            _allBans = new ObservableCollection<BanViewModel>();
            _filteredBans = new ObservableCollection<BanViewModel>();
            DgBan.ItemsSource = _filteredBans;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
            TxtLoc.Focus();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var service = new LocalBanKhuVucService();
                var bans = await service.GetBanListAsync("");
                
                _allBans.Clear();
                int stt = 1;
                foreach (var b in bans)
                {
                    b.Stt = stt++;
                    if (_preselectedIds != null && _preselectedIds.Contains(b.Id))
                    {
                        b.IsSelected = true;
                    }
                    _allBans.Add(b);
                }
                FilterData("");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phòng/bàn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterData(string filter)
        {
            _filteredBans.Clear();
            var query = _allBans.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query = query.Where(x => 
                    (x.Name != null && x.Name.ToLower().Contains(filter)) ||
                    (x.KhuVucName != null && x.KhuVucName.ToLower().Contains(filter)) ||
                    (x.LoaiPhongName != null && x.LoaiPhongName.ToLower().Contains(filter))
                );
            }

            int stt = 1;
            foreach (var item in query)
            {
                item.Stt = stt++;
                _filteredBans.Add(item);
            }
        }

        private void TxtLoc_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData(TxtLoc.Text);
        }

        private void DgBan_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgBan.SelectedItem is BanViewModel selected)
            {
                selected.IsSelected = !selected.IsSelected;
            }
        }

        private void DgBan_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (DgBan.SelectedItem is BanViewModel selected)
                {
                    selected.IsSelected = !selected.IsSelected;
                    e.Handled = true;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
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
                    selected.IsSelected = true;
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
