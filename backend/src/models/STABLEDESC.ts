import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class STABLEDESC extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public defaultid!: any;
    public sortby!: any;
    public description!: any;
    public tabletype!: any;
    public cottang!: any;
    public cotgiam!: any;
    public sfunctionid!: any;
    public aelayout!: any;
    public sortorder!: any;
    public parentid!: any;
    public parentdir!: any;
    public itemtype!: any;
    public autoid!: any;
    public simageid!: any;
    public issystem!: any;
    public usedefaultaeform!: any;
    public genno!: any;
    public notemplate!: any;
    public image32!: any;
    public image!: any;
    public code!: any;
    public designcode!: any;
    public categorycol!: any;
    public filterconfig!: any;
    public invoiceparam!: any;
    public invoicedetailparam!: any;
    public invoiceprint!: any;
    public barcodeprint!: any;
    public lasttemplateid!: any;
    public mustselectcategory!: any;
    public mastertableid!: any;
    public format!: any;
    public refconfig!: any;
    public autoreport!: any;
    public showtotal!: any;
    public readonly!: any;
    public clientcode!: any;
    public servercode!: any;
    public syncoption!: any;
    public tscode!: any;
    public tslayout!: any;
    public tsdesigncode!: any;
}

STABLEDESC.init({
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
    DEFAULTID: {
        type: DataTypes.INTEGER,
        
    },
    SORTBY: {
        type: DataTypes.STRING,
        
    },
    DESCRIPTION: {
        type: DataTypes.STRING,
        
    },
    TABLETYPE: {
        type: DataTypes.STRING,
        
    },
    COTTANG: {
        type: DataTypes.STRING,
        
    },
    COTGIAM: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SFUNCTIONID: {
        type: DataTypes.INTEGER,
        
    },
    AELAYOUT: {
        type: DataTypes.STRING,
        
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
    ISSYSTEM: {
        type: DataTypes.BOOLEAN,
        
    },
    USEDEFAULTAEFORM: {
        type: DataTypes.STRING,
        
    },
    GENNO: {
        type: DataTypes.STRING,
        
    },
    NOTEMPLATE: {
        type: DataTypes.STRING,
        
    },
    IMAGE32: {
        type: DataTypes.BLOB,
        
    },
    IMAGE: {
        type: DataTypes.BLOB,
        
    },
    CODE: {
        type: DataTypes.STRING,
        
    },
    DESIGNCODE: {
        type: DataTypes.STRING,
        
    },
    CATEGORYCOL: {
        type: DataTypes.STRING,
        
    },
    FILTERCONFIG: {
        type: DataTypes.STRING,
        
    },
    INVOICEPARAM: {
        type: DataTypes.STRING,
        
    },
    INVOICEDETAILPARAM: {
        type: DataTypes.STRING,
        
    },
    INVOICEPRINT: {
        type: DataTypes.STRING,
        
    },
    BARCODEPRINT: {
        type: DataTypes.STRING,
        
    },
    LASTTEMPLATEID: {
        type: DataTypes.INTEGER,
        
    },
    MUSTSELECTCATEGORY: {
        type: DataTypes.STRING,
        
    },
    MASTERTABLEID: {
        type: DataTypes.INTEGER,
        
    },
    FORMAT: {
        type: DataTypes.STRING,
        
    },
    REFCONFIG: {
        type: DataTypes.STRING,
        
    },
    AUTOREPORT: {
        type: DataTypes.STRING,
        
    },
    SHOWTOTAL: {
        type: DataTypes.STRING,
        
    },
    READONLY: {
        type: DataTypes.STRING,
        
    },
    CLIENTCODE: {
        type: DataTypes.STRING,
        
    },
    SERVERCODE: {
        type: DataTypes.STRING,
        
    },
    SYNCOPTION: {
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
    modelName: 'STABLEDESC',
    tableName: 'STABLEDESC',
    timestamps: false
});

export default STABLEDESC;
