import firebirdDb from './firebird';
import sqlServerDb from './database';

export enum DbType {
    FirebirdServer = 0,
    SqlServer = 1,
    FirebirdFile = 2
}

class DbManager {
    private static instance: DbManager;
    public currentDbType: DbType = DbType.FirebirdFile; // Default

    private constructor() {}

    public static getInstance(): DbManager {
        if (!DbManager.instance) {
            DbManager.instance = new DbManager();
        }
        return DbManager.instance;
    }

    public async setConnection(config: any) {
        this.currentDbType = config.ConnectionType;
        
        if (this.currentDbType === DbType.SqlServer) {
            // Re-init Sequelize
            await sqlServerDb.reconnect({
                host: config.Server,
                username: config.Username,
                password: config.Password,
                database: config.Path // For SQL Server, Path is usually the DB name
            });
        } else {
            // Firebird Server or File
            firebirdDb.setDatabasePath(config.Path);
            firebirdDb.updateOptions({
                host: this.currentDbType === DbType.FirebirdServer ? config.Server : '127.0.0.1',
                user: config.Username || 'SYSDBA',
                password: config.Password || 'masterkey'
            });
        }
    }
}

export default DbManager.getInstance();
