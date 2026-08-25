import { Router } from 'express';
import banController from '../controllers/BanController';

const router = Router();

router.get('/', banController.getList);
router.post('/', banController.create);
router.put('/:id', banController.update);
router.delete('/:id', banController.delete);

export default router;
