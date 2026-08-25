import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TINCHEBIEN extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public tdonhangid!: any;
    public usercreatedid!: any;
    public lanso!: any;
    public tenhang!: any;
    public dloaidoid!: any;
    public ddonvitinhid!: any;
    public soluong!: any;
}

TINCHEBIEN.init({
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
    LANSO: {
        type: DataTypes.STRING,
        
    },
    TENHANG: {
        type: DataTypes.STRING,
        
    },
    DLOAIDOID: {
        type: DataTypes.INTEGER,
        
    },
    DDONVITINHID: {
        type: DataTypes.INTEGER,
        
    },
    SOLUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TINCHEBIEN',
    tableName: 'TINCHEBIEN',
    timestamps: false
});

export default TINCHEBIEN;
