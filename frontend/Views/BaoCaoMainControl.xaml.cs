using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        private static readonly Dictionary<string, List<ComboLookupItem>> _lookupCache = new Dictionary<string, List<ComboLookupItem>>();
        private List<ReportCategoryNode> _reportCategories = new List<ReportCategoryNode>();
        private string _currentSelectedReport = "BÁO CÁO TỒN QUỸ";

        public BaoCaoMainControl()
        {
            InitializeComponent();
            InitDates();
            BuildReportTreeData();
            RenderTreeView();
            RenderThuongDung();
            SelectReport("BÁO CÁO TỒN QUỸ");
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
                        case "LyDoThu":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DLYDOTHUCHI WHERE (STATUS IS NULL OR STATUS <> 0) AND (LALYDOTHU = 1 OR LOAILYDO = 1 OR LALYDOTHU IS NULL) ORDER BY SORTORDER, NAME";
                            break;
                        case "LyDoChi":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DLYDOTHUCHI WHERE (STATUS IS NULL OR STATUS <> 0) AND (LALYDOTHU = 0 OR LOAILYDO = 2 OR LALYDOTHU IS NULL) ORDER BY SORTORDER, NAME";
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
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHOMKH WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhaCungCap":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHACUNGCAP WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "NhomNCC":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DNHOMNCC WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "TaiKhoanNganHang":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DTAIKHOANNGANHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
                            break;
                        case "KhuVuc":
                            sql = "SELECT CAST(ID AS VARCHAR(50)) AS Id, NAME as Name FROM DKHUVUC WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME";
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

            PnlFilter1.Visibility = Visibility.Visible;
            PnlFilter2.Visibility = Visibility.Visible;
            PnlFilter3.Visibility = Visibility.Collapsed;
            PnlFilter4.Visibility = Visibility.Collapsed;

            if (r.Contains("PHIẾU THU") || r.Contains("PHIEU THU"))
            {
                LblFilter1.Text = "Lý do thu chi";
                CboFilter1.ItemsSource = await GetLookupListAsync("LyDoThu");

                LblFilter2.Text = "Cửa hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("CuaHang");
            }
            else if (r.Contains("PHIẾU CHI") || r.Contains("PHIEU CHI"))
            {
                LblFilter1.Text = "Lý do thu chi";
                CboFilter1.ItemsSource = await GetLookupListAsync("LyDoChi");

                LblFilter2.Text = "Cửa hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("CuaHang");
            }
            else if (r.Contains("THU CHI") || r.Contains("QUỸ") || r.Contains("QUY"))
            {
                if (r.Contains("TỒN QUỸ") || r.Contains("TON QUY"))
                {
                    LblFilter1.Text = "Kho hàng";
                    CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");

                    LblFilter2.Text = "Nhân viên";
                    CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
                }
                else
                {
                    LblFilter1.Text = "Lý do thu chi";
                    CboFilter1.ItemsSource = await GetLookupListAsync("LyDoThuChi");

                    LblFilter2.Text = "Cửa hàng";
                    CboFilter2.ItemsSource = await GetLookupListAsync("CuaHang");
                }
            }
            else if (r.Contains("XUẤT BÁN HÀNG") || r.Contains("MAT HANG XUAT BAN") || r.Contains("MẶT HÀNG BÁN"))
            {
                LblFilter1.Text = "Kho hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");

                LblFilter2.Text = "Nhóm hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhomHang");

                LblFilter3.Text = "Mặt hàng";
                CboFilter3.ItemsSource = await GetLookupListAsync("MatHang");
                PnlFilter3.Visibility = Visibility.Visible;
            }
            else if (r.Contains("KHO") || r.Contains("NHẬP HÀNG") || r.Contains("XUẤT") || r.Contains("KIỂM KÊ") || r.Contains("HSD") || r.Contains("XNT"))
            {
                LblFilter1.Text = "Kho hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");

                LblFilter2.Text = "Nhóm hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhomHang");

                if (r.Contains("NHÀ CUNG CẤP") || r.Contains("NCC"))
                {
                    LblFilter3.Text = "Nhà cung cấp";
                    CboFilter3.ItemsSource = await GetLookupListAsync("NhaCungCap");
                    PnlFilter3.Visibility = Visibility.Visible;
                }
                else if (r.Contains("NHÂN VIÊN") || r.Contains("NV"))
                {
                    LblFilter3.Text = "Nhân viên";
                    CboFilter3.ItemsSource = await GetLookupListAsync("NhanVien");
                    PnlFilter3.Visibility = Visibility.Visible;
                }
                else
                {
                    LblFilter3.Text = "Mặt hàng";
                    CboFilter3.ItemsSource = await GetLookupListAsync("MatHang");
                    PnlFilter3.Visibility = Visibility.Visible;
                }
            }
            else if (r.Contains("CÔNG NỢ KHÁCH HÀNG") || r.Contains("CONG NO KHACH HANG"))
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
            else if (r.Contains("NHÂN VIÊN") || r.Contains("NHAN VIEN") || r.Contains("THU NGÂN") || r.Contains("NVKD"))
            {
                LblFilter1.Text = "Nhóm NV";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhanVien");

                LblFilter2.Text = "Nhân viên";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
            }
            else if (r.Contains("KHÁCH HÀNG") || r.Contains("KHACH HANG"))
            {
                LblFilter1.Text = "Nhóm khách";
                CboFilter1.ItemsSource = await GetLookupListAsync("NhomKhach");

                LblFilter2.Text = "Khách hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("KhachHang");
            }
            else if (r.Contains("BÀN") || r.Contains("KHU VỰC"))
            {
                LblFilter1.Text = "Khu vực";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhuVuc");

                LblFilter2.Text = "Kho hàng";
                CboFilter2.ItemsSource = await GetLookupListAsync("KhoHang");
            }
            else
            {
                LblFilter1.Text = "Kho hàng";
                CboFilter1.ItemsSource = await GetLookupListAsync("KhoHang");

                LblFilter2.Text = "Nhân viên";
                CboFilter2.ItemsSource = await GetLookupListAsync("NhanVien");
            }

            CboFilter1.SelectedIndex = 0;
            CboFilter2.SelectedIndex = 0;
            CboFilter3.SelectedIndex = 0;
            CboFilter4.SelectedIndex = 0;
        }

        private void BuildReportTreeData()
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
                                "DANH SÁCH PHIẾU NHẬP HÀNG THEO NGÀY",
                                "DANH SÁCH PHIẾU NHẬP HÀNG THEO NHÀ CUNG CẤP",
                                "DANH SÁCH PHIẾU NHẬP HÀNG THEO NHÂN VIÊN",
                                "TỔNG HỢP MẶT HÀNG NHẬP THEO NHÀ CUNG CẤP",
                                "TỔNG HỢP NHẬP HÀNG THEO NGÀY"
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

            // Populate lower ListBox with default category (BÁO CÁO QUỸ)
            if (_reportCategories.Count > 0)
            {
                LstSubReports.ItemsSource = _reportCategories[0].ReportNames;
                LstSubReports.SelectedItem = "BÁO CÁO TỒN QUỸ";
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
            LstThuongDung.ItemsSource = new List<string>
            {
                "DANH SÁCH PHIẾU THU THEO NGÀY",
                "DANH SÁCH PHIẾU CHI THEO NGÀY",
                "TỔNG HỢP BÁN HÀNG THEO NGÀY",
                "BÁO CÁO BÁN HÀNG THEO NGÀY",
                "BÁO CÁO TỒN KHO",
                "BÁO CÁO CÔNG NỢ KHÁCH HÀNG",
                "BÁO CÁO TỒN QUỸ"
            };
        }

        private void SelectReport(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName)) return;
            _currentSelectedReport = reportName;
            TxtReportTitle.Text = reportName;
            LoadFiltersForReport(reportName);
        }



        private void TvBaoCao_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TvBaoCao.SelectedItem is TreeViewItem item && item.Tag is ReportCategoryNode catNode)
            {
                var reports = catNode.ReportNames;
                LstSubReports.ItemsSource = reports;
                if (reports != null && reports.Count > 0)
                {
                    LstSubReports.SelectedItem = reports[0];
                    SelectReport(reports[0]);
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
        }

        private void TxtSearchReport_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = TxtSearchReport.Text?.Trim()?.ToLower() ?? "";
            if (string.IsNullOrEmpty(search))
            {
                LstSearchResults.ItemsSource = null;
                return;
            }

            var allReports = new List<string>();
            GetAllReportsRecursive(_reportCategories, allReports);

            LstSearchResults.ItemsSource = allReports.Where(r => r.ToLower().Contains(search)).ToList();
        }

        private void GetAllReportsRecursive(List<ReportCategoryNode> cats, List<string> result)
        {
            foreach (var c in cats)
            {
                result.AddRange(c.ReportNames);
                GetAllReportsRecursive(c.SubCategories, result);
            }
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
            OpenReport(_currentSelectedReport);
        }

        private async void OpenReport(string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName)) return;
            if (Window.GetWindow(this) is MainAppWindow mainWin)
            {
                await mainWin.OpenTabByNameAsync(reportName);
            }
        }

        private void BtnThietKeMau_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thiết kế mẫu báo cáo đang được cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnTuyChonThamSo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng tùy chọn tham số báo cáo nâng cao đang được cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXemDuLieuTho_Click(object sender, RoutedEventArgs e)
        {
            OpenReport(_currentSelectedReport);
        }
    }
}
