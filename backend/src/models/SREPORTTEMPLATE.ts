import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SREPORTTEMPLATE extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public template!: any;
    public stemplateid!: any;
    public sreportid!: any;
    public config!: any;
    public sortorder!: any;
    public parentid!: any;
    public parentdir!: any;
    public itemtype!: any;
    public autoid!: any;
    public simageid!: any;
    public autogenreport!: any;
    public colautowidth!: any;
    public stemplatelandscapeid!: any;
}

SREPORTTEMPLATE.init({
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
    TEMPLATE: {
        type: DataTypes.STRING,
        
    },
    STEMPLATEID: {
        type: DataTypes.INTEGER,
        
    },
    SREPORTID: {
        type: DataTypes.INTEGER,
        
    },
    CONFIG: {
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
    AUTOGENREPORT: {
        type: DataTypes.STRING,
        
    },
    COLAUTOWIDTH: {
        type: DataTypes.STRING,
        
    },
    STEMPLATELANDSCAPEID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SREPORTTEMPLATE',
    tableName: 'SREPORTTEMPLATE',
    timestamps: false
});

export default SREPORTTEMPLATE;
