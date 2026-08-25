import { Request, Response } from 'express';
import matHangService from '../services/MatHangService';

export class MatHangController {
    
    /**
     * GET /api/mat-hang
     * Hiển thị và Lọc mặt hàng
     */
    public async getList(req: Request, res: Response): Promise<void> {
        try {
            const query = {
                ten_hang: req.query.ten_hang as string,
                nhom_mat_hang_id: req.query.nhom_mat_hang_id ? parseInt(req.query.nhom_mat_hang_id as string) : undefined
            };

            const data = await matHangService.getList(query);
            
            res.status(200).json({ success: true, data });
        } catch (error: any) {
            res.status(500).json({ success: false, message: error.message });
        }
    }

    /**
     * POST /api/mat-hang
     * Thêm mới mặt hàng
     */
    public async create(req: Request, res: Response): Promise<void> {
        try {
            // Mapping "Giá nhập" (từ UI) thành "gia_von" (vào DB) nếu có
            const data = { ...req.body };
            if (data.gia_nhap !== undefined) {
                data.gia_von = data.gia_nhap;
            }

            const result = await matHangService.create(data);
            res.status(201).json({ success: true, message: 'Thêm mới thành công', data: result });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }

    /**
     * PUT /api/mat-hang/:id
     * Cập nhật mặt hàng
     */
    public async update(req: Request, res: Response): Promise<void> {
        try {
            const id = parseInt(req.params.id as string);
            if (isNaN(id)) {
                res.status(400).json({ success: false, message: 'ID không hợp lệ' });
                return;
            }

            // Mapping "Giá nhập" thành "gia_von"
            const data = { ...req.body };
            if (data.gia_nhap !== undefined) {
                data.gia_von = data.gia_nhap;
            }

            const result = await matHangService.update(id, data);
            res.status(200).json({ success: true, message: 'Cập nhật thành công', data: result });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }

    /**
     * DELETE /api/mat-hang/:id
     * Xóa mặt hàng
     */
    public async delete(req: Request, res: Response): Promise<void> {
        try {
            const id = parseInt(req.params.id as string);
            if (isNaN(id)) {
                res.status(400).json({ success: false, message: 'ID không hợp lệ' });
                return;
            }

            const result = await matHangService.delete(id);
            res.status(200).json({ success: true, message: result.message, softDeleted: (result as any).softDeleted });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }
}

export default new MatHangController();
