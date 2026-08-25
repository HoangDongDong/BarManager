import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SIMAGE extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public sortorder!: any;
    public usercreatedid!: any;
    public image!: any;
}

SIMAGE.init({
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
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    IMAGE: {
        type: DataTypes.BLOB,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SIMAGE',
    tableName: 'SIMAGE',
    timestamps: false
});

export default SIMAGE;
