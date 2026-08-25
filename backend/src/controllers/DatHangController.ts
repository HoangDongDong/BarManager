import { Request, Response } from 'express';
import datHangService from '../services/DatHangService';

export class DatHangController {
    
    public async getList(req: Request, res: Response): Promise<void> {
        try {
            const query = {
                tu_ngay: req.query.tu_ngay as string,
                den_ngay: req.query.den_ngay as string,
                khach_hang_id: req.query.khach_hang_id ? parseInt(req.query.khach_hang_id as string) : undefined,
                phuong_thuc_dat_id: req.query.phuong_thuc_dat_id ? parseInt(req.query.phuong_thuc_dat_id as string) : undefined
            };

            const data = await datHangService.getList(query);
            res.status(200).json({ success: true, data });
        } catch (error: any) {
            res.status(500).json({ success: false, message: error.message });
        }
    }

    public async getById(req: Request, res: Response): Promise<void> {
        try {
            const id = parseInt(req.params.id as string);
            if (isNaN(id)) {
                res.status(400).json({ success: false, message: 'ID không hợp lệ' });
                return;
            }

            const data = await datHangService.getById(id);
            res.status(200).json({ success: true, data });
        } catch (error: any) {
            res.status(404).json({ success: false, message: error.message });
        }
    }

    public async create(req: Request, res: Response): Promise<void> {
        try {
            const result = await datHangService.create(req.body);
            res.status(201).json({ success: true, message: 'Tạo phiếu đặt hàng thành công', data: result });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }

    public async update(req: Request, res: Response): Promise<void> {
        try {
            const id = parseInt(req.params.id as string);
            if (isNaN(id)) {
                res.status(400).json({ success: false, message: 'ID không hợp lệ' });
                return;
            }

            const result = await datHangService.update(id, req.body);
            res.status(200).json({ success: true, message: 'Cập nhật phiếu đặt hàng thành công', data: result });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }

    public async delete(req: Request, res: Response): Promise<void> {
        try {
            const id = parseInt(req.params.id as string);
            if (isNaN(id)) {
                res.status(400).json({ success: false, message: 'ID không hợp lệ' });
                return;
            }

            const result = await datHangService.delete(id);
            res.status(200).json({ success: true, message: result.message, softDeleted: result.softDeleted });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }
}

export default new DatHangController();
