using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemKhuVucWindow : Window
    {
        private readonly LocalBanKhuVucService _service;
        private string _khuVucIdToEdit;
        private bool _isThuMuc;
        private string _parentId;
        private List<KhuVucViewModel> _khuVucList;
        private int _currentIndex = -1;
        private Action _onDataSaved;

        public string TenKhuVuc => TxtTenKhuVuc.Text.Trim();
        public string GhiChu => TxtGhiChu.Text.Trim();

        public ThemKhuVucWindow(bool isThuMuc = false, string khuVucIdToEdit = null, List<KhuVucViewModel> khuVucList = null, int initialIndex = -1, Action onDataSaved = null, string initialName = null, string parentId = null)
        {
            InitializeComponent();
            _service = new LocalBanKhuVucService();
            _isThuMuc = isThuMuc;
            _khuVucIdToEdit = khuVucIdToEdit;
            _parentId = parentId;
            _khuVucList = khuVucList;
            _currentIndex = initialIndex;
            _onDataSaved = onDataSaved;

            if (!string.IsNullOrEmpty(_khuVucIdToEdit))
            {
                this.Title = _isThuMuc ? "THƯ MỤC KHU VỰC - SỬA" : "KHU VỰC - SỬA";
                TxtHeaderTitle.Text = _isThuMuc ? "Thư mục khu vực" : "Khu vực";
                TxtHeaderIcon.Text = _isThuMuc ? "📁" : "📝";

                if (!string.IsNullOrEmpty(initialName))
                {
                    TxtTenKhuVuc.Text = initialName;
                }
                else if (_khuVucList != null && _currentIndex >= 0 && _currentIndex < _khuVucList.Count)
                {
                    TxtTenKhuVuc.Text = _khuVucList[_currentIndex].Name ?? "";
                }

                LoadKhuVucDataAsync(_khuVucIdToEdit);
            }
            else
            {
                this.Title = _isThuMuc ? "THƯ MỤC KHU VỰC - THÊM MỚI" : "KHU VỰC - THÊM MỚI";
                TxtHeaderTitle.Text = _isThuMuc ? "Thư mục khu vực" : "Khu vực";
                TxtHeaderIcon.Text = _isThuMuc ? "📁" : "📝";
                if (!string.IsNullOrEmpty(initialName))
                {
                    TxtTenKhuVuc.Text = initialName;
                }
                TxtTenKhuVuc.Focus();
            }

            UpdateNavigationButtons();
        }

        private async void LoadKhuVucDataAsync(string khuVucId)
        {
            try
            {
                var data = await _service.GetKhuVucByIdAsync(khuVucId);
                if (data != null)
                {
                    var dict = data as IDictionary<string, object>;
                    string name = null;
                    string note = null;
                    object simageObj = null;

                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            string key = kvp.Key?.ToUpperInvariant();
                            if (key == "NAME") name = kvp.Value?.ToString();
                            else if (key == "NOTE") note = kvp.Value?.ToString();
                            else if (key == "SIMAGEID" || key == "BIEUTUONGID") simageObj = kvp.Value;
                        }
                    }
                    else
                    {
                        try { name = data.NAME?.ToString(); } catch { }
                        try { note = data.NOTE?.ToString(); } catch { }
                        try { simageObj = data.SIMAGEID; } catch { }
                    }

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        TxtTenKhuVuc.Text = name;
                    }
                    if (note != null)
                    {
                        TxtGhiChu.Text = note;
                    }

                    if (simageObj != null && int.TryParse(simageObj.ToString(), out int sImgId))
                    {
                        if (sImgId >= 0 && sImgId < CboAnh.Items.Count)
                            CboAnh.SelectedIndex = sImgId;
                    }
                }

                TxtTenKhuVuc.Focus();
                TxtTenKhuVuc.SelectAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadKhuVucDataAsync: " + ex.Message);
            }
        }

        private void TxtTenKhuVuc_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtTenKhuVuc.SelectAll();
        }

        private void UpdateNavigationButtons()
        {
            if (_khuVucList != null && _khuVucList.Count > 1 && _currentIndex >= 0)
            {
                BtnTruoc.IsEnabled = _currentIndex > 0;
                BtnSau.IsEnabled = _currentIndex < _khuVucList.Count - 1;
            }
            else
            {
                BtnTruoc.IsEnabled = false;
                BtnSau.IsEnabled = false;
            }
        }

        private void BtnTruoc_Click(object sender, RoutedEventArgs e)
        {
            if (_khuVucList != null && _currentIndex > 0)
            {
                _currentIndex--;
                var target = _khuVucList[_currentIndex];
                _khuVucIdToEdit = target.Id;
                TxtTenKhuVuc.Text = target.Name ?? "";
                LoadKhuVucDataAsync(target.Id);
                UpdateNavigationButtons();
            }
        }

        private void BtnSau_Click(object sender, RoutedEventArgs e)
        {
            if (_khuVucList != null && _currentIndex < _khuVucList.Count - 1)
            {
                _currentIndex++;
                var target = _khuVucList[_currentIndex];
                _khuVucIdToEdit = target.Id;
                TxtTenKhuVuc.Text = target.Name ?? "";
                LoadKhuVucDataAsync(target.Id);
                UpdateNavigationButtons();
            }
        }

        private void MenuTaoMoi_Click(object sender, RoutedEventArgs e)
        {
            _khuVucIdToEdit = null;
            this.Title = _isThuMuc ? "THƯ MỤC KHU VỰC - THÊM MỚI" : "KHU VỰC - THÊM MỚI";
            TxtTenKhuVuc.Text = "";
            TxtGhiChu.Text = "";
            TxtTenKhuVuc.Focus();
            UpdateNavigationButtons();
        }

        private void MenuSaoChep_Click(object sender, RoutedEventArgs e)
        {
            _khuVucIdToEdit = null;
            this.Title = _isThuMuc ? "THƯ MỤC KHU VỰC - THÊM MỚI" : "KHU VỰC - THÊM MỚI";
            TxtTenKhuVuc.Text = $"{TxtTenKhuVuc.Text.Trim()} (Bản sao)";
            TxtTenKhuVuc.Focus();
            TxtTenKhuVuc.SelectAll();
        }

        private async Task<bool> SaveDataAsync()
        {
            if (string.IsNullOrWhiteSpace(TenKhuVuc))
            {
                MessageBox.Show("Vui lòng nhập tên khu vực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenKhuVuc.Focus();
                return false;
            }

            int selectedIconIndex = CboAnh.SelectedIndex >= 0 ? CboAnh.SelectedIndex : 0;

            bool success;
            if (!string.IsNullOrEmpty(_khuVucIdToEdit))
            {
                success = await _service.UpdateKhuVucFullAsync(_khuVucIdToEdit, TenKhuVuc, GhiChu, selectedIconIndex);
            }
            else
            {
                success = await _service.InsertKhuVucAsync(TenKhuVuc, _parentId);
            }

            if (success)
            {
                _onDataSaved?.Invoke();
            }

            return success;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            await SaveDataAsync();
        }

        private async void BtnLuuVaMoi_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveDataAsync();
            if (success)
            {
                MenuTaoMoi_Click(sender, e);
            }
        }

        private async void BtnLuuVaThoat_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveDataAsync();
            if (success)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BtnThoat_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F10)
            {
                BtnTruoc_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnSau_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                MenuTaoMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                MenuSaoChep_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                await SaveDataAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnLuuVaMoi_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.FocusedElement is not TextBox)
            {
                BtnLuuVaThoat_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
