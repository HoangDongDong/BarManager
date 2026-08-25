using System;

namespace QuanLyBar.Client.Models
{
    public class MatHangViewModel
    {
        public int Stt { get; set; } // Số thứ tự
        // Các trường từ DMATHANG
        public string Id { get; set; }
        public string Code { get; set; } // Mã hàng
        public string Name { get; set; } // Tên mặt hàng
        
        public decimal? Gianhap { get; set; } // Giá nhập
        public decimal? Giaban { get; set; } // Giá bán
        public decimal? Giabanchan { get; set; } // Giá bán chẵn
        
        public string Quydoi { get; set; } // Quy đổi
        public string Tamkhoa { get; set; } // Tạm khóa
        public decimal? Giatheothoigia { get; set; } // Giá theo thời giá
        
        public string DnhommathangId { get; set; }
        public string DloaimathangId { get; set; }
        public string DdonvitinhId { get; set; }
        public string DdonvitinhchanId { get; set; }

        // Các trường lấy từ JOIN bảng khác
        public string NhomMatHangName { get; set; } // Tên nhóm mặt hàng
        public string LoaiMatHangName { get; set; } // Tên loại mặt hàng
        public string DonViTinhName { get; set; } // Tên đơn vị tính
        public string DonViTinhChanName { get; set; } // Tên đơn vị tính chẵn
    }
}
