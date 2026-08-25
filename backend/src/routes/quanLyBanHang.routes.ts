import express from 'express';
import quanLyBanHangController from '../controllers/QuanLyBanHangController';

const router = express.Router();

router.get('/hoa-don', quanLyBanHangController.getListHoaDon);
router.post('/hoa-don/:id/cancel', quanLyBanHangController.cancelHoaDon);
router.get('/hoa-don/:id/print', quanLyBanHangController.getPrintBill);

export default router;
