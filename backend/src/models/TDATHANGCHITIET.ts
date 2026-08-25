import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDATHANGCHITIET extends Model {
    public note!: any;
    public soluong!: any;
    public dongia!: any;
    public thanhtien!: any;
    public tilegiamgia!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public baohanh!: any;
    public ddonvitinhid!: any;
    public tdathangid!: any;
    public dmathangid!: any;
}

TDATHANGCHITIET.init({
    NOTE: {
        type: DataTypes.STRING,
        
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
    BAOHANH: {
        type: DataTypes.STRING,
        
    },
    DDONVITINHID: {
        type: DataTypes.INTEGER,
        
    },
    TDATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDATHANGCHITIET',
    tableName: 'TDATHANGCHITIET',
    timestamps: false
});

export default TDATHANGCHITIET;
