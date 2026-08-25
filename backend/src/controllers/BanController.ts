import { Request, Response } from 'express';
import banService, { ConflictError } from '../services/BanService';

export class BanController {
    
    public async getList(req: Request, res: Response): Promise<void> {
        try {
            const query = {
                ten_ban: req.query.ten_ban as string,
                khu_vuc_id: req.query.khu_vuc_id ? parseInt(req.query.khu_vuc_id as string) : undefined
            };

            const data = await banService.getList(query);
            res.status(200).json({ success: true, data });
        } catch (error: any) {
            res.status(500).json({ success: false, message: error.message });
        }
    }

    public async create(req: Request, res: Response): Promise<void> {
        try {
            const result = await banService.create(req.body);
            res.status(201).json({ success: true, message: 'Thêm mới bàn thành công', data: result });
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

            const result = await banService.update(id, req.body);
            res.status(200).json({ success: true, message: 'Cập nhật bàn thành công', data: result });
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

            const result = await banService.delete(id);
            res.status(200).json({ success: true, message: result.message });
        } catch (error: any) {
            if (error instanceof ConflictError || error.status === 409) {
                res.status(409).json({ success: false, message: error.message });
            } else {
                res.status(400).json({ success: false, message: error.message });
            }
        }
    }
}

export default new BanController();
