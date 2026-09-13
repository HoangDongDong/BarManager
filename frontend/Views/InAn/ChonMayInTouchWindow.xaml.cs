using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyBar.Client.Views.InAn
{
    public partial class ChonMayInTouchWindow : Window
    {
        public string? SelectedPrinter { get; private set; }

        public ChonMayInTouchWindow(string currentPrinter = "")
        {
            InitializeComponent();
            SelectedPrinter = currentPrinter;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var printers = new List<string>();
            try
            {
                foreach (string p in PrinterSettings.InstalledPrinters)
                {
                    printers.Add(p);
                }
            }
            catch { }

            if (printers.Count == 0)
            {
                printers.Add("Microsoft Print to PDF");
            }

            IcPrinters.ItemsSource = printers;
        }

        private void BtnPrinter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string printerName)
            {
                SelectedPrinter = printerName;
                DialogResult = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }
    }
}
