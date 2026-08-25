import { Request, Response, NextFunction } from 'express';
import jwt from 'jsonwebtoken';

// Mở rộng interface Request của Express để chứa thông tin user
declare global {
    namespace Express {
        interface Request {
            user?: any;
        }
    }
}

/**
 * Middleware: Xác thực JWT Token
 */
export const verifyToken = (req: Request, res: Response, next: NextFunction): void => {
    const authHeader = req.headers.authorization;

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
        res.status(401).json({ success: false, message: 'Vui lòng cung cấp Token xác thực hợp lệ (Bearer Token).' });
        return;
    }

    const token = authHeader.split(' ')[1];

    try {
        const decoded = jwt.verify(token, process.env.JWT_SECRET || 'secret_key');
        req.user = decoded; // Gắn dữ liệu payload của token (id, vai_tro...) vào request
        next();
    } catch (error) {
        res.status(401).json({ success: false, message: 'Token không hợp lệ hoặc đã hết hạn.' });
    }
};

/**
 * Middleware: Phân quyền Role (RBAC)
 * Lưu ý nghiệp vụ: Admin luôn có toàn quyền (Bypass check).
 */
export const checkRole = (allowedRoles: string[]) => {
    return (req: Request, res: Response, next: NextFunction): void => {
        if (!req.user) {
            res.status(401).json({ success: false, message: 'Không tìm thấy thông tin xác thực.' });
            return;
        }

        const userRole = req.user.vai_tro;

        // Admin có đặc quyền thực hiện mọi thao tác của nhân viên (không bị chặn)
        if (userRole === 'admin') {
            return next();
        }

        // Kiểm tra xem role của user hiện tại có nằm trong danh sách cho phép không
        if (!allowedRoles.includes(userRole)) {
            res.status(403).json({ 
                success: false, 
                message: 'Truy cập bị từ chối. Bạn không có quyền thực hiện chức năng này.' 
            });
            return;
        }

        next();
    };
};
