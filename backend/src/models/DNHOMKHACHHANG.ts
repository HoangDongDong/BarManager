import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DNHOMKHACHHANG extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public sortorder!: any;
    public usercreatedid!: any;
    public parentid!: any;
    public parentdir!: any;
    public itemtype!: any;
    public autoid!: any;
    public simageid!: any;
    public tilegiamgia!: any;
    public diemtichluy!: any;
    public tilegiamgiatienhang!: any;
    public tilegiamgiatiengio!: any;
    public tilegiamdoan!: any;
    public tilegiamdouong!: any;
    public tilegiamdichvu!: any;
    public tilegiamdokhac!: any;
}

DNHOMKHACHHANG.init({
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
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTDIR: {
        type: DataTypes.STRING,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    AUTOID: {
        type: DataTypes.INTEGER,
        
    },
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DIEMTICHLUY: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIATIENHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIATIENGIO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMDOAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMDOUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMDICHVU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMDOKHAC: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DNHOMKHACHHANG',
    tableName: 'DNHOMKHACHHANG',
    timestamps: false
});

export default DNHOMKHACHHANG;
