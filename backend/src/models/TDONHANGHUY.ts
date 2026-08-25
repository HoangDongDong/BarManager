import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDONHANGHUY extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public khachhang!: any;
    public nhanven!: any;
    public thungan!: any;
    public doitra!: any;
    public dathanhtoan!: any;
    public giothanhtoan!: any;
    public tralai!: any;
    public tienhang!: any;
    public tilethue!: any;
    public tienthue!: any;
    public tilegiamgia!: any;
    public tiengiamgia!: any;
    public phivanchuyen!: any;
    public thanhtoanboi!: any;
    public ngayhuy!: any;
    public giohuy!: any;
    public tdonhangid!: any;
    public lydohuy!: any;
    public tiengio!: any;
    public phidichvu!: any;
    public ban!: any;
}

TDONHANGHUY.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NAME: {
        type: DataTypes.STRING,
        
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
    KHACHHANG: {
        type: DataTypes.STRING,
        
    },
    NHANVEN: {
        type: DataTypes.STRING,
        
    },
    THUNGAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DOITRA: {
        type: DataTypes.STRING,
        
    },
    DATHANHTOAN: {
        type: DataTypes.STRING,
        
    },
    GIOTHANHTOAN: {
        type: DataTypes.DATE,
        
    },
    TRALAI: {
        type: DataTypes.STRING,
        
    },
    TIENHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILETHUE: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENTHUE: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    PHIVANCHUYEN: {
        type: DataTypes.STRING,
        
    },
    THANHTOANBOI: {
        type: DataTypes.STRING,
        
    },
    NGAYHUY: {
        type: DataTypes.DATE,
        
    },
    GIOHUY: {
        type: DataTypes.DATE,
        
    },
    TDONHANGID: {
        type: DataTypes.INTEGER,
        
    },
    LYDOHUY: {
        type: DataTypes.STRING,
        
    },
    TIENGIO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    PHIDICHVU: {
        type: DataTypes.STRING,
        
    },
    BAN: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDONHANGHUY',
    tableName: 'TDONHANGHUY',
    timestamps: false
});

export default TDONHANGHUY;
