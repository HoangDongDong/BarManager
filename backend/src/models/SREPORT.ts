import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SREPORT extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public sortorder!: any;
    public usercreatedid!: any;
    public parentid!: any;
    public parentdir!: any;
    public itemtype!: any;
    public autoid!: any;
    public simageid!: any;
    public image32!: any;
    public filterconfig!: any;
    public verticallayout!: any;
    public runcount!: any;
    public masterview!: any;
    public detailview!: any;
    public hasdetail!: any;
    public filterontop!: any;
    public hasdetail2!: any;
    public detail2view!: any;
    public viewinreportmg!: any;
    public sql!: any;
    public params!: any;
    public lasttemplateid!: any;
    public sqlmode!: any;
    public reminder!: any;
    public format!: any;
    public thuongdung!: any;
    public code!: any;
    public colconfig!: any;
    public rawmode!: any;
    public classname!: any;
    public servercode!: any;
    public clientcode!: any;
}

SREPORT.init({
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
    IMAGE32: {
        type: DataTypes.BLOB,
        
    },
    FILTERCONFIG: {
        type: DataTypes.STRING,
        
    },
    VERTICALLAYOUT: {
        type: DataTypes.STRING,
        
    },
    RUNCOUNT: {
        type: DataTypes.STRING,
        
    },
    MASTERVIEW: {
        type: DataTypes.STRING,
        
    },
    DETAILVIEW: {
        type: DataTypes.STRING,
        
    },
    HASDETAIL: {
        type: DataTypes.BOOLEAN,
        
    },
    FILTERONTOP: {
        type: DataTypes.STRING,
        
    },
    HASDETAIL2: {
        type: DataTypes.BOOLEAN,
        
    },
    DETAIL2VIEW: {
        type: DataTypes.STRING,
        
    },
    VIEWINREPORTMG: {
        type: DataTypes.STRING,
        
    },
    SQL: {
        type: DataTypes.STRING,
        
    },
    PARAMS: {
        type: DataTypes.STRING,
        
    },
    LASTTEMPLATEID: {
        type: DataTypes.INTEGER,
        
    },
    SQLMODE: {
        type: DataTypes.STRING,
        
    },
    REMINDER: {
        type: DataTypes.STRING,
        
    },
    FORMAT: {
        type: DataTypes.STRING,
        
    },
    THUONGDUNG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    CODE: {
        type: DataTypes.STRING,
        
    },
    COLCONFIG: {
        type: DataTypes.STRING,
        
    },
    RAWMODE: {
        type: DataTypes.STRING,
        
    },
    CLASSNAME: {
        type: DataTypes.STRING,
        
    },
    SERVERCODE: {
        type: DataTypes.STRING,
        
    },
    CLIENTCODE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SREPORT',
    tableName: 'SREPORT',
    timestamps: false
});

export default SREPORT;
