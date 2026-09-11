using System;

namespace QuanLyBar.Client.Models
{
    public class ReportFilterParams
    {
        public string ReportName { get; set; } = "";
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? KhoId { get; set; }
        public string? KhoXuatId { get; set; }
        public string? KhoNhapId { get; set; }
        public string? NhanVienId { get; set; }
        public string? CuaHangId { get; set; }
        public string? KhachHangId { get; set; }
        public string? NhaCungCapId { get; set; }
        public string? NhomHangId { get; set; }
        public string? MatHangId { get; set; }
        public string? LyDoId { get; set; }
        public string? SoPhieu { get; set; }
        public bool AutoLoad { get; set; } = true;
    }

    public interface IReportWithFilters
    {
        void ApplyFilterParams(ReportFilterParams filters);
    }
}
