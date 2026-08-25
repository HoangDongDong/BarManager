import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DTHETRATRUOC extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public dnhomthetratruocid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public khoa!: any;
    public ngayhethan!: any;
}

DTHETRATRUOC.init({
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
    DNHOMTHETRATRUOCID: {
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
    KHOA: {
        type: DataTypes.STRING,
        
    },
    NGAYHETHAN: {
        type: DataTypes.DATE,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DTHETRATRUOC',
    tableName: 'DTHETRATRUOC',
    timestamps: false
});

export default DTHETRATRUOC;
