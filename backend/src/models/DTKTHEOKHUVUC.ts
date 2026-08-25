import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DTKTHEOKHUVUC extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public dkhuvucid!: any;
    public usercreatedid!: any;
    public suserid!: any;
}

DTKTHEOKHUVUC.init({
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
    SUSERID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DTKTHEOKHUVUC',
    tableName: 'DTKTHEOKHUVUC',
    timestamps: false
});

export default DTKTHEOKHUVUC;
