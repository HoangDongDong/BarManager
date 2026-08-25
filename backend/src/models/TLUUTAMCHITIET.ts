import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TLUUTAMCHITIET extends Model {
    public id!: any;
    public note!: any;
    public tluutamid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public soluong!: any;
    public dongia!: any;
    public thanhtien!: any;
    public tilegiamgia!: any;
    public baohanh!: any;
    public dmathangid!: any;
    public soluongchuaquydoi!: any;
    public ddonvitinhid!: any;
}

TLUUTAMCHITIET.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NOTE: {
        type: DataTypes.STRING,
        
    },
    TLUUTAMID: {
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
    SOLUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    THANHTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    BAOHANH: {
        type: DataTypes.STRING,
        
    },
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    SOLUONGCHUAQUYDOI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DDONVITINHID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TLUUTAMCHITIET',
    tableName: 'TLUUTAMCHITIET',
    timestamps: false
});

export default TLUUTAMCHITIET;
