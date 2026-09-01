using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views
{
    public partial class ThemLoaiHinhKhuyenMaiWindow : Window
    {
        private string _id;
        private bool _isNew = true;
        private List<SImageViewModel> _images = new();

        public event Action OnSaved;

        public ThemLoaiHinhKhuyenMaiWindow(string id = null)
        {
            InitializeComponent();
            _id = id;
            _isNew = string.IsNullOrEmpty(id);

            Loaded += ThemLoaiHinhKhuyenMaiWindow_Loaded;
        }

        private async void ThemLoaiHinhKhuyenMaiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var service = new LocalKhachDatHangService();
                _images = await service.GetSImagesAsync();
                CboAnh.ItemsSource = _images;

                if (!_isNew)
                {
                    TxtHeaderTitle.Text = "Chỉnh sửa loại hình khuyến mại";
                    this.Title = "Chỉnh sửa loại hình khuyến mại";
                    await LoadDataAsync();
                }
                else
                {
                    TxtHeaderTitle.Text = "Thêm mới loại hình khuyến mại";
                    this.Title = "Thêm mới loại hình khuyến mại";
                    if (_images.Count > 0) CboAnh.SelectedIndex = 0;
                }

                TxtTen.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải giao diện: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                    string sql = $"SELECT ID, NAME, NOTE, SIMAGEID FROM DLOAIHINHKHUYENMAI WHERE ID = '{_id.Replace("'", "''")}'";
                    var item = (await conn.QueryAsync(sql)).FirstOrDefault();
                    if (item != null)
                    {
                        TxtTen.Text = item.NAME?.ToString() ?? "";
                        TxtGhiChu.Text = item.NOTE?.ToString() ?? "";
                        string simageId = item.SIMAGEID?.ToString();
                        if (!string.IsNullOrEmpty(simageId))
                        {
                            var img = _images.FirstOrDefault(x => x.Id == simageId);
                            if (img != null) CboAnh.SelectedItem = img;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnGhiDuLieu_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtTen.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên loại hình khuyến mại không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTen.Focus();
                return;
            }

            string note = TxtGhiChu.Text.Trim();
            string selectedImgId = (CboAnh.SelectedItem as SImageViewModel)?.Id;
            string userId = SessionContext.CurrentUser?.Id ?? "4f1466a0-0756-4ba9-afa8-053b96ca7569";

            try
            {
                using (var conn = DbConnectionManager.GetConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                    if (_isNew)
                    {
                        string newId = Guid.NewGuid().ToString();
                        string sql = $@"
                            INSERT INTO DLOAIHINHKHUYENMAI (
                                ID, NAME, NOTE, STATUS, USERCREATEDID, USERMODIFIEDID, 
                                TIMECREATED, TIMEMODIFIED, ITEMTYPE, PARENTDIR, SORTORDER, SIMAGEID
                            ) VALUES (
                                '{newId}', '{name.Replace("'", "''")}', '{note.Replace("'", "''")}', 30, 
                                '{userId}', '{userId}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 
                                '0', '0', 'ZZZZ', {(string.IsNullOrEmpty(selectedImgId) ? "NULL" : $"'{selectedImgId}'")}
                            )";
                        await conn.ExecuteAsync(sql);
                    }
                    else
                    {
                        string sql = $@"
                            UPDATE DLOAIHINHKHUYENMAI SET
                                NAME = '{name.Replace("'", "''")}',
                                NOTE = '{note.Replace("'", "''")}',
                                USERMODIFIEDID = '{userId}',
                                TIMEMODIFIED = CURRENT_TIMESTAMP,
                                SIMAGEID = {(string.IsNullOrEmpty(selectedImgId) ? "NULL" : $"'{selectedImgId}'")}
                            WHERE ID = '{_id.Replace("'", "''")}'";
                        await conn.ExecuteAsync(sql);
                    }
                }

                OnSaved?.Invoke();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm loại hình: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
