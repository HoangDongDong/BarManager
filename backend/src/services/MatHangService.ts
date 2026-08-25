import { Op } from 'sequelize';
import { DMATHANG, DNHOMMATHANG, DDONVITINH } from '../models';

export class MatHangService {
    public async getList(query: { ten_hang?: string, nhom_hang_id?: number, trang_thai?: string }) {
        const whereClause: any = {};
        
        if (query.ten_hang) {
            whereClause.NAME = { [Op.like]: `%${query.ten_hang}%` };
        }
        
        if (query.nhom_hang_id) {
            whereClause.DNHOMMATHANGID = query.nhom_hang_id;
        }

        if (query.trang_thai) {
            whereClause.STATUS = query.trang_thai === 'active' ? 1 : 0;
        }

        return await DMATHANG.findAll({
            where: whereClause,
            order: [['NAME', 'ASC']]
        });
    }

    public async getById(id: number) {
        const matHang = await DMATHANG.findByPk(id);
        if (!matHang) {
            throw new Error('Không tìm thấy mặt hàng.');
        }
        return matHang;
    }

    public async create(data: any) {
        const mappedData = {
            NAME: data.ten_hang || data.NAME,
            DNHOMMATHANGID: data.nhom_hang_id,
            DDONVITINHID: data.don_vi_tinh_id,
            DONGIA: data.gia_ban,
            STATUS: 1,
            ...data
        };
        return await DMATHANG.create(mappedData);
    }

    public async update(id: number, data: any) {
        const matHang = await DMATHANG.findByPk(id);
        if (!matHang) {
            throw new Error('Không tìm thấy mặt hàng.');
        }
        
        const mappedData = {
            NAME: data.ten_hang !== undefined ? data.ten_hang : data.NAME,
            DNHOMMATHANGID: data.nhom_hang_id !== undefined ? data.nhom_hang_id : data.DNHOMMATHANGID,
            DDONVITINHID: data.don_vi_tinh_id !== undefined ? data.don_vi_tinh_id : data.DDONVITINHID,
            DONGIA: data.gia_ban !== undefined ? data.gia_ban : data.DONGIA,
        };
        Object.keys(mappedData).forEach(key => mappedData[key as keyof typeof mappedData] === undefined && delete mappedData[key as keyof typeof mappedData]);

        return await matHang.update({ ...data, ...mappedData });
    }

    public async delete(id: number) {
        const matHang = await DMATHANG.findByPk(id);
        if (!matHang) {
            throw new Error('Không tìm thấy mặt hàng.');
        }

        await matHang.update({ STATUS: 0 });
        return { message: 'Đã chuyển mặt hàng sang trạng thái ngừng kinh doanh.' };
    }
}

export default new MatHangService();
