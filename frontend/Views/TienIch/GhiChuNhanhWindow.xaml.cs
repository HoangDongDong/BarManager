using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace QuanLyBar.Client.Views.TienIch
{
    public partial class GhiChuNhanhWindow : Window
    {
        private int _currentPage = 1;
        private int _totalPages = 1;
        private bool _isLoading = false;

        public GhiChuNhanhWindow()
        {
            InitializeComponent();
            _ = InitNotesFromDbAsync();
        }

        private async Task InitNotesFromDbAsync()
        {
            _isLoading = true;
            try
            {
                string pagesStr = await Services.LocalCauHinhService.GetConfigValueAsync("GHICHU_TOTAL_PAGES", "1");
                if (!int.TryParse(pagesStr, out _totalPages) || _totalPages < 1)
                {
                    _totalPages = 1;
                }

                UpdatePageDropdown();
                await LoadCurrentPageFromDbAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitNotesFromDbAsync error: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void UpdatePageDropdown()
        {
            CboPages.SelectionChanged -= CboPages_SelectionChanged;
            CboPages.Items.Clear();

            for (int i = 1; i <= _totalPages; i++)
            {
                CboPages.Items.Add($"Trang {i}");
            }

            if (_currentPage > _totalPages) _currentPage = _totalPages;
            if (_currentPage < 1) _currentPage = 1;

            CboPages.SelectedIndex = _currentPage - 1;
            TxtPageInfo.Text = $"{_currentPage}/{_totalPages}";
            CboPages.SelectionChanged += CboPages_SelectionChanged;
        }

        private async Task LoadCurrentPageFromDbAsync()
        {
            _isLoading = true;
            try
            {
                string key = $"GHICHU_PAGE_{_currentPage}";
                string rtfData = await Services.LocalCauHinhService.GetConfigValueAsync(key, "");

                FlowDoc.Blocks.Clear();

                if (!string.IsNullOrWhiteSpace(rtfData))
                {
                    try
                    {
                        var range = new TextRange(FlowDoc.ContentStart, FlowDoc.ContentEnd);
                        using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rtfData)))
                        {
                            range.Load(ms, DataFormats.Rtf);
                        }
                    }
                    catch
                    {
                        // Fallback plain text if not RTF
                        FlowDoc.Blocks.Add(new Paragraph(new Run(rtfData)));
                    }
                }
                else
                {
                    FlowDoc.Blocks.Add(new Paragraph(new Run("")));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadCurrentPageFromDbAsync error: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task SaveCurrentPageToDbAsync()
        {
            if (_isLoading) return;
            try
            {
                string rtfData = "";
                var range = new TextRange(FlowDoc.ContentStart, FlowDoc.ContentEnd);
                using (var ms = new MemoryStream())
                {
                    range.Save(ms, DataFormats.Rtf);
                    rtfData = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                }

                string key = $"GHICHU_PAGE_{_currentPage}";
                await Services.LocalCauHinhService.SaveSingleConfigAsync(key, rtfData);
                await Services.LocalCauHinhService.SaveSingleConfigAsync("GHICHU_TOTAL_PAGES", _totalPages.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveCurrentPageToDbAsync error: " + ex.Message);
            }
        }

        private async void RtbContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoading) return;
            await SaveCurrentPageToDbAsync();
        }

        private async void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                await SaveCurrentPageToDbAsync();
                _currentPage--;
                UpdatePageDropdown();
                await LoadCurrentPageFromDbAsync();
            }
        }

        private async void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                await SaveCurrentPageToDbAsync();
                _currentPage++;
                UpdatePageDropdown();
                await LoadCurrentPageFromDbAsync();
            }
        }

        private async void BtnAddPage_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentPageToDbAsync();
            _totalPages++;
            _currentPage = _totalPages;
            UpdatePageDropdown();
            await LoadCurrentPageFromDbAsync();
        }

        private async void BtnDeletePage_Click(object sender, RoutedEventArgs e)
        {
            if (_totalPages <= 1)
            {
                FlowDoc.Blocks.Clear();
                FlowDoc.Blocks.Add(new Paragraph(new Run("")));
                await SaveCurrentPageToDbAsync();
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa Trang {_currentPage} không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                await Services.LocalCauHinhService.SaveSingleConfigAsync($"GHICHU_PAGE_{_currentPage}", "");
                _totalPages--;
                if (_currentPage > _totalPages) _currentPage = _totalPages;
                UpdatePageDropdown();
                await LoadCurrentPageFromDbAsync();
                await Services.LocalCauHinhService.SaveSingleConfigAsync("GHICHU_TOTAL_PAGES", _totalPages.ToString());
            }
        }

        private async void CboPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            int selected = CboPages.SelectedIndex + 1;
            if (selected >= 1 && selected <= _totalPages && selected != _currentPage)
            {
                await SaveCurrentPageToDbAsync();
                _currentPage = selected;
                TxtPageInfo.Text = $"{_currentPage}/{_totalPages}";
                await LoadCurrentPageFromDbAsync();
            }
        }

        private void BtnInsertTimestamp_Click(object sender, RoutedEventArgs e)
        {
            string timestamp = $" [{DateTime.Now:HH:mm:ss dd/MM/yyyy}] ";
            RtbContent.CaretPosition.InsertTextInRun(timestamp);
        }

        private void BtnInsertLine_Click(object sender, RoutedEventArgs e)
        {
            Paragraph p = new Paragraph(new Run("--------------------------------------------------------------------------------"));
            p.Foreground = System.Windows.Media.Brushes.Gray;
            FlowDoc.Blocks.Add(p);
        }

        private void BtnStrikethrough_Click(object sender, RoutedEventArgs e)
        {
            var selection = RtbContent.Selection;
            if (!selection.IsEmpty)
            {
                var currentDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
                if (currentDecorations != null && currentDecorations.Contains(TextDecorations.Strikethrough[0]))
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
                else
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                }
            }
        }
    }
}
