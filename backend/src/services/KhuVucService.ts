import { DKHUVUC, DBAN } from '../models';

export class KhuVucService {
    
    public async getList() {
        return await DKHUVUC.findAll({
            order: [['NAME', 'ASC']]
        });
    }

    public async create(data: any) {
        const name = data.ten_khu_vuc || data.NAME;
        if (!name || name.trim() === '') {
            throw new Error('Tên khu vực không được để trống.');
        }

        const mappedData = {
            NAME: name,
            STATUS: data.trang_thai !== undefined ? data.trang_thai : data.STATUS,
            ...data
        };

        return await DKHUVUC.create(mappedData);
    }

    public async update(id: number, data: any) {
        const name = data.ten_khu_vuc !== undefined ? data.ten_khu_vuc : data.NAME;
        if (name !== undefined && name.trim() === '') {
            throw new Error('Tên khu vực không được để trống.');
        }
        
        const khuVuc = await DKHUVUC.findByPk(id);
        if (!khuVuc) {
            throw new Error('Không tìm thấy khu vực.');
        }

        const mappedData = {
            NAME: name,
            STATUS: data.trang_thai !== undefined ? data.trang_thai : data.STATUS,
        };
        // Remove undefined keys
        Object.keys(mappedData).forEach(key => mappedData[key as keyof typeof mappedData] === undefined && delete mappedData[key as keyof typeof mappedData]);

        return await khuVuc.update({ ...data, ...mappedData });
    }

    public async delete(id: number) {
        const khuVuc = await DKHUVUC.findByPk(id);
        if (!khuVuc) {
            throw new Error('Không tìm thấy khu vực.');
        }

        if (DBAN) {
            const countBan = await DBAN.count({ where: { DKHUVUCID: id } }).catch(() => 0);
            if (countBan > 0) {
                throw new Error(`Không thể xóa khu vực này vì đang chứa ${countBan} bàn.`);
            }
        }

        await khuVuc.destroy();
        return { message: 'Đã xóa khu vực thành công.' };
    }
}

export default new KhuVucService();
