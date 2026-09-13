using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchDatePickerWindow : Window
    {
        public DateTime? SelectedDate { get; private set; }
        private int _selectedDay;
        private int _selectedMonth;
        private int _selectedYear;
        private int _yearStart;

        public TouchDatePickerWindow(DateTime? initialDate = null)
        {
            InitializeComponent();
            var dt = initialDate ?? DateTime.Today;
            _selectedDay = dt.Day;
            _selectedMonth = dt.Month;
            _selectedYear = dt.Year;
            _yearStart = _selectedYear - 2;
            SelectedDate = dt;

            RenderAll();
        }

        public static DateTime? SelectDate(Window? owner, DateTime? initialDate = null)
        {
            var win = new TouchDatePickerWindow(initialDate ?? DateTime.Today);
            if (owner != null) win.Owner = owner;
            bool? result = win.ShowDialog();
            return result == true ? win.SelectedDate : null;
        }

        private void RenderAll()
        {
            // Clamp day to valid days in month/year
            int maxDays = DateTime.DaysInMonth(_selectedYear, _selectedMonth);
            if (_selectedDay > maxDays) _selectedDay = maxDays;

            if (SelectedDate.HasValue)
            {
                SelectedDate = new DateTime(_selectedYear, _selectedMonth, _selectedDay);
                TxtSelectedDateDisplay.Text = SelectedDate.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                TxtSelectedDateDisplay.Text = "";
            }

            RenderDays(maxDays);
            RenderMonths();
            RenderYears();
        }

        private void RenderDays(int maxDays)
        {
            UgDays.Children.Clear();
            var tealNormal = (Brush)new BrushConverter().ConvertFromString("#0B4F55")!;
            var tealBorder = (Brush)new BrushConverter().ConvertFromString("#1F7A85")!;
            var redSelected = (Brush)new BrushConverter().ConvertFromString("#D32F2F")!;
            var redBorder = (Brush)new BrushConverter().ConvertFromString("#FF5252")!;
            var greyNormal = (Brush)new BrushConverter().ConvertFromString("#616161")!;
            var greyBorder = (Brush)new BrushConverter().ConvertFromString("#888888")!;

            for (int d = 1; d <= 31; d++)
            {
                int dayVal = d;
                bool isSelected = SelectedDate.HasValue && dayVal == _selectedDay;
                bool isValid = dayVal <= maxDays;

                Brush bg = isSelected ? redSelected : (dayVal == 31 ? greyNormal : tealNormal);
                Brush border = isSelected ? redBorder : (dayVal == 31 ? greyBorder : tealBorder);

                var btn = CreateTouchButton(dayVal.ToString(), bg, border, isSelected);
                btn.IsEnabled = isValid;
                if (!isValid) btn.Opacity = 0.3;

                btn.Click += (s, e) =>
                {
                    _selectedDay = dayVal;
                    if (!SelectedDate.HasValue)
                    {
                        SelectedDate = new DateTime(_selectedYear, _selectedMonth, _selectedDay);
                    }
                    RenderAll();
                };

                UgDays.Children.Add(btn);
            }
        }

        private void RenderMonths()
        {
            UgMonths.Children.Clear();
            var oliveNormal = (Brush)new BrushConverter().ConvertFromString("#555500")!;
            var oliveBorder = (Brush)new BrushConverter().ConvertFromString("#777700")!;
            var redSelected = (Brush)new BrushConverter().ConvertFromString("#D32F2F")!;
            var redBorder = (Brush)new BrushConverter().ConvertFromString("#FF5252")!;

            for (int m = 1; m <= 12; m++)
            {
                int monthVal = m;
                bool isSelected = SelectedDate.HasValue && monthVal == _selectedMonth;

                Brush bg = isSelected ? redSelected : oliveNormal;
                Brush border = isSelected ? redBorder : oliveBorder;

                var btn = CreateTouchButton(monthVal.ToString(), bg, border, isSelected);
                btn.Click += (s, e) =>
                {
                    _selectedMonth = monthVal;
                    if (!SelectedDate.HasValue)
                    {
                        SelectedDate = new DateTime(_selectedYear, _selectedMonth, 1);
                    }
                    RenderAll();
                };

                UgMonths.Children.Add(btn);
            }
        }

        private void RenderYears()
        {
            UgYears.Children.Clear();
            var greenNormal = (Brush)new BrushConverter().ConvertFromString("#1B5E20")!;
            var greenBorder = (Brush)new BrushConverter().ConvertFromString("#388E3C")!;
            var redSelected = (Brush)new BrushConverter().ConvertFromString("#D32F2F")!;
            var redBorder = (Brush)new BrushConverter().ConvertFromString("#FF5252")!;

            for (int i = 0; i < 4; i++)
            {
                int yearVal = _yearStart + i;
                bool isSelected = SelectedDate.HasValue && yearVal == _selectedYear;

                Brush bg = isSelected ? redSelected : greenNormal;
                Brush border = isSelected ? redBorder : greenBorder;

                var btn = CreateTouchButton(yearVal.ToString(), bg, border, isSelected);
                btn.Click += (s, e) =>
                {
                    _selectedYear = yearVal;
                    if (!SelectedDate.HasValue)
                    {
                        SelectedDate = new DateTime(_selectedYear, 1, 1);
                    }
                    RenderAll();
                };

                UgYears.Children.Add(btn);
            }
        }

        private Button CreateTouchButton(string text, Brush bg, Brush border, bool isSelected)
        {
            var btn = new Button
            {
                Content = text,
                Margin = new Thickness(2),
                Cursor = Cursors.Hand,
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeights.Bold
            };

            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, bg);
            factory.SetValue(Border.BorderBrushProperty, border);
            factory.SetValue(Border.BorderThicknessProperty, isSelected ? new Thickness(2.5) : new Thickness(1.5));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            factory.AppendChild(presenter);
            template.VisualTree = factory;
            btn.Template = template;

            return btn;
        }

        private void BtnYearPrev_Click(object sender, RoutedEventArgs e)
        {
            _yearStart -= 4;
            RenderYears();
        }

        private void BtnYearNext_Click(object sender, RoutedEventArgs e)
        {
            _yearStart += 4;
            RenderYears();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            SelectedDate = null;
            TxtSelectedDateDisplay.Text = "";
            RenderAll();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedDate.HasValue)
            {
                SelectedDate = new DateTime(_selectedYear, _selectedMonth, _selectedDay);
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
