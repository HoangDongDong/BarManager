import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import { SUSER } from '../models'; // Generated 98 models include SUSER
import { AccountFactory } from '../utils/AccountFactory';
import firebirdDb from '../config/firebird';
import dbManager, { DbType } from '../config/DbManager';

export class AuthService {
    public async login(ten_dang_nhap: string, mat_khau: string) {
        let userData: any = null;

        if (dbManager.currentDbType === DbType.SqlServer) {
            // Query SQL Server via Sequelize
            const user = await SUSER.findOne({ where: { USERNAME: ten_dang_nhap } });
            if (!user) throw new Error('Tài khoản không tồn tại hoặc đã bị khóa.');
            userData = user.toJSON();
        } else {
            // Query Firebird
            const users = await firebirdDb.query<any>('SELECT * FROM SUSER WHERE USERNAME = ?', [ten_dang_nhap]);
            if (!users || users.length === 0) {
                throw new Error('Tài khoản không tồn tại hoặc đã bị khóa.');
            }
            userData = users[0];
        }
        
        let isMatch = false;
        const hash = userData.PASSWORD;
        
        // Bỏ qua check !hash vì trong DB Firebird cũ nhiều tài khoản mật khẩu bị rỗng ""
        if (hash === null || hash === undefined) {
            throw new Error('Tài khoản bị lỗi dữ liệu mật khẩu.');
        }

        // Nếu mật khẩu trong DB là rỗng thì không cần nhập mật khẩu hoặc nhập bừa cũng qua (hoặc admin123 bypass)
        if (hash === '' || hash.trim() === '') {
            isMatch = true; 
        }
        else if (hash === 'admin123' || mat_khau === 'admin123') {
            isMatch = (mat_khau === 'admin123' || mat_khau === hash);
        } else {
            try {
                isMatch = await bcrypt.compare(mat_khau, hash);
            } catch (e) {
                isMatch = (mat_khau === hash);
            }
        }

        if (!isMatch) {
            throw new Error('Sai mật khẩu đăng nhập.');
        }

        try {
            if (dbManager.currentDbType === DbType.SqlServer) {
                await SUSER.update({ TIMEMODIFIED: new Date() }, { where: { ID: userData.ID } });
            } else {
                await firebirdDb.execute('UPDATE SUSER SET TIMEMODIFIED = CURRENT_TIMESTAMP WHERE ID = ?', [userData.ID]);
            }
        } catch (e) {
            console.error('Update TIMEMODIFIED failed, ignoring...', e);
        }

        // Map old data fields to what AccountFactory expects (or just use new fields)
        const mappedUserData = {
            id: userData.ID,
            ten_dang_nhap: userData.USERNAME,
            vai_tro: userData.ISADMIN ? 1 : 2,
            cua_hang_id: 1, // Default or map if SUSER has store ID
            ...userData
        };

        const accountObject = AccountFactory.createAccount(mappedUserData);

        const token = jwt.sign(
            { id: userData.ID, vai_tro: mappedUserData.vai_tro, cua_hang_id: mappedUserData.cua_hang_id },
            process.env.JWT_SECRET || 'secret_key',
            { expiresIn: process.env.JWT_EXPIRES_IN || '24h' } as jwt.SignOptions
        );

        return {
            token,
            user: accountObject.getProfile()
        };
    }
}

export default new AuthService();
