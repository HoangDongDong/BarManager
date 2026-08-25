import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TBANGLUONGCHITIET extends Model {
    public id!: any;
    public note!: any;
    public tbangluongid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dnhanvienid!: any;
    public dcalamviecid!: any;
    public trangthai!: any;
    public ngay!: any;
}

TBANGLUONGCHITIET.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NOTE: {
        type: DataTypes.STRING,
        
    },
    TBANGLUONGID: {
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
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    DCALAMVIECID: {
        type: DataTypes.INTEGER,
        
    },
    TRANGTHAI: {
        type: DataTypes.STRING,
        
    },
    NGAY: {
        type: DataTypes.DATE,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TBANGLUONGCHITIET',
    tableName: 'TBANGLUONGCHITIET',
    timestamps: false
});

export default TBANGLUONGCHITIET;
