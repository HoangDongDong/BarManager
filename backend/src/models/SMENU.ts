import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SMENU extends Model {
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
    public operation!: any;
    public stabledescid!: any;
    public sformid!: any;
    public bold!: any;
    public italic!: any;
    public image!: any;
    public toolbar!: any;
    public toolbarindex!: any;
    public shortcut!: any;
    public toolbartitlte!: any;
    public visibleconfigid!: any;
    public sreportid!: any;
    public viewcount!: any;
    public sfunctionid!: any;
    public loai!: any;
    public loctheoloai!: any;
    public hideinmenu!: any;
    public menucode!: any;
    public viewonts!: any;
    public viewonmobi!: any;
    public viewonweb!: any;
}

SMENU.init({
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
    OPERATION: {
        type: DataTypes.STRING,
        
    },
    STABLEDESCID: {
        type: DataTypes.INTEGER,
        
    },
    SFORMID: {
        type: DataTypes.INTEGER,
        
    },
    BOLD: {
        type: DataTypes.BOOLEAN,
        
    },
    ITALIC: {
        type: DataTypes.BOOLEAN,
        
    },
    IMAGE: {
        type: DataTypes.BLOB,
        
    },
    TOOLBAR: {
        type: DataTypes.STRING,
        
    },
    TOOLBARINDEX: {
        type: DataTypes.STRING,
        
    },
    SHORTCUT: {
        type: DataTypes.STRING,
        
    },
    TOOLBARTITLTE: {
        type: DataTypes.STRING,
        
    },
    VISIBLECONFIGID: {
        type: DataTypes.INTEGER,
        
    },
    SREPORTID: {
        type: DataTypes.INTEGER,
        
    },
    VIEWCOUNT: {
        type: DataTypes.INTEGER,
        
    },
    SFUNCTIONID: {
        type: DataTypes.INTEGER,
        
    },
    LOAI: {
        type: DataTypes.STRING,
        
    },
    LOCTHEOLOAI: {
        type: DataTypes.STRING,
        
    },
    HIDEINMENU: {
        type: DataTypes.STRING,
        
    },
    MENUCODE: {
        type: DataTypes.STRING,
        
    },
    VIEWONTS: {
        type: DataTypes.STRING,
        
    },
    VIEWONMOBI: {
        type: DataTypes.STRING,
        
    },
    VIEWONWEB: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SMENU',
    tableName: 'SMENU',
    timestamps: false
});

export default SMENU;
