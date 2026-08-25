import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TCHITIETTHANHTOAN extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public tdonhangid!: any;
    public tthuchiid!: any;
    public sotien!: any;
}

TCHITIETTHANHTOAN.init({
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
    TDONHANGID: {
        type: DataTypes.INTEGER,
        
    },
    TTHUCHIID: {
        type: DataTypes.INTEGER,
        
    },
    SOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TCHITIETTHANHTOAN',
    tableName: 'TCHITIETTHANHTOAN',
    timestamps: false
});

export default TCHITIETTHANHTOAN;
