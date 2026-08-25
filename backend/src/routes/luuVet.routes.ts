import express from 'express';
import nhatKyHoatDongController from '../controllers/NhatKyHoatDongController';

const router = express.Router();

router.get('/', nhatKyHoatDongController.layLichSu);

export default router;
