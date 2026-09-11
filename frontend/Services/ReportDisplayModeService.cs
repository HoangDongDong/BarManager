using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace QuanLyBar.Client.Services
{
    /// <summary>
    /// Adds the three compact report display modes used by the legacy report viewer.
    /// The service works with the common A4PageBorder/ReportPaper layouts so every
    /// report gets the same behaviour without duplicating click handlers.
    /// </summary>
    public static class ReportDisplayModeService
    {
        private enum DisplayMode
        {
            FullReport,
            WithoutCompany,
            TablesOnly
        }

        private sealed class ReportViewState
        {
            public FrameworkElement Page { get; init; } = null!;
            public FrameworkElement? CompanySection { get; init; }
            public FrameworkElement? TitleSection { get; init; }
            public Visibility CompanyVisibility { get; init; }
            public Visibility TitleVisibility { get; init; }
            public double Width { get; init; }
            public double MinHeight { get; init; }
            public HorizontalAlignment HorizontalAlignment { get; init; }
            public Thickness Margin { get; init; }
            public Thickness? Padding { get; init; }
            public List<Button> ModeButtons { get; } = new List<Button>();
        }

        private static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(ReportDisplayModeService),
            new PropertyMetadata(false));

        private static readonly DependencyProperty HasLocalizedRawGridProperty = DependencyProperty.RegisterAttached(
            "HasLocalizedRawGrid", typeof(bool), typeof(ReportDisplayModeService), new PropertyMetadata(false));

        private static readonly DependencyProperty OriginalGridColumnWidthsProperty = DependencyProperty.RegisterAttached(
            "OriginalGridColumnWidths", typeof(List<GridLength>), typeof(ReportDisplayModeService), new PropertyMetadata(null));

        private static readonly DependencyProperty HasAutoFitTablesProperty = DependencyProperty.RegisterAttached(
            "HasAutoFitTables", typeof(bool), typeof(ReportDisplayModeService), new PropertyMetadata(false));

        public static void Attach(FrameworkElement report)
        {
            if ((bool)report.GetValue(IsAttachedProperty))
                return;

            report.Loaded += (_, _) => TryAttach(report);
            if (report.IsLoaded)
                TryAttach(report);
        }

        private static void TryAttach(FrameworkElement report)
        {
            AttachRawDataGridHeaders(report);
            HideMisplacedSignatureBlocks(report);

            if ((bool)report.GetValue(IsAttachedProperty))
                return;

            var page = FindNamedElement(report, "A4PageBorder", "ReportPaper", "PaperContainer");
            var toolbarAnchor = FindNamedElement(report, "BtnExcel", "BtnXuatExcel", "BtnExport", "BtnIn", "BtnPrint") as Button;
            if (page == null || toolbarAnchor?.Parent is not Panel toolbar)
                return;

            var contentRoot = page is Border border && border.Child is FrameworkElement child
                ? child
                : page;

            var companyMarker = FindNamedElement(report, "GridCompanyHeader", "TxtCompanyName", "TxtTenCuaHang");
            var titleMarker = FindNamedElement(report, "GridReportTitleArea", "TxtReportTitle");
            var companySection = FindSection(contentRoot, companyMarker);
            var titleSection = FindSection(contentRoot, titleMarker);

            // A report must expose at least one of these sections. This keeps the
            // buttons away from operational screens that merely happen to print.
            if (companySection == null && titleSection == null)
                return;

            var state = new ReportViewState
            {
                Page = page,
                CompanySection = companySection,
                TitleSection = titleSection,
                CompanyVisibility = companySection?.Visibility ?? Visibility.Visible,
                TitleVisibility = titleSection?.Visibility ?? Visibility.Visible,
                Width = page.Width,
                MinHeight = page.MinHeight,
                HorizontalAlignment = page.HorizontalAlignment,
                Margin = page.Margin,
                Padding = page is Border pageBorder ? pageBorder.Padding : null
            };

            var separator = new Separator
            {
                Width = 1,
                Height = 18,
                Margin = new Thickness(6, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(176, 196, 222))
            };
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            buttonPanel.Children.Add(CreateModeButton(state, "▥", "Đầy đủ thông tin quán", DisplayMode.FullReport));
            buttonPanel.Children.Add(CreateModeButton(state, "▤", "Không hiển thị thông tin quán", DisplayMode.WithoutCompany));
            buttonPanel.Children.Add(CreateModeButton(state, "▦", "Chỉ hiển thị các bảng", DisplayMode.TablesOnly));

            var anchorIndex = toolbar.Children.IndexOf(toolbarAnchor);
            toolbar.Children.Insert(anchorIndex + 1, separator);
            toolbar.Children.Insert(anchorIndex + 2, buttonPanel);
            report.SetValue(IsAttachedProperty, true);
            AttachTableAutoFit(page);
            HideMisplacedSignatureBlocks(page);
            ApplyMode(state, DisplayMode.FullReport);
        }

        private static void AttachRawDataGridHeaders(DependencyObject root)
        {
            if (root is DataGrid grid && IsRawDataGrid(grid) && !(bool)grid.GetValue(HasLocalizedRawGridProperty))
            {
                LocalizeExistingColumns(grid);
                grid.AutoGeneratingColumn += (_, e) =>
                {
                    string label = ReportTextLocalizationService.GetColumnLabel(e.PropertyName);
                    if (!string.IsNullOrWhiteSpace(label)) e.Column.Header = label;
                };
                grid.SetValue(HasLocalizedRawGridProperty, true);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                AttachRawDataGridHeaders(VisualTreeHelper.GetChild(root, i));
        }

        private static void LocalizeExistingColumns(DataGrid grid)
        {
            foreach (DataGridColumn column in grid.Columns)
            {
                string fieldName = column.SortMemberPath;
                if (string.IsNullOrWhiteSpace(fieldName) &&
                    column is DataGridBoundColumn boundColumn &&
                    boundColumn.Binding is Binding binding)
                {
                    fieldName = binding.Path?.Path ?? "";
                }

                if (string.IsNullOrWhiteSpace(fieldName))
                    fieldName = column.Header?.ToString() ?? "";

                string label = ReportTextLocalizationService.GetColumnLabel(fieldName);
                if (!string.IsNullOrWhiteSpace(label))
                    column.Header = label;
            }
        }

        private static bool IsRawDataGrid(DataGrid grid)
        {
            string name = grid.Name ?? "";
            return name.Contains("DuLieuTho", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("RawData", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("InlineDataGrid", StringComparison.OrdinalIgnoreCase);
        }

        private static Button CreateModeButton(ReportViewState state, string icon, string tooltip, DisplayMode mode)
        {
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = icon,
                    FontFamily = new FontFamily("Segoe UI Symbol"),
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 29,
                Height = 24,
                Padding = new Thickness(1),
                Margin = new Thickness(0, 0, 2, 0),
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromRgb(164, 184, 204)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = tooltip,
                Tag = mode
            };
            button.Click += (_, _) => ApplyMode(state, mode);
            state.ModeButtons.Add(button);
            return button;
        }

        private static void ApplyMode(ReportViewState state, DisplayMode mode)
        {
            if (state.CompanySection != null)
                state.CompanySection.Visibility = mode == DisplayMode.FullReport
                    ? state.CompanyVisibility
                    : Visibility.Collapsed;

            if (state.TitleSection != null)
                state.TitleSection.Visibility = mode == DisplayMode.TablesOnly
                    ? Visibility.Collapsed
                    : state.TitleVisibility;

            if (mode == DisplayMode.TablesOnly)
            {
                state.Page.Width = double.NaN;
                state.Page.MinHeight = 0;
                state.Page.HorizontalAlignment = HorizontalAlignment.Stretch;
                state.Page.Margin = new Thickness(4);
                if (state.Page is Border tablePage)
                    tablePage.Padding = new Thickness(6);
            }
            else
            {
                state.Page.Width = state.Width;
                state.Page.MinHeight = state.MinHeight;
                state.Page.HorizontalAlignment = state.HorizontalAlignment;
                state.Page.Margin = state.Margin;
                if (state.Page is Border reportPage && state.Padding.HasValue)
                    reportPage.Padding = state.Padding.Value;
            }

            foreach (var button in state.ModeButtons)
            {
                bool selected = Equals(button.Tag, mode);
                button.Background = selected
                    ? new SolidColorBrush(Color.FromRgb(255, 244, 194))
                    : Brushes.Transparent;
                button.BorderBrush = selected
                    ? new SolidColorBrush(Color.FromRgb(224, 168, 0))
                    : new SolidColorBrush(Color.FromRgb(164, 184, 204));
                button.FontWeight = selected ? FontWeights.Bold : FontWeights.Normal;
            }

            FitReportTablesToPage(state.Page);
            state.Page.Dispatcher.BeginInvoke(new Action(() => FitReportTablesToPage(state.Page)));
        }

        private static void AttachTableAutoFit(FrameworkElement page)
        {
            if ((bool)page.GetValue(HasAutoFitTablesProperty))
                return;

            page.LayoutUpdated += (_, _) => FitReportTablesToPage(page);
            page.SetValue(HasAutoFitTablesProperty, true);
        }

        private static void FitReportTablesToPage(FrameworkElement page)
        {
            HideMisplacedSignatureBlocks(page);

            double availableWidth = GetPageContentWidth(page);
            if (availableWidth <= 0)
                return;

            FitGridsToWidth(page, availableWidth);
        }

        private static double GetPageContentWidth(FrameworkElement page)
        {
            double width = page.ActualWidth;
            if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width))
                width = page.Width;
            if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width))
                return 0;

            if (page is Border border)
            {
                width -= border.Padding.Left + border.Padding.Right;
            }

            return Math.Max(0, width);
        }

        private static void FitGridsToWidth(DependencyObject root, double availableWidth)
        {
            if (root is FrameworkElement element)
            {
                availableWidth -= element.Margin.Left + element.Margin.Right;
            }

            if (root is Grid grid && ShouldFitReportGrid(grid))
            {
                FitGridColumns(grid, availableWidth);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                FitGridsToWidth(VisualTreeHelper.GetChild(root, i), availableWidth);
        }

        private static bool ShouldFitReportGrid(Grid grid)
        {
            if (grid.ColumnDefinitions.Count < 4 || grid.RowDefinitions.Count < 2)
                return false;

            if (IsLayoutContainerGrid(grid))
                return false;

            return grid.Children.OfType<Border>().Count() >= Math.Min(4, grid.ColumnDefinitions.Count);
        }

        private static bool IsLayoutContainerGrid(Grid grid)
        {
            string name = grid.Name ?? "";
            return name.Contains("Title", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("Company", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("Signature", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("Filter", StringComparison.OrdinalIgnoreCase);
        }

        private static void FitGridColumns(Grid grid, double availableWidth)
        {
            if (availableWidth <= 0)
                return;

            var originalWidths = grid.GetValue(OriginalGridColumnWidthsProperty) as List<GridLength>;
            if (originalWidths == null || originalWidths.Count != grid.ColumnDefinitions.Count)
            {
                originalWidths = grid.ColumnDefinitions.Select(c => c.Width).ToList();
                grid.SetValue(OriginalGridColumnWidthsProperty, originalWidths);
            }

            double absoluteTotal = originalWidths
                .Where(w => w.GridUnitType == GridUnitType.Pixel)
                .Sum(w => w.Value);
            bool hasStarColumns = originalWidths.Any(w => w.GridUnitType == GridUnitType.Star);
            if (absoluteTotal <= 0)
                return;

            double scale = !hasStarColumns && absoluteTotal > availableWidth
                ? availableWidth / absoluteTotal
                : 1.0;

            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                var original = originalWidths[i];
                grid.ColumnDefinitions[i].Width = original.GridUnitType == GridUnitType.Pixel
                    ? new GridLength(Math.Max(24, Math.Floor(original.Value * scale)))
                    : original;
            }

            if (!hasStarColumns)
            {
                grid.Width = Math.Min(absoluteTotal, availableWidth);
                grid.MaxWidth = availableWidth;
                grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }

        private static void HideMisplacedSignatureBlocks(FrameworkElement root)
        {
            var contentRoot = root is Border border && border.Child is FrameworkElement child
                ? child
                : root;

            if (contentRoot is not Grid grid)
                return;

            int titleRow = FindTitleRow(grid);
            if (titleRow < 0)
                titleRow = 1;

            foreach (UIElement childElement in grid.Children)
            {
                if (childElement is not FrameworkElement element)
                    continue;

                int row = Grid.GetRow(element);
                if (row > titleRow)
                    continue;

                if (ContainsSignaturePair(element))
                    element.Visibility = Visibility.Collapsed;
            }
        }

        private static int FindTitleRow(Grid grid)
        {
            foreach (UIElement childElement in grid.Children)
            {
                if (childElement is FrameworkElement element && ContainsReportTitle(element))
                    return Grid.GetRow(element);
            }

            return -1;
        }

        private static bool ContainsReportTitle(DependencyObject root)
        {
            if (root is TextBlock textBlock)
            {
                string name = textBlock.Name ?? "";
                string text = textBlock.Text ?? "";
                if (name.Equals("TxtReportTitle", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("BÁO CÁO", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("BIỂU ĐỒ", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("DANH SÁCH", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("TỔNG HỢP", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("CHI TIẾT", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                if (ContainsReportTitle(VisualTreeHelper.GetChild(root, i)))
                    return true;
            }

            return false;
        }

        private static bool ContainsSignaturePair(DependencyObject root)
        {
            var texts = new List<string>();
            CollectTextBlocks(root, texts);
            bool hasManager = texts.Any(t => t.Contains("Trưởng phòng", StringComparison.OrdinalIgnoreCase));
            bool hasCreator = texts.Any(t => t.Contains("Người lập", StringComparison.OrdinalIgnoreCase));
            bool hasSignatureHint = texts.Any(t => t.Contains("Ký", StringComparison.OrdinalIgnoreCase) &&
                                                   t.Contains("họ tên", StringComparison.OrdinalIgnoreCase));
            return hasManager && hasCreator && hasSignatureHint;
        }

        private static void CollectTextBlocks(DependencyObject root, List<string> texts)
        {
            if (root is TextBlock textBlock && !string.IsNullOrWhiteSpace(textBlock.Text))
                texts.Add(textBlock.Text);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                CollectTextBlocks(VisualTreeHelper.GetChild(root, i), texts);
        }

        private static FrameworkElement? FindNamedElement(FrameworkElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.FindName(name) is FrameworkElement element)
                    return element;
            }
            return null;
        }

        private static FrameworkElement? FindSection(FrameworkElement contentRoot, FrameworkElement? marker)
        {
            if (marker == null)
                return null;
            if (ReferenceEquals(marker, contentRoot))
                return marker;

            DependencyObject current = marker;
            while (true)
            {
                var parent = VisualTreeHelper.GetParent(current);
                if (parent == null)
                    return marker;
                if (ReferenceEquals(parent, contentRoot))
                    return current as FrameworkElement ?? marker;
                current = parent;
            }
        }
    }
}
