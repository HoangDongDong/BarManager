import { Request, Response } from 'express';
import posService from '../services/POSService';

export class POSController {
    public async openTable(req: Request, res: Response): Promise<void> {
        try {
            const { ban_id, nv_id } = req.body;
            const order = await posService.openTable(ban_id, nv_id);
            res.status(200).json({ success: true, data: order });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }

    public async addItems(req: Request, res: Response): Promise<void> {
        try {
            const { order_id, items } = req.body;
            await posService.addItems(order_id, items);
            res.status(200).json({ success: true, message: 'Thêm món thành công' });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }

    public async payOrder(req: Request, res: Response): Promise<void> {
        try {
            const { order_id } = req.body;
            const order = await posService.payOrder(order_id);
            res.status(200).json({ success: true, data: order, message: 'Thanh toán thành công' });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }
}

export default new POSController();
