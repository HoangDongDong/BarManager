import { Op } from 'sequelize';
import { DKHUVUC, DBAN, TDONHANG } from '../models';

class ConflictError extends Error {
    public status = 409;
    constructor(message: string) {
        super(message);
        this.name = 'ConflictError';
    }
}

export class BanService {
    
    public async getList(query: { ten_ban?: string, khu_vuc_id?: number }) {
        const whereClause: any = {};
        
        // Map frontend/old query fields to new DB fields
        if (query.ten_ban) {
            whereClause.NAME = {
                [Op.like]: `%${query.ten_ban}%`
            };
        }
        
        if (query.khu_vuc_id) {
            whereClause.DKHUVUCID = query.khu_vuc_id;
        }

        return await DBAN.findAll({
            where: whereClause,
            // Temporarily disable include if DKHUVUC association isn't set up yet in index.ts
            // include: [
            //     {
            //         model: DKHUVUC,
            //         as: 'khu_vuc',
            //         attributes: ['ID', 'NAME']
            //     }
            // ],
            order: [['NAME', 'ASC']] // removed thu_tu since DBAN might not have it
        });
    }

    public async create(data: any) {
        this.validateBan(data);
        
        // Map old data to new schema
        const mappedData = {
            NAME: data.ten_ban || data.NAME,
            DKHUVUCID: data.khu_vuc_id || data.DKHUVUCID,
            STATUS: data.trang_thai !== undefined ? data.trang_thai : data.STATUS,
            ...data
        };
        
        return await DBAN.create(mappedData);
    }

    public async update(id: number, data: any) {
        this.validateBan(data, true);
        
        const ban = await DBAN.findByPk(id);
        if (!ban) {
            throw new Error('Không tìm thấy bàn.');
        }

        const mappedData = {
            NAME: data.ten_ban !== undefined ? data.ten_ban : data.NAME,
            DKHUVUCID: data.khu_vuc_id !== undefined ? data.khu_vuc_id : data.DKHUVUCID,
            STATUS: data.trang_thai !== undefined ? data.trang_thai : data.STATUS,
        };
        // Remove undefined keys
        Object.keys(mappedData).forEach(key => mappedData[key as keyof typeof mappedData] === undefined && delete mappedData[key as keyof typeof mappedData]);

        return await ban.update({ ...data, ...mappedData });
    }

    public async delete(id: number) {
        const ban = await DBAN.findByPk(id);
        if (!ban) {
            throw new Error('Không tìm thấy bàn.');
        }

        // Kiểm tra bàn trong bảng hóa đơn (TDONHANG)
        if (TDONHANG) {
            const countHoaDon = await TDONHANG.count({ where: { DBANID: id } }).catch(() => 0);
            if (countHoaDon > 0) {
                throw new ConflictError('Không thể xóa bàn đã phát sinh giao dịch.');
            }
        }

        await ban.destroy();
        return { message: 'Đã xóa cứng bàn thành công.' };
    }

    private validateBan(data: any, isUpdate: boolean = false) {
        const name = data.ten_ban || data.NAME;
        if (!isUpdate || name !== undefined) {
            if (!name || name.trim() === '') {
                throw new Error('Tên bàn không được để trống.');
            }
        }
        
        const khuVuc = data.khu_vuc_id || data.DKHUVUCID;
        if (!isUpdate || khuVuc !== undefined) {
            if (!khuVuc) {
                throw new Error('Vui lòng chọn khu vực.');
            }
        }
    }
}

export default new BanService();
export { ConflictError };
