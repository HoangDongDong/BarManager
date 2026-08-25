import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SUSER extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public password!: any;
    public username!: any;
    public email!: any;
    public isadmin!: any;
    public sgroupuserid!: any;
    public smtp!: any;
    public ssl!: any;
    public port!: any;
    public pass!: any;
    public vietinput!: any;
    public inputmode!: any;
    public simageid!: any;
    public autoid!: any;
    public parentid!: any;
    public parentdir!: any;
    public sortorder!: any;
    public itemtype!: any;
    public dnhanvienid!: any;
    public defaultfuncid!: any;
    public userid!: any;
    public cardcode!: any;
}

SUSER.init({
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
    PASSWORD: {
        type: DataTypes.STRING,
        
    },
    USERNAME: {
        type: DataTypes.STRING,
        
    },
    EMAIL: {
        type: DataTypes.STRING,
        
    },
    ISADMIN: {
        type: DataTypes.BOOLEAN,
        
    },
    SGROUPUSERID: {
        type: DataTypes.INTEGER,
        
    },
    SMTP: {
        type: DataTypes.STRING,
        
    },
    SSL: {
        type: DataTypes.STRING,
        
    },
    PORT: {
        type: DataTypes.INTEGER,
        
    },
    PASS: {
        type: DataTypes.STRING,
        
    },
    VIETINPUT: {
        type: DataTypes.STRING,
        
    },
    INPUTMODE: {
        type: DataTypes.STRING,
        
    },
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
    AUTOID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTDIR: {
        type: DataTypes.STRING,
        
    },
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    DEFAULTFUNCID: {
        type: DataTypes.INTEGER,
        
    },
    USERID: {
        type: DataTypes.INTEGER,
        
    },
    CARDCODE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SUSER',
    tableName: 'SUSER',
    timestamps: false
});

export default SUSER;
