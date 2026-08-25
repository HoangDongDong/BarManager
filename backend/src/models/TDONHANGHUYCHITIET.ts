import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDONHANGHUYCHITIET extends Model {
    public id!: any;
    public note!: any;
    public tdonhanghuyid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public mahang!: any;
    public tenhang!: any;
    public dvt!: any;
    public dongia!: any;
    public thanhtien!: any;
    public soluong!: any;
}

TDONHANGHUYCHITIET.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NOTE: {
        type: DataTypes.STRING,
        
    },
    TDONHANGHUYID: {
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
    MAHANG: {
        type: DataTypes.STRING,
        
    },
    TENHANG: {
        type: DataTypes.STRING,
        
    },
    DVT: {
        type: DataTypes.STRING,
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    THANHTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SOLUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDONHANGHUYCHITIET',
    tableName: 'TDONHANGHUYCHITIET',
    timestamps: false
});

export default TDONHANGHUYCHITIET;
