import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TTHUCHI extends Model {
    public note!: any;
    public name!: any;
    public ngay!: any;
    public tendoituong!: any;
    public diachi!: any;
    public loai!: any;
    public loaidoituong!: any;
    public diengiai!: any;
    public chungtugoc!: any;
    public thu!: any;
    public chi!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dnhacungcapid!: any;
    public chuyenkhoan!: any;
    public latamung!: any;
    public tbangluongid!: any;
    public tdathangid!: any;
    public dcuahangid!: any;
    public laphieuthucongno!: any;
    public khongthaydoicongno!: any;
    public dnhanvienid!: any;
    public dkhachhangid!: any;
    public dlydothuchiid!: any;
    public dtaikhoannganhangid!: any;
    public dthetratruocid!: any;
    public tdonhangid!: any;
}

TTHUCHI.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
    },
    NGAY: {
        type: DataTypes.DATE,
        
    },
    TENDOITUONG: {
        type: DataTypes.STRING,
        
    },
    DIACHI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    LOAI: {
        type: DataTypes.STRING,
        
    },
    LOAIDOITUONG: {
        type: DataTypes.STRING,
        
    },
    DIENGIAI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    CHUNGTUGOC: {
        type: DataTypes.STRING,
        
    },
    THU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    CHI: {
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
    DNHACUNGCAPID: {
        type: DataTypes.INTEGER,
        
    },
    CHUYENKHOAN: {
        type: DataTypes.STRING,
        
    },
    LATAMUNG: {
        type: DataTypes.STRING,
        
    },
    TBANGLUONGID: {
        type: DataTypes.INTEGER,
        
    },
    TDATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DCUAHANGID: {
        type: DataTypes.INTEGER,
        
    },
    LAPHIEUTHUCONGNO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    KHONGTHAYDOICONGNO: {
        type: DataTypes.STRING,
        
    },
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    DKHACHHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DLYDOTHUCHIID: {
        type: DataTypes.INTEGER,
        
    },
    DTAIKHOANNGANHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DTHETRATRUOCID: {
        type: DataTypes.INTEGER,
        
    },
    TDONHANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TTHUCHI',
    tableName: 'TTHUCHI',
    timestamps: false
});

export default TTHUCHI;
