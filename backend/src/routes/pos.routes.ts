import express from 'express';
import posController from '../controllers/POSController';

const router = express.Router();

router.post('/open-table', posController.openTable);
router.post('/add-items', posController.addItems);
router.post('/pay-order', posController.payOrder);

export default router;
