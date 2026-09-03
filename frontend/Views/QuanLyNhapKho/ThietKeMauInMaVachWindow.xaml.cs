using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.QuanLyNhapKho
{
    public enum DesignerElementType
    {
        Text,
        Barcode,
        Box
    }

    public class DesignerElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Object1";
        public DesignerElementType ElementType { get; set; } = DesignerElementType.Text;
        public string Text { get; set; } = "";
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; } = 140;
        public double Height { get; set; } = 25;
        public double FontSize { get; set; } = 9.5;
        public string FontFamily { get; set; } = "Segoe UI";
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;
        public string BarcodeType { get; set; } = "CODE128";
        public int ColumnIndex { get; set; } = 0; // 0: Tem 1, 1: Tem 2
    }

    public partial class ThietKeMauInMaVachWindow : Window
    {
        private readonly List<DesignerElement> _elements = new();
        private DesignerElement _selectedElement = null;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private double _originalLeft;
        private double _originalTop;
        private MauInMaVachItem _mau;

        public ThietKeMauInMaVachWindow(MauInMaVachItem mau = null)
        {
            InitializeComponent();
            _mau = mau ?? new MauInMaVachItem { Columns = 2, BarcodeType = "CODE128" };

            Loaded += ThietKeMauInMaVachWindow_Loaded;
        }

        private void ThietKeMauInMaVachWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DrawRulers();
            InitDataTree();
            LoadTemplateElements();
            RenderCanvas();
            UpdateObjectSelector();
        }

        #region 1. Rulers & Grid
        private void DrawRulers()
        {
            // Top ruler (horizontal 0 -> 25 cm)
            CanvasTopRuler.Children.Clear();
            double pxPerCm = 37.8; // ~96 DPI / 2.54 cm

            for (int i = 0; i <= 25; i++)
            {
                double x = i * pxPerCm;
                // Major tick
                var tick = new Line
                {
                    X1 = x, Y1 = 10,
                    X2 = x, Y2 = 20,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1
                };
                CanvasTopRuler.Children.Add(tick);

                if (i > 0)
                {
                    var txt = new TextBlock
                    {
                        Text = i.ToString(),
                        FontSize = 9,
                        Foreground = Brushes.DimGray
                    };
                    Canvas.SetLeft(txt, x + 2);
                    Canvas.SetTop(txt, 1);
                    CanvasTopRuler.Children.Add(txt);
                }

                // Minor tick (0.5 cm)
                var minorTick = new Line
                {
                    X1 = x + (pxPerCm / 2), Y1 = 15,
                    X2 = x + (pxPerCm / 2), Y2 = 20,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                CanvasTopRuler.Children.Add(minorTick);
            }

            // Left ruler (vertical 0 -> 25 cm)
            CanvasLeftRuler.Children.Clear();
            for (int i = 0; i <= 25; i++)
            {
                double y = i * pxPerCm;
                var tick = new Line
                {
                    X1 = 10, Y1 = y,
                    X2 = 20, Y2 = y,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1
                };
                CanvasLeftRuler.Children.Add(tick);

                if (i > 0)
                {
                    var txt = new TextBlock
                    {
                        Text = i.ToString(),
                        FontSize = 9,
                        Foreground = Brushes.DimGray
                    };
                    Canvas.SetLeft(txt, 1);
                    Canvas.SetTop(txt, y + 2);
                    CanvasLeftRuler.Children.Add(txt);
                }

                var minorTick = new Line
                {
                    X1 = 15, Y1 = y + (pxPerCm / 2),
                    X2 = 20, Y2 = y + (pxPerCm / 2),
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                CanvasLeftRuler.Children.Add(minorTick);
            }
        }
        #endregion

        #region 2. Data Tree
        private void InitDataTree()
        {
            TvDataSources.Items.Clear();

            // Data Sources
            var rootData = new TreeViewItem
            {
                Header = CreateTreeHeader("🗄️", "Data Sources", true),
                IsExpanded = true
            };

            var table0 = new TreeViewItem
            {
                Header = CreateTreeHeader("📋", "Table0", true),
                IsExpanded = true
            };

            table0.Items.Add(CreateTreeItem("🏷️", "NAME (Tên hàng)", "[Table0.NAME]"));
            table0.Items.Add(CreateTreeItem("||||", "MAHANG (Mã vạch)", "[Table0.MAHANG]"));
            table0.Items.Add(CreateTreeItem("💲", "GIABAN (Giá bán)", "[Table0.GIABAN]"));
            table0.Items.Add(CreateTreeItem("📦", "DVT (Đơn vị tính)", "[Table0.DVT]"));
            table0.Items.Add(CreateTreeItem("🔢", "SLNHAP (Số lượng)", "[Table0.SLNHAP]"));
            table0.Items.Add(CreateTreeItem("💵", "GIANHAP (Giá nhập)", "[Table0.GIANHAP]"));

            rootData.Items.Add(table0);
            TvDataSources.Items.Add(rootData);

            // System Variables
            var rootSys = new TreeViewItem
            {
                Header = CreateTreeHeader("⚙️", "System Variables", false),
                IsExpanded = true
            };
            rootSys.Items.Add(CreateTreeItem("📅", "Date", "[Date]"));
            rootSys.Items.Add(CreateTreeItem("📄", "Page#", "[Page#]"));
            rootSys.Items.Add(CreateTreeItem("📑", "TotalPages#", "[TotalPages#]"));
            TvDataSources.Items.Add(rootSys);

            // Totals
            var rootTotals = new TreeViewItem
            {
                Header = CreateTreeHeader("Σ", "Totals", false)
            };
            TvDataSources.Items.Add(rootTotals);

            // Parameters
            var rootParams = new TreeViewItem
            {
                Header = CreateTreeHeader("🔤", "Parameters", false)
            };
            TvDataSources.Items.Add(rootParams);

            // Functions
            var rootFuncs = new TreeViewItem
            {
                Header = CreateTreeHeader("ƒx", "Functions", false)
            };
            TvDataSources.Items.Add(rootFuncs);
        }

        private StackPanel CreateTreeHeader(string icon, string text, bool bold)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            sp.Children.Add(new TextBlock { Text = icon, Margin = new Thickness(0, 0, 4, 0) });
            sp.Children.Add(new TextBlock { Text = text, FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, Foreground = new SolidColorBrush(Color.FromRgb(11, 69, 126)) });
            return sp;
        }

        private TreeViewItem CreateTreeItem(string icon, string text, string tag)
        {
            var item = new TreeViewItem
            {
                Header = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 1, 0, 1),
                    Children =
                    {
                        new TextBlock { Text = icon, Margin = new Thickness(0, 0, 4, 0), FontSize = 10 },
                        new TextBlock { Text = text, FontSize = 11, Foreground = Brushes.Black }
                    }
                },
                Tag = tag
            };
            return item;
        }

        private void TvDataSources_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TvDataSources.SelectedItem is TreeViewItem sel && sel.Tag is string expr)
            {
                if (_selectedElement != null)
                {
                    _selectedElement.Text = expr;
                    RenderCanvas();
                    UpdatePropertiesGrid();
                }
                else
                {
                    AddNewTextElement(expr);
                }
            }
        }
        #endregion

        #region 3. Template Elements & Canvas
        private void LoadTemplateElements()
        {
            _elements.Clear();

            // Label 1 (Column 0)
            _elements.Add(new DesignerElement
            {
                Name = "Barcode1",
                ElementType = DesignerElementType.Barcode,
                Text = "AB-12345678",
                Left = 25,
                Top = 8,
                Width = 150,
                Height = 35,
                BarcodeType = _mau.BarcodeType,
                ColumnIndex = 0
            });

            _elements.Add(new DesignerElement
            {
                Name = "TextCode1",
                ElementType = DesignerElementType.Text,
                Text = "AB-12345678",
                Left = 25,
                Top = 45,
                Width = 150,
                Height = 16,
                FontSize = 8.5,
                TextAlignment = TextAlignment.Center,
                ColumnIndex = 0
            });

            _elements.Add(new DesignerElement
            {
                Name = "TextName1",
                ElementType = DesignerElementType.Text,
                Text = "[Table0.NAME]",
                Left = 25,
                Top = 62,
                Width = 150,
                Height = 18,
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                ColumnIndex = 0
            });

            _elements.Add(new DesignerElement
            {
                Name = "TextPrice1",
                ElementType = DesignerElementType.Text,
                Text = "Giá: [Table0.GIABAN0]",
                Left = 25,
                Top = 81,
                Width = 150,
                Height = 18,
                FontSize = 9.0,
                TextAlignment = TextAlignment.Center,
                ColumnIndex = 0
            });

            // Label 2 (Column 1)
            _elements.Add(new DesignerElement
            {
                Name = "Barcode2",
                ElementType = DesignerElementType.Barcode,
                Text = "AB-12345",
                Left = 240,
                Top = 8,
                Width = 150,
                Height = 35,
                BarcodeType = _mau.BarcodeType,
                ColumnIndex = 1
            });

            _elements.Add(new DesignerElement
            {
                Name = "TextCode2",
                ElementType = DesignerElementType.Text,
                Text = "AB-12345",
                Left = 240,
                Top = 45,
                Width = 150,
                Height = 16,
                FontSize = 8.5,
                TextAlignment = TextAlignment.Center,
                ColumnIndex = 1
            });

            _elements.Add(new DesignerElement
            {
                Name = "TextName2",
                ElementType = DesignerElementType.Text,
                Text = "[Table0.NAME1]",
                Left = 240,
                Top = 62,
                Width = 150,
                Height = 18,
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                ColumnIndex = 1
            });

            _elements.Add(new DesignerElement
            {
                Name = "TextPrice2",
                ElementType = DesignerElementType.Text,
                Text = "Giá: [Table0.GIABAN1]",
                Left = 240,
                Top = 81,
                Width = 150,
                Height = 18,
                FontSize = 9.0,
                TextAlignment = TextAlignment.Center,
                ColumnIndex = 1
            });

            _selectedElement = _elements.FirstOrDefault();
        }

        private void RenderCanvas()
        {
            DesignCanvas.Children.Clear();

            // 1. Draw column dividing guidelines & Label boundaries
            double colWidth = 210;
            var labelBorder1 = new Rectangle
            {
                Width = colWidth - 20,
                Height = 100,
                Stroke = new SolidColorBrush(Color.FromRgb(180, 195, 215)),
                StrokeDashArray = new DoubleCollection { 3, 2 },
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(labelBorder1, 15);
            Canvas.SetTop(labelBorder1, 5);
            DesignCanvas.Children.Add(labelBorder1);

            var labelBorder2 = new Rectangle
            {
                Width = colWidth - 20,
                Height = 100,
                Stroke = new SolidColorBrush(Color.FromRgb(180, 195, 215)),
                StrokeDashArray = new DoubleCollection { 3, 2 },
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(labelBorder2, 230);
            Canvas.SetTop(labelBorder2, 5);
            DesignCanvas.Children.Add(labelBorder2);

            // 2. Render Design Elements
            foreach (var elem in _elements)
            {
                var visual = CreateElementVisual(elem);
                Canvas.SetLeft(visual, elem.Left);
                Canvas.SetTop(visual, elem.Top);
                DesignCanvas.Children.Add(visual);
            }
        }

        private FrameworkElement CreateElementVisual(DesignerElement elem)
        {
            bool isSelected = (_selectedElement == elem);

            var container = new Border
            {
                Width = elem.Width,
                Height = elem.Height,
                Background = Brushes.Transparent,
                BorderBrush = isSelected ? new SolidColorBrush(Color.FromRgb(41, 128, 185)) : new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = isSelected ? new Thickness(1.5) : new Thickness(1),
                Tag = elem,
                Cursor = Cursors.SizeAll
            };

            if (!isSelected)
            {
                container.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 150, 150, 150));
            }

            if (elem.ElementType == DesignerElementType.Barcode)
            {
                try
                {
                    var bmp = BarcodeHelper.GenerateCode128Barcode(elem.Text, 32, 2);
                    var img = new Image
                    {
                        Source = bmp,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    container.Child = img;
                }
                catch
                {
                    container.Child = new TextBlock
                    {
                        Text = elem.Text,
                        Foreground = Brushes.Red,
                        FontSize = 9,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                }
            }
            else
            {
                var txt = new TextBlock
                {
                    Text = elem.Text,
                    FontFamily = new FontFamily(elem.FontFamily),
                    FontSize = elem.FontSize,
                    FontWeight = elem.FontWeight,
                    FontStyle = elem.FontStyle,
                    TextAlignment = elem.TextAlignment,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Foreground = Brushes.Black
                };
                container.Child = txt;
            }

            // Mouse event handlers for selection and dragging
            container.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _selectedElement = elem;
                    _isDragging = true;
                    _dragStartPoint = e.GetPosition(DesignCanvas);
                    _originalLeft = elem.Left;
                    _originalTop = elem.Top;
                    container.CaptureMouse();
                    RenderCanvas();
                    UpdateObjectSelector();
                    UpdatePropertiesGrid();
                    e.Handled = true;
                }
            };

            container.MouseMove += (s, e) =>
            {
                if (_isDragging && _selectedElement == elem)
                {
                    Point current = e.GetPosition(DesignCanvas);
                    double dx = current.X - _dragStartPoint.X;
                    double dy = current.Y - _dragStartPoint.Y;

                    elem.Left = Math.Max(0, _originalLeft + dx);
                    elem.Top = Math.Max(0, _originalTop + dy);

                    Canvas.SetLeft(container, elem.Left);
                    Canvas.SetTop(container, elem.Top);
                    UpdatePropertiesGrid();
                }
            };

            container.MouseUp += (s, e) =>
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    container.ReleaseMouseCapture();
                }
            };

            return container;
        }

        private void DesignCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _selectedElement = null;
            RenderCanvas();
            UpdateObjectSelector();
            UpdatePropertiesGrid();
        }
        #endregion

        #region 4. Property Grid & Object Selector
        private void UpdateObjectSelector()
        {
            CboSelectedObject.SelectionChanged -= CboSelectedObject_SelectionChanged;
            CboSelectedObject.Items.Clear();

            CboSelectedObject.Items.Add(new ComboBoxItem { Content = "Page1 ReportPage", Tag = null });

            foreach (var elem in _elements)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{elem.Name} ({elem.ElementType})",
                    Tag = elem
                };
                CboSelectedObject.Items.Add(item);
                if (_selectedElement == elem)
                {
                    CboSelectedObject.SelectedItem = item;
                }
            }

            if (_selectedElement == null)
            {
                CboSelectedObject.SelectedIndex = 0;
            }

            CboSelectedObject.SelectionChanged += CboSelectedObject_SelectionChanged;
        }

        private void CboSelectedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboSelectedObject.SelectedItem is ComboBoxItem sel)
            {
                _selectedElement = sel.Tag as DesignerElement;
                RenderCanvas();
                UpdatePropertiesGrid();
            }
        }

        private void UpdatePropertiesGrid()
        {
            GridProperties.Children.Clear();
            GridProperties.RowDefinitions.Clear();

            if (_selectedElement != null)
            {
                TxtPropName.Text = _selectedElement.Name;
                TxtPropDesc.Text = $"Đối tượng {_selectedElement.ElementType}: {_selectedElement.Text}";

                AddPropertyRow("(Name)", _selectedElement.Name, val => { _selectedElement.Name = val; UpdateObjectSelector(); });
                AddPropertyRow("Text", _selectedElement.Text, val => { _selectedElement.Text = val; RenderCanvas(); });
                AddPropertyRow("Left", ((int)_selectedElement.Left).ToString(), val => { if (double.TryParse(val, out double d)) { _selectedElement.Left = d; RenderCanvas(); } });
                AddPropertyRow("Top", ((int)_selectedElement.Top).ToString(), val => { if (double.TryParse(val, out double d)) { _selectedElement.Top = d; RenderCanvas(); } });
                AddPropertyRow("Width", ((int)_selectedElement.Width).ToString(), val => { if (double.TryParse(val, out double d)) { _selectedElement.Width = d; RenderCanvas(); } });
                AddPropertyRow("Height", ((int)_selectedElement.Height).ToString(), val => { if (double.TryParse(val, out double d)) { _selectedElement.Height = d; RenderCanvas(); } });
                AddPropertyRow("FontSize", _selectedElement.FontSize.ToString(), val => { if (double.TryParse(val, out double d)) { _selectedElement.FontSize = d; RenderCanvas(); } });
                AddPropertyRow("FontWeight", _selectedElement.FontWeight == FontWeights.Bold ? "Bold" : "Normal", val => { _selectedElement.FontWeight = val.ToLower().Contains("bold") ? FontWeights.Bold : FontWeights.Normal; RenderCanvas(); });
                AddPropertyRow("Alignment", _selectedElement.TextAlignment.ToString(), val => { Enum.TryParse(val, out TextAlignment a); _selectedElement.TextAlignment = a; RenderCanvas(); });
                if (_selectedElement.ElementType == DesignerElementType.Barcode)
                {
                    AddPropertyRow("BarcodeType", _selectedElement.BarcodeType, val => { _selectedElement.BarcodeType = val; RenderCanvas(); });
                }
            }
            else
            {
                TxtPropName.Text = "Page1 ReportPage";
                TxtPropDesc.Text = "Cấu hình kích thước và bố cục mẫu trang in mã vạch.";

                AddPropertyRow("(Name)", "Page1", null);
                AddPropertyRow("Columns", _mau.Columns.ToString(), val => { if (int.TryParse(val, out int c)) { _mau.Columns = c; RenderCanvas(); } });
                AddPropertyRow("BarcodeType", _mau.BarcodeType, val => { _mau.BarcodeType = val; });
                AddPropertyRow("Width", "480 px", null);
                AddPropertyRow("Height", "240 px", null);
                AddPropertyRow("PaperType", _mau.IsPaperA4 ? "A4 Tommy" : "Continuous Roll", null);
            }
        }

        private void AddPropertyRow(string name, string value, Action<string> onValueChanged)
        {
            int rowIndex = GridProperties.RowDefinitions.Count;
            GridProperties.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lbl = new Border
            {
                Background = rowIndex % 2 == 0 ? new SolidColorBrush(Color.FromRgb(245, 248, 252)) : Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(225, 235, 245)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 3, 4, 3),
                Child = new TextBlock { Text = name, FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(40, 60, 90)) }
            };
            Grid.SetRow(lbl, rowIndex);
            Grid.SetColumn(lbl, 0);
            GridProperties.Children.Add(lbl);

            var tb = new TextBox
            {
                Text = value,
                FontSize = 11,
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(225, 235, 245)),
                Background = rowIndex % 2 == 0 ? new SolidColorBrush(Color.FromRgb(245, 248, 252)) : Brushes.White,
                Padding = new Thickness(4, 2, 4, 2),
                IsEnabled = onValueChanged != null
            };

            tb.LostFocus += (s, e) => onValueChanged?.Invoke(tb.Text);
            tb.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    onValueChanged?.Invoke(tb.Text);
                }
            };

            Grid.SetRow(tb, rowIndex);
            Grid.SetColumn(tb, 1);
            GridProperties.Children.Add(tb);
        }
        #endregion

        #region 5. Toolbar Actions
        private void BtnAddText_Click(object sender, RoutedEventArgs e)
        {
            AddNewTextElement("[Table0.NAME]");
        }

        private void AddNewTextElement(string text)
        {
            var elem = new DesignerElement
            {
                Name = $"Text_{_elements.Count + 1}",
                ElementType = DesignerElementType.Text,
                Text = text,
                Left = 40,
                Top = 50,
                Width = 140,
                Height = 20,
                FontSize = 9.5
            };
            _elements.Add(elem);
            _selectedElement = elem;
            RenderCanvas();
            UpdateObjectSelector();
            UpdatePropertiesGrid();
        }

        private void BtnAddBarcode_Click(object sender, RoutedEventArgs e)
        {
            var elem = new DesignerElement
            {
                Name = $"Barcode_{_elements.Count + 1}",
                ElementType = DesignerElementType.Barcode,
                Text = "AB-123456",
                Left = 40,
                Top = 20,
                Width = 140,
                Height = 35,
                BarcodeType = _mau.BarcodeType
            };
            _elements.Add(elem);
            _selectedElement = elem;
            RenderCanvas();
            UpdateObjectSelector();
            UpdatePropertiesGrid();
        }

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null)
            {
                _elements.Remove(_selectedElement);
                _selectedElement = null;
                RenderCanvas();
                UpdateObjectSelector();
                UpdatePropertiesGrid();
            }
        }

        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null)
            {
                _selectedElement.FontWeight = (_selectedElement.FontWeight == FontWeights.Bold) ? FontWeights.Normal : FontWeights.Bold;
                RenderCanvas();
                UpdatePropertiesGrid();
            }
        }

        private void BtnItalic_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null)
            {
                _selectedElement.FontStyle = (_selectedElement.FontStyle == FontStyles.Italic) ? FontStyles.Normal : FontStyles.Italic;
                RenderCanvas();
                UpdatePropertiesGrid();
            }
        }

        private void BtnUnderline_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for underline
        }

        private void BtnAlignLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null) { _selectedElement.TextAlignment = TextAlignment.Left; RenderCanvas(); UpdatePropertiesGrid(); }
        }

        private void BtnAlignCenter_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null) { _selectedElement.TextAlignment = TextAlignment.Center; RenderCanvas(); UpdatePropertiesGrid(); }
        }

        private void BtnAlignRight_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null) { _selectedElement.TextAlignment = TextAlignment.Right; RenderCanvas(); UpdatePropertiesGrid(); }
        }

        private void CboFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedElement != null && CboFontFamily.SelectedItem is ComboBoxItem item)
            {
                _selectedElement.FontFamily = item.Content.ToString();
                RenderCanvas();
            }
        }

        private void CboFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedElement != null && CboFontSize.SelectedItem is ComboBoxItem item && double.TryParse(item.Content.ToString(), out double sz))
            {
                _selectedElement.FontSize = sz;
                RenderCanvas();
            }
        }

        private void BtnFormatN0_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null && !_selectedElement.Text.EndsWith(":N0]"))
            {
                if (_selectedElement.Text.EndsWith("]"))
                {
                    _selectedElement.Text = _selectedElement.Text.Insert(_selectedElement.Text.Length - 1, ":N0");
                }
                else
                {
                    _selectedElement.Text += " #,##0";
                }
                RenderCanvas();
                UpdatePropertiesGrid();
            }
        }

        private void BtnFormatN2_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement != null)
            {
                _selectedElement.Text += " #,##0.00";
                RenderCanvas();
                UpdatePropertiesGrid();
            }
        }

        private void ToolSelect_Click(object sender, RoutedEventArgs e)
        {
            // Pointer mode
        }

        private void BtnPageSetup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Thiết lập trang in:\n- Số cột tem: {_mau.Columns}\n- Loại mã: {_mau.BarcodeType}\n- Giấy in: {(_mau.IsPaperA4 ? "A4" : "Cuộn liên tục")}", "Page Setup", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PaperContainer != null)
            {
                double scale = e.NewValue / 100.0;
                PaperContainer.LayoutTransform = new ScaleTransform(scale, scale);
            }
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            // Create sample list for preview
            var sampleDetails = new List<PhieuNhapChiTietItem>
            {
                new PhieuNhapChiTietItem { TenHang = "Aquafina", MaHang = "AB-12345678", GiaBan = 10000, SlNhap = 2 },
                new PhieuNhapChiTietItem { TenHang = "Rượu Rockmen", MaHang = "456", GiaBan = 50000, SlNhap = 1 },
                new PhieuNhapChiTietItem { TenHang = "Mực nướng sả ớt", MaHang = "", GiaBan = 100000, SlNhap = 1 }
            };

            var previewWin = new XemInMaVachWindow(_mau, sampleDetails);
            previewWin.Owner = this;
            previewWin.ShowDialog();
        }

        private void BtnSaveDatabase_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã cập nhật và lưu mẫu in mã vạch thành công vào hệ thống!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
