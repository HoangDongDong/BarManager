import { Router } from 'express';
import khuVucController from '../controllers/KhuVucController';

const router = Router();

router.get('/', khuVucController.getList);
router.post('/', khuVucController.create);
router.put('/:id', khuVucController.update);
router.delete('/:id', khuVucController.delete);

export default router;
