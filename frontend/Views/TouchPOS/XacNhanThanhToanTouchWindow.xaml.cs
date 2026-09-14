using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;
using QuanLyBar.Views.TouchPOS;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class XacNhanThanhToanTouchWindow : Window
    {
        private PosBanViewModel _ban;
        private List<TouchCartItemVM> _items;

        public decimal TongCong { get; private set; }
        public decimal KhachDua { get; private set; }
        public decimal TheATM { get; private set; }
        public decimal ChuyenKhoan { get; private set; }
        public decimal TheTraTruoc { get; private set; }
        public decimal TraLai { get; private set; }
        public bool IsInBill { get; private set; } = true;
        public string SelectedTaiKhoanNganHangId { get; private set; }
        public string SelectedTaiKhoanNganHangName { get; private set; }

        private string _activeField = "KhachDua";
        private bool _isUserTyping = false;

        public XacNhanThanhToanTouchWindow(PosBanViewModel ban, List<TouchCartItemVM> items)
        {
            InitializeComponent();
            _ban = ban;
            _items = items ?? new List<TouchCartItemVM>();

            if (_ban != null)
            {
                TongCong = _ban.TongCong;
                KhachDua = _ban.TongCong;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTongCong.Text = TongCong.ToString("N0");
            TxtKhachDua.Text = KhachDua.ToString("N0");
            TxtTheATM.Text = "0";
            TxtChuyenKhoan.Text = "0";
            TxtTheTraTruoc.Text = "0";
            TxtTraLai.Text = "0";

            try
            {
                var configs = await LocalCauHinhService.LoadAllConfigsAsync();

                // 1. In tạm tính
                bool choPhepInTamTinh = !configs.TryGetValue("ChoPhepInTamTinh", out var cp) || cp == "1" || cp.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (BtnInTamTinh != null) BtnInTamTinh.Visibility = choPhepInTamTinh ? Visibility.Visible : Visibility.Collapsed;

                // 2. Bắt buộc in khi thanh toán (ràng buộc cấu hình hệ thống)
                bool batBuocIn = configs.TryGetValue("BatBuocInKhiThanhToan", out var bbi) && (bbi == "1" || bbi.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (batBuocIn && BtnDongBillKhongIn != null)
                {
                    BtnDongBillKhongIn.Visibility = Visibility.Collapsed;
                }

                // 3. Phương thức thanh toán
                bool coThe = !configs.TryGetValue("CoThanhToanThe", out var ctt) || ctt == "1" || ctt.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (RowTheATM != null) RowTheATM.Visibility = coThe ? Visibility.Visible : Visibility.Collapsed;

                bool coCK = !configs.TryGetValue("CoThanhToanChuyenKhoan", out var cck) || cck == "1" || cck.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (RowChuyenKhoan != null) RowChuyenKhoan.Visibility = coCK ? Visibility.Visible : Visibility.Collapsed;
                if (RowTaiKhoan != null)
                {
                    RowTaiKhoan.Visibility = coCK ? Visibility.Visible : Visibility.Collapsed;
                    await LoadTaiKhoanNganHangAsync();
                }

                bool suDungTheTraTruoc = !configs.TryGetValue("SuDungTheTraTruoc", out var sdtt) || sdtt == "1" || sdtt.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (RowTheTraTruoc != null) RowTheTraTruoc.Visibility = suDungTheTraTruoc ? Visibility.Visible : Visibility.Collapsed;
            }
            catch { }

            SelectField("KhachDua");
        }

        private void SelectField(string fieldName)
        {
            _activeField = fieldName;
            _isUserTyping = false;

            var activeBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F39C12"));
            var transparentBrush = System.Windows.Media.Brushes.Transparent;

            BorderKhachDua.BorderBrush = (_activeField == "KhachDua") ? activeBrush : transparentBrush;
            BorderTheATM.BorderBrush = (_activeField == "TheATM") ? activeBrush : transparentBrush;
            BorderChuyenKhoan.BorderBrush = (_activeField == "ChuyenKhoan") ? activeBrush : transparentBrush;
            BorderTaiKhoan.BorderBrush = (_activeField == "TaiKhoan") ? activeBrush : transparentBrush;
            BorderTheTraTruoc.BorderBrush = (_activeField == "TheTraTruoc") ? activeBrush : transparentBrush;

            TextBox activeTb = GetActiveTextBox();
            if (activeTb != null)
            {
                activeTb.Focus();
                activeTb.SelectAll();
            }
        }

        private TextBox GetActiveTextBox()
        {
            return _activeField switch
            {
                "KhachDua" => TxtKhachDua,
                "TheATM" => TxtTheATM,
                "ChuyenKhoan" => TxtChuyenKhoan,
                "TheTraTruoc" => TxtTheTraTruoc,
                _ => TxtKhachDua
            };
        }

        private void Field_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag != null)
            {
                SelectField(el.Tag.ToString() ?? "KhachDua");
            }
        }

        private void BtnDenom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && decimal.TryParse(btn.Tag.ToString(), out decimal denomVal))
            {
                TextBox activeTb = GetActiveTextBox();
                if (activeTb != null)
                {
                    decimal curVal = ParseDecimal(activeTb.Text);
                    if (!_isUserTyping || curVal == 0)
                    {
                        activeTb.Text = denomVal.ToString("N0");
                    }
                    else
                    {
                        activeTb.Text = (curVal + denomVal).ToString("N0");
                    }
                    _isUserTyping = true;
                }
            }
        }

        private void NumpadKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string digit = btn.Tag.ToString() ?? "";
                TextBox activeTb = GetActiveTextBox();
                if (activeTb == null) return;

                string currentStr = activeTb.Text.Replace(",", "").Replace(".", "").Trim();

                if (!_isUserTyping || currentStr == "0")
                {
                    currentStr = (digit == "000") ? "0" : digit;
                    _isUserTyping = true;
                }
                else
                {
                    currentStr += digit;
                }

                if (decimal.TryParse(currentStr, out decimal val))
                {
                    activeTb.Text = val.ToString("N0");
                }
            }
        }

        private void BtnBackspace_Click(object sender, RoutedEventArgs e)
        {
            TextBox activeTb = GetActiveTextBox();
            if (activeTb == null) return;

            string currentStr = activeTb.Text.Replace(",", "").Replace(".", "").Trim();
            if (currentStr.Length > 1)
            {
                currentStr = currentStr.Substring(0, currentStr.Length - 1);
            }
            else
            {
                currentStr = "0";
                _isUserTyping = false;
            }

            if (decimal.TryParse(currentStr, out decimal val))
            {
                activeTb.Text = val.ToString("N0");
            }
            else
            {
                activeTb.Text = "0";
            }
        }

        private void InputMoney_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            KhachDua = ParseDecimal(TxtKhachDua?.Text);
            TheATM = ParseDecimal(TxtTheATM?.Text);
            ChuyenKhoan = ParseDecimal(TxtChuyenKhoan?.Text);
            TheTraTruoc = ParseDecimal(TxtTheTraTruoc?.Text);

            decimal totalPaid = KhachDua + TheATM + ChuyenKhoan + TheTraTruoc;
            decimal change = totalPaid - TongCong;
            if (change < 0) change = 0;

            TraLai = change;
            if (TxtTraLai != null)
            {
                TxtTraLai.Text = change.ToString("N0");
            }
        }

        private decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            string clean = input.Replace(",", "").Replace(".", "").Trim();
            return decimal.TryParse(clean, out decimal res) ? res : 0;
        }

        private void BtnInTamTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_ban == null) return;
            try
            {
                var printWin = new HoaDonBanHangPrintWindow(_ban, isTamTinh: true);
                printWin.Owner = this;
                printWin.ShowDialog();
            }
            catch (Exception ex)
            {
                TouchConfirmWindow.ShowAlert(this, $"LỖI IN TẠM TÍNH: {ex.Message}", "LỖI");
            }
        }

        private static object GetValue(IDictionary<string, object> d, string name)
        {
            if (d == null) return null;
            foreach (var kv in d)
            {
                if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
            return null;
        }

        private async System.Threading.Tasks.Task LoadTaiKhoanNganHangAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                    string sql = "SELECT ID, NAME, NOTE FROM DTAIKHOANNGANHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY COALESCE(SORTORDER, 0), ID";
                    var rows = (await conn.QueryAsync(sql)).ToList();
                    var list = new List<TaiKhoanNganHangComboItem>();
                    list.Add(new TaiKhoanNganHangComboItem { Id = "", Name = "", DisplayName = "-- Chọn tài khoản --" });

                    foreach (var r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string name = GetValue(dict, "NAME")?.ToString() ?? "";
                        string note = GetValue(dict, "NOTE")?.ToString() ?? "";

                        string displayName = name;
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            displayName = !string.IsNullOrWhiteSpace(note) ? note : "Tài khoản " + id;
                        }
                        else if (!string.IsNullOrWhiteSpace(note) && !displayName.Contains(note, StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = $"{displayName} ({note})";
                        }

                        list.Add(new TaiKhoanNganHangComboItem { Id = id, Name = displayName, DisplayName = displayName });
                    }

                    if (CboTaiKhoan != null)
                    {
                        CboTaiKhoan.ItemsSource = list;
                        CboTaiKhoan.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadTaiKhoanNganHangAsync Touch: " + ex.Message);
            }
        }

        private void CboTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string selectedId = CboTaiKhoan?.SelectedValue?.ToString();
            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                if (ChuyenKhoan == 0 && KhachDua > 0)
                {
                    ChuyenKhoan = KhachDua;
                    KhachDua = 0;
                    TxtChuyenKhoan.Text = ChuyenKhoan.ToString("N0");
                    TxtKhachDua.Text = "0";
                }
            }
            else
            {
                if (KhachDua == 0 && ChuyenKhoan > 0)
                {
                    KhachDua = ChuyenKhoan;
                    ChuyenKhoan = 0;
                    TxtKhachDua.Text = KhachDua.ToString("N0");
                    TxtChuyenKhoan.Text = "0";
                }
            }
        }

        private void BtnDongBillVaIn_Click(object sender, RoutedEventArgs e)
        {
            string selId = CboTaiKhoan?.SelectedValue?.ToString();
            SelectedTaiKhoanNganHangId = string.IsNullOrWhiteSpace(selId) ? null : selId;
            SelectedTaiKhoanNganHangName = string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId) ? null : (CboTaiKhoan?.SelectedItem as TaiKhoanNganHangComboItem)?.Name;
            
            if (!string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId))
            {
                if (ChuyenKhoan == 0 && KhachDua > 0)
                {
                    ChuyenKhoan = KhachDua;
                    KhachDua = 0;
                }
            }

            IsInBill = true;
            DialogResult = true;
            Close();
        }

        private void BtnDongBillKhongIn_Click(object sender, RoutedEventArgs e)
        {
            string selId = CboTaiKhoan?.SelectedValue?.ToString();
            SelectedTaiKhoanNganHangId = string.IsNullOrWhiteSpace(selId) ? null : selId;
            SelectedTaiKhoanNganHangName = string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId) ? null : (CboTaiKhoan?.SelectedItem as TaiKhoanNganHangComboItem)?.Name;
            
            if (!string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId))
            {
                if (ChuyenKhoan == 0 && KhachDua > 0)
                {
                    ChuyenKhoan = KhachDua;
                    KhachDua = 0;
                }
            }

            IsInBill = false;
            DialogResult = true;
            Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
