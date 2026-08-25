import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TLUUTAM extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public doitra!: any;
    public giaohang!: any;
    public tdathangid!: any;
    public loaigia!: any;
    public diengiai!: any;
    public dnhanvienxuatid!: any;
    public dkhoxuatid!: any;
    public tienhang!: any;
    public tilethue!: any;
    public tienthue!: any;
    public tilegiamgia!: any;
    public tiengiamgia!: any;
    public phivanchuyen!: any;
    public tongcong!: any;
    public giamtheotien!: any;
    public dkhachhangid!: any;
}

TLUUTAM.init({
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
    DOITRA: {
        type: DataTypes.STRING,
        
    },
    GIAOHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TDATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    LOAIGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DIENGIAI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DNHANVIENXUATID: {
        type: DataTypes.INTEGER,
        
    },
    DKHOXUATID: {
        type: DataTypes.INTEGER,
        
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
    GIAMTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DKHACHHANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TLUUTAM',
    tableName: 'TLUUTAM',
    timestamps: false
});

export default TLUUTAM;
