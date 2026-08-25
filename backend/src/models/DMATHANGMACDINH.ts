import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DMATHANGMACDINH extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public dkhuvucid!: any;
    public usercreatedid!: any;
    public dmathangid!: any;
    public soluong!: any;
}

DMATHANGMACDINH.init({
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
    DKHUVUCID: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    SOLUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DMATHANGMACDINH',
    tableName: 'DMATHANGMACDINH',
    timestamps: false
});

export default DMATHANGMACDINH;
