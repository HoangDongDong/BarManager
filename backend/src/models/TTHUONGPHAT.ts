import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TTHUONGPHAT extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public dnhanvienid!: any;
    public thuong!: any;
    public phat!: any;
    public dlydothuongphatid!: any;
}

TTHUONGPHAT.init({
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
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    THUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    PHAT: {
        type: DataTypes.STRING,
        
    },
    DLYDOTHUONGPHATID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TTHUONGPHAT',
    tableName: 'TTHUONGPHAT',
    timestamps: false
});

export default TTHUONGPHAT;
