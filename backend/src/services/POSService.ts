import db from '../config/database';
import { TDONHANG, TDONHANGCHITIET, DBAN, DMATHANG, DCUAHANG } from '../models';

export class POSService {
    public async openTable(banId: number, nvId: number) {
        const transaction = await db.sequelize.transaction();
        try {
            const ban: any = await DBAN.findByPk(banId);
            if (!ban) throw new Error('Bàn không tồn tại.');
            if (ban.STATUS === 1) throw new Error('Bàn đang có khách.');

            const order: any = await TDONHANG.create({
                DBANID: banId,
                USERCREATEDID: nvId,
                TIMECREATED: new Date(),
                STATUS: 1 // 1: Đang phục vụ
            }, { transaction });

            await ban.update({ STATUS: 1, TDONHANGID: order.id }, { transaction });
            await transaction.commit();
            return order;
        } catch (error) {
            await transaction.rollback();
            throw error;
        }
    }

    public async addItems(orderId: number, items: any[]) {
        const transaction = await db.sequelize.transaction();
        try {
            const order = await TDONHANG.findByPk(orderId);
            if (!order) throw new Error('Hóa đơn không tồn tại.');

            const details = items.map(item => ({
                TDONHANGID: orderId,
                DMATHANGID: item.mat_hang_id,
                SOLUONG: item.so_luong,
                DONGIA: item.don_gia,
                THANHTIEN: item.so_luong * item.don_gia
            }));

            await TDONHANGCHITIET.bulkCreate(details, { transaction });
            
            // Cập nhật tổng tiền hóa đơn (giả lập)
            await order.update({ TIMEMODIFIED: new Date() }, { transaction });

            await transaction.commit();
            return true;
        } catch (error) {
            await transaction.rollback();
            throw error;
        }
    }

    public async payOrder(orderId: number) {
        const transaction = await db.sequelize.transaction();
        try {
            const order: any = await TDONHANG.findByPk(orderId);
            if (!order) throw new Error('Hóa đơn không tồn tại.');

            await order.update({ STATUS: 2 }, { transaction }); // 2: Đã thanh toán
            const ban = await DBAN.findByPk(order.DBANID);
            if (ban) {
                await ban.update({ STATUS: 0, TDONHANGID: null }, { transaction });
            }

            await transaction.commit();
            return order;
        } catch (error) {
            await transaction.rollback();
            throw error;
        }
    }
}

export default new POSService();
