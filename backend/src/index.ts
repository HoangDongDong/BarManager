import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import db from './config/database';
import firebirdDb from './config/firebird';
import authRoutes from './routes/auth.routes';
import matHangRoutes from './routes/matHang.routes';
import khuVucRoutes from './routes/khuVuc.routes';
import datHangRoutes from './routes/datHang.routes';
import banRoutes from './routes/ban.routes';
import posRoutes from './routes/pos.routes';
import quanLyBanHangRoutes from './routes/quanLyBanHang.routes';
import luuVetRoutes from './routes/luuVet.routes';
import { verifyToken } from './middlewares/authMiddleware';

dotenv.config();

const app = express();

// Middlewares
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// API Public không cần Token
app.use('/api/auth', authRoutes);
app.get('/api/health', (req, res) => {
    res.json({ status: 'ok', message: 'Backend API is running smoothly!' });
});

// Health check cho Firebird
app.get('/api/health/firebird', async (req, res) => {
    const isConnected = await firebirdDb.testConnection();
    if (isConnected) {
        res.json({ status: 'ok', message: 'Kết nối Firebird thành công!' });
    } else {
        res.status(500).json({ status: 'error', message: 'Không thể kết nối Firebird!' });
    }
});

// Gắn Middleware xác thực (Global) cho TẤT CẢ các API bên dưới
app.use(verifyToken);

// Các API Private (Phải có Token mới truy cập được)
app.use('/api/mat-hang', matHangRoutes);
app.use('/api/ban', banRoutes);
app.use('/api/dat-hang', datHangRoutes);
app.use('/api/pos', posRoutes);
app.use('/api/quan-ly-ban-hang', quanLyBanHangRoutes);
app.use('/api/luu-vet', luuVetRoutes);

// Khởi động server
const PORT = process.env.PORT || 5000;

const startServer = async () => {
    try {
        await db.connect(); // Singleton connection
        
        // await db.sequelize.sync({ force: false }); // Uncomment để auto-sync DB
        
        app.listen(PORT, () => {
            console.log(`🚀 Server đang chạy tại http://localhost:${PORT}`);
        });
    } catch (error) {
        console.error('❌ Lỗi khởi động Server:', error);
    }
};

startServer();
