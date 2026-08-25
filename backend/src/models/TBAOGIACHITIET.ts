import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TBAOGIACHITIET extends Model {
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
    public ddonvitinhid!: any;
    public tbaogiaid!: any;
    public dmathangid!: any;
}

TBAOGIACHITIET.init({
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
    DDONVITINHID: {
        type: DataTypes.INTEGER,
        
    },
    TBAOGIAID: {
        type: DataTypes.INTEGER,
        
    },
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TBAOGIACHITIET',
    tableName: 'TBAOGIACHITIET',
    timestamps: false
});

export default TBAOGIACHITIET;
