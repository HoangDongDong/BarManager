import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DKHACHHANG extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public dnhomkhachhangid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public makhach!: any;
    public diachi!: any;
    public dienthoai!: any;
    public email!: any;
    public masothue!: any;
    public dnhanvienid!: any;
    public ngaysinh!: any;
    public diemtichluybandau!: any;
    public giaban!: any;
    public dtinhthanhid!: any;
    public facebook!: any;
    public dthetratruocid!: any;
}

DKHACHHANG.init({
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
    DNHOMKHACHHANGID: {
        type: DataTypes.INTEGER,
        
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
    MAKHACH: {
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
    MASOTHUE: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    NGAYSINH: {
        type: DataTypes.DATE,
        
    },
    DIEMTICHLUYBANDAU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIABAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DTINHTHANHID: {
        type: DataTypes.INTEGER,
        
    },
    FACEBOOK: {
        type: DataTypes.STRING,
        
    },
    DTHETRATRUOCID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DKHACHHANG',
    tableName: 'DKHACHHANG',
    timestamps: false
});

export default DKHACHHANG;
