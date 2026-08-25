import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DNHOMMATHANG extends Model {
    public note!: any;
    public name!: any;
    public id!: any;
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
    public dloaidoid!: any;
    public mausac!: any;
    public anh!: any;
}

DNHOMMATHANG.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
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
    DLOAIDOID: {
        type: DataTypes.INTEGER,
        
    },
    MAUSAC: {
        type: DataTypes.STRING,
        
    },
    ANH: {
        type: DataTypes.BLOB,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DNHOMMATHANG',
    tableName: 'DNHOMMATHANG',
    timestamps: false
});

export default DNHOMMATHANG;
