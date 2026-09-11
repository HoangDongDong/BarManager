using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using QuanLyBar.Client.Services;
using QuanLyBar.Client.Views.BaoCaoQuy;

namespace QuanLyBar.Client.Views
{
    public partial class BaoCaoMainControl : UserControl
    {
        private class ReportCategoryNode
        {
            public string Name { get; set; } = "";
            public List<string> ReportNames { get; set; } = new List<string>();
            public List<ReportCategoryNode> SubCategories { get; set; } = new List<ReportCategoryNode>();
        }

        private class SReportDbItem
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string ParentId { get; set; } = "";
            public int ItemType { get; set; }
            public string SortOrder { get; set; } = "";
        }

        private static readonly Dictionary<string, List<ComboLookupItem>> _lookupCache = new Dictionary<string, List<ComboLookupItem>>();
        private List<ReportCategoryNode> _reportCategories = new List<ReportCategoryNode>();
        private List<string> _searchableReports = new List<string>();
        private string _currentSelectedReport = "DANH SÁCH PHIẾU NHẬP KHO THEO NGÀY";

        public BaoCaoMainControl()
        {
            InitializeComponent();
            InitDates();
            BuildReportTreeData();
            RebuildSearchableReports();
            RenderTreeView();
            RenderThuongDung();
            RefreshSearchResults();
            SelectInitialReport();
        }

        private void InitDates()
        {
            DateTime now = DateTime.Now;
            DpTuNgay.SelectedDate = new DateTime(now.Year, now.Month, 1);
            DpDenNgay.SelectedDate = now;
        }

        private async Task<List<ComboLookupItem>> GetLookupListAsync(string key)
        {
            if (_lookupCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var list = new List<ComboLookupItem>
            {
                new ComboLookupItem { Id = "", Name = "Tất cả" }
            };

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "";
                    switch (key)
                    {
                        case "KhoHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DKHOHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhomHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHOMMATHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "MatHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DMATHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "LyDoThuChi":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DLYDOTHUCHI WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME";
                            break;
                        case "CuaHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DCUAHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY ID";
                            break;
                        case "NhanVien":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHANVIEN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "KhachHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DKHACHHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhomKhach":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHOMKHACHHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhaCungCap":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhomNCC":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHOMNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "HangSanXuat":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DHANGSANXUAT WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhomNguoiDung":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM SGROUPUSER WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "TaiKhoanNganHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DTAIKHOANNGANHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "KhuVuc":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DKHUVUC WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "Ban":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DBAN WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhomHienThi":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHOMHIENTHI WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "LoaiMatHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DLOAIMATHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "TheTraTruoc":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DTHETRATRUOC WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "Voucher":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DVOUCHER WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                    }

                    if (!string.IsNullOrEmpty(sql))
                    {
                        var dbItems = (await conn.QueryAsync<ComboLookupItem>(sql)).ToList();
                        list.AddRange(dbItems);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLookupListAsync error [{key}]: " + ex.Message);
            }

            _lookupCache[key] = list;
            return list;
        }

        private async void LoadFiltersForReport(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName)) return;
            string r = reportName.Trim().ToUpper();

            try
            {
                var customFilters = await QuanLyBar.Client.Services.ReportTemplateConfigService.GetFilterConfigAsync(reportName);
                if (customFilters != null && customFilters.Count > 0)
                {
                    await ApplyCustomFilterConfigAsync(customFilters);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading custom filters: " + ex.Message);
            }

            if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Visible;
            PnlFilter1.Visibility = Visibility.Visible;
            PnlFilter2.Visibility = Visibility.Visible;
            PnlFilter3.Visibility = Visibility.Collapsed;
            PnlFilter4.Visibility = Visibility.Collapsed;
            if (PnlFilter5 != null) PnlFilter5.Visibility = Visibility.Collapsed;
            if (PnlFilterSoPhieu != null) PnlFilterSoPhieu.Visibility = Visibility.Collapsed;

            // -------------------------------------------------------------
            // BÁO CÁO DANH MỤC (10 reports)
            // -------------------------------------------------------------
            if (r == "DANH SÁCH KHÁCH HÀNG THEO NHÓM")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhóm khách hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomKhach");
                LblFilter2.Text = "Nhân viên";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
            }
            else if (r == "DANH SÁCH KHÁCH HÀNG THEO NHÂN VIÊN")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhân viên";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhanVien");
                LblFilter2.Text = "Nhóm khách hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhomKhach");
            }
            else if (r == "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhóm NCC";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomNCC");
                PnlFilter2.Visibility = Visibility.Collapsed;
            }
            else if (r == "DANH SÁCH ĐỢT KHUYẾN MẠI")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Trạng thái";
                CboFilter1.ItemsSource = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả" },
                    new ComboLookupItem { Id = "0", Name = "Đang áp dụng" },
                    new ComboLookupItem { Id = "1", Name = "Ngừng áp dụng" }
                };
                PnlFilter2.Visibility = Visibility.Collapsed;
            }
            else if (r == "DANH SÁCH MẶT HÀNG THEO NHÓM")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhóm hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomHang");
                LblFilter2.Text = "Kho hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("KhoHang");
            }
            else if (r == "DANH SÁCH MẶT HÀNG THEO HÃNG SẢN XUẤT")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Hãng sản xuất";
                CboFilter1.ItemsSource = await GetLookupListAsync("HangSanXuat");
                LblFilter2.Text = "Nhóm hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhomHang");
            }
            else if (r == "KHÁCH HÀNG ĐẾN NGÀY SINH NHẬT")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhóm khách hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomKhach");
                LblFilter2.Text = "Tháng sinh";
                var thangList = new List<ComboLookupItem> { new ComboLookupItem { Id = "", Name = "Tất cả" } };
                for (int i = 1; i <= 12; i++) thangList.Add(new ComboLookupItem { Id = i.ToString(), Name = $"Tháng {i}" });
                CboFilter2.ItemsSource = thangList;
            }
            else if (r == "BÁO CÁO CẤU HÌNH BÀN KHU VỰC")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Khu vực";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhuVuc");
                PnlFilter2.Visibility = Visibility.Collapsed;
            }
            else if (r == "CÔNG THỨC ĐỊNH LƯỢNG")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhóm hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomHang");
                LblFilter2.Text = "Mặt hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("MatHang");
            }
            else if (r == "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG")
            {
                if (PnlDateFilter != null) PnlDateFilter.Visibility = Visibility.Collapsed;
                LblFilter1.Text = "Nhóm người dùng";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomNguoiDung");
                PnlFilter2.Visibility = Visibility.Collapsed;
            }
            else if (r.Contains("TỒN QUỸ") || r.Contains("TON QUY"))
            {
                LblFilter1.Text = "Loại quỹ";
                CboFilter1.ItemsSource = new List<ComboLookupItem>
                {
                    new ComboLookupItem { Id = "", Name = "Tất cả" },
                    new ComboLookupItem { Id = "TIEN_MAT", Name = "Tiền mặt" },
                    new ComboLookupItem { Id = "NGAN_HANG", Name = "Ngân hàng" },
                    new ComboLookupItem { Id = "QUET_THE", Name = "Quẹt thẻ" }
                };
                LblFilter2.Text = "Tài khoản";
                CboFilter2.ItemsSource = await GetLookupListAsync("TaiKhoanNganHang");
            }
            else if (r.Contains("PHIẾU THU") || r.Contains("PHIEU THU") || r.Contains("PHIẾU CHI") || r.Contains("PHIEU CHI") || r.Contains("THU CHI") || r.Contains("QUỸ"))
            {
                LblFilter1.Text = "Lý do thu chi";
                CboFilter1.ItemsSource = await GetLookupListAsync("LyDoThuChi");
                LblFilter2.Text = "Cửa hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("CuaHang");
            }
            else if ((r.Contains("NHẬP") || r.Contains("NHAP")) && (r.Contains("MẶT HÀNG") || r.Contains("MAT HANG")))
            {
                LblFilter1.Text = "Nhà cung cấp";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhaCungCap");
                LblFilter2.Text = "Kho nhập";
                CboFilter2.ItemsSource = await GetLookupListAsync("KhoHang");
                LblFilter3.Text = "Nhân viên nhập";
                CboFilter3.ItemsSource = await GetLookupListAsync("NhanVien");
                PnlFilter3.Visibility = Visibility.Visible;
                LblFilter4.Text = "Nhóm mặt hàng";
                CboFilter4.ItemsSource = await GetLookupListAsync("NhomHang");
                PnlFilter4.Visibility = Visibility.Visible;
                if (PnlFilter5 != null)
                {
                    LblFilter5.Text = "Mặt hàng";
                    CboFilter5.ItemsSource = await GetLookupListAsync("MatHang");
                    PnlFilter5.Visibility = Visibility.Visible;
                }
            }
            else if (r.Contains("NHẬP") || r.Contains("NHAP"))
            {
                LblFilter1.Text = "Nhà cung cấp";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhaCungCap");
                LblFilter2.Text = "Nhân viên nhập";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
                if (PnlFilterSoPhieu != null) PnlFilterSoPhieu.Visibility = Visibility.Visible;
            }
            else if (r.Contains("XUẤT BÁN") || r.Contains("MẶT HÀNG BÁN") || (r.Contains("MẶT HÀNG") && r.Contains("BÁN")))
            {
                LblFilter1.Text = "Kho hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");
                LblFilter2.Text = "Nhóm hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhomHang");
                LblFilter3.Text = "Mặt hàng";
                CboFilter3.ItemsSource = await GetLookupListAsync("MatHang");
                PnlFilter3.Visibility = Visibility.Visible;
            }
            else if (r.Contains("NHÂN VIÊN") || r.Contains("NHAN VIEN"))
            {
                LblFilter1.Text = "Nhân viên";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhanVien");
                LblFilter2.Text = "Cửa hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("CuaHang");
            }
            else if (r.Contains("KHU VỰC") || r.Contains("BÀN"))
            {
                LblFilter1.Text = "Khu vực";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhuVuc");
                LblFilter2.Text = "Bàn";
                CboFilter2.ItemsSource = await GetLookupListAsync("Ban");
            }
            else if (r.Contains("THU NGÂN") || r.Contains("THU NGAN"))
            {
                LblFilter1.Text = "Thu ngân";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhanVien");
                LblFilter2.Text = "Cửa hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("CuaHang");
            }
            else if (r.Contains("KHÁCH HÀNG") || r.Contains("KHACH HANG"))
            {
                LblFilter1.Text = "Nhóm khách";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomKhach");
                LblFilter2.Text = "Khách hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("KhachHang");
            }
            else if (r.Contains("NHÀ CUNG CẤP") || r.Contains("NHA CUNG CAP") || r.Contains("NCC"))
            {
                LblFilter1.Text = "Nhóm NCC";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomNCC");
                LblFilter2.Text = "Nhà cung cấp";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhaCungCap");
            }
            else if (r.Contains("CHUYỂN KHO") || r.Contains("CHUYEN KHO"))
            {
                LblFilter1.Text = "Kho xuất";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");
                LblFilter2.Text = "Kho nhận";
                CboFilter2.ItemsSource = await GetLookupListAsync("KhoHang");
            }
            else if (r.Contains("KIỂM KÊ") || r.Contains("KIEM KE"))
            {
                LblFilter1.Text = "Kho hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");
                LblFilter2.Text = "Nhân viên";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
            }
            else if (r.Contains("ĐẶT HÀNG") || r.Contains("DAT HANG"))
            {
                LblFilter1.Text = "Khách hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhachHang");
                LblFilter2.Text = "Nhân viên";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
                if (PnlFilterSoPhieu != null) PnlFilterSoPhieu.Visibility = Visibility.Visible;
            }
            else if (r.Contains("LÃI GỘP") || r.Contains("BÁN CHẠY") || r.Contains("DANH MỤC") || r.Contains("MẶT HÀNG") || r.Contains("TỒN KHO") || r.Contains("XUẤT NHẬP TỒN") || r.Contains("THẺ KHO") || r.Contains("HSD") || r.Contains("HẠN DÙNG"))
            {
                LblFilter1.Text = "Kho hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");
                LblFilter2.Text = "Nhóm hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhomHang");
                LblFilter3.Text = "Mặt hàng";
                CboFilter3.ItemsSource = await GetLookupListAsync("MatHang");
                PnlFilter3.Visibility = Visibility.Visible;
            }
            else
            {
                LblFilter1.Text = "Cửa hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("CuaHang");
                LblFilter2.Text = "Nhân viên";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
            }

            CboFilter1.SelectedIndex = 0;
            CboFilter2.SelectedIndex = 0;
            CboFilter3.SelectedIndex = 0;
            CboFilter4.SelectedIndex = 0;
            if (CboFilter5 != null) CboFilter5.SelectedIndex = 0;
        }

        private void BuildReportTreeData()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    var all = conn.Query<SReportDbItem>(
                        "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name, CAST(PARENTID AS VARCHAR(50)) AS ParentId, COALESCE(ITEMTYPE, 0) as ItemType, COALESCE(SORTORDER, '') as SortOrder FROM SREPORT WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME").ToList();

                    if (all.Count > 0)
                    {
                        var standardRootNames = new List<string>
                        {
                            "BÁO CÁO QUỸ",
                            "BÁO CÁO DANH MỤC",
                            "BÁO CÁO BÁN HÀNG",
                            "BÁO CÁO ĐẶT HÀNG",
                            "BÁO CÁO KHO HÀNG",
                            "BÁO CÁO CÔNG NỢ",
                            "BÁO CÁO QUẢN TRỊ",
                            "BÁO CÁO BIỂU ĐỒ"
                        };

                        var rootDbItems = all.Where(x => string.IsNullOrEmpty(x.ParentId) && x.ItemType == 1).ToList();
                        rootDbItems = rootDbItems.OrderBy(x =>
                        {
                            int idx = standardRootNames.IndexOf(x.Name.Trim().ToUpper());
                            return idx >= 0 ? idx : 999;
                        }).ThenBy(x => x.SortOrder).ThenBy(x => x.Name).ToList();

                        _reportCategories = new List<ReportCategoryNode>();
                        foreach (var rItem in rootDbItems)
                        {
                            _reportCategories.Add(BuildCategoryNode(rItem, all));
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("BuildReportTreeData from DB error: " + ex.Message);
            }

            BuildFallbackReportTreeData();
        }

        private ReportCategoryNode BuildCategoryNode(SReportDbItem catItem, List<SReportDbItem> all)
        {
            var node = new ReportCategoryNode
            {
                Name = catItem.Name,
                ReportNames = all.Where(x => x.ParentId == catItem.Id && x.ItemType == 0)
                                 .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                                 .Select(x => x.Name).ToList(),
                SubCategories = all.Where(x => x.ParentId == catItem.Id && x.ItemType == 1)
                                   .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                                   .Select(c => BuildCategoryNode(c, all)).ToList()
            };
            return node;
        }

        private void BuildFallbackReportTreeData()
        {
            _reportCategories = new List<ReportCategoryNode>
            {
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO QUỸ",
                    ReportNames = new List<string>
                    {
                        "DANH SÁCH PHIẾU THU THEO NGÀY",
                        "DANH SÁCH PHIẾU THU THEO LÝ DO THU CHI",
                        "DANH SÁCH PHIẾU CHI THEO NGÀY",
                        "DANH SÁCH PHIẾU CHI THEO LÝ DO THU CHI",
                        "TỔNG HỢP THU CHI THEO NGÀY",
                        "TỔNG HỢP THU CHI THEO LÝ DO",
                        "BÁO CÁO TỒN QUỸ"
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO DANH MỤC",
                    ReportNames = new List<string>
                    {
                        "BÁO CÁO MẶT HÀNG",
                        "BÁO CÁO BÀN KHU VỰC",
                        "BÁO CÁO KHÁCH HÀNG",
                        "BÁO CÁO NHÀ CUNG CẤP",
                        "BÁO CÁO ĐỊNH LƯỢNG",
                        "BÁO CÁO KHUYẾN MẠI"
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO BÁN HÀNG",
                    ReportNames = new List<string>
                    {
                        "TỔNG HỢP BÁN HÀNG THEO NGÀY",
                        "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY",
                        "BÁO CÁO CHI TIẾT BÁN HÀNG THEO NGÀY",
                        "TỔNG HỢP DOANH THU THEO LOẠI ĐỒ",
                        "TỔNG HỢP DOANH THU CHƯA THANH TOÁN",
                        "BÁO CÁO BÁN HÀNG THEO NGÀY"
                    },
                    SubCategories = new List<ReportCategoryNode>
                    {
                        new ReportCategoryNode
                        {
                            Name = "THEO NHÂN VIÊN PHỤC VỤ",
                            ReportNames = new List<string>
                            {
                                "TỔNG HỢP BÁN THEO NHÂN VIÊN",
                                "BÁO CÁO BÁN HÀNG THEO NHÂN VIÊN",
                                "TỔNG HỢP MẶT HÀNG BÁN THEO NHÂN VIÊN"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "THEO KHU VỰC",
                            ReportNames = new List<string>
                            {
                                "TỔNG HỢP MẶT HÀNG BÁN THEO KHU VỰC",
                                "DANH SÁCH HÓA ĐƠN THEO KHU VỰC"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "THEO THU NGÂN",
                            ReportNames = new List<string>
                            {
                                "TỔNG HỢP BÁN THEO THU NGÂN",
                                "BÁO CÁO BÁN HÀNG THEO THU NGÂN",
                                "TỔNG HỢP MẶT HÀNG BÁN THEO THU NGÂN"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "THEO KHÁCH HÀNG",
                            ReportNames = new List<string>
                            {
                                "TỔNG HỢP BÁN THEO KHÁCH HÀNG",
                                "TỔNG HỢP MẶT HÀNG BÁN THEO KHÁCH HÀNG"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO BÁN HÀNG KHÁC",
                            ReportNames = new List<string>
                            {
                                "TỔNG HỢP BÁN HÀNG THEO NGÀY",
                                "BÁO CÁO BÁN HÀNG THEO NGÀY",
                                "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY",
                                "TỔNG HỢP HOA HỒNG THEO NVKD",
                                "CHI TIẾT BÁN HÀNG THEO HÓA ĐƠN",
                                "BÁO CÁO CHI TIẾT HÀNG KHUYẾN MẠI"
                            }
                        }
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO ĐẶT HÀNG",
                    ReportNames = new List<string>
                    {
                        "DANH SÁCH ĐẶT HÀNG THEO NGÀY",
                        "DANH SÁCH ĐẶT HÀNG THEO KHÁCH HÀNG",
                        "TỔNG HỢP ĐẶT HÀNG THEO NGÀY",
                        "TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG",
                        "TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY"
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO KHO HÀNG",
                    SubCategories = new List<ReportCategoryNode>
                    {
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO XUẤT BÁN HÀNG",
                            ReportNames = new List<string>
                            {
                                "TỔNG HỢP MẶT HÀNG XUẤT BÁN THEO NGÀY",
                                "BÁO CÁO MẶT HÀNG BÁN THEO ĐƠN HÀNG",
                                "BÁO CÁO XUẤT ĐỊNH LƯỢNG MẶT HÀNG THEO ĐƠN HÀNG"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO KIỂM KÊ",
                            ReportNames = new List<string>
                            {
                                "DANH SÁCH PHIẾU KIỂM KÊ THEO NGÀY",
                                "BÁO CÁO KIỂM KÊ KHO"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO XUẤT KHÁC",
                            ReportNames = new List<string>
                            {
                                "DANH SÁCH PHIẾU XUẤT KHÁC THEO NGÀY"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO NHẬP HÀNG",
                            ReportNames = new List<string>
                            {
                                "DANH SÁCH PHIẾU NHẬP KHO THEO NGÀY",
                                "DANH SÁCH PHIẾU NHẬP KHO THEO NHÀ CUNG CẤP",
                                "DANH SÁCH PHIẾU NHẬP KHO THEO NHÂN VIÊN",
                                "TỔNG HỢP MẶT HÀNG PHIẾU NHẬP THEO NHÀ CUNG CẤP",
                                "TỔNG HỢP MẶT HÀNG PHIẾU NHẬP THEO NHÂN VIÊN NHẬP",
                                "TỔNG HỢP NHẬP THEO NGÀY",
                                "TỔNG HỢP NHẬP THEO NHÀ CUNG CẤP",
                                "TỔNG HỢP NHẬP THEO NHÂN VIÊN NHẬP",
                                "TỔNG HỢP MẶT HÀNG NHẬP KHO THEO NGÀY"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO CHUYỂN KHO",
                            ReportNames = new List<string>
                            {
                                "DANH SÁCH PHIẾU CHUYỂN KHO THEO NGÀY",
                                "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NGÀY"
                            }
                        },
                        new ReportCategoryNode
                        {
                            Name = "BÁO CÁO THEO HSD",
                            ReportNames = new List<string>
                            {
                                "BÁO CÁO HÀNG HÓA THEO HẠN DÙNG",
                                "BÁO CÁO HÀNG HÓA ĐÃ HẾT HẠN DÙNG",
                                "BÁO CÁO HÀNG TỒN KHO CÓ HẠN DÙNG"
                            }
                        }
                    },
                    ReportNames = new List<string>
                    {
                        "BÁO CÁO THẺ KHO",
                        "TỔNG HỢP XUẤT NHẬP TỒN",
                        "BÁO CÁO TỒN KHO"
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO CÔNG NỢ",
                    ReportNames = new List<string>
                    {
                        "BÁO CÁO CÔNG NỢ KHÁCH HÀNG",
                        "TỔNG HỢP CÔNG NỢ KHÁCH HÀNG",
                        "ĐỐI CHIẾU CÔNG NỢ KHÁCH HÀNG",
                        "BÁO CÁO CÔNG NỢ NHÀ CUNG CẤP",
                        "TỔNG HỢP CÔNG NỢ NHÀ CUNG CẤP",
                        "ĐỐI CHIẾU CÔNG NỢ NHÀ CUNG CẤP"
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO QUẢN TRỊ",
                    ReportNames = new List<string>
                    {
                        "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG",
                        "CHI TIẾT LÃI THEO HÓA ĐƠN",
                        "BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT",
                        "PHÂN TÍCH TÌNH HÌNH BÁN HÀNG",
                        "BÁO CÁO BÁN HÀNG THEO GIỜ",
                        "DANH SÁCH MÓN XÓA GIẢM TRẢ LẠI"
                    }
                },
                new ReportCategoryNode
                {
                    Name = "BÁO CÁO BIỂU ĐỒ",
                    ReportNames = new List<string>
                    {
                        "BIỂU ĐỒ DOANH SỐ THEO NHÓM",
                        "BIỂU ĐỒ DOANH THU NGÀY TRONG THÁNG",
                        "BIỂU ĐỒ DOANH THU THÁNG TRONG NĂM",
                        "BIỂU ĐỒ THEO NHÂN VIÊN KINH DOANH"
                    }
                }
            };
        }

        private void RenderTreeView()
        {
            TvBaoCao.Items.Clear();

            foreach (var cat in _reportCategories)
            {
                var item = CreateTreeViewItem(cat);
                TvBaoCao.Items.Add(item);
            }

            var khoHangNode = _reportCategories.FirstOrDefault(c => c.Name == "BÁO CÁO KHO HÀNG");
            var nhapHangSub = khoHangNode?.SubCategories.FirstOrDefault(c => c.Name == "BÁO CÁO NHẬP HÀNG");
            if (nhapHangSub != null)
            {
                var allowedReports = nhapHangSub.ReportNames.Where(CanUseReport).ToList();
                LstSubReports.ItemsSource = allowedReports;
                LstSubReports.SelectedItem = allowedReports.Contains("DANH SÁCH PHIẾU NHẬP KHO THEO NGÀY")
                    ? "DANH SÁCH PHIẾU NHẬP KHO THEO NGÀY"
                    : allowedReports.FirstOrDefault();
            }
            else if (_reportCategories.Count > 0)
            {
                var allowedReports = _reportCategories[0].ReportNames.Where(CanUseReport).ToList();
                LstSubReports.ItemsSource = allowedReports;
                LstSubReports.SelectedItem = allowedReports.FirstOrDefault();
            }
        }

        private TreeViewItem CreateTreeViewItem(ReportCategoryNode cat)
        {
            var item = new TreeViewItem
            {
                Header = $"📁 {cat.Name}",
                Tag = cat,
                IsExpanded = true
            };

            foreach (var subCat in cat.SubCategories)
            {
                item.Items.Add(CreateTreeViewItem(subCat));
            }

            return item;
        }

        private void RenderThuongDung()
        {
            var favoriteReports = new List<string>
            {
                "DANH SÁCH PHIẾU THU THEO NGÀY",
                "DANH SÁCH PHIẾU CHI THEO NGÀY",
                "TỔNG HỢP BÁN HÀNG THEO NGÀY",
                "BÁO CÁO BÁN HÀNG THEO NGÀY",
                "BÁO CÁO TỒN KHO",
                "BÁO CÁO CÔNG NỢ KHÁCH HÀNG",
                "BÁO CÁO TỒN QUỸ"
            };

            LstThuongDung.ItemsSource = favoriteReports.Where(CanUseReport).ToList();
        }

        private void SelectReport(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName)) return;
            if (!CanUseReport(reportName)) return;
            _currentSelectedReport = reportName;
            TxtReportTitle.Text = reportName;
            LoadFiltersForReport(reportName);
            DgDuLieuTho.ItemsSource = null;
        }

        private void SelectInitialReport()
        {
            var defaultReport = "DANH SÁCH PHIẾU NHẬP KHO THEO NGÀY";
            if (CanUseReport(defaultReport))
            {
                SelectReport(defaultReport);
                return;
            }

            var firstAllowedReport = _searchableReports.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstAllowedReport))
            {
                SelectReport(firstAllowedReport);
            }
            else
            {
                _currentSelectedReport = "";
                TxtReportTitle.Text = "Không có báo cáo được phân quyền";
                DgDuLieuTho.ItemsSource = null;
            }
        }

        private void TvBaoCao_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TvBaoCao.SelectedItem is TreeViewItem item && item.Tag is ReportCategoryNode catNode)
            {
                var reports = catNode.ReportNames;
                var allowedReports = reports.Where(CanUseReport).ToList();
                LstSubReports.ItemsSource = allowedReports;
                if (allowedReports.Count > 0)
                {
                    LstSubReports.SelectedItem = allowedReports[0];
                    SelectReport(allowedReports[0]);
                }
                else
                {
                    LstSubReports.SelectedItem = null;
                }
            }
        }

        private void TvBaoCao_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TvBaoCao.SelectedItem is TreeViewItem item && item.Tag is string reportName)
            {
                OpenReport(reportName);
            }
        }

        private void LstSubReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstSubReports.SelectedItem is string reportName)
            {
                SelectReport(reportName);
            }
        }

        private void LstSubReports_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LstSubReports.SelectedItem is string reportName)
            {
                OpenReport(reportName);
            }
        }

        private void LstThuongDung_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstThuongDung.SelectedItem is string reportName)
            {
                SelectReport(reportName);
            }
        }

        private void LstThuongDung_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LstThuongDung.SelectedItem is string reportName)
            {
                OpenReport(reportName);
            }
        }

        private void TabLeftNav_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabLeftNav?.SelectedItem is TabItem tabItem && string.Equals(tabItem.Header?.ToString(), "Tìm kiếm", StringComparison.OrdinalIgnoreCase))
            {
                RefreshSearchResults();
            }
        }

        private void TxtSearchReport_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSearchResults();
        }

        private void RebuildSearchableReports()
        {
            var allReports = new List<string>();
            GetAllReportsRecursive(_reportCategories, allReports);
            _searchableReports = allReports
                .Where(CanUseReport)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r)
                .ToList();
        }

        private void RefreshSearchResults()
        {
            if (LstSearchResults == null) return;

            string search = NormalizeSearchText(TxtSearchReport?.Text);
            var results = string.IsNullOrWhiteSpace(search)
                ? _searchableReports
                : _searchableReports.Where(r => NormalizeSearchText(r).Contains(search)).ToList();

            LstSearchResults.ItemsSource = results;
        }

        private void GetAllReportsRecursive(List<ReportCategoryNode> cats, List<string> result)
        {
            foreach (var c in cats)
            {
                result.AddRange(c.ReportNames);
                GetAllReportsRecursive(c.SubCategories, result);
            }
        }

        private bool CanUseReport(string reportName)
        {
            return LocalPhanQuyenService.HasReportPermission(reportName);
        }

        private static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            string normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private void LstSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstSearchResults.SelectedItem is string reportName)
            {
                SelectReport(reportName);
            }
        }

        private void LstSearchResults_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LstSearchResults.SelectedItem is string reportName)
            {
                OpenReport(reportName);
            }
        }

        private void BtnHienThi_Click(object sender, RoutedEventArgs e)
        {
            var filterParams = BuildCurrentFilterParams();
            OpenReport(_currentSelectedReport, filterParams);
        }

        private async Task ApplyCustomFilterConfigAsync(List<QuanLyBar.Client.Services.ReportFilterConfigItem> filters)
        {
            bool hasDate = filters.Any(f => f.Name.Contains("NGAY", StringComparison.OrdinalIgnoreCase));
            if (PnlDateFilter != null) PnlDateFilter.Visibility = hasDate ? Visibility.Visible : Visibility.Collapsed;

            bool hasSoPhieu = filters.Any(f => f.Name.Equals("SOPHIEU", StringComparison.OrdinalIgnoreCase));
            if (PnlFilterSoPhieu != null) PnlFilterSoPhieu.Visibility = hasSoPhieu ? Visibility.Visible : Visibility.Collapsed;

            var nonDateFilters = filters
                .Where(f => !f.Name.Contains("NGAY", StringComparison.OrdinalIgnoreCase) && !f.Name.Equals("SOPHIEU", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.SortOrder)
                .ToList();

            var slotPanels = new[] { PnlFilter1, PnlFilter2, PnlFilter3, PnlFilter4, PnlFilter5 };
            var slotLabels = new[] { LblFilter1, LblFilter2, LblFilter3, LblFilter4, LblFilter5 };
            var slotCombos = new[] { CboFilter1, CboFilter2, CboFilter3, CboFilter4, CboFilter5 };

            for (int i = 0; i < slotPanels.Length; i++)
            {
                if (i < nonDateFilters.Count)
                {
                    var f = nonDateFilters[i];
                    var (label, lookupKey) = MapFilterCodeToLabelAndLookup(f.Name);
                    if (slotLabels[i] != null) slotLabels[i].Text = label;
                    if (slotCombos[i] != null)
                    {
                        var lookupData = await GetLookupListAsync(lookupKey);
                        slotCombos[i].ItemsSource = lookupData;
                        if (!string.IsNullOrWhiteSpace(f.Value) && f.Value != "Tất cả")
                        {
                            slotCombos[i].SelectedValue = f.Value;
                        }
                        else
                        {
                            slotCombos[i].SelectedIndex = 0;
                        }
                    }
                    if (slotPanels[i] != null) slotPanels[i].Visibility = Visibility.Visible;
                }
                else
                {
                    if (slotPanels[i] != null) slotPanels[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        private (string Label, string LookupKey) MapFilterCodeToLabelAndLookup(string code)
        {
            string c = code.ToUpper().Trim();
            if (c.Contains("DNHANVIENGIAO")) return ("Nhân viên giao hàng", "NhanVien");
            if (c.Contains("NHOMHIENTHI")) return ("Nhóm hiển thị", "NhomHienThi");
            if (c.Contains("THETRATRUOC")) return ("Thẻ trả trước", "TheTraTruoc");
            if (c.Contains("VOUCHER")) return ("Voucher", "Voucher");
            if (c.Contains("KHOXUAT")) return ("Kho xuất", "KhoHang");
            if (c.Contains("KHONHAP")) return ("Kho nhập", "KhoHang");
            if (c.Contains("KHOHANG")) return ("Kho hàng", "KhoHang");
            if (c.Contains("CUAHANG")) return ("Cửa hàng", "CuaHang");
            if (c.Contains("NHANVIENNHAP")) return ("Nhân viên nhập", "NhanVien");
            if (c.Contains("NHANVIENXUAT")) return ("Nhân viên xuất", "NhanVien");
            if (c.Contains("USERTHANHTOAN")) return ("Thu ngân", "NhanVien");
            if (c.Contains("NHANVIEN")) return ("Nhân viên", "NhanVien");
            if (c.Contains("NHOMKHACH")) return ("Nhóm khách", "NhomKhach");
            if (c.Contains("KHACHHANG")) return ("Khách hàng", "KhachHang");
            if (c.Contains("NHOMNHACUNGCAP") || c.Contains("NHOMNCC")) return ("Nhóm NCC", "NhomNCC");
            if (c.Contains("NHACUNGCAP")) return ("Nhà cung cấp", "NhaCungCap");
            if (c.Contains("LOAIMATHANG")) return ("Loại mặt hàng", "LoaiMatHang");
            if (c.Contains("NHOMMATHANG")) return ("Nhóm hàng", "NhomHang");
            if (c.Contains("MATHANG")) return ("Mặt hàng", "MatHang");
            if (c.Contains("HANGSANXUAT")) return ("Hãng sản xuất", "HangSanXuat");
            if (c.Contains("LYDOTHUCHI")) return ("Lý do thu chi", "LyDoThuChi");
            if (c.Contains("TAIKHOANNGANHANG")) return ("Tài khoản NH", "TaiKhoanNganHang");
            if (c.Contains("KHUVUC")) return ("Khu vực", "KhuVuc");
            if (c.Contains("BAN")) return ("Bàn / Phòng", "Ban");
            if (c.Contains("GROUPUSER")) return ("Nhóm người dùng", "NhomNguoiDung");

            return (ReportTextLocalizationService.GetParameterLabel(code), "KhoHang");
        }

        private QuanLyBar.Client.Models.ReportFilterParams BuildCurrentFilterParams()
        {
            var p = new QuanLyBar.Client.Models.ReportFilterParams
            {
                ReportName = _currentSelectedReport,
                TuNgay = (PnlDateFilter?.Visibility == Visibility.Visible) ? DpTuNgay?.SelectedDate : null,
                DenNgay = (PnlDateFilter?.Visibility == Visibility.Visible) ? DpDenNgay?.SelectedDate : null,
                SoPhieu = (PnlFilterSoPhieu?.Visibility == Visibility.Visible) ? TxtSoPhieu?.Text?.Trim() : null
            };

            if (PnlFilter1?.Visibility == Visibility.Visible) AssignFilterByLabel(LblFilter1?.Text, CboFilter1?.SelectedValue?.ToString(), p);
            if (PnlFilter2?.Visibility == Visibility.Visible) AssignFilterByLabel(LblFilter2?.Text, CboFilter2?.SelectedValue?.ToString(), p);
            if (PnlFilter3?.Visibility == Visibility.Visible) AssignFilterByLabel(LblFilter3?.Text, CboFilter3?.SelectedValue?.ToString(), p);
            if (PnlFilter4?.Visibility == Visibility.Visible) AssignFilterByLabel(LblFilter4?.Text, CboFilter4?.SelectedValue?.ToString(), p);
            if (PnlFilter5?.Visibility == Visibility.Visible) AssignFilterByLabel(LblFilter5?.Text, CboFilter5?.SelectedValue?.ToString(), p);

            return p;
        }

        private void AssignFilterByLabel(string? lbl, string? val, QuanLyBar.Client.Models.ReportFilterParams p)
        {
            if (string.IsNullOrWhiteSpace(lbl) || string.IsNullOrWhiteSpace(val)) return;
            string l = lbl.Trim().ToLower();
            if (l.Contains("xuất") || l.Contains("kho xuất")) p.KhoXuatId = val;
            else if (l.Contains("nhập") && l.Contains("kho")) p.KhoNhapId = val;
            else if (l.Contains("kho")) p.KhoId = val;
            else if (l.Contains("cửa hàng")) p.CuaHangId = val;
            else if (l.Contains("nhân viên")) p.NhanVienId = val;
            else if (l.Contains("khách")) p.KhachHangId = val;
            else if (l.Contains("nhà cung cấp") || l.Contains("ncc")) p.NhaCungCapId = val;
            else if (l.Contains("nhóm hàng")) p.NhomHangId = val;
            else if (l.Contains("mặt hàng")) p.MatHangId = val;
            else if (l.Contains("lý do")) p.LyDoId = val;
        }

        private async void OpenReport(string reportName, QuanLyBar.Client.Models.ReportFilterParams? filterParams = null)
        {
            if (string.IsNullOrWhiteSpace(reportName)) return;
            if (!CanUseReport(reportName))
            {
                MessageBox.Show(
                    "Bạn không có quyền xem báo cáo này.",
                    "Phân quyền báo cáo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (Window.GetWindow(this) is MainAppWindow mainWin)
            {
                await mainWin.OpenTabByNameAsync(reportName, filterParams);
            }
        }

        private void BtnThietKeMau_Click(object sender, RoutedEventArgs e)
        {
            var win = new QuanLyBar.Client.Views.InAn.ThietKeMauInWindow(
                _currentSelectedReport,
                DgDuLieuTho,
                onSaveCallback: (columns) =>
                {
                    ApplyColumnConfigToDataGrid(columns);
                });

            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void ApplyColumnConfigToDataGrid(List<QuanLyBar.Client.Views.InAn.MauInCotModel> columns)
        {
            if (columns == null || columns.Count == 0 || DgDuLieuTho.Columns.Count == 0) return;

            foreach (var col in DgDuLieuTho.Columns)
            {
                string header = col.Header?.ToString() ?? "";
                var cfg = columns.FirstOrDefault(c => c.Cot == header || c.TieuDe == header || c.DataField == header);
                if (cfg != null)
                {
                    col.Visibility = cfg.HienThi ? Visibility.Visible : Visibility.Collapsed;
                    if (!string.IsNullOrWhiteSpace(cfg.TieuDe))
                    {
                        col.Header = cfg.TieuDe;
                    }
                }
            }
        }

        private void BtnTuyChonThamSo_Click(object sender, RoutedEventArgs e)
        {
            var win = new QuanLyBar.Client.Views.InAn.TuyChonThamSoBaoCaoWindow(
                _currentSelectedReport,
                onSavedCallback: () =>
                {
                    LoadFiltersForReport(_currentSelectedReport);
                });
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private async void BtnXemDuLieuTho_Click(object sender, RoutedEventArgs e)
        {
            await LoadDuLieuThoAsync();
        }

        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tính năng xuất Excel dữ liệu thô đang được xử lý và sẽ sẵn sàng trong bản cập nhật tiếp theo.",
                "Xuất excel dữ liệu thô",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void DgDuLieuTho_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            int index = e.Row.GetIndex() + 1;
            bool isSelected = e.Row.IsSelected;
            e.Row.Header = isSelected ? $"► {index:D2}" : $"{index:D2}";
        }

        private void DgDuLieuTho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            for (int i = 0; i < DgDuLieuTho.Items.Count; i++)
            {
                if (DgDuLieuTho.ItemContainerGenerator.ContainerFromIndex(i) is DataGridRow row)
                {
                    int index = i + 1;
                    row.Header = row.IsSelected ? $"► {index:D2}" : $"{index:D2}";
                }
            }
        }

        private void DgDuLieuTho_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "GhiChu": case "NOTE": e.Column.Header = "Ghi chú"; break;
                case "SoPhieu": case "SOPHIEU": e.Column.Header = "Số phiếu"; break;
                case "Ngay": case "NGAY": e.Column.Header = "Ngày"; break;
                case "TongCong": case "TongCongBan": case "TONGCONG": e.Column.Header = "Tổng cộng"; break;
                case "PhiVanChuyen": e.Column.Header = "Phí vận chuyển"; break;
                case "TienGiamGia": case "GiamGia": e.Column.Header = "Tiền giảm giá"; break;
                case "TiLeGiamGia": case "CkPhanTram": e.Column.Header = "Tỉ lệ giảm giá (%)"; break;
                case "TienThue": e.Column.Header = "Tiền thuế"; break;
                case "TiLeThue": e.Column.Header = "Tỉ lệ thuế (%)"; break;
                case "TienHang": case "TIENHANG": e.Column.Header = "Tiền hàng"; break;
                case "NhaCungCap": case "TenNCC": e.Column.Header = "Nhà cung cấp"; break;
                case "MaNCC": e.Column.Header = "Mã NCC"; break;
                case "KhoNhap": e.Column.Header = "Kho nhập"; break;
                case "KhoXuat": e.Column.Header = "Kho xuất"; break;
                case "KhoNhan": e.Column.Header = "Kho nhận"; break;
                case "KhoHang": e.Column.Header = "Kho hàng"; break;
                case "SoHd": case "SoHoaDon": case "SOHD": e.Column.Header = "Số hóa đơn"; break;
                case "BanKhuVuc": case "KhuVuc": e.Column.Header = "Khu vực"; break;
                case "BanPhong": case "TenBan": e.Column.Header = "Bàn / Phòng"; break;
                case "MaBan": e.Column.Header = "Mã bàn"; break;
                case "ThuNgan": e.Column.Header = "Thu ngân"; break;
                case "NhanVien": case "NhanVienBan": e.Column.Header = "Nhân viên"; break;
                case "NhanVienNhap": e.Column.Header = "Nhân viên nhập"; break;
                case "NhanVienPhucVu": e.Column.Header = "Nhân viên phục vụ"; break;
                case "NhanVienKiemKe": e.Column.Header = "Nhân viên kiểm kê"; break;
                case "KhachHang": case "TenKhach": case "TenKhachHang": e.Column.Header = "Khách hàng"; break;
                case "MaKhach": e.Column.Header = "Mã khách"; break;
                case "ThanhToan": e.Column.Header = "Thanh toán"; break;
                case "TienMat": e.Column.Header = "Tiền mặt"; break;
                case "ChuyenKhoan": e.Column.Header = "Chuyển khoản"; break;
                case "The": e.Column.Header = "Thẻ"; break;
                case "TheTt": e.Column.Header = "Thẻ TT"; break;
                case "LyDo": case "LyDoThuChi": e.Column.Header = "Lý do thu chi"; break;
                case "LyDoThu": e.Column.Header = "Lý do thu"; break;
                case "LyDoChi": e.Column.Header = "Lý do chi"; break;
                case "LyDoHuy": e.Column.Header = "Lý do hủy"; break;
                case "NguoiNop": e.Column.Header = "Người nộp"; break;
                case "NguoiNhan": e.Column.Header = "Người nhận"; break;
                case "SoTien": e.Column.Header = "Số tiền"; break;
                case "SoTienThu": case "THU": e.Column.Header = "Số tiền thu"; break;
                case "SoTienChi": case "CHI": e.Column.Header = "Số tiền chi"; break;
                case "CuaHang": e.Column.Header = "Cửa hàng"; break;
                case "MaHang": case "MAHANG": e.Column.Header = "Mã hàng"; break;
                case "MatHang": case "TenHang": case "TenMatHang": case "MATHANG": e.Column.Header = "Mặt hàng"; break;
                case "NguyenLieu": e.Column.Header = "Nguyên liệu"; break;
                case "DVT": case "Dvt": e.Column.Header = "ĐVT"; break;
                case "SoLuong": case "SoLuongMon": case "SOLUONG": e.Column.Header = "Số lượng"; break;
                case "SoLuongNhap": e.Column.Header = "Số lượng nhập"; break;
                case "SoLuongXuat": e.Column.Header = "Số lượng xuất"; break;
                case "SoLuongDon": e.Column.Header = "Số đơn"; break;
                case "DoiTra": e.Column.Header = "Đổi trả"; break;
                case "DonGia": case "DonGiaBan": case "DONGIA": e.Column.Header = "Đơn giá"; break;
                case "DonGiaVon": e.Column.Header = "Đơn giá vốn"; break;
                case "ThanhTien": case "ThanhTienBan": case "THANHTIEN": e.Column.Header = "Thành tiền"; break;
                case "GiaTriVon": case "TienVon": e.Column.Header = "Tiền vốn"; break;
                case "LaiGop": case "LoiNhuan": e.Column.Header = "Lãi gộp"; break;
                case "TiLeLai": e.Column.Header = "Tỉ lệ lãi (%)"; break;
                case "DoanhThu": case "TongDoanhSo": case "DoanhSo": e.Column.Header = "Doanh thu"; break;
                case "TonKho": e.Column.Header = "Tồn kho"; break;
                case "TonDau": case "NoDau": case "NoDauKy": case "CongNoDau": e.Column.Header = "Số dư đầu"; break;
                case "TonCuoi": case "NoCuoi": case "NoCuoiKy": case "CongNoCuoi": e.Column.Header = "Số dư cuối"; break;
                case "NhapTrongKy": case "Mua": case "PhatSinhMua": case "GhiNo": e.Column.Header = "Phát sinh tăng"; break;
                case "XuatTrongKy": case "GhiCo": e.Column.Header = "Phát sinh giảm"; break;
                case "SoDu": case "CongNo": case "TongNo": e.Column.Header = "Công nợ"; break;
                case "TonSoSach": e.Column.Header = "Tồn sổ sách"; break;
                case "TonThucTe": e.Column.Header = "Tồn thực tế"; break;
                case "ChenhLech": e.Column.Header = "Chênh lệch"; break;
                case "TienTon": e.Column.Header = "Giá trị tồn"; break;
                case "GiaMua": e.Column.Header = "Giá mua"; break;
                case "GiaBan": e.Column.Header = "Giá bán"; break;
                case "NhomHang": case "TenNhom": case "TenNhomHang": e.Column.Header = "Nhóm hàng"; break;
                case "NhomKhach": e.Column.Header = "Nhóm khách"; break;
                case "NhomNCC": e.Column.Header = "Nhóm NCC"; break;
                case "DienThoai": e.Column.Header = "Điện thoại"; break;
                case "DiaChi": e.Column.Header = "Địa chỉ"; break;
                case "Email": e.Column.Header = "Email"; break;
                case "HanDung": e.Column.Header = "Hạn dùng"; break;
                case "SoNgayConLai": e.Column.Header = "Số ngày còn lại"; break;
                case "KhungGio": case "GioKhachVao": e.Column.Header = "Khung giờ"; break;
                case "Gio": e.Column.Header = "Giờ"; break;
                case "BatDau": e.Column.Header = "Bắt đầu"; break;
                case "KetThuc": e.Column.Header = "Kết thúc"; break;
                case "TenDoiTuong": e.Column.Header = "Tên đối tượng"; break;
                case "ChungTuGoc": e.Column.Header = "Chứng từ gốc"; break;
                case "DienGiai": case "DIENGIAI": e.Column.Header = "Diễn giải"; break;
                case "TAOBOI": e.Column.Header = "Tạo bởi"; break;
                case "THOIGIAN": e.Column.Header = "Thời gian"; break;
                case "DoAn": e.Column.Header = "Đồ ăn"; break;
                case "DoUong": e.Column.Header = "Đồ uống"; break;
                case "DichVu": e.Column.Header = "Dịch vụ"; break;
                case "DoKhac": e.Column.Header = "Đồ khác"; break;
                case "HoaHong": e.Column.Header = "Hoa hồng"; break;
                case "TaiKhoan": e.Column.Header = "Tài khoản"; break;
                case "ThietBi": e.Column.Header = "Thiết bị"; break;
                case "ThaoTac": e.Column.Header = "Thao tác"; break;
                case "TrangThai": e.Column.Header = "Trạng thái"; break;
                case "TyLe": case "TiLePhanTram": e.Column.Header = "Tỷ lệ (%)"; break;
                case "Thang": e.Column.Header = "Tháng"; break;
                case "Nam": e.Column.Header = "Năm"; break;
                case "MaKM": e.Column.Header = "Mã KM"; break;
                case "TenKM": e.Column.Header = "Tên khuyến mại"; break;
                case "GiaTriKM": e.Column.Header = "Giá trị KM"; break;
                case "DinhLuong": e.Column.Header = "Định lượng"; break;
                case "HangSanXuat": e.Column.Header = "Hãng sản xuất"; break;
                case "NgaySinh": e.Column.Header = "Ngày sinh"; break;
                case "NhomNguoiDung": e.Column.Header = "Nhóm người dùng"; break;
                case "TenNguoiDung": e.Column.Header = "Tên người dùng"; break;
                case "TenDangNhap": e.Column.Header = "Tên đăng nhập"; break;
            }

            string localizedHeader = ReportTextLocalizationService.GetColumnLabel(e.PropertyName);
            if (!string.IsNullOrWhiteSpace(localizedHeader))
                e.Column.Header = localizedHeader;

            if (e.PropertyName == "THOIGIAN" && e.Column is DataGridTextColumn colTg)
            {
                colTg.Binding.StringFormat = "M/d/yyyy h:mm tt";
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                colTg.ElementStyle = style;
            }
            else if (e.PropertyType == typeof(DateTime) || e.PropertyType == typeof(DateTime?))
            {
                if (e.Column is DataGridTextColumn col)
                {
                    col.Binding.StringFormat = "dd/MM/yyyy";
                    var style = new Style(typeof(TextBlock));
                    style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                    col.ElementStyle = style;
                }
            }
            else if (e.PropertyType == typeof(decimal) || e.PropertyType == typeof(decimal?) ||
                     e.PropertyType == typeof(int) || e.PropertyType == typeof(int?) ||
                     e.PropertyType == typeof(double) || e.PropertyType == typeof(double?) ||
                     e.PropertyType == typeof(long) || e.PropertyType == typeof(long?))
            {
                if (e.Column is DataGridTextColumn col)
                {
                    col.Binding.StringFormat = "{0:#,##0.##}";
                    var style = new Style(typeof(TextBlock));
                    style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                    col.ElementStyle = style;
                }
            }
        }

        private async Task LoadDuLieuThoAsync()
        {
            try
            {
                DateTime? tuNgay = DpTuNgay.SelectedDate?.Date;
                DateTime? denNgay = DpDenNgay.SelectedDate?.Date;
                string filter1 = CboFilter1.SelectedValue?.ToString() ?? "";
                string filter2 = CboFilter2.SelectedValue?.ToString() ?? "";
                string filter3 = CboFilter3.SelectedValue?.ToString() ?? "";
                string filter4 = CboFilter4.SelectedValue?.ToString() ?? "";
                string filter5 = CboFilter5?.SelectedValue?.ToString() ?? "";
                string soPhieu = TxtSoPhieu.Text?.Trim() ?? "";

                string r = (_currentSelectedReport ?? "").Trim().ToUpper();

                using var conn = DbConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                DataTable dt = new DataTable();

                // -------------------------------------------------------------
                // 1. BÁO CÁO QUỸ
                // -------------------------------------------------------------
                if (r == "BÁO CÁO TỒN QUỸ" || r.Contains("TỒN QUỸ") || r.Contains("TON QUY"))
                {
                    string sql = @"
                        SELECT 
                            CAST(t.NGAY AS DATE) AS NGAY,
                            COALESCE(t.NAME, '') AS SOPHIEU,
                            COALESCE(t.DIENGIAI, '') AS DIENGIAI,
                            t.TIMECREATED AS THOIGIAN,
                            'Administrator' AS TAOBOI,
                            CAST(COALESCE(t.THU, 0) AS DECIMAL(18,0)) AS THU,
                            CAST(COALESCE(t.CHI, 0) AS DECIMAL(18,0)) AS CHI
                        FROM TTHUCHI t
                        WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(t.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(t.NGAY AS DATE) <= @DenNgay)
                        ORDER BY t.NGAY, t.TIMECREATED, t.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("TỔNG HỢP THU CHI THEO NGÀY") || r.Contains("TONG HOP THU CHI THEO NGAY"))
                {
                    string sql = @"
                        SELECT 
                            CAST(t.NGAY AS DATE) AS Ngay,
                            CAST(COALESCE(SUM(t.THU), 0) AS DECIMAL(18,0)) AS SoTienThu,
                            CAST(COALESCE(SUM(t.CHI), 0) AS DECIMAL(18,0)) AS SoTienChi
                        FROM TTHUCHI t
                        WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(t.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(t.NGAY AS DATE) <= @DenNgay)
                        GROUP BY CAST(t.NGAY AS DATE)
                        ORDER BY CAST(t.NGAY AS DATE)";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("TỔNG HỢP THU CHI THEO LÝ DO") || r.Contains("TONG HOP THU CHI THEO LY DO"))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(ld.NAME, 'Khác') AS LyDoThuChi,
                            CAST(COALESCE(SUM(t.THU), 0) AS DECIMAL(18,0)) AS SoTienThu,
                            CAST(COALESCE(SUM(t.CHI), 0) AS DECIMAL(18,0)) AS SoTienChi
                        FROM TTHUCHI t
                        LEFT JOIN DLYDOTHUCHI ld ON CAST(t.DLYDOTHUCHIID AS VARCHAR(50)) = CAST(ld.ID AS VARCHAR(50))
                        WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(t.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(t.NGAY AS DATE) <= @DenNgay)
                        GROUP BY ld.NAME
                        ORDER BY ld.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("PHIẾU CHI") || r.Contains("PHIEU CHI"))
                {
                    string sql = @"
                        SELECT 
                            t.NAME AS SoPhieu,
                            CAST(t.NGAY AS DATE) AS Ngay,
                            COALESCE(t.TENDOITUONG, '') AS TenDoiTuong,
                            COALESCE(t.DIACHI, '') AS DiaChi,
                            COALESCE(ld.NAME, '') AS LyDoThuChi,
                            COALESCE(t.DIENGIAI, '') AS DienGiai,
                            COALESCE(t.CHUNGTUGOC, '') AS ChungTuGoc,
                            CAST(COALESCE(t.CHI, 0) AS DECIMAL(18,0)) AS SoTienChi
                        FROM TTHUCHI t
                        LEFT JOIN DLYDOTHUCHI ld ON CAST(t.DLYDOTHUCHIID AS VARCHAR(50)) = CAST(ld.ID AS VARCHAR(50))
                        WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                          AND t.CHI > 0
                          AND (@TuNgay IS NULL OR CAST(t.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(t.NGAY AS DATE) <= @DenNgay)
                          AND (CAST(@LyDoId AS VARCHAR(255)) IN ('', '0') OR CAST(t.DLYDOTHUCHIID AS VARCHAR(50)) = @LyDoId)
                        ORDER BY t.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { 
                        TuNgay = (object?)tuNgay ?? DBNull.Value, 
                        DenNgay = (object?)denNgay ?? DBNull.Value,
                        LyDoId = filter1
                    });
                    dt.Load(reader);
                }
                else if (r.Contains("PHIẾU THU") || r.Contains("PHIEU THU") || r.Contains("THU CHI"))
                {
                    string sql = @"
                        SELECT 
                            t.NAME AS SoPhieu,
                            CAST(t.NGAY AS DATE) AS Ngay,
                            COALESCE(t.TENDOITUONG, '') AS TenDoiTuong,
                            COALESCE(t.DIACHI, '') AS DiaChi,
                            COALESCE(ld.NAME, '') AS LyDoThuChi,
                            COALESCE(t.DIENGIAI, '') AS DienGiai,
                            COALESCE(t.CHUNGTUGOC, '') AS ChungTuGoc,
                            CAST(COALESCE(t.THU, 0) AS DECIMAL(18,0)) AS SoTienThu
                        FROM TTHUCHI t
                        LEFT JOIN DLYDOTHUCHI ld ON CAST(t.DLYDOTHUCHIID AS VARCHAR(50)) = CAST(ld.ID AS VARCHAR(50))
                        WHERE (t.STATUS IS NULL OR t.STATUS <> 0)
                          AND t.THU > 0
                          AND (@TuNgay IS NULL OR CAST(t.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(t.NGAY AS DATE) <= @DenNgay)
                          AND (CAST(@LyDoId AS VARCHAR(255)) IN ('', '0') OR CAST(t.DLYDOTHUCHIID AS VARCHAR(50)) = @LyDoId)
                        ORDER BY t.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { 
                        TuNgay = (object?)tuNgay ?? DBNull.Value, 
                        DenNgay = (object?)denNgay ?? DBNull.Value,
                        LyDoId = filter1
                    });
                    dt.Load(reader);
                }

                // -------------------------------------------------------------
                // 2. BÁO CÁO NHẬP HÀNG (KHO HÀNG)
                // -------------------------------------------------------------
                else if ((r.Contains("NHẬP") || r.Contains("NHAP")) && (r.Contains("MẶT HÀNG") || r.Contains("MAT HANG")))
                {
                    string leadCol = "COALESCE(nv.NAME, '') AS NhanVienNhap";
                    string orderCol = "COALESCE(nv.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";
                    if (r.Contains("NHÀ CUNG CẤP") || r.Contains("NHA CUNG CAP") || r.Contains("NCC"))
                    {
                        leadCol = "COALESCE(ncc.NAME, '') AS NhaCungCap";
                        orderCol = "COALESCE(ncc.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";
                    }
                    else if (r.Contains("NGÀY") || r.Contains("NGAY"))
                    {
                        leadCol = "CAST(d.NGAY AS DATE) AS Ngay";
                        orderCol = "d.NGAY DESC, COALESCE(c.TENHANG, m.NAME, '')";
                    }

                    string sql = $@"
                        SELECT 
                            {leadCol},
                            COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                            COALESCE(m.CODE, '') AS MaHang,
                            CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) AS ThanhTien,
                            CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) AS DonGia,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) AS SoLuongNhap
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        WHERE d.LOAI = 1
                          AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                          AND (CAST(@NccId AS VARCHAR(255)) IN ('', '0') OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                          AND (CAST(@KhoId AS VARCHAR(255)) IN ('', '0') OR CAST(d.DKHONHAPID AS VARCHAR(50)) = @KhoId)
                          AND (CAST(@NvId AS VARCHAR(255)) IN ('', '0') OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                          AND (CAST(@NhomHangId AS VARCHAR(255)) IN ('', '0') OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomHangId)
                          AND (CAST(@MatHangId AS VARCHAR(255)) IN ('', '0') OR CAST(c.DMATHANGID AS VARCHAR(50)) = @MatHangId)
                        ORDER BY {orderCol}";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { 
                        TuNgay = (object?)tuNgay ?? DBNull.Value, 
                        DenNgay = (object?)denNgay ?? DBNull.Value, 
                        NccId = filter1, 
                        KhoId = filter2, 
                        NvId = filter3, 
                        NhomHangId = filter4, 
                        MatHangId = filter5 
                    });
                    dt.Load(reader);
                }
                else if (r.Contains("TỔNG HỢP NHẬP THEO") || r.Contains("TONG HOP NHAP THEO"))
                {
                    string groupCol = "CAST(d.NGAY AS DATE)";
                    string groupName = "CAST(d.NGAY AS DATE) AS Ngay";
                    if (r.Contains("NHÀ CUNG CẤP") || r.Contains("NCC"))
                    {
                        groupCol = "ncc.NAME";
                        groupName = "COALESCE(ncc.NAME, 'Chưa xác định') AS NhaCungCap";
                    }
                    else if (r.Contains("NHÂN VIÊN") || r.Contains("NV"))
                    {
                        groupCol = "nv.NAME";
                        groupName = "COALESCE(nv.NAME, 'Chưa xác định') AS NhanVienNhap";
                    }

                    string sql = $@"
                        SELECT 
                            {groupName},
                            COUNT(d.ID) AS SoLuongDon,
                            CAST(COALESCE(SUM(d.TIENHANG), 0) AS DECIMAL(18,0)) AS TienHang,
                            CAST(COALESCE(SUM(d.TIENGIAMGIA), 0) AS DECIMAL(18,0)) AS TienGiamGia,
                            CAST(COALESCE(SUM(d.TIENTHUE), 0) AS DECIMAL(18,0)) AS TienThue,
                            CAST(COALESCE(SUM(d.PHIVANCHUYEN), 0) AS DECIMAL(18,0)) AS PhiVanChuyen,
                            CAST(COALESCE(SUM(d.TONGCONG), 0) AS DECIMAL(18,0)) AS TongCong
                        FROM TDONHANG d
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE d.LOAI = 1
                          AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY {groupCol}
                        ORDER BY {groupCol}";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("NHẬP KHO") || r.Contains("NHAP KHO") || (r.Contains("DANH SÁCH") && r.Contains("NHẬP")))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(d.NOTE, '') AS GhiChu,
                            d.NAME AS SoPhieu,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong,
                            CAST(COALESCE(d.PHIVANCHUYEN, 0) AS DECIMAL(18,0)) AS PhiVanChuyen,
                            CAST(COALESCE(d.TIENGIAMGIA, 0) AS DECIMAL(18,0)) AS TienGiamGia,
                            CAST(COALESCE(d.TILEGIAMGIA, 0) AS DECIMAL(18,0)) AS TiLeGiamGia,
                            CAST(COALESCE(d.TIENTHUE, 0) AS DECIMAL(18,0)) AS TienThue,
                            CAST(COALESCE(d.TILETHUE, 0) AS DECIMAL(18,0)) AS TiLeThue,
                            CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,0)) AS TienHang,
                            COALESCE(ncc.NAME, '') AS NhaCungCap,
                            COALESCE(k.NAME, '') AS KhoNhap
                        FROM TDONHANG d
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE d.LOAI = 1
                          AND (d.STATUS IS NULL OR d.STATUS = 30 OR d.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                          AND (CAST(@NccId AS VARCHAR(255)) IN ('', '0') OR CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = @NccId)
                          AND (CAST(@NvId AS VARCHAR(255)) IN ('', '0') OR CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = @NvId)
                          AND (CAST(@SoPhieu AS VARCHAR(255)) = '' OR UPPER(d.NAME) LIKE '%' || @SoPhieu || '%')
                        ORDER BY d.NGAY DESC, d.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { 
                        TuNgay = (object?)tuNgay ?? DBNull.Value, 
                        DenNgay = (object?)denNgay ?? DBNull.Value, 
                        NccId = filter1, 
                        NvId = filter2, 
                        SoPhieu = soPhieu.ToUpper() 
                    });
                    dt.Load(reader);
                }

                // -------------------------------------------------------------
                // 3. BÁO CÁO KIỂM KÊ, CHUYỂN KHO, XUẤT KHÁC
                // -------------------------------------------------------------
                else if (r.Contains("KIỂM KÊ") || r.Contains("KIEM KE"))
                {
                    if (r.Contains("CHI TIẾT") || r.Contains("KHO"))
                    {
                        string sql = @"
                            SELECT 
                                d.NAME AS SoPhieu,
                                CAST(d.NGAY AS DATE) AS Ngay,
                                COALESCE(k.NAME, '') AS KhoHang,
                                COALESCE(m.CODE, '') AS MaHang,
                                COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                                COALESCE(dvt.NAME, '') AS DVT,
                                CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) AS TonThucTe,
                                CAST(COALESCE(m.SOLUONGTON, 0) AS DECIMAL(18,2)) AS TonSoSach,
                                CAST(COALESCE(c.SOLUONG, 0) - COALESCE(m.SOLUONGTON, 0) AS DECIMAL(18,2)) AS ChenhLech
                            FROM TDONHANGCHITIET c
                            JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                            LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                            WHERE d.LOAI = 3
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            ORDER BY d.NGAY DESC, d.NAME";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                    else
                    {
                        string sql = @"
                            SELECT 
                                d.NAME AS SoPhieu,
                                CAST(d.NGAY AS DATE) AS Ngay,
                                COALESCE(k.NAME, '') AS KhoHang,
                                COALESCE(nv.NAME, '') AS NhanVienKiemKe,
                                COALESCE(d.NOTE, '') AS GhiChu
                            FROM TDONHANG d
                            LEFT JOIN DKHOHANG k ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(k.ID AS VARCHAR(50))
                            LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENNHAPID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                            WHERE d.LOAI = 3
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            ORDER BY d.NGAY DESC, d.NAME";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                }
                else if (r.Contains("CHUYỂN KHO") || r.Contains("CHUYEN KHO"))
                {
                    if (r.Contains("MẶT HÀNG") || r.Contains("MAT HANG"))
                    {
                        string sql = @"
                            SELECT 
                                CAST(d.NGAY AS DATE) AS Ngay,
                                COALESCE(kx.NAME, '') AS KhoXuat,
                                COALESCE(kn.NAME, '') AS KhoNhan,
                                COALESCE(m.CODE, '') AS MaHang,
                                COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                                COALESCE(dvt.NAME, '') AS DVT,
                                CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) AS SoLuong,
                                CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) AS ThanhTien
                            FROM TDONHANGCHITIET c
                            JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                            LEFT JOIN DKHOHANG kx ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(kx.ID AS VARCHAR(50))
                            LEFT JOIN DKHOHANG kn ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(kn.ID AS VARCHAR(50))
                            WHERE d.LOAI = 5
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            ORDER BY d.NGAY DESC, d.NAME";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                    else
                    {
                        string sql = @"
                            SELECT 
                                d.NAME AS SoPhieu,
                                CAST(d.NGAY AS DATE) AS Ngay,
                                COALESCE(kx.NAME, '') AS KhoXuat,
                                COALESCE(kn.NAME, '') AS KhoNhan,
                                COALESCE(d.NOTE, '') AS GhiChu,
                                CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong
                            FROM TDONHANG d
                            LEFT JOIN DKHOHANG kx ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(kx.ID AS VARCHAR(50))
                            LEFT JOIN DKHOHANG kn ON CAST(d.DKHONHAPID AS VARCHAR(50)) = CAST(kn.ID AS VARCHAR(50))
                            WHERE d.LOAI = 5
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            ORDER BY d.NGAY DESC, d.NAME";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                }
                else if (r.Contains("XUẤT KHÁC") || r.Contains("XUAT KHAC"))
                {
                    string sql = @"
                        SELECT 
                            d.NAME AS SoPhieu,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            COALESCE(kx.NAME, '') AS KhoXuat,
                            COALESCE(nv.NAME, '') AS NhanVien,
                            CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong,
                            COALESCE(d.NOTE, '') AS GhiChu
                        FROM TDONHANG d
                        LEFT JOIN DKHOHANG kx ON CAST(d.DKHOXUATID AS VARCHAR(50)) = CAST(kx.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE d.LOAI = 4
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        ORDER BY d.NGAY DESC, d.NAME";
                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("HSD") || r.Contains("HẠN DÙNG") || r.Contains("HAN DUNG"))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(m.CODE, '') AS MaHang,
                            m.NAME AS MatHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            COALESCE(nh.NAME, '') AS NhomHang,
                            CAST(COALESCE(m.SOLUONGTON, 0) AS DECIMAL(18,2)) AS TonKho,
                            CAST(m.HANDUNG AS DATE) AS HanDung,
                            DATEDIFF(day, CURRENT_DATE, m.HANDUNG) AS SoNgayConLai
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                        ORDER BY m.HANDUNG ASC";
                    using var reader = await conn.ExecuteReaderAsync(sql);
                    dt.Load(reader);
                }
                else if (r.Contains("TỒN KHO") || r.Contains("TON KHO") || r.Contains("THẺ KHO") || r.Contains("XUẤT NHẬP TỒN"))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(m.CODE, '') AS MaHang,
                            m.NAME AS MatHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            COALESCE(nh.NAME, '') AS NhomHang,
                            CAST(COALESCE(m.SOLUONGTON, 0) AS DECIMAL(18,2)) AS TonKho,
                            CAST(COALESCE(m.GIAMUA, 0) AS DECIMAL(18,0)) AS GiaMua,
                            CAST(COALESCE(m.GIABAN1, 0) AS DECIMAL(18,0)) AS GiaBan,
                            CAST(COALESCE(m.SOLUONGTON, 0) * COALESCE(m.GIAMUA, 0) AS DECIMAL(18,0)) AS TienTon
                        FROM DMATHANG m
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                          AND (CAST(@NhomHangId AS VARCHAR(255)) IN ('', '0') OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomHangId)
                        ORDER BY nh.NAME, m.NAME";
                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomHangId = filter2 });
                    dt.Load(reader);
                }

                // -------------------------------------------------------------
                // 4. BÁO CÁO BÁN HÀNG
                // -------------------------------------------------------------
                else if (r.Contains("DOANH THU THEO LOẠI ĐỒ") || r.Contains("DOANH THU THEO LOAI DO"))
                {
                    string sql = @"
                        SELECT 
                            CAST(d.NGAY AS DATE) AS Ngay,
                            CAST(COALESCE(SUM(CASE WHEN nh.LOAI = 1 THEN c.THANHTIEN ELSE 0 END), 0) AS DECIMAL(18,0)) AS DoAn,
                            CAST(COALESCE(SUM(CASE WHEN nh.LOAI = 2 THEN c.THANHTIEN ELSE 0 END), 0) AS DECIMAL(18,0)) AS DoUong,
                            CAST(COALESCE(SUM(CASE WHEN nh.LOAI = 3 THEN c.THANHTIEN ELSE 0 END), 0) AS DECIMAL(18,0)) AS DichVu,
                            CAST(COALESCE(SUM(CASE WHEN nh.LOAI IS NULL OR nh.LOAI NOT IN (1,2,3) THEN c.THANHTIEN ELSE 0 END), 0) AS DECIMAL(18,0)) AS DoKhac,
                            CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) AS TongCong
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY CAST(d.NGAY AS DATE)
                        ORDER BY CAST(d.NGAY AS DATE)";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("CHƯA THANH TOÁN") || r.Contains("CHUA THANH TOAN"))
                {
                    string sql = @"
                        SELECT 
                            d.NAME AS SoPhieu,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            COALESCE(b.NAME, 'Mang về') AS BanPhong,
                            d.TIMECREATED AS BatDau,
                            CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,0)) AS TienHang,
                            CAST(COALESCE(d.TIENGIAMGIA, 0) AS DECIMAL(18,0)) AS GiamGia,
                            CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong
                        FROM TDONHANG d
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE d.LOAI = 2 AND d.STATUS = 1
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        ORDER BY d.TIMECREATED DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("HOA HỒNG") || r.Contains("HOA HONG"))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(nv.NAME, 'Administrator') AS NhanVienBan,
                            COALESCE(c.TENHANG, m.NAME, '') AS TenHang,
                            CAST(COALESCE(SUM(c.SOLUONG), 0) AS DECIMAL(18,2)) AS SoLuong,
                            CAST(COALESCE(SUM(c.THANHTIEN * 0.05), 0) AS DECIMAL(18,0)) AS HoaHong,
                            CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) AS ThanhTien
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY nv.NAME, c.TENHANG, m.NAME
                        ORDER BY nv.NAME, ThanhTien DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("KHUYẾN MẠI") || r.Contains("KHUYEN MAI"))
                {
                    string sql = @"
                        SELECT 
                            d.NAME AS SoPhieu,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            COALESCE(kh.NAME, 'Khách lẻ') AS KhachHang,
                            COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                            CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) AS SoLuong
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2) AND (c.DONGIA = 0 OR c.THANHTIEN = 0 OR c.TILEGIAMGIA = 100)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        ORDER BY d.NGAY DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("CHI TIẾT BÁN HÀNG") || r.Contains("CHI TIET BAN HANG"))
                {
                    string sql = @"
                        SELECT 
                            d.NAME AS SoHoaDon,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            COALESCE(b.NAME, 'Mang về') AS BanKhuVuc,
                            COALESCE(m.CODE, '') AS MaHang,
                            COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) AS SoLuong,
                            CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) AS DonGia,
                            CAST(COALESCE(c.THANHTIEN * COALESCE(c.TILEGIAMGIA, 0) / 100, 0) AS DECIMAL(18,0)) AS GiamGia,
                            CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) AS ThanhTien
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        ORDER BY d.NGAY DESC, d.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("MẶT HÀNG BÁN") || r.Contains("MAT HANG BAN") || (r.Contains("MẶT HÀNG") && r.Contains("BÁN")))
                {
                    string leadCol = "COALESCE(m.CODE, '') AS MaHang";
                    string groupBy = "m.CODE, c.TENHANG, m.NAME, dvt.NAME";
                    string orderBy = "COALESCE(c.TENHANG, m.NAME, '')";

                    if (r.Contains("NHÂN VIÊN") || r.Contains("NV"))
                    {
                        leadCol = "COALESCE(nv.NAME, 'Administrator') AS NhanVienPhucVu";
                        groupBy = "nv.NAME, m.CODE, c.TENHANG, m.NAME, dvt.NAME";
                        orderBy = "COALESCE(nv.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";
                    }
                    else if (r.Contains("KHU VỰC") || r.Contains("BÀN"))
                    {
                        leadCol = "COALESCE(b.NAME, 'Mang về') AS BanKhuVuc";
                        groupBy = "b.NAME, m.CODE, c.TENHANG, m.NAME, dvt.NAME";
                        orderBy = "COALESCE(b.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";
                    }
                    else if (r.Contains("THU NGÂN"))
                    {
                        leadCol = "COALESCE(u.NAME, 'Administrator') AS ThuNgan";
                        groupBy = "u.NAME, m.CODE, c.TENHANG, m.NAME, dvt.NAME";
                        orderBy = "COALESCE(u.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";
                    }
                    else if (r.Contains("KHÁCH HÀNG"))
                    {
                        leadCol = "COALESCE(kh.NAME, 'Khách lẻ') AS KhachHang";
                        groupBy = "kh.NAME, m.CODE, c.TENHANG, m.NAME, dvt.NAME";
                        orderBy = "COALESCE(kh.NAME, ''), COALESCE(c.TENHANG, m.NAME, '')";
                    }

                    string sql = $@"
                        SELECT 
                            {leadCol},
                            COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                            COALESCE(m.CODE, '') AS MaHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(SUM(c.SOLUONG), 0) AS DECIMAL(18,2)) AS SoLuong,
                            CAST(COALESCE(AVG(c.DONGIA), 0) AS DECIMAL(18,0)) AS DonGia,
                            CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) AS ThanhTien
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY {groupBy}
                        ORDER BY {orderBy}";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("TỔNG HỢP BÁN") || r.Contains("TONG HOP BAN"))
                {
                    string groupCol = "CAST(d.NGAY AS DATE)";
                    string groupDisplay = "CAST(d.NGAY AS DATE) AS Ngay";

                    if (r.Contains("NHÂN VIÊN") || r.Contains("NV"))
                    {
                        groupCol = "nv.NAME";
                        groupDisplay = "COALESCE(nv.NAME, 'Administrator') AS NhanVienPhucVu";
                    }
                    else if (r.Contains("THU NGÂN") || r.Contains("THU NGAN"))
                    {
                        groupCol = "u.NAME";
                        groupDisplay = "COALESCE(u.NAME, 'Administrator') AS ThuNgan";
                    }
                    else if (r.Contains("KHÁCH HÀNG") || r.Contains("KHACH HANG"))
                    {
                        groupCol = "kh.NAME";
                        groupDisplay = "COALESCE(kh.NAME, 'Khách lẻ') AS KhachHang";
                    }
                    else if (r.Contains("KHU VỰC") || r.Contains("BÀN"))
                    {
                        groupCol = "b.NAME";
                        groupDisplay = "COALESCE(b.NAME, 'Mang về') AS BanKhuVuc";
                    }

                    string sql = $@"
                        SELECT 
                            {groupDisplay},
                            COUNT(d.ID) AS SoLuongDon,
                            CAST(COALESCE(SUM(d.TIENHANG), 0) AS DECIMAL(18,0)) AS TienHang,
                            CAST(COALESCE(SUM(d.TIENGIAMGIA), 0) AS DECIMAL(18,0)) AS TienGiamGia,
                            CAST(COALESCE(SUM(d.TIENTHUE), 0) AS DECIMAL(18,0)) AS TienThue,
                            CAST(COALESCE(SUM(d.TONGCONG), 0) AS DECIMAL(18,0)) AS TongCong,
                            CAST(COALESCE(SUM(d.THANHTOAN), 0) AS DECIMAL(18,0)) AS ThanhToan
                        FROM TDONHANG d
                        LEFT JOIN DBAN b ON CAST(d.DBANID AS VARCHAR(50)) = CAST(b.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY {groupCol}
                        ORDER BY {groupCol}";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("BÁN HÀNG THEO NGÀY") || r.Contains("BAN HANG THEO NGAY") || r.Contains("BÁO CÁO BÁN HÀNG"))
                {
                    string sql = @"
                        SELECT 
                            d.NAME AS SoPhieu,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            COALESCE(u.NAME, 'Administrator') AS ThuNgan,
                            COALESCE(nv.NAME, '') AS NhanVienBan,
                            CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,0)) AS TienHang,
                            CAST(COALESCE(d.TIENGIAMGIA, 0) AS DECIMAL(18,0)) AS GiamGia,
                            CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong,
                            CAST(COALESCE(d.TIENMAT, d.TONGCONG, 0) AS DECIMAL(18,0)) AS TienMat,
                            CAST(COALESCE(d.CHUYENKHOAN, 0) AS DECIMAL(18,0)) AS ChuyenKhoan,
                            CAST(COALESCE(d.THE, 0) AS DECIMAL(18,0)) AS The,
                            CAST(COALESCE(d.THETRATRUOC, 0) AS DECIMAL(18,0)) AS TheTt
                        FROM TDONHANG d
                        LEFT JOIN DNHANVIEN nv ON CAST(d.DNHANVIENXUATID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN u ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(u.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        ORDER BY d.NGAY DESC, d.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }

                // -------------------------------------------------------------
                // 5. BÁO CÁO ĐẶT HÀNG
                // -------------------------------------------------------------
                else if (r.Contains("ĐẶT HÀNG") || r.Contains("DAT HANG"))
                {
                    if (r.Contains("MẶT HÀNG"))
                    {
                        string sql = @"
                            SELECT 
                                CAST(d.NGAY AS DATE) AS Ngay,
                                COALESCE(m.CODE, '') AS MaHang,
                                COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                                COALESCE(dvt.NAME, '') AS DVT,
                                CAST(COALESCE(c.SOLUONG, 0) AS DECIMAL(18,2)) AS SoLuong,
                                CAST(COALESCE(c.DONGIA, 0) AS DECIMAL(18,0)) AS DonGia,
                                CAST(COALESCE(c.THANHTIEN, 0) AS DECIMAL(18,0)) AS ThanhTien
                            FROM TDONHANGCHITIET c
                            JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                            LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                            LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                            WHERE d.LOAI = 10
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            ORDER BY d.NGAY DESC";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                    else if (r.Contains("TỔNG HỢP"))
                    {
                        string groupCol = "CAST(d.NGAY AS DATE)";
                        string groupDisplay = "CAST(d.NGAY AS DATE) AS Ngay";
                        if (r.Contains("KHÁCH HÀNG"))
                        {
                            groupCol = "kh.NAME";
                            groupDisplay = "COALESCE(kh.NAME, 'Khách lẻ') AS KhachHang";
                        }

                        string sql = $@"
                            SELECT 
                                {groupDisplay},
                                COUNT(d.ID) AS SoLuongDon,
                                CAST(COALESCE(SUM(d.TIENHANG), 0) AS DECIMAL(18,0)) AS TienHang,
                                CAST(COALESCE(SUM(d.TIENGIAMGIA), 0) AS DECIMAL(18,0)) AS GiamGia,
                                CAST(COALESCE(SUM(d.TONGCONG), 0) AS DECIMAL(18,0)) AS TongCong
                            FROM TDONHANG d
                            LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                            WHERE d.LOAI = 10
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            GROUP BY {groupCol}
                            ORDER BY {groupCol}";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                    else
                    {
                        string sql = @"
                            SELECT 
                                d.NAME AS SoPhieu,
                                CAST(d.NGAY AS DATE) AS Ngay,
                                COALESCE(kh.NAME, 'Khách lẻ') AS KhachHang,
                                COALESCE(kh.DIENTHOAI, '') AS DienThoai,
                                COALESCE(kh.DIACHI, '') AS DiaChi,
                                CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,0)) AS TienHang,
                                CAST(COALESCE(d.TIENGIAMGIA, 0) AS DECIMAL(18,0)) AS GiamGia,
                                CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong,
                                COALESCE(nv.NAME, '') AS NhanVien
                            FROM TDONHANG d
                            LEFT JOIN DKHACHHANG kh ON CAST(d.DKHACHHANGID AS VARCHAR(50)) = CAST(kh.ID AS VARCHAR(50))
                            LEFT JOIN DNHANVIEN nv ON CAST(d.USERCREATEDID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                            WHERE d.LOAI = 10
                              AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                              AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                            ORDER BY d.NGAY DESC, d.NAME";
                        using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                        dt.Load(reader);
                    }
                }

                // -------------------------------------------------------------
                // 6. BÁO CÁO CÔNG NỢ
                // -------------------------------------------------------------
                else if (r.Contains("CÔNG NỢ") || r.Contains("CONG NO"))
                {
                    if (r.Contains("NHÀ CUNG CẤP") || r.Contains("NCC"))
                    {
                        string sql = @"
                            SELECT 
                                COALESCE(ncc.MANHACUNGCAP, ncc.CODE, '') AS MaNCC,
                                ncc.NAME AS NhaCungCap,
                                COALESCE(ncc.DIENTHOAI, '') AS DienThoai,
                                COALESCE(ncc.DIACHI, '') AS DiaChi,
                                COALESCE(nn.NAME, 'Chưa phân nhóm') AS NhomNCC,
                                CAST(COALESCE(ncc.CONGNO, 0) AS DECIMAL(18,0)) AS CongNo
                            FROM DNHACUNGCAP ncc
                            LEFT JOIN DNHOMNCC nn ON CAST(ncc.DNHOMNCCID AS VARCHAR(50)) = CAST(nn.ID AS VARCHAR(50))
                            WHERE (ncc.STATUS IS NULL OR ncc.STATUS <> 0)
                            ORDER BY ncc.NAME";

                        using var reader = await conn.ExecuteReaderAsync(sql);
                        dt.Load(reader);
                    }
                    else
                    {
                        string sql = @"
                            SELECT 
                                COALESCE(kh.MAKHACHHANG, kh.CODE, '') AS MaKhach,
                                kh.NAME AS KhachHang,
                                COALESCE(kh.DIENTHOAI, '') AS DienThoai,
                                COALESCE(kh.DIACHI, '') AS DiaChi,
                                COALESCE(nk.NAME, 'Chưa phân nhóm') AS NhomKhach,
                                CAST(COALESCE(kh.CONGNO, 0) AS DECIMAL(18,0)) AS CongNo
                            FROM DKHACHHANG kh
                            LEFT JOIN DNHOMKH nk ON CAST(kh.DNHOMKHID AS VARCHAR(50)) = CAST(nk.ID AS VARCHAR(50))
                            WHERE (kh.STATUS IS NULL OR kh.STATUS <> 0)
                            ORDER BY kh.NAME";

                        using var reader = await conn.ExecuteReaderAsync(sql);
                        dt.Load(reader);
                    }
                }

                // -------------------------------------------------------------
                // 7. BÁO CÁO QUẢN TRỊ
                // -------------------------------------------------------------
                else if (r.Contains("LÃI GỘP") || r.Contains("LAI GOP"))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(nh.NAME, 'KHÁC') AS TenNhomHang,
                            COALESCE(m.CODE, '') AS MaHang,
                            COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(SUM(c.SOLUONG), 0) AS DECIMAL(18,2)) AS SoLuong,
                            CAST(COALESCE(AVG(c.DONGIA), 0) AS DECIMAL(18,0)) AS DonGia,
                            CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) AS DoanhThu,
                            CAST(COALESCE(SUM(c.SOLUONG * COALESCE(m.GIAMUA, 0)), 0) AS DECIMAL(18,0)) AS TienVon,
                            CAST(COALESCE(SUM(c.THANHTIEN - c.SOLUONG * COALESCE(m.GIAMUA, 0)), 0) AS DECIMAL(18,0)) AS LaiGop
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY nh.NAME, m.CODE, c.TENHANG, m.NAME, dvt.NAME
                        ORDER BY LaiGop DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("BÁN CHẠY") || r.Contains("BAN CHAY"))
                {
                    string sql = @"
                        SELECT FIRST 20
                            COALESCE(m.CODE, '') AS MaHang,
                            COALESCE(c.TENHANG, m.NAME, '') AS MatHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(SUM(c.SOLUONG), 0) AS DECIMAL(18,2)) AS SoLuong,
                            CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) AS DoanhThu
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY m.CODE, c.TENHANG, m.NAME, dvt.NAME
                        ORDER BY SoLuong DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("THEO GIỜ") || r.Contains("THEO GIO"))
                {
                    string sql = @"
                        SELECT 
                            EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY)) || 'h đến ' || (EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY)) + 1) || 'h' AS KhungGio,
                            COUNT(d.ID) AS SoHoaDon,
                            CAST(COALESCE(SUM(d.TONGCONG), 0) AS DECIMAL(18,0)) AS TongDoanhSo
                        FROM TDONHANG d
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY))
                        ORDER BY EXTRACT(HOUR FROM COALESCE(d.TIMECREATED, d.NGAY)) ASC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else if (r.Contains("XÓA GIẢM") || r.Contains("XOA GIAM") || r.Contains("TRẢ LẠI"))
                {
                    string sql = @"
                        SELECT 
                            CAST(l.NGAY AS DATE) AS Ngay,
                            l.GIO AS Gio,
                            COALESCE(l.SODONHANG, '') AS SoHd,
                            COALESCE(l.TAIKHOAN, '') AS TaiKhoan,
                            COALESCE(l.THIETBI, '') AS ThietBi,
                            COALESCE(l.CHUCNANG, '') AS ThaoTac,
                            COALESCE(l.NOTE, '') AS GhiChu
                        FROM TLUUVET l
                        WHERE (@TuNgay IS NULL OR CAST(l.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(l.NGAY AS DATE) <= @DenNgay)
                        ORDER BY l.NGAY DESC, l.GIO DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }

                // -------------------------------------------------------------
                // 8. BÁO CÁO DANH MỤC (10 reports)
                // -------------------------------------------------------------
                else if (r == "DANH SÁCH KHÁCH HÀNG THEO NHÓM" || r == "DANH SÁCH KHÁCH HÀNG THEO NHÂN VIÊN")
                {
                    string nhomKhach = (r == "DANH SÁCH KHÁCH HÀNG THEO NHÓM") ? filter1 : filter2;
                    string nhanVien = (r == "DANH SÁCH KHÁCH HÀNG THEO NHÓM") ? filter2 : filter1;
                    string orderBy = (r == "DANH SÁCH KHÁCH HÀNG THEO NHÓM") ? "nk.NAME, kh.NAME" : "nv.NAME, kh.NAME";

                    string sql = $@"
                        SELECT 
                            COALESCE(kh.MAKHACH, '') AS MaKhach,
                            kh.NAME AS TenKhach,
                            COALESCE(nk.NAME, 'Chưa phân nhóm') AS NhomKhach,
                            COALESCE(nv.NAME, '') AS NhanVien,
                            COALESCE(kh.DIENTHOAI, '') AS DienThoai,
                            COALESCE(kh.DIACHI, '') AS DiaChi,
                            COALESCE(kh.NOTE, '') AS GhiChu
                        FROM DKHACHHANG kh
                        LEFT JOIN DNHOMKHACHHANG nk ON CAST(kh.DNHOMKHACHHANGID AS VARCHAR(50)) = CAST(nk.ID AS VARCHAR(50))
                        LEFT JOIN DNHANVIEN nv ON CAST(kh.DNHANVIENID AS VARCHAR(50)) = CAST(nv.ID AS VARCHAR(50))
                        WHERE (kh.STATUS IS NULL OR kh.STATUS <> 0)
                          AND (COALESCE(CAST(@NhomKhach AS VARCHAR(255)), '') = '' OR CAST(kh.DNHOMKHACHHANGID AS VARCHAR(50)) = @NhomKhach)
                          AND (COALESCE(CAST(@NhanVien AS VARCHAR(255)), '') = '' OR CAST(kh.DNHANVIENID AS VARCHAR(50)) = @NhanVien)
                        ORDER BY {orderBy}";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomKhach = nhomKhach, NhanVien = nhanVien });
                    dt.Load(reader);
                }
                else if (r == "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(nn.NAME, 'Chưa phân nhóm') AS NhomNCC,
                            COALESCE(ncc.MANHACUNGCAP, '') AS MaNCC,
                            ncc.NAME AS TenNCC,
                            COALESCE(ncc.DIENTHOAI, '') AS DienThoai,
                            COALESCE(ncc.DIACHI, '') AS DiaChi,
                            COALESCE(ncc.EMAIL, '') AS Email,
                            COALESCE(ncc.NOTE, '') AS GhiChu
                        FROM DNHACUNGCAP ncc
                        LEFT JOIN DNHOMNHACUNGCAP nn ON CAST(ncc.DNHOMNHACUNGCAPID AS VARCHAR(50)) = CAST(nn.ID AS VARCHAR(50))
                        WHERE (ncc.STATUS IS NULL OR ncc.STATUS <> 0)
                          AND (COALESCE(CAST(@NhomNCC AS VARCHAR(255)), '') = '' OR CAST(ncc.DNHOMNHACUNGCAPID AS VARCHAR(50)) = @NhomNCC)
                        ORDER BY nn.NAME, ncc.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomNCC = filter1 });
                    dt.Load(reader);
                }
                else if (r == "DANH SÁCH ĐỢT KHUYẾN MẠI")
                {
                    string sql = @"
                        SELECT 
                            km.NAME AS TenKM,
                            CAST(km.TUNGAY AS DATE) AS TuNgay,
                            CAST(km.DENNGAY AS DATE) AS DenNgay,
                            CAST(COALESCE(km.TILEGIAMGIA, 0) AS DECIMAL(18,2)) AS TiLeGiamGia,
                            CASE WHEN km.NGUNGAPDUNG = 1 THEN 'Ngừng áp dụng' ELSE 'Đang áp dụng' END AS TrangThai
                        FROM DDOTKHUYENMAI km
                        WHERE (km.STATUS IS NULL OR km.STATUS <> 0)
                          AND (COALESCE(CAST(@Status AS VARCHAR(255)), '') = '' OR CAST(COALESCE(km.NGUNGAPDUNG, 0) AS VARCHAR(10)) = @Status)
                        ORDER BY km.TUNGAY DESC, km.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { Status = filter1 });
                    dt.Load(reader);
                }
                else if (r == "DANH SÁCH MẶT HÀNG THEO NHÓM")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(nh.NAME, 'Chưa phân nhóm') AS TenNhomHang,
                            COALESCE(m.CODE, '') AS MaHang,
                            m.NAME AS TenHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(m.GIABAN, 0) AS DECIMAL(18,0)) AS GiaBan,
                            CAST(COALESCE(m.GIANHAP, 0) AS DECIMAL(18,0)) AS GiaNhap
                        FROM DMATHANG m
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                          AND (COALESCE(CAST(@NhomHang AS VARCHAR(255)), '') = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomHang)
                        ORDER BY nh.NAME, m.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomHang = filter1 });
                    dt.Load(reader);
                }
                else if (r == "DANH SÁCH MẶT HÀNG THEO HÃNG SẢN XUẤT")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(hsx.NAME, 'Chưa xác định') AS HangSanXuat,
                            COALESCE(nh.NAME, '') AS TenNhomHang,
                            COALESCE(m.CODE, '') AS MaHang,
                            m.NAME AS TenHang,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(m.GIABAN, 0) AS DECIMAL(18,0)) AS GiaBan
                        FROM DMATHANG m
                        LEFT JOIN DHANGSANXUAT hsx ON CAST(m.DHANGSANXUATID AS VARCHAR(50)) = CAST(hsx.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(m.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                          AND (COALESCE(CAST(@HangSX AS VARCHAR(255)), '') = '' OR CAST(m.DHANGSANXUATID AS VARCHAR(50)) = @HangSX)
                          AND (COALESCE(CAST(@NhomHang AS VARCHAR(255)), '') = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomHang)
                        ORDER BY hsx.NAME, m.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { HangSX = filter1, NhomHang = filter2 });
                    dt.Load(reader);
                }
                else if (r == "KHÁCH HÀNG ĐẾN NGÀY SINH NHẬT")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(kh.MAKHACH, '') AS MaKhach,
                            kh.NAME AS TenKhach,
                            CAST(kh.NGAYSINH AS DATE) AS NgaySinh,
                            COALESCE(kh.DIENTHOAI, '') AS DienThoai,
                            COALESCE(kh.DIACHI, '') AS DiaChi,
                            COALESCE(nk.NAME, '') AS NhomKhach
                        FROM DKHACHHANG kh
                        LEFT JOIN DNHOMKHACHHANG nk ON CAST(kh.DNHOMKHACHHANGID AS VARCHAR(50)) = CAST(nk.ID AS VARCHAR(50))
                        WHERE (kh.STATUS IS NULL OR kh.STATUS <> 0)
                          AND kh.NGAYSINH IS NOT NULL
                          AND (COALESCE(CAST(@NhomKhach AS VARCHAR(255)), '') = '' OR CAST(kh.DNHOMKHACHHANGID AS VARCHAR(50)) = @NhomKhach)
                          AND (COALESCE(CAST(@ThangSinh AS VARCHAR(255)), '') = '' OR EXTRACT(MONTH FROM kh.NGAYSINH) = CAST(@ThangSinh AS INTEGER))
                        ORDER BY EXTRACT(MONTH FROM kh.NGAYSINH), EXTRACT(DAY FROM kh.NGAYSINH)";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomKhach = filter1, ThangSinh = filter2 });
                    dt.Load(reader);
                }
                else if (r == "BÁO CÁO CẤU HÌNH BÀN KHU VỰC")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(kv.NAME, 'Chưa xếp khu') AS BanKhuVuc,
                            b.NAME AS TenBan,
                            CASE WHEN b.STATUS = 1 THEN 'Đang sử dụng' ELSE 'Trống' END AS TrangThai,
                            COALESCE(b.NOTE, '') AS GhiChu
                        FROM DBAN b
                        LEFT JOIN DKHUVUC kv ON CAST(b.DKHUVUCID AS VARCHAR(50)) = CAST(kv.ID AS VARCHAR(50))
                        WHERE (b.STATUS IS NULL OR b.STATUS <> 0)
                          AND (COALESCE(CAST(@KhuVuc AS VARCHAR(255)), '') = '' OR CAST(b.DKHUVUCID AS VARCHAR(50)) = @KhuVuc)
                        ORDER BY kv.NAME, b.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { KhuVuc = filter1 });
                    dt.Load(reader);
                }
                else if (r == "CÔNG THỨC ĐỊNH LƯỢNG")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(m.NAME, '') AS MatHang,
                            COALESCE(vt.NAME, '') AS NguyenLieu,
                            COALESCE(dvt.NAME, '') AS DVT,
                            CAST(COALESCE(dl.SOLUONG, 0) AS DECIMAL(18,3)) AS SoLuong
                        FROM DDINHLUONG dl
                        JOIN DMATHANG m ON CAST(dl.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG vt ON CAST(dl.DVATTUID AS VARCHAR(50)) = CAST(vt.ID AS VARCHAR(50))
                        LEFT JOIN DDONVITINH dvt ON CAST(vt.DDONVITINHID AS VARCHAR(50)) = CAST(dvt.ID AS VARCHAR(50))
                        WHERE (m.STATUS IS NULL OR m.STATUS <> 0)
                          AND (COALESCE(CAST(@NhomHang AS VARCHAR(255)), '') = '' OR CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = @NhomHang)
                          AND (COALESCE(CAST(@MatHang AS VARCHAR(255)), '') = '' OR CAST(dl.DMATHANGID AS VARCHAR(50)) = @MatHang)
                        ORDER BY m.NAME, vt.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomHang = filter1, MatHang = filter2 });
                    dt.Load(reader);
                }
                else if (r == "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG")
                {
                    string sql = @"
                        SELECT 
                            COALESCE(g.NAME, 'Chưa nhóm') AS NhomNguoiDung,
                            u.NAME AS TenNguoiDung,
                            COALESCE(u.USERNAME, '') AS TenDangNhap,
                            CASE WHEN u.STATUS = 1 THEN 'Hoạt động' ELSE 'Khóa' END AS TrangThai
                        FROM SUSER u
                        LEFT JOIN SGROUPUSER g ON CAST(u.SGROUPUSERID AS VARCHAR(50)) = CAST(g.ID AS VARCHAR(50))
                        WHERE (u.STATUS IS NULL OR u.STATUS <> 0)
                          AND (COALESCE(CAST(@NhomNguoiDung AS VARCHAR(255)), '') = '' OR CAST(u.SGROUPUSERID AS VARCHAR(50)) = @NhomNguoiDung)
                        ORDER BY g.NAME, u.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { NhomNguoiDung = filter1 });
                    dt.Load(reader);
                }
                else if (r.Contains("BIỂU ĐỒ") || r.Contains("BIEU DO"))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(nh.NAME, 'KHÁC') AS TenNhom,
                            CAST(COALESCE(SUM(c.THANHTIEN), 0) AS DECIMAL(18,0)) AS DoanhSo
                        FROM TDONHANGCHITIET c
                        JOIN TDONHANG d ON CAST(c.TDONHANGID AS VARCHAR(50)) = CAST(d.ID AS VARCHAR(50))
                        LEFT JOIN DMATHANG m ON CAST(c.DMATHANGID AS VARCHAR(50)) = CAST(m.ID AS VARCHAR(50))
                        LEFT JOIN DNHOMMATHANG nh ON CAST(m.DNHOMMATHANGID AS VARCHAR(50)) = CAST(nh.ID AS VARCHAR(50))
                        WHERE (d.LOAI = 2 OR d.LOAI = 10) AND (d.STATUS = 2 OR d.STATUS IS NULL)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay)
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        GROUP BY nh.NAME
                        ORDER BY DoanhSo DESC";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }
                else
                {
                    // Generic fallback for any other reports
                    string sql = @"
                        SELECT 
                            COALESCE(d.NOTE, '') AS GhiChu,
                            d.NAME AS SoPhieu,
                            CAST(d.NGAY AS DATE) AS Ngay,
                            CAST(COALESCE(d.TONGCONG, 0) AS DECIMAL(18,0)) AS TongCong,
                            CAST(COALESCE(d.TIENHANG, 0) AS DECIMAL(18,0)) AS TienHang,
                            COALESCE(ncc.NAME, '') AS NhaCungCap
                        FROM TDONHANG d
                        LEFT JOIN DNHACUNGCAP ncc ON CAST(d.DNHACUNGCAPID AS VARCHAR(50)) = CAST(ncc.ID AS VARCHAR(50))
                        WHERE (d.STATUS IS NULL OR d.STATUS <> 0)
                          AND (@TuNgay IS NULL OR CAST(d.NGAY AS DATE) >= @TuNgay) 
                          AND (@DenNgay IS NULL OR CAST(d.NGAY AS DATE) <= @DenNgay)
                        ORDER BY d.NGAY DESC, d.NAME";

                    using var reader = await conn.ExecuteReaderAsync(sql, new { TuNgay = (object?)tuNgay ?? DBNull.Value, DenNgay = (object?)denNgay ?? DBNull.Value });
                    dt.Load(reader);
                }

                DgDuLieuTho.ItemsSource = dt.DefaultView;
                if (dt.Rows.Count > 0)
                {
                    DgDuLieuTho.SelectedIndex = 0;
                }

                await ApplySavedTemplateConfigAsync(conn, r);
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadDuLieuThoAsync error: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task ApplySavedTemplateConfigAsync(IDbConnection conn, string reportName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportName) || DgDuLieuTho.Columns.Count == 0) return;

                var rep = await conn.QueryFirstOrDefaultAsync(
                    "SELECT CAST(ID AS VARCHAR(50)) AS ID FROM SREPORT WHERE UPPER(TRIM(NAME)) = @Name",
                    new { Name = reportName.Trim().ToUpper() });

                if (rep != null)
                {
                    var repTemplate = await conn.QueryFirstOrDefaultAsync(
                        "SELECT CONFIG FROM SREPORTTEMPLATE WHERE SREPORTID = @RepId AND (STATUS IS NULL OR STATUS <> 0)",
                        new { RepId = rep.ID });

                    if (repTemplate != null && repTemplate.CONFIG != null)
                    {
                        string xmlStr = (repTemplate.CONFIG is byte[] b) ? System.Text.Encoding.UTF8.GetString(b) : repTemplate.CONFIG.ToString();
                        if (!string.IsNullOrWhiteSpace(xmlStr))
                        {
                            var doc = System.Xml.Linq.XDocument.Parse(xmlStr);
                            foreach (var col in DgDuLieuTho.Columns)
                            {
                                string header = col.Header?.ToString() ?? "";
                                var dataElem = doc.Descendants("Data").FirstOrDefault(d =>
                                    d.Element("COT")?.Value == header ||
                                    d.Element("CAPTION")?.Value == header ||
                                    d.Element("DATAFIELD")?.Value == header);

                                if (dataElem != null)
                                {
                                    string hienThiVal = dataElem.Element("HIENTHI")?.Value ?? "30";
                                    col.Visibility = (hienThiVal == "30" || hienThiVal == "1") ? Visibility.Visible : Visibility.Collapsed;
                                    string caption = dataElem.Element("CAPTION")?.Value;
                                    if (!string.IsNullOrWhiteSpace(caption))
                                    {
                                        col.Header = caption;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ApplySavedTemplateConfigAsync error: " + ex.Message);
            }
        }
    }
}
