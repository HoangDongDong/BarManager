using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace QuanLyBar.Client.Models
{
    public class TheoDoiDatPhongCell
    {
        public DateTime Date { get; set; }
        public string Text { get; set; } = "-";
        public bool IsBooked { get; set; }
        public string BookingId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string SoPhieu { get; set; }
        public string Note { get; set; }
        public Brush CellBackground { get; set; }
        public Brush CellForeground { get; set; }
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
    }

    public class TheoDoiDatPhongRowViewModel
    {
        public string KhuVucName { get; set; }
        public string PhongName { get; set; }
        public string BanId { get; set; }
        public bool IsSummary { get; set; }
        public List<TheoDoiDatPhongCell> Cells { get; set; } = new List<TheoDoiDatPhongCell>();
        public string Tong { get; set; } = "-";
        public Brush RowForeground { get; set; } = Brushes.Black;
        public FontWeight RowFontWeight { get; set; } = FontWeights.Normal;
        public Brush RowBackground { get; set; } = Brushes.Transparent;
    }

    public class MonthGroupHeader
    {
        public string Title { get; set; }
        public int DayCount { get; set; }
        public DateTime StartMonth { get; set; }
    }
}
