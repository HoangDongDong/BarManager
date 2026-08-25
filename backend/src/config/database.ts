import { Sequelize } from 'sequelize';
import dotenv from 'dotenv';

dotenv.config();

class Database {
    private static instance: Database;
    public sequelize: Sequelize;

    private constructor() {
        this.sequelize = new Sequelize(
            process.env.DB_NAME || 'QuanLyBar',
            process.env.DB_USER || 'sa',
            process.env.DB_PASS || 'YourPassword123',
            {
                host: process.env.DB_HOST || 'localhost',
                dialect: 'mssql',
                logging: false, // Set to console.log to see SQL queries
                dialectOptions: {
                    options: {
                        encrypt: false, // Tắt mã hóa cho local development
                        trustServerCertificate: true, // Trust self-signed certs
                        instanceName: undefined, // Đặt tên instance nếu cần, ví dụ: 'SQLEXPRESS'
                    }
                },
                pool: {
                    max: 10,
                    min: 0,
                    acquire: 30000,
                    idle: 10000
                }
            }
        );
    }

    public static getInstance(): Database {
        if (!Database.instance) {
            Database.instance = new Database();
        }
        return Database.instance;
    }

    public async connect(): Promise<void> {
        try {
            await this.sequelize.authenticate();
            console.log('✅ Kết nối CSDL SQL Server thành công (Singleton Instance).');
        } catch (error) {
            console.error('❌ Lỗi kết nối CSDL SQL Server (Tiếp tục chạy để hỗ trợ Firebird):', error);
        }
    }

    public async reconnect(config: any): Promise<void> {
        try {
            await this.sequelize.close(); // Close existing connection if any
            
            this.sequelize = new Sequelize(
                config.database,
                config.username,
                config.password,
                {
                    host: config.host,
                    dialect: 'mssql',
                    logging: false,
                    dialectOptions: {
                        options: {
                            encrypt: false,
                            trustServerCertificate: true,
                        }
                    },
                    pool: {
                        max: 10,
                        min: 0,
                        acquire: 30000,
                        idle: 10000
                    }
                }
            );
            await this.sequelize.authenticate();
            console.log('✅ Đã kết nối lại CSDL SQL Server thành công.');
        } catch (error) {
            console.error('❌ Lỗi kết nối lại SQL Server:', error);
            throw new Error('Không thể kết nối SQL Server. Kiểm tra lại cấu hình.');
        }
    }
}

export default Database.getInstance();
