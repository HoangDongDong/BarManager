import { Request, Response } from 'express';
import nhatKyHoatDongService from '../services/NhatKyHoatDongService';

export class NhatKyHoatDongController {
    public async layLichSu(req: Request, res: Response): Promise<void> {
        try {
            const query = {
                tu_ngay: req.query.tu_ngay as string,
                den_ngay: req.query.den_ngay as string,
                nguoi_dung_id: req.query.nguoi_dung_id ? parseInt(req.query.nguoi_dung_id as string) : undefined
            };

            const data = await nhatKyHoatDongService.layLichSu(query);
            res.status(200).json({ success: true, data });
        } catch (error: any) {
            res.status(500).json({ success: false, message: error.message });
        }
    }
}

export default new NhatKyHoatDongController();
