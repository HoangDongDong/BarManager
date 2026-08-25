import { Router } from 'express';
import authController from '../controllers/AuthController';

const router = Router();

// POST /api/auth/login
router.post('/login', authController.login);
router.post('/set-db', authController.setDb);

export default router;
