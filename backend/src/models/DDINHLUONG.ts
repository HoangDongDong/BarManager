import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DDINHLUONG extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dmathangid!: any;
    public soluong!: any;
    public dvattuid!: any;
}

DDINHLUONG.init({
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
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    SOLUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DVATTUID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DDINHLUONG',
    tableName: 'DDINHLUONG',
    timestamps: false
});

export default DDINHLUONG;
