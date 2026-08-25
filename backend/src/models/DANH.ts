import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DANH extends Model {
    public note!: any;
    public name!: any;
    public italic!: any;
    public controllevel!: any;
    public bold!: any;
    public fontsize!: any;
    public visible!: any;
    public fontcolor!: any;
    public controlname!: any;
    public underline!: any;
    public icon!: any;
    public transparent!: any;
    public textalign!: any;
    public hasborder!: any;
    public parentcontrolname!: any;
    public width!: any;
    public zindex!: any;
    public flat!: any;
    public ref1id!: any;
    public ref2id!: any;
    public fontname!: any;
    public imagezoom!: any;
    public borderwidth!: any;
    public backcolor!: any;
    public height!: any;
    public iconalign!: any;
    public posleft!: any;
    public customdata1!: any;
    public customdata2!: any;
    public customdata3!: any;
    public controltype!: any;
    public postop!: any;
    public anh!: any;
    public buttonshape!: any;
    public rotate!: any;
    public numchairs!: any;
    public dkhuvucid!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
}

DANH.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
    },
    ITALIC: {
        type: DataTypes.BOOLEAN,
        
    },
    CONTROLLEVEL: {
        type: DataTypes.STRING,
        
    },
    BOLD: {
        type: DataTypes.BOOLEAN,
        
    },
    FONTSIZE: {
        type: DataTypes.STRING,
        
    },
    VISIBLE: {
        type: DataTypes.BOOLEAN,
        
    },
    FONTCOLOR: {
        type: DataTypes.STRING,
        
    },
    CONTROLNAME: {
        type: DataTypes.STRING,
        
    },
    UNDERLINE: {
        type: DataTypes.BOOLEAN,
        
    },
    ICON: {
        type: DataTypes.BLOB,
        
    },
    TRANSPARENT: {
        type: DataTypes.BOOLEAN,
        
    },
    TEXTALIGN: {
        type: DataTypes.STRING,
        
    },
    HASBORDER: {
        type: DataTypes.BOOLEAN,
        
    },
    PARENTCONTROLNAME: {
        type: DataTypes.STRING,
        
    },
    WIDTH: {
        type: DataTypes.STRING,
        
    },
    ZINDEX: {
        type: DataTypes.STRING,
        
    },
    FLAT: {
        type: DataTypes.BOOLEAN,
        
    },
    REF1ID: {
        type: DataTypes.INTEGER,
        
    },
    REF2ID: {
        type: DataTypes.INTEGER,
        
    },
    FONTNAME: {
        type: DataTypes.STRING,
        
    },
    IMAGEZOOM: {
        type: DataTypes.STRING,
        
    },
    BORDERWIDTH: {
        type: DataTypes.STRING,
        
    },
    BACKCOLOR: {
        type: DataTypes.STRING,
        
    },
    HEIGHT: {
        type: DataTypes.STRING,
        
    },
    ICONALIGN: {
        type: DataTypes.STRING,
        
    },
    POSLEFT: {
        type: DataTypes.STRING,
        
    },
    CUSTOMDATA1: {
        type: DataTypes.STRING,
        
    },
    CUSTOMDATA2: {
        type: DataTypes.STRING,
        
    },
    CUSTOMDATA3: {
        type: DataTypes.STRING,
        
    },
    CONTROLTYPE: {
        type: DataTypes.STRING,
        
    },
    POSTOP: {
        type: DataTypes.STRING,
        
    },
    ANH: {
        type: DataTypes.BLOB,
        
    },
    BUTTONSHAPE: {
        type: DataTypes.STRING,
        
    },
    ROTATE: {
        type: DataTypes.STRING,
        
    },
    NUMCHAIRS: {
        type: DataTypes.STRING,
        
    },
    DKHUVUCID: {
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
    modelName: 'DANH',
    tableName: 'DANH',
    timestamps: false
});

export default DANH;
