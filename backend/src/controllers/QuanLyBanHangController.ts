import { Request, Response } from 'express';
import posService from '../services/POSService';

export class QuanLyBanHangController {
    public async getListHoaDon(req: Request, res: Response): Promise<void> {
        res.status(200).json({ success: true, data: [] }); // Dummy
    }

    public async cancelHoaDon(req: Request, res: Response): Promise<void> {
        res.status(200).json({ success: true, message: 'Đã huỷ' }); // Dummy
    }

    public async getPrintBill(req: Request, res: Response): Promise<void> {
        res.status(200).json({ success: true, data: {} }); // Dummy
    }
}

export default new QuanLyBanHangController();
