import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TNGUOIDUNGTHEOCUAHANG extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public suserid!: any;
    public dcuahangid!: any;
}

TNGUOIDUNGTHEOCUAHANG.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
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
    SUSERID: {
        type: DataTypes.INTEGER,
        
    },
    DCUAHANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TNGUOIDUNGTHEOCUAHANG',
    tableName: 'TNGUOIDUNGTHEOCUAHANG',
    timestamps: false
});

export default TNGUOIDUNGTHEOCUAHANG;
