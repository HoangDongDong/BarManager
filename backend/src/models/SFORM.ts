import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SFORM extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public sortorder!: any;
    public parentid!: any;
    public parentdir!: any;
    public itemtype!: any;
    public autoid!: any;
    public simageid!: any;
    public code!: any;
    public designcode!: any;
    public formtype!: any;
    public stabledescid!: any;
    public notemplate!: any;
    public aelayout!: any;
    public loai!: any;
    public image32!: any;
    public tabledesc!: any;
    public lasttemplateid!: any;
    public classname!: any;
    public filterconfig!: any;
    public refconfig!: any;
    public reportdesc!: any;
    public sfunctionid!: any;
    public showtotal!: any;
    public clientcode!: any;
    public servercode!: any;
    public tscode!: any;
    public tslayout!: any;
    public tsdesigncode!: any;
}

SFORM.init({
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTDIR: {
        type: DataTypes.STRING,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    AUTOID: {
        type: DataTypes.INTEGER,
        
    },
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
    CODE: {
        type: DataTypes.STRING,
        
    },
    DESIGNCODE: {
        type: DataTypes.STRING,
        
    },
    FORMTYPE: {
        type: DataTypes.STRING,
        
    },
    STABLEDESCID: {
        type: DataTypes.INTEGER,
        
    },
    NOTEMPLATE: {
        type: DataTypes.STRING,
        
    },
    AELAYOUT: {
        type: DataTypes.STRING,
        
    },
    LOAI: {
        type: DataTypes.STRING,
        
    },
    IMAGE32: {
        type: DataTypes.BLOB,
        
    },
    TABLEDESC: {
        type: DataTypes.STRING,
        
    },
    LASTTEMPLATEID: {
        type: DataTypes.INTEGER,
        
    },
    CLASSNAME: {
        type: DataTypes.STRING,
        
    },
    FILTERCONFIG: {
        type: DataTypes.STRING,
        
    },
    REFCONFIG: {
        type: DataTypes.STRING,
        
    },
    REPORTDESC: {
        type: DataTypes.STRING,
        
    },
    SFUNCTIONID: {
        type: DataTypes.INTEGER,
        
    },
    SHOWTOTAL: {
        type: DataTypes.STRING,
        
    },
    CLIENTCODE: {
        type: DataTypes.STRING,
        
    },
    SERVERCODE: {
        type: DataTypes.STRING,
        
    },
    TSCODE: {
        type: DataTypes.STRING,
        
    },
    TSLAYOUT: {
        type: DataTypes.STRING,
        
    },
    TSDESIGNCODE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SFORM',
    tableName: 'SFORM',
    timestamps: false
});

export default SFORM;
