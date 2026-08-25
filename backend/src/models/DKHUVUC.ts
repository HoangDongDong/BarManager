import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DKHUVUC extends Model {
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
    public stemplateid!: any;
    public dkhohangid!: any;
    public tabhienthi!: any;
    public dgiamathangid!: any;
    public dbieutuongid!: any;
    public tienmoban!: any;
    public dgiatheogioid!: any;
    public tugio!: any;
    public dengio!: any;
    public cachtinhgio!: any;
    public dongia!: any;
    public dbanggiaid!: any;
    public gridsize!: any;
    public width!: any;
    public height!: any;
    public backcolor!: any;
    public backgroundimage!: any;
    public mausac!: any;
    public anh!: any;
}

DKHUVUC.init({
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
    STEMPLATEID: {
        type: DataTypes.INTEGER,
        
    },
    DKHOHANGID: {
        type: DataTypes.INTEGER,
        
    },
    TABHIENTHI: {
        type: DataTypes.STRING,
        
    },
    DGIAMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DBIEUTUONGID: {
        type: DataTypes.INTEGER,
        
    },
    TIENMOBAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DGIATHEOGIOID: {
        type: DataTypes.INTEGER,
        
    },
    TUGIO: {
        type: DataTypes.DATE,
        
    },
    DENGIO: {
        type: DataTypes.DATE,
        
    },
    CACHTINHGIO: {
        type: DataTypes.DATE,
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
    GRIDSIZE: {
        type: DataTypes.STRING,
        
    },
    WIDTH: {
        type: DataTypes.STRING,
        
    },
    HEIGHT: {
        type: DataTypes.STRING,
        
    },
    BACKCOLOR: {
        type: DataTypes.STRING,
        
    },
    BACKGROUNDIMAGE: {
        type: DataTypes.BLOB,
        
    },
    MAUSAC: {
        type: DataTypes.STRING,
        
    },
    ANH: {
        type: DataTypes.BLOB,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DKHUVUC',
    tableName: 'DKHUVUC',
    timestamps: false
});

export default DKHUVUC;
