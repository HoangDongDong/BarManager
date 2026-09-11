using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.InAn;
using static QuanLyBar.Client.Services.LocalBaoCaoKhoHangService;

namespace QuanLyBar.Client.Views.BaoCaoKhoHang
{
    public partial class BaoCaoChuyenKhoControl : UserControl, QuanLyBar.Client.Models.IReportWithFilters
    {
        public enum ReportMode
        {
            TH_NV_Nhan,
            TH_NV_Chuyen,
            TH_Ngay,
            DS_NV_Xuat,
            DS_NV_Nhap,
            DS_Ngay
        }

        private readonly LocalBaoCaoKhoHangService _reportService = new LocalBaoCaoKhoHangService();
        private readonly ReportMode _mode;
        private bool _isLoaded = false;
        private List<QuanLyBar.Client.Services.ReportColumnConfigItem>? _colConfigs;
        private QuanLyBar.Client.Services.ReportPaperLayout? _currentLayout;

        private List<BaoCaoChuyenKhoTongHopMatHangItem> _dataTh = new List<BaoCaoChuyenKhoTongHopMatHangItem>();
        private List<BaoCaoChuyenKhoDanhSachItem> _dataDs = new List<BaoCaoChuyenKhoDanhSachItem>();

        public BaoCaoChuyenKhoControl(ReportMode mode = ReportMode.DS_Ngay)
        {
            InitializeComponent();
            _mode = mode;
            UpdateTitle();
        }

        private QuanLyBar.Client.Models.ReportFilterParams? _pendingParams;

        private class ActiveReportCol<T>
        {
            public string Key { get; set; } = "";
            public string Caption { get; set; } = "";
            public GridLength Width { get; set; } = new GridLength(100);
            public HorizontalAlignment Align { get; set; } = HorizontalAlignment.Left;
            public Func<T, string> ValueFunc { get; set; } = _ => "";
            public bool IsTotalQuantity { get; set; } = false;
            public bool IsTotalMoney { get; set; } = false;
        }

        public void ApplyFilterParams(QuanLyBar.Client.Models.ReportFilterParams p)
        {
            if (p == null) return;
            _pendingParams = p;

            if (p.TuNgay.HasValue) DpTuNgay.SelectedDate = p.TuNgay.Value;
            if (p.DenNgay.HasValue) DpDenNgay.SelectedDate = p.DenNgay.Value;

            SelectCombo(CboKhoXuat, p.KhoXuatId ?? p.KhoId);
            SelectCombo(CboKhoNhap, p.KhoNhapId);
            SelectCombo(CboNhanVien, p.NhanVienId);
            SelectCombo(CboNhomHang, p.NhomHangId);

            if (_isLoaded)
            {
                _ = LoadDataAsync();
            }
        }

        private void SelectCombo(ComboBox cbo, string? id)
        {
            if (string.IsNullOrEmpty(id) || cbo == null) return;
            cbo.SelectedValue = id;
            if (cbo.SelectedItem == null && cbo.ItemsSource != null)
            {
                foreach (var item in cbo.ItemsSource)
                {
                    if (item is FilterComboItem fi && fi.Id == id)
                    {
                        cbo.SelectedItem = fi;
                        break;
                    }
                }
            }
        }

        private async Task LoadTemplateConfigAsync(QuanLyBar.Client.Services.ReportPaperLayout? immediateLayout = null)
        {
            try
            {
                if (immediateLayout == null)
                {
                    var fullConfig = await QuanLyBar.Client.Services.ReportTemplateConfigService.GetFullTemplateConfigAsync(TxtReportTitle.Text);
                    _colConfigs = fullConfig.Columns;
                    _currentLayout = fullConfig.Layout;
                }
                else
                {
                    _currentLayout = immediateLayout;
                }
                ApplyLayoutToPaper(_currentLayout);
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadTemplateConfigAsync error: " + ex.Message);
            }
        }

        private void ApplyLayoutToPaper(QuanLyBar.Client.Services.ReportPaperLayout? layout)
        {
            if (layout == null) return;

            ReportPaper.Width = layout.PaperWidth;
            ReportPaper.MinHeight = layout.PaperMinHeight;
            ReportPaper.Padding = layout.PaperPadding;

            if (layout.Is80mm)
            {
                // Header quán
                ColLogo.Width = new GridLength(0);
                BrdLogo.Visibility = Visibility.Collapsed;
                PnlCompanyText.HorizontalAlignment = HorizontalAlignment.Center;
                TxtCompanyName.HorizontalAlignment = HorizontalAlignment.Center;
                TxtCompanyName.FontSize = 13;
                TxtCompanyAddress.HorizontalAlignment = HorizontalAlignment.Center;
                TxtCompanyAddress.FontSize = 9.5;
                TxtCompanyAddress.TextWrapping = TextWrapping.Wrap;
                TxtCompanyContact.HorizontalAlignment = HorizontalAlignment.Center;
                TxtCompanyContact.FontSize = 9.5;

                // Tiêu đề báo cáo
                ColTitleLeft.Width = new GridLength(1, GridUnitType.Star);
                ColTitleRight.Width = new GridLength(0);
                Grid.SetRow(TxtReportTitle, 0);
                Grid.SetColumn(TxtReportTitle, 0);
                Grid.SetColumnSpan(TxtReportTitle, 2);
                TxtReportTitle.HorizontalAlignment = HorizontalAlignment.Center;
                TxtReportTitle.FontSize = 13;
                TxtReportTitle.TextWrapping = TextWrapping.Wrap;
                TxtReportTitle.Margin = new Thickness(0, 0, 0, 6);

                Grid.SetRow(PnlDateAndFilter, 1);
                Grid.SetColumn(PnlDateAndFilter, 0);
                Grid.SetColumnSpan(PnlDateAndFilter, 2);
                PnlDateAndFilter.HorizontalAlignment = HorizontalAlignment.Center;
                TxtSubTitleDate.HorizontalAlignment = HorizontalAlignment.Center;
                TxtSubTitleDate.FontSize = 10;
                TxtFilterSummary.HorizontalAlignment = HorizontalAlignment.Center;
                TxtFilterSummary.FontSize = 9;

                // Chữ ký: 2 cột
                SigCol0.Width = new GridLength(1, GridUnitType.Star);
                SigCol1.Width = new GridLength(0);
                SigCol2.Width = new GridLength(0);
                SigCol3.Width = new GridLength(1, GridUnitType.Star);
                SigBlock0.Visibility = Visibility.Visible;
                SigBlock1.Visibility = Visibility.Collapsed;
                SigBlock2.Visibility = Visibility.Collapsed;
                SigBlock3.Visibility = Visibility.Visible;
            }
            else
            {
                // A4 (Thẳng đứng hoặc Nằm ngang)
                ColLogo.Width = new GridLength(100);
                BrdLogo.Visibility = Visibility.Visible;
                PnlCompanyText.HorizontalAlignment = HorizontalAlignment.Right;
                TxtCompanyName.HorizontalAlignment = HorizontalAlignment.Right;
                TxtCompanyName.FontSize = 16;
                TxtCompanyAddress.HorizontalAlignment = HorizontalAlignment.Right;
                TxtCompanyAddress.FontSize = 10.5;
                TxtCompanyContact.HorizontalAlignment = HorizontalAlignment.Right;
                TxtCompanyContact.FontSize = 10.5;

                // Tiêu đề báo cáo
                ColTitleLeft.Width = new GridLength(1, GridUnitType.Star);
                ColTitleRight.Width = new GridLength(1, GridUnitType.Auto);
                Grid.SetRow(TxtReportTitle, 1);
                Grid.SetColumn(TxtReportTitle, 1);
                Grid.SetColumnSpan(TxtReportTitle, 1);
                TxtReportTitle.HorizontalAlignment = HorizontalAlignment.Right;
                TxtReportTitle.FontSize = layout.TitleFontSize;
                TxtReportTitle.Margin = new Thickness(0);

                Grid.SetRow(PnlDateAndFilter, 1);
                Grid.SetColumn(PnlDateAndFilter, 0);
                Grid.SetColumnSpan(PnlDateAndFilter, 1);
                PnlDateAndFilter.HorizontalAlignment = HorizontalAlignment.Left;
                TxtSubTitleDate.HorizontalAlignment = HorizontalAlignment.Left;
                TxtSubTitleDate.FontSize = 11.5;
                TxtFilterSummary.HorizontalAlignment = HorizontalAlignment.Left;
                TxtFilterSummary.FontSize = 10.5;

                // Chữ ký: 4 cột
                SigCol0.Width = new GridLength(1, GridUnitType.Star);
                SigCol1.Width = new GridLength(1, GridUnitType.Star);
                SigCol2.Width = new GridLength(1, GridUnitType.Star);
                SigCol3.Width = new GridLength(1, GridUnitType.Star);
                SigBlock0.Visibility = Visibility.Visible;
                SigBlock1.Visibility = Visibility.Visible;
                SigBlock2.Visibility = Visibility.Visible;
                SigBlock3.Visibility = Visibility.Visible;
            }
        }

        private bool IsColVisible(params string[] aliases)
        {
            if (_colConfigs == null || _colConfigs.Count == 0) return true;
            return _colConfigs.IsColumnVisible(aliases, true);
        }

        private List<ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>> GetActiveColsTH()
        {
            var list = new List<ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>>();
            bool is80 = _currentLayout?.Is80mm == true;

            // 1. STT (Always column 0)
            list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
            {
                Key = "STT",
                Caption = "STT",
                Width = new GridLength(is80 ? 25 : 35),
                Align = HorizontalAlignment.Center,
                ValueFunc = x => x.STT.ToString()
            });

            // 2. Mã hàng
            if (IsColVisible("Mã hàng", "DMATHANG_CODE", "MaHang"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
                {
                    Key = "Mã hàng",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Mã hàng", "DMATHANG_CODE" }, "Mã hàng"),
                    Width = new GridLength(is80 ? 45 : 85),
                    Align = _colConfigs.GetColumnAlign(new[] { "Mã hàng", "DMATHANG_CODE" }, HorizontalAlignment.Center),
                    ValueFunc = x => x.MaHang
                });
            }

            // 3. Tên mặt hàng
            if (IsColVisible("Tên mặt hàng", "Mặt hàng", "DMATHANG_NAME", "TenHang"))
            {
                string cap = _colConfigs.GetColumnCaption(new[] { "Tên mặt hàng", "Mặt hàng", "DMATHANG_NAME" }, "Tên mặt hàng");
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
                {
                    Key = "Tên mặt hàng",
                    Caption = cap,
                    Width = new GridLength(1, GridUnitType.Star),
                    Align = _colConfigs.GetColumnAlign(new[] { "Tên mặt hàng", "Mặt hàng", "DMATHANG_NAME" }, HorizontalAlignment.Left),
                    ValueFunc = x => x.TenHang
                });
            }

            // 4. ĐVT
            if (IsColVisible("ĐVT", "DDONVITINH_NAME", "DVT"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
                {
                    Key = "ĐVT",
                    Caption = _colConfigs.GetColumnCaption(new[] { "ĐVT", "DDONVITINH_NAME" }, "ĐVT"),
                    Width = new GridLength(is80 ? 28 : 60),
                    Align = _colConfigs.GetColumnAlign(new[] { "ĐVT", "DDONVITINH_NAME" }, HorizontalAlignment.Center),
                    ValueFunc = x => x.DVT
                });
            }

            // 5. Số lượng
            if (IsColVisible("Số lượng", "SLNHAPCHUAQUYDOI", "SLXUATCHUAQUYDOI", "SOLUONG"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
                {
                    Key = "Số lượng",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Số lượng", "SLNHAPCHUAQUYDOI", "SOLUONG" }, "Số lượng"),
                    Width = new GridLength(is80 ? 38 : 90),
                    Align = _colConfigs.GetColumnAlign(new[] { "Số lượng", "SLNHAPCHUAQUYDOI", "SOLUONG" }, HorizontalAlignment.Right),
                    ValueFunc = x => x.SoLuong.ToString("#,##0.##"),
                    IsTotalQuantity = true
                });
            }

            // 6. Đơn giá
            if (IsColVisible("Đơn giá", "DONGIA"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
                {
                    Key = "Đơn giá",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Đơn giá", "DONGIA" }, "Đơn giá"),
                    Width = new GridLength(is80 ? 50 : 100),
                    Align = _colConfigs.GetColumnAlign(new[] { "Đơn giá", "DONGIA" }, HorizontalAlignment.Right),
                    ValueFunc = x => x.DonGia.ToString("#,##0")
                });
            }

            // 7. Thành tiền
            if (IsColVisible("Thành tiền", "THANHTIEN"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoTongHopMatHangItem>
                {
                    Key = "Thành tiền",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Thành tiền", "THANHTIEN" }, "Thành tiền"),
                    Width = new GridLength(is80 ? 65 : 130),
                    Align = _colConfigs.GetColumnAlign(new[] { "Thành tiền", "THANHTIEN" }, HorizontalAlignment.Right),
                    ValueFunc = x => x.ThanhTien.ToString("#,##0"),
                    IsTotalMoney = true
                });
            }

            return list;
        }

        private List<ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>> GetActiveColsDS()
        {
            var list = new List<ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>>();
            bool is80 = _currentLayout?.Is80mm == true;

            // 1. STT
            list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
            {
                Key = "STT",
                Caption = "STT",
                Width = new GridLength(is80 ? 25 : 35),
                Align = HorizontalAlignment.Center,
                ValueFunc = x => x.STT.ToString()
            });

            // 2. Số phiếu
            if (IsColVisible("Số phiếu", "SOPHIEU", "TDONHANG_NAME"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "Số phiếu",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Số phiếu", "SOPHIEU", "TDONHANG_NAME" }, "Số phiếu"),
                    Width = new GridLength(is80 ? 55 : 85),
                    Align = _colConfigs.GetColumnAlign(new[] { "Số phiếu", "SOPHIEU" }, HorizontalAlignment.Center),
                    ValueFunc = x => x.SoPhieu
                });
            }

            // 3. Ngày
            if (IsColVisible("Ngày", "NGAY", "TDONHANG_NGAY"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "Ngày",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Ngày", "NGAY", "TDONHANG_NGAY" }, "Ngày"),
                    Width = new GridLength(is80 ? 55 : 85),
                    Align = _colConfigs.GetColumnAlign(new[] { "Ngày", "NGAY" }, HorizontalAlignment.Center),
                    ValueFunc = x => x.NgayDisplay
                });
            }

            // 4. Kho xuất
            if (IsColVisible("Kho xuất", "KHOXUAT", "DKHOXUAT_NAME"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "Kho xuất",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Kho xuất", "KHOXUAT", "DKHOXUAT_NAME" }, "Kho xuất"),
                    Width = new GridLength(is80 ? 50 : 1, is80 ? GridUnitType.Pixel : GridUnitType.Star),
                    Align = _colConfigs.GetColumnAlign(new[] { "Kho xuất", "KHOXUAT" }, HorizontalAlignment.Left),
                    ValueFunc = x => x.KhoXuat
                });
            }

            // 5. Kho nhập
            if (IsColVisible("Kho nhập", "KHONHAP", "DKHONHAP_NAME"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "Kho nhập",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Kho nhập", "KHONHAP", "DKHONHAP_NAME" }, "Kho nhập"),
                    Width = new GridLength(is80 ? 50 : 1, is80 ? GridUnitType.Pixel : GridUnitType.Star),
                    Align = _colConfigs.GetColumnAlign(new[] { "Kho nhập", "KHONHAP" }, HorizontalAlignment.Left),
                    ValueFunc = x => x.KhoNhap
                });
            }

            // 6. NV xuất
            if (IsColVisible("NV xuất", "NHANVIENXUAT", "Nhân viên xuất", "DNHANVIENXUAT_NAME"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "NV xuất",
                    Caption = _colConfigs.GetColumnCaption(new[] { "NV xuất", "Nhân viên xuất", "NHANVIENXUAT" }, "NV xuất"),
                    Width = new GridLength(is80 ? 55 : 100),
                    Align = _colConfigs.GetColumnAlign(new[] { "NV xuất", "Nhân viên xuất" }, HorizontalAlignment.Left),
                    ValueFunc = x => x.NhanVienXuat
                });
            }

            // 7. NV nhập
            if (IsColVisible("NV nhập", "NHANVIENNHAP", "Nhân viên nhập", "DNHANVIENNHAP_NAME"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "NV nhập",
                    Caption = _colConfigs.GetColumnCaption(new[] { "NV nhập", "Nhân viên nhập", "NHANVIENNHAP" }, "NV nhập"),
                    Width = new GridLength(is80 ? 55 : 95),
                    Align = _colConfigs.GetColumnAlign(new[] { "NV nhập", "Nhân viên nhập" }, HorizontalAlignment.Left),
                    ValueFunc = x => x.NhanVienNhap
                });
            }

            // 8. Tổng cộng
            if (IsColVisible("Tổng cộng", "TONGCONG", "Thành tiền", "THANHTIEN"))
            {
                list.Add(new ActiveReportCol<BaoCaoChuyenKhoDanhSachItem>
                {
                    Key = "Tổng cộng",
                    Caption = _colConfigs.GetColumnCaption(new[] { "Tổng cộng", "TONGCONG", "Thành tiền", "THANHTIEN" }, "Tổng cộng"),
                    Width = new GridLength(is80 ? 65 : 80),
                    Align = _colConfigs.GetColumnAlign(new[] { "Tổng cộng", "Thành tiền" }, HorizontalAlignment.Right),
                    ValueFunc = x => x.TongCong.ToString("#,##0"),
                    IsTotalMoney = true
                });
            }

            return list;
        }

        private void UpdateTitle()
        {
            switch (_mode)
            {
                case ReportMode.TH_NV_Nhan:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NHÂN VIÊN NHẬN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_NV_Chuyen:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NHÂN VIÊN CHUYỂN";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.TH_Ngay:
                    TxtReportTitle.Text = "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Visible;
                    CboNhomHang.Visibility = Visibility.Visible;
                    break;
                case ReportMode.DS_NV_Xuat:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHUYỂN KHO THEO NHÂN VIÊN XUẤT";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_NV_Nhap:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHUYỂN KHO THEO NHÂN VIÊN NHẬP";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
                case ReportMode.DS_Ngay:
                    TxtReportTitle.Text = "DANH SÁCH PHIẾU CHUYỂN KHO THEO NGÀY";
                    LblNhomHang.Visibility = Visibility.Collapsed;
                    CboNhomHang.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            var today = DateTime.Today;
            if (DpTuNgay.SelectedDate == null) DpTuNgay.SelectedDate = today;
            if (DpDenNgay.SelectedDate == null) DpDenNgay.SelectedDate = today;
            TxtSignDate.Text = $"Ngày {today:dd} tháng {today:MM} năm {today:yyyy}";

            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();

            if (_pendingParams != null)
            {
                SelectCombo(CboKhoXuat, _pendingParams.KhoXuatId ?? _pendingParams.KhoId);
                SelectCombo(CboKhoNhap, _pendingParams.KhoNhapId);
                SelectCombo(CboNhanVien, _pendingParams.NhanVienId);
                SelectCombo(CboNhomHang, _pendingParams.NhomHangId);
            }

            await LoadTemplateConfigAsync();
            BuildTableHeader();
            await LoadDataAsync();
        }

        private async Task LoadCompanyInfoAndLogoAsync()
        {
            try
            {
                var comp = await LocalCauHinhService.GetCompanyInfoAsync();
                TxtCompanyName.Text = comp.Name;
                TxtCompanyAddress.Text = comp.FormattedAddress;
                TxtCompanyContact.Text = comp.FormattedContact;

                if (comp.LogoBytes != null && comp.LogoBytes.Length > 0)
                {
                    var bi = LocalCauHinhService.ImageFromBytes(comp.LogoBytes);
                    if (bi != null)
                    {
                        ImgLogo.Source = bi;
                        ImgLogo.Visibility = Visibility.Visible;
                        if (FindName("VbDefaultLogo") is UIElement vb) vb.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var curXuat = (CboKhoXuat.SelectedItem as FilterComboItem)?.Id;
                var curNhap = (CboKhoNhap.SelectedItem as FilterComboItem)?.Id;
                var curNv = (CboNhanVien.SelectedItem as FilterComboItem)?.Id;
                var curNhom = (CboNhomHang.SelectedItem as FilterComboItem)?.Id;

                var khoList = await _reportService.GetKhoHangFilterAsync();
                CboKhoXuat.ItemsSource = khoList;
                if (!string.IsNullOrEmpty(curXuat)) SelectCombo(CboKhoXuat, curXuat);
                else if (CboKhoXuat.SelectedIndex < 0 && khoList.Count > 0) CboKhoXuat.SelectedIndex = 0;

                var khoNhapList = await _reportService.GetKhoHangFilterAsync();
                CboKhoNhap.ItemsSource = khoNhapList;
                if (!string.IsNullOrEmpty(curNhap)) SelectCombo(CboKhoNhap, curNhap);
                else if (CboKhoNhap.SelectedIndex < 0 && khoNhapList.Count > 0) CboKhoNhap.SelectedIndex = 0;

                var nvList = await _reportService.GetNhanVienFilterAsync();
                CboNhanVien.ItemsSource = nvList;
                if (!string.IsNullOrEmpty(curNv)) SelectCombo(CboNhanVien, curNv);
                else if (CboNhanVien.SelectedIndex < 0 && nvList.Count > 0) CboNhanVien.SelectedIndex = 0;

                var nhomList = await _reportService.GetNhomMatHangFilterAsync();
                CboNhomHang.ItemsSource = nhomList;
                if (!string.IsNullOrEmpty(curNhom)) SelectCombo(CboNhomHang, curNhom);
                else if (CboNhomHang.SelectedIndex < 0 && nhomList.Count > 0) CboNhomHang.SelectedIndex = 0;
            }
            catch { }
        }

        private void BuildTableHeader()
        {
            BrdTableHeader.Child = null;
            double rowH = _currentLayout?.RowHeight ?? 26;
            Grid grid = new Grid { Height = rowH };

            if (_mode == ReportMode.DS_NV_Xuat || _mode == ReportMode.DS_NV_Nhap || _mode == ReportMode.DS_Ngay)
            {
                var cols = GetActiveColsDS();
                for (int i = 0; i < cols.Count; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[i].Width });
                    grid.Children.Add(CreateHeaderCell(cols[i].Caption, i, isLast: (i == cols.Count - 1)));
                }
            }
            else
            {
                var cols = GetActiveColsTH();
                for (int i = 0; i < cols.Count; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[i].Width });
                    grid.Children.Add(CreateHeaderCell(cols[i].Caption, i, isLast: (i == cols.Count - 1)));
                }
            }

            BrdTableHeader.Child = grid;
        }

        private Border CreateHeaderCell(string text, int col, bool isLast = false)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontSize = _currentLayout?.HeaderFontSize ?? 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            return b;
        }

        private Border CreateCell(string text, int col, HorizontalAlignment align = HorizontalAlignment.Left, bool isBold = false, bool isLast = false)
        {
            bool is80 = _currentLayout?.Is80mm == true;
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0),
                Padding = is80 ? new Thickness(2, 0, 2, 0) : new Thickness(4, 0, 4, 0)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontSize = _currentLayout?.CellFontSize ?? 12,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            return b;
        }

        private async Task LoadDataAsync()
        {
            if (DpTuNgay.SelectedDate == null || DpDenNgay.SelectedDate == null) return;

            DateTime tuNgay = DpTuNgay.SelectedDate.Value.Date;
            DateTime denNgay = DpDenNgay.SelectedDate.Value.Date;

            string khoXuatId = (CboKhoXuat.SelectedItem as FilterComboItem)?.Id ?? "";
            string khoNhapId = (CboKhoNhap.SelectedItem as FilterComboItem)?.Id ?? "";
            string nvId = (CboNhanVien.SelectedItem as FilterComboItem)?.Id ?? "";
            string nhomId = (CboNhomHang.SelectedItem as FilterComboItem)?.Id ?? "";

            if (tuNgay == denNgay)
                TxtSubTitleDate.Text = $"Ngày: {tuNgay:dd/MM/yyyy}";
            else
                TxtSubTitleDate.Text = $"Ngày từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            string khoXuatName = (CboKhoXuat.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string khoNhapName = (CboKhoNhap.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nhomName = (CboNhomHang.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            string nvName = (CboNhanVien.SelectedItem as FilterComboItem)?.Name ?? "Tất cả";
            var parts = new List<string>();
            if (Utilities.IsSpecificFilter(khoXuatName)) parts.Add($"Kho xuất: {khoXuatName}");
            if (Utilities.IsSpecificFilter(khoNhapName)) parts.Add($"Kho nhập: {khoNhapName}");
            if (Utilities.IsSpecificFilter(nhomName)) parts.Add($"Nhóm: {nhomName}");
            if (Utilities.IsSpecificFilter(nvName)) parts.Add($"Nhân viên: {nvName}");
            if (parts.Count > 0)
            {
                TxtFilterSummary.Text = string.Join("\n", parts);
                TxtFilterSummary.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                TxtFilterSummary.Text = "";
                TxtFilterSummary.Visibility = System.Windows.Visibility.Collapsed;
            }

            switch (_mode)
            {
                case ReportMode.TH_NV_Nhan:
                    _dataTh = await _reportService.GetChuyenKhoTongHopMatHangTheoNhanVienNhanAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
                    break;
                case ReportMode.TH_NV_Chuyen:
                    _dataTh = await _reportService.GetChuyenKhoTongHopMatHangTheoNhanVienChuyenAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
                    break;
                case ReportMode.TH_Ngay:
                    _dataTh = await _reportService.GetChuyenKhoTongHopMatHangTheoNgayAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nhomId, nvId);
                    break;
                case ReportMode.DS_NV_Xuat:
                    _dataDs = await _reportService.GetChuyenKhoDanhSachTheoNhanVienXuatAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
                    break;
                case ReportMode.DS_NV_Nhap:
                    _dataDs = await _reportService.GetChuyenKhoDanhSachTheoNhanVienNhapAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
                    break;
                case ReportMode.DS_Ngay:
                    _dataDs = await _reportService.GetChuyenKhoDanhSachTheoNgayAsync(tuNgay, denNgay, khoXuatId, khoNhapId, nvId);
                    break;
            }

            BuildTableHeader();
            RenderTable();
        }

        private void RenderTable()
        {
            StkDataRows.Children.Clear();
            string keyword = TxtFilter.Text.Trim().ToLower();
            double rowH = _currentLayout?.RowHeight ?? 26;
            double sumH = (_currentLayout?.Is80mm == true) ? 24 : 28;
            bool is80 = _currentLayout?.Is80mm == true;

            if (_mode == ReportMode.DS_NV_Xuat || _mode == ReportMode.DS_NV_Nhap || _mode == ReportMode.DS_Ngay)
            {
                var cols = GetActiveColsDS();
                var filtered = _dataDs;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataDs.Where(x =>
                        x.SoPhieu.ToLower().Contains(keyword) ||
                        x.KhoXuat.ToLower().Contains(keyword) ||
                        x.KhoNhap.ToLower().Contains(keyword) ||
                        x.NhanVienXuat.ToLower().Contains(keyword) ||
                        x.NhanVienNhap.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalTongCong = 0;

                var grouped = _mode == ReportMode.DS_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    (_mode == ReportMode.DS_NV_Xuat ?
                    filtered.GroupBy(x => x.NhanVienXuat).ToList() :
                    filtered.GroupBy(x => x.NhanVienNhap).ToList());

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    string grpLabel = _mode == ReportMode.DS_Ngay ? $"Ngày: {g.Key}" : (_mode == ReportMode.DS_NV_Xuat ? $"NV xuất: {g.Key}" : $"NV nhập: {g.Key}");
                    grpBorder.Child = new TextBlock
                    {
                        Text = grpLabel,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };
                    StkDataRows.Children.Add(grpBorder);

                    int stt = 1;
                    foreach (var item in g)
                    {
                        item.STT = stt++;
                        totalTongCong += item.TongCong;

                        Border rowBorder = new Border
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 1, 1),
                            Height = rowH
                        };

                        Grid rowGrid = new Grid();
                        for (int c = 0; c < cols.Count; c++)
                        {
                            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                            rowGrid.Children.Add(CreateCell(cols[c].ValueFunc(item), c, cols[c].Align, isLast: (c == cols.Count - 1)));
                        }

                        rowBorder.Child = rowGrid;
                        StkDataRows.Children.Add(rowBorder);
                    }
                }

                // Total
                Border sumBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = sumH
                };
                Grid sumGrid = new Grid();
                for (int c = 0; c < cols.Count; c++)
                {
                    sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                }

                int totalMoneyIdx = cols.FindLastIndex(c => c.IsTotalMoney);
                if (totalMoneyIdx > 0)
                {
                    Border span = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 0),
                        Padding = new Thickness(4, 0, 10, 0)
                    };
                    Grid.SetColumn(span, 0);
                    Grid.SetColumnSpan(span, totalMoneyIdx);
                    span.Child = new TextBlock
                    {
                        Text = "TỔNG CỘNG",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    sumGrid.Children.Add(span);

                    for (int c = totalMoneyIdx; c < cols.Count; c++)
                    {
                        string val = cols[c].IsTotalMoney ? totalTongCong.ToString("#,##0") : "";
                        sumGrid.Children.Add(CreateCell(val, c, cols[c].Align, isBold: true, isLast: (c == cols.Count - 1)));
                    }
                }
                else
                {
                    Border span = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(4, 0, 10, 0)
                    };
                    Grid.SetColumn(span, 0);
                    Grid.SetColumnSpan(span, cols.Count);
                    span.Child = new TextBlock
                    {
                        Text = $"TỔNG CỘNG: {totalTongCong:#,##0}",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    sumGrid.Children.Add(span);
                }

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
            else
            {
                var cols = GetActiveColsTH();
                var filtered = _dataTh;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = _dataTh.Where(x =>
                        x.MaHang.ToLower().Contains(keyword) ||
                        x.TenHang.ToLower().Contains(keyword) ||
                        x.DVT.ToLower().Contains(keyword)
                    ).ToList();
                }

                decimal totalQty = 0;
                decimal totalMoney = 0;

                var grouped = _mode == ReportMode.TH_Ngay ?
                    filtered.GroupBy(x => x.NgayDisplay).ToList() :
                    (_mode == ReportMode.TH_NV_Nhan ?
                    filtered.GroupBy(x => x.NhanVienNhan).ToList() :
                    filtered.GroupBy(x => x.NhanVienChuyen).ToList());

                foreach (var g in grouped)
                {
                    Border grpBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 1, 1),
                        Padding = new Thickness(6, 4, 6, 4)
                    };
                    string grpLabel = _mode == ReportMode.TH_Ngay ? $"Ngày: {g.Key}" : (_mode == ReportMode.TH_NV_Nhan ? $"NV nhận: {g.Key}" : $"NV chuyển: {g.Key}");
                    grpBorder.Child = new TextBlock
                    {
                        Text = grpLabel,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };
                    StkDataRows.Children.Add(grpBorder);

                    int stt = 1;
                    foreach (var item in g)
                    {
                        item.STT = stt++;
                        totalQty += item.SoLuong;
                        totalMoney += item.ThanhTien;

                        Border rowBorder = new Border
                        {
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 1, 1),
                            Height = rowH
                        };

                        Grid rowGrid = new Grid();
                        for (int c = 0; c < cols.Count; c++)
                        {
                            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                            rowGrid.Children.Add(CreateCell(cols[c].ValueFunc(item), c, cols[c].Align, isLast: (c == cols.Count - 1)));
                        }

                        rowBorder.Child = rowGrid;
                        StkDataRows.Children.Add(rowBorder);
                    }
                }

                // Total
                Border sumBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Height = sumH
                };
                Grid sumGrid = new Grid();
                for (int c = 0; c < cols.Count; c++)
                {
                    sumGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cols[c].Width });
                }

                int firstTotalIdx = cols.FindIndex(c => c.IsTotalQuantity || c.IsTotalMoney);
                if (firstTotalIdx > 0)
                {
                    Border span = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 1, 0),
                        Padding = new Thickness(4, 0, 10, 0)
                    };
                    Grid.SetColumn(span, 0);
                    Grid.SetColumnSpan(span, firstTotalIdx);
                    span.Child = new TextBlock
                    {
                        Text = "TỔNG CỘNG",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    sumGrid.Children.Add(span);

                    for (int c = firstTotalIdx; c < cols.Count; c++)
                    {
                        string val = "";
                        if (cols[c].IsTotalQuantity) val = totalQty.ToString("#,##0.##");
                        else if (cols[c].IsTotalMoney) val = totalMoney.ToString("#,##0");
                        sumGrid.Children.Add(CreateCell(val, c, cols[c].Align, isBold: true, isLast: (c == cols.Count - 1)));
                    }
                }
                else
                {
                    Border span = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(4, 0, 10, 0)
                    };
                    Grid.SetColumn(span, 0);
                    Grid.SetColumnSpan(span, cols.Count);
                    span.Child = new TextBlock
                    {
                        Text = $"TỔNG CỘNG: SL={totalQty:#,##0.##}, TIỀN={totalMoney:#,##0}",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    sumGrid.Children.Add(span);
                }

                sumBorder.Child = sumGrid;
                StkDataRows.Children.Add(sumBorder);
            }
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            await LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderTable();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chế độ xem trước trang in đã sẵn sàng trên màn hình.", "Xem trước", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(ReportPaper, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV UTF-8 (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"{TxtReportTitle.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (sfd.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine(TxtCompanyName.Text);
                    sb.AppendLine(TxtReportTitle.Text);
                    sb.AppendLine(TxtSubTitleDate.Text);
                    sb.AppendLine(TxtFilterSummary.Text);
                    sb.AppendLine();

                    if (_mode == ReportMode.DS_NV_Xuat || _mode == ReportMode.DS_NV_Nhap || _mode == ReportMode.DS_Ngay)
                    {
                        var cols = GetActiveColsDS();
                        sb.AppendLine(string.Join(",", cols.Select(c => $"\"{c.Caption}\"")));
                        foreach (var i in _dataDs)
                        {
                            sb.AppendLine(string.Join(",", cols.Select(c => $"\"{c.ValueFunc(i).Replace("\"", "\"\"")}\"")));
                        }
                    }
                    else
                    {
                        var cols = GetActiveColsTH();
                        sb.AppendLine(string.Join(",", cols.Select(c => $"\"{c.Caption}\"")));
                        foreach (var i in _dataTh)
                        {
                            sb.AppendLine(string.Join(",", cols.Select(c => $"\"{c.ValueFunc(i).Replace("\"", "\"\"")}\"")));
                        }
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThietKeMau_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new ThietKeMauInWindow(
                    TxtReportTitle.Text,
                    null,
                    async (cols) =>
                    {
                        await LoadTemplateConfigAsync();
                        BuildTableHeader();
                        RenderTable();
                    },
                    onLayoutCallback: layout => _ = LoadTemplateConfigAsync(layout));
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở thiết kế mẫu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThamSoTuyChinh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new TuyChonThamSoBaoCaoWindow(
                    TxtReportTitle.Text,
                    async () =>
                    {
                        await LoadFiltersAsync();
                        await LoadDataAsync();
                    });
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tùy chọn tham số: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXemDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (InlineDataBorder.Visibility == System.Windows.Visibility.Visible)
            {
                InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            // Use reflection to find the largest data list in this control
            var fields = this.GetType().GetFields(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object? dataSource = null;
            int maxCount = 0;
            foreach (var f in fields)
            {
                var val = f.GetValue(this);
                if (val is System.Collections.IList list && list.Count > maxCount)
                {
                    maxCount = list.Count;
                    dataSource = val;
                }
            }

            if (dataSource != null)
                InlineDataGrid.ItemsSource = (System.Collections.IEnumerable)dataSource;
            else
                InlineDataGrid.ItemsSource = new[] { new { ThongBao = "Không có dữ liệu. Hãy tải dữ liệu trước (F5) roi mo lai." } };

            TxtSoBanGhi.Text = $"Tổng số: {maxCount} bản ghi";
            InlineDataBorder.Visibility = System.Windows.Visibility.Visible;

            // Scroll to bottom so user can see the panel
            var scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(this);
            scrollViewer?.ScrollToEnd();
        }

        private void BtnDongDuLieuTho_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InlineDataBorder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
