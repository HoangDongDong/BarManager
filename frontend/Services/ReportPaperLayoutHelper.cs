using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanLyBar.Client.Services
{
    public static class ReportPaperLayoutHelper
    {
        public static void ApplyReportPaperLayout(
            Border reportPaper,
            ReportPaperLayout? layout,
            ColumnDefinition? colLogo = null,
            Border? brdLogo = null,
            StackPanel? pnlCompanyText = null,
            TextBlock? txtCompanyName = null,
            TextBlock? txtCompanyAddress = null,
            TextBlock? txtCompanyContact = null,
            Grid? gridTitleArea = null,
            ColumnDefinition? colTitleLeft = null,
            ColumnDefinition? colTitleRight = null,
            TextBlock? txtReportTitle = null,
            StackPanel? pnlDateAndFilter = null,
            TextBlock? txtSubTitleDate = null,
            TextBlock? txtFilterSummary = null,
            Grid? gridSignatures = null,
            ColumnDefinition? sigCol0 = null,
            ColumnDefinition? sigCol1 = null,
            ColumnDefinition? sigCol2 = null,
            ColumnDefinition? sigCol3 = null,
            StackPanel? sigBlock0 = null,
            StackPanel? sigBlock1 = null,
            StackPanel? sigBlock2 = null,
            StackPanel? sigBlock3 = null)
        {
            if (reportPaper == null || layout == null) return;

            reportPaper.Width = layout.PaperWidth;
            reportPaper.MinHeight = layout.PaperMinHeight;
            reportPaper.Padding = layout.PaperPadding;

            if (layout.Is80mm)
            {
                // 1. Thông tin Quán & Logo (Thu gọn in nhiệt)
                if (colLogo != null) colLogo.Width = new GridLength(0);
                if (brdLogo != null) brdLogo.Visibility = Visibility.Collapsed;
                if (pnlCompanyText != null) pnlCompanyText.HorizontalAlignment = HorizontalAlignment.Center;
                if (txtCompanyName != null)
                {
                    txtCompanyName.HorizontalAlignment = HorizontalAlignment.Center;
                    txtCompanyName.FontSize = 13;
                }
                if (txtCompanyAddress != null)
                {
                    txtCompanyAddress.HorizontalAlignment = HorizontalAlignment.Center;
                    txtCompanyAddress.FontSize = 9.5;
                    txtCompanyAddress.TextWrapping = TextWrapping.Wrap;
                }
                if (txtCompanyContact != null)
                {
                    txtCompanyContact.HorizontalAlignment = HorizontalAlignment.Center;
                    txtCompanyContact.FontSize = 9.5;
                }

                // 2. Tiêu đề báo cáo & Ngày lọc
                if (gridTitleArea != null)
                {
                    gridTitleArea.RowDefinitions.Clear();
                    gridTitleArea.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    gridTitleArea.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                if (colTitleLeft != null) colTitleLeft.Width = new GridLength(1, GridUnitType.Star);
                if (colTitleRight != null) colTitleRight.Width = new GridLength(0);

                if (txtReportTitle != null)
                {
                    Grid.SetRow(txtReportTitle, 0);
                    Grid.SetColumn(txtReportTitle, 0);
                    Grid.SetColumnSpan(txtReportTitle, 2);
                    txtReportTitle.HorizontalAlignment = HorizontalAlignment.Center;
                    txtReportTitle.FontSize = 13;
                    txtReportTitle.TextWrapping = TextWrapping.Wrap;
                    txtReportTitle.Margin = new Thickness(0, 0, 0, 6);
                }

                if (pnlDateAndFilter != null)
                {
                    Grid.SetRow(pnlDateAndFilter, 1);
                    Grid.SetColumn(pnlDateAndFilter, 0);
                    Grid.SetColumnSpan(pnlDateAndFilter, 2);
                    pnlDateAndFilter.HorizontalAlignment = HorizontalAlignment.Center;
                }
                if (txtSubTitleDate != null)
                {
                    txtSubTitleDate.HorizontalAlignment = HorizontalAlignment.Center;
                    txtSubTitleDate.FontSize = 10;
                }
                if (txtFilterSummary != null)
                {
                    txtFilterSummary.HorizontalAlignment = HorizontalAlignment.Center;
                    txtFilterSummary.FontSize = 9;
                }

                // 3. Chữ ký (2 cột thu gọn)
                if (sigCol0 != null) sigCol0.Width = new GridLength(1, GridUnitType.Star);
                if (sigCol1 != null) sigCol1.Width = new GridLength(0);
                if (sigCol2 != null) sigCol2.Width = new GridLength(0);
                if (sigCol3 != null) sigCol3.Width = new GridLength(1, GridUnitType.Star);

                if (sigBlock0 != null) sigBlock0.Visibility = Visibility.Visible;
                if (sigBlock1 != null) sigBlock1.Visibility = Visibility.Collapsed;
                if (sigBlock2 != null) sigBlock2.Visibility = Visibility.Collapsed;
                if (sigBlock3 != null) sigBlock3.Visibility = Visibility.Visible;
            }
            else
            {
                // A4 (Thẳng đứng hoặc Nằm ngang)
                if (colLogo != null) colLogo.Width = new GridLength(100);
                if (brdLogo != null) brdLogo.Visibility = Visibility.Visible;
                if (pnlCompanyText != null) pnlCompanyText.HorizontalAlignment = HorizontalAlignment.Right;
                if (txtCompanyName != null)
                {
                    txtCompanyName.HorizontalAlignment = HorizontalAlignment.Right;
                    txtCompanyName.FontSize = 16;
                }
                if (txtCompanyAddress != null)
                {
                    txtCompanyAddress.HorizontalAlignment = HorizontalAlignment.Right;
                    txtCompanyAddress.FontSize = 10.5;
                }
                if (txtCompanyContact != null)
                {
                    txtCompanyContact.HorizontalAlignment = HorizontalAlignment.Right;
                    txtCompanyContact.FontSize = 10.5;
                }

                // Tiêu đề báo cáo
                if (gridTitleArea != null)
                {
                    gridTitleArea.RowDefinitions.Clear();
                    gridTitleArea.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                if (colTitleLeft != null) colTitleLeft.Width = new GridLength(1, GridUnitType.Star);
                if (colTitleRight != null) colTitleRight.Width = new GridLength(1, GridUnitType.Auto);

                if (txtReportTitle != null)
                {
                    Grid.SetRow(txtReportTitle, 0);
                    Grid.SetColumn(txtReportTitle, 1);
                    Grid.SetColumnSpan(txtReportTitle, 1);
                    txtReportTitle.HorizontalAlignment = HorizontalAlignment.Right;
                    txtReportTitle.FontSize = layout.TitleFontSize;
                    txtReportTitle.Margin = new Thickness(0);
                }

                if (pnlDateAndFilter != null)
                {
                    Grid.SetRow(pnlDateAndFilter, 0);
                    Grid.SetColumn(pnlDateAndFilter, 0);
                    Grid.SetColumnSpan(pnlDateAndFilter, 1);
                    pnlDateAndFilter.HorizontalAlignment = HorizontalAlignment.Left;
                }
                if (txtSubTitleDate != null)
                {
                    txtSubTitleDate.HorizontalAlignment = HorizontalAlignment.Left;
                    txtSubTitleDate.FontSize = 11.5;
                }
                if (txtFilterSummary != null)
                {
                    txtFilterSummary.HorizontalAlignment = HorizontalAlignment.Left;
                    txtFilterSummary.FontSize = 10.5;
                }

                // Chữ ký (4 cột đầy đủ)
                if (sigCol0 != null) sigCol0.Width = new GridLength(1, GridUnitType.Star);
                if (sigCol1 != null) sigCol1.Width = new GridLength(1, GridUnitType.Star);
                if (sigCol2 != null) sigCol2.Width = new GridLength(1, GridUnitType.Star);
                if (sigCol3 != null) sigCol3.Width = new GridLength(1, GridUnitType.Star);

                if (sigBlock0 != null) sigBlock0.Visibility = Visibility.Visible;
                if (sigBlock1 != null) sigBlock1.Visibility = Visibility.Visible;
                if (sigBlock2 != null) sigBlock2.Visibility = Visibility.Visible;
                if (sigBlock3 != null) sigBlock3.Visibility = Visibility.Visible;
            }
        }

        public static double GetScaleFactor(this ReportPaperLayout? layout, double baseWidth = 720.0)
        {
            if (layout == null) return 1.0;
            double contentWidth = layout.PaperWidth - layout.PaperPadding.Left - layout.PaperPadding.Right;
            if (contentWidth <= 0 || baseWidth <= 0) return 1.0;
            return contentWidth / baseWidth;
        }

        public static Border CreateHeaderCell(string text, int col, ReportPaperLayout? layout, bool isLast = false)
        {
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontSize = layout?.HeaderFontSize ?? 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            return b;
        }

        public static Border CreateDataCell(string text, int col, HorizontalAlignment align, ReportPaperLayout? layout, bool isBold = false, bool isLast = false)
        {
            bool is80 = layout?.Is80mm == true;
            var b = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = isLast ? new Thickness(0) : new Thickness(0, 0, 1, 0),
                Padding = is80 ? new Thickness(2, 0, 2, 0) : new Thickness(4, 0, 4, 0)
            };
            Grid.SetColumn(b, col);
            b.Child = new TextBlock
            {
                Text = text,
                FontSize = layout?.CellFontSize ?? 12,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            return b;
        }
    }
}
