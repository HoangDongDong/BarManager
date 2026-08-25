import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TLUUVET extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public gio!: any;
    public sodonhang!: any;
    public taikhoan!: any;
    public thietbi!: any;
    public phanloai!: any;
    public ban!: any;
    public chucnang!: any;
    public soluong!: any;
    public dongia!: any;
    public thanhtien!: any;
    public tenhang!: any;
}

TLUUVET.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NOTE: {
        type: DataTypes.STRING,
        
    },
    STATUS: {
        type: DataTypes.BOOLEAN,
        
    },
    USERMODIFIEDID: {
        type: DataTypes.INTEGER,
        
    },
    TIMEMODIFIED: {
        type: DataTypes.DATE,
        
    },
    TIMECREATED: {
        type: DataTypes.DATE,
        
    },
    NGAY: {
        type: DataTypes.DATE,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    GIO: {
        type: DataTypes.DATE,
        
    },
    SODONHANG: {
        type: DataTypes.STRING,
        
    },
    TAIKHOAN: {
        type: DataTypes.STRING,
        
    },
    THIETBI: {
        type: DataTypes.STRING,
        
    },
    PHANLOAI: {
        type: DataTypes.STRING,
        
    },
    BAN: {
        type: DataTypes.STRING,
        
    },
    CHUCNANG: {
        type: DataTypes.STRING,
        
    },
    SOLUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    THANHTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TENHANG: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TLUUVET',
    tableName: 'TLUUVET',
    timestamps: false
});

export default TLUUVET;
