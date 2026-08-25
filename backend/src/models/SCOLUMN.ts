import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SCOLUMN extends Model {
    public id!: any;
    public name!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public stabledescid!: any;
    public caption!: any;
    public format!: any;
    public reftableid!: any;
    public allowempty!: any;
    public allowduplicate!: any;
    public issystem!: any;
    public enableconfigid!: any;
    public titleconfigid!: any;
    public tooltip!: any;
    public sfunctionid!: any;
    public maxvalue!: any;
    public minvalue!: any;
    public defaultconfigid!: any;
    public tag!: any;
    public controltype!: any;
    public showonlookup!: any;
    public formula!: any;
    public autoreport!: any;
}

SCOLUMN.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NAME: {
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    STABLEDESCID: {
        type: DataTypes.INTEGER,
        
    },
    CAPTION: {
        type: DataTypes.STRING,
        
    },
    FORMAT: {
        type: DataTypes.STRING,
        
    },
    REFTABLEID: {
        type: DataTypes.INTEGER,
        
    },
    ALLOWEMPTY: {
        type: DataTypes.STRING,
        
    },
    ALLOWDUPLICATE: {
        type: DataTypes.STRING,
        
    },
    ISSYSTEM: {
        type: DataTypes.BOOLEAN,
        
    },
    ENABLECONFIGID: {
        type: DataTypes.INTEGER,
        
    },
    TITLECONFIGID: {
        type: DataTypes.INTEGER,
        
    },
    TOOLTIP: {
        type: DataTypes.STRING,
        
    },
    SFUNCTIONID: {
        type: DataTypes.INTEGER,
        
    },
    MAXVALUE: {
        type: DataTypes.STRING,
        
    },
    MINVALUE: {
        type: DataTypes.STRING,
        
    },
    DEFAULTCONFIGID: {
        type: DataTypes.INTEGER,
        
    },
    TAG: {
        type: DataTypes.STRING,
        
    },
    CONTROLTYPE: {
        type: DataTypes.STRING,
        
    },
    SHOWONLOOKUP: {
        type: DataTypes.STRING,
        
    },
    FORMULA: {
        type: DataTypes.STRING,
        
    },
    AUTOREPORT: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SCOLUMN',
    tableName: 'SCOLUMN',
    timestamps: false
});

export default SCOLUMN;
