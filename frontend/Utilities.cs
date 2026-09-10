using System;

namespace QuanLyBar.Client.Views
{
    public static class Utilities
    {
        public static bool IsSpecificFilter(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            string t = text.Trim();
            if (t.Equals("Tất cả", StringComparison.OrdinalIgnoreCase)) return false;
            if (t.StartsWith("-- Tất cả", StringComparison.OrdinalIgnoreCase)) return false;
            if (t.EndsWith("-- Tất cả --", StringComparison.OrdinalIgnoreCase)) return false;
            if (t.Equals("-- Tất cả --", StringComparison.OrdinalIgnoreCase)) return false;
            if (t.Contains("Tất cả")) return false;
            if (t.Equals("ALL", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        public static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject depObj) where T : System.Windows.DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    System.Windows.DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
