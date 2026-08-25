import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DMATKHAUWIFI extends Model {
    public note!: any;
    public name!: any;
    public dnhomwifiid!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
}

DMATKHAUWIFI.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
    },
    DNHOMWIFIID: {
        type: DataTypes.INTEGER,
        
    },
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
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
}, {
    sequelize: db.sequelize,
    modelName: 'DMATKHAUWIFI',
    tableName: 'DMATKHAUWIFI',
    timestamps: false
});

export default DMATKHAUWIFI;
