using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.BaoCaoDanhMuc
{
    public partial class BaoCaoPhanQuyenControl : UserControl
    {
        private readonly string _reportType;
        private bool _isLoaded = false;
        private List<GroupUserItem> _allGroups = new List<GroupUserItem>();
        private List<UserAccountItem> _allUsers = new List<UserAccountItem>();
        private List<BaoCaoPhanQuyenItem> _rawItems = new List<BaoCaoPhanQuyenItem>();

        public BaoCaoPhanQuyenControl(string reportType = "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG")
        {
            InitializeComponent();
            _reportType = string.IsNullOrWhiteSpace(reportType) ? "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG" : reportType.Trim();

            TxtReportTitle.Text = _reportType;

            // Keyboard Shortcuts: F5 (Refresh), F3 (Search), Ctrl+P (Print), F12 (Excel)
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F5)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
                else if (e.Key == Key.F3)
                {
                    TxtSearch.Focus();
                    TxtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.F12)
                {
                    ExportToCsv();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
                {
                    PrintReport();
                    e.Handled = true;
                }
                else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
                {
                    _ = LoadDataAsync();
                    e.Handled = true;
                }
            };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            await LoadCompanyInfoAndLogoAsync();
            await LoadFiltersAsync();
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
                _allGroups = await LocalPhanQuyenService.GetGroupUsersAsync();
                _allUsers = await LocalPhanQuyenService.GetUsersAsync();

                var listGroups = new List<GroupUserItem>
                {
                    new GroupUserItem { Id = "", Name = "Tất cả" }
                };
                listGroups.AddRange(_allGroups);

                CboNhomNguoiDung.ItemsSource = listGroups;
                CboNhomNguoiDung.SelectedIndex = 0;

                UpdateUserCombo();
            }
            catch { }
        }

        private void UpdateUserCombo()
        {
            var selGrp = CboNhomNguoiDung.SelectedItem as GroupUserItem;
            string selGrpId = selGrp?.Id ?? "";

            var listUsers = new List<UserAccountItem>
            {
                new UserAccountItem { Id = "", Name = "Tất cả", Username = "Tất cả" }
            };

            var filteredUsers = _allUsers.Where(u =>
                string.IsNullOrEmpty(selGrpId) || u.SgroupuserId == selGrpId
            ).ToList();

            listUsers.AddRange(filteredUsers);

            CboNguoiDung.ItemsSource = listUsers;
            CboNguoiDung.SelectedIndex = 0;
        }

        private void CboNhomNguoiDung_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            UpdateUserCombo();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (!_isLoaded) return;

            try
            {
                var selGrp = CboNhomNguoiDung.SelectedItem as GroupUserItem;
                var selUser = CboNguoiDung.SelectedItem as UserAccountItem;
                bool includeReports = ChkHienThiCaBaoCao.IsChecked == true;

                string grpId = selGrp?.Id ?? "";
                string userId = selUser?.Id ?? "";

                _rawItems = await LocalPhanQuyenService.GetBaoCaoPhanQuyenDataAsync(grpId, userId, includeReports);
                RenderReportTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderReportTable()
        {
            TableContainer.Children.Clear();

            string search = TxtSearch.Text?.Trim()?.ToLower() ?? "";
            var selGrp = CboNhomNguoiDung.SelectedItem as GroupUserItem;
            var selUser = CboNguoiDung.SelectedItem as UserAccountItem;

            string grpName = selGrp != null && !string.IsNullOrEmpty(selGrp.Id) ? selGrp.Name : "Tất cả";
            string userName = selUser != null && !string.IsNullOrEmpty(selUser.Id) ? (string.IsNullOrEmpty(selUser.Name) ? selUser.Username : selUser.Name) : "Tất cả";

            TxtSubtitleGroup.Text = $"Nhóm người dùng: {grpName}";
            TxtSubtitleUser.Text = $"Người dùng: {userName}";

            var filtered = _rawItems.Where(x =>
                string.IsNullOrEmpty(search) ||
                (x.GroupUserName?.ToLower().Contains(search) == true) ||
                (x.CategoryType?.ToLower().Contains(search) == true) ||
                (x.SubGroup?.ToLower().Contains(search) == true) ||
                (x.FunctionName?.ToLower().Contains(search) == true)
            ).ToList();

            var tableBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 0, 0),
                Background = Brushes.White
            };

            var stackTable = new StackPanel();

            // Header row: STT (40), Quyền sử dụng (Star), Xem (60), Thêm (60), Sửa (60), Xóa (60)
            stackTable.Children.Add(CreateRowPhanQuyen("STT", "Quyền sử dụng", "Xem", "Thêm", "Sửa", "Xóa", isHeader: true));

            var grpGroups = filtered.GroupBy(x => string.IsNullOrWhiteSpace(x.GroupUserName) ? "Chưa xếp nhóm" : x.GroupUserName)
                                    .OrderBy(g => g.Key)
                                    .ToList();

            if (grpGroups.Count == 0)
            {
                stackTable.Children.Add(CreateLevel1GroupHeaderRow("NHÓM:"));
            }
            else
            {
                foreach (var gGroup in grpGroups)
                {
                    // Level 1 Header (Yellow Background)
                    stackTable.Children.Add(CreateLevel1GroupHeaderRow($"NHÓM: {gGroup.Key.ToUpper()}"));

                    var grpCategory = gGroup.GroupBy(x => x.CategoryType)
                                            .OrderBy(g => g.Key == "CHỨC NĂNG" ? 0 : 1)
                                            .ToList();

                    foreach (var gCat in grpCategory)
                    {
                        // Level 2 Header (Cyan Background)
                        stackTable.Children.Add(CreateLevel2CategoryHeaderRow(gCat.Key.ToUpper()));

                        var grpSubGroup = gCat.GroupBy(x => string.IsNullOrWhiteSpace(x.SubGroup) ? "Bán hàng" : x.SubGroup)
                                              .OrderBy(g => g.Key)
                                              .ToList();

                        foreach (var gSub in grpSubGroup)
                        {
                            // Level 3 Header (Soft Bluish Purple Background)
                            stackTable.Children.Add(CreateLevel3SectionHeaderRow(gSub.Key));

                            int stt = 1;
                            foreach (var item in gSub.OrderBy(x => x.FunctionName))
                            {
                                stackTable.Children.Add(CreateRowPhanQuyen(
                                    stt: (stt++).ToString(),
                                    quyen: item.FunctionName ?? "",
                                    xem: item.CanView ? "x" : "",
                                    them: item.CanAdd ? "x" : "",
                                    sua: item.CanEdit ? "x" : "",
                                    xoa: item.CanDelete ? "x" : "",
                                    isHeader: false
                                ));
                            }
                        }
                    }
                }
            }

            tableBorder.Child = stackTable;
            TableContainer.Children.Add(tableBorder);
        }

        private UIElement CreateLevel1GroupHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 23 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff59d")) // Yellow group row
            };
            Grid.SetColumnSpan(border, 6);

            var tb = new TextBlock
            {
                Text = title,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            grid.Children.Add(border);
            return grid;
        }

        private UIElement CreateLevel2CategoryHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b2ebf2")) // Cyan category row
            };
            Grid.SetColumnSpan(border, 6);

            var tb = new TextBlock
            {
                Text = title,
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            grid.Children.Add(border);
            return grid;
        }

        private UIElement CreateLevel3SectionHeaderRow(string title)
        {
            var grid = new Grid { MinHeight = 21 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 2, 4, 2),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2e8f0")) // Soft purple/gray row
            };
            Grid.SetColumnSpan(border, 6);

            var tb = new TextBlock
            {
                Text = title,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            grid.Children.Add(border);
            return grid;
        }

        private UIElement CreateRowPhanQuyen(string stt, string quyen, string xem, string them, string sua, string xoa, bool isHeader)
        {
            var grid = new Grid { MinHeight = isHeader ? 24 : 21 };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });                  // STT
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Quyền sử dụng
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });                  // Xem
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });                  // Thêm
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });                  // Sửa
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });                  // Xóa

            var weight = isHeader ? FontWeights.Bold : FontWeights.Normal;

            grid.Children.Add(CreateTableCell(stt, 0, HorizontalAlignment.Center, weight, isHeader));
            grid.Children.Add(CreateTableCell(quyen, 1, isHeader ? HorizontalAlignment.Center : HorizontalAlignment.Left, weight, isHeader));
            grid.Children.Add(CreateTableCell(xem, 2, HorizontalAlignment.Center, weight, isHeader));
            grid.Children.Add(CreateTableCell(them, 3, HorizontalAlignment.Center, weight, isHeader));
            grid.Children.Add(CreateTableCell(sua, 4, HorizontalAlignment.Center, weight, isHeader));
            grid.Children.Add(CreateTableCell(xoa, 5, HorizontalAlignment.Center, weight, isHeader));

            return grid;
        }

        private Border CreateTableCell(string text, int col, HorizontalAlignment align, FontWeight weight, bool isHeader)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(4, 2, 4, 2),
                Background = isHeader ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9")) : Brushes.White
            };
            Grid.SetColumn(border, col);

            var tb = new TextBlock
            {
                Text = text ?? "",
                FontSize = isHeader ? 10.5 : 10,
                FontWeight = weight,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = Brushes.Black
            };
            border.Child = tb;
            return border;
        }

        #region Event Handlers

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            _ = LoadDataAsync();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            _ = LoadDataAsync();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            RenderReportTable();
        }

        private void BtnTaiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnXem_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }

        private void PrintReport()
        {
            try
            {
                var printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintVisual(A4PageBorder, TxtReportTitle.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"{TxtReportTitle.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"\"{TxtCompanyName.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyAddress.Text}\"");
                    sb.AppendLine($"\"{TxtCompanyContact.Text}\"");
                    sb.AppendLine("");
                    sb.AppendLine($"\"{TxtReportTitle.Text}\"");
                    sb.AppendLine($"\"{TxtSubtitleGroup.Text}\"");
                    sb.AppendLine($"\"{TxtSubtitleUser.Text}\"");
                    sb.AppendLine("");

                    sb.AppendLine("Nhóm người dùng,Loại,Phân mục,STT,Quyền sử dụng,Xem,Thêm,Sửa,Xóa");

                    int stt = 1;
                    foreach (var item in _rawItems)
                    {
                        string xem = item.CanView ? "x" : "";
                        string them = item.CanAdd ? "x" : "";
                        string sua = item.CanEdit ? "x" : "";
                        string xoa = item.CanDelete ? "x" : "";

                        sb.AppendLine($"\"{item.GroupUserName?.Replace("\"", "\"\"")}\",\"{item.CategoryType?.Replace("\"", "\"\"")}\",\"{item.SubGroup?.Replace("\"", "\"\"")}\",\"{stt++}\",\"{item.FunctionName?.Replace("\"", "\"\"")}\",\"{xem}\",\"{them}\",\"{sua}\",\"{xoa}\"");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
