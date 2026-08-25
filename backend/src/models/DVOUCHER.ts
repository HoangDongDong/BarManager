import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DVOUCHER extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public dnhomvoucherid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public giatri!: any;
    public ngayphathanh!: any;
    public hansudung!: any;
    public dkhachhangid!: any;
}

DVOUCHER.init({
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
    DNHOMVOUCHERID: {
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
    GIATRI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    NGAYPHATHANH: {
        type: DataTypes.DATE,
        
    },
    HANSUDUNG: {
        type: DataTypes.DATE,
        
    },
    DKHACHHANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DVOUCHER',
    tableName: 'DVOUCHER',
    timestamps: false
});

export default DVOUCHER;
