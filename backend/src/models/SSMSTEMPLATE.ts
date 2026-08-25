import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SSMSTEMPLATE extends Model {
    public autoid!: any;
    public status!: any;
    public itemtype!: any;
    public timemodified!: any;
    public timecreated!: any;
    public id!: any;
    public name!: any;
    public note!: any;
    public usermodifiedid!: any;
    public sortorder!: any;
    public usercreatedid!: any;
    public parentid!: any;
    public parentdir!: any;
    public simageid!: any;
}

SSMSTEMPLATE.init({
    AUTOID: {
        type: DataTypes.INTEGER,
        
    },
    STATUS: {
        type: DataTypes.BOOLEAN,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    TIMEMODIFIED: {
        type: DataTypes.DATE,
        
    },
    TIMECREATED: {
        type: DataTypes.DATE,
        
    },
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
    USERMODIFIEDID: {
        type: DataTypes.INTEGER,
        
    },
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTDIR: {
        type: DataTypes.STRING,
        
    },
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SSMSTEMPLATE',
    tableName: 'SSMSTEMPLATE',
    timestamps: false
});

export default SSMSTEMPLATE;
