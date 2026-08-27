using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyBar.Client.Views
{
    public partial class InLuoiWindow : Window
    {
        public class ColumnInfo
        {
            public string Header { get; set; }
            public bool IsChecked { get; set; }
        }

        public InLuoiWindow(DataGrid grid)
        {
            InitializeComponent();
            LoadColumns(grid);
        }

        private void LoadColumns(DataGrid grid)
        {
            if (grid == null) return;
            LstCotHienThi.Tag = grid.ItemsSource;

            var columns = new List<ColumnInfo>();
            foreach (var col in grid.Columns)
            {
                if (col.Header != null && col.Header.ToString() != "STT")
                {
                    columns.Add(new ColumnInfo { Header = col.Header.ToString(), IsChecked = true });
                }
            }
            LstCotHienThi.ItemsSource = columns;
        }

        private void BtnHienThi_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                var data = LstCotHienThi.Tag as System.Collections.IEnumerable;
                var columns = LstCotHienThi.ItemsSource as List<ColumnInfo>;
                
                if (data == null)
                {
                    MessageBox.Show("Không có dữ liệu để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (columns == null || !columns.Any(c => c.IsChecked))
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một cột để hiển thị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string title = TxtTieuDe.Text;
                string note = TxtGhiChu.Text;
                string template = (LstMauIn.SelectedItem as ListBoxItem)?.Content?.ToString() ?? "Mẫu A4 nằm ngang";
                bool inSTT = ChkInSTT.IsChecked == true;

                var previewWin = new PrintPreviewWindow(data.Cast<object>(), columns, title, note, template, inSTT);
                previewWin.ShowDialog();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo bản in: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
