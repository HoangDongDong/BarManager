import Firebird from 'node-firebird';
import dotenv from 'dotenv';

dotenv.config();

// Cấu hình kết nối Firebird
const firebirdOptions: Firebird.Options = {
    host: process.env.FB_HOST || '127.0.0.1',
    port: parseInt(process.env.FB_PORT || '3050'),
    database: process.env.FB_DATABASE || '',
    user: process.env.FB_USER || 'SYSDBA',
    password: process.env.FB_PASSWORD || 'masterkey',
    lowercase_keys: false,
    role: undefined,
    pageSize: 4096,
};

/**
 * Class quản lý kết nối Firebird
 * Sử dụng raw SQL vì Sequelize không hỗ trợ Firebird
 */
class FirebirdDatabase {
    private static instance: FirebirdDatabase;
    private options: Firebird.Options;

    private constructor() {
        this.options = firebirdOptions;
    }

    public static getInstance(): FirebirdDatabase {
        if (!FirebirdDatabase.instance) {
            FirebirdDatabase.instance = new FirebirdDatabase();
        }
        return FirebirdDatabase.instance;
    }

    public setDatabasePath(path: string) {
        this.options.database = path;
    }

    public updateOptions(newOptions: Partial<Firebird.Options>) {
        this.options = { ...this.options, ...newOptions };
    }

    /**
     * Thực thi một câu query SQL trên Firebird
     * @param sql - Câu lệnh SQL
     * @param params - Tham số cho câu lệnh SQL (optional)
     * @returns Promise với kết quả trả về
     */
    public query<T = any>(sql: string, params: any[] = []): Promise<T[]> {
        return new Promise((resolve, reject) => {
            Firebird.attach(this.options, (err, db) => {
                if (err) {
                    console.error('❌ Lỗi kết nối Firebird:', err);
                    reject(err);
                    return;
                }

                db.query(sql, params, (err, result) => {
                    db.detach(); // Luôn đóng kết nối sau khi query

                    if (err) {
                        console.error('❌ Lỗi query Firebird:', err);
                        reject(err);
                        return;
                    }

                    resolve(result as T[]);
                });
            });
        });
    }

    /**
     * Thực thi câu lệnh INSERT/UPDATE/DELETE trên Firebird
     * @param sql - Câu lệnh SQL
     * @param params - Tham số cho câu lệnh SQL (optional)
     * @returns Promise<void>
     */
    public execute(sql: string, params: any[] = []): Promise<void> {
        return new Promise((resolve, reject) => {
            Firebird.attach(this.options, (err, db) => {
                if (err) {
                    console.error('❌ Lỗi kết nối Firebird:', err);
                    reject(err);
                    return;
                }

                db.execute(sql, params, (err) => {
                    db.detach();

                    if (err) {
                        console.error('❌ Lỗi execute Firebird:', err);
                        reject(err);
                        return;
                    }

                    resolve();
                });
            });
        });
    }

    /**
     * Kiểm tra kết nối tới Firebird
     * @returns Promise<boolean>
     */
    public testConnection(): Promise<boolean> {
        return new Promise((resolve) => {
            Firebird.attach(this.options, (err, db) => {
                if (err) {
                    console.error('❌ Không thể kết nối Firebird:', err.message);
                    resolve(false);
                    return;
                }

                console.log('✅ Kết nối Firebird thành công!');
                db.detach();
                resolve(true);
            });
        });
    }

    /**
     * Thực thi transaction trên Firebird
     * @param callback - Hàm nhận transaction để thực thi các câu lệnh
     */
    public transaction(callback: (db: Firebird.Database) => Promise<void>): Promise<void> {
        return new Promise((resolve, reject) => {
            Firebird.attach(this.options, (err, db) => {
                if (err) {
                    reject(err);
                    return;
                }

                db.transaction(Firebird.ISOLATION_READ_COMMITTED, (err, transaction) => {
                    if (err) {
                        db.detach();
                        reject(err);
                        return;
                    }

                    callback(db)
                        .then(() => {
                            transaction.commit((err) => {
                                db.detach();
                                if (err) reject(err);
                                else resolve();
                            });
                        })
                        .catch((error) => {
                            transaction.rollback(() => {
                                db.detach();
                                reject(error);
                            });
                        });
                });
            });
        });
    }
}

export default FirebirdDatabase.getInstance();
