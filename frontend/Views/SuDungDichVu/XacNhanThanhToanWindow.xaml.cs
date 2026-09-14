using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class XacNhanThanhToanWindow : Window
    {
        private PosBanViewModel _ban;
        public decimal TongTien { get; private set; }
        public decimal KhachDua { get; private set; }
        public decimal TheATM { get; private set; }
        public decimal ChuyenKhoan { get; private set; }
        public decimal TheTraTruoc { get; private set; }
        public decimal Voucher { get; private set; }
        public int DiemDoi { get; private set; }
        public decimal TienDiem { get; private set; }
        public decimal TamUng { get; private set; }
        public decimal TraLai { get; private set; }
        public bool IsKhachNo { get; private set; }
        public bool IsInBill { get; private set; } = true;
        public string MaTheTraTruoc { get; private set; }
        public string MaVoucher { get; private set; }
        public string SelectedTaiKhoanNganHangId { get; private set; }
        public string SelectedTaiKhoanNganHangName { get; private set; }
        private decimal _quyDoi1DiemSangTien = 1000;
        private decimal _lamTronTien = 1000;

        public XacNhanThanhToanWindow(PosBanViewModel ban)
        {
            InitializeComponent();
            _ban = ban;

            if (_ban != null)
            {
                TxtTenBanHeader.Text = _ban.Name;
                TongTien = _ban.TongCong;
                KhachDua = _ban.TongCong;
                TxtTongTien.Text = TongTien.ToString("N0");
                TxtKhachDua.Text = KhachDua.ToString("N0");
                TxtTheATM.Text = "0";
                TxtChuyenKhoan.Text = "0";
                TxtTheTraTruoc.Text = "0";
                TxtVoucher.Text = "0";
                TxtDiemTichLuy.Text = "0";
                TxtTienDiem.Text = "0";
                TxtTamUng.Text = "0";
                TxtTraLai.Text = "0";

                var tagParts = new List<string>();
                if (_ban.TienHang != _ban.TongCong || _ban.GiamGia > 0 || _ban.TienThue > 0 || _ban.TienPhiDichVu > 0)
                {
                    tagParts.Add($"Tiền hàng: {_ban.TienHang:N0}đ");
                    if (_ban.GiamGia > 0) tagParts.Add($"Giảm giá: -{_ban.GiamGia:N0}đ");
                    if (_ban.TienPhiDichVu > 0) tagParts.Add($"Phí DV ({_ban.PhiDichVuPt:0.##}%): +{_ban.TienPhiDichVu:N0}đ");
                    if (_ban.TienThue > 0) tagParts.Add($"VAT ({_ban.ThueSuatPt:0.##}%): +{_ban.TienThue:N0}đ");
                    
                    BorderKhuyenMaiTag.Visibility = Visibility.Visible;
                    TxtKhuyenMaiTag.Text = "📊 " + string.Join(" | ", tagParts);
                }
                else
                {
                    BorderKhuyenMaiTag.Visibility = Visibility.Collapsed;
                }
            }

            Loaded += async (s, e) =>
            {
                try
                {
                    var configs = await LocalCauHinhService.LoadAllConfigsAsync();
                    
                    // 1. Cho phép in tạm tính
                    bool choPhepInTamTinh = !configs.TryGetValue("ChoPhepInTamTinh", out var cp) || cp == "1" || cp.Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (BtnInTamTinh != null) BtnInTamTinh.Visibility = choPhepInTamTinh ? Visibility.Visible : Visibility.Collapsed;

                    // 2. Cho phép khách nợ
                    bool choPhepKhachNo = configs.TryGetValue("ChoPhepKhachNo", out var cno) && (cno == "1" || cno.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (ChkKhachHangNo != null) ChkKhachHangNo.Visibility = choPhepKhachNo ? Visibility.Visible : Visibility.Collapsed;

                    // 3. Có thanh toán voucher
                    bool coVoucher = configs.TryGetValue("CoThanhToanVoucher", out var cttv) && (cttv == "1" || cttv.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (RowVoucher != null) RowVoucher.Visibility = coVoucher ? Visibility.Visible : Visibility.Collapsed;

                    bool chonVoucherTuDs = configs.TryGetValue("LuaChonVoucherTuDanhSach", out var lcv) && (lcv == "1" || lcv.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (BtnChonVoucher != null) BtnChonVoucher.Visibility = (coVoucher && chonVoucherTuDs) ? Visibility.Visible : Visibility.Collapsed;

                    // 4. Sử dụng thẻ trả trước
                    bool suDungTheTraTruoc = !configs.TryGetValue("SuDungTheTraTruoc", out var sdtt) || sdtt == "1" || sdtt.Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (RowTheTraTruoc != null) RowTheTraTruoc.Visibility = suDungTheTraTruoc ? Visibility.Visible : Visibility.Collapsed;

                    // 5. Sử dụng điểm tích lũy để thanh toán
                    bool suDungDiem = configs.TryGetValue("SuDungDiemTichLuyDeThanhToan", out var sdd) && (sdd == "1" || sdd.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (RowDiemTichLuy != null) RowDiemTichLuy.Visibility = suDungDiem ? Visibility.Visible : Visibility.Collapsed;

                    if (configs.TryGetValue("QuyDoi1DiemSangTien", out var qdd) && decimal.TryParse(qdd.Replace(",", "").Replace(".", ""), out var qdVal) && qdVal > 0)
                    {
                        _quyDoi1DiemSangTien = qdVal;
                    }

                    // 6. Có thanh toán thẻ
                    bool coThe = !configs.TryGetValue("CoThanhToanThe", out var ctt) || ctt == "1" || ctt.Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (RowTheATM != null) RowTheATM.Visibility = coThe ? Visibility.Visible : Visibility.Collapsed;

                    // 7. Có thanh toán chuyển khoản
                    bool coCK = !configs.TryGetValue("CoThanhToanChuyenKhoan", out var cck) || cck == "1" || cck.Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (RowChuyenKhoan != null) RowChuyenKhoan.Visibility = coCK ? Visibility.Visible : Visibility.Collapsed;
                    if (RowTaiKhoan != null)
                    {
                        RowTaiKhoan.Visibility = coCK ? Visibility.Visible : Visibility.Collapsed;
                        await LoadTaiKhoanNganHangAsync();
                    }

                    // 8. Làm tròn tiền
                    if (configs.TryGetValue("LamTronTien", out var lt) && decimal.TryParse(lt.Replace(",", "").Replace(".", ""), out var ltVal) && ltVal > 0)
                    {
                        _lamTronTien = ltVal;
                        if (_ban != null && _lamTronTien > 1)
                        {
                            TongTien = Math.Round(_ban.TongCong / _lamTronTien, MidpointRounding.AwayFromZero) * _lamTronTien;
                            TxtTongTien.Text = TongTien.ToString("N0");
                            TxtKhachDua.Text = TongTien.ToString("N0");
                            KhachDua = TongTien;
                        }
                    }

                    // 9. Bắt buộc in khi thanh toán
                    bool batBuocIn = configs.TryGetValue("BatBuocInKhiThanhToan", out var bbi) && (bbi == "1" || bbi.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (batBuocIn && BtnDongBillKhongIn != null)
                    {
                        BtnDongBillKhongIn.Visibility = Visibility.Collapsed;
                    }

                    // 10. Sử dụng chức năng tạm ứng trong đơn hàng
                    bool suDungTamUng = configs.TryGetValue("SuDungChucNangTamUngTrongDonHang", out var sdtu) && (sdtu == "1" || sdtu.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (RowTamUng != null) RowTamUng.Visibility = suDungTamUng ? Visibility.Visible : Visibility.Collapsed;
                }
                catch { }

                TxtKhachDua.Focus();
                TxtKhachDua.SelectAll();
            };
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                e.Handled = true;
                BtnInTamTinh_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.F9)
            {
                e.Handled = true;
                BtnDongBillKhongIn_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                BtnHuyBo_Click(this, new RoutedEventArgs());
            }
        }

        private void DiemTichLuy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            int diem = int.TryParse(TxtDiemTichLuy?.Text?.Trim(), out var d) ? d : 0;
            DiemDoi = diem;
            TienDiem = diem * _quyDoi1DiemSangTien;
            if (TxtTienDiem != null) TxtTienDiem.Text = TienDiem.ToString("N0");
            RecalculateTotals();
        }

        private void MoneyInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            decimal kd = ParseDecimal(TxtKhachDua?.Text);
            decimal atm = ParseDecimal(TxtTheATM?.Text);
            decimal ck = ParseDecimal(TxtChuyenKhoan?.Text);
            decimal tt = ParseDecimal(TxtTheTraTruoc?.Text);
            decimal vc = ParseDecimal(TxtVoucher?.Text);
            decimal tu = ParseDecimal(TxtTamUng?.Text);

            KhachDua = kd;
            TheATM = atm;
            ChuyenKhoan = ck;
            TheTraTruoc = tt;
            Voucher = vc;
            TamUng = tu;

            decimal totalPaid = kd + atm + ck + tt + vc + TienDiem + tu;
            decimal change = totalPaid - TongTien;

            if (change < 0) change = 0;
            TraLai = change;
            if (TxtTraLai != null)
            {
                TxtTraLai.Text = change.ToString("N0");
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string clean = text.Replace(",", "").Replace(".", "").Trim();
            decimal.TryParse(clean, out decimal val);
            return val;
        }

        private void BtnNhapTheTraTruoc_Click(object sender, RoutedEventArgs e)
        {
            var win = new InputWindow("NHẬP THẺ TRẢ TRƯỚC", "MÃ THẺ", MaTheTraTruoc ?? "");
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.InputText))
            {
                MaTheTraTruoc = win.InputText.Trim();
                TxtTheTraTruoc.Text = TongTien.ToString("N0");
                TxtKhachDua.Text = "0";
            }
        }

        private void BtnChonVoucher_Click(object sender, RoutedEventArgs e)
        {
            var win = new InputWindow("NHẬP HOẶC QUÉT MÃ VOUCHER", "MÃ VOUCHER", MaVoucher ?? "");
            win.Owner = this;
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.InputText))
            {
                MaVoucher = win.InputText.Trim();
                // Gán mặc định hoặc gợi ý voucher
                if (ParseDecimal(TxtVoucher.Text) == 0)
                {
                    TxtVoucher.Text = (TongTien > 50000 ? 50000 : TongTien).ToString("N0");
                }
            }
        }

        private void ChkKhachHangNo_Checked(object sender, RoutedEventArgs e)
        {
            IsKhachNo = true;
            TxtKhachDua.Text = "0";
            TxtTheATM.Text = "0";
            TxtChuyenKhoan.Text = "0";
            TxtTheTraTruoc.Text = "0";
            TxtVoucher.Text = "0";
            TxtTamUng.Text = "0";
            TxtDiemTichLuy.Text = "0";
        }

        private void ChkKhachHangNo_Unchecked(object sender, RoutedEventArgs e)
        {
            IsKhachNo = false;
            TxtKhachDua.Text = TongTien.ToString("N0");
        }

        private void BtnInTamTinh_Click(object sender, RoutedEventArgs e)
        {
            if (_ban != null)
            {
                var printWin = new HoaDonBanHangPrintWindow(_ban, isTamTinh: true);
                printWin.Owner = this;
                printWin.ShowDialog();
            }
        }

        private bool ValidateCustomerBeforePayment()
        {
            if (IsKhachNo)
            {
                if (_ban == null || string.IsNullOrWhiteSpace(_ban.KhachHangName) ||
                    _ban.KhachHangName.Equals("Khách lẻ", StringComparison.OrdinalIgnoreCase) ||
                    _ban.KhachHangName.Equals("KHÁCH LẺ", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Khách hàng nợ bắt buộc phải chọn khách hàng cụ thể (không thể để Khách lẻ)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
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

        private async Task LoadTaiKhoanNganHangAsync()
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
                Console.WriteLine("Error LoadTaiKhoanNganHangAsync: " + ex.Message);
            }
        }

        private void CboTaiKhoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            string selectedId = CboTaiKhoan?.SelectedValue?.ToString();
            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                decimal ck = ParseDecimal(TxtChuyenKhoan?.Text);
                decimal kd = ParseDecimal(TxtKhachDua?.Text);
                if (ck == 0 && kd > 0)
                {
                    TxtChuyenKhoan.Text = kd.ToString("N0");
                    TxtKhachDua.Text = "0";
                }
            }
            else
            {
                decimal ck = ParseDecimal(TxtChuyenKhoan?.Text);
                decimal kd = ParseDecimal(TxtKhachDua?.Text);
                if (kd == 0 && ck > 0)
                {
                    TxtKhachDua.Text = ck.ToString("N0");
                    TxtChuyenKhoan.Text = "0";
                }
            }
        }

        private void BtnDongBillVaIn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCustomerBeforePayment()) return;
            string selId = CboTaiKhoan?.SelectedValue?.ToString();
            SelectedTaiKhoanNganHangId = string.IsNullOrWhiteSpace(selId) ? null : selId;
            SelectedTaiKhoanNganHangName = string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId) ? null : (CboTaiKhoan?.SelectedItem as TaiKhoanNganHangComboItem)?.Name;
            
            if (!string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId))
            {
                decimal ck = ParseDecimal(TxtChuyenKhoan?.Text);
                decimal kd = ParseDecimal(TxtKhachDua?.Text);
                if (ck == 0 && kd > 0)
                {
                    ChuyenKhoan = kd;
                    KhachDua = 0;
                }
            }

            IsInBill = true;
            DialogResult = true;
            Close();
        }

        private void BtnDongBillKhongIn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCustomerBeforePayment()) return;
            string selId = CboTaiKhoan?.SelectedValue?.ToString();
            SelectedTaiKhoanNganHangId = string.IsNullOrWhiteSpace(selId) ? null : selId;
            SelectedTaiKhoanNganHangName = string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId) ? null : (CboTaiKhoan?.SelectedItem as TaiKhoanNganHangComboItem)?.Name;
            
            if (!string.IsNullOrWhiteSpace(SelectedTaiKhoanNganHangId))
            {
                decimal ck = ParseDecimal(TxtChuyenKhoan?.Text);
                decimal kd = ParseDecimal(TxtKhachDua?.Text);
                if (ck == 0 && kd > 0)
                {
                    ChuyenKhoan = kd;
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
    }
}
