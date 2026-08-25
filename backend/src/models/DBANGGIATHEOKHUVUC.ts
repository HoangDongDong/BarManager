import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DBANGGIATHEOKHUVUC extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public dkhuvucid!: any;
    public usercreatedid!: any;
    public dbanggiaid!: any;
}

DBANGGIATHEOKHUVUC.init({
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
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DBANGGIATHEOKHUVUC',
    tableName: 'DBANGGIATHEOKHUVUC',
    timestamps: false
});

export default DBANGGIATHEOKHUVUC;
