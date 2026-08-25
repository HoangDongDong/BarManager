import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDONHANGGIO extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public tdonhangid!: any;
    public usercreatedid!: any;
    public tugio!: any;
    public dengio!: any;
    public dongia!: any;
    public dbanggiaid!: any;
    public dbanid!: any;
    public thanhtien!: any;
    public cachtinhgia!: any;
}

TDONHANGGIO.init({
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
    TDONHANGID: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    TUGIO: {
        type: DataTypes.DATE,
        
    },
    DENGIO: {
        type: DataTypes.DATE,
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
    DBANID: {
        type: DataTypes.INTEGER,
        
    },
    THANHTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    CACHTINHGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDONHANGGIO',
    tableName: 'TDONHANGGIO',
    timestamps: false
});

export default TDONHANGGIO;
