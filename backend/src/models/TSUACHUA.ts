import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TSUACHUA extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public dbanid!: any;
    public dasuaxong!: any;
    public noidung!: any;
    public dloaiphongid!: any;
    public dnhanvienid!: any;
    public consudungduoc!: any;
}

TSUACHUA.init({
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
    NGAY: {
        type: DataTypes.DATE,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DBANID: {
        type: DataTypes.INTEGER,
        
    },
    DASUAXONG: {
        type: DataTypes.STRING,
        
    },
    NOIDUNG: {
        type: DataTypes.STRING,
        
    },
    DLOAIPHONGID: {
        type: DataTypes.INTEGER,
        
    },
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    CONSUDUNGDUOC: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TSUACHUA',
    tableName: 'TSUACHUA',
    timestamps: false
});

export default TSUACHUA;
