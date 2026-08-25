import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SCONFIG extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public sortorder!: any;
    public usercreatedid!: any;
    public textvalue!: any;
    public datetimevalue!: any;
    public intvalue!: any;
    public decimalvalue!: any;
    public datatype!: any;
    public blobvalue!: any;
    public caption!: any;
    public itemtype!: any;
    public simageid!: any;
    public moredetail!: any;
    public controltype!: any;
    public reftableid!: any;
    public otherconfig!: any;
    public sconfiggroupid!: any;
    public tab!: any;
    public socot!: any;
    public header!: any;
    public footer!: any;
    public showonreport!: any;
    public parentid!: any;
}

SCONFIG.init({
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
    TEXTVALUE: {
        type: DataTypes.STRING,
        
    },
    DATETIMEVALUE: {
        type: DataTypes.DATE,
        
    },
    INTVALUE: {
        type: DataTypes.STRING,
        
    },
    DECIMALVALUE: {
        type: DataTypes.STRING,
        
    },
    DATATYPE: {
        type: DataTypes.STRING,
        
    },
    BLOBVALUE: {
        type: DataTypes.BLOB,
        
    },
    CAPTION: {
        type: DataTypes.STRING,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
    MOREDETAIL: {
        type: DataTypes.STRING,
        
    },
    CONTROLTYPE: {
        type: DataTypes.STRING,
        
    },
    REFTABLEID: {
        type: DataTypes.INTEGER,
        
    },
    OTHERCONFIG: {
        type: DataTypes.STRING,
        
    },
    SCONFIGGROUPID: {
        type: DataTypes.INTEGER,
        
    },
    TAB: {
        type: DataTypes.STRING,
        
    },
    SOCOT: {
        type: DataTypes.INTEGER,
        
    },
    HEADER: {
        type: DataTypes.STRING,
        
    },
    FOOTER: {
        type: DataTypes.STRING,
        
    },
    SHOWONREPORT: {
        type: DataTypes.STRING,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SCONFIG',
    tableName: 'SCONFIG',
    timestamps: false
});

export default SCONFIG;
