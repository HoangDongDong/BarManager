import { Op } from 'sequelize';
import db from '../config/database';
import { TDATHANG, TDATHANGCHITIET, DKHACHHANG, DPHUONGTHUCDAT, DMATHANG, DDONVITINH } from '../models';

export class DatHangService {
    
    public async getList(query: { tu_ngay?: string, den_ngay?: string, khach_hang_id?: number, phuong_thuc_dat_id?: number }) {
        const whereClause: any = {};
        
        if (query.tu_ngay && query.den_ngay) {
            whereClause.TIMECREATED = {
                [Op.between]: [new Date(query.tu_ngay), new Date(query.den_ngay)]
            };
        } else if (query.tu_ngay) {
            whereClause.TIMECREATED = { [Op.gte]: new Date(query.tu_ngay) };
        } else if (query.den_ngay) {
            whereClause.TIMECREATED = { [Op.lte]: new Date(query.den_ngay) };
        }
        
        if (query.khach_hang_id) {
            whereClause.DKHACHHANGID = query.khach_hang_id;
        }

        if (query.phuong_thuc_dat_id) {
            whereClause.DPHUONGTHUCDATID = query.phuong_thuc_dat_id;
        }

        return await TDATHANG.findAll({
            where: whereClause,
            order: [['TIMECREATED', 'DESC']]
        });
    }

    public async getById(id: number) {
        const datHang = await TDATHANG.findByPk(id);

        if (!datHang) {
            throw new Error('Không tìm thấy phiếu đặt hàng.');
        }
        return datHang;
    }

    public async create(data: any) {
        const transaction = await db.sequelize.transaction();
        
        try {
            let tong_cong = 0;
            const chiTietList = data.chi_tiet || [];
            
            chiTietList.forEach((item: any) => {
                tong_cong += (item.so_luong || 0) * (item.don_gia || 0);
            });

            data.TONGCONG = tong_cong;
            data.STATUS = 1;

            const datHang = await TDATHANG.create(data, { transaction });

            const chiTietData = chiTietList.map((item: any) => ({
                ...item,
                TDATHANGID: datHang.id,
                SOLUONG: item.so_luong,
                DONGIA: item.don_gia
            }));

            if (chiTietData.length > 0) {
                await TDATHANGCHITIET.bulkCreate(chiTietData, { transaction });
            }

            await transaction.commit();
            return datHang;
        } catch (error) {
            await transaction.rollback();
            throw error;
        }
    }

    public async update(id: number, data: any) {
        const datHang = await TDATHANG.findByPk(id);
        if (!datHang) {
            throw new Error('Không tìm thấy phiếu đặt hàng.');
        }

        const transaction = await db.sequelize.transaction();
        try {
            let tong_cong = 0;
            const chiTietList = data.chi_tiet || [];
            
            chiTietList.forEach((item: any) => {
                tong_cong += (item.so_luong || 0) * (item.don_gia || 0);
            });

            data.TONGCONG = tong_cong;
            
            await datHang.update(data, { transaction });

            await TDATHANGCHITIET.destroy({ where: { TDATHANGID: id }, transaction });

            const chiTietData = chiTietList.map((item: any) => ({
                ...item,
                TDATHANGID: datHang.id,
                SOLUONG: item.so_luong,
                DONGIA: item.don_gia
            }));

            if (chiTietData.length > 0) {
                await TDATHANGCHITIET.bulkCreate(chiTietData, { transaction });
            }

            await transaction.commit();
            return datHang;
        } catch (error) {
            await transaction.rollback();
            throw error;
        }
    }

    public async delete(id: number) {
        const datHang = await TDATHANG.findByPk(id);
        if (!datHang) {
            throw new Error('Không tìm thấy phiếu đặt hàng.');
        }

        const transaction = await db.sequelize.transaction();
        try {
            await TDATHANGCHITIET.destroy({ where: { TDATHANGID: id }, transaction });
            await datHang.destroy({ transaction });
            await transaction.commit();
            return { message: 'Đã xóa phiếu đặt hàng thành công.', softDeleted: false };
        } catch (error) {
            await transaction.rollback();
            throw error;
        }
    }
}

export default new DatHangService();
