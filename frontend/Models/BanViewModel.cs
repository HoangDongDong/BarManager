namespace QuanLyBar.Client.Models
{
    public class BanViewModel
    {
        public bool IsSelected { get; set; }
        public int Stt { get; set; } // Số thứ tự
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string KhuVucName { get; set; }
        public string NhomHienThiName { get; set; }
        public string LoaiPhongName { get; set; }
        public string BanggiaName { get; set; }
        public decimal? Dongia { get; set; }
        public decimal? Tienmoban { get; set; }
        public DateTime? Timecreated { get; set; }
        public string UsercreatedId { get; set; }
        public string UsercreatedName { get; set; }
        public DateTime? Timemodified { get; set; }
        public string UsermodifiedId { get; set; }
        public string UsermodifiedName { get; set; }
    }
}
