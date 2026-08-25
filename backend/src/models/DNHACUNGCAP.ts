import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DNHACUNGCAP extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public dnhomnhacungcapid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public manhacungcap!: any;
    public diachi!: any;
    public dienthoai!: any;
    public email!: any;
    public website!: any;
}

DNHACUNGCAP.init({
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
    DNHOMNHACUNGCAPID: {
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
    MANHACUNGCAP: {
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
    WEBSITE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DNHACUNGCAP',
    tableName: 'DNHACUNGCAP',
    timestamps: false
});

export default DNHACUNGCAP;
