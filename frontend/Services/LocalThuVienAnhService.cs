using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapper;

namespace QuanLyBar.Client.Services
{
    public class ThuVienAnhItemViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Khác";
        public string Note { get; set; } = "";
        public int SortOrder { get; set; } = 0;
        public byte[] ImageBytes { get; set; }
        public ImageSource ImageSource { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class ThuVienAnhGroupViewModel : INotifyPropertyChanged
    {
        public string GroupName { get; set; } = "";
        public bool HasGroupHeader => !string.IsNullOrEmpty(GroupName) && GroupName != "Biểu tượng" && GroupName != "Chung" && GroupName != "General";
        public ObservableCollection<ThuVienAnhItemViewModel> Items { get; set; } = new ObservableCollection<ThuVienAnhItemViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public static class LocalThuVienAnhService
    {
        private static IDbConnection GetConnection() => DbConnectionManager.GetConnection();

        public static BitmapImage BytesToBitmapImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            try
            {
                using var ms = new MemoryStream(bytes);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ImageSourceToBytes(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch
            {
                return null;
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

        private static async Task<int> GetNextSImageIdAsync(IDbConnection conn)
        {
            try
            {
                var rows = (await conn.QueryAsync("SELECT ID FROM SIMAGE")).Cast<IDictionary<string, object>>().ToList();
                int maxId = 0;
                foreach (var dict in rows)
                {
                    object valObj = GetValue(dict, "ID");
                    if (valObj != null && !Convert.IsDBNull(valObj))
                    {
                        if (int.TryParse(valObj.ToString(), out int val) && val > maxId)
                            maxId = val;
                    }
                }
                return maxId + 1;
            }
            catch
            {
                return 1;
            }
        }

        private static readonly List<string> KnownCategories = new List<string>
        {
            "Biểu tượng",
            "Nguồn thông tin",
            "Ngành hàng",
            "Thành viên",
            "Cờ",
            "Giới tính",
            "Người",
            "Sao",
            "Số",
            "Trạng thái",
            "Tập hợp",
            "Đối tượng",
            "Ưu tiên",
            "Khác"
        };

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalizedString)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace("đ", "d").Replace("Đ", "D");
        }

        public static string DetermineCategory(string note, string name, string catVal)
        {
            string[] candidates = new[] { catVal, note, name };

            foreach (var raw in candidates)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string clean = raw.Trim();
                string cleanNoDia = RemoveDiacritics(clean).ToLowerInvariant();

                // 1. Direct match with known categories (with and without diacritics)
                foreach (var cat in KnownCategories)
                {
                    if (string.Equals(clean, cat, StringComparison.OrdinalIgnoreCase))
                        return cat;

                    string catNoDia = RemoveDiacritics(cat).ToLowerInvariant();
                    if (cleanNoDia == catNoDia)
                        return cat;
                }

                // 2. Keyword check
                if (cleanNoDia.Contains("nguon thong tin") || cleanNoDia.Contains("thong tin") || cleanNoDia.Contains("nguon"))
                    return "Nguồn thông tin";
                if (cleanNoDia.Contains("nganh hang") || cleanNoDia.Contains("hang hoa") || cleanNoDia.Contains("mat hang"))
                    return "Ngành hàng";
                if (cleanNoDia.Contains("thanh vien") || cleanNoDia.Contains("member") || cleanNoDia.Contains("vip") || cleanNoDia.Contains("cup") || cleanNoDia.Contains("medal") || cleanNoDia.Contains("danh hieu"))
                    return "Thành viên";
                if (cleanNoDia.Contains("gioi tinh") || cleanNoDia.Contains("gender") || cleanNoDia == "nam" || cleanNoDia == "nu" || cleanNoDia.Contains("gioitinh"))
                    return "Giới tính";
                if (cleanNoDia.Contains("nguoi") || cleanNoDia.Contains("nhan vien") || cleanNoDia.Contains("user") || cleanNoDia.Contains("person") || cleanNoDia.Contains("khach hang") || cleanNoDia.Contains("khach") || cleanNoDia.Contains("nhanvien"))
                    return "Người";
                if (cleanNoDia.Contains("sao") || cleanNoDia.Contains("star"))
                    return "Sao";
                if (cleanNoDia.Contains("co") || cleanNoDia.Contains("flag") || cleanNoDia.Contains("quoc gia"))
                    return "Cờ";
                if (cleanNoDia.Contains("so") || cleanNoDia.Contains("number") || cleanNoDia.Contains("num") || int.TryParse(clean, out _))
                    return "Số";
                if (cleanNoDia.Contains("trang thai") || cleanNoDia.Contains("status"))
                    return "Trạng thái";
                if (cleanNoDia.Contains("tap hop"))
                    return "Tập hợp";
                if (cleanNoDia.Contains("doi tuong") || cleanNoDia.Contains("object"))
                    return "Đối tượng";
                if (cleanNoDia.Contains("uu tien") || cleanNoDia.Contains("priority"))
                    return "Ưu tiên";
                if (cleanNoDia.Contains("bieu tuong") || cleanNoDia.Contains("icon") || cleanNoDia.Contains("symbol"))
                    return "Biểu tượng";
            }

            return "Biểu tượng";
        }

        public static async Task<List<ThuVienAnhGroupViewModel>> GetGroupedImagesAsync()
        {
            var groups = new List<ThuVienAnhGroupViewModel>();
            var allItems = new List<ThuVienAnhItemViewModel>();

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = "SELECT * FROM SIMAGE WHERE (STATUS IS NULL OR STATUS <> 0) ORDER BY SORTORDER, ID";
                var rows = (await conn.QueryAsync(sql)).Cast<IDictionary<string, object>>().ToList();

                foreach (var r in rows)
                {
                    string id = GetValue(r, "ID")?.ToString() ?? "";
                    string name = GetValue(r, "NAME")?.ToString() ?? "";
                    string note = GetValue(r, "NOTE")?.ToString() ?? "";
                    string catVal = (GetValue(r, "CATEGORY") 
                                 ?? GetValue(r, "NHOM") 
                                 ?? GetValue(r, "GROUPNAME") 
                                 ?? GetValue(r, "NHOMANH"))?.ToString() ?? "";

                    byte[] bytes = GetValue(r, "IMAGE") as byte[];
                    int sortOrder = 0;
                    object sortObj = GetValue(r, "SORTORDER");
                    if (sortObj != null && int.TryParse(sortObj.ToString(), out int sVal)) sortOrder = sVal;

                    string category = DetermineCategory(note, name, catVal);

                    var item = new ThuVienAnhItemViewModel
                    {
                        Id = id,
                        Name = name,
                        Category = category,
                        Note = note,
                        SortOrder = sortOrder,
                        ImageBytes = bytes,
                        ImageSource = BytesToBitmapImage(bytes)
                    };

                    allItems.Add(item);
                }

                // If database has items, group them
                if (allItems.Count > 0)
                {
                    // Order groups matching categories dropdown
                    var groupOrder = new List<string>
                    {
                        "Biểu tượng", "Nguồn thông tin", "Ngành hàng", "Thành viên",
                        "Cờ", "Giới tính", "Người", "Sao", "Số", "Trạng thái", "Tập hợp", "Đối tượng", "Ưu tiên", "Khác"
                    };
                    var distinctCategories = allItems.Select(x => x.Category).Distinct().ToList();

                    var orderedCats = distinctCategories.OrderBy(c =>
                    {
                        int idx = groupOrder.IndexOf(c);
                        return idx >= 0 ? idx : 99;
                    }).ToList();

                    foreach (var cat in orderedCats)
                    {
                        var grp = new ThuVienAnhGroupViewModel
                        {
                            GroupName = cat,
                            Items = new ObservableCollection<ThuVienAnhItemViewModel>(allItems.Where(x => x.Category == cat))
                        };
                        groups.Add(grp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetGroupedImagesAsync: " + ex.Message);
            }

            return groups;
        }

        private static async Task<object> GetCurrentUserIdAsync(IDbConnection conn)
        {
            if (SessionContext.CurrentUser != null && !string.IsNullOrEmpty(SessionContext.CurrentUser.Id))
            {
                if (int.TryParse(SessionContext.CurrentUser.Id, out int intId)) return intId;
                return SessionContext.CurrentUser.Id;
            }

            try
            {
                var userId = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER WHERE STATUS IS NULL OR STATUS <> 0");
                if (userId != null) return userId;
            }
            catch { }

            try
            {
                var userId = await conn.ExecuteScalarAsync<object>("SELECT FIRST 1 ID FROM SUSER");
                if (userId != null) return userId;
            }
            catch { }

            return 1;
        }

        public static async Task<(bool ok, string error, string newId)> AddImageAsync(string name, string category, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return (false, "Dữ liệu hình ảnh không hợp lệ", null);

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                int nextId = await GetNextSImageIdAsync(conn);
                string newIdStr = nextId.ToString();
                object userId = await GetCurrentUserIdAsync(conn);

                string catName = !string.IsNullOrWhiteSpace(category) ? category.Trim() : "Biểu tượng";
                string imgName = !string.IsNullOrWhiteSpace(name) ? name.Trim() : catName;

                try
                {
                    string sql = @"
                        INSERT INTO SIMAGE (ID, NAME, NOTE, IMAGE, STATUS, SORTORDER, USERCREATEDID, USERMODIFIEDID, TIMECREATED, TIMEMODIFIED)
                        VALUES (@Id, @Name, @Note, @Image, 1, @SortOrder, @UserId, @UserId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

                    await conn.ExecuteAsync(sql, new
                    {
                        Id = nextId,
                        Name = imgName,
                        Note = catName,
                        Image = bytes,
                        SortOrder = nextId,
                        UserId = userId
                    });
                }
                catch
                {
                    try
                    {
                        string sql2 = @"
                            INSERT INTO SIMAGE (ID, NAME, NOTE, IMAGE, STATUS, SORTORDER, USERCREATEDID, TIMECREATED)
                            VALUES (@Id, @Name, @Note, @Image, 1, @SortOrder, @UserId, CURRENT_TIMESTAMP)";

                        await conn.ExecuteAsync(sql2, new
                        {
                            Id = nextId,
                            Name = imgName,
                            Note = catName,
                            Image = bytes,
                            SortOrder = nextId,
                            UserId = userId
                        });
                    }
                    catch
                    {
                        string sql3 = @"
                            INSERT INTO SIMAGE (ID, NAME, NOTE, IMAGE, STATUS, USERCREATEDID)
                            VALUES (@Id, @Name, @Note, @Image, 1, @UserId)";

                        await conn.ExecuteAsync(sql3, new
                        {
                            Id = nextId,
                            Name = imgName,
                            Note = catName,
                            Image = bytes,
                            UserId = userId
                        });
                    }
                }

                return (true, null, newIdStr);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<(bool ok, string error)> UpdateImageAsync(string id, byte[] bytes)
        {
            if (string.IsNullOrEmpty(id) || bytes == null || bytes.Length == 0) return (false, "Dữ liệu không hợp lệ");

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                object userId = await GetCurrentUserIdAsync(conn);

                string sql = @"
                    UPDATE SIMAGE SET 
                        IMAGE = @Image,
                        USERMODIFIEDID = @UserId,
                        TIMEMODIFIED = CURRENT_TIMESTAMP
                    WHERE CAST(ID AS VARCHAR(50)) = @IdStr";

                try
                {
                    await conn.ExecuteAsync(sql, new { IdStr = id, Image = bytes, UserId = userId });
                }
                catch
                {
                    string sqlMin = "UPDATE SIMAGE SET IMAGE = @Image WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                    await conn.ExecuteAsync(sqlMin, new { IdStr = id, Image = bytes });
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool ok, string error)> DeleteImageAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return (false, "Chưa chọn hình ảnh");

            try
            {
                using var conn = GetConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                string sql = "DELETE FROM SIMAGE WHERE CAST(ID AS VARCHAR(50)) = @IdStr";
                await conn.ExecuteAsync(sql, new { IdStr = id });

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
