using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Data.SqlClient;

namespace QuanLyBar.Client.Services
{
    public class GroupUserItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private ImageSource _imageSource;

        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; } = 1;
        public string SimageId { get; set; } = "";
        public int? Sortorder { get; set; }
        public byte[] ImageBytes { get; set; }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

        public string IconDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "👥";
                string n = Name.ToLower().Trim();
                if (n == "tất cả") return "🌐";
                if (n.Contains("thu ngân")) return "⭐";
                if (n.Contains("thủ kho")) return "📋";
                if (n.Contains("kế toán")) return "🎟️";
                if (n.Contains("quản lý")) return "👤";
                if (n.Contains("phục vụ")) return "🏷️";
                if (n.Contains("bếp") || n.Contains("pha chế")) return "🍹";
                return "👥";
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class UserAccountItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private ImageSource _imageSource;

        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string SgroupuserId { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string DnhanvienId { get; set; } = "";
        public string NhanVienName { get; set; } = "";
        public string Note { get; set; } = "";
        public int? Status { get; set; } = 1;
        public string UserId { get; set; } = "";
        public string CardCode { get; set; } = "";
        public bool IsAdmin { get; set; } = false;
        public string SimageId { get; set; } = "";
        public byte[] ImageBytes { get; set; }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class UserShopItem : INotifyPropertyChanged
    {
        private bool _hasAccess = true;
        public string ShopId { get; set; } = "";
        public string ShopName { get; set; } = "";

        public bool HasAccess
        {
            get => _hasAccess;
            set { _hasAccess = value; OnPropertyChanged(nameof(HasAccess)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class FunctionRoleItem : INotifyPropertyChanged
    {
        private bool _isLocked;
        private bool _canView;
        private bool _canAdd;
        private bool _canEdit;
        private bool _canDelete;
        private bool _isAll;
        private bool _isUpdatingInternally;

        public string FunctionId { get; set; } = "";
        public string FunctionName { get; set; } = "";
        public string GroupName { get; set; } = "";
        public bool HasCrud { get; set; } = true;

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged(nameof(IsLocked));

                    if (!_isUpdatingInternally && value)
                    {
                        _isUpdatingInternally = true;
                        CanView = false;
                        CanAdd = false;
                        CanEdit = false;
                        CanDelete = false;
                        _isAll = false;
                        OnPropertyChanged(nameof(CanView));
                        OnPropertyChanged(nameof(CanAdd));
                        OnPropertyChanged(nameof(CanEdit));
                        OnPropertyChanged(nameof(CanDelete));
                        OnPropertyChanged(nameof(IsAll));
                        _isUpdatingInternally = false;
                    }
                }
            }
        }

        public bool CanView
        {
            get => _canView;
            set
            {
                if (_canView != value)
                {
                    _canView = value;
                    OnPropertyChanged(nameof(CanView));

                    if (!_isUpdatingInternally)
                    {
                        if (value && IsLocked)
                        {
                            _isUpdatingInternally = true;
                            IsLocked = false;
                            OnPropertyChanged(nameof(IsLocked));
                            _isUpdatingInternally = false;
                        }
                        UpdateAllState();
                    }
                }
            }
        }

        public bool CanAdd
        {
            get => _canAdd;
            set
            {
                if (_canAdd != value)
                {
                    _canAdd = value;
                    OnPropertyChanged(nameof(CanAdd));

                    if (!_isUpdatingInternally)
                    {
                        if (value && IsLocked)
                        {
                            _isUpdatingInternally = true;
                            IsLocked = false;
                            OnPropertyChanged(nameof(IsLocked));
                            _isUpdatingInternally = false;
                        }
                        if (value && !CanView)
                        {
                            _isUpdatingInternally = true;
                            CanView = true;
                            OnPropertyChanged(nameof(CanView));
                            _isUpdatingInternally = false;
                        }
                        UpdateAllState();
                    }
                }
            }
        }

        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                if (_canEdit != value)
                {
                    _canEdit = value;
                    OnPropertyChanged(nameof(CanEdit));

                    if (!_isUpdatingInternally)
                    {
                        if (value && IsLocked)
                        {
                            _isUpdatingInternally = true;
                            IsLocked = false;
                            OnPropertyChanged(nameof(IsLocked));
                            _isUpdatingInternally = false;
                        }
                        if (value && !CanView)
                        {
                            _isUpdatingInternally = true;
                            CanView = true;
                            OnPropertyChanged(nameof(CanView));
                            _isUpdatingInternally = false;
                        }
                        UpdateAllState();
                    }
                }
            }
        }

        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                if (_canDelete != value)
                {
                    _canDelete = value;
                    OnPropertyChanged(nameof(CanDelete));

                    if (!_isUpdatingInternally)
                    {
                        if (value && IsLocked)
                        {
                            _isUpdatingInternally = true;
                            IsLocked = false;
                            OnPropertyChanged(nameof(IsLocked));
                            _isUpdatingInternally = false;
                        }
                        if (value && !CanView)
                        {
                            _isUpdatingInternally = true;
                            CanView = true;
                            OnPropertyChanged(nameof(CanView));
                            _isUpdatingInternally = false;
                        }
                        UpdateAllState();
                    }
                }
            }
        }

        public bool IsAll
        {
            get => _isAll;
            set
            {
                if (_isAll != value)
                {
                    _isAll = value;
                    OnPropertyChanged(nameof(IsAll));

                    if (!_isUpdatingInternally)
                    {
                        _isUpdatingInternally = true;
                        if (value)
                        {
                            IsLocked = false;
                            CanView = true;
                            if (HasCrud)
                            {
                                CanAdd = true;
                                CanEdit = true;
                                CanDelete = true;
                            }
                        }
                        else
                        {
                            CanView = false;
                            CanAdd = false;
                            CanEdit = false;
                            CanDelete = false;
                        }
                        OnPropertyChanged(nameof(IsLocked));
                        OnPropertyChanged(nameof(CanView));
                        OnPropertyChanged(nameof(CanAdd));
                        OnPropertyChanged(nameof(CanEdit));
                        OnPropertyChanged(nameof(CanDelete));
                        _isUpdatingInternally = false;
                    }
                }
            }
        }

        private void UpdateAllState()
        {
            if (HasCrud)
            {
                bool allChecked = CanView && CanAdd && CanEdit && CanDelete;
                if (_isAll != allChecked)
                {
                    _isAll = allChecked;
                    OnPropertyChanged(nameof(IsAll));
                }
            }
            else
            {
                if (_isAll != CanView)
                {
                    _isAll = CanView;
                    OnPropertyChanged(nameof(IsAll));
                }
            }
        }

        public string GetModeString()
        {
            if (IsLocked) return "0";
            int mode = 0;
            if (CanView) mode |= 16;
            if (CanAdd && HasCrud) mode |= 32;
            if (CanEdit && HasCrud) mode |= 64;
            if (CanDelete && HasCrud) mode |= 128;
            return mode.ToString();
        }

        public void SetFromModeString(string mode)
        {
            _isUpdatingInternally = true;
            if (string.IsNullOrWhiteSpace(mode) || mode == "0" || mode.Trim().ToUpper() == "L")
            {
                IsLocked = true;
                CanView = false;
                CanAdd = false;
                CanEdit = false;
                CanDelete = false;
                _isAll = false;
            }
            else if (int.TryParse(mode.Trim(), out int modeVal))
            {
                if (modeVal == 0)
                {
                    IsLocked = true;
                    CanView = false;
                    CanAdd = false;
                    CanEdit = false;
                    CanDelete = false;
                    _isAll = false;
                }
                else
                {
                    IsLocked = false;
                    CanView = (modeVal & 16) != 0;
                    CanAdd = HasCrud && (modeVal & 32) != 0;
                    CanEdit = HasCrud && (modeVal & 64) != 0;
                    CanDelete = HasCrud && (modeVal & 128) != 0;
                    UpdateAllState();
                }
            }
            else
            {
                string m = mode.ToUpper();
                IsLocked = false;
                CanView = m.Contains("V");
                CanAdd = HasCrud && m.Contains("A");
                CanEdit = HasCrud && m.Contains("E");
                CanDelete = HasCrud && m.Contains("D");
                UpdateAllState();
            }
            _isUpdatingInternally = false;
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(CanView));
            OnPropertyChanged(nameof(CanAdd));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(IsAll));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class CategoryItem
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "📁";
        public ObservableCollection<CategoryItem> Children { get; set; } = new ObservableCollection<CategoryItem>();
    }

    public class ReportCategoryNode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string ParentId { get; set; } = "";
        public string Icon { get; set; } = "📁";
        public ObservableCollection<ReportCategoryNode> Children { get; set; } = new ObservableCollection<ReportCategoryNode>();
    }

    public class ReportRoleItem : INotifyPropertyChanged
    {
        private bool _notAllowed;
        private bool _isAllowed;
        private bool _isUpdatingInternally;

        public string ReportId { get; set; } = "";
        public string ReportName { get; set; } = "";
        public string GroupName { get; set; } = "";
        public string ParentId { get; set; } = "";

        public bool NotAllowed
        {
            get => _notAllowed;
            set
            {
                if (_notAllowed != value)
                {
                    _notAllowed = value;
                    OnPropertyChanged(nameof(NotAllowed));

                    if (!_isUpdatingInternally && value)
                    {
                        _isUpdatingInternally = true;
                        IsAllowed = false;
                        OnPropertyChanged(nameof(IsAllowed));
                        _isUpdatingInternally = false;
                    }
                }
            }
        }

        public bool IsAllowed
        {
            get => _isAllowed;
            set
            {
                if (_isAllowed != value)
                {
                    _isAllowed = value;
                    OnPropertyChanged(nameof(IsAllowed));

                    if (!_isUpdatingInternally && value)
                    {
                        _isUpdatingInternally = true;
                        NotAllowed = false;
                        OnPropertyChanged(nameof(NotAllowed));
                        _isUpdatingInternally = false;
                    }
                }
            }
        }

        public string GetModeString()
        {
            if (IsAllowed) return "30";
            return "0";
        }

        public void SetFromModeString(string mode)
        {
            _isUpdatingInternally = true;
            if (!string.IsNullOrWhiteSpace(mode) && (mode == "30" || mode.ToUpper().Contains("V") || (int.TryParse(mode, out int v) && v > 0)))
            {
                IsAllowed = true;
                NotAllowed = false;
            }
            else
            {
                IsAllowed = false;
                NotAllowed = true;
            }
            _isUpdatingInternally = false;
            OnPropertyChanged(nameof(NotAllowed));
            OnPropertyChanged(nameof(IsAllowed));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalPhanQuyenService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        private static async Task<string> GetCurrentUserIdAsync(IDbConnection conn, IDbTransaction trans = null)
        {
            try
            {
                var u = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0", transaction: trans);
                if (u != null && u != DBNull.Value && !string.IsNullOrWhiteSpace(u.ToString()))
                    return u.ToString();
            }
            catch { }
            return "4f1466a0-0756-4ba9-afa8-053b96ca7569";
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

        #region Runtime Permission Checking
        private static readonly Dictionary<string, int> _userFunctionPermissions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _userReportPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static bool _isCurrentUserAdmin = false;

        public static async Task LoadCurrentUserPermissionsAsync(string userId, string groupId, bool isAdmin)
        {
            _userFunctionPermissions.Clear();
            _userReportPermissions.Clear();
            _isCurrentUserAdmin = isAdmin;

            if (isAdmin || string.IsNullOrEmpty(groupId) || groupId == "0")
            {
                return;
            }

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Lấy quyền chức năng từ SGROUPROLE + SFUNCTION
                    var funcRoles = (await conn.QueryAsync(@"
                        SELECT f.NAME, r.MODE 
                        FROM SGROUPROLE r 
                        JOIN SFUNCTION f ON r.SFUNCTIONID = f.ID 
                        WHERE r.SGROUPUSERID = @GroupId AND (r.STATUS IS NULL OR r.STATUS <> 0) AND (f.STATUS IS NULL OR f.STATUS <> 0)
                    ", new { GroupId = groupId })).ToList();

                    foreach (object row in funcRoles)
                    {
                        var dict = row as IDictionary<string, object>;
                        string name = GetValue(dict, "NAME")?.ToString()?.Trim() ?? "";
                        int mode = int.TryParse(GetValue(dict, "MODE")?.ToString(), out int m) ? m : 0;
                        if (!string.IsNullOrEmpty(name))
                        {
                            _userFunctionPermissions[name] = mode;
                        }
                    }

                    // Lấy quyền xem báo cáo từ SREPORTROLE + SREPORT
                    var repRoles = (await conn.QueryAsync(@"
                        SELECT rep.NAME, r.MODE 
                        FROM SREPORTROLE r 
                        JOIN SREPORT rep ON r.SREPORTID = rep.ID 
                        WHERE r.SGROUPUSERID = @GroupId AND (r.STATUS IS NULL OR r.STATUS <> 0) AND (rep.STATUS IS NULL OR rep.STATUS <> 0)
                    ", new { GroupId = groupId })).ToList();

                    foreach (object row in repRoles)
                    {
                        var dict = row as IDictionary<string, object>;
                        string name = GetValue(dict, "NAME")?.ToString()?.Trim() ?? "";
                        int mode = int.TryParse(GetValue(dict, "MODE")?.ToString(), out int m) ? m : 0;
                        if (!string.IsNullOrEmpty(name) && mode == 30)
                        {
                            _userReportPermissions.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LoadCurrentUserPermissionsAsync: " + ex.Message);
            }
        }

        private static string NormalizeFunctionName(string funcName)
        {
            if (string.IsNullOrWhiteSpace(funcName)) return "";
            string f = funcName.Trim();
            if (string.Equals(f, "Sử dụng dịch vụ", StringComparison.OrdinalIgnoreCase)) return "Hóa đơn bán hàng";
            if (string.Equals(f, "Khách đặt hàng", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(f, "Theo dõi đặt phòng", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f, "Đặt phòng", StringComparison.OrdinalIgnoreCase))
            {
                return "Đặt hàng";
            }
            if (string.Equals(f, "Thống kê bán hàng", StringComparison.OrdinalIgnoreCase)) return "Thống kê mặt hàng bán";
            if (string.Equals(f, "Tổng hợp KQKD", StringComparison.OrdinalIgnoreCase)) return "Tổng hợp kết quả kinh doanh";
            if (string.Equals(f, "Chi tiết hoạt động", StringComparison.OrdinalIgnoreCase)) return "Chi tiết hoạt động ngày";
            if (string.Equals(f, "Thưởng phạt", StringComparison.OrdinalIgnoreCase)) return "Quản lý thưởng phạt";
            if (string.Equals(f, "Quản lý chuyển kho", StringComparison.OrdinalIgnoreCase)) return "Chuyển kho";
            if (string.Equals(f, "Tạo phiếu thu", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(f, "Tạo phiếu chi", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f, "Danh mục phiếu thu", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f, "Danh mục phiếu chi", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f, "Phiếu thu", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f, "Phiếu chi", StringComparison.OrdinalIgnoreCase))
            {
                return "Danh mục phiếu thu chi";
            }
            return f;
        }

        private static bool IsCurrentUserAdmin()
        {
            if (_isCurrentUserAdmin) return true;

            var user = SessionContext.CurrentUser;
            if (user == null) return false;
            if (user.IsAdmin) return true;

            string username = user.TenDangNhap?.Trim() ?? "";
            if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase)) return true;

            string role = user.VaiTro?.Trim() ?? "";
            string groupName = user.GroupName?.Trim() ?? "";
            return string.Equals(role, "Quản trị viên", StringComparison.OrdinalIgnoreCase)
                || string.Equals(groupName, "Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasFunctionPermission(string funcName, string action = "View")
        {
            if (IsCurrentUserAdmin()) return true;
            if (SessionContext.CurrentUser == null) return true; // Standalone / Dev mode

            if (string.IsNullOrWhiteSpace(funcName)) return true;

            string normalized = NormalizeFunctionName(funcName);

            if (!_userFunctionPermissions.TryGetValue(normalized, out int mode) && 
                !_userFunctionPermissions.TryGetValue(funcName.Trim(), out mode))
            {
                // Chưa được phân quyền -> Mặc định khóa
                return false;
            }

            if (mode == 0) return false;

            switch (action?.Trim()?.ToLower())
            {
                case "add":
                case "them":
                    return (mode & 32) != 0;
                case "edit":
                case "sua":
                    return (mode & 64) != 0;
                case "delete":
                case "xoa":
                    return (mode & 128) != 0;
                case "all":
                case "tatca":
                    return (mode & 240) == 240;
                case "view":
                case "xem":
                default:
                    return (mode & 16) != 0;
            }
        }

        public static bool HasReportPermission(string reportName)
        {
            if (IsCurrentUserAdmin()) return true;
            if (SessionContext.CurrentUser == null) return true;

            if (string.IsNullOrWhiteSpace(reportName)) return true;
            return _userReportPermissions.Contains(reportName.Trim());
        }

        public static bool CheckPermissionAndAlert(string funcName, string action = "View", Window owner = null)
        {
            if (string.IsNullOrWhiteSpace(funcName)) return true;
            if (funcName.Equals("Báo cáo", StringComparison.OrdinalIgnoreCase) || 
                funcName.Equals("Ghi chú", StringComparison.OrdinalIgnoreCase) ||
                funcName.Equals("Ghi chú nhanh", StringComparison.OrdinalIgnoreCase))
                return true;

            bool isReport = MasterReports.Any(r => r.Name.Equals(funcName, StringComparison.OrdinalIgnoreCase));
            bool hasPerm = isReport ? HasReportPermission(funcName) : HasFunctionPermission(funcName, action);
            if (!hasPerm)
            {
                MessageBox.Show("Bạn không có quyền sử dụng chức năng này! Mời bạn liên hệ với quản trị để xử lý.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        #endregion


        #region Master Function & Report Definitions
        public static readonly List<(int Id, string Name, string GroupName, bool HasCrud)> MasterFunctions = new List<(int, string, string, bool)>
        {
            // Bán hàng
            (1, "Sử dụng dịch vụ", "Bán hàng", false),
            (2, "Khách đặt hàng", "Bán hàng", true),
            (3, "Theo dõi đặt phòng", "Bán hàng", false),
            (4, "Điều chỉnh hóa đơn", "Bán hàng", false),
            (5, "Quản lý bán hàng", "Bán hàng", true),
            (6, "Hóa đơn bán hàng", "Bán hàng", false),
            (7, "Hủy hóa đơn", "Bán hàng", false),
            (8, "Thanh toán", "Bán hàng", false),
            (9, "In tạm tính", "Bán hàng", false),
            (10, "In lại bill", "Bán hàng", false),
            (11, "Gộp bàn", "Bán hàng", false),
            (12, "Sửa đơn giá", "Bán hàng", false),
            (13, "Giảm giá mặt hàng", "Bán hàng", false),
            (14, "Thay đổi giảm giá tổng hóa đơn", "Bán hàng", false),
            (15, "Thay đổi phí dịch vụ", "Bán hàng", false),
            (16, "Được giảm đồ sau khi in tạm tính", "Bán hàng", false),
            (17, "Xóa giảm món sau khi in chế biến", "Bán hàng", false),
            (18, "Kiểm soát order", "Bán hàng", false),
            (19, "Sử dụng màn hình bếp", "Bán hàng", false),
            (20, "Xem đơn giá trong hóa đơn bán hàng", "Bán hàng", false),
            (21, "Xem giá nhập", "Bán hàng", false),
            (22, "Đổi giờ mặt hàng dịch vụ theo giờ", "Bán hàng", false),

            // Kho hàng
            (23, "Danh mục kho hàng", "Kho hàng", true),
            (24, "Nhập kho", "Kho hàng", true),
            (25, "Xuất kho", "Kho hàng", true),
            (26, "Quản lý chuyển kho", "Kho hàng", true),
            (27, "Kiểm kê kho", "Kho hàng", true),
            (28, "Tồn kho", "Kho hàng", false),
            (29, "Tính lại giá vốn", "Kho hàng", false),
            (30, "Xuất lại định lượng", "Kho hàng", false),

            // Danh mục
            (31, "Danh mục mặt hàng", "Danh mục", true),
            (32, "Danh mục bàn khu vực", "Danh mục", true),
            (33, "Danh mục bảng giá", "Danh mục", true),
            (34, "Danh mục ca làm việc", "Danh mục", true),
            (35, "Danh mục chỉ tiêu doanh thu", "Danh mục", true),
            (36, "Danh mục cửa hàng", "Danh mục", true),
            (37, "Danh mục đợt khuyến mại", "Danh mục", true),
            (38, "Danh mục hóa đơn hủy", "Danh mục", true),
            (39, "Danh mục khách hàng", "Danh mục", true),
            (40, "Danh mục nhà cung cấp", "Danh mục", true),
            (41, "Danh mục nhân viên", "Danh mục", true),
            (42, "Danh mục lý do thu chi", "Danh mục", true),
            (43, "Danh mục lý do hủy", "Danh mục", true),
            (44, "Danh mục mật khẩu wifi", "Danh mục", true),
            (45, "Danh mục tài khoản ngân hàng", "Danh mục", true),
            (46, "Danh mục thẻ trả trước", "Danh mục", true),
            (47, "Danh mục voucher", "Danh mục", true),
            (48, "Khách hàng thân thiết", "Danh mục", true),
            (49, "Gửi tin nhắn tới khách hàng", "Danh mục", false),

            // Công nợ
            (50, "Công nợ khách hàng", "Công nợ", false),
            (51, "Công nợ nhà cung cấp", "Công nợ", false),

            // Quỹ
            (52, "Tạo phiếu thu", "Quỹ", false),
            (53, "Tạo phiếu chi", "Quỹ", false),
            (54, "Danh mục phiếu thu", "Quỹ", true),
            (55, "Danh mục phiếu chi", "Quỹ", true),
            (56, "Tồn quỹ", "Quỹ", false),
            (57, "Thu chi", "Quỹ", false),
            (58, "Thu tạm ứng khách hàng", "Quỹ", false),
            (59, "Thay đổi ngày trong phiếu thu chi", "Quỹ", false),
            (60, "Thay đổi ngày trong các phiếu", "Quỹ", false),

            // Nhân sự
            (61, "Tạm ứng lương", "Nhân sự", true),
            (62, "Thưởng phạt", "Nhân sự", true),
            (63, "Chấm công", "Nhân sự", true),
            (64, "Tính lương", "Nhân sự", false),
            (65, "Điều chỉnh giờ tính lương dịch vụ theo...", "Nhân sự", false),

            // Báo cáo
            (66, "Thống kê doanh thu", "Báo cáo", false),
            (67, "Thống kê mặt hàng bán", "Báo cáo", false),
            (68, "Thống kê trong sử dụng dịch vụ", "Báo cáo", false),
            (69, "Tổng hợp kết quả kinh doanh", "Báo cáo", false),
            (70, "Chi tiết hoạt động ngày", "Báo cáo", false),
            (71, "Xem thống kê của các tài khoản khác", "Báo cáo", false),

            // Cơ sở dữ liệu
            (72, "Sao lưu cơ sở dữ liệu", "Cơ sở dữ liệu", false),
            (73, "Phục hồi cơ sở dữ liệu", "Cơ sở dữ liệu", false),
            (74, "Tạo mới cơ sở dữ liệu", "Cơ sở dữ liệu", false),

            // Hệ thống & Quản trị
            (75, "Đăng nhập", "Hệ thống", false),
            (76, "Lưu vết hoạt động", "Hệ thống", false),
            (77, "Thông báo sửa chữa", "Hệ thống", false),
            (78, "Thư viện ảnh", "Quản trị", false),
            (79, "Quản lý người dùng", "Quản trị", true),
            (80, "Dữ liệu ban đầu", "Quản trị", false),
            (81, "Xóa dữ liệu", "Quản trị", false)
        };

        public static readonly List<(int Id, string Name, string GroupName)> MasterReports = new List<(int, string, string)>
        {
            // BÁO CÁO QUỸ
            (1, "DANH SÁCH PHIẾU THU THEO NGÀY", "BÁO CÁO QUỸ"),
            (2, "DANH SÁCH PHIẾU THU THEO LÝ DO THU CHI", "BÁO CÁO QUỸ"),
            (3, "DANH SÁCH PHIẾU CHI THEO NGÀY", "BÁO CÁO QUỸ"),
            (4, "DANH SÁCH PHIẾU CHI THEO LÝ DO THU CHI", "BÁO CÁO QUỸ"),
            (5, "TỔNG HỢP THU CHI THEO NGÀY", "BÁO CÁO QUỸ"),
            (6, "TỔNG HỢP THU CHI THEO LÝ DO", "BÁO CÁO QUỸ"),
            (7, "BÁO CÁO TỒN QUỸ", "BÁO CÁO QUỸ"),

            // BÁO CÁO DANH MỤC
            (8, "DANH SÁCH KHÁCH HÀNG THEO NHÓM", "BÁO CÁO DANH MỤC"),
            (9, "DANH SÁCH KHÁCH HÀNG THEO NHÂN VIÊN", "BÁO CÁO DANH MỤC"),
            (10, "DANH SÁCH NHÀ CUNG CẤP THEO NHÓM", "BÁO CÁO DANH MỤC"),
            (11, "DANH SÁCH ĐỢT KHUYẾN MẠI", "BÁO CÁO DANH MỤC"),
            (12, "DANH SÁCH MẶT HÀNG THEO NHÓM", "BÁO CÁO DANH MỤC"),
            (13, "DANH SÁCH MẶT HÀNG THEO HÃNG SẢN XUẤT", "BÁO CÁO DANH MỤC"),
            (14, "KHÁCH HÀNG ĐẾN NGÀY SINH NHẬT", "BÁO CÁO DANH MỤC"),
            (15, "BÁO CÁO CẤU HÌNH BÀN KHU VỰC", "BÁO CÁO DANH MỤC"),
            (16, "CÔNG THỨC ĐỊNH LƯỢNG", "BÁO CÁO DANH MỤC"),
            (17, "BÁO CÁO CHI TIẾT PHÂN QUYỀN HỆ THỐNG", "BÁO CÁO DANH MỤC"),

            // BÁO CÁO BÁN HÀNG
            (18, "BÁO CÁO 20 MẶT HÀNG BÁN CHẠY NHẤT", "BÁO CÁO BÁN HÀNG"),
            (19, "BÁO CÁO BÁN HÀNG", "BÁO CÁO BÁN HÀNG"),
            (20, "BÁO CÁO BÁN HÀNG THEO GIỜ", "BÁO CÁO BÁN HÀNG"),
            (21, "BÁO CÁO BÁN HÀNG THEO KHÁCH HÀNG", "BÁO CÁO BÁN HÀNG"),
            (22, "BÁO CÁO BÁN HÀNG THEO NGÀY", "BÁO CÁO BÁN HÀNG"),
            (23, "BÁO CÁO BÁN HÀNG THEO NHÂN VIÊN", "BÁO CÁO BÁN HÀNG"),
            (24, "BÁO CÁO BÁN HÀNG THEO THU NGÂN", "BÁO CÁO BÁN HÀNG"),
            (25, "BÁO CÁO CHI TIẾT BÁN HÀNG THEO NGÀY", "BÁO CÁO BÁN HÀNG"),
            (26, "BÁO CÁO CHI TIẾT BÁN HÀNG THEO THU NGÂN", "BÁO CÁO BÁN HÀNG"),
            (27, "BÁO CÁO CHI TIẾT HÀNG KHUYẾN MẠI", "BÁO CÁO BÁN HÀNG"),
            (28, "TỔNG HỢP BÁN HÀNG THEO NGÀY", "BÁO CÁO BÁN HÀNG"),
            (29, "TỔNG HỢP MẶT HÀNG BÁN THEO NGÀY", "BÁO CÁO BÁN HÀNG"),
            (30, "TỔNG HỢP DOANH THU CHƯA THANH TOÁN", "BÁO CÁO BÁN HÀNG"),
            (31, "TỔNG HỢP HOA HỒNG THEO NVKD", "BÁO CÁO BÁN HÀNG"),
            (32, "CHI TIẾT BÁN HÀNG THEO HÓA ĐƠN", "BÁO CÁO BÁN HÀNG"),
            (33, "BÁO CÁO TỔNG HỢP GIÁ TRỊ BÁN THEO THÁNG", "BÁO CÁO BÁN HÀNG"),
            (34, "BÁO CÁO TỔNG HỢP SỐ LƯỢNG BÁN THEO THÁNG", "BÁO CÁO BÁN HÀNG"),

            // BÁO CÁO ĐẶT HÀNG
            (35, "DANH SÁCH ĐẶT HÀNG THEO NGÀY", "BÁO CÁO ĐẶT HÀNG"),
            (36, "DANH SÁCH ĐẶT HÀNG THEO KHÁCH HÀNG", "BÁO CÁO ĐẶT HÀNG"),
            (37, "TỔNG HỢP ĐẶT HÀNG THEO NGÀY", "BÁO CÁO ĐẶT HÀNG"),
            (38, "TỔNG HỢP ĐẶT HÀNG THEO KHÁCH HÀNG", "BÁO CÁO ĐẶT HÀNG"),
            (39, "TỔNG HỢP MẶT HÀNG ĐẶT THEO NGÀY", "BÁO CÁO ĐẶT HÀNG"),

            // BÁO CÁO KHO HÀNG
            (40, "TỔNG HỢP MẶT HÀNG XUẤT BÁN THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (41, "BÁO CÁO MẶT HÀNG BÁN THEO ĐƠN HÀNG", "BÁO CÁO KHO HÀNG"),
            (42, "BÁO CÁO XUẤT ĐỊNH LƯỢNG MẶT HÀNG THEO ĐƠN HÀNG", "BÁO CÁO KHO HÀNG"),
            (43, "DANH SÁCH PHIẾU KIỂM KÊ THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (44, "TỔNG HỢP MẶT HÀNG KIỂM KÊ THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (45, "TỔNG HỢP MẶT HÀNG KIỂM KÊ THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (46, "DANH SÁCH PHIẾU KIỂM KÊ THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (47, "TỔNG HỢP MẶT HÀNG XUẤT KHÁC THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (48, "TỔNG HỢP MẶT HÀNG XUẤT KHÁC THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (49, "DANH SÁCH PHIẾU XUẤT KHÁC THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (50, "TỔNG HỢP XUẤT KHÁC THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (51, "TỔNG HỢP XUẤT KHÁC THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (52, "DANH SÁCH PHIẾU XUẤT KHÁC THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (53, "DANH SÁCH PHIẾU NHẬP HÀNG THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (54, "DANH SÁCH PHIẾU NHẬP HÀNG THEO NHÀ CUNG CẤP", "BÁO CÁO KHO HÀNG"),
            (55, "DANH SÁCH PHIẾU NHẬP HÀNG THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (56, "TỔNG HỢP MẶT HÀNG NHẬP THEO NHÀ CUNG CẤP", "BÁO CÁO KHO HÀNG"),
            (57, "TỔNG HỢP MẶT HÀNG NHẬP THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (58, "TỔNG HỢP NHẬP HÀNG THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (59, "TỔNG HỢP NHẬP HÀNG THEO NHÀ CUNG CẤP", "BÁO CÁO KHO HÀNG"),
            (60, "TỔNG HỢP NHẬP HÀNG THEO NHÂN VIÊN", "BÁO CÁO KHO HÀNG"),
            (61, "TỔNG HỢP MẶT HÀNG NHẬP THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (62, "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NHÂN VIÊN NHẬN", "BÁO CÁO KHO HÀNG"),
            (63, "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NHÂN VIÊN CHUYỂN", "BÁO CÁO KHO HÀNG"),
            (64, "TỔNG HỢP MẶT HÀNG CHUYỂN KHO THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (65, "DANH SÁCH PHIẾU CHUYỂN KHO THEO NHÂN VIÊN XUẤT", "BÁO CÁO KHO HÀNG"),
            (66, "DANH SÁCH PHIẾU CHUYỂN KHO THEO NHÂN VIÊN NHẬP", "BÁO CÁO KHO HÀNG"),
            (67, "DANH SÁCH PHIẾU CHUYỂN KHO THEO NGÀY", "BÁO CÁO KHO HÀNG"),
            (68, "BÁO CÁO HÀNG HÓA THEO HẠN DÙNG", "BÁO CÁO KHO HÀNG"),
            (69, "BÁO CÁO HÀNG HÓA ĐÃ HẾT HẠN DÙNG", "BÁO CÁO KHO HÀNG"),
            (70, "BÁO CÁO HÀNG TỒN KHO CÓ HẠN DÙNG", "BÁO CÁO KHO HÀNG"),
            (71, "BÁO CÁO HÀNG TỒN KHO", "BÁO CÁO KHO HÀNG"),
            (72, "BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN", "BÁO CÁO KHO HÀNG"),
            (73, "BÁO CÁO TỔNG HỢP XUẤT NHẬP TỒN CHI TIẾT", "BÁO CÁO KHO HÀNG"),
            (74, "THẺ KHO", "BÁO CÁO KHO HÀNG"),

            // BÁO CÁO CÔNG NỢ
            (48, "ĐỐI CHIẾU CÔNG NỢ NHÀ CUNG CẤP", "BÁO CÁO CÔNG NỢ"),
            (49, "BÁO CÁO CÔNG NỢ NHÀ CUNG CẤP", "BÁO CÁO CÔNG NỢ"),
            (50, "ĐỐI CHIẾU CÔNG NỢ KHÁCH HÀNG", "BÁO CÁO CÔNG NỢ"),
            (51, "TỔNG HỢP CÔNG NỢ KHÁCH HÀNG", "BÁO CÁO CÔNG NỢ"),
            (52, "TỔNG HỢP CÔNG NỢ NHÀ CUNG CẤP", "BÁO CÁO CÔNG NỢ"),
            (53, "BÁO CÁO CÔNG NỢ KHÁCH HÀNG", "BÁO CÁO CÔNG NỢ"),

            // BÁO CÁO QUẢN TRỊ
            (54, "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ VỐN)", "BÁO CÁO QUẢN TRỊ"),
            (55, "TỔNG HỢP LÃI GỘP THEO MẶT HÀNG (GIÁ NHẬP)", "BÁO CÁO QUẢN TRỊ"),
            (56, "CHI TIẾT LÃI THEO HÓA ĐƠN (GIÁ NHẬP)", "BÁO CÁO QUẢN TRỊ"),
            (57, "CHI TIẾT LÃI THEO HÓA ĐƠN (GIÁ VỐN)", "BÁO CÁO QUẢN TRỊ"),
            (58, "BÁO CÁO KẾT QUẢ KINH DOANH", "BÁO CÁO QUẢN TRỊ"),
            (59, "CHI TIẾT HOẠT ĐỘNG TRONG NGÀY", "BÁO CÁO QUẢN TRỊ"),
            (60, "PHÂN TÍCH TÌNH HÌNH BÁN HÀNG", "BÁO CÁO QUẢN TRỊ"),
            (61, "DANH SÁCH MÓN XÓA, GIẢM, TRẢ LẠI", "BÁO CÁO QUẢN TRỊ"),

            // BÁO CÁO BIỂU ĐỒ
            (62, "BIỂU ĐỒ DOANH SỐ THEO NHÓM", "BÁO CÁO BIỂU ĐỒ"),
            (63, "BIỂU ĐỒ DOANH THU NGÀY TRONG THÁNG", "BÁO CÁO BIỂU ĐỒ"),
            (64, "BIỂU ĐỒ DOANH THU THÁNG TRONG NĂM", "BÁO CÁO BIỂU ĐỒ"),
            (65, "BIỂU ĐỒ THEO NHÂN VIÊN KINH DOANH", "BÁO CÁO BIỂU ĐỒ")
        };
        #endregion

        #region Nhóm người dùng (SGROUPUSER)
        public static async Task<List<GroupUserItem>> GetGroupUsersAsync()
        {
            var list = new List<GroupUserItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT g.ID, g.NAME, g.NOTE, g.STATUS, g.SORTORDER, g.SIMAGEID, img.IMAGE as IMAGE_BYTES
                        FROM SGROUPUSER g
                        LEFT JOIN SIMAGE img ON CAST(g.SIMAGEID AS VARCHAR(50)) = CAST(img.ID AS VARCHAR(50))
                        ORDER BY g.SORTORDER, g.NAME";

                    var rows = (await conn.QueryAsync(sql)).ToList();
                    foreach (object r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string name = GetValue(dict, "NAME")?.ToString() ?? "";
                        string note = GetValue(dict, "NOTE")?.ToString() ?? "";
                        string simageId = GetValue(dict, "SIMAGEID")?.ToString() ?? "";
                        byte[] imgBytes = GetValue(dict, "IMAGE_BYTES") as byte[];
                        
                        int? status = null;
                        var rawStatus = GetValue(dict, "STATUS");
                        if (rawStatus != null && int.TryParse(rawStatus.ToString(), out int sVal)) status = sVal;

                        int? sortorder = null;
                        var rawSort = GetValue(dict, "SORTORDER");
                        if (rawSort != null && int.TryParse(rawSort.ToString(), out int stVal)) sortorder = stVal;

                        if (status != 0 && !string.IsNullOrEmpty(name))
                        {
                            list.Add(new GroupUserItem
                            {
                                Id = id,
                                Name = name,
                                Note = note,
                                Status = status,
                                SimageId = simageId,
                                Sortorder = sortorder,
                                ImageBytes = imgBytes,
                                ImageSource = LocalThuVienAnhService.BytesToBitmapImage(imgBytes)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetGroupUsersAsync: " + ex.Message);
            }

            // Nếu DB không có nhóm nào, trả về danh sách nhóm mẫu
            if (list.Count == 0)
            {
                list.Add(new GroupUserItem { Id = "1", Name = "Thu ngân", Note = "Thu ngân", Sortorder = 1 });
                list.Add(new GroupUserItem { Id = "2", Name = "Thủ kho", Note = "Thủ kho", Sortorder = 2 });
                list.Add(new GroupUserItem { Id = "3", Name = "Kế toán", Note = "Kế toán", Sortorder = 3 });
                list.Add(new GroupUserItem { Id = "4", Name = "Quản lý", Note = "Quản lý", Sortorder = 4 });
                list.Add(new GroupUserItem { Id = "5", Name = "Phục vụ", Note = "Phục vụ", Sortorder = 5 });
            }

            return list.OrderBy(x => x.Sortorder ?? 999).ThenBy(x => x.Name).ToList();
        }

        public static async Task<bool> SaveGroupUserAsync(GroupUserItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string currentUserId = await GetCurrentUserIdAsync(conn);
                    object simageParam = string.IsNullOrWhiteSpace(item.SimageId) ? null : item.SimageId.Trim();

                    if (string.IsNullOrEmpty(item.Id) || item.Id == "0")
                    {
                        string newId = Guid.NewGuid().ToString();
                        string sqlInsert = "INSERT INTO SGROUPUSER (ID, NAME, NOTE, STATUS, SORTORDER, SIMAGEID, USERCREATEDID, TIMECREATED) VALUES (@Id, @Name, @Note, 30, @Sortorder, @SimageId, @UserCreatedId, CURRENT_TIMESTAMP)";
                        if (conn is SqlConnection) sqlInsert = "INSERT INTO SGROUPUSER (ID, NAME, NOTE, STATUS, SORTORDER, SIMAGEID, USERCREATEDID, TIMECREATED) VALUES (@Id, @Name, @Note, 30, @Sortorder, @SimageId, @UserCreatedId, GETDATE())";

                        await conn.ExecuteAsync(sqlInsert, new
                        {
                            Id = newId,
                            Name = item.Name.Trim(),
                            Note = item.Note?.Trim() ?? "",
                            Sortorder = item.Sortorder ?? 99,
                            SimageId = simageParam,
                            UserCreatedId = currentUserId
                        });
                        item.Id = newId;
                    }
                    else
                    {
                        string sqlUpdate = "UPDATE SGROUPUSER SET NAME = @Name, NOTE = @Note, SIMAGEID = @SimageId, USERMODIFIEDID = @UserModifiedId, TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = @Id";
                        if (conn is SqlConnection) sqlUpdate = "UPDATE SGROUPUSER SET NAME = @Name, NOTE = @Note, SIMAGEID = @SimageId, USERMODIFIEDID = @UserModifiedId, TIMEMODIFIED = GETDATE() WHERE ID = @Id";

                        await conn.ExecuteAsync(sqlUpdate, new
                        {
                            Id = item.Id,
                            Name = item.Name.Trim(),
                            Note = item.Note?.Trim() ?? "",
                            SimageId = simageParam,
                            UserModifiedId = currentUserId
                        });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveGroupUserAsync: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> DeleteGroupUserAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId) || groupId == "0") return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var userCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM SUSER WHERE SGROUPUSERID = @GroupId", new { GroupId = groupId });
                    if (userCount > 0)
                    {
                        throw new Exception($"Không thể xóa nhóm này vì đang có {userCount} tài khoản người dùng trực thuộc.");
                    }

                    await conn.ExecuteAsync("DELETE FROM SGROUPROLE WHERE SGROUPUSERID = @GroupId", new { GroupId = groupId });
                    await conn.ExecuteAsync("DELETE FROM SREPORTROLE WHERE SGROUPUSERID = @GroupId", new { GroupId = groupId });
                    await conn.ExecuteAsync("DELETE FROM SGROUPUSER WHERE ID = @GroupId", new { GroupId = groupId });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteGroupUserAsync: " + ex.Message);
                throw;
            }
        }
        #endregion

        #region Tài khoản người dùng (SUSER)
        public static async Task<List<UserAccountItem>> GetUsersAsync(string groupId = null)
        {
            var list = new List<UserAccountItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT u.ID, u.USERNAME, u.NAME, u.PASSWORD, u.EMAIL, u.SGROUPUSERID, u.DNHANVIENID, u.NOTE, u.STATUS, u.ISADMIN, u.SIMAGEID,
                               u.USERID, u.CARDCODE,
                               img.IMAGE as IMAGE_BYTES, g.NAME as GROUP_NAME, n.NAME as NHANVIEN_NAME
                        FROM SUSER u
                        LEFT JOIN SGROUPUSER g ON u.SGROUPUSERID = g.ID
                        LEFT JOIN DNHANVIEN n ON u.DNHANVIENID = n.ID
                        LEFT JOIN SIMAGE img ON CAST(u.SIMAGEID AS VARCHAR(50)) = CAST(img.ID AS VARCHAR(50))
                        WHERE (u.STATUS IS NULL OR u.STATUS <> 0)";

                    if (!string.IsNullOrEmpty(groupId) && groupId != "0")
                    {
                        sql += " AND u.SGROUPUSERID = @GroupId";
                    }

                    var rows = (await conn.QueryAsync(sql, new { GroupId = groupId })).ToList();

                    foreach (object r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string username = GetValue(dict, "USERNAME")?.ToString() ?? "";
                        string name = GetValue(dict, "NAME")?.ToString() ?? "";
                        string password = GetValue(dict, "PASSWORD")?.ToString() ?? "";
                        string email = GetValue(dict, "EMAIL")?.ToString() ?? "";
                        string sgroupuserId = GetValue(dict, "SGROUPUSERID")?.ToString() ?? "";
                        string groupName = GetValue(dict, "GROUP_NAME")?.ToString() ?? "";
                        string dnhanvienId = GetValue(dict, "DNHANVIENID")?.ToString() ?? "";
                        string nhanVienName = GetValue(dict, "NHANVIEN_NAME")?.ToString() ?? "";
                        string note = GetValue(dict, "NOTE")?.ToString() ?? "";
                        string simageId = GetValue(dict, "SIMAGEID")?.ToString() ?? "";
                        string userId = GetValue(dict, "USERID")?.ToString() ?? "";
                        string cardCode = GetValue(dict, "CARDCODE")?.ToString() ?? "";
                        byte[] imgBytes = GetValue(dict, "IMAGE_BYTES") as byte[];

                        int? status = null;
                        var rawStatus = GetValue(dict, "STATUS");
                        if (rawStatus != null && int.TryParse(rawStatus.ToString(), out int sVal)) status = sVal;

                        bool isAdmin = false;
                        var rawAdmin = GetValue(dict, "ISADMIN");
                        if (rawAdmin != null)
                        {
                            if (bool.TryParse(rawAdmin.ToString(), out bool bAdmin)) isAdmin = bAdmin;
                            else if (int.TryParse(rawAdmin.ToString(), out int iAdmin)) isAdmin = (iAdmin == 1);
                        }

                        if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        list.Add(new UserAccountItem
                        {
                            Id = id,
                            Username = username,
                            Name = name,
                            Password = password,
                            Email = email,
                            SgroupuserId = sgroupuserId,
                            GroupName = groupName,
                            DnhanvienId = dnhanvienId,
                            NhanVienName = nhanVienName,
                            Note = note,
                            Status = status,
                            IsAdmin = isAdmin,
                            SimageId = simageId,
                            UserId = userId,
                            CardCode = cardCode,
                            ImageBytes = imgBytes,
                            ImageSource = LocalThuVienAnhService.BytesToBitmapImage(imgBytes)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetUsersAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<List<UserShopItem>> GetUserShopAccessListAsync(string userId)
        {
            var list = new List<UserShopItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var shops = (await conn.QueryAsync("SELECT ID, NAME FROM DCUAHANG WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME")).ToList();

                    var accessibleShopIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var userShops = (await conn.QueryAsync<string>("SELECT DCUAHANGID FROM TNGUOIDUNGTHEOCUAHANG WHERE SUSERID = @UserId AND (STATUS IS NULL OR STATUS <> 0)", new { UserId = userId })).ToList();
                        foreach (var sId in userShops)
                        {
                            if (!string.IsNullOrEmpty(sId)) accessibleShopIds.Add(sId.Trim());
                        }
                    }

                    foreach (object r in shops)
                    {
                        var dict = r as IDictionary<string, object>;
                        string sId = GetValue(dict, "ID")?.ToString() ?? "";
                        string sName = GetValue(dict, "NAME")?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(sId))
                        {
                            bool hasAccess = string.IsNullOrEmpty(userId) ? true : accessibleShopIds.Contains(sId);
                            list.Add(new UserShopItem
                            {
                                ShopId = sId,
                                ShopName = sName,
                                HasAccess = hasAccess
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetUserShopAccessListAsync: " + ex.Message);
            }
            return list;
        }

        public static async Task<bool> SaveUserAsync(UserAccountItem item, IEnumerable<UserShopItem> shopList = null)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Username)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string currentUserId = await GetCurrentUserIdAsync(conn);

                    string groupVal = string.IsNullOrWhiteSpace(item.SgroupuserId) ? null : item.SgroupuserId;
                    string nvVal = string.IsNullOrWhiteSpace(item.DnhanvienId) ? null : item.DnhanvienId;
                    object simageParam = string.IsNullOrWhiteSpace(item.SimageId) ? null : item.SimageId.Trim();
                    string userIdParam = string.IsNullOrWhiteSpace(item.UserId) ? null : item.UserId.Trim();
                    string cardCodeParam = string.IsNullOrWhiteSpace(item.CardCode) ? null : item.CardCode.Trim();

                    if (string.IsNullOrEmpty(item.Id))
                    {
                        var existing = await conn.ExecuteScalarAsync<int>(
                            "SELECT COUNT(*) FROM SUSER WHERE LOWER(USERNAME) = LOWER(@Username)",
                            new { Username = item.Username.Trim() }
                        );
                        if (existing > 0)
                        {
                            throw new Exception("Tên đăng nhập đã tồn tại trong hệ thống.");
                        }

                        string nextId = Guid.NewGuid().ToString();

                        string sqlInsert = @"
                            INSERT INTO SUSER (
                                ID, USERNAME, PASSWORD, NAME, EMAIL, SGROUPUSERID, DNHANVIENID, 
                                NOTE, SIMAGEID, USERID, CARDCODE, STATUS, ISADMIN, USERCREATEDID, TIMECREATED
                            ) VALUES (
                                @Id, @Username, @Password, @Name, @Email, @SgroupuserId, @DnhanvienId, 
                                @Note, @SimageId, @UserId, @CardCode, 30, 0, @UserCreatedId, CURRENT_TIMESTAMP
                            )";

                        if (conn is SqlConnection)
                        {
                            sqlInsert = @"
                                INSERT INTO SUSER (
                                    ID, USERNAME, PASSWORD, NAME, EMAIL, SGROUPUSERID, DNHANVIENID, 
                                    NOTE, SIMAGEID, USERID, CARDCODE, STATUS, ISADMIN, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @Username, @Password, @Name, @Email, @SgroupuserId, @DnhanvienId, 
                                    @Note, @SimageId, @UserId, @CardCode, 30, 0, @UserCreatedId, GETDATE()
                                )";
                        }

                        await conn.ExecuteAsync(sqlInsert, new
                        {
                            Id = nextId,
                            Username = item.Username.Trim(),
                            Password = item.Password ?? "",
                            Name = item.Name?.Trim() ?? item.Username.Trim(),
                            Email = item.Email?.Trim() ?? "",
                            SgroupuserId = groupVal,
                            DnhanvienId = nvVal,
                            Note = item.Note?.Trim() ?? "",
                            SimageId = simageParam,
                            UserId = userIdParam,
                            CardCode = cardCodeParam,
                            UserCreatedId = currentUserId
                        });
                        item.Id = nextId;
                    }
                    else
                    {
                        string sqlUpdate = @"
                            UPDATE SUSER SET
                                NAME = @Name,
                                PASSWORD = @Password,
                                EMAIL = @Email,
                                SGROUPUSERID = @SgroupuserId,
                                DNHANVIENID = @DnhanvienId,
                                NOTE = @Note,
                                SIMAGEID = @SimageId,
                                USERID = @UserId,
                                CARDCODE = @CardCode,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE ID = @Id";

                        if (conn is SqlConnection)
                        {
                            sqlUpdate = @"
                                UPDATE SUSER SET
                                    NAME = @Name,
                                    PASSWORD = @Password,
                                    EMAIL = @Email,
                                    SGROUPUSERID = @SgroupuserId,
                                    DNHANVIENID = @DnhanvienId,
                                    NOTE = @Note,
                                    SIMAGEID = @SimageId,
                                    USERID = @UserId,
                                    CARDCODE = @CardCode,
                                    USERMODIFIEDID = @UserModifiedId,
                                    TIMEMODIFIED = GETDATE()
                                WHERE ID = @Id";
                        }

                        await conn.ExecuteAsync(sqlUpdate, new
                        {
                            Id = item.Id,
                            Password = item.Password ?? "",
                            Name = item.Name?.Trim() ?? item.Username.Trim(),
                            Email = item.Email?.Trim() ?? "",
                            SgroupuserId = groupVal,
                            DnhanvienId = nvVal,
                            Note = item.Note?.Trim() ?? "",
                            SimageId = simageParam,
                            UserId = userIdParam,
                            CardCode = cardCodeParam,
                            UserModifiedId = currentUserId
                        });
                    }

                    // Save shop access
                    if (shopList != null && !string.IsNullOrEmpty(item.Id))
                    {
                        await conn.ExecuteAsync("DELETE FROM TNGUOIDUNGTHEOCUAHANG WHERE SUSERID = @UserId", new { UserId = item.Id });
                        foreach (var shop in shopList.Where(s => s.HasAccess))
                        {
                            string newUsId = Guid.NewGuid().ToString();
                            string sqlUs = "INSERT INTO TNGUOIDUNGTHEOCUAHANG (ID, SUSERID, DCUAHANGID, STATUS, USERCREATEDID, TIMECREATED) VALUES (@Id, @UserId, @ShopId, 30, @UserCreatedId, CURRENT_TIMESTAMP)";
                            if (conn is SqlConnection) sqlUs = "INSERT INTO TNGUOIDUNGTHEOCUAHANG (ID, SUSERID, DCUAHANGID, STATUS, USERCREATEDID, TIMECREATED) VALUES (@Id, @UserId, @ShopId, 30, @UserCreatedId, GETDATE())";
                            await conn.ExecuteAsync(sqlUs, new
                            {
                                Id = newUsId,
                                UserId = item.Id,
                                ShopId = shop.ShopId,
                                UserCreatedId = currentUserId
                            });
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveUserAsync: " + ex.Message);
                throw;
            }
        }

        public static async Task<bool> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    await conn.ExecuteAsync("DELETE FROM SUSER WHERE ID = @Id", new { Id = userId });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DeleteUserAsync: " + ex.Message);
                throw;
            }
        }
        #endregion

        #region Phân quyền sử dụng chức năng (SFUNCTION & SGROUPROLE)
        public static async Task<List<CategoryItem>> GetFunctionCategoriesAsync()
        {
            var list = new List<CategoryItem>();
            list.Add(new CategoryItem { Name = "Tất cả", Icon = "🌐" });

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var groups = (await conn.QueryAsync<string>(
                        "SELECT DISTINCT GROUPNAME FROM SFUNCTION WHERE (STATUS IS NULL OR STATUS <> 0) AND GROUPNAME IS NOT NULL AND GROUPNAME <> '' ORDER BY GROUPNAME"
                    )).ToList();

                    foreach (var g in groups)
                    {
                        if (!string.IsNullOrWhiteSpace(g))
                        {
                            list.Add(new CategoryItem { Name = g.Trim(), Icon = "📁" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetFunctionCategoriesAsync: " + ex.Message);
            }

            if (list.Count == 1)
            {
                var fallbackGroups = new[] { "Bán hàng", "Báo cáo", "Cơ sở dữ liệu", "Công nợ", "Danh mục", "Hệ thống", "Hóa đơn bán hàng", "Kho hàng", "Nhân sự", "Quản trị", "Quỹ" };
                foreach (var g in fallbackGroups) list.Add(new CategoryItem { Name = g, Icon = "📁" });
            }

            return list;
        }

        public static async Task<List<FunctionRoleItem>> GetFunctionRolesAsync(string groupId)
        {
            var list = new List<FunctionRoleItem>();
            var roleLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var dbFuncs = new List<(string Id, string Name, string GroupName, bool HasCrud, int SortOrder)>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Lấy danh sách SFUNCTION từ DB
                    try
                    {
                        var funcRows = (await conn.QueryAsync("SELECT ID, NAME, GROUPNAME, NOTE, SORTORDER FROM SFUNCTION WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY NAME")).ToList();
                        foreach (object r in funcRows)
                        {
                            var dict = r as IDictionary<string, object>;
                            string fId = GetValue(dict, "ID")?.ToString() ?? "";
                            string fName = GetValue(dict, "NAME")?.ToString() ?? "";
                            string gName = GetValue(dict, "GROUPNAME")?.ToString() ?? "";
                            string note = GetValue(dict, "NOTE")?.ToString() ?? "";
                            int sortOrder = int.TryParse(GetValue(dict, "SORTORDER")?.ToString(), out int s) ? s : 0;

                            if (!string.IsNullOrEmpty(fId) && !string.IsNullOrEmpty(fName))
                            {
                                bool hasCrud = string.IsNullOrWhiteSpace(note) || !note.Equals("View", StringComparison.OrdinalIgnoreCase);
                                dbFuncs.Add((fId, fName, gName, hasCrud, sortOrder));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Query SFUNCTION error: " + ex.Message);
                    }

                    // 2. Lấy danh sách SGROUPROLE đã lưu cho nhóm
                    if (!string.IsNullOrEmpty(groupId) && groupId != "0")
                    {
                        try
                        {
                            var roleRows = (await conn.QueryAsync(
                                "SELECT SFUNCTIONID, MODE FROM SGROUPROLE WHERE SGROUPUSERID = @GroupId AND (STATUS IS NULL OR STATUS <> 0)",
                                new { GroupId = groupId }
                            )).ToList();

                            foreach (object r in roleRows)
                            {
                                var dict = r as IDictionary<string, object>;
                                string fId = GetValue(dict, "SFUNCTIONID")?.ToString() ?? "";
                                string mode = GetValue(dict, "MODE")?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(fId))
                                {
                                    roleLookup[fId] = mode;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Query SGROUPROLE error: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Connection error in GetFunctionRolesAsync: " + ex.Message);
            }

            // Nếu DB có bảng SFUNCTION, dùng dữ liệu DB
            if (dbFuncs.Count > 0)
            {
                foreach (var f in dbFuncs.OrderBy(x => x.Name))
                {
                    var item = new FunctionRoleItem
                    {
                        FunctionId = f.Id,
                        FunctionName = f.Name,
                        GroupName = f.GroupName,
                        HasCrud = f.HasCrud
                    };

                    if (roleLookup.TryGetValue(f.Id, out string mode))
                    {
                        item.SetFromModeString(mode);
                    }
                    else
                    {
                        item.SetFromModeString("0");
                    }

                    list.Add(item);
                }
            }
            else
            {
                // Fallback nếu DB trống
                foreach (var mf in MasterFunctions.OrderBy(x => x.Name))
                {
                    var item = new FunctionRoleItem
                    {
                        FunctionId = mf.Id.ToString(),
                        FunctionName = mf.Name,
                        GroupName = mf.GroupName,
                        HasCrud = mf.HasCrud
                    };

                    if (roleLookup.TryGetValue(mf.Id.ToString(), out string mode))
                    {
                        item.SetFromModeString(mode);
                    }
                    else
                    {
                        item.SetFromModeString("0");
                    }

                    list.Add(item);
                }
            }

            return list;
        }

        public static async Task<bool> SaveFunctionRolesAsync(string groupId, IEnumerable<FunctionRoleItem> roles)
        {
            if (string.IsNullOrWhiteSpace(groupId) || groupId == "0" || roles == null) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string currentUserId = await GetCurrentUserIdAsync(conn);

                    using (var trans = conn.BeginTransaction())
                    {
                        await conn.ExecuteAsync("DELETE FROM SGROUPROLE WHERE SGROUPUSERID = @GroupId", new { GroupId = groupId }, trans);

                        foreach (var role in roles)
                        {
                            if (string.IsNullOrEmpty(role.FunctionId)) continue;
                            string mode = role.GetModeString();
                            int intMode = int.TryParse(mode, out int m) ? m : 0;

                            string sqlInsert = @"
                                INSERT INTO SGROUPROLE (
                                    ID, SGROUPUSERID, SFUNCTIONID, MODE, STATUS, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @GroupId, @FunctionId, @Mode, 30, @UserCreatedId, CURRENT_TIMESTAMP
                                )";

                            if (conn is SqlConnection)
                            {
                                sqlInsert = @"
                                    INSERT INTO SGROUPROLE (
                                        ID, SGROUPUSERID, SFUNCTIONID, MODE, STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @Id, @GroupId, @FunctionId, @Mode, 30, @UserCreatedId, GETDATE()
                                    )";
                            }

                            await conn.ExecuteAsync(sqlInsert, new
                            {
                                Id = Guid.NewGuid().ToString(),
                                GroupId = groupId,
                                FunctionId = role.FunctionId,
                                Mode = intMode,
                                UserCreatedId = currentUserId
                            }, trans);
                        }

                        trans.Commit();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveFunctionRolesAsync: " + ex.Message);
                throw;
            }
        }
        #endregion

        #region Phân quyền xem báo cáo (SREPORT & SREPORTROLE)
        public static async Task<ObservableCollection<ReportCategoryNode>> GetReportCategoriesTreeAsync(bool sortByName = false)
        {
            var rootNodes = new ObservableCollection<ReportCategoryNode>();
            rootNodes.Add(new ReportCategoryNode { Id = "0", Name = "Tất cả", Icon = "🌐" });

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string orderBy = sortByName ? "NAME" : "SORTORDER, NAME";
                    var rows = (await conn.QueryAsync(
                        $"SELECT ID, NAME, PARENTID, ITEMTYPE, SORTORDER FROM SREPORT WHERE (STATUS IS NULL OR STATUS <> 0) AND ITEMTYPE = 1 ORDER BY {orderBy}"
                    )).ToList();

                    var nodeLookup = new Dictionary<string, ReportCategoryNode>(StringComparer.OrdinalIgnoreCase);
                    var allFolders = new List<ReportCategoryNode>();

                    foreach (object r in rows)
                    {
                        var dict = r as IDictionary<string, object>;
                        string id = GetValue(dict, "ID")?.ToString() ?? "";
                        string name = GetValue(dict, "NAME")?.ToString() ?? "";
                        string parentId = GetValue(dict, "PARENTID")?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                        {
                            var node = new ReportCategoryNode
                            {
                                Id = id,
                                Name = name,
                                ParentId = parentId,
                                Icon = "📁"
                            };
                            nodeLookup[id] = node;
                            allFolders.Add(node);
                        }
                    }

                    // Build hierarchy
                    foreach (var folder in allFolders)
                    {
                        if (!string.IsNullOrEmpty(folder.ParentId) && nodeLookup.TryGetValue(folder.ParentId, out var parentNode))
                        {
                            parentNode.Children.Add(folder);
                        }
                        else
                        {
                            rootNodes.Add(folder);
                        }
                    }

                    if (sortByName)
                    {
                        SortNodeChildrenByName(rootNodes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetReportCategoriesTreeAsync: " + ex.Message);
            }

            return rootNodes;
        }

        private static void SortNodeChildrenByName(IEnumerable<ReportCategoryNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Children != null && node.Children.Count > 1)
                {
                    var sorted = node.Children.OrderBy(x => x.Name).ToList();
                    node.Children.Clear();
                    foreach (var c in sorted) node.Children.Add(c);
                }
                if (node.Children != null)
                {
                    SortNodeChildrenByName(node.Children);
                }
            }
        }

        public static async Task<List<ReportRoleItem>> GetReportRolesAsync(string groupId)
        {
            var list = new List<ReportRoleItem>();
            var roleLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var dbReports = new List<(string Id, string Name, string ParentId, string GroupName)>();

            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Lấy danh sách SREPORT từ DB và phân giải Category Hierarchy qua PARENTID
                    try
                    {
                        var repRows = (await conn.QueryAsync("SELECT ID, NAME, PARENTID, ITEMTYPE, SORTORDER FROM SREPORT WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, NAME")).ToList();
                        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (object r in repRows)
                        {
                            var dict = r as IDictionary<string, object>;
                            string id = GetValue(dict, "ID")?.ToString() ?? "";
                            string name = GetValue(dict, "NAME")?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(id))
                            {
                                map[id] = name;
                            }
                        }

                        foreach (object r in repRows)
                        {
                            var dict = r as IDictionary<string, object>;
                            string repId = GetValue(dict, "ID")?.ToString() ?? "";
                            string repName = GetValue(dict, "NAME")?.ToString() ?? "";
                            string pId = GetValue(dict, "PARENTID")?.ToString() ?? "";
                            string itemType = GetValue(dict, "ITEMTYPE")?.ToString() ?? "0";

                            // Bỏ qua các mục folder (ITEMTYPE == 1)
                            if (itemType == "1") continue;

                            if (!string.IsNullOrEmpty(repId) && !string.IsNullOrEmpty(repName))
                            {
                                string groupName = "Báo cáo khác";
                                if (!string.IsNullOrEmpty(pId) && map.TryGetValue(pId, out string pName))
                                {
                                    groupName = pName;
                                }
                                dbReports.Add((repId, repName, pId, groupName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Query SREPORT error: " + ex.Message);
                    }

                    // 2. Lấy danh sách SREPORTROLE cho nhóm
                    if (!string.IsNullOrEmpty(groupId) && groupId != "0")
                    {
                        try
                        {
                            var roleRows = (await conn.QueryAsync(
                                "SELECT SREPORTID, MODE FROM SREPORTROLE WHERE SGROUPUSERID = @GroupId AND (STATUS IS NULL OR STATUS <> 0)",
                                new { GroupId = groupId }
                            )).ToList();

                            foreach (object r in roleRows)
                            {
                                var dict = r as IDictionary<string, object>;
                                string repId = GetValue(dict, "SREPORTID")?.ToString() ?? "";
                                string mode = GetValue(dict, "MODE")?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(repId))
                                {
                                    roleLookup[repId] = mode;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Query SREPORTROLE error: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Connection error in GetReportRolesAsync: " + ex.Message);
            }

            // Dùng dữ liệu thật từ DB
            if (dbReports.Count > 0)
            {
                foreach (var mr in dbReports.OrderBy(x => x.Name))
                {
                    var item = new ReportRoleItem
                    {
                        ReportId = mr.Id,
                        ReportName = mr.Name,
                        ParentId = mr.ParentId,
                        GroupName = mr.GroupName
                    };

                    if (roleLookup.TryGetValue(mr.Id, out string mode))
                    {
                        item.SetFromModeString(mode);
                    }
                    else
                    {
                        // Mặc định không được cấp quyền xem nếu không có trong SREPORTROLE
                        item.SetFromModeString("0");
                    }

                    list.Add(item);
                }
            }
            else
            {
                // Fallback nếu DB trống
                foreach (var mr in MasterReports.OrderBy(x => x.Name))
                {
                    var item = new ReportRoleItem
                    {
                        ReportId = mr.Id.ToString(),
                        ReportName = mr.Name,
                        GroupName = mr.GroupName
                    };

                    if (roleLookup.TryGetValue(mr.Id.ToString(), out string mode))
                    {
                        item.SetFromModeString(mode);
                    }
                    else
                    {
                        item.SetFromModeString("0");
                    }

                    list.Add(item);
                }
            }

            return list;
        }

        public static async Task<bool> SaveReportRolesAsync(string groupId, IEnumerable<ReportRoleItem> roles)
        {
            if (string.IsNullOrWhiteSpace(groupId) || groupId == "0" || roles == null) return false;
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string currentUserId = await GetCurrentUserIdAsync(conn);

                    using (var trans = conn.BeginTransaction())
                    {
                        await conn.ExecuteAsync("DELETE FROM SREPORTROLE WHERE SGROUPUSERID = @GroupId", new { GroupId = groupId }, trans);

                        foreach (var role in roles)
                        {
                            if (string.IsNullOrEmpty(role.ReportId)) continue;
                            string mode = role.GetModeString();
                            int intMode = int.TryParse(mode, out int m) ? m : 30;

                            string sqlInsert = @"
                                INSERT INTO SREPORTROLE (
                                    ID, SGROUPUSERID, SREPORTID, MODE, STATUS, USERCREATEDID, TIMECREATED
                                ) VALUES (
                                    @Id, @GroupId, @ReportId, @Mode, 30, @UserCreatedId, CURRENT_TIMESTAMP
                                )";

                            if (conn is SqlConnection)
                            {
                                sqlInsert = @"
                                    INSERT INTO SREPORTROLE (
                                        ID, SGROUPUSERID, SREPORTID, MODE, STATUS, USERCREATEDID, TIMECREATED
                                    ) VALUES (
                                        @Id, @GroupId, @ReportId, @Mode, 30, @UserCreatedId, GETDATE()
                                    )";
                            }

                            await conn.ExecuteAsync(sqlInsert, new
                            {
                                Id = Guid.NewGuid().ToString(),
                                GroupId = groupId,
                                ReportId = role.ReportId,
                                Mode = intMode,
                                UserCreatedId = currentUserId
                            }, trans);
                        }

                        trans.Commit();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SaveReportRolesAsync: " + ex.Message);
                throw;
            }
        }
        #endregion

        public static async Task<List<BaoCaoPhanQuyenItem>> GetBaoCaoPhanQuyenDataAsync(string selectedGroupId, string selectedUserId, bool includeReports)
        {
            var result = new List<BaoCaoPhanQuyenItem>();
            try
            {
                var allGroups = await GetGroupUsersAsync();
                var allUsers = await GetUsersAsync();

                string filterGroupId = selectedGroupId;

                // If user selected a specific user, prioritize that user's group
                if (!string.IsNullOrEmpty(selectedUserId))
                {
                    var userObj = allUsers.FirstOrDefault(u => u.Id == selectedUserId || u.UserId == selectedUserId);
                    if (userObj != null && !string.IsNullOrEmpty(userObj.SgroupuserId))
                    {
                        filterGroupId = userObj.SgroupuserId;
                    }
                }

                var targetGroups = allGroups.Where(g =>
                    string.IsNullOrEmpty(filterGroupId) || g.Id == filterGroupId
                ).ToList();

                foreach (var grp in targetGroups)
                {
                    if (string.IsNullOrEmpty(grp.Id)) continue;

                    // 1. Chức năng
                    var funcs = await GetFunctionRolesAsync(grp.Id);
                    foreach (var f in funcs)
                    {
                        string subG = string.IsNullOrWhiteSpace(f.GroupName) ? "Chức năng khác" : f.GroupName;
                        result.Add(new BaoCaoPhanQuyenItem
                        {
                            GroupUserId = grp.Id,
                            GroupUserName = grp.Name,
                            CategoryType = "CHỨC NĂNG",
                            SubGroup = subG,
                            FunctionName = f.FunctionName,
                            CanView = f.CanView,
                            CanAdd = f.CanAdd,
                            CanEdit = f.CanEdit,
                            CanDelete = f.CanDelete
                        });
                    }

                    // 2. Báo cáo (nếu chọn CheckBox)
                    if (includeReports)
                    {
                        var reports = await GetReportRolesAsync(grp.Id);
                        foreach (var r in reports)
                        {
                            string subG = string.IsNullOrWhiteSpace(r.GroupName) ? "Báo cáo hệ thống" : r.GroupName;
                            result.Add(new BaoCaoPhanQuyenItem
                            {
                                GroupUserId = grp.Id,
                                GroupUserName = grp.Name,
                                CategoryType = "BÁO CÁO",
                                SubGroup = subG,
                                FunctionName = r.ReportName,
                                CanView = r.IsAllowed,
                                CanAdd = false,
                                CanEdit = false,
                                CanDelete = false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBaoCaoPhanQuyenDataAsync: " + ex.Message);
            }
            return result;
        }
    }

    public class BaoCaoPhanQuyenItem
    {
        public string GroupUserId { get; set; } = "";
        public string GroupUserName { get; set; } = "";
        public string CategoryType { get; set; } = ""; // "CHỨC NĂNG" hoặc "BÁO CÁO"
        public string SubGroup { get; set; } = "";     // Ví dụ: "Bán hàng", "Kho hàng", "Báo cáo"
        public string FunctionName { get; set; } = "";
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
