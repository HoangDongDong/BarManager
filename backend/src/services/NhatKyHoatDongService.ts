import { TLUUVET, TDONHANG } from '../models';
import { Op } from 'sequelize';

export class NhatKyHoatDongService {
    public async ghiLog(hanh_dong: string, doi_tuong: string, chi_tiet: string, nguoi_dung_id: number) {
        return await TLUUVET.create({
            ACTION: hanh_dong,
            TABLENAME: doi_tuong,
            NOTE: chi_tiet,
            USERCREATEDID: nguoi_dung_id,
            TIMECREATED: new Date()
        });
    }

    public async layLichSu(query: { tu_ngay?: string, den_ngay?: string, nguoi_dung_id?: number }) {
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

        if (query.nguoi_dung_id) {
            whereClause.USERCREATEDID = query.nguoi_dung_id;
        }

        return await TLUUVET.findAll({
            where: whereClause,
            order: [['TIMECREATED', 'DESC']]
        });
    }
}

export default new NhatKyHoatDongService();
