import { Router } from 'express';
import matHangController from '../controllers/MatHangController';
import { checkRole } from '../middlewares/authMiddleware';

const router = Router();

// GET /api/mat-hang
router.get('/', matHangController.getList);

// POST /api/mat-hang
router.post('/', matHangController.create);

// PUT /api/mat-hang/:id
router.put('/:id', matHangController.update);

// DELETE /api/mat-hang/:id (Chỉ Admin và Quản lý mới được xóa món)
router.delete('/:id', checkRole(['quan_ly']), matHangController.delete);

export default router;
