using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.DanhMucNhaCungCap
{
    public partial class ThemNhomNhaCungCapWindow : Window
    {
        public event Action OnSaved;
        private string _id;
        private string _parentId;
        private bool _isEdit => !string.IsNullOrEmpty(_id);

        public ThemNhomNhaCungCapWindow(string id = null, string name = null, string parentId = null)
        {
            InitializeComponent();
            _id = id;
            _parentId = parentId;
            if (_isEdit)
            {
                Title = "SỬA NHÓM NHÀ CUNG CẤP";
                TxtTenNhom.Text = name ?? "";
            }
            else
            {
                Title = "THÊM NHÓM NHÀ CUNG CẤP";
            }
            Loaded += (s, e) => TxtTenNhom.Focus();
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtTenNhom.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenNhom.Focus();
                return;
            }

            string note = TxtGhiChu.Text.Trim();

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

                    string userId = await LocalNhaCungCapService.GetCurrentUserIdAsync(conn);

                    if (!_isEdit)
                    {
                        string newId = Guid.NewGuid().ToString();
                        string sql = @"
                            INSERT INTO DNHOMNHACUNGCAP (
                                ID, NAME, NOTE, STATUS, PARENTID, USERCREATEDID, TIMECREATED, SORTORDER
                            ) VALUES (
                                @Id, @Name, @Note, 30, @ParentId, @UserCreatedId, CURRENT_TIMESTAMP, 'ZZZZ'
                            )";
                        await conn.ExecuteAsync(sql, new
                        {
                            Id = newId,
                            Name = name,
                            Note = note,
                            ParentId = string.IsNullOrEmpty(_parentId) ? null : _parentId,
                            UserCreatedId = userId
                        });
                    }
                    else
                    {
                        string sql = @"
                            UPDATE DNHOMNHACUNGCAP SET
                                NAME = @Name,
                                NOTE = @Note,
                                USERMODIFIEDID = @UserModifiedId,
                                TIMEMODIFIED = CURRENT_TIMESTAMP
                            WHERE CAST(ID AS VARCHAR(50)) = @Id";
                        await conn.ExecuteAsync(sql, new
                        {
                            Id = _id,
                            Name = name,
                            Note = note,
                            UserModifiedId = userId
                        });
                    }

                    OnSaved?.Invoke();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu nhóm:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}
