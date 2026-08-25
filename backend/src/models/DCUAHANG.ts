import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DCUAHANG extends Model {
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
    public diachi!: any;
    public dienthoai!: any;
    public dkhohangid!: any;
    public stemplateid!: any;
    public code!: any;
}

DCUAHANG.init({
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
    DIACHI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DIENTHOAI: {
        type: DataTypes.STRING,
        
    },
    DKHOHANGID: {
        type: DataTypes.INTEGER,
        
    },
    STEMPLATEID: {
        type: DataTypes.INTEGER,
        
    },
    CODE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DCUAHANG',
    tableName: 'DCUAHANG',
    timestamps: false
});

export default DCUAHANG;
