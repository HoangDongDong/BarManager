# Sơ đồ ERD - Database Quản lý Bar, Nhà hàng

## Sơ đồ tổng quan các phân hệ

```mermaid
graph TB
    subgraph HT["🏢 HỆ THỐNG"]
        CUA_HANG["Cửa hàng"]
        TAI_KHOAN["Tài khoản"]
        PHAN_QUYEN["Phân quyền"]
        THIET_BI["Thiết bị"]
    end

    subgraph BAN_KV["🪑 BÀN & KHU VỰC"]
        KHU_VUC["Khu vực"]
        BAN["Bàn"]
        DAT_BAN["Đặt bàn"]
    end

    subgraph MENU["🍽️ THỰC ĐƠN"]
        NHOM_MH["Nhóm mặt hàng"]
        MAT_HANG["Mặt hàng"]
        DVT["Đơn vị tính"]
        DINH_LUONG["Định lượng"]
        BANG_GIA["Bảng giá bàn"]
    end

    subgraph KH["👤 KHÁCH HÀNG"]
        NHOM_KH["Nhóm KH"]
        KHACH_HANG["Khách hàng"]
    end

    subgraph NS["👥 NHÂN SỰ"]
        NHAN_VIEN["Nhân viên"]
        CA_LAM["Ca làm việc"]
        CHAM_CONG["Chấm công"]
        THUONG_PHAT["Thưởng phạt"]
        TAM_UNG["Tạm ứng lương"]
        BANG_LUONG["Bảng lương"]
    end

    subgraph BH["💰 BÁN HÀNG"]
        HOA_DON["Hóa đơn"]
        HOA_DON_CT["Hóa đơn CT"]
        HOA_DON_HUY["Hóa đơn hủy"]
    end

    subgraph DH["📋 ĐẶT HÀNG"]
        PT_DAT["Phương thức đặt"]
        DAT_HANG["Đặt hàng"]
        DAT_HANG_CT["Đặt hàng CT"]
    end

    subgraph KM["🎁 KHUYẾN MẠI"]
        LOAI_KM["Loại KM"]
        DOT_KM["Đợt KM"]
        KM_CT["KM chi tiết"]
    end

    subgraph THE["💳 THẺ TRẢ TRƯỚC"]
        NHOM_THE["Nhóm thẻ"]
        THE_TT["Thẻ trả trước"]
        THE_LS["Lịch sử thẻ"]
    end

    subgraph NCC_G["🏭 NHÀ CUNG CẤP"]
        NHOM_NCC["Nhóm NCC"]
        NCC["Nhà cung cấp"]
    end

    subgraph KHO["📦 KHO HÀNG"]
        KHO_H["Kho hàng"]
        P_NHAP["Phiếu nhập kho"]
        P_XUAT["Phiếu xuất kho"]
        P_CHUYEN["Phiếu chuyển kho"]
        P_KIEM["Phiếu kiểm kê"]
        TON_KHO["Tồn kho"]
    end

    subgraph CN["📊 CÔNG NỢ"]
        CN_KH["Công nợ KH"]
        CN_NCC["Công nợ NCC"]
    end

    subgraph QUY["💵 QUỸ - THU CHI"]
        LY_DO["Lý do thu chi"]
        TK_NH["TK Ngân hàng"]
        P_THU["Phiếu thu"]
        P_CHI["Phiếu chi"]
    end

    subgraph LOG["📝 NHẬT KÝ"]
        NK_HD["Nhật ký hoạt động"]
    end

    HT --> BAN_KV
    HT --> NS
    BAN_KV --> BH
    MENU --> BH
    KH --> BH
    NS --> BH
    NCC_G --> KHO
    BH --> CN
    BH --> THE
    BH --> QUY
    KHO --> CN
    BH --> LOG
```

---

## ERD chi tiết - Phân hệ Hệ thống & Bàn

```mermaid
erDiagram
    cua_hang {
        INT id PK
        VARCHAR ten_cua_hang
        VARCHAR dia_chi
        VARCHAR dien_thoai
        VARCHAR email
        VARCHAR ma_so_thue
        VARCHAR logo_url
        TINYINT trang_thai
        DATETIME ngay_tao
    }

    tai_khoan {
        INT id PK
        VARCHAR ten_dang_nhap UK
        VARCHAR mat_khau_hash
        VARCHAR ho_ten
        VARCHAR email
        ENUM vai_tro "admin|quan_ly|thu_ngan|phuc_vu|bep|kho"
        INT cua_hang_id FK
        TINYINT trang_thai
        DATETIME lan_dang_nhap_cuoi
    }

    phan_quyen {
        INT id PK
        INT tai_khoan_id FK
        VARCHAR ma_chuc_nang
        TINYINT quyen_xem
        TINYINT quyen_them
        TINYINT quyen_sua
        TINYINT quyen_xoa
        TINYINT quyen_in
        TINYINT quyen_xuat_excel
    }

    thiet_bi {
        INT id PK
        VARCHAR ten_thiet_bi
        ENUM loai_thiet_bi "desktop|mobile|pos|tablet"
        VARCHAR ma_thiet_bi
        INT cua_hang_id FK
        TINYINT trang_thai
    }

    khu_vuc {
        INT id PK
        VARCHAR ten_khu_vuc
        INT cua_hang_id FK
        INT thu_tu
        TINYINT trang_thai
    }

    ban {
        INT id PK
        VARCHAR ten_ban
        INT khu_vuc_id FK
        VARCHAR loai_phong
        INT so_cho
        ENUM trang_thai "trong|dang_su_dung|dat_truoc|don_dep"
    }

    dat_ban {
        INT id PK
        INT ban_id FK
        INT khach_hang_id FK
        DATE ngay_dat
        TIME tu_gio
        TIME den_gio
        INT so_nguoi
        ENUM trang_thai "cho_xac_nhan|da_xac_nhan|da_den|huy"
    }

    cua_hang ||--o{ tai_khoan : "có nhiều"
    cua_hang ||--o{ thiet_bi : "quản lý"
    cua_hang ||--o{ khu_vuc : "có nhiều"
    tai_khoan ||--o{ phan_quyen : "được phân quyền"
    khu_vuc ||--o{ ban : "chứa nhiều"
    ban ||--o{ dat_ban : "được đặt"
```

---

## ERD chi tiết - Phân hệ Thực đơn & Mặt hàng

```mermaid
erDiagram
    nhom_mat_hang {
        INT id PK
        VARCHAR ten_nhom "BÒ-BÊ-TRÂU-DÊ, CÁC MÓN CÁ..."
        INT nhom_cha_id FK "Phân cấp nhóm"
        VARCHAR icon_url
        INT thu_tu
        TINYINT trang_thai
    }

    don_vi_tinh {
        INT id PK
        VARCHAR ten_dvt "dia, lit, bat, kg, cai, chai..."
        TINYINT trang_thai
    }

    mat_hang {
        INT id PK
        VARCHAR ma_hang UK
        VARCHAR ten_hang
        INT nhom_mat_hang_id FK
        INT dvt_id FK
        DECIMAL gia_ban
        DECIMAL gia_von
        VARCHAR hinh_anh_url
        TEXT mo_ta
        TINYINT la_combo
        TINYINT co_dinh_luong
        ENUM trang_thai "con_mon|het_hang|ngung_ban"
    }

    dinh_luong {
        INT id PK
        INT mat_hang_id FK "Mon thanh pham"
        INT nguyen_lieu_id FK "Nguyen lieu"
        DECIMAL so_luong
        INT dvt_id FK
        VARCHAR ghi_chu
    }

    bang_gia_ban {
        INT id PK
        INT ban_id FK
        INT mat_hang_id FK
        DECIMAL gia_ban
        DATE ngay_bat_dau
        DATE ngay_ket_thuc
    }

    nhom_mat_hang ||--o{ mat_hang : "thuộc nhóm"
    nhom_mat_hang ||--o{ nhom_mat_hang : "nhóm cha-con"
    don_vi_tinh ||--o{ mat_hang : "đơn vị"
    mat_hang ||--o{ dinh_luong : "thành phẩm có"
    mat_hang ||--o{ bang_gia_ban : "giá theo bàn"
```

---

## ERD chi tiết - Phân hệ Khách hàng & Nhân sự

```mermaid
erDiagram
    nhom_khach_hang {
        INT id PK
        VARCHAR ten_nhom "VIP, Thuong, Than thiet"
        VARCHAR mo_ta
        TINYINT trang_thai
    }

    khach_hang {
        INT id PK
        VARCHAR ma_khach UK
        VARCHAR ten_khach
        INT nhom_kh_id FK
        VARCHAR dia_chi
        VARCHAR dien_thoai
        VARCHAR email
        VARCHAR ma_so_thue
        DATE ngay_sinh
        ENUM gioi_tinh "nam|nu|khac"
        INT diem_tich_luy
    }

    nhan_vien {
        INT id PK
        VARCHAR ma_nhan_vien UK
        VARCHAR ho_ten
        VARCHAR chuc_vu
        VARCHAR dien_thoai
        VARCHAR dia_chi
        DATE ngay_sinh
        VARCHAR cmnd_cccd
        DATE ngay_vao_lam
        DECIMAL luong_co_ban
        INT cua_hang_id FK
        INT tai_khoan_id FK
        TINYINT trang_thai
    }

    ca_lam_viec {
        INT id PK
        VARCHAR ten_ca "Ca sang, Ca chieu, Ca toi"
        TIME gio_bat_dau
        TIME gio_ket_thuc
    }

    cham_cong {
        INT id PK
        INT nhan_vien_id FK
        DATE ngay
        INT ca_lam_viec_id FK
        ENUM trang_thai "khong_co_lich|di_lam|nghi_co_phep|nghi_khong_phep"
        TIME gio_vao
        TIME gio_ra
    }

    thuong_phat {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay
        INT nhan_vien_id FK
        DECIMAL so_tien_thuong
        DECIMAL so_tien_phat
        TEXT ly_do
    }

    tam_ung_luong {
        INT id PK
        INT nhan_vien_id FK
        DATETIME ngay
        DECIMAL so_tien
        ENUM trang_thai "cho_duyet|da_duyet|da_hoan"
    }

    bang_luong {
        INT id PK
        VARCHAR ten_bang_luong
        INT thang
        INT nam
    }

    bang_luong_chi_tiet {
        INT id PK
        INT bang_luong_id FK
        INT nhan_vien_id FK
        DECIMAL so_ngay_cong
        DECIMAL luong_co_ban
        DECIMAL tien_thuong
        DECIMAL tien_phat
        DECIMAL tam_ung
        DECIMAL tong_luong
    }

    nhom_khach_hang ||--o{ khach_hang : "thuộc nhóm"
    nhan_vien ||--o{ cham_cong : "chấm công"
    ca_lam_viec ||--o{ cham_cong : "theo ca"
    nhan_vien ||--o{ thuong_phat : "thưởng/phạt"
    nhan_vien ||--o{ tam_ung_luong : "tạm ứng"
    bang_luong ||--o{ bang_luong_chi_tiet : "chi tiết"
    nhan_vien ||--o{ bang_luong_chi_tiet : "nhận lương"
```

---

## ERD chi tiết - Phân hệ Hóa đơn & Bán hàng

```mermaid
erDiagram
    hoa_don {
        INT id PK
        VARCHAR so_hoa_don UK "082600001"
        INT ban_id FK
        INT khach_hang_id FK
        INT nhan_vien_id FK
        INT cua_hang_id FK
        DATETIME ngay_tao
        INT so_khach
        DECIMAL tien_hang
        DECIMAL tien_giam_mat_hang
        DECIMAL tien_giam_tong_bill
        DECIMAL phan_tram_giam
        DECIMAL phi_dich_vu
        DECIMAL thue_vat
        DECIMAL tong_cong
        DECIMAL tien_mat
        DECIMAL chuyen_khoan
        DECIMAL tien_the
        DECIMAL voucher
        DECIMAL the_tra_truoc
        DECIMAL con_no
        ENUM trang_thai "dang_phuc_vu|cho_thanh_toan|da_thanh_toan|huy"
        INT thiet_bi_id FK
    }

    hoa_don_chi_tiet {
        INT id PK
        INT hoa_don_id FK
        INT mat_hang_id FK
        VARCHAR ten_hang "Snapshot"
        VARCHAR dvt "Snapshot"
        DECIMAL so_luong
        DECIMAL don_gia
        DECIMAL chiet_khau_phan_tram
        DECIMAL tien_chiet_khau
        DECIMAL thanh_tien
        ENUM trang_thai "dang_cho|dang_che_bien|da_che_bien|da_phuc_vu|huy"
        DATETIME thoi_gian_gui_bep "In che bien F10"
    }

    hoa_don_huy {
        INT id PK
        INT hoa_don_id FK
        TEXT ly_do_huy
        VARCHAR nguoi_huy
        DATETIME ngay_huy
    }

    hoa_don ||--o{ hoa_don_chi_tiet : "gồm nhiều món"
    hoa_don ||--o| hoa_don_huy : "có thể bị hủy"
    ban ||--o{ hoa_don : "phục vụ tại bàn"
    khach_hang ||--o{ hoa_don : "khách mua"
    nhan_vien ||--o{ hoa_don : "nhân viên phục vụ"
    mat_hang ||--o{ hoa_don_chi_tiet : "món được bán"
```

---

## ERD chi tiết - Phân hệ Đặt hàng & Khuyến mại

```mermaid
erDiagram
    phuong_thuc_dat {
        INT id PK
        VARCHAR ten_phuong_thuc "Cong van, Dien thoai, Email, Tin nhan, Truc tiep"
    }

    dat_hang {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay_dat
        INT khach_hang_id FK
        VARCHAR ten_khach
        VARCHAR dia_chi
        VARCHAR dien_thoai
        INT phuong_thuc_dat_id FK
        VARCHAR muc_dich_dat
        TIME tu_gio
        TIME den_gio
        DECIMAL tong_cong
        ENUM trang_thai "moi|da_xac_nhan|dang_phuc_vu|hoan_tat|huy"
    }

    dat_hang_chi_tiet {
        INT id PK
        INT dat_hang_id FK
        INT mat_hang_id FK
        DECIMAL so_luong
        DECIMAL don_gia
        DECIMAL thanh_tien
    }

    loai_khuyen_mai {
        INT id PK
        VARCHAR ten_loai "Giam gia phan tram, Giam gia tien, Mua X tang Y"
    }

    dot_khuyen_mai {
        INT id PK
        VARCHAR ten_dot
        INT loai_km_id FK
        DATE tu_ngay
        DATE den_ngay
        TINYINT ngung_ap_dung
        DECIMAL ti_le_giam_gia
        TIME tu_gio
        TIME den_gio
    }

    khuyen_mai_chi_tiet {
        INT id PK
        INT dot_km_id FK
        INT mat_hang_id FK
        INT nhom_mat_hang_id FK
        DECIMAL ti_le_giam
        DECIMAL so_tien_giam
        INT so_luong_mua
        INT so_luong_tang
    }

    phuong_thuc_dat ||--o{ dat_hang : "phương thức"
    khach_hang ||--o{ dat_hang : "đặt hàng"
    dat_hang ||--o{ dat_hang_chi_tiet : "chi tiết"
    mat_hang ||--o{ dat_hang_chi_tiet : "món được đặt"
    loai_khuyen_mai ||--o{ dot_khuyen_mai : "loại KM"
    dot_khuyen_mai ||--o{ khuyen_mai_chi_tiet : "chi tiết"
    mat_hang ||--o{ khuyen_mai_chi_tiet : "áp dụng cho"
```

---

## ERD chi tiết - Phân hệ Thẻ trả trước & Nhà cung cấp

```mermaid
erDiagram
    nhom_the_tra_truoc {
        INT id PK
        VARCHAR ten_nhom
        TINYINT trang_thai
    }

    the_tra_truoc {
        INT id PK
        VARCHAR ma_the UK "123456"
        INT nhom_the_id FK
        INT khach_hang_id FK
        DECIMAL so_du
        TINYINT khoa "0 Mo, 1 Khoa"
        DATE ngay_het_han
    }

    the_tra_truoc_lich_su {
        INT id PK
        INT the_id FK
        ENUM loai_giao_dich "nap|su_dung|hoan"
        DECIMAL so_tien
        DECIMAL so_du_truoc
        DECIMAL so_du_sau
        INT hoa_don_id FK
        DATETIME ngay_giao_dich
    }

    nhom_nha_cung_cap {
        INT id PK
        VARCHAR ten_nhom
        TINYINT trang_thai
    }

    nha_cung_cap {
        INT id PK
        VARCHAR ten_ncc
        INT nhom_ncc_id FK
        VARCHAR dia_chi
        VARCHAR dien_thoai
        VARCHAR email
        DECIMAL con_no
    }

    nhom_the_tra_truoc ||--o{ the_tra_truoc : "thuộc nhóm"
    khach_hang ||--o{ the_tra_truoc : "sở hữu thẻ"
    the_tra_truoc ||--o{ the_tra_truoc_lich_su : "lịch sử giao dịch"
    hoa_don ||--o{ the_tra_truoc_lich_su : "thanh toán bằng thẻ"
    nhom_nha_cung_cap ||--o{ nha_cung_cap : "thuộc nhóm"
```

---

## ERD chi tiết - Phân hệ Kho hàng

```mermaid
erDiagram
    kho_hang {
        INT id PK
        VARCHAR ten_kho "KHO BAN HANG"
        INT cua_hang_id FK
        VARCHAR dia_chi
        TINYINT trang_thai
    }

    phieu_nhap_kho {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay_nhap
        INT nha_cung_cap_id FK
        INT kho_id FK
        INT nhan_vien_id FK
        DECIMAL tong_tien
        DECIMAL thanh_toan
        DECIMAL con_lai
        ENUM trang_thai "nhap|da_duyet|huy"
    }

    phieu_nhap_kho_chi_tiet {
        INT id PK
        INT phieu_nhap_id FK
        INT mat_hang_id FK
        DECIMAL so_luong
        DECIMAL don_gia
        DECIMAL thanh_tien
        INT dvt_id FK
    }

    phieu_xuat_kho {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay_xuat
        INT kho_id FK
        INT nhan_vien_id FK
        VARCHAR ly_do_xuat
        DECIMAL tong_tien
        ENUM trang_thai "nhap|da_duyet|huy"
    }

    phieu_xuat_kho_chi_tiet {
        INT id PK
        INT phieu_xuat_id FK
        INT mat_hang_id FK
        DECIMAL so_luong
        DECIMAL don_gia
        DECIMAL thanh_tien
        INT dvt_id FK
    }

    phieu_chuyen_kho {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay_chuyen
        INT kho_xuat_id FK
        INT kho_nhap_id FK
        INT nhan_vien_xuat FK
        INT nhan_vien_nhap FK
        DECIMAL tong_cong
        ENUM trang_thai "cho_duyet|da_duyet|huy"
    }

    phieu_chuyen_kho_chi_tiet {
        INT id PK
        INT phieu_chuyen_id FK
        INT mat_hang_id FK
        DECIMAL so_luong
        INT dvt_id FK
    }

    phieu_kiem_ke {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay_kiem_ke
        INT kho_id FK
        INT nhan_vien_id FK
        ENUM trang_thai "dang_kiem|hoan_tat|huy"
    }

    phieu_kiem_ke_chi_tiet {
        INT id PK
        INT phieu_kiem_ke_id FK
        INT mat_hang_id FK
        DECIMAL ton_he_thong
        DECIMAL ton_thuc_te
        DECIMAL chenh_lech
        INT dvt_id FK
    }

    ton_kho {
        INT id PK
        INT mat_hang_id FK
        INT kho_id FK
        DECIMAL so_luong_ton
        DECIMAL gia_von_tb
        DECIMAL quy_doi
        DECIMAL ton_2_dvt
    }

    kho_hang ||--o{ phieu_nhap_kho : "nhập vào"
    kho_hang ||--o{ phieu_xuat_kho : "xuất từ"
    kho_hang ||--o{ phieu_chuyen_kho : "chuyển đi"
    kho_hang ||--o{ phieu_kiem_ke : "kiểm kê"
    kho_hang ||--o{ ton_kho : "tồn kho"
    nha_cung_cap ||--o{ phieu_nhap_kho : "cung cấp"
    phieu_nhap_kho ||--o{ phieu_nhap_kho_chi_tiet : "chi tiết"
    phieu_xuat_kho ||--o{ phieu_xuat_kho_chi_tiet : "chi tiết"
    phieu_chuyen_kho ||--o{ phieu_chuyen_kho_chi_tiet : "chi tiết"
    phieu_kiem_ke ||--o{ phieu_kiem_ke_chi_tiet : "chi tiết"
    mat_hang ||--o{ ton_kho : "tồn kho"
```

---

## ERD chi tiết - Phân hệ Công nợ & Quỹ Thu chi

```mermaid
erDiagram
    cong_no_khach_hang {
        INT id PK
        INT khach_hang_id FK
        INT hoa_don_id FK
        VARCHAR so_phieu
        DATETIME ngay
        DECIMAL tong_cong
        DECIMAL tien_thanh_toan
        TEXT dien_giai
        DECIMAL luy_ke
        ENUM trang_thai "con_no|da_thanh_toan"
    }

    cong_no_nha_cung_cap {
        INT id PK
        INT nha_cung_cap_id FK
        INT phieu_nhap_id FK
        VARCHAR so_phieu
        DATETIME ngay
        DECIMAL tong_cong
        DECIMAL tien_thanh_toan
        DECIMAL luy_ke
        ENUM trang_thai "con_no|da_thanh_toan"
    }

    ly_do_thu_chi {
        INT id PK
        VARCHAR ten_ly_do "Chi luong, Tien dien, Tien nha..."
        ENUM loai "thu|chi"
        TINYINT trang_thai
    }

    tai_khoan_ngan_hang {
        INT id PK
        VARCHAR ten_tai_khoan "tk1, tk2, tk3"
        VARCHAR so_tai_khoan
        VARCHAR ngan_hang
        VARCHAR chi_nhanh
        DECIMAL so_du
    }

    phieu_thu {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay
        INT ly_do_id FK
        VARCHAR doi_tuong
        TEXT dien_giai
        DECIMAL so_tien
        TINYINT chuyen_khoan
        INT tai_khoan_ngan_hang_id FK
        INT the_tra_truoc_id FK
        INT cua_hang_id FK
        ENUM trang_thai "nhap|da_duyet|huy"
    }

    phieu_chi {
        INT id PK
        VARCHAR so_phieu
        DATETIME ngay
        INT ly_do_id FK
        VARCHAR doi_tuong
        VARCHAR dia_chi
        TEXT dien_giai
        DECIMAL so_tien
        TINYINT chuyen_khoan
        INT tai_khoan_ngan_hang_id FK
        INT cua_hang_id FK
        ENUM trang_thai "nhap|da_duyet|huy"
    }

    phieu_thu_cong_no {
        INT id PK
        INT phieu_thu_id FK
        INT cong_no_id FK
        DECIMAL so_tien
    }

    nhat_ky_hoat_dong {
        BIGINT id PK
        DATE ngay
        TIME gio
        VARCHAR so_don_hang
        TEXT noi_dung "Mo hoa don tren ban 01..."
        VARCHAR tai_khoan
        VARCHAR thiet_bi "LAPTOP-6KP13OSJ"
        VARCHAR ban
        VARCHAR chuc_nang
    }

    khach_hang ||--o{ cong_no_khach_hang : "công nợ"
    hoa_don ||--o{ cong_no_khach_hang : "phát sinh từ"
    nha_cung_cap ||--o{ cong_no_nha_cung_cap : "công nợ"
    phieu_nhap_kho ||--o{ cong_no_nha_cung_cap : "phát sinh từ"
    ly_do_thu_chi ||--o{ phieu_thu : "lý do"
    ly_do_thu_chi ||--o{ phieu_chi : "lý do"
    tai_khoan_ngan_hang ||--o{ phieu_thu : "qua TK"
    tai_khoan_ngan_hang ||--o{ phieu_chi : "qua TK"
    phieu_thu ||--o{ phieu_thu_cong_no : "thu công nợ"
    cong_no_khach_hang ||--o{ phieu_thu_cong_no : "thanh toán"
```

---

## Tổng kết

| Thống kê | Số lượng |
|----------|----------|
| **Tổng số bảng** | 42 |
| **Views báo cáo** | 4 |
| **Phân hệ** | 15 |
| **Foreign Keys** | 60+ |
| **Indexes** | 15+ |

> [!TIP]
> File SQL đầy đủ: [quanlybar_database.sql](file:///d:/QuanLyBar/database/quanlybar_database.sql)
