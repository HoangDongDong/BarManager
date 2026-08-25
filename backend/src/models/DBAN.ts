import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DBAN extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public dkhuvucid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dbanggiaid!: any;
    public dongia!: any;
    public cachtinhgio!: any;
    public dnhomhienthiid!: any;
    public tienmoban!: any;
    public dloaiphongid!: any;
    public tdonhangid!: any;
    public trangthai!: any;
    public taikhoangiu!: any;
    public giuluc!: any;
    public ipcongtac!: any;
    public italic!: any;
    public controllevel!: any;
    public fontsize!: any;
    public bold!: any;
    public controlname!: any;
    public fontcolor!: any;
    public visible!: any;
    public underline!: any;
    public transparent!: any;
    public icon!: any;
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
    public postop!: any;
    public controltype!: any;
    public anh!: any;
    public buttonshape!: any;
    public rotate!: any;
    public numchairs!: any;
}

DBAN.init({
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
    DKHUVUCID: {
        type: DataTypes.INTEGER,
        
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
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    CACHTINHGIO: {
        type: DataTypes.DATE,
        
    },
    DNHOMHIENTHIID: {
        type: DataTypes.INTEGER,
        
    },
    TIENMOBAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DLOAIPHONGID: {
        type: DataTypes.INTEGER,
        
    },
    TDONHANGID: {
        type: DataTypes.INTEGER,
        
    },
    TRANGTHAI: {
        type: DataTypes.STRING,
        
    },
    TAIKHOANGIU: {
        type: DataTypes.STRING,
        
    },
    GIULUC: {
        type: DataTypes.STRING,
        
    },
    IPCONGTAC: {
        type: DataTypes.STRING,
        
    },
    ITALIC: {
        type: DataTypes.BOOLEAN,
        
    },
    CONTROLLEVEL: {
        type: DataTypes.STRING,
        
    },
    FONTSIZE: {
        type: DataTypes.STRING,
        
    },
    BOLD: {
        type: DataTypes.BOOLEAN,
        
    },
    CONTROLNAME: {
        type: DataTypes.STRING,
        
    },
    FONTCOLOR: {
        type: DataTypes.STRING,
        
    },
    VISIBLE: {
        type: DataTypes.BOOLEAN,
        
    },
    UNDERLINE: {
        type: DataTypes.BOOLEAN,
        
    },
    TRANSPARENT: {
        type: DataTypes.BOOLEAN,
        
    },
    ICON: {
        type: DataTypes.BLOB,
        
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
    POSTOP: {
        type: DataTypes.STRING,
        
    },
    CONTROLTYPE: {
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
}, {
    sequelize: db.sequelize,
    modelName: 'DBAN',
    tableName: 'DBAN',
    timestamps: false
});

export default DBAN;
