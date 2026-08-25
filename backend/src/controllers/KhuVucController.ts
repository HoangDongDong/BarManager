import { Request, Response } from 'express';
import khuVucService from '../services/KhuVucService';

export class KhuVucController {
    
    public async getList(req: Request, res: Response): Promise<void> {
        try {
            const data = await khuVucService.getList();
            res.status(200).json({ success: true, data });
        } catch (error: any) {
            res.status(500).json({ success: false, message: error.message });
        }
    }

    public async create(req: Request, res: Response): Promise<void> {
        try {
            const result = await khuVucService.create(req.body);
            res.status(201).json({ success: true, message: 'Thêm mới khu vực thành công', data: result });
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

            const result = await khuVucService.update(id, req.body);
            res.status(200).json({ success: true, message: 'Cập nhật khu vực thành công', data: result });
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

            const result = await khuVucService.delete(id);
            res.status(200).json({ success: true, message: result.message });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message }); // Lỗi 400 nếu chứa bàn
        }
    }
}

export default new KhuVucController();
