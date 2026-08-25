import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DNHANVIEN extends Model {
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
    public cachtinhluong!: any;
    public nghithu7!: any;
    public nghichunhat!: any;
    public luongca!: any;
    public luongthang!: any;
    public diachi!: any;
    public dienthoai!: any;
    public luongtheoca!: any;
    public dcalamviecid!: any;
    public code!: any;
}

DNHANVIEN.init({
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
    CACHTINHLUONG: {
        type: DataTypes.STRING,
        
    },
    NGHITHU7: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    NGHICHUNHAT: {
        type: DataTypes.STRING,
        
    },
    LUONGCA: {
        type: DataTypes.STRING,
        
    },
    LUONGTHANG: {
        type: DataTypes.STRING,
        
    },
    DIACHI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DIENTHOAI: {
        type: DataTypes.STRING,
        
    },
    LUONGTHEOCA: {
        type: DataTypes.STRING,
        
    },
    DCALAMVIECID: {
        type: DataTypes.INTEGER,
        
    },
    CODE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DNHANVIEN',
    tableName: 'DNHANVIEN',
    timestamps: false
});

export default DNHANVIEN;
