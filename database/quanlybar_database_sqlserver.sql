-- =====================================================
-- DATABASE: QUẢN LÝ BAR, NHÀ HÀNG
-- Phân tích từ: Phần mềm Quản lý Bar, Nhà hàng v6.0
--              Tân An Phát
-- Engine: MySQL 8.0+
-- Charset: utf8mb4 (hỗ trợ tiếng Việt Unicode đầy đủ)
-- =====================================================

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'quanly_bar')
BEGIN
    ALTER DATABASE quanly_bar SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE quanly_bar;
END
GO
CREATE DATABASE quanly_bar;
GO
USE quanly_bar;
GO
GO
USE quanly_bar;
GO

-- =====================================================
-- PHÂN HỆ 1: HỆ THỐNG & TÀI KHOẢN
-- =====================================================

-- Bảng Cửa hàng (hỗ trợ chuỗi nhiều chi nhánh)
CREATE TABLE cua_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_cua_hang    VARCHAR(200) NOT NULL,
    dia_chi         VARCHAR(500),
    dien_thoai      VARCHAR(20),
    email           VARCHAR(100),
    website         VARCHAR(200),
    ma_so_thue      VARCHAR(20),
    logo_url        VARCHAR(500),
    trang_thai      TINYINT DEFAULT 1 /* 1=Hoạt động, 0=Ngừng */,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE()
);
GO

-- Bảng Tài khoản người dùng
CREATE TABLE tai_khoan (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_dang_nhap   VARCHAR(50) NOT NULL UNIQUE,
    mat_khau_hash   VARCHAR(255) NOT NULL,
    ho_ten          VARCHAR(100) NOT NULL,
    email           VARCHAR(100),
    dien_thoai      VARCHAR(20),
    vai_tro         VARCHAR(100) NOT NULL DEFAULT 'phuc_vu',
    cua_hang_id     INT,
    trang_thai      TINYINT DEFAULT 1 /* 1=Hoạt động, 0=Khóa */,
    lan_dang_nhap_cuoi DATETIME,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Phân quyền chi tiết
CREATE TABLE phan_quyen (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    tai_khoan_id    INT NOT NULL,
    ma_chuc_nang    VARCHAR(50) NOT NULL /* VD: HE_THONG, HOAT_DONG, KHO_HANG... */,
    quyen_xem       TINYINT DEFAULT 0,
    quyen_them      TINYINT DEFAULT 0,
    quyen_sua       TINYINT DEFAULT 0,
    quyen_xoa       TINYINT DEFAULT 0,
    quyen_in        TINYINT DEFAULT 0,
    quyen_xuat_excel TINYINT DEFAULT 0,
    FOREIGN KEY (tai_khoan_id) REFERENCES tai_khoan(id) ON DELETE CASCADE,
    CONSTRAINT uk_taikhoan_chucnang UNIQUE (tai_khoan_id, ma_chuc_nang)
);
GO

-- Bảng Thiết bị đăng nhập (theo dõi thiết bị: Desktop, POS, Mobile)
CREATE TABLE thiet_bi (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_thiet_bi    VARCHAR(100) NOT NULL /* VD: LAPTOP-6KP13OSJ */,
    loai_thiet_bi   VARCHAR(100) NOT NULL,
    ma_thiet_bi     VARCHAR(255) /* Device ID / MAC address */,
    cua_hang_id     INT,
    trang_thai      TINYINT DEFAULT 1,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 2: HOẠT ĐỘNG - BÀN & KHU VỰC
-- =====================================================

-- Bảng Khu vực (Tầng 1, Tầng 2, VIP, Sân vườn...)
CREATE TABLE khu_vuc (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_khu_vuc     VARCHAR(100) NOT NULL,
    cua_hang_id     INT,
    thu_tu          INT DEFAULT 0 /* Thứ tự hiển thị */,
    trang_thai      TINYINT DEFAULT 1,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Bàn
CREATE TABLE ban (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_ban         VARCHAR(50) NOT NULL /* VD: Bàn 01, Bàn 02... */,
    khu_vuc_id      INT NOT NULL,
    nhom_hien_thi   VARCHAR(100) /* Nhóm hiển thị trên sơ đồ */,
    loai_phong      VARCHAR(50) /* VD: Phòng VIP, Phòng thường */,
    so_cho          INT DEFAULT 4,
    trang_thai      VARCHAR(100) DEFAULT 'trong',
    ghi_chu         TEXT,
    thu_tu          INT DEFAULT 0,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (khu_vuc_id) REFERENCES khu_vuc(id)
);
GO

-- =====================================================
-- PHÂN HỆ 3: DANH MỤC MẶT HÀNG (THỰC ĐƠN)
-- =====================================================

-- Bảng Nhóm mặt hàng (loại món)
CREATE TABLE nhom_mat_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_nhom        VARCHAR(100) NOT NULL /* VD: BÒ-BÊ-TRÂU-DÊ, CÁC MÓN CÁ, ĐỒ UỐNG CÁC LOẠI... */,
    nhom_cha_id     INT /* Hỗ trợ phân cấp nhóm */,
    icon_url        VARCHAR(500),
    thu_tu          INT DEFAULT 0,
    trang_thai      TINYINT DEFAULT 1,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (nhom_cha_id) REFERENCES nhom_mat_hang(id)
);
GO

-- Bảng Đơn vị tính
CREATE TABLE don_vi_tinh (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_dvt         VARCHAR(50) NOT NULL /* VD: đĩa, lít, bát, kg, cái, bìa, chai, suất, gói, nồi, bao, mâm */,
    trang_thai      TINYINT DEFAULT 1
);
GO

-- Bảng Mặt hàng (Sản phẩm / Món ăn / Đồ uống)
CREATE TABLE mat_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ma_hang         VARCHAR(50) /* Mã hàng tự sinh hoặc tùy chỉnh */,
    ten_hang        VARCHAR(200) NOT NULL /* VD: Quýt tráng miệng, Rượu trắng, Gà HMông... */,
    nhom_mat_hang_id INT,
    dvt_id          INT /* Đơn vị tính mặc định */,
    gia_ban         DECIMAL(15,2) DEFAULT 0 /* Giá bán lẻ */,
    gia_von         DECIMAL(15,2) DEFAULT 0 /* Giá vốn / giá nhập */,
    hinh_anh_url    VARCHAR(500),
    mo_ta           TEXT,
    la_combo        TINYINT DEFAULT 0 /* 1=Là combo nhiều món */,
    co_dinh_luong   TINYINT DEFAULT 0 /* 1=Có định lượng nguyên liệu */,
    trang_thai      VARCHAR(100) DEFAULT 'con_mon',
    thu_tu          INT DEFAULT 0,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (nhom_mat_hang_id) REFERENCES nhom_mat_hang(id),
    FOREIGN KEY (dvt_id) REFERENCES don_vi_tinh(id)
);
GO

-- Bảng Định lượng nguyên liệu cho mặt hàng
CREATE TABLE dinh_luong (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    mat_hang_id     INT NOT NULL /* Món ăn thành phẩm */,
    nguyen_lieu_id  INT NOT NULL /* Nguyên liệu cần dùng (cũng là mặt hàng) */,
    so_luong        DECIMAL(15,4) NOT NULL,
    dvt_id          INT,
    ghi_chu         VARCHAR(200),
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id) ON DELETE CASCADE,
    FOREIGN KEY (nguyen_lieu_id) REFERENCES mat_hang(id),
    FOREIGN KEY (dvt_id) REFERENCES don_vi_tinh(id)
);
GO

-- Bảng Bảng giá theo bàn (giá khác nhau cho VIP, sân vườn...)
CREATE TABLE bang_gia_ban (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ban_id          INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    gia_ban         DECIMAL(15,2) NOT NULL,
    ngay_bat_dau    DATE,
    ngay_ket_thuc   DATE,
    FOREIGN KEY (ban_id) REFERENCES ban(id),
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 4: KHÁCH HÀNG
-- =====================================================

-- Bảng Nhóm khách hàng
CREATE TABLE nhom_khach_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_nhom        VARCHAR(100) NOT NULL /* VD: VIP, Thường, Thân thiết */,
    mo_ta           VARCHAR(500),
    trang_thai      TINYINT DEFAULT 1
);
GO

-- Bảng Khách hàng
CREATE TABLE khach_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ma_khach        VARCHAR(50) UNIQUE,
    ten_khach       VARCHAR(200) NOT NULL,
    nhom_kh_id      INT,
    dia_chi         VARCHAR(500),
    dien_thoai      VARCHAR(20),
    email           VARCHAR(100),
    ma_so_thue      VARCHAR(20),
    ngay_sinh       DATE,
    gioi_tinh       VARCHAR(100),
    diem_tich_luy   INT DEFAULT 0,
    ghi_chu         TEXT,
    trang_thai      TINYINT DEFAULT 1,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (nhom_kh_id) REFERENCES nhom_khach_hang(id)
);
GO

-- Bảng Đặt bàn (Theo dõi đặt phòng)
CREATE TABLE dat_ban (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ban_id          INT,
    khach_hang_id   INT,
    ngay_dat        DATE NOT NULL,
    tu_gio          TIME,
    den_gio         TIME,
    so_nguoi        INT,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'cho_xac_nhan',
    nguoi_tao       VARCHAR(50),
    ngay_tao        DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ban_id) REFERENCES ban(id),
    FOREIGN KEY (khach_hang_id) REFERENCES khach_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 5: NHÂN SỰ
-- =====================================================

-- Bảng Ca làm việc
CREATE TABLE ca_lam_viec (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_ca          VARCHAR(100) NOT NULL /* VD: Ca sáng, Ca chiều, Ca tối */,
    gio_bat_dau     TIME,
    gio_ket_thuc    TIME,
    ghi_chu         TEXT
);
GO

-- Bảng Nhân viên
CREATE TABLE nhan_vien (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ma_nhan_vien    VARCHAR(20),
    ho_ten          VARCHAR(100) NOT NULL,
    chuc_vu         VARCHAR(50) /* VD: Quản lý, Thu ngân, Phục vụ, Bếp... */,
    dien_thoai      VARCHAR(20),
    dia_chi         VARCHAR(500),
    email           VARCHAR(100),
    ngay_sinh       DATE,
    gioi_tinh       VARCHAR(100),
    cmnd_cccd       VARCHAR(20),
    ngay_vao_lam    DATE,
    ngay_nghi_viec  DATE,
    luong_co_ban    DECIMAL(15,2) DEFAULT 0,
    cua_hang_id     INT,
    tai_khoan_id    INT /* Liên kết tài khoản đăng nhập */,
    hinh_anh_url    VARCHAR(500),
    trang_thai      TINYINT DEFAULT 1 /* 1=Đang làm, 0=Nghỉ việc */,
    ghi_chu         TEXT,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id),
    FOREIGN KEY (tai_khoan_id) REFERENCES tai_khoan(id)
);
GO

-- Bảng Chấm công
CREATE TABLE cham_cong (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    nhan_vien_id    INT NOT NULL,
    ngay            DATE NOT NULL,
    ca_lam_viec_id  INT,
    trang_thai      VARCHAR(100) NOT NULL DEFAULT 'di_lam',
    gio_vao         TIME,
    gio_ra          TIME,
    ghi_chu         VARCHAR(500),
    CONSTRAINT uk_nhanvien_ngay UNIQUE (nhan_vien_id, ngay),
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id),
    FOREIGN KEY (ca_lam_viec_id) REFERENCES ca_lam_viec(id)
);
GO

-- Bảng Thưởng phạt
CREATE TABLE thuong_phat (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50),
    ngay            DATETIME DEFAULT GETDATE(),
    nhan_vien_id    INT NOT NULL,
    so_tien_thuong  DECIMAL(15,2) DEFAULT 0,
    so_tien_phat    DECIMAL(15,2) DEFAULT 0,
    ly_do           TEXT,
    ghi_chu         TEXT,
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id)
);
GO

-- Bảng Tạm ứng lương
CREATE TABLE tam_ung_luong (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50),
    nhan_vien_id    INT NOT NULL,
    ngay            DATETIME DEFAULT GETDATE(),
    so_tien         DECIMAL(15,2) NOT NULL,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'cho_duyet',
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id)
);
GO

-- Bảng Bảng lương
CREATE TABLE bang_luong (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_bang_luong  VARCHAR(100) NOT NULL /* VD: Bảng lương tháng 8/2026 */,
    thang           INT NOT NULL,
    nam             INT NOT NULL,
    CONSTRAINT uk_thang_nam UNIQUE (thang, nam)
);
GO

-- Bảng Chi tiết bảng lương
CREATE TABLE bang_luong_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    bang_luong_id   INT NOT NULL,
    nhan_vien_id    INT NOT NULL,
    so_ngay_cong    DECIMAL(5,1) DEFAULT 0,
    so_ngay_nghi_phep DECIMAL(5,1) DEFAULT 0,
    so_ngay_nghi_kophep DECIMAL(5,1) DEFAULT 0,
    luong_co_ban    DECIMAL(15,2) DEFAULT 0,
    luong_thuc_te   DECIMAL(15,2) DEFAULT 0,
    tien_thuong     DECIMAL(15,2) DEFAULT 0,
    tien_phat       DECIMAL(15,2) DEFAULT 0,
    tam_ung         DECIMAL(15,2) DEFAULT 0,
    tong_luong      DECIMAL(15,2) DEFAULT 0 /* Lương thực lĩnh */,
    ghi_chu         TEXT,
    FOREIGN KEY (bang_luong_id) REFERENCES bang_luong(id) ON DELETE CASCADE,
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id)
);
GO

-- =====================================================
-- PHÂN HỆ 6: HÓA ĐƠN & BÁN HÀNG
-- =====================================================

-- Bảng Hóa đơn (đơn hàng)
CREATE TABLE hoa_don (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_hoa_don      VARCHAR(20) NOT NULL UNIQUE /* VD: 082600001, 082600004 */,
    ban_id          INT,
    khach_hang_id   INT,
    nhan_vien_id    INT /* Nhân viên phục vụ / thu ngân */,
    cua_hang_id     INT,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    gio_thanh_toan  TIME,
    so_phieu        VARCHAR(50),
    so_khach        INT DEFAULT 1,

    -- Tính tiền
    tien_hang       DECIMAL(15,2) DEFAULT 0 /* Tổng tiền hàng chưa giảm */,
    tien_giam_mat_hang DECIMAL(15,2) DEFAULT 0 /* Giảm giá trên từng mặt hàng */,
    tien_giam_tong_bill DECIMAL(15,2) DEFAULT 0 /* Giảm giá tổng bill */,
    phan_tram_giam  DECIMAL(5,2) DEFAULT 0,
    phi_dich_vu     DECIMAL(15,2) DEFAULT 0,
    thue_vat        DECIMAL(15,2) DEFAULT 0,
    tong_cong       DECIMAL(15,2) DEFAULT 0 /* Số tiền phải thanh toán cuối cùng */,

    -- Thanh toán
    tien_mat        DECIMAL(15,2) DEFAULT 0,
    chuyen_khoan    DECIMAL(15,2) DEFAULT 0,
    tien_the        DECIMAL(15,2) DEFAULT 0 /* Thẻ ATM / tín dụng */,
    voucher         DECIMAL(15,2) DEFAULT 0,
    the_tra_truoc   DECIMAL(15,2) DEFAULT 0 /* Thẻ trả trước / prepaid */,
    tru_tich_luy    DECIMAL(15,2) DEFAULT 0 /* Trừ điểm tích lũy */,
    dat_truoc       DECIMAL(15,2) DEFAULT 0 /* Tiền đặt trước */,
    con_no          DECIMAL(15,2) DEFAULT 0,

    trang_thai      VARCHAR(100) DEFAULT 'dang_phuc_vu',
    ghi_chu         TEXT,
    thiet_bi_id     INT /* Thiết bị tạo hóa đơn */,
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ban_id) REFERENCES ban(id),
    FOREIGN KEY (khach_hang_id) REFERENCES khach_hang(id),
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id),
    FOREIGN KEY (thiet_bi_id) REFERENCES thiet_bi(id)
);
GO

-- Bảng Chi tiết hóa đơn
CREATE TABLE hoa_don_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    hoa_don_id      INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    ten_hang        VARCHAR(200) /* Snapshot tên hàng tại thời điểm bán */,
    dvt             VARCHAR(50) /* Snapshot đơn vị tính */,
    so_luong        DECIMAL(10,2) NOT NULL DEFAULT 1,
    don_gia         DECIMAL(15,2) NOT NULL DEFAULT 0,
    chiet_khau_phan_tram DECIMAL(5,2) DEFAULT 0 /* CK% trên mặt hàng */,
    tien_chiet_khau DECIMAL(15,2) DEFAULT 0,
    thanh_tien      DECIMAL(15,2) DEFAULT 0,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'dang_cho',
    thoi_gian_gui_bep DATETIME /* Thời điểm In chế biến (F10) */,
    thoi_gian_xong  DATETIME,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (hoa_don_id) REFERENCES hoa_don(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id)
);
GO

-- Bảng Hóa đơn hủy
CREATE TABLE hoa_don_huy (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    hoa_don_id      INT NOT NULL,
    ly_do_huy       TEXT,
    nguoi_huy       VARCHAR(50),
    ngay_huy        DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (hoa_don_id) REFERENCES hoa_don(id)
);
GO

-- =====================================================
-- PHÂN HỆ 7: ĐẶT HÀNG (Khách đặt hàng trước)
-- =====================================================

-- Bảng Phương thức đặt
CREATE TABLE phuong_thuc_dat (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_phuong_thuc VARCHAR(100) NOT NULL /* VD: Công văn, Điện thoại, Email, Tin nhắn, Trực tiếp */
);
GO

-- Bảng Đặt hàng
CREATE TABLE dat_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50),
    ngay_dat        DATETIME DEFAULT GETDATE(),
    khach_hang_id   INT,
    ten_khach       VARCHAR(200),
    dia_chi         VARCHAR(500),
    dien_thoai      VARCHAR(20),
    email           VARCHAR(100),
    phuong_thuc_dat_id INT,
    muc_dich_dat    VARCHAR(200),
    tu_gio          TIME,
    den_gio         TIME,
    tu_ngay         DATE,
    den_ngay        DATE,
    tong_cong       DECIMAL(15,2) DEFAULT 0,
    trang_thai      VARCHAR(100) DEFAULT 'moi',
    ghi_chu         TEXT,
    nguoi_tao       VARCHAR(50),
    ngay_tao        DATETIME DEFAULT GETDATE(),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (khach_hang_id) REFERENCES khach_hang(id),
    FOREIGN KEY (phuong_thuc_dat_id) REFERENCES phuong_thuc_dat(id)
);
GO

-- Bảng Chi tiết đặt hàng
CREATE TABLE dat_hang_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    dat_hang_id     INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    so_luong        DECIMAL(10,2) DEFAULT 1,
    don_gia         DECIMAL(15,2) DEFAULT 0,
    thanh_tien      DECIMAL(15,2) DEFAULT 0,
    ghi_chu         TEXT,
    FOREIGN KEY (dat_hang_id) REFERENCES dat_hang(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 8: KHUYẾN MẠI & GIẢM GIÁ
-- =====================================================

-- Bảng Loại hình khuyến mại
CREATE TABLE loai_khuyen_mai (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_loai        VARCHAR(100) NOT NULL /* VD: Giảm giá % theo SP, Giảm giá theo nhóm hàng, Giảm giá tổng bill, Mua X tặng Y */
);
GO

-- Bảng Đợt khuyến mại
CREATE TABLE dot_khuyen_mai (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_dot         VARCHAR(200) NOT NULL,
    loai_km_id      INT,
    tu_ngay         DATE,
    den_ngay        DATE,
    ngung_ap_dung   TINYINT DEFAULT 0,
    ghi_chu         TEXT,
    ti_le_giam_gia  DECIMAL(5,2) DEFAULT 0,
    ti_le_giam_gia_tien_gio DECIMAL(5,2) DEFAULT 0,
    khuyen_mai_gio_hat TINYINT DEFAULT 0,
    ti_le_giam_gia_tong DECIMAL(5,2) DEFAULT 0,
    tu_gio          TIME,
    den_gio         TIME,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (loai_km_id) REFERENCES loai_khuyen_mai(id)
);
GO

-- Bảng Chi tiết khuyến mại (áp dụng cho mặt hàng nào)
CREATE TABLE khuyen_mai_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    dot_km_id       INT NOT NULL,
    mat_hang_id     INT,
    nhom_mat_hang_id INT,
    ti_le_giam      DECIMAL(5,2) DEFAULT 0,
    so_tien_giam    DECIMAL(15,2) DEFAULT 0,
    so_luong_mua    INT DEFAULT 0 /* Số lượng mua để được khuyến mại */,
    so_luong_tang   INT DEFAULT 0 /* Số lượng tặng */,
    FOREIGN KEY (dot_km_id) REFERENCES dot_khuyen_mai(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id),
    FOREIGN KEY (nhom_mat_hang_id) REFERENCES nhom_mat_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 9: THẺ TRẢ TRƯỚC (PREPAID CARD)
-- =====================================================

-- Bảng Nhóm thẻ trả trước
CREATE TABLE nhom_the_tra_truoc (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_nhom        VARCHAR(100) NOT NULL,
    trang_thai      TINYINT DEFAULT 1
);
GO

-- Bảng Thẻ trả trước
CREATE TABLE the_tra_truoc (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ma_the          VARCHAR(50) NOT NULL UNIQUE /* VD: 123456 */,
    nhom_the_id     INT,
    khach_hang_id   INT,
    so_du           DECIMAL(15,2) DEFAULT 0,
    khoa            TINYINT DEFAULT 0 /* 0=Mở, 1=Khóa */,
    ngay_het_han    DATE,
    ghi_chu         TEXT,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (nhom_the_id) REFERENCES nhom_the_tra_truoc(id),
    FOREIGN KEY (khach_hang_id) REFERENCES khach_hang(id)
);
GO

-- Bảng Lịch sử nạp/sử dụng thẻ
CREATE TABLE the_tra_truoc_lich_su (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    the_id          INT NOT NULL,
    loai_giao_dich  VARCHAR(100) NOT NULL,
    so_tien         DECIMAL(15,2) NOT NULL,
    so_du_truoc     DECIMAL(15,2),
    so_du_sau       DECIMAL(15,2),
    hoa_don_id      INT,
    ghi_chu         VARCHAR(500),
    ngay_giao_dich  DATETIME DEFAULT GETDATE(),
    nguoi_thuc_hien VARCHAR(50),
    FOREIGN KEY (the_id) REFERENCES the_tra_truoc(id),
    FOREIGN KEY (hoa_don_id) REFERENCES hoa_don(id)
);
GO

-- =====================================================
-- PHÂN HỆ 10: NHÀ CUNG CẤP
-- =====================================================

-- Bảng Nhóm nhà cung cấp
CREATE TABLE nhom_nha_cung_cap (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_nhom        VARCHAR(100) NOT NULL,
    trang_thai      TINYINT DEFAULT 1
);
GO

-- Bảng Nhà cung cấp
CREATE TABLE nha_cung_cap (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_ncc         VARCHAR(200) NOT NULL,
    nhom_ncc_id     INT,
    dia_chi         VARCHAR(500),
    dien_thoai      VARCHAR(20),
    email           VARCHAR(100),
    website         VARCHAR(200),
    ghi_chu         TEXT,
    con_no          DECIMAL(15,2) DEFAULT 0,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    nguoi_cap_nhat  VARCHAR(50),
    FOREIGN KEY (nhom_ncc_id) REFERENCES nhom_nha_cung_cap(id)
);
GO

-- =====================================================
-- PHÂN HỆ 11: KHO HÀNG
-- =====================================================

-- Bảng Kho hàng
CREATE TABLE kho_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_kho         VARCHAR(100) NOT NULL /* VD: KHO BÁN HÀNG */,
    cua_hang_id     INT,
    dia_chi         VARCHAR(500),
    ghi_chu         TEXT,
    trang_thai      TINYINT DEFAULT 1,
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Phiếu nhập kho
CREATE TABLE phieu_nhap_kho (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50) NOT NULL,
    ngay_nhap       DATETIME DEFAULT GETDATE(),
    nha_cung_cap_id INT,
    kho_id          INT NOT NULL,
    nhan_vien_id    INT,
    cua_hang_id     INT,
    tong_tien       DECIMAL(15,2) DEFAULT 0,
    giam_gia_phan_tram DECIMAL(5,2) DEFAULT 0,
    tien_giam       DECIMAL(15,2) DEFAULT 0,
    thanh_toan      DECIMAL(15,2) DEFAULT 0,
    con_lai         DECIMAL(15,2) DEFAULT 0,
    tai_khoan_ngan_hang VARCHAR(100),
    ma_voucher      VARCHAR(50),
    dien_giai       TEXT,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'nhap',
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    FOREIGN KEY (nha_cung_cap_id) REFERENCES nha_cung_cap(id),
    FOREIGN KEY (kho_id) REFERENCES kho_hang(id),
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Chi tiết phiếu nhập kho
CREATE TABLE phieu_nhap_kho_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    phieu_nhap_id   INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    so_luong        DECIMAL(15,4) NOT NULL,
    don_gia         DECIMAL(15,2) DEFAULT 0,
    thanh_tien      DECIMAL(15,2) DEFAULT 0,
    giam_gia_phan_tram DECIMAL(5,2) DEFAULT 0,
    dvt_id          INT,
    ghi_chu         VARCHAR(500),
    FOREIGN KEY (phieu_nhap_id) REFERENCES phieu_nhap_kho(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id),
    FOREIGN KEY (dvt_id) REFERENCES don_vi_tinh(id)
);
GO

-- Bảng Phiếu xuất kho
CREATE TABLE phieu_xuat_kho (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50) NOT NULL,
    ngay_xuat       DATETIME DEFAULT GETDATE(),
    kho_id          INT NOT NULL,
    nhan_vien_id    INT,
    cua_hang_id     INT,
    ly_do_xuat      VARCHAR(200) /* Xuất bán, xuất hủy, xuất chuyển kho... */,
    tong_tien       DECIMAL(15,2) DEFAULT 0,
    dien_giai       TEXT,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'nhap',
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    FOREIGN KEY (kho_id) REFERENCES kho_hang(id),
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Chi tiết phiếu xuất kho
CREATE TABLE phieu_xuat_kho_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    phieu_xuat_id   INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    so_luong        DECIMAL(15,4) NOT NULL,
    don_gia         DECIMAL(15,2) DEFAULT 0,
    thanh_tien      DECIMAL(15,2) DEFAULT 0,
    dvt_id          INT,
    ghi_chu         VARCHAR(500),
    FOREIGN KEY (phieu_xuat_id) REFERENCES phieu_xuat_kho(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id),
    FOREIGN KEY (dvt_id) REFERENCES don_vi_tinh(id)
);
GO

-- Bảng Phiếu chuyển kho
CREATE TABLE phieu_chuyen_kho (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50) NOT NULL,
    ngay_chuyen     DATETIME DEFAULT GETDATE(),
    kho_xuat_id     INT NOT NULL,
    kho_nhap_id     INT NOT NULL,
    nhan_vien_xuat  INT,
    nhan_vien_nhap  INT,
    cua_hang_id     INT,
    tong_cong       DECIMAL(15,2) DEFAULT 0,
    dien_giai       TEXT,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'cho_duyet',
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    FOREIGN KEY (kho_xuat_id) REFERENCES kho_hang(id),
    FOREIGN KEY (kho_nhap_id) REFERENCES kho_hang(id),
    FOREIGN KEY (nhan_vien_xuat) REFERENCES nhan_vien(id),
    FOREIGN KEY (nhan_vien_nhap) REFERENCES nhan_vien(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Chi tiết phiếu chuyển kho
CREATE TABLE phieu_chuyen_kho_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    phieu_chuyen_id INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    so_luong        DECIMAL(15,4) NOT NULL,
    dvt_id          INT,
    ghi_chu         VARCHAR(500),
    FOREIGN KEY (phieu_chuyen_id) REFERENCES phieu_chuyen_kho(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id),
    FOREIGN KEY (dvt_id) REFERENCES don_vi_tinh(id)
);
GO

-- Bảng Phiếu kiểm kê
CREATE TABLE phieu_kiem_ke (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50) NOT NULL,
    ngay_kiem_ke    DATETIME DEFAULT GETDATE(),
    kho_id          INT NOT NULL,
    nhan_vien_id    INT,
    cua_hang_id     INT,
    dien_giai       TEXT,
    ghi_chu         TEXT,
    trang_thai      VARCHAR(100) DEFAULT 'dang_kiem',
    thanh_toan      DECIMAL(15,2) DEFAULT 0,
    con_lai         DECIMAL(15,2) DEFAULT 0,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    FOREIGN KEY (kho_id) REFERENCES kho_hang(id),
    FOREIGN KEY (nhan_vien_id) REFERENCES nhan_vien(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Chi tiết kiểm kê
CREATE TABLE phieu_kiem_ke_chi_tiet (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    phieu_kiem_ke_id INT NOT NULL,
    mat_hang_id     INT NOT NULL,
    ton_he_thong    DECIMAL(15,4) DEFAULT 0 /* Tồn kho trên hệ thống */,
    ton_thuc_te     DECIMAL(15,4) DEFAULT 0 /* Tồn kho thực tế kiểm đếm */,
    chenh_lech      DECIMAL(15,4) DEFAULT 0 /* Chênh lệch = thực tế - hệ thống */,
    dvt_id          INT,
    ghi_chu         VARCHAR(500),
    FOREIGN KEY (phieu_kiem_ke_id) REFERENCES phieu_kiem_ke(id) ON DELETE CASCADE,
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id),
    FOREIGN KEY (dvt_id) REFERENCES don_vi_tinh(id)
);
GO

-- Bảng Tồn kho (snapshot tồn kho hiện tại)
CREATE TABLE ton_kho (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    mat_hang_id     INT NOT NULL,
    kho_id          INT NOT NULL,
    so_luong_ton    DECIMAL(15,4) DEFAULT 0 /* Tổng tồn kho */,
    gia_von_tb      DECIMAL(15,2) DEFAULT 0 /* Giá vốn trung bình */,
    quy_doi         DECIMAL(15,4) DEFAULT 1 /* Hệ số quy đổi DVT */,
    ton_2_dvt       DECIMAL(15,4) DEFAULT 0 /* Tồn quy đổi theo DVT 2 */,
    ngay_cap_nhat   DATETIME DEFAULT GETDATE(),
    CONSTRAINT uk_mathang_kho UNIQUE (mat_hang_id, kho_id),
    FOREIGN KEY (mat_hang_id) REFERENCES mat_hang(id),
    FOREIGN KEY (kho_id) REFERENCES kho_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 12: CÔNG NỢ
-- =====================================================

-- Bảng Công nợ khách hàng
CREATE TABLE cong_no_khach_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    khach_hang_id   INT NOT NULL,
    hoa_don_id      INT,
    so_phieu        VARCHAR(50),
    ngay            DATETIME DEFAULT GETDATE(),
    tong_cong       DECIMAL(15,2) DEFAULT 0,
    tien_thanh_toan DECIMAL(15,2) DEFAULT 0,
    dien_giai       TEXT,
    luy_ke          DECIMAL(15,2) DEFAULT 0 /* Lũy kế công nợ */,
    trang_thai      VARCHAR(100) DEFAULT 'con_no',
    FOREIGN KEY (khach_hang_id) REFERENCES khach_hang(id),
    FOREIGN KEY (hoa_don_id) REFERENCES hoa_don(id)
);
GO

-- Bảng Công nợ nhà cung cấp
CREATE TABLE cong_no_nha_cung_cap (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    nha_cung_cap_id INT NOT NULL,
    phieu_nhap_id   INT,
    so_phieu        VARCHAR(50),
    ngay            DATETIME DEFAULT GETDATE(),
    tong_cong       DECIMAL(15,2) DEFAULT 0,
    tien_thanh_toan DECIMAL(15,2) DEFAULT 0,
    dien_giai       TEXT,
    luy_ke          DECIMAL(15,2) DEFAULT 0,
    trang_thai      VARCHAR(100) DEFAULT 'con_no',
    FOREIGN KEY (nha_cung_cap_id) REFERENCES nha_cung_cap(id),
    FOREIGN KEY (phieu_nhap_id) REFERENCES phieu_nhap_kho(id)
);
GO

-- =====================================================
-- PHÂN HỆ 13: QUỸ - THU CHI
-- =====================================================

-- Bảng Lý do thu chi
CREATE TABLE ly_do_thu_chi (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_ly_do       VARCHAR(200) NOT NULL /* VD: Chi lương NV, Đặt trước, Đồ dùng, Tiền điện, Tiền nhà... */,
    loai            VARCHAR(100) NOT NULL,
    trang_thai      TINYINT DEFAULT 1
);
GO

-- Bảng Tài khoản ngân hàng
CREATE TABLE tai_khoan_ngan_hang (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    ten_tai_khoan   VARCHAR(100) NOT NULL /* VD: tk1, tk2, tk3 */,
    so_tai_khoan    VARCHAR(50),
    ngan_hang       VARCHAR(100),
    chi_nhanh       VARCHAR(100),
    so_du           DECIMAL(15,2) DEFAULT 0,
    ghi_chu         TEXT,
    trang_thai      TINYINT DEFAULT 1
);
GO

-- Bảng Phiếu thu
CREATE TABLE phieu_thu (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50) NOT NULL,
    ngay            DATETIME DEFAULT GETDATE(),
    ly_do_id        INT,
    doi_tuong       VARCHAR(200) /* Tên khách hàng/nhà cung cấp */,
    dien_giai       TEXT,
    chung_tu_goc    VARCHAR(100) /* Số chứng từ gốc */,
    so_tien         DECIMAL(15,2) NOT NULL DEFAULT 0,
    ghi_chu         TEXT,
    chuyen_khoan    TINYINT DEFAULT 0,
    dat_hang_id     INT /* Liên kết đặt hàng */,
    la_phieu_thu_cong_no TINYINT DEFAULT 0,
    khong_thay_doi_cong_no TINYINT DEFAULT 0,
    tai_khoan_ngan_hang_id INT,
    the_tra_truoc_id INT,
    don_hang_id     INT,
    cua_hang_id     INT,
    trang_thai      VARCHAR(100) DEFAULT 'nhap',
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    FOREIGN KEY (ly_do_id) REFERENCES ly_do_thu_chi(id),
    FOREIGN KEY (tai_khoan_ngan_hang_id) REFERENCES tai_khoan_ngan_hang(id),
    FOREIGN KEY (the_tra_truoc_id) REFERENCES the_tra_truoc(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Phiếu chi
CREATE TABLE phieu_chi (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    so_phieu        VARCHAR(50) NOT NULL,
    ngay            DATETIME DEFAULT GETDATE(),
    ly_do_id        INT,
    doi_tuong       VARCHAR(200),
    dia_chi         VARCHAR(500),
    dien_giai       TEXT,
    chung_tu_goc    VARCHAR(100),
    so_tien         DECIMAL(15,2) NOT NULL DEFAULT 0,
    ghi_chu         TEXT,
    chuyen_khoan    TINYINT DEFAULT 0,
    dat_hang_id     INT,
    la_phieu_thu_cong_no TINYINT DEFAULT 0,
    khong_thay_doi_cong_no TINYINT DEFAULT 0,
    tai_khoan_ngan_hang_id INT,
    the_tra_truoc_id INT,
    don_hang_id     INT,
    cua_hang_id     INT,
    trang_thai      VARCHAR(100) DEFAULT 'nhap',
    ngay_tao        DATETIME DEFAULT GETDATE(),
    nguoi_tao       VARCHAR(50),
    FOREIGN KEY (ly_do_id) REFERENCES ly_do_thu_chi(id),
    FOREIGN KEY (tai_khoan_ngan_hang_id) REFERENCES tai_khoan_ngan_hang(id),
    FOREIGN KEY (the_tra_truoc_id) REFERENCES the_tra_truoc(id),
    FOREIGN KEY (cua_hang_id) REFERENCES cua_hang(id)
);
GO

-- Bảng Phiếu thu công nợ
CREATE TABLE phieu_thu_cong_no (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    phieu_thu_id    INT,
    cong_no_id      INT,
    so_tien         DECIMAL(15,2) DEFAULT 0,
    FOREIGN KEY (phieu_thu_id) REFERENCES phieu_thu(id),
    FOREIGN KEY (cong_no_id) REFERENCES cong_no_khach_hang(id)
);
GO

-- =====================================================
-- PHÂN HỆ 14: NHẬT KÝ HOẠT ĐỘNG (LOG)
-- =====================================================

-- Bảng Lưu vết hoạt động (Audit Log)
CREATE TABLE nhat_ky_hoat_dong (
    id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    ngay            DATE NOT NULL,
    gio             TIME NOT NULL,
    so_don_hang     VARCHAR(20),
    noi_dung        TEXT NOT NULL /* VD: Mở hóa đơn trên bàn Bàn 01, Đặt khách hàng Anh Chung... */,
    tai_khoan       VARCHAR(50),
    thiet_bi        VARCHAR(100) /* VD: LAPTOP-6KP13OSJ */,
    ban             VARCHAR(50),
    chuc_nang       VARCHAR(100) /* VD: Sử dụng dịch vụ, Đặt bàn... */,
    ngay_tao        DATETIME DEFAULT GETDATE(),
    INDEX idx_ngay (ngay),
    INDEX idx_tai_khoan (tai_khoan),
    INDEX idx_so_don_hang (so_don_hang)
);
GO

-- =====================================================
-- PHÂN HỆ 15: BÁO CÁO & THỐNG KÊ (Views hỗ trợ)
-- =====================================================

-- View: Thống kê doanh thu theo ngày
GO
CREATE OR ALTER VIEW v_thong_ke_doanh_thu AS
SELECT
    CAST(hd.ngay_tao AS DATE) AS ngay,
    hd.cua_hang_id,
    COUNT(hd.id) AS so_hoa_don,
    SUM(hd.tien_hang) AS tong_tien_hang,
    SUM(hd.tien_giam_mat_hang + hd.tien_giam_tong_bill) AS tong_giam_gia,
    SUM(hd.phi_dich_vu) AS tong_phi_dich_vu,
    SUM(hd.thue_vat) AS tong_thue,
    SUM(hd.tong_cong) AS tong_doanh_thu,
    SUM(hd.tien_mat) AS tong_tien_mat,
    SUM(hd.chuyen_khoan) AS tong_chuyen_khoan,
    SUM(hd.tien_the) AS tong_tien_the,
    SUM(hd.voucher) AS tong_voucher,
    SUM(hd.the_tra_truoc) AS tong_the_tra_truoc,
    SUM(hd.con_no) AS tong_con_no
FROM hoa_don hd
WHERE hd.trang_thai = 'da_thanh_toan'
GROUP BY CAST(hd.ngay_tao AS DATE), hd.cua_hang_id;
GO


-- View: Thống kê mặt hàng bán
GO
CREATE OR ALTER VIEW v_thong_ke_mat_hang_ban AS
SELECT
    CAST(hd.ngay_tao AS DATE) AS ngay,
    hdct.mat_hang_id,
    mh.ma_hang,
    mh.ten_hang,
    dvt.ten_dvt AS don_vi_tinh,
    SUM(hdct.so_luong) AS tong_so_luong,
    hdct.don_gia,
    SUM(hdct.tien_chiet_khau) AS tong_giam_gia,
    SUM(hdct.thanh_tien) AS tong_thanh_tien_ban,
    mh.gia_von AS gia_von,
    SUM(hdct.so_luong * mh.gia_von) AS tong_thanh_tien_nhap,
    SUM(hdct.thanh_tien) - SUM(hdct.so_luong * mh.gia_von) AS lai,
    CASE
        WHEN SUM(hdct.so_luong * mh.gia_von) > 0
        THEN ROUND((SUM(hdct.thanh_tien) - SUM(hdct.so_luong * mh.gia_von))
             / SUM(hdct.so_luong * mh.gia_von) * 100, 1)
        ELSE 100
    END AS ti_le_lai
FROM hoa_don_chi_tiet hdct
JOIN hoa_don hd ON hd.id = hdct.hoa_don_id
JOIN mat_hang mh ON mh.id = hdct.mat_hang_id
LEFT JOIN don_vi_tinh dvt ON dvt.id = mh.dvt_id
WHERE hd.trang_thai = 'da_thanh_toan'
GROUP BY CAST(hd.ngay_tao AS DATE), hdct.mat_hang_id, mh.ma_hang, mh.ten_hang,
         dvt.ten_dvt, hdct.don_gia, mh.gia_von;
GO


-- View: Tồn kho nhiều kho
GO
CREATE OR ALTER VIEW v_ton_nhieu_kho AS
SELECT
    mh.id AS mat_hang_id,
    mh.ma_hang,
    mh.ten_hang,
    dvt.ten_dvt,
    kh.ten_kho AS kho_ban_hang,
    tk.so_luong_ton AS tong_ton,
    tk.quy_doi,
    tk.ton_2_dvt,
    nmh.ten_nhom
FROM ton_kho tk
JOIN mat_hang mh ON mh.id = tk.mat_hang_id
JOIN kho_hang kh ON kh.id = tk.kho_id
LEFT JOIN don_vi_tinh dvt ON dvt.id = mh.dvt_id
LEFT JOIN nhom_mat_hang nmh ON nmh.id = mh.nhom_mat_hang_id;
GO

-- View: Tồn quỹ
GO
CREATE OR ALTER VIEW v_ton_quy AS
SELECT
    'tien_mat' AS loai_tai_khoan,
    'Tiền mặt' AS ten_tai_khoan,
    COALESCE(SUM(CASE WHEN pt.trang_thai = 'da_duyet' THEN pt.so_tien ELSE 0 END), 0) AS tong_thu,
    COALESCE((SELECT SUM(pc.so_tien) FROM phieu_chi pc WHERE pc.trang_thai = 'da_duyet'), 0) AS tong_chi
FROM phieu_thu pt
UNION ALL
SELECT
    'ngan_hang' AS loai_tai_khoan,
    tkngh.ten_tai_khoan,
    tkngh.so_du AS tong_thu,
    0 AS tong_chi
FROM tai_khoan_ngan_hang tkngh
WHERE tkngh.trang_thai = 1;
GO

-- =====================================================
-- DỮ LIỆU MẪU BAN ĐẦU
-- =====================================================

-- Cửa hàng mặc định
INSERT INTO cua_hang (ten_cua_hang, dia_chi, dien_thoai, email) VALUES
('NÀNG HƯƠNG QUÁN', 'Số 28 Giang Văn Minh - Đội Cấn - Ba Đình - Hà Nội', '0909090880', '');

-- Tài khoản admin mặc định (password: admin123 - cần hash trong thực tế)
INSERT INTO tai_khoan (ten_dang_nhap, mat_khau_hash, ho_ten, vai_tro, cua_hang_id) VALUES
('admin', '$2b$12$placeholder_hash_admin123', 'Administrator', 'admin', 1);

-- Đơn vị tính
INSERT INTO don_vi_tinh (ten_dvt) VALUES
('đĩa'), ('lít'), ('bát'), ('kg'), ('cái'), ('bìa'), ('chai'), ('suất'), ('gói'), ('nồi'), ('bao'), ('mâm');

-- Khu vực mặc định
INSERT INTO khu_vuc (ten_khu_vuc, cua_hang_id) VALUES
('Tầng 1', 1), ('Tầng 2', 1);

-- Bàn mặc định
INSERT INTO ban (ten_ban, khu_vuc_id) VALUES
('Bàn 01', 1), ('Bàn 02', 1), ('Bàn 03', 1), ('Bàn 04', 1), ('Bàn 05', 1),
('Bàn 06', 1), ('Bàn 07', 1), ('Bàn 08', 1), ('Bàn 09', 1),
('Bàn 20', 2), ('Bàn 21', 2), ('Bàn 22', 2), ('Bàn 23', 2), ('Bàn 24', 2),
('Bàn 25', 2), ('Bàn 26', 2), ('Bàn 27', 2), ('Bàn 28', 2), ('Bàn 29', 2);

-- Nhóm mặt hàng (từ video)
INSERT INTO nhom_mat_hang (ten_nhom) VALUES
('BÒ - BÊ - TRÂU - DÊ'),
('CÁC MÓN CÁ'),
('CÁC MÓN LẨU VÀ MÓN ĂN KÈM'),
('CÁC MÓN RAU'),
('CHIM CÁC LOẠI'),
('CƠM VÀ MÓN KÈM'),
('ĐỒ UỐNG CÁC LOẠI'),
('GÀ - VỊT'),
('HẢI SẢN'),
('LƯƠN - CUA - ỐC - ẾCH'),
('MÓN KHAI VỊ'),
('ĐỒ DÙNG NHÀ HÀNG'),
('ĐỒ NHÂN VIÊN'),
('GIA VỊ'),
('NGUYÊN LIỆU ĐÃ QUA CHẾ BIẾN'),
('NGUYÊN LIỆU RAU'),
('NGUYÊN LIỆU TƯƠI SỐNG'),
('THỊT LỢN');

-- Mặt hàng mẫu (từ video)
INSERT INTO mat_hang (ten_hang, nhom_mat_hang_id, dvt_id, gia_ban) VALUES
('Quýt tráng miệng', 11, 1, 30000),
('Rượu trắng', 7, 2, 60000),
('Rượu Vọc', 7, 2, 90000),
('Lòng nấu miến', 1, 3, 25000),
('Gà HMông', 8, 4, 200000),
('Lòng xào rau cần', 1, 1, 25000),
('Lòng xào muộp giá', 1, 1, 25000),
('Mì tôm úp', 6, 3, 15000),
('Xúc xích', 11, 5, 10000),
('Đậu rán cà cái', 4, 6, 5000),
('Cơm rang dưa bò', 6, 1, 50000),
('Cơm rang trứng hành', 6, 1, 50000),
('Ruốc', 11, 3, 10000),
('Cá quả nấu chua', 2, 10, 250000),
('Cá nganh đủ món', 2, 4, 280000),
('Mỳ chũ', 6, 1, 15000),
('Mì tôm', 6, 9, 5000),
('Chim trĩ đủ món', 5, 4, 320000);

-- Nhóm khách hàng
INSERT INTO nhom_khach_hang (ten_nhom) VALUES
('VIP'), ('Thường');

-- Kho hàng
INSERT INTO kho_hang (ten_kho, cua_hang_id) VALUES
('KHO BÁN HÀNG', 1);

-- Nhân viên mẫu (từ video)
INSERT INTO nhan_vien (ho_ten, chuc_vu, cua_hang_id) VALUES
('Bá Đạo', 'Phục vụ', 1),
('Bá Văn Vành', 'Phục vụ', 1),
('Bùi Thu Minh', 'Thu ngân', 1),
('Đào Văn Tuyến', 'Bếp', 1),
('Hằng', 'Phục vụ', 1),
('Lê Thị Hựu', 'Phục vụ', 1),
('Nguyễn Ánh Tuyết', 'Phục vụ', 1),
('Nguyễn Bửu', 'Quản lý', 1),
('Nguyễn Minh Nguyệt', 'Phục vụ', 1),
('Tạ Bích Đào', 'Phục vụ', 1),
('Trần Thị Sương', 'Phục vụ', 1),
('Trần Trung Thành', 'Bếp', 1),
('Vũ Văn Huy', 'Phục vụ', 1);

-- Phương thức đặt
INSERT INTO phuong_thuc_dat (ten_phuong_thuc) VALUES
('Công văn'), ('Điện thoại'), ('Email'), ('Tin nhắn'), ('Trực tiếp');

-- Loại khuyến mại
INSERT INTO loai_khuyen_mai (ten_loai) VALUES
('Giảm giá % theo sản phẩm'),
('Giảm giá theo nhóm hàng'),
('Giảm giá tiền theo sản phẩm'),
('Giảm giá tổng bill'),
('Mua x sản phẩm tặng y sản phẩm');

-- Lý do thu chi (từ video)
INSERT INTO ly_do_thu_chi (ten_ly_do, loai) VALUES
('Chi khác', 'chi'),
('Chi lương nhân viên', 'chi'),
('Đặt trước', 'chi'),
('Đồ dùng, dụng cụ', 'chi'),
('Lương nhân viên', 'chi'),
('Lương vệ sỹ', 'chi'),
('Nạp thẻ trả trước', 'chi'),
('Ngoại giao', 'chi'),
('Tạm ứng', 'chi'),
('Thanh toán công nợ', 'chi'),
('Thu công nợ', 'thu'),
('Thu đặt trước tiền mua hàng', 'thu'),
('Thu tạm ứng', 'thu'),
('Thu tiền hoàn ứng', 'thu'),
('Thưởng nhân viên', 'chi'),
('Tiền điện', 'chi'),
('Tiền điện thoại', 'chi'),
('Tiền nhà', 'chi'),
('Tiền nước', 'chi'),
('Trả trước tiền mua hàng', 'chi'),
('Vận chuyển', 'chi'),
('Văn phòng phẩm, in ấn', 'chi'),
('Xây dựng, sửa chữa, thiết kế', 'chi');

-- Tài khoản ngân hàng
INSERT INTO tai_khoan_ngan_hang (ten_tai_khoan) VALUES
('tk1'), ('tk2'), ('tk3');

-- Ca làm việc
INSERT INTO ca_lam_viec (ten_ca, gio_bat_dau, gio_ket_thuc) VALUES
('Ca sáng', '06:00', '14:00'),
('Ca chiều', '14:00', '22:00'),
('Ca tối', '18:00', '02:00');

-- =====================================================
-- INDEXES BỔ SUNG CHO HIỆU NĂNG
-- =====================================================

CREATE INDEX idx_hoadon_ngaytao ON hoa_don(ngay_tao);
CREATE INDEX idx_hoadon_trangthai ON hoa_don(trang_thai);
CREATE INDEX idx_hoadon_ban ON hoa_don(ban_id);
CREATE INDEX idx_hoadon_cuahang ON hoa_don(cua_hang_id);
CREATE INDEX idx_hdct_hoadon ON hoa_don_chi_tiet(hoa_don_id);
CREATE INDEX idx_hdct_mathang ON hoa_don_chi_tiet(mat_hang_id);
CREATE INDEX idx_mathang_nhom ON mat_hang(nhom_mat_hang_id);
CREATE INDEX idx_ban_khuvuc ON ban(khu_vuc_id);
CREATE INDEX idx_tonkho_mathang ON ton_kho(mat_hang_id);
CREATE INDEX idx_phieunhap_ncc ON phieu_nhap_kho(nha_cung_cap_id);
CREATE INDEX idx_congno_kh ON cong_no_khach_hang(khach_hang_id);
CREATE INDEX idx_congno_ncc ON cong_no_nha_cung_cap(nha_cung_cap_id);
CREATE INDEX idx_chamcong_nv ON cham_cong(nhan_vien_id);
CREATE INDEX idx_chamcong_ngay ON cham_cong(ngay);
