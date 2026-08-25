import { Router } from 'express';
import datHangController from '../controllers/DatHangController';

const router = Router();

router.get('/', datHangController.getList);
router.get('/:id', datHangController.getById);
router.post('/', datHangController.create);
router.put('/:id', datHangController.update);
router.delete('/:id', datHangController.delete);

export default router;
