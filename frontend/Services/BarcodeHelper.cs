using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuanLyBar.Client.Services
{
    public static class BarcodeHelper
    {
        // Code 128 patterns: 11 modules per character (lengths of alternating bars and spaces)
        private static readonly int[][] Code128Patterns = new int[][]
        {
            new int[]{2,1,2,2,2,2}, // 0
            new int[]{2,2,2,1,2,2}, // 1
            new int[]{2,2,2,2,2,1}, // 2
            new int[]{1,2,1,2,2,3}, // 3
            new int[]{1,2,1,3,2,2}, // 4
            new int[]{1,3,1,2,2,2}, // 5
            new int[]{1,2,2,2,1,3}, // 6
            new int[]{1,2,2,3,1,2}, // 7
            new int[]{1,3,2,2,1,2}, // 8
            new int[]{2,2,1,2,1,3}, // 9
            new int[]{2,2,1,3,1,2}, // 10
            new int[]{2,3,1,2,1,2}, // 11
            new int[]{1,1,2,2,3,2}, // 12
            new int[]{1,2,2,1,3,2}, // 13
            new int[]{1,2,2,2,3,1}, // 14
            new int[]{1,1,3,2,2,2}, // 15
            new int[]{1,2,3,1,2,2}, // 16
            new int[]{1,2,3,2,2,1}, // 17
            new int[]{2,2,3,2,1,1}, // 18
            new int[]{2,2,1,1,3,2}, // 19
            new int[]{2,2,1,2,3,1}, // 20
            new int[]{2,1,3,2,1,2}, // 21
            new int[]{2,2,3,1,1,2}, // 22
            new int[]{3,1,2,1,3,1}, // 23
            new int[]{3,1,1,2,2,2}, // 24
            new int[]{3,2,1,1,2,2}, // 25
            new int[]{3,2,1,2,2,1}, // 26
            new int[]{3,1,2,2,1,2}, // 27
            new int[]{3,2,2,1,1,2}, // 28
            new int[]{3,2,2,2,1,1}, // 29
            new int[]{2,1,2,1,2,3}, // 30
            new int[]{2,1,2,3,2,1}, // 31
            new int[]{2,3,2,1,2,1}, // 32
            new int[]{1,1,1,3,2,3}, // 33
            new int[]{1,3,1,1,2,3}, // 34
            new int[]{1,3,1,3,2,1}, // 35
            new int[]{1,1,2,3,1,3}, // 36
            new int[]{1,3,2,1,1,3}, // 37
            new int[]{1,3,2,3,1,1}, // 38
            new int[]{2,1,1,3,1,3}, // 39
            new int[]{2,3,1,1,1,3}, // 40
            new int[]{2,3,1,3,1,1}, // 41
            new int[]{1,1,2,1,3,3}, // 42
            new int[]{1,1,2,3,3,1}, // 43
            new int[]{1,3,2,1,3,1}, // 44
            new int[]{1,1,3,1,2,3}, // 45
            new int[]{1,1,3,3,2,1}, // 46
            new int[]{1,3,3,1,2,1}, // 47
            new int[]{3,1,3,1,2,1}, // 48
            new int[]{2,1,1,3,3,1}, // 49
            new int[]{2,3,1,1,3,1}, // 50
            new int[]{2,1,3,1,1,3}, // 51
            new int[]{2,1,3,3,1,1}, // 52
            new int[]{2,1,3,1,3,1}, // 53
            new int[]{3,1,1,1,2,3}, // 54
            new int[]{3,1,1,3,2,1}, // 55
            new int[]{3,3,1,1,2,1}, // 56
            new int[]{3,1,2,1,1,3}, // 57
            new int[]{3,1,2,3,1,1}, // 58
            new int[]{3,3,2,1,1,1}, // 59
            new int[]{3,1,4,1,1,1}, // 60
            new int[]{2,2,1,4,1,1}, // 61
            new int[]{4,3,1,1,1,1}, // 62
            new int[]{1,1,1,2,2,4}, // 63
            new int[]{1,1,1,4,2,2}, // 64
            new int[]{1,2,1,1,2,4}, // 65
            new int[]{1,2,1,4,2,1}, // 66
            new int[]{1,4,1,1,2,2}, // 67
            new int[]{1,4,1,2,2,1}, // 68
            new int[]{1,1,2,2,1,4}, // 69
            new int[]{1,1,2,4,1,2}, // 70
            new int[]{1,2,2,1,1,4}, // 71
            new int[]{1,2,2,4,1,1}, // 72
            new int[]{1,4,2,1,1,2}, // 73
            new int[]{1,4,2,2,1,1}, // 74
            new int[]{2,4,1,2,1,1}, // 75
            new int[]{2,2,1,1,1,4}, // 76
            new int[]{4,1,3,1,1,1}, // 77
            new int[]{2,4,1,1,1,2}, // 78
            new int[]{1,3,4,1,1,1}, // 79
            new int[]{1,1,1,2,4,2}, // 80
            new int[]{1,2,1,1,4,2}, // 81
            new int[]{1,2,1,2,4,1}, // 82
            new int[]{1,1,4,2,1,2}, // 83
            new int[]{1,2,4,1,1,2}, // 84
            new int[]{1,2,4,2,1,1}, // 85
            new int[]{4,1,1,2,1,2}, // 86
            new int[]{4,2,1,1,1,2}, // 87
            new int[]{4,2,1,2,1,1}, // 88
            new int[]{2,1,2,1,4,1}, // 89
            new int[]{2,1,4,1,2,1}, // 90
            new int[]{4,1,2,1,2,1}, // 91
            new int[]{1,1,1,1,4,3}, // 92
            new int[]{1,1,1,3,4,1}, // 93
            new int[]{1,3,1,1,4,1}, // 94
            new int[]{1,1,4,1,1,3}, // 95
            new int[]{1,1,4,3,1,1}, // 96
            new int[]{4,1,1,1,1,3}, // 97
            new int[]{4,1,1,3,1,1}, // 98
            new int[]{1,1,3,1,4,1}, // 99
            new int[]{1,1,4,1,3,1}, // 100
            new int[]{3,1,1,1,4,1}, // 101
            new int[]{4,1,1,1,3,1}, // 102
            new int[]{2,1,1,4,1,2}, // 103: Start A
            new int[]{2,1,1,2,1,4}, // 104: Start B
            new int[]{2,1,1,2,3,2}, // 105: Start C
            new int[]{2,3,3,1,1,1,2} // 106: Stop (7 elements)
        };

        public static BitmapSource GenerateCode128Barcode(string content, int height = 50, int barWidth = 2)
        {
            if (string.IsNullOrEmpty(content)) content = "0000";

            // Encode using Code 128 Set B
            List<int> patternIndices = new List<int>();
            int startB = 104;
            patternIndices.Add(startB);

            long checkSum = startB;
            for (int i = 0; i < content.Length; i++)
            {
                int charVal = (int)content[i] - 32;
                if (charVal < 0 || charVal > 95) charVal = 0;
                patternIndices.Add(charVal);
                checkSum += (long)charVal * (i + 1);
            }

            int checkDigit = (int)(checkSum % 103);
            patternIndices.Add(checkDigit);
            patternIndices.Add(106); // Stop pattern

            // Convert to boolean bar/space modules
            List<bool> modules = new List<bool>();
            // Quiet zone
            for (int i = 0; i < 10; i++) modules.Add(false);

            foreach (int pIndex in patternIndices)
            {
                int[] pat = Code128Patterns[pIndex];
                bool isBar = true;
                foreach (int len in pat)
                {
                    for (int l = 0; l < len; l++)
                    {
                        modules.Add(isBar);
                    }
                    isBar = !isBar;
                }
            }

            // Quiet zone
            for (int i = 0; i < 10; i++) modules.Add(false);

            int totalWidth = modules.Count * barWidth;
            int totalHeight = height;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, totalWidth, totalHeight));
                var brush = Brushes.Black;

                for (int m = 0; m < modules.Count; m++)
                {
                    if (modules[m])
                    {
                        dc.DrawRectangle(brush, null, new Rect(m * barWidth, 0, barWidth, totalHeight));
                    }
                }
            }

            var rtb = new RenderTargetBitmap(totalWidth, totalHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
        }
    }
}
