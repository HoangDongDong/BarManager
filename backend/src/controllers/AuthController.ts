import { Request, Response } from 'express';
import authService from '../services/AuthService';
import firebirdDb from '../config/firebird';

export class AuthController {
    /**
     * API Login: POST /api/auth/login
     */
    public async login(req: Request, res: Response): Promise<void> {
        try {
            const { ten_dang_nhap, mat_khau } = req.body;

            if (!ten_dang_nhap || !mat_khau) {
                res.status(400).json({
                    success: false,
                    message: 'Vui lòng cung cấp tên đăng nhập và mật khẩu.'
                });
                return;
            }

            // Gọi Service để xử lý logic
            const loginData = await authService.login(ten_dang_nhap, mat_khau);

            res.status(200).json({
                success: true,
                message: 'Đăng nhập thành công!',
                data: loginData
            });

        } catch (error: any) {
            res.status(401).json({
                success: false,
                message: error.message || 'Lỗi đăng nhập.'
            });
        }
    }

    public async setDb(req: Request, res: Response): Promise<void> {
        try {
            const config = req.body;
            if (!config || config.ConnectionType === undefined) {
                res.status(400).json({ success: false, message: 'Thiếu thông tin cấu hình ConnectionType' });
                return;
            }
            
            // Require path for File, or Database name for Server
            if (config.ConnectionType === 2) {
                if (!config.Path) {
                    res.status(400).json({ success: false, message: 'Thiếu đường dẫn CSDL file (Path)' });
                    return;
                }
                
                const fs = require('fs');
                if (!fs.existsSync(config.Path)) {
                    // Sử dụng file TEMPLATE.FDB trắng gốc của phần mềm
                    const templatePath = 'C:/Program Files (x86)/TAN AN PHAT/POS/v6.0/Data/TEMPLATE.FDB';
                    if (fs.existsSync(templatePath)) {
                        fs.copyFileSync(templatePath, config.Path);
                        console.log(`Đã tạo file CSDL trắng (copy từ TEMPLATE) tại: ${config.Path}`);
                    } else {
                        // Fallback về DEMO.FDB nếu không tìm thấy Template
                        const currentDbPath = process.env.DB_PATH || 'D:/taifirebird/DEMO.FDB';
                        if (fs.existsSync(currentDbPath)) {
                            fs.copyFileSync(currentDbPath, config.Path);
                            console.log(`Đã tạo file CSDL (copy từ DEMO vì không thấy TEMPLATE) tại: ${config.Path}`);
                        } else {
                            res.status(400).json({ success: false, message: `Không tìm thấy file nguồn (Template/Demo) để tạo CSDL trắng.` });
                            return;
                        }
                    }
                }
            }

            // Gọi DbManager để cập nhật cấu hình chung
            const DbManager = (await import('../config/DbManager')).default;
            await DbManager.setConnection(config);
            
            res.status(200).json({ success: true, message: 'Đã thay đổi kết nối DB thành công' });
        } catch (error: any) {
            res.status(400).json({ success: false, message: error.message });
        }
    }
}

export default new AuthController();
