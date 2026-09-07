using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                    bool coCK = configs.TryGetValue("CoThanhToanChuyenKhoan", out var cck) && (cck == "1" || cck.Equals("true", StringComparison.OrdinalIgnoreCase));
                    if (RowChuyenKhoan != null) RowChuyenKhoan.Visibility = coCK ? Visibility.Visible : Visibility.Collapsed;

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

        private void BtnDongBillVaIn_Click(object sender, RoutedEventArgs e)
        {
            IsInBill = true;
            DialogResult = true;
            Close();
        }

        private void BtnDongBillKhongIn_Click(object sender, RoutedEventArgs e)
        {
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
