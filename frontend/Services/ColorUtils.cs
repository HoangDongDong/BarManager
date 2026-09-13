using System;
using System.Globalization;

namespace QuanLyBar.Client.Services
{
    public static class ColorUtils
    {
        /// <summary>
        /// Convert hex string (e.g. "#1976D2", "#FF1976D2", "1976D2") or integer string to Firebird ARGB integer format.
        /// </summary>
        public static int HexToColorInt(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return 0;
            hex = hex.Trim();

            // If already a signed integer string like "-15108398"
            if (int.TryParse(hex, out int existingInt)) return existingInt;

            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length == 6) hex = "FF" + hex;
            
            if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb))
            {
                return (int)argb;
            }
            return 0;
        }

        /// <summary>
        /// Convert Firebird DB MAUSAC value (int/string) to standard WPF Hex string "#RRGGBB" or "#AARRGGBB".
        /// </summary>
        public static string ColorIntToHex(object? colorDbValue, string fallbackHex = "#5B7F95")
        {
            if (colorDbValue == null || colorDbValue == DBNull.Value) return fallbackHex;
            string str = colorDbValue.ToString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(str)) return fallbackHex;

            // If it's already a valid hex starting with '#'
            if (str.StartsWith("#"))
            {
                if (str.Length == 7 || str.Length == 9) return str;
                if (str.Length == 4) // #RGB format
                {
                    return $"#{str[1]}{str[1]}{str[2]}{str[2]}{str[3]}{str[3]}";
                }
            }

            // If it's a signed integer like -15108398 or -8388608
            if (int.TryParse(str, out int argb))
            {
                if (argb == 0) return fallbackHex;
                uint uargb = (uint)argb;
                string fullHex = uargb.ToString("X8");
                if (fullHex.StartsWith("FF", StringComparison.OrdinalIgnoreCase))
                {
                    return "#" + fullHex.Substring(2);
                }
                return "#" + fullHex;
            }

            // If it's pure 6 or 8 hex characters without '#'
            if (str.Length == 6 || str.Length == 8)
            {
                return "#" + str;
            }

            return fallbackHex;
        }

        /// <summary>
        /// Determine whether text should be dark (#1A1A1A) or light (#FFFFFF) based on background hex color.
        /// </summary>
        public static string GetContrastTextColor(string? hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor)) return "#FFFFFF";
            try
            {
                string hex = hexColor.Trim();
                if (hex.StartsWith("#")) hex = hex.Substring(1);
                if (hex.Length == 8) hex = hex.Substring(2); // Remove Alpha channel
                if (hex.Length == 6)
                {
                    int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    double brightness = (r * 299 + g * 587 + b * 114) / 1000.0;
                    return brightness > 140 ? "#1A1A1A" : "#FFFFFF";
                }
            }
            catch { }
            return "#FFFFFF";
        }
    }
}
