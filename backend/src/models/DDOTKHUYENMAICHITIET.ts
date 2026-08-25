import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DDOTKHUYENMAICHITIET extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public ddotkhuyenmaiid!: any;
    public dnhommathangid!: any;
    public tilegiamgia!: any;
    public dmathangid!: any;
    public giaban!: any;
    public giatridonhang!: any;
    public soluongmathang!: any;
    public soluongmua!: any;
    public soluongtang!: any;
    public dmathangtangid!: any;
}

DDOTKHUYENMAICHITIET.init({
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DDOTKHUYENMAIID: {
        type: DataTypes.INTEGER,
        
    },
    DNHOMMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    GIABAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIATRIDONHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SOLUONGMATHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SOLUONGMUA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SOLUONGTANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DMATHANGTANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DDOTKHUYENMAICHITIET',
    tableName: 'DDOTKHUYENMAICHITIET',
    timestamps: false
});

export default DDOTKHUYENMAICHITIET;
