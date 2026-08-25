import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDATHANG extends Model {
    public note!: any;
    public name!: any;
    public ngay!: any;
    public tenkhach!: any;
    public diachi!: any;
    public dienthoai!: any;
    public email!: any;
    public tienhang!: any;
    public tilethue!: any;
    public tienthue!: any;
    public tilegiamgia!: any;
    public tiengiamgia!: any;
    public phivanchuyen!: any;
    public tongcong!: any;
    public loai!: any;
    public giamtheotien!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dkhachhangid!: any;
    public loaigia!: any;
    public dphuongthucdatid!: any;
    public dmucdichdatid!: any;
    public tugio!: any;
    public dengio!: any;
    public tungay!: any;
    public denngay!: any;
    public giodat!: any;
    public mausac!: any;
    public dbanid!: any;
    public guid!: any;
}

TDATHANG.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
    },
    NGAY: {
        type: DataTypes.DATE,
        
    },
    TENKHACH: {
        type: DataTypes.STRING,
        
    },
    DIACHI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DIENTHOAI: {
        type: DataTypes.STRING,
        
    },
    EMAIL: {
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
    TONGCONG: {
        type: DataTypes.STRING,
        
    },
    LOAI: {
        type: DataTypes.STRING,
        
    },
    GIAMTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DKHACHHANGID: {
        type: DataTypes.INTEGER,
        
    },
    LOAIGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DPHUONGTHUCDATID: {
        type: DataTypes.INTEGER,
        
    },
    DMUCDICHDATID: {
        type: DataTypes.INTEGER,
        
    },
    TUGIO: {
        type: DataTypes.DATE,
        
    },
    DENGIO: {
        type: DataTypes.DATE,
        
    },
    TUNGAY: {
        type: DataTypes.DATE,
        
    },
    DENNGAY: {
        type: DataTypes.DATE,
        
    },
    GIODAT: {
        type: DataTypes.DATE,
        
    },
    MAUSAC: {
        type: DataTypes.STRING,
        
    },
    DBANID: {
        type: DataTypes.INTEGER,
        
    },
    GUID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDATHANG',
    tableName: 'TDATHANG',
    timestamps: false
});

export default TDATHANG;
